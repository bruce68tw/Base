# 即時語音橋接

來源：`Interfaces/AbLiveLlmSvc.cs`、`Services/GeminiLiveSvc.cs`、`Services/QwenLiveSvc.cs`、`Services/AudioTurnBufferSvc.cs`、`Models/LiveLlmOptDto.cs`、`Models/LlmTurnResultDto.cs`、`Models/LlmToolsDto.cs`、`Enums/LlmRespTypeEnum.cs`。

## 工作階段入口

`GeminiLiveSvc(string llmUrl, AbLlmSvc llmSvc)` 與 `QwenLiveSvc(string llmUrl, AbLlmSvc llmSvc)` 均繼承 AbLiveLlmSvc；第二參數用於共用 STT 備援。

應用需先啟用 ASP.NET Core WebSocket middleware，再呼叫：

```csharp
await live.StartA(httpContext, options,
    fnTurnEndA: SaveTurnA,
    fnToolCallA: ExecuteToolA,
    fnBeforeReconnectA: SaveBeforeReconnectA,
    fnGetHistories: GetHistories,
    ct: httpContext.RequestAborted);
```

上例名稱是應用提供的處理器，非庫內現成方法。其型別：

- 回合：`Func<LlmTurnResultDto, CancellationToken, Task<bool>>`，必填；**true 表示要求重建 provider session**，不能拿來表示一般保存成功。
- 工具：`Func<LlmToolCallDto, CancellationToken, Task<object>>`，選填。依名稱與 ArgumentsJson 路由應用允許的工具，並保留呼叫 Id/Name 對應。
- 重連前：`Func<CancellationToken, Task<bool>>`，選填；回傳 false 會取消重連。結束收束也可能呼叫此 callback，修改時核對 InitBridgeA 各呼叫點，避免將它當成只會執行一次的通知。
- 動態歷史：`Func<string?, List<LlmHistoryDto>?>`，選填；回傳 null 會略過這次，且不消耗該 socket 的歷史送出機會。確切注入時機讀 UiToLlm 路徑。

StartA 對非 WebSocket 請求回 HTTP 400，接受後由其管理 UI socket。provider socket 與注入的 STT 服務之釋放責任須核對 DisposeAsync；應用仍需管理服務實例生命週期。

## 前端與回合

- 前端音訊 JSON 使用 `type=audio`、Base64 `audioData`、`frontTurnComplete`；文字使用 `type=text`、`message`。不要將 provider 原生事件直接當成前端協定。
- 輸出使用 audioData、transcription、turnComplete、error 等訊息；確切 JSON 欄位與中斷行為以基底的送出方法及事件 switch 為準。
- 完成及中斷回合交給單一 queue consumer 順序處理。重連先停止當輪 I/O、排空回合，再執行重連前 callback；UI socket 保留。
- 回合 callback 使用 `CancellationToken.None`，以便請求取消後仍處理已排入的回合；保存邏輯需自行設置合理的外部 I/O 超時。
- 缺少有效逐字稿且有音訊的中斷或 NeedSTT 回合，會以 20 秒上限呼叫 STT，MIME 固定為 `audio/pcm;rate=16000`。ChatGptSvc 目前不接受此 MIME，不能假定它能直接充當此備援。
- 無有效內容與純 `[Interrupted]` 標記可能被略過。AudioTurnBufferSvc 負責音訊、文字、history snapshot、token 與中斷尾段；修改中斷流程時連同 TryComplete/TryBreak/ConsumeBreakTail 核對。

## Provider 差異

| 項目 | GeminiLiveSvc | QwenLiveSvc |
| --- | --- | --- |
| endpoint | 檢查 options.EndPoint 非空，但實際連線用建構時的 `_liveLlmUrl`；整合時保持一致 | 優先 options.EndPoint，空時才用建構 URL |
| 初始化 | setup → setupComplete | session.update → session.updated |
| 憑證 | 依傳入的連線 URL；本實作沒有獨立 apiKey 建構參數 | query api_key/apikey/key，否則 DASHSCOPE_API_KEY，轉為 Bearer header |
| 模型 | 自動補 models/ 前綴 | query model 優先於 options.Model |
| 音訊 | 輸入標示 PCM 16 kHz；輸出 MIME 取 provider，缺省 24 kHz | session 格式為 pcm；音訊回應標示 24 kHz；輸入條件須依實際服務核對 |

LiveLlmOptDto 的 Language 目前未直接被 provider 使用；Gemini setup 直接要求輸入/輸出逐字稿，不能僅憑 InputTranScript/OutputTranScript 欄位名稱推定可關閉。AudioResponse=false 是否適用，須對實際模型確認。Histories 是初始／重連歷史，與 fnGetHistories 的動態注入需一起檢查，避免重複送出。
