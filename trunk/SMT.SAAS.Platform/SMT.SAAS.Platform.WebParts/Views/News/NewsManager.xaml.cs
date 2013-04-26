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
using SMT.SAAS.Platform.WebParts.NewsWS;
using SMT.SAAS.Platform.WebParts.Models;
using SMT.SAAS.Controls.Toolkit.Windows;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class NewsManager : UserControl
    {
        private string AddFormID = "CA85092A-0611-4BC6-A8D2-ABA68C9C64EE";
        private string UpdateFormID = "7B545E0F-10F5-4BAA-9F5D-40324E7E6CA9";
        private NewsManagerViewModel viewModel;
        private PlatformServicesClient client = null;
        public NewsManager()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WebPartManage_Loaded);
            client = new BasicServices().PlatformClient;
        }
        private void LoadDate()
        {
            viewModel = new NewsManagerViewModel();
            this.DataContext = viewModel;

        }
        private void WebPartManage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDate();
            this.Loaded -= new RoutedEventHandler(WebPartManage_Loaded);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NewsViewModel wm = new NewsViewModel() { NEWSID = Guid.NewGuid().ToString(), ParentVM = viewModel };
            NewsView addView = new NewsView(wm, ViewState.ADD);
            SMT.SAAS.Controls.Toolkit.Windows.Window host = ProgramManager.ShowProgram("新建新闻", "", AddFormID, addView, true, true, null);
            addView.Reset += (obj, arg) =>
            {
                host.Close();
            };
            host.Closed += (obj, arg) =>
            {
                // viewModel.Refresh();
            };
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgNewsList.SelectedItem == null)
                {
                    MessageWindow.Show("", "请先选择一条新闻,再进行编辑!", MessageIcon.Error, MessageWindowType.Flow);

                    return;
                }
                viewModel.CurrentEntity.ParentVM = viewModel;
                NewsView addView = new NewsView(viewModel.CurrentEntity, ViewState.UPDATE);
                SMT.SAAS.Controls.Toolkit.Windows.Window host = ProgramManager.ShowProgram("修改新闻", "", UpdateFormID, addView, true, true, null);
                addView.Reset += (obj, arg) =>
                {
                    host.Close();
                };
                host.Closed += (obj, arg) =>
                {
                    //  viewModel.Refresh();
                };
            }
            catch (Exception ex)
            {

                 
            }

        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgNewsList.SelectedItem == null)
            {
                MessageWindow.Show("", "请先选择一条新闻,再进行删除!", MessageIcon.Error, MessageWindowType.Flow);

                return;
            }
            viewModel.CurrentEntity.ParentVM = viewModel;
            viewModel.CurrentEntity.DeleteEntity.Execute(null);

            // viewModel.Refresh();
        }
    }
}
