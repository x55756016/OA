
/*
 * 文件名：AttendanceSolutionDeductDAL.cs
 * 作  用：考勤方案异常扣款 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-2-26 15:10:36
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
    public class AttendanceSolutionDeductDAL : CommDal<T_HR_ATTENDANCESOLUTIONDEDUCT>
    {
        public AttendanceSolutionDeductDAL()
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

            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_ATTENDANCEDEDUCTMASTER")                    
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
        /// 获取指定条件的考勤方案异常扣款信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤方案异常扣款信息</returns>
        public T_HR_ATTENDANCESOLUTIONDEDUCT GetAttendanceSolutionDeductRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_ATTENDANCEDEDUCTMASTER")
                    select v;

            if (objArgs.Count() > 0)
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的考勤方案异常扣款信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤方案异常扣款信息</returns>
        public IQueryable<T_HR_ATTENDANCESOLUTIONDEDUCT> GetAttendanceSolutionDeductRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_ATTENDANCEDEDUCTMASTER")
                    select v;

            if (objArgs.Count() > 0)
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.OrderBy(strOrderBy);
        }
    }
}