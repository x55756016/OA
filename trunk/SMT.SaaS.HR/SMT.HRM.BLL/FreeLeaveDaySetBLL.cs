
/*
 * 文件名：FreeLeaveDaySetBLL.cs
 * 作  用：带薪假设置 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-3-1 11:09:04
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
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    public class FreeLeaveDaySetBLL : BaseBll<T_HR_FREELEAVEDAYSET>, ILookupEntity
    {
        public FreeLeaveDaySetBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取带薪假设置信息
        /// </summary>
        /// <param name="strFreeLeaveDaySetId">主键索引</param>
        /// <returns></returns>
        public T_HR_FREELEAVEDAYSET GetFreeLeaveDaySetByID(string strFreeLeaveDaySetId)
        {
            if (string.IsNullOrEmpty(strFreeLeaveDaySetId))
            {
                return null;
            }

            FreeLeaveDaySetDAL dalFreeLeaveDaySet = new FreeLeaveDaySetDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strFreeLeaveDaySetId))
            {
                strfilter.Append(" FREELEAVEDAYSETID == @0");
                objArgs.Add(strFreeLeaveDaySetId);
            }

            T_HR_FREELEAVEDAYSET entRd = dalFreeLeaveDaySet.GetFreeLeaveDaySetRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 获取针对年假的带薪假设置信息
        /// </summary>
        /// <param name="strFreeLeaveDaySetId">考勤方案设置主键索引</param>
        /// <param name="dEntryMoth">员工入职月份</param>
        /// <param name="strLeaveTypeValue">带薪假关联的请假类型之请假类型值</param>
        /// <returns></returns>
        public T_HR_FREELEAVEDAYSET GetFreeLeaveDaySetByAttendanceSolutionId(string strAttendanceSolutionId, decimal? dEntryMoth, string strLeaveTypeValue)
        {
            if (string.IsNullOrEmpty(strAttendanceSolutionId) || dEntryMoth == null || string.IsNullOrEmpty(strLeaveTypeValue))
            {
                return null;
            }

            var q = from f in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                    join a in dal.GetObjects<T_HR_ATTENDFREELEAVE>().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_LEAVETYPESET") on f.T_HR_LEAVETYPESET.LEAVETYPESETID equals a.T_HR_LEAVETYPESET.LEAVETYPESETID
                    where a.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == strAttendanceSolutionId && f.MINIMONTH<= dEntryMoth && f.MAXMONTH >= dEntryMoth && f.T_HR_LEAVETYPESET.LEAVETYPEVALUE == strLeaveTypeValue
                    select f;
            return q.FirstOrDefault();
        }

        /// <summary>
        /// 根据考勤方案主键索引，获取其配置的带薪假设置信息
        /// </summary>        
        /// <param name="strAttendanceSolutionId">配置带薪假的考勤方案主键索引</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>返回带薪假设置信息</returns>
        public IQueryable<T_HR_FREELEAVEDAYSET> GetFreeLeaveDaySetRdListForAttendanceSolution(string strAttendanceSolutionId, string strSortKey)
        {
            FreeLeaveDaySetDAL dalFreeLeaveDaySet = new FreeLeaveDaySetDAL();

            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " FREELEAVEDAYSETID ";
            }

            var q = dalFreeLeaveDaySet.GetFreeLeaveDaySetRdListForAttendanceSolution(strOrderBy, strAttendanceSolutionId);
            return q;
        }

        /// <summary>
        /// 根据条件，获取带薪假设置信息
        /// </summary>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strLeaveTypeSetID">考勤方案外键索引</param>
        /// <param name="strIsFactor">是否扣全勤</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns></returns>
        public IQueryable<T_HR_FREELEAVEDAYSET> GetAllFreeLeaveDaySetRdListByMultSearch(string strOwnerID, string strLeaveTypeSetID, string strIsFactor, string strSortKey)
        {
            FreeLeaveDaySetDAL dalFreeLeaveDaySet = new FreeLeaveDaySetDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strLeaveTypeSetID))
            {
                strfilter.Append(" T_HR_LEAVETYPESET.LEAVETYPESETID == @0");
                objArgs.Add(strLeaveTypeSetID);
            }

            if (!string.IsNullOrEmpty(strIsFactor))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" ISPERFECTATTENDANCEFACTOR == @" + iIndex.ToString());
                objArgs.Add(strIsFactor);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " FREELEAVEDAYSETID ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_FREELEAVEDAYSET");

            var q = dalFreeLeaveDaySet.GetFreeLeaveDaySetRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取带薪假设置信息,并进行分页
        /// </summary>
        /// <param name="strLeaveTypeSetID">请假类型设置外键索引</param>
        /// <param name="strIsFactor">是否扣全勤</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>T_HR_FREELEAVEDAYSET信息</returns>
        public IQueryable<T_HR_FREELEAVEDAYSET> GetFreeLeaveDaySetRdListByMultSearch(string strOwnerID, string strLeaveTypeSetID, string strIsFactor,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllFreeLeaveDaySetRdListByMultSearch(strOwnerID, strLeaveTypeSetID, strIsFactor, strSortKey);

            return Utility.Pager<T_HR_FREELEAVEDAYSET>(q, pageIndex, pageSize, ref pageCount);
        }

        /// <summary>
        /// 根据假期标准ID,获取关联的带薪假设置记录
        /// </summary>
        /// <param name="strLeaveTypeID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_FREELEAVEDAYSET> GetFreeLeaveDaySetByLeaveTypeID(string strLeaveTypeID)
        {
            if (string.IsNullOrEmpty(strLeaveTypeID))
            {
                return null;
            }

            var q = from f in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                    where f.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveTypeID && f.T_HR_LEAVETYPESET.FINETYPE == "1"
                    select f;

            return q;
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增带薪假设置信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddFreeLeaveDaySet(T_HR_FREELEAVEDAYSET entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                T_HR_FREELEAVEDAYSET ent = new T_HR_FREELEAVEDAYSET();
                Utility.CloneEntity<T_HR_FREELEAVEDAYSET>(entTemp, ent);
                ent.T_HR_LEAVETYPESETReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_LEAVETYPESET", "LEAVETYPESETID", entTemp.T_HR_LEAVETYPESET.LEAVETYPESETID);
                Utility.RefreshEntity(ent);

                FreeLeaveDaySetDAL dalFreeLeaveDaySet = new FreeLeaveDaySetDAL();
                dalFreeLeaveDaySet.Add(ent);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改带薪假设置信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyFreeLeaveDaySet(T_HR_FREELEAVEDAYSET entTemp)
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

                strFilter.Append(" FREELEAVEDAYSETID == @0");

                objArgs.Add(entTemp.FREELEAVEDAYSETID);

                FreeLeaveDaySetDAL dalFreeLeaveDaySet = new FreeLeaveDaySetDAL();
                flag = dalFreeLeaveDaySet.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_FREELEAVEDAYSET entUpdate = dalFreeLeaveDaySet.GetFreeLeaveDaySetRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                //Utility.RefreshEntity(entUpdate);
                Utility.CloneEntity<T_HR_FREELEAVEDAYSET>(entTemp, entUpdate);
                if (entUpdate.T_HR_LEAVETYPESETReference.EntityKey == null)
                {
                    entUpdate.T_HR_LEAVETYPESETReference.EntityKey =
                        new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_LEAVETYPESET", "LEAVETYPESETID", entTemp.T_HR_LEAVETYPESET.LEAVETYPESETID);
                }

                dalFreeLeaveDaySet.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除带薪假设置信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteFreeLeaveDaySet(string strFreeLeaveDaySetId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strFreeLeaveDaySetId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" FREELEAVEDAYSETID == @0");

                objArgs.Add(strFreeLeaveDaySetId);

                FreeLeaveDaySetDAL dalFreeLeaveDaySet = new FreeLeaveDaySetDAL();
                flag = dalFreeLeaveDaySet.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_FREELEAVEDAYSET entDel = dalFreeLeaveDaySet.GetFreeLeaveDaySetRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalFreeLeaveDaySet.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }
        /// <summary>
        /// 根据员工ID，获取员工当前的带薪假期
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>返回带薪假期</returns>
        public List<V_EMPLOYEELEAVE> GetFreeLeaveDaySetByEmployeeID(string employeeID)
        {
            return GetEmployeeLeaveByEmployeeID(employeeID, DateTime.Now.Date);
        }

        /// <summary>
        /// 根据员工ID获取员工的带薪假期的天数，调休假天数，及已休假天数。
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <param name="date">日期</param>
        /// <returns></returns>
        public List<V_EMPLOYEELEAVE> GetEmployeeLeaveByEmployeeID(string employeeID, DateTime date)
        {
            List<V_EMPLOYEELEAVE> leaveList = new List<V_EMPLOYEELEAVE>();
            try
            {
                //获取员工组织架构
                EmployeeBLL employeeBll = new EmployeeBLL();
                V_EMPLOYEEPOST employeePost = employeeBll.GetEmployeeDetailByID(employeeID);
                //获取员工的入职信息
                EmployeeEntryBLL entryBll = new EmployeeEntryBLL();
                T_HR_EMPLOYEEENTRY entry = entryBll.GetEmployeeEntryByEmployeeID(employeeID);
                DateTime entryDate = entry.ENTRYDATE.Value;
                int m = Utility.DateDiff(entryDate, date, "M");
                string departmentID = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                string comparyID = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;

               
                var ent = from a in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                          join al in dal.GetObjects<T_HR_ATTENDFREELEAVE>().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_LEAVETYPESET") on a.T_HR_LEAVETYPESET.LEAVETYPESETID equals al.T_HR_LEAVETYPESET.LEAVETYPESETID
                          join ad in dal.GetObjects<T_HR_ATTENDANCESOLUTION>() on al.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID equals ad.ATTENDANCESOLUTIONID
                          join at in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>() on ad.ATTENDANCESOLUTIONID equals at.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID
                          where (at.ASSIGNEDOBJECTID == departmentID || at.ASSIGNEDOBJECTID == comparyID)
                          && a.MINIMONTH <= m
                          && a.MAXMONTH >= m
                          select new V_EMPLOYEELEAVE()
                          {
                              EmployeeID = employeeID,
                              EmployeeCode = employeePost.T_HR_EMPLOYEE.EMPLOYEECODE,
                              EmployeeName = employeePost.T_HR_EMPLOYEE.EMPLOYEECNAME,
                              LeaveTypeName = a.T_HR_LEAVETYPESET.LEAVETYPENAME,
                              LeaveTypeSetID = a.T_HR_LEAVETYPESET.LEAVETYPESETID,
                              MaxDays = a.T_HR_LEAVETYPESET.MAXDAYS.Value,
                              FineType = a.T_HR_LEAVETYPESET.FINETYPE,
                              IsPerfectAttendanceFactor = a.ISPERFECTATTENDANCEFACTOR,
                              LeaveDays = a.LEAVEDAYS.Value
                          };
                leaveList = ent.Count() > 0 ? ent.ToList() : null;

                //获取年度考勤结算日
                AttendanceSolutionAsignBLL bll = new AttendanceSolutionAsignBLL();
                var tempEnt = bll.GetAttendanceSolutionAsignByEmployeeID(employeeID);
                string strDate = tempEnt.T_HR_ATTENDANCESOLUTION.YEARLYBALANCEDATE;
                string strType = tempEnt.T_HR_ATTENDANCESOLUTION.YEARLYBALANCETYPE; //结算方式
                //获取加班调休假的天数
                EmployeeLevelDayCountBLL leaveDaybll = new EmployeeLevelDayCountBLL();
                AdjustLeaveBLL adjustbll = new AdjustLeaveBLL();
                decimal leaveDays = leaveDaybll.GetLevelDayCountByEmployeeID(employeeID, date, strDate);

                V_EMPLOYEELEAVE leave = new V_EMPLOYEELEAVE();
                ent = from a in dal.GetObjects<T_HR_LEAVETYPESET>()
                      where !string.IsNullOrEmpty(a.LEAVETYPEVALUE)
                      select new V_EMPLOYEELEAVE
                      {
                          EmployeeID = employeeID,
                          EmployeeCode = employeePost.T_HR_EMPLOYEE.EMPLOYEECODE,
                          EmployeeName = employeePost.T_HR_EMPLOYEE.EMPLOYEECNAME,
                          LeaveTypeName = a.LEAVETYPENAME,
                          LeaveTypeSetID = a.LEAVETYPESETID,
                          MaxDays = a.MAXDAYS.Value,
                          FineType = a.FINETYPE,
                          LeaveDays = leaveDays
                      };
                leave = ent.Count() > 0 ? ent.FirstOrDefault() : null;
                if (leave != null)
                {
                    leaveList.Add(leave);
                }
                //获取已休天数,并算出可休假天数
                foreach (var obj in leaveList)
                {
                    //剩余可休假为累计的情况
                    if (strType == "1")
                    {
                        //往年可休假的总数
                        decimal leaveHistoryDays = leaveDaybll.GetLevelDayCountHistoryByEmployeeID(employeeID, date, strDate);
                        //往年已休假的总数
                        decimal UsedUpDays = adjustbll.GetUseUpHistory(obj.LeaveTypeSetID, obj.EmployeeID, date, strDate);
                        obj.LeaveDays += leaveHistoryDays - UsedUpDays;
                    }
                    obj.UsedLeaveDays = adjustbll.GetUseUp(obj.LeaveTypeSetID, obj.EmployeeID, date, strDate);
                    obj.UseableLeaveDays = obj.LeaveDays - obj.UsedLeaveDays;
                }
              
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
            return leaveList;
        }
        #endregion

        #region ILookupEntity 成员

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            FreeLeaveDaySetDAL dalFreeLeaveDaySet = new FreeLeaveDaySetDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();
            string strOrderBy = string.Empty;
            strOrderBy = "FREELEAVEDAYSETID";

            IQueryable<T_HR_FREELEAVEDAYSET> ents = dalFreeLeaveDaySet.GetFreeLeaveDaySetRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs);

            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        #endregion

    }
}