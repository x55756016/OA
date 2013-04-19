/*
 * 文件名：AttendanceSummary.xaml.cs
 * 作  用：在线考勤汇总方一览表
 * 创建人：魏瑞
 * 创建时间：2012年12月1日, 14:26:11
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
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.LocalData;
using System.Text;
using System.IO;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class AttendanceSummary : BasePage, IClient
    {
        private List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity  = new List<AbnormalAttendanceeEntity>();

        private List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity2 = new List<AbnormalAttendanceeEntity>();

        private List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity3 = new List<AbnormalAttendanceeEntity>();

        private List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity4 = new List<AbnormalAttendanceeEntity>();

        byte[] byExport;

        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;

        private SMTLoading loadbar = new SMTLoading();
        //登录公司ID
        string ownerCompanyId = string.Empty, ownerID = string.Empty;
        private int fistLonger = 0;
        private AttendanceServiceClient attendanceClient = new AttendanceServiceClient();
        public AttendanceSummary()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(AttendanceSummary_Loaded);
        }

        void AttendanceSummary_Loaded(object sender, RoutedEventArgs e)
        {
            UnVisibleGridToolControl();
            RegisterEvents();
            InitPage();

        }

        /// <summary>
        /// 加载页面各项数据
        /// </summary>
        private void InitPage()
        {
            // BindTree();
            //登录人
            string strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //登录人公司ID
            List<UserPost> userPost = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts;
            foreach (UserPost item in userPost)
            {
                if (item.IsAgency)
                {
                    ownerCompanyId = item.CompanyID;
                }
            }
            //首次次加载
            BindClockInRdList();
        }

        /// <summary>
        /// 注册事件c
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);
            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            //开始时间
            if (string.IsNullOrWhiteSpace(dpClockInRdStartDate.Text))
            {
                dpClockInRdStartDate.Text = dtNow.ToString("yyyy-M") + "-1";
            }
            //结束时间
            if (string.IsNullOrWhiteSpace(dpClockInRdEndDate.Text))
            {
                dpClockInRdEndDate.Text = dtNow.ToString("yyyy-M") + "-" + iMaxDay.ToString();
            }

            toolbar1.btnOutExcel.Visibility = Visibility.Visible;
            //导出EXECL
            toolbar1.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            //组织架构
            //treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);

            fistLonger = 1;

            //注册服务异常考勤
            attendanceClient.ListEMPLOYEEABNORMRECORDCompleted += new EventHandler<ListEMPLOYEEABNORMRECORDCompletedEventArgs>(attendanceClient_ListEMPLOYEEABNORMRECORDCompleted);

            //注册请假服务
            attendanceClient.GetEmployeeLeaverecordCompleted += new EventHandler<GetEmployeeLeaverecordCompletedEventArgs>(attendanceClient_GetEmployeeLeaverecordCompleted);
        
            //注册可调休假服务
            attendanceClient.GetAdjustableVacationCompleted += new EventHandler<GetAdjustableVacationCompletedEventArgs>(attendanceClient_GetAdjustableVacationCompleted);
        
            //注册超额工时服务
            attendanceClient.GetLasterClockInRecordCompleted += new EventHandler<GetLasterClockInRecordCompletedEventArgs>(attendanceClient_GetLasterClockInRecordCompleted);

            //导出数据
            attendanceClient.ExportEmployeesIntimeCompleted += new EventHandler<ExportEmployeesIntimeCompletedEventArgs>(attendanceClient_ExportEmployeesIntimeCompleted);
            loadbar.Start();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            abnormalAttendanceeEntity.Clear();
            abnormalAttendanceeEntity2.Clear();
            abnormalAttendanceeEntity3.Clear();
            abnormalAttendanceeEntity4.Clear();
            BindClockInRdList();
        }

        /// <summary>
        /// 导出考勤数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void attendanceClient_ExportEmployeesIntimeCompleted(object sender, ExportEmployeesIntimeCompletedEventArgs e)
        {
            if (e.Error==null)
            {
                if (e.Result!=null)
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
                MessageBox.Show("导出考勤数据错误，错误信息："+e.Error.ToString());
            }

            loadbar.Stop();
        }


        /// <summary>
        /// 超额工时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void attendanceClient_GetLasterClockInRecordCompleted(object sender, GetLasterClockInRecordCompletedEventArgs e)
        {
            if (e.Error==null)
            {
                if (e.Result!=null)
                {
                    var v = from t in e.Result
                            group t by new
                            {
                                t.cname,
                                t.EMPLOYEEID
                            } into g
                            select new AbnormalAttendanceeEntity 
                            {
                                //员工ID
                                EMPLOYEEID = g.Key.EMPLOYEEID,
                                //员工姓名
                                cname = g.Key.cname,

                                ExcessHoursTotal= g.Where(c => c.Punchdate.Value.Hour > 6).Sum(c => c.Punchdate.Value.Hour)
                                + (g.Where(c => c.Punchdate.Value.Hour > 6).Sum(c => c.Punchdate.Value.Minute)/60)
                            };
                    abnormalAttendanceeEntity4 = v.ToList();
                }
            }
            else
            {
                MessageBox.Show("超额工时服务出错，错误信息：" + e.Error.ToString());
            }
        }

        /// <summary>
        /// 可调休假
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void attendanceClient_GetAdjustableVacationCompleted(object sender, GetAdjustableVacationCompletedEventArgs e)
        {
            if (e.Error==null)
            {
                if (e.Result!=null)
                {
                    var v = from t in e.Result
                            group t by
                            new
                            {
                                t.EMPLOYEEID,
                                t.cname
                            } into g
                            select new AbnormalAttendanceeEntity 
                            {
                                //员工ID
                                EMPLOYEEID = g.Key.EMPLOYEEID,
                                //员工姓名
                                cname = g.Key.cname,

                                //可调休假
                                AdjustableDay = g.Sum(c=>c.AdjustableDay)
                            };
                    abnormalAttendanceeEntity3 = v.ToList();
                }
            }
            else
            {
                MessageBox.Show("可调休假服务错误，错误信息GetAdjustableVacation："+e.Error .ToString());
            }
        }

        /// <summary>
        /// 请假
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void attendanceClient_GetEmployeeLeaverecordCompleted(object sender, GetEmployeeLeaverecordCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    var v = from t in e.Result
                            group t by
                            new
                            {
                                t.EMPLOYEEID,
                                t.cname
                            }
                                into g
                                select new AbnormalAttendanceeEntity
                                {
                                    //员工ID
                                    EMPLOYEEID = g.Key.EMPLOYEEID,
                                    //员工姓名
                                    cname=g.Key.cname,

                                    //请事假时长
                                    LeaveHour = g.Where(c => c.LeaverecordStyple == "2").Sum(c => c.LeaverecordTime),
                                    //请年假时长
                                    AnnualLeave = g.Where(c => c.LeaverecordStyple == "4").Sum(c => c.LeaverecordTime),
                                    //请病假时长
                                    SickLeave = g.Where(c => c.LeaverecordStyple == "3").Sum(c => c.LeaverecordTime),
                                    //请调休假时长
                                    OffHour = g.Where(c => c.LeaverecordStyple == "1").Sum(c => c.LeaverecordTime)
                                };
                    abnormalAttendanceeEntity = v.ToList();
                }
            }
            else
            {
                MessageBox.Show("请假务错误，方法GetAbnormRecordByEmployeeID，信息：" + e.Error.ToString());
            }
        }

        /// <summary>
        /// 异常考勤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void attendanceClient_ListEMPLOYEEABNORMRECORDCompleted(object sender, ListEMPLOYEEABNORMRECORDCompletedEventArgs e)
        {
            if (e.Error==null)
            {
                if (e.Result!=null)
	            {

                    var v = from t in e.Result
                            group t by new 
                            {
                                t.EMPLOYEEID,
                                t.cname
                            } into g
                            select new AbnormalAttendanceeEntity
                            {
                                //名字
                                cname=g.Key.cname,
                                //员工ID
                                EMPLOYEEID=g.Key.EMPLOYEEID,
                                //迟到/早退次数
                                outTimes=g.Where(c=>c.ABNORMCATEGORY=="1").Count(),
                                //迟到/早退合计小时
                                outMinutes = g.Where(c => c.ABNORMCATEGORY == "1").Sum(c => c.ABNORMALTIME),
                                //缺勤次数
                                DrainTimeNumber =g.Where(c => c.ABNORMCATEGORY == "3").Count(),

                                //超额工时合计
                                ExcessHoursTotal=0,

                                //可调休假
                                AdjustableVacation=0,

                                //事假
                                LeaveHour=0,
                                //年休假
                                AnnualLeave=0,
                                //病假
                                SickLeave=0,
                                //调休假
                                OffHour=0
                            };
                    abnormalAttendanceeEntity2 = v.ToList();

                    foreach (var item in abnormalAttendanceeEntity)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID==item2.EMPLOYEEID)
                            {
                                item2.LeaveHour=item.LeaveHour;
                                item2.AnnualLeave=item.AnnualLeave;
                                item2.SickLeave=item.SickLeave;
                                item2.OffHour=item.OffHour;
                            }


                        }
                    }

                    foreach (var item in abnormalAttendanceeEntity3)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID==item2.EMPLOYEEID)
                            {
                                double dou=Double.Parse(item.AdjustableDay.ToString()) * 7.5
                                    - Double.Parse(item2.OffHour.ToString());

                                item2.AdjustableVacation =decimal.Parse(dou.ToString());
                            }
                        }
                    }

                    foreach (var item in abnormalAttendanceeEntity4)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID==item2.EMPLOYEEID)
                            {
                                item2.ExcessHoursTotal = item.ExcessHoursTotal;
                            }
                        }
                    }

                    //if (abnormalAttendanceeEntity2!=null&&!string.IsNullOrEmpty(ownerID))
                    //{
                    //    abnormalAttendanceeEntity2.Where(c=>c.EMPLOYEEID==ownerID);
                    //}


                    dgClockInRdList.ItemsSource = abnormalAttendanceeEntity2;

                    //byExport = dgClockInRdList.ItemsSource.to;

                    #region 删除
                    //if (abnormalAttendanceeEntity.Count >0)
                    //{

                    //}

                    //if (abnormalAttendanceeEntity2.Count>0)
                    //{
                        
                    //}
                    //var i = from k in abnormalAttendanceeEntity2
                    //        join j in abnormalAttendanceeEntity
                    //        on k.EMPLOYEEID equals j.EMPLOYEEID into g
                    //        from n in g.DefaultIfEmpty()
                    //        select n;
                    //        //select new AbnormalAttendanceeEntityStr
                    //        //{
                    //        //    //名字
                    //        //    cname = n.cname==null ? "0":n.cname,
                    //        //    //迟到/早退次数
                    //        //    outTimes = n.outTimes == null ? 0 : n.outTimes,
                    //        //    //迟到/早退合计小时
                    //        //    outMinutes = n.outMinutes == null ? 0 : n.outMinutes,
                    //        //    //缺勤次数
                    //        //    DrainTimeNumber = n.DrainTimeNumber == null ? 0 : n.DrainTimeNumber,

                    //        //    //请事假时长
                    //        //    LeaveHour = n.LeaveHour == null ? 0 : n.LeaveHour,
                    //        //    //请年假时长
                    //        //    AnnualLeave = n.AnnualLeave == null ? 0 : n.AnnualLeave,
                    //        //    //请病假时长
                    //        //    SickLeave = n.SickLeave == null ? 0 : n.SickLeave,
                    //        //    //请调休假时长
                    //        //    OffHour = n.OffHour == null ? 0 : n.OffHour
                    //        //};
                    //i.ToList();

                    //var u = from g in i
                    //        select new AbnormalAttendanceeEntity
                    //        {
                    //            cname=g.cname,
                    //            //迟到/早退次数
                    //            outTimes = g.outTimes == null ? 0 : g.outTimes,
                    //            //迟到/早退合计小时
                    //            outMinutes = g.outMinutes == null ? 0 : g.outMinutes,
                    //            //缺勤次数
                    //            DrainTimeNumber = g.DrainTimeNumber == null ? 0 : g.DrainTimeNumber,

                    //            //请事假时长
                    //            LeaveHour = g.LeaveHour == null ? 0 : g.LeaveHour,
                    //            //请年假时长
                    //            AnnualLeave = g.AnnualLeave == null ? 0 : g.AnnualLeave,
                    //            //请病假时长
                    //            SickLeave = g.SickLeave == null ? 0 : g.SickLeave,
                    //            //请调休假时长
                    //            OffHour = g.OffHour == null ? 0 : g.OffHour
                    //        };
                    //if (u.Count() > 0)
                    //{
                        
                    //    dgClockInRdList.ItemsSource = u;
                    //}

                    #endregion
                    loadbar.Stop();
	            }
                //loadbar.Stop();
            }
            else
            {
                MessageBox.Show("异常考勤服务错误，方法ListEMPLOYEEABNORMRECORD，信息："+e.Error.ToString());
            }   
        }


        public void ExportEmployeesIntime()
        {
            try
            {
                if (abnormalAttendanceeEntity2 != null)
                {
                    List<string> colName = new List<string>();
                    colName.Add("员工姓名");
                    colName.Add("迟到/早退总次数");
                    colName.Add("迟到/早退合计（分钟)");
                    colName.Add("漏打卡次数");
                    colName.Add("超额工时合计（小时）");
                    colName.Add("可调休假（小时）");
                    colName.Add("事假（小时）");
                    colName.Add("年休假（小时）");
                    colName.Add("病假（小时）");
                    colName.Add("调休假（小时）");
                    //var tmp = new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "EMPLOYEESTATE", "TOPEDUCATION", "NATION" });

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < colName.Count; i++)
                    {
                        sb.Append(colName[i] + ",");
                    }
                    sb.Append("\r\n"); // 列头

                    //内容
                    foreach (var employeeinfo in abnormalAttendanceeEntity2)
                    {
                        sb.Append(employeeinfo.cname + ",");
                        sb.Append(employeeinfo.outTimes + ",");
                        sb.Append(employeeinfo.outMinutes + ",");
                        sb.Append(employeeinfo.DrainTimeNumber + ",");
                        sb.Append(employeeinfo.ExcessHoursTotal + ",");
                        sb.Append(employeeinfo.AdjustableVacation + ",");
                        sb.Append(employeeinfo.LeaveHour+",");
                        sb.Append(employeeinfo.AnnualLeave+ ",");
                        sb.Append(employeeinfo.SickLeave + ",");
                        sb.Append(employeeinfo.OffHour+ ",");
                        sb.Append("\r\n");
                    }
                    byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());

                    if (result!=null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(result, 0, result.Length);
                        }
                        //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        MessageBox.Show("导出成功");
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出信息错误，ExportEmployeesIntime:" + ex.Message);
            }

            loadbar.Stop();
        }

        /// <summary>
        /// 绑定日常打卡信息
        /// </summary>
        private void BindClockInRdList()
        {
            //开始时间
            string startDate = this.dpClockInRdStartDate.SelectedDate.Value.ToShortDateString();
            //结束日期
            string endDate = this.dpClockInRdEndDate.SelectedDate.Value.ToShortDateString();
          
            //校正时间
            if (this.dpClockInRdEndDate.SelectedDate.Value <this.dpClockInRdStartDate.SelectedDate.Value)
            {
                MessageBox.Show("结束时间不能小于开始时间");
                return;
            }

            string strEmployeeID = ownerID;

            //SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

            //if (ent!=null)
            //{
            //    strEmployeeID = ent.EMPLOYEEID.Trim();
            //}

            //考勤异常
            attendanceClient.ListEMPLOYEEABNORMRECORDAsync(strEmployeeID, ownerCompanyId, startDate, endDate);
            //请假信息
            attendanceClient.GetEmployeeLeaverecordAsync(strEmployeeID, ownerCompanyId, startDate, endDate);
            //可调休假
            attendanceClient.GetAdjustableVacationAsync(strEmployeeID, ownerCompanyId, startDate, endDate);
            //超额工时间
            attendanceClient.GetLasterClockInRecordAsync(strEmployeeID, ownerCompanyId, startDate, endDate);
            
            loadbar.Start();
        }

        /// <summary>
        /// 导出EXECL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {
            //开始时间
            string startDate = this.dpClockInRdStartDate.SelectedDate.Value.ToShortDateString();
            //结束日期
            string endDate = this.dpClockInRdEndDate.SelectedDate.Value.ToShortDateString();

            string strEmployeeID = ownerID;

            dialog.Filter = "MS csv Files|*.csv";
            dialog.FilterIndex = 1;
            result = dialog.ShowDialog();
            if (result == true)
            {
                loadbar.Start();

                attendanceClient.ExportEmployeesIntimeAsync(strEmployeeID, ownerCompanyId, startDate, endDate);
            }
        }


        /// <summary>
        /// 根据查询条件，查询对应的日常考勤数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            abnormalAttendanceeEntity.Clear();
            abnormalAttendanceeEntity2.Clear();
            abnormalAttendanceeEntity3.Clear();
            abnormalAttendanceeEntity4.Clear();
            BindClockInRdList();
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIsNull_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            //开始时间
            dpClockInRdStartDate.Text = dtNow.ToString("yyyy-M") + "-1";
           
            //结束时间
            dpClockInRdEndDate.Text = dtNow.ToString("yyyy-M") + "-" + iMaxDay.ToString();

            ownerID = string.Empty;
            lkEmpName.DataContext =null;
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

                    ownerID = ent.EMPLOYEEID;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

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

        public void ClosedWCFClient()
        {
            throw new NotImplementedException();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
    }
}
