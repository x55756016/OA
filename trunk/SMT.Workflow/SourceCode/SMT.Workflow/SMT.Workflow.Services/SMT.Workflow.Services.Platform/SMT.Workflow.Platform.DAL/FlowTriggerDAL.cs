using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using System.Data;
using System.Linq.Dynamic;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Common.Model.Views;
namespace SMT.Workflow.Platform.DAL
{
    public class FlowTriggerDAL : BaseDAL
    {
        #region 触发规则定义增、删、改、查方法
        //自动触发规则定义
        public IQueryable<V_DotaskRule> GetFlowTrigger(string filterString)
        {
            try
            {
                string sql = string.Empty;
                string detailID = string.Empty;
                if (filterString != null && filterString != "")
                {
                    sql = "SELECT * FROM T_WF_DOTASKRULE where " + filterString;
                }
                else
                {
                    sql = "SELECT * FROM T_WF_DOTASKRULE";
                }
                dao.Open();
                DataTable dtTriggerDefine = dao.GetDataTable(sql);
                var items = ToList<T_WF_DOTASKRULE>(dtTriggerDefine).AsQueryable();
                if (items.Count() > 0)
                {
                    foreach (var item in items)
                    {
                        if (detailID == "")
                        {
                            detailID = "('" + item.DOTASKRULEID + "'";
                        }
                        else
                        {
                            detailID += ",'" + item.DOTASKRULEID + "'";
                        }
                    }
                    detailID += ")";
                }
                List<V_DotaskRule> Dotaskrule = new List<V_DotaskRule>();
                if (detailID != "")
                {
                    string strSqls = "SELECT * FROM T_WF_DOTASKRULEDETAIL where DOTASKRULEID in" + detailID;
                    DataTable dtTriggerDefines = dao.GetDataTable(strSqls);
                    dao.Close();
                    var itemes = ToList<T_WF_DOTASKRULEDETAIL>(dtTriggerDefines).AsQueryable();
                   
                    foreach (var item in itemes)
                    {
                        T_WF_DOTASKRULE rule = new T_WF_DOTASKRULE();
                        rule = items.Where(p => p.DOTASKRULEID == item.DOTASKRULEID).FirstOrDefault();
                        rule.DOTASKRULEDETAIL = null;
                        if (rule != null)
                        {
                            Dotaskrule.Add(new V_DotaskRule { 
                            DOTASKRULE = rule,
                            DOTASKRULEDETAIL = item
                            });
                        }
                    }
                }
                return Dotaskrule.AsQueryable();
            }
            catch
            {
                return null;
            }
        }
        //自动触发规则定义
        public IQueryable<V_DotaskRule> GetListFlowTrigger(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            try
            {
                string sql = string.Empty;
                if (filterString != null && filterString != "")
                {
                    sql = "SELECT * FROM T_WF_DOTASKRULEDETAIL where " + filterString;
                }
                else
                {
                    sql = "SELECT * FROM T_WF_DOTASKRULEDETAIL";
                }
                dao.Open();
                DataTable dtTriggerDefine = dao.GetDataTable(sql);
               
                var items = ToList<T_WF_DOTASKRULEDETAIL>(dtTriggerDefine).AsQueryable();

                if (filterString != null && filterString != "")
                {
                    sql = "SELECT * FROM T_WF_DOTASKRULE where " + filterString;
                }
                else
                {
                    sql = "SELECT * FROM T_WF_DOTASKRULE";
                }

                DataTable dtTriggerDefines = dao.GetDataTable(sql);
                dao.Close();
                var itemes = ToList<T_WF_DOTASKRULE>(dtTriggerDefines).OrderBy(s=>s.MODELNAME);
                List<V_DotaskRule> Dotaskrule = new List<V_DotaskRule>();
                foreach (var item in items)
                {
                    T_WF_DOTASKRULE rule = new T_WF_DOTASKRULE();
                    rule = itemes.Where(p => p.DOTASKRULEID == item.DOTASKRULEID).FirstOrDefault();
                    rule.DOTASKRULEDETAIL = null;
                    if (rule != null)
                    {
                        Dotaskrule.Add(new V_DotaskRule { 
                        DOTASKRULE = rule,
                        DOTASKRULEDETAIL = item
                        });
                    }
                }
                Dotaskrule = Pager<V_DotaskRule>(Dotaskrule.AsQueryable(), pageIndex, pageSize, ref pageCount).ToList();
                return Dotaskrule.AsQueryable();
            }
            catch 
            {
                return null;
            }
        }
        ////添加触发规则定义
        //public bool AddFlowTrigger(List<T_FLOW_FLOWPROCESSDEFINE> ListFlowTrigger)
        //{
        //    try
        //    {
        //        int result = 0;
        //        dao.Open();
        //        dao.BeginTransaction();
        //        foreach (var FlowTrigger in ListFlowTrigger)
        //        {
        //            string sql = "insert into T_FLOW_FLOWPROCESSDEFINE(PROCESSID, ENGINECODE , COMPANYCODE , SYSTEMCODE,SYSTEMNAME ,MODELCODE ,MODELNAME ,PROCESSWCFURL ,PROCESSFUNCNAME ,"
        //                                                            + " PROCESSFUNCPAMETER ,PAMETERSPLITCHAR ,WCFBINDINGCONTRACT ,MESSAGEBODY ,AVAILABILITYPROCESSDATES ,CREATEDATE ,"
        //                                                            + " CREATETIME ,APPLICATIONURL ,RECEIVEUSER ,RECEIVEUSERNAME ,OWNERCOMPANYID ,OWNERDEPARTMENTID ,OWNERPOSTID ,DEFAULTMSG ,"
        //                                                            + " CREATEUSERNAME ,CREATEUSERID ,PROCESSFUNCLANGUAGE ,ISOTHERSOURCE ,OTHERSYSTEMCODE ,OTHERMODELCODE,TRIGGERCONDITION)values("
        //                                                            + " '" + FlowTrigger.PROCESSID + "','" + FlowTrigger.ENGINECODE + "','" + FlowTrigger.COMPANYCODE + "',"
        //                                                            + " '" + FlowTrigger.SYSTEMCODE + "','" + FlowTrigger.SYSTEMNAME + "','" + FlowTrigger.MODELCODE + "',"
        //                                                            + " '" + FlowTrigger.MODELNAME + "','" + FlowTrigger.PROCESSWCFURL + "','" + FlowTrigger.PROCESSFUNCNAME + "',"
        //                                                            + " '" + FlowTrigger.PROCESSFUNCPAMETER + "','" + FlowTrigger.PAMETERSPLITCHAR + "','" + FlowTrigger.WCFBINDINGCONTRACT + "',"
        //                                                            + " '" + FlowTrigger.MESSAGEBODY + "','" + FlowTrigger.AVAILABILITYPROCESSDATES + "','" + FlowTrigger.CREATEDATE + "',"
        //                                                            + " '" + FlowTrigger.CREATETIME + "','" + FlowTrigger.APPLICATIONURL + "','" + FlowTrigger.RECEIVEUSER + "',"
        //                                                            + " '" + FlowTrigger.RECEIVEUSERNAME + "','" + FlowTrigger.OWNERCOMPANYID + "','" + FlowTrigger.OWNERDEPARTMENTID + "',"
        //                                                            + " '" + FlowTrigger.OWNERPOSTID + "','" + FlowTrigger.DEFAULTMSG + "','" + FlowTrigger.CREATEUSERNAME + "',"
        //                                                            + " '" + FlowTrigger.CREATEUSERID + "','" + FlowTrigger.PROCESSFUNCLANGUAGE + "','" + FlowTrigger.ISOTHERSOURCE + "',"
        //                                                            + " '" + FlowTrigger.OTHERSYSTEMCODE + "','" + FlowTrigger.OTHERMODELCODE + "','" + FlowTrigger.TRIGGERCONDITION + "')";
        //            result = dao.ExecuteNonQuery(sql);
        //            if (result > 0)
        //            {
        //                string sqlstr = "insert into T_FLOW_FLOWTRIGGER(ENGINECODE , COMPANYCODE , SYSTEMCODE,SYSTEMNAME ,MODELCODE ,MODELNAME ,TRIGGERCONDITION ,CREATEDATE ,"
        //                                                            + " CREATETIME ,CREATEUSERNAME ,CREATEUSERID)values("
        //                                                            + " '" + FlowTrigger.ENGINECODE + "','" + FlowTrigger.COMPANYCODE + "',"
        //                                                            + " '" + FlowTrigger.SYSTEMCODE + "','" + FlowTrigger.SYSTEMNAME + "','" + FlowTrigger.MODELCODE + "',"
        //                                                            + " '" + FlowTrigger.MODELNAME + "','" + FlowTrigger.TRIGGERCONDITION + "','" + FlowTrigger.CREATEDATE + "',"
        //                                                            + " '" + FlowTrigger.CREATETIME + "','" + FlowTrigger.CREATEUSERNAME + "','" + FlowTrigger.CREATEUSERID + "')";
        //                result = dao.ExecuteNonQuery(sqlstr);
        //                if (result < 1)
        //                {
        //                    dao.RollbackTransaction();
        //                    return false;
        //                }
        //            }
        //            else 
        //            {
        //                dao.RollbackTransaction();
        //                return false;
        //            }
        //        }
        //        if (result > 0)
        //        {
        //            dao.CommitTransaction();
        //            return true;
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
        ////修改触发规则定义
        //public bool UpdateFlowTrigger(List<T_FLOW_FLOWPROCESSDEFINE> ListFlowTrigger)
        //{
        //    try
        //    {
        //        int result = 0;
        //        dao.Open();
        //        dao.BeginTransaction();
        //        foreach (var FlowTrigger in ListFlowTrigger)
        //        {
        //            string sql = "update T_FLOW_FLOWPROCESSDEFINE set "
        //                            + " ENGINECODE = '" + FlowTrigger.ENGINECODE + "', "
        //                            + " COMPANYCODE = '" + FlowTrigger.COMPANYCODE + "', "
        //                            + " SYSTEMCODE = '" + FlowTrigger.SYSTEMCODE + "', "
        //                            + " SYSTEMNAME = '" + FlowTrigger.SYSTEMNAME + "', "
        //                            + " MODELCODE = '" + FlowTrigger.MODELCODE + "', "
        //                            + " MODELNAME = '" + FlowTrigger.MODELNAME + "', "
        //                            + " TRIGGERCONDITION = '" + FlowTrigger.TRIGGERCONDITION + "', "
        //                            + " PROCESSWCFURL = '" + FlowTrigger.PROCESSWCFURL + "', "
        //                            + " PROCESSFUNCNAME = '" + FlowTrigger.PROCESSFUNCNAME + "', "
        //                            + " PROCESSFUNCPAMETER = '" + FlowTrigger.PROCESSFUNCPAMETER + "', "
        //                            + " PAMETERSPLITCHAR = '" + FlowTrigger.PAMETERSPLITCHAR + "', "
        //                            + " WCFBINDINGCONTRACT = '" + FlowTrigger.WCFBINDINGCONTRACT + "', "
        //                            + " MESSAGEBODY = '" + FlowTrigger.MESSAGEBODY + "', "
        //                            + " AVAILABILITYPROCESSDATES = '" + FlowTrigger.AVAILABILITYPROCESSDATES + "', "
        //                            + " CREATEDATE = '" + FlowTrigger.CREATEDATE + "', "
        //                            + " CREATETIME = '" + FlowTrigger.CREATETIME + "', "
        //                            + " APPLICATIONURL = '" + FlowTrigger.APPLICATIONURL + "', "
        //                            + " RECEIVEUSER = '" + FlowTrigger.RECEIVEUSER + "', "
        //                            + " RECEIVEUSERNAME = '" + FlowTrigger.RECEIVEUSERNAME + "', "
        //                            + " OWNERCOMPANYID = '" + FlowTrigger.OWNERCOMPANYID + "', "
        //                            + " OWNERDEPARTMENTID = '" + FlowTrigger.OWNERDEPARTMENTID + "', "
        //                            + " OWNERPOSTID = '" + FlowTrigger.OWNERPOSTID + "', "
        //                            + " DEFAULTMSG = '" + FlowTrigger.DEFAULTMSG + "', "
        //                            + " CREATEUSERNAME = '" + FlowTrigger.CREATEUSERNAME + "', "
        //                            + " CREATEUSERID = '" + FlowTrigger.CREATEUSERID + "', "
        //                            + " PROCESSFUNCLANGUAGE = '" + FlowTrigger.PROCESSFUNCLANGUAGE + "', "
        //                            + " ISOTHERSOURCE = '" + FlowTrigger.ISOTHERSOURCE + "', "
        //                            + " OTHERSYSTEMCODE = '" + FlowTrigger.OTHERSYSTEMCODE + "', "
        //                            + " OTHERMODELCODE = '" + FlowTrigger.OTHERMODELCODE + "' "
        //                            + "where PROCESSID = '" + FlowTrigger.PROCESSID + "' ";
        //            result = dao.ExecuteNonQuery(sql);
        //            if (result > 0)
        //            {
        //                string sqlstr = "update T_FLOW_FLOWTRIGGER set "
        //                                        + " COMPANYCODE = '" + FlowTrigger.COMPANYCODE + "', "
        //                                        + " SYSTEMCODE = '" + FlowTrigger.SYSTEMCODE + "', "
        //                                        + " SYSTEMNAME = '" + FlowTrigger.SYSTEMNAME + "', "
        //                                        + " MODELCODE = '" + FlowTrigger.MODELCODE + "', "
        //                                        + " MODELNAME = '" + FlowTrigger.MODELNAME + "', "
        //                                        + " TRIGGERCONDITION = '" + FlowTrigger.TRIGGERCONDITION + "', "
        //                                        + " CREATEDATE = '" + FlowTrigger.CREATEDATE + "', "
        //                                        + " CREATETIME = '" + FlowTrigger.CREATETIME + "', "
        //                                        + " CREATEUSERNAME = '" + FlowTrigger.CREATEUSERNAME + "', "
        //                                        + " CREATEUSERID = '" + FlowTrigger.CREATEUSERID + "' "
        //                                        + "where ENGINECODE = '" + FlowTrigger.ENGINECODE + "' ";
        //                result = dao.ExecuteNonQuery(sqlstr);
        //                if (result < 1)
        //                {
        //                    dao.RollbackTransaction();
        //                    return false;
        //                }
        //            }
        //            else
        //            {
        //                dao.RollbackTransaction();
        //                return false;
        //            }
        //        }
        //        if (result > 0)
        //        {
        //            dao.CommitTransaction();
        //            return true;
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
        //删除触发规则定义
        public bool DeleteFlowTrigger(List<T_WF_DOTASKRULEDETAIL> ListFlowTrigger)
        {
            try
            {
                List<T_WF_DOTASKRULEDETAIL> listflowProcess = new List<T_WF_DOTASKRULEDETAIL>();
                List<T_WF_DOTASKRULE> listDotaskRule = new List<T_WF_DOTASKRULE>();
                dao.Open();
                foreach (var items in ListFlowTrigger)
                {
                    string sql = "select * from T_WF_DOTASKRULEDETAIL "
                                                        + "where DOTASKRULEID = '" + items.DOTASKRULEID + "' ";
                  
                    DataTable dtTriggerDefine = dao.GetDataTable(sql);
                    var itemes = ToList<T_WF_DOTASKRULEDETAIL>(dtTriggerDefine).AsQueryable();
                    
                    if (itemes.ToList().Count() == 1)
                    {
                        string sqls = "select * from T_WF_DOTASKRULE "
                                                       + "where DOTASKRULEID = '" + itemes.FirstOrDefault().DOTASKRULEID + "' ";
                        DataTable dtTriggerDefines = dao.GetDataTable(sqls);
                        var itemess = ToList<T_WF_DOTASKRULE>(dtTriggerDefines).AsQueryable();
                        listDotaskRule.Add(itemess.FirstOrDefault());
                    }
                }
                int result = 0;
                //dao.Open();
                dao.BeginTransaction();

                foreach (var item in ListFlowTrigger)
                {
                    string strSql = "delete from T_WF_DOTASKRULEDETAIL where DOTASKRULEDETAILID = '" + item.DOTASKRULEDETAILID + "' ";
                    result = dao.ExecuteNonQuery(strSql);
                    if (result == 0)
                    {
                        dao.RollbackTransaction();
                        return false;
                    }
                    if (listDotaskRule.Where(p => p.DOTASKRULEID == item.DOTASKRULEID).Count() > 0)
                    {
                        string strstr = "delete from T_WF_DOTASKRULE where DOTASKRULEID = '" + item.DOTASKRULEID + "' ";
                        result = dao.ExecuteNonQuery(strstr);
                        if (result < 1)
                        {
                            dao.RollbackTransaction();
                            return false;
                        }
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
