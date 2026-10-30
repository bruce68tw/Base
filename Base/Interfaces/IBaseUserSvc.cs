using Base.Models;

namespace Base.Interfaces
{
    /// <summary>
    /// 提供 Base 共用服務所需的目前使用者資料。
    /// </summary>
    public interface IBaseUserSvc
    {
        /// <summary>
        /// 取得使用者資料；內建 BaseUserSvc 回傳空白資料，不負責登入驗證。
        /// </summary>
        BaseUserDto GetData();
    }
}
