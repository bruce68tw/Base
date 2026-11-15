---
name: service-excel-import
description: 在 C# 應用整合 BaseOffice 的泛型 ExcelImportSvc，將 XLSX 對應成模型、執行欄位驗證、透過委派儲存資料並產生失敗清單。
---

# BaseOffice Excel 模型匯入

確認應用參照 [BaseOffice.csproj](../../../BaseOffice.csproj) 或對應版本的組件。使用 [ExcelImportSvc<T>](../../../Services/ExcelImportSvc.cs) 與 [ExcelImportDto<T>](../../../Models/ExcelImportDto.cs)。`T` 必須是具有公開無參數建構子的類別。每次匯入建立新的服務實例，因為服務會保留成功及失敗列的索引。

## 選擇輸入入口

- `ImportByStreamA(stream, importDto, dirUpload, fileName, uiDtFormat, writeLog)` 接受可定位的 XLSX 串流。服務從開頭讀取，正常返回前會關閉由它開啟的活頁簿。
- `ImportByDocxA(docx, importDto, dirUpload, fileName, uiDtFormat, writeLog)` 接受已開啟的 Open XML `SpreadsheetDocument`，由呼叫端管理文件生命週期。
- 從 ASP.NET Core 的上傳檔進入時，可用 [service-excel](../service-excel/SKILL.md) 所載的 HTTP 上傳入口。兩個直接入口的檔案讀取與轉換可能拋出例外，呼叫端依應用既有流程處理。

## 設定欄位與驗證

- `SheetNo` 從 0 起算，依活頁簿列出的工作表順序選取。`FidRowNo` 從 1 起算，定位的是過濾空白列後的欄位列。資料從該列之後開始，活頁簿須含 Open XML 共用字串表。
- `ExcelFids` 留空時，服務從欄位列讀取模型屬性名稱，這種方式使用沒有合併格的欄位列。明確傳入時，清單長度須與該列的儲存格元素數一致，順序對應 A 到 Z。空字串項目會被略過。欄位名稱須與模型屬性完全一致，欄位標記至少兩個字元。
- 對 Excel 數值日期欄位用 `D:屬性名` 標記，並設定 `uiDtFormat`。服務先轉成日期字串，再依模型屬性分類寫入。現行轉型按日期時間、整數與字串處理，空的整數欄位會先轉成 0。使用實際範本檢查日期格式及空值。
- 模型欄位賦值後以 Data Annotations 驗證。轉型或欄位對應例外可能在逐列驗證前中斷匯入。

## 儲存及判讀結果

- 需要寫入資料庫時，在 DTO 設定 `FnSaveImportRows` 或 `FnSaveImportRowsA`。委派收到通過模型驗證的資料列。兩者同時設定時，優先使用同步委派。以輸入順序回傳每筆的處理結果，成功列填空字串，失敗列填錯誤訊息，讓服務對回正確的列。成功筆數本身無法證明已寫入資料庫，因為儲存委派可以省略。
- `dirUpload` 指向存在且可寫入的目錄。來源活頁簿依 `LogRowId` 留存，有失敗列時再依 `TplPath` 產生帶 `_fail` 後綴的檔案，填入資料與錯誤訊息。失敗範本的第一張工作表須有對應欄位與資料起始列。若未提供 `LogRowId`，服務會產生一個。
- 回傳 `ResultImportDto`，檢查 `_ErrorMsg` 及 `OkCount`、`FailCount`、`TotalCount`、`LogRowId`。`writeLog` 控制是否新增 `XpImportLog` 紀錄。儲存委派早於檔案輸出與紀錄寫入，後續步驟失敗時應先查明已寫入哪些資料。
- 公開的 `AddError(rowNo, error)` 記錄非空錯誤，`rowNo` 是轉成模型後的資料清單索引，從 0 起算。儲存委派依序回傳逐筆結果即可由服務對應索引。
