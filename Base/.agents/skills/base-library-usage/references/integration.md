# 初始化與相依

本文件依建立 skill 時的工作區來源核對，未宣稱適用所有歷史版本。來源位置相對於大 Base 儲存庫根目錄，不相對於本文件所在的小 Base skill 目錄；文件連結則相對於本文件。外部應用沒有來源時，以下仍提供必要契約，新增用法需核對實際組件。

## 專案與執行環境

`Base/Base.csproj` 使用 `Microsoft.NET.Sdk`、目標 `net8.0`、啟用 nullable，並參照 `Microsoft.AspNetCore.App`。套件包含 Newtonsoft.Json、HtmlSanitizer、Handlebars.Net、記憶體快取、LDAP、ImageSharp、SkiaSharp、OleDb、DirectoryServices 與 System.Drawing.Common。實際套件版本以引用版本的 `.csproj` 為準；不能由 `net8.0` 推定繪圖、LDAP、OleDb 等功能可在所有平台執行。

有來源時可依應用現有專案配置加入指向 `Base/Base.csproj` 的 `ProjectReference`；路徑由應用位置決定。沒有來源時沿用已提供的 DLL／套件接入方式，不臆造套件名稱。參照程式庫不會自動載入 skill。

## 啟動順序與最小契約

來源：`Base/Services/_Fun.cs`、`Base/Models/ConfigDto.cs`、`Base/Enums/DbTypeEnum.cs`、`Base/Enums/AuthTypeEnum.cs`。

1. 應用先建立或載入 `ConfigDto`，指派 `_Fun.Config`。它預設為 `null!`；`Init` 直接讀取它，不會自行讀取 appsettings。
2. 建立所需 DI 服務，取得適合應用生命週期的 `IServiceProvider`。
3. 呼叫以下實際簽章，檢查回傳錯誤字串；空字串才代表初始化成功：

```csharp
public static string Init(bool isDev, IServiceProvider? diBox,
    DbTypeEnum dbType, AuthTypeEnum authType, bool multiLang = false)
```

`DbTypeEnum` 有 `None`、`MSSql`、`MySql`、`Oracle`；`AuthTypeEnum` 有 `None`、`Ctrl`、`Action`、`Row`。按應用既有需求選擇，不為讓範例通過而關閉授權。MySQL／Oracle 初始化分支仍有刪除 SQL 與測試 TODO，列舉存在不表示 CRUD 完整相容。

下例只展示初始化契約，`config`、`serviceProvider`、`dbType`、`authType`、`isDevelopment` 都必須由應用提供：

```csharp
using Base.Services;

_Fun.Config = config;
string error = _Fun.Init(isDevelopment, serviceProvider, dbType, authType);
if (error.Length != 0)
    throw new InvalidOperationException(error);
```

`Init` 設定全域模式、DB 類型、授權、語系與 SQL 格式；不驗證 DI 完整性、不測試資料庫連線。`diBox` 雖允許 null，但 DB、使用者、快取等功能後續會直接使用它。`Config.Encode=true` 時會透過 `_Str.ReadFileKey()` 解密敏感設定，並原地改寫 `Config`，不要在每次請求重複初始化。SMTP 非空時預期七個逗號分隔欄位：Host、Port、Ssl、Id、Pwd、FromEmail、FromName。

## 按功能準備 DI

| 使用功能 | 已確認需求與限制 | 來源 |
| --- | --- | --- |
| `Db`／CRUD | 以 `DbConnection` 與 `DbCommand` 類型從 `_Fun.DiBox.GetService` 取得實例；應用提供符合 DB 類型的 provider 與註冊，Base 未直接提供 SqlClient/MySQL/Oracle provider | `Base/Services/Db.cs` 的 `InitDbA`、`InitCmdA` |
| 目前使用者、稽核、權限 | 註冊 `IBaseUserSvc`，其 `GetData()` 回傳 `BaseUserDto`；`GetBaseUser()` 不會在服務缺失時自動建立實例 | `Base/Interfaces/IBaseUserSvc.cs`、`Base/Services/_Fun.cs` |
| 記憶體快取 | `_Cache` 要求 `ICacheSvc`；可用 `CacheMemSvc(IMemoryCache cache)`，另備妥 `IMemoryCache` | `Base/Services/_Cache.cs`、`CacheMemSvc.cs` |

`Db.DisposeAsync` 會釋放 command 與 connection；不要讓不同並行操作共用同一個會被釋放的實例。DI 生命週期應依應用核對，提供每個 `Db` 操作可獨立使用的連線／命令，不能只給一個全域 singleton。

`BaseUserSvc` 內建實作回傳新的空白使用者資料，不是登入整合。Web 應用需提供能正確取得目前使用者的實作；`_Fun.DiBox` 為靜態 provider，勿把請求 scoped 服務直接從根 provider 解析的問題藏在範例中。Base 未在這裡提供完整的請求 scope 接線方案。

`CacheMemSvc` 使用 `userId + "_" + key` 作為鍵，過期採 `_Fun.TimeOut` 分鐘的滑動期限。`ResetDbA` 會對實際 `MemoryCache` 執行 `Compact(1.0)`，不是只清目前使用者的一個鍵。設定 `Config.Redis` 不會自動註冊 Redis 實作。

其他限制：`_Fun.DirRoot` 由應用基底目錄與 `bin` 路徑推導，相關 `_data`、`_image`、`_draft`、`_temp` 位置須依部署核對；不能假設就是應用目前工作目錄。金鑰檔與加密部署慣例尚未做端到端驗證。
