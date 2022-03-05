using Base.Models;
using Base.Services;

namespace BaseWeb.Services
{
    public class XgProgEdit : XgEdit
    {
        public XgProgEdit(string ctrl) : base(ctrl) { }

        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpProg",
                PkeyFid = "Id",
                Col4 = null,
                Items = new EitemDto[] 
				{
					new() { Fid = "Id" },
					new() { Fid = "Code", Required = true },
					new() { Fid = "Name", Required = true },
					//new() { Fid = "Icon" },
					new() { Fid = "Url" },
                    new() { Fid = "Sort" },
                    new() { Fid = "AuthRow", Value = 0 },    //default 0
                    new() { Fid = "FunCreate" },
                    new() { Fid = "FunRead" },
                    new() { Fid = "FunUpdate" },
                    new() { Fid = "FunDelete" },
                    new() { Fid = "FunPrint" },
                    new() { Fid = "FunExport" },
                    new() { Fid = "FunView" },
                },
                Childs = new EditDto[]
                {
                    new EditDto
                    {
                        Table = "dbo.XpRoleProg",
                        PkeyFid = "Id",
                        FkeyFid = "ProgId",
                        Col4 = null,
                        Items = new EitemDto[] 
						{
							new() { Fid = "Id" },
							new() { Fid = "ProgId" },
							new() { Fid = "RoleId", Required = true },
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
