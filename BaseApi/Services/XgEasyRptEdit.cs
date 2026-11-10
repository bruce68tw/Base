using Base.Enums;
using Base.Models;
using Base.Services;

namespace BaseApi.Services
{
    /// <summary>
    /// 定義簡易報表資料表 XpEasyRpt 的 CRUD 維護設定。
    /// </summary>
    public class XgEasyRptEdit : BaseEditSvc
    {
        /// <summary>
        /// 以指定的控制器名稱初始化共用維護服務。
        /// </summary>
        /// <param name="ctrl">傳給基底維護服務的控制器名稱。</param>
        public XgEasyRptEdit(string ctrl) : base(ctrl) { }

        /// <summary>
        /// 取得簡易報表的共用 CRUD 維護設定。
        /// </summary>
        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpEasyRpt",
                PkeyFid = "Id",
                Col4 = null,
                Items =
                [
                    new() { Fid = "Id" },
					new() { Fid = "Name", Required = true },
                    new() { Fid = "TplFile", Required = true },
                    new() { Fid = "ToEmails" },
					//new() { Fid = "ToRoles" },
					new() { Fid = "Sql", Required = true },
					new() { Fid = "Status" },
                ],
            };
        }

    } //class
}
