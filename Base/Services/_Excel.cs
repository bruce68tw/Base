using Base.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    public class _Excel
    {
        /// <summary>
        /// convert excel Stream to Openxml Workbook(Docx)
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static SpreadsheetDocument StreamToDocx(Stream stream)
        {
            //return SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook).WorkbookPart.Workbook;
            return SpreadsheetDocument.Open(stream, false);
        }

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
        /// <returns>error msg if any</returns>
        public static async Task<string> ImportByFileAsync(string uiDtFormat, string filePath, string insertSql, int[] excelCols, int excelStartRow, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            //check
            if (!File.Exists(filePath))
            {
                return "_Excel.ToTable() failed, no file: " + filePath;
                //return false;
            }

            var docx = SpreadsheetDocument.Open(filePath, false);
            return await ImportByDocxAsync(uiDtFormat, docx, insertSql, excelCols, excelStartRow, isDates, sheetNo, db);
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
        /// <returns>error msg if any</returns>
        public static async Task<string> ImportByDocxAsync(string uiDtFormat, SpreadsheetDocument docx, string insertSql, int[] excelCols, int excelStartRow, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            //var rb = _Locale.RB;
            var emptyDb = false;
            _Fun.CheckOpenDb(ref db, ref emptyDb);

            //open excel
            var wbPart = docx.WorkbookPart;
            var ssPart = wbPart.GetPartsOfType<SharedStringTablePart>().First();
            var ssTable = ssPart.SharedStringTable;

            //var ok = true;
            var error = "";
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
                            ? DateTime.FromOADate(double.Parse(value)).ToString(uiDtFormat)
                            : value;

                    if (cols[j + 1] != "" && cols[j + 1] != "null")
                        rowHasCol = true;
                }

                //insert into only when all columns has value
                //transfer emtpy datetime to null, or it will be 1900/1/1
                var sql2 = string.Format(insertSql, cols).Replace("'null'", "null");
                if (rowHasCol && await db.ExecSqlAsync(sql2) == 0)
                {
                    //ok = false;
                    error = "_Excel.cs ImportByDocx failed, sql is empty.";
                    break;
                }
            }

            await _Fun.CheckCloseDb(db, emptyDb);

            //book = null;    //can not Dispose(), must close by caller
            //sheet = null;
            return error;
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
        /// <returns>error msg if any</returns>
        public static async Task<string> ImportByStreamAsync(string uiDtFormat, Stream stream, string insertSql, int excelStartRow, int[] excelCols, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            stream.Position = 0;
            var docx = StreamToDocx(stream);
            return await ImportByDocxAsync(uiDtFormat, docx, insertSql, excelCols, excelStartRow, isDates, sheetNo, db);
        }

        /// <summary>
        /// file path to memory stream docx
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static SpreadsheetDocument FileToMsDocx(string filePath, MemoryStream ms)
        {
            var tplBytes = File.ReadAllBytes(filePath);
            //ms.Position = 0;
            ms.Write(tplBytes, 0, tplBytes.Length);
            return SpreadsheetDocument.Open(ms, true);
        }

        /// <summary>
        /// crud read export to Excel file
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="docx"></param>
        /// <param name="readDto"></param>
        /// <param name="findJson"></param>
        /// <param name="srcRowNo"></param>
        /// <param name="dbStr"></param>
        public static async Task DocxByReadAsync(string ctrl, SpreadsheetDocument docx, ReadDto readDto, 
            JObject findJson, int srcRowNo, string dbStr = "")
        {
            DocxByRows(await new CrudRead(dbStr).GetExportRowsAsync(ctrl, readDto, findJson), docx, srcRowNo);
        }

        /// <summary>
        /// sql statement to excel file
        /// </summary>
        /// <param name="filePath">excel file path to save</param>
        /// <param name="sql"></param>
        /// <param name="dbStr">db property name in config file</param>
        public static async Task DocxBySqlAsync(string sql, SpreadsheetDocument docx, int srcRowNo, Db db = null)
        {
            var emptyDb = false;
            _Fun.CheckOpenDb(ref db, ref emptyDb);

            var rows = await db.GetJsonsAsync(sql);
            await _Fun.CheckCloseDb(db, emptyDb);

            DocxByRows(rows, docx, srcRowNo);
        }

        /// <summary>
        /// json rows to openXml excel object
        /// see https://blog.johnwu.cc/article/asp-net-core-export-to-excel.html
        /// </summary>
        /// <param name="rows">json array</param>
        /// <param name="docx"></param>
        /// <param name="srcRowNo">excel start row, base 1</param>
        /// <returns>error msg if any</returns>
        public static string DocxByRows(JArray rows, SpreadsheetDocument docx, int srcRowNo)
        {
            #region 1.check docx
            if (docx == null)
                return "_Excel.cs RowsToDocx() failed, docx is null.";
            #endregion

            //2.get col name list from source rows[0]
            var rowCount = (rows == null) ? 0 : rows.Count;
            var cols = new List<string>();
            if (rowCount > 0)
            {
                foreach (var item in (JObject)rows[0])
                    cols.Add(item.Key);
            }

            #region prepare excel variables
            SheetData sheetData = null;
            var colCount = cols.Count;
            var sheet = docx.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
            var wsPart = (WorksheetPart)docx.WorkbookPart.GetPartById(sheet.Id);
            sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();
            #endregion

            //3.loop of write excel rows, use template
            for (var rowNo = 0; rowNo < rowCount; rowNo++)
            {
                //add row and fill data, TODO: copy row style
                var row = (JObject)rows[rowNo];
                var newRow = new Row();
                for (var colNo = 0; colNo < colCount; colNo++)
                {
                    newRow.Append(new Cell()
                    {
                        CellValue = new CellValue(row[cols[colNo]] == null ? "" : row[cols[colNo]].ToString()),
                        DataType = CellValues.String,
                    });
                }

                //insert row into sheet
                sheetData.InsertAt(newRow, rowNo + srcRowNo);
            }

            //case of ok
            return "";
            #region remark: no template file
            /*
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
            */
            #endregion
        }

        /// <summary>
        /// excel column name(ex:A) to column index(base 0)
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static int ColNameToIdx(string colName)
        {
            //if (_Str.IsEmpty(colStr)) throw new ArgumentNullException("columnName");

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

        #region remark code
        /// <summary>
        /// template file to file docx
        /// </summary>
        /// <param name="filePath">output file path</param>
        /// <param name="tplPath">excel template file path</param>
        /// <returns></returns>
        /*
        private static SpreadsheetDocument GetFileDocxByTpl(ref string error, string tplPath, string filePath)
        {
            //check
            error = ""; //initial
            if (_Str.IsEmpty(filePath))
                return null;

            if (!File.Exists(tplPath))
            {
                error = "no template file: " + tplPath;
                return null;
            }

            //copy to filepath if need
            File.Copy(tplPath, filePath, true);
            return SpreadsheetDocument.Open(filePath, true);

            //new docx
            //return SpreadsheetDocument.Create(new MemoryStream(), SpreadsheetDocumentType.Workbook);
        }
        */

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
        #endregion

    } //class
}
