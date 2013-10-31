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
    }
}
