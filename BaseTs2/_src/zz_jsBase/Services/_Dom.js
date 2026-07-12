"use strict";
//操作DOM元素
class _Dom {
    //傳回字串, 不會自動轉型
    static getData(elm, fid) {
        return elm.getAttribute("data-" + fid);
    }
    static setData(elm, fid, value) {
        elm.setAttribute("data-" + fid, value);
    }
}
