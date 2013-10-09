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
    public partial class VehicleDispatchRecord_aud : BaseForm, IClient, IEntityEditor, IAudit
    {
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        /// <summary>
        /// 存放选中的调度记录
        /// </summary>
        private List<T_OA_VEHICLEDISPATCHRECORD> _lstRecord = new List<T_OA_VEHICLEDISPATCHRECORD>();

        /// <summary>
        /// 车辆信息
        /// </summary>
        public List<T_OA_VEHICLE> _lstVehicle = new List<T_OA_VEHICLE>();
        private T_OA_VEHICLEDISPATCHRECORD vehicleDispatchRecord = null;
        private string vdrId = string.Empty;
        private bool isSubmitFlow = false;
        private FormTypes types;
        public T_OA_VEHICLEDISPATCHRECORD VehicleDispatchRecord
        {
            get { return vehicleDispatchRecord; }
            set { vehicleDispatchRecord = value; }
        }
        public VehicleDispatchRecord_aud()
        {
            InitializeComponent();
            _VM.Upd_VDRecordCompleted += new EventHandler<Upd_VDRecordCompletedEventArgs>(Upd_VDRecordCompleted);
            _VM.Get_VDRecordCompleted += new EventHandler<Get_VDRecordCompletedEventArgs>(Get_VDRecordCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }
        public VehicleDispatchRecord_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.vdrId = SendDocID;
            this.types = operationType;
            _VM.Upd_VDRecordCompleted += new EventHandler<Upd_VDRecordCompletedEventArgs>(Upd_VDRecordCompleted);
            _VM.Get_VDRecordCompleted += new EventHandler<Get_VDRecordCompletedEventArgs>(Get_VDRecordCompleted);
            _VM.Get_VDRecordAsync(SendDocID);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            SetForms();
            if (vehicleDispatchRecord != null)
            {
                _VM.Get_VDRecordAsync(VehicleDispatchRecord.VEHICLEDISPATCHRECORDID);
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
            InitFBControl();
        }
        /// <summary>
        /// 费用控件 审核界面
        /// </summary>
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";

            fbCtr.Order.ORDERID = VehicleDispatchRecord.VEHICLEDISPATCHRECORDID;//费用对象

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
        }

        private void SetForms()
        {
            txtREMARK.IsReadOnly = true;
            txtDriverID.IsReadOnly = true;
            txtNum.IsReadOnly = true;
            txtRoute.IsReadOnly = true;
            txtTel.IsReadOnly = true;
            dtiEndDate.IsReadOnly = true;
            dtiStartDate.IsReadOnly = true;
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
        }

        private void Upd_VDRecord()
        {
            _lstRecord[0].NUM = txtNum.Text;
            _lstRecord[0].STARTTIME = dtiStartDate.DateTimeValue;
            _lstRecord[0].ENDTIME = dtiEndDate.DateTimeValue;
            _lstRecord[0].TEL = txtTel.Text;
            _lstRecord[0].ROUTE = txtRoute.Text;
            _lstRecord[0].FUEL = decimal.Parse(txtFuel.Text);
            _lstRecord[0].RANGE = decimal.Parse(txtRange2.Text);
            _lstRecord[0].ISCHARGE = (bool)ckbHasFee.IsChecked ? "1" : "0";
            _lstRecord[0].CHARGEMONEY = decimal.Parse(txtFee.Text);
            _lstRecord[0].CONTENT = txtREMARK.Text;
            _lstRecord[0].CHECKSTATE = vehicleDispatchRecord.CHECKSTATE;

            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> o = new ObservableCollection<T_OA_VEHICLEDISPATCHRECORD>();
            o.Add(_lstRecord[0]);
            _VM.Upd_VDRecordAsync(o);
        }
        private void Upd_VDRecordCompleted(object sender, Upd_VDRecordCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                if (ckbHasFee.IsChecked == true)
                {
                    fbCtr.Order.ORDERID = vehicleDispatchRecord.VEHICLEDISPATCHRECORDID;
                    fbCtr.Save(_flwResult);//提交费用
                }
                else
                {
                    Utility.ShowMessageBox("SUCCESSAUDIT", true, true);
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
            }
            else
            {
                Utility.ShowMessageBox("FAILURETOAPPROVE", true, false);
                RefreshUI(RefreshedTypes.ProgressBar);
            }
        }
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                Utility.ShowMessageBox("AUDITFAILURE", true, false);
                RefreshUI(RefreshedTypes.ProgressBar);
            }
            else
            {
                Utility.ShowMessageBox("AUDITSUCCESSED", true, true);
                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }
        #region IEntityEditor
        public string GetTitle()
        {
            if (types == FormTypes.Audit)
            {
                return Utility.GetResourceStr("AUDIT");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE");
            }
        }

        public string GetStatus() { return ""; }
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
        private void Save()
        {
            Upd_VDRecord();
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

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