using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 流程获取用户信息
    /// </summary>
    public class V_EMPLOYEEPOSTFLOWUSER
    {
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EMPLOYEENAME { get; set; }
        /// <summary>
        /// 员工岗位ID
        /// </summary>
        public string EMPLOYEEPOSTID { get; set; }
        /// <summary>
        /// 岗位ID
        /// </summary>
        public string POSTID { get; set; }        
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string PostName { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        public string DepartmentID { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>
        public string CompanyID { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 是否是主岗位
        /// </summary>
        public string ISAGENCY { get; set; }
        /// <summary>
        /// 岗位级别
        /// </summary>
        public decimal? POSTLEVEL { get; set; }
        /// <summary>
        /// 是否是所在的部门的负责人 默认为false
        /// </summary>
        public bool ISHEAD { get; set; }
        /// <summary>
        /// 是否是所在的岗位的直接上级 默认为false
        /// </summary>
        public bool ISSUPERIOR { get; set; } 
    }
}
