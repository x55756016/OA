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



using SMT.HRM.UI.Form.Attendance;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Views.Attendance
{
    public partial class EvectionReport : BasePage, IClient
    {
        public string Checkstate { get; set; }

        AttendanceServiceClient client;
        private SMTLoading loadbar = new SMTLoading();
        public EvectionReport()
        {
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);
            client.EmployeeEvectionRecordDeleteCompleted += new EventHandler<EmployeeEvectionRecordDeleteCompletedEventArgs>(client_EmployeeEvectionRecordDeleteCompleted);
            client.EmployeeEvectionReportPagingCompleted += new EventHandler<EmployeeEvectionReportPagingCompletedEventArgs>(client_EmployeeEvectionReportPagingCompleted);
            toolbar1.Visibility = Visibility.Collapsed;
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //T_SYS_DICTIONARY dict = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            //if (dict != null)
            //{
            //    Checkstate = dict.DICTIONARYVALUE.Value.ToString();
            //    Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_EMPLOYEEEVECTIONRECORD");
            //    LoadData();
            //}
        }

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            filter += "CHECKSTATE==@" + paras.Count().ToString();
            // paras.Add(Checkstate);
            paras.Add(Convert.ToInt32(CheckStates.Approved).ToString());

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (!string.IsNullOrEmpty(txtEmpName.Text))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                paras.Add(txtEmpName.Text.Trim());
            }

            client.EmployeeEvectionReportPagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECODE", filter, paras, pageCount);
            loadbar.Start();
        }

        void client_EmployeeEvectionReportPagingCompleted(object sender, EmployeeEvectionReportPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_EMPLOYEEEVECTIONREPORT> list = new List<T_HR_EMPLOYEEEVECTIONREPORT>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dgEvection.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }
       

        private void dgEvection_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgEvection, e.Row, "T_HR_EMPLOYEEEVECTIONREPORT");
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 添加，修改，删除，审核
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            EvectionForm form = new EvectionForm(FormTypes.New, "");

            EntityBrowser entBrowser = new EntityBrowser(form);
            entBrowser.FormType = FormTypes.New;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgEvection.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEvection.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            T_HR_EMPLOYEEEVECTIONRECORD tmpEnt = dgEvection.SelectedItems[0] as T_HR_EMPLOYEEEVECTIONRECORD;

            EvectionForm form = new EvectionForm(FormTypes.Edit, tmpEnt.EVECTIONRECORDID);

            EntityBrowser entBrowser = new EntityBrowser(form);
            entBrowser.FormType = FormTypes.Edit;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgEvection.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEvection.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            ObservableCollection<string> ids = new ObservableCollection<string>();
            foreach (object ovj in dgEvection.SelectedItems)
            {
                T_HR_EMPLOYEEEVECTIONRECORD entEvection = ovj as T_HR_EMPLOYEEEVECTIONRECORD;
                if (entEvection != null)
                {
                    ids.Add(entEvection.EVECTIONRECORDID);
                }
            }

            string Result = "";
            if (ids.Count > 0)
            {
                ComfirmWindow delComfirm = new ComfirmWindow();
                delComfirm.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.EmployeeLeaveRecordDeleteAsync(ids);
                };
                delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
        }

        void client_EmployeeEvectionRecordDeleteCompleted(object sender, EmployeeEvectionRecordDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEEEVECTIONRECORD"));
                LoadData();
            }
        }
        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            client = new AttendanceServiceClient();
            InitEvent();
            LoadData();
        }

        #region IClient 成员

        public void ClosedWCFClient()
        {
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
