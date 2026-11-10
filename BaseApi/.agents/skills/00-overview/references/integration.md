# 初始化與目前使用者

來源：`BaseApi/BaseApi.csproj`、`BaseApi/Services/_Http.cs`、`_Fun2.cs`，以及 `Base/Services/_Fun.cs`、`_Cache.cs`、`CacheMemSvc.cs`、`Db.cs`、`Base/Interfaces/IBaseUserSvc.cs`。本文件描述建立時來源的契約；不同版本以實際引用為準。

## 接入與初始化

BaseApi 直接參照 Base；套件包含 ASP.NET Core Html.Abstractions、Mvc.Core、Mvc.ViewFeatures（各 2.3.10）、Otp.NET 1.4.1、QRCoder 1.8.0、System.IdentityModel.Tokens.Jwt 8.18.0。目標為 net8.0 不表示上述套件是當代完整宿主配置；應用仍需自己的 ASP.NET Core 啟動、MVC 路由與所需 middleware。BaseApi 不提供 Program.cs 或統一 AddBaseApi 註冊入口。

啟動時先給 `_Fun.Config` 指派應用載入的 `Base.Models.ConfigDto`，再呼叫：

```csharp
// 變數由應用的設定、DI 與執行環境提供。
Base.Services._Fun.Config = config;
string error = Base.Services._Fun.Init(
    isDevelopment, serviceProvider, dbType, authType, multiLang);
if (error.Length != 0)
    throw new InvalidOperationException(error);
```

實際簽章是 `Init(bool isDev, IServiceProvider? diBox, DbTypeEnum dbType, AuthTypeEnum authType, bool multiLang = false)`，成功為空字串，不會自行讀 appsettings、測試 DB 連線或確認 DI 完整。設定解密會原地改寫 Config，勿重複初始化。`_Fun2.DirWeb` 在靜態初始化時由 `_Fun.DirRoot + "wwwroot/"` 是否存在決定，否則使用 DirRoot；先初始化 Base，再使用相關工具，部署時核對實際資料目錄。

## 按功能提供服務

| 功能 | 應用端責任 |
| --- | --- |
| `_Http.GetHttp()` 與依賴它的功能 | 註冊 `IHttpContextAccessor`（通常用 `AddHttpContextAccessor()`）；`_Fun.DiBox` 必須能解析它，呼叫當下需有 HttpContext。方法直接使用 null-forgiving，不會優雅回傳缺少請求的錯誤。 |
| `_Fun.GetBaseUser()`、權限與稽核 | 提供 `IBaseUserSvc.GetData()`，回傳目前請求的 `BaseUserDto`。Base 內建 BaseUserSvc 回傳空白模型；BaseApi 的 XgUserSvc 是查詢使用者資料的服務，並非此介面的登入實作。 |
| Cookie／JWT 使用者快取 | 註冊 `ICacheSvc`；可用 `CacheMemSvc` 並提供 IMemoryCache。多執行個體須核對快取共享方式，僅填 Config.Redis 不會自動註冊實作。 |
| DB、登入查詢、Xp 服務 | 提供相符 DbConnection、DbCommand 的 DI 註冊、連線設定及 schema；每個 Db 操作須能獨立使用與釋放，避免共用全域 connection。 |
| `_Http.GetSession()`、SessionExtension | 應用提供 Session 服務及 middleware；CookieToBr 使用快取，不表示所有功能都要求 Session。 |

`_Fun.DiBox` 是靜態 provider。不要以從根 provider 直接解析 scoped 使用者服務的範例掩蓋生命週期問題；依應用設計讓使用者服務從目前 HttpContext 取得資料，或使用符合現有請求 scope 的橋接方式。不要快取某次請求的 HttpContext 到 singleton 欄位。

## 目前使用者橋接

應用的 IBaseUserSvc 可依已選定的身份方案呼叫 `_Http.CookieToBr()` 或 `_Http.JwtToBr()`，不應將兩者任意混用。Cookie 模式讀 `_Fun.FidClientKey`（`_jwt`），但值為登入時產生的隨機 clientKey，並非 JWT。

兩條路徑最後用 `UserKeyIp(key) = key + GetIp(false)` 查快取中的 `_BaseUser`；GetIp(false) 只移除 RemoteIpAddress 字串的句點。結果存於同一請求 HttpContext.Items 的 `_BaseUser`，之後直接重用。IP 改變、代理配置不同、快取逾期或多節點快取不一致都會影響結果。快取 miss 的回傳沒有在 `_Http` 內補空模型；應用的橋接需核對實際 ICacheSvc 與登入失效處理。

反向代理情境應確認可信代理與 forwarded headers 在讀取 RemoteIpAddress 前已正確處理。不要以客戶端自行傳入的 IP 字串直接取代身份判定。`BaseApi/Extensions/SessionExtension.cs` 的 `Set<T>(ISession session, string key, T value)`／`Get<T>(ISession session, string key)` 以 JSON 序列化物件；Get 在鍵不存在時回傳 default。
