using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;

using System.Data;
using SMT.Foundation.Core;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT_HRM_EFModel;

namespace SMT.HRM.DAL
{
    public class ReportsDAL : CommDal<T_HR_EMPLOYEE>
    {
        /// <summary>
        /// 查询员工基本信息
        /// </summary>
        /// <param name="strOrderBy"></param>
        /// <param name="strFilter"></param>
        /// <param name="objArgs"></param>
        /// <returns></returns>
        public DataTable GetDataForExportEmployeesBasicInfo(string strOrderBy, string strFilter, params object[] objArgs)
        {
            string strSql = "select * from t_hr_employeechangehistory t where ";
            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
            {
                strFilter = strFilter.Replace("==", "=");
                for (int i = objArgs.Count() - 1; i >= 0; i--)
                {
                    strFilter = strFilter.Replace("@" + i.ToString(), "'" + objArgs[i].ToString() + "'");
                }
                //strSql += " and " + strFilter;
                strSql +=  strFilter;
            }

            strSql += " order by "+  strOrderBy;
            strSql = strSql.Replace("CREATEUSERID","OWNERID");

            DataTable dtRes = new DataTable();
            dtRes = this.GetDataTableByCustomerSql(strSql);
             
            return dtRes;
        }

        /// <summary>
        /// 根据公司ID查询员工基本信息并判断此公司是否有子公司
        /// </summary>
        /// <param name="CompanyID">公司ID</param>
        /// <param name="style">类型</param>
        /// <returns></returns>
        public DataTable GetDataEmployeecInfo(string CompanyID, string style, string StartTime, string EndTime)
        {
            try
            {
                DataTable dt = new DataTable();
                StringBuilder sql = new StringBuilder();
                sql.Append("select p.EMPLOYEEID,p.marriage,p.employeecname,p.sex,p.TOPEDUCATION,p.BIRTHDAY,t.ENTRYDATE from T_HR_EMPLOYEEENTRY t");
                sql.Append(" inner join  T_HR_EMPLOYEE p on t.employeeid=p.employeeid where");
                //0包含子公司查询
                if (style=="0")
                {
                    sql.Append(" (p.OWNERCOMPANYID in (select t.COMPANYID from t_hr_company t where t.FATHERID='" + CompanyID + "') or");
                    sql.Append(" p.OWNERCOMPANYID='" + CompanyID + "')");
                }
                //1不包含子公司查询
                else if (style == "1")
                {
                    sql.Append(" p.OWNERCOMPANYID='" + CompanyID + "'");
                }
                sql.Append(" and t.entrydate between to_date('" + StartTime + "','YYYY-MM-DD')" + " and to_date('" + EndTime + "','YYYY-MM-DD')");
                sql.Append(" and p.EMPLOYEESTATE !='2'");
                dt = this.GetDataTableByCustomerSql(sql.ToString());
                if (dt != null)
                {
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("根据公司ID查询员工错误-GetDataEmployeecInfo：" + ex.ToString());
                throw ex;
            }
            
        }

        /// <summary>
        /// 根据年月查询集团各产业的信息
        /// </summary>
        /// <param name="CompanyID"></param>
        /// <param name="style"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        public DataTable GetEmployeecByYearMM(string CompanyID, string style, string StartTime, string EndTime) 
        {
            try
            {
                DataTable dt = new DataTable();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT  k.EMPLOYEEID,k.marriage,k.employeecname,k.sex,k.TOPEDUCATION,k.BIRTHDAY,k.ENTRYDATE,k.OWNERCOMPANYID,j.ename,j.cname FROM t_hr_company j");
                sql.Append(" INNER JOIN (");
                sql.Append(" select p.EMPLOYEEID,p.marriage,p.employeecname,p.sex,p.TOPEDUCATION,p.BIRTHDAY,t.ENTRYDATE,T.OWNERCOMPANYID from T_HR_EMPLOYEEENTRY t");
                sql.Append(" inner join  T_HR_EMPLOYEE p on t.employeeid=p.employeeid where");

                //0包含子公司查询
                if (style == "0")
                {
                    sql.Append(" (p.OWNERCOMPANYID in (select t.COMPANYID from t_hr_company t where t.FATHERID='" + CompanyID + "') or");
                    sql.Append(" p.OWNERCOMPANYID='" + CompanyID + "')");
                }
                //1不包含子公司查询
                else if (style == "1")
                {
                    sql.Append(" p.OWNERCOMPANYID='" + CompanyID + "'");
                }
                sql.Append(" and t.entrydate between to_date('" + StartTime + "','YYYY-MM-DD')" + " and to_date('" + EndTime + "','YYYY-MM-DD')");
                sql.Append(" and p.EMPLOYEESTATE !='2'");
                sql.Append(" ORDER BY T.OWNERCOMPANYID) k on j.companyid =k.OWNERCOMPANYID");
                dt = this.GetDataTableByCustomerSql(sql.ToString());
                if (dt != null)
                {
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("根据年月查询集团各产业的信息-GetEmployeecByYearMM：" + ex.ToString());
                throw ex;
            }
            
        }


    }
}
