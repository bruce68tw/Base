using Base.Enums;
using Base.Models;
using Base.Services;

namespace BaseApi.Services
{
    /// <summary>
    /// 定義系統作業及其角色權限關聯的 CRUD 維護設定。
    /// </summary>
    public class XgProgEdit : BaseEditSvc
    {
        /// <summary>
        /// 以指定的控制器名稱初始化共用維護服務。
        /// </summary>
        /// <param name="ctrl">傳給基底維護服務的控制器名稱。</param>
        public XgProgEdit(string ctrl) : base(ctrl) { }

        /// <summary>
        /// 取得系統作業的共用 CRUD 維護設定。
        /// </summary>
        override public EditDto GetDto()
        {
            return new EditDto
            {
				Table = "dbo.XpProg",
                PkeyFid = "Id",
                Col4 = null,
                Items =
                [
                    new() { Fid = "Id" },
					new() { Fid = "Code", Required = true },
					new() { Fid = "Name", Required = true },
					//new() { Fid = "Icon" },
					new() { Fid = "Url" },
                    new() { Fid = "Sort" },
                    new() { Fid = "AuthRow", Value = 0 },    //default 0
                    new() { Fid = "FunCreate" },
                    new() { Fid = "FunRead" },
                    new() { Fid = "FunUpdate" },
                    new() { Fid = "FunDelete" },
                    new() { Fid = "FunPrint" },
                    new() { Fid = "FunExport" },
                    new() { Fid = "FunView" },
                ],
                Childs =
                [
                    new EditDto
                    {
                        Table = "dbo.XpRoleProg",
                        PkeyFid = "Id",
                        FkeyFid = "ProgId",
                        Col4 = null,
                        Items =
                        [
                            new() { Fid = "Id" },
							new() { Fid = "ProgId" },
							new() { Fid = "RoleId", Required = true },
                            new() { Fid = "FunCreate" },
                            new() { Fid = "FunRead" },
                            new() { Fid = "FunUpdate" },
                            new() { Fid = "FunDelete" },
                            new() { Fid = "FunPrint" },
                            new() { Fid = "FunExport" },
                            new() { Fid = "FunView" },
                        ],
                    },
                ],
            };
        }
        
    } //class
}
