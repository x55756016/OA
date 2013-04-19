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

namespace SMT.HRM.UI.Views.Salary
{
    public partial class SalaryStandard : BasePage,IClient
    {
        string flag = "1";
        private int num = 0;
        private int recordPoint = 0;
        List<V_SALARYSTANDARDITEM> items = new List<V_SALARYSTANDARDITEM>();
        List<T_HR_SALARYSTANDARD> std = new List<T_HR_SALARYSTANDARD>();
        private SalaryServiceClient client = new SalaryServiceClient();
        private string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient orgClient;
        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;
        private bool sign = false;
        private int recordnum = 0;
        List<string> getItemID = new List<string>();
        List<object> itemsAll = new List<object>();
        //List<T_HR_EMPLOYEESALARYRECORD> list = new List<T_HR_EMPLOYEESALARYRECORD>();
        T_HR_SALARYSTANDARD selectedStand = new T_HR_SALARYSTANDARD();
        private ObservableCollection<string> paymentIDs = new ObservableCollection<string>();
        public ObservableCollection<string> PaymentIDs
        {
            get { return paymentIDs; }
            set { paymentIDs = value; }
        }
        public SalaryStandard()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SalaryStandard_Loaded);           
        }

        void SalaryStandard_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            GetEntityLogo("T_HR_SALARYSTANDARD");
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
         //   Utility.DisplayGridToolBarButton(ToolBar, "T_HR_SALARYSTANDARD", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());

        }
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGridStand, e.Row, "T_HR_SALARYSTANDARD");
        }

        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.SalaryStandardDeleteCompleted += new EventHandler<SalaryStandardDeleteCompletedEventArgs>(client_SalaryStandardDeleteCompleted);
            client.GetSalaryStandardPagingCompleted += new EventHandler<GetSalaryStandardPagingCompletedEventArgs>(client_GetSalaryStandardPagingCompleted);
            client.GetSalaryStandardItemsViewByStandarIDCompleted += new EventHandler<GetSalaryStandardItemsViewByStandarIDCompletedEventArgs>(client_GetSalaryStandardItemsViewByStandarIDCompleted);
            
            #region 工具栏
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            //this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            #endregion

            orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
            BindTree();
        }

        void client_GetSalaryStandardItemsViewByStandarIDCompleted(object sender, GetSalaryStandardItemsViewByStandarIDCompletedEventArgs e)
        {
            List<V_SALARYSTANDARDITEM> its = new List<V_SALARYSTANDARDITEM>();
            List<V_SALARYSTANDARDITEM> items = new List<V_SALARYSTANDARDITEM>();
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        //its = e.Result.ToList();
                        items = e.Result.OrderBy(c => c.ORDERNUMBER).ToList();
                        if (sign)
                        {
                            foreach (var AA in items)
                            {
                                DataGridTextColumn txtCol = new DataGridTextColumn();

                                txtCol.Header = AA.SALARYITEMNAME;
                                txtCol.Binding = new Binding("SUM");
                                txtCol.Width = DataGridLength.SizeToCells;
                                txtCol.MinWidth = 100;
                                txtCol.MaxWidth = 100;
                                DtGrid.Columns.Add(txtCol);
                                getItemID.Add(AA.STANDRECORDITEMID);
                            }
                            sign = false;
                            //items = its;
                        }

                    }
                }
                itemsAll.Add(items);
                ++recordnum;
                if (recordnum < std.Count)
                {
                    for (int i = recordnum; i < std.Count; i++)
                    {
                        try
                        {
                            client.GetSalaryStandardItemsViewByStandarIDAsync(std[recordnum].SALARYSTANDARDID);
                            break;
                        }
                        catch (Exception ex)
                        {
                            ex.Message.ToString();
                        }
                    }
                }
                else
                {
                    int i = 0;
                    foreach (List<V_SALARYSTANDARDITEM> a in itemsAll)
                    {
                        i += a.Count;
                    }
                    if (i == 0) SpStandDetail.Children.Add(DtGrid);
                    else
                    {
                        SpStandDetail.Children.Add(DtGrid);
                        SpStandDetail.Loaded += new RoutedEventHandler(SpSalaryRecord_Loaded);
                    }
                }
            }
            catch (Exception exx)
            {
                exx.Message.ToString();
            }
            if (recordnum > std.Count) loadbar.Stop();
        }

        void SpSalaryRecord_Loaded(object sender, RoutedEventArgs e)
        {
            GetBindContent();
            loadbar.Stop();
        }

        private void GetBindContent()
        {
            try
            {
                recordnum = 0;
                if (DtGrid.ItemsSource != null)
                {
                    foreach (object obj in DtGrid.ItemsSource)
                    {
                        List<V_SALARYSTANDARDITEM> q = (List<V_SALARYSTANDARDITEM>)itemsAll[recordnum];
                        if (q.Count > 0)
                        {
                            for (int i = recordPoint; i < DtGrid.Columns.Count; i++)
                            {
                                try
                                {
                                    DtGrid.Columns[i].GetCellContent(obj).DataContext = (itemsAll[recordnum] as List<V_SALARYSTANDARDITEM>)[i - recordPoint];
                                }
                                catch { }
                            }
                        }
                        recordnum++;
                    }
                }
            }
            catch { }

        }

        //void client_GetSalaryStandardItemsViewByStandarIDCompleted(object sender, GetSalaryStandardItemsViewByStandarIDCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            items = e.Result.OrderBy(c=>c.ORDERNUMBER).ToList();
        //            //if (num <= 0)
        //            //{
        //                foreach (var AA in items)
        //                {
        //                    DataGridTextColumn txtCol = new DataGridTextColumn();
        //                    txtCol.Header = AA.SALARYITEMNAME;
        //                    txtCol.Binding = new Binding("SUM");
        //                    //txtCol.Width = DataGridLength.SizeToCells;
        //                    //txtCol.MinWidth = 100;
        //                    //txtCol.MaxWidth = 100;
        //                    DtGrid.Columns.Add(txtCol);
        //                }
        //            //}

        //        }

        //        //SpStandDetail.Children.Add(DtGrid);
        //        //SpStandDetail.Loaded += new RoutedEventHandler(SpStandDetail_Loaded);
        //        //num--;
        //        //if (num > 0)
        //        //    client.GetSalaryStandardItemsViewByStandarIDAsync(std[std.Count-num].SALARYSTANDARDID);
        //    }
        //}



        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                selectedStand = DtGrid.SelectedItems[0] as T_HR_SALARYSTANDARD;
                Form.Salary.SalaryStandardForm form = new SMT.HRM.UI.Form.Salary.SalaryStandardForm(FormTypes.Browse, selectedStand.SALARYSTANDARDID);
                //form.IsEnabled = false;
                form.CustomGuerdonWinForm.ToolBar.IsEnabled = false;
                form.StandardItemWinForm.ToolBar.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinWidth = 780;
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

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选择某审核状态是重新加载数据

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
              //  Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_SALARYSTANDARD");
                LoadStander();
            }
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }
        void client_GetSalaryStandardPagingCompleted(object sender, GetSalaryStandardPagingCompletedEventArgs e)
        {
            List<T_HR_SALARYSTANDARD> list = new List<T_HR_SALARYSTANDARD>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                loadbar.Stop();
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    std = e.Result.ToList();
                    num = std.Count;
                }

                DtGridStand.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
                if (list.Count > 0)
                {
                    //selectedStand = list.FirstOrDefault() as T_HR_SALARYSTANDARD;
                    //foreach(var li in list)
                    //{
                    //     //selectedStand = li as T_HR_SALARYSTANDARD;
                    //    LoadData();
                    //}

                    LoadData();
                    
                }
                else
                    loadbar.Stop();

            }

        }

        void client_SalaryStandardDeleteCompleted(object sender, SalaryStandardDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SalaryStandard"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SalaryStandard"));
                // LoadData();
                LoadStander();
            }
        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            // LoadData();
            LoadStander();
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            // LoadData();
            LoadStander();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }


        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.SalaryStandardForm form = new SMT.HRM.UI.Form.Salary.SalaryStandardForm(FormTypes.New, "");

            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 780;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void browser_ReloadDataEvent()
        {
            //LoadData();

        }

        private DataGrid DtGrid;

        //private void LoadData()
        //{
        //    SpStandDetail.Children.Clear();
        //    loadbar.Start();
        //    int pageCount = 0;
        //    string filter = "";
        //    string strState = "";
        //    System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

        //    TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
        //    if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
        //    {
        //        //filter += " c.SALARYSTANDARDNAME==@" + paras.Count().ToString();
        //        filter += " @" + paras.Count().ToString() + ".Contains(SALARYSTANDARDNAME)";
        //        //filter += " SALARYSTANDARDNAME.Contains(@" + paras.Count().ToString() + ")";
        //        paras.Add(txtName.Text.Trim());
        //    }
        //    if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
        //    {
        //        strState = Checkstate;
        //    }


        //    int sType = 0;
        //    string sValue = "";
        //    TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
        //    if (selectedItem != null)
        //    {
        //        string IsTag = selectedItem.Tag.ToString();
        //        switch (IsTag)
        //        {
        //            case "0":
        //                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
        //                sType = 0;
        //                sValue = company.COMPANYID;
        //                //if (paras.Count > 0)
        //                //{
        //                //    filter += " and ";
        //                //}
        //                //filter += "COMPANYID==@" + paras.Count().ToString();
        //                //paras.Add(sValue);
        //                break;
        //            case "1":
        //                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
        //                sType = 1;
        //                sValue = department.DEPARTMENTID;
        //                //if (paras.Count > 0)
        //                //{
        //                //    filter += " and ";
        //                //}
        //                //filter += "DEPARTMENTID==@" + paras.Count().ToString();
        //                //paras.Add(sValue);
        //                break;
        //            case "2":
        //                SMT.Saas.Tools.OrganizationWS.T_HR_POST post = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
        //                sType = 2;
        //                sValue = post.POSTID;
        //                //if (paras.Count > 0)
        //                //{
        //                //    filter += " and ";
        //                //}
        //                //filter += "POSTID==@" + paras.Count().ToString();
        //                //paras.Add(sValue);
        //                break;
        //        }
        //    }

        //    client.GetSalaryStandardPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYSTANDARDNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, sValue, strState);
        //}

        #region   旧标准加载
        private void LoadData() 
        {
            loadbar.Start();
            recordnum = 0;
            sign = true;
            getItemID.Clear();
            SpStandDetail.Children.Clear();
            items.Clear();
            DtGrid = new DataGrid();
            #region 设置当前加载DtGriddy样式
            DtGrid.Style = Application.Current.Resources["DataGridStyle"] as Style;
            DtGrid.CellStyle = Application.Current.Resources["DataGridCellStyle"] as Style;
            DtGrid.RowHeaderStyle = Application.Current.Resources["DataGridRowHeaderStyle"] as Style;
            DtGrid.RowStyle = Application.Current.Resources["DataGridRowStyle"] as Style;
            DtGrid.ColumnHeaderStyle = Application.Current.Resources["DataGridColumnHeaderStyle"] as Style;
            DtGrid.VerticalAlignment = VerticalAlignment.Stretch;
            #endregion
            DtGrid.AutoGenerateColumns = false;
            DtGrid.IsReadOnly = true;
            List<T_HR_SALARYSTANDARD> standitems = new List<T_HR_SALARYSTANDARD>();
            DataGridTextColumn col = new DataGridTextColumn();
            col.Header = Utility.GetResourceStr("SALARYSTANDARDNAME");
            col.Binding = new Binding("SALARYSTANDARDNAME");
            DtGrid.Columns.Add(col);
            foreach(var st in std)
            standitems.Add(st);
            //standitems.Add(selectedStand);
            DtGrid.ItemsSource = standitems;
            recordPoint = DtGrid.Columns.Count;
            if(std.Count>0)
            client.GetSalaryStandardItemsViewByStandarIDAsync(std[0].SALARYSTANDARDID);
            else
                SpStandDetail.Children.Add(DtGrid);

        }
        #endregion

        void SpStandDetail_Loaded(object sender, RoutedEventArgs e)
        {
            BindData();
        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGridStand.SelectedItems.Count > 0)
            {
                selectedStand = DtGrid.SelectedItems[0] as T_HR_SALARYSTANDARD;
                //  T_HR_SALARYSTANDARD tmpStandard = DtGrid.SelectedItems[0] as T_HR_SALARYSTANDARD;

                Form.Salary.SalaryStandardForm form = new SMT.HRM.UI.Form.Salary.SalaryStandardForm(FormTypes.Edit, selectedStand.SALARYSTANDARDID);

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

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGridStand.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_SALARYSTANDARD tmp in DtGridStand.SelectedItems)
                {
                    if (!(tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() || tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString()))
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                        return;
                    }
                    ids.Add(tmp.SALARYSTANDARDID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.SalaryStandardDeleteAsync(ids);
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



        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            /////TODO:ADD 审核  
            if (selectedStand != null)
            {
                // T_HR_SALARYSTANDARD tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSTANDARD;
                Form.Salary.SalaryStandardForm form = new Form.Salary.SalaryStandardForm(FormTypes.Audit, selectedStand.SALARYSTANDARDID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 780;
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
        void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (selectedStand != null)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    #region 测试代码
                    // client.SalaryStandardDeleteAsync(ids);
                    //T_HR_SALARYSTANDARD stand = new T_HR_SALARYSTANDARD();
                    //stand.SALARYSTANDARDID = "s1234567";

                    //ObservableCollection<T_HR_SALARYSTANDARDITEM> items = new ObservableCollection<T_HR_SALARYSTANDARDITEM>();
                    //T_HR_SALARYSTANDARDITEM item1 = new T_HR_SALARYSTANDARDITEM();
                    //item1.STANDRECORDITEMID = "i1234566";
                    //T_HR_SALARYSTANDARDITEM item2 = new T_HR_SALARYSTANDARDITEM();
                    //item2.STANDRECORDITEMID = "i1234567";
                    //items.Add(item1);
                    //items.Add(item2);
                    // client.AddSalaryStanderAndItemsAsync(stand,items);
                    #endregion

                };
                com.SelectionBox(Utility.GetResourceStr("BATCHCREATESTAND"), Utility.GetResourceStr("BATCHCREATESTANDALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "BATCHCREATESTAND"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "BATCHCREATESTAND"));
                return;
            }
        }
        #endregion

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //LoadData();
            LoadStander();
        }
        //#region 树形控件的操作
        ////绑定树
        //private void BindTree()
        //{
        //    orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
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
        //        BindCompany();
        //        orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
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
        //        BindDepartment();
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
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allPositions = e.Result.ToList();
        //        }
        //        BindPosition();
        //    }
        //}

        //private void BindCompany()
        //{
        //    treeOrganization.Items.Clear();
        //    if (allCompanys != null)
        //    {
        //        foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
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
        //    }
        //}

        //private void AddChildOrgItems(TreeViewItem item, string companyID)
        //{
        //    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> childs = GetChildORG(companyID);
        //    if (childs == null || childs.Count <= 0)
        //        return;

        //    foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY childOrg in childs)
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

        //private List<T_HR_COMPANY> GetChildORG(string companyID)
        //{
        //    List<T_HR_COMPANY> orgs = new List<T_HR_COMPANY>();

        //    foreach (T_HR_COMPANY org in allCompanys)
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
        //        foreach (SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDep in allDepartments)
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
        //                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
        //                if (tmpOrg != null)
        //                {
        //                    if (tmpOrg.COMPANYID == parentID)
        //                        return item;
        //                }
        //                break;
        //            case "1":
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

        //private void BindPosition()
        //{
        //    if (allPositions != null)
        //    {
        //        foreach (SMT.Saas.Tools.OrganizationWS.T_HR_POST tmpPosition in allPositions)
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

        #region 树形控件的操作  NEW缓存
        //绑定树
        private void BindTree()
        {

            if (Application.Current.Resources.Contains("ORGTREESYSCompanyInfo"))
            {
                // allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                BindCompany();
            }
            else
            {
                orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }

        }

        void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> entTemps = e.Result;
                allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                allCompanys.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allCompanys.Add(item);
                });

                UICache.CreateCache("ORGTREESYSCompanyInfo", allCompanys);
                orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> entTemps = e.Result;
                allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                allDepartments.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allDepartments.Add(item);
                });

                UICache.CreateCache("ORGTREESYSDepartmentInfo", allDepartments);

                BindCompany();

                orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

            }
        }

        void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        {

        }

        void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                UICache.CreateCache("ORGTREESYSPostInfo", allPositions);
                BindPosition();
            }
        }

        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

            if (allCompanys == null)
            {
                return;
            }

            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> TopCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();

            foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
            {
                //如果当前公司没有父机构的ID，则为顶级公司
                if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = tmpOrg.CNAME;
                    item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                    item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    item.DataContext = tmpOrg;

                    //状态在未生效和撤消中时背景色为红色
                    SolidColorBrush brush = new SolidColorBrush();
                    if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                    {
                        brush.Color = Colors.Red;
                        item.Foreground = brush;
                    }
                    else
                    {
                        brush.Color = Colors.Black;
                        item.Foreground = brush;
                    }
                    //标记为公司
                    item.Tag = OrgTreeItemTypes.Company.ToInt32();
                    treeOrganization.Items.Add(item);
                    TopCompany.Add(tmpOrg);
                }
                else
                {
                    //查询当前公司是否在公司集合内有父公司存在
                    var ent = from c in allCompanys
                              where tmpOrg.FATHERTYPE == "0" && c.COMPANYID == tmpOrg.FATHERID
                              select c;
                    var ent2 = from c in allDepartments
                               where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
                               select c;

                    //如果不存在，则为顶级公司
                    if (ent.Count() == 0 && ent2.Count() == 0)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = tmpOrg;

                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为公司
                        item.Tag = OrgTreeItemTypes.Company.ToInt32();
                        treeOrganization.Items.Add(item);

                        TopCompany.Add(tmpOrg);
                    }
                }
            }
            //开始递归
            foreach (var topComp in TopCompany)
            {
                TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, topComp.COMPANYID);
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany = (from ent in allCompanys
                                                                              where ent.FATHERTYPE == "0"
                                                                              && ent.FATHERID == topComp.COMPANYID
                                                                              select ent).ToList();

                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
                                                                                    where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
                                                                                    select ent).ToList();

                AddOrgNode(lsCompany, lsDepartment, parentItem);
            }
            allPositions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            if (allPositions != null)
            {
                BindPosition();
            }
        }

        private void AddOrgNode(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany, List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        {
            //绑定公司的子公司
            foreach (var childCompany in lsCompany)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childCompany.CNAME;
                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                item.DataContext = childCompany;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childCompany.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为公司
                item.Tag = OrgTreeItemTypes.Company.ToInt32();
                FatherNode.Items.Add(item);

                if (lsCompany.Count() > 0)
                {
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                                                     where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                     select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
            //绑定公司下的部门
            foreach (var childDepartment in lsDepartment)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                item.DataContext = childDepartment;
                item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childDepartment.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为部门
                item.Tag = OrgTreeItemTypes.Department.ToInt32();
                FatherNode.Items.Add(item);

                if (lsDepartment.Count() > 0)
                {
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                                                     where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                     select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
        }

        /// <summary>
        /// 绑定岗位
        /// </summary>
        private void BindPosition()
        {
            if (allPositions != null)
            {
                foreach (SMT.Saas.Tools.OrganizationWS.T_HR_POST tmpPosition in allPositions)
                {
                    if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
                        continue;
                    TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Department, tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                        item.DataContext = tmpPosition;
                        item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpPosition.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为岗位
                        item.Tag = OrgTreeItemTypes.Post.ToInt32();
                        parentItem.Items.Add(item);
                    }
                }
            }
            //树全部展开
            //  treeOrganization.ExpandAll();
            if (treeOrganization.Items.Count > 0)
            {
                TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
                selectedItem.IsSelected = true;
            }
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private TreeViewItem GetParentItem(OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            foreach (TreeViewItem item in treeOrganization.Items)
            {
                tmpItem = GetParentItemFromChild(item, parentType, parentID);
                if (tmpItem != null)
                {
                    break;
                }
            }
            return tmpItem;
        }

        private TreeViewItem GetParentItemFromChild(TreeViewItem item, OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            if (item.Tag != null && item.Tag.ToString() == parentType.ToInt32().ToString())
            {
                switch (parentType)
                {
                    case OrgTreeItemTypes.Company:
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case OrgTreeItemTypes.Department:
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        if (tmpDep != null)
                        {
                            if (tmpDep.DEPARTMENTID == parentID)
                                return item;
                        }
                        break;
                }

            }
            if (item.Items != null && item.Items.Count > 0)
            {
                foreach (TreeViewItem childitem in item.Items)
                {
                    tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
                    if (tmpItem != null)
                    {
                        break;
                    }
                }
            }
            return tmpItem;
        }


        #endregion

        #region
        private void DtGridStand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DtGridStand.SelectedItems.Count > 0)
            {
                //selectedStand = DtGridStand.SelectedItems[0] as T_HR_SALARYSTANDARD;
                //LoadData();
            }
        }
        void LoadStander()
        {
            std.Clear();
            SpStandDetail.Children.Clear();
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            string strState = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                //filter += " c.SALARYSTANDARDNAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(SALARYSTANDARDNAME)";
                //filter += " SALARYSTANDARDNAME.Contains(@" + paras.Count().ToString() + ")";
                paras.Add(txtName.Text.Trim());
            }
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }


            int  sType = 0;
            string sValue = "";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "0":
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        sType = 0;
                        sValue = company.COMPANYID;
                        //if (paras.Count > 0)
                        //{
                        //    filter += " and ";
                        //}
                        //filter += "COMPANYID==@" + paras.Count().ToString();
                        //paras.Add(sValue);
                        break;
                    case "1":
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        sType = 1;
                        sValue = department.DEPARTMENTID;
                        //if (paras.Count > 0)
                        //{
                        //    filter += " and ";
                        //}
                        //filter += "DEPARTMENTID==@" + paras.Count().ToString();
                        //paras.Add(sValue);
                        break;
                    case "2":
                        SMT.Saas.Tools.OrganizationWS.T_HR_POST post = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                        sType =2;
                        sValue = post.POSTID;
                        //if (paras.Count > 0)
                        //{
                        //    filter += " and ";
                        //}
                        //filter += "POSTID==@" + paras.Count().ToString();
                        //paras.Add(sValue);
                        break;
                }
            }

            client.GetSalaryStandardPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYSTANDARDNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType,  sValue, strState);
        }
        #endregion

        private void DtGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // BindData();
            //BindData(sender);
        }

        private void BindData()
        {
            if (DtGrid.ItemsSource != null)
            {
                foreach (object obj in DtGrid.ItemsSource)
                {
                    for (int i = 1; i < DtGrid.Columns.Count; i++)
                    {
                        DtGrid.Columns[i].GetCellContent(obj).DataContext = items[i - 1];
                    }
                }
            }
            loadbar.Stop();
        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
            orgClient.DoClose();
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
