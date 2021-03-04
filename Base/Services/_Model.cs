using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Base.Services
{
    public class _Model
    {
        /// <summary>
        /// convert model to string, seperate with "," for show model content only, can not decode !!
        /// </summary>
        /// <returns></returns>
        public static string ToStr<T>(T model)
        {
            if (model == null)
                return "";

            var result = "";
            foreach (var prop in model.GetType().GetProperties())
                result += prop.Name + "=" + prop.GetValue(model, null) + ", ";

            return result;
        }

        /// <summary>
        /// convert model variables to json string
        /// </summary>
        /// <returns></returns>
        public static string ToJsonStr<T>(T model)
        {
            if (model == null)
                return "";

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

        public static JObject ToJson<T>(T model)
        {
            if (model == null)
                return null;

            var result = new JObject();
            foreach (var prop in model.GetType().GetProperties())
            {
                var value = prop.GetValue(model, null);
                result[prop.Name] = (value == null) ? "" : value.ToString();
            }
            return result;
        }

        /// <summary>
        /// decode string into model 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T JsonStrToModel<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }

        /// <summary>
        /// get property value of model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object GetValue<T>(T model, string name)
        {
            //if model hasn't this property, log error
            var prop = model.GetType().GetProperty(name);
            return (prop == null)
                ? null
                : prop.GetValue(model, null);
        }

        /// <summary>
        /// model set property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value">property new value</param>
        /// <returns></returns>
        public static bool SetValue<T>(T model, string name, object value)
        {
            //if model hasn't this property, log error
            var prop = model.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return false;

            prop.SetValue(model, value, null);
            return true;
        }

        /// <summary>
        /// check model if has property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop">property name</param>
        /// <returns></returns>
        public static bool HasProp<T>(T model, string prop)
        {
            return (model.GetType().GetProperty(prop) != null);
        }

        /// <summary>
        /// convert hashtable to model, must add where T : new() !!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static T HashtableToModel<T>(Hashtable table) where T : new()
        {
            T model = new T();
            foreach (var prop in model.GetType().GetProperties())
            {
                if (!table.Contains(prop.Name))
                    continue;

                if (prop.PropertyType == typeof(int))
                    prop.SetValue(model, Convert.ToInt32(table[prop.Name]), null);
                else
                    prop.SetValue(model, table[prop.Name].ToString(), null);

            }

            return model;
        }
        
        //convert list model to json string(for javascript)
        public static string ListToStr<T>(List<T> rows)
        {
            return JsonConvert.SerializeObject(rows);
        }

    } //class
}