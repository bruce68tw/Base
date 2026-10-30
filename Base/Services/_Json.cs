using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Base.Services
{
    /// <summary>
    /// Newtonsoft JObject、JArray 與 CRUD 固定鍵的處理工具；多數寫入方法直接修改輸入物件。
    /// </summary>
    public class _Json
    {
        /// <summary>
        /// 將 JObject 轉為模型，null 輸入回傳 default。
        /// </summary>
        public static T? ToModel<T>(JObject json)
        {
            return (json == null)
                ? default
                : json.ToObject<T>()!;
        }

        /// <summary>
        /// 建立單一字串欄位的 JObject。
        /// </summary>
        public static JObject PairToJson(string fid, string value)
        {
            return new JObject { [fid] = value };
        }
        /// <summary>
        /// 建立單一字串欄位並轉成 JSON 文字。
        /// </summary>
        public static string PairToJsonStr(string fid, string value)
        {
            return PairToJson(fid, value).ToString();
        }

        /// <summary>
        /// 取得分頁 JSON 的 data 陣列，缺失或型別不符可為 null。
        /// </summary>
        public static JArray? PageGetRows(JObject? json)
        {
            return (json == null || json["data"] == null)
                ? null
                : json["data"] as JArray;
        }

        /// <summary>
        /// 將陣列或單列包成 _rows，直接更新傳入 JSON。
        /// </summary>
        public static void SetRows(JObject json, JArray rows)
        {
            json[_Fun.FidRows] = rows;
        }
        /// <summary>
        /// 將陣列或單列包成 _rows，直接更新傳入 JSON。
        /// </summary>
        public static void SetRows(JObject json, JObject row)
        {
            json[_Fun.FidRows] = new JArray(row);
        }

        /// <summary>
        /// 取代既有 _childs 中指定索引的物件；結構缺失時記錄並回傳 false。
        /// </summary>
        public static bool SetChildJson(JObject upJson, int childIdx, JObject childJson)
        {
            var error = "";
            const string fid = _Fun.FidChilds;
            if (upJson == null || upJson[fid] == null)
            {
                error = "upJson or childs is empty.";
                goto lab_error;
            }

            var childs = (upJson[fid]! as JArray)!;
            if (childs.Count < childIdx + 1)
            {
                error = $"childs.Count < arg value({childIdx})";
                goto lab_error;
            }

            upJson[fid]![childIdx] = childJson;
            return true;

        lab_error:
            _Log.Error("_Json.SetChildJson() failed: " + error);
            return false;
        }

        /// <summary>
        /// 建立必要的 _childs 項目並填入 _rows，回傳該子物件。
        /// </summary>
        public static JObject SetChildRows(JObject upJson, int childIdx, JArray rows)
        {
            // 假設 _edit.Childs 和 _edit.Rows 是字串變數
            var fid = _Fun.FidChilds;

            // 1. 如果 upJson 為 null，初始化為新的 JObject
            if (upJson == null)
                upJson = new JObject();

            // 2. 如果 upJson[fid] 為 null，初始化為新的 JArray
            if (upJson[fid] == null)
                upJson[fid] = new JArray();

            // 3. C# 陣列無法直接跨索引賦值，需要補足中間的空物件
            var childs = (upJson[fid] as JArray)!;
            while (childs.Count <= childIdx)
            {
                childs.Add(new JObject());
            }

            // 4. 取得指定索引的 child 物件並賦值
            var child = (JObject)childs[childIdx];
            child[_Fun.FidRows] = rows;
            return child;
        }

        /*
        public static JObject StrToJson(string str)
        {
            return _Str.IsEmpty(str) ? null : JObject.Parse(str);
        }
        */

        /// <summary>
        /// 輸出不含縮排的 JSON 字串。
        /// </summary>
        public static string ToStr(JObject json)
        {
            return json.ToString(Formatting.None);
        }

        /// <summary>
        /// 解析 JSON 陣列，空值回傳 null，無效格式會拋出。
        /// </summary>
        public static JArray? StrToArray(string? str)
        {
            return _Str.IsEmpty(str) ? null : JArray.Parse(str!);
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

        /// <summary>
        /// 讀取欄位字串，欄位不存在時回傳空字串。
        /// </summary>
        public static string NullToEmpty(JObject json, string fid)
        {
            return (json[fid] == null) 
                ? "" : json[fid]!.ToString();
        }

        /// <summary>
        /// 建立 _ErrorMsg 物件，空訊息改用系統錯誤文字。
        /// </summary>
        public static JObject GetError(string error = "")
        {
            return new JObject { [_Fun.FidErrorMsg] = _Str.EmptyToValue(error, _Fun.SystemError) };
        }

        /// <summary>
        /// 建立 _ErrorMsg 為 B:語系鍵 的物件。
        /// </summary>
        /// <param name="fid">_BR fid</param>
        /// <returns></returns>
        public static JObject GetBrError(string fid)
        {
            return new JObject { [_Fun.FidErrorMsg] = _Fun.PreBrError + fid };
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
        /// 輸出縮排陣列 JSON，空陣列回傳 null。
        /// </summary>
        public static string? ArrayToStr(JArray rows)
        {
            return (rows.Count == 0) 
                ? null : rows.ToString(Formatting.Indented);
        }

        /// <summary>
        /// 依欄位字串排序成新陣列；空字串以 zzz 作排序替代值。
        /// </summary>
        public static JArray SortArray(JArray rows, string fid)
        {
            //put empty field to last
            return new JArray(rows.OrderBy(a => (a[fid]!.ToString() == "") ? "zzz" : a[fid]!.ToString()));
        }

        /// <summary>
        /// 依各列指定欄位建立字串清單，欄位須存在。
        /// </summary>
        public static List<string> ArrayToListStr(JArray rows, string fid)
        {
            var data = new List<string>();
            foreach (var row in rows)
                data.Add(row[fid]!.ToString());
            return data;
        }

        /// <summary>
        /// 連接非空欄位值，可加引號與分隔符，不跳脫內容。
        /// </summary>
        public static string ArrayToStrs(JArray rows, string fid, bool addQuote = true, string sep = ",")
        {
            var quote = addQuote ? "'" : "";
            var data = "";
            foreach (var row in rows)
            {
                if (_Var.NotEmpty(row[fid]))
                    data += quote + row[fid] + quote + sep;
            }
            return (data == "")
                ? "" : data[..^sep.Length];
        }

        /// <summary>
        /// 篩選欄位字串等於指定值的列，無結果回傳 null。
        /// FilterArray -> Filter
        /// </summary>
        public static JArray? Filter(JArray rows, string fid, string value)
        {
            //if (rows == null) return null;

            var finds = new JArray();
            foreach (var row in rows)
                if (row[fid]!.ToString() == value) finds.Add(row);
            return (finds.Count == 0) 
                ? null : finds;
        }

        /// <summary>
        /// 取得首個符合欄位字串的 JObject，無結果回傳 null。
        /// FindArray -> FilterOne
        /// </summary>
        public static JObject? FilterOne(JArray rows, string fid, string value)
        {
            var finds = Filter(rows, fid, value);
            return (finds == null) ? null : (JObject)finds[0];
        }

        //get left line??
        private static string GetLeftLine(string src, int rightPos)
        {
            var pos = src.LastIndexOf('\n', rightPos - 1);
            pos = (pos < 0) ? 0 : pos + 1;
            return src[pos..rightPos];
        }

        /// <summary>
        /// 將 JArray 轉成字串陣列，內容不符時可能拋出。
        /// </summary>
        public static string[] ArrayToStrArray(JArray rows)
        {
            return rows.ToObject<string[]>()!;
        }

        /// <summary>
        /// 原地移除各列欄位的指定尾碼；欄位長度須至少與尾碼相同。
        /// </summary>
        public static void ArrayRemoveTailStr(ref JArray rows, string fid, string tail)
        {
            var tailLen = tail.Length;
            foreach (var row in rows)
            {
                var value = row[fid]!.ToString();
                var len = value.Length - tailLen;
                if (value[len..] == tail)
                    row[fid] = value[..len];
            }
        }

        /// <summary>
        /// 取得 JObject 的欄位名稱清單。
        /// </summary>
        public static List<string> GetKeys(JObject row)
        {
            var data = new List<string>();
            foreach (var item in row)
                data.Add(item.Key);
            return data;
        }

        /// <summary>
        /// 以字串值寫入同名公開靜態欄位，欄位型別不符時可能拋出。
        /// </summary>
        public static void CopyToStaticClass(JObject from, Type type)
        {
            //Type type = typeof(T);
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                var key = field.Name;
                if (from[key] != null) 
                    field.SetValue(null, from[key]!.ToString());
            }
        }

        /// <summary>
        /// 將 JSON 轉型後寫入同名可寫屬性，略過不存在或轉型失敗的屬性。
        /// 注意: 新版newtonsoft JObject 不支援GetType().GetProperties(), 改用linq寫法
        /// </summary>
        public static void CopyToModel(JObject from, object model)
        {
            var type = model.GetType();
            foreach (var item in from)
            {
                var prop = type.GetProperty(item.Key);
                if (prop == null || !prop.CanWrite) continue;

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                try
                {
                    var value = item.Value?.ToObject(targetType);
                    prop.SetValue(model, value);
                }
                catch
                {
                    // 忽略不相容的屬性類型
                    //todo: log error
                }
            }

            /*
            //var models = model.GetType().GetProperties();
            foreach (var item in from)
            {
                var modelProp = model.GetType().GetProperty(item.Key);
                if (modelProp != null)
                    modelProp.SetValue(model, item.Value!.ToString(), null);
            }
            */

            /* 舊的寫法
            foreach (var prop in from.GetType().GetProperties())
            {
                var toProp = to.GetType().GetProperty(prop.Name);
                if (toProp != null)
                    toProp.SetValue(to, prop.GetValue(from, null), null);
            }
            */
        }

        /// <summary>
        /// 判斷 JObject 為 null 或沒有欄位。
        /// </summary>
        public static bool IsEmpty(JObject? json)
        {
            return (json == null || !json.HasValues);
        }

        /// <summary>
        /// 判斷 JObject 非 null 且含有欄位。
        /// </summary>
        public static bool NotEmpty(JObject? json)
        {
            return !IsEmpty(json);
        }

        /// <summary>
        /// 判斷物件或欄位缺失，或欄位字串為空。
        /// </summary>
        public static bool IsFidEmpty(JObject? json, string fid)
        {
            return (IsEmpty(json) || json![fid] == null || json![fid]!.ToString() == "");
        }

        /// <summary>
        /// 判斷指定欄位存在且字串非空。
        /// </summary>
        public static bool NotFidEmpty(JObject? json, string fid)
        {
            return !IsFidEmpty(json, fid);
        }

        /// <summary>
        /// 忽略底線開頭的特殊欄位後判斷是否沒有一般欄位，不按字串內容判空。
        /// </summary>
        public static bool IsEmptyNoSpec(JObject? json)
        {
            if (IsEmpty(json)) return true;

            foreach(var item in json!)
            {
                if (item.Key[..1] != "_" && item.Value != null)
                    return false;
            }

            //case else
            return true;
        }

        /// <summary>
        /// 取得 _rows 首列；鍵缺失回傳 null，但空陣列或結構錯誤可能拋出。
        /// </summary>
        public static JObject? GetRows0(JObject json)
        {
            //var fid = "_rows";  //same to CrudEdit.Rows
            if (json[_Fun.FidRows] == null) return null;

            var rows = json[_Fun.FidRows] as JArray;
            return rows![0] as JObject;
        }

        /// <summary>
        /// 判斷欄位存在且字串等於指定值。
        /// </summary>
        public static bool IsFidEqual(JObject json, string fid, string value)
        {
            return (json[fid] != null && json[fid]!.ToString() ==  value);
        }

        /// <summary>
        /// 依從 0 起算的子索引讀取 _rows，結構缺失可為 null。
        /// </summary>
        /// <param name="upJson">input row</param>
        /// <param name="childIdx">child index</param>
        /// <returns>JArray</returns>
        public static JArray? GetChildRows(JObject upJson, int childIdx)
        {
            var child = GetChildJson(upJson, childIdx);
            return (child == null || child[_Fun.FidRows] == null)
                ? null : child[_Fun.FidRows] as JArray;
        }
        /// <summary>
        /// 取得指定子項目的首列，無陣列時回傳 null；空陣列可能拋出。
        /// </summary>
        public static JObject? GetChildRows0(JObject upJson, int childIdx)
        {
            var rows = GetChildRows(upJson, childIdx);
            return (rows == null)
                ? null : rows![0] as JObject;

        }

        /// <summary>
        /// 取得指定 _childs 物件，缺失、空物件或索引超出時回傳 null。
        /// </summary>
        public static JObject? GetChildJson(JObject upJson, int childIdx)
        {
            const string fid = _Fun.FidChilds;
            if (upJson == null || upJson[fid] == null) return null;

            //JArray childs = upJson[Childs] as JArray;
            return (upJson[fid]!.Count() <= childIdx
                    || IsEmpty(upJson[fid]![childIdx] as JObject))
                ? null : upJson[fid]![childIdx] as JObject;
        }

		/// <summary>
		/// 取得非空欄位字串，物件或欄位缺失及空值時回傳 defaultValue。
		/// </summary>
		public static string GetFidStr(JObject? json, string fid, string defaultValue = "")
		{
			if (json == null || json[fid] == null)
				return defaultValue;

			var value = json[fid]!.ToString();
			return _Str.IsEmpty(value) ? defaultValue : value;
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
