//using Base.Enums;

namespace Base.Models
{
	//workflow sign line
    public class SignLineDto
    {
        public string FlowId { get; set; } = "";
        public string FromNodeId { get; set; } = "";
        public string FromNodeName { get; set; } = "";
        public string FromNodeType { get; set; } = "";

        public string ToNodeId { get; set; } = "";
        public string ToNodeName { get; set; } = "";
        public string ToNodeType { get; set; } = "";

        public string SignerType { get; set; } = "";
        public string SignerValue { get; set; } = "";

        public int Sort { get; set; }
        public string CondStr { get; set; } = "";

    }
}