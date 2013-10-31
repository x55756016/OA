using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;
namespace SMT.SaaS.Permission.DAL
{
    public class V_UserPermission
    {
        public string RoleMenuPermissionValue { get; set; }
        public string EntityMenuID { get; set; }
        public string PermissionDataRange { get; set; }
        public string EntityRoleID { get; set; }
        public string RoleID { get; set; }
        public string SysUserID { get; set; }
    }
}
