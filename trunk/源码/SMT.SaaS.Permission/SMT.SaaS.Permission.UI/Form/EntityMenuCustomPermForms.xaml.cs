using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.Saas.Tools.PermissionWS;


using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.SaaS.Permission.UI.Form
{
    public partial class EntityMenuCustomPermForms : UserControl, IEntityEditor
    {
        private T_SYS_ENTITYMENUCUSTOMPERM custPerm;
        private string saveType = "0";       //保存方式 0:添加 1:关闭

        public T_SYS_ENTITYMENUCUSTOMPERM CustPerm
        {
            get { return custPerm; }
            set
            {
                custPerm = value;
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

        public EntityMenuCustomPermForms(FormTypes type)
        {
            InitializeComponent();
            FormType = type;
            InitParas("");
        }

        public EntityMenuCustomPermForms(FormTypes type, string CustPermID)
        {
            InitializeComponent();
            FormType = type;
            InitParas(CustPermID);
            if (type == FormTypes.Browse)
            {
                UnEnableFormControl();
            }
        }

        /// <summary>
        /// 禁用表单控件
        /// </summary>
        private void UnEnableFormControl()
        {
            cbxPerm.IsEnabled = false;
            lkParentMenu.IsEnabled = false;
            lkCompany.IsEnabled = false;
            lkDepartment.IsEnabled = false;
            lkPost.IsEnabled = false;
            txtRemark.IsEnabled = false;
            dtStartDate.IsEnabled = false;
            dtEndDate.IsEnabled = false;
        }

        private void InitParas(string CustPermID)
        {
            ServiceClient = new PermissionServiceClient();
            
            ServiceClient.GetEntityMenuCustomPermByIDCompleted += new EventHandler<GetEntityMenuCustomPermByIDCompletedEventArgs>(ServiceClient_GetEntityMenuCustomPermByIDCompleted);
           
            //ServiceClient.GetSysMenuByTypeCompleted += new EventHandler<GetSysMenuByTypeCompletedEventArgs>(ServiceClient_GetSysMenuByTypeCompleted);
           
            ServiceClient.FindSysPermissionByTypeCompleted += new EventHandler<FindSysPermissionByTypeCompletedEventArgs>(ServiceClient_FindSysPermissionByTypeCompleted);
            
            ServiceClient.EntityMenuCustomPermAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_EntityMenuCustomPermAddCompleted);
                        
            ServiceClient.EntityMenuCustomPermUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_EntityMenuCustomPermUpdateCompleted);
            
            if (FormType == FormTypes.New)
            {
                CustPerm = new T_SYS_ENTITYMENUCUSTOMPERM();
                CustPerm.ENTITYMENUCUSTOMPERMID = Guid.NewGuid().ToString();

            }

            //初始化角色权限
            if (!string.IsNullOrEmpty(CustPermID))
            {
                ServiceClient.GetEntityMenuCustomPermByIDAsync(CustPermID);
            }
            else
            {
                //ServiceClient.GetSysMenuByTypeAsync("");
                ServiceClient.FindSysPermissionByTypeAsync("");
            }
        }

        void ServiceClient_EntityMenuCustomPermUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox("错误信息", e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox("信息", Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void ServiceClient_EntityMenuCustomPermAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox("错误信息", e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox("信息", Utility.GetResourceStr("ADDSUCCESSED",""), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        //void ServiceClient_GetSysMenuByTypeCompleted(object sender, GetSysMenuByTypeCompletedEventArgs e)
        //{
        //    cbxMenu.ItemsSource = null;
        //    if (e.Result != null)
        //    {
        //        //绑定角色名称
        //        List<T_SYS_ENTITYMENU> ents = e.Result.ToList();
        //        cbxMenu.ItemsSource = ents;
        //        cbxMenu.DisplayMemberPath = "MENUNAME";
        //        foreach (var item in cbxMenu.Items)
        //        {
        //            T_SYS_ENTITYMENU tmp = item as T_SYS_ENTITYMENU;
        //            if (tmp != null && CustPerm != null && CustPerm.T_SYS_ENTITYMENU != null
        //                && tmp.ENTITYMENUID == this.CustPerm.T_SYS_ENTITYMENU.ENTITYMENUID)
        //            {
        //                cbxMenu.SelectedItem = item;
        //                break;
        //            }
        //        }
        //    }
        //}

        void ServiceClient_GetEntityMenuCustomPermByIDCompleted(object sender, GetEntityMenuCustomPermByIDCompletedEventArgs e)
        {
            CustPerm = e.Result;
            BindORG();

            ServiceClient.GetSysMenuByTypeAsync("","");
            ServiceClient.FindSysPermissionByTypeAsync("");
        }

        #region 数据绑定
        private void BindORG()
        {
            //绑定公司
            if (!string.IsNullOrEmpty(CustPerm.COMPANYID) && !string.IsNullOrEmpty(CustPerm.COMPANYNAME))
            {
                T_HR_COMPANY obj = new T_HR_COMPANY();
                obj.COMPANYID = CustPerm.COMPANYID;
                obj.CNAME = CustPerm.COMPANYNAME;

                lkCompany.DataContext = null;
                lkCompany.DataContext = obj;
            }

            //绑定部门
            if (!string.IsNullOrEmpty(CustPerm.DEPARTMENTID) && !string.IsNullOrEmpty(CustPerm.DEPARTMENTNAME))
            {
                T_HR_DEPARTMENT obj = new T_HR_DEPARTMENT();

                obj.DEPARTMENTID = CustPerm.DEPARTMENTID;
                obj.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                obj.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = CustPerm.DEPARTMENTNAME;

                lkDepartment.DataContext = null;
                lkDepartment.DataContext = obj;
            }

            //绑定岗位
            if (!string.IsNullOrEmpty(CustPerm.POSTID) && !string.IsNullOrEmpty(CustPerm.POSTNAME))
            {
                T_HR_POST obj = new T_HR_POST();

                obj.POSTID = CustPerm.POSTID;
                obj.T_HR_POSTDICTIONARY = new T_HR_POSTDICTIONARY();
                obj.T_HR_POSTDICTIONARY.POSTNAME = CustPerm.POSTNAME;

                lkPost.DataContext = null;
                lkPost.DataContext = obj;
            }
        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void lkCompany_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.All;

            lookup.SelectedClick += (obj, ev) =>
            {

                if (lookup.SelectedObj is T_HR_COMPANY)
                {
                    lkCompany.DataContext = lookup.SelectedObj;
                    lkDepartment.DataContext = null;
                    lkPost.DataContext = null;
                }
                else if (lookup.SelectedObj is T_HR_DEPARTMENT)
                {
                    T_HR_COMPANY company = new T_HR_COMPANY();
                    company.COMPANYID = ((T_HR_DEPARTMENT)lookup.SelectedObj).T_HR_COMPANY.COMPANYID;
                    company.CNAME = ((T_HR_DEPARTMENT)lookup.SelectedObj).T_HR_COMPANY.CNAME;

                    lkCompany.DataContext = company;

                    lkDepartment.DataContext = lookup.SelectedObj;
                    lkPost.DataContext = null;
                }
                else if (lookup.SelectedObj is T_HR_POST)
                {
                    T_HR_COMPANY company = new T_HR_COMPANY();
                    company.COMPANYID = ((T_HR_POST)lookup.SelectedObj).T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                    company.CNAME = ((T_HR_POST)lookup.SelectedObj).T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;

                    lkCompany.DataContext = company;

                    T_HR_DEPARTMENT depart = new T_HR_DEPARTMENT();
                    depart.DEPARTMENTID = ((T_HR_POST)lookup.SelectedObj).T_HR_DEPARTMENT.DEPARTMENTID;
                    depart.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                    depart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = ((T_HR_POST)lookup.SelectedObj).T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

                    lkDepartment.DataContext = depart;

                    lkPost.DataContext = lookup.SelectedObj;
                } 

            };
            lookup.Show();
        }

        void ServiceClient_FindSysPermissionByTypeCompleted(object sender, FindSysPermissionByTypeCompletedEventArgs e)
        {
            cbxPerm.ItemsSource = null;

            if (e.Result != null)
            {
                //绑定权限名称
                List<T_SYS_PERMISSION> ents = e.Result.ToList();
                cbxPerm.ItemsSource = ents;
                cbxPerm.DisplayMemberPath = "PERMISSIONNAME";
                foreach (var item in cbxPerm.Items)
                {
                    T_SYS_PERMISSION tmpPerm = item as T_SYS_PERMISSION;
                    if (tmpPerm != null && CustPerm != null && CustPerm.T_SYS_PERMISSION != null
                        && tmpPerm.PERMISSIONID == this.CustPerm.T_SYS_PERMISSION.PERMISSIONID)
                    {
                        cbxPerm.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ENTITYMENUCUSTOMPERM");
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
                    AddToClose();
                    break;
                case "1":
                    Close();
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
                Title = "保存并关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_close.png"
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
            //CustPerm.T_SYS_ENTITYMENU = cbxMenu.SelectedItem as T_SYS_ENTITYMENU;
            CustPerm.T_SYS_PERMISSION = cbxPerm.SelectedItem as T_SYS_PERMISSION;

            //绑定公司
            T_HR_COMPANY company = lkCompany.DataContext as T_HR_COMPANY;
            if (company == null || string.IsNullOrEmpty(company.COMPANYID) || string.IsNullOrEmpty(company.CNAME))
            {
                CustPerm.COMPANYID = "";
                CustPerm.COMPANYNAME = "";
            }
            else
            {
                CustPerm.COMPANYID = company.COMPANYID;
                CustPerm.COMPANYNAME = company.CNAME;
            }

            //绑定部门
            T_HR_DEPARTMENT depart = lkDepartment.DataContext as T_HR_DEPARTMENT;
            if (depart == null || depart.T_HR_DEPARTMENTDICTIONARY == null
                || string.IsNullOrEmpty(depart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME)
                || string.IsNullOrEmpty(depart.DEPARTMENTID))
            {
                CustPerm.DEPARTMENTID = "";
                CustPerm.DEPARTMENTNAME = "";
            }
            else
            {
                CustPerm.DEPARTMENTID = depart.DEPARTMENTID;
                CustPerm.DEPARTMENTNAME = depart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            }


            //绑定岗位
            T_HR_POST post = lkPost.DataContext as T_HR_POST;
            if (post == null || post.T_HR_POSTDICTIONARY == null
                || string.IsNullOrEmpty(post.T_HR_POSTDICTIONARY.POSTNAME)
                || string.IsNullOrEmpty(post.POSTID))
            {
                CustPerm.POSTID = "";
                CustPerm.POSTNAME = "";
            }
            else
            {
                CustPerm.POSTID = post.POSTID;
                CustPerm.POSTNAME = post.T_HR_POSTDICTIONARY.POSTNAME;
            }

            if (FormTypes.New == this.FormType)
            {
                CustPerm.CREATEDATE = System.DateTime.Now;
                ///TODO增加修改人
                //CustPerm.CREATEUSER = Common.CurrentLoginUserInfo.sysuserID;
                ServiceClient.EntityMenuCustomPermAddAsync(this.CustPerm);
            }
            else
            {
                CustPerm.UPDATEDATE = System.DateTime.Now;
                ///TODO增加修改人
                //CustPerm.UPDATEUSER = Common.CurrentLoginUserInfo.sysuserID;
                ServiceClient.EntityMenuCustomPermUpdateAsync(this.CustPerm);
            }
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
        }

        public void Close()
        {
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
        }

        private void lkParentMenu_FindClick(object sender, EventArgs e)
        {
            MenuLookupForm lookup = new MenuLookupForm("");
            lookup.SelectedClick += (obj, ev) =>
            {
                lkParentMenu.DataContext = lookup.SelectedObj;
            };
            lookup.Show();
        }
    }
}
