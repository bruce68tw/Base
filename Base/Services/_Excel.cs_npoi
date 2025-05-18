using Base.Models;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Base.Services
{
    public class _Excel
    {
        /*
        public static IWorkbook StreamToDocx(Stream stream)
        {
            return new XSSFWorkbook(stream);
        }
        */

        public static async Task<string> ImportByFileA(string uiDtFormat, string filePath, string insertSql, int[] excelCols, int excelStartRow, bool[]? isDates = null, int sheetNo = 0, Db? db = null)
        {
            if (!File.Exists(filePath))
                return "_Excel.cs ImportByFileA() failed, no file: " + filePath;

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var docx = new XSSFWorkbook(fs);
            return await ImportByDocxA(uiDtFormat, docx, insertSql, excelCols, excelStartRow, isDates, sheetNo, db);
        }

        public static async Task<string> ImportByDocxA(string uiDtFormat, IWorkbook docx, string insertSql, int[] excelCols, int excelStartRow, bool[]? isDates = null, int sheetNo = 0, Db? db = null)
        {
            var newDb = _Db.CheckOpenDb(ref db);
            var sheet = docx.GetSheetAt(sheetNo);
            var rowLen = sheet.PhysicalNumberOfRows;
            var colLen = excelCols.Length;
            var dateLen = isDates?.Length ?? 0;
            var cols = new string[colLen + 1];
            var error = "";

            for (var i = excelStartRow - 1; i < rowLen; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;
                cols[0] = (i + 1).ToString();

                for (int j = 0; j < colLen; j++)
                {
                    var cell = row.GetCell(excelCols[j]);
                    var value = cell?.ToString() ?? "";
                    if (dateLen > j && isDates[j] && double.TryParse(value, out var dateVal))
                        value = DateTime.FromOADate(dateVal).ToString(uiDtFormat);
                    cols[j + 1] = string.IsNullOrEmpty(value) ? (isDates?[j] == true ? "null" : "") : value;
                }

                var sql2 = string.Format(insertSql, cols).Replace("'null'", "null");
                if (await db!.ExecSqlA(sql2) == 0)
                {
                    error = "_Excel.cs ImportByDocxA() failed, sql is empty.";
                    break;
                }
            }

            await _Db.CheckCloseDbA(db!, newDb);
            return error;
        }

        /// <summary>
        /// file path to memory stream docx
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static IWorkbook FileToMsDocx(string filePath, MemoryStream ms)
        {
            // 讀取檔案 bytes
            var tplBytes = File.ReadAllBytes(filePath);

            // 將 bytes 寫入 MemoryStream
            ms.Write(tplBytes, 0, tplBytes.Length);
            ms.Position = 0;  // 一定要重置位置

            // 用 NPOI 讀取 Excel Workbook
            IWorkbook workbook = new XSSFWorkbook(ms);
            return workbook;
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
        public static async Task DocxByReadA(string ctrl, IWorkbook workbook, ReadDto readDto,
            JObject findJson, int srcRowNo, string dbStr = "")
        {
            var rows = await new CrudReadSvc(dbStr).GetExportRowsA(ctrl, readDto, findJson);
            if (rows != null)
                DocxByRows(rows, workbook, srcRowNo);
        }

        /// <summary>
        /// sql statement to excel file
        /// </summary>
        /// <param name="filePath">excel file path to save</param>
        /// <param name="sql"></param>
        /// <param name="dbStr">db property name in config file</param>
        public static async Task DocxBySqlA(string sql, IWorkbook workbook, int srcRowNo, Db? db = null)
        {
            var newDb = _Db.CheckOpenDb(ref db);
            var rows = await db!.GetRowsA(sql);
            await _Db.CheckCloseDbA(db, newDb);
            if (rows != null)
                DocxByRows(rows, workbook, srcRowNo);
        }

        /// <summary>
        /// json rows to openXml excel object
        /// see https://blog.johnwu.cc/article/asp-net-core-export-to-excel.html
        /// </summary>
        /// <param name="rows">json array</param>
        /// <param name="docx"></param>
        /// <param name="srcRowNo">excel start row, base 1</param>
        /// <returns>error msg if any</returns>
        public static string DocxByRows(JArray rows, IWorkbook workbook, int srcRowNo)
        {
            //if (workbook == null) return "_Excel.cs RowsToDocx() failed, workbook is null.";

            //2.get col name list from source rows[0]
            var rowCount = rows.Count;
            var cols = new List<string>();
            if (rowCount > 0)
            {
                foreach (var item in (JObject)rows[0])
                    cols.Add(item.Key);
            }

            #region prepare excel variables
            var colCount = cols.Count;

            // 取得第一個工作表
            var sheet = workbook.GetSheetAt(0);

            // 設定插入起始列 (NPOI 的 row index 從 0 開始)
            int startRowIndex = srcRowNo - 1;
            #endregion

            //3.loop of write excel rows, use template
            for (var rowNo = 0; rowNo < rowCount; rowNo++)
            {
                var rowData = (JObject)rows[rowNo];
                // 取得或建立該列
                var newRow = sheet.GetRow(startRowIndex + rowNo) ?? sheet.CreateRow(startRowIndex + rowNo);

                for (var colNo = 0; colNo < colCount; colNo++)
                {
                    var cell = newRow.GetCell(colNo) ?? newRow.CreateCell(colNo);
                    var value = rowData[cols[colNo]] == null ? "" : rowData[cols[colNo]]!.ToString();
                    cell.SetCellValue(value);

                    // 設定為字串型態 (NPOI 預設為字串)
                    // 如果需要特別格式化，可以在此加
                }
            }

            //case of ok
            return "";
        }


        //=== no change ===
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
        public static async Task<string> ImportByStreamA(string uiDtFormat, Stream stream, string insertSql, int excelStartRow, int[] excelCols, bool[]? isDates = null, int sheetNo = 0, Db? db = null)
        {
            stream.Position = 0;
            var docx = new XSSFWorkbook(stream);
            return await ImportByDocxA(uiDtFormat, docx, insertSql, excelCols, excelStartRow, isDates, sheetNo, db);
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
            var value = idx;  //商數
            //var mod = 0;    //餘數
            var colName = String.Empty;
            while (value > 0)
            {
                var mod = (value - 1) % 26;   //餘數
                value = (value - mod) / 26;
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
