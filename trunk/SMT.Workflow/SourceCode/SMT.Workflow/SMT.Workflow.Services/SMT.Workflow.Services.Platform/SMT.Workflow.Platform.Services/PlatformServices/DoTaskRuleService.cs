using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model.FlowEngine;


namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : IDoTaskRule
    {

        private DoTaskRuleBLL doTaskbll = new DoTaskRuleBLL();


        public List<T_WF_DOTASKRULE> GetGetDoTaskList(int pageIndex, int pageSize, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {
                return doTaskbll.GetDoTaskList(pageSize, pageIndex, strFilter, strOrderBy, ref pageCount);
            }
            catch (Exception ex)
            {
                //记录错误日志
                return null;
            }
        }

        public T_WF_DOTASKRULE GetDoTaskRule(string RuleID)
        {
            try
            {
                return doTaskbll.GetDoTaskRule(RuleID);
            }
            catch (Exception ex)
            {
                //记录错误日志
                return null;
            }
        }
        public List<T_WF_DOTASKRULEDETAIL> GetDoTaskRuleDetail(string RuleID)
        {
            try
            {
                return doTaskbll.GetDoTaskRuleDetail(RuleID);
            }
            catch (Exception ex)
            {
                //记录错误日志
                return null;
            }
        }


        public string AddDoTaskRule(T_WF_DOTASKRULE entity)
        {
            try
            {
                if (doTaskbll.GetBool(entity))
                {
                    doTaskbll.AddDoTaskRule(entity);
                }
                else
                {
                    return "2";
                }
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string EditDoTaskRule(T_WF_DOTASKRULE entity)
        {
            try
            {
                if (doTaskbll.GetBool(entity))
                {
                    doTaskbll.EditDoTaskRule(entity);
                }
                else
                {
                    return "2";
                }
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteDoTaskRule(string ruleID)
        {
            try
            {
                doTaskbll.DeleteDoTaskRule(ruleID);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string AddDoTaskRuleDetail(T_WF_DOTASKRULEDETAIL Detail)
        {
            try
            {
                doTaskbll.AddDoTaskRuleDetail(Detail);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string EditDoTaskRuleDetail(T_WF_DOTASKRULEDETAIL Detail)
        {
            try
            {
                doTaskbll.EditDoTaskRuleDetail(Detail);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteDoTaskRuleDetail(string DetailID)
        {
            try
            {
                doTaskbll.DeleteDoTaskRuleDetail(DetailID);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}