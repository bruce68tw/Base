# LLM、STT 與嵌入

來源：`Interfaces/AbLlmSvc.cs`、`Services/ChatGptSvc.cs`、`Services/GeminiSvc.cs`。

## 共用契約

- `AskA(string prompt, string question)` 回傳字串；兩個現有 provider 都回傳 HTTP response body，不是已解析的助理文字。由呼叫端按實際 provider schema 解析。
- `SpeechToTextA(ReadOnlyMemory<byte> audioData, string mimeType, CancellationToken ct = default)` 回傳辨識文字。
- `TextToVectorA(string text)` 回傳可為 null 的 `float[]`，呼叫端要處理失敗及核對維度。
- 基底類別實作 `IAsyncDisposable` 並釋放自己的 HttpClient。

## ChatGptSvc

建構參數依序為 `llmUrl`、`embedUrl`、`apiKey`、`transcriptionUrl`、`transcriptionModel`，後四項有預設值；以目前建構式為準。

- AskA 送出含 system/user 的 `input`。實作沒有送出 model，也沒有套用 apiKey 的 Authorization；需確認傳入 URL 是否為應用代理及其契約。不能假設設定 apiKey 就能讓 AskA 直連官方服務。非成功 HTTP 會拋例外。
- STT 使用 multipart file/model，將 apiKey 放入 Bearer header。接受 WAV、MP3/MPEG、MP4/M4A、WebM、Ogg；原始 `audio/pcm;rate=16000` 不在 switch 支援範圍內。
- `TextToVectorA` 仍直接回傳 null；不能以此實作完成 embedding 流程。

## GeminiSvc

建構參數為 `llmUrl`、`embedUrl = ""`、`embedDim = 0`。

- AskA 送出 systemInstruction/contents；空 llmUrl 回傳空字串。catch 回傳空字串，且未呼叫 EnsureSuccessStatusCode，因此收到 body 不等於成功。
- STT 對 `audio/pcm` 開頭的 MIME 加上固定 16 kHz、單聲道、16-bit WAV header；這不是重採樣，輸入位元組須符合該格式。請讀 SpeechToTextA 的超時與取消處理。
- embedding 讀取 `TextToVectorA` 的 request/response 實作，核對 `_embedUrl`、`_embedDim` 與資料庫維度，不把不同模型的向量混存於同一 collection。
- embedding 的 taskType 目前固定為 `RETRIEVAL_QUERY`，outputDimensionality 使用 `_embedDim`。文件建索引時需核對是否需要不同 taskType；不要直接假定同一方法已區分查詢與文件嵌入。

範例只使用應用提供的變數，不假定 endpoint 或模型：

```csharp
await using var llm = new BaseAI.Services.GeminiSvc(llmUrl, embedUrl, embedDim);
var responseBody = await llm.AskA(systemPrompt, question);
var vector = await llm.TextToVectorA(question);
// responseBody 仍需解析；vector 仍需檢查 null 與維度。
```
