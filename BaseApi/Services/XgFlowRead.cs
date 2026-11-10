using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// 查詢 XpFlow 的簽核流程定義並回傳分頁結果。
    /// </summary>
    public class XgFlowRead
    {
        private readonly ReadDto dto = new()
        {
            ReadSql = @"
select *
from dbo.XpFlow
order by Id
",
        };

        /// <summary>
        /// 依控制器名稱及分頁查詢輸入，讀取簽核流程的分頁結果。
        /// </summary>
        /// <param name="ctrl">交由共用 CRUD 查詢使用的控制器名稱。</param>
        /// <param name="dt">包含查詢條件及分頁資訊的輸入。</param>
        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(dto, dt, ctrl);
        }

    } //class
}
