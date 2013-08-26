using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using SMT.Workflow.Platform.Designer.PlatformService;
using SMT.Workflow.Platform.Designer.DesignerControl;



namespace SMT.Workflow.Platform.Designer.Views.FlowDesign
{
    public partial class FlowSetting : UserControl
    {
        public FlowSetting()
        {
            InitializeComponent();                    
        }

        /// <summary>
        /// 显示元素属性
        /// </summary>
        /// <param name="element"></param>
        public void ShowProperty(UIElement element)
        {
            HideAllProperties();

            if (element == null)
            {
                flowProperty.Visibility = Visibility.Visible;
                flowProperty.ShowPropertyWindow(null);             
                txtTitle.Text = "流程设置";
            }
            else
            {
                switch (((IControlBase)element).Type)
                {
                    case ElementType.Begin:
                        beginProperty.Visibility = Visibility.Visible;
                        beginProperty.ShowPropertyWindow(element);                       
                        txtTitle.Text = "开始节点";
                        break;
                    case ElementType.Finish:
                        finishProperty.Visibility = Visibility.Visible;
                        finishProperty.ShowPropertyWindow(element);
                        txtTitle.Text = "结束节点";
                        break;
                    case ElementType.Activity:
                        activityProperty.Visibility = Visibility.Visible;
                        activityProperty.ShowPropertyWindow(element);                      
                        activityProperty.LoadProperty();
                        txtTitle.Text = "节点设置";
                        break;
                    case ElementType.Line:
                        lineProperty.Visibility = Visibility.Visible;
                        lineProperty.ShowPropertyWindow(element);
                        SetLineProperty();
                        lineProperty.LoadProperty();
                        txtTitle.Text = "条件设置";
                        break;
                }
            }
        }

        private void SetLineProperty()
        {
            if (flowProperty.cbSystemCode.SelectedIndex >0)
            {              
                lineProperty.systemCode = (flowProperty.cbSystemCode.SelectedItem as FLOW_MODELDEFINE_T).SYSTEMCODE; //业务系统(基本设置)
            }

            if (flowProperty.cbModelCode.SelectedIndex>0)
            {
                lineProperty.modelCode = (flowProperty.cbModelCode.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE;//业务对象(基本设置)
            }
        }

        /// <summary>
        /// 隐藏所有的属性
        /// </summary>
        private void HideAllProperties()
        {
            flowProperty.Visibility = Visibility.Collapsed;
            beginProperty.Visibility = Visibility.Collapsed;
            finishProperty.Visibility = Visibility.Collapsed;
            activityProperty.Visibility = Visibility.Collapsed;
            lineProperty.Visibility = Visibility.Collapsed;
        }
    }
}
