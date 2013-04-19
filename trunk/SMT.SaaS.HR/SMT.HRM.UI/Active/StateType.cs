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

namespace SMT.HRM.UI.Active
{
    public enum OptState
    {
        Add = 0,
        Update = 1,
        Delete = 2
      
    }

    public class StateType
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
    }

    public class SystemItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
