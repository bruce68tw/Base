using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Newtonsoft.Json.Linq;

namespace BaseFlow.Services
{
    /// <summary>
    /// 維護簽核流程定義、節點及連線，並在儲存時處理連線對節點的外鍵。
    /// </summary>
    public class XgFlowEdit : BaseEditSvc
    {
        /// <summary>
        /// 以指定的控制器名稱初始化共用維護服務。
        /// </summary>
        /// <param name="ctrl">傳給基底維護服務的控制器名稱。</param>
        public XgFlowEdit(string ctrl) : base(ctrl) { }

        /// <summary>
        /// 取得簽核流程的共用 CRUD 維護設定。
        /// </summary>
        override public EditDto GetDto()
        {
            return new EditDto()
            {
                FnSetNewKeyJsonA = FnSetNewKeyJsonA,
                Table = "dbo.XpFlow",
                PkeyFid = "Id",
                Col4 = null,
                Items = [
                    new() { Fid = "Id" },
                    new() { Fid = "Code", Required = true },
                    new() { Fid = "Name", Required = true },
                    new() { Fid = "Portrait", Value = "1" },
                    new() { Fid = "Status", Value = "1" },
                ],
                Childs =
                [
                    new EditDto
                    {
                        Table = "dbo.XpFlowNode",
                        PkeyFid = "Id",
                        FkeyFid = "FlowId",
                        Col4 = null,
                        Items =
                        [
                            new() { Fid = "Id" },
                            new() { Fid = "FlowId" },
                            new() { Fid = "Name",    Required = true },
                            new() { Fid = "NodeType", Required = true },
                            new() { Fid = "PosX", Required = true },
                            new() { Fid = "PosY", Required = true },
                            new() { Fid = "SignerType" },
                            new() { Fid = "SignerValue" },
                            new() { Fid = "PassType", Value = "0" },
                            //new() { Fid = "PassNum" },
                        ],
                    },
                    new EditDto
                    {
                        Table = "dbo.XpFlowLine",
                        PkeyFid = "Id",
                        FkeyFid = "FlowId",
                        Col4 = null,
                        Items =
                        [
                            new() { Fid = "Id" },
                            new() { Fid = "FlowId" },
                            new() { Fid = "FromNodeId", Required = true },
                            new() { Fid = "ToNodeId",   Required = true },
                            new() { Fid = "FromType" },
                            new() { Fid = "CondStr" },
                            new() { Fid = "Sort", Required = true },
                        ],
                    },
                ],
            };
        }

        /// <summary>
        /// delegate for setNewKey
        /// </summary>
        /// <param name="inputJson"></param>
        /// <param name="editDto"></param>
        /// <returns></returns>
        private async Task<string> FnSetNewKeyJsonA(bool isNew, CrudEditSvc crudEditSvc, JObject inputJson, EditDto editDto)
        {
            var error = await crudEditSvc.SetNewKeyJsonA(inputJson, editDto);
            if (_Str.NotEmpty(error))
                return error;

            error = crudEditSvc.SetChildFkey(inputJson, 1, "FromNodeId", "00");
            if (_Str.NotEmpty(error))
                return error;

            return crudEditSvc.SetChildFkey(inputJson, 1, "ToNodeId", "00");
        }

    } //class
}
