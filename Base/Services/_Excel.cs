using Base.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Base.Services
{
    public class _Excel
    {
        /// <summary>
        /// convert excel Stream to Openxml Workbook(Docx), 外部可呼叫
        /// 必須引入 using DocumentFormat.OpenXml.Spreadsheet;
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static SpreadsheetDocument StreamToDocx(Stream stream)
        {
            //return SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook).WorkbookPart.Workbook;
            return SpreadsheetDocument.Open(stream, false);
        }

        /// <summary>
        /// import large excel(.xlsx 2007 after) to db, need DAE(microsoft data access engine) !!
        /// Note:
        ///   1.import table first column must be LineNo
        ///   2.can not assign rows range(only OpenRowSet, but need db server install access engine, not consider!!),
        ///   3.can not assign excel begin row & has header or not
        ///   4.use bulkcopy(fast, OpenXml is slow), need include sqlClient
        /// </summary>
        /// <param name="filePath">excel file path</param>
        /// <param name="sheetName">excel sheet name</param>
        /// <param name="excelStartRow">excel start row(base 1), not work!!</param>
        /// <param name="toTable">into db table name</param>
        /// <param name="tableCols">into table column list, type need match to excel</param>
        /// <param name="notNullFid">not empty excel column</param>
        /// <param name="excelHeader">excel has header or not, not work, must be true!!</param>
        /// <param name="excelCols">excel columns name, if empty then use tableCols</param>
        /// <returns>import status</returns>
        /*
        public static bool ImportByLargeFile(string filePath, string sheetName, int excelStartRow, string toTable, string[] tableCols, string notNullFid = "", bool excelHeader = true, string[] excelCols = null)
        {
            //excel to datatable
            string header;
            int startRow;
            if (excelHeader)
            {
                header = "YES";
                startRow = 2;
            }
            else
            {
                header = "NO";
                startRow = 1;
            }

            if (excelCols == null)
                excelCols = tableCols;

            var connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR={1};IMEX=1'";
            connStr = string.Format(connStr, filePath, header);

            //get sql for read excel columns
            var sql = "";
            foreach (var col in excelCols)
                sql += col + ",";
            sql = "select " + sql.Substring(0, sql.Length - 1) + " from [" + sheetName + "$]";
            //sql = "select * from [" + sheetName + "]";
            if (notNullFid != "")
                sql += string.Format(" where not ({0} is null or {0} = '')", notNullFid);

            var dt = new DataTable();
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connStr))
                {
                    conn.Open();

                    //get columnName if need
                    //DataTable schema = conn.GetSchema("Columns");
                    //DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    //var cols = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, null);
                    //sheetName = schema.Rows[0]["TABLE_NAME"].ToString() + "$A5:R";

                    //dataAdapter
                    //OleDbDataAdapter da = new OleDbDataAdapter();
                    //DataSet ds = new DataSet();
                    //da.Fill(ds);

                    OleDbCommand cmd = new OleDbCommand(sql, conn);
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(dt);

                    //need if {}, or will error !!
                    //if (excelStartRow > 2)
                    //{
                    //    dt = dt.AsEnumerable().Skip(excelStartRow - 2) as DataTable;
                    //}
                    //conn.Close();
                }

                //datatable add LineNum
                var lineNum = "LineNum";
                dt.Columns.Add(lineNum, typeof(Int32));
                var rowNo = startRow;  //excel start with row 2 !!
                foreach (DataRow row in dt.Rows)
                    row[lineNum] = rowNo++;

                //remove not importd rows
                //for (var i = startRow; i < excelStartRow; i++)
                //    dt.Rows.RemoveAt(0);

                //copy to table
                using (var bulkCopy = new SqlBulkCopy(_Fun.Config.Db))
                {
                    bulkCopy.DestinationTableName = toTable;
                    bulkCopy.ColumnMappings.Add(lineNum, lineNum);
                    for (var i = 0; i < tableCols.Length; i++)
                        bulkCopy.ColumnMappings.Add(excelCols[i], tableCols[i]);

                    bulkCopy.WriteToServer(dt);
                }
                return true;
            }
            catch (Exception ex)
            {
                _Log.Error("_Excel.cs ToTableByFile() failed: " + ex.Message);
                return false;
            }
        }
        */

        /// <summary>
        /// import db by excel file, table first column must be LineNo
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="insertSql">sql for import</param>
        /// <param name="excelCols">import excel column no, base 0</param>
        /// <param name="excelStartRow"></param>
        /// <param name="isDates">excel column flag of date</param>
        /// <param name="sheetNo"></param>
        /// <param name="db"></param>
        /// <returns>import status</returns>
        public static bool ImportByPath(string frontDtFormat, string filePath, string insertSql, int[] excelCols, int excelStartRow, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            //check
            if (!File.Exists(filePath))
            {
                _Log.Error("_Excel.ToTable() failed, no file: " + filePath);
                return false;
            }

            var docx = SpreadsheetDocument.Open(filePath, false);
            return ImportByDocx(frontDtFormat, docx, insertSql, excelCols, excelStartRow, isDates, sheetNo, db);
        }

        /// <summary>
        /// import db by openXml object
        /// </summary>
        /// <param name="docx"></param>
        /// <param name="insertSql"></param>
        /// <param name="excelCols"></param>
        /// <param name="excelStartRow"></param>
        /// <param name="isDates"></param>
        /// <param name="sheetNo"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool ImportByDocx(string frontDtFormat, SpreadsheetDocument docx, string insertSql, int[] excelCols, int excelStartRow, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            //var rb = _Locale.RB;
            var emptyDb = false;
            _Fun.CheckOpenDb(ref db, ref emptyDb);

            //open excel
            var wbPart = docx.WorkbookPart;
            var ssPart = wbPart.GetPartsOfType<SharedStringTablePart>().First();
            var ssTable = ssPart.SharedStringTable;

            var ok = true;
            var colLen = excelCols.Length;
            var dateLen = (isDates == null) ? 0 : isDates.Length;
            var cols = new string[colLen + 1];  //first column must be LineNo
            var rows = wbPart.WorksheetParts.ElementAt(sheetNo).Worksheet.Descendants<Row>();
            var rowLen = rows.LongCount();
            for (var i = excelStartRow - 1; i < rowLen; i++)
            {
                var rowHasCol = false;
                cols[0] = (i + 1).ToString(); //base 1
                var row = rows.ElementAt(i);
                var cells = row.Elements<Cell>();
                int colNo;
                Cell cell;
                for (int j = 0; j < colLen; j++)
                {
                    colNo = excelCols[j];
                    cell = cells.ElementAt(colNo);
                    var value = "";
                    if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                    {
                        var ssid = int.Parse(cell.CellValue.Text);
                        value = ssTable.ChildElements[ssid].InnerText;
                    }
                    else if (cell.CellValue != null)
                    {
                        value = cell.CellValue.Text;
                    }

                    //excel date column is double type, must transfer to datetime with format !!
                    cols[j + 1] = (value == "")
                        ? (dateLen > j && isDates[j])
                            ? "null"
                            : ""
                        : (dateLen > j && isDates[j])
                            ? DateTime.FromOADate(double.Parse(value)).ToString(frontDtFormat)
                            : value;

                    if (cols[j + 1] != "" && cols[j + 1] != "null")
                        rowHasCol = true;
                }

                //insert into only when all columns has value
                //transfer emtpy datetime to null, or it will be 1900/1/1
                var sql2 = string.Format(insertSql, cols).Replace("'null'", "null");
                if (rowHasCol && db.ExecSql(sql2) == 0)
                {
                    ok = false;
                    break;
                }
            }

            _Fun.CheckCloseDb(db, emptyDb);

            //book = null;    //can not Dispose(), must close by caller
            //sheet = null;
            return ok;
        }

        /// <summary>
        /// import db by stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="insertSql"></param>
        /// <param name="excelStartRow"></param>
        /// <param name="excelCols"></param>
        /// <param name="isDates"></param>
        /// <param name="sheetNo"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool ImportByStream(string frontDtFormat, Stream stream, string insertSql, int excelStartRow, int[] excelCols, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            stream.Position = 0;
            var docx = StreamToDocx(stream);
            var ok = ImportByDocx(frontDtFormat, docx, insertSql, excelCols, excelStartRow, isDates, sheetNo, db);

            //release docx
            //docx = null;
            return ok;
        }

        /// <summary>
        /// tpl file to docx
        /// </summary>
        /// <param name="tplPath"></param>
        /// <returns></returns>
        public static SpreadsheetDocument TplToDocx(MemoryStream ms, string tplPath)
        {
            //var ms = new MemoryStream();
            //SpreadsheetDocument docx = null;
            var tplBytes = File.ReadAllBytes(tplPath);
            ms.Write(tplBytes, 0, tplBytes.Length);
            return SpreadsheetDocument.Open(ms, true);
        }

        /// <summary>
        /// file or tpl to docx
        /// </summary>
        /// <param name="filePath">要產生的excel檔案, 不可為空白</param>
        /// <param name="tplPath">excel template file path, 可為空白</param>
        /// <returns></returns>
        private static SpreadsheetDocument PathToDocx(string filePath, string tplPath = "")
        {
            //filePath不可空白
            if (string.IsNullOrEmpty(filePath))
                return null;

            if (!string.IsNullOrEmpty(tplPath))
            {
                if (!File.Exists(tplPath))
                {
                    _Log.Error("no template file: " + tplPath);
                    return null;
                }

                //copy to filepath if need
                File.Copy(tplPath, filePath, true);
                return SpreadsheetDocument.Open(filePath, true);
            }
            else
            {
                //var stream = new MemoryStream();
                return SpreadsheetDocument.Create(new MemoryStream(), SpreadsheetDocumentType.Workbook);
            }
        }

        /// <summary>
        /// crud export to Excel file
        /// </summary>
        /// <param name="readDto">crud setting</param>
        /// <param name="findJson"></param>
        /// <param name="filePath">export excel path</param>
        /// <param name="sheetName">excel sheet name</param>
        /// <param name="headers">excel header list</param>
        /// <param name="cols"></param>
        /// <param name="dbStr"></param>
        public static void ExportByRead(string filePath, ReadDto readDto, JObject findJson, string tplPath = "", int srcRowNo = 1, string dbStr = "")
        {
            ExportByRows(filePath, new CrudRead(dbStr).GetAllRows(readDto, findJson, true), tplPath, srcRowNo);
        }
        public static void ExportByRead(SpreadsheetDocument docx, ReadDto readDto, JObject findJson, string dbStr = "")
        {
            ExportByRows(docx, new CrudRead(dbStr).GetAllRows(readDto, findJson, true));
        }

        /// <summary>
        /// sql statement to excel file
        /// </summary>
        /// <param name="filePath">excel file path to save</param>
        /// <param name="sql"></param>
        /// <param name="dbStr">db property name in config file</param>
        public static void ExportBySql(string filePath, string sql, string template = "", int srcRowNo = 1, Db db = null)
        {
            var emptyDb = false;
            _Fun.CheckOpenDb(ref db, ref emptyDb);

            var rows = db.GetJsons(sql);
            _Fun.CheckCloseDb(db, emptyDb);

            ExportByRows(filePath, rows, template, srcRowNo);
        }
        public static void ExportBySql(SpreadsheetDocument docx, string sql, Db db = null)
        {
            var emptyDb = false;
            _Fun.CheckOpenDb(ref db, ref emptyDb);

            var rows = db.GetJsons(sql);
            _Fun.CheckCloseDb(db, emptyDb);
            
            ExportByRows(docx, rows);
        }

        /// <summary>
        /// json rows to Excel file
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filePath"></param>
        /// <param name="tplPath">excel template, 如果空白, 則欄位名稱會使用row field id</param>
        /// <param name="srcRowNo">資料開始列數, base 1, 如果為0表示沒有使用template file, 則此參數自動設為1</param>
        /// <param name="sheetName">default 'Sheet1', excel save sheet name</param>
        //public static void FileByRows(JArray rows, string filePath, string sheetName, List<string> headers = null, List<string> cols = null)
        public static void ExportByRows(string filePath, JArray rows, string tplPath = "", int srcRowNo = 1)
        {
            //var docx = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
            var docx = PathToDocx(filePath, tplPath);
            ExportByRows(docx, rows);
            /*
            if (string.IsNullOrEmpty(tplPath))
                srcRowNo = 0;
            RowsToDocx(rows, docx, srcRowNo);
            */
            docx.Dispose();
        }

        public static void ExportByRows(SpreadsheetDocument docx, JArray rows)
        {
            RowsToDocx(rows, docx, 1);
        }

        /*
        public static void DocxByCrud(ReadModel crud, JObject findJson, SpreadsheetDocument docx, string sheetName, List<string> headers = null, List<string> cols = null, string dbStr = "")
        {
            DocxByRows(new CrudRead(dbStr).GetAllRows(crud, findJson), docx, sheetName, headers, cols);
        }

        public static void DocxBySql(string sql, SpreadsheetDocument docx, string sheetName, List<string> headers = null, List<string> cols = null, string dbStr = "")
        {
            var rows = _Db.GetJsons(sql, null, dbStr);
            DocxByRows(rows, docx, sheetName, headers, cols);
        }
        */

        /// <summary>
        /// json rows to openXml excel object
        /// see https://blog.johnwu.cc/article/asp-net-core-export-to-excel.html
        /// </summary>
        /// <param name="rows">json array</param>
        /// <param name="docx"></param>
        /// <param name="srcRowNo">資料開始列數, base 1, 如果為0表示沒有使用template file, 則此參數自動設為1</param>
        public static void RowsToDocx(JArray rows, SpreadsheetDocument docx, int srcRowNo = 1)
        {
            //check
            if (docx == null)
            {
                _Log.Error("_Excel.cs RowsToDocx() failed, docx is null.");
                return;
            }

            //set cols
            var rowCount = (rows == null) ? 0 : rows.Count;
            var cols = new List<string>();
            if (rowCount > 0)
            {
                foreach (var item in (JObject)rows[0])
                    cols.Add(item.Key);
            }

            //if (headers == null)
            //    headers = cols;

            SheetData sheetData = null;
            var hasTpl = (srcRowNo > 0);
            var colCount = cols.Count;
            if (hasTpl)
            {
                //use template file
                #region prepre excel-sheetData
                var sheet = docx.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                var wsPart = (WorksheetPart)docx.WorkbookPart.GetPartById(sheet.Id);
                //var ws = wsPart.Worksheet;
                sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();                
                #endregion

                //var count = sheetData.Elements<Row>().Count();
                //TODO: copy row style
                //srcRow = sheetData.Elements<Row>().ElementAt(srcRowNo);

                //write excel row, use template
                for (var rowNo = 0; rowNo < rowCount; rowNo++)
                {
                    //第一筆時不必新增一列
                    var row = (JObject)rows[rowNo];
                    /*
                    if (rowNo == 0)
                    {
                        nowRow = sheetData.Elements<Row>().ElementAt(srcRowNo);
                        Cell cell;
                        for (var colNo = 0; colNo < colCount; colNo++)
                        {
                            cell = nowRow.Elements<Cell>().ElementAt(colNo);
                            cell.CellValue = new CellValue(row[cols[colNo]] == null ? "" : row[cols[colNo]].ToString());
                            cell.DataType = CellValues.String;
                        }
                    }
                    else
                    {
                    */
                        //新增一列 & 填入欄位
                        //TODO: copy row style
                        var newRow = new Row();
                        for (var colNo = 0; colNo < colCount; colNo++)
                        {
                            newRow.Append(new Cell()
                            {
                                CellValue = new CellValue(row[cols[colNo]] == null ? "" : row[cols[colNo]].ToString()),
                                DataType = CellValues.String,
                            });
                        }
                        sheetData.InsertAt(newRow, rowNo + srcRowNo);
                    //}
                }
            }
            else
            {
                //no template file
                #region prepre excel-sheetData
                var bookPart = docx.AddWorkbookPart();
                bookPart.Workbook = new Workbook();

                var sheetPart = bookPart.AddNewPart<WorksheetPart>();
                sheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = bookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet()
                {
                    Id = bookPart.GetIdOfPart(sheetPart),
                    SheetId = 1,
                    Name = "Sheet1",
                });
                sheetData = sheetPart.Worksheet.GetFirstChild<SheetData>();
                #endregion

                //add header row
                //srcRowNo = 1;   //base 1
                var newRow = new Row();
                for (var i = 0; i < colCount; i++)
                {
                    newRow.Append(new Cell()
                    {
                        CellValue = new CellValue(cols[i]),
                        DataType = CellValues.String,
                    });
                }
                sheetData.AppendChild(newRow);

                //write excel row, no template
                for (var rowNo = 0; rowNo < rowCount; rowNo++)
                {
                    //var excelRow = NewRow(row, colCount, cols);
                    var row = (JObject)rows[rowNo];
                    newRow = new Row();
                    for (var colNo = 0; colNo < colCount; colNo++)
                    {
                        newRow.Append(new Cell()
                        {
                            CellValue = new CellValue(row[cols[colNo]] == null ? "" : row[cols[colNo]].ToString()),
                            DataType = CellValues.String,
                        });
                    }
                    sheetData.AppendChild(newRow);
                }
            }
        }

        /*
        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;
            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
        */

        /// <summary>
        /// excel column name(ex:A) to column index(base 0)
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static int ColNameToIdx(string colName)
        {
            //if (string.IsNullOrEmpty(colStr)) throw new ArgumentNullException("columnName");

            colName = colName.ToUpperInvariant();
            var idx = 0;
            for (var i = 0; i < colName.Length; i++)
            {
                idx *= 26;
                idx += (colName[i] - 'A' + 1);
            }
            return idx;
        }

        /// <summary>
        /// excel column number to letter string, ex: 'A'
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>col index, base 1</returns>
        public static string ColIdxToName(int idx)
        {
            var div = idx;  //商數
            //var mod = 0;    //餘數
            var colName = String.Empty;
            while (div > 0)
            {
                var mod = (div - 1) % 26;   //餘數
                div = (div - mod) / 26;
                colName = (char)(65 + mod) + colName;
            }
            return colName;
        }

        /*
        //產生一筆新的excel row
        private static Row NewRow(JObject row, int colCount, List<string> cols)
        {
            var excelRow = new Row();
            for (var colNo = 0; colNo < colCount; colNo++)
            {
                excelRow.Append(new Cell()
                {
                    CellValue = new CellValue(row[cols[colNo]] == null ? "" : row[cols[colNo]].ToString()),
                    DataType = CellValues.String,
                });
            }
            return excelRow;
        }

        //設定excel row
        private static void SetRow(Row excelRow, JObject row, int colCount, List<string> cols)
        {
            for (var colNo = 0; colNo < colCount; colNo++)
            {
                excelRow.Elements<Cell>().ElementAt(colNo + 1).CellValue =
                    new CellValue(row[cols[colNo]] == null ? "" : row[cols[colNo]].ToString());
            }
        }
        */

        #region remark code
        /* (NPOI) ToTableByBook
        /// <summary>
        /// by NPOI book
        /// </summary>
        /// <param name="file">檔案路徑</param>
        /// <param name="sql">insert sql</param>
        /// <param name="excelCols">要匯入的excel檔案的欄位, base 0</param>
        /// <param name="excelStartRow">開始匯入的 row no, base 1(配合 excel)</param>
        /// <param name="db"></param>
        /// <returns></returns>
        //public static bool ToTableByBook(XSSFWorkbook book, string sql, List<int> excelCols, List<string> formats, int excelStartRow, int sheetNo, Db db)
        public static bool ToTableByBook(XSSFWorkbook book, string sql, int excelStartRow, int[] excelCols, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            //open excel
            //XSSFWorkbook workbook = new XSSFWorkbook(FileUpload1.FileContent,);
            var emptyDb = (db == null);
            if (emptyDb)
                db = new Db();

            var ok = true;
            XSSFSheet sheet = (XSSFSheet)book.GetSheetAt(sheetNo);
            var colLen = excelCols.Length;
            var dateLen = (isDates == null) ? 0 : isDates.Length;
            var cols = new string[colLen + 1];  //第一個欄位必須為lineNo
            var rowLen = sheet.LastRowNum;
            //var formatLen = (formats == null) ? 0 : formats.Count;
            var dbFormat = db.GetUserProfile().GetDbFormat();
            for (int i = excelStartRow - 1; i <= rowLen; i++)
            {
                var hasCol = false;
                cols[0] = (i+1).ToString(); //base 1
                XSSFRow row = (XSSFRow)sheet.GetRow(i);
                for (int j = 0; j < colLen; j++)
                {
                    //var prop = props[j];
                    var colNo = excelCols[j];
                    var cell = row.GetCell(colNo);
                    cols[j + 1] = (cell == null || cell.CellType == CellType.Blank)
                        ? (dateLen > j && isDates[j])
                            ? "null"
                            : ""
                        : (dateLen > j && isDates[j])
                            ? row.GetCell(colNo).DateCellValue.ToString(dbFormat.FrontDt)
                            : row.GetCell(colNo).ToString();

                    if (cols[j + 1] != "" && cols[j + 1] != "null")
                        hasCol = true;
                }

                //insert into, 欄位都有值才寫入
                var sql2 = string.Format(sql, cols).Replace("'null'", "null");
                if (hasCol && !db.Update(sql2))
                {
                    ok = false;
                    break;
                }
            }

            if (emptyDb)
                db.Dispose();

            //book = null;    //無法 Dispose(), 並且由上一層程式 close
            sheet = null;
            return ok;
        }
        */

        /* (NPOI) SheetsToExcel
        public static MemoryStream SheetsToExcel(List<ExcelSheetModel> sheets)
        {
            IWorkbook book = new XSSFWorkbook(); //XSSF 用來產生Excel 2007檔案（.xlsx）
            var stream = new MemoryStream();
            foreach (var sheet in sheets)
            {
                    //isheet;
                //建立一個工作表
                ISheet isheet = book.CreateSheet(sheet.SheetName);
                for (int rowNo = 0; rowNo < sheet.Rows.Count; rowNo++)
                {
                    //建立列
                    var row = sheet.Rows[rowNo];
                    isheet.CreateRow(rowNo);
                    for (int cellNo = 0; cellNo < row.Cells.Count; cellNo++)
                    {
                        //建立欄 全部以字串的方式存入
                        isheet.GetRow(rowNo).CreateCell(cellNo).SetCellValue(row.Cells[cellNo]);
                    }
                }
            }

            //寫入 MemoryStream
            book.Write(stream);
            //釋出
            book = null;
            return stream;
        }
        */

        /* JsonsToExcel
        //把json資料列輸出成為excel檔案
        //cols: 欄位id, 如果為null, 則寫全rows全部欄位
        public static MemoryStream JsonsToExcel(string sheetName, JArray rows, List<string> headers = null, List<string> cols = null)
        {
            //add one excel worksheet
            var sheets = new List<ExcelSheetModel>();
            var sheet = new ExcelSheetModel()
            {
                SheetName = sheetName
            };

            //set cols & headers
            if ((cols == null || cols.Count == 0) && rows.Count > 0)
            {
                cols = new List<string>();
                foreach (var item in (JObject)rows[0])
                    cols.Add(item.Key);
            }
            if (headers == null)
                headers = cols;

            IWorkbook book = new XSSFWorkbook(); //XSSF 用來產生Excel 2007檔案（.xlsx）
            ISheet isheet = book.CreateSheet(sheetName);

            //add header
            var excelRow = isheet.CreateRow(0);
            for(var i=0; i<headers.Count; i++)
                excelRow.CreateCell(i).SetCellValue(headers[i]);

            //add header
            //sheet.Rows.Add(new ExcelRowModel() { Cells = headers });

            //資料寫入excel
            var colCount = cols.Count;
            for (var rowNo = 0; rowNo < rows.Count; rowNo++)
            {
                var row = (JObject)rows[rowNo];
                excelRow = isheet.CreateRow(rowNo + 1);
                for (var i = 0; i < colCount; i++)
                    excelRow.CreateCell(i).SetCellValue(row[cols[i]] == null ? "" : row[cols[i]].ToString());
            }

            var stream = new MemoryStream();
            book.Write(stream);

            Response.AddHeader("Content-Disposition", "attachment; filename=" + sheetName + ".xlsx");
            Response.BinaryWrite(stream.ToArray());

            book = null;                //釋出
            stream.Close();
            stream.Dispose();
        }
        */
        #endregion

    } //class
}
