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
using SMT.HRM.UI.Form;
using SMT.HRM.UI.Form.Salary;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

using SMT.SaaS.Globalization;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class PaySalaryHistory : BasePage, IClient
    {
        private int recordPoint = 0;
        private int recorddel = 0;
        string salaryStandardid = string.Empty;
        SMTLoading loadbar = new SMTLoading();
        //OrganizationServiceClient orgClient;
        //private List<T_HR_COMPANY> allCompanys;
        //private List<T_HR_DEPARTMENT> allDepartments;
        //private List<T_HR_POST> allPositions;
        private bool sign = false;
        List<object> itemsAll = new List<object>();
        List<string> getItemID = new List<string>();
        List<T_HR_EMPLOYEESALARYRECORD> list = new List<T_HR_EMPLOYEESALARYRECORD>();
        private T_HR_SALARYSTANDARD selectedStand = new T_HR_SALARYSTANDARD();
        private T_HR_EMPLOYEESALARYRECORD salaryRecord = new T_HR_EMPLOYEESALARYRECORD();
        private DataGrid DtGriddy;
        private int recordnum = 0;
        private ObservableCollection<string> paymentIDs = new ObservableCollection<string>();
        public ObservableCollection<string> PaymentIDs
        {
            get { return paymentIDs; }
            set { paymentIDs = value; }
        }

        private SalaryServiceClient client = new SalaryServiceClient();
        private string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        private string Checkstate { get; set; }
        private bool hisSign = true;

        public PaySalaryHistory()
        {
            InitializeComponent();
            InitParas();
            GetEntityLogo("T_HR_EMPLOYEESALARYRECORD");
            EditTitle("SALARY", "薪资发放查询");
            //LoadFristData();
            //LoadSalaryRecordData();
            loadbar.Stop();
            hisSign = true;
        }

        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.GetMenuSignAutoEmployeeSalaryRecordPagingsCompleted += new EventHandler<GetMenuSignAutoEmployeeSalaryRecordPagingsCompletedEventArgs>(client_GetMenuSignAutoEmployeeSalaryRecordPagingsCompleted);
            client.GetAutoEmployeeSalaryRecordPagingsCompleted += new EventHandler<GetAutoEmployeeSalaryRecordPagingsCompletedEventArgs>(client_GetAutoEmployeeSalaryRecordPagingsCompleted);
            client.GetEmployeeSalaryRecordItemByIDCompleted += new EventHandler<GetEmployeeSalaryRecordItemByIDCompletedEventArgs>(client_GetEmployeeSalaryRecordItemByIDCompleted);
            client.EmployeeSalaryRecordDeleteCompleted += new EventHandler<EmployeeSalaryRecordDeleteCompletedEventArgs>(client_EmployeeSalaryRecordDeleteCompleted);
            client.GetEmployeeSalaryRecordPagingCompleted += new EventHandler<GetEmployeeSalaryRecordPagingCompletedEventArgs>(client_GetEmployeeSalaryRecordPagingCompleted);
            client.FilterStandardCompleted += new EventHandler<FilterStandardCompletedEventArgs>(client_FilterStandardCompleted);

            #region 工具栏初试化
            ToolBar.txtOtherName.Visibility = Visibility.Collapsed;
            ToolBar.btnImport.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Visible;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.retRefresh.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            #endregion

            client.EmployeeSalaryRecordOrItemDeleteCompleted += new EventHandler<EmployeeSalaryRecordOrItemDeleteCompletedEventArgs>(client_EmployeeSalaryRecordOrItemDeleteCompleted);
            client.EmployeeSalaryRecordItemDeleteCompleted += new EventHandler<EmployeeSalaryRecordItemDeleteCompletedEventArgs>(client_EmployeeSalaryRecordItemDeleteCompleted);
            client.GetSalaryRecordOneCompleted += new EventHandler<GetSalaryRecordOneCompletedEventArgs>(client_GetSalaryRecordOneCompleted);
            this.DtGrid.CanUserSortColumns = false;
            this.DtGrid.LoadingRowDetails += new EventHandler<DataGridRowDetailsEventArgs>(DtGrid_LoadingRowDetails);
            this.Loaded += new RoutedEventHandler(Left_Loaded);
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            this.DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);

            client.GetSalaryStandardPagingCompleted += new EventHandler<GetSalaryStandardPagingCompletedEventArgs>(client_GetSalaryStandardPagingCompleted);

            //orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            salaryStandardid = string.Empty;
            //LoadStandard();
            LoadSalaryRecordData();
        }

        void client_GetMenuSignAutoEmployeeSalaryRecordPagingsCompleted(object sender, GetMenuSignAutoEmployeeSalaryRecordPagingsCompletedEventArgs e)
        {
            //if (hisSign)
            //{
            //    TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            //    if (txtName != null)
            //    {
            //        txtName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            //        hisSign = false;
            //    }
            //}
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].ACTUALLYPAY = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(list[i].ACTUALLYPAY);
                    }
                    DtGrid.ItemsSource = list;
                    dataPager.PageCount = e.pageCount;
                }
                else
                {
                    list = null;
                }
                LoadAutoData();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                loadbar.Stop();
            }
        }


        void client_EmployeeSalaryRecordOrItemDeleteCompleted(object sender, EmployeeSalaryRecordOrItemDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
     Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.ToString()));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEESALARYRECORD"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadSalaryRecordData();
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEESALARYRECORD"));
            }
            loadbar.Stop();
        }


        void client_EmployeeSalaryRecordItemDeleteCompleted(object sender, EmployeeSalaryRecordItemDeleteCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();
                ids.Add(e.UserState.ToString());
                client.EmployeeSalaryRecordDeleteAsync(ids);
            }
        }

        void client_GetAutoEmployeeSalaryRecordPagingsCompleted(object sender, GetAutoEmployeeSalaryRecordPagingsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].ACTUALLYPAY = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(list[i].ACTUALLYPAY);
                    }
                    DtGrid.ItemsSource = list;
                    dataPager.PageCount = e.pageCount;
                }
                else
                {
                    list = null;
                }
                LoadAutoData();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                loadbar.Stop();
            }

        }

        void client_GetEmployeeSalaryRecordItemByIDCompleted(object sender, GetEmployeeSalaryRecordItemByIDCompletedEventArgs e)
        {
            List<V_EMPLOYEESALARYRECORDITEM> its = new List<V_EMPLOYEESALARYRECORDITEM>();
            List<V_EMPLOYEESALARYRECORDITEM> items = new List<V_EMPLOYEESALARYRECORDITEM>();
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        //its = e.Result.ToList();
                        its = e.Result.OrderBy(m => m.ORDERNUMBER).ToList();
                        for (int i = 0; i < its.Count; i++)
                        {
                            its[i].SUM = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(its[i].SUM);
                        }
                        if (sign)
                        {
                            foreach (var it in its)
                            {
                                DataGridTextColumn txtCol = new DataGridTextColumn();

                                txtCol.Header = it.SALARYITEMNAME;
                                txtCol.Binding = new Binding("SUM");
                                txtCol.Width = DataGridLength.SizeToCells;
                                txtCol.MinWidth = 100;
                                txtCol.MaxWidth = 550;
                                DtGriddy.Columns.Add(txtCol);
                                getItemID.Add(it.SALARYITEMID);
                            }
                            sign = false;
                            items = its;
                        }
                        else
                        {
                            for (int i = 0; i < its.Count; i++)
                            {
                                if (i > getItemID.Count - 1) break;
                                foreach (var it in its)
                                {
                                    try
                                    {
                                        if (it.SALARYITEMID == getItemID[i])
                                        {
                                            items.Add(it);
                                            break;
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }

                    }
                }
                itemsAll.Add(items);
                ++recordnum;
                if (recordnum < list.Count)
                {
                    for (int i = recordnum; i < list.Count; i++)
                    {
                        try
                        {
                            client.GetEmployeeSalaryRecordItemByIDAsync(list[recordnum].EMPLOYEESALARYRECORDID);
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
                    foreach (List<V_EMPLOYEESALARYRECORDITEM> a in itemsAll)
                    {
                        i += a.Count;
                    }
                    if (i == 0) SpSalaryRecord.Children.Add(DtGriddy);
                    else
                    {
                        SpSalaryRecord.Children.Add(DtGriddy);
                        SpSalaryRecord.Loaded += new RoutedEventHandler(SpSalaryRecord_Loaded);
                    }
                }
            }
            catch (Exception exx)
            {
                exx.Message.ToString();
                loadbar.Stop();
            }
            if (recordnum >= list.Count) loadbar.Stop();
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
                if (DtGriddy.ItemsSource != null)
                {
                    foreach (object obj in DtGriddy.ItemsSource)
                    {
                        List<V_EMPLOYEESALARYRECORDITEM> q = (List<V_EMPLOYEESALARYRECORDITEM>)itemsAll[recordnum];
                        if (q.Count > 0)
                        {
                            for (int i = recordPoint; i < DtGriddy.Columns.Count; i++)
                            {
                                try
                                {
                                    DtGriddy.Columns[i].GetCellContent(obj).DataContext = (itemsAll[recordnum] as List<V_EMPLOYEESALARYRECORDITEM>)[i - recordPoint];
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

        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            //LoadAutoData();
            //LoadData();
            // BindTree();
        }


        void client_GetSalaryRecordOneCompleted(object sender, GetSalaryRecordOneCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    List<T_HR_EMPLOYEESALARYRECORD> salaryrecordlast = new List<T_HR_EMPLOYEESALARYRECORD>();
                    //txtbtitle.Text = Utility.GetResourceStr("SALARYLAST") + ":";
                }
                else
                {
                    //DGSalary.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void DtGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            DataGrid gridDetails = e.DetailsElement as DataGrid;
            T_HR_EMPLOYEESALARYRECORD sr = e.Row.DataContext as T_HR_EMPLOYEESALARYRECORD;
            if (sr.EMPLOYEEID != string.Empty && sr.SALARYYEAR != string.Empty && sr.SALARYMONTH != string.Empty)
            {
                string months = sr.SALARYMONTH;
                string years = sr.SALARYYEAR;
                if (sr.SALARYMONTH == "1")
                {
                    years = (Convert.ToInt32(years) - 1).ToString();
                    months = "12";
                }
                else
                {
                    months = (Convert.ToInt32(months) - 1).ToString();
                }
                SalaryServiceClient clienttemp = new SalaryServiceClient();
                clienttemp.GetSalaryRecordOneCompleted += (o, ev) =>
                {
                    if (ev.Error == null)
                    {
                        if (ev.Result != null)
                        {
                            List<T_HR_EMPLOYEESALARYRECORD> salaryrecordlast = new List<T_HR_EMPLOYEESALARYRECORD>();
                            salaryrecordlast.Add(ev.Result);
                            gridDetails.ItemsSource = salaryrecordlast;
                            gridDetails.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            gridDetails.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ev.Error.Message));
                    }
                };
                clienttemp.GetSalaryRecordOneAsync(sr.EMPLOYEEID, years, months);
            }
            else
            {
                gridDetails.Visibility = Visibility.Collapsed;
            }
        }


        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
        }

        void client_GetEmployeeSalaryRecordPagingCompleted(object sender, GetEmployeeSalaryRecordPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                LoadAutoData();
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            loadbar.Stop();
        }

        void client_EmployeeSalaryRecordDeleteCompleted(object sender, EmployeeSalaryRecordDeleteCompletedEventArgs e)
        {
            recorddel--;
            if (e.Error != null)
            {
                if (recorddel == 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                else
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (recorddel == 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEESALARYRECORD"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEESALARYRECORD"));
                    //LoadData();
                    loadbar.Stop();
                    LoadSalaryRecordData();
                }
            }
            if (recorddel == 0)
            {
                loadbar.Stop();
                ToolBar.btnDelete.IsEnabled = true;
            }
        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region 添加,修改,删除,查询,审核

        public T_HR_EMPLOYEESALARYRECORD SelectID { get; set; }
        void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectID = grid.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
            }
        }

        void browser_ReloadDataEvent()
        {
            LoadSalaryRecordData();
        }

        private void LoadSalaryRecordData()
        {
            loadbar.Start();
            int pageCount = 0;
            itemsAll.Clear();
            string filter = "";
            int sType = 0;
            string sValue = "";
            list = null;
            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            DateTime? starttimes;
            DateTime? endtimes = DateTime.Now.Date;
            if (DateTime.Now.Month <= 2)
                starttimes = new DateTime(DateTime.Now.Year - 1, 1, 1);
            else
                starttimes = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 2, 1);

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
                sType = 0;
                sValue = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                //loadbar.Stop();
                //return;
            }

            string strState = Convert.ToInt32(CheckStates.Approved).ToString();

            //DatePicker dpstarttimes = Utility.FindChildControl<DatePicker>(expander, "dpstarttime");
            //DatePicker dpendtimes = Utility.FindChildControl<DatePicker>(expander, "dpendtime");
            NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
            NumericUpDown nuEndYear = Utility.FindChildControl<NumericUpDown>(expander, "NuEndyear");
            NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
            NumericUpDown nuEndmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuEndmounth");

            //if (nuYear != null && nuStartmounth != null)
            //{
            starttimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), 1);
            endtimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), DateTime.DaysInMonth(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32()));
            //}
            //else
            //{
            //    int year, month;
            //    year = DateTime.Now.Year;
            //    month = DateTime.Now.Month - 1;
            //    if (DateTime.Now.Month <= 1)
            //    {
            //        year = DateTime.Now.Year - 1;
            //        month = 12;
            //    }
            //    starttimes = new DateTime(year, month, 1);
            //    endtimes = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            //}

            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                filter += " EMPLOYEENAME.Contains(@" + paras.Count().ToString() + ")";
                paras.Add(txtName.Text.Trim());
            }
            //else
            //{
            //    filter += " EMPLOYEENAME.Contains(@" + paras.Count().ToString() + ")";
            //    paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName);
            //}
            if (!string.IsNullOrEmpty(userID))
            {
                if (!string.IsNullOrEmpty(filter)) filter += " and ";
                //filter += "EMPLOYEEID==@" + paras.Count().ToString();
                //paras.Add(userID);
                //filter += " OR EMPLOYEEID==@" + paras.Count().ToString();
                //paras.Add(userID);
                //filter += " AND PAYCONFIRM==@" + paras.Count().ToString();
                filter += " OWNERID==@" + paras.Count().ToString();
                paras.Add(userID);

            }
            if (!string.IsNullOrEmpty(userID))
            {
                if (!string.IsNullOrEmpty(filter)) filter += " and ";
                //filter += "EMPLOYEEID==@" + paras.Count().ToString();
                //paras.Add(userID);
                //filter += " OR EMPLOYEEID==@" + paras.Count().ToString();
                //paras.Add(userID);
                //filter += " AND PAYCONFIRM==@" + paras.Count().ToString();
                filter += " PAYCONFIRM==@" + paras.Count().ToString();
                paras.Add("2");

            }
            else
            {
                loadbar.Stop();
                return;
            }

            #region  薪资标准过滤
            if (!string.IsNullOrEmpty(salaryStandardid))    //过滤测试时候注释
            {
                if (!string.IsNullOrEmpty(filter)) filter += " and ";
                filter += "SALARYSTANDARDID==@" + paras.Count().ToString();
                paras.Add(salaryStandardid);
            }
            //else
            //{
            //    loadbar.Stop();
            //    return;
            //}
            #endregion

            client.GetMenuSignAutoEmployeeSalaryRecordPagingsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), sType, sValue, strState, userID, "PAYSALARYHISTORY");
        }

        private void LoadFristData()
        {
            loadbar.Start();
            int pageCount = 0;
            itemsAll.Clear();
            string filter = "";
            int sType = 0;
            string sValue = "";
            list = null;
            DateTime? starttimes;
            DateTime? endtimes = DateTime.Now.Date;
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            string strState = Convert.ToInt32(CheckStates.Approved).ToString();
            int year, month;
            year = DateTime.Now.Year;
            month = DateTime.Now.Month - 1;
            if (DateTime.Now.Month <= 1)
            {
                year = DateTime.Now.Year - 1;
                month = 12;
            }
            starttimes = new DateTime(year, month, 1);
            endtimes = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            sType = 0;
            sValue = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            filter += " EMPLOYEENAME.Contains(@" + paras.Count().ToString() + ")";
            paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName);
            if (!string.IsNullOrEmpty(userID))
            {
                if (!string.IsNullOrEmpty(filter)) filter += " and ";
                filter += " PAYCONFIRM==@" + paras.Count().ToString();
                paras.Add("2");

            }
            if (!string.IsNullOrEmpty(userID))
            {
                if (!string.IsNullOrEmpty(filter)) filter += " and ";
                //filter += "EMPLOYEEID==@" + paras.Count().ToString();
                //paras.Add(userID);
                //filter += " OR EMPLOYEEID==@" + paras.Count().ToString();
                //paras.Add(userID);
                //filter += " AND PAYCONFIRM==@" + paras.Count().ToString();
                filter += " OWNERID==@" + paras.Count().ToString();
                paras.Add(userID);

            }
            client.GetMenuSignAutoEmployeeSalaryRecordPagingsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), sType, sValue, strState, userID, "PAYSALARYHISTORY");
        }

        private void LoadAutoData()
        {
            loadbar.Start();
            recordnum = 0;
            sign = true;
            getItemID.Clear();
            SpSalaryRecord.Children.Clear();
            DtGriddy = new DataGrid();

            #region 设置当前加载DtGriddy样式
            DtGriddy.Style = Application.Current.Resources["DataGridStyle"] as Style;
            DtGriddy.CellStyle = Application.Current.Resources["DataGridCellStyle"] as Style;
            DtGriddy.RowHeaderStyle = Application.Current.Resources["DataGridRowHeaderStyle"] as Style;
            DtGriddy.RowStyle = Application.Current.Resources["DataGridRowStyle"] as Style;
            DtGriddy.ColumnHeaderStyle = Application.Current.Resources["DataGridColumnHeaderStyle"] as Style;
            #endregion

            DtGriddy.AutoGenerateColumns = false;
            DtGriddy.HorizontalAlignment = HorizontalAlignment.Stretch;
            DtGriddy.VerticalAlignment = VerticalAlignment.Stretch;
            DtGriddy.IsReadOnly = true;

            #region  初始化必需项
            DataGridTextColumn col = new DataGridTextColumn();
            col.Header = Utility.GetResourceStr("EMPLOYEESALARYRECORDID");
            col.Binding = new Binding("EMPLOYEESALARYRECORDID");
            col.Visibility = Visibility.Collapsed;
            DtGriddy.Columns.Add(col);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = Utility.GetResourceStr("EMPLOYEENAME");
            col2.Binding = new Binding("EMPLOYEENAME");
            DtGriddy.Columns.Add(col2);

            DataGridTextColumn col7 = new DataGridTextColumn();
            col7.Header = Utility.GetResourceStr("SALARYYEAR");
            col7.Binding = new Binding("SALARYYEAR");
            DtGriddy.Columns.Add(col7);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = Utility.GetResourceStr("SALARYMONTH");
            col4.Binding = new Binding("SALARYMONTH");
            DtGriddy.Columns.Add(col4);

            DataGridTextColumn col8 = new DataGridTextColumn();
            col8.Header = Utility.GetResourceStr("ACTUALLYPAY");
            col8.Binding = new Binding("ACTUALLYPAY");
            DtGriddy.Columns.Add(col8);

            #endregion

            DtGriddy.ItemsSource = list;
            recordPoint = DtGriddy.Columns.Count;
            if (list != null)
                client.GetEmployeeSalaryRecordItemByIDAsync(list[recordnum].EMPLOYEESALARYRECORDID);
            else
            {
                SpSalaryRecord.Children.Add(DtGriddy);
                loadbar.Stop();
            }
        }

        void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_EMPLOYEESALARYRECORD tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.EMPLOYEESALARYRECORDID);
                }
                client.EmployeeSalaryRecordDeleteAsync(ids);
            }

        }

        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_EMPLOYEESALARYRECORD");
        }

        private void dpstarttime_Loaded(object sender, RoutedEventArgs e)
        {
            //DatePicker dp = (DatePicker)sender;
            //dp.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString();           
        }

        private void dpendtime_Loaded(object sender, RoutedEventArgs e)
        {
            //DatePicker dp = (DatePicker)sender;
            //int MaxDate = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            //dp.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, MaxDate).ToShortDateString();
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            salaryStandardid = string.Empty;
            //LoadStandard();
            LoadSalaryRecordData();
        }

        void client_FilterStandardCompleted(object sender, FilterStandardCompletedEventArgs e)
        {
            List<T_HR_SALARYSTANDARD> list = new List<T_HR_SALARYSTANDARD>();
            list = e.UserState as List<T_HR_SALARYSTANDARD>;
            DtGridStand.ItemsSource = list;
            //dataPagerStand.PageCount = e.pageCount;
            try
            {
                salaryStandardid = list[0].SALARYSTANDARDID;
                LoadSalaryRecordData();
            }
            catch (Exception exc)
            {
                exc.Message.ToString();
            }
        }

        void client_GetSalaryStandardPagingCompleted(object sender, GetSalaryStandardPagingCompletedEventArgs e)
        {
            List<T_HR_SALARYSTANDARD> list = new List<T_HR_SALARYSTANDARD>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    //list = (from a in list.AsQueryable() select a.SALARYSTANDARDID).Intersect(from b in list.AsQueryable() select b.SALARYSTANDARDID);
                    //NumericUpDown nuYear1 = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
                    //NumericUpDown nuStartmounth1 = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
                    //client.FilterStandardAsync(nuYear1.Value.ToString(), nuStartmounth1.Value.ToString(),list);

                    DtGridStand.ItemsSource = list;
                    dataPagerStand.PageCount = e.pageCount;
                    //try
                    //{
                    //    salaryStandardid = list[0].SALARYSTANDARDID;
                    //    LoadSalaryRecordData();
                    //}
                    //catch (Exception exc)
                    //{
                    //    exc.Message.ToString();
                    //}
                }
                else
                {
                    list = null;
                    DtGridStand.ItemsSource = null;
                }
            }

            //loadbar.Stop();
        }

        void LoadStandard()
        {
            SpSalaryRecord.Children.Clear();
            //loadbar.Start();
            int pageCount = 0;
            string filter = "";
            string strState = "2";
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
            //LoadSalaryRecordData();
            client.GetSalaryStandardPagingAsync(dataPagerStand.PageIndex, dataPagerStand.PageSize, "SALARYSTANDARDNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, sValue, strState);
        }

        private void DtGridStand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DtGridStand.SelectedItems.Count > 0)
            {
                //selectedStand = DtGridStand.SelectedItems[0] as T_HR_SALARYSTANDARD;
                //salaryStandardid = selectedStand.SALARYSTANDARDID;
                //LoadSalaryRecordData();
            }
        }

        private void Nuyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Year.ToDouble();
            if (System.DateTime.Now.Month <= 1)
            {
                nuyear.Value = (System.DateTime.Now.Year - 1).ToDouble();
            }
        }

        private void NuStartmounth_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nustartmonuth = (NumericUpDown)sender;
            nustartmonuth.Value = DateTime.Now.Month.ToDouble();
            if (System.DateTime.Now.Month <= 1)
            {
                nustartmonuth.Value = 12;
            }
            else
            {
                nustartmonuth.Value = System.DateTime.Now.Month - 1;
            }
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

        private void Login_HandlerClick(object sender, EventArgs e)
        {
            LayoutRoot.Visibility = Visibility.Visible;
        }

        private void dataPagerStand_Click(object sender, RoutedEventArgs e)
        {
            LoadStandard();
        }

        private void EditTitle(string parentTitle, string newTitle)
        {
            System.Text.StringBuilder sbtitle = new System.Text.StringBuilder();
            sbtitle.Append(Utility.GetResourceStr(parentTitle));
            sbtitle.Append(">>");
            sbtitle.Append(Utility.GetResourceStr(newTitle));
            ViewTitles.TextTitle.Text = sbtitle.ToString();
        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
            // orgClient.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = "薪资发放查询"; //GetTitleFromURL(e.Uri.ToString());
        }

        private void txtName_Loaded(object sender, RoutedEventArgs e)
        {
            if (hisSign)
            {
                TextBox txtName = sender as TextBox;
                txtName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                LoadFristData();
            }
        }

    }
}
