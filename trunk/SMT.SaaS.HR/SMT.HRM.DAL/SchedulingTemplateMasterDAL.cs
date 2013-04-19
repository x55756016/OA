
/*
 * 文件名：SchedulingTemplateMasterDAL.cs
 * 作  用：排班模板 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-3-8 16:02:31
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
    public class SchedulingTemplateMasterDAL : CommDal<T_HR_SCHEDULINGTEMPLATEMASTER>
    {
        public SchedulingTemplateMasterDAL()
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

            var q = from v in GetObjects().Include("T_HR_SCHEDULINGTEMPLATEDETAIL")
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
        /// 根据考勤方案主键索引，获取其关联排版
        /// </summary>
        /// <param name="strAttendanceSolutionId"></param>
        /// <returns></returns>
        public T_HR_SCHEDULINGTEMPLATEMASTER GetSchedulingTemplateMasterByAttSolID(string strAttendanceSolutionId)
        {
            var q = from a in GetObjects<T_HR_ATTENDANCESOLUTION>().Include("T_HR_SCHEDULINGTEMPLATEMASTER")
                    where a.ATTENDANCESOLUTIONID == strAttendanceSolutionId
                    select a.T_HR_SCHEDULINGTEMPLATEMASTER;
            T_HR_SCHEDULINGTEMPLATEMASTER ent = q.FirstOrDefault();

            return ent;
        }

        /// <summary>
        /// 获取指定条件的排班模板信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回排班模板信息</returns>
        public T_HR_SCHEDULINGTEMPLATEMASTER GetSchedulingTemplateMasterRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_SCHEDULINGTEMPLATEDETAIL")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的排班模板信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回排班模板信息</returns>
        public IQueryable<T_HR_SCHEDULINGTEMPLATEMASTER> GetSchedulingTemplateMasterRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_SCHEDULINGTEMPLATEDETAIL")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.OrderBy(strOrderBy);
        }

        
    }
}