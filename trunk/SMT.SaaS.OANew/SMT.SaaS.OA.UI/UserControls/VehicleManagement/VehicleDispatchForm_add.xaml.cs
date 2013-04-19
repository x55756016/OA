using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class VehicleDispatchForm_add : BaseForm,IClient, IEntityEditor, IAudit
    {
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        VehicleDispatchForm_sel frmD;
        VehicleUseAppForm_sel frmU;
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        private RefreshedTypes saveType;
        private FormTypes types;
        bool isAdd = true;
        private bool isSubmitFlow = false;
        private T_OA_VEHICLEDISPATCH vehicleDispatch = null;
        public T_OA_VEHICLEDISPATCH VehicleDispatch
        {
            get { return vehicleDispatch; }
            set { vehicleDispatch = value; }
        }
        /// <summary>
        /// 存放选中的派车单
        /// </summary>
        private List<T_OA_VEHICLEDISPATCH> _lstVDispatch = new List<T_OA_VEHICLEDISPATCH>();
        /// <summary>
        /// 存放将要删除的用车申请单
        /// </summary>
        private List<T_OA_VEHICLEUSEAPP> _lstVUseApp_Add = new List<T_OA_VEHICLEUSEAPP>();
        /// <summary>
        /// 车辆信息
        /// </summary>
        public List<T_OA_VEHICLE> _lstVehicle = new List<T_OA_VEHICLE>();
        public VehicleDispatchForm_add(FormTypes type)
        {
            InitializeComponent();
            vehicleDispatch = new T_OA_VEHICLEDISPATCH();
            vehicleDispatch.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            vehicleDispatch.VEHICLEDISPATCHID = System.Guid.NewGuid().ToString();
            vehicleDispatch.STARTTIME = System.DateTime.Now;
            vehicleDispatch.ENDTIME = System.DateTime.Now;
            this.types = type;
            _VM.UpdateVehicleDispatchAndDetailCompleted += new EventHandler<UpdateVehicleDispatchAndDetailCompletedEventArgs>(_VM_UpdateVehicleDispatchAndDetailCompleted);
            _VM.AddVehicleDispatchAndDetailCompleted += new EventHandler<AddVehicleDispatchAndDetailCompletedEventArgs>(_VM_AddVehicleDispatchAndDetailCompleted);
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(GetVehicleInfoListCompleted);
            _VM.Del_VDDetailsCompleted += new EventHandler<Del_VDDetailsCompletedEventArgs>(Del_VDDetailsCompleted);

            personclient.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(personclient_GetEmployeeDetailByIDCompleted);
            
            this.Loaded += new RoutedEventHandler(VehicleDispatchForm_add_Loaded);
        }

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

        void VehicleDispatchForm_add_Loaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            _VM.GetVehicleInfoListAsync();
            if (vehicleDispatch != null)
                SetFormDefalueValue(vehicleDispatch);
            else
                dtiStartDate.DateTimeValue = System.DateTime.Now;
            dtiEndDate.DateTimeValue = dtiStartDate.DateTimeValue.AddHours(1);

            
            
        }

        //加载
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //_VM.GetVehicleInfoListAsync();
            //if (vehicleDispatch != null)
            //    SetFormDefalueValue(vehicleDispatch);
            //else
            //    dtiStartDate.DateTimeValue = System.DateTime.Now;
            //dtiEndDate.DateTimeValue = dtiStartDate.DateTimeValue.AddHours(1);

            //vehicleDispatch = new T_OA_VEHICLEDISPATCH();
            //vehicleDispatch.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            //vehicleDispatch.VEHICLEDISPATCHID = System.Guid.NewGuid().ToString();
            //RefreshUI(RefreshedTypes.AuditInfo);
            //RefreshUI(RefreshedTypes.All);
        }

        //获取车辆 2
        private void GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfo = e.Result;
            if (vehicleInfo != null)
                _lstVehicle = new List<T_OA_VEHICLE>(vehicleInfo);
            if (_lstVehicle != null)
            {
                cmbVehicleInfo.ItemsSource = _lstVehicle;
                cmbVehicleInfo.DisplayMemberPath = "VIN";
                if (vehicleInfo != null)
                {
                    cmbVehicleInfo.SelectedIndex = 0;
                }

            }
            if (types == FormTypes.New)
            {
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
        }
        //设置车辆选中值
        private void SetComboBoxSelect(ObservableCollection<T_OA_VEHICLE> cmbData, string assetId)
        {
        }

        private bool Check()
        {
            //List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = dtiStartDate.Group1.ValidateAll();
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }

            validators = dtiEndDate.Group1.ValidateAll();
            if (validators.Count > 0)
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }

            validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(h.ErrorMessage));
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
                string strStartDate = dtiStartDate.DateTimeValue.ToString("yyyy-mm-dd");
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("StartEndTime"));
                return false;
            }
            if (string.IsNullOrEmpty(txtNum.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "NumberOfPeople"));
                return false;
            }

            if (_lstVUseApp_Add.Count == 0)
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
                    return false;
            }
            return true;
        }

        private bool IsNullOrEmpty(string textContent)
        {
            if (textContent.Trim() == null || textContent.Trim() == string.Empty || textContent.Trim() == "")
                return false;
            return true;
        }
        private void AddVehicleDispatch()
        {

            T_OA_VEHICLEDISPATCH vInfo = new T_OA_VEHICLEDISPATCH();
            
            vInfo.CHECKSTATE = vehicleDispatch.CHECKSTATE;
            vInfo.CONTENT = txtContent.Text;
            vInfo.CREATEDATE = System.DateTime.Now;
            vInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            vInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            vInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            vInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            vInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            vInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            vInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            vInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            vInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            vInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            vInfo.DRIVER = txtDriverID.Text;
            vInfo.ENDTIME = dtiEndDate.DateTimeValue;
            vInfo.ISCANCEL = "1";
            vInfo.NUM = txtNum.Text;
            vInfo.ROUTE = txtRoute.Text;
            vInfo.STARTTIME = dtiStartDate.DateTimeValue;
            vInfo.TEL = txtTel.Text;
            vInfo.UPDATEDATE = System.DateTime.Now;
            vInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            vInfo.VEHICLEDISPATCHID = vehicleDispatch.VEHICLEDISPATCHID;
            vInfo.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleInfo.SelectedItem;


            ObservableCollection<T_OA_VEHICLEDISPATCHDETAIL> lstDetail = new ObservableCollection<T_OA_VEHICLEDISPATCHDETAIL>();
            foreach (var v in _lstVUseApp_Add)
            {
                T_OA_VEHICLEDISPATCHDETAIL info = new T_OA_VEHICLEDISPATCHDETAIL();
                info.VEHICLEDISPATCHDETAILID = System.Guid.NewGuid().ToString();    //有bug修改时也会增加一条
                info.T_OA_VEHICLEDISPATCH = vInfo;
                info.T_OA_VEHICLEUSEAPP = v;
                
              
                info.CREATEDATE = DateTime.Now;        //*******************************************************************************
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

            if (isAdd)
                _VM.AddVehicleDispatchAndDetailAsync(vInfo, lstDetail);
            else
                _VM.UpdateVehicleDispatchAndDetailAsync(vInfo, lstDetail);
        }
        void _VM_AddVehicleDispatchAndDetailCompleted(object sender, AddVehicleDispatchAndDetailCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                isAdd = false;
                Utility.ShowMessageBox("ADD", isSubmitFlow, true);
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                types = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                if (isSubmitFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("ADD", isSubmitFlow, false);
            }
        }
        void _VM_UpdateVehicleDispatchAndDetailCompleted(object sender, UpdateVehicleDispatchAndDetailCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                Utility.ShowMessageBox("SUBMITAUDITSUCCESSFUL", isSubmitFlow, true);//提交审核成功!
                if (isSubmitFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("AUDITFAILURE", isSubmitFlow, false);//提交审核失败,请重试！
            }
        }

        private void SetFormDefalueValue(T_OA_VEHICLEDISPATCH vehDispatchInfo)
        {

            txtContent.Text = string.IsNullOrEmpty(vehDispatchInfo.CONTENT) ? "":vehDispatchInfo.CONTENT ;
            txtDriverID.Text = string.IsNullOrEmpty(vehDispatchInfo.DRIVER) ? "":vehDispatchInfo.DRIVER;
            txtDriverName.Text = string.IsNullOrEmpty(vehDispatchInfo.DRIVER) ? "":vehDispatchInfo.DRIVER;
            txtNum.Text = string.IsNullOrEmpty(vehDispatchInfo.NUM) ? "":vehDispatchInfo.NUM;
            txtRoute.Text =string.IsNullOrEmpty(vehDispatchInfo.ROUTE) ? "":vehDispatchInfo.ROUTE;
            txtTel.Text = string.IsNullOrEmpty(vehDispatchInfo.TEL) ? "":vehDispatchInfo.TEL;

            dtiStartDate.DateTimeValue = Convert.ToDateTime(vehDispatchInfo.STARTTIME);
            dtiEndDate.DateTimeValue = Convert.ToDateTime(vehDispatchInfo.ENDTIME);
            _VM.GetDetailListByDispatchIdAsync(vehDispatchInfo.VEHICLEDISPATCHID);

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

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VehicleDispatch");
        }
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
                        saveType = RefreshedTypes.HideProgressBar;
                        Save();
                        break;
                    case "1":
                        isSubmitFlow = false;
                        saveType = RefreshedTypes.CloseAndReloadData;
                        Save();
                        break;
                }
            }
        }
        public string GetStatus()
        {
            return "";
        }
        //工具栏
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
        // 选择已审核通过的申请单 2
        private void sel_vUseApp_end()
        {
            dg.ItemsSource = null;
            foreach (var h in frmU._lstVUseApp_Add)
            {
                var aa = from a in _lstVUseApp_Add where h.VEHICLEUSEAPPID == a.VEHICLEUSEAPPID select a;
                if (aa.Count() == 0)
                    _lstVUseApp_Add.Add(h);
            }
            dg.ItemsSource = _lstVUseApp_Add;
        }
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
        // 选择已审核通过的派车单 2
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
            AddVehicleDispatch();
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

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
                    if (!string.IsNullOrEmpty(empInfo.OFFICEPHONE))
                    {
                        txtTel.Text = empInfo.OFFICEPHONE;
                    }
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        //删除子表
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            dg.ItemsSource = null;
            ObservableCollection<string> o = new ObservableCollection<string>();
            T_OA_VEHICLEUSEAPP i = ((Button)sender).DataContext as T_OA_VEHICLEUSEAPP;
            o.Add(i.VEHICLEUSEAPPID);

            if (_lstVUseApp_Add.Contains(i))
                _lstVUseApp_Add.Remove(i);
            else
                _VM.Del_VDDetailsAsync(o);
            dg.ItemsSource = _lstVUseApp_Add;

        }
        private void Del_VDDetailsCompleted(object sender, Del_VDDetailsCompletedEventArgs e)
        {

        }
        //行加载删除按钮
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_VEHICLEUSEAPP tmp = (T_OA_VEHICLEUSEAPP)e.Row.DataContext;
            ImageButton MyButton_Delbaodao = dg.Columns[7].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Delbaodao.Tag = tmp;

        }

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
