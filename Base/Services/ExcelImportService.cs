using Base.Models;
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
    public class ExcelImportService<T> where T : class, new()
    {
        //constant
        const string RowSep = "\r\n";  //row seperator
        
        //ok excel row no
        private List<int> _okRowNos = new List<int>();

        //failed excel row no/msg
        private List<SnStrDto> _failRows = new List<SnStrDto>();

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
        /// <returns></returns>
        public async Task<ResultImportDto> ImportByStreamAsync(Stream stream, ExcelImportDto<T> importDto, string dirUpload, string fileName, string uiDtFormat)
        {
            stream.Position = 0;
            var docx = _Excel.StreamToDocx(stream);
            var result = await ImportByDocxAsync(docx, importDto, dirUpload, fileName, uiDtFormat);

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
        /// <param name="uiDtFormat"></param>
        /// <returns>error msg if any</returns>
        public async Task<ResultImportDto> ImportByDocxAsync(SpreadsheetDocument docx, ExcelImportDto<T> importDto, string dirUpload, string fileName, string uiDtFormat)
        {
            #region 1.set variables
            #region set docx, excelRows, ssTable
            //var errorMsg = "";
            var wbPart = docx.WorkbookPart;
            var wsPart = (WorksheetPart)wbPart.GetPartById(
                wbPart.Workbook.Descendants<Sheet>().ElementAt(importDto.SheetNo).Id);

            var excelRows = wsPart.Worksheet.Descendants<Row>();    //include empty rows
            var ssTable = wbPart.GetPartsOfType<SharedStringTablePart>().First().SharedStringTable;
            #endregion

            #region set importDto.ExcelFids, excelFidLen
            int idx;
            var colMap = new JObject();     //col x-way name(ex:A) -> col index
            var cells = excelRows.ElementAt(importDto.FidRowNo - 1).Elements<Cell>();
            var excelFids = new List<string>();
            //if (importDto.ExcelFids == null || importDto.ExcelFids.Count == 0)
            //{
                //如果沒有傳入excel欄位名稱, 則使用第一行excel做為欄位名稱
                idx = 0;
                foreach (var cell in cells)
                {
                    excelFids.Add(GetCellValue(ssTable, cell));
                    colMap[CellXname(cell.CellReference)] = idx;
                    idx++;
                }
            /*
            }
            else
            {
                //有傳入excel欄位名稱
                //check
                var cellLen = cells.Count();
                if (cellLen != importDto.ExcelFids.Count)
                {
                    errorMsg = "importDto.ExcelFids length should be " + cellLen;
                    goto lab_error;
                }

                //set colMap
                for (var i=0; i< cellLen; i++)
                {
                    var colName = CellXname(cells.ElementAt(i).CellReference);
                    colMap[colName] = i;
                }
            }
            */

            //initial excelIsDates & set excelFidLen
            var excelIsDates = new List<bool>();        //是否為是日期欄位
            var excelFidLen = excelFids.Count;
            for (var i = 0; i < excelFidLen; i++)
                excelIsDates.Add(false);    //initial
            #endregion

            #region set excelIsDates, modelFids, modelDateFids/Fno/Len, modelNotDateFids/Fno/Len
            int fno;
            var modelFids = new List<string>();         //全部欄位
            var model = new T();
            foreach (var prop in model.GetType().GetProperties())
            {
                //如果對應的excel欄位不存在, 則不記錄此欄位(skip)
                //var type = prop.GetValue(model, null).GetType();
                var fid = prop.Name;
                fno = excelFids.FindIndex(a => a == fid);
                if (fno < 0)
                    continue;

                modelFids.Add(fid);
                if (prop.PropertyType == typeof(DateTime?))
                    excelIsDates[fno] = true;
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
                foreach (Cell cell in excelRow)
                {
                    /*
                    if (i == 2 && j == 1)
                    {
                        var aa = "aa";
                    }
                    */
                    //var cell = cells.ElementAt(modelNotDateFnos[j]);
                    //var cell = excelRow.Descendants<Cell>().ElementAt(modelNotDateFnos[j]);
                    //colName = ;
                    fno = (int)colMap[CellXname(cell.CellReference)];
                    var value = (cell.DataType == CellValues.SharedString) 
                        ? ssTable.ChildElements[int.Parse(cell.CellValue.Text)].InnerText 
                        : cell.CellValue.Text;
                    _Model.SetValue(fileRow, excelFids[fno], excelIsDates[fno]
                        ? DateTime.FromOADate(double.Parse(value)).ToString(uiDtFormat)
                        : value
                    );
                }

                fileRows.Add(fileRow);
            }
            #endregion
            #endregion

            #region 2.validate fileRows loop
            idx = 0;
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
                        _okRowNos.Add(idx);
                    //else
                    //    AddError(idx, error);
                }
                else
                {
                    AddErrorByResults(idx, results);
                }
                idx++;
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
                idx = 0;
                var saveResults = importDto.FnSaveImportRows(okRows);
                if (saveResults != null)
                {
                    foreach (var result in saveResults)
                    {
                        if (!_Str.IsEmpty(result))
                            AddError(_okRowNos[idx], result);
                        idx++;
                    }
                }
            }
            #endregion

            #region 4.save ok excel file
            if (_Str.IsEmpty(importDto.LogRowId))
                importDto.LogRowId = _Str.NewId();
            var fileStem = _Str.AddDirSep(dirUpload) + importDto.LogRowId;
            docx.SaveAs(fileStem + ".xlsx");
            #endregion

            #region 5.save fail excel file (tail _fail.xlsx)
            var failCount = _failRows.Count;
            if (failCount > 0)
            {
                //set excelFnos: excel column map model column
                var excelFnos = new List<int>();
                for (var i = 0; i < excelFidLen; i++)
                {
                    fno = modelFids.FindIndex(a => a == excelFids[i]);
                    excelFnos.Add(fno);    //<0 means no mapping
                }

                //get docx
                var failFilePath = fileStem + "_fail.xlsx";
                File.Copy(importDto.TplPath, failFilePath, true);

                var docx2 = SpreadsheetDocument.Open(failFilePath, true);
                var wbPart2 = docx2.WorkbookPart;
                var wsPart2 = (WorksheetPart)wbPart2.GetPartById(
                    wbPart2.Workbook.Descendants<Sheet>().ElementAt(0).Id);
                var sheetData2 = wsPart2.Worksheet.GetFirstChild<SheetData>();

                var startRow = importDto.FidRowNo;    //insert position
                for (var i = 0; i < failCount; i++)
                {
                    //add row, fill value & copy row style
                    var modelRow = fileRows[_failRows[i].Sn];
                    var newRow = new Row();     //new excel row
                    for (var colNo = 0; colNo < excelFidLen; colNo++)
                    {
                        fno = excelFnos[colNo];
                        var value2 = _Model.GetValue(modelRow, excelFids[colNo]);
                        newRow.Append(new Cell()
                        {
                            CellValue = new CellValue(fno < 0 || value2 == null ? "" : value2.ToString()),
                            DataType = CellValues.String,
                        });
                    }

                    //write cell for error msg
                    newRow.Append(new Cell()
                    {
                        CellValue = new CellValue(_failRows[i].Str),
                        DataType = CellValues.String,
                    });

                    sheetData2.InsertAt(newRow, startRow + i);
                }
                docx2.Save();
                docx2.Dispose();
            }
            #endregion

            #region 6.insert ImportLog table
            var totalCount = fileRows.Count;
            var okCount = totalCount - failCount;
            var sql = $@"
insert into dbo.XpImportLog(Id, Type, FileName,
OkCount, FailCount, TotalCount,
CreatorName, Created)
values('{importDto.LogRowId}', '{importDto.ImportType}', '{fileName}',
{okCount}, {failCount}, {totalCount}, 
'{importDto.CreatorName}', '{_Date.NowDbStr()}')
";
            await _Db.ExecSqlAsync(sql);
            #endregion

            //7.return import result
            return new ResultImportDto()
            {
                OkCount = okCount,
                FailCount = failCount,
                TotalCount = totalCount,
            };
        }

        private string GetCellValue(SharedStringTable ssTable, Cell cell)
        {
            //SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var value = cell.CellValue.InnerXml;
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
            if (!_Str.IsEmpty(error))
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
