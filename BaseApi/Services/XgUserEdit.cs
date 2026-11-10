using Base.Models;
using Base.Services;

namespace BaseApi.Services
{
    /// <summary>
    /// 定義使用者及其角色關聯的 CRUD 維護設定。
    /// </summary>
    /// <param name="ctrl">傳給基底維護服務的控制器名稱。</param>
    public class XgUserEdit(string ctrl) : BaseEditSvc(ctrl)
    {
        /// <summary>
        /// 取得使用者的共用 CRUD 維護設定。
        /// </summary>
        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpUser",
                PkeyFid = "Id",
                Col4 = null,
                Items =
                [
                    new() { Fid = "Id" },
					new() { Fid = "Account" },
					new() { Fid = "Name" },
                    new() { Fid = "Pwd", Value = "" },
					new() { Fid = "Status" },
                ],
                Childs =
                [
                    new EditDto
                    {
                        Table = "dbo.XpUserRole",
                        PkeyFid = "Id",
                        FkeyFid = "UserId",
                        Col4 = null,
                        Items =
                        [
                            new() { Fid = "Id" },
							new() { Fid = "UserId" },
							new() { Fid = "RoleId", Required = true },
                        ],
                    },
                ],
            };
        }

    } //class
}
