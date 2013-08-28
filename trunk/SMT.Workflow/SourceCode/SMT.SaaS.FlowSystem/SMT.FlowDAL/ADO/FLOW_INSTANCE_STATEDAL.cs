/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2012     
	 * 文件名：FLOW_INSTANCE_STATEDAL.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2012/9/10 16:31:33   
	 * NET版本： 4.0.30319.225 
	 * 命名空间：SMT.Workflow.Monitoring.DAL 
	 * 模块名称：流程监控
	 * 描　　述： 流程审核过程中的持久化实例
	 * 修改人员：
	 * 修改日期：
	 * 修改内容：
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using SMT.FlowDAL;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.DataAccess;
using System.Data;

namespace SMT.FLOWDAL.ADO
{
    /// <summary>
    /// [流程审核过程中的持久化实例]
    /// </summary>
   public  class FLOW_INSTANCE_STATEDAL:BaseDAL
    {
      /// <summary>
        /// 得到审核主表最新的一个实体
      /// </summary>
        /// <param name="conn">OracleConnection</param>
        /// <param name="formid">formid</param>
      /// <returns></returns>
       public FLOW_FLOWRECORDMASTER_T GetFlowerMasterIDByFormid(OracleConnection conn, string formid)
       {
           try
           {
               FLOW_FLOWRECORDMASTER_T model = new FLOW_FLOWRECORDMASTER_T();
               string selSql = "SELECT INSTANCEID,FORMID,EDITUSERID,EDITUSERNAME FROM FLOW_FLOWRECORDMASTER_T WHERE   FORMID=:FORMID   ORDER BY CREATEDATE DESC";
               OracleParameter[] pageparm =
                {               
                    new OracleParameter(":FORMID",OracleType.NVarChar,100) 

                };
               pageparm[0].Value = formid;
               DataTable dt = OracleDataProvider.GetDataTable(conn, selSql, pageparm);
               if (dt.Rows.Count > 0)
               {//多次提交单据的时候，取最新的一条数据
                   model.INSTANCEID = dt.Rows[0]["INSTANCEID"].ToString();// 
                   model.FORMID = dt.Rows[0]["FORMID"].ToString();// 
                   model.EDITUSERID = dt.Rows[0]["EDITUSERID"].ToString();// 
                   model.EDITUSERNAME = dt.Rows[0]["EDITUSERNAME"].ToString();// 

               }
               return model;
           }
           catch (Exception e)
           {
               throw new Exception(e.Message, e);
           }

        }       
       /// <summary>
        /// 增加一条数据(以实体传值)
       /// </summary>
        /// <param name="conn">OracleConnection</param>
        /// <param name="model">FLOW_INSTANCE_STATE</param>
       /// <returns></returns>
       public int Add(OracleConnection conn, FLOW_INSTANCE_STATE model)
        {
            string insSql = "INSERT INTO FLOW_INSTANCE_STATE (INSTANCE_ID,STATE,STATUS,UNLOCKED,BLOCKED,INFO,MODIFIED,OWNER_ID,OWNED_UNTIL,NEXT_TIMER,FORMID,CREATEID,CREATENAME,EDITID,EDITNAME) VALUES (:INSTANCE_ID,:STATE,:STATUS,:UNLOCKED,:BLOCKED,:INFO,:MODIFIED,:OWNER_ID,:OWNED_UNTIL,:NEXT_TIMER,:FORMID,:CREATEID,:CREATENAME,:EDITID,:EDITNAME)";
            OracleParameter[] pageparm =
                {               
                    new OracleParameter(":INSTANCE_ID",OracleType.Char,36), 
                    new OracleParameter(":STATE",OracleType.Blob), 
                    new OracleParameter(":STATUS",OracleType.Number,22), 
                    new OracleParameter(":UNLOCKED",OracleType.Number,22), 
                    new OracleParameter(":BLOCKED",OracleType.Number,22), 
                    new OracleParameter(":INFO",OracleType.NClob), 
                    new OracleParameter(":MODIFIED",OracleType.DateTime), 
                    new OracleParameter(":OWNER_ID",OracleType.Char,36), 
                    new OracleParameter(":OWNED_UNTIL",OracleType.DateTime), 
                    new OracleParameter(":NEXT_TIMER",OracleType.DateTime), 
                    new OracleParameter(":FORMID",OracleType.NVarChar,100), 
                    new OracleParameter(":CREATEID",OracleType.NVarChar,100), 
                    new OracleParameter(":CREATENAME",OracleType.NVarChar,100), 
                    new OracleParameter(":EDITID",OracleType.NVarChar,100), 
                    new OracleParameter(":EDITNAME",OracleType.NVarChar,100) 
                  

                };
            pageparm[0].Value = OracleDataProvider.GetValue(model.INSTANCE_ID);//
          pageparm[1].Value = OracleDataProvider.GetValue(model.STATE);//
          pageparm[2].Value = OracleDataProvider.GetValue(model.STATUS);//
          pageparm[3].Value = OracleDataProvider.GetValue(model.UNLOCKED);//
          pageparm[4].Value = OracleDataProvider.GetValue(model.BLOCKED);//
          pageparm[5].Value = OracleDataProvider.GetValue(model.INFO);//
          pageparm[6].Value = OracleDataProvider.GetValue(model.MODIFIED);//
          pageparm[7].Value = OracleDataProvider.GetValue(model.OWNER_ID);//
          pageparm[8].Value = OracleDataProvider.GetValue(model.OWNED_UNTIL);//
          pageparm[9].Value = OracleDataProvider.GetValue(model.NEXT_TIMER);//
          pageparm[10].Value = OracleDataProvider.GetValue(model.FORMID);//
          pageparm[11].Value = OracleDataProvider.GetValue(model.CREATEID);//创建人ID
          pageparm[12].Value = OracleDataProvider.GetValue(model.CREATENAME);//创建人姓名
          pageparm[13].Value = OracleDataProvider.GetValue(model.EDITID);//下一个审核人ID
          pageparm[14].Value = OracleDataProvider.GetValue(model.EDITNAME);//下一个审核人姓名

          return ExecuteSQL(conn,insSql, pageparm);
        }     
       /// <summary>
       /// 得到流程持久化实例一个对象实体
       /// </summary>
       /// <param name="conn">OracleConnection</param>
       /// <param name="instance_id">实例ID</param>
       /// <returns></returns>
       public FLOW_INSTANCE_STATE GetInstanceModel(OracleConnection conn, string instance_id)
       {
           FLOW_INSTANCE_STATE model = new FLOW_INSTANCE_STATE();
           string selSql = "SELECT INSTANCE_ID,STATE,STATUS,UNLOCKED,BLOCKED,INFO,MODIFIED,OWNER_ID,OWNED_UNTIL,NEXT_TIMER FROM INSTANCE_STATE WHERE   INSTANCE_ID=:INSTANCE_ID";
           OracleParameter[] pageparm =
                {               
                    new OracleParameter(":INSTANCE_ID",OracleType.Char,36) 

                };
           pageparm[0].Value = instance_id;
           OracleDataReader dr = OracleDataProvider.ExecuteReader(conn, selSql, pageparm);
           if (dr.Read())
           {
               model.INSTANCE_ID = dr["INSTANCE_ID"].ToString();// 
               model.STATE = (byte[])dr["STATE"];//   
               model.STATUS = dr["STATUS"].ToString() != "" ? Convert.ToDecimal(dr["STATUS"]) : 0; //  
               model.UNLOCKED = dr["UNLOCKED"].ToString() != "" ? Convert.ToDecimal(dr["UNLOCKED"]) : 0; //  
               model.BLOCKED = dr["BLOCKED"].ToString() != "" ? Convert.ToDecimal(dr["BLOCKED"]) : 0; //  
               model.INFO = dr["INFO"].ToString();// 
               model.MODIFIED = dr["MODIFIED"].ToString() != "" ? Convert.ToDateTime(dr["MODIFIED"]) : DateTime.Now; //  
               model.OWNER_ID = dr["OWNER_ID"].ToString();// 
               model.OWNED_UNTIL = dr["OWNED_UNTIL"].ToString() != "" ? Convert.ToDateTime(dr["OWNED_UNTIL"]) : DateTime.Now; //  
               model.NEXT_TIMER = dr["NEXT_TIMER"].ToString() != "" ? Convert.ToDateTime(dr["NEXT_TIMER"]) : DateTime.Now; //  
               
           }
           dr.Close();
           conn.Close();
           return model;
       }
    }
}
