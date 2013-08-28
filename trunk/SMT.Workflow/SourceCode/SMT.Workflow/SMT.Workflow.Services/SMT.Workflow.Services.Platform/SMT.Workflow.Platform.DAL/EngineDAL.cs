using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using System.Data;
using System.Linq.Dynamic;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Data.OracleClient;
namespace SMT.Workflow.Platform.DAL
{
    public class EngineDAL : BaseDAL
    {
        #region 默认消息增、删、改、查方法
        //默认消息查询
        public IQueryable<T_WF_MESSAGEBODYDEFINE> GetFlowMsgDefine()
        {
            try
            {
                string sql = "SELECT * FROM T_WF_MESSAGEBODYDEFINE";
                dao.Open();
                DataTable dtFlowModelDefine = dao.GetDataTable(sql);
                dao.Close();
                var items = ToList<T_WF_MESSAGEBODYDEFINE>(dtFlowModelDefine);
                return items.AsQueryable();
            }
            catch 
            {
                return null;
            }
        }
        //默认消息条件查询
        public IQueryable<T_WF_MESSAGEBODYDEFINE> GetListFlowMsgDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            try
            {
                string sql = string.Empty;
                if (filterString != null && filterString!= "")
                {
                    sql = "SELECT * FROM T_WF_MESSAGEBODYDEFINE where " + filterString;
                }
                else 
                {
                    sql = "SELECT * FROM T_WF_MESSAGEBODYDEFINE";
                }
                dao.Open();
                DataTable dtEngineNews = dao.GetDataTable(sql);
                dao.Close();
                var items = ToList<T_WF_MESSAGEBODYDEFINE>(dtEngineNews).AsQueryable();
                items = Pager<T_WF_MESSAGEBODYDEFINE>(items, pageIndex, pageSize, ref pageCount);
                return items;
            }
            catch
            {
                return null;
            }
        }
        //默认消息查询
        public T_WF_MESSAGEBODYDEFINE GetListFlowMsgBodyDefine(string moduleCode)
        {
            try
            {
                string sql = "SELECT * FROM T_WF_MESSAGEBODYDEFINE where MODELCODE = '" + moduleCode + "'";
                dao.Open();
                DataTable dtFlowModelDefine = dao.GetDataTable(sql);
                dao.Close();
                var items = ToList<T_WF_MESSAGEBODYDEFINE>(dtFlowModelDefine);
                return items[0];
            }
            catch
            {
                return null;
            }
        }
        //添加默认消息
        public bool AddFlowMsgDefine(T_WF_MESSAGEBODYDEFINE FlowMsg)
        {
            try
            {
                int result = 0;

                dao.Open();

                //string sql = "insert into T_WF_MESSAGEBODYDEFINE(DEFINEID, SYSTEMCODE, MODELCODE, MESSAGEBODY, MESSAGEURL, CREATEDATE,COMPANYID,CREATEUSERNAME,CREATEUSERID,MESSAGETYPE)"
                //                                 + "values('" + FlowMsg.DEFINEID + "','" + FlowMsg.SYSTEMCODE + "','" + FlowMsg.MODELCODE + "',"
                //                                        + "'" + FlowMsg.MESSAGEBODY + "','" + FlowMsg.MESSAGEURL + "',to_date('" + FlowMsg.CREATEDATE + "','yyyy-mm-dd hh24:mi:ss'),"
                //                                        + "'" + FlowMsg.COMPANYID + "','" + FlowMsg.CREATEUSERNAME + "','" + FlowMsg.CREATEUSERID + "','" + FlowMsg.MESSAGETYPE + "')";
                //result = dao.ExecuteNonQuery(sql);

                 string insSql = "INSERT INTO T_WF_MESSAGEBODYDEFINE (DEFINEID,COMPANYID,SYSTEMCODE,MODELCODE,MESSAGEBODY,MESSAGEURL,MESSAGETYPE,CREATEDATE,CREATEUSERNAME,CREATEUSERID) VALUES (:DEFINEID,:COMPANYID,:SYSTEMCODE,:MODELCODE,:MESSAGEBODY,:MESSAGEURL,:MESSAGETYPE,:CREATEDATE,:CREATEUSERNAME,:CREATEUSERID)";
            OracleParameter[] pageparm =
                {  
                    new OracleParameter(":DEFINEID",GetValue(FlowMsg.DEFINEID)), //默认消息ID 
                    new OracleParameter(":COMPANYID",GetValue(FlowMsg.COMPANYID)), //公司ID 
                    new OracleParameter(":SYSTEMCODE",GetValue(FlowMsg.SYSTEMCODE)), //系统代号 
                    new OracleParameter(":MODELCODE",GetValue(FlowMsg.MODELCODE)), //模块代码 
                    new OracleParameter(":MESSAGEBODY",GetValue(FlowMsg.MESSAGEBODY)), //消息体 
                    new OracleParameter(":MESSAGEURL",GetValue(FlowMsg.MESSAGEURL)), //消息链接 
                    new OracleParameter(":MESSAGETYPE",GetValue(FlowMsg.MESSAGETYPE)), //消息类型 
                    new OracleParameter(":CREATEDATE",GetValue(FlowMsg.CREATEDATE)), //创建日期 
                    new OracleParameter(":CREATEUSERNAME",GetValue(FlowMsg.CREATEUSERNAME)), //创建人名称 
                    new OracleParameter(":CREATEUSERID",GetValue(FlowMsg.CREATEUSERID)) //创建人 

                };
               result = dao.ExecuteNonQuery(insSql, System.Data.CommandType.Text, pageparm);
                return result > 0 ? true : false;
            }
            catch
            {
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        //修改默认消息
        public bool UpdateFlowMsgDefine(T_WF_MESSAGEBODYDEFINE FlowMsg)
        {
            try
            {
                int result = 0;
                dao.Open();
                //string sql = "update  T_WF_MESSAGEBODYDEFINE set "
                //                         + "SYSTEMCODE = '" + FlowMsg.SYSTEMCODE + "', "
                //                         + "MODELCODE = '" + FlowMsg.MODELCODE + "', "
                //                         + "MESSAGEBODY = '" + FlowMsg.MESSAGEBODY + "',"
                //                         + "MESSAGEURL = '" + FlowMsg.MESSAGEURL + "' "
                //                         + "where DEFINEID = '" + FlowMsg.DEFINEID + "' ";

                //result = dao.ExecuteNonQuery(sql);

                 string updSql = "UPDATE T_WF_MESSAGEBODYDEFINE SET SYSTEMCODE=:SYSTEMCODE,MODELCODE=:MODELCODE,MESSAGEBODY=:MESSAGEBODY,MESSAGEURL=:MESSAGEURL WHERE   DEFINEID=:DEFINEID";
            OracleParameter[] pageparm =
                { 
                    new OracleParameter(":DEFINEID",GetValue(FlowMsg.DEFINEID)), //默认消息ID 
                    new OracleParameter(":SYSTEMCODE",GetValue(FlowMsg.SYSTEMCODE)), //系统代号 
                    new OracleParameter(":MODELCODE",GetValue(FlowMsg.MODELCODE)), //模块代码 
                    new OracleParameter(":MESSAGEBODY",GetValue(FlowMsg.MESSAGEBODY)), //消息体 
                    new OracleParameter(":MESSAGEURL",GetValue(FlowMsg.MESSAGEURL)) //消息链接 

                };
            result = dao.ExecuteNonQuery(updSql, System.Data.CommandType.Text, pageparm);
                return result > 0 ? true : false;
            }
            catch
            {
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        //删除默认消息
        public bool DeleteFlowMsgDefine(List<T_WF_MESSAGEBODYDEFINE> FlowMsglList)
        {
            try
            {
                int result = 0;
                dao.Open();
                dao.BeginTransaction();

                foreach (var item in FlowMsglList)
                {
                    string Sql = "delete from T_WF_MESSAGEBODYDEFINE "
                                  + "where DEFINEID = '" + item.DEFINEID + "' "; 
                    result = dao.ExecuteNonQuery(Sql);
                    if (result == 0)
                    {
                        dao.RollbackTransaction();
                        return false;
                    }
                }

                dao.CommitTransaction();
                return result > 0 ? true : false;
            }
            catch 
            {
                dao.RollbackTransaction();
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        //删除默认消息
        public bool DeleteFlowMsgsDefine(T_WF_MESSAGEBODYDEFINE FlowMsglList)
        {
            try
            {
                int result = 0;
                dao.Open();
                dao.BeginTransaction();
                string Sql = "delete from T_WF_MESSAGEBODYDEFINE "
                                + "where DEFINEID = '" + FlowMsglList.DEFINEID + "' ";
                result = dao.ExecuteNonQuery(Sql);
                if (result == 0)
                {
                    dao.RollbackTransaction();
                    return false;
                }
                dao.CommitTransaction();
                return result > 0 ? true : false;
            }
            catch 
            {
                dao.RollbackTransaction();
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        #endregion
    }
}
