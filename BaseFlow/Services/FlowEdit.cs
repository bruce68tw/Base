using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseFlow.Services
{
    public class FlowEdit
    {
        private EditDto GetDto()
        {
            return new EditDto()
            {
                Table = "dbo.Flow",
                PkeyFid = "Id",
                Col4 = null,
                Items = new [] {
                    new EitemDto { Fid = "Id" },
                    new EitemDto { Fid = "Code", Required = true },
                    new EitemDto { Fid = "Name", Required = true },
                    new EitemDto { Fid = "Portrait", Value = "1" },
                    new EitemDto { Fid = "Status", Value = "1" },
                },
                Childs = new EditDto[]
                {
                    new EditDto
                    {
                        Table = "dbo.FlowNode",
                        PkeyFid = "Id",
                        FkeyFid = "FlowId",
                        Col4 = null,
                        Items = new []
                        {
                            new EitemDto { Fid = "Id" },
                            new EitemDto { Fid = "FlowId" },
                            new EitemDto { Fid = "Name",    Required = true },
                            new EitemDto { Fid = "NodeType", Required = true },
                            new EitemDto { Fid = "PosX", Required = true },
                            new EitemDto { Fid = "PosY", Required = true },
                            new EitemDto { Fid = "SignerType" },
                            new EitemDto { Fid = "SignerValue" },
                            new EitemDto { Fid = "PassType", Value = "0" },
                            //new EitemDto { Fid = "PassNum" },
                        },
                    },
                    new EditDto
                    {
                        Table = "dbo.FlowLine",
                        PkeyFid = "Id",
                        FkeyFid = "FlowId",
                        Col4 = null,
                        Items = new []
                        {
                            new EitemDto { Fid = "Id" },
                            new EitemDto { Fid = "FlowId" },
                            new EitemDto { Fid = "CondStr" },
                            new EitemDto { Fid = "StartNode", Required = true },
                            new EitemDto { Fid = "EndNode",   Required = true },
                            new EitemDto { Fid = "Sort", Required = true },
                        },
                    },
                },
            };
        }

        private CrudEdit Service()
        {
            return new CrudEdit(GetDto());
        }

        public JObject GetJson(string key)
        {
            return Service().GetJson(key);
        }

        public ResultDto Create(JObject json, FnSetNewKeyJson fnSetNewKey)
        {
            return Service().Create(json, fnSetNewKey);
        }

        public ResultDto Update(string key, JObject json, FnSetNewKeyJson fnSetNewKey)
        {
            return Service().Update(key, json, fnSetNewKey);
        }

        public ResultDto Delete(string key)
        {
            return Service().Delete(key);
        }

    } //class
}
