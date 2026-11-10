using Base.Models;
using System.Threading.Tasks;

namespace Base.Interfaces
{
    /// <summary>
    /// 提供簡訊傳送介面。
    /// </summary>
    public interface ISmsSvc
    {
        /// <summary>
        /// 非同步傳送 msg 至 phone，回傳實作的操作結果；true 不保證手機已收到簡訊。
        /// </summary>
        /// <param name="phone">簡訊接收者的電話號碼</param>
        /// <param name="msg">要傳送或記錄的訊息</param>
        Task<bool> SendA(string phone, string msg);

    }
}
