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
    static readonly InputWrong = '输入错误。 ';

    //for crud form
    static readonly Create = '新增';
    static readonly Update = '修改';
    static readonly View = '检视';
    static readonly UpdateOk = '资料更新完成。 ';
    static readonly DeleteOk = '资料删除完成。 ';
    static readonly SaveOk = '资料储存完成。 ';
    static readonly SaveNone = '无任何资料异动。 ';
    static readonly Done = '作业完成。 ';

    //find form
    static readonly FindOk = '查询完成。 ';
    static readonly FindNone = '找不到资料。 ';

    //form tip
    static readonly TipUpdate = '修改这笔资料';
    static readonly TipDelete = '删除这笔资料';
    static readonly TipView = '检视这笔资料';
    static readonly TipCopy = '复制这笔资料并且进入新增模式';

    //message-upload file
    static readonly UploadFileNotBig = '上传档案不可大于{0}M !';
    static readonly UploadFileNotMatch = '上传档案种类不符合 !';
    static readonly NewFileNotView = '新档案尚未上传, 无法检视。 ';

    //message-others
    static readonly PlsSelect = '-请选取-';
    static readonly PlsSelectDeleted = '请选取要删除的资料。 ';
    static readonly PlsSelectRows = '请先选取资料。 ';
    static readonly SureDeleteRow = '是否确定删除这笔资料?';
    static readonly SureDeleteSelected = '是否确定删除选取的资料?';

    //authority
    static readonly NoAuthUser = '您只能存取个人资料，请联络管理者。 ';
    static readonly NoAuthDept = '您只能存取同部门资料，请联络管理者。 ';
    static readonly NoAuthProg = '您没有此功能的权限，请联络管理者。 ';
    static readonly NoFile = '档案不存在。 ';
    static readonly NotLogin = '您尚未登入系统。 ';

    //others
    static readonly Working = '作业处理中...';
    static readonly TimeOut = '待机时间过久，或未登入系统。 ';
    static readonly Page = '每页显示 _Menu @@笔, 第 _Start 至 _End 笔, 总共 _Total 笔';
    static readonly UniqueError = '此笔资料已经存在，不可重复。 ';
}