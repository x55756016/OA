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
using System.Collections.ObjectModel;

namespace SMT.SaaS.FrameworkUI
{
    public class ToolBarItemGroupModel
    {
        public ToolBarItemGroupModel()
        {
            Items = new ObservableCollection<ToolBarItemGroupModel>();
        }
        public string Titel { get; set; }
        public string IocPath { get; set; }
        public bool IsActivate { get; set; }
        public string URL { get; set; }
        public object DataContext { get; set; }
        public ObservableCollection<ToolBarItemGroupModel> Items
        {
            get;
            set;
        }
    }
}
