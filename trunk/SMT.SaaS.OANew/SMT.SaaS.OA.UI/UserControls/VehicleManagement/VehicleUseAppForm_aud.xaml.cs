using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class VehicleUseAppForm_aud : BaseForm,IClient, IEntityEditor
    {
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        public VehicleUseAppForm_aud()
        {
            InitializeComponent();
            _VM.UpdateVehicleUseAppCompleted += new EventHandler<UpdateVehicleUseAppCompletedEventArgs>(vehicleUseAppManager_UpdateVehicleUseAppCompleted);
        }
        //平台调用
        public VehicleUseAppForm_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            _VM.UpdateVehicleUseAppCompleted += new EventHandler<UpdateVehicleUseAppCompletedEventArgs>(vehicleUseAppManager_UpdateVehicleUseAppCompleted);
            _VM.Get_VehicleUseAppCompleted += new EventHandler<Get_VehicleUseAppCompletedEventArgs>(Get_VehicleUseAppCompleted);
            _VM.Get_VehicleUseAppAsync(SendDocID);
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Load_Data();
        }

        private void Load_Data()
        {
            if (vehicleUsrApp != null)
            {
                SetFormDefailValue(vehicleUsrApp);
                viewApproval.XmlObject = DataObjectToXml<T_OA_VEHICLEUSEAPP>.ObjListToXml(vehicleUsrApp, "OA");
            }
            SetForms();
        }
        private void SetForms()
        {
            txtContent.IsReadOnly = true;
            txtDepartmentId.IsReadOnly = true;
            txtNum.IsReadOnly = true;
            txtRoute.IsReadOnly = true;
            txtTel.IsReadOnly = true;
            dtiEndDate.IsReadOnly = true;
            dtiStartDate.IsReadOnly = true;
            btnLookUpDepartment.IsEnabled = false;
            slvView.Height = 200;
            InitAudit(vehicleUsrApp.VEHICLEUSEAPPID);
        }

        void vehicleUseAppManager_UpdateVehicleUseAppCompleted(object sender, UpdateVehicleUseAppCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                Utility.ShowMessageBox("AUDITSUCCESSED", true, true);
                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
            else
            {
                Utility.ShowMessageBox("AUDITFAILURE", true, false);
                RefreshUI(RefreshedTypes.ProgressBar);
            }
        }
        void Get_VehicleUseAppCompleted(object sender, Get_VehicleUseAppCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                vehicleUsrApp = e.Result;
                Load_Data();
            }
        }
        private T_OA_VEHICLEUSEAPP GetEntityData(T_OA_VEHICLEUSEAPP inputEntity)
        {
            inputEntity.CONTENT = txtContent.Text;
            inputEntity.STARTTIME = dtiStartDate.DateTimeValue;
            inputEntity.ENDTIME = dtiEndDate.DateTimeValue;
            inputEntity.NUM = txtNum.Text;
            inputEntity.ROUTE = txtRoute.Text;
            inputEntity.DEPARTMENTID = txtDepartmentId.Text;
            inputEntity.TEL = txtTel.Text;
            return inputEntity;
        }
        private void UpdateInfo(T_OA_VEHICLEUSEAPP updateInfo)
        {
            updateInfo.UPDATEDATE = System.DateTime.Now;
            updateInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            updateInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            updateInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            updateInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            updateInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            updateInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _VM.UpdateVehicleUseAppAsync(updateInfo);
        }


        private string editState = null;
        public string EditState
        {
            get { return editState; }
            set { editState = value; }
        }

        private T_OA_VEHICLEUSEAPP vehicleUsrApp = null;
        public T_OA_VEHICLEUSEAPP VehicleUsrApp
        {
            get { return vehicleUsrApp; }
            set { vehicleUsrApp = value; }
        }

        private void SetFormDefailValue(T_OA_VEHICLEUSEAPP defaultInfo)
        {
            txtDepartmentId.Text = defaultInfo.DEPARTMENTID;
            string departName = GlobalFunction.GetDepartmentNameByID(defaultInfo.DEPARTMENTID);
            if (departName != null)
            {
                txtDepartName.Text = departName;
            }
            txtTel.Text = defaultInfo.TEL;
            dtiStartDate.DateTimeValue = Convert.ToDateTime(defaultInfo.STARTTIME);
            dtiEndDate.DateTimeValue = Convert.ToDateTime(defaultInfo.ENDTIME);
            txtNum.Text = defaultInfo.NUM;
            txtRoute.Text = defaultInfo.ROUTE;
            txtContent.Text = defaultInfo.CONTENT == null ? "" : defaultInfo.CONTENT;
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VEHICLEUSEAPP");
        }

        public string GetStatus() { return ""; }

        public void DoAction(string actionType) { }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 确定、取消

        private void Save()
        {
            if (vehicleUsrApp != null)
                if (GetEntityData(vehicleUsrApp) != null)
                    UpdateInfo(vehicleUsrApp);
        }

        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region 流程
        /// <summary>
        /// 提交流程
        /// </summary>
        private void SumbitFlow()
        {
            if (vehicleUsrApp != null)
            {
                SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.viewApproval.AuditEntity;
                entity.ModelCode = "T_OA_VEHICLEUSEAPP";
                entity.FormID = vehicleUsrApp.VEHICLEUSEAPPID;
                entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
                entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
                entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
                entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
                viewApproval.Submit();
            }
        }

        private void InitAudit(string entityID)
        {
            SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.viewApproval.AuditEntity;
            entity.ModelCode = "T_OA_VEHICLEUSEAPP";
            entity.FormID = entityID;
            entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
            entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
            entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
            viewApproval.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
            viewApproval.BindingData();
        }
        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
        }
        private void SumbitCompleted()
        {
            try
            {
                if (vehicleUsrApp != null)
                {
                    vehicleUsrApp.UPDATEDATE = DateTime.Now;
                    vehicleUsrApp.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    vehicleUsrApp.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                            vehicleUsrApp.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                            vehicleUsrApp.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                            vehicleUsrApp.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }
                    Save();
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        private void Cancel()
        {
            this.Close();
        }
        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            this.Close();
        }
        private void Close()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;

        /// <summary>
        /// 提交审核完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void audit_AuditCompleted(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            auditResult = e.Result;
            switch (auditResult)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    //todo 审核中
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel:
                    //todo 取消
                    Cancel();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    //todo 终审通过
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    //todo 审核不通过
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Error:
                    //todo 审核异常
                    HandError();
                    break;
            }
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}