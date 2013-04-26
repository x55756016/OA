//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 滚动新闻
// 完成日期：2011-05-16 
// 版    本：V1.0 
// 作    者：王玲 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System.Windows.Controls;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Threading;
using System;
using SMT.SAAS.AnimationEngine;
using Model = SMT.SAAS.AnimationEngine.Model;
using System.Windows.Media.Animation;
using SMT.SAAS.AnimationEngine.Model;

using SMT.SAAS.Platform.WebParts.Models;
using SMT.SAAS.Platform.WebParts.NewsWS;
using SMT.SAAS.Platform.WebParts.Views;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class RollNews : UserControl
    {
        private DispatcherTimer _timer;
        private List<NewsModel> _listnews;
        private int currentIndex = 0;

        private Storyboard _SHOW;

        PlatformServicesClient client = null;
        //基础服务通讯
        BasicServices services = null;

        public RollNews()
        {
            InitializeComponent();
            _SHOW = ShowStoryBoard(tblRollnews);

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 8);
            _timer.Tick += new System.EventHandler(_timer_Tick);
            this.Loaded += new RoutedEventHandler(RollNews_Loaded);

            ContentPanel.MouseEnter += new System.Windows.Input.MouseEventHandler(ContentPanel_MouseEnter);
            ContentPanel.MouseLeave += new System.Windows.Input.MouseEventHandler(ContentPanel_MouseLeave);
            tblRollnews.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(tblRollnews_MouseLeftButtonUp);
            if (_SHOW != null)
            {
                _SHOW.Completed += new EventHandler(_SHOW_Completed);
            }

            if (services == null)
                services = new BasicServices();

            client = services.PlatformClient;
            if (client != null)
            {
                client.GetNewsListByParamsCompleted += new EventHandler<GetNewsListByParamsCompletedEventArgs>(client_GetNewsListByParamsCompleted);
                client.GetNewsListByParamsAsync("0|1", 10, "1");
            }
        }

        void tblRollnews_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            T_PF_NEWS news = ((sender as TextBlock).DataContext as NewsModel).DataContent as T_PF_NEWS;

            NewsShow newsview = new NewsShow();
            newsview.LoadNewsDetails(news.NEWSID);
            string titel = "";
            switch (news.NEWSTYPEID)
            {
                case "0": titel = "新    闻"; break;
                case "1": titel = "动    态"; break;
                case "2": titel = "公    告"; break;
                case "3": titel = "通    知"; break;
                default:
                    break;
            }
            System.Windows.Controls.Window.Show(titel, "", news.NEWSID, true, true, newsview, null);
        }


        #region 加载新闻服务端数据

        void client_GetNewsListByParamsCompleted(object sender, GetNewsListByParamsCompletedEventArgs e)
        {
            _listnews = new List<NewsModel>();
            if (e.Result != null)
            {
                foreach (var item in e.Result)
                {
                    _listnews.Add(new NewsModel()
                    {
                        Titel = item.NEWSTITEL,
                        DataContent = item
                    });
                }
                InitNewInfo();
            }
        }
        #endregion
 
        void RollNews_Loaded(object sender, RoutedEventArgs e)
        {
            // InitNewInfo();
            //_timer.Start();
            currentIndex = 0;
        }

        #region 动画暂停处理理
        void ContentPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _SHOW.Resume();
        }

        void ContentPanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _SHOW.Pause();
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _SHOW.Pause();
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _SHOW.Resume();
        }
        #endregion

        #region 下一条数据读取
        public void MoveNext()
        {
            if (currentIndex < _listnews.Count - 1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }
            NewsModel currentInfo = _listnews[currentIndex];
            tblRollnews.DataContext = currentInfo;
            tblRollnews.Visibility = Visibility.Visible;
            _SHOW.Begin();
        }
        #endregion

        #region 动画帧处理
        void _SHOW_Completed(object sender, EventArgs e)
        {
            if (currentIndex != 0 && currentIndex == _listnews.Count)
            {
                currentIndex = 0;
                InitNewInfo();
            }
            _timer.Start();
        }

        void _timer_Tick(object sender, System.EventArgs e)
        {
            if (_listnews.Count == 0)
            {
                _timer.Stop();
                InitNewInfo();
            }
            else
            {
                _timer.Stop();
                MoveNext();
            }
        }
        #endregion

        #region 依次显示数据
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button btn = sender as Button;
            _timer.Stop();
            switch (btn.Tag.ToString())
            {
                case "0":
                    if (currentIndex >= 1)
                    {
                        currentIndex--;
                    }
                    else
                    {
                        currentIndex = 0;
                    }
                    break;
                case "1":
                    if (currentIndex < (_listnews.Count - 1))
                    {
                        currentIndex++;
                    }
                    else
                    {
                        currentIndex = (_listnews.Count - 1);
                    }
                    break;
                default:
                    currentIndex = 0;
                    break;
            }
            InitNewInfo();
        }
        #endregion

        #region 数据替换动画
        public Storyboard ShowStoryBoard(DependencyObject target)
        {
            List<IModel> _models = new List<IModel>() 
            { 
                new DoubleModel() { Target = target, From = 0, To = 1, PropertyPath = ConstPropertyPath.UIELEMENT_OPACITY, Duration = 0.75 },
                new DoubleModel() { Target = target, From = (22), To = 2, PropertyPath = ConstPropertyPath.CANVAS_TOP, Duration = 0.75 },
            };
            return Engine.CreateStoryboard(_models);

        }
        #endregion

        #region 重置数据包
        private void InitNewInfo()
        {
            if (_listnews != null && _listnews.Count > 0)
            {
                NewsModel currentInfo = _listnews[currentIndex];
                tblRollnews.DataContext = currentInfo;
                _timer.Start();
            }
        }
        #endregion

    }

    public class NewsModel
    {
        public string Titel { get; set; }
        public string URL { get; set; }
        public object DataContent { get; set; }
    }
}
