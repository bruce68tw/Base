using System;
using System.Collections.Generic;

namespace Base.Services
{
    /// <summary>
    /// 數值解析、CRUD 暫存鍵與版面欄數轉換工具。
    /// </summary>
    public class _Num
    {
        /// <summary>
        /// 空鍵回傳 -1，非數字或正數鍵回傳 0；零或負數轉為非負的暫存列號。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>-1(error/not found), 0(正常key值), n(new key)</returns>
        public static int KeyToUpRowNo(string key)
        {
            return string.IsNullOrEmpty(key) ? -1 :     //case error
                !Int32.TryParse(key, out int num) ? 0 : //表示key為非數字，視為正常key值
                 num > 0 ? 0 : num * (-1);              //>0 表示key為數字, 否則視為new key
        }

        /// <summary>
        /// 依每列項目數換算 12 欄格線寬度；未列出的項目數回傳 12。
        /// </summary>
        /// <param name="itemCount">columns count, 1-12</param>
        /// <returns></returns>
        public static int NumToRwdCol(int itemCount)
        {
            return itemCount switch
            {
                1 => 12,
                2 => 6,
                3 => 4,
                4 => 3,
                6 => 2,
                12 => 1,
                _ => 12,
            };
        }

        /// <summary>
        /// 依分隔符轉整數清單；空字串回傳空清單，無法解析的項目略過。
        /// </summary>
        public static List<int> StrToList(string text, char sep)
        {
            var result = new List<int>();
            if (_Str.IsEmpty(text)) return result;

            var list = text.Split(sep);
            foreach (var item in list)
                if (int.TryParse(item, out var num)) result.Add(num);
            return result;
        }

        /// <summary>
        /// 嘗試解析整數；null 或失敗時回傳 value。
        /// </summary>
        public static int TryParse(object? obj, int value)
        {
            return (obj == null) ? value : 
                int.TryParse(obj.ToString(), out var num) ? num : value;
        }

        /// <summary>
        /// 回傳 0 起、未達 maxNum 的一般 Random 整數，非密碼學安全亂數。
        /// </summary>
        public static int GetRandom(int maxNum)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            return random.Next(0, maxNum);
            /*
            var rng = new RNGCryptoServiceProvider();
            var rndBytes = new byte[4];
            rng.GetBytes(rndBytes);
            return BitConverter.ToInt32(rndBytes, 0);
            */
        }
    }
}
