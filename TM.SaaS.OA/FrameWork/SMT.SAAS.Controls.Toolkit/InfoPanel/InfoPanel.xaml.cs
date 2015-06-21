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
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace SMT.SAAS.Controls.Toolkit.InfoPanel
{
    public partial class InfoPanel : UserControl
    {
        private int _currentIndex = 0;
        private List<Info> _listinfo = new List<Info>();
        private int _waitTime = 5000;
        private static int FPS = 24;                // 事件关键帧数
        private bool _Off = true;
        private bool _ControlOff = true;
        private int _indexSize = 48;
        public event EventHandler<OnInfoClickEventArgs> OnInfoClick;
        public InfoPanel()
        {
            InitializeComponent();

        }
        private DispatcherTimer _timer = new DispatcherTimer(); //控制器
        public List<Info> InfoList { get { return _listinfo; } set { _listinfo = value; } }

        /// <summary>
        /// 处理器.元素复位
        /// </summary>
        void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            DisplayNext();
        }

        private void DisplayNext()
        {
            MoveIndex();

            if (_Off)
                _timer.Start();
            //Show.Begin();
            //Show.Completed += (objects, args) =>
            //{
            //    MoveIndex();
            //    Hide.Begin();
            //    Hide.Completed += (objects2, args2) =>
            //    {
            //        if (_Off)
            //            _timer.Start();
            //        Content_copy.DataContext = Content.DataContext;
            //    };
            //};
        }
        /// <summary>
        /// 处理器启动
        /// </summary>
        public void Start()
        {
            if (InfoList.Count > 0)
                Content.DataContext = InfoList[_currentIndex];
            //Content_copy.DataContext = InfoList[_currentIndex];
            AddIndex();
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, _waitTime);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
        }

        public void MoveIndex()
        {
            _currentIndex++;
            if (_currentIndex == InfoList.Count)
                _currentIndex = 0;

            ItemController.Index = _currentIndex;
            Content.DataContext = InfoList[_currentIndex];

        }
        private void AddIndex()
        {
            for (int i = 0; i < InfoList.Count; i++)
            {
                initIndexImage(InfoList[i], i);
            }

            this.ItemController.TotalItems = this.InfoList.Count;
            this.ItemController.Index = this._currentIndex;
            this.ItemController.OnSelectedIndexChanged += new EventHandler<SelectedIndexChangedArgs>(this.ItemController_OnSelectedIndexChanged);
        }
        private void ItemController_OnSelectedIndexChanged(object sender, SelectedIndexChangedArgs e)
        {
            this._timer.Stop();
            int Index = e.NewIndex;
            _currentIndex = Index;


            Content.DataContext = InfoList[Index];
            // Content_copy.DataContext = Content.DataContext;
            ////if (this._delayTimer.IsEnabled)
            ////{
            ////    this._delayTimer.Stop();
            ////}
            //this._timer.Stop();
            //if (e.NewIndex > e.PreviousIndex)
            //{
            //    //this.SetIndex(e.NewIndex - e.PreviousIndex);
            //    //this._previousDirection = 1;
            //}
            //else
            //{
            //   // this.SetIndex(e.NewIndex - e.PreviousIndex);
            //  //  this._previousDirection = -1;
            //}
            this._timer.Start();
        }

        private void initIndexImage(Info info, int index)
        {
            InfoIndexItem item = new InfoIndexItem();
            Image s = new Image();
            s.Margin = new Thickness(2.0, 1.0, 2.0, 1.0);
            s.Height = 45.0;
            s.Width = 57.0;
            s.Stretch = Stretch.Fill;
            s.Source = info.ImageSource;
            s.Tag = index;
            s.MouseLeftButtonDown += (objects, arts) =>
                {
                    Image source = objects as Image;
                    int Index = (int)source.Tag;
                    Content.DataContext = InfoList[Index];
                    _currentIndex = Index;
                };
            s.MouseEnter += new MouseEventHandler(Content_MouseEnter);
            s.MouseLeave += new MouseEventHandler(Content_MouseLeave);

        }

        private void Content_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_ControlOff)
            {
                _timer.Stop();
                _Off = false;
            }
        }
        private void Content_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_ControlOff)
            {
                _Off = true;
                _timer.Start();
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {


        }

        private void Content_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (OnInfoClick != null)
                OnInfoClick(this, new OnInfoClickEventArgs(Content.DataContext as Info));
        }

        //private void ControlOff_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (_ControlOff)
        //    {
        //        (sender as Image).Source = new BitmapImage(new Uri("/SMT.SAAS.Resources;component/images/stop.png", UriKind.Relative));
        //        _ControlOff = false;
        //        _timer.Stop();

        //    }
        //    else
        //    {
        //        (sender as Image).Source = new BitmapImage(new Uri("/SMT.SAAS.Resources;component/images/star.png", UriKind.Relative));
        //        _ControlOff = true;
        //        _timer.Start();
        //    }
        //}
    }

    public class OnInfoClickEventArgs : EventArgs
    {
        public Info Info { get; set; }
        public OnInfoClickEventArgs(Info info)
        {
            this.Info = info;
        }

    }
}
