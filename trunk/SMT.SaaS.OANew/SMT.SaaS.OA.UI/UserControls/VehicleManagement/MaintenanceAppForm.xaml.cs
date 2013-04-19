using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.EngineDataSource;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MaintenanceAppForm : BaseForm, IClient, IEntityEditor, IAudit
    {
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        private FormTypes actions;

        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;

        private string editState = null;
        public string EditState { get { return editState; } set { editState = value; } }
        private T_OA_MAINTENANCEAPP conserVation = null;
        public T_OA_MAINTENANCEAPP ConserVation { get { return conserVation; } set { conserVation = value; } }
        private bool isSubmitFlow = false;

        public MaintenanceAppForm(FormTypes actions)
        {
            InitializeComponent();
            this.actions = actions;
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(vehicleInfoManager_GetVehicleInfoListCompleted);
            _VM.AddMaintenanceAppCompleted += new EventHandler<AddMaintenanceAppCompletedEventArgs>(cvmWS_AddMaintenanceAppCompleted);
            _VM.UpdateMaintenanceAppCompleted += new EventHandler<UpdateMaintenanceAppCompletedEventArgs>(cvmWS_UpdateMaintenanceAppCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            if (actions == FormTypes.New)
            {
                conserVation = new T_OA_MAINTENANCEAPP();
                conserVation.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                conserVation.MAINTENANCEAPPID = System.Guid.NewGuid().ToString();
            }
            this.Loaded += new RoutedEventHandler(LayoutRoot_Loaded);
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _VM.GetVehicleInfoListAsync();
            Utility.BindComboBox(cmbRepairName, "MAINTENANCENAME", 0);
            dateREPAIRDATE.Text = DateTime.Today.ToShortDateString();
            dateRETRIEVEDATE.Text = DateTime.Today.AddDays(2).ToShortDateString();
            txtTel.Text = Common.CurrentLoginUserInfo.Telphone == null ? "" : Common.CurrentLoginUserInfo.Telphone;
            if (conserVation != null)
            {
                Utility.SetComboboxSelectByText(cmbRepairName, conserVation.MAINTENANCETYPE, -1);
                txtFee.Text = conserVation.CHARGEMONEY.ToString();
                ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;
                //cmbRepairName.SelectedIndex = 0;
            }
            if (actions == FormTypes.Edit)
                cmbVehicleAssetId.IsEnabled = false;
            //InitAudit(conserVation.MAINTENANCEAPPID);

            if (actions == FormTypes.Resubmit)//重新提交
            {
                conserVation.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            }
            if (actions != FormTypes.New)
            {
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
            InitFBControl();
        }
        /// <summary>
        /// 费用控件 add update 操作时
        /// </summary>
        /// <param name="editStateString"></param>
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";

            if (actions == FormTypes.Edit)
                fbCtr.Order.ORDERID = conserVation.MAINTENANCEAPPID;//费用对象
            else
            {
                fbCtr.Order.ORDERID = "";
                fbCtr.Order.ORDERCODE = "JGZC" + string.Format("{0:yyyyMMddHHmmssffff}", System.DateTime.Now);
            }
            fbCtr.Order.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.CREATECOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.CREATEDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.CREATEPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.Order.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.OWNERCOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.OWNERDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.OWNERPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;


            fbCtr.Order.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.InitDataComplete += (o, e) =>
            {
                Binding bding = new Binding();
                bding.Path = new PropertyPath("TOTALMONEY");
                this.txtFee.SetBinding(TextBox.TextProperty, bding);
                this.txtFee.DataContext = fbCtr.Order;
            };

            //Action.AUDIT  fbCtr.InitData(false);
            fbCtr.InitData();
        }
        private void Save()
        {
            bool b = (bool)ckbHasFee.IsChecked;
            if (b)
                if (!CheckInputIsDecimal(txtFee.Text.Trim()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr("IsDouble", "RepairFees"));
                    return;
                }
            if (actions == FormTypes.New)
                AddInfo();
            else
                if (conserVation != null)
                    UpdateInfo(conserVation);
        }
        private void AddInfo()
        {
            T_OA_MAINTENANCEAPP info = new T_OA_MAINTENANCEAPP();
            info.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleAssetId.SelectedItem;
            info.CHECKSTATE = conserVation.CHECKSTATE;
            info.MAINTENANCEAPPID = conserVation.MAINTENANCEAPPID;
            info.MAINTENANCETYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbRepairName.SelectedItem).DICTIONARYNAME.ToString();

            info.TEL = txtTel.Text;
            info.CONTENT = txtContent.Text;
            info.REMARK = txtReMark.Text;

            info.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
            info.CHARGEMONEY = ckbHasFee.IsChecked == true ? Convert.ToDecimal(txtFee.Text) : 0;

            info.REPAIRDATE = DateTime.Parse(dateREPAIRDATE.Text);
            info.RETRIEVEDATE = DateTime.Parse(dateRETRIEVEDATE.Text);
            info.REPAIRCOMPANY = txtREPAIRCOMPANY.Text;
            info.TEL = txtTel.Text;

            info.CREATEDATE = System.DateTime.Now;
            info.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            info.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            info.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            info.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            info.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            info.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            info.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            info.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            info.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            info.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            //info.UPDATEDATE = System.DateTime.Now;
            //info.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            //info.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            _VM.AddMaintenanceAppAsync(info);
            conserVation = info;
        }

        private void UpdateInfo(T_OA_MAINTENANCEAPP info)
        {
            T_SYS_DICTIONARY StrDepCity = cmbRepairName.SelectedItem as T_SYS_DICTIONARY;
            info.MAINTENANCETYPE = StrDepCity.DICTIONARYNAME.ToString();
            info.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleAssetId.SelectedItem;
            //info.MAINTENANCETYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbRepairName.SelectedItem).DICTIONARYNAME.ToString();
            info.TEL = txtTel.Text;
            info.CONTENT = txtContent.Text;
            info.REMARK = txtReMark.Text;

            info.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
            info.CHARGEMONEY = ckbHasFee.IsChecked == true ? Convert.ToDecimal(txtFee.Text) : 0;

            info.REPAIRDATE = DateTime.Parse(dateREPAIRDATE.Text);
            info.RETRIEVEDATE = DateTime.Parse(dateRETRIEVEDATE.Text);
            info.REPAIRCOMPANY = txtREPAIRCOMPANY.Text;
            info.TEL = txtTel.Text;

            info.UPDATEDATE = System.DateTime.Now;
            info.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            _VM.UpdateMaintenanceAppAsync(info);
            conserVation = info;
        }

        void cvmWS_UpdateMaintenanceAppCompleted(object sender, UpdateMaintenanceAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                
                if (e.Result > 0)
                {
                    if (e.Error != null && e.Error.Message != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                        return;
                    }
                    if (ckbHasFee.IsChecked == true && Convert.ToInt32(txtFee.Text) > 0)
                    {
                        fbCtr.Order.ORDERID = conserVation.MAINTENANCEAPPID;
                        fbCtr.Save(_flwResult);//提交费用
                    }
                    else
                    {
                        Utility.ShowMessageBox("UPDATE", isSubmitFlow, true);
                        if (isSubmitFlow)
                            saveType = RefreshedTypes.CloseAndReloadData;
                        RefreshUI(saveType);
                    }
                }
                else
                {
                    Utility.ShowMessageBox("UPDATE", isSubmitFlow, false);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                
            }
        }

        void cvmWS_AddMaintenanceAppCompleted(object sender, AddMaintenanceAppCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Result > 0)
                {
                    if (e.Error != null && e.Error.Message != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                        return;
                    }
                    if (ckbHasFee.IsChecked == true && Convert.ToInt32(txtFee.Text) > 0)
                    {
                        fbCtr.Order.ORDERID = conserVation.MAINTENANCEAPPID;
                        fbCtr.Save(_flwResult);//提交费用
                    }
                    else
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        actions = FormTypes.Edit;
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                        Utility.ShowMessageBox("ADD", isSubmitFlow, true);
                        if (isSubmitFlow)
                            saveType = RefreshedTypes.CloseAndReloadData;
                        RefreshUI(saveType);
                    }
                }
                else
                {
                    Utility.ShowMessageBox("ADD", isSubmitFlow, false);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            try
            {
                if (e.Message != null && e.Message.Count() > 0)
                {
                    Utility.ShowMessageBox("UPDATE", isSubmitFlow, false);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    if (actions == FormTypes.New)
                    {
                        Utility.ShowMessageBox("ADD", isSubmitFlow, true);
                        if (isSubmitFlow)
                            saveType = RefreshedTypes.CloseAndReloadData;
                        RefreshUI(saveType);
                    }
                    else
                    {
                        Utility.ShowMessageBox("UPDATE", isSubmitFlow, true);
                        if (isSubmitFlow)
                            saveType = RefreshedTypes.CloseAndReloadData;
                        RefreshUI(saveType);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        void vehicleInfoManager_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfoList = e.Result;
            if (vehicleInfoList != null)
            {
                if (string.IsNullOrEmpty(conserVation.OWNERCOMPANYID))
                    SetComboBoxSelect(vehicleInfoList, null);
                else
                {
                    dateREPAIRDATE.Text = Convert.ToDateTime(conserVation.REPAIRDATE).ToShortDateString();
                    dateRETRIEVEDATE.Text = Convert.ToDateTime(conserVation.RETRIEVEDATE).ToShortDateString();
                    if (!string.IsNullOrEmpty(conserVation.REPAIRCOMPANY))
                        txtREPAIRCOMPANY.Text = conserVation.REPAIRCOMPANY;
                    if (!string.IsNullOrEmpty(conserVation.TEL))
                        txtTel.Text = conserVation.TEL;
                    if (!string.IsNullOrEmpty(conserVation.CONTENT))
                        txtContent.Text = conserVation.CONTENT;
                    if (!string.IsNullOrEmpty(conserVation.REMARK))
                        txtReMark.Text = conserVation.REMARK;
                    ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;
                    txtFee.Text = conserVation.CHARGEMONEY.ToString();
                    if (conserVation.ISCHARGE == "1")
                        txtFee.Visibility = Visibility.Visible;
                    if (conserVation.T_OA_VEHICLEReference.EntityKey != null)
                    {
                        SetComboBoxSelect(vehicleInfoList
                               , conserVation.T_OA_VEHICLEReference.EntityKey.EntityKeyValues[0].ToString());
                    }
                    //SetComboBoxSelect(vehicleInfoList, (new System.Collections.Generic.List<SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityKeyMember>(((SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityReference)(conserVation.T_OA_VEHICLEReference)).EntityKey.EntityKeyValues))[0].Value.ToString());
                }
            }
        }
        private bool Check()
        {
            string StrStart = string.Empty;
            string StrEnd = string.Empty;
            StrStart = dateREPAIRDATE.Text.ToString();//送修时间
            StrEnd = dateRETRIEVEDATE.Text.ToString();//取回时间
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd);
                if (DtEnd < DtStart)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBELESSTHANTHEREPAIRTIME", "RETRIEVEDATE"));
                    return false;
                }
            }

            if (string.IsNullOrEmpty(StrStart))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "REPAIRDATE"));
                return false;
            }

            if (string.IsNullOrEmpty(StrEnd))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "RETRIEVEDATE"));
                return false;
            }

            if (string.IsNullOrEmpty(txtTel.Text.Trim()))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "APPTEL"));
                return false;
            }

            
            return true;
        }
        private bool CheckInputIsDecimal(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return false;
            foreach (char c in inputString)
            {
                if (!char.IsNumber(c) && c != '.')
                {
                    return false;
                }
            }
            return true;
        }
        private void SetComboBoxSelect(ObservableCollection<T_OA_VEHICLE> cmbData, string assetId)
        {
            cmbVehicleAssetId.Items.Clear();
            T_OA_VEHICLE selectObj = null;
            foreach (T_OA_VEHICLE obj in cmbData)
            {
                cmbVehicleAssetId.Items.Add(obj);
                if (obj.ASSETID == assetId)
                {
                    selectObj = obj;
                }
            }
            cmbVehicleAssetId.DisplayMemberPath = "VIN";
            if (selectObj != null)
                cmbVehicleAssetId.SelectedItem = selectObj;
            else
                cmbVehicleAssetId.SelectedIndex = 0;
        }



        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("MAINTENANCEAPP");
        }

        public string GetStatus()
        {
            return "";
        }
        private RefreshedTypes saveType;
        public void DoAction(string actionType)
        {
            if (conserVation == null)
            {
                conserVation = new T_OA_MAINTENANCEAPP();
                conserVation.CHECKSTATE = "0";
                conserVation.MAINTENANCEAPPID = System.Guid.NewGuid().ToString();
            }
            if (!Check()) return;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            switch (actionType)
            {
                case "0":
                    saveType = RefreshedTypes.HideProgressBar; _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
                    isSubmitFlow = false;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData; _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
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
            object[,] arr = new object[,]{          
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

        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        public int StarEngine(object oldDataObject, object newDataObject, string nextUserID, string nFlowNode, string oFlowNode)
        {
            string xmlString = null;
            string errorString = null;
            if (GlobalFunction.ObjListToXml<SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityKeyMember>(oldDataObject, newDataObject, ref xmlString, ref errorString, nextUserID, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, nFlowNode, oFlowNode) == -1)
            {
                return -1;//生成失败
            }
            EngineWcfGlobalFunctionClient wcfclient = new EngineWcfGlobalFunctionClient();
            wcfclient.SaveTriggerDataAsync(xmlString);
            wcfclient.SaveTriggerDataCompleted += new EventHandler<SaveTriggerDataCompletedEventArgs>(wcfclient_SaveTriggerDataCompleted);
            return 1;
        }

        void wcfclient_SaveTriggerDataCompleted(object sender, SaveTriggerDataCompletedEventArgs e)
        { }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                txbFee.Visibility = Visibility.Visible;
                txtFee.Visibility = Visibility.Visible;
                scvFB.Visibility = Visibility.Visible;
            }
            else
            {
                txbFee.Visibility = Visibility.Collapsed;
                txtFee.Visibility = Visibility.Collapsed;
                scvFB.Visibility = Visibility.Collapsed;
            }
        }

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("BorrowMoney", fbCtr.GetParamenter().BorrowMoney.ToString());
            strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_MAINTENANCEAPP>(conserVation, "OA", parameters);
            Utility.SetAuditEntity(entity, "T_OA_MAINTENANCEAPP", conserVation.MAINTENANCEAPPID, strXmlObjectSource);
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
            conserVation.CHECKSTATE = state;
            isSubmitFlow = true;
            _VM.UpdateMaintenanceAppAsync(conserVation);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (conserVation != null)
                state = conserVation.CHECKSTATE;
            if (editState == "view")
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
