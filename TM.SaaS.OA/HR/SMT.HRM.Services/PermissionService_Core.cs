using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT.HRM.BLL.Permission;
using TM_SaaS_OA_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using SMT.HRM.DAL.Permission;
using SMT.HRM.CustomModel.Permission;
using SMT.Foundation.Log;
using InterActiveDirectory;
using SMT.HRM.CustomModel.Permission;

namespace SMT.SaaS.Permission.Services
{
    public partial class PermissionService
    {

        /// <summary>
        /// 平台登录获取用户菜单2015-6-24
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <param name="userID">用户ID</param>
        /// <returns>菜单信息列表</returns>
        /// <param name="menuids"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_UserMenuPermission> GetSysLeftMenuFilterPermissionToNewFrame(string userID)
        {

            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<V_UserMenuPermission> menuList = bll.GetSysLeftMenuFilterPermissionToNewFrame(userID);
                return menuList != null ? menuList.ToList() : null;
            }

        }

        /// <summary>
        /// ken 平台打开菜单时根据菜单编号判断权限
        /// 2015-6-24 
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        [OperationContract]
        public List<V_UserPermissionUI> GetEntityPermissionByUser(string userID, string StrMenuId)
        {

            using (SysUserBLL bll = new SysUserBLL())
            {
                IQueryable<V_UserPermissionUI> plist;
                plist = bll.GetUserPermissionByUserToUI(userID, StrMenuId);
                return plist.Count() > 0 ? plist.ToList() : null;
            }
        }

        /// <summary>
        /// 判断登录用户在某一菜单中时候有自定义权限 2010-10-19
        /// </summary>
        /// <param name="menuID">系统自定义菜单权限编号</param>
        [OperationContract]
        public T_SYS_ENTITYMENUCUSTOMPERM GetCustomerPermissionByUserIDAndEntityCode(string StrUserId, string StrEntityCode)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                return bll.GetCustomerPermissionByUserIDAndEntityCode(StrUserId, StrEntityCode);
            }
        }
    }
}