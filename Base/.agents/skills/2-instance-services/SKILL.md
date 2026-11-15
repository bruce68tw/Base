---
name: 2-instance-services
description: 在 Base 查找CRUD、資料庫、快取或其他共用服務時，依檔案用途選擇並定位 C# 原始碼。
---

# Base 實例服務索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [BaseEditSvc.cs](../../../Services/BaseEditSvc.cs) | 以 GetDto 提供設定，延遲建立讀取與異動服務的 CRUD 包裝基底。 |
| [BaseUserSvc.cs](../../../Services/BaseUserSvc.cs) | 提供空白使用者資料的 IBaseUserSvc 實作。 |
| [CacheMemSvc.cs](../../../Services/CacheMemSvc.cs) | 以 userId_鍵 存取 IMemoryCache 的實作。 |
| [CrudEditGetSvc.cs](../../../Services/CrudEditGetSvc.cs) | 依 EditDto 讀取單筆主從資料，組成 _rows 與 _childs JSON。 |
| [CrudEditSvc.cs](../../../Services/CrudEditSvc.cs) | 依 EditDto 驗證並儲存或刪除主從資料，依設定與子表狀態管理交易。 |
| [CrudGetSvc.cs](../../../Services/CrudGetSvc.cs) | 以 EditDto 讀取編輯、檢視及草稿所需的主從資料。 |
| [CrudReadSvc.cs](../../../Services/CrudReadSvc.cs) | 依 ReadDto 組合查詢條件、分頁與資料存取範圍。 |
| [Db.cs](../../../Services/Db.cs) | 以 DI 提供的 DbConnection 與 DbCommand 管理連線、執行查詢和異動及處理交易。 |
| [FnDelegate.cs](../../../Services/FnDelegate.cs) | 宣告 CRUD 驗證、儲存前後、主鍵設定、匯入檢查與儲存、簽核後處理的回呼委派。 |
| [GenImageSvc.cs](../../../Services/GenImageSvc.cs) | 使用 SkiaSharp 在記憶體繪圖並輸出 PNG；尺寸以像素表示。 |
| [WebSocketSvc.cs](../../../Services/WebSocketSvc.cs) | 管理單一伺服端或用戶端 WebSocket，提供完整訊息接收及序列化傳送；使用後需釋放。 |
