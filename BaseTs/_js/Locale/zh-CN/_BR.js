export default class _BR {
    //=== moment.js convert these to UI format ===
    static MmUiDateFmt = 'YYYY/MM/DD'; //match bootstrap-datepicker.js format property
    static MmUiDtFmt = 'YYYY/MM/DD HH:mm:ss'; //datetime
    static MmUiDt2Fmt = 'YYYY/MM/DD HH:mm'; //datetime no second
    //row status
    static StatusYes = '正常';
    static StatusNo = '停用';
    static Yes = '是';
    //check input
    static InputWrong = '输入错误。 ';
    //for crud form
    static Create = '新增';
    static Update = '修改';
    static View = '检视';
    static UpdateOk = '资料更新完成。 ';
    static DeleteOk = '资料删除完成。 ';
    static SaveOk = '资料储存完成。 ';
    static SaveNone = '无任何资料异动。 ';
    static Done = '作业完成。 ';
    //find form
    static FindOk = '查询完成。 ';
    static FindNone = '找不到资料。 ';
    //form tip
    static TipUpdate = '修改这笔资料';
    static TipDelete = '删除这笔资料';
    static TipView = '检视这笔资料';
    static TipCopy = '复制这笔资料并且进入新增模式';
    //message-upload file
    static UploadFileNotBig = '上传档案不可大于{0}M !';
    static UploadFileNotMatch = '上传档案种类不符合 !';
    static NewFileNotView = '新档案尚未上传, 无法检视。 ';
    //message-others
    static PlsSelect = '-请选取-';
    static PlsSelectDeleted = '请选取要删除的资料。 ';
    static PlsSelectRows = '请先选取资料。 ';
    static SureDeleteRow = '是否确定删除这笔资料?';
    static SureDeleteSelected = '是否确定删除选取的资料?';
    //authority
    static NoAuthUser = '您只能存取个人资料，请联络管理者。 ';
    static NoAuthDept = '您只能存取同部门资料，请联络管理者。 ';
    static NoAuthProg = '您没有此功能的权限，请联络管理者。 ';
    static NoFile = '档案不存在。 ';
    static NotLogin = '您尚未登入系统。 ';
    //others
    static Working = '作业处理中...';
    static TimeOut = '待机时间过久，或未登入系统。 ';
    static Page = '每页显示 _Menu @@笔, 第 _Start 至 _End 笔, 总共 _Total 笔';
    static UniqueError = '此笔资料已经存在，不可重复。 ';
}
