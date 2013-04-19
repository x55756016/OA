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
using System.IO;


namespace SMT.HRM.UI.Views.Salary
{
    public partial class EmployeeSalaryRecord : BasePage, IClient
    {
        private bool FBSign = false;
        private int recordPoint = 0;
        private int recorddel = 0;
        private string recordFB = string.Empty;
        string salaryStandardid = string.Empty;
        SMTLoading loadbar = new SMTLoading();
        Form.Salary.SalaryRecordMassAudit form;
        private bool sign = false;
        List<object> itemsAll = new List<object>();
        List<string> getItemID = new List<string>();
        List<T_HR_EMPLOYEESALARYRECORD> list = new List<T_HR_EMPLOYEESALARYRECORD>();
        private T_HR_SALARYSTANDARD selectedStand = new T_HR_SALARYSTANDARD();
        private T_HR_EMPLOYEESALARYRECORD salaryRecord = new T_HR_EMPLOYEESALARYRECORD();
        private DataGrid DtGriddy;
        private int recordnum = 0;
        private TextBox _txtBox = new TextBox();
        private TextBlock _Textshow = new TextBlock();
        private ObservableCollection<string> paymentIDs = new ObservableCollection<string>();
        public ObservableCollection<string> PaymentIDs
        {
            get { return paymentIDs; }
            set { paymentIDs = value; }
        }

        private SalaryServiceClient client = new SalaryServiceClient();
        private string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        private string Checkstate { get; set; }

        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;

        public EmployeeSalaryRecord()
        {
            InitializeComponent();
            InitParas();
            GetEntityLogo("T_HR_EMPLOYEESALARYRECORD");
         
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEESALARYRECORD", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }


        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.SalaryContrastAllCompleted += new EventHandler<SalaryContrastAllCompletedEventArgs>(client_SalaryContrastAllCompleted);
            client.GetAutoEmployeeSalaryRecordPagingsCompleted += new EventHandler<GetAutoEmployeeSalaryRecordPagingsCompletedEventArgs>(client_GetAutoEmployeeSalaryRecordPagingsCompleted);
            client.GetEmployeeSalaryRecordItemByIDCompleted += new EventHandler<GetEmployeeSalaryRecordItemByIDCompletedEventArgs>(client_GetEmployeeSalaryRecordItemByIDCompleted);
            client.EmployeeSalaryRecordDeleteCompleted += new EventHandler<EmployeeSalaryRecordDeleteCompletedEventArgs>(client_EmployeeSalaryRecordDeleteCompleted);
            client.GetEmployeeSalaryRecordPagingCompleted += new EventHandler<GetEmployeeSalaryRecordPagingCompletedEventArgs>(client_GetEmployeeSalaryRecordPagingCompleted);
            client.FBStatisticsCompleted += new EventHandler<FBStatisticsCompletedEventArgs>(client_FBStatisticsCompleted);
            client.FBStatisticsMassCompleted += new EventHandler<FBStatisticsMassCompletedEventArgs>(client_FBStatisticsMassCompleted);
            client.FilterStandardCompleted += new EventHandler<FilterStandardCompletedEventArgs>(client_FilterStandardCompleted);

            #region  工具栏
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("MASSAUDIT")).Click += new RoutedEventHandler(MassAudit_Click);

            ImageButton _ImgButtonDeductionmoney = new ImageButton();
            _ImgButtonDeductionmoney.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonDeductionmoney.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("员工扣款明细")).Click += new RoutedEventHandler(_ImgButtonDeductionmoney_Click);
            ToolBar.stpOtherAction.Children.Add(_ImgButtonDeductionmoney);
            _ImgButtonDeductionmoney.Visibility = Visibility.Visible;
            //if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue("YGKKMX", SMT.SaaS.FrameworkUI.Common.Permissions.Browse) > 0)
            //{
            //    _ImgButtonDeductionmoney.Visibility = Visibility.Visible;
            //}

            ImageButton _ImgButtonDeductionTax = new ImageButton();
            _ImgButtonDeductionTax.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonDeductionTax.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("月度薪资扣税")).Click += new RoutedEventHandler(_ImgButtonDeductionTax_Click);
            ToolBar.stpOtherAction.Children.Add(_ImgButtonDeductionTax);
            _ImgButtonDeductionTax.Visibility = Visibility.Visible;

            //if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue("YDXZKS", SMT.SaaS.FrameworkUI.Common.Permissions.Browse) > 0)
            //{
            //    _ImgButtonDeductionTax.Visibility = Visibility.Visible;
            //}

            ImageButton _ImgButtonSalarySum = new ImageButton();
            _ImgButtonSalarySum.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonSalarySum.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("薪资汇总")).Click += new RoutedEventHandler(_ImgButtonSalarySum_Click);
            ToolBar.stpOtherAction.Children.Add(_ImgButtonSalarySum);
            _ImgButtonSalarySum.Visibility = Visibility.Visible;

            //薪酬报表导出
            ImageButton _ImgButtonReport = new ImageButton();
            _ImgButtonReport.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonReport.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("薪酬保险缴交报表")).Click += new RoutedEventHandler(_ImgButtonReports_Click);
            ToolBar.stpOtherAction.Children.Add(_ImgButtonReport);
            #endregion

            #region  新增控件(薪水对比度)
            Rectangle _Rectangle = new Rectangle();
            _Rectangle.Height = 18;
            _Rectangle.Width = 1;
            _Rectangle.Fill = new SolidColorBrush(Color.FromArgb(255, 154, 154, 153));
            _Rectangle.Margin = new Thickness(3, 4, 0, 3);

            TextBlock _txtblock = new TextBlock();
            _txtblock.Text = "薪水对比提醒额度";
            _txtblock.VerticalAlignment = VerticalAlignment.Center;
            _txtblock.Margin = new Thickness(4, 0, 0, 0);

            _txtBox.Margin = new Thickness(3, 0, 0, 0);
            _txtBox.Width = 100;
            _txtBox.Height = 24;
            _txtBox.Text = "500";
            _txtBox.VerticalAlignment = VerticalAlignment.Center;

            ToolBar.stpOtherAction.Children.Add(_Rectangle);
            ToolBar.stpOtherAction.Children.Add(_txtblock);
            ToolBar.stpOtherAction.Children.Add(_txtBox);

            ToolBar.stpOtherAction.Visibility = Visibility.Visible;

            _Textshow.Text = string.Empty;
            _Textshow.Margin = new Thickness(6, 0, 0, 0);
            ToolBar.stpOtherAction.Children.Add(_Textshow);
            _Textshow.VerticalAlignment = VerticalAlignment.Center;
            #endregion

            client.EmployeeSalaryRecordOrItemDeleteCompleted += new EventHandler<EmployeeSalaryRecordOrItemDeleteCompletedEventArgs>(client_EmployeeSalaryRecordOrItemDeleteCompleted);
            client.EmployeeSalaryRecordItemDeleteCompleted += new EventHandler<EmployeeSalaryRecordItemDeleteCompletedEventArgs>(client_EmployeeSalaryRecordItemDeleteCompleted);
            client.GetSalaryRecordOneCompleted += new EventHandler<GetSalaryRecordOneCompletedEventArgs>(client_GetSalaryRecordOneCompleted);
            this.DtGrid.CanUserSortColumns = false;
            this.DtGrid.LoadingRowDetails += new EventHandler<DataGridRowDetailsEventArgs>(DtGrid_LoadingRowDetails);
            this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            this.Loaded += new RoutedEventHandler(Left_Loaded);
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            this.DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);

            client.GetSalaryStandardPagingCompleted += new EventHandler<GetSalaryStandardPagingCompletedEventArgs>(client_GetSalaryStandardPagingCompleted);
            client.ExportEmployeePensionReportsCompleted += new EventHandler<ExportEmployeePensionReportsCompletedEventArgs>(client_ExportEmployeePensionReportsCompleted);

        }

        void client_ExportEmployeePensionReportsCompleted(object sender, ExportEmployeePensionReportsCompletedEventArgs e)
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
        void _ImgButtonDeductionmoney_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.EmployeeDeductionMoney form = new SMT.HRM.UI.Form.Salary.EmployeeDeductionMoney();
            form.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        void _ImgButtonDeductionTax_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.MonthDeductionTax form = new SMT.HRM.UI.Form.Salary.MonthDeductionTax();
            form.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void _ImgButtonSalarySum_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.EmployeeSalarySummary form = new SMT.HRM.UI.Form.Salary.EmployeeSalarySummary();
            form.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        /// <summary>
        /// 导出薪资、社保明细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ImgButtonReports_Click(object sender, RoutedEventArgs e)
        {
            
            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;
            
            result = dialog.ShowDialog();
            if (result.Value == true)
            {
                int pageCount = 0;
                itemsAll.Clear();
                string filter = "";
                int sType = 0;
                string sValue = "";
                list = null;
                DateTime? starttimes;
                DateTime? endtimes = DateTime.Now.Date;
                if (DateTime.Now.Month <= 2)
                    starttimes = new DateTime(DateTime.Now.Year - 1, 1, 1);
                else
                    starttimes = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 2, 1);

                System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
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
                if (!string.IsNullOrEmpty(sValue))
                {
                    filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                    paras.Add(sValue);
                }

                #region
                if (sType != 0)
                {
                    ComfirmWindow.ConfirmationBoxs("提示", "请选择公司", Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    return;
                }
                #endregion

                


                NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
                NumericUpDown nuEndYear = Utility.FindChildControl<NumericUpDown>(expander, "NuEndyear");
                NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
                NumericUpDown nuEndmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuEndmounth");

                #region 多月份的过滤 后备代码
                //try
                //{   
                starttimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), 1);

                #endregion

                //if (nuStartmounth.Value.ToInt32() == 12) endtimes = new DateTime(nuYear.Value.ToInt32() + 1, 1, 1); else endtimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), 1);
                endtimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), DateTime.DaysInMonth(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32()));
                loadbar.Start();
                client.ExportEmployeePensionReportsAsync("COMPANYNAME", filter, paras, Common.CurrentLoginUserInfo.EmployeeID, sValue,(DateTime)starttimes);
                




            }

            
        }
        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            salaryStandardid = string.Empty;
            //LoadData();
            LoadStandard();
            LoadSalaryRecordData();
        }

        void client_SalaryContrastAllCompleted(object sender, SalaryContrastAllCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    ObservableCollection<string> title = e.Result;
                    ObservableCollection<string> nowdata = e.nowData;
                    ObservableCollection<string> lastdata = e.lastData;
                    sp_newdata.Children.Clear();
                    sp_lastdata.Children.Clear();
                    sp_title.Children.Clear();
                    for (int i = 0; i < e.nowData.Count; i++)
                    {
                        TextBlock txtTitle = new TextBlock();
                        if (i == 0)
                        {
                            txtTitle.Text = Utility.GetResourceStr("ACTUALLYPAY");
                        }
                        else
                            txtTitle.Text = title[i - 1] + "  ";
                        txtTitle.Width = 100;
                        txtTitle.Height = 15;
                        txtTitle.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        sp_title.Children.Add(txtTitle);

                        TextBlock txt = new TextBlock();
                        if (i == 0)
                            txt.Text = Utility.GetResourceStr("THISMONTH") + " " + nowdata[i] + "\x20\x20";
                        else
                            txt.Text = nowdata[i];
                        txt.Width = txtTitle.Width;
                        txt.Height = 15;
                        txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        sp_newdata.Children.Add(txt);

                        TextBlock txtlast = new TextBlock();
                        string strlastdata = string.Empty;
                        if (i < lastdata.Count())
                        {
                            strlastdata = lastdata[i];
                        }
                        if (i == 0)
                            txtlast.Text = Utility.GetResourceStr("LASTMONTH") + " " + strlastdata + "\x20\x20";
                        else
                            txtlast.Text = strlastdata;
                        txtlast.Width = txtTitle.Width;
                        txtlast.Height = 15;
                        txtlast.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                        if (!string.IsNullOrEmpty(_txtBox.Text))
                        {
                            decimal dNowData = 0, dLastData = 0, dCheckData = 0;
                            decimal.TryParse(txt.Text, out dNowData);
                            decimal.TryParse(txtlast.Text, out dLastData);
                            decimal.TryParse(_txtBox.Text, out dCheckData);

                            if (dNowData - dLastData > dCheckData)
                            {
                                txt.Foreground = new SolidColorBrush(Colors.Red);
                            }
                        }
                        sp_lastdata.Children.Add(txtlast);
                    }
                }
                catch
                {

                }

                ShowPasswordStoryboard.Begin();
            }
        }

        void client_FBStatisticsMassCompleted(object sender, FBStatisticsMassCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result[0] == "false")
                {
                    FBSign = true;
                    recordFB = e.Result[1];
                }
                else
                {
                    // string employeesalaryrecordid = e.UserState as string;
                }
            }
            else
            {

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
            }
            loadbar.Stop();
        }

        public void MassAudit_Click(object o, RoutedEventArgs e)
        {
            try
            {
                ///TODO:ADD 审核  
                if (DtGriddy == null) return;
                int sType = 0;
                string sValue = "";
                string state = "";

                //选择一条数据
                if (DtGriddy.SelectedItems.Count > 0)
                {
                    T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
                    if (tmpEnt.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        sType = 3; //单条记录
                        sValue = tmpEnt.EMPLOYEESALARYRECORDID;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
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
                        return;
                    }
                }
                state = CheckStates.UnSubmit.ToInt32().ToString();

                NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
                NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");

                List<string> paras = new List<string>();
                paras.Add(sType.ToString());
                paras.Add(sValue);
                paras.Add(nuYear.Value.ToString());
                paras.Add(nuStartmounth.Value.ToString());
                paras.Add(state);
                paras.Add("");


                form = new Form.Salary.SalaryRecordMassAudit(FormTypes.Audit, paras);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGriddy.SelectedItems.Count > 0)
            {
                T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
                //client.FBStatisticsAsync(tmpEnt.EMPLOYEEID, Convert.ToInt32(tmpEnt.SALARYYEAR), Convert.ToInt32(tmpEnt.SALARYMONTH), (object)tmpEnt.EMPLOYEESALARYRECORDID);
                //client.FBStatisticsAsync(tmpEnt.EMPLOYEEID, Convert.ToInt32(tmpEnt.SALARYYEAR), Convert.ToInt32(tmpEnt.SALARYMONTH), (object)tmpEnt.EMPLOYEESALARYRECORDID);
                Form.Salary.EmployeeSalaryRecordForm form = new Form.Salary.EmployeeSalaryRecordForm(FormTypes.Audit, tmpEnt.EMPLOYEESALARYRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                //browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
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
                                // txtCol.MaxWidth = 100;
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
                    if (i == 0)
                    {
                        gdSalaryRecord.Children.Add(DtGriddy);
                    }
                    else
                    {
                        gdSalaryRecord.Children.Add(DtGriddy);
                        gdSalaryRecord.Loaded += new RoutedEventHandler(gdSalaryRecord_Loaded);
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

        void gdSalaryRecord_Loaded(object sender, RoutedEventArgs e)
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

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DtGriddy.SelectedItems.Count > 0)
                {
                    T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
                    //client.FBStatisticsAsync(tmpEnt.EMPLOYEEID, Convert.ToInt32(tmpEnt.SALARYYEAR), Convert.ToInt32(tmpEnt.SALARYMONTH), (object)tmpEnt.EMPLOYEESALARYRECORDID);
                    Form.Salary.EmployeeSalaryRecordForm form = new SMT.HRM.UI.Form.Salary.EmployeeSalaryRecordForm(FormTypes.Browse, tmpEnt.EMPLOYEESALARYRECORDID);
                    //form.StandardItemWinForm.ToolBar.IsEnabled =true;
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Browse;
                    //browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            catch
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

        }


        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            //LoadAutoData();
            //LoadData();
            //BindTree();
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

        void client_FBStatisticsCompleted(object sender, FBStatisticsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result[0] == "false")
                {
                    FBSign = true;
                    recordFB = e.Result[1];
                }
                else
                {
                    string employeesalaryrecordid = e.UserState as string;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选择某审核状态是重新加载数据

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_EMPLOYEESALARYRECORD");
                Checkstate = dict.DICTIONARYVALUE.ToString();
                LoadSalaryRecordData();
                //LoadData();
            }
            ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
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

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.GenerateSalaryForm form = new SMT.HRM.UI.Form.Salary.GenerateSalaryForm();
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinWidth = 500;
            browser.MinHeight = 480;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (FBSign)
            {
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OVERBUDGET", recordFB),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                form.RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        void browser_ReloadDataEvent()
        {
            LoadSalaryRecordData();
            //LoadData();
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
                loadbar.Stop();
                return;
            }
            #region
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
            //    loadbar.Stop();
            //    //if (frist) frist = false;  else  Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "PLEASESELECT");
            //    return;
            //}
            #endregion
            string strState = "";
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            //DatePicker dpstarttimes = Utility.FindChildControl<DatePicker>(expander, "dpstarttime");
            //DatePicker dpendtimes = Utility.FindChildControl<DatePicker>(expander, "dpendtime");
            NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
            NumericUpDown nuEndYear = Utility.FindChildControl<NumericUpDown>(expander, "NuEndyear");
            NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
            NumericUpDown nuEndmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuEndmounth");

            #region 多月份的过滤 后备代码
            //try
            //{   
            starttimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), 1);
            //    if (nuYear.Value > nuEndYear.Value)
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始年份不能大于结束年份"));
            //        loadbar.Stop();
            //        return;
            //    }
            //    if ((nuYear.Value == nuEndYear.Value) && (nuStartmounth.Value > nuEndmounth.Value))
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始月份不能小于结束月份"));
            //        loadbar.Stop();
            //        return;
            //    }
            //    if (nuEndmounth.Value.ToInt32() == 12) endtimes = new DateTime(nuEndYear.Value.ToInt32()+1,1,1); else endtimes = new DateTime(nuEndYear.Value.ToInt32(), nuEndmounth.Value.ToInt32(), 1);
            //}
            //catch (Exception ex)
            //{
            //    ex.Message.ToString();
            //}
            #endregion

            //if (nuStartmounth.Value.ToInt32() == 12) endtimes = new DateTime(nuYear.Value.ToInt32() + 1, 1, 1); else endtimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), 1);
            endtimes = new DateTime(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32(), DateTime.DaysInMonth(nuYear.Value.ToInt32(), nuStartmounth.Value.ToInt32()));

            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                // filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                //filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                filter += " EMPLOYEENAME.Contains(@" + paras.Count().ToString() + ")";
                paras.Add(txtName.Text.Trim());
            }

            #region  薪资标准过滤
            //if (!string.IsNullOrEmpty(salaryStandardid))    //过滤测试时候注释
            //{
            //    if (!string.IsNullOrEmpty(filter)) filter += " and ";
            //    filter += "SALARYSTANDARDID==@" + paras.Count().ToString();
            //    paras.Add(salaryStandardid);
            //}
            //else
            //{
            //    loadbar.Stop();
            //    return;
            //}
            #endregion

            client.GetAutoEmployeeSalaryRecordPagingsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), sType, sValue, strState, userID);
        }

        private void LoadAutoData()
        {
            loadbar.Start();
            recordnum = 0;
            sign = true;
            getItemID.Clear();
            UIElement uiRemove = this.FindName("DtGriddy") as UIElement;
            if (uiRemove != null)
            {
                gdSalaryRecord.Children.Remove(uiRemove);
            }
            
            DtGriddy = new DataGrid();
            DtGriddy.Name = "DtGriddy";

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
            //DtGriddy.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //DtGriddy.Margin = new Thickness(0, 0, 0, 10);  

            //为DtGriddy附加属性：Grid.Row = 0, Grid.Column = 4
            Grid.SetRow(DtGriddy, 0);
            Grid.SetColumn(DtGriddy, 4);
            
            DtGriddy.IsReadOnly = true;
            DtGriddy.FrozenColumnCount = 4;
            #region  初始化必需项 NEW
            DataGridTextColumn col = new DataGridTextColumn();
            col.Header = Utility.GetResourceStr("EMPLOYEESALARYRECORDID");
            col.Binding = new Binding("EMPLOYEESALARYRECORDID");
            col.Visibility = Visibility.Collapsed;
            DtGriddy.Columns.Add(col);

            #region 序号列
            DataGridTemplateColumn colNo = new DataGridTemplateColumn();
            colNo.Header = Utility.GetResourceStr("序号");
            colNo.CellEditingTemplate = (DataTemplate)Resources["DataTemplateTextBlock"];
            colNo.IsReadOnly = true;
            DtGriddy.Columns.Add(colNo);
            #endregion

            DataGridTemplateColumn colchb = new DataGridTemplateColumn();
            colchb.Header = "操作";
            colchb.CellEditingTemplate = (DataTemplate)Resources["dt"];
            //colchb.HeaderStyle = Application.Current.Resources["DataGridCheckBoxColumnHeaderStyle"] as Style;
            DtGriddy.Columns.Add(colchb);

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

            //DataGridTextColumn col5 = new DataGridTextColumn();
            //col5.Header = Utility.GetResourceStr("CHECKSTATE");
            //col5.Binding = new Binding("CHECKSTATE");
            //DtGriddy.Columns.Add(col5);

            //DataGridTextColumn col34 = new DataGridTextColumn();
            //col34.Header = Utility.GetResourceStr("REMARK");
            //col34.Binding = new Binding("REMARK");
            //DtGriddy.Columns.Add(col34);

            //DataGridTextColumn col5 = new DataGridTextColumn(); 
            //col5.Header = Utility.GetResourceStr("CHECKSTATE");
            //col5.Binding = new Binding("CHECKSTATE");   //("{Binding CHECKSTATE,Converter={StaticResource DictionaryConverter},ConverterParameter=CHECKSTATE}");         
            //DtGriddy.Columns.Add(col5);
            #endregion

            DtGriddy.LoadingRow += new EventHandler<DataGridRowEventArgs>(DtGriddy_LoadingRow);
            DtGriddy.ItemsSource = list;
            recordPoint = DtGriddy.Columns.Count;
            if (list != null)
                client.GetEmployeeSalaryRecordItemByIDAsync(list[recordnum].EMPLOYEESALARYRECORDID);
            else
            {
                gdSalaryRecord.Children.Add(DtGriddy);
                loadbar.Stop();
            }
        }

        void DtGriddy_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            TextBlock tborder = DtGriddy.Columns[1].GetCellContent(e.Row).FindName("tbNO") as TextBlock;
            if (tborder != null)
            {
                tborder.Text = (e.Row.GetIndex() + 1).ToString();
            }
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            DateTime? starttimes;
            DateTime? endtimes = DateTime.Now.Date;
            if (DateTime.Now.Month < 3)
                starttimes = new DateTime(DateTime.Now.Year - 1, 1, 1);
            else
                starttimes = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 3, 1);
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            string strState = "";
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            DatePicker dpstarttimes = Utility.FindChildControl<DatePicker>(expander, "dpstarttime");
            DatePicker dpendtimes = Utility.FindChildControl<DatePicker>(expander, "dpendtime");
            try
            {
                starttimes = dpstarttimes.SelectedDate;
                endtimes = dpendtimes.SelectedDate;
                if (starttimes > endtimes)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始时间不能小于结束时间"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始时间不能小于结束时间"));
                    loadbar.Stop();
                    return;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                paras.Add(txtName.Text.Trim());
            }
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " AND ";
                }
                filter += "CHECKSTATE==@" + paras.Count().ToString();
                paras.Add(Checkstate);
            }
            dataPager.PageIndex = 1;
            client.GetEmployeeSalaryRecordPagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), strState, userID);

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DtGriddy.SelectedItems.Count > 0)
                {
                    T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
                    if (tmpEnt.CHECKSTATE == "0" || tmpEnt.CHECKSTATE == "3")
                    {
                        //if (string.IsNullOrEmpty(tmpEnt.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID))
                        //{
                        client.FBStatisticsAsync(tmpEnt.EMPLOYEEID, Convert.ToInt32(tmpEnt.SALARYYEAR), Convert.ToInt32(tmpEnt.SALARYMONTH), (object)tmpEnt.EMPLOYEESALARYRECORDID);
                        Form.Salary.EmployeeSalaryRecordForm form = new SMT.HRM.UI.Form.Salary.EmployeeSalaryRecordForm(FormTypes.Edit, tmpEnt.EMPLOYEESALARYRECORDID);
                        EntityBrowser browser = new EntityBrowser(form);
                        browser.FormType = FormTypes.Edit;
                        //browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                        //}
                        //else
                        //{ 

                        //}
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTEDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTEDIT"));
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                }
            }
            catch
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"));
            }

            #region ----
            //if (DtGrid.SelectedItems.Count > 0)
            //{
            //    T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
            //    //if (tmpEnt.CHECKSTATE == "0" || tmpEnt.CHECKSTATE == "3")
            //    //{
            //        client.FBStatisticsAsync(tmpEnt.EMPLOYEEID, Convert.ToInt32(tmpEnt.SALARYYEAR), Convert.ToInt32(tmpEnt.SALARYMONTH), (object)tmpEnt.EMPLOYEESALARYRECORDID);
            //        Form.Salary.EmployeeSalaryRecordForm form = new SMT.HRM.UI.Form.Salary.EmployeeSalaryRecordForm(FormTypes.Edit, tmpEnt.EMPLOYEESALARYRECORDID);
            //        EntityBrowser browser = new EntityBrowser(form);
            //        browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            //        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //        browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            //    //}
            //    //else 
            //    //{
            //    //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTEDIT"));
            //    //}
            //}
            //else 
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            //}
            #endregion
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //ComfirmBox deleComfir = new ComfirmBox();
            //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
            //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            //deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK_Click);
            //deleComfir.Show();  

            try
            {
                string Result = "";
                if (DtGriddy.SelectedItems.Count > 0)
                {
                    T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
                    if (tmpEnt.CHECKSTATE == "0" || tmpEnt.CHECKSTATE == "3")
                    {
                        ObservableCollection<string> ids = new ObservableCollection<string>();

                        foreach (T_HR_EMPLOYEESALARYRECORD tmp in DtGriddy.SelectedItems)
                        {
                            ids.Add(tmp.EMPLOYEESALARYRECORDID);
                            if (!(tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() || tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString()))
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                                return;
                            }
                        }
                        recorddel = DtGriddy.SelectedItems.Count;
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            loadbar.Start();
                            client.EmployeeSalaryRecordOrItemDeleteAsync(ids);
                            //foreach (string id in ids)
                            //    client.EmployeeSalaryRecordItemDeleteAsync(id, (object)id);
                        };
                        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                }
            }
            catch
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"));
            }

            #region -----
            //string Result = "";
            //if (DtGrid.SelectedItems.Count > 0)
            //{
            //    T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGrid.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
            //    if (tmpEnt.CHECKSTATE == "0" || tmpEnt.CHECKSTATE == "3")
            //    {
            //        ObservableCollection<string> ids = new ObservableCollection<string>();

            //        foreach (T_HR_EMPLOYEESALARYRECORD tmp in DtGrid.SelectedItems)
            //        {
            //            ids.Add(tmp.EMPLOYEESALARYRECORDID);
            //        }

            //        ComfirmWindow com = new ComfirmWindow();
            //        com.OnSelectionBoxClosed += (obj, result) =>
            //        {
            //            client.EmployeeSalaryRecordDeleteAsync(ids);
            //        };
            //        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //    }
            //    else 
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
            //    }
            //}
            //else 
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            //}
            #endregion

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


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///TODO:ADD 审核  
                if (DtGriddy.SelectedItems.Count > 0)
                {
                    T_HR_EMPLOYEESALARYRECORD tmpEnt = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
                    if (tmpEnt.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                        client.FBStatisticsAsync(tmpEnt.EMPLOYEEID, Convert.ToInt32(tmpEnt.SALARYYEAR), Convert.ToInt32(tmpEnt.SALARYMONTH), (object)tmpEnt.EMPLOYEESALARYRECORDID);
                    Form.Salary.EmployeeSalaryRecordForm form = new Form.Salary.EmployeeSalaryRecordForm(FormTypes.Audit, tmpEnt.EMPLOYEESALARYRECORDID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
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
                        return;
                    }
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

                    //Form.Salary.SalaryRecordMassAudit form = new Form.Salary.SalaryRecordMassAudit(FormTypes.Audit, sType, sValue, nuYear.Value.ToString(), nuStartmounth.Value.ToString(), state);
                    strCollection = sType.ToString() + "," + sValue + "," + nuYear.Value.ToString() + "," + nuStartmounth.Value.ToString() + "," + state;
                    Form.Salary.SalaryRecordMassAudit form = new Form.Salary.SalaryRecordMassAudit(FormTypes.Audit, strCollection);


                    EntityBrowser browser = new EntityBrowser(form);
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    if ((DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD).CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                        browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
                    browser.FormType = FormTypes.Audit;
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }

        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_EMPLOYEESALARYRECORD");
        }


        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            salaryStandardid = string.Empty;
            //LoadData();
            LoadStandard();
            LoadSalaryRecordData();
        }

        void client_FilterStandardCompleted(object sender, FilterStandardCompletedEventArgs e)
        {
            List<T_HR_SALARYSTANDARD> list = new List<T_HR_SALARYSTANDARD>();
            list = e.UserState as List<T_HR_SALARYSTANDARD>;
            //DtGridStand.ItemsSource = list;
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

                    //DtGridStand.ItemsSource = list;
                    //dataPagerStand.PageCount = e.pageCount;
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
                    //DtGridStand.ItemsSource = null;
                }
            }

            //loadbar.Stop();
        }

        void LoadStandard()
        {
            UIElement uiRemove = this.FindName("DtGriddy") as UIElement;
            if (uiRemove != null)
            {
                gdSalaryRecord.Children.Remove(uiRemove);
            }
            //SpSalaryRecord.Children.Clear();
            //loadbar.Start();
            int pageCount = 0;
            string filter = "";
            string strState = "2";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            //TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            //if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            //{
            //    filter += "SALARYSTANDARDNAME==@" + paras.Count().ToString();
            //    paras.Add(txtName.Text.Trim());
            //}
            //if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            //{
            //    strState = Checkstate;
            //}


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
                loadbar.Stop();
                return;
            }

            //client.GetSalaryStandardPagingAsync(dataPagerStand.PageIndex, dataPagerStand.PageSize, "SALARYSTANDARDNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, sValue, strState);
        }

        private void DtGridStand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (DtGridStand.SelectedItems.Count > 0)
            //{
            //    selectedStand = DtGridStand.SelectedItems[0] as T_HR_SALARYSTANDARD;
            //    salaryStandardid = selectedStand.SALARYSTANDARDID;
            //    LoadSalaryRecordData();
            //}
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

        private void HyperlinkButton_MouseLeave(object sender, MouseEventArgs e)
        {
            //hidePasswordStoryboard.Begin();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            T_HR_EMPLOYEESALARYRECORD temp = DtGriddy.SelectedItems[0] as T_HR_EMPLOYEESALARYRECORD;
            txtbDetail.Text = temp.SALARYYEAR + Utility.GetResourceStr("YEAR") + temp.SALARYMONTH + Utility.GetResourceStr("MONTH") + "  " + temp.EMPLOYEENAME;
            client.SalaryContrastAllAsync(temp.EMPLOYEESALARYRECORDID);
            //ShowPasswordStoryboard.Begin();
        }

        private void hbClose_Click(object sender, RoutedEventArgs e)
        {
            hidePasswordStoryboard.Begin();
        }

    }
}
