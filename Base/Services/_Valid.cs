using Base.Models;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Base.Services
{
    /// <summary>
    /// backend validation
    /// </summary>
    public class _Valid
    {
        /// <summary>
        /// check email
        /// </summary>
        /// <param name="data">input email string</param>
        /// <return>true/false</return>
        public static bool IsEmail(string data)
        {
            return Regex.IsMatch(data,
                @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
        }

        //check float
        public static bool IsNumber(object data)
        {
            return Regex.IsMatch(data.ToString(), @"^[0-9.]");
        }

        //check integer
        public static bool IsDigits(object data)
        {
            return Regex.IsMatch(data.ToString(), @"^[0-9]");
        }

		//check text, allow "," for string list
        public static bool IsText(object data)
        {
            return _Object.IsEmpty(data) 
                ? true 
                : Regex.IsMatch(data.ToString(), @"^[\w-,./]");
        }

        public static bool IsHtml(object data)
        {
            return _Object.IsEmpty(data) 
                ? true 
                : Regex.IsMatch(data.ToString(), @"^[\w-,&;]");
        }

        /// <summary>
        /// check json for normal text
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static bool IsJsonText(JObject row)
        {
            if (row == null)
                return true;

            foreach (var item in row)
            {
                var key = item.Key;
                if (!IsText(row[key].ToString()))
                    return false;
            }
            //case of ok
            return true;
        }

        /// <summary>
        /// check text for form input
        /// </summary>
        /// <param name="errorModel"></param>
        /// <param name="row"></param>
        public static bool CheckFormInput(ResultDto errorModel, JObject row)
        {
            var result = true;
            foreach (var item in row)
            {
                var key = item.Key;
                if (!IsText(row[key].ToString()))
                {
                    result = false;
                    //TODO: temp remark
                    //AddError(errorModel, key, "input wrong.");
                }
            }
            return result;
        }

        public static bool ResultStatus(ResultDto result)
        {
            return _Str.IsEmpty(result.ErrorMsg);
        }

        #region remark code
        /// <summary>
        /// 檢查model資料的欄位資料是否正常,
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>error msg if aay</returns>
        /*
        public static JObject CheckModel<T>(T model)
        {
            foreach (var prop in model.GetType().GetProperties())
            {
                if (prop is IList)
                {
                    for (var i = 0; i < ((IList)prop).Count; i++)
                    {
                        var error2 = CheckModel(((IList)prop)[i]);
                        if (error2 != null)
                            return error2;
                    }
                }
                else if (prop.GetType().IsGenericType)
                    return CheckModel(prop);    //recrusive
                else if (!IsText(prop.GetValue(model, null)))
                {
                    var result = new JObject();
                    result[_Fun.?ErrorMsg] = "資料輸入錯誤。";
                    return result;
                }
            }

            //case of ok
            return null;
        }
        */

        ///// <summary>
        ///// 傳回內空白值的欄位list, fields放在最後面寫code比較方便
        ///// 無法用 reflection方式傳回變數名稱, 改用比較簡單的做法, 傳入欄位名稱 list
        ///// </summary>
        ///// <param name="result"></param>
        ///// <param name="fields"></param>
        ///// <returns></returns>
        //public static List<ErrorFieldModel> GetEmptyFields(string errorMsg, object model, List<string> fields)
        //{
        //    var result = new List<ErrorFieldModel>();
        //    foreach (var field in fields)
        //    {
        //        object value = model.GetType().GetProperty(field).GetValue(model, null);
        //        if (_Str.IsEmpty(value))
        //            result.Add(new IdStrDto() { Id = field, Str = errorMsg });

        //    }
        //    return result.Count > 0 ? result : null; 
        //}

        ///// <summary>
        ///// 把 List<string>的錯誤欄位清單寫入 ErrorModel變數
        ///// </summary>
        ///// <param name="errorModel"></param>
        ///// <param name="rowNo"></param>
        ///// <param name="fields"></param>
        ///// <param name="errorMsg"></param>
        ///*
        //public static void FillErrorFields(ErrorModel errorModel, int rowNo, List<string> fields, string errorMsg)
        //{
        //    if (fields == null || fields.Count == 0)
        //        return;

        //    var errorRow = new ErrorRowModel();
        //    errorRow.RowNo = rowNo;
        //    errorRow.ErrorFields = new List<KeyValueModel>();

        //    for (var i=0; i<fields.Count; i++)
        //    {
        //        errorRow.ErrorFields.Add(new KeyValueModel()
        //        {
        //            Key = fields[i],
        //            Value = errorMsg,
        //        });
        //    }

        //    //initial if need
        //    if (errorModel.ErrorFields == null)
        //        errorModel.ErrorFields = new List<KeyValueModel>();
        //    errorModel.ErrorRows.Add(errorRow);
        //}
        //*/

        ///// <summary>
        ///// 增加一個錯誤欄位
        ///// </summary>
        ///// <param name="rows"></param>
        ///// <param name="field"></param>
        ///// <param name="errorMsg"></param>
        //public static void AddError(ErrorModel errorModel, string field, string errorMsg)
        //{
        //    if (errorModel.ErrorFields == null)
        //        errorModel.ErrorFields = new List<IdStrDto>();
        //    errorModel.ErrorFields.Add(new IdStrDto() { Id = field, Str = errorMsg });
        //}

        #endregion

    } //class
}
