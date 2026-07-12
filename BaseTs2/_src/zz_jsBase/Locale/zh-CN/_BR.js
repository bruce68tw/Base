"use strict";
var _BR = /** @class */ (function () {
    function _BR() {
    }
    //=== moment.js convert these to UI format ===
    _BR.MmUiDateFmt = 'YYYY/MM/DD'; //match bootstrap-datepicker.js format property
    _BR.MmUiDtFmt = 'YYYY/MM/DD HH:mm:ss'; //datetime
    _BR.MmUiDt2Fmt = 'YYYY/MM/DD HH:mm'; //datetime no second
    //row status
    _BR.StatusYes = '正常';
    _BR.StatusNo = '停用';
    _BR.Yes = '是';
    //check input
    _BR.InputWrong = '输入错误。 ';
    //for crud form
    _BR.Create = '新增';
    _BR.Update = '修改';
    _BR.View = '检视';
    _BR.UpdateOk = '资料更新完成。 ';
    _BR.DeleteOk = '资料删除完成。 ';
    _BR.SaveOk = '资料储存完成。 ';
    _BR.SaveNone = '无任何资料异动。 ';
    _BR.Done = '作业完成。 ';
    //find form
    _BR.FindOk = '查询完成。 ';
    _BR.FindNone = '找不到资料。 ';
    //form tip
    _BR.TipUpdate = '修改这笔资料';
    _BR.TipDelete = '删除这笔资料';
    _BR.TipView = '检视这笔资料';
    _BR.TipCopy = '复制这笔资料并且进入新增模式';
    //message-upload file
    _BR.UploadFileNotBig = '上传档案不可大于{0}M !';
    _BR.UploadFileNotMatch = '上传档案种类不符合 !';
    _BR.NewFileNotView = '新档案尚未上传, 无法检视。 ';
    //message-others
    _BR.PlsSelect = '-请选取-';
    _BR.PlsSelectDeleted = '请选取要删除的资料。 ';
    _BR.PlsSelectRows = '请先选取资料。 ';
    _BR.SureDeleteRow = '是否确定删除这笔资料?';
    _BR.SureDeleteSelected = '是否确定删除选取的资料?';
    //authority
    _BR.NoAuthUser = '您只能存取个人资料，请联络管理者。 ';
    _BR.NoAuthDept = '您只能存取同部门资料，请联络管理者。 ';
    _BR.NoAuthProg = '您没有此功能的权限，请联络管理者。 ';
    _BR.NoFile = '档案不存在。 ';
    _BR.NotLogin = '您尚未登入系统。 ';
    //others
    _BR.Working = '作业处理中...';
    _BR.TimeOut = '待机时间过久，或未登入系统。 ';
    _BR.Page = '每页显示 _Menu @@笔, 第 _Start 至 _End 笔, 总共 _Total 笔';
    _BR.UniqueError = '此笔资料已经存在，不可重复。 ';
    return _BR;
}());
