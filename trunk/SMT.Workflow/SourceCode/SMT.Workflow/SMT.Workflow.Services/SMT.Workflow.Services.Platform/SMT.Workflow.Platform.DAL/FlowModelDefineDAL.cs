/****************************************************************
 * 作者：    亢晓方
 * 书写时间：2012/9/4 16:19:19 
 * 内容概要： 
 *  ------------------------------------------------------
 * 修改：亢晓方    2012/9/4 16:19:19 
****************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using System.Data;
using System.Reflection;
using System.Linq.Dynamic;
using SMT.Workflow.Common.Model.FlowXml;
using SMT.Workflow.Common.Model.Views;
using System.Data.OracleClient;
using SMT.Workflow.Common.DataAccess;
namespace SMT.Workflow.Platform.DAL
{
    /// <summary>
    /// 模块数据操作
    /// </summary>
    public class FlowModelDefineDAL : BaseDAL
    {
        #region

        /// <summary>
        ///  查询模块分页列表
        /// </summary>
        /// <param name="filterString">filterString</param>
        /// <param name="pageIndex">pageIndex</param>
        /// <param name="pageSize">pageSize</param>
        /// <param name="pageCount">pageCount</param>
        /// <returns>List<FLOW_MODELDEFINE_T></returns>
        public List<FLOW_MODELDEFINE_T> GetModelDefineList(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            try
            {             
                if (pageSize < 1)
                {
                    pageSize = 15;
                }
                int number = pageIndex <= 1 ? 1 : ((pageIndex - 1) * pageSize) + 1;
                OracleConnection conn = MicrosoftOracle.CreateOracleConnection(ConnectionString);
                string countSql = @"SELECT count(1)  from FLOW_MODELDEFINE_T where (1=1)";
                if (!string.IsNullOrWhiteSpace(filterString))
                {
                    countSql += filterString + " ";
                }
                string sql = "SELECT * FROM (SELECT A.*, ROWNUM Page FROM (select * from FLOW_MODELDEFINE_T  order by CREATEDATE DESC ) A WHERE (1=1) AND ROWNUM<= " + pageIndex * pageSize + " ";
                if (!string.IsNullOrWhiteSpace(filterString))
                {
                    sql += filterString + " ";
                }
                sql += ") WHERE  Page >= " + number + " ";
                DataTable dt = MicrosoftOracle.ExecuteTable(conn, sql);// dao.GetDataTable(sql);              
                pageCount = Convert.ToInt32(MicrosoftOracle.ExecuteScalar(conn,countSql));            
                pageCount = (pageCount / pageSize) + ((pageCount % pageSize) > 0 ? 1 : 0);
                MicrosoftOracle.Close(conn);
                return ToList<FLOW_MODELDEFINE_T>(dt);  
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        /// <summary>
        /// 判断模块是否存在
        /// </summary>
        /// <param name="modelCode">modelCode</param>
        /// <param name="descrtption">descrtption</param>
        /// <param name="modelDefineID">modelDefineID</param>
        /// <returns>bool</returns>
        public bool GetExistModelDefine(string modelCode,string descrtption,string modelDefineID)
        {
            try
            {
                OracleConnection conn = MicrosoftOracle.CreateOracleConnection(ConnectionString);
                string sqlstr = "SELECT count(*) FROM FLOW_MODELDEFINE_T where (MODELCODE = '" + modelCode + "' or DESCRIPTION = '" + descrtption + "')";
                if (!string.IsNullOrWhiteSpace(modelDefineID))
                {
                    sqlstr += " AND MODELDEFINEID!='" + modelDefineID + "'";
                }
                int result = Convert.ToInt32(MicrosoftOracle.ExecuteScalar(conn, sqlstr));
                MicrosoftOracle.Close(conn);
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        /// <summary>
        /// 新增模块代码
        /// </summary>
        /// <param name="flowModel">FlowModel</param>
        /// <returns>string</returns>
        public string AddModelDefine(FLOW_MODELDEFINE_T flowModel)
        {
            try
            {
                OracleConnection conn = MicrosoftOracle.CreateOracleConnection(ConnectionString);
                StringBuilder inssql = new StringBuilder();
                inssql.Append("INSERT INTO FLOW_MODELDEFINE_T (MODELDEFINEID,SYSTEMCODE,SYSTEMNAME,MODELCODE,PARENTMODELCODE,DESCRIPTION,");
                inssql.Append("CREATEUSERID,CREATEUSERNAME,CREATECOMPANYID,CREATEDEPARTMENTID,CREATEPOSTID,CREATEDATE) VALUES (:MODELDEFINEID,");
                inssql.Append(":SYSTEMCODE,:SYSTEMNAME,:MODELCODE,:PARENTMODELCODE,:DESCRIPTION,:CREATEUSERID,:CREATEUSERNAME,:CREATECOMPANYID,");
                inssql.Append(":CREATEDEPARTMENTID,:CREATEPOSTID,:CREATEDATE)");             
                OracleParameter[] pageparm =
                {                    
                    new OracleParameter(":MODELDEFINEID",GetValue(flowModel.MODELDEFINEID)), //模块ID 
                    new OracleParameter(":SYSTEMCODE",GetValue(flowModel.SYSTEMCODE)), //系统代码 
                    new OracleParameter(":SYSTEMNAME",GetValue(flowModel.SYSTEMNAME)), //系统代码 
                    new OracleParameter(":MODELCODE",GetValue(flowModel.MODELCODE)), //模块代码 
                    new OracleParameter(":PARENTMODELCODE",GetValue(flowModel.PARENTMODELCODE)), //上级模块代码 
                    new OracleParameter(":DESCRIPTION",GetValue(flowModel.DESCRIPTION)), //模块描述 
                    new OracleParameter(":CREATEUSERID",GetValue(flowModel.CREATEUSERID)), //创建人ID 
                    new OracleParameter(":CREATEUSERNAME",GetValue(flowModel.CREATEUSERNAME)), //创建人名 
                    new OracleParameter(":CREATECOMPANYID",GetValue(flowModel.CREATECOMPANYID)), //创建公司ID 
                    new OracleParameter(":CREATEDEPARTMENTID",GetValue(flowModel.CREATEDEPARTMENTID)), //创建部门ID 
                    new OracleParameter(":CREATEPOSTID",GetValue(flowModel.CREATEPOSTID)), //创建岗位ID 
                    new OracleParameter(":CREATEDATE",GetValue(DateTime.Now)) //创建时间 
                };
                int result = MicrosoftOracle.ExecuteNonQuery(conn, CommandType.Text, inssql.ToString(), pageparm);
                MicrosoftOracle.Close(conn);
                return result > 0 ? "1" : "0";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        /// <summary>
        /// 修改模块代码
        /// </summary>
        /// <param name="flowModel">flowModel</param>
        /// <returns>string</returns>
        public string UpdateModelDefine(FLOW_MODELDEFINE_T flowModel)
        {
            try
            {
                OracleConnection conn = MicrosoftOracle.CreateOracleConnection(ConnectionString);
                string updSql = "UPDATE FLOW_MODELDEFINE_T SET MODELDEFINEID=:MODELDEFINEID,SYSTEMCODE=:SYSTEMCODE,SYSTEMNAME=:SYSTEMNAME,PARENTMODELCODE=:PARENTMODELCODE,DESCRIPTION=:DESCRIPTION,EDITUSERID=:EDITUSERID,EDITUSERNAME=:EDITUSERNAME,EDITDATE=:EDITDATE WHERE   MODELCODE=:MODELCODE";
                OracleParameter[] pageparm =
                { 
                    new OracleParameter(":MODELDEFINEID",GetValue(flowModel.MODELDEFINEID)), //模块ID 
                    new OracleParameter(":SYSTEMCODE",GetValue(flowModel.SYSTEMCODE)), //系统代码 
                    new OracleParameter(":SYSTEMNAME",GetValue(flowModel.SYSTEMNAME)), //系统代码 
                    new OracleParameter(":MODELCODE",GetValue(flowModel.MODELCODE)), //模块代码 
                    new OracleParameter(":PARENTMODELCODE",GetValue(flowModel.PARENTMODELCODE)), //上级模块代码 
                    new OracleParameter(":DESCRIPTION",GetValue(flowModel.DESCRIPTION)), //模块描述 
                    new OracleParameter(":EDITUSERID",GetValue(flowModel.EDITUSERID)), //修改人ID 
                    new OracleParameter(":EDITUSERNAME",GetValue(flowModel.EDITUSERNAME)), //修改人用户名 
                    new OracleParameter(":EDITDATE",GetValue(DateTime.Now)) //修改时间 
                };
                int result = MicrosoftOracle.ExecuteNonQuery(conn, CommandType.Text, updSql, pageparm);
                if (result > 0)
                {
                    #region  哪些公司在模块中可以允许自选流程
                    if (flowModel.FreeFlowCompanyList != null && flowModel.FreeFlowCompanyList.Count > 0)
                    {
                        foreach (var ent in flowModel.FreeFlowCompanyList)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("declare  ");
                            sb.AppendLine("  i integer;");
                            sb.AppendLine("  MODELDEFINEFREEFLOWID varchar2(50) :='" + ent.MODELDEFINEFREEFLOWID + "';");
                            sb.AppendLine("  MODELCODE_temp varchar2(50) :='" + ent.MODELCODE + "';");
                            sb.AppendLine("  COMPANYNAME_temp varchar2(50) :='" + ent.COMPANYNAME + "';");
                            sb.AppendLine("  COMPANYID_temp varchar2(50) :='" + ent.COMPANYID + "';");
                            sb.AppendLine("  CREATEUSERID_temp varchar2(50) :='" + ent.CREATEUSERID + "';");
                            sb.AppendLine("  CREATEUSERNAME_temp varchar2(50) :='" + ent.CREATEUSERNAME + "';");
                            sb.AppendLine("  CREATECOMPANYID_temp varchar2(50) :='" + ent.CREATECOMPANYID + "';");
                            sb.AppendLine("  CREATEDEPARTMENTID_temp varchar2(50) :='" + ent.CREATEDEPARTMENTID + "';");
                            sb.AppendLine("  CREATEPOSTID_temp varchar2(50) :='" + ent.CREATEPOSTID + "';");
                            sb.AppendLine("  CREATEDATE_temp date :=to_date('" + DateTime.Now.ToString() + "', 'yyyy/mm/dd hh24:mi:ss');");
                            sb.AppendLine("begin");
                            sb.AppendLine("   select count(1) into i from FLOW_MODELDEFINE_FREEFLOW t where t.MODELCODE='" + ent.MODELCODE + "' and t.COMPANYID='" + ent.COMPANYID + "';");
                            sb.AppendLine("   if i<1 then");
                            sb.AppendLine("      execute immediate 'INSERT INTO FLOW_MODELDEFINE_FREEFLOW (MODELDEFINEFREEFLOWID,MODELCODE,COMPANYNAME,COMPANYID,CREATEUSERID,CREATEUSERNAME,CREATECOMPANYID,CREATEDEPARTMENTID,CREATEPOSTID,CREATEDATE) values (:MODELDEFINEFREEFLOWID,:MODELCODE,:COMPANYNAME,:COMPANYID,:CREATEUSERID,:CREATEUSERNAME,:CREATECOMPANYID,:CREATEDEPARTMENTID,:CREATEPOSTID,:CREATEDATE)'");
                            sb.AppendLine("      using MODELDEFINEFREEFLOWID,MODELCODE_temp,COMPANYNAME_temp,COMPANYID_temp,CREATEUSERID_temp,CREATEUSERNAME_temp,CREATECOMPANYID_temp,CREATEDEPARTMENTID_temp,CREATEPOSTID_temp,CREATEDATE_temp; ");
                            sb.AppendLine("      --commit;   ");
                            sb.AppendLine("   end if;");
                            sb.AppendLine("end;");

                            int n = MicrosoftOracle.ExecuteNonQuery(conn, CommandType.Text, sb.ToString());
                        }
                    }
                    #endregion
                    #region  哪些公司在模块中可以允许提单人撒回流程
                    if (flowModel.FlowCancelCompanyList != null && flowModel.FlowCancelCompanyList.Count > 0)
                    {
                        foreach (var ent in flowModel.FlowCancelCompanyList)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("declare  ");
                            sb.AppendLine("  i integer;");
                            sb.AppendLine("  MODELDEFINEFLOWCANCLEID_temp varchar2(50) :='" + ent.MODELDEFINEFLOWCANCLEID + "';");
                            sb.AppendLine("  MODELCODE_temp varchar2(50) :='" + ent.MODELCODE + "';");
                            sb.AppendLine("  COMPANYNAME_temp varchar2(50) :='" + ent.COMPANYNAME + "';");
                            sb.AppendLine("  COMPANYID_temp varchar2(50) :='" + ent.COMPANYID + "';");
                            sb.AppendLine("  CREATEUSERID_temp varchar2(50) :='" + ent.CREATEUSERID + "';");
                            sb.AppendLine("  CREATEUSERNAME_temp varchar2(50) :='" + ent.CREATEUSERNAME + "';");
                            sb.AppendLine("  CREATECOMPANYID_temp varchar2(50) :='" + ent.CREATECOMPANYID + "';");
                            sb.AppendLine("  CREATEDEPARTMENTID_temp varchar2(50) :='" + ent.CREATEDEPARTMENTID + "';");
                            sb.AppendLine("  CREATEPOSTID_temp varchar2(50) :='" + ent.CREATEPOSTID + "';");
                            sb.AppendLine("  CREATEDATE_temp date :=to_date('" + DateTime.Now.ToString() + "', 'yyyy/mm/dd hh24:mi:ss');");
                            sb.AppendLine("begin");
                            sb.AppendLine("   select count(1) into i from FLOW_MODELDEFINE_FLOWCANCLE t where  t.MODELCODE='" + ent.MODELCODE + "' and t.COMPANYID='" + ent.COMPANYID + "';");
                            sb.AppendLine("   if i<1 then");
                            sb.AppendLine("      execute immediate 'INSERT INTO FLOW_MODELDEFINE_FLOWCANCLE (MODELDEFINEFLOWCANCLEID,MODELCODE,COMPANYNAME,COMPANYID,CREATEUSERID,CREATEUSERNAME,CREATECOMPANYID,CREATEDEPARTMENTID,CREATEPOSTID,CREATEDATE) values (:MODELDEFINEFLOWCANCLEID,:MODELCODE,:COMPANYNAME,:COMPANYID,:CREATEUSERID,:CREATEUSERNAME,:CREATECOMPANYID,:CREATEDEPARTMENTID,:CREATEPOSTID,:CREATEDATE)'");
                            sb.AppendLine("      using MODELDEFINEFLOWCANCLEID_temp,MODELCODE_temp,COMPANYNAME_temp,COMPANYID_temp,CREATEUSERID_temp,CREATEUSERNAME_temp,CREATECOMPANYID_temp,CREATEDEPARTMENTID_temp,CREATEPOSTID_temp,CREATEDATE_temp; ");
                            sb.AppendLine("      --commit;   ");
                            sb.AppendLine("   end if;");
                            sb.AppendLine("end;");
                            int n = MicrosoftOracle.ExecuteNonQuery(conn, CommandType.Text, sb.ToString());
                        }
                    }
                    #endregion
                }
                MicrosoftOracle.Close(conn);
                return result > 0 ? "1" : "0";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 删除模块代码
        /// </summary>
        /// <param name="deleteList">deleteList</param>
        /// <returns>string</returns>
        public string DeleteModelDefine(List<string> deleteList)
        {
            OracleTransaction transaction = null;
            try
            {
                OracleConnection conn = MicrosoftOracle.CreateOracleConnection(ConnectionString);
                OracleConnection conndef = MicrosoftOracle.CreateOracleConnection(ConnectionString);
                OracleCommand command = conndef.CreateCommand();
                transaction = conndef.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                command.Transaction = transaction;              
                foreach (string item in deleteList)
                {
                    string sqlstr = "SELECT count(*) FROM FLOW_MODELFLOWRELATION_T where MODELCODE = '" + item + "'";
                    if (Convert.ToInt32(MicrosoftOracle.ExecuteScalar(conn, sqlstr)) < 1)
                    {
                        string sql = "delete from FLOW_MODELDEFINE_T where MODELCODE = '" + item + "'";
                        int result = MicrosoftOracle.ExecuteNonQuery(conndef, command, sql);
                    }
                    else
                    {
                        transaction.Rollback();
                        return "10";
                    }
                }
                transaction.Commit();
                MicrosoftOracle.Close(conn);
                return "1";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取所有的系统代码模块代码
        /// </summary>
        /// <returns>List<FLOW_MODELDEFINE_T></returns>
        public List<FLOW_MODELDEFINE_T> GetSystemCodeModelCodeList()
        {
            try
            {
                OracleConnection conn = MicrosoftOracle.CreateOracleConnection(ConnectionString);
                string sql = @"SELECT SYSTEMCODE，SYSTEMNAME，MODELCODE，DESCRIPTION  from FLOW_MODELDEFINE_T where (1=1)";
                DataTable dt = MicrosoftOracle.ExecuteTable(conn, sql);
                MicrosoftOracle.Close(conn);
                return ToList<FLOW_MODELDEFINE_T>(dt);        
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

        #region  哪些公司在模块中可以允许提单人撒回流程
        /// <summary>
        /// 哪些公司在模块中可以允许提单人撒回流程
        /// </summary>
        /// <param name="pageSize">pageSize</param>
        /// <param name="pageIndex">pageIndex</param>
        /// <param name="strFilter">strFilter</param>
        /// <param name="strOrderBy">strOrderBy</param>
        /// <param name="pageCount">pageCount</param>
        /// <returns>List<FLOW_MODELDEFINE_FLOWCANCLE></returns>
        public List<FLOW_MODELDEFINE_FLOWCANCLE> GetFlowCancleList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {
                dao.Open();
                string sql = "select * from FLOW_MODELDEFINE_FLOWCANCLE  WHERE (1=1) ";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    sql += strFilter + " order by " + strOrderBy + " ";
                }
                DataTable dt = dao.GetDataTable(sql);
                var items = ToList<FLOW_MODELDEFINE_FLOWCANCLE>(dt).AsQueryable();
                items = Pager<FLOW_MODELDEFINE_FLOWCANCLE>(items, pageIndex, pageSize, ref pageCount);

                return items.ToList();// ToList<FLOW_MODELDEFINE_FLOWCANCLE>(dt);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取可以撒回流程的公司 异常信息:" + ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="modelcode">modelcode</param>
        /// <param name="companyid">companyid</param>
        /// <returns>bool</returns>
        public bool DeleteFlowCancle(string modelcode, string companyid)
        {
            try
            {
                dao.Open();
                string sql = "delete FLOW_MODELDEFINE_FLOWCANCLE where modelcode='" + modelcode + "' and companyid='" + companyid + "'";
                return dao.ExecuteNonQuery(sql) > 0 ? true : false; ;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("删除撒回流程的公司 异常信息:" + ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }
        }
        #endregion
        #region  哪些公司在模块中可以允许自选流程
        /// <summary>
        /// 哪些公司在模块中可以允许自选流程
        /// </summary>
        /// <param name="pageSize">pageSize</param>
        /// <param name="pageIndex">pageIndex</param>
        /// <param name="strFilter">strFilter</param>
        /// <param name="strOrderBy">strOrderBy</param>
        /// <param name="pageCount">pageCount</param>
        /// <returns> List<FLOW_MODELDEFINE_FREEFLOW></returns>
        public List<FLOW_MODELDEFINE_FREEFLOW> GetFreeFlowList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {

                dao.Open();           

                string sql = "select * from FLOW_MODELDEFINE_FREEFLOW  WHERE (1=1) ";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    sql += strFilter + " order by " + strOrderBy + " ";
                }
                DataTable dt = dao.GetDataTable(sql);
                var items = ToList<FLOW_MODELDEFINE_FREEFLOW>(dt).AsQueryable();
                items = Pager<FLOW_MODELDEFINE_FREEFLOW>(items, pageIndex, pageSize, ref pageCount);

                return items.ToList();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取可以自选流程的公司 异常信息:" + ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 删除可以自选
        /// </summary>
        /// <param name="modelcode">modelcode</param>
        /// <param name="companyid">companyid</param>
        /// <returns>bool</returns>
        public bool DeleteFreeFlow(string modelcode, string companyid)
        {
            try
            {
                dao.Open();
                string sql = "delete FLOW_MODELDEFINE_FREEFLOW where modelcode='" + modelcode + "' and companyid='" + companyid + "'";
                return dao.ExecuteNonQuery(sql) > 0 ? true : false; ;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("删除自选流程的公司 异常信息:" + ex.ToString());
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }
        }
        #endregion
    }
}
