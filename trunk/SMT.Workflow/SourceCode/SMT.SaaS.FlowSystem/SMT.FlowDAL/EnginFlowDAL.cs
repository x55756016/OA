/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：EnginFlowDAL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 9:55:15   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.FlowDAL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Linq;
using System.IO;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Data.OracleClient;
using SMT.Workflow.Common.DataAccess;
using SMT.Workflow.SMTCache;
using EngineDataModel;

namespace SMT.FlowDAL
{
    public class EnginFlowDAL : BaseDAL
    {
        #region 我的单据

        /// <summary>
        /// 查询我的单据
        /// </summary>
        /// <param name="sysCode"></param>
        /// <param name="modelCode"></param>
        /// <param name="orderID"></param>
        /// <param name="owenrID"></param>
        /// <param name="companyID"></param>
        /// <param name="forwar"></param>
        /// <returns></returns>
        public string GetExistRecord(OracleConnection con, string sysCode, string modelCode, string orderID,  string forwar)
        {

            try
            {
                string sql = "SELECT PERSONALRECORDID FROM T_WF_PERSONALRECORD WHERE SYSTYPE='" + sysCode + "' AND MODELCODE='" + modelCode + "' AND MODELID='" +orderID + "' ";
                sql += " AND ISFORWARD=" + forwar + " AND rownum=1";
               // dao.Open();
                Log.WriteLog("GetExistRecord():" + sql + "");
                LogHelper.WriteLog(sql);
               // object obj = dao.ExecuteScalar(sql);
                DataTable obj= MsOracle.GetDataTableByTransaction(con, sql,null);
                return obj.Rows.Count > 0 ? obj.Rows[0]["PERSONALRECORDID"].ToString() : "";
            }

            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetExistRecord()", "查询是否存在我的单据", ex);
                return "";
            }
            finally
            {
                //dao.Close();
            }
        }

        /// <summary>
        /// 新增我的单据
        /// </summary>
        /// <param name="model"></param>       
        /// <returns></returns>
        public bool AddPersonalRecord(OracleConnection con, T_PF_PERSONALRECORD model)
        {

            try
            {
                string sql = @"INSERT INTO T_WF_PERSONALRECORD(PERSONALRECORDID,SYSTYPE,MODELCODE,MODELID,CHECKSTATE,OWNERID,OWNERPOSTID,OWNERDEPARTMENTID,
                              OWNERCOMPANYID,CONFIGINFO,MODELDESCRIPTION,ISFORWARD) VALUES 
                             ('" + Guid.NewGuid().ToString() + "','" + model.SYSTYPE + "','" + model.MODELCODE + "','" + model.MODELID + "'," + model.CHECKSTATE+ ",";
                sql += @"'" + model.OWNERID + "','" + model.OWNERPOSTID + "','" + model.OWNERDEPARTMENTID + "',";
                //sql += @"to_date('" + Convert.ToDateTime(entity.TRIGGERTIME).ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi:ss')          
                sql += @"'" + model.OWNERCOMPANYID + "','" + model.CONFIGINFO + "','" + model.MODELDESCRIPTION + "'," + (model.ISFORWARD == "1" ? 1 : 0) + ")";
                Log.WriteLog("AddPersonalRecord():" + sql + "");
               // dao.Open();
                //LogHelper.WriteLog("新增我的单据 FormID=" + model.MODELID + "");
                //return dao.ExecuteNonQuery(sql) > 0 ? true : false;
                return MsOracle.ExecuteSQLByTransaction(con, sql) >= 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "AddPersonalRecord()", "新增我的单据发生异常 FormID=" + model.MODELID + "", ex);
                return false;
            }
            finally
            {
               // dao.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="recordID"></param>
        /// <returns></returns>
        public bool UpdatePersonalRecord(OracleConnection con, T_PF_PERSONALRECORD model, string recordID)
        {

            try
            {
                string sql = "UPDATE T_WF_PERSONALRECORD SET CHECKSTATE=" + model.CHECKSTATE + ",CONFIGINFO='" + model.CONFIGINFO + "',MODELDESCRIPTION='" +model.MODELDESCRIPTION + "',";
                sql += "UPDATEDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi') WHERE PERSONALRECORDID='" + recordID + "'";
                Log.WriteLog("UpdatePersonalRecord():" + sql + "");
                //dao.Open();
                LogHelper.WriteLog("修改我的单据 FormID=" + model.MODELID + " SQL语句＝" + sql);
               // return dao.ExecuteNonQuery(sql) >= 0 ? true : false;
                return MsOracle.ExecuteSQLByTransaction(con, sql) >= 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "UpdatePersonalRecord()", "修改我的单据发生异常 FormID=" + model.MODELID + "", ex);
                return false;
            }
            finally
            {
               // dao.Close();
            }

        }
        #endregion
        #region 龙康才新增
        /// <summary>
        /// 关闭待办任务
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>
        /// <param name="strReceiveID"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public bool CloseDoTaskStatus(OracleConnection con, string strSystemCode, string strFormID, string strReceiveID)
        {
            string sql = "";
            try
            {//to_date('" + DateTime.Now + "','YYYY-MM-DD hh24:mi:ss')"
                sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now + "','YYYY-MM-DD hh24:mi:ss'),REMARK='关闭待办' WHERE SYSTEMCODE='" + strSystemCode + "' AND DOTASKSTATUS=0 AND ORDERID='" + strFormID + "'";
                if (!string.IsNullOrEmpty(strReceiveID))
                {
                    sql += "  AND RECEIVEUSERID='" + strReceiveID + "'";
                }
                //dao.Open();
                int result = MsOracle.ExecuteSQLByTransaction(con, sql);
                LogHelper.WriteLog("关闭待办任务 FormID=" + strFormID + " SQL语句＝" + sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("关闭待办任务出错 FormID=" + strFormID + " 命名空间： SMT.FlowDAL.EnginFlowDAL 类方法：CloseDoTaskStatus（）\r\n SQL语句：" + sql + ";\r\n异常信息：" + ex.Message);
                throw new Exception("关闭待办任务出错 FormID=" + strFormID);
            }
        }
        /// <summary>
        /// 流程咨讯关闭
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>
        /// <param name="strReceiveID"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public bool CloseConsultati(OracleConnection con, string strSystemCode, string strFormID, string strReceiveID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now + "','YYYY-MM-DD hh24:mi:ss'),REMARK='关闭流程咨询' WHERE SYSTEMCODE='" + strSystemCode + "' AND RECEIVEUSERID='" + strReceiveID + "' AND DOTASKSTATUS=0 AND ORDERID='" + strFormID + "'";

                int result = MsOracle.ExecuteSQLByTransaction(con, sql);
                LogHelper.WriteLog("流程咨讯关闭 FormID=" + strFormID + " SQL语句＝" + sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "CloseConsultati()", "流程咨讯关闭", ex);
                return false;
            }
        }
        /// <summary>
        /// 审核结束时关闭所有的流程资讯在待办中
        /// </summary>
        /// <param name="con"></param>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>
        /// <returns></returns>
        public bool CloseAllConsultati(OracleConnection con, string strSystemCode, string strFormID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now + "','YYYY-MM-DD hh24:mi:ss'),REMARK='关闭流程咨询' WHERE SYSTEMCODE='" + strSystemCode + "' AND DOTASKSTATUS=0 AND ORDERID='" + strFormID + "'";

                int result = MsOracle.ExecuteSQLByTransaction(con, sql);
                LogHelper.WriteLog("流程咨讯关闭 FormID=" + strFormID + " SQL语句＝" + sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "CloseAllConsultati()", "审核结束时关闭所有的流程资讯在待办中", ex);
                return false;
            }
        }
        /// <summary>
        /// 获取默认消息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMessageDefine(OracleConnection con, string key)
        {
            try
            {
                string sql = "SELECT MESSAGEBODY FROM T_WF_MESSAGEDEFINE WHERE MESSAGEKEY='" + key + "'";
                //dao.Open();
                return MsOracle.ExecuteScalarByTransaction(con, sql, null).ToString(); //dao.ExecuteScalar(sql).ToString();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetMessageDefine()", "获取默认消息", ex);
                return "";
            }
        }
        /// <summary>
        ///  流程数据是否匹配引擎配置界面所定义的条件
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strModelCode"></param>
        /// <param name="status"></param>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public DataTable FlowTriggerTable(OracleConnection con, string strSystemCode, string strModelCode, string status, string strCompanyID)
        {
            try
            {
                string sql = @"SELECT B.APPLICATIONURL,B.SYSTEMCODE,B.MODELCODE,B.MESSAGEBODY,B.ISDEFAULTMSG,
                B.RECEIVEUSER,B.LASTDAYS,B.WCFBINDINGCONTRACT,B.WCFURL,B.FUNCTIONNAME,B.FUNCTIONPARAMTER,B.PAMETERSPLITCHAR,B.OWNERDEPARTMENTID, 
                B.OWNERCOMPANYID,  
                B.OWNERPOSTID,  A.DOTASKRULEID  FROM T_WF_DOTASKRULE A LEFT JOIN   T_WF_DOTASKRULEDETAIL B ON A.DOTASKRULEID=B.DOTASKRULEID";
                sql += "  WHERE A.SYSTEMCODE='" + strSystemCode + "' AND A.MODELCODE='" + strModelCode + "' AND A.COMPANYID='" + strCompanyID + "'";
                if (!string.IsNullOrEmpty(status))
                {
                    sql += " AND A.TRIGGERORDERSTATUS=" + status + "";
                }
                LogHelper.WriteLog("流程数据是否匹配引擎配置界面所定义的（审核通过、审核不通过、审核中）消息规则 SQL语句＝" + sql);
                return MsOracle.GetDataTableByTransaction(con, sql, null);// dao.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "FlowTriggerTable()", "流程数据是否匹配引擎配置界面所定义的（审核通过、审核不通过、审核中）消息规则 发生异常：", ex);
                return null;
            }
        }
        /// <summary>
        /// 流程触发新增代办查询全局的默认消息
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strModelCode"></param>
        /// <param name="status"></param>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public DataTable FlowTriggerTable(OracleConnection con, string strSystemCode, string strModelCode, string status)
        {
            try
            {
                string sql = @" SELECT SYSTEMCODE,MODELCODE,APPLICATIONURL,MESSAGECONTENT MESSAGEBODY, 1 ISDEFAULTMSG,LASTDAYS FROM T_WF_DEFAULTMESSAGE ";
                sql += "  WHERE SYSTEMCODE='" + strSystemCode + "' AND MODELCODE='" + strModelCode + "'";
                if (!string.IsNullOrEmpty(status))
                {
                    sql += " AND AUDITSTATE=" + status + "";
                }
                LogHelper.WriteLog("流程触发新增代办查询（全局）的默认消息（审核通过、审核不通过、审核中）消息规则 SQL语句＝" + sql);
                return MsOracle.GetDataTableByTransaction(con, sql, null);// dao.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "FlowTriggerTable()", "流程触发新增代办查询（全局）的默认消息（审核通过、审核不通过、审核中）消息规则 发生异常：", ex);
                return null;
            }
        }
        public bool FlowConsultatiMsg(OracleConnection con, string strReceiveUser, string SystemCode, string strModelCode, string FormID, string Content, string Url, string APPFIELDVALUE, string strFlowXML, string strAppXML)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now + "','YYYY-MM-DD hh24:mi:ss'),REMARK='关闭流程咨询' WHERE SYSTEMCODE='" + SystemCode + "' AND RECEIVEUSERID='" + strReceiveUser + "' AND DOTASKSTATUS=0 AND ORDERID='" + FormID + "'";
                //dao.Open();
                //int result = dao.ExecuteNonQuery(con,sql);
                
                int result = MsOracle.ExecuteNonQuery(con, CommandType.Text, sql);
                LogHelper.WriteLog("关闭流程咨询（T_WF_DOTASK） FormID=" + FormID + "  SQL语句=" + sql);

                string addsql = @"INSERT INTO T_WF_DOTASK(DOTASKID,COMPANYID,ORDERID,MESSAGEBODY,APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,DOTASKTYPE，
                                DOTASKSTATUS,MAILSTATUS,RTXSTATUS,APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELCODE,REMARK) VALUES('" + Guid.NewGuid().ToString() + "',";
                addsql += "'','" + FormID + "','" + Content + "','" + Url + "','" + strReceiveUser + "','',1,0,0,0,'" + APPFIELDVALUE + "','" + strFlowXML + "','" + strAppXML + "','" + SystemCode + "','','流程咨询')";
                
                //int insert = MsOracle.ExecuteNonQuery(con, CommandType.Text, addsql);// dao.ExecuteNonQuery(addsql);
               
                //return true;

              string insSql = "INSERT INTO T_WF_DOTASK (DOTASKID,COMPANYID,ORDERID,MESSAGEBODY,APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,DOTASKTYPE,DOTASKSTATUS,MAILSTATUS,RTXSTATUS,APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELCODE,REMARK) VALUES (:DOTASKID,:COMPANYID,:ORDERID,:MESSAGEBODY,:APPLICATIONURL,:RECEIVEUSERID,:BEFOREPROCESSDATE,:DOTASKTYPE,:DOTASKSTATUS,:MAILSTATUS,:RTXSTATUS,:APPFIELDVALUE,:FLOWXML,:APPXML,:SYSTEMCODE,:MODELCODE,:REMARK)";
            OracleParameter[] pageparm =
                { 
                    new OracleParameter(":DOTASKID",GetValue(Guid.NewGuid().ToString())), //待办任务ID 
                    new OracleParameter(":COMPANYID",GetValue(null)), //公司ID 
                    new OracleParameter(":ORDERID",GetValue(FormID)), //单据ID 
                    new OracleParameter(":MESSAGEBODY",GetValue(Content)), //消息体 
                    new OracleParameter(":APPLICATIONURL",GetValue(Url)), //应用URL 
                    new OracleParameter(":RECEIVEUSERID",GetValue(strReceiveUser)), //接收用户ID 
                    new OracleParameter(":BEFOREPROCESSDATE",GetValue(null)), //可处理时间（主要针对KPI考核） 
                    new OracleParameter(":DOTASKTYPE",GetValue(1)), //待办任务类型(0、待办任务、1、流程咨询、3 ) 
                    new OracleParameter(":DOTASKSTATUS",GetValue(0)), //代办任务状态(0、未处理 1、已处理 、2、任务撤销 10、删除) 
                    new OracleParameter(":MAILSTATUS",GetValue(0)), //邮件状态(0、未发送 1、已发送、2、未知 ) 
                    new OracleParameter(":RTXSTATUS",GetValue(0)), //RTX状态(0、未发送 1、已发送、2、未知 ) 
                    new OracleParameter(":APPFIELDVALUE",GetValue(APPFIELDVALUE)), //应用字段值 
                    new OracleParameter(":FLOWXML",GetValue(strFlowXML)), //流程XML 
                    new OracleParameter(":APPXML",GetValue(strAppXML)), //应用XML 
                    new OracleParameter(":SYSTEMCODE",GetValue(SystemCode)), //系统代码 
                    new OracleParameter(":MODELCODE",GetValue(null)), //模块代码 
                    new OracleParameter(":REMARK",GetValue("流程咨询")) //备注 

                };

            int insert = MsOracle.ExecuteNonQuery(con, CommandType.Text, insSql, pageparm);// dao.ExecuteNonQuery(addsql);
            LogHelper.WriteLog("新增流程咨询（T_WF_DOTASK） FormID=" + FormID + "  SQL语句=" + addsql);
            return true;

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("流程咨询消息 FormID=" + FormID + " 数据库连接 =" + con.ConnectionString+ " 状态＝"+con.State.ToString()+" 发生异常:\r\n" + ex);
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                return false;
            }
            finally
            {
                //dao.Close();
            }
        }
        public void DoTaskCancel(OracleConnection con, string strSystemCode, string strModelCode, string strOrederID, string strReceiveID, string strContent)
        {
            try
            {
                //DOTASKSTATUS 代办任务状态(0、未处理 1、已处理 、2、任务撤销 )
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=2, CLOSEDDATE=to_date('" + DateTime.Now + "','YYYY-MM-DD hh24:mi:ss'),REMARK='任务撤销' WHERE DOTASKSTATUS=0 AND SYSTEMCODE='" + strSystemCode + "'  AND ORDERID='" + strOrederID + "'";
                //dao.Open();
                int result = MsOracle.ExecuteSQLByTransaction(con,sql);
                LogHelper.WriteLog("执行结果=" + result + " ;待办任务撤销（T_WF_DOTASK） FormID=" + strOrederID + "  SQL语句=" + sql);
                string addsql = @"INSERT INTO T_WF_DOTASKMESSAGE(DOTASKMESSAGEID,MESSAGEBODY,SYSTEMCODE,RECEIVEUSERID,ORDERID,MESSAGESTATUS,MAILSTATUS,RTXSTATUS,REMARK) VALUES('" + Guid.NewGuid().ToString() + "',";
                addsql += "'" + strContent + "','" + strSystemCode + "','" + strReceiveID + "','" + strOrederID + "',0,0,0,'新增任务消息')";
                int insert = MsOracle.ExecuteSQLByTransaction(con, sql);// dao.ExecuteNonQuery(addsql);
                LogHelper.WriteLog("执行结果=" + result + ";新增待办任务消息（T_WF_DOTASKMESSAGE） FormID=" + strOrederID + "  SQL语句=" + addsql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("待办任务撤销和新增待办任务消息发生异常："+ ex.ToString());
            }

        }
        /// <summary>
        /// 新增待办
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dr1"></param>
        /// <param name="SourceValueDT"></param>
        /// <param name="strAPPFIELDVALUE"></param>
        public void AddDoTask(OracleConnection con, T_WF_DOTASK entity, DataRow dr1, DataTable SourceValueDT, string strAPPFIELDVALUE)
        {
            CloseDoTaskStatus(con, entity.SYSTEMCODE, entity.ORDERID, null);
            try
            {
                string[] strListUser;
                if (entity.RECEIVEUSERID.IndexOf('|') != -1)
                {
                    strListUser = entity.RECEIVEUSERID.ToString().Split('|');
                }
                else
                {
                    strListUser = new string[1];
                    strListUser[0] = entity.RECEIVEUSERID.ToString();
                }
                //dao.Open();
                foreach (string User in strListUser)
                {
                    string insSql = @"INSERT INTO T_WF_DOTASK (DOTASKID,COMPANYID,ORDERID,ORDERUSERID,ORDERUSERNAME,ORDERSTATUS,MESSAGEBODY,
                                     APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,DOTASKTYPE,DOTASKSTATUS,MAILSTATUS,
                                     RTXSTATUS,APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELCODE,MODELNAME)
                                     VALUES (:DOTASKID,:COMPANYID,:ORDERID,:ORDERUSERID,:ORDERUSERNAME,:ORDERSTATUS,:MESSAGEBODY,:APPLICATIONURL,
                                    :RECEIVEUSERID,:BEFOREPROCESSDATE,:DOTASKTYPE,:DOTASKSTATUS,:MAILSTATUS,:RTXSTATUS,
                                    :APPFIELDVALUE,:FLOWXML,:APPXML,:SYSTEMCODE,:MODELCODE,:MODELNAME)";
                    OracleParameter[] pageparm =
                        {               
                            new OracleParameter(":DOTASKID",OracleType.NVarChar,100), 
                            new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERUSERID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERUSERNAME",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERSTATUS",OracleType.Number,22), 
                            new OracleParameter(":MESSAGEBODY",OracleType.NVarChar,4000), 
                            new OracleParameter(":APPLICATIONURL",OracleType.NVarChar,2000), 
                            new OracleParameter(":RECEIVEUSERID",OracleType.NVarChar,100), 
                            new OracleParameter(":BEFOREPROCESSDATE",OracleType.DateTime), 
                            new OracleParameter(":DOTASKTYPE",OracleType.Number,22),
                            new OracleParameter(":DOTASKSTATUS",OracleType.Number,22), 
                            new OracleParameter(":MAILSTATUS",OracleType.Number,22), 
                            new OracleParameter(":RTXSTATUS",OracleType.Number,22),                  
                            new OracleParameter(":APPFIELDVALUE",OracleType.Clob), 
                            new OracleParameter(":FLOWXML",OracleType.Clob), 
                            new OracleParameter(":APPXML",OracleType.Clob), 
                            new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":MODELNAME",OracleType.NVarChar,200),                  

                        };
                    pageparm[0].Value = MsOracle.GetValue(Guid.NewGuid().ToString());//待办任务ID
                    pageparm[1].Value = MsOracle.GetValue(entity.COMPANYID);//公司ID
                    pageparm[2].Value = MsOracle.GetValue(entity.ORDERID);//单据ID
                    pageparm[3].Value = MsOracle.GetValue(entity.ORDERUSERID);//单据所属人ID
                    pageparm[4].Value = MsOracle.GetValue(entity.ORDERUSERNAME);//单据所属人名称
                    pageparm[5].Value = MsOracle.GetValue(entity.ORDERSTATUS);//单据状态
                    #region 消息体
                    string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                                  "<System>" + "\r\n" +
                                  "{0}" +
                                  "</System>";
                    if (dr1["MESSAGEBODY"].ToString() == "")//默认消息为空
                    {
                        string strMsgBody = string.Empty;
                        string strMsgUrl = string.Empty;
                        ModelMsgDefine(con, dr1["SYSTEMCODE"].ToString(), dr1["MODELCODE"].ToString(), entity.COMPANYID, ref strMsgBody, ref strMsgUrl);
                        if (string.IsNullOrEmpty(strMsgBody))
                        {
                            try
                            {
                                DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
                                if (drvList.Count() == 1)
                                {
                                    string value = drvList[0]["ColumnValue"].ToString();
                                    if (string.IsNullOrWhiteSpace(value))
                                    {
                                        value = drvList[0]["ColumnText"].ToString();
                                    }         
                                    pageparm[6].Value = MsOracle.GetValue(value + "已审批通过");//消息体                                    
                                }
                                else
                                {
                                    pageparm[6].Value = MsOracle.GetValue(entity.ORDERID + "已审批通过");//消息体 

                                }
                            }
                            catch { }

                        }
                        else
                        {
                            pageparm[6].Value = MsOracle.GetValue(ReplaceMessageBody(strMsgBody, SourceValueDT));//消息体                             
                        }
                        string strUrl = string.Format(XmlTemplete, ReplaceValue(strMsgUrl, SourceValueDT));
                        LogHelper.WriteLog("查询到得消息链接：" + strUrl + "单据ID：" + entity.ORDERID);
                        pageparm[7].Value = MsOracle.GetValue(strUrl);//应用URL                         
                    }
                    else//在引擎配置界面定义了消息内容
                    {
                        LogHelper.WriteLog("Formid=" + entity.ORDERID + "开始 待办消息体:" + dr1["MESSAGEBODY"].ToString() + "\n\r 开始 打开待办连接的参数:" + dr1["APPLICATIONURL"].ToString());
                        string rowsValues = "Formid=" + entity.ORDERID + "\r\n";//每一行的值
                        for (int j = 0; j < SourceValueDT.Rows.Count; j++)
                        {
                            for (int i = 0; i < SourceValueDT.Columns.Count; i++)
                            {
                                string columnName = SourceValueDT.Columns[i].ColumnName;
                                rowsValues += columnName + "=" + SourceValueDT.Rows[j][columnName].ToString() + ";";
                            }
                            rowsValues += "\r\n";
                        }
                        LogHelper.WriteLog("SourceValueDT表数据:" + rowsValues);
                        pageparm[6].Value = MsOracle.GetValue(ReplaceMessageBody(dr1["MESSAGEBODY"].ToString(), SourceValueDT));//消息体
                        pageparm[7].Value = MsOracle.GetValue(string.Format(XmlTemplete, ReplaceValue(dr1["APPLICATIONURL"].ToString(), SourceValueDT)));//应用URL   
                        LogHelper.WriteLog("Formid=" + entity.ORDERID + "最后 待办消息体:" + pageparm[6].Value + "\n\r 最后 打开待办连接的参数:" + pageparm[7].Value);
                    }
                    #endregion                  
                    pageparm[8].Value =MsOracle.GetValue(User);// MsOracle.GetValue(entity.RECEIVEUSERID);//接收用户ID
                    if (entity.BEFOREPROCESSDATE != null)//流程过期时间属性
                    {
                        //sql += "to_date('" + entity.BEFOREPROCESSDATE + "','YYYY-MM-DD hh24:mi:ss')";
                        pageparm[9].Value = MsOracle.GetValue(entity.BEFOREPROCESSDATE);//可处理时间（主要针对KPI考核）
                    }
                    else
                    {
                        if (dr1["LASTDAYS"] != null)
                        {
                            if (string.IsNullOrEmpty(dr1["LASTDAYS"].ToString()))
                            {
                                pageparm[9].Value = MsOracle.GetValue(DateTime.Now.AddDays(3));//可处理时间（主要针对KPI考核）
                            }
                            else
                            {
                                pageparm[9].Value = MsOracle.GetValue(DateTime.Now.AddDays(int.Parse(dr1["LASTDAYS"].ToString())));//可处理时间（主要针对KPI考核）                             
                            }
                        }
                        else
                        {
                            pageparm[9].Value = MsOracle.GetValue(DateTime.Now.AddDays(3));
                        }
                    }
                    pageparm[10].Value = MsOracle.GetValue(0);//待办任务类型(0、待办任务、1、流程咨询、3 ) 
                    pageparm[11].Value = MsOracle.GetValue(0);//代办任务状态(0、未处理 1、已处理 、2、任务撤销 10、删除)
                    pageparm[12].Value = MsOracle.GetValue(0);//邮件状态(0、未发送 1、已发送、2、未知 )
                    pageparm[13].Value = MsOracle.GetValue(0);//RTX状态(0、未发送 1、已发送、2、未知 )
                    pageparm[14].Value = MsOracle.GetValue(strAPPFIELDVALUE);//应用字段值
                    pageparm[15].Value = MsOracle.GetValue(entity.FLOWXML);//流程XML
                    pageparm[16].Value = MsOracle.GetValue(entity.APPXML);//应用XML
                    pageparm[17].Value = MsOracle.GetValue(entity.SYSTEMCODE);//系统代码                  
                    pageparm[18].Value = MsOracle.GetValue(entity.MODELCODE);//模块代码
                    pageparm[19].Value = MsOracle.GetValue(entity.MODELNAME);//模块名称
                    //DataRow[] ModelCodeList = SourceValueDT.Select("ColumnName='ModelCode'");
                    //if (ModelCodeList.Count() == 1)
                    //{
                    //    sql += "'" + ModelCodeList[0]["ColumnValue"].ToString() + "')";
                    //}
                    //else
                    //{
                    //    sql += "'')";
                    //}
                    int result = MsOracle.ExecuteSQLByTransaction(con, insSql, pageparm);
                    if (result > 0)
                    {
                        LogHelper.WriteLog("A新增待办任务AddDoTask （成功）  FormID=" + entity.ORDERID + " 接收人ID＝" + User);
                    }
                    else
                    {
                        LogHelper.WriteLog("A新增待办任务AddDoTask （失败）  FormID=" + entity.ORDERID + " 接收人ID＝" + User); 
                    }
                }


            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("A新增待办任AddDoTask （失败） FormID=" + entity.ORDERID + " 命名空间： SMT.FlowDAL.EnginFlowDAL 类方法：AddDoTask（）" + ex.Message);
                throw new Exception("新增待办任失败 FormID="+entity.ORDERID);
            }

        }
        public void AddDoTask(OracleConnection con, T_WF_DOTASK entity, DataRow dr1, DataTable SourceValueDT, string strReceiveID, string ApplicationCode, string strAPPFIELDVALUE, string strMsg, string strFormTypes)
        {
            CloseDoTaskStatus(con, entity.SYSTEMCODE, entity.ORDERID, null);
            try
            {
                string insSql = @"INSERT INTO T_WF_DOTASK (DOTASKID,COMPANYID,ORDERID,ORDERUSERID,ORDERUSERNAME,ORDERSTATUS,MESSAGEBODY,
                                     APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,ENGINECODE,DOTASKTYPE,DOTASKSTATUS,MAILSTATUS,
                                     RTXSTATUS,APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELCODE,MODELNAME)
                                     VALUES (:DOTASKID,:COMPANYID,:ORDERID,:ORDERUSERID,:ORDERUSERNAME,:ORDERSTATUS,:MESSAGEBODY,:APPLICATIONURL,
                                    :RECEIVEUSERID,:BEFOREPROCESSDATE,:ENGINECODE,:DOTASKTYPE,:DOTASKSTATUS,:MAILSTATUS,:RTXSTATUS,
                                    :APPFIELDVALUE,:FLOWXML,:APPXML,:SYSTEMCODE,:MODELCODE,:MODELNAME)";
                #region
                #region
                OracleParameter[] pageparm =
                        {               
                            new OracleParameter(":DOTASKID",OracleType.NVarChar,100), 
                            new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERUSERID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERUSERNAME",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERSTATUS",OracleType.Number,22), 
                            new OracleParameter(":MESSAGEBODY",OracleType.NVarChar,4000), 
                             new OracleParameter(":APPLICATIONURL",OracleType.NVarChar,2000), 
                            new OracleParameter(":RECEIVEUSERID",OracleType.NVarChar,100), 
                            new OracleParameter(":BEFOREPROCESSDATE",OracleType.DateTime), 
                             new OracleParameter(":ENGINECODE",OracleType.NVarChar,100), 
                            new OracleParameter(":DOTASKTYPE",OracleType.Number,22),
                            new OracleParameter(":DOTASKSTATUS",OracleType.Number,22), 
                            new OracleParameter(":MAILSTATUS",OracleType.Number,22), 
                            new OracleParameter(":RTXSTATUS",OracleType.Number,22),                  
                            new OracleParameter(":APPFIELDVALUE",OracleType.Clob), 
                            new OracleParameter(":FLOWXML",OracleType.Clob), 
                            new OracleParameter(":APPXML",OracleType.Clob), 
                            new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":MODELNAME",OracleType.NVarChar,200),                  

                        };
                #endregion
                pageparm[0].Value = MsOracle.GetValue(Guid.NewGuid().ToString());//待办任务ID
                pageparm[1].Value = MsOracle.GetValue(entity.COMPANYID);//公司ID
                pageparm[2].Value = MsOracle.GetValue(ApplicationCode);//单据ID
                pageparm[3].Value = MsOracle.GetValue("");//单据所属人ID
                pageparm[4].Value = MsOracle.GetValue("");//单据所属人名称
                pageparm[5].Value = MsOracle.GetValue("");//单据状态
                #region 消息体
                string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                              "<System>" + "\r\n" +
                              "{0}" +
                              "</System>";
                if (dr1["MESSAGEBODY"].ToString() == "")//默认消息为空
                {
                    string strMsgBody = string.Empty;
                    string strMsgUrl = string.Empty;
                    ModelMsgDefine(con, dr1["SYSTEMCODE"].ToString(), dr1["MODELCODE"].ToString(), entity.COMPANYID, ref strMsgBody, ref strMsgUrl);
                    if (string.IsNullOrEmpty(strMsgBody))
                    {
                        try
                        {
                            DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
                            if (drvList.Count() == 1)
                            {
                                string value = drvList[0]["ColumnValue"].ToString();
                                if (string.IsNullOrWhiteSpace(value))
                                {
                                    value = drvList[0]["ColumnText"].ToString();
                                }                             
                                pageparm[6].Value = MsOracle.GetValue(value + "已审批通过");//消息体                                    
                            }
                            else
                            {
                                pageparm[6].Value = MsOracle.GetValue(ApplicationCode + "已审批通过");//消息体 

                            }
                        }
                        catch { }

                    }
                    else
                    {
                        pageparm[6].Value = MsOracle.GetValue(ReplaceMessageBody(strMsgBody, SourceValueDT));//消息体                             
                    }
                    if (string.IsNullOrEmpty(strFormTypes))
                    {
                        pageparm[7].Value = MsOracle.GetValue(string.Format(XmlTemplete, ReplaceValue(strMsgUrl, SourceValueDT)));//应用URL 
                        LogHelper.WriteLog("查询到得消息链接：" + pageparm[7].Value + "单据ID：" + entity.ORDERID);
                    }
                    else
                    {
                        pageparm[7].Value = MsOracle.GetValue(EncyptFormType(ReplaceValue(strMsgUrl, SourceValueDT), strFormTypes));//应用URL
                        LogHelper.WriteLog("查询到得消息链接：" + pageparm[7].Value + "单据ID：" + entity.ORDERID);
                    }
                }
                else//在引擎配置界面定义了消息内容
                {
                    pageparm[6].Value = MsOracle.GetValue(ReplaceMessageBody(strMsg, SourceValueDT));//消息体
                    pageparm[7].Value = MsOracle.GetValue(string.Format(XmlTemplete, ReplaceValue(dr1["APPLICATIONURL"].ToString(), SourceValueDT)));//应用URL   

                }
                #endregion              
                pageparm[8].Value = MsOracle.GetValue(strReceiveID);//接收用户ID
                if (entity.BEFOREPROCESSDATE != null)//流程过期时间属性
                {
                    //sql += "to_date('" + entity.BEFOREPROCESSDATE + "','YYYY-MM-DD hh24:mi:ss')";
                    pageparm[9].Value = MsOracle.GetValue(entity.BEFOREPROCESSDATE);//可处理时间（主要针对KPI考核）
                }
                else
                {
                    if (string.IsNullOrEmpty(dr1["LASTDAYS"].ToString()))
                    {
                        pageparm[9].Value = MsOracle.GetValue(DateTime.Now.AddDays(3));//可处理时间（主要针对KPI考核）
                    }
                    else
                    {
                        pageparm[9].Value = MsOracle.GetValue(DateTime.Now.AddDays(int.Parse(dr1["LASTDAYS"].ToString())));//可处理时间（主要针对KPI考核）                             
                    }
                }
                pageparm[10].Value = MsOracle.GetValue(dr1["DOTASKRULEID"].ToString());               
                pageparm[11].Value = MsOracle.GetValue(0);//待办任务类型(0、待办任务、1、流程咨询、3 ) 
                pageparm[12].Value = MsOracle.GetValue(0);//代办任务状态(0、未处理 1、已处理 、2、任务撤销 10、删除)
                pageparm[13].Value = MsOracle.GetValue(0);//邮件状态(0、未发送 1、已发送、2、未知 )
                pageparm[14].Value = MsOracle.GetValue(0);//RTX状态(0、未发送 1、已发送、2、未知 )
                pageparm[15].Value = MsOracle.GetValue(strAPPFIELDVALUE);//应用字段值
                pageparm[16].Value = MsOracle.GetValue(entity.FLOWXML);//流程XML
                pageparm[17].Value = MsOracle.GetValue(entity.APPXML);//应用XML
                pageparm[18].Value = MsOracle.GetValue(entity.SYSTEMCODE);//系统代码
                DataRow[] ModelCodeList = SourceValueDT.Select("ColumnName='ModelCode'");
                if (ModelCodeList.Count() == 1)
                {
                    pageparm[19].Value = ModelCodeList[0]["ColumnValue"];//模块                    
                }
                else
                {
                    pageparm[19].Value = "";//模块               
                }
                DataRow[] ModelCodeList1 = SourceValueDT.Select("ColumnName='ModelName'");
                if (ModelCodeList1.Count() == 1)
                {
                    pageparm[20].Value = ModelCodeList1[0]["ColumnValue"];//模块                    
                }
                else
                {
                    pageparm[20].Value = "";//模块               
                }
                //pageparm[19].Value = MsOracle.GetValue(entity.MODELCODE);//模块代码
                //pageparm[20].Value = MsOracle.GetValue(entity.MODELNAME);//模块名称
                #endregion
                int result = MsOracle.ExecuteSQLByTransaction(con, insSql, pageparm);// dao.ExecuteNonQuery(sql);
                if (result > 0)
                {
                    LogHelper.WriteLog("B新增待办任务AddDoTask （成功）  FormID=" + entity.ORDERID + " 接收人ID＝" + strReceiveID);
                }
                else
                {
                    LogHelper.WriteLog("B新增待办任务AddDoTask （失败）  FormID=" + entity.ORDERID + " 接收人ID＝" + strReceiveID);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("B新增待办任务发生AddDoTask（失败）FormID=" + entity.ORDERID + " 命名空间： SMT.FlowDAL.EnginFlowDAL 类方法：AddDoTask（）" + ex.Message);
              
                throw new Exception("新增待办任务发生异常 FormID=" + entity.ORDERID);

            }
        }
        /// <summary>
        /// 模块消息获取
        /// </summary>
        /// <param name="strSystemCode">系统代码</param>
        /// <param name="strModelCode">模块代码</param>
        /// <param name="strMsgBody">返回消息主体内容</param>
        /// <param name="strMsgUrl">返回消息链接</param>
        public void ModelMsgDefine(OracleConnection con, string strSystemCode, string strModelCode, string strCompanyCode, ref string strMsgBody, ref string strMsgUrl)
        {

            try
            {
                string sql = " SELECT MESSAGEBODY,MESSAGEURL FROM t_wf_messagebodydefine WHERE SYSTEMCODE='" + strSystemCode + "' AND MODELCODE='" + strModelCode + "' AND COMPANYID='" + strCompanyCode + "'";
                DataTable dt = MsOracle.GetDataTableByTransaction(con, sql, null);// dao.GetDataTable(sql);
                LogHelper.WriteLog("模块消息获取ModelMsgDefine()" + sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    strMsgBody = dt.Rows[0]["MESSAGEBODY"].ToString();
                    strMsgUrl = dt.Rows[0]["MESSAGEURL"].ToString();
                    LogHelper.WriteLog("获取待办设置信息:SQL="+sql+"\r\n 待办消息体:"+strMsgBody+"\r\n打开待办的连接参数:"+strMsgUrl);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "ModelMsgDefine()", "模块消息获取", ex);
            }
            finally
            {
                dao.Close();
            }


        }
        /// <summary>
        /// 新增待办任务消息
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="drDefine"></param>
        /// <param name="SourceValueDT"></param>
        public void AddDoTaskMessage(OracleConnection con, T_WF_DOTASK entity, DataRow drDefine, DataTable SourceValueDT)
        {
            string sql = "";
            CloseDoTaskStatus(con, entity.SYSTEMCODE, entity.ORDERID, null);
            try
            {
                string ReceiveUser = entity.RECEIVEUSERID;
                if ((string.IsNullOrEmpty(ReceiveUser) || ReceiveUser.ToUpper() == "END") && drDefine != null)
                {
                    entity.RECEIVEUSERID = drDefine["RECEIVEUSER"].ToString();
                }
                 sql = "INSERT INTO T_WF_DOTASKMESSAGE(DOTASKMESSAGEID,MESSAGEBODY,SYSTEMCODE,RECEIVEUSERID,ORDERID,COMPANYID,MESSAGESTATUS,MAILSTATUS,RTXSTATUS,REMARK) VALUES ('" + Guid.NewGuid().ToString() + "',";
                #region MESSAGEBODY
                if (drDefine != null)//在引擎定义了触发条件有消息定义
                {
                    string strMsgBody = drDefine["MESSAGEBODY"].ToString();
                    if (!string.IsNullOrEmpty(strMsgBody))
                    {
                        sql += "'" + ReplaceMessageBody(strMsgBody, SourceValueDT).Replace("\'","\"") + "'";
                    }
                    else
                    {
                        DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
                        if (drvList.Count() == 1)
                        {
                            sql += "'" + drvList[0]["ColumnValue"].ToString() + "已审批通过'";
                        }
                        else
                        {
                            DataRow[] drvList2 = SourceValueDT.Select("ColumnName='ModelCode'");
                            if (drvList2.Count() == 1)
                            {
                                sql += "'" + drvList2[0]["ColumnValue"].ToString() + "已审批通过'";
                            }
                            else
                            {
                                sql += "'已审批通过'";
                            }

                        }
                    }
                }
                else//在引擎配置界面定义了消息内容
                {
                    DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
                    if (drvList.Count() == 1)
                    {
                        sql += "'" + drvList[0]["ColumnValue"].ToString() + "已审批通过'";
                    }
                    else
                    {
                        DataRow[] drvList2 = SourceValueDT.Select("ColumnName='ModelCode'");
                        if (drvList2.Count() == 1)
                        {
                            sql += "'" + drvList2[0]["ColumnValue"].ToString() + "已审批通过'";
                        }
                        else
                        {
                            sql += "'已审批通过'";
                        }
                    }
                }
                #endregion
                sql += ",'" + entity.SYSTEMCODE + "','" + entity.RECEIVEUSERID + "','" + entity.ORDERID + "','" + entity.COMPANYID + "',0,0,0,'系统新增')";
                
                int result = MsOracle.ExecuteSQLByTransaction(con, sql, null);// dao.ExecuteNonQuery(sql);
                LogHelper.WriteLog("新增待办任务消息（成功） FormID=" + entity.ORDERID + " SQL语句=" + sql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("AddDoTaskMessage()新增待办任务消息（失败）:FormID=" + entity.ORDERID + "SQL语句=" +sql+" \r\n 异常信息:\r\n"+ ex.Message);
                throw new Exception("新增待办任务消息发生异常 FormID=" + entity.ORDERID);               
            }
        }
        #endregion
        #region 原来旧的
        //        /// <summary>
        //        /// 关闭待办任务
        //        /// </summary>
        //        /// <param name="strSystemCode"></param>
        //        /// <param name="strFormID"></param>
        //        /// <param name="strReceiveID"></param>
        //        /// <param name="con"></param>
        //        /// <returns></returns>
        //        public bool CloseDoTaskStatus(string strSystemCode, string strFormID, string strReceiveID)
        //        {
        //            try
        //            {
        //                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE='" + DateTime.Now + "',REMARK='关闭待办' WHERE SYSTEMCODE='" + strSystemCode + "' AND DOTASKSTATUS=0 AND ORDERID='" + strFormID + "'";
        //                if (!string.IsNullOrEmpty(strReceiveID))
        //                {
        //                    sql += "  AND RECEIVEUSERID='" + strReceiveID + "'";
        //                }
        //                dao.Open();
        //                int result = dao.ExecuteNonQuery(sql);
        //                return result > -1 ? true : false;
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "CloseDoTaskStatus()", "关闭待办任务", ex);
        //                return false;
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }
        //        }

        //        /// <summary>
        //        /// 流程咨讯关闭
        //        /// </summary>
        //        /// <param name="strSystemCode"></param>
        //        /// <param name="strFormID"></param>
        //        /// <param name="strReceiveID"></param>
        //        /// <param name="con"></param>
        //        /// <returns></returns>
        //        public bool CloseConsultati(string strSystemCode, string strFormID, string strReceiveID)
        //        {
        //            try
        //            {
        //                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE='" + DateTime.Now + "',REMARK='关闭流程咨询' WHERE SYSTEMCODE='" + strSystemCode + "' AND RECEIVEUSERID='" + strReceiveID + "' AND DOTASKSTATUS=0 AND ORDERID='" + strFormID + "'";
        //                dao.Open();
        //                int result = dao.ExecuteNonQuery(sql);
        //                return result > -1 ? true : false;
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "CloseConsultati()", "流程咨讯关闭", ex);
        //                return false;
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }
        //        }

        //        /// <summary>
        //        /// 获取默认消息
        //        /// </summary>
        //        /// <param name="key"></param>
        //        /// <returns></returns>
        //        public string GetMessageDefine(string key)
        //        {
        //            try
        //            {
        //                string sql = "SELECT MESSAGEBODY FROM T_WF_MESSAGEDEFINE WHERE MESSAGEKEY='" + key + "'";
        //                dao.Open();
        //                return dao.ExecuteScalar(sql).ToString();
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "GetMessageDefine()", "获取默认消息", ex);
        //                return "";
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }
        //        }

        //        /// <summary>
        //        ///  流程数据是否匹配引擎配置界面所定义的条件
        //        /// </summary>
        //        /// <param name="strSystemCode"></param>
        //        /// <param name="strModelCode"></param>
        //        /// <param name="status"></param>
        //        /// <param name="strCompanyID"></param>
        //        /// <returns></returns>
        //        public DataTable FlowTriggerTable(string strSystemCode, string strModelCode, string status, string strCompanyID)
        //        {
        //            try
        //            {
        //                string sql = "SELECT B.APPLICATIONURL,B.SYSTEMCODE,B.MODELCODE,B.MESSAGEBODY,B.ISDEFAULTMSG,B.RECEIVEUSER,B.LASTDAYS,A.DOTASKRULEID FROM T_WF_DOTASKRULE A LEFT JOIN  T_WF_DOTASKRULEDETAIL B ON A.DOTASKRULEID=B.DOTASKRULEID";
        //                sql += "  WHERE A.SYSTEMCODE='" + strSystemCode + "' AND A.MODELCODE='" + strModelCode + "' AND A.COMPANYID='" + strCompanyID + "'";
        //                if (!string.IsNullOrEmpty(status))
        //                {
        //                    sql += " AND A.TRIGGERORDERSTATUS=" + status + "";
        //                }
        //                dao.Open();
        //                return dao.GetDataTable(sql);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "FlowTriggerTable()", "流程数据是否匹配引擎配置界面所定义的条件", ex);
        //                return null;
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }
        //        }
        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        /// <param name="strReceiveUser"></param>
        //        /// <param name="SystemCode"></param>
        //        /// <param name="strModelCode"></param>
        //        /// <param name="FormID"></param>
        //        /// <param name="Content"></param>
        //        /// <param name="Url"></param>
        //        /// <param name="APPFIELDVALUE"></param>
        //        /// <param name="strFlowXML"></param>
        //        /// <param name="strAppXML"></param>
        //        /// <returns></returns>
        //        public bool FlowConsultatiMsg(string strReceiveUser, string SystemCode, string strModelCode, string FormID, string Content, string Url, string APPFIELDVALUE, string strFlowXML, string strAppXML)
        //        {
        //            try
        //            {
        //                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE='" + DateTime.Now + "',REMARK='关闭流程咨询' WHERE SYSTEMCODE='" + SystemCode + "' AND RECEIVEUSERID='" + strReceiveUser + "' AND DOTASKSTATUS=0 AND ORDERID='" + FormID + "'";
        //                dao.Open();
        //                int result = dao.ExecuteNonQuery(sql);
        //                string addsql = @"INSERT INTO T_WF_DOTASK(DOTASKID,COMPANYID,ORDERID,MESSAGEBODY,APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,DOTASKTYPE，
        //                                DOTASKSTATUS,MAILSTATUS,RTXSTATUS,APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELCODE,REMARK) VALUES('" + Guid.NewGuid().ToString() + "',";
        //                addsql += "'','" + FormID + "','" + Content + "','" + Url + "','" + strReceiveUser + "','',1,0,0,0,'" + APPFIELDVALUE + "','" + strFlowXML + "','" + strAppXML + "','" + SystemCode + "','','流程咨询')";
        //                int insert = dao.ExecuteNonQuery(addsql);
        //                return true;

        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "FlowConsultatiMsg()", "FlowConsultatiMsg", ex);
        //                return false;
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }
        //        }

        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        /// <param name="strSystemCode"></param>
        //        /// <param name="strModelCode"></param>
        //        /// <param name="strOrederID"></param>
        //        /// <param name="strReceiveID"></param>
        //        /// <param name="strContent"></param>
        //        public void DoTaskCancel(string strSystemCode, string strModelCode, string strOrederID, string strReceiveID, string strContent)
        //        {
        //            try
        //            {
        //                //DOTASKSTATUS 代办任务状态(0、未处理 1、已处理 、2、任务撤销 )
        //                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=2, CLOSEDDATE='" + DateTime.Now + "',REMARK='任务撤销' WHERE DOTASKSTATUS=0 AND SYSTEMCODE='" + strSystemCode + "'  AND ORDERID='" + strOrederID + "'";
        //                dao.Open();
        //                int result = dao.ExecuteNonQuery(sql);
        //                string addsql = @"INSERT INTO T_WF_DOTASKMESSAGE(DOTASKMESSAGEID,MESSAGEBODY,SYSTEMCODE,RECEIVEUSERID,ORDERID,COMPANYID,MESSAGESTATUS,MAILSTATUS,RTXSTATUS,REMARK) VALUES('" + Guid.NewGuid().ToString() + "',";
        //                addsql += "" + strContent + "','" + strSystemCode + "','" + strReceiveID + "','" + strOrederID + "',0,0,0,'任务撤销消息')";
        //                int insert = dao.ExecuteNonQuery(addsql);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "DoTaskCancel()", "DoTaskCancel", ex);
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }

        //        }

        //        /// <summary>
        //        /// 新增待办
        //        /// </summary>
        //        /// <param name="entity"></param>
        //        /// <param name="dr1"></param>
        //        /// <param name="SourceValueDT"></param>
        //        /// <param name="strAPPFIELDVALUE"></param>
        //        public void AddDoTask(T_WF_DOTASK entity, DataRow dr1, DataTable SourceValueDT, string strAPPFIELDVALUE)
        //        {
        //            CloseDoTaskStatus(entity.SYSTEMCODE, entity.ORDERID, null);
        //            try
        //            {
        //                string[] strListUser;
        //                if (entity.RECEIVEUSERID.IndexOf('|') != -1)
        //                {
        //                    strListUser = entity.RECEIVEUSERID.ToString().Split('|');
        //                }
        //                else
        //                {
        //                    strListUser = new string[1];
        //                    strListUser[0] = entity.RECEIVEUSERID.ToString();
        //                }
        //                dao.Open();
        //                foreach (string User in strListUser)
        //                {
        //                    string sql = @"INSERT INTO T_WF_DOTASK(DOTASKID,COMPANYID,ORDERID,MESSAGEBODY,APPLICATIONURL,RECEIVEUSERID,
        //                              ORDERUSERID,ORDERUSERNAME,ORDERSTATUS,
        //                              BEFOREPROCESSDATE,ENGINECODE,DOTASKTYPE,DOTASKSTATUS,MAILSTATUS,RTXSTATUS,
        //                              APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELNAME,MODELCODE) VALUES ('" + Guid.NewGuid().ToString() + "','" + entity.COMPANYID + "','" + entity.ORDERID + "',";
        //                    string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
        //                                       "<System>" + "\r\n" +
        //                                       "{0}" +
        //                                       "</System>";
        //                    #region
        //                    if (dr1["MESSAGEBODY"].ToString() == "")//默认消息为空
        //                    {
        //                        string strMsgBody = string.Empty;
        //                        string strMsgUrl = string.Empty;
        //                        ModelMsgDefine(dr1["SYSTEMCODE"].ToString(), dr1["MODELCODE"].ToString(), entity.COMPANYID, ref strMsgBody, ref strMsgUrl);
        //                        if (string.IsNullOrEmpty(strMsgBody))
        //                        {
        //                            try
        //                            {
        //                                DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
        //                                if (drvList.Count() == 1)
        //                                {
        //                                    sql += "'" + drvList[0]["ColumnText"].ToString() + "已审批通过'";
        //                                }
        //                                else
        //                                {
        //                                    sql += "'" + entity.ORDERID + "已审批通过'";

        //                                }
        //                            }
        //                            catch { }

        //                        }
        //                        else
        //                        {
        //                            sql += "'" + ReplaceMessageBody(strMsgBody, SourceValueDT) + "'";
        //                        }
        //                        string strUrl = string.Format(XmlTemplete, ReplaceValue(strMsgUrl, SourceValueDT));
        //                        sql += ",'" + strUrl + "',";
        //                    }
        //                    else//在引擎配置界面定义了消息内容
        //                    {
        //                        sql += "'" + ReplaceMessageBody(dr1["MESSAGEBODY"].ToString(), SourceValueDT) + "',";
        //                        sql += "'" + string.Format(XmlTemplete, ReplaceValue(dr1["APPLICATIONURL"].ToString(), SourceValueDT)) + "',";
        //                    }
        //                    #endregion

        //                    sql += "'" + entity.RECEIVEUSERID + "','" + entity.ORDERUSERID + "','" + entity.ORDERUSERNAME + "'," + entity.ORDERSTATUS + ",";
        //                    if (entity.BEFOREPROCESSDATE != null)//流程过期时间属性
        //                    {
        //                        sql += "to_date('" + entity.BEFOREPROCESSDATE + "','YYYY-MM-DD hh24:mi:ss')";
        //                    }
        //                    else
        //                    {
        //                        if (string.IsNullOrEmpty(dr1["LASTDAYS"].ToString()))
        //                        {
        //                            sql += "to_date('" + DateTime.Now.AddDays(3) + "','YYYY-MM-DD hh24:mi:ss'),";

        //                        }
        //                        else
        //                        {
        //                            sql += "to_date('" + DateTime.Now.AddDays(int.Parse(dr1["LASTDAYS"].ToString())) + "','YYYY-MM-DD hh24:mi:ss'),";
        //                        }
        //                    }
        //                    sql += "'',";
        //                    sql += "0,0,0,0,'" + strAPPFIELDVALUE + "','" + entity.FLOWXML + "','" + entity.APPXML + "','" + entity.SYSTEMCODE + "','" + entity.MODELNAME + "',";
        //                    DataRow[] ModelCodeList = SourceValueDT.Select("ColumnName='ModelCode'");
        //                    if (ModelCodeList.Count() == 1)
        //                    {
        //                        sql += "'" + ModelCodeList[0]["ColumnValue"].ToString() + "')";
        //                    }
        //                    else
        //                    {
        //                        sql += "'')";
        //                    }
        //                    int result = dao.ExecuteNonQuery(sql);
        //                }


        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "AddDoTask(T_WF_DOTASK entity, DataRow dr1, DataTable SourceValueDT, string strAPPFIELDVALUE)", "新增待办任务", ex);
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }
        //        }

        //        /// <summary>
        //        /// 新增待办任务
        //        /// </summary>
        //        /// <param name="entity"></param>
        //        /// <param name="dr1"></param>
        //        /// <param name="SourceValueDT"></param>
        //        /// <param name="strReceiveID"></param>
        //        /// <param name="ApplicationCode"></param>
        //        /// <param name="strAPPFIELDVALUE"></param>
        //        /// <param name="strMsg"></param>
        //        /// <param name="strFormTypes"></param>
        //        public void AddDoTask(T_WF_DOTASK entity, DataRow dr1, DataTable SourceValueDT, string strReceiveID, string ApplicationCode, string strAPPFIELDVALUE, string strMsg, string strFormTypes)
        //        {
        //            CloseDoTaskStatus(entity.SYSTEMCODE, entity.ORDERID, null);
        //            try
        //            {

        //                string sql = @"INSERT INTO T_WF_DOTASK(DOTASKID,COMPANYID,ORDERID,MESSAGEBODY,APPLICATIONURL,RECEIVEUSERID,
        //                              ORDERUSERID,ORDERUSERNAME,ORDERSTATUS,
        //                              BEFOREPROCESSDATE,ENGINECODE,DOTASKTYPE,DOTASKSTATUS,MAILSTATUS,RTXSTATUS,
        //                              APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELNAME,MODELCODE) VALUES ('" + Guid.NewGuid().ToString() + "','" + entity.COMPANYID + "','" + entity.ORDERID + "',";
        //                string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
        //                                   "<System>" + "\r\n" +
        //                                   "{0}" +
        //                                   "</System>";
        //                #region
        //                if (dr1["MESSAGEBODY"].ToString() == "")
        //                {
        //                    string strMsgBody = string.Empty;
        //                    string strMsgUrl = string.Empty;
        //                    ModelMsgDefine(dr1["SYSTEMCODE"].ToString(), dr1["MODELCODE"].ToString(), entity.COMPANYID, ref strMsgBody, ref strMsgUrl);
        //                    if (string.IsNullOrEmpty(strMsgBody))
        //                    {
        //                        try
        //                        {
        //                            DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
        //                            if (drvList.Count() == 1)
        //                            {
        //                                sql += "'" + drvList[0]["ColumnValue"].ToString() + "已审批通过'";
        //                            }
        //                            else
        //                            {
        //                                sql += "'" + ApplicationCode + "已审批通过'";
        //                            }
        //                        }
        //                        catch { }

        //                    }
        //                    else
        //                    {
        //                        sql += "'" + ReplaceMessageBody(strMsgBody, SourceValueDT) + "'";
        //                    }
        //                    string strUrl = string.Empty;
        //                    if (string.IsNullOrEmpty(strFormTypes))
        //                    {
        //                        strUrl = string.Format(XmlTemplete, ReplaceValue(strMsgUrl, SourceValueDT));
        //                    }
        //                    else
        //                    {
        //                        strUrl = EncyptFormType(ReplaceValue(strMsgUrl, SourceValueDT), strFormTypes);
        //                    }
        //                    sql += ",'" + strUrl + "',";
        //                }
        //                else//在引擎配置界面定义了消息内容
        //                {
        //                    sql += "'" + ReplaceMessageBody(strMsg, SourceValueDT) + "',";
        //                    sql += "'" + string.Format(XmlTemplete, ReplaceValue(dr1["APPLICATIONURL"].ToString(), SourceValueDT)) + "',";

        //                }
        //                #endregion
        //                sql += "'" + strReceiveID + "','" + entity.BEFOREPROCESSDATE + "','" + dr1["DOTASKRULECODE"].ToString() + "',0,0,0,0,'" + strAPPFIELDVALUE + "','" + entity.FLOWXML + "','" + entity.APPXML + "','" + entity.SYSTEMCODE + "','" + entity.MODELNAME + "',";
        //                DataRow[] ModelCodeList = SourceValueDT.Select("ColumnName='ModelCode'");
        //                if (ModelCodeList.Count() == 1)
        //                {
        //                    sql += "'" + ModelCodeList[0]["ColumnValue"].ToString() + "')";
        //                }
        //                else
        //                {
        //                    sql += "'')";
        //                }
        //                dao.Open();
        //                int result = dao.ExecuteNonQuery(sql);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "AddDoTask()", "新增待办任务", ex);
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }
        //        }



        //        /// <summary>
        //        /// 新增待办任务消息
        //        /// </summary>
        //        /// <param name="entity"></param>
        //        /// <param name="drDefine"></param>
        //        /// <param name="SourceValueDT"></param>
        //        public void AddDoTaskMessage(T_WF_DOTASK entity, DataRow drDefine, DataTable SourceValueDT)
        //        {

        //            CloseDoTaskStatus(entity.SYSTEMCODE, entity.ORDERID, null);
        //            try
        //            {
        //                string ReceiveUser = entity.RECEIVEUSERID;
        //                if ((string.IsNullOrEmpty(ReceiveUser) || ReceiveUser.ToUpper() == "END") && drDefine != null)
        //                {
        //                    entity.RECEIVEUSERID = drDefine["RECEIVEUSER"].ToString();
        //                }
        //                string sql = "INSERT INTO T_WF_DOTASKMESSAGE(DOTASKMESSAGEID,MESSAGEBODY,SYSTEMCODE,RECEIVEUSER,ORDERID,COMPANYID,MESSAGESTATUS,MAILSTATUS,RTXSTATUS,REMARK) VALUES ('" + Guid.NewGuid().ToString() + "',";
        //                #region MESSAGEBODY
        //                if (drDefine != null)//在引擎定义了触发条件有消息定义
        //                {
        //                    string strMsgBody = drDefine["MESSAGEBODY"].ToString();
        //                    if (!string.IsNullOrEmpty(strMsgBody))
        //                    {
        //                        sql += "'" + ReplaceMessageBody(strMsgBody, SourceValueDT) + "'";
        //                    }
        //                    else
        //                    {
        //                        DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
        //                        if (drvList.Count() == 1)
        //                        {
        //                            sql += "'" + drvList[0]["ColumnValue"].ToString() + "已审批通过'";
        //                        }
        //                        else
        //                        {
        //                            DataRow[] drvList2 = SourceValueDT.Select("ColumnName='ModelCode'");
        //                            if (drvList2.Count() == 1)
        //                            {
        //                                sql += "'" + drvList2[0]["ColumnValue"].ToString() + "已审批通过'";
        //                            }
        //                            else
        //                            {
        //                                sql += "'已审批通过'";
        //                            }

        //                        }
        //                    }
        //                }
        //                else//在引擎配置界面定义了消息内容
        //                {
        //                    DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
        //                    if (drvList.Count() == 1)
        //                    {
        //                        sql += "'" + drvList[0]["ColumnValue"].ToString() + "已审批通过'";
        //                    }
        //                    else
        //                    {
        //                        DataRow[] drvList2 = SourceValueDT.Select("ColumnName='ModelCode'");
        //                        if (drvList2.Count() == 1)
        //                        {
        //                            sql += "'" + drvList2[0]["ColumnValue"].ToString() + "已审批通过'";
        //                        }
        //                        else
        //                        {
        //                            sql += "'已审批通过'";
        //                        }
        //                    }
        //                }
        //                #endregion
        //                sql += ",'" + entity.SYSTEMCODE + "','" + entity.RECEIVEUSERID + "','" + entity.ORDERID + "','" + entity.COMPANYID + "',0,0,0,'系统新增')";
        //                dao.Open();
        //                int result = dao.ExecuteNonQuery(sql);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "AddDoTaskMessage()", "新增待办任务消息", ex);
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }

        //        }


        //        /// <summary>
        //        /// 模块消息获取
        //        /// </summary>
        //        /// <param name="strSystemCode">系统代码</param>
        //        /// <param name="strModelCode">模块代码</param>
        //        /// <param name="strMsgBody">返回消息主体内容</param>
        //        /// <param name="strMsgUrl">返回消息链接</param>
        //        public void ModelMsgDefine(string strSystemCode, string strModelCode, string strCompanyCode, ref string strMsgBody, ref string strMsgUrl)
        //        {

        //            try
        //            {
        //                string sql = " SELECT MESSAGEBODY,MESSAGEURL FROM T_WF_MESSAGEDEFINE WHERE SYSTEMCODE='" + strSystemCode + "' AND MODELCODE='" + strModelCode + "' AND COMPANYID='" + strCompanyCode + "'";
        //                DataTable dt = dao.GetDataTable(sql);
        //                if (dt != null && dt.Rows.Count > 0)
        //                {
        //                    strMsgBody = dt.Rows[0]["MESSAGEBODY"].ToString();
        //                    strMsgUrl = dt.Rows[0]["MESSAGEURL"].ToString();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog(this, "ModelMsgDefine()", "模块消息获取", ex);
        //            }
        //            finally
        //            {
        //                dao.Close();
        //            }


        //        }

        #region 方法

        /// <summary>
        /// 元数据替换 DataValue 适用于替换 Guid
        /// </summary>
        /// <param name="PorcessString"></param>
        /// <param name="SourceValueDT"></param>
        /// <returns></returns>
        public string ReplaceValue(string PorcessString, DataTable SourceValueDT)
        {
            string temp = "=====================================\r\n";
            for (int i = 0; i < SourceValueDT.Rows.Count; i++)
            {
                temp += i.ToString() + "|" + SourceValueDT.Rows[i]["ColumnName"].ToString() + "=" + SourceValueDT.Rows[i]["ColumnValue"].ToString() + "\r\n";
            }
            temp +="开始：\r\n"+ PorcessString + "\r\n";
            foreach (DataRow dr in SourceValueDT.Rows)
            {              
                if (!string.IsNullOrEmpty(dr["ColumnValue"].ToString().Trim()))
                {
                    PorcessString = PorcessString.Replace("{" + dr["ColumnName"].ToString() + "}", dr["ColumnValue"].ToString());
                }
            }
            temp += "结束：\r\n" + PorcessString + "\r\n";
            temp += "=====================================\r\n";
            LogHelper.WriteLog("URL临时值" + temp);
            return PorcessString;
        }
        public string EncyptFormType(string Url, string strFormTypes)
        {
            if (!string.IsNullOrEmpty(Url))
            {

                try
                {
                    Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Url);
                    XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                    (from c in xele.Descendants("FormTypes") select c).FirstOrDefault().Value = strFormTypes;
                    string xmlHead = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "\r\n" + xele.ToString();
                    return xmlHead;
                }
                catch
                {

                }
                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 数据替换（将带有{}）字段进行替换
        /// </summary>
        /// <param name="InString">需要替换的字符串</param>
        /// <param name="valueTable">源数据DataTable</param>
        /// <returns></returns>
        private string ReplaceMessageBody(string InString, DataTable valueTable)
        {
           
       

            string temp = "";
            string[] KEY_SQLNODE = InString.Split('{');
            for (int k = 0; k < KEY_SQLNODE.Length; k++)
            {
                if ((KEY_SQLNODE[k].Split('}')).Length == 2)
                {
                    string Node = (KEY_SQLNODE[k].Split('}'))[0];
                    string[] NodeList = Node.Split(':');
                    DataRow[] drvList = valueTable.Select("ColumnName='" + NodeList[1].Trim() + "'");
                    if (NodeList[0].ToUpper().Trim() == "NEW" && drvList.Length > 0)
                    {
                        string newValue = GetColumnString(drvList[0], "ColumnValue");
                        newValue = GetColumnString(drvList[0], "ColumnText");
                        newValue = newValue.Replace("'", "''");
                        InString = InString.Replace("{" + NodeList[0] + ":" + NodeList[1] + "}", newValue);
                        temp += "有NEW{" + NodeList[0] + ":" + NodeList[1] + "}=" + newValue + "\r\n";
                    }
                    else
                    {
                        InString = InString.Replace("{" + NodeList[0] + ":" + NodeList[1] + "}", "");
                        temp += "没NEW{" + NodeList[0] + ":" + NodeList[1] + "}=空\r\n";
                    }
                    //string Node = (KEY_SQLNODE[k].Split('}'))[0];
                    //string[] NodeList = Node.Split(':');
                    //DataRow[] drvList = valueTable.Select("ColumnName='" + NodeList[1].Trim() + "'");
                    //if (NodeList[0].ToUpper().Trim() == "NEW" && drvList.Length > 0)
                    //{
                    //    string newValue = GetColumnString(drvList[0], "ColumnValue");
                    //    if (string.IsNullOrWhiteSpace(newValue))
                    //    {
                    //        newValue = GetColumnString(drvList[0], "ColumnText");
                    //    }                
                    //    //string newValue = GetColumnString(drvList[0], "ColumnText");
                    //    newValue = newValue.Replace("'", "''");
                    //    InString = InString.Replace("{" + NodeList[0] + ":" + NodeList[1] + "}", newValue);
                    //    temp += "有NEW{" + NodeList[0] + ":" + NodeList[1] + "}=" + newValue + "\r\n";
                    //}
                    //else
                    //{
                    //    InString = InString.Replace("{" + NodeList[0] + ":" + NodeList[1] + "}", "");
                    //    temp += "没NEW{" + NodeList[0] + ":" + NodeList[1] + "}=空\r\n";
                    //}
                }
            }
            LogHelper.WriteLog("待办消息替换过程:\r\n"+temp);
            return InString;

        }
        private object GetDataRowValue(DataRow dr, string colname)
        {
            if (dr[colname] == DBNull.Value)
            {
                switch ((dr[colname]).GetType().ToString())
                {
                    case "System.String":
                        return "";
                    case "System.Int64":
                        return 0;
                    case "System.Decimal":
                        return 0;
                    case "System.Int32":
                        return 0;
                    case "System.Double":
                        return 0;
                    case "System.Boolean":
                        return false;
                    case "System.DateTime":
                        return "";
                    default:
                        return "";
                }
            }
            return dr[colname];
        }
        private string GetColumnString(DataRow dr, string colname)
        {
            try
            {
                object value = GetDataRowValue(dr, colname);
                if (value == null)
                {
                    return "";
                }
                switch (value.GetType().ToString())
                {
                    case "System.String":
                        return (string)value;
                    case "System.Int64":
                        return ((long)value).ToString();
                    case "System.Decimal":
                        return ((decimal)value).ToString();
                    case "System.Int32":
                        return ((int)value).ToString();
                    case "System.Double":
                        return ((double)value).ToString();
                    case "System.Boolean":
                        return ((bool)value).ToString();
                    case "System.DateTime":
                        return ((DateTime)value).ToString();
                    default:
                        return "";
                }
            }
            catch
            {
                return "";
            }
        }
        #endregion
        #endregion


    }
}
