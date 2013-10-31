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
        /// oa 数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetOASysMenuByTypeCompleted(object sender, GetOASysMenuByTypeCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    OAMenu = e.Result.ToList();
                    DataGridBindingPcv(DaGrOA,OAMenu);
                    //d.ItemsSource = pcv;
                }
            }
            //开始获取FB系统菜单
            taboa.IsEnabled = true;//加载完菜单后启用OA标签
            RoleClient.GetFBSysMenuByTypeAsync("3",Common.CurrentLoginUserInfo.EmployeeID);
        }

        


        private void DaGrOA_Loaded(object sender, RoutedEventArgs e)
        {
            FillPermissionDataRange(DaGrOA, "myChkBtn", "OArating");
        }

         ///<summary>
         ///权限按钮设置
         ///</summary>
         ///<param name="sender"></param>
         ///<param name="e"></param>
        private void OArating_Click(object sender, RoutedEventArgs e)
        {
            Button Txtrating = sender as Button;
            SetPermissionRate(Txtrating);
        }
        /// <summary>
        /// 单击1行选中当前行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OABtn_Click(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            MenuSetPermissionRate(Btn, DaGrOA, "OArating");

        }
        private void DaGrOA_LoadingRow(object sender, DataGridRowEventArgs e)
        {            
            Button myoaBtn = DaGrOA.Columns[0].GetCellContent(e.Row).FindName("OABtn2") as Button;
            myoaBtn.Tag = e;            
        }
        
    }
}
