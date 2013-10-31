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
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;

namespace SMT.SaaS.Permission.UI.UserControls
{
    public partial class UserRoleApplyForm : UserControl, IClient, IEntityEditor, IAudit
    {
        #region 变量定义
        
        T_SYS_ROLE roleInfo = new T_SYS_ROLE();//角色实体
        PermissionServiceClient permClient = null;
        PersonnelServiceClient personnelClient = null;
        private List<T_SYS_PERMISSION> tmpPermission = new List<T_SYS_PERMISSION>();
        FormTypes operationType;
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private RefreshedTypes saveType;
        private string tmpRoleID = "";
        private T_SYS_ENTITYMENU menu = null;
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

        private List<T_SYS_ENTITYMENU> ListShowMenus = new List<T_SYS_ENTITYMENU>();

        private bool IsAdd = false; //用来控制是否是第1次添加

        
        private List<T_SYS_DICTIONARY> tmpDicts;
        private string tmpDictionaryValue = ""; //字典值
        /// <summary>
        /// 需要修改和更新的角色实体权限范围列表
        /// </summary>
        private string tmpAllList = "";

        EntityMenuCustomerPermission2 EntityMenuCustomer = null;

        bool IsRoleEntityMenuChange = false;//是否对菜单进行了赋权
        #endregion

        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="StrID"></param>
        public UserRoleApplyForm(FormTypes action,string StrID)
        {
            InitializeComponent();
            permClient = new PermissionServiceClient();
            tmpRoleID = StrID;
            personnelClient = new PersonnelServiceClient();
            operationType = action;
            InitEvent();
            this.Loaded += new RoutedEventHandler(UserRoleApplyForm_Loaded);
        }
        

        void UserRoleApplyForm_Loaded(object sender, RoutedEventArgs e)
        {
            permClient.GetSysCommonPermissionAllAsync();
            
            if (operationType == FormTypes.New)
            {
                InitApplyerInfo();
                roleInfo.ROLEID = Guid.NewGuid().ToString();
                roleInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                roleInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                roleInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                roleInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                this.cbxSystemType.SelectedIndex = 0;
                EntityMenuCustomer = new EntityMenuCustomerPermission2(FormTypes.New,roleInfo.ROLEID);
                EntityMenuCustomer.IsCheckState = true;//被权限控制调用
                EntityBrowser brow = new EntityBrowser(EntityMenuCustomer);
                CustomerPermission.Children.Add(EntityMenuCustomer);
                
            }
            else
            {
                permClient.GetSysRoleSingleInfoByIdAsync(tmpRoleID);
                
                EntityMenuCustomer = new EntityMenuCustomerPermission2(operationType, tmpRoleID);
                EntityBrowser brow = new EntityBrowser(EntityMenuCustomer);
                EntityMenuCustomer.IsCheckState = true;//被权限控制调用
                if (operationType == FormTypes.Audit || operationType == FormTypes.Browse)
                {
                    SetControlsEnabled();
                    EntityMenuCustomer.StateIsRead = true;
                    
                }
                CustomerPermission.Children.Add(EntityMenuCustomer);
            }
        }
        #region 设置控件是否可用
        
        /// <summary>
        /// 当审核或浏览时，控件设置为不可改变状态
        /// </summary>
        private void SetControlsEnabled()
        {
            this.DaGr.IsEnabled = false;
            this.DaGr.Columns[0].IsReadOnly = true;
            this.DaGr.Columns[1].IsReadOnly = true;
            this.cbxSystemType.IsEnabled = false;
            //this.btnLookUpPartyb.Visibility = Visibility.Collapsed;
            this.SearchBtn.IsEnabled = false;
            this.SearchBtn.Visibility = Visibility.Collapsed;
            //this.txtRemark.IsReadOnly = true;
            //this.txtRoleName.IsReadOnly = true;
            this.DelBtn.Visibility = Visibility.Collapsed;
        }
        #endregion

        #endregion

        #region 初始化事件

        private void InitEvent()
        {
            personnelClient.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(personnelClient_GetEmployeeDetailByIDCompleted);
            permClient.GetSysCommonPermissionAllCompleted += new EventHandler<GetSysCommonPermissionAllCompletedEventArgs>(permClient_GetSysCommonPermissionAllCompleted);            
            permClient.GetRoleEntityIDListInfosByRoleIDNewToUserRoleAppCompleted += new EventHandler<GetRoleEntityIDListInfosByRoleIDNewToUserRoleAppCompletedEventArgs>(permClient_GetRoleEntityIDListInfosByRoleIDNewToUserRoleAppCompleted);
            permClient.GetPermissionByRoleIDSecondCompleted += new EventHandler<GetPermissionByRoleIDSecondCompletedEventArgs>(permClient_GetPermissionByRoleIDSecondCompleted);
            permClient.GetSysRoleSingleInfoByIdCompleted += new EventHandler<GetSysRoleSingleInfoByIdCompletedEventArgs>(permClient_GetSysRoleSingleInfoByIdCompleted);
            permClient.SysRoleInfoUpdateCompleted += new EventHandler<SysRoleInfoUpdateCompletedEventArgs>(permClient_SysRoleInfoUpdateCompleted);
            permClient.UserRoleApplyBatchAddRoleEntityPermissionInfosCompleted += new EventHandler<UserRoleApplyBatchAddRoleEntityPermissionInfosCompletedEventArgs>(permClient_UserRoleApplyBatchAddRoleEntityPermissionInfosCompleted);
        }

        void permClient_SysRoleInfoUpdateCompleted(object sender, SysRoleInfoUpdateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    if (e.StrResult != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.StrResult, "COMPANYDOCUMENT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                    else
                    {

                        if (e.UserState.ToString() == "Edit")
                        {
                            
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                            if (saveType == RefreshedTypes.CloseAndReloadData)
                            {
                                RefreshUI(saveType);
                            }
                        }
                        else if (e.UserState.ToString() == "Audit")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                        else if (e.UserState.ToString() == "Submit")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                        RefreshUI(RefreshedTypes.All);

                    }

                }


            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void permClient_UserRoleApplyBatchAddRoleEntityPermissionInfosCompleted(object sender, UserRoleApplyBatchAddRoleEntityPermissionInfosCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.FormType = FormTypes.Edit;
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            
            if (e.Result)
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if(EntityMenuCustomer.SelectedMenus.Count() >0)
                    EntityMenuCustomer.PermissionAppSave();//保存自定义权限

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "PERMISSIONAPP"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;

            }
            else
            {

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SAVEFAILED", "PERMISSIONAPP"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }

        
        #endregion

        #region 获取角色信息

        void permClient_GetSysRoleSingleInfoByIdCompleted(object sender, GetSysRoleSingleInfoByIdCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                tmprole = e.Result as T_SYS_ROLE;
                tmpDictionaryValue = tmprole.SYSTEMTYPE;
                //string aa = GetDictionaryInfo(tmpDictionaryValue);
                GetDictionaryInfo(tmprole.SYSTEMTYPE);
                roleInfo = tmprole;
                //this.txtRemark.Text = tmprole.REMARK != null ? tmprole.REMARK : "";
                if (!string.IsNullOrEmpty(tmprole.OWNERID))
                {
                    //personnelClient.GetEmployeeDetailByIDAsync(tmprole.OWNERID);
                }
                //this.txtRoleName.Text = tmprole.ROLENAME;
                
            }
        }

        #endregion

        #region 获取字典值，并赋给控件
        
        /// <summary>
        /// 获取选取的字典信息
        /// </summary>
        /// <param name="DictionaryValue"></param>
        /// <returns></returns>
        private void GetDictionaryInfo(string DictionaryValue)
        {
            
            if (!string.IsNullOrEmpty(DictionaryValue))
            {
                foreach (T_SYS_DICTIONARY Region in cbxSystemType.Items)
                {
                    if (Region.DICTIONARYVALUE.ToString() == DictionaryValue)
                    {
                        cbxSystemType.SelectedItem = Region;
                        break;
                    }
                }
            }

            
        }

        #endregion

        #region 获取角色对应的角色菜单值
        
        
        void permClient_GetRoleEntityIDListInfosByRoleIDNewToUserRoleAppCompleted(object sender, GetRoleEntityIDListInfosByRoleIDNewToUserRoleAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                return;
            }
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    tmpEditRoleEntityLIst = e.Result.ToList();
                    foreach (V_RoleEntity menu in tmpEditRoleEntityLIst)
                    {
                        tmpRoleEntityIDsList.Add(menu.ROLEENTITYMENUID);
                    }
                    ListShowMenus = e.listmenu.ToList();
                    SelectedMenus = ListShowMenus;
                    permClient.GetPermissionByRoleIDSecondAsync(tmpRoleID);
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }

            }
        }
        #endregion

        #region 获取角色菜单的权限值
        
        
        void permClient_GetPermissionByRoleIDSecondCompleted(object sender, GetPermissionByRoleIDSecondCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
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
                    
                    return;
                }
            }
        }
        #endregion

        #region 设置菜单对应的角色值
        
        
        /// <summary>
        /// 获取角色菜单权限范围完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InitSetPermissionValue()
        {
            if (ListShowMenus != null) this.DaGr.ItemsSource = ListShowMenus;            
            DaGr.Loaded += new RoutedEventHandler(DaGr_Loaded);            
            
        }

        void DaGr_Loaded(object sender, RoutedEventArgs e)
        {
            FillPermissionDataRange(DaGr, "myChkBtnHR", "HRrating");
        }
        #endregion

        #region 获取所有的权限名称


        void permClient_GetSysCommonPermissionAllCompleted(object sender, GetSysCommonPermissionAllCompletedEventArgs e)
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
                    DataGridColumnsAdd(DaGr, "HRCellTemplate");//添加不动的列头
                    DataGridColumnsAdd(DaGrHead, "");
                    if (!string.IsNullOrEmpty(tmpRoleID))
                    {
                        //permClient.GetRoleEntityIDListInfosByRoleIDAsync(tmpRoleID);

                        ObservableCollection<T_SYS_ENTITYMENU> listmenu = new ObservableCollection<T_SYS_ENTITYMENU>();
                        permClient.GetRoleEntityIDListInfosByRoleIDNewToUserRoleAppAsync(tmpRoleID, listmenu);
                    }
                }
            }
        }
        #endregion

        #region 动态加载列
        
        /// <summary>
        /// 创建权限列
        /// </summary>
        /// <param name="dg"></param>
        private void DataGridColumnsAdd(DataGrid dg, string resources)
        {

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
        #endregion
        
        #region 保存函数
        private void Save()
        {
            if (operationType == FormTypes.New)
            {
                roleInfo.ROLENAME = Common.CurrentLoginUserInfo.EmployeeName+"权限申请";
                roleInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                roleInfo.CREATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
                roleInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                roleInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                roleInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                roleInfo.REMARK = Common.CurrentLoginUserInfo.EmployeeName + "权限申请";
                roleInfo.SYSTEMTYPE = "";
                roleInfo.CHECKSTATE = "0";
                roleInfo.ISAUTHORY = "1";
                roleInfo.UPDATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
                roleInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                roleInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                roleInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                roleInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                roleInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                //permClient.SysRoleInfoAddAsync(roleInfo);
                
                
                
            }
            SaveGridMenuPermission(DaGr, "HRrating");
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (IsRoleEntityMenuChange)
            {
                permClient.UserRoleApplyBatchAddRoleEntityPermissionInfosAsync(roleInfo, tmpAllList, roleInfo.OWNERID);
            }
            else
            {
                if (EntityMenuCustomer.SelectedMenus.Count() > 0)
                    EntityMenuCustomer.PermissionAppSave();//保存自定义权限

                RefreshUI(RefreshedTypes.HideProgressBar);
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
        }


        private void SaveGridMenuPermission(DataGrid Dtgrid, string rateName)
        {
            if (Dtgrid.ItemsSource != null)
            {
                foreach (object obj in Dtgrid.ItemsSource)
                {
                    //T_SYS_ENTITYMENU menu = new T_SYS_ENTITYMENU();

                    if (Dtgrid.Columns[1].GetCellContent(obj) != null)
                    {
                        //menu = Dtgrid.Columns[0].GetCellContent(obj).DataContext as T_SYS_ENTITYMENU;
                        //tmpAllList += menu.T_SYS_ENTITYMENU2.ENTITYMENUID + ",";
                        string StrMenuID = ""; //菜单ID
                        menu = Dtgrid.Columns[1].GetCellContent(obj).DataContext as T_SYS_ENTITYMENU;
                        StrMenuID = menu.ENTITYMENUID;
                        string StrPermissionID = "";//权限ID
                        int PermCount = 0;
                        PermCount = tmpPermission.Count;
                        int IndexCount = 1;
                        //IndexCount = PermCount
                        bool IsCheckRange = false;//是否选择了权限范围 i =2  表示从第3列开始计算
                        for (int i = 2; i < PermCount + 2; i++)
                        {
                            IndexCount = IndexCount + i;
                            if (Dtgrid.Columns[i].GetCellContent(obj) != null)
                            {
                                string NewDataRange = "";
                                /* 
                                 * 首先根据菜单ID和角色ID获取  角色菜单表的记录 存在则获取ROLEENTITYID
                                 * 然后再根据 ROLEENTITYID和权限ID 获取角色菜单权限表中的记录，取得对应的权限和现在的权限值比较
                                 * 如果相同则不处理  不同则处理
                                 */
                                StrPermissionID = tmpPermission[i - 2].PERMISSIONID;  //权限ID
                                //var q = from a in EntityPermissionInfosList//权限视图集合
                                //        where a.EntityRole.T_SYS_ENTITYMENU.ENTITYMENUID == StrMenuID
                                //        && a.EntityRole.T_SYS_ROLE.ROLEID == tmprole.ROLEID
                                //        select a;
                                var q = from a in EntityPermissionInfosListSecond//权限视图集合
                                        where a.EntityMenuID == StrMenuID
                                        && a.RoleID == tmprole.ROLEID
                                        select a;
                                string RoleEntityID = "";
                                if (q.Count() > 0)
                                {
                                    RoleEntityID = q.ToList().FirstOrDefault().RoleEntityMenuID.ToString(); //获取角色菜单ID
                                }
                                //var m = from a in EntityPermissionInfosList
                                //var k = from b in EntityPermissionInfosList//权限视图集合
                                //        where b.RoleMenuPermission.T_SYS_PERMISSION.PERMISSIONID == StrPermissionID && b.RoleMenuPermission.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == RoleEntityID
                                //        select b;
                                var k = from b in EntityPermissionInfosListSecond//权限视图集合
                                        where b.PermissionID == StrPermissionID && b.RoleEntityMenuID == RoleEntityID
                                        select b;
                                string OldRangeValue = ""; //获取数据库中 权限对应的值

                                if (k.Count() > 0)
                                {
                                    OldRangeValue = k.ToList().FirstOrDefault().PermissionDataRange.ToString();
                                }
                                //Rating hrrate = DaGrHR.Columns[i].GetCellContent(obj).FindName("HRrating") as Rating;
                                Button hrrate = Dtgrid.Columns[i].GetCellContent(obj).FindName(rateName) as Button;

                                switch (hrrate.Content.ToString())
                                {
                                    case "★"://员工 0.2 ×10
                                        NewDataRange = "4";
                                        break;
                                    case "★★"://岗位 0.4×10
                                        NewDataRange = "3";
                                        break;
                                    case "★★★"://部门 0.6*10
                                        NewDataRange = "2";
                                        break;
                                    case "★★★★"://公司 0.8*10
                                        NewDataRange = "1";
                                        break;
                                    //case "★★★★★"://集团 1.0*10
                                    //    NewDataRange = "0";
                                    //    break;
                                    case ""://权限
                                        NewDataRange = "";
                                        break;
                                }
                                if (OldRangeValue != NewDataRange)
                                {
                                    tmpAllList += NewDataRange + ",";
                                    tmpAllList += tmpPermission[i - 2].PERMISSIONID + ";";
                                    IsCheckRange = true;
                                }
                            }
                        }
                        if (IsCheckRange)
                        {
                            tmpAllList += "@" + StrMenuID + "," + "";
                            tmpAllList += "#";
                        }
                    }
                }

                if (IsAdd)
                {
                    if (string.IsNullOrEmpty(tmpAllList))
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请先设置权限", Utility.GetResourceStr("CONFIRMBUTTON"));
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        return;
                    }

                }
            }
        }

        private bool Check()
        {
            bool IsReturn = true;
            //string StrName = this.txtRoleName.Text.ToString().Trim();
            //if (StrName.Length == 0)
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "ROLENAME"),
            //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return false;
            //}
            return IsReturn;
        }
        #endregion

        #region Iclient接口

        public void ClosedWCFClient()
        {
            permClient.DoClose();
            personnelClient.DoClose();
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

        #region IEntityEditor接口

        public string GetTitle()
        {
            string StrReturn = "";
            if (operationType == FormTypes.New)
            {
                StrReturn = Utility.GetResourceStr("ADDTITLE", "PERMISSIONAPP");
            }
            if (operationType == FormTypes.Edit)
            {
                StrReturn = Utility.GetResourceStr("EDITTITLE", "PERMISSIONAPP");
            }
            if (operationType == FormTypes.Browse)
            {
                StrReturn = Utility.GetResourceStr("VIEWTITLE", "PERMISSIONAPP");
            }
            if (operationType == FormTypes.Audit)
            {
                StrReturn = Utility.GetResourceStr("AUDIT", "PERMISSIONAPP");
            }
            return StrReturn;
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    saveType = RefreshedTypes.All;
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
                    Save();
                    break;
            }
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
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            if (operationType != FormTypes.Browse && operationType != FormTypes.Audit)
            {

                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1", //保存并关闭
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

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            if (operationType == FormTypes.Edit)
            {
                EntityBrowser browser = this.FindParentByType<EntityBrowser>();
                browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            }
            string strXmlObjectSource = string.Empty;
                        
            Utility.SetAuditEntity(entity, "T_SYS_ROLEAPP", roleInfo.ROLEID, strXmlObjectSource);
            
        }
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (Common.CurrentLoginUserInfo.EmployeeID != roleInfo.OWNERID)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
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
            if (roleInfo.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            roleInfo.CHECKSTATE = state;
            roleInfo.UPDATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
            //_VM.UpdateApporvalAsync(approvalInfo, UserState);
            string StrResult = "";
            permClient.SysRoleInfoUpdateAsync(roleInfo,StrResult,UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (roleInfo != null)
            {
                state = roleInfo.CHECKSTATE;
                //if (operationType == FormTypes.Browse)
                //    state = "-1";
                if (operationType == FormTypes.Resubmit)
                    state = "0";
            }

            return state;
        }
        #endregion

        #region HRrating事件
        
        
        private void HRrating_Click(object sender, RoutedEventArgs e)
        {
            IsRoleEntityMenuChange = true;
            Button Txtrating = sender as Button;
            SetPermissionRate(Txtrating);
        }

        /// <summary>
        /// 点击事件-设置权限数据范围
        /// </summary>
        /// <param name="sender"></param>
        private static void SetPermissionRate(Button sender)
        {
            Button Hrrating = sender as Button;
            string StrContent = "";
            if (Hrrating.Content == null)
            {
                StrContent = "";
            }
            else
            {
                StrContent = Hrrating.Content.ToString();
            }

            switch (StrContent)
            {
                case "":
                    StrContent = "★";
                    break;
                case "★":
                    StrContent = "★★";
                    break;
                case "★★":
                    StrContent = "★★★";
                    break;
                case "★★★":
                    StrContent = "★★★★";
                    break;
                //case "★★★★":
                //    StrContent = "★★★★★";
                //    break;
                case "★★★★":
                    StrContent = "";
                    break;
            }
            Hrrating.Content = StrContent;
        }

        #endregion

        #region 单击菜单


        private void HRBtn2_Click(object sender, RoutedEventArgs e)
        {
            IsRoleEntityMenuChange = true;
            Button Btn = sender as Button;
            MenuSetPermissionRate(Btn, DaGr, "HRrating");
        }

        /// <summary>
        /// 菜单点击事件-设置所有权限数据范围
        /// </summary>
        /// <param name="sender"></param>
        private void MenuSetPermissionRate(Button sender, DataGrid dtGrid, string rateName)
        {
            Button BtnPM = sender as Button;
            if (BtnPM.Tag == null)
            {
                MessageBox.Show("无法选择此行！");
                return;
            }
            DataGridRowEventArgs PMrating = BtnPM.Tag as DataGridRowEventArgs;
            string StrContent = "";
            int PermCount = 0;
            PermCount = tmpPermission.Count;

            for (int i = 2; i < PermCount + 2; i++)
            {
                Button mybtn = dtGrid.Columns[i].GetCellContent(PMrating.Row).FindName(rateName) as Button;
                //StrContent = mybtn.Content.ToString();
                if (mybtn.Content == null)
                {
                    StrContent = "";
                }
                else
                {
                    StrContent = mybtn.Content.ToString();
                }

                switch (StrContent)
                {
                    case "":
                        StrContent = "★";
                        break;
                    case "★":
                        StrContent = "★★";
                        break;
                    case "★★":
                        StrContent = "★★★";
                        break;
                    case "★★★":
                        StrContent = "★★★★";
                        break;
                    //case "★★★★":
                    //    StrContent = "★★★★★";
                    //    break;
                    case "★★★★":
                        StrContent = "";
                        break;
                }
                mybtn.Content = StrContent;
            }


        }

        #endregion

        #region 初始化dagr的样式
        
        private void InitDataGridCloumn()
        {            
            DaGr.Style = Application.Current.Resources["DataGridStyle"] as Style;            
            DaGr.CacheMode = new BitmapCache();            
        }
        #endregion

        #region 填充星星

        void DaGrHR_Loaded(object sender, RoutedEventArgs e)
        {
            FillPermissionDataRange(DaGr, "myChkBtnHR", "HRrating");
        }

        /// <summary>
        /// 填充Grid星星★★★★★权限
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="chx"></param>
        /// <param name="btn"></param>
        private void FillPermissionDataRange(DataGrid dg, string chx, string btn)
        {
            if (dg.Tag == "1")
            {//如果填充过了，就不填了
                return;
            }
            //注消事件
            //RoleClient.GetRolePermsCompleted -= new EventHandler<GetRolePermsCompletedEventArgs>(RoleClient_GetRolePermsCompleted);
            //if (tmpEditRoleEntityPermList.Count() == 0) return;
            if (EntityPermissionInfosListSecond.Count() == 0) return;
            if (dg.Columns.Count < tmpPermission.Count) return;//还没有动态生成列
            if (dg.ItemsSource != null)
            {
                #region
                //T_SYS_ROLEENTITYMENU tmpRoleEntity = new T_SYS_ROLEENTITYMENU();
                V_UserPermissionRoleID tmpRoleEntity = new V_UserPermissionRoleID();
                //Button hrrate;
                Button hrrate;
                if (dg.ItemsSource == null)
                    return;
                foreach (T_SYS_ENTITYMENU obj in (List<T_SYS_ENTITYMENU>)dg.ItemsSource)
                {
                    //var bb = from a in tmpEditRoleEntityLIst
                    //         where a.T_SYS_ENTITYMENU.ENTITYMENUID == obj.ENTITYMENUID
                    //         select a;
                    var bb = from a in EntityPermissionInfosListSecond
                             where a.EntityMenuID == obj.ENTITYMENUID
                             select a;
                    if (bb.Count() > 0)
                    {
                        tmpRoleEntity = bb.FirstOrDefault();
                        int PermCount = tmpPermission.Count;
                        for (int i = 2; i < PermCount + 2; i++)
                        {
                            if (dg.Columns[i].GetCellContent(obj) != null)
                            {
                                var roles = from cc in EntityPermissionInfosListSecond
                                            where cc.RoleEntityMenuID == tmpRoleEntity.RoleEntityMenuID
                                            && cc.PermissionID == tmpPermission[i - 2].PERMISSIONID
                                            select cc;
                                if (roles.Count() > 0)
                                {
                                    //hrrate = dg.Columns[i].GetCellContent(obj).FindName(btn) as Button;
                                    hrrate = dg.Columns[i].GetCellContent(obj).FindName(btn) as Button;
                                    if (hrrate == null) continue;
                                    #region 填星星
                                    switch (roles.FirstOrDefault().PermissionDataRange)
                                    {
                                        //case "0"://集团
                                        //    //hrrate.Value = 1;
                                        //    hrrate.Content = "★★★★★";
                                        //    break;
                                        case "1"://公司
                                            //hrrate.Value = 0.8;
                                            hrrate.Content = "★★★★";
                                            break;
                                        case "2"://部门
                                            //hrrate.Value = 0.6;
                                            hrrate.Content = "★★★";
                                            break;
                                        case "3"://岗位
                                            //hrrate.Value = 0.4;
                                            hrrate.Content = "★★";
                                            break;
                                        case "4"://个人
                                            //hrrate.Value = 0.2;
                                            hrrate.Content = "★";
                                            break;

                                    }
                                    #endregion
                                    if (dg.Tag != "1")
                                    {
                                        dg.Tag = "1";
                                    }
                                }
                            }
                        }
                    }
                    //}
                }
                #endregion
            }
        }
        #endregion

        #region LoadingRow事件       
        
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_SYS_ENTITYMENU drMenu = (T_SYS_ENTITYMENU)e.Row.DataContext;
            Button myhrBtn = DaGr.Columns[1].GetCellContent(e.Row).FindName("HRBtn2") as Button;
            CheckBox mychk = DaGr.Columns[0].GetCellContent(e.Row).FindName("ChkBox") as CheckBox;
            if (mychk != null && myhrBtn != null)
            {
                mychk.Tag = drMenu;
                myhrBtn.Tag = e;
            }

        }
        #endregion

        #region 右上方全选事件
        
        
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        #region checkbox事件

        private void chk_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region checkbox事件

        private void ChkBoxrole_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb1 = sender as CheckBox;
            cb1.IsChecked = true;
            SetCheckedMenu(cb1);
        }

        private void ChkBoxrole_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb1 = sender as CheckBox;
            cb1.IsChecked = false;
            SetUnCheckedMenu(cb1);
        }

        

        private void SetCheckedMenu(CheckBox chBox)
        {
            
            

            //PermissionValue PerObj = new PermissionValue();
            //PerObj.Permission = entPerm.PERMISSIONID;
            //T_SYS_ENTITYMENU entTemp = DaGrMenu.SelectedItems[0] as T_SYS_ENTITYMENU;
            //AddCustomPermissionByPerm(entTemp, PerObj);
        }

        private void SetUnCheckedMenu(CheckBox chBox)
        {
            if (DaGr.SelectedItems.Count == 0)
            {
                return;
            }
            T_SYS_ENTITYMENU entPerm = chBox.Tag as T_SYS_ENTITYMENU;

            //PermissionValue PerObj = new PermissionValue();
            //PerObj.Permission = entPerm.PERMISSIONID;
            //T_SYS_ENTITYMENU entTemp = DaGrMenu.SelectedItems[0] as T_SYS_ENTITYMENU;
            //RemoveCustomPermissionByPerm(entTemp, PerObj);
        }

        #endregion

        


    }
}
