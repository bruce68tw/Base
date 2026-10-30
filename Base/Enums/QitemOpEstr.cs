namespace Base.Enums
{
    /// <summary>
    /// 查詢欄位的運算代碼，由 CrudReadSvc 組成查詢條件。
    /// </summary>
    public class QitemOpEstr
    {
        /// <summary>
        /// 等值比對，為 QitemDto 的預設運算。
        /// </summary>
        public const string Equal = "Equal";
        /// <summary>
        /// 前綴比對，參數為輸入值加上 %。
        /// </summary>
        public const string Like = "Like";
        /// <summary>
        /// 排除前綴比對，參數為輸入值加上 %。
        /// </summary>
        public const string NotLike = "NotLike";
        /// <summary>
        /// 以逗號或換行分隔的值清單進行 IN 比對。
        /// </summary>
        public const string In = "In";
        /// <summary>
        /// 包含比對，參數前後加上 %。
        /// </summary>
        public const string Like2 = "Like2";
        /// <summary>
        /// 逗號分隔多個值，以 OR 連接前綴比對。
        /// </summary>
        public const string LikeList = "LikeList";
        /// <summary>
        /// Col 指定逗號分隔多欄位，以 OR 連接前綴比對。
        /// </summary>
        public const string LikeCols = "LikeCols";
        /// <summary>
        /// Col 指定逗號分隔多欄位，以 OR 連接包含比對。
        /// </summary>
        public const string Like2Cols = "Like2Cols";
        /// <summary>
        /// 空值選擇：輸入 1 查非 NULL，其餘輸入查 NULL。
        /// </summary>
        public const string Is = "Is";
        /// <summary>
        /// 輸入 1 時加入 IS NULL 條件。
        /// </summary>
        public const string IsNull = "IsNull";
        /// <summary>
        /// 輸入 1 時加入 IS NOT NULL 條件。
        /// </summary>
        public const string NotNull = "NotNull";
        /// <summary>
        /// 包含端點的上下限查詢，第二個輸入鍵為 Fid 加上 2。
        /// </summary>
        public const string InRange = "InRange";
        /// <summary>
        /// 直接組合 Col 與輸入值為 SQL 條件，不使用值參數化。
        /// </summary>
        public const string UserDefined = "UD";
    }
}
