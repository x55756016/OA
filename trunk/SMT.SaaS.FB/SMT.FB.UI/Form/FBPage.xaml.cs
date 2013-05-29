using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.FB.UI.Common;
using SMT.FB.UI.FBCommonWS;
using SMT.FB.UI.Common.Controls;
using System.Collections;
using SMT.SaaS.FrameworkUI;
using SMT.FB.UI.Form.DailyManagement;
using SMT.FB.UI.Form.BudgetApply;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.FB.UI.Form
{


    [FBPageEditor(typeof(T_FB_CHARGEAPPLYMASTER), typeof(ChargeApplyForm))]
    [FBPageEditor(typeof(T_FB_TRAVELEXPAPPLYMASTER), typeof(TravelApplyForm))]
    [FBPageEditor(typeof(T_FB_BORROWAPPLYMASTER), typeof(BorrowApplyForm))]
    [FBPageEditor(typeof(T_FB_REPAYAPPLYMASTER), typeof(RepayApplyForm))]

    [FBPageEditor(typeof(T_FB_COMPANYBUDGETAPPLYMASTER), typeof(CompanyBudgetApplyForm))]
    [FBPageEditor(typeof(T_FB_COMPANYBUDGETMODMASTER), typeof(CompanyBudgetModForm))]
    [FBPageEditor(typeof(T_FB_COMPANYTRANSFERMASTER), typeof(CompanyTransferAppForm))]
    [FBPageEditor(typeof(T_FB_COMPANYBUDGETSUMMASTER), typeof(CompanyBudgetSumForm))]

    [FBPageEditor(typeof(T_FB_DEPTTRANSFERMASTER), typeof(DepartmentTransferAppForm))]
    [FBPageEditor(typeof(T_FB_DEPTBUDGETAPPLYMASTER), typeof(DeptBudgetApplyForm))]
    [FBPageEditor(typeof(T_FB_DEPTBUDGETADDMASTER), typeof(DeptBudgetAddForm))]
    [FBPageEditor(typeof(T_FB_DEPTBUDGETSUMMASTER), typeof(DeptBudgetSumForm))]
    [FBPageEditor(typeof(T_FB_PERSONMONEYASSIGNMASTER), typeof(PersonMoneyAssignForm))]
    [FBPageEditor(typeof(T_FB_SUMSETTINGSMASTER), typeof(SumSettingsForm))]
    //[FBPageEditor(typeof(T_FB_PERSONBUDGETAPPLYMASTER), typeof(PersonBudgetApplyForm))]
    //[FBPageEditor(typeof(T_FB_PERSONBUDGETADDMASTER), typeof(PersonBudgetAddForm))]


    public partial class FBPage : FBBaseControl, IEntityEditor
    {
        #region 静态方法
        static FBPage()
        {
            Type type = typeof(FBPage);
            object[] attrs = type.GetCustomAttributes(typeof(FBPageEditor), true);
            _EditorList = attrs.ToObjectList<FBPageEditor>();
        }
        private static List<FBPageEditor> _EditorList;

        public static FBPage GetPage(OrderEntity orderEntity)
        {

            Type typeOfEditor = typeof(FBPage);
            FBPageEditor fbPageEditor = _EditorList.FirstOrDefault(item =>
                {
                    return item.OrderType == orderEntity.OrderType;
                });

            if (fbPageEditor != null)
            {
                typeOfEditor = fbPageEditor.EditorType;
            }
            try
            {
                FBPage page = (FBPage)Activator.CreateInstance(typeOfEditor, orderEntity);
                page.InitForm();
                return page;
            }
            catch (Exception ex)
            {
                throw ex;
                //return new FBPage(orderEntity);
            }
        }
        #endregion

        private bool IsClose = false;

        public FBPage()
        {
            InitializeComponent();


            IsNeedToRefresh = false;
            // this.KeyUp += new KeyEventHandler(OrderForm_KeyUp);
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.FBBasePageLoaded += new EventHandler(FBPage_FBBasePageLoaded);
        }

        public FBPage(OrderEntity orderEntity)
            : this()
        {

            this.EditForm.LoadDataComplete += new EventHandler(EditForm_LoadDataComplete);
            this.EditForm.LoadControlComplete += new EventHandler(EditForm_LoadControlComplete);
            this.EditForm.SaveCompleted += new EventHandler<SavingEventArgs>(EditForm_SaveCompleted);
            // this.EditForm.OrderEntityChanged += new EventHandler(EditForm_OrderEntityChanged);
            this.EditForm.OrderEntity = orderEntity;
            this.auditControl.AuditCompleted += new EventHandler<AuditEventArgs>(auditControl_AuditCompleted);
        }



        void FBPage_FBBasePageLoaded(object sender, EventArgs e)
        {
            try
            {
                ShowProcess();
                this.editForm.InitForm();
            }
            catch (Exception ex)
            {
                CommonFunction.ShowErrorMessage(ex.ToString());
                CloseProcess();
            }
        }
        //void EditForm_OrderEntityChanged(object sender, EventArgs e)
        //{
        //    this.InitAudit();
        //}

        void OrderForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (this.OrderEntity.FBEntityState != FBEntityState.Unchanged)
                {
                    Action actionSave = () =>
                    {
                        DoAction("SaveAndClose");
                    };
                    CommonFunction.NotifySaveB4Close(string.Empty, actionSave, null);
                    return;
                }
                //this.DialogResult = false;
            }
        }

        protected virtual void OnLoadDataComplete()
        {

        }

        protected virtual void OnLoadControlComplete()
        {

        }

        protected virtual void OnAuditing(object sender, AuditEventArgs e)
        {
        }

        #region 属性

        public string OrderMessage
        {
            get
            {
                return this.tbMessage.Text;
            }
            set
            {
                this.tbMessage.Text = value;
            }
        }
        public OrderEntity OrderEntity
        {
            get
            {
                return editForm.OrderEntity;
            }

        }

        public EditForm EditForm
        {
            get
            {
                return editForm;
            }
        }

        public bool IsNeedToRefresh { get; set; }

        public event EventHandler PageClosing;

        public event EventHandler RefreshData;

        #endregion
        public void InitForm()
        {
            this.auditControl.Auditing += new EventHandler<AuditEventArgs>(auditControl_Auditing);
        }



        private void EditForm_LoadControlComplete(object sender, EventArgs e)
        {
            if (this.OrderEntity.FBEntityState != FBEntityState.Added)
            {
                ComboBox cbb = this.EditForm.FindControl("OwnerCompanyID") as ComboBox;
                if (cbb != null)
                {
                    cbb.IsEnabled = false;
                }

                //cbb = this.EditForm.FindControl("OwnerDepartmentID") as ComboBox;
                //if (cbb != null)
                //{
                //    cbb.IsEnabled = false;
                //}
            }

            //  OnDetaiGridDelete(); // 临时处理方法
            OnLoadControlComplete();
            CloseProcess();
        }

        private void EditForm_LoadDataComplete(object sender, EventArgs e)
        {
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                string code = "<自动生成>";
                this.OrderEntity.SetObjValue("Entity." + this.OrderEntity.CodeName, code);
            }
            this.InitAudit();
            OnLoadDataComplete();
        }

        private void EditForm_SaveCompleted(object sender, SavingEventArgs e)
        {
            CloseProcess();

            if (e.Action == Actions.Save)
            {
                CommonFunction.ShowMessage("保存成功!");
            }

            if (e.Action != Actions.Cancel)
            {
                CommonFunction.ShowMessage("保存成功!");
                OnClose();
            }




        }


        //private void OnDetaiGridDelete()
        //{
        //    DetailGrid dGrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
        //    if (dGrid != null)
        //    {
        //        dGrid.ToolBarItemClick += (sa, ea) =>
        //        {
        //            if (ea.Action == Actions.Delete)
        //            {
        //                IList<FBEntity> listSource = dGrid.ItemsSource as IList<FBEntity>;
        //                IList list = dGrid.ADGrid.SelectedItems as IList;
        //                for (int i = 0; i < list.Count; i++)
        //                {
        //                    FBEntity entity = list[i] as FBEntity;
        //                    listSource.Remove(entity);
        //                }
        //            }
        //        };
        //    }
        //}

        #region IEntityEditor 成员

        public string GetTitle()
        {
            // return Utility.GetResourceStr("PUBLICVACATIONFORM");
            return this.OrderEntity.OrderInfo.Name;
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {

            ShowProcess();
            switch (actionType)
            {
                case "Save":
                    IsClose = false;
                    IsNeedToRefresh |= this.EditForm.Save();
                    break;
                case "SaveAndClose":
                    IsClose = true;
                    IsNeedToRefresh |= this.EditForm.Save();

                    break;
                case "Delete":
                    CloseProcess();
                    Action action = () =>
                    {
                        ShowProcess();
                        this.OrderEntity.FBEntityState = FBEntityState.Deleted;
                        IsNeedToRefresh |= this.EditForm.Save();
                        this.IsClose = true;

                    };

                    CommonFunction.AskDelete(string.Empty, action);
                    break;
                case "Submit":
                    //IsClose = true;
                    IsNeedToRefresh |= SubmitAudit();

                    break;
                case "Cancel":

                    Action actionCancel = () =>
                    {
                        IsClose = true;
                        OnClose();
                    };

                    if (this.OrderEntity.FBEntityState != FBEntityState.Unchanged)
                    {
                        Action actionSave = () =>
                        {
                            DoAction("SaveAndClose");
                        };

                        CommonFunction.NotifySaveB4Close(string.Empty, actionSave, actionCancel);
                        return;
                    }
                    else
                    {
                        actionCancel();
                    }

                    break;
            }

        }



        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            //NavigateItem item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("BASEINFO"),
            //    Tooltip = Utility.GetResourceStr("BASEINFO")
            //};
            //items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            bool canSave = true;
            bool canSubmit = true;
            List<ToolbarItem> items = new List<ToolbarItem>();
            switch (this.EditForm.OperationType)
            {
                case OperationTypes.Browse:
                    canSave &= false;
                    canSubmit &= false;
                    break;
                case OperationTypes.Audit:
                    canSave &= false;
                    canSubmit &= false;
                    break;
                case OperationTypes.ReSubmit:
                    canSave = false;
                    canSubmit = true;
                    break;
            }
            if (!IsUnSubmit)
            {
                canSave &= false;
                canSubmit &= false;
            }
            else if ((this.OrderEntity.OrderType == typeof(T_FB_DEPTBUDGETSUMMASTER) ||
                     this.OrderEntity.OrderType == typeof(T_FB_COMPANYBUDGETSUMMASTER)))
            {
                canSave &= false;
            }

            if (this.EditForm.OperationType == OperationTypes.ReSubmit)
            {
                object states = this.OrderEntity.GetObjValue(EntityFieldName.CheckStates);
                SMT.FB.UI.FBCommonWS.CheckStates currentStates = CommonFunction.TryConvertValue<SMT.FB.UI.FBCommonWS.CheckStates>(states);
                if (currentStates == FBCommonWS.CheckStates.UnApproved)
                {
                    canSubmit = true;
                }

                if ((currentStates == FBCommonWS.CheckStates.Approved)
                        && (this.OrderEntity.OrderType == typeof(T_FB_DEPTBUDGETAPPLYMASTER) ||
                           this.OrderEntity.OrderType == typeof(T_FB_COMPANYBUDGETAPPLYMASTER)))
                {
                    var isValid = Convert.ToString(this.OrderEntity.Entity.GetObjValue("ISVALID"));
                    if (isValid == "2")
                    {
                        canSubmit = true;
                    }
                }


            }

            if (canSave)
            {
                // items.Add(ToolBarItems.SaveAndClose);
                items.Add(ToolBarItems.Save);
            }

            //预算汇总设置不需要提交
            if (this.OrderEntity.OrderType == typeof(T_FB_SUMSETTINGSMASTER))
                canSubmit = false;

            if (canSubmit)
            {
                items.Add(ToolBarItems.Submit);
            }

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        private void OnRefreshData()
        {
            if (IsNeedToRefresh && RefreshData != null)
            {
                RefreshData(this, null);
            }
        }

        private void OnClose()
        {
            if (IsClose && PageClosing != null)
            {
                OnRefreshData();
                PageClosing(this, null);
            }
        }

        #region 审核

        protected bool IsUnSubmit
        {
            get
            {
                object states = this.OrderEntity.GetObjValue(EntityFieldName.CheckStates);
                SMT.FB.UI.FBCommonWS.CheckStates currentStates = CommonFunction.TryConvertValue<SMT.FB.UI.FBCommonWS.CheckStates>(states);
                return currentStates == SMT.FB.UI.FBCommonWS.CheckStates.UnSubmit;
            }
        }

        private void InitAudit()
        {
            this.auditControl.OrderEntity = this.OrderEntity;
            if (IsUnSubmit)
            {
                this.PnlAudit.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.PnlAudit.Visibility = Visibility.Visible;
                this.auditControl.BindingData();
            }
        }

        private bool SubmitAudit()
        {
            FBEntity saveFBEntity = this.OrderEntity.GetModifiedFBEntity();
            string ownerID = Convert.ToString(this.OrderEntity.GetObjValue(EntityFieldName.OwnerID));
            if ((ownerID != DataCore.CurrentUser.Value.ToString()) && (ownerID != DataCore.SuperUser.Value.ToString()))
            {
                CommonFunction.ShowMessage(Utility.GetResourceStr("Msg_NoSubmitPerson"));
                CloseProcess();
            }
            else if (this.EditForm.CheckBeforeSave(saveFBEntity))
            {
                this.EditForm.SaveData();
                auditControl.Submit(saveFBEntity);
                return true;
            }
            else
            {
                CloseProcess();
            }
            return false;
        }

        void auditControl_Auditing(object sender, AuditEventArgs e)
        {
            ShowProcess();
            OnAuditing(sender, e);
        }

        void auditControl_AuditCompleted(object sender, AuditEventArgs e)
        {
            CloseProcess();

            if (e.Action == AuditEventArgs.AuditAction.Audit)
            {
                if (e.Result != AuditEventArgs.AuditResult.Error)
                {
                    CommonFunction.ShowMessage("审核成功!");
                }
            }
            else if (e.Action == AuditEventArgs.AuditAction.Submit)
            {
                CommonFunction.ShowMessage("提交成功!");
            }
            else if (e.Result == AuditEventArgs.AuditResult.Cancel)
            {
                return;
            }
            if (auditControl.OrderEntity != null)
            {
                this.EditForm.OrderEntity = auditControl.OrderEntity;
            }
            this.EditForm.AuditinitForm(true);
            IsNeedToRefresh = true;

            //            IsClose = true;
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            OnClose();

        }


        #endregion


        protected void ChangeCreator()
        {
            OrderEntity orderEntity = this.OrderEntity;

            if (this.EditForm.OperationType == OperationTypes.Add)
            {
                var curentLoginUser = this.OrderEntity.LoginUser;
                #region 判断是否与创建人一致
                var posts = (this.OrderEntity.LoginUser as LoginUserData).PostInfos;
                EmployeerData ownerInfo = orderEntity.GetOwnerInfo();

                var finds = posts.Where(item => item.Company.Value.ToString() == ownerInfo.Company.Value.ToString());
                if (finds.Count() > 0)
                {
                    curentLoginUser = finds.First();
                }

                finds = finds.Where(item => item.Department.Value.ToString() == ownerInfo.Department.Value.ToString());
                if (finds.Count() > 0)
                {
                    curentLoginUser = finds.First();
                }

                finds = finds.Where(item => item.Post.Value.ToString() == ownerInfo.Post.Value.ToString());
                if (finds.Count() > 0)
                {
                    curentLoginUser = finds.First();
                }

                #endregion

                if (curentLoginUser != this.OrderEntity.LoginUser)
                {
                    EmployeerData CreateUser = curentLoginUser;

                    orderEntity.SetObjValue("Entity.CREATECOMPANYID", CreateUser.Company.Value);
                    orderEntity.SetObjValue("Entity.CREATECOMPANYNAME", CreateUser.Company.Text);

                    orderEntity.SetObjValue("Entity.CREATEDEPARTMENTID", CreateUser.Department.Value);
                    orderEntity.SetObjValue("Entity.CREATEDEPARTMENTNAME", CreateUser.Department.Text);

                    orderEntity.SetObjValue("Entity.CREATEPOSTID", CreateUser.Post.Value);
                    orderEntity.SetObjValue("Entity.CREATEPOSTNAME", CreateUser.Post.Text);
                    orderEntity.LoginUser = CreateUser;
                }
            }


        }

        protected void ChangeOwnerID()
        {
            EmployeerData create = this.OrderEntity.GetCreateInfo();
            EmployeerData owner = this.OrderEntity.GetOwnerInfo();

            if (!object.Equals(create.Department.Value, owner.Department.Value) && !string.IsNullOrEmpty(owner.Department.Value.ToString()))
            {
                SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient ps = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
                ps.GetDepartmentLeadersCompleted += (o, e) =>
                    {
                        if (e.Result != null && e.Result.Count > 0)
                        {
                            this.OrderEntity.SetObjValue(EntityFieldName.OwnerID, e.Result[0].T_HR_EMPLOYEE.EMPLOYEEID);
                            this.OrderEntity.SetObjValue(EntityFieldName.OwnerName, e.Result[0].T_HR_EMPLOYEE.EMPLOYEECNAME);
                            this.OrderEntity.SetObjValue(EntityFieldName.OwnerPostID, e.Result[0].T_HR_POST.POSTID);
                        }
                        else
                        {
                            CommonFunction.ShowMessage("找不到相应的部门负责人");
                            //this.OrderEntity.SetObjValue(EntityFieldName.OwnerDepartmentID, null);
                            //this.OrderEntity.SetObjValue(EntityFieldName.OwnerDepartmentName, null);
                            this.OrderEntity.ReferencedData[EntityFieldName.OwnerDepartmentID] = null;
                            OnOwnerIsNotReady();
                        }
                        this.CloseProcess();
                    };

                ps.GetDepartmentLeadersAsync(owner.Department.Value.ToString());
                this.ShowProcess();
            }
            else
            {

                this.OrderEntity.SetObjValue(EntityFieldName.OwnerID, create.Value);
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerName, create.Text);
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerPostID, create.Post.Value);
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerPostName, create.Post.Text);
            }
        }

        protected virtual void OnOwnerIsNotReady()
        {
        }

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class FBPageEditor : Attribute
    {
        public Type OrderType { get; set; }
        public Type EditorType { get; set; }

        public FBPageEditor(Type orderType, Type editorType)
        {
            this.OrderType = orderType;
            this.EditorType = editorType;
        }
    }

}
