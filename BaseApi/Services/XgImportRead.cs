using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// 依建構時指定的匯入類型查詢 XpImportLog，並回傳分頁結果。
    /// </summary>
    public class XgImportRead
    {
        private readonly string _importType;

        //建構時設定匯入類型。
        /// <summary>
        /// 設定匯入紀錄查詢使用的匯入類型。
        /// </summary>
        /// <param name="importType">用於篩選匯入紀錄的匯入類型。</param>
        public XgImportRead(string importType)
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
                    new() { Fid = "FileName", Op = QitemOpEstr.Like },
                ],
            };
        }

        /// <summary>
        /// 依控制器名稱及分頁查詢輸入，讀取匯入紀錄的分頁結果。
        /// </summary>
        /// <param name="ctrl">交由共用 CRUD 查詢使用的控制器名稱。</param>
        /// <param name="dt">包含查詢條件及分頁資訊的輸入。</param>
        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(GetDto(), dt, ctrl);
        }

    } //class
}