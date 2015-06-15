using System.Windows;
using System.Windows.Controls;
using System;
using System.Linq;
using SMT.SAAS.Controls.Toolkit;
using SMT.SaaS.Platform;

// 内容摘要: 首页中的WEBPART容器，用于承载当前系统的WEBPART

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    /// <summary>
    /// 首页中的WEBPART容器，用于承载当前系统的WEBPART
    /// </summary>
    public partial class WebPartHost : UserControl
    {

        DragDockPanelHost radtileview;
        /// <summary>
        /// 创建一个WebPartHost的新实例。
        /// </summary>
        public WebPartHost()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WebPartHost_Loaded);
        }

        void WebPartHost_Loaded(object sender, RoutedEventArgs e)
        {

            if (System.Windows.Application.Current.Resources["MvcOpenRecordSource"] != null)
            {
                //从mvc打开不需要加载相关webpart
            }
            else
            {
                LoadWebPartData();
            }
        }

        //modify by 安凯航.2011年9月5日
        //如果radtileview有值则表示不进行初始化
        private void LoadWebPartData()
        {
            if (radtileview == null)
            {
                radtileview = new DragDockPanelHost();

                DragDockPanel item2 = new DragDockPanel();
                item2.Header = "待办任务";
                item2.Content = new SMT.SAAS.Platform.WebParts.Views.PendingTask();
                IWebPart currentContent = item2.Content as IWebPart;
                item2.Maximized += (obj, args) =>
                {
                    currentContent.ShowMaxiWebPart();
                };
                item2.Minimized += (obj1, args1) =>
                {
                    currentContent.ShowMiniWebPart();
                };
                item2.PanelState = PanelState.Maximized;
                //currentContent.OnMoreChanged += (obj3, arg3) =>
                //{
                //    item2.PanelState = PanelState.Maximized;
                //};
                item2.Style = Application.Current.Resources["WebPartDragDockPanelStyle1"] as Style;
                radtileview.AddPanel(item2);

                DragDockPanel item5 = new DragDockPanel();
                item5.Header = "我的单据";
                item5.Content = new SMT.SAAS.Platform.WebParts.Views.MyRecord();

                currentContent = item5.Content as SMT.SaaS.Platform.IWebPart;
                item5.Maximized += (obj, args) =>
                {
                    currentContent.ShowMaxiWebPart();
                };
                item5.Minimized += (obj1, args1) =>
                {
                    currentContent.ShowMiniWebPart();
                };
                //currentContent.OnMoreChanged += (obj3, arg3) =>
                //{
                //    item5.PanelState = PanelState.Minimized;
                //};
                item5.Style = Application.Current.Resources["WebPartDragDockPanelStyle1"] as Style;

                radtileview.AddPanel(item5);

                //RadTileViewItem item3 = new RadTileViewItem();
                //item3.TileState = TileViewItemState.Minimized;
                //item3.Header = "消息提醒";
                //item3.Content = new SMT.SAAS.Platform.WebParts.Views.NoteRemind();
                //radtileview.Items.Add(item3);

                DragDockPanel item4 = new DragDockPanel();
                item4.Header = "新闻动态";
                item4.Content = new SMT.SAAS.Platform.WebParts.Views.News();

                currentContent = item4.Content as SMT.SaaS.Platform.IWebPart;
                item4.Maximized += (obj, args) =>
                {
                    currentContent.ShowMaxiWebPart();
                };
                item4.Minimized += (obj1, args1) =>
                {
                    currentContent.ShowMiniWebPart();
                };
                //currentContent.OnMoreChanged += (obj3, arg3) =>
                //{
                //    item4.PanelState = PanelState.Minimized;
                //};
                item4.Style = Application.Current.Resources["WebPartDragDockPanelStyle1"] as Style;

                radtileview.AddPanel(item4);

                //CheckeDepends("SMT.SaaS.OA.UI");
                DragDockPanel item6 = new DragDockPanel();
                //item6.TileState = TileViewItemState.Minimized;
                item6.Header = "公司发文";
                item6.Content = new SMT.SAAS.Platform.WebParts.Views.OAWebPart();

                currentContent = item6.Content as SMT.SaaS.Platform.IWebPart;
                item6.Maximized += (obj, args) =>
                {
                    currentContent.ShowMaxiWebPart();
                };
                item6.Minimized += (obj1, args1) =>
                {
                    currentContent.ShowMiniWebPart();
                };
                //currentContent.OnMoreChanged += (obj3, arg3) =>
                //{
                //    item6.PanelState = PanelState.Minimized;
                //};
                item6.Style = Application.Current.Resources["WebPartDragDockPanelStyle1"] as Style;

                radtileview.AddPanel(item6);

                Root.Children.Add(radtileview);
            }
            else
            {
                foreach (DragDockPanel item in radtileview.Children)
                {
                    IWebpart webPart = item.Content as IWebpart;
                    if (webPart != null)
                    {
                        webPart.Initialize();
                    }
                }
            }
        }

        public void Clear()
        {
            Root.Children.Clear();
            if (radtileview != null)
            {
                foreach (var item in radtileview.Children)
                {
                    var radItem = item as DragDockPanel;
                    if (radItem != null)
                    {
                        ICleanup clearup = radItem.Content as ICleanup;
                        if (clearup != null)
                            clearup.Cleanup();
                    }
                }
                radtileview = null;
            }
        }

        public void LoadWebPart()
        {
            LoadWebPartData();
        }

        public void Star()
        {
            if (radtileview != null)
            {
                foreach (var item in radtileview.Children)
                {
                    var radItem = item as DragDockPanel;
                    if (radItem != null)
                    {
                        IWebpart clearup = radItem.Content as IWebpart;
                        if (clearup != null)
                            clearup.Star();
                    }
                }
            }
        }


        public void Stop()
        {
            if (radtileview != null)
            {
                foreach (var item in radtileview.Children)
                {
                    var radItem = item as DragDockPanel;
                    if (radItem != null)
                    {
                        IWebpart clearup = radItem.Content as IWebpart;
                        if (clearup != null)
                            clearup.Stop();
                    }
                }
            }
        }
        //管理匿名事件
        //EventHandler<ViewModel.LoadModuleEventArgs> LoadTaskHandler = null;
        //private void CheckeDepends(string moduleName)
        //{
        //    var module = ViewModel.Context.Managed.Catalog.FirstOrDefault(item => item.ModuleName == moduleName);
        //    if (module != null)
        //    {
        //        ViewModel.Context.Managed.OnSystemLoadModuleCompleted += LoadTaskHandler = (o, e) =>
        //        {
        //            ViewModel.Context.Managed.OnSystemLoadModuleCompleted -= LoadTaskHandler;
        //            if (e.Error == null)
        //            {
        //                AddOAWebPart();
        //            }
        //        };

        //        ViewModel.Context.Managed.LoadModule(moduleName);
        //    }
        //}

        private void AddOAWebPart()
        {
            var OAWebPaer = CreateOAWebPart();
            if (OAWebPaer != null)
            {

                DragDockPanel item6 = new DragDockPanel();
                //item6.TileState = TileViewItemState.Minimized;
                item6.Header = "公司发文";
                item6.Content = OAWebPaer;
                radtileview.AddPanel(item6);
            }
        }

        private object CreateOAWebPart()
        {
            try
            {
                string oawebpart = "SMT.SaaS.OA.UI.UserControls.OAWebPart, SMT.SaaS.OA.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

                Type type = Type.GetType(oawebpart);

                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        //public int Top
        //{
        //    get;
        //    set;
        //}

        //public int RefDate
        //{
        //    get;
        //    set;
        //}

        //public string Titel
        //{
        //    get;
        //    set;
        //}

        //public void LoadDate()
        //{
            
        //}

        //public void Initialize()
        //{
           
        //}

        //public void Cleanup()
        //{
        //    //this.Clear();
        //}
    }
}
