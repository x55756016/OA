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
using SMT.SAAS.Platform.WebParts.NewsWS;
using SMT.SAAS.Platform.WebParts.Models;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class NewsShow : UserControl
    {

        PlatformServicesClient client = null;
        BasicServices services = null;
        SMT.Saas.Tools.PublicInterfaceWS.PublicServiceClient publicWS;
        private static Dictionary<string, T_PF_NEWS> CacheList = new Dictionary<string, T_PF_NEWS>();
        private int _cacheCount = 10;
        private static T_PF_NEWS CacheNews;
        public NewsShow()
        {
            InitializeComponent();

            services = new BasicServices();
            publicWS = new Saas.Tools.PublicInterfaceWS.PublicServiceClient();
            client = services.PlatformClient;
            client.GetNewsModelByIDCompleted += new EventHandler<GetNewsModelByIDCompletedEventArgs>(client_GetNewsModelByIDCompleted);
            publicWS.GetContentCompleted += new EventHandler<Saas.Tools.PublicInterfaceWS.GetContentCompletedEventArgs>(publicWS_GetContentCompleted);
        }

        void publicWS_GetContentCompleted(object sender, Saas.Tools.PublicInterfaceWS.GetContentCompletedEventArgs e)
        {
            loading.Stop();
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    CacheNews.NEWSCONTENT = e.Result;
                    this.ViewModel = CacheNews;
                    if (CacheList.Count >= _cacheCount)
                        CacheList.Clear();

                    CacheList.Add(CacheNews.NEWSID, CacheNews);
                }
            }
        }

        void client_GetNewsModelByIDCompleted(object sender, GetNewsModelByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    publicWS.GetContentAsync(e.Result.NEWSID);

                    CacheNews = e.Result;
                    // CacheList.Add(e.Result.NEWSID, e.Result);
                }
            }
        }

        public void LoadNewsDetails(string newsID)
        {
            loading.Start();
            if (CacheList.ContainsKey(newsID))
            {
                this.ViewModel = CacheList[newsID];
                loading.Stop();
            }
            else
            {
                client.GetNewsModelByIDAsync(newsID);
            }
        }

        public T_PF_NEWS ViewModel
        {
            set
            {
                if (value != null)
                {
                    this.DataContext = value;
                    System.IO.Stream stream = null;
                    if (value != null)
                    {
                        rtbContent.Document = value.NEWSCONTENT;
                    }
                }
            }
        }
    }
}
