/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Config.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/21 9:13:11   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SMT.Workflow.Engine.Services.BLL
{
    public static class Config
    {
        // <!--是否执行更新审核状态 0：执行 1：不执行-->
        //<add key="IsNeedUpdateAudit" value="0"/>
        public static bool IsNeedUpdateAudit
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["IsNeedUpdateAudit"].ToString() == "0" ? true : false;
                }
                catch
                {
                    return false;
                }
            }
        }
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

        public static int pageSize
        {
            get
            {
                try
                {
                    return Convert.ToInt32(ConfigurationManager.AppSettings["PageNumber"]);
                }
                catch
                {
                    return 15;
                }
            }
        }
    }
}
