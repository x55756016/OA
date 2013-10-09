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

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class VehicleDispatchForm_upd : BaseForm, IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        VehicleDispatchForm_sel frmD;
        VehicleUseAppForm_sel frmU;
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        private FormTypes types;
        private string vupId = string.Empty;
        private bool isSubmitFlow = false;

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
        #endregion

        #region 构造函数
        public VehicleDispatchForm_upd(FormTypes type, string vehicledispatchId)
        {
            InitializeComponent();
            this.types = type;
            this.vupId = vehicledispatchId;
            _VM.UpdateVehicleDispatchAndDetailCompleted += new EventHandler<UpdateVehicleDispatchAndDetailCompletedEventArgs>(UpdateVehicleDispatchAndDetailCompleted);
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(GetVehicleInfoListCompleted);
            _VM.Get_ByParentIDCompleted += new EventHandler<Get_ByParentIDCompletedEventArgs>(Get_ByParentIDCompleted);
            _VM.Del_VDDetailsCompleted += new EventHandler<Del_VDDetailsCompletedEventArgs>(Del_VDDetailsCompleted);
            _VM.Get_VDInfoCompleted += new EventHandler<Get_VDInfoCompletedEventArgs>(Get_VDInfoCompleted);
            _VM.Get_VDInfoAsync(vupId);

            personclient.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(personclient_GetEmployeeDetailByIDCompleted);
        }
        #endregion

        void personclient_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    string PostName = "";
                    string DepartmentName = "";
                    string CompanyName = "";
                    string StrName = "";
                    PostName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                    DepartmentName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    CompanyName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    StrName = e.Result.T_HR_EMPLOYEE.EMPLOYEECNAME + "-" + PostName + "-" + DepartmentName + "-" + CompanyName;
                    txtDriverName.Text = StrName;

                    ToolTipService.SetToolTip(txtDriverName, StrName);
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
        }

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Load_Data();
        }
        #endregion

        #region Load_Data
        private void Load_Data()
        {
            _VM.GetVehicleInfoListAsync();

            // by ldx
            // 2011-08-09
            // 修改这里多次调用
            //if (vehicleDispatch != null)
            //{
            //    SetFormDefalueValue(vehicleDispatch);
            //}
        }
        #endregion

        #region 根据ID获取
        void Get_VDInfoCompleted(object sender, Get_VDInfoCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                ObservableCollection<T_OA_VEHICLEDISPATCH> vhdch = e.Result;
                if (vhdch != null)
                {
                    vehicleDispatch = vhdch[0];
                }
                // by ldx
                // 2011-08-09
                // 修改这里多次调用
                if (vehicleDispatch != null)
                {
                    SetFormDefalueValue(vehicleDispatch);
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                //Load_Data();
            }
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
                    if (vehicleDispatch != null && types == FormTypes.Edit)
                    {
                        if (vehicleDispatch.T_OA_VEHICLEReference.EntityKey != null)
                        {
                            SetComboBoxSelect(vehicleList
                                , vehicleDispatch.T_OA_VEHICLEReference.EntityKey.EntityKeyValues[0].Value.ToString());
                        }
                    }
                }
            }
        }
        #endregion

        #region 修改
        private void UpdateVehicleDispatchAndDetailCompleted(object sender, UpdateVehicleDispatchAndDetailCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                if (e.Result == 1)
                {
                    if (e.UserState.ToString() == "Edit")
                    {
                        Utility.ShowMessageBox("UPDATE", isSubmitFlow, true);//修改成功!
                    }
                    if (e.UserState.ToString() == "Audit")
                    {
                        Utility.ShowMessageBox("SUCCESSAUDIT", isSubmitFlow, true);//审核成功
                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                    else if (e.UserState.ToString() == "Submit")
                    {
                        Utility.ShowMessageBox("SUCCESSSUBMITAUDIT", isSubmitFlow, true);//提交审核成功
                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                    RefreshUI(RefreshedTypes.All);
                    RefreshUI(saveType);
                }
                else
                {
                    if (e.UserState.ToString() == "Edit")
                    {
                        Utility.ShowMessageBox("UPDATE", isSubmitFlow, false);//修改失败!
                    }
                    else
                    {
                        Utility.ShowMessageBox("AUDITFAILURE", isSubmitFlow, false);//提交审核失败,请重试!
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        private void vehicleInfoManager_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfoList = e.Result;
            if (vehicleInfoList != null)
            {
                foreach (T_OA_VEHICLE obj in vehicleInfoList)
                {
                    cmbVehicleInfo.Items.Add(obj);
                }
                cmbVehicleInfo.DisplayMemberPath = "VIN";
                if (vehicleDispatch != null && editState == "update")
                {
                    SetComboBoxSelect(vehicleInfoList, (new System.Collections.Generic.List<SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityKeyMember>(((SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityReference)(vehicleDispatch.T_OA_VEHICLEReference)).EntityKey.EntityKeyValues))[0].Value.ToString());
                }
                else
                {
                    SetComboBoxSelect(vehicleInfoList, null);
                }
            }
        }
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
        }


        private bool Check()
        {
            //List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = dtiStartDate.Group1.ValidateAll();
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }

            validators = dtiEndDate.Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }

            validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }

            //add by zl
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
            // end
            if (dtiStartDate.DateTimeValue >= dtiEndDate.DateTimeValue)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("StartEndTime"));
                return false;
            }
            if (string.IsNullOrEmpty(txtNum.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "NumberOfPeople"));
                return false;
            }

            if (_lstVUseApp.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("ERROR_VDUSEAPP"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return false;
            }
            return true;
        }

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
            //foreach (var v in _lstVUseApp_Add)
            foreach (var v in _lstVUseApp)
            {
                T_OA_VEHICLEDISPATCHDETAIL info = new T_OA_VEHICLEDISPATCHDETAIL();
                info.VEHICLEDISPATCHDETAILID = System.Guid.NewGuid().ToString();
                info.T_OA_VEHICLEDISPATCH = vInfo;
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
            _VM.UpdateVehicleDispatchAndDetailAsync(vInfo, lstDetail, "Edit");
        }

        private string editState = null;
        public string EditState
        {
            get { return editState; }
            set { editState = value; }
        }

        private T_OA_VEHICLEDISPATCH vehicleDispatch = null;
        public T_OA_VEHICLEDISPATCH VehicleDispatch
        {
            get { return vehicleDispatch; }
            set { vehicleDispatch = value; }
        }

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

            if (types == FormTypes.Resubmit)//重新提交
            {
                vehDispatchInfo.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            }
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //InitAudit(vupId);
            //加载已经派车的申请用车数据
            _VM.Get_ByParentIDAsync(vupId);

            PersonnelServiceClient client = new PersonnelServiceClient();
            client.GetEmployeeByIDAsync(vehDispatchInfo.DRIVER);
            client.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(client_GetEmployeeByIDCompleted);
        }

        void client_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Result != null)
                txtDriverName.Text = ((T_HR_EMPLOYEE)e.Result).EMPLOYEECNAME;
        }

        void client_GetEducateHistoryAllCompleted(object sender, GetEducateHistoryAllCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region 获取申请列表

        //获取已经派车的申请用车数据
        void Get_ByParentIDCompleted(object sender, Get_ByParentIDCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEUSEAPP> lst = e.Result;
            if (lst != null)
            {
                _lstVUseApp.AddRange(lst);
                dg.ItemsSource = _lstVUseApp;
                //add by zl
                //_lstVUseApp_Add = _lstVUseApp;
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
        {
            if (actionType.ToInt32() == 2)
                sel_vUseApp();
            else if (actionType.ToInt32() == 3)
                sel_vDipatch();
            else
            {
                if (!Check()) return;
                RefreshUI(RefreshedTypes.ShowProgressBar);
                switch (actionType)
                {
                    case "0":
                        isSubmitFlow = false;
                        saveType = RefreshedTypes.All;
                        Save();
                        break;
                    case "1":
                        saveType = RefreshedTypes.CloseAndReloadData;
                        Save();
                        break;
                }
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
            object[,] arr = new object[,]{
             {ToolbarItemDisplayTypes.Image,"3","BTNVEHICLENO","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"},
             {ToolbarItemDisplayTypes.Image,"2","BTNVEHICLEUSEAPP","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"},           
             {ToolbarItemDisplayTypes.Image,"1","SAVEANDCLOSE", "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
             {ToolbarItemDisplayTypes.Image,"0","SAVE","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
            };
            return VehicleMgt.GetToolBarItems(ref arr);
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
            if (Check())
                if (vehicleDispatch != null)
                    UpdatevehicleDispatchUpdateInfo(vehicleDispatch);
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion



        #region txtNum_KeyUp
        private void txtNum_KeyUp(object sender, KeyEventArgs e)
        {
            //GlobalFunction.TextBoxInputInt(sender, e);
        }
        #endregion

        #region 查找司机
        private void btnLookUpOwner_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE empInfo = (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)companyInfo.ObjectInstance;
                    vehicleDispatch.OWNERCOMPANYID = empInfo.OWNERCOMPANYID;
                    vehicleDispatch.OWNERDEPARTMENTID = empInfo.OWNERDEPARTMENTID;
                    vehicleDispatch.OWNERID = empInfo.EMPLOYEEID;
                    vehicleDispatch.OWNERNAME = empInfo.EMPLOYEEENAME;
                    vehicleDispatch.OWNERPOSTID = empInfo.T_HR_EMPLOYEEPOST.FirstOrDefault().EMPLOYEEPOSTID;
                    txtDriverName.Text = companyInfo.ObjectName;
                    txtDriverID.Text = companyInfo.ObjectID;
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion

        #region 选择已审核通过的申请单 1
        /// <summary>
        /// 选择已审核通过的申请单 1
        /// </summary>
        private void sel_vUseApp()
        {
            frmU = new VehicleUseAppForm_sel(ref _lstVehicle, cmbVehicleInfo.SelectedIndex, dtiStartDate.DateTimeValue);
            EntityBrowser browser = new EntityBrowser(frmU);
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(sel_vUseApp_end);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

        }
        #endregion

        #region 选择已审核通过的申请单 2
        private void sel_vUseApp_end()
        {
            dg.ItemsSource = null;
            foreach (var h in frmU._lstVUseApp_Add)
            {
                var aa = from a in _lstVUseApp where h.VEHICLEUSEAPPID == a.VEHICLEUSEAPPID select a;
                if (aa.Count() == 0)
                {
                    //_lstVUseApp_Add.Add(h);
                    _lstVUseApp.Add(h);
                }
            }
            dg.ItemsSource = _lstVUseApp;
        }
        #endregion

        #region 选择已审核通过的派车单 1
        /// <summary>
        /// 选择已审核通过的派车单 1
        /// </summary>
        private void sel_vDipatch()
        {
            frmD = new VehicleDispatchForm_sel(ref _lstVehicle, cmbVehicleInfo.SelectedIndex, dtiStartDate.DateTimeValue);
            EntityBrowser browser = new EntityBrowser(frmD);
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(sel_vDipatch_end);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

        }
        #endregion

        #region 选择已审核通过的派车单 2
        private void sel_vDipatch_end()
        {
            if (frmD._lstVDispatch != null && frmD._lstVDispatch.Count > 0)
            {
                cmbVehicleInfo.SelectedItem = frmD._lstVDispatch[0].T_OA_VEHICLE;
                txtNum.Text = frmD._lstVDispatch[0].NUM;
                dtiStartDate.DateTimeValue = Convert.ToDateTime(frmD._lstVDispatch[0].STARTTIME);
                dtiEndDate.DateTimeValue = Convert.ToDateTime(frmD._lstVDispatch[0].ENDTIME);
                txtDriverID.Text = frmD._lstVDispatch[0].DRIVER;
                //txtDriverName.Text = frmD._lstVDispatch[0].DRIVER;
                personclient.GetEmployeeDetailByIDAsync(frmD._lstVDispatch[0].OWNERID);
                txtTel.Text = frmD._lstVDispatch[0].TEL;
                txtRoute.Text = frmD._lstVDispatch[0].ROUTE;
                txtContent.Text = frmD._lstVDispatch[0].CONTENT;
            }
        }
        #endregion

        #region 删除子表
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            dg.ItemsSource = null;
            ObservableCollection<string> o = new ObservableCollection<string>();
            T_OA_VEHICLEUSEAPP i = ((Button)sender).DataContext as T_OA_VEHICLEUSEAPP;
            o.Add(i.VEHICLEUSEAPPID);

            if (_lstVUseApp_Add.Contains(i))
            {
                _lstVUseApp_Add.Remove(i);
                _lstVUseApp.Remove(i);
            }
            else if (_lstVUseApp.Contains(i))
            {
                ComfirmWindow com = new ComfirmWindow();
                if (_lstVUseApp.Count > 1) //不能为空
                {
                    string Result = "";

                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        _VM.Del_VDDetailsAsync(o);
                        _lstVUseApp.Remove(i);
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
                else
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("ERROR_VDUSEAPP"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            dg.ItemsSource = _lstVUseApp;
        }
        private void Del_VDDetailsCompleted(object sender, Del_VDDetailsCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                dg.SelectedIndex = 0;
                Utility.ShowMessageBox("DELETE", false, true);
            }
            else
                Utility.ShowMessageBox("DELETE", false, false);

            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        #endregion

        #region 行加载删除按钮
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_VEHICLEUSEAPP tmp = (T_OA_VEHICLEUSEAPP)e.Row.DataContext;
            ImageButton MyButton_Delbaodao = dg.Columns[7].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Delbaodao.Tag = tmp;

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
            string UserState = string.Empty;
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
            if (vehicleDispatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            vehicleDispatch.CHECKSTATE = state;
            isSubmitFlow = true;
            //_VM.UpdateVehicleDispatchAndDetailAsync(vehicleDispatch, null, UserState);
            UpdatevehicleDispatchUpdateInfo(vehicleDispatch);
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