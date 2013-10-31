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
    public partial class SysRoleSetMenu2 : UserControl, IEntityEditor
    {
        /// <summary>
        /// oa 数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetEDMSysMenuByTypeCompleted(object sender, GetEDMSysMenuByTypeCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    EDMMenu = e.Result.ToList();
                    DataGridBindingPcv(DaGrEDM, EDMMenu);
                    //d.ItemsSource = pcv;
                }
            }
            //获取当前角色所有的实体菜单权限
            tabedm.IsEnabled = true;//加载完菜单后启用EDM标签
            //RoleClient.GetRoleEntityIDListInfosByRoleIDAsync(tmprole.ROLEID);
            RoleClient.GetPMSysMenuByTypeAsync("13");
            
            
        }




        private void DaGrEDM_Loaded(object sender, RoutedEventArgs e)
        {
            FillPermissionDataRange(DaGrEDM, "myChkBtn", "EDMrating");
        }

        ///<summary>
        ///权限按钮设置
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        private void EDMrating_Click(object sender, RoutedEventArgs e)
        {
            Button Txtrating = sender as Button;
            SetPermissionRate(Txtrating);
        }
        /// <summary>
        /// 单击1行选中当前行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EDMBtn_Click(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            MenuSetPermissionRate(Btn, DaGrEDM, "EDMrating");

        }
        
        private void DaGrEDM_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Button myoaBtn = DaGrEDM.Columns[0].GetCellContent(e.Row).FindName("EDMBtn2") as Button;
            myoaBtn.Tag = e;
        }
    }
}
