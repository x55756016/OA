using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TM_SaaS_OA_EFModel;
namespace SMT.HRM.CustomModel.Permission
{
    [Serializable]
    [DataContract]
    public class V_UserPermission
    {
        [DataMember]
        public string RoleMenuPermissionValue { get; set; }
        [DataMember]
        public string EntityMenuID { get; set; }
        [DataMember]
        public string PermissionDataRange { get; set; }
        [DataMember]
        public string EntityRoleID { get; set; }
        [DataMember]
        public string RoleID { get; set; }
        [DataMember]
        public string SysUserID { get; set; }
    }
}
