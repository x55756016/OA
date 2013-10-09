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
    public partial class VehicleInfo_upd : BaseForm,IClient, IEntityEditor
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
        private FormTypes tmpformtype;
        public VehicleInfo_upd(FormTypes formtype)
        {
            InitializeComponent();
            _VM.UpdateVehicleCompleted += new EventHandler<UpdateVehicleCompletedEventArgs>(UpdateVehicleCompleted);
            _VM.Get_VICardCompleted += new EventHandler<Get_VICardCompletedEventArgs>(Get_VICardCompleted);
            _VM.Del_VICardCompleted += new EventHandler<Del_VICardCompletedEventArgs>(Del_VICardCompleted);
            _VM.Save_VICardCompleted += new EventHandler<Save_VICardCompletedEventArgs>(Save_VICardCompleted);
            tmpformtype = formtype;

            this.Loaded += new RoutedEventHandler(VehicleInfo_upd_Loaded);
        }

        void VehicleInfo_upd_Loaded(object sender, RoutedEventArgs e)
        {
            if (tmpformtype == FormTypes.Browse)
            {
                //this.first11.Children.IsReadOnly = true;
                //this.tabcontrol.IsEnabled = true;
                //this.LayoutRoot.Children.IsReadOnly = true;
                this.txtVIN.IsReadOnly = true;
                this.txtCarModel.IsReadOnly = true;
                this.txtPrice.IsReadOnly = true;
                this.txtINITIALRANGE.IsReadOnly = true;
                this.txtINTERVALRANGE.IsReadOnly = true;
                this.txtMAINTAINCOMPANY.IsReadOnly = true;
                this.txtMAINTAINTEL.IsReadOnly = true;
                this.txtMAINTENANCECYCLE.IsReadOnly = true;
                this.txtMAINTENANCEREMIND.IsReadOnly = true;
                this.txtSEATQUANTITY.IsReadOnly = true;
                this.txtVEHICLEBRANDS.IsReadOnly = true;
                this.txtVEHICLETYPE.IsReadOnly = true;
                this.txtWEIGHT.IsReadOnly = true;
                this.cmbUserFlag.IsEnabled = false;
                this.dpBuyDate.IsEnabled = false;
                this.dg.IsEnabled = false;
                Utility.FindChildControlToIsEnable<TextBox>(first);
            }
            if (vehicleInfo != null)
            {
                SetFormDefailValue(vehicleInfo);
                _VM.Get_VICardAsync(vehicleInfo.ASSETID);
            }
        }
        //加载
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        //获取加载停车卡信息
        void Get_VICardCompleted(object sender, Get_VICardCompletedEventArgs e)
        {
            _oVCard = e.Result;
            if (_oVCard == null)
                _oVCard = new ObservableCollection<T_OA_VEHICLECARD>();

            T_OA_VEHICLECARD card = new T_OA_VEHICLECARD();

            card.INVALIDDATE = DateTime.Today.AddYears(1);
            card.EFFECTDATE = DateTime.Today;
            card.CARDTYPE = "公司";
            //_oVCard.Add(card);
            dg.ItemsSource = _oVCard;
            dg.SelectedIndex = 0;
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
            if (tmpformtype != FormTypes.Browse)
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
            //RefreshUI(RefreshedTypes.ShowProgressBar);
            switch (actionType)
            {
                case "0": saveType = RefreshedTypes.All;
                    if (vehicleInfo != null)
                        UpdateInfo(vehicleInfo);
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    if (vehicleInfo != null)
                        UpdateInfo(vehicleInfo);
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

        private void SetFormDefailValue(T_OA_VEHICLE defaultInfo)
        {
            txtCarModel.Text = defaultInfo.VEHICLEMODEL;
            txtCompanyID.Text = defaultInfo.COMPANYID;
            txtVehicleAssetId.Text = defaultInfo.ASSETID;
            txtVehicleAssetId.IsReadOnly = true;
            txtVIN.Text = defaultInfo.VIN;
            dpBuyDate.Text = defaultInfo.BUYDATE.ToShortDateString();
            txtPrice.Text = defaultInfo.BUYPRICE.ToString();
            txtINITIALRANGE.Text = defaultInfo.INITIALRANGE.ToString();
            txtINTERVALRANGE.Text = defaultInfo.INTERVALRANGE.ToString();
            txtMAINTAINCOMPANY.Text = defaultInfo.MAINTAINCOMPANY;
            txtMAINTAINTEL.Text = defaultInfo.MAINTAINTEL;
            txtMAINTENANCECYCLE.Text = defaultInfo.MAINTENANCECYCLE.ToString();
            txtMAINTENANCEREMIND.Text = defaultInfo.MAINTENANCEREMIND.ToString();
            txtSEATQUANTITY.Text = defaultInfo.SEATQUANTITY.ToString();
            txtVEHICLEBRANDS.Text = defaultInfo.VEHICLEBRANDS;
            txtVEHICLETYPE.Text = defaultInfo.VEHICLETYPE;
            txtWEIGHT.Text = defaultInfo.WEIGHT.ToString();

            if (!string.IsNullOrEmpty(defaultInfo.VEHICLEFLAG))
            {
                foreach (T_SYS_DICTIONARY Region in cmbUserFlag.Items)
                {
                    if (Region.DICTIONARYVALUE == Convert.ToInt32(defaultInfo.VEHICLEFLAG))
                    {
                        cmbUserFlag.SelectedItem = Region;
                        break;
                    }
                }
            }
            //if (defaultInfo.VEHICLEFLAG.Trim() == null)
            //    cmbUserFlag.SelectedIndex = -1;
            //else
            //{
            //    if (defaultInfo.VEHICLEFLAG.Trim() == "1")
            //        cmbUserFlag.SelectedIndex = 1;
            //    else
            //        cmbUserFlag.SelectedIndex = 0;
            //}
        }

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
            return true;
        }

        private void UpdateInfo(T_OA_VEHICLE updateInfo)
        {
            if (Check())
            {
                T_SYS_DICTIONARY StrDepCity = cmbUserFlag.SelectedItem as T_SYS_DICTIONARY;
                updateInfo.BUYDATE = Convert.ToDateTime(dpBuyDate.Text);
                updateInfo.BUYPRICE = Convert.ToDecimal(txtPrice.Text);
                updateInfo.INITIALRANGE = Convert.ToDecimal(txtINITIALRANGE.Text);
                updateInfo.INTERVALRANGE = Convert.ToDecimal(txtINTERVALRANGE.Text);
                updateInfo.MAINTAINCOMPANY = txtMAINTAINCOMPANY.Text;
                updateInfo.MAINTAINTEL = txtMAINTAINTEL.Text;
                updateInfo.MAINTENANCECYCLE = Convert.ToDecimal(txtMAINTENANCECYCLE.Text);
                updateInfo.MAINTENANCEREMIND = Convert.ToDecimal(txtMAINTENANCEREMIND.Text);
                updateInfo.SEATQUANTITY = Convert.ToDecimal(txtSEATQUANTITY.Text);
                updateInfo.VEHICLEBRANDS = txtVEHICLEBRANDS.Text;
                updateInfo.VEHICLETYPE = txtVEHICLETYPE.Text;
                updateInfo.WEIGHT = Convert.ToDecimal(txtWEIGHT.Text);

                updateInfo.COMPANYID = txtCompanyID.Text;
                updateInfo.UPDATEDATE = System.DateTime.Now;
                updateInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                updateInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                if (cmbUserFlag.SelectedIndex > 0)
                {
                    updateInfo.VEHICLEFLAG = StrDepCity.DICTIONARYVALUE.ToString();
                }

                updateInfo.VEHICLEID = System.Guid.NewGuid().ToString();
                updateInfo.VEHICLEMODEL = txtCarModel.Text;
                updateInfo.VIN = txtVIN.Text;


                //停车卡
                foreach (T_OA_VEHICLECARD card in dg.ItemsSource)
                {
                    card.T_OA_VEHICLE = updateInfo;
                    if (card.VEHICLECARDID == null)
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
                        //card.UPDATEDATE = DateTime.Now;
                        card.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        card.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        if (card.CHARGEMONEY != null && card.CARDNAME != null)
                            oCard_upd.Add(card);
                    }
                }
                //RefreshUI(saveType);
                RefreshUI(RefreshedTypes.ShowProgressBar);
                _VM.UpdateVehicleAsync(updateInfo);
            }
        }
        void UpdateVehicleCompleted(object sender, UpdateVehicleCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                _VM.Save_VICardAsync(oCard_add, oCard_upd);
                //add by zl
                //RefreshUI(RefreshedTypes.HideProgressBar);    
                Utility.ShowMessageBox("UPDATE", false, true);

                // by ldx
                // 2011-08-09
                // 修改保存不能关闭
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", false, false);
            }
        }
        private void Save_VICardCompleted(object sender, Save_VICardCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
                Utility.ShowMessageBox("UPDATE", false, true);
            else
                Utility.ShowMessageBox("UPDATE", false, false);
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
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
        }
        void Del_VICardCompleted(object sender, Del_VICardCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                dg.SelectedIndex = 0;
                Utility.ShowMessageBox("DELETE", false, true);
            }
            else
                Utility.ShowMessageBox("DELETE", false, false);
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
                case "公 司": cmb.SelectedIndex = 1; break;
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