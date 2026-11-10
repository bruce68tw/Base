# BaseApi 架構入口

依 2026-11-03 工作區來源核對。既有 [baseapi-overview](../.agents/skills/00-overview/SKILL.md) 與 references 是詳細整合入口。

## 責任與相依

[BaseApi.csproj](../BaseApi.csproj) 是參照 [Base](../../Base/docs/architecture.md) 的 `net8.0` 類別庫，提供 MVC 控制器基底、驗證授權、HTTP／檔案與 Xp 服務。沒有 Program.cs，也不代替應用啟動、設定載入或 middleware。

| 來源入口 | 說明 |
| --- | --- |
| [BaseCtrl.cs](../Controllers/BaseCtrl.cs) | 回應與檔案 helper；繼承它不會自動套用登入或授權；[控制器與驗證授權](../.agents/skills/00-overview/references/controllers-and-auth.md) |
| [XgLoginAttribute.cs](../Attributes/XgLoginAttribute.cs)、[Attributes](../Attributes) | 登入與權限 filters；XgLogin 未登入時導向 Home/Login，不是指令路由白名單 |
| [_Http.cs](../Services/_Http.cs)、[_Fun2.cs](../Services/_Fun2.cs) | HttpContext、Cookie／使用者資料與 Web 路徑；[初始化與目前使用者](../.agents/skills/00-overview/references/integration.md) |
| [Services](../Services) | 上下載、語系與 Xp 等應用 schema 服務；[服務與整合檢查](../.agents/skills/00-overview/references/services-and-validation.md) |

使用端依需求接上 IHttpContextAccessor、IBaseUserSvc、ICacheSvc 及 DB provider；不要全面啟用無需的 Session／JWT。HTTP 工具需要有效請求 context，不可把某次請求快取到 singleton。JWT 讀取不等於驗證，登入通過也不等於允許執行任意模型產生的 Ctrl／Action。

## 維護與驗證

從此目錄執行 `dotnet build BaseApi.csproj --no-restore -m:1`，依賴 Base 與可用還原資料。來源以 csproj 為準，`.cs_old` 等不視為現行 API。初始化、登入與資料表細節按既有 references 查證，應用 schema 不寫入共用契約。

本次文件任務核對相依、Controller／登入 filter 與文件入口，未驗證實際登入、JWT、DB 或應用執行流程；勿把文件範例當成已通過整合測試。
