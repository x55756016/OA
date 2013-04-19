/*
 * 文件名：AttendYearlyBalanceBLL.cs
 * 作  用：员工考勤年度结算 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-3-29 13:50:12
 * 修改人：
 * 修改时间：
 */


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

namespace SMT.HRM.BLL
{
    public class AttendYearlyBalanceBLL : BaseBll<T_HR_ATTENDYEARLYBALANCE>
    {
        public AttendYearlyBalanceBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取员工考勤年度结算信息
        /// </summary>
        /// <param name="strYearlyBalanceId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDYEARLYBALANCE GetAttendYearlyBalanceByID(string strYearlyBalanceId)
        {
            if (string.IsNullOrEmpty(strYearlyBalanceId))
            {
                return null;
            }

            AttendYearlyBalanceDAL dalAttendYearlyBalance = new AttendYearlyBalanceDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strYearlyBalanceId))
            {
                strfilter.Append(" YEARLYBALANCEID == @0");
                objArgs.Add(strYearlyBalanceId);
            }

            T_HR_ATTENDYEARLYBALANCE entRd = dalAttendYearlyBalance.GetAttendYearlyBalanceRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据员工ID和考勤结算年份，获取员工年度结算信息
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dBalanceYear">考勤结算年份</param>
        /// <returns>员工年度结算信息</returns>
        public T_HR_ATTENDYEARLYBALANCE GetAttendYearlyBalanceByEmployeeIDAndYear(string strEmployeeID, decimal dBalanceYear)
        {
            if (string.IsNullOrEmpty(strEmployeeID) || dBalanceYear <= 0)
            {
                return null;
            }

            var q = from v in dal.GetObjects()
                    where v.EMPLOYEEID == strEmployeeID && v.BALANCEYEAR == dBalanceYear
                    select v;

            if (q == null)
            {
                return null;            
            }

            if (q.Count() == 0)
            {
                return null;
            }

            return q.FirstOrDefault();
        }

        /// <summary>
        /// 根据条件，获取员工考勤年度结算信息
        /// </summary>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strEmployeeID">员工序号</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>员工考勤年度结算信息</returns>
        public IQueryable<T_HR_ATTENDYEARLYBALANCE> GetAllAttendYearlyBalanceRdListByMultSearch(string strOwnerID, string strEmployeeID, decimal dBalanceYear, string strSortKey)
        {
            AttendYearlyBalanceDAL dalAttendYearlyBalance = new AttendYearlyBalanceDAL();
            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                strfilter.Append(" EMPLOYEEID == @0");
                objArgs.Add(strEmployeeID);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " YEARLYBALANCEID ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDYEARLYBALANCE");


            var q = dalAttendYearlyBalance.GetAttendYearlyBalanceRdListByMultSearch(strOrderBy, dBalanceYear, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取员工考勤年度结算信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strEmployeeID">员工序号</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工考勤年度结算信息</returns>
        public IQueryable<T_HR_ATTENDYEARLYBALANCE> GetAttendYearlyBalanceRdListByMultSearch(string strOwnerID, string strEmployeeID, decimal dBalanceYear,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendYearlyBalanceRdListByMultSearch(strOwnerID, strEmployeeID, dBalanceYear, strSortKey);

            return Utility.Pager<T_HR_ATTENDYEARLYBALANCE>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增员工考勤年度结算信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddYearlyBalance(T_HR_ATTENDYEARLYBALANCE entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;

                AttendYearlyBalanceDAL dalAttendYearlyBalance = new AttendYearlyBalanceDAL();
                flag = dalAttendYearlyBalance.IsExistsRd(entTemp.EMPLOYEEID, entTemp.BALANCEYEAR.Value);

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dalAttendYearlyBalance.Add(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
                Utility.SaveLog(strMsg);
            }

            return strMsg;
        }

        /// <summary>
        /// 修改员工考勤年度结算信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string ModifyYearlyBalance(T_HR_ATTENDYEARLYBALANCE entTemp)
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

                strFilter.Append(" YEARLYBALANCEID == @0");

                objArgs.Add(entTemp.YEARLYBALANCEID);

                AttendYearlyBalanceDAL dalAttendYearlyBalance = new AttendYearlyBalanceDAL();
                flag = dalAttendYearlyBalance.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDYEARLYBALANCE entUpdate = dalAttendYearlyBalance.GetAttendYearlyBalanceRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalAttendYearlyBalance.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
                Utility.SaveLog(strMsg);
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除员工考勤年度结算信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strYearlyBalanceId">主键索引</param>
        /// <returns></returns>
        public string DeleteYearlyBalance(string strYearlyBalanceId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strYearlyBalanceId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" YEARLYBALANCEID == @0");

                objArgs.Add(strYearlyBalanceId);

                AttendYearlyBalanceDAL dalAttendYearlyBalance = new AttendYearlyBalanceDAL();
                flag = dalAttendYearlyBalance.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDYEARLYBALANCE entDel = dalAttendYearlyBalance.GetAttendYearlyBalanceRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalAttendYearlyBalance.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(ex.Message);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }

            return strMsg;
        }

        /// <summary>
        /// 计算指定年份指定公司的考勤年度结算
        /// </summary>
        /// <param name="strCurDateYear">指定年份</param>
        /// <param name="strCompanyID">指定公司ID</param>
        public void CalculateEmployeeAttendanceYearlyByCompanyID(string strCurDateYear, string strCompanyID)
        {
            try
            {
                if (string.IsNullOrEmpty(strCompanyID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strCurDateYear))
                {
                    return;
                }

                decimal dYear = 0;
                bool flag = false;
                flag = decimal.TryParse(strCurDateYear, out dYear);
                if (!flag)
                {
                    return;
                }

                DateTime dtCheck = DateTime.Parse(dYear.ToString() + "-1-1");

                EmployeeBLL bllEmployee = new EmployeeBLL();
                IQueryable<T_HR_EMPLOYEE> entEmployees = bllEmployee.GetEmployeeByCompanyID(strCompanyID, dtCheck);

                if (entEmployees.Count() == 0)
                {
                    return;
                }

                string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();

                CalculateEmployeesAttendYearlyBalance(entEmployees, dYear, strCheckState);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(ex.Message);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }

        }


        /// <summary>
        /// 计算指定年份指定部门的考勤年度结算
        /// </summary>
        /// <param name="strCurDateYear">指定年份</param>
        /// <param name="strDepartmentID">指定部门ID</param>
        public void CalculateEmployeeAttendanceYearlyByDepartmentID(string strCurDateYear, string strDepartmentID)
        {
            try
            {
                if (string.IsNullOrEmpty(strDepartmentID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strCurDateYear))
                {
                    return;
                }

                decimal dYear = 0;
                bool flag = false;
                flag = decimal.TryParse(strCurDateYear, out dYear);
                if (!flag)
                {
                    return;
                }

                EmployeeBLL bllEmployee = new EmployeeBLL();
                IQueryable<T_HR_EMPLOYEE> entEmployees = bllEmployee.GetEmployeeByDepartmentID(strDepartmentID);

                if (entEmployees.Count() == 0)
                {
                    return;
                }

                string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();

                CalculateEmployeesAttendYearlyBalance(entEmployees, dYear, strCheckState);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(ex.Message);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// 计算指定年份指定岗位的考勤年度结算
        /// </summary>
        /// <param name="strCurDateYear">指定年份</param>
        /// <param name="strCompanyID">指定岗位ID</param>
        public void CalculateEmployeeAttendanceYearlyByPostID(string strCurDateYear, string strPostID)
        {
            try
            {
                if (string.IsNullOrEmpty(strPostID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strCurDateYear))
                {
                    return;
                }

                decimal dYear = 0;
                bool flag = false;
                flag = decimal.TryParse(strCurDateYear, out dYear);
                if (!flag)
                {
                    return;
                }

                EmployeeBLL bllEmployee = new EmployeeBLL();
                IQueryable<T_HR_EMPLOYEE> entEmployees = bllEmployee.GetEmployeeByPostID(strPostID);

                if (entEmployees.Count() == 0)
                {
                    return;
                }

                string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();

                CalculateEmployeesAttendYearlyBalance(entEmployees, dYear, strCheckState);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(ex.Message);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// 计算指定年份指定员工的考勤年度结算
        /// </summary>
        /// <param name="strCurDateYear">指定年份</param>
        /// <param name="strCompanyID">指定公司ID</param>
        public void CalculateEmployeeAttendanceYearlyByEmployeeID(string strCurDateYear, string strEmployeeID)
        {
            try
            {
                if (string.IsNullOrEmpty(strEmployeeID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strCurDateYear))
                {
                    return;
                }

                decimal dYear = 0;
                bool flag = false;
                flag = decimal.TryParse(strCurDateYear, out dYear);
                if (!flag)
                {
                    return;
                }

                EmployeeBLL bllEmployee = new EmployeeBLL();
                IQueryable<T_HR_EMPLOYEE> entEmployees = from e in dal.GetObjects<T_HR_EMPLOYEE>()
                                                         where e.EMPLOYEEID == strEmployeeID
                                                         select e;

                if (entEmployees.Count() == 0)
                {
                    return;
                }

                string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();

                CalculateEmployeesAttendYearlyBalance(entEmployees, dYear, strCheckState);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(ex.Message);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }
        }

        #region 私有方法
        /// <summary>
        /// 读取员工指定年份的所有月份结算信息，统计并进行考勤年度结算
        /// </summary>
        /// <param name="entEmployees">员工统计对象(多个员工)</param>
        /// <param name="dYear">结算年份</param>
        /// <param name="strCheckState">月度结算记录的审核状态</param>
        public void CalculateEmployeesAttendYearlyBalance(IQueryable<T_HR_EMPLOYEE> entEmployees, decimal dYear, string strCheckState)
        {
            foreach (T_HR_EMPLOYEE item in entEmployees)
            {
                CalculateSingleEmployeeAttendYearlyBalance(item, dYear, strCheckState);
            }
        }

        /// <summary>
        /// 为单一员工核算指定年份的考勤信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dYear"></param>
        /// <param name="strCheckState"></param>
        public void CalculateSingleEmployeeAttendYearlyBalance(T_HR_EMPLOYEE item, decimal dBalanceYear, string strCheckState)
        {
            try
            {
                if (item == null)
                {
                    return;
                }

                if (dBalanceYear == 0)
                {
                    return;
                }

                DateTime dtBalance = new DateTime();
                string strEmployeeID = item.EMPLOYEEID;

                AttendMonthlyBalanceBLL bllAttendMonthlyBalance = new AttendMonthlyBalanceBLL();
                IQueryable<T_HR_ATTENDMONTHLYBALANCE> entAttendMonthlyBalances = bllAttendMonthlyBalance.GetAttendMonthlyBalanceRdListByEmployeeAndYear(strEmployeeID, dBalanceYear, strCheckState);

                if (entAttendMonthlyBalances == null)
                {
                    return;
                }

                if (entAttendMonthlyBalances.Count() == 0)
                {
                    return;
                }

                int iYear = 0, iMonth = 12, iMonthDay = 0;
                int.TryParse(dBalanceYear.ToString(), out iYear);                               

                iMonthDay = DateTime.DaysInMonth(iYear, iMonth);
                dtBalance = DateTime.Parse(iYear + "-" + iMonth + "-" + iMonthDay);

                bool flag = false;
                T_HR_ATTENDYEARLYBALANCE entYearlyBalance = GetAttendYearlyBalanceByEmployeeIDAndYear(strEmployeeID, dBalanceYear);

                if (entYearlyBalance == null)
                {
                    entYearlyBalance = new T_HR_ATTENDYEARLYBALANCE();
                    entYearlyBalance.YEARLYBALANCEID = System.Guid.NewGuid().ToString().ToUpper();
                }
                else
                {
                    flag = true;
                }

                //基础部分
                entYearlyBalance.EMPLOYEEID = strEmployeeID;
                entYearlyBalance.EMPLOYEECODE = item.EMPLOYEECODE;
                entYearlyBalance.EMPLOYEENAME = item.EMPLOYEECNAME;
                entYearlyBalance.BALANCEYEAR = dBalanceYear;
                entYearlyBalance.BALANCEDATE = DateTime.Now;

                //年假部分
                decimal? dLastAnnualLevelUnusedDays = 0, dAnnualLeaveUsedDays = 0, dAnnualLeaveSumDays = 0, dAnnualLeaveValidDays = 0;
                decimal? dLastLeaveValidDays = 0, dLeaveUsedDays = 0, dLeaveValidDays = 0, dLeaveSumDays = 0;

                CalculateEmployeeLeaveDays(strEmployeeID, dtBalance, ref dLastAnnualLevelUnusedDays, ref dAnnualLeaveUsedDays, ref dAnnualLeaveSumDays, ref dAnnualLeaveValidDays,
                    ref dLastLeaveValidDays, ref dLeaveUsedDays, ref dLeaveValidDays, ref dLeaveSumDays);

                entYearlyBalance.LASTANNUALLEVELUNUSEDDAYS = dLastAnnualLevelUnusedDays;
                entYearlyBalance.ANNUALLEAVEUSEDDAYS = dAnnualLeaveUsedDays;
                entYearlyBalance.ANNUALLEAVESUMDAYS = dAnnualLeaveSumDays;
                entYearlyBalance.ANNUALLEAVEVALIDDAYS = dAnnualLeaveValidDays;
                entYearlyBalance.LASTLEAVEVALIDDAYS = dLastLeaveValidDays;
                entYearlyBalance.LEAVEUSEDDAYS = dLeaveUsedDays;
                entYearlyBalance.LEAVEVALIDDAYS = dLeaveValidDays;
                entYearlyBalance.LEAVESUMDAYS = dLeaveSumDays;

                //考勤异常部分
                decimal? dAbsentDays = 0, dLateDays = 0, dLeaveEarlyDays = 0;//旷工，迟到，早退

                //请假部分
                decimal? dAffairLeaveDays = 0, dSickLeaveDays = 0, dOtherLeaveDays = 0;

                //加班部分
                decimal? dOverTimeTimes = 0;
                decimal? dOverTimeSumHours = 0, dOvertimeSumDays = 0;

                //出勤天数
                decimal? dNeedAttendDays = 0, dRealAttendDays = 0;//应出勤总天数，实际出勤天数

                foreach (T_HR_ATTENDMONTHLYBALANCE entAttendMonthlyBalance in entAttendMonthlyBalances)
                {
                    if (entAttendMonthlyBalance.NEEDATTENDDAYS != null)
                    {
                        dNeedAttendDays += entAttendMonthlyBalance.NEEDATTENDDAYS;
                    }

                    if (entAttendMonthlyBalance.REALATTENDDAYS != null)
                    {
                        dRealAttendDays += entAttendMonthlyBalance.REALATTENDDAYS;
                    }

                    if (entAttendMonthlyBalance.LATEDAYS != null)
                    {
                        dLateDays += entAttendMonthlyBalance.LATEDAYS;
                    }

                    if (entAttendMonthlyBalance.LEAVEEARLYDAYS != null)
                    {
                        dLeaveEarlyDays += entAttendMonthlyBalance.LEAVEEARLYDAYS;
                    }

                    if (entAttendMonthlyBalance.ABSENTDAYS != null)
                    {
                        dAbsentDays += entAttendMonthlyBalance.ABSENTDAYS;
                    }

                    if (entAttendMonthlyBalance.AFFAIRLEAVEDAYS != null)
                    {
                        dAffairLeaveDays += entAttendMonthlyBalance.AFFAIRLEAVEDAYS;
                    }

                    if (entAttendMonthlyBalance.SICKLEAVEDAYS != null)
                    {
                        dSickLeaveDays += entAttendMonthlyBalance.SICKLEAVEDAYS;
                    }

                    if (entAttendMonthlyBalance.OTHERLEAVEDAYS != null)
                    {
                        dOtherLeaveDays += entAttendMonthlyBalance.OTHERLEAVEDAYS;
                    }

                    if (entAttendMonthlyBalance.OVERTIMETIMES != null)
                    {
                        dOverTimeTimes += entAttendMonthlyBalance.OVERTIMETIMES;
                    }

                    if (entAttendMonthlyBalance.OVERTIMESUMHOURS != null)
                    {
                        dOverTimeSumHours += entAttendMonthlyBalance.OVERTIMESUMHOURS;
                    }

                    if (entAttendMonthlyBalance.OVERTIMESUMDAYS != null)
                    {
                        dOvertimeSumDays += entAttendMonthlyBalance.OVERTIMESUMDAYS;
                    }
                }

                entYearlyBalance.NEEDATTENDDAYS = dNeedAttendDays;
                entYearlyBalance.REALATTENDDAYS = dRealAttendDays;
                entYearlyBalance.LATEDAYS = dLateDays;
                entYearlyBalance.LEAVEEARLYDAYS = dLeaveEarlyDays;
                entYearlyBalance.ABSENTDAYS = dAbsentDays;
                entYearlyBalance.AFFAIRLEAVEDAYS = dAffairLeaveDays;
                entYearlyBalance.SICKLEAVEDAYS = dSickLeaveDays;
                entYearlyBalance.OTHERLEAVEDAYS = dOtherLeaveDays;
                entYearlyBalance.OVERTIMETIMES = dOverTimeTimes;
                entYearlyBalance.OVERTIMESUMHOURS = dOverTimeSumHours;
                entYearlyBalance.OVERTIMESUMDAYS = dOvertimeSumDays;

                //权限
                entYearlyBalance.OWNERCOMPANYID = item.OWNERCOMPANYID;
                entYearlyBalance.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                entYearlyBalance.OWNERPOSTID = item.OWNERPOSTID;
                entYearlyBalance.OWNERID = item.OWNERID;

                //基础部分                
                entYearlyBalance.REMARK = string.Empty;
                entYearlyBalance.UPDATEUSERID = item.UPDATEUSERID;
                entYearlyBalance.UPDATEDATE = DateTime.Now;


                if (flag)
                {
                    ModifyYearlyBalance(entYearlyBalance);
                }
                else
                {
                    entYearlyBalance.CREATEDATE = DateTime.Now;
                    entYearlyBalance.CREATEPOSTID = item.CREATEPOSTID;
                    entYearlyBalance.CREATEDEPARTMENTID = item.CREATEDEPARTMENTID;
                    entYearlyBalance.CREATECOMPANYID = item.CREATECOMPANYID;
                    entYearlyBalance.CREATEUSERID = item.CREATEUSERID;

                    AddYearlyBalance(entYearlyBalance);
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 获取员工年度年假及调休假统计情况
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtBalance">结算日期</param>
        /// <param name="dLastAnnualLevelUnusedDays">上年度未休年假</param>
        /// <param name="dAnnualLeaveUsedDays">本年度已休年假</param>
        /// <param name="dAnnualLeaveSumDays">本年度总年假</param>
        /// <param name="dAnnualLeaveValidDays">本年度剩余年假</param>
        /// <param name="dLastLeaveValidDays">上年度未休调休假</param>
        /// <param name="dLeaveUsedDays">本年度已休调休假</param>
        /// <param name="dLeaveValidDays">本年度剩余调休假</param>
        /// <param name="dLeaveSumDays">本年度总调休假</param>
        private void CalculateEmployeeLeaveDays(string strEmployeeID, DateTime dtBalance, ref decimal? dLastAnnualLevelUnusedDays, ref decimal? dAnnualLeaveUsedDays,
            ref decimal? dAnnualLeaveSumDays, ref decimal? dAnnualLeaveValidDays, ref decimal? dLastLeaveValidDays, ref decimal? dLeaveUsedDays,
            ref decimal? dLeaveValidDays, ref decimal? dLeaveSumDays)
        {
            EmployeeLevelDayCountBLL bllEmployeeLevelDayCount = new EmployeeLevelDayCountBLL();
            V_AttendSummaryRecord entCounts = bllEmployeeLevelDayCount.GetAttendSummaryRecordByEmployeeIDAndDate(strEmployeeID, dtBalance);

            if (entCounts == null)
            {
                return;
            }

            dLastAnnualLevelUnusedDays = ParseDecimalValue(entCounts.LastUseYearDays);
            dAnnualLeaveUsedDays = ParseDecimalValue(entCounts.UsedYearDays);
            dAnnualLeaveSumDays = ParseDecimalValue(entCounts.YearDays);
            dAnnualLeaveValidDays = ParseDecimalValue(entCounts.UseYearDays);
            dLastLeaveValidDays = ParseDecimalValue(entCounts.LastUseLevelDayCount);
            dLeaveUsedDays = ParseDecimalValue(entCounts.UsedLeavelDays);
            dLeaveValidDays = ParseDecimalValue(entCounts.UseLeavelDay);
            dLeaveSumDays = ParseDecimalValue(entCounts.LeavelDayCount);
        }

        private decimal ParseDecimalValue(string strValue)
        {
            if (string.IsNullOrWhiteSpace(strValue))
            {
                return 0;
            }

            return decimal.Parse(strValue);
        }

        #endregion

        #endregion

    }
}