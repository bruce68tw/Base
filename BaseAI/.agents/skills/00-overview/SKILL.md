---
name: baseai-overview
description: 整合或維護 BaseAI C# 類別庫時使用，涵蓋 LLM 問答與語音辨識、Gemini/Qwen 即時 WebSocket、Chroma 向量與上下文處理。適用於明確參照 BaseAI.csproj 的工作；獨立 LlmChatGpt、LlmGemini 模組須另行確認。
---

# BaseAI 程式庫整合

## 定位與來源

本 skill 位於 `BaseAI/.agents/skills/00-overview/`；下列原始碼路徑均相對於 `BaseAI/`，可由本目錄向上三層找到。搬移 skill 時仍需取得對應原始碼。

- 先核對 `BaseAI.csproj`：目前為 `net8.0` 類別庫，參照 `../BaseApi/BaseApi.csproj` 與 `../Base/Base.csproj`，沒有應用程式啟動入口。
- `Interfaces/AbLlmSvc.cs`、`AbLiveLlmSvc.cs`、`AbEmbedDbSvc.cs` 是目前抽象契約；三個 `zz_I*.cs` 與 `Services/zz_RagSvc.cs` 被排除編譯，不能當成現行 API。
- 不推定本庫取代獨立 LlmChatGpt 或 LlmGemini。先查應用端 ProjectReference、using 與實際建立的服務。
- 使用 Base 的設定或日誌時，核對應用啟動流程與 `../Base/Services/_Fun.cs`，不要杜撰 BaseAI 專用 Init 或 DI 註冊擴充方法。

架構與音訊分工入口：[architecture.md](../../../docs/architecture.md)。

## 按任務讀取

| 任務 | 參考文件 |
| --- | --- |
| 問答、STT、文字嵌入、provider 選擇 | [llm.md](references/llm.md) |
| 即時語音、前端協定、工具、歷史與重連 | [live.md](references/live.md) |
| Chroma CRUD、相似查詢、上下文組裝 | [vectors-context.md](references/vectors-context.md) |
| 選用音訊處理或語音指令提示詞服務 | [靜態服務索引](../01-service-static/SKILL.md) |

只載入當前任務需要的文件，再核對其中指向的方法實作。這些文件記錄本庫行為；不代表外部服務目前的模型清單或協定保證。

## 整合與修改

應用端參照本 `.csproj`，由應用提供 endpoint、模型、憑證、生命週期與保存政策。不要把實際憑證寫進範例。HTTP 與 WebSocket 服務有可變狀態；避免直接將服務實例跨使用者共用。以 `await using` 或既有容器生命週期處理 `IAsyncDisposable`。

修改抽象契約或 DTO 時，同時搜尋所有衍生 provider 與應用端呼叫。Live 的工具 callback、回合保存與重連語義須一起核對，不能只看方法名稱或註解。

## 驗證

文件變更檢查 frontmatter、相對連結與原始碼依據即可。程式變更從 BaseAI 目錄執行 `dotnet build ./BaseAI.csproj --no-restore`，前提是已有可用還原資料；缺少時再評估還原條件。建置會牽涉 Base 參照，先確認寫入範圍。沒有已確認的正式自動測試流程；`CengSvcTest.cs` 不能視為測試套件。分別回報編譯、離線驗證與實際外部服務測試結果，不能互相代替。
