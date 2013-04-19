/*
 * 文件名：EmployeeSignInDetailDAL.cs
 * 作  用：员工异常记录 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-4-7 18:57:12
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
    public class EmployeeSignInDetailDAL : CommDal<T_HR_EMPLOYEESIGNINDETAIL>
    {
        public EmployeeSignInDetailDAL()
        {
        }

        /// <summary>
        /// 根据参数，检查是否存在指定记录(方便进行数据增删改)
        /// </summary>
        /// <param name="strFilter">异常日期</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>True/False(是/否)</returns>
        public bool IsExistsRd(string strFilter, params object[] objArgs)
        {
            bool flag = false;

            var q = from v in GetObjects().Include("T_HR_EMPLOYEEABNORMRECORD").Include("T_HR_EMPLOYEESIGNINRECORD")
                    select v;
            
            if (objArgs.Count() <= 0 || string.IsNullOrEmpty(strFilter))
            {
                return false;
            }

            q = q.Where(strFilter, objArgs);

            flag = false;
            if (q.Count() > 0)
            {
                flag = true;
            }

            return flag;
        }

        /// <summary>
        /// 获取指定条件的员工异常记录信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工异常记录信息</returns>
        public T_HR_EMPLOYEESIGNINDETAIL GetEmployeeSignInDetailRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_EMPLOYEEABNORMRECORD").Include("T_HR_EMPLOYEESIGNINRECORD")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的员工异常记录信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strOrderBy">查询的月份</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工异常记录信息</returns>
        public IQueryable<T_HR_EMPLOYEESIGNINDETAIL> GetEmployeeSignInDetailRdListByMultSearch(string strOrderBy, string strCurDateMonth, string strFilter, params object[] objArgs)
        {

            var q = from v in GetObjects().Include("T_HR_EMPLOYEEABNORMRECORD").Include("T_HR_EMPLOYEESIGNINRECORD")
                    select v;

            if (!string.IsNullOrEmpty(strCurDateMonth))
            {
                DateTime dtStart = DateTime.Parse(strCurDateMonth + "-1");
                DateTime dtEnd = dtStart.AddMonths(1).AddDays(-1);

                q = q.Where(c => c.ABNORMALDATE >= dtStart && c.ABNORMALDATE <= dtEnd);
            }

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.OrderBy(strOrderBy);
        }
    }
}