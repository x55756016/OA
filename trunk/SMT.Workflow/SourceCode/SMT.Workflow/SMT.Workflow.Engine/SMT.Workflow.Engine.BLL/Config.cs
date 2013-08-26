/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Config.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/24 9:11:25   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SMT.Workflow.Engine.BLL
{
    public static class Config
    {
        /// <summary>
        /// 邮件地址
        /// </summary>
        public static string MailAddress
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["MailAddress"].ToString();
                }
                catch
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// 邮件地址
        /// </summary>
        public static string Account
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["Account"].ToString();
                }
                catch
                {
                    return "1001";
                }
            }
        }

        /// <summary>
        /// 邮件服务地址
        /// </summary>
        public static string MailServerAddress
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["MailServerAddress"].ToString();
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 邮件服务端口
        /// </summary>
        public static int MailServerPort
        {
            get
            {
                try
                {
                    return Int32.Parse(ConfigurationManager.AppSettings["MailServerPort"].ToString());
                }
                catch
                {
                    return 25;
                }
            }
        }

        /// <summary>
        /// 邮件密码
        /// </summary>
        public static string MailPwd
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["MailPwd"].ToString();
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 邮件标题
        /// </summary>
        public static string MailTitle
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["MailTitle"].ToString();
                }
                catch
                {
                    return "{0}";
                }
            }
        }

        public static string MailUrl
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["MailUrl"];
                }
                catch
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// 邮件模板地址
        /// </summary>
        public static string MailTempletePath
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["MailTempletePath"];
                }
                catch
                {
                    return "";
                }
            }
        }


        public static string RTXTitle
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["RTXTitle"];
                }
                catch
                {
                    return "";
                }
            }
        }

        public static string GetSystemCode(string code)
        {
            try
            {
                return ConfigurationManager.AppSettings[code];
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 邮件调用间隔
        /// </summary>
        public static int MailRtxTimer
        {
            get
            {
                try
                {
                    return Convert.ToInt32(ConfigurationManager.AppSettings["MailRtxTimer"]);
                }
                catch
                {
                    return 5;
                }
            }
        }
        /// <summary>
        /// 服务调用间隔
        /// </summary>
        public static int TriggerTimer
        {
            get
            {
                try
                {
                    return Convert.ToInt32(ConfigurationManager.AppSettings["TriggerTimer"]);
                }
                catch
                {
                    return 30;
                }
            }
        }

    }
}
