using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace SMT.WFLib
{
    public partial class SMTSubmitEvent
    {
        #region Designer generated code

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        private void InitializeComponent()
        {
            // 
            // SMTSubmitEvent
            // 
            this.EventName = "DoFlow";
            this.InterfaceType = typeof(SMT.WFLib.IFlowEvent);
            this.Name = "SMTSubmitEvent";
            this.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.SMTSubmitEvent_Invoked);

        }

        #endregion
    }
}
