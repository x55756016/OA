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

using SMT.Saas.Tools.PermissionWS;
namespace SMT.SaaS.Permission.UI.Views
{
    public class BasePage : Page
    {
         protected MainPage CurrentMainPage
        {
            get
            {
                Grid grid = Application.Current.RootVisual as Grid;
                if (grid != null && grid.Children.Count>0)
                {
                    return grid.Children[0] as MainPage;
                }
                else
                    return null;
            }
        }

         #region 检查资源是否加载
         /// <summary>
         /// 检查转换器资源是否加载
         /// </summary>
         private void CheckResourceConverter()
         {             
             if (Application.Current.Resources["ResourceConveter"] == null)
             {
                 Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
             }

             if (Application.Current.Resources["DictionaryConverter"] == null)
             {
                 Application.Current.Resources.Add("DictionaryConverter", new SMT.SaaS.Permission.UI.DictionaryConverter());
             }

             
         }
         #endregion
        #region "显示和隐藏"
        public void ShowPageStyle()
        {
            if (CurrentMainPage != null)
                CurrentMainPage.ShowWaitingControl();
        }
        public void HidePageStyle()
        {
            if (CurrentMainPage != null)
                CurrentMainPage.HideWaitingControl();
      
        }
        #endregion

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

        #region 获取实体标示
        
        
        public string EntityLogo { get; set; }

        public void GetEntityLogo(string entityCode)
        {
            //SMT.Saas.Tools.PermissionWS.PermissionServiceClient client = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
            PermissionServiceClient client = new PermissionServiceClient();
            //client.GetSysMenuByEntityCodeCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysMenuByEntityCodeCompletedEventArgs>(client_GetSysMenuByEntityCodeCompleted);
            client.GetSysMenuByEntityCodeCompleted += new EventHandler<GetSysMenuByEntityCodeCompletedEventArgs>(client_GetSysMenuByEntityCodeCompleted);
            client.GetSysMenuByEntityCodeAsync(entityCode);
            //client.CloseAsync();//龙康才新增
            //client.Abort();//龙康才新增

        }

        void client_GetSysMenuByEntityCodeCompleted(object sender, GetSysMenuByEntityCodeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    T_SYS_ENTITYMENU menu = e.Result;
                    EntityLogo = menu.MENUICONPATH;

                    Image logo = e.UserState as Image;

                    if (logo != null)
                    {
                        logo.Margin = new Thickness(2, 2, 0, 0);
                        logo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(EntityLogo, UriKind.Relative));
                    }
                }
            }
        }

        public void SetRowLogo(DataGrid DtGrid, DataGridRow row, string entityCode)
        {
            if (DtGrid.ItemsSource != null)
            {
                Image logo = DtGrid.Columns[0].GetCellContent(row).FindName("entityLogo") as Image;
                if (logo != null)
                {
                    if (string.IsNullOrEmpty(EntityLogo))
                    {
                        PermissionServiceClient client = new PermissionServiceClient();
                        //SMT.Saas.Tools.PermissionWS.PermissionServiceClient client = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
                        client.GetSysMenuByEntityCodeCompleted +=new EventHandler<GetSysMenuByEntityCodeCompletedEventArgs>(client_GetSysMenuByEntityCodeCompleted);
                        client.GetSysMenuByEntityCodeAsync(entityCode, logo);                      
                    }
                    else
                    {
                        logo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(EntityLogo, UriKind.Relative));
                    }
                }
            }
        }

        //void client_GetSysMenuByEntityCodeCompleted(object sender, SMT.Saas.Tools.PermissionWS.GetSysMenuByEntityCodeCompletedEventArgs e)
        //{
           
        //}
        #endregion
    }
}
