using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using SMT.SAAS.Controls.Toolkit.Windows;
using System.Windows.Controls.Primitives;
using SMT.SAAS.AnimationEngine;
using Model = SMT.SAAS.AnimationEngine.Model;
using System.Collections.Generic;
using System.Windows.Threading;

namespace System.Windows.Controls
{
    /// <summary>
    /// 消息窗口.
    /// 弹出模式对话框或者浮动对话框
    /// <remarks>
    /// 消息窗口.
    /// 弹出模式对话框或者浮动对话框
    /// </remarks>
    /// </summary>
    public sealed class MessageWindow
    {
        /// <summary>
        /// 窗口返回的结果
        /// </summary>
        public static object Result
        {
            get;
            set;
        }
        /// <summary>
        /// 创建一个新的<see cref="MessageWindow"/>实例
        /// </summary>
        private MessageWindow()
        {
        }
        private static Popup _Popup;
        private static Popup Popup
        {
            get
            {
                if (_Popup.IsNull())
                {
                    _Popup = new Popup();
                }
                return _Popup;
            }
            set
            {
                _Popup = value;
            }
        }
        private static DialogMode _DialogMode { get; set; }
        private static Window _ApplicationModal;
        private static DispatcherTimer _refdateTimer;

        /// <summary>
        /// 显示一个只包含内容的窗口.
        /// 默认标题为Message
        /// </summary>
        /// <param name="content">要显示的内容</param>
        public static void Show(object content)
        {
            MessageWindow.Show("Message", content);
        }
        /// <summary>
        /// 显示一个消息窗
        /// </summary>
        /// <param name="caption">窗口标题</param>
        /// <param name="message">内容</param>
        /// <param name="icon">图标类型.为<see cref="MessageIcon"/>类型</param>
        /// <param name="type">窗口类型.为<see cref="MessageWindowType"/>类型
        /// Default为默认窗口.Flow为浮动窗口
        /// </param>
        public static void Show(string caption, string message, MessageIcon icon, MessageWindowType type)
        {
            switch (type)
            {
                case MessageWindowType.Default: MessageWindow.Show(caption, message, icon);
                    break;
                case MessageWindowType.Flow: ShowFlowWindow(message, icon);
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// 打开一个浮动窗口
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="icon">图标类型.为<see cref="MessageIcon"/>类型</param>
        private static void ShowFlowWindow(string message, MessageIcon icon)
        {
            //Get image path
            string IocPath = string.Format("/SMT.SaaS.Window;Component/Resources/{0}.png", icon.ToString());
            //create content
            Size size = new Size(0, 0);

            Grid grid = new Grid() { Opacity = 1 };
            grid.Margin = new Thickness(4);
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            //grid.Effect = new DropShadowEffect() {BlurRadius=2};

            TransformGroup group = new TransformGroup();
            ScaleTransform scaleTrans = new ScaleTransform();
            RotateTransform rotateTrans = new RotateTransform();
            TranslateTransform translateTrans = new TranslateTransform() { Y = 100 };
            SkewTransform skewTrans = new SkewTransform();
            group.Children.Add(scaleTrans);
            group.Children.Add(skewTrans);
            group.Children.Add(rotateTrans);
            group.Children.Add(translateTrans);
            grid.RenderTransform = group;

            Border _background = new Border() { BorderThickness = new Thickness(1) };
            _background.Background = Application.Current.Resources["FlowMessageBackgroundBrush"] as Brush;
            _background.BorderBrush = Application.Current.Resources["FlowMessageBorderSolidBrush"] as Brush;
            _background.CornerRadius = new CornerRadius(3);
            Grid.SetColumn(_background, 0);
            Grid.SetColumnSpan(_background, 2);
            Grid.SetRow(_background,0);
            Grid.SetRowSpan(_background,3);

            grid.Children.Add(_background);

            if (icon != MessageIcon.None)
            {
                Rectangle rect = new Rectangle() { Margin = new Thickness(20, 5, 5, 5) };
                ImageBrush imgbr = new ImageBrush();
                imgbr.ImageSource = new BitmapImage(new Uri(IocPath, UriKind.RelativeOrAbsolute));
                imgbr.Stretch = Stretch.Fill;
                rect.Fill = imgbr;
                rect.Height = 24; rect.Width = 24;
                rect.HorizontalAlignment = HorizontalAlignment.Center;
                rect.VerticalAlignment = VerticalAlignment.Center;

                Grid.SetColumn(rect, 0);
                Grid.SetRow(rect, 0);
                grid.Children.Add(rect);
            }

            TextBlock txbTitle = new TextBlock() { Text = "系统", FontFamily = new FontFamily("Arial,SimSun"), FontSize = 16, Foreground = new SolidColorBrush(Colors.Black), Margin = new Thickness(5, 5, 5, 2) };
            txbTitle.HorizontalAlignment = HorizontalAlignment.Left;
            txbTitle.VerticalAlignment = VerticalAlignment.Center;
            txbTitle.MinWidth = 250.0;
            Grid.SetColumn(txbTitle,1);
            Grid.SetRow(txbTitle,0);
            grid.Children.Add(txbTitle);

            TextBlock txbInfo = new TextBlock() { Text = message, FontFamily = new FontFamily("Arial,SimSun"), FontSize = 18, Foreground = new SolidColorBrush(Colors.Black), Margin = new Thickness(20, 5, 20, 10) };
            txbInfo.HorizontalAlignment = HorizontalAlignment.Left;
            txbInfo.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(txbInfo, 0);
            Grid.SetRow(txbInfo, 2);
            Grid.SetColumnSpan(txbInfo,2);
            grid.Children.Add(txbInfo);

            Border lborder = new Border() { Height = 1, CornerRadius =new CornerRadius(1.0), Margin=new Thickness (20,0,5,5)};
            lborder.Background = Application.Current.Resources["FlowMessageLineBorderSolidBrush"] as Brush;
            Grid.SetColumn(lborder, 0);
            Grid.SetRow(lborder, 1);
            Grid.SetColumnSpan(lborder, 2);
            grid.Children.Add(lborder);

            Model.DoubleKeyFramesModel transXkeyframes =

           new SMT.SAAS.AnimationEngine.Model.DoubleKeyFramesModel()
           {
               Target = grid,
               BeginTime = 0,
               PropertyPath = ConstPropertyPath.UIELEMENT_TRANSLATETRS_Y
           };

            transXkeyframes.KeyFrames = new SMT.SAAS.AnimationEngine.Model.KeyFrames<Double>[] 
            { 
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0,Value=100},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0.8,Value=0},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 1.6,Value=0},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 2.4,Value=-100}
            };

            Model.DoubleKeyFramesModel opactytykeyframes =
            new SMT.SAAS.AnimationEngine.Model.DoubleKeyFramesModel()
            {
                Target = grid,
                BeginTime = 0,
                PropertyPath = ConstPropertyPath.UIELEMENT_OPACITY
            };

            opactytykeyframes.KeyFrames = new SMT.SAAS.AnimationEngine.Model.KeyFrames<Double>[] 
            { 
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0,Value=0},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0.8,Value=1},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 1.6,Value=1},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 2.4,Value=0}
            };

            List<IModel> _models = new List<IModel>()
            {
               transXkeyframes,
               opactytykeyframes
            };

            Storyboard _flowStroyboard = Engine.CreateStoryboard(_models);
            

            _flowStroyboard.Completed += (o, e) =>
                {
                    if (Popup.IsOpen)
                    {
                        Popup.IsOpen = false;
                    }
                };

            if (!Popup.IsOpen)
            {
                Point point = new Point(1024 / 2, 768 / 2);
                Size windowSize = new Size(1024, 768);
                if (WindowsManager.Desktop != null)
                {
                    point = new Point(WindowsManager.Desktop.ActualWidth / 2, WindowsManager.Desktop.ActualHeight / 2);
                    windowSize.Height = WindowsManager.Desktop.ActualHeight;
                    windowSize.Width = WindowsManager.Desktop.ActualWidth;
                }
                else
                {
                    //new
                    var parent = (Application.Current.RootVisual as UserControl);
                    point = new Point(parent.ActualWidth / 2, parent.ActualHeight / 2);
                    windowSize.Height = parent.ActualHeight;
                    windowSize.Width = parent.ActualWidth;
                }

                Canvas canvas = new Canvas() { Width = windowSize.Width, Height = windowSize.Height };

                Rectangle Overlay = new Rectangle() { Width = windowSize.Width, Height = windowSize.Height };
                Overlay.Fill = new SolidColorBrush(Colors.Gray);

                Overlay.Opacity = .3;
                //canvas.Children.Add(Overlay);


                Canvas.SetLeft(grid, (point.X - 150 - grid.ActualWidth / 2));
                Canvas.SetTop(grid, (point.Y - 200 - grid.ActualHeight / 2));
                canvas.Children.Add(grid);
                Popup.IsOpen = true;
                Popup.Child = canvas;
                _flowStroyboard.Begin();
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// 打开一个窗口
        /// </summary>
        /// <param name="caption">标题</param>
        /// <param name="content">内容</param>
        public static void Show(string caption, object content)
        {
            MessageWindow.Show(caption, content, MessageIcon.None);
        }
        /// <summary>
        /// 打开一个窗口
        /// </summary>
        /// <param name="caption">标题</param>
        /// <param name="content">内容</param>
        /// <param name="icon">图标类型.为<see cref="MessageIcon"/>类型</param>
        public static void Show(string caption, object content, MessageIcon icon)
        {
            MessageWindow.Show(DialogMode.ApplicationModal, null, caption, content, icon);
        }
        /// <summary>
        /// 打开一个窗口
        /// </summary>
        /// <param name="dialogMode">窗口模式.为<see cref="DialogMode"/>类型</param>
        /// <param name="container">缺省,此参数已过期</param>
        /// <param name="caption">标题</param>
        /// <param name="content">内容</param>
        /// <param name="icon">图标类型.为<see cref="MessageIcon"/>类型</param>
        public static void Show(DialogMode dialogMode, FrameworkElement container, string caption, object content, MessageIcon icon)
        {
            MessageWindow.Show<object>(dialogMode, container, caption, content, icon, null, null);
        }
        /// <summary>
        /// 模态化窗口
        /// </summary>
        /// <typeparam name="TResult">返回结果的类型</typeparam>
        /// <param name="caption">标题</param>
        /// <param name="content">内容</param>
        /// <param name="icon">标示</param>
        /// <param name="result">结果</param>
        /// <param name="defaultResult">默认结果</param>
        /// <param name="buttons">显示按钮</param>
        public static void Show<TResult>(string caption, object content, MessageIcon icon, Action<TResult> result, TResult defaultResult, params TResult[] buttons)
        {
            MessageWindow.Show<TResult>(DialogMode.ApplicationModal, null, caption, content, icon, result, defaultResult, buttons);
        }
        /// <summary>
        /// 打开一个窗口
        /// </summary>
        /// <typeparam name="TResult">窗口返回的类型</typeparam>
        /// </summary>
        /// <param name="dialogMode">窗口模式.为<see cref="DialogMode"/>类型</param>
        /// <param name="container">缺省,此参数已过期</param>
        /// <param name="caption">标题</param>
        /// <param name="content">内容</param>
        /// <param name="icon">图标类型.为<see cref="MessageIcon"/>类型</param>
        /// <param name="result">点击按钮执行的Action</param>
        /// <param name="defaultResult">默认结果</param>
        /// <param name="buttons">要显示的按钮.一般使用String集合</param>
        public static void Show<TResult>(DialogMode dialogMode, FrameworkElement container, string caption, object content, MessageIcon icon, Action<TResult> result, TResult defaultResult, params TResult[] buttons)
        {
            SMT.SAAS.Controls.Toolkit.Windows.Window window = null;
            _DialogMode = dialogMode;

            #region 根据选项判断弹出框大小    
            Size size = new Size(300, 150);
            switch (icon)
            {
                case MessageIcon.None:
                    size = new Size(300, 150);             
                    break;
                case MessageIcon.Information:
                    size = new Size(300, 150);
                    break;
                case MessageIcon.Exclamation:
                    size = new Size(300, 150);
                    break;
                case MessageIcon.Question:
                    size = new Size(300, 150);
                    break;
                case MessageIcon.Error:
                    size = new Size(480, 210);
                    break;
                default:
                    break;
            }
            #endregion

            #region MessageContent
            var grid = new Grid();
            grid.Margin = new Thickness(4);
            grid.Width = size.Width - 40;
            grid.Height = size.Height - 60;

            grid.Background = new SolidColorBrush(Colors.Transparent);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            string IocPath = string.Format("/SMT.SaaS.Window;Component/Resources/{0}.png", icon.ToString());
            if (icon != MessageIcon.None)
            {
                Rectangle rect = new Rectangle();
                ImageBrush imgbr = new ImageBrush();
                imgbr.ImageSource = new BitmapImage(new Uri(IocPath, UriKind.RelativeOrAbsolute));
                imgbr.Stretch = Stretch.Fill;
                rect.Fill = imgbr;
                rect.Height = 48; rect.Width = 48;
                rect.HorizontalAlignment = HorizontalAlignment.Center;
                rect.VerticalAlignment = VerticalAlignment.Top;

                Grid.SetColumn(rect, 0);
                Grid.SetRow(rect, 0);
                // grid.Children.Add(rect);
            }
            #endregion

            # region Buttons

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 8, 0, 8),
                Background = new SolidColorBrush(Colors.Transparent)
            };

            var buttonArray = buttons as object[];

            if (buttonArray.Length == 0)
            {
                buttonArray = new[] { "Ok" };
            }
          
            foreach (var button in buttonArray)
            {
                var _button = new Button()
                {
                    Content = button,
                    Margin = new Thickness(5, 0, 0, 0),
                    Width = 70,
                    Height = 25,
                    Style = Application.Current.Resources["CommonButtonStyle"] as Style
                };
              

                _button.Click += (sender, e) =>
                {

                    window.Closed += (obj, args) =>
                    {
                        if (result.IsNotNull())
                        {
                            string rs = _button.Content.ToString();
                            var length = result.Method.GetParameters().Length;

                            if (length == 0)
                            {
                                result.DynamicInvoke();
                            }
                            else if (result.IsNull())
                            {
                                throw new InvalidOperationException("The \"Result\" was not setted.");
                            }
                            else
                            {
                                result.DynamicInvoke(rs);
                            }
                        }

                    };
                    window.Close();

                };

                stackPanel.Children.Add(_button);
                _button.Focus();
            }

            Grid.SetRow(stackPanel, 1);
            Grid.SetColumnSpan(stackPanel, 2);

            grid.Children.Add(stackPanel);

            # endregion

            # region ContentElement

            //显示内容
            var contentElement = content as FrameworkElement;

            if (contentElement.IsNull())
            {
                if (icon == MessageIcon.Error)
                {
                    var info = new TextBox
                    {
                        Margin = new Thickness(0),
                        Style = Application.Current.Resources["TextBoxNoBorderStyle"] as Style,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 12,
                        BorderThickness = new Thickness(0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        IsReadOnly=true,
                        Text = content.ToString()
                    };
                    contentElement = new ScrollViewer()
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Style = Application.Current.Resources["ScrollViewerStyle"] as Style,
                        Content = info,
                        Margin = new Thickness(4, 4, 0, 0),

                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Colors.Gray)
                    };
                }
                else
                {
                    var info = new TextBox
                    {
                        Margin = new Thickness(10, 10, 0, 0),
                        Style = Application.Current.Resources["TextBoxNoBorderStyle"] as Style,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 12,
                        BorderThickness = new Thickness(0),
                        IsReadOnly=true,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Text = content.ToString()
                    };
                    contentElement = new ScrollViewer()
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Style = Application.Current.Resources["ScrollViewerStyle"] as Style,
                        Content = info,
                        Margin = new Thickness(4, 4, 0, 0),

                        BorderThickness = new Thickness(0),
                        BorderBrush = new SolidColorBrush(Colors.Gray)
                    };
                }
            }

            Grid.SetColumn(contentElement, 1);
            Grid.SetColumnSpan(contentElement, 2);
            Grid.SetRow(contentElement, 0);
            grid.Children.Add(contentElement);
            # endregion
            #region Show
            if (dialogMode == DialogMode.ApplicationModal)
            {
                #region ApplicationModal
                if (!Popup.IsOpen)
                {
                    //throw new InvalidOperationException(
                    //    "已经有一个应用模式窗口打开。");

                    Point point = new Point(WindowsManager.Desktop.ActualWidth / 2 - size.Width / 2, WindowsManager.Desktop.ActualHeight / 2 - size.Height / 2);

                    window = WindowsManager.GetMessageWindow(new ContentControl() { Content = grid }, caption, point, size, IocPath);
                    window.HorizontalAlignment = HorizontalAlignment.Center; window.VerticalAlignment = VerticalAlignment.Center;
                    Canvas canvas = new Canvas() { Width = WindowsManager.Desktop.ActualWidth, Height = WindowsManager.Desktop.ActualHeight };
                    Rectangle Overlay = new Rectangle() { Width = WindowsManager.Desktop.ActualWidth, Height = WindowsManager.Desktop.ActualHeight };
                    Overlay.Fill = new SolidColorBrush(Colors.Gray);
                    Overlay.Opacity = .3;
                    canvas.Children.Add(Overlay);
                    canvas.Children.Add(window);
                    Popup.IsOpen = true;
                    Popup.Child = canvas;

                    window.Closed += (obj, arg) =>
                    {
                        if (_DialogMode == DialogMode.ApplicationModal)
                        {
                            Popup.IsOpen = false;
                            AppUseful.GetRootVisual<Control>().IsEnabled = true;
                        }

                    };
                    AppUseful.GetRootVisual<Control>().IsEnabled = false;
                }
                else
                {
                    return;
                }
                #endregion

            }
            else
            {

                Point point = new Point(WindowsManager.Desktop.ActualWidth / 2 - size.Width / 2, WindowsManager.Desktop.ActualHeight / 2 - size.Height / 2);
                window = WindowsManager.ShowModal(new ContentControl() { Content = grid }, caption, point, size, IocPath);
            }
            #endregion

            //Storyboard _fStroyboard1 = CreatOpacty(window);
            //if (icon == MessageIcon.Exclamation || icon==MessageIcon.Information)
            //{
            //    _fStroyboard1.Begin();
            //}

            //#region 动画完成关闭

            //_fStroyboard1.Completed += (o, e) =>
            //{
            //    Popup.IsOpen = false;
            //    window.Close();
            //};
            //window.MouseEnter += (obj, args) =>
            //{
            //    _fStroyboard1.Pause();
            //};
            //window.MouseLeave += (obj, args) =>
            //{
            //    _fStroyboard1.Begin();
            //};
            
            //#endregion
        }

        #region 创建窗口的动画
        public static Storyboard CreatOpacty(object obj)
        {
            Storyboard _fStroyboard = new Storyboard();
            Model.DoubleKeyFramesModel opactytykeyframes =
                new SMT.SAAS.AnimationEngine.Model.DoubleKeyFramesModel()
                {
                    Target = (DependencyObject)obj,
                    BeginTime = 0,
                    PropertyPath = ConstPropertyPath.UIELEMENT_OPACITY
                };

            opactytykeyframes.KeyFrames = new SMT.SAAS.AnimationEngine.Model.KeyFrames<Double>[] 
                { 
                    new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0,Value=1},
                    new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0.4,Value=1},
                    new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0.6,Value=1},
                    new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 1.0,Value=0}
                };
            List<IModel> _models = new List<IModel>()
                {
                    opactytykeyframes
                };
            _fStroyboard = Engine.CreateStoryboard(_models);
            return _fStroyboard;
        }

        public static Storyboard CreatTrans(object obj)
        {
            Storyboard _TStroyboard = new Storyboard();
            Model.DoubleKeyFramesModel transXkeyframes =

          new SMT.SAAS.AnimationEngine.Model.DoubleKeyFramesModel()
          {
              Target = (DependencyObject)obj,
              BeginTime = 0,
              PropertyPath = ConstPropertyPath.UIELEMENT_TRANSLATETRS_Y
          };

            transXkeyframes.KeyFrames = new SMT.SAAS.AnimationEngine.Model.KeyFrames<Double>[] 
            { 
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0,Value=100},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0.4,Value=0},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 0.6,Value=0},
                new Model.KeyFrames<Double>(){Type=KeyFramesType.Linear, KeyTime = 1.0,Value=-100}
            };
            List<IModel> _models = new List<IModel>()
                {
                    transXkeyframes
                };
            _TStroyboard = Engine.CreateStoryboard(_models);
            return _TStroyboard;
        }
        #endregion
    }



    /// <summary>
    /// 没有；信息；感叹；问题；错误；
    /// </summary>
    public enum MessageIcon
    {
        /// <summary>
        /// 无图标
        /// </summary>
        None,
        /// <summary>
        /// 信息
        /// </summary>
        Information,
        /// <summary>
        /// 感叹号
        /// </summary>
        Exclamation,
        /// <summary>
        /// 问题
        /// </summary>
        Question,
        /// <summary>
        /// 异常
        /// </summary>
        Error
    }
    /// <summary>
    /// 窗口类型
    /// </summary>
    public enum MessageWindowType
    {
        /// <summary>
        /// 普通窗口
        /// </summary>
        Default,
        /// <summary>
        /// 浮动窗口
        /// </summary>
        Flow
    }
}
