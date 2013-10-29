using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.SaaS.LocalData.Tables
{
    public class V_PermissionCheck
    {
        private string _employeeID;
        private DateTime _userRoleDate;
        private DateTime _roleEntityMenuDate;
        private DateTime _roleMenuPermissionDate;
        private DateTime _entityMenuCustompermDate;
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID
        {
            set { _employeeID = value; }
            get { return _employeeID; }
        }
        /// <summary>
        /// 用户角色更新（T_SYS_USERROLE）时间
        /// </summary>
        public DateTime UserRoleDate
        {
            set { _userRoleDate = value; }
            get { return _userRoleDate; }
        }
        /// <summary>
        /// 角色菜单更新（T_SYS_ROLEENTITYMENU）时间
        /// </summary>
        public DateTime RoleEntityMenuDate
        {
            set { _roleEntityMenuDate = value; }
            get { return _roleEntityMenuDate; }
        }
        /// <summary>
        /// 角色菜单权限（T_SYS_ROLEMENUPERMISSION）更新时间
        /// </summary>
        public DateTime RoleMenuPermissionDate
        {
            set { _roleMenuPermissionDate = value; }
            get { return _roleMenuPermissionDate; }
        }
        /// <summary>
        /// 角色菜单自定义权限（T_SYS_ENTITYMENUCUSTOMPERM）更新时间
        /// </summary>
        public DateTime EntityMenuCustompermDate
        {
            set { _entityMenuCustompermDate = value; }
            get { return _entityMenuCustompermDate; }
        }
    }
}
