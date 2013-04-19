using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class AdjustLeaveBLL : BaseBll<T_HR_ADJUSTLEAVE>
    {
        /// <summary>
        /// 根据休假类型，员工ID获取，获取当年已调休的天数
        /// </summary>
        /// <param name="type">调假类型</param>
        /// <param name="employeeID">员工ID</param>
        /// <returns></returns>
        public decimal GetUseUp(string type, string employeeID, DateTime currDate, string strDate)
        {
            if (string.IsNullOrEmpty(strDate))
            {
                return 0;
            }

            decimal UseUpDay = 0;
            string checkState = Convert.ToInt32(CheckStates.Approved).ToString();

            DateTime startDate = Convert.ToDateTime(currDate.Year.ToString() + "-" + strDate);
            DateTime endDate = currDate;
            if (startDate >= currDate)
            {
                startDate = startDate.AddYears(-1);
                endDate = currDate;
            }
            ///TO QUERY:算已调休记录是根据已调休开始日期，还是结束日期进行判断
            var ent = dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD").Where(s => s.LEAVETYPESETID == type
                && s.T_HR_EMPLOYEELEAVERECORD.CHECKSTATE == checkState && (s.BEGINTIME >= startDate && s.BEGINTIME <= endDate)).Select(o => o.ADJUSTLEAVEDAYS).Sum();
            if (ent != null)
            {
                UseUpDay = ent.Value;
            }

            return UseUpDay;
        }
        /// <summary>
        /// 根据休假类型，员工ID获取，获取往年已调休的所有天数
        /// </summary>
        /// <param name="type">调假类型</param>
        /// <param name="employeeID">员工ID</param>
        /// <returns></returns>
        public decimal GetUseUpHistory(string type, string employeeID, DateTime currDate, string strDate)
        {
            if (string.IsNullOrEmpty(strDate))
            {
                return 0;
            }

            decimal UseUpDay = 0;
            string checkState = Convert.ToInt32(CheckStates.Approved).ToString();


            DateTime dDate = Convert.ToDateTime(currDate.Year.ToString() + "-" + strDate);
            if (dDate >= currDate)
            {
                dDate = dDate.AddYears(-1);
            }
            ///TO QUERY:算已调休记录是根据已调休开始日期，还是结束日期进行判断
            var ent = dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD").Where(s => s.LEAVETYPESETID == type
                && s.T_HR_EMPLOYEELEAVERECORD.CHECKSTATE == checkState && s.BEGINTIME <= dDate).Select(o => o.ADJUSTLEAVEDAYS).Sum();
            if (ent != null)
            {
                UseUpDay = ent.Value;
            }

            return UseUpDay;
        }

        /// <summary>
        /// 根据请假记录ID获取对应调休假
        /// </summary>
        /// <param name="strLeaveRecordID">请假记录ID</param>
        /// <returns></returns>
        public T_HR_ADJUSTLEAVE GetAdjustLeaveByLeaveRecordID(string strLeaveRecordID)
        {
            var q = from a in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD")
                    where a.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strLeaveRecordID
                    select a;

            return q.FirstOrDefault();
        }

        /// <summary>
        /// 根据请假记录ID获取对应调休假
        /// </summary>
        /// <param name="strLeaveRecordID">请假记录ID</param>
        /// <returns></returns>
        public List<V_ADJUSTLEAVE> GetAdjustLeaveDetailListByLeaveRecordID(string strLeaveRecordID)
        {
            List<V_ADJUSTLEAVE> entViews = new List<V_ADJUSTLEAVE>();


            IQueryable<T_HR_ADJUSTLEAVE> entAds = from a in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD")
                                                  join l in dal.GetObjects<T_HR_LEAVETYPESET>() on a.LEAVETYPESETID equals l.LEAVETYPESETID
                                                  where a.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strLeaveRecordID
                                                  select a;

            if (entAds.Count() == 0)
            {
                return entViews;
            }

            foreach(T_HR_ADJUSTLEAVE item in entAds)
            {
                var q = from ec in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
                        where ec.LEAVETYPESETID == item.LEAVETYPESETID && ec.EMPLOYEEID == item.EMPLOYEEID
                        select ec;

                if(q.Count() == 0)
                {
                    continue;
                }

                T_HR_EMPLOYEELEVELDAYCOUNT entDayCount = q.FirstOrDefault();
                
                V_ADJUSTLEAVE entView = new V_ADJUSTLEAVE();
                entView.T_HR_ADJUSTLEAVE = item;
                entView.VacationType = entDayCount.VACATIONTYPE;
                entView.VacationDays = entDayCount.DAYS;

                entViews.Add(entView);
            }           

            return entViews;
        }
    }
}
