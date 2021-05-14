using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseWeb.Services
{
    public class _XpFlow
    {
        //match to Code.Type="AndOr" && Flow.js
        private const string OrSep = "{O}";
        private const string AndSep = "{A}";
        private const string ColSep = ",";

        //get user's dept manager
        public static string SqlUserMgr = @"
select d.MgrId 
from dbo.Dept d
inner join dbo.[User] u on d.Id=u.DeptId
where u.Id='{0}'
";
        //get user name
        public static string SqlUserName = "select Name from dbo.[User] where Id='{0}'";
        //get dept manager Id
        public static string SqlDeptMgr = "select MgrId from dbo.Dept where Id='{0}'";

        /// <summary>
        /// create workflow signing rows
        /// </summary>
        /// <param name="row">flow data</param>
        /// <param name="userFid">fid of owner user id in row</param>
        /// <param name="flowCode">Flow.Code</param>
        /// <param name="sourceId">source data Id(key)</param>
        /// <param name="db"></param>
        /// <returns>error msg if any</returns>
        public static string CreateSignRows(JObject row, string userFid, 
            string flowCode, string sourceId, Db db)
        {
            //get flow lines
            var error = string.Empty;
            var sql = string.Format(@"
select 
    FlowId=f.Id,
    StartNodeId=l.StartNode,
	StartNodeName=nf.Name,
	StartNodeType=nf.NodeType,
    EndNodeId=l.EndNode,
    EndNodeName=nt.Name,
	EndNodeType=nt.NodeType,
	nf.SignerType, nf.SignerValue,
    l.Sort, l.CondStr
from dbo.XpFlowLine l
join dbo.XpFlow f on l.FlowId=f.Id
join dbo.XpFlowNode nf on l.StartNode=nf.Id
join dbo.XpFlowNode nt on l.EndNode=nt.Id
where f.Code='{0}'
order by l.StartNode, l.Sort
", flowCode);
            var lines = db.GetModels<SignLineDto>(sql);

            /*
            //get start node
            var checkLines = lines
                .Where(a => a.StartNodeType == EnumNodeType.Start)
                .OrderBy(a => a.Sort)
                .ToList();
            var firstLine = checkLines.FirstOrDefault();

            //now start node
            var nowNodeId = firstLine.StartNodeId;
            var nowNodeName = firstLine.StartNodeName;
            */

            //get now line
            var firstLine = lines
                .Where(a => a.StartNodeType == NodeTypeEstr.Start)
                .OrderBy(a => a.Sort)
                .FirstOrDefault();
            if (firstLine == null)
            {
                error = "No Start Node.";
                goto lab_exit;
            }

            var nowNodeId = firstLine.StartNodeId;
            var nowNodeName = firstLine.StartNodeName;

            //
            //string nowNodeId = "", nowNodeName = "";
            //var first = true;
            var findIdxs = new List<int>(); //find index list
            //List<SignLineModel> checkLines = null;
            while(true)
            {
                /*
                if (first)
                {
                    checkLines = lines
                        .Where(a => a.StartNodeType == EnumNodeType.Start)
                        .OrderBy(a => a.Sort)
                        .ToList();

                    first = false;

                    //now start node
                    var firstLine = checkLines.FirstOrDefault();
                    nowNodeId = firstLine.StartNodeId;
                    nowNodeName = firstLine.StartNodeName;
                }
                else
                {
                */
                    var checkLines = lines
                        .Where(a => a.StartNodeId == nowNodeId)
                        .OrderBy(a => a.Sort)
                        .ToList();
                //}
                

                //get matched line
                SignLineDto findLine = null;
                foreach(var line in checkLines)
                {
                    if (IsLineMatch(row, line.CondStr))
                    {
                        findLine = line;
                        break;
                    }
                }

                //return error if no matched line
                if (findLine == null)
                {
                    error = "No Match Line for StartNode=" + nowNodeName;
                    goto lab_exit;
                }

                //check endless loop
                var idx = lines.IndexOf(findLine);
                if (findIdxs.IndexOf(idx) >= 0)
                {
                    error = "Find Node Twice(" + checkLines[idx].StartNodeName + ")";
                    goto lab_exit;
                }

                //add find index
                findIdxs.Add(idx);

                //case of end node
                if (findLine.EndNodeType == NodeTypeEstr.End)
                    break;

                //set variables
                nowNodeId = findLine.EndNodeId;
                nowNodeName = findLine.EndNodeName;
            }

            //case of ok, write XpFlowSign table
            sql = @"
insert into dbo.XpFlowSign(
    Id, FlowId, SourceId, 
    NodeName, FlowLevel, TotalLevel,
    SignerId, SignerName, 
    SignStatus, SignTime) values(
    @Id, @FlowId, @SourceId,
    @NodeName, @FlowLevel, @TotalLevel,
    @SignerId, @SignerName, 
    @SignStatus, @SignTime)
";

            //write first node
            var totalLevel = findIdxs.Count - 1;

            //string signerValue = "", signerName = "";
            var level = 0;
            foreach (var idx in findIdxs)
            {
                #region get signerId by rules
                var line = lines[idx];
                var signerId = "";
                DateTime? signTime = null;
                if (level == 0)
                {
                    signTime = DateTime.Now;
                    if (row[userFid] != null)
                        signerId = row[userFid].ToString();
                } 
                else
                {
                    switch (line.SignerType)
                    {
                        /*
                        case EnumSignerType.User:
                            signerValue = line.SignerValue;
                            break;
                            */
                        case SignerTypeEstr.Fid:
                            if (row[line.SignerValue] != null)
                                signerId = row[line.SignerValue].ToString();
                            break;
                        case SignerTypeEstr.UserMgr:
                            if (row[userFid] != null)
                                signerId = db.GetStr(string.Format(SqlUserMgr, row[userFid].ToString()));
                            break;
                        case SignerTypeEstr.DeptMgr:
                            if (line.SignerValue != null)
                                signerId = db.GetStr(string.Format(SqlDeptMgr, line.SignerValue));
                            break;
                    }
                }
                #endregion

                if (string.IsNullOrEmpty(signerId))
                {
                    error = "SignerId is empty.";
                    goto lab_exit;
                }

                //get signer Name
                var signerName = db.GetStr(string.Format(SqlUserName, signerId));

                //update db: add row
                db.ExecSql(sql, new List<object>() {
                    "Id", _Str.NewId(),
                    "FlowId", line.FlowId,
                    "SourceId", sourceId,
                    "NodeName", line.StartNodeName,
                    "FlowLevel", level,
                    "TotalLevel", totalLevel,
                    "SignerId", signerId,
                    "SignerName", signerName,
                    "SignStatus", (level == 0) ? "1" : "0",
                    "SignTime", signTime,
                });
                level++;
            }

            //case of ok
            return "";

            //case of error
        lab_exit:
            //db.Dispose();
            error = $"_Flow.cs CreateSignRows() failed(Flow.Code={flowCode}): {error}";
            //_Log.Error(error);
            return error;
        }

        /// <summary>
        /// check is line match condition string or not
        /// refer Flow.js condStrToList()
        /// </summary>
        /// <param name="row"></param>
        /// <param name="condStr"></param>
        /// <returns>match status</returns>
        private static bool IsLineMatch(JObject row, string condStr)
        {
            if (string.IsNullOrEmpty(condStr))
                return true;

            //var list = [];
            //var k = 0;
            bool match = false;
            var error = "";
            var orList = condStr.Split(OrSep);
            var orLen = orList.Length;
            //var hasOr = (orLen > 1);
            for (var i = 0; i < orLen; i++)
            {
                match = true;  //line match or not
                var andList = orList[i].Split(AndSep);
                //decimal rowValue2, condValue2;
                foreach (var andItem in andList)
                {
                    #region check column 
                    //check column length
                    var cols = andItem.Split(ColSep);
                    if (cols.Length != 3)
                    {
                        error = $"cols.Length should be 3({andItem})";
                        goto lab_error;
                    }

                    //check column existed
                    if (row[cols[0]] == null)
                    {
                        error = $"no column ({cols[0]})";
                        goto lab_error;
                    }

                    #region check condition
                    var rowValue = row[cols[0]].ToString();
                    var condValue = cols[2];
                    switch (cols[1])
                    {
                        case LineOpEstr.Eq:
                            if (decimal.TryParse(rowValue, out var rowValue2) && decimal.TryParse(condValue, out var condValue2))
                            {
                                if (rowValue2 != condValue2)
                                    match = false;
                            }
                            else
                            {
                                if (rowValue != condValue)
                                    match = false;
                            }
                            break;
                        case LineOpEstr.NotEq:
                            if (decimal.TryParse(rowValue, out rowValue2) && decimal.TryParse(condValue, out condValue2))
                            {
                                if (rowValue2 == condValue2)
                                    match = false;
                            }
                            else
                            {
                                if (rowValue == condValue)
                                    match = false;
                            }
                            break;
                        case LineOpEstr.Gt:
                            if (!decimal.TryParse(rowValue, out rowValue2) || !decimal.TryParse(condValue, out condValue2) || !(rowValue2 > condValue2))
                                match = false;
                            break;
                        case LineOpEstr.Ge:
                            if (!decimal.TryParse(rowValue, out rowValue2) || !decimal.TryParse(condValue, out condValue2) || !(rowValue2 >= condValue2))
                                match = false;
                            break;
                        case LineOpEstr.St:
                            if (!decimal.TryParse(rowValue, out rowValue2) || !decimal.TryParse(condValue, out condValue2) || !(rowValue2 < condValue2))
                                match = false;
                            break;
                        case LineOpEstr.Se:
                            if (!decimal.TryParse(rowValue, out rowValue2) || !decimal.TryParse(condValue, out condValue2) || !(rowValue2 <= condValue2))
                                match = false;
                            break;
                        default:
                            match = false;
                            break;
                    }
                    #endregion (check condition)
                    #endregion (check column)

                    //break loop if not match
                    if (!match)
                        break;  //break for loop(and list)
                }//for and

                if (match)
                    return true;
            }//for or

            //case of not error
            return match;

        //case of error
        lab_error:
            _Log.Error("_Flow.cs IsLineMatch() failed: " + error);
            return false;
        }

        /// <summary>
        /// sign one row
        /// </summary>
        /// <param name="flowSignId">XpFlowSign.Id</param>
        /// <param name="signYes">agree or not</param>
        /// <param name="signNote">sign note</param>
        /// <param name="sourceTable">source Table for update FlowLevel, FlowStatus columns</param>
        /// <param name="sourceId">source Table Key</param>
        /// <param name="db"></param>
        /// <returns>ResultDto for called by controller</returns>
        public static ResultDto SignRow(string flowSignId, bool signYes, string signNote,
            string sourceTable)
        {
            //security check
            Db db = null;
            string error;
            if (!_Str.CheckKeyRule(flowSignId))
            {
                error = $"key has wrong char.({flowSignId})";
                goto lab_error;
            }

            db = new Db();
            db.BeginTran();

            //get XpFlowSign row
            var sql = $"select SourceId, FlowLevel, TotalLevel from dbo.XpFlowSign where Id='{flowSignId}' and SignStatus='0'";
            var row = db.GetJson(sql);
            if (row == null)
            {
                error = $"not found XpFlowSign row.(Id={flowSignId})";
                goto lab_error;
            }

            //sql for update XpFlowSign
            var signStatus = signYes ? "Y" : "N";
            sql = $@"
update dbo.XpFlowSign set 
    SignStatus=@SignStatus, 
    Note=@Note,
    SignTime=getDate()
where Id='{flowSignId}' 
";
            var count = db.ExecSql(sql, new List<object>() { "SignStatus", signStatus, "Note", signNote });
            if (count != 1)
            {
                error = $"should update XpFlowSign one row.({count})";
                goto lab_error;
            }

            //flowStatus: Y(agree flow), N(not agree), 0(signing)
            var flowStatus = !signYes ? "N" :
                (Convert.ToInt32(row["FlowLevel"]) == Convert.ToInt32(row["TotalLevel"])) ? "Y" : 
                "0";

            //update source table
            var sourceId = row["SourceId"].ToString();
            sql = $@"
update {sourceTable} set
    FlowLevel=FlowLevel+1,
    FlowStatus='{flowStatus}'
where Id='{sourceId}'
";
            count = db.ExecSql(sql);
            if (count != 1)
            {
                error = $"should update {sourceTable} one row.({count})";
                goto lab_error;
            }

            //case of ok
            db.Commit();
            db.Dispose();
            return new ResultDto()
            {
                Value = "2",  //update 2 rows
            };

        lab_error:
            if (db != null)
            {
                db.Rollback();
                db.Dispose();
            }
            _Log.Error("_Flow.cs SignRow() failed: " + error);
            return _Fun.GetSystemError();
        }

    }//class
}
