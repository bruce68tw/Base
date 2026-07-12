//輸入欄位種類, 對應 Base InputTypeEstr 
export default class InputTypeEstr {
    // 原始物件字面量中的常數值被轉換為 static readonly 屬性，以符合 class 語法
    // 且保持 InputTypeEstr.Check 這樣的外部存取方式。
    static Check = "check";
    static Date = "date";
    static DateTime = "dt";
    static Decimal = "dec";
    static File = "file";
    static Hide = "hide";
    static Html = "html";
    static Integer = "int";
    static Link = "link";
    static Modal = "modal";
    static Password = "pwd";
    static Radio = "radio";
    static Read = "read";
    static Select = "select";
    static Sort = "sort";
    static Text = "text";
    static Textarea = "textarea";
}
