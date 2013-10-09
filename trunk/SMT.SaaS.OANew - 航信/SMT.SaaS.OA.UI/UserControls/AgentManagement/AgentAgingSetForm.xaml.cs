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
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.UserControls.AgentManagement
{
    public partial class AgentAgingSetForm : BaseForm, IClient, IEntityEditor
    {
        #region 全局变量
        private T_OA_AGENTDATESET agentSetInfo;

        public T_OA_AGENTDATESET AgentSetInfo
        {
            get { return agentSetInfo; }
            set { this.DataContext = value; agentSetInfo = value; }
        }
        private List<FLOW_MODELDEFINE_T> ModelDefineList;
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
        public event EventHandler InitModelCode;
        private FLOW_MODELDEFINE_T MODELDEFINE;
        private SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeepost;
        #endregion

        #region 使用代理时效设置构造
        public AgentAgingSetForm(FormTypes action, string AgentSetIDs)
        {
            InitializeComponent();
            this.actions = action;
            this.AgentSetID = AgentSetIDs;
            this.Loaded += new RoutedEventHandler(AgentAgingSetForm_Loaded);
        }

        void AgentAgingSetForm_Loaded(object sender, RoutedEventArgs e)
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
                AgentSetInfo = new T_OA_AGENTDATESET();
                this.dEFFECTDATE.Text = DateTime.Now.ToShortDateString();//生效日期
                this.dPLANEXPIRATIONDATE.Text = DateTime.Now.ToShortDateString();//计划失效日期
                this.dINVALIDDATE.Text = string.Empty;//失效日期
                this.dINVALIDDATE.IsEnabled = false;
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
                    SoaChannel.GetAgentDataSetBysIdAsync(AgentSetID);
                }
                this.dINVALIDDATE.IsEnabled = true;
                cbModelCode.Visibility = Visibility.Visible;//隐藏ModeCode的下拉框
                txtAGENTMODULE.Visibility = Visibility.Visible;//显示ModeCode的文本框
            }
            if (actions == FormTypes.Browse)
            {
                this.dEFFECTDATE.IsEnabled = false;
                this.dINVALIDDATE.IsEnabled = false;
                this.dPLANEXPIRATIONDATE.IsEnabled = false;
                this.txtAGENTMODULE.IsEnabled = false;
                this.cbSYSTEMTYPE.IsEnabled = false;
            }
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            SoaChannel = new SmtOACommonOfficeClient();
            client = new PersonnelServiceClient();
            PermissionServiceWcf = new PermissionServiceClient();
            FlowDesigner = new ServiceClient();//获取模块
            
            
            InitModelCode += new EventHandler(ModuleDefinitionControl_InitModelCode);//获取模块
            client.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(client_GetEmployeeDetailByIDCompleted);
            client.GetEmployeeByIDsCompleted += new EventHandler<GetEmployeeByIDsCompletedEventArgs>(client_GetEmployeeByIDsCompleted);
            SoaChannel.GetAgentDataSetBysIdCompleted += new EventHandler<GetAgentDataSetBysIdCompletedEventArgs>(SoaChannel_GetAgentDataSetBysIdCompleted);
            SoaChannel.UpdateAgentDataSetCompleted += new EventHandler<UpdateAgentDataSetCompletedEventArgs>(SoaChannel_UpdateAgentDataSetCompleted);
            SoaChannel.AgentDataSetAddCompleted += new EventHandler<AgentDataSetAddCompletedEventArgs>(SoaChannel_AgentDataSetAddCompleted);
            FlowDesigner.GetModelNameInfosComboxCompleted += new EventHandler<GetModelNameInfosComboxCompletedEventArgs>(GetModelNameInfosComboxCompleted);//获取模块
            PermissionServiceWcf.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(GetSysDictionaryByCategoryCompleted);
            PermissionServiceWcf.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
            FlowDesigner.GetModelNameInfosComboxAsync();
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

        void SoaChannel_GetAgentDataSetBysIdCompleted(object sender, GetAgentDataSetBysIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        AgentSetInfo = e.Result;
                        this.DataContext = AgentSetInfo;
                        //将模块代码、生效时间、计划生效时间控件设为只读
                        this.dEFFECTDATE.IsEnabled = false;
                        this.dPLANEXPIRATIONDATE.IsEnabled = false;
                        this.txtAGENTMODULE.IsEnabled = false;
                        //如果代理已经关闭，将失效时间控件设为只读
                        if (AgentSetInfo.EXPIRATIONDATE != null)
                        {
                            this.dINVALIDDATE.IsEnabled = false;
                        }

                        txtAGENTMODULE.Text = Utility.GetMododuelName(AgentSetInfo.MODELCODE);
                        dEFFECTDATE.Text = AgentSetInfo.EFFECTIVEDATE.ToShortDateString();
                        dINVALIDDATE.Text = AgentSetInfo.EXPIRATIONDATE.ToString();
                        dPLANEXPIRATIONDATE.Text = AgentSetInfo.PLANEXPIRATIONDATE.ToShortDateString();

                        Party.Add(AgentSetInfo.USERID);
                        client.GetEmployeeByIDsAsync(Party);

                        client.GetEmployeeDetailByIDAsync(AgentSetInfo.USERID);
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
                string PostName = "";
                string DepartmentName = "";
                string CompanyName = "";
                string StrName = "";
                PostName = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                DepartmentName = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                CompanyName = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                StrName = ent.T_HR_EMPLOYEE.EMPLOYEECNAME + "[" + PostName + "-" + DepartmentName + "-" + CompanyName + "]";
                txtEMPLOYEENAME.Text = StrName;
                ToolTipService.SetToolTip(txtEMPLOYEENAME, StrName);
            }
        }
        #endregion

        #region 修改Completed
        void SoaChannel_UpdateAgentDataSetCompleted(object sender, UpdateAgentDataSetCompletedEventArgs e)
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
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CLOSEAGENTSUCCESS"));
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
        void SoaChannel_AgentDataSetAddCompleted(object sender, AgentDataSetAddCompletedEventArgs e)
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
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "AGENTAGINGSET"));
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                    else
                    {
                        this.dINVALIDDATE.IsEnabled = true;
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

                    if (actions == FormTypes.New)
                    {
                        agentSetInfo.AGENTDATESETID = System.Guid.NewGuid().ToString();
                        if (!string.IsNullOrEmpty(dEFFECTDATE.Text))//生效日期
                        {
                            agentSetInfo.EFFECTIVEDATE = Convert.ToDateTime(dEFFECTDATE.Text);
                        }
                        if (!string.IsNullOrEmpty(dEFFECTDATE.Text))
                        {
                            agentSetInfo.PLANEXPIRATIONDATE = Convert.ToDateTime(dPLANEXPIRATIONDATE.Text);//计划失效日期
                        }
                        agentSetInfo.USERID = Common.CurrentLoginUserInfo.EmployeeID;
                        agentSetInfo.MODELCODE = (cbModelCode.SelectedItem as FLOW_MODELDEFINE_T).MODELCODE; //代理模块
                        agentSetInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                        agentSetInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                        agentSetInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                        agentSetInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                        agentSetInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                        agentSetInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                        agentSetInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                        agentSetInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                        agentSetInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属人ID
                        agentSetInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//报销人ID(出差人)
                        agentSetInfo.CREATEDATE = DateTime.Now;//创建时间

                        SoaChannel.AgentDataSetAddAsync(agentSetInfo);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(dINVALIDDATE.Text))
                        {
                            agentSetInfo.EXPIRATIONDATE = Convert.ToDateTime(dINVALIDDATE.Text);//失效时间
                        }
                        agentSetInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        agentSetInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        SoaChannel.UpdateAgentDataSetAsync(agentSetInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.ToString()));
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
            string SxData = string.Empty;//生效日期
            string EndDt = string.Empty;//失效时间
            string JhEndDt = string.Empty;//计划失效日期
            SxData = dEFFECTDATE.Text.ToString();
            EndDt = dINVALIDDATE.Text.ToString();
            JhEndDt = dPLANEXPIRATIONDATE.Text.ToString();
            DateTime DtEnd = new DateTime();
            DateTime SxDataTiem = new DateTime();
            DateTime JhDataTime = new DateTime();

            if (actions != FormTypes.New)
            {
                if (dPLANEXPIRATIONDATE.IsEnabled == true)//控件只有在可输入的状态下才进行判断
                {
                    if (!string.IsNullOrEmpty(this.dPLANEXPIRATIONDATE.Text))
                    {
                        JhDataTime = System.Convert.ToDateTime(JhEndDt);
                        if (JhDataTime < DateTime.Now)//计划失效日期不能小于当前日期
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANNOTSMOLLERTHANTHECURRENTTIME", "PLANEXPIRATIONDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            RefreshUI(RefreshedTypes.HideProgressBar);
                            return false;
                        }
                    }
                }
                if (dINVALIDDATE.IsEnabled == true)
                {
                    if (!string.IsNullOrEmpty(this.dINVALIDDATE.Text))//失效日期不能小于当前日期
                    {
                        DtEnd = System.Convert.ToDateTime(EndDt);
                        if (DtEnd < DateTime.Now)
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANNOTSMOLLERTHANTHECURRENTTIME", "INVALIDDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            RefreshUI(RefreshedTypes.HideProgressBar);
                            return false;
                        }
                    }
                    if (!string.IsNullOrEmpty(this.dINVALIDDATE.Text))//失效日期如果大于计划失效日期，表示已过期，不能关闭
                    {
                        DtEnd = System.Convert.ToDateTime(EndDt);
                        SxDataTiem = System.Convert.ToDateTime(JhEndDt);
                        if (DtEnd > SxDataTiem)
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("HASEXPIREDCANNOTBECLOSED", "INVALIDDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            RefreshUI(RefreshedTypes.HideProgressBar);
                            return false;
                        }
                    }
                    if (!string.IsNullOrEmpty(this.dINVALIDDATE.SelectedDate.ToString()))
                    {
                        EndDt = this.dINVALIDDATE.SelectedDate.Value.ToString("d");//失效日期不能为空
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "INVALIDDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        this.dINVALIDDATE.Focus();
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        return false;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.dEFFECTDATE.Text))//生效日期
                {
                    SxDataTiem = System.Convert.ToDateTime(SxData);
                }
                if (!string.IsNullOrEmpty(this.dPLANEXPIRATIONDATE.Text))//计划失效日期
                {
                    JhDataTime = System.Convert.ToDateTime(JhEndDt);
                }
                if (dEFFECTDATE.IsEnabled == true && dPLANEXPIRATIONDATE.IsEnabled == true)//控件只有在可输入的状态下才进行判断
                {
                    if (!string.IsNullOrEmpty(this.dEFFECTDATE.SelectedDate.ToString()))//生效日期
                    {
                        SxData = this.dEFFECTDATE.SelectedDate.Value.ToString("d");
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "dEFFECTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        this.dEFFECTDATE.Focus();
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        return false;
                    }
                    if (!string.IsNullOrEmpty(this.dPLANEXPIRATIONDATE.SelectedDate.ToString()))//计划失效日期非空验证
                    {
                        JhEndDt = this.dPLANEXPIRATIONDATE.SelectedDate.Value.ToString("d");
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "PLANEXPIRATIONDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        this.dPLANEXPIRATIONDATE.Focus();
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        return false;
                    }

                    if (!string.IsNullOrEmpty(this.dPLANEXPIRATIONDATE.Text))
                    {
                        JhDataTime = System.Convert.ToDateTime(JhEndDt);
                        if (JhDataTime < DateTime.Now)//计划失效日期不能小于当前日期
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANNOTSMOLLERTHANTHECURRENTTIME", "PLANEXPIRATIONDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            RefreshUI(RefreshedTypes.HideProgressBar);
                            return false;
                        }
                    }
                }
                if (cbModelCode.SelectedIndex <= 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "AGENTMODULE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
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
                return Utility.GetResourceStr("ADDTITLE", "AGENTAGINGSET");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "AGENTAGINGSET");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "AGENTAGINGSET");
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

        #region IForm 成员

        public void ClosedWCFClient()
        {
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
    }
}
