using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.EntityFlowSys;
using System.Data.OracleClient;
using System.Data;
using SMT.Workflow.Common.DataAccess;

namespace SMT.FLOWDAL.ADO
{
    /// <summary>
    /// 模块定义
    /// </summary>
    public class FLOW_MODELDEFINE_TDAL
    {
        #region 龙康才新增
        /// <summary>
        /// 模块定义
        /// </summary>
        /// <param name="con">OracleConnection</param>
        /// <param name="ModelCode">模块代码</param>
        /// <returns></returns>
        public static List<FLOW_MODELDEFINE_T> GetModelDefineByCode(OracleConnection con,string ModelCode)
        {
                if (string.IsNullOrEmpty(con.ConnectionString))
                {
                    con.ConnectionString = ADOHelper.ConnectionString;
                }
                List<FLOW_MODELDEFINE_T> list = new List<FLOW_MODELDEFINE_T>();
                OracleDataReader dr = null;
                try
                {
                    #region  

                    //OracleCommand cmd = con.CreateCommand();
                    //cmd.CommandText = "select * from FLOW_MODELDEFINE_T where MODELCODE='" + ModelCode + "'";

                    //dr = cmd.ExecuteReader();
                    string sql = "select * from FLOW_MODELDEFINE_T where MODELCODE='" + ModelCode + "'";
                    dr= MsOracle.ExecuteReaderByTransaction(con, sql, null);
                    while (dr.Read())
                    {
                        #region define
                        FLOW_MODELDEFINE_T define = new FLOW_MODELDEFINE_T();
                        define.CREATECOMPANYID = dr["CREATECOMPANYID"] == DBNull.Value ? null : dr["CREATECOMPANYID"].ToString();
                        define.CREATEDATE = (DateTime)dr["CREATEDATE"];
                        define.CREATEDEPARTMENTID = dr["CREATEDEPARTMENTID"] == DBNull.Value ? null : dr["CREATEDEPARTMENTID"].ToString();
                        define.CREATEPOSTID = dr["CREATEPOSTID"] == DBNull.Value ? null : dr["CREATEDEPARTMENTID"].ToString();
                        define.CREATEUSERID = dr["CREATEUSERID"] == DBNull.Value ? null : dr["CREATEUSERID"].ToString();
                        define.CREATEUSERNAME = dr["CREATEUSERNAME"] == DBNull.Value ? null : dr["CREATEUSERNAME"].ToString();
                        define.DESCRIPTION = dr["DESCRIPTION"] == DBNull.Value ? null : dr["DESCRIPTION"].ToString();
                        define.EDITDATE = dr["EDITDATE"] == DBNull.Value ? null : (DateTime?)dr["EDITDATE"];
                        define.EDITUSERID = dr["EDITUSERID"] == DBNull.Value ? null : dr["EDITUSERID"].ToString();
                        define.EDITUSERNAME = dr["EDITUSERNAME"] == DBNull.Value ? null : dr["EDITUSERNAME"].ToString();
                        define.MODELCODE = dr["MODELCODE"] == DBNull.Value ? null : dr["MODELCODE"].ToString();
                        define.MODELDEFINEID = dr["MODELDEFINEID"] == DBNull.Value ? null : dr["MODELDEFINEID"].ToString();
                        define.PARENTMODELCODE = dr["PARENTMODELCODE"] == DBNull.Value ? null : dr["PARENTMODELCODE"].ToString();
                        define.SYSTEMCODE = dr["SYSTEMCODE"] == DBNull.Value ? null : dr["SYSTEMCODE"].ToString();
                        list.Add(define);
                        #endregion
                    }
                    dr.Close();              
                    #endregion
                    return list;
                }
                catch (Exception ex)
                {
                    if (dr != null && !dr.IsClosed)
                    {
                        dr.Close();
                    }
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    throw new Exception("GetModelDefineByCode-->" + ex.Message);
                }

          


        }
        #endregion
        public static List<FLOW_MODELDEFINE_T> GetModelDefineByCode(string ModelCode)
        {
            using (OracleConnection con = new OracleConnection(ADOHelper.ContextOracleConnection))
            {
                List<FLOW_MODELDEFINE_T> list = new List<FLOW_MODELDEFINE_T>();
                OracleDataReader dr = null;
                try
                {
                    #region
                    con.Open();


                    OracleCommand cmd = con.CreateCommand();
                    cmd.CommandText = "select * from FLOW_MODELDEFINE_T where MODELCODE='"+ModelCode+"'";

                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        #region define
                        FLOW_MODELDEFINE_T define = new FLOW_MODELDEFINE_T();
                        define.CREATECOMPANYID = dr["CREATECOMPANYID"] == DBNull.Value ? null : dr["CREATECOMPANYID"].ToString();
                        define.CREATEDATE = (DateTime)dr["CREATEDATE"];
                        define.CREATEDEPARTMENTID = dr["CREATEDEPARTMENTID"] == DBNull.Value ? null : dr["CREATEDEPARTMENTID"].ToString();
                        define.CREATEPOSTID = dr["CREATEPOSTID"] == DBNull.Value ? null : dr["CREATEDEPARTMENTID"].ToString();
                        define.CREATEUSERID = dr["CREATEUSERID"] == DBNull.Value ? null : dr["CREATEUSERID"].ToString();
                        define.CREATEUSERNAME = dr["CREATEUSERNAME"] == DBNull.Value ? null : dr["CREATEUSERNAME"].ToString();
                        define.DESCRIPTION = dr["DESCRIPTION"] == DBNull.Value ? null : dr["DESCRIPTION"].ToString();
                        define.EDITDATE = dr["EDITDATE"] == DBNull.Value ? null : (DateTime?)dr["EDITDATE"];
                        define.EDITUSERID = dr["EDITUSERID"] == DBNull.Value ? null : dr["EDITUSERID"].ToString();
                        define.EDITUSERNAME = dr["EDITUSERNAME"] == DBNull.Value ? null : dr["EDITUSERNAME"].ToString();
                        define.MODELCODE = dr["MODELCODE"] == DBNull.Value ? null : dr["MODELCODE"].ToString();
                        define.MODELDEFINEID = dr["MODELDEFINEID"] == DBNull.Value ? null : dr["MODELDEFINEID"].ToString();
                        define.PARENTMODELCODE = dr["PARENTMODELCODE"] == DBNull.Value ? null : dr["PARENTMODELCODE"].ToString();
                        define.SYSTEMCODE = dr["SYSTEMCODE"] == DBNull.Value ? null : dr["SYSTEMCODE"].ToString();
                        list.Add(define);
                        #endregion
                    }
                    dr.Close();
                    con.Close();
                    #endregion
                    return list;


                }
                catch (Exception ex)
                {
                    if (dr != null && !dr.IsClosed)
                    {
                        dr.Close();
                    }
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    throw new Exception("GetModelDefineByCode-->" + ex.Message);
                }

            }

           
        }
    }
}
