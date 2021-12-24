using System;
using System.Reflection;

namespace Base.Services
{
    public static class _Object
    {

        public static bool IsEmpty(object data)
        {
            return (data == null || data.ToString() == "");
        }

        public static bool NotEmpty(object data)
        {
            return !IsEmpty(data);
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

        /// <summary>
        /// get property value by  name
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name">property name</param>
        /// <returns></returns>
        public static object GetValueByName(this object obj, string name)
        {
            return obj.GetType().GetProperty(name).GetValue(obj, null);
        }

        /// <summary>
        /// set propery 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name">property name</param>
        /// <returns></returns>
        public static void SetValueByName(this object obj, string name, object value)
        {
            obj.GetType().GetProperty(name).SetValue(obj, value, null);
        }

        /// <summary>
        /// get Type name
        /// </summary>
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
