/*
 * 文件名：AttendMonthlyBalanceDAL.cs
 * 作  用：员工考勤月度结算 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-3-29 9:11:21
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
    public class AttendMonthlyBalanceDAL : CommDal<T_HR_ATTENDMONTHLYBALANCE>
    {
        public AttendMonthlyBalanceDAL()
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

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            if (q.Count() > 0)
            {
                flag = true;
            }

            return flag;
        }

        /// <summary>
        /// 根据参数，检查是否存在指定记录(方便进行数据增删改)
        /// </summary>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>True/False(是/否)</returns>
        public bool IsExistsRd(string strCompanyID, string strEmployeeID, decimal dBalanceYear, decimal dBalanceMonth)
        {
            bool flag = false;
            decimal dYearCheck = 1900, dMonthStartCheck = 1, dMonthEndCheck = 12;

            var q = from v in GetObjects()
                    select v;

            if (!string.IsNullOrEmpty(strCompanyID))
            {
                q = q.Where(c => c.OWNERCOMPANYID == strCompanyID);
            }

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                q = q.Where(c => c.EMPLOYEEID == strEmployeeID);
            }

            if (dBalanceYear > dYearCheck)
            {
                q = q.Where(c => c.BALANCEYEAR == dBalanceYear);
            }

            if (dBalanceMonth >= dMonthStartCheck && dBalanceMonth <= dMonthEndCheck)
            {
                q = q.Where(c => c.BALANCEMONTH == dBalanceMonth);
            }

            if (q.Count() > 0)
            {
                flag = true;
            }

            return flag;
        }

        /// <summary>
        /// 获取指定条件的员工考勤月度结算信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工考勤月度结算信息</returns>
        public T_HR_ATTENDMONTHLYBALANCE GetAttendMonthlyBalanceRdByEmployeeID(string strCompanyID, string strEmployeeID, decimal dBalanceYear, decimal dBalanceMonth)
        {
            decimal dYearCheck = 1900, dMonthStartCheck = 1, dMonthEndCheck = 12;

            var q = from v in GetObjects().Include("T_HR_ATTENDMONTHLYBATCHBALANCE")
                    select v;

            if (!string.IsNullOrEmpty(strCompanyID))
            {
                q = q.Where(c => c.OWNERCOMPANYID == strCompanyID);
            }

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                q = q.Where(c => c.EMPLOYEEID == strEmployeeID);
            }

            if (dBalanceYear > dYearCheck)
            {
                q = q.Where(c => c.BALANCEYEAR == dBalanceYear);
            }

            if (dBalanceMonth >= dMonthStartCheck && dBalanceMonth <= dMonthEndCheck)
            {
                q = q.Where(c => c.BALANCEMONTH == dBalanceMonth);
            }

            return q.FirstOrDefault();
        }

        /// <summary>
        /// 获取指定条件的员工考勤月度结算信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工考勤月度结算信息</returns>
        public T_HR_ATTENDMONTHLYBALANCE GetAttendMonthlyBalanceRdByMultSearch(string strFilter, params object[] objArgs)
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
        /// 获取指定条件的员工考勤月度结算信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工考勤月度结算信息</returns>
        public IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetAttendMonthlyBalanceRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var ents = from v in GetObjects()
                       select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                ents = ents.Where(strFilter, objArgs);
            }

            ents = ents.OrderBy(strOrderBy);

            return ents;
        }

        /// <summary>
        /// 获取指定条件的员工考勤月度结算信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的</param>
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算年份</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工考勤月度结算信息</returns>
        public IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetAttendMonthlyBalanceRdListByMultSearch(string sType, string sValue, string strOrderBy, decimal dBalanceYear,
            decimal dBalanceMonth, string strFilter, params object[] objArgs)
        {
            decimal dYearCheck = 1900;

            var ents = from v in GetObjects()
                       select v;

            switch (sType)
            {
                case "Company":
                    ents = GetBlanceByCompanyId(sValue);
                    break;
                case "Department":
                    ents = GetBlanceByDepartmentId(sValue);
                    break;
                case "Post":
                    ents = GetBlanceByPostId(sValue);
                    break;
            }

            if (dBalanceYear > dYearCheck)
            {
                ents = ents.Where(c => c.BALANCEYEAR == dBalanceYear);
            }

            ents = ents.Where(c => c.BALANCEMONTH == dBalanceMonth);

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                ents = ents.Where(strFilter, objArgs);
            }

            ents = ents.OrderBy(strOrderBy);

            return ents;
        }

        /// <summary>
        /// 根据岗位Id获取考勤结算信息
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>
        private IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetBlanceByPostId(string strPostID)
        {

            string strIsAgnecy = Convert.ToInt32(IsAgencyPost.No).ToString();
            string strEditState = Convert.ToInt32(EditStates.Actived).ToString();
            string strCheckState = Convert.ToInt32(CheckStates.Approved).ToString();
            //查正式和试用员工
            IQueryable<T_HR_EMPLOYEE> qy = from e in GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join en in GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE") on e.T_HR_EMPLOYEE.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                                           where e.T_HR_POST.POSTID == strPostID && e.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && en.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState
                                           select e.T_HR_EMPLOYEE;

            //查已经过离职确认，但还未到离职时间的员工
            IQueryable<T_HR_EMPLOYEE> qn = from e in GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join l in GetObjects<T_HR_LEFTOFFICECONFIRM>() on e.T_HR_EMPLOYEE.EMPLOYEEID equals l.EMPLOYEEID
                                           where e.T_HR_POST.POSTID == strPostID && l.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy
                                           select e.T_HR_EMPLOYEE;

            IQueryable<T_HR_EMPLOYEE> ents = qy;
            if (qn.Count() > 0)
            {
                ents = qy.Concat(qn);
            }

            var q = from b in GetObjects()
                    join e in ents on b.EMPLOYEEID equals e.EMPLOYEEID
                    select b;

            return q.Distinct();
        }

        /// <summary>
        /// 根据部门Id获取考勤结算信息
        /// </summary>
        /// <param name="strDepartmentID"></param>
        /// <returns></returns>
        private IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetBlanceByDepartmentId(string strDepartmentID)
        {
            string strIsAgnecy = Convert.ToInt32(IsAgencyPost.No).ToString();
            string strEditState = Convert.ToInt32(EditStates.Actived).ToString();
            string strCheckState = Convert.ToInt32(CheckStates.Approved).ToString();
            //查正式和试用员工
            IQueryable<T_HR_EMPLOYEE> qy = from e in GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join en in GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE") on e.T_HR_EMPLOYEE.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                                           where d.DEPARTMENTID == strDepartmentID && e.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && en.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState
                                           select e.T_HR_EMPLOYEE;

            //查已经过离职确认，但还未到离职时间的员工
            IQueryable<T_HR_EMPLOYEE> qn = from e in GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join l in GetObjects<T_HR_LEFTOFFICECONFIRM>() on e.T_HR_EMPLOYEE.EMPLOYEEID equals l.EMPLOYEEID
                                           where d.DEPARTMENTID == strDepartmentID && l.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy
                                           select e.T_HR_EMPLOYEE;

            IQueryable<T_HR_EMPLOYEE> ents = qy;
            if (qn.Count() > 0)
            {
                ents = qy.Concat(qn);
            }

            var q = from b in GetObjects()
                    join e in ents on b.EMPLOYEEID equals e.EMPLOYEEID
                    select b;

            return q.Distinct();
        }

        /// <summary>
        /// 根据公司Id获取考勤结算信息
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        private IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetBlanceByCompanyId(string strCompanyID)
        {
            string strIsAgnecy = Convert.ToInt32(IsAgencyPost.No).ToString();
            string strEditState = Convert.ToInt32(EditStates.Actived).ToString();
            string strCheckState = Convert.ToInt32(CheckStates.Approved).ToString();
            //查正式和试用员工
            IQueryable<T_HR_EMPLOYEE> qy = from e in GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY") on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join c in GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                           join en in GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE") on e.T_HR_EMPLOYEE.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                                           where c.COMPANYID == strCompanyID && e.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && en.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState
                                           select e.T_HR_EMPLOYEE;

            //查已经过离职确认，但还未到离职时间的员工
            IQueryable<T_HR_EMPLOYEE> qn = from e in GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY") on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join c in GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                           join l in GetObjects<T_HR_LEFTOFFICECONFIRM>() on e.T_HR_EMPLOYEE.EMPLOYEEID equals l.EMPLOYEEID
                                           where c.COMPANYID == strCompanyID && l.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy
                                           select e.T_HR_EMPLOYEE;

            IQueryable<T_HR_EMPLOYEE> ents = qy;
            if (qn.Count() > 0)
            {
                ents = qy.Concat(qn);
            }

            var q = from b in GetObjects()
                    join e in ents on b.EMPLOYEEID equals e.EMPLOYEEID
                    select b;

            return q.Distinct();
        }
    }
}