
/*
 * 文件名：AttendMonthlyBatchBalanceDAL.cs
 * 作  用：T_HR_ATTENDMONTHLYBATCHBALANCE 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-5-21 16:27:41
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
    public class AttendMonthlyBatchBalanceDAL : CommDal<T_HR_ATTENDMONTHLYBATCHBALANCE>
    {
        public AttendMonthlyBatchBalanceDAL()
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

        public bool IsExistsRd(decimal dBalanceYear, decimal dBalanceMonth, string strFilter, params object[] objArgs)
        {
            bool flag = false;

            var q = from v in GetObjects()
                    select v;

            q = q.Where(c => c.BALANCEYEAR == dBalanceYear && c.BALANCEMONTH == dBalanceMonth);

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
        /// 获取指定条件的T_HR_ATTENDMONTHLYBATCHBALANCE信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_ATTENDMONTHLYBATCHBALANCE信息</returns>
        public T_HR_ATTENDMONTHLYBATCHBALANCE GetAttendMonthlyBatchBalanceRdByMultSearch(string strFilter, params object[] objArgs)
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
        /// 获取指定条件的T_HR_ATTENDMONTHLYBATCHBALANCE信息
        /// </summary>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算月份</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_ATTENDMONTHLYBATCHBALANCE信息</returns>
        public T_HR_ATTENDMONTHLYBATCHBALANCE GetAttendMonthlyBatchBalanceByMultSearch(decimal dBalanceYear, decimal dBalanceMonth, string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects()
                    select v;

            q = q.Where(c => c.BALANCEYEAR == dBalanceYear && c.BALANCEMONTH == dBalanceMonth);

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            T_HR_ATTENDMONTHLYBATCHBALANCE entRes = null;

            if (q.Count() == 0)
            {
                return null;
            }
            else if (q.Count() >= 1)
            {
                foreach (T_HR_ATTENDMONTHLYBATCHBALANCE entTemp in q)
                {
                    var d = from b in GetObjects<T_HR_ATTENDMONTHLYBALANCE>().Include("T_HR_ATTENDMONTHLYBATCHBALANCE")
                            where b.T_HR_ATTENDMONTHLYBATCHBALANCE.MONTHLYBATCHID == entTemp.MONTHLYBATCHID
                            select b;

                    if (d == null)
                    {
                        continue;
                    }

                    if (d.Count() > 0)
                    {
                        entRes = entTemp;
                        break;
                    }
                }
            }

            return entRes;
        }

        /// <summary>
        /// 获取指定条件的T_HR_ATTENDMONTHLYBATCHBALANCE信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_ATTENDMONTHLYBATCHBALANCE信息</returns>
        public IQueryable<T_HR_ATTENDMONTHLYBATCHBALANCE> GetAttendMonthlyBatchBalanceRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
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