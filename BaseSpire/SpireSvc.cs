using Base.Interfaces;
using Base.Models;
using Base.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Spire.Doc;
using Spire.Doc.License;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;

namespace BaseSpire
{
    public class SpireSvc : IPdfSvc
    {
        //private string _key = "";

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

    }
}
