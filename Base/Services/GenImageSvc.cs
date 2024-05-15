using SkiaSharp;
using System.IO;

namespace Base.Services
{
    //generate image
    public class GenImageSvc
    {
        //private const decimal INCH_TO_CM = 2.54M;
        //private const decimal CM_TO_EMU = 360000M;

        /*
        public string FileName = string.Empty;
        public Stream? DataStream;

        public long WidthInEMU;
        public long HeightInEMU;
        public string ImageName = "";
        */

        private string ToFile;
        private SKBitmap Bitmap;
        private SKCanvas Canvas;
        private int Width;
        private string FontName;
        //int Height;

        public GenImageSvc(string toFile, int width, int height, string fontName)
        {
            ToFile = toFile;
            Width = width;
            FontName = fontName;
            //Height = height;
            Bitmap = new SKBitmap(width, height);
            Canvas = new SKCanvas(Bitmap);
        }

        public void AddText(int posX, int posY, string text, int fontSize, string? fontName = null, SKFontStyle? style = null, SKColor? color = null)
        {
            using var paint = new SKPaint();
            paint.TextSize = fontSize;
            paint.Color = color ?? SKColors.Black;

            // 設定文字水平對齊為中央
            if (posX == -1)
            {
                posX = Width / 2;
                paint.TextAlign = SKTextAlign.Center;
            }

            // 設定字體
            var typeface = SKTypeface.FromFamilyName(fontName ?? FontName, style ?? SKFontStyle.Normal);
            paint.Typeface = typeface;

            Canvas.DrawText(text, posX, posY, paint);
        }
        public void AddTextBlob(float posX, float posY, int width, string text, int fontSize, string? fontName = null, SKFontStyle? style = null, SKColor? color = null)
        {
            using var paint = new SKPaint();
            paint.TextSize = fontSize;
            paint.Color = color ?? SKColors.Black;

            /*
            // 設定文字水平對齊為中央
            if (x == -1)
            {
                x = Width / 2;
                paint.TextAlign = SKTextAlign.Center;
            }
            */

            // 設定字體
            var typeface = SKTypeface.FromFamilyName(fontName ?? FontName, style ?? SKFontStyle.Normal);
            paint.Typeface = typeface;


            // 設定文字區域的寬度
            //float textWidth = 300;

            // 計算每行文字的位置
            float x = posX;
            float y = posY;
            int lineCount = 0;

            // 逐字元繪製文字，並處理換行
            foreach (char c in text)
            {
                string character = c.ToString();
                float characterWidth = paint.MeasureText(character);

                if (x + characterWidth > width)
                {
                    // 超過文字區域的寬度，換行
                    x = posX;
                    y += paint.FontSpacing;
                    lineCount++;
                }

                Canvas.DrawText(character, x, y, paint);
                x += characterWidth;
            }

            /*
            // 設定字體
            var typeface = SKTypeface.FromFamilyName(fontName ?? FontName, style ?? SKFontStyle.Normal);
            paint.Typeface = typeface;

            Canvas.DrawText(text, posX, posY, paint);
            */
        }

        public void AddImage(int posX, int posY, string filePath, int width, int height, SKColor? borderColor = null)
        {
            using var image = SKBitmap.Decode(filePath);
            if (image != null)
            {
                //縮放圖檔
                var resizeImage = image.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
                Canvas.DrawBitmap(resizeImage, new SKPoint(posX, posY));

                //繪製外框
                if (borderColor != null)
                {
                    using SKPaint borderPaint = new SKPaint();
                    borderPaint.Style = SKPaintStyle.Stroke;
                    borderPaint.Color = borderColor.Value;
                    borderPaint.StrokeWidth = 1;

                    const int gap = 1;
                    SKRect borderRect = new SKRect(posX - gap, posY - gap, posX + width + gap, posY + height + gap);
                    Canvas.DrawRect(borderRect, borderPaint);
                }
                //resizeImage.Dispose();
            }
        }

        public void Save()
        {
            //string outputPath = "path_to_save_image.png";
            using var image = SKImage.FromBitmap(Bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new FileStream(ToFile, FileMode.Create);
            data.SaveTo(stream);
        }

    }//class
}
