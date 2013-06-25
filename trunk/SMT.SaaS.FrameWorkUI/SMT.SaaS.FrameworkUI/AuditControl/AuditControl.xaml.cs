/// <summary>
/// Log No.： 1
/// Modify Desc： 记录业务ID和流程模块关联ID（表单提交），引擎发起消息
/// Modifier： 冉龙军
/// Modify Date： 2010-08-10
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI.Common;
using System.Windows.Data;
using SMT.Saas.Tools.FlowWFService;
using SMT.SaaS.FrameworkUI.KPIControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
// 1s 冉龙军
using System.Text;
using SMT.Saas.Tools;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
// 1e

namespace SMT.SaaS.FrameworkUI.AuditControl
{
    /// <summary>
    /// 审核控件
    /// </summary>
    [TemplatePart(Name = "imgCheckStateIoc", Type = typeof(Image)),
    TemplatePart(Name = "txtCreatorName", Type = typeof(TextBlock)),
    TemplatePart(Name = "txtCreateDate", Type = typeof(TextBlock))]
    public partial class AuditControl : UserControl
    {      
        /// <summary>
        /// 审核操作
        /// </summary>
        protected enum AuditOperation
        {
            /// <summary>
            /// 新增
            /// </summary>
            Add,
            /// <summary>
            /// 修改
            /// </summary>
            Update,
            #region beyond
            Cancel = 5

            #endregion 
        }
        /// <summary>
        /// 审核动作
        /// </summary>
        public enum AuditAction
        {
            /// <summary>
            /// 审核不通过
            /// </summary>
            Fail = 0,
            /// <summary>
            /// 审核通过
            /// </summary>
            Pass = 1
           
        }        

        public AuditControl()
        {
            InitializeComponent();
            
            //注册按钮事件。
            this.btnFail.Click += new RoutedEventHandler(btnFail_Click);
            this.btnSuccess.Click += new RoutedEventHandler(btnSuccess_Click);
            RdbAuditModel.Checked += new RoutedEventHandler(RdbAuditModel_Checked);
            RdbAuditFree.Checked += new RoutedEventHandler(RdbAuditFree_Checked);
            ckbIsEndAudit.Click += new RoutedEventHandler(ckbIsEndAudit_Click);
            btnLookUpDepartment.Click += new RoutedEventHandler(btnLookUpDepartment_Click);
            //this.AuditListPnl
            AuditEntity = new Flow_FlowRecord_T();
            InitParameter();

            // this.txRemark.MaxLength = 1000;
            AuditSubmitData = new SubmitData();

            // Added by Water 20100916 默认是固定流程
            IsFixedFlow = true;

            // beyond 加入撤单
            this.btnCancelSubmit.Click+=new RoutedEventHandler(btnCancelSubmit_Click);

            
        }

        protected AuditOperation currAuditOperation { get; set; }
        protected AuditAction curAuditAction { get; set; }
        /// <summary>
        /// 是否可以使用自选流程
        /// </summary>
        private bool IsUserFreeFlow = false;
        /// <summary>
        /// 提单人是可以撤回流程
        /// </summary>
        private bool IsCanCancel = false;
        #region 属   性
        private IAuditService auditService = null;
        private AuditAction currentAction;
        /// <summary>
        /// 当前审核流程记录详情
        /// </summary>
        private FLOW_FLOWRECORDDETAIL_T currentFLOWRECORDDETAIL = null;
        // 1s 冉龙军
        // 绩效考核服务
        private SMT.Saas.Tools.PerformanceWS.PerformanceServiceClient pclient = null;
        // 1e
        #region 控制界面视图状态属性

        /// <summary>
        /// 是否要显示审核信息面板
        /// </summary>
        protected bool IsShowList
        {
            get
            {
                return this.AuditListPnl.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (this.AuditListPnl.IsNotNull())
                        this.AuditListPnl.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this.AuditListPnl.IsNotNull())
                        this.AuditListPnl.Visibility = Visibility.Collapsed;
                }
            }
        }
        /// <summary>
        /// 是否要显示输入审核意见面板
        /// </summary>
        protected bool IsShowForm
        {
            get
            {
                return this.AuditFormPnl.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (this.AuditFormPnl.IsNotNull())
                        this.AuditFormPnl.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this.AuditFormPnl.IsNotNull())
                        this.AuditFormPnl.Visibility = Visibility.Collapsed;
                }
            }
        }
        /// <summary>
        /// 是否显示选择审核方式面板
        /// 仅在提交审批时使用
        /// </summary>
        protected bool IsShowAuditTypePnl
        {
            get
            {
                return this.AuditTypePnl.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (this.AuditTypePnl.IsNotNull())
                        this.AuditTypePnl.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this.AuditTypePnl.IsNotNull())
                        this.AuditTypePnl.Visibility = Visibility.Collapsed;
                }
            }
        }
        /// <summary>
        /// 是否显示选择终审面板
        /// </summary>
        protected bool IsShowEndAuditPnl
        {
            get
            {
                return this.EndAuditPnl.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (this.EndAuditPnl.IsNotNull())
                        this.EndAuditPnl.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this.EndAuditPnl.IsNotNull())
                        this.EndAuditPnl.Visibility = Visibility.Collapsed;
                }
            }
        }
        /// <summary>
        /// 是否显示选择下一审核人面板
        /// 1：在提交审核时，选择逐级审批时需要显示
        /// 2：审批过程中不为终审时需要显示
        /// </summary>
        protected bool IsShowSelectAuditPersonPnl
        {
            get
            {
                return this.SelectAuditPersonPnl.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (this.SelectAuditPersonPnl.IsNotNull())
                        this.SelectAuditPersonPnl.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this.SelectAuditPersonPnl.IsNotNull())
                        this.SelectAuditPersonPnl.Visibility = Visibility.Collapsed;
                }
            }
        }
        /// <summary>
        /// 是否审核按钮面板
        /// </summary>
        protected bool IsShowAuditButtonPnl
        {
            get
            {
                return this.AuditButtonPnl.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (this.AuditButtonPnl.IsNotNull())
                        this.AuditButtonPnl.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this.AuditButtonPnl.IsNotNull())
                        this.AuditButtonPnl.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion

        /// <summary>
        /// WcfFlowService.FlowResult cc = service.StartFlow(
        /// "FormID"[FormID:业务表单ID，必填], 
        /// ""[FlowGUID:待审批流程GUID，不填],
        /// "SMTTestFlow"[ModelCode:模块代码,必填],
        /// "SMT"[CompanyID:公司代码,必填], 
        /// "Manage"[OfficeID:部门代码,必填], 
        /// "User"[CreateUserID:创建用户ID,必填],
        /// ""[NextStateCode:自定义流程代码,可选], 
        /// "User"[AppUserId:下一步骤人ID,可选], 
        /// ""[Content:审批意见内容,不填],
        /// ""[Appopt:审批意见(0-不同意,1-同意),不填],
        /// "Add"[操作标志：Add增加，Update：审批]);  //操作成功返回SUCCESS
        /// </summary>
        public Flow_FlowRecord_T AuditEntity { set; get; }
        public SubmitData AuditSubmitData { set; get; }

        public string XmlObject
        {
            get
            {
                return AuditEntity.XmlObject;
            }
            set
            {
                AuditEntity.XmlObject = value;
            }

        }

        /// <summary>
        /// CompanyID = ""; //公司ID --可作为查询参数
        /// OfficeID = ""; //岗位ID
        /// CreateUserID = "LoginUserID";Guid   //创建人
        /// EditUserID = "LoginUserID";Guid   //审批人
        /// FormID = ""; //表单ID：如何申请单ID; Guid  --可作为查询参数
        /// ModelCode = "";  //模块代码: 如 'S01'-申请单、 'S02'-报销单. --可作为查询参数
        /// </summary>
        public List<FLOW_FLOWRECORDDETAIL_T> AuditEntityList { get; set; }
        public List<FLOW_FLOWRECORDDETAIL_T> AllAuditEntityList { get; set; }
        public IAuditService AuditService
        {
            get
            {
                if (auditService == null)
                {
                    auditService = new CommonAudtiService();

                    auditService.SubimtFlowCompleted += new EventHandler<SubimtFlowCompletedEventArgs>(auditService_SubimtFlowCompleted);
                    auditService.GetFlowInfoCompleted += new EventHandler<GetFlowInfoCompletedEventArgs>(auditService_GetFlowInfoCompleted);
                }
                return auditService;
            }
            set
            {
                if (auditService != value)
                {
                    auditService = value;
                    auditService.SubimtFlowCompleted += new EventHandler<SubimtFlowCompletedEventArgs>(auditService_SubimtFlowCompleted);
                    auditService.GetFlowInfoCompleted += new EventHandler<GetFlowInfoCompletedEventArgs>(auditService_GetFlowInfoCompleted);
                }
            }
        }
        // 1s 冉龙军
        // 定义服务Pclient及其提交打分事件
        public SMT.Saas.Tools.PerformanceWS.PerformanceServiceClient Pclient
        {
            get
            {
                if (pclient == null)
                {
                    pclient = new Saas.Tools.PerformanceWS.PerformanceServiceClient();
                    pclient.KPIScorePostCompleted += new EventHandler<Saas.Tools.PerformanceWS.KPIScorePostCompletedEventArgs>(pclient_KPIScorePostCompleted);
                    pclient.GetKPIPointRandomPersonIDCompleted += new EventHandler<Saas.Tools.PerformanceWS.GetKPIPointRandomPersonIDCompletedEventArgs>(pclient_GetKPIPointRandomPersonIDCompleted);
                }
                return pclient;
            }
        }


        public bool IsAuditUser { get; set; }
        protected string NextStateCode { get; set; }

        protected string NextCompanyID { get; set; }
        protected string NextDepartmentID { get; set; }
        protected string NextPostID { get; set; }
        protected string NextUserID { get; set; }
        protected string NextUserName { get; set; }
        protected string AuditRemark { get; set; }
        public bool IsFixedFlow
        {
            get
            {
                if (this.RdbAuditFree.IsChecked.Value == true)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (value)
                {
                    this.RdbAuditFree.IsChecked = false;
                    this.RdbAuditModel.IsChecked = true;
                }
                else
                {
                    this.RdbAuditFree.IsChecked = true;
                    this.RdbAuditModel.IsChecked = false;
                }
            }
        }
        public bool IsEndAudit { get; set; }
        // 1s 冉龙军
        protected string flowStateCode { get; set; }
        protected string flowCheckState { get; set; }
        protected string flowModelFlowRelationID { get; set; }
        // 1e
        #endregion

        #region 审核控件事件
        public virtual event EventHandler<AuditEventArgs> AuditCompleted;
        public event EventHandler<AuditEventArgs> Auditing;
        public event EventHandler OnBindingDataCompleted;
        #endregion

        #region 审核后

        void auditService_SubimtFlowCompleted(object sender, SubimtFlowCompletedEventArgs e)
        {
            
            DoAuditResult(e.Result);
        }

        private DataResult selectDataresult;
        private bool isSelect = false;
        private void OtherAction(DataResult result)
        {
            isSelect = false;
            this.selectDataresult = result;
            NextStateCode = result.AppState;

            System.Windows.Controls.Window winSelector = new System.Windows.Controls.Window();
            winSelector.Unloaded += new RoutedEventHandler(winSelector_Unloaded);
            winSelector.Height = 250;
            winSelector.Width = 400;
            winSelector.TitleContent = "确认审核人";
            Grid gridSelector = new Grid();
            RowDefinition r1 = new RowDefinition();
            RowDefinition r2 = new RowDefinition();
            RowDefinition r3 = new RowDefinition();
            r1.Height = new GridLength(20, GridUnitType.Auto);
            r2.Height = new GridLength(1, GridUnitType.Star);
            r3.Height = new GridLength(20, GridUnitType.Auto);
            gridSelector.RowDefinitions.Add(r1);
            gridSelector.RowDefinitions.Add(r2);
            gridSelector.RowDefinitions.Add(r3);


            TextBlock tb = new TextBlock();
            tb.Text = "不能确定下一审核人, 请重新选择一个审核人，并按确认提交";
            tb.SetValue(Grid.RowProperty, 0);

            StackPanel sp = new StackPanel();

            sp.Margin = new Thickness(15, 5, 0, 0);
            sp.Orientation = Orientation.Vertical;
            sp.SetValue(Grid.RowProperty, 1);

            for (int i = 0; i < result.UserInfo.Count; i++)
            {
                RadioButton rbtn = new RadioButton();
                //rbtn.Content = result.UserInfo[i].UserName;
                rbtn.Content = result.UserInfo[i].UserName + "(" + result.UserInfo[i].CompanyName + "->" + result.UserInfo[i].DepartmentName + "->" + result.UserInfo[i].PostName + ")";

                rbtn.DataContext = result.UserInfo[i];
                rbtn.GroupName = "User";
                sp.Children.Add(rbtn);
            }

            Button btnOK = new Button();
            btnOK.Height = 26;
            btnOK.Width = 80;
            btnOK.Content = Utility.GetResourceStr("lblConfirm");
            btnOK.HorizontalAlignment = HorizontalAlignment.Right;
            btnOK.Margin = new Thickness(0, 0, 5, 10);
            btnOK.SetValue(Grid.RowProperty, 2);

            btnOK.Click += (e, o) =>
            {
                this.isSelect = true;
                UIElement element = sp.Children.FirstOrDefault(item =>
                {
                    RadioButton rb = item as RadioButton;
                    return rb.IsChecked == true;
                });
                if (element == null)
                {
                    this.isSelect = false;
                    ComfirmWindow.ConfirmationBox("警告", "请先选择一个审核人!", Utility.GetResourceStr("CONFIRMBUTTON"));

                    //MessageBox.Show("请先选择一个审核人");
                }
                else
                {
                    RadioButton rbSelect = element as RadioButton;
                    UserInfo otherUser = rbSelect.DataContext as UserInfo;
                    NextCompanyID = otherUser.CompanyID;
                    NextDepartmentID = otherUser.DepartmentID;
                    NextPostID = otherUser.PostID;
                    NextUserID = otherUser.UserID;
                    NextUserName = otherUser.UserName;

                    InnerHandIn(currAuditOperation, curAuditAction);
                    winSelector.Close();
                }
            };
            ContentControl parent = new ContentControl();
            parent.Content = gridSelector;

            winSelector.Content = parent;
            gridSelector.Children.Add(tb);
            gridSelector.Children.Add(sp);
            gridSelector.Children.Add(btnOK);
            FrameworkElement fe = SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot;

            // Window.Show("", "", Guid.NewGuid().ToString(), true, false, parent, null);
            winSelector.Show<string>(DialogMode.Default, fe, "", (resulta) => { });

        }

        void winSelector_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!this.isSelect)
            {
                AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.Error, this.selectDataresult);
                args.StartDate = this.AuditEntity.StartDate;
                args.EndDate = System.DateTime.Now;
                OnAuditCompleted(this, args);
            }
        }
        private void DoAuditResult(DataResult dataResult)
        {
            try
            {
                
                
                //beyond 加入撤单
                if (dataResult.FlowResult == FlowResult.SUCCESS && dataResult.SubmitFlag == SubmitFlag.Cancel)
                {
                    AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.CancelSubmit, dataResult);
                    args.StartDate = this.AuditEntity.StartDate;
                    args.EndDate = System.DateTime.Now;
                    OnAuditCompleted(this, args);
                    this.BindingData();
                }else   if (dataResult.FlowResult == FlowResult.SUCCESS)
                {

                    AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.Auditing, dataResult);
                    if (currAuditOperation == AuditOperation.Add)
                    {
                        args.Action = AuditEventArgs.AuditAction.Submit;
                    }
                    args.StartDate = this.AuditEntity.StartDate;
                    args.EndDate = System.DateTime.Now;
                    OnAuditCompleted(this, args);
                    this.BindingData();
                }
                else if (dataResult.FlowResult == FlowResult.END)
                {
                    if (currentAction == AuditAction.Fail)
                    {
                        AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.Fail, dataResult);
                        args.StartDate = this.AuditEntity.StartDate;
                        args.EndDate = System.DateTime.Now;
                        OnAuditCompleted(this, args);
                    }
                    else if (currentAction == AuditAction.Pass)
                    {
                        AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.Successful, dataResult);
                        args.StartDate = this.AuditEntity.StartDate;
                        args.EndDate = System.DateTime.Now;
                        OnAuditCompleted(this, args);
                    }
                    this.BindingData();
                }
                else if (dataResult.FlowResult == FlowResult.FAIL)
                {
                    AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.Error, dataResult);
                    args.StartDate = this.AuditEntity.StartDate;
                    args.EndDate = System.DateTime.Now;
                    OnAuditCompleted(this, args);
                }
                else if (dataResult.FlowResult == FlowResult.Countersign)
                {
                    this.CounterAction(dataResult);
                }
                else
                {
                    OtherAction(dataResult);
                }
            }

            // 1s 冉龙军
            //    catch
            //{
            //    throw new Exception();
            //}
            catch (Exception ex)
            {
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            // 1e

            finally
            {
                CanAudit(true);
            }

        }
        private void OnAuditCompleted(object sender, AuditEventArgs args)
        {
            // 加KPI
            // 1s 冉龙军
            // 提交处理KPI打分
            // 2s
            //if (args.Action == AuditEventArgs.AuditAction.Submit && args.Result == AuditEventArgs.AuditResult.Successful)
            if (args.Action == AuditEventArgs.AuditAction.Submit)
            // 2e
            {
                // ModelFlowRelationID：流程关联ID
                if (args.InnerResult.ModelFlowRelationID != null && args.InnerResult.ModelFlowRelationID != "")
                {

                    //记录流程的值
                    //flowStateCode = args.InnerResult.AppState;
                    if (args.InnerResult.AppState != null)
                    {
                        flowStateCode = args.InnerResult.AppState;
                    }
                    else
                    {
                        flowStateCode = "";
                    }
                    if (args.InnerResult.CheckState != null)
                    {
                        flowCheckState = args.InnerResult.CheckState;
                    }
                    else
                    {
                        flowCheckState = "";
                    }
                    if (args.InnerResult.ModelFlowRelationID != null)
                    {
                        flowModelFlowRelationID = args.InnerResult.ModelFlowRelationID;
                    }
                    else
                    {
                        flowModelFlowRelationID = "";
                    }
                    //记录业务ID和流程模块关联ID（表单提交），然后随机获取KPI考核点的抽查组成员
                    //beyond 隐藏kpi
                    KPIPost(this.AuditEntity.FormID, args.InnerResult.ModelFlowRelationID, AuditEntity.CreateCompanyID, AuditEntity.CreateDepartmentID, AuditEntity.CreatePostID, AuditEntity.CreateUserID);

                }
                else
                {
                    //没有关联，没有KPI点
                }
            }
            // 审核处理KPI打分(2010-9-14 19:25:25 暂时屏蔽)
            if (args.Action == AuditEventArgs.AuditAction.Audit)
            {
                //随机获取KPI考核点的抽查组成员
                //flowStateCode = args.InnerResult.AppState;
                if (args.InnerResult.AppState != null)
                {
                    flowStateCode = args.InnerResult.AppState;

                    flowCheckState = args.InnerResult.CheckState;
                }
                else
                {
                    flowStateCode = "";
                    flowCheckState = "";
                }
                // 2s 冉龙军

                //Pclient.GetKPIPointRandomPersonIDAsync(this.AuditEntity.CreateCompanyID, "", this.AuditEntity.FormID, args.InnerResult.AppState, this.AuditEntity.FormID);

                if (currentFLOWRECORDDETAIL == null)
                {
                }
                else
                {
                    FLOW_FLOWRECORDDETAIL_T ParentFlow = this.AllAuditEntityList.FirstOrDefault(item =>
                                    {
                                        return item.FLOWRECORDDETAILID == currentFLOWRECORDDETAIL.PARENTSTATEID;
                                    });
                    //beyond 隐藏kpi
                    Pclient.KPISystemScoreAsync(AuditEntity.CreateCompanyID, "",
                        this.AuditEntity.FormID, currentFLOWRECORDDETAIL.STATECODE, this.AuditEntity.FormID, "", currentFLOWRECORDDETAIL.STATECODE,
                        ParentFlow.EDITDATE.Value, currentFLOWRECORDDETAIL.EDITDATE.Value, currentFLOWRECORDDETAIL.EDITUSERID);
                    //使用userstate 保存审核时的statecode（审核控件还未刷新）
                    Pclient.GetKPIPointRandomPersonIDAsync(this.AuditEntity.CreateCompanyID, "", this.AuditEntity.FormID, currentFLOWRECORDDETAIL.STATECODE, this.AuditEntity.FormID, currentFLOWRECORDDETAIL.STATECODE);
                }
                // 2e
            }
            // 1e
            if (AuditCompleted != null)
            {
                AuditCompleted(sender, args);
            }
        }
        #endregion

        #region 审核中


        protected void HandIn(AuditOperation auditOperation, AuditAction action)
        {
            try
            {
                CanAudit(false);
                #region 参数信息
                /*
           FormID	string	业务表单ID	必填
           FlowGUID	string	待审批流程GUID	Status为Add时不填, Status为Update时必填
           ModelCode	string	模块代码	必填
           CompanyID	string	公司代码	必填
           PostID	string	岗位ID	必填
           CreateUserID	string	创建用户ID	必填
           CreateUserName	string	创建用户名称	必填
           NextStateCode	string	自定义流程代码	可选
           AppUserId	string	下一步骤人ID	必填
           AppUserName	string	下一步骤人名称	必填
           Content	string	审批意见内容	Status为Add时不填
           Status为Update时必填
           AppOpt	string	审批意见(0-不同意,1-同意)	Status为Add时不填
           Status为Update时必填 Status	string	操作标志:Add-增加，Update-审批]	必填

            */
                #endregion

                currAuditOperation = auditOperation;
                curAuditAction = action;

                currentAction = action;
                AuditRemark = this.txRemark.Text;

                string op = auditOperation.ToString();

                if (string.IsNullOrEmpty(AuditRemark))
                {
                    if (auditOperation == AuditOperation.Update)
                    {
                        AuditRemark = action == AuditAction.Fail ? Utility.GetResourceStr("AUDITNOPASS") : Utility.GetResourceStr("AUDITPASS");
                    }
                    else
                    {
                        AuditRemark = Utility.GetResourceStr("SUBMITAUDIT");
                    }
                }
                // 审核前的事件
                if (!OnAuditing(auditOperation, action))
                {
                    return;
                }
                if (!AuditCheck())
                {
                    return;
                }
                this.AuditEntity.StartDate = System.DateTime.Now;
                InnerHandIn(auditOperation, action);
                // ShowKPI(auditOperation, action);
            }
            catch
            {
                CanAudit(true);
            }
        }

        protected virtual void InnerHandInOld(AuditOperation auditOperation, AuditAction action)
        {
            string op = auditOperation.ToString();
            string tmpnextStateCode = IsEndAudit ? "EndFlow" : NextStateCode; //EndFlow

            SubmitFlag AuditSubmitFlag = op.ToUpper() == "ADD" ? SubmitFlag.New : SubmitFlag.Approval;

            AuditSubmitData.FormID = AuditEntity.FormID;
            AuditSubmitData.ModelCode = AuditEntity.ModelCode;
            AuditSubmitData.ApprovalUser = new UserInfo();
            AuditSubmitData.ApprovalUser.CompanyID = AuditEntity.CreateCompanyID;

            AuditSubmitData.ApprovalUser.DepartmentID = AuditEntity.CreateDepartmentID;
            AuditSubmitData.ApprovalUser.PostID = AuditEntity.CreatePostID;
            AuditSubmitData.ApprovalUser.UserID = AuditEntity.CreateUserID;
            AuditSubmitData.ApprovalUser.UserName = AuditEntity.CreateUserName;
            AuditSubmitData.ApprovalContent = AuditRemark;
            AuditSubmitData.NextStateCode = tmpnextStateCode;
            AuditSubmitData.NextApprovalUser = new UserInfo();
            AuditSubmitData.NextApprovalUser.CompanyID = NextCompanyID;
            AuditSubmitData.NextApprovalUser.DepartmentID = NextDepartmentID;
            AuditSubmitData.NextApprovalUser.PostID = NextPostID;
            AuditSubmitData.NextApprovalUser.UserID = NextUserID;
            AuditSubmitData.NextApprovalUser.UserName = NextUserName;
            AuditSubmitData.SubmitFlag = AuditSubmitFlag;
            AuditSubmitData.XML = XmlObject;
            AuditSubmitData.FlowSelectType = IsFixedFlow ? FlowSelectType.FixedFlow : FlowSelectType.FreeFlow;

            if (AuditSubmitData.FlowType == null)
                AuditSubmitData.FlowType = FlowType.Approval;

            if (AuditSubmitFlag == SubmitFlag.Approval)
            {
                AuditSubmitData.ApprovalUser.CompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                AuditSubmitData.ApprovalUser.DepartmentID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                AuditSubmitData.ApprovalUser.PostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                AuditSubmitData.ApprovalUser.UserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                AuditSubmitData.ApprovalUser.UserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            }
            //提交人(只帮别人提单的时候起作用,区分单据所属人)
            AuditSubmitData.SumbitCompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            AuditSubmitData.SumbitDeparmentID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            AuditSubmitData.SumbitPostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            AuditSubmitData.SumbitUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            AuditSubmitData.SumbitUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            //end /提交人(只帮别人提单的时候起作用,区分单据所属人)

            AuditSubmitData.ApprovalResult = (ApprovalResult)((int)action);// SMTWFTest.WcfFlowService.ApprovalResult.Pass;
            AuditService.SubimtFlowAsync(AuditSubmitData);
        }

        protected virtual void InnerHandIn(AuditOperation auditOperation, AuditAction action)
        {
            string op = auditOperation.ToString();
            string tmpnextStateCode = IsEndAudit ? "EndFlow" : NextStateCode; //EndFlow

            SubmitFlag AuditSubmitFlag = op.ToUpper() == "ADD" ? SubmitFlag.New : SubmitFlag.Approval;
            #region beyond
            switch (auditOperation)
            {
                case AuditOperation.Add:
                    AuditSubmitFlag = SubmitFlag.New;
                    break;
                case AuditOperation.Update:
                    AuditSubmitFlag = SubmitFlag.Approval;
                    break;
                case AuditOperation.Cancel:
                    AuditSubmitFlag = SubmitFlag.Cancel;
                    break;
                default:
                    break;

            }
            AuditSubmitData.DictCounterUser = this.DictCounterUser;
            if (AuditSubmitFlag == SubmitFlag.New)
            {
                AuditSubmitData.XML = XmlObject;
            }

            
            #endregion 

            AuditSubmitData.FormID = AuditEntity.FormID;
            AuditSubmitData.ModelCode = AuditEntity.ModelCode;
            AuditSubmitData.ApprovalUser = new UserInfo();
            AuditSubmitData.ApprovalUser.CompanyID = AuditEntity.CreateCompanyID;

            AuditSubmitData.ApprovalUser.DepartmentID = AuditEntity.CreateDepartmentID;
            AuditSubmitData.ApprovalUser.PostID = AuditEntity.CreatePostID;
            AuditSubmitData.ApprovalUser.UserID = AuditEntity.CreateUserID;
            AuditSubmitData.ApprovalUser.UserName = AuditEntity.CreateUserName;
            AuditSubmitData.ApprovalContent = AuditRemark;
            AuditSubmitData.NextStateCode = tmpnextStateCode;
            AuditSubmitData.NextApprovalUser = new UserInfo();
            AuditSubmitData.NextApprovalUser.CompanyID = NextCompanyID;
            AuditSubmitData.NextApprovalUser.DepartmentID = NextDepartmentID;
            AuditSubmitData.NextApprovalUser.PostID = NextPostID;
            AuditSubmitData.NextApprovalUser.UserID = NextUserID;
            AuditSubmitData.NextApprovalUser.UserName = NextUserName;
            AuditSubmitData.SubmitFlag = AuditSubmitFlag;
            //AuditSubmitData.XML = XmlObject;
           
            AuditSubmitData.FlowSelectType = IsFixedFlow ? FlowSelectType.FixedFlow : FlowSelectType.FreeFlow;

            if (!IsFixedFlow && ckbIsEndAudit.IsChecked.Value != true && action!=AuditAction.Fail)
            {
                if (string.IsNullOrEmpty(this.txtAuditId.Text))
                {
                    //ComfirmWindow.ConfirmationBox("","请选择下一审核人" , Utility.GetResourceStr("CONFIRMBUTTON"));
                    DataResult dataResult = new DataResult();
                    dataResult.FlowResult = FlowResult.FAIL;
                    dataResult.Err = "请选择下一审核人";
                    //AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.Error, dataResult);
                    //args.StartDate = this.AuditEntity.StartDate;
                    //args.EndDate = System.DateTime.Now;
                    this.DoAuditResult(dataResult);
                    //this.CloseProcess();
                    return;
                }
                else if (this.txtAuditId.Text == SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
                {
                    DataResult dataResult = new DataResult();
                    dataResult.FlowResult = FlowResult.FAIL;
                    dataResult.Err = "不能提交给自己";
                    //AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.Error, dataResult);
                    //args.StartDate = this.AuditEntity.StartDate;
                    //args.EndDate = System.DateTime.Now;
                    this.DoAuditResult(dataResult);
                    //this.CloseProcess();
                    return;
                }
            }

            if (AuditSubmitData.FlowType == null)
                AuditSubmitData.FlowType = FlowType.Approval;

            if (AuditSubmitFlag == SubmitFlag.Approval )
            {
                AuditSubmitData.ApprovalUser.CompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                AuditSubmitData.ApprovalUser.DepartmentID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                AuditSubmitData.ApprovalUser.PostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                AuditSubmitData.ApprovalUser.UserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                AuditSubmitData.ApprovalUser.UserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            }
            //提交人(只帮别人提单的时候起作用,区分单据所属人)
            AuditSubmitData.SumbitCompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            AuditSubmitData.SumbitDeparmentID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            AuditSubmitData.SumbitPostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            AuditSubmitData.SumbitUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            AuditSubmitData.SumbitUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            //end /提交人(只帮别人提单的时候起作用,区分单据所属人)
            AuditSubmitData.ApprovalResult = (ApprovalResult)((int)action);// SMTWFTest.WcfFlowService.ApprovalResult.Pass;
            AuditService.SubimtFlowAsync(AuditSubmitData);
            // beyond 记录日志
            //submitStartTime = DateTime.Now;
        }

        //beyond 记录日志
        //private DateTime submitStartTime = DateTime.Now;
        //private DateTime getflowStartTime = DateTime.Now;
        // 1s 冉龙军
        /// <summary>
        /// 记录业务ID和流程模块关联ID（表单提交）
        /// </summary>
        /// <param name="BusinessCodePost">FormID</param>
        /// <param name="RelationIDPost">RelationID</param>
        /// <returns></returns>
        protected void KPIPost(string BusinessCodePost, string RelationIDPost, string CreateCompanyIDPost, string CreateDepartmentIDPost, string CreatePostIDPost, string CreateUserIDPost)
        {
            //GetKPIPointListByBusinessCode
            if (RelationIDPost != null && RelationIDPost != "")
            {
                Pclient.KPIScorePostAsync(BusinessCodePost, RelationIDPost, CreateCompanyIDPost, CreateDepartmentIDPost, CreatePostIDPost, CreateUserIDPost);
            }
        }
        // 1e
        //protected void ShowKPI(AuditOperation auditOperation, AuditAction action)
        //{
        //    if (currentFLOWRECORDDETAIL == null)
        //    {
        //        return;
        //    }
        //    FLOW_FLOWRECORDDETAIL_T c = currentFLOWRECORDDETAIL;

        //    FLOW_FLOWRECORDDETAIL_T parentFlow = this.AuditEntityList.FirstOrDefault(item =>
        //        {
        //            return item.FLOWRECORDDETAILID == c.PARENTSTATEID;
        //        });
        //    if (parentFlow == null)
        //    {
        //        parentFlow = new FLOW_FLOWRECORDDETAIL_T();
        //        parentFlow.STATECODE = "";

        //        // 1s 冉龙军
        //        //parentFlow.EDITDATE = DateTime.MinValue;
        //        //parentFlow.EDITUSERID = "";
        //        parentFlow.EDITDATE = c.CREATEDATE;
        //        parentFlow.EDITUSERID = c.EDITUSERID;
        //        // 1e
        //    }

        //    KPIScoring kpiForm = new KPIScoring();
        //    // 1s 冉龙军
        //    //kpiForm.InitialInfo(c.CREATECOMPANYID,
        //    //    AuditEntity.ModelCode,
        //    //    c.STATECODE, parentFlow.STATECODE, AuditEntity.FormID,
        //    //    c.FLOWRECORDDETAILID, parentFlow.FLOWRECORDDETAILID, parentFlow.EDITDATE.Value,
        //    //    System.DateTime.Now, parentFlow.EDITUSERID, c.EDITUSERID);
        //    kpiForm.InitialInfo(c.CREATECOMPANYID,
        //        AuditEntity.ModelCode,
        //        c.STATECODE, parentFlow.STATECODE, AuditEntity.FormID,
        //        c.FLOWRECORDDETAILID, parentFlow.FLOWRECORDDETAILID, parentFlow.EDITDATE.Value,
        //        System.DateTime.Now, parentFlow.EDITUSERID, c.EDITUSERID);
        //    // 1s
        //    kpiForm.ShowFrom += (o, e) =>
        //        {
        //            if (e.IsShow)
        //            {
        //                FrameworkElement plRoot = SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot as FrameworkElement;
        //                EntityBrowser eb = new EntityBrowser(kpiForm);
        //                eb.Show(DialogMode.Modal, plRoot, "", (result) => { });

        //                eb.MinWidth = 800;
        //                eb.MinHeight = 600;

        //                kpiForm.Scored += () =>
        //                    {
        //                        // 1s 冉龙军
        //                        //InnerHandIn(auditOperation, action);
        //                        // 1e
        //                    };
        //            }
        //            else
        //            {
        //                // 1s 冉龙军
        //                //InnerHandIn(auditOperation, action);
        //                // 1e
        //            }
        //        };

        //}

        #endregion

        #region 审核前
        /// <summary>
        /// 提交审核流程系统。审核前，AuditEntity需要被赋值
        /// </summary>
        public void Submit()
        {
            HandIn(AuditOperation.Add, AuditAction.Pass);
        }
        /// <summary>
        /// 审核单据， 审核前，AuditEntity需要被赋值
        /// </summary>
        /// <param name="auditAction">Pass : 审核通过， Fail : 审核不通过</param>
        public void Submit(AuditAction auditAction)
        {
            HandIn(AuditOperation.Update, auditAction);
        }
        /// <summary>
        /// 检测审核信息
        /// </summary>
        private bool AuditCheck()
        {
            string strExceptionMsg = "";
            //if (string.IsNullOrEmpty(this.AuditEntity.CreateCompanyID))
            //{
            //    strExceptionMsg = "公司ID不能为空";
            //}

            if (string.IsNullOrEmpty(this.AuditEntity.CreateUserID))
            {
                strExceptionMsg = "创建人不能为空";
            }
            //if (string.IsNullOrEmpty(this.AuditEntity.EditUserID))
            //{
            //    strExceptionMsg = "修改人不能为空";
            //}

            if (string.IsNullOrEmpty(this.AuditEntity.CreatePostID))
            {
                strExceptionMsg = "岗位不能为空";
            }

            if (string.IsNullOrEmpty(this.AuditEntity.ModelCode))
            {
                strExceptionMsg = "模块代码不能为空";
            }

            if (string.IsNullOrEmpty(this.AuditEntity.FormID))
            {
                strExceptionMsg = "表单ID不能为空";
            }

            if (!string.IsNullOrEmpty(strExceptionMsg))
            {
                EntityBrowser dt = this.Parent as EntityBrowser;
                ComfirmWindow.ConfirmationBox("警告", strExceptionMsg, Utility.GetResourceStr("CONFIRMBUTTON"));
                if (dt != null) dt.SmtProgressBar.Visibility = Visibility.Collapsed;
                return false;
                //throw new Exception(strExceptionMsg);
            }
            else
            {
                return true;
            }

        }
        private bool OnAuditing(AuditOperation auditOperation, AuditAction action)
        {

            if (Auditing != null)
            {

                AuditEventArgs.AuditResult result = AuditEventArgs.AuditResult.Auditing;
                if (auditOperation == AuditOperation.Update)
                {
                    result = action == AuditAction.Fail ? AuditEventArgs.AuditResult.Fail : AuditEventArgs.AuditResult.Successful;
                }

                AuditEventArgs args = new AuditEventArgs(result, null);
                Auditing(this, args);
                return args.Result != AuditEventArgs.AuditResult.Cancel;
            }
            return true;
        }
        #endregion

        #region WCF事件完成方法
        // 1s 冉龙军
        /// <summary>
        /// 获取BusinessCode所有KPI点打分后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pclient_KPIScorePostCompleted(object sender, Saas.Tools.PerformanceWS.KPIScorePostCompletedEventArgs e)
        {

            if (e.Error != null && e.Error.Message != "")
            {
                //throw new NotImplementedException();
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //this.KPIPointList = e.Result;
                if (e.Result != null)
                {
                    int scoreCount = e.Result;
                }
                // 2s 冉龙军
                // 需求错了
                //随机获取KPI考核点的抽查组成员
                //Pclient.GetKPIPointRandomPersonIDAsync(this.AuditEntity.CreateCompanyID, flowModelFlowRelationID, "", flowStateCode, this.AuditEntity.FormID);
                // 2e
            }
        }
        /// <summary>
        /// 随机获取KPI考核点的抽查组成员后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pclient_GetKPIPointRandomPersonIDCompleted(object sender, Saas.Tools.PerformanceWS.GetKPIPointRandomPersonIDCompletedEventArgs e)
        {

            if (e.Error != null && e.Error.Message != "")
            {
                //throw new NotImplementedException();
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //结果处理
                if (e.Result == "") { }
                else
                {
                    //string stateCode = string.Empty;
                    //if (e.UserState != null)
                    //{
                    //    stateCode = e.UserState.ToString();
                    //}
                    string[] randomPersonRemind = e.Result.Split('|');

                    //抽查打分发送消息
                    string MessageUserID = randomPersonRemind[0];
                    string MessageUserName = "";
                    int i = randomPersonRemind.Length;
                    //提醒方式
                    int MessageRemind = int.Parse(randomPersonRemind[i - 1]);
                    if (MessageUserID != "")
                    {
                        // 1s 冉龙军
                        ////抽查打分发送消息XML
                        //StringBuilder scoreRandomXml = new StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                        //scoreRandomXml.Append(Environment.NewLine);
                        //scoreRandomXml.Append(@"    <System>");
                        //scoreRandomXml.Append(Environment.NewLine);
                        //scoreRandomXml.Append(@"       <Name>Flow</Name>");
                        ////scoreRandomXml.Append(@"       <Name>Score</Name>");
                        //scoreRandomXml.Append(Environment.NewLine);
                        ////scoreRandomXml.Append(@"       <SystemCode>""" + SystemCodeTask + @"""</SystemCode>");
                        //scoreRandomXml.Append(@"       <SystemCode>""" + "" + @"""</SystemCode>");
                        //scoreRandomXml.Append(Environment.NewLine);
                        //scoreRandomXml.Append(@"       <Message>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""CompanyID""  DataValue=""" + this.AuditEntity.CreateCompanyID + @"""></Attribute>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""ModelCode""  DataValue=""" + this.AuditEntity.ModelCode + @"""></Attribute>");
                        ////scoreRandomXml.Append(@"           <Attribute Name=""FormID""     DataValue=""" + this.AuditEntity.FormID + @"""></Attribute>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""FormID""     DataValue=""" + System.Guid.NewGuid().ToString() + @"""></Attribute>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""StateCode""  DataValue=""" + flowStateCode + @"""></Attribute>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""CheckState""  DataValue=""" + flowCheckState + @"""></Attribute>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""IsTask""     DataValue=""" + "1" + @"""></Attribute>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""AppUserID""  DataValue=""" + MessageUserID + @"""></Attribute>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""AppUserName""  DataValue=""" + MessageUserName + @"""></Attribute>");
                        //// 平台打分StateCode
                        //scoreRandomXml.Append(@"           <Attribute Name=""IsKpi""  DataValue=""" + "1" + @"""></Attribute>");
                        //scoreRandomXml.Append(@"           <Attribute Name=""FlowStateCode""  DataValue=""" + flowStateCode + @"""></Attribute>");
                        string guidRandomPersonRemind = System.Guid.NewGuid().ToString(); ;
                        //// 引擎提醒guid
                        //scoreRandomXml.Append(@"           <Attribute Name=""RemindGuid""  DataValue=""" + guidRandomPersonRemind + @"""></Attribute>");
                        //scoreRandomXml.Append(@"       </Message>");
                        //scoreRandomXml.Append(@"     </System>");

                        //SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient FlowEngine = new SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient();
                        //// 平台打分消息
                        //FlowEngine.SaveFlowTriggerDataAsync(scoreRandomXml.ToString(), this.AuditEntity.XmlObject);
                        //实体XML解析
                        string auditEntitySystemCode = "";
                        if (!string.IsNullOrEmpty(this.AuditEntity.XmlObject))
                        {
                            Byte[] Bo = System.Text.UTF8Encoding.UTF8.GetBytes(this.AuditEntity.XmlObject);
                            XElement xemeBoObject = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(Bo)));
                            auditEntitySystemCode = (from item in xemeBoObject.Descendants("Name") select item).FirstOrDefault().Value;
                        }

                        //抽查打分发送消息XML
                        StringBuilder scoreRandomXml = new StringBuilder();
                        scoreRandomXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                        scoreRandomXml.AppendLine("<System>");
                        scoreRandomXml.AppendLine("<Name>" + auditEntitySystemCode + "</Name>");
                        scoreRandomXml.AppendLine("<Object Name=\"Approval\" Description=\"\">");
                        scoreRandomXml.AppendLine("<Attribute Name=\"" + "AppUserID" + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + MessageUserID + "\"/>");
                        scoreRandomXml.AppendLine("<Attribute Name=\"" + "AppUserName" + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + MessageUserName + "\"/>");
                        // 平台打分StateCode
                        scoreRandomXml.AppendLine("<Attribute Name=\"" + "IsKpi" + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + "1" + "\"/>");
                        scoreRandomXml.AppendLine("<Attribute Name=\"" + "FlowStateCode" + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + flowStateCode + "\"/>");
                        scoreRandomXml.AppendLine("<Attribute Name=\"" + "RemindGuid" + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + guidRandomPersonRemind + "\"/>");
                        scoreRandomXml.AppendLine("</Object>");
                        scoreRandomXml.AppendLine("</System>");
                        string engineXml = scoreRandomXml.ToString();
                        if (engineXml[0].Equals("{"))
                        {
                            engineXml = engineXml.Substring(1, (engineXml.Length - 2));
                        }
                        SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient FlowEngine = new SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient();
                        // 平台打分消息
                        FlowEngine.AppMsgTriggerAsync(MessageUserID, this.AuditEntity.FormID, "您有绩效考核抽查组评分，请及时处理", this.AuditEntity.ModelCode, engineXml, guidRandomPersonRemind);
                        //1e
                        for (int j = 0; j < MessageRemind; j++)
                        {
                            if (j == 0)
                            {
                                //提醒引擎
                                RemindEngineXml(FlowEngine, randomPersonRemind[1], guidRandomPersonRemind, MessageUserID);
                            }
                            if (j == 1)
                            {
                                //提醒引擎
                                RemindEngineXml(FlowEngine, randomPersonRemind[2], guidRandomPersonRemind, MessageUserID);
                            }
                            if (j == 2)
                            {
                                //提醒引擎
                                RemindEngineXml(FlowEngine, randomPersonRemind[3], guidRandomPersonRemind, MessageUserID);
                            }

                        }

                    }
                }
            }
        }
        /// <summary>
        /// 引擎及其XML
        /// </summary>
        public void RemindEngineXml(SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient FlowEngine, string randomPersonRemind, string guidRandomPersonRemind, string MessageUserID)
        {
            DateTime strDay = DateTime.Now;
            string Day = strDay.ToString("yyyy/MM/dd", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            string Hour = strDay.AddHours(int.Parse(randomPersonRemind)).ToString("HH:mm");
            string strXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                        "<System>" +
                        "<CompanyCode></CompanyCode>" +
                        "<SystemCode></SystemCode>" +
                        "<ModelCode></ModelCode>" +
                        "<ApplicationOrderCode>" + guidRandomPersonRemind + "</ApplicationOrderCode>" +
                        "<TaskStartDate>" + Day + "</TaskStartDate>" +
                        "<TaskStartTime>" + Hour + "</TaskStartTime>" +
                        "<ProcessCycle></ProcessCycle>" +
                        "<ReceiveUser>" + MessageUserID + "</ReceiveUser>" +
                        "<ReceiveRole>Role0002</ReceiveRole>" +
                        "<MessageBody>消息主体{ReceiveUser}</MessageBody>" +
                        "<MsgLinkUrl><TTT></TTT>链接Url</MsgLinkUrl>" +
                        "<ProcessWcfUrl></ProcessWcfUrl>" +
                        "<WcfFuncName>EventTriggerProcess</WcfFuncName>" +
                        "<WcfFuncParamter>" +
                            "<FormID>" + guidRandomPersonRemind + "</FormID>" +
                            "<ReceiveUser>" + MessageUserID + "</ReceiveUser>" +
                            "<Content>距离抽查打分还有" + randomPersonRemind + " 小时请迟快处理</Content>" +
                            "<Url></Url>" +
                        "</WcfFuncParamter>" +
                        "<WcfParamSplitChar>Г</WcfParamSplitChar>" +
                        "<WcfBinding>basicHttpBinding</WcfBinding>" +
                        "</System>";
            //调用引擎
            FlowEngine.SaveEventDataAsync(strXml);

        }
        // 1e
        /// <summary>
        /// 初始化参数
        /// </summary>
        private void InitParameter()
        {
            IsAuditUser = false;
            //IsShowList = false;
            IsShowForm = false;

            this.NextStateCode = "";
            this.NextUserID = "";
            this.NextUserName = "";
            this.AuditRemark = "";
            // GotoState(AuditFormViewState.AuditStart);
        }

        /// <summary>
        /// 1. 通过 AuditEntity 从流程服务中获取流的记录
        /// etc:由控件用户执行此方法,First
        /// </summary>
        public void BindingData()
        {
            #region beyond 测试
            //if (AuditEntity.CreateCompanyID == "bac05c76-0f5b-40ae-b73b-8be541ed35ed")
            //{
            //    this.expanderTest.Visibility = System.Windows.Visibility.Visible;
            //}
            #endregion 

            txtAuditId.Text = string.Empty;
            txtAuditName.Text = string.Empty;
            this.LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
            if (!AuditCheck())
            {
                return;
            }
            InitParameter();
           // GetIsFreeFlowAndIsCancel();
            AuditService.GetFlowInfoAsync(this.AuditEntity.FormID, "", "", "", this.AuditEntity.ModelCode,
                                            "", "");

        }
        void auditService_GetFlowInfoCompleted(object sender, GetFlowInfoCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    this.LayoutRoot.Visibility = System.Windows.Visibility.Visible;
                    //为NULL 代表为新记录
                    if (e.Result == null)
                    {
                        #region 是否可以使用自选流程

                        //if (IsUserFreeFlow)
                        //{//使用自选流程
                        //    RdbAuditFree.Visibility = Visibility.Visible;
                        //    EndAuditPnl.Visibility = Visibility.Visible;
                        //    SelectAuditPersonPnl.Visibility = Visibility.Visible;
                        //}
                        //else
                        //{
                        //    RdbAuditFree.Visibility = Visibility.Collapsed;
                        //    EndAuditPnl.Visibility = Visibility.Collapsed;
                        //    SelectAuditPersonPnl.Visibility = Visibility.Collapsed;
                        //}
                        #endregion
                        //若无结果则显示 无信息面板
                        //TempPanel.Visibility = Visibility.Visible;
                        //GotoState(AuditFormViewState.AuditStart);
                        GetIsFreeFlowAndIsCancel(AuditFormViewState.AuditStart);
                        return;
                    }
                    else
                    {
                        //获取审核信息列表,若无审核记录则表明用户未曾提交过流程，否则 则可能为审核中或完成
                        var items = from item in e.Result
                                    where item.STATECODE.ToUpper() != "STARTFLOW"
                                    orderby item.FLAG descending, item.EDITDATE
                                    select item;
                        //获取当前节点之前所有的的节点 用于系统自动打分
                        AllAuditEntityList = e.Result.ToList();

                        if (items.ToList().Count > 0)
                        {
                            #region 数据审核
                            this.AuditEntityList = items.ToList();
                            TempPanel.Visibility = Visibility.Collapsed;
                            //审核记录
                            List<FLOW_FLOWRECORDDETAIL_T> list = this.AuditEntityList;

                            //获取第一个符合条件的实体数据,判断审核人
                            FLOW_FLOWRECORDDETAIL_T currentFlow = this.AuditEntityList.FirstOrDefault(item =>
                            {
                                //当前用户是否为审核用户
                                bool bUser = (item.EDITUSERID == AuditEntity.EditUserID || item.AGENTUSERID == AuditEntity.EditUserID);
                                bool bResult = item.FLAG == "0";//0：未处理，1：已处理
                                return bResult && bUser;
                            });
                            //若不为NULL 则表示需要当前用户进行审批
                            if (currentFlow != null)
                            {
                                //判断流程审批与逐级审批
                                IsFixedFlow = (currentFlow.FLOW_FLOWRECORDMASTER_T.FLOWSELECTTYPE == "0") ? true : false;
                                if (IsFixedFlow)
                                {
                                    GotoState(AuditFormViewState.Auditing);
                                }
                                else
                                {
                                    GotoState(AuditFormViewState.FreeAuditing);
                                }

                                list.Remove(currentFlow);
                                AuditEntity.ModelCode = currentFlow.FLOW_FLOWRECORDMASTER_T.MODELCODE;
                                AuditEntity.GUID = currentFlow.FLOWRECORDDETAILID;
                                currentFLOWRECORDDETAIL = currentFlow;
                            }
                            else
                            {
                                //判断流程是否为终审通过
                                FLOW_FLOWRECORDDETAIL_T AuditingFlow = this.AuditEntityList.FirstOrDefault(item2 =>
                                {
                                    return item2.FLOW_FLOWRECORDMASTER_T.CHECKSTATE == "1";
                                });

                                //若为终审则修改其最后一条数据为终审状态
                                if (AuditingFlow == null)
                                {
                                    FLOW_FLOWRECORDDETAIL_T temp = list[list.Count - 1];
                                    if (temp.CHECKSTATE == "0")
                                        temp.CHECKSTATE = "3";
                                    else if (temp.CHECKSTATE == "1")
                                        temp.CHECKSTATE = "4";

                                    list.RemoveAt(list.Count - 1);
                                    list.Add(temp);
                                }

                                GotoState(AuditFormViewState.End);
                                IsAuditUser = false;
                            }
                            //Modify by 安凯航 2011年5月21日
                            //修改审核信息显示,改为按照不同的提交过程分组.并始终显示提交人
                            //获取审核记录并根据审核记录显示记录列表
                            //this.IsShowList = list.Count == 0 ? false : true;
                            //绑定审核记录信息
                            //if (list.Count > 0)
                            //{

                            /* 原来的代码
                            List<FLOW_FLOWRECORDMASTER_T> tdlist = new List<FLOW_FLOWRECORDMASTER_T>();
                            tdlist.Add(list[0].FLOW_FLOWRECORDMASTER_T);
                            TDInfo.ItemsSource = tdlist;
                            this.AuditListBox.ItemsSource = list; */
                            //}
                            //修改后代码
                            var auditResults = from t in e.Result
                                               group t by t.FLOW_FLOWRECORDMASTER_T
                                                   into g
                                                   orderby g.Key.CREATEDATE
                                                   select g.Key;
                            foreach (var item in auditResults)
                            {
                                //排除流程起始的默认数据
                                FLOW_FLOWRECORDDETAIL_T sub = item.FLOW_FLOWRECORDDETAIL_T.FirstOrDefault(p => p.STATECODE.ToUpper() == "STARTFLOW");
                                item.FLOW_FLOWRECORDDETAIL_T.Remove(sub);
                                //暂时屏蔽,不确定是否需要.2011年5月24日
                                //sub = item.FLOW_FLOWRECORDDETAIL_T.FirstOrDefault(p => p.EDITUSERID == SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID && p.CHECKSTATE == "2");
                                //item.FLOW_FLOWRECORDDETAIL_T.Remove(sub);//排出当前审核人是自己的数据

                                //审核记录按照时间先后顺序排序.
                                ObservableCollection<FLOW_FLOWRECORDDETAIL_T> details = new ObservableCollection<FLOW_FLOWRECORDDETAIL_T>();
                                //foreach (var detail in item.FLOW_FLOWRECORDDETAIL_T.OrderBy(p => p.EDITDATE))
                                //{
                                //    details.Add(detail);
                                //}



                                List<FLOW_FLOWRECORDDETAIL_T> source = item.FLOW_FLOWRECORDDETAIL_T.Where(d => d.FLAG == "1").OrderBy(d => d.EDITDATE).ToList();
                                source.AddRange(item.FLOW_FLOWRECORDDETAIL_T.Where(d => d.FLAG != "1").OrderBy(d => d.EDITDATE).ToList());

                                foreach (var detail in source)
                                {
                                    details.Add(detail);
                                }
                                item.FLOW_FLOWRECORDDETAIL_T = details;
                                //foreach (var detail in details)
                                //{
                                //    item.FLOW_FLOWRECORDDETAIL_T.Add(detail);
                                //}
                            }
                            if (auditResults.Count() > 0)
                            {
                                //显示代理人beyond
                                auditResults.ForEach(master =>
                                {
                                    if (master.FLOW_FLOWRECORDDETAIL_T != null)
                                    {
                                        master.FLOW_FLOWRECORDDETAIL_T.ForEach(detail =>
                                        {
                                            if (!string.IsNullOrEmpty(detail.AGENTERNAME))
                                            {
                                                detail.EDITUSERNAME = detail.EDITUSERNAME + "(" + detail.AGENTERNAME + ")";
                                            }
                                        });
                                    }
                                });
                                this.AuditListPnl.ItemsSource = auditResults;
                                this.AuditListPnl.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                this.AuditListPnl.ItemsSource = null;
                            }
                            //end Modify.

                            #endregion
                        }
                        else
                        {
                            #region 使用自选流程

                            //if (IsUserFreeFlow)
                            //{//使用自选流程
                            //    RdbAuditFree.Visibility = Visibility.Visible;
                            //    EndAuditPnl.Visibility = Visibility.Visible;
                            //    SelectAuditPersonPnl.Visibility = Visibility.Visible;
                            //}
                            //else
                            //{
                            //    RdbAuditFree.Visibility = Visibility.Collapsed;
                            //    EndAuditPnl.Visibility = Visibility.Collapsed;
                            //    SelectAuditPersonPnl.Visibility = Visibility.Collapsed;
                            //}
                            #endregion
                            #region 数据未曾提交成功,需要重新 提交审核
                            //GotoState(AuditFormViewState.AuditStart);
                            GetIsFreeFlowAndIsCancel(AuditFormViewState.AuditStart);
                            //IsFixedFlow = true;
                            #endregion
                        }

                        #region beyond
                        this.ExtendLoad(AllAuditEntityList);
                        #endregion
                    }
                    if (OnBindingDataCompleted.IsNotNull())
                        OnBindingDataCompleted(this, EventArgs.Empty);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    //未提交单据
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                
            }
            ////beyond
            //if (this.tbTest.Text != "")
            //{
            //    //beyond 记录日记
            //    string message = "GetFlowInfoCompleted:" +  DateTime.Now.ToString() + "\n";
            //    this.tbTest.Text =this.tbTest.Text+message;
            //}
          
        }

        #region 判断是否可能用自选流程或提单人可以撤回流程
        /// <summary>
        /// 判断是否可能用自选流程或提单人可以撤回流程
        /// </summary>
        private void GetIsFreeFlowAndIsCancel(AuditFormViewState viewState)
        {
            SMT.Saas.Tools.FlowWFService.ServiceClient client = new ServiceClient();

            client.IsFreeFlowAndIsCancelAsync(AuditEntity.ModelCode, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            client.IsFreeFlowAndIsCancelCompleted += (obj, e) =>
            {
                // 判断是否可能用自选流程或提单人可以撤回流程
                // string[0]=1 可以用自选流程
                // string[1]=1 提交人可以撤回流程,当前登录人=提单人才可以撤回，而且该单还是在审核中的
                if (e.Error == null)
                {
                    if (e.Result[0] == "1")
                    {
                        IsCanCancel = true;
                        //RdbAuditFree.Visibility = Visibility.Visible;
                        //EndAuditPnl.Visibility = Visibility.Visible;
                        //SelectAuditPersonPnl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        IsCanCancel = false;
                        //RdbAuditFree.Visibility = Visibility.Collapsed;
                        //EndAuditPnl.Visibility = Visibility.Collapsed;
                        //SelectAuditPersonPnl.Visibility = Visibility.Collapsed; 
                    }
                    if (e.Result[1] == "1")
                    {
                        IsUserFreeFlow = true;
                    }
                    #region 是否使用自选流程
                    if (IsUserFreeFlow)
                    {//使用自选流程
                        GotoState(viewState);
                        //RdbAuditFree.Visibility = Visibility.Visible;
                        //EndAuditPnl.Visibility = Visibility.Visible;
                        //SelectAuditPersonPnl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //RdbAuditFree.Visibility = Visibility.Collapsed;
                        //EndAuditPnl.Visibility = Visibility.Collapsed;
                        //SelectAuditPersonPnl.Visibility = Visibility.Collapsed;
                        if (AuditFormViewState.AuditStart == viewState)
                        {
                            IsShowAuditButtonPnl = false;
                            IsShowAuditTypePnl = true;
                            //IsShowList = true;
                            IsShowForm = false;
                            IsShowEndAuditPnl = false;
                            IsShowSelectAuditPersonPnl = false;
                            IsFixedFlow = true;
                            RdbAuditFree.Visibility = Visibility.Collapsed;
                            EndAuditPnl.Visibility = Visibility.Collapsed;
                            SelectAuditPersonPnl.Visibility = Visibility.Collapsed;
                        }
                    }
                    #endregion
                }
            };
        }
        #endregion

        #endregion

        #region 内部事件/方法
        /// <summary>
        /// 视图状态转换
        /// </summary>
        /// <param name="viewState">审核控件视图状态</param>
        public void GotoState(AuditFormViewState viewState)
        {
            switch (viewState)
            {
                case AuditFormViewState.AuditStart:
                    {
                        IsShowAuditButtonPnl = false;
                        IsShowAuditTypePnl = true;
                        //IsShowList = true;
                        IsShowForm = false;
                        IsShowEndAuditPnl = false;
                        IsShowSelectAuditPersonPnl = false;
                        IsFixedFlow = true;
                        break;
                    }
                case AuditFormViewState.FreeAuditStart:
                    {
                        IsShowAuditButtonPnl = false;
                        IsShowAuditTypePnl = true;
                        //IsShowList = true;
                        IsShowForm = false;
                        IsShowEndAuditPnl = false;
                        IsShowSelectAuditPersonPnl = true;
                        IsFixedFlow = false;
                        break;
                    }
                case AuditFormViewState.Auditing:
                    {
                        IsShowAuditButtonPnl = true;
                        //IsShowList = true;
                        IsShowForm = true;
                        IsShowAuditTypePnl = false;
                        IsShowEndAuditPnl = false;
                        IsShowSelectAuditPersonPnl = false;
                        break;
                    }
                case AuditFormViewState.FreeAuditing:
                    {
                        IsShowAuditTypePnl = false;
                        IsShowAuditButtonPnl = true;
                        //IsShowList = true;
                        IsShowForm = true;
                        IsShowEndAuditPnl = true;
                        IsShowSelectAuditPersonPnl = true;
                        break;
                    }
                case AuditFormViewState.End:
                    {
                        IsShowAuditButtonPnl = false;
                        IsShowAuditTypePnl = false;
                        IsShowForm = false;
                        IsShowEndAuditPnl = false;
                        IsShowSelectAuditPersonPnl = false;
                        //IsShowList = true;
                        break;
                    }
                default:
                    {
                        TempPanel.Visibility = Visibility.Visible;
                        break;
                    }
            }
        }

        public void ClickTest()
        {
            btnSuccess_Click(null, null);
        }
        /// <summary>
        /// 审核通过
        /// </summary>
        private void btnSuccess_Click(object sender, RoutedEventArgs e)
        {
            if (radOk.IsChecked == false && radNo.IsChecked == false)
            {
                ComfirmWindow.ConfirmationBox("提示信息", "请选择[审核通过]或[审核不通过]", "确定");
                return;
            }
            if (radOk.IsChecked == true)
            {// 审核通过
                HandIn(AuditOperation.Update, AuditAction.Pass);
            }
            if (radNo.IsChecked == true)
            {// 审核不通过
                HandIn(AuditOperation.Update, AuditAction.Fail);
            }
        }
        /// <summary>
        /// 取消(原来是审核不通过)
        /// </summary>
        private void btnFail_Click(object sender, RoutedEventArgs e)
        {
            radOk.IsChecked = false;
            radNo.IsChecked = false;
            txRemark.Text = "";
            // HandIn(AuditOperation.Update, AuditAction.Fail);
        }
        /// <summary>
        /// 取消
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (AuditCompleted != null)
            {
                AuditCompleted(this, new AuditEventArgs(AuditEventArgs.AuditResult.Cancel, null));
            }
        }

        private void CanAudit(bool isOnOrOff)
        {
            this.btnSuccess.IsEnabled = isOnOrOff;
            this.btnFail.IsEnabled = isOnOrOff;
        }
        /// <summary>
        /// 选择审核方式
        /// </summary>
        private void RdbAuditModel_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton str = (sender as RadioButton);
            if ((bool)str.IsChecked)
            {
                GotoState(AuditFormViewState.AuditStart);
                if (AuditSubmitData.IsNotNull())
                    AuditSubmitData.FlowSelectType = FlowSelectType.FixedFlow;
            }
        }
        private void RdbAuditFree_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton str = (sender as RadioButton);
            if ((bool)str.IsChecked)
            {
                GotoState(AuditFormViewState.FreeAuditStart);
                if (AuditSubmitData.IsNotNull())
                    AuditSubmitData.FlowSelectType = FlowSelectType.FreeFlow;
            }
        }
        /// <summary>
        /// 选择审核人
        /// </summary>
        private void btnLookUpDepartment_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    if (ent.Count > 1)
                    {
                        txtAuditId.Text = string.Empty;
                        txtAuditName.Text = string.Empty;
                        NextUserID = string.Empty;
                        NextUserName = string.Empty;
                        ComfirmWindow.ConfirmationBox("", "只能选择一项", Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj result = ent.FirstOrDefault();
                    if (result.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
                    {
                        txtAuditId.Text = result.ObjectID;
                        txtAuditName.Text = result.ObjectName;
                        //AuditEntity.EditUserID = result.ObjectID;
                        //AuditEntity.EditUserName = result.ObjectName;
                        NextUserID = result.ObjectID;
                        NextUserName = result.ObjectName;
                        var postObject = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)result.ParentObject;
                        if (postObject.IsNotNull())
                        {
                            NextPostID = postObject.ObjectID;
                            var deptObject = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)postObject.ParentObject;
                            if (deptObject.IsNotNull())
                            {
                                NextDepartmentID = deptObject.ObjectID;
                                var compObject = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)deptObject.ParentObject;
                                if (compObject.IsNotNull())
                                {
                                    NextCompanyID = compObject.ObjectID;
                                }
                                else
                                {
                                    var cmp = deptObject.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                                    NextCompanyID = cmp.T_HR_COMPANY.COMPANYID;

                                }
                            }

                        }
                    }
                    else
                    {
                        txtAuditId.Text = string.Empty;
                        txtAuditName.Text = string.Empty;
                        NextUserID = string.Empty;
                        NextUserName = string.Empty;
                        ComfirmWindow.ConfirmationBox("", "请选择人员", Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                }

                //if (ent != null && ent.Count > 0)
                //{
                //    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                //    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)companyInfo.ParentObject;
                //    string postid = post.ObjectID;
                //    //  fromPostLevel=(post as SMT.Saas.Tools.OrganizationWS.T_HR_POST).POSTLEVEL.ToString();

                //    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                //    string deptid = dept.ObjectID;

                //    // ExtOrgObj corp = (ExtOrgObj)dept.ParentObject;
                //    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                //    string corpid = corp.COMPANYID;
                //}

            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        /// <summary>
        /// 是否为终审
        /// </summary>
        private void ckbIsEndAudit_Click(object sender, RoutedEventArgs e)
        {
            CheckBox ckb = sender as CheckBox;
            if ((bool)ckb.IsChecked)
            {
                IsEndAudit = true;
                IsShowSelectAuditPersonPnl = false;
            }
            else
            {
                IsEndAudit = false;
                IsShowSelectAuditPersonPnl = true;
            }
        }
        #endregion



        #region beyond
        /// <summary>
        /// 设置是否显示撤单
        /// </summary>
        public Visibility ShowCancelSubmit
        {
            set
            {
                this.AuditButtonCancel.Visibility = value;
            }
        }

        #region process Bar
        SMTLoading loadbar = null;

        public void ShowProcess()
        {
            if (this.loadbar == null)
            {
                loadbar = new SMTLoading();
                this.gridLoading.Children.Add(loadbar);
            }
            loadbar.Start();//调用服务时写
        }
        public void CloseProcess()
        {
            if (loadbar != null)
            {
                loadbar.Stop();
            }

        }
        #endregion

        private void ExtendLoad(List<FLOW_FLOWRECORDDETAIL_T> listDetail)
        {
            this.LoadConsultation(listDetail);
            this.LoadCancel(listDetail);
            this.LoadReSubmit(listDetail);
        }
        private void LoadConsultation(List<FLOW_FLOWRECORDDETAIL_T> listDetail)
        {

           
            SubmitData submmitData = new SubmitData();
            submmitData.FormID = AuditEntity.FormID;
            submmitData.ModelCode = AuditEntity.ModelCode;
            submmitData.ApprovalUser = new UserInfo();
            submmitData.ApprovalUser.CompanyID = AuditEntity.CreateCompanyID;

            submmitData.ApprovalUser.DepartmentID = AuditEntity.CreateDepartmentID;
            submmitData.ApprovalUser.PostID = AuditEntity.CreatePostID;
            submmitData.ApprovalUser.UserID = AuditEntity.CreateUserID;
            submmitData.ApprovalUser.UserName = AuditEntity.CreateUserName;
            submmitData.ApprovalContent = AuditRemark;
            submmitData.NextStateCode = "";
            submmitData.NextApprovalUser = new UserInfo();
            submmitData.NextApprovalUser.CompanyID = NextCompanyID;
            submmitData.NextApprovalUser.DepartmentID = NextDepartmentID;
            submmitData.NextApprovalUser.PostID = NextPostID;
            submmitData.NextApprovalUser.UserID = NextUserID;
            submmitData.NextApprovalUser.UserName = NextUserName;
            submmitData.XML = this.XmlObject;
            //submmitData.SubmitFlag =SubmitFlag.Approval;
            string loginUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            string loginUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;

            #region 发送的咨询
            List<ConsultationViewModel> listSend = new List<ConsultationViewModel>();
            //登录人已发送的咨询
            listDetail.Where(detail => (detail.EDITUSERID == loginUserID || detail.AGENTUSERID == loginUserID) && detail.FLOW_CONSULTATION_T != null).ForEach(item =>
            {
                item.FLOW_CONSULTATION_T.ForEach(consultation =>
                {
                    ConsultationViewModel viewModel = new ConsultationViewModel(consultation, loginUserID, submmitData);
                    viewModel.WaitingHandler += new EventHandler(viewModel_WaitingHandler);
                    viewModel.CompletedHandler += new EventHandler(viewModel_CompletedHandler);
                    listSend.Add(viewModel);
                });
            });
            //登录人发起新咨询
            if (this.currentFLOWRECORDDETAIL != null)
            {
                if (listSend.FirstOrDefault(v => v.FlowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOWRECORDDETAILID == this.currentFLOWRECORDDETAIL.FLOWRECORDDETAILID) == null)
                {
                    FLOW_CONSULTATION_T consultation = new FLOW_CONSULTATION_T();
                    consultation.CONSULTATIONID = Guid.NewGuid().ToString();
                    consultation.CONSULTATIONUSERID = loginUserID;
                    consultation.CONSULTATIONUSERNAME = loginUserName;
                    consultation.FLAG = "0";
                    consultation.FLOW_FLOWRECORDDETAIL_T = this.currentFLOWRECORDDETAIL;
                    if (this.currentFLOWRECORDDETAIL.FLOW_CONSULTATION_T == null)
                    {
                        this.currentFLOWRECORDDETAIL.FLOW_CONSULTATION_T = new ObservableCollection<FLOW_CONSULTATION_T>();
                    }
                    this.currentFLOWRECORDDETAIL.FLOW_CONSULTATION_T.Add(consultation);
                    ConsultationViewModel viewModel = new ConsultationViewModel(consultation, loginUserID, submmitData);
                    viewModel.WaitingHandler += new EventHandler(viewModel_WaitingHandler);
                    viewModel.CompletedHandler += new EventHandler(viewModel_CompletedHandler);
                    listSend.Add(viewModel);
                }


            }

            if (listSend.Count > 0)
            {
               // this.expanderConsultationSend.IsExpanded = true;//龙康才:有咨询信息时默认展开
                this.expanderConsultationSend.Visibility = System.Windows.Visibility.Visible;
                this.ConsultationListSend.ItemsSource = listSend;
            }
            else
            {
                this.expanderConsultationSend.Visibility = System.Windows.Visibility.Collapsed;
            }
            #endregion 

            #region 回复咨询
            List<ConsultationViewModel> listReply = new List<ConsultationViewModel>();
            listDetail.Where(detail => detail.FLOW_CONSULTATION_T != null).ForEach(item =>
            {
                item.FLOW_CONSULTATION_T.ForEach(consultation =>
                {
                    if (consultation.REPLYUSERID == loginUserID)
                    {
                        ConsultationViewModel viewModel = new ConsultationViewModel(consultation, loginUserID, submmitData);
                        viewModel.WaitingHandler += new EventHandler(viewModel_WaitingHandler);
                        viewModel.CompletedHandler += new EventHandler(viewModel_CompletedHandler);
                        listReply.Add(viewModel);
                    }
                });
            });
            if (listReply.Count > 0)
            {
                this.expanderConsultationSend.IsExpanded = true;//龙康才:有回复内容,咨询人的咨询信息时默认展开
                this.expanderConsultationReply.IsExpanded = true;//龙康才:有回复内容信息时默认展开
                this.expanderConsultationReply.Visibility = System.Windows.Visibility.Visible;
                this.ConsultationListReply.ItemsSource = listReply;
            }
            else
            {
                this.expanderConsultationReply.Visibility = System.Windows.Visibility.Collapsed;
            }
            #endregion 

            #region old

            //List<ConsultationViewModel> listViewModel = new List<ConsultationViewModel>();
            //#region 找出所有登录人的咨询


            //listDetail.Where(detail => (detail.EDITUSERID == loginUserID || detail.AGENTUSERID == loginUserID) && detail.FLOW_CONSULTATION_T != null).ForEach(item =>
            //{
            //    item.FLOW_CONSULTATION_T.ForEach(consultation =>
            //    {
            //        ConsultationViewModel viewModel = new ConsultationViewModel(consultation, loginUserID, submmitData);
            //        viewModel.WaitingHandler += new EventHandler(viewModel_WaitingHandler);
            //        viewModel.CompletedHandler += new EventHandler(viewModel_CompletedHandler);
            //        listViewModel.Add(viewModel);
            //    });
            //});


            //#endregion

            //#region 登录人发起咨询
            //if (this.currentFLOWRECORDDETAIL != null)
            //{
            //    if (listViewModel.FirstOrDefault(v => v.FlowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOWRECORDDETAILID == this.currentFLOWRECORDDETAIL.FLOWRECORDDETAILID) == null)
            //    {
            //        FLOW_CONSULTATION_T consultation = new FLOW_CONSULTATION_T();
            //        consultation.CONSULTATIONID = Guid.NewGuid().ToString();
            //        consultation.CONSULTATIONUSERID = loginUserID;
            //        consultation.CONSULTATIONUSERNAME = loginUserName;
            //        consultation.FLAG = "0";
            //        consultation.FLOW_FLOWRECORDDETAIL_T = this.currentFLOWRECORDDETAIL;
            //        if (this.currentFLOWRECORDDETAIL.FLOW_CONSULTATION_T == null)
            //        {
            //            this.currentFLOWRECORDDETAIL.FLOW_CONSULTATION_T = new ObservableCollection<FLOW_CONSULTATION_T>();
            //        }
            //        this.currentFLOWRECORDDETAIL.FLOW_CONSULTATION_T.Add(consultation);
            //        ConsultationViewModel viewModel = new ConsultationViewModel(consultation, loginUserID, submmitData);
            //        viewModel.WaitingHandler += new EventHandler(viewModel_WaitingHandler);
            //        viewModel.CompletedHandler += new EventHandler(viewModel_CompletedHandler);
            //        listViewModel.Add(viewModel);
            //    }


            //}
            //#endregion

            //if (listViewModel.Count == 0)
            //{

            //    #region 登录人为被咨询人
            //    listDetail.Where(detail => detail.FLOW_CONSULTATION_T != null).ForEach(item =>
            //    {
            //        item.FLOW_CONSULTATION_T.ForEach(consultation =>
            //        {
            //            if (consultation.REPLYUSERID == loginUserID)
            //            {
            //                ConsultationViewModel viewModel = new ConsultationViewModel(consultation, loginUserID, submmitData);
            //                viewModel.WaitingHandler += new EventHandler(viewModel_WaitingHandler);
            //                viewModel.CompletedHandler += new EventHandler(viewModel_CompletedHandler);
            //                listViewModel.Add(viewModel);
            //            }
            //        });
            //    });
            //    #endregion
            //}

            //if (listViewModel.Count > 0)
            //{
            //    this.expanderConsultation.Visibility = System.Windows.Visibility.Visible;
            //    this.ConsultationList.ItemsSource = listViewModel;
            //}
            //else
            //{
            //    this.expanderConsultation.Visibility = System.Windows.Visibility.Collapsed;
            //}
            #endregion 

            if (listDetail.FirstOrDefault(detail => detail.EDITUSERID == loginUserID || detail.AGENTUSERID == loginUserID) != null)
            {
                this.gridAudit.Visibility = System.Windows.Visibility.Visible;
            }            
            else
            {
                //this.gridAudit.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (listDetail != null && listDetail.Count > 0 &&
                (listDetail[0].FLOW_FLOWRECORDMASTER_T.CHECKSTATE == "2" || listDetail[0].FLOW_FLOWRECORDMASTER_T.CHECKSTATE == "3"))
            {
                this.expanderConsultationSend.Visibility = System.Windows.Visibility.Collapsed;
            }

        }

        void viewModel_CompletedHandler(object sender, EventArgs e)
        {
            //this.ConsultationLoading.Stop();

            this.CloseProcess();
            this.BindingData();
        }

        void viewModel_WaitingHandler(object sender, EventArgs e)
        {
            this.ShowProcess();
            //this.BindingData();
            //this.ConsultationLoading.Start();
        }
        #region 是否可以撤回
        /// <summary>
        /// 是否可以撤回（只有提单人和单据在审核中才可又撤单）
        /// </summary>
        /// <param name="listDetail"></param>
        private void LoadCancel(List<FLOW_FLOWRECORDDETAIL_T> listDetail)
        {
            if (listDetail != null && listDetail.Count > 0)
            {
                string loginUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                for (int i = 0; i < listDetail.Count; i++)
                {
                    FLOW_FLOWRECORDMASTER_T master = listDetail[i].FLOW_FLOWRECORDMASTER_T;
                    if (master.CREATEUSERID == loginUserID && master.CHECKSTATE == "1")
                    {
                        if (IsCanCancel)
                        {
                            //this.btnCancelSubmit.Visibility = System.Windows.Visibility.Visible; //暂时关闭撤单按钮功能，等待业务系统有了撤单功能，才能开放
                        }
                        //this.btnCancelSubmit.Visibility = System.Windows.Visibility.Visible;
                        return;
                    }
                }

            }
            this.btnCancelSubmit.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion
        private void LoadReSubmit(List<FLOW_FLOWRECORDDETAIL_T> listDetail)
        {
            //if (listDetail != null && listDetail.Count > 0)
            //{
            //    string loginUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    FLOW_FLOWRECORDMASTER_T master = listDetail[0].FLOW_FLOWRECORDMASTER_T;
            //    if (master.CREATEUSERID == loginUserID && master.CHECKSTATE == "5")
            //    {
            //        this.expanderReSubmit.Visibility = System.Windows.Visibility.Visible;
            //        return;
            //    }
            //}
            //this.expanderReSubmit.Visibility = System.Windows.Visibility.Collapsed;
        }

        private bool bCancel = false;
        private void btnCancel_Click_1(object sender, RoutedEventArgs e)
        {
            bCancel = true;
            //this.HandIn(AuditOperation.Cancel, AuditAction.Cancel);


        }


        private bool bReSubmit = false;
        private void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            bReSubmit = true;
            //this.HandIn(AuditOperation.ReSubmit, AuditAction.ReSubmit);

        }
        private Dictionary<Role_UserType, ObservableCollection<UserInfo>> DictCounterUser;
        private void CounterAction(DataResult result)
        {
            this.isSelect = false;
            this.selectDataresult = result;
            NextStateCode = result.AppState;
            List<Rule_UserInfoViewModel> listviewmodel = new List<Rule_UserInfoViewModel>();
            result.DictCounterUser.Keys.ForEach(key =>
            {
                Rule_UserInfoViewModel vm = new Rule_UserInfoViewModel(key, result.DictCounterUser[key].ToList());
                listviewmodel.Add(vm);
            });
            //this.ListCountersign.ItemsSource = listviewmodel;
            //AuditEventArgs args = new AuditEventArgs(AuditEventArgs.AuditResult.Error, result);
            //args.StartDate = this.AuditEntity.StartDate;
            //args.EndDate = System.DateTime.Now;
            //OnAuditCompleted(this, args);
            //this.BindingData();
            DataTemplate CountersignTemplate = this.Resources["CountersignTemplate"] as DataTemplate;
            Style listboxStyle = this.Resources["ListBoxItemStyle1"] as Style;
            System.Windows.Controls.Window winSelector = new System.Windows.Controls.Window();
            winSelector.Unloaded+=new RoutedEventHandler(winSelector_Unloaded);
            winSelector.MinHeight = 400;
            winSelector.Width = 400;
            //winSelector.Resources.Add("UserInfoTemplate", this.Resources["UserInfoTemplate"]);
            //winSelector.Resources.Add("ListBoxItemStyle1", this.Resources["ListBoxItemStyle1"]);
            //winSelector.Resources.Add("CountersignTemplate", this.Resources["CountersignTemplate"]);

            //winSelector.Width = 400;
            winSelector.TitleContent = "确认审核人";

            Grid gridSelector = new Grid();
            RowDefinition r1 = new RowDefinition();
            RowDefinition r2 = new RowDefinition();
            RowDefinition r3 = new RowDefinition();


            r1.Height = new GridLength(50, GridUnitType.Auto);
            r2.Height = new GridLength(30, GridUnitType.Auto);
            r3.Height = new GridLength(50, GridUnitType.Auto);
            gridSelector.RowDefinitions.Add(r1);
            gridSelector.RowDefinitions.Add(r2);
            gridSelector.RowDefinitions.Add(r3);
            TextBlock tb = new TextBlock();
            tb.Height = 26;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (result.CountersignType == "0")
            {
                tb.Text = "请为每个角色至少选择一人，并按确认提交。";
            }
            else
            {
                tb.Text = "请至少选择一人，并按确认提交。";
            }
            tb.SetValue(Grid.RowProperty, 0);

            ListBox listboxCountersign = new ListBox();

            listboxCountersign.ItemTemplate = CountersignTemplate;
            listboxCountersign.ItemContainerStyle = listboxStyle;
            listboxCountersign.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            listboxCountersign.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            listboxCountersign.SetValue(Grid.RowProperty, 1);
            listviewmodel.ForEach(item => {
                item.ListUserInfo.ForEach(ent => {
                    ent.UserInfo.CompanyName = ent.UserInfo.UserName + "(" + ent.UserInfo.CompanyName + "->" + ent.UserInfo.DepartmentName + "->" + ent.UserInfo.PostName + ")";
                    
                });
            });
            listboxCountersign.ItemsSource = listviewmodel;

            listboxCountersign.Height = 300;
            listboxCountersign.Width = 350;
            Button btnOK = new Button();
            btnOK.Content = "确认";
            btnOK.Margin = new Thickness(0, 0, 5, 10);
            btnOK.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            btnOK.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            btnOK.Width = 80;
            btnOK.Height = 26;
            btnOK.SetValue(Grid.RowProperty, 2);

            btnOK.Click += (o, e) =>
            {
                this.isSelect = true;
                #region

                #region Check
                this.DictCounterUser = new Dictionary<Role_UserType, ObservableCollection<UserInfo>>();
                if (result.CountersignType == "0")
                {
                    foreach (var viewModel in listviewmodel)
                    {
                        bool bUser = false;
                        ObservableCollection<UserInfo> listuserinfo = new ObservableCollection<UserInfo>();
                        viewModel.ListUserInfo.ForEach(user =>
                        {
                            if (user.IsCheck)
                            {
                                bUser = true;
                                listuserinfo.Add(user.UserInfo);
                            }
                        });
                        if (!bUser)
                        {
                            this.isSelect = false;
                            ComfirmWindow.ConfirmationBox("警告", "请选择角色" + viewModel.Role_UserType.Remark + "的审核人", Utility.GetResourceStr("CONFIRMBUTTON"));

                            //MessageBox.Show("请选择角色" + viewModel.Role_UserType.RoleNameName + "的审核人");
                            return;
                        }
                        this.DictCounterUser[viewModel.Role_UserType] = listuserinfo;
                    }

                }
                else
                {
                    bool bUser = false;
                    foreach (var viewModel in listviewmodel)
                    {
                        ObservableCollection<UserInfo> listuserinfo = new ObservableCollection<UserInfo>();
                        viewModel.ListUserInfo.ForEach(user =>
                        {
                            if (user.IsCheck)
                            {
                                bUser = true;
                                listuserinfo.Add(user.UserInfo);
                            }
                        });
                        this.DictCounterUser[viewModel.Role_UserType] = listuserinfo;
                    }
                    if (!bUser)
                    {
                        this.isSelect = false;
                        ComfirmWindow.ConfirmationBox("警告", "至少选择一个审核人", Utility.GetResourceStr("CONFIRMBUTTON"));
                        //MessageBox.Show("至少选择一个审核人");
                        return;
                    }
                }
                #endregion

                InnerHandIn(currAuditOperation, curAuditAction);
                
                
                winSelector.Close();
                #endregion
            };


            ContentControl parent = new ContentControl();
            parent.Content = gridSelector;

            winSelector.Content = parent;
            gridSelector.Children.Add(tb);
            gridSelector.Children.Add(listboxCountersign);
            gridSelector.Children.Add(btnOK);
            FrameworkElement fe = SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot;

            // Window.Show("", "", Guid.NewGuid().ToString(), true, false, parent, null);
            winSelector.Show<string>(DialogMode.Default, fe, "", (resulta) => { });


        }


        private void btnCancelSubmit_Click(object sender, RoutedEventArgs e)
        {
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                this.HandIn(AuditOperation.Cancel, AuditAction.Pass);
            };
            com.SelectionBox("确定撤单", "确定撤单吗？", ComfirmWindow.titlename, "");
           
        }



        #endregion






    }

    #region 流程实体、事件参数、值转换、枚举
    /// <summary>
    /// 视图状态 控制界面元素的显示与隐藏
    /// </summary>
    public enum AuditFormViewState
    {
        /// <summary>
        /// 初始默认状态，流程审批初始状态
        /// </summary>
        AuditStart,
        /// <summary>
        /// 逐级审批,初始状态
        /// </summary>
        FreeAuditStart,
        /// <summary>
        /// 流程审批
        /// </summary>
        Auditing,
        /// <summary>
        /// 逐级审批
        /// </summary>
        FreeAuditing,
        /// <summary>
        /// 结束
        /// </summary>
        End
    }
    /// <summary>
    /// 流程记录表实体
    /// </summary>
    public class Flow_FlowRecord_T : EntityObject
    {
        public Flow_FlowRecord_T()
        {
            this.CreateCompanyID = string.Empty;
            this.CreateDepartmentID = string.Empty;
            this.CreatePostID = string.Empty;
            this.CreateUserID = string.Empty;
            this.EditUserName = string.Empty;
            this.EditUserID = string.Empty;
            this.Content = string.Empty;
            this.Flag = string.Empty;
            this.FlowCode = string.Empty;
            this.FormID = string.Empty;
            this.GUID = string.Empty;
            this.InstanceID = string.Empty;
            this.ModelCode = string.Empty;
            this.ParentStateCode = string.Empty;
            this.StateCode = string.Empty;
            this.XmlObject = "";
            this.SystemCode = string.Empty;
            this.BusinessObjectDefineXML = string.Empty;
        }
        #region 流程记录表属性

        public string CreateCompanyID { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateDepartmentID { get; set; }
        /// <summary>
        /// 创建人职位名
        /// </summary>
        public string CreatePostID { get; set; }
        /// <summary>
        /// 创建用户ID
        /// </summary>
        public string CreateUserID { get; set; }
        /// <summary>
        /// 创建用户名
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 编辑时间
        /// </summary>
        public DateTime? EditDate { get; set; }
        /// <summary>
        /// 编辑人ID
        /// </summary>
        public string EditUserID { get; set; }
        /// <summary>
        /// 编辑人姓名
        /// </summary>
        public string EditUserName { get; set; }
        /// <summary>
        /// 标志
        /// </summary>
        public string Flag { get; set; }
        /// <summary>
        /// 流程编码
        /// </summary>
        public string FlowCode { get; set; }
        /// <summary>
        /// 表单ID
        /// </summary>
        public string FormID { get; set; }
        /// <summary>
        /// 标识ID
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// 实例编码
        /// </summary>
        public string InstanceID { get; set; }
        /// <summary>
        /// 模块编码
        /// </summary>
        public string ModelCode { get; set; }
        /// <summary>
        /// 父状态编码
        /// </summary>
        public string ParentStateCode { get; set; }
        /// <summary>
        /// 状态编码
        /// </summary>
        public string StateCode { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 流程关联的XML数据
        /// </summary>
        public string XmlObject { get; set; }

        /// <summary>
        /// 系统代码
        /// </summary>

        public string SystemCode { get; set; }

        /// <summary>
        /// 模块对应的业务对象定义
        /// </summary>
        public string BusinessObjectDefineXML { get; set; }

        #endregion
    }
    /// <summary>
    /// 审核事件参数
    /// </summary>
    public class AuditEventArgs : EventArgs
    {
        public AuditEventArgs(AuditResult auditResult, DataResult innerResult)
        {
            result = auditResult;
            InnerResult = innerResult;
            this.Action = AuditAction.Audit;
        }
        private AuditResult result = AuditResult.Cancel;
        public AuditResult Result
        {
            get
            {
                return result;
            }
            set
            {
                result = value;
            }
        }

        public AuditAction Action { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DataResult InnerResult
        {
            get;
            set;
        }

        /// <summary>
        /// Auditing : 审核中
        /// Successful : 终审通过
        /// Fail : 终审不通过
        /// Cancel : 取消当前操作
        /// Error : 审核提交异常
        /// CancelSubmit:撤单
        /// </summary>
        public enum AuditResult
        {
            Auditing, Successful, Fail, Cancel, Error, Saved, CancelSubmit=9
        }
        /// <summary>
        /// 审核动作
        /// Submit : 提交
        /// Audit : 审核
        /// </summary>
        public enum AuditAction
        {
            Submit, Audit
        }
    }

    #region 值转换
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string source = (string)value;
            if (source == "3")
                source = "0";
            else if (source == "4")
                source = "1";

            #region beyond
            if (source == "9")
            {
                source = "1";
            }
            #endregion
            string uri = string.Format("/SMT.SaaS.FrameworkUI;Component/Images/Resources/{0}.png", (string)source);
            return uri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string source = (string)value;
            string uri = "";
            switch (source)
            {
                case "0":
                    uri = "审核不通过";
                    break;
                case "1":
                    uri = "审核通过";
                    break;
                case "2":
                    uri = "审核中";
                    break;
                case "3":
                    uri = "终审不通过";
                    break;
                case "4":
                    uri = "终审通过";
                    break;
                #region beyond
                case "9":
                    uri = "撤单";
                    break;
                case "6":
                    uri = "重新提交";
                    break;
                case "7":
                    uri = "会签通过";
                    break;
                case "8":
                    uri = "会签不通过";
                    break;
                #endregion

                default:
                    uri = "";
                    break;
            }

            return uri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            if (value.GetType().IsDateTime())
            {

                string dateForm = parameter.ToString();
                return ((DateTime)value).ToString(dateForm);
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            return DateTime.Parse(value.ToString());
        }
    }

    #endregion

    #region 暂时不用的扩展代码
    //public class AuditFormInfo
    //{
    //    public string ModelCode { get; set; }
    //    public string AssemblyName { get; set; }
    //    public string className { get; set; }
    //}

    //public class AuditHelper
    //{
    //    public List<AuditFormInfo> listInfo;

    //    static AuditHelper()
    //    {
    //       // listInfo
    //    }
    //    public static void RegisterForm(string modelCode, string assemblyName, string className)
    //    {
    //    }
    //}
    #endregion
    #endregion


    #region ConsultationViewModel
    public class ConsultationViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        //public event EventHandler<AsyncCompletedEventArgs> AddConsultationCompleted;
        //public event EventHandler<AsyncCompletedEventArgs> ReplyConsultationCompleted;
        public FLOW_CONSULTATION_T FlowConsultation;
        public SubmitData SubmmitData;

        public ConsultationViewModel(FLOW_CONSULTATION_T FLOW_CONSULTATION_T, string loginUserID, SubmitData submmitData)
        {
            this.SubmmitData = submmitData;
            this.FlowConsultation = FLOW_CONSULTATION_T;
            if (this.FlowConsultation.CONSULTATIONUSERID == loginUserID)
            {
                if (string.IsNullOrEmpty(this.FlowConsultation.CONSULTATIONCONTENT))
                {
                    this.SetAddState();
                }
                else
                {
                    this.SetViewState();
                }

            }
            else if (this.FlowConsultation.REPLYUSERID == loginUserID)
            {
                if (this.FlowConsultation.FLAG == "0")
                {
                    this.SetReplyState();
                }
                else
                {
                    this.SetViewState();
                }
            }
            else
            {
                this.SetViewState();
            }
        }

        public event EventHandler WaitingHandler;
        public event EventHandler CompletedHandler;


        #region Visibility

        private Visibility _IsShowConsultationDate;
        public Visibility IsShowConsultationDate
        {
            get
            {
                return this._IsShowConsultationDate;
            }
            set
            {
                this._IsShowConsultationDate = value;
                this.RaisePropertyChanged("IsShowConsultationDate");
            }
        }

        private Visibility _IsShowReplyDate;
        public Visibility IsShowReplyDate
        {
            get
            {
                return this._IsShowReplyDate;
            }
            set
            {
                this._IsShowReplyDate = value;
                this.RaisePropertyChanged("IsShowReplyDate");
            }
        }

        private Visibility _IsShowConsultationUser;
        public Visibility IsShowConsultationUser
        {
            get
            {
                return this._IsShowConsultationUser;
            }
            set
            {
                this._IsShowConsultationUser = value;
                this.RaisePropertyChanged("IsShowConsultationUser");
            }
        }


        private Visibility _IsShowConsultationContent;
        public Visibility IsShowConsultationContent
        {
            get
            {
                return this._IsShowConsultationContent;
            }
            set
            {
                this._IsShowConsultationContent = value;
                this.RaisePropertyChanged("IsShowConsultationContent");
            }


        }

        private Visibility _CanConsultationContent;
        public Visibility CanConsultationContent
        {
            get
            {
                return this._CanConsultationContent;
            }
            set
            {
                this._CanConsultationContent = value;
                this.RaisePropertyChanged("CanConsultationContent");
            }
        }

        private Visibility _IsShowReplyUser;
        public Visibility IsShowReplyUser
        {
            get
            {
                return this._IsShowReplyUser;
            }
            set
            {
                this._IsShowReplyUser = value;
                this.RaisePropertyChanged("IsShowReplyUser");
            }


        }


        private Visibility _CanSearchReplyUser;
        public Visibility CanSearchReplyUser
        {
            get
            {
                return this._CanSearchReplyUser;
            }
            set
            {
                this._CanSearchReplyUser = value;
                this.RaisePropertyChanged("CanSearchReplyUser");
            }
        }


        private Visibility _IsShowSearchReplyUser;
        public Visibility IsShowSearchReplyUser
        {
            get
            {
                return this._IsShowSearchReplyUser;
            }
            set
            {
                this._IsShowSearchReplyUser = value;
                this.RaisePropertyChanged("IsShowSearchReplyUser");
            }

        }

        private Visibility _CanAdd;
        public Visibility CanAdd
        {
            get
            {
                return this._CanAdd;
            }
            set
            {
                this._CanAdd = value;
                this.RaisePropertyChanged("CanAdd");
            }
        }

        private Visibility _IsShowReplyContentGrid;
        public Visibility IsShowReplyContentGrid
        {
            get
            {
                return this._IsShowReplyContentGrid;
            }
            set
            {
                this._IsShowReplyContentGrid = value;
                this.RaisePropertyChanged("IsShowReplyContentGrid");
            }

        }


        private Visibility _IsShowReplyContent;
        public Visibility IsShowReplyContent
        {
            get
            {
                return this._IsShowReplyContent;
            }
            set
            {
                this._IsShowReplyContent = value;
                this.RaisePropertyChanged("IsShowReplyContent");
            }

        }

        private Visibility _CanReplyContent;
        public Visibility CanReplyContent
        {
            get
            {
                return this._CanReplyContent;
            }
            set
            {
                this._CanReplyContent = value;
                this.RaisePropertyChanged("CanReplyContent");
            }
        }

        private Visibility _CanReply;
        public Visibility CanReply
        {
            get
            {
                return this._CanReply;
            }
            set
            {
                this._CanReply = value;
                this.RaisePropertyChanged("CanReply");
            }
        }





        #endregion

        #region 绑定
        public string CONSULTATIONUSERID
        {
            get
            {
                return this.FlowConsultation.CONSULTATIONUSERID;
            }
            set
            {
                this.FlowConsultation.CONSULTATIONUSERID = value;
                this.RaisePropertyChanged("CONSULTATIONUSERID");
            }
        }

        public string CONSULTATIONUSERNAME
        {
            get
            {
                return this.FlowConsultation.CONSULTATIONUSERNAME;
            }
            set
            {
                this.FlowConsultation.CONSULTATIONUSERNAME = value;
                this.RaisePropertyChanged("CONSULTATIONUSERNAME");
            }
        }

        public string REPLYUSERID
        {
            get
            {
                return this.FlowConsultation.REPLYUSERID;
            }
            set
            {
                this.FlowConsultation.REPLYUSERID = value;
                this.RaisePropertyChanged("REPLYUSERID");
            }
        }

        public string REPLYUSERNAME
        {
            get
            {
                return this.FlowConsultation.REPLYUSERNAME;
            }
            set
            {
                this.FlowConsultation.REPLYUSERNAME = value;
                this.RaisePropertyChanged("REPLYUSERNAME");
            }
        }

        public string CONSULTATIONCONTENT
        {
            get
            {
                return this.FlowConsultation.CONSULTATIONCONTENT;
            }
            set
            {
                this.FlowConsultation.CONSULTATIONCONTENT = value;
                this.RaisePropertyChanged("CONSULTATIONCONTENT");
            }
        }

        public string REPLYCONTENT
        {
            get
            {
                return this.FlowConsultation.REPLYCONTENT;
            }
            set
            {
                this.FlowConsultation.REPLYCONTENT = value;
                this.RaisePropertyChanged("REPLYCONTENT");
            }
        }

        public DateTime? CONSULTATIONDATE
        {
            get
            {
                return this.FlowConsultation.CONSULTATIONDATE;
            }
            set
            {
                this.FlowConsultation.CONSULTATIONDATE = value;
                this.RaisePropertyChanged("CONSULTATIONDATE");
            }
        }

        public DateTime? REPLYDATE
        {
            get
            {
                return this.FlowConsultation.REPLYDATE;
            }
            set
            {
                this.FlowConsultation.REPLYDATE = value;
                this.RaisePropertyChanged("REPLYDATE");
            }
        }
        #endregion


        public ICommand AddConsultation
        {
            get
            {
                AddCommand add = new AddCommand(this);
                add.WaitingHandler += new EventHandler(MyWaitingHandler);
                add.CompletedHandler += new EventHandler(MyCompletedHandler);
                //add.AddConsultationCompleted += new EventHandler<AsyncCompletedEventArgs>(add_AddConsultationCompleted);
                return add;
            }
        }





        //void add_AddConsultationCompleted(object sender, AsyncCompletedEventArgs e)
        //{
        //    if (this.AddConsultationCompleted != null)
        //    {
        //        this.AddConsultationCompleted(sender, e);
        //    }
        //}

        public ICommand ReplyConsultation
        {
            get
            {
                ReplyCommand reply = new ReplyCommand(this);
                reply.WaitingHandler += new EventHandler(this.MyWaitingHandler);
                reply.CompletedHandler += new EventHandler(this.MyCompletedHandler);
                //reply.ReplyConsultationCompleted += new EventHandler<AsyncCompletedEventArgs>(reply_ReplyConsultationCompleted);
                return reply;
            }
        }

        //void reply_ReplyConsultationCompleted(object sender, AsyncCompletedEventArgs e)
        //{
        //    if (this.ReplyConsultationCompleted != null)
        //    {
        //        this.ReplyConsultationCompleted(sender, e);
        //    }
        //}

        public ICommand SearchReplyUser
        {
            get
            {
                return new SearchCommand(this);
            }
        }

        void MyCompletedHandler(object sender, EventArgs e)
        {
            if (this.CompletedHandler != null)
            {
                this.CompletedHandler(sender, e);
            }
        }

        void MyWaitingHandler(object sender, EventArgs e)
        {
            if (this.WaitingHandler != null)
            {
                this.WaitingHandler(sender, e);
            }
        }

        private void SetAddState()
        {
            this.CanAdd = Visibility.Visible;
            this.CanConsultationContent = Visibility.Visible;
            this.CanSearchReplyUser = Visibility.Visible;
            this.CanReply = Visibility.Collapsed;
            this.CanReplyContent = Visibility.Collapsed;

            this.IsShowConsultationContent = Visibility.Collapsed;
            this.IsShowConsultationDate = Visibility.Collapsed;
            this.IsShowConsultationUser = Visibility.Collapsed;
            this.IsShowReplyContent = Visibility.Collapsed;
            this.IsShowReplyDate = Visibility.Collapsed;
            this.IsShowReplyUser = Visibility.Visible;
            this.IsShowReplyContentGrid = Visibility.Collapsed;
            this.IsShowSearchReplyUser = Visibility.Collapsed;

        }

        private void SetReplyState()
        {
            this.CanAdd = Visibility.Collapsed;
            this.CanConsultationContent = Visibility.Collapsed;
            this.CanReply = Visibility.Visible;
            this.CanReplyContent = Visibility.Visible;
            this.CanSearchReplyUser = Visibility.Collapsed;

            this.IsShowConsultationContent = Visibility.Visible;
            this.IsShowConsultationDate = Visibility.Visible;
            this.IsShowConsultationUser = Visibility.Visible;
            this.IsShowReplyContent = Visibility.Collapsed;
            this.IsShowReplyDate = Visibility.Collapsed;
            this.IsShowReplyUser = Visibility.Collapsed;
            this.IsShowReplyContentGrid = Visibility.Visible;
            this.IsShowSearchReplyUser = Visibility.Visible;
        }

        private void SetViewState()
        {
            this.CanAdd = Visibility.Collapsed;
            this.CanConsultationContent = Visibility.Collapsed;
            this.CanReply = Visibility.Collapsed;
            this.CanReplyContent = Visibility.Collapsed;
            this.CanSearchReplyUser = Visibility.Collapsed;

            this.IsShowConsultationContent = Visibility.Visible;
            this.IsShowConsultationDate = Visibility.Visible;
            this.IsShowConsultationUser = Visibility.Visible;
            this.IsShowReplyContent = Visibility.Visible;
            this.IsShowReplyDate = Visibility.Visible;
            this.IsShowReplyUser = Visibility.Visible;
            this.IsShowReplyContentGrid = Visibility.Visible;
            this.IsShowSearchReplyUser = Visibility.Visible;
        }

    }

    public class AddCommand : ICommand
    {
        private ConsultationViewModel ConsultationViewModel;
        public AddCommand(ConsultationViewModel ConsultationViewModel)
        {
            this.ConsultationViewModel = ConsultationViewModel;
        }
        public event EventHandler<AsyncCompletedEventArgs> AddConsultationCompleted;

        public event EventHandler WaitingHandler;
        public event EventHandler CompletedHandler;

        #region ICommand
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (string.IsNullOrEmpty(this.ConsultationViewModel.CONSULTATIONCONTENT))
            {
                ComfirmWindow.ConfirmationBox("警告", "咨询内容不能为空", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            if (string.IsNullOrEmpty(this.ConsultationViewModel.REPLYUSERID))
            {
                ComfirmWindow.ConfirmationBox("警告", "回复人不能为空", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            if (this.WaitingHandler != null)
            {
                this.WaitingHandler(this, null);
            }
            SMT.Saas.Tools.FlowWFService.ServiceClient client = new ServiceClient();
            client.AddConsultationCompleted += new EventHandler<AsyncCompletedEventArgs>(client_AddConsultationCompleted);
            client.AddConsultationAsync(this.ConsultationViewModel.FlowConsultation,this.ConsultationViewModel.SubmmitData);
            client.CloseAsync();
        }
        #endregion

        void client_AddConsultationCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (this.CompletedHandler != null)
            {
                this.CompletedHandler(this, null);
            }
            if (this.AddConsultationCompleted != null)
            {
                this.AddConsultationCompleted(sender, e);
            }
        }
    }

    public class ReplyCommand : ICommand
    {
        private ConsultationViewModel ConsultationViewModel;
        public ReplyCommand(ConsultationViewModel ConsultationViewModel)
        {
            this.ConsultationViewModel = ConsultationViewModel;
        }
        //public event EventHandler<AsyncCompletedEventArgs> ReplyConsultationCompleted;

        public event EventHandler WaitingHandler;
        public event EventHandler CompletedHandler;

        #region ICommand
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (string.IsNullOrEmpty(this.ConsultationViewModel.REPLYCONTENT))
            {
                ComfirmWindow.ConfirmationBox("警告", "回复内容不能为空", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (this.WaitingHandler != null)
            {
                this.WaitingHandler(this, null);
            }
            SMT.Saas.Tools.FlowWFService.ServiceClient client = new ServiceClient();
            client.ReplyConsultationCompleted += new EventHandler<AsyncCompletedEventArgs>(client_ReplyConsultationCompleted);
            client.ReplyConsultationAsync(this.ConsultationViewModel.FlowConsultation,this.ConsultationViewModel.SubmmitData);
            client.CloseAsync();
        }
        #endregion

        void client_ReplyConsultationCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (this.CompletedHandler != null)
            {
                this.CompletedHandler(this, null);
            }
            //if (this.ReplyConsultationCompleted != null)
            //{
            //    this.ReplyConsultationCompleted(sender, e);
            //}
        }

    }

    public class SearchCommand : ICommand
    {
        private ConsultationViewModel ConsultationViewModel;
        public SearchCommand(ConsultationViewModel ConsultationViewModel)
        {
            this.ConsultationViewModel = ConsultationViewModel;
        }


        #region ICommand
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookupAll lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookupAll();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj result = ent.FirstOrDefault();
                    if (result.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
                    {
                        this.ConsultationViewModel.REPLYUSERID = result.ObjectID;
                        this.ConsultationViewModel.REPLYUSERNAME = result.ObjectName;

                    }
                    else
                    {
                        this.ConsultationViewModel.REPLYUSERID = string.Empty;
                        this.ConsultationViewModel.REPLYUSERNAME = string.Empty;
                    }
                }


            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion



    }
    #endregion

    #region 会签
    #region CUserInfoViewModel
    public class UserInfoViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public UserInfoViewModel(UserInfo UserInfo)
        {
            this.UserInfo = UserInfo;
            this.IsCheck = true;
        }

        private UserInfo _UserInfo;
        public UserInfo UserInfo
        {
            get
            {
                return this._UserInfo;
            }
            set
            {
                if (ReferenceEquals(value, this._UserInfo) == true)
                {
                    return;
                }
                this._UserInfo = value;
                this.RaisePropertyChanged("UserInfo");

            }
        }

        private bool _IsCheck;
        public bool IsCheck
        {
            get
            {
                return this._IsCheck;
            }
            set
            {
                if (ReferenceEquals(value, this._IsCheck) == true)
                {
                    return;
                }
                this._IsCheck = value;
                this.RaisePropertyChanged("IsCheck");
            }
        }

    }
    #endregion

    public class Rule_UserInfoViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Rule_UserInfoViewModel(Role_UserType Role_UserType, List<UserInfo> listUserInfo)
        {
            this.Role_UserType = Role_UserType;
            this.ListUserInfo = new List<UserInfoViewModel>();
            listUserInfo.ForEach(item =>
            {
                this.ListUserInfo.Add(new UserInfoViewModel(item));
            });
        }

        public Role_UserType Role_UserType
        {
            get;
            set;
        }

        public List<UserInfoViewModel> ListUserInfo
        {
            get;
            set;
        }


    }
    #endregion 
}
