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

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class ExceptionAttRd : BasePage, IClient
    {
        #region 全局变量
        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        private SMTLoading loadbar = new SMTLoading();
        IEnumerable<T_HR_EMPLOYEEABNORMRECORD> OutCsventList;
        #endregion

        public ExceptionAttRd()
        {
            InitializeComponent();


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
            this.Loaded += new RoutedEventHandler(ExceptionAttRd_Loaded);
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

            //DateTime dtNow = DateTime.Now;
            //int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);
            //if (string.IsNullOrWhiteSpace(dpClockInRdStartDate.Text))
            //{
            //    dpClockInRdStartDate.Text = dtNow.ToString("yyyy-M") + "-1";
            //}

            //if (string.IsNullOrWhiteSpace(dpClockInRdEndDate.Text))
            //{
            //    dpClockInRdEndDate.Text = dtNow.ToString("yyyy-M") + "-" + iMaxDay.ToString();
            //}
            
            clientAtt.GetAbnormRecordRdListByMultSearchCompleted += new EventHandler<GetAbnormRecordRdListByMultSearchCompletedEventArgs>(clientAtt_GetAbnormRecordRdListByMultSearchCompleted);
            
        }       

        /// <summary>
        /// 初始化，加载页面数据
        /// </summary>
        private void InitPage()
        {
            BindGrid();
        }

        /// <summary>
        /// 加载页面普通信息 --加载登录用户的相关信息，及配置权限
        /// </summary>
        private void BindGrid()
        {
            string strEmployeeID = string.Empty, strSignInState = string.Empty, strCurStartDate = string.Empty,strCurEndDate= string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSignInState = (Convert.ToInt32(IsChecked.No) + 1).ToString();

            strSortKey = " T_HR_ATTENDANCERECORD.EMPLOYEEID, ABNORMALDATE ";
            CheckInputFilter(ref strEmployeeID, ref strCurStartDate, ref strCurEndDate);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;
            outExcell = false;
            OutCsventList = null;//重置导出数据集合
            clientAtt.GetAbnormRecordRdListByMultSearchAsync(strOwnerID, strEmployeeID, strSignInState,strCurStartDate,strCurEndDate, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        private void CheckInputFilter(ref string strEmployeeID, ref string strCurStartDate, ref string strCurEndDate)
        {
            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    strEmployeeID = ent.EMPLOYEEID.Trim();
                }
            }

            if (!string.IsNullOrEmpty(dpClockInRdStartDate.Text))
            {
                strCurStartDate = dpClockInRdStartDate.Text;
            }

            if (!string.IsNullOrEmpty(dpClockInRdEndDate.Text))
            {
                strCurEndDate = dpClockInRdEndDate.Text;
            }
        }

        void ExceptionAttRd_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            GetEntityLogo("T_HR_EMPLOYEEABNORMRECORD");
            InitPage();
        }

        void clientAtt_GetAbnormRecordRdListByMultSearchCompleted(object sender, GetAbnormRecordRdListByMultSearchCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (outExcell)
                    {
                        OutCsventList = e.Result;
                    }
                    else
                    {

                        IEnumerable<T_HR_EMPLOYEEABNORMRECORD> entList = e.Result;
                        dgAbnormRecordList.ItemsSource = entList;
                        dataPager.PageCount = e.pageCount;
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
            finally
            {
                loadbar.Stop();
            }

        }        

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //检查前后日期大小D:\123\SMT.SaaS\SMT.SaaS.Portal\SMT.SaaS.HR\SMT.HRM.UI\Views\Salary\
            if (dpClockInRdEndDate.Text != null && dpClockInRdStartDate.Text!=null)
            {
                DateTime EndDate = DateTime.Parse(dpClockInRdEndDate.Text);
                DateTime StartDat = DateTime.Parse(dpClockInRdStartDate.Text);

                if (EndDate < StartDat)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ENDDATETIME"), Utility.GetResourceStr("DATECOMPARE", "ENDDATETIME,STARTDATETIME"));
                }
                else
                {
                    BindGrid();
                } 
            }
            
        }

        /// <summary>
        /// Grid首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAbnormRecordList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgAbnormRecordList, e.Row, "T_HR_EMPLOYEEABNORMRECORD");
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

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

        private bool outExcell = false;
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {


            string strEmployeeID = string.Empty, strSignInState = string.Empty, strCurStartDate = string.Empty, strCurEndDate = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSignInState = (Convert.ToInt32(IsChecked.No) + 1).ToString();

            strSortKey = " T_HR_ATTENDANCERECORD.EMPLOYEEID, ABNORMALDATE ";
            CheckInputFilter(ref strEmployeeID, ref strCurStartDate, ref strCurEndDate);
            pageIndex = dataPager.PageIndex;
            pageSize = 100000;//导出所有  
            if (OutCsventList == null)
            {
                outExcell = true;
                MessageBox.Show("由于silverlight安全限制，请等待返回数据后再次点击导出Excel");
                clientAtt.GetAbnormRecordRdListByMultSearchAsync(strOwnerID, strEmployeeID, strSignInState, strCurStartDate, strCurEndDate, strSortKey, pageIndex, pageSize, pageCount);
                loadbar.Start();
            }
            else
            {
                ExportToCSV.ExportDataGridWithDataSourceSaveAs(dgAbnormRecordList, OutCsventList);
            }

        }
    }
}
