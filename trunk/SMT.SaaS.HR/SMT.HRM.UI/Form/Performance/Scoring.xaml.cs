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
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.FlowWFService;

namespace SMT.HRM.UI.Form.Performance
{
    public partial class Scoring : BaseForm, IEntityEditor
    {
        // 1s 冉龙军
        public T_HR_KPIRECORD KPIRecord { get; set; }
        public FormTypes FormType { get; set; }
        private PerformanceServiceClient client = new PerformanceServiceClient();
        private ServiceClient fClient = new ServiceClient();     //流程信息服务
        private bool isClose = false;
        private string userID;
        // 1e
        public Scoring()
        {
            InitializeComponent();
        }
        // 1s 冉龙军
        /// <summary>
        /// 引擎调用时的构造函数
        /// </summary>
        /// <param name="KPIRecordID"></param>
        /// <param name="type"></param>
        public Scoring(FormTypes type,string KPIRecordID)
        {
            FormType = type;
            InitializeComponent();
            InitPara();
            client.GetKPIRecordByIdAsync(KPIRecordID);
        }
        // 1e

        // 1s 冉龙军
        /// <summary>
        /// 初始化界面参数
        /// </summary>

        private void InitPara()
        {
            client.GetKPIRecordByIdCompleted += new EventHandler<GetKPIRecordByIdCompletedEventArgs>(client_GetKPIRecordByIdCompleted);
            fClient.GetFlowInfoCompleted += new EventHandler<GetFlowInfoCompletedEventArgs>(fClient_GetFlowInfoCompleted);
            client.KPIRandomScoreByKPIPointCompleted += new EventHandler<KPIRandomScoreByKPIPointCompletedEventArgs>(client_KPIRandomScoreByKPIPointCompleted);
        }
        // 1e
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
                MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //获取KPI点 SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID
                KPIRecord = e.Result;
                if (KPIRecord != null)
                {
                    fClient.GetFlowInfoAsync(KPIRecord.BUSINESSCODE, "", "", "", "", "", "");  //BUSINESSCODE：业务单号（FormID）
                    if ( KPIRecord.T_HR_KPIPOINT != null)
                    {
                        
                        if (KPIRecord.T_HR_KPIPOINT.T_HR_SCORETYPE != null)
                        {
                            lblStandDays.Text = KPIRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.INITIALPOINT.ToString();
                            lblStandScore.Text = KPIRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.INITIALSCORE.ToString();
                            lblScoreUnit.Text = KPIRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.COUNTUNIT.ToString();
                            lblAddForForward.Text = KPIRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.ADDSCORE.ToString();
                            lblMaxSystemScore.Text = KPIRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.MAXSCORE.ToString();
                            lblReduceForDelay.Text = KPIRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.REDUCESCORE.ToString();
                            lblMinSystemScore.Text = KPIRecord.T_HR_KPIPOINT.T_HR_SCORETYPE.MINSCORE.ToString();
                        }
                        this.lblKPIPointName.Text = KPIRecord.T_HR_KPIPOINT.KPIPOINTNAME;
                        //this.lblLastAppraisee.Text;
                        //this.lblLastFinishDate.Text;
                    }
                }
                else
                {
                    MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), MessageBoxButton.OK);
                }
            }
        }
        void fClient_GetFlowInfoCompleted(object sender, GetFlowInfoCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
            }
            else
            {
                if (e.Result == null)
                {
                    //若无结果则显示 无信息面板
                    MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), MessageBoxButton.OK);
                    return;
                }
                else
                {
                    //获取审核信息列表
                    var items = from item in e.Result
                                where item.STATECODE == "State"+KPIRecord.STEPDETAILCODE  //STEPDETAILCODE：步骤单号（包含角色的流程节点）
                                orderby item.EDITDATE
                                select item;
                    
                    if (items.ToList().Count > 0)
                    {
                        lblLastStepDate.Text = items.ToList()[0].CREATEDATE.ToString();
                        lblStepDate.Text = items.ToList()[0].EDITDATE.ToString();
                    }
                    else
                    {
                        //若无结果则显示 无信息面板
                        MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), MessageBoxButton.OK);
                        return;
                    }
                }
            }
        }
        private void btnReviewProcess_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Save()
        {
            int score = 0;
            try
            {
                score = int.Parse(txtSCORE.Text.Trim());
            }
            catch
            {
                //MessageBox.Show("请填写数字", "分数错误", MessageBoxButton.OK);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), "打分不是数字");
                return;
            }
            this.userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            if (KPIRecord != null && KPIRecord.T_HR_KPIPOINT != null)
            {
            }
            else
            {
                //MessageBox.Show("没有打分记录", "打分错误", MessageBoxButton.OK);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), "没有打分记录");
                return;
            }
            client.KPIRandomScoreByKPIPointAsync(KPIRecord.T_HR_KPIPOINT, KPIRecord.BUSINESSCODE, KPIRecord.KPIRECORDID, KPIRecord.STEPDETAILCODE, KPIRecord.APPRAISEEID, userID, score);
            
        }
        /// <summary>
        /// KPI点抽查打分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_KPIRandomScoreByKPIPointCompleted(object sender, KPIRandomScoreByKPIPointCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message), MessageBoxButton.OK);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //MessageBox.Show(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SYSTEMSCOREFIELD"), MessageBoxButton.OK);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }

                //MessageBox.Show(Utility.GetResourceStr("SCORESUCCESSED"), Utility.GetResourceStr("KPITYPE"), MessageBoxButton.OK);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SCORESUCCESSED"), string.Format("{0}是{1}", Utility.GetResourceStr("SCORE"), e.Result.RANDOMSCORE.ToString()));

                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }
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

}
