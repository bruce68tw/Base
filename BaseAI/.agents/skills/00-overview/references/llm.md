# LLM、STT 與嵌入

來源路徑相對於 BaseAI：`Interfaces/AbLlmSvc.cs`、`Services/ChatGptSvc.cs`、`Services/GeminiSvc.cs`、`Services/_Voice.cs`、`Models/VoiceCmdDto.cs`。依 2026-11-03 工作區核對，不代表已發佈版本。

## 共用契約

- `AskA(string prompt, string question)` 回傳字串；GeminiSvc 回傳抽取後的回答文字，ChatGptSvc 仍回傳 HTTP response body，不能假設 provider 的回應格式可互換。
- `SttA(ReadOnlyMemory<byte> audioData, string mimeType, string prompt, List<string> keywords, CancellationToken ct = default)` 回傳文字，任務格式與無人聲約定由使用端決定；這已取代舊 `SpeechToTextA` 說明。prompt／keywords 不可為 null，可為空白／空清單；輸入必須是完整音檔，裸 PCM 先明示規格轉換。
- `TextToVectorA(string text)` 回傳可為 null 的 `float[]`，使用端處理失敗並核對維度。
- 基底類別實作 IAsyncDisposable，釋放其 HttpClient；狀態與 DI 生命週期仍依使用端設計。

## ChatGptSvc

建構參數依序為 llmUrl、embedUrl、apiKey、transcriptionUrl、transcriptionModel，後四項有預設值，以實際建構式為準，不在文件固定部署模型或憑證。

- AskA 送出 system/user input；實作沒有在 body 送出 model，也未套用 apiKey 的 Authorization。應確認 URL 是否為應用代理，不能假設填入 apiKey 就可直連。失敗例外可能含回應本文，使用端不要直接輸出。
- SttA 目前只接受 WAV MIME，使用 `_Voice.Validate` 驗證；壓縮格式請先轉換。要求非空 apiKey，採 Bearer header，multipart file/model/prompt。prompt 與 keywords 合併為轉錄背景，不等同 Gemini 的任意輸出指示契約；回傳 text 經 Trim，缺漏時為空字串。
- TextToVectorA 仍回傳 null，不能據此完成 embedding。

## GeminiSvc

建構參數為 llmUrl、embedUrl、embedDim、ffmpegPath；後三項有預設值。實際 endpoint、模型與憑證由應用負責，不在共用文件複製值。

- AskA 送出 systemInstruction/contents，要求 HTTPS；30 秒逾時涵蓋請求與回應讀取，檢查 HTTP、封鎖與候選完成狀態。成功依序串接第一個完成候選中非 thought 的 text 片段，僅回傳回答文字，不含 candidates／usageMetadata 等外層資料；不 trim、不解析回答中的 JSON。失敗呼叫 _Log.Error 並回傳空字串；成功的文字 "[]" 與失敗的空字串不同。AskA 目前無呼叫端取消參數。
- SttA 要求 HTTPS endpoint；20 秒 linked timeout 包含音訊轉換、傳送與讀取。先呼叫 `_Voice.ToWavA`，失敗 null 轉為錯誤。prompt 直接作 systemInstruction，keywords 僅作額外詞彙提示。
- STT 檢查 HTTP、封鎖與候選完成狀態，抽出非 thought 的文字片段後原樣串接；不 Trim、不解讀 `[SILENCE]`、不解析指令 JSON。呼叫端若要純轉錄需自行定義及處理無人聲標記；若要 JSON 則定義與驗證自己的輸出契約，不能宣稱只靠提示保證模型格式。
- 共用音訊規格、FFmpeg 需求及失敗 log/null 契約見 [架構與語音分工](../../../../docs/architecture.md)。不再以固定 header 把任意 PCM 視作 16 kHz WAV。
- TextToVectorA 使用 `_embedUrl`／`_embedDim`，taskType 目前固定 RETRIEVAL_QUERY；索引與查詢維度、taskType 需依模型用途查證，勿混用不同模型的 collection。

## 指令模型與使用端責任

VoiceCmdDto 的 Ctrl／Action 為可空字串，Args 為 JArray。它不提供允許路由清單、Name／Value 型別驗證、逗號多值解析或指令執行。這些由應用負責，不把 JSON 形狀驗證等同授權。共用服務不內建 DemoAI 的提示檔路徑或功能名稱。

本次只核對原始碼與文檔，不宣稱外部模型、FFmpeg 部署或 ChatGPT／Gemini 契約可互換。
