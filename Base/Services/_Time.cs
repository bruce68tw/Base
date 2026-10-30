namespace Base.Services
{
    /// <summary>
    /// 目前執行緒的同步暫停工具。
    /// </summary>
    public class _Time
    {
        /// <summary>
        /// 阻塞目前執行緒指定毫秒數，並非非同步等待。
        /// </summary>
        public static void Sleep(int miniSec)
        {
            System.Threading.Thread.Sleep(miniSec);
        }
    }
}
