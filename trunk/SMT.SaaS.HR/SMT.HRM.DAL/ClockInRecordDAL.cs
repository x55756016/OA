/*
 * 文件名：ClockInRecordDAL.cs
 * 作  用：日常考勤打卡数据类
 * 创建人：吴鹏
 * 创建时间：2010年1月19日, 17:52:38
 * 修改人：
 * 修改时间：
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using SMT.Foundation.Core;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT_HRM_EFModel;
using SMT.HRM.CustomModel;

namespace SMT.HRM.DAL
{
    public class ClockInRecordDAL : CommDal<T_HR_EMPLOYEECLOCKINRECORD>
    {
        /// <summary>
        /// 根据指纹编号，打卡日期及时间，检查是否存在记录
        /// </summary>
        /// <param name="strFingerPrintID">指纹编号</param>
        /// <param name="dtPunchDate">打卡日期</param>
        /// <param name="strPunchTime">打卡时间</param>
        /// <returns>true/false</returns>
        public bool IsExistsRd(string strFingerPrintID, DateTime? dtPunchDate, string strPunchTime)
        {
            bool flag = false;
            DateTime dtCheck = new DateTime();

            if (string.IsNullOrEmpty(strFingerPrintID))
            {
                return false;
            }

            if (dtPunchDate == null)
            {
                return false;
            }

            if (dtPunchDate.Value.CompareTo(dtCheck) <= 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(strPunchTime))
            {
                return false;
            }

            var q = from v in GetObjects()
                    where v.FINGERPRINTID == strFingerPrintID && v.PUNCHDATE == dtPunchDate && v.PUNCHTIME == strPunchTime
                    select v;

            if (q.Count() > 0)
            {
                flag = true;
            }

            return flag;
        }

        /// <summary>
        /// 通过执行sql的方式，查询打卡记录
        /// </summary>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="strPunchDateFrom"></param>
        /// <param name="strPunchDateTo"></param>
        /// <param name="strTimeFrom"></param>
        /// <param name="strTimeTo"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="strFilter"></param>
        /// <param name="objArgs"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEECLOCKINRECORD> GetClockInRdListBySql(string sType, string sValue, string strPunchDateFrom, string strPunchDateTo, string strTimeFrom, string strTimeTo,
            string strOrderBy, string strFilter, int pageIndex, int pageSize, ref int pageCount, params object[] objArgs)
        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strSqlCount = new StringBuilder();
            strSql.Append("Select distinct t.* from t_hr_employeeclockinrecord t");
            strSqlCount.Append("Select count(distinct t.CLOCKINRECORDID) from t_hr_employeeclockinrecord t");

            string strCheckStates = Convert.ToInt32(CheckStates.Approved).ToString();

            string strActived = Convert.ToInt32(EditStates.Actived).ToString();
            StringBuilder strOrgTemp = new StringBuilder();
            switch (sType)
            {
                case "Company":
                    strOrgTemp.Append(" Inner Join T_HR_EMPLOYEE o on t.EMPLOYEEID = o.EMPLOYEEID");
                    strOrgTemp.Append(" Inner Join T_HR_EMPLOYEEPOST ep on t.EMPLOYEEID = ep.EMPLOYEEID");
                    strOrgTemp.Append(" Inner Join T_HR_POST p on ep.POSTID = p.POSTID");
                    strOrgTemp.Append(" Inner Join T_HR_DEPARTMENT d on p.DEPARTMENTID = d.DEPARTMENTID");
                    strOrgTemp.Append(" Inner Join T_HR_COMPANY c on d.COMPANYID = c.COMPANYID");
                    strOrgTemp.Append(" Where ep.checkstate = '" + strCheckStates + "' and c.COMPANYID='" + sValue + "' and ep.EDITSTATE ='" + strActived + "'");
                    break;
                case "Department":
                    strOrgTemp.Append(" Inner Join T_HR_EMPLOYEE o on t.EMPLOYEEID = o.EMPLOYEEID");
                    strOrgTemp.Append(" Inner Join T_HR_EMPLOYEEPOST ep on t.EMPLOYEEID = ep.EMPLOYEEID");
                    strOrgTemp.Append(" Inner Join T_HR_POST p on ep.POSTID = p.POSTID");
                    strOrgTemp.Append(" Inner Join T_HR_DEPARTMENT d on p.DEPARTMENTID = d.DEPARTMENTID");
                    strOrgTemp.Append(" Where ep.checkstate = '" + strCheckStates + "' and d.DEPARTMENTID='" + sValue + "' and ep.EDITSTATE ='" + strActived + "'");
                    break;
                case "Post":
                    strOrgTemp.Append(" Inner Join T_HR_EMPLOYEE o on t.EMPLOYEEID = o.EMPLOYEEID");
                    strOrgTemp.Append(" Inner Join T_HR_EMPLOYEEPOST ep on t.EMPLOYEEID = ep.EMPLOYEEID");
                    strOrgTemp.Append(" Inner Join T_HR_POST p on ep.POSTID = p.POSTID");
                    strOrgTemp.Append(" Where ep.checkstate = '" + strCheckStates + "' and p.POSTID='" + sValue + "' and ep.EDITSTATE ='" + strActived + "'");
                    break;
                default:
                    break;
            }

            string strOrgSql = strOrgTemp.ToString();

            if (!string.IsNullOrWhiteSpace(strOrgSql))
            {
                strSql.Append(strOrgSql);
                strSqlCount.Append(strOrgSql);
            }

            if (!string.IsNullOrEmpty(strPunchDateFrom))
            {
                DateTime dtStart = new DateTime();
                DateTime.TryParse(strPunchDateFrom, out dtStart);

                if (strSql.ToString().Contains("Where") && strSqlCount.ToString().Contains("Where"))
                {
                    strSql.Append(" and ");
                    strSqlCount.Append(" and ");
                }
                else
                {
                    strSql.Append(" Where ");
                    strSqlCount.Append(" Where ");
                }

                strSql.Append("t.PUNCHDATE >= To_date('" + dtStart.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-MM-dd HH24:mi:ss')");
                strSqlCount.Append("t.PUNCHDATE >= To_date('" + dtStart.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-MM-dd HH24:mi:ss')");
            }

            if (!string.IsNullOrEmpty(strPunchDateTo))
            {
                DateTime dtEnd = new DateTime();
                DateTime.TryParse(strPunchDateTo, out dtEnd);

                if (dtEnd.Hour == 0 && dtEnd.Minute == 0)
                {
                    dtEnd = dtEnd.AddDays(1).AddMinutes(-1);
                }

                if (strSql.ToString().Contains("Where") && strSqlCount.ToString().Contains("Where"))
                {
                    strSql.Append(" and ");
                    strSqlCount.Append(" and ");
                }
                else
                {
                    strSql.Append(" Where ");
                    strSqlCount.Append(" Where ");
                }

                strSql.Append("t.PUNCHDATE <= To_date('" + dtEnd.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-MM-dd HH24:mi:ss')");
                strSqlCount.Append("t.PUNCHDATE <= To_date('" + dtEnd.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-MM-dd HH24:mi:ss')");
            }


            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                if (strSql.ToString().Contains("Where") && strSqlCount.ToString().Contains("Where"))
                {
                    strSql.Append(" and ");
                    strSqlCount.Append(" and ");
                }
                else
                {
                    strSql.Append(" Where ");
                    strSqlCount.Append(" Where ");
                }

                strFilter = strFilter.ToUpper();

                if (strFilter.Contains("CREATEUSERID"))
                {
                    strFilter = strFilter.Replace("CREATEUSERID", "t.CREATEUSERID");
                }

                if (strFilter.Contains("OWNERCOMPANYID"))
                {
                    strFilter = strFilter.Replace("OWNERCOMPANYID", "t.OWNERCOMPANYID");
                }

                if (strFilter.Contains("OWNERDEPARTMENTID"))
                {
                    strFilter = strFilter.Replace("OWNERDEPARTMENTID", "t.OWNERDEPARTMENTID");
                }

                if (strFilter.Contains("OWNERPOSTID"))
                {
                    strFilter = strFilter.Replace("OWNERPOSTID", "t.OWNERPOSTID");
                }

                if (strFilter.Contains("OWNERID"))
                {
                    strFilter = strFilter.Replace("OWNERID", "t.OWNERID");
                }

                strFilter = strFilter.Replace("==", "=");

                for (int i = objArgs.Count() - 1; i >= 0; i--)
                {
                    strFilter = strFilter.Replace("@" + i.ToString(), "'" + objArgs[i].ToString() + "'");
                }

                strSql.Append(strFilter);
                strSqlCount.Append(strFilter);
            }

            if (!string.IsNullOrWhiteSpace(strTimeFrom) && !string.IsNullOrWhiteSpace(strTimeTo))
            {
                DateTime dtTimeFrom = new DateTime();
                DateTime dtTimeTo = new DateTime();

                DateTime.TryParse(DateTime.Now.ToString("yyyy-MM-dd") + " " + strTimeFrom, out dtTimeFrom);
                DateTime.TryParse(DateTime.Now.ToString("yyyy-MM-dd") + " " + strTimeTo, out dtTimeTo);

                if (dtTimeFrom < dtTimeTo)
                {
                    StringBuilder sbIds = new StringBuilder();
                    if (dtTimeFrom.Hour == dtTimeTo.Hour)
                    {
                        if (strSql.ToString().Contains("Where") && strSqlCount.ToString().Contains("Where"))
                        {
                            strSql.Append(" and ");
                            strSqlCount.Append(" and ");
                        }
                        else
                        {
                            strSql.Append(" Where ");
                            strSqlCount.Append(" Where ");
                        }
                        strSql.Append("(to_number(to_char(t.punchdate, 'HH24')) =" + dtTimeFrom.Hour + " and to_number(to_char(t.punchdate, 'mi')) >= " + dtTimeFrom.Minute + " and to_number(to_char(t.punchdate, 'mi')) <= " + dtTimeTo.Minute + ")");
                        strSqlCount.Append("(to_number(to_char(t.punchdate, 'HH24')) =" + dtTimeFrom.Hour + " and to_number(to_char(t.punchdate, 'mi')) >= " + dtTimeFrom.Minute + "and to_number(to_char(t.punchdate, 'mi')) <= " + dtTimeTo.Minute + ")");
                    }
                    else if (dtTimeFrom.Hour < dtTimeTo.Hour)
                    {
                        if (strSql.ToString().Contains("Where") && strSqlCount.ToString().Contains("Where"))
                        {
                            strSql.Append(" and ");
                            strSqlCount.Append(" and ");
                        }
                        else
                        {
                            strSql.Append(" Where ");
                            strSqlCount.Append(" Where ");
                        }
                        strSql.Append("((to_number(to_char(t.punchdate, 'HH24')) >" + dtTimeFrom.Hour + " and to_number(to_char(t.punchdate, 'HH24')) < " + dtTimeTo.Hour + ") or (to_number(to_char(t.punchdate, 'HH24')) = " + dtTimeTo.Hour + " and to_number(to_char(t.punchdate, 'mi')) <= " + dtTimeTo.Minute + ") or (to_number(to_char(t.punchdate, 'HH24')) = " + dtTimeFrom.Hour + " and to_number(to_char(t.punchdate, 'mi')) >= " + dtTimeFrom.Minute + "))");
                        strSqlCount.Append("((to_number(to_char(t.punchdate, 'HH24')) >" + dtTimeFrom.Hour + " and to_number(to_char(t.punchdate, 'HH24')) < " + dtTimeTo.Hour + ") or (to_number(to_char(t.punchdate, 'HH24')) = " + dtTimeTo.Hour + " and to_number(to_char(t.punchdate, 'mi')) <= " + dtTimeTo.Minute + ") or (to_number(to_char(t.punchdate, 'HH24')) = " + dtTimeFrom.Hour + " and to_number(to_char(t.punchdate, 'mi')) >= " + dtTimeFrom.Minute + "))");
                    }
                }
            }

            strSql.Append(" order by " + strOrderBy);
            object objResCount = ExecuteCustomerSql(strSqlCount.ToString());
            if (objResCount == null)
            {
                return null;
            }

            int count = 0;
            int.TryParse(objResCount.ToString(), out count);
            pageCount = count / pageSize;
            int tmp = count % pageSize;

            pageCount = pageCount + (tmp > 0 ? 1 : 0);
            if (pageIndex > pageCount)
                pageIndex = 2;

            return GetListEntitiesWithPagingBySql<T_HR_EMPLOYEECLOCKINRECORD>(strSql.ToString(), pageIndex, pageSize);
        }

        /// <summary>
        /// 获取指定条件的公共假期设置信息
        /// </summary>  
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的</param>
        /// <param name="strPunchDateFrom">起始时间(截取打卡日期)</param>
        /// <param name="strPunchDateTo">结束时间(截取打卡日期)</param>
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>公共假期设置信息</returns>
        public IQueryable<T_HR_EMPLOYEECLOCKINRECORD> GetClockInRdListByMultSearch(string sType, string sValue, string strPunchDateFrom, string strPunchDateTo,
            string strOrderBy, string strFilter, params object[] objArgs)
        {

            var ents = from v in GetObjects()
                       select v;

            switch (sType)
            {
                case "Company":
                    ents = from v in GetObjects()
                           join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue
                           select v;
                    break;
                case "Department":
                    ents = from v in GetObjects()
                           join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           where d.DEPARTMENTID == sValue
                           select v;
                    break;
                case "Post":
                    ents = from v in GetObjects()
                           join o in GetObjects<T_HR_EMPLOYEE>() on v.EMPLOYEEID equals o.EMPLOYEEID
                           join ep in GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           where p.POSTID == sValue
                           select v;
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(strPunchDateFrom))
            {
                DateTime dtStart = new DateTime();
                DateTime.TryParse(strPunchDateFrom, out dtStart);

                ents = ents.Where(v => v.PUNCHDATE >= dtStart);

            }

            if (!string.IsNullOrEmpty(strPunchDateTo))
            {
                DateTime dtEnd = new DateTime();
                DateTime.TryParse(strPunchDateTo, out dtEnd);

                if (dtEnd.Hour == 0 && dtEnd.Minute == 0)
                {
                    dtEnd = dtEnd.AddDays(1).AddMinutes(-1);
                }

                ents = ents.Where(v => v.PUNCHDATE <= dtEnd);
            }

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                ents = ents.Where(strFilter, objArgs);
            }

            //ents = ents.OrderBy(strOrderBy);
            return ents.OrderBy(strOrderBy);
        }

        /// <summary>
        /// 获取指定条件的员工日常打卡信息
        /// </summary>        
        /// <param name="strOrderBy">排序字段</param>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>员工日常打卡信息</returns>
        public IQueryable<CLOCKINRECORDINFO> GetClockInInfoListByMultSearch(string strOrderBy, string strFilter, params object[] objArgs)
        {
            var q = from c in GetObjects()
                    join e in GetObjects<T_HR_EMPLOYEE>() on c.EMPLOYEEID equals e.EMPLOYEEID
                    join ep in GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                    join p in GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                    join pc in GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pc.POSTDICTIONARYID
                    join d in GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                    join dc in GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dc.DEPARTMENTDICTIONARYID
                    join cp in GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals cp.COMPANYID
                    select new CLOCKINRECORDINFO()
                    {
                        CLOCKINRECORDID = c.CLOCKINRECORDID,
                        EMPLOYEEID = e.EMPLOYEEID,
                        EMPLOYEENAME = e.EMPLOYEECNAME,
                        POSITIONNAME = p.DEPARTMENTNAME,
                        EMPLOYEECODE = e.EMPLOYEECODE,
                        DEPARTMENTID = d.DEPARTMENTID,
                        DEPARTMENTNAME = dc.DEPARTMENTNAME,
                        COMPANYID = cp.COMPANYID,
                        COMPANYNAME = cp.CNAME,
                        CLOCKINDATE = c.PUNCHDATE,
                        CLOCKINTIME = c.PUNCHTIME
                    };

            if (objArgs.Count() > 0)
            {
                q = q.Where(strFilter, objArgs);
            }

            q = q.OrderBy(strOrderBy);

            return q;
        }

        /// <summary>
        /// 检查特点时间的打卡记录是否存在，并返回查询的记录
        /// </summary>
        /// <param name="dtPunchDate">打开日期</param>
        /// <param name="dtPunchTime">打卡时间</param>
        /// <param name="strFilter">查询条件</param>
        /// <param name="objArgs">查询参数</param>
        /// <returns>返回查询的记录</returns>
        public IQueryable<T_HR_EMPLOYEECLOCKINRECORD> GetClockInRdListByPunchDate(DateTime? dtPunchDate, string dtPunchTime, string strFilter, params object[] objArgs)
        {
            DateTime dtCheck = new DateTime();

            var q = from v in GetObjects()
                    select v;

            if (dtPunchDate != null)
            {
                if (dtPunchDate.Value.CompareTo(dtCheck) > 0)
                {
                    q = q.Where(c => c.PUNCHDATE == dtPunchDate);
                }
            }


            if (!string.IsNullOrEmpty(dtPunchTime))
            {

                q = q.Where(c => c.PUNCHTIME == dtPunchTime);

            }

            if (objArgs.Count() > 0 && string.IsNullOrEmpty(strFilter))
            {
                q = q.Where(strFilter, objArgs);
            }

            return q;
        }


    }
}
