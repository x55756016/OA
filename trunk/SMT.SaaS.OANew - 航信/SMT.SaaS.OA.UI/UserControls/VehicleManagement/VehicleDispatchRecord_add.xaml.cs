using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
    public partial class VehicleDispatchRecord_add : BaseForm,IClient, IEntityEditor, IAudit
    {
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        VehicleDispatchForm_sel frmD;
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        private RefreshedTypes saveType;
        private FormTypes types;
        bool isSubmitFlow = false;
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        /// <summary>
        /// 存放dg选中的 记录
        /// </summary>
        private T_OA_VEHICLEDISPATCHRECORD _record;
        /// <summary>
        /// 存放要保存的 Record记录
        /// </summary>
        private List<T_OA_VEHICLEDISPATCHRECORD> _lstVRecord = new List<T_OA_VEHICLEDISPATCHRECORD>();
        /// <summary>
        /// 车辆信息
        /// </summary>
        public List<T_OA_VEHICLE> _lstVehicle = new List<T_OA_VEHICLE>();
        public VehicleDispatchRecord_add(FormTypes type)
        {
            InitializeComponent();
            _record = new T_OA_VEHICLEDISPATCHRECORD();
            _record.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            this.types = type;
            _VM.Add_VDRecordCompleted += new EventHandler<Add_VDRecordCompletedEventArgs>(Add_VDRecordCompleted);
            _VM.Upd_VDRecordCompleted += new EventHandler<Upd_VDRecordCompletedEventArgs>(Upd_VDRecordCompleted);
            //车辆
            _VM.GetCanUseVehicleInfoListCompleted += new EventHandler<GetCanUseVehicleInfoListCompletedEventArgs>(GetCanUseVehicleInfoListCompleted);
            //派车单
            _VM.Get_VDInfoCompleted += new EventHandler<Get_VDInfoCompletedEventArgs>(Get_VDInfoCompleted);
            _VM.Del_VDDetailsCompleted += new EventHandler<Del_VDDetailsCompletedEventArgs>(Del_VDDetailsCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);

            personclient.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(personclient_GetEmployeeDetailByIDCompleted);
            
        }

        void personclient_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    string PostName = "";
                    string DepartmentName = "";
                    string CompanyName = "";
                    string StrName = "";
                    PostName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                    DepartmentName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    CompanyName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    StrName = e.Result.T_HR_EMPLOYEE.EMPLOYEECNAME + "-" + PostName + "-" + DepartmentName + "-" + CompanyName;
                    txtDriverName.Text = StrName;

                    ToolTipService.SetToolTip(txtDriverName, StrName);
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
        }
        
        //加载
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //获取车辆 1
            _VM.GetCanUseVehicleInfoListAsync();

            dtiStartDate.DateTimeValue = System.DateTime.Now;
            dtiEndDate.DateTimeValue = dtiStartDate.DateTimeValue.AddHours(1);
            
            InitFBControl();
        }
        /// <summary>
        /// 初始化 费用控件 add
        /// </summary>
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";
            fbCtr.Order.ORDERID = "";
            //fbCtr.Order.ORDERCODE = "JGZC" + string.Format("{0:yyyyMMddHHmmssffff}", System.DateTime.Now);
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

        //保存
        private void AddVehicleDispatch()
        {
            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> lst = new ObservableCollection<T_OA_VEHICLEDISPATCHRECORD>();

            T_OA_VEHICLEDISPATCHRECORD r = dg.SelectedItem as T_OA_VEHICLEDISPATCHRECORD;

            r.NUM = txtNum.Text;

            r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.T_OA_VEHICLE = cmbVehicleInfo.SelectedItem as T_OA_VEHICLE;

            r.STARTTIME = dtiStartDate.DateTimeValue;
            r.ENDTIME = dtiEndDate.DateTimeValue;
            r.TEL = txtTel.Text;
            r.ROUTE = txtRoute.Text;
            r.FUEL = decimal.Parse(txtFuel.Text);
            r.RANGE = decimal.Parse(txtRange2.Text);
            r.ISCHARGE = (bool)ckbHasFee.IsChecked ? "1" : "0";
            r.CONTENT = txtREMARK.Text;
            r.CHARGEMONEY = decimal.Parse(txtFee.Text);
            r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.DRIVER = txtDriverID.Text;
            lst.Add(r);

            if (r.VEHICLEDISPATCHRECORDID == null)
            {
                r.VEHICLEDISPATCHRECORDID = System.Guid.NewGuid().ToString();
                _VM.Add_VDRecordAsync(lst);
            }
            else
                _VM.Upd_VDRecordAsync(lst);

            // _record = r;
        }
        private void Add_VDRecordCompleted(object sender, Add_VDRecordCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                if (ckbHasFee.IsChecked == true)
                {
                    fbCtr.Order.ORDERID = _record.VEHICLEDISPATCHRECORDID;
                    fbCtr.Save(_flwResult);//提交费用
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
                RefreshUI(RefreshedTypes.HideProgressBar);
            }

        }
        private void Upd_VDRecordCompleted(object sender, Upd_VDRecordCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                if (ckbHasFee.IsChecked == true)
                {
                    fbCtr.Order.ORDERID = _record.VEHICLEDISPATCHRECORDID;
                    fbCtr.Save(_flwResult);//提交费用
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
                Utility.ShowMessageBox("UPDATE", isSubmitFlow, false);
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
        //获取车辆 2
        private void GetCanUseVehicleInfoListCompleted(object sender, GetCanUseVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> o = e.Result;
            _lstVehicle = new List<T_OA_VEHICLE>(o);
            if (_lstVehicle != null)
            {
                cmbVehicleInfo.ItemsSource = _lstVehicle;
                cmbVehicleInfo.DisplayMemberPath = "VIN";
                cmbVehicleInfo.SelectedIndex = 0;
            }
        }

        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = dtiStartDate.Group1.ValidateAll();
            if (validators.Count > 0)
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
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
            if (_lstVRecord.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("ERROR_VDUSEAPP"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return false;
            }
            //zl
            if (!IsNumber(txtNum.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IsInt1"));
                return false;
            }

            if (!IsNumber(txtTel.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IsInt2"));
                return false;
            }

            if (!IsNumeric(txtRange2.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IsNum"));
                return false;
            }

            if (!IsNumeric(txtFuel.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IsNum1"));
                return false;
            }

            if (!IsNumeric(txtFee.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IsNum2"));
                return false;
            }
            
            //zl
            return true;
        }
        //zl
        private bool IsNumeric(string value)
        {
            try
            {
                Decimal tmp = Convert.ToDecimal(value);
                return true;
            }
            catch
            {
                return false;
            }
        } 
        //zl
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

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VehicleDispatchRecord_add");
        }
        public void DoAction(string actionType)
        {
            if (actionType.ToInt32() == 2)
                sel_vDipatch();
            else
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
        }
        public string GetStatus()
        {
            return "";
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            object[,] arr = new object[,]{
             {ToolbarItemDisplayTypes.Image,"2","BTNVEHICLENO","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"},           
             {ToolbarItemDisplayTypes.Image,"1","SAVEANDCLOSE", "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
             {ToolbarItemDisplayTypes.Image,"0","SAVE","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
            };
            return VehicleMgt.GetToolBarItems(ref arr);
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


        /// <summary>
        /// 选择已审核通过的派车单 1
        /// </summary>
        private void sel_vDipatch()
        {
            frmD = new VehicleDispatchForm_sel(ref _lstVehicle, cmbVehicleInfo.SelectedIndex, dtiStartDate.DateTimeValue);
            EntityBrowser browser = new EntityBrowser(frmD);
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(sel_vDipatch_end);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        // 选择已审核通过的派车单 2
        private void sel_vDipatch_end()
        {
            if (frmD._lstVDispatch != null && frmD._lstVDispatch.Count > 0)
            {
                //加载已经派车的申请用车数据
                _VM.Get_VDInfoAsync(frmD._lstVDispatch[0].VEHICLEDISPATCHID);

                txtNum.Text = frmD._lstVDispatch[0].NUM;
                dtiStartDate.DateTimeValue = Convert.ToDateTime(frmD._lstVDispatch[0].STARTTIME);
                dtiEndDate.DateTimeValue = Convert.ToDateTime(frmD._lstVDispatch[0].ENDTIME);
                txtDriverID.Text = frmD._lstVDispatch[0].DRIVER;
                //txtDriverName.Text = frmD._lstVDispatch[0].DRIVER;
                personclient.GetEmployeeDetailByIDAsync(frmD._lstVDispatch[0].OWNERID);
                txtTel.Text = frmD._lstVDispatch[0].TEL;
                txtRoute.Text = frmD._lstVDispatch[0].ROUTE;
                txtREASON.Text = frmD._lstVDispatch[0].CONTENT;
            }
        }
        //获取已经派车的申请用车数据
        void Get_VDInfoCompleted(object sender, Get_VDInfoCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                _lstVRecord.Clear();
                ObservableCollection<T_OA_VEHICLEDISPATCH> lst = e.Result;
                if (lst != null)
                {
                    foreach (T_OA_VEHICLEDISPATCHDETAIL info in lst[0].T_OA_VEHICLEDISPATCHDETAIL.ToList())
                    //foreach (T_OA_VEHICLEDISPATCHDETAIL info in lst[0].tToList())
                    {
                        T_OA_VEHICLEDISPATCHRECORD r = new T_OA_VEHICLEDISPATCHRECORD();
                        r.T_OA_VEHICLEDISPATCHDETAIL = info;
                        r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH = info.T_OA_VEHICLEDISPATCH;

                        r.T_OA_VEHICLEDISPATCHDETAIL.VEHICLEDISPATCHDETAILID = info.VEHICLEDISPATCHDETAILID;
                        r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.VEHICLEDISPATCHID = info.T_OA_VEHICLEDISPATCH.VEHICLEDISPATCHID;
                        r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.DRIVER = info.T_OA_VEHICLEDISPATCH.DRIVER;
                        r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.T_OA_VEHICLE = frmD._lstVDispatch[0].T_OA_VEHICLE;

                        r.STARTTIME = info.T_OA_VEHICLEDISPATCH.STARTTIME;
                        r.ENDTIME = info.T_OA_VEHICLEDISPATCH.ENDTIME;
                        r.NUM = info.T_OA_VEHICLEDISPATCH.NUM;
                        r.ROUTE = info.T_OA_VEHICLEDISPATCH.ROUTE;
                        r.TEL = info.T_OA_VEHICLEDISPATCH.TEL;

                        r.FUEL = decimal.Parse("0.00");
                        r.RANGE = decimal.Parse("0.00");
                        r.CONTENT = "";
                        //r.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
                        //r.CHARGEMONEY = txtFee.Text == "" ? decimal.Parse("0.00") : decimal.Parse(txtFee.Text);

                        r.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        r.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        r.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        r.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        r.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        r.CREATEDATE = DateTime.Now;

                        r.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        r.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        r.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        r.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        r.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                        r.CHECKSTATE = "0";
                        _lstVRecord.Add(r);
                    }
                    if (_lstVRecord.Count > 0)
                    {
                        dg.ItemsSource = _lstVRecord;
                        dg.SelectedIndex = 0;

                        RdoSelect(_lstVRecord[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        /// <summary>
        ///  dg 选择事件
        /// </summary>
        private void RdoSelect(T_OA_VEHICLEDISPATCHRECORD r)
        {
            _record = r;
            cmbVehicleInfo.SelectedItem = r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.T_OA_VEHICLE;
            txtNum.Text = r.NUM;
            dtiStartDate.DateTimeValue = Convert.ToDateTime(r.STARTTIME);
            dtiEndDate.DateTimeValue = Convert.ToDateTime(r.ENDTIME);
            txtTel.Text = r.TEL;
            txtRoute.Text = r.ROUTE;
            txtFuel.Text = r.FUEL.ToString();
            txtRange2.Text = r.RANGE.ToString();
            //ckbHasFee.IsChecked = r.ISCHARGE == "0" ? false : true;
            //txtFee.Text = r.CHARGEMONEY.ToString();
            txtREMARK.Text = r.CONTENT;

            txtREASON.Text = r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.CONTENT;
            //有用
            txtDriverID.Text = r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.DRIVER;
            // txtDriverName.Text = r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.DRIVER;
            PersonnelServiceClient client = new PersonnelServiceClient();
            client.GetEmployeeByIDAsync(r.T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.DRIVER);
            client.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(client_GetEmployeeByIDCompleted);
        }
        void client_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Result != null)
                txtDriverName.Text = ((T_HR_EMPLOYEE)e.Result).EMPLOYEECNAME;
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
            AddVehicleDispatch();
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

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
                    txtDriverName.Text = companyInfo.ObjectName;
                    txtDriverID.Text = companyInfo.ObjectID;
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }

        //删除子表
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_lstVRecord.Count > 1) //必须有派车单，司机才能根据派车单提交费用 单
            {
                T_OA_VEHICLEDISPATCHRECORD i = ((Button)sender).DataContext as T_OA_VEHICLEDISPATCHRECORD;
                _lstVRecord.Remove(i);
                dg.ItemsSource = null;
                dg.ItemsSource = _lstVRecord;

                if (i.VEHICLEDISPATCHRECORDID != null) //删除已经保存到服务器中的数据
                {
                    ObservableCollection<string> o = new ObservableCollection<string>();
                    o.Add(i.VEHICLEDISPATCHRECORDID);
                    _VM.Del_VDDetailsAsync(o);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
        }
        private void Del_VDDetailsCompleted(object sender, Del_VDDetailsCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                dg.SelectedIndex = 0;
                Utility.ShowMessageBox("DELETE", false, true);
            }
            else
                Utility.ShowMessageBox("DELETE", false, false);

            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        //行加载删除按钮
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_VEHICLEDISPATCHRECORD tmp = (T_OA_VEHICLEDISPATCHRECORD)e.Row.DataContext;
            ImageButton MyButton_Delbaodao = dg.Columns[7].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Delbaodao.Tag = tmp;

        }

        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg.SelectedIndex > -1)
                RdoSelect(dg.SelectedItem as T_OA_VEHICLEDISPATCHRECORD);
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ckbHasFee.IsChecked == true)
                scvFB.Visibility = Visibility.Visible;
            else
                scvFB.Visibility = Visibility.Collapsed;
        }


        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_VEHICLEDISPATCHRECORD>(_record, "OA");
            Utility.SetAuditEntity(entity, "T_OA_VEHICLEDISPATCHRECORD", _record.VEHICLEDISPATCHRECORDID, strXmlObjectSource);
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
            _record.CHECKSTATE = state;
            isSubmitFlow = true;
            Save();
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (_record != null)
                state = _record.CHECKSTATE;
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