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
using SMT.FLOWDAL;

namespace SMT.WFLib
{
  
    public partial class SMTSubmitEvent : HandleExternalEventActivity
    {
        public SMTSubmitEvent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 审批信息
        /// </summary>
        public static DependencyProperty ApproveInfoProperty = DependencyProperty.Register("ApproveInfo", typeof(FlowDataType.FlowData), typeof(SMTSubmitEvent));

        [DescriptionAttribute("ApproveInfo")]
        [CategoryAttribute("ApproveInfo Category")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public FlowDataType.FlowData ApproveInfo
        {
            get
            {
                return ((FlowDataType.FlowData)(base.GetValue(SMTSubmitEvent.ApproveInfoProperty)));
            }
            set
            {
                base.SetValue(SMTSubmitEvent.ApproveInfoProperty, value);
            }
        }

        ITemplate Root()
        {
            Activity o = this.Parent;

            while (o.Parent != null)
            {
                o = o.Parent;
            }
            ITemplate tp = o as ITemplate;

            if (tp != null)
            {
                return tp;
            }
            else
            {
                throw new System.Exception("GetRoot");
            }

        }
        /// <summary>
        /// 接收事件后处理审批流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public  void SMTSubmitEvent_Invoked(object sender, ExternalDataEventArgs e)
        {
            SMTFlowArg input = e as SMTFlowArg;
            this.ApproveInfo = input.FlowData;
            Root().FlowData = input.FlowData;
           
            //Console.WriteLine("事件节点：" + this.Name + "工作流返回结果：" + ApproveInfo.Flow_FlowRecord_T.Content);
           //以下为处理代码
        }
    }
}
