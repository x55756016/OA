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
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;
using System.Windows.Browser;
using System.IO;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.UserControls
{
    
    public partial class SysRoleSetMenu2 : UserControl, IEntityEditor
    {
        private Cache cache = Cache.Instance;

        private  PermissionServiceClient RoleClient = new PermissionServiceClient();
        /// <summary>
        /// 权限实体列表
        /// </summary>
        private  List<T_SYS_PERMISSION> tmpPermission = new List<T_SYS_PERMISSION>();
        private String StrTitle = "";
        /// <summary>
        /// 角色实体
        /// </summary>
        private T_SYS_ROLE tmprole = new T_SYS_ROLE();
        /// <summary>
        /// 实体菜单列表
        /// </summary>
        private ObservableCollection<T_SYS_ENTITYMENU> menuInfosList = new ObservableCollection<T_SYS_ENTITYMENU>(); //菜单列表
        
        /// <summary>
        /// 权限视图集合
        /// </summary>
        private ObservableCollection<V_Permission> EntityPermissionInfosList = new ObservableCollection<V_Permission>(); //权限视图集合


        private ObservableCollection<V_UserPermissionRoleID> EntityPermissionInfosListSecond = new ObservableCollection<V_UserPermissionRoleID>(); //权限视图集合
        /// <summary>
        /// 角色实体菜单Temp列表
        /// </summary>
        private static List<V_RoleEntity> tmpEditRoleEntityLIst = new List<V_RoleEntity>();       
        /// <summary>
        /// 角色菜单权限列表
        /// </summary>
        private List<T_SYS_ROLEMENUPERMISSION> tmpEditRoleEntityPermList = new List<T_SYS_ROLEMENUPERMISSION>();
        /// <summary>
        /// 角色实体菜单ID_Temp列表
        /// </summary>
        private ObservableCollection<string> tmpRoleEntityIDsList = new ObservableCollection<string>();//roleentityid 集合

        private bool IsAdd = false; //用来控制是否是第1次添加

        /// <summary>
        /// 需要修改和更新的角色实体权限范围列表
        /// </summary>
        private string tmpAllList = "";

        /// <summary>
        /// HR系统菜单列表
        /// </summary>
        private List<V_MenuSetRole> HrMenu = new List<V_MenuSetRole>();
        /// <summary>
        /// OA系统菜单列表
        /// </summary>
        private List<V_MenuSetRole> OAMenu = new List<V_MenuSetRole>();
        /// <summary>
        /// FB系统菜单列表
        /// </summary>
        private List<V_MenuSetRole> FBMenu = new List<V_MenuSetRole>();
        /// <summary>
        /// PM系统菜单列表
        /// </summary>
        private List<V_MenuSetRole> LMMenu = new List<V_MenuSetRole>();

        /// <summary>
        /// EDM系统菜单列表(进销存)2010-12-28
        /// </summary>
        private List<V_MenuSetRole> EDMMenu = new List<V_MenuSetRole>();
        /// <summary>
        /// EXM考试系统菜单列表(考试系统)2011-5-31
        /// </summary>
        private List<V_MenuSetRole> PMMenu = new List<V_MenuSetRole>();
        

        //private DataGrid DaGrHR = new DataGrid();
       

        #region IEntityEditor
        public string GetTitle()
        {
            return StrTitle +Utility.GetResourceStr("ROLESETPERMISSION");
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
                    SaveAndClose();
                    break;
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "角色权限",
                Tooltip = "角色权限信息"
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

            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "0",
            //    Title = Utility.GetResourceStr("SAVE"),
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_SAVE.png"
            //};

            //items.Add(item);

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

        /// <summary>
        /// 初始化页面事件
        /// </summary>
        private void InitControlEvent()
        {
            RoleClient.BatchAddRoleEntityPermissionInfosCompleted += new EventHandler<BatchAddRoleEntityPermissionInfosCompletedEventArgs>(RoleClient_BatchAddRoleEntityPermissionInfosCompleted);
            
            //RoleClient.GetSysPermissionAllCompleted += new EventHandler<GetSysPermissionAllCompletedEventArgs>(RoleClient_GetSysPermissionAllCompleted);
            RoleClient.GetSysCommonPermissionAllCompleted += new EventHandler<GetSysCommonPermissionAllCompletedEventArgs>(RoleClient_GetSysCommonPermissionAllCompleted);
            RoleClient.GetHRSysMenuByTypeCompleted += new EventHandler<GetHRSysMenuByTypeCompletedEventArgs>(RoleClient_GetHRSysMenuByTypeCompleted);
            RoleClient.GetOASysMenuByTypeCompleted += new EventHandler<GetOASysMenuByTypeCompletedEventArgs>(RoleClient_GetOASysMenuByTypeCompleted);
            RoleClient.GetFBSysMenuByTypeCompleted += new EventHandler<GetFBSysMenuByTypeCompletedEventArgs>(RoleClient_GetFBSysMenuByTypeCompleted);
            RoleClient.GetLMSysMenuByTypeCompleted += new EventHandler<GetLMSysMenuByTypeCompletedEventArgs>(RoleClient_GetLMSysMenuByTypeCompleted);
            RoleClient.GetEDMSysMenuByTypeCompleted += new EventHandler<GetEDMSysMenuByTypeCompletedEventArgs>(RoleClient_GetEDMSysMenuByTypeCompleted);
            RoleClient.GetPMSysMenuByTypeCompleted += new EventHandler<GetPMSysMenuByTypeCompletedEventArgs>(RoleClient_GetPMSysMenuByTypeCompleted);
            RoleClient.GetRoleEntityIDListInfosByRoleIDCompleted += new EventHandler<GetRoleEntityIDListInfosByRoleIDCompletedEventArgs>(RoleClient_GetRoleEntityIDListInfosByRoleIDCompleted);
            RoleClient.GetRoleEntityIDListInfosByRoleIDNewCompleted += new EventHandler<GetRoleEntityIDListInfosByRoleIDNewCompletedEventArgs>(RoleClient_GetRoleEntityIDListInfosByRoleIDNewCompleted);
            //RoleClient.GetPermissionByRoleIDCompleted += new EventHandler<GetPermissionByRoleIDCompletedEventArgs>(RoleClient_GetPermissionByRoleIDCompleted);

            RoleClient.GetPermissionByRoleIDSecondCompleted += new EventHandler<GetPermissionByRoleIDSecondCompletedEventArgs>(RoleClient_GetPermissionByRoleIDSecondCompleted);

            //RoleClient.GetRolePermsCompleted += new EventHandler<GetRolePermsCompletedEventArgs>(RoleClient_GetRolePermsCompleted);

            
        }

        

        
        

        void RoleClient_GetSysCommonPermissionAllCompleted(object sender, GetSysCommonPermissionAllCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error != null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    return;
                }
                if (e.Result != null)
                {
                    tmpPermission = e.Result.ToList();
                                        
                }
            }
            //开始获取HR菜单
            RoleClient.GetHRSysMenuByTypeAsync("0");
        }

        void RoleClient_GetPermissionByRoleIDSecondCompleted(object sender, GetPermissionByRoleIDSecondCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        EntityPermissionInfosListSecond = e.Result;//权限视图集合
                        
                    }
                    InitSetPermissionValue();
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
            }
        }

        private void InitDataGridCloumn()
        {
            //DaGrHR.AutoGenerateColumns = false;
            //DataTemplate dataTemp=new DataTemplate();
            //dataTemp = SysRoleSetMenu2Name.Resources["HRColumn"] as DataTemplate;

            //DataGridTemplateColumn dcTemp = new DataGridTemplateColumn();           
            //dcTemp.HeaderStyle = SysRoleSetMenu2Name.Resources["DataGridImageColumnHeaderStyle"] as Style;
            //dcTemp.CellTemplate = dataTemp;
            //DaGrHR.Columns.Add(dcTemp);
            //DaGrHR.Height = 460;
            //DaGrHR.Width = 680;
            //DaGrHR.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //DaGrHR.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            //DaGrOA.Height = 460;
            //DaGrOA.Width = 680;
            //DaGrOA.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //DaGrOA.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            //DaGrFB.Height = 460;
            //DaGrFB.Width = 680;
            //DaGrFB.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //DaGrFB.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            ////DaGrLM.Height = 460;
            ////DaGrLM.Width = 680;
            //DaGrLM.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            //DaGrLM.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            //DaGrHR.CacheMode=CacheMode.
            //DaGrHR.Loaded += new RoutedEventHandler(DaGrHR_Loaded);
            //DaGrOA.Loaded +=new RoutedEventHandler(DaGrOA_Loaded);
            //DaGrFB.Loaded +=new RoutedEventHandler(DaGrFB_Loaded);
            //DaGrLM.Loaded +=new RoutedEventHandler(DaGrLM_Loaded);

            DaGrHR.Style = Application.Current.Resources["DataGridStyle"] as Style;
            DaGrOA.Style = Application.Current.Resources["DataGridStyle"] as Style;
            DaGrFB.Style = Application.Current.Resources["DataGridStyle"] as Style;
            DaGrLM.Style = Application.Current.Resources["DataGridStyle"] as Style;
            DaGrEDM.Style = Application.Current.Resources["DataGridStyle"] as Style;
            DaGrPM.Style = Application.Current.Resources["DataGridStyle"] as Style;

            DaGrOA.CacheMode =  new BitmapCache();
            DaGrHR.CacheMode = new BitmapCache();
            DaGrFB.CacheMode = new BitmapCache();
            DaGrLM.CacheMode = new BitmapCache();
            DaGrEDM.CacheMode = new BitmapCache();
            DaGrPM.CacheMode = new BitmapCache();
            tabrolemenu.CacheMode = new BitmapCache();
        }

        void DaGrHR_Loaded(object sender, RoutedEventArgs e)
        {
            FillPermissionDataRange(DaGrHR, "myChkBtnHR", "HRrating");            
        }

        public SysRoleSetMenu2(T_SYS_ROLE obj)
        {
            InitializeComponent();            
            tmprole = obj;
            HiddenTabitem();//将各个标签项先禁用  加载完菜单后再释放
            //this.tblRoleName.Text = obj.ROLENAME;
            StrTitle = obj.ROLENAME;
            InitControlEvent();
            //RoleClient.GetPermissionByRoleIDAsync(tmprole.ROLEID);
            //RoleClient.GetPermissionByRoleIDSecondAsync(tmprole.ROLEID);
            //LoadPermissionInfos();
            InitDataGridCloumn();
        }

        /// <summary>
        /// 隐藏tabitem各项目
        /// </summary>
        private void HiddenTabitem()
        {
            taboa.IsEnabled = false;
            tabhr.IsEnabled = false;
            tabfb.IsEnabled = false;
            tabedm.IsEnabled = false;
            tablm.IsEnabled = false;
            tabpm.IsEnabled = false;
        }

        private void SysRoleSetMenu2_Loaded(object sender, RoutedEventArgs e)
        {
            //loadbar.Start();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            //RoleClient.GetSysPermissionAllAsync();
            RoleClient.GetSysCommonPermissionAllAsync();
            this.IsEnabled = false;
        }

        #region 创建Grid权限列
        int n = 6;//递归6次就停止
        private List<T_SYS_PERMISSION> GetTmpPermission()
        {
            if (cache["tmpPermission"] == null)
            {
                if (tmpPermission == null)
                {
                    if (n < 0)
                    {
                        return tmpPermission = null;
                    }
                    n--;
                    tmpPermission = new List<T_SYS_PERMISSION>();
                    GetTmpPermission();
                }
                else
                {
                    //if (cache["tmpPermission"])
                    cache.Add("tmpPermission", tmpPermission);
                }
            }
            else
            {
                tmpPermission = (List<T_SYS_PERMISSION>)cache["tmpPermission"];
            }
            return tmpPermission;
        }
        /// <summary>
        /// 创建权限列
        /// </summary>
        /// <param name="dg"></param>
        private void DataGridColumnsAdd(DataGrid dg, string resources)
        {

            if (cache["tmpPermission"] == null)
            {
                tmpPermission = GetTmpPermission();
            }
            else
            {
                tmpPermission = (List<T_SYS_PERMISSION>)cache["tmpPermission"];
            }
            if (tmpPermission == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "警告", "没法实例化权限列表请与开发人员或管理员联系！");

                return;
            }
            DataGridTemplateColumn templateColumn;
            foreach (T_SYS_PERMISSION AA in tmpPermission)
            {
                templateColumn = new DataGridTemplateColumn();
                templateColumn.Width = new DataGridLength(70);
                templateColumn.Header = AA.PERMISSIONNAME;
                if (!string.IsNullOrEmpty(resources))
                {
                    templateColumn.CellTemplate = (DataTemplate)Resources[resources];
                }
                if (!dg.Columns.Contains(templateColumn))
                {
                    dg.Columns.Add(templateColumn);
                }
                //Grid aa = new Grid();
                
            }
        }

        /// <summary>
        /// 创建权限列
        /// </summary>
        /// <param name="dg"></param>
        private void DataGridColumnsAddGrid(DataGrid dg)
        {
            if (cache["tmpPermission"] == null)
            {
                tmpPermission = GetTmpPermission();
            }
            else
            {
                tmpPermission = (List<T_SYS_PERMISSION>)cache["tmpPermission"];
            }
            if (tmpPermission == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "警告", "没法实例化权限列表请与开发人员或管理员联系！");

                return;
            }
            DataGridTemplateColumn templateColumn;
            
            foreach (T_SYS_PERMISSION AA in tmpPermission)
            {
                
                templateColumn = new DataGridTemplateColumn();
                templateColumn.Width = new DataGridLength(70);
                templateColumn.Header = AA.PERMISSIONNAME;
                
                if (!dg.Columns.Contains(templateColumn))
                {
                    dg.Columns.Add(templateColumn);
                }
                
                
                
            }
        }
        #endregion

   
        #region  选择 tablecontrol 事件
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl Tabs = sender as TabControl;
            switch (Tabs.SelectedIndex)
            {

                case 0:
                    break;
                case 1: //OA

                    break;
                case 2://HR                    
                    //HrPanel.Children.Clear();
                    //HrPanel.Children.Add(DaGrHR);
                    break;
                case 3:// LM
                   
                    break;
                case 4://FB
                   
                    break;
            }
        }
        #endregion

        

        /// <summary>
        /// 获取角色菜单权限范围完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InitSetPermissionValue()
        {
            //到这里有点卡
            //开始动态生成grid权限Header     
            DataGridColumnsAdd(DaGrOA, "myOACellTemplate");
            DataGridColumnsAdd(DaGrOAHead, "");
            DataGridColumnsAdd(DaGrHR, "HRCellTemplate");
            DataGridColumnsAdd(DaGrHRHead, "");
            DataGridColumnsAdd(DaGrFB, "myFBCellTemplate");
            DataGridColumnsAdd(DaGrFBHead, "");
            DataGridColumnsAdd(DaGrLM, "myLMCellTemplate");
            DataGridColumnsAdd(DaGrLMHead, "");
            DataGridColumnsAdd(DaGrEDM, "myEDMCellTemplate");
            DataGridColumnsAdd(DaGrEDMHead, "");
            DataGridColumnsAdd(DaGrPM, "myPMCellTemplate");
            DataGridColumnsAdd(DaGrPMHead, "");
            //DataGridColumnsAdd(DaGrOA, "myOACellTemplate");
            //DataGridColumnsAdd(DaGrHR, "HRCellTemplate");
            //DataGridColumnsAdd(DaGrFB, "myFBCellTemplate");
            //DataGridColumnsAdd(DaGrLM, "myLMCellTemplate");
            //DataGridColumnsAdd(DaGrLMHead, "myLMCellTemplate");

            //DataGridColumnsAddGrid(DaGrLM, gridlm, "myLMCellTemplate");

            if (HrMenu != null) this.DaGrHR.ItemsSource = HrMenu;
            if (OAMenu != null) this.DaGrOA.ItemsSource = OAMenu;
            if (FBMenu != null) this.DaGrFB.ItemsSource = FBMenu;
            if (EDMMenu != null) this.DaGrEDM.ItemsSource = EDMMenu;
            if (PMMenu != null) this.DaGrPM.ItemsSource = PMMenu;
            if (LMMenu != null)
            {
                this.DaGrLM.ItemsSource = LMMenu;
                //DaGrLM.HeadersVisibility = DataGridHeadersVisibility.None;

            }


            //if (e.Error != null)
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
            //    return;
            //}
            //if (e.Result != null)
            //{
            //    tmpEditRoleEntityPermList = e.Result.ToList();
            //}
            DaGrHR.Loaded += new RoutedEventHandler(DaGrHR_Loaded);
            DaGrOA.Loaded += new RoutedEventHandler(DaGrOA_Loaded);
            DaGrFB.Loaded += new RoutedEventHandler(DaGrFB_Loaded);
            DaGrLM.Loaded += new RoutedEventHandler(DaGrLM_Loaded);
            DaGrEDM.Loaded += new RoutedEventHandler(DaGrEDM_Loaded);
            DaGrPM.Loaded +=new RoutedEventHandler(DaGrPM_Loaded);
            this.IsEnabled = true;
            
            RefreshUI(RefreshedTypes.HideProgressBar);
        }       


        #region 保存
        private void Save()
        {
            try
            {
                SaveGridMenuPermission(DaGrOA, "OArating");

                SaveGridMenuPermission(DaGrHR, "HRrating");

                SaveGridMenuPermission(DaGrLM, "LMrating");

                SaveGridMenuPermission(DaGrFB, "FBrating");

                SaveGridMenuPermission(DaGrEDM, "EDMrating");

                if (PMMenu.Count() > 0)
                {
                    SaveGridMenuPermission(DaGrPM, "PMrating");
                }

                if (!string.IsNullOrEmpty(tmpAllList))
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    tmpAllList = tmpAllList.Substring(0, tmpAllList.Length - 1);
                }
                

                //return;
                RoleClient.BatchAddRoleEntityPermissionInfosAsync(tmpAllList, Common.CurrentLoginUserInfo.SysUserID, tmprole.ROLEID);

            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), ex.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        private void SaveAndClose()
        {
            Save();
            
        }

        #endregion

        

        #region 单击 功能项里的 星星按钮        
        
        //private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    //TabControl Tabs = sender as TabControl;
        //    switch (tabrolemenu.SelectedIndex)
        //    {

        //        case 0:
        //            break;
        //        case 1: //OA

        //            break;
        //        case 2://HR                    
        //            //HrPanel.Children.Clear();
        //            //HrPanel.Children.Add(DaGrHR);
        //            break;
        //        case 3:// LM

        //            break;
        //        case 4://FB

        //            break;
        //    }
        //}
        #endregion



    }
}
