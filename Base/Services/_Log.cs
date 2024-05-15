using System;
using System.IO;
using System.Threading.Tasks;

namespace Base.Services
{
    //write log text file, use sync !!
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
        private static string GetFilePath(string type)
        {
            return GetDir() + "/" + DateTime.Now.ToString(_fileFormat) + "-" + type + ".txt";
        }

        /// <summary>
        /// log info only when _Fun.LogInfo flag is true !!
        /// </summary>
        /// <param name="msg">log msg</param>
        public static void Info(string msg)
        {
            LogFile(GetFilePath("info"), msg);
        }

        /// <summary>
        /// log debug when _Fun.LogDebug true !!
        /// </summary>
        /// <param name="msg">log msg</param>
        public static void Debug(string msg)
        {
            if (_Fun.Config.LogDebug)
                LogFile(GetFilePath("debug"), msg);
        }

        /// <summary>
        /// log sql only when _Fun.LogSql true !!
        /// </summary>
        /// <param name="msg">log msg</param>
        public static void Sql(string msg)
        {
            if (_Fun.Config.LogSql)
                LogFile(GetFilePath("sql"), msg);
        }

        /// <summary>
        /// log error
        /// </summary>
        /// <param name="msg">log message</param>
        public static void Error(string msg)
        {
            LogFile(GetFilePath("error"), msg);

            //send root
            //if (emailRoot) await _Email.SendRootA(msg);
        }

        /*
        /// <summary>
        /// log error and return system error(SE) code
        /// </summary>
        /// <param name="msg"></param>
        public static string ErrorSeCode(string msg)
        {
            Error(msg);
            return _Fun.SysErrCode;
        }
        */

        /// <summary>
        /// log error and mail root
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task ErrorRootA(string msg)
        {
            LogFile(GetFilePath("error"), msg);

            //send root
            await _Email.SendRootA(msg);
        }

        /// <summary>
        /// add one line to log file including current time
        /// </summary>
        /// <param name="path">log file path</param>
        /// <param name="msg"></param>
        private static void LogFile(string path, string msg)
        {
            if (msg == "") return;

            const int loops = 5;
            for (var i=0; i<loops; i++)
            {
                try
                {
                    if (msg.Substring(msg.Length - 1, 1) != "\n") msg += "\n";
                    msg = DateTime.Now.ToString("HH:mm:ss") + "(" + i + "); " + msg;
                    File.AppendAllText(path, msg);
                    break;
                }
                catch
                {
                    //raise error if get max loops
                    if (i < loops - 1) _Time.Sleep(100);
                    else
                    {
                        //throw new Exception("_Log.cs LogFile() failed for file: " + path);
                    }
                }
            }
        }

        /*
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
        */

    }//class
}