using Base.Interfaces;
using Base.Models;
using Base.Services;
using Spire.Doc;
using Spire.Doc.License;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;

namespace PdfSpire
{
    public class SpireSvc : IPdfSvc
    {
        //private string _key = "";

        //在主機才會發生作用(本機不會)!!
        public void SetKey(string keyPath)
        {
            if (string.IsNullOrEmpty(keyPath))
                return;

            var key = _Xml.GetKeyProp(keyPath, "/License", "Key");
            if (_Str.NotEmpty(key))
                LicenseProvider.SetLicenseKey(key);
        }

        public byte[] WordToPdf(byte[] wordBytes, string keyPath = "")
        {
            SetKey(keyPath);

            using var ms = new MemoryStream(wordBytes);
            // 使用 Spire.Doc 加載 Word 檔案
            var pdf = new Document(ms);

            using var pdfStream = new MemoryStream();
            // 將 Word 轉換為 PDF
            pdf.SaveToStream(pdfStream, Spire.Doc.FileFormat.PDF);
            return pdfStream.ToArray();
        }

        /// <summary>
        /// (無跨平台)pdf file add images
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <param name="imageDtos"></param>
        /// <returns></returns>
        /*
        public bool AddImages(string fromPath, string toPath, PdfImageDto[] imageDtos)
        {
            // 建立新的 PDF 文件，從現有檔案載入
            var pdf = new PdfDocument(fromPath);
            // 找到要插入圖片的頁面，這裡假設是第 imageDto.PageIndex 頁（從 0 開始）
            var page = pdf.Pages[0];
            var rotate = (page.Rotation == PdfPageRotateAngle.RotateAngle90);

            // 讀取圖片
            foreach (var imageDto in imageDtos)
            {

                // 載入圖片, Spire 無法跨平台, 必須使用 System.Drawing.Image !!
                using var image = Image.FromFile(imageDto.FilePath);
                if (rotate)
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone); // 轉正方向

                float imageWidth = image.Width;
                float imageHeight = image.Height;

                // 轉換成 PdfImage
                var pdfImage = PdfImage.FromImage(image);

                // 計算等比例縮放後的高度
                float newWidth = (float)imageDto.Width;
                float scale = newWidth / imageWidth;
                float newHeight = imageHeight * scale;

                // 插入圖片（位置與新尺寸）
                var rect = new RectangleF(
                    x: (float)imageDto.PosX,
                    y: (float)imageDto.PosY,
                    width: newWidth,
                    height: newHeight
                );

                page.Canvas.DrawImage(pdfImage, rect);
            }

            // 儲存 PDF
            pdf.SaveToFile(toPath, Spire.Pdf.FileFormat.PDF);
            pdf.Close();

            return true;
        }
        */

        /// <summary>
        /// (跨平台)pdf file add images
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <param name="imageDtos"></param>
        /// <returns></returns>
        public bool AddImages(string fromPath, string toPath, PdfImageDto[] imageDtos)
        {
            using var pdf = new PdfDocument(fromPath);
            foreach (var imageDto in imageDtos)
            {
                var page = pdf.Pages[0];
                var pdfImage = PdfImage.FromFile(imageDto.FilePath);

                float imageWidth = pdfImage.Width;
                float imageHeight = pdfImage.Height;
                float newWidth = (float)imageDto.Width;
                float scale = newWidth / imageWidth;
                float newHeight = imageHeight * scale;

                // 原始座標
                var rect = new RectangleF(
                    (float)imageDto.PosX,
                    (float)imageDto.PosY,
                    newWidth,
                    newHeight
                );

                // PDF 橫印旋轉修正
                rect = ConvertRotation(page, rect);

                // 蓋章
                page.Canvas.DrawImage(pdfImage, rect);
            }

            pdf.SaveToFile(toPath, Spire.Pdf.FileFormat.PDF);
            return true;
        }

        /// <summary>
        /// 依照 PDF Page 的 Rotation 修正圖片位置座標。
        /// 
        /// PDF 的 Rotation 只影響閱讀顯示方向，並不會改變 PDF 內部 Canvas 座標系統。
        /// 當 PDF 頁面為橫向列印 (90/270 度旋轉) 時，
        /// 需要將圖片蓋章位置轉換到正確的 Canvas 座標，
        /// 避免圖片出現在錯誤的位置。
        /// 
        /// 注意：此函式只調整位置與尺寸座標，不旋轉圖片內容。
        /// </summary>
        private RectangleF ConvertRotation(PdfPageBase page, RectangleF rect)
        {
            return page.Rotation switch
            {
                PdfPageRotateAngle.RotateAngle90 => new RectangleF(
                    page.Size.Height - rect.Y - rect.Height,
                    rect.X,
                    rect.Height,
                    rect.Width
                ),
                PdfPageRotateAngle.RotateAngle270 => new RectangleF(
                    rect.Y,
                    page.Size.Width - rect.X - rect.Width,
                    rect.Height,
                    rect.Width
                ),
                _ => rect,
            };
        }

    }//class
}
