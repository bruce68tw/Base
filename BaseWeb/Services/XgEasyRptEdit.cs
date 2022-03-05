using Base.Models;
using Base.Services;

namespace BaseWeb.Services
{
    public class XgEasyRptEdit : XgEdit
    {
        public XgEasyRptEdit(string ctrl) : base(ctrl) { }

        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpEasyRpt",
                PkeyFid = "Id",
                Col4 = null,
                Items = new EitemDto[] 
				{
					new() { Fid = "Id" },
					new() { Fid = "Name", Required = true },
                    new() { Fid = "TplFile", Required = true },
                    new() { Fid = "ToEmails" },
					//new() { Fid = "ToRoles" },
					new() { Fid = "Sql", Required = true },
					new() { Fid = "Status" },
                },
            };
        }

    } //class
}
