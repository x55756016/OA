
// 内容摘要: 用户自定义快捷菜单+系统默认菜单

using System;
using System.Windows.Controls;
using System.Windows;
using SMT.SAAS.Platform.Controls.CWrapPanel;
using System.Collections.Generic;
using System.Windows.Media;
//using SMT.SAAS.Platform.Xamls.Icons;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Linq;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class MenusPanel : UserControl, ICleanup, IDisposable
    {

        public event EventHandler<OnShortCutClickEventArgs> ShortCutClick;
        public event EventHandler<System.Windows.Input.MouseButtonEventArgs> MenuItemMouseDown;
        public event EventHandler<System.Windows.Input.MouseEventArgs> MenuItemMouseMove;
        private SMT.SAAS.Platform.ViewModel.SplashScreen.SplashScreenViewModel _splashScreen;

        private Saas.Tools.PermissionWS.PermissionServiceClient Pmclient;
        public MenusPanel()
        {
            InitializeComponent();
            _splashScreen = new ViewModel.SplashScreen.SplashScreenViewModel();

            this.Loaded += new RoutedEventHandler(CustomMenusSet_Loaded);
            if (Pmclient == null)
            {
                Pmclient = new Saas.Tools.PermissionWS.PermissionServiceClient();
            }
            Pmclient.GetSystemTypeListByUserIDCompleted += Pmclient_GetSystemTypeListByUserIDCompleted;
            Pmclient.GetSysLeftMenuFilterPermissionToNewFrameCompleted += Pmclient_GetSysLeftMenuFilterPermissionToNewFrameCompleted;
        }

        void CustomMenusSet_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.tabControl1.Items == null || this.tabControl1.Items.Count == 0)
            {
                showLoading();
                string strtemp = string.Empty;

                //尝试从本地读取菜单
                try
                {
                    //AllMenus = IosManager.GetValue<List<V_UserMenuPermission>>("AllMenus");
                    //if (AllMenus != null)
                    //{
                    //    Application.Current.Resources.Add("AllMenus", AllMenus);
                    //    ViewModel.Context.CacheMenuAll = AllMenus;


                    //    AllsystemTypes = IosManager.GetValue<List<T_SYS_DICTIONARY>>("AllsystemTypes");
                    //    if (AllsystemTypes != null)
                    //    {
                    //        Application.Current.Resources.Add("AllsystemTypes", AllsystemTypes);
                    //        ViewModel.Context.CacheSystemType = AllsystemTypes;

                    //        Deployment.Current.Dispatcher.BeginInvoke(delegate
                    //        {
                    //            BindControls();
                    //        });
                    //        return;
                    //    }
                    //}
                }catch(Exception ex)
                {

                }


                  AllMenus = Application.Current.Resources["AllMenus"] as List<V_UserMenuPermission>;
                  if (AllMenus == null || AllMenus.Count == 0)
                  {
                      Pmclient.GetSystemTypeListByUserIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID, strtemp);
                  }
                  else
                  {
                      Deployment.Current.Dispatcher.BeginInvoke(delegate
                      {
                          BindControls();
                      });
                  }
            }
        }

        private void BindControls()
        {
            if (AllMenus == null)
            {
                return;
            }

            if (AllMenus.Count == 0)
            {
                return;
            }

            TabItem all = new TabItem()
            {
                Header = "全部",
                IsSelected = true,
                Style = this.Resources["TabItemStyle_Sys2"] as Style
            };
            var allchildMenus = getAllChildMenus();
            all.Content = BuildMenus(allchildMenus);
            this.tabControl1.Items.Insert(0, all);

            foreach (var systemItem in AllsystemTypes)
            {
                if (systemItem.DICTIONARYNAME == "引擎系统")
                {

                }
                #region 添加系统名
                var AllSecondChildMenus = getSecondChildMenus(systemItem.DICTIONARYVALUE.ToString());
                if (AllSecondChildMenus.Count() > 0)
                {
                    TabItem tabSytem = new TabItem()
                    {
                        Header = systemItem.DICTIONARYNAME,
                        Style = this.Resources["TabItemStyle_Sys2"] as Style
                    };
                    this.tabControl1.Items.Add(tabSytem);

                    #region 添加二级菜单
                    ////有三级子菜单才添加二级菜单
                    TabControl SecondMenuTabControl = new TabControl()
                    {
                        Style = this.Resources["TabControlStyle_sys"] as Style
                    };
                    tabSytem.Content = SecondMenuTabControl;
                    foreach (var secondfatherMenu in AllSecondChildMenus)
                    {
                        var allchild = getAllChildMenus(secondfatherMenu.ENTITYMENUID);
                        if (allchild.Count > 0)
                        {
                            if(secondfatherMenu.MENUNAME.Contains("人事管理"))
                            {

                            }
                            TabItem tabSecond = new TabItem()
                            {
                                Header = secondfatherMenu.MENUNAME,                                
                                Style = this.Resources["TabItemStyle_Sys2"] as Style
                            };
                            SecondMenuTabControl.Items.Add(tabSecond);

                            #region 添加三级菜单
                            var AllThirdChildMenus = new List<V_UserMenuPermission>();
                            getAllThirdChildMenus(secondfatherMenu.ENTITYMENUID, ref AllThirdChildMenus);
                          
                            tabSecond.Content = BuildMenus(AllThirdChildMenus);
                            #endregion
                        }
                    }

                    #endregion
                }
                #endregion
            }
        }

        private Grid BuildMenus(List<V_UserMenuPermission> items)
        {
            ScrollViewer sv = new ScrollViewer();
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            SMT.SAAS.Platform.Controls.CWrapPanel.WrapPanel wp = new Controls.CWrapPanel.WrapPanel();
            foreach (var item in items)
            {
                ShortCut shortcut = new ShortCut();
                shortcut.Margin = new Thickness(10);
                shortcut.Titel = item.MENUNAME;
                shortcut.tbTitel.Foreground = new SolidColorBrush(Color.FromArgb(255, 17, 17, 17));
                shortcut.Icon = item.MENUICONPATH;
                shortcut.DataContext = item;

                shortcut.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Menu48Icon_MouseLeftButtonDown);
                shortcut.MouseMove += new System.Windows.Input.MouseEventHandler(ShortCut_MouseMove);
                shortcut.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(Menu48Icon_MouseLeftButtonUp);

                wp.Children.Add(shortcut);
            }
            sv.Content = wp;
            Grid grid = new Grid();
            grid.Children.Add(sv);
            return grid;
        }


        private List<V_UserMenuPermission> getSecondChildMenus(string systemType)
        {
            var allChild = from ent in AllMenus
                                where (ent.EntityMenuFatherID == string.Empty
                                || ent.EntityMenuFatherID==null)
                                && ent.SYSTEMTYPE == systemType
                                orderby ent.ORDERNUMBER
                                select ent;
            return allChild.ToList();
        }



        private List<V_UserMenuPermission> getAllChildMenus()
        {
            List<V_UserMenuPermission> allChild = new List<V_UserMenuPermission>();
            foreach(var item in AllMenus)
            {
               getAllThirdChildMenus(item.ENTITYMENUID, ref allChild);
            }
            return allChild.ToList();
        }


        /// <summary>
        /// 获取所有子菜单
        /// </summary>
        /// <param name="FatherMenuId"></param>
        /// <returns></returns>
        private List<V_UserMenuPermission> getAllChildMenus(string FatherMenuId)
        {
            var AllThirdChild = from ent in AllMenus
                                where ent.EntityMenuFatherID == FatherMenuId
                                orderby ent.ORDERNUMBER
                                select ent;            
            return AllThirdChild.ToList();
        }

        /// <summary>
        /// 获取所有叶子菜单
        /// </summary>
        /// <param name="FatherMenuId"></param>
        /// <returns></returns>
        private void getAllThirdChildMenus(string FatherMenuId, ref List<V_UserMenuPermission> AllThirdChild)
        {
            var allchildMenus = from ent in AllMenus
                                where ent.EntityMenuFatherID == FatherMenuId
                                orderby ent.ORDERNUMBER
                                select ent;
            if (allchildMenus.Count() == 0)
            {
                var child = (from ent in AllMenus
                             where ent.ENTITYMENUID == FatherMenuId
                             select ent).FirstOrDefault();
                AllThirdChild.Add(child);
            }
            else
            {
                foreach (var item in allchildMenus)
                {
                   getAllThirdChildMenus(item.ENTITYMENUID, ref AllThirdChild);
                }
            }
        }

        public void Cleanup()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.Collect();
        }

        private void Menu48Icon_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShortCut s = sender as ShortCut;
            if (s != null)
            {
                V_UserMenuPermission datecontext = s.DataContext as V_UserMenuPermission;
                if (ShortCutClick != null)
                    ShortCutClick(this, new OnShortCutClickEventArgs(datecontext, null));
            }
        }

        private void Menu48Icon_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MenuItemMouseDown != null)
                MenuItemMouseDown(sender, e);
        }

        private void ShortCut_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (MenuItemMouseMove != null)
                MenuItemMouseMove(sender, e);
        }

        public void showLoading()
        {
            loading.Start();
        }

        public void hideLoading()
        {
            loading.Stop();
        }
    }
}