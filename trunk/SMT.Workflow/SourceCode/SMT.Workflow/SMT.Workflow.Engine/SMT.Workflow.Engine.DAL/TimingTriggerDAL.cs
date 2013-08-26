/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：TimingTriggerDAL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/23 14:28:00   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.DAL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SMT.Workflow.Engine.DAL
{
    /// <summary>
    /// 定时数据层
    /// </summary>
    public class TimingTriggerDAL : BaseDAL
    {

        /// <summary>
        /// 获取定时触发活动数据
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetTimingTriggerList()
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = @"SELECT * FROM T_WF_TIMINGTRIGGERACTIVITY 
                                WHERE  TRIGGERTIME<=to_date('" + DateTime.Now.ToString() + "','YYYY-MM-DD hh24:mi:ss') and TRIGGEREND>=to_date('" + DateTime.Now.ToString() + "','YYYY-MM-DD hh24:mi:ss') and TRIGGERSTART<=to_date('" + DateTime.Now.ToString() + "','YYYY-MM-DD hh24:mi:ss') ";
                dao.Open();
                dt = dao.GetDataTable(sql);
                return dt;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "GetTimingTriggerList()", "获取定时触发活动数据", ex);
                return null;
            }
            finally
            {
                dt.Dispose();
                dao.Close();
            }
        }

        /// <summary>
        /// 删除触发数据
        /// </summary>
        /// <param name="businessid">ID</param>
        /// <returns>bool</returns>
        public bool DeleteTrigger(string triggerID)
        {
            try
            {
                string sql = "DELETE T_WF_TIMINGTRIGGERACTIVITY  WHERE TRIGGERID='" + triggerID + "'";
                dao.Open();
                return dao.ExecuteNonQuery(sql) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "DeleteTrigger()", "删除触发数据triggerID:" + triggerID + " ", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 删除触发数据
        /// </summary>
        /// <param name="businessid">ID</param>
        /// <returns>bool</returns>
        public bool DeleteTriggerByBusinessId(string businessid)
        {
            try
            {
                string sql = "DELETE T_WF_TIMINGTRIGGERACTIVITY  WHERE BUSINESSID='" + businessid + "'";
                dao.Open();
                return dao.ExecuteNonQuery(sql) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "DeleteTrigger()", "删除触发数据 BUSINESSID:" + businessid + " ", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 修改触发时间
        /// </summary>
        /// <param name="triggerID">ID</param>
        /// <param name="triggerdate">时间</param>
        /// <returns>bool</returns>
        public bool UpdateTriggerDate(string triggerID, string triggerdate)
        {
            try
            {
                string sql = "UPDATE T_WF_TIMINGTRIGGERACTIVITY SET TRIGGERTIME=to_date('" + triggerdate + "','YYYY-MM-DD hh24:mi:ss') WHERE TRIGGERID='" + triggerID + "'";
                dao.Open();
                return dao.ExecuteNonQuery(sql) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "UpdateTriggerDate()", "修改触发时间triggerID:" + triggerID + "、triggerdate：" + triggerdate + " ", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
        /// <summary>
        /// 获取短信发送的代办任务
        /// </summary>   
        /// <returns>DataTable</returns>
        public DataTable GetSMSDoTask()
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "SELECT   RECEIVEUSERID,COUNT(DOTASKID) TASKCOUNT  FROM T_WF_DOTASK WHERE  DOTASKSTATUS = 0  GROUP BY RECEIVEUSERID";
                dao.Open();
                dt = dao.GetDataTable(sql);
                return dt;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "GetSMSDoTask()", "获取短信发送的代办任务", ex);
                return null;
            }
            finally
            {
                dt.Dispose();
                dao.Close();
            }
        }

        /// <summary>
        /// 插入一条发送短信的记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>int</returns>
        public int AddSMSRecord(T_FLOW_SMSRECORD entity)
        {
            try
            {
                string sql = "INSERT INTO T_WF_SMSRECORD(SMSRECORD,BATCHNUMBER,COMPANYID,SENDSTATUS,ACCOUNT,MOBILE,SENDMESSAGE,SENDTIME,OWNERID,OWNERNAME,TASKCOUNT,REMARK) ";
                sql += " VALUES ('" + entity.SMSRECORD + "','" + entity.BATCHNUMBER + "','" + entity.COMPANYID + "','" + entity.SENDSTATUS + "','" + entity.ACCOUNT + "',";
                sql += " '" + entity.MOBILE + "','" + entity.SENDMESSAGE + "',to_date('" + entity.SENDTIME + "','YYYY-MM-DD hh24:mi:ss'),'" + entity.OWNERID + "','" + entity.OWNERNAME + "'," + entity.TASKCOUNT + ",'" + entity.REMARK + "')";
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "AddSMSRecord()", "插入一条发送短信的记录", ex);
                return 0;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 添加定时触发发送记录 
        /// </summary>
        /// <param name="dr">DataRow</param>
        /// <param name="remark">remark</param>
        /// <returns>bool</returns>
        public bool AddTimingRecord(DataRow dr,string remark)
        {
            try
            {
                string sql = @"INSERT INTO T_WF_TIMINGTRIGGERRECORD(RECORDID,TRIGGERID,TRIGGERNAME,COMPANYID,SYSTEMCODE,MODELCODE,
                              MODELNAME,TRIGGERACTIVITYTYPE,TRIGGERTIME,TRIGGERROUND,WCFURL,
                              FUNCTIONNAME,FUNCTIONPARAMTER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,RECEIVERUSERID,RECEIVEROLE,
                              RECEIVERNAME,MESSAGEBODY,MESSAGEURL,TRIGGERSTATUS,TRIGGERTYPE,TRIGGERDESCRIPTION,CONTRACTTYPE,CREATEUSERNAME,REMARK) VALUES 
                             ('" + Guid.NewGuid().ToString() + "','" + dr["TRIGGERID"].ToString() + "','" + dr["TRIGGERNAME"].ToString() + "','" + dr["COMPANYID"].ToString() + "','" + dr["SYSTEMCODE"].ToString() + "',";
                sql += @"'" + dr["MODELCODE"].ToString() + "','" + dr["MODELNAME"].ToString() + "','" + dr["TRIGGERACTIVITYTYPE"].ToString() + "',";
                sql += @"to_date('" + dr["TRIGGERTIME"].ToString()+"','YYYY-MM-DD hh24:mi:ss'),'" + dr["TRIGGERROUND"].ToString() + "','" + dr["WCFURL"].ToString() + "',";
                sql += @"'" + dr["FUNCTIONNAME"].ToString() + "','" + dr["FUNCTIONPARAMTER"].ToString() + "','" + dr["PAMETERSPLITCHAR"].ToString() + "','" + dr["WCFBINDINGCONTRACT"].ToString() + "',";
                sql += @"'" + dr["RECEIVERUSERID"].ToString() + "','" + dr["RECEIVEROLE"].ToString() + "','" + dr["RECEIVERNAME"].ToString() + "','" + dr["MESSAGEBODY"].ToString() + "',";
                sql += @"'" + dr["MESSAGEURL"].ToString() + "','" + dr["TRIGGERSTATUS"].ToString() + "','" + dr["TRIGGERTYPE"].ToString() + "','" + dr["TRIGGERDESCRIPTION"].ToString() + "',";
                sql += @"'" + dr["CONTRACTTYPE"].ToString() + "','" + dr["CREATEUSERNAME"].ToString() + "','" + remark + "')";
                dao.Open();
                return dao.ExecuteNonQuery(sql) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "AddTimingRecord()", "添加定时触发发送记录", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
    }
}
