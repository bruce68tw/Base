export default class _BR {
    //=== moment.js convert these to UI format ===
    static readonly MmUiDateFmt = 'YYYY/MM/DD';          //match bootstrap-datepicker.js format property
    static readonly MmUiDtFmt = 'YYYY/MM/DD HH:mm:ss';   //datetime 
    static readonly MmUiDt2Fmt = 'YYYY/MM/DD HH:mm';     //datetime no second

    //row status
    static readonly StatusYes = '正常';
    static readonly StatusNo = '停用';
    static readonly Yes = '是';

    //check input
    static readonly InputWrong = '輸入錯誤。';

    //for crud form
    static readonly Create = '新增';
    static readonly Update = '修改';
    static readonly View = '檢視';
    static readonly UpdateOk = '資料更新完成。';
    static readonly DeleteOk = '資料刪除完成。';
    static readonly SaveOk = '資料儲存完成。';
    static readonly SaveNone = '無任何資料異動。';
    static readonly Done = '作業完成。';

    //find form
    static readonly FindOk = '查詢完成。';
    static readonly FindNone = '找不到資料。';

    //form tip
    static readonly TipUpdate = '修改這筆資料';
    static readonly TipDelete = '刪除這筆資料';
    static readonly TipView = '檢視這筆資料';
    static readonly TipCopy = '複製這筆資料並且進入新增模式';

    //message-upload file
    static readonly UploadFileNotBig = '上傳檔案不可大於{0}M !';
    static readonly UploadFileNotMatch = '上傳檔案種類不符合 !';
    static readonly NewFileNotView = '新檔案尚未上傳, 無法檢視。';

    //message-others
    static readonly PlsSelect = '-請選取-';
    static readonly PlsSelectDeleted = '請選取要刪除的資料。';
    static readonly PlsSelectRows = '請先選取資料。';
    static readonly SureDeleteRow = '是否確定刪除這筆資料?';
    static readonly SureDeleteSelected = '是否確定刪除選取的資料?';

    //authority
    static readonly NoAuthUser = '您只能存取個人資料，請聯絡管理者。';
    static readonly NoAuthDept = '您只能存取同部門資料，請聯絡管理者。';
    static readonly NoAuthProg = '您沒有此功能的權限，請聯絡管理者。';
    static readonly NoFile = '檔案不存在。';
    static readonly NotLogin = '您尚未登入系統。';

    //others
    static readonly Working = '作業處理中...';
    static readonly TimeOut = '待機時間過久，或未登入系統。';
    static readonly Page = '每頁顯示 _Menu @@筆, 第 _Start 至 _End 筆, 總共 _Total 筆';
    static readonly UniqueError = '此筆資料已經存在，不可重複。';
}