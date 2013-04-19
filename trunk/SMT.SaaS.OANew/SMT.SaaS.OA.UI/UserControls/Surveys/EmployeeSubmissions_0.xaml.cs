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
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.Saas.Tools.FlowWFService;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    /// 参与调查
    /// </summary>
    public partial class EmployeeSubmissions_0 : BaseForm, IClient, IEntityEditor
    {
        private SmtOAPersonOfficeClient empSurveysWS;
        private SmtOAPersonOfficeClient _VM;

        public EmployeeSubmissions_0()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeSubmissions_0_Loaded);
         }

        void EmployeeSubmissions_0_Loaded(object sender, RoutedEventArgs e)
        {
            empSurveysWS = new SmtOAPersonOfficeClient();
            _VM = new SmtOAPersonOfficeClient();
            empSurveysWS.SubmitResultCompleted += new EventHandler<SubmitResultCompletedEventArgs>(empSurveysWS_SubmitResultCompleted);
            _VM.Get_ESurveyCompleted += new EventHandler<Get_ESurveyCompletedEventArgs>(Get_ESurveyCompleted);
            _VM.Get_ESurveyAsync(require.T_OA_REQUIREMASTER.REQUIREMASTERID);
        }
      
        void Get_ESurveyCompleted(object sender, Get_ESurveyCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeeSurveyInfo = e.Result as V_EmployeeSurvey;
                txbTitle.Text = EmployeeSurveyInfo.RequireMaster.REQUIRETITLE;
                txbContent.Text = EmployeeSurveyInfo.RequireMaster.CONTENT;
                //题目，答案
                foreach (V_EmployeeSurveySubject subInfo in EmployeeSurveyInfo.SubjectViewList)
                {
                    SurveyShowList subjectItem = new SurveyShowList();
                    subjectItem.SubjectViewInfo = subInfo;
                    subjectItem.IsShowResult = 0;
                    spSurveysList.Children.Add(subjectItem);
                }
            }
        }
        void empSurveysWS_SubmitResultCompleted(object sender, SubmitResultCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                RefreshUI(RefreshedTypes.CloseAndReloadData);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("THESUCCESSFULSUBMISSIONOFINVESTIGATION"));
            }
            else
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("FAILURETOSUBMITTHEINVESTIGATION"));
            }
        }

        private void SaveResult()
        {
            ObservableCollection<T_OA_REQUIRERESULT> resultList = new ObservableCollection<T_OA_REQUIRERESULT>();
            SurveyShowList ssL = null;
            T_OA_REQUIRERESULT resultInfo = null;
            bool AnswerIsNull = true;
            string StrAnswer = "";
            for (int i = 0; i < spSurveysList.Children.Count; i++)
            {
                ssL = new SurveyShowList();
                ssL = spSurveysList.Children[i] as SurveyShowList;
                if (string.IsNullOrEmpty(ssL.ResultDetail.RESULT))
                {
                    AnswerIsNull = false;
                    StrAnswer += (i + 1).ToString() + ",";
                    continue;
                }
                resultInfo = ssL.ResultDetail;
                resultInfo.REQUIRERESULTID = System.Guid.NewGuid().ToString();
                resultInfo.T_OA_REQUIREMASTER = employeeSurveyInfo.RequireMaster;
                resultInfo.T_OA_REQUIRE = require;

                resultInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                resultInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                resultInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                resultInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                resultInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                resultInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                resultInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                resultInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID; 
                resultInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                resultInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                resultInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                resultInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                resultInfo.UPDATEDATE = System.DateTime.Now;

                resultList.Add(resultInfo);
            }
            if (!AnswerIsNull)
            {
                StrAnswer = StrAnswer.Substring(0, StrAnswer.Length - 1);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), "第" + StrAnswer + "道题没有答案");
            }
            if (resultList.Count() > 0)
            {
                empSurveysWS.SubmitResultAsync(resultList);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), "请填写答案");
            }

        }

        private string userID;
        public string UserID { get { return userID; } set { userID = value; } }
        private T_OA_REQUIRE require;
        /// <summary>
        /// 调查申请
        /// </summary>
        public T_OA_REQUIRE Require { get { return require; } set { require = value; } }
        private V_EmployeeSurvey employeeSurveyInfo;
        public V_EmployeeSurvey EmployeeSurveyInfo { get { return employeeSurveyInfo; } set { employeeSurveyInfo = value; } }

        #region 模式窗口
        public event UIRefreshedHandler OnUIRefreshed;

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"
            };
            items.Add(item);
            return items;
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }
        private bool isSubmitFlow = false;
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    isSubmitFlow = false;
                    SaveResult();
                    break;
                case "1":
                    CancelAndClose();
                    break;
            }
        }

        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("INVOLVEDINTHEINVESTIGATION");
        }

        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
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