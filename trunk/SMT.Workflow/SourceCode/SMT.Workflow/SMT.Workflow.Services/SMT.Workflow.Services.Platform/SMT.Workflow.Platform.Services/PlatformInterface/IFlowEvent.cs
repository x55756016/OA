using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Common.Model.Views;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
     /// <summary>
    /// 引擎添加、删除接口
    /// </summary>
    [ServiceContract]
    public interface IFlowEvent
    {
        [OperationContract]
        string FlowEventAll(T_WF_MESSAGEBODYDEFINE FlowMsg, List<T_WF_DOTASKRULE> ListFlowProecss, List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine, List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger);
    }
}