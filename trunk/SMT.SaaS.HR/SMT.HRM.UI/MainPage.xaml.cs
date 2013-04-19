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

using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI;
using System.Windows.Threading;

using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.Helper;
using System.Collections.ObjectModel;


namespace SMT.HRM.UI
{
    public partial class MainPage : UserControl
    {
        //页面替换动画
        private readonly Image _imgLast = new Image();
        private readonly Image _imgNext = new Image();
        private Storyboard _sb = new Storyboard();

        private string _lasttext;
        private bool _isNavigating = false;

        public MainPage()
        {

            InitializeComponent();

            permClient.GetSysLeftMenuCompleted += new EventHandler<GetSysLeftMenuCompletedEventArgs>(permClient_GetSysLeftMenuCompleted);

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderMenu.SetUserNameAndDepartmentName(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName,
                SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName,
                SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName,
                SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName);

            System.Windows.Controls.Window.Parent = windowParent;
            System.Windows.Controls.Window.TaskBar = new StackPanel();
            System.Windows.Controls.Window.Wrapper = this;
            System.Windows.Controls.Window.IsShowtitle = false;
            /*
             * Test             
             */
            //SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient a = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
            //System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            //int pageCount = 0;
            //string abc = "@0.Contains(Mobile) or  @1.Contains(Tel) or  @2.Contains(EmployeeCNName) or  @3.Contains(PostName) or  @4.Contains(DepartMentName)";
            //paras.Add("刘映");
            //paras.Add("刘映");
            //paras.Add("刘映");
            //paras.Add("刘映");
            //paras.Add("刘映");
            //a.GetEmployeeListMobileAsync(1, 20, "EmployeeCNName", abc, paras, pageCount, "bbac270b-120d-4b6d-bc72-22fa6476ec9f", false);
            //a.GetEmployeeListMobileCompleted += (oo, ee) =>
            //    {
            //        IEnumerable<SMT.Saas.Tools.PersonnelWS.V_MOBILEEMPLOYEE> list;
            //        list = ee.Result.ToList().Distinct();
            //    };
            //SMT.Saas.Tools.SalaryWS.SalaryServiceClient a = new Saas.Tools.SalaryWS.SalaryServiceClient();
            //SMT.Saas.Tools.SalaryWS.T_HR_CUSTOMGUERDONARCHIVE custom = new Saas.Tools.SalaryWS.T_HR_CUSTOMGUERDONARCHIVE
            //{
            //    CUSTOMGUERDONARCHIVEID = "aeb0e17b-e650-45c8-9380-f3e569c71778",
            //    SUM = decimal.Parse("5555555"),
            //    REMARK = "6666666666"
            //};
            //custom.T_HR_SALARYARCHIVE = new SMT.Saas.Tools.SalaryWS.T_HR_SALARYARCHIVE();
            //custom.T_HR_SALARYARCHIVE.SALARYARCHIVEID="b355b0b8-be24-4ab7-8258-0c8646597a45";
            //a.CustomGuerdonArchiveUpdateAsync(custom);
            //ObservableCollection<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB> list = new ObservableCollection<Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB>();
            //SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB employee = new Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB
            //{
            //    OWNERID = "27f547f7-bae4-4df4-ae03-07b579afcae0",
            //    OWNERPOSTID = "c51a4e93-265d-45a0-aa16-3c9f0ebf27d8",
            //    PERSONBUDGETAPPLYDETAILID = "022edac5-f060-4d41-a510-defaf6955230"
            //};
            //SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB employee1 = new Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB
            //{
            //    OWNERID = "eb732bab-8589-43a9-b51a-f13d72b056cc",
            //    OWNERPOSTID = "782541c5-86f9-4658-947d-61d264554640",
            //    PERSONBUDGETAPPLYDETAILID = "022edac5-f060-4d41-a510-defaf6955230"
            //};
            //SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB employee2 = new Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB
            //{
            //    OWNERID = "0a9cfa23-acff-49fb-9b6d-3d3eac7608ca",
            //    OWNERPOSTID = "dd9048f5-0125-4e0b-8e79-9f6ef1c39dd5",
            //    PERSONBUDGETAPPLYDETAILID = "022edac5-f060-4d41-a510-defaf6955230"
            //};
            //list.Add(employee);
            //////list.Add(employee1);
            //////list.Add(employee2);
            //ObservableCollection<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB> listResult = new ObservableCollection<Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB>();

            //a.GetEmployeeListForFBAsync(list);
            //a.GetEmployeeListForFBCompleted += (obj, eve) =>
            //    {
            //        listResult = eve.Result;
            //        string rer = "";
            //    };
        }

        private PermissionServiceClient permClient = new PermissionServiceClient();

        public void NavigateTo(Uri uri)
        {
            ContentFrame.Navigate(uri);
        }

        private void InitLeftMenu(List<T_SYS_ENTITYMENU> menulist)
        {
            #region 备份的代码
            ////StackPanel menuTemp = new StackPanel();
            ////menuTemp.Margin = new Thickness(0, 1, 0, 1);
            ////leftMenu.MenuRoot.Children.Add(menuTemp);

            //HyperlinkButton btnTest = new HyperlinkButton();
            //btnTest.Height = 22;
            //btnTest.FontSize = 13.333;
            //btnTest.FontWeight = FontWeights.Bold;
            //btnTest.Content = "     动态菜单";
            //btnTest.Style = new Style(typeof(HyperlinkButton));
            ////Link3.NavigateUri=null;
            ////btnTest.NavigateUri = new Uri("/SMT.HRM.UI;component/Views/Home.xaml", System.UriKind.Relative); //("//Home.xaml");           
            ////btnTest.TargetName = "contenxtPanle";
            //btnTest.Click += new RoutedEventHandler(delegate{
            //    ContentFrame.Navigate(new Uri("/Home", UriKind.Relative));});

            ////menuTemp.Children.Add(btnTest);

            ////btnTest.Style = new Style();
            #endregion


            if (MainLeftMenu.Items != null)
                MainLeftMenu.Items.Clear();

            //生成分组
            // 1s 冉龙军
            //var groupItems = from m in menulist
            //                 where m.T_SYS_ENTITYMENU2 == null
            //                 orderby m.ORDERNUMBER
            //                 select m;
            var groupItems = from m in menulist
                             where m.T_SYS_ENTITYMENU2Reference.EntityKey == null
                             orderby m.ORDERNUMBER
                             select m;
            // 1e
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
                //var menuItems = from m in menulist
                //                where m.T_SYS_ENTITYMENU2 == item
                //                orderby m.ORDERNUMBER
                //                 select m;
                var menuItems = from m in menulist
                                where m.T_SYS_ENTITYMENU2Reference.EntityKey != null
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
                tree.Margin = new Thickness(0);
                tree.HorizontalAlignment = HorizontalAlignment.Stretch;
                tree.Width = MainLeftMenu.ActualWidth;


                foreach (var menu in menuItems)
                {
                    TreeViewItem treeItem = CreateTreeItem(menu);

                    AddSubMenu(menulist, treeItem, menu);
                    tree.Items.Add(treeItem);

                    #region 原始方法生成
                    //StackPanel itempanel = new StackPanel();
                    //itempanel.Height = 24;
                    //itempanel.Orientation = Orientation.Horizontal;

                    //Image img = new Image();
                    //img.Width = 16;
                    //img.Height = 16;
                    //img.HorizontalAlignment = HorizontalAlignment.Left;

                    //if (!string.IsNullOrEmpty(menu.URLADDRESS))
                    //    img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(menu.MENUICONPATH, UriKind.Relative));
                    //img.Margin = new Thickness(6, 1, 0, 0);

                    //itempanel.Children.Add(img);

                    //HyperlinkButton link = new HyperlinkButton();
                    //link.Content = menu.MENUNAME;

                    //if (!string.IsNullOrEmpty(menu.URLADDRESS))
                    //    link.NavigateUri = new Uri(menu.URLADDRESS, UriKind.Relative);

                    //link.Margin = new Thickness(6, 6, 0, 0);
                    //itempanel.Children.Add(link);

                    //pnl.Children.Add(itempanel);
                    #endregion
                }

                ctrl.Content = tree;
                //ctrl.Content = pnl;

                group.Content = ctrl;
                MainLeftMenu.Items.Add(group);
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
                _lasttext = menu.URLADDRESS;
                ContentFrame.Navigate(new Uri(menu.URLADDRESS, UriKind.Relative));
                //_lasttext = (HyperlinkButton)sender;
                if (ContentFrame.Source.ToString() != _lasttext && !_isNavigating)
                {
                    _sb = new Storyboard();
                    _sb.Completed += SbCompleted;

                    Panel panel = ContentFrame.Parent as Panel;
                    if (panel != null) panel.Children.Add(_imgLast);
                    ContentFrame.Navigate(new Uri(_lasttext, UriKind.Relative));

                    WriteableBitmap bitmapL = new WriteableBitmap(ContentFrame, new TranslateTransform());
                    _imgLast.Source = bitmapL;
                    TranslateTransform ttLast = new TranslateTransform();

                    _imgLast.RenderTransform = ttLast;

                    ContentFrame.Navigated += ContentFrameNavigated;
                    _sb.Children.Add(CreateDoubleAnimation(ttLast, "X", 0, bitmapL.PixelWidth, true));

                    _isNavigating = true;
                }
            }
        }

        void SbCompleted(object sender, EventArgs e)
        {
            Panel panel = ContentFrame.Parent as Panel;
            panel.Children.Remove(_imgLast);
            panel.Children.Remove(_imgNext);
            ContentFrame.Visibility = Visibility.Visible;
            _sb.Stop();
            _isNavigating = false;
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
                    var q = from ent in list
                            select new SMT.SaaS.LocalData.T_SYS_ENTITYMENU
                            {
                                CHILDSYSTEMNAME = ent.CHILDSYSTEMNAME,
                                ENTITYCODE = ent.CHILDSYSTEMNAME,
                                //SUPERIORID = ent.T_SYS_ENTITYMENU2.ENTITYMENUID,
                                ENTITYMENUID = ent.ENTITYMENUID,
                                ENTITYNAME = ent.ENTITYNAME,
                                HASSYSTEMMENU = ent.HASSYSTEMMENU,
                                // HELPFILEPATH = ent.HELPFILEPATH,
                                //  HELPTITLE = ent.HELPTITLE,
                                ISAUTHORITY = ent.ISAUTHORITY,
                                MENUCODE = ent.MENUCODE,
                                MENUICONPATH = ent.MENUICONPATH,
                                MENUNAME = ent.MENUNAME,
                                ORDERNUMBER = ent.ORDERNUMBER,
                                SYSTEMTYPE = ent.SYSTEMTYPE,
                                URLADDRESS = ent.URLADDRESS,
                            };
                    //SMT.SAAS.Main.CurrentContext.Common.EntityMenu = list;
                    SMT.SAAS.Main.CurrentContext.Common.EntityMenu = q.ToList();
                }
                InitLeftMenu(list);
            }
        }

        #endregion


        /// <summary>
        ///     After the Frame navigates, ensure the <see cref="HyperlinkButton"/> representing the current page is selected
        /// </summary>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            #region 导航显示隐藏
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
            #endregion
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

        private void Menu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainLeftMenu.Height = ((Border)sender).ActualHeight;
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            permClient.GetSysLeftMenuAsync("0", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

            HeaderMenu.HBPervious.Click += new RoutedEventHandler(HBPervious_Click);

            HeaderMenu.HBNext.Click += new RoutedEventHandler(HBNext_Click);
            HeaderMenu.HBFullScreen.Click += new RoutedEventHandler(HBFullScreen_Click);
        }

        #region 全屏显示
        void HBFullScreen_Click(object sender, RoutedEventArgs e)
        {
            Content contentObject = Application.Current.Host.Content;
            contentObject.IsFullScreen = !contentObject.IsFullScreen;
        }
        #endregion

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

        #region 页面动画；创建动画
        private static DoubleAnimation CreateDoubleAnimation(DependencyObject element, string property, double from,
                                                          double to, bool addEasing)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.To = to;
            da.From = from;
            da.Duration = new Duration(TimeSpan.FromSeconds(0.68));

            if (addEasing)
                da.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 3 };

            Storyboard.SetTargetProperty(da, new PropertyPath(property));
            Storyboard.SetTarget(da, element);
            return da;
        }


        private DispatcherTimer timer;
        private void ContentFrameNavigated(object sender, NavigationEventArgs e)
        {
            ContentFrame.Navigated -= ContentFrameNavigated;
            Panel panel = ContentFrame.Parent as Panel;
            panel.UpdateLayout();

            timer = new DispatcherTimer();

            timer.Tick += (arg, obj) =>
            {

                WriteableBitmap bitmapN = new WriteableBitmap(ContentFrame, new TranslateTransform());
                _imgNext.Source = bitmapN;
                TranslateTransform ttNext = new TranslateTransform();

                _imgNext.RenderTransform = ttNext;

                panel.Children.Add(_imgNext);

                //StoryboardStart.Begin();
                _sb.Children.Add(CreateDoubleAnimation(ttNext, "X", -bitmapN.PixelWidth, 0, true));
                ContentFrame.Visibility = Visibility.Collapsed;
                _sb.Begin();
                timer.Stop();
                timer = null;
            };

            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();
        }
        #endregion
    }
}
