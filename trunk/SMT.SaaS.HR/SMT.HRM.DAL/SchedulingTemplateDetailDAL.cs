
/*
 * 文件名：SchedulingTemplateDetailDAL.cs
 * 作  用：排班模板明细 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-3-10 19:27:09
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
    public class SchedulingTemplateDetailDAL : CommDal<T_HR_SCHEDULINGTEMPLATEDETAIL>
    {
        public SchedulingTemplateDetailDAL()
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

            var q = from v in GetObjects().Include("T_HR_SCHEDULINGTEMPLATEMASTER").Include("T_HR_SHIFTDEFINE")                    
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
        /// 获取指定条件的排班模板明细信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回排班模板明细信息</returns>
        public T_HR_SCHEDULINGTEMPLATEDETAIL GetSchedulingTemplateDetailRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_SCHEDULINGTEMPLATEMASTER").Include("T_HR_SHIFTDEFINE")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的排班模板明细信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回排班模板明细信息</returns>
        public IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> GetSchedulingTemplateDetailRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_SCHEDULINGTEMPLATEMASTER").Include("T_HR_SHIFTDEFINE")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }


            return q.OrderBy(strOrderBy);
        }

        /// <summary>
        /// 根据考勤方案获取对应的排班模板及班次定义
        /// </summary>
        /// <param name="strAttendanceSolutionId"></param>
        /// <returns></returns>
        public IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> GetTemplateDetailRdListByAttendanceSolutionId(string strAttendanceSolutionId)
        {
            if (string.IsNullOrEmpty(strAttendanceSolutionId))
            {
                return null;
            }

            var q = from s in GetObjects().Include("T_HR_SCHEDULINGTEMPLATEMASTER").Include("T_HR_SHIFTDEFINE")
                    join a in GetObjects<T_HR_ATTENDANCESOLUTION>().Include("T_HR_SCHEDULINGTEMPLATEMASTER") on s.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID equals a.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID
                    where a.ATTENDANCESOLUTIONID == strAttendanceSolutionId
                    orderby s.SCHEDULINGDATE
                    select s;

            StringBuilder strTemplateDetailIDs = new StringBuilder();

            foreach (T_HR_SCHEDULINGTEMPLATEDETAIL item in q)
            {
                strTemplateDetailIDs.Append(item.TEMPLATEDETAILID + ",");
            }

            List<object> objArgs = new List<object>();
            objArgs.Add(strTemplateDetailIDs.ToString());

            string strFilter = " TEMPLATEDETAILID.Contains(@0)";

            string strOrderBy = " SCHEDULINGDATE ";

            return GetSchedulingTemplateDetailRdListByMultSearch(strOrderBy, strFilter, objArgs.ToArray()); 
        }
    }
}