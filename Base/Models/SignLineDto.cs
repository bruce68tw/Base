//using Base.Enums;

namespace Base.Models
{
	//workflow sign line
    public class SignLineDto
    {
        public string FlowId { get; set; }
        public string StartNodeId { get; set; }
        public string StartNodeName { get; set; }
        public string StartNodeType { get; set; }

        public string EndNodeId { get; set; }
        public string EndNodeName { get; set; }
        public string EndNodeType { get; set; }

        public string SignerType { get; set; }
        public string SignerValue { get; set; }

        public int Sort { get; set; }
        public string CondStr { get; set; }

    }
}