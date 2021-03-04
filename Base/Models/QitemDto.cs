using Base.Enums;

namespace Base.Models
{
    //query fields
    public class QitemDto
    {
        //client field id
        public string Fid;

        //column id, has table alias
        public string Col;

        //where operator
        public string Op = ItemOpEstr.Equal;

        //query field data type
        public QitemTypeEnum Type = QitemTypeEnum.None;        

        //other info, when Type=Date2, Other=another Date Col, ex: ShowEnd/u.ShowEnd
        public string Other;
    }
}
