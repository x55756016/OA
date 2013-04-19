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

using System.Collections.Generic;

namespace SMT.SaaS.FrameworkUI
{
    public class NavigateItem
    {
        public string Title { get; set; }
        public string Tooltip { get; set; }
        //public int ItemType { get; set; }
        public string Url { get; set; }
        public List<NavigateItem> SubItems { get; set; }
    }
}
