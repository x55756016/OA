/*
 * 文件名：ClockInRecordDAL.cs
 * 作  用：员工加班数据操作类
 * 创建人：吴鹏
 * 创建时间：2010年1月26日, 14:24:04
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Dynamic;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using SMT.HRM.CustomModel;
using SMT_HRM_EFModel;

namespace SMT.HRM.DAL
{
    public class OverTimeRecordDAL: CommDal<T_HR_EMPLOYEEOVERTIMERECORD>
    {
        public OverTimeRecordDAL()
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
            var q = from o in GetObjects()
                    select o;

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
        /// 获取指定条件的员工加班基本信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>员工加班信息</returns>
        public T_HR_EMPLOYEEOVERTIMERECORD GetOverTimeRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from o in GetObjects()
                    select o;

            if (objArgs.Count() > 0)
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的员工加班信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>员工加班信息</returns>
        public IQueryable<V_HR_EMPLOYEEOVERTIMERECORD> GetOverTimeRdListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from o in GetObjects()
                    join e in GetObjects<T_HR_EMPLOYEE>() on o.EMPLOYEEID equals e.EMPLOYEEID
                    join ep in GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                    join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                    join pc in GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pc.POSTDICTIONARYID
                    join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                    join dc in GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dc.DEPARTMENTDICTIONARYID
                    join cp in GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals cp.COMPANYID
                    select new V_HR_EMPLOYEEOVERTIMERECORD()
                    {
                        OVERTIMERECORDID = o.OVERTIMERECORDID,
                        STARTDATETIME = o.STARTDATETIME,
                        ENDDATETIME = o.ENDDATETIME,
                        OVERTIMEHOURS = o.OVERTIMEHOURS,
                        OVERTIMECATE = o.OVERTIMECATE,
                        PAYCATEGORY = o.PAYCATEGORY,
                        CHECKSTATE = o.CHECKSTATE,
                        REMARK = o.REMARK,
                        EMPLOYEEID = e.EMPLOYEEID,
                        EMPLOYEENAME = e.EMPLOYEECNAME,
                        POSITIONNAME = p.DEPARTMENTNAME,
                        EMPLOYEECODE = e.EMPLOYEECODE,
                        DEPARTMENTID = d.DEPARTMENTID,
                        DEPARTMENTNAME = dc.DEPARTMENTNAME,
                        COMPANYID = cp.COMPANYID,
                        COMPANYNAME = cp.CNAME
                    };

            if (objArgs.Count() > 0)
            {
                q = q.Where(strFilter, objArgs);
            }


            return q.OrderBy(strOrderBy);
        }

        /// <summary>
        /// 根据主键索引，获取员工加班信息
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <returns></returns>
        public V_HR_EMPLOYEEOVERTIMERECORD GetOverTimeRdByID(string strOverTimeRecordId)
        {
            var q = from o in GetObjects()
                    join e in GetObjects<T_HR_EMPLOYEE>() on o.EMPLOYEEID equals e.EMPLOYEEID
                    join p in GetObjects<T_HR_POST>() on o.OWNERPOSTID equals p.POSTID
                    join pc in GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pc.POSTDICTIONARYID
                    join d in GetObjects<T_HR_DEPARTMENT>() on o.OWNERDEPARTMENTID equals d.DEPARTMENTID
                    join dc in GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dc.DEPARTMENTDICTIONARYID
                    join cp in GetObjects<T_HR_COMPANY>() on o.OWNERCOMPANYID equals cp.COMPANYID
                    where o.OVERTIMERECORDID == strOverTimeRecordId
                    select new V_HR_EMPLOYEEOVERTIMERECORD()
                    {
                        OVERTIMERECORDID = o.OVERTIMERECORDID,
                        STARTDATETIME = o.STARTDATETIME,
                        ENDDATETIME = o.ENDDATETIME,
                        OVERTIMEHOURS = o.OVERTIMEHOURS,
                        OVERTIMECATE = o.OVERTIMECATE,
                        PAYCATEGORY = o.PAYCATEGORY,
                        CHECKSTATE = o.CHECKSTATE,
                        REMARK = o.REMARK,
                        EMPLOYEEID = e.EMPLOYEEID,
                        EMPLOYEENAME = e.EMPLOYEECNAME,
                        POSITIONNAME = p.DEPARTMENTNAME,
                        EMPLOYEECODE = e.EMPLOYEECODE,
                        DEPARTMENTID = d.DEPARTMENTID,
                        DEPARTMENTNAME = dc.DEPARTMENTNAME,
                        COMPANYID = cp.COMPANYID,
                        COMPANYNAME = cp.CNAME
                    };

            return q.First();
        }        
    }
}
