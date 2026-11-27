using Base.Enums;
using Base.Models;
using Base.Services;
using BaseWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    /// <summary>
    /// 查詢 XpRole 的角色資料並回傳分頁結果。
    /// </summary>
    public class XgRoleRead
    {
        //for controller Read()
        public async Task<ActionResult> ReadA(Controller ctrl)
        {
            await using var db = new Db();
            ctrl.ViewBag.Progs = await _XgCode.ProgsA(db);
            ctrl.ViewBag.AuthRanges = await _XgCode.AuthRangesA(db);
            return ctrl.View("~/Views/XgRole/Read.cshtml");
        }

        private readonly ReadDto dto = new()
        {
            ReadSql = @"
select * from dbo.XpRole
order by Id
",
            Items = [
                new() { Fid = "Name", Op = QitemOpEstr.Like },
            ],
        };

        /// <summary>
        /// 依控制器名稱及分頁查詢輸入，讀取角色的分頁結果。
        /// </summary>
        /// <param name="ctrl">交由共用 CRUD 查詢使用的控制器名稱。</param>
        /// <param name="dt">包含查詢條件及分頁資訊的輸入。</param>
        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(dto, dt, ctrl);
        }

    } //class
}