using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
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
        /// 依流程維護設定建立流程主檔、節點和連線，回傳共用 CRUD 結果。
        /// </summary>
        /// <param name="json">新增流程、節點和連線的 CRUD 輸入 JSON。</param>
        public override async Task<ResultDto> CreateA(JObject json)
        {
            return await EditSvc().CreateA(json, GetDto());
        }

        /// <summary>
        /// 以指定主鍵更新流程主檔及子資料，回傳共用 CRUD 結果。
        /// </summary>
        /// <param name="key">要更新的流程主鍵。</param>
        /// <param name="json">更新流程、節點和連線的 CRUD 輸入 JSON。</param>
        public override async Task<ResultDto> UpdateA(string key, JObject json)
        {
            return await EditSvc().UpdateA(key, json, GetDto());
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
