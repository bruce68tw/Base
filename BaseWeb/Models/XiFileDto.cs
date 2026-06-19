using Base.Enums;

namespace BaseWeb.Models
{
    /// <summary>
    /// XiFile 上傳檔案欄位的傳入參數類型
    /// </summary>
    public class XiFileDto : XiBaseDto
    {
        /// <summary>
        /// 同 XiLinkDto
        /// </summary>
        public string Table { get; set; } = "";

        /// <summary>
        /// 上傳檔案大小的上限，單位為 MB
        /// </summary>
        public int MaxSize { get; set; } = 10;

        /// <summary>
        /// 上傳檔案類型限制，預設為圖檔
        /// </summary>
        public UpFileTypeEnum FileType { get; set; } = UpFileTypeEnum.Image;

        /// <summary>
        /// FileType=UpFileTypeEnum.Custom時, 必須設定此欄位, 以逗號分隔多個副檔名, 不含點號(dot), 例如: "jpg,png,gif"
        /// </summary>
        public string FileTypeExts { get; set; } = "";

    }
}