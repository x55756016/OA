
/*
 * 文件名：AbnormRecordDAL.cs
 * 作  用：T_HR_EMPLOYEEABNORMRECORD 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-6-12 10:23:43
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
    public class AbnormRecordDAL : CommDal<T_HR_EMPLOYEEABNORMRECORD>
    {
        public AbnormRecordDAL()
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

            var q = from v in GetObjects().Include("T_HR_ATTENDANCERECORD")
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
        /// 获取指定条件的员工异常记录信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_EMPLOYEEABNORMRECORD信息</returns>
        public T_HR_EMPLOYEEABNORMRECORD GetAbnormRecordRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_ATTENDANCERECORD")
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的员工异常记录信息   weirui 2012/9/12 方法做了修改 添加开始时间~结束时间
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strCurDateMonth">异常记录月份</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回员工异常记录信息</returns>
        public IQueryable<T_HR_EMPLOYEEABNORMRECORD> GetAbnormRecordRdListByMultSearch(string strOrderBy, string strCurStartDate, string strCurEndDate, string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects().Include("T_HR_ATTENDANCERECORD")
                    select v;

            if (!string.IsNullOrEmpty(strCurStartDate) && !string.IsNullOrEmpty(strCurEndDate))
            {
                //DateTime dtStart = DateTime.Parse(strCurDateMonth + "-1");
                //DateTime dtEnd = dtStart.AddMonths(1).AddDays(-1);
                DateTime dtStart = DateTime.Parse(strCurStartDate);
                DateTime dtEnd = DateTime.Parse(strCurEndDate);

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