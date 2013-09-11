using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;

using SMT.Saas.Tools.OrganizationWS;
using SMT.HRM.UI.Form;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.AutoCompleteComboBox;

namespace SMT.HRM.UI.Views.Organization
{
    public partial class Department : BasePage
    {
        public string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient client;
        public Department()
        {
            InitializeComponent();
            InitEvent();
            //GetEntityLogo("T_HR_DEPARTMENT");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_DEPARTMENT", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);


            client = new OrganizationServiceClient();
            client.DepartmentPagingCompleted += new EventHandler<DepartmentPagingCompletedEventArgs>(client_DepartmentPagingCompleted);
            client.DepartmentDeleteCompleted += new EventHandler<DepartmentDeleteCompletedEventArgs>(client_DepartmentDeleteCompleted);
            client.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(client_GetCompanyActivedCompleted);
            client.DepartmentCancelCompleted += new EventHandler<DepartmentCancelCompletedEventArgs>(client_DepartmentCancelCompleted);
            this.Loaded += new RoutedEventHandler(Department_Loaded);
            this.DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            //航信发布需要打卡
            ToolBar.btnImport.Visibility = Visibility.Visible;
            ToolBar.btnImport.Click += new RoutedEventHandler(btnImport_Click);

            ImageButton btnCancel = new ImageButton();
            btnCancel.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png", Utility.GetResourceStr("CANCEL")).Click += new RoutedEventHandler(btnCancel_Click);
            ToolBar.stpOtherAction.Children.Add(btnCancel);
        }

        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            ImportOrgInfoForm form = new ImportOrgInfoForm();
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 260;
            form.MinWidth = 400;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void client_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        {
            ObservableCollection<T_HR_COMPANY> cmp = new ObservableCollection<T_HR_COMPANY>();
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
               // ComboBox cbxCompanyName = Utility.FindChildControl<ComboBox>(expander, "cbxCompanyName");
                AutoCompleteComboBox acbCompanyName = Utility.FindChildControl<AutoCompleteComboBox>(expander, "acbCompanyName");
                cmp = e.Result;
                T_HR_COMPANY all = new T_HR_COMPANY();
                all.COMPANYID = "companyID";
                all.CNAME = Utility.GetResourceStr("ALL");
                if (cmp != null)
                {
                    cmp.Insert(0, all);
                }
                acbCompanyName.ItemsSource = cmp;
                acbCompanyName.ValueMemberPath = "CNAME";
            }
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

            //ComboBox cbxCompanyName = Utility.FindChildControl<ComboBox>(expander, "cbxCompanyName");
            AutoCompleteComboBox acbCompanyName = Utility.FindChildControl<AutoCompleteComboBox>(expander, "acbCompanyName");
            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (acbCompanyName != null)
            {
                if (acbCompanyName.SelectedItem != null)
                {
                    T_HR_COMPANY ent = acbCompanyName.SelectedItem as T_HR_COMPANY;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "T_HR_COMPANY.CNAME==@" + paras.Count().ToString();
                    paras.Add(ent.CNAME);
                }
            }
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //   filter += "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME.Contains(@" + paras.Count().ToString()+")";
                    filter += " @" + paras.Count().ToString() + ".Contains(T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }
            }

            client.DepartmentPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SORTINDEX", filter,
                paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
        }

        void client_DepartmentPagingCompleted(object sender, DepartmentPagingCompletedEventArgs e)
        {
            List<T_HR_DEPARTMENT> list = new List<T_HR_DEPARTMENT>();
            if (e.Error != null && e.Error.Message != "")
            {
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

        void Department_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_DEPARTMENT");
            //  Utility.DisplayGridToolBarButton(ToolBar, "T_HR_DEPARTMENT", true);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            //绑定公司
            client.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_DEPARTMENT");
                //if (dict.DICTIONARYVALUE.Value.ToInt32() == Convert.ToInt32(CheckStates.Approved))
                //{
                //    ToolBar.btnReSubmit.Visibility = Visibility.Visible;
                //}
                dataPager.PageIndex = 1;
                LoadData();
            }
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

        #region 添加,修改,删除
        public T_HR_DEPARTMENT SelectDepartment { get; set; }
        void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectDepartment = grid.SelectedItems[0] as T_HR_DEPARTMENT;
            }
        }
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            DepartmentForm form = new DepartmentForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 300;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
           // dataPager.PageIndex = 1;
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectDepartment != null)
            {
                DepartmentForm form = new DepartmentForm(FormTypes.Browse, SelectDepartment.DEPARTMENTID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinHeight = 300;
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
            if (SelectDepartment != null)
            {
                if (SelectDepartment.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                DepartmentForm form = new DepartmentForm(FormTypes.Edit, SelectDepartment.DEPARTMENTID);
                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(SelectDepartment, "T_HR_DEPARTMENT", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                form.MinHeight = 300;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
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

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            string strMsg = "";
            if (SelectDepartment != null)
            {
                if (SelectDepartment.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(SelectDepartment, "T_HR_DEPARTMENT", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NODELETEPERMISSION", SelectDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.DepartmentDeleteAsync(SelectDepartment.DEPARTMENTID, strMsg);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }


        void client_DepartmentDeleteCompleted(object sender, DepartmentDeleteCompletedEventArgs e)
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
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg),
                    //                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                dataPager.PageIndex = 1;
                LoadData();
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectDepartment != null)
            {
                DepartmentForm form = new DepartmentForm(FormTypes.Audit, SelectDepartment.DEPARTMENTID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 300;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectDepartment != null)
            {
                DepartmentForm form = new DepartmentForm(FormTypes.Resubmit, SelectDepartment.DEPARTMENTID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                form.MinHeight = 300;
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


        //撤销部门
        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            string strMsg = "";
            if (SelectDepartment != null)
            {
                if (SelectDepartment.CHECKSTATE != Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("审核通过的才能进行撤销"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if (SelectDepartment.EDITSTATE == Convert.ToInt32(EditStates.Canceled).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("已经撤销了"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if (SelectDepartment.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        SelectDepartment.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
                        SelectDepartment.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                        client.DepartmentCancelAsync(SelectDepartment, strMsg);
                      
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
        void client_DepartmentCancelCompleted(object sender, DepartmentCancelCompletedEventArgs e)
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
                    SelectDepartment.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    SelectDepartment.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg),
                    //   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }

              

                DepartmentForm form = new DepartmentForm(FormTypes.Resubmit, SelectDepartment.DEPARTMENTID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_DEPARTMENT");
        }

        private void cbxCompanyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataPager.PageIndex = 1;
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

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }

        /// <summary>
        /// 选择相应公司名称后进行加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void acbCompanyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }
    }
}
