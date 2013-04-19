/// <summary>
/// Log No.： 1
/// Modify Desc： 查看审核的Combobox绑定
/// Modifier： 冉龙军
/// Modify Date： 2010-08-10
/// </summary>
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PerformanceWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Performance
{
    public partial class ComplainProcess : BaseForm, IEntityEditor
    {
        public T_HR_KPITYPE KPIType { get; set; }
        public FormTypes FormType { get; set; }
        private PerformanceServiceClient client = new PerformanceServiceClient();
        public ObservableCollection<T_HR_KPIRECORDCOMPLAIN> complainList = new ObservableCollection<T_HR_KPIRECORDCOMPLAIN>();//申诉列表
        public int complainIndex = 0;
        public T_HR_KPIRECORDCOMPLAIN Complain { get; set; }
        public T_HR_KPIRECORD Record { get; set; }
        private string kpiRecordID = string.Empty;
        private string complainId = string.Empty;

        public ComplainProcess(FormTypes type, string kpiRecordID, string complainId)
        {
            FormType = type;
            this.kpiRecordID = kpiRecordID;
            this.complainId = complainId;
            InitializeComponent();
            SetComplainIsEnable(false);
            SetReviewIsEnable(false);
            SetNextButtonIsVisable(false);
            InitPara();
            InitFormControl();
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
        }

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

        /// <summary>
        /// 设置申诉信息可用性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetComplainIsEnable(bool isEnable)
        {
            txtComplainReason.IsEnabled = isEnable;
        }

        /// <summary>
        /// 设置审核信息可用性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetReviewIsEnable(bool isEnable)
        {
            cboAppraisalResult.IsEnabled = isEnable;
            //是否可以打分由审批结果决定
            txtAppraisalScore.IsEnabled = false;
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
        /// 保存
        /// </summary>
        private void Save()
        {
            //处理页面验证
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                //  RefreshUI(RefreshedTypes.HideProgressBar);
                return ;
            }
            else
            {
                Complain.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                Complain.UPDATEDATE = DateTime.Now;

                if(FormType == FormTypes.New)
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
                else if(FormType == FormTypes.Audit)
                {
                    //审批
                    Complain.REVIEWERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.REVIEWDATE = DateTime.Now;
                    Complain.CHECKSTATE = cboAppraisalResult.SelectedIndex.ToString();
                    // 1s 冉龙军
                    //try
                    //{
                    //    Complain.REVIEWSCORE = int.Parse(txtAppraisalScore.Text.Trim());
                    //}
                    //catch { }
                    try
                    {
                        
                        if (txtAppraisalScore.Text.Trim() == "")
                        {
                            //审核通过或者未填
                            if (txtAppraisalScore.IsEnabled == true)
                            {
                                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOFOUNDSCORE"));
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOFOUNDSCORE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }

                        }
                        else
                        {
                            Complain.REVIEWSCORE = int.Parse(txtAppraisalScore.Text.Trim());
                        }

                    }
                    catch
                    {
                        throw new Exception();
                    }
                    // 1e
                    Complain.REVIEWREMARK = txtAppraisalRemark.Text.Trim();
                    client.UpdateKPIRecordComplainAsync(Complain);
                }
            }
        }

        /// <summary>
        /// 保存并关闭
        /// </summary>
        private void SaveAndCancel()
        {
            Save();
        }

        /// <summary>
        /// 窗口为浏览的时候，绑定数据
        /// </summary>
        private void BindListData()
        {
            this.Complain = complainList[complainIndex] as T_HR_KPIRECORDCOMPLAIN;
            btnPreview.IsEnabled = complainIndex == complainList.Count - 1 ? false : true;
            btnNext.IsEnabled = complainIndex == 0 ? false : true;
            LayoutRoot.DataContext = Complain;
            if (Complain.CHECKSTATE != null && !Complain.CHECKSTATE.Trim().Equals(""))
                cboAppraisalResult.SelectedIndex = int.Parse(Complain.CHECKSTATE.Trim());
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
                    SetNextButtonIsVisable(false);
                    return;
                }
                //获取申诉记录列表
                complainList = e.Result;
                //如果是审批
                if (FormType == FormTypes.Audit)
                {
                    //获取需要审批的申诉记录
                    this.Complain = complainList[complainIndex];
                    complainId = Complain.COMPLAINID;
                    //审批信息
                    Complain.REVIEWERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.REVIEWDATE = DateTime.Now;
                    Complain.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Complain.UPDATEDATE = DateTime.Now;
                    LayoutRoot.DataContext = Complain;
                    // 1s 冉龙军
                    if (Complain.CHECKSTATE != null && !Complain.CHECKSTATE.Trim().Equals(""))
                        cboAppraisalResult.SelectedIndex = int.Parse(Complain.CHECKSTATE.Trim());
                    // 1e
                }
                else
                    BindListData();
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
                bool result = e.Result;
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
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
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
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "KPIRECORDCOMPLAIN"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.All);
                FormType = FormTypes.Browse;
                SetComplainIsEnable(false);
                SetReviewIsEnable(false);
                InitFormControl();
            }
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
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "KPIRECORDCOMPLAIN"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.All);
                FormType = FormTypes.Browse;
                SetComplainIsEnable(false);
                SetReviewIsEnable(false);
                InitFormControl();
            }
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
            //Scoring s = new Scoring(FormTypes.New, "0cb73ec9-e05c-4f6c-b6f4-5337afa160a6");
            //SMT.HRM.UI.Utility.CreateFormFromEngine("0cb73ec9-e05c-4f6c-b6f4-5337afa160a6", "SMT.HRM.UI.Form.Performance.Scoring", "Edit");
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
                    SaveAndCancel();
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
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
            {
                items = Utility.CreateFormSaveButton();
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
    }
}
