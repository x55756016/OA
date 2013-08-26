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
using SMT.SAAS.Platform.WebParts.NewsWS;
using SMT.SAAS.Platform.WebParts.NewsCallBackWS;
using SMT.SAAS.Platform.WebParts.Models;
using SMT.SAAS.Controls.Toolkit.Windows;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class News : UserControl, IWebpart
    {
        public News()
        {
            InitializeComponent();
            Initialize();

        }
        /// <summary>
        /// 新闻类别
        /// </summary>
        public string NewsType { get; set; }

        //定时器
        private DispatcherTimer _refdateTimer;
        //非双工客户端
        PlatformServicesClient client = null;
        //双工客户端
        NewsCallBackClient callbackClient = null;
        //基础服务通讯
        BasicServices services = null;
        //测试变量
        private static int RefCount = 0;
        //8分钟进行一次客户端自检，并进行客户端修复
        private int minutes = 60;
        //获取前多少条
        private int topCount = 17;

        private void RegiestServices()
        {
            if (services == null)
                services = new BasicServices();

            client = services.PlatformClient;
            callbackClient = services.CallBackClient;
            if (callbackClient != null)
            {
                callbackClient.ReceiveReceived += new EventHandler<ReceiveReceivedEventArgs>(CallBackClient_ReceiveReceived);
                callbackClient.LoginCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(CallBackClient_LoginCompleted);
                callbackClient.LoginAsync();
            }
        }

        private void InitDate()
        {
           // client.GetNewsListByParamsCompleted += (obj, args) =>
           //{
           //    loading.Stop();
           //    if (args.Error == null)
           //    {
           //        if (args.Result != null)
           //        {
           //            if (args.Result.Count > 0)
           //            {
           //                if (args.Result.Count >= topCount)
           //                    btnMore.Visibility = Visibility.Visible;

           //                NewsList.ItemsSource = null;
           //                NewsList.ItemsSource = args.Result.ToList();
           //            }
           //        }
           //    }
           //};
           // loading.Start();
           // client.GetNewsListByParamsAsync(NewsType, topCount, "1");

            client.GetNewsListByEmployeeIDCompleted += (obj, args) =>
            {
                loading.Stop();
                if (args.Error == null)
                {
                    if (args.Result != null)
                    {
                        if (args.Result.Count > 0)
                        {
                            if (args.Result.Count >= topCount)
                                btnMore.Visibility = Visibility.Visible;

                            NewsList.ItemsSource = null;
                            NewsList.ItemsSource = args.Result.ToList();
                           
                        }
                    }
                }
            };
            loading.Start();
            client.GetNewsListByEmployeeIDAsync(NewsType, topCount, "1",SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void CheckServices()
        {
            if (callbackClient != null)
            {
                RefCount += 1;
                switch (callbackClient.State)
                {
                    case System.ServiceModel.CommunicationState.Closed:
                        RegiestServices();
                        break;
                    case System.ServiceModel.CommunicationState.Faulted:
                        callbackClient.Abort();
                        RegiestServices();
                        break;
                    case System.ServiceModel.CommunicationState.Opened:
                        callbackClient.LoginAsync();
                        break;
                }
            }

            if (_refdateTimer != null)
                _refdateTimer.Start();
        }

        #region 事件处理 Event Hanlder

        private void _refdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_refdateTimer != null)
                    _refdateTimer.Stop();

                CheckServices();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void CallBackClient_LoginCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //注册服务若产生错误则重新注册
            //if (e.Error != null)
            //    RegiestServices();
        }

        private void CallBackClient_ReceiveReceived(object sender, ReceiveReceivedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (!(e.news.NEWSSTATE == "ACTIVE"))
                    {
                        List<SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS> list = (NewsList.ItemsSource as List<SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS>);
                        var tnews = list.Where(news => news.NEWSID == e.news.NEWSID).FirstOrDefault();
                        if (tnews != null)
                            list.Remove(tnews);

                        list.Add(new SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS()
                        {
                            COMMENTCOUNT = e.news.COMMENTCOUNT,
                            CREATECOMPANYID = e.news.CREATECOMPANYID,
                            CREATEDATE = e.news.CREATEDATE,
                            CREATEDEPARTMENTID = e.news.CREATEDEPARTMENTID,
                            CREATEPOSTID = e.news.CREATEPOSTID,
                            CREATEUSERID = e.news.CREATEPOSTID,
                            CREATEUSERNAME = e.news.CREATEUSERNAME,
                            NEWSCONTENT = e.news.NEWSCONTENT,
                            NEWSID = e.news.NEWSID,
                            NEWSSTATE = e.news.NEWSSTATE,
                            NEWSTITEL = e.news.NEWSTITEL,
                            NEWSTYPEID = e.news.NEWSTYPEID,
                            OWNERCOMPANYID = e.news.OWNERCOMPANYID,
                            OWNERDEPARTMENTID = e.news.OWNERDEPARTMENTID,
                            OWNERID = e.news.OWNERID,
                            OWNERNAME = e.news.OWNERNAME,
                            OWNERPOSTID = e.news.OWNERPOSTID,
                            READCOUNT = e.news.READCOUNT,
                            UPDATEDATE = e.news.UPDATEDATE,
                            UPDATEUSERID = e.news.UPDATEUSERID,
                            UPDATEUSERNAME = e.news.UPDATEUSERNAME
                        });

                        if (list.Count >= 20)
                            btnMore.Visibility = Visibility.Visible;

                        NewsList.ItemsSource = null;
                        NewsList.ItemsSource = list.OrderByDescending(news => news.UPDATEDATE).ToList(); ;
                    }
                }
            }
            catch (Exception ex)
            {
                //throw;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS source =
                 (sender as HyperlinkButton).DataContext as SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS;
            ShowNewsInfo(source.NEWSID);
            //try
            //{
            //    HyperlinkButton bybutton = sender as HyperlinkButton;
            //    bybutton.Foreground = new SolidColorBrush(Color.FromArgb(255, 63, 40, 92));

            //    SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS source =
            //       (sender as HyperlinkButton).DataContext as SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS;
            //    NewsShow newsview = new NewsShow();
            //    newsview.LoadNewsDetails(source.NEWSID);
            //    string titel = "";
            //    switch (source.NEWSTYPEID)
            //    {
            //        case "0": titel = "新    闻"; break;
            //        case "1": titel = "动    态"; break;
            //        case "2": titel = "公    告"; break;
            //        case "3": titel = "通    知"; break;
            //        default:
            //            break;
            //    }
            //    var host = ProgramManager.ShowProgram(titel, string.Empty, source.NEWSID, newsview, true, true, null);
            //}
            //catch (Exception ex)
            //{

            //}
        }

        public void ShowNewsInfo(string strNewsID)
        {
            NewsMore manager = new NewsMore(strNewsID);
            if (ViewModel.Context.MainPanel != null)
            {
                if (ViewModel.Context.MainPanel.DefaultContent != null)
                {
                    IWebpart webpart = ViewModel.Context.MainPanel.DefaultContent as IWebpart;
                    if (webpart != null)
                        webpart.Stop();

                }
                Stop();
                ViewModel.Context.MainPanel.Navigation(manager, "新    闻");


                //Cleanup();
            }
        }

        private void btnMore_Click(object sender, RoutedEventArgs e)
        {
            NewsMore newsview = new NewsMore();
            string titel = "新闻动态";
            var host = ProgramManager.ShowProgram(titel, string.Empty,
                "DD71AE94-F53B-4B5A-AD09-8F3402C9A280", newsview, true, true, null);
        }

        #endregion

        #region IWebpart
        public int Top
        {
            get;
            set;
        }

        public int RefDate
        {
            get;
            set;
        }

        public string Titel
        {
            get;
            set;
        }

        public void LoadDate()
        {
        }

        public void Initialize()
        {
            _refdateTimer = new DispatcherTimer();
            _refdateTimer.Interval = new TimeSpan(0, minutes, 0);
            _refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
            _refdateTimer.Start();
            NewsType = "0|1";
            RegiestServices();

            InitDate();
        }

        public void Cleanup()
        {
            _refdateTimer.Tick -= _refdateTimer_Tick;
            _refdateTimer.Stop();
            _refdateTimer = null;
            //非双工客户端
            client = null;
            //双工客户端
            callbackClient = null;
            //基础服务通讯
            services = null;
        }

        public void Stop()
        {
            if (_refdateTimer != null)
                _refdateTimer.Stop();
        }

        public void Star()
        {
            if (_refdateTimer != null)
                _refdateTimer.Start();
        }
        #endregion



    }
}
