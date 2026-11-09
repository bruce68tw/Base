# 向量與上下文

來源：`Interfaces/AbEmbedDbSvc.cs`、`Services/ChromaSvc.cs`、`Models/EmbedRowDto.cs`、`Services/CengSvc.cs`、`Services/CengSvcTest.cs`。

## Chroma

`ChromaSvc(string embedDbStr, string embedTable, int embedDim, double maxDistance, int queryRows)` 使用 HTTP API；embedDbStr 是 collection API 根路徑，不能直接假定只給主機位址即可。

- CreateA/UpdateA 參數為 `(table, id, vector, fileId)`；送出 ids、embeddings、metadata.FileId，沒有寫入 documents。GetA 雖可解析 Text，也不代表本庫寫入後就一定有文字。
- GetA、DeleteA 為 `(table, id)`；DeleteByFileIdA 為 `(table, fileId)`；VectorToIdsA 為 `(table, vector, maxDistance = 0, queryRows = 0)`，回傳 ID 清單，不是文件內容或相似度分數。
- SetNowTableA 先 GET `{embedDbStr}/{table}`，404 時 POST 根路徑建立 cosine collection；後續使用回傳 id 組合 `{embedDbStr}/{collectionId}/{action}`。讀取與查詢也可能觸發建立 collection。
- 基底註解說空 table 使用預設名稱，但 ChromaSvc 目前沒有這項 fallback。請明確傳入非空 table；不要以註解代替實作。
- 查詢以 `distance <= maxDistance` 篩選，距離越小越近。maxDistance=0 改用建構預設；queryRows>0 才覆蓋預設筆數。明確選擇合理預設，不能將零解讀為不限距離。
- 維度檢查目前註解停用，呼叫端需確認向量長度。`_nowTable`/`_nowTableId` 是可變快取，跨 collection 並行共用實例可能混用目標；依應用生命週期隔離或在修改範圍內處理同步。
- VectorToTextA、ClearA 為註解內容，不是可呼叫 API。CRUD 失敗多回傳 false/null/空集合並記錄日誌，仍需核對各方法未捕捉的例外。

整合流程為：產生並檢查 embedding → 明確指定 table 寫入 → VectorToIdsA 取得 ID → 從應用自己的文件來源取回文字 → 組裝上下文。不要把 EmbedFindResultDto.Score 當成 Chroma 查詢實際回傳欄位。

## 上下文管線的實際能力

`CengSvc.cs` 雖檔名含 Svc，並沒有 CengSvc 類別；公開入口是同檔中的 ContextEnginePipeline。它透過 IRetriever、IReRanker、IMemory、ICompressor、ITokenBudget 組合。

`BuildContextAsync(query, maxTokenBudget)` 依序檢索 topK=5、重排、將短期記憶置前、每份文件壓縮到傳入 maxTokens=500，再依預算收納。超預算時 FitToBudget 直接 break，不繼續尋找後面的短文件。

- MockRetriever、SimpleReRanker 是模擬／簡化實作，不能當成 Chroma 或 cross-encoder 整合。
- InMemoryStore 是實例內 List，不是持久儲存或已隔離使用者的記憶服務。
- BasicCompressor 只是字元截斷；TokenBudgetManager 以字元長度粗估，不能保證符合任何實際 tokenizer。
- 壓縮會改寫 Document.Content；如果文件物件來自共用記憶或快取，要確認是否需要複製。
- ContextPacket.Compile() 以分隔符串接 Items.Content，沒有納入 SystemPrompt；需由應用另外組裝。
- CengSvcTest.cs 是示範來源，並非正式自動測試專案。需要實際 RAG 時提供適當的介面實作並驗證檢索及預算結果。
