using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PermissionWS;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class VehicleInfo_add : BaseForm,IClient, IEntityEditor
    {
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        private ObservableCollection<T_OA_VEHICLECARD> _oVCard = new ObservableCollection<T_OA_VEHICLECARD>();
        /// <summary>
        /// 新增数据
        /// </summary>
        ObservableCollection<T_OA_VEHICLECARD> oCard_add = new ObservableCollection<T_OA_VEHICLECARD>();
        /// <summary>
        /// 已修改数据
        /// </summary>
        ObservableCollection<T_OA_VEHICLECARD> oCard_upd = new ObservableCollection<T_OA_VEHICLECARD>();

        T_OA_VEHICLE newInfo = null;
        public VehicleInfo_add()
        {
            InitializeComponent();
            _VM.UpdateVehicleCompleted += new EventHandler<UpdateVehicleCompletedEventArgs>(UpdateVehicleCompleted);
            _VM.AddVehicleCompleted += new EventHandler<AddVehicleCompletedEventArgs>(AddVehicleCompleted);

            _VM.Del_VICardCompleted += new EventHandler<Del_VICardCompletedEventArgs>(Del_VICardCompleted);
            _VM.Save_VICardCompleted += new EventHandler<Save_VICardCompletedEventArgs>(Save_VICardCompleted);
            this.cmbUserFlag.SelectedIndex = -1;
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
            if (!Check()) return;
            RefreshUI(RefreshedTypes.ProgressBar);
            switch (actionType)
            {
                case "0": saveType = RefreshedTypes.All;
                        AddInfo();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                        AddInfo();
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
            return Utility.GetResourceStr("VehicleInfo");
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _oVCard = new ObservableCollection<T_OA_VEHICLECARD>();

            T_OA_VEHICLECARD card = new T_OA_VEHICLECARD();

            card.INVALIDDATE = DateTime.Today.AddYears(1);
            card.EFFECTDATE = DateTime.Today;
            card.CARDTYPE = "公司";
            _oVCard.Add(card);
            dg.ItemsSource = _oVCard;
            dg.SelectedIndex = 0;
        }
        //private void SetFormDefailValue(T_OA_VEHICLE defaultInfo)
        //{
        //    txtCarModel.Text = defaultInfo.VEHICLEMODEL;
        //    txtCompanyID.Text = defaultInfo.COMPANYID;
        //    txtVehicleAssetId.Text = defaultInfo.ASSETID;
        //    txtVehicleAssetId.IsReadOnly = true;
        //    txtVIN.Text = defaultInfo.VIN;
        //    dpBuyDate.Text = defaultInfo.BUYDATE.ToShortDateString();
        //    txtPrice.Text = defaultInfo.BUYPRICE.ToString();
        //    txtINITIALRANGE.Text = defaultInfo.INITIALRANGE.ToString();
        //    txtINTERVALRANGE.Text = defaultInfo.INTERVALRANGE.ToString();
        //    txtMAINTAINCOMPANY.Text = defaultInfo.MAINTAINCOMPANY;
        //    txtMAINTAINTEL.Text = defaultInfo.MAINTAINTEL;
        //    txtMAINTENANCECYCLE.Text = defaultInfo.MAINTENANCECYCLE.ToString();
        //    txtMAINTENANCEREMIND.Text = defaultInfo.MAINTENANCEREMIND.ToString();
        //    txtSEATQUANTITY.Text = defaultInfo.SEATQUANTITY.ToString();
        //    txtVEHICLEBRANDS.Text = defaultInfo.VEHICLEBRANDS;
        //    txtVEHICLETYPE.Text = defaultInfo.VEHICLETYPE;
        //    txtWEIGHT.Text = defaultInfo.WEIGHT.ToString();
        //    if (defaultInfo.VEHICLEFLAG.Trim() == null)
        //    {
        //        cmbUserFlag.SelectedIndex = -1;
        //    }
        //    else
        //    {
        //        if (defaultInfo.VEHICLEFLAG.Trim() == "1")
        //        {
        //            cmbUserFlag.SelectedIndex = 1;
        //        }
        //        else
        //        {
        //            cmbUserFlag.SelectedIndex = 0;
        //        }
        //    }
        //}

        //验证
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

            if (this.cmbUserFlag.SelectedIndex <= 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "UseFlag"));
                this.cmbUserFlag.Focus();
                return false;
            }

            if (dpBuyDate.Text == null || dpBuyDate.Text == "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("BuyDate"));
                return false;
            }
            return true;
        }

        private void AddInfo()
        {
            if (Check() == false)
                return;
            T_SYS_DICTIONARY StrDepCity = cmbUserFlag.SelectedItem as T_SYS_DICTIONARY;
            oCard_upd.Clear();
            oCard_add.Clear();

            if (newInfo == null)
                newInfo = new T_OA_VEHICLE();

            newInfo.ASSETID = txtVehicleAssetId.Text;
            newInfo.BUYDATE = Convert.ToDateTime(dpBuyDate.Text);
            newInfo.BUYPRICE = Convert.ToDecimal(txtPrice.Text);
            newInfo.INITIALRANGE = Convert.ToDecimal(txtINITIALRANGE.Text);
            newInfo.INTERVALRANGE = Convert.ToDecimal(txtINTERVALRANGE.Text);
            newInfo.MAINTAINCOMPANY = txtMAINTAINCOMPANY.Text;
            newInfo.MAINTAINTEL = txtMAINTAINTEL.Text;
            newInfo.MAINTENANCECYCLE = Convert.ToDecimal(txtMAINTENANCECYCLE.Text);
            newInfo.MAINTENANCEREMIND = Convert.ToDecimal(txtMAINTENANCEREMIND.Text);
            newInfo.SEATQUANTITY = Convert.ToDecimal(txtSEATQUANTITY.Text);
            newInfo.VEHICLEBRANDS = txtVEHICLEBRANDS.Text;
            newInfo.VEHICLETYPE = txtVEHICLETYPE.Text;
            newInfo.WEIGHT = Convert.ToDecimal(txtWEIGHT.Text);
            newInfo.COMPANYID = txtCompanyID.Text;
            newInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            newInfo.CREATEDATE = System.DateTime.Now;
            newInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            newInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            newInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            newInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            newInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            newInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            newInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            newInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            newInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

            if (cmbUserFlag.SelectedIndex != 0)
            {
                newInfo.VEHICLEFLAG = StrDepCity.DICTIONARYVALUE.ToString();
            }
            newInfo.VEHICLEMODEL = txtCarModel.Text;
            newInfo.VIN = txtVIN.Text;

            //停车卡
            foreach (T_OA_VEHICLECARD card in dg.ItemsSource)
            {
                card.T_OA_VEHICLE = newInfo;
                if (card.VEHICLECARDID == null && (card.CARDNAME!=null||card.CHARGEMONEY!=null||card.CONTENT!=null))
                {
                    card.VEHICLECARDID = Guid.NewGuid().ToString();

                    card.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    card.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    card.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    card.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    card.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    card.CREATEDATE = DateTime.Now;

                    card.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    card.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    card.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    card.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    card.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    if (card.CHARGEMONEY != null && card.CARDNAME != null)
                        oCard_add.Add(card);
                }
                else
                {
                    card.UPDATEDATE = DateTime.Now;
                    card.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    card.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    if (card.CHARGEMONEY != null && card.CARDNAME != null)
                        oCard_upd.Add(card);
                }
            }

            if (newInfo.VEHICLEID == null)
            {
                newInfo.VEHICLEID = Guid.NewGuid().ToString();
                _VM.AddVehicleAsync(newInfo);
            }
            else
            {
                newInfo.UPDATEDATE = System.DateTime.Now;
                newInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                newInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                _VM.UpdateVehicleAsync(newInfo);
            }
        }
        void AddVehicleCompleted(object sender, AddVehicleCompletedEventArgs e)
        {
            
            if (e.Result < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ADDFAILED"));
                RefreshUI(RefreshedTypes.ProgressBar);
                RefreshUI(saveType);
            }
            else
            {
                if (oCard_add.Count == 0 && oCard_upd.Count == 0)
                {
                    Utility.ShowMessageBox("ADD", false, true);
                    RefreshUI(RefreshedTypes.ProgressBar);
                    
                }
                else
                    _VM.Save_VICardAsync(oCard_add, oCard_upd);
                // by ldx
                // 2011-08-09
                // 保存并关闭不能关闭
                RefreshUI(saveType);
            }
        }
        void UpdateVehicleCompleted(object sender, UpdateVehicleCompletedEventArgs e)
        {
            
            if (e.Result < 0)
            {
                Utility.ShowMessageBox("UPDATE", false, false);
                RefreshUI(RefreshedTypes.ProgressBar);
                RefreshUI(saveType);
            }
            else
            {
                if (oCard_add.Count == 0 && oCard_upd.Count == 0)
                {
                    Utility.ShowMessageBox("UPDATE", false, true);
                    RefreshUI(RefreshedTypes.ProgressBar);
                    RefreshUI(saveType);
                }
                else
                    _VM.Save_VICardAsync(oCard_add, oCard_upd);
            }
        }
        private void Save_VICardCompleted(object sender, Save_VICardCompletedEventArgs e)
        {
            if (e.Result > 0)
                Utility.ShowMessageBox("ADD", false, true);
            else
                Utility.ShowMessageBox("ADD", false, false);

            RefreshUI(RefreshedTypes.ProgressBar);
        }
        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }

        private T_OA_VEHICLE vehicleInfo;
        public T_OA_VEHICLE VehicleInfo
        {
            get { return vehicleInfo; }
            set { vehicleInfo = value; }
        }
        /// <summary>
        /// 公司id
        /// </summary>
        private string _companyID = "";

        private void btnLookUpCompany_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    txtCompanyID.Text = companyInfo.ObjectName;
                    _companyID = companyInfo.ObjectID;
                }
            };
            lookup.Show();
        }
      
        #region 停车卡
     
        //删除子表
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_oVCard.Count > 1) //必须有派车单，司机才能根据派车单提交费用 单
            {
                T_OA_VEHICLECARD i = ((Button)sender).DataContext as T_OA_VEHICLECARD;
                _oVCard.Remove(i);

                if (i.VEHICLECARDID != null) //删除已经保存到服务器中的数据
                {
                    ObservableCollection<string> o = new ObservableCollection<string>();
                    o.Add(i.VEHICLECARDID);
                    _VM.Del_VICardAsync(o);
                    RefreshUI(RefreshedTypes.ProgressBar);
                }
            }
        }
        void Del_VICardCompleted(object sender, Del_VICardCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                dg.SelectedIndex = 0;
                Utility.ShowMessageBox("DELETE", false, true);
            }
            else
                Utility.ShowMessageBox("DELETE", false, false);
            RefreshUI(RefreshedTypes.ProgressBar);
        }
        //行加载删除按钮
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_VEHICLECARD tmp = (T_OA_VEHICLECARD)e.Row.DataContext;
            ImageButton MyButton_Delbaodao = dg.Columns[7].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Delbaodao.Tag = tmp;

            ComboBox cmb = dg.Columns[1].GetCellContent(e.Row).FindName("cmbType") as ComboBox;
            switch (tmp.CARDTYPE)
            {
                case "家里": cmb.SelectedIndex = 1; break;
                default: cmb.SelectedIndex = 0; break;
            }
        }
        //根据 回车键，判断 是否新增行，保存修改行
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (dg.SelectedIndex == _oVCard.Count - 1)
                {
                    T_OA_VEHICLECARD card = new T_OA_VEHICLECARD();
                    card.INVALIDDATE = DateTime.Today.AddYears(1);
                    card.EFFECTDATE = DateTime.Today;
                    card.CARDTYPE = "公司";
                    _oVCard.Add(card);
                }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Name == "txtCardName")
                _oVCard[dg.SelectedIndex].CARDNAME = txt.Text.ToString();
            else if (txt.Name == "txtChargeMoney")
            {
                decimal d = 0;
                try { d = decimal.Parse(txt.Text.ToString()); }
                catch { txt.Text = "0"; }
                _oVCard[dg.SelectedIndex].CHARGEMONEY = d;
            }
            else if (txt.Name == "txtRemark")
                _oVCard[dg.SelectedIndex].CONTENT = txt.Text.ToString();

        }
        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _oVCard[dg.SelectedIndex].CARDTYPE = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString();
        }
        //生效日期
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _oVCard[dg.SelectedIndex].EFFECTDATE = (DateTime)((DatePicker)sender).SelectedDate;
        }
        //失效日期 
        private void DatePicker_SelectedDateChanged1(object sender, SelectionChangedEventArgs e)
        {
            _oVCard[dg.SelectedIndex].INVALIDDATE = (DateTime)((DatePicker)sender).SelectedDate;
        }
       
        #endregion 停车卡 

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