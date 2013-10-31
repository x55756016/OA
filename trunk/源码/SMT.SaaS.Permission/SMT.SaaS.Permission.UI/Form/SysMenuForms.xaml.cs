using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;

using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using System.Windows;
using System.Windows.Browser;
namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SysMenuForms : UserControl, IEntityEditor
    {

        private string saveType = "0";       //保存方式 0:添加 1:关闭
        private RefreshedTypes RefreshType = RefreshedTypes.Close;
        #region IEntityEditor 成员

        public string GetTitle()
        {
            return SMT.SaaS.Globalization.Localization.GetString("SYSTEMMENU");
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
                    RefreshType = RefreshedTypes.All;
                    AddToClose();
                    break;
                case "1":
                    RefreshType = RefreshedTypes.CloseAndReloadData;
                    AddToClose();
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
            //T_SYS_ENTITYMENU tmpMenu = cbxParentMenu.SelectedItem as T_SYS_ENTITYMENU;
            //if (tmpMenu != null && tmpMenu.ENTITYMENUID != "")
            //{
            //    SysMenu.T_SYS_ENTITYMENU2 = tmpMenu;
            //}
            //else
            //{
            //    SysMenu.T_SYS_ENTITYMENU2 = null;
            //}


            T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
            if (dict == null)
            {
                //系统类型不能为空
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SYSTEMTYPE") + Utility.GetResourceStr("NOTALLOWNULL"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            else
            {
                
                SysMenu.SYSTEMTYPE = dict.DICTIONARYVALUE.ToString();
            }

            if (sysMenu.HASSYSTEMMENU != null && (sysMenu.HASSYSTEMMENU.ToLower() == "true" 
                || sysMenu.HASSYSTEMMENU.ToLower() == "1"))
                sysMenu.HASSYSTEMMENU = "1";
            else
                sysMenu.HASSYSTEMMENU = "0";
            if (sysMenu.ISAUTHORITY != null && (sysMenu.ISAUTHORITY.ToLower() == "true"
                || sysMenu.ISAUTHORITY.ToLower() == "1"))
                sysMenu.ISAUTHORITY = "1";
            else
                sysMenu.ISAUTHORITY = "0";
            //if (sysMenu.ISMOBILE != null && (sysMenu.ISMOBILE.ToLower() == "true" || sysMenu.ISMOBILE.ToLower() == "1"))
            //{
            //    sysMenu.ISMOBILE = "1";
            //}
            //else
            //{
            //    sysMenu.ISMOBILE = "0";
            //}
            if (CheckMenu())
            {
                if (FormTypes.New == this.FormType)
                {
                    SysMenu.CREATEDATE = System.DateTime.Now;
                    ///TODO增加修改人
                    //SysMenu.CREATEUSER = Common.CurrentConfig.CurrentUser.SYSUSERID;
                    //SysMenu.CREATEUSER = Common.CurrentLoginUserInfo.sysuserID;
                    
                    ServiceClient.SysMenuAddAsync(this.SysMenu);
                }
                else
                {
                    SysMenu.UPDATEDATE = System.DateTime.Now;

                    SysMenu.CREATEUSER = Common.CurrentLoginUserInfo.SysUserID;

                    ServiceClient.SysMenuUpdateAsync(this.SysMenu);
                }
            }
            saveType = "1";
            //RefreshUI(RefreshedTypes.All);
        }
        private bool CheckMenu()
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
        public void Close()
        {
            saveType = "1";
            RefreshType = RefreshedTypes.CloseAndReloadData;
            RefreshUI(RefreshedTypes.All);
        }

         private T_SYS_ENTITYMENU sysMenu;

        public T_SYS_ENTITYMENU SysMenu
        {
            get { return sysMenu; }
            set
            {
                sysMenu = value;
                this.DataContext = value;
            }
        }
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        protected PermissionServiceClient ServiceClient;

        public SysMenuForms(FormTypes type)
        {
            InitializeComponent();
            ////TODO:加操作用户信息
            FormType = type;
            InitParas("");
        }
        public SysMenuForms(FormTypes type, string menuID)
        {
            InitializeComponent();  
            FormType = type;
            InitParas(menuID);
        }
        private void InitParas(string menuID)
        {

            ServiceClient = new PermissionServiceClient();
            //ServiceClient.SysMenuAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysMenuAddCompleted);
            ServiceClient.SysMenuAddCompleted += new EventHandler<SysMenuAddCompletedEventArgs>(ServiceClient_SysMenuAddCompleted);
            ServiceClient.SysMenuUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysMenuUpdateCompleted);
            
            //ServiceClient.GetSysMenuByTypeCompleted += new EventHandler<GetSysMenuByTypeCompletedEventArgs>(ServiceClient_GetSysMenuByTypeCompleted);
            ServiceClient.GetSysMenuByIDCompleted += new EventHandler<GetSysMenuByIDCompletedEventArgs>(ServiceClient_GetSysMenuByIDCompleted);
            ServiceClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(ServiceClient_GetSysDictionaryByCategoryCompleted);

            if (FormType == FormTypes.New)
            {
                SysMenu = new T_SYS_ENTITYMENU();
                SysMenu.ENTITYMENUID = Guid.NewGuid().ToString();
                //绑定系统类型
                ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
                
                //BindParentMenu();
            }
            if (!string.IsNullOrEmpty(menuID))
            {
                ServiceClient.GetSysMenuByIDAsync(menuID);
            }

        }

        

        void ServiceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            //绑定系统类型
            if (e.Result != null)
            {
                List<T_SYS_DICTIONARY> dicts = e.Result.ToList();
                cbxSystemType.ItemsSource = dicts;
                cbxSystemType.DisplayMemberPath = "DICTIONARYNAME";

                if (SysMenu != null)
                {
                    foreach (var item in cbxSystemType.Items)
                    {
                        T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                        if (dict != null)
                        {
                            if (dict.DICTIONARYVALUE.ToString() == SysMenu.SYSTEMTYPE)
                            {
                                cbxSystemType.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
        }

        void ServiceClient_GetSysMenuByIDCompleted(object sender, GetSysMenuByIDCompletedEventArgs e)
        {
            //获取要修改的菜单
            this.SysMenu = e.Result;
            ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");

            //BindParentMenu();
            RefreshUI(RefreshedTypes.All);
        }

        


        void ServiceClient_SysMenuUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            RefreshUI(RefreshType);
        }

        void ServiceClient_SysMenuAddCompleted(object sender, SysMenuAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Result != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("ADDSUCCESSEDMENU"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            RefreshUI(RefreshType);
        }

        private void cbxSystemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //BindParentMenu();
        }

        private void lkParentMenu_FindClick(object sender, EventArgs e)
        {
            T_SYS_DICTIONARY systype = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
            if (systype == null)
                return;

            MenuLookupForm lookup = new MenuLookupForm(systype.DICTIONARYVALUE.ToString());


            lookup.SelectedClick += (obj, ev) =>
            {
                lkParentMenu.DataContext = lookup.SelectedObj;
            };
            lookup.Show();
        }


        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            HtmlWindow wd = HtmlPage.Window;
            //string strHost = Application.Current.Resources["PlatformWShost"].ToString().Split('/')[0];
            string strUrl = "";
            string MenuCode = this.txtEntityCode.Text.ToString();
            //strUrl = "http://" + strHost + "/" + strUrl;
            if (!string.IsNullOrEmpty(MenuCode))
            {
                MenuCode = MenuCode.Trim();
            }
            strUrl = "http://demo.smt-online.net/New/Services/ckeditor/Default2.aspx?menucode="+MenuCode;
            Uri uri = new Uri(strUrl);
            //wd.Navigate(uri, "_bank");
            HtmlPopupWindowOptions options = new HtmlPopupWindowOptions();
            options.Directories = false;
            options.Location = false;
            options.Menubar = false;
            options.Status = false;
            options.Toolbar = false;
            options.Status = false;
            options.Resizeable = true;
            options.Left = 280;
            options.Top = 100;
            options.Width = 800;
            options.Height = 600;
            // HtmlPage.PopupWindow(uri, AssemblyName, options);
            string strWindow = System.DateTime.Now.ToString("yyMMddHHmsssfff");
            wd.Navigate(uri, strWindow, "directories=no,fullscreen=no,menubar=no,resizable=yes,scrollbars=yes,status=no,titlebar=no,toolbar=no");


        }

    }
}
