namespace SmsTaiwan.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    //[Table("sendMessage")]
    public partial class SmsDto
    {
        [Key]
        public int sid { get; set; }

        //public string username { get; set; }

        //public string password { get; set; }

        //[StringLength(5)]
        //public string rateplan { get; set; }

        //[StringLength(50)]
        //public string srcaddr { get; set; }

        //public string dstaddr { get; set; }

        [StringLength(10)]
        public string encoding { get; set; }

        public string smbody { get; set; }

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
