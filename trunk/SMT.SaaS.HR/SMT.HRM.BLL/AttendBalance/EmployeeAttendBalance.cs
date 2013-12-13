
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    public partial class AttendMonthlyBalanceBLL : BaseBll<T_HR_ATTENDMONTHLYBALANCE>
    {
        #region 私有方法

        /// <summary>
        /// 计算员工一段时间内的考勤情况
        /// </summary>
        /// <param name="dtStart">考勤结算起始日期</param>
        /// <param name="dtEnd">考勤结算截止日期</param>
        /// <param name="entEmployees">需进行考勤结算的员工</param>
        private void CalculateEmployeeAttendMonthlyBalance(ref DateTime dtStart, ref DateTime dtEnd,
            IQueryable<T_HR_EMPLOYEE> entEmployees, string strCompanyID)
        {
            try
            {
                AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
                AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL();
                AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();
                EmployeeEntryBLL bllEntry = new EmployeeEntryBLL();
                LeftOfficeConfirmBLL bllConfirm = new LeftOfficeConfirmBLL();

                List<string> entEmployeeList = new List<string>();

                foreach (T_HR_EMPLOYEE item in entEmployees)
                {
                    string strEmployeeID = item.EMPLOYEEID;
                    if (entEmployeeList.Contains(strEmployeeID))
                    {
                        continue;
                    }

                    entEmployeeList.Add(strEmployeeID);

                    IQueryable<T_HR_ATTENDANCERECORD> entAttRds = bllAttendanceRecord.GetAttendanceRecordByEmployeeIDAndDate(strCompanyID, item.EMPLOYEEID, dtStart, dtEnd);

                    if (entAttRds.Count() == 0)
                    {
                        continue;
                    }

                    string strEmployeeId = entAttRds.FirstOrDefault().EMPLOYEEID;
                    DateTime dt = entAttRds.FirstOrDefault().ATTENDANCEDATE.Value;

                    //获取考勤记录对应的考勤方案
                    T_HR_ATTENDANCESOLUTION entAttSol = bllAttendanceSolution.GetAttendanceSolutionByEmployeeIDAndDate(strEmployeeId, dtStart, dtEnd);

                    Tracer.Debug(" 结算月度考勤结果，结算时间范围：" + dtStart.ToString("yyyy-MM-dd") + "--" + dtEnd.ToString("yyyy-MM-dd")
                        + "员工姓名：+" + item.EMPLOYEECNAME + " 使用的考勤方案名：" + entAttSol.ATTENDANCESOLUTIONNAME
                        );

                    decimal dNeedAttendDays = 0, dRealNeedAttendDays = 0;    //标称应出勤天数, 实际应出勤天数
                    decimal? dWorkTimePerDay = entAttSol.WORKTIMEPERDAY;    //每日工作时长
                    decimal dWorkServiceMonths = 0; //在职总月份数
                    DateTime dtRealStart = dtStart;
                    DateTime dtRealEnd = dtEnd;

                    //检查考勤方案定义的工作制：1 固定制；2 实际天数(按月)
                    if (entAttSol.WORKDAYTYPE == (Convert.ToInt32(Common.WorkDayType.Fixed) + 1).ToString())
                    {
                        dNeedAttendDays = entAttSol.WORKDAYS.Value; //固定制，则应出勤天数为考勤方案设定的WorkDays值                    
                        dRealNeedAttendDays = GetRealNeedAttendDaysForEmployee(ref dtRealStart, ref dtRealEnd, dNeedAttendDays, item.EMPLOYEEID, strCompanyID, entAttSol);    //实际应出勤天数（考虑到入离职）                        
                    }
                    else if (entAttSol.WORKDAYTYPE == (Convert.ToInt32(Common.WorkDayType.Fact) + 1).ToString())
                    {
                        //说明：以往是以考勤初始化记录来判断当月的应出勤天数，现在进行修正，改为实时计算
                        //dNeedAttendDays = entAttRds.Count();        //按实际天数,旧计算方式

                        dNeedAttendDays = GetNeedAttendDays(dtStart, dtEnd, strCompanyID, entAttSol);            //标称应出勤天数
                        dRealNeedAttendDays = GetRealNeedAttendDaysForEmployee(ref dtRealStart, ref dtRealEnd, dNeedAttendDays, item.EMPLOYEEID, strCompanyID, entAttSol);//实际应出勤天数（考虑到入离职）
                    }

                    if (dWorkTimePerDay == null)
                    {
                        continue;
                    }

                    if (dWorkTimePerDay.Value == 0)
                    {
                        continue;
                    }

                    GetEmployeeWorkServiceMonths(strEmployeeID, ref dWorkServiceMonths);//计算当前月员工的在职总月份数


                    T_HR_ATTENDMONTHLYBALANCE entAttendMonthlyBalance = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdByEmployeeID(strCompanyID, strEmployeeID, dtStart.Year, dtStart.Month);

                    if (entAttendMonthlyBalance != null)
                    {
                        DeleteMonthlyBalance(entAttendMonthlyBalance.MONTHLYBALANCEID);
                    }

                    entAttendMonthlyBalance = new T_HR_ATTENDMONTHLYBALANCE();
                    entAttendMonthlyBalance.MONTHLYBALANCEID = System.Guid.NewGuid().ToString().ToUpper();

                    //基础部分
                    entAttendMonthlyBalance.EMPLOYEEID = strEmployeeID;
                    entAttendMonthlyBalance.EMPLOYEECODE = item.EMPLOYEECODE;
                    entAttendMonthlyBalance.EMPLOYEENAME = item.EMPLOYEECNAME;
                    entAttendMonthlyBalance.BALANCEYEAR = dtStart.Year;
                    entAttendMonthlyBalance.BALANCEMONTH = dtStart.Month;
                    entAttendMonthlyBalance.BALANCEDATE = DateTime.Now;
                    entAttendMonthlyBalance.REMARK = string.Empty;
                    entAttendMonthlyBalance.WORKSERVICEMONTHS = dWorkServiceMonths;
                    entAttendMonthlyBalance.WORKTIMEPERDAY = dWorkTimePerDay;

                    entAttendMonthlyBalance.NEEDATTENDDAYS = dNeedAttendDays;   //应出勤天数（固定方式）
                    entAttendMonthlyBalance.REALNEEDATTENDDAYS = dRealNeedAttendDays; //应出勤天数(实际)

                    //考勤异常部分
                    decimal dAbsentMinutes = 0, dAbsentDays = 0, dLateMinutes = 0, dLateDays = 0, dLeaveEarlyDays = 0;//旷工，迟到，早退
                    int iLateTimes = 0, iLeaveEarlyTimes = 0, iForgetCardTimes = 0;//迟到，早退，漏打卡
                    decimal? dSkipWork = 0;//计算旷工总时数，以便换算成天数
                    decimal dNonWorkDays = 0;   //未计算的出勤天数

                    dNonWorkDays = GetNonWorkDays(strCompanyID, item.EMPLOYEEID, dtRealStart, dtRealEnd, dNeedAttendDays);

                    //2012-9-20 修改关于迟到的计算方式，达到4次以上（含四次），
                    //即按如下处理：旷工天数=迟到第四次算作旷工0.5天+迟到第5次算作旷工1天+未签卡旷工天数
                    CalculateSignInDetail(strEmployeeID, dtRealStart, dtRealEnd, dNeedAttendDays, dWorkTimePerDay, ref dAbsentMinutes,
                        ref dAbsentDays, ref dLateMinutes, ref dLateDays, ref dLeaveEarlyDays, ref iLateTimes, ref iLeaveEarlyTimes,
                        ref iForgetCardTimes, ref dSkipWork);

                    dAbsentDays += dNonWorkDays;
                    dAbsentMinutes += dNonWorkDays * entAttSol.WORKTIMEPERDAY.Value * 60;

                    if (entAttSol.ATTENDANCETYPE == (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString())
                    {
                        //不考勤：实际未出勤天数 = 所有请假天数之和
                        dAbsentDays = 0;
                        dAbsentMinutes = 0;
                    }

                    entAttendMonthlyBalance.LATEDAYS = dLateDays;
                    entAttendMonthlyBalance.LEAVEEARLYDAYS = dLeaveEarlyDays;
                    entAttendMonthlyBalance.LATETIMES = iLateTimes;
                    entAttendMonthlyBalance.LATEMINUTES = dLateMinutes;
                    entAttendMonthlyBalance.LEAVEEARLYTIMES = iLeaveEarlyTimes;
                    entAttendMonthlyBalance.FORGETCARDTIMES = iForgetCardTimes;
                    entAttendMonthlyBalance.ABSENTDAYS = dAbsentDays;
                    entAttendMonthlyBalance.ABSENTMINUTES = dAbsentMinutes;

                    //请假部分
                    string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
                    decimal? dAdjustLeaveDays = 0, dAffairLeaveDays = 0, dSickLeaveDays = 0, dOtherLeaveDays = 0;
                    decimal? dAnnualLevelDays = 0, dLeaveUsedDays = 0, dMarryDays = 0, dMaternityLeaveDays = 0;
                    decimal? dNursesDays = 0, dFuneralLeaveDays = 0, dTripDays = 0, dInjuryLeaveDays = 0, dPrenatalcareLeaveDays = 0;
                    decimal dSickLeaveFreeDay = 0;  //针对每月一天带薪病假使用

                    decimal dLeaveTotalDays = 0;  //实际请假天数

                    string strAttendState = (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString();
                    //增加考勤结算时计算同时存在考勤异常和请假的天数
                    string strAttendStateMix = (Convert.ToInt32(Common.AttendanceState.MixLeveAbnormal) + 1).ToString();
                    IQueryable<T_HR_ATTENDANCERECORD> entCurAttRds = entAttRds.Where(t => t.ATTENDANCESTATE == strAttendState || t.ATTENDANCESTATE == strAttendStateMix);

                    decimal dCurLeaveDays = entCurAttRds.Count();//标称请假天数

                    CalculateEmployeeLeaveDays(strEmployeeID, dtRealStart, dtRealEnd, strCheckState, dWorkTimePerDay, dNeedAttendDays, dRealNeedAttendDays, dCurLeaveDays,
                        ref dAdjustLeaveDays, ref dAffairLeaveDays, ref dSickLeaveDays, ref dOtherLeaveDays, ref dAnnualLevelDays, ref dLeaveUsedDays,
                        ref dMarryDays, ref dMaternityLeaveDays, ref dNursesDays, ref dFuneralLeaveDays, ref dTripDays, ref dInjuryLeaveDays, ref dPrenatalcareLeaveDays, ref dSickLeaveFreeDay);

                    if (dAffairLeaveDays == dRealNeedAttendDays)
                    {
                        dAffairLeaveDays = dNeedAttendDays;
                    }

                    if (dSickLeaveDays == dRealNeedAttendDays)
                    {
                        dSickLeaveDays = dNeedAttendDays;
                    }

                    if (dLeaveUsedDays == dRealNeedAttendDays)
                    {
                        dLeaveUsedDays = dNeedAttendDays;
                    }

                    if (dAnnualLevelDays == dRealNeedAttendDays)
                    {
                        dAnnualLevelDays = dNeedAttendDays;
                    }

                    if (dMaternityLeaveDays == dRealNeedAttendDays)
                    {
                        dMaternityLeaveDays = dNeedAttendDays;
                    }

                    if (dMarryDays == dRealNeedAttendDays)
                    {
                        dMarryDays = dNeedAttendDays;
                    }

                    if (dNursesDays == dRealNeedAttendDays)
                    {
                        dNursesDays = dNeedAttendDays;
                    }

                    if (dTripDays == dRealNeedAttendDays)
                    {
                        dTripDays = dNeedAttendDays;
                    }

                    if (dInjuryLeaveDays == dRealNeedAttendDays)
                    {
                        dInjuryLeaveDays = dNeedAttendDays;
                    }

                    if (dPrenatalcareLeaveDays == dRealNeedAttendDays)
                    {
                        dPrenatalcareLeaveDays = dNeedAttendDays;
                    }

                    if (dFuneralLeaveDays == dRealNeedAttendDays)
                    {
                        dFuneralLeaveDays = dNeedAttendDays;
                    }

                    if (dAdjustLeaveDays == dRealNeedAttendDays)
                    {
                        dAdjustLeaveDays = dNeedAttendDays;
                    }

                    if (dSickLeaveDays != null)
                    {
                        if (dSickLeaveDays < dSickLeaveFreeDay)
                        {
                            dSickLeaveFreeDay = dSickLeaveDays.Value;
                        }
                    }

                    entAttendMonthlyBalance.AFFAIRLEAVEDAYS = dAffairLeaveDays;
                    entAttendMonthlyBalance.SICKLEAVEDAYS = dSickLeaveDays;
                    entAttendMonthlyBalance.OTHERLEAVEDAYS = dOtherLeaveDays;
                    entAttendMonthlyBalance.ANNUALLEVELDAYS = dAnnualLevelDays;
                    entAttendMonthlyBalance.LEAVEUSEDDAYS = dLeaveUsedDays;
                    entAttendMonthlyBalance.MARRYDAYS = dMarryDays;
                    entAttendMonthlyBalance.MATERNITYLEAVEDAYS = dMaternityLeaveDays;
                    entAttendMonthlyBalance.NURSESDAYS = dNursesDays;
                    entAttendMonthlyBalance.FUNERALLEAVEDAYS = dFuneralLeaveDays;
                    entAttendMonthlyBalance.TRIPDAYS = dTripDays;
                    entAttendMonthlyBalance.INJURYLEAVEDAYS = dInjuryLeaveDays;
                    entAttendMonthlyBalance.PRENATALCARELEAVEDAYS = dPrenatalcareLeaveDays;

                    //实际请假天数 = 所有请假天数之和
                    dLeaveTotalDays = dLeaveUsedDays.Value + dAffairLeaveDays.Value + dSickLeaveDays.Value + dAnnualLevelDays.Value + dMarryDays.Value
                            + dMaternityLeaveDays.Value + dNursesDays.Value + dFuneralLeaveDays.Value + dTripDays.Value + dInjuryLeaveDays.Value + dPrenatalcareLeaveDays.Value;

                    if (dLeaveTotalDays < 0)
                    {
                        dLeaveTotalDays = 0;
                    }

                    dRealNeedAttendDays = dRealNeedAttendDays - dAbsentDays;
                    if (dRealNeedAttendDays <= 0)
                    {
                        dRealNeedAttendDays = 0;
                    }

                    if (dLeaveTotalDays != 0)
                    {
                        //如果大于等于应出勤天数，则进一步判断标称请假天数是否等于实际应出勤天数(为请假人员使用)
                        if (dLeaveTotalDays >= dRealNeedAttendDays)
                        {
                            dRealNeedAttendDays = 0;
                        }
                        else if (dLeaveTotalDays < dRealNeedAttendDays)
                        {
                            dRealNeedAttendDays = dRealNeedAttendDays - dLeaveTotalDays;
                        }
                    }
                    //根据假期设置修改实际出勤天数
                    dRealNeedAttendDays = getRealNeedAttendDaysWithVacSet(dRealNeedAttendDays, item.EMPLOYEEID, dtStart, dtEnd);
                    //实际出勤天数
                    entAttendMonthlyBalance.REALATTENDDAYS = dRealNeedAttendDays;

                    //加班部分
                    int iOverTimeTimes = 0;
                    decimal? dOverTimeSumHours = 0, dOvertimeSumDays = 0;
                    CalculateEmployeeOverTimeDays(entAttSol, strEmployeeID, dtRealStart, dtRealEnd, strCheckState, dWorkTimePerDay, ref iOverTimeTimes, ref dOverTimeSumHours, ref dOvertimeSumDays);

                    entAttendMonthlyBalance.OVERTIMETIMES = iOverTimeTimes;
                    entAttendMonthlyBalance.OVERTIMESUMHOURS = dOverTimeSumHours;
                    entAttendMonthlyBalance.OVERTIMESUMDAYS = dOvertimeSumDays;

                    //出差部分
                    decimal? dEvectionTime = 0;
                    CalculateEmployeeEvectionTime(entAttSol, entAttRds, strEmployeeID, dtRealStart, dtRealEnd, dWorkTimePerDay, ref dEvectionTime);
                    entAttendMonthlyBalance.EVECTIONTIME = dEvectionTime;

                    //外出申请时长计算部分
                    decimal? dOutApplyTime = 0;
                    CalculateEmployeeOutApplyTime(entAttSol, entAttRds, strEmployeeID, dtRealStart, dtRealEnd, dWorkTimePerDay, ref dOutApplyTime);
                    entAttendMonthlyBalance.OUTAPPLYTIME = dOutApplyTime;

                    //权限
                    entAttendMonthlyBalance.OWNERCOMPANYID = item.OWNERCOMPANYID;
                    entAttendMonthlyBalance.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                    entAttendMonthlyBalance.OWNERPOSTID = item.OWNERPOSTID;
                    entAttendMonthlyBalance.OWNERID = item.OWNERID;
                    entAttendMonthlyBalance.CREATEPOSTID = item.CREATEPOSTID;
                    entAttendMonthlyBalance.CREATEDEPARTMENTID = item.CREATEDEPARTMENTID;
                    entAttendMonthlyBalance.CREATECOMPANYID = item.CREATECOMPANYID;
                    entAttendMonthlyBalance.CREATEUSERID = item.CREATEUSERID;
                    entAttendMonthlyBalance.CREATEDATE = DateTime.Now;
                    entAttendMonthlyBalance.UPDATEUSERID = item.UPDATEUSERID;
                    entAttendMonthlyBalance.UPDATEDATE = DateTime.Now;

                    //审批
                    entAttendMonthlyBalance.CHECKSTATE = Convert.ToInt32(Common.CheckStates.UnSubmit).ToString();
                    entAttendMonthlyBalance.EDITSTATE = Convert.ToInt32(Common.EditStates.UnActived).ToString();

                    //string strMsg = AddMonthlyBalance(entAttendMonthlyBalance);
                    //if (strMsg == "{ALREADYEXISTSRECORD}")
                    //{
                    //    ModifyMonthlyBalance(entAttendMonthlyBalance);
                    //}
                    bool flag = this.IsExitEmployeeMonthlyBalance(entAttendMonthlyBalance.EMPLOYEEID, dtStart.Year, dtStart.Month);
                    if (!flag)//返回false，即没有该员工该月审核中月度考勤才进行添加
                    {
                        string strMsg = AddMonthlyBalance(entAttendMonthlyBalance);
                        if (strMsg == "{ALREADYEXISTSRECORD}")
                        {
                            ModifyMonthlyBalance(entAttendMonthlyBalance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        private decimal getRealNeedAttendDaysWithVacSet(
            decimal dRealNeedAttendDays
            , string employeeid
            , DateTime dtStart
            , DateTime dtEnd)
        {
            OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
            IQueryable<T_HR_OUTPLANDAYS> entOutPlanDays = bllOutPlanDays.GetOutPlanDaysRdListByEmployeeID(employeeid);
            string strVacDayType = (Convert.ToInt32(Common.OutPlanDaysType.WorkDay) + 1).ToString();
            IQueryable<T_HR_OUTPLANDAYS> entVacDays = entOutPlanDays.Where(s => s.DAYTYPE == strVacDayType);
            //例外工作日考勤初始化记录公共假期
            var qVacDay = from ent in entVacDays
                          where ent.STARTDATE >= dtStart
                          && ent.STARTDATE <= dtEnd
                          select ent;
            if (qVacDay.Count() > 0)
            {
                foreach (var set in qVacDay)
                {
                    if (set.ISHALFDAY == "1")
                    {
                        //设置了半天上班，实际出勤1天-0.5
                        dRealNeedAttendDays = Convert.ToDecimal(Convert.ToDouble(dRealNeedAttendDays) - 0.5);
                    }
                }
            }
            return dRealNeedAttendDays;
        }

        /// <summary>
        /// 计算考勤实际应出勤天数（针对月中入离职人员与普通在职人员的应出勤天数区别）
        /// </summary>
        /// <param name="dtStart">考勤结算起始日期</param>
        /// <param name="dtEnd">考勤结算截止日期</param>
        /// <param name="dNeedAttendDays">本月应出勤天数</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strCompanyID">员工所在公司ID</param>
        /// <param name="entAttSol">员工使用的考勤方案</param>
        /// <returns></returns>
        private decimal GetRealNeedAttendDaysForEmployee(ref DateTime dtStart, ref DateTime dtEnd, decimal dNeedAttendDays, string strEmployeeID, string strCompanyID, T_HR_ATTENDANCESOLUTION entAttSol)
        {
            decimal dRes = dNeedAttendDays;
            DateTime? dtEntry = null;
            DateTime? dtLeft = null;

            string strCheckState = Convert.ToInt32(CheckStates.Approved).ToString();

            var eEntry = from n in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")
                         where n.T_HR_EMPLOYEE.EMPLOYEEID == strEmployeeID && n.CHECKSTATE == strCheckState && n.OWNERCOMPANYID == strCompanyID
                         orderby n.UPDATEDATE descending
                         select n;

            if (eEntry == null)
            {
                return dRes;
            }

            T_HR_EMPLOYEEENTRY entEntry = eEntry.ToList().FirstOrDefault();

            if (entEntry == null)
            {
                return dRes;
            }

            if (entEntry.ENTRYDATE == null)
            {
                return dRes;
            }

            dtEntry = entEntry.ENTRYDATE.Value;

            var eLeft = from n in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>()
                        where n.EMPLOYEEID == strEmployeeID && n.CHECKSTATE == strCheckState && n.OWNERCOMPANYID == strCompanyID
                        orderby n.CREATEDATE descending
                        select n;

            if (eLeft != null)
            {
                T_HR_LEFTOFFICECONFIRM entLeft = eLeft.ToList().FirstOrDefault();

                if (entLeft != null)
                {
                    string strEmpPostID = entLeft.EMPLOYEEPOSTID;

                    var empos = from n in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                where n.EMPLOYEEPOSTID == strEmpPostID
                                select n;

                    T_HR_EMPLOYEEPOST empost = empos.ToList().FirstOrDefault();

                    if (entLeft.STOPPAYMENTDATE != null && empost != null)
                    {
                        if (empost.ISAGENCY == Convert.ToInt32(Common.IsAgencyPost.No).ToString())
                        {
                            dtLeft = entLeft.STOPPAYMENTDATE.Value;
                        }
                    }
                }
            }

            bool bCalculate = false;

            if (dtEntry != null)
            {
                if (dtEntry <= dtEnd && dtEntry > dtStart)
                {
                    dtStart = dtEntry.Value;
                    bCalculate = true;
                }
            }

            if (dtLeft != null)
            {
                if (dtLeft <= dtEnd && dtLeft > dtStart)
                {
                    dtEnd = dtLeft.Value;
                    bCalculate = true;
                }
            }

            if (bCalculate)
            {
                dRes = this.GetRealAttendDays(dtStart, dtEnd, strCompanyID, entAttSol);
            }

            return dRes;
        }

        /// <summary>
        /// 获取指定员工的在职总月份
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dWorkServiceMonths">在职总月份</param>
        private void GetEmployeeWorkServiceMonths(string strEmployeeID, ref decimal dWorkServiceMonths)
        {
            try
            {
                EmployeeBLL bllEmp = new EmployeeBLL();
                V_EMPLOYEEDETAIL entEmpDetail = bllEmp.GetEmployeeDetailView(strEmployeeID);

                if (entEmpDetail == null)
                {
                    return;
                }

                dWorkServiceMonths = entEmpDetail.WORKAGE;
            }
            catch (Exception ex)
            {
                Utility.SaveLog("执行GetEmployeeWorkServiceMonths函数获取指定员工的在职总月份失败，参数：strEmployeeID= "
                    + strEmployeeID + ",失败原因：" + ex.ToString());
            }
        }

        /// <summary>
        /// 实时计算应出勤天数
        /// </summary>
        /// <param name="dtStart">起始日期</param>
        /// <param name="dtEnd">截止日期</param>
        /// <param name="strCompanyID">所属公司</param>
        /// <param name="entAttSol">考勤方案定义</param>
        /// <returns>应出勤天数</returns>
        public decimal GetNeedAttendDays(DateTime dtStart, DateTime dtEnd, string strCompanyID, T_HR_ATTENDANCESOLUTION entAttSol)
        {
            decimal dRes = 0;
            decimal dWorkMode = entAttSol.WORKMODE.Value;
            string strAttendanceSolutionID = entAttSol.ATTENDANCESOLUTIONID;

            int iWorkMode = 0;
            int.TryParse(dWorkMode.ToString(), out iWorkMode);

            List<int> iWorkDays = new List<int>();
            Utility.GetWorkDays(iWorkMode, ref iWorkDays);

            if (entAttSol.WORKDAYTYPE != (Convert.ToInt32(Common.WorkDayType.Fact) + 1).ToString())
            {
                if (entAttSol.WORKDAYS != null)
                {
                    dRes = entAttSol.WORKDAYS.Value;
                }
                return dRes;
            }

            TimeSpan ts = dtEnd.Subtract(dtStart);
            int iTotalDay = ts.Days + 1;
            int n = 0;
            DateTime dtCheck = dtStart;

            while (n < iTotalDay)
            {
                dtCheck = dtStart.AddDays(n);
                n++;
                if (iWorkDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)) == false)
                {
                    continue;
                }

                dRes += 1;
            }

            return dRes;
        }



        /// <summary>
        /// 实时计算实际出勤天数
        /// </summary>
        /// <param name="dtStart">起始日期</param>
        /// <param name="dtEnd">截止日期</param>
        /// <param name="strCompanyID">所属公司</param>
        /// <param name="entAttSol">考勤方案定义</param>
        /// <returns>实际出勤天数</returns>
        public decimal GetRealAttendDays(DateTime dtStart, DateTime dtEnd, string strCompanyID, T_HR_ATTENDANCESOLUTION entAttSol)
        {
            decimal dRes = 0;
            decimal dWorkMode = entAttSol.WORKMODE.Value;
            string strAttendanceSolutionID = entAttSol.ATTENDANCESOLUTIONID;

            int iWorkMode = 0;
            int.TryParse(dWorkMode.ToString(), out iWorkMode);

            List<int> iWorkDays = new List<int>();
            Utility.GetWorkDays(iWorkMode, ref iWorkDays);

            TimeSpan ts = dtEnd.Subtract(dtStart);
            int iTotalDay = ts.Days + 1;
            int n = 0;
            DateTime dtCheck = dtStart;

            while (n < iTotalDay)
            {
                dtCheck = dtStart.AddDays(n);
                n++;
                if (iWorkDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)) == false)
                {
                    continue;
                }

                dRes += 1;
            }

            if (entAttSol.WORKDAYTYPE == (Convert.ToInt32(Common.WorkDayType.Fixed) + 1).ToString())
            {
                if (entAttSol.WORKDAYS != null)
                {
                    if (dRes > entAttSol.WORKDAYS.Value)
                    {
                        dRes = entAttSol.WORKDAYS.Value;
                    }
                }
            }

            return dRes;
        }

        /// <summary>
        /// 计算未进行考勤结算的天数
        /// </summary>
        /// <param name="strCompanyID">公司ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">结算起始日期</param>
        /// <param name="dtEnd">结算截至日期</param>
        /// <returns></returns>
        private decimal GetNonWorkDays(string strCompanyID, string strEmployeeID, DateTime dtStart, DateTime dtEnd, decimal dNeedAttendDays)
        {
            AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();
            IQueryable<T_HR_ATTENDANCERECORD> entAttRds = bllAttendanceRecord.GetAttendanceRecordByEmployeeIDAndDate(strCompanyID, strEmployeeID, dtStart, dtEnd);

            decimal dDays = 0;
            if (entAttRds == null)
            {
                return dDays;
            }

            int dCheckDays = entAttRds.Count();

            if (dCheckDays == 0)
            {
                return dDays;
            }

            foreach (T_HR_ATTENDANCERECORD item in entAttRds)
            {
                if (string.IsNullOrWhiteSpace(item.ATTENDANCESTATE))
                {
                    dDays += 1;
                }
            }

            if (dDays == dCheckDays)
            {
                dDays = dNeedAttendDays;
            }

            return dDays;
        }

        /// <summary>
        ///  获取计算时间内，公休假天数，方便计算应出勤天数
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="iPubVacDays"></param>
        private void GetPubVacDays(T_HR_ATTENDANCESOLUTION entAttSol, DateTime dtStart, DateTime dtEnd, ref int iPubVacDays)
        {
            try
            {
                if (entAttSol == null)
                {
                    return;
                }

                if (entAttSol.WORKMODE == null)
                {
                    return;
                }

                int iWorkmode = int.Parse(entAttSol.WORKMODE.Value.ToString());

                string strDayType = "1";

                OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                IQueryable<T_HR_OUTPLANDAYS> ents = bllOutPlanDays.GetOutPlanDaysRdListByCompanyIdAndDate(entAttSol, dtStart, dtEnd, strDayType);

                if (ents == null)
                {
                    return;
                }

                if (ents.Count() == 0)
                {
                    return;
                }

                decimal? dTotalVacDays = ents.Sum(t => t.DAYS);

                if (dTotalVacDays == null)
                {
                    return;
                }

                if (dTotalVacDays == 7)
                {
                    iPubVacDays = 3;
                }
                else if (dTotalVacDays >= 1 && dTotalVacDays < 7)
                {
                    iPubVacDays = 1;
                }

            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定员工一段时间内的各类型考勤异常的天数，次数
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">考勤日期查询起始日</param>
        /// <param name="dtEnd">考勤日期查询截止日</param>
        /// <param name="dNeedAttendDays">应出勤天数</param>
        /// <param name="dWorkTimePerDay">每日工作时长</param>
        /// <param name="dAbsentMinutes">旷工时长（分钟）</param>
        /// <param name="dAbsentDays">旷工天数</param>
        /// <param name="dLateMinutes">迟到时长（分钟）</param>
        /// <param name="dLateDays">迟到天数</param>
        /// <param name="dLeaveEarlyDays">早退天数</param>
        /// <param name="iLateTimes">迟到次数</param>
        /// <param name="iLeaveEarlyTimes">早退次数</param>
        /// <param name="iForgetCardTimes">漏打卡次数</param>
        /// <param name="dSkipWork">旷工天数</param>
        private static void CalculateSignInDetail(string strEmployeeID, DateTime dtStart, DateTime dtEnd, decimal dNeedAttendDays, decimal? dWorkTimePerDay,
            ref decimal dAbsentMinutes, ref decimal dAbsentDays, ref decimal dLateMinutes, ref decimal dLateDays, ref decimal dLeaveEarlyDays,
            ref int iLateTimes, ref int iLeaveEarlyTimes, ref int iForgetCardTimes, ref decimal? dSkipWork)
        {
            try
            {
                AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
                IQueryable<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords = bllAbnormRecord.GetAbnormRecordRdListByEmpIdAndDate(strEmployeeID, dtStart, dtEnd);

                string strCheckState = Convert.ToInt32(CheckStates.Approved).ToString();

                if (entAbnormRecords.Count() == 0)
                {
                    return;
                }

                foreach (T_HR_EMPLOYEEABNORMRECORD item in entAbnormRecords)
                {
                    if (item.ABNORMCATEGORY == (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString())
                    {
                        if (item.SINGINSTATE != (Convert.ToInt32(Common.IsChecked.Yes) + 1).ToString())
                        {
                            dSkipWork += item.ABNORMALTIME;
                        }
                        else
                        {
                            EmployeeSignInDetailBLL bllSignInDetail = new EmployeeSignInDetailBLL();
                            T_HR_EMPLOYEESIGNINDETAIL ent = bllSignInDetail.GetEmployeeSignInDetailByAbnormRecordIDAndCheckState(item.ABNORMRECORDID, strCheckState);

                            if (ent == null)
                            {
                                continue;
                            }

                            if (ent.REASONCATEGORY == (Convert.ToInt32(Common.AbnormReasonCategory.DrainPunch) + 1).ToString())
                            {
                                iForgetCardTimes += 1;
                            }
                        }
                    }
                    else if (item.ABNORMCATEGORY == (Convert.ToInt32(Common.AttendAbnormalType.Late) + 1).ToString())
                    {
                        iLateTimes += 1;
                        dLateMinutes += item.ABNORMALTIME.Value;
                    }
                    else if (item.ABNORMCATEGORY == (Convert.ToInt32(Common.AttendAbnormalType.LeaveEarly) + 1).ToString())
                    {
                        iLeaveEarlyTimes += 1;
                    }
                }

                string strLateCate = (Convert.ToInt32(Common.AttendAbnormalType.Late) + 1).ToString();
                IQueryable<T_HR_EMPLOYEEABNORMRECORD> lat = entAbnormRecords.Where(c => c.ABNORMCATEGORY == strLateCate);
                dLateDays = lat.Select(s => s.T_HR_ATTENDANCERECORD).Distinct().Count();

                string strLeaveEarlyCate = (Convert.ToInt32(Common.AttendAbnormalType.LeaveEarly) + 1).ToString();
                IQueryable<T_HR_EMPLOYEEABNORMRECORD> lea = entAbnormRecords.Where(c => c.ABNORMCATEGORY == strLeaveEarlyCate);
                dLeaveEarlyDays = lea.Select(s => s.T_HR_ATTENDANCERECORD).Distinct().Count();

                dAbsentMinutes = dSkipWork.Value;
                dWorkTimePerDay = dWorkTimePerDay.Value * 60;
                dAbsentDays = decimal.Round(dSkipWork.Value / dWorkTimePerDay.Value, 0);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定员工一段时间内的各类请假天数
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">请假日期起始时间</param>
        /// <param name="dtEnd">请假日期截止时间</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="dWorkTimePerDay">日工作时长</param>
        /// <param name="dNeedAttendDays">标称应出勤天数</param>
        /// <param name="dRealNeedAttendDays">实际应出勤天数</param>
        /// <param name="dCurLeaveDays">标称应出勤天数</param>
        /// <param name="dAffairLeaveDays">事假天数</param>
        /// <param name="dSickLeaveDays">病假天数</param>
        /// <param name="dOtherLeaveDays">其他假期天数</param>
        /// <param name="dAnnualLevelDays">年休假天数</param>
        /// <param name="dLeaveUsedDays">调休假天数</param>
        /// <param name="dMarryDays">婚假天数</param>
        /// <param name="dMaternityLeaveDays">产假天数</param>
        /// <param name="dNursesDays">看护假天数</param>
        /// <param name="dFuneralLeaveDays">丧假天数</param>
        /// <param name="dTripDays">路程假天数</param>
        /// <param name="dInjuryLeaveDays">工伤假天数</param>
        /// <param name="dPrenatalcareLeaveDays">产前检查假天数</param>
        /// <param name="dInjuryLeaveDays">每月一天带薪病假</param>
        private void CalculateEmployeeLeaveDays(string strEmployeeID, DateTime dtStart, DateTime dtEnd, string strCheckState, decimal? dWorkTimePerDay,
            decimal dNeedAttendDays, decimal dRealNeedAttendDays, decimal dCurLeaveDays, ref decimal? dAdjustLeaveDays, ref decimal? dAffairLeaveDays,
            ref decimal? dSickLeaveDays, ref decimal? dOtherLeaveDays, ref decimal? dAnnualLevelDays, ref decimal? dLeaveUsedDays, ref decimal? dMarryDays,
            ref decimal? dMaternityLeaveDays, ref decimal? dNursesDays, ref decimal? dFuneralLeaveDays, ref decimal? dTripDays, ref decimal? dInjuryLeaveDays,
            ref decimal? dPrenatalcareLeaveDays, ref decimal dSickLeaveFreeDay)
        {
            try
            {
                EmployeeLeaveRecordBLL bllEmpLeaveRd = new EmployeeLeaveRecordBLL();
                dtEnd = dtEnd.AddDays(1).AddSeconds(-1);
                IQueryable<T_HR_EMPLOYEELEAVERECORD> entEmployeeLeaveRecords = bllEmpLeaveRd.GetEmployeeLeaveRdListByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd, strCheckState);

                if (entEmployeeLeaveRecords == null)
                {
                    return;
                }

                if (entEmployeeLeaveRecords.Count() == 0)
                {
                    return;
                }

                foreach (T_HR_EMPLOYEELEAVERECORD entEmployeeLeaveRecord in entEmployeeLeaveRecords)
                {
                    decimal? dLeaveTotalHours = 0;//计算实际请假总时长
                    decimal? dCancelLeaveTotalHours = 0;//计算实际销假总时长
                    if (entEmployeeLeaveRecord.TOTALHOURS == null)
                    {
                        continue;
                    }

                    AdjustLeaveBLL bllAdjustLeave = new AdjustLeaveBLL();
                    T_HR_ADJUSTLEAVE entAdjustLeave = bllAdjustLeave.GetAdjustLeaveByLeaveRecordID(entEmployeeLeaveRecord.LEAVERECORDID);

                    EmployeeCancelLeaveBLL bllCancelLeave = new EmployeeCancelLeaveBLL();
                    //T_HR_EMPLOYEECANCELLEAVE entCancelLeave = bllCancelLeave.GetEmployeeLeaveRdListByLeaveRecordID(entEmployeeLeaveRecord.LEAVERECORDID, strCheckState);
                    IQueryable<T_HR_EMPLOYEECANCELLEAVE> entCancelLeaveList = bllCancelLeave.GetEmployeeLeaveRdListByLeaveRecordID(entEmployeeLeaveRecord.LEAVERECORDID, strCheckState);

                    T_HR_LEAVETYPESET entLeaveTypeSet = entEmployeeLeaveRecord.T_HR_LEAVETYPESET;

                    if (entLeaveTypeSet == null)
                    {
                        continue;
                    }

                    if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AffairLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dAffairLeaveDays += dLeaveTotalHours;

                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }
                        else if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.AdjLevDeduct) + 1).ToString() || entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.AdjLevPaidDayDeduct) + 1).ToString())
                        {
                            if (entAdjustLeave != null)
                            {

                                dAdjustLeaveDays += entAdjustLeave.ADJUSTLEAVEDAYS * dWorkTimePerDay.Value;
                            }

                        }


                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dAffairLeaveDays = dAffairLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.SickLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dSickLeaveDays += dLeaveTotalHours;

                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dSickLeaveFreeDay = 1;
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }
                        else if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.AdjLevDeduct) + 1).ToString() || entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.AdjLevPaidDayDeduct) + 1).ToString())
                        {
                            dSickLeaveFreeDay = 1;
                            if (entAdjustLeave != null)
                            {
                                dAdjustLeaveDays += entAdjustLeave.ADJUSTLEAVEDAYS * dWorkTimePerDay.Value;
                            }

                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dSickLeaveDays = dSickLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dLeaveUsedDays += dLeaveTotalHours;

                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }


                        dLeaveUsedDays = dLeaveUsedDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AnnualLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dAnnualLevelDays += dLeaveTotalHours;

                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }


                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dAnnualLevelDays = dAnnualLevelDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.MaternityLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dMaternityLeaveDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dMaternityLeaveDays = dMaternityLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.MarryLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dMarryDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.NursesLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dNursesDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dNursesDays = dNursesDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.TripLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dTripDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }


                        dTripDays = dTripDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.InjuryLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dInjuryLeaveDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dInjuryLeaveDays = dInjuryLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.PrenatalcareLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dPrenatalcareLeaveDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dPrenatalcareLeaveDays = dPrenatalcareLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.FuneralLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dFuneralLeaveDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dFuneralLeaveDays = dFuneralLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                }

                dWorkTimePerDay = dWorkTimePerDay.Value;
                string strNumOfDecDefault = "0.5";

                if (dAdjustLeaveDays == null)
                {
                    dAdjustLeaveDays = 0;
                }

                if (dAffairLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dAffairLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dSickLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dSickLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dLeaveUsedDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dLeaveUsedDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dAnnualLevelDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dAnnualLevelDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dMaternityLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dMaternityLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dMarryDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dMarryDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dNursesDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dNursesDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dTripDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dTripDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dInjuryLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dInjuryLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dFuneralLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dFuneralLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                dAffairLeaveDays = RoundOff(dAffairLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dSickLeaveDays = RoundOff(dSickLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dOtherLeaveDays = 0;
                dLeaveUsedDays = RoundOff(dLeaveUsedDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dAnnualLevelDays = RoundOff(dAnnualLevelDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dMaternityLeaveDays = RoundOff(dMaternityLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dMarryDays = RoundOff(dMarryDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dNursesDays = RoundOff(dNursesDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dTripDays = RoundOff(dTripDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dInjuryLeaveDays = RoundOff(dInjuryLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dFuneralLeaveDays = RoundOff(dFuneralLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dPrenatalcareLeaveDays = RoundOff(dPrenatalcareLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dAdjustLeaveDays = RoundOff(dAdjustLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 获取员工考勤结算月每条 请假/销假 的实际总时长
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtLeaveStart">当前请假起始日期</param>
        /// <param name="dtLeaveEnd">当前请假结束日期</param>
        /// <param name="dtCurrMonthStart">考勤结算月起始日期</param>
        /// <param name="dtCurrMonthEnd">考勤结算月截止日期</param>
        /// <param name="dLeaveTotalHours">单条 请假/销假 的实际总时长</param>
        private void CalculateRealCurrMonthLeaveDays(string strEmployeeID, DateTime dtLeaveStart, DateTime dtLeaveEnd,
            DateTime dtCurrMonthStart, DateTime dtCurrMonthEnd, ref decimal? dLeaveTotalHours)
        {
            if (string.IsNullOrWhiteSpace(strEmployeeID))
            {
                return;
            }

            if (dtLeaveStart == null || dtLeaveEnd == null)
            {
                return;
            }

            DateTime dtCalculateStart = new DateTime();
            DateTime dtCalculateEnd = new DateTime();
            DateTime dtCheck = new DateTime();

            //判断 请假/销假 起始日期是否小于考勤结算月起始日期，根据日期大小进一步判断 请假/销假 结束日期与考勤结算截止日期大小，从而获取
            // 请假/销假 实际请假的起止日期，以便计算实际的  请假/销假 天数
            if (dtLeaveStart < dtCurrMonthStart)
            {
                if (dtLeaveEnd >= dtCurrMonthEnd)
                {
                    dtCalculateStart = dtCurrMonthStart;
                    dtCalculateEnd = dtCurrMonthEnd;
                }
                else
                {
                    if (dtLeaveEnd > dtCurrMonthStart)
                    {
                        dtCalculateStart = dtCurrMonthStart;
                        dtCalculateEnd = dtLeaveEnd;
                    }
                }
            }
            else
            {
                dtCalculateStart = dtLeaveStart;

                if (dtLeaveEnd >= dtCurrMonthEnd)
                {
                    dtCalculateStart = dtLeaveStart;
                    dtCalculateEnd = dtCurrMonthEnd;
                }
                else
                {
                    dtCalculateEnd = dtLeaveEnd;
                }
            }

            if (dtCalculateStart <= dtCheck || dtCalculateEnd <= dtCheck)
            {
                return;
            }

            decimal dRealLeaveDays = 0, dRealLeaveHours = 0, dRealLeaveTotalHours = 0;
            string strId = System.Guid.NewGuid().ToString();
            EmployeeLeaveRecordBLL bllLeaveRecord = new EmployeeLeaveRecordBLL();
            bllLeaveRecord.GetRealLeaveDayByEmployeeIdAndDate(strId, strEmployeeID, dtCalculateStart, dtCalculateEnd, ref dRealLeaveDays, ref dRealLeaveHours, ref dRealLeaveTotalHours);

            dLeaveTotalHours = dRealLeaveTotalHours;
        }

        /// <summary>
        /// 四舍五入浮点数
        /// </summary>
        /// <param name="dValue">浮点数</param>
        /// <param name="strNumOfDec">小数位数值比较值</param>
        /// <param name="ilength"></param>
        /// <returns></returns>
        private decimal RoundOff(decimal dValue, string strNumOfDec, int ilength)
        {
            decimal dRes = 0;
            try
            {
                if (dValue == 0)
                {
                    return dRes;
                }

                dRes = decimal.Round(dValue, ilength);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

            return dRes;
        }

        #region 计算加班，出差，外出申请时长
        /// <summary>
        /// 计算指定员工一段时间内的加班情况
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="strCheckState"></param>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="iOverTimeTimes"></param>
        /// <param name="dOverTimeSumHours"></param>
        /// <param name="dOvertimeSumDays"></param>
        private void CalculateEmployeeOverTimeDays(T_HR_ATTENDANCESOLUTION entAttSol, string strEmployeeID, DateTime dtStart, DateTime dtEnd, string strCheckState,
            decimal? dWorkTimePerDay, ref int iOverTimeTimes, ref decimal? dOverTimeSumHours, ref decimal? dOvertimeSumDays)
        {
            try
            {
                if (entAttSol.OVERTIMEPAYTYPE == (Convert.ToInt32(Common.OverTimePayType.NoPay) + 1).ToString())
                {
                    return;
                }

                T_HR_OVERTIMEREWARD entOvertimereward = entAttSol.T_HR_OVERTIMEREWARD;  //获取员工加班报酬倍率设置记录

                EmployeeBLL bllEmployee = new EmployeeBLL();
                V_EMPLOYEEPOST entEmployeeDetail = bllEmployee.GetEmployeeDetailByID(strEmployeeID);    //获取员工的关联信息，如公司，岗位，部门，个人信息等

                //获取员工 所在地区/国家
                string strCountryType = "1";    //默认为"1"，即中国
                if (entEmployeeDetail.EMPLOYEEPOSTS != null)
                {
                    if (entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST != null)
                    {
                        if (entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT != null)
                        {
                            if (entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY != null)
                            {
                                if (!string.IsNullOrWhiteSpace(entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COUNTYTYPE))
                                {
                                    strCountryType = entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COUNTYTYPE;
                                }
                            }
                        }
                    }
                }
                int iWorkMode = int.Parse(entAttSol.WORKMODE.Value.ToString());                                                 //获取当前应用的考勤方案的工作制,即每周工作几天

                List<int> iWeekDays = GetWeekDayList(iWorkMode, dtStart, dtEnd);        //获取员工当月休假日序号
                List<int> iVacDays = GetVacDayList(strCountryType, dtStart, dtEnd);     //获取员工当月休息日序号

                //获取员工的加班记录(审核通过)
                OverTimeRecordBLL bllOverTimeRecord = new OverTimeRecordBLL();
                IQueryable<T_HR_EMPLOYEEOVERTIMERECORD> entEmpOverTimeRds = bllOverTimeRecord.GetOverTimeRdListByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd, strCheckState);

                //检查当前考勤方案是否要求加班打卡
                if (entAttSol.OVERTIMECHECK == (Convert.ToInt32(Common.IsChecked.No) + 1).ToString())
                {
                    //要求加班打卡，则检查当前考勤方案的加班生效方式(0 审核通过的加班申请；1 超过工作时间外自动累计；2 仅节假日累计；)
                    //判断当前加班生效方式是否为：0 审核通过的加班申请；
                    if (entAttSol.OVERTIMEVALID == (Convert.ToInt32(Common.OverTimeValid.ToCheck) + 1).ToString())
                    {
                        if (entEmpOverTimeRds.Count() == 0)
                        {
                            return;
                        }

                        iOverTimeTimes = entEmpOverTimeRds.Count();

                        foreach (T_HR_EMPLOYEEOVERTIMERECORD entEmpOverTimeRd in entEmpOverTimeRds)
                        {
                            DateTime dtNoCheckStartDate = entEmpOverTimeRd.STARTDATE.Value;
                            DateTime dtNoCheckEndDate = entEmpOverTimeRd.STARTDATE.Value;
                            string strStartTime = entEmpOverTimeRd.STARTDATETIME;
                            string strEndTime = entEmpOverTimeRd.ENDDATETIME;

                            GetOTSumHoursByNoCheck(dWorkTimePerDay, entOvertimereward, iWeekDays, iVacDays, dtNoCheckStartDate, dtNoCheckEndDate, strStartTime, strEndTime, ref dOverTimeSumHours);
                        }
                    }
                }
                else
                {
                    //预设加班报酬倍率变量，以便后面赋值使用
                    decimal? dVacPayRate = 1;

                    //要求加班打卡，则检查当前考勤方案的加班生效方式(0 审核通过的加班申请；1 超过工作时间外自动累计；2 仅节假日累计；)
                    //判断当前加班生效方式是否为：0 审核通过的加班申请；
                    if (entAttSol.OVERTIMEVALID == (Convert.ToInt32(Common.OverTimeValid.ToCheck) + 1).ToString())
                    {
                        //当前加班生效方式为：0 审核通过的加班申请
                        //无加班申请，返回
                        if (entEmpOverTimeRds.Count() == 0)
                        {
                            return;
                        }

                        foreach (T_HR_EMPLOYEEOVERTIMERECORD entEmpOverTimeRd in entEmpOverTimeRds)
                        {
                            DateTime dtEmpOTStartDate = entEmpOverTimeRd.STARTDATE.Value;
                            DateTime dtEmpOTEndDate = entEmpOverTimeRd.ENDDATE.Value;

                            IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRecords = from c in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                                                                                       where c.PUNCHDATE >= entEmpOverTimeRd.STARTDATE && c.PUNCHDATE <= entEmpOverTimeRd.ENDDATE
                                                                                       orderby c.PUNCHDATE
                                                                                       select c;
                            //无打卡记录，转向下一条记录
                            if (entClockInRecords.Count() == 0)
                            {
                                continue;
                            }

                            //当日加班，加班次数累加1次
                            if (entEmpOverTimeRd.STARTDATE == entEmpOverTimeRd.ENDDATE)
                            {
                                iOverTimeTimes += 1;
                            }

                            //重新填充打卡记录，打卡日期附加打卡时间
                            List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

                            foreach (T_HR_EMPLOYEECLOCKINRECORD entTempRd in entClockInRecords)
                            {
                                entTempRd.PUNCHDATE = DateTime.Parse(entTempRd.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + entTempRd.PUNCHTIME);
                                entTemps.Add(entTempRd);
                            }

                            var cls = from c in entTemps
                                      orderby c.PUNCHDATE
                                      select c;

                            T_HR_EMPLOYEECLOCKINRECORD entFirst = entTemps.FirstOrDefault();
                            T_HR_EMPLOYEECLOCKINRECORD entLast = entTemps.LastOrDefault();

                            TimeSpan tsStart = new TimeSpan(dtEmpOTStartDate.Ticks);
                            TimeSpan tsEnd = new TimeSpan(dtEmpOTEndDate.Ticks);
                            TimeSpan ts = tsEnd.Subtract(tsStart).Duration();
                            if (ts.Days == 0)
                            {
                                decimal dHours = 0;
                                GetOTSumHoursByLessOT(dWorkTimePerDay, entOvertimereward, iWeekDays, iVacDays, dtEmpOTStartDate, ts, ref dOverTimeSumHours, ref dVacPayRate, ref dHours);
                            }
                            else
                            {
                                for (int i = 0; i < ts.Days; i++)
                                {
                                    DateTime dtCheck = DateTime.Parse(dtEmpOTStartDate.AddDays(i).ToString("yyyy-MM-dd") + "0:00:00");
                                    DateTime dtCheckEnd = DateTime.Parse(dtEmpOTStartDate.AddDays(i).ToString("yyyy-MM-dd") + "23:59:59");

                                    //检查加班日是否为节假日
                                    if (iVacDays.Contains(dtCheck.Day))
                                    {
                                        iOverTimeTimes += 1;
                                        //获取节假日加班报酬倍率
                                        if (entOvertimereward != null)
                                        {
                                            dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                                        }

                                        GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                    }
                                    else
                                    {
                                        //检查加班日是否为休息日
                                        if (iWeekDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)))
                                        {
                                            iOverTimeTimes += 1;
                                            //获取休息日加班报酬倍率
                                            if (entOvertimereward != null)
                                            {
                                                dVacPayRate = entOvertimereward.WEEKENDPAYRATE.Value == 0 ? 0 : entOvertimereward.WEEKENDPAYRATE;
                                            }

                                            GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                        }
                                        else
                                        {
                                            iOverTimeTimes += 1;
                                            //获取工作日加班报酬倍率
                                            if (entOvertimereward != null)
                                            {
                                                dVacPayRate = entOvertimereward.USUALOVERTIMEPAYRATE.Value == 0 ? 0 : entOvertimereward.USUALOVERTIMEPAYRATE;
                                            }

                                            GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else if (entAttSol.OVERTIMEVALID == (Convert.ToInt32(Common.OverTimeValid.AutoAccumulate) + 1).ToString())
                    {
                        IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRecords = from c in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                                                                                   where c.PUNCHDATE >= dtStart && c.PUNCHDATE <= dtEnd
                                                                                   orderby c.PUNCHDATE
                                                                                   select c;
                        //无打卡记录，转向下一条记录
                        if (entClockInRecords.Count() == 0)
                        {
                            return;
                        }

                        List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

                        foreach (T_HR_EMPLOYEECLOCKINRECORD entTempRd in entClockInRecords)
                        {
                            entTempRd.PUNCHDATE = DateTime.Parse(entTempRd.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + entTempRd.PUNCHTIME);
                            entTemps.Add(entTempRd);
                        }

                        var cls = from c in entTemps
                                  orderby c.PUNCHDATE
                                  select c;

                        T_HR_EMPLOYEECLOCKINRECORD entFirst = entTemps.FirstOrDefault();
                        T_HR_EMPLOYEECLOCKINRECORD entLast = entTemps.LastOrDefault();

                        TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
                        TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
                        TimeSpan ts = tsEnd.Subtract(tsStart).Duration();
                        if (ts.Days > 0)
                        {
                            for (int i = 0; i < ts.Days; i++)
                            {
                                DateTime dtCheck = DateTime.Parse(dtStart.AddDays(i).ToString("yyyy-MM-dd") + "0:00:00");
                                DateTime dtCheckEnd = DateTime.Parse(dtStart.AddDays(i).ToString("yyyy-MM-dd") + "23:59:59");

                                //检查加班日是否为节假日
                                if (iVacDays.Contains(dtCheck.Day))
                                {
                                    iOverTimeTimes += 1;
                                    //获取节假日加班报酬倍率
                                    if (entOvertimereward != null)
                                    {
                                        dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                                    }

                                    GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                }
                                else
                                {
                                    //检查加班日是否为休息日
                                    if (iWeekDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)))
                                    {
                                        iOverTimeTimes += 1;
                                        //获取休息日加班报酬倍率
                                        if (entOvertimereward != null)
                                        {
                                            dVacPayRate = entOvertimereward.WEEKENDPAYRATE.Value == 0 ? 0 : entOvertimereward.WEEKENDPAYRATE;
                                        }

                                        GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                    }
                                    else
                                    {
                                        iOverTimeTimes += 1;
                                        //获取工作日加班报酬倍率
                                        if (entOvertimereward != null)
                                        {
                                            dVacPayRate = entOvertimereward.USUALOVERTIMEPAYRATE.Value == 0 ? 0 : entOvertimereward.USUALOVERTIMEPAYRATE;
                                        }

                                        GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                    }
                                }
                            }
                        }
                    }
                    else if (entAttSol.OVERTIMEVALID == (Convert.ToInt32(Common.OverTimeValid.OnlyHoliday) + 1).ToString())
                    {
                        IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRecords = from c in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                                                                                   where c.PUNCHDATE >= dtStart && c.PUNCHDATE <= dtEnd
                                                                                   orderby c.PUNCHDATE
                                                                                   select c;
                        //无打卡记录，转向下一条记录
                        if (entClockInRecords.Count() == 0)
                        {
                            return;
                        }

                        List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

                        foreach (T_HR_EMPLOYEECLOCKINRECORD entTempRd in entClockInRecords)
                        {
                            entTempRd.PUNCHDATE = DateTime.Parse(entTempRd.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + entTempRd.PUNCHTIME);
                            entTemps.Add(entTempRd);
                        }

                        var cls = from c in entTemps
                                  orderby c.PUNCHDATE
                                  select c;

                        T_HR_EMPLOYEECLOCKINRECORD entFirst = entTemps.FirstOrDefault();
                        T_HR_EMPLOYEECLOCKINRECORD entLast = entTemps.LastOrDefault();

                        TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
                        TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
                        TimeSpan ts = tsEnd.Subtract(tsStart).Duration();
                        if (ts.Days > 0)
                        {
                            for (int i = 0; i < ts.Days; i++)
                            {
                                DateTime dtCheck = DateTime.Parse(dtStart.AddDays(i).ToString("yyyy-MM-dd") + "0:00:00");
                                DateTime dtCheckEnd = DateTime.Parse(dtStart.AddDays(i).ToString("yyyy-MM-dd") + "23:59:59");

                                //检查加班日是否为节假日
                                if (iVacDays.Contains(dtCheck.Day))
                                {
                                    iOverTimeTimes += 1;
                                    //获取节假日加班报酬倍率
                                    if (entOvertimereward != null)
                                    {
                                        dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                                    }

                                    GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                }
                            }
                        }
                    }
                }

                dWorkTimePerDay = dWorkTimePerDay.Value * 60;
                dOvertimeSumDays = decimal.Round(dOverTimeSumHours.Value / dWorkTimePerDay.Value, 0);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定员工一段时间内的出差情况
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="dEvectionTime"></param>
        private void CalculateEmployeeEvectionTime(T_HR_ATTENDANCESOLUTION entAttSol, IQueryable<T_HR_ATTENDANCERECORD> entAttRds, string strEmployeeID, DateTime dtStart, DateTime dtEnd, decimal? dWorkTimePerDay, ref decimal? dEvectionTime)
        {
            try
            {
                string strAttState = (Convert.ToInt32(Common.AttendanceState.Travel) + 1).ToString();
                //考勤结算时计算同时存在考勤异常和出差天数
                string strAttendStateMix = (Convert.ToInt32(Common.AttendanceState.MixTravelAbnormal) + 1).ToString();
                IQueryable<T_HR_ATTENDANCERECORD> entAttRdTemps = from r in entAttRds
                                                                  where r.ATTENDANCESTATE == strAttState
                                                                  || r.ATTENDANCESTATE == strAttendStateMix
                                                                  select r;

                if (entAttRdTemps.Count() == 0)
                {
                    return;
                }
                else
                {
                    dEvectionTime = entAttRdTemps.Count();
                }

                //decimal? dCurEvecDays = entAttRdTemps.Count();

                //IQueryable<T_HR_EMPLOYEEEVECTIONRECORD> entEvecRds = from n in dal.GetObjects<T_HR_EMPLOYEEEVECTIONRECORD>()
                //                                                     where n.EMPLOYEEID == strEmployeeID 
                //                                                     && n.STARTDATE>=dtStart
                //                                                     && n.ENDDATE <=dtEnd
                //                                                     select n;

                //if (entEvecRds.Count() == 0)
                //{
                //    return;
                //}
                //List<string> startDayList = new List<string>();
                //foreach (T_HR_EMPLOYEEEVECTIONRECORD item in entEvecRds)
                //{
                //    string starDay=item.STARTDATE.Value.ToString("yyyy-MM-dd");
                //    if (startDayList.Contains(starDay))
                //    {
                //        continue;
                //    }
                //    else
                //    {
                //        dEvectionTime += item.TOTALDAYS;
                //        startDayList.Add(starDay);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定员工一段时间内的外出申请情况
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="dOutApplyHours"></param>
        private void CalculateEmployeeOutApplyTime(T_HR_ATTENDANCESOLUTION entAttSol, IQueryable<T_HR_ATTENDANCERECORD> entAttRds, string strEmployeeID, DateTime dtStart, DateTime dtEnd, decimal? dWorkTimePerDay, ref decimal? dOutApplyHours)
        {
            try
            {
                string strAttState = (Convert.ToInt32(Common.AttendanceState.OutApply) + 1).ToString();
                //考勤结算时计算同时存在考勤异常和出差天数
                string strAttendStateMix = (Convert.ToInt32(Common.AttendanceState.MixOutApplyAbnormal) + 1).ToString();
                IQueryable<T_HR_ATTENDANCERECORD> entAttRdTemps = from r in entAttRds
                                                                  where r.ATTENDANCESTATE == strAttState
                                                                  || r.ATTENDANCESTATE == strAttendStateMix
                                                                  select r;

                if (entAttRdTemps.Count() == 0)
                {
                    return;
                }

                //decimal? dCurEvecDays = entAttRdTemps.Count();

                IQueryable<T_HR_OUTAPPLYRECORD> entEvecRds = from n in dal.GetObjects<T_HR_OUTAPPLYRECORD>()
                                                             where n.EMPLOYEEID == strEmployeeID
                                                             && n.STARTDATE >= dtStart
                                                             && n.ENDDATE <= dtEnd
                                                             && n.CHECKSTATE == "2"
                                                             select n;

                if (entEvecRds.Count() == 0)
                {
                    return;
                }
                //decimal? dCheckEveDays = 0;
                //DateTime dtCheckStart = new DateTime(), dtCheckEnd = new DateTime();
                foreach (T_HR_OUTAPPLYRECORD item in entEvecRds)
                {
                    decimal dHOURS = 0;
                    decimal.TryParse(item.OUTAPLLYTIMES, out dHOURS);
                    dOutApplyHours += dHOURS;
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        #endregion

        /// <summary>
        /// 无需打卡，但需要审核的加班，计算其加班时长
        /// </summary>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="entOvertimereward"></param>
        /// <param name="iWeekDays"></param>
        /// <param name="iVacDays"></param>
        /// <param name="dtStartDate"></param>
        /// <param name="dtEndDate"></param>
        /// <param name="strStartTime"></param>
        /// <param name="strEndTime"></param>
        /// <param name="dOverTimeSumHours"></param>
        private static void GetOTSumHoursByNoCheck(decimal? dWorkTimePerDay, T_HR_OVERTIMEREWARD entOvertimereward, List<int> iWeekDays, List<int> iVacDays,
            DateTime dtStartDate, DateTime dtEndDate, string strStartTime, string strEndTime, ref decimal? dOverTimeSumHours)
        {
            try
            {
                decimal? dVacPayRate = 1;
                decimal dHours = 0;

                dtStartDate = DateTime.Parse(dtStartDate.ToString("yyyy-MM-dd") + strStartTime);
                dtEndDate = DateTime.Parse(dtEndDate.ToString("yyyy-MM-dd") + strEndTime);

                TimeSpan tsStart = new TimeSpan(dtStartDate.Ticks);
                TimeSpan tsEnd = new TimeSpan(dtEndDate.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

                if (ts.Days == 0)
                {
                    GetOTSumHoursByLessOT(dWorkTimePerDay, entOvertimereward, iWeekDays, iVacDays, dtStartDate, ts, ref dOverTimeSumHours, ref dVacPayRate, ref dHours);
                }
                else
                {
                    int iTotalDays = ts.Days;
                    if (ts.Hours >= dWorkTimePerDay.Value)
                    {
                        iTotalDays += 1;
                    }
                    else
                    {
                        dHours = ts.Hours;
                    }

                    for (int i = 0; i < iTotalDays; i++)
                    {
                        DateTime dtCheck = dtStartDate.AddDays(i);
                        //检查加班日是否为节假日
                        if (iVacDays.Contains(dtCheck.Day))
                        {
                            //获取节假日加班报酬倍率
                            if (entOvertimereward != null)
                            {
                                dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                            }

                            dOverTimeSumHours += (dHours + dWorkTimePerDay) * dVacPayRate;
                        }
                        else
                        {
                            //检查加班日是否为休息日
                            if (iWeekDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)))
                            {
                                //获取休息日加班报酬倍率
                                if (entOvertimereward != null)
                                {
                                    dVacPayRate = entOvertimereward.WEEKENDPAYRATE.Value == 0 ? 0 : entOvertimereward.WEEKENDPAYRATE;
                                }

                                dOverTimeSumHours += (dHours + dWorkTimePerDay) * dVacPayRate;
                            }
                            else
                            {
                                //获取工作日加班报酬倍率
                                if (entOvertimereward != null)
                                {
                                    dVacPayRate = entOvertimereward.USUALOVERTIMEPAYRATE.Value == 0 ? 0 : entOvertimereward.USUALOVERTIMEPAYRATE;
                                }

                                dOverTimeSumHours += (dHours + dWorkTimePerDay) * dVacPayRate;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算少于一天的加班时长
        /// </summary>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="entOvertimereward"></param>
        /// <param name="iWeekDays"></param>
        /// <param name="iVacDays"></param>
        /// <param name="dtStartDate"></param>
        /// <param name="ts"></param>
        /// <param name="dOverTimeSumHours"></param>
        /// <param name="dVacPayRate"></param>
        /// <param name="dHours"></param>
        private static void GetOTSumHoursByLessOT(decimal? dWorkTimePerDay, T_HR_OVERTIMEREWARD entOvertimereward, List<int> iWeekDays, List<int> iVacDays,
            DateTime dtStartDate, TimeSpan ts, ref decimal? dOverTimeSumHours, ref decimal? dVacPayRate, ref decimal dHours)
        {
            try
            {
                if (ts.Hours >= dWorkTimePerDay.Value)
                {
                    dHours = dWorkTimePerDay.Value;
                }
                else
                {
                    dHours = ts.Hours;
                }

                //检查加班日是否为节假日
                if (iVacDays.Contains(dtStartDate.Day))
                {
                    //获取节假日加班报酬倍率
                    if (entOvertimereward != null)
                    {
                        dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                    }

                    dOverTimeSumHours += dHours * dVacPayRate;
                }
                else
                {
                    //检查加班日是否为休息日
                    if (iWeekDays.Contains(Convert.ToInt32(dtStartDate.DayOfWeek)))
                    {
                        //获取休息日加班报酬倍率
                        if (entOvertimereward != null)
                        {
                            dVacPayRate = entOvertimereward.WEEKENDPAYRATE.Value == 0 ? 0 : entOvertimereward.WEEKENDPAYRATE;
                        }

                        dOverTimeSumHours += dHours * dVacPayRate;
                    }
                    else
                    {
                        //获取工作日加班报酬倍率
                        if (entOvertimereward != null)
                        {
                            dVacPayRate = entOvertimereward.USUALOVERTIMEPAYRATE.Value == 0 ? 0 : entOvertimereward.USUALOVERTIMEPAYRATE;
                        }

                        dOverTimeSumHours += dHours * dVacPayRate;
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 根据起止时间，获取当日的加班打卡记录，并根据报酬倍率，计算返回实际的加班时长
        /// </summary>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="dVacPayRate"></param>
        /// <param name="entTemps"></param>
        /// <param name="dtCheck"></param>
        /// <param name="dtCheckEnd"></param>
        /// <param name="dOverTimeSumHours"></param>
        private static void GetOverTimeSumHours(decimal? dWorkTimePerDay, decimal? dVacPayRate, List<T_HR_EMPLOYEECLOCKINRECORD> entTemps,
            DateTime dtCheck, DateTime dtCheckEnd, ref decimal? dOverTimeSumHours)
        {
            try
            {
                //获取当日的加班打卡记录
                var vcs = from n in entTemps
                          where n.PUNCHDATE >= dtCheck && n.PUNCHDATE <= dtCheckEnd
                          orderby n.PUNCHDATE
                          select n;

                T_HR_EMPLOYEECLOCKINRECORD entVacFirst = vcs.FirstOrDefault();
                T_HR_EMPLOYEECLOCKINRECORD entVacLast = vcs.LastOrDefault();

                TimeSpan tsVacStart = new TimeSpan(dtCheck.Ticks);
                TimeSpan tsVacEnd = new TimeSpan(dtCheckEnd.Ticks);
                TimeSpan tsVac = tsVacEnd.Subtract(tsVacStart).Duration();

                if (tsVac.Hours >= dWorkTimePerDay.Value)
                {
                    dOverTimeSumHours += dWorkTimePerDay * dVacPayRate;
                }
                else
                {
                    dOverTimeSumHours += tsVac.Hours * dVacPayRate;
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 获取当月，所在地区/国家的节假日
        /// </summary>
        /// <param name="strCountryType">所在地区/国家</param>
        /// <param name="dtStart">当月起始日</param>
        /// <param name="dtEnd">当月截止日</param>
        /// <returns>返回节假日序号数组</returns>
        private List<int> GetVacDayList(string strCountryType, DateTime dtStart, DateTime dtEnd)
        {
            List<int> entVacList = new List<int>();

            var qv = from v in dal.GetObjects<T_HR_OUTPLANDAYS>().Include("T_HR_VACATIONSET")
                     where v.T_HR_VACATIONSET.COUNTYTYPE == strCountryType && v.STARTDATE >= dtStart && v.ENDDATE <= dtEnd
                     select v;

            if (qv.Count() == 0)
            {
                return entVacList;
            }

            foreach (T_HR_OUTPLANDAYS item in qv)
            {
                TimeSpan tsStart = new TimeSpan(item.STARTDATE.Value.Ticks);
                TimeSpan tsEnd = new TimeSpan(item.ENDDATE.Value.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

                if (ts.Days == 0)
                {
                    entVacList.Add(item.STARTDATE.Value.Day);
                }
                else
                {
                    for (int i = 0; i < ts.Days; i++)
                    {
                        int j = item.STARTDATE.Value.Day + i;
                        entVacList.Add(j);
                    }
                }
            }

            return entVacList;
        }

        /// <summary>
        /// 获取当月，普通
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        private List<int> GetWeekDayList(int iWorkMode, DateTime dtStart, DateTime dtEnd)
        {
            List<int> entWeekDayList = new List<int>();

            switch (iWorkMode)
            {
                case 1:
                    entWeekDayList.Add(1);
                    break;
                case 2:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    break;
                case 3:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    break;
                case 4:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    entWeekDayList.Add(4);
                    break;
                case 5:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    entWeekDayList.Add(4);
                    entWeekDayList.Add(5);
                    break;
                case 6:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    entWeekDayList.Add(4);
                    entWeekDayList.Add(5);
                    entWeekDayList.Add(6);
                    break;
                case 7:
                    entWeekDayList.Add(0);
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    entWeekDayList.Add(4);
                    entWeekDayList.Add(5);
                    entWeekDayList.Add(6);
                    break;
            }
            return entWeekDayList;
        }
        #endregion
    }
}
