//內容為mouse事件名稱
export default class MouseEstr {
    // 原始物件字面量中的常數值被轉換為 static readonly 屬性，以符合 class 語法
    // 且保持 MouseEstr.RightMenu 這樣的外部存取方式。
    // 使用 as const 以避免 event 警示 !!
    static RightMenu = 'contextmenu';
    static MouseDown = 'mousedown';
    static MouseUp = 'mouseup';
    static MouseMove = 'mousemove';
    static MouseEnter = 'mouseenter';
    static MouseLeave = 'mouseleave';
    static DragStart = 'dragstart';
    static DragEnd = 'dragend';
    static DragMove = 'dragmove';
    static DragOver = 'dragover';
    static DragEnter = 'dragenter';
    static DragLeave = 'dragleave';
    static Drop = 'drop';
}
