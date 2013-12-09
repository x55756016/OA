using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;
namespace SMT.SaaS.Permission.DAL
{
    public class V_FBAdmin
    {
        /// <summary>
        /// 预算管理员ID
        /// </summary>
        public string FBADMINID { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string OWNERCOMPANYNAME { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string OWNERDEPARTMENTNAME { get; set; }
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string OWNERPOSTNAME { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>
        public string OWNERCOMPANYID { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        public string OWNERDEPARTMENTID { get; set; }
        /// <summary>
        /// 岗位ID
        /// </summary>
        public string OWNERPOSTID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EMPLOYEENAME { get; set; }
        /// <summary>
        /// 添加人
        /// </summary>
        public string ADDUSERID { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime? AddDate { get; set; }
        /// <summary>
        /// 员工公司ID
        /// </summary>
        public string EMPLOYEECOMPANYID { get; set; }
        /// <summary>
        /// 员工部门ID
        /// </summary>
        public string EMPLOYEEDEPARTMENTID { get; set; }
        /// <summary>
        /// 员工岗位ID
        /// </summary>
        public string EMPLOYEEPOSTID { get; set; }
    }
}
