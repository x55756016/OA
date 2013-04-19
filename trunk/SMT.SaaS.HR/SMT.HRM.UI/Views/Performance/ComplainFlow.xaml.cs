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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PerformanceWS;
using SMT.HRM.UI.Form.Performance;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.HRM.UI.Views.Performance
{
    public partial class ComplainFlow : BasePage, IClient
    {
        PerformanceServiceClient client;
        SMTLoading loadbar = new SMTLoading();
        //  private OrganizationServiceClient orgClient;
        private string Checkstate { get; set; }
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;//所有公司
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;//所有部门
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions; //所有岗位

        public ComplainFlow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ComplainFlow_Loaded);
        }

        void ComplainFlow_Loaded(object sender, RoutedEventArgs e)
        {
            InitPara();
            GetEntityLogo("T_HR_KPIRECORDCOMPLAIN");
            loadbar.Stop();
        }

        public void InitPara()
        {
            PARENT.Children.Add(loadbar);
            try
            {
                //orgClient = new OrganizationServiceClient();
                //orgClient = new OrganizationServiceClient();
                //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);

                client = new PerformanceServiceClient();
                client.GetComplainRecordPagingCompleted += new EventHandler<GetComplainRecordPagingCompletedEventArgs>(client_GetComplainRecordPagingCompleted);
                client.ComplainRecordDeleteCompleted += new EventHandler<ComplainRecordDeleteCompletedEventArgs>(client_ComplainRecordDeleteCompleted);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                loadbar.Stop();
            }

            //ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
            //ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
            //this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            //this.Loaded += new RoutedEventHandler(Left_Loaded);
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            LoadData();
        }


        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_KPIRECORDCOMPLAIN", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            //  BindTree();
        }

        void client_ComplainRecordDeleteCompleted(object sender, ComplainRecordDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "RECORDCOMPLAIN"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
                loadbar.Stop();
            }
        }

        void client_GetComplainRecordPagingCompleted(object sender, GetComplainRecordPagingCompletedEventArgs e)
        {
            List<V_COMPLAINRECORD> list = new List<V_COMPLAINRECORD>();
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
                DtGrid.SelectedIndex = 0;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_COMPLAINRECORD tmpEnt = DtGrid.SelectedItems[0] as V_COMPLAINRECORD;
                Form.Performance.ComplainRecordForm form = new Form.Performance.ComplainRecordForm(FormTypes.Audit, tmpEnt.T_HR_KPIRECORDCOMPLAIN.COMPLAINID);
                //form = new SMT.HRM.UI.Form.Salary.EmployeeSalaryRecordForm(FormTypes.Browse, tmpEnt.EMPLOYEESALARYRECORDID);
                //form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 230;
                browser.FormType = FormTypes.Audit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

            //if (DtGrid.SelectedItems.Count > 0)
            //{
            //    V_COMPLAINRECORD tmpEnt = DtGrid.SelectedItems[0] as V_COMPLAINRECORD;
            //    Form.Performance.ComplainRecordForm form = new Form.Performance.ComplainRecordForm(FormTypes.Audit, tmpEnt);
            //    //form.IsEnabled = false;
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinHeight = 230;
            //    browser.FormType = FormTypes.Audit;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            //}
            //else
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            //}

        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_COMPLAINRECORD tmpEnt = DtGrid.SelectedItems[0] as V_COMPLAINRECORD;
                //Form.Performance.ComplainRecordForm form = new Form.Performance.ComplainRecordForm(FormTypes.Audit, tmpEnt);
                Form.Performance.ComplainRecordForm form = new Form.Performance.ComplainRecordForm(FormTypes.Browse, tmpEnt.T_HR_KPIRECORDCOMPLAIN.COMPLAINID);
                //form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 230;
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }



            //if (DtGrid.SelectedItems.Count > 0)
            //{
            //    V_COMPLAINRECORD tmpEnt = DtGrid.SelectedItems[0] as V_COMPLAINRECORD;
            //    Form.Performance.ComplainRecordForm form = new Form.Performance.ComplainRecordForm(FormTypes.Browse, tmpEnt.T_HR_KPIRECORDCOMPLAIN.COMPLAINID);
            //    //form = new SMT.HRM.UI.Form.Salary.EmployeeSalaryRecordForm(FormTypes.Browse, tmpEnt.EMPLOYEESALARYRECORDID);
            //    form.IsEnabled = false;
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinHeight = 230;
            //    browser.FormType = FormTypes.Audit;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            //}
            //else
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            //}

        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            #region -----
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_COMPLAINRECORD tmpEnt = DtGrid.SelectedItems[0] as V_COMPLAINRECORD;
                if (tmpEnt.T_HR_KPIRECORDCOMPLAIN.CHECKSTATE == "0" || tmpEnt.T_HR_KPIRECORDCOMPLAIN.CHECKSTATE == "3")
                {
                    System.Collections.ObjectModel.ObservableCollection<string> ids = new System.Collections.ObjectModel.ObservableCollection<string>();

                    foreach (V_COMPLAINRECORD tmp in DtGrid.SelectedItems)
                    {
                        ids.Add(tmp.T_HR_KPIRECORDCOMPLAIN.COMPLAINID);
                    }

                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        client.ComplainRecordDeleteAsync(ids);
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTDELETE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            #endregion

        }

        public void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            int sType = -1;
            string sValue = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            string selectedType = treeOrganization.sType;
            if (!string.IsNullOrEmpty(selectedType))
            {
                switch (selectedType)
                {
                    case "Company":
                        sType = 0;
                        break;
                    case "Department":
                        sType = 1;
                        break;
                    case "Post":
                        sType = 2;
                        break;
                }
                sValue = treeOrganization.sValue;
            }
            else
            {
                loadbar.Stop();
                //if (frist) frist = false;  else  Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "PLEASESELECT");
                return;
            }

            DatePicker dpStartDate = Utility.FindChildControl<DatePicker>(expander, "dpStartDate");
            DatePicker dpEndDate = Utility.FindChildControl<DatePicker>(expander, "dpEndDate");

            string StartDate = string.Empty;
            string EndDate = string.Empty;

            if (dpStartDate != null)
            {
                StartDate = dpStartDate.Text;
            }
            if (dpEndDate != null)
            {
                EndDate = dpEndDate.Text;
            }

            string strState = "";
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            TextBox txtEmpCode = Utility.FindChildControl<TextBox>(expander, "txtEmpCode");

            if (!string.IsNullOrEmpty(txtEmpCode.Text.Trim()))
            {
                filter += "EMPLOYEECODE==@" + paras.Count().ToString();
                paras.Add(txtEmpCode.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "EMPLOYEECNAME==@" + paras.Count().ToString();
                paras.Add(txtEmpName.Text.Trim());
            }

            //client.GetCustomGuerdonSetPagingAsync(dataPager.PageIndex, dataPager.PageSize, "GUERDONNAME", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            client.GetComplainRecordPagingAsync(dataPager.PageIndex, dataPager.PageSize, "T_HR_KPIRECORD.UPDATEDATE", filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, StartDate, EndDate, strState);
            //loadbar.Stop();
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选择某审核状态是重新加载数据

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_KPIRECORDCOMPLAIN");
                Checkstate = dict.DICTIONARYVALUE.ToString();
                LoadData();
            }
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            //ToolBar.btnAudit.Visibility = Visibility.Visible;
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        //private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{

        //}

        private void dpStartDate_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dpEndDate_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

    }
}
