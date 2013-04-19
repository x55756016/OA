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

using System.Globalization;
using System.Windows.Media.Imaging;


namespace SMT.SaaS.FrameworkUI
{
    public partial class GridPager : UserControl
    {
        public GridPager()
        {
            InitializeComponent();
            PageIndex = 1;
            PageButtonCount = 3;

            this.Loaded += new RoutedEventHandler(Pager_Loaded);
        }
        #region Properties
        public int PageSize { get; set; }

        private int pageCount;

        public int PageCount
        {
            get { return pageCount; }
            set
            {
                pageCount = value;
                BuildPager();
            }
        }
        /// <summary>
        /// Total Number of Pages for this Pager Control
        /// </summary>

        /// <summary>
        /// Current Page Index
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// Number of buttons to render before/after current selection
        /// For example, if = 2, buttons appear for a control with 100 pages
        /// /// Button 5 selected:
        ///     Previous 1 2 3 4 (5) 6 7 ... 99 100 Next
        /// Button 6 selected:
        ///     Previous 1 2 ... 4 5 (6) 7 8 ... 99 100 Next
        /// Button 95 selected
        ///     Previous 1 2 ... 93 94 (95) 96 97 ... 99 100 Next
        /// Button 96 selected
        ///     Previous 1 2 ... 94 95 (96) 97 98 99 100 Next
        /// </summary>
        public int PageButtonCount { get; set; }
        /// <summary>
        /// EVent delegate for a specific Pager Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void PagerButtonClick(object sender, RoutedEventArgs e);
        /// <summary>
        /// Event handler for a specific Pager Button
        /// </summary>
        public event PagerButtonClick Click;
        #endregion


        public GridPager(int pageCount, int pageButtonCount)
        {
            InitializeComponent();

            PageCount = pageCount;
            PageIndex = 1;
            PageButtonCount = pageButtonCount;

            this.Loaded += new RoutedEventHandler(Pager_Loaded);
        }

        protected void Pager_Loaded(object sender, RoutedEventArgs e)
        {
            BuildPager();
        }

        #region Internal
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
                MinWidth = 22,
                MaxWidth=30
            };

            if (inner == false)
            {
                b.MinWidth = 22;
                b.MaxWidth = 30;
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
                    Width = 30
                };
                Border b = new Border()
                {
                    Margin = new Thickness(3, 0, 3, 0),
                    BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5),

                    MinWidth = 20,
                    MaxWidth=30,
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

                    MinWidth = 20,
                    MaxWidth = 30,
                    Height = 18,
                    BorderThickness = new Thickness(1)
                };
            }
        }

        //省略号...
        private UIElement OmitBuildSpan(string text, bool border)
        {
            TextBlock t = new TextBlock()
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 30,
                FontSize=14
            };
            return t;
        }

        private void SetButtonEnable(Button btn, bool isEnabled)
        {
            btn.IsEnabled = isEnabled;
        }

        //显示当前分页按钮
        public void ShowPagerButton()
        {
            imgFL1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_FL1.png", UriKind.Relative));
            imgFL2.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_FL2.png", UriKind.Relative));
            imgL1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_L1.png", UriKind.Relative));
            imgR1.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/grid/page_R1.png", UriKind.Relative));
        }

        /// <summary>
        /// Build the Pager Control
        /// </summary>
        private void BuildPager()
        {
            this.spPager.Children.Clear();

            if (PageCount >= 1)
            {
                int min = PageIndex - PageButtonCount;
                int max = PageIndex + PageButtonCount;

                if (max > PageCount)
                    min -= max - PageCount;
                else if (min < 1)
                    max += 1 - min;

                if (PageIndex > 1)
                {
                    ShowPagerButton();
                    if (PageIndex == pageCount)
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

                    if (pageCount <= 1)
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
                        this.spPager.Children.Add(OmitBuildSpan("...", false));
                        needDiv = false;
                    }
                }
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles Pager Button click
        /// Sets proper PageIndex for rendering
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion

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
    }
}
