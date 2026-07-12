import _Edit from './_Edit';
import _Obj from './_Obj';
import _Form from './_Form';
import _iText from './_iText';
import _Str from './_Str';
import _Tool from './_Tool';
import _Input from './_Input';
import _Json from './_Json';
import _iFile from './_iFile';
import _Fun from './_Fun';
export default class EditOne {
    [_Edit.Childs];
    kid;
    eform;
    is1to1;
    dataJson;
    systemError;
    validator;
    fidTypes;
    fidRadios;
    hasFile;
    fileLen;
    fileFids;
    fnValid;
    constructor(kid, eformId, childs) {
        this[_Edit.Childs] = childs;
        this.kid = kid || 'Id';
        eformId = eformId || 'eform';
        this.eform = $('#' + eformId);
        this.is1to1 = false;
        this.dataJson = null;
        this.systemError = '';
        var error = (this.eform.length != 1) ? 'EditOne.js input eformId is wrong. (' + eformId + ')' :
            (_Obj.get(this.kid, this.eform) == null) ? 'EditOne.js input kid is wrong. (' + this.kid + ')' :
                '';
        if (error != '') {
            this.systemError = error;
            alert(error);
        }
        _Edit.initVars(this, this.eform);
    }
    showErrors(json) {
        this.validator.showErrors(json);
    }
    setIs1to1() {
        this.is1to1 = true;
    }
    _resetAndNew(init) {
        _Form.reset(this.eform, init);
        _iText.set(this.kid, -1, this.eform);
    }
    valid() {
        if (_Str.notEmpty(this.systemError)) {
            _Tool.msg(this.systemError);
            return false;
        }
        if (!this.eform.valid())
            return false;
        return (this.fnValid) ? this.fnValid() : true;
    }
    getKey() {
        return _Input.get(this.kid, this.eform);
    }
    getValue(fid) {
        return _Input.get(fid, this.eform);
    }
    isNewRow() {
        return _Edit.isNewBox(this.eform, this.kid);
    }
    loadRow(row) {
        if (this.is1to1 && _Json.isEmpty(row))
            this._resetAndNew();
        else
            _Edit.loadRow(this, this.eform, row);
    }
    getUpdRow(upKey) {
        var row = _Edit.getUpdRow(this, this.eform);
        if (this.is1to1 && row != null) {
            row[_Edit.DataFkeyFid] = upKey;
            return row;
        }
        else {
            return row;
        }
    }
    reset(init) {
        if (this.is1to1)
            this._resetAndNew(init);
        else
            _Form.reset(this.eform, init);
    }
    resetKey() {
        _Input.set(this.kid, '', this.eform);
    }
    setEdit(status) {
        _Form.setEdit(this.eform, status);
    }
    dataAddFiles(levelStr, data) {
        if (!this.hasFile)
            return null;
        var fileJson = {};
        for (var i = 0; i < this.fileLen; i++) {
            var fid = this.fileFids[i];
            var serverFid = _Edit.getFileSid(levelStr, fid);
            if (_iFile.dataAddFile(data, fid, serverFid, this.eform)) {
                fileJson[serverFid] = this.getKey();
            }
        }
        return fileJson;
    }
    async onViewFile(table, fid) {
        var elm = _Fun.getMeElm();
        var key = this.getKey();
        await _Edit.viewFileA(table, fid, elm, key);
    }
}
