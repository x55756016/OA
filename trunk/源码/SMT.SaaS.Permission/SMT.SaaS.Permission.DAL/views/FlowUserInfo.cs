using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.DAL.views
{
    /// <summary>
    /// 供流程、引擎调用
    /// </summary>
    public class FlowUserInfo
    {
        /// <summary>
        /// 用户姓名ID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EmployeeName { get; set; }
        /// <summary>
        /// 所属公司ID
        /// </summary>
        public string CompayID { get; set; }
        /// <summary>
        /// 所属公司名称
        /// </summary>
        public string CompayName { get; set; }
        /// <summary>
        /// 所属部门ID
        /// </summary>
        public string DepartmentID { get; set; }
        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 所属岗位ID
        /// </summary>
        public string PostID { get; set; }
        /// <summary>
        /// 所属岗位名称
        /// </summary>
        public string PostName { get; set; }
        /// <summary>
        /// 所包含的角色：一个人在同一家公司可以有多个角色
        /// </summary>
        public List<T_SYS_ROLE> Roles { get; set; }
        /// <summary>
        /// 是否是所在的部门的负责人 默认为false
        /// </summary>
        public bool IsHead { get; set; }
        /// <summary>
        /// 是否是所在的岗位的直接上级 默认为false
        /// </summary>
        public bool IsSuperior { get; set; } 

    }
}
