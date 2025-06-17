using Base.Models;

namespace Base.Interfaces
{
    public interface IPdfSvc
    {

        void SetKey(string keyPath);

        /// <summary>
        /// convert word(docx) to pdf
        /// </summary>
        /// <param name="wordBytes"></param>
        /// <returns></returns>
        byte[] WordToPdf(byte[] wordBytes, string keyPath = "");

        /// <summary>
        /// pdf 增加多個圖檔
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath">新pdf檔案</param>
        /// <param name="imageDtos"></param>
        /// <returns></returns>
        bool AddImages(string fromPath, string toPath, PdfImageDto[] imageDtos);
    }
}
