using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.EntityFlowSys;
using System.Data;
using System.Data.OracleClient;
using SMT.Workflow.Common.DataAccess;

namespace SMT.FLOWDAL.ADO
{
    /// <summary>
    /// 模块与流程定义关联表管理类
    /// </summary>
    public class FLOW_MODELFLOWRELATION_TDAL
    {
        #region 龙康才新增
        /// <summary>
        /// 查询 [模块与流程定义关联]
        /// </summary>
        /// <param name="CompanyID">创建公司ID</param>
        /// <param name="DepartID">创建部门ID</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="FlowType">0:审批流程，1：任务流程</param>
        /// <param name="OrgType">1：代表部门ID为空，否则部门ID不为空</param>
        /// <returns></returns>
        public static List<FLOW_MODELFLOWRELATION_T> GetFlowByModelName(OracleConnection con,string CompanyID, string DepartID, string ModelCode, string FlowType, string OrgType)
        {
                List<FLOW_MODELFLOWRELATION_T> listRelation = new List<FLOW_MODELFLOWRELATION_T>();
                OracleDataReader dr = null;
                string sql = "";    
            try
                {
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                     sql = @" select * from FLOW_MODELFLOWRELATION_T where COMPANYID='{0}' and FLOWTYPE='{1}' and FLAG='1'  and MODELCODE='{2}'";

                    if (OrgType != "1")
                    {
                        sql = sql + " and DEPARTMENTID is null";

                    }
                    else
                    {
                        sql = sql + " and DEPARTMENTID='{3}'";
                    }

                    sql = string.Format(sql, CompanyID, FlowType, ModelCode, DepartID);
                    #region
               


                    //OracleCommand cmd = con.CreateCommand();
                    //cmd.CommandText = sql;

                    //dr = cmd.ExecuteReader();
                    LogHelper.WriteLog("FLOW_MODELFLOWRELATION_TDAL->GetFlowByModelName SQL语句：" + sql);
                    dr = MsOracle.ExecuteReaderByTransaction(con, sql, null);
                    while (dr.Read())
                    {
                        #region FLOW_MODELFLOWRELATION_T
                        FLOW_MODELFLOWRELATION_T relation = new FLOW_MODELFLOWRELATION_T();
                        relation.COMPANYID = dr["COMPANYID"] == DBNull.Value ? null : dr["COMPANYID"].ToString();
                        relation.CREATECOMPANYID = dr["CREATECOMPANYID"] == DBNull.Value ? null : dr["CREATECOMPANYID"].ToString();
                        relation.CREATEDATE = (DateTime)dr["CREATEDATE"];
                        relation.CREATEDEPARTMENTID = dr["CREATEDEPARTMENTID"] == DBNull.Value ? null : dr["CREATEDEPARTMENTID"].ToString();
                        relation.CREATEPOSTID = dr["CREATEPOSTID"] == DBNull.Value ? null : dr["CREATEDEPARTMENTID"].ToString();
                        relation.CREATEUSERID = dr["CREATEUSERID"] == DBNull.Value ? null : dr["CREATEUSERID"].ToString();
                        relation.CREATEUSERNAME = dr["CREATEUSERNAME"] == DBNull.Value ? null : dr["CREATEUSERNAME"].ToString();
                        relation.DEPARTMENTID = dr["DEPARTMENTID"] == DBNull.Value ? null : dr["DEPARTMENTID"].ToString();
                        relation.EDITDATE = dr["EDITDATE"] == DBNull.Value ? null : (DateTime?)dr["EDITDATE"];
                        relation.EDITUSERID = dr["EDITUSERID"] == DBNull.Value ? null : dr["EDITUSERID"].ToString();
                        relation.EDITUSERNAME = dr["EDITUSERNAME"] == DBNull.Value ? null : dr["EDITUSERNAME"].ToString();
                        relation.FLAG = dr["FLAG"] == DBNull.Value ? null : dr["FLAG"].ToString();
                        relation.FLOWTYPE = dr["FLOWTYPE"] == DBNull.Value ? null : dr["FLOWTYPE"].ToString();
                        relation.MODELFLOWRELATIONID = dr["MODELFLOWRELATIONID"] == DBNull.Value ? null : dr["MODELFLOWRELATIONID"].ToString();
                        relation.FLOW_FLOWDEFINE_T = new FLOW_FLOWDEFINE_T();
                        relation.FLOW_FLOWDEFINE_T.FLOWCODE = dr["FLOWCODE"] == DBNull.Value ? null : dr["FLOWCODE"].ToString();
                        listRelation.Add(relation);
                        break;
                        #endregion

                    }
                    dr.Close();
                    if (listRelation.Count > 0)
                    {
                         sql = @"select * from FLOW_FLOWDEFINE_T where FLOWCODE='" + listRelation[0].FLOW_FLOWDEFINE_T.FLOWCODE + "'";
                        //dr = cmd.ExecuteReader();

                        dr = MsOracle.ExecuteReaderByTransaction(con, sql, null);
                        while (dr.Read())
                        {
                            #region FLOW_FLOWDEFINE_T
                            FLOW_FLOWDEFINE_T define = new FLOW_FLOWDEFINE_T();
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
                            define.FLOWCODE = dr["FLOWCODE"] == DBNull.Value ? null : dr["FLOWCODE"].ToString();
                            define.FLOWTYPE = dr["FLOWTYPE"] == DBNull.Value ? null : dr["FLOWTYPE"].ToString();
                            define.FLOWDEFINEID = dr["FLOWDEFINEID"] == DBNull.Value ? null : dr["FLOWDEFINEID"].ToString();
                            define.LAYOUT = dr["LAYOUT"] == DBNull.Value ? null : dr["LAYOUT"].ToString();
                            define.RULES = dr["RULES"] == DBNull.Value ? null : dr["RULES"].ToString();
                            define.XOML = dr["XOML"] == DBNull.Value ? null : dr["XOML"].ToString(); ;
                            listRelation[0].FLOW_FLOWDEFINE_T = define;
                            break;
                            #endregion

                        }
                        dr.Close();

                    }
                 
                    #endregion
                    return listRelation;


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
                    throw new Exception("GetFlowByModelName-->" + ex.Message);
                    
                }
        }

        #endregion
        /// <summary>
        /// 查询 [模块与流程定义关联]
        /// </summary>
        /// <param name="CompanyID">创建公司ID</param>
        /// <param name="DepartID">创建部门ID</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="FlowType">0:审批流程，1：任务流程</param>
        /// <param name="OrgType">1：代表部门ID为空，否则部门ID不为空</param>
        /// <returns></returns>
        public static List<FLOW_MODELFLOWRELATION_T> GetFlowByModelName(string CompanyID, string DepartID, string ModelCode, string FlowType, string OrgType)
        {
            using (OracleConnection con = new OracleConnection(ADOHelper.ContextOracleConnection))
            {
                List<FLOW_MODELFLOWRELATION_T> listRelation = new List<FLOW_MODELFLOWRELATION_T>();
                OracleDataReader dr = null;               
                try
                {

                    string sql = @" select * from FLOW_MODELFLOWRELATION_T where COMPANYID='{0}' and FLOWTYPE='{1}' and FLAG='1'  and MODELCODE='{2}'";
                    
                    if (OrgType != "1")
                    {
                        sql = sql + " and DEPARTMENTID is null";
                       
                    }
                    else
                    {
                        sql = sql + " and DEPARTMENTID='{3}'";
                    }

                    sql = string.Format(sql, CompanyID, FlowType, ModelCode, DepartID);
                    #region
                    con.Open();


                    OracleCommand cmd = con.CreateCommand();
                    cmd.CommandText = sql;

                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        #region FLOW_MODELFLOWRELATION_T
                        FLOW_MODELFLOWRELATION_T relation = new FLOW_MODELFLOWRELATION_T();
                        relation.COMPANYID = dr["COMPANYID"] == DBNull.Value ? null : dr["COMPANYID"].ToString();
                        relation.CREATECOMPANYID = dr["CREATECOMPANYID"] == DBNull.Value ? null : dr["CREATECOMPANYID"].ToString();
                        relation.CREATEDATE = (DateTime)dr["CREATEDATE"];
                        relation.CREATEDEPARTMENTID = dr["CREATEDEPARTMENTID"] == DBNull.Value ? null : dr["CREATEDEPARTMENTID"].ToString();
                        relation.CREATEPOSTID = dr["CREATEPOSTID"] == DBNull.Value ? null : dr["CREATEDEPARTMENTID"].ToString();
                        relation.CREATEUSERID = dr["CREATEUSERID"] == DBNull.Value ? null : dr["CREATEUSERID"].ToString();
                        relation.CREATEUSERNAME = dr["CREATEUSERNAME"] == DBNull.Value ? null : dr["CREATEUSERNAME"].ToString();
                        relation.DEPARTMENTID = dr["DEPARTMENTID"] == DBNull.Value ? null : dr["DEPARTMENTID"].ToString();
                        relation.EDITDATE = dr["EDITDATE"] == DBNull.Value ? null : (DateTime?)dr["EDITDATE"];
                        relation.EDITUSERID = dr["EDITUSERID"] == DBNull.Value ? null : dr["EDITUSERID"].ToString();
                        relation.EDITUSERNAME = dr["EDITUSERNAME"] == DBNull.Value ? null : dr["EDITUSERNAME"].ToString();
                        relation.FLAG = dr["FLAG"] == DBNull.Value ? null : dr["FLAG"].ToString();
                        relation.FLOWTYPE = dr["FLOWTYPE"] == DBNull.Value ? null : dr["FLOWTYPE"].ToString();
                        relation.MODELFLOWRELATIONID = dr["MODELFLOWRELATIONID"] == DBNull.Value ? null : dr["MODELFLOWRELATIONID"].ToString();
                        relation.FLOW_FLOWDEFINE_T = new FLOW_FLOWDEFINE_T();
                        relation.FLOW_FLOWDEFINE_T.FLOWCODE = dr["FLOWCODE"] == DBNull.Value ? null : dr["FLOWCODE"].ToString();
                        listRelation.Add(relation);
                        break;
                        #endregion 
                       
                    }
                    dr.Close();
                    if (listRelation.Count > 0)
                    {
                        cmd.CommandText = @"select * from FLOW_FLOWDEFINE_T where FLOWCODE='" + listRelation[0].FLOW_FLOWDEFINE_T.FLOWCODE + "'";
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            #region FLOW_FLOWDEFINE_T
                            FLOW_FLOWDEFINE_T define = new FLOW_FLOWDEFINE_T();                           
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
                            define.FLOWCODE = dr["FLOWCODE"] == DBNull.Value ? null : dr["FLOWCODE"].ToString();
                            define.FLOWTYPE = dr["FLOWTYPE"] == DBNull.Value ? null : dr["FLOWTYPE"].ToString();
                            define.FLOWDEFINEID = dr["FLOWDEFINEID"] == DBNull.Value ? null : dr["FLOWDEFINEID"].ToString();
                            define.LAYOUT = dr["LAYOUT"] == DBNull.Value ? null : dr["LAYOUT"].ToString();
                            define.RULES = dr["RULES"] == DBNull.Value ? null : dr["RULES"].ToString();
                            define.XOML = dr["XOML"] == DBNull.Value ? null : dr["XOML"].ToString(); ;
                            listRelation[0].FLOW_FLOWDEFINE_T = define;
                            break;
                            #endregion

                        }
                        dr.Close();

                    }
                    con.Close();
                    #endregion
                    return listRelation;


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
                    throw new Exception("GetFlowByModelName-->" + ex.Message);
                }

            }

        }

        
    }
}
