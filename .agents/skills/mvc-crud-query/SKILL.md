---
name: mvc-crud-query
description: 在既有 Base 系列 MVC 應用新增或修改 CRUD 查詢功能時，逐題補齊需求、確認摘要，並核對查詢欄位一致性及處理同名檔備份。第四主題初稿，包含編輯需求收集，詳細維護實作尚待整理，不涵蓋建立專案。
---

# MVC CRUD 查詢開發指引

本檔為第四主題「功能開發指引」初稿，供持續討論；包含已確認的查詢開發原則與編輯需求收集，詳細維護實作尚待整理。僅在既有 MVC 應用開發功能，不建立專案，也不將參考功能的個案設定當成通用規則。

## 需求確認與產生時機（使用者已確認）

收到產生 CRUD 查詢功能的需求時，先確認目標既有專案目錄，再先問是否需要編輯畫面，之後依序補齊適用資訊。已提供的資訊直接沿用，只詢問缺漏，一次一個問題：

1. 功能名稱／代碼，用於命名檔案。
2. 查詢條件欄位，以及各欄位的比對種類、資料型態與輸入元件；使用下拉式元件時，另確認選項來源。
3. 查詢結果顯示欄位及對應 SQL。

大多數功能的查詢與編輯在一起；需要編輯畫面時，新增、修改、單筆檢視共用同一份編輯 View，不假設三種操作全部啟用。再依序一次一題收集：

1. 支援哪些操作：新增、修改、單筆檢視。
2. 編輯欄位。
3. 各欄位的種類／輸入元件，先依下述清單建議供使用者修正，不全部逐欄先問。
4. 使用下拉式或 radio 元件時的選項來源；沿用已知資訊，只補問缺漏，不自行編造來源或儲存值。
5. 必填及驗證規則。
6. 唯讀欄位。

有編輯畫面時，收齊編輯欄位後，AI 根據已知欄位資訊先建議種類，列出清單供使用者直接修改編排與種類，不全部逐欄先問。每個欄位採「欄位代碼-顯示名稱-種類」；每一文字行代表畫面一列，同列多欄以逗號分隔，行與行內順序就是顯示順序。

種類可包含文字、數字、日期、下拉式、多行文字等。唯讀附在原種類後，例如「日期（唯讀）」，不能以唯讀取代原種類，也不以括號包住整個種類。以下僅為示意，不固定欄名或要求所有備註都使用多行文字：

```text
ItemId-品項-下拉式, Amount-數量-數字
Created-建立日期-日期（唯讀）
Note-備註-多行文字
```

checkbox 僅代表單一欄位的一個勾選框，使用四段特例「欄位代碼-欄位顯示名稱-checkbox-勾選框後方 label」，例如 `Enabled-啟用設定-checkbox-啟用此功能`。第四段 label 也納入產生前摘要。

radio 維持三段式，例如 `Gender-性別-radio`；選項資料來源比照下拉式另行詢問，已知資訊直接沿用，只補問缺漏，不自行編造選項來源或儲存值。

使用者確認編排與種類後，將結果納入產生前需求摘要；下拉式／radio 選項來源、必填與驗證規則等仍依既有流程逐題詢問缺漏。不額外詢問 Bootstrap 欄寬或分組，也不自行發明寬度標記；純查詢跳過此編排詢問。

純查詢時不追問編輯細節，也不產生維護檔案。需要編輯時，將上述資訊與查詢需求合併為同一份摘要，經使用者確認後才產生。

列表逐列操作須分別一次一題詢問：

1. 每列是否顯示修改 icon？
2. 每列是否顯示刪除 icon？
3. 每列是否顯示單筆檢視 icon？

不預設全部啟用，答案須與是否需要 Edit View 及已確認的支援操作一致；修改與單筆檢視共用同一份 Edit View。新增按鈕不屬於這三種逐列操作。操作虛擬欄的資料對應依 [ReadDto 指引](../../../Base/.agents/skills/model-ReadDto/SKILL.md)，本輪不增訂刪除實作契約。

生成 CRUD 時，另必問「此功能是否需考慮操作者的資料權限？」此處指操作者可存取哪些資料，與是否可進入功能頁的權限不同。若需要，再一次一題詢問「此資料表哪個代表 User ID 的欄位要關聯 XpUser.Id？」常見為 Creator，但不能自行固定；若涉及關聯路徑，須由使用者明確指定來源，不套用固定路徑。將答案納入需求摘要；Model 設定依 [EditDto 的資料權限規則](../../../Base/.agents/skills/model-EditDto/SKILL.md)。

資訊收齊後，提供需求摘要（summary）供使用者確認；摘要獲得確認後才產生程式，不能未確認就開工產生。以下備份處理也在確認後、產生程式前執行。

## 下拉式／radio 選項來源（使用者已確認）

需要下拉式或 radio 選項來源時，先詢問「來源是一般資料表，還是 XpCode 資料表？」已知答案直接沿用；一般資料表例如 Project，另一類直接稱為 `XpCode` 資料表。需求摘要須標明來源類別。

目前 AI 代寫選項來源程式的支援範圍僅為這兩類；其他來源明確交由程式設計師自行撰寫，AI 不代寫來源程式或猜測選項。一般資料表分支只詢問資料表名稱（已提供則沿用），固定讀取 `Id` 與 `Name`：Id 為選項值、Name 為顯示文字，不再詢問值／文字欄名，也不另加排序或過濾的必問項。摘要記錄表名與此固定對應；下拉式與 radio 均適用。XpCode 分支只詢問 `Type`，已提供則沿用，缺漏時才逐題詢問；固定以 `Value` 作選項值、`Name` 作顯示文字，依 `Sort` 排序。摘要記錄 Type、此欄位對應與排序；來源仍透過目標專案 `_XpCode` 的既有適用方法，不另猜排序升降或新增問題。使用者提供的 XpCode 欄位為 Type、Value、Name、Sort、Ext、Note；Ext／Note 用途未指定，不推論或列為必問。

`_XpCode` 是應用的共用類別，`XpCode` 是資料表，兩者須區分。上述兩類選項來源皆依下列方式串接。

選項來源由目標應用專案的 `Services/_XpCode.cs` 集中設定；`Controller.Read` 呼叫其方法並以 `await` 取得資料，經 `ViewBag` 交給 View 元件，不直接在前端另寫資料來源。先重用 `_XpCode` 既有適用方法；沒有適用方法時，才依已確認的選項來源新增，不猜測資料表或選項值。來源仍依前述訪談只補問缺漏。

支援來源的答案確認後，`Services/_XpCode.cs` 必須列入本功能的生成／修改範圍，落實對應取值函數，而不只是記錄來源：一般資料表依已確認表名提供 Id／Name，XpCode 依 Type 提供 Value／Name 並依 Sort 排序。已有完全相同且適用的函數時重用，否則新增；方法名稱沿用目標既有風格，不另訂尚未討論的命名標準。

`_XpCode.cs` 是多功能共用檔，應精準新增並保留其他函數，不將整個檔案當作生成目標改名備份後覆蓋。同名函數須先核對語意，不覆寫或破壞其他功能。此處為同名目標檔備份流程的共用檔處理例外。

產生前摘要須列出預計新增或重用的來源函數，以及 Controller `await` → ViewBag → View 的串接；摘要確認後才實際生成。其他來源仍由程式設計師自行撰寫，不因本段擴大 AI 代寫範圍。

DbAdm Column 的下拉式串接示例（僅示意此資料來源，不固定所有功能使用 Projects）：

```csharp
// Services/_XpCode.cs
public static async Task<List<IdStrDto>> ProjectsA(Db? db = null)
{
    return await _Code.TableToCodesA("dbo.Project", db, "Name");
}

// XpCode.Type 來源：Services/_XpCode.cs 的 RptTypesA
public static async Task<List<IdStrDto>> RptTypesA(Db? db = null)
{
    return await _Code.TypeToCodesA(RptType, db);
}

// ColumnController.Read()（一般資料表範例）
ViewBag.Projects = await _XpCode.ProjectsA();
```

```cshtml
@await Component.InvokeAsync(XiEstr.Select,
    new XiSelectDto { Fid = "ProjectId", Rows = ViewBag.Projects })
```

此處 `_XpCode` 選項取值方法的命名：末尾 `A` 表示非同步，呼叫時使用 `await`；`A` 前的 `s` 表示多筆。對應的 ViewBag 變數同樣使用複數 `s`，但不加 `A`，例如 `ViewBag.Projects = await _XpCode.ProjectsA();`；`RptTypesA` 對應 `ViewBag.RptTypes`。此規則僅限本段選項取值方法與 ViewBag，不泛化至其他函數或變數。

以上兩個來源函數入口為 DbAdm `Services/_XpCode.cs` 的 `ProjectsA`（一般資料表）與 `RptTypesA`（XpCode.Type）。生成時依使用者提供的表名或 Type 替換，不固定使用 Project 或 RptType，也不猜測 RptType 的具體值；直接沿用 `_Code.TableToCodesA`／`_Code.TypeToCodesA`，不重寫其查詢邏輯。既有適用函數優先重用，缺少才新增，Controller／View 串接依前述方式。

radio 同樣經 `_XpCode` 提供來源是使用者確認的約定；上述 Column 範例本身展示的是下拉式，不是 radio。具體元件契約依目標使用的模組 skill 與來源核對，不在此複製 API 文件。

## 同名目標檔備份（使用者已確認）

只檢查本功能將產生的相關目標檔案。已有同名檔時，先將原檔改名備份、保留完整舊內容，再產生新的目標檔，不覆寫舊內容，也不處理無關檔案。

備份名稱在完整檔名（含副檔名）後加 `-1`、`-2` 等流水號，例如 `ColumnRead.cs-1`、`Read.cshtml-1`、`Column.ts-1`。已存在的編號不得覆蓋，選下一個可用號；不使用括號編號。

備份的目的是保留舊檔並使其不再參與編譯或打包。不能只靠改名就宣稱所有工具都會忽略：核對目標 `.csproj`、TypeScript 與 gulp 等實際編譯／打包設定，確認備份已排除，必要時處理本功能備份的排除設定。

## 產生前閱讀與一致性檢查（使用者已確認）

產生 Read Service 前，必讀 [ReadDto](../../../Base/.agents/skills/model-ReadDto/SKILL.md) 及 [QitemDto](../../../Base/.agents/skills/model-QitemDto/SKILL.md)，再依其導引閱讀相關類別與註解。ReadDto 設定、ReadSql 選用及資料欄／虛擬欄對應細節集中於該 skill 維護。

新增或修改查詢功能時，同步核對畫面查詢欄位、後端查詢條件、SQL 回傳欄位與前端列表設定，確保名稱及資料意義一致；詳細對應規則依上述 Model 指引。

涉及編輯時，必讀 [EditDto](../../../Base/.agents/skills/model-EditDto/SKILL.md) 及 [EitemDto](../../../Base/.agents/skills/model-EitemDto/SKILL.md)，依其入口閱讀相關類別與註解；詳細維護實作仍待整理，只處理使用者已確認的操作。

## 前端列表操作欄組裝（已核對參考）

參考 DbAdm 的 `_src/tsView/Column.ts` 與 `_src/tsBase/svc/CrudR.ts`，目前本機專案為 `C:/_project/DbAdm`。以下是 Column 原例，不代表新功能預設顯示所有操作：

```typescript
// columns 中的操作欄
{ data: '_Fun' }

// columnDefs 中對應的 renderer
{
    targets: [7],
    render(data, type, full) {
        return _me.crudR.dtCrudFun(full.Id, full.Name, '*');
    }
}
```

- `targets` 從 0 起算；原例 `[7]` 是第 8 欄，產生時依實際列表位置設定，不照抄 7。
- 現行簽章為 `dtCrudFun(key: string, rowName: string, funs: string, exts: string = '')`。`key` 使用資料列實際識別值，範例為 `Id`；SQL 須提供該值及其他 render 依賴資料。虛擬欄位規則見 [ReadDto](../../../Base/.agents/skills/model-ReadDto/SKILL.md)。
- `rowName` 註解用途為刪除前顯示名稱，目前實作未使用；範例傳 `Name` 不代表每張資料表都必須有 Name 欄位，依目標資料提供參數，不杜撰欄位。
- `funs`：`U` 修改、`D` 刪除、`V` 單筆檢視，可組合如 `UV`。依逐題確認的操作設定；只有三者皆要顯示時才使用 `*`，不能預設全開。
- `dtCrudFun` 僅渲染操作圖示，不代表已檢查功能或資料權限；權限需求仍依前述確認分支處理。此示例不新增刪除等操作的實作契約。

## 參考來源與按需導航

標準參考為 DbAdm 的 Column 功能；目前本機專案位置為 `C:/_project/DbAdm`，搬移後依實際來源重新定位。對照目標功能時，按需閱讀：

- `Views/Column/Read.cshtml`：畫面查詢欄位。
- `Services/ColumnRead.cs`：後端查詢條件與 SQL 回傳欄位。
- `_src/tsView/Column.ts`：前端列表設定。

上述檔案是查證與討論入口，不代表必須複製 Column 的資料表、欄位或商業規則。

依當前工作範圍讀取既有指引，不在此重複維護：

- 專案種類與環境：[第一主題](../project-types-and-environments/SKILL.md)。
- 命名、分工、風格與重用原則：[第二主題](../shared-coding-rules/SKILL.md)。
- 共用模組選用與使用 skill 入口：[第三主題](../shared-projects-overview/SKILL.md)；依實際相依按需載入各模組的 `.agents/skills/library-usage/SKILL.md`，詳細 API 仍以模組文件與來源為準。
