using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    [ServiceContract]
    public interface IDoTaskRule
    {
        [OperationContract]
        List<T_WF_DOTASKRULE> GetGetDoTaskList(int pageIndex, int pageSize, string strFilter, string strOrderBy, ref int pageCount);

        [OperationContract]
        string AddDoTaskRule(T_WF_DOTASKRULE entity);

        [OperationContract]
        string EditDoTaskRule(T_WF_DOTASKRULE entity);

        [OperationContract]
        string DeleteDoTaskRule(string ruleID);

        [OperationContract]
        List<T_WF_DOTASKRULEDETAIL> GetDoTaskRuleDetail(string RuleID);
        [OperationContract]
        T_WF_DOTASKRULE GetDoTaskRule(string RuleID);


        [OperationContract]
        string AddDoTaskRuleDetail(T_WF_DOTASKRULEDETAIL Detail);
        [OperationContract]
        string EditDoTaskRuleDetail(T_WF_DOTASKRULEDETAIL Detail);
        [OperationContract]
        string DeleteDoTaskRuleDetail(string DetailID);
    }
}