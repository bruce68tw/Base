using System;
using System.Drawing;
//nuGet System.Drawing.Common
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;
using System.IO;

namespace Base.Models
{
    /// <summary>
    /// TODO 驗證碼類別
    /// </summary>
    public class ValidateCodeDto
    {
        /// <summary>
        /// 驗證碼的最大長度
        /// </summary>
        public int MaxLength
        {
            get { return 10; }
        }

        /// <summary>
        /// 驗證碼的最小長度
        /// </summary>
        public int MinLength
        {
            get { return 1; }
        }

        /// <summary>
        /// 產生驗證碼
        /// </summary>
        /// <param name="length">指定驗證碼的長度</param>
        /// <returns></returns>
        public string CreateValidateCode(int length)
        {
            int[] randMembers = new int[length];
            int[] validateNums = new int[length];
            string validateNumberStr = "";
            //產生起始序列值
            int seekSeek = unchecked((int)DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            int beginSeek = (int)seekRand.Next(0, Int32.MaxValue - length * 10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }
            //產生隨機數字
            for (int i = 0; i < length; i++)
            {
                Random rand = new Random(seeks[i]);
                int pownum = 1 * (int)Math.Pow(10, length);
                randMembers[i] = rand.Next(pownum, Int32.MaxValue);
            }
            //抽取隨機數字
            for (int i = 0; i < length; i++)
            {
                string numStr = randMembers[i].ToString();
                int numLength = numStr.Length;
                Random rand = new Random();
                int numPosition = rand.Next(0, numLength - 1);
                validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
            }
            //產生驗證碼
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i].ToString();
            }
            return validateNumberStr;
        }

        /// <summary>
        /// 建立驗證的圖片
        /// </summary>
        /// <param name="validateCode">驗證碼</param>
        /*
        public byte[] CreateValidateGraphic(string validateCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * 15.0), 28);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //產生随機產生器
                Random random = new Random();
                //清空圖片背景色
                g.Clear(Color.White);
                //畫圖片的干擾線
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 16, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                 Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 3, 2);
                //畫圖片的前景干擾點
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //畫圖片的邊框線
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //儲存圖片
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                //輸出圖片流
                return stream.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
        */

        /// <summary>
        /// 得到驗證碼圖片的長度
        /// </summary>
        /// <param name="validateNumLength">驗證碼的長度</param>
        /// <returns></returns>
        public static int GetImageWidth(int validateNumLength)
        {
            return (int)(validateNumLength * 12.0);
        }

        /// <summary>
        /// 得到驗證碼的高度
        /// </summary>
        /// <returns></returns>
        public static double GetImageHeight()
        {
            return 22.5;
        }
    }
}
