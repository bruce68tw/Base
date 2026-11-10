using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseFlow.Services
{
    /// <summary>提供簽核流程定義的分頁查詢。</summary>
    public class XgFlowR
    {
        private readonly ReadDto dto = new()
        {
            ReadSql = @"
select *
from dbo.XpFlow
order by Id
",
        };

        /// <summary>依控制器及查詢條件回傳簽核流程的分頁結果。</summary>
        /// <param name="ctrl">對應作業的控制器名稱</param>
        /// <param name="dt">包含分頁、排序與查詢條件的輸入</param>
        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(dto, dt, ctrl);
        }

    } //class
}
