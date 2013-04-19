using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class VehicleUseAppForm : BaseForm,IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        private string editState = null;
        public string EditState { get { return editState; } set { editState = value; } }
        private T_OA_VEHICLEUSEAPP vehicleUsrApp = null;
        public T_OA_VEHICLEUSEAPP VehicleUsrApp { get { return vehicleUsrApp; } set { vehicleUsrApp = value; } }

        private bool isSubmitFlow = false;
        private FormTypes types;
        private string vapId = string.Empty;
        private SmtOACommonAdminClient _VM;
        private RefreshedTypes saveType;
        #endregion

        #region 构造函数
        public VehicleUseAppForm(FormTypes type, string vehicleuseappId)
        {
            InitializeComponent();
            this.types = type;
            this.vapId = vehicleuseappId;
            InitEvent();
            InitData();
        }
        #endregion

        #region InitData()
        private void InitData()
        {
            if (types == FormTypes.New)
            {
                VehicleUsrApp = new T_OA_VEHICLEUSEAPP();
                VehicleUsrApp.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            }
            else
            {
                _VM.GetVehicleUseAppByIdAsync(vapId);
            }
            SetForms();
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //if (types == FormTypes.Edit || types == FormTypes.Browse || types == FormTypes.Audit)
            //{
            //    //SetFormDefailValue(vehicleUsrApp);
            //    _VM.GetVehicleUseAppByIdAsync(vapId);
            //}
            //else
            //{
            //    dtiStartDate.DateTimeValue = System.DateTime.Now;
            //    dtiEndDate.DateTimeValue = dtiStartDate.DateTimeValue.AddHours(1);
            //    txtTel.Text = Common.CurrentLoginUserInfo.Telphone == null ? "" : Common.CurrentLoginUserInfo.Telphone;
            //}
            //SetForms();
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            _VM = new SmtOACommonAdminClient();
            _VM.AddVehicleUseAppCompleted += new EventHandler<AddVehicleUseAppCompletedEventArgs>(vehicleUseAppManager_AddVehicleUseAppCompleted);
            _VM.UpdateVehicleUseAppCompleted += new EventHandler<UpdateVehicleUseAppCompletedEventArgs>(vehicleUseAppManager_UpdateVehicleUseAppCompleted);
            _VM.GetVehicleUseAppByIdCompleted += new EventHandler<GetVehicleUseAppByIdCompletedEventArgs>(_VM_GetVehicleUseAppByIdCompleted);
        }

        void _VM_GetVehicleUseAppByIdCompleted(object sender, GetVehicleUseAppByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        VehicleUsrApp = e.Result;

                        if (VehicleUsrApp != null)
                        {
                            txtDepartmentId.Text = VehicleUsrApp.DEPARTMENTID;
                            string departName = GlobalFunction.GetDepartmentNameByID(VehicleUsrApp.DEPARTMENTID);
                            if (departName != null)
                            {
                                txtDepartName.Text = departName;
                            }
                            txtTel.Text = VehicleUsrApp.TEL;
                            dtiStartDate.DateTimeValue = Convert.ToDateTime(VehicleUsrApp.STARTTIME);
                            dtiEndDate.DateTimeValue = Convert.ToDateTime(VehicleUsrApp.ENDTIME);
                            txtNum.Text = VehicleUsrApp.NUM;
                            txtRoute.Text = VehicleUsrApp.ROUTE;
                            txtContent.Text = VehicleUsrApp.CONTENT == null ? "" : VehicleUsrApp.CONTENT;
                        }
                        if (types == FormTypes.Resubmit)//重新提交
                        {
                            VehicleUsrApp.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                        }
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region SetForms
        private void SetForms()
        {
            if (types == FormTypes.Audit || types == FormTypes.Browse)
            {
                txtContent.IsReadOnly = true;
                txtDepartmentId.IsReadOnly = true;
                txtNum.IsReadOnly = true;
                txtRoute.IsReadOnly = true;
                txtTel.IsReadOnly = true;
                dtiEndDate.IsReadOnly = true;
                dtiStartDate.IsReadOnly = true;
                btnLookUpDepartment.IsEnabled = false;
            }
        }
        #endregion

        #region SetFormDefailValue
        private void SetFormDefailValue(T_OA_VEHICLEUSEAPP defaultInfo)
        {
            if (defaultInfo != null)
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
            if (types == FormTypes.Resubmit)//重新提交
            {
                defaultInfo.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            }
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
        }
        #endregion

        private bool IsNumber(string textContent)
        {
            if (textContent.Trim() == null || textContent.Trim() == string.Empty)
            {
                return false;
            }
            foreach (char c in textContent)
            {
                if (!char.IsNumber(c))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsNullOrEmpty(string textContent)
        {
            if (textContent.Trim() == null || textContent.Trim() == string.Empty || textContent.Trim() == "")
            {
                return false;
            }
            return true;
        }

        private T_OA_VEHICLEUSEAPP GetEntityData(T_OA_VEHICLEUSEAPP inputEntity)
        {
            if (Check())
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
            return null;
        }

        private void AddInfo(T_OA_VEHICLEUSEAPP addInfo)
        {
            //addInfo.VEHICLEUSEAPPID = System.Guid.NewGuid().ToString();
            addInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            addInfo.CREATEDATE = System.DateTime.Now;
            addInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            addInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            addInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            addInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            addInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            addInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            addInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            addInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            addInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            addInfo.UPDATEDATE = System.DateTime.Now;
            addInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            
            // by LDX
            // 2011-08-08
            // 添加部门ID
            addInfo.DEPARTMENTID = txtDepartmentId.Text;

            _VM.AddVehicleUseAppAsync(addInfo);
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

            // by LDX
            // 2011-08-08
            // 添加部门ID
            updateInfo.DEPARTMENTID = txtDepartmentId.Text;

            _VM.UpdateVehicleUseAppAsync(updateInfo, "Edit");
        }

        #region 修改
        void vehicleUseAppManager_UpdateVehicleUseAppCompleted(object sender, UpdateVehicleUseAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
            if (e.Result > 0)
            {
                if (e.UserState.ToString() == "Edit")
                {
                    Utility.ShowMessageBox("UPDATE", isSubmitFlow, true);//修改成功
                    if (isSubmitFlow)
                        saveType = RefreshedTypes.CloseAndReloadData;
                    RefreshUI(saveType);
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    Utility.ShowMessageBox("AUDIT", isSubmitFlow, true);//审核成功
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    // by ldx
                    // 2011-08-09
                    // 将提示信息为英文改成中文
                    //Utility.ShowMessageBox("AUDITSUCCESSED", isSubmitFlow, true);//提交审核成功
                    Utility.ShowCustomMessage(MessageTypes.Message, "成功", "提交审核成功！");
                }
                RefreshUI(RefreshedTypes.All);
            }
            else
            {
                if (e.UserState.ToString() == "Audit" || e.UserState.ToString() == "Submit")
                {
                    Utility.ShowMessageBox("AUDITFAILURE", isSubmitFlow, false);//提交失败
                }
                else
                {
                    Utility.ShowMessageBox("UPDATE", isSubmitFlow, false);//修改失败
                }
            }
        }
        #endregion

        #region 新增
        void vehicleUseAppManager_AddVehicleUseAppCompleted(object sender, AddVehicleUseAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                types = FormTypes.Edit;
                Utility.ShowMessageBox("ADD", isSubmitFlow, true);
                if (isSubmitFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("ADD", isSubmitFlow, false);
            }
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VEHICLEUSEAPP");
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            if (!Check()) return;
            if (types == FormTypes.New)
            {
                vehicleUsrApp = new T_OA_VEHICLEUSEAPP();
                vehicleUsrApp.VEHICLEUSEAPPID = System.Guid.NewGuid().ToString();
                vehicleUsrApp.CHECKSTATE = "0";
            }
            RefreshUI(RefreshedTypes.ShowProgressBar);
            switch (actionType)
            {
                case "0":
                    saveType = RefreshedTypes.HideProgressBar;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
            }
        }

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
            ToolbarItem item = null;
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };
            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);


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
        //验证
        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = dtiStartDate.Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            //validators = dtiEndDate.Group1.ValidateAll();
            //if (validators.Count > 0)
            //{
            //    foreach (var h in validators)
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
            //        return false;
            //    }
            //}
            //validators = Group1.ValidateAll();
            //if (validators.Count > 0)
            //{
            //    foreach (var h in validators)
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
            //        return false;
            //    }
            //}
            string sta = string.Empty;
            sta = "0001-01-01";
            if (string.IsNullOrEmpty(dtiStartDate.DateTimeValue.ToShortDateString()))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                return false;
            }
            else
            {
                string strStartDate = dtiStartDate.DateTimeValue.ToString("yyyy-MM-dd");
                if (strStartDate == sta)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                    return false;
                }
            }
            if (string.IsNullOrEmpty(dtiEndDate.DateTimeValue.ToShortDateString()))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                return false;
            }
            else
            {
                string strEndDate = dtiEndDate.DateTimeValue.ToString("yyyy-MM-dd");
                if (strEndDate == sta)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    return false;
                }
            }
            if (dtiStartDate.DateTimeValue >= dtiEndDate.DateTimeValue)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                return false;
            }
            return true;
        }

        private void Save()
        {
            if (vehicleUsrApp != null)
            {
                if (GetEntityData(vehicleUsrApp) != null)
                {
                    if (types == FormTypes.New)
                        AddInfo(vehicleUsrApp);
                    else
                        UpdateInfo(vehicleUsrApp);
                }
            }
        }

        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region 选择部门
        private void btnLookUpDepartment_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    if (companyInfo.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department)
                    {
                        txtDepartmentId.Text = companyInfo.ObjectID;
                        txtDepartName.Text = companyInfo.ObjectName;
                    }
                    else
                    {
                        txtDepartmentId.Text = string.Empty;
                    }
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_VEHICLEUSEAPP>(VehicleUsrApp, "OA");
            Utility.SetAuditEntity(entity, "T_OA_VEHICLEUSEAPP", VehicleUsrApp.VEHICLEUSEAPPID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (VehicleUsrApp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            VehicleUsrApp.CHECKSTATE = state;

            _VM.UpdateVehicleUseAppAsync(VehicleUsrApp, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (VehicleUsrApp != null)
                state = VehicleUsrApp.CHECKSTATE;
            if (types == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
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