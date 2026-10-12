using Base.Services;
using BaseAI.Enums;
using BaseAI.Models;
using BaseAI.Services;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading.Channels;

namespace BaseAI.Interfaces
{
    /// <summary>
    /// Live LLM provider 工作階段的抽象基底類別。
    /// 定義 provider 連線、音訊／文字／工具收送及事件接收，並提供標準 client WebSocket 橋接流程。
    /// 呼叫端仍負責提供應用程式設定與每回合的保存政策。
    /// </summary>
    /// <param name="liveLlmUrl">Live LLM provider 的 WebSocket endpoint。</param>
    /// <param name="llmSvc">提供共用語音辨識功能的 LLM 服務。</param>
    public abstract class AbLiveLlmSvc(string liveLlmUrl, AbLlmSvc llmSvc) : IAsyncDisposable
    {
        #region 變數
        /// <summary>瀏覽器端 WebSocket；由 StartA 接受連線，並在工作階段結束時關閉及釋放。</summary>
        private string _audioInput = string.Empty;

        /// <summary>瀏覽器端 WebSocket；由 StartA 接受連線，並在工作階段結束時關閉及釋放。</summary>
        private WebSocketSvc _uiSocketSvc = null!;

        /// <summary>每個完整或中斷回合完成後通知應用層的處理器。</summary>
        private Func<LlmTurnResultDto, CancellationToken, Task>? _fnTurnFinishA;

        /// <summary>模型要求執行工具時，供應用層處理並回傳結果的處理器。</summary>
        private Func<LlmToolCallDto, CancellationToken, Task<object>>? _fnToolCallA;
        private Func<IReadOnlyCollection<LlmHistoryDto>>? _fnGetHistoryA;

        /// <summary>提供共用語音辨識功能，作為音訊回合缺少可用逐字稿時的備援。</summary>
        private readonly AbLlmSvc _llmSvc = llmSvc
            ?? throw new ArgumentNullException(nameof(llmSvc));

        /// <summary>server 連往 Live LLM provider 的 WebSocket endpoint，供 provider 實作建立上游連線。</summary>
        protected string _liveLlmUrl = liveLlmUrl;

        /// <summary>封裝 server 端的上游 provider WebSocket，供 provider 實作收送原生協定資料。</summary>
        protected WebSocketSvc _llmSocketSvc = new();

        /// <summary>判斷上游 Live LLM WebSocket 是否已連線並處於可傳輸資料的開啟狀態。</summary>
        public bool IsOpen => _llmSocketSvc?.IsOpen == true;
        #endregion

        #region 以下函數各家LLM做法不同, 宣告抽象, 各自實作
        /// <summary>
        /// 建立或重建上游 provider WebSocket，套用模型、提示詞、歷史與工具設定，並完成 provider 所需的工作階段初始化。
        /// 子類別負責將共用設定轉換成 provider 專屬的協定訊息。
        /// </summary>
        /// <param name="optDto">本次連線要套用的模型、提示詞、語音、工具及對話歷史設定。</param>
        /// <param name="ct">取消上游連線或 provider 初始化的權杖。</param>
        public abstract Task ConnectLlmA(LiveLlmOptDto optDto, CancellationToken ct = default);

        /// <summary>依 provider 所需格式，將一段前端送入的原始音訊傳送至上游模型。</summary>
        /// <param name="audioMem">已解碼的音訊位元組；格式、取樣率及聲道須符合目前 provider 設定。</param>
        /// <param name="ct">取消等待或傳送此音訊資料的權杖。</param>
        public abstract Task WebToLlmAudioA(ReadOnlyMemory<byte> audioMem, CancellationToken ct = default);

        /// <summary>通知 provider 前端音訊輸入已結束，讓 provider 將累積音訊提交為一個使用者回合。</summary>
        /// <param name="ct">取消提交音訊回合操作的權杖。</param>
        public abstract Task WebToLlmAudioEndA(CancellationToken ct = default);

        /// <summary>將前端提供的使用者文字作為一筆輸入送至上游模型。</summary>
        /// <param name="text">要提交給模型的使用者文字內容。</param>
        /// <param name="ct">取消等待或傳送文字操作的權杖。</param>
        public abstract Task WebToLlmTextA(string text, CancellationToken ct = default);
        /// <summary>將本回合開始前的對話歷史加入 provider 的即時對話上下文。</summary>
        public abstract Task WebToLlmHistoryA(
            IReadOnlyCollection<LlmHistoryDto> history, CancellationToken ct = default);

        /// <summary>將應用層完成的工具呼叫結果回送給 provider，讓模型得以繼續產生回應。</summary>
        /// <param name="responses">包含工具呼叫識別碼、名稱及執行結果的回覆集合。</param>
        /// <param name="ct">取消等待或傳送工具結果的權杖。</param>
        public abstract Task WebToLlmToolRespA(IEnumerable<LlmToolRespDto> responses, CancellationToken ct = default);

        /// <summary>
        /// 持續讀取並解析 provider 上游事件，轉成共用 DTO，供橋接流程映射為瀏覽器訊息或內部回合資料。
        /// </summary>
        /// <param name="ct">取消 provider 事件接收迴圈的權杖。</param>
        /// <returns>依序輸出的音訊、輸入/輸出逐字稿、工具呼叫、錯誤、用量或回合完成事件。</returns>
        public abstract IAsyncEnumerable<LlmRespDto> OnLlmToWebTurnA(CancellationToken ct = default);
        #endregion

        #region 開始-結束
        /// <summary>
        /// 接受 UI WebSocket，並以指定設定啟動 client 與 Live LLM 的雙向橋接；依回合數重建 LLM session。
        /// client 音訊訊息使用 <c>type=audio</c>、Base64 <c>audioData</c> 及 <c>frontTurnComplete</c>；
        /// 文字訊息使用 <c>type=text</c> 和 <c>message</c>。LLM 回應轉為 <c>audioData</c>、
        /// <c>transcription</c>、<c>turnComplete</c> 或 <c>error</c> 傳回。
        /// </summary>
        /// <param name="httpContext">目前的 ASP.NET Core HTTP context。</param>
        /// <param name="llmOpt">此工作階段固定的 provider 設定；逐題歷史由 fnGetHistoryA 提供。</param>
        /// <param name="fnTurnFinishA">整理完成的回合通知上層處理器。</param>
        /// <param name="fnBeforeReconnectA">回合佇列排空後、重建 provider session 前執行的保存處理器。</param>
        /// <param name="fnToolCallA">模型要求工具時通知上層處理器。</param>
        /// <param name="maxHistories">每題最多附加的歷史問答回合數。</param>
        /// <param name="reconnectTurns">回合處理器完成指定筆數（包含合併後的中斷回合）後重建 provider session。</param>
        /// <param name="ct">取消整個工作階段的權杖。</param>
        public async Task StartA(
            HttpContext httpContext,
            LiveLlmOptDto llmOpt,
            Func<LlmTurnResultDto, CancellationToken, Task> fnTurnFinishA,
            Func<CancellationToken, Task<bool>> fnBeforeReconnectA,
            Func<LlmToolCallDto, CancellationToken, Task<object>> fnToolCallA,
            string audioInput, int maxHistories = 10, int reconnectTurns = 3,
            CancellationToken ct = default,
            Func<IReadOnlyCollection<LlmHistoryDto>>? fnGetHistoryA = null,
            Func<LlmTurnResultDto, bool>? fnCountReconnectTurnA = null)
        {
            // 先檢查所有必要依賴與正整數設定，避免 WebSocket 升級後才發現呼叫參數無效。
            ArgumentNullException.ThrowIfNull(httpContext);
            ArgumentNullException.ThrowIfNull(llmOpt);
            ArgumentNullException.ThrowIfNull(fnTurnFinishA);
            ArgumentNullException.ThrowIfNull(fnBeforeReconnectA);
            ArgumentNullException.ThrowIfNull(fnToolCallA);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxHistories);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(reconnectTurns);

            _audioInput = audioInput;

            // 非 WebSocket 請求無法進入雙向橋接；在接受連線前以 HTTP 400 結束請求。
            if (!httpContext.WebSockets.IsWebSocketRequest)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(
                    new { error = "WebSocket upgrade is required." }, ct);
                return;
            }

            _uiSocketSvc = new WebSocketSvc();
            try
            {
                await _uiSocketSvc.AcceptA(httpContext);
                _fnTurnFinishA = fnTurnFinishA;
                _fnToolCallA = fnToolCallA;
                _fnGetHistoryA = fnGetHistoryA;
                await RunSessionA(
                    llmOpt, maxHistories, reconnectTurns, fnBeforeReconnectA,
                    fnCountReconnectTurnA, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                var message = $"Llm Live 連線失敗：{ex.Message}";
                if (_uiSocketSvc.IsOpen)
                {
                    await WebToUiErrorA(message);
                    await _uiSocketSvc.CloseA(WebSocketCloseStatus.InternalServerError,
                        "Llm Live connection failed");
                }
                else if (!httpContext.Response.HasStarted)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await httpContext.Response.WriteAsJsonAsync(new { error = message });
                }
                else
                {
                    _Log.Error("AbLiveLlmSvc failed after response started: " + ex.Message);
                }
            }
            finally
            {
                _fnTurnFinishA = null;
                _fnToolCallA = null;
                _fnGetHistoryA = null;
                try
                {
                    if (_uiSocketSvc.IsOpen)
                        await _uiSocketSvc.CloseA(WebSocketCloseStatus.NormalClosure,
                            "Live chat session ended");
                }
                finally
                {
                    _uiSocketSvc.Dispose();
                    _uiSocketSvc = null!;
                }
            }
        }

        /// <summary>
        /// 維持 UI、server 與 LLM 之間的背景橋接：接收並轉送訊息、累積對話歷史，並在指定回合數後重連 provider。
        /// provider session 重連或結束時會先收束並排空回合佇列；UI socket 則由外層 StartA 負責釋放。
        /// </summary>
        /// <param name="llmOpt">固定的模型與對話設定；每次上游重連時會複製並附加目前歷史。</param>
        /// <param name="maxHistories">歷史最多保留的完整 user/assistant 回合數。</param>
        /// <param name="reconnectTurns">回合 consumer 處理多少筆回合後要求重建上游 provider session。</param>
        /// <param name="fnBeforeReconnectA">回合佇列排空後、重建 provider session 前執行的保存處理器。</param>
        /// <param name="ct">取消整體瀏覽器與模型橋接工作的權杖。</param>
        private async Task RunSessionA(
            LiveLlmOptDto llmOpt, int maxHistories, int reconnectTurns,
            Func<CancellationToken, Task<bool>> fnBeforeReconnectA,
            Func<LlmTurnResultDto, bool>? fnCountReconnectTurnA, CancellationToken ct)
        {
            //_Log.Debug("AbLiveLlmSvc InitA");

            // 以下佇列與 UI 接收工作存活於整個 Live session；provider 重連時保留 client 已送達的訊息。
            // 單一 writer/reader 對應各自唯一的接收或送出工作；有界 messages queue 滿載時會等待，形成背壓。
            var messages = Channel.CreateBounded<string>(new BoundedChannelOptions(128)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });
            // 取消來源涵蓋整個 Live session，並連結 HTTP 請求取消；每個 provider session 另有自己的取消來源。
            using var lifeTime = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // turnBuffer 只累積目前正在處理的使用者/模型回合；歷史由呼叫端逐題提供。
            var turnBuffer = new AudioTurnBufferSvc(_audioInput);

            // client 接收工作跨越多次 provider session，避免重連期間停止讀取造成訊息遺失或背壓無法排空。
            var uiToWebTurn = OnUiToWebTurnA(messages.Writer, lifeTime.Token);

            // 呼叫端取消整段工作階段是正常的結束方式，不再轉成錯誤訊息回報。
            try
            {
                // 每次迴圈代表一個 provider session；provider 重連後的歷史由呼叫端 queue 規則重設。
                while (_uiSocketSvc.IsOpen && !lifeTime.IsCancellationRequested)
                {
                    turnBuffer.EnableLiveTranScript();

                    // WebSocket 已升級時不能再回傳 HTTP status，改透過 WebSocket 傳錯誤並關閉連線。
                    // 每次 provider socket 建立時不載入舊歷史；每一題的歷史由輸入工作即時附加。
                    await ConnectLlmA(new LiveLlmOptDto
                    {
                        EndPoint = llmOpt.EndPoint,
                        Model = llmOpt.Model,
                        VoiceName = llmOpt.VoiceName,
                                    // 尚未升級且 HTTP response 尚未送出時，仍可用 HTTP 500 回報初始化失敗。
                        SystemPrompt = llmOpt.SystemPrompt,
                        Language = llmOpt.Language,
                        AudioResponse = llmOpt.AudioResponse,
                        Tools = llmOpt.Tools,
                        History = Array.Empty<LlmHistoryDto>()
                                    // Response 已開始但 WebSocket 尚未開啟時，無法再更改狀態或回傳錯誤本文，只能記錄。
                    }, lifeTime.Token);

                    var turns = Channel.CreateUnbounded<LlmTurnResultDto>(new UnboundedChannelOptions
                    {
                        SingleReader = true,
                        SingleWriter = true
                    });
                    var reconnectRequested = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);
                    var turnConsumer = ProcessTurnQueueA(
                        turns.Reader, reconnectTurns, reconnectRequested,
                        fnCountReconnectTurnA);

                    // 這兩項工作只屬於目前 provider session：receiver 控制 provider 事件接收，sender 控制 client 訊息轉送。
                    using var receiverCts = CancellationTokenSource.CreateLinkedTokenSource(lifeTime.Token);
                    using var senderCts = CancellationTokenSource.CreateLinkedTokenSource(lifeTime.Token);
                                // 清除 delegate，避免 session 結束後仍保留呼叫端物件；socket 關閉失敗時仍會進入 Dispose。
                    var webToLlmTurn = OnWebToLlmTurnA(
                        messages.Reader, turnBuffer, maxHistories, senderCts.Token, lifeTime.Token);
                    var llmToUiTurn = OnLlmToUiTurnA(
                        turnBuffer, turns.Writer, receiverCts.Token);

                    // queue 處理器達到門檻時提出重連要求；其他工作結束則收束整段 Live session。
                    var completedTask = await Task.WhenAny(
                        uiToWebTurn, webToLlmTurn, llmToUiTurn, reconnectRequested.Task);
                    var shouldReconnect = completedTask == reconnectRequested.Task;
                    var shouldStop = false;
                    try
                    {
                        if (completedTask == uiToWebTurn)
                            shouldStop = true;

                        else if (completedTask == webToLlmTurn)
                        {
                            await webToLlmTurn;
                            shouldStop = true;
                        }
                        else if (completedTask == llmToUiTurn)
                            await llmToUiTurn;
                    }
                    finally
                    {
                        // 先停止目前 session 的接收/送出工作並等待它們結束，避免舊工作仍使用 socket 時建立新連線。
                        receiverCts.Cancel();
                        senderCts.Cancel();
                        await WatchTaskA(webToLlmTurn);
                        await WatchTaskA(llmToUiTurn);

                        // 確認不會再有新回合後排空 queue，避免 consumer 留在背景或遺失中斷尾段。
                        turns.Writer.TryComplete();
                        await WatchTaskA(turnConsumer);
                    }

                    // Queue 排空期間可能剛好處理到門檻回合，讀取最終重連決策。
                    shouldReconnect |= reconnectRequested.Task.IsCompletedSuccessfully;

                    if (!shouldReconnect)
                        break;

                    // 佇列已排空，現在保存包含觸發門檻回合在內的 pending talks；失敗則不重連。
                    if (!await fnBeforeReconnectA(lifeTime.Token))
                        break;

                    // provider session 已完全收束，才正常關閉舊上游 socket 並回到迴圈頂端重新連線。
                    await CloseA(CancellationToken.None);

                    if (shouldStop)
                        break;
                }
            }
            // 整個工作階段被取消屬正常結束，不回報錯誤。
            catch (OperationCanceledException) when (lifeTime.IsCancellationRequested)
            {
                _Log.Debug("AbLiveLlmSvc 取消");
            }
            catch (Exception ex)
            {
                // 連線中斷時已無法回覆 client，只在 socket 仍開啟時回報。
                if (_uiSocketSvc.IsOpen)
                    await WebToUiErrorA($"Live LLM 連線錯誤：{ex.Message}");
            }
            finally
            {
                // 整體結束時停止跨 session 的 client reader，關閉佇列讓回合 consumer 排空後再結束。
                // UI socket 由外層 StartA 擁有並釋放；本方法只負責關閉上游 provider socket。
                lifeTime.Cancel();
                messages.Writer.TryComplete();
                try
                {
                    await WatchTaskA(uiToWebTurn);
                    // 工作階段可能在未達重連門檻時結束，仍需保存 queue 已累積的回合。
                    await fnBeforeReconnectA(CancellationToken.None);
                }
                finally
                {
                    await CloseA(CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// 等待一個橋接背景工作結束；取消視為正常收束，其他例外在此吸收，避免與外層錯誤回報重複。
        /// </summary>
        /// <param name="task">要等待收束的背景工作。</param>
        private static async Task WatchTaskA(Task task)
        {
            try { await task; }
            // 取消屬正常收束；其他例外已由外層連線錯誤處理回報，此處不重複拋出。
            catch (OperationCanceledException) { }
            catch { }
        }

        /// <summary>以正常關閉狀態送出上游 provider WebSocket close frame；尚未建立上游 socket 時不做事。</summary>
        /// <param name="ct">保留供呼叫端使用的取消權杖；close frame 目前由 WebSocketSvc 執行。</param>
        public virtual async Task CloseA(CancellationToken ct = default)
        {
            _Log.Debug("AbLiveLlmSvc CloseA");

            if (_llmSocketSvc != null)
                await _llmSocketSvc.CloseA(WebSocketCloseStatus.NormalClosure, "Live LLM session ended");
        }

        /// <summary>
        /// 釋放上游 provider socket 與傳輸包裝資源，並釋放目前持有的 UI socket；可安全用於非同步 using。
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _Log.Debug("AbLiveLlmSvc DisposeAsync");

            _llmSocketSvc?.Dispose();
            _llmSocketSvc = null!;
            _uiSocketSvc?.Dispose();
            _uiSocketSvc = null!;

            await ValueTask.CompletedTask;
            GC.SuppressFinalize(this);
        }
        #endregion

        #region UI-Web-LLM 處理程式
        /// <summary>將橋接事件或資料序列化為 JSON，並透過目前的瀏覽器 WebSocket 傳送。</summary>
        /// <param name="payload">要傳給前端的事件或資料物件。</param>
        /// <param name="ct">取消等待或傳送操作的權杖。</param>
        private Task WebToUiDataA(object payload, CancellationToken ct = default)
        {
            return _uiSocketSvc.SendDataA(payload, ct);
        }

        /// <summary>
        /// 持續讀取瀏覽器 WebSocket 訊息，依序寫入有界佇列供 provider sender 消費；連線結束或取消時完成 writer。
        /// 此工作跨越 provider 重連，因此不隨單一上游 session 停止。
        /// </summary>
        /// <param name="writer">接收原始瀏覽器訊息的佇列 writer。</param>
        /// <param name="ct">取消瀏覽器訊息接收工作的權杖。</param>
        private async Task OnUiToWebTurnA(ChannelWriter<string> writer, CancellationToken ct)
        {
            _Log.Debug("AbLiveLlmSvc Start OnUiToWebTurnA");

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

        /// <summary>
        /// 依序解析 client 訊息，將 audio/text 輸入送至 provider，並更新回合緩衝中的逐字稿、音訊及提交狀態。
        /// 無法解析的 JSON 訊息會略過，不中止後續訊息處理。
        /// </summary>
        /// <param name="reader">保存瀏覽器原始訊息的佇列 reader。</param>
        /// <param name="turnBuffer">累積目前使用者輸入音訊與文字的回合緩衝。</param>
        /// <param name="readCt">取消佇列讀取的權杖，通常只涵蓋目前 provider session。</param>
        /// <param name="sendCt">取消將音訊或文字送至 provider 的權杖。</param>
        private async Task OnWebToLlmTurnA(ChannelReader<string> reader,
            AudioTurnBufferSvc turnBuffer, int maxHistoryTurns,
            CancellationToken readCt, CancellationToken sendCt)
        {
            _Log.Debug("AbLiveLlmSvc Start OnWebToLlmTurnA");
            var historySentOnSocket = false;
            var audioTurnStarted = false;

            async Task SendHistoryA(bool includeHistory)
            {
                if (!includeHistory || historySentOnSocket)
                {
                    turnBuffer.SetHistorySnapshot(Array.Empty<LlmHistoryDto>());
                    return;
                }

                var history = (_fnGetHistoryA?.Invoke() ?? Array.Empty<LlmHistoryDto>())
                    .TakeLast(maxHistoryTurns * 2).ToArray();
                turnBuffer.SetHistorySnapshot(history);

                if (history.Length > 0)
                    await WebToLlmHistoryA(history, sendCt);
                historySentOnSocket = true;
            }

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
                        if (!audioTurnStarted)
                        {
                            await SendHistoryA(includeHistory: true);
                            audioTurnStarted = true;
                        }
                        turnBuffer.AppendInputAudio(audioBytes);
                        await WebToLlmAudioA(audioBytes, sendCt);
                    }

                    if (input["frontTurnComplete"]?.Value<bool>() == true)
                    {
                        // 音訊輸入結束時提交回合；沒有前端逐字稿則保留通用備援文字。
                        turnBuffer.SetFallbackUserText(_audioInput);
                        turnBuffer.FinishInputAudio();
                        await WebToLlmAudioEndA(sendCt);
                        audioTurnStarted = false;
                    }
                }
                else if (type == "text")
                {
                    // 文字訊息同時作為本回合的使用者文字，供歷史與保存使用。
                    var text = input["message"]?.Value<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        turnBuffer.SetUserText(text);
                        await SendHistoryA(
                            includeHistory: !text.StartsWith("【系統觸發】", StringComparison.Ordinal));
                        await WebToLlmTextA(text, sendCt);
                    }
                }
            }
        }

        /// <summary>
        /// 讀取目前 provider session 的事件，將音訊、逐字稿、錯誤及完成狀態轉成瀏覽器訊息，並累積回合歷史。
        /// 完整或中斷回合會排入背景 consumer；provider session 的重連門檻由 consumer 統一判斷。
        /// </summary>
        /// <param name="turnBufferSvc">累積目前輸入及模型回應內容的回合緩衝。</param>
        /// <param name="history">跨 provider 重連保存的 user/assistant 歷史，會在完成回合時更新。</param>
        /// <param name="maxHistoryTurns">歷史中最多保留的完整對話回合數。</param>
        /// <param name="turnWriter">將完成或中斷回合交給背景 consumer 的佇列 writer。</param>
        /// <param name="ct">取消目前 provider 事件接收工作的權杖。</param>
        private async Task OnLlmToUiTurnA(
            AudioTurnBufferSvc turnBufferSvc, ChannelWriter<LlmTurnResultDto> turnWriter,
            CancellationToken ct)
        {
            _Log.Debug("AbLiveLlmSvc Start OnLlmToUiTurnA");   //這裡不寫log, 會搞混
            string? inputTranscriptText = null;

            try
            {
                await foreach (var respDto in OnLlmToWebTurnA(ct))
                {
                    if (!_uiSocketSvc.IsOpen) break;

                    //case Tool Call
                    if (respDto.Type == LlmRespTypeEnum.ToolCall && respDto.ToolCalls != null)
                    {
                        turnBufferSvc.MarkLlmResping();
                        // 未回覆工具結果時 provider 會一直等待，因此失敗也要回傳錯誤內容。
                        var toolResps = new List<LlmToolRespDto>();
                        foreach (var call in respDto.ToolCalls)
                        {
                            object result;
                            try
                            {
                                result = await (_fnToolCallA
                                    ?? throw new InvalidOperationException("Live session handlers are not configured."))(call, ct);
                            }
                            catch (Exception ex) when (ex is not OperationCanceledException)
                            {
                                result = new { error = ex.Message };
                            }
                            toolResps.Add(new LlmToolRespDto { Id = call.Id, Name = call.Name, Response = result });
                        }
                        await WebToLlmToolRespA(toolResps, ct);
                        continue;
                    }

                    // 錯誤事件轉成 error 訊息，但不結束接收迴圈。
                    if (respDto.Type == LlmRespTypeEnum.Error)
                    {
                        await WebToUiErrorA(respDto.Text ?? "Live LLM 回傳錯誤。", ct);
                    }
                    //中斷
                    else if (respDto.Type == LlmRespTypeEnum.Interrupted)
                    {
                        if (turnBufferSvc.TryGetInterruptedTurn(out var interruptedTurn, inputTranscriptText))
                            TurnToQueueA(turnWriter, interruptedTurn);

                        inputTranscriptText = null;
                        turnBufferSvc.ResetForInterruptedResponse();

                        await WebToUiDataA(new { type = "interrupted" }, ct);
                    }
                    //語音
                    else if (respDto.Type == LlmRespTypeEnum.Audio && respDto.Audio != null)
                    {
                        turnBufferSvc.MarkLlmResping();
                        // 音訊以 Base64 傳給前端播放。
                        await WebToUiDataA(new
                        {
                            mimeType = respDto.MimeType,
                            audioData = Convert.ToBase64String(respDto.Audio)
                        }, ct);
                    }
                    //輸出轉譯文字
                    else if (respDto.Type == LlmRespTypeEnum.OutputTranScript &&
                        !string.IsNullOrWhiteSpace(respDto.Text))
                    {
                        turnBufferSvc.MarkLlmResping();
                        // 逐字稿同時累積到回合緩衝，並即時顯示於前端。
                        turnBufferSvc.AppendLlmText(respDto.Text);
                        await WebToUiDataA(new { type = "transcription", text = respDto.Text }, ct);
                    }
                    //輸入轉譯文字
                    else if (respDto.Type == LlmRespTypeEnum.InputTranScript &&
                        !string.IsNullOrWhiteSpace(respDto.Text))
                    {
                        inputTranscriptText = respDto.Text;
                        await WebToUiDataA(new { type = "transcription", text = respDto.Text }, ct);
                    }
                    //計算token
                    else if (respDto.Type == LlmRespTypeEnum.Usage)
                    {
                        turnBufferSvc.SetTokens(respDto.TotalTokens);
                    }
                    //資料傳送完成
                    else if (respDto.Type == LlmRespTypeEnum.Completed)
                    {
                        if (turnBufferSvc.ConsumeBreakTail())
                        {
                            await WebToUiDataA(new { type = "turnComplete" }, ct);
                            continue;
                        }

                        if (turnBufferSvc.TryComplete(out var turn, inputTranscriptText))
                        {
                            _Log.Debug($"AbLiveLlmSvc Turn tokens={turn.TotalTokens}");
                            TurnToQueueA(turnWriter, turn);
                        }
                        inputTranscriptText = null;

                        // 是否需重連由背景回合 consumer 決定；此處完成 UI 通知後繼續讀取 provider 事件。
                        await WebToUiDataA(new { type = "turnComplete" }, ct);

                        _Log.Debug("AbLiveLlmSvc OnLlmToUiTurnA Completed");
                    }
                }
            }
            finally
            {
                // Provider 關閉或 session 被取消時，保存尚未收到 Completed 的使用者回合。
                if (turnBufferSvc.TryBreak(out var pendingTurn, inputTranscriptText))
                {
                    _Log.Debug("AbLiveLlmSvc Flush pending turn after receive loop ended.");
                    TurnToQueueA(turnWriter, pendingTurn);
                }
            }

        }

        /// <summary>以橋接協定的標準 error 事件格式回覆瀏覽器 client。</summary>
        /// <param name="message">要提供給前端的錯誤訊息。</param>
        /// <param name="ct">取消錯誤訊息傳送的權杖。</param>
        public Task WebToUiErrorA(string message, CancellationToken ct = default)
        {
            _Log.Debug("AbLiveLlmSvc WebToUiErrorA");
            return WebToUiDataA(new { type = "error", message }, ct);
        }

        /// <summary>從目前 provider 的上游 WebSocket 接收資料，並由 WebSocketSvc 組合成一個完整訊息。</summary>
        /// <param name="maxMsgBytes">單一訊息允許的最大位元組數。</param>
        /// <param name="ct">取消接收操作的權杖。</param>
        /// <returns>完整訊息內容；對端關閉連線時回傳 <see langword="null"/>。</returns>
        protected Task<string?> LlmToWebDataA(int maxMsgBytes = 256 * 1024, CancellationToken ct = default)
        {
            return _llmSocketSvc?.GetDataA(maxMsgBytes, ct)
                ?? throw new InvalidOperationException("Live LLM is not connected.");
        }

        /// <summary>將 provider 專屬 payload 序列化為 JSON，並透過上游 WebSocket 傳送。</summary>
        /// <param name="payload">要序列化並傳送的 provider payload。</param>
        /// <param name="ct">取消傳送操作的權杖。</param>
        protected Task WebToLlmDataA(object payload, CancellationToken ct = default)
        {
            return _llmSocketSvc?.SendDataA(payload, ct)
                ?? throw new InvalidOperationException("Gemini Live is not connected.");
        }
        #endregion

        #region Trun, Queue 功能
        /// <summary>將一筆完成或中斷的回合排入背景處理佇列；佇列已關閉時會擲回例外。</summary>
        /// <param name="writer">供回合 consumer 讀取的佇列 writer。</param>
        /// <param name="turn">要交由應用層處理及保存的回合結果。</param>
        private static void TurnToQueueA(ChannelWriter<LlmTurnResultDto> writer, LlmTurnResultDto turn)
        {
            if (!writer.TryWrite(turn))
                throw new InvalidOperationException("Live turn queue is closed.");
        }

        /// <summary>
        /// 以單一 consumer 順序處理每筆完成或中斷回合，必要時執行語音辨識備援，再呼叫回合完成處理器。
        /// callback 完成後計數；達到門檻時通知 session loop，並在 queue 排空後保存及重連。
        /// </summary>
        /// <param name="reader">接收 provider 完成或中斷回合的佇列 reader。</param>
        /// <param name="reconnectTurns">達到此回合數時通知 session loop 準備重連。</param>
        /// <param name="reconnectRequested">通知 session loop 重連要求的完成來源。</param>
        private async Task ProcessTurnQueueA(
            ChannelReader<LlmTurnResultDto> reader,
            int reconnectTurns,
            TaskCompletionSource<bool> reconnectRequested,
            Func<LlmTurnResultDto, bool>? fnCountReconnectTurnA)
        {
            var processedTurns = 0;
            async Task ProcessTurnA(LlmTurnResultDto turn)
            {
                if (turn.IsBreak && string.Equals(turn.LlmText, "[Interrupted]", StringComparison.Ordinal))
                {
                    _Log.Debug("AbLiveLlmSvc Skip interrupted input without an assistant response.");
                    return;
                }

                var userText = turn.UserText;
                var hasOkUserText = !string.IsNullOrWhiteSpace(userText) && userText != _audioInput;

                // 中斷或標記需辨識的回合若有音訊，嘗試用 STT 補出使用者文字；既有有效文字優先保留。
                if ((turn.IsBreak || turn.NeedSpeechToText) && !hasOkUserText && turn.InputAudio.Length > 0)
                {
                    try
                    {
                        // STT 設定獨立 20 秒上限，避免辨識服務無限等待而卡住整個回合佇列。
                        using var sttCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                        var transcript = await _llmSvc.SpeechToTextA(
                            turn.InputAudio, "audio/pcm;rate=16000", sttCts.Token);
                        if (!hasOkUserText && !string.IsNullOrWhiteSpace(transcript))
                            userText = transcript;
                    }
                    catch (Exception ex)
                    {
                        _Log.Error("AbLiveLlmSvc SpeechToTextA fallback failed: " + ex.Message);
                    }
                }

                var hasUserTextAfterFallback =
                    !string.IsNullOrWhiteSpace(userText) && userText != _audioInput;
                var hasAssistantText =
                    !string.IsNullOrWhiteSpace(turn.LlmText) &&
                    !string.Equals(turn.LlmText, "[Interrupted]", StringComparison.Ordinal);
                if (!hasUserTextAfterFallback && !hasAssistantText)
                {
                    _Log.Debug("AbLiveLlmSvc Skip empty interrupted turn.");
                    return;
                }

                var completedTurn = turn with { UserText = userText };

                // 每筆回合依序通知應用層保存；單筆 callback 失敗只記錄，不中斷後續佇列處理。
                try
                {
                    await (_fnTurnFinishA
                        ?? throw new InvalidOperationException("Live session handlers are not configured."))(
                        completedTurn, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _Log.Error("AbLiveLlmSvc OnTurnFinishA failed: " + ex.Message);
                }

                // 只計算呼叫端認定的有效使用者問答，避免系統提示或占位中斷回合觸發重連。
                if ((fnCountReconnectTurnA?.Invoke(completedTurn) ?? true) &&
                    ++processedTurns >= reconnectTurns)
                    reconnectRequested.TrySetResult(true);
            }

            // 單一 reader 保持回合順序；中斷回合也立即交給 callback 並計入重連門檻。
            await foreach (var turn in reader.ReadAllAsync())
            {
                await ProcessTurnA(turn);
            }
        }
        #endregion

    }//class
}
