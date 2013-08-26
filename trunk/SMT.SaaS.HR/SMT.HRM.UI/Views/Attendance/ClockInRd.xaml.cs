/*
 * 文件名：ClockInRd.xaml.cs
 * 作  用：员工日常打卡查询页
 * 创建人：吴鹏
 * 创建时间：2010年1月19日, 16:31:45
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

using SMT.Saas.Tools.AttendanceWS;      //考勤接口
using SMT.Saas.Tools.OrganizationWS;    //公司组织接口
using System.ServiceModel;
using System.IO;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.Windows.Data;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.HRM.UI.Views.Attendance
{
    public partial class ClockInRd : BasePage, IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        byte[] byExport;
        private SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient orgClient;
        PermissionServiceClient PermClient = new PermissionServiceClient();
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;
        private T_SYS_ENTITYMENUCUSTOMPERM customerPermission;
        #endregion

        #region 初始化
        public ClockInRd()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ClockInRd_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 注册事件c
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);

            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            if (string.IsNullOrWhiteSpace(dpClockInRdStartDate.Text))
            {
                dpClockInRdStartDate.Text = dtNow.ToString("yyyy-M") + "-1";
            }

            if (string.IsNullOrWhiteSpace(dpClockInRdEndDate.Text))
            {
                dpClockInRdEndDate.Text = dtNow.ToString("yyyy-M") + "-" + iMaxDay.ToString();
            }

            //toolbar1.btnImport.Visibility = Visibility.Visible;
            toolbar1.btnOutExcel.Visibility = Visibility.Visible;


            toolbar1.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.btnImport.Click += new RoutedEventHandler(btnImport_Click);

            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);

            clientAtt.GetClockInRdListByMultSearchCompleted += new EventHandler<GetClockInRdListByMultSearchCompletedEventArgs>(clientAtt_GetClockInRdListByMultSearchCompleted);
            clientAtt.OutClockInRdListByMultSearchCompleted += new EventHandler<OutClockInRdListByMultSearchCompletedEventArgs>(clientAtt_OutClockInRdListByMultSearchCompleted);
            PermClient.GetCustomerPermissionByUserIDAndEntityCodeCompleted += new EventHandler<GetCustomerPermissionByUserIDAndEntityCodeCompletedEventArgs>(PermClient_GetCustomerPermissionByUserIDAndEntityCodeCompleted);
            if (customerPermission == null)
                PermClient.GetCustomerPermissionByUserIDAndEntityCodeAsync(Common.CurrentLoginUserInfo.SysUserID, "T_HR_EMPLOYEECLOCKINRECORD");
        }
        /// <summary>
        /// 读取自定义权限
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PermClient_GetCustomerPermissionByUserIDAndEntityCodeCompleted(object sender, GetCustomerPermissionByUserIDAndEntityCodeCompletedEventArgs e)
        {
            toolbar1.btnImport.Visibility = Visibility.Visible;//首先显示，为了读取失败之后不影响原有操作。
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        customerPermission = new T_SYS_ENTITYMENUCUSTOMPERM();
                        customerPermission = e.Result;
                    }
                    else
                    {
                        toolbar1.btnImport.Visibility = Visibility.Collapsed;//没有自定义权限则隐藏
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.Message);
            }
        }
        /// <summary>
        /// 组织架构树的选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            BindClockInRdList();
        }


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
            toolbar1.btnNew.Visibility = Visibility.Collapsed;
            toolbar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar1.BtnView.Visibility = Visibility.Collapsed;

            toolbar1.retNew.Visibility = Visibility.Collapsed;
            toolbar1.retEdit.Visibility = Visibility.Collapsed;
            toolbar1.retDelete.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 加载页面各项数据
        /// </summary>
        private void InitPage()
        {
            // BindTree();
            BindClockInRdList();
        }

        //绑定树
        //private void BindTree()
        //{
        //    orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //}

        /// <summary>
        /// 绑定日常打卡信息
        /// </summary>
        private void BindClockInRdList()
        {
            string strEmployeeID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            string strPunchDateFrom = string.Empty, strPunchDateTo = string.Empty, strTimeFrom = string.Empty, strTimeTo = string.Empty;

            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " t.PUNCHDATE ";
            CheckInputFilter(ref strEmployeeID, ref strPunchDateFrom, ref strPunchDateTo, ref strTimeFrom, ref strTimeTo);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;
            string sType = treeOrganization.sType, sValue = treeOrganization.sValue;
            clientAtt.GetClockInRdListByMultSearchAsync(sType, sValue, strOwnerID, strEmployeeID, strPunchDateFrom, strPunchDateTo, strTimeFrom, strTimeTo, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 效验查询输入的查询参数
        /// </summary>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strPunchDateFrom">打卡日期起始时间</param>
        /// <param name="strPunchDateTo">打卡日期截止时间</param>
        /// <param name="strTimeFrom">打卡时段起始时间</param>
        /// <param name="strTimeTo">打卡时段截止时间</param>
        private void CheckInputFilter(ref string strEmployeeID, ref string strPunchDateFrom, ref  string strPunchDateTo, ref string strTimeFrom, ref string strTimeTo)
        {
            DateTime dtPunchDateFrom = new DateTime();
            DateTime dtPunchDateTo = new DateTime();

            DateTime dtTimeFrom = new DateTime();
            DateTime dtTimeTo = new DateTime();

            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    strEmployeeID = ent.EMPLOYEEID.Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(dpClockInRdStartDate.Text))
            {
                bool flag = false;
                flag = DateTime.TryParse(dpClockInRdStartDate.Text, out dtPunchDateFrom);
                if (flag)
                {
                    strPunchDateFrom = dpClockInRdStartDate.Text;
                }
            }

            if (!string.IsNullOrEmpty(dpClockInRdEndDate.Text))
            {
                bool flag = false;
                flag = DateTime.TryParse(dpClockInRdEndDate.Text, out dtPunchDateTo);
                if (flag)
                {
                    strPunchDateTo = dpClockInRdEndDate.Text;
                }
            }

            if (dpClockInRdStartTime.Value != null)
            {
                dtTimeFrom = dpClockInRdStartTime.Value.Value;
            }

            if (dpClockInRdEndTime.Value != null)
            {
                dtTimeTo = dpClockInRdEndTime.Value.Value;
            }

            if (dtTimeTo >= dtTimeFrom)
            {
                strTimeFrom = dtTimeFrom.ToString("HH:mm");
                strTimeTo = dtTimeTo.ToString("HH:mm");
            }

            if (dtPunchDateFrom.CompareTo(dtPunchDateTo) > 0)
            {
                dtPunchDateFrom = new DateTime();
                dtPunchDateTo = new DateTime();
                dpClockInRdStartDate.Text = string.Empty;
                dpClockInRdEndDate.Text = string.Empty;
            }
        }

        void browser_ReloadDataEvent()
        {
            BindClockInRdList();
        }
        #endregion

        #region 事件

        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClockInRd_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            orgClient = new OrganizationServiceClient();
            GetEntityLogo("T_HR_EMPLOYEECLOCKINRECORD");
            RegisterEvents();
            UnVisibleGridToolControl();
            InitPage();
        }

        /// <summary>
        /// 加载数据列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetClockInRdListByMultSearchCompleted(object sender, GetClockInRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_EMPLOYEECLOCKINRECORD> entCRList = e.Result;

                dgClockInRdList.ItemsSource = entCRList;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }





        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgClockInRdList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgClockInRdList, e.Row, "T_HR_EMPLOYEECLOCKINRECORD");
        }

        /// <summary>
        /// 获取员工资料
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

        /// <summary>
        /// 根据查询条件，查询对应的日常考勤数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BindClockInRdList();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BindClockInRdList();
        }

        /// <summary>
        /// 读取打卡的Excel文件，并导入数据库，返回导入后的结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            DateTime dtPunchDateFrom = new DateTime();
            DateTime dtPunchDateTo = new DateTime();

            DateTime dtCheck = new DateTime();

            ImportEmpClockInRdForm form = new ImportEmpClockInRdForm(ref dtPunchDateFrom, ref dtPunchDateTo);
            EntityBrowser entBrowser = new EntityBrowser(form);

            if (dtPunchDateFrom > dtCheck && dtPunchDateTo > dtCheck)
            {
                dpClockInRdStartDate.Text = dtPunchDateFrom.ToString("yyyy-MM-dd");
                dpClockInRdEndDate.Text = dtPunchDateTo.ToString("yyyy-MM-dd");
            }

            form.MinWidth = 600;
            form.MinHeight = 200;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 手动导入打卡记录(记录为指定格式的Excel文件)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindClockInRdList();
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {
            string strEmployeeID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            string strPunchDateFrom = string.Empty, strPunchDateTo = string.Empty, strTimeFrom = string.Empty, strTimeTo = string.Empty;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = "PUNCHDATE, PUNCHTIME";
            CheckInputFilter(ref strEmployeeID, ref strPunchDateFrom, ref strPunchDateTo, ref strTimeFrom, ref strTimeTo);

            string sType = treeOrganization.sType, sValue = treeOrganization.sValue;

            if (byExport == null)
            {
                MessageBox.Show("由于silverlight安全限制，请等待返回数据后再次点击导出Excel");
                clientAtt.OutClockInRdListByMultSearchAsync(sType, sValue, strOwnerID, strEmployeeID, strPunchDateFrom, strPunchDateTo, strSortKey);

                loadbar.Start();
            }
            else
            {
                if (byExport.Count() > 0)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.Filter = "MS Excel Files|*.xls";
                    dialog.FilterIndex = 1;

                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(byExport, 0, byExport.Length);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("未获取到任何数据。");
                }
            }

        }
        /// <summary>
        /// 获取导出数据流
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_OutClockInRdListByMultSearchCompleted(object sender, OutClockInRdListByMultSearchCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error == null)
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    return;
                }

                byExport = e.Result;

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
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
