using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
namespace BaseApi.Services
{
    public class XgOtpSvc
    {
        /// <summary>
        /// send email, 如果有error 應為系統錯誤, 所以log error and return false
        /// </summary>
        /// <param name="fromServer"></param>
        /// <param name="email"></param>
        /// <param name="tplPath">email template file path</param>
        /// <returns>callback url, 'E'(error)</returns>
        public async Task<bool> SendEmailA(string clientIp, string email, string tplPath)
        {
            //client主機是否被授權使用本服務
            var row = await GetClientRowA(clientIp);
            if (row == null)
                return false;

            //email 範本固定 _template\OtpEmail.html
            var html = await _File.ToStrA(tplPath);
            if (string.IsNullOrEmpty(html))
            {
                _Log.Error("找不到範本檔案: " + tplPath);
                return false;
            }

            //設定資料 for email template
            var otpCode = GenCode();
            var json = new JObject
            {
                { "OtpCode", otpCode },
                { "OkMin", _Fun.Config.OtpEmailMin },
            };

            //寄送email
            var dto = new EmailDto()
            {
                Subject = $"一次性密碼(OTP)發送通知",
                ToUsers = [email],
                Body = _Str.ReplaceJson(html, json),
            };
            await _Email.SendByDtoA(dto);

            //Db記錄驗証碼
            return await SaveCodeA(clientIp, email, OtpTypeEstr.Email, otpCode, _Fun.Config.OtpEmailMin);
        }

        private async Task<JObject?> GetClientRowA(string clientIp)
        {
            var row = await _Db.GetRowA($"select * from dbo.XpServer where Ip='{clientIp}'");
            if (row == null)
                _Log.Error($"ip={clientIp} 無權限使用 XpOtp Controller");
            return row;
        }

        /// <summary>
        /// Db記錄驗証碼
        /// </summary>
        /// <param name="fromServer"></param>
        /// <param name="userId">可以是email</param>
        /// <param name="otpType"></param>
        /// <param name="okMin">有效時間(分)</param>
        /// <returns></returns>
        private async Task<bool> SaveCodeA(string fromServer, string userId, string otpType, string otpCode, int okMin)
        {
            //產生驗証碼
            //var otpCode = GenCode();
            var now = DateTime.Now;
            var expireDt = now.AddMinutes(10); //10分鐘後過期

            //不處理舊資料, 直接新增
            var sql = $@"
insert into dbo.XpOtpLog(FromServer, UserId, OtpType, 
    OtpCode, ExpireDt, IsUsed, Created) 
values(@FromServer, @UserId, '{otpType}',
    '{otpCode}', '{_Date.GetDtStr(expireDt)}', 0, '{_Date.GetDtStr(now)}')
";
            return (await _Db.ExecSqlA(sql, ["FromServer", fromServer, "UserId", userId]) == 1);
        }

        //產生5碼數字驗証碼
        private string GenCode()
        {
            return _Str.RandomStr(5, RandomTypeEnum.Num);
        }

    } //class
}