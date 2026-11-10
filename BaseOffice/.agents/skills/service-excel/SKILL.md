---
name: service-excel
description: 在 C# 應用整合 BaseOffice 的 _Excel 與 _HttpExcel，使用 Open XML 讀取、匯入或填寫 XLSX，或在 ASP.NET Core 接收上傳與輸出下載。
---

# BaseOffice Excel 服務

先確認使用端參照 [BaseOffice.csproj](../../../BaseOffice.csproj) 或對應版本的組件。兩個服務都在 `BaseOffice.Services`。[_Excel](../../../Services/_Excel.cs) 操作活頁簿及資料庫。[_HttpExcel](../../../Services/_HttpExcel.cs) 接收上傳檔與寫入 HTTP 下載回應。方法名稱裡的 `Docx` 在此指 Open XML 的 `SpreadsheetDocument`，對應 XLSX 活頁簿。

## 選擇公開入口

| 需求 | 方法 |
| --- | --- |
| 將上傳檔依模型規則匯入 | `_HttpExcel.ImportByFileA<T>(file, dirUpload, importDto, uiDtFormat, writeLog)` |
| 依指定 SQL 樣板逐列匯入資料庫 | `_Excel.ImportByFileA`、`ImportByStreamA` 或 `ImportByDocxA`，依輸入是路徑、串流或已開啟的活頁簿選用 |
| 把 JSON 資料列填入已開啟的活頁簿 | `_Excel.DocxByRows(rows, docx, srcRowNo)` |
| 取得 CRUD 查詢或 SQL 結果並填入活頁簿 | `_Excel.DocxByReadA` 或 `DocxBySqlA` |
| 在目前的 HTTP 請求輸出下載 | `_HttpExcel.ExportByRowsA`、`ExportByReadA` 或 `ExportBySqlA`，依來源是 JSON 資料列、CRUD 查詢或 SQL 選用 |
| 唯讀開啟 XLSX 串流 | `_Excel.StreamToDocx(stream)`，由呼叫端釋放回傳的活頁簿 |
| 轉換日期序號或欄名 | `_Excel.DateIntToStr(value, uiDtFormat)` 依指定格式輸出 OLE Automation 日期。欄名與欄號用 `ColNameToIdx` 和 `ColIdxToName`，兩者的欄號皆從 1 起算，例如 A 對應 1。 |

## 匯入資料

- **HTTP 上傳**使用 `IFormFile`、可寫入的 `dirUpload` 和 [ExcelImportDto<T>](../../../Models/ExcelImportDto.cs)。以 DTO 指定工作表、欄位對應與儲存委派。空檔的錯誤訊息在結果的 `_ErrorMsg`，正常流程則回傳匯入筆數與紀錄識別值。`writeLog` 控制是否寫入 `XpImportLog`。若有失敗列，匯入流程使用 `TplPath` 產生失敗清單。
- **SQL 樣板匯入**的 `sheetNo` 從 0 起算，依 `WorksheetParts` 的列舉順序取工作表。`excelStartRow` 從 1 起算，依現有 `Row` 元素的順序計算。`excelCols` 從 0 起算，依列內現有 `Cell` 元素定位。省略空白列或儲存格的活頁簿可能造成位置偏移，且此路徑要求活頁簿含共用字串表。
- `insertSql` 的格式參數 `{0}` 是列序號，`{1}` 起依 `excelCols` 的順序填入值。`isDates` 也按選取欄位的順序設定。任一選取欄位有有效值時便會嘗試執行 SQL。此路徑直接將儲存格值格式化到 SQL，處理未受信任的上傳資料時，使用具備參數化儲存委派的模型匯入流程。
- SQL 樣板方法回傳空字串表示未回報錯誤。讀檔、索引、格式轉換與資料庫操作仍可能拋出例外。`ImportByStreamA` 會重設串流位置，因此傳入可定位的串流。

## 填寫或下載活頁簿

- `DocxByRows` 寫入活頁簿列出的第一張工作表。第一筆 JSON 物件的鍵決定欄位及順序。插入的儲存格是字串，現行實作未複製範本列樣式。
- `srcRowNo` 實際是 `SheetData` 的零起算子元素插入位置，與 Excel 顯示的列號可能不同。直接呼叫 `DocxByRows` 或其查詢入口時，傳入可編輯的活頁簿，由呼叫端在完成後儲存並關閉。
- HTTP 匯出以記憶體中的範本副本寫入資料，關閉活頁簿後直接將串流送到目前的回應。使用有效的 `tplPath` 與帶 `.xlsx` 的下載檔名。匯出後結束 action，避免再寫另一份回應。CRUD 或 SQL 查詢回傳 `null` 時會跳過下載。

整合時用實際的 XLSX 範本與資料核對起始位置、空白格、日期與輸出格式。使用模型匯入時，再核對成功及失敗列的筆數與輸出檔。
