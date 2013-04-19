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
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Windows.Controls.Theming;

namespace SMT.HRM.UI
{

    public class AppConfig
    {
        public AppConfig()
        {
            CurrentUser = new LoginUser();
        }

        public DateTime LastAcitvedTime { get; set; }

        public LoginUser CurrentUser { get; set; }
    }
}
