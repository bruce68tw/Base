using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseWeb.Services
{
    public class XpTranLogRead
    {
        private ReadDto dto = new ReadDto()
        {
            ReadSql = @"
select *
from dbo.XpTranLog
order by Sn desc
",
            Items = new [] {
                new QitemDto { Fid = "TableName", Op = ItemOpEstr.Like },
                new QitemDto { Fid = "ColName", Op = ItemOpEstr.Like },
                new QitemDto { Fid = "RowId" },
            },
        };

        public JObject GetPage(string ctrl, DtDto dt)
        {
            return new CrudRead().GetPage(ctrl, dto, dt);
        }

    } //class
}