using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.Workflow.Platform.Designer.PlatformService;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SMT.Workflow.Platform.Designer.Class
{
    public static class FlowSystemModel
    {
        public static List<AppSystem> ListAppSystem;


        public static ObservableCollection<FLOW_MODELDEFINE_T> appModel;//XML模块代码
        public static ObservableCollection<FLOW_MODELDEFINE_T> appSystem;//XML系统代码
    }
}
