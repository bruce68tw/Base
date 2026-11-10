using Base.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Base.Services
{
    /// <summary>
    /// Newtonsoft JSON 與公開屬性反射的模型轉換工具。
    /// </summary>
    public class _Model
    {
        /// <summary>
        /// 將公開屬性串成名稱與值的顯示文字，非可還原的表單編碼。
        /// </summary>
        /// <param name="model">要讀取、寫入或序列化的模型</param>
        public static string ToFormStr<T>(T model)
        {
            //if (model == null) return "";
            var result = "";
            foreach (var prop in model!.GetType().GetProperties())
                result += prop.Name + "=" + prop.GetValue(model, null) + ", ";
            return result;
        }

        /// <summary>
        /// 以 JsonConvert 序列化模型，包含其預設支援的公開欄位。
        /// </summary>
        /// <param name="model">要讀取、寫入或序列化的模型</param>
        public static string ToJsonStr<T>(T model)
        {
            //if (model == null) return "";
            return JsonConvert.SerializeObject(model);
            /*
            var result = new JObject();
            //foreach (var prop in model.GetType().GetProperties())
            var props = Activator.CreateInstance<T>().GetType().GetProperties();
            foreach (var prop in props)
                result[prop.Name] = prop.GetValue(model, null).ToString();

            return _Json.ToStr(result);
            */
        }

        /// <summary>
        /// 僅列舉公開屬性，將值轉成字串或空字串；不保留原始 JSON 型別。
        /// </summary>
        /// <param name="model">要讀取、寫入或序列化的模型</param>
        public static JObject ToJson<T>(T model)
        {
            //if (model == null) return null;
            var result = new JObject();
            foreach (var prop in model!.GetType().GetProperties())
            {
                var value = prop.GetValue(model, null);
                result[prop.Name] = (value == null) ? "" : value.ToString();
            }
            return result;
        }

        /// <summary>
        /// 將同名公開屬性淺拷貝至新模型，不複製欄位或自動轉型。
        /// </summary>
        /// <param name="from">提供要複製資料的來源物件</param>
        public static TTo Copy<TFrom, TTo>(TFrom from) where TTo : new()
        {
            //if (from == null) return default;

            var result = new TTo();
            foreach (var prop in from!.GetType().GetProperties())
            {
                var fid = prop.Name;
                if (HasProp<TTo>(result, fid))
                    SetValue<TTo>(result, fid, prop!.GetValue(from, null)!);
            }
            return result;
        }

        /*
        public static List<PropertyInfo> GetProps<T>(T model) where T : new()
        {
            return (model == null)
                ? null
                : model.GetType().GetProperties();
        }
        */

        /// <summary>
        /// move to _Str.ToModel()
        /// 反序列化 JSON 為模型，格式錯誤可拋出例外。
        /// </summary>
        /*
        public static T? StrToModel<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }
        */

        /// <summary>
        /// move to _Str.ToModels()
        /// 反序列化 JSON 陣列為模型清單。
        /// </summary>
        /*
        public static List<T>? StrToModels<T>(string str)
        {
            return JsonConvert.DeserializeObject<List<T>>(str);
        }
        */

        /// <summary>依名稱讀取模型的公開屬性，屬性不存在時回傳 null。</summary>
        /// <param name="model">要讀取、寫入或序列化的模型</param>
        /// <param name="fid">要處理的欄位識別值</param>
        public static object? GetValue<T>(T model, string fid)
        {
            //if model hasn't this property, log error
            var prop = model!.GetType().GetProperty(fid);
            return prop?.GetValue(model, null);
        }

        /// <summary>
        /// 寫入公開實例屬性；屬性不存在回傳 false，型別不符仍可能拋出。
        /// </summary>
        /// <param name="model">要讀取、寫入或序列化的模型</param>
        /// <param name="fid">要處理的欄位識別值</param>
        /// <param name="value">要使用的輸入值</param>
        public static bool SetValue<T>(T model, string fid, object value)
        {
            //if model hasn't this property, log error
            var prop = model!.GetType().GetProperty(fid, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return false;

            prop.SetValue(model, value, null);
            return true;
        }

        /// <summary>
        /// 判斷模型是否具有指定名稱的公開屬性。
        /// </summary>
        /// <param name="model">要讀取、寫入或序列化的模型</param>
        /// <param name="fid">要處理的欄位識別值</param>
        public static bool HasProp<T>(T model, string fid)
        {
            return (model!.GetType().GetProperty(fid) != null);
        }

        /// <summary>
        /// 以 Hashtable 填入同名屬性；Int32 特別轉換，其餘值轉字串。
        /// </summary>
        /// <param name="hash">提供欄位名稱與值的 Hashtable</param>
        public static T HashToModel<T>(Hashtable hash) where T : new()
        {
            T model = new();
            foreach (var prop in model.GetType().GetProperties())
            {
                if (!hash.Contains(prop.Name)) continue;
                if (prop.PropertyType == typeof(int))
                    prop.SetValue(model, Convert.ToInt32(hash[prop.Name]), null);
                else
                    prop.SetValue(model, hash[prop.Name]!.ToString(), null);

            }
            return model;
        }

        /// <summary>
        /// 以 JsonConvert 序列化模型清單。
        /// </summary>
        /// <param name="rows">要處理的資料列清單</param>
        public static string ListToJsonStr<T>(List<T> rows)
        {
            return JsonConvert.SerializeObject(rows);
        }

        /// <summary>
        /// 建立錯誤結果，空訊息改用系統錯誤文字。
        /// </summary>
        /// <param name="error">要記錄或回傳的錯誤訊息</param>
        public static ResultDto GetError(string error = "")
        {
            return new ResultDto() { _ErrorMsg = _Str.EmptyToValue(error, _Fun.SystemError) };
        }

        /// <summary>
        /// 建立帶有欄位驗證錯誤清單的結果。
        /// </summary>
        /// <param name="errors">要附加至結果的欄位驗證錯誤清單</param>
        public static ResultDto GetValidError(List<ErrorRowDto> errors)
        {
            return new ResultDto() { ErrorRows = errors };
        }

        /// <summary>
        /// 建立以 B: 前綴表示語系錯誤鍵的結果。
        /// </summary>
        /// <param name="fid">要處理的欄位識別值</param>
        public static ResultDto GetBrError(string fid)
        {
            return new ResultDto() { _ErrorMsg = _Fun.PreBrError + fid };
        }

    } //class
}
