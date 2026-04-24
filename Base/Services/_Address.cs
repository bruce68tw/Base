using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Base.Services
{
    //以後用到再測試, 先放這裡備用   
    //(AI生成)處理中文字地址中的數字雙向轉換功能，特別針對巷、弄、段等地址元素進行處理。
    public static class _Address
    {
        // 中文數字
        private static readonly Dictionary<char, int> TwMap = new()
        {
            ['零'] = 0,
            ['一'] = 1,
            ['二'] = 2,
            ['三'] = 3,
            ['四'] = 4,
            ['五'] = 5,
            ['六'] = 6,
            ['七'] = 7,
            ['八'] = 8,
            ['九'] = 9,
            ['十'] = 10
        };

        static readonly string TwDigits = "零一二三四五六七八九";

        // =========================
        // 對外入口（雙向）
        // =========================
        public static string Normalize(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return address;

            // 中文數字 → 阿拉伯數字（巷/弄/段）
            address = TwToNum(address);

            // 阿拉伯數字 → 中文數字（巷/弄/段）
            address = NumToTw(address);

            return address;
        }

        // =========================
        // 中文 → 阿拉伯
        // =========================
        private static string TwToNum(string input)
        {
            return Regex.Replace(input,
                @"([一二三四五六七八九十]+)(?=(段|巷|弄))",
                m => TwCharToNum(m.Groups[1].Value).ToString());
        }

        //單一中文轉數字
        private static int TwCharToNum(string cn)
        {
            int result = 0;
            int temp = 0;

            foreach (var c in cn)
            {
                int val = TwMap[c];

                if (val == 10)
                {
                    temp = (temp == 0 ? 1 : temp) * 10;
                    result += temp;
                    temp = 0;
                }
                else
                {
                    temp = val;
                }
            }

            result += temp;
            return result;
        }

        // =========================
        // 阿拉伯 → 中文
        // =========================
        private static string NumToTw(string input)
        {
            return Regex.Replace(input,
                @"(?<=[\u4e00-\u9fff])(\d+)(?=(段|巷|弄))",
                m => NumCharToTw(m.Value));
        }

        //單一數字轉中文
        private static string NumCharToTw(string number)
        {
            return string.Concat(number.Select(c => TwDigits[c - '0']));
        }
    }
}