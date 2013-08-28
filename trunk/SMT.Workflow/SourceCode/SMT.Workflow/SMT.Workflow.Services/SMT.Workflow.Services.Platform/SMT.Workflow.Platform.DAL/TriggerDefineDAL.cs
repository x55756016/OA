using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using System.Data;
using System.Linq.Dynamic;
using SMT.Workflow.Common.Model.FlowEngine;
namespace SMT.Workflow.Platform.DAL
{
    public class TriggerDefineDAL : BaseDAL
    {
        #region 定时触发增、删、改、查方法
        //定时触发查询
        public IQueryable<T_WF_TIMINGTRIGGERCONFIG> GetFlowTriggerDefine(string filterString)
        {
            try
            {
                string sql = string.Empty;
                if (filterString != null && filterString != "")
                {
                    sql = "SELECT * FROM T_WF_TIMINGTRIGGERCONFIG where " + filterString;
                }
                else
                {
                    sql = "SELECT * FROM T_WF_TIMINGTRIGGERCONFIG";
                }
                dao.Open();
                DataTable dtTriggerDefine = dao.GetDataTable(sql);
                dao.Close();
                var items = ToList<T_WF_TIMINGTRIGGERCONFIG>(dtTriggerDefine).AsQueryable();
                return items;
            }
            catch 
            {
                return null;
            }
        }
        //定时触发查询
        public IQueryable<T_WF_TIMINGTRIGGERCONFIG> GetListFlowTriggerDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            try
            {
                string sql = string.Empty;
                if (filterString != null && filterString != "")
                {
                    sql = "SELECT * FROM T_WF_TIMINGTRIGGERCONFIG where " + filterString;
                }
                else
                {
                    sql = "SELECT * FROM T_WF_TIMINGTRIGGERCONFIG";
                }
                dao.Open();
                DataTable dtTriggerDefine = dao.GetDataTable(sql);
                dao.Close();
                var items = ToList<T_WF_TIMINGTRIGGERCONFIG>(dtTriggerDefine).AsQueryable();
                items = Pager<T_WF_TIMINGTRIGGERCONFIG>(items, pageIndex, pageSize, ref pageCount);
                return items;
            }
            catch 
            {
                return null;
            }
        }
        ////添加定时触发
        //public bool AddTriggerDefine(List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger)
        //{
        //    try
        //    {
        //        int result = 0;
        //        dao.Open();
        //        dao.BeginTransaction();
        //        foreach (var FlowTrigger in ListFlowTrigger)
        //        {
                    
        //            string sql = "insert into T_FLOW_TIMINGTRIGGERDEFINE(PROCESSID, COMPANYCODE, SYSTEMCODE, APPLICATIONORDERCODE, PROCESSSTARTDATE, PROCESSSTARTTIME, PROCESSCYCLE,RECEIVER,RECEIVEROLE,MESSAGEBODY,MSGLINKURL,"
        //                                                       + " PROCESSWCFURL,PROCESSFUNCNAME,PROCESSFUNCPAMETER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,DATASTATUS,CREATEDATE,CREATETIME,UPDATEDATE,UPDATETIME,FUNCTIONMARK, "
        //                                                       + " MODELCODE,TRIGGERTYPE,TRIGGERDESCRIPTION,SYSTEMNAME,MODELNAME,TRIGGERNAME,RECEIVERNAME,CONTRACTTYPE,PROCESSNUM)values("
        //                                                       + " '" + FlowTrigger.PROCESSID + "','" + FlowTrigger.COMPANYCODE + "','" + FlowTrigger.SYSTEMCODE + "',"
        //                                                       + " '" + FlowTrigger.APPLICATIONORDERCODE + "','" + FlowTrigger.PROCESSSTARTDATE + "','" + FlowTrigger.PROCESSSTARTTIME + "',"
        //                                                       + " '" + FlowTrigger.PROCESSCYCLE + "','" + FlowTrigger.RECEIVER + "','" + FlowTrigger.RECEIVEROLE + "',"
        //                                                       + " '" + FlowTrigger.MESSAGEBODY + "','" + FlowTrigger.MSGLINKURL + "','" + FlowTrigger.PROCESSWCFURL + "',"
        //                                                       + " '" + FlowTrigger.PROCESSFUNCNAME + "','" + FlowTrigger.PROCESSFUNCPAMETER + "','" + FlowTrigger.PAMETERSPLITCHAR + "',"
        //                                                       + " '" + FlowTrigger.WCFBINDINGCONTRACT + "','" + FlowTrigger.DATASTATUS + "','" + FlowTrigger.CREATEDATE + "',"
        //                                                       + " '" + FlowTrigger.CREATETIME + "','" + FlowTrigger.UPDATEDATE + "','" + FlowTrigger.UPDATETIME + "',"
        //                                                       + " '" + FlowTrigger.FUNCTIONMARK + "','" + FlowTrigger.MODELCODE + "','" + FlowTrigger.TRIGGERTYPE + "',"
        //                                                       + " '" + FlowTrigger.TRIGGERDESCRIPTION + "','" + FlowTrigger.SYSTEMNAME + "','" + FlowTrigger.MODELNAME + "',"
        //                                                       + " '" + FlowTrigger.TRIGGERNAME + "','" + FlowTrigger.RECEIVERNAME + "','" + FlowTrigger.CONTRACTTYPE + "',"
        //                                                       + " '" + FlowTrigger.PROCESSNUM + "')";
        //            result = dao.ExecuteNonQuery(sql);
        //            if (result > 0)
        //            {
        //                string sqlstr = "insert into T_WF_TIMINGTRIGGERCONFIG(PROCESSID, COMPANYCODE, SYSTEMCODE, APPLICATIONORDERCODE, PROCESSSTARTDATE, PROCESSSTARTTIME, PROCESSCYCLE,RECEIVEUSER,RECEIVEROLE,MESSAGEBODY,MSGLINKURL,"
        //                                                       + " PROCESSWCFURL,PROCESSFUNCNAME,PROCESSFUNCPAMETER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,DATASTATUS,CREATEDATE,CREATETIME,UPDATEDATE,UPDATETIME,FUNCTIONMARK, "
        //                                                       + " MODELCODE,TRIGGERTYPE,CONTRACTTYPE)values("
        //                                                       + " SQPROCESSID.NEXTVAL,'" + FlowTrigger.COMPANYCODE + "','" + FlowTrigger.SYSTEMCODE + "',"
        //                                                       + " '" + FlowTrigger.PROCESSID + "','" + FlowTrigger.PROCESSSTARTDATE + "','" + FlowTrigger.PROCESSSTARTTIME + "',"
        //                                                       + " '" + FlowTrigger.PROCESSCYCLE + "','" + FlowTrigger.RECEIVER + "','" + FlowTrigger.RECEIVEROLE + "',"
        //                                                       + " '" + FlowTrigger.MESSAGEBODY + "','" + FlowTrigger.MSGLINKURL + "','" + FlowTrigger.PROCESSWCFURL + "',"
        //                                                       + " '" + FlowTrigger.PROCESSFUNCNAME + "','" + FlowTrigger.PROCESSFUNCPAMETER + "','" + FlowTrigger.PAMETERSPLITCHAR + "',"
        //                                                       + " '" + FlowTrigger.WCFBINDINGCONTRACT + "','" + FlowTrigger.DATASTATUS + "','" + FlowTrigger.CREATEDATE + "',"
        //                                                       + " '" + FlowTrigger.CREATETIME + "','" + FlowTrigger.UPDATEDATE + "','" + FlowTrigger.UPDATETIME + "',"
        //                                                       + " '" + FlowTrigger.FUNCTIONMARK + "','" + FlowTrigger.MODELCODE + "','" + FlowTrigger.TRIGGERTYPE + "',"
        //                                                       + " '" + FlowTrigger.CONTRACTTYPE + "')";
        //                result = dao.ExecuteNonQuery(sqlstr);
        //            }
        //        }
        //        if (result > 0)
        //        {
        //            dao.CommitTransaction();
        //            return true ;
        //        }
        //        else 
        //        {
        //            dao.RollbackTransaction();
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        dao.RollbackTransaction();
        //        return false;
        //    }
        //    finally
        //    {
        //        dao.Close();
        //    }
        //}
        //删除定时触发
        public bool DeleteTriggerDefine(List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger)
        {
            try
            {
                int result = 0;
                dao.Open();
                dao.BeginTransaction();

                foreach (var item in ListFlowTrigger)
                {
                    string Sql = "delete from T_WF_TIMINGTRIGGERCONFIG "
                                    + "where TIMINGCONFIGID = '" + item.TIMINGCONFIGID + "' ";
                    result = dao.ExecuteNonQuery(Sql);
                    if (result > 0)
                    {
                        string strSql = "delete from T_WF_TIMINGTRIGGERACTIVITY "
                                                        + "where TIMINGCONFIGID = '" + item.TIMINGCONFIGID + "' ";
                        result = dao.ExecuteNonQuery(strSql);
                    }
                   
                }
                dao.CommitTransaction();
                return true;
            
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
