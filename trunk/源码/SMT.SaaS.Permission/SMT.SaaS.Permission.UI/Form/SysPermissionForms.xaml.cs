using System;
using System.Collections.Generic;
using System.Windows.Controls;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI.ChildWidow;



namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SysPermissionForms : UserControl, IEntityEditor
    {
        T_SYS_PERMISSION perm = new T_SYS_PERMISSION();
        PermissionServiceClient client;
        private string saveType = "0";       //保存方式 0:添加 1:关闭
        private RefreshedTypes refreshtype;
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        public SysPermissionForms(FormTypes formType)
        {
            InitializeComponent();
            InitParas("");
            FormType = formType;
            this.Loaded += new RoutedEventHandler(SysPermissionForms_Loaded);
            
   
            
        }

        void SysPermissionForms_Loaded(object sender, RoutedEventArgs e)
        {
            bool IsLoadDicts = false;
            if (Application.Current.Resources["SYS_DICTIONARY"] == null)
            {
                IsLoadDicts = true;
                LoadDicts();
            }
            this.LayoutRoot.RowDefinitions[3].Height = new GridLength(0);
            this.LayoutRoot.RowDefinitions[4].Height = new GridLength(0);
            
            perm.PERMISSIONID = Guid.NewGuid().ToString();
            perm.CREATEDATE = DateTime.Now;
            //perm.CREATEUSER = Common.CurrentConfig.;
            //perm.CREATEUSER = Common.CurrentConfig.CurrentUser.SYSUSERID;
            //perm.CREATEUSER = Common.CurrentLoginUserInfo.sysuserID;
            this.DataContext = perm;
            if (formType == FormTypes.New)
            {
                rbtIsAutoyes.IsChecked = true;
            }
            if (IsLoadDicts == false)
            {
                client.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
            }
        }
        protected void LoadDicts()
        {
            client.GetSysDictionaryByCategoryCompleted += (o, e) =>
            {
                List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
                dicts = e.Result == null ? null : e.Result.ToList();
                var ents = from ent in Application.Current.Resources
                           where ent.Key == "SYS_DICTIONARY"
                           select ent;
                if (ents != null)
                {
                    Application.Current.Resources.Remove("SYS_DICTIONARY");
                    Application.Current.Resources.Add("SYS_DICTIONARY",dicts);
                    
                }
                else
                {

                    Application.Current.Resources.Add("SYS_DICTIONARY", dicts);
                }

                
            };
            //TODO: 按需取出字典值
            client.GetSysDictionaryByCategoryAsync("");
        }
        public SysPermissionForms(FormTypes formType,string strID)
        {
            InitializeComponent();
            FormType = formType;

            InitParas(strID);
            if (formType == FormTypes.Browse)
            {
                UnEnableFormControl();
            }
            
        }

        /// <summary>
        /// 禁用表单控件
        /// </summary>
        private void UnEnableFormControl()
        {
            txtPerName.IsEnabled = false;
            txtPerValue.IsEnabled = false;
            //txtRemark.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            rbtIsAutono.IsEnabled = false;
            rbtIsAutoyes.IsEnabled = false;
        }

        private void InitParas(string strID)
        {
            client = new PermissionServiceClient();
            
            client.SysPermissionUpdateCompleted += new EventHandler<SysPermissionUpdateCompletedEventArgs>(client_SysPermissionUpdateCompleted);
            client.GetSysPermissionByIDCompleted += new EventHandler<GetSysPermissionByIDCompletedEventArgs>(client_GetSysPermissionByIDCompleted);
            client.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(ServiceClient_GetSysDictionaryByCategoryCompleted);
            client.SysPermissionAddCompleted += new EventHandler<SysPermissionAddCompletedEventArgs>(client_SysPermissionAddCompleted);
            client.GetSysMenuByIDCompleted += new EventHandler<GetSysMenuByIDCompletedEventArgs>(ServiceClient_GetSysMenuByIDCompleted);
            if (!string.IsNullOrEmpty(strID))
            {
                client.GetSysPermissionByIDAsync(strID);
            }
            
        }
        void ServiceClient_GetSysMenuByIDCompleted(object sender, GetSysMenuByIDCompletedEventArgs e)
        {

            LookUp lkParentMenu = Utility.FindChildControl<LookUp>(expander, "lkParentMenu");

            lkParentMenu.DataContext = e.Result;
            lkParentMenu.DisplayMemberPath = "MENUNAME";
        }
        void ServiceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            //绑定系统类型
            if (e.Result != null)
            {
                ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
                List<T_SYS_DICTIONARY> dicts = e.Result.ToList();

                cbxSystemType.ItemsSource = dicts;
                cbxSystemType.DisplayMemberPath = "DICTIONARYNAME";
            }
        }

        void client_GetSysPermissionByIDCompleted(object sender, GetSysPermissionByIDCompletedEventArgs e)
        {
            perm = e.Result;
            perm.UPDATEDATE = DateTime.Now;
            //perm.UPDATEUSER = Common.CurrentLoginUserInfo.sysuserID;

            this.DataContext = perm;
            
            if (perm.ISCOMMOM == "1" || perm.ISCOMMOM==null)
            {
                rbtIsAutoyes.IsChecked = true;
                rbtIsAutono.IsChecked = false;
                this.LayoutRoot.RowDefinitions[3].Height = new GridLength(0);
                this.LayoutRoot.RowDefinitions[4].Height = new GridLength(0);
            }
            else
            {
                rbtIsAutoyes.IsChecked = false;
                rbtIsAutono.IsChecked = true;
                this.LayoutRoot.RowDefinitions[3].Height = new GridLength(30);
                this.LayoutRoot.RowDefinitions[4].Height = new GridLength(30);
                if(perm.ISCOMMOM =="0" && perm.T_SYS_ENTITYMENUReference.EntityKey !=null)
                    client.GetSysMenuByIDAsync(perm.T_SYS_ENTITYMENUReference.EntityKey.EntityKeyValues[0].Value.ToString());
                
                
            }
            client.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        }

        #region "确定，更新，取消 
        void client_SysPermissionAddCompleted(object sender, SysPermissionAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                if (e.Result == "")
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("ADDSUCCESSED", ""), Utility.GetResourceStr("CONFIRMBUTTON"));
                }
                else
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRMBUTTON"));
                }
                saveType = "1";
                RefreshUI(refreshtype);
                
            }
        }

        void client_SysPermissionUpdateCompleted(object sender, SysPermissionUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                if (e.Result == "")
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRMBUTTON"));
                }
                else
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRMBUTTON"));
                }
                saveType = "1";
                RefreshUI(refreshtype);
            }
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return SMT.SaaS.Globalization.Localization.GetString("SYSPERMISSION");
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    refreshtype = RefreshedTypes.All;
                    AddToClose();
                    break;
                case "1":
                    refreshtype = RefreshedTypes.CloseAndReloadData;
                    AddToClose();
                    // Close();
                    break;
            }

        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
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

        public void AddToClose()
        {
            if (CheckRole())
            {
                if (rbtIsAutono.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(perm.PERMISSIONCODE))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), "权限编码不能为空");
                        return;
                    }
                    else
                    {
                        perm.ISCOMMOM = "0";
                    }
                    LookUp lkParentMenu = Utility.FindChildControl<LookUp>(expander, "lkParentMenu");
                    T_SYS_ENTITYMENU menu = lkParentMenu.DataContext as T_SYS_ENTITYMENU;
                    if (menu != null)
                    {
                        perm.T_SYS_ENTITYMENU = menu;
                        //perm.T_SYS_ENTITYMENUReference.EntityKey = menu.EntityKey;
                        perm.T_SYS_ENTITYMENU.ENTITYMENUID = menu.ENTITYMENUID;
                    }
                }
                else
                {
                    perm.ISCOMMOM = "1";
                    perm.PERMISSIONCODE = "";//是公共权限不设置编码
                }
                if (!string.IsNullOrEmpty(perm.PERMISSIONVALUE))
                {
                    if (perm.PERMISSIONVALUE.IndexOf("-") > -1)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), "权限值不能为负数");
                        return;
                    }
                    try
                    {
                        int IntValue = System.Int32.Parse(perm.PERMISSIONVALUE);
                    }
                    catch (Exception ex)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), "权限值必须为整数");
                        return;
                    }
                }
                if (FormTypes.New == this.FormType)
                {
                    client.SysPermissionAddAsync(perm);
                }
                else
                {
                    client.SysPermissionUpdateAsync(perm);
                }
            }
        }
        private bool CheckRole()
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
        private void rbtIsAutoyes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.LayoutRoot.RowDefinitions[3].Height = new GridLength(0);
            this.LayoutRoot.RowDefinitions[4].Height = new GridLength(0);
            if (rbtIsAutoyes.IsChecked == false)
            {
                rbtIsAutoyes.IsChecked = true;
                rbtIsAutono.IsChecked = false;
            }
        }

        private void tbtIsAutono_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.LayoutRoot.RowDefinitions[3].Height = new GridLength(30);
            this.LayoutRoot.RowDefinitions[4].Height = new GridLength(30);
            if (rbtIsAutono.IsChecked == false)
            {
                rbtIsAutono.IsChecked = true;
                rbtIsAutoyes.IsChecked = false;
            }
        }

        private void lkParentMenu_FindClick(object sender, EventArgs e)
        {
            ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
            T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;

            string systype = dict == null ? "" : dict.DICTIONARYVALUE.GetValueOrDefault().ToString();
            MenuLookupForm lookup = new MenuLookupForm(systype);

            LookUp lkParentMenu = Utility.FindChildControl<LookUp>(expander, "lkParentMenu");
            lookup.SelectedClick += (obj, ev) =>
            {
                lkParentMenu.DataContext = lookup.SelectedObj;
                lkParentMenu.DisplayMemberPath = "MENUNAME";
            };
            lookup.Show();
        }
    }
}
