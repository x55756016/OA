using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace SMT.WFLib
{
    public partial class SMTStateMachineWorkflowActivity : StateMachineWorkflowActivity, ITemplate
	{
		public SMTStateMachineWorkflowActivity()
		{
			InitializeComponent();
		}







        public static DependencyProperty FlowDataProperty = DependencyProperty.Register("FlowData", typeof(FlowDataType.FlowData), typeof(SMTStateMachineWorkflowActivity));

        [DescriptionAttribute("FlowData")]
        [CategoryAttribute("FlowData Category")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public FlowDataType.FlowData FlowData
        {
            get
            {
                return ((FlowDataType.FlowData)(base.GetValue(SMTStateMachineWorkflowActivity.FlowDataProperty)));
            }
            set
            {
                base.SetValue(SMTStateMachineWorkflowActivity.FlowDataProperty, value);
            }
        }
	}
}
