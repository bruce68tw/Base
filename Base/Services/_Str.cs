using Base.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Services
{
    public class _Str
    {

        //base34 encode(remove I/O for readable)
        private static readonly char[] _base34 = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] _base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] _base62 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly ulong _baseLen = (ulong)_base34.Length;
        private static readonly long _startTicks = new DateTime(2000, 1, 1).Ticks;
        //private static long _startMilliSec = new DateTime(2000, 1, 1).Ticks / 1000;

        //random string for reptcha
        //private static Random _random = new Random();

        /// <summary>
        /// get random string
        /// </summary>
        /// <param name="len"></param>
        /// <param name="type">1(num),2(upper alpha),3(all alpha),4(num & alpha)</param>
        /// <returns></returns>
        public static string RandomStr(int len, int type)
        {
            var chars =
                (type == 1) ? "0123456789" :
                (type == 2) ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" :
                (type == 3) ? "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789" : 
                "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var random = new Random();
            return new string(Enumerable.Repeat(chars, len)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetError(string error = "")
        {
            return _Fun.PreError + _Str.EmptyToValue(error, _Fun.SystemError);
        }

        //get BR code error
        public static string GetBrError(string error)
        {
            return _Fun.PreBrError + error;
        }

        /// <summary>
        /// check object is empty or not
        /// </summary>
        /// <param name="data">input string</param>
        /// <returns></returns>
        public static bool IsEmpty(string data)
        {
            return (data == null || data == "");
        }

        public static bool NotEmpty(string data)
        {
            return !IsEmpty(data);
        }

        /*
        public static bool IsInList(string longStr, string shortStr, string sep = ",")
        {
            return (sep + longStr + sep).IndexOf(sep + shortStr + sep) >= 0;
        }
        */

        public static string EmptyToValue(string data, string value)
        {
            return _Str.IsEmpty(data)
                ? value : data;
        }

        /// <summary>
        /// add directory seperator for path if need
        /// </summary>
        /// <param name="dir">now path</param>
        /// <returns></returns>
        public static string AddDirSep(string dir)
        {
            if (IsEmpty(dir))
                return _Fun.DirSep.ToString();
            if (dir.Substring(dir.Length - 1, 1) != "/" && dir.Substring(dir.Length - 1, 1) != "\\")
                dir += _Fun.DirSep;
            return dir;
        }

        /// <summary>
        /// add right slash for url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string AddSlash(string url)
        {
            if (IsEmpty(url))
                return "/";
            if (url.Substring(url.Length - 1, 1) != "/")
                url += "/";
            return url;
        }

        public static string RemoveRightSlash(string dir)
        {
            if (dir.Substring(dir.Length - 1, 1) == "/" || dir.Substring(dir.Length - 1, 1) == "\\")
                dir = dir[0..^1];
                //dir = dir.Substring(0, dir.Length - 1);
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
        public static string PreZero(int len, object obj, bool matchLen = false)
        {
            var str = obj.ToString();
            return (str.Length < len) ? new string('0', len - str.Length) + str :
                matchLen ? str[..len] :
                str;
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
            var data2 = new JObject { [fid] = data };
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

            try
            {
                return JObject.Parse(str);
            }
            catch
            {
                //_Log.Error("_Str.cs ToJson() error: " + str);
                return null;
            }
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
            return (_Str.IsEmpty(str))
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
            return (_Str.IsEmpty(str))
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
        /// get Md5 string(22 chars, base64)
        /// </summary>
        /// <param name="str"></param>
        /// <returns>22 char</returns>
        public static string Md5(string str)
        {
            var bytes = MD5.Create().ComputeHash(Encoding.Default.GetBytes(str));
            return Convert.ToBase64String(bytes)[..22]
                .Replace('+', '-')
                .Replace('/', '_');
        }

        /// <summary>
        /// get new Id for db key, 10 char
        /// max date is 2300/1/1(A+9char, from 2000/1/1)
        /// </summary>
        /// <returns></returns>
        public static string NewId(DateTime? dt = null)
        {
            //1.stop 1 milli second for avoid repeat(sync way here !!)
            Thread.Sleep(1);

            var ticks = (dt == null) ? DateTime.Now.Ticks : dt.Value.Ticks;
            var num = (ulong)((ticks - _startTicks) / TimeSpan.TicksPerMillisecond) * 3;
            using var sha1 = SHA256.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(num.ToString()));
            return _Fun.Config.ServerId + string.Concat(Convert.ToBase64String(hash)
                .ToCharArray()
                .Where(a => char.IsLetterOrDigit(a))
                .Take(9));
        }

        /*
        /// <summary>
        /// old version
        /// get new Id for db key, 10 char(upperCase), consider db index performance
        /// max date is 2300/1/1(9char+A, from 2000/1/1)
        /// </summary>
        /// <returns></returns>
        public static string NewId(DateTime? dt = null)
        {
            //1.stop 1 milli second for avoid repeat(sync way here !!)
            Thread.Sleep(1);
            //await Task.Delay(1);

            //2.get current time
            //var ticks = new DateTime(2500, 1, 1).Ticks;
            var ticks = (dt == null) ? DateTime.Now.Ticks : dt.Value.Ticks;
            var num = (ulong)((ticks - _startTicks)/ TimeSpan.TicksPerMillisecond) * 4;
            //var num = (ulong)(DateTime.Now.Millisecond - _startMilliSec);

            //3.convert to base34
            var data = "";
            ulong mod;
            while (num > 0)
            {
                mod = num % _baseLen;
                num /= _baseLen;
                data = _base34[(int)mod] + data;
            }

            //4.min length 5 chars, add pre '0'
            const int minLen = 5;
            if (data.Length < minLen)
                data = new string('0', minLen - data.Length) + data;

            //5.tail add 1 random char & server id for multiple web server
            //var tail = _base34[_Num.GetRandom((int)_baseLen - 1)];
            //data += tail + _Fun.Config.ServerId;
            //data += _base34[_Num.GetRandom((int)_baseLen - 1)];
            data += _Fun.Config.ServerId;
            return data;
        }
        */

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
            return Convert.ToString(Convert.FromHexString(hex));
            /*
            var date = "";
            while (hex.Length > 0)
            {
                //date += Convert.ToChar(Convert.ToUInt32(hex.Substring(0, 2), 16)).ToString();
                date += Convert.ToChar(Convert.ToUInt32(hex[..3], 16)).ToString();
                hex = hex[2..];
                //hex = hex.Substring(2, hex.Length - 2);
            }
            return date;
            */
        }

        /// <summary>
        /// list string add single quota(')
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
        public static async Task<bool> CheckKeyAsync(string str, bool logError = true)
        {
            if (_Str.IsEmpty(str))
                return true;

            Regex rg = new(@"^[a-zA-Z0-9,]*$");
            if (rg.IsMatch(str))
                return true;

            if (logError)
                await _Log.ErrorAsync($"_Str.CheckKeyAsync() failed ({str})");
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
            for (var i = 0; i < ary.Length; i += 2)
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
            if (pos1 < 0)
                return "";

            source = source[(left.Length + pos1)..];
            pos1 = source.IndexOf(right);
            if (pos1 > 0)
                source = source[..pos1];
            return source;
        }

        /// <summary>
        /// get left part string(first find), not include found string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="find">find string</param>
        /// <returns></returns>
        public static string GetLeft(string source, string find)
        {
            var pos = source.IndexOf(find);
            return (pos < 0) ? source : source[..pos];
        }

        /// <summary>
        /// get left part string(last find), not include found string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        public static string GetLeft2(string source, string find)
        {
            var pos = source.LastIndexOf(find);
            return (pos < 0) ? source : source[..pos];
        }

        /// <summary>
        /// get left part string, not include found string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="find">find string</param>
        /// <returns></returns>
        public static string GetRight(string source, string find)
        {
            var pos = source.IndexOf(find);
            return (pos < 0) ? source : source[(pos + 1)..];
        }

        /// <summary>
        /// AES encode, ECB mode, padding PKCS7
        /// </summary>
        /// <param name="data">source data</param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns>encoded base64 string</returns>
        public static string AesEncode(string data, string key, string iv)
        {
            byte[] bytes;
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.ASCII.GetBytes(key);
                aes.IV = Encoding.ASCII.GetBytes(iv);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(data);
                }
                bytes = ms.ToArray();
            }
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// AES decode, ECB mode, padding PKCS7
        /// </summary>
        /// <param name="data">encoded base64 string</param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns>decoded plain text</returns>
        public static string AesDecode(string data, string key, string iv)
        {
            string result;
            var bytes = Convert.FromBase64String(data);
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.ASCII.GetBytes(key);
                aes.IV = Encoding.ASCII.GetBytes(iv);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                var encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(bytes);
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Read);
                using var sw = new StreamReader(cs);
                result = sw.ReadToEnd();
            }
            return result;
        }

        //get guid base36 string(25 char)
        public static string Guid36()
        {
            return ToBaseStr(GuidBytes(), _base36);
        }
        //get guid base62 string(22 char)
        public static string Guid62()
        {
            return ToBaseStr(GuidBytes(), _base62);
        }

        private static byte[] GuidBytes()
        {
            return Guid.NewGuid().ToByteArray();
        }

        /// <summary>
        /// see: https://github.com/ghost1face/base62/blob/master/Base62/Base62Converter.cs BaseConvert()
        /// </summary>
        /// <param name="source"></param>
        /// <param name="baseChars"></param>
        /// <returns></returns>
        private static string ToBaseStr(byte[] source, char[] baseChars)
        {
            //modify
            //var result = new List<int>();
            var targetBase = baseChars.Length;
            var result = "";

            int count;
            while ((count = source.Length) > 0)
            {
                var quotient = new List<byte>();
                int remainder = 0;
                for (var i = 0; i != count; i++)
                {
                    int accumulator = source[i] + remainder * 256;  //fix to 256
                    byte digit = (byte)((accumulator - (accumulator % targetBase)) / targetBase);
                    remainder = accumulator % targetBase;
                    if (quotient.Count > 0 || digit != 0)
                    {
                        quotient.Add(digit);
                    }
                }

                //result.Insert(0, remainder);
                result = baseChars[remainder] + result; //to baseBytes[]
                source = quotient.ToArray();
            }

            //modify
            return result;
            /*
            var output = new byte[result.Count];
            for (int i = 0; i < result.Count; i++)
                output[i] = (byte)result[i];

            return output;
            */
        }

        /// <summary>
        /// file extension to http content type, for download file
        /// </summary>
        /// <param name="ext">file ext both has dot or not</param>
        /// <returns></returns>
        /*
        public static string FileExtToContentType(string ext)
        {
            ext = ext.Replace(".", "");
            var type = (ext == "xls" || ext == "xlsx") ? "application/msexcel" :
                (ext == "doc" || ext == "docx") ? "application/ms-word" :
                (ext == "pdf") ? "application/pdf" :
                "";

            if (type == "")
                _Log.Error("_Str.cs FileExtToContentType() failed, ext wrong (" + ext + ")");
            return type;
        }
        */

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