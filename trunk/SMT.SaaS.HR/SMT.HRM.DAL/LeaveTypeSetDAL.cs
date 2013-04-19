/*
 * 文件名：LeaveTypeSetDAL.cs
 * 作  用：请假类型设置数据操作类
 * 创建人：吴鹏
 * 创建时间：2010年2月23日, 15:43:39
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
    public class LeaveTypeSetDAL:CommDal<T_HR_LEAVETYPESET>
    {
        public LeaveTypeSetDAL()
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

            var q = from l in GetObjects()
                    select l;

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
        /// 获取指定条件的请假类型设置信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>公共假期设置信息</returns>
        public T_HR_LEAVETYPESET GetLeaveTypeSetRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from l in GetObjects()
                    select l;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的公共假期设置信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>公共假期设置信息</returns>
        public IQueryable<T_HR_LEAVETYPESET> GetLeaveTypeSetRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from l in GetObjects()
                    select l;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }


            return q.OrderBy(strOrderBy);
        }
    }
}
