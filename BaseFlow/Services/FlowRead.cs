using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseFlow.Services
{
    public class FlowRead
    {
        private ReadDto dto = new ReadDto()
        {
            ReadSql = @"
select *
from dbo.Flow
order by Id
",
        };

        public JObject GetPage(DtDto dt)
        {
            return new CrudRead().GetPage(dto, dt);
        }

    } //class
}
