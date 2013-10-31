using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Core;
using SMT_System_EFModel;
using System.Data.Objects;

namespace SMT.SaaS.Permission.DAL
{
    public class SysRoleDAL : CommDAL<T_SYS_ROLE>
    {
       
       
    }

    public class SysRoleEntityMenuDAL : CommDAL<T_SYS_ROLEENTITYMENU>
    {
        public void BeginTransaction()
        {
            base.BeginTransaction();
        }
        public void CommitTransaction()
        {
            base.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            base.RollbackTransaction();
        }
    }
    public class SysRoleMenuPermissionDAL : CommDAL<T_SYS_ROLEMENUPERMISSION>
    {

    }
    public class SysUserDAL : CommDAL<T_SYS_USER>
    {



    }

    public class SysProvinceCityDAL : CommDAL<T_SYS_PROVINCECITY>
    {

    }
    public class SysUserLoginRecordDAL : CommDAL<T_SYS_USERLOGINRECORD>
    {

    }

    

    public class SysUserLoginRecordHistroryDAL : CommDAL<T_SYS_USERLOGINRECORDHIS>
    {

    }

    public class SysUserRoleDAL : CommDAL<T_SYS_USERROLE>
    {
       

    }

}
