using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Workflow.Activities;

namespace SMT.WFLib
{
    /// <summary>
    ///   事件参数类
    /// </summary>
    [Serializable]
    public class SMTFlowArg : ExternalDataEventArgs 
    {
       
        public FlowDataType.FlowData FlowData { get; set; }


        public SMTFlowArg(Guid instanceId, FlowDataType.FlowData flowdata)
            : base(instanceId)
        {
            FlowData = flowdata;
        }
    }
}
