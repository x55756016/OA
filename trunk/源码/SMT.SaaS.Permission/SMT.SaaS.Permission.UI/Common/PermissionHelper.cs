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
using System.Linq;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI
{
    public class PermissionHelper
    {
        /// <summary>
        /// 查询时获取对应实体的权限,适合判断GridView上面的按钮
        /// </summary>
        /// <param name="menuCode">实体菜单</param>
        /// <param name="perm">权限类型</param>
        /// <returns>权限范围</returns>
        public static int GetPermissionValue(string menuCode, Permissions perm)
        {
            //return 1;
            int rslt = 0;
            if (Common.CurrentLoginUserInfo.PermissionInfoUI != null)
            {
                int permvalue = Convert.ToInt32(perm);
                var tmp = Common.CurrentLoginUserInfo.PermissionInfoUI.Where(p => p.MenuCode == "T_HR_COMPANY").ToList();
                var objs = from o in Common.CurrentLoginUserInfo.PermissionInfoUI
                           where o.PermissionValue == Convert.ToInt32(permvalue).ToString()
                           && o.MenuCode == menuCode
                           select o;
                //获取查询的权限,值越小，权限越大
                if (objs == null || objs.Count() <= 0)
                    rslt = -1;
                else
                    rslt = objs.Min(p => Convert.ToInt32(p.DataRange));

            }
            return rslt;

        }
        /// <summary>
        /// 获取具体某条记录的权限，适合编辑窗体上的菜单按钮
        /// </summary>
        /// <param name="menuCode">实体菜单</param>
        /// <param name="perm">具体的权限</param>
        /// <param name="postID">记录的所属岗位</param>
        /// <param name="departID">记录的所属部门</param>
        /// <param name="companyID">记录的所属公司</param>
        /// <returns>权限范围</returns>
        public static int GetPermissionValue(string menuCode, Permissions perm,
            string ownerid, string postID, string departID, string companyID)
        {
            return 1;
            //int rslt = 0;
            //int permvalue = Convert.ToInt32(perm);
            ////角色权限
            //if (Common.CurrentConfig.CurrentUser.PermissionInfo != null)
            //{

            //    var objs = from o in Common.CurrentConfig.CurrentUser.PermissionInfo
            //               where o.Permission.PERMISSIONVALUE == Convert.ToInt32(permvalue).ToString()
            //               && o.EntityMenu.MENUCODE == menuCode
            //               select o;

            //    rslt = objs.Min(p => Convert.ToInt32(p.RoleMenuPermission.DATARANGE));
            //}

            ////自定义权限
            ////有岗位的特殊权限
            //var pPerms = from o in Common.CurrentConfig.CurrentUser.CustomPerms
            //             where o.T_SYS_ENTITYMENU.MENUCODE == menuCode
            //             && o.T_SYS_PERMISSION.PERMISSIONVALUE == Convert.ToInt32(permvalue).ToString()
            //             && o.POSTID == postID
            //             select o;
            //if (pPerms.Count() > 0)
            //    rslt = 2;

            ////有部门的特殊权限
            //var dPerms = from o in Common.CurrentConfig.CurrentUser.CustomPerms
            //             where o.T_SYS_ENTITYMENU.MENUCODE == menuCode
            //             && o.T_SYS_PERMISSION.PERMISSIONVALUE == Convert.ToInt32(permvalue).ToString()
            //             && o.DEPARTMENTID == departID
            //             select o;
            //if (dPerms.Count() > 0)
            //    rslt = 1;

            ////有公司的特殊权限
            //var cPerms = from o in Common.CurrentConfig.CurrentUser.CustomPerms
            //             where o.T_SYS_ENTITYMENU.MENUCODE == menuCode
            //             && o.T_SYS_PERMISSION.PERMISSIONVALUE == Convert.ToInt32(permvalue).ToString()
            //             && o.COMPANYID == companyID
            //             select o;
            //if (cPerms.Count() > 0)
            //    rslt = 0;

            //return rslt;

        }


        public static void SetFormToolBarPermission()
        {

        }

        /// <summary>
        /// 设置DataGrid的ToolBar上的按钮的权限
        /// </summary>
        /// <param name="toolBar">工具条</param>
        /// <param name="entityMenuCode">实体菜单编号</param>
        public static void SetGridToolBarPermission(SMT.SaaS.FrameworkUI.FormToolBar toolBar, string entityMenuCode)
        {
            //新增
            int perm = GetPermissionValue(entityMenuCode, Permissions.Add);

            if (perm < 0)
                toolBar.btnNew.Visibility = Visibility.Collapsed;

            //修改
            perm = GetPermissionValue(entityMenuCode, Permissions.Edit);

            if (perm < 0)
                toolBar.btnEdit.Visibility = Visibility.Collapsed;

            //删除
            perm = GetPermissionValue(entityMenuCode, Permissions.Delete);

            if (perm < 0)
                toolBar.btnDelete.Visibility = Visibility.Collapsed;

            //审核
            perm = GetPermissionValue(entityMenuCode, Permissions.Audit);

            if (perm < 0)
            {
                toolBar.btnAudit.Visibility = Visibility.Collapsed;
                //toolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
                //toolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            }

            //导出
            perm = GetPermissionValue(entityMenuCode, Permissions.Export);

            if (perm < 0)
            {
                toolBar.btnOutExcel.Visibility = Visibility.Collapsed;
                toolBar.btnOutPDF.Visibility = Visibility.Collapsed;
            }

            //导入
            perm = GetPermissionValue(entityMenuCode, Permissions.Import);

            if (perm < 0)
                toolBar.btnImport.Visibility = Visibility.Collapsed;


        }




        



    }
}
