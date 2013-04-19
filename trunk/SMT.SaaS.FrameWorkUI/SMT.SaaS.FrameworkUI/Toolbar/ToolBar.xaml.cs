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
using System.Windows.Media.Imaging;

namespace SMT.SaaS.FrameworkUI
{
    public partial class ToolBar : UserControl
    {
        public ToolBar()
        {
            InitializeComponent();
        }

        public StackPanel ButtonContainer
        {
            get
            {
                return this.buttonContainer;
            }
        }
        public string TitleImageName
        {
            get
            {
                return this.imageTitle.Name;
            }
        }
        public string HelpButtonName
        {
            get
            {
                return this.ButtonHelp.Name;
            }
        }
        public void InitToolBarItem(List<ToolbarItem> list)
        {
            //初始化菜单按钮
            ButtonContainer.Children.Clear();

            List<ToolbarItem> bars = list;
            foreach (var bar in bars)
            {
                ImageButton btn = new ImageButton();
                btn.TextBlock.Text = bar.Title;
                btn.Name = bar.Key;
                btn.Tag = bar.Key;
                btn.Image.Source = new BitmapImage(new Uri(bar.ImageUrl, UriKind.Relative));
                btn.Click += new RoutedEventHandler(btn_Click);
                ButtonContainer.Children.Add(btn); 
            }
        }

        public event EventHandler<ToolBarItemClickArgs> ItemClicked;

        void btn_Click(object sender, RoutedEventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            OnItemClicked(btn.Tag.ToString());
        }

        private void OnItemClicked(string key)
        {
            if (ItemClicked != null)
            {
                ToolBarItemClickArgs args = new ToolBarItemClickArgs(key);
                ItemClicked(this, args);
            }
        }

        public void ShowItem(string key, bool isShow)
        {
            FrameworkElement item = this.FindName(key) as FrameworkElement;
            if (item != null)
            {
                item.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
            }
            if (this.imageTitle.Visibility == Visibility.Visible)
            {
                this.tbPanel.Margin = new Thickness(70, 0, 0, 0);
            }
            else
            {
                this.tbPanel.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        public class ToolBarItemClickArgs : EventArgs
        {
            private string _Key;
            public string Key
            {
                get
                {
                    return _Key;
                }
            }
            public ToolBarItemClickArgs(string key)
            {
                _Key = key;
            }
        }

    }
}
