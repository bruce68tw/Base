using Base.Services;
using System.Text.RegularExpressions;

namespace Base
{
    /// <summary>
    /// 依 _Fun.PwdStrongLevel 檢查密碼強度。
    /// </summary>
    public class _Pwd
    {
        /// <summary>
        /// 符合規則回傳空字串，否則回傳中文原因；0 不限制，1 要求英數，其他層級要求大小寫、數字、符號及至少 10 字元。
        /// </summary>
        /// <param name="pwd">要驗證的密碼</param>
        public static string CheckStrong(string pwd)
        {
            switch (_Fun.PwdStrongLevel)
            {
                //無限制
                case 0: return "";

                //英數字
                case 1:
                    return Regex.IsMatch(pwd, @"^(?=.*[a-zA-Z])(?=.*\d).+$")
                        ? "" : "密碼必須包含英數字";

                //大小寫英文,數字,特殊符號,長度10以上
                default:
                    var status = (pwd.Length < 10) ? false :
                        !Regex.IsMatch(pwd, "[a-z]") ? false :
                        !Regex.IsMatch(pwd, "[A-Z]") ? false :
                        !Regex.IsMatch(pwd, "[0-9]") ? false :
                        Regex.IsMatch(pwd, "[!-/:-@\\[-`{-~]");
                    return status
                        ? "" : "密碼必須包含大小寫英文、數字、特殊符號、長度10以上。";
            }

        }
    }
}
