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
    public partial class BeginProperty : UserControl, IProperty
    {
        private BeginControl _begin;

        public BeginProperty()
        {
            InitializeComponent();
        }

        public void ShowPropertyWindow(UIElement element)
        {
            _begin = element as BeginControl;
            if (_begin == null) return;

            this.txtName.Text = "流程开始";
        }

        public void LoadProperty()
        {
        }
    }
}
