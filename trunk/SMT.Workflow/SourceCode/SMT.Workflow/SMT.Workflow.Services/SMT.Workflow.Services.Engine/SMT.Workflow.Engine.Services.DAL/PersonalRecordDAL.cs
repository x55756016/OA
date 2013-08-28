/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：PersonalRecordDAL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/26 9:29:51   
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
using System.Data;
using EngineDataModel;
using SMT.Workflow.Common.DataAccess;
using System.Data.OracleClient; 

namespace SMT.Workflow.Engine.Services.DAL
{
    public class PersonalRecordDAL : BaseDAL
    {
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
        public string GetExistDoTask(string modelCode, string orderID, string owenrID, string companyID)
        {

            try
            {
                string sql = "SELECT DOTASKID FROM T_WF_DOTASK WHERE  MODELCODE='" + modelCode + "' AND ORDERID='" + orderID + "' ";
                sql += " AND RECEIVEUSERID='" + owenrID + "' AND COMPANYID='" + companyID + "' AND DOTASKSTATUS=0  AND rownum=1";
                dao.Open();             
                object obj = dao.ExecuteScalar(sql);
                return obj == null ? "" : obj.ToString();
            }

            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetExistDoTask()", "查询是否存在我的单据的代办", ex);
                return "";
            }
            finally
            {
                dao.Close();
            }
        }
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
        public string GetExistRecord(string sysCode, string modelCode, string orderID, string owenrID, string companyID, string forwar)
        {
           
            try
            {
                string sql = "SELECT PERSONALRECORDID FROM T_WF_PERSONALRECORD WHERE SYSTYPE='" + sysCode + "' AND MODELCODE='" + modelCode + "' AND MODELID='" + orderID + "' ";
                sql += " AND OWNERID='" + owenrID + "' AND OWNERCOMPANYID='" + companyID + "' AND ISFORWARD=" + forwar + " AND rownum=1";
                dao.Open();              
                object obj = dao.ExecuteScalar(sql);
                return obj == null ? "" : obj.ToString();
            }

            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetExistRecord()", "查询是否存在我的单据", ex);
                return "";
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 新增我的单据
        /// </summary>
        /// <param name="model"></param>       
        /// <returns></returns>
        public bool AddPersonalRecord(T_PF_PERSONALRECORD model)
        {
           
            try
            {
                string sql = @"INSERT INTO T_WF_PERSONALRECORD(PERSONALRECORDID,SYSTYPE,MODELCODE,MODELID,CHECKSTATE,OWNERID,OWNERPOSTID,OWNERDEPARTMENTID,
                              OWNERCOMPANYID,CONFIGINFO,MODELDESCRIPTION,ISFORWARD) VALUES 
                             ('" + Guid.NewGuid().ToString() + "','" + model.SYSTYPE + "','" + model.MODELCODE + "','" + model.MODELID + "'," + model.CHECKSTATE + ",";
                sql += @"'" + model.OWNERID + "','" + model.OWNERPOSTID + "','" + model.OWNERDEPARTMENTID + "',";
                //sql += @"to_date('" + Convert.ToDateTime(entity.TRIGGERTIME).ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi:ss')          
                sql += @"'" + model.OWNERCOMPANYID + "','" + model.CONFIGINFO + "','" + model.MODELDESCRIPTION + "'," + (model.ISFORWARD == "1" ? 1 : 0) + ")";
                Log.WriteLog("AddPersonalRecord():" + sql + "");
                dao.Open();
                LogHelper.WriteLog("新增我的单据 FormID=" + model.MODELID+ "");
                return dao.ExecuteNonQuery(sql) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "AddPersonalRecord()", "新增我的单据发生异常 FormID=" + model.MODELID + "", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }

        public bool AddDoTask(T_PF_PERSONALRECORD model)
        {         
            ClosedDoTaskStatus(model.SYSTYPE, model.MODELID, model.OWNERID);
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
                    pageparm[1].Value = MsOracle.GetValue(model.OWNERCOMPANYID);//公司ID
                    pageparm[2].Value = MsOracle.GetValue(model.MODELID);//单据ID
                    pageparm[3].Value = MsOracle.GetValue(model.OWNERID);//单据所属人ID
                    pageparm[4].Value = MsOracle.GetValue("");//单据所属人名称
                    pageparm[5].Value = MsOracle.GetValue(0);//单据状态
                    pageparm[6].Value = MsOracle.GetValue(model.MODELDESCRIPTION);
                    pageparm[7].Value = MsOracle.GetValue(model.CONFIGINFO);
                    pageparm[8].Value = MsOracle.GetValue(model.OWNERID);
                    pageparm[9].Value = MsOracle.GetValue(DateTime.Now.AddDays(3));
                    pageparm[10].Value = MsOracle.GetValue("");//
                    pageparm[11].Value = MsOracle.GetValue(4);//待办任务类型(0、待办任务、1、流程咨询、3  ) 
                    pageparm[12].Value = MsOracle.GetValue(0);//代办任务状态(0、未处理 1、已处理 、2、任务撤销 10、删除)
                    pageparm[13].Value = MsOracle.GetValue(1);//邮件状态(0、未发送 1、已发送、2、未知 )
                    pageparm[14].Value = MsOracle.GetValue(1);//RTX状态(0、未发送 1、已发送、2、未知 )
                    pageparm[15].Value = MsOracle.GetValue(model.SYSTYPE);
                    pageparm[16].Value = MsOracle.GetValue(model.MODELCODE);
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
                string sql = "UPDATE T_WF_DOTASK SET DOTASKSTATUS=1, CLOSEDDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi'),REMARK='单据未提交代办关闭' WHERE DOTASKSTATUS=0 AND  SYSTEMCODE='" + strSystemCode + "' AND ORDERID='" + strFormID + "'";
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
                LogHelper.WriteLog(this, "ClosedDoTaskStatus()", "单据未提交代办关闭", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="recordID"></param>
        /// <returns></returns>
        public bool UpdatePersonalRecord(T_PF_PERSONALRECORD model, string recordID)
        {
         
            try
            {
                string sql = "UPDATE T_WF_PERSONALRECORD SET CHECKSTATE=" + model.CHECKSTATE + ",CONFIGINFO='" + model.CONFIGINFO + "',MODELDESCRIPTION='" + model.MODELDESCRIPTION + "',";
                sql += "UPDATEDATE=to_date('" + DateTime.Now.ToString("yyyy/MM/dd hh:mm") + "','YYYY-MM-DD hh24:mi') WHERE PERSONALRECORDID='" + recordID + "'";
                Log.WriteLog("UpdatePersonalRecord():" + sql + "");
                dao.Open();
                LogHelper.WriteLog("修改我的单据 FormID=" + model.MODELID + " SQL语句＝" + sql);
                return dao.ExecuteNonQuery(sql) >= 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "UpdatePersonalRecord()", "修改我的单据发生异常 FormID=" + model.MODELID + "", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }

        }
        /// <summary>
        /// 删除我的单据
        /// </summary>
        /// <param name="recordID"></param>
        /// <returns></returns>
        public bool DeletePersonalRecord(string recordID)
        {
            try
            {
                string sql = "DELETE  T_WF_DOTASK  WHERE DOTASKID='" + recordID + "' AND DOTASKSTATUS = 0";
                dao.Open();
                return dao.ExecuteNonQuery(sql) >= 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "DeletePersonalRecord()", "删除我的单据的代办", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }

        public T_PF_PERSONALRECORD GetEntity(string RecordID)
        {
            try
            {
                string sql = "SELECT * FROM T_WF_PERSONALRECORD WHERE PERSONALRECORDID='" + RecordID + "'";
                dao.Open();
                T_PF_PERSONALRECORD record = new T_PF_PERSONALRECORD();
                DataTable dt = dao.GetDataTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    record.PERSONALRECORDID = dt.Rows[0]["PERSONALRECORDID"].ToString();
                    record.SYSTYPE = dt.Rows[0]["SYSTYPE"].ToString();
                    record.MODELCODE = dt.Rows[0]["MODELCODE"].ToString();
                    record.MODELID = dt.Rows[0]["MODELID"].ToString();
                    record.CHECKSTATE = dt.Rows[0]["CHECKSTATE"].ToString();
                    record.OWNERID = dt.Rows[0]["OWNERID"].ToString();
                    record.OWNERPOSTID = dt.Rows[0]["OWNERPOSTID"].ToString();
                    record.OWNERDEPARTMENTID = dt.Rows[0]["OWNERDEPARTMENTID"].ToString();
                    record.OWNERCOMPANYID = dt.Rows[0]["OWNERCOMPANYID"].ToString();
                    record.CONFIGINFO = dt.Rows[0]["CONFIGINFO"].ToString();
                    record.MODELDESCRIPTION = dt.Rows[0]["MODELDESCRIPTION"].ToString();
                    record.ISFORWARD = dt.Rows[0]["ISFORWARD"].ToString();
                    record.ISVIEW = dt.Rows[0]["ISVIEW"].ToString();
                    record.CREATEDATE = Convert.ToDateTime(dt.Rows[0]["CREATEDATE"].ToString());
                }
                return record;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetEntity()", "获取单据实体", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 调整内容，原来是写入我的单据的数据，现在改为代办，查询做删除的时间要删除代办
        /// </summary>
        /// <param name="sysType"></param>
        /// <param name="modelCode"></param>
        /// <param name="modelID"></param>
        /// <param name="IsForward"></param>
        /// <returns></returns>
        public T_PF_PERSONALRECORD GetEntity(string sysType, string modelCode, string modelID, string IsForward)
        {
            try
            {
                string sql = "SELECT * FROM T_WF_DOTASK WHERE (1=1)";
                if (!string.IsNullOrEmpty(sysType))
                {
                    sql += "  AND SYSTEMCODE='" + sysType + "'";
                }
                if (!string.IsNullOrEmpty(modelCode))
                {
                    sql += "  AND MODELCODE ='" + modelCode + "'";
                }
                if (!string.IsNullOrEmpty(modelID))
                {
                    sql += "  AND ORDERID='" + modelID + "'";
                }
                sql += "  AND DOTASKSTATUS = 0";
               
                dao.Open();
                T_PF_PERSONALRECORD record = new T_PF_PERSONALRECORD();
                DataTable dt = dao.GetDataTable(sql);            
                if (dt != null && dt.Rows.Count > 0)
                {
                    record.PERSONALRECORDID = dt.Rows[0]["DOTASKID"].ToString();
                    record.SYSTYPE = dt.Rows[0]["SYSTEMCODE"].ToString();
                    record.MODELCODE = dt.Rows[0]["MODELCODE"].ToString();
                    record.MODELID = dt.Rows[0]["ORDERID"].ToString();
                    record.CHECKSTATE = "0";
                    record.OWNERID = dt.Rows[0]["RECEIVEUSERID"].ToString();
                    //record.OWNERPOSTID = dt.Rows[0]["OWNERPOSTID"].ToString();
                    //record.OWNERDEPARTMENTID = dt.Rows[0]["OWNERDEPARTMENTID"].ToString();
                    //record.OWNERCOMPANYID = dt.Rows[0]["OWNERCOMPANYID"].ToString();
                    record.CONFIGINFO = dt.Rows[0]["APPLICATIONURL"].ToString();
                    //record.MODELDESCRIPTION = dt.Rows[0]["MODELDESCRIPTION"].ToString();
                    //record.ISFORWARD = dt.Rows[0]["ISFORWARD"].ToString();
                    //record.ISVIEW = dt.Rows[0]["ISVIEW"].ToString();
                    record.CREATEDATE = Convert.ToDateTime(dt.Rows[0]["CREATEDATETIME"].ToString());
                }
                return record;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetEntity()", "获取我的单据代办实体", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 获取我的单据（两种状态；未提交、转发）
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public List<T_PF_PERSONALRECORD> GetPersonalRecordList(int pageSize, string strFilter, int pageIndex, string strOrderBy, ref int pageCount)
        {
            try
            {
                if (string.IsNullOrEmpty(strOrderBy))
                {
                    strOrderBy = "CREATEDATE DESC";
                }
                if (pageSize == 0)
                {
                    pageSize = 15;
                }
                int number = pageIndex <= 1 ? 1 : (pageIndex - 1) * pageSize + 1;
                dao.Open();
                string countSql = @"SELECT count(1)  from T_WF_PERSONALRECORD where (1=1)";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    countSql += strFilter + "";
                }
                string sql = @"SELECT * FROM (SELECT A.*, ROWNUM Page FROM (select * from T_WF_PERSONALRECORD 
                                   order by " + strOrderBy + " ) A WHERE (1=1) AND ROWNUM<= " + pageIndex * pageSize + "";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    sql += strFilter + ")";
                }
                sql += "WHERE  Page >= " + number + "";
                DataTable dt = dao.GetDataTable(sql);
                LogHelper.WriteLog("查找我的单据SQL："+sql);
                pageCount = Convert.ToInt32(dao.ExecuteScalar(countSql));
                //LogHelper.WriteLog(countSql);
                pageCount = pageCount / pageSize + (pageCount % pageSize > 0 ? 1 : 0);
                List<T_PF_PERSONALRECORD> list = new List<T_PF_PERSONALRECORD>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        T_PF_PERSONALRECORD entity = new T_PF_PERSONALRECORD();
                        entity.PERSONALRECORDID = dr["PERSONALRECORDID"].ToString();
                        entity.SYSTYPE = dr["SYSTYPE"].ToString();
                        entity.MODELCODE = dr["MODELCODE"].ToString();
                        entity.MODELID = dr["MODELID"].ToString();
                        entity.CHECKSTATE = dr["CHECKSTATE"].ToString();
                        entity.MODELDESCRIPTION = dr["MODELDESCRIPTION"].ToString();
                        entity.CREATEDATE = Convert.ToDateTime(dr["CREATEDATE"].ToString());
                        entity.CONFIGINFO = dr["CHECKSTATE"].ToString() == "3" ? dr["CONFIGINFO"].ToString().Replace("<FormTypes>Audit", "<FormTypes>Resubmit").Replace("<FormTypes>Edit", "<FormTypes>Resubmit") : dr["CONFIGINFO"].ToString(); ;
                        list.Add(entity);
                    }
                }
                return list;

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetPersonalRecordList()", "获取我的单据（两种状态；未提交、转发）", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 获取我的单据列表（三种状态，审核中，审核通过，审核不通过）
        /// </summary>
        /// <param name="strOrderBy"></param>
        /// <param name="strCreateID"></param>
        /// <param name="pageIndex"></param>
        /// <param name="checkstate"></param>
        /// <param name="strFilter"></param>
        /// <param name="BeginDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_PF_PERSONALRECORD> GetPersonalRecord(int pageSize, string strOrderBy, string strCreateID, int pageIndex, string checkstate, string strFilter,
          string BeginDate, string EndDate, ref int pageCount)
        {
            try
            {               
                if (string.IsNullOrEmpty(strOrderBy))
                {
                    strOrderBy = "CREATEDATE DESC";
                }
                if (pageSize == 0)
                {
                    pageSize = 15;
                }
                string filter = "";
                if (!string.IsNullOrEmpty(checkstate))
                {
                    filter += " AND CHECKSTATE=" + checkstate + "";
                }
                if (!string.IsNullOrEmpty(strCreateID))
                {
                    filter += " AND CREATEUSERID='" + strCreateID + "'";
                }
                int number = pageIndex <= 1 ? 1 : (pageIndex - 1) * pageSize + 1;
                dao.Open();             
                string countSql = @"SELECT COUNT(1) FROM(
                                SELECT FORMID, MODELCODE FROM FLOW_FLOWRECORDMASTER_T  WHERE (1=1) {0} GROUP BY  FORMID,MODELCODE
                                ) B
                                INNER JOIN (SELECT ORDERID,MODELCODE FROM T_WF_DOTASK GROUP BY  ORDERID ,MODELCODE) C
                                ON C.ORDERID=B.FORMID AND B.MODELCODE=C.MODELCODE WHERE (1=1) ";
                countSql = string.Format(countSql, filter);               
                string sql = @"SELECT D.*,C.DESCRIPTION FROM 
                            (
                            SELECT B.SYSTYPE,B.MODELCODE,A.FORMID MODELID,A.CHECKSTATE,A.CREATEDATE,B.CONFIGINFO,rownum page FROM 
                            (
                            SELECT FORMID,MAX(CHECKSTATE) CHECKSTATE,MAX(CREATEDATE) CREATEDATE, MODELCODE FROM FLOW_FLOWRECORDMASTER_T  WHERE (1=1) {0} GROUP BY  FORMID,MODELCODE ORDER BY CREATEDATE DESC
                            ) A
                            INNER JOIN (SELECT ORDERID,MODELCODE,MAX(APPLICATIONURL) CONFIGINFO,MAX(SYSTEMCODE) SYSTYPE,MAX(CREATEDATETIME) CREATEDATE FROM T_WF_DOTASK GROUP BY  ORDERID ,MODELCODE order by
                            CREATEDATE desc) B
                            ON B.ORDERID=A.FORMID AND A.MODELCODE=B.MODELCODE where rownum<={1}
                            ) D
                            LEFT OUTER JOIN  FLOW_MODELDEFINE_T C ON C.MODELCODE= D.MODELCODE where D.page>={2} order by D.CREATEDATE desc";
                sql = string.Format(sql, filter, pageIndex * pageSize, number);           
                DataTable dt = dao.GetDataTable(sql);
                pageCount = Convert.ToInt32(dao.ExecuteScalar(countSql));              
                pageCount = pageCount / pageSize + (pageCount % pageSize > 0 ? 1 : 0);
                List<T_PF_PERSONALRECORD> list = new List<T_PF_PERSONALRECORD>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        T_PF_PERSONALRECORD entity = new T_PF_PERSONALRECORD();
                        entity.PERSONALRECORDID = Guid.NewGuid().ToString();
                        entity.SYSTYPE = dr["SYSTYPE"].ToString();
                        entity.MODELCODE = dr["MODELCODE"].ToString();
                        entity.MODELID = dr["MODELID"].ToString();
                        entity.CHECKSTATE = dr["CHECKSTATE"].ToString();
                        entity.MODELDESCRIPTION = string.Format("{0:F}", Convert.ToDateTime(dr["CREATEDATE"].ToString())) + " " + dr["DESCRIPTION"].ToString();
                        entity.CREATEDATE = Convert.ToDateTime(dr["CREATEDATE"].ToString());
                        entity.CONFIGINFO = dr["CHECKSTATE"].ToString() == "3" ? dr["CONFIGINFO"].ToString().Replace("Audit", "Resubmit").Replace("Edit", "Resubmit") : dr["CONFIGINFO"].ToString();                        
                        list.Add(entity);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(this, "GetPersonalRecord()", "获取我的单据列表（三种状态，审核中，审核通过，审核不通过）", ex);
                return null;
            }
            finally
            {
                dao.Close();
            }
        }
    }
}
