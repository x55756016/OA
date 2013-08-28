using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Common.Model.Views;

namespace SMT.Workflow.Platform.BLL
{
    public class FlowEventBLL
    {
        FlowEventDAL dal = new FlowEventDAL();
         //引擎添加、修改方法
        public string FlowEventAll(T_WF_MESSAGEBODYDEFINE FlowMsg, List<T_WF_DOTASKRULE> ListFlowProecss, List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine, List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger)
        {
            return dal.FlowEventAll(FlowMsg, ListFlowProecss, ListCuostomFlowsDefine, ListFlowTrigger);
        }
    }
}
