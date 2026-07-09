export default class BR {
    //=== moment.js convert these to UI format ===
    static MmUiDateFmt = 'YYYY/MM/DD'; //match bootstrap-datepicker.js format property
    static MmUiDtFmt = 'YYYY/MM/DD HH:mm:ss'; //datetime 
    static MmUiDt2Fmt = 'YYYY/MM/DD HH:mm'; //datetime no second
    //row status
    static StatusYes = '正常';
    static StatusNo = '停用';
    static Yes = '是';
    //check input
    static InputWrong = '輸入錯誤。';
    //for crud form
    static Create = '新增';
    static Update = '修改';
    static View = '檢視';
    static UpdateOk = '資料更新完成。';
    static DeleteOk = '資料刪除完成。';
    static SaveOk = '資料儲存完成。';
    static SaveNone = '無任何資料異動。';
    static Done = '作業完成。';
    //find form
    static FindOk = '查詢完成。';
    static FindNone = '找不到資料。';
    //form tip
    static TipUpdate = '修改這筆資料';
    static TipDelete = '刪除這筆資料';
    static TipView = '檢視這筆資料';
    static TipCopy = '複製這筆資料並且進入新增模式';
    //message-upload file
    static UploadFileNotBig = '上傳檔案不可大於{0}M !';
    static UploadFileNotMatch = '上傳檔案種類不符合 !';
    static NewFileNotView = '新檔案尚未上傳, 無法檢視。';
    //message-others
    static PlsSelect = '-請選取-';
    static PlsSelectDeleted = '請選取要刪除的資料。';
    static PlsSelectRows = '請先選取資料。';
    static SureDeleteRow = '是否確定刪除這筆資料?';
    static SureDeleteSelected = '是否確定刪除選取的資料?';
    //authority
    static NoAuthUser = '您只能存取個人資料，請聯絡管理者。';
    static NoAuthDept = '您只能存取同部門資料，請聯絡管理者。';
    static NoAuthProg = '您沒有此功能的權限，請聯絡管理者。';
    static NoFile = '檔案不存在。';
    static NotLogin = '您尚未登入系統。';
    //others
    static Working = '作業處理中...';
    static TimeOut = '待機時間過久，或未登入系統。';
    static Page = '每頁顯示 _Menu @@筆, 第 _Start 至 _End 筆, 總共 _Total 筆';
    static UniqueError = '此筆資料已經存在，不可重複。';
}
