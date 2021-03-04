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

//excel 匯入
namespace Base.Services
{
    //匯入excel, 提供公用的method
    //實際匯入的程式繼承此類別
    public class ExcelImportService<T> where T : class, new()
    {
        //constant
        //const string _inputError = "輸入錯誤。";
        const string _rowSep = "\r\n";  //row seperator

        
        #region instance variables
        //protected DataTable dt;   //excel to dataTable
        //protected int nowIndex;  //current datatable row index, base 0

        //記錄錯誤的 Datatable row index
        //protected List<KeyValuePair<int, string>> _errorRows = new List<KeyValuePair<int, string>>();

        //目前這筆資料的 error msg
        //protected List<string> rowError = null;

        //正確的資料列序號
        //protected List<T> okRows = new List<T>();
        private List<int> _okRowNos = new List<int>();

        //錯誤的資料列序號/訊息
        private List<SnStrDto> _errorRows = new List<SnStrDto>();
        #endregion

        /*
        //excel license file path
        //protected string asposeLicPath;
        protected string saveExcelPath; //with path
        protected string tplPath;
        protected string sheetName;
        protected string uploadFileName;
        protected string sysFileName;
        */

        #region 抽象方法宣告, 在子代實作
        //檢查table row(excel轉dataTable)
        //abstract public T CheckTableRow(DataRow dr);

        //執行匯入
        //abstract public void RunImport();
        #endregion

        /*
        /// <summary>
        /// constructor, 如果傳入參數有誤則送出 Exception
        /// </summary>
        /// <param name="asposeLicPath">server path</param>
        /// <param name="excelPath">excel file full path</param>
        /// <param name="checkTableRow"></param>
        /// <param name="sheetName">excel sheet name</param>
        public ImportExcelService(string asposeLicPath, string excelPath, string tplPath, string sheetName = "Sheet1")
        {
            try
            {
                //set instance variables
                this.asposeLicPath = asposeLicPath;
                this.excelPath = excelPath;
                this.tplPath = tplPath;
                this.sheetName = sheetName;

                //建立excel connection
                var excelBook = Utils.OpenExcel(asposeLicPath, excelPath);

                //set dataTable _dt
                var errorMsg = "";
                this.dt = Utils.ReadWorksheet(excelBook, sheetName, 1, 0, out errorMsg, true);
            }
            catch (Exception ex)
            {
                throw new Exception("ImportExcelService.cs constructor failed: " + ex.Message);
            }
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asposeLicPath"></param>
        /// <param name="uploadFileName"></param>
        /// <param name="saveExcelPath"></param>
        /// <param name="tplPath"></param>
        /// <param name="sheetName"></param>
        /// <returns>system error, not excel row error</returns>
        /*
        public string ImportByDocx_new(string asposeLicPath, string uploadFileName, string saveExcelPath, string tplPath, string sheetName = "Sheet1")
        {
            //initial
            var error = Init(asposeLicPath, uploadFileName, saveExcelPath, tplPath, sheetName);
            if (!string.IsNullOrEmpty(error))
                return error;

            //檢查excel rows
            CheckRows();

            //匯入
            RunImport();
            return "";
        }
        */

        public string ImportByStream(string frontDtFormat, Stream stream, ExcelImportDto<T> import)
        {
            stream.Position = 0;
            var docx = _Excel.StreamToDocx(stream);
            //var docx = SpreadsheetDocument.Open(stream, false);
            var error = ImportByDocx(frontDtFormat, docx, import);

            //release docx
            docx = null;
            return error;
        }

        //欄位字元移除後面的數字
        private string CellNameRemoveNum(string colName)
        {
            return Regex.Replace(colName, @"[\d]", string.Empty);
        }

        /// <summary>
        /// import by excel docx
        /// excel row/cell 為base 0
        /// </summary>
        /// <param name="docx"></param>
        /// <param name="import">匯入參數</param>
        /// <returns>error msg if any</returns>
        public string ImportByDocx(string frontDtFormat, SpreadsheetDocument docx, ExcelImportDto<T> import)
        {
            //檢查傳入參數           

            #region set docx, excelRows, ssTable
            var wbPart = docx.WorkbookPart;
            var wsPart = (WorksheetPart)wbPart.GetPartById(
                wbPart.Workbook.Descendants<Sheet>().ElementAt(import.SheetNo).Id);

            var excelRows = wsPart.Worksheet.Descendants<Row>();
            var ssTable = wbPart.GetPartsOfType<SharedStringTablePart>().First().SharedStringTable;
            #endregion

            #region set import.ExcelFids, excelFidLen
            int idx;
            //string colName;
            var colMap = new JObject();     //欄位字元(ex:A) -> 欄位陣列元素idx
            var cells = excelRows.ElementAt(0).Elements<Cell>();
            if (import.ExcelFids == null || import.ExcelFids.Count == 0)
            {
                //如果沒有傳入excel欄位名稱, 則使用第一行excel做為欄位名稱
                idx = 0;
                foreach (var cell in cells)
                {
                    import.ExcelFids.Add(GetCellValue(ssTable, cell));
                    colMap[CellNameRemoveNum(cell.CellReference)] = idx;
                    idx++;
                }
            }
            else
            {
                //有傳入excel欄位名稱
                //check
                var cellLen = cells.Count();
                if (cellLen != import.ExcelFids.Count)
                {
                    return "import.ExcelFids length should be " + cellLen;
                }

                //set colMap
                for (var i=0; i< cellLen; i++)
                {
                    var colName = CellNameRemoveNum(cells.ElementAt(i).CellReference);
                    colMap[colName] = i;
                }
            }

            //initial excelIsDates & set excelFidLen
            var excelIsDates = new List<bool>();        //是否為是日期欄位
            var excelFidLen = import.ExcelFids.Count;
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
                fno = import.ExcelFids.FindIndex(a => a == fid);
                if (fno < 0)
                    continue;

                modelFids.Add(fid);
                if (prop.PropertyType == typeof(DateTime?))
                    excelIsDates[fno] = true;
            }

            //var modelDateFidLen = modelDateFids.Count;
            //var modelNotDateFidLen = modelNotDateFids.Count;
            #endregion

            #region set modelRows
            //var rb = _Locale.RB;
            var modelRows = new List<T>();
            //Cell cell;
            //string value;
            var excelRowLen = excelRows.LongCount();
            for (var i = import.ExcelStartRow - 1; i < excelRowLen; i++)
            {
                //var cells = excelRows.ElementAt(i).Elements<Cell>();
                var excelRow = excelRows.ElementAt(i);
                //var cells = excelRows.ElementAt(i).Descendants<Cell>();
                //var cells = row.Elements<Cell>();

                var modelRow = new T();
                /*
                //寫入日期欄位
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
                        _Model.SetValue(modelRow, modelDateFids[j], DateTime.FromOADate(double.Parse(value)).ToString(rb.FrontDtFormat));
                    }
                }
                */

                //寫入非日期欄位
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
                    fno = (int)colMap[CellNameRemoveNum(cell.CellReference)];
                    var value = (cell.DataType == CellValues.SharedString) 
                        ? ssTable.ChildElements[int.Parse(cell.CellValue.Text)].InnerText 
                        : cell.CellValue.Text;
                    _Model.SetValue(modelRow, import.ExcelFids[fno], excelIsDates[fno]
                        ? DateTime.FromOADate(double.Parse(value)).ToString(frontDtFormat)
                        : value
                    );
                }

                modelRows.Add(modelRow);
            }
            #endregion

            #region validate modelRows loop
            idx = 0;
            var error = "";
            foreach (var modelRow in modelRows)
            {
                //validate
                var context = new ValidationContext(modelRow, null, null);
                var results = new List<ValidationResult>();
                if (Validator.TryValidateObject(modelRow, context, results, true))
                {
                    //user validate rule
                    if (import.FnCheckImportRow != null)
                        error = import.FnCheckImportRow(modelRow);
                    if (string.IsNullOrEmpty(error))
                        _okRowNos.Add(idx);
                    else
                        AddError(idx, error);
                }
                else
                {
                    AddErrorByResults(idx, results);
                }
                idx++;
            }
            #endregion

            #region save database if need
            if (_okRowNos.Count > 0)
            {
                //set okRows
                var okRows = new List<T>();
                foreach(var okRowNo in _okRowNos)
                    okRows.Add(modelRows[okRowNo]);

                idx = 0;
                var saveResults = import.FnSaveImportRows(okRows);
                if (saveResults != null)
                {
                    foreach (var result in saveResults)
                    {
                        if (!string.IsNullOrEmpty(result))
                            AddError(_okRowNos[idx], result);
                        idx++;
                    }
                }
            }
            #endregion

            //save excel file
            var fileStem = _Str.AddAntiSlash(import.SaveDir) + import.LogRowId;
            docx.SaveAs(fileStem + "_source.xlsx");

            #region save error excel file
            var errorLen = _errorRows.Count;
            if (errorLen > 0)
            {
                //產生 excel 欄位與 model 欄位的對應 excelFnos
                var excelFnos = new List<int>();
                for (var i = 0; i < excelFidLen; i++)
                {
                    fno = modelFids.FindIndex(a => a == import.ExcelFids[i]);
                    excelFnos.Add(fno);    //小於0表示無對應欄位
                }

                //get docx
                var errorFilePath = fileStem + "_error.xlsx";
                File.Copy(import.TplFilePath, errorFilePath, true);

                var docx2 = SpreadsheetDocument.Open(errorFilePath, true);
                var wbPart2 = docx2.WorkbookPart;
                var wsPart2 = (WorksheetPart)wbPart2.GetPartById(
                    wbPart2.Workbook.Descendants<Sheet>().ElementAt(0).Id);
                var sheetData2 = wsPart2.Worksheet.GetFirstChild<SheetData>();

                var startRow = import.ExcelStartRow - 1;    //insert position
                for (var i = 0; i < errorLen; i++)
                {
                    //add row, fill value & copy row style
                    var modelRow = modelRows[_errorRows[i].Sn];
                    var newRow = new Row();     //new excel row
                    for (var colNo = 0; colNo < excelFidLen; colNo++)
                    {
                        fno = excelFnos[colNo];
                        var value2 = _Model.GetValue(modelRow, import.ExcelFids[colNo]);
                        newRow.Append(new Cell()
                        {
                            CellValue = new CellValue(fno < 0 || value2 == null ? "" : value2.ToString()),
                            DataType = CellValues.String,
                        });
                    }

                    //write cell for error msg
                    newRow.Append(new Cell()
                    {
                        CellValue = new CellValue(_errorRows[i].Str),
                        DataType = CellValues.String,
                    });

                    sheetData2.InsertAt(newRow, startRow + i);
                }
                docx2.Save();
                docx2.Dispose();
            }
            #endregion

            #region add import log
            var sql = string.Format(@"
insert into dbo._ImportLog(Id, UploadFileName,
OkCount, FailCount, TotalCount,
CreatorName, Created)
values('{0}', '{1}',
{2}, {3}, {4},
'{5}', '{6}')
", import.LogRowId, import.UploadFileName,
modelRows.Count - _errorRows.Count, _errorRows.Count, modelRows.Count,
import.CreatorName, _Date.NowDbStr());

            _Db.ExecSql(sql);
            #endregion

            return string.Empty;
        }

        private string GetCellValue(SharedStringTable ssTable, Cell cell)
        {
            //SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var value = cell.CellValue.InnerXml;
            return (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                ? ssTable.ChildElements[Int32.Parse(value)].InnerText
                : value;
        }

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
            var fileName = Path.GetDirectoryName(this.saveExcelPath) + "\\"
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
        */

        /// <summary>
        /// 增加一筆row error
        /// </summary>
        /// <param name="error"></param>
        public void AddError(int rowNo, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _errorRows.Add(new SnStrDto() {
                    Sn = rowNo,
                    Str = error,
                });
            }
        }

        /// <summary>
        /// 增加一筆row error for 多個 error msg
        /// </summary>
        /// <param name="rowNo"></param>
        /// <param name="results"></param>
        private void AddErrorByResults(int rowNo, List<ValidationResult> results)
        {
            var error = string.Join(_rowSep, results.Select(a => a.ErrorMessage).ToList());
            AddError(rowNo, error);
        }

        /*
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

    }//class
}
