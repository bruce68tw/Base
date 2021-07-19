using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseWeb.Services
{
    public class XpEasyRptRead
    {
        private ReadDto dto = new ReadDto()
        {
            ReadSql = @"
select * from dbo.XpEasyRpt
order by Id
",
            Items = new [] {
                new QitemDto { Fid = "Name" },
            },
        };

        public JObject GetPage(string ctrl, DtDto dt)
        {
            return new CrudRead().GetPage(ctrl ,dto, dt);
        }

    } //class
}