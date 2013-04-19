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

namespace SMT.HRM.UI.Views.Salary
{
    public partial class TemporaryPayroll : BasePage,IClient
    {
        SMTLoading loadbar = new SMTLoading();
        private SalaryServiceClient client = new SalaryServiceClient();
        private string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        private string Checkstate { get; set; }
        public TemporaryPayroll()
        {
            InitializeComponent();
            InitParas();
            GetEntityLogo("T_HR_CUSTOMGUERDONRECORD");
        }

        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            this.DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);
            client.GetCustomGuerdonRecordPagingCompleted += new EventHandler<GetCustomGuerdonRecordPagingCompletedEventArgs>(client_GetCustomGuerdonRecordPagingCompleted);
            client.CustomGuerdonRecordDeleteCompleted += new EventHandler<CustomGuerdonRecordDeleteCompletedEventArgs>(client_CustomGuerdonRecordDeleteCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            this.DtGrid.CanUserSortColumns = false;
            this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO: 重新提交审核 
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_CUSTOMGUERDONRECORD tmpEnt = DtGrid.SelectedItems[0] as T_HR_CUSTOMGUERDONRECORD;
                Form.Salary.TemporaryPayrollEditForm form = new Form.Salary.TemporaryPayrollEditForm(FormTypes.Resubmit, tmpEnt.CUSTOMGUERDONRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 250;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_CUSTOMGUERDONRECORD tempid = DtGrid.SelectedItems[0] as T_HR_CUSTOMGUERDONRECORD;
                Form.Salary.TemporaryPayrollEditForm form = new SMT.HRM.UI.Form.Salary.TemporaryPayrollEditForm(FormTypes.Browse, tempid.CUSTOMGUERDONRECORDID);
                form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form); 
                form.MinHeight = 250;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }

        void DtGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            T_HR_CUSTOMGUERDONRECORD sr = e.Row.DataContext as T_HR_CUSTOMGUERDONRECORD;
            SalaryContrast getdgdetail = Utility.FindChildControl<SalaryContrast>(DtGrid, "dgdetail");
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
                getdgdetail.FlashData(sr.EMPLOYEEID, years, months, SalaryGenerateType.CustomSalaryRecord);
            }
            else
            {
                getdgdetail.Visibility = Visibility.Collapsed;
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void client_CustomGuerdonRecordDeleteCompleted(object sender, CustomGuerdonRecordDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CUSTOMGUERDONRECORD"));
                LoadData();
            }
        }

        void client_GetCustomGuerdonRecordPagingCompleted(object sender, GetCustomGuerdonRecordPagingCompletedEventArgs e)
        {
            List<T_HR_CUSTOMGUERDONRECORD> list = new List<T_HR_CUSTOMGUERDONRECORD>();
            if (e.Result != null)
            {
                list = e.Result.ToList();
            }
            DtGrid.ItemsSource = list;

            dataPager.PageCount = e.pageCount;

            loadbar.Stop();
        }

        #region 添加,修改,删除,查询,审核

        public T_HR_CUSTOMGUERDONRECORD SelectID { get; set; }
        void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectID = grid.SelectedItems[0] as T_HR_CUSTOMGUERDONRECORD;
            }
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.TemporaryPayrollForm form = new SMT.HRM.UI.Form.Salary.TemporaryPayrollForm();
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 500;
            form.MinHeight = 180;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }


        private void LoadData()
        {
            int pageCount = 0;
            loadbar.Start();
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

            NumericUpDown nudyears = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
            NumericUpDown nudmonths = Utility.FindChildControl<NumericUpDown>(expander, "NuMounth");
            try
            {
                starttimes = dpstarttimes.SelectedDate;
                endtimes = dpendtimes.SelectedDate;
                if (starttimes > endtimes)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始时间不能小于结束时间"));
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
                //filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                paras.Add(txtName.Text.Trim());
            }
            if (nudyears != null)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "SALARYYEAR==@" + paras.Count().ToString();
                paras.Add(nudyears.Value.ToString());
            }

            if (nudmonths != null)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "SALARYMONTH==@" + paras.Count().ToString();
                paras.Add(nudmonths.Value.ToString());
            }

            client.GetCustomGuerdonRecordPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CUSTOMGUERDONRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), strState, userID);

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_CUSTOMGUERDONRECORD tempid = DtGrid.SelectedItems[0] as T_HR_CUSTOMGUERDONRECORD;
                Form.Salary.TemporaryPayrollEditForm form = new SMT.HRM.UI.Form.Salary.TemporaryPayrollEditForm(tempid.CUSTOMGUERDONRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit; 
                form.MinHeight = 250;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else 
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //ComfirmBox deleComfir = new ComfirmBox();
            //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
            //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            //deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK_Click);
            //deleComfir.Show();

            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_CUSTOMGUERDONRECORD tmp in DtGrid.SelectedItems)
                {
                    if (!(tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() || tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString()))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                        return;
                    }
                    ids.Add(tmp.CUSTOMGUERDONRECORDID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.CustomGuerdonRecordDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else 
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }

        }

        void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_CUSTOMGUERDONRECORD tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.CUSTOMGUERDONRECORDID);
                }
                client.CustomGuerdonRecordDeleteAsync(ids);
            }

        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核   
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_CUSTOMGUERDONRECORD tmpEnt = DtGrid.SelectedItems[0] as T_HR_CUSTOMGUERDONRECORD;
                Form.Salary.TemporaryPayrollEditForm form = new Form.Salary.TemporaryPayrollEditForm(FormTypes.Audit, tmpEnt.CUSTOMGUERDONRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 250;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else 
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
            }
        }
        #endregion


        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选择某审核状态是重新加载数据

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_CUSTOMGUERDONRECORD");
                LoadData();
            }

        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEESALARYRECORD", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_CUSTOMGUERDONRECORD");
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

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

        private void Nuyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Year.ToDouble();
            if (System.DateTime.Now.Month <= 1)
            {
                nuyear.Value = (System.DateTime.Now.Year - 1).ToDouble();
            }
        }

        private void NuMounth_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown numonuth = (NumericUpDown)sender;
            numonuth.Value = DateTime.Now.Month.ToDouble();
            if (System.DateTime.Now.Month <= 1)
            {
                numonuth.Value = 12;
            }
            else
            {
                numonuth.Value = System.DateTime.Now.Month - 1;
            }
        }

        #region IClient
        public void ClosedWCFClient()
        {
        
        }
        public  bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        { 
           
        }
        #endregion

    }
}
