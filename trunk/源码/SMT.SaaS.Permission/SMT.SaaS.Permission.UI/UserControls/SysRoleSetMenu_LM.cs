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
        /// 物流菜单获取完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetLMSysMenuByTypeCompleted(object sender, GetLMSysMenuByTypeCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    LMMenu = e.Result.ToList();
                }
            }
            //获取进销存的菜单
            tablm.IsEnabled = true;//加载完菜单后启用LM标签
            RoleClient.GetEDMSysMenuByTypeAsync("10");
            
        }

        private void DaGrLM_Loaded(object sender, RoutedEventArgs e)
        {
            FillPermissionDataRange(DaGrLM, "myChkBtnLM", "LMrating");
        }
        /// <summary>
        /// 权限按钮设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LMrating_Click(object sender, RoutedEventArgs e)
        {
            Button Txtrating = sender as Button;
            SetPermissionRate(Txtrating);
        }

        /// <summary>
        /// 单击1行选中当前行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LMBtn_Click(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;
            MenuSetPermissionRate(Btn, DaGrLM, "LMrating");
        }

        private void DaGrLM_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Button mylmBtn = DaGrLM.Columns[0].GetCellContent(e.Row).FindName("LMBtn2") as Button;
            mylmBtn.Tag = e;
        }
        
    }
}
