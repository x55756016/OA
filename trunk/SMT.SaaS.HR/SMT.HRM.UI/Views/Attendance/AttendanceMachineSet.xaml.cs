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

using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;

using SMT.HRM.UI.Form.Attendance;
using System.Collections.ObjectModel;

using System.Windows.Data;
using System.Windows.Controls.Primitives;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class AttendanceMachineSet : BasePage
    {
        AttendanceServiceClient client;
        SMTLoading loadbar = new SMTLoading();
        public AttendanceMachineSet()
        {
            InitializeComponent();
            InitPara();
            LoadData();
            GetEntityLogo("T_HR_ATTENDMACHINESET");
        }

        public void InitPara()
        {
            PARENT.Children.Add(loadbar);
            try
            {
                client = new AttendanceServiceClient();
                client.GetAttendMachineSetPagingCompleted += new EventHandler<GetAttendMachineSetPagingCompletedEventArgs>(client_GetAttendMachineSetPagingCompleted);
                client.AttendMachineSetDeleteCompleted += new EventHandler<AttendMachineSetDeleteCompletedEventArgs>(client_AttendMachineSetDeleteCompleted);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                loadbar.Stop();
            }

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            //ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
        }

        void client_AttendMachineSetDeleteCompleted(object sender, AttendMachineSetDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "ATTENDMACHINESET"));
                LoadData();
            }
        }

        void client_GetAttendMachineSetPagingCompleted(object sender, GetAttendMachineSetPagingCompletedEventArgs e)
        {
            List<T_HR_ATTENDMACHINESET> list = new List<T_HR_ATTENDMACHINESET>();
            if (e.Result != null)
            {
                list = e.Result.ToList();
            }
            DtGrid.ItemsSource = list;

            dataPager.PageCount = e.pageCount;
            loadbar.Stop();
        }

        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            AttendanceMachineSetForm form = new AttendanceMachineSetForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            form.MinWidth = 600;
            form.MinHeight = 300;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }


        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            loadbar.Start();
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            string strState = "";

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                filter += "ATTENDMACHINENAME==@" + paras.Count().ToString();
                paras.Add(txtName.Text.Trim());
            }

            client.GetAttendMachineSetPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_ATTENDMACHINESET tmp= DtGrid.SelectedItems[0] as T_HR_ATTENDMACHINESET;

                if (tmp == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                    return;
                }
                AttendanceMachineSetForm form = new AttendanceMachineSetForm(FormTypes.Edit, tmp.ATTENDMACHINESETID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 600;
                form.MinHeight = 300;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_ATTENDMACHINESET tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.ATTENDMACHINESETID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.AttendMachineSetDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }

        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_ATTENDMACHINESET tmp = DtGrid.SelectedItems[0] as T_HR_ATTENDMACHINESET;
                AttendanceMachineSetForm form = new AttendanceMachineSetForm(FormTypes.Browse, tmp.ATTENDMACHINESETID);
                form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 600;
                form.MinHeight = 300;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }

        #endregion  

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_ATTENDMACHINESET");
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
