---
name: service-word
description: 在 C# 應用整合 BaseOffice 的 Word 服務，使用 _Word、_HttpWord 和 WordSetSvc 套表、輸出 DOCX 串流或 HTTP 下載、合併文件，或使用 WordImageSvc 準備圖片。
---

# BaseOffice Word 服務

先確認使用端參照 [BaseOffice.csproj](../../../BaseOffice.csproj) 或對應版本的組件。這些服務都在 `BaseOffice.Services`，圖片設定在 `BaseOffice.Models`。以實際引用版本的公開方法為準。套表由 [WordSetSvc](../../../Services/WordSetSvc.cs) 使用 Open XML 處理。

## 依輸出需求選用方法

| 需求 | 公開方法 | 使用方式 |
| --- | --- | --- |
| 單筆套表，取得 DOCX 串流 | `_Word.TplRowToMs(tplPath, row, images)` | 傳入模型或 `JObject`。 |
| 多筆套表，取得一份 DOCX 串流 | `_Word.TplRowsToMs(tplPath, rows, images)` | 接受 `IEnumerable<dynamic>`。每筆複製範本本文後填值，再組成同一份文件。 |
| 在 HTTP 請求內下載單筆套表 | `await _HttpWord.OutputTplRowA(tplPath, fileName, row, images)` | 指定含 `.docx` 的下載檔名。 |
| 在 HTTP 請求內下載多筆套表 | `await _HttpWord.OutputTplRowsA(tplPath, fileName, rows, images)` | `rows` 必須是 `List<dynamic>`。既有模型集合可用 `rows.Cast<dynamic>().ToList()`。 |
| 合併既有 DOCX | `_Word.MergeFiles(srcFiles, toFile, deleteSrc)` | 依輸入順序合併，細節見下節。 |

來源見 [_Word.cs](../../../Services/_Word.cs) 與 [_HttpWord.cs](../../../Services/_HttpWord.cs)。

- 串流入口適合背景工作、儲存檔案，或由 Controller 自行回傳檔案結果。回傳型別為 `MemoryStream?`，成功時已定位到開頭。自行消費串流時用完即釋放。交給框架傳送時，讓框架在傳送完成後釋放。
- HTTP 入口透過 `_Fun2.ExportByStreamA` 直接寫入目前的 ASP.NET Core 回應。成功後結束 action，避免再寫另一份回應。回傳 `false` 的分支是套表回傳 `null`，而 `true` 代表匯出呼叫已完成。使用端依既有錯誤處理處理例外，因為範本缺失、文件格式錯誤或 I/O 失敗仍可能拋出例外。
- `tplPath` 使用應用程式解析出的有效 DOCX 範本路徑。多筆輸出若要求每筆固定另起一頁，先用實際範本驗證分頁。目前實作在加入第二筆及後續本文後才附加分頁符號。

## 準備範本及資料

- 主資料以 `[欄位名]` 對應模型轉成 JSON 後的鍵。標記必須完整位於同一個 Open XML `Text` 節點。若 Word 的格式編輯把標記拆成多個節點，請重建標記並確認 XML。
- 單個子表使用 `_child` 陣列。多個子表用 `_childs`，其每個元素是一個子表的資料陣列。索引從零開始，在範本本文的表格列放 `[!0]`、`[!1]` 等定位標記。子表欄位可用 `[欄位名]`，目前依第一筆子資料的鍵找包含該鍵的儲存格，再把值寫入對應儲存格。用空集合測試範本列是否符合輸出需求，因為空子表會跳過填值。
- 可省略 `images`。需要替換圖片時，用 [WordImageDto](../../../Models/WordImageDto.cs) 的 `Code` 對應行內圖片的描述屬性，`FilePath` 指向 PNG。現行替換沿用範本圖片尺寸。多筆套表目前在換上組合後的本文之前執行圖片替換，使用圖片時須用實際範本驗證輸出。

## 直接使用套表或圖片服務

- [WordSetSvc.cs](../../../Services/WordSetSvc.cs) 提供 `new WordSetSvc(tplPath)`、`IsOk()`、`RowToMs(row, images)` 與 `RowsToMs(rows, images)`。一般套表可用上表的 `_Word` 入口。需要在填值前判斷範本是否就緒時，可直接建立服務並檢查 `IsOk()`。找不到檔案時會記錄錯誤並維持 `false`，而損毀的文件或讀取失敗仍可能在建構時拋出例外。兩種輸出方法都會儲存並關閉文件，每次輸出建立一個新實例。
- [WordImageSvc.cs](../../../Services/WordImageSvc.cs) 可供自行組裝 Open XML 圖片的程式使用。呼叫 `new WordImageSvc(fileName, width, height)`，寬高以公分傳入，讀取 `WidthInEMU`、`HeightInEMU` 和產生的 `ImageName`。目前 `dpi` 參數不參與換算。成功時 `DataStream` 包含原始圖片位元組且已定位到開頭，用完由呼叫端釋放。檔案不存在時 `DataStream` 為 `null`，讀取或解碼失敗可能拋出例外。範本既有圖片的替換仍使用上節的 `WordImageDto`。

## 合併現有文件

`MergeFiles` 要求至少一個來源。使用與各來源不同的輸出路徑，既有目的檔會被覆寫。第一個來源是目的檔的基礎，其餘透過 Open XML `AltChunk` 加入，各追加文件前插入分頁符號。用實際開啟或轉檔的軟體確認它能解析 `AltChunk`。需要保留來源時，傳入 `deleteSrc: false`。傳入 `true` 會在目的文件儲存並關閉後逐一刪除來源。

整合時使用應用程式的實際範本與資料，確認欄位值、子表列、分頁及下載文件能開啟。使用合併或換圖時，另外核對該路徑的輸出。
