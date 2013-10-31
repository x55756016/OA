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
using System.Windows.Navigation;
using System.Windows.Interop;
//using SMT.Saas.Tools.PersonnelWS;
using System.Windows.Browser;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.Helper;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.Permission.UI
{
    public partial class MainPage : UserControl
    {
        private PermissionServiceClient permClient = new PermissionServiceClient();
        private ObservableCollection<string> MenuInfosList = new ObservableCollection<string>();
        public MainPage()
        {

            InitializeComponent();
            //this.loginContainer.Child = new LoginStatus();
            //initLeftMenu();
            HeaderMenu.HBFullScreen.Click += new RoutedEventHandler(HBFullScreen_Click);            
            permClient.GetSysLeftMenuFilterPermissionCompleted += new EventHandler<GetSysLeftMenuFilterPermissionCompletedEventArgs>(permClient_GetSysLeftMenuFilterPermissionCompleted);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            

            #region 源代码
            //InitializeComponent();

            //HeaderMenu.HBFullScreen.Click += new RoutedEventHandler(HBFullScreen_Click);

            //HeaderMenu.HBLoginOut.Click += new RoutedEventHandler(HBLoginOut_Click);
            //this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            //HeaderMenu.SetUserNameAndDepartmentName("公司", "部门", "岗位", "员工"); 
            #endregion
        }

        void permClient_GetSysLeftMenuFilterPermissionCompleted(object sender, GetSysLeftMenuFilterPermissionCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                List<T_SYS_ENTITYMENU> list = new List<T_SYS_ENTITYMENU>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                MenuInfosList = e.menuids;
                initLeftMenu(list);
            }
                        
        }

        
        private void initLeftMenu(List<T_SYS_ENTITYMENU> menulist)
        {
            #region 源代码备份
            //StackPanel menuTemp = new StackPanel();
            //menuTemp.Margin = new Thickness(0, 1, 0, 1);
            //leftMenu.MenuRoot.Children.Add(menuTemp);

            //HyperlinkButton btnTest = new HyperlinkButton();
            //btnTest.Height = 22;
            //btnTest.FontSize = 13.333;
            //btnTest.FontWeight = FontWeights.Bold;
            //btnTest.Content = "     动态菜单";
            //btnTest.Style = new Style(typeof(HyperlinkButton));
            //Link3.NavigateUri=null;
            //btnTest.NavigateUri = new Uri("/SMT.SaaS.OA.UI;component/Views/Home.xaml", System.UriKind.Relative); //("//Home.xaml");           
            //btnTest.TargetName = "contenxtPanle";
            //btnTest.Click += new RoutedEventHandler(delegate
            //{
            //    ContentFrame.Navigate(new Uri("/Home", UriKind.Relative));
            //});
            //menuTemp.Children.Add(btnTest);
            //btnTest.Style = new Style();
            #endregion

            if (toolkitacc.Items != null)
                toolkitacc.Items.Clear();

            //生成分组
            var groupItems = from m in menulist
                             where m.T_SYS_ENTITYMENU2Reference.EntityKey == null
                             orderby m.ORDERNUMBER
                             select m;

            foreach (var item in groupItems)
            {
                AccordionItem group = new AccordionItem();
                group.Header = item;

                group.Style = this.Resources["TreeMenuGroupStyle"] as Style;
                group.BorderThickness = new Thickness(0);
                group.FontSize = 14.0;

                if (UIHelper._CurrentStyleCode_2 == 1)
                {
                    group.Foreground = Application.Current.Resources["TextBBlue1"] as SolidColorBrush;
                }

                group.VerticalContentAlignment = VerticalAlignment.Stretch;
                group.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                group.VerticalAlignment = VerticalAlignment.Stretch;
                group.HorizontalAlignment = HorizontalAlignment.Stretch;

                //生成菜单明细
                var menuItems = from m in menulist
                                where m.T_SYS_ENTITYMENU2Reference.EntityKey !=null
                                && m.T_SYS_ENTITYMENU2Reference.EntityKey.EntityKeyValues[0].Value.ToString() == item.ENTITYMENUID
                                orderby m.ORDERNUMBER
                                select m;

                TransitioningContentControl ctrl = new TransitioningContentControl();

                StackPanel pnl = new StackPanel();
                pnl.VerticalAlignment = VerticalAlignment.Stretch;
                pnl.HorizontalAlignment = HorizontalAlignment.Stretch;
                pnl.Margin = new Thickness(0, 0, 0, 0);

                TreeView tree = new TreeView();
                tree.Style = (Style)Application.Current.Resources["TreeViewStyle"];
                tree.BorderThickness = new Thickness(0);
                tree.HorizontalAlignment = HorizontalAlignment.Stretch;
                tree.Width = toolkitacc.ActualWidth;
                tree.Margin = new Thickness(0);

                foreach (var menu in menuItems)
                {
                    TreeViewItem treeItem = CreateTreeItem(menu);
                    AddSubMenu(menulist, treeItem, menu);
                    tree.Items.Add(treeItem);
                }

                ctrl.Content = tree;
                group.Content = ctrl;
                toolkitacc.Items.Add(group);
            }

        }

        #region 创建左侧导航树

        private TreeViewItem CreateTreeItem(T_SYS_ENTITYMENU menu)
        {
            TreeViewItem treeItem = new TreeViewItem();
            treeItem.Header = menu;
            treeItem.Tag = menu;
            treeItem.DataContext = menu;
            treeItem.Margin = new Thickness(0);

            treeItem.Style = this.Resources["TreeMenuItemStyle"] as Style;

            treeItem.Selected += new RoutedEventHandler(treeItem_Selected);

            return treeItem;
        }

        void treeItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeItem = sender as TreeViewItem;
            if (treeItem == null)
                return;

            T_SYS_ENTITYMENU menu = treeItem.Tag as T_SYS_ENTITYMENU;

            if (menu == null)
                return;

            if (menu.URLADDRESS != null)
            {
                ContentFrame.Navigate(new Uri(menu.URLADDRESS, UriKind.Relative));
            }
        }

        private void AddSubMenu(List<T_SYS_ENTITYMENU> menulist, TreeViewItem parentItem, T_SYS_ENTITYMENU menu)
        {
            var menuItems = from m in menulist
                            where m.T_SYS_ENTITYMENU2 != null && m.T_SYS_ENTITYMENU2.ENTITYMENUID == menu.ENTITYMENUID
                            orderby m.ORDERNUMBER
                            select m;
            if (menuItems == null || menuItems.Count() <= 0)
                return;

            foreach (var subMenu in menuItems)
            {

                TreeViewItem treeItem = CreateTreeItem(subMenu);

                parentItem.Items.Add(treeItem);

                AddSubMenu(menulist, treeItem, subMenu);
            }
        }

        void permClient_GetSysLeftMenuCompleted(object sender, GetSysLeftMenuCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                List<T_SYS_ENTITYMENU> list = new List<T_SYS_ENTITYMENU>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                initLeftMenu(list);
            }
        }

        #endregion

        void HBLoginOut_Click(object sender, RoutedEventArgs e)
        {
            if (HtmlPage.Window.Confirm("确定退出系统吗?"))
            {
                PermissionServiceClient PermClient = new PermissionServiceClient();
                PermClient.SysUserLoginRecordInfoUpdateCompleted += new EventHandler<SysUserLoginRecordInfoUpdateCompletedEventArgs>(PermClient_SysUserLoginRecordInfoUpdateCompleted);
                //Common.CurrentConfig.CurrentEmpploy.UserLoginRecord.ONLINESTATE = "0";
                //Common.CurrentConfig.CurrentUser.Curre
                //PermClient.SysUserLoginRecordInfoUpdateAsync(Common.CurrentConfig.CurrentUser.CurrentEmpploy.UserLoginRecord);
                //PermClient.SysUserLoginRecordInfoUpdateAsync(Common.CurrentConfig.CurrentUser.UserLoginRecord);
                
            }
        }

        void PermClient_SysUserLoginRecordInfoUpdateCompleted(object sender, SysUserLoginRecordInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    T_SYS_USERLOGINRECORDHIS history = new T_SYS_USERLOGINRECORDHIS();
                    T_SYS_USERLOGINRECORD LoginRecord = new T_SYS_USERLOGINRECORD();
                    //LoginRecord = Common.CurrentConfig.CurrentEmpploy.UserLoginRecord;
                    //LoginRecord = Common.CurrentConfig.lo
                    history.LOGINRECORDHISID = System.Guid.NewGuid().ToString();
                    history.LOGINDATE = LoginRecord.LOGINDATE;
                    history.LOGINIP = LoginRecord.LOGINIP;
                    history.ONLINESTATE = "0";
                    history.REMARK = "";
                    history.USERNAME = LoginRecord.USERNAME;
                    history.LOGINTIME = LoginRecord.LOGINTIME;
                    PermissionServiceClient PermClient = new PermissionServiceClient();
                    PermClient.SysUserLoginHistoryRecordInfoAddCompleted += new EventHandler<SysUserLoginHistoryRecordInfoAddCompletedEventArgs>(PermClient_SysUserLoginHistoryRecordInfoAddCompleted);
                    PermClient.SysUserLoginHistoryRecordInfoAddAsync(history);
                }
            }
        }

        void PermClient_SysUserLoginHistoryRecordInfoAddCompleted(object sender, SysUserLoginHistoryRecordInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    //Common.CurrentConfig.CurrentUser = null;
                    
                    //Common.CurrentConfig.CurrentEmpploy = null;
                    //Common.CurrentConfig.CurrentUser = null;
                    //Common.CurrentConfig.CurrentPermissions = null;
                    HtmlWindow html = HtmlPage.Window;
                    html.Navigate(new Uri("http://localhost:1604/PermissionService.svc"));
                    //html.Navigate(new Uri("/Views/SysRole"));
                }
            }
        }
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderMenu.HBPervious.Click += new RoutedEventHandler(HBPervious_Click);
            
            HeaderMenu.HBNext.Click += new RoutedEventHandler(HBNext_Click);
            //HeaderMenu.SetUserNameAndDepartmentName(
            //    Common.CurrentConfig.CurrentEmpploy.EmployeeInfo.EMPLOYEECNAME,
            //    Common.CurrentConfig.CurrentEmpploy.EmployeeInfo.T_HR_EMPLOYEE.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
            //HeaderMenu.SetUserNameAndDepartmentName(Common.CurrentConfig.CurrentEmpploy.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME,Common.CurrentConfig.CurrentEmpploy.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
            System.Windows.Controls.Window.Parent = windowParent;
            System.Windows.Controls.Window.TaskBar = new StackPanel();
            System.Windows.Controls.Window.Wrapper = this;
        }

        void HBNext_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoForward)
            {
                ContentFrame.GoForward();
            }
        }

        void HBPervious_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        void HBFullScreen_Click(object sender, RoutedEventArgs e)
        {
            Content contentObject = Application.Current.Host.Content;
            contentObject.IsFullScreen = !contentObject.IsFullScreen;
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Handles beforSpinnerShowShowBorder.Completed event.
        /// </summary>
        private void beforSpinnerShowShowBorder_Completed(object sender, EventArgs e)
        {
            //spinnerBackShowBorder.Stop();
            //spinnerShowBorder.Stop();
            //spinnerBackShowBorder.Begin();
            //spinnerShowBorder.Begin();
        }

        private void ProgressBar_Cancel(object sender, EventArgs e)
        {
            if (CancelWaiting != null)
            {
                CancelWaiting(this, EventArgs.Empty);
            }
            HideWaitingControl();
        }

        /// <summary>
        /// Hides waiting controls.
        /// </summary>
        public void HideWaitingControl()
        {
            //waitSpinner.Stop();
            //waitSpinnerBack.Visibility = Visibility.Collapsed;
            //waitSpinner.Visibility = Visibility.Collapsed;
            //spinnerBackShowBorder.Stop();
            //spinnerShowBorder.Stop();
            //beforSpinnerShowShowBorder.Stop();
        }

        /// <summary>
        ///     If an error occurs during navigation, show an error window
        /// </summary>
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            ChildWindow errorWin = new ErrorWindow(e.Uri);
            errorWin.Show();
        }


        /// <summary>
        /// Occurs when waiting is canceled
        /// </summary>
        public event EventHandler CancelWaiting;

        //获取当前页面导航
        private string PageNavgation;

        /// <summary>
        ///     After the Frame navigates, ensure the <see cref="HyperlinkButton"/> representing the current page is selected
        /// </summary>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            PageNavgation = e.Uri.ToString();
            
            if (ContentFrame.CanGoBack == false)
            {
                HeaderMenu.HBPervious.Opacity = 0.3;
            }
            else
            {
                HeaderMenu.HBPervious.Opacity = 1.0;
            }
            if (ContentFrame.CanGoForward == false)
            {
                HeaderMenu.HBNext.Opacity = 0.3;
            }
            else
            {
                HeaderMenu.HBNext.Opacity = 1.0;
            }
            //HeaderMenu.SetUserNameAndDepartmentName(Common.CurrentConfig.CurrentEmpploy.EmployeeInfo.EMPLOYEEENAME, Common.CurrentConfig.CurrentEmpploy.EmployeeInfo.T_HR_EMPLOYEE.EMPLOYEEPOSTID);
        }

        /// <summary>
        /// Calculates wait spinner size and location.
        /// </summary>
        private void CalculatesSpinner()
        {
            //waitSpinnerBack.Width = LayoutRoot.ActualWidth;
            //waitSpinnerBack.Height = LayoutRoot.ActualHeight;
            //waitSpinner.SetValue(Canvas.TopProperty, waitSpinnerBack.Height / 2 - waitSpinner.ActualHeight / 2 - 300);
            //waitSpinner.SetValue(Canvas.LeftProperty, waitSpinnerBack.Width / 2 - waitSpinner.ActualWidth / 2 - 500);
        }
        
        /// <summary>
        /// Shows waiting controls.
        /// </summary>
        public void ShowWaitingControl()
        {
            CalculatesSpinner();
            
       
        }

        private void Menu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            toolkitacc.Height = ((Border)sender).ActualHeight;
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //permClient.GetSysLeftMenuAsync("7", Common.CurrentLoginUserInfo.SysUserID);
            permClient.GetSysLeftMenuFilterPermissionAsync("7", Common.CurrentLoginUserInfo.SysUserID, MenuInfosList);
            string flag = "";
            permClient.GetEntityMenuByUserAsync("1", Common.CurrentLoginUserInfo.SysUserID, flag);
            permClient.GetEntityMenuByUserAsync("7", Common.CurrentLoginUserInfo.SysUserID, flag);
            HeaderMenu.HBPervious.Click += new RoutedEventHandler(HBPervious_Click);

            HeaderMenu.HBNext.Click += new RoutedEventHandler(HBNext_Click);
            HeaderMenu.HBFullScreen.Click += new RoutedEventHandler(HBFullScreen_Click);
        }

        public void NavigateTo(Uri uri)
        {
            ContentFrame.Navigate(uri);
        }
    }
}
