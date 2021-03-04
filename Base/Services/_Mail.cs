using Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Services
{
    public class _Mail
    {
        /// <summary>
        /// email to Root(web.config RootMail field)
        /// </summary>
        /// <param name="msg"></param>
        public static void SendRoot(string msg)
        {
            //check
            if (string.IsNullOrEmpty(_Fun.Config.RootMail))
                return;

            //send
            SendMail(new MailDto()
            {
                Subject = _Fun.Config.SystemName + " Error",
                ToUsers = StrToUsers(_Fun.Config.RootMail),
                Body = msg,
            });
        }

        /// <summary>
        /// mail users string to list string
        /// </summary>
        /// <param name="userStr"></param>
        /// <returns></returns>
        public static List<string> StrToUsers(string userStr)
        {
            return userStr.Replace(';', ',').Split(',').ToList();
        }

        /// <summary>
        /// send one mail
        /// </summary>
        /// <param name="mail">email model</param>
        /// <param name="smtp">smtp model</param>
        /// <param name="sync">sync send or not, false(web ap), console(true)</param>
        public static void Send(MailDto mail, SmtpDto smtp = null, bool sync = false)
        {
            Sends(new List<MailDto>() { mail }, smtp, sync);
        }

        //send many mails
        public static void Sends(List<MailDto> mails, SmtpDto smtp = null, bool sync = false)
        {
            //change receiver to tester if need !!
            var mail = new MailDto();// = null;
            var testMode = !string.IsNullOrEmpty(_Fun.Config.TesterMail);
            if (testMode)
            {
                mail = mails[0];
                mail.ToUsers = StrToUsers(_Fun.Config.TesterMail);
                mail.CcUsers = null;
            }

            //sync = true;    //temp add
            if (sync)
            {
                if (testMode)
                    SendMail(mail, smtp);
                else
                    SendMails(mails, smtp);
            }
            else
            {
                if (testMode)
                {                    
                    var thread = new Thread(delegate ()
                    {
                        SendMail(mail, smtp);
                    });
                    thread.Start();
                }
                else
                {
                    /*
                    Task.Factory.StartNew(() =>
                    {
                        SendMails(mails, smtp);
                    });
                    */
                    var thread = new Thread(delegate ()
                    {
                        SendMails(mails, smtp);                        
                    });
                    thread.Start();
                }
            }
        }

        //send one mail
        private static void SendMail(MailDto mail, SmtpDto smtp = null)
        {
            SendMails(new List<MailDto>() { mail }, smtp);
        }

        //send mails, send one by one for security reason
        private static void SendMails(List<MailDto> mails, SmtpDto smtp = null)
        {
            //check
            if (smtp == null)
                smtp = _Fun.Smtp;

            //var smtp = smtp0.Value;
            //send
            try
            {
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

                //string encode = "utf-8";
                var utf8 = Encoding.GetEncoding("utf-8");
                var sender = new MailAddress(_Str.IsEmpty(smtp.Id) ? smtp.FromEmail : smtp.Id);  //real sender
                var from = new MailAddress(_Str.IsEmpty(smtp.FromEmail) ? smtp.Id : smtp.FromEmail, smtp.FromName, utf8);
                foreach (var mail in mails)
                {
                    var msg = new MailMessage()
                    {
                        SubjectEncoding = utf8,
                        BodyEncoding = utf8,   //or will get random code !!
                        Subject = mail.Subject,
                        Body = mail.Body,
                        IsBodyHtml = true,
                        Sender = sender,
                        From = from,
                        //Sender = new MailAddress(_Str.IsEmpty(smtp.Id) ? smtp.FromEmail : smtp.Id),  //real sender
                        //From = new MailAddress(_Str.IsEmpty(smtp.FromEmail) ? smtp.Id : smtp.FromEmail, smtp.FromName, utf8),
                    };

                    //add image, use cid(better than base64 !!)
                    if (mail.ImageIds != null && mail.ImageIds.Count > 0)
                    {
                        for (var i = 0; i < mail.ImageIds.Count; i++)
                        {
                            var imageHtml = string.Format("<html><body><img src=cid:{0}/><br></body></html>", mail.ImageIds[i]);
                            var altView = AlternateView.CreateAlternateViewFromString(imageHtml, null, MediaTypeNames.Text.Html);

                            var linkSrc = new LinkedResource(mail.ImagePaths[i], MediaTypeNames.Image.Jpeg);
                            linkSrc.ContentId = Guid.NewGuid().ToString();
                            altView.LinkedResources.Add(linkSrc);

                            //MailMessage mail = new MailMessage();
                            msg.AlternateViews.Add(altView);
                        }
                    }

                    //attach files
                    if (mail.Files != null)
                    {
                        foreach (var file in mail.Files)
                        {
                            if (File.Exists(file))
                            {
                                var attach = new Attachment(file);
                                attach.Name = _File.GetFileName(file);  //否則email附檔會出現路徑
                                msg.Attachments.Add(attach);
                            }
                        }
                    }

                    if (mail.ToUsers != null)
                    {
                        foreach (var user in mail.ToUsers)
                        {
                            msg.To.Clear();
                            msg.To.Add(user);
                            client.Send(msg);   //call sync method here!!
                        }
                    }
                    /*
                    if (mail.CcUsers != null)
                    {
                        foreach (var user in mail.CcUsers)
                            msg.CC.Add(user);
                    }
                    */

                }//for

                client.Dispose();
            }
            catch (Exception ex)
            {
                var error = "Mail failed: " + ex.Message;
                _Log.Error(error, false);    //false here!!
            }
        }

        private static void AddImage(MailDto mail, string imageId, string imagePath)
        {
            if (!File.Exists(imagePath))
                return;

            if (mail.ImageIds == null)
            {
                mail.ImageIds = new List<string>();
                mail.ImagePaths = new List<string>();
            }

            mail.ImageIds.Add(imageId);
            mail.ImagePaths.Add(imagePath);
        }

        #region remark code
        /*
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