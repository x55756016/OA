using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.EngineDataSource;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MaintenanceRecordForm_add : BaseForm,IClient, IEntityEditor, IAudit
    {
        /// <summary>
        /// 选择框
        /// </summary>
        MaintenanceApp_sel frmM;
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        private FormTypes actions;
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private string editState = null;
        public string EditState { get { return editState; } set { editState = value; } }
        private T_OA_MAINTENANCERECORD conserVation = null;
        public T_OA_MAINTENANCERECORD ConserVation { get { return conserVation; } set { conserVation = value; } }
        private bool isSubmitFlow = false;


        public MaintenanceRecordForm_add(FormTypes action)
        {
            InitializeComponent();
            this.actions = action;
            _VM.Add_VMRecordCompleted += new EventHandler<Add_VMRecordCompletedEventArgs>(Add_VMRecordCompleted);
            _VM.Upd_VMRecordCompleted += new EventHandler<Upd_VMRecordCompletedEventArgs>(Upd_VMRecordCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            this.Loaded += new RoutedEventHandler(MaintenanceRecordForm_add_Loaded);
            if (actions == FormTypes.New)
            {
                conserVation = new T_OA_MAINTENANCERECORD();
                conserVation.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                conserVation.MAINTENANCERECORDID = System.Guid.NewGuid().ToString();
            }
        }

        void MaintenanceRecordForm_add_Loaded(object sender, RoutedEventArgs e)
        {
            

            Utility.BindComboBox(cmbRepairName, "MAINTENANCENAME", 0);
            dateREPAIRDATE.Text = DateTime.Today.ToShortDateString();
            dateRETRIEVEDATE.Text = DateTime.Today.AddDays(2).ToShortDateString();
            InitFBControl();
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            

        }

        /// <summary>
        /// 费用  add页面用
        /// </summary>
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";

            fbCtr.Order.ORDERID = "";
            fbCtr.Order.ORDERCODE = "JGZC" + string.Format("{0:yyyyMMddHHmmssffff}", System.DateTime.Now);
            // update fbCtr.Order.ORDERID = organ.ORGANIZATIONID;//费用对象
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
            if (editState == "add") { AddInfo(); }
            if (editState == "update")
                if (conserVation != null)
                    UpdateInfo();

        }
        private void AddInfo()
        {
            conserVation.T_OA_MAINTENANCEAPP = frmM._lst[0];
            conserVation.MAINTENANCETYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbRepairName.SelectedItem).DICTIONARYNAME.ToString();

            conserVation.TEL = txtTel.Text;
            conserVation.CONTENT = txtContent.Text;
            conserVation.REMARK = txtReMark.Text;
            conserVation.REPAIRCOMPANY = txtREPAIRCOMPANY.Text;

            conserVation.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
            conserVation.CHARGEMONEY = ckbHasFee.IsChecked == true ? Convert.ToDecimal(txtFee.Text) : 0;

            conserVation.REPAIRDATE = DateTime.Parse(dateREPAIRDATE.Text);
            conserVation.RETRIEVEDATE = DateTime.Parse(dateRETRIEVEDATE.Text);

            conserVation.CREATEDATE = System.DateTime.Now;
            conserVation.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            conserVation.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            conserVation.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            conserVation.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            conserVation.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            conserVation.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            conserVation.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            conserVation.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            conserVation.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            conserVation.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            _VM.Add_VMRecordAsync(conserVation);

        }

        private void UpdateInfo()
        {
            conserVation.T_OA_MAINTENANCEAPP = conserVation.T_OA_MAINTENANCEAPP;
            conserVation.MAINTENANCETYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbRepairName.SelectedItem).DICTIONARYNAME.ToString();
            conserVation.TEL = txtTel.Text;
            conserVation.CONTENT = txtContent.Text;
            conserVation.REMARK = txtReMark.Text;

            conserVation.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
            conserVation.CHARGEMONEY = ckbHasFee.IsChecked == true ? Convert.ToDecimal(txtFee.Text) : 0;

            conserVation.REPAIRDATE = DateTime.Parse(dateREPAIRDATE.Text);
            conserVation.RETRIEVEDATE = DateTime.Parse(dateRETRIEVEDATE.Text);
            conserVation.REPAIRCOMPANY = txtREPAIRCOMPANY.Text;
            conserVation.TEL = txtTel.Text;

            conserVation.UPDATEDATE = System.DateTime.Now;
            conserVation.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            conserVation.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            _VM.Upd_VMRecordAsync(conserVation);


        }
        void Upd_VMRecordCompleted(object sender, Upd_VMRecordCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                if (ckbHasFee.IsChecked == true && Convert.ToInt32(txtFee.Text) > 0)
                {
                    fbCtr.Order.ORDERID = conserVation.MAINTENANCERECORDID;
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
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        void Add_VMRecordCompleted(object sender, Add_VMRecordCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                if (ckbHasFee.IsChecked == true && Convert.ToInt32(txtFee.Text) > 0)
                {
                    fbCtr.Order.ORDERID = conserVation.MAINTENANCERECORDID;
                    fbCtr.Save(_flwResult);//提交费用               
                    editState = "update";
                }
                else
                {
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
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
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
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
        private bool CheckInputIsDecimal(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return false;
            foreach (char c in inputString)
                if (!char.IsNumber(c) && c != '.') { return false; }
            return true;
        }
        private bool Check()
        {
            string StrStart = string.Empty;
            string StrEnd = string.Empty;
            StrStart = dateREPAIRDATE.Text.ToString();//送修时间
            StrEnd = dateRETRIEVEDATE.Text.ToString();//取回时间
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

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            bool b = (bool)ckbHasFee.IsChecked;
            if (b)
                if (!CheckInputIsDecimal(txtFee.Text.Trim()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IsDouble", "RepairFees"));
                    return false;
                }
            return true;
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VEHICLEMAINTENANCERECORD");
        }

        public string GetStatus()
        {
            return "";
        }
        private RefreshedTypes saveType;
        public void DoAction(string actionType)
        {
            if (actionType == "2")
                sel_App();
            else
            {
                if (!Check()) return;

                RefreshUI(RefreshedTypes.ShowProgressBar);
                switch (actionType)
                {
                    case "0":
                        saveType = RefreshedTypes.HideProgressBar;
                        isSubmitFlow = false;
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
             {ToolbarItemDisplayTypes.Image,"2","BTNMAINTENANCEAPP","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"},       
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
        {
            //if (e. == 1)
            //{
            //    MessageBox.Show("成功!");
            //}
        }

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
        /// <summary>
        /// 选择已审核通过的申请单 1
        /// </summary>
        private void sel_App()
        {
            frmM = new MaintenanceApp_sel();
            EntityBrowser browser = new EntityBrowser(frmM);
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(sel_App_end);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        // 选择已审核通过的派车单 2
        private void sel_App_end()
        {
            if (frmM._lst != null && frmM._lst.Count > 0)
            {
                txtMAINTENANCEAPPID.Text = frmM._lst[0].MAINTENANCEAPPID;
                txtVehicleVIN.Text = frmM._lst[0].T_OA_VEHICLE.VIN;
                Utility.SetComboboxSelectByText(cmbRepairName, frmM._lst[0].MAINTENANCETYPE, -1);
                dateREPAIRDATE.Text = Convert.ToDateTime(frmM._lst[0].REPAIRDATE).ToShortDateString();
                dateRETRIEVEDATE.Text = Convert.ToDateTime(frmM._lst[0].RETRIEVEDATE).ToShortDateString();
                txtREPAIRCOMPANY.Text = frmM._lst[0].REPAIRCOMPANY;
                txtTel.Text = frmM._lst[0].TEL;

                txtContent.Text = frmM._lst[0].CONTENT;
                txtReMark.Text = frmM._lst[0].REMARK == null ? "" : frmM._lst[0].REMARK;

                ckbHasFee.IsChecked = frmM._lst[0].ISCHARGE == "1" ? true : false;
                txtFee.Text = frmM._lst[0].CHARGEMONEY.ToString();
                txtFee.Visibility = frmM._lst[0].ISCHARGE == "1" ? Visibility.Visible : Visibility.Collapsed;

            }
        }

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("BorrowMoney", fbCtr.GetParamenter().BorrowMoney.ToString());
            strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_MAINTENANCERECORD>(conserVation, "OA", parameters);
            Utility.SetAuditEntity(entity, "T_OA_MAINTENANCERECORD", conserVation.MAINTENANCERECORDID, strXmlObjectSource);
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
            Save();
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