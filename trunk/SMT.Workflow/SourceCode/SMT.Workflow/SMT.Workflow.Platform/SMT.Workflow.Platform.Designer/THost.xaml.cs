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
using SMT.Workflow.Platform.Designer.Views.Message;

namespace SMT.Workflow.Platform.Designer
{
    public partial class THost : UserControl
    {
        public THost()
        {
            InitializeComponent();
            SetRoot(new SMT.Workflow.Platform.Designer.Login.Login());
            //SetRoot(new DefaultList());
        }

        public void SetRoot(UserControl userControl)
        {
            LayoutRoot.Children.Clear();
            LayoutRoot.Children.Add(userControl);
        }
    }
}
