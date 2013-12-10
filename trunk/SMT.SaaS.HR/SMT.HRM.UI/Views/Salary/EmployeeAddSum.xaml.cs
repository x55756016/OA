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

using System.Windows.Data;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

using SMT.SaaS.Globalization;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.HRM.UI.Form.Salary;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class EmployeeAddSum : BasePage, IClient
    {
        private SalaryServiceClient client = new SalaryServiceClient();
        //OrganizationServiceClient orgClient;
        //private List<T_HR_COMPANY> allCompanys;
        //private List<T_HR_DEPARTMENT> allDepartments;
        //private List<T_HR_POST> allPositions;
        private string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        public EmployeeAddSum()
        {
            InitializeComponent();
            //InitParas();
            //GetEntityLogo("T_HR_EMPLOYEEADDSUM");
            this.Loaded += new RoutedEventHandler(EmployeeAddSum_Loaded);
        }

        void EmployeeAddSum_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            GetEntityLogo("T_HR_EMPLOYEEADDSUM");
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEEADDSUM", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }


        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            client.EmployeeAddSumDeleteCompleted += new EventHandler<EmployeeAddSumDeleteCompletedEventArgs>(client_EmployeeAddSumDeleteCompleted);
            client.GetEmployeeAddSumWithPagingCompleted += new EventHandler<GetEmployeeAddSumWithPagingCompletedEventArgs>(client_GetEmployeeAddSumWithPagingCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("MASSAUDIT")).Click += new RoutedEventHandler(MassAudit_Click);

            ImageButton _ImgButtonBankPaySalary = new ImageButton();
            _ImgButtonBankPaySalary.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonBankPaySalary.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("test")).Click += new RoutedEventHandler(_ImgButtonBankPaySalary_Click);
            //ToolBar.stpOtherAction.Children.Add(_ImgButtonBankPaySalary);

            //ToolBar.btnAduitNoTPass.Click += new RoutedEventHandler(btnAuitNoTPass_click);
            //ToolBar.btnSumbitAudit.Click += new RoutedEventHandler(btnSumbitAudit_click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            //this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            this.Loaded += new RoutedEventHandler(Left_Loaded);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            //航信版本发布时需要打开
            ToolBar.btnImport.Visibility = Visibility.Visible;
            ToolBar.btnImport.Click += new RoutedEventHandler(btnImport_Click);

            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            //orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
        }

        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            EmployeeAddImportForm form = new EmployeeAddImportForm();
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 260;
            form.MinWidth = 400;


            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
       
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            LoadData();
        }

        void _ImgButtonBankPaySalary_Click(object sender, RoutedEventArgs e)
        {
            //Utility.CreateFormFromEngine("a7f35e92-181a-4e5f-9a0a-639ab5f3d73d", "SMT.HRM.UI.Views.Salary.Payment", "Audit");
            //("37BA4B12-E51B-4DFD-89C2-C809DAC097DFggdg", "SMT.HRM.UI.Views.Salary.Payment", "Audit");
            //("24ac690d-ca11-443e-85b6-28f2adcb998d", "SMT.HRM.UI.Form.Salary.EmployeeAddSumMassAudit", "Audit");
            //("24ac690d-ca11-443e-85b6-28f2adcb998d", "SMT.HRM.UI.Form.Salary.EmployeeAddSumMassAudit", "Audit");
        }

        public void MassAudit_Click(object o, RoutedEventArgs e)
        {
            try
            {
                ///TODO:ADD 审核   
                if (DtGrid.SelectedItems.Count > 0)
                {
                    T_HR_EMPLOYEEADDSUM tmpEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEEADDSUM;
                    Form.Salary.EmployeeAddSumForm form = new Form.Salary.EmployeeAddSumForm(FormTypes.Audit, tmpEnt.ADDSUMID);
                    EntityBrowser browser = new EntityBrowser(form);
                    form.MinWidth = 570;
                    form.MinHeight = 240;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.FormType = FormTypes.Audit;
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    int sType = 0;
                    string sValue = "";
                    string state = "";
                    string strCollection = string.Empty;
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
                        return;
                    }


                    // state = CheckStates.UnSubmit.ToInt32().ToString(); //注释掉是因为在审核中取不到数据  liujx
                    state = Checkstate;//批量审核也可以根据状态进行加载
                    
                    
                    NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
                    NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
                    Form.Salary.EmployeeAddSumMassAudit form = new Form.Salary.EmployeeAddSumMassAudit(FormTypes.Audit, sType, sValue, nuYear.Value.ToString(), nuStartmounth.Value.ToString(), state);

                    //Form.Salary.EmployeeAddSumMassAudit form = new Form.Salary.EmployeeAddSumMassAudit(FormTypes.Audit, "919a594b-77fc-49e1-a024-a6f9b2009d5b");
                    //Form.Salary.EmployeeAddSumMassAudit form = new Form.Salary.EmployeeAddSumMassAudit();
                    //strCollection = sType.ToString() + "," + sValue + "," + nuYear.Value.ToString() + "," + nuStartmounth.Value.ToString() + "," + state;
                    //Form.Salary.EmployeeAddSumMassAudit form = new Form.Salary.EmployeeAddSumMassAudit(FormTypes.Audit, strCollection);  

                    EntityBrowser browser = new EntityBrowser(form);
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.FormType = FormTypes.Audit;
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO: 重新提交审核
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEEADDSUM tmpEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEEADDSUM;
                Form.Salary.EmployeeAddSumForm form = new Form.Salary.EmployeeAddSumForm(FormTypes.Resubmit, tmpEnt.ADDSUMID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 600;
                form.MinHeight = 240;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                return;
            }
        }

        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            // BindTree();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEEADDSUM tmpEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEEADDSUM;

                Form.Salary.EmployeeAddSumForm form = new SMT.HRM.UI.Form.Salary.EmployeeAddSumForm(FormTypes.Browse, tmpEnt.ADDSUMID);
                form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinWidth = 600;
                form.MinHeight = 240;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("MASSAUDIT")).Visibility = Visibility.Visible;
                if (Checkstate == Convert.ToString(Convert.ToInt32(CheckStates.Approving)))//审核中则隐藏
                {
                    ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("MASSAUDIT")).Visibility = Visibility.Collapsed;
                }
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_EMPLOYEEADDSUM");
                LoadData();
            }
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }
        void client_GetEmployeeAddSumWithPagingCompleted(object sender, GetEmployeeAddSumWithPagingCompletedEventArgs e)
        {
            List<T_HR_EMPLOYEEADDSUM> list = new List<T_HR_EMPLOYEEADDSUM>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    foreach (var item in list)
                    {
                        item.DEALYEAR = item.DEALYEAR + "年" + item.DEALMONTH + "月";
                    }
                }
                DtGrid.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }


            loadbar.Stop();
        }

        void client_EmployeeAddSumDeleteCompleted(object sender, EmployeeAddSumDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEEADDSUM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEEADDSUM"));
                LoadData();
            }
        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {

            Form.Salary.EmployeeAddSumForm form = new SMT.HRM.UI.Form.Salary.EmployeeAddSumForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 600;
            form.MinHeight = 240;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }


        private void LoadData()
        {
            loadbar.Start();

            int pageCount = 0;
            string filter = "";
            int sType = 0;
            string sValue = "";
            string strState = "";
            DateTime? starttimes;
            DateTime? endtimes = DateTime.Now.Date;
            if (DateTime.Now.Month <= 2)
                starttimes = new DateTime(DateTime.Now.Year - 1, 1, 1);
            else
                starttimes = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 2, 1);
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
            NumericUpDown nuEndYear = Utility.FindChildControl<NumericUpDown>(expander, "NuEndyear");
            NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
            NumericUpDown nuEndmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuEndmounth");

            #region 多月份的过滤
            try
            {
                starttimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), 1);
                if (nuYear.Value > nuEndYear.Value)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始年份不能大于结束年份"));
                    loadbar.Stop();
                    return;
                }
                if ((nuYear.Value == nuEndYear.Value) && (nuStartmounth.Value > nuEndmounth.Value))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始月份不能小于结束月份"));
                    loadbar.Stop();
                    return;
                }
                if (nuEndmounth.Value.ToInt32() == 12) endtimes = new DateTime(nuEndYear.Value.ToInt32() + 1, 1, 1); else endtimes = new DateTime(nuEndYear.Value.ToInt32(), nuEndmounth.Value.ToInt32(), 1);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            #endregion

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
                return;
            }

            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                // filter += " @" + paras.Count().ToString() + ".Contains(SALARYSOLUTIONNAME)";
                //filter += " EMPLOYEENAME==@" + paras.Count().ToString();
                //filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                filter += " EMPLOYEENAME.Contains(@" + paras.Count().ToString() + ")";
                paras.Add(txtName.Text.Trim());
            }

            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }
            client.GetEmployeeAddSumWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "ADDSUMID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState, sType, sValue);

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEEADDSUM tmpEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEEADDSUM;

                if (tmpEnt.CHECKSTATE == "0" || tmpEnt.CHECKSTATE == "3")
                {

                    Form.Salary.EmployeeAddSumForm form = new SMT.HRM.UI.Form.Salary.EmployeeAddSumForm(FormTypes.Edit, tmpEnt.ADDSUMID);

                    EntityBrowser browser = new EntityBrowser(form);
                    form.MinWidth = 600;
                    form.MinHeight = 240;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.FormType = FormTypes.Edit;
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTEDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }

        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            string Result = "";
            ObservableCollection<string> ids = new ObservableCollection<string>();
            if (DtGrid.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DtGrid.SelectedItems.Count; i++)
                {
                    T_HR_EMPLOYEEADDSUM tmpEnt = DtGrid.SelectedItems[i] as T_HR_EMPLOYEEADDSUM;
                    ids.Add(tmpEnt.ADDSUMID);
                    if (!(tmpEnt.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() || tmpEnt.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString()))
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                        return;
                    }
                }
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.EmployeeAddSumDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                return;
            }
        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///TODO:ADD 审核   
                if (DtGrid.SelectedItems.Count > 0)
                {
                    T_HR_EMPLOYEEADDSUM tmpEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEEADDSUM;
                    Form.Salary.EmployeeAddSumForm form = new Form.Salary.EmployeeAddSumForm(FormTypes.Audit, tmpEnt.ADDSUMID);
                    EntityBrowser browser = new EntityBrowser(form);
                    form.MinWidth = 600;
                    form.MinHeight = 240;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.FormType = FormTypes.Audit;
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    int sType = 0;
                    string sValue = "";
                    string state = "";
                    //TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
                    //if (selectedItem != null)
                    //{
                    //    string IsTag = selectedItem.Tag.ToString();
                    //    switch (IsTag)
                    //    {
                    //        case "0":
                    //            OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
                    //            sType = 0;
                    //            sValue = company.COMPANYID;
                    //            break;
                    //        case "1":
                    //            OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as OrganizationWS.T_HR_DEPARTMENT;
                    //            sType = 1;
                    //            sValue = department.DEPARTMENTID;
                    //            break;
                    //        case "2":
                    //            OrganizationWS.T_HR_POST post = selectedItem.DataContext as OrganizationWS.T_HR_POST;
                    //            sType = 2;
                    //            sValue = post.POSTID;
                    //            break;
                    //    }
                    //}
                    //else
                    //{
                    //    return;
                    //}
                    string selectedtype = treeOrganization.sType;
                    if (!string.IsNullOrEmpty(selectedtype))
                    {
                        switch (selectedtype)
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
                        return;
                    }
                    if (Checkstate != CheckStates.All.ToInt32().ToString())
                    {
                        state = Checkstate;
                    }
                    else
                    {
                        state = CheckStates.UnSubmit.ToInt32().ToString();
                    }
                    NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
                    NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
                    Form.Salary.EmployeeAddSumMassAudit form = new Form.Salary.EmployeeAddSumMassAudit(FormTypes.Audit, sType, sValue, nuYear.Value.ToString(), nuStartmounth.Value.ToString(), state);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.FormType = FormTypes.Audit;
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }

        }

        void btnSumbitAudit_click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 提交审核            
        }
        void btnAuitNoTPass_click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核不通过          
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_EMPLOYEEADDSUM");
        }

        private void Nuyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Year.ToDouble();
        }

        private void NuStartmounth_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nustartmonuth = (NumericUpDown)sender;
            nustartmonuth.Value = DateTime.Now.Month.ToDouble();
        }

        private void NuEndmounth_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuendmonuth = (NumericUpDown)sender;
            nuendmonuth.Value = DateTime.Now.Month.ToDouble();
        }

        private void NuEndyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuEndyear = (NumericUpDown)sender;
            nuEndyear.Value = DateTime.Now.Year.ToDouble();
        }


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
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
        //            item.Tag = OrgTreeItemTypes.Company.ToInt32();
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
        //                item.Tag = OrgTreeItemTypes.Company.ToInt32();
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
        //        item.Tag = OrgTreeItemTypes.Company.ToInt32();
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
        //        item.Tag = OrgTreeItemTypes.Department.ToInt32();
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
        //                item.Tag = OrgTreeItemTypes.Post.ToInt32();
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
        //    if (item.Tag != null && item.Tag.ToString() == parentType.ToInt32().ToString())
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


        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
            //  orgClient.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        //private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    LoadData();
        //}

    }
}
