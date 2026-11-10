using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Base.Services
{
    /// <summary>
    /// 物件空值判斷與公開屬性的反射存取。
    /// </summary>
    public static class _Object
    {
        /// <summary>
        /// 判斷動態物件是否為 JObject；判斷發生例外時回傳 false。
        /// </summary>
        /// <param name="item">要判斷型別的輸入物件</param>
        public static bool IsJObject(dynamic item)
        {
            try
            {
                // 嘗試轉型為 JObject，如果成功則回傳 true
                return item is JObject || item.GetType() == typeof(JObject);
            }
            catch
            {
                // 動態物件類型不符，回傳 false
                return false;
            }
        }

        /// <summary>
        /// 判斷物件為 null 或字串表示為空字串，不將空白視為空值。
        /// </summary>
        /// <param name="data">要處理的輸入值</param>
        public static bool IsEmpty(object? data)
        {
            return (data == null || data.ToString() == "");
        }

        /// <summary>
        /// 判斷物件非 null 且字串表示非空。
        /// </summary>
        /// <param name="data">要處理的輸入值</param>
        public static bool NotEmpty(object? data)
        {
            return !IsEmpty(data);
        }

        /// <summary>
        /// 將 null 轉空字串，否則使用物件的字串表示。
        /// </summary>
        /// <param name="data">要處理的輸入值</param>
        public static string NullToEmpty(object? data)
        {
            return (data == null) ? "" : data.ToString()!;
        }

        /// <summary>
        /// get object size ??
        /// </summary>
        /// <param name="TestObject"></param>
        /// <returns></returns>
        /*
        public static int GetObjectSize(object obj)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            //byte[] Array;
            bf.Serialize(ms, obj);
            var array = ms.ToArray();
            return array.Length;
        }
        */

        /// <summary>以屬性名稱透過反射讀值，屬性不存在時可能擲出例外。</summary>
        /// <param name="data">要透過反射讀取屬性的實例。</param>
        /// <param name="fid">要讀取的公開屬性名稱，須存在於實例型別。</param>
        /// <returns></returns>
        public static object GetValueByFid(this object data, string fid)
        {
            return data.GetType().GetProperty(fid)!.GetValue(data, null)!;
        }

        /// <summary>
        /// 依屬性名稱寫值，不自動轉型或處理不存在的屬性。
        /// </summary>
        /// <param name="data">要處理的輸入值</param>
        /// <param name="fid">要寫入的公開屬性名稱，須存在於實例型別。</param>
        /// <param name="value">要使用的輸入值</param>
        public static void SetValueByName(this object data, string fid, object value)
        {
            data.GetType().GetProperty(fid)!.SetValue(data, value, null);
        }

        /// <summary>
        /// 組合屬性型別名稱；Nullable 型別另加上其底層型別名稱。
        /// </summary>
        /// <param name="inputObject">要檢查公開屬性型別的物件</param>
        public static string GetTypeFullName(this PropertyInfo inputObject)
        {
            var propertyType = inputObject.PropertyType;
            var extraName = "";

            //??
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                extraName = propertyType.GetGenericArguments()[0].Name;

            return string.Format("{0} {1}", extraName, propertyType.Name);
        }

        #region remark code
        /*
        /// <summary>
        /// 判斷是否為 集合類型(list..IEnumerable)
        /// </summary>
        public static bool isIEnumerableOrCollection(this PropertyInfo inputObject)
        {
            //string也是集合型別 但不是我想要的結果
            if (inputObject.PropertyType == typeof(string) ||
                inputObject.PropertyType == typeof(String))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(inputObject.PropertyType);
        }


        ///// <summary>
        ///// 判斷是否為集合型別 (若值為null會無法辨識)
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static bool isIEnumerableOrCollection<T>(this T source) where T :struct
        //{
        //    if (source == null)
        //    {
        //        Debug.Print("您嘗試判斷是否為集合型別。但是null值無法辨識");
        //        Console.WriteLine("您嘗試判斷是否為集合型別。但是null值無法辨識");
        //        return false;
        //    }

        //    //string 也屬於 IEnumerable 但不是我想要的結果
        //    if (source.GetType() == typeof(string) ||
        //        source.GetType() == typeof(String))
        //        return false;

        //    var intttt = source.GetType().GetInterfaces();

        //    if (source.GetType().GetInterfaces().Any(x =>
        //          x.IsGenericType &&
        //          x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        //    {
        //        return true;
        //    }

        //    return false;
        //}
        */
        #endregion
    }
}
