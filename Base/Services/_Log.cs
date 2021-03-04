using System;
using System.IO;

namespace Base.Services
{
    //write log text file
    public static class _Log
    {

        //log folder name
        private const string _folder = "_log";

        //log file name format
        private const string _fileFormat = "yyyy-MM-dd";

        /// <summary>
        /// constructor
        /// </summary>
        //public static void Init()
        static _Log()
        {
            _File.MakeDir(GetDir());
        }

        //get log dir path
        private static string GetDir()
        {
            return _Fun.DirRoot + _folder;
        }

        //get log file path
        private static string GetFile(string tail)
        {
            return GetDir() + "/" + DateTime.Now.ToString(_fileFormat) + "-" + tail + ".txt";
        }

        /// <summary>
        /// log info only when _Fun.LogInfo flag is true !!
        /// </summary>
        /// <param name="msg">log msg</param>
        public static void Info(string msg)
        {
            if (_Fun.Config.LogInfo)
                LogFile(GetFile("info"), msg);
        }

        public static void Sql(string msg)
        {
            if (_Fun.Config.LogSql)
                LogFile(GetFile("sql"), msg);
        }

        /// <summary>
        /// log error
        /// </summary>
        /// <param name="msg">錯誤訊息</param>
        public static void Error(string msg, bool mailRoot = true)
        {
            LogFile(GetFile("error"), msg);

            //send root
            if (mailRoot)
                _Mail.SendRoot(msg);
        }

        /// <summary>
        /// add one line to log file including current time
        /// </summary>
        /// <param name="path">log file path</param>
        /// <param name="msg"></param>
        private static void LogFile(string path, string msg)
        {
            const int loops = 5;
            for (var i=0; i<loops; i++)
            {
                try
                {
                    var file = File.Exists(path) ? File.AppendText(path) : File.CreateText(path);
                    file.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "(" + i + "); " + msg);
                    file.Close();
                    break;
                }
                catch
                {
                    //raise error if get max loops
                    if (i < loops - 1)
                        _Time.Sleep(100);
                    else
                    {
                        //throw new Exception("_Log.cs LogFile() failed for file: " + path);
                    }
                }
            }
        }

        /// <summary>
        /// Log an Exception
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="source"></param>
        public static void LogException(Exception ex, string source)
        {
            //add inner exception
            var msg = "Exception: " + _Fun.TextCarrier;
            if (ex.InnerException != null)
            {
                msg += "Inner Exception Type: " + ex.InnerException.GetType().ToString() + _Fun.TextCarrier +
                    "Inner Exception: " + ex.InnerException.Message + _Fun.TextCarrier +
                    "Inner Source: " + ex.InnerException.Source + _Fun.TextCarrier;
                if (ex.InnerException.StackTrace != null)
                    msg += "Inner Stack Trace: " + ex.InnerException.StackTrace + _Fun.TextCarrier;
            }

            //add exception
            msg += "Exception Type: " + ex.GetType().ToString() + _Fun.TextCarrier +
                "Exception: " + ex.Message + _Fun.TextCarrier +
                "Source: " + source + _Fun.TextCarrier;
            if (ex.StackTrace != null)
                msg += "Stack Trace: " + ex.StackTrace;

            Error(msg);
        }

    }//class
}