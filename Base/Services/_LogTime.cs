using System;

namespace Base
{
    //log running time
    public class _LogTime
    {
        //instance variables
        private static DateTime _start;
        private static DateTime _now;
        private static string _result = "";

        private const string _newLine = "\r\n";

        //constructor
        public static void Init(string name = "") 
        {
            if (name != "")
                name += _newLine;

            _start = DateTime.Now;
            _now = _start;
            _result = _newLine + name;
        }

        //log time
        public static void Log(string name)
        {
            var now = DateTime.Now;
            _result += name + ":" + (int)(now - _now).TotalMilliseconds + "/" + (int)(now - _start).TotalMilliseconds + _newLine;
            _now = DateTime.Now;    //reset
        }

        //return log string
        public static string GetLogMsg()
        {
            return _result;
        }
    }
}
