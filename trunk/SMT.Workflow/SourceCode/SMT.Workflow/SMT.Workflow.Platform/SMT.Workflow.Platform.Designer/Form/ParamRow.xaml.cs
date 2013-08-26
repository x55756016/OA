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
using SMT.Workflow.Platform.Designer.Class;
using SMT.Workflow.Platform.Designer.PlatformService;
using System.Collections.ObjectModel;

namespace SMT.Workflow.Platform.Designer.Form
{
    public partial class ParamRow : UserControl
    {
        public delegate void RowDoubleClicked(object sender, DefineClickedArgs args);
        public event RowDoubleClicked OnSubmitComplated; //注册事件
        public ParamRow()
        {
            InitializeComponent();
           
        }
        private void btnRole_Click(object sender, RoutedEventArgs e)
        {
            Param  model = (Param)((Button)e.OriginalSource).DataContext;
            if (OnSubmitComplated != null)
                OnSubmitComplated(sender, new DefineClickedArgs() { SelectedItem = model });                
        }
       
        private void ToolDelete_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
    public class DefineClickedArgs : EventArgs
    {
        public Param SelectedItem { get; set; }
    }
      
}
