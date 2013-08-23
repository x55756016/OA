using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowXml
{
    /// <summary>
    /// 周期转换
    /// </summary>
    [DataContract]
    public class Cycle
    {
        [DataMember]
        public string CycleType
        { get; set; }
    }
}
