using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using SMT.FLOWDAL.ADO;
using SMT.Workflow.Common.DataAccess;
using System.Data;

namespace SMT.FlowDAL.ADO
{
    /// <summary>
    /// 提供对外接口，让外部操作流程的数据，内部不用
    /// </summary>
    public class OutInterface
    {
        #region 获取元数据
        /// <summary>
        /// 获取元数据
        /// </summary>
        /// <param name="formid">formid</param>
        /// <returns></returns>
        public static string GetMetadataByFormid(string formid)
        {
            try
            {
                string sql = "select businessobject from FLOW_FLOWRECORDMASTER_T where formid='" + formid + "' order by createdate desc ";
                using (OracleConnection con = new OracleConnection(ADOHelper.ContextOracleConnection))
                {
                    try
                    {
                        con.Open();
                       DataTable  dt= OracleDataProvider.GetDataTable(con, sql);
                       if (dt != null && dt.Rows.Count > 0)
                       {
                           return dt.Rows[0]["businessobject"].ToString();//取新新的一条
                       }
                       else
                       {
                           return "";
                       }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("获取元数据:GetMetadataByFormid-> OracleDataProvider.GetDataTable:异常信息：" + ex.Message);
                        return "";
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }
                        LogHelper.WriteLog("获取元数据:GetMetadataByFormid-> SQL=" + sql);

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取元数据:GetMetadataByFormid:异常信息：" + ex.Message);
                return "";
            }
        }
        #endregion
        #region 更新元数据
       /// <summary>
        /// 更新元数据
       /// </summary>
        /// <param name="formid">formid</param>
       /// <param name="xml"></param>
       /// <returns></returns>
        public static bool UpdateMetadataByFormid(string formid, string xml)
        {
            try
            {
                string sql = "UPDATE FLOW_FLOWRECORDMASTER_T set BUSINESSOBJECT=:BUSINESSOBJECT where FORMID=:FORMID ";
                string sql2 = "UPDATE T_WF_DOTASK set APPXML=:APPXML where ORDERID=:FORMID ";
                using (OracleConnection con = new OracleConnection(ADOHelper.ContextOracleConnection))
                {
                    try
                    {
                        con.Open();
                        MsOracle.BeginTransaction(con);
                        #region 审核主表
                        OracleParameter[] pageparm =
                        { 
                            new OracleParameter(":FORMID",OracleType.NVarChar), 
                            new OracleParameter(":BUSINESSOBJECT",OracleType.Clob)                   

                        };
                        pageparm[0].Value = MsOracle.GetValue(formid);//
                        pageparm[1].Value = MsOracle.GetValue(xml);//
                        int n = MsOracle.ExecuteSQLByTransaction(con, sql, pageparm);
                        LogHelper.WriteLog("时间：" + DateTime.Now.ToString() + "UpdateMetadataByFormid：【审核主表FLOW_FLOWRECORDMASTER_T】[更新元数据]成功 影响记录数：" + n + ";formid＝" + formid + ";xml=" + xml);
                        #endregion
                        #region 待办任务
                        OracleParameter[] pageparm2 =
                        { 
                            new OracleParameter(":FORMID",OracleType.NVarChar), 
                            new OracleParameter(":APPXML",OracleType.Clob)                   

                        };
                        pageparm2[0].Value = MsOracle.GetValue(formid);//
                        pageparm2[1].Value = MsOracle.GetValue(xml);//
                        int n2 = MsOracle.ExecuteSQLByTransaction(con, sql2, pageparm2);
                        LogHelper.WriteLog("时间：" + DateTime.Now.ToString() + "UpdateMetadataByFormid：【待办任务T_WF_DOTASK】[更新元数据]成功 影响记录数：" + n2 + ";formid＝" + formid + ";xml=" + xml);

                        #endregion
                        MsOracle.CommitTransaction(con);
                        if ((n+n2) > 0)
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
                        LogHelper.WriteLog("更新元数据 UpdateMetadataByFormid 异常信息：" + ex.Message);
                        return false;
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }
                        LogHelper.WriteLog("更新元数据:UpdateMetadataByFormid-> SQL=" + sql);

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("更新元数据:UpdateMetadataByFormid:异常信息：" + ex.Message);
                return false;
            }
        }
        #endregion
    }
}
