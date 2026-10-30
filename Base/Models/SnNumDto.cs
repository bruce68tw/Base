namespace Base.Models
{
    /// <summary>
    /// 整數序號與整數值的配對。
    /// </summary>
    public class SnNumDto
    {
        /// <summary>
        /// 項目序號，起算方式由使用端決定。
        /// </summary>
        public int Sn { get; set; }
        /// <summary>
        /// 對應的整數值。
        /// </summary>
        public int Num { get; set; }
    }
}
