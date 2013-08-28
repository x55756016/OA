using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Workflow.Activities;



namespace SMT.WFLib
{
    [ExternalDataExchange()]
    public interface IFlowEvent
    {
        event EventHandler<SMTFlowArg> DoFlow;
        void OnDoFlow(Guid guid, FlowDataType.FlowData Arg);
    }

    public class FlowEvent : IFlowEvent
    {
        public event EventHandler<SMTFlowArg> DoFlow;
        public void OnDoFlow(Guid guid, FlowDataType.FlowData Arg)
        {

            SMTFlowArg e = new SMTFlowArg(guid, Arg);
            e.WaitForIdle = true;
            DoFlow(null, e);
        }
    }
}
