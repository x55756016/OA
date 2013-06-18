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
using SMT.Saas.Tools.FlowWFService;

namespace SMT.Saas.Tools
{
    public interface IAuditService
    {
         event System.EventHandler<SubimtFlowCompletedEventArgs> SubimtFlowCompleted;
         event System.EventHandler<GetFlowInfoCompletedEventArgs> GetFlowInfoCompleted;
         void SubimtFlowAsync(SMT.Saas.Tools.FlowWFService.SubmitData ApprovalData);
         void GetFlowInfoAsync(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID);
    }

    public class CommonAudtiService  : IAuditService
    {
        public ServiceClient scFlow;
        public CommonAudtiService()
        {
            scFlow = new ServiceClient();
            scFlow.SubimtFlowCompleted += new EventHandler<SubimtFlowCompletedEventArgs>(scFlow_SubimtFlowCompleted);
            scFlow.GetFlowInfoCompleted += new EventHandler<GetFlowInfoCompletedEventArgs>(scFlow_GetFlowInfoCompleted);
        }

        void scFlow_GetFlowInfoCompleted(object sender, GetFlowInfoCompletedEventArgs e)
        {
            if (GetFlowInfoCompleted != null)
            {
                GetFlowInfoCompleted(sender, e);
            }
        }

        void scFlow_SubimtFlowCompleted(object sender, SubimtFlowCompletedEventArgs e)
        {
            
            if (SubimtFlowCompleted != null)
            {
                SubimtFlowCompleted(sender, e);
            }
        }

        #region IAuditService Members

        public event EventHandler<SubimtFlowCompletedEventArgs> SubimtFlowCompleted;

        public event EventHandler<GetFlowInfoCompletedEventArgs> GetFlowInfoCompleted;

        public void SubimtFlowAsync(SubmitData ApprovalData)
        {
            scFlow.SubimtFlowAsync(ApprovalData);
        }

        public void GetFlowInfoAsync(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            scFlow.GetFlowInfoAsync(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID);
        }

        #endregion
    }
}
