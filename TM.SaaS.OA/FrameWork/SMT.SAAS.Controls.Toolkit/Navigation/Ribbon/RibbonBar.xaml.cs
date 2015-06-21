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
using System.Windows.Markup;
using System.Collections.ObjectModel;
using SMT.SAAS.AnimationEngine;
using Model = SMT.SAAS.AnimationEngine.Model;
using System.Windows.Threading;


namespace SMT.SAAS.Controls.Toolkit.Navigation.Ribbon
{
    //RibbonBar 
    //导航菜单一级
    //1.负责加载一级菜单以及根据一级菜单初始二级菜单.二级菜单使用RibbonGroup+RibbonButton布局
    //2.负责检测控制用户操作，比如移入、移出、切换一级菜单.以及根据一级菜单加载二级菜单等.
    //3.RibbonTab为一级菜单，RibbonGroup承载二级菜单以及三级菜单。
    //4.菜单目前只负责控制到3级。如果支持多级菜单方案待定

    [ContentPropertyAttribute("Items")]
    public partial class RibbonBar : UserControl
    {
        private double clickOffset = 8;//点击tabButton时产生的偏移量,处理选中后二级容器与选中效果的偏差
        private double tabButtonMargint = 20;//tab控件之间的间距
        private Thickness ContentMargin = new Thickness(10, 0, 10, 0);//tab容器与主容器的间距量
        private double contentHeightOffset = 30;
        private Thickness _groupMargin = new Thickness(10, 5, 10, 5);
        private static bool isClick = false;//判断用户是否首次进行操作
        private static bool isUsingRibbon = false;//判断用户是否在使用菜单
        private double splitLinesOffsetWidth = 2;



        public static readonly DependencyProperty ItemsProperty =
          DependencyProperty.Register("Items", typeof(ObservableCollection<RibbonTabButton>), typeof(RibbonBar),
                                        new PropertyMetadata(new PropertyChangedCallback(RibbonBar.OnItemsPropertyChanged)));

        public static readonly DependencyProperty RibbonBarWidthProperty =
         DependencyProperty.Register("RibbonBarWidth", typeof(double), typeof(RibbonBar),
                                       new PropertyMetadata(new PropertyChangedCallback(RibbonBar.OnRibbonBarWidthPropertyChanged)));
        /// <summary>
        /// RibbonBar元素
        /// </summary>
        public ObservableCollection<RibbonTabButton> Items
        {
            get { return (ObservableCollection<RibbonTabButton>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public double RibbonBarWidth
        {
            get { return (double)GetValue(RibbonBarWidthProperty); }
            set { SetValue(RibbonBarWidthProperty, value); }
        }


        public double RibbonBarHeight { get; set; }
        private DispatcherTimer _checkStateTimer = new DispatcherTimer();
        private int _waitTime = 300;

        /// <summary>
        /// 单击事件
        /// </summary>
        public event EventHandler<RibbonButtonClickArgs> Click;

        public RibbonBar()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(RibbonBar_Loaded);
        }
        void RibbonBar_Loaded(object sender, RoutedEventArgs e)
        {
            this.OnApplyTemplate();
            _checkStateTimer = new DispatcherTimer();
            _checkStateTimer.Interval = new TimeSpan(0, 0, 0, 0, _waitTime);
            _checkStateTimer.Tick += new EventHandler(_checkStateTimer_Tick);
            RibbonBarHeight = 36;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RibbonTabContent.Margin = ContentMargin;
        }
        /// <summary>
        /// 加载RibbonBar子元素
        /// </summary>
        private void LoadItems()
        {
            if (Items != null)
            {
                double maxLeft = 0;
                double maxTop = 0;
                double centerTop = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    #region 添加TabButton，即RibbonBarItem
                    RibbonTabButton item = Items[i];
                    Point size = GetTextWidthHeight(item.Titel, item.txbtitel);
                    maxLeft+=size.X;
                    item.txbtitel.Text = item.Titel;
                    item.Width = size.X + RibbonTabButton.OffsetWidth;
                    item.Height = RibbonTabContent.Height;
                    item.MouseLeftButtonDown += new MouseButtonEventHandler(item_MouseLeftButtonDown);
                    item.MouseEnter += new MouseEventHandler(item_MouseEnter);
                    item.MouseLeave += new MouseEventHandler(item_MouseLeave);
                    #endregion
                    #region 添加分隔符
                    if (i != 0)
                    {
                        Rectangle SplitLines = new Rectangle();
                        SplitLines.Height = 14;
                        SplitLines.Width = 2;
                        SplitLines.Margin = new Thickness(1, 10, 1, 0);
                        LinearGradientBrush _l = new LinearGradientBrush();
                        _l.StartPoint = new Point(0, 0.5);
                        _l.EndPoint = new Point(1, 0.5);
                        GradientStop _G1 = new GradientStop();
                        _G1.Color = Color.FromArgb(255, 8, 39, 99);
                        _G1.Offset = 0;
                        GradientStop _G2 = new GradientStop();

                        _G2.Color = Color.FromArgb(255, 8, 39, 99);
                        _G2.Offset = 0.5;
                        GradientStop _G3 = new GradientStop();
                        _G3.Color = Color.FromArgb(175, 255, 255, 255);
                        _G3.Offset = 0.5;
                        GradientStop _G4 = new GradientStop();
                        _G4.Color = Color.FromArgb(175, 255, 255, 255);
                        _G4.Offset = 1;
                        _l.GradientStops.Add(_G1);
                        _l.GradientStops.Add(_G2);
                        _l.GradientStops.Add(_G3);
                        _l.GradientStops.Add(_G4);
                        SplitLines.Fill = _l;
                        SplitLines.HorizontalAlignment = HorizontalAlignment.Left;

                        Canvas.SetLeft(SplitLines, maxLeft + tabButtonMargint/2-2);
                        RibbonTabContent.Children.Add(SplitLines);
                        maxLeft += tabButtonMargint;
                    }
                    #endregion
                    
                    Canvas.SetLeft(item, maxLeft);
                    RibbonTabContent.Children.Add(item);
                    //maxLeft += size.X+=4;
                }

                SetRibbonBarWidth(maxLeft + ContentMargin.Right + ContentMargin.Left);
            }
        }
        private void SetRibbonBarWidth(double newWideth)
        {
            if (!Double.IsNaN(ribbonBarbackground.Width))
            {
                if (ribbonBarbackground.Width < newWideth)
                    ribbonBarbackground.Width = newWideth;
            }
            else if (Double.IsNaN(ribbonBarbackground.Width) || ribbonBarbackground.Width <= 0)
            {
                ribbonBarbackground.Width = newWideth;
            }
        }
        void item_MouseLeave(object sender, MouseEventArgs e)
        {
            isUsingRibbon = false;
            _checkStateTimer.Start();
        }
        void _checkStateTimer_Tick(object sender, EventArgs e)
        {
            _checkStateTimer.Stop();
            if (!isUsingRibbon)
            {
                ChangeVisualState("Hide", false);
                isClick = false;
            }
        }
        void item_MouseEnter(object sender, MouseEventArgs e)
        {
            isUsingRibbon = true;
            RibbonTabButton source = sender as RibbonTabButton;
            if (source.Items.Count <= 0)
            {
                source.MouseLeftButtonDown += (obj, args) =>
                    {
                        if (Click != null)
                        {
                            Click(source, new RibbonButtonClickArgs() { DataContext = source.DataContext });
                        }
                    };
            }
            else
            {
                #region Load Items
                Point currentPoint = new Point() { X = Canvas.GetLeft(source) + ContentMargin.Left };
                if (!isClick)
                {
                    LoadTabItems(source);
                    RibbonGroupParentPanel.Visibility = Visibility.Visible;
                    TabSelectedBackground.Width = source.Width + clickOffset * 2;
                    Canvas.SetLeft(TabSelectedBackground, currentPoint.X - clickOffset);
                    Canvas.SetLeft(RibbonGroupParentPanel, currentPoint.X - clickOffset * 3);
                    ChangeVisualState("Show", false);
                    isClick = true;
                }
                else
                {
                    LoadTabItems(source);
                    double tabprevleft = Canvas.GetLeft(TabSelectedBackground);
                    double panelprevleft = Canvas.GetLeft(RibbonGroupParentPanel);
                    double tabcurrentleft = currentPoint.X - clickOffset;
                    double panelcurrentleft = currentPoint.X - clickOffset * 3;

                    List<IModel> _models = new List<IModel>() 
                    { 
                        new Model.DoubleModel() { Target = TabSelectedBackground, From = tabprevleft, To = tabcurrentleft, PropertyPath = ConstPropertyPath.CANVAS_LEFT, Duration = 0.2 },
                        new Model.DoubleModel() { Target = RibbonGroupParentPanel, From =panelprevleft, To = panelcurrentleft, PropertyPath = ConstPropertyPath.CANVAS_LEFT, Duration = 0.2 }
                     };
                    Engine.CreateStoryboard(_models).Begin();
                }
                TabSelectedBackground.BorderThickness = new Thickness(1, 1, 1, 0);
                #endregion
            }

        }
        /// <summary>
        /// 加载tab控件的子元素
        /// </summary>
        /// <param name="source"></param>
        private void LoadTabItems(RibbonTabButton source)
        {
            RibbonGroupPanel.Children.Clear();
            ObservableCollection<RibbonButtonModel> tempSource = new ObservableCollection<RibbonButtonModel>();
            #region 对数据进行分组
            IEnumerable<RibbonButtonModel> groups = from hasChildGroup in source.Items
                                                    where hasChildGroup.Items.Count > 0
                                                    select hasChildGroup;
            //获取拥有子元素的菜单
            if (groups.Count() > 0)
            {
                foreach (var item in groups)
                {
                    tempSource.Add(item);
                }
            }
            IEnumerable<RibbonButtonModel> NaNitemgroups = from NanChildGroup in source.Items
                                                           where NanChildGroup.Items.Count <= 0
                                                           select NanChildGroup;
            if (NaNitemgroups.Count() > 0)
            {
                //对不包含子元素的菜单进行重组
                ObservableCollection<RibbonButtonModel> restructureGroup = new ObservableCollection<RibbonButtonModel>();
                foreach (var item in NaNitemgroups)
                {
                    restructureGroup.Add(item);
                }
                RibbonButtonModel nanItemModel = new RibbonButtonModel() { Titel = source.Titel, Items = restructureGroup };
                tempSource.Add(nanItemModel);
            }
            #endregion
            //对分组以及重组后的集合进行按元素多少进行排序
            IEnumerable<RibbonButtonModel> orderSource = from groupItem in tempSource
                                                         orderby groupItem.Items.Count descending
                                                         select groupItem;
            double maxHeight = 0;
            //WrapPanel GroupParent = new WrapPanel()
            //{
            //    Orientation = Orientation.Vertical,
            //    Margin = new Thickness(5, 5, 5, 5),
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    VerticalAlignment = VerticalAlignment.Center
            //};
            StackPanel GroupParent = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 5, 5, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            foreach (RibbonButtonModel itemgroup in orderSource)
            {
                if (itemgroup.Items.Count > 0)
                {
                    RibbonButtonGroup ribbongroups = AddGroup(itemgroup);
                    ribbongroups.Margin = _groupMargin;
                    if (maxHeight < ribbongroups.GroupHeight)
                        maxHeight = ribbongroups.GroupHeight;

                    GroupParent.Children.Add(ribbongroups);
                }
            }
            double ParentHeight = maxHeight + 8;// groupMargin.Bottom + groupMargin.Top;// +contentHeightOffset;
            GroupParent.Height = ParentHeight;

            RibbonGroupPanel.Children.Add(GroupParent);
            SetParentHeight(ParentHeight);

            #region 页面分割栏 
            Rectangle SplitLines = new Rectangle();
            SplitLines.Height = 3;
            SplitLines.Fill = Application.Current.Resources["RibbonBarSplitLines1"] as Brush;
            Canvas.SetLeft(SplitLines, -1);
            Canvas.SetTop(SplitLines, -1.4);
            RibbonGroupPanel.Children.Add(SplitLines);
            #endregion

            GroupParent.SizeChanged += (obj, arg) =>
            {
                RibbonGroupPanel.Width = arg.NewSize.Width + 12;
                SplitLines.Width = arg.NewSize.Width + 12;
                RibbonGroupParentPanel.Width = arg.NewSize.Width + 12;

            };
        }

        private RibbonButtonGroup AddGroup(RibbonButtonModel groupModel)
        {
            RibbonButtonGroup groups = new RibbonButtonGroup() { GroupTitle = groupModel.Titel, Items = groupModel.Items };
            groups.Click += (obj, args) =>
            {
                if (Click != null)
                    Click(obj, args);
            };
            return groups;
        }

        /// <summary>
        /// 设置父容器大小
        /// </summary>
        /// <param name="parentHeight"></param>
        private void SetParentHeight(double parentHeight)
        {
            RibbonGroupPanel.Height = parentHeight;
            RibbonGroupParentPanel.Height = parentHeight;
            LayoutRoot.Height = RibbonBarHeight + parentHeight;
        }
        void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        public static void OnItemsPropertyChanged(DependencyObject objects, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                RibbonBar bases = objects as RibbonBar;
                bases.LoadItems();
            }
        }
        public static void OnRibbonBarWidthPropertyChanged(DependencyObject objects, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                RibbonBar bases = objects as RibbonBar;
                bases.SetRibbonBarWidth((double)args.NewValue);
            }
        }
        private Point GetTextWidthHeight(string text, TextBlock sourceText)
        {
            TextBlock tbtemp = new TextBlock();
            tbtemp.Text = text;
            tbtemp.Style = sourceText.Style;
            tbtemp.FontFamily = sourceText.FontFamily;
            tbtemp.FontSize = sourceText.FontSize;
            tbtemp.FontSource = sourceText.FontSource;
            tbtemp.FontStretch = sourceText.FontStretch;
            tbtemp.FontStyle = sourceText.FontStyle;
            tbtemp.FontWeight = sourceText.FontWeight;


            Point xy = new Point(tbtemp.ActualWidth, tbtemp.ActualHeight);
            return xy;
        }
        private void RibbonGroupParentPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            isUsingRibbon = false;
            _checkStateTimer.Start();


        }
        private void RibbonGroupParentPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            isUsingRibbon = true;

        }
        private void Hide_Completed(object sender, EventArgs e)
        {

        }
        private void ribbonBarbackground_MouseLeave(object sender, MouseEventArgs e)
        {
            TabSelectedBackground.BorderThickness = new Thickness(0);
            isUsingRibbon = false;
            _checkStateTimer.Start();
        }
        private void ChangeVisualState(string StateName, bool useTransitions)
        {
            VisualStateManager.GoToState(this, StateName, useTransitions);
        }

        private void TabSelectedBackground_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void TabSelectedBackground_MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }

    public class RibbonButtonClickArgs : EventArgs
    {
        public object DataContext { get; set; }
    }

}
