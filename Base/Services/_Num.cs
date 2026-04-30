using System;
using System.Collections.Generic;

namespace Base.Services
{
    public class _Num
    {
        /// <summary>
        /// for crud, 從資料key欄位讀取對應上層的rowNo, parse file key
        /// called by: CrudEditSvc.cs、_HttpFile.cs
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
        /// get RWD col number
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
        /// convert string into List<int>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static List<int> StrToList(string text, char sep)
        {
            var result = new List<int>();
            if (_Str.IsEmpty(text)) return result;

            var list = text.Split(sep);
            foreach (var item in list)
                if (int.TryParse(item, out var num)) result.Add(num);
            return result;
        }

        public static int TryParse(object? obj, int value)
        {
            return (obj == null) ? value : 
                int.TryParse(obj.ToString(), out var num) ? num : value;
        }

        /// <summary>
        /// get random number
        /// consider Vulnerability "Use of Cryptographically Weak PRNG"
        /// @param maxNum max number
        /// </summary>
        /// <returns></returns>
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
