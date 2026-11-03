# Xg 工具元件

依據是 `ViewComponents/Xg…ViewComponent.cs` 的 `Invoke` 及生成 HTML，不能以名稱推論完整 CRUD 功能。多數元件直接生成按鈕與 `data-onclick`，需要應用提供 `_me`、相關處理器與樣式。

| 任務 | 優先來源 |
| --- | --- |
| 查詢工具列 | XgFindTbarViewComponent、Models/XgFindTbarDto.cs；查詢 `_me.crudR.onFind`，可選重設與進階查詢 |
| 新增／匯出 | XgCreateViewComponent、XgExportViewComponent；預設 `_me.crudR.onCreate`、`_me.crudR.onExport` |
| 儲存／返回 | XgSave、XgSave2、XgSave3、XgToRead；各自 Invoke 參數及預設處理器不同 |
| 列更新／刪除／檢視 | XgCrudFun；hasUpdate、hasDelete、hasView 預設 false；key、rowName 會放入 data-args |
| 明細列操作 | XgAddRow、XgDeleteRow、XgDeleteUpDown；分別確認 fnOnClick、mName、fnDeleteRow 的契約 |
| 欄位與區塊排版 | XgTh 與 Models/XgThDto.cs、XgCol4、XgGroup、XgHr、XgReq |
| 導覽與其他工具 | XgLeftMenu、XgProgPath、XgTool、XgOpenModal、XgHideLink；先讀來源再選用 |

Razor 呼叫方式與 Xi 相同，例如：

```cshtml
@await Component.InvokeAsync("XgCreate", new { fnOnClick = "_me.crudR.onCreate" })
```

事件字串目前有差異：XgSave 預設 `_me.crudE.onSave()`，XgSave2 預設 `_me.crudE.onSave`。不可在不了解使用端派送器前統一加上或移除括號。含逗號的 `data-args` 值也須確認派送器如何解析。

保留使用端依賴的 id（如 btnFind、btnSave）、data-edit、xd-read 等標記；同頁多次放同元件時檢查重複 id。按鈕顯示及 hasUpdate 等參數只處理 UI，後端權限仍需獨立確認。
