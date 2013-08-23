using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Common.Model.Views
{
    /// <summary>
    /// [触发规则]
    /// </summary>
    [DataContract]
    public class V_DotaskRule
    {
        /// <summary>
        /// 规则主表
        /// </summary>
        [DataMember]
        public T_WF_DOTASKRULE DOTASKRULE { get; set; }

        /// <summary>
        /// 规则明细表
        /// </summary>
        [DataMember]
        public T_WF_DOTASKRULEDETAIL DOTASKRULEDETAIL { get; set; }
    }
}
