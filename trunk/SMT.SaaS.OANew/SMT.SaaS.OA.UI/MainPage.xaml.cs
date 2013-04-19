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
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI
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
            AppConfig._CurrentStyleCode = 1;//默认为蓝色系

            HeaderMenu.HBFullScreen.Click += new RoutedEventHandler(HBFullScreen_Click);
            permClient.GetSysLeftMenuCompleted += new EventHandler<GetSysLeftMenuCompletedEventArgs>(permClient_GetSysLeftMenuCompleted);
            permClient.GetSysLeftMenuFilterPermissionCompleted += new EventHandler<GetSysLeftMenuFilterPermissionCompletedEventArgs>(permClient_GetSysLeftMenuFilterPermissionCompleted);

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

        }

        void permClient_GetSysLeftMenuFilterPermissionCompleted(object sender, GetSysLeftMenuFilterPermissionCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
            else
            {
                List<T_SYS_ENTITYMENU> list = new List<T_SYS_ENTITYMENU>();
                MenuInfosList = e.menuids;
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                initLeftMenu(list);
            }
        }




        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderMenu.HBPervious.Click += new RoutedEventHandler(HBPervious_Click);

            HeaderMenu.HBNext.Click += new RoutedEventHandler(HBNext_Click);

            System.Windows.Controls.Window.Parent = windowParent;
            System.Windows.Controls.Window.TaskBar = new StackPanel();
            System.Windows.Controls.Window.Wrapper = this;
            System.Windows.Controls.Window.IsShowtitle = false;
            //HeaderMenu.SetUserNameAndDepartmentName(
            //    Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEECNAME,
            //    Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
        }


        void HBFullScreen_Click(object sender, RoutedEventArgs e)
        {
            Content contentObject = Application.Current.Host.Content;
            contentObject.IsFullScreen = !contentObject.IsFullScreen;
        }

        private void initLeftMenu(List<T_SYS_ENTITYMENU> menulist)
        {
            #region 源代码备份
            StackPanel menuTemp = new StackPanel();
            menuTemp.Margin = new Thickness(0, 1, 0, 1);
            //leftMenu.MenuRoot.Children.Add(menuTemp);

            HyperlinkButton btnTest = new HyperlinkButton();
            btnTest.Height = 22;
            btnTest.FontSize = 13.333;
            btnTest.FontWeight = FontWeights.Bold;
            btnTest.Content = "     动态菜单";
            btnTest.Style = new Style(typeof(HyperlinkButton));
            //Link3.NavigateUri=null;
            //btnTest.NavigateUri = new Uri("/SMT.SaaS.OA.UI;component/Views/Home.xaml", System.UriKind.Relative); //("//Home.xaml");           
            //btnTest.TargetName = "contenxtPanle";
            btnTest.Click += new RoutedEventHandler(delegate
            {
                ContentFrame.Navigate(new Uri("/TravelapplicationPage", UriKind.Relative));
            });
            menuTemp.Children.Add(btnTest);
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
                group.Header = item.MENUNAME;
                group.BorderThickness = new Thickness(0);

                group.Style = this.Resources["TreeMenuGroupStyle"] as Style;
                group.VerticalContentAlignment = VerticalAlignment.Stretch;
                group.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                group.VerticalAlignment = VerticalAlignment.Stretch;
                group.HorizontalAlignment = HorizontalAlignment.Stretch;

                //生成菜单明细
                var menuItems = from m in menulist
                                where m.T_SYS_ENTITYMENU2Reference.EntityKey != null
                                && m.T_SYS_ENTITYMENU2Reference.EntityKey.EntityKeyValues[0].Value.ToString() == item.ENTITYMENUID
                                orderby m.ORDERNUMBER
                                select m;


                TransitioningContentControl ctrl = new TransitioningContentControl();

                StackPanel pnl = new StackPanel();
                pnl.VerticalAlignment = VerticalAlignment.Stretch;
                pnl.HorizontalAlignment = HorizontalAlignment.Stretch;
                pnl.Margin = new Thickness(0, 6, 0, 0);

                TreeView tree = new TreeView();
                tree.Style = Application.Current.Resources["TreeViewStyle"] as Style;
                tree.BorderThickness = new Thickness(0);
                tree.HorizontalAlignment = HorizontalAlignment.Stretch;
                tree.Width = toolkitacc.ActualWidth;

                foreach (var menu in menuItems)
                {
                    TreeViewItem treeItem = CreateTreeItem(menu);

                    treeItem.HeaderTemplate = this.Resources["TreeMenuItemStyle"] as DataTemplate;
                    treeItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
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
            treeItem.HeaderTemplate = this.Resources["TreeMenuItemStyle"] as DataTemplate;
            treeItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

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
                if (!(MenuInfosList.IndexOf(subMenu.ENTITYMENUID) > -1))
                {
                    //treeItem.IsEnabled = false;
                }
                parentItem.Items.Add(treeItem);

                AddSubMenu(menulist, treeItem, subMenu);
            }
        }

        void permClient_GetSysLeftMenuCompleted(object sender, GetSysLeftMenuCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
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

        /// <summary>
        ///     After the Frame navigates, ensure the <see cref="HyperlinkButton"/> representing the current page is selected
        /// </summary>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        /// <summary>
        ///     If an error occurs during navigation, show an error window
        /// </summary>
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception.Message);
        }

        #region "进度条控制"
        private void ProgressBar_Cancel(object sender, EventArgs e)
        {
            if (CancelWaiting != null)
            {
                CancelWaiting(this, EventArgs.Empty);
            }
            HideWaitingControl();
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

        //----------------------------------------------------------------------------
        /// <summary>
        /// Occurs when waiting is canceled
        /// </summary>
        public event EventHandler CancelWaiting;

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

        //----------------------------------------------------------------------------
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

        //----------------------------------------------------------------------------
        /// <summary>
        /// Shows waiting controls.
        /// </summary>
        public void ShowWaitingControl()
        {
            CalculatesSpinner();
            //waitSpinnerBack.Visibility = Visibility.Visible;
            //waitSpinner.Visibility = Visibility.Visible;
            //waitSpinner.Start();
            //beforSpinnerShowShowBorder.Begin();
        }
        #endregion

        private void Link4_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Content contentObject = Application.Current.Host.Content;
            contentObject.IsFullScreen = !contentObject.IsFullScreen;
        }



        private void HyperlinkButton_MouseLeave(object sender, MouseEventArgs e)
        {

        }


        private void Menu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            toolkitacc.Height = ((Border)sender).ActualHeight;
        }

        private void NH3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //permClient.GetSysLeftMenuAsync("1", Common.CurrentConfig.CurrentUser.UserInfo.SYSUSERID);
            permClient.GetSysLeftMenuFilterPermissionAsync("1", Common.CurrentLoginUserInfo.SysUserID, MenuInfosList);
            HeaderMenu.HBPervious.Click += new RoutedEventHandler(HBPervious_Click);

            HeaderMenu.HBNext.Click += new RoutedEventHandler(HBNext_Click);
            HeaderMenu.HBFullScreen.Click += new RoutedEventHandler(HBFullScreen_Click);

        }


        #region 上一页下一页事件
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
        #endregion

        public void NavigateTo(Uri uri)
        {
            ContentFrame.Navigate(uri);
        }
    }
}
