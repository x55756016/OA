using System.Collections.Generic;

namespace SMT.SaaS.LocalData.Tables
{
   /// <summary>
    /// 用来处理各个菜单的UI中的权限的获取
    /// </summary>
    public class V_UserPermUILocal
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string UserModuleID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID { get; set; }                
        /// <summary>
        /// 权限值
        /// </summary>
        public string DataRange { get; set; }
        /// <summary>
        /// 权限数据范围
        /// </summary>
        public string PermissionValue { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public string EntityMenuID { get; set; }
        /// <summary>
        /// 菜单编码
        /// </summary>
        public string MenuCode { get; set; }
    }
    /// <summary>
    /// 菜单自定义权限
    /// </summary>
    public class V_CustomerPermission
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string UserModuleID { get; set; }
        /// <summary>
        /// 外键（员工菜单权限主键ID）
        /// </summary>
        public string PermissionUIID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public string EntityMenuId { get; set; }
    }
    /// <summary>
    /// 菜单权限项值
    /// </summary>
    public class V_PermissionValue
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string UserModuleID { get; set; }
        /// <summary>
        /// 外键（菜单自定义权限主键ID）
        /// </summary>
        public string CusPermID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID { get; set; }
        /// <summary>
        /// 权限项的值
        /// </summary>
        public string Permission;
    }
    /// <summary>
    /// 权限项应用范围
    /// </summary>
    public class V_OrgObject
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string UserModuleID { get; set; }
        /// <summary>
        /// 外键（菜单权限项值主键ID）
        /// </summary>
        public string PermValueID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID { get; set; }
        /// <summary>
        /// 机构类型
        /// </summary>
        public string OrgType { get; set; }
        /// <summary>
        /// 机构ID
        /// </summary>
        public string OrgID { get; set; }
    }
}
