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
using System.Runtime.InteropServices;
//using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PermissionWS;
//using SMT.Saas.Tools.PersonnelWS;

namespace SMT.SaaS.OA.UI
{
    public partial class BasePage : Page
    {
        #region 检查资源是否加载
        /// <summary>
        /// 检查转换器资源是否加载
        /// </summary>
        private void CheckResourceConverter()
        {

            //if (Application.Current.Resources["FLOW_MODELDEFINE_T"] == null)
            //{
            //    Utility.LoadDictss();
            //}

            if (Application.Current.Resources["GridHeaderConverter"] == null)
            {
                Application.Current.Resources.Add("GridHeaderConverter", new SMT.SaaS.Globalization.GridHeaderConverter());
            }

            if (Application.Current.Resources["ResourceConveter"] == null)
            {
                Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            }

            if (Application.Current.Resources["DictionaryConverter"] == null)
            {
                Application.Current.Resources.Add("DictionaryConverter", new SMT.SaaS.OA.UI.DictionaryConverter());
            }

            if (Application.Current.Resources["CustomDateConverter"] == null)
            {
                Application.Current.Resources.Add("CustomDateConverter", new SMT.SaaS.OA.UI.CustomDateConverter());
            }
            if (Application.Current.Resources["StateConvert"] == null)
            {
                Application.Current.Resources.Add("StateConvert", new SMT.SaaS.OA.UI.CheckStateConverter());
            }
            if (Application.Current.Resources["RentConvert"] == null)
            {
                Application.Current.Resources.Add("RentConvert", new SMT.SaaS.OA.UI.RentFlagConverter());
            }
            if (!Application.Current.Resources.Contains("ModuleNameConverter"))
            {
                Application.Current.Resources.Add("ModuleNameConverter", new SMT.SaaS.OA.UI.ModuleNameConverter());
            }
            if (!Application.Current.Resources.Contains("ObjectTypeConverter"))
            {
                Application.Current.Resources.Add("ObjectTypeConverter", new SMT.SaaS.OA.UI.ObjectTypeConverter());
            }
        }
        #endregion

        public string EntityLogo { get; set; }

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


        /// <summary>
        /// 获取实体Logo
        /// </summary>
        /// <param name="entityCode">实体标示</param>
        public void GetEntityLogo(string entityCode)
        {
            PermissionServiceClient client = new PermissionServiceClient();
            client.GetSysMenuByEntityCodeCompleted += new EventHandler<GetSysMenuByEntityCodeCompletedEventArgs>(client_GetSysMenuByEntityCodeCompleted);
            client.GetSysMenuByEntityCodeAsync(entityCode);

        }


        /// <summary>
        ///在没有获取图片时，重复调用，设置每一行的Logo图片，
        /// </summary>
        /// <param name="DtGrid"></param>
        /// <param name="row"></param>
        /// <param name="entityCode"></param>
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
            if (Application.Current.Resources["RowLogo" + menu.ENTITYCODE] == null)
            {
                Application.Current.Resources.Add("RowLogo" + menu.ENTITYCODE, menu.MENUICONPATH);
            }
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
