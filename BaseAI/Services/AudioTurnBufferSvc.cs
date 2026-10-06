using System.Text;

namespace BaseAI.Services
{
    /// <summary>收集單一對話回合的使用者輸入與助理逐字稿，並以鎖保護並行讀寫。</summary>
    public class AudioTurnBufferSvc
    {
        private readonly object _syncRoot = new();
        private string _userText = string.Empty;
        private bool _hasClientUserText;
        private readonly StringBuilder _assistantText = new();
        private int _totalTokens;

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
                if (_hasClientUserText) return;
                if (_userText == "[Audio Input]") _userText = string.Empty;
                _userText += text;
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
            lock (_syncRoot) _assistantText.Append(text);
        }

        /// <summary>取出並清空本回合內容；使用者與助理文字皆有值才視為有效回合。</summary>
        public bool TryComplete(out string userText, out string assistantText, out int totalTokens)
        {
            lock (_syncRoot)
            {
                userText = _userText;
                assistantText = _assistantText.ToString();
                totalTokens = _totalTokens;
                _userText = string.Empty;
                _hasClientUserText = false;
                _assistantText.Clear();
                _totalTokens = 0;
                return !string.IsNullOrWhiteSpace(userText) &&
                    !string.IsNullOrWhiteSpace(assistantText);
            }
        }
    }

}
