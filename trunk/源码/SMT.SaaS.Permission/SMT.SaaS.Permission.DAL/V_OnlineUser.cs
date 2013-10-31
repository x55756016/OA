using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_System_EFModel;
using SMT.SaaS.Permission.DAL.views;

namespace SMT.SaaS.Permission.DAL
{
    public class V_OnlineUser
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
        /// 员工姓名
        /// </summary>
        public string EMPLOYEENAME { get; set; }
        /// <summary>
        /// 员工编号
        /// </summary>
        public string EMPLOYEECODE { get; set; }
        /// <summary>
        /// 登录IP
        /// </summary>
        public string LOGINIP { get; set; }
        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LOGINDATE { get; set; }
        /// <summary>
        /// 登录状态
        /// </summary>
        public string ONLINESTATE { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string USERNAME { get; set; }
        
    }
}
