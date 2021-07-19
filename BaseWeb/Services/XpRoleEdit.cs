using Base.Models;
using Base.Services;

namespace BaseWeb.Services
{
    public class XpRoleEdit : MyEdit
    {
        public XpRoleEdit(string ctrl) : base(ctrl) { }

        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpRole",
                PkeyFid = "Id",
                Col4 = null,
                Items = new [] 
				{
					new EitemDto { Fid = "Id" },
					new EitemDto { Fid = "Name" },
                },
                Childs = new EditDto[]
                {
                    new EditDto
                    {
                        Table = "dbo.XpUserRole",
                        PkeyFid = "Id",
                        FkeyFid = "RoleId",
                        Col4 = null,
                        Items = new []
                        {
                            new EitemDto { Fid = "Id" },
                            new EitemDto { Fid = "UserId", Required = true },
                            new EitemDto { Fid = "RoleId" },
                        },
                    },
                    new EditDto
                    {
                        Table = "dbo.XpRoleProg",
                        PkeyFid = "Id",
                        FkeyFid = "RoleId",
                        Col4 = null,
                        Items = new [] 
						{
							new EitemDto { Fid = "Id" },
							new EitemDto { Fid = "RoleId" },
							new EitemDto { Fid = "ProgId", Required = true },
                            new EitemDto { Fid = "FunCreate" },
                            new EitemDto { Fid = "FunRead" },
                            new EitemDto { Fid = "FunUpdate" },
                            new EitemDto { Fid = "FunDelete" },
                            new EitemDto { Fid = "FunPrint" },
                            new EitemDto { Fid = "FunExport" },
                            new EitemDto { Fid = "FunView" },
                        },
                    },
                },
            };
        }

    } //class
}
