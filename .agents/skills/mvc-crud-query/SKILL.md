---
name: mvc-crud-query
description: 在既有 Base 系列 MVC 應用新增或修改 CRUD 查詢功能時，逐題補齊需求、確認摘要，再依序備份、產生、串接及驗證查詢與已確認的維護操作。不涵蓋建立專案。
---

# MVC CRUD 查詢開發指引

本檔為第四主題「功能開發指引」初稿，供持續討論；包含已確認的查詢開發原則與編輯需求收集，詳細維護實作尚待整理。僅在既有 MVC 應用開發功能，不建立專案，也不將參考功能的個案設定當成通用規則。

## 確認生成目錄

- 以使用者指定的既有應用專案目錄為準。在 Codex 中，使用者若以專案屬性指定目錄，就以該目錄為準。確認其絕對路徑及對應的應用程式 `.csproj`，並把路徑列入產生前的需求摘要。
- 目前工作目錄、Codex 自動建立的 Git worktree 和暫存目錄可能與目標不同。只有使用者明確指定 worktree 為目標時，才把正式檔案產生在該 worktree。若目標目錄無法寫入，依環境的權限流程處理，並回報無法完成的部分。
- 備份、產生、修改與正式打包產物一律以已確認的目標專案為根目錄。動手前在目標專案查看 Git 狀態並核對相關檔案，保留既有變更。若用隔離目錄建置，完成前仍要確認正式檔案已落在目標專案。

## 需求確認與產生時機（使用者已確認）

收到產生 CRUD 查詢功能的需求時，先確認目標既有專案目錄，再先問是否需要編輯畫面，之後依序補齊適用資訊。已提供的資訊直接沿用，只詢問缺漏，一次一個問題：

1. 功能名稱／代碼，用於命名檔案。
2. 查詢條件欄位，以及各欄位的比對種類、資料型態與輸入元件；使用下拉式元件時，另確認選項來源。
3. 查詢條件欄位編排。收齊查詢條件欄位、比對種類、資料型態與輸入元件後，由 AI 先提出一版。每一文字行代表畫面一列，同列多欄以逗號分隔。請使用者調整換行與欄位順序並確認，將確認結果納入產生前需求摘要。純查詢也須確認查詢欄位編排。
4. 查詢結果顯示欄位及對應 SQL。

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

使用者確認編排與種類後，將結果納入產生前需求摘要；下拉式／radio 選項來源、必填與驗證規則等仍依既有流程逐題詢問缺漏。不額外詢問 Bootstrap 欄寬或分組，也不自行發明寬度標記。純查詢只跳過編輯畫面的編排詢問。

純查詢時不追問編輯細節，也不產生維護檔案。需要編輯時，將上述資訊與查詢需求合併為同一份摘要，經使用者確認後才產生。

列表逐列操作須分別一次一題詢問：

1. 每列是否顯示修改 icon？
2. 每列是否顯示刪除 icon？
3. 每列是否顯示單筆檢視 icon？

不預設全部啟用，答案須與是否需要 Edit View 及已確認的支援操作一致；修改與單筆檢視共用同一份 Edit View。新增按鈕不屬於這三種逐列操作。操作虛擬欄的資料對應依 [ReadDto 指引](../../../Base/.agents/skills/model-ReadDto/SKILL.md)，本輪不增訂刪除實作契約。

生成 CRUD 時，另必問「此功能是否需考慮操作者的資料權限？」此處指操作者可存取哪些資料，與是否可進入功能頁的權限不同。若需要，再一次一題詢問「此資料表哪個代表 User ID 的欄位要關聯 XpUser.Id？」常見為 Creator，但不能自行固定；若涉及關聯路徑，須由使用者明確指定來源，不套用固定路徑。將答案納入需求摘要；Model 設定依 [EditDto 的資料權限規則](../../../Base/.agents/skills/model-EditDto/SKILL.md)。

資訊收齊後，提供需求摘要（summary）供使用者確認；摘要獲得確認後才產生程式，不能未確認就開工產生。以下備份處理也在確認後、產生程式前執行。

## 欄位顯示名稱對照（使用者已確認）

以目標應用的專案目錄為起點，固定使用全小寫的 `_data/tables`。每張資料表使用一份 UTF-8 JSON，檔名沿用實際資料表名稱，例如 `<DemoAI 專案目錄>/_data/tables/XpUser.json`。JSON 物件直接以欄位代碼為鍵、顯示名稱為值，例如 `{"Name": "姓名"}`。

收集需求時，先依欄位的實際來源表查找對照，既有名稱可用於查詢條件、列表及編輯畫面。跨表欄位依各自來源表查找。計算欄位的顯示名稱在功能需求摘要中確認。

缺少對照檔或欄位時，將建議名稱列入需求摘要，經使用者確認後才隨該次 CRUD 在目標應用新建或補寫對應 JSON，保留既有對照。單一畫面的特別叫法記在該功能的摘要並用於該畫面。只有使用者確認要統一更名時，才更新共用名稱。

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

## 摘要確認後的程式產生順序

1. **定位實作入口與目標檔。** 依目標應用的 `.csproj`、實際參照版本與最相近的既有功能，核對 Controller、查詢 Service、Read View、TypeScript 入口及其打包方式。需要編輯時，再定位維護 Service 與共用的 Edit View。按[共用開發規則](../shared-coding-rules/SKILL.md)及摘要確認的功能代碼決定路徑，資料表專用 Service 按實際需求納入。若路由、欄位或操作仍有缺漏，先查目標應用相同用途的入口。查證後仍有會影響需求或實作的歧義，先釐清再處理受影響的部分。

2. **備份同名目標檔。** 在產生程式前執行上節的改名備份與排除檢查。若需調整共用的 `_XpCode.cs`，依選項來源規則精準修改，保留其他功能。需要新建或補寫顯示名稱時，依[欄位顯示名稱對照](#欄位顯示名稱對照使用者已確認)在目標應用更新對應 JSON。已有對照檔時，按確認的欄位逐項更新。

3. **產生查詢後端。** 必讀 [ReadDto](../../../Base/.agents/skills/model-ReadDto/SKILL.md) 與 [QitemDto](../../../Base/.agents/skills/model-QitemDto/SKILL.md)，再依其導引閱讀類別與註解。依摘要實作 `ReadSql` 與查詢條件，重用目標應用採用的分頁服務。DbAdm Column 由 `ColumnController.GetPage` 接收 `DtDto`，經 `ColumnRead` 呼叫 `CrudReadSvc.GetPageA`。實際 action 名稱、HTTP 方法、輸入、回傳與授權，須對照目標前端的請求及既有 Controller，並參照 [BaseApi 指引](../../../BaseApi/.agents/skills/00-overview/SKILL.md)。需要資料權限時，另核對列表服務實際套用的限制。使用 `CrudReadSvc` 時須核對應用的列權限設定、傳入的 `ctrl` 及 `ReadDto` 權限條件。

4. **產生查詢畫面與列表。** 依已確認的查詢欄位編排產生 Read View，按 [BaseWeb 指引](../../../BaseWeb/.agents/skills/00-overview/SKILL.md)核對元件及實際輸出的欄位識別。下拉式／radio 依上節接好 `_XpCode`、Controller 與 View。沿用目標應用的表單、列表及腳本載入入口，建立 TypeScript 的查詢與欄位設定。核對表單送出的名稱與型態、`QitemDto` 的欄位及比對、SQL 回傳名稱與列表欄位順序。虛擬欄依 ReadDto 指引處理，SQL 須提供 renderer 所需的資料與列識別值。操作欄依下節及摘要設定。

5. **串接已確認的維護操作。** 純查詢直接進入驗證。涉及編輯時，必讀 [EditDto](../../../Base/.agents/skills/model-EditDto/SKILL.md) 與 [EitemDto](../../../Base/.agents/skills/model-EitemDto/SKILL.md)，再核對類別與註解。DbAdm Column 由 `ColumnEdit.GetDto()` 提供設定，透過 `BaseEditSvc` 串接單筆讀取與異動。依摘要設定資料表、主鍵、讀寫欄位及驗證，單筆 SQL 與資料權限依 EditDto 指引處理。按已確認的編排產生同一份 Edit View，並依操作串接按鈕或圖示、表單與 Controller。修改須核對取回資料及儲存兩條路徑，單筆檢視須核對取回資料及唯讀狀態，新增須核對初始值及提交。對照目標應用的 Controller 和前端 CRUD 來源，核對主鍵傳遞、資料填入、提交格式、後端驗證及成功／錯誤回應。若需刪除，先核對目標應用既有刪除事件、action、服務與權限，再確認適用的刪除語意。找不到可沿用的契約時，先釐清該操作再實作。

6. **驗證生成結果。** 檢查 diff、檔案範圍與備份排除，依目標應用的建置方式驗證受影響的 C#／Razor 與 TypeScript／打包產物。能啟動時，由功能入口確認查詢畫面與已確認的編排、篩選、結果欄位、分頁及排序（依啟用項目），再逐一驗證已確認的維護操作。若有資料權限，依適用的列表、單筆與異動路徑檢查允許及拒絕的案例。回報重用的入口、實際驗證結果及未執行的項目。

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
- `Controllers/ColumnController.cs`：查詢及維護 action 的接線。
- `Services/ColumnEdit.cs` 與 `Views/Column/Edit.cshtml`：維護設定與共用編輯表單。
- `_src/tsBase/svc/CrudR.ts` 與 `_src/tsBase/svc/CrudE.ts`：表單、列表及維護操作的前端串接。

上述檔案是查證與討論入口，不代表必須複製 Column 的資料表、欄位或商業規則。

依當前工作範圍讀取既有指引，不在此重複維護：

- 專案種類與環境：[第一主題](../project-types-and-environments/SKILL.md)。
- 命名、分工、風格與重用原則：[第二主題](../shared-coding-rules/SKILL.md)。
- 共用模組選用與使用 skill 入口：[第三主題](../00-overview/SKILL.md)；依實際相依按需載入各模組的 `.agents/skills/00-overview/SKILL.md`，詳細 API 仍以模組文件與來源為準。
