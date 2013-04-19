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

using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.HRM.UI.Form.Attendance;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class AttendYearlyBalance : BasePage,IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 初始化
        public AttendYearlyBalance()
        {
            InitializeComponent();
            clientAtt = new AttendanceServiceClient();
            RegisterEvents();
            GetEntityLogo("T_HR_ATTENDYEARLYBALANCE");
            UnVisibleGridToolControl();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString()); 
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);

            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            if (string.IsNullOrEmpty(txtBalanceYear.Text))
            {
                txtBalanceYear.Text = dtNow.Year.ToString();
            }

            toolbar1.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1022.png", Utility.GetResourceStr("BALANCECALCULATE")).Click += new RoutedEventHandler(AttendYearlyBalance_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            
            toolbar1.btnNew.Visibility = Visibility.Collapsed;
            toolbar1.retNew.Visibility = Visibility.Collapsed;
            toolbar1.retAudit.Visibility = Visibility.Collapsed;
            toolbar1.retDelete.Visibility = Visibility.Collapsed;
            toolbar1.retEdit.Visibility = Visibility.Collapsed;
            clientAtt.GetAttendYearlyBalanceListByMultSearchCompleted += new EventHandler<GetAttendYearlyBalanceListByMultSearchCompletedEventArgs>(clientAtt_GetAttendYearlyBalanceListByMultSearchCompleted);

            this.Loaded += new RoutedEventHandler(AttendYearlyBalance_Loaded);
        }        

        void AttendYearlyBalance_Click(object sender, RoutedEventArgs e)
        {
            CalculateEmployeeAttendanceYearlyForm form = new CalculateEmployeeAttendanceYearlyForm();
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 280.0;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
     
        }        


        /// <summary>
        /// 加载页面各项数据
        /// </summary>
        private void InitPage()
        {
            BindGrid();
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 隐藏当前页不需要使用的吃GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            //toolbar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbar1.btnAudit.Visibility = Visibility.Collapsed;
            //toolbar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbar1.btnDelete.Visibility = Visibility.Collapsed;
            toolbar1.btnEdit.Visibility = Visibility.Collapsed;
            toolbar1.btnRefresh.Visibility = Visibility.Collapsed;
            toolbar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
        }  

        /// <summary>
        /// 绑定Grid
        /// </summary>
        private void BindGrid()
        {
            string strEmployeeID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            decimal dBalanceYear = 0;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = "EMPLOYEEID, BALANCEYEAR";
            CheckInputFilter(ref strEmployeeID, ref dBalanceYear);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;


            clientAtt.GetAttendYearlyBalanceListByMultSearchAsync(strOwnerID, strEmployeeID, dBalanceYear, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 效验输入内容
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dBalanceYear"></param>
        /// <param name="strCheckState"></param>
        private void CheckInputFilter(ref string strEmployeeID, ref decimal dBalanceYear)
        {
            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    strEmployeeID = ent.EMPLOYEEID.Trim();
                }
            }

            if (!string.IsNullOrEmpty(txtBalanceYear.Text.Trim()))
            {
                decimal.TryParse(txtBalanceYear.Text, out dBalanceYear);
            }            
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void browser_ReloadDataEvent()
        {
            BindGrid();
        }
        #endregion

        #region 事件

        /// <summary>
        /// 根据审核状态显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 加载页面各项数据
        /// </summary>
        void AttendYearlyBalance_Loaded(object sender, RoutedEventArgs e)
        {
            InitPage();
        }

        /// <summary>
        /// 返回考勤月度结算数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendYearlyBalanceListByMultSearchCompleted(object sender, GetAttendYearlyBalanceListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_ATTENDYEARLYBALANCE> entAYBList = e.Result;

                dgAYBList.ItemsSource = entAYBList;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        /// <summary>
        /// 查询员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmpName_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
                typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

                if (ent != null)
                {
                    lkEmpName.DataContext = ent.T_HR_EMPLOYEE;
                }
            };
            
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        /// <summary>
        /// 根据指定条件，查询员工考勤年度结算数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        private void dgAYBList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgAYBList, e.Row, "T_HR_ATTENDYEARLYBALANCE");
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }
        #endregion


        #region IClient 成员

        public void ClosedWCFClient()
        {
            clientAtt.DoClose();
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
