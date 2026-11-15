namespace SmsTaiwan.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    //[Table("sendMessage")]
    /// <summary>保存企業簡訊請求需要的編碼與內容。</summary>
    public partial class SmsDto
    {
        /// <summary>標記為主鍵的簡訊識別值。</summary>
        [Key]
        public int sid { get; set; }

        //public string username { get; set; }

        //public string password { get; set; }

        //[StringLength(5)]
        //public string rateplan { get; set; }

        //[StringLength(50)]
        //public string srcaddr { get; set; }

        //public string dstaddr { get; set; }

        /// <summary>傳給供應商的簡訊編碼名稱，例如 BIG5 或 LBIG5。</summary>
        [StringLength(10)]
        public string encoding { get; set; } = "";

        /// <summary>依供應商格式編碼的簡訊內容。</summary>
        public string smbody { get; set; } = "";

        //public string smbodyOriginal { get; set; }

        //public int? vldtime { get; set; }

        //[StringLength(128)]
        //public string response { get; set; }

        //[StringLength(50)]
        //public string msgid { get; set; }

        //public int? statuscode { get; set; }

        //[StringLength(16)]
        //public string statusstr { get; set; }

        //[StringLength(32)]
        //public string donetime { get; set; }

        //public int? point { get; set; }

        //[StringLength(20)]
        //public string createUser { get; set; }

        //public DateTime? createDate { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public int? createYear { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public int? createMonth { get; set; }

        //[StringLength(50)]
        //public string dlvtime { get; set; }
    }
}
