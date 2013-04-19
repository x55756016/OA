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

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Browser;

namespace SMT.SaaS.OA.UI.Views.ArchivesManagement
{
    public partial class CFrmArchivesBrowser : ChildWindow
    {

        private SmtOADocumentAdminClient client;
        private string archiveID;
        //List<V_ArchivesLending> archiveslending;

       

        

        public CFrmArchivesBrowser(string archivesID)
        {
            InitializeComponent();
            archiveID = archivesID;
            InitEvent();

        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.GetArchivesByIdCompleted += new EventHandler<GetArchivesByIdCompletedEventArgs>(client_GetArchivesByIdCompleted);
            client.GetArchivesByIdAsync(archiveID);
            //client.GetLendingListByLendingIdCompleted += new EventHandler<GetLendingListByLendingIdCompletedEventArgs>(client_GetLendingListByLendingIdCompleted);
            //client.GetArchivesByIdCompleted += new EventHandler<GetArchivesByIdCompletedEventArgs>(client_GetArchivesByIdCompleted);
            //client.GetLendingListByLendingIdAsync(archiveID);
        }

        private void client_GetArchivesByIdCompleted(object sender, GetArchivesByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    this.txtTitle.Text = e.Result.ARCHIVESTITLE;
                    //this.txtContent.Text = e.Result.CONTENT;
                }
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Alert(ex.ToString());
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

