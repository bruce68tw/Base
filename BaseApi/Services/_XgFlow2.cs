using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// 第2種簽核流程, 使用 XpUser.NextSignerId 做為下一關簽核者, 用法:
    ///  1.XpUser 增加 NextSignerId 欄位
    ///  2.呼叫 _Fun.Init() 後, 設定 _Fun.UseNextSigner = true;
    ///  3.存檔時, 使用 _XgFlow2.CreateSignA 代替 _XgFlow.CreateSignA 來建立簽核資料。
    /// </summary>
    public class _XgFlow2
    {
        /// <summary>
        /// create workflow signing rows: XpFlowMap, XpFlowSign(or test)
        /// 修改時不會建立XpFlowMap
        /// </summary>
        /// <param name="isNew">是否新增, false表示申請者退回後重新送單</param>
        /// <param name="row">flow data</param>
        /// <param name="ownerId">資料擁有者</param>
        /// <param name="progCode">XpProg.Code</param>
        /// <param name="sourceId">source row Id(key)</param>
        /// <param name="db"></param>
        /// <returns>error msg if any</returns>
        public static async Task<string> CreateSignA(bool isNew, JObject row, DateTime now, string ownerId, 
            string progCode, string sourceId, bool isTest, Db db)
        {
            #region insert XpFlowMap, 類似 _XgFlow
            string error, sql, flowMapId;
            var userId = _Fun.UserId();
            var mapTable = _XgFlow.GetMapTable(isTest);
            var signTable = _XgFlow.GetSignTable(isTest);
            var signLevel = 0;
            //FlowLevel=1, 第0關會直接送出
            if (isNew)
            {
                //TotalLevel=0, FlowId填空白(not null)
                flowMapId = _Str.NewId();
                sql = $@"
insert into dbo.{mapTable}(
    Id, FlowId, ProgCode, SourceId, 
    TotalLevel, FlowLevel, FlowStatus,
    Creator, Created) values(
'{flowMapId}', '', @ProgCode, '{sourceId}',
0, 1, '{FlowStatusEstr.Work}',
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
select Id, FlowLevel
from dbo.XpFlowMap
where ProgCode=@ProgCode 
and SourceId=@SourceId
";
                var signRow = (await db.GetRowA(sql, args))!;
                flowMapId = signRow["Id"]!.ToString();
                signLevel = Convert.ToInt32(signRow["FlowLevel"]) + 1;
                //FlowLevel直接到下一關，所以+2
                sql = $@"
update dbo.{mapTable} set
    FlowLevel=FlowLevel+2, FlowStatus='{FlowStatusEstr.Work}'
where ProgCode=@ProgCode 
and SourceId=@SourceId
";
                await db.ExecSqlA(sql, args);
            }
            #endregion

            //get 下個關卡簽核者
            var nextSigner = await GetNextSignerA(userId, db);
            if (nextSigner == null)
            {
                error = "Find No XpUser.NextSignerId.";
                goto lab_exit;
            }

            //寫入2筆 XpFlowSign/XpTestFlowSign, 1:申請者, 2:下一關簽核者
            //XgFlow2不使用 NodeName(='')
            sql = $@"
insert into dbo.{signTable}(
    Id, FlowMapId, FlowLevel,
    NodeName, SignerId, SignerName, 
    SignStatus, GetTime, SignTime) values(
'{_Str.NewId()}', '{flowMapId}', {signLevel},
'', '{userId}', '{_Fun.UserName()}', 
'{SignStatusEstr.Agree}', @Now, @Now)
;
insert into dbo.{signTable}(
    Id, FlowMapId, FlowLevel,
    NodeName, SignerId, SignerName, 
    SignStatus, GetTime, SignTime) values(
'{_Str.NewId()}', '{flowMapId}', {signLevel+1},
'', '{nextSigner!.Id}', '{nextSigner!.Str}', 
'{SignStatusEstr.None}', @Now, null);
";
            await db.ExecSqlA(sql, [
                "Now", now,
            ]);

            //case of ok
            return "";

        //case of error
        lab_exit:
            return isTest
                ? error
                : $"_XgFlow2.cs CreateSignA() failed(XpProg.Code={progCode}, XpFlowMap.SourceId={sourceId}): {error}";
        }

        /// <summary>
        /// get 下個關卡簽核者, 由 XpUser.NextSignerId 決定
        /// 使用inner join, 如果NextSignerId不存在則回傳null, 由呼叫端決定是否要回傳錯誤訊息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<IdStrDto?> GetNextSignerA(string userId, Db db)
        {
            var sql = $@"
select Id=NextSignerId, Str=u2.Name
from dbo.XpUser u
join dbo.XpUser u2 on u.NextSignerId=u2.Id
where u.Id='{userId}'
";
            return await db.GetModelA<IdStrDto>(sql);
        }

    }//class
}
