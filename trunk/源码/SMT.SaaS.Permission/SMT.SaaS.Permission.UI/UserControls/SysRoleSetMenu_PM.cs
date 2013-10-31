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


        private void GetPMDataInfos()
        {
            RoleClient.GetPMSysMenuByTypeCompleted += new EventHandler<GetPMSysMenuByTypeCompletedEventArgs>(RoleClient_GetPMSysMenuByTypeCompleted);
            RoleClient.GetPMSysMenuByTypeAsync("13");
        }


        void RoleClient_GetPMSysMenuByTypeCompleted(object sender, GetPMSysMenuByTypeCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    PMMenu = e.Result.ToList();

                    DataGridBindingPcv(DaGrPM, PMMenu);
                    RoleClient.GetRoleEntityIDListInfosByRoleIDNewAsync(tmprole.ROLEID);
                    this.tabpm.IsEnabled = true;
                }
                else
                {
                    RoleClient.GetRoleEntityIDListInfosByRoleIDNewAsync(tmprole.ROLEID);
                }

            }
            //loadbar.Stop();
        }


        //PM
        void LoadDaGrPMDataRange()
        {
            DataGridColumnsAdd(DaGrPM, "myPMCellTemplate");
        }


        private void DaGrPM_Loaded(object sender, RoutedEventArgs e)
        {
            FillPermissionDataRange(DaGrPM, "myChkBtnPM", "PMrating");
        }

        private void DaGrPM_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Button myoaBtn = DaGrPM.Columns[0].GetCellContent(e.Row).FindName("PMBtn") as Button;
            myoaBtn.Tag = e;            
        }

        /// <summary>
        /// 权限按钮设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PMrating_Click(object sender, RoutedEventArgs e)
        {
            Button PMrating = sender as Button;

            string StrContent = "";
            StrContent = PMrating.Content.ToString();

            switch (StrContent)
            {
                case "":
                    StrContent = "★";
                    break;
                case "★":
                    StrContent = "★★";
                    break;
                case "★★":
                    StrContent = "★★★";
                    break;
                case "★★★":
                    StrContent = "★★★★";
                    break;
                //case "★★★★":
                //    StrContent = "★★★★★";
                //    break;
                case "★★★★":
                    StrContent = "";
                    break;
            }
            PMrating.Content = StrContent;
        }

        /// <summary>
        /// 单击1行选中当前行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PMBtn_Click(object sender, RoutedEventArgs e)
        {
            Button BtnPM = sender as Button;
            if (BtnPM.Tag == null)
            {
                MessageBox.Show("无法选择此行！");
                return;
            }
            DataGridRowEventArgs PMrating = BtnPM.Tag as DataGridRowEventArgs;
            string StrContent = "";
            int PermCount = 0;
            PermCount = tmpPermission.Count;

            for (int i = 1; i < PermCount + 1; i++)
            {
                Button mybtn = DaGrPM.Columns[i].GetCellContent(PMrating.Row).FindName("PMrating") as Button;
                StrContent = mybtn.Content.ToString();

                switch (StrContent)
                {
                    case "":
                        StrContent = "★";
                        break;
                    case "★":
                        StrContent = "★★";
                        break;
                    case "★★":
                        StrContent = "★★★";
                        break;
                    case "★★★":
                        StrContent = "★★★★";
                        break;
                    //case "★★★★":
                    //    StrContent = "★★★★★";
                    //    break;
                    case "★★★★":
                        StrContent = "";
                        break;
                }
                mybtn.Content = StrContent;
            }
        }



        private void myChkBtnPM_Click(object sender, RoutedEventArgs e)
        {

        }
        
    }
}
