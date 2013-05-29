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
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.FB.UI.FBCommonWS;
using System.Collections.ObjectModel;
using System.Linq;
using SMT.Saas.Tools;
namespace SMT.FB.UI.Common.Controls
{


    public class AuditControlFB : AuditControl
    {

        private OrderEntity orderEntity;
        public OrderEntity OrderEntity
        {
            get
            {
                return orderEntity;
            }
            set
            {
                if (!object.Equals(orderEntity, value))
                {
                    orderEntity = value;
                    InitAudit(AuditOperation.Update);
                }


            }
        }

        private FBEntity _SaveFBEntity;
        private FBEntity SaveFBEntity
        {
            get
            {
                if (_SaveFBEntity == null)
                {
                    _SaveFBEntity = this.OrderEntity.GetModifiedFBEntity();
                }
                return _SaveFBEntity;
            }
            set
            {
                _SaveFBEntity = value;
            }
        }

        private AuditService auditServiceLocal;
        public AuditService AuditServiceLocal
        {
            get
            {
                if (auditServiceLocal == null)
                {
                    auditServiceLocal = new AuditService();
                    AuditServiceLocal.AuditFBEntityCompleted += new EventHandler<AuditFBEntityCompletedEventArgs>(AuditService_AuditFBEntityCompleted);
                }
                return auditServiceLocal;
            }
        }

        FBCommonServiceClient clientFBComm = null;
        FBFlowService fbFlowService = null;
        AuditEventArgs curAuditEventArgs = null;
        public AuditControlFB()
        {
            clientFBComm = new FBCommonServiceClient();
            fbFlowService = new FBFlowService();
            this.AuditService = fbFlowService;
            fbFlowService.DoSubitFlowAction = this.DoSubitFlowAction;
            base.AuditCompleted += new EventHandler<AuditEventArgs>(AuditControlFB_AuditCompleted);
            clientFBComm.GetFBEntityCompleted += new EventHandler<GetFBEntityCompletedEventArgs>(clientFBComm_GetFBEntityCompleted);
        }

        void clientFBComm_GetFBEntityCompleted(object sender, GetFBEntityCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    this.IsEnabled = false;
                    return;
                }

                FBEntity fbEntity = e.Result;
                bool isExtend = false;
                string strOrderId = string.Empty, strModelCode = string.Empty;

                if (this.OrderEntity.OrderType == typeof(T_FB_CHARGEAPPLYMASTER) || this.OrderEntity.OrderType == typeof(T_FB_BORROWAPPLYMASTER))
                {
                    T_FB_EXTENSIONALORDER entMaster = fbEntity.Entity as T_FB_EXTENSIONALORDER;
                    if (entMaster.T_FB_EXTENSIONALTYPE != null)
                    {
                        isExtend = true;
                        strModelCode = entMaster.T_FB_EXTENSIONALTYPE.MODELCODE;
                        strOrderId = entMaster.ORDERID;
                    }
                }

                if (!isExtend)
                {
                    return;
                }

                AuditEntity.ModelCode = strModelCode;
                AuditEntity.FormID = strOrderId;
            }
        }

        void AuditControlFB_AuditCompleted(object sender, AuditEventArgs e)
        {
            if (curAuditEventArgs == null)
            {
                curAuditEventArgs = e;
            }

            if (this.AuditCompleted != null)
            {
                this.AuditCompleted(this, curAuditEventArgs);
            }

        }

        private void DoSubitFlowAction(Saas.Tools.FlowWFService.SubmitData submitData)
        {
            CheckStates checkStates = CheckStates.Approving;
            if (submitData.SubmitFlag == Saas.Tools.FlowWFService.SubmitFlag.Approval)
            {
                if (submitData.ApprovalResult == Saas.Tools.FlowWFService.ApprovalResult.NoPass)
                {
                    checkStates = CheckStates.UnApproved;
                }
                else
                {
                    checkStates = CheckStates.Approved;
                }

            }

            VirtualAudit va = InitVirtualAudit(submitData);

            this.OrderEntity.SetObjValue(EntityFieldName.UpdateUserID, OrderEntity.LoginUser.Value);

            FBEntity entityOrder = this.SaveFBEntity;

            FBEntity FBEntityAudit = va.ToFBEntity();
            FBEntityAudit.AddReferenceFBEntity<EntityObject>(entityOrder);

            if (checkStates == CheckStates.Approving)
            {
                entityOrder.SetObjValue(EntityFieldName.UpdateUserID, this.OrderEntity.LoginUser.Value);
                entityOrder.SetObjValue(EntityFieldName.UpdateDate, DateTime.Now);
            }
            // 重新提交
            if (this.OrderEntity.IsReSubmit)
            {
                checkStates = CheckStates.ReSubmit;
            }
            AuditServiceLocal.AuditFBEntity(FBEntityAudit, checkStates);
        }

        private VirtualAudit InitVirtualAudit(Saas.Tools.FlowWFService.SubmitData submitData)
        {
            string op = submitData.SubmitFlag == Saas.Tools.FlowWFService.SubmitFlag.New ? "ADD" : "UPDATE";

            string nextStateCode = submitData.NextStateCode;

            VirtualAudit va = new VirtualAudit()
            {
                FormID = submitData.FormID,

                ModelCode = submitData.ModelCode,
                CREATECOMPANYID = submitData.ApprovalUser.CompanyID,
                CREATEDEPARTMENTID = submitData.ApprovalUser.DepartmentID,
                CREATEPOSTID = submitData.ApprovalUser.PostID,
                CREATEUSERID = submitData.ApprovalUser.UserID,
                CREATEUSERNAME = submitData.ApprovalUser.UserName,
                NextStateCode = nextStateCode,
                OWNERCOMPANYID = submitData.NextApprovalUser.CompanyID,
                OWNERDEPARTMENTID = submitData.NextApprovalUser.DepartmentID,
                OWNERID = submitData.NextApprovalUser.UserID,
                OWNERNAME = submitData.NextApprovalUser.UserName,
                OWNERPOSTID = submitData.NextApprovalUser.PostID,
                UPDATEUSERID = submitData.NextApprovalUser.UserID,
                UPDATEUSERNAME = submitData.NextApprovalUser.UserName,
                Content = submitData.ApprovalContent,
                Op = op,
                Result = (int)submitData.ApprovalResult,
                FlowSelectType = (int)submitData.FlowSelectType
            };
            return va;
        }

        private Saas.Tools.FlowWFService.SubimtFlowCompletedEventArgs GetFlowCompletedArgs(AuditFBEntityCompletedEventArgs e)
        {
            var dr = e.Result.DataResult;
            Saas.Tools.FlowWFService.DataResult newDr = null;
            if (dr == null)
            {
                dr = new DataResult();
                dr.FlowResult = FlowResult.FAIL;

            }
            newDr = new Saas.Tools.FlowWFService.DataResult
            {
                //  AgentUserInfo = dr.AgentUserInfo,
                AppState = dr.AppState,
                CheckState = dr.CheckState,
                Err = dr.Err,
                ErrNum = dr.ErrNum,
                FlowResult = (Saas.Tools.FlowWFService.FlowResult)((int)dr.FlowResult),
                ModelFlowRelationID = dr.ModelFlowRelationID,
                RunTime = dr.RunTime,
                SubModelCode = dr.SubModelCode,
                UserInfo = new ObservableCollection<Saas.Tools.FlowWFService.UserInfo>(),

            };

            if (dr.UserInfo != null)
            {
                dr.UserInfo.ToList().ForEach(item =>
                {
                    Saas.Tools.FlowWFService.UserInfo ui = new Saas.Tools.FlowWFService.UserInfo
                    {
                        UserID = item.UserID,
                        UserName = item.UserName,
                        CompanyID = item.CompanyID,                        
                        CompanyName = item.CompanyName,
                        DepartmentID = item.DepartmentID,
                        DepartmentName = item.DepartmentName,
                        PostID = item.PostID,
                        PostName = item.PostName
                    };
                    newDr.UserInfo.Add(ui);
                });
            }

            Saas.Tools.FlowWFService.SubimtFlowCompletedEventArgs args = new Saas.Tools.FlowWFService.SubimtFlowCompletedEventArgs(new object[] { newDr },
                null, e.Cancelled, e.UserState);
            return args;
        }


        private void InitAudit(AuditOperation op)
        {
            EmployeerData auditor = null;
            if (op == AuditOperation.Add)
            {
                auditor = this.OrderEntity.GetOwnerInfo();
                string userID = Convert.ToString(auditor.Value);
                if (string.IsNullOrEmpty(userID) || userID == DataCore.SuperUser.Value.ToString())
                {
                    auditor = this.OrderEntity.LoginUser;
                }
            }
            else
            {
                auditor = this.OrderEntity.LoginUser;
            }

            // 提交前，需要对AuditEntity赋值 ，以下属性必填

            AuditEntity.CreateCompanyID = auditor.Company.Value.ToString();            
            AuditEntity.CreateDepartmentID = auditor.Department.Value.ToString();
            AuditEntity.CreatePostID = auditor.Post.Value.ToString();
            AuditEntity.CreateUserID = auditor.Value.ToString();
            AuditEntity.CreateUserName = auditor.Text;
            AuditEntity.EditUserID = auditor.Value.ToString();
            AuditEntity.EditUserName = auditor.Text;

            AuditEntity.ModelCode = this.OrderEntity.OrderInfo.Type;
            AuditEntity.FormID = this.OrderEntity.OrderID;

            // 如果这个单据是报销单，或借款单，且有外部扩展单据关联。重新赋FormID和ModelCode 和EditUserID= ""

            if (this.OrderEntity.OrderType != typeof(T_FB_CHARGEAPPLYMASTER) &&
                    this.OrderEntity.OrderType != typeof(T_FB_BORROWAPPLYMASTER))
            {
                return;
            }

            if (this.OrderEntity.OrderType == typeof(T_FB_CHARGEAPPLYMASTER))
            {
                T_FB_CHARGEAPPLYMASTER entView = orderEntity.Entity as T_FB_CHARGEAPPLYMASTER;
                if (entView.T_FB_EXTENSIONALORDER == null)
                {
                    return;
                }


                if (entView.T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE != null)
                {
                    AuditEntity.ModelCode = entView.T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE.MODELCODE;
                    AuditEntity.FormID = entView.T_FB_EXTENSIONALORDER.ORDERID;
                }

            }
            else if (this.OrderEntity.OrderType == typeof(T_FB_BORROWAPPLYMASTER))
            {
                T_FB_BORROWAPPLYMASTER entView = orderEntity.Entity as T_FB_BORROWAPPLYMASTER;
                if (entView.T_FB_EXTENSIONALORDER == null)
                {
                    return;
                }

                if (entView.T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE != null)
                {
                    AuditEntity.ModelCode = entView.T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE.MODELCODE;
                    AuditEntity.FormID = entView.T_FB_EXTENSIONALORDER.ORDERID;
                }
            }
        }

        void AuditService_AuditFBEntityCompleted(object sender, AuditFBEntityCompletedEventArgs e)
        {
            DoAuditResult(e.Result);
            Saas.Tools.FlowWFService.SubimtFlowCompletedEventArgs args = GetFlowCompletedArgs(e);
            fbFlowService.OnSubimtFlowCompleted(args);
        }

        private void DoAuditResult(AuditResult result)
        {
            try
            {
                DataResult dataResult = result.DataResult;
                curAuditEventArgs = null;
                if (result.FBEntity != null)
                {

                    var newOrderEntity = new OrderEntity(result.FBEntity);
                    if (result.DataResult == null)
                    {
                        MessageBox.Show("流程返回结果为空，请联系管理员：" + result.Successful.ToString());
                    }
                    else
                    {
                        if (!(result.DataResult.FlowResult == FlowResult.SUCCESS || result.DataResult.FlowResult == FlowResult.END))
                        {
                            newOrderEntity.IsReSubmit = this.OrderEntity.IsReSubmit;
                        }
                        this.orderEntity = newOrderEntity;

                        this.SaveFBEntity = null;

                    }
                }
                else
                {
                    orderEntity = null;
                }

                if (!string.IsNullOrEmpty(result.Exception))
                {
                    curAuditEventArgs = new AuditEventArgs(AuditEventArgs.AuditResult.Cancel, null);
                    CommonFunction.ShowErrorMessage(result.Exception);

                }
                else if (dataResult.FlowResult == FlowResult.FAIL)
                {
                    curAuditEventArgs = new AuditEventArgs(AuditEventArgs.AuditResult.Error, null);
                    CommonFunction.ShowErrorMessage(dataResult.Err);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("流程返回结果后处理报错，请联系管理员：" + ex.ToString());
            }

        }

        public void Submit(FBEntity saveFBEntity)
        {
            this.SaveFBEntity = saveFBEntity;
            InitAudit(AuditOperation.Add);
            base.Submit();
        }

        public override event EventHandler<AuditEventArgs> AuditCompleted;
    }
    public class FBFlowService : IAuditService
    {
        public SMT.Saas.Tools.FlowWFService.ServiceClient flowService;

        public FBFlowService()
        {
            flowService = new Saas.Tools.FlowWFService.ServiceClient();
            flowService.GetFlowInfoCompleted += new EventHandler<Saas.Tools.FlowWFService.GetFlowInfoCompletedEventArgs>(flowService_GetFlowInfoCompleted);
        }




        public void GetFlowInfoAsync(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            flowService.GetFlowInfoAsync(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID);
        }

        public event EventHandler<Saas.Tools.FlowWFService.GetFlowInfoCompletedEventArgs> GetFlowInfoCompleted;
        void flowService_GetFlowInfoCompleted(object sender, Saas.Tools.FlowWFService.GetFlowInfoCompletedEventArgs e)
        {
            if (GetFlowInfoCompleted != null)
            {
                GetFlowInfoCompleted(this, e);
            }
        }

        public void SubimtFlowAsync(Saas.Tools.FlowWFService.SubmitData ApprovalData)
        {
            if (DoSubitFlowAction != null)
            {
                DoSubitFlowAction(ApprovalData);
            }
        }

        public event EventHandler<Saas.Tools.FlowWFService.SubimtFlowCompletedEventArgs> SubimtFlowCompleted;

        public Action<Saas.Tools.FlowWFService.SubmitData> DoSubitFlowAction;

        public void OnSubimtFlowCompleted(Saas.Tools.FlowWFService.SubimtFlowCompletedEventArgs args)
        {
            if (SubimtFlowCompleted != null)
            {
                SubimtFlowCompleted(this, args);
                
            }
        }


    }
}
