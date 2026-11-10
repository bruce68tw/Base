using Base.Models;
using Base.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;

namespace BaseOffice.Services
{
    /// <summary>以 Open XML 讀寫 Excel，支援查詢結果匯出與逐列匯入。</summary>
    public class _Excel
    {
        /// <summary>將 Excel 日期序號轉成指定格式的日期字串。</summary>
        /// <param name="value">要解析的 Excel 日期序號字串。</param>
        /// <param name="uiDtFormat">日期儲存格轉成字串時使用的格式</param>
        /// <returns></returns>
        public static string DateIntToStr(string value, string uiDtFormat)
        {
            return DateTime.FromOADate(double.Parse(value)).ToString(uiDtFormat);
        }

        /// <summary>以唯讀方式從串流開啟 Open XML 試算表。 (Docx)</summary>
        /// <param name="stream">包含 Excel 工作簿內容的串流。</param>
        /// <returns></returns>
        public static SpreadsheetDocument StreamToDocx(Stream stream)
        {
            //return SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook).WorkbookPart.Workbook;
            return SpreadsheetDocument.Open(stream, false);
        }

        /// <summary>從 Excel 檔案讀取指定工作表，並依 SQL 逐列寫入資料庫。</summary>
        /// <param name="filePath">要讀取或寫入的完整檔案路徑</param>
        /// <param name="insertSql">逐列寫入資料庫使用的 INSERT SQL</param>
        /// <param name="excelCols">依匯入順序選取的 Excel 欄索引清單，從零開始</param>
        /// <param name="excelStartRow">要開始匯入的 Excel 列號，從 1 開始</param>
        /// <param name="isDates">與匯入欄逐項對應的日期轉換旗標</param>
        /// <param name="sheetNo">要讀取的工作表索引，從零開始</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="uiDtFormat">日期儲存格轉成字串時使用的格式</param>
        /// <returns>成功時回傳空字串，失敗時回傳錯誤訊息。</returns>
        public static async Task<string> ImportByFileA(string uiDtFormat, string filePath, string insertSql, int[] excelCols, int excelStartRow, bool[]? isDates = null, int sheetNo = 0, Db? db = null)
        {
            //check
            if (!File.Exists(filePath))
                return "_Excel.ToTable() failed, no file: " + filePath;

            var docx = SpreadsheetDocument.Open(filePath, false);
            return await ImportByDocxA(uiDtFormat, docx, insertSql, excelCols, excelStartRow, isDates, sheetNo, db);
        }

        /// <summary>從已開啟的 Open XML 試算表讀取指定工作表，並依 SQL 逐列寫入資料庫。</summary>
        /// <param name="docx">已開啟的 Open XML 工作簿</param>
        /// <param name="insertSql">逐列寫入資料庫使用的 INSERT SQL</param>
        /// <param name="excelCols">依匯入順序選取的 Excel 欄索引清單，從零開始</param>
        /// <param name="excelStartRow">要開始匯入的 Excel 列號，從 1 開始</param>
        /// <param name="isDates">與匯入欄逐項對應的日期轉換旗標</param>
        /// <param name="sheetNo">要讀取的工作表索引，從零開始</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="uiDtFormat">日期儲存格轉成字串時使用的格式</param>
        /// <returns>成功時回傳空字串，失敗時回傳錯誤訊息。</returns>
        public static async Task<string> ImportByDocxA(string uiDtFormat, SpreadsheetDocument docx, string insertSql, int[] excelCols, int excelStartRow, bool[]? isDates = null, int sheetNo = 0, Db? db = null)
        {
            //open excel
            var newDb = _Db.CheckOpenDb(ref db);
            var wbPart = docx.WorkbookPart;
            // 此匯入路徑要求活頁簿含共用字串表，即使選取的欄位都是數值。
            var ssPart = wbPart!.GetPartsOfType<SharedStringTablePart>().First();
            var ssTable = ssPart.SharedStringTable!;

            //var ok = true;
            var error = "";
            var colLen = excelCols.Length;
            var dateLen = (isDates == null) ? 0 : isDates.Length;
            var cols = new string[colLen + 1];  //first column must be LineNo
            var rows = wbPart.WorksheetParts.ElementAt(sheetNo).Worksheet!.Descendants<Row>();
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
                    // 欄號依該列現有的 Cell 元素順序計算。若省略空白格，後續欄位的位置可能偏移。
                    colNo = excelCols[j];
                    cell = cells.ElementAt(colNo);
                    var value = "";
                    if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                    {
                        var ssid = int.Parse(cell.CellValue!.Text);
                        value = ssTable.ChildElements[ssid].InnerText;
                    }
                    else if (cell.CellValue != null)
                    {
                        value = cell.CellValue.Text;
                    }

                    //excel date column is double type, must transfer to datetime with format !!
                    cols[j + 1] = (value == "")
                        ? (dateLen > j && isDates![j])
                            ? "null"
                            : ""
                        : (dateLen > j && isDates![j])
                            ? DateIntToStr(value, uiDtFormat)
                            : value;

                    if (cols[j + 1] != "" && cols[j + 1] != "null")
                        rowHasCol = true;
                }

                // 只要有一個選取欄位含有效值，就會嘗試寫入這一列。
                //insert into only when all columns has value
                //transfer emtpy datetime to null, or it will be 1900/1/1
                var sql2 = string.Format(insertSql, cols).Replace("'null'", "null");
                if (rowHasCol && await db!.ExecSqlA(sql2) == 0)
                {
                    //ok = false;
                    error = "_Excel.cs ImportByDocx failed, sql is empty.";
                    break;
                }
            }

            await _Db.CheckCloseDbA(db!, newDb);

            //book = null;    //can not Dispose(), must close by caller
            //sheet = null;
            return error;
        }

        /// <summary>
        /// FileToMsDocx -> FileToDocx
        /// file path to memory stream docx
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /*
        public static SpreadsheetDocument FileToMsDocx(string filePath, MemoryStream ms)
        {
            // 讀取檔案 bytes
            var tplBytes = File.ReadAllBytes(filePath);

            // 將 bytes 寫入 MemoryStream
            ms.Write(tplBytes, 0, tplBytes.Length);
            ms.Position = 0;  // 一定要重置位置

            // 讀取 Excel 物件
            var docx = SpreadsheetDocument.Open(ms, true);
            return docx;
        }
        */

        /// <summary>依 CRUD 查詢與匯出權限取得資料，填入已開啟的試算表。</summary>
        /// <param name="ctrl">對應作業的控制器名稱</param>
        /// <param name="docx">已開啟的 Open XML 工作簿</param>
        /// <param name="readDto">查詢 SQL 與欄位條件設定</param>
        /// <param name="findJson">查詢欄位與條件值的 JSON</param>
        /// <param name="srcRowNo">資料列寫入 SheetData 的零起算插入位置。</param>
        /// <param name="dbStr">資料庫連線字串</param>
        public static async Task DocxByReadA(string ctrl, SpreadsheetDocument docx, ReadDto readDto,
            JObject findJson, int srcRowNo, string dbStr = "")
        {
            var rows = await new CrudReadSvc(dbStr).GetExportRowsA(ctrl, readDto, findJson);
            if (rows != null)
                DocxByRows(rows, docx, srcRowNo);
        }

        /// <summary>執行 SQL 取得資料，填入已開啟的試算表。</summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="docx">已開啟的 Open XML 工作簿</param>
        /// <param name="srcRowNo">資料列寫入 SheetData 的零起算插入位置。</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task DocxBySqlA(string sql, SpreadsheetDocument docx, int srcRowNo, Db? db = null)
        {
            var newDb = _Db.CheckOpenDb(ref db);
            var rows = await db!.GetRowsA(sql);
            await _Db.CheckCloseDbA(db, newDb);
            if (rows != null)
                DocxByRows(rows, docx, srcRowNo);
        }

        /// <summary>將 JSON 資料列寫入已開啟的 Open XML 試算表。</summary>
        /// <param name="rows">依順序處理的資料列集合</param>
        /// <param name="docx">已開啟的 Open XML 工作簿</param>
        /// <param name="srcRowNo">資料列寫入 SheetData 的零起算插入位置。</param>
        /// <returns>成功時回傳空字串，失敗時回傳錯誤訊息。</returns>
        public static string DocxByRows(JArray rows, SpreadsheetDocument docx, int srcRowNo)
        {
            //if (docx == null) return "_Excel.cs RowsToDocx() failed, docx is null.";

            //2.get col name list from source rows[0]
            var rowCount = rows.Count;
            var cols = new List<string>();
            if (rowCount > 0)
            {
                foreach (var item in (JObject)rows[0])
                    cols.Add(item.Key);
            }

            #region prepare excel variables
            //SheetData? sheetData = null;
            var colCount = cols.Count;
            var sheet = docx.WorkbookPart!.Workbook!.Descendants<Sheet>().FirstOrDefault();
            var wsPart = (WorksheetPart)docx.WorkbookPart.GetPartById(sheet!.Id!);
            var sheetData = wsPart.Worksheet!.GetFirstChild<SheetData>();
            #endregion

            //3.loop of write excel rows, use template
            for (var rowNo = 0; rowNo < rowCount; rowNo++)
            {
                //add row and fill data, TODO: copy row style
                var row = (JObject)rows![rowNo];
                var newRow = new Row();
                for (var colNo = 0; colNo < colCount; colNo++)
                {
                    newRow.Append(new Cell()
                    {
                        CellValue = new CellValue(row[cols[colNo]] == null ? "" : row[cols[colNo]]!.ToString()),
                        DataType = CellValues.String,
                    });
                }

                // srcRowNo 實際作為 SheetData 子元素的零起算插入位置。
                //insert row into sheet
                sheetData!.InsertAt(newRow, rowNo + srcRowNo);
            }

            //case of ok
            return "";
        }


        //=== no change ===
        /// <summary>將 Excel 串流移回開頭後開啟工作簿，並依 SQL 匯入資料。</summary>
        /// <param name="stream">承載文件內容的串流</param>
        /// <param name="insertSql">逐列寫入資料庫使用的 INSERT SQL</param>
        /// <param name="excelStartRow">要開始匯入的 Excel 列號，從 1 開始</param>
        /// <param name="excelCols">依匯入順序選取的 Excel 欄索引清單，從零開始</param>
        /// <param name="isDates">與匯入欄逐項對應的日期轉換旗標</param>
        /// <param name="sheetNo">要讀取的工作表索引，從零開始</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="uiDtFormat">日期儲存格轉成字串時使用的格式</param>
        /// <returns>成功時回傳空字串，失敗時回傳錯誤訊息。</returns>
        public static async Task<string> ImportByStreamA(string uiDtFormat, Stream stream, string insertSql, int excelStartRow, int[] excelCols, bool[]? isDates = null, int sheetNo = 0, Db? db = null)
        {
            stream.Position = 0;
            var docx = StreamToDocx(stream);
            return await ImportByDocxA(uiDtFormat, docx, insertSql, excelCols, excelStartRow, isDates, sheetNo, db);
        }

        /// <summary>將 Excel 欄名轉為從零開始的索引。 (ex:A) (base 0)</summary>
        /// <param name="colName">要轉換的 Excel 字母欄名</param>
        /// <returns></returns>
        public static int ColNameToIdx(string colName)
        {
            //if (_Str.IsEmpty(colStr)) throw new ArgumentNullException("columnName");

            // 現行回傳值從 1 開始，例如 A 為 1，AA 為 27。
            colName = colName.ToUpperInvariant();
            var idx = 0;
            for (var i = 0; i < colName.Length; i++)
            {
                idx *= 26;
                idx += (colName[i] - 'A' + 1);
            }
            return idx;
        }

        /// <summary>將從零開始的 Excel 欄索引轉為字母欄名。</summary>
        /// <param name="idx">要轉換的零起算欄索引</param>
        /// <returns>對應的 Excel 字母欄名。</returns>
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
