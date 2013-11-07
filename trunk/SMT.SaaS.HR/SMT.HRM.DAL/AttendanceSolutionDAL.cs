
/*
 * 文件名：AttendanceSolutionDAL.cs
 * 作  用：考勤方案 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-2-26 14:19:12
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using SMT_HRM_EFModel;

namespace SMT.HRM.DAL
{
    public class AttendanceSolutionDAL : CommDal<T_HR_ATTENDANCESOLUTION>
    {
        public AttendanceSolutionDAL()
        {
        }

        /// <summary>
        /// 根据参数，检查是否存在指定记录(方便进行数据增删改)
        /// </summary>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>True/False(是/否)</returns>
        public bool IsExistsRd(string strFilter, params object[] objArgs)
        {
            bool flag = false;

            var q = from v in GetObjects()
                    select v;

            if (objArgs.Count() <= 0)
            {
                return flag;
            }

            q = q.Where(strFilter, objArgs);

            if (q.Count() > 0)
            {
                flag = true;
            }

            return flag;
        }


       /// <summary>
        /// 根据加班设置id查找考勤方案定义表是否有用到
       /// </summary>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤方案信息</returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionRdByOVERId(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects()
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        
        /// <summary>
        /// 获取指定条件的考勤方案信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤方案信息</returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_OVERTIMEREWARD").Include("T_HR_SCHEDULINGTEMPLATEMASTER")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 根据员工ID,起止时间(一般为月头至月尾)获取其应用的考勤方案
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd)
        {
            var qr = from r in GetObjects<T_HR_ATTENDANCERECORD>()
                     where r.EMPLOYEEID == strEmployeeID && r.ATTENDANCEDATE >= dtStart && r.ATTENDANCEDATE <= dtEnd                     
                     select r;

            if (qr.Count() == 0)
            {
                return null;
            }

            T_HR_ATTENDANCERECORD entRecord = qr.FirstOrDefault();

            var qs = from s in GetObjects()
                     where s.ATTENDANCESOLUTIONID == entRecord.ATTENDANCESOLUTIONID
                     select s;

            if (qs.Count() == 0)
            {
                return null;
            }

            return qs.FirstOrDefault();
        }

        /// <summary>
        /// 根据员工ID获取其应用的考勤方案
        /// </summary>
        /// <param name="strPostID"></param>
        /// <param name="strDepartmentID"></param>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByEmployeeID(string strPostID, string strDepartmentID, string strCompanyID)
        {
            var qp = from v in GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                     where v.ASSIGNEDOBJECTID == strPostID && v.STARTDATE <= DateTime.Now && v.ENDDATE >= DateTime.Now
                     select v.T_HR_ATTENDANCESOLUTION;

            var qd = from v in GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                     where v.ASSIGNEDOBJECTID == strDepartmentID && v.STARTDATE <= DateTime.Now && v.ENDDATE >= DateTime.Now
                     select v.T_HR_ATTENDANCESOLUTION;

            var qc = from v in GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                     where v.ASSIGNEDOBJECTID == strCompanyID && v.STARTDATE <= DateTime.Now && v.ENDDATE >= DateTime.Now
                     select v.T_HR_ATTENDANCESOLUTION;

            if (qp.Count() > 0)
            {
                return qp.FirstOrDefault();
            }
            else
            {
                if (qd.Count() > 0)
                {
                    return qd.FirstOrDefault();
                }
                else
                {
                    return qc.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// 获取指定条件的考勤方案信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤方案信息</returns>
        public IQueryable<T_HR_ATTENDANCESOLUTION> GetAttendanceSolutionRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects()
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.OrderBy(strOrderBy);
        }
    }
}