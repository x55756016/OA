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

using System.Collections.ObjectModel;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.SalaryWS;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.HRM.UI.Form.Personnel;
using System.Windows.Media.Imaging;
using System.IO;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class PensionDetail : BasePage, IClient
    {
        PersonnelServiceClient client;
        SalaryServiceClient clientSalary;
        ObservableCollection<string> strDelIDs = new ObservableCollection<string>();
        SMTLoading loadbar = new SMTLoading();
        ComfirmWindow com = new ComfirmWindow();
        string strDelResult = string.Empty;
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        public PensionDetail()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                InitEvent();
                this.LayoutRoot_Loaded(sender, args);
                GetEntityLogo("T_HR_PENSIONDETAIL");
            };
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_PENSIONDETAIL", false);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultData();
            UnVisibleGridToolControl();
        }

        private void InitEvent()
        {
            try
            {
                PARENT.Children.Add(loadbar);

                client = new PersonnelServiceClient();
                clientSalary = new SalaryServiceClient();
                client.PensionDetailPagingCompleted += new EventHandler<PensionDetailPagingCompletedEventArgs>(client_PensionDetailPagingCompleted);
                client.PensionDetailDeleteCompleted += new EventHandler<PensionDetailDeleteCompletedEventArgs>(client_PensionDetailDeleteCompleted);
                client.ExportPensionDetailReportCompleted += new EventHandler<ExportPensionDetailReportCompletedEventArgs>(client_ExportPensionDetailReportCompleted);
                clientSalary.CheckSalaryAuditStateCompleted += new EventHandler<CheckSalaryAuditStateCompletedEventArgs>(clientSalary_CheckSalaryAuditStateCompleted);

                ToolBar.btnImport.Visibility = Visibility.Visible;
                ToolBar.btnOutExcel.Visibility = Visibility.Collapsed;

                ToolBar.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);
                ToolBar.btnImport.Click += new RoutedEventHandler(btnImport_Click);
                //ToolBar.btnRead.Click += new RoutedEventHandler(btnRead_Click);
                ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
                //ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

                ImageButton ChangeMeetingBtn = new ImageButton();
                ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_service.png", UriKind.Relative));
                ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("导出社保报表");// 导出社保
                ChangeMeetingBtn.Image.Width = 16.0;
                ChangeMeetingBtn.Image.Height = 22.0;
                ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
                ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
                ChangeMeetingBtn.Click += new RoutedEventHandler(btnExportReports_Click);
                ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void clientSalary_CheckSalaryAuditStateCompleted(object sender, CheckSalaryAuditStateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!string.IsNullOrWhiteSpace(e.strMsg))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), e.strMsg, Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    return;
                }

                if (strDelIDs.Count > 0)
                {
                    client.PensionDetailDeleteAsync(strDelIDs);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_ExportPensionDetailReportCompleted(object sender, ExportPensionDetailReportCompletedEventArgs e)
        {
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
            loadbar.Stop();
        }

        void btnExportReports_Click(object sender, RoutedEventArgs e)
        {

            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;

            result = dialog.ShowDialog();
            if (result.Value == true)
            {

                int pageCount = 0;
                string filter = "";
                ObservableCollection<object> paras = new ObservableCollection<object>();

                TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
                TextBox txtCardID = Utility.FindChildControl<TextBox>(expander, "txtCardID");
                NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
                NumericUpDown NuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
                if (txtEmpName != null)
                {
                    if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
                    {
                        //  filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                        filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                        paras.Add(txtEmpName.Text.Trim());
                    }
                }
                if (txtCardID != null)
                {
                    if (!string.IsNullOrEmpty(txtCardID.Text.Trim()))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        //  filter += "CARDID==@" + paras.Count().ToString();
                        filter += " @" + paras.Count().ToString() + ".Contains(CARDID)";
                        paras.Add(txtCardID.Text.Trim());
                    }
                }
                if (nuYear != null)
                {
                    if (!string.IsNullOrEmpty(nuYear.Value.ToString()))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "PENSIONYEAR==@" + paras.Count().ToString();
                        paras.Add((decimal?)nuYear.Value);
                    }
                }
                if (NuStartmounth != null)
                {
                    if (!string.IsNullOrEmpty(NuStartmounth.Value.ToString()))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "PENSIONMOTH==@" + paras.Count().ToString();
                        paras.Add((decimal?)NuStartmounth.Value);
                    }
                }
                loadbar.Start();
                string EmployeeID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                string CompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                client.ExportPensionDetailReportAsync("CARDID", filter, paras, EmployeeID, CompanyID, nuYear.Value.ToString(), NuStartmounth.Value.ToString());

            }

        }


        void btnImport_Click(object sender, RoutedEventArgs e)
        {

            PensionDetailForm form = new PensionDetailForm();
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 260;
            form.MinWidth = 400;


            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            //throw new NotImplementedException();
            LoadData();
        }

        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 隐藏当前页不需要使用的GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            //ToolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            //ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;            

            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;

            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 社保缴纳记录：普通员工进去后默认显示一年的缴纳记录按降序排列
        /// </summary>
        private void LoadDefaultData()
        {

            int pageCount = 0;
            string filter = string.Empty;
            ObservableCollection<object> paras = new ObservableCollection<object>();
            DateTime nowDate = DateTime.Now;
            DateTime yeDate = DateTime.Now.AddYears(-1);
            for (int j = yeDate.Year; j <= nowDate.Year; j++)
            {
                int tempMonth = 1;//如果跨年查询，则最大为12月
                if (j != nowDate.Year)
                {
                    tempMonth = nowDate.Month;
                }
                for (int i = tempMonth; i <= nowDate.Month; i++)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " or ";//&&操作优先级大于||，不用括号
                    }
                    else
                    {
                        filter += " ( ";//括号包含薪资年月
                    }
                    filter += " PENSIONYEAR==@" + paras.Count.ToString();
                    paras.Add(j);
                    filter += " and ";
                    filter += " PENSIONMOTH==@" + paras.Count.ToString();
                    paras.Add(i);
                }
            }
            filter += " ) ";//括号包含薪资年月
            filter += " and ";
            filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
            paras.Add(Common.CurrentLoginUserInfo.EmployeeName);
            loadbar.Start();
            client.PensionDetailPagingAsync(dataPager.PageIndex, dataPager.PageSize, "PENSIONMOTH", filter,
                paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void LoadData()
        {

            int pageCount = 0;
            string filter = "";
            ObservableCollection<object> paras = new ObservableCollection<object>();

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            TextBox txtCardID = Utility.FindChildControl<TextBox>(expander, "txtCardID");
            NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
            NumericUpDown NuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
                {
                    //  filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }
            }
            if (txtCardID != null)
            {
                if (!string.IsNullOrEmpty(txtCardID.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //  filter += "CARDID==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(CARDID)";
                    paras.Add(txtCardID.Text.Trim());
                }
            }
            if (nuYear != null)
            {
                if (!string.IsNullOrEmpty(nuYear.Value.ToString()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "PENSIONYEAR==@" + paras.Count().ToString();
                    paras.Add((decimal?)nuYear.Value);
                }
            }
            if (NuStartmounth != null)
            {
                if (!string.IsNullOrEmpty(NuStartmounth.Value.ToString()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "PENSIONMOTH==@" + paras.Count().ToString();
                    paras.Add((decimal?)NuStartmounth.Value);
                }
            }
            loadbar.Start();
            client.PensionDetailPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CARDID", filter,
                paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void client_PensionDetailPagingCompleted(object sender, PensionDetailPagingCompletedEventArgs e)
        {
            List<T_HR_PENSIONDETAIL> list = new List<T_HR_PENSIONDETAIL>();
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
            loadbar.Stop();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            strDelResult = string.Empty;
            if (DtGrid.SelectedItems.Count > 0)
            {
                strDelIDs.Clear();
                string filterString = string.Empty;
                ObservableCollection<string> paras = new ObservableCollection<string>();

                NumericUpDown nuYear = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
                NumericUpDown NuStartmounth = Utility.FindChildControl<NumericUpDown>(expander, "NuStartmounth");

                if (nuYear != null)
                {
                    filterString = " SALARYYEAR == @" + paras.Count().ToString();
                    paras.Add(nuYear.Value.ToString());
                }

                if (NuStartmounth != null)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " AND";
                    }

                    filterString = " SALARYMONTH == @" + paras.Count().ToString();
                    paras.Add(NuStartmounth.Value.ToString());
                }

                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND (";
                }

                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    for (int i = 0; i < DtGrid.SelectedItems.Count; i++)
                    {
                        T_HR_PENSIONDETAIL tmp = DtGrid.SelectedItems[i] as T_HR_PENSIONDETAIL;

                        strDelIDs.Add(tmp.PENSIONDETAILID);
                        filterString += " EMPLOYEEID == @" + paras.Count().ToString();

                        if (i < DtGrid.SelectedItems.Count - 1)
                        {
                            filterString += " OR ";
                        }
                        else
                        {
                            filterString += " )";

                        }

                        paras.Add(tmp.PENSIONDETAILID);
                    }

                    clientSalary.CheckSalaryAuditStateAsync(strDelResult, filterString, paras);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, strDelResult);
            }
        }

        void client_PensionDetailDeleteCompleted(object sender, PensionDetailDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //  SetRowLogo(DtGrid, e.Row, "T_HR_PENSIONDETAIL");
            TextBlock tborder = DtGrid.Columns[0].GetCellContent(e.Row).FindName("tbNo") as TextBlock;
            if (tborder != null)
            {
                tborder.Text = (e.Row.GetIndex() + 1).ToString();
            }
        }

        private void Nuyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Year.ToDouble();
        }

        private void NuStartmounth_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Month.ToDouble();
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
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
