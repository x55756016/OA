//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 信息面板操作
// 完成日期：2011-05-16 
// 版    本：V1.0 
// 作    者：高雁 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SMT.SAAS.Platform.Controls.InfoPanel
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

            this._timer.Start();
        }

        private void initIndexImage(Info info, int index)
        {
            InfoIndexItem item = new InfoIndexItem();
            if (info.ImageSource != null)
            {
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
