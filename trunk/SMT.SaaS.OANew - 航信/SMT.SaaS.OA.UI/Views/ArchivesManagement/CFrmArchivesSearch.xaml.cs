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
using System.Windows.Data;

namespace SMT.SaaS.OA.UI.Views.ArchivesManagement
{
    public partial class CFrmArchivesSearch : ChildWindow
    {
        public T_OA_ARCHIVES returnValue;
        private SmtOADocumentAdminClient client;
         

        public class searchItem
        {
            public string companyID;
            public string archivesTitle;
            public string archivesID;

            public searchItem()
            { 
            
            }

            public searchItem(string companyIDs, string archivesTitles, string archivesIDs)
            {
                this.archivesID = archivesIDs;
                this.archivesTitle = archivesTitles;
                this.companyID = companyIDs;
            }
        }
        public CFrmArchivesSearch()
        {
            InitializeComponent();
            InitEvent();
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            //client.GetArchivesCanBorrowCompleted += new EventHandler<GetArchivesCanBorrowCompletedEventArgs>(client_GetArchivesCanBorrowCompleted);
            client.GetArchivesCanBorrowByConditionCompleted += new EventHandler<GetArchivesCanBorrowByConditionCompletedEventArgs>(client_GetArchivesCanBorrowByConditionCompleted);
            //client.GetArchivesCanBorrowAsync();
            client.GetArchivesCanBorrowByConditionAsync("", "");
        }

        void client_GetArchivesCanBorrowByConditionCompleted(object sender, GetArchivesCanBorrowByConditionCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    BindDataGrid(e.Result.ToList());
                }
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Alert(ex.ToString());
            }
        }

        #region  绑定DataGird
        private void BindDataGrid(List<T_OA_ARCHIVES> obj)
        {
            if (obj.Count < 1) return;
            PagedCollectionView pcv = new PagedCollectionView(obj);
            Type a = ((PagedCollectionView)pcv).CurrentItem.GetType();
            pcv.PageSize = 10;
            FirstLastPreviousNextNumeric.DataContext = pcv;
            dgArchives.ItemsSource = pcv;
        }
        #endregion

        void client_GetArchivesCanBorrowCompleted(object sender, GetArchivesCanBorrowCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                BindDataGrid(e.Result.ToList());
            }
            else
            {
                HtmlPage.Window.Alert("对不起！未能找到相关记录。");
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {        
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            returnValue = null;
            this.DialogResult = false;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(txtSearchType.Text.Trim()) || !string.IsNullOrEmpty(txtSearchTitle.Text.Trim()))
            //{
            //    client.GetArchivesCanBorrowByConditionAsync(txtSearchTitle.Text.Trim(),txtSearchType.Text.Trim());
            //}
            //else
            //{
            //    client.GetArchivesCanBorrowByConditionAsync("", "");
            //}
            client.GetArchivesCanBorrowByConditionAsync(txtSearchTitle.Text.Trim(), txtSearchType.Text.Trim());
        }

        private void dgArchives_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                T_OA_ARCHIVES archives = (T_OA_ARCHIVES)grid.SelectedItems[0];
                //returnValue = new searchItem(archives.COMPANYID, archives.ARCHIVESTITLE, archives.ARCHIVESID);
                returnValue = archives;
            }
        }
    }
}

