
namespace Base.Services
{
    /// <summary>
    /// 簡單測試佔位方法，非自動測試執行器。
    /// </summary>
    public class _Test
    {
        /// <summary>
        /// 回傳固定字串 Hello.。
        /// </summary>
        public static string GetHello()
        {
            return "Hello.";
        }

        /// <summary>
        /// 回傳固定字串 OK，不執行傳入的 SQL。
        /// </summary>
        public static string RunSql(string sql)
        {
            //xxx

            return "OK";
        }
    }
}
