using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Base.Services
{
    public class _Num
    {
        /// <summary>
        /// get RWD col number
        /// </summary>
        /// <param name="itemCount">columns count, 1-12</param>
        /// <returns></returns>
        public static int NumToRwdCol(int itemCount)
        {
            switch (itemCount)
            {
                case 1:
                    return 12;
                case 2:
                    return 6;
                case 3:
                    return 4;
                case 4:
                    return 3;
                case 6:
                    return 2;
                case 12:
                    return 1;
                default:
                    return 12;
            }
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
            if (_Str.IsEmpty(text))
                return result;

            var list = text.Split(sep);
            //int num;
            foreach (var item in list)
            {
                if (int.TryParse(item, out var num))
                    result.Add(num);
            }
            return result;
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
