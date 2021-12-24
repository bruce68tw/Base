using Base.Models;
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
        /// convert model to form string, seperate with "," for show model content only, can not decode !!
        /// </summary>
        /// <returns></returns>
        public static string ToFormStr<T>(T model)
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

        /// <summary>
        /// model to json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
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

        public static TTo Copy<TFrom, TTo>(TFrom from) where TTo : new()
        {
            if (from == null)
                return default;

            var result = new TTo();
            foreach (var prop in from.GetType().GetProperties())
            {
                var fid = prop.Name;
                if (HasProp<TTo>(result, fid))
                    SetValue<TTo>(result, fid, prop.GetValue(from, null));
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
        public static T HashToModel<T>(Hashtable table) where T : new()
        {
            T model = new();
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

        /// <summary>
        /// convert list model to json string(for javascript)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static string ListToJsonStr<T>(List<T> rows)
        {
            return JsonConvert.SerializeObject(rows);
        }

        /// <summary>
        /// get error model 
        /// </summary>
        /// <param name="error">default to system error msg</param>
        /// <returns></returns>
        public static ResultDto GetError(string error = "")
        {
            return new ResultDto() { ErrorMsg = _Str.EmptyToValue(error, _Fun.SystemError) };
        }

        /// <summary>
        /// get js _BR error
        /// </summary>
        /// <param name="fid">_BR fid</param>
        /// <returns></returns>
        public static ResultDto GetBrError(string fid)
        {
            return new ResultDto() { ErrorMsg = _Fun.PreBrError + fid };
        }

    } //class
}