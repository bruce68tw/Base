namespace Base.Models
{
    /// <summary>
    /// 將驗證錯誤定位至編輯區、資料列與欄位。
    /// </summary>
    public class ErrorRowDto
    {
        /// <summary>
        /// 建立空白錯誤資料，編輯區索引預設為 0。
        /// </summary>
        public ErrorRowDto()
        {
            Fid = "";
            Msg = "";
            EditNo = 0;
            RowId = "";
        }
        /// <summary>
        /// 發生錯誤的欄位識別名稱。
        /// </summary>
        public string Fid { get; set; }
        /// <summary>
        /// 欄位錯誤訊息。
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 編輯區索引，預設 0 對應主編輯區。
        /// </summary>
        public int EditNo { get; set; }
        /// <summary>
        /// 發生錯誤的資料列識別，用於多列編輯。
        /// </summary>
        public string RowId { get; set; }

        //public List<IdStrDto> ErrorFields { get; set; } = null!;

    }
}
