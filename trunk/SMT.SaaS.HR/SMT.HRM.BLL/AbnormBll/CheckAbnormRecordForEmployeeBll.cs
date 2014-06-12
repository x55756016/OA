
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using System.Data;
using SMT.HRM.CustomModel;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using SMT.HRM.CustomModel.Reports;
using SMT.Foundation.Log;
using SMT.HRM.BLL.Common;
using System.Threading;

namespace SMT.HRM.BLL
{
    public partial class AbnormRecordBLL : BaseBll<T_HR_EMPLOYEEABNORMRECORD>
    {
        #region 私有方法---检查考勤日每天各工作段的出勤情况

        /// <summary>
        /// 根据指定范围的员工和时间段，检查各员工的考勤记录是否存在异常
        /// </summary>
        /// <param name="entEmployees">根据指定范围的员工</param>
        /// <param name="dtStart">考勤检查起始日期</param>
        /// <param name="dtEnd">考勤检查截止日期</param>
        /// <param name="strMsg">检查后返回的消息</param>
        public void CheckAbnormRecordForEmployees(List<T_HR_EMPLOYEE> entEmployees, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            foreach (T_HR_EMPLOYEE entEmployee in entEmployees)
            {
                if (entEmployee.EMPLOYEECNAME == "曹利宁")
                {
                }
                bool isAbnorm = false;
                string strEmployeeID = entEmployee.EMPLOYEEID;
                //获取导入打卡记录对应时间段的考勤记录，以便进行比对
                AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();
                dtStart=new DateTime(dtStart.Year, dtStart.Month, 1);//修改为从当月1日开始检查考勤异常
                IQueryable<T_HR_ATTENDANCERECORD> entAttRds
                    = bllAttendanceRecord.GetAttendanceRecordByEmployeeIDAndDate(entEmployee.OWNERCOMPANYID
                    , strEmployeeID, dtStart, dtEnd);

                string dealType = "检查异常考勤，员工姓名：" + entEmployee.EMPLOYEECNAME;
                //如果没有找到考勤初始化记录，初始考勤一次。
                if (entAttRds.Count() == 0)
                {
                    try
                    {
                        string dtInit = dtStart.Year.ToString() + "-" + dtStart.Month.ToString();
                        Tracer.Debug(dealType + dtStart.ToString("yyyy-MM-dd") + " 至 " + dtEnd.ToString("yyyy-MM-dd")
                            + " 没有查到考勤初始化数据，开始初始化员工考勤：" + dtInit);
                        AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL();
                        //初始化该员工当月考勤记录
                        bllAttendanceSolutionAsign.AsignAttendanceSolutionByOrgID("4", entEmployee.EMPLOYEEID
                            , dtInit);
                        Tracer.Debug(dealType + " 初始化员工考勤成功，初始化月份：" + dtInit);

                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug(dealType + " 初始化考勤异常：" + ex.ToString());
                    }
                    //continue;
                }

                entAttRds
                = bllAttendanceRecord.GetAttendanceRecordByEmployeeIDAndDate(entEmployee.OWNERCOMPANYID
                , strEmployeeID, dtStart, dtEnd);
                //如果初始化考勤后任然没有考勤记录，跳过。
                if (entAttRds.Count() == 0)
                {
                    Tracer.Debug("导入打卡记录没有找到初始化考勤记录，未修改考勤初始化记录状态。");
                    continue;
                }
                AttendanceSolutionAsignBLL asbll = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entAttendanceSolution = asbll.GetAttendanceSolutionAsignByEmployeeIDAndDate(entEmployee.EMPLOYEEID, dtStart);
                
                if (entAttendanceSolution.T_HR_ATTENDANCESOLUTION.ATTENDANCETYPE == (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString())//考勤方案设置为不考勤
                {
                    Tracer.Debug(dealType + ",被跳过，该员工使用的考勤方案为免打卡方案，考勤方案名："
                        + entAttendanceSolution.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME);

                    continue;
                }
                //查询打卡记录
                string strSortKey = "PUNCHDATE";
                ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL();
                IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRecords
                    = bllClockInRecord.GetAllClockInRdListByMultSearch(string.Empty
                    , string.Empty, string.Empty, strEmployeeID, dtStart.ToString()
                    , dtEnd.ToString(), strSortKey);
                
                //检查考勤异常
                CheckAbnormRecord(entAttRds, entClockInRecords, ref strMsg, ref isAbnorm);

                //生成签卡记录
                AbnormRecordCheckAlarm(entEmployee.EMPLOYEEID, dtStart.ToString("yyyy-MM") + "-1", dtEnd.ToString("yyyy-MM-dd"));
                //检查当月处理错误的考勤异常
                DateTime thisMonthStarDay=new DateTime(dtStart.Year,dtStart.Month,1);
                DateTime thisMonthEndDay = dtEnd;//检查从1号到本次考勤结束时间的异常
                Tracer.Debug("检查从本月1号到本次考勤结束时间的异常");
                Tracer.Debug("开始处理当月错误的异常考勤");
                ClearWornSigleAndAbnormRecord(entEmployee.EMPLOYEEID, thisMonthStarDay, thisMonthEndDay);
            }
        }

        /// <summary>
        /// 检查上班打卡情况
        /// </summary>
        /// <param name="entAttRds"></param>
        /// <param name="entClockInRecords"></param>
        /// <param name="strMsg"></param>
        /// <param name="bIsAbnorm"></param>
        private void CheckAbnormRecord(IQueryable<T_HR_ATTENDANCERECORD> entAttRds, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strMsg, ref bool bIsAbnorm)
        {
            try
            {
                if (entAttRds.Count() < 1)
                {
                    Tracer.Debug("检查异常考勤退出，无考勤初始化记录");
                    return;
                }
                var startItem = entAttRds.ToList().OrderBy(c => c.ATTENDANCEDATE).FirstOrDefault();
                var endItem = entAttRds.ToList().OrderBy(c => c.ATTENDANCEDATE).LastOrDefault();

                AttendanceSolutionBLL bllAttSol = new AttendanceSolutionBLL();
                AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();
                AttendanceSolutionAsignBLL bllAttSolAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTION attSolution = new T_HR_ATTENDANCESOLUTION();

                //获取对应的考勤方案
                //T_HR_ATTENDANCESOLUTION entAttSol = bllAttSol.GetAttendanceSolutionByID(item.ATTENDANCESOLUTIONID);
                T_HR_ATTENDANCESOLUTIONASIGN entAttendanceSolution
                    = bllAttSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(startItem.EMPLOYEEID, startItem.ATTENDANCEDATE.Value);
                T_HR_ATTENDANCESOLUTION entAttSol = entAttendanceSolution.T_HR_ATTENDANCESOLUTION;

                //对考勤记录进行轮询
                foreach (T_HR_ATTENDANCERECORD item in entAttRds)
                {

                    if (entAttSol == null)
                    {
                        continue;
                    }
                    else
                    {
                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                        + " 打卡记录导入 检查异常，员工姓名：" + item.EMPLOYEENAME
                        + ",获取的考勤方案为：" + entAttSol.ATTENDANCESOLUTIONNAME + "考勤方式为(1打卡考勤，2免打卡，3登录系统，4打卡+登录系统)：" + entAttSol.ATTENDANCETYPE);
                    }

                    //考勤方案设定为不需要考勤时，跳过考勤异常检查
                    if (entAttSol.ATTENDANCETYPE == (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString())
                    {
                        item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Regular) + 1).ToString();
                        bllAttendanceRecord.ModifyAttRd(item);
                        continue;
                    }
                    if (item.ATTENDANCESTATE == (Convert.ToInt32(Common.AttendanceState.Regular) + 1).ToString())
                    {
                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                        + " 打卡记录导入 检查异常，员工姓名：" + item.EMPLOYEENAME
                        + "已检查过考勤且考勤状态为正常，跳过");
                    
                        continue;
                    }
                    //if (item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") == "2013-06-21")
                    //{
                    //}

                    //当天出勤状态
                    string strAbnormCategory = string.Empty;

                    //公共假期设置判断是否检查考勤异常
                    if (!string.IsNullOrEmpty(item.NEEDFRISTATTEND))
                    {
                        if (item.NEEDFRISTATTEND == "1")//如果公共假期设置上午上班，则第一段考勤需要考勤
                        {
                            Tracer.Debug("上午需要上班,日期：" + item.ATTENDANCEDATE + " 检测考勤公共假期设置,检测上午考勤异常，needfristattend=1 " + item.NEEDFRISTATTEND);
                            //检查第一段工作期，打卡情况
                            CheckAbnormRecordByFirstWorkTime(item, entClockInRds, ref strAbnormCategory);
                            Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                                + " 打卡记录导入 员工姓名：" + item.EMPLOYEENAME
                                + ",检测第一段工作状态为：" + strAbnormCategory);
                        }
                        if (item.NEEDSECONDATTEND == "1")//如果公共假期设置下午上班，则
                            //第二段（如果设置为三段打卡则为中午，如果设置为二段打卡则为下午），第三段考勤（如果设置了三段打卡则为下午）需要考勤
                        {   
                            //检查第二段上班，打卡情况
                            Tracer.Debug("下午需要上班，日期：" + item.ATTENDANCEDATE + " 检测考勤公共假期设置,检测中午晚上考勤异常，needsecondattend=1 " + item.NEEDFRISTATTEND);
                            
                            CheckAbnormRecordBySecondWorkTime(item, entClockInRds, ref strAbnormCategory);
                            Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                                + " 打卡记录导入 员工姓名：" + item.EMPLOYEENAME
                                + ",检测第二段工作状态为：" + strAbnormCategory);

                            //检查第三段上班，打卡情况
                            CheckAbnormRecordByThirdWorkTime(item, entClockInRds, ref strAbnormCategory);
                            Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                                + " 打卡记录导入 员工姓名：" + item.EMPLOYEENAME
                                + ",检测第三段工作状态为：" + strAbnormCategory);
                        }
                    }
                    else
                    { 
                        //检查第一段工作期，打卡情况
                        CheckAbnormRecordByFirstWorkTime(item, entClockInRds, ref strAbnormCategory);
                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                            + " 打卡记录导入 员工姓名：" + item.EMPLOYEENAME
                            + ",检测第一段工作状态为：" + strAbnormCategory);
                        //检查第二段上班，打卡情况
                        CheckAbnormRecordBySecondWorkTime(item, entClockInRds, ref strAbnormCategory);
                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                            + " 打卡记录导入 员工姓名：" + item.EMPLOYEENAME
                            + ",检测第二段工作状态为：" + strAbnormCategory);

                        //检查第三段上班，打卡情况
                        CheckAbnormRecordByThirdWorkTime(item, entClockInRds, ref strAbnormCategory);
                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                            + " 打卡记录导入 员工姓名：" + item.EMPLOYEENAME
                            + ",检测第三段工作状态为：" + strAbnormCategory);
                        //检查第四段上班，打卡情况
                        CheckAbnormRecordByFourthWorkTime(item, entClockInRds, ref strAbnormCategory);
                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd")
                            + " 打卡记录导入 员工姓名：" + item.EMPLOYEENAME
                            + ",检测第四段工作状态为：" + strAbnormCategory);
                    }                   
                    if (string.IsNullOrEmpty(strAbnormCategory))
                    {
                        item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Regular) + 1).ToString();
                    }
                    else
                    {
                        item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Abnormal) + 1).ToString();
                    }
                    item.REMARK = " 打卡记录导入检查考勤状态成功，考勤状态:" + item.ATTENDANCESTATE;
                    strMsg = bllAttendanceRecord.ModifyAttRd(item);
                    if (strMsg == "{SAVESUCCESSED}")
                    {
                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd") + " 检查异常完成，员工姓名：" + item.EMPLOYEENAME
                            + ",打卡记录导入 修改的状态为：" + item.ATTENDANCESTATE);
                    }
                    else
                    {
                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd") + " 检查异常失败，员工姓名：" + item.EMPLOYEENAME
                              + ",失败原因：" + strMsg);
                    }

                }
                //检查是否有出差及请假并确认一次状态
                string EMPLOYEEID = startItem.EMPLOYEEID;
                DateTime dtStartDate = startItem.ATTENDANCEDATE.Value;
                DateTime dtEndDate = endItem.ATTENDANCEDATE.Value;
                //检查出差，请假，外出考勤状态
                CheckEvectionRecordAndLeaveRecordAttendState(startItem.EMPLOYEEID, dtStartDate, dtEndDate, ref strMsg, ref bIsAbnorm);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                Utility.SaveLog("调用私有函数CheckAbnormRecord(代码行起点位置：631)发生错误，报错时间为:" + DateTime.Now.ToString() + "，报错原因：" + ex.ToString());
            }
        }

        /// <summary>
        /// 检查第一段上班打卡情况
        /// </summary>
        /// <param name="entAttRd"></param>
        /// <param name="entClockInRds"></param>
        /// <param name="strAbnormCategory"></param>
        private void CheckAbnormRecordByFirstWorkTime(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strAbnormCategory)
        {
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFIRSTCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFIRSTOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime,
                strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime, strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);
        }

        /// <summary>
        /// 检查第二段工作时间考勤是否存在异常
        /// </summary>
        /// <param name="entAttRd"></param>
        /// <param name="entClockInRds"></param>
        /// <param name="strAbnormCategory"></param>
        private void CheckAbnormRecordBySecondWorkTime(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strAbnormCategory)
        {
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDSECONDCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDSECONDOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.SECONDSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.SECONDENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;


            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime, strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime,
                strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);
        }

        /// <summary>
        /// 检查第三段工作时间考勤是否存在异常
        /// </summary>
        /// <param name="entAttRd"></param>
        /// <param name="entClockInRds"></param>
        /// <param name="strAbnormCategory"></param>
        private void CheckAbnormRecordByThirdWorkTime(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strAbnormCategory)
        {
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDTHIRDCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDTHIRDOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.THIRDSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.THIRDENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime, strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime,
                strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);
        }

        /// <summary>
        /// 检查第四段工作时间考勤是否存在异常
        /// </summary>
        /// <param name="entAttRd"></param>
        /// <param name="entClockInRds"></param>
        /// <param name="strAbnormCategory"></param>
        private void CheckAbnormRecordByFourthWorkTime(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strAbnormCategory)
        {
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFOURTHCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFOURTHOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FOURTHSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FOURTHENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime, strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime,
                strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);
        }


        /// <summary>
        /// 检查上班是否存在异常情况
        /// </summary>
        /// <param name="entAttRd">当前对应的考勤记录</param>
        /// <param name="entClockInList">当前日期下的所有该员工的打卡记录</param>
        /// <param name="strAttendPeriod">当前时间所属时间段(1:上午;2:中午;3:下午;4:晚上)</param>
        /// <param name="strAttStartTime">考勤记录设定的上班时间</param>
        /// <param name="strAttCardStartTime">考勤记录设定的上班打卡有效起始时间</param>
        /// <param name="strAttCardEndTime">考勤记录设定的上班打卡有效截止时间</param>
        /// <param name="bCheck">判断该段工作期是否存在未刷卡的标志位</param>
        /// <param name="strTemp">记录该段工作期是否存在未刷卡的标志位</param>
        /// <param name="strAbnormCategory">考勤异常类型</param>
        private void CheckAbnormRecordWithWorkStart(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInList, string strAttendPeriod,
            string strAttStartTime, string strAttCardStartTime, string strAttCardEndTime, ref bool bCheck, ref StringBuilder strTemp, ref string strAbnormCategory)
        {
            DateTime dtAttStart = new DateTime();
            DateTime dtAttCardStart = new DateTime();
            DateTime dtAttCardEnd = new DateTime();

            DateTime.TryParse(strAttStartTime.Trim(), out dtAttStart);
            DateTime.TryParse(strAttCardStartTime.Trim(), out dtAttCardStart);
            DateTime.TryParse(strAttCardEndTime.Trim(), out dtAttCardEnd);

            List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

            foreach (T_HR_EMPLOYEECLOCKINRECORD item in entClockInList)
            {
                item.PUNCHDATE = DateTime.Parse(item.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + item.PUNCHTIME);
                entTemps.Add(item);
            }

            //先检查该段上班
            var cls = from c in entTemps
                      where c.PUNCHDATE >= dtAttCardStart && c.PUNCHDATE <= dtAttCardEnd
                      orderby c.PUNCHDATE
                      select c;

            //liujx加2014-3-14
            try
            {
                EmployeeEntryBLL entry = new EmployeeEntryBLL();
                T_HR_EMPLOYEEENTRY EmployeeEntry = entry.GetEmployeeEntryByEmployeeID(entAttRd.EMPLOYEEID);
                //入职时间终审通过且更新时间比
                if (EmployeeEntry.CHECKSTATE == "2" && EmployeeEntry.UPDATEDATE > dtAttStart)
                {
                    Tracer.Debug("检查异常，Liujx添加员工入职逻辑退出异常检查");
                    return;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("CheckAbnormRecordWithWorkStart-获取入职记录时出错，员工ID:" + entAttRd.EMPLOYEEID + " 错误信息： " + ex.ToString());
            }
            //结束

            //无记录，即视为缺勤
            if (cls.Count() == 0)
            {
                if (CheckLeaveRecord(entAttRd, dtAttCardStart, dtAttCardEnd) == true)
                {
                    return;
                }
                bCheck = true;
                strTemp.Append("START,");
                Tracer.Debug("检查异常考勤，员工姓名："+entAttRd.EMPLOYEENAME+" 日期："+entAttRd.ATTENDANCEDATE
                +"考勤状态缺勤，未找到打卡时间，打卡时间有效范围：" + dtAttCardStart + "-" + dtAttCardEnd);
            }
            else
            {
                T_HR_EMPLOYEECLOCKINRECORD entStart = cls.FirstOrDefault();
                DateTime dtStartPunch = entStart.PUNCHDATE.Value;

                //判断迟到
                if (dtAttStart < dtStartPunch && dtAttCardEnd >= dtStartPunch)
                {                   
                    TimeSpan ts = dtStartPunch.Subtract(dtAttStart);
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Late) + 1).ToString();
                    Tracer.Debug("检查异常考勤，员工姓名：" + entAttRd.EMPLOYEENAME 
                        + " 日期：" + entAttRd.ATTENDANCEDATE
                        + " 考勤状态迟到，打卡时间有效范围：" + dtAttCardStart + "-" + dtAttCardEnd 
                        + " 打卡时间：" + dtStartPunch
                        + " 迟到分钟数：" + ts.Minutes);

                    CreateAbnormRecordByCheckClockInRd(entAttRd, ts, strAbnormCategory, strAttendPeriod, strReasonCategory);
                }
            }
        }

        /// <summary>
        /// 检查下班是否存在异常情况
        /// </summary>
        /// <param name="entAttRd">当前对应的考勤记录</param>
        /// <param name="entClockInList">当前日期下的所有该员工的打卡记录</param>
        /// <param name="strAttendPeriod">当前时间所属时间段(1:上午;2:中午;3:下午;4:晚上)</param>
        /// <param name="strAttEndTime">考勤记录设定的上班时间</param>
        /// <param name="strAttOffCardStartTime">考勤记录设定的下班打卡有效起始时间</param>
        /// <param name="strAttOffCardEndTime">考勤记录设定的下班打卡有效截止时间</param>
        /// <param name="bCheck">判断该段工作期是否存在未刷卡的标志位</param>
        /// <param name="strTemp">记录该段工作期是否存在未刷卡的标志位</param>
        /// <param name="strAbnormCategory">考勤异常类型</param>
        private void CheckAbnormRecordWithWorkOff(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInList, string strAttendPeriod, string strAttEndTime,
            string strAttOffCardStartTime, string strAttOffCardEndTime, ref bool bCheck, ref StringBuilder strTemp, ref string strAbnormCategory)
        {
            DateTime dtAttEnd = new DateTime();
            DateTime dtAttCardStart = new DateTime();
            DateTime dtAttCardEnd = new DateTime();

            DateTime.TryParse(strAttEndTime.Trim(), out dtAttEnd);
            DateTime.TryParse(strAttOffCardStartTime.Trim(), out dtAttCardStart);
            DateTime.TryParse(strAttOffCardEndTime.Trim(), out dtAttCardEnd);
            //liujx加2014-3-14
            try
            {
                EmployeeEntryBLL entry = new EmployeeEntryBLL();
                T_HR_EMPLOYEEENTRY EmployeeEntry = entry.GetEmployeeEntryByEmployeeID(entAttRd.EMPLOYEEID);
                //入职时间终审通过且更新时间比
                if (EmployeeEntry.CHECKSTATE == "2" && EmployeeEntry.UPDATEDATE > dtAttEnd)
                {
                    Tracer.Debug("检查异常，Liujx添加员工入职逻辑退出异常检查");
                    return;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("获取入职记录时出错，员工ID:" + entAttRd.EMPLOYEEID + " 错误信息： " + ex.ToString());
            }
            //end
            List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

            foreach (T_HR_EMPLOYEECLOCKINRECORD item in entClockInList)
            {
                item.PUNCHDATE = DateTime.Parse(item.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + item.PUNCHTIME);
                entTemps.Add(item);
            }

            //先检查该段下班
            var clo = from c in entTemps
                      where c.PUNCHDATE >= dtAttCardStart && c.PUNCHDATE <= dtAttCardEnd
                      orderby c.PUNCHDATE descending
                      select c;

            //无记录，即视为缺勤
            if (clo.Count() == 0)
            {
                if (CheckLeaveRecord(entAttRd, dtAttCardStart, dtAttCardEnd) == true)
                {
                    return;
                }
                bCheck = true;
                strTemp.Append("END,");

                Tracer.Debug("检查异常考勤，员工姓名：" + entAttRd.EMPLOYEENAME 
                    + " 日期：" + entAttRd.ATTENDANCEDATE
                    + " 考勤状态缺勤，未找到打卡时间，打卡时间有效范围：" + dtAttCardStart + "-" + dtAttCardEnd);
            }
            else
            {
                T_HR_EMPLOYEECLOCKINRECORD entOff = clo.FirstOrDefault();
                DateTime dtOffPunch = entOff.PUNCHDATE.Value;

                //判断早退
                if (dtAttCardStart < dtOffPunch && dtAttEnd >= dtOffPunch)
                {


                    TimeSpan ts = dtAttEnd.Subtract(dtAttCardStart);//早退时常
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.LeaveEarly) + 1).ToString();
                    Tracer.Debug("检查异常考勤，员工姓名：" + entAttRd.EMPLOYEENAME 
                        + " 日期：" + entAttRd.ATTENDANCEDATE
                        + " 考勤状态早退，打卡时间有效范围：" + dtAttCardStart + "-" + dtAttCardEnd 
                        + " 打卡时间：" + dtOffPunch
                        + " 早退分钟数：" + ts.Minutes);
                    CreateAbnormRecordByCheckClockInRd(entAttRd, ts, strAbnormCategory, strAttendPeriod, strReasonCategory);
                }
            }
        }

        /// <summary>
        /// 检查当前起止时间内是否有请假记录,如存在，
        /// 则判断当前考勤作息记录的考勤状态是否是请假，不是即更新考勤作息记录
        /// </summary>
        /// <param name="entAttRd">考勤作息记录</param>
        /// <param name="dtAttCardStart">打卡起始时间</param>
        /// <param name="dtAttCardEnd">打卡截止时间</param>
        /// <returns></returns>
        private bool CheckLeaveRecord(T_HR_ATTENDANCERECORD entAttRd, DateTime dtAttCardStart, DateTime dtAttCardEnd)
        {
            string strEmployeeId = entAttRd.EMPLOYEEID;
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            var q = from l in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                    where l.EMPLOYEEID == strEmployeeId && l.STARTDATETIME >= dtAttCardStart && l.ENDDATETIME <= dtAttCardEnd && l.CHECKSTATE == strCheckState
                    select l;

            if (q.Count() > 0)
            {
                if (entAttRd.ATTENDANCESTATE != (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString())
                {
                    entAttRd.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString();
                    AttendanceRecordBLL bllAttRd = new AttendanceRecordBLL();
                    bllAttRd.ModifyAttRd(entAttRd);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 新增考勤记录下对应的异常记录
        /// </summary>
        /// <param name="item">考勤记录</param>
        /// <param name="ts">考勤异常累计时间</param>
        /// <param name="strAbnormCategory">考勤异常类型</param>
        /// <returns></returns>
        private void CreateAbnormRecordByCheckClockInRd(T_HR_ATTENDANCERECORD item, TimeSpan ts, string strAbnormCategory, string strAttendPeriod, string strReasonCategory)
        {
            T_HR_EMPLOYEEABNORMRECORD entAbnormRecord = new T_HR_EMPLOYEEABNORMRECORD();

            entAbnormRecord.ABNORMRECORDID = System.Guid.NewGuid().ToString().ToUpper();

            entAbnormRecord.T_HR_ATTENDANCERECORDReference.EntityKey =
                   new EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCERECORD", "ATTENDANCERECORDID", item.ATTENDANCERECORDID);

            entAbnormRecord.ABNORMALDATE = item.ATTENDANCEDATE;
            entAbnormRecord.ABNORMCATEGORY = strAbnormCategory;
            entAbnormRecord.ATTENDPERIOD = strAttendPeriod;
            entAbnormRecord.ABNORMALTIME = ts.Hours * 60 + ts.Minutes;
            entAbnormRecord.SINGINSTATE = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();   //默认未签卡。"1"为未签卡，"2"为已签卡
            entAbnormRecord.REMARK = string.Empty;

            entAbnormRecord.CREATEUSERID = item.CREATEUSERID;
            entAbnormRecord.CREATEDATE = DateTime.Now;
            entAbnormRecord.UPDATEUSERID = item.UPDATEUSERID;
            entAbnormRecord.UPDATEDATE = DateTime.Now;

            entAbnormRecord.OWNERID = item.EMPLOYEEID;//.OWNERID;
            entAbnormRecord.OWNERPOSTID = item.OWNERPOSTID;
            entAbnormRecord.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
            entAbnormRecord.OWNERCOMPANYID = item.OWNERCOMPANYID;

            entAbnormRecord.CREATEPOSTID = item.CREATEPOSTID;
            entAbnormRecord.CREATEDEPARTMENTID = item.CREATEDEPARTMENTID;
            entAbnormRecord.CREATECOMPANYID = item.CREATECOMPANYID;

            AddAbnormRecord(entAbnormRecord);
        }

        /// <summary>
        /// 判断当前打卡时间属于哪个时间段
        /// </summary>
        /// <param name="strPunchTime"></param>
        /// <returns></returns>
        private string GetAttendPeriod(string strPunchTime)
        {
            string strRes = string.Empty;
            DateTime dtCheck = new DateTime();
            DateTime.TryParse(strPunchTime, out dtCheck);

            if (dtCheck.Hour > 6 && dtCheck.Hour <= 11)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Morning) + 1).ToString();
            }

            if (dtCheck.Hour > 11 && dtCheck.Hour <= 13)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Midday) + 1).ToString();
            }

            if (dtCheck.Hour > 13 && dtCheck.Hour <= 17)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Afternoon) + 1).ToString();
            }

            if (dtCheck.Hour > 17 && dtCheck.Hour <= 24)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Evening) + 1).ToString();
            }

            if (dtCheck.Hour > 0 && dtCheck.Hour <= 6)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Evening) + 1).ToString();
            }

            return strRes;
        }

        /// <summary>
        /// 根据员工id，起始日期检测考勤异常，如果存在即触发引擎发送待办任务
        /// </summary>
        /// <param name="strEmployeeId"></param>
        /// <param name="strdtStart"></param>
        /// <param name="strdtEnd"></param>
        private void AbnormRecordCheckAlarm(string strEmployeeId, string strdtStart, string strdtEnd)
        {
            DateTime dtStart = new DateTime();
            DateTime dtEnd = new DateTime();
            DateTime.TryParse(strdtStart, out dtStart);
            DateTime.TryParse(strdtEnd, out dtEnd);

            CreateTempSignInRecord(strEmployeeId, dtStart, dtEnd);
        }

        /// <summary>
        /// 出现考勤异常时，主动为有异常的员工创建一条签卡记录供其签卡
        /// </summary>
        /// <param name="strEmployeeId">员工Id</param>
        /// <param name="dtStart">打卡起始日期</param>
        /// <param name="dtEnd">打卡截止日期</param>
        private void CreateTempSignInRecord(string strEmployeeId, DateTime dtStart, DateTime dtEnd)
        {
            EmployeeBLL bllEmployee = new EmployeeBLL();
            V_EMPLOYEEDETAIL entEmployeeDetail = bllEmployee.GetEmployeeDetailView(strEmployeeId);

            if (entEmployeeDetail == null)
            {
                return;
            }

            if (entEmployeeDetail.EMPLOYEEPOSTS == null)
            {
                return;
            }

            if (entEmployeeDetail.EMPLOYEEPOSTS.Count() == 0)
            {
                return;
            }

            string strIsAgency = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
            var entEmpPost = entEmployeeDetail.EMPLOYEEPOSTS.Where(c => c.ISAGENCY == strIsAgency).FirstOrDefault();

            if (entEmpPost == null)
            {
                return;
            }

            string strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
            string strSignState = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();
            List<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords = (from ent in dal.GetObjects<T_HR_EMPLOYEEABNORMRECORD>()
                                                                where ent.OWNERID == strEmployeeId
                                                                && ent.ABNORMCATEGORY == strAbnormCategory//异常考勤
                                                                && ent.SINGINSTATE == strSignState//未签卡
                                                                && ent.ABNORMALDATE >= dtStart
                                                                && ent.ABNORMALDATE <= dtEnd
                                                                select ent).ToList();

            //string strOrderKey = "ABNORMALDATE";
            //GetAbnormRecordRdListByEmpIdAndDate(strEmployeeId, strAbnormCategory, strSignState, dtStart, dtEnd, strOrderKey);
            if (entAbnormRecords == null)
            {
                Tracer.Debug(entEmployeeDetail.EMPLOYEENAME + " 未找到未签卡的漏打卡考勤异常记录,不再生成签卡记录及待办任务。");
                return;
            }

            if (entAbnormRecords.Count() == 0)
            {
                Tracer.Debug(entEmployeeDetail.EMPLOYEENAME + " 未找到未签卡的漏打卡考勤异常记录,不再生成签卡记录及待办任务。");
                return;
            }

            bool needCreateSignInRecord = false;
            foreach (var item in entAbnormRecords)
            {
                var q = from ent in dal.GetObjects<T_HR_EMPLOYEESIGNINDETAIL>()
                        where ent.T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID == item.ABNORMRECORDID
                        select ent;
                if (q.Count() < 1)
                {
                    needCreateSignInRecord = true;
                    break;
                }

            }
            //全部异常都已生成签卡明细，返回
            if (!needCreateSignInRecord)
            {
                Tracer.Debug(entEmployeeDetail.EMPLOYEENAME + " 所有异常都已生成签卡,不再生成待办任务。");
                return;
            }


            EmployeeSignInRecordBLL bllSignInRecord = new EmployeeSignInRecordBLL();
            bllSignInRecord.ClearNoSignInRecord("T_HR_EMPLOYEESIGNINRECORD", entEmployeeDetail.EMPLOYEEID, entAbnormRecords);

            T_HR_EMPLOYEESIGNINRECORD entSignInRd = new T_HR_EMPLOYEESIGNINRECORD();
            entSignInRd.SIGNINID = Guid.NewGuid().ToString().ToUpper();
            entSignInRd.EMPLOYEEID = entEmployeeDetail.EMPLOYEEID;
            entSignInRd.EMPLOYEENAME = entEmployeeDetail.EMPLOYEENAME;
            //entSignInRd.EMPLOYEECODE = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEECODE;
            entSignInRd.SIGNINTIME = DateTime.Now;
            entSignInRd.SIGNINCATEGORY = string.Empty;
            entSignInRd.CHECKSTATE = Convert.ToInt32(Common.CheckStates.UnSubmit).ToString();
            entSignInRd.REMARK = string.Empty;
            entSignInRd.CREATEUSERID = entEmployeeDetail.EMPLOYEEID;
            entSignInRd.CREATEDATE = DateTime.Now;
            entSignInRd.UPDATEUSERID = entEmployeeDetail.EMPLOYEEID;
            entSignInRd.UPDATEDATE = DateTime.Now;
            entSignInRd.OWNERID = entEmployeeDetail.EMPLOYEEID;
            entSignInRd.OWNERPOSTID = entEmpPost.POSTID;
            entSignInRd.OWNERDEPARTMENTID = entEmpPost.DepartmentID;
            entSignInRd.OWNERCOMPANYID = entEmpPost.CompanyID;
            entSignInRd.CREATECOMPANYID = entEmpPost.CompanyID;
            entSignInRd.CREATEDEPARTMENTID = entEmpPost.DepartmentID;
            entSignInRd.CREATEPOSTID = entEmpPost.POSTID;

            List<T_HR_EMPLOYEESIGNINDETAIL> entSignInDetails = new List<T_HR_EMPLOYEESIGNINDETAIL>();
            foreach (T_HR_EMPLOYEEABNORMRECORD item in entAbnormRecords)
            {
                T_HR_EMPLOYEESIGNINDETAIL entSignInDetail = new T_HR_EMPLOYEESIGNINDETAIL();
                entSignInDetail.SIGNINDETAILID = System.Guid.NewGuid().ToString().ToUpper();
                entSignInDetail.T_HR_EMPLOYEESIGNINRECORD = entSignInRd;
                entSignInDetail.T_HR_EMPLOYEEABNORMRECORD = item;
                entSignInDetail.ABNORMALDATE = item.ABNORMALDATE;
                entSignInDetail.ABNORMCATEGORY = item.ABNORMCATEGORY;
                entSignInDetail.ATTENDPERIOD = item.ATTENDPERIOD;
                entSignInDetail.ABNORMALTIME = item.ABNORMALTIME;
                entSignInDetail.REASONCATEGORY = (Convert.ToInt32(Common.AbnormReasonCategory.DrainPunch) + 1).ToString();
                entSignInDetail.DETAILREASON = string.Empty;
                entSignInDetail.REMARK = string.Empty;
                entSignInDetail.CREATEUSERID = entEmployeeDetail.EMPLOYEEID;
                entSignInDetail.CREATEDATE = DateTime.Now;
                entSignInDetail.UPDATEUSERID = entEmployeeDetail.EMPLOYEEID;
                entSignInDetail.UPDATEDATE = DateTime.Now;
                entSignInDetail.OWNERID = entEmployeeDetail.EMPLOYEEID;
                entSignInDetail.OWNERPOSTID = entEmpPost.POSTID;
                entSignInDetail.OWNERDEPARTMENTID = entEmpPost.DepartmentID;
                entSignInDetail.OWNERCOMPANYID = entEmpPost.CompanyID;
                entSignInDetail.CREATECOMPANYID = entEmpPost.CompanyID;
                entSignInDetail.CREATEDEPARTMENTID = entEmpPost.DepartmentID;
                entSignInDetail.CREATEPOSTID = entEmpPost.POSTID;

                entSignInDetails.Add(entSignInDetail);
            }

            string strMsg = bllSignInRecord.EmployeeSignInRecordAdd(entSignInRd, entSignInDetails);

            if (strMsg != "{SAVESUCCESSED}")
            {
                return;
            }

            string submitName = string.Empty;

            EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
            EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
            userMsg.FormID = entSignInRd.SIGNINID;
            userMsg.UserID = strEmployeeId;
            EngineWS.CustomUserMsg[] List = new EngineWS.CustomUserMsg[1];
            List[0] = userMsg;
            submitName = entSignInRd.EMPLOYEENAME;
            Client.ApplicationMsgTrigger(List, "HR", "T_HR_EMPLOYEESIGNINRECORD", Utility.ObjListToXml(entSignInRd, "HR", submitName), EngineWS.MsgType.Task);
        }

        private void ClearWornSigleAndAbnormRecord(string strEmployeeId, DateTime dtStart, DateTime dtEnd)
        {
            EmployeeBLL bllEmployee = new EmployeeBLL();
            V_EMPLOYEEDETAIL entEmployeeDetail = bllEmployee.GetEmployeeDetailView(strEmployeeId);

             string strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
            string strSignState = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();
            List<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords = (from ent in dal.GetObjects<T_HR_EMPLOYEEABNORMRECORD>().Include("T_HR_ATTENDANCERECORD")
                                                                where ent.OWNERID == strEmployeeId
                                                                && ent.ABNORMCATEGORY == strAbnormCategory//异常考勤
                                                                && ent.SINGINSTATE == strSignState//未签卡
                                                                && ent.ABNORMALDATE >= dtStart
                                                                && ent.ABNORMALDATE <= dtEnd
                                                                select ent).ToList();

            //string strOrderKey = "ABNORMALDATE";
            //GetAbnormRecordRdListByEmpIdAndDate(strEmployeeId, strAbnormCategory, strSignState, dtStart, dtEnd, strOrderKey);
            if (entAbnormRecords.Count() <= 0)
            {
                Tracer.Debug(entEmployeeDetail.EMPLOYEENAME + " 未找到未签卡的漏打卡考勤异常记录,不需要处理。");
                return;
            }
            else
            {
                Tracer.Debug("处理错误的考勤异常，检查到考勤记录共：" + entAbnormRecords.Count()+"条");
                foreach (var item in entAbnormRecords)
                {   //如果当天正常，删除相关的异常考勤，签卡记录，待办任务
                    Tracer.Debug("处理错误的考勤异常，异常日期：" + item.ABNORMALDATE + "考勤状态：" + item.T_HR_ATTENDANCERECORD.ATTENDANCESTATE
                        +" 异常签卡状态："+item.SINGINSTATE);
                    if (item.T_HR_ATTENDANCERECORD.ATTENDANCESTATE == (Convert.ToInt32(Common.AttendanceState.Regular) + 1).ToString())
                    {
                        DeleteSigFromAbnormal(item);
                    }
                }
            }

        }

        #endregion
        /// <summary>
        /// 判断该段上班及下班考勤是否存在考勤异常
        /// </summary>
        /// <param name="entAttRd">当前对应的考勤记录</param>
        /// <param name="entClockInList">当前日期下的所有该员工的打卡记录</param>
        /// <param name="strAttendPeriod">当前时间所属时间段(1:上午;2:中午;3:下午;4:晚上)</param>
        /// <param name="dtAttStart">考勤记录设定的上班时间</param>
        /// <param name="dtAttEnd">考勤记录设定的下班时间</param>
        /// <param name="dtAttCardStart">考勤记录设定的上班打卡有效起始时间</param>
        /// <param name="dtAttCardEnd">考勤记录设定的上班打卡有效截止时间</param>
        /// <param name="dtAttOffCardStart">考勤记录设定的下班打卡有效起始时间</param>
        /// <param name="dtAttOffCardEnd">考勤记录设定的下班打卡有效截止时间</param>
        private void CheckAbnormRecordWithShiftDefine(
            T_HR_ATTENDANCERECORD entAttRd
            , IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds
            , string strStartAttendPeriod
            , string strOffAttendPeriod
            , string strAttStartTime
            , string strAttEndTime
            , string strAttCardStartTime
            , string strAttCardEndTime
            , string strAttOffCardStartTime
            , string strAttOffCardEndTime
            , string strNeedCard
            , string strNeedOffCard
            , ref string strAbnormCategory)
        {
            bool bCheck = false; //判断该段工作期是否存在未刷卡的标志位;
            string strNo = string.Empty, strYes = string.Empty;

            strNo = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();
            strYes = (Convert.ToInt32(Common.IsChecked.Yes) + 1).ToString();

            //上下班都不打卡，则不记录该段工作期的异常
            if (strNeedCard == strNo && strNeedOffCard == strNo)
            {
                return;
            }
            StringBuilder strTemp = new StringBuilder();

            if (strNeedCard == strYes)
            {
                CheckAbnormRecordWithWorkStart(entAttRd, entClockInRds, strStartAttendPeriod, strAttStartTime, strAttCardStartTime, strAttCardEndTime, ref bCheck, ref strTemp, ref strAbnormCategory);
            }
            if (strNeedOffCard == strYes)
            {
                CheckAbnormRecordWithWorkOff(entAttRd, entClockInRds, strOffAttendPeriod, strAttEndTime, strAttOffCardStartTime, strAttOffCardEndTime, ref bCheck, ref strTemp, ref strAbnormCategory);
            }

            if (bCheck)
            {
                string strFlag = strTemp.ToString();
                if (strFlag == "START,")
                {
                    TimeSpan ts = DateTime.Parse(strAttEndTime).Subtract(DateTime.Parse(strAttStartTime));
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
                    strReasonCategory = string.Empty;
                    CreateAbnormRecordByCheckClockInRd(entAttRd, ts, strAbnormCategory, strStartAttendPeriod, strReasonCategory);
                }
                else if (strFlag == "END,")
                {
                    TimeSpan ts = DateTime.Parse(strAttEndTime).Subtract(DateTime.Parse(strAttStartTime));
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
                    strReasonCategory = string.Empty;
                    CreateAbnormRecordByCheckClockInRd(entAttRd, ts, strAbnormCategory, strOffAttendPeriod, strReasonCategory);
                }
                else if (strFlag == "START,END,")
                {
                    TimeSpan tsStart = DateTime.Parse(strAttStartTime).Subtract(DateTime.Parse(strAttStartTime));
                    TimeSpan tsEnd = DateTime.Parse(strAttEndTime).Subtract(DateTime.Parse(strAttStartTime));
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
                    strReasonCategory = string.Empty;
                    CreateAbnormRecordByCheckClockInRd(entAttRd, tsStart, strAbnormCategory, strStartAttendPeriod, strReasonCategory);
                    CreateAbnormRecordByCheckClockInRd(entAttRd, tsEnd, strAbnormCategory, strOffAttendPeriod, strReasonCategory);
                }
            }
        }

    }
}
