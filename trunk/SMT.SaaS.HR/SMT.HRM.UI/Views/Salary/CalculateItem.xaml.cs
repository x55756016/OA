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

using SMT.HRM.UI.Form.Salary;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;

using System.Windows.Data;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;

using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class CalculateItem : BasePage, IClient
    {
        SalaryServiceClient client;
        SMTLoading loadbar = new SMTLoading();
        //OrganizationServiceClient orgClient;
        //private List<T_HR_COMPANY> allCompanys;
        //private List<T_HR_DEPARTMENT> allDepartments;
        private string Checkstate { get; set; }

        public CalculateItem()
        {
            InitializeComponent();
            //InitPara();
            //GetEntityLogo("T_HR_SALARYITEM");
            //LoadData();
            this.Loaded += new RoutedEventHandler(CalculateItem_Loaded);
        }

        void CalculateItem_Loaded(object sender, RoutedEventArgs e)
        {
            InitPara();
            GetEntityLogo("T_HR_SALARYITEM");
            LoadData();
        }

        public void InitPara()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            try
            {
                client = new SalaryServiceClient();
                client.GetSalaryItemSetPagingCompleted += new EventHandler<GetSalaryItemSetPagingCompletedEventArgs>(client_GetSalaryItemSetPagingCompleted);
                client.SalaryItemSetDeleteCompleted += new EventHandler<SalaryItemSetDeleteCompletedEventArgs>(client_SalaryItemSetDeleteCompleted);

                client.ExecuteSalaryItemSqlCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_ExecuteSalaryItemSqlCompleted);
                this.Loaded += new RoutedEventHandler(Left_Loaded);

                // orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
                //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);

            //xiedx
            //自定义的一个按钮
            ImageButton _ImgButtonExecuteSalaryItemSql = new ImageButton();
            _ImgButtonExecuteSalaryItemSql.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonExecuteSalaryItemSql.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png",
                Utility.GetResourceStr("初始化薪资")).Click += new RoutedEventHandler(_ImgButtonExecuteSalaryItemSql_Click);
            ToolBar.stpOtherAction.Children.Add(_ImgButtonExecuteSalaryItemSql);

            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);

            ImageButton _ImgButtonRecoverySalaryItem = new ImageButton();
            _ImgButtonRecoverySalaryItem.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("RECOVERYSALARY")).Click += new RoutedEventHandler(_ImgButtonRecoverySalaryItem_Click);
            _ImgButtonRecoverySalaryItem.Visibility = Visibility.Collapsed;
            ToolBar.stpOtherAction.Children.Add(_ImgButtonRecoverySalaryItem);
        }

        //初始化薪资完成事件
        void client_ExecuteSalaryItemSqlCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
           
                if (e.Error != null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CREATESUCCESS", "SALARYITEM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYITEM"));
                    LoadData();
                }
            
           
        }
        
      
        /// <summary>
        /// 2012-8-24
        /// xiedx
        /// 初始化按钮
        /// </summary>
        void _ImgButtonExecuteSalaryItemSql_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            ComfirmWindow com = new ComfirmWindow();
            T_HR_SALARYITEM temp = new T_HR_SALARYITEM();
            temp.SALARYITEMID = Guid.NewGuid().ToString();
            temp.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            temp.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            temp.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            temp.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            temp.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
          
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();
                client.ExecuteSalaryItemSqlAsync(temp);
            };

            com.SelectionBox(Utility.GetResourceStr("CAUTION"), "该选项会生成新的薪资，请向相关部门确认先", ComfirmWindow.titlename, Result);
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            LoadData();
        }

        void _ImgButtonRecoverySalaryItem_Click(object sender, RoutedEventArgs e)
        {
            Formulatemplate form = new Formulatemplate();
            EntityBrowser browser = new EntityBrowser(form);
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            form.MinWidth = 700;
            form.MinHeight = 280;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            //  BindTree();
        }

        void client_SalaryItemSetDeleteCompleted(object sender, SalaryItemSetDeleteCompletedEventArgs e)
        {
            try
            {
                if (e.Result == false)
                {
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "该薪资项正在使用中，不能删除");
                }
                if (e.Error != null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYITEM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYITEM"));
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_GetSalaryItemSetPagingCompleted(object sender, GetSalaryItemSetPagingCompletedEventArgs e)
        {
            List<T_HR_SALARYITEM> list = new List<T_HR_SALARYITEM>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
            loadbar.Stop();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYITEM tmp = DtGrid.SelectedItems[0] as T_HR_SALARYITEM;

                //if (customSalaryIDs.Count <= 0)
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                //    return;
                //}
                CalculateItemForm form = new CalculateItemForm(FormTypes.Browse, tmp.SALARYITEMID);
                form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 700;
                form.MinHeight = 280;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }


        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选择某审核状态是重新加载数据

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_CUSTOMGUERDONRECORD");
                Checkstate = dict.DICTIONARYVALUE.ToString();
                LoadData();
            }
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.Approved).ToString());
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_SALARYITEM", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            ToolBar.btnEdit.Visibility = Visibility.Visible;
            ToolBar.btnNew.Visibility = Visibility.Visible;
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //customSalaryIDs.Clear();
            //DataGrid grid = sender as DataGrid;
            //if (grid.SelectedItem != null)
            //{
            //    SelectID = grid.SelectedItems[0] as T_HR_SALARYITEM;
            //    customSalaryIDs.Add((grid.SelectedItems[0] as T_HR_SALARYITEM).CUSTOMGUERDONSETID);
            //}

        }

        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            CalculateItemForm form = new CalculateItemForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            form.MinWidth = 700;
            form.MinHeight = 280;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }


        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            string sValue = "";
            loadbar.Start();
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            string strState = "";
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                //filter += "SALARYITEMNAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(SALARYITEMNAME)";
                paras.Add(txtName.Text.Trim());
            }

            //TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            //if (selectedItem != null)
            //{
            //    OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
            //    sValue = company.COMPANYID;
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "CREATECOMPANYID==@" + paras.Count().ToString();
            //    paras.Add(sValue);
            //}
            //else
            //{
            //    loadbar.Stop();
            //    return;
            //}

            string selectedType = treeOrganization.sType;
            if (!string.IsNullOrEmpty(selectedType) && selectedType == "Company")
            {

                sValue = treeOrganization.sValue;
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CREATECOMPANYID==@" + paras.Count().ToString();
                paras.Add(sValue);
            }
            else
            {
                loadbar.Stop();
                return;
            }
            #region --
            //if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "CHECKSTATE==@" + paras.Count().ToString();
            //    paras.Add(Checkstate);
            //}
            #endregion

            client.GetSalaryItemSetPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYITEM tmpStandard = DtGrid.SelectedItems[0] as T_HR_SALARYITEM;

                if (tmpStandard == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                    return;
                }
                CalculateItemForm form = new CalculateItemForm(FormTypes.Edit, tmpStandard.SALARYITEMID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 700;
                form.MinHeight = 280;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //ComfirmBox deleComfir = new ComfirmBox();
            //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
            //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            //deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK1_Click);
            //deleComfir.Show();

            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_SALARYITEM tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.SALARYITEMID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.SalaryItemSetDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }

        }

        void ButtonOK1_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_SALARYITEM tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.SALARYITEMID);
                }
                client.SalaryItemSetDeleteAsync(ids);
            }

        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核     
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYITEM tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYITEM;
                CalculateItemForm form = new Form.Salary.CalculateItemForm(FormTypes.Audit, tmpEnt.SALARYITEMID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 700;
                form.MinHeight = 280;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
            }
        }
        #endregion

        #region 公司树形控件的操作  NEW缓存
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

        //        //orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentConfig.CurrentUser.UserInfo.EMPLOYEEID);

        //    }
        //}

        //void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        //{

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
        //        // 标记为公司
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


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_SALARYITEM");
        }

        //private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    LoadData();
        //}

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
    }
}
