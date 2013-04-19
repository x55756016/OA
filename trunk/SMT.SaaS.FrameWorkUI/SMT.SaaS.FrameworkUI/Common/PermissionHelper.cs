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

using SMT.SaaS.FrameworkUI;
using CurrentContext = SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.FrameworkUI.Common
{
    public class PermissionHelper
    {

        ///// <summary>
        ///// 查询时获取对应实体的权限,适合判断GridView上面的按钮
        ///// </summary>
        ///// <param name="menuCode">实体菜单</param>
        ///// <param name="perm">权限类型</param>
        ///// <returns>权限范围</returns>
        //public static int GetPermissionValue(string menuCode, Permissions perm)
        //{

        //        //return 1;
        //        int rslt = 0;
        //        if (CurrentContext.Common.CurrentConfig.CurrentUser.PermissionInfo != null)
        //        {

        //            int permvalue = Convert.ToInt32(perm);

        //            //CurrentContext.Common.CurrentConfig.CurrentUser.PermissionInfo.ForEach(item =>
        //            //{
        //            //    System.Diagnostics.Debug.WriteLine(item.EntityMenu.MENUCODE + ":" + item.Permission.PERMISSIONNAME);
        //            //});
        //            var objs = from o in CurrentContext.Common.CurrentConfig.CurrentUser.PermissionInfo
        //                       where o.Permission.PERMISSIONVALUE == Convert.ToInt32(permvalue).ToString()
        //                       && o.EntityMenu.MENUCODE == menuCode
        //                       select o;

        //            //获取查询的权限,值越小，权限越大
        //            if (objs == null || objs.Count() <= 0)
        //                rslt = -1;
        //            else
        //                rslt = objs.Min(p => Convert.ToInt32(p.RoleMenuPermission.DATARANGE));

        //        }
        //        return rslt;


        //}

        /// <summary>
        /// 查询时获取对应实体的权限,适合判断GridView上面的按钮
        /// </summary>
        /// <param name="menuCode">实体菜单</param>
        /// <param name="perm">权限类型</param>
        /// <returns>权限范围</returns>
        public static int GetPermissionValue(string menuCode, Permissions perm)
        {
            //return 1;
            // edit liujx  将rslt=0 改为rslt=-1 有集团的权限为0 ，为最大权限
            int rslt = -1;
            try
            {
                if (CurrentContext.Common.CurrentLoginUserInfo != null)
                {
                    if (CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI != null)
                    {

                        int permvalue = Convert.ToInt32(perm);
                        //var objs = from o in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI
                        //           where o.PermissionDataRange == Convert.ToInt32(permvalue).ToString()
                        //           && o.EntityMenuCode == menuCode
                        //           select o;
                        var objs = from o in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI
                                   where o.PermissionValue == Convert.ToInt32(permvalue).ToString()
                                   && o.MenuCode == menuCode
                                   select o;
                        //获取查询的权限,值越小，权限越大
                        if (objs == null || objs.Count() <= 0)
                        {
                            rslt = -1;
                        }
                        else
                        {
                            rslt = objs.Min(p => Convert.ToInt32(p.DataRange));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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

            try
            {

                // return 1;
                int rslt = 0;
                int permvalue = Convert.ToInt32(perm);
                //角色权限
                if (CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI != null)
                {

                    var objs = from o in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI
                               where o.PermissionValue == Convert.ToInt32(permvalue).ToString()
                               && o.MenuCode == menuCode
                               select o;

                    rslt = objs.Min(p => Convert.ToInt32(p.DataRange));
                }
                else
                {
                    return -1;
                }
                #region 注释.原因：自定义权限CustomPerms集合已经过期 GaoYan-2010.12.15
                ////自定义权限
                ////有岗位的特殊权限
                //if (CurrentContext.Common.CurrentConfig.CurrentUser.CustomPerms != null)
                //{
                //    var pPerms = from o in CurrentContext.Common.CurrentConfig.CurrentUser.CustomPerms
                //                 where o.T_SYS_ENTITYMENU.MENUCODE == menuCode
                //                 && o.T_SYS_PERMISSION.PERMISSIONVALUE == Convert.ToInt32(permvalue).ToString()
                //                 && o.POSTID == postID
                //                 select o;
                //    if (pPerms.Count() > 0)
                //        rslt = 2;


                //    //有部门的特殊权限
                //    var dPerms = from o in CurrentContext.Common.CurrentConfig.CurrentUser.CustomPerms
                //                 where o.T_SYS_ENTITYMENU.MENUCODE == menuCode
                //                 && o.T_SYS_PERMISSION.PERMISSIONVALUE == Convert.ToInt32(permvalue).ToString()
                //                 && o.DEPARTMENTID == departID
                //                 select o;
                //    if (dPerms.Count() > 0)
                //        rslt = 1;

                //    //有公司的特殊权限
                //    var cPerms = from o in CurrentContext.Common.CurrentConfig.CurrentUser.CustomPerms
                //                 where o.T_SYS_ENTITYMENU.MENUCODE == menuCode
                //                 && o.T_SYS_PERMISSION.PERMISSIONVALUE == Convert.ToInt32(permvalue).ToString()
                //                 && o.COMPANYID == companyID
                //                 select o;
                //    if (cPerms.Count() > 0)
                //        rslt = 0;
                //}
                #endregion
                return rslt;
            }
            catch (Exception)
            {

                return -1;
            }

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

        /// <summary>
        /// 显示DataGrid上面通用按钮
        /// </summary>
        /// <param name="toolBar">所属工具条</param>
        /// <param name="entityName">表名称</param>
        /// <param name="displayAuditButton">是示有审核按钮</param>
        public static void DisplayGridToolBarButton(FormToolBar toolBar, string entityName, bool displayAuditButton)
        {
            //查看
            if (PermissionHelper.GetPermissionValue(entityName, Permissions.Browse) < 0)
            {
                MessageBox.Show(SMT.SaaS.Globalization.Localization.GetString("NOPERMISSION"));
                Uri uri = new Uri("/Home", UriKind.Relative);

                //取当前主页
                Grid grid = Application.Current.RootVisual as Grid;
                if (grid != null && grid.Children.Count > 0)
                {
                    //MainPage page = grid.Children[0] as MainPage;
                    //if (page != null)
                    //{
                    //    page.NavigateTo(uri);
                    //}
                }

            }
            //添加
            if (PermissionHelper.GetPermissionValue(entityName, Permissions.Add) < 0)
            {
                toolBar.btnNew.Visibility = Visibility.Collapsed;
                toolBar.retNew.Visibility = Visibility.Collapsed;
            }
            //修改
            if (PermissionHelper.GetPermissionValue(entityName, Permissions.Edit) < 0)
            {
                toolBar.btnEdit.Visibility = Visibility.Collapsed;
                toolBar.retEdit.Visibility = Visibility.Collapsed;
            }
            //删除
            if (PermissionHelper.GetPermissionValue(entityName, Permissions.Delete) < 0)
            {
                toolBar.btnDelete.Visibility = Visibility.Collapsed;
            }

            if (displayAuditButton)
            {
                //审核
                if (PermissionHelper.GetPermissionValue(entityName, Permissions.Audit) < 0)
                {
                    toolBar.btnAudit.Visibility = Visibility.Collapsed;
                    toolBar.retAudit.Visibility = Visibility.Collapsed;

                }
            }
            else
            {
                toolBar.btnAudit.Visibility = Visibility.Collapsed;
                toolBar.retAudit.Visibility = Visibility.Collapsed;

                toolBar.stpCheckState.Visibility = Visibility.Collapsed;
            }
        }

    }
    public enum Permissions
    {
        /// <summary>
        /// 添加
        /// </summary>
        Add,
        /// <summary>
        /// 编辑
        /// </summary>
        Edit,
        /// <summary>
        /// 删除
        /// </summary>
        Delete,
        /// <summary>
        /// 查询
        /// </summary>
        Browse,
        /// <summary>
        /// 导出
        /// </summary>
        Export,
        /// <summary>
        /// 报表
        /// </summary>
        Report,
        /// <summary>
        /// 审核
        /// </summary>
        Audit,
        /// <summary>
        /// 导入
        /// </summary>
        Import,
        /// <summary>
        /// 提交
        /// </summary>
        Submit,
        /// <summary>
        /// 重新提交
        /// </summary>
        ReSubmit


        /*
        #region CompanyDictionary

        CompanyDictionary_Add,
        CompanyDictionary_Delete,
        CompanyDictionary_Browse,
        CompanyDictionary_Edit,

        #endregion

        #region DepartmentDictionary

        DepartmentDictionary_Add,
        DepartmentDictionary_Delete,
        DepartmentDictionary_Browse,
        DepartmentDictionary_Edit,

        #endregion

        #region PostDictionary

        PostDictionary_Add,
        PostDictionary_Edit,
        PostDictionary_Delete,
        PostDictionary_Browse,

        #endregion

        #region Company

        Company_Add,
        Company_Delete,     
        Comapny_Browse,
        Company_Edit,
        Company_Audit,
        Company_Merge,
        Company_Cancel,

        #endregion

        #region Department

        Department_Add,
        Department_Delete,
        Department_Browse,
        Department_Edit,
        Department_Audit,
        Department_Cancel,

        #endregion

        #region Post

        Post_Add,
        Post_Delete,
        Post_Browse,
        Post_Edit,
        Post_Audit,
        Post_Cancel,

        #endregion

        #region Resume

        Resume_Add,
        Resume_Edit,
        Resume_Delete,
        Resume_Browse,

        #endregion

        #region BlackList

        BlackList_Add,
        BlackList_Edit,
        BlackList_Delete,
        BlackList_Browse,

        #endregion

        #region PensionAlarmSet

        PensionAlarmSet_Add,
        PensionAlarmSet_Edit,
        PensionAlarmSet_Delete,
        PensionAlarmSet_Browse,

        #endregion

        #region PensionMaster

        PensionMaster_Add,
        PensionMaster_Edit,
        PensionMaster_Delete,
        PensionMaster_Browse,

        #endregion

        #region PensionDetail

        PensionDetail_Read,
        PensionDetail_Delete,
        PensionDetail_Browse,

        #endregion

        #region EmployeeInsurance

        EmployeeInsurance_Add,
        EmployeeInsurance_Edit,
        EmployeeInsurance_Delete,
        EmployeeInsurance_Browse,
        EmployeeInsurance_Audit,

        #endregion

        #region SalarySolution

        SalarySolution_Add,
        SalarySolution_Edit,
        SalarySolution_Delete,
        SalarySolution_Browse,
        SalarySolution_Audit,

        #endregion

        #region SalaryStandard

        SalaryStandard_Add,
        SalaryStandard_Edit,
        SalaryStandard_Delete,
        SalaryStandard_Browse,
        SalaryStandard_Audit,

        #endregion

        #region SalaryArchive

        SalaryArchive_Add,
        SalaryArchive_Edit,
        SalaryArchive_Delete,
        SalaryArchive_Browse,
        SalaryArchive_Audit,

        #endregion

        #region EmployeeSalaryRecord

        EmployeeSalaryRecord_Add,
        EmployeeSalaryRecord_Edit,
        EmployeeSalaryRecord_Delete,
        EmployeeSalaryRecord_Browse,
        EmployeeSalaryRecord_Audit,

        #endregion

        #region EmployeeEntry

        EmployeeEntry_Add,
        EmployeeEntry_Edit,
        EmployeeEntry_Delete,
        EmployeeEntry_Browse,
        EmployeeEntry_Audit,

        #endregion

        #region EmployeeCheck

        EmployeeCheck_Add,
        EmployeeCheck_Edit,
        EmployeeCheck_Delete,
        EmployeeCheck_Browse,
        EmployeeCheck_Audit,

        #endregion

        #region LeftOffice

        LeftOffice_Add,
        LeftOffice_Edit,
        LeftOffice_Delete,
        LeftOffice_Browse,
        LeftOffice_Audit,

        #endregion

        #region EmployeePostChange

        EmployeePostChange_Add,
        EmployeePostChange_Edit,
        EmployeePostChange_Delete,
        EmployeePostChange_Browse,
        EmployeePostChange_Audit,

        #endregion

        #region Employee

        Employee_Add,
        Employee_Edit,
        Employee_Delete,
        Employee_Browse,
        Employee_Audit,

        #endregion
        */

    }
}
