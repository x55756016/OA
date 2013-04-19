
/*
 * 文件名：AttendYearlyBalanceDAL.cs
 * 作  用：员工考勤年度结算 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-3-29 10:03:42
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
    public class AttendYearlyBalanceDAL : CommDal<T_HR_ATTENDYEARLYBALANCE>
    {
        public AttendYearlyBalanceDAL()
        {
        }

        /// <summary>
        /// 根据参数，检查是否存在指定记录(方便进行数据增删改)
        /// </summary>
        /// <param name="strEmployeeID">员工序号</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <returns>True/False(是/否)</returns>
        public bool IsExistsRd(string strEmployeeID, decimal dBalanceYear)
        {
            bool flag = false;
            decimal dYearCheck = 1900;

            var q = from v in GetObjects()
                    select v;

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                q = q.Where(c => c.EMPLOYEEID == strEmployeeID);
            }

            if (dBalanceYear > dYearCheck)
            {
                q = q.Where(c => c.BALANCEYEAR == dYearCheck);
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
        public bool IsExistsRd(string strFilter, params object[] objArgs)
        {
            bool flag = false;

            var q = from v in GetObjects()
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
        /// 获取指定条件的员工考勤年度结算信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工考勤年度结算信息</returns>
        public T_HR_ATTENDYEARLYBALANCE GetAttendYearlyBalanceRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects()
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            if (q.Count() == 0)
            {
                return null;
            }

            return q.First();
        }
        

        /// <summary>
        /// 获取指定条件的员工考勤年度结算信息
        /// </summary>
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工考勤年度结算信息</returns>
        public IQueryable<T_HR_ATTENDYEARLYBALANCE> GetAttendYearlyBalanceRdListByMultSearch(string strOrderBy, decimal dBalanceYear, 
            string strFilter, params object[] objArgs)
        {
            decimal dYearCheck = 1900;

            var q = from v in GetObjects()
                    select v;            

            if (dBalanceYear > dYearCheck)
            {
                q = q.Where(c => c.BALANCEYEAR == dBalanceYear);
            }

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            q = q.OrderBy(strOrderBy);

            return q;
        }
    }
}