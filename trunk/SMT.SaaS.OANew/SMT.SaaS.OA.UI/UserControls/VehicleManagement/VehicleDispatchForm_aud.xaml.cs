using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PersonnelWS;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class VehicleDispatchForm_aud : BaseForm,IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        /// <summary>
        /// 存放选中的派车单
        /// </summary>
        private List<T_OA_VEHICLEDISPATCH> _lstVDispatch = new List<T_OA_VEHICLEDISPATCH>();
        /// <summary>
        /// 存放新增的用车申请单
        /// </summary>
        private List<T_OA_VEHICLEUSEAPP> _lstVUseApp_Add = new List<T_OA_VEHICLEUSEAPP>();
        /// <summary>
        /// 包含 新增和 原有的
        /// </summary>
        private List<T_OA_VEHICLEUSEAPP> _lstVUseApp = new List<T_OA_VEHICLEUSEAPP>();

        

        /// <summary>
        /// 车辆信息
        /// </summary>
        public List<T_OA_VEHICLE> _lstVehicle = new List<T_OA_VEHICLE>();
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        private string vdId = string.Empty;
        private bool isSubmitFlow = false;
        private FormTypes types;
        #endregion

        #region 构造函数
        public VehicleDispatchForm_aud()
        {
            InitializeComponent();
            _VM.UpdateVehicleDispatchAndDetailCompleted += new EventHandler<UpdateVehicleDispatchAndDetailCompletedEventArgs>(vehicleDispatchManagr_UpdateVehicleDispatchAndDetailCompleted);
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(GetVehicleInfoListCompleted);
            _VM.Get_ByParentIDCompleted += new EventHandler<Get_ByParentIDCompletedEventArgs>(Get_ByParentIDCompleted);
        }
        public VehicleDispatchForm_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.vdId = SendDocID;
            this.types = operationType;
            _VM.UpdateVehicleDispatchAndDetailCompleted += new EventHandler<UpdateVehicleDispatchAndDetailCompletedEventArgs>(vehicleDispatchManagr_UpdateVehicleDispatchAndDetailCompleted);
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(GetVehicleInfoListCompleted);
            _VM.Get_ByParentIDCompleted += new EventHandler<Get_ByParentIDCompletedEventArgs>(Get_ByParentIDCompleted);
            _VM.Get_VDInfoCompleted += new EventHandler<Get_VDInfoCompletedEventArgs>(Get_VDInfoCompleted);
            _VM.Get_VDInfoAsync(vdId);
            
        }

        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //Load_Data();
        }
        #endregion

        #region Load_Data
        private void Load_Data()
        {
            _VM.GetVehicleInfoListAsync();

            
            SetForms();
        }
        #endregion

        #region SetForms
        private void SetForms()
        {
            cmbVehicleInfo.IsEnabled = false;
            txtContent.IsReadOnly = true;
            txtDriverID.IsReadOnly = true;
            txtNum.IsReadOnly = true;
            txtRoute.IsReadOnly = true;
            txtTel.IsReadOnly = true;
            dtiEndDate.IsReadOnly = true;
            dtiStartDate.IsReadOnly = true;
            dg.IsReadOnly = true;
        }
        #endregion

        #region 获取车辆 2
        private void GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleList = e.Result;
            if (vehicleList != null)
            {
                _lstVehicle = new List<T_OA_VEHICLE>(vehicleList);
                if (_lstVehicle != null)
                {
                    if (vehicleDispatch.T_OA_VEHICLEReference.EntityKey != null)
                    {
                        SetComboBoxSelect(vehicleList
                               , vehicleDispatch.T_OA_VEHICLEReference.EntityKey.EntityKeyValues[0].Value.ToString());
                    }
                }
                    //SetComboBoxSelect(vehicleList, 
                    //    (new System.Collections.Generic.List<SmtOACommonAdminService.EntityKeyMember>(
                    //        ((SmtOACommonAdminService.EntityReference)
                    //        (VehicleDispatch.T_OA_VEHICLEReference)).EntityKey.EntityKeyValues))[0].Value.ToString());
            }
        }
        #endregion

        #region 修改
        private void vehicleDispatchManagr_UpdateVehicleDispatchAndDetailCompleted(object sender, UpdateVehicleDispatchAndDetailCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                if (e.Result > 0)
                {
                    // by ldx
                    // 2011-08-09
                    // 将提示信息为提交成功改成审核通过
                    //Utility.ShowMessageBox("AUDIT", isSubmitFlow, true);//审核成功
                    Utility.ShowCustomMessage(MessageTypes.Message, "成功", "审核通过！");
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
                else
                {
                    Utility.ShowMessageBox("FAILURETOAPPROVE", isSubmitFlow, false);//审核失败
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 获取用车申请
        private void vehicleInfoManager_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfoList = e.Result;
            if (vehicleInfoList != null)
            {
                foreach (T_OA_VEHICLE obj in vehicleInfoList)
                    cmbVehicleInfo.Items.Add(obj);
                cmbVehicleInfo.DisplayMemberPath = "VIN";
                // SetComboBoxSelect(vehicleInfoList, (new System.Collections.Generic.List<SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityKeyMember>(((SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityReference)(vehicleDispatch.T_OA_VEHICLEReference)).EntityKey.EntityKeyValues))[0].Value.ToString());
                SetComboBoxSelect(vehicleInfoList, null);
            }
        }
        #endregion

        #region SetComboBoxSelect
        private void SetComboBoxSelect(ObservableCollection<T_OA_VEHICLE> cmbData, string assetId)
        {
            cmbVehicleInfo.Items.Clear();
            T_OA_VEHICLE selectObj = null;
            foreach (T_OA_VEHICLE obj in cmbData)
            {
                cmbVehicleInfo.Items.Add(obj);
                if (obj.ASSETID == assetId)
                {
                    selectObj = obj;
                }
            }
            cmbVehicleInfo.DisplayMemberPath = "VIN";
            if (selectObj != null)
            {
                cmbVehicleInfo.SelectedItem = selectObj;
            }
            else
            {
                cmbVehicleInfo.SelectedIndex = 0;
            }
        }
        #endregion

        #region 根据ID获取派车单
        void Get_VDInfoCompleted(object sender, Get_VDInfoCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                ObservableCollection<T_OA_VEHICLEDISPATCH> o = e.Result;
                VehicleDispatch = o[0];
                SetFormDefalueValue(VehicleDispatch);

                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                Load_Data();   //add by zl
            }
        }
        #endregion

        #region 修改函数
        private void UpdatevehicleDispatchUpdateInfo(T_OA_VEHICLEDISPATCH vInfo)
        {
            vInfo.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleInfo.SelectedItem;
            // vInfo.CHECKSTATE = "0";
            vInfo.CONTENT = txtContent.Text;
            vInfo.DRIVER = txtDriverID.Text;
            vInfo.ENDTIME = dtiEndDate.DateTimeValue;
            vInfo.ISCANCEL = "1";
            vInfo.NUM = txtNum.Text;
            vInfo.ROUTE = txtRoute.Text;
            vInfo.STARTTIME = dtiStartDate.DateTimeValue;
            vInfo.TEL = txtTel.Text;
            vInfo.UPDATEDATE = System.DateTime.Now;
            vInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

            vInfo.OWNERCOMPANYID = vehicleDispatch.OWNERCOMPANYID;
            vInfo.OWNERDEPARTMENTID = vehicleDispatch.OWNERDEPARTMENTID;
            vInfo.OWNERID = vehicleDispatch.OWNERID;
            vInfo.OWNERNAME = vehicleDispatch.OWNERNAME;
            vInfo.OWNERPOSTID = vehicleDispatch.OWNERPOSTID;

            ObservableCollection<T_OA_VEHICLEDISPATCHDETAIL> lstDetail = new ObservableCollection<T_OA_VEHICLEDISPATCHDETAIL>();
            foreach (var v in _lstVUseApp_Add)
            {
                T_OA_VEHICLEDISPATCHDETAIL info = new T_OA_VEHICLEDISPATCHDETAIL();
                info.VEHICLEDISPATCHDETAILID = System.Guid.NewGuid().ToString();
                // info.T_OA_VEHICLEDISPATCH = vInfo;
                info.T_OA_VEHICLEUSEAPP = v;

                info.CREATEDATE = DateTime.Now;
                info.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                info.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                info.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                info.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                info.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

                info.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                info.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                info.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                info.OWNERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                info.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                lstDetail.Add(info);
            }
            _VM.UpdateVehicleDispatchAndDetailAsync(vInfo, lstDetail);
        }
        #endregion

        private T_OA_VEHICLEDISPATCH vehicleDispatch = null;
        public T_OA_VEHICLEDISPATCH VehicleDispatch
        {
            get { return vehicleDispatch; }
            set { vehicleDispatch = value; }
        }

        #region SetFormDefalueValue
        private void SetFormDefalueValue(T_OA_VEHICLEDISPATCH vehDispatchInfo)
        {
            txtContent.Text = vehDispatchInfo.CONTENT;
            txtDriverID.Text = vehDispatchInfo.DRIVER;
            txtDriverName.Text = vehDispatchInfo.DRIVER;
            txtNum.Text = vehDispatchInfo.NUM;
            txtRoute.Text = vehDispatchInfo.ROUTE;
            txtTel.Text = vehDispatchInfo.TEL;
            dtiStartDate.DateTimeValue = Convert.ToDateTime(vehDispatchInfo.STARTTIME);
            dtiEndDate.DateTimeValue = Convert.ToDateTime(vehDispatchInfo.ENDTIME);

            //加载已经派车的申请用车数据
            _VM.Get_ByParentIDAsync(vehDispatchInfo.VEHICLEDISPATCHID);

            PersonnelServiceClient client = new PersonnelServiceClient();
            client.GetEmployeeByIDAsync(vehDispatchInfo.DRIVER);
            client.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(client_GetEmployeeByIDCompleted);
        }
        #endregion

        #region 获取用户信息
        void client_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Result != null)
                txtDriverName.Text = ((T_HR_EMPLOYEE)e.Result).EMPLOYEECNAME;
        }

        void client_GetEducateHistoryAllCompleted(object sender, GetEducateHistoryAllCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
       
        #region 获取申请列表

        //获取已经派车的申请用车数据
        void Get_ByParentIDCompleted(object sender, Get_ByParentIDCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEUSEAPP> lst = e.Result;
            if (lst != null)
            {
                _lstVUseApp.AddRange(lst);
                dg.ItemsSource = _lstVUseApp;
            }
        }

        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VehicleDispatch");
        }

        public string GetStatus()
        {
            return "";
        }
        private RefreshedTypes saveType;
        public void DoAction(string actionType)
        { }

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
            return new List<ToolbarItem>();
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
            if (vehicleDispatch != null)
                UpdatevehicleDispatchUpdateInfo(vehicleDispatch);
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_VEHICLEDISPATCH>(vehicleDispatch, "OA");
            Utility.SetAuditEntity(entity, "T_OA_VEHICLEDISPATCH", vehicleDispatch.VEHICLEDISPATCHID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
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
            vehicleDispatch.CHECKSTATE = state;
            isSubmitFlow = true;
            Save();
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (vehicleDispatch != null)
                state = vehicleDispatch.CHECKSTATE;
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