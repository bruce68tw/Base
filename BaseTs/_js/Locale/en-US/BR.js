//resource for js base component
export default class BR {
    //=== moment.js ymd format ===
    static MmUiDateFmt = 'MMM-D-YYYY'; //match datepicker format
    static MmUiDtFmt = 'MMM-D-YYYY HH:mm:ss';
    static MmUiDt2Fmt = 'MMM-D-YYYY HH:mm'; //no second
    //row status
    static StatusYes = 'Active';
    static StatusNo = 'Off';
    static Yes = 'Yes';
    //check input
    static InputWrong = 'Input Wrong.';
    //for crud form
    static Create = 'Create';
    static Update = 'Update';
    static View = 'View';
    static UpdateOk = 'Update Ok.';
    static DeleteOk = 'Delete Ok.';
    static SaveOk = 'Save Ok.';
    static SaveNone = 'No row changed !';
    static Done = 'Done.';
    //find form
    static FindOk = 'Find Ok.';
    static FindNone = 'Find None !';
    //form tip
    static TipUpdate = 'Update this Row.';
    static TipDelete = 'Delete this Row.';
    static TipView = 'View this Row.';
    //message-upload file
    static UploadFileNotBig = 'Upload File Size Should Less Than {0}M !';
    static UploadFileNotMatch = 'Upload File Type Not Match !';
    static NewFileNotView = 'Save First Then View !';
    //message-others
    static PlsSelectDeleted = 'Please Select Deleted Rows.';
    static PlsSelectRows = 'Please Select Rows First.';
    static SureDeleteRow = 'Sure to Delete Row ?';
    static SureDeleteSelected = 'Sure to Delete Selected ?';
    //authority
    static NoAuthUser = 'No right for this user, Please connect Admin.';
    static NoAuthDept = 'No right for this department, Please connect Admin.';
    static NoAuthProg = 'You have not access right, Please connect Admin.';
    static NotLogin = 'Please Login First.';
    //others
    static Working = 'Working...';
    static TimeOut = 'Standby too long, or not Login.';
}
