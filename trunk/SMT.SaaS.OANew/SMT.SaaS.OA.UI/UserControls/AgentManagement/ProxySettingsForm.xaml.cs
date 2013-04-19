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
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SaaS.OA.UI.Class;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.FlowDesignerWS;

namespace SMT.SaaS.OA.UI.UserControls.AgentManagement
{
    public partial class ProxySettingsForm : BaseForm, IClient, IEntityEditor
    {

        #region 全局变量
        private T_OA_AGENTSET agentSetInfo;

        public T_OA_AGENTSET AgentSetInfo
        {
            get { return agentSetInfo; }
            set { this.DataContext = value; agentSetInfo = value; }
        }
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();
        private ObservableCollection<string> Party = new ObservableCollection<string>();
        private FormTypes actions;//操作类型
        private SmtOACommonOfficeClient SoaChannel;
        private PersonnelServiceClient client;
        private PermissionServiceClient PermissionServiceWcf;
        private ServiceClient FlowDesigner;
        private string AgentSetID = "";
        private string UserId = string.Empty;
        private SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeepost;
        private List<FLOW_MODELDEFINE_T> ModelDefineList;
        public event EventHandler InitModelCode;
        private FLOW_MODELDEFINE_T MODELDEFINE;
        #endregion

        #region 查看申请构造
        public ProxySettingsForm(FormTypes action, string AgentSetIDs)
        {
            InitializeComponent();
            this.actions = action;
            this.AgentSetID = AgentSetIDs;
            //InitEvent();
            //InitData();
            this.Loaded += new RoutedEventHandler(ProxySettingsForm_Loaded);
        }

        void ProxySettingsForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            InitData();
        }
        #endregion

        #region InitData
        private void InitData()
        {
            if (actions == FormTypes.New)
            {
                AgentSetInfo = new T_OA_AGENTSET();
                //this.cbSYSTEMTYPE.SelectedIndex = 0;
                client.GetEmployeeDetailByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);//获取当期用户信息
            }
            else
            {
                if (string.IsNullOrEmpty(AgentSetID))
                {
                    this.DataContext = AgentSetInfo;
                }
                else
                {
                    SoaChannel.GetAgentSetBysIdAsync(AgentSetID);
                }
            }
            if (actions == FormTypes.Browse)
            {
                this.cbSYSTEMTYPE.IsEnabled = false;
                this.txtEMPLOYEENAME.IsEnabled = false;
            }
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            SoaChannel = new SmtOACommonOfficeClient();
            client = new PersonnelServiceClient();
            FlowDesigner = new ServiceClient();//获取模块
            PermissionServiceWcf = new PermissionServiceClient();
            InitModelCode += new EventHandler(ModuleDefinitionControl_InitModelCode);//获取模块
            FlowDesigner.GetModelNameInfosComboxAsync();
            PermissionServiceWcf.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
            client.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(client_GetEmployeeDetailByIDCompleted);
            client.GetEmployeeByIDsCompleted += new EventHandler<GetEmployeeByIDsCompletedEventArgs>(client_GetEmployeeByIDsCompleted);
            SoaChannel.GetAgentSetBysIdCompleted += new EventHandler<GetAgentSetBysIdCompletedEventArgs>(SoaChannel_GetAgentSetBysIdCompleted);
            SoaChannel.UpdateAgentSetCompleted += new EventHandler<UpdateAgentSetCompletedEventArgs>(SoaChannel_UpdateAgentSetCompleted);
            SoaChannel.AgentSetAddCompleted += new EventHandler<AgentSetAddCompletedEventArgs>(SoaChannel_AgentSetAddCompleted);
            FlowDesigner.GetModelNameInfosComboxCompleted += new EventHandler<GetModelNameInfosComboxCompletedEventArgs>(GetModelNameInfosComboxCompleted);//获取模块
            PermissionServiceWcf.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(GetSysDictionaryByCategoryCompleted);
        }

        void client_GetEmployeeByIDsCompleted(object sender, GetEmployeeByIDsCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        vemployeeObj = e.Result.ToList();

                        if (vemployeeObj == null)
                            return;
                        var objc = from a in vemployeeObj
                                   where a.EMPLOYEEID == AgentSetInfo.USERID
                                   select a.EMPLOYEECNAME;
                        if (objc.Count() > 0)//如果数据存在
                        {
                            UserId = AgentSetInfo.USERID;
                            this.txtEMPLOYEENAME.Text = objc.FirstOrDefault();
                        }
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        void SoaChannel_GetAgentSetBysIdCompleted(object sender, GetAgentSetBysIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        AgentSetInfo = e.Result;
                        this.DataContext = AgentSetInfo;
                        if (!string.IsNullOrEmpty(AgentSetInfo.SYSCODE.ToString()))//系统代码
                        {
                            foreach (T_SYS_DICTIONARY Region in cbSYSTEMTYPE.Items)
                            {
                                if (Region.DICTIONARYVALUE == int.Parse(AgentSetInfo.SYSCODE))
                                {
                                    cbSYSTEMTYPE.SelectedItem = Region;
                                    break;
                                }
                            }
                        }
                        //if (!string.IsNullOrEmpty(AgentSetInfo.MODELCODE.ToString()))//模块代码
                        //{
                        //    foreach (FLOW_MODELDEFINE_T Region in cbModelCode.Items)
                        //    {
                        //        if (Region.MODELCODE == AgentSetInfo.MODELCODE)
                        //        {
                        //            ModelDefineList.Add(Region);
                        //            cbModelCode.SelectedItem = Region;
                        //            break;
                        //        }
                        //    }
                        //}
                        FlowDesigner.GetModelNameInfosComboxAsync();
                        //T_OA_AGENTSET PARENTMODELCODE = cbModelCode.ItemsSource.Cast<T_OA_AGENTSET>().Where(a => a.MODELCODE == MODELDEFINE.MODELCODE).ToList().First();
                        //cbModelCode.SelectedItem = PARENTMODELCODE;
                        txtPostId.Text = Utility.GetPostName(AgentSetInfo.AGENTPOSTID);//岗位ID
                        txtCompanyId.Text = Utility.GetCompanyName(AgentSetInfo.OWNERCOMPANYID);
                        txtDepartmentId.Text = Utility.GetDepartmentName(AgentSetInfo.OWNERDEPARTMENTID);

                        Party.Add(AgentSetInfo.USERID);
                        client.GetEmployeeByIDsAsync(Party);
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 获取当前员工的信息
        void client_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeepost = e.Result;
                GetAllPost(e.Result);
            }
        }

        private void GetAllPost(SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST ent)//获取当前员工、所属公司、所属部门
        {
            if (ent != null && ent.EMPLOYEEPOSTS != null)
            {
                txtEMPLOYEENAME.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME; //员工姓名
                txtCompanyId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;//公司
                txtDepartmentId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;//部门
            }
        }
        #endregion

        #region 修改Completed
        void SoaChannel_UpdateAgentSetCompleted(object sender, UpdateAgentSetCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        return;
                    }
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "PROXYSETTINGS"));
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 新增Complete
        void SoaChannel_AgentSetAddCompleted(object sender, AgentSetAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        return;
                    }
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "PROXYSETTINGS"));
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                    else
                    {
                        actions = FormTypes.Edit;
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            try
            {
                if (Check())
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);//点击保存后显示进度条
                    //T_SYS_DICTIONARY StrSYSTEMTYPE = cbSYSTEMTYPE.SelectedItem as T_SYS_DICTIONARY;
                    agentSetInfo.SYSCODE = (cbSYSTEMTYPE.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE.ToString(); //系统代码
                    //agentSetInfo.SYSCODE = StrSYSTEMTYPE.DICTIONARYNAME.ToString();//系统代码
                    //agentSetInfo.MODELCODE = txtAGENTMODULE.Text;//代理模块
                    agentSetInfo.MODELCODE = (cbModelCode.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE; //代理模块
                    agentSetInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                    agentSetInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户名
                    agentSetInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                    agentSetInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                    agentSetInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                    agentSetInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                    agentSetInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                    agentSetInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                    if (actions == FormTypes.New)
                    {
                        agentSetInfo.AGENTSETID = System.Guid.NewGuid().ToString();
                        agentSetInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人
                        agentSetInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                        if (employeepost != null)
                        {
                            agentSetInfo.USERID = employeepost.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEEID;//员工ID
                        }
                        SoaChannel.AgentSetAddAsync(agentSetInfo);
                    }
                    else if (actions == FormTypes.Edit)
                    {
                        agentSetInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        agentSetInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                        SoaChannel.UpdateAgentSetAsync(agentSetInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.ToString()));
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
            if (this.cbModelCode.SelectedIndex <= 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "AGENTMODULE"));
                return false;
            }

            if (this.cbSYSTEMTYPE.SelectedIndex < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SYSTEMCODE"));
                return false;
            }

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            return true;
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region LayoutRoot_SizeChanged
        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "PROXYSETTINGS");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "PROXYSETTINGS");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "PROXYSETTINGS");
            }
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
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
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
            if (actions != FormTypes.Browse && actions != FormTypes.Audit)
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
        private void Close()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region 岗位选择
        private void lkLicenseBorrow_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = ent.FirstOrDefault();

                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位
                    agentSetInfo.AGENTPOSTID = postid;
                    this.txtPostId.Text = postName;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        #region 获取系统、模块代码
        void GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)//获取系统代码
        {
            if (e.Result != null)
            {
                cbSYSTEMTYPE.ItemsSource = e.Result;
                cbSYSTEMTYPE.DisplayMemberPath = "DICTIONARYNAME";
                cbSYSTEMTYPE.SelectedIndex = 0;

                InitModelCode(sender, null);
            }
        }

        void ModuleDefinitionControl_InitModelCode(object sender, EventArgs e)//获取系统代码
        {
            if (cbSYSTEMTYPE.ItemsSource != null)
            {
                BindModelCode(cbSYSTEMTYPE.SelectedItem as T_SYS_DICTIONARY);
            }
        }

        void GetModelNameInfosComboxCompleted(object sender, GetModelNameInfosComboxCompletedEventArgs e)//获取模块代码
        {
            if (e.Result != null)
            {
                ModelDefineList = e.Result.ToList();
                MODELDEFINE = ModelDefineList.FirstOrDefault();

                InitModelCode(sender, null);
            }
        }

        void BindModelCode(T_SYS_DICTIONARY DICTIONARY)//绑定模块代码
        {
            cbModelCode.ItemsSource = null;
            if (AgentSetInfo != null && DICTIONARY != null && ModelDefineList != null)
            {
                List<FLOW_MODELDEFINE_T> tmpModelDefine = ModelDefineList.Where(c => c.SYSTEMCODE == ((T_SYS_DICTIONARY)cbSYSTEMTYPE.SelectedItem).SYSTEMCODE).ToList();
                FLOW_MODELDEFINE_T tmp = new FLOW_MODELDEFINE_T();
                tmp.DESCRIPTION = Utility.GetResourceStr("PLEASESELECTL");
                tmpModelDefine.Insert(0, tmp);

                if (tmpModelDefine.Count > 0)
                {
                    cbModelCode.ItemsSource = tmpModelDefine;
                    cbModelCode.DisplayMemberPath = "DESCRIPTION";
                    cbModelCode.SelectedIndex = 0;
                }
            }
        }

        private void cbSYSTEMTYPE_SelectionChanged(object sender, SelectionChangedEventArgs e)//根据系统代码获取对应的模块
        {
            BindModelCode(cbSYSTEMTYPE.SelectedItem as T_SYS_DICTIONARY);
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            PermissionServiceWcf.DoClose();
            client.DoClose();
            SoaChannel.DoClose();
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
