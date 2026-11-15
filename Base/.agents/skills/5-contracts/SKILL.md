---
name: 5-contracts
description: 在 Base 查找使用者資料、快取、PDF 或簡訊契約時，依檔案用途選擇並定位 C# 原始碼。
---

# Base 服務契約索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [IBaseUserSvc.cs](../../../Interfaces/IBaseUserSvc.cs) | 提供 Base 共用服務所需的目前使用者資料。 |
| [ICacheSvc.cs](../../../Interfaces/ICacheSvc.cs) | 依使用者與鍵操作字串快取。讀取用 `GetStr`，寫入用 `SetStr`，刪除用 `DeleteKey`。 |
| [IPdfSvc.cs](../../../Interfaces/IPdfSvc.cs) | Word 位元組轉 PDF 用 `WordToPdf`，將圖片加到 PDF 並寫入目的檔用 `AddImages`。 |
| [ISmsSvc.cs](../../../Interfaces/ISmsSvc.cs) | 依電話號碼與內容發送簡訊用 `SendA`，回傳 `Task<bool>`。 |
