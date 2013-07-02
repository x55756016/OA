using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using System;
using System.Linq;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 首页中的WEBPART容器，用于承载当前系统的WEBPART
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    /// <summary>
    /// 首页中的WEBPART容器，用于承载当前系统的WEBPART
    /// </summary>
    public partial class WebPartHost : UserControl,IWebpart
    {

        RadTileView radtileview;
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
                radtileview = new RadTileView();

                radtileview.MinimizedColumnWidth = new GridLength(310);

                //RadTileViewItem item1 = new RadTileViewItem();
                //item1.TileState = TileViewItemState.Minimized;
                //item1.Header = "系统日志";
                //item1.Content = new SystemLogger();
                //radtileview.Items.Add(item1);

                RadTileViewItem item2 = new RadTileViewItem();
                item2.TileState = TileViewItemState.Maximized;
                item2.Header = "待办任务";
                item2.Content = new SMT.SAAS.Platform.WebParts.Views.PendingTask();
                radtileview.Items.Add(item2);

                RadTileViewItem item5 = new RadTileViewItem();
                item5.TileState = TileViewItemState.Minimized;
                item5.Header = "我的单据";
                item5.Content = new SMT.SAAS.Platform.WebParts.Views.MyRecord();
                radtileview.Items.Add(item5);

                //RadTileViewItem item3 = new RadTileViewItem();
                //item3.TileState = TileViewItemState.Minimized;
                //item3.Header = "消息提醒";
                //item3.Content = new SMT.SAAS.Platform.WebParts.Views.NoteRemind();
                //radtileview.Items.Add(item3);

                RadTileViewItem item4 = new RadTileViewItem();
                item4.TileState = TileViewItemState.Minimized;
                item4.Header = "新闻动态";
                item4.Content = new SMT.SAAS.Platform.WebParts.Views.News();
                radtileview.Items.Add(item4);

                //CheckeDepends("SMT.SaaS.OA.UI");
                RadTileViewItem item6 = new RadTileViewItem();
                item6.TileState = TileViewItemState.Minimized;
                item6.Header = "公司发文";
                item6.Content = new SMT.SAAS.Platform.WebParts.Views.OAWebPart();
                radtileview.Items.Add(item6);

                Root.Children.Add(radtileview);
            }
            else
            {
                foreach (RadTileViewItem item in radtileview.Items)
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
                foreach (var item in radtileview.Items)
                {
                    var radItem = item as RadTileViewItem;
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
                foreach (var item in radtileview.Items)
                {
                    var radItem = item as RadTileViewItem;
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
                foreach (var item in radtileview.Items)
                {
                    var radItem = item as RadTileViewItem;
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
        EventHandler<ViewModel.LoadModuleEventArgs> LoadTaskHandler = null;
        private void CheckeDepends(string moduleName)
        {
            var module = ViewModel.Context.Managed.Catalog.FirstOrDefault(item => item.ModuleName == moduleName);
            if (module != null)
            {
                ViewModel.Context.Managed.OnSystemLoadModuleCompleted += LoadTaskHandler = (o, e) =>
                {
                    ViewModel.Context.Managed.OnSystemLoadModuleCompleted -= LoadTaskHandler;
                    if (e.Error == null)
                    {
                        AddOAWebPart();
                    }
                };

                ViewModel.Context.Managed.LoadModule(moduleName);
            }
        }

        private void AddOAWebPart()
        {
            var OAWebPaer = CreateOAWebPart();
            if (OAWebPaer != null)
            {

                RadTileViewItem item6 = new RadTileViewItem();
                item6.TileState = TileViewItemState.Minimized;
                item6.Header = "公司发文";
                item6.Content = OAWebPaer;
                radtileview.Items.Add(item6);
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

        public int Top
        {
            get;
            set;
        }

        public int RefDate
        {
            get;
            set;
        }

        public string Titel
        {
            get;
            set;
        }

        public void LoadDate()
        {
            
        }

        public void Initialize()
        {
           
        }

        public void Cleanup()
        {
            //this.Clear();
        }
    }
}
