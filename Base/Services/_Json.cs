using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Base.Services
{
    public class _Json
    {

        /*
        public static JObject StrToJson(string str)
        {
            return _Str.IsEmpty(str) ? null : JObject.Parse(str);
        }
        */

        //to json string, return "{}" if input null for jquery parsing !!
        public static string ToStr(JObject json)
        {
            return (json == null) ? "{}" : json.ToString(Formatting.None);
        }

        public static JArray StrToArray(string str)
        {
            return _Str.IsEmpty(str) ? null : JArray.Parse(str);
        }

        /*
        public static JObject SystemError(string error)
        {
            return new JObject
            {
                ["SystemError"] = error
            };
        }
        */

        public static JObject GetError(string error = "")
        {
            return new JObject { ["ErrorMsg"] = _Str.EmptyToValue(error, _Fun.SystemError) };
        }

        /// <summary>
        /// get js _BR error
        /// </summary>
        /// <param name="fid">_BR fid</param>
        /// <returns></returns>
        public static JObject GetBrError(string fid)
        {
            return new JObject { ["ErrorMsg"] = _Fun.PreBrError + fid };
        }

        /*
        public static void AddFieldError(JObject json, string fid, string error)
        {
            if (json["FieldError"] == null)
                json["FieldError"] = new JObject();

            json["FieldError"][fid] = error;
        }
        */

        /// <summary>
        /// json array to string
        /// </summary>
        public static string ArrayToStr(JArray rows)
        {
            return (rows == null || rows.Count == 0) 
                ? null 
                : rows.ToString(Formatting.Indented);
        }

        //sort json array
        public static JArray SortArray(JArray rows, string fid)
        {
            //put empty field to last
            return new JArray(rows.OrderBy(a => (a[fid].ToString() == "") ? "zzz" : a[fid].ToString()));
        }

        //convert array to list string
        public static List<string> ArrayToListStr(JArray rows, string fid)
        {
            var data = new List<string>();
            foreach (var row in rows)
                data.Add(row[fid].ToString());
            return data;
        }

        /// <summary>
        /// json array to string list(default seperate with ",")
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public static string ArrayToStrs(JArray rows, string fid, bool addQuote = true, string sep = ",")
        {
            var quote = addQuote ? "'" : "";
            var data = "";
            foreach (var row in rows)
                data += quote + row[fid] + quote + sep;
            return (data == "")
                ? ""
                : data[..^sep.Length];
        }

        //find json array
        public static JArray FindArray(JArray rows, string fid, string value)
        {
            if (rows == null)
                return null;

            var finds = new JArray();
            foreach (var row in rows)
            {
                if (row[fid].ToString() == value)
                    finds.Add(row);
            }
            return finds.Count == 0 ? null : finds;
        }

        //find json array 1 row
        public static JObject FindArray1(JArray rows, string fid, string value)
        {
            var finds = FindArray(rows, fid, value);
            return (finds == null) ? null : (JObject)finds[0];
        }

        //get left line??
        private static string GetLeftLine(string src, int rightPos)
        {
            var pos = src.LastIndexOf('\n', rightPos - 1);
            pos = (pos < 0) ? 0 : pos + 1;
            return src[pos..rightPos];
        }

        //JArray to string[], JArray must be string[] !!
        public static string[] ArrayToStrArray(JArray rows)
        {
            return rows.ToObject<string[]>();
        }

        /// <summary>
        /// remove last one char for cross report result
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="fid"></param>
        /// <param name="tail"></param>
        /// <returns></returns>
        public static void ArrayRemoveTailStr(ref JArray rows, string fid, string tail)
        {
            var tailLen = tail.Length;
            foreach (var row in rows)
            {
                var value = row[fid].ToString();
                var len = value.Length - tailLen;
                if (value[len..] == tail)
                    row[fid] = value[..len];
            }
        }

        //get key list
        public static List<string> GetKeys(JObject row)
        {
            var data = new List<string>();
            foreach (var item in row)
                data.Add(item.Key);
            return data;
        }

        //copy json to static class
        //https://stackoverflow.com/questions/7334067/how-to-get-fields-and-their-values-from-a-static-class-in-referenced-assembly
        public static void CopyToStaticClass(JObject from, Type type)
        {
            //Type type = typeof(T);
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                var key = field.Name;
                if (from[key] != null)
                {
                    field.SetValue(null, from[key].ToString());
                }
            }
        }

        /// <summary>
        /// copy properties of json/model(from) to json/model(to)
        /// 新版newtonsoft JObject 不支援GetType().GetProperties(), 改用linq寫法
        /// model仍然用GetType().GetProperties()來讀取property
        /// </summary>
        /// <param name="from"></param>
        /// <param name="model"></param>
        public static void CopyToModel(JObject from, object model)
        {
            //var models = model.GetType().GetProperties();
            foreach (var item in from)
            {
                var modelProp = model.GetType().GetProperty(item.Key);
                if (modelProp != null)
                    modelProp.SetValue(model, item.Value.ToString(), null);
            }
            /* 舊的寫法
            foreach (var prop in from.GetType().GetProperties())
            {
                var toProp = to.GetType().GetProperty(prop.Name);
                if (toProp != null)
                    toProp.SetValue(to, prop.GetValue(from, null), null);
            }
            */
        }

        //is empty or not
        public static bool IsEmpty(JObject json)
        {
            return (json == null || !json.HasValues);
        }

        //json是否為空白, 並且不考慮底線欄位
        public static bool IsEmptyBySkipUnderLine(JObject json)
        {
            if (IsEmpty(json))
                return true;

            foreach(var item in json)
            {
                if (item.Key[..1] != "_" && item.Value != null)
                    return false;
            }

            //case else
            return true;
        }

        /// <summary>
        /// read inputJson(CrudEdit.cs) and get first row
        /// </summary>
        /// <param name="inputJson"></param>
        /// <returns></returns>
        public static JObject ReadInputJson0(JObject inputJson)
        {
            var fid = "_rows";  //same to CrudEdit.Rows
            if (inputJson[fid] == null)
                return null;

            var rows = inputJson[fid] as JArray;
            return rows[0] as JObject;
        }

        public static bool IsFidEqual(JObject json, string fid, string value)
        {
            return (json == null || string.IsNullOrEmpty(value)) ? false :
                (json[fid].ToString() ==  value);
        }

        /// <summary>
        /// get child rows from upJson
        /// </summary>
        /// <param name="upJson">input row</param>
        /// <param name="childIdx">child index</param>
        /// <returns>JArray</returns>
        public static JArray GetChildRows(JObject upJson, int childIdx, string fidRows = _Fun.Rows, string fidChilds = _Fun.Childs)
        {
            var child = GetChildJson(upJson, childIdx, fidChilds);
            return (child == null || child[fidRows] == null)
                ? null
                : child[fidRows] as JArray;
        }

        /// <summary>
        /// get child json from upJson
        /// </summary>
        /// <param name="upJson"></param>
        /// <param name="childIdx"></param>
        /// <returns></returns>
        public static JObject GetChildJson(JObject upJson, int childIdx, string fidChilds = _Fun.Childs)
        {
            if (upJson == null || upJson[fidChilds] == null)
                return null;

            //JArray childs = upJson[Childs] as JArray;
            return (upJson[fidChilds].Count() <= childIdx
                    || _Json.IsEmpty(upJson[fidChilds][childIdx] as JObject))
                ? null
                : upJson[fidChilds][childIdx] as JObject;
        }

        /*
        //copy json to static class
        public static void CopyToModel<T>(JObject from, T to) where T : class
        {
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                var key = field.Name;
                if (from[key] != null)
                {
                    field.SetValue(null, from[key].ToString());
                }
            }
        }
        */

        #region remark code
        /*
        /// <summary>
        /// convert IEnumerable<dynamic> to json string
        /// </summary>
        /// <param name="rows">rows</param>
        /// <returns>json string</returns>
        public static string EnumerableToString(IEnumerable<dynamic> rows)
        {
            return new JavaScriptSerializer().Serialize(rows);
        }

        /// <summary>
        /// ??
        /// 將某個字串以 p_row 的內容取代, 保留欄位名稱參數 replaceStr()
        /// </summary>
        /// <param name="ps_source">原始字串</param>
        /// <param name="p_row">要取代的 JsonObject 資料</param>
        /// <param name="pb_html">是否為 html 資料, 如果是, 則系統會自動置換換行符號</param>
        /// <returns>新字串</returns>
        public static string IntoStr(JObject row, string src)
        {
            return ArrayIntoStr(new JArray() { row }, src);
        }

        /// <summary>
        /// ??
        /// 將某個字串以 rows 的內容取代, 用在置換 mail body 和 inf 檔案內容, 
        /// 搜尋的步驟為尋找 ps_source 裡的 [xxx], 再和 pa_row 比對, 如果找不到, 則不會做置換這個欄位. (以免破壞 sql 的內容)
        /// 保留欄位名稱: _now, _today, _userId, _userName, _loginId, _deptId, _deptName
        /// 可使用的變數: cf(config), db(db), ss(session), f2(fun2)
        /// </summary>
        /// <param name="rows">要取代的 JsonArray 資料</param>
        /// <param name="src">原始字串</param>
        /// //<param name="isHtml">是否為 html 資料, 如果是, 則系統會自動置換換行符號</param>
        /// <returns>新字串</returns>
        public static string ArrayIntoStr(JArray rows, string src)
        {
            const string endTag = "<<_end>>";
            int startPos = 0;
            int rowNo;
            int leftPos, rightPos;
            string findStr, fid, value;
            JObject row;
            var finds = new List<string>();     //find
            var replaces = new List<string>();  //replace
            while (true)
            {
                //initial
                value = "??";   //flag for not found

                //find "<<"
                leftPos = src.IndexOf("<<", startPos);
                if (leftPos < 0)
                    break;

                //find ">>"
                rightPos = src.IndexOf(">>", leftPos + 1);
                if (rightPos < 0)
                    break;

                findStr = src.Substring(leftPos, rightPos - leftPos + 2); //find string
                fid = findStr.Substring(2, findStr.Length - 4);
                //fid = src.Substring(leftPos + 2, rightPos - leftPos - 2); //find string
                startPos = rightPos + 1;
                //startPos = leftPos + 1;     //考慮 "[[" 的情形

                //=== case of 公用變數 ===
                if (findStr == endTag)
                {
                    continue;
                }
                else if (fid.Substring(0, 1) == "_")
                {
                    switch (fid)
                    {
                        case "_Now":    //現在時間 (yyyy/MM/dd HH:mm:ss)
                            value = _Date.NowStr();
                            break;
                        case "_Today":  //今天日期 (yyyy/MM/dd)
                            value = _Date.TodayStr();
                            break;
                        case "_Year":   //西元年度
                            value = DateTime.Now.Year.ToString();
                            break;
                        case "_twYear": //民國年度
                            value = (DateTime.Now.Year - 1911).ToString();
                            break;
                        default:
                            continue;  //do nothing
                    }
                }

                //=== 多筆情形(m:x) ===
                else if (fid.Substring(0, 2) == "m:")
                {
                    if (!int.TryParse(fid.Substring(2, 1), out rowNo))
                        continue;

                    //尋找 fid 左邊的換行符號
                    var leftLine = GetLeftLine(src, leftPos);
                    var leftLine2 = "";
                    value = "";
                    foreach (string row2 in rows[rowNo] as JArray)
                    {
                        //row2為多行時, 將換行符號加上左側空白 for 對齊 !!
                        value += leftLine2 + row2.Replace(_Fun.TextCarrier, _Fun.TextCarrier + leftLine) + _Fun.TextCarrier;
                        leftLine2 = leftLine;
                    }
                    if (value != "")
                        value = value.Substring(0, value.Length - 2);   //移除最後的換行符號
                }

                //=== 條件符合才顯示 ===
                else if (fid.IndexOf("=") > 0)
                {
                    //調整資料
                    var fid2 = fid.Replace("==", "=").Replace(" ", "");
                    var cols = fid2.Split('=');
                    if (cols.Length != 2)
                        continue;

                    //尋找 <<end>>
                    //var end = "<<end>>";
                    var endPos = src.IndexOf(endTag, rightPos + 2);
                    if (endPos < 0)
                        continue;

                    //check
                    row = rows[0] as JObject;
                    if (row[cols[0]] == null)
                        continue;

                    if (row[cols[0]].ToString() == cols[1])
                    {
                        //case of 條件符合(移除條件)
                        findStr = GetLeftLine(src, leftPos) + findStr + _Fun.TextCarrier;  //同時左邊空白 & 移除換行
                        value = "";
                    }
                    else
                    {
                        //case of 條件不符合(移除條件, 區段內容, 條件結尾, 換行符號)
                        src = src.Substring(0, leftPos) + src.Substring(endPos + endTag.Length + 2);
                        startPos = leftPos + 2;    //重新設定搜尋位置 !!
                        continue;
                    }
                }

                //=== case of 單筆(可以有2個以上) ===
                else
                {
                    if (fid.IndexOf(":") > 0)
                    {
                        //2nd row
                        if (!int.TryParse(fid.Substring(0, 1), out rowNo))
                            continue;
                        fid = fid.Substring(2);
                    }
                    else
                    {
                        rowNo = 0;
                    }

                    row = rows[rowNo] as JObject;
                    if (row[fid] == null)
                        continue;
                    value = row[fid].ToString();
                }

                //add to finds/replaces
                if (value != "??" && !finds.Any(a => a == fid))
                {
                    finds.Add(findStr);
                    replaces.Add(value);
                }
            }

            //加上其他要取代的內容
            finds.Add(endTag + _Fun.TextCarrier);   //同時移除換行
            replaces.Add("");

            //replace string[]
            for (int i = 0; i < finds.Count; i++)
                src = src.Replace(finds[i], replaces[i]);

            //lab_exit:
            //replace carrier if need.
            //if (isHtml)
            //    src = src.Replace(_Fun.TextCarrier, _Fun.HtmlCarrier);

            return src;
        }
        */
        #endregion
    }//class
}