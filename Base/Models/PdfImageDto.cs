namespace Base.Models
{
    //pdf image
    public class PdfImageDto
    {
        public string FilePath = "";

        public double PosX;

        public double PosY;

        //等比例縮放, 只考慮 width
        public double Width;

        //public double Height;

    }
}