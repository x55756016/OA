using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_System_EFModel;

namespace SMT.SaaS.Permission.DAL
{
    public class V_Permission
    {
        public T_SYS_ROLEMENUPERMISSION RoleMenuPermission { get; set; }
        public T_SYS_ENTITYMENU EntityMenu { get; set; }
        public T_SYS_PERMISSION Permission { get; set; }
        public T_SYS_ROLEENTITYMENU EntityRole { get; set; }
        public T_SYS_ROLE Role { get; set; }
        public T_SYS_USER SysUser { get; set; }
        public int Flag { get; set; }

    }
}
