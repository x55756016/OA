using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.CustomModel;
using SMT_HRM_EFModel;
using System.Data;

namespace SMT.HRM.DAL
{
    public class PersonalLandDetailDAL : CommDal<V_LandDetail>
    {
        public List<V_LandDetail> GetPersonalLandDetailList(string strOrderBy, int pageIndex, int pageSize,
           ref int pageCount, ref int iLoginTimes, ref int iLoginPersonCount, string strFilter, params object[] objArgs)
        {
            List<V_LandDetail> entResList = new List<V_LandDetail>();
            try
            {
                StringBuilder strSql = new StringBuilder();
                StringBuilder strSqlCount = new StringBuilder();

                strSql.Append(" select * from smtsystem.t_sys_userloginrecord p ");

                strSqlCount.Append("select count(*) from smtsystem.t_sys_userloginrecord p");

                string strCountFilter = string.Empty;
                if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
                {
                    strCountFilter = string.Copy(strFilter);

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
                        strFilter = strFilter.Replace("CREATEUSERID", "OWNERID");
                    }

                    if (strFilter.Contains("OWNERCOMPANYID"))
                    {
                        strFilter = strFilter.Replace("OWNERCOMPANYID", "p.OWNERCOMPANYID");
                    }

                    if (strFilter.Contains("OWNERDEPARTMENTID"))
                    {
                        strFilter = strFilter.Replace("OWNERDEPARTMENTID", "p.OWNERDEPARTMENTID");
                    }

                    if (strFilter.Contains("OWNERPOSTID"))
                    {
                        strFilter = strFilter.Replace("OWNERPOSTID", "p.OWNERPOSTID");
                    }

                    if (strFilter.Contains("OWNERID"))
                    {
                        strFilter = strFilter.Replace("OWNERID", "p.OWNERID");
                    }

                    strFilter = strFilter.Replace("==", "=");

                    for (int i = objArgs.Count() - 1; i >= 0; i--)
                    {
                        strFilter = strFilter.Replace("@" + i.ToString(), "'" + objArgs[i].ToString() + "'");
                    }

                    strSql.Append(strFilter);
                    strSqlCount.Append(strFilter);
                }

                strSql.Append(" order by " + strOrderBy);

                object objResCount = ExecuteCustomerSql(strSqlCount.ToString());
                if (objResCount == null)
                {
                    return null;
                }

                int count = 0;
                int.TryParse(objResCount.ToString(), out count);
                iLoginTimes = count;
                GetLoginPersonCount(ref iLoginPersonCount, strCountFilter, objArgs);
                pageCount = count / pageSize;
                int tmp = count % pageSize;

                pageCount = pageCount + (tmp > 0 ? 1 : 0);
                if (pageIndex > pageCount)
                    pageIndex = 2;

                return GetListEntitiesWithPagingBySql<V_LandDetail>(strSql.ToString(), pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取登陆记录统计失败，错误原因：" + ex.ToString());
            }
            return entResList;
        }

        public System.Data.DataTable GetPersonalLandDetailForExport(string strOrderBy, string strFilter, params object[] objArgs)
        {
            DataTable dtRes = new DataTable();
            try
            {
                StringBuilder strSql = new StringBuilder();

                strSql.Append(" select * from smtsystem.t_sys_userloginrecord p ");

                if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
                {
                    if (strSql.ToString().Contains("Where"))
                    {
                        strSql.Append(" and ");
                    }
                    else
                    {
                        strSql.Append(" Where ");
                    }

                    strFilter = strFilter.ToUpper();

                    if (strFilter.Contains("CREATEUSERID"))
                    {
                        strFilter = strFilter.Replace("CREATEUSERID", "OWNERID");
                    }

                    if (strFilter.Contains("OWNERCOMPANYID"))
                    {
                        strFilter = strFilter.Replace("OWNERCOMPANYID", "p.OWNERCOMPANYID");
                    }

                    if (strFilter.Contains("OWNERDEPARTMENTID"))
                    {
                        strFilter = strFilter.Replace("OWNERDEPARTMENTID", "p.OWNERDEPARTMENTID");
                    }

                    if (strFilter.Contains("OWNERPOSTID"))
                    {
                        strFilter = strFilter.Replace("OWNERPOSTID", "p.OWNERPOSTID");
                    }

                    if (strFilter.Contains("OWNERID"))
                    {
                        strFilter = strFilter.Replace("OWNERID", "p.OWNERID");
                    }

                    strFilter = strFilter.Replace("==", "=");

                    for (int i = objArgs.Count() - 1; i >= 0; i--)
                    {
                        strFilter = strFilter.Replace("@" + i.ToString(), "'" + objArgs[i].ToString() + "'");
                    }

                    strSql.Append(strFilter);
                }

                strSql.Append(" order by " + strOrderBy);
                string strExportSql = "select exp.ownercompanyname 所属公司, exp.ownerdepartmentname 所属部门, exp.ownerpostname 所属岗位, exp.ownername 员工姓名, exp.logindate 登录时间, exp.loginip 登录IP from (" + strSql.ToString() + ") exp";
                return GetDataTableByCustomerSql(strExportSql);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取登陆记录明细失败，错误原因：" + ex.ToString());                
                return dtRes;
            }
        }



        private void GetLoginPersonCount(ref int iLoginPersonCount, string strFilter, object[] objArgs)
        {
            string strOrderBy = "a.ownercompanyname, a.loginmonth";
            PersonalLandStatisticDAL dalStatistic = new PersonalLandStatisticDAL();

            //由于在查询员工登录记录时，查询条件内已经含带查询子公司的查询语句,
            //因而此处isCountSub设定为false状态，防止对子公司的登录人数进行重复统计
            bool isCountSub = false;
            List<V_LandStatistic> entResList = dalStatistic.GetPersonalLandStatisticList(isCountSub, strOrderBy, strFilter, objArgs);

            if (entResList == null)
            {
                return;
            }

            if (entResList.Count() == 0)
            {
                return;
            }

            iLoginPersonCount = entResList.Where(c => c.OrganizationName != "合计").Sum(c => c.Subtotal);
        }
    }
}
