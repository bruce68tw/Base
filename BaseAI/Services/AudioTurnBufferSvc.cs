using System.Text;
using BaseAI.Models;

namespace BaseAI.Services
{
    /// <summary>收集單一對話回合的使用者輸入與助理逐字稿，並以鎖保護並行讀寫。</summary>
    public class AudioTurnBufferSvc
    {
        private readonly object _syncRoot = new();
        private string _userText = string.Empty;
        private bool _hasClientUserText;
        private readonly StringBuilder _assistantText = new();
        private readonly MemoryStream _currentInputAudio = new();
        private readonly Queue<byte[]> _completedInputAudio = new();
        private int _totalTokens;
        private bool _requiresSpeechToText;
        private bool _discardInterruptedTranscriptTail;
        private bool _assistantResponseInProgress;

        /// <summary>記錄 provider 回報的本回合 token 累計值，以最後一筆為準。</summary>
        public void SetTokens(int totalTokens)
        {
            lock (_syncRoot) _totalTokens = totalTokens;
        }

        /// <summary>設定使用者文字；空白內容不覆蓋既有值。</summary>
        public void SetUserText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            lock (_syncRoot)
            {
                _userText = text;
                _hasClientUserText = true;
            }
        }

        public void AppendInputTranScript(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            lock (_syncRoot)
            {
                if (_requiresSpeechToText || _discardInterruptedTranscriptTail) return;
                if (_hasClientUserText) return;
                if (_userText == "[Audio Input]") _userText = string.Empty;
                _userText += text;
            }
        }

        public void AppendInputAudio(byte[] audioData)
        {
            if (audioData.Length == 0) return;
            lock (_syncRoot) _currentInputAudio.Write(audioData);
        }

        public void BeginInputTurn()
        {
            lock (_syncRoot)
            {
                if (!_assistantResponseInProgress) return;
                _requiresSpeechToText = true;
                _discardInterruptedTranscriptTail = true;
            }
        }

        public void MarkAssistantResponseInProgress()
        {
            lock (_syncRoot) _assistantResponseInProgress = true;
        }

        public void CompleteInputAudio()
        {
            lock (_syncRoot)
            {
                if (_currentInputAudio.Length == 0) return;
                _completedInputAudio.Enqueue(_currentInputAudio.ToArray());
                _currentInputAudio.SetLength(0);
            }
        }

        /// <summary>僅在尚無使用者文字時設定備援文字。</summary>
        public void SetFallbackUserText(string text)
        {
            lock (_syncRoot)
            {
                if (string.IsNullOrWhiteSpace(_userText))
                    _userText = text;
            }
        }

        /// <summary>累加助理逐字稿片段。</summary>
        public void AppendAssistantText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            lock (_syncRoot)
            {
                if (!_discardInterruptedTranscriptTail)
                    _assistantText.Append(text);
            }
        }

        /// <summary>快照並清空一個已正常完成的對話回合。</summary>
        public bool TryComplete(out LlmTurnResultDto turn)
        {
            lock (_syncRoot)
            {
                var userText = _userText;
                var assistantText = _assistantText.ToString();
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

        /// <summary>封存被 Gemini 中斷的回合，並停用本 session 後續可能錯配的 Live 輸入轉錄。</summary>
        public bool TryInterrupt(out LlmTurnResultDto turn)
        {
            lock (_syncRoot)
            {
                _requiresSpeechToText = true;
                _discardInterruptedTranscriptTail = true;

                var userText = _userText;
                if (string.IsNullOrWhiteSpace(userText))
                    userText = "[Audio Input]";

                var assistantText = _assistantText.ToString();
                if (string.IsNullOrWhiteSpace(assistantText))
                    assistantText = "[Interrupted]";

                var inputAudio = TakeCompletedInputAudio();
                if (inputAudio.Length == 0 &&
                    string.IsNullOrWhiteSpace(_userText) &&
                    string.IsNullOrWhiteSpace(_assistantText.ToString()))
                {
                    turn = default!;
                    return false;
                }

                turn = CreateTurn(userText, assistantText, wasInterrupted: true, inputAudio);
                ResetTextTurn();
                return true;
            }
        }

        /// <summary>消耗中斷回合殘留的 Completed 訊號，避免將其套用到下一回合。</summary>
        public bool ConsumeInterruptedTail()
        {
            lock (_syncRoot)
            {
                if (!_discardInterruptedTranscriptTail) return false;
                _discardInterruptedTranscriptTail = false;
                return true;
            }
        }

        /// <summary>新 provider session 建立後，可重新使用 Live 輸入轉錄。</summary>
        public void EnableLiveTranscription()
        {
            lock (_syncRoot)
            {
                _requiresSpeechToText = false;
                _discardInterruptedTranscriptTail = false;
            }
        }

        private LlmTurnResultDto CreateTurn(
            string userText, string assistantText, bool wasInterrupted, byte[]? inputAudio = null)
        {
            inputAudio ??= TakeCompletedInputAudio();
            return new LlmTurnResultDto(
                userText,
                assistantText,
                _totalTokens,
                inputAudio,
                wasInterrupted,
                _requiresSpeechToText || userText == "[Audio Input]");
        }

        private byte[] TakeCompletedInputAudio()
        {
            return _completedInputAudio.Count > 0 ? _completedInputAudio.Dequeue() : [];
        }

        private void ResetTextTurn()
        {
            _userText = string.Empty;
            _hasClientUserText = false;
            _assistantText.Clear();
            _totalTokens = 0;
            _assistantResponseInProgress = false;
        }
    }

}
