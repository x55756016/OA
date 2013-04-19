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
    public partial class SalaryPayImport : BasePage,IClient
    {
        private bool frist = true;
        SalaryServiceClient client;
        byte[] byExport;
        private bool sign = false;
        private int recordnum = 0;
        private int recordPoint = 0;
        SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient orgClient;
        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;
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
        public SalaryPayImport()
        {
            InitializeComponent();
            GetEntityLogo("T_HR_EMPLOYEESALARYRECORD");
            InitPara();
            LoadSalaryRecordData();
            //LoadData();
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
                client.ExportExcelCompleted += new EventHandler<ExportExcelCompletedEventArgs>(client_ExportExcelCompleted);
                client.ImportExcelCompleted += new EventHandler<ImportExcelCompletedEventArgs>(client_ImportExcelCompleted);
                client.ReadExcelCompleted += new EventHandler<ReadExcelCompletedEventArgs>(client_ReadExcelCompleted);
                this.Loaded += new RoutedEventHandler(Left_Loaded);

                client.GetSalaryStandardPagingCompleted += new EventHandler<GetSalaryStandardPagingCompletedEventArgs>(client_GetSalaryStandardPagingCompleted);

                orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
                orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
            #region 工具栏初试化
            ToolBar.txtOtherName.Visibility = Visibility.Collapsed;          
           // ToolBar.txtOtherName.Text = string.Empty;
           
            ToolBar.btnImport.Visibility = Visibility.Visible;
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
            ToolBar.retAudit.Visibility = Visibility.Visible;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.retRefresh.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            
            #region //新增控件(发放成功标记)
            Rectangle _Rectangle = new Rectangle();
            _Rectangle.Height = 18;
            _Rectangle.Width = 1;
            _Rectangle.Fill = new SolidColorBrush(Color.FromArgb(255,154,154,153));
            _Rectangle.Margin = new Thickness(3,4,0,3);

            TextBlock _txtblock = new TextBlock();
            _txtblock.Text = "发放成功标记";
            _txtblock.VerticalAlignment = VerticalAlignment.Center;
            _txtblock.Margin = new Thickness(4,0,0,0);

            _txtBox.Margin = new Thickness(3,0,0,0);
            _txtBox.Width = 100;
            _txtBox.Height = 24;
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

            //ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("PAY")).Click += new RoutedEventHandler(Payment_Click);
            #endregion
        }

        void Left_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSalaryRecordData();
            //LoadData();
            BindTree();
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
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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

        void client_ImportExcelCompleted(object sender, ImportExcelCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_EMPLOYEESALARYRECORD> lists = e.Result.ToList();
                if (lists.Count > 0)
                {
                    list = lists;
                    LoadAutoData();
                }
                //dataPager.PageCount = e.pageCount;
                string resultstr = Utility.GetResourceStr("NUMBEROFTRANSACTIONS") + e.successcount.ToString();
                resultstr += " , " + Utility.GetResourceStr("UNSUCCESSFULLOTS") + e.failcount;
               // ToolBar.txtOtherName.Text = resultstr;
                _Textshow.Text = resultstr;
                dataPager.PageSize = (e.Result.Count <= 20) ? 200 : e.Result.Count;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DATAIMPORT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DATAIMPORT"));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXCELUPLOADFILEERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXCELUPLOADFILEERROR"));
            }
            loadbar.Stop();
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
            NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "years");
            NumericUpDown nuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "months");
            if (string.IsNullOrEmpty(_txtBox.Text))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR", "发放标记不能为空"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), "发放标记不能为空"); //第四,五列发放标记不能为空
                return;
            }
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
                client.ImportExcelAsync(UploadFile,nuYear.Value.ToString(),nuStartmounth.Value.ToString(),_txtBox.Text.Trim());
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_ExportExcelCompleted(object sender, ExportExcelCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //if (!string.IsNullOrEmpty(e.strMsg))
                //{
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));
                //    return;
                //}
                if (e.Result != null)
                    byExport = e.Result;
                else
                {
                    byExport = null;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ISNOT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ISNOT"));
                }
            }
            else
            {
                byExport = null;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
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
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "0":
                        OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
                        sType = 0;
                        sValue = company.COMPANYID;
                        break;
                    case "1":
                        OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as OrganizationWS.T_HR_DEPARTMENT;
                        sType = 1;
                        sValue = department.DEPARTMENTID;
                        break;
                    case "2":
                        OrganizationWS.T_HR_POST post = selectedItem.DataContext as OrganizationWS.T_HR_POST;
                        sType = 2;
                        sValue = post.POSTID;
                        break;
                }
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
            ViewTitles.TextTitle.Text = GetTitleFromURL("月薪 >>薪资发放数据导入");


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
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "0":
                        OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
                        sType = 0;
                        sValue = company.COMPANYID;
                        break;
                    case "1":
                        OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as OrganizationWS.T_HR_DEPARTMENT;
                        sType = 1;
                        sValue = department.DEPARTMENTID;
                        break;
                    case "2":
                        OrganizationWS.T_HR_POST post = selectedItem.DataContext as OrganizationWS.T_HR_POST;
                        sType = 2;
                        sValue = post.POSTID;
                        break;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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

                if (cbxPaystate != null && cbxPaystate.SelectedIndex > 0)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "PAYCONFIRM==@" + parass.Count().ToString();
                    parass.Add((cbxPaystate.SelectedIndex - 1).ToString());
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CHECKSTATE==@" + parass.Count().ToString();
                parass.Add(CHECKSTATEED.ToString());

                if (cbxPaystate != null && cbxPaystate.SelectedIndex > 0)
                {
                    filter += "PAYCONFIRM==@" + parass.Count().ToString();
                    parass.Add((cbxPaystate.SelectedIndex - 1).ToString());
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
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("EMPLOYEESALARYRECORD"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("EMPLOYEESALARYRECORD"));
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
            LoadData();
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
                        break;
                    case "1":
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        sType = 1;
                        sValue = department.DEPARTMENTID;
                        break;
                    case "2":
                        SMT.Saas.Tools.OrganizationWS.T_HR_POST post = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                        sType = 2;
                        sValue = post.POSTID;
                        break;
                }
            }

            client.GetSalaryStandardPagingAsync(dataPagerStand.PageIndex, dataPagerStand.PageSize, "SALARYSTANDARDNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, sType, sValue, strState);
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
            if (DateTime.Now.Month < 3)
                starttimes = new DateTime(DateTime.Now.Year - 1, 1, 1);
            else
                starttimes = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 3, 1);

            DtGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "0":
                        OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
                        sType = 0;
                        sValue = company.COMPANYID;
                        break;
                    case "1":
                        OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as OrganizationWS.T_HR_DEPARTMENT;
                        sType = 1;
                        sValue = department.DEPARTMENTID;
                        break;
                    case "2":
                        OrganizationWS.T_HR_POST post = selectedItem.DataContext as OrganizationWS.T_HR_POST;
                        sType = 2;
                        sValue = post.POSTID;
                        break;
                }
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
                if (nudmonths.Value.ToInt32() == 12) endtimes = new DateTime(nudyears.Value.ToInt32() + 1, 1, 1); else endtimes = new DateTime(nudyears.Value.ToInt32(), nudmonths.Value.ToInt32(), 1);
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

            //if (cbxPaystate != null && cbxPaystate.SelectedIndex > 0)
            //{
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            filter += "PAYCONFIRM==@" + paras.Count().ToString();
            paras.Add("2");
            //}

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
            DtGriddy.IsReadOnly = true;

            #region  初始化必需项
            //DataGridTextColumn col = new DataGridTextColumn();
            //col.Header = Utility.GetResourceStr("EMPLOYEESALARYRECORDID");
            //col.Binding = new Binding("EMPLOYEESALARYRECORDID");
            //col.Visibility = Visibility.Collapsed;
            //DtGriddy.Columns.Add(col);

            //DataGridTextColumn col2 = new DataGridTextColumn();
            //col2.Header = Utility.GetResourceStr("EMPLOYEENAME");
            //col2.Binding = new Binding("EMPLOYEENAME");
            //DtGriddy.Columns.Add(col2);

            //DataGridTextColumn col8 = new DataGridTextColumn();
            //col8.Header = Utility.GetResourceStr("ACTUALLYPAY");
            //col8.Binding = new Binding("ACTUALLYPAY");
            //DtGriddy.Columns.Add(col8);

            //DataGridTextColumn col7 = new DataGridTextColumn();
            //col7.Header = Utility.GetResourceStr("SALARYYEAR");
            //col7.Binding = new Binding("SALARYYEAR");
            //DtGriddy.Columns.Add(col7);

            //DataGridTextColumn col4 = new DataGridTextColumn();
            //col4.Header = Utility.GetResourceStr("SALARYMONTH");
            //col4.Binding = new Binding("SALARYMONTH");
            //DtGriddy.Columns.Add(col4);

            //DataGridTextColumn col5 = new DataGridTextColumn();
            //col5.Header = Utility.GetResourceStr("PERSONALSICOST");
            //col5.Binding = new Binding("PERSONALSICOST");
            //DtGriddy.Columns.Add(col5);

            //DataGridTextColumn col6 = new DataGridTextColumn();
            //col6.Header = Utility.GetResourceStr("PERSONALINCOMETAX");
            //col6.Binding = new Binding("PERSONALINCOMETAX");
            //DtGriddy.Columns.Add(col6);

            //DataGridTextColumn col9 = new DataGridTextColumn();
            //col9.Header = Utility.GetResourceStr("ATTENDANCEUNUSUALDEDUCT");
            //col9.Binding = new Binding("ATTENDANCEUNUSUALDEDUCT");
            //DtGriddy.Columns.Add(col9);

            //DataGridTextColumn col10 = new DataGridTextColumn();
            //col10.Header = Utility.GetResourceStr("ATTENDANCEUNUSUALTIME");
            //col10.Binding = new Binding("ATTENDANCEUNUSUALTIME");
            //DtGriddy.Columns.Add(col10);

            //DataGridTextColumn col11 = new DataGridTextColumn();
            //col11.Header = Utility.GetResourceStr("ATTENDANCEUNUSUALTIMES");
            //col11.Binding = new Binding("ATTENDANCEUNUSUALTIMES");
            //DtGriddy.Columns.Add(col11);

            //DataGridTextColumn col12 = new DataGridTextColumn();
            //col12.Header = Utility.GetResourceStr("OVERTIMETIMES");
            //col12.Binding = new Binding("OVERTIMETIMES");
            //DtGriddy.Columns.Add(col12);

            //DataGridTextColumn col13 = new DataGridTextColumn();
            //col13.Header = Utility.GetResourceStr("OVERTIMESUM");
            //col13.Binding = new Binding("OVERTIMESUM");
            //DtGriddy.Columns.Add(col13);

            //DataGridTextColumn col14 = new DataGridTextColumn();
            //col14.Header = Utility.GetResourceStr("ABSENTTIMES");
            //col14.Binding = new Binding("ABSENTTIMES");
            //DtGriddy.Columns.Add(col14);

            //DataGridTextColumn col15 = new DataGridTextColumn();
            //col15.Header = Utility.GetResourceStr("ABSENTFROMWORKPAY");
            //col15.Binding = new Binding("ABSENTDEDUCT");
            //DtGriddy.Columns.Add(col15);

            //DataGridTextColumn col16 = new DataGridTextColumn();
            //col16.Header = Utility.GetResourceStr("LEAVETIME");
            //col16.Binding = new Binding("LEAVETIME");
            //DtGriddy.Columns.Add(col16);

            //DataGridTextColumn col17 = new DataGridTextColumn();
            //col17.Header = Utility.GetResourceStr("LEAVEDEDUCT");
            //col17.Binding = new Binding("LEAVEDEDUCT");
            //DtGriddy.Columns.Add(col17);

            //DataGridTextColumn col18 = new DataGridTextColumn();
            //col18.Header = Utility.GetResourceStr("EVECTIONTIMES");
            //col18.Binding = new Binding("EVECTIONTIMES");
            //DtGriddy.Columns.Add(col18);

            //DataGridTextColumn col19 = new DataGridTextColumn();
            //col19.Header = Utility.GetResourceStr("EVECTIONSUBSIDY");
            //col19.Binding = new Binding("EVECTIONSUBSIDY");
            //DtGriddy.Columns.Add(col19);

            ////DataGridTextColumn col20 = new DataGridTextColumn();
            ////col20.Header = Utility.GetResourceStr("FIXEDINCOMESUM");
            ////col20.Binding = new Binding("FIXEDINCOMESUM");
            ////DtGriddy.Columns.Add(col20);

            //DataGridTextColumn col21 = new DataGridTextColumn();
            //col21.Header = Utility.GetResourceStr("ABSENCEDAYS");
            //col21.Binding = new Binding("ABSENCEDAYS");
            //DtGriddy.Columns.Add(col21);

            //DataGridTextColumn col22 = new DataGridTextColumn();
            //col22.Header = Utility.GetResourceStr("WORKINGSALARY");
            //col22.Binding = new Binding("WORKINGSALARY");
            //DtGriddy.Columns.Add(col22);

            //DataGridTextColumn col23 = new DataGridTextColumn();
            //col23.Header = Utility.GetResourceStr("SUBTOTAL");
            //col23.Binding = new Binding("SUBTOTAL");
            //DtGriddy.Columns.Add(col23);

            //DataGridTextColumn col24 = new DataGridTextColumn();
            //col24.Header = Utility.GetResourceStr("PRETAXSUBTOTAL");
            //col24.Binding = new Binding("PRETAXSUBTOTAL");
            //DtGriddy.Columns.Add(col24);

            //DataGridTextColumn col25 = new DataGridTextColumn();
            //col25.Header = Utility.GetResourceStr("BALANCE");
            //col25.Binding = new Binding("BALANCE");
            //DtGriddy.Columns.Add(col25);

            ////DataGridTextColumn col26 = new DataGridTextColumn();
            ////col26.Header = Utility.GetResourceStr("DEDUCTTOTAL");
            ////col26.Binding = new Binding("DEDUCTTOTAL");
            ////DtGriddy.Columns.Add(col26);

            //DataGridTextColumn col27 = new DataGridTextColumn();
            //col27.Header = Utility.GetResourceStr("PERFORMANCESUM");
            //col27.Binding = new Binding("PERFORMANCESUM");
            //DtGriddy.Columns.Add(col27);

            //DataGridTextColumn col28 = new DataGridTextColumn();
            //col28.Header = Utility.GetResourceStr("CUSTOMERSUM");
            //col28.Binding = new Binding("CUSTOMERSUM");
            //DtGriddy.Columns.Add(col28);

            ////DataGridTextColumn col5 = new DataGridTextColumn(); 
            ////col5.Header = Utility.GetResourceStr("CHECKSTATE");
            ////col5.Binding = new Binding("CHECKSTATE");   //("{Binding CHECKSTATE,Converter={StaticResource DictionaryConverter},ConverterParameter=CHECKSTATE}");         
            ////DtGriddy.Columns.Add(col5);
            #endregion

            #region  初始化必需项  22
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

        private void Login_HandlerClick(object sender, EventArgs e)
        {
            LayoutRoot.Visibility = Visibility.Visible;
        }

    }
}
