using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_System_EFModel;
using SMT.SaaS.Permission.DAL.views;

namespace SMT.SaaS.Permission.DAL
{
    public class V_UserLogin
    {
        /// <summary>
        /// 权限中系统用户ID
        /// </summary>
        public string SYSUSERID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 是否管理员
        /// </summary>
        public string ISMANAGER { get; set; }
        /// <summary>
        /// 登录记录ID
        /// </summary>
        public string LOGINRECORDID { get; set; }
        
        
    }
}
