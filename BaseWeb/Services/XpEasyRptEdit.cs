using Base.Models;
using Base.Services;

namespace BaseWeb.Services
{
    public class XpEasyRptEdit : MyEdit
    {
        public XpEasyRptEdit(string ctrl) : base(ctrl) { }

        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpEasyRpt",
                PkeyFid = "Id",
                Col4 = null,
                Items = new [] 
				{
					new EitemDto { Fid = "Id" },
					new EitemDto { Fid = "Name", Required = true },
                    new EitemDto { Fid = "TplFile", Required = true },
                    new EitemDto { Fid = "ToEmails" },
					//new EitemDto { Fid = "ToRoles" },
					new EitemDto { Fid = "Sql", Required = true },
					new EitemDto { Fid = "Status" },
                },
            };
        }

    } //class
}
