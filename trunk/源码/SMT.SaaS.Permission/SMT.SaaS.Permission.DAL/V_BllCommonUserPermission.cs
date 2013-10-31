using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.DAL
{
    public class V_BllCommonUserPermission
    {   
        public string PermissionDataRange { get; set; }
        public string RoleMenuPermissionValue { get; set; }
        public string OwnerCompanyID { get; set; }
        public string OwnerDepartmentID { get; set; }
        public string OwnerPostID { get; set; }
        
    }
}
