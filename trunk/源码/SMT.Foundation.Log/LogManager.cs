using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using System.Net.Mail;
using System.Net;
namespace SMT.Foundation.Log
{
    public class LogManager
    {
        private List<ILogger> Loggers;
        private bool aleadySendEmail=false;

        private static string strAssemblyName = "SMT.Foundation.Log";

        public LogManager()
        {
        }

        private List<ILogger> InitLogger()
        {
            string[] strLoggers = LogConfig.Instance.Loggers;
            Loggers = new List<ILogger>();
            string strClassName;

            Assembly a = Assembly.Load(strAssemblyName);

            //if (System.Web.HttpContext.Current.Cache["LogAssembly"] != null)
            //{
            //    a = (Assembly)System.Web.HttpContext.Current.Cache["LogAssembly"];
            //}
            //else
            //{
            //    a = Assembly.Load(strAssemblyName);
            //    System.Web.HttpContext.Current.Cache.Insert("LogAssembly", a);
            //}

            foreach (string slog in strLoggers)
            {
                strClassName = strAssemblyName +"."+slog;
                ILogger iLogger = (ILogger)a.CreateInstance(strClassName);
                Loggers.Add(iLogger);
            }
            return Loggers;
        }

        public void WriteLog(ErrorLog message)
        {
            //Loggers = new List<ILogger>();
            //  Loggers = InitLogger();
            foreach (ILogger l in InitLogger())
            {
                l.Write(message);
            }
            if (LogConfig.Instance.SendErrorEmail == "Yes")
            {
                if (aleadySendEmail == false)
                {
                    SendMail(message);
                    aleadySendEmail = true;
                }
            }
        }

        public void SendMail(ErrorLog message)
        {
            try
            {
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(LogConfig.Instance.SMTPServer);
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(LogConfig.Instance.SMTPLogin, LogConfig.Instance.SMTPPassword);//CredentialCache.DefaultNetworkCredentials;//// CredentialCache.DefaultNetworkCredentials;// new ;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                MailAddress from = new MailAddress(LogConfig.Instance.ErrorEmailSender);
                MailAddress to = new MailAddress(LogConfig.Instance.ErrorEmailReceiver);
                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage(from, to);
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.IsBodyHtml = true;
                //mailMessage.Subject = "Auto Generated Error Message – Version " + message.AWBVersion;
                //mail body
                StringBuilder body = new StringBuilder();
                body.Append("<html ><head><style type='text/css' >td{font-size:12px;bold:true;}</style></head><body><table border=1 width=600px align=center> <tr><td colspan=4  >Error Log</td></tr>");
                body.Append("<tr><td>Client Host Name:</td><td colspan='3'>" + message.ClientHostName + "</td></tr>");
                body.Append("<tr><td>Client Host Address:</td><td  colspan='3'>" + message.ClientHostAddress + "</td></tr>");

                body.Append("<tr><td>Computer Name:</td><td colspan='3'>" + message.ComputerName + "</td></tr>");
                body.Append("<tr><td>Created On:</td><td  colspan='3'>" + message.CreatedOn + "</td></tr>");
                body.Append("<tr><td colspan=4>Login Information</td></tr>");
                body.Append("<tr><td> Login User Name:</td><td>" + message.LoginUser + "</td><td>Login User ID:</td><td>"
                                    + message.LoginUserId + "</td></tr>");
                //body.Append("<tr><td>Organization ID:</td><td colspan=3>" + message.OrganizationID + "</td></tr>");
                body.Append("<tr><td colspan=4>Exception Details</td></tr>");
                body.Append("<tr><td>Error URL:</td><td  colspan='3'>" + message.ErrorURL + "</td></tr>");
                body.Append("<tr><td>Error Stack Trace:</td><td  colspan='3'>" + message.ErrorMessage.ToString() + "</td></tr>");
                body.Append("<tr><td colspan=4>Server Information</td></tr>");
                body.Append("<tr><td>Server OS:</td><td  colspan='3'>" + message.ServerOS + "</td></tr>");
                body.Append("<tr><td>Server .Net Runtime:</td><td  colspan='3'>" + message.ServerNetRuntime + "</td></tr>");
                //body.Append("<tr><td>AWB Assemblies Version:</td><td  colspan='3'>" + message.AWBAssembliesVersion + "</td></tr>");
                body.Append("<tr><td colspan=4>Client Information</td></tr>");
                body.Append("<tr><td>Client OS:</td><td  colspan='3'>" + message.ClientOS + "</td></tr>");
                body.Append("<tr><td>Client OS Language:</td><td  colspan='3'>" + message.ClientOSLanguage + "</td></tr>");
                body.Append("<tr><td>Client Browser:</td><td  colspan='3'>" + message.ClientBrowser + "</td></tr>");
                body.Append("<tr><td>Client Browser Language:</td><td  colspan='3'>" + message.ClientBrowserLanguage + "</td></tr>");
                body.Append("<tr><td>Client .Net Runtime:</td><td  colspan='3'>" + message.ClientNetRuntime + "</td></tr>");
                body.Append("<tr><td>Client JavaScript Support:</td><td  colspan='3'>" + (message.ClientJavaScriptSupport == true ? "Yes" : "No") + "</td></tr>");
                body.Append("</table></body></html>");
                mailMessage.Body = body.ToString();
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Tracer.Error(ex.ToString());
            }
        }
    }

}
