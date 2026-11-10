# Base 架構入口

依 2026-11-03 工作區來源核對；詳細整合契約沿用既有 [base-overview](../.agents/skills/00-overview/SKILL.md)，不複製另一套 API 文件。

## 責任與邊界

[Base.csproj](../Base.csproj) 是 `net8.0` 類別庫，包含資料模型、介面、初始化、工具、DB 與 CRUD；ASP.NET framework reference 與平台相關套件以 csproj 為準。HTTP／MVC 專用工具由 BaseApi 負責，AI 由 BaseAI 負責，具體資料庫 provider 與應用啟動由使用端提供。

| 來源入口 | 責任／應讀文件 |
| --- | --- |
| [_Fun.cs](../Services/_Fun.cs)、[ConfigDto.cs](../Models/ConfigDto.cs) | 全域設定、DI provider 與初始化；[初始化與相依](../.agents/skills/00-overview/references/integration.md) |
| [Db.cs](../Services/Db.cs)、[CrudReadSvc.cs](../Services/CrudReadSvc.cs)、[CrudEditSvc.cs](../Services/CrudEditSvc.cs) | DB 操作與 CRUD；[資料存取](../.agents/skills/00-overview/references/data-access.md) |
| [_Cache.cs](../Services/_Cache.cs)、[CacheMemSvc.cs](../Services/CacheMemSvc.cs)、[IBaseUserSvc.cs](../Interfaces/IBaseUserSvc.cs) | 快取與目前使用者介面，實作與生命週期由應用接線 |
| [_Str.cs](../Services/_Str.cs)、[_Model.cs](../Services/_Model.cs)、[_File.cs](../Services/_File.cs)、[_Log.cs](../Services/_Log.cs) | 字串、模型、檔案與日誌；[工具與驗證](../.agents/skills/00-overview/references/api-and-validation.md) |

應用先指派 `_Fun.Config`，註冊所需 DI，再呼叫 `_Fun.Init`；它是處理程序共用狀態，不在每次請求重設。依實際 Init 回傳契約處理錯誤；初始化不等同 DB 可連線或權限已驗證。不要把空白使用者實作當成完成登入。

`_Fun.DirRoot` 由應用基底目錄與 bin 路徑推導，不保證等於 ContentRootPath 或工作目錄。`_File.ToStr(path, logError)` 缺檔可記錄錯誤並回傳 null，存在時以 UTF-8 讀取；不自行拒絕空白檔，其他 I/O 例外不在此方法轉成 null。呼叫端決定 fallback／啟動失敗政策。不得將所有檔案 API 都推論成同樣契約。

## 維護與驗證

從此目錄執行 `dotnet build Base.csproj --no-restore -m:1`，前提是還原資料可用。實際編譯來源依 csproj 的 `Compile Remove`，略過舊版及被排除檔案。DB／登入整合另需使用端環境，不以建置成功替代功能測試。

本次僅核對文件、初始化／檔案契約與來源位置，未全面重審工具 API 或執行 DB／外部服務。工作區未提交的修改不代表發佈版本；具體方法行為仍需按任務讀取來源。
