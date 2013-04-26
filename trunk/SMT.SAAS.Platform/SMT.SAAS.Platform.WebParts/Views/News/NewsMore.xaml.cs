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
using SMT.SAAS.Platform.WebParts.ViewModels;
using SMT.SAAS.Controls.Toolkit.Windows;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class NewsMore : UserControl
    {
        public NewsMore()
        {
            InitializeComponent();
            this.DataContext = new NewsMoreViewModel();

        }

        public NewsMore(string newsid)
            : this()
        {
            newsShow.LoadNewsDetails(newsid);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            NewsViewModel source = (sender as HyperlinkButton).DataContext as NewsViewModel;
            newsShow.LoadNewsDetails(source.NEWSID);
            //    NewsShow newsview = new NewsShow();
            //    newsview.LoadNewsDetails(source.NEWSID);
            //    string titel = "";
            //    switch (source.NEWSTYPE.ID)
            //    {
            //        case 0: titel = "新    闻"; break;
            //        case 1: titel = "动    态"; break;
            //        default:
            //            break;
            //    }
            //    var host = ProgramManager.ShowProgram(titel, string.Empty, source.NEWSID, newsview, true, true, null);

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
        }
    }
}
