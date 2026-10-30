namespace Base.Models
{
    /// <summary>
    /// Base 共用畫面的語系文字資源；以屬性供反射複製，文字預設為空字串。
    /// </summary>
    public class BaseResDto
    {
        /// <summary>
        /// 新增按鈕文字。
        /// </summary>
        public string BtnCreate { get; set; } = "";
        /// <summary>
        /// 新增資料列按鈕文字。
        /// </summary>
        public string BtnAddRow { get; set; } = "";
        /// <summary>
        /// 查詢按鈕文字。
        /// </summary>
        public string BtnFind { get; set; } = "";
        /// <summary>
        /// 第二種查詢按鈕文字。
        /// </summary>
        public string BtnFind2 { get; set; } = "";
        /// <summary>
        /// 草稿按鈕文字。
        /// </summary>
        public string BtnDraft { get; set; } = "";
        /// <summary>
        /// 儲存按鈕文字。
        /// </summary>
        public string BtnSave { get; set; } = "";
        /// <summary>
        /// 儲存並送出按鈕文字。
        /// </summary>
        public string BtnSaveSend { get; set; } = "";
        /// <summary>
        /// 返回列表按鈕文字。
        /// </summary>
        public string BtnToRead { get; set; } = "";
        /// <summary>
        /// 匯出按鈕文字。
        /// </summary>
        public string BtnExport { get; set; } = "";
        /// <summary>
        /// 開啟按鈕文字。
        /// </summary>
        public string BtnOpen { get; set; } = "";
        /// <summary>
        /// 重設按鈕文字。
        /// </summary>
        public string BtnReset { get; set; } = "";
        /// <summary>
        /// 確認按鈕文字。
        /// </summary>
        public string BtnYes { get; set; } = "";
        /// <summary>
        /// 取消按鈕文字。
        /// </summary>
        public string BtnCancel { get; set; } = "";
        /// <summary>
        /// 關閉按鈕文字。
        /// </summary>
        public string BtnClose { get; set; } = "";
        /// <summary>
        /// 開始錄音按鈕文字。
        /// </summary>
        public string BtnStartRec { get; set; } = "";
        /// <summary>
        /// 停止錄音按鈕文字。
        /// </summary>
        public string BtnStopRec { get; set; } = "";
        /// <summary>
        /// 修改提示文字。
        /// </summary>
        public string TipUpdate { get; set; } = "";
        /// <summary>
        /// 刪除提示文字。
        /// </summary>
        public string TipDelete { get; set; } = "";
        /// <summary>
        /// 檢視提示文字。
        /// </summary>
        public string TipView { get; set; } = "";
        /// <summary>
        /// 刪除資料列提示文字。
        /// </summary>
        public string TipDeleteRow { get; set; } = "";
        /// <summary>
        /// 請選擇提示文字。
        /// </summary>
        public string PlsSelect { get; set; } = "";
        /// <summary>
        /// 無權限提示文字。
        /// </summary>
        public string NoAuth { get; set; } = "";
        /// <summary>
        /// 處理中提示文字。
        /// </summary>
        public string Working { get; set; } = "";
        /// <summary>
        /// 語音視窗標題文字。
        /// </summary>
        public string TitleVoice { get; set; } = "";
    }
}
