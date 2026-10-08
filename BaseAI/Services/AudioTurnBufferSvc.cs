using BaseAI.Models;
using System.Text;

namespace BaseAI.Services
{
    /// <summary>
    /// 管理「語音單一對話回合」的暫存狀態。
    /// 主要負責：
    /// 1) 聚合使用者文字（前端直接文字或 Live 轉錄）
    /// 2) 聚合助理輸出逐字稿
    /// 3) 暫存本回合輸入音訊（可在中斷時封存）
    /// 4) 控制中斷後的狀態旗標，避免舊事件污染下一回合
    /// 
    /// 所有可變狀態均由同一把鎖保護，避免 WebSocket / provider callback / request thread
    /// 同時寫入造成資料競爭。
    /// </summary>
    public class AudioTurnBufferSvc
    {
        /// <summary>
        /// 單一同步鎖：所有欄位皆在 lock(_syncRoot) 下讀寫。
        /// </summary>
        private readonly object _syncRoot = new();

        /// <summary>本回合最終使用者文字（可由前端文字或 Live 轉錄累積而成）。</summary>
        private string _userText = string.Empty;

        /// <summary>
        /// 是否已收到「前端明確提供」的使用者文字。
        /// 一旦為 true，便不再接受 Live 轉錄覆寫，避免兩種來源互相衝突。
        /// </summary>
        private bool _hasUserText;

        /// <summary>本回合助理輸出文字緩衝。</summary>
        private readonly StringBuilder _llmText = new();

        /// <summary>正在收集中的一段輸入音訊（尚未結束封包）。</summary>
        private readonly MemoryStream _nowInputAudio = new();

        /// <summary>
        /// 已完成的一段輸入音訊佇列。
        /// 每次 CompleteInputAudio 會把當前 stream 封存成 byte[] 放入佇列。
        /// </summary>
        private readonly Queue<byte[]> _finishInputAudio = new();

        /// <summary>Provider 回報的本回合 token 累計值（以最後一次回報為準）。</summary>
        private int _totalTokens;

        /// <summary>
        /// 是否要求後續改走 Speech-to-Text。
        /// 常見於中斷發生後，避免繼續信任可能錯位的 Live 逐字事件。
        /// </summary>
        private bool _needSpeechToText;

        /// <summary>
        /// 是否要丟棄中斷後殘留的轉錄尾巴。
        /// 用來吸收「上一回合延遲到達」的事件，避免誤拼到下一回合。
        /// </summary>
        private bool _skipBreakTranScriptTail;

        /// <summary>
        /// 助理是否已開始回應。
        /// 若使用者在此期間又開始輸入，會被視為插話（interrupt）前兆。
        /// </summary>
        private bool _llmResping;

        /// <summary>
        /// 更新本回合 token 統計。
        /// </summary>
        /// <param name="totalTokens">Provider 回報的累計 token。</param>
        public void SetTokens(int totalTokens)
        {
            lock (_syncRoot) _totalTokens = totalTokens;
        }

        /// <summary>
        /// 設定使用者文字（通常是前端直接提交的輸入）。
        /// 空白字串會被忽略，不覆蓋既有內容。
        /// </summary>
        /// <param name="text">使用者文字。</param>
        public void SetUserText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            lock (_syncRoot)
            {
                _userText = text;
                _hasUserText = true;
            }
        }

        /// <summary>
        /// 追加 Live Input Transcription 片段到使用者文字。
        /// 僅在「未要求 STT、未進入丟尾巴模式、且尚未有前端明確文字」時生效。
        /// </summary>
        /// <param name="text">本次收到的轉錄文字片段。</param>
        public void AppendInputTranScript(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            lock (_syncRoot)
            {
                // 中斷後或要求 STT 時，不再信任 Live 轉錄。
                if (_needSpeechToText || _skipBreakTranScriptTail) return;

                // 前端明確文字優先，避免雙來源混寫。
                if (_hasUserText) return;

                // 若先前僅有音訊占位符，改為實際轉錄文字。
                if (_userText == "[Audio Input]") _userText = string.Empty;
                _userText += text;
            }
        }

        /// <summary>
        /// 追加一段輸入音訊位元組到目前音訊緩衝。
        /// </summary>
        /// <param name="audioData">音訊資料塊。</param>
        public void AppendInputAudio(byte[] audioData)
        {
            if (audioData.Length == 0) return;
            lock (_syncRoot) _nowInputAudio.Write(audioData);
        }

        /// <summary>
        /// 標記「使用者開始新的輸入回合」。
        /// 若此時助理仍在回應，代表使用者插話，需切換到保守模式：
        /// 1) 後續要求 STT
        /// 2) 丟棄可能延遲到達的中斷尾巴
        /// </summary>
        public void BeginInputTurn()
        {
            lock (_syncRoot)
            {
                if (!_llmResping) return;
                _needSpeechToText = true;
                _skipBreakTranScriptTail = true;
            }
        }

        /// <summary>
        /// 標記助理回應已開始。
        /// 供 BeginInputTurn 判斷是否形成「插話」場景。
        /// </summary>
        public void MarkLlmResping()
        {
            lock (_syncRoot) _llmResping = true;
        }

        /// <summary>
        /// 將目前累積的輸入音訊封存為一段完成片段。
        /// 若目前沒有音訊資料，則不進行任何操作。
        /// </summary>
        public void FinishInputAudio()
        {
            lock (_syncRoot)
            {
                if (_nowInputAudio.Length == 0) return;
                _finishInputAudio.Enqueue(_nowInputAudio.ToArray());
                _nowInputAudio.SetLength(0);
            }
        }

        /// <summary>
        /// 設定備援使用者文字（僅在目前尚無文字時才會生效）。
        /// 常用於僅有音訊、尚未有可用轉錄內容時的占位敘述。
        /// </summary>
        /// <param name="text">備援文字。</param>
        public void SetFallbackUserText(string text)
        {
            lock (_syncRoot)
            {
                if (string.IsNullOrWhiteSpace(_userText))
                    _userText = text;
            }
        }

        /// <summary>
        /// 累加助理逐字稿片段。
        /// 若目前處於「丟棄中斷尾巴」階段，則忽略該片段，避免回合污染。
        /// </summary>
        /// <param name="text">助理文字片段。</param>
        public void AppendLlmText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            lock (_syncRoot)
            {
                if (!_skipBreakTranScriptTail)
                    _llmText.Append(text);
            }
        }

        /// <summary>
        /// 嘗試完成一個「正常結束」的回合。
        /// 需要同時具備 userText 與 assistantText，否則視為資料不足並回傳 false。
        /// 成功時會輸出快照並重置文字相關狀態。
        /// </summary>
        /// <param name="turn">完成的回合資料。</param>
        /// <returns>是否成功產生回合結果。</returns>
        public bool TryComplete(out LlmTurnResultDto turn)
        {
            lock (_syncRoot)
            {
                var userText = _userText;
                var assistantText = _llmText.ToString();
                if (string.IsNullOrWhiteSpace(userText) || string.IsNullOrWhiteSpace(assistantText))
                {
                    turn = default!;
                    return false;
                }

                turn = CreateTurn(userText, assistantText, wasInterrupted: false);
                ResetTextTurn();
                return true;
            }
        }

        /// <summary>
        /// 嘗試封存一個「中斷回合」。
        /// 此方法會主動切入保守模式（requires STT + discard tail），
        /// 並盡量產生可追蹤的回合紀錄，即使文字片段不完整。
        /// </summary>
        /// <param name="turn">中斷回合資料。</param>
        /// <returns>是否成功產生可用的中斷回合結果。</returns>
        public bool TryBreak(out LlmTurnResultDto turn)
        {
            lock (_syncRoot)
            {
                // 中斷後立即切換為保守模式，避免殘留事件污染下一回合。
                _needSpeechToText = true;
                _skipBreakTranScriptTail = true;

                var userText = _userText;
                if (string.IsNullOrWhiteSpace(userText))
                    userText = "[Audio Input]";

                var assistantText = _llmText.ToString();
                if (string.IsNullOrWhiteSpace(assistantText))
                    assistantText = "[Interrupted]";

                if (_nowInputAudio.Length > 0)
                {
                    _finishInputAudio.Enqueue(_nowInputAudio.ToArray());
                    _nowInputAudio.SetLength(0);
                }

                var inputAudio = TakeFinishInputAudio();
                // 文字與音訊都為空，代表沒有可封存內容。
                if (inputAudio.Length == 0 &&
                    string.IsNullOrWhiteSpace(_userText) &&
                    string.IsNullOrWhiteSpace(_llmText.ToString()))
                {
                    turn = default!;
                    return false;
                }

                turn = CreateTurn(userText, assistantText, wasInterrupted: true, inputAudio);
                ResetTextTurn();
                return true;
            }
        }

        /// <summary>
        /// 消耗一次「中斷尾巴」旗標。
        /// 通常在收到上一回合遲到的完成訊號時呼叫，避免該訊號誤套用到新回合。
        /// </summary>
        /// <returns>若確實消耗到尾巴旗標則為 true；否則 false。</returns>
        public bool ConsumeBreakTail()
        {
            lock (_syncRoot)
            {
                if (!_skipBreakTranScriptTail) return false;
                _skipBreakTranScriptTail = false;
                return true;
            }
        }

        /// <summary>
        /// 在新 provider session 建立後，重新允許 Live 轉錄寫入。
        /// </summary>
        public void EnableLiveTranScript()
        {
            lock (_syncRoot)
            {
                _needSpeechToText = false;
                _skipBreakTranScriptTail = false;
            }
        }

        /// <summary>
        /// 建立回合 DTO。
        /// 若未指定 inputAudio，則自 completed queue 取出一段已封存音訊。
        /// </summary>
        /// <param name="userText">使用者文字。</param>
        /// <param name="assistantText">助理文字。</param>
        /// <param name="wasInterrupted">是否為中斷回合。</param>
        /// <param name="inputAudio">可選，指定要附帶的輸入音訊。</param>
        /// <returns>回合結果 DTO。</returns>
        private LlmTurnResultDto CreateTurn(
            string userText, string assistantText, bool wasInterrupted, byte[]? inputAudio = null)
        {
            inputAudio ??= TakeFinishInputAudio();
            return new LlmTurnResultDto(
                userText,
                assistantText,
                _totalTokens,
                inputAudio,
                wasInterrupted,
                _needSpeechToText || userText == "[Audio Input]");
        }

        /// <summary>
        /// 從已完成音訊佇列取出一段音訊。
        /// 若佇列為空，回傳空陣列。
        /// </summary>
        /// <returns>一段輸入音訊或空陣列。</returns>
        private byte[] TakeFinishInputAudio()
        {
            return _finishInputAudio.Count > 0 ? _finishInputAudio.Dequeue() : [];
        }

        /// <summary>
        /// 重置單回合「文字相關」狀態。
        /// 注意：不重置 requires STT 與 discard tail，
        /// 這兩個旗標屬於跨回合保護機制，需由外部流程在適當時機清除。
        /// </summary>
        private void ResetTextTurn()
        {
            _userText = string.Empty;
            _hasUserText = false;
            _llmText.Clear();
            _totalTokens = 0;
            _llmResping = false;
        }
    }

}
