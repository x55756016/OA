using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.EngineDataSource;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MaintenanceRecordForm_aud : BaseForm,IClient, IEntityEditor, IAudit
    {

        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private FormTypes types;

        private T_OA_MAINTENANCERECORD conserVation = null;
        public T_OA_MAINTENANCERECORD ConserVation { get { return conserVation; } set { conserVation = value; } }

        public MaintenanceRecordForm_aud()
        {
            InitializeComponent();
            _VM.Upd_VMRecordCompleted += new EventHandler<Upd_VMRecordCompletedEventArgs>(Upd_VMRecordCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }
        public MaintenanceRecordForm_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.types = operationType;
            _VM.Upd_VMRecordCompleted += new EventHandler<Upd_VMRecordCompletedEventArgs>(Upd_VMRecordCompleted);
            _VM.Get_VMRecordCompleted += new EventHandler<Get_VMRecordCompletedEventArgs>(Get_VMRecordCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            _VM.Get_VMRecordAsync(SendDocID);
            Load_Data();
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            dateREPAIRDATE.IsEnabled = false;
            dateRETRIEVEDATE.IsEnabled = false;
            txtREPAIRCOMPANY.IsEnabled = false;
            txtTel.IsEnabled = false;
            txtContent.IsEnabled = false;
            txtReMark.IsEnabled = false;
            ckbHasFee.IsEnabled = false;
            fbCtr.IsEnabled = false;
        }
        private void Load_Data()
        {
            Utility.BindComboBox(cmbRepairName, "MAINTENANCENAME", 0);
        }
        private void SetForms()
        {
            txtFee.IsReadOnly = true;
            cmbRepairName.IsEnabled = false;
            //InitAudit(conserVation.MAINTENANCERECORDID);
        }
        /// <summary>
        /// 费用控件 审核界面用 
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

            fbCtr.InitData(false);

        }
        private void Save()
        {
            UpdateInfo();
        }
        private void UpdateInfo()
        {
            conserVation.UPDATEDATE = System.DateTime.Now;
            conserVation.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            _VM.Upd_VMRecordAsync(conserVation);
        }
        void Upd_VMRecordCompleted(object sender, Upd_VMRecordCompletedEventArgs e)
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
                    if (ckbHasFee.IsChecked == true && Convert.ToInt32(txtFee.Text) > 0)
                    {
                        fbCtr.Order.ORDERID = conserVation.MAINTENANCERECORDID;
                        fbCtr.Save(_flwResult);//提交费用
                    }
                    else
                    {
                        Utility.ShowMessageBox("SUCCESSAUDIT", true, true);//审核成功
                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                }
                else
                {
                    Utility.ShowMessageBox("FAILURETOAPPROVE", true, false);//审核失败
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
            if (e.Message != null && e.Message.Count() > 0)
            {
                Utility.ShowMessageBox("UPDATE", true, false);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", true, true);
                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }
        void Get_VMRecordCompleted(object sender, Get_VMRecordCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                conserVation = e.Result;

                Utility.SetComboboxSelectByText(cmbRepairName, conserVation.CONTENT, -1);
                SetForms();
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

                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                InitFBControl();
                //viewApproval.XmlObject = DataObjectToXml<T_OA_MAINTENANCERECORD>.ObjListToXml(conserVation, "OA");
            }
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