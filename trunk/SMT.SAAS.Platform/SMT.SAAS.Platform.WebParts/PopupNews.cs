using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
 
using SMT.SAAS.Controls.Toolkit.Windows;
using SMT.SAAS.Platform.WebParts.ClientServices;
using SMT.SAAS.Platform.WebParts.Views;

namespace SMT.SAAS.Platform.WebParts.News
{
    public class PopupNews
    {
        NewsServices _newsServices;
        public PopupNews()
        {
            _newsServices = new NewsServices();
            _newsServices.OnGetNewsListCompleted += new EventHandler<GetEntityListEventArgs<SAAS.Platform.Model.NewsModel>>(_newsServices_OnGetNewsListCompleted);
        }

        void _newsServices_OnGetNewsListCompleted(object sender, GetEntityListEventArgs<SAAS.Platform.Model.NewsModel> e)
        {
            if (e.Error == null)
            {
                try
                {
                    var result = e.Result;
                    if (result.Count > 0)
                    {
                        foreach (var item in e.Result)
                        {
                            NewsShow newsview = new NewsShow();
                            newsview.LoadNewsDetails(item.NEWSID);
                            string titel = "";
                            switch (item.NEWSTYPEID)
                            {
                                case "0": titel = "新    闻"; break;
                                case "1": titel = "动    态"; break;
                                default:
                                    break;
                            }
                            var host = ProgramManager.ShowProgram(titel, string.Empty, item.NEWSID, newsview, true, true, null);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }


        public void PoputNews()
        {
            _newsServices.GetPopupNewsList();
        }
        
    }
}
