using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    public class _XgFlow
    {
        //match to XpCode.Type="AndOr" && Flow.js
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
        //get first role member
        public static string SqlRole = "select UserId from dbo.XpUserRole where RoleId='{0}'";

        /// <summary>
        /// create workflow signing rows
        /// </summary>
        /// <param name="row">flow data</param>
        /// <param name="userFid">fid of owner user id of row</param>
        /// <param name="flowCode">Flow.Code</param>
        /// <param name="sourceId">source row Id(key)</param>
        /// <param name="db"></param>
        /// <returns>error msg if any</returns>
        public static async Task<string> CreateSignRowsAsync(JObject row, string userFid, string flowCode,
            string sourceId, bool isTest, Db db)
        {
            #region 1.get flow lines by flow code
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
            var flowLines = await db.GetModelsAsync<SignLineDto>(sql);
            #endregion

            #region 2.get start node id/name
            var firstLine = flowLines
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
            #endregion

            //3.get matched lines
            var signTable = GetSignTable(isTest);
            var findIdxs = new List<int>(); //found lines for insert XpFlowSign/XpFlowSignTest
            while (true)
            {
                #region 4.get lines of current node
                //CondStr with value will check first !!
                var nodeLines = flowLines
                    .Where(a => a.StartNodeId == nowNodeId)
                    //.OrderByDescending(a => string.IsNullOrEmpty(a.CondStr))
                    .OrderBy(a => string.IsNullOrEmpty(a.CondStr))
                    .ThenBy(a => a.Sort)
                    .ToList();
                #endregion

                #region 5.get matched line by condition string
                SignLineDto findLine = null;
                foreach(var line in nodeLines)
                {
                    if (IsLineMatch(ref error, row, line.CondStr))
                    {
                        findLine = line;
                        break;
                    }
                    else if(!_Str.IsEmpty(error))
                        goto lab_exit;
                }

                //return error if no matched line
                if (findLine == null)
                {
                    error = "No Match Line for StartNode=" + nowNodeName;
                    goto lab_exit;
                }

                //check endless loop
                var idx = flowLines.IndexOf(findLine);
                if (findIdxs.IndexOf(idx) >= 0)
                {
                    error = "Find Node Twice(" + nodeLines[idx].StartNodeName + ")";
                    goto lab_exit;
                }
                #endregion

                //add found line index
                findIdxs.Add(idx);

                //when end node then exit loop
                if (findLine.EndNodeType == NodeTypeEstr.End)
                    break;

                //set node id/name for next loop
                nowNodeId = findLine.EndNodeId;
                nowNodeName = findLine.EndNodeName;
            }//loop

            #region 6.prepare sql for insert XpFlowSign/XpFlowSignTest
            sql = $@"
insert into dbo.{signTable}(
    Id, FlowId, SourceId, 
    NodeName, FlowLevel, TotalLevel,
    SignerId, SignerName, 
    SignStatus, SignTime) values(
    @Id, @FlowId, @SourceId,
    @NodeName, @FlowLevel, @TotalLevel,
    @SignerId, @SignerName, 
    @SignStatus, @SignTime)
";
            #endregion

            //insert XpFlowSign/XpFlowSignTest rows
            var totalLevel = findIdxs.Count - 1;
            var level = 0;  //current flow level, start 0
            var userType = "";
            foreach (var idx in findIdxs)
            {
                #region 7.get signer Id/name by rules
                var line = flowLines[idx];
                var signerId = "";
                DateTime? signTime = null;
                if (level == 0)
                {
                    userType = "UserId";
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
                            userType = line.SignerValue;
                            if (row[line.SignerValue] != null)
                                signerId = row[line.SignerValue].ToString();
                            break;
                        case SignerTypeEstr.UserMgr:
                            userType = "User Manager";
                            if (row[userFid] != null)
                                signerId = await db.GetStrAsync(string.Format(SqlUserMgr, row[userFid].ToString()));
                            break;
                        case SignerTypeEstr.DeptMgr:
                            userType = "Depart Manager";
                            if (line.SignerValue != null)
                                signerId = await db.GetStrAsync(string.Format(SqlDeptMgr, line.SignerValue));
                            break;
                        case SignerTypeEstr.Role:
                            userType = "Role";
                            if (line.SignerValue != null)
                                signerId = await db.GetStrAsync(string.Format(SqlRole, line.SignerValue));
                            break;
                    }
                }

                if (string.IsNullOrEmpty(signerId))
                {
                    error = $"SignerId is empty. ({userType})";
                    goto lab_exit;
                }

                //get signer Name
                var signerName = await db.GetStrAsync(string.Format(SqlUserName, signerId));
                if (string.IsNullOrEmpty(signerName))
                {
                    error = $"SignerId not existed. ({userType}={signerId})";
                    goto lab_exit;
                }
                #endregion

                #region 8.insert XpFlowSign/XpFlowSignTest
                await db.ExecSqlAsync(sql, new List<object>() {
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
                #endregion
            }

            //case of ok
            return "";

        //case of error
        lab_exit:
            return isTest
                ? error
                : $"_XpFlow.cs CreateSignRows() failed(Flow.Code={flowCode}): {error}";
        }

        private static string GetSignTable(bool isTest)
        {
            return isTest ? "XpFlowSignTest" : "XpFlowSign";
        }

        /// <summary>
        /// check is line match condition string or not
        /// refer Flow.js _condStrToList()
        /// </summary>
        /// <param name="row"></param>
        /// <param name="condStr"></param>
        /// <returns>match line or not</returns>
        private static bool IsLineMatch(ref string error, JObject row, string condStr)
        {
            if (_Str.IsEmpty(condStr))
                return true;

            //var list = [];
            //var k = 0;
            error = ""; //initial
            var match = false;
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
            error = "_Flow.cs IsLineMatch() failed: " + error;
            return false;
        }

        /// <summary>
        /// sign one row
        /// </summary>
        /// <param name="flowSignId">XpFlowSign.Id</param>
        /// <param name="signYes">agree or not</param>
        /// <param name="signNote">sign note</param>
        /// <param name="sourceTable">source Table for update FlowLevel, FlowStatus columns</param>
        /// <returns>ResultDto for called by controller</returns>
        public static async Task<ResultDto> SignRowAsync(string flowSignId, bool signYes,
            string signNote, string sourceTable, bool isTest)
        {
            #region 1.check XpFlowSign/XpFlowSignTest row existed
            Db db = null;
            var error = "";
            if (!await _Str.CheckKeyAsync(flowSignId))
            {
                //error = $"key has wrong char.({flowSignId})";
                goto lab_error;
            }

            db = new Db();
            await db.BeginTranAsync();

            //get XpFlowSign/XpFlowSignTest row
            var signTable = GetSignTable(isTest);
            var sql = $"select SourceId, FlowLevel, TotalLevel from dbo.{signTable} where Id='{flowSignId}' and SignStatus='0'";
            var row = await db.GetJsonAsync(sql);
            if (row == null)
            {
                error = $"not found {signTable} row.(Id={flowSignId})";
                goto lab_error;
            }
            #endregion

            #region 2.update XpFlowSign/XpFlowSignTest row
            var signStatus = signYes ? "Y" : "N";
            sql = $@"
update dbo.{signTable} set 
    SignStatus=@SignStatus, 
    Note=@Note,
    SignTime=getDate()
where Id='{flowSignId}' 
";
            var count = await db.ExecSqlAsync(sql, new List<object>() { "SignStatus", signStatus, "Note", signNote });
            if (count != 1)
            {
                error = "_XpFlow.cs SignRow() failed, should update one row: " + sql;
                goto lab_error;
            }
            #endregion

            #region 3.update source row FlowLevel/FlowStatus
            //flowStatus: Y(agree flow), N(not agree), 0(signing)
            var nowLevel = Convert.ToInt32(row["FlowLevel"]);
            var flowStatus = !signYes ? "N" :
                (nowLevel == Convert.ToInt32(row["TotalLevel"])) ? "Y" : 
                "0";
            //final flowLevel must between 0-totalLevel
            var flowLevel = (flowStatus == "0") ? nowLevel + 1 : 0;

            //update source table
            var sourceId = row["SourceId"].ToString();
            sql = $@"
update {sourceTable} set
    FlowLevel={flowLevel},
    FlowStatus='{flowStatus}'
where Id='{sourceId}'
";
            #region case ok error
            count = await db.ExecSqlAsync(sql);
            if (count != 1)
            {
                error = "_XpFlow.cs SignRow() failed, should update one row: " + sql;
                goto lab_error;
            }
            #endregion

            #region case of ok
            await db.CommitAsync();
            await db.DisposeAsync();

            return new ResultDto()
            {
                Value = "2",  //update 2 rows
            };
            #endregion
            #endregion

        //case of error
        lab_error:
            if (db != null)
            {
                await db.RollbackAsync();
                await db.DisposeAsync();
            }
            return _Model.GetError(error);
        }

    }//class
}
