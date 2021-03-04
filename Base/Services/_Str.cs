using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Linq.Expressions;
using System.Threading;
using System.Linq;
using Base.Models;

namespace Base.Services
{
    public class _Str
    {
        //base34(remove I,O, case unsensitive)
        private static char[] _base34 = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private static ulong _baseLen = (ulong)_base34.Length;
        private static long _startTicks = new DateTime(2000, 1, 1).Ticks;

        /*
        //random string for reptcha
        private static Random _random = new Random();
        public static string RandomStr(int len, int type = 1)
        {
            var chars = 
                (type == 1) ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" : 
                (type == 2) ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : 
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            return new string(Enumerable.Repeat(chars, len)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
        */

        /// <summary>
        /// check object is empty or not
        /// </summary>
        /// <param name="data">輸入字串</param>
        /// <returns></returns>
        public static bool IsEmpty(object data)
        {
            return (data == null || data.ToString() == "");
        }

        /// <summary>
        /// add anti slash for path if need
        /// </summary>
        /// <param name="dir">now path</param>
        /// <returns></returns>
        public static string AddAntiSlash(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return "\\";

            if (dir.Substring(dir.Length - 1, 1) != "/" && dir.Substring(dir.Length - 1, 1) != "\\")
                dir += "\\";
            return dir;
        }

        /// <summary>
        /// add right slash for url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string AddSlash(string url)
        {
            if (url.Substring(url.Length - 1, 1) != "/")
                url += "/";
            return url;
        }

        public static string RemoveRightSlash(string dir)
        {
            if (dir.Substring(dir.Length - 1, 1) == "/" || dir.Substring(dir.Length - 1, 1) == "\\")
                dir = dir.Substring(0, dir.Length - 1);
            return dir;
        }

        /// <summary>
        /// replace multiple char to another string
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="oldStr">old char list(string)</param>
        /// <param name="newStr">new string</param>
        /// <returns></returns>
        public static string ReplaceChars(string source, string oldStr, string newStr)
        {
            return (source == "")
                ? ""
                : Regex.Replace(source, @"[" + oldStr + "]", newStr);
        }

        /// <summary>
        /// add pre char
        /// </summary>
        /// <param name="len">total string length</param>
        /// <param name="obj">now string</param>
        /// <returns></returns>
        public static string PreChar(int len, object obj, char c1)
        {
            var str2 = obj.ToString();
            return (str2.Length >= len) ? str2 : new string(c1, len - str2.Length) + str2;
        }

        //add pre zero
        public static string PreZero(int len, object obj)
        {
            var str2 = obj.ToString();
            return (str2.Length >= len) ? str2 : new string('0', len - str2.Length) + str2;
        }

        //字串後面補字元
        public static string TailChar(int len, object obj, char c1 = '0')
        {
            var str2 = obj.ToString();
            return (str2.Length >= len) ? str2 : str2 + new string(c1, len - str2.Length);
        }

        //trim string, return "" if null       
        public static string Trim(string str)
        {
            return (str == null) ? "" : str.Trim();
        }

        /// <summary>
        /// get \t string times
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public static string Tab(int times)
        {
            var result = "";
            for (var i = 0; i < times; i++)
                result += "\t";
            return result;
        }

        /// <summary>
        /// get JObject string
        /// </summary>
        /// <param name="fid">JObject field name</param>
        /// <param name="data">value</param>
        /// <returns>JObject string</returns>
        public static string JsonData(string fid, string data)
        {
            var data2 = new JObject
            {
                [fid] = data
            };
            return data2.ToString();
        }

        /** 將 url64 字串還原為正常的 base64 字串, $pb_decode=true表示要進行 base64 解碼 */
        //public static string url64(string sour, bool pb_decode)
        /*
        public static string url64ToBase64(string sour, bool pb_decode)
        {
            //sour = _string.replaceChars(sour, "-_", "+/");
            sour = sour.Replace('-','+').Replace('_','/');    //會更換所有的字元 (javascript replace() 只會更換一個字元 !!)
            if (pb_decode)
                sour = Encoding.UTF8.GetString(Convert.FromBase64String(sour));

            return sour;
        }    
        */

        /// <summary>
        /// convert json string to json object
        /// </summary>
        /// <param name="str">json string</param>
        /// <returns>JObject</returns>
        public static JObject ToJson(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

		    try {
			    return JObject.Parse(str);
		    } catch {
                _Log.Error("_Str.cs ToJson() error: " + str);
                return null;
		    }

            /*
		    if (!ok){
			    var pos = str.IndexOf(",");
			    if (pos < 0){
				    _Log.Error("_Str.cs ToJson() error:" + str);
				    return null;
			    }
				
			    str = "{" + str.Substring(pos+1) ;
			    try {
                    return JObject.Parse(str);
			    } catch {
				    return null;
			    }
		    }		
		
		    //case else
		    return null;
            */
        }

        /// <summary>
        /// ??
        /// get variables name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string GetVarName<T>(Expression<Func<T>> expr)
        {
            var body = (MemberExpression)expr.Body;
            return body.Member.Name;
        }

        /// <summary>
        /// to List<string>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sep">seperator</param>
        /// <returns></returns>
        public static List<string> ToList(string str, char sep = ',')
        {
            return (string.IsNullOrEmpty(str))
                ? null
                : new List<string>(str.Split(sep));
        }

        /// <summary>
        /// convert string to list int
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public static List<int> ToIntList(string str, char sep = ',')
        {
            return (string.IsNullOrEmpty(str))
                ? null
                : str.Split(sep).Select(Int32.Parse).ToList();
        }

        //get hash key(base64, 25 char) for cache key
        public static string GetHashKey(string data)
        {
            var md5 = Md5(data);
            return md5 + PreZero(3, md5.GetHashCode() % 1000);
        }

        /// <summary>
        /// get Md5 string(base64)
        /// </summary>
        /// <param name="str"></param>
        /// <returns>22 char</returns>
        public static string Md5(string str)
        {
            //var md5 = MD5.Create();
            //var bytes = Encoding.Default.GetBytes(obj); //to Byte[] 
            var bytes = MD5.Create().ComputeHash(Encoding.Default.GetBytes(str)); //encrypt
            return Convert.ToBase64String(bytes).Substring(0, 22).Replace('+', '-').Replace('/', '_');//將加密後的字串從byte[]轉回string
        }
        
        /// <summary>
        /// get new Id for db key, 10 char(upperCase), consider db index performance
        /// max date is 2120/1/1(10 char, from 2000/1/1)
        /// </summary>
        /// <returns></returns>
        public static string NewId()
        {
            //stop 1 mini second for avoid repeat
            Thread.Sleep(1);

            //乘上一個數字(16)以增加尾數的差異性
            const int baseNum = 16;
            var num = (ulong)((DateTime.Now.Ticks - _startTicks) / TimeSpan.TicksPerMillisecond);

            //tail add one random number(0-baseNum - 1, guid for random seed)
            var random = new Random(Guid.NewGuid().GetHashCode());
            num = num * baseNum + (ulong)random.Next(0, baseNum - 1);
            //var tail = ticks % 10; 
            //num = num * 10 + (ulong)(ticks % 10);

            //convert to base34
            var data = "";
            ulong mod;
            while (num > 0)
            {
                mod = num % _baseLen;
                num = (num / _baseLen);
                data = _base34[(int)mod] + data;
            }

            //fixed length of 10 chars, add tailed '0'
            if (data.Length < 9)
                data = data + new string('0', 9 - data.Length);

            //add server id for multiple web server
            data += _Fun.Config.ServerId;
            return data;
        }

        /// <summary>
        /// get new key from newKeyJson(CrudEdit.cs)
        /// </summary>
        /// <param name="newKeyJson">CrudEdit._newKeyJson</param>
        /// <param name="tableSn">table sn</param>
        /// <param name="rowIndex">row index, base 0 !! (different)</param>
        /// <returns></returns>
        public static string ReadNewKeyJson(JObject newKeyJson, string tableSn = "0", int rowIndex = 0)
        {
            var fid = "t" + tableSn;
            if (newKeyJson == null || newKeyJson[fid] == null)
                return "";

            JObject json = newKeyJson[fid] as JObject;
            fid = "f" + (rowIndex + 1); //change to base 1
            return (json[fid] == null) ? "" : json[fid].ToString();
        }

        /// <summary>
        /// string to boolean
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ToBool(object obj)
        {
            var str = obj.ToString().ToLower();
            return (str == "1" || str == "true");
        }

        /// <summary>
        /// replace json string
        /// </summary>
        /// <param name="source">source json string</param>
        /// <param name="json">for replace</param>
        /// <returns></returns>
        public static string ReplaceJson(string source, JObject json)
        {
            if (json != null)
            {
                foreach (var item in json)
                    source = source.Replace("[" + item.Key + "]", item.Value.ToString());
            }
            return source;
        }

        //convert string to hex string
        public static string ToHex(string str)
        {
            var data = "";
            foreach (var c1 in str)
                data += string.Format("{0:X}", Convert.ToInt32(c1));
            return data;
        }

        //convert hex string to normal string
        public static string HexToStr(string hex)
        {
            var date = "";
            while (hex.Length > 0)
            {
                date += Convert.ToChar(Convert.ToUInt32(hex.Substring(0, 2), 16)).ToString();
                hex = hex.Substring(2, hex.Length - 2);
            }
            return date;
        }

        /// <summary>
        /// list string 加上單引號
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListAddQuote(string list)
        {
            return "'" + list.Replace(",", "','") + "'";
        }

        /// <summary>
        /// check key rule: alphbetic, numeric, comma(,)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="logTail">optional, when wrong add log tail</param>
        /// <returns></returns>
        public static bool CheckKeyRule(string str, string logTail = "")
        {
            if (string.IsNullOrEmpty(str))
                return true;

            Regex rg = new Regex(@"^[a-zA-Z0-9,]*$");
            if (rg.IsMatch(str))
                return true;

            if (!string.IsNullOrEmpty(logTail))
                _Log.Error("_Str.IsAlphaNum() failed (" + logTail + ")");
            return false;
        }

        /// <summary>
        /// convert string array to IdStrDto list
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static List<IdStrDto> ToIdStrs(string list)
        {
            var ary = list.Split(',');
            var data = new List<IdStrDto>();
            for (var i = 0; i < ary.Length; i = i + 2)
            {
                data.Add(new IdStrDto()
                {
                    Id = ary[i],
                    Str = ary[i + 1],
                });
            }
            return data;
        }

        /// <summary>
        /// get mid part string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static string GetMid(string source, string left, string right)
        {
            var pos1 = source.IndexOf(left);
            if (pos1 > 0)
                source = source.Substring(pos1 + 1);
            pos1 = source.IndexOf(right);
            if (pos1 > 0)
                source = source.Substring(0, pos1);
            return source;
        }

        /// <summary>
        /// get left part string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="find">find string</param>
        /// <returns></returns>
        public static string GetLeft(string source, string find)
        {
            var pos = source.IndexOf(find);
            return (pos < 0) ? source : source.Substring(0, pos);
        }

        /// <summary>
        /// get left part string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="find">find string</param>
        /// <returns></returns>
        public static string GetRight(string source, string find)
        {
            var pos = source.IndexOf(find);
            return (pos < 0) ? source : source.Substring(pos + 1);
        }

        /// <summary>
        /// mustache 字串填充, 使用 Handlebars.Net
        /// </summary>
        /// <param name="source">來源字串</param>
        /// <param name="json">要填入的資料</param>
        /*
        public static void Mustache(ref string source, JObject json)
        {
            var date = "";
            while (hex.Length > 0)
            {
                date += Convert.ToChar(Convert.ToUInt32(hex.Substring(0, 2), 16)).ToString();
                hex = hex.Substring(2, hex.Length - 2);
            }
            return date;
        }
        */

    }//class
}