namespace Base.Enums
{
    /// <summary>
    /// 上傳檔案的副檔名分類，供 _File.TypeToExts 產生清單；不驗證檔案內容。
    /// </summary>
    public enum UpFileTypeEnum
    {
        /// <summary>
        /// 圖片副檔名 jpg、jpeg、png、gif；列舉預設值。
        /// </summary>
        Image,
        /// <summary>
        /// 使用呼叫端提供的自訂副檔名清單。
        /// </summary>
        Custom,
        /// <summary>
        /// 所有副檔名，回傳萬用字元 *。
        /// </summary>
        All
    }
}
