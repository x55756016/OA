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
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MaintenanceRecordForm_upd : BaseForm,IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        private FormTypes actions;
        private bool isSubmitFlow = false;
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private string editState = null;
        public string EditState { get { return editState; } set { editState = value; } }
        private T_OA_MAINTENANCERECORD conserVation = null;
        public T_OA_MAINTENANCERECORD ConserVation { get { return conserVation; } set { conserVation = value; } }
        #endregion

        #region 构造函数
        public MaintenanceRecordForm_upd(FormTypes action, string SendDocID)
        {
            InitializeComponent();
            this.actions = action;
            _VM.Upd_VMRecordCompleted += new EventHandler<Upd_VMRecordCompletedEventArgs>(Upd_VMRecordCompleted);
            _VM.Get_VMRecordCompleted += new EventHandler<Get_VMRecordCompletedEventArgs>(_VM_Get_VMRecordCompleted);
            _VM.Get_VMRecordAsync(SendDocID);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            this.Loaded += new RoutedEventHandler(LayoutRoot_Loaded);
        }

        void _VM_Get_VMRecordCompleted(object sender, Get_VMRecordCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                conserVation = e.Result;

                Utility.SetComboboxSelectByText(cmbRepairName, conserVation.CONTENT, -1);
                Utility.SetComboboxSelectByText(cmbRepairName, conserVation.MAINTENANCETYPE, -1);
                txtContent.Text = conserVation.CONTENT;
                txtReMark.Text = conserVation.REMARK == null ? "" : conserVation.REMARK;

                txtMAINTENANCEAPPID.Text = conserVation.T_OA_MAINTENANCEAPP.MAINTENANCEAPPID;
                txtVehicleVIN.Text = conserVation.T_OA_MAINTENANCEAPP.T_OA_VEHICLE.VIN;
                dateREPAIRDATE.Text = Convert.ToDateTime(conserVation.REPAIRDATE).ToShortDateString();
                dateRETRIEVEDATE.Text = Convert.ToDateTime(conserVation.RETRIEVEDATE).ToShortDateString();
                txtREPAIRCOMPANY.Text = conserVation.REPAIRCOMPANY;
                txtTel.Text = conserVation.TEL;
                txtContent.Text = conserVation.CONTENT;

                ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;
                txtFee.Text = conserVation.CHARGEMONEY.ToString();

                if (actions == FormTypes.Resubmit)//重新提交
                {
                    conserVation.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);

                InitFBControl();
            }
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Utility.BindComboBox(cmbRepairName, "MAINTENANCENAME", 0);  
        }
        #endregion
        
        #region 费用控件 修改页面调用
        /// <summary>
        /// 费用控件 修改页面调用
        /// </summary>
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";

            fbCtr.Order.ORDERID = conserVation.MAINTENANCERECORDID;//费用对象

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
            fbCtr.InitData();
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            if (conserVation != null)
                UpdateInfo(conserVation);
        }
        #endregion

        #region 修改函数
        private void UpdateInfo(T_OA_MAINTENANCERECORD info)
        {
            T_SYS_DICTIONARY StrDepCity = cmbRepairName.SelectedItem as T_SYS_DICTIONARY;
            info.T_OA_MAINTENANCEAPP = info.T_OA_MAINTENANCEAPP;
            info.MAINTENANCETYPE = StrDepCity.DICTIONARYNAME.ToString();
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

            _VM.Upd_VMRecordAsync(info);

        }
        #endregion
       
        #region 修改
        void Upd_VMRecordCompleted(object sender, Upd_VMRecordCompletedEventArgs e)
        {
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
        #endregion

        #region 费用保存
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                if (ckbHasFee.IsChecked == true && Convert.ToInt32(txtFee.Text) > 0)
                {
                    fbCtr.Order.ORDERID = conserVation.MAINTENANCERECORDID;
                    fbCtr.Save(_flwResult);//提交费用               
                    actions = FormTypes.Edit;
                }
                else
                {
                    Utility.ShowMessageBox("ADD", isSubmitFlow, true);
                    if (isSubmitFlow)
                        saveType = RefreshedTypes.CloseAndReloadData;
                    RefreshUI(saveType);
                }
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
        #endregion

        #region CheckInputIsDecimal
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
        #endregion

        #region 验证
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
        #endregion
       
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

        #region StarEngine
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
        #endregion

        #region MyRegion
        void wcfclient_SaveTriggerDataCompleted(object sender, SaveTriggerDataCompletedEventArgs e)
        {
        }
        #endregion

        #region CheckBox_Click
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
        #endregion
        
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
            if (actions == FormTypes.Browse)
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