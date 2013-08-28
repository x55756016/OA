using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Common.Model.Views;

namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : IFlowEvent
    {
        FlowEventBLL FlowEventBll = new FlowEventBLL();
        //引擎添加、修改方法
        public string FlowEventAll(T_WF_MESSAGEBODYDEFINE FlowMsg, List<T_WF_DOTASKRULE> ListFlowTrigger, List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine, List<T_WF_TIMINGTRIGGERCONFIG> ListFlowsTrigger)
        {
            return FlowEventBll.FlowEventAll(FlowMsg, ListFlowTrigger, ListCuostomFlowsDefine, ListFlowsTrigger);
        }
    }
}