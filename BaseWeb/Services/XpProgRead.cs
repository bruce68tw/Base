using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseWeb.Services
{
    public class XpProgRead
    {
        private ReadDto dto = new ReadDto()
        {
            ReadSql = @"
select * from dbo.XpProg
order by Sort
",
            Items = new [] {
                new QitemDto { Fid = "Code", Op = ItemOpEstr.Like },
                new QitemDto { Fid = "Name", Op = ItemOpEstr.Like },
            },
        };

        public JObject GetPage(string ctrl, DtDto dt)
        {
            return new CrudRead().GetPage(ctrl, dto, dt);
        }

    } //class
}