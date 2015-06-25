using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SMT.HRM.CustomModel.Permission
{

    /// <summary>
    /// 功能菜单项
    /// </summary>
    ///     
    [Serializable]
    [DataContract]
    public class CustomerPermission
    {
         [DataMember]
        public string EntityMenuId;
        [DataMember]
        public List<PermissionValue> PermissionValue;
    }

    /// <summary>
    /// 权限项
    /// </summary>
    [Serializable]
   
    public class PermissionValue
    {
    
        public string Permission;
        
        public List<OrgObject> OrgObjects;
    }
    /// <summary>
    /// 组织架构项
    /// </summary>
    [Serializable]
    
    public class OrgObject
    {      
        public string OrgType;
       
        public string OrgID;
    }
}
