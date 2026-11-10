using Base.Models;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Base.Services
{
    /// <summary>
    /// 欄位檢查與驗證結果工具；現有字元檢查多為開頭比對，不等同完整內容驗證。
    /// </summary>
    public class _Valid
    {
        /// <summary>
        /// 將欄位訊息轉成單項錯誤清單，預設編輯區為 0。
        /// </summary>
        /// <param name="fid">要處理的欄位識別值</param>
        /// <param name="msg">要附加到指定欄位的驗證訊息。</param>
        /// <param name="editNo">發生驗證錯誤的編輯區編號</param>
        public static List<ErrorRowDto> MsgToErrors(string fid, string msg, int editNo = 0)
        {
            return
                [
                    new ErrorRowDto()
                    {
                        Fid = fid,
                        Msg = msg,
                        EditNo = editNo,
                    }
                ];
        }

        /// <summary>
        /// 以現有正規表示式檢查電子郵件格式，不驗證信箱存在。
        /// </summary>
        /// <param name="data">要檢查的電子郵件地址。</param>
        /// <return>true/false</return>
        public static bool IsEmail(string data)
        {
            return Regex.IsMatch(data,
                @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
        }

        /// <summary>
        /// 僅檢查字串開頭是否為數字或句點，非完整數值解析。
        /// </summary>
        /// <param name="data">要處理的輸入值</param>
        public static bool IsNumber(object data)
        {
            return Regex.IsMatch(data.ToString()!, @"^[0-9.]");
        }

        /// <summary>
        /// 僅檢查開頭是否為數字，非全字串純數字檢查。
        /// </summary>
        /// <param name="data">要處理的輸入值</param>
        public static bool IsDigits(object data)
        {
            return Regex.IsMatch(data.ToString()!, @"^[0-9]");
        }

        /// <summary>
        /// 檢查空值或開頭是否符合文字字元集合，不檢查全字串。
        /// </summary>
        /// <param name="data">要處理的輸入值</param>
        public static bool IsText(object data)
        {
            return _Object.IsEmpty(data) 
                ? true : Regex.IsMatch(data.ToString()!, @"^[\w-,./]");
        }

        /// <summary>
        /// 檢查空值或開頭是否符合指定字元集合，不清理或驗證 HTML 安全性。
        /// </summary>
        /// <param name="data">要處理的輸入值</param>
        public static bool IsHtml(object data)
        {
            return _Object.IsEmpty(data) 
                ? true : Regex.IsMatch(data.ToString()!, @"^[\w-,&;]");
        }

        /// <summary>
        /// 以 IsText 檢查 JSON 各欄位的字串表示。
        /// </summary>
        /// <param name="row">要逐欄檢查字串表示的 JSON 資料列。</param>
        public static bool IsJsonText(JObject row)
        {
            //if (row == null) return true;
            foreach (var item in row)
                if (!IsText(row[item.Key]!.ToString())) return false;            
            return true;    //case of ok
        }

        /// <summary>
        /// 以 IsText 檢查各欄位；目前不填入傳入的 errorModel。
        /// </summary>
        /// <param name="errorModel">保留的錯誤資料參數，現行方法不會填入</param>
        /// <param name="row">要讀取、檢查或寫入的單列 JSON</param>
        public static bool CheckFormInput(ResultDto errorModel, JObject row)
        {
            var result = true;
            foreach (var item in row)
            {
                var key = item.Key;
                if (!IsText(row[key]!.ToString()))
                {
                    result = false;
                    //TODO: temp remark
                    //AddError(errorModel, key, "input wrong.");
                }
            }
            return result;
        }

        /// <summary>
        /// 僅在 _ErrorMsg 為空時回傳 true，不檢查 ErrorRows。
        /// </summary>
        /// <param name="result">要判斷操作是否成功的結果物件</param>
        public static bool ResultStatus(ResultDto result)
        {
            return _Str.IsEmpty(result._ErrorMsg);
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
                    result[_Fun.FidErrorMsg] = "資料輸入錯誤。";
                    return result;
                }
            }

            //case of ok
            return null;
        }
        */

        // <summary>
        // 傳回內空白值的欄位list, fields放在最後面寫code比較方便
        // 無法用 reflection方式傳回變數名稱, 改用比較簡單的做法, 傳入欄位名稱 list
        // </summary>
        // <param name="result"></param>
        // <param name="fields"></param>
        // <returns></returns>
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

        // <summary>
        // 把 List<string>的錯誤欄位清單寫入 ErrorModel變數
        // </summary>
        // <param name="errorModel"></param>
        // <param name="rowNo"></param>
        // <param name="fields"></param>
        // <param name="errorMsg"></param>
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

        // <summary>
        // 增加一個錯誤欄位
        // </summary>
        // <param name="rows"></param>
        // <param name="field"></param>
        // <param name="errorMsg"></param>
        //public static void AddError(ErrorModel errorModel, string field, string errorMsg)
        //{
        //    if (errorModel.ErrorFields == null)
        //        errorModel.ErrorFields = new List<IdStrDto>();
        //    errorModel.ErrorFields.Add(new IdStrDto() { Id = field, Str = errorMsg });
        //}

        #endregion

    } //class
}
