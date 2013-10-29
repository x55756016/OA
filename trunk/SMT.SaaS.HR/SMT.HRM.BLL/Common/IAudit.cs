using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public interface IAudit
    {
        void SetFlowRecordEntity(Flow_FlowRecord_T entity);
        
        void OnSubmitCompleted(AuditEventArgs.AuditResult args);
        string GetAuditState();
    }

    public interface IAuditing
    {
        void OnAuditing(AuditEventArgs e);
    }
}
