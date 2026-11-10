using SkiaSharp;
using System.IO;

namespace Base.Services
{
    /// <summary>
    /// 使用 SkiaSharp 在記憶體繪圖並輸出 PNG；尺寸以像素表示。
    /// </summary>
    public class GenImageSvc
    {
        private string _toFile;
        private SKBitmap _bitmap;
        private SKCanvas _canvas;
        private int _width;
        private string _fontName;

        /// <summary>
        /// 建立指定尺寸的畫布，保存輸出路徑與預設字型。
        /// </summary>
        /// <param name="toFile">產生圖片的輸出檔案路徑</param>
        /// <param name="width">輸出或繪製區域的寬度，單位為像素</param>
        /// <param name="height">輸出或繪製區域的高度，單位為像素</param>
        /// <param name="fontName">繪製文字使用的字型名稱</param>
        public GenImageSvc(string toFile, int width, int height, string fontName)
        {
            _toFile = toFile;
            _width = width;
            _fontName = fontName;
            //Height = height;
            _bitmap = new SKBitmap(width, height);
            _canvas = new SKCanvas(_bitmap);
        }

        /// <summary>
        /// 在指定基線位置繪字；posX 為 -1 時水平置中。
        /// </summary>
        /// <param name="posX">文字基線的水平位置，為 -1 時水平置中。</param>
        /// <param name="posY">文字基線的垂直位置。</param>
        /// <param name="text">要處理的文字</param>
        /// <param name="fontSize">繪製文字使用的字型大小</param>
        /// <param name="fontName">繪製文字使用的字型名稱</param>
        /// <param name="style">繪製文字時使用的字型樣式</param>
        /// <param name="color">繪製文字使用的顏色</param>
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

        /// <summary>
        /// 逐字繪製文字，超過指定寬度時換行，非完整文字排版。
        /// </summary>
        /// <param name="posX">繪製位置的水平座標</param>
        /// <param name="posY">繪製位置的垂直座標</param>
        /// <param name="width">輸出或繪製區域的寬度，單位為像素</param>
        /// <param name="text">要處理的文字</param>
        /// <param name="fontSize">繪製文字使用的字型大小</param>
        /// <param name="fontName">繪製文字使用的字型名稱</param>
        /// <param name="style">繪製文字時使用的字型樣式</param>
        /// <param name="color">繪製文字使用的顏色</param>
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

        /// <summary>
        /// 讀取圖片並縮放到指定矩形，可加外框；解碼失敗時略過。
        /// </summary>
        /// <param name="posX">繪製位置的水平座標</param>
        /// <param name="posY">繪製位置的垂直座標</param>
        /// <param name="filePath">要讀取或寫入的完整檔案路徑</param>
        /// <param name="width">輸出或繪製區域的寬度，單位為像素</param>
        /// <param name="height">輸出或繪製區域的高度，單位為像素</param>
        /// <param name="borderColor">圖片外框顏色</param>
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

        /// <summary>
        /// 以 PNG 覆寫輸出檔案，不建立父目錄。
        /// </summary>
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
