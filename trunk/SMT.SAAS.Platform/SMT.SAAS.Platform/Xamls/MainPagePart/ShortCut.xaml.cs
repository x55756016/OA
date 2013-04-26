//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 快捷48-菜单
// 完成日期：2011-05-16 
// 版    本：V1.0 
// 作    者：王玲 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System;
using System.Diagnostics;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class ShortCut : UserControl
    {
        public bool IsDeskTop = false;
        public bool IsShowStroke = true;
        public ShortCut()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Item_Loaded);
            this.Root.MouseEnter += new MouseEventHandler(img_MouseEnter);
            this.Root.MouseLeave += new MouseEventHandler(img_MouseLeave);
        }

        public static readonly DependencyProperty TitelProperty =
                      DependencyProperty.Register("Titel", typeof(string), typeof(ShortCut),
                      new PropertyMetadata(new PropertyChangedCallback(ShortCut.OnTitelPropertyChanged)));

        private static void OnTitelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShortCut item = d as ShortCut;
            item.tbTitel.Text = e.NewValue.ToString();

        }

        public string Titel
        {
            get { return (string)base.GetValue(TitelProperty); }
            set { base.SetValue(TitelProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
                    DependencyProperty.Register("Icon", typeof(string), typeof(ShortCut),
                    new PropertyMetadata(new PropertyChangedCallback(ShortCut.OnIconPropertyChanged)));

        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShortCut item = d as ShortCut;
            string uriString = string.Empty;
            if (e.NewValue != null)
            {
                 uriString = e.NewValue.ToString();
            }
            if(string.IsNullOrEmpty(uriString))
                uriString="/Images/icons/Comicon.png";
          
            var source = new BitmapImage(new Uri(uriString, UriKind.Relative));
            item.imIoc.Source = source;
        }

        public string Icon
        {
            get { return (string)base.GetValue(IconProperty); }
            set { base.SetValue(IconProperty, value); }
        }

        void img_MouseLeave(object sender, MouseEventArgs e)
        {
            sb2.Begin();
        }
        void img_MouseEnter(object sender, MouseEventArgs e)
        {
            sb1.Begin();
        }
        void Item_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
