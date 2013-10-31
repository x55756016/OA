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
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.UserControls
{
    public partial class SysUserRoleSet : UserControl,IEntityEditor
    {
        protected static PermissionServiceClient ServiceClient = new PermissionServiceClient();//龙康才新增
        SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient orgclient = new OrganizationServiceClient();
       // protected PermissionServiceClient ServiceClient;        
        OrganizationServiceClient Organ = new OrganizationServiceClient();
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        private T_SYS_USER tmpUser=new T_SYS_USER();
        private T_SYS_ROLE role;
        private string StrCompanyID="";//所属公司
        private string StrDepartmentID = "";//所属部门
        private string StrPositionID = ""; //所在岗位
        private string StrAction = "0"; //默认 0 为开始设置  1 为修改
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        private ObservableCollection<string> PositionidsList = new ObservableCollection<string>(); //员工所属岗位ID集合，用来获取岗位信息
        private ObservableCollection<T_SYS_USERROLE> ViewInfosList = new ObservableCollection<T_SYS_USERROLE>();
        private ObservableCollection<T_SYS_ROLE> ViewRoleList = new ObservableCollection<T_SYS_ROLE>();
        private List<T_SYS_USERROLE> tmpRoleList = new List<T_SYS_USERROLE>();//roleentityid 集合
        public string EntityLogo { get; set; }
        string systype = ""; //系统类型 
        public T_SYS_ROLE Role
        {
            get { return role; }
            set { role = value; }
        }

        public SysUserRoleSet(T_SYS_USER UserObj,T_SYS_USERROLE userRole)
        {
            if (Application.Current.Resources["SYS_DICTIONARY"] == null)
                LoadDicts();
            tmpUser = UserObj;
            this.GetTitle();
            InitializeComponent();
            if (userRole != null)
            {
                StrAction = "1";//修改用户角色
            }
            this.Loaded += new RoutedEventHandler(SysUserRoleSet_Loaded);
            ViewInfosList.Clear();
        }

        protected void LoadDicts()
        {
            ServiceClient.GetSysDictionaryByCategoryCompleted += (o, e) =>
            {
                List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
                dicts = e.Result == null ? null : e.Result.ToList();
                Application.Current.Resources.Add("SYS_DICTIONARY", dicts);

                
            };
            //TODO: 按需取出字典值
            ServiceClient.GetSysDictionaryByCategoryAsync("");
        }

        void SysUserRoleSet_Loaded(object sender, RoutedEventArgs e)
        {
            InitControlEvent();            
        }

        private void InitControlEvent()
        {
            ServiceClient = new PermissionServiceClient();
            ServiceClient.GetSysRoleInfosCompleted += new EventHandler<GetSysRoleInfosCompletedEventArgs>(SysRoleClient_GetSysRoleInfosCompleted);
            ServiceClient.UserRoleBatchAddInfosCompleted += new EventHandler<UserRoleBatchAddInfosCompletedEventArgs>(SysRoleClient_UserRoleBatchAddInfosCompleted);
            ServiceClient.GetSysUserRoleByUserCompleted += new EventHandler<GetSysUserRoleByUserCompletedEventArgs>(ServiceClient_GetSysUserRoleByUserCompleted);
            personclient.GetAllPostByEmployeeIDCompleted += new EventHandler<GetAllPostByEmployeeIDCompletedEventArgs>(personclient_GetAllPostByEmployeeIDCompleted);
            
            ServiceClient.BatchAddUserRoleCompleted += new EventHandler<BatchAddUserRoleCompletedEventArgs>(ServiceClient_BatchAddUserRoleCompleted);
            ServiceClient.GetSysRoleInfosByCompanyIdAndDepartmentIdCompleted += new EventHandler<GetSysRoleInfosByCompanyIdAndDepartmentIdCompletedEventArgs>(ServiceClient_GetSysRoleInfosByCompanyIdAndDepartmentIdCompleted);
            ServiceClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(ServiceClient_GetSysDictionaryByCategoryCompleted);
            
            //StrCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //txtCompany.Text = Common.CurrentLoginUserInfo.companyName;
            orgclient.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(orgclient_GetCompanyByIdCompleted);
            orgclient.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(orgclient_GetDepartmentByIdCompleted);

            personclient.GetAllPostByEmployeeIDAsync(tmpUser.EMPLOYEEID);
            
            
            ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
           
        }

        void orgclient_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    StrDepartmentID = e.Result.DEPARTMENTID;
                    txtDepartmentName.Text = e.Result.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                }
                LoadData();
            }
        }

        void ServiceClient_GetSysRoleInfosByCompanyIdAndDepartmentIdCompleted(object sender, GetSysRoleInfosByCompanyIdAndDepartmentIdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_ROLE> menulist = e.Result.ToList();
                var q = from ent in menulist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 200;
            }
            dataPager.DataContext = pcv;
            DtGrid_user.ItemsSource = pcv;

            ServiceClient.GetSysUserRoleByUserAsync(tmpUser.SYSUSERID);
        }

        void ServiceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            //绑定系统类型
            if (e.Result != null)
            {
                ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(gridfirst, "cbxSystemType");
                List<T_SYS_DICTIONARY> dicts = e.Result.ToList();

                cbxSystemType.ItemsSource = dicts;
                cbxSystemType.DisplayMemberPath = "DICTIONARYNAME";


            }
        }

        void ServiceClient_BatchAddUserRoleCompleted(object sender, BatchAddUserRoleCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result)
                {
                      ViewInfosList.Clear(); //清空列表记录
                      Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ACCREDITUSERSUCCED"));
                      RefreshUI(RefreshedTypes.CloseAndReloadData);
                    
                }
                else
                {
                    if (string.IsNullOrEmpty(e.IsResult))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));
                        return;
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), "添加了相同的角色");
                        return;
                    }
                    
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),e.Error.ToString());
                return;
            }
            //throw new NotImplementedException();
        }
        
        void personclient_GetAllPostByEmployeeIDCompleted(object sender, GetAllPostByEmployeeIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST> dicts = e.Result.ToList();
                var qUsingDicts = from d in dicts
                                 where d.CHECKSTATE == "2" && d.EDITSTATE == "1"
                                 select d;
                List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST> UsingDicts;
                if (qUsingDicts != null) UsingDicts = qUsingDicts.ToList();
                else UsingDicts = null;
                //dicts[0].T_HR_EMPLOYEE.t
                ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(gridfirst, "cbxPosition");
                cbxSystemType.ItemsSource = UsingDicts;
                cbxSystemType.DisplayMemberPath = "T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME";
                if (dicts.Count > 0)
                {
                    cbxPosition.SelectedIndex = 0;
                    
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST dict = cbxSystemType.SelectedItem as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST;

                     StrPositionID = dict == null ? "" : dict.T_HR_POST.POSTID.ToString();
                     orgclient.GetCompanyByIdAsync(tmpUser.OWNERCOMPANYID);
                     //orgclient.GetDepartmentByIdAsync(tmpUser.OWNERDEPARTMENTID);
                }

            }
            //LoadData();
        }

        void orgclient_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    StrCompanyID = e.Result.COMPANYID;
                    txtCompany.Text = e.Result.CNAME;
                }
                //LoadData();
                orgclient.GetDepartmentByIdAsync(tmpUser.OWNERDEPARTMENTID);
            }
        }

        void ServiceClient_GetSysUserRoleByUserCompleted(object sender, GetSysUserRoleByUserCompletedEventArgs e)
        {
            
            if (e.Result != null)
            {
                List<T_SYS_USERROLE> listuserrole = new List<T_SYS_USERROLE>();
                listuserrole = e.Result.ToList();
                foreach (T_SYS_USERROLE roleid in listuserrole)
                {
                    tmpRoleList.Add(roleid);
                }
                //SetUserRoleInfos();
                //this.DtGrid.Loaded += new RoutedEventHandler(DtGrid_Loaded);
            }
        }
        
        

        void SysRoleClient_GetSysRoleInfosCompleted(object sender, GetSysRoleInfosCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_ROLE> menulist = e.Result.ToList();
                var q = from ent in menulist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 200;
            }
            dataPager.DataContext = pcv;
            DtGrid_user.ItemsSource = pcv;
            
            ServiceClient.GetSysUserRoleByUserAsync(tmpUser.SYSUSERID);
            
            
        }

        

        void SysRoleClient_UserRoleBatchAddInfosCompleted(object sender, UserRoleBatchAddInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "用户授权成功", Utility.GetResourceStr("CONFIRMBUTTON"));
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ACCREDITUSERSUCCED"));
                }
            }
        }
        public void SetRowLogo(DataGrid DtGrid, DataGridRow row, string entityCode)
        {
            if (DtGrid.ItemsSource != null)
            {
                Image logo = DtGrid.Columns[0].GetCellContent(row).FindName("entityLogo") as Image;
                if (logo != null)
                {
                    if (string.IsNullOrEmpty(EntityLogo))
                    {
                        PermissionServiceClient client = new PermissionServiceClient();
                        //SMT.Saas.Tools.PermissionWS.PermissionServiceClient client = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
                        client.GetSysMenuByEntityCodeCompleted += new EventHandler<GetSysMenuByEntityCodeCompletedEventArgs>(client_GetSysMenuByEntityCodeCompleted);
                        client.GetSysMenuByEntityCodeAsync(entityCode, logo);
                    }
                    else
                    {
                        logo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(EntityLogo, UriKind.Relative));
                    }
                }
            }
        }
        void client_GetSysMenuByEntityCodeCompleted(object sender, GetSysMenuByEntityCodeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    T_SYS_ENTITYMENU menu = e.Result;
                    EntityLogo = menu.MENUICONPATH;

                    Image logo = e.UserState as Image;

                    if (logo != null)
                    {
                        logo.Margin = new Thickness(2, 2, 0, 0);
                        logo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(EntityLogo, UriKind.Relative));
                    }
                }
            }
        }
        
        /// <summary>
        /// 加载菜单数据
        /// </summary>
        private void LoadData()
        {            
            //ServiceClient.GetSysRoleInfosAsync(systype,StrCompanyID,StrDepartmentID);
            RefreshUI(RefreshedTypes.ShowProgressBar);
            ServiceClient.GetSysRoleInfosByCompanyIdAndDepartmentIdAsync(systype, StrCompanyID, StrDepartmentID);
        }


             

        #region 模板中checkbox单击事件
        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {
            
            

        }
        #endregion

        #region 全选事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            
        }


        private void CheckAllRole(bool IsChecked, T_SYS_ROLE RoleT)
        {
           
        }


        #endregion

        #region LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid_user, e.Row, "T_SYS_ROLE");

        }

        #endregion



        

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            AddSave();
        }

        private void AddSave()
        {
            if (string.IsNullOrEmpty(StrCompanyID))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTCOMPANY"));
                return;
            }
            if (string.IsNullOrEmpty(StrPositionID))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTPOSITION"));
                return;
            }
            
            AddUserRole();
            if (ViewInfosList.Count > 0)
            {
                if (StrAction == "0")
                {
                    string Isresult = "";
                    ServiceClient.BatchAddUserRoleAsync(ViewInfosList,Isresult);
                }
                else
                { 
                    //修改某条具体的用户角色信息
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECTEDROLE"));
                return;
            }
        }

        private void AddUserRole()
        {
            
            if (string.IsNullOrEmpty(StrCompanyID))
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(""));
                //return;
            }
            ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(gridfirst, "cbxPosition");
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST dict = cbxSystemType.SelectedItem as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST;
            if(dict !=null)
                StrPositionID = dict.T_HR_POST.POSTID;
            

            T_SYS_ROLE RoleT = new T_SYS_ROLE();
            if (this.DtGrid_user.SelectedItems.Count >0)
            {
                ViewInfosList.Clear();
                for (int i = 0; i < DtGrid_user.SelectedItems.Count; i++)
                {
                    
                    Role = (T_SYS_ROLE)DtGrid_user.SelectedItems[i];//获取当前选中的行数据并转换为对应的实体 

                    var q = from a in ViewRoleList
                            where a == Role
                            select a;
                    if (!(q.Count() > 0))
                    {
                        ViewRoleList.Add(Role);//添加角色
                        T_SYS_USERROLE role = new T_SYS_USERROLE();
                        role.USERROLEID = System.Guid.NewGuid().ToString();
                        role.CREATEDATE = System.DateTime.Now;
                        role.OWNERCOMPANYID = StrCompanyID;
                        role.POSTID = StrPositionID;
                        role.EMPLOYEEPOSTID = StrPositionID;
                        role.T_SYS_ROLEReference=new EntityReferenceOfT_SYS_ROLECIR1sILv();
                        role.T_SYS_ROLEReference.EntityKey = Role.EntityKey;

                        role.T_SYS_USERReference=new EntityReferenceOfT_SYS_USERCIR1sILv();
                        role.T_SYS_USERReference.EntityKey = tmpUser.EntityKey;
                        role.CREATEUSER = Common.CurrentLoginUserInfo.SysUserID;
                        ViewInfosList.Add(role);//添加角色用户
                    }
                }             

                

            }
        }
        
             


        #region 获取公司信息
		private void CompanyObject_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    var q = from up in Common.CurrentLoginUserInfo.UserPosts
                            where up.CompanyID == companyInfo.ObjectID
                            select ent;

                    if (q.Count() > 0)
                    {

                        StrCompanyID = companyInfo.ObjectID;
                        txtCompany.Text = companyInfo.ObjectName;
                        StrDepartmentID = "";
                        txtDepartmentName.Text = "";
                        LoadData();

                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "此公司的角色您没有权限使用！", Utility.GetResourceStr("CONFIRMBUTTON"));
                    }
                }
                
              
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }

        private void GetCompanyNameByCompanyID(string StrCompanyID)
        {            
            Organ.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(Organ_GetCompanyByIdCompleted);
            Organ.GetCompanyByIdAsync(StrCompanyID);
        }

        void Organ_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                    company = e.Result;
                    StrCompanyID = company.COMPANYID;
                    txtCompany.Text = company.CNAME;

                }
            }
        } 
	#endregion

        #region 获取部门信息
        /// <summary>
        /// 获取部门信息
        /// </summary>
        /// <param name="StrDepartmentID"></param>
        void GetDepartmentNameByDepartmentID(string StrDepartmentID)
        {
            Organ.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(Organ_GetDepartmentByIdCompleted);
            Organ.GetDepartmentByIdAsync(StrDepartmentID);
        }

        void Organ_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                    department = e.Result;
                    StrDepartmentID = department.DEPARTMENTID;
                    txtDepartmentName.Text = department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    LoadData();
                }
            }
        }


        /// <summary>
        /// 选择部门按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLookUpDepartment_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    StrDepartmentID = companyInfo.ObjectID;
                    txtDepartmentName.Text = companyInfo.ObjectName;
                    LoadData();
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }



        #endregion




        #region IEntityEditor 成员

        public string GetTitle()
        {
            //return Utility.GetResourceStr("SYSROLE");
            return tmpUser.EMPLOYEENAME + "角色设置";
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

                    AddSave();

                    break;
                case "1":

                    AddSave();
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
                ImageUrl = "/SMT.SaaS.Images;Component/Images/ToolBar/16_SaveClose.png"
            };

            items.Add(item);

            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "0",
            //    Title = Utility.GetResourceStr("SAVE"),
            //    ImageUrl = "/SMT.SaaS.Images;Component/Images/ToolBar/16_SAVE.png"
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

        private void cbxSystemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(gridfirst, "cbxSystemType");

            if (cbxSystemType.SelectedIndex > 0)
            {
                
                T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
                systype = dict == null ? "" : dict.DICTIONARYVALUE.GetValueOrDefault().ToString();
            }
            
            LoadData();
        }

        private void cbxPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(gridfirst, "cbxPosition");
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST dict = cbxSystemType.SelectedItem as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST;

            StrPositionID = dict == null ? "" : dict.T_HR_POST.POSTID.ToString();
        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (StrAction == "1")
            //{
            //    foreach (object obj in DtGrid.ItemsSource)
            //    {
            //        if (DtGrid.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DtGrid.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox; //cb为
            //            cb1.IsChecked = false;
            //        }
            //    }
            //    //cb.IsChecked = true;
            //}
            if (string.IsNullOrEmpty(StrCompanyID))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTCOMPANY"));
                return;
            }
            if (string.IsNullOrEmpty(StrPositionID))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTPOSITION"));
                return;
            }
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
            {
                return;
            }
            if (grid.SelectedItems.Count > 0)
            {
                Role = (T_SYS_ROLE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体 

                ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(gridfirst, "cbxPosition");
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST dict = cbxSystemType.SelectedItem as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST;
                if (dict != null)
                    StrPositionID = dict.T_HR_POST.POSTID;

                //T_SYS_ROLE RoleT = new T_SYS_ROLE();
                //RoleT = cb.Tag as T_SYS_ROLE;
                var q = from a in ViewRoleList
                        where a == Role
                        select a;
                if (!(q.Count() > 0))
                {
                    ViewRoleList.Add(Role);
                    T_SYS_USERROLE role = new T_SYS_USERROLE();
                    role.USERROLEID = System.Guid.NewGuid().ToString();
                    role.CREATEDATE = System.DateTime.Now;
                    role.OWNERCOMPANYID = StrCompanyID;
                    role.POSTID = StrPositionID;
                    role.EMPLOYEEPOSTID = StrPositionID;
                    role.T_SYS_USER = tmpUser;
                    role.T_SYS_ROLE = Role;
                    role.CREATEUSER = Common.CurrentLoginUserInfo.SysUserID;
                    ViewInfosList.Add(role);
                }
            }

            
                
            
        }

        

    }
}
