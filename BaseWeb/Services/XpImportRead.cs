using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseWeb.Services
{
    public class XpImportRead
    {
        private string _importType;

        //constructor
        public XpImportRead(string importType)
        {
            _importType = importType;
        }

        private ReadDto GetDto()
        {
            return new ReadDto()
            {
                ReadSql = $@"
select *
from dbo.XpImportLog
where Type='{_importType}'
order by Id desc
",
                Items = new[] {
                    new QitemDto { Fid = "FileName", Op = ItemOpEstr.Like },
                },
            };
        }

        public JObject GetPage(DtDto dt)
        {
            return new CrudRead().GetPage(GetDto(), dt);
        }

    } //class
}