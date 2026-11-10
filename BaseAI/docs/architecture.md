# BaseAI 架構入口

依 2026-11-03 工作區來源核對，包含未提交的新契約；詳細整合入口是 [baseai-overview](../.agents/skills/00-overview/SKILL.md)。

## 責任與相依

[BaseAI.csproj](../BaseAI.csproj) 是 `net8.0` 類別庫，直接參照 BaseApi 與 Base，沒有應用啟動入口。它不等同相鄰的獨立 LlmChatGpt／LlmGemini 專案。模型、endpoint、憑證、提示、功能白名單與保存政策由使用端提供。

| 入口 | 責任與詳細文件 |
| --- | --- |
| [AbLlmSvc.cs](../Interfaces/AbLlmSvc.cs)、[GeminiSvc.cs](../Services/GeminiSvc.cs)、[ChatGptSvc.cs](../Services/ChatGptSvc.cs) | Ask／STT／embedding；[llm.md](../.agents/skills/00-overview/references/llm.md) |
| [_Voice.cs](../Services/_Voice.cs) | 完整 WAV 驗證、明確 PCM 規格封裝與壓縮音訊轉換 |
| [VoiceProgDto.cs](../Models/VoiceProgDto.cs) | `Ctrl`／`Action` 可空字串或 null，`Args` 為 JArray；DTO 本身不驗證 Name／Value 或授權、不執行路由 |
| [Interfaces](../Interfaces) | Live LLM 與嵌入資料庫抽象契約；[live.md](../.agents/skills/00-overview/references/live.md)、[vectors-context.md](../.agents/skills/00-overview/references/vectors-context.md) |

## 語音分工

`AbLlmSvc.SttA(audioData, mimeType, prompt, keywords, ct)` 接收完整音檔、實際 MIME、任務提示與術語，輸出字串；輸出格式及無人聲約定由呼叫端決定。不能推定所有 provider 都支援以提示產生任意 JSON。

GeminiSvc 先以 `_Voice.ToWavA` 轉成完整 WAV，再將 prompt 原樣用作 systemInstruction，keywords 作為額外辨識詞彙提示。完成後只抽取非 thought 的文字片段，不 trim、不解析 JSON、不替呼叫端解讀 `[SILENCE]`。純轉錄端如需此標記，必須自行在提示定義並在使用端處理；指令 JSON 端應驗證自己的 schema。沒有固定的 Ctrl／Action 清單，也沒有由此服務觸發路由執行。

`_Voice.ToWavA`：WAV 驗證後保留；裸 PCM 必須明示 sampleRate／channels／bitsPerSample，不能靠改 MIME 冒充 WAV；壓縮格式依賴部署環境的 FFmpeg，解碼成 16 kHz 單聲道 16-bit PCM WAV。輸入／輸出各限 10 MB，工具轉換上限 30 秒；Gemini 的整段 STT 另有 20 秒上限。失敗及取消記錄 `_Log.Error` 的安全分類訊息並回傳 null，Gemini 接到 null 會失敗；取消 token 亦向下游傳遞。FFmpeg 不會自動下載或安裝。

[VoiceProgDto](../Models/VoiceProgDto.cs) 只作資料載體；Name／Value、逗號多值語意與允許功能清單由應用定義及驗證，不能把反序列化成功當作信任或執行許可。

## 維護與驗證

從此目錄執行 `dotnet build BaseAI.csproj --no-restore -m:1`，需要相鄰 Base／BaseApi 與還原資料。修改 SttA 簽章時一併查所有 provider 及使用端；檢查 csproj 排除的 zz_ 來源。Live 狀態與生命週期依既有 skill，不因 DemoAI 的單一 STT 接線推論所有服務都可跨使用者共用。

本次文件任務查證抽象簽章、Gemini 文字抽取、音訊轉換與 DTO，未呼叫模型、測試 FFmpeg 安裝或驗證所有 provider。部署及實際模型輸出仍須分別驗證。
