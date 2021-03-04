
namespace Base.Enums
{
    /// <summary>
    /// for query field
    /// </summary>
    public class ItemOpEstr
    {
        //=== same as sql op ===
        public const string Equal = "Equal";
        public const string Like = "Like";
        public const string NotLike = "NotLike";

        //input string list as: u01,u02,...multiple in query for one field
        //backend will replace carrier line to comma for TextArea field
        public const string In = "In";


        //=== not same as sql op ===
        //%xxx% compare
        public const string Like2 = "Like2";

        //input string list as: u01,u02,... multiple like query for one field
        public const string LikeList = "LikeList";

        //input one string, do like query for multiple columns(set Col field)
        public const string LikeCols = "LikeCols";

        //do like('%xxx%') query for multiple columns(set Col and seperate with comma)
        public const string Like2Cols = "Like2Cols";

        public const string Is = "Is";
        public const string IsNull = "IsNull";
        public const string NotNull = "NotNull";
        public const string InRange = "InRange";

        //PG defined
        public const string UserDefined = "UD";

    }//class
}
