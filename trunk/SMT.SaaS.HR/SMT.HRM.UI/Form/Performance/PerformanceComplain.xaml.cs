/// <summary>
/// Log No.： 1
/// Modify Desc： 判断申诉（含重新申诉）
/// Modifier： 冉龙军
/// Modify Date： 2010-09-16
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

using SMT.Saas.Tools.PerformanceWS;
using System.Collections;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Performance
{
    public partial class PerformanceComplain : BaseForm, IEntityEditor, IAudit, IClient
    {
        // 1s 冉龙军
        //public FormTypes FormType { get; set; }
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        // 1e
        private PerformanceServiceClient client = new PerformanceServiceClient();
        public ObservableCollection<T_HR_KPIRECORDCOMPLAIN> complainList = new ObservableCollection<T_HR_KPIRECORDCOMPLAIN>();//申诉列表
        public int complainIndex = 0;
        public T_HR_KPIRECORDCOMPLAIN Complain { get; set; }
        public T_HR_KPIRECORD Record { get; set; }
        private string kpiRecordID = string.Empty;
        private string complainId = string.Empty;
        private bool auditsign = false, signCancel = false;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        // 1s 冉龙军
        // 待完善
        public PerformanceComplain(FormTypes type, string complainId)
        {
            InitializeComponent();

            FormType = type;
            InitPara();
        }

        // 1e

        public PerformanceComplain(FormTypes type, string kpiRecordID, string complainId)
        {
            InitializeComponent();
            FormType = type;
            this.kpiRecordID = kpiRecordID;
            this.complainId = complainId;
            SetComplainIsEnable(false);
            SetReviewIsEnable(false);
            SetNextButtonIsVisable(false);
            InitPara();

            // 1s 冉龙军
            //if (FormType == FormTypes.Edit) { txtComplainReason.Visibility = Visibility.Visible; }
            InitFormControl();
            // 1e
        }

        /// <summary>
        /// 初始化窗口事件
        /// </summary>
        private void InitPara()
        {
            //加载事件
            client.GetKPIRecordComplainByRecordCompleted += new EventHandler<GetKPIRecordComplainByRecordCompletedEventArgs>(client_GetKPIRecordComplainByRecordCompleted);
            client.GetKPIRecordComplainByIDCompleted += new EventHandler<GetKPIRecordComplainByIDCompletedEventArgs>(client_GetKPIRecordComplainByIDCompleted);
            client.GetKPIRecordByIdCompleted += new EventHandler<GetKPIRecordByIdCompletedEventArgs>(client_GetKPIRecordByIdCompleted);
            client.CheckRecordIsSummarizeCompleted += new EventHandler<CheckRecordIsSummarizeCompletedEventArgs>(client_CheckRecordIsSummarizeCompleted);
            client.AddKPIRecordComplainCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AddKPIRecordComplainCompleted);
            client.UpdateKPIRecordComplainCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_UpdateKPIRecordComplainCompleted);
            client.KPIRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_KPIRecordUpdateCompleted);

            // 1s 冉龙军
            //if (FormType == FormTypes.New)
            //{
            //    client.CheckRecordIsSummarizeAsync(kpiRecordID);
            //}
            //else
            //{
            //    client.GetKPIRecordComplainByRecordAsync(kpiRecordID);
            //}
            // 1e
        }

        void client_KPIRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        // 1s 冉龙军
        /// <summary>
        /// 初始化窗口状态
        /// </summary>
        private void InitFormControl()
        {
            //窗口状态
            switch ((int)FormType)
            {
                case 0://NEW
                    // 检查是否可以申诉
                    client.CheckRecordIsSummarizeAsync(kpiRecordID);
                    break;
                case 1://EDIT
                    break;
                case 2: //BROWE
                    SetNextButtonIsVisable(true);
                    //获取所有申诉信息
                    client.GetKPIRecordComplainByRecordAsync(kpiRecordID);
                    break;
                case 3: //ADUIT
                    SetReviewIsEnable(true);
                    if (complainId != "")
                        client.GetKPIRecordComplainByIDAsync(complainId);
                    else if (kpiRecordID != "")
                        client.GetKPIRecordComplainByRecordAsync(kpiRecordID);
                    break;
            }
        }
        // 1e
        private void SetToolBar()
        {
            // 1s 冉龙军
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_KPIRECORDCOMPLAIN", Complain.OWNERID,
            //        Complain.OWNERPOSTID, Complain.OWNERDEPARTMENTID, Complain.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse) return;
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_KPIRECORDCOMPLAIN", Complain.OWNERID,
                    Complain.OWNERPOSTID, Complain.OWNERDEPARTMENTID, Complain.OWNERCOMPANYID);
            //if (FormType == FormTypes.Browse) return;
            //if (FormType == FormTypes.New)
            //    ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_KPIRECORDCOMPLAIN", Complain.OWNERID,
            //        Complain.OWNERPOSTID, Complain.OWNERDEPARTMENTID, Complain.OWNERCOMPANYID);

            //if (FormType == FormTypes.Edit)
            //{
            //    ToolbarItems = Utility.CreateFormEditButton();
            //}
            //else
            //    ToolbarItems = Utility.CreateFormEditButton("T_HR_KPIRECORDCOMPLAIN", Complain.OWNERID,
            //        Complain.OWNERPOSTID, Complain.OWNERDEPARTMENTID, Complain.OWNERCOMPANYID);
            RefreshUI(RefreshedTypes.All);
            // 1e
        }

        /// <summary>
        /// 设置申诉信息可用性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetComplainIsEnable(bool isEnable)
        {
            txtComplainReason.IsEnabled = isEnable;
            // 1s 冉龙军
            // 审核得分由用户填
            txtAppraisalScore.IsEnabled = isEnable;
            // 1e
        }

        /// <summary>
        /// 设置审核信息可用性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetReviewIsEnable(bool isEnable)
        {
            cboAppraisalResult.IsEnabled = isEnable;
            //是否可以打分由审批结果决定

            // 1s 冉龙军
            // 审核得分由用户填
            //txtAppraisalScore.IsEnabled = false;
            // 1e

            txtAppraisalRemark.IsEnabled = isEnable;
        }

        /// <summary>
        /// 设置浏览导航按钮可见性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetNextButtonIsVisable(bool isEnable)
        {
            btnPreview.Visibility = isEnable == true ? Visibility.Visible : Visibility.Collapsed;
            btnNext.Visibility = isEnable == true ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 窗口为浏览的时候，绑定数据
        /// </summary>
        private void BindListData()
        {
            this.Complain = complainList[complainIndex] as T_HR_KPIRECORDCOMPLAIN;
            //btnPreview.IsEnabled = complainIndex == complainList.Count - 1 ? false : true;
            //btnNext.IsEnabled = complainIndex == 0 ? false : true;
            LayoutRoot.DataContext = Complain;
            // 1s 冉龙军
            //if (Complain.CHECKSTATE != null && !Complain.CHECKSTATE.Trim().Equals(""))
            //    cboAppraisalResult.SelectedIndex = int.Parse(Complain.CHECKSTATE.Trim());
            // 1e
        }

        #region 所有事件

        /// <summary>
        /// 根据KPI明细记录ID获取申诉数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetKPIRecordComplainByRecordCompleted(object sender, GetKPIRecordComplainByRecordCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("NOTFOUNDCOMPLAIN"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUNDCOMPLAIN"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //SetNextButtonIsVisable(false);
                    return;
                }
                //获取申诉记录列表
                complainList = e.Result;
                // 1s 冉龙军
                ////如果是审批
                //if (FormType == FormTypes.Audit)
                //{
                //    //获取需要审批的申诉记录
                //    this.Complain = complainList[complainIndex];
                //    complainId = Complain.COMPLAINID;
                //    //审批信息
                //    Complain.REVIEWERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //    Complain.REVIEWDATE = DateTime.Now;
                //    Complain.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //    Complain.UPDATEDATE = DateTime.Now;
                //    LayoutRoot.DataContext = Complain;
                //    if (Complain.CHECKSTATE != null && !Complain.CHECKSTATE.Trim().Equals(""))
                //        cboAppraisalResult.SelectedIndex = int.Parse(Complain.CHECKSTATE.Trim());
                //}
                //else if (FormType == FormTypes.Edit)
                //{
                //    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                //    entBrowser.FormType = FormTypes.Edit;
                //}
                // 1e
                BindListData();
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        /// <summary>
        /// 获取KPI记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_CheckRecordIsSummarizeCompleted(object sender, CheckRecordIsSummarizeCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //获取类别
                bool result = e.Result; result = false;
                if (!result)
                {
                    Complain = new T_HR_KPIRECORDCOMPLAIN();
                    Complain.COMPLAINID = Guid.NewGuid().ToString();
                    Complain.COMPLAINANTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.COMPLAINDATE = DateTime.Now;

                    Complain.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.CREATEDATE = DateTime.Now;
                    Complain.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.UPDATEDATE = DateTime.Now;
                    Complain.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();

                    // 1s 冉龙军
                    SetToolBar();
                    // 1e
                    SetComplainIsEnable(true);
                    //获取KPI明细记录
                    client.GetKPIRecordByIdAsync(kpiRecordID);
                }
                else
                {
                    // 1s 冉龙军
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("ALREADYSUMMARIZE", "KPIRECORDCOMPLAIN"));
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("ALREADYSUMMARIZE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ALREADYSUMMARIZE"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    // 1e
                    return;
                }
            }
        }

        /// <summary>
        /// 获取KPI记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetKPIRecordByIdCompleted(object sender, GetKPIRecordByIdCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
 Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
 Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                //获取类别
                this.Record = e.Result;

                if (Record.COMPLAINSTATUS == "2")
                {
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.Close();
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("本条记录已申诉完成了"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("本条记录已申诉完成了"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                Complain.T_HR_KPIRECORD = this.Record;
                Complain.INITIALSCORE = this.Record.SUMSCORE;
                LayoutRoot.DataContext = Complain;

                //lblKPIRecordName.Text = Record.T_HR_KPIPOINT.KPIPOINTNAME;
            }
        }

        /// <summary>
        /// 获取申诉
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetKPIRecordComplainByIDCompleted(object sender, GetKPIRecordComplainByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                //获取申诉记录
                this.Complain = e.Result;

                //审批
                Complain.REVIEWERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                Complain.REVIEWDATE = DateTime.Now;
                Complain.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                Complain.UPDATEDATE = DateTime.Now;
                //获取KPI明细记录
                //client.GetKPIRecordByIdAsync(kpiRecordID);
                LayoutRoot.DataContext = Complain;
                //lblKPIRecordName.Text = Record.T_HR_KPIPOINT.KPIPOINTNAME;

                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        /// <summary>
        /// 添加完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AddKPIRecordComplainCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPIRECORDCOMPLAIN"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPIRECORDCOMPLAIN"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                FormType = FormTypes.Edit;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "KPIRECORDCOMPLAIN"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (signCancel)
                {
                    signCancel = false;
                    entBrowser.Close();
                }

                RefreshUI(RefreshedTypes.All);
                SetComplainIsEnable(false);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 更新完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_UpdateKPIRecordComplainCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPIRECORDCOMPLAIN"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPIRECORDCOMPLAIN"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                if (auditsign)
                {
                    T_HR_KPIRECORD kpirecord = new T_HR_KPIRECORD();
                    kpirecord = Record;
                    kpirecord.COMPLAINSTATUS = "1";
                    client.KPIRecordUpdateAsync(kpirecord);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITAUDITSUCCESSFUL"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITAUDITSUCCESSFUL"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "KPIRECORDCOMPLAIN"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                }
                if (signCancel)
                {
                    signCancel = false;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.Close();
                }
                //FormType = FormTypes.Browse;
                SetComplainIsEnable(false);
                //SetReviewIsEnable(false);
                // InitFormControl();
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 上一条记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (complainIndex != complainList.Count - 1)
            {
                complainIndex += 1;
                btnNext.IsEnabled = true;
            }
            BindListData();

        }

        /// <summary>
        /// 下一条记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (complainIndex != 0)
            {
                complainIndex -= 1;
                btnPreview.IsEnabled = true;
            }
            BindListData();
        }

        /// <summary>
        /// 审批结果事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboAppraisalResult_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            txtAppraisalScore.IsEnabled = cboAppraisalResult.SelectedIndex == 1 && this.FormType == FormTypes.Audit ? true : false;
        }

        #endregion 所有事件

        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            //处理页面验证
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                Complain.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                Complain.UPDATEDATE = DateTime.Now;

                if (FormType == FormTypes.New)
                {
                    //提出申诉
                    Complain.COMPLAINREMARK = txtComplainReason.Text.Trim();

                    //所属
                    Complain.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    Complain.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    Complain.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    Complain.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    Complain.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    Complain.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                    Complain.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.CREATEDATE = DateTime.Now;

                    client.AddKPIRecordComplainAsync(Complain);
                }
                // 1s 冉龙军
                //else if(FormType == FormTypes.Edit)
                else if (FormType == FormTypes.Audit)
                // 1e
                {
                    Complain.REVIEWERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.UPDATEDATE = DateTime.Now;
                    try
                    {

                        if (txtAppraisalScore.Text.Trim() == "")
                        {
                            //审核通过或者未填
                            if (txtAppraisalScore.IsEnabled == true)
                            {
                                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOFOUNDSCORE"));
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOFOUNDSCORE"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                                return;
                            }

                        }
                        else
                        {
                            //Complain.REVIEWSCORE = int.Parse(txtAppraisalScore.Text.Trim());
                            // 1s 冉龙军
                            Complain.REVIEWSCORE = int.Parse(txtAppraisalScore.Text.Trim());
                            // 1e
                        }

                    }
                    catch (Exception ex)
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        return;
                    }
                    Complain.REVIEWREMARK = txtAppraisalRemark.Text.Trim();
                    client.UpdateKPIRecordComplainAsync(Complain);
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
        }

        /// <summary>
        /// 保存并关闭
        /// </summary>
        private void Cancel()
        {
            signCancel = true;
            Save();
        }

        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EntityKey", Complain.COMPLAINID);

            Dictionary<string, string> para2 = new Dictionary<string, string>();

            string strXmlObjectSource = string.Empty;
            string strKeyName = "COMPLAINID";
            string strKeyValue = Complain.COMPLAINID;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_KPIRECORDCOMPLAIN>(Complain, para, "HR", para2, strKeyName, strKeyValue);

            // 1s 冉龙军
            //Utility.SetAuditEntity(entity, "T_HR_KPIRECORDCOMPLAIN", Complain.COMPLAINANTID, strXmlObjectSource);
            Utility.SetAuditEntity(entity, "T_HR_KPIRECORDCOMPLAIN", Complain.COMPLAINID, strXmlObjectSource);
            // 1e
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            auditsign = true;
            //Utility.UpdateCheckState("T_HR_KPIRECORDCOMPLAIN", "COMPLAINID", Complain.COMPLAINID, args);
            string state = "";
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
            Complain.CHECKSTATE = state;
            //Complain.CHECKSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            client.UpdateKPIRecordComplainAsync(Complain);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (Complain != null)
                state = Complain.CHECKSTATE;
            return state;
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("KPIRECORDCOMPLAIN");
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
                    Cancel();
                    break;
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("KPIRECORDCOMPLAIN"),
                Tooltip = Utility.GetResourceStr("KPIRECORDCOMPLAIN")
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
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

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion
    }
}
