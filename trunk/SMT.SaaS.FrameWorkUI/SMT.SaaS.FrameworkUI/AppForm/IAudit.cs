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
using SMT.SaaS.FrameworkUI.AuditControl;


namespace SMT.SaaS.FrameworkUI
{

    public interface IAudit
    {
        void SetFlowRecordEntity(Flow_FlowRecord_T entity);
        event UIRefreshedHandler OnUIRefreshed;
        void OnSubmitCompleted(AuditEventArgs.AuditResult args);
        string GetAuditState();
    }

    public interface IAuditing
    {
        void OnAuditing(AuditEventArgs e);
    }

    
}
