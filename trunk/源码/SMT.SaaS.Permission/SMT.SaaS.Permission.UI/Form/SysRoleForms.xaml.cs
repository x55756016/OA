using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.OrganizationWS;
using System.Windows.Browser;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SysRoleForms : UserControl, IEntityEditor
    {
        #region 初始化数据
        //private string saveType = "0";       //保存方式 0:添加 1:关闭

        OrganizationServiceClient Organ = new OrganizationServiceClient();
        private T_SYS_ROLE sysRole;
        private List<T_SYS_DICTIONARY> tmpDicts;
        private string tmpDictionaryValue = ""; //字典值
        private string tmpRoleId = "";  //角色ID
        string StrCompanyID = ""; //公司ID
        string StrDepartmentID = ""; //部门ID
        RefreshedTypes action;
        public T_SYS_ROLE SysRole
        {
            get { return sysRole; }
            set
            {
                sysRole = value;
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
        //protected SysRoleManagementServiceClient SysRoleClient = new SysRoleManagementServiceClient();

        public SysRoleForms(FormTypes type)
        {
            InitializeComponent();
            ////TODO:加操作用户信息
            FormType = type;
            InitParas("");
        }


        public SysRoleForms(FormTypes type, string roleID)
        {
            InitializeComponent();
            FormType = type;
            InitParas(roleID);
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
            txtCompanyName.IsEnabled = false;
            btnLookUpPartyb.IsEnabled = false;
            txtRoleName.IsEnabled = false;
            //txtRemark.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            cbxSystemType.IsEnabled = false;
        }


        private void InitParas(string roleID)
        {

            ServiceClient = new PermissionServiceClient();
            ServiceClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(ServiceClient_GetSysDictionaryByCategoryCompleted);
            ServiceClient.SysRoleInfoAddCompleted += new EventHandler<SysRoleInfoAddCompletedEventArgs>(SysRoleClient_SysRoleInfoAddCompleted);
            //ServiceClient.SysRoleInfoUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(SysRoleClient_SysRoleInfoUpdateCompleted);
            ServiceClient.SysRoleInfoUpdateCompleted += new EventHandler<SysRoleInfoUpdateCompletedEventArgs>(ServiceClient_SysRoleInfoUpdateCompleted);
            ServiceClient.GetSysRoleSingleInfoByIdCompleted += new EventHandler<GetSysRoleSingleInfoByIdCompletedEventArgs>(SysRoleClient_GetSysRoleSingleInfoByIdCompleted);

            if (!string.IsNullOrEmpty(roleID))
            {
                tmpRoleId = roleID;
                //ServiceClient.GetSysRoleByIDAsync(roleID);

            }
            //绑定系统类型
            ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");

            if (FormType == FormTypes.New)
            {
                SysRole = new T_SYS_ROLE();
                this.txtCompanyName.Text = string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].CompanyName) ? "" : Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;//公司名称
                StrCompanyID = string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID) ? "" : Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//公司ID
                this.txtDepartmentName.Text = string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName) ? "" : Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;//部门名称
                this.StrDepartmentID = string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID) ? "" : Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//部门ID
                SysRole.ROLEID = Guid.NewGuid().ToString();
                //GetCompanyNameByCompanyID(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            }

        }

        void ServiceClient_SysRoleInfoUpdateCompleted(object sender, SysRoleInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.StrResult==null ||e.StrResult == "")
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "修改角色信息成功", Utility.GetResourceStr("CONFIRMBUTTON"));

                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.StrResult.ToString()));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                }
            }
        }
        #endregion

        #region 获取公司信息


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
                    T_HR_COMPANY company = new T_HR_COMPANY();
                    company = e.Result;
                    StrCompanyID = company.COMPANYID;
                    txtCompanyName.Text = company.CNAME;

                }
            }
        }

        /// <summary>
        /// 选择公司按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    StrCompanyID = companyInfo.ObjectID;
                    txtCompanyName.Text = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
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
                    T_HR_DEPARTMENT department = new T_HR_DEPARTMENT();
                    department = e.Result;
                    StrDepartmentID = department.DEPARTMENTID;
                    txtDepartmentName.Text = department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

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
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }



        #endregion

        #region 添加角色信息


        /// <summary>
        /// 添加角色信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SysRoleClient_SysRoleInfoAddCompleted(object sender, SysRoleInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSEDSYSROLE"));
                    FormType = FormTypes.Edit;
                    RefreshUI(action);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Result.ToString());
                    return;
                }
            }
        }

        #endregion

        #region 修改角色信息
        /// <summary>
        /// 修改系统角色信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="?"></param>
        void SysRoleClient_SysRoleInfoUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "修改角色信息成功", Utility.GetResourceStr("CONFIRMBUTTON"));

                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }
        #endregion

        #region 获取某一系统角色信息
        /// <summary>
        /// 获取某一角色的详细信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SysRoleClient_GetSysRoleSingleInfoByIdCompleted(object sender, GetSysRoleSingleInfoByIdCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                sysRole = e.Result as T_SYS_ROLE;
                tmpDictionaryValue = sysRole.SYSTEMTYPE;
                //string aa = GetDictionaryInfo(tmpDictionaryValue);
                this.cbxSystemType.SelectedItem = GetDictionaryInfo(tmpDictionaryValue);
                this.txtRemark.Text = sysRole.REMARK != null ? sysRole.REMARK : "";
                if (!string.IsNullOrEmpty(sysRole.OWNERCOMPANYID))
                {
                    GetCompanyNameByCompanyID(sysRole.OWNERCOMPANYID);
                    GetDepartmentNameByDepartmentID(sysRole.OWNERDEPARTMENTID);
                }
                this.txtRoleName.Text = sysRole.ROLENAME;
                //this.txtRemark.Text = HttpUtility.HtmlDecode(sysRole.REMARK);
                // this.txtRemark.Text = sysRole.REMARK;
            }
        }
        #endregion

        #region 获取字典信息

        void ServiceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            //绑定系统类型
            if (e.Result != null)
            {
                List<T_SYS_DICTIONARY> dicts = e.Result.ToList();
                tmpDicts = dicts;
                cbxSystemType.ItemsSource = dicts;
                cbxSystemType.DisplayMemberPath = "DICTIONARYNAME";
                if (FormType == FormTypes.Edit || formType == FormTypes.Browse)
                {
                    ServiceClient.GetSysRoleSingleInfoByIdAsync(tmpRoleId);
                }
                //if(tmpDictionaryValue)
            }
        }

        /// <summary>
        /// 获取选取的字典信息
        /// </summary>
        /// <param name="DictionaryValue"></param>
        /// <returns></returns>
        private T_SYS_DICTIONARY GetDictionaryInfo(string DictionaryValue)
        {
            string StrReturn = "";
            T_SYS_DICTIONARY tmpDict = new T_SYS_DICTIONARY();

            foreach (T_SYS_DICTIONARY dictT in tmpDicts)
            {
                //if(aa.(cbxSystemType.Items))
                if (dictT.DICTIONARYVALUE.ToString() == DictionaryValue)
                {
                    StrReturn = dictT.DICTIONARYNAME;
                    //DictionaryValue.IndexOf(tmpDicts);
                    tmpDict = dictT;
                    break;
                }
            }
            return tmpDict;
        }
        #endregion

        #region LayoutRoot_BindingValidationError


        private void LayoutRoot_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                // btnSave.IsEnabled = false;
                e.Handled = true;
            }
            else
            {
                // btnSave.IsEnabled = true;
            }
        }

        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SYSROLE");
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
                    action = RefreshedTypes.All;
                    AddToClose();

                    break;
                case "1":
                    action = RefreshedTypes.CloseAndReloadData;
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
            if (FormType == FormTypes.New || FormType == FormTypes.Edit)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1",
                    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_SaveClose.png"
                };

                items.Add(item);

                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("SAVE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_SAVE.png"
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

        #region 添加/修改按钮
        public void AddToClose()
        {
            T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SysRole.SYSTEMTYPE = dict.DICTIONARYVALUE.ToString();
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SYSTEMTYPE") + Utility.GetResourceStr("NOTALLOWNULL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "请选择系统类型！",
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            string StrName = "";
            string StrRemark = "";
            bool IsNull = true;
            string StrReturn = "";
            StrName = this.txtRoleName.Text.ToString().Trim();
            StrRemark = this.txtRemark.Text.ToString().Trim();
            if (string.IsNullOrEmpty(StrName))
            {
                IsNull = false;
                StrReturn += "角色名称不能为空!";
            }
            if (!IsNull)
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ROLENAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "角色名称不能为空！",
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.txtRoleName.Focus();
                return;

            }

            if (FormTypes.New == this.FormType)
            {

                sysRole.ROLENAME = StrName;
                sysRole.REMARK = StrRemark;//HttpUtility.HtmlEncode(StrRemark);
                SysRole.CREATEDATE = System.DateTime.Now;
                sysRole.OWNERCOMPANYID = StrCompanyID;
                sysRole.OWNERDEPARTMENTID = StrDepartmentID;
                sysRole.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                sysRole.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                sysRole.CHECKSTATE = "0";
                sysRole.ISAUTHORY = "0";
                sysRole.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                sysRole.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                sysRole.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                sysRole.CREATEUSERNAME = Common.CurrentLoginUserInfo.UserName;
                
                SysRole.CREATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
                sysRole.UPDATEDATE = null;
                sysRole.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                sysRole.UPDATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
                ServiceClient.SysRoleInfoAddAsync(this.sysRole);
            }
            else
            {
                SysRole.UPDATEDATE = System.DateTime.Now;
                sysRole.UPDATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
                sysRole.ROLENAME = StrName;
                sysRole.REMARK = StrRemark;//HttpUtility.HtmlEncode(StrRemark);
                sysRole.UPDATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
                sysRole.OWNERCOMPANYID = StrCompanyID;
                sysRole.OWNERDEPARTMENTID = StrDepartmentID;
                sysRole.T_SYS_ROLEENTITYMENU = null;
                sysRole.T_SYS_USERROLE = null;
                sysRole.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                sysRole.UPDATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
                string Result = "";
                ServiceClient.SysRoleInfoUpdateAsync(this.sysRole, Result);
            }

        }
        #endregion

        public void Close()
        {
            //saveType = "1";
            RefreshUI(RefreshedTypes.Close);
        }
    }
}
        
        
