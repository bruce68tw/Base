# 資料存取與 CRUD

先依 [初始化與相依](integration.md) 備妥設定、DI 與 DB provider。文件連結相對於本文件；以下來源路徑相對於大 Base 儲存庫根目錄，不相對於本文件所在的小 Base skill 目錄。沒有來源時，先核對應用所引用版本的 API。

## 直接查詢與交易

來源：`Base/Services/Db.cs`、`Base/Services/_Db.cs`。

- `new Db(string dbStr = "")`：空值使用 `_Fun.Config.Db`。`InitDbA()` 延遲開啟連線並回傳 bool；一般命令會自動呼叫它。
- `GetRowA(string sql, List<object>? sqlArgs = null)` 回傳 `Task<JObject?>`；`GetRowsA` 回傳 `Task<JArray?>`；另有 `GetStrA`、`GetIntA`、`GetModelA<T>`、`GetModelsA<T>`。
- 參數清單是「不含 @ 的名稱、值」交錯配對，不是字典，也不是單純值清單。實作為名稱加上 `@`，null 轉 `DBNull.Value`，DateTime 保留型別，其他值轉字串。數值、二進位與各 provider 的型別推導須另驗證。
- 空結果與讀取失敗可能都回傳 null，不能只用 null 判定資料不存在；依應用錯誤處理與記錄進一步判讀。

以下為唯讀呼叫形狀；`dbo.Products`、`Id`、`Name` 是示意 schema，先改成應用實際資料表並確認權限：

```csharp
using Base.Services;
using System.Collections.Generic;

await using var db = new Db();
var row = await db.GetRowA(
    "select Id, Name from dbo.Products where Id=@Id",
    new List<object> { "Id", productId });
```

SQL 值透過參數傳入。表名、欄名、排序等 SQL 結構無法用此參數機制保護，須由可信設定或白名單產生；不要插入未檢查的前端字串。OleDb 註解明示不支援具名參數，勿直接視為可用 provider。

`ExecSqlA(string sql, List<object>? sqlArgs = null)` 回傳受影響筆數；一般 DB 例外為 `-1`、唯一鍵例外可能為 `-2`，初始化失敗實作回傳 `0`。不要將未丟例外或 `0` 當作寫入成功。唯一鍵判斷使用 provider 例外的 `Number`，跨 provider 行為須驗證。

手動交易 API 為 `Task<bool> BeginTranA()`、`Task CommitA()`、`Task RollbackA()`；須在同一個 `Db` 實例執行。先確認 Begin 成功，再根據命令結果決定 commit 或 rollback，不能假設回傳錯誤碼會自動擲回例外。`await using` 負責釋放，不等於應用已成功提交交易。

`_Db` 是便利包裝，多數常用查詢為 static：未傳 `Db` 時自行建立／關閉，傳入既有 `Db` 時沿用它。需共享交易時明確傳同一實例；不要假設每個 `_Db` 成員都是 static。

## 列表查詢

來源：`Base/Services/CrudReadSvc.cs`、`Base/Models/ReadDto.cs`、`DtDto.cs`、`EasyDtDto.cs`、`QitemDto.cs`。

`new CrudReadSvc().GetPageA(ReadDto readDto, DtDto dtDto, string ctrl = "")` 回傳 `Task<JObject?>`。核心設定是 `ReadDto.ReadSql` 和 `Items`；條件項目以 `QitemDto.Fid`、`QitemOpEstr` 等描述。`ReadSql` 的選取欄位順序須與客戶端排序一致；`UseSquare` 是此庫的 SQL 區段標記模式，不是一般 SQL 字串自動轉換功能。

回傳實作包含 `data` 與 `recordsFiltered`，不能假設有完整 DataTables 所有標準欄位。前端傳入的 `sort` 在實作中會直接構成 Order By，因此應用需先限制允許的排序欄位與分頁範圍。

既有使用例 `BaseApi/Services/XgProgRead.cs` 以 `ReadDto { ReadSql=..., Items=... }` 呼叫 `new CrudReadSvc().GetPageA(dto, dt, ctrl)`。它只作為 Base API 的使用證據，不表示應用必須依賴 BaseApi 或建立 `XpProg` 資料表。

## 編輯、主從表與結果

來源：`Base/Services/BaseEditSvc.cs`、`CrudEditSvc.cs`、`CrudGetSvc.cs`、`CrudEditGetSvc.cs`；模型為 `Base/Models/EditDto.cs`、`EitemDto.cs`、`ResultDto.cs`。

常見做法是繼承 `BaseEditSvc(string ctrl, bool hasDraft = false)`，實作 `EditDto GetDto()`，再使用 `CreateA(JObject)`、`UpdateA(string key, JObject)`、`DeleteA(string key)`（皆回傳 `Task<ResultDto>`）；讀取使用 `GetUpdJsonA` 或 `GetViewJsonA`。

| 設定 | 整合時須確認 |
| --- | --- |
| `Table`、`PkeyFid`、`Items` | 使用實際資料表、主鍵、可寫欄位；`Items` 不能漏成 null |
| `AutoIdLen` | 預設 `_Fun.AutoIdMid`；設 0 表示允許前端傳入新 Id，不能誤認預設為 DB identity |
| `Col4` | 預設 Creator、Created、Reviser、Revised；實際表沒有時設 null，不會自動建立欄位 |
| `EitemDto` | `Required` 控制必填；`Read=true` 跳過新增／修改；`Create`、`Update` 控制可異動方向 |
| `EmptyToNulls` | 指定空值需轉成 DB null 的欄位，例如日期 |
| `Childs`、`FkeyFid` | 描述主從表；child 外鍵不能設 Required；依實際關聯核對多層讀取 SQL |
| `AutoTrans` | 預設 true，但實作還依有無 child 決定是否自動開交易，不能宣稱每次儲存必有交易 |
| 回呼 | `FnWhenSaveA` 在交易前；`FnAfterSaveA` 在儲存後、提交前，須核對當次是否確有交易 |

`CreateA` 使用 `_Json.GetRows0(json)` 取得首列；輸入不是直接的單列 JObject，最小形狀是：

```json
{"_rows":[{"Id":"example-id","Name":"範例"}]}
```

Id 是否提供依 `AutoIdLen` 和主鍵策略決定。主從表、刪除欄位另使用 `_childs`、`_deletes` 等固定鍵，必須核對實際層級與編輯服務，勿自行拼接未驗證的完整 payload。範例 `BaseApi/Services/XgProgEdit.cs` 展示 `BaseEditSvc`、`Col4=null` 與 `Childs` 設定；不代表其 schema 適用其他應用。

儲存後用 `_Result.HasError(result)` 判讀 `_ErrorMsg` 與 `ErrorRows`，不能只看 `Value` 或 HTTP 狀態碼。授權模式與目前使用者另有前提，`AuthTypeEnum.Row` 的條件／使用者欄位需依應用設定；不能把庫內行為視為已完成應用所有入口的授權。
