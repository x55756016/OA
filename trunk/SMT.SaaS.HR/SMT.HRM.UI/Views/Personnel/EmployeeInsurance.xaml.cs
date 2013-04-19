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

using SMT.HRM.UI.Form.Personnel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class EmployeeInsurance : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();

        PersonnelServiceClient client;
        public string Checkstate { get; set; }

        public EmployeeInsurance()
        {
            InitializeComponent();
            InitEvent();
            //GetEntityLogo("T_HR_EMPLOYEEINSURANCE");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEEINSURANCE", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
        }

        private void InitEvent()
        {
            try
            {
                PARENT.Children.Add(loadbar);

                client = new PersonnelServiceClient();
                client.EmployeeInsurancePagingCompleted += new EventHandler<EmployeeInsurancePagingCompletedEventArgs>(client_EmployeeInsurancePagingCompleted);
                client.EmployeeInsuranceDeleteCompleted += new EventHandler<EmployeeInsuranceDeleteCompletedEventArgs>(client_EmployeeInsuranceDeleteCompleted);

                ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
                ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
                ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
                ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
                ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
                ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

                this.Loaded += new RoutedEventHandler(EmployeeInsurance_Loaded);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        void EmployeeInsurance_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_EMPLOYEEINSURANCE");
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_EMPLOYEEINSURANCE");
                ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
                LoadData();
            }
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            string strState = "";
            ObservableCollection<object> paras = new ObservableCollection<object>();
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }
            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            TextBox txtInsuranceName = Utility.FindChildControl<TextBox>(expander, "txtInsuranceName");
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
                {
                    // filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(T_HR_EMPLOYEE.EMPLOYEECNAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }
            }
            if (txtInsuranceName != null)
            {
                if (!string.IsNullOrEmpty(txtInsuranceName.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    // filter += "INSURANCENAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(INSURANCENAME)";
                    paras.Add(txtInsuranceName.Text.Trim());
                }
            }
            client.EmployeeInsurancePagingAsync(dataPager.PageIndex, dataPager.PageSize, "INSURANCENAME", filter,
                paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void client_EmployeeInsurancePagingCompleted(object sender, EmployeeInsurancePagingCompletedEventArgs e)
        {
            List<T_HR_EMPLOYEEINSURANCE> list = new List<T_HR_EMPLOYEEINSURANCE>();
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
                dataPager.PageCount = e.pageCount; ;
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

        #region 添加,修改,删除,查询,审核
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            EmployeeInsuranceForm form = new EmployeeInsuranceForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 330;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void from_ReloadDataEvent()
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEEINSURANCE tempEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEEINSURANCE;
                EmployeeInsuranceForm form = new EmployeeInsuranceForm(FormTypes.Browse, tempEnt.EMPLOYINSURANCEID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinHeight = 330;
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
                T_HR_EMPLOYEEINSURANCE tempEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEEINSURANCE;
                if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tempEnt, "T_HR_EMPLOYEEINSURANCE", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                EmployeeInsuranceForm form = new EmployeeInsuranceForm(FormTypes.Edit, tempEnt.EMPLOYINSURANCEID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                form.MinHeight = 330;
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
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();
                bool flag = false;
                foreach (T_HR_EMPLOYEEINSURANCE tmp in DtGrid.SelectedItems)
                {
                    if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        flag = true;
                        break;
                    }
                    if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_EMPLOYEEINSURANCE", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.T_HR_EMPLOYEE.EMPLOYEECNAME + ",EMPLOYINSURANCE"),
                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    ids.Add(tmp.EMPLOYINSURANCEID);
                }
                ComfirmWindow com = new ComfirmWindow();
                if (flag != true)
                {
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        client.EmployeeInsuranceDeleteAsync(ids);
                    };
                }
                else 
                {
                    return;
                }
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                //ComfirmWindow com = new ComfirmWindow();
                //com.OnSelectionBoxClosed += (obj, result) =>
                //{
                //    ObservableCollection<string> ids = new ObservableCollection<string>();
                //    bool flag = false;
                //    foreach (T_HR_EMPLOYEEINSURANCE tmp in DtGrid.SelectedItems)
                //    {
                //        if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                //        {
                //            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //            flag = true;
                //            break;
                //        }
                //        if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_EMPLOYEEINSURANCE", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //        {
                //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.T_HR_EMPLOYEE.EMPLOYEECNAME + ",EMPLOYINSURANCE"),
                //          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //            return;
                //        }
                //        ids.Add(tmp.EMPLOYINSURANCEID);
                //    }
                //    if (flag == true)
                //    {
                //        return;
                //    }
                //    client.EmployeeInsuranceDeleteAsync(ids);
                //};
                //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

            }
        }

        void client_EmployeeInsuranceDeleteCompleted(object sender, EmployeeInsuranceDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYINSURANCE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEEINSURANCE tempEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEEINSURANCE;
                EmployeeInsuranceForm form = new EmployeeInsuranceForm(FormTypes.Audit, tempEnt.EMPLOYINSURANCEID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                form.MinHeight = 330;
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
            SetRowLogo(DtGrid, e.Row, "T_HR_EMPLOYEEINSURANCE");
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
