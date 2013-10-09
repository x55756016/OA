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

using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.EngineDataSource;

using System.Reflection;
using System.Text;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ConserVationForm_aud : BaseForm, IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private FormTypes types;
        private bool isFlowed = false;
        private T_OA_CONSERVATION conserVation = null;
        public T_OA_CONSERVATION ConserVation { get { return conserVation; } set { conserVation = value; } }
        public ConserVationForm_aud()
        {
            InitializeComponent();
            _VM.UpdateConserVationCompleted += new EventHandler<UpdateConserVationCompletedEventArgs>(UpdateConserVationCompleted);
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(GetVehicleInfoListCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }
        #endregion
       
        #region 构造函数
        public ConserVationForm_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.types = operationType;
            _VM.UpdateConserVationCompleted += new EventHandler<UpdateConserVationCompletedEventArgs>(UpdateConserVationCompleted);
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(GetVehicleInfoListCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            _VM.Get_VConserVationCompleted += new EventHandler<Get_VConserVationCompletedEventArgs>(Get_VConserVationCompleted);
            _VM.Get_VConserVationAsync(SendDocID);
            if (operationType == FormTypes.Audit || operationType == FormTypes.Browse)
            {
                cmbVehicleAssetId.IsEnabled = false;
                cmbConserVationName.IsEnabled = false;
                dateREPAIRDATE.IsEnabled = false;
                dateRETRIEVEDATE.IsEnabled = false;
                txtREPAIRCOMPANY.IsEnabled = false;
                txtTel.IsEnabled = false;
                txtCONSERVATIONRANGE.IsEnabled = false;
                txtContent.IsEnabled = false;
                txtReMark.IsEnabled = false;
                ckbHasFee.IsEnabled = false;
            }
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
        }
        #endregion
       
        #region 费用控件
        /// <summary>
        /// 费用控件
        /// </summary>
        private void InitFBControl()
        {
            //fbCtr.Order.ORDERID = "";
            //fbCtr.Order.ORDERCODE = "JGZC" + string.Format("{0:yyyyMMddHHmmssffff}", System.DateTime.Now);
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";

            fbCtr.Order.ORDERID = conserVation.CONSERVATIONID;//费用对象

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

        #region 保存函数
        private void Save()
        {
            if (conserVation != null)
                UpdateInfo();
        }
        #endregion
       
        #region 修改函数
        private void UpdateInfo()
        {
            _VM.UpdateConserVationAsync(conserVation);
        }
        #endregion
       
        #region 获取车辆信息
        void GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfoList = e.Result;
            if (vehicleInfoList != null)
            {
                if (conserVation == null)
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
                    {
                        txtFee.Visibility = Visibility.Visible;
                    }
                    if (conserVation.T_OA_VEHICLEReference.EntityKey != null)
                    {
                        SetComboBoxSelect(vehicleInfoList, conserVation.T_OA_VEHICLEReference.EntityKey.EntityKeyValues[0].Value.ToString());
                    }
                }
            }
        }
        #endregion
       
        #region 修改
        private void UpdateConserVationCompleted(object sender, UpdateConserVationCompletedEventArgs e)
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
                        fbCtr.Order.ORDERID = conserVation.CONSERVATIONID;
                        fbCtr.Save(_flwResult);//提交费用
                    }
                    else
                    {
                        Utility.ShowMessageBox("APPOVALBUTTON", true, true);
                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                }
                else
                {
                    Utility.ShowMessageBox("APPOVALBUTTON", isFlowed, false);
                    RefreshUI(RefreshedTypes.HideProgressBar);
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
       
        #region 根据ID查询
        void Get_VConserVationCompleted(object sender, Get_VConserVationCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                conserVation = e.Result;

                if (conserVation != null)
                {
                    Utility.SetComboboxSelectByText(cmbConserVationName, conserVation.CONSERVATYPE, -1);
                    txtContent.Text = conserVation.CONTENT;
                    txtFee.Text = conserVation.CHARGEMONEY.ToString();
                    ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;

                    txtContent.IsEnabled = true;
                    txtFee.IsEnabled = true;
                    cmbConserVationName.IsEnabled = false;

                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                    //InitAudit(conserVation.CONSERVATIONID);
                    InitFBControl();
                    //viewApproval.XmlObject = DataObjectToXml<T_OA_CONSERVATION>.ObjListToXml(conserVation, "OA");
                }
            }
        }
        #endregion
       
        #region SetComboBoxSelect
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
        #endregion
        
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
            strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_CONSERVATION>(conserVation, "OA", parameters);
            Utility.SetAuditEntity(entity, "T_OA_CONSERVATION", conserVation.CONSERVATIONID, strXmlObjectSource);
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