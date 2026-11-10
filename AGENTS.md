# Base 專案工作指引

## 專案概況與入口

- 本庫提供供其他 C# 專案參照的共用程式，並非單一可直接啟動的應用程式。目前各 `.csproj` 皆以 `net8.0` 為目標。
- [Readme-TW.md](Readme-TW.md) 是繁體中文介紹；另有 [Readme-CN.md](Readme-CN.md) 與 [Readme.md](Readme.md)。README 僅介紹早期四個專案，現況應同時核對原始碼與 `.csproj`。
- `Base/`：底層工具、資料模型、介面、資料庫與 CRUD；優先查看 [Base.csproj](Base/Base.csproj)、[初始化入口](Base/Services/_Fun.cs)、[查詢服務](Base/Services/CrudReadSvc.cs)。
- `BaseApi/`：Controller 基底、驗證與授權、HTTP、使用者與簽核等服務；入口為 [BaseCtrl.cs](BaseApi/Controllers/BaseCtrl.cs) 及 `Services/`、`Attributes/`，參照 `Base`。
- `BaseWeb/`：MVC 輸入欄位與工具 ViewComponents，參照 `BaseApi`、`Base`；雖採 Web SDK，[專案設定](BaseWeb/BaseWeb.csproj) 明確指定 `OutputType=Library`，亦見 [readme.txt](BaseWeb/readme.txt)。
- `BaseFlow/`：參照 `BaseWeb` 的 Razor 類別庫，包含 `Areas/XpFlow/` 簽核畫面。
- `BaseAI/`：LLM、即時語音、嵌入向量與上下文處理；從 [AbLlmSvc.cs](BaseAI/Interfaces/AbLlmSvc.cs)、`Interfaces/` 和 `Services/` 查找。
- `BaseEther/`：Ethereum 與 IPFS 整合。
- `BaseOffice/`：Word 與 Excel 處理。
- `Mongo/`：MongoDB 資料存取服務。
- `Redis/`：Redis 快取整合。
- `RedisStack/`：Redis Stack 整合，參照 `BaseAI` 與 `Base`。
- `PdfSpire/`：以 Spire 套件提供文件與 PDF 處理。
- `SmsTaiwan/`：台灣簡訊服務整合。

各模組的套件與專案相依，以其目錄內 `.csproj` 為準。

## 工作與驗證方式

- 使用端需參照所需類別庫，並依 README 在啟動時呼叫 `Base.Services._Fun.Init`；實際參數、設定讀取與錯誤回傳，以 [目前實作](Base/Services/_Fun.cs) 和 [ConfigDto.cs](Base/Models/ConfigDto.cs) 為準。
- 尚未找到 solution、`Program.cs`、CI 設定或正式測試專案，沒有已確認的整庫啟動或測試命令。`Base/Services/_Test.cs`、`BaseAI/Services/CengSvcTest.cs` 與 `BaseFlow/Areas/XpFlow/FlowTest.cshtml` 不足以證明已有自動測試流程。
- 修改程式時，以受影響的 `.csproj` 和參照關係決定建置範圍。從根目錄使用 `dotnet build .\Base\Base.csproj`（其他模組替換為其專案路徑）；已確認 Base 原始碼隔離副本可沿用既有 NuGet 還原資料以 `--no-restore` 建置，不能推論其他模組或全新還原也可通過。
- 初次檢查環境已安裝 .NET SDK `9.0.318`；全新 NuGet 還原、外部服務與資料庫整合測試是否可行仍待確認。驗證報告需區分實際執行結果與未執行事項。

## 重要約定

- 專案溝通與新增文件預設使用繁體中文，沿用專案現有術語；程式識別名稱與必要原文維持原樣。
- 開工先查看 `git status --short`，保留既有修改與未追蹤檔案，不覆寫或回復非本次工作的內容。
- 沿用所在模組既有命名、namespace 與程式風格；常見結構為 `Services/`、`Models/`、`Enums/`、`Interfaces/`，但不是每個模組都有全部目錄。
- 判斷有效程式以 `.csproj` 為準；多處有 `Compile Remove`，不能將所有 `.cs` 都視為編譯來源。`*_old`、`*_bak`、`*_npoi`、`*_op` 等版本先核對用途，不直接視為現行入口，也不順手清理。
- 遵循 [.gitignore](.gitignore)，避免納入建置產物；勿把實際金鑰、密碼或連線資訊寫入指引。

- 實作前先查相關模組的使用 skill 與來源，優先呼叫符合需求的既有共用函式，避免重寫相同邏輯；完成時交代關鍵重用結果或自行實作的原因。
- 跨模組與應用的前端／後端開發慣例見 [shared-coding-rules](.agents/skills/shared-coding-rules/SKILL.md)，按任務讀取對應規則；各模組 API 用法仍留在原有模組 skill。

## 知識整理原則

- Base 根目錄下名稱前綴為 `zz_` 的子目錄目前未使用，本次專案知識整理先略過，不納入模組介紹或知識索引；不刪除或修改這些目錄。
- 本檔只保留長期有效、跨任務適用的約定、入口與已確認的驗證方式；新發現須有檔案或實際執行依據，不確定處明確標記。
- 詳細模組知識優先引用現有文件與原始碼，避免在此複製大量內容；文件與實作不一致時，記錄差異再核對。
- 穩定、可重複的流程可整理為 skills；臨時進度、猜測與單次操作紀錄不當作長期規則。
- 各獨立 `.csproj` 子專案在自己的 `.agents/skills/` 維護 skills 原始檔；從大 Base 儲存庫工作時，依需要讀取子專案 skill，不假設根目錄會自動遞迴探索。跨應用的發現與使用留待未來插件封裝處理，本次不安裝或發佈。
- 新增、搬移或刪除子專案的專用 skill 時，同步更新該子專案的 `.agents/skills/00-overview/SKILL.md`，確保清單中的每項都有有效連結與一句適用情境。大 Base 根總覽只在模組清單或用途變動時同步更新。
- 使用 `Base/Base.csproj` 的應用端整合，參考 [base-overview](Base/.agents/skills/00-overview/SKILL.md)；只涵蓋 Base 程式庫，詳細 API 按需載入其 references。程式參照不代表應用端已載入此 skill。

## 子專案架構入口

- [大 Base 總覽](.agents/skills/00-overview/SKILL.md)提供各現用子專案的入口。各子專案使用相同的總覽相對路徑，依任務選讀。

- [Base](Base/docs/architecture.md)、[BaseAI](BaseAI/docs/architecture.md)、[BaseApi](BaseApi/docs/architecture.md) 各自維護職責與公共契約；先讀子專案 AGENTS，再按任務載入既有 skill references。
- 應用提示檔、路由白名單與實際啟動流程由應用文件維護，不在共用專案複製；文件中的工作區核對結果不代表發布保證。
