using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 获取员工所在的公司、部门、岗位信息
    /// 以供流程使用
    /// </summary>
    public class V_FlowUserInfo
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
        /// 是否是所在的部门的负责人
        /// </summary>
        public bool IsHead { get; set; }
        /// <summary>
        /// 是否是所在的岗位的直接上级
        /// </summary>
        public bool IsSuperior { get; set; }
        

    }
}
