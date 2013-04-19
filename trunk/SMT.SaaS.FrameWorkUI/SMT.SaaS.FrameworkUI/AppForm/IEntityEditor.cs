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
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.FrameworkUI
{

    public delegate void UIRefreshedHandler(object sender, UIRefreshedEventArgs args);
    public interface IEntityEditor
    {
        string GetTitle();
        string GetStatus();
        void DoAction(string actionType);
        List<NavigateItem> GetLeftMenuItems();
        List<ToolbarItem> GetToolBarItems();
        event UIRefreshedHandler OnUIRefreshed;
    }
}
