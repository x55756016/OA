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

using SMT.Saas.Tools.PersonnelWS;
using SMT.HRM.UI.Form.Personnel;

using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;

using System.Windows.Data;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.HRM.UI.Views.Personnel
{
    public partial class BlackList : BasePage,IClient
    {
        PersonnelServiceClient client;
        SMTLoading loadbar = new SMTLoading();

        private ObservableCollection<string> blackListIDs = new ObservableCollection<string>();

        public ObservableCollection<string> BlackListIDs
        {
            get { return blackListIDs; }
            set { blackListIDs = value; }
        }

        public BlackList()
        {
            InitializeComponent();
            InitPara();
            //GetEntityLogo("T_HR_BLACKLIST");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_BLACKLIST", false);            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        public void InitPara()
        {
            try
            {
                PARENT.Children.Add(loadbar);
                client = new PersonnelServiceClient();
                client.GetBlackListPagingCompleted += new EventHandler<GetBlackListPagingCompletedEventArgs>(client_GetBlackListPagingCompleted);
                client.BlackListDeleteCompleted += new EventHandler<BlackListDeleteCompletedEventArgs>(client_BlackListDeleteCompleted);
                this.Loaded += new RoutedEventHandler(BlackList_Loaded);
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }



        void BlackList_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_BLACKLIST");
            LoadData();
        }

        void client_GetBlackListPagingCompleted(object sender, GetBlackListPagingCompletedEventArgs e)
        {
            List<T_HR_BLACKLIST> list = new List<T_HR_BLACKLIST>();
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
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            TextBox txtBlackIdCard = Utility.FindChildControl<TextBox>(expander, "txtBlackIdCard");
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
               // filter += "NAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(NAME)";
                paras.Add(txtName.Text.Trim());
            }

            if (txtBlackIdCard != null && !string.IsNullOrEmpty(txtBlackIdCard.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                //filter += "IDCARDNUMBER==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(IDCARDNUMBER)";
                paras.Add(txtBlackIdCard.Text.Trim());
            }
            client.GetBlackListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "NAME", filter,
                paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }      

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 添加，修改，删除，查询
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            BlackListForm2 form = new BlackListForm2(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            form.MinWidth = 470;
            form.MinHeight = 300;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_BLACKLIST tmpStandard = DtGrid.SelectedItems[0] as T_HR_BLACKLIST;
                BlackListForm2 form = new BlackListForm2(FormTypes.Browse, tmpStandard.BLACKLISTID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 470;
                form.MinHeight = 300;
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
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_BLACKLIST tmpStandard = DtGrid.SelectedItems[0] as T_HR_BLACKLIST;
                if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmpStandard, "T_HR_BLACKLIST", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                BlackListForm2 form = new BlackListForm2(FormTypes.Edit, tmpStandard.BLACKLISTID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 470;
                form.MinHeight = 300;
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
            if (DtGrid.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();
                    foreach (T_HR_BLACKLIST tmp in DtGrid.SelectedItems)
                    {
                        if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_BLACKLIST", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.NAME + ",BLACKLIST"),
                          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        ids.Add(tmp.BLACKLISTID);
                    }
                    client.BlackListDeleteAsync(ids);
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

        void client_BlackListDeleteCompleted(object sender, BlackListDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "BLACKLIST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_BLACKLIST");
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

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }
    }
}
