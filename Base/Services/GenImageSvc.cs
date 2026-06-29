using SkiaSharp;
using System.IO;

namespace Base.Services
{
    //generate image
    public class GenImageSvc
    {
        private string _toFile;
        private SKBitmap _bitmap;
        private SKCanvas _canvas;
        private int _width;
        private string _fontName;
        //int Height;

        public GenImageSvc(string toFile, int width, int height, string fontName)
        {
            _toFile = toFile;
            _width = width;
            _fontName = fontName;
            //Height = height;
            _bitmap = new SKBitmap(width, height);
            _canvas = new SKCanvas(_bitmap);
        }

        public void AddText(int posX, int posY, string text, int fontSize, string? fontName = null, 
            SKFontStyle? style = null, SKColor? color = null)
        {
            using var paint = new SKPaint
            {
                Color = color ?? SKColors.Black,
                IsAntialias = true
            };

            using var typeface = SKTypeface.FromFamilyName(fontName ?? _fontName, style ?? SKFontStyle.Normal);
            using var font = new SKFont(typeface, fontSize);

            // 設定文字水平對齊為中央
            if (posX == -1)
            {
                posX = _width / 2;
                float textWidth = font.MeasureText(text);
                posX -= (int)(textWidth / 2);
            }

            _canvas.DrawText(text, posX, posY, font, paint);
        }

        public void AddTextBlob(float posX, float posY, int width, string text, int fontSize, 
            string? fontName = null, SKFontStyle? style = null, SKColor? color = null)
        {
            using var paint = new SKPaint
            {
                Color = color ?? SKColors.Black,
                IsAntialias = true
            };

            using var typeface = SKTypeface.FromFamilyName(fontName ?? _fontName, style ?? SKFontStyle.Normal);
            using var font = new SKFont(typeface, fontSize);
            float x = posX;
            float y = posY;
            foreach (char c in text)
            {
                string character = c.ToString();

                // 改由 SKFont 計算文字寬度
                float characterWidth = font.MeasureText(character);
                if (x + characterWidth > posX + width)
                {
                    x = posX;                    
                    y += font.Spacing;  // 行高
                }

                _canvas.DrawText(character, x, y, font, paint);
                x += characterWidth;
            }
        }

        public void AddImage(int posX, int posY, string filePath, int width, 
            int height, SKColor? borderColor = null)
        {
            using var bitmap = SKBitmap.Decode(filePath);
            if (bitmap == null) return;

            //不使用 Resize（避免 CS1061 / CS0618 問題）
            var destRect = new SKRect(posX, posY, posX + width, posY + height);

            //直接縮放繪製（最穩定 + 最快）
            _canvas.DrawBitmap(bitmap, destRect);

            // 外框（可選）
            if (borderColor is not null)
            {
                using var borderPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = borderColor.Value,
                    StrokeWidth = 1,
                    IsAntialias = true
                };

                const int gap = 1;
                var borderRect = new SKRect(
                    posX - gap,
                    posY - gap,
                    posX + width + gap,
                    posY + height + gap
                );

                _canvas.DrawRect(borderRect, borderPaint);
            }
        }

        public void Save()
        {
            //string outputPath = "path_to_save_image.png";
            using var image = SKImage.FromBitmap(_bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new FileStream(_toFile, FileMode.Create);
            data.SaveTo(stream);
        }

    }//class
}
