
/*
 * 文件名：AttendanceRecordDAL.cs
 * 作  用：T_HR_ATTENDANCERECORD 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-4-6 22:00:47
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
    public class AttendanceRecordDAL : CommDal<T_HR_ATTENDANCERECORD>
    {
        public AttendanceRecordDAL()
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

            var q = from v in GetObjects().Include("T_HR_SHIFTDEFINE")
                    select v;

            if (objArgs.Count() <= 0 || string.IsNullOrEmpty(strFilter))
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
        /// 根据参数，检查是否存在指定记录(方便进行数据增删改)
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtAttendanceDate">考勤日期</param>
        /// <returns>True/False(是/否)</returns>
        public bool IsExistsRd(string strEmployeeID, DateTime? dtAttendanceDate)
        {
            bool flag = false;

            var q = from v in GetObjects().Include("T_HR_SHIFTDEFINE")
                    select v;

            if (string.IsNullOrEmpty(strEmployeeID) || dtAttendanceDate == null)
            {
                return flag;
            }

            q = q.Where(c => c.EMPLOYEEID == strEmployeeID && c.ATTENDANCEDATE == dtAttendanceDate);

            if (q.Count() > 0)
            {
                flag = true;
            }

            return flag;
        }

        /// <summary>
        /// 获取指定条件的T_HR_ATTENDANCERECORD信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_ATTENDANCERECORD信息</returns>
        public T_HR_ATTENDANCERECORD GetAttendanceRecordRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_SHIFTDEFINE")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的T_HR_ATTENDANCERECORD信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_ATTENDANCERECORD信息</returns>
        public IQueryable<T_HR_ATTENDANCERECORD> GetAttendanceRecordRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_SHIFTDEFINE")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }


            return q.OrderBy(strOrderBy);
        }

        /// <summary>
        /// 获取指定条件的公共假期设置信息
        /// </summary>  
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的</param>
        /// <param name="strPunchDateFrom">起始时间(截取打卡日期)</param>
        /// <param name="strPunchDateTo">结束时间(截取打卡日期)</param>
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>公共假期设置信息</returns>
        public IQueryable<T_HR_ATTENDANCERECORD> GetAttendanceRdListByMultSearch(string sType, string sValue, string strPunchDateFrom, string strPunchDateTo,
            string strOrderBy, string strFilter, params object[] objArgs)
        {
            var ents = from v in GetObjects()
                       select v;

            switch (sType)
            {
                case "Company":
                    ents = from v in GetObjects()
                           join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue
                           select v;
                    break;
                case "Department":
                    ents = from v in GetObjects()
                           join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           where d.DEPARTMENTID == sValue
                           select v;
                    break;
                case "Post":
                    ents = from v in GetObjects()
                           join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           where p.POSTID == sValue
                           select v;
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(strPunchDateFrom))
            {
                DateTime dtStart = new DateTime();
                DateTime.TryParse(strPunchDateFrom, out dtStart);

                ents = ents.Where(v => v.ATTENDANCEDATE >= dtStart);

            }

            if (!string.IsNullOrEmpty(strPunchDateTo))
            {
                DateTime dtEnd = new DateTime();
                DateTime.TryParse(strPunchDateTo, out dtEnd);
                dtEnd = dtEnd.AddDays(1).AddMinutes(-1);

                ents = ents.Where(v => v.ATTENDANCEDATE <= dtEnd);
            }

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                ents = ents.Where(strFilter, objArgs);
            }

            //ents = ents.OrderBy(strOrderBy);
            return ents.OrderBy(strOrderBy);
        }
    }
}