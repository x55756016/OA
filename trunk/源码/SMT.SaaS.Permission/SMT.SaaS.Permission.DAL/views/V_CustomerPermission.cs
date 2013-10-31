using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.Permission.DAL.views
{

    /// <summary>
    /// 功能菜单项
    /// </summary>
    ///     
    [Serializable]
    public class CustomerPermission
    {
        public string EntityMenuId;
       
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
