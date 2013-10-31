using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.DAL
{
    /// <summary>
    /// 用户对应的自定义权限，主要用于获取用户自定义权限
    /// </summary>
    public class V_UserRoleCustomerPerm
    {
        /// <summary>
        /// 自定义权限
        /// </summary>
        public T_SYS_ENTITYMENUCUSTOMPERM ENTITYMENUCUSTOMPERM { get; set; }        
        /// <summary>
        /// 角色实体
        /// </summary>
        public T_SYS_ROLE ROLE { get; set; }
        
    }
}
