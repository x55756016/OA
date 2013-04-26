//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 用户自定义快捷菜单+系统默认菜单
// 完成日期：2011-05-16 
// 版    本：V1.0 
// 作    者：王玲 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

using System;
using System.Windows.Controls;
using System.Windows;
using SMT.SAAS.Platform.Controls.CWrapPanel;
using System.Collections.Generic;
using System.Windows.Media;
using SMT.SAAS.Platform.Xamls.Icons;
using SMT.SAAS.Platform.ViewModel.Menu;
using System.Collections.ObjectModel;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class CustomMenusSet : UserControl, ICleanup, IDisposable
    {

        public event EventHandler<OnShortCutClickEventArgs> ShortCutClick;
        public event EventHandler<System.Windows.Input.MouseButtonEventArgs> MenuItemMouseDown;
        public event EventHandler<System.Windows.Input.MouseEventArgs> MenuItemMouseMove;
        private ViewModel.Menu.MainMenuViewModel _menuViewModel;
        private SMT.SAAS.Platform.ViewModel.SplashScreen.SplashScreenViewModel _splashScreen;
        private bool bIsLoaded = false;

        public CustomMenusSet()
        {
            InitializeComponent();
            _splashScreen = new ViewModel.SplashScreen.SplashScreenViewModel();

            this.Loaded += new RoutedEventHandler(CustomMenusSet_Loaded);
        }

        void CustomMenusSet_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Context.Managed != null)
            {
                if (ViewModel.Context.Managed.Catalog != null)
                {
                    if (ViewModel.Context.Managed.Catalog.Count > 0)
                    {
                        bIsLoaded = true;
                    }
                }
            }

            if (!bIsLoaded)
            {
                InitMenu();
                return;
            }

            if (this.tabControl1.Items == null)
            {
                _menuViewModel = new MainMenuViewModel();
                BindControls();
                return;
            }

            if (this.tabControl1.Items.Count == 0)
            {
                _menuViewModel = new MainMenuViewModel();
                BindControls();
                return;
            }
        }

        public void InitMenu()
        {
            Start();
            _splashScreen.InitCompleted += new EventHandler(_splashScreen_InitCompleted);
            _splashScreen.GetModules();
        }

        void _splashScreen_InitCompleted(object sender, EventArgs e)
        {
            if (this.tabControl1.Items != null)
            {
                this.tabControl1.Items.Clear();
                cache.Clear();
            }

            _menuViewModel = new MainMenuViewModel();
            BindControls();

            Stop();
        }

        public void Cleanup()
        {
            //MessageBox.Show("Cleanup : CustomMenusSet");
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
                MenuViewModel datecontext = s.DataContext as MenuViewModel;
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


        private static bool setSelect = true;
        private List<string> cache = new List<string>();
        private void BindControls()
        {

            var result = _menuViewModel.GetSystemMenu();

            if (result == null)
            {
                return;
            }

            if (result.Count == 0)
            {
                return;
            }


            foreach (var item in result)
            {
                if (!cache.Contains(item.MenuID))
                {
                    cache.Add(item.MenuID);
                    TabItem ti = new TabItem()
                    {
                        Header = item.MenuName,
                        Style = this.Resources["TabItemStyle_Sys2"] as Style
                    };

                    if (item.Item.Count > 0)
                    {
                        TabControl moduleTabControl = new TabControl()
                        {
                            Style = this.Resources["TabControlStyle_sys"] as Style
                        };

                        ObservableCollection<MenuViewModel> sysAllItems = new ObservableCollection<MenuViewModel>();

                        foreach (var moduleitem in item.Item)
                        {
                            var items = _menuViewModel.GetChildMenu(moduleitem.MenuID);

                            if (items.Count > 0)
                            {
                                TabItem t2 = new TabItem()
                                {
                                    Header = moduleitem.MenuName,
                                    DataContext = moduleitem,
                                    Style = this.Resources["TabItemStyle_sys"] as Style
                                };


                                foreach (var moduleitem2 in items)
                                {
                                    sysAllItems.Add(moduleitem2);
                                }
                                t2.Content = BuildItems(items);
                                moduleTabControl.Items.Add(t2);
                            }
                        }

                        TabItem sysAll = new TabItem()
                        {
                            Header = "所有",
                            IsSelected = true,
                            Style = this.Resources["TabItemStyle_sys"] as Style
                        };

                        sysAll.Content = BuildItems(sysAllItems);

                        moduleTabControl.Items.Insert(0, sysAll);

                        ti.Content = moduleTabControl;
                    }

                    this.tabControl1.Items.Add(ti);
                }
            }
            TabItem all = new TabItem()
            {
                Header = "全部",
                IsSelected = true,
                Style = this.Resources["TabItemStyle_Sys2"] as Style
            };

            all.Content = BuildItems(_menuViewModel.GetAllMenu());
            this.tabControl1.Items.Insert(0, all);
        }

        public Grid load(string parentID)
        {
            var items = _menuViewModel.GetChildMenu(parentID);
            return BuildItems(items);
        }

        private Grid BuildItems(ObservableCollection<MenuViewModel> items)
        {
            ScrollViewer sv = new ScrollViewer();
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //已经作废
            //Style st = App.Current.Resources["ScrollViewerStyle"] as Style;
            //if (st != null)
            //    sv.Style = st;


            SMT.SAAS.Platform.Controls.CWrapPanel.WrapPanel wp = new Controls.CWrapPanel.WrapPanel();
            foreach (var item in items)
            {
                ShortCut shortcut = new ShortCut();
                shortcut.Margin = new Thickness(10);
                shortcut.Titel = item.MenuName;
                shortcut.tbTitel.Foreground = new SolidColorBrush(Color.FromArgb(255, 17, 17, 17));
                shortcut.Icon = item.MenuIconPath;
                shortcut.DataContext = item;

                shortcut.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Menu48Icon_MouseLeftButtonDown);
                shortcut.MouseMove += new System.Windows.Input.MouseEventHandler(ShortCut_MouseMove);
                shortcut.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(Menu48Icon_MouseLeftButtonUp);

                wp.Children.Add(shortcut);
            }
            sv.Content=wp;
            Grid grid = new Grid();
            grid.Children.Add(sv);
            return grid;
        }

        public Grid LoadAll()
        {
            var items = _menuViewModel.GetChildMenu("");
            SMT.SAAS.Platform.Controls.CWrapPanel.WrapPanel wp = new Controls.CWrapPanel.WrapPanel();
            foreach (var item in items)
            {
                ShortCut shortcut = new ShortCut();
                shortcut.Margin = new Thickness(10);
                shortcut.tbTitel.Foreground = new SolidColorBrush(Color.FromArgb(255,17,17,17));
                shortcut.Titel = item.MenuName;
                shortcut.Icon = item.MenuIconPath;
                shortcut.DataContext = item;

                shortcut.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Menu48Icon_MouseLeftButtonDown);
                shortcut.MouseMove += new System.Windows.Input.MouseEventHandler(ShortCut_MouseMove);
                shortcut.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(Menu48Icon_MouseLeftButtonUp);

                wp.Children.Add(shortcut);
            }

            Grid grid = new Grid();
            grid.Children.Add(wp);
            return grid;
        }


        public void Start()
        {
            loading.Start();
        }

        public void Stop()
        {
            loading.Stop();
        }
    }


}
