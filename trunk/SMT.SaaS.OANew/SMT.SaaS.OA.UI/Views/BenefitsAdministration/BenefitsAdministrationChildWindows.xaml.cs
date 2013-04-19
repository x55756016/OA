/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-18

** 修改人：刘锦

** 修改时间：2010-07-12

** 描述：

**    主要用于福利标准定义信息的资料录入

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Collections.ObjectModel;
using System.Windows.Data;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PersonnelWS;

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class BenefitsAdministrationChildWindows : BaseForm,IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        private SmtOADocumentAdminClient wssc;
        private PersonnelServiceClient client;
        private T_OA_WELFAREMASERT welfareInfo;
        private ObservableCollection<T_OA_WELFAREDETAIL> welfareDetailList = new ObservableCollection<T_OA_WELFAREDETAIL>();
        private T_OA_WELFAREDETAIL welfareDetail = new T_OA_WELFAREDETAIL();
        private FormTypes actions; //操作类型
        private string StrRadio = "1";
        private string StrPostId = null;
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private string checkState = ((int)CheckStates.WaittingApproval).ToString();
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private V_EMPLOYEEPOST employeepost;
        private string welfareInfoID = "";
        public T_OA_WELFAREMASERT InfoObj
        {
            get { return welfareInfo; }
            set
            {
                this.DataContext = value;
                welfareInfo = value;
            }
        }
        #endregion

        #region 构造函数
        public BenefitsAdministrationChildWindows(FormTypes action, string welfareInfoId)
        {
            InitializeComponent();
            this.actions = action;
            this.welfareInfoID = welfareInfoId;
            this.Loaded += (sender, args) =>
            {
                #region 原来的
                InitEvent();
                InitData();
                Utility.CbxItemBinders(cbWelfareID, "WELFAREPROID", "0");

                cobPostId.SelectedIndex = -1;
                cobPostLevle.SelectedIndex = -1;
                if (action == FormTypes.New)
                {
                    wssc.GetBenefitsAdministrationDetailsAsync(InfoObj.WELFAREPROID, InfoObj.COMPANYID, InfoObj.WELFAREID);//获取明细
                    client.GetEmployeeDetailByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);//获取当期用户信息
                    this.StartTime.Text = string.Empty;
                    this.EndTime.Text = string.Empty;
                    //this.audit.Visibility = Visibility.Collapsed;
                }
                if (action == FormTypes.Audit || action == FormTypes.Browse)
                {
                    SetButtonVisible();
                    ShieldedControl();
                }
                #endregion
            };
        }
        #endregion

        #region InitData
        private void InitData()
        {
            try
            {
                if (actions == FormTypes.New)
                {
                    InfoObj = new T_OA_WELFAREMASERT();
                    InfoObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                }
                else
                {
                    if (actions == FormTypes.Audit)
                    {
                        actionFlag = DataActionFlag.SubmitComplete;
                    }
                    wssc.GetWelfareByIdAsync(welfareInfoID);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region 屏蔽控件
        private void ShieldedControl()
        {
            cbWelfareID.IsEnabled = false;
            WelfareStandard.IsReadOnly = true;
            CompanyObject.IsEnabled = false;
            PostsObject.IsEnabled = false;
            txtNotes.IsReadOnly = true;
            cobPostId.IsEnabled = false;
            cobPostLevle.IsEnabled = false;
            RbtNo.IsEnabled = false;
            rbtYes.IsEnabled = false;
            StartTime.IsEnabled = false;
            textTell.IsReadOnly = true;
            txtRemark1.IsReadOnly = true;
            SearchBtn.IsEnabled = false;
        }
        #endregion

        #region DataGrid 数据绑定
        /// <summary>
        /// DataGrid数据绑定
        /// </summary>
        /// <param name="obj"></param>
        private void BindDataGrid(ObservableCollection<T_OA_WELFAREDETAIL> obj)
        {
            welfareDetailList = obj;
            if (welfareDetailList.Count > 0)
            {
                welfareDetailList.ForEach(item =>
                {
                    item.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(d_PropertyChanged);
                });
                NewDetail();
            }
            else
            {
                NewDetail();
            }
            this.DaGrs.ItemsSource = welfareDetailList;
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 数据初始化
        /// </summary>
        private void InitEvent()
        {
            wssc = new SmtOADocumentAdminClient();
            client = new PersonnelServiceClient();
            wssc.WelfareStandardAddCompleted += new EventHandler<WelfareStandardAddCompletedEventArgs>(wssc_WelfareStandardAddCompleted);//添加
            wssc.UpdateWelfareStandardCompleted += new EventHandler<UpdateWelfareStandardCompletedEventArgs>(wssc_UpdateWelfareStandardCompleted);//修改
            wssc.GetBenefitsAdministrationDetailsCompleted += new EventHandler<GetBenefitsAdministrationDetailsCompletedEventArgs>(wssc_GetBenefitsAdministrationDetailsCompleted);//查询标准明细
            wssc.GetWelfareByIdCompleted += new EventHandler<GetWelfareByIdCompletedEventArgs>(wssc_GetWelfareByIdCompleted);
            client.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(client_GetEmployeeDetailByIDCompleted);
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void wssc_GetWelfareByIdCompleted(object sender, GetWelfareByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DIDNOTFINDRELEVANT", "WELFARESTANDARD"));
                        return;
                    }
                    InfoObj = e.Result;
                    if (actions == FormTypes.Resubmit)//重新提交
                    {
                        InfoObj.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                    }
                    cbWelfareID.SelectedIndex = Convert.ToInt32(InfoObj.WELFAREPROID);//福利项目编号
                    WelfareStandard.Text = Convert.ToDecimal(welfareDetail.STANDARD).ToString();//标准  
                    if (InfoObj.REMARK != null)
                    {
                        txtNotes.Text = InfoObj.REMARK;//备注
                    }
                    textCreateUser.Text = InfoObj.CREATEUSERNAME.ToString();
                    textTell.Text = InfoObj.TEL;//联系电话
                    StartTime.Text = InfoObj.STARTDATE.ToString();//生效时间
                    GetCompanyNameByCompanyID(InfoObj.COMPANYID);//获取公司
                    wssc.GetBenefitsAdministrationDetailsAsync(InfoObj.WELFAREPROID, InfoObj.COMPANYID, InfoObj.WELFAREID);//获取明细
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        void wssc_GetBenefitsAdministrationDetailsCompleted(object sender, GetBenefitsAdministrationDetailsCompletedEventArgs e)//根据条件查询标准明细
        {
            try
            {
                if (e.Result != null)
                {
                    BindDataGrid(e.Result);
                }
                else
                {
                    BindDataGrid(null);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 获取当前员工的信息
        /// <summary>
        /// 获取当前员工的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeepost = e.Result;
                GetAllPost(e.Result);
            }
        }
        /// <summary>
        /// 获取当前员工、联系电话
        /// </summary>
        /// <param name="ent">员工信息实体</param>
        private void GetAllPost(V_EMPLOYEEPOST ent)
        {
            if (ent != null && ent.EMPLOYEEPOSTS != null)
            {
                textCreateUser.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME; //员工姓名
                if (ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.MOBILE != null)
                {
                    textTell.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.MOBILE;//手机号码
                }
            }
        }
        #endregion

        #region 修改时获取岗位
        /// <summary>
        /// 修改时获取岗位
        /// </summary>
        /// <param name="StrPostID">岗位ID</param>
        private void GetCompanyNameByPostID(string StrPostID)
        {
            SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient Organ = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();

            Organ.GetPostByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs>(Organ_GetPostByIdCompleted);
            Organ.GetPostByIdAsync(StrPostID);
        }

        void Organ_GetPostByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_POST company = new SMT.Saas.Tools.OrganizationWS.T_HR_POST();
                    company = e.Result;
                    PostsObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                    PostsObject.DataContext = company;
                }
            }
        }
        #endregion

        #region 修改时获取公司
        /// <summary>
        /// 修改时获取公司
        /// </summary>
        /// <param name="StrCompanyID">公司ID</param>
        private void GetCompanyNameByCompanyID(string StrCompanyID)
        {
            SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient Organ = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();

            Organ.GetCompanyByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyByIdCompletedEventArgs>(Organ_GetCompanyByIdCompleted);
            Organ.GetCompanyByIdAsync(StrCompanyID);
        }

        void Organ_GetCompanyByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                    company = e.Result;
                    CompanyObject.DisplayMemberPath = "CNAME";
                    CompanyObject.DataContext = company;
                }
            }
        }
        #endregion

        #region ReloadData LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Binding redoBind = new Binding();
            redoBind.Path = new PropertyPath("ISLEVEL");
            redoBind.Mode = BindingMode.TwoWay;
            BoolConvert bc = new BoolConvert();
            bc.IsYes = true;
            redoBind.Converter = bc;
            this.rbtYes.SetBinding(RadioButton.IsCheckedProperty, redoBind);

            Binding redoBindto = new Binding();
            redoBindto.Path = new PropertyPath("ISLEVEL");
            redoBindto.Mode = BindingMode.TwoWay;
            BoolConvert bcTo = new BoolConvert();
            bcTo.IsYes = false;
            redoBindto.Converter = bcTo;
            this.RbtNo.SetBinding(RadioButton.IsCheckedProperty, redoBindto);

            Binding postBind = new Binding();
            CompanyInfoConverter converterPost = new CompanyInfoConverter();
            postBind.Converter = converterPost;
            postBind.ConverterParameter = "Post";
            postBind.Path = new PropertyPath("POSTID");
            this.PostsObject.TxtLookUp.SetBinding(TextBox.TextProperty, postBind);
            PostsObject.IsEnabled = false;//隐藏岗位选择
        }
        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "WELFARESTANDARD");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "WELFARESTANDARD");
            }
            else if (actions == FormTypes.Audit)
            {
                return Utility.GetResourceStr("AUDIT", "WELFARESTANDARD");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "WELFARESTANDARD");
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

        #region 添加Completed
        void wssc_WelfareStandardAddCompleted(object sender, WelfareStandardAddCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                else
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        return;
                    }
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "WELFARESTANDARD"));
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                    else
                    {
                        actions = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 修改Completed
        void wssc_UpdateWelfareStandardCompleted(object sender, UpdateWelfareStandardCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                else
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        return;
                    }
                    if (e.UserState.ToString() == "Edit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "WELFARESTANDARD"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                        }
                    }
                    else if (e.UserState.ToString() == "Audit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "WELFARESTANDARD"));
                    }
                    else if (e.UserState.ToString() == "Submit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "WELFARESTANDARD"));
                    }
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 提交审核
        private void SubmitAuditToClose()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            try
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);//点击保存后显示进度条

                if (welfareInfo.COMPANYID == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "COMPANYNAME"));
                    this.CompanyObject.Focus();
                    RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                    return;
                }
                if (string.IsNullOrEmpty(StartTime.Text.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "AVAILABLEDATE"));
                    this.StartTime.Focus();
                    RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                    return;
                }
                if (string.IsNullOrEmpty(textTell.Text.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TELL"));
                    this.textTell.Focus();
                    RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                    return;
                }
                if (Convert.ToDateTime(StartTime.Text) <= DateTime.Now)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATECOMPARECURENTTIME", "AVAILABLEDATE"));
                    this.StartTime.Focus();
                    RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                    return;
                }
                if (welfareDetailList.Count <= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTADDEDPLEASEREENTER", "WelfareStandardsForDetails"));
                    RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                    return;
                }
                if (actions == FormTypes.New)
                {
                    welfareInfo.WELFAREID = System.Guid.NewGuid().ToString();
                    welfareInfo.REMARK = txtNotes.Text.ToString();//备注
                    welfareInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                    welfareInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                    welfareInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                    welfareInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//审批状态
                    welfareInfo.WELFAREPROID = this.cbWelfareID.SelectedIndex.ToString();//福利项目编号
                    welfareInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                    welfareInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    welfareInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                    welfareInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户名
                    welfareInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                    welfareInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                    welfareInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                    welfareInfo.TEL = textTell.Text.ToString();//联系电话
                    welfareInfo.STARTDATE = Convert.ToDateTime(StartTime.Text);//生效时间

                    wssc.WelfareStandardAddAsync(welfareInfo, welfareDetailList);
                }
                else
                {
                    welfareInfo.WELFAREPROID = cbWelfareID.SelectedIndex.ToString();//福利项目编号
                    welfareInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                    welfareInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    welfareInfo.REMARK = txtNotes.Text;//备注
                    welfareInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//审批状态
                    welfareInfo.STARTDATE = Convert.ToDateTime(StartTime.Text);//生效时间
                    welfareInfo.TEL = textTell.Text.ToString();//联系电话

                    wssc.UpdateWelfareStandardAsync(welfareInfo, welfareDetailList, "Edit");
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
            T_SYS_DICTIONARY Strpostlevel = cobPostId.SelectedItem as T_SYS_DICTIONARY;//岗位级别

            if (welfareInfo.COMPANYID == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "COMPANYNAME"));
                this.CompanyObject.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(StartTime.Text.ToString()))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "AVAILABLEDATE"));
                this.StartTime.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(textTell.Text.ToString()))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TELL"));
                this.textTell.Focus();
                return false;
            }
            if (Convert.ToDateTime(StartTime.Text) <= DateTime.Now)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTSMOLLERTHANTHECURRENTTIME", "AVAILABLEDATE"));
                this.StartTime.Focus();
                return false;
            }
            if (StrRadio == "1")
            {
                if (this.cobPostId.SelectedIndex == 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                    cobPostId.Focus();
                    return false;
                }
                if (this.cobPostLevle.SelectedIndex == 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                    cobPostLevle.Focus();
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(this.PostsObject.TxtLookUp.Text))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTNAME"));
                    this.PostsObject.Focus();
                    return false;
                }
            }

            if (string.IsNullOrEmpty(this.WelfareStandard.Text) && string.IsNullOrEmpty(this.txtRemark1.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MUSTCOMPLETEA", "PAYMENTAMOUNTANDPAYMENTOFGOODS"));
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
                RefreshUI(RefreshedTypes.ShowProgressBar);//关闭进度条动画
            }
            return true;
        }
        #endregion

        #region 获取公司ID
        private void CompanyObject_FindClick(object sender, EventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();

            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                CompanyObject.DataContext = ent;
                if (ent != null)
                {
                    welfareInfo.COMPANYID = ent.COMPANYID;
                    CompanyObject.DisplayMemberPath = "CNAME";
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 获取岗位ID
        private void PostsObject_FindClick(object sender, EventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();

            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_POST ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                //PostsObject.DataContext = ent;
                if (ent != null)
                {
                    welfareDetail.POSTID = ent.POSTID;
                    //PostsObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                    welfareDetail.POSTLEVELA = ent.T_HR_POSTDICTIONARY.POSTLEVEL.ToString();
                    StrPostId = ent.POSTID;
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region Redio岗位级别
        private void rbtYes_Click(object sender, RoutedEventArgs e)
        {
            this.rbtYes.IsChecked = true;
            this.RbtNo.IsChecked = false;

            PostsObject.IsEnabled = false;//隐藏岗位选择
            cobPostId.IsEnabled = true;//显示级别选择
            cobPostLevle.IsEnabled = true;//显示级别选择
            StrRadio = "1";
        }
        #endregion

        #region Redio岗位
        private void RbtNo_Click(object sender, RoutedEventArgs e)
        {
            this.RbtNo.IsChecked = true;
            this.rbtYes.IsChecked = false;

            cobPostId.IsEnabled = false;//隐藏级别选择
            cobPostLevle.IsEnabled = false;//隐藏级别选择
            PostsObject.IsEnabled = true;//显示岗位选择
            StrRadio = "0";
        }
        #endregion

        #region DaGrs_LoadingRow
        private void DaGrs_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_WELFAREDETAIL tmp = (T_OA_WELFAREDETAIL)e.Row.DataContext;

            ImageButton MyButton_Delbaodao = DaGrs.Columns[6].GetCellContent(e.Row).FindName("BtnDel") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Delbaodao.Tag = tmp;
        }
        #endregion

        #region 添加事件
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Check())
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);

                    this.welfareDetailList.Add(this.welfareDetail);
                    this.cobPostId.SelectedIndex = 0;
                    this.cobPostLevle.SelectedIndex = 0;

                    NewDetail();
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
            }
        }
        private void NewDetail()
        {
            welfareDetail = new T_OA_WELFAREDETAIL();
            welfareDetail.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(d_PropertyChanged);
            welfareDetail.WELFAREDETAILID = Guid.NewGuid().ToString();
            welfareDetail.ISLEVEL = StrRadio;//默认选中岗位级别
            //PostsObject.IsEnabled = false;//隐藏岗位选择
            welfareDetail.T_OA_WELFAREMASERT = welfareInfo;
            //this.welfareDetailList.Add(welfareDetail);
            this.DetailBinding(welfareDetail);
        }
        #endregion

        #region DataGrid事件处理

        private void DaGrs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //T_OA_WELFAREDETAIL detail = DaGrs.SelectedItem as T_OA_WELFAREDETAIL;
            //this.welfareDetail = detail;
            //this.DetailBinding(detail);
        }
        #endregion

        #region 动态绑定DataGrid
        private void DetailBinding(T_OA_WELFAREDETAIL detail)
        {
            this.WelfareStandard.DataContext = detail;
            this.txtRemark1.DataContext = detail;
            this.cobPostLevle.DataContext = detail;
            this.cobPostId.DataContext = detail;
            this.PostsObject.DataContext = detail;
            this.rbtYes.DataContext = detail;
            this.RbtNo.DataContext = detail;
        }
        #endregion

        #region Redio事件处理

        public class BoolConvert : IValueConverter
        {
            public bool IsYes { get; set; }

            #region IValueConverter 成员

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {

                if (targetType == typeof(Nullable<Boolean>))
                {
                    if (string.IsNullOrEmpty(value as string))
                    {
                        return null;
                    }
                    bool result = System.Convert.ToBoolean(System.Convert.ToInt32(value));
                    if (IsYes)
                    {
                        return result;
                    }
                    else
                    {
                        return !result;
                    }
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                bool v = (bool)value;
                if (!IsYes)
                {
                    v = !v;
                }
                return System.Convert.ToInt32(v).ToString();
            }
            #endregion
        }
        #endregion

        #region 删除
        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            if (DaGrs.SelectedItems == null)
            {
                return;
            }

            if (DaGrs.SelectedItems.Count == 0)
            {
                return;
            }

            welfareDetailList = DaGrs.ItemsSource as ObservableCollection<T_OA_WELFAREDETAIL>;

            for (int i = 0; i < DaGrs.SelectedItems.Count; i++)
            {
                T_OA_WELFAREDETAIL entDel = DaGrs.SelectedItems[i] as T_OA_WELFAREDETAIL;

                if (welfareDetailList.Contains(entDel))
                {
                    welfareDetailList.Remove(entDel);
                }
            }
            DaGrs.ItemsSource = welfareDetailList;
        }
        #endregion

        #region 判断岗位级别段

        void d_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "POSTID" || e.PropertyName == "POSTLEVELA" || e.PropertyName == "POSTLEVELB")
            {
                T_OA_WELFAREDETAIL detailInfo = sender as T_OA_WELFAREDETAIL;
                CheckDuplicate(detailInfo);
            }
        }
        private bool CheckDuplicate(T_OA_WELFAREDETAIL detailIsCeck)
        {
            welfareDetailList.ForEach(item =>
            {
                if (item == detailIsCeck)
                {
                    return;
                }
                if (detailIsCeck.ISLEVEL == "0")
                {
                    if (item.ISLEVEL != "0")
                    {
                        return;
                    }
                    if (item.POSTID == detailIsCeck.POSTID && !string.IsNullOrEmpty(detailIsCeck.POSTID))
                    {
                        detailIsCeck.POSTID = null;
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEREPEATEDTOADD", "POSTNAME"));
                        return;
                    }
                }
                else
                {
                    if (item.ISLEVEL == "0")
                    {
                        return;
                    }

                    int levelAItem = string.IsNullOrEmpty(item.POSTLEVELA) ? -1 : Convert.ToInt32(item.POSTLEVELA);
                    int levelBItem = string.IsNullOrEmpty(item.POSTLEVELB) ? -1 : Convert.ToInt32(item.POSTLEVELB);

                    if (levelAItem == -1)
                    {
                        levelAItem = levelBItem;
                    }

                    if (levelBItem == -1)
                    {
                        levelBItem = levelAItem;
                    }
                    if (levelBItem == -1 && levelAItem == -1)
                    {
                        return;
                    }

                    int? rA = null;
                    int? rB = null;

                    if (!string.IsNullOrEmpty(detailIsCeck.POSTLEVELA))
                    {
                        int levelADetail = Convert.ToInt32(detailIsCeck.POSTLEVELA);

                        if (levelADetail < levelAItem)
                        {
                            rA = -1;
                        }
                        else if (levelADetail > levelBItem)
                        {
                            rA = 1;
                        }
                        else
                        {
                            rA = 0;
                        }
                        if (rA == 0)
                        {
                            detailIsCeck.POSTLEVELA = null;
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ORLEVELSEGMENTREPEAT", "POSTLEVEL"));
                            return;
                        }
                    }

                    if (!string.IsNullOrEmpty(detailIsCeck.POSTLEVELB))
                    {
                        int levelBDetail = Convert.ToInt32(detailIsCeck.POSTLEVELB);

                        if (levelBDetail < levelAItem)
                        {
                            rB = -1;
                        }
                        else if (levelBDetail > levelBItem)
                        {
                            rB = 1;
                        }
                        else
                        {
                            rB = 0;
                        }

                        if (rB == 0)
                        {
                            detailIsCeck.POSTLEVELB = null;
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ORLEVELSEGMENTREPEAT", "POSTLEVEL"));
                            return;
                        }
                    }

                    if (rB != null && rA != null && rB.Value != rA.Value)
                    {
                        detailIsCeck.POSTLEVELB = null;
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ORLEVELSEGMENTREPEAT", "POSTLEVEL"));
                        return;
                    }
                }
            });
            return true;
        }
        #endregion

        #region SetButtonVisible
        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //草稿箱
                    this.DaGrs.Columns[6].Visibility = Visibility.Visible;//隐藏操作列
                    break;
                case "1":  //审批中
                    this.DaGrs.Columns[6].Visibility = Visibility.Collapsed;//隐藏操作列
                    break;
                case "2":  //审批通过
                    this.DaGrs.Columns[6].Visibility = Visibility.Collapsed;//显示操作列
                    break;
                case "3":  //审批未通过
                    this.DaGrs.Columns[6].Visibility = Visibility.Visible;//隐藏操作列
                    break;
                case "4":  //待审核
                    this.DaGrs.Columns[6].Visibility = Visibility.Collapsed;//隐藏操作列
                    break;
            }
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_WELFAREMASERT>(InfoObj, "OA");
            Utility.SetAuditEntity(entity, "T_OA_WELFAREMASERT", InfoObj.WELFAREID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (InfoObj.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            InfoObj.CHECKSTATE = state;
            wssc.UpdateWelfareStandardAsync(InfoObj, welfareDetailList, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (InfoObj != null)
                state = InfoObj.CHECKSTATE;
            if (actions == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            wssc.DoClose();
            client.DoClose();
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
