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

namespace SMT.SaaS.DatePager
{
    public partial class SmtPager : UserControl
    {
        /// <summary>
        /// 切换页面事件委托
        /// </summary>
        public delegate void PagerButtonClick(object sender, RoutedEventArgs e);
        /// <summary>
        /// 切换页面事件
        /// </summary>
        public event PagerButtonClick Click;

        #region 定义变量
        /// <summary>
        /// 总页数
        /// </summary>
        //private int pageCount = 0;
        //public int PageCount
        //{
        //    get { return pageCount; }
        //    set { pageCount = value; }
        //}
        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            set { SetValue(PageCountProperty, value); }
        }

        public static readonly DependencyProperty PageCountProperty =
           DependencyProperty.Register("PageCount", typeof(int), typeof(SmtPager),
           new PropertyMetadata(new PropertyChangedCallback(OnPageCountChanged)));

        private static void OnPageCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SmtPager).BuildNumberButton();
        }

        /// <summary>
        /// 页索引
        /// </summary>
        private int pageIndex = 1;
        public int PageIndex
        {
            get { return pageIndex; }
            set { pageIndex = value; }
        }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// //显示按钮数
        /// </summary>
        private int buttonNumber = 10;
        public int ButtonNumber
        {
            get { return buttonNumber; }
            set { buttonNumber = value; }
        }
        /// <summary>
        /// 按钮边框颜色
        /// </summary>
        private Color blockBorderBrush = Colors.Orange;
        public Color BlockBorderBrush
        {
            get { return blockBorderBrush; }
            set { blockBorderBrush = value; }
        }
        /// <summary>
        /// 按钮大小
        /// </summary>
        private double buttonSize = 18;
        public double ButtonSize
        {
            get { return buttonSize; }
            set { buttonSize = value; }
        }

        private double fontSize = 11;
        public double FontNumberSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }
        private DoubleAnimation animation1;
        private DoubleAnimation animation2;
        #endregion

        public SmtPager()
        {
            InitializeComponent();
            this.FirstIcon.Click += new RoutedEventHandler(FirstIcon_Click);
            this.NextIcon.Click += new RoutedEventHandler(NextIcon_Click);
            this.PreIcon.Click += new RoutedEventHandler(PreIcon_Click);
            this.LastIcon.Click += new RoutedEventHandler(LastIcon_Click);
            //this.PrePager.Click += new RoutedEventHandler(PrePager_Click);          
            this.PrePager.MouseLeftButtonUp += new MouseButtonEventHandler(PrePager_MouseLeftButtonUp);
            //this.NextPager.Click += new RoutedEventHandler(NextPager_Click);
            this.NextPager.MouseLeftButtonDown += new MouseButtonEventHandler(NextPager_MouseLeftButtonDown);
            this.animation1 = this.storyboard.Children[0] as DoubleAnimation;
            this.animation2 = this.storyboard.Children[1] as DoubleAnimation;
            BuildNumberButton();

        }

      

      
        #region 生成分页序号按钮
        public void BuildNumberButton()
        {
            pageIndex = 1;
            this.spPager.Children.Clear();
            this.PrePager.Visibility = System.Windows.Visibility.Collapsed;
            this.NextPager.Visibility = System.Windows.Visibility.Collapsed;
            this.animation1.To = 0;
            this.animation2.To = 0;
            this.storyboard.Begin();
            if (PageCount <= ButtonNumber && PageCount != 1)
            {
                this.block.Height = this.block.Width = ButtonSize;
                this.block.BorderBrush = new SolidColorBrush(this.BlockBorderBrush);
                numbersCanvas.Width = this.PageCount * ButtonSize;
                numbersCanvas.Height = ButtonSize;
                numbersCanvas.VerticalAlignment = VerticalAlignment.Top;
                numbersCanvas.Margin = new Thickness(0, 2, 0, 0);
                for (int i = 1; i <= PageCount; i++)
                {
                    TextBlock num = new TextBlock();
                    num.Width = ButtonSize;
                    num.Height = ButtonSize;
                    num.FontSize = FontNumberSize;                  
                    num.Tag = i;
                    num.TextAlignment = TextAlignment.Center;
                    num.HorizontalAlignment = HorizontalAlignment.Center;
                    num.Cursor = Cursors.Hand;
                    num.Text = i.ToString();
                    num.MouseLeftButtonDown += new MouseButtonEventHandler(PagerButton_Click);
                    spPager.Children.Add(num);
                }
                SetButton("Ago");//前进按钮禁用"
            }
            if (PageCount > ButtonNumber)
            {
                #region ButtonNumber
                this.block.Height = this.block.Width = ButtonSize;
                this.block.BorderBrush = new SolidColorBrush(this.BlockBorderBrush);

                if (this.ButtonNumber > 1)
                    numbersCanvas.Width = this.ButtonNumber * ButtonSize;//计算显示按钮数
                else
                    numbersCanvas.Width = this.PageCount * ButtonSize;
                numbersCanvas.VerticalAlignment = VerticalAlignment.Top;
                numbersCanvas.Margin = new Thickness(0, 2, 0, 0);
                numbersCanvas.Height = ButtonSize;
                numbersCanvas.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, numbersCanvas.Width, numbersCanvas.Height + 2) };
                #endregion
                for (int i = 1; i <= PageCount; i++)
                {
                    TextBlock num = new TextBlock();
                    num.Width = ButtonSize;
                    num.Height = ButtonSize;
                    num.FontSize = FontNumberSize;
                    num.Tag = i;
                    //num.Margin = new Thickness(0, 2, 0, 0);
                    num.TextAlignment = TextAlignment.Center;
                    num.HorizontalAlignment = HorizontalAlignment.Center;
                    num.Cursor = Cursors.Hand;
                    num.Text = i.ToString();
                    num.MouseLeftButtonDown += new MouseButtonEventHandler(PagerButton_Click);
                    spPager.Children.Add(num);
                }
                this.NextPager.Visibility = System.Windows.Visibility.Visible;
                SetButton("Ago");//前进按钮禁用"
            }
            #region 总页数等一处理
            else if (PageCount == 1 && PageCount <= ButtonNumber)
            {
                this.block.Height = this.block.Width = ButtonSize;
                this.block.BorderBrush = new SolidColorBrush(this.BlockBorderBrush);
                numbersCanvas.Width = this.PageCount * ButtonSize;
                numbersCanvas.Height = ButtonSize;
                numbersCanvas.VerticalAlignment = VerticalAlignment.Top;
                numbersCanvas.Margin = new Thickness(0, 2, 0, 0);
                TextBlock num = new TextBlock();
                num.Width = ButtonSize;
                num.Height = ButtonSize;
                //num.Margin = new Thickness(0, 2, 0, 0);
                num.FontSize = FontNumberSize;
                num.Tag = 1;
                num.TextAlignment = TextAlignment.Center;
                num.HorizontalAlignment = HorizontalAlignment.Center;
                num.Cursor = Cursors.Hand;
                num.Text = 1.ToString();
                spPager.Children.Add(num);
                SetButton("All");//"All"://禁用所有"
                this.PrePager.Visibility = System.Windows.Visibility.Collapsed;
                this.NextPager.Visibility = System.Windows.Visibility.Collapsed;
                this.animation2.To = 0;
                this.storyboard.Begin();
            }
            #endregion
            else if (PageCount < 1)
            {
                SetButton("All");//"All"://禁用所有"
                this.block.BorderBrush = new SolidColorBrush();
                this.block.Height = this.block.Width = 0;
                this.PrePager.Visibility = System.Windows.Visibility.Collapsed;
                this.NextPager.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

        }
        #endregion

        #region 按钮分页事件
        /// <summary>
        /// 下一组按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NextPager_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (pageIndex % ButtonNumber == 0)
                pageIndex = pageIndex - 1;
            PageIndex = (PageIndex / ButtonNumber + 1) * ButtonNumber;
            PageIndex++;
            this.animation1.To = -(ButtonNumber * this.ButtonSize) * (PageIndex / ButtonNumber);
            this.animation2.To = (PageIndex - 1) % ButtonNumber * this.ButtonSize;
            this.storyboard.Begin();
            if (PageIndex == PageCount || (PageIndex / ButtonNumber + 1) == (PageCount / ButtonNumber) + (PageCount % ButtonNumber == 0 ? 0 : 1))
            {
                this.NextPager.Visibility = System.Windows.Visibility.Collapsed;
                SetButton("Retreat");//禁用后退唤醒前进按钮"
            }
            this.PrePager.Visibility = System.Windows.Visibility.Visible;
            SetButton("A");
            if (Click != null)
                Click(sender, e);
        }

        /// <summary>
        /// 上一组按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PrePager_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (pageIndex % ButtonNumber == 0)
                pageIndex = pageIndex - 1;
            PageIndex = (PageIndex / ButtonNumber) * ButtonNumber;
            PageIndex = PageIndex - ButtonNumber + 1;
            this.animation1.To = this.numbersCanvas.Width - (ButtonNumber * this.ButtonSize) * (PageIndex / ButtonNumber + 1);
            this.animation2.To = (PageIndex - 1) % ButtonNumber * this.ButtonSize;
            this.storyboard.Begin();
            if (PageIndex == 1)
            {
                this.PrePager.Visibility = System.Windows.Visibility.Collapsed;
                SetButton("C");//禁用前进
            }
            this.NextPager.Visibility = System.Windows.Visibility.Visible;
            SetButton("B");
            if (Click != null)
                Click(sender, e);
        }
        #endregion

        #region 向前 向后事件及生成的按钮事件
        /// <summary>
        /// 当分页按钮点击事件处理函数
        /// </summary>
        protected void PagerButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock index = e.OriginalSource as TextBlock;
            int p = PageIndex;
            if (!(PageIndex == int.Parse(index.Tag.ToString())))//本就是当前页不予处理
            {
                int.TryParse(index.Tag.ToString(), out p);
                this.animation2.To = (p - 1) % ButtonNumber * this.ButtonSize;
                this.storyboard.Begin();
                PageIndex = p;
                if (PageIndex != 1 && PageIndex != PageCount)
                {
                    SetButton("ABC");//唤醒所有按钮"
                }
                if (PageIndex == PageCount)
                {
                    SetButton("Retreat");//禁用后退唤醒前进按钮"
                }
                if (pageIndex == 1)
                {
                    SetButton("C");//禁用前进"
                }
                if (Click != null)
                    Click(sender, e);

            }
        }
        /// <summary>
        /// 最后页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LastIcon_Click(object sender, RoutedEventArgs e)
        {
            PageIndex = PageCount;
            this.animation1.To = -(ButtonNumber * this.ButtonSize) * (PageIndex / ButtonNumber - (PageIndex % ButtonNumber == 0 ? 1 : 0));
            this.animation2.To = (PageIndex - 1) % ButtonNumber * this.ButtonSize;
            this.storyboard.Begin();
            SetButton("Retreat");//后退按钮禁用"
            if (PageCount > ButtonNumber)
            {
                this.NextPager.Visibility = Visibility.Collapsed;
                this.PrePager.Visibility = Visibility.Visible;
            }
            else
            {
                this.NextPager.Visibility = Visibility.Collapsed;
                this.PrePager.Visibility = Visibility.Collapsed;
            }
            if (Click != null)
                Click(sender, e);
        }

        /// <summary>
        /// 向前一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PreIcon_Click(object sender, RoutedEventArgs e)
        {
            if (PageIndex == 1)
            {
                PageIndex = 1;
            }
            else
            {
                PageIndex--;
            }
            if (PageIndex % ButtonNumber == 0 && PageIndex != 1)
            {

                this.animation1.To = this.numbersCanvas.Width - (ButtonNumber * this.ButtonSize) * (PageIndex / ButtonNumber);
                this.animation2.To = (PageIndex - 1) % ButtonNumber * this.ButtonSize;
                this.storyboard.Begin();
                this.NextPager.Visibility = Visibility.Visible;
            }
            else
            {
                this.animation2.To = (PageIndex - 1) % ButtonNumber * this.ButtonSize;
                this.storyboard.Begin();
            }

            //判断是否是第一组按钮
            if (PageIndex == 1 || ((pageIndex == ButtonNumber ? pageIndex - 1 : pageIndex) / ButtonNumber == 0))
            {
                this.PrePager.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (PageIndex != 1 && PageIndex != PageCount)
            {
                SetButton("ABC");//唤醒所有按钮"
            }
            if (PageIndex == 1)
            {
                SetButton("Ago");//禁用前进唤醒后退按钮"
            }
            if (Click != null)
                Click(sender, e);
        }

        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NextIcon_Click(object sender, RoutedEventArgs e)
        {
            PageIndex++;
            if (PageIndex >= PageCount)
            {
                PageIndex = PageCount;
            }
            if (PageIndex % ButtonNumber == 1 && PageIndex != 1)
            {

                this.animation1.To = -(ButtonNumber * this.ButtonSize) * (PageIndex / ButtonNumber);
                this.animation2.To = (PageIndex - 1) % ButtonNumber * this.ButtonSize;
                this.storyboard.Begin();
                this.PrePager.Visibility = Visibility.Visible;
            }
            else
            {
                this.animation2.To = (PageIndex - 1) % ButtonNumber * this.ButtonSize;
                this.storyboard.Begin();
            }
            //判断是否是最后一组按钮
            if (PageIndex == PageCount || (PageIndex / ButtonNumber + (PageIndex % ButtonNumber == 0 ? 0 : 1)) == (PageCount / ButtonNumber) + (PageCount % ButtonNumber == 0 ? 0 : 1))
            {
                this.NextPager.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (PageIndex != 1 && PageIndex != PageCount)
            {
                SetButton("ABC");//唤醒所有按钮"
            }
            if (PageIndex == PageCount)
            {
                SetButton("Retreat");//禁用后退唤醒前进按钮"
            }
            if (Click != null)
                Click(sender, e);
        }

        /// <summary>
        /// 第一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FirstIcon_Click(object sender, RoutedEventArgs e)
        {
            PageIndex = 1;
            this.animation1.To = 0;
            this.animation2.To = (PageIndex - 1) % ButtonNumber * this.ButtonSize;
            this.storyboard.Begin();
            SetButton("Ago");//前进按钮禁用"
            if (PageCount > ButtonNumber)
            {
                this.NextPager.Visibility = Visibility.Visible;
                this.PrePager.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.NextPager.Visibility = Visibility.Collapsed;
                this.PrePager.Visibility = Visibility.Collapsed;
            }
            if (Click != null)
                Click(sender, e);

        }
        #endregion

        #region 设置按钮禁用与启用
        /// <summary>
        /// 设置前进后退按钮启用与禁用
        /// </summary>
        /// <param name="direction"></param>
        public void SetButton(string direction)
        {
            switch (direction)
            {
                case "Ago"://前进按钮禁用
                    this.FirstIcon.IsEnabled = false;
                    this.FirstIcon.Opacity = 0.6;
                    this.PreIcon.IsEnabled = false;
                    this.PreIcon.Opacity = 0.6;
                    this.NextIcon.IsEnabled = true;
                    this.NextIcon.Opacity = 1;
                    this.LastIcon.IsEnabled = true;
                    this.LastIcon.Opacity = 1;
                    break;
                case "Retreat"://后退按钮禁用
                    this.FirstIcon.IsEnabled = true;
                    this.FirstIcon.Opacity = 1;
                    this.PreIcon.IsEnabled = true;
                    this.PreIcon.Opacity = 1;
                    this.NextIcon.IsEnabled = false;
                    this.NextIcon.Opacity = 0.6;
                    this.LastIcon.IsEnabled = false;
                    this.LastIcon.Opacity = 0.6;
                    break;
                case "All"://禁用所有
                    this.FirstIcon.IsEnabled = false;
                    this.FirstIcon.Opacity = 0.6;
                    this.PreIcon.IsEnabled = false;
                    this.PreIcon.Opacity = 0.6;
                    this.NextIcon.IsEnabled = false;
                    this.NextIcon.Opacity = 0.6;
                    this.LastIcon.IsEnabled = false;
                    this.LastIcon.Opacity = 0.6;
                    break;
                case "C"://禁用前进
                    this.FirstIcon.IsEnabled = false;
                    this.FirstIcon.Opacity = 0.6;
                    this.PreIcon.IsEnabled = false;
                    this.PreIcon.Opacity = 0.6;
                    break;
                case "A"://唤醒前进
                    this.FirstIcon.IsEnabled = true;
                    this.FirstIcon.Opacity = 1;
                    this.PreIcon.IsEnabled = true;
                    this.PreIcon.Opacity = 1;
                    break;
                case "B"://唤醒后退
                    this.NextIcon.IsEnabled = true;
                    this.NextIcon.Opacity = 1;
                    this.LastIcon.IsEnabled = true;
                    this.LastIcon.Opacity = 1;
                    break;
                default://唤醒所有
                    this.FirstIcon.IsEnabled = true;
                    this.FirstIcon.Opacity = 1;
                    this.PreIcon.IsEnabled = true;
                    this.PreIcon.Opacity = 1;
                    this.NextIcon.IsEnabled = true;
                    this.NextIcon.Opacity = 1;
                    this.LastIcon.IsEnabled = true;
                    this.LastIcon.Opacity = 1;
                    break;
            }
        }
        #endregion
    }
}
