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
namespace SMT.HRM.UI.Views.Salary
{
    public partial class PerformanceRewardRecord : BasePage,IClient
    {
        private SalaryServiceClient client = new SalaryServiceClient();
        private string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        public PerformanceRewardRecord()
        {
            InitializeComponent();
            InitParas();
            GetEntityLogo("T_HR_PERFORMANCEREWARDRECORD");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_PERFORMANCEREWARDRECORD", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        private void InitParas()
        {
            PARENT.Children.Add(loadbar);

            client.PerformanceRewardRecordDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PerformanceRewardRecordDeleteCompleted);
            client.GetPerformanceRewardRecordWithPagingCompleted += new EventHandler<GetPerformanceRewardRecordWithPagingCompletedEventArgs>(client_GetPerformanceRewardRecordWithPagingCompleted);
            #region 工具栏
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            //ToolBar.btnAduitNoTPass.Click += new RoutedEventHandler(btnAuitNoTPass_click);
            //ToolBar.btnSumbitAudit.Click += new RoutedEventHandler(btnSumbitAudit_click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            #endregion


        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO: 重新提交审核 
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PERFORMANCEREWARDRECORD tmpEnt = DtGrid.SelectedItems[0] as T_HR_PERFORMANCEREWARDRECORD;
                Form.Salary.PerformanceRewardForm form = new Form.Salary.PerformanceRewardForm(FormTypes.Resubmit, tmpEnt.PERFORMANCEREWARDRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 210;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                return;
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD修改    
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PERFORMANCEREWARDRECORD tmpEnt = DtGrid.SelectedItems[0] as T_HR_PERFORMANCEREWARDRECORD;
                Form.Salary.PerformanceRewardForm form = new Form.Salary.PerformanceRewardForm(FormTypes.Browse, tmpEnt.PERFORMANCEREWARDRECORDID);
                form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinHeight = 250;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                //  client.PerformanceRewardRecordUpdateAsync(tmpEnt);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }

        void client_PerformanceRewardRecordDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "PERFORMANCEREWARDRECORD"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "PERFORMANCEREWARDRECORD"));
                LoadData();
            }
        }

        void client_GetPerformanceRewardRecordWithPagingCompleted(object sender, GetPerformanceRewardRecordWithPagingCompletedEventArgs e)
        {
            List<T_HR_PERFORMANCEREWARDRECORD> list = new List<T_HR_PERFORMANCEREWARDRECORD>();
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
                }
                DtGrid.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }


            loadbar.Stop();
        }
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_PERFORMANCEREWARDRECORD");
                LoadData();
            }
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #region 添加,修改,删除,查询,审核
        void btnNew_Click(object sender, RoutedEventArgs e)
        {

            Form.Salary.PerformanceRewardRecordForm form = new SMT.HRM.UI.Form.Salary.PerformanceRewardRecordForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 650;
            form.MinHeight = 200;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核  
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PERFORMANCEREWARDRECORD tmpEnt = DtGrid.SelectedItems[0] as T_HR_PERFORMANCEREWARDRECORD;
                Form.Salary.PerformanceRewardForm form = new Form.Salary.PerformanceRewardForm(FormTypes.Audit, tmpEnt.PERFORMANCEREWARDRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 210;
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
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD修改    
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PERFORMANCEREWARDRECORD tmpEnt = DtGrid.SelectedItems[0] as T_HR_PERFORMANCEREWARDRECORD;
                Form.Salary.PerformanceRewardForm form = new Form.Salary.PerformanceRewardForm(FormTypes.Edit, tmpEnt.PERFORMANCEREWARDRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit; 
                form.MinHeight = 210;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                //  client.PerformanceRewardRecordUpdateAsync(tmpEnt);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }
        void btnSumbitAudit_click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 提交审核            
        }
        void btnAuitNoTPass_click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核不通过          
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }


        private void LoadData()
        {
            loadbar.Start();

            int pageCount = 0;
            string filter = "";
            string strState = "";
            DateTime? starttimes;
            DateTime? endtimes = DateTime.Now.Date;
            if (DateTime.Now.Month <=2)
                starttimes = new DateTime(DateTime.Now.Year - 1, 1, 1);
            else
                starttimes = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 2, 1);

            DatePicker dpstarttimes = Utility.FindChildControl<DatePicker>(expander, "dpstarttime");
            DatePicker dpendtimes = Utility.FindChildControl<DatePicker>(expander, "dpendtime");

            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            NumericUpDown nudyears = Utility.FindChildControl<NumericUpDown>(expander, "Nuyear");
            NumericUpDown nudmonths = Utility.FindChildControl<NumericUpDown>(expander, "NuMounth");


            try
            {
                starttimes = dpstarttimes.SelectedDate;
                endtimes = dpendtimes.SelectedDate;
                if (starttimes > endtimes)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始时间不能小于结束时间"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                   // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("开始时间不能小于结束时间"));
                    return;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                //filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                paras.Add(txtName.Text.Trim());
            }
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
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

            client.GetPerformanceRewardRecordWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "PERFORMANCEREWARDRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);

        }
        //void btnEdit_Click(object sender, RoutedEventArgs e)
        //{
        //    if (DtGrid.SelectedItems.Count > 0)
        //    {
        //        T_HR_PERFORMANCEREWARDSET tmpEnt = DtGrid.SelectedItems[0] as T_HR_PERFORMANCEREWARDSET;

        //        Form.Salary.PerformanceRewardSetForm form = new SMT.HRM.UI.Form.Salary.PerformanceRewardSetForm(FormTypes.Edit, tmpEnt.PERFORMANCEREWARDSETID);

        //        EntityBrowser browser = new EntityBrowser(form);
        //        
        //        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //        browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        //    }
        //}

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_PERFORMANCEREWARDRECORD tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.PERFORMANCEREWARDRECORDID);
                    if (!(tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() || tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString()))
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                        return;
                    }
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.PerformanceRewardRecordDeleteAsync(ids);
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


        #endregion
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_PERFORMANCEREWARDSET");
        }

        private void years_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;
            num.Value = Convert.ToDouble(System.DateTime.Now.Year);
            if (System.DateTime.Now.Month <= 1)
            {
                num.Value = (System.DateTime.Now.Year - 1).ToDouble();
            }
        }

        private void months_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;
            num.Value = Convert.ToDouble(System.DateTime.Now.Month);
            if (System.DateTime.Now.Month <= 1)
            {
                num.Value = 12;
            }
            else
            {
                num.Value = System.DateTime.Now.Month - 1;
            }
        }

        private void dpstarttime_Loaded(object sender, RoutedEventArgs e)
        {
            DatePicker dp = (DatePicker)sender;
            dp.Text = DateTime.Now.Date.ToString();
        }

        private void dpendtime_Loaded(object sender, RoutedEventArgs e)
        {
            DatePicker dp = (DatePicker)sender;
            int MaxDate = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            dp.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, MaxDate).ToShortDateString();
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

        private void Nuyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Year.ToDouble();
            if (System.DateTime.Now.Month <= 1)
            {
                nuyear.Value = (System.DateTime.Now.Year - 1).ToDouble();
            }
        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
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
