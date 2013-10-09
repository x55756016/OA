using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
namespace SMT.SaaS.OA.UI.Views.VehicleManagement
{
    public partial class CFrmAccidentRecordManager : BaseForm, IClient, IEntityEditor
    {
        private SmtOACommonAdminClient vehicleManager = new SmtOACommonAdminClient();
        private FormTypes types;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;


        public CFrmAccidentRecordManager(FormTypes type)
        {
            InitializeComponent();
            
            this.types = type;
            vehicleManager.UpdateAccidentRecordCompleted += new EventHandler<UpdateAccidentRecordCompletedEventArgs>(vehicleManager_UpdateAccidentRecordCompleted);
            vehicleManager.AddAccidentRecordCompleted += new EventHandler<AddAccidentRecordCompletedEventArgs>(vehicleManager_AddAccidentRecordCompleted);
            vehicleManager.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(vehicleInfoManager_GetVehicleInfoListCompleted);
            this.Loaded += new RoutedEventHandler(CFrmAccidentRecordManager_Loaded);
        }

        void CFrmAccidentRecordManager_Loaded(object sender, RoutedEventArgs e)
        {
            if (types == FormTypes.Browse)
            {
                this.IsEnabled = false;
            }
        }
        void vehicleInfoManager_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfoList = e.Result;
            if (vehicleInfoList != null)
            {
                if (accidentInfo == null)
                {
                    SetComboBoxSelect(vehicleInfoList, null);
                }
                else
                {
                    SetComboBoxSelect(vehicleInfoList, accidentInfo.T_OA_VEHICLE.ASSETID);
                }
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
        void vehicleManager_AddAccidentRecordCompleted(object sender, AddAccidentRecordCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", ""));

                if (GlobalFunction.IsSaveAndClose(refreshType))
                {
                    RefreshUI(refreshType);
                }
                else
                {
                    types = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", ""));
                RefreshUI(refreshType);
            }
        }

        void vehicleManager_UpdateAccidentRecordCompleted(object sender, UpdateAccidentRecordCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                if (e.Result < 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("UPDATEFAILED", ""));
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", ""));
                    RefreshUI(refreshType);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void Save()
        {
            if (types == FormTypes.New)
            { AddInfo(); }
            if (types == FormTypes.Edit)
            {
                if (accidentInfo != null)
                {
                    UpdateInfo(accidentInfo);
                }
            }
        }

        private bool DataValidation()
        {
            if (cmbVehicleAssetId.SelectedIndex < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("PLEASESELECT", "Vehicle"));
                return false;
            }
            if (string.IsNullOrEmpty(txtContent.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "AccidentRecord"));
                return false;
            }
            if (string.IsNullOrEmpty(txtOwnerID.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "PartyPeople"));
                return false;
            }
            return true;
        }

        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (types != FormTypes.Browse)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1",
                    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                };
                items.Add(item);
                item = new ToolbarItem
               {
                   DisplayType = ToolbarItemDisplayTypes.Image,
                   Key = "0",
                   Title = Utility.GetResourceStr("SAVE"),
                   ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
               };
                items.Add(item);
            }
            return items;
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
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
            }
        }

        public event UIRefreshedHandler OnUIRefreshed;

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("AccidentRecord");
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            SetComboxData();
            if (accidentInfo != null)
            {
                SetFormDefailValue(accidentInfo);
            }
            else
            {
                dpDate.Text = System.DateTime.Now.ToShortDateString();
            }
            if (types == FormTypes.Edit)
            {
                cmbVehicleAssetId.IsEnabled = false;
            }
        }
        private void SetComboxData()
        {
            vehicleManager.GetVehicleInfoListAsync();
        }
        private void SetFormDefailValue(T_OA_ACCIDENTRECORD defaultInfo)
        {
            
            txtContent.Text = defaultInfo.CONTENT;
            dpDate.Text = defaultInfo.ACCIDENTDATE.ToShortDateString();
            txtOwnerID.Text = defaultInfo.OWNERID;
            txtOwnerName.Text = defaultInfo.OWNERNAME;
        }

        private void AddInfo()
        {
            
            string StrStart = string.Empty;
            StrStart = dpDate.Text.ToString();
            DateTime DtStart = new DateTime();
            if (!Check()) return;
            if (!string.IsNullOrEmpty(StrStart))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                if (DtStart > DateTime.Now)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEGREATERTHANTHECURRENTDATE", "ACCIDENTDATE"));
                    return;
                }
            }
            T_OA_ACCIDENTRECORD newInfo = new T_OA_ACCIDENTRECORD();
            newInfo.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleAssetId.SelectedItem;
            try
            {
                newInfo.ACCIDENTDATE = Convert.ToDateTime(dpDate.Text);
            }
            catch
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("ISNotAllowed", "AccidentTime"));
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (txtOwnerName.Text == "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "OWNERID"));

                return;
            }
            newInfo.CONTENT = txtContent.Text;
            newInfo.ACCIDENTRECORDID = System.Guid.NewGuid().ToString();
            newInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            newInfo.CREATEDATE = System.DateTime.Now;
            newInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            newInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            newInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            newInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            newInfo.OWNERCOMPANYID = accidentInfo.OWNERCOMPANYID;
            newInfo.OWNERDEPARTMENTID = accidentInfo.OWNERDEPARTMENTID;
            newInfo.OWNERPOSTID = accidentInfo.OWNERPOSTID;
            newInfo.OWNERNAME = accidentInfo.OWNERNAME;
            newInfo.OWNERID = accidentInfo.OWNERID;
            newInfo.UPDATEDATE = System.DateTime.Now;
            newInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            newInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            newInfo.FLAG = "0";
            RefreshUI(RefreshedTypes.ShowProgressBar);
            try
            {
                vehicleManager.AddAccidentRecordAsync(newInfo);
               // accidentInfo = new T_OA_ACCIDENTRECORD();
                txtOwnerID.Text = accidentInfo.OWNERID;
                accidentInfo = newInfo;

            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                throw (ex);

            }
        }
        private void UpdateInfo(T_OA_ACCIDENTRECORD updateInfo)
        {
            string StrStart = string.Empty;
            StrStart = dpDate.Text.ToString();
            DateTime DtStart = new DateTime();
            if (!Check()) return;
            if (!string.IsNullOrEmpty(StrStart))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                if (DtStart > DateTime.Now)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEGREATERTHANTHECURRENTDATE", "ACCIDENTDATE"));
                    return;
                }
            }

            updateInfo.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleAssetId.SelectedItem;
            updateInfo.ACCIDENTDATE = Convert.ToDateTime(dpDate.Text);
            updateInfo.CONTENT = txtContent.Text;
            updateInfo.OWNERID = txtOwnerID.Text;
            updateInfo.OWNERCOMPANYID = accidentInfo.OWNERCOMPANYID;
            updateInfo.OWNERDEPARTMENTID = accidentInfo.OWNERDEPARTMENTID;
            updateInfo.OWNERPOSTID = accidentInfo.OWNERPOSTID;
            updateInfo.OWNERNAME = accidentInfo.OWNERNAME;
            updateInfo.OWNERID = accidentInfo.OWNERID;
            updateInfo.UPDATEDATE = System.DateTime.Now;
            updateInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            updateInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeName;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            try
            {
                vehicleManager.UpdateAccidentRecordAsync(updateInfo);
            }
            catch (Exception ex)
            { 
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));
            }
        }
        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }

        private T_OA_ACCIDENTRECORD accidentInfo;
        public T_OA_ACCIDENTRECORD AccidentInfo
        {
            get { return accidentInfo; }
            set { accidentInfo = value; }
        }

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
                    accidentInfo = new T_OA_ACCIDENTRECORD();
                    accidentInfo.OWNERCOMPANYID = empInfo.OWNERCOMPANYID;
                    accidentInfo.OWNERDEPARTMENTID = empInfo.OWNERDEPARTMENTID;
                    accidentInfo.OWNERID = empInfo.EMPLOYEEID;
                    accidentInfo.OWNERNAME = empInfo.EMPLOYEECNAME;
                    if (accidentInfo.OWNERNAME == null)
                    {
                        accidentInfo.OWNERNAME = companyInfo.ObjectName;
                    }
                    accidentInfo.OWNERPOSTID = empInfo.T_HR_EMPLOYEEPOST.FirstOrDefault().EMPLOYEEPOSTID;
                    //PersonnelServiceClient psClient = new PersonnelServiceClient();
                    //psClient.GetEmployeeByIDAsync(empInfo.EMPLOYEEID);
                    //psClient.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(psClient_GetEmployeeByIDCompleted);
                    txtOwnerName.Text = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }

        #region 检查合法性
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
            }

            string strDPDate = string.Empty;
            strDPDate = this.dpDate.Text.ToString();
            if (string.IsNullOrEmpty(strDPDate))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ACCIDENTDATE"));
                return false;
            }

            
            return true;
        }
        #endregion
        #region IForm 成员

        public void ClosedWCFClient()
        {
            vehicleManager.DoClose();
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