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
using System.Windows.Navigation;
using System.Windows.Interop;
using SMT.SaaS.FrameworkUI;
using SMT.FB.UI.Common;
using SMT.FB.UI.FBCommonWS;

using SMT.SaaS.FrameworkUI.Helper;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.OrganizationControl;

using CurrentContext = SMT.SAAS.Main.CurrentContext;

namespace SMT.FB.UI
{
    public partial class MainPage : UserControl
    {
        public static int istyle;
        public event RoutedEventHandler MoveOver;
        private HyperlinkButton _lasttext;

        private bool _isNavigating = false;

        private readonly Image _imgLast = new Image();
        private readonly Image _imgNext = new Image();
        private Storyboard _sb = new Storyboard();

        public int Istyle
        {
            get { return istyle; }
            set { istyle = value; }
        }

        public MainPage()
        {
            UIHelper._CurrentStyleCode_2 = AppConfig._CurrentStyleCode;
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            HeaderMenu.HBHome.Click += new RoutedEventHandler(HBHome_Click);
            HeaderMenu.HBFullScreen.Click += new RoutedEventHandler(HBFullScreen_Click);


            HeaderMenu.SetUserNameAndDepartmentName(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName,
             SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName,
             SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName,
              SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName);
            HideWaitingControl();
        }

        void SbCompleted(object sender, EventArgs e)
        {
            //Panel panel = ContentFrame.Parent as Panel;
            //panel.Children.Remove(_imgLast);
            //panel.Children.Remove(_imgNext);
            //ContentFrame.Visibility = Visibility.Visible;
            //_sb.Stop();
            //_isNavigating = false;
        }

       // private DispatcherTimer timer;
        private void ContentFrameNavigated(object sender, NavigationEventArgs e)
        {
            //ContentFrame.Navigated -= ContentFrameNavigated;
            //Panel panel = ContentFrame.Parent as Panel;
            //panel.UpdateLayout();

            //timer = new DispatcherTimer();

            //timer.Tick += (arg, obj) =>
            //{

            //    WriteableBitmap bitmapN = new WriteableBitmap(ContentFrame, new TranslateTransform());
            //    _imgNext.Source = bitmapN;
            //    TranslateTransform ttNext = new TranslateTransform();

            //    _imgNext.RenderTransform = ttNext;

            //    panel.Children.Add(_imgNext);

            //    StoryboardStart.Begin();
            //    _sb.Children.Add(CreateDoubleAnimation(ttNext, "X", -bitmapN.PixelWidth, 0, true));
            //    ContentFrame.Visibility = Visibility.Collapsed;
            //    _sb.Begin();
            //    timer.Stop();
            //    timer = null;
            //};

            //timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            //timer.Start();
            //WriteableBitmap bitmapN = new WriteableBitmap(ContentFrame, new TranslateTransform());
            //_imgNext.Source = bitmapN;
            //StoryboardStart.Begin();
            //TranslateTransform ttNext = new TranslateTransform();

            //_imgNext.RenderTransform = ttNext;
            //panel.Children.Add(_imgNext);

            //_sb.Children.Add(CreateDoubleAnimation(ttNext, "X", -bitmapN.PixelWidth, 0, true));
            //ContentFrame.Visibility = Visibility.Collapsed;
            //_sb.Begin();
        }



        private static DoubleAnimation CreateDoubleAnimation(DependencyObject element, string property, double from,
                                                            double to, bool addEasing)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.To = to;
            da.From = from;
            da.Duration = new Duration(TimeSpan.FromSeconds(1.68));

            if (addEasing)
                da.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 3 };

            Storyboard.SetTargetProperty(da, new PropertyPath(property));
            Storyboard.SetTarget(da, element);

            TransformGroup transgroup = new TransformGroup();
            ScaleTransform scaletrans = new ScaleTransform();
            transgroup.Children.Add(scaletrans);
            scaletrans.CenterX = 0.5;
            scaletrans.CenterY = 0.5;

            return da;
        }

        private void H1_Click(object sender, RoutedEventArgs e)
        {
            //_lasttext = (HyperlinkButton)sender;
            //string source = Convert.ToString(ContentFrame.Source);
            //if (source != _lasttext.Tag.ToString() && !_isNavigating)
            //{
            //    _sb = new Storyboard();
            //    _sb.Completed += SbCompleted;

            //    Panel panel = ContentFrame.Parent as Panel;
            //    if (panel != null) panel.Children.Add(_imgLast);
            //    ContentFrame.Navigate(new Uri(_lasttext.Tag.ToString(), UriKind.Relative));

            //    WriteableBitmap bitmapL = new WriteableBitmap(ContentFrame, new TranslateTransform());
            //    _imgLast.Source = bitmapL;
            //    TranslateTransform ttLast = new TranslateTransform();

            //    _imgLast.RenderTransform = ttLast;

            //    ContentFrame.Navigated += ContentFrameNavigated;
            //    _sb.Children.Add(CreateDoubleAnimation(ttLast, "X", 0, bitmapL.PixelWidth, true));

            //    _isNavigating = true;
            //}
        }

        #region 导航功能

        void HBFullScreen_Click(object sender, RoutedEventArgs e)
        {
            Content contentObject = Application.Current.Host.Content;
            contentObject.IsFullScreen = !contentObject.IsFullScreen;
        }

        void HBHome_Click(object sender, RoutedEventArgs e)
        {

            //PopMsgBox box = new PopMsgBox();
            //box.GetContent("最新销售", "", "楼上的解放乐山大佛蓝色的立法解释领导法律手段杰弗里斯立法解释了解发神经飞");
            //box.ParentLayoutRoot = SMT.SaaS.FrameworkUI.Common.Common.ParentLayoutRoot;
            //box.HorizontalAlignment = HorizontalAlignment.Right;
            //box.VerticalAlignment = VerticalAlignment.Bottom;
            //box.Show();

            //App currentApp = (App)Application.Current;
            //currentApp.rootGrid.Children.Clear();

            //currentApp.rootGrid.Children.Add(new MainPage());

        }

        void HBNext_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoForward)
            {
                ContentFrame.GoForward();
            }
        }

        void HBPervious_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Window.Parent = canvasName;

            System.Windows.Controls.Window.Wrapper = this;

            HeaderMenu.HBPervious.Click += new RoutedEventHandler(HBPervious_Click);

            HeaderMenu.HBNext.Click += new RoutedEventHandler(HBNext_Click);
            HeaderMenu.HBLoginOut.Click += new RoutedEventHandler(HBLoginOut_Click);
        }

        void HBLoginOut_Click(object sender, RoutedEventArgs e)
        {
            App currentApp = (App)Application.Current;
            currentApp.rootGrid.Children.Clear();
            currentApp.rootGrid.Children.Add(new SMT.SaaS.LM.UI.AppMain.Login.Login());
        }

        /// <summary>
        ///     After the Frame navigates, ensure the <see cref="HyperlinkButton"/> representing the current page is selected
        /// </summary>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // PageNavgation = e.Uri.ToString();
            if (ContentFrame.CanGoBack == false)
            {
                HeaderMenu.HBPervious.Opacity = 0.3;
            }
            else
            {
                HeaderMenu.HBPervious.Opacity = 1.0;
            }
            if (ContentFrame.CanGoForward == false)
            {
                HeaderMenu.HBNext.Opacity = 0.3;
            }
            else
            {
                HeaderMenu.HBNext.Opacity = 1.0;
            }
            // HideWaitingControl();
        }

        /// <summary>
        ///     If an error occurs during navigation, show an error window
        /// </summary>
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            //e.Handled = true;
            //ChildWindow errorWin = new ErrorWindow(e.Uri);
            //errorWin.Show();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // ComfirmBox Cbox = new ComfirmBox();
            // Cbox.MessageTextBox = "  这是个新建计划,请大家仔细阅览，积极参与 合作愉快；这是个新建计划,请大家仔细阅览，积极参与 合作愉快；这是个新建计划,请大家仔细阅览，积极参与 合作愉快。";
            //  Cbox.ShowImg(2);
            // Cbox.Show();
            //  SetStyle();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //ComfirmBox Cbox = new ComfirmBox();
            //Cbox.MessageTextBox = "  请确定以下信息~";
            //Cbox.ShowImg(1);
            //Cbox.Show();
            //SetStyle();
        }

        #region "进度条控制"
        private void ProgressBar_Cancel(object sender, EventArgs e)
        {
            if (CancelWaiting != null)
            {
                CancelWaiting(this, EventArgs.Empty);
            }
            HideWaitingControl();
        }

        //----------------------------------------------------------------------------
        //<summary>
        //Occurs when waiting is canceled
        //</summary>
        public event EventHandler CancelWaiting;

        /// <summary>
        /// Calculates wait spinner size and location.
        /// </summary>
        private void CalculatesSpinner()
        {
            waitSpinnerBack.Width = LayoutRoot.ActualWidth;
            waitSpinnerBack.Height = LayoutRoot.ActualHeight;
            waitSpinner.SetValue(Canvas.TopProperty, waitSpinnerBack.Height / 2 - waitSpinner.ActualHeight / 2 - 100);
            waitSpinner.SetValue(Canvas.LeftProperty, waitSpinnerBack.Width / 2 - waitSpinner.ActualWidth / 2 - 100);
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Hides waiting controls.
        /// </summary>
        public void HideWaitingControl()
        {
            waitSpinner.Stop();
            waitSpinnerBack.Visibility = Visibility.Collapsed;
            //waitSpinner.Visibility = Visibility.Collapsed;
            spinnerBackShowBorder.Stop();
            spinnerShowBorder.Stop();
            beforSpinnerShowShowBorder.Stop();
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Shows waiting controls.
        /// </summary>
        public void ShowWaitingControl()
        {
            CalculatesSpinner();
            waitSpinnerBack.Visibility = Visibility.Visible;
            waitSpinner.Visibility = Visibility.Visible;
            waitSpinner.Start();
            // beforSpinnerShowShowBorder.Begin();  
        }
        #endregion

        private void Menu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            toolkitacc.Height = ((Border)sender).ActualHeight;// this.Height;
        }

        private void waitSpinner_Cancel(object sender, EventArgs e)
        {
            waitSpinnerBack.Visibility = Visibility.Collapsed;
        }

        private void ContentFrame_FragmentNavigation(object sender, FragmentNavigationEventArgs e)
        {

        }
        #endregion

    }
}
