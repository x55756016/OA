using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.EngineDataSource;

using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ConserVationForm : BaseForm,IClient, IEntityEditor, IAudit
    {
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private string editState = null;
        public string EditState { get { return editState; } set { editState = value; } }
        private T_OA_CONSERVATION conserVation = null;
        public T_OA_CONSERVATION ConserVation { get { return conserVation; } set { conserVation = value; } }
        private bool isFlowed = false;
        private FormTypes types;

        public ConserVationForm(FormTypes type)
        {
            InitializeComponent();
            this.types = type;
            this.Loaded += new RoutedEventHandler(ConserVationForm_Loaded);
        }

        void ConserVationForm_Loaded(object sender, RoutedEventArgs e)
        {
            _VM.AddConserVationCompleted += new EventHandler<AddConserVationCompletedEventArgs>(AddConserVationCompleted);
            _VM.UpdateConserVationCompleted += new EventHandler<UpdateConserVationCompletedEventArgs>(UpdateConserVationCompleted);
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(_VM_GetVehicleInfoListCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            if (types == FormTypes.New)
            {
                conserVation = new T_OA_CONSERVATION();
                conserVation.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                conserVation.CONSERVATIONID = System.Guid.NewGuid().ToString();
            }
            //LayoutRoot_Loaded(sender,e);
            _VM.GetVehicleInfoListAsync(null);
            Utility.BindComboBox(cmbConserVationName, "CONSERVANAME", 0);
            dateREPAIRDATE.Text = DateTime.Today.ToShortDateString();
            dateRETRIEVEDATE.Text = DateTime.Today.AddDays(2).ToShortDateString();
            txtTel.Text = Common.CurrentLoginUserInfo.Telphone == null ? "" : Common.CurrentLoginUserInfo.Telphone;
            if (conserVation != null)
                SetFormDefailValue(conserVation);
            if (types != FormTypes.New)
            {
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
            SetForms();
            InitFBControl();
            LayoutRoot_Loaded(sender,e);
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _VM.GetVehicleInfoListAsync(null);
            Utility.BindComboBox(cmbConserVationName, "CONSERVANAME", 0);
            dateREPAIRDATE.Text = DateTime.Today.ToShortDateString();
            dateRETRIEVEDATE.Text = DateTime.Today.AddDays(2).ToShortDateString();
            txtTel.Text = Common.CurrentLoginUserInfo.Telphone == null ? "" : Common.CurrentLoginUserInfo.Telphone;
            if (conserVation != null)
                SetFormDefailValue(conserVation);
            if (types != FormTypes.New)
            {
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
            SetForms();
            InitFBControl();
        }
        private void SetForms()
        {
            if (types == FormTypes.Browse)
            {
                txtContent.IsReadOnly = true;
                txtFee.IsReadOnly = true;
                cmbConserVationName.IsEnabled = false;
                //slvView.Visibility = Visibility.Visible;
                //InitAudit(conserVation.CONSERVATIONID);
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
            if (types == FormTypes.Edit)
            {
                cmbVehicleAssetId.IsEnabled = false;
            }
        }
        /// <summary>
        /// 费用控件 add update 操作时
        /// </summary>
        /// <param name="editStateString"></param>
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";
            if (types == FormTypes.Edit)
                fbCtr.Order.ORDERID = ConserVation.CONSERVATIONID;//费用对象
            else
            {
                fbCtr.Order.ORDERID = "";
                //fbCtr.Order.ORDERCODE = "JGZC" + string.Format("{0:yyyyMMddHHmmssffff}", System.DateTime.Now);
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

        void _VM_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
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
                    txtREPAIRCOMPANY.Text = conserVation.CONSERVATIONCOMPANY;
                    txtTel.Text = conserVation.TEL;
                    txtContent.Text = conserVation.CONTENT;
                    txtReMark.Text = conserVation.REMARK == null ? "" : conserVation.REMARK;
                    txtCONSERVATIONRANGE.Text = conserVation.CONSERVATIONRANGE.ToString();

                    ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;
                    txtFee.Text = conserVation.CHARGEMONEY.ToString();
                    if (conserVation.ISCHARGE == "1")
                        txtFee.Visibility = Visibility.Visible;
                    SetComboBoxSelect(vehicleInfoList, (new System.Collections.Generic.List<SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityKeyMember>(((SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityReference)(conserVation.T_OA_VEHICLEReference)).EntityKey.EntityKeyValues))[0].Value.ToString());

                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }
            }
        }

        private void UpdateConserVationCompleted(object sender, UpdateConserVationCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                if (ckbHasFee.IsChecked == true)
                {
                    fbCtr.Order.ORDERID = conserVation.CONSERVATIONID;
                    fbCtr.Save(_flwResult);//提交费用
                }
                else
                {
                    if (e.UserState.ToString() == "Edit")
                    {
                        Utility.ShowMessageBox("UPDATE", isFlowed, true);
                        RefreshUI(saveType);
                    }
                    else if (e.UserState.ToString() == "Audit")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else if (e.UserState.ToString() == "Submit")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    RefreshUI(RefreshedTypes.All);
                }
            }
            else
            {
                if (e.UserState.ToString() == "Edit")
                {
                    Utility.ShowMessageBox("UPDATE", isFlowed, false);//修改失败!
                }
                else
                {
                    Utility.ShowMessageBox("AUDITFAILURE", isFlowed, false);//提交审核失败,请重试!
                }
            }
        }

        private void AddConserVationCompleted(object sender, AddConserVationCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                if (ckbHasFee.IsChecked == true)
                {
                    fbCtr.Order.ORDERID = conserVation.CONSERVATIONID;
                    fbCtr.Save(_flwResult);//提交费用
                }
                else
                {
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                    Utility.ShowMessageBox("ADD", isFlowed, true);
                    RefreshUI(saveType);
                    types = FormTypes.Edit;
                }
            }
            else
            {
                Utility.ShowMessageBox("ADD", isFlowed, false);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                Utility.ShowMessageBox("UPDATE", isFlowed, false);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", isFlowed, true);
                if (isFlowed)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
            }
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
        private void AddInfo()
        {
            T_OA_CONSERVATION info = new T_OA_CONSERVATION();
            info.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleAssetId.SelectedItem;
            info.CHECKSTATE = conserVation.CHECKSTATE;
            info.CONSERVATYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbConserVationName.SelectedItem).DICTIONARYNAME.ToString();

            info.CONSERVATIONID = conserVation.CONSERVATIONID;
            info.TEL = txtTel.Text;
            info.CONTENT = txtContent.Text;
            info.REMARK = txtReMark.Text;
            info.CONSERVATIONCOMPANY = txtREPAIRCOMPANY.Text;
            info.CONSERVATIONRANGE = txtCONSERVATIONRANGE.Text.Trim() == "" ? 0 : int.Parse(txtCONSERVATIONRANGE.Text);
            info.REPAIRDATE = DateTime.Parse(dateREPAIRDATE.Text);
            info.RETRIEVEDATE = DateTime.Parse(dateRETRIEVEDATE.Text);


            info.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
            conserVation.CHARGEMONEY = ckbHasFee.IsChecked == true ? fbCtr.Order.TOTALMONEY : 0;

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
            info.UPDATEDATE = System.DateTime.Now;
            info.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            info.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            conserVation = info;
            _VM.AddConserVationAsync(info);
        }

        private void UpdateInfo(T_OA_CONSERVATION info)
        {
            info.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleAssetId.SelectedItem;
            info.CONSERVATYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbConserVationName.SelectedItem).DICTIONARYNAME.ToString();

            info.TEL = txtTel.Text;
            info.CONTENT = txtContent.Text;
            info.REMARK = txtReMark.Text;
            info.CONSERVATIONCOMPANY = txtREPAIRCOMPANY.Text;
            info.REPAIRDATE = DateTime.Parse(dateREPAIRDATE.Text);
            info.RETRIEVEDATE = DateTime.Parse(dateRETRIEVEDATE.Text);
            info.CONSERVATIONRANGE = txtCONSERVATIONRANGE.Text.Trim() == "" ? 0 : int.Parse(txtCONSERVATIONRANGE.Text);
            info.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
            //***************************************************************************************************
            ////******************************************************************************************************************************
            info.CHARGEMONEY = ckbHasFee.IsChecked == true ? fbCtr.Order.TOTALMONEY : 0;
            // info.CHARGEMONEY = ckbHasFee.IsChecked == true ? Convert.ToDecimal(txtFee.Text) : 0;

            info.UPDATEDATE = System.DateTime.Now;
            info.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

            _VM.UpdateConserVationAsync(info, "Edit");
        }
        private void SetFormDefailValue(T_OA_CONSERVATION defaultInfo)
        {
            conserVation = defaultInfo;
            if (!string.IsNullOrEmpty(conserVation.OWNERCOMPANYID))
            {
                Utility.SetComboboxSelectByText(cmbConserVationName, conserVation.CONSERVATYPE, -1);
                txtContent.Text = conserVation.CONTENT;
                txtFee.Text = conserVation.CHARGEMONEY.ToString();
                ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;

                if (types == FormTypes.Resubmit)//重新提交
                {
                    conserVation.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
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
            {
                cmbVehicleAssetId.SelectedItem = selectObj;
            }
            else
            {
                cmbVehicleAssetId.SelectedIndex = 0;
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {

        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("ConserVation");
        }

        public string GetStatus()
        {
            return "";
        }
        private RefreshedTypes saveType;
        public void DoAction(string actionType)
        {
            if (cmbVehicleAssetId.SelectedIndex < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("PLEASESELECT", "Vehicle"));
                return;
            }
            if (cmbConserVationName.SelectedIndex < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("PLEASESELECT", "ConserVationName"));
                return;
            }
            if (string.IsNullOrEmpty(txtCONSERVATIONRANGE.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "CONSERVATIONRANGE"));
                return;
            }
            else
            {
                //判断是否为整数
                int iResult = 0;
                if (!int.TryParse(txtCONSERVATIONRANGE.Text.Trim(), out iResult))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "CONSERVATIONRANGE"));
                    return;
                }

            }
            if (ckbHasFee.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtFee.Text))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "MaintenanceFees"));
                    return;
                }
                if (!CheckInputIsDecimal(txtFee.Text.Trim()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("IsDouble", "MaintenanceFees"));
                    return;
                }
            }
            if (types == FormTypes.New)
            {
                conserVation = new T_OA_CONSERVATION();
                conserVation.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                conserVation.CONSERVATIONID = System.Guid.NewGuid().ToString();
            }
            if (!Check()) return;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            switch (actionType)
            {
                case "0":
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
                    saveType = RefreshedTypes.HideProgressBar;
                    Save();
                    break;
                case "1":
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
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
        private void Save()
        {
            if (Check())
            {
                if (types == FormTypes.New)
                {
                    AddInfo();
                }
                if (types == FormTypes.Edit)
                {
                    if (conserVation != null)
                    {
                        UpdateInfo(conserVation);
                    }
                }
            }
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            //add by zl
            string StrStart = string.Empty;
            string StrEnd = string.Empty;
            StrStart = this.dateREPAIRDATE.Text.ToString();
            StrEnd = this.dateRETRIEVEDATE.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
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

            //end

            return true;
        }

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
        {
            //if (e. == 1)
            //{
            //    MessageBox.Show("成功!");
            //}
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ckbHasFee.IsChecked == true)
            {
                txtFee.Visibility = Visibility.Visible;
                txbFee.Visibility = Visibility.Visible;
                scvFB.Visibility = Visibility.Visible;
            }
            else
            {
                txtFee.Visibility = Visibility.Collapsed;
                txbFee.Visibility = Visibility.Collapsed;
                scvFB.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            //GlobalFunction.TextBoxInputDecimal(sender, e);
        }

        private void txtCONSERVATIONRANGE_KeyUp(object sender, KeyEventArgs e)
        {
            //GlobalFunction.TextBoxInputInt(sender, e, "CONSERVATIONRANGE");
        }

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("BorrowMoney", fbCtr.GetParamenter().BorrowMoney.ToString());
            strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_CONSERVATION>(conserVation, "OA", parameters);
            Utility.SetAuditEntity(entity, "T_OA_CONSERVATION", conserVation.CONSERVATIONID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.Approving;
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.Approved;
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnApproved;
                    break;
            }
            if (conserVation.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }

            conserVation.CHECKSTATE = state;

            _VM.UpdateConserVationAsync(conserVation, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (conserVation != null)
                state = conserVation.CHECKSTATE;
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
