using System;
using System.Windows;
using System.Windows.Controls;

namespace SMT.HRM.UI
{
    public partial class BasePage : Page
    {
        #region "显示和隐藏进度条"
        protected MainPage CurrentMainPage
        {
            get
            {
                Grid grid = Application.Current.RootVisual as Grid;
                if (grid != null && grid.Children.Count > 0)
                {
                    return grid.Children[0] as MainPage;
                }
                else
                    return null;
            }
        }
        public void ShowPageStyle()
        {
            //  CurrentMainPage.ShowWaitingControl();
        }
        public void HidePageStyle()
        {
            // CurrentMainPage.HideWaitingControl();
        }
        #endregion

        #region"验证登录信息"
        //通过Permission项目提供的WCf服务验证
        //protected Identity CurrentIdentity = null;
        //protected override void OnInit(EventArgs e)
        //{
        //    if (HttpContext.Current.User.Identity.Name.Trim() != string.Empty)
        //    {
        //        AuthenticateModule.SetPrincipal(HttpContext.Current.User.Identity.Name);
        //        CurrentIdentity = (Identity)Principal.Current.Identity;
        //    }
        //    if (CurrentIdentity == null)
        //    {
        //        Response.Redirect("~/login.aspx", true);
        //    }
        //    base.OnInit(e);
        //}
        #endregion

        #region"验证Form，View权限"

        #endregion
                
        public string EntityLogo { get; set; }

        public void GetEntityLogo(string entityCode)
        {
            SMT.Saas.Tools.PermissionWS.PermissionServiceClient client = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
            client.GetSysMenuByEntityCodeCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysMenuByEntityCodeCompletedEventArgs>(client_GetSysMenuByEntityCodeCompleted);
            client.GetSysMenuByEntityCodeAsync(entityCode);

        }

        public void SetRowLogo(DataGrid DtGrid, DataGridRow row, string entityCode)
        {
            if (DtGrid.ItemsSource == null)
            {
                return;
            }

            Image logo = DtGrid.Columns[0].GetCellContent(row).FindName("entityLogo") as Image;
            if (logo == null)
            {
                return;
            }

            if (Application.Current.Resources["RowLogo" + entityCode] != null)
            {
                string strPpath = Application.Current.Resources["RowLogo" + entityCode].ToString();
                logo.Margin = new Thickness(2, 2, 0, 0);
                logo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(strPpath, UriKind.Relative));
                return;
            }

            SMT.Saas.Tools.PermissionWS.PermissionServiceClient client = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
            client.GetSysMenuByEntityCodeCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysMenuByEntityCodeCompletedEventArgs>(client_GetSysMenuByEntityCodeCompleted);
            client.GetSysMenuByEntityCodeAsync(entityCode);

            //if (string.IsNullOrEmpty(EntityLogo))
            //{
            //    SMT.Saas.Tools.PermissionWS.PermissionServiceClient client = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
            //    client.GetSysMenuByEntityCodeCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysMenuByEntityCodeCompletedEventArgs>(client_GetSysMenuByEntityCodeCompleted);
            //    client.GetSysMenuByEntityCodeAsync(entityCode, logo);
            //}
            //else
            //{
            //    logo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(EntityLogo, UriKind.Relative));
            //}
        }

        void client_GetSysMenuByEntityCodeCompleted(object sender, SMT.Saas.Tools.PermissionWS.GetSysMenuByEntityCodeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            if (e.Result == null)
            {
                return;
            }

            SMT.Saas.Tools.PermissionWS.T_SYS_ENTITYMENU menu = e.Result;

            if (string.IsNullOrWhiteSpace(menu.MENUICONPATH))
            {
                return;
            }

            if (Application.Current.Resources["RowLogo" + menu.ENTITYCODE] == null)
            {
                Application.Current.Resources.Add("RowLogo" + menu.ENTITYCODE, menu.MENUICONPATH);
            }

            //Image logo = new Image();
            //EntityLogo = menu.MENUICONPATH;

            //logo = e.UserState as Image;

            //if (logo == null)
            //{
            //    return;
            //}

            //logo.Margin = new Thickness(2, 2, 0, 0);
            //logo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(menu.MENUICONPATH, UriKind.Relative));
            
        }

        public string GetTitleFromURL(string url)
        {
            System.Text.StringBuilder rslt = new System.Text.StringBuilder();

            string[] tmparray = url.Split('/');
            foreach (string tmp in tmparray)
            {
                if (string.IsNullOrEmpty(tmp.Trim()))
                    continue;

                if (rslt.Length > 0)
                    rslt.Append(">>");

                rslt.Append(Utility.GetResourceStr(tmp.ToUpper()));
            }
            return rslt.ToString();
        }

    }
}
