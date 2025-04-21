using Base.Enums;
using Base.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
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
    public class ExcelImportSvc<T> where T : class, new()
    {
        //constant
        const string RowSep = "\r\n";  //row seperator
        
        //ok excel row no
        private List<int> _okRowNos = [];

        //failed excel row no/msg
        private List<SnStrDto> _failRows = [];

        //cell x-way name(no number)
        private string CellXname(string colName)
        {
            return Regex.Replace(colName, @"[\d]", string.Empty);
        }

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
            docx = null;
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
            //var colMap = new JObject();     //col x-way name(ex:A) -> col index
            var colNameNos = new Dictionary<string, int>();     //excel位置字母,位置對應, ex:(A,0)
            var cells = excelRows.ElementAt(importDto.FidRowNo - 1).Elements<Cell>();
            var excelFids = new List<string>();
            var excelFnos = new List<int>();
            if (importDto.ExcelFids == null || importDto.ExcelFids.Count == 0)
            {
                //如果沒有傳入excel欄位名稱, 則使用第一行excel做為欄位名稱(不可有合併儲存格!!)
                no = 0;
                foreach (var cell in cells)
                {
                    excelFids.Add(GetCellValue(ssTable, cell));
                    excelFnos.Add(no);
                    colNameNos[CellXname(cell.CellReference!)] = no;
                    no++;
                }
            }
            else
            {
                //有傳入excel欄位名稱(最多到Z)
                //check
                var cellLen = cells.Count();
                if (cellLen != importDto.ExcelFids.Count)
                {
                    return new ResultImportDto()
                    {
                        ErrorMsg = "importDto.ExcelFids length should be " + cellLen,
                    };
                }

                //set colMap
                no = 0;
                for (var i=0; i< importDto.ExcelFids.Count; i++)
                {
                    //空白表示有合併儲存格
                    if (importDto.ExcelFids[i] == "")
                        continue;

                    excelFids.Add(importDto.ExcelFids[i]);
                    excelFnos.Add(i);
                    //var colName = CellXname(cells.ElementAt(no).CellReference!);
                    var colName = ((char)('A' + i)).ToString();
                    colNameNos[colName] = i;
                    no++;
                }
            }

            //initial excelIsDates & set excelFidLen
            //var excelIsDates = new List<bool>();        //是否為是日期欄位
            var excelFidLen = excelFids.Count;
            //for (var i = 0; i < excelFidLen; i++)
            //    excelIsDates.Add(false);    //initial
            #endregion

            #region set excelIsDates, modelFids, modelDateFids/Fno/Len, modelNotDateFids/Fno/Len
            int fno;
            //var modelFids = new List<string>();         //全部欄位
            //var modelTypes = new List<string>();        //欄位種類
            var modelFidTypes = new Dictionary<string, string>();
            var model = new T();
            foreach (var prop in model.GetType().GetProperties())
            {
                //如果對應的excel欄位不存在, 則不記錄此欄位(skip)
                //var type = prop.GetValue(model, null).GetType();
                var fid = prop.Name;
                fno = excelFids.FindIndex(a => a == fid);
                if (fno < 0) continue;

                var typeName = prop.PropertyType.Name;
                var ftype = typeName.Contains("DateTime") ? ModelTypeEstr.Datetime :
                    typeName.Contains("Int") ? ModelTypeEstr.Int :
                    ModelTypeEstr.String;

                //modelFids.Add(fid);
                //modelTypes.Add(ftype);
                modelFidTypes.Add(fid, ftype);
                //if (prop.PropertyType == typeof(DateTime?))
                //    excelIsDates[fno] = true;
            }

            //var modelDateFidLen = modelDateFids.Count;
            //var modelNotDateFidLen = modelNotDateFids.Count;
            #endregion

            #region set fileRows by excel file
            var fileRows = new List<T>();   //excel rows with data(not empty row)
            var excelRowLen = excelRows.LongCount();
            for (var i = importDto.FidRowNo; i < excelRowLen; i++)
            {
                var excelRow = excelRows.ElementAt(i);
                var fileRow = new T();
                /*
                //set datetime column
                //var rowHasCol = false;
                for(var j=0; j<modelDateFidLen; j++)
                {
                    //var cell = cells.ElementAt(modelDateFnos[j]);
                    var cell = excelRow.Descendants<Cell>().ElementAt(j);
                    if (cell.DataType != null)
                    {
                        //rowHasCol = true;
                        value = (cell.DataType == CellValues.SharedString) ? ssTable.ChildElements[int.Parse(cell.CellValue.Text)].InnerText :
                            cell.CellValue.Text;
                        _Model.SetValue(modelRow, modelDateFids[j], DateTime.FromOADate(double.Parse(value)).ToString(rb.uiDtFormat));
                    }
                }
                */

                //write not date column
                //for (var j = 0; j < modelNotDateFidLen; j++)
                //var j = 0;
                //foreach (Cell cell in excelRow)
                no = 0;
                foreach (var col in colNameNos)
                {
                    //var cell = cells.ElementAt(modelNotDateFnos[j]);
                    //var cell = excelRow.Descendants<Cell>().ElementAt(modelNotDateFnos[j]);
                    //colName = ;
                    //fno = (int)colMap[CellXname(cell.CellReference!)]!;
                    var cname = col.Key;    //cell name
                    var cno = col.Value;
                    var fid = excelFids[no];
                    var ftype = modelFidTypes[fid];
                    var cell = excelRow.Elements<Cell>().ElementAt(cno);
                    var value = cell.CellValue!.Text;
                    object value2 = (cell.DataType != null && cell.DataType! == CellValues.SharedString) 
                            ? ssTable.ChildElements[int.Parse(value)].InnerText :
                        (ftype == ModelTypeEstr.Datetime) ? DateTime.FromOADate(double.Parse(value)).ToString(uiDtFormat) :
                        (ftype == ModelTypeEstr.Int) ? Convert.ToInt32(value) :
                        value.ToString();
                    
                    _Model.SetValue(fileRow, fid, value2);
                    no++;
                }

                fileRows.Add(fileRow);
            }
            #endregion
            #endregion

            #region 2.validate fileRows loop
            no = 0;
            //var error = "";
            foreach (var fileRow in fileRows)
            {
                //validate
                var context = new ValidationContext(fileRow, null, null);
                var results = new List<ValidationResult>();
                if (Validator.TryValidateObject(fileRow, context, results, true))
                {
                    //user validate rule
                    //if (importDto.FnCheckImportRow != null)
                    //    error = importDto.FnCheckImportRow(fileRow);
                    //if (_Str.IsEmpty(error))
                        _okRowNos.Add(no);
                    //else
                    //    AddError(idx, error);
                }
                else
                {
                    AddErrorByResults(no, results);
                }
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

            //todo: 未測試新方法
            //docx.SaveAs(fileStem + ".xlsx");
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
                /*
                //set excelFnos: excel column map model column
                var excelFnos = new List<int>();
                for (var i = 0; i < excelFidLen; i++)
                {
                    fno = colNameNos[excelFids[i]];
                    excelFnos.Add(fno);    //<0 means no mapping
                }
                */

                //get docx
                var failFilePath = fileStem + "_fail.xlsx";
                File.Copy(importDto.TplPath, failFilePath, true);

                var docx2 = SpreadsheetDocument.Open(failFilePath, true);
                var wbPart2 = docx2.WorkbookPart!;
                var wsPart2 = (WorksheetPart)wbPart2.GetPartById(
                    wbPart2.Workbook.Descendants<Sheet>().ElementAt(0).Id!);
                var sheetData2 = wsPart2.Worksheet.GetFirstChild<SheetData>();

                //新增空白列(含格式, 合併儲存格)
                var startRow = importDto.FidRowNo;    //insert position
                if (failCount > 1)
                    CopyTplRow(sheetData2!, startRow, failCount - 1);

                for (var i = 0; i < failCount; i++)
                {
                    //var row = sheetData2!.ElementAt[startRow + i];
                    var row = sheetData2!.Elements<Row>().ElementAt(startRow + i + 1);  //base 0
                    //var row = sheetData2!.Elements<Row>().FirstOrDefault(r => r.RowIndex! == (uint)(startRow + i + 1));

                    //add row, fill value & copy row style
                    var modelRow = fileRows[_failRows[i].Sn];
                    var newRow = new Row();     //new excel row
                    for (var ci = 0; ci < excelFidLen; ci++)
                    {
                        fno = excelFnos[ci];
                        var value2 = _Model.GetValue(modelRow, excelFids[ci]);
                        
                        var cell = row.Elements<Cell>().ElementAt(fno);
                        cell.CellValue = new CellValue(value2?.ToString() ?? string.Empty);
                        cell.DataType = CellValues.String; // 設定為字串
                        /*
                        newRow.Append(new Cell()
                        {
                            CellValue = new CellValue(fno < 0 || value2 == null ? "" : value2.ToString()!),
                            DataType = CellValues.String,
                        });
                        */
                    }

                    //write cell for error msg
                    row.Append(new Cell()
                    {
                        CellValue = new CellValue(_failRows[i].Str),
                        DataType = CellValues.String,
                    });

                    //sheetData2!.InsertAt(newRow, startRow + i);
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

        private void CopyTplRow(SheetData sheet, int fromRow, int newRows)
        {
            // 找出範本列
            //var tplRow = sheet.Elements<Row>().FirstOrDefault(r => r.RowIndex! == (uint)fromRow);
            var tplRow = sheet.Elements<Row>().ElementAt(fromRow);  //base 0
            if (tplRow == null) return;

            // 找到範本列在 SheetData 中的 index
            int insertIndex = sheet.Elements<Row>().ToList().IndexOf(tplRow) + 1;

            // 取得 MergeCells
            var mergeCells = sheet.Elements<MergeCells>().FirstOrDefault() ?? new MergeCells();

            // 複製合併儲存格
            var tplMerges = mergeCells.Elements<MergeCell>()
                .Where(mc => mc.Reference!.Value!.Contains(fromRow.ToString()))
                .ToList();

            // 插入新列
            uint nowRowIndex = (uint)(fromRow + 1);
            for (int i = 0; i < newRows; i++)
            {
                Row newRow = new Row() { RowIndex = nowRowIndex };
                foreach (var cell in tplRow.Elements<Cell>())
                {
                    string col = new string(cell.CellReference!.Value!.Where(char.IsLetter).ToArray());
                    newRow.Append(new Cell()
                    {
                        CellReference = col + nowRowIndex,
                        StyleIndex = cell.StyleIndex,
                        DataType = CellValues.String,
                        CellValue = new CellValue("") // 空資料
                    });
                }
                sheet.InsertAt(newRow, insertIndex + i);

                // 複製合併儲存格
                foreach (var mc in tplMerges)
                {
                    var parts = mc.Reference!.Value!.Split(':');
                    string newRef = $"{parts[0].First()} {nowRowIndex}: {parts[1]}{nowRowIndex}";
                    mergeCells.Append(new MergeCell() { Reference = new StringValue(newRef) });
                }

                nowRowIndex++;
            }

            // 如果新增了 mergeCells，更新回 SheetData
            if (mergeCells.Elements<MergeCell>().Any())
                sheet.InsertAfter(mergeCells, sheet);
        }

        private string GetCellValue(SharedStringTable ssTable, Cell cell)
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
