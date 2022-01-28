using Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Base.Services
{
    public class _Email
    {
        //get from _Fun.Config.EmailImagePairs
        //null(will get)
        private static List<IdStrDto> _emailImagePairs = null;

        /// <summary>
        /// email to Root(web.config RootEmail field)
        /// </summary>
        /// <param name="msg"></param>
        public static async Task SendRootAsync(string msg)
        {
            //check
            if (_Str.IsEmpty(_Fun.Config.RootEmail))
                return;

            //send
            var email = new EmailDto()
            {
                Subject = _Fun.Config.SystemName + " Info",
                ToUsers = StrToUsers(_Fun.Config.RootEmail),
                Body = msg,
            };
            await SendByMsgAsync(DtoToMsg(email), null, false);
        }

        /// <summary>
        /// mail users string to list string
        /// </summary>
        /// <param name="userStr"></param>
        /// <returns></returns>
        private static List<string> StrToUsers(string userStr)
        {
            return userStr.Replace(';', ',').Split(',').ToList();
        }

        /// <summary>
        /// send one mail by emailDto
        /// </summary>
        /// <param name="email">email model</param>
        /// <param name="smtp">smtp model</param>
        /// <param name="sync">sync send or not, false(web ap), console(true)</param>
        public static async Task SendByDtoAsync(EmailDto email, SmtpDto smtp = null)
        {
            await SendByDtosAsync(new List<EmailDto>() { email }, smtp);
        }

        /// <summary>
        /// send mails by emailDtos asynchronus
        /// </summary>
        /// <param name="emails"></param>
        /// <param name="smtp"></param>
        public static async Task SendByDtosAsync(List<EmailDto> emails, SmtpDto smtp = null)
        {
            //change receiver to tester if need !!
            //var email = new EmailDto();// = null;
            var testMode = _Str.NotEmpty(_Fun.Config.TesterEmail);
            if (testMode)
            {
                var email = emails[0];
                email.ToUsers = StrToUsers(_Fun.Config.TesterEmail);
                email.CcUsers = null;
                emails = new List<EmailDto>() { email };
            }

            //send
            await SendByMsgsAsync(DtosToMsgs(emails, smtp), smtp);
        }

        /*
        //send one mail
        private static void SendByDto2(EmailDto email, SmtpDto smtp = null)
        {
            SendByDtos2(new List<EmailDto>() { email }, smtp);
        }
        */

        //mailMessage add embeded image list
        private static void MsgAddImages(MailMessage msg, List<IdStrDto> images)
        {
            if (images == null || images.Count == 0)
                return;

            foreach (var image in images)
            {
                var imageHtml = $"<html><body><img src=cid:{image.Id}/><br></body></html>";
                var altView = AlternateView.CreateAlternateViewFromString(imageHtml, null, MediaTypeNames.Text.Html);

                var linkSrc = new LinkedResource(image.Str, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                altView.LinkedResources.Add(linkSrc);

                //MailMessage mail = new MailMessage();
                msg.AlternateViews.Add(altView);
            }
        }

        /// <summary>
        /// convert emailDto & smtp to mailMessage object
        /// </summary>
        /// <param name="email"></param>
        /// <param name="smtp"></param>
        /// <returns></returns>
        public static MailMessage DtoToMsg(EmailDto email, SmtpDto smtp = null)
        {
            if (smtp == null)
                smtp = _Fun.Smtp;

            var utf8 = Encoding.GetEncoding("utf-8");
            var sender = new MailAddress(_Str.IsEmpty(smtp.Id) ? smtp.FromEmail : smtp.Id);  //real sender
            var from = new MailAddress(_Str.IsEmpty(smtp.FromEmail) ? smtp.Id : smtp.FromEmail, smtp.FromName, utf8);
            return DtoToMsgByArg(email, utf8, sender, from);
        }

        /// <summary>
        /// convert emailDto & args to mailMessage
        /// </summary>
        /// <param name="email"></param>
        /// <param name="utf8"></param>
        /// <param name="sender"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        private static MailMessage DtoToMsgByArg(EmailDto email, Encoding utf8, MailAddress sender, MailAddress from)
        {
            var msg = new MailMessage()
            {
                SubjectEncoding = utf8,
                BodyEncoding = utf8,   //or will get random code !!
                Subject = email.Subject,
                Body = email.Body,
                IsBodyHtml = true,
                Sender = sender,
                From = from,
            };

            //add image, use cid(better than base64 !!)
            MsgAddImages(msg, email.Images);

            //add attach files
            if (email.Files != null)
            {
                foreach (var file in email.Files)
                {
                    if (File.Exists(file))
                    {
                        var attach = new Attachment(file)
                        {
                            Name = _File.GetFileName(file)  //否則email附檔會出現路徑
                        };
                        msg.Attachments.Add(attach);
                    }
                }
            }

            //add receivers
            if (email.ToUsers != null)
            {
                foreach (var user in email.ToUsers)
                {
                    msg.To.Clear();
                    msg.To.Add(user);
                    //client.Send(msg);   //call sync method here!!
                }
            }
            //if (mail.CcUsers != null)
            //{
            //    foreach (var user in mail.CcUsers)
            //        msg.CC.Add(user);
            //}
            return msg;
        }

        /// <summary>
        /// convert emailDtos to mailMessages
        /// </summary>
        /// <param name="emails"></param>
        /// <param name="smtp"></param>
        /// <returns></returns>
        public static List<MailMessage> DtosToMsgs(List<EmailDto> emails, SmtpDto smtp = null)
        {
            if (smtp == null)
                smtp = _Fun.Smtp;

            var msgs = new List<MailMessage>();
            var utf8 = Encoding.GetEncoding("utf-8");
            var sender = new MailAddress(_Str.IsEmpty(smtp.Id) ? smtp.FromEmail : smtp.Id);  //real sender
            var from = new MailAddress(_Str.IsEmpty(smtp.FromEmail) ? smtp.Id : smtp.FromEmail, smtp.FromName, utf8);
            foreach (var email in emails)
                msgs.Add(DtoToMsgByArg(email, utf8, sender, from));

            return msgs;
        }

        /*
        /// <summary>
        /// send by MailMessage
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="smtp"></param>
        public static void SendByMsg(MailMessage msg, SmtpDto smtp = null)
        {
            SendByMsgs(new List<MailMessage>() { msg }, smtp);
        }

        /// <summary>
        /// send mails async, send one by one for security reason
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="smtp"></param>
        public static void SendByMsgs(List<MailMessage> msgs, SmtpDto smtp = null)
        {
            //async send email
            var thread = new Thread(delegate ()
            {
                SendByMsgsAsync(msgs, smtp);
            });
            thread.Start();
        }
        */

        /// <summary>
        /// send email
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="smtp"></param>
        /// <param name="sendImage"></param>
        /// <returns></returns>
        public static async Task SendByMsgAsync(MailMessage msg, SmtpDto smtp = null, bool sendImage = true)
        {
            await SendByMsgsAsync(new List<MailMessage>() { msg }, smtp, sendImage);
        }

        /// <summary>
        /// send mails, send one by one for security reason
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="smtp"></param>
        public static async Task SendByMsgsAsync(List<MailMessage> msgs, SmtpDto smtp = null, bool sendImage = true)
        {
            //check
            //error = ""; //initial
            if (smtp == null)
                smtp = _Fun.Smtp;

            //set _emailImagePairs if need
            if (sendImage && _emailImagePairs == null)
            { 
                _emailImagePairs = new List<IdStrDto>();
                if (!_Str.IsEmpty(_Fun.Config.EmailImagePairs))
                {
                    var values = _Fun.Config.EmailImagePairs.Split(',');
                    for (var i = 0; i < values.Length; i += 2)
                    {
                        _emailImagePairs.Add(new IdStrDto()
                        {
                            Id = values[i],
                            Str = values[i + 1],
                        });
                    }
                }
            }

            //send email
            try
            {
                //set smtp
                var client = new SmtpClient()
                {
                    Host = smtp.Host,
                    Port = smtp.Port,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new System.Net.NetworkCredential(smtp.Id, smtp.Pwd),
                    EnableSsl = smtp.Ssl,
                    Timeout = 30000,
                };

                //add images & send
                foreach (var msg in msgs)
                {
                    if (sendImage)
                        MsgAddImages(msg, _emailImagePairs);
                    await client.SendMailAsync(msg);
                }

                client.Dispose();
            }
            catch (Exception ex)
            {
                //var error = "_Email.cs SendByMsgsSync() failed: " + ex.InnerException.Message;
                var error = _Str.NotEmpty(ex.Message) ? ex.Message :
                    (ex.InnerException != null) ? ex.InnerException.Message :
                    "Email Error !!";
                await _Log.ErrorAsync("_Email.cs SendByMsgsAsync() failed: " + error, false);    //false here, not mailRoot, or endless roop !!
            }
        }

        #region remark code
        /*
        private static void AddImage(EmailDto email, string imageId, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                _Log.Error("_Email.cs AddImage() failed, no image(" + imagePath +")");
                return;
            }

            if (email.ImageIds == null)
            {
                email.ImageIds = new List<string>();
                email.ImagePaths = new List<string>();
            }

            email.ImageIds.Add(imageId);
            email.ImagePaths.Add(imagePath);
        }

        public static bool sendMailsByFile(string ps_template, JsonObject p_tplDw, string ps_subject, JsonArray pa_dw)
        {
            return sendMailsByFile(ps_template, p_tplDw, ps_subject, pa_dw, "", "");
        }
        */

        /// <summary>
        /// 使用 web 主機上的 smtp server 傳送多封郵件, template 和 body 的內容會因人而易
        /// </summary>
        /// <param name="ps_template">_template 目錄下的 Mail 範本檔案名稱, 如果沒有副檔名則使用 .mail</param>
        /// <param name="ps_subject">主旨</param>
        /// <param name="p_tplDw">用來置換 mail template 的欄位內容</param>
        /// <param name="pa_dw">用來置換 mail body 的 JsonArray, 裡面必須包含 ps_fieldTo 和 ps_fieldCC 的欄位</param>
        /// <param name="ps_fieldTo">收件者的欄位名稱, 如果空白則使用預設(Fun.csToUsers)</param>
        /// <param name="ps_fieldCC">cc的欄位名稱, 如果空白則使用預設(Fun.csMailCC)</param>
        /// <returns>錯誤訊息 if any, 如果有信箱傳送失敗, 則會傳回這些信箱的清單, 並且以逗號分隔.</returns>
        //public static string sendMails(string ps_template, string ps_subject, JsonArray pa_dw, JsonObject p_tplDw, string ps_fieldTo, string ps_fieldCC)
        /*
        public static bool sendMailsByFile(string ps_template, JsonObject p_tplDw, string ps_subject, JsonArray pa_dw, string ps_fieldTo, string ps_fieldCC)
        {
            //check
            if (pa_dw == null || pa_dw.Length == 0)
                return true;


            //=== mail template file to string and replace with p_tplDw begin ===
            //string ts_template = p_mail["template"].ToString();
            if (ps_template.IndexOf(".") < 0)
                ps_template += ".mail";

            //string ts_file = Fun.sDirTemplate + ps_template;
            string ts_file = _iFile.getLangFile(ps_template);
            if (ts_file == "")
            {
                _log.logError("Mail Template does not Exist! (" + ps_template + ")");
                return false;
            }


            //get mail content
            string ts_body = _iFile.fileToStr(ts_file);
            if (p_tplDw != null)
                ts_body = _string.fillStrByJson(ts_body, p_tplDw, false);   //先置換 template file 內容 with p_tplDw

            //=== end ===


            //adjust
            if (ps_fieldTo == "")
                ps_fieldTo = _fun._aToUsers;

            if (ps_fieldCC == "")
                ps_fieldCC = _fun._aCcUser;


            //send mails
            ps_subject = _fun.config("sysName") + "--" + ps_subject;
            //string ts_encode = "BIG5";
            string ts_encode = "utf-8";
            string ts_box;
            string ts_error = "";
            SmtpClient t_smtp = new SmtpClient();
            MailMessage t_mail = null;
            JsonObject t_dw;
            for (int i = 0; i < pa_dw.Length; i++)
            {
                t_dw = (JsonObject)pa_dw[i];
                t_mail = new MailMessage();
                t_mail.SubjectEncoding = System.Text.Encoding.GetEncoding(ts_encode);
                t_mail.Subject = ps_subject;
                t_mail.BodyEncoding = System.Text.Encoding.GetEncoding(ts_encode);   //否則會出現亂碼 !!
                t_mail.Body = _string.fillStrByJson(ts_body, t_dw, true);
                t_mail.IsBodyHtml = true;


                //ts_box = t_dw[ps_fieldTo].ToString().Replace(';', ',');    //2個以上的信箱
                //t_mail.To.Add(ts_box);
                //if (t_dw[ps_fieldCC] != null)
                //{
                //    t_mail.CC.Add(t_dw[ps_fieldCC].ToString().Replace(';', ','));
                //    ts_box += "," + t_dw[ps_fieldCC].ToString();
                //}

                string ts_cc;
                ts_box = _mail.jsonToMailBoxes(t_dw[ps_fieldTo].ToString());
                t_mail.To.Add(ts_box);
                if (t_dw[ps_fieldCC] != null)
                {
                    ts_cc = t_dw[ps_fieldCC].ToString();
                    t_mail.CC.Add(_mail.jsonToMailBoxes(ts_cc));
                    ts_box += "," + ts_cc;
                }

                try
                {
                    t_smtp.Send(t_mail);
                    return true;
                }
                catch
                {
                    ts_error += ts_box + ",";
                    _log.logError("Mail failed, Subject: " + ps_subject + ", toUser: " + ts_box);
                    return false;
                }
            }

            //temp
            return true;
            //return ts_error;
        }
        */

        /// <summary>
        /// 使用 mail template file 來傳送郵件.
        /// </summary>
        /// <param name="ps_template">Mail template file name</param>
        /// <param name="p_dw">用來置換 mail body 的 JsonObject</param>
        /// <returns>錯誤訊息 if any.</returns>
        /// 
        /*
        public static bool sendMailByFile(string ps_template, JsonObject p_dw)
        {
            JsonArray ta_dw = new JsonArray();
            ta_dw.Add(p_dw);
            return sendMailByFile(ps_template, ta_dw);
        }

        public static bool sendMailByFile(string ps_template, JsonArray pa_dw)
        {
            JsonObject t_mail = _iFile.mailFileToJson(ps_template);
            if (t_mail == null)
            {
                _log.logError("Fun.mailFileToJson() failed, (" + ps_template + ")");
                return false;
            }
            else
                return sendMailByJson(t_mail, pa_dw, null);

        }
        */

        /// <summary>
        /// 使用 JsonObject 資料來傳送郵件.
        /// </summary>
        /// <param name="p_mail">Mail 的資料, 包含欄位: template, Fun.csToUsers, Fun.csMailCC, subject</param>
        /// <param name="p_dw">用來置換 mail body 的 JsonObject</param>
        /// <returns>錯誤訊息 if any.</returns>
        /*
        public static bool sendMailByJson(JsonObject p_mail, JsonObject p_dw, SmtpClient p_smtp)
        {
            JsonArray ta_dw = new JsonArray();
            ta_dw.Add(p_dw);
            return sendMailByJson(p_mail, ta_dw, p_smtp);
        }
        */

        /// <summary>
        /// 使用 JsonObject 資料來傳送郵件.
        /// </summary>
        /// <param name="p_mail">Mail 的資料, 包含欄位: //template, subject, body, Fun.csToUsers, Fun.csCCUsers</param>
        /// <param name="pa_dw">用來置換 mail body 的 JsonArray</param>
        /// <returns>錯誤訊息 if any.</returns>
        //public static string sendMailByJson(JsonObject p_mail, JsonObject p_body)
        /*
        public static bool sendMailByJson(JsonObject p_mail, JsonArray pa_dw, string ps_files, SmtpClient p_smtp)
        {

            //send Mail
            //string ts_to = (p_mail[Fun.csToUsers] != null) ? Fun.jsonToMailBoxes((JsonObject)p_mail[Fun.csToUsers], pa_dw) : "";
            //string ts_cc = (p_mail[Fun.csCCUsers] != null) ? Fun.jsonToMailBoxes((JsonObject)p_mail[Fun.csCCUsers], pa_dw) : "";
            string ts_to = (p_mail[_fun._aToUsers] != null) ? _mail.jsonToMailBoxes(p_mail[_fun._aToUsers].ToString(), pa_dw) : "";
            string ts_cc = (p_mail[_fun._aCcUser] != null) ? _mail.jsonToMailBoxes(p_mail[_fun._aCcUser].ToString(), pa_dw) : "";

            return _mail.sendMail(p_mail["subject"].ToString(),
                _string.fillStrByJsons(p_mail["body"].ToString(), pa_dw, true),
                ts_to, ts_cc, ps_files, p_smtp);

        }
        */

        /* 暫時取消 2010-12-20
        /// <summary>
        /// 傳送郵件給管理者.(web.config 裡的 adminMail 變數).
        /// </summary>
        /// <param name="p_mail">mail JsonObject 欄位</param>
        /// <param name="p_body">訊息內容 JsonObject 資料</param>
        /// <returns>錯誤訊息 if any.</returns>
        public static string sendAdmin(JsonObject p_mail, JsonObject p_body)
        {
            JsonObject t_mail = new JsonObject();
            JsonObject t_to = new JsonObject();
            t_to["box"] = config("adminMail");
            t_mail[Fun.csToUsers] = t_to;
            t_mail["subject"] = p_mail["subject"].ToString();
            t_mail["template"] = p_mail["template"].ToString();

            return Fun.sendMailByJson(t_mail, p_body);
        }
        */

        /*
        public static string jsonToMailBoxes(string ps_users)
        {
            return jsonToMailBoxes(ps_users, null);
        }

        /// <summary>
        /// 傳回郵件信箱清單, 以逗號分隔, 如果資料不正確會通知 Root.
        /// </summary>
        /// <param name="p_box">要解碼的信箱資料 JsonObject, 包含: user, dept, role, dm 變數</param>
        /// <param name="pa_dw">儲存欄位變數的 JsonArray 資料</param>
        /// <returns>信箱字串清單, 以逗號分隔</returns>
        //public static string jsonToMailBoxes(JsonObject p_box, JsonArray pa_dw)
        public static string jsonToMailBoxes(string ps_users, List<JObject> pa_dw)
        {

            string ts_box = "";
            string ts_comm = "";
            JObject t_sql = new JObject();    //for call Fun2.getSql()
            t_sql["result"] = "MailBox";
            t_sql["type"] = "user";     //必須為此名稱 !!
            string ts_db = "";
            string ts_sql;
            string ts_users;
            string[] tas_user = ps_users.Split(',');
            for (int i = 0; i < tas_user.Length; i++)
            {
                if (tas_user[i].IndexOf("@") >= 0)
                {
                    //just mail box
                    ts_box += ts_comm + tas_user[i];
                    ts_comm = ",";
                }
                else
                {
                    //t_sql["type"] = ts_key;
                    ts_users = _Str2.fillStrByJsons(tas_user[i], pa_dw, false);   //從 pa_dw 設定信箱資料
                    if (ts_users.IndexOf("@") >= 0)     //如果得到 email address, 則直接加入清單
                    {
                        //just mail box
                        ts_box += ts_comm + ts_users;
                        ts_comm = ",";
                    }
                    else  //如果不是得到 email address, 則利 Fun2._getSql() 
                    {
                        t_sql["queryFids"] = ts_users;
                        //temp change
                        //ts_sql = _fun2.getSql(t_sql, ref ts_db);
                        ts_sql = "";
                        //JArray ta_box = _Db.readRowsByDb(ts_db, ts_sql);
                        JArray ta_box = new Db(ts_db).GetRows(ts_sql);

                        //add to ts_box
                        if (ta_box != null)
                        {
                            JObject t_box;
                            for (int j = 0; j < ta_box.Count; j++)
                            {
                                t_box = (JObject)ta_box[j];
                                ts_box += ts_comm + t_box["email"].ToString();
                                ts_comm = ",";
                            }
                        }
                    }
                }

            }

            return ts_box;
        }
        */
        #endregion

    }//class
}