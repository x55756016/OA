
/*
 * 文件名：EmployeeLevelDayCountDAL.cs
 * 作  用：T_HR_EMPLOYEELEVELDAYCOUNT 数据操作类
 * 创建人：吴鹏
 * 创建时间：2010-4-7 8:43:31
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
    public class EmployeeLevelDayCountDAL : CommDal<T_HR_EMPLOYEELEVELDAYCOUNT>
    {
        public EmployeeLevelDayCountDAL()
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

        /// <summary>
        /// 获取指定条件的T_HR_EMPLOYEELEVELDAYCOUNT信息
        /// </summary>        
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_EMPLOYEELEVELDAYCOUNT信息</returns>
        public T_HR_EMPLOYEELEVELDAYCOUNT GetEmployeeLevelDayCountRdByMultSearch(string strFilter, params object[] objArgs)
        {
            var q = from v in GetObjects()
                    select v;

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q.First();
        }

        /// <summary>
        /// 获取指定条件的T_HR_EMPLOYEELEVELDAYCOUNT信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>返回T_HR_EMPLOYEELEVELDAYCOUNT信息</returns>
        public IQueryable<T_HR_EMPLOYEELEVELDAYCOUNT> GetEmployeeLevelDayCountRdListByMultSearch(string sType, string sValue,
            string strEfficDateFrom, string strEfficDateTo, string strOrderBy, string strFilter, params object[] objArgs)
        {
            var ents = from v in GetObjects()
                       
                       select v;

            switch (sType)
            {
                case "Company":
                    ents = from v in GetObjects()
                           //join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           //join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           //join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           //join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           //join c in GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where v.OWNERCOMPANYID == sValue
                           select v;
                    break;
                case "Department":
                    ents = from v in GetObjects()
                           //join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           //join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           //join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           //join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           where v.OWNERDEPARTMENTID == sValue
                           select v;
                    break;
                case "Post":
                    ents = from v in GetObjects()
                           //join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           //join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           //join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           where v.OWNERPOSTID == sValue
                           select v;
                    break;
                //case "Vacation":
                //    ents = from v in GetObjects()
                //           join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                //           join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                //           join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                //           where p.POSTID == sValue
                //           select v;
                   // break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(strEfficDateFrom))
            {
                DateTime dtStart = new DateTime();
                DateTime.TryParse(strEfficDateFrom, out dtStart);

                ents = ents.Where(v => v.EFFICDATE >= dtStart);

            }

            if (!string.IsNullOrEmpty(strEfficDateTo))
            {
                DateTime dtEnd = new DateTime();
                DateTime.TryParse(strEfficDateTo, out dtEnd);

                if (dtEnd.Hour == 0 && dtEnd.Minute == 0)
                {
                    dtEnd = dtEnd.AddDays(1).AddMinutes(-1);
                }

                ents = ents.Where(v => v.EFFICDATE <= dtEnd);
            }

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                ents = ents.Where(strFilter, objArgs);
            }

            var ent1 = from ent in ents.ToList()
                       orderby  ent.EMPLOYEENAME
                       select ent;
            //return ents.ToList().ord.AsQueryable();// (strOrderBy);
            return ent1.AsQueryable();
        }

        /// <summary>
        /// 根据请假开始时间、结束时间、请假类型ID，查询请假记录表
        /// </summary>
        /// <param name="employeeid"></param>
        /// <param name="leavetypesetid"></param>
        /// <param name="OWNERCOMPANYID"></param>
        /// <param name="strEfficDateFrom"></param>
        /// <param name="strEfficDateTo"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEELEAVERECORD> GetEmployeeleaverecordByMultSearchId(string employeeid, string leavetypesetid, string OWNERCOMPANYID, 
            string strEfficDateFrom, string strEfficDateTo) 
        {
            var ent = from v in GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                      where v.EMPLOYEEID == employeeid
                      && v.T_HR_LEAVETYPESET.LEAVETYPESETID == leavetypesetid
                      && v.CHECKSTATE =="2"
                      //orderby v.STARTDATETIME
                      select v;
            if (ent.Count()>0)
            {
                if (!string.IsNullOrEmpty(strEfficDateFrom) && !string.IsNullOrEmpty(strEfficDateTo))
                {
                    DateTime startDate = DateTime.Parse(strEfficDateFrom);
                    DateTime endDate = DateTime.Parse(strEfficDateTo);
                    ent = ent.Where(v => v.STARTDATETIME >= startDate && v.ENDDATETIME <= endDate);
                    //ent=ent.o
                    if (ent.Count()>0)
                    {
                        return ent.OrderBy("STARTDATETIME");
                    }
                }
            }
            return null;
        }
    }
}