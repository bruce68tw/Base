# 資料主表

以 `DbAdm/Table` 為例，主表為 `dbo.[Table]`，主鍵為 `Id`。

## 編輯 View

- 在 `Views/Table/Read.cshtml` 的 `divEdit` 載入 `Edit` partial。
- 在 `Views/Table/Edit.cshtml` 建立 `<form id="eform">`，以 `<vc:xi-hide-id />` 放入隱藏主鍵。
- 依確認的欄位順序、元件種類及必填設定放入維護欄位。例如 `ProjectId` 用必填下拉，`Code` 用必填文字欄位。
- 顯示用欄位使用唯讀元件，例如 `CreatorName`、`Created`。編輯區使用 `<vc:xg-save2 />` 提供儲存按鈕。

## TypeScript

- 在 `_src/tsView/Table.ts` 初始化 `CrudR`。預設 `EditOne` 使用 `Id` 與 `eform`。
- 只有主表時可省略 `edits` 參數。有一對多子表時，陣列第一格用 `null` 代表預設主表，後續接子表物件。子表接線請讀取 `table-many.md`。

## Edit service

- 在 `Services/TableEdit.cs` 繼承 `BaseEditSvc`，並在 `GetDto()` 回傳主表的 `EditDto`。
- 設定 `Table`、`PkeyFid` 與 `Items`。`Items` 列入主鍵與資料維護欄位，畫面輸入元件的 `Fid` 對應其中的欄位。
- 編輯載入需要關聯資料時，可設定 `ReadSql`。範例以 `t.Id=@Id` 查主表，並查出唯讀顯示用的 `CreatorName`，該欄位未列入 `Items`。
