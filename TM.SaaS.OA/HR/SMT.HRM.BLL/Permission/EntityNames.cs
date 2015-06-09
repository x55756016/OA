﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.BLL.Permission
{
    public enum EntityNames
    {
        SysDictionary,
        SysMenu,
        SysPermission,
        SysPermMenu,
        SysRole,
        SysRolePerm,
        SysUserRole,
        SysUser
    }

    public enum AssignObjectType
    {
        Company,
        Department,
        Post,
        Employee
    }
}
