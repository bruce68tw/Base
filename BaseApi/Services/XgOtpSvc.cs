using Base.Enums;
using Base.Interfaces;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using OtpNet;
using QRCoder;
using System;
using System.Threading.Tasks;
namespace BaseApi.Services
{
    /// <summary>
    /// 處理電子郵件與簡訊的一次性驗證碼、驗證器註冊 QR Code 及驗證器代碼檢查。
    /// </summary>
    public class XgOtpSvc
    {
        //驗証碼長度
        const int CodeLen = 6;

        /// <summary>
        /// 使用指定範本寄送一次性驗證碼，並回傳驗證碼紀錄的儲存結果。
        /// </summary>
        /// <param name="server">在 XpServer 登錄的來源主機識別值。</param>
        /// <param name="email">接收驗證碼的電子郵件地址。</param>
        /// <param name="tplPath">包含驗證碼與有效時間佔位欄位的郵件範本路徑。</param>
        /// <returns>驗證碼紀錄的儲存結果。主機未登錄或範本為空時回傳 false。</returns>
        public async Task<bool> SendEmailA(string server, string email, string tplPath)
        {
            //client主機是否被授權使用本服務
            if (!await HasServerA(server)) return false;

            //email 範本固定 _template\OtpEmail.html
            var html = await _File.ToStrA(tplPath);
            if (string.IsNullOrEmpty(html))
            {
                _Log.Error("找不到範本檔案: " + tplPath);
                return false;
            }

            //設定json資料 for email template
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
            return await SaveOtpLogA(server, email, OtpTypeEstr.Email, otpCode, _Fun.Config.OtpEmailMin);
        }

        /// <summary>
        /// 使用傳入的簡訊服務發送一次性驗證碼。主機未登錄或發送失敗時回傳 false，發送成功後儲存驗證碼。
        /// </summary>
        /// <param name="smsSvc">用來發送簡訊的服務實作。</param>
        /// <param name="server">在 XpServer 登錄的來源主機識別值。</param>
        /// <param name="phone">接收驗證碼的電話號碼。</param>
        public async Task<bool> SendSmsA(ISmsSvc smsSvc, string server, string phone)
        {
            //client主機是否被授權使用本服務
            var row = await GetServerRowA(server);
            if (row == null) return false;

            //寄送簡訊
            var otpCode = GenCode();
            var min = _Fun.Config.OtpSmsMin;
            var msg = string.Format(_Fun.Config.OtpSmsTpl, otpCode, row["SystemName"]!.ToString(), min);
            var status = await smsSvc.SendA(phone, msg);

            //Db記錄驗証碼
            if (status)
                return await SaveOtpLogA(server, phone, OtpTypeEstr.Sms, otpCode, min);
            else
                return false;
        }

        /// <summary>
        /// 現行方法產生 TOTP 註冊用的 Base64 PNG，供畫面顯示。主機未登錄時回傳空字串。
        /// </summary>
        /// <param name="server">在 XpServer 登錄的來源主機識別值。</param>
        /// <param name="email">用於 TOTP 註冊 QR Code 的帳號標籤。</param>
        /// <returns>供畫面顯示的 Base64 PNG，主機未登錄時為空字串。</returns>
        public async Task<string> SendAuthA(string server, string email)
        {
            //因為要傳回QrCode, 所以error時傳回空字串
            if (!await HasServerA(server)) return "";

            //產生QrCode, 不必產生驗証碼
            //var codeLen = 6; //驗証碼為6個字元
            //var issuer = "Duotify"; // 顯示在 APP 中的發行者名稱
            //var label = "user@example.com"; // 顯示在 APP 中的標題
            var uri = new OtpUri(OtpType.Totp, _Fun.Config.OtpAuthKey, email, _Fun.Config.SystemEngName, 
                digits : CodeLen, period : _Fun.Config.OtpAuthSec).ToString();
            byte[] image = PngByteQRCodeHelper.GetQRCode(uri, QRCodeGenerator.ECCLevel.Q, 10);
            return Convert.ToBase64String(image);
        }

        //產生數字驗証碼 for Email & Sms only; Authenticator自動產生
        private string GenCode()
        {
            return _Str.RandomStr(CodeLen, RandomTypeEnum.Num);
        }

        private async Task<bool> HasServerA(string server)
        {
            var value = await _Db.GetStrA($"select Server from dbo.XpServer where Server='{server}'");
            var status = !string.IsNullOrEmpty(value);
            if (!status)
                _Log.Error($"XpServer.Server={server} 不存在。");
            return status;
        }

        private async Task<JObject?> GetServerRowA(string server)
        {
            var row = await _Db.GetRowA($"select * from dbo.XpServer where Server='{server}'");
            var status = (row != null);
            if (!status)
                _Log.Error($"XpServer.Server={server} 不存在。");
            return row;
        }

        /// <summary>
        /// 檢查Authenticator驗証碼
        /// 允許前後各一個 TOTP 時間區間。驗證成功後讀取指定主機的回呼網址，查無網址也回傳空字串。
        /// </summary>
        /// <param name="input">用戶輸入的驗証碼</param>
        /// <param name="server">驗證成功後，用來查詢回呼網址的 XpServer 識別值。</param>
        /// <returns>驗證成功且找到主機設定時回傳回呼網址，否則為空字串。</returns>
        public async Task<string> CheckAuthA(string server, string input)
        {
            //var codeLen = 6; //驗証碼為6個字元
            var bytes = Base32Encoding.ToBytes(_Fun.Config.OtpAuthKey);
            var totp = new Totp(bytes, step: _Fun.Config.OtpAuthSec, totpSize: CodeLen, mode: OtpHashMode.Sha1);

            //允許前後時間漂移 (例如 +/- 1 個時間區間)
            var status = totp.VerifyTotp(input, out _, new VerificationWindow(previous: 1, future: 1));
            if (!status)
                return "";

            //驗証成功, 產生 callback url
            var sql = "select OtpBackUrl from dbo.XpServer where Server=@Server";
            var url = await _Db.GetStrA(sql, ["Server", server]);
            if (string.IsNullOrEmpty(url))
            {
                _Log.Error($"XgOtpSvc.cs CheckAuthA error: Server={server} 不存在。");
                return "";
            }
            else
            {
                return url;
            }
        }

        /// <summary>
        /// Db記錄驗証碼
        /// </summary>
        /// <param name="server"></param>
        /// <param name="userId">可以是email</param>
        /// <param name="otpType"></param>
        /// <param name="okMin">有效時間(分)</param>
        /// <returns></returns>
        private async Task<bool> SaveOtpLogA(string server, string userId, string otpType, string otpCode, int okMin)
        {
            //產生驗証碼
            //var otpCode = GenCode();
            var now = DateTime.Now;
            var expireDt = now.AddMinutes(okMin);

            //不處理舊資料, 直接新增
            var sql = $@"
insert into dbo.XpOtpLog(Server, UserId, OtpType, 
    OtpCode, ExpireDt, IsUsed, Created) 
values(@Server, @UserId, '{otpType}',
    '{otpCode}', '{_Date.GetDtStr(expireDt)}', 0, '{_Date.GetDtStr(now)}')
";
            return (await _Db.ExecSqlA(sql, ["Server", server, "UserId", userId]) == 1);
        }

    } //class
}