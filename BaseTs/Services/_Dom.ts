//操作DOM元素
export default class _Dom {

    //傳回字串, 不會自動轉型
    static getData(elm: HTMLElement, fid: string): string | null {
        return elm.getAttribute("data-" + fid);
    }

    static setData(elm: HTMLElement, fid: string, value: string): void {
        elm.setAttribute("data-" + fid, value);
    }

}