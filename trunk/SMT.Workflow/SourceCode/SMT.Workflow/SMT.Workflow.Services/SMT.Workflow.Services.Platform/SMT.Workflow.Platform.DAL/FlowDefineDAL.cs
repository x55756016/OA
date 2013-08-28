/****************************************************************
 * 作者：    亢晓方
 * 书写时间：2012/9/12 15:41:32 
 * 内容概要： 
 *  ------------------------------------------------------
 * 修改：  修改服务接口  
****************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;

using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.Views;
using SMT.Workflow.Common.DataAccess;
using System.IO;
using System.Xml;
using System.Transactions;
using System.Collections;

namespace SMT.Workflow.Platform.DAL
{
    /// <summary>
    /// 流程数据操作
    /// </summary>
    public class FlowDefineDAL : BaseDAL
    {
        #region 新方法

        /// <summary>
        /// 获取流程列表
        /// </summary>
        /// <param name="allOwnerCompanyId">公司ＩＤ</param>
        /// <returns>List<V_FlowDefine></returns>
        public List<V_FlowDefine> GetFlowDefineList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {            
                if (string.IsNullOrEmpty(strOrderBy))
                {
                    strOrderBy = "a.createdate desc";
                }
                if (pageSize == 0)
                {
                    pageSize = 15;
                }
                int number = pageIndex <= 1 ? 1 : 1 + ((pageIndex - 1) * pageSize);
                dao.Open();
                StringBuilder sb = new StringBuilder();
                string countSql = @"select count(1) from flow_flowdefine_t a
                                    inner join flow_modelflowrelation_t b on a.flowcode=b.flowcode
                                    inner join flow_modeldefine_t c on c.modelcode=b.modelcode where (1=1)";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    countSql += strFilter + " ";
                }
                string sql = @"SELECT * FROM (SELECT A.*, ROWNUM Page FROM (select a.createdate, a.flowcode,a.description,b.companyid,
                                    b.departmentid,b.companyname,b.departmentname, 
                                    c.systemcode,c.modelcode,c.description modelname ,c.systemname from flow_flowdefine_t a
                                    inner join flow_modelflowrelation_t b on a.flowcode=b.flowcode
                                    inner join flow_modeldefine_t c on c.modelcode=b.modelcode
                                    WHERE (1=1)  " + strFilter + "  order by   " + strOrderBy + " ) A WHERE (1=1) AND ROWNUM<= " + pageIndex * pageSize + " ";             
                sql += ") WHERE  Page >= " + number + " ";
                DataTable dt = dao.GetDataTable(sql);
                LogHelper.WriteLog("查找到的流程SQL"+sql);
                pageCount = Convert.ToInt32(dao.ExecuteScalar(countSql));       
                pageCount = (pageCount / pageSize) + ((pageCount % pageSize) > 0 ? 1 : 0);
                List<V_FlowDefine> list = new List<V_FlowDefine>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        V_FlowDefine entity = new V_FlowDefine();
                        entity.FlowCode = dr["flowcode"].ToString();
                        entity.FlowName = dr["description"] == null ? string.Empty : dr["description"].ToString();
                        entity.CompanyName = dr["companyname"] == null ? string.Empty : dr["companyname"].ToString();
                        entity.DepartmentName = dr["departmentname"] == null ? string.Empty : dr["departmentname"].ToString();
                        entity.SystemName = dr["systemname"] == null ? string.Empty : dr["systemname"].ToString();
                        entity.ModelName = dr["modelname"] == null ? string.Empty : dr["modelname"].ToString();
                        entity.CREATEDATE = Convert.ToDateTime(dr["createdate"].ToString());                      
                        list.Add(entity);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        /// <summary>
        /// 新增流程
        /// </summary>
        /// <param name="flow">视图</param>    
        /// <returns>bool</returns>
        public bool AddFlowDefine(V_FLOWDEFINITION flow)
        {
            OracleConnection conn = MicrosoftOracle.CreateOracleConnection(ConnectionString);
            OracleCommand command = conn.CreateCommand();
            OracleTransaction transaction;
            transaction = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            command.Transaction = transaction;
            try
            {
                AdddFlowDefine(conn, command, flow);
                transaction.Commit();
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                conn.Close();
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 新增流程
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="command"></param>
        /// <param name="flow"></param>
        private void AdddFlowDefine(OracleConnection conn, OracleCommand command, V_FLOWDEFINITION flow)
        {
            try
            {
                FLOW_MODELFLOWRELATION_T relation = flow.ModelRelation;
                FLOW_FLOWDEFINE_T define = flow.FlowDefinition;
                string[] companyids = relation.COMPANYID.TrimEnd('|').Split('|');//所有关联公司的ID
                string[] companynames = relation.COMPANYNAME.TrimEnd('|').Split('|');//所有关联公司的名称
                for (int i = 0; i < companyids.Length; i++)//循环公司新增
                {
                    if (!string.IsNullOrEmpty(relation.DEPARTMENTID))//如果存在部门
                    {
                        string[] departmentids = relation.DEPARTMENTID.TrimEnd('|').Split('|');//所有关联部门的ID
                        string[] departmentnames = relation.DEPARTMENTNAME.TrimEnd('|').Split('|');//所有关联部门名称
                        for (int j = 0; j < departmentids.Length; j++)//循环部门新增
                        {
                            string flowCode = InsertHistory(conn, command, companyids[i], relation.MODELCODE, departmentids[j], flow);
                            if (string.IsNullOrWhiteSpace(flowCode))
                            {
                                flowCode = Guid.NewGuid().ToString().Replace("-", string.Empty);
                            }
                            relation.COMPANYID = companyids[i];
                            relation.COMPANYNAME = companynames[i];
                            relation.DEPARTMENTID = departmentids[j];
                            relation.DEPARTMENTNAME = departmentnames[j];
                            relation.FLOWCODE = flowCode;
                            define.FLOWCODE = flowCode;
                            AddFlow(conn, command, define);
                            AddRelation(conn, command, relation);
                        }
                    }
                    else
                    {
                        string flowCode = InsertHistory(conn, command, companyids[i], relation.MODELCODE, string.Empty, flow);
                        if (string.IsNullOrWhiteSpace(flowCode))
                        {
                            flowCode = Guid.NewGuid().ToString().Replace("-", string.Empty);
                        }
                        relation.COMPANYID = companyids[i];
                        relation.COMPANYNAME = companynames[i];
                        relation.FLOWCODE = flowCode;
                        define.FLOWCODE = flowCode;
                        AddFlow(conn, command, define);
                        AddRelation(conn, command, relation);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 判断流程是否存在
        /// </summary>
        /// <param name="flowCode">Code</param>
        /// <returns>bool</returns>
        private bool IsExistFlow(string flowCode)
        {
            bool isExists = false;
            string sql = "SELECT FlowCode FROM FLOW_FLOWDEFINE_T WHERE FlowCode = '" + flowCode + "'";
            dao.Open();
            DataTable dt = dao.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                isExists = true;
            }
            dao.Close();
            return isExists;
        }   

        /// <summary>
        /// 新增流程定义
        /// </summary>
        /// <param name="conn">链接</param>
        /// <param name="command">数据库命令</param>
        /// <param name="define">流程定义</param>
        /// <returns>bool</returns>
        private bool AddFlow(OracleConnection conn, OracleCommand command, FLOW_FLOWDEFINE_T define)
        {
            try
            {
                string insSql = @"INSERT INTO FLOW_FLOWDEFINE_T (FLOWDEFINEID,FLOWCODE,DESCRIPTION,XOML,RULES,LAYOUT,FLOWTYPE,
                            CREATEUSERID,CREATEUSERNAME,CREATECOMPANYID,CREATEDEPARTMENTID,CREATEPOSTID,CREATEDATE,EDITUSERID,EDITUSERNAME,
                            EDITDATE,SYSTEMCODE,BUSINESSOBJECT,WFLAYOUT) VALUES (:FLOWDEFINEID,:FLOWCODE,:DESCRIPTION,:XOML,:RULES,:LAYOUT,
                            :FLOWTYPE,:CREATEUSERID,:CREATEUSERNAME,:CREATECOMPANYID,:CREATEDEPARTMENTID,:CREATEPOSTID,:CREATEDATE,:EDITUSERID,:EDITUSERNAME,
                            :EDITDATE,:SYSTEMCODE,:BUSINESSOBJECT,:WFLAYOUT)";
                OracleParameter[] pageparm =
                                    {               
                                    #region  参数
                                    new OracleParameter(":FLOWDEFINEID",OracleType.NVarChar,100), 
                                    new OracleParameter(":FLOWCODE",OracleType.NVarChar,100), 
                                    new OracleParameter(":DESCRIPTION",OracleType.NVarChar,100), 
                                    new OracleParameter(":XOML",OracleType.Clob), 
                                    new OracleParameter(":RULES",OracleType.Clob), 
                                    new OracleParameter(":LAYOUT",OracleType.Clob), 
                                    new OracleParameter(":FLOWTYPE",OracleType.NVarChar,2), 
                                    new OracleParameter(":CREATEUSERID",OracleType.NVarChar,100), 
                                    new OracleParameter(":CREATEUSERNAME",OracleType.NVarChar,100), 
                                    new OracleParameter(":CREATECOMPANYID",OracleType.NVarChar,100), 
                                    new OracleParameter(":CREATEDEPARTMENTID",OracleType.NVarChar,100), 
                                    new OracleParameter(":CREATEPOSTID",OracleType.NVarChar,100), 
                                    new OracleParameter(":CREATEDATE",OracleType.DateTime), 
                                    new OracleParameter(":EDITUSERID",OracleType.NVarChar,100), 
                                    new OracleParameter(":EDITUSERNAME",OracleType.NVarChar,100), 
                                    new OracleParameter(":EDITDATE",OracleType.DateTime), 
                                    new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                                    new OracleParameter(":BUSINESSOBJECT",OracleType.NVarChar,100), 
                                    new OracleParameter(":WFLAYOUT",OracleType.Clob)                                  
                                    #endregion
                                };
                pageparm[0].Value = GetValue(Guid.NewGuid().ToString().Replace("-", string.Empty));//流程定义ID
                pageparm[1].Value = GetValue(define.FLOWCODE);//流程代码
                pageparm[2].Value = GetValue(define.DESCRIPTION); ;//名称描述
                pageparm[3].Value = GetValue(define.XOML);//模型文件
                pageparm[4].Value = GetValue(define.RULES);//模型规则
                pageparm[5].Value = GetValue(define.LAYOUT);//模型布局
                pageparm[6].Value = GetValue(define.FLOWTYPE);//流程类型 -- 0:审批流程, 1:任务流程
                pageparm[7].Value = GetValue(define.CREATEUSERID);//操作人员ID
                pageparm[8].Value = GetValue(define.CREATEUSERNAME);//操作人员名
                pageparm[9].Value = GetValue(define.CREATECOMPANYID);//创建公司ID
                pageparm[10].Value = GetValue(define.CREATEDEPARTMENTID);//创建部门ID
                pageparm[11].Value = GetValue(define.CREATEPOSTID);//创建岗位ID
                pageparm[12].Value = GetValue(define.CREATEDATE);//创建时间
                pageparm[13].Value = GetValue(define.EDITUSERID);//修改人ID
                pageparm[14].Value = GetValue(define.EDITUSERNAME);//修改人用户名
                pageparm[15].Value = GetValue(define.EDITDATE);//修改时间
                pageparm[16].Value = GetValue(define.SYSTEMCODE);//业务系统:OA,HR,TM等
                pageparm[17].Value = GetValue(define.BUSINESSOBJECT);//业务对象：各种申请报销单
                pageparm[18].Value = GetValue(define.WFLAYOUT);//流程定义文件,把旧的ID换成新的ID                            
                int n = MicrosoftOracle.ExecuteSQL(conn, command, insSql, pageparm);
                if (n > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 新增流程关系
        /// </summary>
        /// <param name="conn">数据库链接</param>
        /// <param name="command">数据库命令</param>
        /// <param name="relation">关系实体</param>
        /// <returns>bool</returns>
        private bool AddRelation(OracleConnection conn, OracleCommand command, FLOW_MODELFLOWRELATION_T relation)
        {
            try
            {
                string relationSql = @"INSERT INTO FLOW_MODELFLOWRELATION_T (MODELFLOWRELATIONID,COMPANYID,DEPARTMENTID,COMPANYNAME,DEPARTMENTNAME,
                        SYSTEMCODE,MODELCODE,FLOWCODE,FLAG,FLOWTYPE,CREATEUSERID,CREATEUSERNAME,CREATECOMPANYID,CREATEDEPARTMENTID,
                        CREATEPOSTID,CREATEDATE,EDITUSERID,EDITUSERNAME,EDITDATE) VALUES (:MODELFLOWRELATIONID,:COMPANYID,:DEPARTMENTID,
                        :COMPANYNAME,:DEPARTMENTNAME,:SYSTEMCODE,:MODELCODE,:FLOWCODE,:FLAG,:FLOWTYPE,:CREATEUSERID,:CREATEUSERNAME,
                        :CREATECOMPANYID,:CREATEDEPARTMENTID,:CREATEPOSTID,:CREATEDATE,:EDITUSERID,:EDITUSERNAME,:EDITDATE)";
                OracleParameter[] relationParm =
                                {   
                            new OracleParameter(":MODELFLOWRELATIONID",OracleType.NVarChar,100), 
                            new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                            new OracleParameter(":DEPARTMENTID",OracleType.NVarChar,100), 
                            new OracleParameter(":COMPANYNAME",OracleType.NVarChar,100), 
                            new OracleParameter(":DEPARTMENTNAME",OracleType.NVarChar,100), 
                            new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":FLOWCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":FLAG",OracleType.NVarChar,2), 
                            new OracleParameter(":FLOWTYPE",OracleType.NVarChar,2), 
                            new OracleParameter(":CREATEUSERID",OracleType.NVarChar,100), 
                            new OracleParameter(":CREATEUSERNAME",OracleType.NVarChar,100), 
                            new OracleParameter(":CREATECOMPANYID",OracleType.NVarChar,100), 
                            new OracleParameter(":CREATEDEPARTMENTID",OracleType.NVarChar,100), 
                            new OracleParameter(":CREATEPOSTID",OracleType.NVarChar,100), 
                            new OracleParameter(":CREATEDATE",OracleType.DateTime), 
                            new OracleParameter(":EDITUSERID",OracleType.NVarChar,100), 
                            new OracleParameter(":EDITUSERNAME",OracleType.NVarChar,100), 
                            new OracleParameter(":EDITDATE",OracleType.DateTime)                          
                                };
                relationParm[0].Value = Guid.NewGuid().ToString();//关联ID
                relationParm[1].Value = GetValue(relation.COMPANYID);//公司ID
                relationParm[2].Value = GetValue(relation.DEPARTMENTID);//部门ID
                relationParm[3].Value = GetValue(relation.COMPANYNAME);//公司名称
                relationParm[4].Value = GetValue(relation.DEPARTMENTNAME);//部门名称
                relationParm[5].Value = GetValue(relation.SYSTEMCODE);//系统代码
                relationParm[6].Value = GetValue(relation.MODELCODE);//模块代码
                relationParm[7].Value = GetValue(relation.FLOWCODE);//流程代码
                relationParm[8].Value = "1";//1这可用，0为不可用
                relationParm[9].Value = GetValue(relation.FLOWTYPE);//0:审批流程，1：任务流程
                relationParm[10].Value = GetValue(relation.CREATEUSERID);//操作人员ID
                relationParm[11].Value = GetValue(relation.CREATEUSERNAME);//操作人员名
                relationParm[12].Value = GetValue(relation.CREATECOMPANYID);//创建公司ID
                relationParm[13].Value = GetValue(relation.CREATEDEPARTMENTID);//创建部门ID
                relationParm[14].Value = GetValue(relation.CREATEPOSTID);//创建岗位ID
                relationParm[15].Value = DateTime.Now;//创建时间
                relationParm[16].Value = GetValue(relation.CREATEUSERID);//修改人ID
                relationParm[17].Value = GetValue(relation.CREATEUSERNAME);//修改人用户名
                relationParm[18].Value = DateTime.Now;//修改时间               
                int k = MicrosoftOracle.ExecuteSQL(conn, command, relationSql, relationParm);
                if (k > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 插入历史数据，然后删除
        /// </summary>
        /// <param name="conn">数据库链接</param>
        /// <param name="command">数据库命令</param>
        /// <param name="companyID">公司ＩＤ</param>
        /// <param name="modelCode">模块</param>
        /// <param name="departmentID">部门</param>
        /// <param name="flow">视图</param>
        /// <returns>string</returns>
        private string InsertHistory(OracleConnection conn, OracleCommand command, string companyID, string modelCode, string departmentID, V_FLOWDEFINITION flow)
        {           
            try
            {
                StringBuilder sb = new StringBuilder();
                string flowCode = string.Empty;
                sb.AppendLine("select  flowcode from FLOW_MODELFLOWRELATION_T t");
                sb.AppendLine(" where t.COMPANYID='" + companyID + "'  and t.MODELCODE='" + modelCode + "'");
                if (!string.IsNullOrWhiteSpace(departmentID))
                {                   
                    sb.AppendLine(" AND DEPARTMENTID ='" + departmentID + "'");
                }
                else
                {
                    sb.AppendLine(" AND t.departmentid is null");
                }
                DataTable dt = MicrosoftOracle.ExecuteTable(conn, command, sb.ToString(), null);
                if (dt.Rows.Count > 0)
                {
                    sb.Clear();
                    flowCode = dt.Rows[0]["FLOWCODE"].ToString();
                    sb.AppendLine("insert into flow_flowdefine_t_history ");
                    sb.AppendLine("SELECT sys_guid(),FLOWCODE,DESCRIPTION,XOML,RULES,LAYOUT,FLOWTYPE,CREATEUSERID,CREATEUSERNAME,CREATECOMPANYID,CREATEDEPARTMENTID,");
                    sb.AppendLine("CREATEPOSTID,CREATEDATE,'" + flow.FlowDefinition.EDITUSERID + "' as EDITUSERID,'" + flow.FlowDefinition.EDITUSERNAME + "' as EDITUSERNAME,EDITDATE,");
                    sb.AppendLine("SYSTEMCODE,BUSINESSOBJECT,WFLAYOUT,FLOWCODE1 FROM FLOW_FLOWDEFINE_T  t");
                    sb.AppendLine("where t.FLOWCODE='" + flowCode + "'");
                    int n = MicrosoftOracle.ExecuteSQL(conn, command, sb.ToString(), null);
                    if (n > 0)
                    {                      
                        sb.Clear();
                        sb.AppendLine("delete FLOW_MODELFLOWRELATION_T t where FLOWCODE='" + flowCode + "'");
                        MicrosoftOracle.ExecuteSQL(conn, command, sb.ToString(), null);
                        sb.Clear();
                        sb.AppendLine("delete FLOW_FLOWDEFINE_T t where FLOWCODE='" + flowCode + "'");
                        MicrosoftOracle.ExecuteSQL(conn, command, sb.ToString(), null);
                    }
                }
                return flowCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 删除流程
        /// </summary>
        /// <param name="flowCodeList">删除Ｃｏｄｅ</param>
        /// <returns>是否成功</returns>
        public bool DeleteFlow(List<string> flowCodeList)
        {
            OracleConnection conn = MicrosoftOracle.CreateOracleConnection(ConnectionString);
            OracleCommand command = conn.CreateCommand();
            OracleTransaction transaction;
            transaction = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            command.Transaction = transaction;
            bool result = true;          
            try
            {             
                foreach (var flowCode in flowCodeList)
                {

                     string sql = "DELETE FROM  FLOW_MODELFLOWRELATION_T WHERE FLOWCODE='" + flowCode + "'";
                     int flow=  MicrosoftOracle.ExecuteNonQuery(conn, command, sql);
                    //流程与分类关系表
                     if (flow > 0 && result)
                    {
                        result = true;
                    }
                    else
                    {
                        transaction.Rollback();
                        result = false;                       
                    }

                    sql = "DELETE FROM FLOW_FLOWDEFINE_T  WHERE FLOWCODE='" + flowCode + "'";
                    flow = MicrosoftOracle.ExecuteNonQuery(conn, command, sql);
                    if (flow > 0 && result)
                    {
                        result = true;
                      
                    }
                    else
                    {
                        transaction.Rollback();
                        result = false;
                    }
                }
                transaction.Commit();
                conn.Close();
                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                conn.Close();
                throw new Exception(ex.Message, ex);
            }           
        }
        
        /// <summary>
        /// 根据FlowCode取流程
        /// </summary>
        /// <param name="flowCode">FlowCode</param>
        /// <returns>V_FLOWDEFINITION</returns>
        public V_FLOWDEFINITION GetFlowEntity(string flowCode)
        {
            string sql = "SELECT  a.FlowDefineID, a.FlowCode, a.Description, a.XOML, a.Rules, a.WFLayout, a.Layout, a.FlowType, a.CreateUserId, " +
                         "        a.CreateUserName, a.CreateCompanyId, a.CreateDepartmentID, a.CreatePostId, " +
                         "        a.CreateDate, a.EditUserId, a.EditUserName, a.EditDate,a.SYSTEMCODE,a.BUSINESSOBJECT, " +
                         "        b.MODELFLOWRELATIONID, b.COMPANYID, b.DEPARTMENTID, b.MODELCODE, " +
                          "        b.COMPANYNAME, b.DEPARTMENTNAME, b.SYSTEMCODE as SYSTEMCODE2, " +
                         "        b.FLAG, b.CREATEUSERID M_CreateUserId, b.CREATEUSERNAME M_CreateuserName, b.CREATECOMPANYID M_CreateCompanyId, " +
                         "        b.CREATEDEPARTMENTID M_CreateDepartmentId, b.CREATEPOSTID M_CreatePostId, b.CREATEDATE M_CreateDate, " +
                         "        b.EDITUSERID M_EditUserId, b.EDITUSERNAME M_EditUserName, b.EDITDATE M_EditDate " +
                         " FROM    Flow_FlowDefine_t a left join Flow_Modelflowrelation_t b on a.Flowcode = b.FlowCode " +
                         "WHERE   a.FlowCode = '" + flowCode + "' ";
            V_FLOWDEFINITION flowDefinition = new V_FLOWDEFINITION();
            try
            {
                dao.Open();
                DataTable dtFlow = dao.GetDataTable(sql);
                dao.Close();
                if (dtFlow.Rows.Count > 0)
                {
                    DataRow drFlow = dtFlow.Rows[0];
                    #region 流程定义表
                    flowDefinition.FlowDefinition = new FLOW_FLOWDEFINE_T()
                    {
                        FLOWDEFINEID = drFlow["FlowDefineID"].ToString(),
                        FLOWCODE = drFlow["FlowCode"].ToString(),
                        DESCRIPTION = drFlow["Description"].ToString(),
                        XOML = drFlow["XOML"].ToString(),
                        RULES = drFlow["Rules"].ToString(),
                        LAYOUT = drFlow["Layout"].ToString(),
                        WFLAYOUT = drFlow["WFLayout"].ToString(),
                        FLOWTYPE = drFlow["FlowType"].ToString(),
                        CREATEUSERID = drFlow["CreateUserId"].ToString(),
                        CREATEUSERNAME = drFlow["CreateUserName"].ToString(),
                        CREATECOMPANYID = drFlow["CreateCompanyId"].ToString(),
                        CREATEDEPARTMENTID = drFlow["CreateDepartmentID"].ToString(),
                        CREATEPOSTID = drFlow["CreatePostId"].ToString(),
                        CREATEDATE = DateTime.Parse(drFlow["CreateDate"].ToString()),
                        EDITUSERID = DBNull.Value.Equals(drFlow["EditUserId"]) ? string.Empty : drFlow["EditUserId"].ToString(),
                        EDITUSERNAME = DBNull.Value.Equals(drFlow["EditUserName"]) ? string.Empty : drFlow["EditUserName"].ToString(),
                        EDITDATE = DBNull.Value.Equals(drFlow["EditDate"]) ? DateTime.Now : DateTime.Parse(drFlow["EditDate"].ToString()),
                        SYSTEMCODE = drFlow["SYSTEMCODE"].ToString(),
                        BUSINESSOBJECT = drFlow["BUSINESSOBJECT"].ToString()
                    };
                    #endregion
                    #region 模块与流程定义关联表
                    if (!DBNull.Value.Equals(drFlow["MODELCODE"]))
                    {
                        flowDefinition.ModelRelation = new FLOW_MODELFLOWRELATION_T()
                        {
                            MODELFLOWRELATIONID = drFlow["MODELFLOWRELATIONID"].ToString(),
                            COMPANYID = drFlow["COMPANYID"].ToString(),
                            DEPARTMENTID = drFlow["DEPARTMENTID"].ToString(),

                            COMPANYNAME = drFlow["COMPANYNAME"].ToString(),//公司名称 
                            DEPARTMENTNAME = drFlow["DEPARTMENTNAME"].ToString(),//部门名称 
                            SYSTEMCODE = drFlow["SYSTEMCODE2"].ToString(),//系统代码 

                            MODELCODE = drFlow["MODELCODE"].ToString(),
                            FLOWCODE = drFlow["FLOWCODE"].ToString(),
                            FLAG = drFlow["FLAG"].ToString(),
                            FLOWTYPE = drFlow["FLOWTYPE"].ToString(),
                            CREATEUSERID = drFlow["M_CREATEUSERID"].ToString(),
                            CREATEUSERNAME = drFlow["M_CREATEUSERNAME"].ToString(),
                            CREATECOMPANYID = drFlow["M_CREATECOMPANYID"].ToString(),
                            CREATEDEPARTMENTID = drFlow["M_CREATEDEPARTMENTID"].ToString(),
                            CREATEPOSTID = drFlow["M_CREATEPOSTID"].ToString(),
                            CREATEDATE = DateTime.Parse(drFlow["M_CREATEDATE"].ToString()),
                            EDITUSERID = DBNull.Value.Equals(drFlow["M_EDITUSERID"]) ? string.Empty : drFlow["M_EDITUSERID"].ToString(),
                            EDITUSERNAME = DBNull.Value.Equals(drFlow["M_EDITUSERNAME"]) ? string.Empty : drFlow["M_EDITUSERNAME"].ToString(),
                            EDITDATE = DBNull.Value.Equals(drFlow["M_EditDate"]) ? DateTime.Now : DateTime.Parse(drFlow["M_EDITDATE"].ToString()),
                        };
                    }
                    #endregion
                    return flowDefinition;//返回
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion
     
        /// <summary>
        /// 返回字符串
        /// </summary>
        /// <param name="dr">行</param>
        /// <param name="filedName">列名称</param>
        /// <returns>string</returns>
        private string ConvertNullValue(DataRow dr, string filedName)
        {
            if (DBNull.Value.Equals(dr[filedName]))
            {
                return string.Empty;
            }
            else
            {
                return dr[filedName].ToString();
            }
        }
      
        /// <summary>
        /// 如果value是String;值为null则返回""字符串,否则返回value值。
        /// 如果value是DateTime;值为null则返回DBNull.Value字符串,否则返回value值。
        /// 如果value是Int32;值为null则返回0字符串,否则返回value值。
        /// </summary>
        /// <param name="value">value值</param>
        /// <param name="valuetype">值类型:string;datetime;int</param>
        /// <returns>object</returns>
        private object GetValue(object value, string valuetype)
        {
            switch (valuetype.ToLower())
            {
                case "string":
                    return value == null ? DBNull.Value.ToString() : value;
                case "datetime":
                    return value == null ? DBNull.Value : value;
                case "int":
                    return value == null ? 0 : value;
                default:
                    return value;
            }
        }
    }
}
