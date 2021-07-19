using Base.Models;
using Base.Services;

namespace BaseWeb.Services
{
    public class XpProgEdit : MyEdit
    {
        public XpProgEdit(string ctrl) : base(ctrl) { }

        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpProg",
                PkeyFid = "Id",
                Col4 = null,
                Items = new [] 
				{
					new EitemDto { Fid = "Id" },
					new EitemDto { Fid = "Code", Required = true },
					new EitemDto { Fid = "Name", Required = true },
					//new EitemDto { Fid = "Icon" },
					new EitemDto { Fid = "Url" },
                    new EitemDto { Fid = "Sort" },
                    new EitemDto { Fid = "AuthRow", Value = 0 },    //default 0
                    new EitemDto { Fid = "FunCreate" },
                    new EitemDto { Fid = "FunRead" },
                    new EitemDto { Fid = "FunUpdate" },
                    new EitemDto { Fid = "FunDelete" },
                    new EitemDto { Fid = "FunPrint" },
                    new EitemDto { Fid = "FunExport" },
                    new EitemDto { Fid = "FunView" },
                },
                Childs = new EditDto[]
                {
                    new EditDto
                    {
                        Table = "dbo.XpRoleProg",
                        PkeyFid = "Id",
                        FkeyFid = "ProgId",
                        Col4 = null,
                        Items = new [] 
						{
							new EitemDto { Fid = "Id" },
							new EitemDto { Fid = "ProgId" },
							new EitemDto { Fid = "RoleId", Required = true },
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
