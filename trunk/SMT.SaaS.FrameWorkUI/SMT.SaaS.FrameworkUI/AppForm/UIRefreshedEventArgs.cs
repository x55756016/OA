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

namespace SMT.SaaS.FrameworkUI
{
    public class UIRefreshedEventArgs : EventArgs
    {
        public RefreshedTypes RefreshedType { get; set; }
        public string RefreshedArgs { get; set; }
    }
    public enum RefreshedTypes
    { 
        All,
        LeftMenu,
        ToolBar,
        EntityInfo,
        Close,
        CloseAndReloadData,
        AuditInfo,
        ProgressBar,
        HideProgressBar,
        ShowProgressBar,
        UploadBar,
        HideAudit,
        ShowAudit
    }
}
