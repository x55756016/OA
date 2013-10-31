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
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.Permission.UI.Form;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.Helper;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;

namespace SMT.SaaS.Permission.UI.UserControls
{
    public partial class SelectMultiMenu : UserControl, IEntityEditor
    {
        #region 参数定义
        private PermissionServiceClient client = new PermissionServiceClient();
        public List<T_SYS_ENTITYMENU> SelectedMultiMenu ; //选中的菜单集合
        List<T_SYS_ENTITYMENU> allMenu = new List<T_SYS_ENTITYMENU>();
        private T_SYS_ENTITYMENU EntityObj;
        CheckBox SelectBox = new CheckBox();
        private string StrSystemType = "";
        #endregion

        

        public SelectMultiMenu(string SysType,List<T_SYS_ENTITYMENU> ListEntity)
        {
            InitializeComponent();
            EntityObj = new T_SYS_ENTITYMENU();
            StrSystemType = SysType;
            SelectedMultiMenu = new List<T_SYS_ENTITYMENU>();
            if(ListEntity != null)
                SelectedMultiMenu = ListEntity;
            InitEvent();
            this.Loaded += new RoutedEventHandler(SelectMultiMenu_Loaded);
            
        }
        /// <summary>
        /// 由自定义权限调用
        /// </summary>
        /// <param name="CustomerCheck"></param>
        public SelectMultiMenu(List<T_SYS_ENTITYMENU> ListEntity)
        {
            InitializeComponent();
            EntityObj = new T_SYS_ENTITYMENU();
            if (ListEntity != null)
                SelectedMultiMenu = ListEntity;
            SelectedMultiMenu = new List<T_SYS_ENTITYMENU>();
            
            InitEvent();
            this.Loaded += new RoutedEventHandler(SelectMultiMenuCustomer_Loaded);

        }
        /// <summary>
        /// 由自定义权限调用
        /// </summary>
        /// <param name="CustomerCheck"></param>
        public SelectMultiMenu(List<T_SYS_ENTITYMENU> ListEntity,string SystemType)
        {
            InitializeComponent();
            EntityObj = new T_SYS_ENTITYMENU();
            StrSystemType = SystemType;

            if (ListEntity != null)
                SelectedMultiMenu = ListEntity;
            SelectedMultiMenu = new List<T_SYS_ENTITYMENU>();

            InitEvent();
            this.Loaded += new RoutedEventHandler(SelectMultiMenuCustomer_Loaded);

        }

        void SelectMultiMenuCustomer_Loaded(object sender, RoutedEventArgs e)
        {
            InitParasCustomer();
            //expander.Visibility = Visibility.Collapsed;//隐藏查询按钮
            
        }

        void SelectMultiMenu_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas(StrSystemType);
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void InitEvent()
        {            
            client.GetSysMenuByTypeCompleted += new EventHandler<GetSysMenuByTypeCompletedEventArgs>(client_GetSysMenuByTypeCompleted);
            client.GetSysMenuByTypeTOFbAdminCompleted += new EventHandler<GetSysMenuByTypeTOFbAdminCompletedEventArgs>(client_GetSysMenuByTypeTOFbAdminCompleted);
            client.GetSysMenuByTypePagingCompleted += new EventHandler<GetSysMenuByTypePagingCompletedEventArgs>(client_GetSysMenuByTypePagingCompleted);
            //自定义权限调用
            client.GetCustomerPermissionMenusCompleted += new EventHandler<GetCustomerPermissionMenusCompletedEventArgs>(client_GetCustomerPermissionMenusCompleted);
        }

        void client_GetSysMenuByTypeTOFbAdminCompleted(object sender, GetSysMenuByTypeTOFbAdminCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                allMenu = e.Result.ToList();
                DataGridBindingPcv(DaGrMenu, allMenu);

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        void client_GetCustomerPermissionMenusCompleted(object sender, GetCustomerPermissionMenusCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }
                //根据类型过滤  2011-6-9
                var ents = from ent in e.Result
                           where ent.SYSTEMTYPE == StrSystemType
                           select ent;
                if (ents != null)
                    allMenu = ents.ToList();
                //allMenu = e.Result.ToList();
                DataGridBindingPcv(DaGrMenu, allMenu);

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        void client_GetSysMenuByTypePagingCompleted(object sender, GetSysMenuByTypePagingCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);   
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                allMenu = e.Result.ToList();
                DataGridBindingPcv(DaGrMenu, allMenu);

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitParas(string SysType)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            client.GetSysMenuByTypeTOFbAdminAsync(SysType, "", Common.CurrentLoginUserInfo.EmployeeID);
            //client.GetSysMenuByTypeAsync(SysType,"",Common.CurrentLoginUserInfo.EmployeeID);//添加了登录的ID进行判断是否为预算管理员
        }

        /// <summary>
        /// 初始化获取自定义权限
        /// </summary>
        private void InitParasCustomer()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            client.GetCustomerPermissionMenusAsync("");
        }

        /// <summary>
        /// 获取指定系统下的功能项集合，并加载到树控件上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSysMenuByTypeCompleted(object sender, GetSysMenuByTypeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                allMenu = e.Result.ToList();
                DataGridBindingPcv(DaGrMenu, allMenu);

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        private void DataGridBindingPcv(DataGrid dt, List<T_SYS_ENTITYMENU> OAMenu)
        {
            if (OAMenu == null)
                return;
            if (OAMenu.Count == 0)
                return;
            PagedCollectionView pcv = new PagedCollectionView(OAMenu);
            Type a = ((PagedCollectionView)pcv).CurrentItem.GetType();
            pcv.PageSize = 400;
            
            dt.ItemsSource = pcv;
        }

        #region DataGrid事件


        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (chkbox.IsChecked.Value)
            {
                EntityObj = chkbox.DataContext as T_SYS_ENTITYMENU;
                if (EntityObj != null)
                {
                    if (SelectedMultiMenu.Count > 0)
                    {
                        var entity = from q in SelectedMultiMenu
                                     where q.ENTITYMENUID == EntityObj.ENTITYMENUID
                                     select q;
                        if (entity.Count() == 0)
                        {
                            SelectedMultiMenu.Add(EntityObj);
                        }
                    }
                    else
                    {
                        SelectedMultiMenu.Add(EntityObj);
                    }
                }
            }
        }


        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (!chkbox.IsChecked.Value)
            {
                EntityObj = (T_SYS_ENTITYMENU)chkbox.DataContext;
                if (SelectedMultiMenu != null)
                {
                    foreach (var h in SelectedMultiMenu)
                    {
                        if (h.ENTITYMENUID == EntityObj.ENTITYMENUID)
                        {
                            SelectedMultiMenu.Remove(h);
                            break;
                        }
                    }
                }
            }
        }


        private void DaGrMenu_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (SelectedMultiMenu.Count() > 0)
            {
                EntityObj = (T_SYS_ENTITYMENU)e.Row.DataContext;
                var entity = from q in SelectedMultiMenu
                             where q.ENTITYMENUID == EntityObj.ENTITYMENUID
                             select q;
                if (entity.Count() > 0)
                {
                    CheckBox chkbox = DaGrMenu.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
                    chkbox.IsChecked = true;
                }
            }

            T_SYS_ENTITYMENU EntityT = (T_SYS_ENTITYMENU)e.Row.DataContext;
            CheckBox mychkBox = DaGrMenu.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            mychkBox.Tag = EntityT;
        }

        #endregion


        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
            return Utility.GetResourceStr("选择自定义权限菜单");
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
                    Save();
                    break;
                case "1":
                    Cancel();
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
                Key = "0",
                Title = Utility.GetResourceStr("CONFIRM"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CANCEL"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png"
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

        #region 确定、取消
        private void Save()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        private void Cancel()
        {
            SelectedMultiMenu.Clear();
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        private void lkParentMenu_FindClick(object sender, EventArgs e)
        {
            //StrSystemType = "";
            MenuLookupForm lookup = new MenuLookupForm(StrSystemType);

            LookUp lkParentMenu = Utility.FindChildControl<LookUp>(expander, "lkParentMenu");
            lookup.SelectedClick += (obj, ev) =>
            {
                lkParentMenu.DataContext = lookup.SelectedObj;
                lkParentMenu.DisplayMemberPath = "MENUNAME";
            };
            lookup.Show();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string filter = " 1=1 ";
            int pageCount = 0;
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
            //T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
            //string systype = dict == null ? "" : dict.DICTIONARYVALUE.GetValueOrDefault().ToString();
            
            string parentid = "";

            LookUp lkParentMenu = Utility.FindChildControl<LookUp>(expander, "lkParentMenu");
            T_SYS_ENTITYMENU menu = lkParentMenu.DataContext as T_SYS_ENTITYMENU;
            if (!string.IsNullOrEmpty(StrSystemType))
            {

                filter += " && SYSTEMTYPE ==@" + paras.Count().ToString();
                paras.Add(StrSystemType);
            }
            if (menu != null)
            {
                parentid = menu.ENTITYMENUID;
                if (!string.IsNullOrEmpty(parentid))
                {

                    filter += " && T_SYS_ENTITYMENU2!=null && T_SYS_ENTITYMENU2.ENTITYMENUID ==@" + paras.Count().ToString();
                    paras.Add(parentid);
                }
            }
            SMT.Saas.Tools.PermissionWS.LoginUserInfo loginUserInfo = new SMT.Saas.Tools.PermissionWS.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            //loadbar.Start();
            RefreshUI(RefreshedTypes.ShowProgressBar);   
            client.GetSysMenuByTypePagingAsync(0, 500, "ORDERNUMBER", filter, paras, pageCount, loginUserInfo);
            
        }
    }
}
