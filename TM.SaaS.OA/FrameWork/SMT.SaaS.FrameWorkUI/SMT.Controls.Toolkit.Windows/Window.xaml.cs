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
using SMT.SAAS.AnimationEngine;


namespace SMT.SAAS.Controls.Toolkit.Windows
{
    /// <summary>
    /// 窗口，核心类
    /// </summary>
    public partial class Window : UserControl
    {
        #region 对象声明
        public static int currentZIndex = 1;
        private string caption;
        private MouseClickManager mouseClickManager;
        public double TimeOffset = 0.3;
        private bool draggingEnabled;
        private const int HotSpotWidth = 3;
        private Point initialDragPoint;
        private Point initialResizePoint;
        private Point initialWindowLocation;
        private Point initialBarLocation;
        private Size hideWindowSize;
        private Size initialWindowSize;
        private double innerContentPresenterOffset;
        private bool isDragging;
        private bool isClose=true;
        public bool isMaximized;
        private bool isResizing;
        private const int MinWindowWidth = 60;
        private ResizeAnchor resizeAnchor;
        private bool resizeEnabled;
        private bool showMaxButton;
        private bool showMinButton;
        private bool isHideWindow = true;
        public bool AllMaximized = false;
        #region Dependency Property
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
            DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(Window), new PropertyMetadata(new PropertyChangedCallback(Window.OnHorizontalScrollBarVisibilityPropertyChanged)));
        public static readonly DependencyProperty MaxImageSourceProperty =
            DependencyProperty.Register("MaxImageSource", typeof(ImageSource), typeof(Window), null);
        public static readonly DependencyProperty IocPathProperty =
            DependencyProperty.Register("IocPath", typeof(string), typeof(Window), new PropertyMetadata(new PropertyChangedCallback(Window.OnIocPathProperty)));
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(Window), new PropertyMetadata(new PropertyChangedCallback(Window.OnVerticalScrollBarVisibilityPropertyChanged)));
        #endregion
        #endregion
        #region 事件
        public event EventHandler Closed;
        public event EventHandler Closing;
        public event EventHandler Dragged;
        public event EventHandler Maximized;
        public event EventHandler Minimized;
        public event EventHandler Normalized;
        public event EventHandler SetZIndex;
        #endregion
        #region 属性
        public object Content { get; set; }
        public string ID { get; set; }
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get
            {
                return (ScrollBarVisibility)base.GetValue(HorizontalScrollBarVisibilityProperty);
            }
            set
            {
                base.SetValue(HorizontalScrollBarVisibilityProperty, value);
            }
        }
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get
            {
                return (ScrollBarVisibility)base.GetValue(VerticalScrollBarVisibilityProperty);
            }
            set
            {
                base.SetValue(VerticalScrollBarVisibilityProperty, value);
            }
        }
       
        public bool DraggingEnabled
        {
            get
            {
                return this.draggingEnabled;
            }
            set
            {
                this.draggingEnabled = value;
            }
        }
        private bool CanResize
        {
            get
            {
                return (this.ResizeEnabled && (this.resizeAnchor != ResizeAnchor.None));
            }
        }
        public bool ResizeEnabled
        {
            get
            {
                return this.resizeEnabled;
            }
            set
            {
                this.resizeEnabled = value;
            }
        }
        public string Caption
        {
            get
            {
                return this.caption;
            }
            set
            {
                this.captionText.Text = value;
                this.caption = value;
            }
        }
        public ImageSource MaxImageSource
        {
            get
            {
                return (BitmapImage)base.GetValue(MaxImageSourceProperty);
            }
            set
            {
                base.SetValue(MaxImageSourceProperty, value);
            }
        }
        public Size HideWindowSize
        {
            get { return this.hideWindowSize; }
            set { this.hideWindowSize = value; }
        }
        public Point TaskBarPoint { get; set; }
        public bool IsHideWindow { get { return isHideWindow; } set { this.isHideWindow = value; } }
        public string IocPath
        {
            get { return (string)base.GetValue(IocPathProperty); }
            set { base.SetValue(IocPathProperty, value); }
        }
        public bool IsClose 
        {
            get { return this.isClose; }
            set { this.isClose = value; }
        }
        #endregion
        #region 构造函数
        public Window()
        {
            InitializeComponent();

            this.innerContentPresenterOffset = -1.0;
            this.isMaximized = false;
            this.showMinButton = true;
            this.showMaxButton = true;
            this.draggingEnabled = true;
            this.caption = "";
            this.isDragging = false;
            this.isResizing = false;
            this.resizeAnchor = ResizeAnchor.None;
            this.resizeEnabled = true;
            base.Loaded += new RoutedEventHandler(Windows_Loaded);
            base.LayoutUpdated += new EventHandler(this.Windows_LayoutUpdated);
        }
        public Window(bool isMinButton, bool isMaxButton)
            : this()
        {
            this.showMinButton = isMinButton;
            this.showMaxButton = isMaxButton;
        }
        #endregion
        #region 动态属性方法
        public static void OnIocPathProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window item = (Window)d;
            item.imgWindowIoc.Source = new BitmapImage(new Uri((string)e.NewValue, UriKind.Relative));
        }
        private static void OnHorizontalScrollBarVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window windows = d as Window;
            if ((windows != null) && (windows.scrollcontent != null))
            {
                //windows.scrollcontent.HorizontalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
            }
        }
        private static void OnVerticalScrollBarVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window windows = d as Window;
            if ((windows != null) && (windows.scrollcontent != null))
            {
                //windows.scrollcontent.VerticalScrollBarVisibility = (ScrollBarVisibility)e.NewValue;
            }
        }

        #endregion
        #region 事件方法
        void Windows_Loaded(object sender, RoutedEventArgs e)
        {
            this.mouseClickManager = new MouseClickManager(captionBar, 300);
            this.mouseClickManager.Click += new MouseButtonEventHandler(this.mouseClickManager_Click);
            this.mouseClickManager.DoubleClick += new MouseButtonEventHandler(this.mouseClickManager_DoubleClick);
            //this.scrollcontent.HorizontalScrollBarVisibility = this.HorizontalScrollBarVisibility;
            //this.scrollcontent.VerticalScrollBarVisibility = this.VerticalScrollBarVisibility;
            this.captionText.Text = this.caption;
            this.ToolBar.MouseLeftButtonDown += new MouseButtonEventHandler(ToolBar_MouseLeftButtonDown);
            minButton.Click += new RoutedEventHandler(this.minButton_Click);
            if (!this.showMinButton)
            {
                minButton.Visibility = Visibility.Collapsed;
            }
            maxButton.Click += new RoutedEventHandler(this.maxButton_Click);
            if (!this.showMaxButton)
            {
                maxButton.Visibility = Visibility.Collapsed;
            }
            closeButton.Click += new RoutedEventHandler(this.closeButton_Click);
            this.DefineDragEvents();
            this.DefineResizeEvents();
            Canvas.SetZIndex(this, currentZIndex);
            if (this.SetZIndex != null)
            {
                this.SetZIndex(this, EventArgs.Empty);
            }
            /**************************************************************************************************/
            if (AllMaximized == true)
            {
                DraggingEnabled = false;
                this.initialWindowSize.Width = !double.IsNaN(base.Width) ? base.Width : base.ActualWidth;
                this.initialWindowSize.Height = !double.IsNaN(base.Height) ? base.Height : base.ActualHeight;
                this.initialBarLocation.X = Canvas.GetLeft(this);
                this.initialBarLocation.Y = Canvas.GetTop(this);
                if (this.Maximized != null)
                {
                    this.Maximized(this, EventArgs.Empty);
                }
            }
        }
        void ToolBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetZIndex(this, currentZIndex++);
            if (this.SetZIndex != null)
            {
                this.SetZIndex(this, EventArgs.Empty);
            }
        }
        private void Windows_LayoutUpdated(object sender, EventArgs e)
        {
            if ((this.innerContentPresenterOffset == -1.0) && (this.scrollcontent != null))
            {
                this.innerContentPresenterOffset = base.ActualWidth - ((((((this.scrollcontent.ActualWidth - this.scrollcontent.Margin.Left) - this.scrollcontent.Margin.Right)))));// - this.scrollcontent.Padding.Left) - this.scrollcontent.Padding.Right) - this.scrollcontent.BorderThickness.Left) - this.scrollcontent.BorderThickness.Right);
                this.innerContentPresenterOffset = Math.Max(this.innerContentPresenterOffset, 0.0);
            }
        }
        private void captionBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DraggingEnabled)
            {
                Canvas.SetZIndex(this, currentZIndex++);
                if (this.SetZIndex != null)
                {
                    this.SetZIndex(this, EventArgs.Empty);
                }
                ((FrameworkElement)sender).CaptureMouse();
                this.initialDragPoint = e.GetPosition(base.Parent as UIElement);
                this.initialWindowLocation.X = Canvas.GetLeft(this);
                this.initialWindowLocation.Y = Canvas.GetTop(this);
                this.isDragging = true;
            }
        }
        private void captionBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DraggingEnabled)
            {
                ((FrameworkElement)sender).ReleaseMouseCapture();
                this.isDragging = false;
            }
            this.mouseClickManager.HandleClick(sender, null);
        }
        private void captionBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isDragging)
            {
                Point position = e.GetPosition(base.Parent as UIElement);
                Canvas parent = base.Parent as Canvas;
                double MaxWidth = this.Width * 0.8;
                double length = (this.initialWindowLocation.X + position.X) - this.initialDragPoint.X;
                if ((length >= (-MaxWidth)) && ((length + this.captionBar.ActualWidth) <= (parent.ActualWidth + MaxWidth)))
                {
                    Canvas.SetLeft(this, length);
                }
                double MaxHeight = 10;
                double num2 = (this.initialWindowLocation.Y + position.Y) - this.initialDragPoint.Y;
                if ((num2 >= -5.0) && ((num2 + this.captionBar.ActualHeight) <= parent.ActualHeight - MaxHeight))
                {
                    Canvas.SetTop(this, num2);
                }
                if (this.Dragged != null)
                {
                    this.Dragged(this, EventArgs.Empty);
                }
            }
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Closing += (obj, args) =>
            {
                if (this.IsClose)
                {
                    this.CloseWindow.Begin();
                    CloseWindow.Completed += (s, arg) => { this.Close(); };
                }
            };

            if (this.Closing != null)
                Closing(this, EventArgs.Empty);
        }
        private void window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CanResize)
            {
                ((FrameworkElement)sender).CaptureMouse();
                this.initialResizePoint = e.GetPosition(base.Parent as UIElement);
                this.initialWindowSize.Width = !double.IsNaN(base.Width) ? base.Width : base.ActualWidth;
                this.initialWindowSize.Height = !double.IsNaN(base.Height) ? base.Height : base.ActualHeight;
                this.initialWindowLocation.X = Canvas.GetLeft(this);
                this.initialWindowLocation.Y = Canvas.GetTop(this);
                this.isResizing = true;
            }
        }
        private void window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.ResizeEnabled && this.isResizing)
            {
                ((FrameworkElement)sender).ReleaseMouseCapture();
                this.isResizing = false;
            }

        }
        private void window_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.ResizeEnabled)
            {
                if (!this.isResizing)
                {
                    Point position = e.GetPosition(this.window);
                    if ((position.Y <= 10.0) && (position.X <= 10.0))
                    {
                        this.window.Cursor = Cursors.Hand;
                        this.resizeAnchor = ResizeAnchor.TopLeft;
                    }
                    else if ((position.Y <= 10.0) && (position.X >= (this.window.ActualWidth - 10.0)))
                    {
                        this.window.Cursor = Cursors.Hand;
                        this.resizeAnchor = ResizeAnchor.TopRight;
                    }
                    else if (position.Y <= 3.0)
                    {
                        this.window.Cursor = Cursors.SizeNS;
                        this.resizeAnchor = ResizeAnchor.Top;
                    }
                    else if ((position.Y >= (this.window.ActualHeight - 10.0)) && (position.X <= 10.0))
                    {
                        this.window.Cursor = Cursors.Hand;
                        this.resizeAnchor = ResizeAnchor.BottomLeft;
                    }
                    else if ((position.Y >= (this.window.ActualHeight - 10.0)) && (position.X >= (this.window.ActualWidth - 10.0)))
                    {
                        this.window.Cursor = Cursors.Hand;
                        this.resizeAnchor = ResizeAnchor.BottomRight;
                    }
                    else if (position.Y >= (this.window.ActualHeight - 3.0))
                    {
                        this.window.Cursor = Cursors.SizeNS;
                        this.resizeAnchor = ResizeAnchor.Bottom;
                    }
                    else if (position.X <= 3.0)
                    {
                        this.window.Cursor = Cursors.SizeWE;
                        this.resizeAnchor = ResizeAnchor.Left;
                    }
                    else if (position.X >= (this.window.ActualWidth - 3.0))
                    {
                        this.window.Cursor = Cursors.SizeWE;
                        this.resizeAnchor = ResizeAnchor.Right;
                    }
                    else
                    {
                        this.window.Cursor = null;
                        this.resizeAnchor = ResizeAnchor.None;
                    }
                }
                else
                {
                    Point point2 = e.GetPosition(base.Parent as UIElement);
                    double deltaX = point2.X - this.initialResizePoint.X;
                    double deltaY = point2.Y - this.initialResizePoint.Y;
                    switch (this.resizeAnchor)
                    {
                        case ResizeAnchor.Left:
                            this.ResizeLeft(deltaX);
                            break;

                        case ResizeAnchor.TopLeft:
                            this.ResizeLeft(deltaX);
                            this.ResizeTop(deltaY);
                            break;

                        case ResizeAnchor.Top:
                            this.ResizeTop(deltaY);
                            break;

                        case ResizeAnchor.TopRight:
                            this.ResizeRight(deltaX);
                            this.ResizeTop(deltaY);
                            break;

                        case ResizeAnchor.Right:
                            this.ResizeRight(deltaX);
                            break;

                        case ResizeAnchor.BottomRight:
                            this.ResizeRight(deltaX);
                            this.ResizeBottom(deltaY);
                            break;

                        case ResizeAnchor.Bottom:
                            this.ResizeBottom(deltaY);
                            break;

                        case ResizeAnchor.BottomLeft:
                            this.ResizeLeft(deltaX);
                            this.ResizeBottom(deltaY);
                            break;
                    }
                }
            }
        }
        private void maxButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.isMaximized)
            {
                DraggingEnabled =false;
                this.initialWindowSize.Width = !double.IsNaN(base.Width) ? base.Width : base.ActualWidth;
                this.initialWindowSize.Height = !double.IsNaN(base.Height) ? base.Height : base.ActualHeight;
                this.initialBarLocation.X = Canvas.GetLeft(this);
                this.initialBarLocation.Y = Canvas.GetTop(this);
                if (this.Maximized != null)
                {
                    this.Maximized(this, EventArgs.Empty);
                }
            }
            else
            {
                Storyboard PrepareWindow =
                    CommonAnimation.PrepareWindow(this, TimeOffset, new Point(0, 0), initialBarLocation, new Size(this.Width, this.Height), initialWindowSize, 0, 1, false);
                PrepareWindow.Begin();
                if (this.Normalized != null)
                {
                    this.Normalized(this, EventArgs.Empty);
                }
                DraggingEnabled = true;
            }
            this.isMaximized = !this.isMaximized;
        }
        private void minButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Minimized != null)
            {
                this.Minimized(this, EventArgs.Empty);
            }
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Canvas.SetZIndex(this, currentZIndex++);
            if (this.SetZIndex != null)
            {
                this.SetZIndex(this, EventArgs.Empty);
            }
        }
        private void mouseClickManager_Click(object sender, MouseButtonEventArgs e)
        {

        }
        private void mouseClickManager_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.showMaxButton)
            {
                if (!this.isMaximized)
                {
                    DraggingEnabled = false;
                    this.initialWindowSize.Width = !double.IsNaN(base.Width) ? base.Width : base.ActualWidth;
                    this.initialWindowSize.Height = !double.IsNaN(base.Height) ? base.Height : base.ActualHeight;
                    this.initialBarLocation.X = Canvas.GetLeft(this);
                    this.initialBarLocation.Y = Canvas.GetTop(this);
                    if (this.Maximized != null)
                    {
                        this.Maximized(this, EventArgs.Empty);
                    }
                }
                else
                {
                   
                    Storyboard PrepareWindow = CommonAnimation.PrepareWindow
                        (this, TimeOffset, new Point(0, 0), initialBarLocation, new Size(this.Width, this.Height), initialWindowSize, 0, 1, false);
                    PrepareWindow.Begin();
                    if (this.Normalized != null)
                    {
                        this.Normalized(this, EventArgs.Empty);
                    }
                    DraggingEnabled = true;
                }
                this.isMaximized = !this.isMaximized;
            }
        }
        #endregion
        #region 方法
        public void Close()
        {
            base.Visibility = Visibility.Collapsed;
            if (this.Closed != null)
            {
                this.Closed(this, EventArgs.Empty);
                //if (this.Content is SAAS.Controls.ICleanup)
                //{
                //    (this.Content as ICleanup).Cleanup();
                //}
                this.Content = null;
                GC.Collect();
            }
        }
        private void DefineDragEvents()
        {
            if (this.captionBar != null)
            {
                this.captionBar.MouseLeftButtonDown += new MouseButtonEventHandler(this.captionBar_MouseLeftButtonDown);
                this.captionBar.MouseMove += new MouseEventHandler(this.captionBar_MouseMove);
                this.captionBar.MouseLeftButtonUp += new MouseButtonEventHandler(this.captionBar_MouseLeftButtonUp);
            }
        }
        private void DefineResizeEvents()
        {
            if (this.window != null)
            {
                this.window.MouseLeftButtonDown += new MouseButtonEventHandler(this.window_MouseLeftButtonDown);
                this.window.MouseMove += new MouseEventHandler(this.window_MouseMove);
                this.window.MouseLeftButtonUp += new MouseButtonEventHandler(this.window_MouseLeftButtonUp);
            }
        }
        private void ResizeLeft(double deltaX)
        {
            double num = (this.initialWindowLocation.X + this.initialWindowSize.Width) - 60.0;
            Canvas.SetLeft(this, Math.Min(this.initialResizePoint.X + deltaX, num));
            base.Width = Math.Max((double)(this.initialWindowSize.Width - deltaX), (double)60.0);
        }
        private void ResizeBottom(double deltaY)
        {
            base.Height = Math.Max(this.initialWindowSize.Height + deltaY, this.captionBar.ActualHeight);
        }
        private void ResizeRight(double deltaX)
        {
            base.Width = Math.Max((double)(this.initialWindowSize.Width + deltaX), (double)60.0);
        }
        private void ResizeTop(double deltaY)
        {
            //double num = (this.initialWindowLocation.Y + this.initialWindowSize.Height) - this.captionBar.ActualHeight;
            //Canvas.SetTop(this, Math.Min(this.initialResizePoint.Y + deltaY, num));
            //base.Height = Math.Max(this.initialWindowSize.Height - deltaY, this.captionBar.ActualHeight);
        }
        #endregion
        #region 枚举/鼠标位置
        private enum ResizeAnchor
        {
            None,
            Left,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft
        }
        #endregion
    }
}
