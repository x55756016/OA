/// <summary>
/// Log No.： 1
/// Modify Desc： 手工打分时T_HR_KPIRecord.StepDetailCode（KPI明细记录的步骤单号）使用StepID（表单产生步骤ID）
///               空的businessID（relationID）时使用flowCode为GetKPIPoint()返回结果，控件改造（参照审核控件）
/// Modifier： 冉龙军
/// Modify Date： 2010-08-10
/// </summary>
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
using SMT.Saas.Tools.PerformanceWS;
using System.Collections;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.Common;
using SMT.Saas.Tools.EngineWS;
// 1s 冉龙军
using SMT.Saas.Tools.FlowWFService;
using SMT.SaaS.FrameworkUI.AuditControl;
using System.Text;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.ClientUtility;
// 1e
namespace SMT.SaaS.FrameworkUI.KPIControl
{
    public partial class KPIScoring : UserControl, IEntityEditor
    {
        // 1s 冉龙军
        #region 属   性
        //流程明细
        public List<FLOW_FLOWRECORDDETAIL_T> AuditEntityList { get; set; }

        private ServiceClient auditService = null;
        public ServiceClient AuditService
        {
            get
            {
                if (auditService == null)
                {
                    auditService = new ServiceClient();
                    auditService.SubimtFlowCompleted += new EventHandler<SubimtFlowCompletedEventArgs>(auditService_StartFlowCompleted);
                    auditService.SubimtFlowCompleted += new EventHandler<SubimtFlowCompletedEventArgs>(auditService_SubimtFlowCompleted);
                    auditService.GetFlowInfoCompleted += new EventHandler<GetFlowInfoCompletedEventArgs>(auditService_GetFlowInfoCompleted);

                }
                return auditService;
            }
        }
        //是否打分用户
        public bool IsAuditUser { get; set; }
        protected string EditUserID { get; set; }
        protected string EditUserName { get; set; }
        protected string AuditRemark { get; set; }
        public bool IsFixedFlow { get; set; }
        /// <summary>
        /// 流程打分StepCode
        /// </summary>
        public string FlowStateCode { get; set; }
        /// <summary>
        /// 关闭打分提醒消息的ID
        /// </summary>
        public string RemindGuid { get; set; }
        /// <summary>
        /// 是否KPI打分
        /// </summary>
        public string IsKpi { get; set; }
        /// <summary>
        /// 关闭打分任务消息的ID
        /// </summary>
        public string MessgeID { get; set; }
        //当前审核流程记录详情
        private FLOW_FLOWRECORDDETAIL_T currentFLOWRECORDDETAIL = null;
        //
        #region 控制界面视图状态属性

        // 是否显示KPI打分面板
        protected bool IsShowKPIScorePnl
        {
            get
            {
                return this.KPILayoutRoot.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (this.KPILayoutRoot.IsNotNull())
                        this.KPILayoutRoot.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this.KPILayoutRoot.IsNotNull())
                        this.KPILayoutRoot.Visibility = Visibility.Collapsed;
                }
            }
        }
        public void ShowKPIScoreTabItem(bool IsShowKPIScoreTabItem)
        {
            try
            {
                TabItem tabItem = this.Parent as TabItem;
                if (IsShowKPIScoreTabItem)
                {
                    tabItem.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    tabItem.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        //SMT.SaaS.FrameworkUI.AuditControl流程记录表实体
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
        public int ManualTypeScore = 0;
        #endregion
        #region WCF事件完成方法
        void auditService_StartFlowCompleted(object sender, SubimtFlowCompletedEventArgs e)
        {
            DoAuditResult(e.Result);
        }
        void auditService_SubimtFlowCompleted(object sender, SubimtFlowCompletedEventArgs e)
        {
            DoAuditResult(e.Result);
        }
        private void DoAuditResult(DataResult dataResult)
        {

            try
            {
                if (dataResult.FlowResult == FlowResult.SUCCESS)
                {
                    this.BindingData(this.FlowStateCode, this.RemindGuid, this.IsKpi, this.MessgeID);
                }
                else if (dataResult.FlowResult == FlowResult.END)
                {
                    this.BindingData(this.FlowStateCode, this.RemindGuid, this.IsKpi, this.MessgeID);
                }
                else if (dataResult.FlowResult == FlowResult.FAIL)
                {
                    // 1s 冉龙军 （待续）
                    // 1e
                }
                else
                {
                    // 1s 冉龙军 （待续）
                    // 1e

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // 1s 冉龙军 （待续）
                // 1e
            }

        }
        void auditService_GetFlowInfoCompleted(object sender, GetFlowInfoCompletedEventArgs e)
        {
            //为NULL 代表为新记录
            if (e.Result == null)
            {
                //若无结果则显示 无信息面板     
                //IsShowKPIScorePnl = false;
                ShowKPIScoreTabItem(false);
                return;
            }
            else
            {

                //获取审核信息列表,若无审核记录则表明用户未曾提交过流程，否则 则可能为审核中或完成
                //var items = from item in e.Result
                //            where item.STATECODE.ToUpper() != "STARTFLOW"
                //            orderby item.EDITDATE
                //            select item;
                var items = from item in e.Result
                            orderby item.EDITDATE
                            select item;

                if (items.ToList().Count > 0)
                {
                    #region 数据审核
                    this.AuditEntityList = items.ToList();

                    //审核记录
                    List<FLOW_FLOWRECORDDETAIL_T> list = this.AuditEntityList;
                    if (FlowStateCode != null)
                    {
                        //获取第一个符合条件的实体数据
                        FLOW_FLOWRECORDDETAIL_T currentFlow = this.AuditEntityList.FirstOrDefault(item => item.STATECODE == FlowStateCode);
                        //流程结束 如果有抽查打分 需要获取最后流程的最后两个节点
                        if (currentFlow == null && FlowStateCode.ToUpper() == "ENDFLOW")
                        {
                            currentFlow = AuditEntityList.OrderByDescending(s => s.EDITDATE).FirstOrDefault();
                        }
                        //若不为NULL 则表示需要当前用户进行审批
                        if (currentFlow != null)
                        {
                            //判断流程审批与逐级审批
                            IsFixedFlow = (currentFlow.FLOW_FLOWRECORDMASTER_T.FLOWSELECTTYPE == "0") ? true : false;
                            if (IsFixedFlow)
                            {

                                FLOW_FLOWRECORDDETAIL_T parentFlow = this.AuditEntityList.FirstOrDefault(item =>
                                {
                                    return item.FLOWRECORDDETAILID == currentFlow.PARENTSTATEID;
                                });
                                if (parentFlow == null)
                                {
                                    ShowKPIScoreTabItem(false);
                                    return;
                                }
                                else
                                {
                                    FLOW_FLOWRECORDDETAIL_T preParentFlow = this.AuditEntityList.FirstOrDefault(item =>
                                    {
                                        return item.FLOWRECORDDETAILID == parentFlow.PARENTSTATEID;
                                    });
                                    if (preParentFlow == null)
                                    {
                                        preParentFlow = new FLOW_FLOWRECORDDETAIL_T();
                                        preParentFlow.STATECODE = "";
                                        preParentFlow.EDITDATE = parentFlow.CREATEDATE;
                                        preParentFlow.EDITUSERID = parentFlow.EDITUSERID;
                                    }
                                    if (FlowStateCode.ToUpper() == "ENDFLOW")
                                    {
                                        InitialInfo(currentFlow.CREATECOMPANYID, AuditEntity.ModelCode, currentFlow.STATECODE, currentFlow.STATECODE, AuditEntity.FormID,
                                                    currentFlow.FLOWRECORDDETAILID, currentFlow.FLOWRECORDDETAILID, currentFlow.CREATEDATE,
                                                    currentFlow.EDITDATE.Value, currentFlow.EDITUSERID, currentFlow.EDITUSERID);
                                    }
                                    else
                                    {
                                        InitialInfo(currentFlow.CREATECOMPANYID, AuditEntity.ModelCode, parentFlow.STATECODE, preParentFlow.STATECODE, AuditEntity.FormID,
                                                   parentFlow.FLOWRECORDDETAILID, preParentFlow.FLOWRECORDDETAILID, preParentFlow.EDITDATE.Value,
                                                   parentFlow.EDITDATE.Value, parentFlow.EDITUSERID, parentFlow.EDITUSERID);
                                    }
                                }
                            }
                            else
                            {
                                ShowKPIScoreTabItem(false);
                                return;
                            }
                        }
                        else
                        {

                        }
                    }
                    #region
                    else
                    {
                        //获取第一个符合条件的实体数据
                        FLOW_FLOWRECORDDETAIL_T currentFlow = this.AuditEntityList.FirstOrDefault(item =>
                        {
                            //当前用户是否为审核用户
                            bool bUser = item.EDITUSERID == AuditEntity.EditUserID;
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
                                //流程审核
                                // 1s 冉龙军 来自审核控件
                                list.Remove(currentFlow);
                                AuditEntity.ModelCode = currentFlow.FLOW_FLOWRECORDMASTER_T.MODELCODE;
                                AuditEntity.GUID = currentFlow.FLOWRECORDDETAILID;
                                currentFLOWRECORDDETAIL = currentFlow;

                                #region 显示控件（来自审核控件ShowKPI(AuditOperation auditOperation, AuditAction action)）
                                if (currentFLOWRECORDDETAIL == null)
                                {
                                    //IsShowKPIScorePnl = false;
                                    ShowKPIScoreTabItem(false);
                                    return;
                                }
                                FLOW_FLOWRECORDDETAIL_T c = currentFLOWRECORDDETAIL;

                                FLOW_FLOWRECORDDETAIL_T parentFlow = this.AuditEntityList.FirstOrDefault(item =>
                                {
                                    return item.FLOWRECORDDETAILID == c.PARENTSTATEID;
                                });
                                if (parentFlow == null)
                                {
                                    parentFlow = new FLOW_FLOWRECORDDETAIL_T();
                                    parentFlow.STATECODE = "";
                                    parentFlow.EDITDATE = c.CREATEDATE;
                                    parentFlow.EDITUSERID = c.EDITUSERID;
                                }
                                //初始化控件
                                //InitialInfo(c.CREATECOMPANYID, AuditEntity.ModelCode, c.STATECODE, parentFlow.STATECODE, AuditEntity.FormID,
                                //            c.FLOWRECORDDETAILID, parentFlow.FLOWRECORDDETAILID, parentFlow.EDITDATE.Value,
                                //            System.DateTime.Now, parentFlow.EDITUSERID, c.EDITUSERID);
                                // 2s 冉龙军
                                // 需求错了
                                //InitialInfo(c.CREATECOMPANYID, AuditEntity.ModelCode, c.STATECODE, parentFlow.STATECODE, AuditEntity.FormID,
                                //            c.FLOWRECORDDETAILID, parentFlow.FLOWRECORDDETAILID, parentFlow.EDITDATE.Value,
                                //            c.EDITDATE.Value, parentFlow.EDITUSERID, c.EDITUSERID);
                                if (parentFlow != null)
                                {
                                    FLOW_FLOWRECORDDETAIL_T preParentFlow = this.AuditEntityList.FirstOrDefault(item =>
                                    {
                                        return item.FLOWRECORDDETAILID == parentFlow.PARENTSTATEID;
                                    });
                                    if (preParentFlow == null)
                                    {
                                        preParentFlow = new FLOW_FLOWRECORDDETAIL_T();
                                        preParentFlow.STATECODE = "";
                                        preParentFlow.EDITDATE = parentFlow.CREATEDATE;
                                        preParentFlow.EDITUSERID = parentFlow.EDITUSERID;
                                    }
                                    InitialInfo(c.CREATECOMPANYID, AuditEntity.ModelCode, parentFlow.STATECODE, preParentFlow.STATECODE, AuditEntity.FormID,
                                                parentFlow.FLOWRECORDDETAILID, preParentFlow.FLOWRECORDDETAILID, preParentFlow.EDITDATE.Value,
                                                parentFlow.EDITDATE.Value, parentFlow.EDITUSERID, parentFlow.EDITUSERID);

                                }
                                else
                                {
                                    ShowKPIScoreTabItem(false);
                                    return;
                                }
                                // 2e
                                #endregion
                                // 1e
                            }
                            else
                            {
                                //逐级审核
                                //IsShowKPIScorePnl = false;
                                ShowKPIScoreTabItem(false);
                            }

                        }
                        else
                        {
                            //审核结束
                            //IsShowKPIScorePnl = false;
                            ShowKPIScoreTabItem(false);
                            IsAuditUser = false;
                            //获取第一个符合条件的实体数据
                            currentFlow = this.AuditEntityList.FirstOrDefault(item =>
                            {
                                //当前用户是否为审核用户
                                bool bUser = item.EDITUSERID != AuditEntity.EditUserID;
                                bool bResult = item.FLAG == "0";//0：未处理，1：已处理
                                return bResult && bUser;
                            });
                            //若不为NULL 则表示需要当前用户进行审批
                            if (currentFlow != null)
                            {
                                // 1s 冉龙军 来自审核控件
                                list.Remove(currentFlow);
                                AuditEntity.ModelCode = currentFlow.FLOW_FLOWRECORDMASTER_T.MODELCODE;
                                AuditEntity.GUID = currentFlow.FLOWRECORDDETAILID;
                                currentFLOWRECORDDETAIL = currentFlow;

                                //显示控件
                                if (currentFLOWRECORDDETAIL == null)
                                {
                                    //IsShowKPIScorePnl = false;
                                    ShowKPIScoreTabItem(false);
                                    return;
                                }
                                FLOW_FLOWRECORDDETAIL_T c = currentFLOWRECORDDETAIL;

                                FLOW_FLOWRECORDDETAIL_T parentFlow = this.AuditEntityList.FirstOrDefault(item =>
                                {
                                    return item.FLOWRECORDDETAILID == c.PARENTSTATEID;
                                });
                                if (parentFlow == null)
                                {
                                    parentFlow = new FLOW_FLOWRECORDDETAIL_T();
                                    parentFlow.STATECODE = "";
                                    parentFlow.EDITDATE = c.CREATEDATE;
                                    parentFlow.EDITUSERID = c.EDITUSERID;
                                }
                                this.lastStepDate = parentFlow.EDITDATE.Value;
                                this.stepDate = c.EDITDATE.Value;
                                //是否抽查打分
                                // 2s 冉龙军
                                //client.GetKPIRecordAsync(this.AuditEntity.FormID, this.AuditEntity.FlowCode, currentFlow.STATECODE);
                                // client.GetKPIRecordAsync(this.AuditEntity.FormID, this.AuditEntity.FlowCode, parentFlow.STATECODE, "Flow");
                                // 2e
                                //client.GetKPIRecordRandomPersonIDAsync(this.AuditEntity.FormID, this.AuditEntity.FlowCode, currentFlow.STATECODE);

                            }
                        }
                    }
                    #endregion

                    #endregion
                    //if (IsKpi == "1")
                    //{
                    //    IsShowKPIScorePnl = true;
                    //    ShowKPIScoreTabItem(true);
                    //}
                }
                else
                {
                    //数据未曾提交成功,需要重新 提交审核
                    //IsShowKPIScorePnl = false;
                    ShowKPIScoreTabItem(false);
                }
            }

            // 1s 冉龙军 （待续）
            //if (OnBindingDataCompleted.IsNotNull())
            //    OnBindingDataCompleted(this, EventArgs.Empty);
            // 1e
        }
        #endregion
        // 1e
        public T_HR_KPIPOINT KPIPoint { get; set; }
        public T_HR_KPIPOINT LastKPIPoint { get; set; }
        public T_HR_KPIRECORD KPIRecord { get; set; }
        private ObservableCollection<T_HR_RAMDONGROUPPERSON> groupPersonList { get; set; }
        public FormTypes FormType { get; set; }
        private PerformanceServiceClient client = new PerformanceServiceClient();
        private string companyID, modelRelationID, stepCode, lastStepCode, formID, stepID, lastStepID, AppraiseeID, userID;
        private int systemScore = 100;
        private DateTime lastStepDate, stepDate;
        private bool isClose = false;

        #region 存在考核点时，显示窗口事件
        public delegate void ShowEventHandler(object sender, ScoringEventArgs e);
        public event ShowEventHandler ShowFrom;
        protected virtual void OnShowFrom(bool e)
        {
            if (ShowFrom != null)
            {
                ScoringEventArgs arg = new ScoringEventArgs();
                arg.IsShow = e;
                ShowFrom(this, arg);
            }
        }
        #endregion 存在考核点时，显示窗口事件

        #region 完成手动或抽查打分的事件
        public delegate void ScoredEventHandler();
        public event ScoredEventHandler Scored;
        protected virtual void OnScored()
        {
            if (Scored != null)
            {
                Scored();
            }
        }
        #endregion 完成手动或抽查打分的事件
        //public bool isShow = false;

        /// <summary>
        /// KPI打分情况
        /// </summary>
        /// <param name="type">窗口模式</param>
        /// <param name="companyID">公司ID</param>
        /// <param name="modelRelationID">业务关系ID</param>
        /// <param name="stepCode">步骤Code</param>
        /// <param name="lastStepCode">上一步步骤Code，没有上一步则为string.empty</param>
        /// <param name="formID">每次业务产生的表单ID</param>
        /// <param name="stepID">当前业务表单产生步骤记录ID</param>
        /// <param name="lastStepID">当前业务表单产生的上一步骤记录ID，没有上一步则为string.empty</param>
        /// <param name="lastStepDate">上一步完成时间，没有上一步则为null</param>
        /// <param name="stepDate">该步骤完成时间</param>
        /// <param name="AppraiseeID">被评分人ID</param>
        /// <param name="userID">当前用户ID</param>
        public KPIScoring(FormTypes type, string companyID, string modelRelationID, string stepCode, string lastStepCode,
            string formID, string stepID, string lastStepID, DateTime lastStepDate, DateTime stepDate, string AppraiseeID, string userID)
        {
            FormType = type;
            this.companyID = companyID;
            this.modelRelationID = modelRelationID;
            this.stepCode = stepCode;
            this.lastStepCode = lastStepCode;
            this.formID = formID;
            this.stepID = stepID;
            this.lastStepID = lastStepID;
            this.lastStepDate = lastStepDate;
            this.stepDate = stepDate;
            this.AppraiseeID = AppraiseeID;
            this.userID = userID;
            InitializeComponent();

            DictionaryManager dicManager = new DictionaryManager();
            dicManager.OnDictionaryLoadCompleted += (sender2, e2) =>
                {
                    InitPara();

                    // 1s 冉龙军
                    //client.GetKPIPointAndLastPointAsync(companyID, modelRelationID, "", stepCode, lastStepCode);
                    // 空的businessID（relationID）时使用flowCode为GetKPIPoint()返回结果
                    client.GetKPIPointAndLastPointAsync(companyID, modelRelationID, formID, stepCode, lastStepCode);
                    // 1e
                    //client.GetKPIPointAsync(companyID, modelRelationID, "", lastStepCode);
                };
            dicManager.LoadDictionary("MANUALTYPE");
           
        }

        public KPIScoring()
        {
            // 1s 冉龙军
            AuditEntity = new Flow_FlowRecord_T();
            AuditSubmitData = new SubmitData();
            // 1e
        }

        /// <summary>
        /// 引擎调用时的构造函数
        /// </summary>
        /// <param name="KPIRecordID"></param>
        /// <param name="type"></param>
        public KPIScoring(string KPIRecordID, FormTypes type)
        {
            FormType = type;
            InitializeComponent();
            string[] arr = new string[2];
            DictionaryManager dicManager = new DictionaryManager();
            dicManager.OnDictionaryLoadCompleted += (sender2, e2) =>
            {
                InitPara();
                client.GetKPIRecordByIdAsync(KPIRecordID);
            };
            dicManager.LoadDictionary("MANUALTYPE");

           
        }

        void KPIScoring_Loaded(object sender, RoutedEventArgs e)
        {
            DictionaryManager dicManager = new DictionaryManager();
            dicManager.OnDictionaryLoadCompleted += (sender2, e2) =>
            {
                //窗口状态
                switch ((int)FormType)
                {
                    case 0://NEW——手动打分
                        //给当前用户进行系统评分
                        //if (KPIPoint != null)
                        //{
                        //    //显示评分信息
                        //    lblYourScore.Visibility = Visibility.Visible;
                        //    lblSysScore.Visibility = Visibility.Visible;
                        //    client.KPISystemScoreAsync(companyID, modelRelationID, formID, stepCode, formID, "", stepID, lastStepDate, stepDate, AppraiseeID);
                        //}
                        // 1s 冉龙军
                        ////给上一步KPI进行默认手动评分，默认100分
                        //if (LastKPIPoint != null)
                        //    client.KPIManualScoreAsync(companyID, modelRelationID, formID, lastStepCode, formID, "", lastStepID, AppraiseeID, userID, 100);
                        // 2s 冉龙军
                        // 暂缓
                        ////取上一步KPI手动评分
                        //if (LastKPIPoint != null)
                        //    client.GetKPIRecordScoreDetailAsync(formID, "", stepCode, 1);
                        // 2e
                        break;
                    // 1e

                    case 1://EDIT——抽查打分
                        lblYourScore.Visibility = Visibility.Collapsed;
                        lblSysScore.Visibility = Visibility.Collapsed;
                        ////给上一步KPI进行默认抽查评分，默认100分
                        //if (KPIRecord != null && KPIRecord.T_HR_KPIPOINT != null)
                        //    client.KPIRandomScoreByKPIPointAsync(KPIRecord.T_HR_KPIPOINT, KPIRecord.BUSINESSCODE, formID, KPIRecord.STEPDETAILCODE, KPIRecord.APPRAISEEID, userID, 100);
                        //取上一步KPI抽查评分
                        if (KPIRecord != null && KPIRecord.RANDOMSCORE != null)
                        {
                            //绑定RadioButton
                            BindRadioButton(KPIRecord.RANDOMSCORE.ToString());
                        }

                        break;
                    case 2: //BROWE
                        break;
                    case 3: //ADUIT
                        break;
                }

                //未获取到信息
                if (KPIPoint == null)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
            };
            dicManager.LoadDictionary("MANUALTYPE");

           
        }

        /// <summary>
        /// 流程调用时初始化函数
        /// </summary>
        /// <param name="companyID">公司ID</param>
        /// <param name="modelRelationID">业务关系ID</param>
        /// <param name="stepCode">步骤Code</param>
        /// <param name="lastStepCode">上一步步骤Code，没有上一步则为string.empty</param>
        /// <param name="formID">每次业务产生的表单ID</param>
        /// <param name="stepID">当前业务表单产生步骤记录ID</param>
        /// <param name="lastStepID">当前业务表单产生的上一步骤记录ID，没有上一步则为string.empty</param>
        /// <param name="lastStepDate">上一步完成时间，没有上一步则为null</param>
        /// <param name="stepDate">该步骤完成时间</param>
        /// <param name="AppraiseeID">被评分人ID</param>
        /// <param name="userID">当前用户ID</param>
        public void InitialInfo(string companyID, string modelRelationID, string stepCode, string lastStepCode,
           string formID, string stepID, string lastStepID, DateTime lastStepDate, DateTime stepDate, string AppraiseeID, string userID)
        {
            FormType = FormTypes.New;
            this.companyID = companyID;
            this.modelRelationID = modelRelationID;
            this.stepCode = stepCode;
            this.lastStepCode = lastStepCode;
            this.formID = formID;
            this.stepID = stepID;
            this.lastStepID = lastStepID;
            this.lastStepDate = lastStepDate;
            this.stepDate = stepDate;
            this.AppraiseeID = AppraiseeID;
            this.userID = userID;
            InitializeComponent();

            DictionaryManager dicManager = new DictionaryManager();
            dicManager.OnDictionaryLoadCompleted += (sender2, e2) =>
            {
                InitPara();
                //检查是否已经抽查打分
                client.GetKPIRecordAsync(formID, "", stepCode, "Engine");
                // 1s 冉龙军
                // client.GetKPIPointAndLastPointAsync(companyID, modelRelationID, "", stepCode, lastStepCode);
                // 注意：空的businessID（relationID）时使用flowCode为GetKPIPoint()返回结果
                //client.GetKPIPointAndLastPointAsync(companyID, modelRelationID, formID, stepCode, lastStepCode);
                // 1e
            };
            dicManager.LoadDictionary("MANUALTYPE");

            
        }

        /// <summary>
        /// 初始化界面参数
        /// </summary>
        private void InitPara()
        {
            //加载事件
            client.KPISystemScoreCompleted += new EventHandler<KPISystemScoreCompletedEventArgs>(client_KPISystemScoreCompleted);
            client.KPIManualScoreCompleted += new EventHandler<KPIManualScoreCompletedEventArgs>(client_KPIManualScoreCompleted);
            client.KPIRandomScoreByKPIPointCompleted += new EventHandler<KPIRandomScoreByKPIPointCompletedEventArgs>(client_KPIRandomScoreByKPIPointCompleted);
            client.GetKPIPointCompleted += new EventHandler<GetKPIPointCompletedEventArgs>(client_GetKPIPointCompleted);
            client.GetKPIPointAndLastPointCompleted += new EventHandler<GetKPIPointAndLastPointCompletedEventArgs>(client_GetKPIPointAndLastPointCompleted);
            client.GetRandomGroupPersonByGroupIDCompleted += new EventHandler<GetRandomGroupPersonByGroupIDCompletedEventArgs>(client_GetRandomGroupPersonByGroupIDCompleted);
            client.GetKPIRecordByIdCompleted += new EventHandler<GetKPIRecordByIdCompletedEventArgs>(client_GetKPIRecordByIdCompleted);
            //client.GetKPIRecordCompleted+=new EventHandler<GetKPIRecordCompletedEventArgs>(client_GetKPIRecordCompleted);
            //client.GetKPIRecordByIdCompleted += new EventHandler<GetKPIRecordByIdCompletedEventArgs>(client_GetKPIRecordByIdCompleted);
            //client.AddKPIRecordComplainCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AddKPIRecordComplainCompleted);
            //client.UpdateKPIRecordComplainCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_UpdateKPIRecordComplainCompleted);
            // 1s 冉龙军
            client.GetKPIRecordScoreDetailCompleted += new EventHandler<GetKPIRecordScoreDetailCompletedEventArgs>(client_GetKPIRecordScoreDetailCompleted);
            client.KPIRandomScoreCompleted += new EventHandler<KPIRandomScoreCompletedEventArgs>(client_KPIRandomScoreCompleted);
            // 1e
            this.Loaded += new RoutedEventHandler(KPIScoring_Loaded);

        }




        /// <summary>
        /// 保存页面信息
        /// </summary>
        private void Save()
        {
            int score = 0;
            try
            {
                // 1s 冉龙军
                //score = int.Parse(txtSCORE.Text.Trim());
                score = ManualTypeScore;
                // 1e

            }
            catch
            {
                //  MessageBox.Show("请填写数字", "分数错误", MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("请填写数字"),
                     Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            // 2s 冉龙军
            if (FlowStateCode != null)
            {
                FormType = FormTypes.Edit;
            }

            // 2e
            switch ((int)FormType)
            {
                case 0://NEW——手动打分
                    //给当前用户进行手动评分
                    // 1s 冉龙军
                    //client.KPIManualScoreAsync(companyID, modelRelationID, "", lastStepCode, formID, "", lastStepID, AppraiseeID, userID, score);
                    // 注意：空的businessID（relationID）时使用flowCode为GetKPIPoint()返回结果
                    client.KPIManualScoreAsync(companyID, modelRelationID, formID, stepCode, formID, "", stepID, AppraiseeID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, score);

                    // 1e
                    break;
                case 1://EDIT——抽查打分
                    // 1s 冉龙军
                    // 注意：空的businessID（relationID）时使用flowCode为GetKPIPoint()返回结果
                    //client.KPIRandomScoreByKPIPointAsync(KPIRecord.T_HR_KPIPOINT, KPIRecord.BUSINESSCODE, "", KPIRecord.STEPDETAILCODE, KPIRecord.APPRAISEEID, userID, score);
                    // 2s 冉龙军
                    //client.KPIRandomScoreByKPIPointAsync(KPIRecord.T_HR_KPIPOINT, KPIRecord.BUSINESSCODE, formID, KPIRecord.STEPDETAILCODE, KPIRecord.APPRAISEEID, userID, score);
                    client.KPIRandomScoreAsync(companyID, modelRelationID, formID, stepCode, formID, "", stepID, AppraiseeID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, score);
                    // 2e
                    // 1e
                    break;
            }
            // 1s 冉龙军
            btnReviewProcess.IsEnabled = false;
            // 1e
            //打分完成
            OnScored();
        }

        // 1s 冉龙军
        private void btnReviewProcess_Click(object sender, RoutedEventArgs e)
        {
            var ent =from q in  AuditEntityList
                       orderby q.CREATEDATE
                       select q;
            if (ent.Count() > 0)
            {

                if (ent.FirstOrDefault().EDITUSERID == SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID
                    && ent.FirstOrDefault().FLAG == "0")
                {
                    MessageBox.Show("请先审核单据，单据审核完毕后方可进行评分");
                    return;
                }
            }
            Save();
            //关闭打分提醒消息
            SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient FlowEngine = new SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient();
            //if (this.RemindGuid == null || this.RemindGuid == "")  
            //{
            //    MessageBox.Show("RemindGuid");
            //}
            //if (this.MessgeID == null && this.MessgeID == "")
            //{
            //    MessageBox.Show("MessgeID");
            //}
            if ((this.RemindGuid != null && this.RemindGuid != "") || (this.MessgeID != null && this.MessgeID != ""))
            {
                //MessageBox.Show("RemindGuid" + RemindGuid + "******" + "MessgeID" + MessgeID);
                FlowEngine.MsgCloseAsync(this.MessgeID, this.RemindGuid);
            }
        }
        /// <summary>
        /// 绑定RadioButton
        /// </summary>
        private void BindRadioButton(string score)
        {
            DictionaryManager dicManager = new DictionaryManager();
            dicManager.OnDictionaryLoadCompleted += (sender2, e2) =>
            {
                List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
                var ents = dicts.Where(s => s.DICTIONCATEGORY == "MANUALTYPE").OrderBy(s => s.DICTIONARYVALUE);
                List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> tempDicts = ents.ToList();
                #region beyond
                if (tempDicts == null || tempDicts.Count == 0)
                {
                    MessageBox.Show("没有加载字典：MANUALTYPE");
                    return;
                }
                #endregion
                try
                {
                    if (score != null && score != "")
                    {
                        if (score == tempDicts[0].DICTIONARYVALUE.ToString())
                        {
                            Score1.IsChecked = true;
                        }
                        else if (score == tempDicts[1].DICTIONARYVALUE.ToString())
                        {
                            Score2.IsChecked = true;
                        }
                        else if (score == tempDicts[2].DICTIONARYVALUE.ToString())
                        {
                            Score3.IsChecked = true;
                        }
                        else if (score == tempDicts[3].DICTIONARYVALUE.ToString())
                        {
                            Score4.IsChecked = true;
                        }
                        else if (score == tempDicts[4].DICTIONARYVALUE.ToString())
                        {
                            Score5.IsChecked = true;
                        }
                        else
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            };
            dicManager.LoadDictionary("MANUALTYPE");

           
        }
        /// <summary>
        /// RadioButton选项
        /// </summary>
        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            DictionaryManager dicManager = new DictionaryManager();
            dicManager.OnDictionaryLoadCompleted += (sender2, e2) =>
            {
                List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
                var ents = dicts.Where(s => s.DICTIONCATEGORY == "MANUALTYPE").OrderBy(s => s.DICTIONARYVALUE);
                List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> tempDicts = ents.ToList();

                #region beyond
                if (tempDicts == null || tempDicts.Count == 0)
                {
                    MessageBox.Show("没有加载字典：MANUALTYPE");
                    return;
                }
                #endregion
                try
                {
                    RadioButton rb = sender as RadioButton;
                    switch (rb.Name)
                    {
                        case "Score1":
                            ManualTypeScore = Convert.ToInt32(tempDicts[0].DICTIONARYVALUE);
                            break;
                        case "Score2":
                            ManualTypeScore = Convert.ToInt32(tempDicts[1].DICTIONARYVALUE);
                            break;
                        case "Score3":
                            ManualTypeScore = Convert.ToInt32(tempDicts[2].DICTIONARYVALUE);
                            break;
                        case "Score4":
                            ManualTypeScore = Convert.ToInt32(tempDicts[3].DICTIONARYVALUE);
                            break;
                        case "Score5":
                            ManualTypeScore = Convert.ToInt32(tempDicts[4].DICTIONARYVALUE);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            };
            dicManager.LoadDictionary("MANUALTYPE");

           
        }
        // 1e

        /// <summary>
        /// 设置抽查信息
        /// </summary>
        private void SetRandomScore()
        {
            if (groupPersonList == null || groupPersonList.Count == 0)
                return;

            Random r = new Random();
            //获取随机数
            int i = r.Next(0, groupPersonList.Count);
            //获取抽查人员
            T_HR_RAMDONGROUPPERSON person = groupPersonList[i];

            SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient engineClient = new SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient();
            CustomUserMsg userMsg = new CustomUserMsg();
            //KPI记录ID
            userMsg.FormID = KPIRecord.KPIRECORDID;
            userMsg.UserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //用户列表
            ObservableCollection<CustomUserMsg> List = new ObservableCollection<CustomUserMsg>();
            List.Add(userMsg);
            //调用引擎，发起代办任务
            engineClient.ApplicationMsgTriggerAsync(List, "HR", "T_HR_KPIRECORD", Utility.ObjListToXml<T_HR_KPIRECORD>(KPIRecord, "HR"), MsgType.Task);
        }

        #region 所有服务事件

        /// <summary>
        /// KPI点系统打分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetKPIPointCompleted(object sender, GetKPIPointCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //获取KPI点
                KPIPoint = e.Result;
                if (e.Result != null)
                {
                    //isShow = true;
                    this.companyID = KPIRecord.T_HR_KPIPOINT.SYSTEMID;
                    this.modelRelationID = KPIRecord.T_HR_KPIPOINT.BUSINESSID;
                    this.stepCode = KPIRecord.T_HR_KPIPOINT.STEPID;
                    OnShowFrom(true);
                }
                //判断是否取出了抽查组的人员，没有的话，重新获取
                if (KPIPoint.T_HR_SCORETYPE != null && KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                {
                    if (KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON == null)
                    {
                        client.GetRandomGroupPersonByGroupIDAsync(KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);
                    }
                    else
                        groupPersonList = KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON;
                }
            }
        }

        /// <summary>
        /// 获取该步骤和上一步骤的KPI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetKPIPointAndLastPointCompleted(object sender, GetKPIPointAndLastPointCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                // 1s 冉龙军
                //IsShowKPIScorePnl = false;
                ShowKPIScoreTabItem(false);
                // 1e
                //异常
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                // 1s 冉龙军
                if (e.Result == null)
                {
                    //IsShowKPIScorePnl = false;
                    ShowKPIScoreTabItem(false);
                    return;
                }
                // 1e
                if (e.Result != null)
                {

                    KPIPoint = e.Result.ToList()[0];
                    LastKPIPoint = e.Result.ToList()[1];
                    // 1s 冉龙军
                    //if (LastKPIPoint != null || KPIPoint != null)
                    //    OnShowFrom(true);

                    if (LastKPIPoint != null || KPIPoint != null)
                    {
                        //显示KPI打分面板
                        IsShowKPIScorePnl = true;
                        ShowKPIScoreTabItem(true);
                        OnShowFrom(true);
                    }
                    else
                    {
                        //IsShowKPIScorePnl = false;
                        ShowKPIScoreTabItem(false);
                    }
                    // 1e
                    //获取KPI点

                }

                //判断是否取出了抽查组的人员，没有的话，重新获取
                // 1s
                //if (KPIPoint.T_HR_SCORETYPE != null && KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                //{
                //    if (KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON == null)
                //    {
                //        client.GetRandomGroupPersonByGroupIDAsync(KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);
                //    }
                //    else
                //        groupPersonList = KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON;
                //}
                if (KPIPoint != null)
                {
                    if (KPIPoint.T_HR_SCORETYPE != null)
                    {
                        // 1s 冉龙军
                        //if (KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON == null)
                        //{
                        //    client.GetRandomGroupPersonByGroupIDAsync(KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);
                        //}
                        //else
                        //    groupPersonList = KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON;
                        if (KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                        {
                            if (KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON == null || KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON.Count == 0)
                            {
                                client.GetRandomGroupPersonByGroupIDAsync(KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);
                            }
                            else
                                groupPersonList = KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON;
                        }
                        lblStandDays.Text = KPIPoint.T_HR_SCORETYPE.INITIALPOINT.ToString();
                        //lblStandScore.Text = KPIPoint.T_HR_SCORETYPE.INITIALSCORE.ToString();
                        //lblScoreUnit.Text = KPIPoint.T_HR_SCORETYPE.COUNTUNIT.ToString();
                        lblAddForForward.Text = KPIPoint.T_HR_SCORETYPE.ADDSCORE.ToString();
                        lblMaxSystemScore.Text = KPIPoint.T_HR_SCORETYPE.MAXSCORE.ToString();
                        lblReduceForDelay.Text = KPIPoint.T_HR_SCORETYPE.REDUCESCORE.ToString();
                        lblMinSystemScore.Text = KPIPoint.T_HR_SCORETYPE.MINSCORE.ToString();
                        // 1e
                    }
                    else
                    {
                        lblStandDays.Text = "";
                        //lblStandScore.Text = "";
                        //lblScoreUnit.Text = "";
                        lblAddForForward.Text = "";
                        lblMaxSystemScore.Text = "";
                        lblReduceForDelay.Text = "";
                        lblMinSystemScore.Text = "";
                    }
                    //lblKPIPointName.Text = KPIPoint.KPIPOINTNAME;
                    //lblKPIPointName.Text = KPIPoint.FLOWID;
                }
                else
                {
                    //lblKPIPointName.Text = "";
                    lblStandDays.Text = "";
                    //lblStandScore.Text = "";
                    //lblScoreUnit.Text = "";
                    lblAddForForward.Text = "";
                    lblMaxSystemScore.Text = "";
                    lblReduceForDelay.Text = "";
                    lblMinSystemScore.Text = "";
                }
                if (this.lastStepDate != null && this.stepDate != null)
                {
                    //处理时间
                    TimeSpan ts = this.stepDate - this.lastStepDate;
                    //double diffMinutes = ts.Minutes;
                    //double spendDate = ts.Days * 24 + ts.Hours + diffMinutes / 60;
                    //txtSCORE.Text = string.Format("{0:0.##}", spendDate);
                    //double diffMinutes = ts.Minutes;
                    //double spendDate = ts.Days * 24 + ts.Hours + diffMinutes / 60;
                    //txtSCORE.Text = string.Format("{0:0.##}", spendDate);
                    int spendDate = ts.Days * 24 + ts.Hours;
                    int spendMin = ts.Minutes;
                    if (spendDate < 1)
                    {
                        txtSCORE.Text = spendMin.ToString() + Utility.GetResourceStr("MINUTE");
                    }
                    else
                    {
                        txtSCORE.Text = spendDate.ToString() + Utility.GetResourceStr("HOUR") + spendMin.ToString() + Utility.GetResourceStr("MINUTE");
                    }

                }
                lblLastStepDate.Text = this.lastStepDate.ToString();
                lblStepDate.Text = this.stepDate.ToString();
                lblLastAppraisee.Text = this.AppraiseeID;
                lblLastFinishDate.Text = this.lastStepDate.ToString();
                // 1e

            }
        }

        /// <summary>
        /// KPI点系统打分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetKPIRecordByIdCompleted(object sender, GetKPIRecordByIdCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //获取KPI点 SMT.SAAS.Main.CurrentContext.Common.CurrentConfig.CurrentUser.UserInfo.EMPLOYEEID
                KPIRecord = e.Result;
                if (KPIRecord != null)
                {
                    if (KPIRecord.T_HR_KPIPOINT != null)
                    {
                        KPIPoint = KPIRecord.T_HR_KPIPOINT;
                        if (KPIPoint.T_HR_SCORETYPE != null && KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                        {

                            // 1s 冉龙军
                            //if (KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON == null)
                            //{
                            //    client.GetRandomGroupPersonByGroupIDAsync(KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);
                            //}
                            //else
                            //    groupPersonList = KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON;
                            this.lblStandDays.Text = KPIPoint.T_HR_SCORETYPE.INITIALPOINT.ToString();
                            //this.lblStandScore.Text = KPIPoint.T_HR_SCORETYPE.INITIALSCORE.ToString();
                            //this.lblScoreUnit.Text = KPIPoint.T_HR_SCORETYPE.COUNTUNIT.ToString();
                            this.lblAddForForward.Text = KPIPoint.T_HR_SCORETYPE.ADDSCORE.ToString();
                            this.lblMaxSystemScore.Text = KPIPoint.T_HR_SCORETYPE.MAXSCORE.ToString();
                            this.lblReduceForDelay.Text = KPIPoint.T_HR_SCORETYPE.REDUCESCORE.ToString();
                            this.lblMinSystemScore.Text = KPIPoint.T_HR_SCORETYPE.MINSCORE.ToString();
                            //this.lblKPIPointName.Text = KPIPoint.FLOWID;
                            // 1e
                        }
                        this.companyID = KPIRecord.T_HR_KPIPOINT.SYSTEMID;
                        this.modelRelationID = KPIRecord.T_HR_KPIPOINT.BUSINESSID;
                        this.stepCode = KPIRecord.T_HR_KPIPOINT.STEPID;

                        OnShowFrom(true);
                    }
                    else
                        client.GetKPIPointAsync(companyID, modelRelationID, formID, stepCode);
                    //在抽查的情况下，给界面的参数赋值，以免产生影响过去代码的情况
                    this.lastStepCode = KPIRecord.T_HR_KPIPOINT.STEPID;
                    this.formID = KPIRecord.BUSINESSCODE;
                    this.stepID = KPIRecord.STEPDETAILCODE;
                    this.lastStepID = KPIRecord.STEPDETAILCODE;
                    // 1s 冉龙军
                    //this.lastStepDate = KPIRecord.UPDATEDATE.Value;
                    //this.stepDate = KPIRecord.UPDATEDATE.Value;
                    //this.AppraiseeID = KPIRecord.APPRAISEEID;
                    if (this.lastStepDate != null && this.stepDate != null)
                    {
                        //处理时间
                        TimeSpan ts = this.stepDate - this.lastStepDate;
                        //double diffMinutes = ts.Minutes;
                        //double spendDate = ts.Days * 24 + ts.Hours + diffMinutes / 60;
                        //txtSCORE.Text = string.Format("{0:0.##}", spendDate);
                        int spendDate = ts.Days * 24 + ts.Hours;
                        int spendMin = ts.Minutes;
                        if (spendDate < 1)
                        {
                            txtSCORE.Text = spendMin.ToString() + Utility.GetResourceStr("MINUTE");
                        }
                        else
                        {
                            txtSCORE.Text = spendDate.ToString() + Utility.GetResourceStr("HOUR") + spendMin.ToString() + Utility.GetResourceStr("MINUTE");
                        }
                    }
                    lblLastStepDate.Text = this.lastStepDate.ToString();
                    lblStepDate.Text = this.stepDate.ToString();
                    // 1e
                    this.userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                }
            }
        }

        /// <summary>
        /// 获取抽查组人员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetRandomGroupPersonByGroupIDCompleted(object sender, GetRandomGroupPersonByGroupIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //获取抽查组人员
                if (e.Result != null)
                    groupPersonList = e.Result;
            }
        }

        /// <summary>
        /// KPI点系统打分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_KPISystemScoreCompleted(object sender, KPISystemScoreCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SYSTEMSCOREFIELD"), MessageBoxButton.OK);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SYSTEMSCOREFIELD"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }
                //获取类别
                systemScore = e.Result;
                lblYourScore.Visibility = Visibility.Visible;
                lblSysScore.Visibility = Visibility.Visible;
                lblSysScore.Text = systemScore.ToString();

                //client.KPIManualScoreAsync(companyID, modelRelationID, "", lastStepCode, formID, "", lastStepID, AppraiseeID, userID, 100);
            }
        }

        /// <summary>
        /// KPI点系统打分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_KPIManualScoreCompleted(object sender, KPIManualScoreCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                          Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    //未获取到信息
                    if (e.Result == null)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SYSTEMSCOREFIELD"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    KPIRecord = e.Result;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SCORESUCCESSED") + "！",
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    // 1e
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }

        /// <summary>
        /// KPI点系统打分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_KPIRandomScoreByKPIPointCompleted(object sender, KPIRandomScoreByKPIPointCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SYSTEMSCOREFIELD"), MessageBoxButton.OK);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SYSTEMSCOREFIELD"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }

                //MessageBox.Show(Utility.GetResourceStr("SCORESUCCESSED"), Utility.GetResourceStr("KPITYPE"), MessageBoxButton.OK);
                // 1s 冉龙军
                if (e.Result.RANDOMSCORE != null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SCORESUCCESSED"), string.Format("{0}是{1}", Utility.GetResourceStr("SCORE"), e.Result.RANDOMSCORE.ToString()));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SCORESUCCESSED") + "！",
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                }
                // 1e
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }

        // 1s
        void client_GetKPIRecordScoreDetailCompleted(object sender, GetKPIRecordScoreDetailCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    return;
                }
                else
                {
                    //绑定RadioButton
                    BindRadioButton(e.Result);
                }

            }
        }
        /// <summary>
        /// 1. 通过 AuditEntity 从流程服务中获取流的记录
        /// etc:由控件用户执行此方法,First
        /// </summary>
        public void BindingData(string StepFlowStateCode, string StepRemindGuid, string StepIsKpi, string StepMessgeID)
        {
            FlowStateCode = StepFlowStateCode;
            RemindGuid = StepRemindGuid;
            IsKpi = StepIsKpi;
            MessgeID = StepMessgeID;
            // 2s 冉龙军
            //FlowStateCode = "State836b8ac619054c3b8023f742496059f1";
            //FlowStateCode = "endflow";
            //RemindGuid = "253348bd-6188-431e-b9d9-291c34e423e7";
            //IsKpi = "1";
            //MessgeID = "58466";
            // 2e
            AuditCheck();
            InitParameter();
            //AuditService.GetFlowInfoAsync(this.AuditEntity.FormID, "", "", "", this.AuditEntity.ModelCode,
            //                                this.AuditEntity.CreateCompanyID, "");
            AuditService.GetFlowInfoAsync(this.AuditEntity.FormID, "", "", "", this.AuditEntity.ModelCode,
                                         "", "");
            // 2s 冉龙军
            //AuditEntity.EditUserID = "182c962f-fe66-413c-a17e-ea100e8d9a49";
            //AuditService.GetFlowInfoAsync("93753079-6a87-4e7f-95dc-1f45a796a4e3", "", "", "", "T_HR_EMPLOYEEINSURANCE",
            //                    "703dfb3c-d3dc-4b1d-9bf0-3507ba01b716", "");

            // 2e
        }
        /// <summary>
        /// 检测审核信息
        /// </summary>
        private void AuditCheck()
        {
            string strExceptionMsg = "";
            if (string.IsNullOrEmpty(this.AuditEntity.CreateCompanyID))
            {
                strExceptionMsg = "公司ID不能为空";
            }

            if (string.IsNullOrEmpty(this.AuditEntity.CreateUserID))
            {
                strExceptionMsg = "创建人不能为空";
            }

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

                // 1s 冉龙军（待续）
                //EntityBrowser dt = this.Parent as EntityBrowser;
                //dt.SmtProgressBar.Visibility = Visibility.Collapsed;
                //ComfirmWindow.ConfirmationBox("警告", strExceptionMsg, Utility.GetResourceStr("CONFIRMBUTTON"));
                // 1e
                //throw new Exception(strExceptionMsg);
            }

        }
        /// <summary>
        /// 初始化参数
        /// </summary>
        private void InitParameter()
        {

            //IsShowKPIScorePnl = false;
            ShowKPIScoreTabItem(false);
            this.EditUserID = "";
            this.EditUserName = "";
            //初始化打分(待续)
            client.GetKPIRecordCompleted += new EventHandler<GetKPIRecordCompletedEventArgs>(client_GetKPIRecordCompleted);
            //client.GetKPIRecordRandomPersonIDCompleted += new EventHandler<GetKPIRecordRandomPersonIDCompletedEventArgs>(client_GetKPIRecordRandomPersonIDCompleted);

        }


        void client_GetKPIRecordCompleted(object sender, GetKPIRecordCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //  MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message),
                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                }
                else
                {
                    T_HR_KPIRECORD randomRecord = e.Result;

                    //抽查打分显示面板
                    if (e.UserState.ToString() == "Flow")
                    {
                        #region  流程打分
                        if (randomRecord.RANDOMPERSONID != null && randomRecord.RANDOMPERSONID != "" && randomRecord.RANDOMPERSONID == SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID && randomRecord.RANDOMSCORE == null)
                        {

                            IsShowKPIScorePnl = true;
                            ShowKPIScoreTabItem(true);
                            //KPIScoring(randomRecord.KPIRECORDID,FormTypes.Edit);
                            {
                                FormType = FormTypes.Edit;
                                InitializeComponent();
                                InitPara();
                                client.GetKPIRecordByIdAsync(randomRecord.KPIRECORDID);
                            }
                        }
                        else
                        {

                            //IsShowKPIScorePnl = false;
                            ShowKPIScoreTabItem(false);
                        }
                        #endregion
                    }
                    else
                    {
                        bool isManual = false;
                        bool isRandom = false;
                        if (randomRecord.T_HR_KPIPOINT != null)
                        {
                            if (randomRecord.T_HR_KPIPOINT.T_HR_SCORETYPE != null)
                            {
                                //是否手动打分
                                if (randomRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.ISMANUALSCORE == "1")
                                {
                                    isManual = true;
                                }
                                //是否抽查打分
                                if (randomRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.ISRANDOMSCORE == "1")
                                {
                                    isRandom = true;
                                }
                            }
                        }
                        if ((randomRecord.RANDOMPERSONID != null && randomRecord.RANDOMPERSONID != "" && randomRecord.RANDOMPERSONID == SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID && randomRecord.RANDOMSCORE == null && isRandom)
                            || (IsKpi == null && randomRecord.MANUALSCORE == null && isManual))
                        {
                            client.GetKPIPointAndLastPointAsync(companyID, modelRelationID, formID, stepCode, lastStepCode);
                        }
                        else
                        {
                            ShowKPIScoreTabItem(false);
                        }
                    }

                }
            }
            //throw new NotImplementedException();
        }
        //void client_GetKPIRecordRandomPersonIDCompleted(object sender, GetKPIRecordRandomPersonIDCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        //异常
        //        MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
        //        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        //未获取到信息
        //        if (e.Result == null) { }
        //        else
        //        {

        //            //抽查打分显示面板
        //            string randomUserID = e.Result;
        //            if (randomUserID != "" && randomUserID == SMT.SAAS.Main.CurrentContext.Common.CurrentConfig.CurrentUser.UserInfo.EMPLOYEEID)
        //            {

        //                IsShowKPIScorePnl = true;
        //                ShowKPIScoreTabItem(true);
        //            }
        //            else
        //            {

        //                //IsShowKPIScorePnl = false;
        //                ShowKPIScoreTabItem(false);
        //            }
        //        }
        //    }           
        //}
        void client_KPIRandomScoreCompleted(object sender, KPIRandomScoreCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //MessageBox.Show(Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("ERROR"), MessageBoxButton.OK);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //MessageBox.Show(Utility.GetResourceStr("SYSTEMSCOREFIELD"), Utility.GetResourceStr("ERROR"), MessageBoxButton.OK);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SYSTEMSCOREFIELD"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                // 2s 冉龙军
                //if (e.Result.RANDOMWEIGHT != null)
                //    SetRandomScore();
                // 2e
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("SCORESUCCESSED"), Utility.GetResourceStr("SCORESUCCESSED"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SCORESUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }


        // 1e
        #endregion 所有服务事件

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("KPISCORE");
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
                    isClose = false;
                    break;
                case "1":
                    isClose = true;
                    break;
            }
            Save();
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
            {
                items = CreateFormSaveButton();
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
        /// <summary>
        /// 生成Form上的保存，保存并关闭按钮
        /// </summary>
        /// <returns>按钮列表</returns>
        public static List<ToolbarItem> CreateFormSaveButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                // 1s 冉龙军
                //Title = Utility.GetResourceStr("SUBMIT"),// "保存",
                Title = Utility.GetResourceStr("SAVE"),// "保存",
                // 1e
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                // 1s 冉龙军
                //Title = Utility.GetResourceStr("SUBMITANDCLOSE"),//"保存与关闭"
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),//"保存与关闭"
                // 1e
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            return items;
        }
        #endregion
    }

    // 1s 冉龙军
    #region
    /// <summary>
    /// 流程记录表实体
    /// </summary>
    //public class Flow_FlowRecord_KPI : SMT.Saas.Tools.FlowWFService.EntityObject
    //{
    //    public Flow_FlowRecord_KPI()
    //    {
    //        this.CreateCompanyID = string.Empty;
    //        this.CreateDepartmentID = string.Empty;
    //        this.CreatePostID = string.Empty;
    //        this.CreateUserID = string.Empty;
    //        this.EditUserName = string.Empty;
    //        this.EditUserID = string.Empty;
    //        this.Content = string.Empty;
    //        this.Flag = string.Empty;
    //        this.FlowCode = string.Empty;
    //        this.FormID = string.Empty;
    //        this.GUID = string.Empty;
    //        this.InstanceID = string.Empty;
    //        this.ModelCode = string.Empty;
    //        this.ParentStateCode = string.Empty;
    //        this.StateCode = string.Empty;
    //        this.XmlObject = "";
    //    }
    //    #region 流程记录表属性

    //    public string CreateCompanyID { get; set; }

    //    public DateTime CreateDate { get; set; }

    //    public string CreateDepartmentID { get; set; }
    //    /// <summary>
    //    /// 创建人职位名
    //    /// </summary>
    //    public string CreatePostID { get; set; }
    //    /// <summary>
    //    /// 创建用户ID
    //    /// </summary>
    //    public string CreateUserID { get; set; }
    //    /// <summary>
    //    /// 创建用户名
    //    /// </summary>
    //    public string CreateUserName { get; set; }
    //    /// <summary>
    //    /// 编辑时间
    //    /// </summary>
    //    public DateTime? EditDate { get; set; }
    //    /// <summary>
    //    /// 编辑人ID
    //    /// </summary>
    //    public string EditUserID { get; set; }
    //    /// <summary>
    //    /// 编辑人姓名
    //    /// </summary>
    //    public string EditUserName { get; set; }
    //    /// <summary>
    //    /// 标志
    //    /// </summary>
    //    public string Flag { get; set; }
    //    /// <summary>
    //    /// 流程编码
    //    /// </summary>
    //    public string FlowCode { get; set; }
    //    /// <summary>
    //    /// 表单ID
    //    /// </summary>
    //    public string FormID { get; set; }
    //    /// <summary>
    //    /// 标识ID
    //    /// </summary>
    //    public string GUID { get; set; }
    //    /// <summary>
    //    /// 实例编码
    //    /// </summary>
    //    public string InstanceID { get; set; }
    //    /// <summary>
    //    /// 模块编码
    //    /// </summary>
    //    public string ModelCode { get; set; }
    //    /// <summary>
    //    /// 父状态编码
    //    /// </summary>
    //    public string ParentStateCode { get; set; }
    //    /// <summary>
    //    /// 状态编码
    //    /// </summary>
    //    public string StateCode { get; set; }
    //    /// <summary>
    //    /// 开始时间
    //    /// </summary>
    //    public DateTime StartDate { get; set; }
    //    /// <summary>
    //    /// 结束时间
    //    /// </summary>
    //    public DateTime EndDate { get; set; }
    //    /// <summary>
    //    /// 内容
    //    /// </summary>
    //    public string Content { get; set; }
    //    /// <summary>
    //    /// 流程关联的XML数据
    //    /// </summary>
    //    public string XmlObject { get; set; }

    //    #endregion
    //}
    #endregion
    // 1e
    public class ScoringEventArgs : EventArgs
    {
        public bool IsShow { get; set; }
    }
}
