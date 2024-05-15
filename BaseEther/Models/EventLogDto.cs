using Nethereum.ABI.FunctionEncoding.Attributes;

namespace BaseEther.Models
{
    [Event("EventLog")]
    public class EventLogDto : IEventDTO
    {
        //第4個參數表示是否建立索引, 必須與solidity event內容一致
        //log屬性配合solidity使用小寫
        [Parameter("string", "log", 1, false)]
        public string log { get; set; }

        /*
        [Parameter("string", "myString", 1, true)]
        public string MyString { get; set; }

        [Parameter("uint256", "myNumber", 2, true)]
        public BigInteger MyNumber { get; set; }
        */
    }
}
