using Base.Services;
using System.Text.RegularExpressions;

namespace Base
{
    public class _Pwd
    {
        /// <summary>
        /// 檢查密碼強度
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="db"></param>
        /// <returns>error msg if any(無多國語)</returns>
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
