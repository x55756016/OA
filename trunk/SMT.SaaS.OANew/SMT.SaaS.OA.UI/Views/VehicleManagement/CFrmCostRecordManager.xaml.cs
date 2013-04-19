using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.VehicleManagement
{
    public partial class CFrmCostRecordManager : BaseForm, IClient, IEntityEditor
    {
        private SmtOACommonAdminClient vehicleManager = new SmtOACommonAdminClient();
        public CFrmCostRecordManager()
        {
            InitializeComponent();
            vehicleManager.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(vehicleInfoManager_GetVehicleInfoListCompleted);
            vehicleManager.UpdateCostRecordCompleted += new EventHandler<UpdateCostRecordCompletedEventArgs>(vehicleManager_UpdateCostRecordCompleted);
            vehicleManager.AddCostRecordCompleted += new EventHandler<AddCostRecordCompletedEventArgs>(vehicleManager_AddCostRecordCompleted);
            this.Loaded += new RoutedEventHandler(CFrmCostRecordManager_Loaded);
            
        }

        void CFrmCostRecordManager_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutRoot_Loaded(sender,e);
            if (this.EditState == "add")
            {
                this.IsEnabled = true;
            }
            else if (this.EditState == "update")
            {
                this.IsEnabled = true;
            }
            else
            {
                this.IsEnabled = false;
            }
        }

        void vehicleManager_UpdateCostRecordCompleted(object sender, UpdateCostRecordCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (e.Result == 1)
            {
                Utility.ShowMessageBox("UPDATE", false, true);
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", false, false);
                RefreshUI(saveType);
            }
          
        }

        void vehicleManager_AddCostRecordCompleted(object sender, AddCostRecordCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result == 1)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", ""));
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("ADD", false, true);
                RefreshUI(saveType);
            }
           
        }

        void vehicleInfoManager_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfoList = e.Result;
            if (vehicleInfoList != null)
            {
                if (costInfo == null)
                {
                    SetComboBoxSelect(vehicleInfoList, null);
                }
                else
                {
                    SetComboBoxSelect(vehicleInfoList, costInfo.T_OA_VEHICLE.ASSETID);
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
            if (this.EditState != "view")
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
        private RefreshedTypes saveType;
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    saveType = RefreshedTypes.All;
                    if (editState == "add")
                    { 
                        AddInfo(); 
                    }
                    if (editState == "update")
                    {
                        if (costInfo != null)
                        {
                            UpdateInfo(costInfo);
                        }
                    }
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    if (editState == "add")
                    { 
                        AddInfo(); 
                    }
                    if (editState == "update")
                    {
                        if (costInfo != null)
                        {
                            UpdateInfo(costInfo);
                        }
                    }
                    break;
            }
        }
        // add by zl
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

            string strCostDate = string.Empty;
            strCostDate = this.dpCostDate.Text.ToString();
            if (string.IsNullOrEmpty(strCostDate))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CostDateTime"));
                return false;
            }

            return true;
        }
        // end
        public event UIRefreshedHandler OnUIRefreshed;

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("CostRecord");
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            SetComboxData();
            Utility.BindComboBox(cmbCostType, "FEESTYPE", 0);
            if (costInfo != null)
            {
                SetFormDefailValue(costInfo);
            }
            else
            {
                dpCostDate.Text = System.DateTime.Now.ToShortDateString();
            }
        }
        private void SetComboxData()
        {
            vehicleManager.GetVehicleInfoListAsync();
        }

        private void SetFormDefailValue(T_OA_COSTRECORD defaultInfo)
        {
            txtContent.Text = defaultInfo.CONTENT;
            dpCostDate.Text = defaultInfo.COSTDATE.ToShortDateString();
            Utility.SetComboboxSelectByText(cmbCostType, defaultInfo.CONSTTYPE, -1);
            txtCost.Text = defaultInfo.COST.ToString();
            cmbVehicleAssetId.IsEnabled = false;
        }

        private void AddInfo()
        {
            //if (DataValidation() == false)
            //{
            //    return;
            //}
            if (!Check())
            {
                return;
            }

            T_OA_COSTRECORD newInfo = new T_OA_COSTRECORD();
            newInfo.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleAssetId.SelectedItem;
            newInfo.CONSTTYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbCostType.SelectedItem).DICTIONARYNAME.ToString();
            newInfo.CONTENT = txtContent.Text;
            newInfo.COST = Convert.ToDecimal(txtCost.Text);
            newInfo.COSTDATE = Convert.ToDateTime(dpCostDate.Text);
            newInfo.COSTRECORDID = System.Guid.NewGuid().ToString();
            newInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            newInfo.CREATEDATE = System.DateTime.Now;
            newInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            newInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            newInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            newInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            newInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            newInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            newInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            newInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            newInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            newInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            newInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            newInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            newInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

            newInfo.UPDATEDATE = System.DateTime.Now;
            newInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            vehicleManager.AddCostRecordAsync(newInfo);
        }
        private void UpdateInfo(T_OA_COSTRECORD updateInfo)
        {
            if (!Check())
            {
                return;
            }

            updateInfo.T_OA_VEHICLE = (T_OA_VEHICLE)cmbVehicleAssetId.SelectedItem;
            updateInfo.CONSTTYPE = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbCostType.SelectedItem).DICTIONARYNAME.ToString();
            updateInfo.CONTENT = txtContent.Text;
            updateInfo.COST = Convert.ToDecimal(txtCost.Text);
            updateInfo.COSTDATE = Convert.ToDateTime(dpCostDate.Text);

            //updateInfo.OWNERCOMPANYID = "";
            //updateInfo.OWNERDEPARTMENTID = "";
            //updateInfo.OWNERID = txtOwnerID.Text;
            //updateInfo.OWNERNAME = "";
            //updateInfo.OWNERPOSTID = "";
            updateInfo.UPDATEDATE = System.DateTime.Now;
            updateInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            RefreshUI(RefreshedTypes.ProgressBar);
            vehicleManager.UpdateCostRecordAsync(updateInfo);
        }

        private bool DataValidation()
        {
            if (GlobalFunction.CheckIsDecimal(txtCost.Text.ToString()) == -1)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("IsDouble", "Fees"));
                return false;
            }
            if (GlobalFunction.CheckIsDateTime(dpCostDate.Text.ToString()) == -1)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("ISNotAllowed", "AccidentTime"));
                return false;
            }
            return true;
        }

        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }

        private T_OA_COSTRECORD costInfo;
        public T_OA_COSTRECORD CostInfo
        {
            get { return costInfo; }
            set { costInfo = value; }
        }
        private string editState = null;
        public string EditState
        {
            get { return editState; }
            set { editState = value; }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            GlobalFunction.TextBoxInputDecimal(sender, e);
        }

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