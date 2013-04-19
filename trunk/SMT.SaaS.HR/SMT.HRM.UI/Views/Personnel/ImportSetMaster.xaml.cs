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
using System.Windows.Navigation;

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.HRM.UI.Form.Personnel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class ImportSetMaster : BasePage,IClient
    {
        SMTLoading loadbar = new SMTLoading();
        PersonnelServiceClient client;
        public ImportSetMaster()
        {
            InitializeComponent();
            InitEvent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_BLACKLIST", false);
          //  LoadData();
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);
          
            client = new PersonnelServiceClient();
            client.ImportSetMasterPagingCompleted += new EventHandler<ImportSetMasterPagingCompletedEventArgs>(client_ImportSetMasterPagingCompleted);
            client.ImportSetMasterDeleteCompleted += new EventHandler<ImportSetMasterDeleteCompletedEventArgs>(client_ImportSetMasterDeleteCompleted);
            client.GetImportSetDetailByMasterIDCompleted += new EventHandler<GetImportSetDetailByMasterIDCompletedEventArgs>(client_GetImportSetDetailByMasterIDCompleted);

            this.ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            this.ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            this.ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            this.ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            this.ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            this.Loaded += new RoutedEventHandler(ImportSetMaster_Loaded);
            #region 原来的
            /*
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            */
            #endregion
        }



        void ImportSetMaster_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            #endregion
            LoadData();
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            client.ImportSetMasterPagingAsync(pageMaster.PageIndex, pageMaster.PageSize, "ENTITYNAME", filter,
                paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void client_ImportSetMasterPagingCompleted(object sender, ImportSetMasterPagingCompletedEventArgs e)
        {
            List<T_HR_IMPORTSETMASTER> list = new List<T_HR_IMPORTSETMASTER>();
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                loadbar.Stop();
                ToolBar.btnRefresh.IsEnabled = true;
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dataMaster.ItemsSource = list;
                pageMaster.PageCount = e.pageCount;
            }
            if (list.Count() > 0)
            {
                dataMaster.SelectedItems.Add(list[0]);
            }
            else
            {
                loadbar.Stop();
                ToolBar.btnRefresh.IsEnabled = true;
            }
        }

        #region 添加，修改，删除
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ImportSetMasterForm form = new ImportSetMasterForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (dataMaster.SelectedItems.Count > 0)
            {
                T_HR_IMPORTSETMASTER ent = dataMaster.SelectedItems[0] as T_HR_IMPORTSETMASTER;
                ImportSetMasterForm form = new ImportSetMasterForm(FormTypes.Browse, ent.MASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataMaster.SelectedItems.Count > 0)
            {
                T_HR_IMPORTSETMASTER ent = dataMaster.SelectedItems[0] as T_HR_IMPORTSETMASTER;
                ImportSetMasterForm form = new ImportSetMasterForm(FormTypes.Edit, ent.MASTERID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (dataMaster.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();
                    foreach (T_HR_IMPORTSETMASTER tmp in dataMaster.SelectedItems)
                    {
                        ids.Add(tmp.MASTERID);
                    }
                    client.ImportSetMasterDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void client_ImportSetMasterDeleteCompleted(object sender, ImportSetMasterDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "IMPORTSETMASTER"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                LoadData();
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }
        #endregion

        private void dataMaster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((System.Windows.Controls.DataGrid)(sender)).SelectedItem != null)
            {
                T_HR_IMPORTSETMASTER ent = ((System.Windows.Controls.DataGrid)(sender)).SelectedItem as T_HR_IMPORTSETMASTER;
                if (ent != null)
                {
                    loadbar.Start();
                    client.GetImportSetDetailByMasterIDAsync(ent.MASTERID);
                }
                else
                {
                    loadbar.Stop();
                    ToolBar.btnRefresh.IsEnabled = true;
                }
            }
        }

        void client_GetImportSetDetailByMasterIDCompleted(object sender, GetImportSetDetailByMasterIDCompletedEventArgs e)
        {
            List<T_HR_IMPORTSETDETAIL> list = new List<T_HR_IMPORTSETDETAIL>();
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dateDetail.ItemsSource = list;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
