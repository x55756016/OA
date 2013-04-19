/*
 * 文件名：ShiftDefineDAL.cs
 * 作  用：考勤班次 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-3-3 11:40:00
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT_HRM_EFModel;

namespace SMT.HRM.DAL
{
    public class ShiftDefineDAL: CommDal<T_HR_SHIFTDEFINE>
    {
        public ShiftDefineDAL()
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
        /// 获取指定条件的考勤班次信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤班次信息</returns>
        public T_HR_SHIFTDEFINE GetShiftDefineRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects()
                    select v;

            if (objArgs.Count() > 0)
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取考勤班次信息
        /// </summary>
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回考勤班次信息</returns>
        public IQueryable<T_HR_SHIFTDEFINE> GetShiftDefineListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from s in GetObjects()
                    select s;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }


            return q.OrderBy(strOrderBy);
        }

    }
}
