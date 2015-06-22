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

namespace SMT.SaaS.Permission.UI.UserControls
{

    public partial class SysRoleSetMenuPermisstion : UserControl, IEntityEditor
    {
        /// <summary>
        /// FB 数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetFBSysMenuByTypeCompleted(object sender, GetFBSysMenuByTypeCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    FBMenu = e.Result.ToList();
                }

            }
            //开始获取物流系统菜单
            tabfb.IsEnabled = true;//加载完菜单后启用FB标签
            RoleClient.GetLMSysMenuByTypeAsync("2");
        }

        private void DaGrFB_Loaded(object sender, RoutedEventArgs e)
        {
            FillPermissionDataRange(DaGrFB, "myChkBtnFB", "FBrating");
        }

        /// <summary>
        /// 权限按钮设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FBrating_Click(object sender, RoutedEventArgs e)
        {
            Button Txtrating = sender as Button;
            SetPermissionRate(Txtrating);
        }

        /// <summary>
        /// 单击1行选中当前行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FBBtn_Click(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            MenuSetPermissionRate(Btn, DaGrFB, "FBrating");
        }

        private void DaGrFB_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Button myoaBtn = DaGrFB.Columns[0].GetCellContent(e.Row).FindName("FBBtn2") as Button;
            myoaBtn.Tag = e;

        }

        #region 检查预算菜单的权限
        
        
        /// <summary>
        /// 简称预算菜单的权限
        /// </summary>
        /// <param name="Dtgrid"></param>
        /// <param name="rateName"></param>
        private string CheckFbMenuPermission(DataGrid Dtgrid, string rateName)
        {
            string strReturn = string.Empty;
            //if (Dtgrid.ItemsSource != null)
            //{
            //    FbMenuPermission fbMenuDataRange = new FbMenuPermission();
            //    foreach (object obj in Dtgrid.ItemsSource)
            //    {                    
            //        if (Dtgrid.Columns[0].GetCellContent(obj) != null)
            //        {
            //            string StrMenuID = ""; //菜单ID
            //            menu = Dtgrid.Columns[0].GetCellContent(obj).DataContext as V_MenuSetRole;
            //            StrMenuID = menu.ENTITYMENUID;
            //            var ents = from ent in listFbPermissions
            //                       where ent.menuID == StrMenuID
            //                       select ent;
            //            if (ents.Count() > 0)
            //            {
            //                fbMenuDataRange = ents.FirstOrDefault();
            //                string StrPermissionID = "";//权限ID
            //                int PermCount = 0;
            //                PermCount = tmpPermission.Count;
            //                int IndexCount = 1;                            
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
            //                        //var q = from a in EntityPermissionInfosList//权限视图集合
            //                        //        where a.EntityRole.T_SYS_ENTITYMENU.ENTITYMENUID == StrMenuID
            //                        //        && a.EntityRole.T_SYS_ROLE.ROLEID == tmprole.ROLEID
            //                        //        select a;
            //                        var q = from a in EntityPermissionInfosListSecond//权限视图集合
            //                                where a.EntityMenuID == StrMenuID
            //                                && a.RoleID == tmprole.ROLEID
            //                                select a;
            //                        string RoleEntityID = "";
            //                        if (q.Count() > 0)
            //                        {
            //                            RoleEntityID = q.ToList().FirstOrDefault().RoleEntityMenuID.ToString(); //获取角色菜单ID
            //                        }                                    
            //                        var k = from b in EntityPermissionInfosListSecond//权限视图集合
            //                                where b.PermissionID == StrPermissionID && b.RoleEntityMenuID == RoleEntityID
            //                                select b;
            //                        string OldRangeValue = ""; //获取数据库中 权限对应的值

            //                        if (k.Count() > 0)
            //                        {
            //                            OldRangeValue = k.ToList().FirstOrDefault().PermissionDataRange.ToString();
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
            //                        if (!string.IsNullOrEmpty(NewDataRange))
            //                        {
            //                            if (!string.IsNullOrEmpty(fbMenuDataRange.permissionVlaue))
            //                            {
            //                                int fbValue = System.Convert.ToInt16(fbMenuDataRange.permissionVlaue);
            //                                int newValue = System.Convert.ToInt16(NewDataRange);
            //                                if (newValue > fbValue)                                            
            //                                {
            //                                    strReturn = fbMenuDataRange.menuName + "必须设为：" + fbMenuDataRange.permissionText + "权限";
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
                
            //}
            return strReturn;
        }
        #endregion
    }
}
