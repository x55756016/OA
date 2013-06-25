using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.Form;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;

namespace SMT.HRM.UI.Views.Organization
{
    public partial class Company : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        public string Checkstate { get; set; }

        OrganizationServiceClient client;
        public Company()
        {
            InitializeComponent();
            InitEvent();
            //GetEntityLogo("T_HR_COMPANY");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_COMPANY", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            client = new OrganizationServiceClient();

            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            client.CompanyPagingCompleted += new EventHandler<CompanyPagingCompletedEventArgs>(client_CompanyPagingCompleted);
            client.CompanyDeleteCompleted += new EventHandler<CompanyDeleteCompletedEventArgs>(client_CompanyDeleteCompleted);
            client.CompanyCancelCompleted += new EventHandler<CompanyCancelCompletedEventArgs>(client_CompanyCancelCompleted);
            this.Loaded += new RoutedEventHandler(Company_Loaded);
            this.DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);

            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ImageButton btnCancel = new ImageButton();
            btnCancel.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png", Utility.GetResourceStr("CANCEL")).Click += new RoutedEventHandler(btnCancel_Click);
            ToolBar.stpOtherAction.Children.Add(btnCancel);
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_COMPANY");
                //if (dict.DICTIONARYVALUE.Value.ToInt32() == Convert.ToInt32(CheckStates.Approved))
                //{
                //    ToolBar.btnReSubmit.Visibility = Visibility.Visible;
                //}
                dataPager.PageIndex = 1;
                LoadData();
            }
        }

        void Company_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_COMPANY");
            // Utility.DisplayGridToolBarButton(ToolBar, "T_HR_COMPANY", true);
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        private void LoadData()
        {
            loadbar.Start();

            int pageCount = 0;
            string filter = "";
            string strState = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    // filter += "CNAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(CNAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }

            }
            client.CompanyPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SORTINDEX", filter, paras,
                pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
        }

        void client_CompanyPagingCompleted(object sender, CompanyPagingCompletedEventArgs e)
        {
            List<T_HR_COMPANY> list = new List<T_HR_COMPANY>();
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                    list = e.Result.ToList();

                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            dataPager.PageIndex = 1;
            LoadData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 添加、修改、删除、审核
        public T_HR_COMPANY SelectCompany { get; set; }
        void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectCompany = grid.SelectedItems[0] as T_HR_COMPANY;
            }
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            CompanyForm form = new CompanyForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            //form.MinHeight = 490;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void browser_ReloadDataEvent()
        {
            //dataPager.PageIndex = 1;
            LoadData();
        }


        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectCompany != null)
            {
                CompanyForm form = new CompanyForm(FormTypes.Browse, SelectCompany.COMPANYID);
                EntityBrowser browser = new EntityBrowser(form);
                //form.MinHeight = 490;
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.EntityEditor = form;
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
            if (SelectCompany != null)
            {
                if (SelectCompany.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(SelectCompany, "T_HR_COMPANY", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                CompanyForm form = new CompanyForm(FormTypes.Edit, SelectCompany.COMPANYID);
                EntityBrowser browser = new EntityBrowser(form);
                //form.MinHeight = 490;
                browser.FormType = FormTypes.Edit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.EntityEditor = form;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                //   ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectCompany != null)
            {
                CompanyForm form = new CompanyForm(FormTypes.Resubmit, SelectCompany.COMPANYID);
                EntityBrowser browser = new EntityBrowser(form);
                //form.MinHeight = 490;
                browser.FormType = FormTypes.Resubmit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.EntityEditor = form;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //string strMsg = "";
            //string Result = "";
            //if (SelectCompany != null)
            //{
            //    if (SelectCompany.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
            //    {
            //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

            //        return;
            //    }
            //    //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(SelectCompany, "T_HR_COMPANY", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
            //    //{
            //    //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NODELETEPERMISSION", SelectCompany.CNAME),
            //    //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    //    return;
            //    //}
            //    ComfirmWindow com = new ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        client.CompanyDeleteAsync(SelectCompany.COMPANYID, strMsg);
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //}
            //else
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
            //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //}
            string Result = "";
            string strMsg = string.Empty;
            if (DtGrid.SelectedItems.Count > 0)
            {
                //判断是否可以删除
                string ids = "";
                foreach (T_HR_COMPANY tmp in DtGrid.SelectedItems)
                {
                    if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                        return;
                    }
                    ids += tmp.COMPANYID + ",";
                }
                string a = ids.Substring(0, ids.Length - 1);
                //提示是否删除
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    string newIds = ids.Substring(0, ids.Length - 1);
                    client.CompanyDeleteAsync(newIds, strMsg);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void Company_Click(object sender, RoutedEventArgs e)
        {
            //  client.CompanyDeleteAsync(SelectCompany.COMPANYID);
        }


        void client_CompanyDeleteCompleted(object sender, CompanyDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
            else
            {
                if (e.strMsg != "")
                {
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg),
                    //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "COMPANY"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    dataPager.PageIndex = 1;
                    LoadData();
                }
            }
        }
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectCompany != null)
            {
                CompanyForm form = new CompanyForm(FormTypes.Audit, SelectCompany.COMPANYID);
                EntityBrowser browser = new EntityBrowser(form);
                //form.MinHeight = 490;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        //撤销公司
        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            string strMsg = "";
            if (SelectCompany != null)
            {
                if (SelectCompany.CHECKSTATE != Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("审核通过的才能进行撤销"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if (SelectCompany.EDITSTATE == Convert.ToInt32(EditStates.Canceled).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("已经撤销了"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if (SelectCompany.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString() && SelectCompany.EDITSTATE == Convert.ToInt32(EditStates.Actived).ToString())
                {
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        SelectCompany.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
                        SelectCompany.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                        client.CompanyCancelAsync(SelectCompany, strMsg);
                       
                    };
                    com.SelectionBox(Utility.GetResourceStr("CANCELALTER"), Utility.GetResourceStr("CANCELCONFIRM"), ComfirmWindow.titlename, Result);
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "CANCEL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "CANCEL"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

        }
        void client_CompanyCancelCompleted(object sender, CompanyCancelCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    SelectCompany.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    SelectCompany.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg),
                    //                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
               

                CompanyForm form = new CompanyForm(FormTypes.Resubmit, SelectCompany.COMPANYID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_COMPANY");
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
