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
using System.Collections.ObjectModel;

using SMT.Saas.Tools.PersonnelWS;

using SMT.HRM.UI.Form.Personnel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class PensionMaster : BasePage, IClient
    {
        PersonnelServiceClient client;
        public string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        public PensionMaster()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                InitEvent();
                this.LayoutRoot_Loaded(sender,args);
                GetEntityLogo("T_HR_PENSIONMASTER");
            };
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_PENSIONMASTER", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        private void InitEvent()
        {
            try
            {
                PARENT.Children.Add(loadbar);

                client = new PersonnelServiceClient();
                client.PensionMasterPagingCompleted += new EventHandler<PensionMasterPagingCompletedEventArgs>(client_PensionMasterPagingCompleted);
                client.PensionMasterDeleteCompleted += new EventHandler<PensionMasterDeleteCompletedEventArgs>(client_PensionMasterDeleteCompleted);

                ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
                ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
                ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
                ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
                ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
                ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }



        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_PENSIONMASTER");
                LoadData();
            }
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            //LookUp txtEmpName = Utility.FindChildControl<LookUp>(expander, "lkEmployeeName");
            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmployeeName");
            TextBox txtCardID = Utility.FindChildControl<TextBox>(expander, "txtCardID");
            //  T_HR_EMPLOYEE employee = txtEmpName.DataContext as T_HR_EMPLOYEE;
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
                {
                    // filter += "T_HR_EMPLOYEE.EMPLOYEECNAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(T_HR_EMPLOYEE.EMPLOYEECNAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }
            }
            if (txtCardID != null)
            {
                if (!string.IsNullOrEmpty(txtCardID.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //   filter += "CARDID==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(CARDID)";
                    paras.Add(txtCardID.Text.Trim());
                }
            }
            client.PensionMasterPagingAsync(dataPager.PageIndex, dataPager.PageSize, "COMPUTERNO",
                filter, paras, pageCount, Checkstate, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void client_PensionMasterPagingCompleted(object sender, PensionMasterPagingCompletedEventArgs e)
        {
            List<T_HR_PENSIONMASTER> list = new List<T_HR_PENSIONMASTER>();
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

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 新增，修改，删除
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            PensionMasterForm form = new PensionMasterForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            form.MinHeight = 370.0;

            form.MinWidth = 470;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void Browser_ReloadDataEvent()
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PENSIONMASTER temp = DtGrid.SelectedItems[0] as T_HR_PENSIONMASTER;
                PensionMasterForm form = new PensionMasterForm(FormTypes.Browse, temp.PENSIONMASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinHeight = 370.0;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 470;
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
                T_HR_PENSIONMASTER temp = DtGrid.SelectedItems[0] as T_HR_PENSIONMASTER;
                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(temp, "T_HR_PENSIONMASTER", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                PensionMasterForm form = new PensionMasterForm(FormTypes.Edit, temp.PENSIONMASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                form.MinHeight = 370.0;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 470;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PENSIONMASTER temp = DtGrid.SelectedItems[0] as T_HR_PENSIONMASTER;
                PensionMasterForm form = new PensionMasterForm(FormTypes.Resubmit, temp.PENSIONMASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                form.MinHeight = 370.0;
                form.MinWidth = 470;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
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
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();
                    bool flag = false;
                    foreach (T_HR_PENSIONMASTER tmp in DtGrid.SelectedItems)
                    {
                        if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                        {
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            flag = true;
                            break;
                        }
                        if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_PENSIONMASTER", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.T_HR_EMPLOYEE.EMPLOYEECNAME + ",PENSIONMASTER"),
                          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        ids.Add(tmp.PENSIONMASTERID);
                    }
                    if (flag == true)
                    {
                        return;
                    }
                    client.PensionMasterDeleteAsync(ids);
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

        void client_PensionMasterDeleteCompleted(object sender, PensionMasterDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "PENSIONMASTER"));
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

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PENSIONMASTER temp = DtGrid.SelectedItems[0] as T_HR_PENSIONMASTER;
                PensionMasterForm form = new PensionMasterForm(FormTypes.Audit, temp.PENSIONMASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            LookUp lkEmployeeName = Utility.FindChildControl<LookUp>(expander, "lkEmployeeName");
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
                typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

                if (ent != null)
                {
                    lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_PENSIONMASTER");
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
