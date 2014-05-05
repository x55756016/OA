using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Threading;
//using WSL.Framework.Core.Data.DataAccess;
namespace SMT.Foundation.Log
{
    public class ErrorLog
    {
        public string ThreadId;
        public string ErrorLogID;
        //public string AWBVersion;
        //public string AWBAssembliesVersion;
        public string ClientHostName;
        public string ClientHostAddress;
        public string ClientBrowser;
        public string ClientBrowserLanguage;
        public bool ClientJavaScriptSupport;
        public string ClientNetRuntime;
        public string ClientOS;
        public string ClientOSLanguage;
        public string ServerOS;
        public string ServerNetRuntime;
        public string ComputerName;
        public string ErrorExecutionTrace;
        public string ErrorMessage;
        public string ErrorStackTrace;
        public string ErrorURL;
        public string LoginUser;
        public string OrganizationID;
        public string CreatedOn;
        public string LoginUserId;
        public ErrorLog(Exception e)
        {
            parseException(e, System.Web.HttpContext.Current);
        }

        public ErrorLog(Exception e, HttpContext context)
        {
            
            parseException(e, context);
        }

        private void parseException(Exception e, HttpContext context)
        {
            ErrorLogID = System.Guid.NewGuid().ToString();
            ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
            //AWBVersion = LogConfig.Instance.AWBVersion;
            string awbAssembliesVersion = LogConfig.Instance.AssemblyVersion;
            //SetAssembly(awbAssembliesVersion);
            this.ClientHostName = context == null ? "" : context.Request.UserHostName;
            this.ClientHostAddress = context == null ? "" : context.Request.UserHostAddress;
            ClientBrowser = context == null ? "" : context.Request.Browser.Browser + " " + context.Request.Browser.Version;           
            if (context != null)
            {
                ClientJavaScriptSupport = context.Request.Browser.EcmaScriptVersion.Major >= 1 ? true : false;
                if (context.Request.UserLanguages != null)
                {
                    ClientBrowserLanguage =context.Request.UserLanguages[0];
                    ClientOSLanguage = context.Request.UserLanguages[0];
                }
                if(context.Request.UserAgent!=null)
                {
                    ClientNetRuntime=context.Request.UserAgent;
                }
                if (context.Request.Browser.Platform != null)
                {
                    ClientOS =context.Request.Browser.Platform;
                }
                if (context.Server.MachineName != null)
                {
                    ComputerName = context.Server.MachineName;
                }
                if (context.Request.Url != null)
                {
                    ErrorURL = context.Request.Url.ToString();
                }
            }            
            ServerOS = System.Environment.OSVersion.VersionString;
            ServerNetRuntime = ".Net CLR " + System.Environment.Version.ToString();            
            ErrorExecutionTrace = "";
            ErrorMessage = e.Message;
            ErrorStackTrace = e.StackTrace;           
            SetUserInfo();
            CreatedOn = DateTime.Now.ToString("F");
        }

        //private void parseException(Exception e, OperationContext context)
        //{
        //    ErrorLogID = System.Guid.NewGuid().ToString();
        //    //AWBVersion = LogConfig.Instance.AWBVersion;
        //    string awbAssembliesVersion = LogConfig.Instance.AssemblyVersion;
        //    //SetAssembly(awbAssembliesVersion);
        //    this.ClientHostName = context == null ? "" : context.Channel.RemoteAddress..Request.UserHostName;
        //    this.ClientHostAddress = context == null ? "" : context.Request.UserHostAddress;
        //    ClientBrowser = context == null ? "" : context.Request.Browser.Browser + " " + context.Request.Browser.Version;           
        //    if (context != null)
        //    {
        //        ClientJavaScriptSupport = context.Request.Browser.EcmaScriptVersion.Major >= 1 ? true : false;
        //        if (context.Request.UserLanguages != null)
        //        {
        //            ClientBrowserLanguage =context.Request.UserLanguages[0];
        //            ClientOSLanguage = context.Request.UserLanguages[0];
        //        }
        //        if(context.Request.UserAgent!=null)
        //        {
        //            ClientNetRuntime=context.Request.UserAgent;
        //        }
        //        if (context.Request.Browser.Platform != null)
        //        {
        //            ClientOS =context.Request.Browser.Platform;
        //        }
        //        if (context.Server.MachineName != null)
        //        {
        //            ComputerName = context.Server.MachineName;
        //        }
        //        if (context.Request.Url != null)
        //        {
        //            ErrorURL = context.Request.Url.ToString();
        //        }
        //    }            
        //    ServerOS = System.Environment.OSVersion.VersionString;
        //    ServerNetRuntime = ".Net CLR " + System.Environment.Version.ToString();            
        //    ErrorExecutionTrace = "";
        //    ErrorMessage = e.Message;
        //    ErrorStackTrace = e.StackTrace;           
        //    SetUserInfo();
        //    CreatedOn = DateTime.Now.ToString("F");
        //}
        /// <summary>
        /// 根据配置信息设定assembly
        /// </summary>
        private void SetAssembly(string awbAssembliesVersion)
        {
            Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            Assembly[] assemblyArrayResult = assemblyArray;
            if (awbAssembliesVersion != null)
            {
                string[] assemblyversion = awbAssembliesVersion.Split(',');
                for (int i = 0; i < assemblyversion.Length; i++)
                {
                    foreach (Assembly ass in assemblyArray)
                    {
                        if (ass.GetName().Name.ToString() == assemblyversion[i])
                        {
                            assemblyArrayResult.SetValue(ass, 0);
                        }
                    }
                }
            }
            SetAssembly(assemblyArrayResult);
        }

        private void SetAssembly(Assembly[] assemblyArray)
        {
            foreach (Assembly ass in assemblyArray)
            {
                //AWBAssembliesVersion += ass.GetName().Name.ToString() + ".dll(" + ass.GetName().Version.ToString() + ")" + ";";
            }
        }

        private void SetUserInfo()
        {
            try
            {
                DataTable dt = GetUserInfo();
                if (dt.Rows.Count > 0)
                {
                    OrganizationID = dt.Rows[0]["organizationidname"].ToString();
                    LoginUserId = dt.Rows[0]["wsl_userid"].ToString();
                    LoginUser = dt.Rows[0]["DomainName"].ToString();
                }
            }
            catch
            {
                Tracer.Info("Error Log have not connection configuration,so only get the user name");
                LoginUser = System.Environment.UserDomainName + "\\" + System.Environment.UserName;
            }
        }

        private DataTable GetUserInfo()
        {
            //IDAO dao = null;
            //dao = new SqlServerDAO(LogConfig.Instance.ConnectionString);
            //string sql = "select organizationidname,wsl_userid , DomainName from systemuser  where DomainName = SUSER_SNAME()";
            //return dao.GetDataTable(sql);
            return new DataTable();
        }
    }
}
