using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.DAL
{
    public class V_UserPermissionRoleID
    {
        public string RoleEntityMenuID { get; set; } //角色菜单ID
        public string RoleEntityPermissionID { get; set; } //角色菜单权限ID
        public string RoleID { get; set; } //角色ID
        public string PermissionID { get; set; } //权限ID
        public string EntityMenuID { get; set; } //菜单ID                
        public string PermissionDataRange { get; set; }//权限值                
        public string SysUserID { get; set; }
    }
}
