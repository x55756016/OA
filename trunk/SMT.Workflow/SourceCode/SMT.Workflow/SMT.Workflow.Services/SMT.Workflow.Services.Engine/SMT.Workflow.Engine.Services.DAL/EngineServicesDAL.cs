/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：EngineServicesDAL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/19 9:39:58   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services.DAL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Configuration;
using System.Data;
using SMT.Workflow.Common.Utility;
using EngineDataModel;
using SMT.Workflow.Common.DataAccess;
using System.Data.OracleClient;
using System.Xml.Linq;
using System.IO;

namespace SMT.Workflow.Engine.Services.DAL
{
    public class EngineServicesDAL : BaseDAL
    {

        /// <summary>
        /// 新增待办
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dr1"></param>
        /// <param name="SourceValueDT"></param>
        /// <param name="strAPPFIELDVALUE"></param>
        public void AddDoTask(T_WF_DOTASK entity, DataRow dr1, DataTable SourceValueDT, string strAPPFIELDVALUE)
        {
            ClosedDoTaskStatus(entity.SYSTEMCODE, entity.ORDERID, null);
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
                        ModelMsgDefine(dr1["SYSTEMCODE"].ToString(), dr1["MODELCODE"].ToString(), entity.COMPANYID, ref strMsgBody, ref strMsgUrl);
                        if (string.IsNullOrEmpty(strMsgBody))
                        {
                            try
                            {
                                DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
                                if (drvList.Count() == 1)
                                {
                                    pageparm[6].Value = MsOracle.GetValue(drvList[0]["ColumnText"].ToString() + "已审批通过");//消息体                                    
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
                        pageparm[7].Value = MsOracle.GetValue(strUrl);//应用URL                         
                    }
                    else//在引擎配置界面定义了消息内容
                    {
                        pageparm[6].Value = MsOracle.GetValue(ReplaceMessageBody(dr1["MESSAGEBODY"].ToString(), SourceValueDT));//消息体
                        pageparm[7].Value = MsOracle.GetValue(string.Format(XmlTemplete, ReplaceValue(dr1["APPLICATIONURL"].ToString(), SourceValueDT)));//应用URL   

                    }
                    #endregion
                    pageparm[8].Value = MsOracle.GetValue(entity.RECEIVEUSERID);//接收用户ID
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
                    int result = MsOracle.ExecuteSQLByTransaction(insSql, pageparm);
                }


            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("命名空间：SMT.Workflow.Engine.Services.DAL类方法：AddDoTask（）" + ex.Message);
            }

        }
        public void AddDoTask(T_WF_DOTASK entity, DataRow dr1, DataTable SourceValueDT, string strReceiveID, string ApplicationCode, string strAPPFIELDVALUE, string strMsg, string strFormTypes)
        {
            ClosedDoTaskStatus(entity.SYSTEMCODE, entity.ORDERID, null);
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
                    ModelMsgDefine(dr1["SYSTEMCODE"].ToString(), dr1["MODELCODE"].ToString(), entity.COMPANYID, ref strMsgBody, ref strMsgUrl);
                    if (string.IsNullOrEmpty(strMsgBody))
                    {
                        try
                        {
                            DataRow[] drvList = SourceValueDT.Select("ColumnName='ModelName'");
                            if (drvList.Count() == 1)
                            {
                                pageparm[6].Value = MsOracle.GetValue(drvList[0]["ColumnValue"].ToString() + "已审批通过");//消息体                                    
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
                    }
                    else
                    {
                        pageparm[7].Value = MsOracle.GetValue(EncyptFormType(ReplaceValue(strMsgUrl, SourceValueDT), strFormTypes));//应用URL   
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
                int result = MsOracle.ExecuteSQLByTransaction(insSql, pageparm);// dao.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("新增待办消息出错:命名空间：SMT.Workflow.Engine.Services.DAL类方法:AddDoTask()" + ex);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 新增待办任务消息
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="drDefine"></param>
        /// <param name="SourceValueDT"></param>
        public void AddDoTaskMessage(T_WF_DOTASK entity, DataRow drDefine, DataTable SourceValueDT)
        {

            ClosedDoTaskStatus(entity.SYSTEMCODE, entity.ORDERID, null);
            try
            {
                string ReceiveUser = entity.RECEIVEUSERID;
                if ((string.IsNullOrEmpty(ReceiveUser) || ReceiveUser.ToUpper() == "END") && drDefine != null)
                {
                    entity.RECEIVEUSERID = drDefine["RECEIVEUSER"].ToString();
                }
                string sql = "INSERT INTO T_WF_DOTASKMESSAGE(DOTASKMESSAGEID,MESSAGEBODY,SYSTEMCODE,RECEIVEUSERID,ORDERID,COMPANYID,MESSAGESTATUS,MAILSTATUS,RTXSTATUS,REMARK) VALUES ('" + Guid.NewGuid().ToString() + "',";
                #region MESSAGEBODY
                if (drDefine != null)//在引擎定义了触发条件有消息定义
                {
                    string strMsgBody = drDefine["MESSAGEBODY"].ToString();
                    if (!string.IsNullOrEmpty(strMsgBody))
                    {
                        sql += "'" + ReplaceMessageBody(strMsgBody, SourceValueDT) + "'";
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
                sql += ",'" + entity.SYSTEMCODE + "','" + entity.RECEIVEUSERID + "','" + entity.ORDERID + "','" + entity.COMPANYID + "',0,0,0,'业务系统新增')";
                int result = dao.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("命名空间：SMT.Workflow.Engine.Services.DAL类方法：AddDoTaskMessage（）" + ex.Message);
            }
        }

        public void DoTaskCancel(string strSystemCode, string strModelCode, string strOrederID, string strReceiveID, string strContent)
        {
            try
            {
                //DOTASKSTATUS 代办任务状态(0、未处理 1、已处理 、2、任务撤销 )
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=2, CLOSEDDATE=to_date('" + DateTime.Now + "','YYYY-MM-DD hh24:mi:ss'),REMARK='任务撤销' WHERE DOTASKSTATUS=0 AND SYSTEMCODE='" + strSystemCode + "'  AND ORDERID='" + strOrederID + "'";
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                string addsql = @"INSERT INTO T_WF_DOTASKMESSAGE(DOTASKMESSAGEID,MESSAGEBODY,SYSTEMCODE,RECEIVEUSERID,ORDERID,COMPANYID,MESSAGESTATUS,MAILSTATUS,RTXSTATUS,REMARK) VALUES('" + Guid.NewGuid().ToString() + "',";
                addsql += "" + strContent + "','" + strSystemCode + "','" + strReceiveID + "','" + strOrederID + "',0,0,0,'任务撤销消息')";
                int insert = dao.ExecuteNonQuery(addsql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "DoTaskCancel()", "DoTaskCancel", ex);
            }

        }
        #region  待办任务分页查询
        /// <summary>
        /// 待办任务分页查询
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="strReceiveUser"></param>
        /// <param name="strStatus"></param>
        /// <param name="msgBody"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> MsgListByPaging(int pageIndex, int pageSize, string strReceiveUser, string strStatus, string msgBody, DateTime beginDate, DateTime endDate, ref int rowCount, ref int pageCount)
        {
            List<T_FLOW_ENGINEMSGLIST> DataList = new List<T_FLOW_ENGINEMSGLIST>();
            try
            {
                string strOrderBy = " BEFOREPROCESSDATE ";
                if (strStatus.ToUpper() == "CLOSE")
                {
                    strOrderBy = "CLOSEDDATE DESC";
                }
                int limitPageSize = 0;
                Int32.TryParse(ConfigurationManager.AppSettings["LimitPageSize"].ToString(), out limitPageSize);
                //如果当前pageSize大于最大记录数，则当前pageSize=设置最大记录数。
                pageSize = pageSize > limitPageSize ? limitPageSize : pageSize;

                #region where
                int status = 0;
                string where = "";
                if (strStatus.ToUpper() == "CLOSE")
                {
                    status = 1;
                    where = " RECEIVEUSERID='" + strReceiveUser + "' and DOTASKSTATUS=" + status + " ";
                }
                else if (strStatus.ToUpper() == "OPEN")
                {
                    status = 0;
                    where = " RECEIVEUSERID='" + strReceiveUser + "' and DOTASKSTATUS=" + status + " ";
                }
                else
                {
                    strOrderBy = " CREATEDATETIME DESC";
                    where = " RECEIVEUSERID='" + strReceiveUser + "' ";
                }
                if (!string.IsNullOrEmpty(msgBody))
                    where += " and MESSAGEBODY LIKE '%" + msgBody + "%'";
                if (strStatus.ToUpper() == "CLOSE")
                {
                    if (beginDate != DateTime.MinValue && endDate != DateTime.MinValue)
                    {
                        where += " and CLOSEDDATE>=to_date('" + beginDate + "','YYYY-MM-DD') and CLOSEDDATE<=to_date('" + endDate + "','YYYY-MM-DD')";
                    }
                    else if (beginDate != DateTime.MinValue)
                    {
                        where += " and CLOSEDDATE>=to_date('" + beginDate + "','YYYY-MM-DD')";
                    }
                    else if (endDate != DateTime.MinValue)
                    {
                        where += " and  CLOSEDDATE<=to_date('" + endDate + "','YYYY-MM-DD')";
                    }
                }
                if (strStatus.ToUpper() == "OPEN")
                {
                    if (beginDate != DateTime.MinValue && endDate != DateTime.MinValue)
                    {
                        where += " and CREATEDATETIME>=to_date('" + beginDate + "','YYYY-MM-DD') and CREATEDATETIME<=to_date('" + endDate + "','YYYY-MM-DD')";
                    }
                    else if (beginDate != DateTime.MinValue)
                    {
                        where += " and CREATEDATETIME>=to_date('" + beginDate + "','YYYY-MM-DD')";
                    }
                    else if (endDate != DateTime.MinValue)
                    {
                        where += " and  CREATEDATETIME<=to_date('" + endDate + "','YYYY-MM-DD')";
                    }
                }
                #endregion
                dao.Open();
                string strCountSql = "SELECT count(DOTASKID) FROM  T_WF_DOTASK where " + where;
                DataTable dt = dao.GetDataTable(strCountSql);
                //LogHelper.WriteLog("MsgListByPaging()" + strCountSql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    rowCount = int.Parse(dt.Rows[0][0].ToString());
                }
                if (rowCount == 0)//检查统计没有数据直接返回
                    return null;
                pageCount = rowCount / pageSize + (rowCount % pageSize > 0 ? 1 : 0);
                string fields = " ORDERID,APPLICATIONURL,MODELCODE,BEFOREPROCESSDATE,CREATEDATETIME,MESSAGEBODY,DOTASKID,CLOSEDDATE,RECEIVEUSERID,DOTASKSTATUS,SYSTEMCODE,MODELNAME ";
                int iRecStart = (pageIndex - 1) * pageSize + 1;
                //if (rowCount % pageSize == 0 && pageIndex > 1)
                //{
                //    iRecStart = (pageIndex - 2) * pageSize + 1;
                //}
                int iRecEnd = pageIndex * pageSize;
                where = string.IsNullOrEmpty(where) ? " 1=1 " : where;
                string strSql = "select " + fields + " " +
                        " from( " +
                        " select rownum num," + fields + " " +
                        " from( " +
                            "SELECT " + fields + " FROM T_WF_DOTASK " +
                            "where " + where + " order by  " + strOrderBy + ") temp1 " +
                            " where rownum<=" + iRecEnd +
                        ")temp2  " +
                        "where temp2.num>=" + iRecStart;
                DataTable table = dao.GetDataTable(strSql);
                //LogHelper.WriteLog("MsgListByPaging()" + table.Rows.Count + "||" + strSql);
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow dr in table.Rows)
                    {
                        //LogHelper.WriteLog("消息：" + dr["MESSAGEBODY"].ToString() + "||状态：" + dr["DOTASKSTATUS"].ToString() + "\n\r");
                        T_FLOW_ENGINEMSGLIST msgList = new T_FLOW_ENGINEMSGLIST();
                        msgList.MESSAGEID = string.Concat(dr["DOTASKID"]);
                        msgList.ORDERNODECODE = string.Concat(dr["ORDERID"]);
                        msgList.APPLICATIONURL = string.Concat(dr["APPLICATIONURL"]);
                        msgList.MESSAGESTATUS = string.Concat(dr["DOTASKSTATUS"]);
                        msgList.APPLICATIONCODE = string.Concat(dr["SYSTEMCODE"]);
                        msgList.BEFOREPROCESSDATE = TypeConverter.ToDateTime(dr["BEFOREPROCESSDATE"]);
                        msgList.CREATEDATE = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("yyyy/MM/dd");
                        msgList.CREATETIME = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("HH:mm");
                        msgList.MESSAGEBODY = string.Concat(dr["MESSAGEBODY"]);
                        msgList.OPERATIONDATE = dr["CLOSEDDATE"].ToString() != "" ? Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("yyyy/MM/dd") : "";
                        msgList.OPERATIONTIME = dr["CLOSEDDATE"].ToString() != "" ? Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("HH:mm") : "";
                        msgList.RECEIVEUSER = string.Concat(dr["RECEIVEUSERID"]);
                        msgList.MODELCODE = TypeConverter.ToString(dr["MODELCODE"]);
                        msgList.MODELNAME = TypeConverter.ToString(dr["MODELNAME"]);
                        DateTime OverTime = TypeConverter.ToDateTime(dr["BEFOREPROCESSDATE"]);
                        TimeSpan timeSpan = OverTime - DateTime.Now;
                        if (TypeConverter.ToInt(timeSpan.TotalHours) == 0)//小于一小时范围内
                        {
                            if (timeSpan.Minutes > 0)
                            {
                                msgList.TEMPFIELD = "+" + TypeConverter.ToString(timeSpan.Minutes) + "M";
                            }
                            else if (timeSpan.Minutes == 0)
                            {
                                msgList.TEMPFIELD = "+0M";
                            }
                            else
                            {
                                msgList.TEMPFIELD = TypeConverter.ToString(timeSpan.Minutes) + "M";
                            }
                        }
                        else
                        {
                            if (timeSpan.TotalHours > 0)
                            {
                                msgList.TEMPFIELD = "+" + TypeConverter.ToString(TypeConverter.ToInt(timeSpan.TotalHours)) + "H";
                            }
                            else if (timeSpan.TotalHours == 0)
                            {
                                msgList.TEMPFIELD = "+0H";
                            }
                            else
                            {
                                msgList.TEMPFIELD = TypeConverter.ToString(TypeConverter.ToInt(timeSpan.TotalHours)) + "H";
                            }
                        }
                        DataList.Add(msgList);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "MsgListByPaging()", "待办任务分页列表", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
            //LogHelper.WriteLog("平台待办分页接口 数量＝" + DataList.Count);
            return DataList;
        }
        #endregion

        #region 手机版待办任务分页查询
        /// <summary>
        ///  手机版待办任务分页查询
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="strReceiveUser"></param>
        /// <param name="strStatus"></param>
        /// <param name="msgBody"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> MsgListByPagingForMobile(int pageIndex, int pageSize, string strReceiveUser, string strStatus, string msgBody, DateTime beginDate, DateTime endDate, ref int rowCount, ref int pageCount)
        {
            List<T_FLOW_ENGINEMSGLIST> DataList = new List<T_FLOW_ENGINEMSGLIST>();
            try
            {
                string strOrderBy = "BEFOREPROCESSDATE";
                if (strStatus.ToUpper() == "CLOSE")
                {
                    strOrderBy = " CLOSEDDATE DESC";
                }
                int limitPageSize = 0;
                Int32.TryParse(ConfigurationManager.AppSettings["LimitPageSize"].ToString(), out limitPageSize);
                //如果当前pageSize大于最大记录数，则当前pageSize=设置最大记录数。
                pageSize = pageSize > limitPageSize ? limitPageSize : pageSize;

                #region where
                int status = 0;
                if (strStatus.ToUpper() == "CLOSE")
                {
                    status = 1;
                }
                else if (strStatus.ToUpper() == "OPEN")
                {
                    status = 0;
                }
                string where = " RECEIVEUSERID='" + strReceiveUser + "' and dotaskstatus=" + status + " ";
                if (!string.IsNullOrEmpty(msgBody))
                    where += " and MESSAGEBODY LIKE '%" + msgBody + "%'";
                if (strStatus.ToUpper() == "CLOSE")
                {
                    if (beginDate != DateTime.MinValue && endDate != DateTime.MinValue)
                    {
                        where += " and CLOSEDDATE>=to_date('" + beginDate + "','YYYY-MM-DD') and CLOSEDDATE<=to_date('" + endDate + "','YYYY-MM-DD')";
                    }
                    else if (beginDate != DateTime.MinValue)
                    {
                        where += " and CLOSEDDATE>=to_date('" + beginDate + "','YYYY-MM-DD')";
                    }
                    else if (endDate != DateTime.MinValue)
                    {
                        where += " and  CLOSEDDATE<=to_date('" + endDate + "','YYYY-MM-DD')";
                    }
                }
                if (strStatus.ToUpper() == "OPEN")
                {
                    if (beginDate != DateTime.MinValue && endDate != DateTime.MinValue)
                    {
                        where += " and CREATEDATETIME>=to_date('" + beginDate + "','YYYY-MM-DD') and CREATEDATETIME<=to_date('" + endDate + "','YYYY-MM-DD')";
                    }
                    else if (beginDate != DateTime.MinValue)
                    {
                        where += " and CREATEDATETIME>=to_date('" + beginDate + "','YYYY-MM-DD')";
                    }
                    else if (endDate != DateTime.MinValue)
                    {
                        where += " and  CREATEDATETIME<=to_date('" + endDate + "','YYYY-MM-DD')";
                    }
                }
                #endregion

                dao.Open();
                string strCountSql = "SELECT count(1) FROM  T_WF_DOTASK a RIGHT JOIN T_WF_MOBILEFILTER b on a.MODELCODE = b.MODELCODE   where " + where;

                DataTable tbCount = dao.GetDataTable(strCountSql);
                if (tbCount != null && tbCount.Rows.Count > 0)
                {
                    rowCount = TypeConverter.ToInt(tbCount.Rows[0][0]);
                }
                if (rowCount == 0)//检查统计没有数据直接返回
                    return null;
                pageCount = rowCount / pageSize + (rowCount % pageSize > 0 ? 1 : 0);
                string fields = "BEFOREPROCESSDATE,CREATEDATETIME,MESSAGEBODY,applicationurl,DOTASKID,CLOSEDDATE,RECEIVEUSERID,DOTASKSTATUS";

                int iRecStart = (pageIndex - 1) * pageSize + 1;
                int iRecEnd = pageIndex * pageSize;
                where = string.IsNullOrEmpty(where) ? " 1=1 " : where;
                string strSql = "select MODELCODE,MODELNAME,ORDERID," + fields + ",SYSTEMCODE " +
                        " from( " +
                        " select rownum num,MODELCODE,MODELNAME,ORDERID," + fields + ",SYSTEMCODE " +
                        " from( " +
                            "SELECT a.MODELCODE,a.MODELNAME,a.ORDERID," + fields + ",a.SYSTEMCODE,ROUND(TO_NUMBER(sysdate - BEFOREPROCESSDATE) * 24 * 60) as OverTime FROM T_WF_DOTASK a RIGHT JOIN T_WF_MOBILEFILTER b on a.MODELCODE = b.MODELCODE " +
                            "where " + where + " order by " + strOrderBy + " ) temp1 " +
                            " where rownum<=" + iRecEnd +
                        ")temp2  " +
                        "where temp2.num>=" + iRecStart;
                //LogHelper.WriteLog("手机版待办任务分页查询 SQL语句：\r\n" + strSql);
                DataTable table = dao.GetDataTable(strSql);
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow dr in table.Rows)
                    {
                        T_FLOW_ENGINEMSGLIST msgList = new T_FLOW_ENGINEMSGLIST();
                        msgList.MESSAGEID = string.Concat(dr["DOTASKID"]);
                        msgList.ORDERNODECODE = string.Concat(dr["ORDERID"]);
                        msgList.APPLICATIONURL = string.Concat(dr["applicationurl"]);
                        msgList.APPLICATIONCODE = string.Concat(dr["SYSTEMCODE"]);
                        msgList.BEFOREPROCESSDATE = TypeConverter.ToDateTime(dr["BEFOREPROCESSDATE"]);
                        msgList.CREATEDATE = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("yyyy/MM/dd");
                        msgList.CREATETIME = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("HH:mm");
                        msgList.MESSAGEBODY = string.Concat(dr["MESSAGEBODY"]);
                        msgList.OPERATIONDATE = dr["CLOSEDDATE"].ToString() != "" ? Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("yyyy/MM/dd") : "";
                        msgList.OPERATIONTIME = dr["CLOSEDDATE"].ToString() != "" ? Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("HH:mm") : "";
                        msgList.RECEIVEUSER = string.Concat(dr["RECEIVEUSERID"]);
                        msgList.MODELCODE = TypeConverter.ToString(dr["MODELCODE"]);
                        msgList.MODELNAME = TypeConverter.ToString(dr["MODELNAME"]);
                        DateTime OverTime = TypeConverter.ToDateTime(dr["BEFOREPROCESSDATE"]);
                        TimeSpan timeSpan = OverTime - DateTime.Now;
                        if (TypeConverter.ToInt(timeSpan.TotalHours) == 0)//小于一小时范围内
                        {
                            if (timeSpan.Minutes > 0)
                            {
                                msgList.TEMPFIELD = "+" + TypeConverter.ToString(timeSpan.Minutes) + "M";
                            }
                            else if (timeSpan.Minutes == 0)
                            {
                                msgList.TEMPFIELD = "+0M";
                            }
                            else
                            {
                                msgList.TEMPFIELD = TypeConverter.ToString(timeSpan.Minutes) + "M";
                            }
                        }
                        else
                        {
                            if (timeSpan.TotalHours > 0)
                            {
                                msgList.TEMPFIELD = "+" + TypeConverter.ToString(TypeConverter.ToInt(timeSpan.TotalHours)) + "H";
                            }
                            else if (timeSpan.TotalHours == 0)
                            {
                                msgList.TEMPFIELD = "+0H";
                            }
                            else
                            {
                                msgList.TEMPFIELD = TypeConverter.ToString(TypeConverter.ToInt(timeSpan.TotalHours)) + "H";
                            }
                        }
                        DataList.Add(msgList);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "MsgListByPagingForMobile()", "手机版待办任务分页查询", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
            return DataList;
        }
        #endregion

        public Dictionary<string, T_FLOW_ENGINEMSGLIST> GetPendingTaskPrevNext(string daskID, string status, string receiveUser)
        {

            Dictionary<string, T_FLOW_ENGINEMSGLIST> dict = new Dictionary<string, T_FLOW_ENGINEMSGLIST>();
            try
            {
                dao.Open();
                string strOrderBy = "CLOSEDDATE DESC";
                string where = " RECEIVEUSERID='" + receiveUser + "' and DOTASKSTATUS=1 ";
                string fields = "a.SYSTEMCODE,a.BEFOREPROCESSDATE,a.CREATEDATETIME,a.MESSAGEBODY,a.DOTASKID,a.DOTASKSTATUS,a.CLOSEDDATE,a.RECEIVEUSERID,a.MODELNAME ";
                string strSql = "select rownum,T.* from (select a.MODELCODE,a.ORDERID,a.applicationurl," + fields + " FROM T_WF_DOTASK a " +
                                "where " + where + " order by " + strOrderBy + ") T ";
                DataTable dt = new DataTable();
                switch ((status + "").ToUpper())
                {
                    case "CLOSE":
                        dt = dao.GetDataTable(strSql);
                        break;
                    case "OPEN":
                        strOrderBy = " BEFOREPROCESSDATE ";
                        where = " RECEIVEUSERID='" + receiveUser + "' and DOTASKSTATUS=0  ";
                        strSql = "SELECT rownum,T.* from (SELECT a.MODELCODE,a.ORDERID,a.applicationurl," + fields + " FROM T_WF_DOTASK a " +
                                       "where " + where + " order by " + strOrderBy + ") T";
                        dt = dao.GetDataTable(strSql);
                        break;
                    default:
                        strOrderBy = " CREATEDATETIME DESC ";
                        where = " RECEIVEUSERID='" + receiveUser + "' ";
                        strSql = "SELECT rownum,T.* from (SELECT a.MODELCODE,a.ORDERID,a.applicationurl," + fields + " FROM T_WF_DOTASK a " +
                                       "where " + where + " order by " + strOrderBy + ") T";
                        dt = dao.GetDataTable(strSql);
                        break;
                }
                if (dt.Rows.Count > 0)
                {
                    DataRow[] dr = dt.Select("DOTASKID='" + daskID + "'");
                    if (dr.Length > 0)
                    {
                        int currentRowNum = Convert.ToInt32(dr[0]["rownum"]);
                        int prev = currentRowNum - 1;
                        int next = currentRowNum + 1;

                        DataRow[] drPrev = dt.Select("rownum='" + prev + "'");
                        if (drPrev.Length > 0)
                        {
                            T_FLOW_ENGINEMSGLIST PrevModel = GeneralEngineMsgModel(drPrev[0]);
                            dict.Add("Prev", PrevModel);
                        }

                        DataRow[] drNext = dt.Select("rownum='" + next + "'");
                        if (drNext.Length > 0)
                        {
                            T_FLOW_ENGINEMSGLIST NextModel = GeneralEngineMsgModel(drNext[0]);
                            dict.Add("Next", NextModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetPendingTaskPrevNext()", "  ", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
            return dict;
        }

        private T_FLOW_ENGINEMSGLIST GeneralEngineMsgModel(DataRow dr)
        {
            T_FLOW_ENGINEMSGLIST msgList = new T_FLOW_ENGINEMSGLIST();
            msgList.MESSAGEID = string.Concat(dr["DOTASKID"]);
            msgList.APPLICATIONCODE = string.Concat(dr["SYSTEMCODE"]);
            msgList.ORDERNODECODE = string.Concat(dr["ORDERID"]);
            msgList.MESSAGESTATUS = string.Concat(dr["DOTASKSTATUS"]);
            msgList.APPLICATIONURL = string.Concat(dr["applicationurl"]);
            msgList.BEFOREPROCESSDATE = TypeConverter.ToDateTime(dr["BEFOREPROCESSDATE"]);
            msgList.CREATEDATE = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("yyyy/MM/dd");
            msgList.CREATETIME = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("HH:mm");
            msgList.MESSAGEBODY = string.Concat(dr["MESSAGEBODY"]);
            msgList.OPERATIONDATE = dr["CLOSEDDATE"].ToString() != "" ? Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("yyyy/MM/dd") : "";
            msgList.OPERATIONTIME = dr["CLOSEDDATE"].ToString() != "" ? Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("HH:mm") : "";
            msgList.RECEIVEUSER = string.Concat(dr["RECEIVEUSERID"]);
            msgList.MODELCODE = TypeConverter.ToString(dr["MODELCODE"]);
            msgList.MODELNAME = TypeConverter.ToString(dr["MODELNAME"]);
            DateTime OverTime = TypeConverter.ToDateTime(dr["BEFOREPROCESSDATE"]);

            TimeSpan timeSpan = OverTime - DateTime.Now;
            if (TypeConverter.ToInt(timeSpan.TotalHours) == 0)//小于一小时范围内
            {
                if (timeSpan.Minutes > 0)
                {
                    msgList.TEMPFIELD = "+" + TypeConverter.ToString(timeSpan.Minutes) + "M";
                }
                else if (timeSpan.Minutes == 0)
                {
                    msgList.TEMPFIELD = "+0M";
                }
                else
                {
                    msgList.TEMPFIELD = TypeConverter.ToString(timeSpan.Minutes) + "M";
                }
            }
            else
            {
                if (timeSpan.TotalHours > 0)
                {
                    msgList.TEMPFIELD = "+" + TypeConverter.ToString(TypeConverter.ToInt(timeSpan.TotalHours)) + "H";
                }
                else if (timeSpan.TotalHours == 0)
                {
                    msgList.TEMPFIELD = "+0H";
                }
                else
                {
                    msgList.TEMPFIELD = TypeConverter.ToString(TypeConverter.ToInt(timeSpan.TotalHours)) + "H";
                }
            }
            return msgList;
        }

        /// <summary>
        /// 获取引擎待办任务（主数据）
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="strStatus"></param>
        /// <param name="iTop"></param>
        /// <param name="LastDay"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> GetEngineMainMsgList(string strUserID, string strStatus, string message, int iTop, int LastDay)
        {
            List<T_FLOW_ENGINEMSGLIST> DataList = new List<T_FLOW_ENGINEMSGLIST>();
            if (iTop <= 0)
            {
                return DataList;
            }
            try
            {
                dao.Open();
                string strFields = " APPLICATIONURL,MODELCODE,BEFOREPROCESSDATE,ORDERID,CREATEDATETIME,MESSAGEBODY,DOTASKID,CLOSEDDATE,RECEIVEUSERID,DOTASKSTATUS,SYSTEMCODE,MODELNAME ";
                string strLastDay = string.Empty;
                string sqlWhere = "";
                if (!string.IsNullOrWhiteSpace(message))
                {
                    sqlWhere = " and MESSAGEBODY like'%" + message + "%'";
                }
                string strsql = "";
                if (strStatus.ToUpper() == "OPEN")
                {
                    if (LastDay > 0)
                    {
                        strLastDay = " and CREATEDATETIME>to_date('" + DateTime.Now.AddDays(-LastDay).ToString("yyyy/MM/dd") + "','YYYY-MM-DD')";
                    }
                    strsql = "SELECT " + strFields + " FROM (SELECT " + strFields + " FROM  T_WF_DOTASK where RECEIVEUSERID='" + strUserID + "' and DOTASKSTATUS=0   " + sqlWhere + strLastDay + "  order by BEFOREPROCESSDATE) where rownum <=" + iTop;
                }
                else if (strStatus.ToUpper() == "CLOSE")
                {
                    if (LastDay > 0)
                    {
                        strLastDay = " and CLOSEDDATE>to_date('" + DateTime.Now.AddDays(-LastDay).ToString("yyyy/MM/dd") + "','YYYY-MM-DD')";
                    }
                    strsql = "SELECT " + strFields + " FROM (SELECT " + strFields + " FROM  T_WF_DOTASK where RECEIVEUSERID='" + strUserID + "' and DOTASKSTATUS=1 " + sqlWhere + strLastDay + "   order by CLOSEDDATE desc) where rownum <=" + iTop;

                }
                DataTable table = dao.GetDataTable(strsql);
                int tt = 0;
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow dr in table.Rows)
                    {
                        tt++;
                        T_FLOW_ENGINEMSGLIST msgList = new T_FLOW_ENGINEMSGLIST();
                        msgList.MESSAGEID = string.Concat(dr["DOTASKID"]);
                        msgList.APPLICATIONURL = string.Concat(dr["APPLICATIONURL"]);
                        msgList.MESSAGESTATUS = dr["DOTASKSTATUS"].ToString();
                        msgList.ORDERNODECODE = string.Concat(dr["SYSTEMCODE"]) + "_" + string.Concat(dr["ORDERID"]);
                        msgList.APPLICATIONCODE = string.Concat(dr["SYSTEMCODE"]);
                        msgList.BEFOREPROCESSDATE = TypeConverter.ToDateTime(dr["BEFOREPROCESSDATE"]);
                        msgList.CREATEDATE = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("yyyy/MM/dd");
                        msgList.CREATETIME = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("HH:mm");
                        msgList.MESSAGEBODY = string.Concat(dr["MESSAGEBODY"]);
                        msgList.OPERATIONDATE = dr["CLOSEDDATE"].ToString() != "" ? Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("yyyy/MM/dd") : "";
                        msgList.OPERATIONTIME = dr["CLOSEDDATE"].ToString() != "" ? Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("HH:mm") : "";
                        msgList.RECEIVEUSER = string.Concat(dr["RECEIVEUSERID"]);
                        msgList.MODELCODE = TypeConverter.ToString(dr["MODELCODE"]);
                        msgList.MODELNAME = TypeConverter.ToString(dr["MODELNAME"]);
                        DateTime OverTime = TypeConverter.ToDateTime(dr["BEFOREPROCESSDATE"]);

                        TimeSpan timeSpan = OverTime - DateTime.Now;
                        if (TypeConverter.ToInt(timeSpan.TotalHours) == 0)//小于一小时范围内
                        {
                            if (timeSpan.Minutes > 0)
                            {
                                msgList.TEMPFIELD = "+" + TypeConverter.ToString(timeSpan.Minutes) + "M";
                            }
                            else if (timeSpan.Minutes == 0)
                            {
                                msgList.TEMPFIELD = "+0M";
                            }
                            else
                            {
                                msgList.TEMPFIELD = TypeConverter.ToString(timeSpan.Minutes) + "M";
                            }
                        }
                        else
                        {
                            if (timeSpan.TotalHours > 0)
                            {
                                msgList.TEMPFIELD = "+" + TypeConverter.ToString(TypeConverter.ToInt(timeSpan.TotalHours)) + "H";
                            }
                            else if (timeSpan.TotalHours == 0)
                            {
                                msgList.TEMPFIELD = "+0H";
                            }
                            else
                            {
                                msgList.TEMPFIELD = TypeConverter.ToString(TypeConverter.ToInt(timeSpan.TotalHours)) + "H";
                            }
                        }
                        DataList.Add(msgList);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetEngineMainMsgList()", "获取引擎待办任务（主数据）", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
            return DataList;
        }

        /// <summary>
        /// 获取引擎细节任务（细节数据）
        /// </summary>
        /// <param name="strMsgID"></param>
        /// <returns></returns>
        public T_FLOW_ENGINEMSGLIST GetEngineMsgDetail(string dotaskid, bool isPhone)
        {
            try
            {
                string sql = "SELECT * FROM T_WF_DOTASK where dotaskid='" + dotaskid + "' ";
                dao.Open();
                DataTable dt = dao.GetDataTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    T_FLOW_ENGINEMSGLIST msg = new T_FLOW_ENGINEMSGLIST();
                    msg.APPLICATIONURL = string.Concat(dt.Rows[0]["APPLICATIONURL"]);
                    msg.APPFIELDVALUE = string.Concat(dt.Rows[0]["APPFIELDVALUE"]);
                    msg.MESSAGESTATUS = string.Concat(dt.Rows[0]["DOTASKSTATUS"]) == "1" ? "close" : "open";//供手机使用
                    //msg.SYSTEMNAME = string.Concat(dt.Rows[0]["SYSTEMNAME"]);
                    msg.MODELNAME = string.Concat(dt.Rows[0]["MODELNAME"]);

                    if (isPhone)//手机需要的字段
                    {
                        msg.FLOWXML = TypeConverter.ToString(dt.Rows[0]["FLOWXML"]);
                        msg.APPXML = TypeConverter.ToString(dt.Rows[0]["APPXML"]);
                        msg.ORDERNODECODE = TypeConverter.ToString(dt.Rows[0]["SYSTEMCODE"]) + "_" + TypeConverter.ToString(dt.Rows[0]["ORDERID"]);
                    }
                    return msg;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetEngineMsgDetail()", "获取引擎细节任务（细节数据）", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
        }

        public T_WF_DOTASK GetDoTaskEntity(string orderID, string receiveUserID)
        {
            try
            {
                dao.Open();
                string strFields = " DOTASKID,ORDERID,MESSAGEBODY,APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,CLOSEDDATE,DOTASKSTATUS,SYSTEMCODE,MODELCODE,CREATEDATETIME ";
                string sql = "SELECT  " + strFields + "  FROM T_WF_DOTASK where ORDERID='" + orderID + "' and RECEIVEUSERID='" + receiveUserID + "' order by CREATEDATETIME desc ";
                DataTable dt = dao.GetDataTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    T_WF_DOTASK task = new T_WF_DOTASK();
                    DateTime? now = null;
                    task.DOTASKID = string.Concat(dt.Rows[0]["DOTASKID"]);
                    task.ORDERID = string.Concat(dt.Rows[0]["ORDERID"]);
                    task.MESSAGEBODY = string.Concat(dt.Rows[0]["MESSAGEBODY"]);
                    task.APPLICATIONURL = string.Concat(dt.Rows[0]["APPLICATIONURL"]);
                    task.RECEIVEUSERID = string.Concat(dt.Rows[0]["RECEIVEUSERID"]);
                    task.BEFOREPROCESSDATE = dt.Rows[0]["BEFOREPROCESSDATE"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["BEFOREPROCESSDATE"].ToString()) : now;
                    task.CLOSEDDATE = dt.Rows[0]["CLOSEDDATE"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["CLOSEDDATE"].ToString()) : now;
                    task.DOTASKSTATUS = int.Parse(dt.Rows[0]["DOTASKSTATUS"].ToString());
                    task.SYSTEMCODE = string.Concat(dt.Rows[0]["SYSTEMCODE"]);
                    task.MODELCODE = string.Concat(dt.Rows[0]["MODELCODE"]);
                    task.CREATEDATETIME = Convert.ToDateTime(dt.Rows[0]["CREATEDATETIME"].ToString());
                    return task;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }

        }

        public T_FLOW_ENGINEMSGLIST PendingDetailTasksByPhone(string ordernodecode)
        {
            try
            {
                string sql = "SELECT * FROM T_WF_DOTASK where orderid='" + ordernodecode + "'ORDER BY CREATEDATETIME DESC ";
                dao.Open();
                //LogHelper.WriteLog("获取引擎细节任务（细节数据）SQL语句：\r\n" + sql);
                DataTable dt = dao.GetDataTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    T_FLOW_ENGINEMSGLIST msg = new T_FLOW_ENGINEMSGLIST();
                    msg.APPLICATIONURL = string.Concat(dt.Rows[0]["APPLICATIONURL"]);
                    msg.APPFIELDVALUE = string.Concat(dt.Rows[0]["APPFIELDVALUE"]);
                    msg.MESSAGESTATUS = string.Concat(dt.Rows[0]["DOTASKSTATUS"]) == "1" ? "close" : "open";//供手机使用
                    msg.SYSTEMNAME = string.Concat(dt.Rows[0]["SYSTEMNAME"]);
                    msg.MODELNAME = string.Concat(dt.Rows[0]["MODELNAME"]);
                    msg.FLOWXML = TypeConverter.ToString(dt.Rows[0]["FLOWXML"]);
                    msg.APPXML = TypeConverter.ToString(dt.Rows[0]["APPXML"]);
                    msg.ORDERNODECODE = TypeConverter.ToString(dt.Rows[0]["SYSTEMCODE"]) + "_" + TypeConverter.ToString(dt.Rows[0]["ORDERID"]);
                    return msg;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "PendingDetailTasksByPhone()", "获取引擎细节任务（细节数据）", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 消息（无链接）
        /// </summary>
        /// <param name="strUserID"></param>
        /// <param name="strStatus"></param>
        /// <param name="iTop"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINENOTES> GetMsgNodes(string strUserID, string strStatus, int iTop)
        {

            List<T_FLOW_ENGINENOTES> DataList = new List<T_FLOW_ENGINENOTES>();
            if (iTop <= 0)
            {
                return DataList;
            }
            try
            {
                string sql = "SELECT * FROM (SELECT * FROM  T_WF_DOTASKMESSAGE where RECEIVEUSERID='" + strUserID + "' and MESSAGESTATUS='" + strStatus + "'  order by CREATEDATETIME desc) where rownum <=" + iTop;
                dao.Open();
                //LogHelper.WriteLog("消息（无链接）SQL语句：\r\n" + sql);
                DataTable table = dao.GetDataTable(sql);
                if (table != null && table.Rows.Count > 0)
                {
                    foreach (DataRow dr in table.Rows)
                    {
                        T_FLOW_ENGINENOTES ENGINENOTES = new T_FLOW_ENGINENOTES();
                        ENGINENOTES.MESSAGEBODY = string.Concat(dr["MESSAGEBODY"]);
                        ENGINENOTES.APPLICATIONCODE = string.Concat(dr["SYSTEMCODE"]);
                        ENGINENOTES.RECEIVEUSER = string.Concat(dr["RECEIVEUSERID"]);
                        ENGINENOTES.RECEIVEDATE = Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("yyyy/MM/dd");
                        ENGINENOTES.RECEIVETIME = Convert.ToDateTime(dr["CLOSEDDATE"].ToString()).ToString("HH:mm");
                        ENGINENOTES.MESSAGESTATUS = string.Concat(dr["MESSAGESTATUS"]);
                        ENGINENOTES.CREATEDATE = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("yyyy/MM/dd");
                        ENGINENOTES.CREATETIME = Convert.ToDateTime(dr["CREATEDATETIME"].ToString()).ToString("HH:mm");
                        DataList.Add(ENGINENOTES);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetMsgNodes()", "消息（无链接）", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
            return DataList;
        }


        /// <summary>
        /// 删除待办任务
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>
        /// <param name="strReceiveID"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public bool DeleteDoTaskStatus(string strSystemCode, string strFormID, string strReceiveID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=10, CLOSEDDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi'),REMARK='删除待办' WHERE SYSTEMCODE='" + strSystemCode + "' AND ORDERID='" + strFormID + "'";
                if (!string.IsNullOrEmpty(strReceiveID))
                {
                    sql += "  AND RECEIVEUSERID='" + strReceiveID + "'";
                }
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "DeleteDoTaskStatus()", "删除待办任务", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 关闭待办任务
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>
        /// <param name="strReceiveID"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public bool ClosedDoTaskStatus(string strSystemCode, string strFormID, string strReceiveID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi'),REMARK='关闭待办' WHERE DOTASKSTATUS=0 AND  SYSTEMCODE='" + strSystemCode + "' AND ORDERID='" + strFormID + "'";
                if (!string.IsNullOrEmpty(strReceiveID))
                {
                    sql += "  AND RECEIVEUSERID='" + strReceiveID + "'";
                }
                dao.Open();
                //LogHelper.WriteLog("ClosedDoTaskStatus()" + sql);
                int result = dao.ExecuteNonQuery(sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "ClosedDoTaskStatus()", "关闭待办任务", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 关闭待办
        /// </summary>
        /// <param name="TaskID"></param>
        /// <returns></returns>
        public bool ClosedDoTaskStatus(string TaskID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi'),REMARK='关闭待办' WHERE DOTASKID='" + TaskID + "'";
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "ClosedDoTaskStatus()", "关闭待办任务", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 关闭代办新方法
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="strModelCode"></param>
        /// <param name="strReceiveID"></param>
        /// <returns></returns>
        public bool ClosedDoTaskOrderID(string orderID, string strModelCode, string strReceiveID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi'),REMARK='关闭待办' WHERE DOTASKSTATUS=0 ";
                if (!string.IsNullOrWhiteSpace(orderID))
                {
                    sql += " AND ORDERID='" + orderID + "'";
                }
                if (!string.IsNullOrWhiteSpace(strModelCode))
                {
                    sql += " AND MODELCODE='" + strModelCode + "'";
                }
                if (!string.IsNullOrWhiteSpace(strReceiveID))
                {
                    sql += " AND RECEIVEUSERID='" + strReceiveID + "'";
                }
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 关闭待办
        /// </summary>
        /// <param name="TaskID"></param>
        /// <returns></returns>
        public bool ClosedDoTaskStatus(string strModelCode, string strReceiveID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi'),REMARK='关闭待办' WHERE DOTASKSTATUS=0 AND  MODELCODE='" + strModelCode + "' AND RECEIVEUSERID='" + strReceiveID + "'";
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "ClosedDoTaskStatus()", "关闭待办任务", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 查询代办接收人
        /// </summary>
        /// <param name="strModelCode"></param>
        /// <param name="fromID"></param>
        /// <returns></returns>
        public DataTable SelectTaskReceiveID(string strModelCode, string fromID)
        {
            try
            {
                string sql = "SELECT DOTASKID,RECEIVEUSERID FROM T_WF_DOTASK WHERE DOTASKSTATUS=0 AND MODELCODE='" + strModelCode + "'AND ORDERID='" + fromID + "'";
                dao.Open();
                DataTable dt = dao.GetDataTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "SelectTaskReceiveID()", "根据单据ID查询接收人", ex);
                return null;
            }
        }
        /// <summary>
        /// 自定义发起流程消息
        /// </summary>
        /// <param name="SourceValueDT"></param>
        /// <param name="ApplicationCode"></param>
        /// <param name="define"></param>
        public void SendTriggerMsg(DataTable SourceValueDT, string ApplicationCode, T_FLOW_CUSTOMFLOWDEFINE define, string strUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(strUser))
                {
                    string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + ",REMARK='关闭待办(自定义发起流程)' WHERE SYSTEMCODE='" + define.SYSTEMCODE + "' AND DOTASKSTATUS=0 AND ORDERID='" + ApplicationCode + "'";
                    dao.Open();
                    int result = dao.ExecuteNonQuery(sql);
                    string addsql = @"INSERT INTO T_WF_DOTASK(DOTASKID,COMPANYID,ORDERID,MESSAGEBODY,APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,DOTASKTYPE,
                                     ENGINECODE,DOTASKSTATUS,MAILSTATUS,RTXSTATUS,APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELCODE,MODELNAME,REMARK) ) VALUES ('" + Guid.NewGuid().ToString() + "',";
                    addsql += "'','" + ApplicationCode + "',";
                    string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                                    "<System>" + "\r\n" +
                                    "{0}" +
                                    "</System>";
                    if (string.IsNullOrEmpty(define.MESSAGEBODY))
                    {
                        addsql += "'" + ApplicationCode + "'已审批通过,";
                        addsql += "'" + string.Format(XmlTemplete, porcessValue(define.MSGLINKURL, SourceValueDT)) + "',";
                    }
                    else//在引擎配置界面定义了消息内容
                    {
                        addsql += "'" + ReplaceMessageBody(define.MESSAGEBODY, SourceValueDT) + "'";
                        addsql += "'" + string.Format(XmlTemplete, porcessValue(define.MSGLINKURL, SourceValueDT)) + "',";
                    }
                    addsql += "'','" + strUser + "',to_date('" + DateTime.Now.AddDays(3).ToString("yyyy/MM/dd") + "','YYYY-MM-DD'),0,'',0,0,0,";
                    addsql += "'','','','" + define.SYSTEMCODE + "','" + define.MODELCODE + "','" + define.MODELNAME + "','自发流程')";
                    LogHelper.WriteLog("SendTriggerMsg()SQL语句：\r\n" + addsql);
                    result = dao.ExecuteNonQuery(addsql);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "SendTriggerMsg()", "自定义发起流程消息", ex);
            }
            finally
            {
                dao.Close();
            }
        }

        private string ReplaceMessageBody(string strMsgBody, DataTable SourceValueDT)
        {
            strMsgBody = strMsgBody.Replace("{new:", "").Replace("}", "");
            foreach (DataRow dr in SourceValueDT.Rows)
            {
                strMsgBody = strMsgBody.Replace(dr["ColumnName"].ToString(), dr["ColumnValue"].ToString());
            }
            return strMsgBody;
        }

        /// <summary>
        /// 元数据替换 DataValue 适用于替换 Guid
        /// </summary>
        /// <param name="PorcessString"></param>
        /// <param name="SourceValueDT"></param>
        /// <returns></returns>
        private string ReplaceValue(string PorcessString, DataTable SourceValueDT)
        {
            foreach (DataRow dr in SourceValueDT.Rows)
            {
                PorcessString = PorcessString.Replace("{" + dr["ColumnName"].ToString() + "}", dr["ColumnValue"].ToString());
            }
            return PorcessString;
        }

        private string EncyptFormType(string Url, string strFormTypes)
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
        /// 将WCF参数，根据数据源转换成具体值
        /// </summary>
        /// <param name="PorcessString"></param>
        /// <param name="SourceValueDT"></param>
        /// <returns></returns>
        private string porcessValue(string PorcessString, DataTable SourceValueDT)
        {
            foreach (DataRow dr in SourceValueDT.Rows)
            {
                PorcessString = PorcessString.Replace("{" + dr["ColumnName"].ToString() + "}", dr["ColumnValue"].ToString());
            }
            return PorcessString;
        }


        /// <summary>
        /// 获取默认消息
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strModelCode"></param>
        /// <param name="strCompanyID"></param>
        /// <param name="strMessageType"></param>
        /// <returns></returns>
        public List<T_WF_MESSAGEBODYDEFINE> GetMessageBodyDefine(string strSystemCode, string strModelCode, string strCompanyID, int messageType)
        {
            List<T_WF_MESSAGEBODYDEFINE> list = new List<T_WF_MESSAGEBODYDEFINE>();
            try
            {
                string sql = "SELECT * FROM T_WF_MESSAGEBODYDEFINE WHERE SYSTEMCODE='" + strSystemCode + "'AND MODELCODE='" + strModelCode + "'";
                if (!string.IsNullOrEmpty(strCompanyID))
                {
                    sql += " AND COMPANYID='" + strCompanyID + "'";
                }
                if (messageType >= 0)
                {
                    sql += " AND MESSAGETYPE=" + messageType + "";
                }
                dao.Open();
                DataTable dt = dao.GetDataTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        T_WF_MESSAGEBODYDEFINE MsgDefine = new T_WF_MESSAGEBODYDEFINE();
                        MsgDefine.SYSTEMCODE = string.Concat(dr["SYSTEMCODE"]);
                        MsgDefine.MODELCODE = string.Concat(dr["MODELCODE"]);
                        MsgDefine.MESSAGEBODY = string.Concat(dr["MESSAGEBODY"]);
                        MsgDefine.MESSAGEURL = string.Concat(dr["MESSAGEURL"]);
                        MsgDefine.RECEIVEPOSTID = string.Concat(dr["RECEIVEPOSTID"]);
                        MsgDefine.RECEIVERUSERID = string.Concat(dr["RECEIVERUSERID"]);
                        MsgDefine.RECEIVETYPE = int.Parse(dr["RECEIVETYPE"].ToString());
                        list.Add(MsgDefine);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetMessageBodyDefine()", "获取默认消息", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="strReceiveUser"></param>
        /// <param name="SystemCode"></param>
        /// <param name="strModelCode"></param>
        /// <param name="FormID"></param>
        /// <param name="Content"></param>
        /// <param name="Url"></param>
        /// <param name="APPFIELDVALUE"></param>
        /// <param name="strAppXML"></param>
        public void SendTriggerTaskMsg(string strReceiveID, string SystemCode, string strModelCode, string FormID, string Content, string Url, string APPFIELDVALUE, string strAppXML)
        {
            try
            {
                using (OracleConnection con = new OracleConnection(dao.DatabaseString))
                {
                    string insSql = @"INSERT INTO T_WF_DOTASK (DOTASKID,COMPANYID,ORDERID,MESSAGEBODY,APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,DOTASKTYPE,
                                     DOTASKSTATUS,MAILSTATUS,RTXSTATUS,APPFIELDVALUE,FLOWXML,APPXML,SYSTEMCODE,MODELCODE,REMARK) VALUES (:DOTASKID,:COMPANYID,:ORDERID,:MESSAGEBODY,:APPLICATIONURL,
                                    :RECEIVEUSERID,:BEFOREPROCESSDATE,:DOTASKTYPE,:DOTASKSTATUS,:MAILSTATUS,:RTXSTATUS,:APPFIELDVALUE,:FLOWXML,:APPXML,:SYSTEMCODE,:MODELCODE,:REMARK)";
                    OracleParameter[] pageparm =
                    { 
                         new OracleParameter(":DOTASKID",OracleType.NVarChar,100), 
                        new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                        new OracleParameter(":ORDERID",OracleType.NVarChar,100),                   
                        new OracleParameter(":MESSAGEBODY",OracleType.NVarChar,4000), 
                        new OracleParameter(":APPLICATIONURL",OracleType.NVarChar,4000), 
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
                        new OracleParameter(":REMARK",OracleType.NVarChar,400) 

                    };
                    pageparm[0].Value = GetValue(Guid.NewGuid().ToString());//待办任务ID
                    pageparm[1].Value = GetValue(GetValue(null));//公司ID
                    pageparm[2].Value = GetValue(FormID);//单据ID             
                    pageparm[3].Value = GetValue(Content);//消息体
                    pageparm[4].Value = GetValue(Url);//应用URL
                    pageparm[5].Value = GetValue(strReceiveID);//接收用户ID
                    pageparm[6].Value = GetValue(DateTime.Now.AddDays(3));//可处理时间（主要针对KPI考核）
                    pageparm[7].Value = GetValue(0);//待办任务类型(0、待办任务、1、流程咨询、3 )
                    pageparm[8].Value = GetValue(0);//代办任务状态(0、未处理 1、已处理 、2、任务撤销 10、删除)
                    pageparm[9].Value = GetValue(0);//邮件状态(0、未发送 1、已发送、2、未知 )
                    pageparm[10].Value = GetValue(0);//RTX状态(0、未发送 1、已发送、2、未知 )            
                    pageparm[11].Value = GetValue(APPFIELDVALUE);//应用字段值
                    pageparm[12].Value = GetValue(null);//流程XML
                    pageparm[13].Value = GetValue(strAppXML);//应用XML
                    pageparm[14].Value = GetValue(SystemCode);//系统代码
                    pageparm[15].Value = GetValue(strModelCode);//模块代码
                    pageparm[16].Value = GetValue("直发代办");//备注
                    int insert = MsOracle.ExecuteNonQuery(con, CommandType.Text, insSql, pageparm);// dao.ExecuteNonQuery(addsql);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }


        }

        public void SendTriggerMsg(string strReceiveUser, string SystemCode, string FormID, string Content)
        {
            try
            {
                string addsql = @"INSERT INTO T_WF_DOTASKMESSAGE(DOTASKMESSAGEID,MESSAGEBODY,SYSTEMCODE,RECEIVEUSERID,ORDERID,COMPANYID,MESSAGESTATUS,MAILSTATUS,
                                     RTXSTATUS,REMARK) VALUES ('" + Guid.NewGuid().ToString() + "',";
                addsql += "'" + Content + "','" + SystemCode + "','" + strReceiveUser + "','" + FormID + "','',";
                addsql += "0,0,0,'')";
                dao.Open();
                int result = dao.ExecuteNonQuery(addsql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "SendTriggerMsg()", "", ex);
            }
            finally
            {
                dao.Close();
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
        public DataTable FlowTriggerTable(string strSystemCode, string strModelCode, string status, string strCompanyID)
        {
            try
            {
                string sql = @"SELECT B.APPLICATIONURL,B.SYSTEMCODE,B.MODELCODE,B.MESSAGEBODY,B.ISDEFAULTMSG,
                B.RECEIVEUSER,B.LASTDAYS,B.WCFBINDINGCONTRACT,B.WCFURL,B.FUNCTIONNAME,B.FUNCTIONPARAMTER,B.PAMETERSPLITCHAR,B.OWNERDEPARTMENTID, 
                B.OWNERCOMPANYID,  
                B.OWNERPOSTID,  A.DOTASKRULEID  FROM T_WF_DOTASKRULE A LEFT JOIN   T_WF_DOTASKRULEDETAIL B ON A.DOTASKRULEID=B.DOTASKRULEID";
                sql += "  WHERE A.SYSTEMCODE='" + strSystemCode + "' AND A.MODELCODE='" + strModelCode + "'";
                if (!string.IsNullOrEmpty(status))
                {
                    sql += " AND A.COMPANYID='" + strCompanyID + "'";
                }
                if (!string.IsNullOrEmpty(status))
                {
                    sql += " AND A.TRIGGERORDERSTATUS=" + status + "";
                }
                dao.Open();
                return dao.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "FlowTriggerTable()", "流程数据是否匹配引擎配置界面所定义的条件", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
        }


        /// <summary>
        /// 删除定时触发
        /// </summary>
        /// <param name="businessID"></param>
        public bool DeleteTimingTrigger(string businessID)
        {
            try
            {
                string sql = "DELETE  T_WF_TIMINGTRIGGERACTIVITY WHERE  BUSINESSID='" + businessID + "'";
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "DeleteTimingTrigger()", "删除定时触发", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 获取默认消息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DataTable GetMessageDefine(string key)
        {
            try
            {
                string sql = "SELECT MESSAGEBODY FROM T_WF_MESSAGEDEFINE WHERE MESSAGEKEY='" + key + "'";
                dao.Open();
                return dao.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetMessageDefine()", "获取默认消息", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 获取默认消息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMessageDefineString(string key)
        {
            try
            {
                string sql = "SELECT MESSAGEBODY FROM T_WF_MESSAGEDEFINE WHERE MESSAGEKEY='" + key + "'";
                dao.Open();
                return dao.ExecuteScalar(sql).ToString();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetMessageDefine()", "获取默认消息", ex);
                return "";
            }
        }



        /// <summary>
        /// 新增定时触发数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool AddTimingTrigger(T_WF_TIMINGTRIGGERACTIVITY entity)
        {
            try
            {

                using (OracleConnection con = new OracleConnection(dao.DatabaseString))
                {
                    string insSql = @"INSERT INTO T_WF_TIMINGTRIGGERACTIVITY (TRIGGERID, BUSINESSID,TRIGGERNAME,COMPANYID,SYSTEMCODE,SYSTEMNAME,
                            MODELCODE,MODELNAME,TRIGGERACTIVITYTYPE,TRIGGERTIME,TRIGGERSTART,TRIGGEREND,TRIGGERROUND,TRIGGERMULTIPLE,WCFURL,FUNCTIONNAME,FUNCTIONPARAMTER,PAMETERSPLITCHAR,
                            WCFBINDINGCONTRACT,RECEIVERUSERID,RECEIVEROLE,RECEIVERNAME,MESSAGEBODY,MESSAGEURL,TRIGGERTYPE,TRIGGERDESCRIPTION,CONTRACTTYPE,
                            CREATEUSERID,CREATEUSERNAME,REMARK) VALUES (:TRIGGERID,:BUSINESSID,:TRIGGERNAME,:COMPANYID,:SYSTEMCODE,:SYSTEMNAME,
                            :MODELCODE,:MODELNAME,:TRIGGERACTIVITYTYPE,:TRIGGERTIME,:TRIGGERSTART,:TRIGGEREND,:TRIGGERROUND,:TRIGGERMULTIPLE,:WCFURL,:FUNCTIONNAME,:FUNCTIONPARAMTER,:PAMETERSPLITCHAR,
                            :WCFBINDINGCONTRACT,:RECEIVERUSERID,:RECEIVEROLE,:RECEIVERNAME,:MESSAGEBODY,:MESSAGEURL,:TRIGGERTYPE,:TRIGGERDESCRIPTION,:CONTRACTTYPE,
                            :CREATEUSERID,:CREATEUSERNAME,:REMARK)";
                    OracleParameter[] pageparm =
                { 
                    new OracleParameter(":TRIGGERID",GetValue(entity.TRIGGERID)), 
                    new OracleParameter(":BUSINESSID",GetValue(entity.BUSINESSID)), 
                    new OracleParameter(":TRIGGERNAME",GetValue(entity.TRIGGERNAME)), 
                    new OracleParameter(":COMPANYID",GetValue(entity.COMPANYID)),
                    new OracleParameter(":SYSTEMCODE",GetValue(entity.SYSTEMCODE)),
                    new OracleParameter(":SYSTEMNAME",GetValue(entity.SYSTEMNAME)),
                    new OracleParameter(":MODELCODE",GetValue(entity.MODELCODE)), 
                    new OracleParameter(":MODELNAME",GetValue(entity.MODELNAME)), 
                    new OracleParameter(":TRIGGERACTIVITYTYPE",GetValue(entity.TRIGGERACTIVITYTYPE)),                   
                    new OracleParameter(":TRIGGERTIME",GetValue(entity.TRIGGERTIME)), 
                    new OracleParameter(":TRIGGERSTART",GetValue(entity.TRIGGERSTART)), 
                    new OracleParameter(":TRIGGEREND",GetValue(entity.TRIGGEREND)), 
                    new OracleParameter(":TRIGGERROUND",GetValue(entity.TRIGGERROUND)),
                    new OracleParameter(":TRIGGERMULTIPLE",GetValue(entity.TRIGGERMULTIPLE)),
                    new OracleParameter(":WCFURL",GetValue(entity.WCFURL)),
                    new OracleParameter(":FUNCTIONNAME",GetValue(entity.FUNCTIONNAME)), 
                    new OracleParameter(":FUNCTIONPARAMTER",GetValue(entity.FUNCTIONPARAMTER)), 
                    new OracleParameter(":PAMETERSPLITCHAR",GetValue(entity.PAMETERSPLITCHAR)),                   
                    new OracleParameter(":WCFBINDINGCONTRACT",GetValue(entity.WCFBINDINGCONTRACT)), 
                    new OracleParameter(":RECEIVERUSERID",GetValue(entity.RECEIVERUSERID)),
                    new OracleParameter(":RECEIVEROLE",GetValue(entity.RECEIVEROLE)),
                    new OracleParameter(":RECEIVERNAME",GetValue(entity.RECEIVERNAME)),
                    new OracleParameter(":MESSAGEBODY",GetValue(entity.MESSAGEBODY)), 
                    new OracleParameter(":MESSAGEURL",GetValue(entity.MESSAGEURL)), 
                    new OracleParameter(":TRIGGERTYPE",GetValue(entity.TRIGGERTYPE)), 
                    new OracleParameter(":TRIGGERDESCRIPTION",GetValue(entity.TRIGGERDESCRIPTION)), 
                    new OracleParameter(":CONTRACTTYPE",GetValue(entity.CONTRACTTYPE)),
                    new OracleParameter(":CREATEUSERID",GetValue(entity.CREATEUSERID)),
                    new OracleParameter(":CREATEUSERNAME",GetValue(entity.CREATEUSERNAME)),
                    new OracleParameter(":REMARK",GetValue(entity.REMARK))
                };

                    int insert = MsOracle.ExecuteNonQuery(con, CommandType.Text, insSql, pageparm);
                    return insert > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                dao.Close();
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 模块消息获取
        /// </summary>
        /// <param name="strSystemCode">系统代码</param>
        /// <param name="strModelCode">模块代码</param>
        /// <param name="strMsgBody">返回消息主体内容</param>
        /// <param name="strMsgUrl">返回消息链接</param>
        private void ModelMsgDefine(string strSystemCode, string strModelCode, string strCompanyCode, ref string strMsgBody, ref string strMsgUrl)
        {

            try
            {
                string sql = " SELECT MESSAGEBODY,MESSAGEURL FROM T_WF_MESSAGEDEFINE WHERE SYSTEMCODE='" + strSystemCode + "' AND MODELCODE='" + strModelCode + "' AND COMPANYID='" + strCompanyCode + "'";
                dao.Open();
                DataTable dt = dao.GetDataTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    strMsgBody = dt.Rows[0]["MESSAGEBODY"].ToString();
                    strMsgUrl = dt.Rows[0]["MESSAGEURL"].ToString();
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


        public bool AddTask(T_WF_DOTASK dask)
        {
            if (dask.ENGINECODE != "1")
            {
                ClosedDoTaskStatus(dask.SYSTEMCODE, dask.ORDERID, dask.RECEIVEUSERID);
            }
            try
            {
                using (OracleConnection con = new OracleConnection(dao.DatabaseString))
                {
                    string insSql = @"INSERT INTO T_WF_DOTASK (DOTASKID,COMPANYID,ORDERID,ORDERUSERID,ORDERUSERNAME,ORDERSTATUS,MESSAGEBODY,
                                     APPLICATIONURL,RECEIVEUSERID,BEFOREPROCESSDATE,ENGINECODE,DOTASKTYPE,DOTASKSTATUS,MAILSTATUS,
                                     RTXSTATUS,SYSTEMCODE,MODELCODE,REMARK)
                                     VALUES (:DOTASKID,:COMPANYID,:ORDERID,:ORDERUSERID,:ORDERUSERNAME,:ORDERSTATUS,:MESSAGEBODY,
                                    :APPLICATIONURL,:RECEIVEUSERID,:BEFOREPROCESSDATE,:ENGINECODE,:DOTASKTYPE,:DOTASKSTATUS,:MAILSTATUS,
                                    :RTXSTATUS,:SYSTEMCODE,:MODELCODE,:REMARK)";
                    #region
                    #region
                    OracleParameter[] pageparm =
                        {               
                            new OracleParameter(":DOTASKID",OracleType.NVarChar,100), 
                            new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERUSERID",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERUSERNAME",OracleType.NVarChar,100), 
                            new OracleParameter(":ORDERSTATUS",OracleType.Number), 
                            new OracleParameter(":MESSAGEBODY",OracleType.NVarChar,4000), 
                            new OracleParameter(":APPLICATIONURL",OracleType.NVarChar,2000), 
                            new OracleParameter(":RECEIVEUSERID",OracleType.NVarChar,100), 
                            new OracleParameter(":BEFOREPROCESSDATE",OracleType.DateTime), 
                            new OracleParameter(":ENGINECODE",OracleType.NVarChar,100), 
                            new OracleParameter(":DOTASKTYPE",OracleType.Number),
                            new OracleParameter(":DOTASKSTATUS",OracleType.Number), 
                            new OracleParameter(":MAILSTATUS",OracleType.Number), 
                            new OracleParameter(":RTXSTATUS",OracleType.Number),
                            new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                            new OracleParameter(":REMARK",OracleType.NVarChar,200),                  

                        };
                    #endregion
                    pageparm[0].Value = MsOracle.GetValue(Guid.NewGuid().ToString());//待办任务ID
                    pageparm[1].Value = MsOracle.GetValue(dask.COMPANYID);//公司ID
                    pageparm[2].Value = MsOracle.GetValue(dask.ORDERID);//单据ID
                    pageparm[3].Value = MsOracle.GetValue(dask.RECEIVEUSERID);//单据所属人ID
                    pageparm[4].Value = MsOracle.GetValue(dask.ORDERUSERNAME);//单据所属人名称
                    pageparm[5].Value = MsOracle.GetValue(0);//单据状态
                    pageparm[6].Value = MsOracle.GetValue(dask.MESSAGEBODY);
                    pageparm[7].Value = MsOracle.GetValue(dask.APPLICATIONURL);
                    pageparm[8].Value = MsOracle.GetValue(dask.RECEIVEUSERID);
                    pageparm[9].Value = MsOracle.GetValue(DateTime.Now.AddDays(3));
                    pageparm[10].Value = MsOracle.GetValue("");//
                    pageparm[11].Value = MsOracle.GetValue(4);//待办任务类型(0、待办任务、1、流程咨询、3 ) 
                    pageparm[12].Value = MsOracle.GetValue(0);//代办任务状态(0、未处理 1、已处理 、2、任务撤销 10、删除)
                    pageparm[13].Value = MsOracle.GetValue(0);//邮件状态(0、未发送 1、已发送、2、未知 )
                    pageparm[14].Value = MsOracle.GetValue(0);//RTX状态(0、未发送 1、已发送、2、未知 )
                    pageparm[15].Value = MsOracle.GetValue(dask.SYSTEMCODE);
                    pageparm[16].Value = MsOracle.GetValue(dask.MODELCODE);
                    pageparm[17].Value = MsOracle.GetValue("未提交单据");
                    #endregion
                    return MsOracle.ExecuteNonQuery(con, CommandType.Text, insSql, pageparm) > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("新增未提交单据待办消息出错:" + ex);
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
