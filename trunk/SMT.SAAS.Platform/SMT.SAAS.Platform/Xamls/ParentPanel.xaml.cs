//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 主面板加载区域
// 完成日期：2011-05-19 
// 版    本：V1.0 
// 作    者：王玲 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using SMT.SAAS.Platform.Xamls.Icons;
using System;

namespace SMT.SAAS.Platform.Xamls
{
    public partial class ParentPanel : UserControl, IMainPanel
    {
        public event EventHandler Back;
        public ParentPanel()
        {
            InitializeComponent();
            Navigation("系统");
            if (System.Windows.Application.Current.Resources["CurrentSysUserID"] != null && System.Windows.Application.Current.Resources["MvcOpenRecordSource"] != null)
            {
                //从mvc平台打开，不需要显示关闭按钮
                TCloseButton.Visibility = Visibility.Collapsed;
            }
        }

        public UIElement PanelContent
        {
            get { return (UIElement)base.GetValue(PanelContentProperty); }
            set { base.SetValue(PanelContentProperty, value); }
        }

        public static readonly DependencyProperty PanelContentProperty =
                      DependencyProperty.Register("PanelContent", typeof(UIElement), typeof(ParentPanel),
                      new PropertyMetadata(new PropertyChangedCallback(ParentPanel.OnPanelContentPropertyChanged)));

        private static void OnPanelContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                ParentPanel item = d as ParentPanel;
                ICleanup oldValue = e.OldValue as ICleanup;

                if (oldValue != null)
                {
                    oldValue.Cleanup();
                    oldValue = null;
                }
                if (e.NewValue != null)
                {
                    item.ParentContent.Content = null;

                    item.ParentContent.Content = e.NewValue;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region 关闭当前窗口事件

        /// <summary>
        /// 关闭当前窗口事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCloseButton_Click(object sender, RoutedEventArgs e)
        {

            //System.Windows.Application.Current.Host.Content.IsFullScreen = System.Windows.Application.Current.Host.Content.IsFullScreen ? false : true; 
            ICleanup content = PanelContent as ICleanup;

            if (content != null)
            {

                content.Cleanup();
                PanelContent = null;
            }
            if (Back != null)
            {
                Back.Invoke(sender, e);
            }
        }
        #endregion

        #region 加载导航标签
        /// <summary>
        /// 加载导航标签
        /// </summary>
        /// <param name="name1"></param>
        public void Navigation(string name1)
        {
            Navigation(name1, false);
        }

        public void Navigation(string name1, bool isAppend)
        {
            //if (!isAppend)
            //{
            //    if (NavigationPanel.Children.Count > 0)
            //    {
            //        NavigationPanel.Children.Clear();
            //        Navigation("系统");
            //    }
            //}

            //NavigationPanel.Children.Add(new NavigationTemplate(name1));
        }
        #endregion


        public void Navigation(UIElement Content, string Titel)
        {
            if (DefaultContent != null)
            {
                DefaultContent.Visibility = Visibility.Collapsed;
            }

            this.Visibility = Visibility.Visible;

            this.PanelContent = Content;
            Navigation(Titel);
        }


        #region 前进导航事件
        public string ImgTag;
        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            ImgTag = img.Tag.ToString();
        }
        #endregion

        public UIElement DefaultContent
        {
            get;
            set;
        }


        public void SetTitel(string titel)
        {
            //if (NavigationPanel.Children.Count > 2)
            //{
            //    NavigationPanel.Children.RemoveAt(NavigationPanel.Children.Count - 1);
            //}

            //Navigation(titel, true);
        }

      

        private void FullScreenButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Host.Content.IsFullScreen = System.Windows.Application.Current.Host.Content.IsFullScreen ? false : true; 
        }
    }
}
