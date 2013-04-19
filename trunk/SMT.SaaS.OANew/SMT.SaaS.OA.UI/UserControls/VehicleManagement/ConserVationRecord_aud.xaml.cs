using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;

using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ConserVationRecord_aud : BaseForm,IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private T_OA_CONSERVATIONRECORD conserVation = null;
        private FormTypes types;
        public T_OA_CONSERVATIONRECORD ConserVation { get { return conserVation; } set { conserVation = value; } }
        #endregion
       
        #region 构造函数
        public ConserVationRecord_aud()
        {
            InitializeComponent();
            _VM.Upd_VCRecordCompleted += new EventHandler<Upd_VCRecordCompletedEventArgs>(Upd_VCRecordCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }
        public ConserVationRecord_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.types = operationType;
            _VM.Upd_VCRecordCompleted += new EventHandler<Upd_VCRecordCompletedEventArgs>(Upd_VCRecordCompleted);
            _VM.Get_VCRecordCompleted += new EventHandler<Get_VCRecordCompletedEventArgs>(Get_VCRecordCompleted);
            _VM.Get_VCRecordAsync(SendDocID);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Load_Data();
        }
        #endregion

        #region Load_Data
        private void Load_Data()
        {
            _VM.GetVehicleInfoListAsync(null);
            Utility.BindComboBox(cmbConserVationName, "CONSERVANAME", 0);

            SetForms();
        }
        #endregion

        #region SetForms
        private void SetForms()
        {
            dateREPAIRDATE.IsEnabled = false;
            dateRETRIEVEDATE.IsEnabled = false;
            txtTel.IsReadOnly = true;
            txtCONSERVATIONRANGE.IsReadOnly = true;
            txtContent.IsReadOnly = true;
            txtReMark.IsReadOnly = true;
            ckbHasFee.IsEnabled = false;
            scvFB.IsEnabled = false;
            //btnLookUpDepartment.IsEnabled = false;
            //slvView.Height = 200;
            //InitAudit(vehicleUsrApp.VEHICLEUSEAPPID);
        }
        #endregion

        #region 费用控件 审核界面
        /// <summary>
        /// 费用控件 审核界面
        /// </summary>
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";
            //fbCtr.Order.ORDERID = "";
            //fbCtr.Order.ORDERCODE = "JGZC" + string.Format("{0:yyyyMMddHHmmssffff}", System.DateTime.Now);
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

            fbCtr.InitData(false);
            //add,update  fbCtr.InitData();

        }
        #endregion
        
        #region 修改函数
        private void UpdateInfo()
        {
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
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                //InitAudit(conserVation.CONSERVATIONRECORDID);
                InitFBControl();
            }
        }
        #endregion
        
        #region 费用保存
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
        public void DoAction(string actionType)
        {
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
        private void Save()
        {
            UpdateInfo();
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
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