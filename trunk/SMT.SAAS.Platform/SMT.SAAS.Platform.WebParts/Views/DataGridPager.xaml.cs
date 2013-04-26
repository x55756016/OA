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
using System.Globalization;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class DataGridPager : UserControl
    {
        /// <summary>
        /// 切换页面事件委托
        /// </summary>
        public delegate void PagerButtonClick(object sender, RoutedEventArgs e);
        /// <summary>
        /// 切换页面事件
        /// </summary>
        public event PagerButtonClick Click;

        #region 依赖属性

        #region PageCount 总页数

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            set { SetValue(PageCountProperty, value); }
        }

        public static readonly DependencyProperty PageCountProperty =
           DependencyProperty.Register("PageCount", typeof(int), typeof(DataGridPager),
           new PropertyMetadata(new PropertyChangedCallback(OnPageCountChanged)));

        private static void OnPageCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as DataGridPager).BuildPager();
        }

        #endregion

        #region PageSize 总页数

        /// <summary>
        /// 每页显示的条数
        /// </summary>
        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        public static readonly DependencyProperty PageSizeProperty =
           DependencyProperty.Register("PageSize", typeof(int), typeof(DataGridPager),
           null);
        #endregion

        #region PageIndex 当前页索引
        public static readonly DependencyProperty PageIndexProperty =
           DependencyProperty.Register("PageIndex", typeof(int), typeof(DataGridPager), null);
        /// <summary>
        /// 当前页索引
        /// </summary>
        public int PageIndex
        {
            get { return (int)GetValue(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }
        #endregion

        #region PageButtonCount 要显示按钮总数
        public static readonly DependencyProperty ButtonCountProperty =
           DependencyProperty.Register("ButtonCount", typeof(int), typeof(DataGridPager),
           null);
        /// <summary>
        /// 要显示按钮总数
        /// </summary>
        public int ButtonCount
        {
            get { return (int)GetValue(ButtonCountProperty); }
            set { SetValue(ButtonCountProperty, value); }
        }
        #endregion

        #endregion

        public DataGridPager()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Pager_Loaded);
        }

        protected void Pager_Loaded(object sender, RoutedEventArgs e)
        {
            BuildPager();
        }

        /// <summary>
        /// 设置PagerButton的图片
        /// </summary>
        public void ShowPagerButton()
        {
            imgFL1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_FL1.png", UriKind.Relative));
            imgFL2.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_FL2.png", UriKind.Relative));
            imgL1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_L1.png", UriKind.Relative));
            imgR1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_R1.png", UriKind.Relative));
        }

        #region 私有方法
        /// <summary>
        /// Renders a Button control for the Pager
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="inner">Flag to denote if this refers to the inner buttons or the Previous/Next buttons</param>
        /// <param name="enabled">Flag to denote if this button is enabled</param>
        /// <returns>Button</returns>
        private Button BuildButton(string text, bool inner, bool enabled)
        {
            Button b = new Button()
            {
                Content = text,
                Tag = text,
                Style = (Style)Application.Current.Resources["ButtonToolBarStyle"],
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Top,
                MinWidth = 12,
                MaxWidth = 25
            };

            if (inner == false)
            {
                b.MinWidth = 12;
                b.MaxWidth = 25;
            }

            b.IsEnabled = enabled;
            if (enabled == false)
            {
                //b.BorderThickness = new Thickness(1);
            }

            b.Click += new RoutedEventHandler(PagerButton_Click);

            return b;
        }
        /// <summary>
        /// Renders a selected Button or the Hellip (...) TextBlock
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="border">Flag to denote if this button is to have a border</param>
        /// <returns>UIElement (either a TextBlock or a Border with a TextBlock within)</returns>
        private UIElement BuildSpan(string text, bool border)
        {
            if (border)
            {
                TextBlock t = new TextBlock()
                {
                    Text = text,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 25
                };
                Border b = new Border()
                {
                    Margin = new Thickness(3, 0, 3, 0),
                    BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5),

                    MinWidth = 10,
                    MaxWidth = 25,
                    Height = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                b.Child = t;
                return b;
            }
            else
            {
                return new Button()
                {
                    Content = text,
                    Style = (Style)Application.Current.Resources["ButtonToolBarStyle"],
                    Margin = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Top,

                    MinWidth = 10,
                    MaxWidth = 25,
                    Height = 18,
                    BorderThickness = new Thickness(1)
                };
            }
        }

        private void SetButtonEnable(Button btn, bool isEnabled)
        {
            btn.IsEnabled = isEnabled;
        }

        /// <summary>
        /// 生成分页按钮
        /// </summary>
        private void BuildPager()
        {
            this.spPager.Children.Clear();

            if (PageCount >= 1)
            {
                int min = PageIndex - ButtonCount;
                int max = PageIndex + ButtonCount;

                if (max > PageCount)
                    min -= max - PageCount;
                else if (min < 1)
                    max += 1 - min;

                if (PageIndex > 1)
                {
                    ShowPagerButton();
                    if (PageIndex == PageCount)
                    {
                        imgFL2.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_FL3.png", UriKind.Relative));
                        imgR1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_R0.png", UriKind.Relative));
                    }
                }
                else
                {
                    ShowPagerButton();
                    imgFL1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_FL0.png", UriKind.Relative));
                    imgL1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_L0.png", UriKind.Relative));

                    if (PageCount <= 1)
                    {
                        imgFL2.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_FL3.png", UriKind.Relative));
                        imgR1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_R0.png", UriKind.Relative));
                    }
                }

                // Middle Buttons
                bool needDiv = false;
                for (int i = 1; i <= PageCount; i++)
                {
                    if (i <= 2 || i > PageCount - 2 || (min <= i && i <= max))
                    {
                        string text = i.ToString(NumberFormatInfo.InvariantInfo);

                        if (i == PageIndex) // Currently Selected Index
                            this.spPager.Children.Add(BuildSpan(text, false));
                        else
                        {
                            this.spPager.Children.Add(BuildButton(text, true, true));
                        }
                        needDiv = true;
                    }
                    else if (needDiv)
                    {
                        // This will add the hellip (...) TextBlock
                        this.spPager.Children.Add(BuildSpan("...", false));
                        needDiv = false;
                    }
                }
            }
        }
        #endregion

        #region 按钮切换事件处理函数
        /// <summary>
        /// 当分页按钮点击事件处理函数
        /// </summary>
        protected void PagerButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = e.OriginalSource as Button;

            int p = PageIndex;
            int.TryParse(b.Tag.ToString(), out p);
            PageIndex = p;

            BuildPager();

            if (Click != null)
                Click(sender, e);
        }

        private void BFirstL_Click(object sender, RoutedEventArgs e)
        {
            PageIndex = 1;
            BuildPager();

            if (Click != null)
                Click(sender, e);
        }

        private void BPrivious_Click(object sender, RoutedEventArgs e)
        {
            PageIndex--;
            BuildPager();

            if (Click != null)
                Click(sender, e);
        }

        private void BNext_Click(object sender, RoutedEventArgs e)
        {
            PageIndex++;
            if (PageIndex >= PageCount)
            {
                PageIndex = PageCount;
            }

            BuildPager();

            if (Click != null)
                Click(sender, e);
        }

        private void BLastR_Click(object sender, RoutedEventArgs e)
        {
            PageIndex = PageCount;
            BuildPager();

            if (Click != null)
                Click(sender, e);
        }

        #endregion
    }
}
