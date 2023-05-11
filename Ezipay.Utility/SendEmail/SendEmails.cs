using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.SendEmailViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace Ezipay.Utility.SendEmail
{
    public class SendEmails : ISendEmails
    {

        public string SendEmail(EmailModel emailModel)
        {

            string retVal = "";
            try
            {

                MailMessage message;
                message = new MailMessage();
                message.To.Add(emailModel.TO);
                message.Bcc.Add(CommonSetting.EmailFrom);
                message.Subject = emailModel.Subject;
                message.Body = emailModel.Body;
                message.Priority = MailPriority.Normal;
                message.From = new MailAddress(CommonSetting.EmailFrom);

                var client = new SmtpClient
                {
                    Host = CommonSetting.Host,
                    Port = Convert.ToInt32(CommonSetting.Port), // Port
                    EnableSsl = true,
                    Credentials = new NetworkCredential(CommonSetting.Username, CommonSetting.Password)
                };
                message.IsBodyHtml = true;
                bool mailsend = false;
                try
                {
                    client.Send(message);
                    WriteTextToFile(emailModel.TO + "-Sent Successful" + "Time -" + DateTime.UtcNow.ToString());
                    mailsend = true;
                }
                catch (Exception ex)
                {
                    var exceptionMessage = JsonConvert.SerializeObject(ex);
                    WriteTextToFile(emailModel.TO + "-1:" + exceptionMessage);
                }
                retVal = "Email sent successfully";
            }
            catch (Exception ex)
            {
                var exceptionMessage = JsonConvert.SerializeObject(ex);
                WriteTextToFile(emailModel.TO + "-2" + exceptionMessage);
                retVal = ex.Message;// +"\n\n" + ex.StackTrace;
            }
            return retVal;
        }

        public string SendEmailTxn(EmailModel emailModel, MemoryStream memoryStream)
        {
            string retVal = "";
            try
            {
                MailMessage message;
                message = new MailMessage();
                message.To.Add(emailModel.TO);
                message.Bcc.Add(CommonSetting.EmailFrom);
                message.Subject = emailModel.Subject;
                message.Body = emailModel.Body;
                message.Priority = MailPriority.Normal;
                message.From = new MailAddress(CommonSetting.EmailFrom);


                message.Attachments.Add(new Attachment(memoryStream, "LastMonthTransactionStatement.pdf", "application/pdf"));

                var client = new SmtpClient
                {
                    Host = CommonSetting.Host,
                    Port = Convert.ToInt32(CommonSetting.Port), // Port
                    EnableSsl = true,
                    Credentials = new NetworkCredential(CommonSetting.Username, CommonSetting.Password)
                };
                message.IsBodyHtml = true;
                bool mailsend = false;
                try
                {
                    client.Send(message);
                    WriteTextToFile(emailModel.TO + "SendEmailTxn-Sent Successful" + "Time -" + DateTime.UtcNow.ToString());
                    mailsend = true;
                }
                catch (Exception ex)
                {
                    var exceptionMessage = JsonConvert.SerializeObject(ex);
                    WriteTextToFile(emailModel.TO + "-1:" + exceptionMessage);
                }
                retVal = "Email sent successfully";
            }
            catch (Exception ex)
            {
                var exceptionMessage = JsonConvert.SerializeObject(ex);
                WriteTextToFile(emailModel.TO + "-2" + exceptionMessage);
                retVal = ex.Message;// +"\n\n" + ex.StackTrace;
            }
            return retVal;
        }


        public string SendEmailUsingDifferentCard(EmailModel emailModel)
        {

            string retVal = "";
            try
            {
                List<string> addyList = new List<string>();

                addyList.Add("sumit.sharma@aituniversal.com");
                addyList.Add("rizwan.ramzan@aituniversal.com");
                addyList.Add("mishi.sharma@aituniversal.com");
                addyList.Add("rajnesh.kumar@aituniversal.com");

                MailMessage message;
                message = new MailMessage();
                message.To.Add(emailModel.TO);
                foreach (string address in addyList)
                {
                    MailAddress to = new MailAddress(address);
                    message.CC.Add(to);
                }
                message.Subject = emailModel.Subject;
                message.Body = emailModel.Body;
                message.Priority = MailPriority.Normal;
                message.From = new MailAddress(CommonSetting.EmailFrom);

                var client = new SmtpClient
                {
                    Host = CommonSetting.Host,
                    Port = Convert.ToInt32(CommonSetting.Port), // Port
                    EnableSsl = true,
                    Credentials = new NetworkCredential(CommonSetting.Username, CommonSetting.Password)
                };
                message.IsBodyHtml = true;
                bool mailsend = false;
                try
                {
                    client.Send(message);
                    WriteTextToFile(emailModel.TO + "-Sent Successful" + "Time -" + DateTime.UtcNow.ToString());
                    mailsend = true;
                }
                catch (Exception ex)
                {
                    var exceptionMessage = JsonConvert.SerializeObject(ex);
                    WriteTextToFile(emailModel.TO + "-1:" + exceptionMessage);
                }
                retVal = "Email sent successfully";
            }
            catch (Exception ex)
            {
                var exceptionMessage = JsonConvert.SerializeObject(ex);
                WriteTextToFile(emailModel.TO + "-2" + exceptionMessage);
                retVal = ex.Message;// +"\n\n" + ex.StackTrace;
            }
            return retVal;
        }
        public string ReadEmailformats(string Filename)
        {
            StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/HtmlTemplates/" + Filename + ""));
            string readFile = reader.ReadToEnd();
            string strEmailBody = "";
            strEmailBody = readFile;
            return strEmailBody.ToString();
        }

        public string ReadTemplateEmailformats(string Filename)
        {
            StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/HtmlTemplates/" + Filename + ""));
            string readFile = reader.ReadToEnd();
            string strEmailBody = "";
            strEmailBody = readFile;
            return strEmailBody.ToString();
        }


        public void WriteTextToFile(string message)
        {
            try
            {
                string filePath = (HttpContext.Current.Server.MapPath("~/Logs/EmailLogs.txt"));
                using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                    stream.Write(newline, 0, newline.Length);
                    Byte[] info = new UTF8Encoding(true).GetBytes(message);
                    stream.Write(info, 0, info.Length);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                ex.Message.ErrorLog(ex.Message, "SendEmails.cs", "WriteTextToFile");
            }
        }
    }
}
