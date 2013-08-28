using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using System.Data;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Common.Model.Views;

namespace SMT.Workflow.Platform.DAL
{
    public class FlowEventDAL : BaseDAL
    {
        //引擎添加、修改方法
        public string FlowEventAll(T_WF_MESSAGEBODYDEFINE FlowMsg, List<T_WF_DOTASKRULE> ListFlowProecss, List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine, List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger)
        {
            try
            {
                int result = 0;
                string DelectFlowMsg = string.Empty;//
                List<T_WF_DOTASKRULEDETAIL> DelectProecss = new List<T_WF_DOTASKRULEDETAIL>();//
                List<T_FLOW_CUSTOMFLOWDEFINE> Delectcustom = new List<T_FLOW_CUSTOMFLOWDEFINE>();//
                List<T_WF_TIMINGTRIGGERCONFIG> Delecttrigger = new List<T_WF_TIMINGTRIGGERCONFIG>();//
                List<T_WF_DOTASKRULEDETAIL> NotDelectProecss = new List<T_WF_DOTASKRULEDETAIL>();//
                List<T_WF_DOTASKRULE> DotaskRuid = new List<T_WF_DOTASKRULE>();//
                List<T_WF_DOTASKRULE> DotaskRuids = new List<T_WF_DOTASKRULE>();//
                #region 查询数据
                //查询所有存在的数据
                if (FlowMsg != null) //消息定义
                {
                    string sql = "select * from T_WF_MESSAGEBODYDEFINE "
                                                        + "where DEFINEID = '" + FlowMsg.DEFINEID + "' ";
                    dao.Open();
                    DataTable dtFlowMsg = dao.GetDataTable(sql);
                    var itemes = ToList<T_WF_MESSAGEBODYDEFINE>(dtFlowMsg).AsQueryable();
                    if (itemes.ToList().Count() > 0)
                    {
                        DelectFlowMsg = itemes.FirstOrDefault().DEFINEID;
                    }
                }
                if (ListFlowProecss != null) // 默认规则
                {
                    DotaskRuid.Clear();
                   
                    foreach (var items in ListFlowProecss)
                    {
                        string sqlp = "select * from T_WF_DOTASKRULEDETAIL "
                                                             + "where DOTASKRULEDETAILID ='" + items.DOTASKRULEDETAIL.DOTASKRULEDETAILID + "' ";
                        dao.Open();
                        DataTable dtFlowProecss = dao.GetDataTable(sqlp);
                        var ipross = ToList<T_WF_DOTASKRULEDETAIL>(dtFlowProecss).AsQueryable();
                        if (ipross.ToList().Count() > 0)
                        {
                            DelectProecss.Add(items.DOTASKRULEDETAIL);
                        }
                        string sqlcec = "select * from T_WF_DOTASKRULE "
                                                          + "where SYSTEMCODE = '" + items.SYSTEMCODE + "' and MODELCODE = '" + items.MODELCODE + "' and TRIGGERORDERSTATUS = " + items.TRIGGERORDERSTATUS + " and COMPANYID = '" + items.COMPANYID + "'";
                        DataTable dtCuostomFlowsDefinees = dao.GetDataTable(sqlcec);
                        var iCuostomes = ToList<T_WF_DOTASKRULE>(dtCuostomFlowsDefinees);
                        if (iCuostomes.ToList().Count() == 1)
                        {
                            if (ipross.ToList().Count() > 0)
                            {
                                if (DotaskRuid.Where(p => p.DOTASKRULEID == iCuostomes.FirstOrDefault().DOTASKRULEID).Count() == 0)
                                {
                                    DotaskRuid.Add(iCuostomes[0]);
                                }
                            }
                            else if (ipross.ToList().Count() == 0)
                            {
                               DotaskRuids.Add(iCuostomes[0]);
                            }
                        }
                        else if (iCuostomes.ToList().Count() == 0)
                        {
                            NotDelectProecss.Add(items.DOTASKRULEDETAIL);
                        }
                    }
                }
                if (ListCuostomFlowsDefine != null) //自动发起流程
                {
                    foreach (var itemsc in ListCuostomFlowsDefine)
                    {
                        string sqlc = "select * from T_FLOW_CUSTOMFLOWDEFINE "
                                                            + "where PROCESSID = '" + itemsc.PROCESSID + "' ";
                        dao.Open();
                        DataTable dtCuostomFlowsDefine = dao.GetDataTable(sqlc);
                        var iCuostom = ToList<T_FLOW_CUSTOMFLOWDEFINE>(dtCuostomFlowsDefine).AsQueryable();
                        if (iCuostom.ToList().Count() > 0)
                        {
                            Delectcustom.Add(itemsc);
                        }
                    }
                }
                if (ListFlowTrigger != null) //定时触发
                {
                    foreach (var itemst in ListFlowTrigger)
                    {
                        string sqlc = "select * from T_WF_TIMINGTRIGGERCONFIG "
                                                            + "where TIMINGCONFIGID = '" + itemst.TIMINGCONFIGID + "' ";
                        dao.Open();
                        DataTable dtFlowTrigger = dao.GetDataTable(sqlc);
                        var iTrigger = ToList<T_WF_TIMINGTRIGGERCONFIG>(dtFlowTrigger).AsQueryable();
                        if (iTrigger.ToList().Count() > 0)
                        {
                            Delecttrigger.Add(itemst);
                        }
                    }
                }
                #endregion
                #region 删除数据
                //删除数据
                dao.BeginTransaction();
                if (DelectFlowMsg != "")
                {
                    string Sql = "delete from T_WF_MESSAGEBODYDEFINE "
                                    + "where DEFINEID = '" + DelectFlowMsg + "' ";
                    result = dao.ExecuteNonQuery(Sql);
                    if (result == 0) //消息删除
                    {
                        dao.RollbackTransaction();
                        return "消息定义保存失败！";
                    }
                }
                if (DelectProecss.Count() > 0 || DotaskRuid.Count() > 0)
                {
                    foreach (var itempr in DelectProecss) //默认规则
                    {
                        string Sqlpr = "delete from T_WF_DOTASKRULEDETAIL "
                                                            + "where DOTASKRULEID = '" + itempr.DOTASKRULEID + "' ";
                        result = dao.ExecuteNonQuery(Sqlpr);
                        //if (result == 0)
                        //{
                        //    dao.RollbackTransaction();
                        //    return "默认规则保存失败!";
                        //}
                        if (DotaskRuid.Where(p => p.DOTASKRULEID == itempr.DOTASKRULEID).Count() > 0)
                        {
                            string Sqlfl = "delete from T_WF_DOTASKRULE "
                                                                + "where DOTASKRULEID = '" + itempr.DOTASKRULEID + "' ";
                            result = dao.ExecuteNonQuery(Sqlfl);
                            //if (result == 0)
                            //{
                            //    dao.RollbackTransaction();
                            //    return "默认规则保存失败!";
                            //}
                        }
                       
                    }
                }
                if (Delectcustom.Count() > 0)
                {
                    foreach (var itemctm in Delectcustom) //自动发起流程
                    {
                        string Sqlctm = "delete from T_FLOW_CUSTOMFLOWDEFINE "
                                                            + "where PROCESSID = '" + itemctm.PROCESSID + "' ";
                        result = dao.ExecuteNonQuery(Sqlctm);
                        if (result == 0)
                        {
                            dao.RollbackTransaction();
                            return "自动发起流程保存失败!";
                        }
                    }
                }
                if (Delecttrigger.Count() >0)
                {
                    foreach (var itemter in Delecttrigger) //定时触发
                    {
                        string Sqlter = "delete from T_WF_TIMINGTRIGGERCONFIG "
                                        + "where TIMINGCONFIGID = '" + itemter.TIMINGCONFIGID + "' ";
                        result = dao.ExecuteNonQuery(Sqlter);
                        if (result == 0)
                        {
                            dao.RollbackTransaction();
                            return "定时触发定义保存失败!";
                        }
                        string Sqlger = "delete from T_WF_TIMINGTRIGGERACTIVITY "
                                                           + "where TIMINGCONFIGID = '" + itemter.TIMINGCONFIGID + "' ";
                        result = dao.ExecuteNonQuery(Sqlger);
                        //if (result == 0)
                        //{
                        //    dao.RollbackTransaction();
                        //    return "定时触发定义保存失败!";
                        //}
                    }
                }
                #endregion
                #region 添加数据
                if (FlowMsg != null)
                {
                    string SqlAmsg = "insert into T_WF_MESSAGEBODYDEFINE(DEFINEID, SYSTEMCODE, MODELCODE, MESSAGEBODY, MESSAGEURL, CREATEDATE,COMPANYID,CREATEUSERNAME,CREATEUSERID,MESSAGETYPE)"
                                               + "values('" + FlowMsg.DEFINEID + "','" + FlowMsg.SYSTEMCODE + "','" + FlowMsg.MODELCODE + "',"
                                                      + "'" + FlowMsg.MESSAGEBODY + "','" + FlowMsg.MESSAGEURL + "',to_date('" + FlowMsg.CREATEDATE + "','yyyy-mm-dd hh24:mi:ss'),"
                                                      + "'" + FlowMsg.COMPANYID + "','" + FlowMsg.CREATEUSERNAME + "','" + FlowMsg.CREATEUSERID + "','" + FlowMsg.MESSAGETYPE + "')";
                    result = dao.ExecuteNonQuery(SqlAmsg); //消息定义
                    if (result == 0) //消息删除
                    {
                        dao.RollbackTransaction();
                        return "消息定义保存失败！";
                    }
                }
                if (ListFlowProecss != null)
                {
                    List<string> triggerID = new List<string>();
                    foreach (var FlowTrigger in ListFlowProecss) //触发规则
                    {
                        string DotaskRuleID = string.Empty;
                        bool rueslt = false; //判断是否要添加主表
                    
                        if (DotaskRuid.Where(p => p.SYSTEMCODE == FlowTrigger.SYSTEMCODE && p.MODELCODE == FlowTrigger.MODELCODE && p.TRIGGERORDERSTATUS == FlowTrigger.TRIGGERORDERSTATUS).Count() > 0)
                        {
                            rueslt = true;
                            DotaskRuleID = DotaskRuid.Where(p => p.SYSTEMCODE == FlowTrigger.SYSTEMCODE && p.MODELCODE == FlowTrigger.MODELCODE && p.TRIGGERORDERSTATUS == FlowTrigger.TRIGGERORDERSTATUS).FirstOrDefault().DOTASKRULEID;
                        }
                        else if (DotaskRuids.Where(p => p.SYSTEMCODE == FlowTrigger.SYSTEMCODE && p.MODELCODE == FlowTrigger.MODELCODE && p.TRIGGERORDERSTATUS == FlowTrigger.TRIGGERORDERSTATUS).Count() > 0)
                        {
                            rueslt = false;
                            DotaskRuleID = DotaskRuids.Where(p => p.SYSTEMCODE == FlowTrigger.SYSTEMCODE && p.MODELCODE == FlowTrigger.MODELCODE && p.TRIGGERORDERSTATUS == FlowTrigger.TRIGGERORDERSTATUS).FirstOrDefault().DOTASKRULEID;
                        }
                        else 
                        {
                            if (NotDelectProecss.Where(p => p.DOTASKRULEDETAILID == FlowTrigger.DOTASKRULEDETAIL.DOTASKRULEDETAILID).Count() > 0)
                            {
                                rueslt = true;
                                DotaskRuleID = FlowTrigger.DOTASKRULEID;
                            }
                            else
                            {
                                rueslt = false;
                                DotaskRuleID = FlowTrigger.DOTASKRULEID;
                            }
                        }
                        if (rueslt == true)
                        {
                            if (triggerID.Where(p => p == DotaskRuleID).Count() == 0)
                            {
                                triggerID.Add(DotaskRuleID);
                                string sqlAger = "insert into T_WF_DOTASKRULE(DOTASKRULEID ,COMPANYID, SYSTEMCODE,SYSTEMNAME ,MODELCODE ,MODELNAME ,TRIGGERORDERSTATUS ,CREATEDATETIME)values("
                                                                            + " '" + DotaskRuleID + "','" + FlowTrigger.COMPANYID + "',"
                                                                            + " '" + FlowTrigger.SYSTEMCODE + "','" + FlowTrigger.SYSTEMNAME + "','" + FlowTrigger.MODELCODE + "',"
                                                                            + " '" + FlowTrigger.MODELNAME + "','" + FlowTrigger.TRIGGERORDERSTATUS + "',to_date('" + FlowTrigger.CREATEDATETIME + "','yyyy-mm-dd hh24:mi:ss'))";
                                result = dao.ExecuteNonQuery(sqlAger);
                                if (result == 0)
                                {
                                    dao.RollbackTransaction();
                                    return "默认规则保存失败!";
                                }
                            }
                        }
                        string sqlAter = "insert into T_WF_DOTASKRULEDETAIL(DOTASKRULEDETAILID, DOTASKRULEID , COMPANYID , SYSTEMCODE,SYSTEMNAME ,MODELCODE ,MODELNAME ,WCFURL ,FUNCTIONNAME ,"
                                                                        + " FUNCTIONPARAMTER ,PAMETERSPLITCHAR ,WCFBINDINGCONTRACT ,MESSAGEBODY ,LASTDAYS ,CREATEDATETIME ,"
                                                                        + " APPLICATIONURL ,RECEIVEUSER ,RECEIVEUSERNAME ,OWNERCOMPANYID ,OWNERDEPARTMENTID ,OWNERPOSTID ,ISDEFAULTMSG ,"
                                                                        + " PROCESSFUNCLANGUAGE ,ISOTHERSOURCE ,OTHERSYSTEMCODE ,OTHERMODELCODE,REMARK,CREATEUSERID,CREATEUSERNAME,TRIGGERORDERSTATUS)values("
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.DOTASKRULEDETAILID + "','" + DotaskRuleID + "','" + FlowTrigger.DOTASKRULEDETAIL.COMPANYID + "',"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.SYSTEMCODE + "','" + FlowTrigger.DOTASKRULEDETAIL.SYSTEMNAME + "','" + FlowTrigger.DOTASKRULEDETAIL.MODELCODE + "',"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.MODELNAME + "','" + FlowTrigger.DOTASKRULEDETAIL.WCFURL + "','" + FlowTrigger.DOTASKRULEDETAIL.FUNCTIONNAME + "',"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.FUNCTIONPARAMTER + "','" + FlowTrigger.DOTASKRULEDETAIL.PAMETERSPLITCHAR + "','" + FlowTrigger.DOTASKRULEDETAIL.WCFBINDINGCONTRACT + "',"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.MESSAGEBODY + "','" + FlowTrigger.DOTASKRULEDETAIL.LASTDAYS + "',to_date('" + FlowTrigger.DOTASKRULEDETAIL.CREATEDATETIME + "','yyyy-mm-dd hh24:mi:ss'),"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.APPLICATIONURL + "','" + FlowTrigger.DOTASKRULEDETAIL.RECEIVEUSER + "',"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.RECEIVEUSERNAME + "','" + FlowTrigger.DOTASKRULEDETAIL.OWNERCOMPANYID + "','" + FlowTrigger.DOTASKRULEDETAIL.OWNERDEPARTMENTID + "',"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.OWNERPOSTID + "','" + FlowTrigger.DOTASKRULEDETAIL.ISDEFAULTMSG + "',"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.PROCESSFUNCLANGUAGE + "','" + FlowTrigger.DOTASKRULEDETAIL.ISOTHERSOURCE + "',"
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.OTHERSYSTEMCODE + "','" + FlowTrigger.DOTASKRULEDETAIL.OTHERMODELCODE + "','" + FlowTrigger.DOTASKRULEDETAIL.REMARK + "', "
                                                                        + " '" + FlowTrigger.DOTASKRULEDETAIL.CREATEUSERID + "','" + FlowTrigger.DOTASKRULEDETAIL.CREATEUSERNAME + "','" + FlowTrigger.TRIGGERORDERSTATUS + "')";
                        result = dao.ExecuteNonQuery(sqlAter);
                        if (result == 0)
                        {
                            dao.RollbackTransaction();
                            return "默认规则保存失败!";
                        }
                      
                    }
                }
                if (ListCuostomFlowsDefine != null)
                {
                    foreach (var CuostomFlow in ListCuostomFlowsDefine) //自动发起流程
                    {
                        string sqlAcow = "insert into T_FLOW_CUSTOMFLOWDEFINE(PROCESSID, COMPANYCODE, SYSTEMCODE, SYSTEMNAME,MODELCODE,MODELNAME,FUNCTIONNAME,FUNCTIONDES,"
                                                                    + " PROCESSWCFURL,PROCESSFUNCNAME,PROCESSFUNCPAMETER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,MESSAGEBODY,MSGLINKURL,RECEIVEUSER,RECEIVEUSERNAME,"
                                                                    + " OWNERCOMPANYID,OWNERDEPARTMENTID,OWNERPOSTID,CREATEDATE,CREATETIME,CREATEUSERNAME,CREATEUSERID)values("
                                                                    + " '" + CuostomFlow.PROCESSID + "','" + CuostomFlow.COMPANYCODE + "','" + CuostomFlow.SYSTEMCODE + "',"
                                                                    + " '" + CuostomFlow.SYSTEMNAME + "','" + CuostomFlow.MODELCODE + "','" + CuostomFlow.MODELNAME + "',"
                                                                    + " '" + CuostomFlow.FUNCTIONNAME + "','" + CuostomFlow.FUNCTIONDES + "','" + CuostomFlow.PROCESSWCFURL + "',"
                                                                    + " '" + CuostomFlow.PROCESSFUNCNAME + "','" + CuostomFlow.PROCESSFUNCPAMETER + "','" + CuostomFlow.PAMETERSPLITCHAR + "',"
                                                                    + " '" + CuostomFlow.WCFBINDINGCONTRACT + "','" + CuostomFlow.MESSAGEBODY + "','" + CuostomFlow.MSGLINKURL + "',"
                                                                    + " '" + CuostomFlow.RECEIVEUSER + "','" + CuostomFlow.RECEIVEUSERNAME + "','" + CuostomFlow.OWNERCOMPANYID + "',"
                                                                    + " '" + CuostomFlow.OWNERDEPARTMENTID + "','" + CuostomFlow.OWNERPOSTID + "','" + CuostomFlow.CREATEDATE + "',"
                                                                    + " '" + CuostomFlow.CREATETIME + "','" + CuostomFlow.CREATEUSERNAME + "','" + CuostomFlow.CREATEUSERID + "')";
                        result = dao.ExecuteNonQuery(sqlAcow);
                        if (result == 0)
                        {
                            dao.RollbackTransaction();
                            return "自动发起流程保存失败!";
                        }
                    }
                }
                if (ListFlowTrigger != null)
                {
                    foreach (var FlowTrigger in ListFlowTrigger) //定时触发
                    {

                        string sqlAtger = "insert into T_WF_TIMINGTRIGGERCONFIG(TIMINGCONFIGID, COMPANYID, SYSTEMCODE, SYSTEMNAME, MODELCODE, MODELNAME, TRIGGERACTIVITYTYPE,TRIGGERTIME,TRIGGERROUND,WCFURL,FUNCTIONNAME,"
                                                                   + " FUNCTIONPARAMTER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,RECEIVERUSERID,RECEIVEROLE,RECEIVERNAME,MESSAGEBODY,MESSAGEURL,TRIGGERSTATUS,TRIGGERTYPE,TRIGGERDESCRIPTION, "
                                                                   + " CONTRACTTYPE,CREATEDATETIME,CREATEUSERID,CREATEUSERNAME,REMARK,TRIGGERNAME)values("
                                                                   + " '" + FlowTrigger.TIMINGCONFIGID + "','" + FlowTrigger.COMPANYID + "','" + FlowTrigger.SYSTEMCODE + "',"
                                                                   + " '" + FlowTrigger.SYSTEMNAME + "','" + FlowTrigger.MODELCODE + "','" + FlowTrigger.MODELNAME + "',"
                                                                   + "  " + FlowTrigger.TRIGGERACTIVITYTYPE + ",to_date('" + FlowTrigger.TRIGGERTIME + "','yyyy-mm-dd hh24:mi:ss')," + FlowTrigger.TRIGGERROUND + ","
                                                                   + " '" + FlowTrigger.WCFURL + "','" + FlowTrigger.FUNCTIONNAME + "','" + FlowTrigger.FUNCTIONPARAMTER + "',"
                                                                   + " '" + FlowTrigger.PAMETERSPLITCHAR + "','" + FlowTrigger.WCFBINDINGCONTRACT + "','" + FlowTrigger.RECEIVERUSERID + "',"
                                                                   + " '" + FlowTrigger.RECEIVEROLE + "','" + FlowTrigger.RECEIVERNAME + "','" + FlowTrigger.MESSAGEBODY + "',"
                                                                   + " '" + FlowTrigger.MESSAGEURL + "'," + FlowTrigger.TRIGGERSTATUS + ",'" + FlowTrigger.TRIGGERTYPE + "',"
                                                                   + " '" + FlowTrigger.TRIGGERDESCRIPTION + "','" + FlowTrigger.CONTRACTTYPE + "',to_date('" + FlowTrigger.CREATEDATETIME + "','yyyy-mm-dd hh24:mi:ss'),"
                                                                   + " '" + FlowTrigger.CREATEUSERID + "','" + FlowTrigger.CREATEUSERNAME + "','" + FlowTrigger.REMARK + "', "
                                                                   + " '" + FlowTrigger.TRIGGERNAME + "')";
                        result = dao.ExecuteNonQuery(sqlAtger);
                        if (result == 0)
                        {
                            dao.RollbackTransaction();
                            return "定时触发定义保存失败!";
                        }
                        string sqlAtgger = "insert into T_WF_TIMINGTRIGGERACTIVITY(TIMINGCONFIGID, COMPANYID, SYSTEMCODE, SYSTEMNAME, MODELCODE, MODELNAME, TRIGGERACTIVITYTYPE,TRIGGERTIME,TRIGGERROUND,WCFURL,FUNCTIONNAME,"
                                                                   + " FUNCTIONPARAMTER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,RECEIVERUSERID,RECEIVEROLE,RECEIVERNAME,MESSAGEBODY,MESSAGEURL,TRIGGERSTATUS,TRIGGERTYPE,TRIGGERDESCRIPTION, "
                                                                   + " CONTRACTTYPE,CREATEDATETIME,CREATEUSERID,CREATEUSERNAME,REMARK,TRIGGERNAME,TRIGGERID)values("
                                                                   + " '" + FlowTrigger.TIMINGCONFIGID + "','" + FlowTrigger.COMPANYID + "','" + FlowTrigger.SYSTEMCODE + "',"
                                                                   + " '" + FlowTrigger.SYSTEMNAME + "','" + FlowTrigger.MODELCODE + "','" + FlowTrigger.MODELNAME + "',"
                                                                   + "  " + FlowTrigger.TRIGGERACTIVITYTYPE + ",to_date('" + FlowTrigger.TRIGGERTIME + "','yyyy-mm-dd hh24:mi:ss')," + FlowTrigger.TRIGGERROUND + ","
                                                                   + " '" + FlowTrigger.WCFURL + "','" + FlowTrigger.FUNCTIONNAME + "','" + FlowTrigger.FUNCTIONPARAMTER + "',"
                                                                   + " '" + FlowTrigger.PAMETERSPLITCHAR + "','" + FlowTrigger.WCFBINDINGCONTRACT + "','" + FlowTrigger.RECEIVERUSERID + "',"
                                                                   + " '" + FlowTrigger.RECEIVEROLE + "','" + FlowTrigger.RECEIVERNAME + "','" + FlowTrigger.MESSAGEBODY + "',"
                                                                   + " '" + FlowTrigger.MESSAGEURL + "'," + FlowTrigger.TRIGGERSTATUS + ",'" + FlowTrigger.TRIGGERTYPE + "',"
                                                                   + " '" + FlowTrigger.TRIGGERDESCRIPTION + "','" + FlowTrigger.CONTRACTTYPE + "',to_date('" + FlowTrigger.CREATEDATETIME + "','yyyy-mm-dd hh24:mi:ss'),"
                                                                   + " '" + FlowTrigger.CREATEUSERID + "','" + FlowTrigger.CREATEUSERNAME + "','" + FlowTrigger.REMARK + "', "
                                                                   + " '" + FlowTrigger.TRIGGERNAME + "', '" + Guid.NewGuid() + "')";
                        result = dao.ExecuteNonQuery(sqlAtgger);
                        if (result == 0)
                        {
                            dao.RollbackTransaction();
                            return "定时触发定义保存失败!";
                        }
                    }
                }
                #endregion
                dao.CommitTransaction();
                return "保存成功!";
            }
            catch
            {
                dao.RollbackTransaction();
                return "保存失败!";
            }
            finally
            {
                dao.Close();
            }
        }
    }
}
