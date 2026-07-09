import moment from "moment";
import _Ajax from './_Ajax';
import _Leftmenu from './_Leftmenu';
import _Pjax from './_Pjax';
import _Tool from './_Tool';
import _Input from './_Input';
import _Obj from './_Obj';
export default class _Fun {
    // #region constant (big camel) ===
    static MmDateFmt = 'YYYY/MM/DD';
    static MmDtFmt = 'YYYY/MM/DD HH:mm:ss';
    static FidErrorMsg = '_ErrorMsg';
    static PreBrError = 'B:';
    static CssFlag = 'x-flag';
    static HideRwd = 'x-hide-rwd';
    // #endregion
    // variables
    static locale = 'zh-TW';
    static maxFileSize = 50971520;
    static isRwd = false;
    static pageRows = 10;
    static userId = '';
    static nowDom = '';
    static lengthMenu = [10, 20, 50, 100];
    // mid variables
    static data = {};
    // datatables column define default values
    static dtColDef = {
        className: 'x-center',
        targets: '_all',
        type: 'string',
        orderable: false,
        orderSequence: ['asc', 'desc'],
    };
    /**
     * initial
     * param {string} locale
     * param {string} pjaxArea Filter
     */
    static init(locale) {
        _Fun.locale = locale;
        _Leftmenu.init();
        _Pjax.init('.x-main-right');
        _Tool.init();
        moment.locale(_Fun.locale);
        var body = $('body');
        _Fun.setEvent(body, 'click');
        _Fun.setEvent(body, 'change');
        $.ajaxSetup({
            headers: {
                'RequestVerificationToken': $('meta[name="csrf-token"]').attr('content')
            }
        });
    }
    static async onHelloA() {
        await _Ajax.getStrA('../Fun/Hello', null, function (msg) {
            alert(msg);
        });
    }
    static getMe() {
        return $(_Fun.nowDom);
    }
    static getMeElm() {
        return _Fun.nowDom;
    }
    static getMeValue() {
        return _Input.getO($(_Fun.nowDom));
    }
    /**
     * 註冊事件, 避免使用inline script for CSRF
     * param {object} box 容器
     * param {string} eventName name(不含on)
     */
    static setEvent(box, eventName) {
        var event2 = 'on' + eventName;
        box.on(eventName, `[data-${event2}]`, function () {
            _Fun.nowDom = this;
            var me = $(this);
            const fnPath = me.data(event2);
            var argsStr = me.data("args");
            argsStr = (argsStr == null) ? "" : argsStr.toString();
            const args = argsStr ? argsStr.split(",") : [];
            const parts = fnPath.split(".");
            let obj = window;
            for (let i = 0; i < parts.length - 1; i++) {
                obj = obj[parts[i]];
            }
            const fnName = parts[parts.length - 1];
            const fn = obj[fnName];
            if (typeof fn === "function") {
                fn.apply(obj, args);
            }
            else {
                console.warn(`Function ${fnPath} not found`);
            }
        });
    }
    /**
     * get default value if need
     * param val {object} checked value
     * param defVal {object} default value to return if need
     */
    static default(val, defVal) {
        return (val == null) ? defVal : val;
    }
    static hasValue(obj) {
        return !(obj == null);
    }
    static async onSetLocaleA(code) {
        await _Ajax.getStrA('../Fun/SetLocale', { code: code }, function (msg) {
            location.reload();
        });
    }
    static block(obj) {
        _Obj.show(_Tool.xWork);
    }
    static unBlock(obj) {
        _Obj.hide(_Tool.xWork);
    }
}
