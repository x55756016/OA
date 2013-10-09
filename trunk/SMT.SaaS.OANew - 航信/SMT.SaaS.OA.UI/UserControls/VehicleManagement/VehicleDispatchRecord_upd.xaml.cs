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
using System.Windows.Data;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class VehicleDispatchRecord_upd : BaseForm, IClient, IEntityEditor, IAudit
    {
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        /// <summary>
        /// 车辆信息
        /// </summary>
        public List<T_OA_VEHICLE> _lstVehicle = new List<T_OA_VEHICLE>();
        private T_OA_VEHICLEDISPATCHRECORD vehicleDispatchRecord = null;
        private List<T_OA_VEHICLEDISPATCHRECORD> _lstRecord = new List<T_OA_VEHICLEDISPATCHRECORD>();
        public T_OA_VEHICLEDISPATCHRECORD VehicleDispatchRecord { get { return vehicleDispatchRecord; } set { vehicleDispatchRecord = value; } }
        private FormTypes types;
        private bool isSubmitFlow = false;
        private string vdrId = string.Empty;

        public VehicleDispatchRecord_upd(FormTypes type, string SendDocID)
        {
            InitializeComponent();
            this.types = type;
            this.vdrId = SendDocID;
            _VM.Upd_VDRecordCompleted += new EventHandler<Upd_VDRecordCompletedEventArgs>(Upd_VDRecordCompleted);
            _VM.Get_VDRecordCompleted += new EventHandler<Get_VDRecordCompletedEventArgs>(Get_VDRecordCompleted);
            _VM.Get_VDRecordAsync(SendDocID);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
        }
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();

        //获取详细信息
        private void Get_VDRecordCompleted(object sender, Get_VDRecordCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> o = e.Result;
            _lstRecord.AddRange(o);
            vehicleDispatchRecord = e.Result[0];
            txtVehicleInfo.Text = _lstRecord[0].T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.T_OA_VEHICLE.VIN;
            txtNum.Text = _lstRecord[0].NUM;
            dtiStartDate.DateTimeValue = Convert.ToDateTime(_lstRecord[0].STARTTIME);
            dtiEndDate.DateTimeValue = Convert.ToDateTime(_lstRecord[0].ENDTIME);
            txtDriverID.Text = _lstRecord[0].OWNERID;
            txtDriverName.Text = _lstRecord[0].OWNERNAME;
            txtTel.Text = _lstRecord[0].TEL;
            txtRoute.Text = _lstRecord[0].ROUTE;
            txtREMARK.Text = _lstRecord[0].CONTENT == null ? "" : _lstRecord[0].CONTENT;
            txtFuel.Text = _lstRecord[0].FUEL.ToString();
            ckbHasFee.IsChecked = _lstRecord[0].ISCHARGE == "0" ? false : true;
            txtFee.Text = _lstRecord[0].CHARGEMONEY.ToString();
            txtRange2.Text = _lstRecord[0].RANGE.ToString();

            if (types == FormTypes.Resubmit)//重新提交
            {
                _lstRecord[0].CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            }
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            InitFBControl();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //txtVehicleInfo.Text = vehicleDispatchRecord.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.T_OA_VEHICLE.VIN;
            //txtNum.Text = vehicleDispatchRecord.NUM;
            //dtiStartDate.DateTimeValue = vehicleDispatchRecord.STARTTIME;
            //dtiEndDate.DateTimeValue = vehicleDispatchRecord.ENDTIME;
            //txtDriverID.Text = vehicleDispatchRecord.OWNERID;
            //txtDriverName.Text = vehicleDispatchRecord.OWNERNAME;
            //txtTel.Text = vehicleDispatchRecord.TEL;
            //txtRoute.Text = vehicleDispatchRecord.ROUTE;
            //txtREMARK.Text = vehicleDispatchRecord.CONTENT == null ? "" : vehicleDispatchRecord.CONTENT;
            //txtFuel.Text = vehicleDispatchRecord.FUEL.ToString();
            //ckbHasFee.IsChecked = vehicleDispatchRecord.ISCHARGE == "0" ? false : true;
            //txtFee.Text = vehicleDispatchRecord.CHARGEMONEY.ToString();
            //txtRange2.Text = vehicleDispatchRecord.RANGE.ToString();

            //if (types == FormTypes.Resubmit)//重新提交
            //{
            //    vehicleDispatchRecord.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            //}
            //InitAudit(vehicleDispatchRecord.VEHICLEDISPATCHRECORDID);
            //InitFBControl();
        }
        /// <summary>
        /// 费用控件 修改页面调用
        /// </summary>
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";

            fbCtr.Order.ORDERID = _lstRecord[0].VEHICLEDISPATCHRECORDID;//费用对象

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
        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = dtiStartDate.Group1.ValidateAll();
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
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }

            validators = Group1.ValidateAll();
            if (validators.Count > 0)
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            if (dtiStartDate.DateTimeValue >= dtiEndDate.DateTimeValue)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("StartEndTime"));
                return false;
            }
            return true;
        }

        private bool IsNumber(string textContent)
        {
            if (textContent.Trim() == null || textContent.Trim() == string.Empty)
                return false;
            foreach (char c in textContent)
                if (!char.IsNumber(c))
                    return false;
            return true;
        }

        private bool IsNullOrEmpty(string textContent)
        {
            if (textContent.Trim() == null || textContent.Trim() == string.Empty || textContent.Trim() == "")
                return false;
            return true;
        }
        private void Upd_VDRecord()
        {
            vehicleDispatchRecord.NUM = txtNum.Text;
            vehicleDispatchRecord.STARTTIME = dtiStartDate.DateTimeValue;
            vehicleDispatchRecord.ENDTIME = dtiEndDate.DateTimeValue;
            vehicleDispatchRecord.TEL = txtTel.Text;
            vehicleDispatchRecord.ROUTE = txtRoute.Text;
            vehicleDispatchRecord.FUEL = decimal.Parse(txtFuel.Text);
            vehicleDispatchRecord.RANGE = decimal.Parse(txtRange2.Text);
            vehicleDispatchRecord.ISCHARGE = (bool)ckbHasFee.IsChecked ? "1" : "0";
            vehicleDispatchRecord.CHARGEMONEY = decimal.Parse(txtFee.Text);
            vehicleDispatchRecord.CONTENT = txtREMARK.Text;

            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> o = new ObservableCollection<T_OA_VEHICLEDISPATCHRECORD>();
            o.Add(vehicleDispatchRecord);
            _VM.Upd_VDRecordAsync(o, "Edit");
        }
        private void Upd_VDRecordCompleted(object sender, Upd_VDRecordCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                if (e.Result > 0)
                {
                    if (ckbHasFee.IsChecked == true)
                    {
                        fbCtr.Order.ORDERID = _lstRecord[0].VEHICLEDISPATCHRECORDID;
                        fbCtr.Save(_flwResult);//提交费用
                    }
                    else
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
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                Utility.ShowMessageBox("UPDATE", isSubmitFlow, false);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", isSubmitFlow, true);
                if (isSubmitFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
            }
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VehicleDispatchRecord_upd");
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
                    isSubmitFlow = false;
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
                    saveType = RefreshedTypes.ShowProgressBar;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
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
            Upd_VDRecord();
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        private void txtNum_KeyUp(object sender, KeyEventArgs e)
        {
            GlobalFunction.TextBoxInputInt(sender, e);
        }
        //查找司机
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
                    vehicleDispatchRecord.OWNERCOMPANYID = empInfo.OWNERCOMPANYID;
                    vehicleDispatchRecord.OWNERDEPARTMENTID = empInfo.OWNERDEPARTMENTID;
                    vehicleDispatchRecord.OWNERID = empInfo.EMPLOYEEID;
                    vehicleDispatchRecord.OWNERNAME = empInfo.EMPLOYEEENAME;
                    vehicleDispatchRecord.OWNERPOSTID = empInfo.T_HR_EMPLOYEEPOST.FirstOrDefault().EMPLOYEEPOSTID;
                    txtDriverName.Text = companyInfo.ObjectName;
                    txtDriverID.Text = companyInfo.ObjectID;
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_VEHICLEDISPATCHRECORD>(vehicleDispatchRecord, "OA");
            Utility.SetAuditEntity(entity, "T_OA_VEHICLEDISPATCHRECORD", vehicleDispatchRecord.VEHICLEDISPATCHRECORDID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = string.Empty;
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
            if (vehicleDispatchRecord.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            vehicleDispatchRecord.CHECKSTATE = state;
            isSubmitFlow = true;
            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> o = new ObservableCollection<T_OA_VEHICLEDISPATCHRECORD>();
            o.Add(vehicleDispatchRecord);
            _VM.Upd_VDRecordAsync(o, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (vehicleDispatchRecord != null)
                state = vehicleDispatchRecord.CHECKSTATE;
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