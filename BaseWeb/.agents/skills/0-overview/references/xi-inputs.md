# Xi 輸入欄位

## 呼叫與來源

一般 Xi 元件的 `Invoke` 收 `BaseWeb.Models` 下同名 DTO，回傳 `_Input.Xi…(dto)` 包裝的 `HtmlString`。例如 Razor：

```cshtml
@using BaseWeb.Models
@await Component.InvokeAsync("XiText", new {
    dto = new XiTextDto {
        Fid = "Code", Title = "代碼", Required = true, MaxLen = 20
    }
})
```

此例展示呼叫參數；應用仍須完成 MVC 元件探索。若使用 `<vc:xi-text>` 語法，先確認使用端 `_ViewImports.cshtml` 有 `@addTagHelper *, BaseWeb`。元件回傳的 HTML 字串不會再解析其中的 `<vc:…>`；組合輸出應呼叫 `_Input` 方法或在 Razor 層組合。

特殊入口：`XiHideId.Invoke()` 不收 DTO；`XiRadio0.Invoke(string fid, string value, string label)` 為單個 radio。`XiReadViewComponent` 目前未繼承 `ViewComponent`，不能直接保證標準 MVC 探索成功；使用時檢查探索結果，必要時評估既有 `_Input.XiRead` 呼叫方式。

## 共用欄位契約

依據：`Models/XiBaseDto.cs` 與 `Services/_Input.cs`。

- `Fid` 通常生成 `data-fid` 與 `name`，不是通用 DOM `id`；空值不生成這兩個屬性。
- `Title` 非空時，多數方法才加 label/grid 佈局。`Cols="2,3"` 為水平 label/input，單一數字為垂直；空值由 `_Fun.DefHoriColList` 決定。`InRow=true` 避免再包外層 `.row`。
- `Edit` 寫成 `data-edit`：DTO 約定 C 為新增、U 為修改、空字串不限，其他如 x 為不可編輯。這不是原生 readonly，效果須由使用端前端確認。
- `Width` 大於零產生 `.x-w{Width}`；確認應用有該 CSS。`ClsBox` 可能位於包裝容器，不能假定加在 input。
- `FnOnChange`、`FnOnBlur`、`EventArgs` 的支援依個別方法確認。`GetEventAttr` 產生 `data-on…` 和 `data-args`，本身不綁定瀏覽器事件。
- `Required` 星號與原生 required 並非每種元件都相同；不可將畫面約束視為後端驗證。

## 選型與容易誤判處

| 需求 | 查找與注意事項 |
| --- | --- |
| 單行／多行文字 | XiTextDto、XiTextareaDto；XiText.Pattern 預設 `InputPatternEstr.EngNumExt`，一般中文自由輸入應確認所需 pattern；密碼用 IsPwd |
| 下拉／radio | XiSelectDto、XiRadioDto 的 Rows 為 `List<Base.Models.IdStrDto>`，Id 為值、Str 為文字；select 預設 AddEmpty=true；radio 空清單有內建預設，先讀 XiRadio 方法 |
| checkbox | XiCheckDto 的 Value 是勾選值，IsCheck 才控制初始勾選；XiCheck 會把空 Value 改為 1，並將 FnOnChange 寫入 data-onclick |
| 日期／日期時間 | XiDateDto、XiDtDto；XiDt 組合日期與時分 select，MinuteStep 預設 10；需核對 `_Date.CsToDt` 接受格式及前端插件 |
| 整數／小數 | XiIntDto、XiDecDto；目前 Min、Max 只有大於零才輸出，不能用此實作保證零或負數邊界；XiDec 與 XiInt 的初值 HTML 也須分別檢查 |
| 唯讀／隱藏 | XiReadDto 的 SaveDb 預設 false，輸出標記控制前端是否儲存；XiHideDto 與 XiHideId 用途分開確認，不將畫面隱藏當作授權 |
| 檔案／連結 | XiFileDto、XiLinkDto；Table、Fid 和 `_me.fnViewFileA` 為應用契約；MaxSize 預設 10 MB，非正值改用 `_Fun.Config.UploadFileMax`，實際上傳及伺服器限制由使用端確認 |
| 複合欄位／HTML | XiCheckText、XiRadioText 會生成 Fid 加 Desc 的文字欄；XiHtml 的編輯器初始化須查應用端，不能當一般 textarea 推論 |

部分方法會修改傳入 DTO，例如 XiCheck、XiRadio、XiFile；避免重複共用同一 DTO 渲染而未檢查副作用。DTO 的共用屬性不代表每個方法均使用它：例如 XiDate 沒有將 FnOnChange 傳給 GetDateHtml。
