using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using SMT.HRM.CustomModel;
using SMT_HRM_EFModel;


namespace SMT.HRM.DAL
{
    public class PersonalLandStatisticDAL : CommDal<V_LandStatistic>
    {
        public List<V_LandStatistic> GetPersonalLandStatisticList(bool isCountSub, string strOrderBy,
            string strFilter, params object[] objArgs)
        {
            List<V_LandStatistic> entResList = new List<V_LandStatistic>();
            try
            {
                StringBuilder strSql = new StringBuilder();
                StringBuilder strTemp = new StringBuilder();

                strTemp.Append("select p.ownername, p.ownercompanyname, p.loginyear, p.loginmonth, p.ownerid, p.ownerpostid,");
                strTemp.Append("p.ownerdepartmentid, p.ownercompanyid from smtsystem.t_sys_userloginrecord p ");

                if (objArgs.Count() > 0 && !string.IsNullOrEmpty(strFilter))
                {
                    if (strTemp.ToString().Contains("Where"))
                    {
                        strTemp.Append(" and ");
                    }
                    else
                    {
                        strTemp.Append(" Where ");
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

                    strTemp.Append(strFilter);
                }

                StringBuilder strNestTemp = new StringBuilder();
                strNestTemp.Append("select distinct zzz.ownercompanyid, zzz.ownercompanyname, zzz.ownerid, zzz.loginyear, zzz.loginmonth from (");
                strNestTemp.Append(strTemp.ToString());
                strNestTemp.Append(") zzz");

                strSql.Append("select a.ownercompanyid, a.ownercompanyname, a.loginyear, a.loginmonth, count(a.ownerid) logintimes  from (");
                strSql.Append(strNestTemp.ToString());
                strSql.Append(") a");
                strSql.Append(" group by a.ownercompanyid, a.ownercompanyname, a.loginyear, a.loginmonth");
                strSql.Append(" order by " + strOrderBy);

                DataTable dtRes = GetDataTableByCustomerSql(strSql.ToString());

                V_LandStatistic entTotalRes = new V_LandStatistic();

                foreach (DataRow dr in dtRes.Rows)
                {
                    if (dr["OWNERCOMPANYID"] == null || dr["LOGINYEAR"] == null || dr["LOGINMONTH"] == null || dr["LOGINTIMES"] == null)
                    {
                        continue;
                    }

                    string strCompanyID = string.Empty, strCompanyName = string.Empty;
                    bool bIsExists = false;
                    int iloginyear = 0, iloginmonth, ilogintimes = 0;

                    strCompanyID = dr["OWNERCOMPANYID"].ToString();
                    int.TryParse(dr["LOGINYEAR"].ToString(), out iloginyear);
                    int.TryParse(dr["LOGINMONTH"].ToString(), out iloginmonth);
                    int.TryParse(dr["LOGINTIMES"].ToString(), out ilogintimes);

                    if(dr["OWNERCOMPANYNAME"] != null)
                    {
                        strCompanyName = dr["OWNERCOMPANYNAME"].ToString();
                    }

                    V_LandStatistic entRes = new V_LandStatistic();

                    if (entResList.Count() > 0)
                    {
                        var t = from n in entResList
                                where n.OrganizationID == strCompanyID && n.LoginYear == iloginyear
                                select n;

                        if (t.Count() > 0)
                        {
                            entRes = t.FirstOrDefault();
                            bIsExists = true;
                        }
                    }                    

                    switch (iloginmonth)
                    {
                        case 1:
                            entRes.JanTimes += ilogintimes;
                            entTotalRes.JanTimes += ilogintimes;
                            break;
                        case 2:
                            entRes.FebTimes += ilogintimes;
                            entTotalRes.FebTimes += ilogintimes;
                            break;
                        case 3:
                            entRes.MarTimes += ilogintimes;
                            entTotalRes.MarTimes += ilogintimes;
                            break;
                        case 4:
                            entRes.AprTimes += ilogintimes;
                            entTotalRes.AprTimes += ilogintimes;
                            break;
                        case 5:
                            entRes.MayTimes += ilogintimes;
                            entTotalRes.MayTimes += ilogintimes;
                            break;
                        case 6:
                            entRes.JunTimes += ilogintimes;
                            entTotalRes.JunTimes += ilogintimes;
                            break;
                        case 7:
                            entRes.JulTimes += ilogintimes;
                            entTotalRes.JulTimes += ilogintimes;
                            break;
                        case 8:
                            entRes.AugTimes += ilogintimes;
                            entTotalRes.AugTimes += ilogintimes;
                            break;
                        case 9:
                            entRes.SepTimes += ilogintimes;
                            entTotalRes.SepTimes += ilogintimes;
                            break;
                        case 10:
                            entRes.OctTimes += ilogintimes;
                            entTotalRes.OctTimes += ilogintimes;
                            break;
                        case 11:
                            entRes.NovTimes += ilogintimes;
                            entTotalRes.NovTimes += ilogintimes;
                            break;
                        case 12:
                            entRes.DecTimes += ilogintimes;
                            entTotalRes.DecTimes += ilogintimes;
                            break;
                    }

                    entRes.Subtotal += ilogintimes;
                    entTotalRes.Subtotal += ilogintimes;

                    entRes.OrganizationID = strCompanyID;
                    entRes.OrganizationName = strCompanyName;
                    entRes.LoginYear = iloginyear;
                    if (isCountSub)
                    {
                        GetLoginTimesByFatherCompany(iloginmonth, ref entRes, ref entTotalRes);
                    }

                    if (!bIsExists)
                    {                        
                        entResList.Add(entRes);
                    }
                }

                if (entResList.Count() > 0) 
                {
                    entTotalRes.OrganizationName = "合计";
                    entResList.Add(entTotalRes);
                }
                
                return entResList;                
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取登陆记录统计失败，错误原因：" + ex.ToString());
                return entResList;
            }
        }

        /// <summary>
        /// 取子公司当年当月的登录人数总和
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <param name="iloginyear"></param>
        /// <param name="iloginmonth"></param>
        /// <param name="ilogintimes"></param>
        private void GetLoginTimesByFatherCompany(int iCurmonth, ref V_LandStatistic entRes, ref V_LandStatistic entTotalRes)
        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strTemp = new StringBuilder();

            strTemp.Append("select p.ownername, p.ownercompanyname, p.loginyear, p.loginmonth, p.ownerid, p.ownerpostid,");
            strTemp.Append("p.ownerdepartmentid, p.ownercompanyid from smtsystem.t_sys_userloginrecord p ");
            strTemp.Append(" Where (p.loginyear = " + entRes.LoginYear.ToString() + ") ");
            strTemp.Append(" AND (p.loginmonth = " + iCurmonth.ToString() + ") ");
            strTemp.Append(" AND ((p.ownercompanyid in (select t.companyid from t_hr_company t ");
            strTemp.Append(" where t.companyid in (select cc.companyid from t_hr_company cc");
            strTemp.Append(" start with cc.fathercompanyid = '" + entRes.OrganizationID + "'");
            strTemp.Append(" connect by prior cc.companyid = cc.fathercompanyid)))) ");

            StringBuilder strNestTemp = new StringBuilder();
            strNestTemp.Append("select distinct zzz.ownercompanyid, zzz.ownercompanyname, zzz.ownerid, zzz.loginyear, zzz.loginmonth from (");
            strNestTemp.Append(strTemp.ToString());
            strNestTemp.Append(") zzz");

            strSql.Append("select a.loginyear, a.loginmonth, count(a.ownerid) logintimes  from (");
            strSql.Append(strNestTemp.ToString());
            strSql.Append(") a");
            strSql.Append(" group by a.loginyear, a.loginmonth");

            DataTable dtRes = GetDataTableByCustomerSql(strSql.ToString());

            if (dtRes == null)
            {
                return;
            }

            if (dtRes.Rows == null)
            {
                return;
            }

            if (dtRes.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow dr in dtRes.Rows)
            {
                if (dr["LOGINYEAR"] == null || dr["LOGINMONTH"] == null || dr["LOGINTIMES"] == null)
                {
                    continue;
                }

                int iloginyear = 0, iloginmonth, ilogintimes = 0;

                int.TryParse(dr["LOGINYEAR"].ToString(), out iloginyear);
                int.TryParse(dr["LOGINMONTH"].ToString(), out iloginmonth);
                int.TryParse(dr["LOGINTIMES"].ToString(), out ilogintimes);
                
                switch (iloginmonth)
                {
                    case 1:
                        entRes.JanTimes += ilogintimes;
                        entTotalRes.JanTimes += ilogintimes;
                        break;
                    case 2:
                        entRes.FebTimes += ilogintimes;
                        entTotalRes.FebTimes += ilogintimes;
                        break;
                    case 3:
                        entRes.MarTimes += ilogintimes;
                        entTotalRes.MarTimes += ilogintimes;
                        break;
                    case 4:
                        entRes.AprTimes += ilogintimes;
                        entTotalRes.AprTimes += ilogintimes;
                        break;
                    case 5:
                        entRes.MayTimes += ilogintimes;
                        entTotalRes.MayTimes += ilogintimes;
                        break;
                    case 6:
                        entRes.JunTimes += ilogintimes;
                        entTotalRes.JunTimes += ilogintimes;
                        break;
                    case 7:
                        entRes.JulTimes += ilogintimes;
                        entTotalRes.JulTimes += ilogintimes;
                        break;
                    case 8:
                        entRes.AugTimes += ilogintimes;
                        entTotalRes.AugTimes += ilogintimes;
                        break;
                    case 9:
                        entRes.SepTimes += ilogintimes;
                        entTotalRes.SepTimes += ilogintimes;
                        break;
                    case 10:
                        entRes.OctTimes += ilogintimes;
                        entTotalRes.OctTimes += ilogintimes;
                        break;
                    case 11:
                        entRes.NovTimes += ilogintimes;
                        entTotalRes.NovTimes += ilogintimes;
                        break;
                    case 12:
                        entRes.DecTimes += ilogintimes;
                        entTotalRes.DecTimes += ilogintimes;
                        break;
                }

                entRes.Subtotal += ilogintimes;
                entTotalRes.Subtotal += ilogintimes;
            }
        }               
    }
}
