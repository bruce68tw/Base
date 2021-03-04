using Base.Enums;

namespace Base.Models
{
    /// <summary>
    /// for crud edit form field
    /// </summary>
    public class EitemDto
    {
        //frent side field id
        public string Fid = "";

        //where column id, has table alias
        public string Col = "";

        //field type
        //public QitemTypeEnum Type = QitemTypeEnum.None;

        //check type
        public string CheckType = CheckTypeEstr.None;

        //check data for CheckType, ex:
        //"1,5" for Range, "1" for Min/Max
        public string CheckData = "";

        /// <summary>
        /// default value when add new
        /// </summary>
        public object Value;

        //required or not
        public bool Required = false;

        //allow create 
        public bool Create = true;

        //allow update
        public bool Update = true;
    }
}
