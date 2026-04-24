namespace Base.Enums
{
    /// <summary>
    /// upload file type enum
    /// </summary>
    public enum UpFileTypeEnum
    {
        Image,  //image only
        Custom, //自訂類型, 必須設定 UpFileTypeExts欄位
        All     //全部類型
    }
}
