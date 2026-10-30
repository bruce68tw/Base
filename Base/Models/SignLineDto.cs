//using Base.Enums;

namespace Base.Models
{
    /// <summary>
    /// 簽核流程節點之間的連線、條件與簽核對象設定。
    /// </summary>
    public class SignLineDto
    {
        /// <summary>
        /// 所屬流程識別碼。
        /// </summary>
        public string FlowId { get; set; } = "";
        /// <summary>
        /// 來源節點識別碼。
        /// </summary>
        public string FromNodeId { get; set; } = "";
        /// <summary>
        /// 來源節點名稱。
        /// </summary>
        public string FromNodeName { get; set; } = "";
        /// <summary>
        /// 來源節點種類，使用 NodeTypeEstr 代碼。
        /// </summary>
        public string FromNodeType { get; set; } = "";
        /// <summary>
        /// 目的節點識別碼。
        /// </summary>
        public string ToNodeId { get; set; } = "";
        /// <summary>
        /// 目的節點名稱。
        /// </summary>
        public string ToNodeName { get; set; } = "";
        /// <summary>
        /// 目的節點種類，使用 NodeTypeEstr 代碼。
        /// </summary>
        public string ToNodeType { get; set; } = "";
        /// <summary>
        /// 簽核對象的指定方式，使用 SignerTypeEstr 代碼。
        /// </summary>
        public string SignerType { get; set; } = "";
        /// <summary>
        /// 搭配 SignerType 的欄位、部門或角色等指定值。
        /// </summary>
        public string SignerValue { get; set; } = "";
        /// <summary>
        /// 連線的排序值。
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 決定是否採用連線的條件字串。
        /// </summary>
        public string CondStr { get; set; } = "";

    }
}
