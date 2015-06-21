using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Markup;


namespace SMT.SAAS.Controls.Toolkit
{

    [TemplatePart(Name = "LayoutRootMI2", Type = typeof(Canvas))]
    [TemplatePart(Name = "ItemTextMI", Type = typeof(TextBlock))]
    [TemplatePart(Name = "ItemHighlightMI", Type = typeof(Canvas))]
    [TemplatePart(Name = "ItemHighlightrMI", Type = typeof(Rectangle))]
    [TemplatePart(Name = "ItemText_copyMI", Type = typeof(TextBlock))]
    [TemplateVisualState(Name = "ItemHighlightedMI", GroupName = "menuNavigationMI")]
    [TemplateVisualState(Name = "noneHighlightedMI", GroupName = "menuNavigationMI")]

    [ContentPropertyAttribute("items")]
    public class MenuItem : Control
    {
        #region 字段/事件
        public Canvas LayoutRootMI;
        public Canvas LayoutRootMI2;
        public TextBlock ItemTextMI;
        public Canvas ItemHighlightMI;
        public Rectangle ItemHighlightrMI;
        public TextBlock ItemText_copyMI;
        public Path arrow;
        public Path arrowHighlight;
        public Canvas ItemDropDownMI;
        public Grid baseRectMI;
        public StackPanel itemHolderMI;

        /// <summary>
        /// 是否有子菜单项
        /// </summary>
        private bool isNested; 
        public Point xy;
        //父容器对象
        public MenuItem parentMenuItem;
        public MenuBarItem parentMenuBarItem;
        public static readonly DependencyProperty itemsProperty =
               DependencyProperty.Register("items", typeof(ObservableCollection<MenuItem>), typeof(MenuItem), null);
        public static readonly DependencyProperty menuTextProperty =
                DependencyProperty.Register("MenuText", typeof(string), typeof(MenuItem), null);
        public static readonly DependencyProperty SelectedBgProperty =
                DependencyProperty.Register("SelectedBg", typeof(Brush), typeof(MenuItem), null);


        /// <summary>
        /// 单击事件
        /// </summary>
        public event RoutedEventHandler Click;

        #endregion
        #region 构造函数/方法
   
        public MenuItem()
        {
            this.DefaultStyleKey = typeof(MenuItem);
            this.Loaded += new RoutedEventHandler(MenuItem_Loaded);
            if (items == null)
                items = new ObservableCollection<MenuItem>();
            if (string.IsNullOrEmpty(MenuText))
                MenuText = "Default";

            isNested = false;
            parentMenuBarItem = null;
            parentMenuItem = null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //获得模版组件
            LayoutRootMI = (Canvas)GetTemplateChild("LayoutRootMI");
            LayoutRootMI2 = (Canvas)GetTemplateChild("LayoutRootMI2");
            ItemTextMI = (TextBlock)GetTemplateChild("ItemTextMI");
            ItemHighlightMI = (Canvas)GetTemplateChild("ItemHighlightMI");
            ItemHighlightrMI = (Rectangle)GetTemplateChild("ItemHighlightrMI");
            ItemText_copyMI = (TextBlock)GetTemplateChild("ItemText_copyMI");
            arrow = (Path)GetTemplateChild("arrow");
            arrowHighlight = (Path)GetTemplateChild("arrowHighlight");
            ItemDropDownMI = (Canvas)GetTemplateChild("ItemDropDownMI");
            baseRectMI = (Grid)GetTemplateChild("baseRectMI");
            itemHolderMI = (StackPanel)GetTemplateChild("itemHolderMI");

            //注册事件
            ItemHighlightMI.MouseEnter += new MouseEventHandler(ItemHighlight_MouseEnter);
            ItemHighlightMI.MouseLeave += new MouseEventHandler(ItemHighlight_MouseLeave);
            ItemHighlightMI.MouseLeftButtonDown += new MouseButtonEventHandler(ItemHighlight_MouseLeftButtonDown);
            ItemDropDownMI.MouseLeave += new MouseEventHandler(ItemDropDownMI_MouseLeave);

            // 设置大小
            ItemTextMI.Width = xy.X;
            ItemTextMI.Height = xy.Y;
            ItemText_copyMI.Width = xy.X;
            ItemText_copyMI.Height = xy.Y;


            //设置容器大小
            //父容器
            LayoutRootMI.Width = xy.X + 15;
            LayoutRootMI.Height = xy.Y + 10;
            //Item Canvas大小
            ItemHighlightMI.Width = xy.X + 29;
            ItemHighlightMI.Height = xy.Y + 10;
            //Item Rectangle大小
            ItemHighlightrMI.Width = xy.X + 20;
            ItemHighlightrMI.Height = xy.Y + 10;

        }

        void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.ApplyTemplate();
            //设置菜单文本
            if (!string.IsNullOrEmpty(MenuText))
            {
                ItemTextMI.Text = MenuText;
                ItemText_copyMI.Text = MenuText;
            }

            //如有子项则添加子菜单
            if (items != null && items.Count > 0)
            {
                //标识是否有子项属性为True
                isNested = true;
                //设置按钮可视
                arrow.Visibility = Visibility.Visible;
                arrowHighlight.Visibility = Visibility.Visible;

                 
                Point xy = GetLargest(items);
                xy.X += 39; //为箭头的位置增加空间

                //设置箭头的位置
                arrow.SetValue(Canvas.LeftProperty, this.xy.X + 39);
                arrowHighlight.SetValue(Canvas.LeftProperty, this.xy.X + 39);

                //设置子元素容器大小
                ItemDropDownMI.Width = xy.X + 46;
                baseRectMI.Width = xy.X + 46;

                //设置子元素容器的位置
                ItemDropDownMI.SetValue(Canvas.LeftProperty, LayoutRootMI.Width + 29);

                //高度是使用TextBox的高度+10是为了给TextBox周围留有空间，然后再乘以元素总数再加一点边距
                ItemDropDownMI.Height = (xy.Y + 10) * (items.Count) + 8;
                baseRectMI.Height = (xy.Y + 10) * (items.Count) + 8;


                foreach (MenuItem item in items)
                {
                    //添加前设置菜单项的大小
                    item.SetDimension(xy);
                    //设置父子菜单的父容器
                    item.parentMenuItem = this;
                    
                    itemHolderMI.Children.Add(item);
                }
            }
        }
        /// <summary>
        /// 设置大小
        /// </summary>
        internal void SetDimension(Point xy)
        {
            this.xy = xy;
        }
        /// <summary>
        /// 隐藏菜单项,递归隐藏子元素
        /// </summary>
        public void CollapseDropDown()
        {
            if (isNested)
            {
                foreach (MenuItem item in items)
                {
                    item.CollapseDropDown();
                }
            }
            ItemDropDownMI.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 隐藏菜单项
        /// </summary>
        internal void CollapseChildDropDownMenus()
        {
            foreach (MenuItem item in items)
            {
                item.CollapseDropDown();
            }
        }
        /// <summary>
        /// 显示菜单项列表
        /// </summary>
        public void ShowDropDown()
        {
            ItemDropDownMI.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 获取子元素中Size最大的
        /// </summary>
        public Point GetLargest(ObservableCollection<MenuItem> menuItems)
        {
            double width = 0;
            double height = 0;
            foreach (MenuItem item in menuItems)
            {
                Point xy = GetTextWidthHeight(item.MenuText, ItemTextMI);
                if (xy.X > width) 
                    width = xy.X;
                if (xy.Y > height) 
                    height = xy.Y;
            }
            return new Point(width, height);
        }

        /// <summary>
        /// 返回TextBox在设置了样式之后的大小，以便获取实际大小
        /// </summary>
        private Point GetTextWidthHeight(string text, TextBlock itemTextTB)
        {
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.Style = itemTextTB.Style;
            tb.FontFamily = itemTextTB.FontFamily;
            tb.FontSize = itemTextTB.FontSize;
            tb.FontSource = itemTextTB.FontSource;
            tb.FontStretch = itemTextTB.FontStretch;
            tb.FontStyle = itemTextTB.FontStyle;
            tb.FontWeight = itemTextTB.FontWeight;

            Point xy = new Point(tb.ActualWidth, tb.ActualHeight);
            return xy;
        }

        #endregion
        #region 事件
        private void ItemHighlight_MouseEnter(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "ItemHighlightedMI", true);
            //if it's a root level menuItem collapse all other drop downs
            if (parentMenuBarItem != null)
                parentMenuBarItem.CollapseChildDropDownMenus();
            else if (parentMenuItem != null)
                parentMenuItem.CollapseChildDropDownMenus();

            if (isNested)
            {
                ShowDropDown();
            }

        }
        private void ItemHighlight_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "noneHighlightedMI", true); 
        }
        void ItemDropDownMI_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapseDropDown();
            
        }
        private void ItemHighlight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isNested)
            {
                ShowDropDown();
            }
            else
            {
                if (Click != null)
                {
                    Click(this, new RoutedEventArgs());
                }
            }

        }
        #endregion
        #region 依赖属性
 
        public ObservableCollection<MenuItem> items
        {
            get { return (ObservableCollection<MenuItem>)GetValue(itemsProperty); }
            set { SetValue(itemsProperty, value); }
        }

        public Brush SelectedBg
        {
            get { return (Brush)GetValue(SelectedBgProperty); }
            set { SetValue(SelectedBgProperty, value); }
        } 

        public string MenuText
        {
            get { return (string)GetValue(menuTextProperty); }
            set { SetValue(menuTextProperty, value); }
        }
       
        #endregion
       
    }
}
