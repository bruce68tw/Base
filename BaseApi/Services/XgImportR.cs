using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    public class XgImportR
    {
        private readonly string _importType;

        //constructor
        public XgImportR(string importType)
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
order by Created desc
",
                Items = [
                    new() { Fid = "FileName", Op = ItemOpEstr.Like },
                ],
            };
        }

        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(GetDto(), dt, ctrl);
        }

    } //class
}