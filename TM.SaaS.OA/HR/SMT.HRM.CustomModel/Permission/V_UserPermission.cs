using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TM_SaaS_OA_EFModel;
namespace SMT.HRM.CustomModel.Permission
{
    [Serializable]
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
