using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    public class _XgFlow
    {
        //match to XpCode.Type="xfAndOr" && XpFlow.js??
        private const string OrSep = "{O}";
        private const string AndSep = "{A}";
        private const string ColSep = ",";

        //get user's dept manager
        public static string SqlUserMgr = @"
select d.MgrId 
from dbo.XpDept d
inner join dbo.XpUser u on d.Id=u.DeptId
where u.Id='{0}'
";
        //get userId by account
        public static string SqlUser = "select UserId from dbo.XpUser where Account='{0}'";
        //get user name
        public static string SqlUserName = "select Name from dbo.XpUser where Id='{0}'";
        //get dept manager Id
        public static string SqlDeptMgr = "select MgrId from dbo.XpDept where Id='{0}'";
        //get first role member
        public static string SqlRole = "select UserId from dbo.XpUserRole where RoleId='{0}'";
        //get first role member, 因為有 '{0}', 使用+號串接
        public static string SqlDeptRole = @"
select top 1 u.Id
from dbo.XpUserRole ur
join dbo.XpUser u on ur.UserId=u.Id
where u.DeptId='" + _Fun.DeptId() + "'" + @"
and ur.RoleId='{0}'
order by u.Id
";

        /// <summary>
        /// 刪除 XpFlowSign、XpFlowMap 簽核記錄
        /// </summary>
        /// <param name="progCode"></param>
        /// <param name="sourceId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task DeleteSignA(string progCode, string sourceId, Db? db = null)
        {
            var sql = $@"
delete s
from dbo.XpFlowSign s
join dbo.XpFlowMap m on s.FlowMapId=m.Id
where m.ProgCode=@ProgCode
and m SourceId=@SourceId;
delete m
from dbo.XpFlowMap m
where m.ProgCode=@ProgCode
and m.SourceId=@SourceId;
";
            await _Db.GetRowsA(sql, ["ProgCode", progCode, "SourceId", sourceId], db);
        }

        /// <summary>
        /// 讀取簽核記錄
        /// </summary>
        /// <param name="progCode"></param>
        /// <param name="sourceId"></param>
        /// <param name="db"></param>
        /// <returns>如果無記錄則傳回[]</returns>
        public static async Task<JArray> GetSignRowsA(string progCode, string sourceId, Db? db = null)
        {
            var sql = $@"
select s.NodeName, s.SignerName, s.GetTime, s.SignTime, s.Note,
    SignStatusName=c.Name
from dbo.XpFlowSign s
join dbo.XpFlowMap m on s.FlowMapId=m.Id
join dbo.XpFlow f on m.FlowId=f.Id
join dbo.XpCode c on c.Type='{_Code.SignStatus}' and s.SignStatus=c.Value
where m.ProgCode='{progCode}'
and m.SourceId='{sourceId}'
order by s.FlowLevel
";
            return await _Db.GetRowsA(sql, null, db) ?? [];
        }

        /// <summary>
        /// ReadSetViewBagA -> SetViewBagA
        /// controller Read set viewBag
        /// </summary>
        /// <param name="viewBag"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task SetViewBagA(dynamic viewBag, Db? db = null)
        {
            var locale = _Fun.MultiLang ? _Locale.GetLocaleNoDash() : "";
            var name = string.IsNullOrEmpty(locale) ? "Name" : "Name_" + locale;
            var sql = $@"
select 
    Value as Id, {name} as Str, Type as Ext
from dbo.XpCode
where Type like 'xf%'
order by Type, Sort";
            var rows = await _Db.SqlToCodeExtsA(sql, null, db);
            if (rows == null) return;

            //await using var db = new Db();
            viewBag.NodeTypes = _Code.FilterList(rows, _Code.NodeType);
            viewBag.SignerTypes = _Code.FilterList(rows, _Code.SignerType);
            viewBag.AndOrs = _Code.FilterList(rows, _Code.AndOr);
            viewBag.LineOps = _Code.FilterList(rows, _Code.LineOp);
            viewBag.LineFromTypes = _Code.FilterList(rows, _Code.LineFromType);
        }

        /// <summary>
        /// CreateSignRowsA -> CreateSignA
        /// create workflow signing rows: XpFlowMap, XpFlowSign(or test)
        /// 修改時不會建立XpFlowMap
        /// </summary>
        /// <param name="isNew">是否新增</param>
        /// <param name="row">flow data</param>
        /// <param name="ownerId">資料擁有者</param>
        /// <param name="flowCode">XpFlow.Code</param>
        /// <param name="progCode">XpProg.Code</param>
        /// <param name="sourceId">source row Id(key)</param>
        /// <param name="db"></param>
        /// <returns>error msg if any</returns>
        public static async Task<string> CreateSignA(bool isNew, JObject row, DateTime now, string ownerId, string flowCode,
            string progCode, string sourceId, string startNodeName, bool isTest, Db db)
        {
            #region 1.get flow lines by flow code
            var error = string.Empty;
            var sql = string.Format(@"
select 
    FlowId=f.Id,
    l.FromNodeId,
	FromNodeName=nf.Name,
	FromNodeType=nf.NodeType,
    l.ToNodeId,
    ToNodeName=nt.Name,
	ToNodeType=nt.NodeType,
	nf.SignerType, nf.SignerValue,
    l.Sort, l.CondStr
from dbo.XpFlowLine l
join dbo.XpFlow f on l.FlowId=f.Id
join dbo.XpFlowNode nf on l.FromNodeId=nf.Id
join dbo.XpFlowNode nt on l.ToNodeId=nt.Id
where f.Code='{0}'
order by l.FromNodeId, l.Sort
", flowCode);
            var flowLines = await db.GetModelsA<SignLineDto>(sql);
            #endregion

            #region 2.get start node id/name
            var firstLine = flowLines!
                .Where(a => a.FromNodeType == NodeTypeEstr.Start)
                .OrderBy(a => a.Sort)
                .FirstOrDefault();
            if (firstLine == null)
            {
                error = "No Start Node.";
                goto lab_exit;
            }

            var nowNodeId = firstLine.FromNodeId;
            var nowNodeName = firstLine.FromNodeName;
            #endregion

            //3.get matched lines: set findLineIdxs
            var findLineIdxs = new List<int>(); //found lines for insert XpFlowSign/XpTestFlowSign
            while (true)
            {
                #region 4.get lines of current node
                //CondStr with value will check first !!
                var nodeLines = flowLines!
                    .Where(a => a.FromNodeId == nowNodeId)
                    //.OrderByDescending(a => string.IsNullOrEmpty(a.CondStr))
                    .OrderBy(a => string.IsNullOrEmpty(a.CondStr))
                    .ThenBy(a => a.Sort)
                    .ToList();
                #endregion

                #region 5.get matched line by condition string
                SignLineDto? findLine = null;
                foreach(var line in nodeLines)
                {
                    if (IsLineMatch(ref error, row, line.CondStr))
                    {
                        findLine = line;
                        break;
                    }
                    else if(_Str.NotEmpty(error)) goto lab_exit;
                }

                //return error if no matched line
                if (findLine == null)
                {
                    error = "No Match Line for FromNode=" + nowNodeName;
                    goto lab_exit;
                }

                //check endless loop
                var idx = flowLines!.IndexOf(findLine);
                if (findLineIdxs.IndexOf(idx) >= 0)
                {
                    error = "Find Node Twice(" + nodeLines[idx].FromNodeName + ")";
                    goto lab_exit;
                }
                #endregion

                //add found line index
                findLineIdxs.Add(idx);

                //when end node then exit loop
                if (findLine.ToNodeType == NodeTypeEstr.End)
                    break;

                //set node id/name for next loop
                nowNodeId = findLine.ToNodeId;
                nowNodeName = findLine.ToNodeName;
            }//loop

            //insert XpFlowMap
            var userId = _Fun.UserId();
            var flowId = firstLine.FlowId;
            var mapTable = GetMapTable(isTest);
            var signTable = GetSignTable(isTest);
            var findLineCount = findLineIdxs.Count; //流程線=關卡數
            int oldTotalLevel = 0;  //目前的XpFlowMap.TotalLevel, base 0
            int newTotalLevel = 0;  //寫入XpFlowMap.TotalLevel, base 0
            string flowMapId = "";
            //FlowLevel=1, 第0關會直接送出
            if (isNew)
            {
                newTotalLevel = findLineCount - 1;
                flowMapId = _Str.NewId();
                sql = $@"
insert into dbo.{mapTable}(
    Id, FlowId, ProgCode, SourceId, 
    TotalLevel, FlowLevel, FlowStatus,
    Creator, Created) values(
'{flowMapId}', '{flowId}', @ProgCode, '{sourceId}',
{newTotalLevel}, 1, '{FlowStatusEstr.Work}',
'{userId}', @Created)
";
                await db.ExecSqlA(sql, [
                    "ProgCode", progCode,
                    "Created", now,
                ]);
            }
            else
            {
                var args = new List<Object>() { "ProgCode", progCode, "SourceId", sourceId };
                sql = @"
select Id, TotalLevel
from dbo.XpFlowMap
where ProgCode=@ProgCode 
and SourceId=@SourceId
";
                var signRow = (await db.GetRowA(sql, args))!;
                flowMapId = signRow["Id"]!.ToString();
                oldTotalLevel = Convert.ToInt32(signRow["TotalLevel"]);
                newTotalLevel = oldTotalLevel + findLineCount;    //不必減1
                //FlowLevel直接到下一關，所以+2
                sql = $@"
update dbo.{mapTable} set
    TotalLevel={newTotalLevel}, FlowLevel={oldTotalLevel + 2}, FlowStatus='{FlowStatusEstr.Work}'
where ProgCode=@ProgCode 
and SourceId=@SourceId
";
                await db.ExecSqlA(sql, args);
            }

            //insert XpFlowSign/XpTestFlowSign rows
            var level = 0;  //current flow level, start 0
            var userType = "";
            foreach (var idx in findLineIdxs)
            {
                #region 7.get signer Id/name by rules
                var line = flowLines[idx];
                var uid = "";
                var signerIds = new List<string>();
                var hasRows = false;
                DateTime? signTime = (level == 0) ? now : null;
                DateTime? getTime = (level <= 1) ? now : null;  //等於上一筆的signTime, level=0也填入有助排序
                if (level == 0)
                {
                    userType = "UserId";
                    uid = ownerId;
                } 
                else
                {
                    //每一條流程線可能多位簽核者
                    switch (line.SignerType)
                    {
                        case SignerTypeEstr.User:
                            userType = "User";
                            uid = await db.GetStrA(string.Format(SqlUser, ownerId));
                            break;
                        case SignerTypeEstr.Fid:
                            userType = line.SignerValue;
                            if (row[line.SignerValue] != null)
                                uid = row[line.SignerValue]!.ToString();
                            break;
                        case SignerTypeEstr.UserMgr:
                            userType = "User Manager";
                            uid = await db.GetStrA(string.Format(SqlUserMgr, ownerId));
                            break;
                        case SignerTypeEstr.DeptMgr:
                            userType = "Dept Manager";
                            if (line.SignerValue != null)
                                uid = await db.GetStrA(string.Format(SqlDeptMgr, line.SignerValue));
                            break;
                        case SignerTypeEstr.Role:
                            hasRows = true; //多筆
                            userType = "Role";
                            if (line.SignerValue != null)
                                signerIds = await db.GetStrsA(string.Format(SqlRole, line.SignerValue));
                            break;
                        case SignerTypeEstr.DeptRole:
                            userType = "Dept Role";
                            if (line.SignerValue != null)
                                uid = await db.GetStrA(string.Format(SqlDeptRole, line.SignerValue));
                            break;
                    }
                }

                if (!hasRows)
                    signerIds!.Add(uid!);

                //6.prepare sql for insert XpFlowSign/XpTestFlowSign
                sql = $@"
insert into dbo.{signTable}(
    Id, FlowMapId, FlowLevel,
    NodeName, SignerId, SignerName, 
    SignStatus, SignTime, GetTime) values(
@Id, '{flowMapId}', @FlowLevel,
@NodeName, @SignerId, @SignerName, 
@SignStatus, @SignTime, @GetTime)
";

                //insert XpFlowSign rows by userId list
                foreach (var signerId in signerIds!)
                {
                    if (string.IsNullOrEmpty(signerId))
                    {
                        error = $"SignerId is empty. ({userType})";
                        goto lab_exit;
                    }

                    //get signer name
                    var signerName = await db.GetStrA(string.Format(SqlUserName, signerId));
                    if (string.IsNullOrEmpty(signerName))
                    {
                        error = $"SignerId not existed. ({userType}={signerId})";
                        goto lab_exit;
                    }
                    #endregion

                    #region 8.insert XpFlowSign/XpTestFlowSign, 同時傳入其他欄位
                    await db.ExecSqlA(sql, [
                        "Id", _Str.NewId(),
                        "NodeName", (level == 0) ? startNodeName : line.FromNodeName,
                        "FlowLevel", oldTotalLevel + level, //考慮退回重簽
                        "SignerId", signerId,
                        "SignerName", signerName,
                        "SignStatus", (level == 0) ? SignStatusEstr.Agree : SignStatusEstr.None,
                        "SignTime", signTime!,
                        "GetTime", getTime!,
                    ]);
                }

                level++;
                #endregion
            }

            //case of ok
            return "";

        //case of error
        lab_exit:
            return isTest
                ? error
                : $"_XgFlow.cs CreateSign() failed(Flow.Code={flowCode}): {error}";
        }

        private static string GetMapTable(bool isTest)
        {
            return isTest ? "XpTestFlowMap" : "XpFlowMap";
        }
        private static string GetSignTable(bool isTest)
        {
            return isTest ? "XpTestFlowSign" : "XpFlowSign";
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
                    var rowValue = row[cols[0]]!.ToString();
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
        /// <returns>ResultDto for called by controller</returns>
        public static async Task<ResultDto> SignRowA(string flowSignId, bool signYes,
            string signNote, bool isTest, FnAfterSignRowA? FnAfterSignRowA = null)
        {
            #region 1.check XpFlowSign/XpTestFlowSign row existed
            Db? db = null;
            var error = "";
            if (!_Str.CheckKey(flowSignId)) goto lab_error;

            db = new Db();
            await db.BeginTranA();

            //get XpFlowSign/XpTestFlowSign row
            var flowMapTable = GetMapTable(isTest);
            var flowSignTable = GetSignTable(isTest);
            var sql = $@"
select s.FlowMapId, s.FlowLevel, r.TotalLevel 
from dbo.{flowSignTable} s
join dbo.{flowMapTable} r on s.FlowMapId=r.Id
where s.Id='{flowSignId}' 
and s.SignStatus='{SignStatusEstr.None}'
";
            var signRow = await db.GetRowA(sql);
            if (signRow == null)
            {
                error = $"not found {flowSignTable} row.(Id={flowSignId})";
                goto lab_error;
            }
            #endregion

            #region 2.update XpFlowSign/test row
            var signStatus = signYes ? SignStatusEstr.Agree : SignStatusEstr.Back;
            sql = $@"
update dbo.{flowSignTable} set 
    SignStatus=@SignStatus, 
    Note=@Note,
    SignTime=getDate()
where Id='{flowSignId}' 
";
            var count = await db.ExecSqlA(sql, ["SignStatus", signStatus, "Note", signNote]);
            if (count != 1)
            {
                error = $"{flowSignTable} should only update one row: {sql}";
                goto lab_error;
            }
            #endregion

            #region 3.update XpFlowMap/test table
            //flowStatus: Y(agree flow), N(not agree), 0(signing)
            string flowStatus;
            int nowLevel;
            if (signYes)
            {
                //同意, 如果是最後一關則 Agree, 否則繼續 Work
                nowLevel = Convert.ToInt32(signRow["FlowLevel"]);
                flowStatus = (nowLevel == Convert.ToInt32(signRow["TotalLevel"])) 
                    ? FlowStatusEstr.Agree : FlowStatusEstr.Work;
            }
            else
            {
                //退回
                nowLevel = 0;
                flowStatus = FlowStatusEstr.Back;
            }

            //final flowLevel must between 0-totalLevel
            //繼續流程時, 如果退回則關卡為0
            var flowLevel = (flowStatus != FlowStatusEstr.Work) ? 0 :
                signYes ? nowLevel + 1 :
                _Fun.FlowBackToFirst ? 1 : 0;

            //update XpFlowMap/test set FlowLevel, FlowStatus
            var flowMapId = signRow["FlowMapId"]!.ToString();
            sql = $@"
update dbo.{flowMapTable} set
    FlowLevel={flowLevel},
    FlowStatus='{flowStatus}'
where Id='{flowMapId}'
";
            #endregion

            //case ok error
            count = await db.ExecSqlA(sql);
            if (count != 1)
            {
                error = $"{flowMapTable} should only update one row: {sql}";
                goto lab_error;
            }

            //rollback function if any
            if (FnAfterSignRowA != null)
            {
                try
                {
                    error = await FnAfterSignRowA(flowStatus, db);
                    if (!string.IsNullOrEmpty(error))
                        goto lab_error;
                }
                catch (Exception ex)
                {
                    error = "FnAfterSignRowA() exception: " + ex.Message;
                    goto lab_error;
                }
            }

            //case of ok
            await db.CommitA();
            await db.DisposeAsync();

            return new ResultDto()
            {
                Value = "2",  //update 2 tables
            };

        //case of error
        lab_error:
            if (db != null)
            {
                await db.RollbackA();
                await db.DisposeAsync();
            }
            return _Model.GetError("_XgFlow.cs SignRowA() failed: " + error);
        }

    }//class
}
