using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.EngineDataSource;

using SMT.SaaS.OA.UI.Class;
using System.Windows.Data;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ConserVationRecord_upd : BaseForm, IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private T_OA_CONSERVATIONRECORD conserVation = null;
        public T_OA_CONSERVATIONRECORD ConserVation { get { return conserVation; } set { conserVation = value; } }
        private string editState = null;
        public string EditState { get { return editState; } set { editState = value; } }
        private FormTypes types;
        private bool isFlowed = false;
        #endregion

        #region 构造函数
        public ConserVationRecord_upd(FormTypes type, string SendDocID)
        {
            InitializeComponent();
            this.types = type;
            _VM.Upd_VCRecordCompleted += new EventHandler<Upd_VCRecordCompletedEventArgs>(Upd_VCRecordCompleted);
            _VM.Get_VCRecordCompleted += new EventHandler<Get_VCRecordCompletedEventArgs>(Get_VCRecordCompleted);
            _VM.Get_VCRecordAsync(SendDocID);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            this.Loaded += new RoutedEventHandler(LayoutRoot_Loaded);
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _VM.GetVehicleInfoListAsync(null);
            Utility.BindComboBox(cmbConserVationName, "CONSERVANAME", 0);

            dateREPAIRDATE.Text = DateTime.Today.ToShortDateString();
            dateRETRIEVEDATE.Text = DateTime.Today.AddDays(2).ToShortDateString();
        }
        #endregion

        #region 根据ID查询
        private void Get_VCRecordCompleted(object sender, Get_VCRecordCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                conserVation = e.Result;

                if (conserVation != null)
                {
                    txtMAINTENANCEAPPID.Text = conserVation.CONSERVATIONRECORDID;
                    Utility.SetComboboxSelectByText(cmbConserVationName, conserVation.CONSERVATYPE, -1);
                    dateREPAIRDATE.Text = Convert.ToDateTime(conserVation.REPAIRDATE).ToShortDateString();
                    dateRETRIEVEDATE.Text = Convert.ToDateTime(conserVation.RETRIEVEDATE).ToShortDateString();

                    txtTel.Text = conserVation.TEL;
                    txtContent.Text = conserVation.CONTENT;
                    txtReMark.Text = conserVation.REMARK == null ? "" : conserVation.REMARK;
                    txtCONSERVATIONRANGE.Text = conserVation.CONSERVATIONRANGE.ToString();
                    if (conserVation.T_OA_CONSERVATION != null)
                    {
                        txtVehicleVIN.Text = conserVation.T_OA_CONSERVATION.T_OA_VEHICLE.VIN;
                    }
                    txtFee.Text = conserVation.CHARGEMONEY.ToString();
                    ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;
                }
                if (types == FormTypes.Resubmit)//重新提交
                {
                    conserVation.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                InitFBControl();
            }
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

            fbCtr.Order.ORDERID = conserVation.CONSERVATIONRECORDID;//费用对象

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

        #region 修改函数
        private void UpdateInfo()
        {
            conserVation.CONSERVATYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbConserVationName.SelectedItem).DICTIONARYNAME.ToString();
            conserVation.TEL = txtTel.Text;
            conserVation.CONTENT = txtContent.Text;
            conserVation.REMARK = txtReMark.Text;

            conserVation.REPAIRDATE = DateTime.Parse(dateREPAIRDATE.Text);
            conserVation.RETRIEVEDATE = DateTime.Parse(dateRETRIEVEDATE.Text);
            conserVation.CONSERVATIONRANGE = txtCONSERVATIONRANGE.Text.Trim() == "" ? 0 : int.Parse(txtCONSERVATIONRANGE.Text);

            if (ckbHasFee.IsChecked == true)
            {
                conserVation.ISCHARGE = "1";
                conserVation.CHARGEMONEY = txtFee.Text.Trim() == "" ? 0 : fbCtr.Order.TOTALMONEY;
            }
            else
            {
                conserVation.ISCHARGE = "0";
                conserVation.CHARGEMONEY = decimal.Parse("0");
            }
            conserVation.UPDATEDATE = System.DateTime.Now;
            conserVation.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            _VM.Upd_VCRecordAsync(conserVation);
        }
        #endregion

        #region 修改
        private void Upd_VCRecordCompleted(object sender, Upd_VCRecordCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                if (e.Result > 0)
                {
                    if (ckbHasFee.IsChecked == true)
                    {
                        fbCtr.Order.ORDERID = conserVation.CONSERVATIONRECORDID;
                        fbCtr.Save(_flwResult);//提交费用
                    }
                    else
                    {
                        Utility.ShowMessageBox("UPDATE", isFlowed, true);
                        if (isFlowed)
                            saveType = RefreshedTypes.CloseAndReloadData;
                        RefreshUI(saveType);
                    }
                }
                else
                {
                    Utility.ShowMessageBox("UPDATE", isFlowed, false);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 费用保存完成
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

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("ConserVationRecord");
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
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
                    saveType = RefreshedTypes.ProgressBar;
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

            if (txtVehicleVIN.Text.Length == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("PLEASESELECT", "Vehicle"));
                return false;
            }
            if (cmbConserVationName.SelectedIndex < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("PLEASESELECT", "ConserVationName"));
                return false;
            }
            if (string.IsNullOrEmpty(txtCONSERVATIONRANGE.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "CONSERVATIONRANGE"));
                return false;
            }
            if (string.IsNullOrEmpty(this.txtTel.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "APPTEL"));
                return false;
            }
            if (ckbHasFee.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtFee.Text))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "MaintenanceFees"));
                    return false;
                }
                if (!CheckInputIsDecimal(txtFee.Text.Trim()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("IsDouble", "MaintenanceFees"));
                    return false;
                }
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
        private void Save()
        {

            UpdateInfo();
        }

        private void SaveAndClose()
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
            //if (e. == 1)
            //{
            //    MessageBox.Show("成功!");
            //}
        }
        #endregion

        #region TextBlock_KeyUp
        private void TextBlock_KeyUp(object sender, KeyEventArgs e)
        {
            GlobalFunction.TextBoxInputDecimal(sender, e);
        }
        #endregion

        #region TextBox_KeyUp
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {

        }
        #endregion

        #region CheckBox_Click
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
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("BorrowMoney", fbCtr.GetParamenter().BorrowMoney.ToString());
            strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_CONSERVATIONRECORD>(conserVation, "OA", parameters);
            Utility.SetAuditEntity(entity, "T_OA_CONSERVATIONRECORD", conserVation.CONSERVATIONRECORDID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
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
            conserVation.CHECKSTATE = state;
            isFlowed = true;
            Save();
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