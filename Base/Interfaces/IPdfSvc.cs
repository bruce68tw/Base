namespace Base.Interfaces
{
    public interface IPdfSvc
    {
        /// <summary>
        /// convert word(docx) to pdf
        /// </summary>
        /// <param name="wordBytes"></param>
        /// <returns></returns>
        byte[] WordToPdf(byte[] wordBytes);
    }
}
