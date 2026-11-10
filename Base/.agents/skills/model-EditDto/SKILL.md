---
name: model-edit-dto
description: 在 Base 系列 MVC 功能涉及編輯時，閱讀 EditDto 及直接相關來源與註解，理解維護設定與型別關係。提供跨功能 Model 說明入口；詳細屬性規則待逐項討論，不取代模組總覽 或 CRUD 開發流程。
---

# Model 說明—EditDto

本 skill 是大 Base 跨功能的維護 Model 說明初稿，不是 CRUD 開發流程。流程由使用者設計，AI 閱讀類別與既有註解理解；屬性不清楚時先討論確認，再補來源註解，不在此複製整份 API 文件。需求訪談、摘要確認與備份流程見 [mvc-crud-query](../../../../.agents/skills/mvc-crud-query/SKILL.md)。

## 撰寫前閱讀

涉及編輯時，先讀 [base-overview](../00-overview/SKILL.md) 的資料存取與 CRUD 導引，再閱讀以下來源與既有註解。來源路徑相對於 `Base/Base.csproj` 所在目錄；具體契約由模組維護。

## Model 責任與關係（目前來源觀察）

- `Models/EditDto.cs`：主表或子表的讀寫設定，包含欄位設定、下一層子表、回呼及流程暫存狀態。
- `Items` 的元素型別及欄位設定閱讀入口見 [EitemDto](../model-EitemDto/SKILL.md)，涉及編輯時須一併閱讀。
- `EditDto.Childs` 宣告為 `EditDto[]?`，以同一 Model 描述下一層子表；無子表時預設 null。這是型別關係，不代表本功能必須支援主從表。
- `Models/ResultDto.cs`、`Models/ErrorRowDto.cs`：維護回傳結果及欄位錯誤資料；按實際使用的回傳與驗證路徑閱讀。
- `Services/FnDelegate.cs`：EditDto 所用回呼的委派宣告，並非另一份欄位 Model；只有需求涉及對應回呼時才讀取。

`Services/BaseEditSvc.cs` 透過 `GetDto()` 取得 EditDto，將單筆讀取與異動分別委派給共用服務。此處只提供設定串接入口，詳細維護實作尚待整理，不據方法清單擴增需求操作。

上述內容是目前類別及既有註解的觀察，完整屬性設定原則留待使用者逐欄確認，不將預設值或範例選擇直接升為通用要求。

## ReadSql（使用者已確認）

`EditDto.ReadSql` 非必填：單筆讀取需要 Join 其他資料表時設定；無 Join 且未啟用資料權限的單表讀取可省略，由系統依 `Items` 組出欄位清單。啟用資料權限時依下節例外設定。此規則僅適用 EditDto；列表查詢的 `ReadDto.ReadSql` 仍為必填。

## 資料權限（使用者已確認）

使用者確認此功能需考慮操作者的資料權限時，`EditDto` 必須自訂 `ReadSql` 並關聯 `XpUser`，不是由 AI 自由判斷是否需要 Join。User ID 來源欄位與關聯路徑使用流程中已確認的答案，不固定為 Creator，也不沿用範例的固定路徑。

讀出 `u.Id`、`u.DeptId` 時，須使用共用權限欄位 alias `_Fun.FidUser`、`_Fun.FidDept`，不能只取一般別名。參考 DbAdm 的 `Services/ColumnEdit.cs`，插值 SQL 的 SELECT 片段為：

```csharp
{_Fun.FidUser}=u.Id, {_Fun.FidDept}=u.DeptId
```

其中 `u` 為 XpUser 的 SQL 別名；其關聯來源依使用者確認內容設定。此為一般單表可省略 ReadSql 規則的明確例外，不改變 `ReadDto.ReadSql` 必填規則，也不增加未確認的操作。

## 已確認的畫面範圍

新增、修改、單筆檢視共用同一份編輯 View，但不預設三種操作全部啟用；依已確認的功能需求使用。

## 參考與界線

DbAdm 的 `Services/ColumnEdit.cs` 是 EditDto／EitemDto 設定的參考，目前本機位置為 `C:/_project/DbAdm`；搬移後重新定位。其資料表、主鍵、joins、欄位及其他設定屬於個案，不直接複製為通用規則。

查詢 Model 見 [ReadDto](../model-ReadDto/SKILL.md)；同名 `ReadSql` 等欄位仍須依所屬 Model 註解理解，不將查詢 Model 的設定原則直接套用至 EditDto。開發慣例沿用 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。各子模組總覽 保留原位置與職責。
