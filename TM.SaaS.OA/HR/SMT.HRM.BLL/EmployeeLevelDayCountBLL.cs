
/*
 * 文件名：EmployeeLevelDayCountBLL.cs
 * 作  用：员工可休假 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-3-5 15:09:52
 * 修改人：
 * 修改时间：
 */




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using TM_SaaS_OA_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;
using System.Configuration;
using SMT.HRM.CustomModel.Common;
using SMT.HRM.CustomModel.Request;

using SMT.SaaS.Services;
using SMT.SaaS.Services.Model;
using SMT.HRM.BLL.Permission;

namespace SMT.HRM.BLL
{
    public class EmployeeLevelDayCountBLL : BaseBll<T_HR_EMPLOYEELEVELDAYCOUNT>
    {
        public EmployeeLevelDayCountBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 根据员工ID,休假类型获取此员工当前时间，指定休假类型的可休假记录
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="strVacType"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> GetLevelDayCountByEmployeeIDAndVacType(string employeeID, string strVacType)
        {
            if (string.IsNullOrEmpty(employeeID) || string.IsNullOrEmpty(strVacType))
            {
                return null;
            }

            var ents = from a in dal.GetObjects()
                       where a.EMPLOYEEID == employeeID && a.VACATIONTYPE == strVacType
                       && a.STATUS == 1//2014-07-15,周文斌添加，过滤掉失效的记录
                       select a;

            if (ents.Count() == 0)
            {
                return null;
            }

            return ents;
        }

        /// <summary>
        /// 获取员工当前请假时段内可用冲减天数
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strLeaveRecordId">当前请假记录ID</param>
        /// <param name="strLeaveSetId">当前请假对应的请假标准ID</param>
        /// <param name="strLeaveFineType">请假标准的扣款方式</param>
        /// <param name="dtStartDate">请假起始时间</param>
        /// <param name="dtEndDate">请假结束时间</param>
        /// <returns>当前请假时段内可用冲减天数</returns>
        public decimal GetCurLevelDaysByEmployeeIDAndLeaveFineType(string strEmployeeID, string strLeaveRecordId, string strLeaveSetId, DateTime dtStartDate, DateTime dtEndDate)
        {
            EmployeeLeaveRecordBLL bllLeaveRecord = new EmployeeLeaveRecordBLL();
            #region 重新生成员工带薪假期
            string orgType = (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString();
            CalculateEmployeeLevelDayCountByOrgID(orgType, strEmployeeID);
            #endregion
            //确认实际可用带薪假天数，必须保证员工ID，请假记录ID，假期标准ID都不能为空
            if (string.IsNullOrWhiteSpace(strEmployeeID) || string.IsNullOrWhiteSpace(strLeaveRecordId) || string.IsNullOrWhiteSpace(strLeaveSetId))
            {
                return 0;
            }

            //由于实际可用带薪假天数实时计算，需要请假时间作为判断范围，因此必须非空
            if (dtStartDate == null || dtEndDate == null)
            {
                return 0;
            }

            EmployeeBLL bllEmployee = new EmployeeBLL();
            V_EMPLOYEEVIEW entEmployeeView = bllEmployee.GetEmployeeInfoByEmployeeID(strEmployeeID);

            if (entEmployeeView == null)
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(entEmployeeView.EMPLOYEEID) || string.IsNullOrWhiteSpace(entEmployeeView.OWNERCOMPANYID))
            {
                return 0;
            }

            LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL();
            T_HR_LEAVETYPESET entLeaveTypeSet = bllLeaveTypeSet.GetLeaveTypeSetByID(strLeaveSetId);

            if (entLeaveTypeSet == null)
            {
                return 0;
            }

            string strLeaveFineType = string.Empty, strFilterString = string.Empty;
            List<object> objArgs = new List<object>();

            strLeaveFineType = entLeaveTypeSet.FINETYPE;

            if (strLeaveFineType == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
            {
                strFilterString = " VACATIONTYPE==@0";
                objArgs.Add(entLeaveTypeSet.LEAVETYPEVALUE);
            }
            else if (strLeaveFineType == (Convert.ToInt32(Common.LeaveFineType.AdjLevDeduct) + 1).ToString())
            {
                strFilterString = " VACATIONTYPE == @0";
                objArgs.Add((Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString());
            }
            else if (strLeaveFineType == (Convert.ToInt32(Common.LeaveFineType.AdjLevPaidDayDeduct) + 1).ToString())
            {
                strFilterString = " (VACATIONTYPE == @0 OR VACATIONTYPE == @1)";
                objArgs.Add((Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString());
                objArgs.Add((Convert.ToInt32(Common.LeaveTypeValue.AnnualLeave) + 1).ToString());
            }

            if (string.IsNullOrWhiteSpace(strFilterString) || objArgs.Count() == 0)
            {
                return 0;
            }

            DateTime dtYearStart = DateTime.Parse(dtStartDate.Year.ToString() + "-1-1");
            DateTime dtYearEnd = dtYearStart.AddYears(1).AddSeconds(-1);

            var ems = from a in dal.GetObjects()
                      where a.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                      && a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID
                      select a;

            if (entLeaveTypeSet.LEAVETYPEVALUE != (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
            {
                //考虑
                ems = from e in ems
                      where e.EFFICDATE >= dtYearStart && e.TERMINATEDATE <= dtYearEnd
                          || (e.EFFICDATE <= dtYearStart && e.TERMINATEDATE >= dtYearEnd)
                      select e;
            }

            ems = ems.Where(strFilterString, objArgs.ToArray());

            if (ems == null)
            {
                return 0;
            }

            if (ems.Count() == 0)
            {
                return 0;
            }
            else
            {
                //如果是五四或三八节，大于有效期了则归0
                if (entLeaveTypeSet.LEAVETYPEVALUE == "12" || entLeaveTypeSet.LEAVETYPEVALUE == "13")
                {
                    T_HR_EMPLOYEELEVELDAYCOUNT dayCount = ems.FirstOrDefault();
                    if (dtStartDate > dayCount.TERMINATEDATE || dtEndDate > dayCount.TERMINATEDATE)
                    {
                        return 0;
                    }
                }
            }

            var dets = from d in dal.GetObjects<T_HR_EMPLOYEELEVELDAYDETAILS>().Include("T_HR_EMPLOYEELEVELDAYCOUNT")
                       join m in ems on d.T_HR_EMPLOYEELEVELDAYCOUNT.RECORDID equals m.RECORDID
                       where d.EFFICDATE >= dtStartDate && d.EFFICDATE <= m.TERMINATEDATE
                       orderby d.EFFICDATE descending
                       select d;

            decimal dLeaveYearDays = 0, dLeaveMonthDays = 0, dAdjLevPaidDays = 0;       //同类型假本年已用天数，同类型假本月已用天数，冲减带薪假已用天数

            bllLeaveRecord.GetLeaveDaysHistory(entEmployeeView, strLeaveRecordId, strLeaveSetId, dtStartDate, dtEndDate,
                ref dLeaveYearDays, ref dLeaveMonthDays, ref dAdjLevPaidDays);

            decimal dCurLevelDay = 0;

            if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.SickLeave) + 1).ToString())
            {
                int iStart = dtStartDate.Month;
                int iEnd = dtEndDate.Month;
                decimal dStartYear = dtStartDate.Year;
                decimal dEndYear = dtStartDate.Year;
                if (dStartYear == dEndYear)
                {
                    dCurLevelDay = iEnd - iStart + 1;
                    if (dLeaveYearDays >= 5)
                    {
                        dCurLevelDay = 0;
                    }
                    else
                    {
                        if (dLeaveMonthDays <= 1)
                        {
                            dCurLevelDay -= 0.5M;
                        }
                        else
                        {
                            dCurLevelDay -= 1;
                        }
                    }

                }
                else if (dStartYear < dEndYear)
                {
                    if (iEnd > 5)
                    {
                        iEnd = 5;
                    }

                    if (dLeaveYearDays >= 5)
                    {
                        dCurLevelDay = iEnd;
                    }
                    else
                    {
                        if (dLeaveMonthDays <= 1)
                        {
                            dCurLevelDay = (12 - iStart + 0.5M) + iEnd;
                        }
                        else
                        {
                            dCurLevelDay = (12 - iStart) + iEnd;
                        }
                    }
                }
            }

            foreach (T_HR_EMPLOYEELEVELDAYCOUNT entMaster in ems)
            {
                dCurLevelDay += entMaster.DAYS.Value;
            }

            //统计本年总请假天数，以便计算出带薪假剩余可用的天数  这个方法有问题 需要修改            
            ReCalculateLeavePaidDay(strLeaveSetId, strLeaveRecordId, entEmployeeView, dtStartDate, dtEndDate, strFilterString, objArgs, ref dAdjLevPaidDays);

            dCurLevelDay -= dAdjLevPaidDays;

            if (dets.Count() == 0)
            {
                return dCurLevelDay;
            }

            foreach (T_HR_EMPLOYEELEVELDAYDETAILS entDetail in dets)
            {
                dCurLevelDay -= entDetail.DAYS.Value;
            }

            if (dCurLevelDay < 0)
            {
                dCurLevelDay = 0;
            }

            return dCurLevelDay;
        }


        public decimal GetCurLevelDaysByEmployeeIDAndLeaveFineTypeForMVC(string strEmployeeID, string strLeaveRecordId, string strLeaveSetId, DateTime dtStartDate, DateTime dtEndDate)
        {
            EmployeeLeaveRecordBLL bllLeaveRecord = new EmployeeLeaveRecordBLL();
            #region 重新生成员工带薪假期
            string orgType = (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString();
            CalculateEmployeeLevelDayCountByOrgID(orgType, strEmployeeID);
            #endregion
            ////确认实际可用带薪假天数，必须保证员工ID，请假记录ID，假期标准ID都不能为空
            //if (string.IsNullOrWhiteSpace(strEmployeeID) || string.IsNullOrWhiteSpace(strLeaveRecordId) || string.IsNullOrWhiteSpace(strLeaveSetId))
            //{
            //    return 0;
            //}

            ////由于实际可用带薪假天数实时计算，需要请假时间作为判断范围，因此必须非空
            //if (dtStartDate == null || dtEndDate == null)
            //{
            //    return 0;
            //}

            //EmployeeBLL bllEmployee = new EmployeeBLL();
            //V_EMPLOYEEVIEW entEmployeeView = bllEmployee.GetEmployeeInfoByEmployeeID(strEmployeeID);

            //if (entEmployeeView == null)
            //{
            //    return 0;
            //}

            //if (string.IsNullOrWhiteSpace(entEmployeeView.EMPLOYEEID) || string.IsNullOrWhiteSpace(entEmployeeView.OWNERCOMPANYID))
            //{
            //    return 0;
            //}

            //LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL();
            //T_HR_LEAVETYPESET entLeaveTypeSet = bllLeaveTypeSet.GetLeaveTypeSetByID(strLeaveSetId);

            //if (entLeaveTypeSet == null)
            //{
            //    return 0;
            //}

            //string strLeaveFineType = string.Empty, strFilterString = string.Empty;
            //List<object> objArgs = new List<object>();

            //strLeaveFineType = entLeaveTypeSet.FINETYPE;

            //if (strLeaveFineType == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
            //{
            //    strFilterString = " VACATIONTYPE==@0";
            //    objArgs.Add(entLeaveTypeSet.LEAVETYPEVALUE);
            //}
            //else if (strLeaveFineType == (Convert.ToInt32(Common.LeaveFineType.AdjLevDeduct) + 1).ToString())
            //{
            //    strFilterString = " VACATIONTYPE == @0";
            //    objArgs.Add((Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString());
            //}
            //else if (strLeaveFineType == (Convert.ToInt32(Common.LeaveFineType.AdjLevPaidDayDeduct) + 1).ToString())
            //{
            //    strFilterString = " (VACATIONTYPE == @0 OR VACATIONTYPE == @1)";
            //    objArgs.Add((Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString());
            //    objArgs.Add((Convert.ToInt32(Common.LeaveTypeValue.AnnualLeave) + 1).ToString());
            //}

            //if (string.IsNullOrWhiteSpace(strFilterString) || objArgs.Count() == 0)
            //{
            //    return 0;
            //}

            //DateTime dtYearStart = DateTime.Parse(dtStartDate.Year.ToString() + "-1-1");
            //DateTime dtYearEnd = dtYearStart.AddYears(1).AddSeconds(-1);

            //var ems = from a in dal.GetObjects()
            //          where a.EMPLOYEEID == entEmployeeView.EMPLOYEEID
            //          && a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID
            //          select a;

            //if (entLeaveTypeSet.LEAVETYPEVALUE != (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
            //{
            //    //考虑
            //    ems = from e in ems
            //          where e.EFFICDATE >= dtYearStart && e.TERMINATEDATE <= dtYearEnd
            //              || (e.EFFICDATE <= dtYearStart && e.TERMINATEDATE >= dtYearEnd)
            //          select e;
            //}

            //ems = ems.Where(strFilterString, objArgs.ToArray());

            //if (ems == null)
            //{
            //    return 0;
            //}

            //if (ems.Count() == 0)
            //{
            //    return 0;
            //}
            //else
            //{
            //    //如果是五四或三八节，大于有效期了则归0
            //    if (entLeaveTypeSet.LEAVETYPEVALUE == "12" || entLeaveTypeSet.LEAVETYPEVALUE == "13")
            //    {
            //        T_HR_EMPLOYEELEVELDAYCOUNT dayCount = ems.FirstOrDefault();
            //        if (dtStartDate > dayCount.TERMINATEDATE || dtEndDate > dayCount.TERMINATEDATE)
            //        {
            //            return 0;
            //        }
            //    }
            //}

            //var dets = from d in dal.GetObjects<T_HR_EMPLOYEELEVELDAYDETAILS>().Include("T_HR_EMPLOYEELEVELDAYCOUNT")
            //           join m in ems on d.T_HR_EMPLOYEELEVELDAYCOUNT.RECORDID equals m.RECORDID
            //           where d.EFFICDATE >= dtStartDate && d.EFFICDATE <= m.TERMINATEDATE
            //           orderby d.EFFICDATE descending
            //           select d;

            //decimal dLeaveYearDays = 0, dLeaveMonthDays = 0, dAdjLevPaidDays = 0;       //同类型假本年已用天数，同类型假本月已用天数，冲减带薪假已用天数

            //bllLeaveRecord.GetLeaveDaysHistory(entEmployeeView, strLeaveRecordId, strLeaveSetId, dtStartDate, dtEndDate,
            //    ref dLeaveYearDays, ref dLeaveMonthDays, ref dAdjLevPaidDays);

            //decimal dCurLevelDay = 0;

            //if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.SickLeave) + 1).ToString())
            //{
            //    int iStart = dtStartDate.Month;
            //    int iEnd = dtEndDate.Month;
            //    decimal dStartYear = dtStartDate.Year;
            //    decimal dEndYear = dtStartDate.Year;
            //    if (dStartYear == dEndYear)
            //    {
            //        dCurLevelDay = iEnd - iStart + 1;
            //        if (dLeaveYearDays >= 5)
            //        {
            //            dCurLevelDay = 0;
            //        }
            //        else
            //        {
            //            if (dLeaveMonthDays <= 1)
            //            {
            //                dCurLevelDay -= 0.5M;
            //            }
            //            else
            //            {
            //                dCurLevelDay -= 1;
            //            }
            //        }

            //    }
            //    else if (dStartYear < dEndYear)
            //    {
            //        if (iEnd > 5)
            //        {
            //            iEnd = 5;
            //        }

            //        if (dLeaveYearDays >= 5)
            //        {
            //            dCurLevelDay = iEnd;
            //        }
            //        else
            //        {
            //            if (dLeaveMonthDays <= 1)
            //            {
            //                dCurLevelDay = (12 - iStart + 0.5M) + iEnd;
            //            }
            //            else
            //            {
            //                dCurLevelDay = (12 - iStart) + iEnd;
            //            }
            //        }
            //    }
            //}

            //foreach (T_HR_EMPLOYEELEVELDAYCOUNT entMaster in ems)
            //{
            //    dCurLevelDay += entMaster.DAYS.Value;
            //}

            ////统计本年总请假天数，以便计算出带薪假剩余可用的天数  这个方法有问题 需要修改            
            //ReCalculateLeavePaidDay(strLeaveSetId, strLeaveRecordId, entEmployeeView, dtStartDate, dtEndDate, strFilterString, objArgs, ref dAdjLevPaidDays);

            //dCurLevelDay -= dAdjLevPaidDays;

            //if (dets.Count() == 0)
            //{
            //    return dCurLevelDay;
            //}

            //foreach (T_HR_EMPLOYEELEVELDAYDETAILS entDetail in dets)
            //{
            //    dCurLevelDay -= entDetail.DAYS.Value;
            //}

            //if (dCurLevelDay < 0)
            //{
            //    dCurLevelDay = 0;
            //}
            decimal dCurLevelDay = 0;
            return dCurLevelDay;
        }

        /// <summary>
        /// 重新计算当年带薪假下被使用的请假天数,以便计算出带薪假剩余可用的天数
        /// </summary>
        /// <param name="strLeaveSetId">请假类型的ID</param>
        /// <param name="strLeaveRecordId">请假记录Id</param>
        /// <param name="entEmployeeView">请假申请人的员工信息(主岗位)</param>
        /// <param name="dtLeaveStartTime">请假起始时间</param>
        /// <param name="dtLeaveEndTime">请假结束时间</param>
        /// <param name="strFilterString">带薪假查询条件</param>
        /// <param name="objArgs">带薪假查询参数</param>
        /// <param name="dAdjLevPaidDays">当前已请假天数</param>
        private void ReCalculateLeavePaidDay(string strLeaveSetId, string strLeaveRecordId, V_EMPLOYEEVIEW entEmployeeView,
            DateTime dtLeaveStartTime, DateTime dtLeaveEndTime, string strFilterString, List<object> objArgs, ref decimal dAdjLevPaidDays)
        {
            if (entEmployeeView == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(strLeaveSetId) || string.IsNullOrWhiteSpace(strLeaveRecordId) || string.IsNullOrWhiteSpace(entEmployeeView.EMPLOYEEID) || string.IsNullOrWhiteSpace(entEmployeeView.OWNERCOMPANYID))
            {
                return;
            }

            LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL();
            T_HR_LEAVETYPESET entLeaveTypeSet = bllLeaveTypeSet.GetLeaveTypeSetByID(strLeaveSetId);

            if (entLeaveTypeSet == null)
            {
                return;
            }

            AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(entEmployeeView.EMPLOYEEID, dtLeaveStartTime);
            if (entAttendSolAsign == null)
            {
                //当前员工没有分配考勤方案，无法提交请假申请
                return;
            }

            //获取考勤方案
            T_HR_ATTENDANCESOLUTION entAttendSol = entAttendSolAsign.T_HR_ATTENDANCESOLUTION;

            if (entAttendSol == null)
            {
                //当前员工没有分配考勤方案，无法提交请假申请
                return;
            }

            if (entAttendSol.WORKTIMEPERDAY == null)
            {
                return;
            }

            DateTime dtYearStart = new DateTime();
            DateTime dtYearEnd = new DateTime();
            DateTime dtMonthStart = new DateTime();
            string strCheckStateArd = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            string strCheckStateAri = Convert.ToInt32(Common.CheckStates.Approving).ToString();
            DateTime.TryParse(dtLeaveStartTime.Year.ToString() + "-1-1", out dtYearStart);
            DateTime.TryParse(dtLeaveStartTime.Year.ToString() + "-12-31", out dtYearEnd);
            DateTime.TryParse(dtLeaveStartTime.ToString("yyyy-MM") + "-1", out dtMonthStart);

            //员工请假记录所有的请假记录
            var ey = from e in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET")
                     where e.LEAVERECORDID != strLeaveRecordId
                     && e.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveSetId
                     && e.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID
                     && e.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                     && (e.CHECKSTATE == strCheckStateAri || e.CHECKSTATE == strCheckStateArd)
                         //&& e.ENDDATETIME < dtLeaveStartTime
                     && e.ENDDATETIME < dtYearEnd
                     select e;

            //员工可休假记录(只针对非调休假计算使用)
            var el = from a in dal.GetObjects()
                     where a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID
                     && a.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                     && a.LEAVETYPESETID == strLeaveSetId && a.TERMINATEDATE >= dtYearStart
                     select a;

            //已失效的可休假记录，只针对调休假计算使用
            var oel = from a in dal.GetObjects()
                      where a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID
                      && a.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                      && a.LEAVETYPESETID == strLeaveSetId && a.TERMINATEDATE < dtLeaveStartTime
                      select a;

            //有效的可休假记录，只针对调休假计算使用
            var cel = from a in dal.GetObjects()
                      where a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID
                      && a.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                          //&& a.LEAVETYPESETID == strLeaveSetId && a.EFFICDATE <= dtLeaveEndTime 
                          //&& a.TERMINATEDATE >= dtLeaveEndTime
                      && a.LEAVETYPESETID == strLeaveSetId && a.EFFICDATE <= dtLeaveStartTime
                      && a.TERMINATEDATE >= dtLeaveStartTime
                      select a;

            if (!string.IsNullOrWhiteSpace(strFilterString) && objArgs.Count() != 0)
            {
                el = el.Where(strFilterString, objArgs.ToArray());
                oel = oel.Where(strFilterString, objArgs.ToArray());
                cel = cel.Where(strFilterString, objArgs.ToArray());
            }

            //员工请假记录考虑到是否有销假
            var eu = from u in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>()
                     join o in ey on u.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID equals o.LEAVERECORDID
                     where u.CHECKSTATE == strCheckStateArd && u.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && u.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                     select u;


            if (entLeaveTypeSet.LEAVETYPEVALUE != (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
            {
                ey = from e in ey
                     where e.STARTDATETIME >= dtYearStart
                     select e;

                eu = from e in eu
                     where e.STARTDATETIME >= dtYearStart
                     select e;
            }


            //如果有销假，那么计算销假的时长
            decimal? dLeaveDays = null;
            if (eu.Count() > 0)
            {
                //员工销假时长
                decimal? dCancelLeaveDays = eu.Sum(t => t.TOTALHOURS);

                //员工实际请假时长=员工请假时长-员工销假时长
                dLeaveDays = ey.Sum(t => t.TOTALHOURS) - dCancelLeaveDays;
            }
            else
            {
                dLeaveDays = ey.Sum(t => t.TOTALHOURS);
            }

            //员工可休假时长,本年度可休假天数
            decimal? dFreeDays = el.Sum(t => t.DAYS);
            if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
            {
                decimal? dInvalidAdDays = oel.Sum(t => t.DAYS);
                decimal? dFreeAdDays = cel.Sum(t => t.DAYS);

                dFreeDays = 0;
                if (dInvalidAdDays != null)
                {
                    dFreeDays = dInvalidAdDays.Value;
                }

                if (dFreeAdDays != null)
                {
                    dFreeDays += dFreeAdDays.Value;
                }
            }

            if (dLeaveDays == null)
            {
                return;
            }

            if (dFreeDays == null)
            {
                dAdjLevPaidDays = decimal.Round(dLeaveDays.Value / entAttendSol.WORKTIMEPERDAY.Value, 2);
                return;
            }

            decimal? dInvalidAdjustDays = 0;
            if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
            {
                CalculateInvalidAdjustDay(strLeaveSetId, strLeaveRecordId, entEmployeeView, dtLeaveStartTime, dtLeaveEndTime, strFilterString, objArgs, ref dInvalidAdjustDays);
                if (dInvalidAdjustDays == 0)
                {
                    var entlasts = (from ent11 in ey
                                    where ent11.STARTDATETIME < dtLeaveStartTime
                                    select ent11).OrderByDescending(s => s.STARTDATETIME);
                    if (entlasts.Count() > 0)
                    {
                        T_HR_EMPLOYEELEAVERECORD newRecord = entlasts.ToList().FirstOrDefault();
                        if (newRecord.STARTDATETIME != null && newRecord.ENDDATETIME != null)
                        {
                            decimal? dInvalidAdjustDays2 = 0;
                            CalculateInvalidAdjustDay(strLeaveSetId, strLeaveRecordId, entEmployeeView, (DateTime)newRecord.STARTDATETIME, (DateTime)newRecord.ENDDATETIME, strFilterString, objArgs, ref dInvalidAdjustDays2);
                            dInvalidAdjustDays = dInvalidAdjustDays + dInvalidAdjustDays2;
                        }
                    }
                }
            }

            dLeaveDays += dInvalidAdjustDays.Value;
            if (dFreeDays > 0)
            {
                if (dLeaveDays > dFreeDays * entAttendSol.WORKTIMEPERDAY.Value)
                {
                    dLeaveDays = dFreeDays * entAttendSol.WORKTIMEPERDAY.Value;
                }
            }

            dAdjLevPaidDays = decimal.Round(dLeaveDays.Value / entAttendSol.WORKTIMEPERDAY.Value, 2);
        }

        /// <summary>
        /// 统计当前请假时间段之前已失效的调休假天数
        /// </summary>
        /// <param name="strLeaveSetId">请假类型的ID</param>
        /// <param name="strLeaveRecordId">请假记录Id</param>
        /// <param name="strEmployeeID">请假申请人的员工ID</param>
        /// <param name="dtLeaveStartTime">请假起始时间</param>
        /// <param name="dtLeaveEndTime">请假结束时间</param>
        /// <param name="strFilterString">带薪假查询条件</param>
        /// <param name="objArgs">带薪假查询参数</param>
        /// <param name="dInvalidAdjustDays">当前已失效的调休假天数</param>
        private void CalculateInvalidAdjustDay(string strLeaveSetId, string strLeaveRecordId, V_EMPLOYEEVIEW entEmployeeView, DateTime dtLeaveStartTime, DateTime dtLeaveEndTime, string strFilterString, List<object> objArgs, ref decimal? dInvalidAdjustDays)
        {
            if (entEmployeeView == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(strLeaveSetId) || string.IsNullOrWhiteSpace(strLeaveRecordId) || string.IsNullOrWhiteSpace(entEmployeeView.EMPLOYEEID) || string.IsNullOrWhiteSpace(entEmployeeView.OWNERCOMPANYID))
            {
                return;
            }


            LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL();
            T_HR_LEAVETYPESET entLeaveTypeSet = bllLeaveTypeSet.GetLeaveTypeSetByID(strLeaveSetId);

            if (entLeaveTypeSet == null)
            {
                return;
            }

            AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(entEmployeeView.EMPLOYEEID, dtLeaveStartTime);
            if (entAttendSolAsign == null)
            {
                //当前员工没有分配考勤方案，无法提交请假申请
                return;
            }

            //获取考勤方案
            T_HR_ATTENDANCESOLUTION entAttendSol = entAttendSolAsign.T_HR_ATTENDANCESOLUTION;

            if (entAttendSol == null)
            {
                //当前员工没有分配考勤方案，无法提交请假申请
                return;
            }

            if (entAttendSol.WORKTIMEPERDAY == null)
            {
                return;
            }

            DateTime dtYearStart = new DateTime();
            DateTime dtYearEnd = new DateTime();
            DateTime dtMonthStart = new DateTime();
            string strCheckStateArd = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            string strCheckStateAri = Convert.ToInt32(Common.CheckStates.Approving).ToString();
            DateTime.TryParse(dtLeaveStartTime.Year.ToString() + "-1-1", out dtYearStart);
            DateTime.TryParse(dtLeaveStartTime.Year.ToString() + "-12-31", out dtYearEnd);
            DateTime.TryParse(dtLeaveStartTime.ToString("yyyy-MM") + "-1", out dtMonthStart);

            //员工请假记录
            var ey = from e in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET")
                     where e.LEAVERECORDID != strLeaveRecordId && e.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && e.EMPLOYEEID == entEmployeeView.EMPLOYEEID && e.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveSetId
                     && (e.CHECKSTATE == strCheckStateAri || e.CHECKSTATE == strCheckStateArd)
                     select e;
            //已失效的可休假记录，只针对调休假计算使用
            var oel = from a in dal.GetObjects()
                      where a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && a.EMPLOYEEID == entEmployeeView.EMPLOYEEID && a.LEAVETYPESETID == strLeaveSetId && a.TERMINATEDATE < dtLeaveStartTime
                      select a;

            if (!string.IsNullOrWhiteSpace(strFilterString) && objArgs.Count() != 0)
            {
                oel = oel.Where(strFilterString, objArgs.ToArray());
            }

            //员工请假记录考虑到是否有销假
            var eu = from u in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>()
                     join o in ey on u.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID equals o.LEAVERECORDID
                     where u.CHECKSTATE == strCheckStateArd && u.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && u.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                     select u;

            T_HR_EMPLOYEELEVELDAYCOUNT entInvalidFree = oel.OrderBy(c => c.EFFICDATE).ToList().LastOrDefault();

            DateTime? dtInvaalid = null;
            if (entInvalidFree == null)
            {
                dInvalidAdjustDays = 0;
                return;
            }

            //dtInvaalid = entInvalidFree.TERMINATEDATE;
            dtInvaalid = dtLeaveStartTime;
            var oey = from o in ey
                      where o.ENDDATETIME <= dtInvaalid
                      select o;

            var oeu = from o in eu
                      where o.ENDDATETIME <= dtInvaalid
                      select o;


            //如果有销假，那么计算销假的时长
            decimal? dInvaalidLeaveDays = null, dInvaalidCancelDays = null;

            dInvaalidLeaveDays = oey.Sum(t => t.TOTALHOURS);

            //员工销假时长
            dInvaalidCancelDays = oeu.Sum(t => t.TOTALHOURS);

            dInvaalidLeaveDays = dInvaalidLeaveDays == null ? 0 : dInvaalidLeaveDays;
            dInvaalidCancelDays = dInvaalidCancelDays == null ? 0 : dInvaalidCancelDays;

            //员工实际请假时长=员工请假时长-员工销假时长
            if (dInvaalidLeaveDays != null && dInvaalidCancelDays != null)
            {
                dInvaalidLeaveDays = dInvaalidLeaveDays - dInvaalidCancelDays;
            }

            //员工可休假时长
            if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
            {
                dInvalidAdjustDays = oel.Sum(t => t.DAYS);
            }

            if (dInvalidAdjustDays == null)
            {
                dInvalidAdjustDays = 0;
                return;
            }

            if (dInvaalidLeaveDays > dInvalidAdjustDays * entAttendSol.WORKTIMEPERDAY.Value)
            {
                dInvalidAdjustDays = 0;
                return;
            }

            dInvalidAdjustDays = dInvalidAdjustDays * entAttendSol.WORKTIMEPERDAY.Value - dInvaalidLeaveDays.Value;
        }

        /// <summary>
        /// 根据员工ID获取此员工当前指定休假类型可休假天数
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <param name="strVacType">休假类型</param>
        /// <param name="currDate">查询的日期</param>
        /// <returns></returns>
        public T_HR_EMPLOYEELEVELDAYCOUNT GetCurLevelDayCountByEmployeeID(string employeeID, string strVacType, DateTime currDate)
        {
            IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> ents = GetLevelDayCountByEmployeeIDAndVacType(employeeID, strVacType);

            if (ents == null)
            {
                return null;
            }

            var dets = from d in dal.GetObjects<T_HR_EMPLOYEELEVELDAYDETAILS>().Include("T_HR_EMPLOYEELEVELDAYCOUNT")
                       join m in ents on d.T_HR_EMPLOYEELEVELDAYCOUNT.RECORDID equals m.RECORDID
                       where d.EFFICDATE >= currDate && d.EFFICDATE <= m.TERMINATEDATE
                       orderby d.EFFICDATE descending
                       select d;

            T_HR_EMPLOYEELEVELDAYCOUNT ent = new T_HR_EMPLOYEELEVELDAYCOUNT();
            currDate = DateTime.Parse(currDate.ToString("yyyy-MM-dd"));
            if (dets.Count() == 0)
            {
                var curs = ents.Where(c => c.TERMINATEDATE >= currDate);
                if (curs.Count() == 0)
                {
                    return null;
                }

                ent = curs.FirstOrDefault();
                if (curs.Count() > 1)
                {
                    foreach (T_HR_EMPLOYEELEVELDAYCOUNT cur in curs)
                    {
                        if (ent.RECORDID != cur.RECORDID)
                        {
                            ent.DAYS += cur.DAYS;
                        }
                    }
                }

                return ent;
            }

            decimal dCurLevelDay = 0;
            dCurLevelDay = ent.DAYS.Value;

            foreach (T_HR_EMPLOYEELEVELDAYDETAILS item in dets)
            {
                dCurLevelDay -= item.DAYS.Value;
            }

            ent.DAYS = dCurLevelDay;

            return ent;
        }

        /// <summary>
        /// 根据员工ID获取此员工当前可休假信息
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="currDate">查询的日期</param>
        /// <param name="strDate">考核日(格式mm-dd)</param>
        /// <returns></returns>
        public decimal GetLevelDayCountByEmployeeID(string employeeID, DateTime currDate, string strDate)
        {
            if (string.IsNullOrEmpty(strDate))
            {
                return 0;
            }

            DateTime startDate = Convert.ToDateTime(currDate.Year.ToString() + "-" + strDate);
            DateTime endDate = currDate;
            if (startDate >= currDate)
            {
                startDate = startDate.AddYears(-1);
                endDate = currDate;
            }

            var ent = (from a in dal.GetObjects()
                       where a.EMPLOYEEID == employeeID && a.TERMINATEDATE > currDate
                       && (a.EFFICDATE >= startDate && a.EFFICDATE <= endDate)
                       select a.DAYS).Sum().Value;
            return ent;

        }

        #region "   周文斌添加，获取员工可休假天数 "

        /// <summary>
        /// 获取该员工的所有假期，用于前台计算
        /// 因为这里的假期包括多条记录，在前台根据类型进行计算
        /// 获取状态为1的有效记录
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEELEVELDAYCOUNT> GetAllLeaveDayCountByEmployeeID(string employeeID)
        {
            return dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == employeeID
                && t.STATUS == (int)Enums.Status.Valid).ToList();
        }

        #endregion

        /// <summary>
        /// 根据员工ID获取此员工往年可休假信息总数
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="currDate">查询的日期</param>
        /// <param name="strDate">考核日(格式mm-dd)</param>
        /// <returns></returns>
        public decimal GetLevelDayCountHistoryByEmployeeID(string employeeID, DateTime currDate, string strDate)
        {
            if (strDate != "")
            {
                DateTime dDate = Convert.ToDateTime(currDate.Year.ToString() + "-" + strDate);
                if (dDate >= currDate)
                {
                    dDate = dDate.AddYears(-1);
                }
                var ent = (from a in dal.GetObjects()
                           where a.EMPLOYEEID == employeeID && a.TERMINATEDATE > currDate && a.EFFICDATE <= dDate
                           select a.DAYS).Sum().Value;
                return ent;
            }
            return 0;
        }

        public List<V_AttendSummaryRecord> GetAttendSummaryRecordByObject(string objectID)
        {
            List<V_AttendSummaryRecord> entityList = new List<V_AttendSummaryRecord>();
            V_AttendSummaryRecord entity = new V_AttendSummaryRecord();
            return entityList;
        }

        public V_AttendSummaryRecord GetAttendSummaryRecordByEmployeeID(string employeeID)
        {
            V_AttendSummaryRecord entity = new V_AttendSummaryRecord();
            FreeLeaveDaySetBLL leaveDayBll = new FreeLeaveDaySetBLL();

            List<V_EMPLOYEELEAVE> leaveList = new List<V_EMPLOYEELEAVE>();
            //获取本年度的假期数
            leaveList = leaveDayBll.GetEmployeeLeaveByEmployeeID(employeeID, DateTime.Now.Date);
            return entity;
        }

        /// <summary>
        /// 根据员工ID，结算日期获取年假及调休假信息
        /// </summary>
        /// <param name="strEmployeeID">员工Id</param>
        /// <param name="dtBalance">结算日期</param>
        /// <returns></returns>
        public V_AttendSummaryRecord GetAttendSummaryRecordByEmployeeIDAndDate(string strEmployeeID, DateTime dtBalance)
        {
            V_AttendSummaryRecord entity = new V_AttendSummaryRecord();
            try
            {
                //获取员工组织架构
                EmployeeBLL employeeBll = new EmployeeBLL();
                V_EMPLOYEEPOST employeePost = employeeBll.GetEmployeeDetailByID(strEmployeeID);
                //获取员工的入职信息
                EmployeeEntryBLL entryBll = new EmployeeEntryBLL();
                T_HR_EMPLOYEEENTRY entry = entryBll.GetEmployeeEntryByEmployeeID(strEmployeeID);
                DateTime entryDate = entry.ENTRYDATE.Value;
                int m = Utility.DateDiff(entryDate, dtBalance, "M");
                string departmentID = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                string comparyID = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;

                entity.EmployeeID = employeePost.T_HR_EMPLOYEE.EMPLOYEEID;
                entity.EmployeeCode = employeePost.T_HR_EMPLOYEE.EMPLOYEECODE;
                entity.EmployeeName = employeePost.T_HR_EMPLOYEE.EMPLOYEECNAME;

                decimal dLasrYear = dtBalance.Year - 1;
                DateTime dtLastYearStart = DateTime.Parse(dLasrYear.ToString() + "-1-1");
                DateTime dtLastYearEnd = DateTime.Parse(dLasrYear.ToString() + "-12-31");

                DateTime dtCurrentYearStart = DateTime.Parse(dtBalance.Year.ToString() + "-1-1");
                DateTime dtCurrentYearEnd = dtBalance;

                AttendanceSolutionAsignBLL bllAttSolAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entLastAttSolAsign = bllAttSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeID, dtLastYearEnd);
                T_HR_ATTENDANCESOLUTIONASIGN entCurAttSolAsign = bllAttSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeID, dtBalance);

                if (entCurAttSolAsign == null)
                {
                    return entity;
                }

                //查询上一年度未用的年假和调休假
                var entlasts = from o in dal.GetObjects<T_HR_ATTENDYEARLYBALANCE>()
                               where o.EMPLOYEEID == strEmployeeID && o.BALANCEYEAR == dLasrYear
                               select o;

                decimal dLastUseLevelDayCount = 0, dLastUseYearDays = 0;
                if (entlasts != null)
                {
                    if (entlasts.Count() == 1)
                    {
                        T_HR_ATTENDYEARLYBALANCE entLast = entlasts.FirstOrDefault();

                        if (entLast.LEAVEVALIDDAYS != null)
                        {
                            dLastUseLevelDayCount = entLast.LEAVEVALIDDAYS.Value;
                        }

                        if (entLast.ANNUALLEAVEVALIDDAYS != null)
                        {
                            dLastUseYearDays = entLast.ANNUALLEAVEVALIDDAYS.Value;
                        }
                    }
                }

                entity.LastUseLevelDayCount = dLastUseLevelDayCount.ToString();
                entity.LastUseYearDays = dLastUseYearDays.ToString();

                var entLCs = from c in dal.GetObjects()
                             where c.EMPLOYEEID == strEmployeeID && c.EFFICDATE >= dtCurrentYearStart && c.EFFICDATE <= dtCurrentYearEnd
                             select c;

                decimal dYearDays = 0, dLeavelDayCount = 0, dUsedLeavelDays = 0, dUsedYearDays = 0;
                decimal dUseLeavelDay = 0, dUseYearDays = 0;


                if (!entLastAttSolAsign.T_HR_ATTENDANCESOLUTIONReference.IsLoaded)
                {
                    entLastAttSolAsign.T_HR_ATTENDANCESOLUTIONReference.Load();
                }
                if (entLastAttSolAsign.T_HR_ATTENDANCESOLUTION == null)
                {
                    Tracer.Debug("考勤方案应用的考勤方案为空");
                    throw (new Exception("考勤方案应用的考勤方案为空"));
                }

                T_HR_ATTENDANCESOLUTION entAttSol = entLastAttSolAsign.T_HR_ATTENDANCESOLUTION;

                if (entAttSol != null)
                {
                    if (entAttSol.ISEXPIRED == (Convert.ToInt32(Common.IsChecked.No) + 1).ToString())
                    {
                        dUseLeavelDay = dLastUseLevelDayCount;
                    }
                    else
                    {
                        decimal dLastAdjustExpiredValue = 0;
                        if (entAttSol.ADJUSTEXPIREDVALUE != null)
                        {
                            dLastAdjustExpiredValue = entAttSol.ADJUSTEXPIREDVALUE.Value;
                        }

                        decimal dDays = Utility.DateDiff(dtLastYearEnd, dtCurrentYearEnd, "d");
                        if (dDays < dLastAdjustExpiredValue)
                        {
                            dUseLeavelDay = dLastUseLevelDayCount;
                        }
                    }
                }


                if (entLCs != null)
                {
                    if (entLCs.Count() > 0)
                    {
                        string strAdjustLeave = (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString();
                        string strAnnualLeave = (Convert.ToInt32(Common.LeaveTypeValue.AnnualLeave) + 1).ToString();
                        LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL();
                        IQueryable<T_HR_LEAVETYPESET> entLes = bllLeaveTypeSet.GetLeaveTypeSetRdListForAttendanceSolution(entAttSol.ATTENDANCESOLUTIONID, " LEAVETYPESETID ");

                        var eas = entLes.Where(c => c.LEAVETYPEVALUE == strAdjustLeave);
                        var eys = entLes.Where(c => c.LEAVETYPEVALUE == strAnnualLeave);

                        if (eas.Count() > 0)
                        {
                            T_HR_LEAVETYPESET entAdjustLs = eys.FirstOrDefault();

                            if (entAdjustLs != null)
                            {

                                var eac = from a in dal.GetObjects()
                                          where a.LEAVETYPESETID == entAdjustLs.LEAVETYPESETID && a.EFFICDATE >= dtCurrentYearStart && a.EFFICDATE <= dtCurrentYearEnd && a.TERMINATEDATE <= dtCurrentYearEnd
                                          select a;

                                if (eac != null)
                                {
                                    foreach (T_HR_EMPLOYEELEVELDAYCOUNT entAdjust in eac)
                                    {
                                        if (entAdjust.DAYS != null)
                                        {
                                            dLeavelDayCount += entAdjust.DAYS.Value;

                                            var eacud = from d in dal.GetObjects<T_HR_EMPLOYEELEVELDAYDETAILS>().Include("T_HR_EMPLOYEELEVELDAYCOUNT")
                                                        where d.T_HR_EMPLOYEELEVELDAYCOUNT.RECORDID == entAdjust.RECORDID
                                                        orderby d.EFFICDATE descending
                                                        select d;

                                            if (eacud != null)
                                            {
                                                foreach (T_HR_EMPLOYEELEVELDAYDETAILS entAdjustDet in eacud)
                                                {
                                                    if (entAdjustDet.DAYS != null)
                                                    {
                                                        dUsedLeavelDays += entAdjustDet.DAYS.Value;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (dLeavelDayCount > 0 && dUsedLeavelDays > 0)
                                    {
                                        dUseLeavelDay += dLeavelDayCount - dUsedLeavelDays;
                                    }
                                }
                            }
                        }

                        if (eys.Count() > 0)
                        {
                            T_HR_LEAVETYPESET entAnnuals = eys.FirstOrDefault();
                            if (entAnnuals != null)
                            {

                                var eyc = from n in dal.GetObjects()
                                          where n.LEAVETYPESETID == entAnnuals.LEAVETYPESETID
                                          select n;

                                if (eyc != null)
                                {
                                    foreach (T_HR_EMPLOYEELEVELDAYCOUNT entAnnual in eyc)
                                    {
                                        if (entAnnual.DAYS != null)
                                        {
                                            dYearDays += entAnnual.DAYS.Value;

                                            var eycud = from t in dal.GetObjects<T_HR_EMPLOYEELEVELDAYDETAILS>().Include("T_HR_EMPLOYEELEVELDAYCOUNT")
                                                        where t.T_HR_EMPLOYEELEVELDAYCOUNT.RECORDID == entAnnual.RECORDID
                                                        orderby t.EFFICDATE descending
                                                        select t;

                                            if (eycud != null)
                                            {
                                                foreach (T_HR_EMPLOYEELEVELDAYDETAILS entAnnualDet in eycud)
                                                {
                                                    if (entAnnualDet.DAYS != null)
                                                    {
                                                        dUsedYearDays += entAnnualDet.DAYS.Value;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (dLeavelDayCount > 0 && dUsedLeavelDays > 0)
                                    {
                                        dUseYearDays += dYearDays - dUsedYearDays;
                                    }
                                }
                            }
                        }
                    }
                }


                entity.YearDays = dYearDays.ToString();
                entity.LeavelDayCount = dLeavelDayCount.ToString();
                entity.UsedLeavelDays = dUsedLeavelDays.ToString();
                entity.UsedYearDays = dUsedYearDays.ToString();
                entity.UseLeavelDay = dUseLeavelDay.ToString();
                entity.UseYearDays = dUseYearDays.ToString();
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

            return entity;
        }

        /// <summary>
        /// 根据条件，获取员工可休假信息
        /// </summary>
        /// <param name="strOwnerID">权限控制所有人的员工ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>员工可休假信息</returns>
        public IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> GetAllEmployeeLevelDayCountRdListByMultSearch(string sOrgType,
            string sValue, string strLeaveType, string strOwnerID, string strEmployeeID, string strEfficDateFrom, string strEfficDateTo,
            string strSortKey)
        {
            EmployeeLevelDayCountDAL dalEmployeeleveldaycount = new EmployeeLevelDayCountDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                strfilter.Append(" EMPLOYEEID == @0");
                objArgs.Add(strEmployeeID);
            }

            if (!string.IsNullOrEmpty(strLeaveType))
            {
                int iIndex = 0;
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" LEAVETYPESETID == @" + iIndex.ToString());
                objArgs.Add(strLeaveType);
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEELEVELDAYCOUNT");

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = "EFFICDATE";
            }

            var q = dalEmployeeleveldaycount.GetEmployeeLevelDayCountRdListByMultSearch(sOrgType, sValue, strEfficDateFrom, strEfficDateTo, strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取员工可休假信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">权限控制人的员工ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工可休假信息</returns>
        public IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> GetEmployeeLevelDayCountRdListByMultSearch(string sOrgType,
            string sValue, string strLeaveType, string strOwnerID, string strEmployeeID, string strEfficDateFrom,
            string strEfficDateTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllEmployeeLevelDayCountRdListByMultSearch(sOrgType, sValue, strLeaveType, strOwnerID, strEmployeeID, strEfficDateFrom, strEfficDateTo, strSortKey);

            return Utility.Pager<T_HR_EMPLOYEELEVELDAYCOUNT>(q, pageIndex, pageSize, ref pageCount);
        }

        #region 导出带薪假期
        /// <summary>
        /// 导出带薪假期
        /// </summary>
        /// <param name="sOrgType"></param>
        /// <param name="sValue"></param>
        /// <param name="strLeaveType"></param>
        /// <param name="strOwnerID"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="strEfficDateFrom"></param>
        /// <param name="strEfficDateTo"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public byte[] ExportEmployeeLeaveDayCount(string sOrgType, string sValue, string strLeaveType, string strOwnerID, string strEmployeeID, string strEfficDateFrom,
            string strEfficDateTo, string strSortKey)
        {
            byte[] result = null;
            try
            {
                List<T_HR_EMPLOYEELEVELDAYCOUNT> list = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> entList = GetAllEmployeeLevelDayCountRdListByMultSearch(sOrgType, sValue, strLeaveType, strOwnerID, strEmployeeID, strEfficDateFrom, strEfficDateTo, strSortKey);
                if (entList.Count() > 0)
                {
                    list = entList.ToList();
                }
                result = EmployeeLeaveDayCountStream(list);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeeLeaveDayCount:" + ex.Message);
            }
            return result;
        }

        private byte[] EmployeeLeaveDayCountStream(List<T_HR_EMPLOYEELEVELDAYCOUNT> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetEmployeeLeaveDayCountBody(list));
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());
            return by;
        }

        private string GetEmployeeLeaveDayCountBody(List<T_HR_EMPLOYEELEVELDAYCOUNT> Collects)
        {
            StringBuilder s = new StringBuilder();
            List<T_SYS_DICTIONARY> tmp = new List<T_SYS_DICTIONARY>();// new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "LEAVETYPEVALUE" });
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                tmp = bll.GetSysDictionaryByCategory(new List<string> { "LEAVETYPEVALUE" });
            }
            
            
            s.Append("<body>\n\r");
            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" >员工姓名</td>");
            s.Append("<td align=center class=\"title\" >休假类型</td>");
            s.Append("<td align=center class=\"title\" >天数</td>");
            s.Append("<td align=center class=\"title\" >生效日期</td>");
            s.Append("<td align=center class=\"title\" >终止日期</td>");
            s.Append("</tr>");

            if (Collects.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    string vacationtype = tmp.Where(e => e.DICTIONARYVALUE == decimal.Parse(Collects[i].VACATIONTYPE)).FirstOrDefault().DICTIONARYNAME;
                    s.Append("<tr>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEENAME + "</td>");
                    s.Append("<td class=\"x1282\">" + vacationtype + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].DAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EFFICDATE.Value.ToString("yyyy-MM-dd") + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].TERMINATEDATE.Value.ToString("yyyy-MM-dd") + "</td>");
                    s.Append("</tr>");
                }
            }
            s.Append("</table>");
            s.Append("</body></html>");
            return s.ToString();
        }
        #endregion



        public IQueryable<T_HR_EMPLOYEELEAVERECORD> GetEmployeeleaverecordByMultSearchId(string employeeid, string leavetypesetid, string OWNERCOMPANYID,
            string strEfficDateFrom, string strEfficDateTo, int pageIndex, int pageSize, ref int pageCount)
        {
            EmployeeLevelDayCountDAL dal = new EmployeeLevelDayCountDAL();
            var q = dal.GetEmployeeleaverecordByMultSearchId(employeeid, leavetypesetid, OWNERCOMPANYID, strEfficDateFrom, strEfficDateTo);
            if (q != null)
            {
                return Utility.Pager<T_HR_EMPLOYEELEAVERECORD>(q, pageIndex, pageSize, ref pageCount);
            }
            else
            {
                return null;
            }

        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增员工可休假信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddEmployeeLevelDayCount(T_HR_EMPLOYEELEVELDAYCOUNT entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" RECORDID == @0");

                objArgs.Add(entTemp.RECORDID);

                EmployeeLevelDayCountDAL dalEmployeeleveldaycount = new EmployeeLevelDayCountDAL();
                flag = dalEmployeeleveldaycount.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                int i = dalEmployeeleveldaycount.Add(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改T_HR_EMPLOYEELEVELDAYDETAILS信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyLevelDayCount(T_HR_EMPLOYEELEVELDAYCOUNT entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }


                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" RECORDID == @0");

                objArgs.Add(entTemp.RECORDID);

                EmployeeLevelDayCountDAL dalLevel = new EmployeeLevelDayCountDAL();
                flag = dalLevel.IsExistsRd(null, null, strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_EMPLOYEELEVELDAYCOUNT entUpdate = dal.GetObjects().FirstOrDefault(c => c.RECORDID == entTemp.RECORDID);
                Utility.CloneEntity(entTemp, entUpdate);

                dalLevel.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 新增员工可休假信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddEmployeeLevelDayCountToContext(T_HR_EMPLOYEELEVELDAYCOUNT entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" RECORDID == @0");

                objArgs.Add(entTemp.RECORDID);

                EmployeeLevelDayCountDAL dalEmployeeleveldaycount = new EmployeeLevelDayCountDAL();
                flag = dalEmployeeleveldaycount.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dalEmployeeleveldaycount.AddToContext(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改T_HR_EMPLOYEELEVELDAYDETAILS信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyLevelDayCountFromContext(T_HR_EMPLOYEELEVELDAYCOUNT entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }


                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" RECORDID == @0");

                objArgs.Add(entTemp.RECORDID);

                EmployeeLevelDayCountDAL dalLevel = new EmployeeLevelDayCountDAL();
                flag = dalLevel.IsExistsRd(null, null, strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_EMPLOYEELEVELDAYCOUNT entUpdate = dal.GetObjects().FirstOrDefault(c => c.RECORDID == entTemp.RECORDID);
                Utility.CloneEntity(entTemp, entUpdate);

                dalLevel.UpdateFromContext(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }


        /// <summary>
        /// 升级数据用不再程序中调用
        /// </summary>
        /// <param name="strOrgType"></param>
        /// <param name="strOrgID"></param>
        public decimal GetEmployeeAnnualDaysByEmployee(string strOrgType, string strEmployeeId)
        {
            //strOrgType = 4
            decimal dAnnualDays = 0;
            DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            string strCheckStates = (Convert.ToInt32(Common.CheckStates.Approved)).ToString();

            AttendanceSolutionAsignBLL bllAttAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttAsign = bllAttAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeId, dtCur);

            EmployeeBLL bllEmployee = new EmployeeBLL();
            T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByID(strEmployeeId);

            if (entAttAsign != null && entEmployee != null)
            {
                EmployeeLevelDayCountBLL bllLevelDayCount = new EmployeeLevelDayCountBLL();
                dAnnualDays = bllLevelDayCount.GetEmployeeAnnualDays(entAttAsign, entEmployee, "0");
            }
            return dAnnualDays;
        }


        /// <summary>
        /// 计算指定机构范围内的员工带薪假，根据对应的带薪假记录是否已生成，进行更新或新增
        /// </summary>
        /// <param name="strOrgType"></param>
        /// <param name="strOrgID"></param>
        public void CalculateEmployeeLevelDayCountByOrgID(string strOrgType, string strOrgId)
        {
            DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            string strCheckStates = (Convert.ToInt32(Common.CheckStates.Approved)).ToString();

            if (strOrgType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
            {
                var ents = from n in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                           where n.ASSIGNEDOBJECTTYPE == strOrgType && n.OWNERCOMPANYID == strOrgId
                           && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                           orderby n.ASSIGNEDOBJECTTYPE ascending
                           select n;

                if (ents == null)
                {
                    Tracer.Debug("生成公司带薪假失败，获取考勤方案为NULL，机构类型： " + strOrgType + " 机构id：" + strOrgId);
                    return;
                }

                T_HR_ATTENDANCESOLUTIONASIGN item = ents.FirstOrDefault();
                CreateLevelDayCountByAsignAttSol(item.ATTENDANCESOLUTIONASIGNID);
            }
            else if (strOrgType == (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString())
            {
                AttendanceSolutionAsignBLL bllAttAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entAttAsign = bllAttAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strOrgId, dtCur);

                EmployeeBLL bllEmployee = new EmployeeBLL();
                T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByID(strOrgId);

                if (entAttAsign == null || entEmployee == null)
                {
                    Tracer.Debug("生成带薪假失败，获取的员工或该员工的考勤方案为空或已失效，获取的时间：" + dtCur.ToString("yyyy-MM-dd") + "员工id：" + strOrgId);
                    return;
                }

                EmployeeLevelDayCountBLL bllLevelDayCount = new EmployeeLevelDayCountBLL();
                bllLevelDayCount.CalculateEmployeeLevelDayCount(entAttAsign, entEmployee, "0");
            }
            else if (string.IsNullOrWhiteSpace(strOrgType))
            {
                strOrgType = (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString();
                var ents = from n in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                           where n.ASSIGNEDOBJECTTYPE == strOrgType && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                           select n;

                if (ents == null)
                {
                    return;
                }

                foreach (T_HR_ATTENDANCESOLUTIONASIGN item in ents)
                {
                    try
                    {
                        CreateLevelDayCountByAsignAttSol(item.ATTENDANCESOLUTIONASIGNID);
                        Utility.SaveLog(DateTime.Now.ToString() + " 生成带薪假成功，生成成功的公司ID：" + item.ASSIGNEDOBJECTID);
                    }
                    catch (Exception ex)
                    {
                        Utility.SaveLog(DateTime.Now.ToString() + " 生成带薪假失败，生成失败的公司ID：" + item.ASSIGNEDOBJECTID + "。失败的原因是：" + ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 对所有公司生成带薪假记录
        /// </summary>
        public string CreateLevelDayCountWithAllCompany()
        {
            string strRes = string.Empty;
            try
            {
                DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
                string strCheckStates = Convert.ToInt32(CheckStates.Approved).ToString();
                string strAssignObjectType = Convert.ToInt32(SMT.HRM.DAL.AssignObjectType.Company).ToString();

                var ents = from n in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                           where n.ASSIGNEDOBJECTTYPE == strAssignObjectType && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                           //&& n.OWNERCOMPANYID == "703dfb3c-d3dc-4b1d-9bf0-3507ba01b716"
                           //&& n.OWNERCOMPANYID =="bac05c76-0f5b-40ae-b73b-8be541ed35ed"
                           orderby n.ASSIGNEDOBJECTTYPE ascending
                           select n;

                List<string> strIDs = new List<string>();

                foreach (T_HR_ATTENDANCESOLUTIONASIGN item in ents)
                {
                    if (strIDs.Contains(item.ASSIGNEDOBJECTID))
                    {
                        continue;
                    }

                    CreateLevelDayCountByAsignAttSol(item.ATTENDANCESOLUTIONASIGNID);
                    strIDs.Add(item.ASSIGNEDOBJECTID);
                }

                return "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                strRes = "{ERROR}";
            }

            return strRes;
        }


        public string CreateLevelDayCountWithAllCompanyForSingle(string companyID)
        {
            string strRes = string.Empty;
            try
            {
                DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
                string strCheckStates = Convert.ToInt32(CheckStates.Approved).ToString();
                string strAssignObjectType = Convert.ToInt32(SMT.HRM.DAL.AssignObjectType.Company).ToString();

                var ents = from n in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                           where n.ASSIGNEDOBJECTTYPE == strAssignObjectType && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                               //&& n.OWNERCOMPANYID == "703dfb3c-d3dc-4b1d-9bf0-3507ba01b716"
                           && n.OWNERCOMPANYID == companyID
                           orderby n.ASSIGNEDOBJECTTYPE ascending
                           select n;

                List<string> strIDs = new List<string>();

                foreach (T_HR_ATTENDANCESOLUTIONASIGN item in ents)
                {
                    if (strIDs.Contains(item.ASSIGNEDOBJECTID))
                    {
                        continue;
                    }

                    CreateLevelDayCountByAsignAttSol(item.ATTENDANCESOLUTIONASIGNID);
                    strIDs.Add(item.ASSIGNEDOBJECTID);
                }
                SMT.Foundation.Log.Tracer.Debug("执行完成：" + companyID);
                return companyID + "执行完成";
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("CreateLevelDayCountWithAllCompanyForSingle出现错误：" + ex.ToString());
                Utility.SaveLog(ex.ToString());
                strRes = "{ERROR}";
            }
            return strRes;
        }

        /// <summary>
        /// 对指定员工应用考勤方案，生成其可休假记录，如存在，则重置
        /// </summary>
        /// <param name="entEmployee"></param>
        /// <param name="entTemp"></param>
        public void CreateLevelDayCountByAsignAttSol(string strAttendanceSolutionAsignId)
        {
            //获取考勤方案
            AttendanceSolutionAsignBLL bllAttSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign = bllAttSolAsign.GetAttendanceSolutionAsignByID(strAttendanceSolutionAsignId);

            if (entAttSolAsign == null)
            {
                return;
            }

            if (entAttSolAsign.ENDDATE < DateTime.Now)
            {
                return;
            }

            string strAssignedObjectType = entAttSolAsign.ASSIGNEDOBJECTTYPE;
            string strAssignedObjectID = entAttSolAsign.ASSIGNEDOBJECTID;

            EmployeeBLL bllEmployee = new EmployeeBLL();
            List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();

            //根据考勤方案应用的分配对象获取员工
            DateTime dtCheck = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01");
            if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByCompanyID(strAssignedObjectID, dtCheck).ToList();
            }
            else if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByDepartmentID(strAssignedObjectID).ToList();
            }
            else if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByPostID(strAssignedObjectID).ToList();
            }

            if (entEmployees.Count() == 0)
            {
                return;
            }

            //操作表T_HR_EMPLOYEELEVELDAYCOUNT数据的方式：0：直接逐条新增或修改；
            //1：先把要新增或修改的记录存到内存，然后一次性提交到数据库修改
            string strOperationType = "0";
            foreach (T_HR_EMPLOYEE entEmployee in entEmployees)
            {
                EmployeeLevelDayCountBLL bllLevelDayCount = new EmployeeLevelDayCountBLL();
                bllLevelDayCount.CalculateEmployeeLevelDayCount(entAttSolAsign, entEmployee, strOperationType);
            }
        }


        /// <summary>
        /// 升级数据用不再程序中调用
        /// </summary>
        /// <param name="entAttSolAsign"></param>
        /// <param name="entEmployee"></param>
        /// <param name="strOperationType"></param>
        /// <returns></returns>
        public decimal GetEmployeeAnnualDays(T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign, T_HR_EMPLOYEE entEmployee, string strOperationType)
        {
            decimal dAnnualDays = 0;
            try
            {
                DateTime dtStart = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
                if (dtStart < entAttSolAsign.STARTDATE.Value)
                {
                    dtStart = entAttSolAsign.STARTDATE.Value;
                }

                DateTime dtEnd = dtStart.AddMonths(1).AddDays(-1);

                decimal dCurWorkAge = 0, dEmployeePostLevel = 0;
                //最后一次入职时间
                var qw = from n in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")
                         where n.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID
                         && n.EDITSTATE == "1"
                         && n.CHECKSTATE == "2"
                         select n;
                //获取最早的入职时间
                List<T_HR_EMPLOYEEENTRY> qwo = dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                        .Include("T_HR_EMPLOYEE")
                        .Where(t => t.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID && t.CHECKSTATE == "2")
                        .OrderBy(t => t.ONPOSTDATE).ToList();

                List<T_HR_LEFTOFFICECONFIRM> leftOfficeList = dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE")
                        .Where(t => t.EMPLOYEEID == entEmployee.EMPLOYEEID && t.CHECKSTATE == "2")
                        .OrderBy(t => t.STOPPAYMENTDATE).ToList();
                //最后一次入职时间
                T_HR_EMPLOYEEENTRY entEntry = new T_HR_EMPLOYEEENTRY();
                DateTime dtEntryDate = new DateTime();
                TimeSpan tsWorkTime = DateTime.Now.Subtract(dtEntryDate);

                DateTime tempEntryDate = DateTime.Now;
                TimeSpan tempTsWorkTime = new TimeSpan();

                if (qw != null && qw.Count() > 0)
                {
                    entEntry = qw.FirstOrDefault();
                    if (entEntry != null)
                    {
                        //第一次入职时间
                        dtEntryDate = entEntry.ONPOSTDATE.Value;
                        tempEntryDate = entEntry.ONPOSTDATE.HasValue ? entEntry.ONPOSTDATE.Value : Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                }

                if (isHuNanHangXingSalary == "false")
                {
                    List<string> EntryLeftDateList = new List<string>();


                    if (qwo.Count > leftOfficeList.Count)
                    {
                        int k = 0;
                        foreach (var item in qwo)
                        {
                            //将第一次入职日期特殊处理
                            k++;
                            if (k == 1)
                            {
                                EntryLeftDateList.Add(DateTime.MinValue.ToString() + "|" + item.ONPOSTDATE.Value);
                            }
                            else
                            {
                                if (leftOfficeList.Count > 0)
                                {
                                    string strTempEntryDate = DateTime.MinValue.ToString();
                                    if (leftOfficeList[0].T_HR_LEFTOFFICE != null && !string.IsNullOrEmpty(leftOfficeList[0].T_HR_LEFTOFFICE.DIMISSIONID))
                                    {
                                        strTempEntryDate = leftOfficeList[0].STOPPAYMENTDATE.HasValue ? leftOfficeList[0].STOPPAYMENTDATE.Value.ToString() : string.Empty;
                                        EntryLeftDateList.Add(strTempEntryDate + "|" + leftOfficeList[0].STOPPAYMENTDATE.Value.AddDays(15));
                                    }
                                    else
                                    {
                                        strTempEntryDate = leftOfficeList[0].STOPPAYMENTDATE.HasValue ? leftOfficeList[0].STOPPAYMENTDATE.Value.ToString() : string.Empty;
                                        EntryLeftDateList.Add(strTempEntryDate + "|" + item.ONPOSTDATE.Value);
                                        leftOfficeList.RemoveAt(0);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int k = 0;
                        foreach (var item in leftOfficeList)
                        {
                            //将第一次入职日期特殊处理

                            k++;
                            if (k == 1)
                            {
                                EntryLeftDateList.Add(DateTime.MinValue.ToString() + "|" + qwo[0].ONPOSTDATE.Value);
                                qwo.RemoveAt(0);
                            }
                            else
                            {
                                if (qwo.Count > 0)
                                {
                                    string strTempEntryDate = DateTime.MinValue.ToString();
                                    if (item.T_HR_LEFTOFFICE != null && !string.IsNullOrEmpty(item.T_HR_LEFTOFFICE.DIMISSIONID))
                                    {
                                        //strTempEntryDate = qwo[0].ONPOSTDATE.HasValue ? qwo[0].ONPOSTDATE.Value.ToString() : string.Empty;
                                        EntryLeftDateList.Add(item.STOPPAYMENTDATE.Value + "|" + item.STOPPAYMENTDATE.Value.AddDays(15));
                                    }
                                    else
                                    {
                                        strTempEntryDate = qwo[0].ONPOSTDATE.HasValue ? qwo[0].ONPOSTDATE.Value.ToString() : string.Empty;
                                        EntryLeftDateList.Add(item.STOPPAYMENTDATE.Value + "|" + strTempEntryDate);
                                        qwo.RemoveAt(0);
                                    }
                                }
                            }
                        }
                    }

                    EntryLeftDateList.Reverse();

                    foreach (var item in EntryLeftDateList)
                    {
                        string[] dateArr = item.Split('|');
                        DateTime leftDate = DateTime.MinValue;
                        DateTime entryDate = DateTime.MaxValue;
                        if (!string.IsNullOrEmpty(dateArr[0]))
                        {
                            DateTime.TryParse(dateArr[0], out leftDate);
                        }
                        if (!string.IsNullOrEmpty(dateArr[1]))
                        {
                            DateTime.TryParse(dateArr[1], out entryDate);
                        }
                        tempTsWorkTime = entryDate.Subtract(leftDate);
                        if (tempTsWorkTime.Days <= 30)
                        {
                            tempEntryDate = entryDate;
                        }
                        else
                        {
                            tempEntryDate = entryDate;
                            break;
                        }
                    }
                }

                dtEntryDate = tempEntryDate;
                tsWorkTime = DateTime.Now.Subtract(dtEntryDate);

                T_HR_EMPLOYEEENTRY entEntryEarly = new T_HR_EMPLOYEEENTRY();
                entEntryEarly = dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                                .Include("T_HR_EMPLOYEE")
                                .Where(t => t.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID && t.ONPOSTDATE == dtEntryDate)
                                .FirstOrDefault();


                //员工社保缴交时间，用于计算员工年休假
                string strSocialServiceDate = string.IsNullOrEmpty(entEmployee.SOCIALSERVICEYEAR) ? DateTime.Now.ToString("yyyy-MM-dd") : entEmployee.SOCIALSERVICEYEAR;
                DateTime dtSocialServiceDate = DateTime.Now;
                DateTime.TryParse(strSocialServiceDate, out dtSocialServiceDate);

                //修改入职刚满一年的公式：(当前日期-入职日期)>12月 可以休5天.以后每年可以休5天.                
                TimeSpan tsSocialYears = DateTime.Now.Subtract(dtSocialServiceDate);
                decimal dCurSocialYears = decimal.Round(tsSocialYears.Days / 365, 0);

                //在公司任职时间，也就是工龄
                dCurWorkAge = decimal.Round(tsWorkTime.Days / 30, 0);
                //客户：湖南航信的处理方式
                string isHangXing = Utility.GetAppConfigByName("isForHuNanHangXingSalary");
                decimal AnnualYearDays = 0;
                decimal MaxSickLeaveDay = 15;
                DateTime SickLeaveEffectiveDate = DateTime.MinValue;
                DateTime SickLeaveExpireDate = DateTime.MinValue;
                decimal CurrrentYearAnnualDays = 0;
                double averageWorkPerDay = 7.5;
                if (isHangXing == "false")//集团
                {
                    if (dCurSocialYears < 10 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.LessTenYears.GetHashCode();
                    }
                    else if (dCurSocialYears >= 10 && dCurSocialYears < 20 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.TenToTwentyYears.GetHashCode();
                    }
                    else if (dCurSocialYears >= 20 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.TwentyYears.GetHashCode();
                    }


                    if (dtEntryDate.AddMonths(12) > Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"))
                        && dtEntryDate.AddMonths(12) <= dtEntryDate.AddMonths((int)dCurWorkAge))
                    {
                        //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                        CurrrentYearAnnualDays = Math.Round((12 - dtEntryDate.Month) / (decimal)12.0 * AnnualYearDays, 2);
                    }
                    else if (dCurWorkAge > 12)
                    {
                        decimal h1 = 0;
                        decimal h2 = 0;
                        if ((dCurSocialYears - 10) == 0)
                        {
                            //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                            h1 = Math.Round(dtEntryDate.Month / (decimal)12.0 * 5 * (decimal)averageWorkPerDay, 2);
                            h2 = Math.Round((12 - dtEntryDate.Month) / (decimal)12.0 * 10 * (decimal)averageWorkPerDay, 2);
                            CurrrentYearAnnualDays = Math.Round((h1 + h2) / (decimal)7.5, 2);
                        }
                        else if ((dCurSocialYears - 20) == 0)
                        {
                            //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                            h1 = Math.Round(dtEntryDate.Month / (decimal)12.0 * 10 * (decimal)averageWorkPerDay, 2);
                            h2 = Math.Round((12 - dtEntryDate.Month) / (decimal)12.0 * 15 * (decimal)averageWorkPerDay, 2);
                            CurrrentYearAnnualDays = Math.Round((h1 + h2) / (decimal)7.5, 2);
                        }
                        else
                        {
                            CurrrentYearAnnualDays = AnnualYearDays;
                        }                        
                    }
                    else
                    {
                        //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                        CurrrentYearAnnualDays = Math.Round((12 - dtEntryDate.Month) / (decimal)12.0 * AnnualYearDays, 2);
                    }
                }


                DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("2014-01-01"));
                DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("2015-04-15"));
                T_HR_EMPLOYEELEVELDAYCOUNT empAnnualDay = dal.GetObjects().Where(t => t.EFFICDATE >= currentStartDate
                    && t.TERMINATEDATE <= currentEndDate
                    && t.EMPLOYEEID == entEmployee.EMPLOYEEID
                    && t.VACATIONTYPE == "4" && t.STATUS == 1).FirstOrDefault();
                if (empAnnualDay != null)
                {
                    //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                    decimal h1 = Math.Round(dtEntryDate.Month / (decimal)12.0 * empAnnualDay.DAYS.Value * (decimal)averageWorkPerDay, 2);
                    decimal h2 = Math.Round((12 - dtEntryDate.Month) / (decimal)12.0 * AnnualYearDays * (decimal)averageWorkPerDay, 2);
                    decimal preLeftHours = empAnnualDay.LEFTHOURS.HasValue ? empAnnualDay.LEFTHOURS.Value : 0;
                    decimal preTotalHours = empAnnualDay.HOURS.HasValue ? empAnnualDay.HOURS.Value : 0;
                    //若入职时间大于年假的创建时间，并且入职时间在12个月到24个月之间
                    if (dtEntryDate.AddMonths(12) > Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"))
                        && dtEntryDate.AddMonths(12) <= dtEntryDate.AddMonths((int)dCurWorkAge))
                    {
                        dAnnualDays = Math.Round(CurrrentYearAnnualDays * (decimal)averageWorkPerDay, 2);

                    }
                    else
                    {
                        if (empAnnualDay.DAYS.Value != AnnualYearDays)
                        {
                            dAnnualDays = Math.Round((h1 + h2) - (preTotalHours - preLeftHours), 0);
                            // dAnnualDays = (h1 + h2);
                        }
                        else
                        {
                            //dAnnualDays = Math.Round((h1 + h2) - (preTotalHours - preLeftHours), 0); 
                            dAnnualDays = (h1 + h2);
                        }
                    }
                }

                //dAnnualDays = Math.Round(CurrrentYearAnnualDays * (decimal)7.5, 2);
            }
            catch (Exception ex)
            { }
            return dAnnualDays;
        }

        /// <summary>
        /// 根据考勤方案应用ID，计算员工的带薪假天数，并更新计算结果到可休假表
        /// </summary>
        /// <param name="entAttSolAsign">考勤方案应用实体</param>
        /// <param name="entEmployee">员工实体</param>
        /// <param name="strOperationType">保存到数据库的操作方式</param>
        public void CalculateEmployeeLevelDayCount(T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign, T_HR_EMPLOYEE entEmployee, string strOperationType)
        {
            try
            {
                DateTime dtStart = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
                if (dtStart < entAttSolAsign.STARTDATE.Value)
                {
                    dtStart = entAttSolAsign.STARTDATE.Value;
                }

                DateTime dtEnd = dtStart.AddMonths(1).AddDays(-1);

                decimal dCurWorkAge = 0, dEmployeePostLevel = 0;
                //最后一次入职时间
                var qw = from n in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")
                         where n.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID
                         && n.EDITSTATE == "1"
                         && n.CHECKSTATE == "2"
                         select n;
                //获取最早的入职时间
                List<T_HR_EMPLOYEEENTRY> qwo = dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                        .Include("T_HR_EMPLOYEE")
                        .Where(t => t.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID && t.CHECKSTATE == "2")
                        .OrderBy(t => t.ONPOSTDATE).ToList();

                List<T_HR_LEFTOFFICECONFIRM> leftOfficeList = dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE")
                        .Where(t => t.EMPLOYEEID == entEmployee.EMPLOYEEID && t.CHECKSTATE == "2")
                        .OrderBy(t => t.STOPPAYMENTDATE).ToList();
                //最后一次入职时间
                T_HR_EMPLOYEEENTRY entEntry = new T_HR_EMPLOYEEENTRY();
                DateTime dtEntryDate = new DateTime();
                TimeSpan tsWorkTime = DateTime.Now.Subtract(dtEntryDate);

                DateTime tempEntryDate = DateTime.Now;
                TimeSpan tempTsWorkTime = new TimeSpan();

                if (qw != null && qw.Count() > 0)
                {
                    entEntry = qw.FirstOrDefault();
                    if (entEntry != null)
                    {
                        //第一次入职时间
                        dtEntryDate = entEntry.ONPOSTDATE.Value;
                        tempEntryDate = entEntry.ONPOSTDATE.HasValue ? entEntry.ONPOSTDATE.Value : Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                }

                if (isHuNanHangXingSalary == "false")
                {
                    List<string> EntryLeftDateList = new List<string>();
                    if (qwo.Count > leftOfficeList.Count)
                    {
                        int k = 0;
                        foreach (var item in qwo)
                        {
                            //将第一次入职日期特殊处理
                            k++;
                            if (k == 1)
                            {
                                EntryLeftDateList.Add(DateTime.MinValue.ToString() + "|" + item.ONPOSTDATE.Value);
                            }
                            else
                            {
                                if (leftOfficeList.Count > 0)
                                {
                                    string strTempEntryDate = DateTime.MinValue.ToString();
                                    if (leftOfficeList[0].T_HR_LEFTOFFICE != null && !string.IsNullOrEmpty(leftOfficeList[0].T_HR_LEFTOFFICE.DIMISSIONID))
                                    {
                                        strTempEntryDate = leftOfficeList[0].STOPPAYMENTDATE.HasValue ? leftOfficeList[0].STOPPAYMENTDATE.Value.ToString() : string.Empty;
                                        EntryLeftDateList.Add(strTempEntryDate + "|" + leftOfficeList[0].STOPPAYMENTDATE.Value.AddDays(15));
                                    }
                                    else
                                    {
                                        strTempEntryDate = leftOfficeList[0].STOPPAYMENTDATE.HasValue ? leftOfficeList[0].STOPPAYMENTDATE.Value.ToString() : string.Empty;
                                        EntryLeftDateList.Add(strTempEntryDate + "|" + item.ONPOSTDATE.Value);
                                        leftOfficeList.RemoveAt(0);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int k = 0;
                        foreach (var item in leftOfficeList)
                        {
                            //将第一次入职日期特殊处理

                            k++;
                            if (k == 1)
                            {
                                EntryLeftDateList.Add(DateTime.MinValue.ToString() + "|" + qwo[0].ONPOSTDATE.Value);
                                qwo.RemoveAt(0);
                            }
                            else
                            {
                                if (qwo.Count > 0)
                                {
                                    string strTempEntryDate = DateTime.MinValue.ToString();
                                    if (item.T_HR_LEFTOFFICE != null && !string.IsNullOrEmpty(item.T_HR_LEFTOFFICE.DIMISSIONID))
                                    {
                                        //strTempEntryDate = qwo[0].ONPOSTDATE.HasValue ? qwo[0].ONPOSTDATE.Value.ToString() : string.Empty;
                                        EntryLeftDateList.Add(item.STOPPAYMENTDATE.Value + "|" + item.STOPPAYMENTDATE.Value.AddDays(15));
                                    }
                                    else
                                    {
                                        strTempEntryDate = qwo[0].ONPOSTDATE.HasValue ? qwo[0].ONPOSTDATE.Value.ToString() : string.Empty;
                                        EntryLeftDateList.Add(item.STOPPAYMENTDATE.Value + "|" + strTempEntryDate);
                                        qwo.RemoveAt(0);
                                    }
                                }
                            }
                        }
                    }

                    EntryLeftDateList.Reverse();

                    foreach (var item in EntryLeftDateList)
                    {
                        string[] dateArr = item.Split('|');
                        DateTime leftDate = DateTime.MinValue;
                        DateTime entryDate = DateTime.MaxValue;
                        if (!string.IsNullOrEmpty(dateArr[0]))
                        {
                            DateTime.TryParse(dateArr[0], out leftDate);
                        }
                        if (!string.IsNullOrEmpty(dateArr[1]))
                        {
                            DateTime.TryParse(dateArr[1], out entryDate);
                        }
                        tempTsWorkTime = entryDate.Subtract(leftDate);
                        if (tempTsWorkTime.Days <= 30)
                        {
                            tempEntryDate = entryDate;
                        }
                        else
                        {
                            tempEntryDate = entryDate;
                            break;
                        }
                    }
                }

                dtEntryDate = tempEntryDate;
                tsWorkTime = DateTime.Now.Subtract(dtEntryDate);

                T_HR_EMPLOYEEENTRY entEntryEarly = new T_HR_EMPLOYEEENTRY();
                entEntryEarly = dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                                .Include("T_HR_EMPLOYEE")
                                .Where(t => t.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID && t.ONPOSTDATE == dtEntryDate)
                                .FirstOrDefault();


                //员工社保缴交时间，用于计算员工年休假
                string strSocialServiceDate = string.IsNullOrEmpty(entEmployee.SOCIALSERVICEYEAR) ? DateTime.Now.ToString("yyyy-MM-dd") : entEmployee.SOCIALSERVICEYEAR;
                DateTime dtSocialServiceDate = DateTime.Now;
                DateTime.TryParse(strSocialServiceDate, out dtSocialServiceDate);

                //修改入职刚满一年的公式：(当前日期-入职日期)>12月 可以休5天.以后每年可以休5天.                
                TimeSpan tsSocialYears = DateTime.Now.Subtract(dtSocialServiceDate);
                decimal dCurSocialYears = decimal.Round(tsSocialYears.Days / 365, 0);

                //在公司任职时间，也就是工龄
                dCurWorkAge = decimal.Round(tsWorkTime.Days / 30, 0);
                //客户：湖南航信的处理方式
                string isHangXing = Utility.GetAppConfigByName("isForHuNanHangXingSalary");
                decimal AnnualYearDays = 0;
                decimal MaxSickLeaveDay = 15;
                DateTime SickLeaveEffectiveDate = DateTime.MinValue;
                DateTime SickLeaveExpireDate = DateTime.MinValue;
                decimal CurrrentYearAnnualDays = 0;
                if (isHangXing == "false")//集团
                {
                    if (dCurSocialYears < 10 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.LessTenYears.GetHashCode();
                    }
                    else if (dCurSocialYears >= 10 && dCurSocialYears < 20 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.TenToTwentyYears.GetHashCode();
                    }
                    else if (dCurSocialYears >= 20 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.TwentyYears.GetHashCode();
                    }


                    if (dtEntryDate.AddMonths(12) > Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"))
                        && dtEntryDate.AddMonths(12) <= dtEntryDate.AddMonths((int)dCurWorkAge))
                    {
                        //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                        CurrrentYearAnnualDays = Math.Round((12 - dtEntryDate.Month) / (decimal)12.0 * AnnualYearDays, 2);
                    }
                    else if (dCurWorkAge > 12)
                    {
                        CurrrentYearAnnualDays = AnnualYearDays;
                    }
                    else
                    {
                        //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                        CurrrentYearAnnualDays = Math.Round((12 - dtEntryDate.Month) / (decimal)12.0 * AnnualYearDays, 2);
                    }
                }
                else//湖南航信
                {
                    if (dCurSocialYears < 10 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.LessTenYears.GetHashCode();
                    }
                    else if (dCurSocialYears >= 10 && dCurSocialYears < 20 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.TenToTwentyYears.GetHashCode();
                    }
                    else if (dCurSocialYears >= 20 && dCurWorkAge >= 12)
                    {
                        AnnualYearDays = CustomModel.Common.Enums.AnnualYearVacationDays.TwentyYears.GetHashCode();
                    }

                    if (dCurWorkAge > 12)
                    {
                        CurrrentYearAnnualDays = AnnualYearDays;
                    }
                    else
                    {
                        //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                        CurrrentYearAnnualDays = CustomModel.Common.Enums.AnnualYearVacationDays.LessTenYears.GetHashCode();// Math.Round((12 - entEntry.ENTRYDATE.Value.Month) / (decimal)12.0 * AnnualYearDays, 2);
                    }
                }

                //获取员工的详细信息
                var qp = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                         where ep.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID
                         && ep.T_HR_EMPLOYEE.OWNERCOMPANYID == entAttSolAsign.OWNERCOMPANYID
                         && ep.CHECKSTATE == "2"
                         && ep.EDITSTATE == "1"
                         && ep.ISAGENCY == "0"
                         select ep;

                if (qp == null)
                {
                    Utility.SaveLog(DateTime.Now.ToString() + "生成带薪假失败,考勤方案应用的ID是：" + entAttSolAsign.ATTENDANCESOLUTIONASIGNID + ",员工的ID是：" + entEmployee.EMPLOYEEID + "。出错的原因是：该员工无员工岗位信息");
                    return;
                }

                T_HR_EMPLOYEEPOST entPost = qp.FirstOrDefault();

                if (entPost == null)
                {
                    Utility.SaveLog(DateTime.Now.ToString() + "生成带薪假失败,考勤方案应用的ID是：" + entAttSolAsign.ATTENDANCESOLUTIONASIGNID + ",员工的ID是：" + entEmployee.EMPLOYEEID + "。出错的原因是：该员工无员工主岗位信息");
                    return;
                }

                if (entPost.POSTLEVEL == null)
                {
                    Utility.SaveLog(DateTime.Now.ToString() + "生成带薪假失败,考勤方案应用的ID是：" + entAttSolAsign.ATTENDANCESOLUTIONASIGNID + ",员工的ID是：" + entEmployee.EMPLOYEEID + "。出错的原因是：该员工无员工主岗位的岗位级别");
                    return;
                }

                decimal.TryParse(entPost.POSTLEVEL.ToString(), out dEmployeePostLevel);

                //周文斌添加，造这一年12个月份的考勤每天的工作小时数
                //DateList的格式：List<2014-05-01|2014-05-31>
                List<string> monthlist = new List<string>() { 
                    DateTime.Now.ToString("yyyy-MM-01")+"|"+Convert.ToDateTime( DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1)
                    };

                //List<string> monthlist = new List<string>() { 
                //    DateTime.Now.ToString("yyyy-01-01")+"|"+DateTime.Now.ToString("yyyy-01-31"),
                //    DateTime.Now.ToString("yyyy-02-01")+"|"+Convert.ToDateTime( DateTime.Now.ToString("yyyy-02-01")).AddMonths(1).AddDays(-1),
                //    DateTime.Now.ToString("yyyy-03-01")+"|"+DateTime.Now.ToString("yyyy-03-31"),
                //    DateTime.Now.ToString("yyyy-04-01")+"|"+DateTime.Now.ToString("yyyy-04-30"),
                //    DateTime.Now.ToString("yyyy-05-01")+"|"+DateTime.Now.ToString("yyyy-05-31"),
                //    DateTime.Now.ToString("yyyy-06-01")+"|"+DateTime.Now.ToString("yyyy-06-30"),
                //    DateTime.Now.ToString("yyyy-07-01")+"|"+DateTime.Now.ToString("yyyy-07-31"),
                //    DateTime.Now.ToString("yyyy-08-01")+"|"+DateTime.Now.ToString("yyyy-08-31"),
                //    DateTime.Now.ToString("yyyy-09-01")+"|"+DateTime.Now.ToString("yyyy-09-30"),
                //    DateTime.Now.ToString("yyyy-10-01")+"|"+DateTime.Now.ToString("yyyy-10-31"),
                //    DateTime.Now.ToString("yyyy-11-01")+"|"+DateTime.Now.ToString("yyyy-11-30"),
                //    DateTime.Now.ToString("yyyy-12-01")+"|"+DateTime.Now.ToString("yyyy-12-31"),
                //    };

                AttendanceSolutionBLL attendSolutionBll = new AttendanceSolutionBLL();
                List<T_HR_ATTENDANCESOLUTION> attendSolutionList = attendSolutionBll.GetAttendenceSolutionByEmployeeIDAndStartDateAndEndDate(entEmployee.EMPLOYEEID, monthlist);

                double averageWorkPerDay = 7.5;
                //假期的相关设置  
                if (attendSolutionList.Count > 0)
                {
                    //计算跨多个月份时的平均工作时间，只用于参考，因为每个月的考勤不一样，上班时间长度也可能不一样，可能有的7.5小时/天
                    //有的8小时/天，在此取一个平均数,四舍五入计算
                    averageWorkPerDay = Math.Round(Convert.ToDouble(attendSolutionList.Sum(t => t.WORKTIMEPERDAY) / attendSolutionList.Count()), 2);
                }

                //获取考勤方案关联的假期标准(只为带薪假的)
                AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL();
                IQueryable<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves = bllAttendFreeLeave.GetAttendFreeLeaveByAttendSolID(entAttSolAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);

                if (entAttendFreeLeaves == null)
                {
                    Tracer.Debug(entEmployee.EMPLOYEECNAME + "无法生成生成过带薪假" + " 通过考勤方案没有获取到关联的假期标准(只为带薪假的)，方案id：" + entAttSolAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                    return;
                }


                if (isHangXing == "false")//集团
                {
                    #region "   病假可休天数      "
                    if (dCurSocialYears < 10)
                    {
                        if (dCurWorkAge < 12)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(180).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 15;
                        }
                        else if (dCurWorkAge >= 12 && dCurWorkAge < 24)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(180).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 30;
                        }
                        else if (dCurWorkAge >= 24 && dCurWorkAge < 36)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(180).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 45;
                        }
                        else if (dCurWorkAge >= 36 && dCurWorkAge < 48)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(180).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 60;
                        }
                        else if (dCurWorkAge >= 48 && dCurWorkAge < 60)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(180).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 90;
                        }
                        else if (dCurWorkAge >= 60)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(360).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 180;
                        }
                    }
                    else
                    {
                        if (dCurWorkAge < 60)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(360).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 180;
                        }
                        else if (dCurWorkAge >= 60 && dCurWorkAge < 120)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(450).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 270;
                        }
                        else if (dCurWorkAge >= 120 && dCurWorkAge < 180)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(540).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 360;
                        }
                        else if (dCurWorkAge >= 180 && dCurWorkAge < 360)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(720).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 540;
                        }
                        else if (dCurWorkAge >= 360)
                        {
                            SickLeaveEffectiveDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            SickLeaveExpireDate = Convert.ToDateTime(DateTime.Now.AddDays(900).ToString("yyyy-01-01"));
                            MaxSickLeaveDay = 720;
                        }
                    }

                    #endregion

                    LeaveTypeSetBLL leaveTypeSetBll = new LeaveTypeSetBLL();
                    List<T_HR_LEAVETYPESET> ltsList = leaveTypeSetBll.GetLeaveTypeSetAll(entEmployee.EMPLOYEEID);
                    string strSickLeaveDayType = Enums.LeaveVacationType.SickLeaveDay.GetHashCode().ToString();

                    T_HR_ATTENDFREELEAVE freeLtsEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPEVALUE == strSickLeaveDayType).OrderByDescending(t => t.CREATEDATE).FirstOrDefault();
                    T_HR_LEAVETYPESET ltsEntity = new T_HR_LEAVETYPESET();
                    if (freeLtsEntity != null)
                    {
                        ltsEntity = freeLtsEntity.T_HR_LEAVETYPESET;
                    }
                    else
                    {
                        ltsEntity = ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                    }


                    if (ltsEntity != null)
                    {
                        T_HR_EMPLOYEELEVELDAYCOUNT LeaveDayCountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                        DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                        DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-12-31"));

                        LeaveDayCountEntity = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(
                            t => t.LEAVETYPESETID == ltsEntity.LEAVETYPESETID
                            && t.EFFICDATE >= currentStartDate
                            && t.TERMINATEDATE <= currentEndDate
                            && t.EMPLOYEEID == entEmployee.EMPLOYEEID).FirstOrDefault();

                        if (LeaveDayCountEntity == null)
                        {
                            T_HR_EMPLOYEELEVELDAYCOUNT entAdd = new T_HR_EMPLOYEELEVELDAYCOUNT();
                            entAdd.RECORDID = System.Guid.NewGuid().ToString().ToUpper();
                            entAdd.DAYS = MaxSickLeaveDay;                  //可休假 的 可休天数 即为 新方案 对应 假期标准 的 带薪假设置 的 可休假数(天)
                            //if (SickLeaveEffectiveDate <= dtStart)
                            //{
                            //    SickLeaveEffectiveDate = dtStart;
                            //}
                            entAdd.EFFICDATE = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01")); //SickLeaveEffectiveDate;    //可休假 的 生效日期 即为 新方案 的 生效日期
                            entAdd.TERMINATEDATE = Convert.ToDateTime(DateTime.Now.ToString("yyyy-12-31")); //SickLeaveExpireDate;  //可休假 的 终止日期 即为 新方案 的 终止日期
                            entAdd.EMPLOYEEID = entEmployee.EMPLOYEEID;
                            entAdd.EMPLOYEENAME = entEmployee.EMPLOYEECNAME;
                            entAdd.EMPLOYEECODE = entEmployee.EMPLOYEECODE;
                            entAdd.LEAVETYPESETID = ltsEntity.LEAVETYPESETID;
                            entAdd.VACATIONTYPE = Enums.LeaveVacationType.SickLeaveDay.GetHashCode().ToString();
                            entAdd.REMARK = string.Empty;
                            //计算假期的小时数
                            entAdd.STATUS = 1;
                            entAdd.LEFTHOURS = Math.Round(5 * Convert.ToDecimal(averageWorkPerDay), 2);
                            entAdd.HOURS = Math.Round(5 * Convert.ToDecimal(averageWorkPerDay), 2);

                            //获取员工当前主岗位
                            var entEmployeePost = from ent in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                                  join post in dal.GetObjects<T_HR_POST>() on ent.T_HR_POST.POSTID equals post.POSTID
                                                  join dep in dal.GetObjects<T_HR_DEPARTMENT>() on post.T_HR_DEPARTMENT.DEPARTMENTID equals dep.DEPARTMENTID
                                                  where ent.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID
                                                    && ent.ISAGENCY == "0"
                                                    && ent.CHECKSTATE == "2"
                                                    && ent.EDITSTATE == "1"
                                                  orderby ent.UPDATEDATE descending
                                                  select new { ent, post, dep };
                            var ep = entEmployeePost.FirstOrDefault();
                            if (ep != null)
                            {
                                entAdd.OWNERPOSTID = ep.post.POSTID;
                                entAdd.OWNERDEPARTMENTID = ep.dep.DEPARTMENTID;
                                entAdd.OWNERCOMPANYID = ep.post.COMPANYID;
                            }
                            else
                            {
                                entAdd.OWNERPOSTID = entAttSolAsign.OWNERPOSTID;
                                entAdd.OWNERDEPARTMENTID = entAttSolAsign.OWNERDEPARTMENTID;
                                entAdd.OWNERCOMPANYID = entAttSolAsign.OWNERCOMPANYID;
                            }
                            entAdd.OWNERID = entEmployee.EMPLOYEEID;//entAttSolAsign.OWNERID;
                            entAdd.CREATEPOSTID = entAttSolAsign.CREATEPOSTID;
                            entAdd.CREATEDEPARTMENTID = entAttSolAsign.CREATEDEPARTMENTID;
                            entAdd.CREATECOMPANYID = entAttSolAsign.CREATECOMPANYID;
                            entAdd.CREATEUSERID = entEmployee.EMPLOYEEID;//entAttSolAsign.CREATEUSERID;
                            entAdd.CREATEDATE = DateTime.Now;
                            entAdd.UPDATEUSERID = entAttSolAsign.UPDATEUSERID;
                            entAdd.UPDATEDATE = DateTime.Now;
                            dal.AddToContext(entAdd);

                            string strSickLeaveDay = Enums.LeaveVacationType.SickLeaveDay.GetHashCode().ToString();
                            List<T_HR_EMPLOYEELEVELDAYCOUNT> lastYearSickdayList = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t =>
                                 t.EMPLOYEEID == entEmployee.EMPLOYEEID
                                 && t.EFFICDATE.Value >= currentStartDate && t.TERMINATEDATE <= currentEndDate && t.VACATIONTYPE == strSickLeaveDay).ToList();

                            foreach (var lastYearVacation in lastYearSickdayList)
                            {
                                lastYearVacation.STATUS = 0;
                                lastYearVacation.LEFTHOURS = 0;
                                dal.UpdateFromContext(lastYearVacation);
                            }
                        }
                    }
                }

                EmployeeLevelDayCountBLL bllLevelDayCount = new EmployeeLevelDayCountBLL();
                foreach (T_HR_ATTENDFREELEAVE entAttendFreeLeave in entAttendFreeLeaves)
                {
                    T_HR_LEAVETYPESET entLeaveTypeSet = entAttendFreeLeave.T_HR_LEAVETYPESET;

                    if (entLeaveTypeSet == null)
                    {
                        Tracer.Debug(entEmployee.EMPLOYEECNAME + "生成过带薪假" + entLeaveTypeSet.LEAVETYPENAME + "被跳过，无假期设置项目T_HR_LEAVETYPESET无记录");
                        continue;
                    }

                    string strVacType = entLeaveTypeSet.LEAVETYPEVALUE;
                    string strFineType = entLeaveTypeSet.FINETYPE;
                    //如果是调休假，就不需要自动生成
                    if (strVacType == (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
                    {
                        continue;
                    }

                    //获取假期标准的带薪假设置记录
                    FreeLeaveDaySetBLL bllFreeLeave = new FreeLeaveDaySetBLL();
                    IQueryable<T_HR_FREELEAVEDAYSET> entFreeLeaves = bllFreeLeave.GetFreeLeaveDaySetByLeaveTypeID(entLeaveTypeSet.LEAVETYPESETID);

                    if (strFineType != (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                    {
                        Tracer.Debug(entEmployee.EMPLOYEECNAME + "生成过带薪假" + entLeaveTypeSet.LEAVETYPENAME + "被跳过，假期已被设置为不扣款");
                        continue;
                    }

                    decimal dPostLevelStrict = 0;
                    decimal.TryParse(entLeaveTypeSet.POSTLEVELRESTRICT, out dPostLevelStrict);

                    if (dPostLevelStrict > dEmployeePostLevel)
                    {
                        Tracer.Debug(entEmployee.EMPLOYEECNAME + "生成过带薪假" + entLeaveTypeSet.LEAVETYPENAME + "被跳过，岗位级别不够");
                        continue;
                    }

                    if (entLeaveTypeSet.ENTRYRESTRICT == (Convert.ToInt32(Common.IsChecked.Yes) + 1).ToString())
                    {
                        if (entEmployee.EMPLOYEESTATE != "1" && entEmployee.EMPLOYEESTATE != "3")
                        {
                            Tracer.Debug(entEmployee.EMPLOYEECNAME + "生成过带薪假" + entLeaveTypeSet.LEAVETYPENAME + "被跳过，员工状态EMPLOYEESTATE不为1");

                            if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString() && isHangXing == "false"
                                && entEmployee.EMPLOYEESTATE != "3")//年假
                            {
                                DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                                DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-04-15")).AddYears(1);
                                var qq = from l in dal.GetObjects()
                                         where l.VACATIONTYPE == strVacType
                                         && l.EMPLOYEEID == entEmployee.EMPLOYEEID
                                         && l.EFFICDATE >= currentStartDate && l.TERMINATEDATE <= currentEndDate
                                         select l;
                                if (qq != null && qq.Count() > 0)
                                {
                                    foreach (var item in qq)
                                    {
                                        item.LEFTHOURS = 0;
                                        item.STATUS = 0;
                                        dal.DeleteFromContext(item);
                                    }
                                }
                            }
                            continue;
                        }
                    }

                    if (entLeaveTypeSet.SEXRESTRICT != "2")
                    {
                        if (entLeaveTypeSet.SEXRESTRICT != entEmployee.SEX)
                        {
                            Tracer.Debug(entEmployee.EMPLOYEECNAME + "生成过带薪假" + entLeaveTypeSet.LEAVETYPENAME + "被跳过，性别不符");
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(entLeaveTypeSet.POSTLEVELRESTRICT))
                    {
                        decimal dPostLeavlStrict = decimal.Parse(entLeaveTypeSet.POSTLEVELRESTRICT);
                    }

                    int j = -1;
                    decimal dLeaveDay = 0;
                    string LeaveDayName = string.Empty;
                    if (entFreeLeaves.Count() > 0)
                    {
                        for (int i = 0; i < entFreeLeaves.Count(); i++)
                        {
                            if (entFreeLeaves.ToList()[i].MINIMONTH > dCurWorkAge)
                            {
                                continue;
                            }

                            if (entFreeLeaves.ToList()[i].MAXMONTH < dCurWorkAge)
                            {
                                continue;
                            }

                            dLeaveDay = entFreeLeaves.ToList()[i].LEAVEDAYS.Value;
                            LeaveDayName = entLeaveTypeSet.LEAVETYPENAME;
                            j = i;
                            break;
                        }
                    }
                    else
                    {
                        dLeaveDay = entLeaveTypeSet.MAXDAYS.Value;
                        j = 1;
                    }

                    decimal dAddDays = 0;
                    if (j > -1)
                    {
                        if (entFreeLeaves.Count() > 0)
                        {
                            if (j == 0)
                            {
                                dAddDays = dLeaveDay;
                            }
                            else
                            {
                                dAddDays = dLeaveDay - entFreeLeaves.ToList()[j - 1].LEAVEDAYS.Value;
                            }
                        }
                        else
                        {
                            dAddDays = dLeaveDay;
                        }
                    }

                    if (strVacType == (Convert.ToInt32(Common.LeaveTypeValue.AnnualLeave) + 1).ToString())
                    {
                        GetEmployeeAnnualLeaveDays(entAttSolAsign.ATTENDANCESOLUTIONASIGNID
                            , entEmployee.EMPLOYEEID, dtEntryDate
                            , entLeaveTypeSet.MAXDAYS.Value,
                            ref dAddDays, entAttSolAsign.OWNERCOMPANYID);
                    }

                    string strNumOfDecDefault = "0.5";
                    dAddDays = RoundOff(dAddDays, strNumOfDecDefault, 1);
                    dLeaveDay = RoundOff(dLeaveDay, strNumOfDecDefault, 1);
                    if (dAddDays == 0)
                    {
                        Tracer.Debug(entEmployee.EMPLOYEECNAME + "生成过带薪假" + entLeaveTypeSet.LEAVETYPENAME + "被跳过，生成的天数为0天");

                        if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString() && isHangXing == "false")//年假
                        {
                            DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-04-15")).AddYears(1);
                            var qq = from l in dal.GetObjects()
                                     where l.VACATIONTYPE == strVacType
                                     && l.EMPLOYEEID == entEmployee.EMPLOYEEID
                                     && l.EFFICDATE >= currentStartDate && l.TERMINATEDATE <= currentEndDate
                                     select l;
                            if (qq != null && qq.Count() > 0)
                            {
                                foreach (var item in qq)
                                {
                                    item.LEFTHOURS = 0;
                                    item.STATUS = 0;
                                    dal.DeleteFromContext(item);
                                }
                            }
                        }

                        continue;
                    }

                    DateTime dtEfficDate = DateTime.Parse(DateTime.Now.ToString("yyyy") + "-01-01");
                    DateTime dtTerminateDate = DateTime.Parse(DateTime.Now.ToString("yyyy") + "-12-31");

                    if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString() && isHangXing == "false")
                    {
                        //年假的过期时间为第二年：04-15
                        dtTerminateDate = DateTime.Parse(DateTime.Now.ToString("yyyy-04-15")).AddYears(1);
                    }
                    else
                    {
                        if (dtTerminateDate > entAttSolAsign.ENDDATE)
                        {
                            dtTerminateDate = entAttSolAsign.ENDDATE.Value;
                        }
                    }

                    //产前检查假
                    if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.PrenatalExamLeaveDay.GetHashCode().ToString())
                    {
                        dAddDays = 1;
                        dtEfficDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
                        dtTerminateDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1);
                    }

                    var q = from l in dal.GetObjects()
                            where l.VACATIONTYPE == strVacType
                            && l.OWNERCOMPANYID == entAttSolAsign.OWNERCOMPANYID
                            && l.EMPLOYEEID == entEmployee.EMPLOYEEID
                            && l.LEAVETYPESETID == entAttendFreeLeave.T_HR_LEAVETYPESET.LEAVETYPESETID
                            select l;

                    if (q.Count() <= 0)
                    {
                        DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                        DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-12-31"));
                        if (isHangXing == "false")
                        {
                            currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-04-15")).AddYears(1);
                        }
                        string qstrVactionType = Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString();
                        if (entEntryEarly != null)
                        {
                            q = from l in dal.GetObjects()
                                where l.VACATIONTYPE == qstrVactionType
                                && l.OWNERCOMPANYID == entEntryEarly.OWNERCOMPANYID
                                && l.EMPLOYEEID == entEmployee.EMPLOYEEID
                                && l.EFFICDATE >= currentStartDate && l.TERMINATEDATE <= currentEndDate
                                select l;
                            Tracer.Debug("该员工有异动，最早的入职时间：" + entEntryEarly.ONPOSTDATE.Value.ToString()
                                + "q.Count() <= 0,年假：" + qstrVactionType
                                + ",entEntryEarly.OWNERCOMPANYID：" + entEntryEarly.OWNERCOMPANYID
                                + ",entEmployee.EMPLOYEEID:" + entEmployee.EMPLOYEEID
                                + ",currentStartDate:" + currentStartDate
                                + ",currentEndDate:" + currentEndDate);
                        }
                    }

                    q = q.Where(t => t.TERMINATEDATE >= dtEfficDate);

                    if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.RoadLeaveDay.GetHashCode().ToString() ||
                        entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.PrenatalExamLeaveDay.GetHashCode().ToString())
                    {
                        DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
                        DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1);
                        q = q.Where(t => t.EFFICDATE >= currentStartDate && t.TERMINATEDATE <= currentEndDate);
                    }

                    if (q.Count() > 0)
                    {
                        if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString())//年假
                        {
                            DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-12-31"));
                            //集团年假有效期
                            if (isHangXing == "false")
                            {
                                currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-04-15")).AddYears(1);
                            }

                            //周文斌修改年假的生成策略
                            T_HR_EMPLOYEELEVELDAYCOUNT empAnnualDay = q.Where(t => t.EFFICDATE >= currentStartDate && t.TERMINATEDATE <= currentEndDate).FirstOrDefault();
                            if (empAnnualDay != null)
                            {
                                //【次年的年假天数 =（12 - 入职月份）/12 * N (N表示按工龄确定的年假天数)】
                                decimal h1 = Math.Round(dtEntryDate.Month / (decimal)12.0 * empAnnualDay.DAYS.Value * (decimal)averageWorkPerDay, 2);
                                decimal h2 = Math.Round((12 - dtEntryDate.Month) / (decimal)12.0 * AnnualYearDays * (decimal)averageWorkPerDay, 2);
                                decimal preLeftHours = empAnnualDay.LEFTHOURS.HasValue ? empAnnualDay.LEFTHOURS.Value : 0;
                                decimal preTotalHours = empAnnualDay.HOURS.HasValue ? empAnnualDay.HOURS.Value : 0;
                                //若入职时间大于年假的创建时间，并且入职时间在12个月到24个月之间
                                if (dtEntryDate > empAnnualDay.CREATEDATE.Value
                                    && dtEntryDate.AddMonths(12) > Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"))
                                    && dtEntryDate.AddMonths(12) <= dtEntryDate.AddMonths((int)dCurWorkAge))
                                {
                                    empAnnualDay.LEFTHOURS = Math.Round(CurrrentYearAnnualDays * (decimal)averageWorkPerDay, 2);
                                    empAnnualDay.HOURS = Math.Round(AnnualYearDays * (decimal)averageWorkPerDay, 2);
                                    empAnnualDay.DAYS = AnnualYearDays;
                                    empAnnualDay.CREATEDATE = DateTime.Now;
                                    empAnnualDay.UPDATEDATE = DateTime.Now;
                                }
                                else
                                {
                                    if (empAnnualDay.DAYS.Value != AnnualYearDays)
                                    {
                                        empAnnualDay.LEFTHOURS = Math.Round((h1 + h2) - (preTotalHours - preLeftHours), 0);
                                        empAnnualDay.HOURS = Math.Round(AnnualYearDays * (decimal)averageWorkPerDay, 2);
                                        empAnnualDay.DAYS = AnnualYearDays;
                                    }
                                }

                                Tracer.Debug("该员工有年假有变动，变动前的年假天数：" + empAnnualDay.DAYS.Value.ToString()
                                                    + "变动前的年假天数：" + empAnnualDay.DAYS.Value.ToString()
                                                    + "变动前的已经请假天数：" + preLeftHours.ToString()
                                                    + "变动前的已经总时间：" + preTotalHours.ToString()
                                                    + "变动后的年假天数：" + AnnualYearDays.ToString()
                                                    + "变动后的可用时长：" + empAnnualDay.LEFTHOURS.ToString());

                                empAnnualDay.LEAVETYPESETID = entLeaveTypeSet.LEAVETYPESETID;
                                empAnnualDay.OWNERDEPARTMENTID = entEmployee.OWNERDEPARTMENTID;
                                empAnnualDay.OWNERCOMPANYID = entEmployee.OWNERCOMPANYID;//避免用户调岗，换了公司
                                empAnnualDay.OWNERPOSTID = entEmployee.OWNERPOSTID;//避免用户调岗，换了公司
                                dal.UpdateFromContext(empAnnualDay);

                            }

                            //清空年假，重新生成
                            //dal.Delete(q.FirstOrDefault());
                        }
                        else if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.MaternityLeaveDay.GetHashCode().ToString())
                        {
                            DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-12-31"));
                            T_HR_EMPLOYEELEVELDAYCOUNT empMaternityDay = q.Where(t => t.EFFICDATE >= currentStartDate && t.TERMINATEDATE <= currentEndDate).FirstOrDefault();
                            if (empMaternityDay != null)
                            {
                                List<T_HR_FREELEAVEDAYSET> freeLeaveTypeSetList = new List<T_HR_FREELEAVEDAYSET>();
                                freeLeaveTypeSetList = dal.GetObjects<T_HR_FREELEAVEDAYSET>().Include("T_HR_LEAVETYPESET").Where(t => t.T_HR_LEAVETYPESET.LEAVETYPESETID == entLeaveTypeSet.LEAVETYPESETID
                                    && t.OWNERCOMPANYID == entEmployee.OWNERCOMPANYID).ToList();

                                if (freeLeaveTypeSetList != null && freeLeaveTypeSetList.Count > 0)
                                {
                                    dAddDays = (decimal)freeLeaveTypeSetList.Max(t => t.LEAVEDAYS);
                                }
                                else if (entLeaveTypeSet.MAXDAYS != null)
                                {
                                    dAddDays = (decimal)entLeaveTypeSet.MAXDAYS;
                                }

                                decimal dUsedHours = (empMaternityDay.HOURS.HasValue ? empMaternityDay.HOURS.Value : 0) - (empMaternityDay.LEFTHOURS.HasValue ? empMaternityDay.LEFTHOURS.Value : 0);

                                empMaternityDay.DAYS = dAddDays;
                                empMaternityDay.LEFTHOURS = Math.Round(dAddDays * Convert.ToDecimal(averageWorkPerDay), 2) - dUsedHours;
                                empMaternityDay.HOURS = Math.Round(dAddDays * Convert.ToDecimal(averageWorkPerDay), 2);
                                dal.UpdateFromContext(empMaternityDay);

                            }
                            else
                            {
                                Tracer.Debug(entEmployee.EMPLOYEECNAME
                                    + "今年已生成过带薪假：" + entLeaveTypeSet.LEAVETYPENAME
                                    + " 总天数：" + q.FirstOrDefault().DAYS
                                    + " 生效日期" + q.FirstOrDefault().EFFICDATE
                                    + " 终止日期" + q.FirstOrDefault().TERMINATEDATE);
                                continue;
                            }

                        }
                        else
                        {
                            Tracer.Debug(entEmployee.EMPLOYEECNAME
                                + "今年已生成过带薪假：" + entLeaveTypeSet.LEAVETYPENAME
                                + " 总天数：" + q.FirstOrDefault().DAYS
                                + " 生效日期" + q.FirstOrDefault().EFFICDATE
                                + " 终止日期" + q.FirstOrDefault().TERMINATEDATE);
                            continue;
                        }
                    }
                    else
                    {
                        if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString())//年假
                        {
                            dAddDays = CurrrentYearAnnualDays;
                        }
                        else if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.MaternityLeaveDay.GetHashCode().ToString())
                        {
                            DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-12-31"));
                            T_HR_EMPLOYEELEVELDAYCOUNT empMaternityDay = q.Where(t => t.EFFICDATE >= currentStartDate && t.TERMINATEDATE <= currentEndDate).FirstOrDefault();
                            if (empMaternityDay == null)
                            {
                                List<T_HR_FREELEAVEDAYSET> freeLeaveTypeSetList = new List<T_HR_FREELEAVEDAYSET>();
                                freeLeaveTypeSetList = dal.GetObjects<T_HR_FREELEAVEDAYSET>().Include("T_HR_LEAVETYPESET").Where(t => t.T_HR_LEAVETYPESET.LEAVETYPESETID == entLeaveTypeSet.LEAVETYPESETID
                                    && t.OWNERCOMPANYID == entEmployee.OWNERCOMPANYID).ToList();

                                if (freeLeaveTypeSetList != null && freeLeaveTypeSetList.Count > 0)
                                {
                                    dAddDays = (decimal)freeLeaveTypeSetList.Max(t => t.LEAVEDAYS);
                                }
                                else if (entLeaveTypeSet.MAXDAYS != null)
                                {
                                    dAddDays = (decimal)entLeaveTypeSet.MAXDAYS;
                                }
                            }
                            else
                            {
                                Tracer.Debug(entEmployee.EMPLOYEECNAME
                                    + "今年已生成过带薪假：" + entLeaveTypeSet.LEAVETYPENAME
                                    + " 总天数：" + q.FirstOrDefault().DAYS
                                    + " 生效日期" + q.FirstOrDefault().EFFICDATE
                                    + " 终止日期" + q.FirstOrDefault().TERMINATEDATE);
                                continue;
                            }

                        }
                        else if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.RoadLeaveDay.GetHashCode().ToString())
                        {
                            DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
                            DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1);

                            //周文斌修改年假的生成策略
                            T_HR_EMPLOYEELEVELDAYCOUNT empRoadDay = q.Where(t => t.EFFICDATE >= currentStartDate && t.TERMINATEDATE <= currentEndDate).FirstOrDefault();

                            if (empRoadDay == null)
                            {
                                //empSickDay.OWNERCOMPANYID = entEmployee.OWNERCOMPANYID;//避免用户调岗，换了公司
                                //empSickDay.OWNERPOSTID = entEmployee.OWNERPOSTID;//避免用户调岗，换了公司
                                //dal.Update(empSickDay);
                                dtTerminateDate = currentEndDate;
                                dAddDays = 3;
                            }
                            else
                            {
                                Tracer.Debug(entEmployee.EMPLOYEECNAME
                                    + "今年已生成过带薪假：" + entLeaveTypeSet.LEAVETYPENAME
                                    + " 总天数：" + q.FirstOrDefault().DAYS
                                    + " 生效日期" + q.FirstOrDefault().EFFICDATE
                                    + " 终止日期" + q.FirstOrDefault().TERMINATEDATE);
                                continue;
                            }
                        }
                    }


                    if (entEmployee.MARRIAGE == "1" && strVacType == Enums.LeaveVacationType.MarriageLeaveDay.GetHashCode().ToString())
                    {
                        dAddDays = 0;
                    }

                    #region 三八五四节日处理
                    //员工生日
                    DateTime birthDate = (DateTime)entEmployee.BIRTHDAY;
                    DateTime dtYouth = new DateTime();
                    //五四
                    if (strVacType == Enums.LeaveVacationType.YouthLeaveDay.GetHashCode().ToString())
                    {
                        DateTime.TryParse(DateTime.Now.Year.ToString() + "-05-04", out dtYouth);
                        //年龄小于等于28岁
                        if (birthDate.AddYears(28) <= dtYouth)
                        {
                            continue;
                        }
                    }
                    //三八
                    if (strVacType == Enums.LeaveVacationType.WomenDay.GetHashCode().ToString())
                    {
                        DateTime.TryParse(DateTime.Now.Year.ToString() + "-03-08", out dtYouth);
                    }

                    if (entAttSolAsign.T_HR_ATTENDANCESOLUTION != null)
                    {
                        if (strVacType == "12" || strVacType == "13")
                        {
                            //三八五四是否进行了延期设置
                            string strExtend = string.Empty;
                            decimal dbDays = 0;
                            if (string.IsNullOrEmpty(entAttSolAsign.T_HR_ATTENDANCESOLUTION.YOUTHEXTEND))
                            {
                                strExtend = "0";
                            }
                            else
                            {
                                strExtend = entAttSolAsign.T_HR_ATTENDANCESOLUTION.YOUTHEXTEND;
                            }
                            if (strExtend == "1")
                            {
                                //延期天数
                                dbDays = (decimal)entAttSolAsign.T_HR_ATTENDANCESOLUTION.YOUTHEXTENDVALUE;
                            }
                            if (dbDays > 0)
                            {
                                dtEfficDate = dtYouth;
                                dtStart = dtYouth;
                                dtTerminateDate = dtYouth.AddDays((double)dbDays);
                            }
                            else
                            {
                                //不延长则是当天
                                dtEfficDate = dtYouth;
                                dtStart = dtYouth;
                                dtTerminateDate = dtYouth;
                            }
                            if (entLeaveTypeSet.MAXDAYS != null)
                            {
                                dAddDays = (decimal)entLeaveTypeSet.MAXDAYS;
                            }
                        }
                    }
                    #endregion
                    T_HR_EMPLOYEELEVELDAYCOUNT entAdd = new T_HR_EMPLOYEELEVELDAYCOUNT();
                    entAdd.RECORDID = System.Guid.NewGuid().ToString().ToUpper();

                    if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString())
                    {
                        entAdd.DAYS = AnnualYearDays;
                    }
                    else
                    {
                        entAdd.DAYS = dAddDays;
                    }//可休假 的 可休天数 即为 新方案 对应 假期标准 的 带薪假设置 的 可休假数(天)

                    if (dtEfficDate <= dtStart)
                    {
                        dtEfficDate = dtStart;
                    }
                    entAdd.EFFICDATE = dtEfficDate;    //可休假 的 生效日期 即为 新方案 的 生效日期
                    entAdd.TERMINATEDATE = dtTerminateDate;  //可休假 的 终止日期 即为 新方案 的 终止日期
                    entAdd.EMPLOYEEID = entEmployee.EMPLOYEEID;
                    entAdd.EMPLOYEENAME = entEmployee.EMPLOYEECNAME;
                    entAdd.EMPLOYEECODE = entEmployee.EMPLOYEECODE;
                    entAdd.LEAVETYPESETID = entLeaveTypeSet.LEAVETYPESETID;
                    entAdd.VACATIONTYPE = strVacType;
                    entAdd.REMARK = string.Empty;

                    if (entLeaveTypeSet.LEAVETYPEVALUE == Enums.LeaveVacationType.SickLeaveDay.GetHashCode().ToString())
                    {//病假限定5天不扣款
                        dAddDays = 5;
                    }
                    //计算假期的小时数
                    entAdd.STATUS = 1;
                    entAdd.LEFTHOURS = Math.Round(dAddDays * Convert.ToDecimal(averageWorkPerDay), 1);
                    entAdd.HOURS = Math.Round((entAdd.DAYS.HasValue ? entAdd.DAYS.Value : 0) * Convert.ToDecimal(averageWorkPerDay), 1);
                    //获取员工当前主岗位
                    var entEmployeePost = from ent in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                          join post in dal.GetObjects<T_HR_POST>() on ent.T_HR_POST.POSTID equals post.POSTID
                                          join dep in dal.GetObjects<T_HR_DEPARTMENT>() on post.T_HR_DEPARTMENT.DEPARTMENTID equals dep.DEPARTMENTID
                                          where ent.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID
                                            && ent.ISAGENCY == "0"
                                            && ent.CHECKSTATE == "2"
                                            && ent.EDITSTATE == "1"
                                          orderby ent.UPDATEDATE descending
                                          select new { ent, post, dep };
                    var ep = entEmployeePost.FirstOrDefault();
                    if (ep != null)
                    {
                        entAdd.OWNERPOSTID = ep.post.POSTID;
                        entAdd.OWNERDEPARTMENTID = ep.dep.DEPARTMENTID;
                        entAdd.OWNERCOMPANYID = ep.post.COMPANYID;
                    }
                    else
                    {
                        entAdd.OWNERPOSTID = entAttSolAsign.OWNERPOSTID;
                        entAdd.OWNERDEPARTMENTID = entAttSolAsign.OWNERDEPARTMENTID;
                        entAdd.OWNERCOMPANYID = entAttSolAsign.OWNERCOMPANYID;
                    }
                    entAdd.OWNERID = entEmployee.EMPLOYEEID;//entAttSolAsign.OWNERID;

                    entAdd.CREATEPOSTID = entAttSolAsign.CREATEPOSTID;
                    entAdd.CREATEDEPARTMENTID = entAttSolAsign.CREATEDEPARTMENTID;
                    entAdd.CREATECOMPANYID = entAttSolAsign.CREATECOMPANYID;
                    entAdd.CREATEUSERID = entEmployee.EMPLOYEEID;//entAttSolAsign.CREATEUSERID;
                    entAdd.CREATEDATE = DateTime.Now;

                    entAdd.UPDATEUSERID = entAttSolAsign.UPDATEUSERID;
                    entAdd.UPDATEDATE = DateTime.Now;
                    //如果是年休假则判断是否已经产生
                    //按员工进行判断 edit by ljx
                    //已经存在的带薪假记录
                    List<T_HR_EMPLOYEELEVELDAYCOUNT> entExists = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                    //每月都有的假期
                    //if (strVacType == Enums.LeaveVacationType.SickLeaveDay.GetHashCode().ToString() || strVacType == Enums.LeaveVacationType.RoadLeaveDay.GetHashCode().ToString())
                    if (strVacType == Enums.LeaveVacationType.RoadLeaveDay.GetHashCode().ToString()
                        || strVacType == Enums.LeaveVacationType.PrenatalExamLeaveDay.GetHashCode().ToString())
                    {
                        entExists = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t =>
                            t.EMPLOYEEID == entEmployee.EMPLOYEEID
                            && t.EFFICDATE.Value.Year == dtStart.Year && t.EFFICDATE.Value.Month == dtStart.Month).ToList();
                    }
                    else
                    {//一年一次的假期
                        if (strVacType == Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString() && isHangXing == "false")
                        {
                            DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-01-01"));
                            DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-04-15")).AddYears(1);

                            entExists = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t =>
                                t.EMPLOYEEID == entEmployee.EMPLOYEEID
                                && t.VACATIONTYPE == strVacType
                                && t.TERMINATEDATE >= currentEndDate).ToList();
                            Tracer.Debug("是否已经存在年假：" + entExists.Count().ToString());
                        }
                        else
                        {
                            entExists = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t =>
                                t.EMPLOYEEID == entEmployee.EMPLOYEEID
                                && t.EFFICDATE.Value.Year == dtStart.Year).ToList();
                        }
                    }


                    var entExit = entExists.Where(s => s.VACATIONTYPE == entAdd.VACATIONTYPE).ToList();
                    if (entExit.Count() == 0)
                    {
                        //将往年的没用的数据清零
                        List<T_HR_EMPLOYEELEVELDAYCOUNT> lastYearVacationList = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                        //if (strVacType == Enums.LeaveVacationType.SickLeaveDay.GetHashCode().ToString() || strVacType == Enums.LeaveVacationType.RoadLeaveDay.GetHashCode().ToString())
                        if (strVacType == Enums.LeaveVacationType.RoadLeaveDay.GetHashCode().ToString())
                        {
                            lastYearVacationList = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t =>
                             t.EMPLOYEEID == entEmployee.EMPLOYEEID
                             && t.EFFICDATE.Value.Year == dtStart.Year && t.EFFICDATE.Value.Month != dtStart.Month && t.VACATIONTYPE == entAdd.VACATIONTYPE).ToList();
                        }
                        else
                        {
                            lastYearVacationList = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t =>
                             t.EMPLOYEEID == entEmployee.EMPLOYEEID
                             && t.EFFICDATE.Value.Year != dtStart.Year && t.VACATIONTYPE == entAdd.VACATIONTYPE).ToList();
                        }
                        Tracer.Debug("过期的假期更新：" + lastYearVacationList.Count().ToString());


                        foreach (var lastYearVacation in lastYearVacationList)
                        {
                            if (strVacType == Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString() && isHangXing == "false")
                            {
                                if (DateTime.Now > lastYearVacation.TERMINATEDATE)
                                {
                                    lastYearVacation.STATUS = 0;
                                    lastYearVacation.LEFTHOURS = 0;
                                    Tracer.Debug("年假的有效期为下一年的04月15日，当前时间已经大于：" + lastYearVacation.TERMINATEDATE.ToString() + "自动过期");
                                }
                            }
                            else
                            {
                                lastYearVacation.STATUS = 0;
                                lastYearVacation.LEFTHOURS = 0;
                            }
                            dal.UpdateFromContext(lastYearVacation);
                        }


                        dal.AddToContext(entAdd);
                        Tracer.Debug("生成员工：" + entAdd.EMPLOYEENAME + " 带薪假:" + entLeaveTypeSet.LEAVETYPENAME
                         + " 总天数：" + entAdd.DAYS + " 生效日期：" + entAdd.EFFICDATE.Value.ToString("yyyy-MM-dd") + "- 终止日期：" + entAdd.TERMINATEDATE.Value.ToString("yyyy-MM-dd"));
                    }
                    else
                    {
                        //dal.AddToContext(entAdd);
                        Tracer.Debug("没有生成员工：" + entAdd.EMPLOYEENAME + " 带薪假:" + entLeaveTypeSet.LEAVETYPENAME);
                    }
                }
                int intDay = dal.SaveContextChanges();
                if (intDay > 0)
                {
                    Tracer.Debug("生成员工：" + entEmployee.EMPLOYEECNAME + " 所有带薪假成功！");
                }
                else
                {
                    Tracer.Debug("生成员工：" + entEmployee.EMPLOYEECNAME + " 所有带薪假,保存成功，记录：" + intDay.ToString());
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(DateTime.Now.ToString() + "生成带薪假失败,考勤方案应用的ID是："
                    + entAttSolAsign.ATTENDANCESOLUTIONASIGNID + ",员工的ID是："
                    + entEmployee.EMPLOYEEID + "。出错的原因是："
                    + ex.ToString() + ",Message:"
                    + ex.Message + ",Source:"
                    + ",StackTrace:" + ex.StackTrace
                    + ",InnerException" + ex.InnerException
                    + ex.Source);
            }
        }

        /// <summary>
        /// 计算员工当前可用年假  weirui年假修改
        /// </summary>
        /// <param name="dtEntryDate"></param>
        /// <param name="dMaxLeaveDays"></param>
        /// <param name="totalDays"></param>
        private void GetEmployeeAnnualLeaveDays(string AttSolAsignId, string EMPLOYEEID, DateTime dtEntryDate, decimal dMaxLeaveDays, ref decimal totalDays, string ownerCompanyid)
        {
            totalDays = 0;
            //DateTime dtYearStartDate = DateTime.Parse(dtEntryDate.ToString("yyyy") + "-1-1");
            DateTime thisYearFirstDay = DateTime.Parse((DateTime.Now.Year.ToString() + "-1-1").ToString());
            //DateTime dtLastYearEndDate = DateTime.Parse(DateTime.Now.AddYears(-1).ToString("yyyy") + "-12-31");
            try
            {
                //现在时间-入职时间=时间差
                TimeSpan tsCurCheck = DateTime.Now.Subtract(dtEntryDate);
                Tracer.Debug("生成员工带薪年假：员工入职时间：" + dtEntryDate.ToString("yyyy-MM-dd"));
                int entryYear = tsCurCheck.Days / 366;
                if (entryYear < 1)//入职日到当前时间，总工作天数不满一年，不计算年假
                {
                    //不满一年，年假=0
                    Tracer.Debug("生成员工带薪年假：员工入职时间：" + dtEntryDate.ToString("yyyy-MM-dd") + " 入职不满一年，不生成年假");
                    totalDays = 0;
                    return;
                }

                #region 获取该员工本年应休假天数
                string dSOCIALSERVICE = string.Empty;

                var emp = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                          where ent.EMPLOYEEID == EMPLOYEEID
                          select ent;
                if (emp.FirstOrDefault() != null)
                {
                    if (emp.FirstOrDefault().SOCIALSERVICEYEAR != null)
                    {
                        dSOCIALSERVICE = emp.FirstOrDefault().SOCIALSERVICEYEAR;
                        Tracer.Debug("生成员工带薪年假：获取的员工档案中员工社保缴交起始时间：" + dSOCIALSERVICE.ToString());
                    }
                }
                //如果员工档案未获取到员工社保缴交日期，从社保档案获取
                if (string.IsNullOrEmpty(dSOCIALSERVICE))
                {

                    var q = (from t in dal.GetObjects<T_HR_PENSIONMASTER>()
                             where t.T_HR_EMPLOYEE.EMPLOYEEID == EMPLOYEEID
                             && t.CHECKSTATE == "2"
                             select t).ToList();
                    //根据员工ID查询社保档案表
                    T_HR_PENSIONMASTER p = q.OrderByDescending(c => c.UPDATEDATE).FirstOrDefault();
                    //社保缴交起始时间

                    if (p != null)
                    {
                        if (string.IsNullOrEmpty(p.SOCIALSERVICEYEAR))
                        {
                            Tracer.Debug("生成员工带薪年假：获取的员工社保档案员工社保缴交起始时间为空");
                            dSOCIALSERVICE = null;
                        }
                        else
                        {
                            dSOCIALSERVICE = p.SOCIALSERVICEYEAR.ToString();
                            Tracer.Debug("生成员工带薪年假：获取的员工社保档案员工社保缴交起始时间：" + dSOCIALSERVICE);
                        }
                    }
                    else
                    {
                        Tracer.Debug("生成员工带薪年假：获取的员工社保档案为空");
                        dSOCIALSERVICE = null;
                    }
                }
                //DateTime DateTime.Now = DateTime.Now;
                TimeSpan needTimeSpan;
                //解决工作开始时间为空的情况，如果为空，那么取入职时间来计算
                if (string.IsNullOrEmpty(dSOCIALSERVICE))
                {
                    //社保缴交时间为空，取入职时间来计算年假
                    needTimeSpan = DateTime.Now.Subtract(dtEntryDate);

                    Tracer.Debug("生成员工带薪年假," + " 计算时间区间：" +
                        dtEntryDate.ToString("yyyy-MM-dd")
                        + "--"
                        + dtEntryDate.ToString("yyyy-MM-dd")
                        + "社保缴交起始时间为空，按员工入职时间计算:入职天数" + needTimeSpan.Days);
                }
                else
                {
                    //社保缴交时间不为空，计算年假方式 当年1月1日-工作开始时间
                    //新的工作时间的计算：工作时间=当年1月1日-工作开始时间
                    needTimeSpan = thisYearFirstDay.Subtract(DateTime.Parse(dSOCIALSERVICE));
                    Tracer.Debug("生成员工带薪年假," + "计算时间区间：" +
                        thisYearFirstDay.ToString("yyyy-MM-dd")
                        + "--"
                        + dSOCIALSERVICE
                        + "按社保缴交起始时间计算:计算天数" + needTimeSpan.Days);
                }
                //得到工作时间
                decimal workTimes = needTimeSpan.Days / 365;
                Tracer.Debug("生成员工带薪年假：计算得出该员工工作年限" + workTimes);
                if (workTimes >= 0 && workTimes < 10)
                {
                    totalDays = 5;
                    Tracer.Debug("生成员工带薪年假：5天");
                    //return;
                }
                else if (workTimes >= 10 && workTimes < 20)
                {
                    totalDays = 10;
                    Tracer.Debug("生成员工带薪年假：10天");
                    //return;
                }
                else if (workTimes >= 20)
                {
                    totalDays = 15;
                    Tracer.Debug("生成员工带薪年假：15天");
                    //return;
                }
                #endregion
                //如果是第二年在职，年假按一下方式计算
                if (entryYear >= 1 && entryYear < 2)
                {
                    if (DateTime.Now.Year == dtEntryDate.Year + 1)
                    {
                        string isForHuNanHangXingSalary = ConfigurationManager.AppSettings["isForHuNanHangXingSalary"];
                        if (isForHuNanHangXingSalary == "true")
                        {
                            //湖南航信年假计算方法
                            DateTime dtNow = DateTime.Now;
                            int intYear = (dtNow - dtEntryDate).Days / 366;
                            //如果当前时间-入职时间>12则年休假为5天
                            if (intYear >= 1)
                            {
                                totalDays = 5;
                            }
                            //totalDays = decimal.Round(totalDays * (12 - dtEntryDate.Month) / 12);
                            Tracer.Debug("航信-员工入职刚满一年：生成年假天数：" + totalDays);
                        }
                        else
                        {
                            //满一年，基础年假为5天(此标准仅适用于中国)
                            totalDays = decimal.Round(totalDays * (12 - dtEntryDate.Month) / 12, 2);
                            Tracer.Debug("员工入职刚满一年：生成年假天数：" + totalDays);
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                Utility.SaveLog(DateTime.Now.ToString() + "计算员工当前可用年假错误,出错的原因是：" + ex.ToString());
            }

        }



        /// <summary>
        /// 四舍五入
        /// </summary>
        /// <param name="dValue">浮点数</param>
        /// <param name="strNumOfDec">小数位数值比较值</param>
        /// <param name="ilength">四舍五入的精度</param>
        /// <returns></returns>
        private decimal RoundOff(decimal dValue, string strNumOfDec, int ilength)
        {
            decimal dRes = 0;
            try
            {
                dRes = decimal.Round(dValue, ilength);

                if (!string.IsNullOrEmpty(strNumOfDec))
                {
                    decimal dNumOfDec = 0, dCheck = 0;
                    decimal.TryParse(strNumOfDec, out dNumOfDec);

                    string[] strlist = dRes.ToString().Split('.');
                    if (strlist.Length == 2)
                    {
                        decimal.TryParse("0." + strlist[1].ToString(), out dCheck);

                        if (dCheck > dNumOfDec)
                        {
                            dRes = decimal.Parse(strlist[0]) + 1;
                        }
                        else
                        {
                            dRes = decimal.Parse(strlist[0]) + dNumOfDec;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

            return dRes;
        }
        #endregion

        #region ----
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询  查询带薪假期明细
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEELEVELDAYDETAILS> EmployeeLeaveDetailPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            IQueryable<T_HR_EMPLOYEELEVELDAYDETAILS> ents = dal.GetObjects<T_HR_EMPLOYEELEVELDAYDETAILS>().Include("T_HR_EMPLOYEELEVELDAYCOUNT");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEELEVELDAYDETAILS>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 根据时间条件，获取员工可休假信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">权限控制人的员工ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工可休假信息</returns>
        public IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> GetEmployeeLevelDayCountRdListByMultSearch(string strOwnerID, string strEmployeeID, string strSortKey, int pageIndex, int pageSize, ref int pageCount, DateTime startTime, DateTime endTime)
        {
            string filterString = "";
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            DateTime? startTimes = startTime;
            DateTime? endTimes = endTime;
            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                filterString = "EMPLOYEEID == @0";
                objArgs.Add(strEmployeeID);
            }

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                filterString = "VACATIONTYPE != @1";
                //objArgs.Add((Convert.ToInt32(Common.LeaveTypeValue.OtherLeave) + 1).ToString());
            }

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEELEVELDAYCOUNT");

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " RECORDID ";
            }

            var ent = from v in dal.GetObjects()
                      select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(filterString))
            {
                ent = ent.Where(filterString, objArgs.ToArray());
            }
            ent = ent.Where(m => m.EFFICDATE >= startTimes);
            ent = ent.Where(m => m.EFFICDATE <= endTimes);

            ent = ent.OrderBy(strOrderBy);

            return Utility.Pager<T_HR_EMPLOYEELEVELDAYCOUNT>(ent, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region "   周文斌添加的方法    "
        /// <summary>
        /// 周文斌添加的方法
        /// QueryEmployeeLeaveDayCount
        /// </summary>
        /// <param name="strOwnerID"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEELEVELDAYCOUNT> QueryEmployeeLeaveDayCount(EmployeeVacationDayCountRequest request)
        {
            //查询条件拼接 去掉 where


            string filterString = "  STATUS == 1 ";
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            DateTime startTimes = request.StartDate;
            DateTime endTimes = request.EndDate;

            T_HR_EMPLOYEELEVELDAYCOUNT entity = new T_HR_EMPLOYEELEVELDAYCOUNT();

            //if (request.StartDate != null)
            //{
            //    filterString += " and EFFICDATE >=@" + objArgs.Count.ToString();
            //    objArgs.Add(request.StartDate);
            //}

            if (request.EndDate != null)
            {
                filterString += " and TERMINATEDATE >=@" + objArgs.Count.ToString();
                objArgs.Add(request.StartDate);
            }

            SetOrganizationFilter(ref filterString, ref objArgs, request.OwnerID, "T_HR_EMPLOYEELEVELDAYCOUNT");

            var ent = from v in dal.GetObjects()
                      select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(filterString))
            {
                ent = ent.Where(filterString, objArgs.ToArray());
            }

            List<T_HR_EMPLOYEELEVELDAYCOUNT> list = ent != null ? ent.ToList() : new List<T_HR_EMPLOYEELEVELDAYCOUNT>();

            if (!string.IsNullOrEmpty(request.EmployeeName))
            {
                list = list.Where(t => t.EMPLOYEENAME.Contains(request.EmployeeName)).ToList();
            }

            return list;
        }

        /// <summary>
        /// 周文斌添加，2014-07-31
        /// 用于定时任务调用，使调休假过期
        /// </summary>
        /// <param name="OvertimeRecordID"></param>
        /// <returns></returns>
        public int ExpireOvertimeVacation(string LeaveDayCountRecordID)
        {
            int effect = 0;
            try
            {
                SMT.Foundation.Log.Tracer.Debug(
                    "From：定时任务调用，更新过期的调休假开始"
                    + " ，Function:ExpireOvertimeVacation"
                    + " ，Parameters:" + LeaveDayCountRecordID);
                T_HR_EMPLOYEELEVELDAYCOUNT entity = dal.GetObjects().Where(t => t.RECORDID == LeaveDayCountRecordID || t.REMARK == LeaveDayCountRecordID).FirstOrDefault();
                if (entity != null)
                {
                    entity.STATUS = 0;
                    entity.UPDATEDATE = DateTime.Now;
                    effect = dal.Update(entity);
                }

                T_HR_EMPLOYEEOVERTIMERECORD ot = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == LeaveDayCountRecordID).FirstOrDefault();
                if (ot != null)
                {
                    ot.STATUS = 2;//系统自动过期
                    ot.UPDATEDATE = DateTime.Now;
                    dal.Update(ot);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(
                    "From：定时任务调用，更新过期的调休假出错"
                    + " ，Function:ExpireOvertimeVacation"
                    + " ，Error:" + ex.Message
                    + " ，Source:" + ex.Source
                    + " ，Parameters:" + LeaveDayCountRecordID);
            }
            return effect;
        }

        public int SendMailForExpiredOvertimeRecord(string LeaveDayCountRecordID)
        {
            int effect = 0;
            try
            {
                SMT.Foundation.Log.Tracer.Debug(
                    "From：定时任务调用，加班假期过期提醒开始"
                    + " ，Function:SendMailForExpiredOvertimeRecord"
                    + " ，Parameters:" + LeaveDayCountRecordID);
                T_HR_EMPLOYEELEVELDAYCOUNT entity = dal.GetObjects().Where(t => t.RECORDID == LeaveDayCountRecordID || t.REMARK == LeaveDayCountRecordID).FirstOrDefault();

                if (entity != null)
                {
                    T_HR_EMPLOYEE employee = dal.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEEID == entity.EMPLOYEEID).FirstOrDefault();
                    EngineService engine = new EngineService();

                    SMT.SaaS.Services.EngineWS.MailParams[] mps = new SaaS.Services.EngineWS.MailParams[1];

                    if (employee != null)
                    {
                        SMT.SaaS.Services.EngineWS.MailParams mp = new SaaS.Services.EngineWS.MailParams();
                        mp.ReceiveUserMail = employee.EMAIL;
                        mp.MailTitle = "加班假期过期提醒";
                        //string.Format("加班假期过期提醒:亲爱的{0}:\r\n，你在{1}的加班假期即将过期，还剩余{2}小时，过期时间为：{3}，请及时处理。",
                        //employee.EMPLOYEECNAME,
                        //entity.EFFICDATE.Value.ToString("yyyy年MM月dd日"),
                        //entity.LEFTHOURS.ToString(),
                        //entity.TERMINATEDATE.Value.ToString("yyyy年MM月dd日"));

                        mp.MailContent = string.Format("亲爱的{0}:\r\n，你在{1}的加班假期即将过期，还剩余{2}小时，过期时间为：{3}，请及时处理。",
                            employee.EMPLOYEECNAME,
                            entity.EFFICDATE.Value.ToString("yyyy年MM月dd日"),
                            entity.LEFTHOURS.ToString(),
                            entity.TERMINATEDATE.Value.ToString("yyyy年MM月dd日"));
                        mps[0] = mp;
                        engine.SendMail(mps);
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(
                    "From：定时任务调用，发送过期的调休假提醒邮件出错"
                    + " ，Function:SendMailForExpiredOvertimeRecord"
                    + " ，Error:" + ex.Message
                    + " ，Source:" + ex.Source
                    + " ，Parameters:" + LeaveDayCountRecordID);
            }
            return effect;


        }

        #endregion
        #region "  梁杰文添加的方法  "
        /// <summary>
        /// 查询员工可用加班数据
        /// T_HR_EMPLOYEELEVELDAYCOUNT
        /// </summary>
        /// <param name="PageIndex">第几页</param>
        /// <param name="PageSize">每页显示的数据条数</param>
        /// <param name="Sort">排序字段</param>
        /// <param name="FilterString">数据搜索条件</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="strOwenerID">用户ID</param>
        /// <param name="PageCount">数据共有多少页</param>
        /// <param name="Type">类型：OverTime表加班，Leave表调休</param>
        /// <returns>根据类型返回员工可用加班数据</returns>
        public List<EmployeeAlreadyLeave> QueryEmployeeLeaveDayCount(int PageIndex, int PageSize, string Sort, string FilterString, List<object> parameters, string strOwenerID, out int PageCount)
        {
            try
            {
                IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> otList = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>();
                //过滤当前用户能查看的数据
                this.SetOrganizationFilter(ref FilterString, ref parameters, strOwenerID, "T_HR_EMPLOYEELEVELDAYCOUNT");
                //提交查询
                otList = otList.Where(FilterString, parameters.ToArray());
                IQueryable<EmployeeAlreadyLeave> eaList = from a in otList
                                                          select new EmployeeAlreadyLeave
                                                         {
                                                             RECORDID = a.RECORDID,
                                                             EFFICDATE = a.EFFICDATE,
                                                             EMPLOYEECODE = a.EMPLOYEECODE,
                                                             EMPLOYEENAME = a.EMPLOYEENAME,
                                                             EMPLOYEEID = a.EMPLOYEEID,
                                                             HOURS = a.HOURS,
                                                             LEFTHOURS = a.LEFTHOURS,
                                                             OVERTIMERECORDID = a.RECORDID,
                                                             TERMINATEDATE = a.TERMINATEDATE
                                                         };
                //结果排序
                eaList = eaList.OrderBy(Sort);
                int refPageCount = 0;
                eaList = Utility.Pager<EmployeeAlreadyLeave>(eaList, PageIndex, PageSize, ref refPageCount);
                PageCount = refPageCount;
                return eaList.ToList();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取员工可用加班数据出错,Function:QueryEmployeeLeaveDayCount," + ex.ToString());
                PageCount = 0;
                return null;
            }
        }
        /// <summary>
        /// 查询员工可用加班或已用调休数据
        /// T_HR_EMPLOYEELEVELDAYCOUNT
        /// </summary>
        /// <param name="PageIndex">第几页</param>
        /// <param name="PageSize">每页显示的数据条数</param>
        /// <param name="Sort">排序字段</param>
        /// <param name="FilterString">数据搜索条件</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="strOwenerID">用户ID</param>
        /// <param name="PageCount">数据共有多少页</param>
        /// <param name="Type">类型：OverTime表加班，Leave表调休</param>
        /// <returns>根据类型返回员工可用加或已用调休班数据</returns>
        public List<EmployeeAlreadyLeave> QueryEmployeeLeaveDayCount(int PageIndex, int PageSize, string Sort, string FilterString, List<object> parameters, string strOwenerID, out int PageCount, string Type)
        {
            if (Type == "OverTime")
                return QueryEmployeeLeaveDayCount(PageIndex, PageSize, Sort, FilterString, parameters, strOwenerID, out PageCount);
            try
            {
                IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> ListDayCount = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>();
                IQueryable<T_HR_LEAVEREFEROT> listLeave = dal.GetObjects<T_HR_LEAVEREFEROT>();
                //过滤当前用户能查看的数据                
                List<object> PramFirst = new List<object>();
                string FilterFirst = " VACATIONTYPE==@" + PramFirst.Count;
                string str = "1";
                PramFirst.Add(str);

                ListDayCount = ListDayCount.Where(FilterFirst, PramFirst.ToArray());
                listLeave = listLeave.Where(FilterString, parameters.ToArray());
                IQueryable<EmployeeAlreadyLeave> eaList = from a in ListDayCount
                                                          join b in listLeave on a.RECORDID equals b.OVERTIME_RECORDID
                                                          select new EmployeeAlreadyLeave
                                                          {
                                                              EFFICDATE = a.EFFICDATE,
                                                              EMPLOYEECODE = a.EMPLOYEECODE,
                                                              EMPLOYEEID = a.EMPLOYEEID,
                                                              EMPLOYEENAME = a.EMPLOYEENAME,
                                                              HOURS = a.HOURS,
                                                              LEFTHOURS = b.LEAVE_TOTAL_HOURS,
                                                              OVERTIMERECORDID = a.RECORDID,
                                                              RECORDID = b.RECORDID,
                                                              TERMINATEDATE = a.TERMINATEDATE
                                                          };
                eaList = eaList.OrderBy(Sort);
                int refPageCount = 0;
                eaList = Utility.Pager<EmployeeAlreadyLeave>(eaList, PageIndex, PageSize, ref refPageCount);
                PageCount = refPageCount;
                return eaList.ToList();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取员工已用调休表列表数据出错,Function:QueryEmployeeLeaveDayCount,T_HR_LEAVEREFEROT" + ex.ToString());
                PageCount = 0;
                return null;
            }

        }
        #endregion

    }
}
