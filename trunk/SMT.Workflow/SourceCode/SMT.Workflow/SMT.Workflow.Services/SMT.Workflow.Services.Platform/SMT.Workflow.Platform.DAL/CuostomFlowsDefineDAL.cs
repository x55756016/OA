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
    public class CuostomFlowsDefineDAL:BaseDAL
    {
        #region 自动发起流程增、删、改、查方法
        //自动发起流程查询
        public IQueryable<T_FLOW_CUSTOMFLOWDEFINE> GetCuostomFlowsDefine(string filterString)
        {
            try
            {
                string sql = string.Empty;
                if (filterString != null && filterString != "")
                {
                    sql = "SELECT * FROM T_FLOW_CUSTOMFLOWDEFINE where " + filterString;
                }
                else
                {
                    sql = "SELECT * FROM T_FLOW_CUSTOMFLOWDEFINE";
                }
                dao.Open();
                DataTable dtTriggerDefine = dao.GetDataTable(sql);
                dao.Close();
                var items = ToList<T_FLOW_CUSTOMFLOWDEFINE>(dtTriggerDefine).AsQueryable();
                return items;
            }
            catch 
            {
                return null;
            }
        }
        //自动发起流程查询
        public IQueryable<T_FLOW_CUSTOMFLOWDEFINE> GetListCuostomFlowsDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            try
            {
                string sql = string.Empty;
                if (filterString != null && filterString != "")
                {
                    sql = "SELECT * FROM T_FLOW_CUSTOMFLOWDEFINE where " + filterString;
                }
                else
                {
                    sql = "SELECT * FROM T_FLOW_CUSTOMFLOWDEFINE";
                }
                dao.Open();
                DataTable dtTriggerDefine = dao.GetDataTable(sql);
                dao.Close();
                var items = ToList<T_FLOW_CUSTOMFLOWDEFINE>(dtTriggerDefine).AsQueryable();
                items = Pager<T_FLOW_CUSTOMFLOWDEFINE>(items, pageIndex, pageSize, ref pageCount);
                return items;
            }
            catch
            {
                return null;
            }
        }

        //添加自动发起流程
        public bool AddCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            try
            {
                int result = 0;
                dao.Open();
                dao.BeginTransaction();
                foreach (var CuostomFlow in ListCuostomFlowsDefine)
                {
                   string sql = "insert into T_FLOW_CUSTOMFLOWDEFINE(PROCESSID, COMPANYCODE, SYSTEMCODE, SYSTEMNAME,MODELCODE,MODELNAME,FUNCTIONNAME,FUNCTIONDES,"
                                                               + " PROCESSWCFURL,PROCESSFUNCNAME,PROCESSFUNCPAMETER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,MESSAGEBODY,MSGLINKURL,RECEIVEUSER,RECEIVEUSERNAME,"
                                                               + " OWNERCOMPANYID,OWNERDEPARTMENTID,OWNERPOSTID,CREATEDATE,CREATETIME,CREATEUSERNAME,CREATEUSERID)values("
                                                               + " '" + CuostomFlow.PROCESSID + "','" + CuostomFlow.COMPANYCODE + "','" + CuostomFlow.SYSTEMCODE + "',"
                                                               + " '" + CuostomFlow.SYSTEMNAME + "','" + CuostomFlow.MODELCODE + "','" + CuostomFlow.MODELNAME + "',"
                                                               + " '" + CuostomFlow.FUNCTIONNAME + "','" + CuostomFlow.FUNCTIONDES + "','" + CuostomFlow.PROCESSWCFURL + "',"
                                                               + " '" + CuostomFlow.PROCESSFUNCNAME + "','" + CuostomFlow.PROCESSFUNCPAMETER + "','" + CuostomFlow.PAMETERSPLITCHAR + "',"
                                                               + " '" + CuostomFlow.WCFBINDINGCONTRACT + "','" + CuostomFlow.MESSAGEBODY + "','" + CuostomFlow.MSGLINKURL + "',"
                                                               + " '" + CuostomFlow.RECEIVEUSER + "','" + CuostomFlow.RECEIVEUSERNAME + "','" + CuostomFlow.OWNERCOMPANYID + "',"
                                                               + " '" + CuostomFlow.OWNERDEPARTMENTID + "','" + CuostomFlow.OWNERPOSTID + "','" + DateTime.Now.ToString("yyyy/mm/dd") + "',"
                                                               + " '" + DateTime.Now.ToString("HH:mm:ss") + "','" + CuostomFlow.CREATEUSERNAME + "','" + CuostomFlow.CREATEUSERID + "')";
                    result = dao.ExecuteNonQuery(sql);
                }
                if (result > 0)
                {
                    dao.CommitTransaction();
                    return true;
                }
                else
                {
                    dao.RollbackTransaction();
                    return false;
                }
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
        //修改自动发起流程
        public bool UpdateCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            try
            {
                List<T_FLOW_CUSTOMFLOWDEFINE> ListcuostomFlows = new List<T_FLOW_CUSTOMFLOWDEFINE>();
                foreach (var items in ListCuostomFlowsDefine)
                {
                    string sql = "select * from T_FLOW_CUSTOMFLOWDEFINE "
                                                        + "where PROCESSID = '" + items.PROCESSID + "' ";
                    dao.Open();
                    DataTable dtTriggerDefine = dao.GetDataTable(sql);
                    var itemes = ToList<T_FLOW_CUSTOMFLOWDEFINE>(dtTriggerDefine).AsQueryable();
                    if (itemes.ToList().Count() > 0)
                    {
                        ListcuostomFlows.Add(items);
                    }
                }
                int result = 0;
                dao.BeginTransaction();
                foreach (var CuostomFlow in ListCuostomFlowsDefine)
                {
                    if (ListcuostomFlows.Where(p => p.PROCESSID == CuostomFlow.PROCESSID).Count() > 0)
                    {
                        string sql = "update T_FLOW_CUSTOMFLOWDEFINE set "
                                        + " COMPANYCODE = '" + CuostomFlow.COMPANYCODE + "', "
                                        + " SYSTEMNAME = '" + CuostomFlow.SYSTEMNAME + "', "
                                        + " MODELCODE = '" + CuostomFlow.MODELCODE + "', "
                                        + " MODELNAME = '" + CuostomFlow.MODELNAME + "', "
                                        + " FUNCTIONNAME = '" + CuostomFlow.FUNCTIONNAME + "', "
                                        + " FUNCTIONDES = '" + CuostomFlow.FUNCTIONDES + "', "
                                        + " PROCESSWCFURL = '" + CuostomFlow.PROCESSWCFURL + "', "
                                        + " PROCESSFUNCNAME = '" + CuostomFlow.PROCESSFUNCNAME + "', "
                                        + " PROCESSFUNCPAMETER = '" + CuostomFlow.PROCESSFUNCPAMETER + "', "
                                        + " PAMETERSPLITCHAR = '" + CuostomFlow.PAMETERSPLITCHAR + "', "
                                        + " WCFBINDINGCONTRACT = '" + CuostomFlow.WCFBINDINGCONTRACT + "', "
                                        + " MESSAGEBODY = '" + CuostomFlow.MESSAGEBODY + "', "
                                        + " MSGLINKURL = '" + CuostomFlow.MSGLINKURL + "', "
                                        + " RECEIVEUSER = '" + CuostomFlow.RECEIVEUSER + "', "
                                        + " RECEIVEUSERNAME = '" + CuostomFlow.RECEIVEUSERNAME + "', "
                                        + " OWNERCOMPANYID = '" + CuostomFlow.OWNERCOMPANYID + "', "
                                        + " OWNERDEPARTMENTID = '" + CuostomFlow.OWNERDEPARTMENTID + "', "
                                        + " OWNERPOSTID = '" + CuostomFlow.OWNERPOSTID + "', "
                                        + " CREATEDATE = '" + CuostomFlow.CREATEDATE + "', "
                                        + " CREATETIME = '" + CuostomFlow.CREATETIME + "', "
                                        + " CREATEUSERNAME = '" + CuostomFlow.CREATEUSERNAME + "', "
                                        + " CREATEUSERID = '" + CuostomFlow.CREATEUSERID + "' "
                                        + "where PROCESSID = '" + CuostomFlow.PROCESSID + "' ";
                        result = dao.ExecuteNonQuery(sql);
                        if (result < 1)
                        {
                            dao.CommitTransaction();
                            return false;
                        }
                    }
                }
                if (result > 0)
                {
                    dao.CommitTransaction();
                    return true;
                }
                else
                {
                    dao.RollbackTransaction();
                    return false;
                }
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
        //删除自动发起流程
        public bool DeleteCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            try
            {
                int result = 0;
                dao.Open();
                dao.BeginTransaction();

                foreach (var item in ListCuostomFlowsDefine)
                {
                    string strSql = "delete from T_FLOW_CUSTOMFLOWDEFINE "
                                                        + "where PROCESSID = '" + item.PROCESSID + "' ";
                    result = dao.ExecuteNonQuery(strSql);
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
