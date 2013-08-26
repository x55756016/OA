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
using SMT.Workflow.Platform.Designer.DesignerControl;

namespace SMT.Workflow.Platform.Designer.ActivityProperty
{
    public partial class FinishProperty : UserControl, IProperty
    {
        private FinishControl _finishControl;

        public FinishProperty()
        {
            InitializeComponent();
        }
        
        public void ShowPropertyWindow(UIElement element)
        {
            _finishControl = element as FinishControl;
            if (_finishControl == null) return;

            this.txtName.Text = "流程结束";
        }

        public void LoadProperty()
        {
        }
    }
}
