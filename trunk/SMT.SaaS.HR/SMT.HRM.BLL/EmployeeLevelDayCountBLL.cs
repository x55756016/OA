
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
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;

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
                      where a.EMPLOYEEID == entEmployeeView.EMPLOYEEID && a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID
                      select a;

            if (entLeaveTypeSet.LEAVETYPEVALUE != (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
            {
                ems = from e in ems
                      where e.EFFICDATE >= dtYearStart && e.TERMINATEDATE <= dtYearEnd
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

            var dets = from d in dal.GetObjects<T_HR_EMPLOYEELEVELDAYDETAILS>().Include("T_HR_EMPLOYEELEVELDAYCOUNT")
                       join m in ems on d.T_HR_EMPLOYEELEVELDAYCOUNT.RECORDID equals m.RECORDID
                       where d.EFFICDATE >= dtStartDate && d.EFFICDATE <= m.TERMINATEDATE
                       orderby d.EFFICDATE descending
                       select d;

            decimal dLeaveYearDays = 0, dLeaveMonthDays = 0, dAdjLevPaidDays = 0;       //同类型假本年已用天数，同类型假本月已用天数，冲减带薪假已用天数
            EmployeeLeaveRecordBLL bllLeaveRecord = new EmployeeLeaveRecordBLL();
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

            //员工请假记录
            var ey = from e in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET")
                     where e.LEAVERECORDID != strLeaveRecordId && e.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveSetId && e.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && e.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                     && (e.CHECKSTATE == strCheckStateAri || e.CHECKSTATE == strCheckStateArd) && e.ENDDATETIME < dtLeaveStartTime
                     select e;

            //员工可休假记录(只针对非调休假计算使用)
            var el = from a in dal.GetObjects()
                     where a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && a.EMPLOYEEID == entEmployeeView.EMPLOYEEID && a.LEAVETYPESETID == strLeaveSetId && a.TERMINATEDATE >= dtYearStart
                     select a;

            //已失效的可休假记录，只针对调休假计算使用
            var oel = from a in dal.GetObjects()
                      where a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && a.EMPLOYEEID == entEmployeeView.EMPLOYEEID && a.LEAVETYPESETID == strLeaveSetId && a.TERMINATEDATE < dtLeaveStartTime
                      select a;

            //有效的可休假记录，只针对调休假计算使用
            var cel = from a in dal.GetObjects()
                      where a.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && a.EMPLOYEEID == entEmployeeView.EMPLOYEEID && a.LEAVETYPESETID == strLeaveSetId && a.EFFICDATE <= dtLeaveEndTime && a.TERMINATEDATE >= dtLeaveEndTime
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

            //员工可休假时长
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
            }

            dLeaveDays += dInvalidAdjustDays.Value;

            if (dLeaveDays > dFreeDays * entAttendSol.WORKTIMEPERDAY.Value)
            {
                dLeaveDays = dFreeDays * entAttendSol.WORKTIMEPERDAY.Value;
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

            dtInvaalid = entInvalidFree.TERMINATEDATE;

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

                dalEmployeeleveldaycount.Add(entTemp);

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
                           where n.ASSIGNEDOBJECTTYPE == strOrgType && n.OWNERCOMPANYID == strOrgId && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                           orderby n.ASSIGNEDOBJECTTYPE ascending
                           select n;

                if (ents == null)
                {
                    Tracer.Debug("生成公司带薪假失败，获取考勤方案为NULL，机构类型： " + strOrgType +" 机构id："+ strOrgId);
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
                    Tracer.Debug("生成带薪假失败，获取的员工或该员工的考勤方案为空或已失效，获取的时间：" + dtCur.ToString("yyyy-MM-dd") +"员工id："+ strOrgId);
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
                string strAssignObjectType = Convert.ToInt32(AssignObjectType.Company).ToString();

                var ents = from n in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                           where n.ASSIGNEDOBJECTTYPE == strAssignObjectType && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
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
            DateTime dtCheck = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
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
        /// 根据考勤方案应用ID，计算员工的带薪假天数，并更新计算结果到可休假表
        /// </summary>
        /// <param name="entAttSolAsign">考勤方案应用实体</param>
        /// <param name="entEmployee">员工实体</param>
        /// <param name="strOperationType">保存到数据库的操作方式</param>
        public void CalculateEmployeeLevelDayCount(T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign, T_HR_EMPLOYEE entEmployee, string strOperationType)
        {
            try
            {
                DateTime dtStart = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
                if (dtStart < entAttSolAsign.STARTDATE.Value)
                {
                    dtStart = entAttSolAsign.STARTDATE.Value;
                }

                DateTime dtEnd = dtStart.AddMonths(1).AddDays(-1);

                decimal dCurWorkAge = 0, dEmployeePostLevel = 0;

                var qw = from n in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")
                         where n.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID
                         select n;

                T_HR_EMPLOYEEENTRY entEntry = qw.FirstOrDefault();
                DateTime dtEntryDate = entEntry.ENTRYDATE.Value;
                TimeSpan tsWorkTime = DateTime.Now.Subtract(dtEntryDate);

                dCurWorkAge = decimal.Round(tsWorkTime.Days / 30, 0);

                //获取员工的详细信息
                var qp = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                         where ep.T_HR_EMPLOYEE.EMPLOYEEID == entEmployee.EMPLOYEEID && ep.T_HR_EMPLOYEE.OWNERCOMPANYID == entAttSolAsign.OWNERCOMPANYID
                         && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1" && ep.ISAGENCY == "0"
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


                //获取考勤方案关联的假期标准(只为带薪假的)
                AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL();
                IQueryable<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves = bllAttendFreeLeave.GetAttendFreeLeaveByAttendSolID(entAttSolAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);

                if (entAttendFreeLeaves == null)
                {
                    Tracer.Debug(entEmployee.EMPLOYEECNAME + "无法生成生成过带薪假" + " 通过考勤方案没有获取到关联的假期标准(只为带薪假的)，方案id：" + entAttSolAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID); 
                    return;
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
                        if (entEmployee.EMPLOYEESTATE != "1")
                        {
                            Tracer.Debug(entEmployee.EMPLOYEECNAME + "生成过带薪假" + entLeaveTypeSet.LEAVETYPENAME + "被跳过，员工状态EMPLOYEESTATE不为1");                            
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
                            ,entEmployee.EMPLOYEEID, dtEntryDate
                            ,entLeaveTypeSet.MAXDAYS.Value, 
                            ref dAddDays, entAttSolAsign.OWNERCOMPANYID);
                    }

                    string strNumOfDecDefault = "0.5";
                    dAddDays = RoundOff(dAddDays, strNumOfDecDefault, 1);
                    dLeaveDay = RoundOff(dLeaveDay, strNumOfDecDefault, 1);
                    if (dAddDays == 0)
                    {
                        Tracer.Debug(entEmployee.EMPLOYEECNAME + "生成过带薪假" + entLeaveTypeSet.LEAVETYPENAME + "被跳过，生成的天数为0天");
                        continue;
                    }

                    DateTime dtTerminateDate = DateTime.Parse(DateTime.Now.ToString("yyyy") + "-12-31");
                    if (dtTerminateDate > entAttSolAsign.ENDDATE)
                    {
                        dtTerminateDate = entAttSolAsign.ENDDATE.Value;
                    }

                    DateTime dtEfficDate = DateTime.Parse(DateTime.Now.ToString("yyyy") + "-1-1");

                    var q = from l in dal.GetObjects()
                            where l.VACATIONTYPE == strVacType && l.OWNERCOMPANYID == entAttSolAsign.OWNERCOMPANYID
                                && l.EMPLOYEEID == entEmployee.EMPLOYEEID && l.OWNERCOMPANYID == entAttSolAsign.OWNERCOMPANYID
                                && l.LEAVETYPESETID == entAttendFreeLeave.T_HR_LEAVETYPESET.LEAVETYPESETID
                            select l;

                    q = q.Where(t => t.TERMINATEDATE >= dtEfficDate);

                    if (q.Count() > 0)
                    {
                        if (entLeaveTypeSet.LEAVETYPEVALUE == "4")//年假
                        {
                            //清空年假，重新生成
                            dal.Delete(q.FirstOrDefault());
                        }
                        else
                        {
                            Tracer.Debug(entEmployee.EMPLOYEECNAME + "今年已生成过带薪假：" + entLeaveTypeSet.LEAVETYPENAME + " 总天数：" + q.FirstOrDefault().DAYS
                                + " 生效日期" + q.FirstOrDefault().EFFICDATE + " 终止日期" + q.FirstOrDefault().TERMINATEDATE);
                            continue;
                        }
                    }

                    T_HR_EMPLOYEELEVELDAYCOUNT entAdd = new T_HR_EMPLOYEELEVELDAYCOUNT();
                    entAdd.RECORDID = System.Guid.NewGuid().ToString().ToUpper();
                    entAdd.DAYS = dAddDays;                  //可休假 的 可休天数 即为 新方案 对应 假期标准 的 带薪假设置 的 可休假数(天)

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
                    entAdd.OWNERPOSTID = entAttSolAsign.OWNERPOSTID;
                    entAdd.OWNERDEPARTMENTID = entAttSolAsign.OWNERDEPARTMENTID;
                    entAdd.OWNERCOMPANYID = entAttSolAsign.OWNERCOMPANYID;
                    entAdd.OWNERID = entAttSolAsign.OWNERID;

                    entAdd.CREATEPOSTID = entAttSolAsign.CREATEPOSTID;
                    entAdd.CREATEDEPARTMENTID = entAttSolAsign.CREATEDEPARTMENTID;
                    entAdd.CREATECOMPANYID = entAttSolAsign.CREATECOMPANYID;
                    entAdd.CREATEUSERID = entAttSolAsign.CREATEUSERID;
                    entAdd.CREATEDATE = DateTime.Now;

                    entAdd.UPDATEUSERID = entAttSolAsign.UPDATEUSERID;
                    entAdd.UPDATEDATE = DateTime.Now;

                    //if (strOperationType == "0")
                    //{
                    //    bllLevelDayCount.AddEmployeeLevelDayCount(entAdd);
                    //}
                    //else if (strOperationType == "1")
                    //{
                    dal.AddToContext(entAdd);
                    //}
                    Tracer.Debug("生成员工：" + entAdd.EMPLOYEENAME + " 带薪假:" + entLeaveTypeSet.LEAVETYPENAME 
                        +" 总天数："+entAdd.DAYS+ " 生效日期：" + entAdd.EFFICDATE.Value.ToString("yyyy-MM-dd") + "- 终止日期：" + entAdd.TERMINATEDATE.Value.ToString("yyyy-MM-dd"));
                }
                int intDay = dal.SaveContextChanges();
                if (intDay > 0)
                {
                    Tracer.Debug("生成员工：" + entEmployee.EMPLOYEECNAME + " 所有带薪假成功！");
                }
                else
                {
                    Tracer.Debug("生成员工：" + entEmployee.EMPLOYEECNAME + " 所有带薪假保存失败！");
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(DateTime.Now.ToString() + "生成带薪假失败,考勤方案应用的ID是：" + entAttSolAsign.ATTENDANCESOLUTIONASIGNID + ",员工的ID是：" + entEmployee.EMPLOYEEID + "。出错的原因是：" + ex.ToString());
            }
        }

        /// <summary>
        /// 计算员工当前可用年假  weirui年假修改
        /// </summary>
        /// <param name="dtEntryDate"></param>
        /// <param name="dMaxLeaveDays"></param>
        /// <param name="totalDays"></param>
        private void GetEmployeeAnnualLeaveDays(string AttSolAsignId,string EMPLOYEEID, DateTime dtEntryDate, decimal dMaxLeaveDays, ref decimal totalDays, string ownerCompanyid)
        {
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

                var q = (from t in dal.GetObjects<T_HR_PENSIONMASTER>()
                         where t.T_HR_EMPLOYEE.EMPLOYEEID == EMPLOYEEID
                         && t.CHECKSTATE == "2"
                         select t).ToList();
                //根据员工ID查询社保档案表
                T_HR_PENSIONMASTER p = q.OrderByDescending(c => c.UPDATEDATE).FirstOrDefault();
                //社保缴交起始时间
                string dSOCIALSERVICE;
                if (p != null)
                {
                    if (string.IsNullOrEmpty(p.SOCIALSERVICEYEAR))
                    {
                        Tracer.Debug("生成员工带薪年假：获取的员工社保缴交起始时间为空");
                        dSOCIALSERVICE = null;
                    }
                    else
                    {
                        dSOCIALSERVICE = p.SOCIALSERVICEYEAR.ToString();
                        Tracer.Debug("生成员工带薪年假：获取的员工社保缴交起始时间：" + dSOCIALSERVICE);
                    }
                }
                else
                {
                    Tracer.Debug("生成员工带薪年假：获取的员工社保档案为空");
                    dSOCIALSERVICE = null;
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
                        dtEntryDate.ToString("yyyy-MM-dd")
                        + "--"
                        + dtEntryDate.ToString("yyyy-MM-dd")
                        + "按社保缴交起始时间计算:计算天数" + needTimeSpan.Days);
                }
                //得到工作时间
                decimal workTimes = needTimeSpan.Days / 365;
                Tracer.Debug("生成员工带薪年假：计算得出该员工工作年限" + workTimes);
                if (workTimes >= 1 && workTimes < 10)
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
                    //满一年，基础年假为5天(此标准仅适用于中国)
                    totalDays = decimal.Round(totalDays * (12 - dtEntryDate.Month) / 12);                   
                }

                var allday = from ent in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                        join levType in dal.GetObjects<T_HR_LEAVETYPESET>() 
                        on ent.T_HR_LEAVETYPESET.LEAVETYPESETID equals levType.LEAVETYPESETID
                        where ent.EMPLOYEEID == EMPLOYEEID
                        && ent.OWNERCOMPANYID == ownerCompanyid
                        && ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE=="4"//年假
                        && ent.CHECKSTATE=="2"
                        && ent.STARTDATETIME >= thisYearFirstDay
                       select ent;
                if (allday.Count() > 0)
                {
                    try
                    {
                        var AttSolution = from v in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                                          where v.ATTENDANCESOLUTIONASIGNID == AttSolAsignId
                                          select v.T_HR_ATTENDANCESOLUTION;
                        if (AttSolution.Count() > 0)
                        {
                            foreach (var item in allday)
                            {
                                if (item.LEAVEDAYS != null && item.LEAVEDAYS > 0)
                                {
                                    totalDays = totalDays - item.LEAVEDAYS.Value;
                                    Tracer.Debug("生成年假总天数减去已请年假：" + item.LEAVEDAYS.Value+" 天");
                                }
                                if (item.LEAVEHOURS != null && item.LEAVEHOURS > 0)
                                {
                                    totalDays = (totalDays * AttSolution.FirstOrDefault().WORKTIMEPERDAY.Value
                                        - item.LEAVEHOURS.Value) / AttSolution.FirstOrDefault().WORKTIMEPERDAY.Value;
                                    Tracer.Debug("生成年假总天数减去已请年假：" + item.LEAVEHOURS.Value + " 小时");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug("生成带薪年假扣减已请天数异常：" + ex.ToString());
                        totalDays = 0;
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

    }
}