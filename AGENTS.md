# Base 專案工作指引

## 專案概況與入口

- 本庫提供其他 C# 專案參照的共用類別庫。目前各現用專案的目標框架為 `net8.0`。目標框架、套件與專案相依以各模組的 `.csproj` 為準。
- 選用模組或查找子專案入口時，先讀取 `.agents/skills/projects-intro.md`，再依任務查閱對應的子專案總覽。
- [Readme-TW.md](Readme-TW.md) 提供早期四個專案的介紹。

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

- 查找 Base 共用程式時，先讀取 `.agents/skills/0-find-base-code/SKILL.md`，依需求查閱相關模組的使用 skill 與來源，優先呼叫符合需求的既有共用函式，避免重寫相同邏輯；完成時交代關鍵重用結果或自行實作的原因。
- 跨模組與應用的前端／後端開發慣例請先讀取 `.agents/skills/coding-rules.md`，按任務讀取對應規則；各模組 API 用法仍留在原有模組 skill。

## 知識整理原則

- Base 根目錄下名稱前綴為 `zz_` 的子目錄目前未使用，本次專案知識整理先略過，不納入模組介紹或知識索引；不刪除或修改這些目錄。
- 本檔只保留長期有效、跨任務適用的約定、入口與已確認的驗證方式；新發現須有檔案或實際執行依據，不確定處明確標記。
- 詳細模組知識優先引用現有文件與原始碼，避免在此複製大量內容；文件與實作不一致時，記錄差異再核對。
- 穩定、可重複的流程可整理為 skills；臨時進度、猜測與單次操作紀錄不當作長期規則。
- 在 Markdown 中引用 skill 或共用指引時，明寫讀取動作，並用反引號標示相對於目前文件的路徑。例如「先讀取 `.agents/skills/ask-question.md`，依其中的流程收集需求」。
- 各獨立 `.csproj` 子專案在自己的 `.agents/skills/` 維護 skills 原始檔；從大 Base 儲存庫工作時，依需要讀取子專案 skill，不假設根目錄會自動遞迴探索。跨應用的發現與使用留待未來插件封裝處理，本次不安裝或發佈。
- 新增、搬移或刪除子專案的專用 skill 時，同步更新該子專案的 `.agents/skills/0-overview/SKILL.md`，確保清單中的每項都有有效連結與一句適用情境。大 Base 根總覽只在模組清單或用途變動時同步更新。
- 使用 `Base/Base.csproj` 的應用端整合，參考 [base-overview](Base/.agents/skills/0-overview/SKILL.md)；只涵蓋 Base 程式庫，詳細 API 按需載入其 references。程式參照不代表應用端已載入此 skill。

## 子專案架構入口

- [Base](Base/docs/architecture.md)、[BaseAI](BaseAI/docs/architecture.md)、[BaseApi](BaseApi/docs/architecture.md) 各自維護職責與公共契約；先讀子專案 AGENTS，再按任務載入既有 skill references。
- 應用提示檔、路由白名單與實際啟動流程由應用文件維護，不在共用專案複製；文件中的工作區核對結果不代表發布保證。
