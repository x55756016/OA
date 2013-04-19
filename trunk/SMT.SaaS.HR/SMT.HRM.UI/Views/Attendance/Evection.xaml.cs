/*
 * 文件名：Evection.xaml.cs
 * 作  用：员工出差记录 页
 * 创建人：吴鹏
 * 创建时间：2010年1月14日, 17:24:29
 * 修改人：
 * 修改时间：
 */

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
using SMT.SaaS.FrameworkUI.OrganizationControl;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class Evection : BasePage, IClient
    {
        public string Checkstate { get; set; }

        AttendanceServiceClient client;
        private SMTLoading loadbar = new SMTLoading();

        public Evection()
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

            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            if (string.IsNullOrEmpty(dpEvectionStartDate.Text))
            {
                dpEvectionStartDate.Text = dtNow.ToString("yyyy-M") + "-1";
            }

            if (string.IsNullOrEmpty(dpEvectionEndDate.Text))
            {
                dpEvectionEndDate.Text = dtNow.ToString("yyyy-M") + "-" + iMaxDay.ToString();
            }

            client.EmployeeEvectionRecordPagingCompleted += new EventHandler<EmployeeEvectionRecordPagingCompletedEventArgs>(client_EmployeeEvectionRecordPagingCompleted);
            client.EmployeeEvectionRecordDeleteCompleted += new EventHandler<EmployeeEvectionRecordDeleteCompletedEventArgs>(client_EmployeeEvectionRecordDeleteCompleted);

            toolbar1.Visibility = Visibility.Collapsed;
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
        }

        private void LoadData()
        {
            string strEmployeeID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            string strDateFrom = string.Empty, strDateTo = string.Empty;

            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " STARTDATE ";
            CheckInputFilter(ref strEmployeeID, ref strDateFrom, ref strDateTo);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            client.EmployeeEvectionRecordPagingAsync(strOwnerID, strEmployeeID, strDateFrom, strDateTo, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 效验查询输入的查询参数
        /// </summary>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strDateFrom">打卡日期起始时间</param>
        /// <param name="strDateTo">打卡日期截止时间</param>
        private void CheckInputFilter(ref string strEmployeeID, ref string strDateFrom, ref  string strDateTo)
        {
            DateTime dtDateFrom = new DateTime();
            DateTime dtDateTo = new DateTime();

            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    strEmployeeID = ent.EMPLOYEEID.Trim();
                }
            }

            if (!string.IsNullOrEmpty(dpEvectionStartDate.Text.Trim()))
            {
                bool flag = false;
                flag = DateTime.TryParse(dpEvectionStartDate.Text, out dtDateFrom);
                if (flag)
                {
                    strDateFrom = dpEvectionStartDate.Text;
                }
            }

            if (!string.IsNullOrEmpty(dpEvectionEndDate.Text.Trim()))
            {
                bool flag = false;
                flag = DateTime.TryParse(dpEvectionEndDate.Text, out dtDateTo);
                if (flag)
                {
                    strDateTo = dpEvectionEndDate.Text;
                }
            }

            if (dtDateFrom.CompareTo(dtDateTo) > 0)
            {
                dtDateFrom = new DateTime();
                dtDateTo = new DateTime();
                dpEvectionStartDate.Text = string.Empty;
                dpEvectionEndDate.Text = string.Empty;
            }
        }

        void client_EmployeeEvectionRecordPagingCompleted(object sender, EmployeeEvectionRecordPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_EMPLOYEEEVECTIONRECORD> list = new List<T_HR_EMPLOYEEEVECTIONRECORD>();
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
            SetRowLogo(dgEvection, e.Row, "T_HR_EMPLOYEEEVECTIONRECORD");
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

        /// <summary>
        /// 选取员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmpName_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                if (ent != null)
                {
                    lkEmpName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
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
