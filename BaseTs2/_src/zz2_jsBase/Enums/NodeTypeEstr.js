//flow node type enum
export default class NodeTypeEstr {
    // 原始物件字面量中的常數值被轉換為 static readonly 屬性，以符合 class 語法
    // 且保持 NodeTypeEstr.Start 這樣的外部存取方式。
    static Start = 'S'; //startNode
    static End = 'E'; //endNode
    static Node = 'N'; //normal node
}
