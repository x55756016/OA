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
using System.Collections.ObjectModel;
using SMT.HRM.UI.Form.Personnel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.FlowWFService;
using System.Text;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.HRM.UI.Views.Personnel
{
    public partial class EmployeeEntry : BasePage, IClient
    {
        PersonnelServiceClient client;
        SMTLoading loadbar = new SMTLoading();
        public string Checkstate { get; set; }
        //  OrganizationWS.OrganizationServiceClient orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;
        ObservableCollection<string> ids;
        ObservableCollection<string> employeeids;
        string strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        ServiceClient clientFlow = new ServiceClient();
        ObservableCollection<SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T> flowList { get; set; }
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient perclient = new PermissionServiceClient();
        bool pageHasLoaded = false;
        public EmployeeEntry()
        {
            InitializeComponent();
            InitEvent();
            //GetEntityLogo("T_HR_EMPLOYEEENTRY");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEEENTRY", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());

        }


        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            client = new PersonnelServiceClient();
            client.GetEmployeeEntryPagingCompleted += new EventHandler<GetEmployeeEntryPagingCompletedEventArgs>(client_GetEmployeeEntryPagingCompleted);
            client.EmployeeEntryDeleteCompleted += new EventHandler<EmployeeEntryDeleteCompletedEventArgs>(client_EmployeeEntryDeleteCompleted);
            clientFlow.GetFlowInfoCompleted += new EventHandler<GetFlowInfoCompletedEventArgs>(clientFlow_GetFlowInfoCompleted);

            //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);

            perclient.SysUserBatchDelCompleted += new EventHandler<SysUserBatchDelCompletedEventArgs>(perclient_SysUserBatchDelCompleted);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            //航信
            ToolBar.btnImport.Visibility = Visibility.Visible;
            ToolBar.btnImport.Click += new RoutedEventHandler(btnImport_Click);

            this.Loaded += new RoutedEventHandler(EmployeeEntry_Loaded);
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 批量导入员工入职
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            ImportEmployeeEntryForm form = new ImportEmployeeEntryForm();
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 260;
            form.MinWidth = 400;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        void EmployeeEntry_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_EMPLOYEEENTRY");

            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            pageHasLoaded = true;
            //  BindTree();
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_EMPLOYEEENTRY");
                if (pageHasLoaded)
                {
                    LoadData();
                }
            }
        }

        void clientFlow_GetFlowInfoCompleted(object sender, GetFlowInfoCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //flowList = e.Result;

                if (flowList == null)
                {
                    flowList = new ObservableCollection<SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T>();
                }

                BindGrid();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
                loadbar.Stop();
            }
        }

        private void BindGrid()
        {
            // StringBuilder strIds = new StringBuilder();

            //foreach (SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T item in flowList)
            //{
            //    strIds.Append(item.FormID + ",");
            //}

            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    // filter += "T_HR_EMPLOYEE.EMPLOYEECNAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECNAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }
            }

            TextBox txtEmpCode = Utility.FindChildControl<TextBox>(expander, "txtEmpCode");
            if (txtEmpCode != null)
            {
                if (!string.IsNullOrEmpty(txtEmpCode.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    // filter += "T_HR_EMPLOYEE.EMPLOYEECODE==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECODE)";
                    paras.Add(txtEmpCode.Text.Trim());
                }
            }
            string strCheckState = "";
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strCheckState = Checkstate;
            }

            //  Utility.SetFilterWithflow("EMPLOYEEENTRYID", strIds.ToString(), ref strCheckState, ref filter, ref paras);

            //if (strCheckState != Convert.ToInt32(CheckStates.All).ToString())
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "CHECKSTATE==@" + paras.Count().ToString();
            //    paras.Add(strCheckState);
            //}
            string sType = "", sValue = "";
            //  TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            sType = treeOrganization.sType;
            sValue = treeOrganization.sValue;
            //if (selectedItem != null)
            //{
            //    string IsTag = selectedItem.Tag.ToString();
            //    switch (IsTag)
            //    {
            //        case "Company":
            //            OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
            //            sType = "Company";
            //            sValue = company.COMPANYID;
            //            break;
            //        case "Department":
            //            OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as OrganizationWS.T_HR_DEPARTMENT;
            //            sType = "Department";
            //            sValue = department.DEPARTMENTID;
            //            break;
            //        case "Post":
            //            OrganizationWS.T_HR_POST post = selectedItem.DataContext as OrganizationWS.T_HR_POST;
            //            sType = "Post";
            //            sValue = post.POSTID;
            //            break;
            //    }
            //}
            client.GetEmployeeEntryPagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECODE", filter,
                paras, pageCount, sType, sValue, strOwnerID, strCheckState);
        }

        private void LoadData()
        {
            loadbar.Start();
            BindGrid();
            // clientFlow.GetFlowInfoAsync("", "", "0", "", "T_HR_EMPLOYEEENTRY", "", strOwnerID);
        }

        void client_GetEmployeeEntryPagingCompleted(object sender, GetEmployeeEntryPagingCompletedEventArgs e)
        {

            List<V_EMPLOYEEENTRY> list = new List<V_EMPLOYEEENTRY>();
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
            dataPager.IsEnabled = true;
            loadbar.Stop();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            btn.IsEnabled = false;
            LoadData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            dataPager.IsEnabled = false;
            LoadData();
        }

        #region 添加，修改，删除
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            EntityPageForm browser = new EntityPageForm();
            browser.ReloadDataEvent += new EntityPageForm.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_EMPLOYEEENTRY temp = DtGrid.SelectedItems[0] as V_EMPLOYEEENTRY;
                EmployeeEntryForm form = new EmployeeEntryForm(FormTypes.Browse, temp.EMPLOYEEENTRYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 300;
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
           // Utility.CreateFormFromEngine("64289b76-50b5-4f06-a5ef-0c88089f259a", "SMT.HRM.UI.Form.Personnel.EmployeeEntryForm", "Audit");
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_EMPLOYEEENTRY temp = DtGrid.SelectedItems[0] as V_EMPLOYEEENTRY;
                if (temp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(temp, "T_HR_EMPLOYEEENTRY", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                EmployeeEntryForm form = new EmployeeEntryForm(FormTypes.Edit, temp.EMPLOYEEENTRYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 300;
                browser.FormType = FormTypes.Edit;
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

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_EMPLOYEEENTRY temp = DtGrid.SelectedItems[0] as V_EMPLOYEEENTRY;
                EmployeeEntryForm form = new EmployeeEntryForm(FormTypes.Audit, temp.EMPLOYEEENTRYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 300;
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
        /// <summary>
        /// 重新提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_EMPLOYEEENTRY temp = DtGrid.SelectedItems[0] as V_EMPLOYEEENTRY;
                EmployeeEntryForm form = new EmployeeEntryForm(FormTypes.Resubmit, temp.EMPLOYEEENTRYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 300;
                browser.FormType = FormTypes.Resubmit;
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
                    ids = new ObservableCollection<string>();
                    employeeids = new ObservableCollection<string>();
                    bool flag = false;
                    foreach (V_EMPLOYEEENTRY tmp in DtGrid.SelectedItems)
                    {
                        if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                        {
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            flag = true;
                            break;
                        }
                        if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_EMPLOYEEENTRY", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.EMPLOYEECNAME + ",EMPLOYEEENTRY"),
                          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        
                        ids.Add(tmp.EMPLOYEEENTRYID);
                        employeeids.Add(tmp.EMPLOYEEID);

                        
                    }
                   
                    if (flag == true)
                    {
                        return;
                    }
                    client.EmployeeEntryDeleteAsync(ids);
                   
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

        void client_EmployeeEntryDeleteCompleted(object sender, EmployeeEntryDeleteCompletedEventArgs e)
        {
            //2012-8-15
            //返回错误则提示信息
            if (e.Result == false)
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("DALETEFAILED"));
                return;
            }
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

                
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEEENTRY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
                //  perclient.SysUserBatchDelAsync(employeeids);
            }
        }
        void perclient_SysUserBatchDelCompleted(object sender, SysUserBatchDelCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (string.IsNullOrEmpty(e.Result))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEEENTRY"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
                                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }


            }
            LoadData();
        }

        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_EMPLOYEEENTRY");
        }


        //#region 树形控件的操作
        ////绑定树
        //private void BindTree()
        //{
        //    if (Application.Current.Resources.Contains(SysClientCache.SYS_CompanyInfo.ToString()))
        //    {
        //        allCompanys = Application.Current.Resources[SysClientCache.SYS_CompanyInfo.ToString()] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
        //        BindCompany();
        //    }
        //    else
        //    {
        //        orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }

        //}

        //void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allCompanys = e.Result.ToList();
        //        }
        //        UICache.CreateCache(SysClientCache.SYS_CompanyInfo.ToString(), allCompanys);
        //        BindCompany();
        //        //orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //}

        //void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allDepartments = e.Result.ToList();
        //        }
        //        UICache.CreateCache(SysClientCache.SYS_DepartmentInfo.ToString(), allDepartments);
        //        BindDepartment();
        //        //   orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //}

        //void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        //{

        //}

        //void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allPositions = e.Result.ToList();
        //        }
        //        UICache.CreateCache(SysClientCache.SYS_PostInfo.ToString(), allPositions);
        //        BindPosition();
        //    }
        //}

        //private void BindCompany()
        //{
        //    treeOrganization.Items.Clear();
        //    if (allCompanys != null)
        //    {
        //        foreach (OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
        //        {
        //            if (tmpOrg.T_HR_COMPANY2 == null || string.IsNullOrEmpty(tmpOrg.T_HR_COMPANY2.COMPANYID))
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpOrg.CNAME;
        //                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //                item.DataContext = tmpOrg;

        //                //标记为公司
        //                item.Tag = "0";
        //                treeOrganization.Items.Add(item);

        //                AddChildOrgItems(item, tmpOrg.COMPANYID);
        //            }
        //        }
        //        if (Application.Current.Resources.Contains(SysClientCache.SYS_DepartmentInfo.ToString()))
        //        {
        //            allDepartments = Application.Current.Resources[SysClientCache.SYS_DepartmentInfo.ToString()] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
        //            BindDepartment();
        //        }
        //        else
        //        {
        //            orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //        }
        //    }
        //}

        //private void AddChildOrgItems(TreeViewItem item, string companyID)
        //{
        //    List<OrganizationWS.T_HR_COMPANY> childs = GetChildORG(companyID);
        //    if (childs == null || childs.Count <= 0)
        //        return;

        //    foreach (OrganizationWS.T_HR_COMPANY childOrg in childs)
        //    {
        //        TreeViewItem childItem = new TreeViewItem();
        //        childItem.Header = childOrg.CNAME;
        //        childItem.DataContext = childOrg;
        //        childItem.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //        childItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

        //        //标记为公司
        //        childItem.Tag = "0";
        //        item.Items.Add(childItem);

        //        AddChildOrgItems(childItem, childOrg.COMPANYID);
        //    }
        //}

        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> GetChildORG(string companyID)
        //{
        //    List<OrganizationWS.T_HR_COMPANY> orgs = new List<OrganizationWS.T_HR_COMPANY>();

        //    foreach (OrganizationWS.T_HR_COMPANY org in allCompanys)
        //    {
        //        if (org.T_HR_COMPANY2 != null && org.T_HR_COMPANY2.COMPANYID == companyID)
        //            orgs.Add(org);
        //    }
        //    return orgs;
        //}

        //private void BindDepartment()
        //{
        //    if (allDepartments != null)
        //    {
        //        foreach (OrganizationWS.T_HR_DEPARTMENT tmpDep in allDepartments)
        //        {
        //            if (tmpDep.T_HR_COMPANY == null)
        //                continue;

        //            TreeViewItem parentItem = GetParentItem("0", tmpDep.T_HR_COMPANY.COMPANYID);
        //            if (parentItem != null)
        //            {
        //                TreeViewItem item = new TreeViewItem();

        //                item.Header = tmpDep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //                item.DataContext = tmpDep;
        //                item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

        //                //标记为部门
        //                item.Tag = "1";
        //                parentItem.Items.Add(item);
        //            }
        //        }
        //        if (Application.Current.Resources.Contains(SysClientCache.SYS_PostInfo.ToString()))
        //        {
        //            allPositions = Application.Current.Resources[SysClientCache.SYS_PostInfo.ToString()] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
        //            BindPosition();
        //        }
        //        else
        //        {
        //            orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //        }

        //    }
        //}

        //private TreeViewItem GetParentItem(string parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    foreach (TreeViewItem item in treeOrganization.Items)
        //    {
        //        tmpItem = GetParentItemFromChild(item, parentType, parentID);
        //        if (tmpItem != null)
        //        {
        //            break;
        //        }
        //    }
        //    return tmpItem;
        //}

        //private TreeViewItem GetParentItemFromChild(TreeViewItem item, string parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    if (item.Tag != null && item.Tag.ToString() == parentType)
        //    {
        //        switch (parentType)
        //        {
        //            case "0":
        //                OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as OrganizationWS.T_HR_COMPANY;
        //                if (tmpOrg != null)
        //                {
        //                    if (tmpOrg.COMPANYID == parentID)
        //                        return item;
        //                }
        //                break;
        //            case "1":
        //                OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as OrganizationWS.T_HR_DEPARTMENT;
        //                if (tmpDep != null)
        //                {
        //                    if (tmpDep.DEPARTMENTID == parentID)
        //                        return item;
        //                }
        //                break;
        //        }

        //    }
        //    if (item.Items != null && item.Items.Count > 0)
        //    {
        //        foreach (TreeViewItem childitem in item.Items)
        //        {
        //            tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
        //            if (tmpItem != null)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return tmpItem;
        //}

        //private void BindPosition()
        //{
        //    if (allPositions != null)
        //    {
        //        foreach (OrganizationWS.T_HR_POST tmpPosition in allPositions)
        //        {
        //            if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
        //                continue;
        //            TreeViewItem parentItem = GetParentItem("1", tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
        //            if (parentItem != null)
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
        //                item.DataContext = tmpPosition;
        //                item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

        //                //标记为岗位
        //                item.Tag = "2";
        //                parentItem.Items.Add(item);
        //            }
        //        }
        //    }
        //    //树全部展开
        //    //treeOrganization.ExpandAll();
        //    //if (treeOrganization.Items.Count > 0)
        //    //{
        //    //    TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
        //    //    selectedItem.IsSelected = true;
        //    //}
        //}
        //#endregion
        #region 树形控件的操作
        //绑定树
        //private void BindTree()
        //{

        //    if (Application.Current.Resources.Contains("ORGTREESYSCompanyInfo"))
        //    {
        //        // allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
        //        BindCompany();
        //    }
        //    else
        //    {
        //        orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }

        //}

        //void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
        //       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        if (e.Result == null)
        //        {
        //            return;
        //        }

        //        ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> entTemps = e.Result;
        //        allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
        //        allCompanys.Clear();
        //        var ents = entTemps.OrderBy(c => c.FATHERID);
        //        ents.ForEach(item =>
        //        {
        //            allCompanys.Add(item);
        //        });

        //        UICache.CreateCache("ORGTREESYSCompanyInfo", allCompanys);
        //        orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //}

        //void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
        //       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        if (e.Result == null)
        //        {
        //            return;
        //        }

        //        ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> entTemps = e.Result;
        //        allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
        //        allDepartments.Clear();
        //        var ents = entTemps.OrderBy(c => c.FATHERID);
        //        ents.ForEach(item =>
        //        {
        //            allDepartments.Add(item);
        //        });

        //        UICache.CreateCache("ORGTREESYSDepartmentInfo", allDepartments);

        //        BindCompany();

        //        orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        //    }
        //}

        //void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        //{

        //}

        //void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
        //       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allPositions = e.Result.ToList();
        //        }
        //        UICache.CreateCache("ORGTREESYSPostInfo", allPositions);
        //        BindPosition();
        //    }
        //}

        //private void BindCompany()
        //{
        //    treeOrganization.Items.Clear();
        //    allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

        //    allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

        //    if (allCompanys == null)
        //    {
        //        return;
        //    }

        //    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> TopCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();

        //    foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
        //    {
        //        //如果当前公司没有父机构的ID，则为顶级公司
        //        if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
        //        {
        //            TreeViewItem item = new TreeViewItem();
        //            item.Header = tmpOrg.CNAME;
        //            item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //            item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //            item.DataContext = tmpOrg;

        //            //状态在未生效和撤消中时背景色为红色
        //            SolidColorBrush brush = new SolidColorBrush();
        //            if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //            {
        //                brush.Color = Colors.Red;
        //                item.Foreground = brush;
        //            }
        //            else
        //            {
        //                brush.Color = Colors.Black;
        //                item.Foreground = brush;
        //            }
        //            //标记为公司
        //            item.Tag = OrgTreeItemTypes.Company;
        //            treeOrganization.Items.Add(item);
        //            TopCompany.Add(tmpOrg);
        //        }
        //        else
        //        {
        //            //查询当前公司是否在公司集合内有父公司存在
        //            var ent = from c in allCompanys
        //                      where tmpOrg.FATHERTYPE == "0" && c.COMPANYID == tmpOrg.FATHERID
        //                      select c;
        //            var ent2 = from c in allDepartments
        //                       where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
        //                       select c;

        //            //如果不存在，则为顶级公司
        //            if (ent.Count() == 0 && ent2.Count() == 0)
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpOrg.CNAME;
        //                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //                item.DataContext = tmpOrg;

        //                //状态在未生效和撤消中时背景色为红色
        //                SolidColorBrush brush = new SolidColorBrush();
        //                if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //                {
        //                    brush.Color = Colors.Red;
        //                    item.Foreground = brush;
        //                }
        //                else
        //                {
        //                    brush.Color = Colors.Black;
        //                    item.Foreground = brush;
        //                }
        //                //标记为公司
        //                item.Tag = OrgTreeItemTypes.Company;
        //                treeOrganization.Items.Add(item);

        //                TopCompany.Add(tmpOrg);
        //            }
        //        }
        //    }
        //    //开始递归
        //    foreach (var topComp in TopCompany)
        //    {
        //        TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, topComp.COMPANYID);
        //        List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany = (from ent in allCompanys
        //                                                                      where ent.FATHERTYPE == "0"
        //                                                                      && ent.FATHERID == topComp.COMPANYID
        //                                                                      select ent).ToList();

        //        List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
        //                                                                            where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
        //                                                                            select ent).ToList();

        //        AddOrgNode(lsCompany, lsDepartment, parentItem);
        //    }
        //    allPositions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
        //    if (allPositions != null)
        //    {
        //        BindPosition();
        //    }
        //}

        //private void AddOrgNode(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany, List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        //{
        //    //绑定公司的子公司
        //    foreach (var childCompany in lsCompany)
        //    {
        //        TreeViewItem item = new TreeViewItem();
        //        item.Header = childCompany.CNAME;
        //        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //        item.DataContext = childCompany;
        //        //状态在未生效和撤消中时背景色为红色
        //        SolidColorBrush brush = new SolidColorBrush();
        //        if (childCompany.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //        {
        //            brush.Color = Colors.Red;
        //            item.Foreground = brush;
        //        }
        //        else
        //        {
        //            brush.Color = Colors.Black;
        //            item.Foreground = brush;
        //        }
        //        //标记为公司
        //        item.Tag = OrgTreeItemTypes.Company;
        //        FatherNode.Items.Add(item);

        //        if (lsCompany.Count() > 0)
        //        {
        //            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
        //                                                                          where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
        //                                                                          select ent).ToList();
        //            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
        //                                                                             where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
        //                                                                             select ent).ToList();

        //            AddOrgNode(lsTempCom, lsTempDep, item);
        //        }
        //    }
        //    //绑定公司下的部门
        //    foreach (var childDepartment in lsDepartment)
        //    {
        //        TreeViewItem item = new TreeViewItem();
        //        item.Header = childDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //        item.DataContext = childDepartment;
        //        item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
        //        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //        //状态在未生效和撤消中时背景色为红色
        //        SolidColorBrush brush = new SolidColorBrush();
        //        if (childDepartment.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //        {
        //            brush.Color = Colors.Red;
        //            item.Foreground = brush;
        //        }
        //        else
        //        {
        //            brush.Color = Colors.Black;
        //            item.Foreground = brush;
        //        }
        //        //标记为部门
        //        item.Tag = OrgTreeItemTypes.Department;
        //        FatherNode.Items.Add(item);

        //        if (lsDepartment.Count() > 0)
        //        {
        //            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
        //                                                                          where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
        //                                                                          select ent).ToList();
        //            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
        //                                                                             where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
        //                                                                             select ent).ToList();

        //            AddOrgNode(lsTempCom, lsTempDep, item);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 绑定岗位
        ///// </summary>
        //private void BindPosition()
        //{
        //    if (allPositions != null)
        //    {
        //        foreach (SMT.Saas.Tools.OrganizationWS.T_HR_POST tmpPosition in allPositions)
        //        {
        //            if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
        //                continue;
        //            TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Department, tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
        //            if (parentItem != null)
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
        //                item.DataContext = tmpPosition;
        //                item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //                //状态在未生效和撤消中时背景色为红色
        //                SolidColorBrush brush = new SolidColorBrush();
        //                if (tmpPosition.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //                {
        //                    brush.Color = Colors.Red;
        //                    item.Foreground = brush;
        //                }
        //                else
        //                {
        //                    brush.Color = Colors.Black;
        //                    item.Foreground = brush;
        //                }
        //                //标记为岗位
        //                item.Tag = OrgTreeItemTypes.Post;
        //                parentItem.Items.Add(item);
        //            }
        //        }
        //    }
        //    //树全部展开
        //    //  treeOrganization.ExpandAll();
        //    if (treeOrganization.Items.Count > 0)
        //    {
        //        TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
        //        selectedItem.IsSelected = true;
        //    }
        //}

        ///// <summary>
        ///// 获取节点
        ///// </summary>
        ///// <param name="parentType"></param>
        ///// <param name="parentID"></param>
        ///// <returns></returns>
        //private TreeViewItem GetParentItem(OrgTreeItemTypes parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    foreach (TreeViewItem item in treeOrganization.Items)
        //    {
        //        tmpItem = GetParentItemFromChild(item, parentType, parentID);
        //        if (tmpItem != null)
        //        {
        //            break;
        //        }
        //    }
        //    return tmpItem;
        //}

        //private TreeViewItem GetParentItemFromChild(TreeViewItem item, OrgTreeItemTypes parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    if (item.Tag != null && item.Tag.ToString() == parentType.ToString())
        //    {
        //        switch (parentType)
        //        {
        //            case OrgTreeItemTypes.Company:
        //                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
        //                if (tmpOrg != null)
        //                {
        //                    if (tmpOrg.COMPANYID == parentID)
        //                        return item;
        //                }
        //                break;
        //            case OrgTreeItemTypes.Department:
        //                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
        //                if (tmpDep != null)
        //                {
        //                    if (tmpDep.DEPARTMENTID == parentID)
        //                        return item;
        //                }
        //                break;
        //        }

        //    }
        //    if (item.Items != null && item.Items.Count > 0)
        //    {
        //        foreach (TreeViewItem childitem in item.Items)
        //        {
        //            tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
        //            if (tmpItem != null)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return tmpItem;
        //}


        #endregion
        //private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    LoadData();
        //}
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
            // orgClient.DoClose();
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
