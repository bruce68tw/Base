using Base.Enums;
using Base.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

//excel import
namespace Base.Services
{
    /// <summary>
    /// 匯入excel功能, 欄位名稱開頭為D:表示excel儲存格為日期欄位(實際存數字)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExcelImportSvc<T> where T : class, new()
    {
        //constant
        const string RowSep = "\r\n";  //row seperator for import error
        
        //ok excel row no
        private List<int> _okRowNos = [];

        //failed excel row no/msg
        private List<SnStrDto> _failRows = [];

        private List<ExcelImportFieldDto> _excelFields = null!;

        /// <summary>
        /// import by stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="importDto"></param>
        /// <param name="fileName"></param>
        /// <param name="uiDtFormat"></param>
        /// <param name="writeLog">是否寫入XpImportLog table, 如果要自行控制寫入log, 則設為false</param>
        /// <returns></returns>
        public async Task<ResultImportDto> ImportByStreamA(Stream stream, ExcelImportDto<T> importDto, 
            string dirUpload, string fileName, string uiDtFormat, bool writeLog)
        {
            stream.Position = 0;
            var docx = _Excel.StreamToDocx(stream);
            var result = await ImportByDocxA(docx, importDto, dirUpload, fileName, uiDtFormat, writeLog);

            //release docx
            //docx = null;
            docx.Dispose();
            return result;
        }

        /// <summary>
        /// import by excel docx
        /// excel row/cell (base 0)
        /// </summary>
        /// <param name="docx"></param>
        /// <param name="importDto"></param>
        /// <param name="fileName">imported excel file name</param>
        /// <param name="uiDtFormat">excel裡面的datetime日期格式</param>
        /// <param name="writeLog">是否寫入XpImportLog table, 如果要自行控制寫入log, 則設為false</param>
        /// <returns>error msg if any</returns>
        public async Task<ResultImportDto> ImportByDocxA(SpreadsheetDocument docx, 
            ExcelImportDto<T> importDto, string dirUpload, string fileName, string uiDtFormat, bool writeLog)
        {
            #region 1.set variables
            #region set docx, excelRows, ssTable
            //var errorMsg = "";
            var wbPart = docx.WorkbookPart;
            var wsPart = (WorksheetPart)wbPart!.GetPartById(
                wbPart.Workbook.Descendants<Sheet>().ElementAt(importDto.SheetNo).Id!);

            //加上where後只傳回非空白列
            var excelRows = wsPart.Worksheet.Descendants<Row>()    //include empty rows
                .Where(r => r.Elements<Cell>().Any(c => !string.IsNullOrWhiteSpace(c.InnerText)));

            var ssTable = wbPart.GetPartsOfType<SharedStringTablePart>().First().SharedStringTable;
            #endregion

            #region set importDto.ExcelFids, excelFidLen
            int no;
            var colNameNos = new Dictionary<string, int>();     //excel位置字母,位置對應, ex:(A,0)
            var tplCells = excelRows.ElementAt(importDto.FidRowNo - 1).Elements<Cell>();
            _excelFields = new List<ExcelImportFieldDto>();
            if (importDto.ExcelFids == null || importDto.ExcelFids.Count == 0)
            {
                //如果沒有傳入excel欄位名稱, 則使用第一行excel做為欄位名稱(不可有合併儲存格!!)
                //set _excelFields
                no = 0;
                foreach (var cell in tplCells)
                {
                    var isDate = false;
                    var fid = CellValue(ssTable, cell);
                    if (fid[..2] == "D:")
                    {
                        fid = fid[2..];
                        isDate = true;
                    }
                    _excelFields.Add(new ExcelImportFieldDto()
                    {
                        Fid = fid,
                        //Fno = no,
                        CellName = CellXname(cell.CellReference!),
                        IsDate = isDate,
                    });
                    no++;
                }
            }
            else
            {
                //有傳入excel欄位名稱(最多到Z)
                var cellLen = tplCells.Count();
                if (cellLen != importDto.ExcelFids.Count)
                {
                    return new ResultImportDto()
                    {
                        _ErrorMsg = "importDto.ExcelFids length should be " + cellLen,
                    };
                }

                //set _excelFields
                //no = 0;
                for (var ci = 0; ci < importDto.ExcelFids.Count; ci++)
                {
                	//空白表示有合併儲存格
                    if (importDto.ExcelFids[ci] == "") continue;

                    var isDate = false;
                    var fid = importDto.ExcelFids[ci];
                    if (fid[..2] == "D:")
                    {
                        fid = fid[2..];
                        isDate = true;
                    }
                    _excelFields.Add(new ExcelImportFieldDto()
                    {
                        Fid = fid,
                        //Fno = ci,
                        CellName = NoToCellName(ci),
                        IsDate = isDate,
                    });
                }
            }

            //initial excelIsDates & set excelFidLen
            var excelFidLen = _excelFields.Count;
            #endregion

            #region set modelFidTypes
            //int fno;
            var modelFidTypes = new Dictionary<string, string>();
            var model = new T();
            foreach (var prop in model.GetType().GetProperties())
            {
                //如果對應的excel欄位不存在, 則不記錄此欄位(skip)
                var fid = prop.Name;
                var field = _excelFields.FirstOrDefault(a => a.Fid == fid);
                if (field == null) continue;

                // 判斷是否為可空類型，提取基礎類型
                var type = prop.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
                var typeName = underlyingType.Name;

                var ftype = typeName.Contains("DateTime") ? ModelTypeEstr.Datetime :
                    typeName.Contains("Int") ? ModelTypeEstr.Int :
                    ModelTypeEstr.String;

                modelFidTypes.Add(fid, ftype);
            }
            #endregion

            #region set fileRows by excel file
            var fileRows = new List<T>();   //excel rows with data(not empty row)
            var excelRowsLen = excelRows.LongCount();
            for (var ri = importDto.FidRowNo; ri < excelRowsLen; ri++)
            {
                var excelRow = excelRows.ElementAt(ri);
                var fileRow = new T();
                var rowIndex = excelRow.RowIndex;   //RowIndex is base 1
                //no = 0;
                //foreach (var col in colNameNos)
                foreach (var field in _excelFields)
                {
                    var fid = field.Fid;
                    var ftype = modelFidTypes[fid];

                    //cell如果有空白(有合併格時)會造成讀取錯誤, 使用查詢方式
                    var cell = excelRow.Elements<Cell>()
                        .FirstOrDefault(c => c.CellReference == field.CellName + rowIndex);

                    var isNull = (cell == null || cell.CellValue == null);
                    var value = isNull ? "" : cell!.CellValue!.Text;   //字串時儲存address !!

                    //有時數值欄位會被判斷為字串, 所有先判斷字串以外型態
                    if (!isNull && cell!.DataType != null && cell.DataType! == CellValues.SharedString)
                        value = ssTable.ChildElements[int.Parse(value)].InnerText;

                    //日期儲存格
                    if (field.IsDate && _Str.NotEmpty(value))
                        value = _Excel.DateIntToStr(value, uiDtFormat);

                    object value2 = 
                        (ftype == ModelTypeEstr.Datetime) ? _Date.CsToDate(value)! :
                        (ftype == ModelTypeEstr.Int) ? Convert.ToInt32(string.IsNullOrEmpty(value) ? "0" : value) :
                        value!;

                    _Model.SetValue(fileRow, fid, value2);
                }

                fileRows.Add(fileRow);
            }
            #endregion
            #endregion

            #region 2.validate fileRows loop
            no = 0;
            foreach (var fileRow in fileRows)
            {
                //validate
                var context = new ValidationContext(fileRow, null, null);
                var results = new List<ValidationResult>();
                if (Validator.TryValidateObject(fileRow, context, results, true))
                    _okRowNos.Add(no);
                else
                    AddErrorByResults(no, results);
                no++;
            }
            #endregion

            #region 3.save database for ok rows(call FnSaveImportRows())
            if (_okRowNos.Count > 0)
            {
                //set okRows
                var okRows = new List<T>();
                foreach(var okRowNo in _okRowNos)
                    okRows.Add(fileRows[okRowNo]);

                //call FnSaveImportRows
                no = 0;
                var saveResults = importDto.FnSaveImportRows!(okRows);
                if (saveResults != null)
                {
                    foreach (var result in saveResults)
                    {
                        if (_Str.NotEmpty(result))
                            AddError(_okRowNos[no], result);
                        no++;
                    }
                }
            }
            #endregion

            #region 4.save ok excel file
            if (_Str.IsEmpty(importDto.LogRowId))
                importDto.LogRowId = _Str.NewId();
            var fileStem = _Str.AddDirSep(dirUpload) + importDto.LogRowId;

            // 新版 SpreadsheetDocument 移除 SaveAs 代碼，改為使用 Clone 方法
            using (var newDocx = docx.Clone(fileStem + ".xlsx"))
            {
                newDocx.Dispose(); // 儲存並關閉新文件
            }
            #endregion

            #region 5.save fail excel file (tail _fail.xlsx)
            var failCount = _failRows.Count;
            if (failCount > 0)
            {
                //get docx
                var failFilePath = fileStem + "_fail.xlsx";
                File.Copy(importDto.TplPath, failFilePath, true);

                var docx2 = SpreadsheetDocument.Open(failFilePath, true);
                var wbPart2 = docx2.WorkbookPart!;
                var wsPart2 = (WorksheetPart)wbPart2.GetPartById(
                    wbPart2.Workbook.Descendants<Sheet>().ElementAt(0).Id!);
                var sheetData2 = wsPart2.Worksheet.GetFirstChild<SheetData>();

                //新增空白列(含格式, 合併儲存格)
                var startRow = importDto.FidRowNo;    //insert position, base 0
                if (failCount > 1)
                    CopyTplRows(sheetData2!, startRow, failCount - 1);

                for (var ri = 0; ri < failCount; ri++)
                {
                    //add row, fill value & copy row style
                    var row = sheetData2!.Elements<Row>().ElementAt(startRow + ri);  //base 0, row position to write
                    var cells = row.Elements<Cell>()!;
                    var modelRow = fileRows[_failRows[ri].Sn];
                    //int fno;
                    foreach (var field in _excelFields)
                    {
                        var value2 = _Model.GetValue(modelRow, field.Fid);
                        //因為有合併格, 必須用字母來找cell
                        //var cell = row.Elements<Cell>().ElementAt(fno);
                        var cell = cells.FirstOrDefault(c =>
                                c.CellReference!.Value!.StartsWith(field.CellName, StringComparison.OrdinalIgnoreCase))!;
                        cell.CellValue = new CellValue(value2?.ToString() ?? string.Empty);
                        cell.DataType = CellValues.String; // 設定為字串
                    }

                    //write cell for error msg
                    row.Append(new Cell()
                    {
                        CellValue = new CellValue(_failRows[ri].Str),
                        DataType = CellValues.String,
                    });
                }

                docx2.Save();
                docx2.Dispose();
            }
            #endregion

            #region 6.insert XpImportLog table if need
            var totalCount = fileRows.Count;
            var okCount = totalCount - failCount;
            if (writeLog)
            {
                var sql = $@"
insert into dbo.XpImportLog(Id, Type, FileName,
OkCount, FailCount, TotalCount,
CreatorName, Created)
values('{importDto.LogRowId}', '{importDto.ImportType}', '{fileName}',
{okCount}, {failCount}, {totalCount}, 
'{importDto.CreatorName}', '{_Date.NowDbStr()}')
";
                await _Db.ExecSqlA(sql);
            }
            #endregion

            //7.return import result
            return new ResultImportDto()
            {
                LogRowId = importDto.LogRowId,
                OkCount = okCount,
                FailCount = failCount,
                TotalCount = totalCount,
            };
        }

        /// <summary>
        /// cell no to cell name
        /// </summary>
        /// <param name="index">base 0</param>
        /// <returns></returns>
        private string NoToCellName(int index)
        {
            return ((char)('A' + index)).ToString();
        }

        /// <summary>
        /// get cell x-way name(no number)
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        private string CellXname(string colName)
        {
            return Regex.Replace(colName, @"[\d]", string.Empty);
        }

        /*
        private Cell GetOrCreateCell(Row row, string columnName, uint rowIndex)
        {
            string cellRef = columnName + rowIndex;

            // 嘗試根據 CellReference 找出現有的 Cell
            var cell = row.Elements<Cell>()
                .FirstOrDefault(c => c.CellReference?.Value == cellRef);

            if (cell == null)
            {
                // 如果找不到，就建立新的 Cell 並加入該列
                cell = new Cell()
                {
                    CellReference = cellRef,
                    DataType = CellValues.String
                };

                row.Append(cell); // 直接加到 Row 尾端。可依需求改為 Insert 排序
            }

            return cell;
        }
        */

        /// <summary>
        /// 從template row 複製多筆空白列
        /// </summary>
        /// <param name="sheetData"></param>
        /// <param name="fromRow">來源Row, base 0</param>
        /// <param name="newRows">需要新增加的列數, 扣除範本這一列</param>
        private void CopyTplRows(SheetData sheetData, int fromRow, int newRows)
        {
            var worksheet = (Worksheet)sheetData.Parent!;
            var baseRowIndex = (uint)fromRow + 1; // OpenXML 是從 1 開始的 RowIndex

            // 取得範本列, RowIndex is base 1 !!
            var tplRow = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex! == baseRowIndex);
            if (tplRow == null) return;

            // 取得或建立 MergeCells 並加入到 Worksheet
            var mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();
            if (mergeCells == null)
            {
                mergeCells = new MergeCells();
                worksheet.InsertAfter(mergeCells, sheetData); // 正確插入位置
            }

            // 取得範本列的合併儲存格
            var tplMerges = mergeCells.Elements<MergeCell>()
                .Where(mc =>
                {
                    if (mc.Reference == null) return false;
                    var parts = mc.Reference.Value!.Split(':');
                    if (parts.Length != 2) return false;

                    var startRow = new string(parts[0].Where(char.IsDigit).ToArray());
                    var endRow = new string(parts[1].Where(char.IsDigit).ToArray());
                    return startRow == baseRowIndex.ToString() && endRow == baseRowIndex.ToString();
                }).ToList();

            // 建立新列, 加在範本列後面
            for (int ri = 1; ri <= newRows; ri++)
            {
                var newRowIndex = baseRowIndex + (uint)ri;
                var newRow = new Row() { RowIndex = newRowIndex };  //base 1
                foreach (var field in _excelFields)
                {
                    var tplCell = tplRow.Elements<Cell>().FirstOrDefault(c =>
                        c.CellReference!.Value!.StartsWith(field.CellName));
                    var newCell = new Cell()
                    {
                        CellReference = field.CellName + newRowIndex,
                        DataType = CellValues.String,
                        CellValue = new CellValue(""),
                    };

                    if (tplCell?.StyleIndex != null)
                        newCell.StyleIndex = tplCell.StyleIndex;

                    newRow.Append(newCell);
                }

                sheetData.InsertAt(newRow, (int)newRowIndex - 1);

                // 複製合併格（更新為新列號）
                foreach (var mc in tplMerges)
                {
                    var parts = mc.Reference!.Value!.Split(':');
                    var startCol = new string(parts[0].Where(char.IsLetter).ToArray());
                    var endCol = new string(parts[1].Where(char.IsLetter).ToArray());
                    var newRef = $"{startCol}{newRowIndex}:{endCol}{newRowIndex}";
                    mergeCells.Append(new MergeCell() { Reference = new StringValue(newRef) });
                }
            }
        }

        private string CellValue(SharedStringTable ssTable, Cell cell)
        {
            //SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var value = cell.CellValue!.InnerXml;
            return (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                ? ssTable.ChildElements[Int32.Parse(value)].InnerText
                : value;
        }

        /// <summary>
        /// add row error
        /// </summary>
        /// <param name="error"></param>
        public void AddError(int rowNo, string error)
        {
            if (error != "")
            {
                _failRows.Add(new SnStrDto()
                {
                    Sn = rowNo,
                    Str = error,
                });
            }
        }

        /// <summary>
        /// add row error for multiple error msg
        /// </summary>
        /// <param name="rowNo"></param>
        /// <param name="results"></param>
        private void AddErrorByResults(int rowNo, List<ValidationResult> results)
        {
            var error = string.Join(RowSep, results.Select(a => a.ErrorMessage).ToList());
            AddError(rowNo, error);
        }

        #region remark code
        /*
        //excel docx to datatable
        private DataTable DocxToDataTable(SpreadsheetDocument docx)
        {
            //open excel
            WorkbookPart wbPart = docx.WorkbookPart;
            SharedStringTablePart ssPart = wbPart.GetPartsOfType<SharedStringTablePart>().First();
            SharedStringTable ssTable = ssPart.SharedStringTable;

        }

        //驗証資料列是否正確
        public bool ValidDto(T dto)
        {
            //欄位驗証
            var context = new ValidationContext(dto, null, null);
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(dto, context, results, true))
                return true;

            //case of error
            AddErrorByResults(results);
            return false;
        }

        private string Init(string asposeLicPath, string uploadFileName, string saveExcelPath, string tplPath, string sheetName)
        {
            //set instance variables
            this.asposeLicPath = asposeLicPath;
            this.uploadFileName = uploadFileName;
            this.saveExcelPath = saveExcelPath;
            this.tplPath = tplPath;
            this.sheetName = sheetName;
            this.sysFileName = Path.GetFileName(saveExcelPath);

            //建立excel connection
            var excelBook = Utils.OpenExcel(asposeLicPath, saveExcelPath);

            //set dataTable _dt
            var error = "";
            this.dt = Utils.ReadWorksheet(excelBook, sheetName, 0, 0, out error, true);
            return error;
        }

        /// <summary>
        /// 驗証excel資料列
        /// </summary>
        /// <returns>status</returns>
        protected void CheckRows()
        {
            //set instance variables
            this.nowIndex = 0;

            //check rows
            foreach (DataRow dr in dt.Rows)
            {
                //reset rowError
                this.rowError = new List<string>();

                //check row
                var model = CheckTableRow(dr);
                if (model != null)
                    okRows.Add(model);

                this.nowIndex++;
            }
        }

        //save error result to excel file
        protected void SaveErrorToExcel(int startRow, int colLen)
        {
            //check
            if (this._errorRows.Count == 0)
                return;

            //copy excel & add _fail for file name
            var fileName = Path.GetDirectoryName(this.saveExcelPath) + _Fun.DirSep
                + Path.GetFileNameWithoutExtension(this.saveExcelPath)
                + "_fail" + Path.GetExtension(this.saveExcelPath);
            File.Copy(this.tplPath, fileName, true);

            //set excel object
            Workbook excel = Utils.OpenExcel(asposeLicPath, fileName);
            //Workbook excel = Utils.NewExcel(asposeLicPath);
            Worksheet sheet = excel.Worksheets[this.sheetName];
            Cells cells = sheet.Cells;

            //set variables
            var addRow = 0; //寫入筆數
            //var colLen = dt.Columns.Count;  //匯入excel檔的欄位數
            //var colLen = dt.Columns.Count;  //匯入excel檔的欄位數
            //var colLen2 = colLen + 1;       //error excel檔的欄位數要加1

            //column index to string
            var colStrs = new string[colLen + 1];
            for (var i = 0; i <= colLen; i++)
                colStrs[i] = ColNumToStr(i + 1);

            //寫入error excel
            foreach (var errorRow in _errorRows)
            {
                //copy/paste row
                var rowPos = startRow + addRow;
                if (addRow > 0)
                {
                    //add row & copy row height
                    cells.InsertRow(rowPos - 1);   //base 1 !! insert after
                    cells.SetRowHeight(rowPos - 1, cells.GetRowHeight(startRow)); //base 1 !!

                    //copy styles(background/foreground colors, fonts, alignment styles etc.)
                    for (int col = 0; col <= colLen; col++)
                        cells[rowPos, col].Style = cells[startRow, col].Style;    //base 1 !!
                }

                //fill row
                var dtRow = this.dt.Rows[errorRow.Key];
                var rowStr = rowPos.ToString();
                for (var i = 0; i < colLen; i++)
                    cells[colStrs[i] + rowStr].PutValue(dtRow[i].ToString());

                //write error msg
                cells[colStrs[colLen] + rowStr].PutValue(errorRow.Value);
                addRow++;
            }
            excel.Save(fileName);
        }

        /// <summary>
        /// 檢查目前這一筆資料的狀態, true(正確), false(有誤)
        /// </summary>
        /// <returns></returns>
        protected bool NowRowStatus()
        {
            return (this.rowError.Count() == 0);
        }

        public void AddOk(int rowNo)
        {
            _okRowNos.Add(rowNo);
        }

        //excel column number to letter string
        //colNo: base 1
        private string ColNumToStr(int colNo)
        {
            int div = colNo;
            string colStr = String.Empty;
            int mod = 0;

            while (div > 0)
            {
                mod = (div - 1) % 26;
                colStr = (char)(65 + mod) + colStr;
                div = (int)((div - mod) / 26);
            }
            return colStr;
        }

        #region 欄位值 parsing function
        protected int? ParseIntAndLog(string colName, string value)
        {
            if (String.IsNullOrEmpty(value))
                return (int?)null;

            int value2;
            if (int.TryParse(value, out value2))
                return value2;

            rowError.Add(colName + _inputError);
            return (int?)null;
        }

        protected decimal? ParseDecimalAndLog(string colName, string value)
        {
            //return String.IsNullOrEmpty(value) ? (decimal?)null : decimal.Parse(value);
            if (String.IsNullOrEmpty(value))
                return (decimal?)null;

            decimal value2;
            if (decimal.TryParse(value, out value2))
                return value2;

            rowError.Add(colName + _inputError);
            return (decimal?)null;
        }

        protected DateTime? ParseDatetimeAndLog(string colName, string value)
        {
            if (String.IsNullOrEmpty(value))
                return (DateTime?)null;

            DateTime value2;
            if (DateTime.TryParse(value, out value2))
                return value2;

            rowError.Add(colName + _inputError);
            return (DateTime?)null;
        }
        #endregion
        */

        #endregion

    }//class
}
