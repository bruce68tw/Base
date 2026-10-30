using System;

namespace Base
{
    /// <summary>
    /// 以靜態狀態累積各步驟及總耗時，不隔離並行操作。
    /// </summary>
    public class _LogTime
    {
        //instance variables
        private static DateTime _start;
        private static DateTime _now;
        private static string _result = "";

        private const string _newLine = "\r\n";

        /// <summary>
        /// 重設計時起點與記錄，可加上作業名稱。
        /// </summary>
        public static void Init(string name = "") 
        {
            if (name != "") name += _newLine;
            _start = DateTime.Now;
            _now = _start;
            _result = _newLine + name;
        }

        /// <summary>
        /// 追加步驟名稱、距前一步與距起點的毫秒數。
        /// </summary>
        public static void Log(string name)
        {
            var now = DateTime.Now;
            _result += name + ":" + (int)(now - _now).TotalMilliseconds + "/" + (int)(now - _start).TotalMilliseconds + _newLine;
            _now = DateTime.Now;    //reset
        }

        //return log string
        /// <summary>
        /// 取得目前累積的計時文字。
        /// </summary>
        public static string GetLogMsg()
        {
            return _result;
        }
    }
}
