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
    private [_Edit.Childs]: any;

    private kid: string;
    private eform: JQuery;
    private is1to1: boolean;
    private dataJson: any;
    private systemError: string;

    private validator: any;
    private fidTypes: any;
    private fidRadios: any;
    private hasFile: any;
    private fileLen: any;
    private fileFids: any;
    private fnValid: any;

    constructor(kid?: string, eformId?: string, childs?: any) {
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

    private showErrors(json: any): void {
        this.validator.showErrors(json);
    }

    private setIs1to1(): void {
        this.is1to1 = true;
    }

    private _resetAndNew(init?: boolean): void {
        _Form.reset(this.eform, init);
        _iText.set(this.kid, -1, this.eform);
    }

    private valid(): boolean {
        if (_Str.notEmpty(this.systemError)) {
            _Tool.msg(this.systemError);
            return false;
        }

        if (!this.eform.valid()) return false;

        return (this.fnValid) ? this.fnValid() : true;
    }

    private getKey(): string {
        return _Input.get(this.kid, this.eform);
    }

    private getValue(fid: string): string {
        return _Input.get(fid, this.eform);
    }

    private isNewRow(): boolean {
        return _Edit.isNewBox(this.eform, this.kid);
    }

    private loadRow(row: any): void {
        if (this.is1to1 && _Json.isEmpty(row))
            this._resetAndNew();
        else
            _Edit.loadRow(this, this.eform, row);
    }

    private getUpdRow(upKey: string): any {
        var row = _Edit.getUpdRow(this, this.eform);
        if (this.is1to1 && row != null) {
            row[_Edit.DataFkeyFid] = upKey;
            return row;
        } else {
            return row;
        }
    }

    private reset(init?: boolean): void {
        if (this.is1to1)
            this._resetAndNew(init);
        else
            _Form.reset(this.eform, init);
    }

    private resetKey(): void {
        _Input.set(this.kid, '', this.eform);
    }

    private setEdit(status: boolean): void {
        _Form.setEdit(this.eform, status);
    }

    private dataAddFiles(levelStr: string, data: FormData): any {
        if (!this.hasFile) return null;

        var fileJson: any = {};
        for (var i = 0; i < this.fileLen; i++) {
            var fid = this.fileFids[i];
            var serverFid = _Edit.getFileSid(levelStr, fid);
            if (_iFile.dataAddFile(data, fid, serverFid, this.eform)) {
                fileJson[serverFid] = this.getKey();
            }
        }
        return fileJson;
    }

    private async onViewFile(table: string, fid: string): Promise<void> {
        var elm = _Fun.getMeElm();
        var key = this.getKey();
        await _Edit.viewFileA(table, fid, elm, key);
    }
}