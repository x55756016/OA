/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：SendMailRTXDAL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/23 15:55:28   
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
    /// 数据处理层
    /// </summary>
    public class SendMailRTXDAL : BaseDAL
    {

        /// <summary>
        /// 获取待办任务数据
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetDoTaskList()
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "SELECT DOTASKID,RECEIVEUSERID,SYSTEMCODE,MESSAGEBODY,CREATEDATETIME,DOTASKSTATUS,MAILSTATUS,RTXSTATUS FROM T_WF_DOTASK WHERE  DOTASKSTATUS=0 AND MAILSTATUS =0 ";
                dao.Open();
                dt = dao.GetDataTable(sql);
                return dt;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "GetDoTaskList()", "获取待办任务数据", ex);
                return null;
            }
            finally
            {
                dt.Dispose();
                dao.Close();
            }
        }

        /// <summary>
        /// 修改待办任务状态
        /// </summary>
        /// <param name="doTaskID">doTaskID</param>
        /// <returns>bool</returns>
        public bool UpDateDoTaskStatus(string doTaskID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASK SET  MAILSTATUS =1 , RTXSTATUS=1 WHERE DOTASKID='" + doTaskID + "'";
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "UpDateDoTaskStatus()", "修改待办任务状态", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }

        /// <summary>
        /// 获取待办消息列表
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetDoTaskMessageList()
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "SELECT DOTASKMESSAGEID,RECEIVEUSERID,SYSTEMCODE,MESSAGEBODY,CREATEDATETIME,MESSAGESTATUS,MAILSTATUS,RTXSTATUS  FROM T_WF_DOTASKMESSAGE WHERE MESSAGESTATUS=0 AND MAILSTATUS =0";
                dao.Open();
                dt = dao.GetDataTable(sql);
                return dt;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "GetDoTaskMessageList()", "获取待办消息列表", ex);
                return null;
            }
            finally
            {
                dt.Dispose();
                dao.Close();
            }
        }
        /// <summary>
        /// 修改待办消息状态
        /// </summary>
        /// <param name="taskMessageID">ID</param>
        /// <returns>bool</returns>
        public bool UpDateDoTaskMessageStatus(string taskMessageID)
        {
            try
            {
                string sql = "UPDATE T_WF_DOTASKMESSAGE SET MAILSTATUS =1 , RTXSTATUS=1 WHERE DOTASKMESSAGEID='" + taskMessageID + "'";
                dao.Open();
                int result = dao.ExecuteNonQuery(sql);
                return result > -1 ? true : false;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "UpDateDoTaskMessageStatus()", "修改待办消息状态", ex);
                return false;
            }
            finally
            {
                dao.Close();
            }
        }
    }
}
