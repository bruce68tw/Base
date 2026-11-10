using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseFlow.Services
{
    /// <summary>提供簽核流程定義及其子資料的維護設定。</summary>
    public class XgFlowE : BaseEditSvc
    {
        /// <summary>以控制器名稱初始化維護服務。</summary>
        /// <param name="ctrl">對應作業的控制器名稱</param>
        public XgFlowE(string ctrl) : base(ctrl) { }

        /// <summary>取得簽核流程主從資料的維護設定。</summary>
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

        /// <summary>以流程維護設定新增資料。</summary>
        /// <param name="json">要寫入的流程主從資料 JSON</param>
        public override async Task<ResultDto> CreateA(JObject json)
        {
            return await EditSvc().CreateA(json);
        }

        /// <summary>依主鍵更新流程主從資料。</summary>
        /// <param name="key">用來定位資料的鍵值</param>
        /// <param name="json">要寫入的流程主從資料 JSON</param>
        public override async Task<ResultDto> UpdateA(string key, JObject json)
        {
            return await EditSvc().UpdateA(key, json);
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
