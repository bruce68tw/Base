namespace Base.Models
{
    public class ErrorRowDto
    {
        //constructor
        public ErrorRowDto()
        {
            Fid = "";
            Msg = "";
            EditNo = 0;
            RowId = "";
        }

        public string Fid { get; set; }
        public string Msg { get; set; }

        /// <summary>
        /// FormId(string) -> EditNo(int)
        /// EditOne/EditMany, 不是 FormId 要修正
        /// default _me.edit0
        /// </summary>
        public int EditNo { get; set; }

        /// <summary>
        /// 用這個欄位來判別錯誤欄位是在那一個資料群組, for update多筆資料時
        /// </summary>
        public string RowId { get; set; }

        /// <summary>
        /// 輸入錯誤的欄位id清單, type為Key(id), Value(錯誤訊息)
        /// </summary>
        //public List<IdStrDto> ErrorFields { get; set; } = null!;

    }
}