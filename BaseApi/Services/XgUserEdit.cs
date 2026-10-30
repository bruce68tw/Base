using Base.Models;
using Base.Services;

namespace BaseApi.Services
{
    public class XgUserEdit(string ctrl) : BaseEditSvc(ctrl)
    {
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
