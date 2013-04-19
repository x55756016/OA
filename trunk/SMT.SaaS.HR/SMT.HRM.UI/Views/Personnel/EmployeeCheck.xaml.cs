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
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class EmployeeCheck : BasePage, IClient
    {
        public string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        PersonnelServiceClient client;

        public EmployeeCheck()
        {
            InitializeComponent();
            InitEvent();
            //GetEntityLogo("T_HR_EMPLOYEECHECK");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEECHECK", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());

        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);

            client = new PersonnelServiceClient();
            client.EmployeeCheckPagingCompleted += new EventHandler<EmployeeCheckPagingCompletedEventArgs>(client_EmployeeCheckPagingCompleted);
            client.EmployeeCheckDeleteCompleted += new EventHandler<EmployeeCheckDeleteCompletedEventArgs>(client_EmployeeCheckDeleteCompleted);

            this.ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            this.ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            this.ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            this.ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            this.ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            this.ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            this.ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            this.ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            this.Loaded += new RoutedEventHandler(EmployeeCheck_Loaded);
        }     

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_EMPLOYEECHECK");
                LoadData();
            }
        }

        void EmployeeCheck_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_EMPLOYEECHECK");
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();


            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    // filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }
            }
            client.EmployeeCheckPagingAsync(dataPager.PageIndex, dataPager.PageSize, "BEREGULARDATE", filter,
                paras, pageCount, Checkstate, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void client_EmployeeCheckPagingCompleted(object sender, EmployeeCheckPagingCompletedEventArgs e)
        {
            List<T_HR_EMPLOYEECHECK> list = new List<T_HR_EMPLOYEECHECK>();
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

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 添加，修改，删除，审核

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            EmployeeCheckForm form = new EmployeeCheckForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 500;
            form.MinHeight = 310;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEECHECK temp = DtGrid.SelectedItems[0] as T_HR_EMPLOYEECHECK;
                EmployeeCheckForm form = new EmployeeCheckForm(FormTypes.Browse, temp.BEREGULARID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinWidth = 500;
                form.MinHeight = 310;
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
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEECHECK temp = DtGrid.SelectedItems[0] as T_HR_EMPLOYEECHECK;
                if (temp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                EmployeeCheckForm form = new EmployeeCheckForm(FormTypes.Edit, temp.BEREGULARID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                form.MinWidth = 500;
                form.MinHeight = 310;
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

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEECHECK temp = DtGrid.SelectedItems[0] as T_HR_EMPLOYEECHECK;
                EmployeeCheckForm form = new EmployeeCheckForm(FormTypes.Resubmit, temp.BEREGULARID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                form.MinWidth = 500;
                form.MinHeight = 310;
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

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_EMPLOYEECHECK tmp in DtGrid.SelectedItems)
                {
                    if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_EMPLOYEECHECK", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                    //{
                    //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.T_HR_EMPLOYEE.EMPLOYEECNAME + ",EMPLOYEECHECK"),
                    //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //    return;
                    //}
                    ids.Add(tmp.BEREGULARID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.EmployeeCheckDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void client_EmployeeCheckDeleteCompleted(object sender, EmployeeCheckDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEECHECK"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEECHECK temp = DtGrid.SelectedItems[0] as T_HR_EMPLOYEECHECK;
                EmployeeCheckForm form = new EmployeeCheckForm(FormTypes.Audit, temp.BEREGULARID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                form.MinWidth = 500;
                form.MinHeight = 310;
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

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_EMPLOYEECHECK");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                T_HR_EMPLOYEECHECK ent = DtGrid.SelectedItem as T_HR_EMPLOYEECHECK;
                CheckGrade form = new CheckGrade(ent.BEREGULARID, ent.T_HR_EMPLOYEE.EMPLOYEEID, "0");
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
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
