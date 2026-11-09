---
name: model-read-dto
description: 在 Base 系列 MVC 查詢功能撰寫 Read Service 時，閱讀 ReadDto 及其註解，套用使用者確認的 ReadSql 選用、SQL 與 DataTable 欄位對應及虛擬欄位規則。提供跨功能 Model 說明入口，不取代模組 library-usage 或 CRUD 開發流程。
---

# Model 說明—ReadDto

本 skill 是大 Base 跨功能的查詢 Model 說明入口。開發流程由使用者設計，AI 閱讀類別與既有註解理解設定；屬性不清楚時先討論確認，再補來源註解，不在此複製整份屬性文件。CRUD 查詢的需求確認、生成與備份流程見 [mvc-crud-query](../../../../.agents/skills/mvc-crud-query/SKILL.md)。各子模組的 library-usage 仍維持原位置與職責。

本指引聚焦 `ReadDto`。`Items` 的元素型別與條件閱讀入口見 [QitemDto](../model-QitemDto/SKILL.md)，產生 Read Service 前須一併閱讀。完整屬性留待逐欄討論。

## Read Service 撰寫前閱讀（使用者已確認）

產生 Read Service 前，先依 [Base library-usage](../library-usage/SKILL.md) 的資料存取導引，閱讀 Base 的 `Models/ReadDto.cs` 及既有註解；來源路徑相對於 `Base/Base.csproj` 所在目錄。由類別與註解理解設定，不在本 skill 複製整份屬性文件；不清楚的屬性先討論，再補充來源註解。

## Read Service 設定（使用者指定撰寫原則）

- `ReadSql` 必填，無論單表查詢或 Join 其他資料表皆須設定；包含 SELECT、所需 joins 與預設排序。結果資料欄位由 SQL SELECT／alias 決定，畫面查詢條件由共用服務組合。
- 依程式設計師答覆判斷是否需 Join 其他 table；無 Join 時可直接寫單表 SQL，例如 `SELECT * FROM <TableName>`（以實際表名替換），仍須填寫 `ReadSql`，不強制所有單表查詢使用 `SELECT *`。
- `Items` 僅描述畫面的查詢條件欄位，不是 SQL 來源或結果欄位清單；可以為 null 或空陣列，表示畫面沒有查詢條件。

## 查詢欄位一致性（使用者已確認）

新增或修改查詢功能時，同步核對畫面查詢欄位、後端查詢條件、SQL 回傳欄位與前端列表設定，確保名稱及資料意義一致。一般資料欄的回傳名稱／alias 須對應 TypeScript jQuery DataTable 的欄位設定。

前端自行 render 的虛擬欄位以 `_` 開頭，SQL 不必輸出同名欄位，但 render 依賴的資料仍須由 SQL 提供。一般資料欄也可用 render 格式化，不因此改用 `_` 前綴；不能要求所有前端欄位都必須出現在 SELECT。


列表操作欄亦屬上述底線開頭的前端虛擬 render 欄，SQL 不必輸出同名欄位，但操作所需識別值必須由資料提供。沿用目標功能的實際操作欄名稱，不硬訂新名稱；現行 DbAdm Column 使用 `_Fun`，不是 `_Crud`。
