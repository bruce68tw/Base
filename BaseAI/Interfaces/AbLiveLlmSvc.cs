using Base.Services;
using BaseAI.Enums;
using BaseAI.Models;
using BaseAI.Services;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Threading.Channels;

namespace BaseAI.Interfaces
{
    /// <summary>
    /// Live LLM provider 工作階段的抽象基底類別。
    /// 定義 provider 連線、音訊／文字／工具收送及事件接收，並提供標準 client WebSocket 橋接流程。
    /// 呼叫端仍負責提供應用程式設定與每回合的保存政策。
    /// </summary>
    /// <param name="llmUrl">Live LLM provider 的 WebSocket endpoint。</param>
    /// <param name="uiSocketSvc">目前 request scope 所屬的 UI WebSocket 服務。</param>
    public abstract class AbLiveLlmSvc(string llmUrl, WebSocketSvc uiSocketSvc) : IAsyncDisposable
    {
        private readonly WebSocketSvc _uiSocketSvc = uiSocketSvc
            ?? throw new ArgumentNullException(nameof(uiSocketSvc));

        /// <summary>server 連往 Live LLM provider 的 endpoint。</summary>
        protected string _llmUrl = llmUrl;

        /// <summary>建立及包裝 server 與 Live LLM 之間連線的服務。</summary>
        protected WebSocketSvc _llmSocketSvc = new();

        /// <summary>取得上游 Live LLM WebSocket 是否處於開啟狀態。</summary>
        public bool IsOpen => _llmSocketSvc?.IsOpen == true;

        #region 以下函數各家LLM做法不同, 宣告抽象, 各自實作
        /// <summary>連線至 live llm provider，套用工作階段設定並完成必要的初始化。</summary>
        /// <param name="optDto">模型、提示詞、語音、工具及對話歷史等設定。</param>
        /// <param name="ct">取消連線或初始化操作的權杖。</param>
        public abstract Task ConnectLlmA(LlmOptDto optDto, CancellationToken ct = default);

        /// <summary>webSocket 傳送一段原始音訊資料至 provider。</summary>
        /// <param name="audioMem">符合目前 provider 音訊格式要求的位元組資料。</param>
        /// <param name="ct">取消傳送操作的權杖。</param>
        public abstract Task WebToLlmAudioA(ReadOnlyMemory<byte> audioMem, CancellationToken ct = default);

        /// <summary>通知 provider 目前的音訊輸入已結束，或提交目前音訊回合。</summary>
        /// <param name="ct">取消操作的權杖。</param>
        public abstract Task WebToLlmAudioEndA(CancellationToken ct = default);

        /// <summary>將一段使用者文字作為輸入傳送至 provider。</summary>
        /// <param name="text">使用者輸入文字。</param>
        /// <param name="ct">取消傳送操作的權杖。</param>
        public abstract Task WebToLlmTextA(string text, CancellationToken ct = default);

        /// <summary>將工具執行結果回傳給 provider。</summary>
        /// <param name="responses">要回傳的工具呼叫結果。</param>
        /// <param name="ct">取消傳送操作的權杖。</param>
        public abstract Task WebToLlmToolRespA(IEnumerable<LiveLlmToolRespDto> responses, CancellationToken ct = default);

        /// <summary>持續接收 provider 事件，並以共用 DTO 逐筆輸出。</summary>
        /// <param name="ct">取消接收操作的權杖。</param>
        /// <returns>音訊、逐字稿、工具呼叫、錯誤或回合完成事件。</returns>
        public abstract IAsyncEnumerable<LlmRespDto> OnLlmToWebTurnA(CancellationToken ct = default);
        #endregion

        /// <summary>以正常狀態關閉上游 provider 工作階段；尚未連線時不做事。</summary>
        /// <param name="ct">取消關閉操作的權杖。</param>
        public virtual async Task CloseA(CancellationToken ct = default)
        {
            if (_llmSocketSvc != null)
                await _llmSocketSvc.CloseA(WebSocketCloseStatus.NormalClosure, "Live LLM session ended");
        }

        /// <summary>等待背景工作收束；忽略取消及由外層連線錯誤處理負責回報的例外。</summary>
        private static async Task WatchTaskA(Task task)
        {
            try { await task; }
            // 取消屬正常收束；其他例外已由外層連線錯誤處理回報，此處不重複拋出。
            catch (OperationCanceledException) { }
            catch { }
        }

        /// <summary>透過建構時提供的 UI socket 傳送 JSON 訊息。</summary>
        public Task WebToUiDataA(object payload, CancellationToken ct = default)
        {
            return _uiSocketSvc.SendDataA(payload, ct);
        }

        /// <summary>
        /// 使用 UI WebSocketSvc 建立 client 與 Live LLM 的雙向橋接，並在回合 callback 要求時重建 LLM session。
        /// client 音訊訊息使用 <c>type=audio</c>、Base64 <c>audioData</c> 及 <c>frontTurnComplete</c>；
        /// 文字訊息使用 <c>type=text</c> 和 <c>message</c>。LLM 回應轉為 <c>audioData</c>、
        /// <c>transcription</c>、<c>turnComplete</c> 或 <c>error</c> 傳回。
        /// </summary>
        /// <param name="fnGetLlmOpt">每次連線或重連時呼叫，依目前對話歷史建立 provider 設定。</param>
        /// <param name="fnTurnEnd">有效回合完成後呼叫，參數為使用者文字、助理文字、本回合 token 數及取消權杖；回傳 true 會重建 LLM session。</param>
        /// <param name="maxHistoryTurns">重連時保留的最近對話回合數，每回合包含一筆 user 與一筆 assistant 歷史。</param>
        /// <param name="ct">取消整個橋接工作階段的權杖。</param>
        public async Task BuildBridgeA(
            Func<IReadOnlyCollection<LlmHistoryDto>, LlmOptDto> fnGetLlmOpt,
            Func<LlmTurnResultDto, CancellationToken, Task<bool>> fnTurnEnd,
            int maxHistoryTurns = 10, CancellationToken ct = default,
            Func<LiveLlmToolCallDto, CancellationToken, Task<object>>? fnToolCall = null)
        {
            ArgumentNullException.ThrowIfNull(fnGetLlmOpt);
            ArgumentNullException.ThrowIfNull(fnTurnEnd);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxHistoryTurns);

            // 有界佇列在 provider 重連期間暫存 client 訊息；滿載時以等待形成背壓。
            var messages = Channel.CreateBounded<string>(new BoundedChannelOptions(128)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });
            using var lifeTime = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var history = new List<LlmHistoryDto>();
            var turnBuffer = new AudioTurnBufferSvc();

            // client 接收工作跨越多次 LLM session，確保重連時仍能接收並暫存訊息。
            var uiToWeb = OnUiToWebTurnA(messages.Writer, lifeTime.Token);

            try
            {
                while (_uiSocketSvc.IsOpen && !lifeTime.IsCancellationRequested)
                {
                    turnBuffer.EnableLiveTranscription();
                    // 每次連線都用目前歷史重新產生設定，讓重連後的 provider session 延續對話。
                    await ConnectLlmA(fnGetLlmOpt(history.ToArray()), lifeTime.Token);

                    // 分別控制 LLM 回應接收及 client 訊息送出，批次重連時可獨立停止兩條工作。
                    using var receiverCts = CancellationTokenSource.CreateLinkedTokenSource(lifeTime.Token);
                    using var senderCts = CancellationTokenSource.CreateLinkedTokenSource(lifeTime.Token);
                    var webToLlm = OnWebToLlmTurnA(
                        messages.Reader, turnBuffer, senderCts.Token, lifeTime.Token);
                    var llmToUi = OnLlmToUiTurnA(
                        turnBuffer, history, maxHistoryTurns,
                        fnTurnEnd, senderCts.Cancel, receiverCts.Token, fnToolCall);

                    // 任一端結束便收束目前 session；clientReceive 本身會持續跨越 LLM 重連。
                    var completedTask = await Task.WhenAny(uiToWeb, webToLlm, llmToUi);
                    var shouldReconnect = false;
                    try
                    {
                        if (completedTask == uiToWeb)
                            break;

                        if (completedTask == webToLlm)
                        {
                            await webToLlm;
                            break;
                        }

                        shouldReconnect = await llmToUi;
                    }
                    finally
                    {
                        // 先取消並等待 session 工作結束，再建立下一個 provider socket，避免新舊工作重疊。
                        receiverCts.Cancel();
                        senderCts.Cancel();
                        await WatchTaskA(webToLlm);
                        await WatchTaskA(llmToUi);
                    }

                    if (!shouldReconnect)
                        break;

                    await CloseA(CancellationToken.None);
                }
            }
            // 整個工作階段被取消屬正常結束，不回報錯誤。
            catch (OperationCanceledException) when (lifeTime.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                // 連線中斷時已無法回覆 client，只在 socket 仍開啟時回報。
                if (_uiSocketSvc.IsOpen)
                    await WebToUiErrorA($"Live LLM 連線錯誤：{ex.Message}");
            }
            finally
            {
                // 結束 client 接收佇列並關閉上游 LLM session；client WebSocket 的所有權仍屬呼叫端。
                lifeTime.Cancel();
                messages.Writer.TryComplete();
                await WatchTaskA(uiToWeb);
                await CloseA(CancellationToken.None);
            }
        }

        /// <summary>持續接收 client WebSocket 訊息，依序寫入有界佇列；結束時完成佇列。</summary>
        private async Task OnUiToWebTurnA(ChannelWriter<string> writer, CancellationToken ct)
        {
            _Log.Info("OnUiToWebTurnA");
            try
            {
                while (_uiSocketSvc.IsOpen && !ct.IsCancellationRequested)
                {
                    var msg = await _uiSocketSvc.GetDataA(64 * 1024, ct);
                    // null 代表瀏覽器已關閉連線。
                    if (msg is null) break;
                    await writer.WriteAsync(msg, ct);
                }
            }
            finally
            {
                writer.TryComplete();
            }
        }

        /// <summary>解析標準 client 訊息，並呼叫 provider 對應的音訊或文字傳送操作。</summary>
        private async Task OnWebToLlmTurnA(ChannelReader<string> reader,
            AudioTurnBufferSvc turnBuffer, CancellationToken readCt, CancellationToken sendCt)
        {
            _Log.Info("OnWebToLlmTurnA");

            await foreach (var msg in reader.ReadAllAsync(readCt))
            {
                // 格式錯誤的訊息直接略過，不中斷整個連線。
                JObject input;
                try { input = JObject.Parse(msg); }
                catch { continue; }

                var type = input["type"]?.Value<string>();
                if (type == "audio")
                {
                    var audioData = input["audioData"]?.Value<string>();
                    if (!string.IsNullOrWhiteSpace(audioData))
                        turnBuffer.BeginInputTurn();

                    // 前端可附帶自行辨識的文字；音訊封包則逐包交給 provider。
                    turnBuffer.SetUserText(input["transcribedText"]?.Value<string>() ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(audioData))
                    {
                        var audioBytes = Convert.FromBase64String(audioData);
                        turnBuffer.AppendInputAudio(audioBytes);
                        await WebToLlmAudioA(audioBytes, sendCt);
                    }

                    if (input["frontTurnComplete"]?.Value<bool>() == true)
                    {
                        // 音訊輸入結束時提交回合；沒有前端逐字稿則保留通用備援文字。
                        turnBuffer.SetFallbackUserText("[Audio Input]");
                        turnBuffer.CompleteInputAudio();
                        await WebToLlmAudioEndA(sendCt);
                    }
                }
                else if (type == "text")
                {
                    // 文字訊息同時作為本回合的使用者文字，供歷史與保存使用。
                    var text = input["message"]?.Value<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        turnBuffer.SetUserText(text);
                        await WebToLlmTextA(text, sendCt);
                    }
                }
            }
        }

        /// <summary>
        /// 將 provider 回應映射為 client 訊息，累積逐字稿及歷史，並在完成回合時詢問呼叫端是否重連。
        /// Turn表回合, Socket傳送語音會分成多個trunk, Turn表示多個trunk
        /// </summary>
        private async Task<bool> OnLlmToUiTurnA(
            AudioTurnBufferSvc turnBuffer, List<LlmHistoryDto> history, int maxHistoryTurns,
            Func<LlmTurnResultDto, CancellationToken, Task<bool>> onTurnCompleted,
            Action pauseSender, CancellationToken ct,
            Func<LiveLlmToolCallDto, CancellationToken, Task<object>>? fnToolCall = null)
        {
            //_Log.Info("OnLlmToUiTurnA");   //這裡不寫log, 會搞混

            await foreach (var response in OnLlmToWebTurnA(ct))
            {
                if (!_uiSocketSvc.IsOpen) break;

                if (response.Type == LlmRespTypeEnum.ToolCall && response.ToolCalls != null)
                {
                    // 未回覆工具結果時 provider 會一直等待，因此失敗也要回傳錯誤內容。
                    var toolResps = new List<LiveLlmToolRespDto>();
                    foreach (var call in response.ToolCalls)
                    {
                        object result;
                        try
                        {
                            result = fnToolCall == null
                                ? new { error = "Tool not supported." }
                                : await fnToolCall(call, ct);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            result = new { error = ex.Message };
                        }
                        toolResps.Add(new LiveLlmToolRespDto { Id = call.Id, Name = call.Name, Response = result });
                    }
                    await WebToLlmToolRespA(toolResps, ct);
                    continue;
                }

                // 錯誤事件轉成 error 訊息，但不結束接收迴圈。
                if (response.Type == LlmRespTypeEnum.Error)
                {
                    await WebToUiErrorA(response.Text ?? "Live LLM 回傳錯誤。", ct);
                }
                //中斷
                else if (response.Type == LlmRespTypeEnum.Interrupted)
                {
                    if (turnBuffer.TryInterrupt(out var interruptedTurn))
                        await onTurnCompleted(interruptedTurn, ct);

                    await WebToUiDataA(new { type = "interrupted" }, ct);
                }
                else if (response.Type == LlmRespTypeEnum.Audio && response.Audio != null)
                {
                    turnBuffer.MarkAssistantResponseInProgress();
                    // 音訊以 Base64 傳給前端播放。
                    await WebToUiDataA(new
                    {
                        mimeType = response.MimeType,
                        audioData = Convert.ToBase64String(response.Audio)
                    }, ct);
                }
                else if (response.Type == LlmRespTypeEnum.OutputTranScript &&
                    !string.IsNullOrWhiteSpace(response.Text))
                {
                    turnBuffer.MarkAssistantResponseInProgress();
                    // 逐字稿同時累積到回合緩衝，並即時顯示於前端。
                    turnBuffer.AppendAssistantText(response.Text);
                    await WebToUiDataA(new { type = "transcription", text = response.Text }, ct);
                }
                else if (response.Type == LlmRespTypeEnum.InputTranScript &&
                    !string.IsNullOrWhiteSpace(response.Text))
                {
                    turnBuffer.AppendInputTranScript(response.Text);
                }
                else if (response.Type == LlmRespTypeEnum.Usage)
                {
                    turnBuffer.SetTokens(response.TotalTokens);
                }
                else if (response.Type == LlmRespTypeEnum.Completed)
                {
                    if (turnBuffer.ConsumeInterruptedTail())
                    {
                        await WebToUiDataA(new { type = "turnComplete" }, ct);
                        continue;
                    }

                    var shouldReconnect = false;
                    if (turnBuffer.TryComplete(out var turn))
                    {
                        _Log.Info($"Turn tokens={turn.TotalTokens}");
                        // 將完整回合加進重連歷史，並限制歷史只保留最近指定回合數。
                        history.Add(new LlmHistoryDto { Role = "user", Text = turn.UserText });
                        history.Add(new LlmHistoryDto { Role = "assistant", Text = turn.AssistantText });
                        var maxHistoryMessages = maxHistoryTurns * 2;
                        if (history.Count > maxHistoryMessages)
                            history.RemoveRange(0, history.Count - maxHistoryMessages);

                        // 保存、計數等應用政策交由呼叫端決定是否重連。
                        shouldReconnect = await onTurnCompleted(turn, ct);
                    }

                    if (shouldReconnect)
                    {
                        // 暫停 client-to-LLM sender，讓重連期間到達的訊息留在有界佇列中。
                        pauseSender();
                        await WebToUiDataA(new { type = "turnComplete" }, ct);
                        return true;
                    }

                    // 未重連時只通知前端本回合結束，繼續使用同一個 provider session。
                    await WebToUiDataA(new { type = "turnComplete" }, ct);

                    _Log.Info("WebToUiDataA Completed");
                }
            }

            // 接收迴圈自然結束（provider 關閉）時不重連。
            return false;
        }

        /// <summary>以標準 error 訊息格式回覆 client。</summary>
        private Task WebToUiErrorA(string message,
            CancellationToken ct = default)
        {
            return WebToUiDataA(new { type = "error", message }, ct);
        }

        /// <summary>從上游 WebSocket 接收並組合一個完整訊息。</summary>
        /// <param name="maxMsgBytes">單一訊息允許的最大位元組數。</param>
        /// <param name="ct">取消接收操作的權杖。</param>
        /// <returns>完整訊息內容；對端關閉連線時回傳 <see langword="null"/>。</returns>
        protected Task<string?> LlmToWebDataA(int maxMsgBytes = 256 * 1024, CancellationToken ct = default)
        {
            return _llmSocketSvc?.GetDataA(maxMsgBytes, ct)
                ?? throw new InvalidOperationException("Live LLM is not connected.");
        }

        /// <summary>將 provider payload 序列化為 JSON，並透過上游 WebSocket 傳送。</summary>
        /// <param name="payload">要序列化並傳送的 provider payload。</param>
        /// <param name="ct">取消傳送操作的權杖。</param>
        protected Task WebToLlmDataA(object payload, CancellationToken ct = default)
        {
            return _llmSocketSvc?.SendDataA(payload, ct)
                ?? throw new InvalidOperationException("Gemini Live is not connected.");
        }

        /// <summary>釋放上游 WebSocket 與其傳輸包裝資源。</summary>
        public async ValueTask DisposeAsync()
        {
            _llmSocketSvc?.Dispose();
            _llmSocketSvc = null!;

            await ValueTask.CompletedTask;
            GC.SuppressFinalize(this);
        }

    }
}
