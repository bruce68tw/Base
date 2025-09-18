namespace BaseWeb.Models
{
    public class XiReadDto : XiBaseDto
    {
        public string Format { get; set; } = "";

        /// <summary>
        /// 是否儲存DB, default false
        /// </summary>
        public bool SaveDb { get; set; } = false;

        /// <summary>
        /// edit field style, default false
        /// </summary>
        public bool EditStyle { get; set; } = false;

        //public int Width { get; set; } = 0;
    }
}