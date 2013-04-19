
/*
 * 文件名：FreeLeaveDaySetDAL.cs
 * 作  用：带薪假设置 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-3-1 11:06:57
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
    public class FreeLeaveDaySetDAL : CommDal<T_HR_FREELEAVEDAYSET>
    {
        public FreeLeaveDaySetDAL()
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

            var q = from v in GetObjects().Include("T_HR_LEAVETYPESET")
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
        /// 获取指定条件的带薪假设置信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_FREELEAVEDAYSET信息</returns>
        public T_HR_FREELEAVEDAYSET GetFreeLeaveDaySetRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_LEAVETYPESET")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的带薪假设置信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_FREELEAVEDAYSET信息</returns>
        public IQueryable<T_HR_FREELEAVEDAYSET> GetFreeLeaveDaySetRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from f in GetObjects().Include("T_HR_LEAVETYPESET")
                    select f;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }


            return q.OrderBy(strOrderBy);
        }

        /// <summary>
        /// 根据考勤方案主键索引，获取其配置的带薪假设置信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strAttendanceSolutionId">配置带薪假的考勤方案主键索引</param>
        /// <returns>返回带薪假设置信息</returns>
        public IQueryable<T_HR_FREELEAVEDAYSET> GetFreeLeaveDaySetRdListForAttendanceSolution(string strOrderBy, string strAttendanceSolutionId)
        {
            var q = from v in GetObjects().Include("T_HR_LEAVETYPESET")
                    join s in GetObjects<T_HR_LEAVETYPESET>() on v.T_HR_LEAVETYPESET.LEAVETYPESETID equals s.LEAVETYPESETID
                    join r in GetObjects<T_HR_ATTENDFREELEAVE>().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_LEAVETYPESET") on v.T_HR_LEAVETYPESET.LEAVETYPESETID equals r.T_HR_LEAVETYPESET.LEAVETYPESETID
                    where r.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == strAttendanceSolutionId
                    select v;

            StringBuilder strFreeLeaveDaySetIds = new StringBuilder();

            foreach (T_HR_FREELEAVEDAYSET item in q)
            {
                strFreeLeaveDaySetIds.Append(item.FREELEAVEDAYSETID + ",");
            }

            List<object> objArgs = new List<object>();
            objArgs.Add(strFreeLeaveDaySetIds.ToString());

            string strFilter = " FREELEAVEDAYSETID.Contains(@0)";

            return GetFreeLeaveDaySetRdListByMultSearch(strOrderBy, strFilter, objArgs.ToArray());            
        }
    }
}