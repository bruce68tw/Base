# 一對多子表

以 `DbAdm/Table` 為例，主表 `Table.Id` 對應子表 `Column.TableId`。

## 編輯 View

範例檔案為 `Views/Table/Edit.cshtml`。

- 每張子表建立一個 HTML `<table>`，並指定空的 `<tbody id="tbodyCol">`。
- 建立 `<script id="tplCol" type="text/template">`，內含一個 `<tr>`、隱藏主鍵、維護欄位及刪除列按鈕。
- Script template 內的欄位必須使用 `@await Component.InvokeAsync(...)` 語法。
- 表頭與模板中的可見欄位順序一致。
- 新增與刪除按鈕分別接到該子表的 `EditMany.onAddRow` 與 `EditMany.onDeleteRow`。

## TypeScript

範例檔案為 `_src/tsView/Table.ts`。

- 每張子表建立一個 `EditMany`，範例為 `mCol = new EditMany('Id', 'tbodyCol', 'tplCol', 'tr')`。
- 透過 `new CrudR(config, [null, _vo.mCol])` 接線。陣列第一格為主表位置。
- 前端子表順序須與後端 `Childs` 一致。

## Edit service

範例檔案為 `Services/TableEdit.cs`。

- 在主表的 `EditDto.Childs` 中，為每張子表加入一個 `EditDto`。
- 設定 `Table`、`PkeyFid` 和 `FkeyFid`，範例值分別為 `"dbo.[Column]"`、`"Id"` 和 `"TableId"`。
- `Items` 包含模板中的欄位 `Fid`、主鍵及外鍵。外鍵由共用 CRUD 依主表鍵值處理。
- 需要排序時設定 `OrderBy`，範例為 `"Sort"`。
