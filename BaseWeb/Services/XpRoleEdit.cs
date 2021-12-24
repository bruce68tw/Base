using Base.Models;
using Base.Services;

namespace BaseWeb.Services
{
    public class XpRoleEdit : XpEdit
    {
        public XpRoleEdit(string ctrl) : base(ctrl) { }

        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpRole",
                PkeyFid = "Id",
                Col4 = null,
                Items = new EitemDto[] 
				{
					new() { Fid = "Id" },
					new() { Fid = "Name" },
                },
                Childs = new EditDto[]
                {
                    new EditDto
                    {
                        Table = "dbo.XpUserRole",
                        PkeyFid = "Id",
                        FkeyFid = "RoleId",
                        Col4 = null,
                        Items = new EitemDto[]
                        {
                            new() { Fid = "Id" },
                            new() { Fid = "UserId", Required = true },
                            new() { Fid = "RoleId" },
                        },
                    },
                    new EditDto
                    {
                        ReadSql = @"
select a.*
from dbo.XpRoleProg a
join dbo.XpProg p on a.ProgId=p.Id
where a.RoleId in ({0})
order by p.Sort
",
                        Table = "dbo.XpRoleProg",
                        PkeyFid = "Id",
                        FkeyFid = "RoleId",
                        //OrderBy = "",
                        Col4 = null,
                        Items = new EitemDto[] 
						{
							new() { Fid = "Id" },
							new() { Fid = "RoleId" },
							new() { Fid = "ProgId", Required = true },
                            new() { Fid = "FunCreate" },
                            new() { Fid = "FunRead" },
                            new() { Fid = "FunUpdate" },
                            new() { Fid = "FunDelete" },
                            new() { Fid = "FunPrint" },
                            new() { Fid = "FunExport" },
                            new() { Fid = "FunView" },
                        },
                    },
                },
            };
        }

    } //class
}
