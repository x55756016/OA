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
using System.IO;

using SMT.Saas.Tools.EngineWS;
using SMT.Saas.Tools.SalaryWS;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;

using SMT.HRM.UI.Form.Salary;
using System.Collections.ObjectModel;
using System.Windows.Data;

using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class Payment : BasePage, IClient
    {
        private bool frist = true;
        SalaryServiceClient client;
        EngineWcfGlobalFunctionClient engineclient;
        byte[] byExport;
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        private int recordnum = 0;
        private int recordPoint = 0;
        string salaryStandardid = string.Empty;
        SMTLoading loadbar = new SMTLoading();
        //OrganizationServiceClient orgClient;
        //private List<T_HR_COMPANY> allCompanys;
        //private List<T_HR_DEPARTMENT> allDepartments;
        //private List<T_HR_POST> allPositions;
        private bool sign = false;
        List<string> getItemID = new List<string>();
        List<object> itemsAll = new List<object>();
        ImageButton _ImgButtonMsgclose = new ImageButton();
        ImageButton _ImgButtonDataExPort = new ImageButton();
        List<T_HR_EMPLOYEESALARYRECORD> list = new List<T_HR_EMPLOYEESALARYRECORD>();
        private T_HR_SALARYSTANDARD selectedStand = new T_HR_SALARYSTANDARD();
        private T_HR_EMPLOYEESALARYRECORD salaryRecord = new T_HR_EMPLOYEESALARYRECORD();
        private DataGrid DtGriddy;
        public string msgCloseID { get; set; }
        private ObservableCollection<string> paymentIDs = new ObservableCollection<string>();
        public ObservableCollection<string> PaymentIDs
        {
            get { return paymentIDs; }
            set { paymentIDs = value; }
        }
        private const int CHECKSTATEED = 2;
        public Payment()
        {
            InitializeComponent();
            GetEntityLogo("T_HR_EMPLOYEESALARYRECORD");
            InitPara();
        }

        public Payment(FormTypes type, string ID)
        {
            InitializeComponent();
            GetEntityLogo("T_HR_EMPLOYEESALARYRECORD");
            InitPara();
            msgCloseID = ID;
        }

        public void InitPara()
        {
            PARENT.Children.Add(loadbar);
            try
            {
                client = new SalaryServiceClient();
                engineclient = new EngineWcfGlobalFunctionClient();
                client.GetAutoEmployeeSalaryRecordPagingsCompleted += new EventHandler<GetAutoEmployeeSalaryRecordPagingsCompletedEventArgs>(client_GetAutoEmployeeSalaryRecordPagingsCompleted);
                client.GetEmployeeSalaryRecordItemByIDCompleted += new EventHandler<GetEmployeeSalaryRecordItemByIDCompletedEventArgs>(client_GetEmployeeSalaryRecordItemByIDCompleted);
                client.PaymentUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PaymentUpdateCompleted);
                client.GetPaymentPagingsCompleted += new EventHandler<GetPaymentPagingsCompletedEventArgs>(client_GetPaymentPagingsCompleted);
                client.GetPaymentPagingCompleted += new EventHandler<GetPaymentPagingCompletedEventArgs>(client_GetPaymentPagingCompleted);
                client.ExportExcelCompleted += new EventHandler<ExportExcelCompletedEventArgs>(client_ExportExcelCompleted);
                client.ImportExcelCompleted += new EventHandler<ImportExcelCompletedEventArgs>(client_ImportExcelCompleted);
                client.ReadExcelCompleted += new EventHandler<ReadExcelCompletedEventArgs>(client_ReadExcelCompleted);
                client.ExportExcelAllCompleted += new EventHandler<ExportExcelAllCompletedEventArgs>(client_ExportExcelAllCompleted);
                this.DtGrid.CanUserSortColumns = false;
                this.Loaded += new RoutedEventHandler(Left_Loaded);
                this.DtGrid.LoadingRowDetails += new EventHandler<DataGridRowDetailsEventArgs>(DtGrid_LoadingRowDetails);
                treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
                client.GetSalaryStandardPagingCompleted += new EventHandler<GetSalaryStandardPagingCompletedEventArgs>(client_GetSalaryStandardPagingCompleted);


                //orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
                //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);

                //engineclient.MsgCloseCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(engineclient_MsgCloseCompleted);
                engineclient.CloseDoTaskCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(engineclient_CloseDoTaskCompleted);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
            #region 工具栏初始化
            ToolBar.txtOtherName.Visibility = Visibility.Visible;
            ToolBar.txtOtherName.Text = string.Empty;
            ToolBar.btnImport.Visibility = Visibility.Collapsed;
            ToolBar.btnImport.Click += new RoutedEventHandler(btnImport_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            //ToolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            //ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Visible;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.retRefresh.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("PAY")).Click += new RoutedEventHandler(Payment_Click);
            ImageButton _ImgButtonBankPaySalary = new ImageButton();
            _ImgButtonBankPaySalary.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonBankPaySalary.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("BANKPAY")).Click += new RoutedEventHandler(_ImgButtonBankPaySalary_Click);
            ToolBar.stpOtherAction.Children.Add(_ImgButtonBankPaySalary);

            //_ImgButtonDataExPort.VerticalAlignment = VerticalAlignment.Center;
            //_ImgButtonDataExPort.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("数据导出")).Click += new RoutedEventHandler(_ImgButtonDataExPort_Click);
            //ToolBar.stpOtherAction.Children.Add(_ImgButtonDataExPort);

            _ImgButtonMsgclose.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonMsgclose.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("关闭代办")).Click += new RoutedEventHandler(_ImgButtonMsgclose_Click);
            ToolBar.stpOtherAction.Children.Add(_ImgButtonMsgclose);

            #endregion
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            salaryStandardid = string.Empty;
            LoadStandard();
            LoadSalaryRecordData();
        }

        void client_ExportExcelAllCompleted(object sender, ExportExcelAllCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    byExport = e.Result;
                    Output();
                }
                else
                {
                    byExport = null;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ISNOT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ISNOT"));
                }
            }
            else
            {
                byExport = null;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            loadbar.Stop();
            _ImgButtonDataExPort.IsEnabled = true;
        }

        //void engineclient_MsgCloseCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("此代办任务已经处理"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
        //    }
        //    loadbar.Stop();
        //}

        void engineclient_CloseDoTaskCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("此代办任务已经处理"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
        }

        void _ImgButtonDataExPort_Click(object sender, RoutedEventArgs e)
        {
            loadbar.Start();
            _ImgButtonDataExPort.IsEnabled = false;
            string filter = "";
            string years = "";
            string months = "";
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
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "COMPANY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            try
            {
                System.Collections.ObjectModel.ObservableCollection<string> parass = new System.Collections.ObjectModel.ObservableCollection<string>();

                TextBox txtName = Utility.FindChildControl<TextBox>(expander, "employeeName");
                ComboBox cbxPaystate = Utility.FindChildControl<ComboBox>(expander, "cbxPayState");
                NumericUpDown nudyears = Utility.FindChildControl<NumericUpDown>(expander, "years");
                NumericUpDown nudmonths = Utility.FindChildControl<NumericUpDown>(expander, "months");

                if (nudyears != null && nudmonths != null)
                {
                    years = nudyears.Value.ToString();
                    months = nudmonths.Value.ToString();
                }
                else
                {
                    years = System.DateTime.Now.Year.ToString();
                    months = System.DateTime.Now.Month.ToString();
                }

                if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
                {
                    filter += "EMPLOYEENAME==@" + parass.Count().ToString();
                    parass.Add(txtName.Text.Trim());
                }

                if (cbxPaystate != null && cbxPaystate.SelectedIndex >= 0)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "PAYCONFIRM==@" + parass.Count().ToString();
                    parass.Add(cbxPaystate.SelectedIndex.ToString());
                    //parass.Add((cbxPaystate.SelectedIndex - 1).ToString());
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CHECKSTATE==@" + parass.Count().ToString();
                parass.Add(CHECKSTATEED.ToString());
                parass.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                //if (cbxPaystate != null && cbxPaystate.SelectedIndex > 0)
                //{
                //    filter += "PAYCONFIRM==@" + parass.Count().ToString();
                //    parass.Add((cbxPaystate.SelectedIndex - 1).ToString());
                //}
                dialog.Filter = "MS Excel Files|*.xls";
                dialog.FilterIndex = 1;
                result = dialog.ShowDialog();
                client.ExportExcelAllAsync("EMPLOYEESALARYRECORDID", filter, parass, years, months, sType, sValue);
            }
            catch { }
        }

        void _ImgButtonMsgclose_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(msgCloseID))
            {
                //engineclient.MsgCloseAsync(msgCloseID, string.Empty);
                ObservableCollection<string> strIds = new ObservableCollection<string>();
                strIds.Add(msgCloseID);
                engineclient.CloseDoTaskAsync(strIds, "T_HR_EMPLOYEESALARYRECORDPAYMENT", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                loadbar.Start();
            }
        }

        void client_GetSalaryStandardPagingCompleted(object sender, GetSalaryStandardPagingCompletedEventArgs e)
        {
            List<T_HR_SALARYSTANDARD> list = new List<T_HR_SALARYSTANDARD>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
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
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }

        }

        void client_GetEmployeeSalaryRecordItemByIDCompleted(object sender, GetEmployeeSalaryRecordItemByIDCompletedEventArgs e)
        {
            List<V_EMPLOYEESALARYRECORDITEM> its = new List<V_EMPLOYEESALARYRECORDITEM>();
            List<V_EMPLOYEESALARYRECORDITEM> items = new List<V_EMPLOYEESALARYRECORDITEM>();

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
                            txtCol.MaxWidth = 100;
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
                                if (it.SALARYITEMID == getItemID[i])
                                {
                                    items.Add(it);
                                    break;
                                }
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
                        MessageBox.Show(ex.ToString());
                        loadbar.Stop();
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
                if (i == 0)
                {
                    SpSalaryRecord.Children.Add(DtGriddy);
                }
                else
                {
                    SpSalaryRecord.Children.Add(DtGriddy);
                    SpSalaryRecord.Loaded += new RoutedEventHandler(SpSalaryRecord_Loaded);
                }
            }
            if (recordnum >= list.Count)
            {
                loadbar.Stop();
            }
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
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());                                
                                }
                            }
                        }
                        recordnum++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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

        void _ImgButtonBankPaySalary_Click(object sender, RoutedEventArgs e)
        {
            string filter = "";
            string years = "";
            string months = "";
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
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "COMPANY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "COMPANY"));
                return;
            }
            try
            {
                System.Collections.ObjectModel.ObservableCollection<string> parass = new System.Collections.ObjectModel.ObservableCollection<string>();

                TextBox txtName = Utility.FindChildControl<TextBox>(expander, "employeeName");
                ComboBox cbxPaystate = Utility.FindChildControl<ComboBox>(expander, "cbxPayState");
                NumericUpDown nudyears = Utility.FindChildControl<NumericUpDown>(expander, "years");
                NumericUpDown nudmonths = Utility.FindChildControl<NumericUpDown>(expander, "months");

                if (nudyears != null && nudmonths != null)
                {
                    years = nudyears.Value.ToString();
                    months = nudmonths.Value.ToString();
                }
                else
                {
                    years = System.DateTime.Now.Year.ToString();
                    months = System.DateTime.Now.Month.ToString();
                }

                if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
                {
                    filter += "EMPLOYEENAME==@" + parass.Count().ToString();
                    parass.Add(txtName.Text.Trim());
                }

                if (cbxPaystate != null && cbxPaystate.SelectedIndex >= 0)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "PAYCONFIRM ==@" + parass.Count().ToString();
                    parass.Add(cbxPaystate.SelectedIndex.ToString());
                    //parass.Add((cbxPaystate.SelectedIndex - 1).ToString());
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += " PAYCONFIRM !=@" + parass.Count().ToString();
                parass.Add("2");
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CHECKSTATE ==@" + parass.Count().ToString();
                parass.Add(CHECKSTATEED.ToString());
                parass.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                //if (cbxPaystate != null && cbxPaystate.SelectedIndex > 0)
                //{
                //    filter += "PAYCONFIRM==@" + parass.Count().ToString();
                //    parass.Add((cbxPaystate.SelectedIndex - 1).ToString());
                //}
                dialog.Filter = "MS Excel Files|*.xls";
                dialog.FilterIndex = 1;
                result = dialog.ShowDialog();
                client.ExportExcelAsync("EMPLOYEESALARYRECORDID", filter, parass, years, months, sType, sValue);
                #region -----
                //if (byExport == null)
                //{
                //    return;
                //}

                //SaveFileDialog dialog = new SaveFileDialog();
                //dialog.Filter = "MS Excel Files|*.xls";
                //dialog.FilterIndex = 1;

                //bool? result = dialog.ShowDialog();
                //if (result == true)
                //{
                //    using (Stream stream = dialog.OpenFile())
                //    {
                //        stream.Write(byExport, 0, byExport.Length);
                //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("EMPLOYEESALARYRECORD"));
                //        byExport = null;
                //    }
                //}
                //else
                //{
                //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EMPLOYEESALARYRECORD"));
                //    byExport = null;
                //}
                #endregion
            }
            catch { }
        }

        void Output()
        {
            if (byExport == null)
            {
                return;
            }

            if (result == true)
            {
                using (Stream stream = dialog.OpenFile())
                {
                    stream.Write(byExport, 0, byExport.Length);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("BANKDATAEXPORT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("BANKDATAEXPORT"));
                    byExport = null;
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EMPLOYEESALARYRECORD"));
                byExport = null;
            }
        }

        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            // LoadData();
            //  BindTree();

        }
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
        }

        void client_ImportExcelCompleted(object sender, ImportExcelCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_EMPLOYEESALARYRECORD> list = e.Result.ToList();
                DtGrid.ItemsSource = e.Result.ToList();
                //dataPager.PageCount = e.pageCount;
                string resultstr = Utility.GetResourceStr("NUMBEROFTRANSACTIONS") + e.successcount.ToString();
                resultstr += " , " + Utility.GetResourceStr("UNSUCCESSFULLOTS") + e.failcount;
                ToolBar.txtOtherName.Text = resultstr;
                dataPager.PageSize = (e.Result.Count <= 20) ? 20 : e.Result.Count;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXCELUPLOADFILEERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXCELUPLOADFILEERROR"));
            }
        }

        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            ImportPayFile();
        }

        private void ImportPayFile()
        {
            OpenFileDialog OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.Multiselect = false;
            OpenFileDialog.Filter = "Excel Files (*.xls)|*.xls;";

            if (OpenFileDialog.ShowDialog() != true)
            {
                return;
            }

            if (OpenFileDialog.File == null)
                return;
            try
            {
                Stream Stream = (System.IO.Stream)OpenFileDialog.File.OpenRead();
                byte[] Buffer = new byte[Stream.Length];
                Stream.Read(Buffer, 0, (int)Stream.Length);

                Stream.Dispose();
                Stream.Close();

                UploadFileModel UploadFile = new UploadFileModel();
                UploadFile.FileName = OpenFileDialog.File.Name;
                UploadFile.File = Buffer;
                //client.ImportExcelAsync(UploadFile);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }

        }

        void client_ReadExcelCompleted(object sender, ReadExcelCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_EMPLOYEESALARYRECORD> list = e.Result.ToList();
                DtGrid.ItemsSource = e.Result.ToList();
                //dataPager.PageCount = e.pageCount;
                //e.successcount;
                //e.failcount;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_ExportExcelCompleted(object sender, ExportExcelCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    byExport = e.Result;
                    Output();
                }
                else
                {
                    byExport = null;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ISNOT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ISNOT"));
                }
            }
            else
            {
                byExport = null;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }

        }

        void client_GetPaymentPagingsCompleted(object sender, GetPaymentPagingsCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }
            try
            {
                if (e.Result.Count > 0)
                {
                    DtGrid.ItemsSource = e.Result.ToList();
                    dataPager.PageCount = e.pageCount;
                }
            }
            catch
            {
                DtGrid.ItemsSource = null;
            }
            loadbar.Stop();
        }

        public void Payment_Click(object o, RoutedEventArgs e)
        {
            try
            {
                if (DtGriddy.SelectedItems.Count > 0)
                {
                    T_HR_EMPLOYEESALARYRECORD tmpPayment = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;

                    //if (paymentIDs.Count <= 0)
                    //{
                    //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                    //    return;
                    //}
                    if (Convert.ToInt32(tmpPayment.PAYCONFIRM.Trim()) <= 1)
                    {
                        tmpPayment.PAIDTYPE = "1";
                        tmpPayment.PAYCONFIRM = "2";
                        tmpPayment.PAIDDATE = System.DateTime.Now;
                        tmpPayment.PAIDBY = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                        client.PaymentUpdateAsync(tmpPayment);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTPAY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTPAY"));
                        return;
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"));
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"));
                ex.Message.ToString();
            }
        }

        void _btn_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_PAYMENT tmpPayment = DtGrid.SelectedItems[0] as V_PAYMENT;

                if (paymentIDs.Count <= 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                    return;
                }
                if (Convert.ToInt32(tmpPayment.PAYCONFIRM.Trim()) <= 1)
                {
                    tmpPayment.PAYTYPE = "1";
                    tmpPayment.PAYCONFIRM = "2";
                    tmpPayment.PAIDDATE = System.DateTime.Now;
                    tmpPayment.PAIDBY = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                    //client.PaymentUpdateAsync(tmpPayment);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTPAY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTPAY"));
                    return;
                }
            }
        }

        void client_PaymentUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("PAYSUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("PAYSUCCESSED"));
                LoadSalaryRecordData();
                //LoadData();
            }
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            //if (DtGrid.SelectedItems.Count > 0)
            //{
            //    V_PAYMENT tmpPayment = DtGrid.SelectedItems[0] as V_PAYMENT;

            //    if (paymentIDs.Count <= 0)
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
            //        return;
            //    }
            //    if (Convert.ToInt32(tmpPayment.PAYCONFIRM.Trim())<=1)
            //    {
            //        tmpPayment.PAYTYPE = "1";
            //        tmpPayment.PAYCONFIRM = "2";
            //        tmpPayment.PAIDDATE = System.DateTime.Now;
            //        tmpPayment.PAIDBY = "Adminstrator";
            //        client.PaymentUpdateAsync(tmpPayment);
            //    }
            //    else
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTPAY"));
            //        return;
            //    }
            //}
        }

        void browser_ReloadDataEvent()
        {
            LoadSalaryRecordData();
            // LoadData();
        }

        void client_GetPaymentPagingCompleted(object sender, GetPaymentPagingCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }
            try
            {
                if (e.Result.Count > 0)
                {
                    DtGrid.ItemsSource = e.Result.ToList();
                    dataPager.PageCount = e.pageCount;
                }
            }
            catch
            {
                DtGrid.ItemsSource = null;
            }
        }

        private DateTime? getDateMax(int years, int months)
        {
            DateTime? dateMax = new DateTime();
            switch (months)
            {
                case 4:
                case 6:
                case 9:
                case 11:
                    dateMax = new DateTime(years, months, 30); break;
                case 2:
                    if ((years % 4 == 0 && years % 100 != 0) || years % 400 == 0)
                        dateMax = new DateTime(years, months, 29);
                    else
                        dateMax = new DateTime(years, months, 28); break;
                default:
                    dateMax = new DateTime(years, months, 31); break;
            }
            return dateMax;
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEESALARYRECORD", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL("月薪 >>薪资发放");


        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
        }

        private void btpayment_Click(object sender, RoutedEventArgs e)
        {
            string filter = "";
            string years = "";
            string months = "";
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
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "PLEASESELECT");
                return;
            }
            try
            {
                System.Collections.ObjectModel.ObservableCollection<string> parass = new System.Collections.ObjectModel.ObservableCollection<string>();

                TextBox txtName = Utility.FindChildControl<TextBox>(expander, "employeeName");
                ComboBox cbxPaystate = Utility.FindChildControl<ComboBox>(expander, "cbxPayState");
                NumericUpDown nudyears = Utility.FindChildControl<NumericUpDown>(expander, "years");
                NumericUpDown nudmonths = Utility.FindChildControl<NumericUpDown>(expander, "months");

                if (nudyears != null && nudmonths != null)
                {
                    years = nudyears.Value.ToString();
                    months = nudmonths.Value.ToString();
                }
                else
                {
                    years = System.DateTime.Now.Year.ToString();
                    months = System.DateTime.Now.Month.ToString();
                }

                if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
                {
                    filter += "EMPLOYEENAME==@" + parass.Count().ToString();
                    parass.Add(txtName.Text.Trim());
                }

                //if (cbxPaystate != null && cbxPaystate.SelectedIndex > 0)
                //{
                //    if (!string.IsNullOrEmpty(filter))
                //    {
                //        filter += " and ";
                //    }
                //    filter += "PAYCONFIRM==@" + parass.Count().ToString();
                //    parass.Add((cbxPaystate.SelectedIndex - 1).ToString());
                //}
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CHECKSTATE==@" + parass.Count().ToString();
                parass.Add(CHECKSTATEED.ToString());

                if (cbxPaystate != null && cbxPaystate.SelectedIndex >= 0)
                {
                    filter += "PAYCONFIRM==@" + parass.Count().ToString();
                    parass.Add(cbxPaystate.SelectedIndex.ToString());
                    //parass.Add((cbxPaystate.SelectedIndex - 1).ToString());
                }

                client.ExportExcelAsync("EMPLOYEESALARYRECORDID", filter, parass, years, months, sType, sValue);

                if (byExport == null)
                {
                    return;
                }

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "MS Excel Files|*.xls";
                dialog.FilterIndex = 1;

                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    using (Stream stream = dialog.OpenFile())
                    {
                        stream.Write(byExport, 0, byExport.Length);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("EMPLOYEESALARYRECORD"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("EMPLOYEESALARYRECORD"));
                        byExport = null;
                    }
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EMPLOYEESALARYRECORD"));
                    byExport = null;
                }
            }
            catch { }
        }

        private void years_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = sender as NumericUpDown;
            nuyear.Value = Convert.ToDouble(System.DateTime.Now.Year);
            if (System.DateTime.Now.Month <= 1)
            {
                nuyear.Value = (System.DateTime.Now.Year - 1).ToDouble();
            }
        }

        private void months_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown numonth = sender as NumericUpDown;
            numonth.Value = Convert.ToDouble(System.DateTime.Now.Month);

            if (System.DateTime.Now.Month <= 1)
            {
                numonth.Value = 12;
            }
            else
            {
                numonth.Value = System.DateTime.Now.Month - 1;
            }
        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            paymentIDs.Clear();
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                paymentIDs.Add((grid.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD).EMPLOYEESALARYRECORDID);
            }
        }

        private void cbxPayState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_EMPLOYEESALARYRECORD");
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            salaryStandardid = string.Empty;
            LoadStandard();
            LoadSalaryRecordData();
            // LoadData();
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

            client.GetSalaryStandardPagingAsync(dataPagerStand.PageIndex, dataPagerStand.PageSize, "SALARYSTANDARDNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, sValue, strState);
        }


        private void DtGridStand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DtGridStand.SelectedItems.Count > 0)
            {
                selectedStand = DtGridStand.SelectedItems[0] as T_HR_SALARYSTANDARD;
                salaryStandardid = selectedStand.SALARYSTANDARDID;
                LoadSalaryRecordData();
            }
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

            string years = "";
            string months = "";
            loadbar.Start();

            DateTime? starttimes;
            DateTime? endtimes = DateTime.Now.Date;
            if (DateTime.Now.Month <= 2)
                starttimes = new DateTime(DateTime.Now.Year - 1, 1, 1);
            else
                starttimes = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 2, 1);

            DtGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;

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
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "employeeName");
            ComboBox cbxPaystate = Utility.FindChildControl<ComboBox>(expander, "cbxPayState");
            NumericUpDown nudyears = Utility.FindChildControl<NumericUpDown>(expander, "years");
            NumericUpDown nudmonths = Utility.FindChildControl<NumericUpDown>(expander, "months");
            if (nudyears != null && nudmonths != null)
            {
                years = nudyears.Value.ToString();
                months = nudmonths.Value.ToString();
            }
            else
            {
                years = System.DateTime.Now.Year.ToString();
                months = System.DateTime.Now.Month.ToString();
            }

            try
            {
                starttimes = new DateTime(nudyears.Value.ToInt32(), nudmonths.Value.ToInt32(), 1);
                endtimes = new DateTime(nudyears.Value.ToInt32(), nudmonths.Value.ToInt32(), DateTime.DaysInMonth(nudyears.Value.ToInt32(), nudmonths.Value.ToInt32()));
                //if (nudmonths.Value.ToInt32() == 12) endtimes = new DateTime(nudyears.Value.ToInt32() + 1, 1, 1); else endtimes = new DateTime(nudyears.Value.ToInt32(), nudmonths.Value.ToInt32(), 1);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }


            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                //filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                //filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                filter += " EMPLOYEENAME.Contains(@" + paras.Count().ToString() + ")";
                paras.Add(txtName.Text.Trim());
            }

            if (cbxPaystate != null && cbxPaystate.SelectedIndex >= 0)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "PAYCONFIRM==@" + paras.Count().ToString();
                paras.Add(cbxPaystate.SelectedIndex.ToString());
                //paras.Add((cbxPaystate.SelectedIndex - 1).ToString());
            }
            
            #region  薪资标准过滤
            if (!string.IsNullOrEmpty(salaryStandardid))
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

            //if (!string.IsNullOrEmpty(filter))
            //{
            //    filter += " and ";
            //}
            //filter += "CHECKSTATE==@" + paras.Count().ToString();
            //paras.Add(CHECKSTATEED.ToString());

            //client.GetPaymentPagingsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, years, months, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            client.GetAutoEmployeeSalaryRecordPagingsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), sType, sValue, CHECKSTATEED.ToString(), SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void LoadAutoData()
        {
            loadbar.Start();
            recordnum = 0;
            sign = true;
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
            //DtGriddy.IsReadOnly = true;

            #region  初始化必需项  NEW
            DataGridTextColumn col = new DataGridTextColumn();
            col.Header = Utility.GetResourceStr("EMPLOYEESALARYRECORDID");
            col.Binding = new Binding("EMPLOYEESALARYRECORDID");
            col.Visibility = Visibility.Collapsed;
            DtGriddy.Columns.Add(col);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = Utility.GetResourceStr("EMPLOYEENAME");
            col2.Binding = new Binding("EMPLOYEENAME");
            DtGriddy.Columns.Add(col2);

            DataGridTextColumn col8 = new DataGridTextColumn();
            col8.Header = Utility.GetResourceStr("ACTUALLYPAY");
            col8.Binding = new Binding("ACTUALLYPAY");
            DtGriddy.Columns.Add(col8);

            DataGridTextColumn col7 = new DataGridTextColumn();
            col7.Header = Utility.GetResourceStr("SALARYYEAR");
            col7.Binding = new Binding("SALARYYEAR");
            DtGriddy.Columns.Add(col7);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = Utility.GetResourceStr("SALARYMONTH");
            col4.Binding = new Binding("SALARYMONTH");
            DtGriddy.Columns.Add(col4);

            #endregion

            DtGriddy.ItemsSource = list;
            recordPoint = DtGriddy.Columns.Count;
            //if (list != null)
            //    client.GetEmployeeSalaryRecordItemByIDAsync(list[recordnum].EMPLOYEESALARYRECORDID);
            //else
            //{
                SpSalaryRecord.Children.Add(DtGriddy);
                loadbar.Stop();
            //}
        }

        private void dataPagerStand_Click(object sender, RoutedEventArgs e)
        {
            LoadStandard();
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

        private void Login_HandlerClick(object sender, EventArgs e)
        {
            LayoutRoot.Visibility = Visibility.Visible;
        }


        #region 废弃函数

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            string years = "";
            string months = "";
            loadbar.Start();
            int sType = 0;
            string sValue = "";
            DtGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
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
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "employeeName");
            ComboBox cbxPaystate = Utility.FindChildControl<ComboBox>(expander, "cbxPayState");
            NumericUpDown nudyears = Utility.FindChildControl<NumericUpDown>(expander, "years");
            NumericUpDown nudmonths = Utility.FindChildControl<NumericUpDown>(expander, "months");
            if (nudyears != null && nudmonths != null)
            {
                years = nudyears.Value.ToString();
                months = nudmonths.Value.ToString();
            }
            else
            {
                years = System.DateTime.Now.Year.ToString();
                months = System.DateTime.Now.Month.ToString();
            }

            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                paras.Add(txtName.Text.Trim());
            }

            if (cbxPaystate != null && cbxPaystate.SelectedIndex >= 0)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "PAYCONFIRM==@" + paras.Count().ToString();
                paras.Add((cbxPaystate.SelectedIndex).ToString());
                //paras.Add((cbxPaystate.SelectedIndex - 1).ToString());
            }
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            filter += "CHECKSTATE==@" + paras.Count().ToString();
            paras.Add(CHECKSTATEED.ToString());
            client.GetPaymentPagingsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, years, months, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

            #region  获取时间段
            //DateTime? starttime = new DateTime();
            //DateTime? endtime = new DateTime();
            //if (nudyears != null && nudmonths != null)
            //{
            //    starttime = new DateTime(Convert.ToInt32(nudyears.Value), Convert.ToInt32(nudmonths.Value), 1);
            //    endtime = new DateTime(Convert.ToInt32(nudyears.Value), Convert.ToInt32(nudmonths.Value+1), 1);               
            //}
            //else 
            //{
            //    starttime = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            //    endtime = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month+1, 1);
            //}
            #endregion
        }

        #endregion
    }
}
