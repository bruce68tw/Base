export default class _BR {
    //=== moment.js ymd format ===
    static readonly MmUiDateFmt = 'MMM-D-YYYY';          //match datepicker format
    static readonly MmUiDtFmt = 'MMM-D-YYYY HH:mm:ss'; 
    static readonly MmUiDt2Fmt = 'MMM-D-YYYY HH:mm';     //no second

    //row status
    static readonly StatusYes = 'Active';
    static readonly StatusNo = 'Off';
    static readonly Yes = 'Yes';

    //check input
    static readonly InputWrong = 'Input Wrong.';

    //for crud form
    static readonly Create = 'Create';
    static readonly Update = 'Update';
    static readonly View = 'View';
    static readonly UpdateOk = 'Update Ok.';
    static readonly DeleteOk = 'Delete Ok.';
    static readonly SaveOk = 'Save Ok.';
    static readonly SaveNone = 'No row changed !';
    static readonly Done = 'Done.';

    //find form
    static readonly FindOk = 'Find Ok.';
    static readonly FindNone = 'Find None !';

    //form tip
    static readonly TipUpdate = 'Update this Row.';
    static readonly TipDelete = 'Delete this Row.';
    static readonly TipView = 'View this Row.';
    static readonly TipCopy = 'Copy this data and enter new mode';
    
    //message-upload file
    static readonly UploadFileNotBig = 'Upload File Size Should Less Than {0}M !';
    static readonly UploadFileNotMatch = 'Upload File Type Not Match !';
    static readonly NewFileNotView = 'Save First Then View !';

    //message-others
    static readonly PlsSelect = '-Select-';
    static readonly PlsSelectDeleted = 'Please Select Deleted Rows.';
    static readonly PlsSelectRows = 'Please Select Rows First.';
    static readonly SureDeleteRow = 'Sure to Delete Row ?';
    static readonly SureDeleteSelected = 'Sure to Delete Selected ?';

    //authority
    static readonly NoAuthUser = 'No right for this user; Please connect Admin.';
    static readonly NoAuthDept = 'No right for this department; Please connect Admin.';
    static readonly NoAuthProg = 'You have not access right; Please connect Admin.';
    static readonly NoFile = 'No File Existed.';
    static readonly NotLogin = 'Please Login First.';

    //others
    static readonly Working = 'Working...';
    static readonly TimeOut = 'Standby too long; or not Login.';
    static readonly UniqueError = 'Record Exists no Repeated.'
}