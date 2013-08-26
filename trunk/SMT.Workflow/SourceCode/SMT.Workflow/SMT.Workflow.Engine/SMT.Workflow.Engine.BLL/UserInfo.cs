/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：UserInfo.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/24 8:57:14   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Engine.DAL;

namespace SMT.Workflow.Engine.BLL
{
    public static class UserInfo
    {
        public static void GetUserInfo(string User, ref string strUserMail, ref string strRtxAccount)
        {
            try
            {
                PersonnelService.PersonnelServiceClient Client = new PersonnelService.PersonnelServiceClient();
                string[] strUser = new string[1];
                strUser[0] = User;
                PersonnelService.EmployeeContactWays[] List = Client.GetEmployeeToEngine(strUser);
                if (List.Length > 0)
                {
                    strUserMail = List[0].MAILADDRESS;
                    //strRtxAccount = List[0].RTX;
                }
            }
            catch (Exception e)
            {
                Log.WriteLog("GetUserInfo()调用HRUserInfo出现异常" + e.Message);
            }
        }
    }
}
