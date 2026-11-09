# 工具選擇與驗證

## 按問題查找 API

下列是 Base 程式庫內的查證入口，來源位置相對於大 Base 儲存庫根目錄，不相對於本文件所在的小 Base skill 目錄；文件連結則相對於本文件。這不是要求應用具備來源。只有組件時，核對該組件公開簽章後再產生呼叫，不憑類名推測方法名稱。

| 工作 | 來源與注意事項 |
| --- | --- |
| 字串、日期、數值、檔案 | `Base/Services/_Str.cs`、`_Date.cs`、`_Num.cs`、`_File.cs`；依實際方法核對空值、格式與副作用，別假設都不依賴 `_Fun` |
| JSON | `Base/Services/_Json.cs` 使用 Newtonsoft `JObject`／`JArray`，與 System.Text.Json 的 JsonObject 不可直接互換 |
| 模型序列化 | `Base/Services/_Model.cs`：`ToJsonStr<T>` 使用 JsonConvert；`ToJson<T>` 與 `Copy<TFrom,TTo>` 走公開屬性反射，不是公開欄位的通用深拷貝，且 `ToJson` 將值轉為字串 |
| 結果與驗證錯誤 | `Base/Services/_Result.cs`：`HasError(ResultDto)` 檢查 `_ErrorMsg` 非空或 `ErrorRows.Count>0`；`Base/Models/ResultDto.cs` 的 Value、Code 等是公開欄位，注意應用序列化器設定 |
| 快取與目前使用者 | 見 [初始化與相依](integration.md)，不要把快取介面或空白使用者實作當成已完成登入／Redis 整合 |
| SQL 與 CRUD | 見 [資料存取與 CRUD](data-access.md)，再按需核對 `_Sql.cs`、相關模型與委派 `FnDelegate.cs` |

`Base/Base.csproj` 明確排除 `Services/_Office.cs`、`Services/_WordSet.cs` 等檔案；存在於磁碟不等於可從 Base 組件呼叫。Office、PDF、Redis provider、HTTP/MVC 與 AI 的專屬實作應另查其模組，不把歷史檔案當作補缺的 API。

## 依使用情境驗證

- **新應用引用 Base**：確認引用解析到預期版本、應用目標框架與 framework reference 可滿足；檢查設定先於 Init，錯誤字串有處理；對實際應用 `.csproj` 執行 `dotnet build <應用專案路徑>`。路徑由工作區決定；若只驗證 Base 本身，有來源時可建置 `Base/Base.csproj`。
- **只有工具／JSON 或記憶體快取**：可用不連外的短測試核對結果判讀、欄位序列化、快取使用者隔離與刪除；快取需備妥 IMemoryCache／ICacheSvc，勿啟用全庫清除作一般驗證。
- **資料庫唯讀接入**：在已備妥的測試 DB 核對 provider DI、連線、參數查詢、查無資料、錯誤回傳及連線釋放；不要用正式資料異動來探測 schema。
- **CRUD 儲存**：在應用測試資料庫確認必填驗證、主鍵產生、無稽核欄位情況、主從外鍵、失敗 rollback 與權限範圍。只有設定檔或編譯成功，不足以證明這些行為已驗證。

此庫尚未找到正式自動測試專案；`Base/Services/_Test.cs` 不是測試執行器。若遇套件還原、環境、provider 或測試 DB 缺失，報告實際失敗與未驗證層級，不把文件範例說成已成功執行。

## 維護邊界

以上已核對原始碼簽章與部分儲存庫內用例；未確認對外 NuGet 發佈資訊、所有平台相容性、MySQL／Oracle 全部 CRUD、加密部署及外部應用實際登入整合。更新 skill 時依相符來源修正，不以本機暫存的 DLL 或單次測試結果推論所有版本。不要將使用者工作區中尚未提交的修改視為已發佈契約。
