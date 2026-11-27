# 小 Base 實例服務

資料更新日期：2026-11-27

| 類別名稱 | 用途說明 |
| --- | --- |
| [BaseUserSvc](../Services/BaseUserSvc.cs) | IBaseUserSvc 的空白使用者實作，不處理登入或請求身分。 |
| [Db](../Services/Db.cs) | 以 DI 提供的 DbConnection、DbCommand 查詢資料、執行 SQL 與管理交易；實作 IAsyncDisposable。 |

`Db` 使用前須提供全域設定及資料庫 DI，使用後以 `await using` 或 `DisposeAsync` 釋放。`BaseUserSvc` 僅回傳空白使用者資料，不負責登入驗證。
