using Base.Interfaces;
using Base.Services;
using Flurl;
using Flurl.Http;
using SmsTaiwan.Models;
using System.Text;
using System.Web;

namespace SmsTaiwan
{
    //台哥大使用https傳送簡訊, 不必安裝套件
    public class SmsTwSvc : ISmsSvc
    {
        const string UrlShortMsg = "http://bizsms.taiwanmobile.com:18994";
        const string UrlLongMsg = "http://bizsms.taiwanmobile.com:18995";
        const string UrlBack = "http://210.61.226.12:8080/domSmsTwm/PhaseTwo";  //todo, 是否需要?

        private Encoding encodeBig5 = Encoding.GetEncoding("big5");

        public async Task<bool> SendA(string phone, string msg)
        {
            //對簡訊內容進行重新處理
            var msgLen = msg.Length;
            string smsUrl;
            if (msgLen <= 70)
                smsUrl = UrlShortMsg;
            else if (msgLen <= 335)
                smsUrl = UrlLongMsg;
            else
            {
                _Log.Error("簡訊內容字數異常: " + msg);
                return false;
            }

            //台哥大傳送簡訊, todo: 是否需要UrlBack?
            var sms = GetSms(1, msg); //todo: sid=1
            var result = await smsUrl
                .AppendPathSegment("send.cgi")
                .SetQueryParams(new
                {
                    username = _Fun.Config.SmsAccount,
                    password = _Fun.Config.SmsPwd,
                    rateplan = "A",
                    srcaddr = _Fun.Config.SmsFromPhone,
                    dstaddr = phone,
                    sms.encoding,
                    //dto3.vldtime, //default 8 hours
                    response = UrlBack,
                })
                .SetQueryParam("smbody", sms.smbody, true)
                .GetAsync()
                .ReceiveString();

            /* todo: 寫入DB log: XpSmsLog
            //回傳的資料取回
            var str = result.Split('\n');
            if (str.Length == 5)
            {
                dto3.msgid = str[0].Remove(0, 6);
                dto3.statuscode = int.Parse(str[1].Remove(0, 11));
                dto3.statusstr = str[2].Remove(0, 10);
                dto3.point = int.Parse(str[3].Remove(0, 6));    //扣除點數
            }
            else
            {
                dto3.msgid = str[0].Remove(0, 6);
                dto3.statuscode = int.Parse(str[1].Remove(0, 11));
                dto3.statusstr = str[2].Remove(0, 10);
            }
            */
            return true;
        }

        private string UrlEncode(string value)
        {
            return HttpUtility.UrlEncode(value, encodeBig5);
        }

        //重新編譯簡訊字串
        //參考 NewsLetterSystem HomeRep.cs
        private SmsDto GetSms(int sid, string msg)
        {
            //const string Big5 = "big5";
            const string LBig5 = "LBIG5";
            const string PreSms = "%05%00%03%"; //簡訊長度前置3個字元

            //var encodeBig5 = Encoding.GetEncoding(Big5);

            //長簡訊在手機中組合的編號，值為00~FF，需隨機產生，同一則長簡訊編碼需相同。
            //用主KEY作為隨機數字 對255取餘數 就不會隨機
            var msgLen = msg.Length;
            //var sid = dto.sid % 255;
            var num = Convert.ToString(sid % 255, 16);
            if (num.Length < 2)
                num = "0" + num;

            //根據不同的內文字數進行拆分簡訊 並且轉16位元文字
            var preStr = PreSms + num;
            var sms = new SmsDto();
            if (msgLen <= 70)
            {
                sms.encoding = "BIG5";
                sms.smbody = UrlEncode(msg);
            }
            else if (msgLen > 70 && msgLen <= 134)
            {
                //67 134 201 268 335
                var code1 = preStr + "%02%01" + UrlEncode(msg.Substring(0, 67));
                var code2 = preStr + "%02%02" + UrlEncode(msg.Substring(67));
                sms.smbody = code1 + code2;
                sms.encoding = LBig5;
            }
            else if (msgLen > 134 && msgLen <= 201)
            {
                //67 134 201 268 335
                var code1 = preStr + "%03%01" + UrlEncode(msg.Substring(0, 67));
                var code2 = preStr + "%03%02" + UrlEncode(msg.Substring(67, 67));
                var code3 = preStr + "%03%03" + UrlEncode(msg.Substring(134));
                sms.smbody = code1 + code2 + code3;
                sms.encoding = LBig5;
            }
            else if (msgLen > 201 && msgLen <= 268)
            {
                //67 134 201 268 335
                var code1 = preStr + "%04%01" + UrlEncode(msg.Substring(0, 67));
                var code2 = preStr + "%04%02" + UrlEncode(msg.Substring(67, 67));
                var code3 = preStr + "%04%03" + UrlEncode(msg.Substring(134, 67));
                var code4 = preStr + "%04%04" + UrlEncode(msg.Substring(201));
                sms.smbody = code1 + code2 + code3 + code4;
                sms.encoding = LBig5;
            }
            else if (msgLen > 268 && msgLen <= 335)
            {
                //67 134 201 268 335
                var code1 = preStr + "%05%01" + UrlEncode(msg.Substring(0, 67));
                var code2 = preStr + "%05%02" + UrlEncode(msg.Substring(67, 67));
                var code3 = preStr + "%05%03" + UrlEncode(msg.Substring(134, 67));
                var code4 = preStr + "%05%04" + UrlEncode(msg.Substring(201));
                var code5 = preStr + "%05%05" + UrlEncode(msg.Substring(268));
                sms.smbody = code1 + code2 + code3 + code4 + code5;
                sms.encoding = LBig5;
            }
            return sms;
        }

    }
}
