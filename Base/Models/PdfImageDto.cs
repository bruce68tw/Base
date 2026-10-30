namespace Base.Models
{
    /// <summary>
    /// PDF 疊加圖片的位置與尺寸；現有 Spire 實作只加入第一頁。
    /// </summary>
    public class PdfImageDto
    {
        /// <summary>
        /// 圖片的實體檔案路徑。
        /// </summary>
        public string FilePath = "";
        /// <summary>
        /// 水平位置，使用 PDF 繪圖座標單位。
        /// </summary>
        public double PosX;
        /// <summary>
        /// 垂直位置，使用 PDF 繪圖座標單位。
        /// </summary>
        public double PosY;
        /// <summary>
        /// 繪製寬度，使用 PDF 繪圖單位；高度依原圖比例計算。
        /// </summary>
        public double Width;

        //public double Height;

    }
}
