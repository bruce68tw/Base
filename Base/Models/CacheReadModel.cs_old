using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 存取DB時使用的 cache組態
    /// </summary>
    public class CacheReadModel
    {
        /// <summary>
        /// primary key 的欄位名稱
        /// </summary>
        public string Kid;

        /// <summary>
        /// primary key 是否為字串, default true
        /// </summary>
        public bool KeyIsString = true;

        /// <summary>
        /// 是否 join table, default false.
        /// </summary>
        public bool IsJoinTable = false;

        /// <summary>
        /// 讀取的 table 清單, 系統會轉小寫
        /// </summary>
        public string[] Tables;

        /// <summary>
        /// where 和 order by 的欄位清單, 不包含 table alias
        /// </summary>
        public List<string> WhereOrders;
    }
}