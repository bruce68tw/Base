using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    public class XgFlowE : BaseEditSvc
    {
        public XgFlowE(string ctrl) : base(ctrl) { }

        override public EditDto GetDto()
        {
            return new EditDto()
            {
                Table = "dbo.XpFlow",
                PkeyFid = "Id",
                Col4 = null,
                Items = new EitemDto[] {
                    new() { Fid = "Id" },
                    new() { Fid = "Code", Required = true },
                    new() { Fid = "Name", Required = true },
                    new() { Fid = "Portrait", Value = "1" },
                    new() { Fid = "Status", Value = "1" },
                },
                Childs = new EditDto[]
                {
                    new EditDto
                    {
                        Table = "dbo.XpFlowNode",
                        PkeyFid = "Id",
                        FkeyFid = "FlowId",
                        Col4 = null,
                        Items = new EitemDto[]
                        {
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
                        },
                    },
                    new EditDto
                    {
                        Table = "dbo.XpFlowLine",
                        PkeyFid = "Id",
                        FkeyFid = "FlowId",
                        Col4 = null,
                        Items = new EitemDto[]
                        {
                            new() { Fid = "Id" },
                            new() { Fid = "FlowId" },
                            new() { Fid = "CondStr" },
                            new() { Fid = "StartNode", Required = true },
                            new() { Fid = "EndNode",   Required = true },
                            new() { Fid = "Sort", Required = true },
                        },
                    },
                },
            };
        }

        public async Task<ResultDto> CreateA(JObject json, FnSetNewKeyJsonA fnSetNewKeyA)
        {
            return await EditService().CreateA(json, fnSetNewKeyA);
        }

        public async Task<ResultDto> UpdateA(string key, JObject json, FnSetNewKeyJsonA fnSetNewKeyA)
        {
            return await EditService().UpdateA(key, json, fnSetNewKeyA);
        }

    } //class
}
