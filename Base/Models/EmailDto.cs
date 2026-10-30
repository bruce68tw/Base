using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 寄送郵件的內容、收件者與附加檔案。
    /// </summary>
    public class EmailDto
    {
        /// <summary>
        /// 郵件主旨。
        /// </summary>
        public string Subject { get; set; } = "";
        /// <summary>
        /// 郵件本文。
        /// </summary>
        public string Body { get; set; } = "";
        /// <summary>
        /// 主要收件者信箱清單，預設未初始化。
        /// </summary>
        public List<string> ToUsers { get; set; } = null!;
        /// <summary>
        /// 副本收件者清單，可為 null。
        /// </summary>
        public List<string>? CcUsers { get; set; }
        /// <summary>
        /// 附件的實體檔案路徑清單，可為 null。
        /// </summary>
        public List<string>? Files { get; set; }
        /// <summary>
        /// 內嵌圖片清單：Id 為圖片識別，Str 為實體路徑；可為 null。
        /// </summary>
        public List<IdStrDto>? Images { get; set; }

        //image id list
        //public List<string> ImageIds { get; set; }

        //image path list
        //public List<string> ImagePaths { get; set; }
    }
}
