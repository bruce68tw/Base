namespace BaseOffice.Models
{
    //word set template dto
    /// <summary>保存範本文字的命中片段及其起訖位置。</summary>
    public class WordSetTplDto
    {
        //found template string
        public string TplStr = "";

        //found start pos
        public int StartPos = 0;

        //found end pos(include found end string length)
        public int EndPos = 0;

    }
}