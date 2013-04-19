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
using System.Text;

using SMT.Saas.Tools.SalaryWS;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;

using SMT.HRM.UI.Form.Salary;
using System.Collections.ObjectModel;
using System.Windows.Data;

using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class SalaryPaidConfirm : BasePage, IClient
    {
        SalaryServiceClient client;
        private bool sign = false;
        private bool selectSign = false;
        private int recordnum = 0;
        private int recordPoint = 0;
        ImageButton _ImgButtonAllSelect = new ImageButton();
        ImageButton _ImgButtonSalaryPaidConfirm = new ImageButton();
        SMTLoading loadbar = new SMTLoading();
        //OrganizationServiceClient orgClient;
        //private List<T_HR_COMPANY> allCompanys;
        //private List<T_HR_DEPARTMENT> allDepartments;
        //private List<T_HR_POST> allPositions;
        string salaryStandardid = string.Empty;
        List<object> itemsAll = new List<object>();
        List<string> getItemID = new List<string>();
        List<T_HR_EMPLOYEESALARYRECORD> list = new List<T_HR_EMPLOYEESALARYRECORD>();
        private T_HR_SALARYSTANDARD selectedStand = new T_HR_SALARYSTANDARD();
        private T_HR_EMPLOYEESALARYRECORD salaryRecord = new T_HR_EMPLOYEESALARYRECORD();
        private DataGrid DtGriddy;
        private ObservableCollection<string> paymentIDs = new ObservableCollection<string>();
        public ObservableCollection<string> PaymentIDs
        {
            get { return paymentIDs; }
            set { paymentIDs = value; }
        }
        private const int CHECKSTATEED = 2;
        private TextBox _txtBox = new TextBox();
        private TextBlock _Textshow = new TextBlock();

        public SalaryPaidConfirm()
        {
            InitializeComponent();
            GetEntityLogo("T_HR_EMPLOYEESALARYRECORD");
            InitPara();
            // LoadSalaryRecordData();
        }

        public void InitPara()
        {
            PARENT.Children.Add(loadbar);
            try
            {
                client = new SalaryServiceClient();
                client.GetAutoEmployeeSalaryRecordPagingsCompleted += new EventHandler<GetAutoEmployeeSalaryRecordPagingsCompletedEventArgs>(client_GetAutoEmployeeSalaryRecordPagingsCompleted);
                client.GetEmployeeSalaryRecordItemByIDCompleted += new EventHandler<GetEmployeeSalaryRecordItemByIDCompletedEventArgs>(client_GetEmployeeSalaryRecordItemByIDCompleted);
                client.PaymentUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PaymentUpdateCompleted);
                client.GetPaymentPagingsCompleted += new EventHandler<GetPaymentPagingsCompletedEventArgs>(client_GetPaymentPagingsCompleted);
                client.GetPaymentPagingCompleted += new EventHandler<GetPaymentPagingCompletedEventArgs>(client_GetPaymentPagingCompleted);
                this.Loaded += new RoutedEventHandler(Left_Loaded);
                treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
                client.PaymentConfirmUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PaymentConfirmUpdateCompleted);
                client.GetSalaryStandardPagingCompleted += new EventHandler<GetSalaryStandardPagingCompletedEventArgs>(client_GetSalaryStandardPagingCompleted);

                //orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
                //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
            #region 工具栏初试化
            ToolBar.txtOtherName.Visibility = Visibility.Collapsed;
            ToolBar.btnImport.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
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

            ToolBarTwo.txtOtherName.Visibility = Visibility.Collapsed;
            ToolBarTwo.btnImport.Visibility = Visibility.Collapsed;
            ToolBarTwo.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBarTwo.btnNew.Visibility = Visibility.Collapsed;

            ToolBarTwo.btnDelete.Visibility = Visibility.Collapsed;
            ToolBarTwo.btnRefresh.Visibility = Visibility.Collapsed;

            ToolBarTwo.btnSumbitAudit.Visibility = Visibility.Collapsed;
            ToolBarTwo.btnAudit.Visibility = Visibility.Collapsed;
            ToolBarTwo.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBarTwo.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBarTwo.btnEdit.Visibility = Visibility.Collapsed;
            ToolBarTwo.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBarTwo.BtnView.Visibility = Visibility.Collapsed;
            ToolBarTwo.retNew.Visibility = Visibility.Collapsed;
            ToolBarTwo.retEdit.Visibility = Visibility.Collapsed;
            ToolBarTwo.retAudit.Visibility = Visibility.Collapsed;
            ToolBarTwo.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBarTwo.retPDF.Visibility = Visibility.Collapsed;
            ToolBarTwo.retDelete.Visibility = Visibility.Collapsed;
            ToolBarTwo.retRefresh.Visibility = Visibility.Collapsed;
            ToolBarTwo.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            _ImgButtonAllSelect.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonAllSelect.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("全选")).Click += new RoutedEventHandler(_ImgButtonAllSelect_Click);
            ToolBarTwo.stpOtherAction.Children.Add(_ImgButtonAllSelect);

            _ImgButtonSalaryPaidConfirm.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonSalaryPaidConfirm.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("发放确认")).Click += new RoutedEventHandler(_ImgButtonSalaryPaidConfirm_Click);
            ToolBarTwo.stpOtherAction.Children.Add(_ImgButtonSalaryPaidConfirm);

            #endregion
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            salaryStandardid = string.Empty;
            LoadStandard();
            LoadSalaryRecordData();
        }

        void _ImgButtonAllSelect_Click(object sender, RoutedEventArgs e)
        {
            if (DtGriddy == null || DtGriddy.ItemsSource == null) return;
            if (DtGriddy.ItemsSource != null)
            {
                foreach (object v in DtGriddy.ItemsSource)
                {
                    CheckBox cb = DtGriddy.Columns[1].GetCellContent(v).FindName("CheckBoxs") as CheckBox;
                    if (cb.IsChecked == true)
                    {
                        selectSign = false;
                        break;
                    }
                    else
                    {
                        selectSign = true;
                    }
                }
                foreach (object v in DtGriddy.ItemsSource)
                {
                    CheckBox cb = DtGriddy.Columns[1].GetCellContent(v).FindName("CheckBoxs") as CheckBox;
                    cb.IsChecked = selectSign;
                }
            }
        }

        void _ImgButtonSalaryPaidConfirm_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGriddy == null || DtGriddy.ItemsSource == null) return;
            ObservableCollection<T_HR_EMPLOYEESALARYRECORD> temp = new ObservableCollection<T_HR_EMPLOYEESALARYRECORD>();
            foreach (object v in DtGriddy.ItemsSource)
            {
                T_HR_EMPLOYEESALARYRECORD ent = v as T_HR_EMPLOYEESALARYRECORD;
                CheckBox cb = DtGriddy.Columns[1].GetCellContent(v).FindName("CheckBoxs") as CheckBox;
                if (cb.IsChecked == true)
                {
                    ent.PAYCONFIRM = "2";
                    ent.PAIDDATE = System.DateTime.Now;
                    ent.PAIDBY = "Adminstrator";
                    temp.Add(ent);
                }
            }
            if (temp.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.PaymentConfirmUpdateAsync(temp);
                    loadbar.Start();
                };
                com.SelectionBox(Utility.GetResourceStr("CONFIRM"), Utility.GetResourceStr("PAYALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void client_PaymentConfirmUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadSalaryRecordData();
            }
        }

        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
            // BindTree();
        }
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
        }

        void client_GetSalaryStandardPagingCompleted(object sender, GetSalaryStandardPagingCompletedEventArgs e)
        {
            List<T_HR_SALARYSTANDARD> list = new List<T_HR_SALARYSTANDARD>();
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
            List<V_EMPLOYEESALARYRECORDITEM> items = new List<V_EMPLOYEESALARYRECORDITEM>();
            List<V_EMPLOYEESALARYRECORDITEM> its = new List<V_EMPLOYEESALARYRECORDITEM>();

            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    //items = e.Result.ToList();
                    items = e.Result.OrderBy(m => m.ORDERNUMBER).ToList();
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].SUM = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(items[i].SUM);
                    }
                    if (sign)
                    {
                        foreach (var it in items)
                        {
                            DataGridTextColumn txtCol = new DataGridTextColumn();
                            txtCol.Header = it.SALARYITEMNAME;
                            txtCol.Binding = new Binding("SUM");
                            txtCol.Width = DataGridLength.SizeToCells;
                            txtCol.MinWidth = 100;
                            txtCol.MaxWidth = 100;
                            txtCol.IsReadOnly = true;
                            DtGriddy.Columns.Add(txtCol);
                            getItemID.Add(it.SALARYITEMID);
                        }
                        sign = false;
                        its = items;
                    }
                    else
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (i > getItemID.Count - 1) break;
                            foreach (var it in items)
                            {
                                if (it.SALARYITEMID == getItemID[i])
                                {
                                    its.Add(it);
                                    break;
                                }
                            }
                        }
                    }

                }
            }
            itemsAll.Add(its);
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
                    catch
                    {
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
                if (i == 0) SpSalaryRecord.Children.Add(DtGriddy);
                else
                {
                    SpSalaryRecord.Children.Add(DtGriddy);
                    SpSalaryRecord.Loaded += new RoutedEventHandler(SpSalaryRecord_Loaded);
                }
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

        void client_GetPaymentPagingsCompleted(object sender, GetPaymentPagingsCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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

        public void NewBtnPayment(string _imgs, string _name)
        {
            //ToolBar.stpOtherAction.Visibility = Visibility.Visible;
            //ToolBar.stpOtherAction.Children.Clear();
            //Button _btn = new Button();
            //_btn.Width = 80;
            //_btn.Height = 24.0;
            //StackPanel _sp = new StackPanel();
            //_sp.Orientation = Orientation.Horizontal;
            //Image _img = new Image();
            //_img.Width = 16.0;
            //_img.Height = 16.0;

            //_img.Source = new BitmapImage(new Uri(_imgs, UriKind.Relative));
            //TextBlock _tb = new TextBlock();
            //_tb.Text = _name;//实体名称
            //_sp.Children.Add(_img);
            //_sp.Children.Add(_tb);
            //_btn.Content = _sp;
            //ToolBar.stpOtherAction.Children.Add(_btn);
            //_btn.Click += new RoutedEventHandler(_btn_Click);
        }

        void _btn_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_PAYMENT tmpPayment = DtGrid.SelectedItems[0] as V_PAYMENT;

                if (paymentIDs.Count <= 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                    return;
                }
                if (Convert.ToInt32(tmpPayment.PAYCONFIRM.Trim()) <= 1)
                {
                    tmpPayment.PAYTYPE = "1";
                    tmpPayment.PAYCONFIRM = "2";
                    tmpPayment.PAIDDATE = System.DateTime.Now;
                    tmpPayment.PAIDBY = "Adminstrator";
                    //client.PaymentUpdateAsync(tmpPayment);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTPAY"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTPAY"));
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
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PAYSUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
            //LoadData();
        }

        void client_GetPaymentPagingCompleted(object sender, GetPaymentPagingCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            string years = "";
            string months = "";
            loadbar.Start();
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

            if (cbxPaystate != null && cbxPaystate.SelectedIndex > 0)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "PAYCONFIRM==@" + paras.Count().ToString();
                paras.Add((cbxPaystate.SelectedIndex - 1).ToString());
            }
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            filter += "CHECKSTATE==@" + paras.Count().ToString();
            paras.Add(CHECKSTATEED.ToString());
            client.GetPaymentPagingsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, years, months, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEESALARYRECORD", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL("月薪 >>薪资发放确认");


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
            //LoadData();    
        }

        void LoadStandard()
        {
            SpSalaryRecord.Children.Clear();
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
                if (cbxPaystate.SelectedIndex == 2 || cbxPaystate.SelectedIndex == 0)
                {
                    _ImgButtonSalaryPaidConfirm.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    _ImgButtonSalaryPaidConfirm.Visibility = System.Windows.Visibility.Visible;
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "PAYCONFIRM==@" + paras.Count().ToString();
                paras.Add(cbxPaystate.SelectedIndex.ToString());
            }
            else
            {
                loadbar.Stop();
                return;
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

            DataGridTemplateColumn colchb = new DataGridTemplateColumn();
            colchb.Header = Utility.GetResourceStr("SELECT");
            colchb.CellEditingTemplate = (DataTemplate)Resources["DataTemplates"];
            //colchb.HeaderStyle = Application.Current.Resources["DataGridCheckBoxColumnHeaderStyle"] as Style;
            DtGriddy.Columns.Add(colchb);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = Utility.GetResourceStr("EMPLOYEENAME");
            col2.Binding = new Binding("EMPLOYEENAME");
            col2.IsReadOnly = true;
            DtGriddy.Columns.Add(col2);

            DataGridTextColumn col8 = new DataGridTextColumn();
            col8.Header = Utility.GetResourceStr("ACTUALLYPAY");
            col8.Binding = new Binding("ACTUALLYPAY");
            col8.IsReadOnly = true;
            DtGriddy.Columns.Add(col8);

            DataGridTextColumn col7 = new DataGridTextColumn();
            col7.Header = Utility.GetResourceStr("SALARYYEAR");
            col7.Binding = new Binding("SALARYYEAR");
            col7.IsReadOnly = true;
            DtGriddy.Columns.Add(col7);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = Utility.GetResourceStr("SALARYMONTH");
            col4.Binding = new Binding("SALARYMONTH");
            col4.IsReadOnly = true;
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
        private List<int> recordRow = new List<int>();
        private void CheckBoxs_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            //MessageBox.Show((cb.Parent as T_HR_EMPLOYEESALARYRECORD).EMPLOYEENAME);
            //recordRow.Add(cb.Parent as );
        }

    }
}
