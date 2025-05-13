using Base.Interfaces;
using Spire.Doc;

namespace BaseSpire
{
    public class SpireSvc : IPdfSvc
    {

        public byte[] WordToPdf(byte[] wordBytes)
        {
            using var ms = new MemoryStream(wordBytes);
            // 使用 Spire.Doc 加載 Word 檔案
            var docx = new Document(ms);

            using var pdfStream = new MemoryStream();
            // 將 Word 轉換為 PDF
            docx.SaveToStream(pdfStream, FileFormat.PDF);
            return pdfStream.ToArray();
        }

    }
}
