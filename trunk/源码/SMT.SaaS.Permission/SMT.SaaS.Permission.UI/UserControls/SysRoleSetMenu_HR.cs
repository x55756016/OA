using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;
using System.Windows.Browser;
using System.IO;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.UserControls
{

    public partial class SysRoleSetMenu2 : UserControl, IEntityEditor
    {
        
        /// <summary>
        /// HR菜单数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetHRSysMenuByTypeCompleted(object sender, GetHRSysMenuByTypeCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    return;
                }

                if (e.Result != null)
                {
                    HrMenu = e.Result.ToList();
                }
            }
            //开始获取OA系统菜单
            tabhr.IsEnabled = true;//加载完菜单后启用HR标签
            RoleClient.GetOASysMenuByTypeAsync("1");
        }

        V_MenuSetRole menu = null;
        //private void SaveGridMenuPermission(DataGrid Dtgrid, string rateName)
        //{
        //    if (Dtgrid.ItemsSource != null)
        //    {
        //        foreach (object obj in Dtgrid.ItemsSource)
        //        {
        //            //T_SYS_ENTITYMENU menu = new T_SYS_ENTITYMENU();

        //            if (Dtgrid.Columns[0].GetCellContent(obj) != null)
        //            {
        //                //menu = Dtgrid.Columns[0].GetCellContent(obj).DataContext as T_SYS_ENTITYMENU;
        //                //tmpAllList += menu.T_SYS_ENTITYMENU2.ENTITYMENUID + ",";
        //                string StrMenuID = ""; //菜单ID
        //                menu = Dtgrid.Columns[0].GetCellContent(obj).DataContext as T_SYS_ENTITYMENU;
        //                StrMenuID = menu.ENTITYMENUID;
        //                string StrPermissionID = "";//权限ID
        //                int PermCount = 0;
        //                PermCount = tmpPermission.Count;
        //                int IndexCount = 1;
        //                //IndexCount = PermCount
        //                bool IsCheckRange = false;//是否选择了权限范围
        //                for (int i = 1; i < PermCount + 1; i++)
        //                {
        //                    IndexCount = IndexCount + i;
        //                    if (Dtgrid.Columns[i].GetCellContent(obj) != null)
        //                    {
        //                        string NewDataRange = "";
        //                        /* 
        //                         * 首先根据菜单ID和角色ID获取  角色菜单表的记录 存在则获取ROLEENTITYID
        //                         * 然后再根据 ROLEENTITYID和权限ID 获取角色菜单权限表中的记录，取得对应的权限和现在的权限值比较
        //                         * 如果相同则不处理  不同则处理
        //                         */
        //                        StrPermissionID = tmpPermission[i - 1].PERMISSIONID;  //权限ID
        //                        var q = from a in EntityPermissionInfosList//权限视图集合
        //                                where a.EntityRole.T_SYS_ENTITYMENU.ENTITYMENUID == StrMenuID 
        //                                && a.EntityRole.T_SYS_ROLE.ROLEID == tmprole.ROLEID
        //                                select a;
        //                        string RoleEntityID = "";
        //                        if (q.Count() > 0)
        //                        {
        //                            RoleEntityID = q.ToList().FirstOrDefault().EntityRole.ROLEENTITYMENUID.ToString(); //获取角色菜单ID
        //                        }
        //                        //var m = from a in EntityPermissionInfosList
        //                        var k = from b in EntityPermissionInfosList//权限视图集合
        //                                where b.RoleMenuPermission.T_SYS_PERMISSION.PERMISSIONID == StrPermissionID && b.RoleMenuPermission.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == RoleEntityID
        //                                select b;
        //                        string OldRangeValue = ""; //获取数据库中 权限对应的值

        //                        if (k.Count() > 0)
        //                        {
        //                            OldRangeValue = k.ToList().FirstOrDefault().RoleMenuPermission.DATARANGE.ToString();
        //                        }
        //                        //Rating hrrate = DaGrHR.Columns[i].GetCellContent(obj).FindName("HRrating") as Rating;
        //                        Button hrrate = Dtgrid.Columns[i].GetCellContent(obj).FindName(rateName) as Button;
                                
        //                        switch (hrrate.Content.ToString())
        //                        {
        //                            case "★"://员工 0.2 ×10
        //                                NewDataRange = "4";
        //                                break;
        //                            case "★★"://岗位 0.4×10
        //                                NewDataRange = "3";
        //                                break;
        //                            case "★★★"://部门 0.6*10
        //                                NewDataRange = "2";
        //                                break;
        //                            case "★★★★"://公司 0.8*10
        //                                NewDataRange = "1";
        //                                break;
        //                            //case "★★★★★"://集团 1.0*10
        //                            //    NewDataRange = "0";
        //                            //    break;
        //                            case ""://权限
        //                                NewDataRange = "";
        //                                break;
        //                        }
        //                        if (OldRangeValue != NewDataRange)
        //                        {
        //                            tmpAllList += NewDataRange + ",";
        //                            tmpAllList += tmpPermission[i - 1].PERMISSIONID + ";";
        //                            IsCheckRange = true;
        //                        }
        //                    }
        //                }
        //                if (IsCheckRange)
        //                {
        //                    tmpAllList += "@" + StrMenuID + "," + "";
        //                    tmpAllList += "#";
        //                }
        //            }
        //        }
                                
        //        if (IsAdd)
        //        {
        //            if (string.IsNullOrEmpty(tmpAllList))
        //            {
        //                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请先设置权限", Utility.GetResourceStr("CONFIRMBUTTON"));

        //                return;
        //            }

        //        }
        //    }
        //}


        private void SaveGridMenuPermission(DataGrid Dtgrid, string rateName)
        {
            if (Dtgrid.ItemsSource != null)
            {
                foreach (object obj in Dtgrid.ItemsSource)
                {
                    //T_SYS_ENTITYMENU menu = new T_SYS_ENTITYMENU();

                    if (Dtgrid.Columns[0].GetCellContent(obj) != null)
                    {
                        //menu = Dtgrid.Columns[0].GetCellContent(obj).DataContext as T_SYS_ENTITYMENU;
                        //tmpAllList += menu.T_SYS_ENTITYMENU2.ENTITYMENUID + ",";
                        string StrMenuID = ""; //菜单ID
                        menu = Dtgrid.Columns[0].GetCellContent(obj).DataContext as V_MenuSetRole;
                        StrMenuID = menu.ENTITYMENUID;
                        string StrPermissionID = "";//权限ID
                        int PermCount = 0;
                        PermCount = tmpPermission.Count;
                        int IndexCount = 1;
                        //IndexCount = PermCount
                        bool IsCheckRange = false;//是否选择了权限范围
                        for (int i = 1; i < PermCount + 1; i++)
                        {
                            IndexCount = IndexCount + i;
                            if (Dtgrid.Columns[i].GetCellContent(obj) != null)
                            {
                                string NewDataRange = "";
                                /* 
                                 * 首先根据菜单ID和角色ID获取  角色菜单表的记录 存在则获取ROLEENTITYID
                                 * 然后再根据 ROLEENTITYID和权限ID 获取角色菜单权限表中的记录，取得对应的权限和现在的权限值比较
                                 * 如果相同则不处理  不同则处理
                                 */
                                StrPermissionID = tmpPermission[i - 1].PERMISSIONID;  //权限ID
                                //var q = from a in EntityPermissionInfosList//权限视图集合
                                //        where a.EntityRole.T_SYS_ENTITYMENU.ENTITYMENUID == StrMenuID
                                //        && a.EntityRole.T_SYS_ROLE.ROLEID == tmprole.ROLEID
                                //        select a;
                                var q = from a in EntityPermissionInfosListSecond//权限视图集合
                                        where a.EntityMenuID == StrMenuID
                                        && a.RoleID == tmprole.ROLEID
                                        select a;
                                string RoleEntityID = "";
                                if (q.Count() > 0)
                                {
                                    RoleEntityID = q.ToList().FirstOrDefault().RoleEntityMenuID.ToString(); //获取角色菜单ID
                                }
                                //var m = from a in EntityPermissionInfosList
                                //var k = from b in EntityPermissionInfosList//权限视图集合
                                //        where b.RoleMenuPermission.T_SYS_PERMISSION.PERMISSIONID == StrPermissionID && b.RoleMenuPermission.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == RoleEntityID
                                //        select b;
                                var k = from b in EntityPermissionInfosListSecond//权限视图集合
                                        where b.PermissionID == StrPermissionID && b.RoleEntityMenuID == RoleEntityID
                                        select b;
                                string OldRangeValue = ""; //获取数据库中 权限对应的值

                                if (k.Count() > 0)
                                {
                                    OldRangeValue = k.ToList().FirstOrDefault().PermissionDataRange.ToString();
                                }
                                //Rating hrrate = DaGrHR.Columns[i].GetCellContent(obj).FindName("HRrating") as Rating;
                                Button hrrate = Dtgrid.Columns[i].GetCellContent(obj).FindName(rateName) as Button;

                                switch (hrrate.Content.ToString())
                                {
                                    case "★"://员工 0.2 ×10
                                        NewDataRange = "4";
                                        break;
                                    case "★★"://岗位 0.4×10
                                        NewDataRange = "3";
                                        break;
                                    case "★★★"://部门 0.6*10
                                        NewDataRange = "2";
                                        break;
                                    case "★★★★"://公司 0.8*10
                                        NewDataRange = "1";
                                        break;
                                    //case "★★★★★"://集团 1.0*10
                                    //    NewDataRange = "0";
                                    //    break;
                                    case ""://权限
                                        NewDataRange = "";
                                        break;
                                }
                                if (OldRangeValue != NewDataRange)
                                {
                                    tmpAllList += NewDataRange + ",";
                                    tmpAllList += tmpPermission[i - 1].PERMISSIONID + ";";
                                    IsCheckRange = true;
                                }
                            }
                        }
                        if (IsCheckRange)
                        {
                            tmpAllList += "@" + StrMenuID + "," + "";
                            tmpAllList += "#";
                        }
                    }
                }

                if (IsAdd)
                {
                    if (string.IsNullOrEmpty(tmpAllList))
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请先设置权限", Utility.GetResourceStr("CONFIRMBUTTON"));
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        return;
                    }

                }
            }
        }

        /// <summary>
        /// HR 授权
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HRrating_Click(object sender, RoutedEventArgs e)
        {
            Button Txtrating = sender as Button;
            SetPermissionRate(Txtrating);
        }

        /// <summary>
        /// 选中当前行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HRBtn_Click(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            MenuSetPermissionRate(Btn,DaGrHR,"HRrating");

        }

        private void DaGrHR_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Button myhrBtn = DaGrHR.Columns[0].GetCellContent(e.Row).FindName("HRBtn2") as Button;
            myhrBtn.Tag = e;
        }

    }
}
