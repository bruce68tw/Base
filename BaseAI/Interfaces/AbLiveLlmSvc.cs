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
        /// <summary>default audio input text</summary>
        private string _defAudioInput = string.Empty;

        /// <summary>瀏覽器端 WebSocket；由 StartA 接受連線，並在工作階段結束時關閉及釋放。</summary>
        private WebSocketSvc _uiSocketSvc = null!;

        /// <summary>每個完整或中斷回合完成後通知應用層的處理器。</summary>
        private Func<LlmTurnResultDto, CancellationToken, Task<bool>>? _fnTurnEndA;

        /// <summary>模型要求執行工具時，供應用層處理並回傳結果的處理器。</summary>
        private Func<LlmToolCallDto, CancellationToken, Task<object>>? _fnToolCallA;

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

        #region 以下7個函數各家LLM做法不同, 宣告為抽象, 由子代各自實作
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
            List<LlmHistoryDto> histories, CancellationToken ct = default);

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
        /// 接受 UI WebSocket，並以指定設定啟動 client 與 Live LLM 的雙向橋接；依上層決策重建 LLM session。
        /// client 音訊訊息使用 <c>type=audio</c>、Base64 <c>audioData</c> 及 <c>frontTurnComplete</c>；
        /// 文字訊息使用 <c>type=text</c> 和 <c>message</c>。LLM 回應轉為 <c>audioData</c>、
        /// <c>transcription</c>、<c>turnComplete</c> 或 <c>error</c> 傳回。
        /// </summary>
        /// <param name="httpContext">目前的 ASP.NET Core HTTP context。</param>
        /// <param name="llmOpt">此工作階段固定的 provider 設定。</param>
        /// <param name="fnTurnEndA">整理並處理完成回合；注意, 回傳 true 表示要求重建 provider session!!</param>
        /// <param name="fnBeforeReconnectA">回合佇列排空後、重建 provider session 前執行的保存處理器。</param>
        /// <param name="fnToolCallA">模型要求工具時通知上層處理器。</param>
        /// <param name="ct">取消整個工作階段的權杖。</param>
        /// <param name="fnGetHistories">依輸入文字提供 history；回傳 null 表示略過本次且不消耗 socket 的 history 送出機會。</param>
        public async Task StartA(
            HttpContext httpContext,
            LiveLlmOptDto llmOpt,
            Func<LlmTurnResultDto, CancellationToken, Task<bool>> fnTurnEndA,
            Func<LlmToolCallDto, CancellationToken, Task<object>>? fnToolCallA = null,
            Func<CancellationToken, Task<bool>>? fnBeforeReconnectA = null,
            Func<string?, List<LlmHistoryDto>?>? fnGetHistories = null,
            CancellationToken ct = default)
        {
            // 先檢查所有必要依賴與正整數設定，避免 WebSocket 升級後才發現呼叫參數無效。
            ArgumentNullException.ThrowIfNull(httpContext);
            ArgumentNullException.ThrowIfNull(llmOpt);
            //ArgumentNullException.ThrowIfNull(fnToolCallA);
            ArgumentNullException.ThrowIfNull(fnTurnEndA);
            //ArgumentNullException.ThrowIfNull(fnBeforeReconnectA);

            _defAudioInput = llmOpt.DefaultAudioInput;

            // 非 WebSocket 請求無法進入雙向橋接；在接受連線前以 HTTP 400 結束請求。
            if (!httpContext.WebSockets.IsWebSocketRequest)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(
                    new { error = "WebSocket upgrade is required." }, ct);
                return;
            }

            //建立 ui socket service
            _uiSocketSvc = new WebSocketSvc();

            try
            {
                await _uiSocketSvc.AcceptA(httpContext);
                _fnTurnEndA = fnTurnEndA;
                _fnToolCallA = fnToolCallA;
                await InitBridgeA(
                    llmOpt, fnBeforeReconnectA, fnGetHistories, ct);
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
                _fnTurnEndA = null;
                _fnToolCallA = null;
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
        /// 維持 UI、server 與 LLM 之間的背景橋接。UI 訊息佇列與 client reader 存活於整個 Live session，
        /// 每輪則建立獨立 provider session、回合 queue 及 I/O workers；收到上層重連要求時，
        /// 先停止該輪 workers、排空已完成回合、執行可選的保存 callback，再關閉舊 provider socket 並開始下一輪。
        /// UI socket 由外層 StartA 負責釋放。
        /// </summary>
        /// <param name="llmOpt">固定的模型與對話設定；逐題歷史由呼叫端提供。</param>
        /// <param name="fnBeforeReconnectA">可選 callback；回合 queue 排空後執行保存，回傳 false 時取消重連。</param>
        /// <param name="fnGetHistories">可選 callback；由呼叫端依目前輸入提供 history snapshot。</param>
        /// <param name="ct">取消整體瀏覽器與模型橋接工作的權杖。</param>
        private async Task InitBridgeA(
            LiveLlmOptDto llmOpt,
            Func<CancellationToken, Task<bool>>? fnBeforeReconnectA,
            Func<string?, List<LlmHistoryDto>?>? fnGetHistories,
            CancellationToken ct)
        {
            //_Log.Debug("AbLiveLlmSvc InitA");

            // 保存尚未轉送的 UI JSON 訊息。這個 queue 和 reader 跨 provider 重連共用，
            // 因此重連期間已抵達的 client 訊息不會因舊 provider worker 結束而遺失。
            // queue 容量為 128；滿載時 UI reader 會等待 WriteAsync，形成背壓而非丟棄訊息。
            var messages = Channel.CreateBounded<string>(new BoundedChannelOptions(128)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });

            // lifeTime 代表整段 UI Live session，並連結 HTTP request cancellation；下方每輪另建立
            // receiver/sender token，讓重連只取消舊 provider I/O 而不停止跨輪的 UI reader。
            using var lifeTime = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // 回合 buffer 跨 provider 輪次存活，保留尚未封存的音訊/回合狀態；history snapshot 則由輸入 callback 提供。
            var turnBufferSvc = new AudioTurnBufferSvc(_defAudioInput);

            // 唯一的 UI reader 將 client 原始訊息送進跨輪 messages queue；不隨 provider 重連取消。
            var uiToWebTurn = OnUiToWebTurnA(messages.Writer, lifeTime.Token);

            // 呼叫端取消整段工作階段是正常的結束方式，不再轉成錯誤訊息回報。
            try
            {
                // 每次迴圈代表一個獨立 provider socket。client reader 與 messages queue 留在迴圈外，
                // 因而 provider 重建期間仍可暫存 client 輸入，待新 sender 接手。
                while (_uiSocketSvc.IsOpen && !lifeTime.IsCancellationRequested)
                {
                    // 新 provider session 開始後允許接收 Live transcription；重連不重設應用層 history queue。
                    turnBufferSvc.EnableLiveTranScript();

                    // 不在連線初始化時載入舊歷史，避免 provider 自動載入與逐題 callback 重複；
                    // 實際 history 會在新 socket 收到第一筆合格輸入前由 OnWebToLlmTurnA 附加。
                    await ConnectLlmA(new LiveLlmOptDto
                    {
                        EndPoint = llmOpt.EndPoint,
                        Model = llmOpt.Model,
                        VoiceName = llmOpt.VoiceName,
                        SystemPrompt = llmOpt.SystemPrompt,
                        Language = llmOpt.Language,
                        AudioResponse = llmOpt.AudioResponse,
                        Tools = llmOpt.Tools,
                        Histories = []
                    }, lifeTime.Token);

                    // 本輪 provider event receiver 是唯一 writer，ProcessTurnQueueA 是唯一 reader。
                    // unbounded queue 可先收下已完成回合，session loop 再於停止 receiver 後統一排空。
                    var turns = Channel.CreateUnbounded<LlmTurnResultDto>(new UnboundedChannelOptions
                    {
                        SingleReader = true,
                        SingleWriter = true
                    });
                    // 回合 consumer 可在背景提出重連要求；非同步 continuation 避免 TrySetResult
                    // 直接在 consumer stack 上執行 session loop 的 continuation。
                    var reconnectRequested = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);
                    var turnConsumer = ProcessTurnQueueA(turns.Reader, reconnectRequested);

                    // 以下兩個 worker 僅屬於目前 provider socket：receiver 讀模型事件並寫入 turns，
                    // sender 從跨輪 messages 讀 client 輸入並送到 provider。
                    using var receiverCts = CancellationTokenSource.CreateLinkedTokenSource(lifeTime.Token);
                    using var senderCts = CancellationTokenSource.CreateLinkedTokenSource(lifeTime.Token);
                    var webToLlmTurn = OnWebToLlmTurnA(messages.Reader, turnBufferSvc,
                        fnGetHistories, senderCts.Token, lifeTime.Token);
                    var llmToUiTurn = OnLlmToUiTurnA(turnBufferSvc, turns.Writer, receiverCts.Token);

                    // 第一個結束的工作決定收束原因：重連訊號只結束本輪；UI 關閉或 sender 結束則停止整段 session；
                    // receiver 正常結束則由下方檢查它是否提出過重連要求。
                    var completedTask = await Task.WhenAny(
                        uiToWebTurn, webToLlmTurn, llmToUiTurn, reconnectRequested.Task);
                    var shouldReconnect = completedTask == reconnectRequested.Task;
                    var shouldStop = false;
                    try
                    {
                        // UI reader 結束表示 client socket 已關閉或讀取完成，停止整段 Live session。
                        if (completedTask == uiToWebTurn)
                            shouldStop = true;

                        else if (completedTask == webToLlmTurn)
                        {
                            // sender 結束代表 client 輸入 stream 已關閉；await 也會把 sender 例外交給外層處理。
                            await webToLlmTurn;
                            shouldStop = true;
                        }
                        else if (completedTask == llmToUiTurn)
                            // receiver 結束可能是 provider 關閉；是否可重連仍以 reconnectRequested 為準。
                            await llmToUiTurn;
                    }
                    finally
                    {
                        // 無論本輪因何種工作結束，都先取消並等待 sender/receiver，
                        // 確保不會有舊 worker 在新 provider socket 建立後繼續讀寫。
                        receiverCts.Cancel();
                        senderCts.Cancel();
                        await WatchTaskA(webToLlmTurn);
                        await WatchTaskA(llmToUiTurn);

                        // receiver 已停止後關閉 turns writer；consumer 會讀完所有已排入回合再結束，
                        // 其中 callback 仍可能提出重連要求或補入待保存資料。
                        turns.Writer.TryComplete();
                        await WatchTaskA(turnConsumer);
                    }

                    // WhenAny 可能先等到 receiver/sender 結束；排空 turns 時 consumer 才處理到門檻回合，
                    // 所以在 queue 完全排空後再讀一次最終重連狀態，避免漏掉較晚提出的要求。
                    shouldReconnect |= reconnectRequested.Task.IsCompletedSuccessfully;

                    // 沒有重連要求表示本輪是正常結束或 client 已離線，不再建立新的 provider socket。
                    if (!shouldReconnect)
                        break;

                    // queue 已排空，此時 pending talks 包含門檻回合及收束期間完成的回合。
                    // callback 為 null 時略過保存；若 callback 回傳 false 則保留目前連線結束、不重建 provider。
                    if (fnBeforeReconnectA != null && !await fnBeforeReconnectA(lifeTime.Token))
                        break;

                    // 保存成功或未設定保存 callback 後才關閉舊 socket；迴圈頂端接著建立下一個 provider session。
                    await CloseA(CancellationToken.None);

                    // 即使 callback 同時要求重連，若 client sender 已結束也不再開新 session。
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
                // 非取消例外視為 bridge/provider 錯誤；client socket 尚可用時回傳錯誤訊息。
                if (_uiSocketSvc.IsOpen)
                    await WebToUiErrorA($"Live LLM 連線錯誤：{ex.Message}");
            }
            finally
            {
                // 整段 session 結束時停止跨輪 UI reader 並完成 messages writer；
                // 等 reader 收束後執行最後一次保存，確保未達重連門檻的 pending talks 也會提交。
                // UI socket 由外層 StartA 釋放，本方法負責保存收尾並關閉 provider socket。
                lifeTime.Cancel();
                messages.Writer.TryComplete();
                try
                {
                    await WatchTaskA(uiToWebTurn);
                    // 工作階段可能在未達重連門檻時結束，仍需保存 queue 已累積的回合。
                    if (fnBeforeReconnectA != null)
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
            AudioTurnBufferSvc turnBuffer,
            Func<string?, List<LlmHistoryDto>?>? fnGetHistoryForInputA,
            CancellationToken readCt, CancellationToken sendCt)
        {
            _Log.Debug("AbLiveLlmSvc Start OnWebToLlmTurnA");
            var historySentOnSocket = false;
            var audioTurnStarted = false;

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
                            historySentOnSocket = await SendHistoryA(
                                turnBuffer, null, historySentOnSocket,
                                fnGetHistoryForInputA, sendCt);
                            audioTurnStarted = true;
                        }
                        turnBuffer.AppendInputAudio(audioBytes);
                        await WebToLlmAudioA(audioBytes, sendCt);
                    }

                    if (input["frontTurnComplete"]?.Value<bool>() == true)
                    {
                        // 音訊輸入結束時提交回合；沒有前端逐字稿則保留通用備援文字。
                        turnBuffer.SetFallbackUserText(_defAudioInput);
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
                        historySentOnSocket = await SendHistoryA(
                            turnBuffer, text, historySentOnSocket,
                            fnGetHistoryForInputA, sendCt);
                        await WebToLlmTextA(text, sendCt);
                    }
                }
            }
        }

        private async Task<bool> SendHistoryA(
            AudioTurnBufferSvc turnBuffer,
            string? inputText,
            bool historySentOnSocket,
            Func<string?, List<LlmHistoryDto>?>? fnGetHistoryForInputA,
            CancellationToken sendCt)
        {
            if (historySentOnSocket)
            {
                turnBuffer.SetHistorySnapshot(Array.Empty<LlmHistoryDto>());
                return true;
            }

            var providedHistory = fnGetHistoryForInputA?.Invoke(inputText);
            if (providedHistory is null)
            {
                turnBuffer.SetHistorySnapshot(Array.Empty<LlmHistoryDto>());
                return false;
            }

            var history = providedHistory.ToList();
            turnBuffer.SetHistorySnapshot(history);

            if (history.Count > 0)
                await WebToLlmHistoryA(history, sendCt);
            return true;
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
                        if (turnBufferSvc.TryGetBreakTurn(out var interruptedTurn, inputTranscriptText))
                            TurnToQueueA(turnWriter, interruptedTurn);

                        inputTranscriptText = null;
                        turnBufferSvc.ResetForBreakResp();

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
        /// 回合 callback 回傳 true 時通知 session loop；session loop 會先停止 I/O 並排空 queue，再保存及重連。
        /// </summary>
        /// <param name="reader">接收 provider 完成或中斷回合的佇列 reader。</param>
        /// <param name="reconnectRequested">通知 session loop 重連要求的完成來源。</param>
        private async Task ProcessTurnQueueA(
            ChannelReader<LlmTurnResultDto> reader,
            TaskCompletionSource<bool> reconnectRequested)
        {
            // 單一 reader 保持回合順序；中斷回合也立即交給 callback 並計入重連門檻。
            await foreach (var turn in reader.ReadAllAsync())
            {
                await ProcessTurnA(turn, reconnectRequested);
            }
        }

        /// <summary>
        /// 正規化並交付單筆 provider 回合：必要時以 STT 補齊使用者文字、略過無有效內容的回合，
        /// 再呼叫上層處理器保存或更新應用狀態。上層回傳 true 才會通知 session loop 準備重連。
        /// called by: ProcessTurnQueueA
        /// </summary>
        /// <param name="turn">由 provider event receiver 組成的完整或中斷回合。</param>
        /// <param name="reconnectRequested">供 session loop 等待重連要求的完成來源。</param>
        private async Task ProcessTurnA(
            LlmTurnResultDto turn,
            TaskCompletionSource<bool> reconnectRequested)
        {
            // 純粹的中斷標記沒有可保存的模型回覆，避免產生無意義 Talk 或觸發重連。
            if (turn.IsBreak && string.Equals(turn.LlmText, "[Interrupted]", StringComparison.Ordinal))
            {
                _Log.Debug("AbLiveLlmSvc Skip interrupted input without an assistant response.");
                return;
            }

            // 音訊回合可能只有通用占位文字；只有非空且非占位文字才視為已有可用逐字稿。
            var userText = turn.UserText;
            var hasOkUserText = !string.IsNullOrWhiteSpace(userText) && userText != _defAudioInput;

            // 中斷回合或明確要求 STT 的回合若缺少有效文字且有音訊，嘗試辨識補字；既有文字優先保留。
            if ((turn.IsBreak || turn.NeedSTT) && !hasOkUserText && turn.InputAudio.Length > 0)
            {
                try
                {
                    // STT 設定獨立 20 秒上限，避免辨識服務無限等待而卡住整個回合佇列。
                    using var sttCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                    var transcript = await _llmSvc.SpeechToTextA(turn.InputAudio, "audio/pcm;rate=16000", sttCts.Token);
                    if (!hasOkUserText && !string.IsNullOrWhiteSpace(transcript))
                        userText = transcript;
                }
                catch (Exception ex)
                {
                    _Log.Error("AbLiveLlmSvc SpeechToTextA fallback failed: " + ex.Message);
                }
            }

            // STT 失敗或原始資料不足時，若使用者與助理兩側都沒有可用文字，就不交給上層保存。
            var hasUserText2 =
                !string.IsNullOrWhiteSpace(userText) && userText != _defAudioInput;
            var hasLlmText =
                !string.IsNullOrWhiteSpace(turn.LlmText) &&
                !string.Equals(turn.LlmText, "[Interrupted]", StringComparison.Ordinal);
            if (!hasUserText2 && !hasLlmText)
            {
                _Log.Debug("AbLiveLlmSvc Skip empty interrupted turn.");
                return;
            }

            // 用補齊後的文字建立回合副本；音訊、history、token 與中斷狀態沿用原 DTO。
            var completedTurn = turn with { UserText = userText };

            // 佇列收束時仍需完成已排入回合的應用層處理，因此不連結 request cancellation token。
            // callback 例外不終止 queue consumer，且預設 shouldReconnect=false，避免失敗時誤觸重連。
            var shouldReconnect = false;
            try
            {
                shouldReconnect = await (_fnTurnEndA
                    ?? throw new InvalidOperationException("Live session handlers are not configured."))(
                    completedTurn, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // 回合處理失敗時不要求重連，避免 callback 例外誤觸 provider session 重建。
                _Log.Error("AbLiveLlmSvc OnTurnEndA failed: " + ex.Message);
            }

            // true 只發出重連訊號，不在此處關閉 socket；session loop 會先停止 I/O 並排空剩餘回合。
            if (shouldReconnect)
                reconnectRequested.TrySetResult(true);
        }
        #endregion

    }//class
}
