using Mongo.Enums;

namespace Mongo.Models
{
    //query fields
    public class MgoQitemDto
    {
        //client field id
        public string Fid = "";

        //column id, has table alias
        //public string Col = "";

        /// <summary>
        /// where compare operator, default ItemOpEstr.Equal
        /// </summary>
        public string Op = MgoQitemOpEstr.Equal;

        //column id, has table alias
        public string Value = "";

        //query field data type
        //public QitemTypeEnum Type = QitemTypeEnum.None;        

        //other info, when Type=Date2, Other=another Date Col, ex: ShowEnd/u.ShowEnd
        //public string Other = "";
    }
}
