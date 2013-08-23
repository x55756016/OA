/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Config.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 10:02:27   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.FlowWFService 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SMT.FlowWFService
{
    public static class Config
    {

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
    }
}
