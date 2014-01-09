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
using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.Common;
using System.Windows.Media.Imaging;
using System.IO;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class SalaryArchive : BasePage, IClient
    {
        //用于导出员工薪资档案变更记录
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;


        SMTLoading loadbar = new SMTLoading();
        private SalaryServiceClient client = new SalaryServiceClient();
        private string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        DataGrid DtGrid;
        bool flag = false;
        int recordcol = 1;
        PermissionServiceClient perclient;
        T_HR_SALARYSTANDARD selectedStandard = new T_HR_SALARYSTANDARD();
        List<V_SALARYARCHIVEITEM> listItems;
        ObservableCollection<string> archiveids;
        private string Checkstate { get; set; }

        //OrganizationServiceClient orgClient;
        //private List<T_HR_COMPANY> allCompanys;
        //private List<T_HR_DEPARTMENT> allDepartments;
        //private List<T_HR_POST> allPositions;
        private ObservableCollection<string> paymentIDs = new ObservableCollection<string>();
        public ObservableCollection<string> PaymentIDs
        {
            get { return paymentIDs; }
            set { paymentIDs = value; }
        }

        public SalaryArchive()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SalaryArchive_Loaded);

        }

        void SalaryArchive_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            GetEntityLogo("T_HR_SALARYARCHIVE");

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_SALARYARCHIVE", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            EditTitle("SALARY", "EMPLOYEESALARYINFORMATION");
        }

        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.SalaryArchiveDeleteCompleted += new EventHandler<SalaryArchiveDeleteCompletedEventArgs>(client_SalaryArchiveDeleteCompleted);
            client.GetSalaryArchivePagingCompleted += new EventHandler<GetSalaryArchivePagingCompletedEventArgs>(client_GetSalaryArchivePagingCompleted);
            client.ExportSalaryArchiveCompleted += new EventHandler<ExportSalaryArchiveCompletedEventArgs>(client_ExportSalaryArchiveCompleted);
            client.GetSalaryStandardPagingCompleted += new EventHandler<GetSalaryStandardPagingCompletedEventArgs>(client_GetSalaryStandardPagingCompleted);
            client.GetSalaryArchiveItemsByArchiveIDsCompleted += new EventHandler<GetSalaryArchiveItemsByArchiveIDsCompletedEventArgs>(client_GetSalaryArchiveItemsByArchiveIDsCompleted);
            //   client.GetCustomGuerdonArchiveWithPagingCompleted += new EventHandler<GetCustomGuerdonArchiveWithPagingCompletedEventArgs>(client_GetCustomGuerdonArchiveWithPagingCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            //this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());

            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            //orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
            //BindTree();
            ImageButton ChangeMeetingBtn = new ImageButton();
            ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_service.png", UriKind.Relative));
            ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("导出");// 导出员工薪资档案变更记录
            ChangeMeetingBtn.Image.Width = 16.0;
            ChangeMeetingBtn.Image.Height = 22.0;
            ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.Click += new RoutedEventHandler(btnExportArchive_Click);
            ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);
            SetToolBarBtn();
            SetOrgTree();
            loadbar.Stop();
        }

        void SetOrgTree()
        {
            switch (PermissionHelper.GetPermissionValue("T_HR_SALARYARCHIVE", Permissions.Browse))
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    SetTreeOrganization();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 根据权限设置隐藏组织架构树
        /// </summary>
        void SetTreeOrganization()
        {
            expander.Visibility = Visibility.Collapsed;
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.Approved).ToString());
            ToolBar.cbxCheckState.IsEnabled = false;
            ToolBar.btnRefresh.Visibility = Visibility.Collapsed;
            LayoutRoot.ColumnDefinitions.Clear();
            ColumnDefinition col1 = new ColumnDefinition();
            LayoutRoot.ColumnDefinitions.Add(col1);
            treeOrganization.Visibility = Visibility.Collapsed;
            DtGridStand.SetValue(Grid.ColumnProperty, 0);
            DtGridStand.SetValue(Grid.ColumnSpanProperty, 5);
            ToolBar.SetValue(Grid.ColumnSpanProperty, 5);
            DtGridStand.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        /// <summary>
        /// 获取当前登录人权限范围
        /// </summary>
        /// <param name="menuCode">菜单代码</param>
        /// <param name="perm">权限</param>
        /// <returns></returns>
        int GetPermissionDataRange(string menuCode, Permissions perm)
        {
            //return 1;
            // edit liujx  将rslt=0 改为rslt=-1 有集团的权限为0 ，为最大权限
            int rslt = -1;
            try
            {
                if (Common.CurrentLoginUserInfo != null)
                {
                    if (Common.CurrentLoginUserInfo.PermissionInfoUI != null)
                    {

                        int permvalue = Convert.ToInt32(perm);
                        var objs = from o in Common.CurrentLoginUserInfo.PermissionInfoUI
                                   where o.PermissionValue == Convert.ToInt32(permvalue).ToString()
                                   && o.MenuCode == menuCode
                                   select o;
                        //获取查询的权限,值越小，权限越大
                        if (objs == null || objs.Count() <= 0)
                        {
                            rslt = -1;
                        }
                        else
                        {
                            rslt = objs.Min(p => Convert.ToInt32(p.DataRange));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return rslt;
        }


        /// <summary>
        /// 根据权限控制ToolBar按钮显示
        /// </summary>
        void SetToolBarBtn()
        {
            if (PermissionHelper.GetPermissionValue("T_HR_SALARYARCHIVE", Permissions.Add) < 0)
            {
                ToolBar.btnNew.Visibility = Visibility.Collapsed;
            }
            if (PermissionHelper.GetPermissionValue("T_HR_SALARYARCHIVE", Permissions.Edit) < 0)
            {
                ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            }
            if (PermissionHelper.GetPermissionValue("T_HR_SALARYARCHIVE", Permissions.Delete) < 0)
            {
                ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            }
            if (PermissionHelper.GetPermissionValue("T_HR_SALARYARCHIVE", Permissions.Audit) < 0)
            {
                ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            }
            if (PermissionHelper.GetPermissionValue("T_HR_SALARYARCHIVE", Permissions.ReSubmit) < 0)
            {
                ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 导出员工薪资档案变更新记录
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        void btnExportArchive_Click(object sender, RoutedEventArgs e)
        {

            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;
            result = dialog.ShowDialog();
            if (result.Value == true)
            {
                string strEmployeeName = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty, strCheckState = string.Empty;
                //当前登录人ID
                strOwnerID = Common.CurrentLoginUserInfo.EmployeeID;
               //排序字段
                strSortKey = "EMPLOYEENAME,OTHERSUBJOIN,OTHERADDDEDUCT";
                loadbar.Start();
                string filter = string.Empty;
                int sType = 0;
                string sValue = string.Empty;

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
                    return;
                }
                TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
                if (txtName != null && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                    paras.Add(txtName.Text);
                }
                ComboBox cmbUsable = Utility.FindChildControl<ComboBox>(expander, "cmbUsable");
                //查询码0:有效 1：历史
                int queryCode = 0;
                string companyID = null;
                if (cmbUsable != null && (string)cmbUsable.SelectionBoxItem == "有效")
                {
                    queryCode = 0;
                    if (treeOrganization != null)
                    {
                        companyID = treeOrganization.sValue;
                    }
                }
                else
                {
                    queryCode = 1;
                    companyID = null;
                }
                client.ExportSalaryArchiveAsync(strSortKey, filter, paras, userID, Checkstate, sType, sValue, queryCode, companyID);
            }
        }



        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            LoadData();
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO: 重新提交审核 
            if (DtGrid != null)
            {
                if (DtGrid.SelectedItems.Count > 0)
                {
                    T_HR_SALARYARCHIVE tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYARCHIVE;
                    Form.Salary.SalaryArchiveForm form = new Form.Salary.SalaryArchiveForm(FormTypes.Resubmit, tmpEnt.SALARYARCHIVEID);
                    EntityBrowser browser = new EntityBrowser(form);

                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.FormType = FormTypes.Resubmit;
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                return;
            }

        }

        void client_GetSalaryArchiveItemsByArchiveIDsCompleted(object sender, GetSalaryArchiveItemsByArchiveIDsCompletedEventArgs e)
        {
            loadbar.Start();
            listItems = new List<V_SALARYARCHIVEITEM>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                try
                {
                    if (e.Result != null)
                    {
                        listItems = e.Result.ToList();
                        for (int i = 0; i < listItems.Count; i++)
                        {
                            listItems[i].SUM = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(listItems[i].SUM);
                        }
                        var ents = (from c in listItems
                                    select c).GroupBy(c => c.SALARYARCHIVEID).First().OrderBy(c => c.ORDERNUMBER);

                        foreach (var ent in ents)
                        {
                            DataGridTextColumn txtCol = new DataGridTextColumn();
                            txtCol.Header = ent.SALARYITEMNAME;
                            txtCol.Binding = new Binding("SUM");
                            //txtCol.Width = DataGridLength.SizeToCells;
                            //txtCol.MinWidth = 100;
                            //txtCol.MaxWidth = 100;
                            DtGrid.Columns.Add(txtCol);
                        }
                    }
                    //DtGrid.ItemsSource = list;
                    spDetail.Children.Clear();
                    DtGrid.CanUserSortColumns = false;
                    spDetail.Children.Add(DtGrid);

                    spDetail.Loaded += new RoutedEventHandler(SpStandDetail_Loaded);
                }
                catch { }
            }
        }
        void SpStandDetail_Loaded(object sender, RoutedEventArgs e)
        {
            BindData();
            loadbar.Stop();
        }
        void BindData()
        {
            try
            {
                List<V_SALARYARCHIVEITEM> list = new List<V_SALARYARCHIVEITEM>();
                if (DtGrid.ItemsSource != null)
                {
                    foreach (object obj in DtGrid.ItemsSource)
                    {
                        list = (from c in listItems
                                where c.SALARYARCHIVEID == (obj as T_HR_SALARYARCHIVE).SALARYARCHIVEID
                                select c).ToList();
                        if (list != null && list.Count > 0)
                        {
                            for (int i = recordcol; i < DtGrid.Columns.Count; i++)
                            {
                                string sn = DtGrid.Columns[i].Header.ToString();
                                V_SALARYARCHIVEITEM ent = list.Where(m => m.SALARYITEMNAME == sn).ToList().FirstOrDefault();
                                if (sn == "应发小计" || sn == "实发工资")
                                {
                                    DtGrid.Columns[i].Visibility = Visibility.Collapsed;
                                    continue;
                                }
                                DtGrid.Columns[i].GetCellContent(obj).DataContext = ent;
                            }
                        }
                    }
                }
                //spDetail.Children.Clear();
                //DtGrid.CanUserSortColumns = false;
                //spDetail.Children.Add(DtGrid);
            }
            catch { }
        }
        void client_GetSalaryStandardPagingCompleted(object sender, GetSalaryStandardPagingCompletedEventArgs e)
        {
            List<T_HR_SALARYSTANDARD> list = new List<T_HR_SALARYSTANDARD>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGridStand.ItemsSource = list;
                dataPagerStand.PageCount = e.pageCount;
                if (list.Count > 0)
                {
                    selectedStandard = list.FirstOrDefault() as T_HR_SALARYSTANDARD;
                    if (treeOrganization.Visibility == Visibility.Collapsed)
                    {
                        LoadDefaultData();
                    }
                    else
                    {
                        LoadData();
                    }
                }
                else
                    loadbar.Stop();

            }

        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid != null)
            {
                if (DtGrid.SelectedItems.Count > 0)
                {
                    T_HR_SALARYARCHIVE tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYARCHIVE;

                    Form.Salary.SalaryArchiveForm form = new SMT.HRM.UI.Form.Salary.SalaryArchiveForm(FormTypes.Browse, tmpEnt.SALARYARCHIVEID);
                    form.ToolBar.IsEnabled = false;
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Browse;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }


        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (flag == true)
            //{
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_SALARYARCHIVE");
                if (dict.DICTIONARYVALUE.Value.ToInt32() == Convert.ToInt32(CheckStates.Approved))
                {
                    ToolBar.btnReSubmit.Visibility = Visibility.Visible;
                    SetToolBarBtn();
                }
                else
                {
                    ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
                }
                if (treeOrganization.Visibility == Visibility.Collapsed)
                {
                    LoadDefaultData();
                }
                else
                {
                    LoadData();
                }
            }
            // }
            //ToolBar.btnAudit.Visibility = Visibility.Visible;
            ToolBar.btnNew.Visibility = Visibility.Visible;
            ToolBar.btnEdit.Visibility = Visibility.Visible;
            SetToolBarBtn();
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        /// <summary>
        /// 导出员工薪资档案变更记录完成事件
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        void client_ExportSalaryArchiveCompleted(object sender, ExportSalaryArchiveCompletedEventArgs e)
        {
            loadbar.Stop();
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
        }


        void client_GetSalaryArchivePagingCompleted(object sender, GetSalaryArchivePagingCompletedEventArgs e)
        {
            List<T_HR_SALARYARCHIVE> list = new List<T_HR_SALARYARCHIVE>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
            else
            {
                try
                {
                    if (e.Result != null)
                    {
                        list = e.Result.ToList();
                    }

                    spDetail.Children.Clear();
                    DtGrid = new DataGrid();
                    #region 设置当前加载DtGriddy样式
                    DtGrid.Style = Application.Current.Resources["DataGridStyle"] as Style;
                    DtGrid.CellStyle = Application.Current.Resources["DataGridCellStyle"] as Style;
                    DtGrid.RowHeaderStyle = Application.Current.Resources["DataGridRowHeaderStyle"] as Style;
                    DtGrid.RowStyle = Application.Current.Resources["DataGridRowStyle"] as Style;
                    DtGrid.VerticalAlignment = VerticalAlignment.Stretch;
                    DtGrid.ColumnHeaderStyle = Application.Current.Resources["DataGridColumnHeaderStyle"] as Style;
                    #endregion

                    DtGrid.AutoGenerateColumns = false;
                    DtGrid.IsReadOnly = true;

                    DataGridTextColumn txtCol1 = new DataGridTextColumn();
                    txtCol1.Header = Utility.GetResourceStr("EMPLOYEEID");
                    txtCol1.Binding = new Binding("EMPLOYEEID");
                    txtCol1.Visibility = Visibility.Collapsed;

                    DataGridTextColumn txtCol2 = new DataGridTextColumn();
                    txtCol2.Header = Utility.GetResourceStr("EMPLOYEECODE");
                    txtCol2.Binding = new Binding("EMPLOYEECODE");

                    DataGridTextColumn txtCol = new DataGridTextColumn();
                    txtCol.Header = Utility.GetResourceStr("EMPLOYEENAME");
                    txtCol.Binding = new Binding("EMPLOYEENAME");

                    DataGridTextColumn txtCol3 = new DataGridTextColumn();
                    txtCol3.Header = Utility.GetResourceStr("EFFECTDATE");
                    txtCol3.Binding = new Binding("OTHERADDDEDUCTDESC");

                    //txtCol.Width = DataGridLength.SizeToCells;
                    //txtCol.MinWidth = 100;
                    //txtCol.MaxWidth = 100;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].OTHERADDDEDUCTDESC = list[i].OTHERSUBJOIN + "年" + list[i].OTHERADDDEDUCT + "月";
                    }

                    DtGrid.Columns.Add(txtCol1);
                    DtGrid.Columns.Add(txtCol2);
                    DtGrid.Columns.Add(txtCol);
                    DtGrid.Columns.Add(txtCol3);
                    DtGrid.ItemsSource = list;
                    recordcol = DtGrid.Columns.Count;
                    dataPager.PageCount = e.pageCount;
                    archiveids = new ObservableCollection<string>();
                    foreach (var item in list)
                    {
                        archiveids.Add(item.SALARYARCHIVEID);
                    }
                    if (archiveids.Count > 0)
                    {
                        client.GetSalaryArchiveItemsByArchiveIDsAsync(archiveids);
                    }
                    else
                        loadbar.Stop();
                }
                catch { loadbar.Stop(); }
            }
            flag = true;
        }

        void client_SalaryArchiveDeleteCompleted(object sender, SalaryArchiveDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYARCHIVE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYARCHIVE"));
                LoadData();
            }
        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            if (treeOrganization.Visibility == Visibility.Collapsed)
            {
                LoadDefaultData();
            }
            else
            {
                LoadData();
            }
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            if (treeOrganization.Visibility == Visibility.Collapsed)
            {
                LoadDefaultData();
            }
            else
            {
                LoadData();
            }
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.SalaryArchiveForm form = new SMT.HRM.UI.Form.Salary.SalaryArchiveForm(FormTypes.New, "");

            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void browser_ReloadDataEvent()
        {
            if (treeOrganization.Visibility == Visibility.Collapsed)
            {
                LoadDefaultData();
            }
            else
            {
                LoadData();
            }
        }

        /// <summary>
        /// 根据组织架构加载数据
        /// </summary>
        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            int sType = 0;
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
                return;
            }


            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                //filter += "  EMPLOYEENAME==@" + paras.Count().ToString();
                //paras.Add(txtName.Text.Trim());
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";

                // filter += " EMPLOYEENAME.Contains(@" + paras.Count().ToString() + ")";
                paras.Add(txtName.Text);
            }

            //if (!string.IsNullOrEmpty(selectedStandard.SALARYSTANDARDID))
            //{
            //    if (!string.IsNullOrEmpty(filter)) filter += " AND ";
            //    filter += " @" + paras.Count().ToString() + ".Contains(T_HR_SALARYSTANDARD.SALARYSTANDARDID)";
            //    paras.Add(selectedStandard.SALARYSTANDARDID);
            //}
            //else
            //{
            //    loadbar.Stop();
            //    return ;
            //}
            //if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            //{
            //    strState = Checkstate;
            //}
            //client.GetSalaryArchivePagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYARCHIVEID", filter, paras, pageCount, userID, Checkstate, sType, sValue);
            // by luojie
            ComboBox cmbUsable = Utility.FindChildControl<ComboBox>(expander, "cmbUsable");
            int queryCode = 0;
            string companyID = null;
            //TextBlock textIsUsable = Utility.FindChildControl<TextBlock>(expander, "textIsUsable");
            if (cmbUsable != null && (string)cmbUsable.SelectionBoxItem == "有效")
            {
                queryCode = 0;
                if (treeOrganization != null)
                {
                    companyID = treeOrganization.sValue;
                    //textIsUsable.Visibility = Visibility.Visible;
                }
            }
            else
            {
                queryCode = 1;
                companyID = null;
                //textIsUsable.Visibility = Visibility.Collapsed;
            }
            client.GetSalaryArchivePagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYARCHIVEID", filter, paras,
                pageCount, userID, Checkstate, sType, sValue, queryCode, companyID);
            //loadbar.Stop();
        }

        /// <summary>
        /// 加载默认数据
        /// </summary>
        private void LoadDefaultData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            int sType = 4;
            string sValue = userID;

            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
            paras.Add(Common.CurrentLoginUserInfo.EmployeeName);
            int queryCode = 0;
            queryCode = 0;
            client.GetSalaryArchivePagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYARCHIVEID", filter, paras,
                pageCount, userID, "2", sType, sValue, queryCode, null);
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid != null)
            {
                if (DtGrid.SelectedItems.Count > 0)
                {
                    T_HR_SALARYARCHIVE tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYARCHIVE;

                    Form.Salary.SalaryArchiveForm form = new SMT.HRM.UI.Form.Salary.SalaryArchiveForm(FormTypes.Edit, tmpEnt.SALARYARCHIVEID);
                    //if (tmpEnt.CHECKSTATE == ((int)CheckStates.Approved).ToString()
                    //    || tmpEnt.CHECKSTATE == ((int)CheckStates.Approving).ToString())
                    //{
                    //    form.IsEnabled = false;
                    //}

                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid != null)
            {
                if (DtGrid.SelectedItems.Count > 0)
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();

                    foreach (T_HR_SALARYARCHIVE tmp in DtGrid.SelectedItems)
                    {
                        if (!(tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() || tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString()))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                            return;
                        }
                        if (tmp.CHECKSTATE != ((int)CheckStates.Approved).ToString()
                             && tmp.CHECKSTATE != ((int)CheckStates.Approving).ToString())
                        {
                            ids.Add(tmp.SALARYARCHIVEID);
                        }

                    }

                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        client.SalaryArchiveDeleteAsync(ids);
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                return;
            }
        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核  
            if (DtGrid != null)
            {
                if (DtGrid.SelectedItems.Count > 0)
                {
                    T_HR_SALARYARCHIVE tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYARCHIVE;
                    Form.Salary.SalaryArchiveForm form = new Form.Salary.SalaryArchiveForm(FormTypes.Audit, tmpEnt.SALARYARCHIVEID);
                    EntityBrowser browser = new EntityBrowser(form);

                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.FormType = FormTypes.Audit;
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                return;
            }
        }
        #endregion


        /// <summary>
        ///  载入自定义薪资档案
        /// </summary>
        //void LoadCustomArchive(string salaryArchiveID)
        //{
        //    int pageCount = 0;
        //    string filter = "";
        //    System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();


        //    filter += " SALARYARCHIVEID==@" + paras.Count().ToString();
        //    paras.Add(salaryArchiveID);

        //    client.GetCustomGuerdonArchiveWithPagingAsync(dataPagerDetail.PageIndex, dataPagerDetail.PageSize, " GUERDONNAME", filter, paras, pageCount);
        //}

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYARCHIVE archive = DtGrid.SelectedItems[0] as T_HR_SALARYARCHIVE;
                //  LoadCustomArchive(archive.SALARYARCHIVEID);
                //  salarySolutionName.Text = "  " + archive.EMPLOYEENAME + "薪资档案-自定义薪资项";
            }
        }

        private void GridPagerDetail_Click(object sender, RoutedEventArgs e)
        {
            //   LoadCustomArchive(((T_HR_SALARYARCHIVE)DtGrid.SelectedItems[0]).SALARYARCHIVEID);
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //  SetRowLogo(DtGrid, e.Row, "T_HR_SALARYARCHIVE");
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (treeOrganization.Visibility == Visibility.Collapsed)
            {
                LoadDefaultData();
            }
            else
            {
                LoadData();
            }
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //LoadData();
            //LoadStander();
        }

        #region
        private void DtGridStand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (DtGridStand.SelectedItems.Count > 0)
            {
                selectedStandard = DtGridStand.SelectedItems[0] as T_HR_SALARYSTANDARD;
                if (treeOrganization.Visibility == Visibility.Collapsed)
                {
                    LoadDefaultData();
                }
                else
                {
                    LoadData();
                }
            }
        }
        void LoadStander()
        {
            spDetail.Children.Clear();
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            string strState = Convert.ToInt32(CheckStates.Approved).ToString();
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            int sType = 0;
            string sValue = "";
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

            client.GetSalaryStandardPagingAsync(dataPagerStand.PageIndex, dataPagerStand.PageSize, "SALARYSTANDARDNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, sValue, strState);

        }
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

        private void GridPagerStand_Click(object sender, RoutedEventArgs e)
        {
            LoadStander();
        }

        private void EditTitle(string parentTitle, string newTitle)
        {
            System.Text.StringBuilder sbtitle = new System.Text.StringBuilder();
            sbtitle.Append(Utility.GetResourceStr(parentTitle));
            sbtitle.Append(">>");
            sbtitle.Append(Utility.GetResourceStr(newTitle));
            ViewTitles.TextTitle.Text = sbtitle.ToString();
        }

        private void Login_HandlerClick(object sender, EventArgs e)
        {
            LayoutRoot.Visibility = Visibility.Visible;
            LoadDefaultData();
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {

        }


    }
}
