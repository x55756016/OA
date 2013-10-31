using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.DAL
{
    public class V_UserMenu
    {
        
        public T_SYS_ENTITYMENU EntityMenu { get; set; }
        public T_SYS_ROLEENTITYMENU RoleEntity { get; set; }
        public T_SYS_USER sysuser { get; set; }
        public T_SYS_USERROLE sysuserrole { get; set; }
    }
}
