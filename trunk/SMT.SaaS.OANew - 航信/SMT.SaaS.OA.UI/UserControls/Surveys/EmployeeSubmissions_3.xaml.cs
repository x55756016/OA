using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.OA.UI.UserControls
{

    /// <summary>
    /// 显示每个员工的明细结果
    /// </summary>
    public partial class EmployeeSubmissions_3 : BaseForm, IClient, IEntityEditor
    {
        private SmtOAPersonOfficeClient empSurveysWS;
        private SmtOAPersonOfficeClient _VM;
        private bool IsView = false;
        private T_OA_REQUIREMASTER TmpMaster = new T_OA_REQUIREMASTER();
        private FormTypes type;

        public EmployeeSubmissions_3()
        {
            InitializeComponent();
            
           
        }
        public EmployeeSubmissions_3(T_OA_REQUIREMASTER obj, bool bView, FormTypes ftype)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeSubmissions_3_Loaded);
            IsView = bView;
            TmpMaster = obj;
            type = ftype;
           
        }

        void EmployeeSubmissions_3_Loaded(object sender, RoutedEventArgs e)
        {
            _VM = new SmtOAPersonOfficeClient();
            empSurveysWS = new SmtOAPersonOfficeClient();
            empSurveysWS.SubmitResultCompleted += new EventHandler<SubmitResultCompletedEventArgs>(empSurveysWS_SubmitResultCompleted);
            empSurveysWS.SubmitResultCompleted += new EventHandler<SubmitResultCompletedEventArgs>(empSurveysWS_SubmitResultCompleted);
            _VM.Get_ESurveyCompleted += new EventHandler<Get_ESurveyCompletedEventArgs>(Get_ESurveyCompleted);
            //获取方案 题目，答案
            if (IsView)
            {
                _VM.Get_ESurveyAsync(TmpMaster.REQUIREMASTERID);
            }
            else
            {
                txbTitle.Text = EmployeeSurveyInfo.RequireMaster.REQUIRETITLE;
                txbContent.Text = EmployeeSurveyInfo.RequireMaster.CONTENT;

                GetSubjectSelectInfoByUser();
            }
        }

        void Get_ESurveyCompleted(object sender, Get_ESurveyCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeeSurveyInfo = e.Result as V_EmployeeSurvey;
                txbTitle.Text = EmployeeSurveyInfo.RequireMaster.REQUIRETITLE;
                txbContent.Text = EmployeeSurveyInfo.RequireMaster.CONTENT;

                GetSubjectSelectInfoByUser();
            }
        }

        void empSurveysWS_SubmitResultCompleted(object sender, SubmitResultCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                RefreshUI(RefreshedTypes.CloseAndReloadData);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EmployeeSubmission"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ADDFAILED", "EmployeeSubmission"));
            }
        }
        private void GetSubjectSelectInfoByUser()
        {
            SmtOAPersonOfficeClient empSurveysManage = new SmtOAPersonOfficeClient();
            empSurveysManage.GetResultByUserIDAsync(userID, EmployeeSurveyInfo.RequireMaster.REQUIREMASTERID);
            empSurveysManage.GetResultByUserIDCompleted += new EventHandler<GetResultByUserIDCompletedEventArgs>(empSurveysManage_GetResultByUserIDCompleted);
        }

        void empSurveysManage_GetResultByUserIDCompleted(object sender, GetResultByUserIDCompletedEventArgs e)
        {
            ObservableCollection<T_OA_REQUIRERESULT> subjectResult = e.Result;
            foreach (V_EmployeeSurveySubject subInfo in EmployeeSurveyInfo.SubjectViewList)
            {
                SurveyShowList subjectItem = new SurveyShowList();
                if (subjectResult != null)
                {
                   // subjectItem.ResultDetail = subjectResult.Where(q => q.SUBJECTID == subInfo.SubjectInfo.SUBJECTID).FirstOrDefault();
                }
                subjectItem.SubjectViewInfo = subInfo;
                subjectItem.IsShowResult = 3;
                spSurveysList.Children.Add(subjectItem);

                if (type == FormTypes.Browse)
                {
                    subjectItem.txtComments.IsReadOnly = true;
                }
            }
        }
        private string userID;
        public string UserID
        {
            get
            {
                return userID;
            }
            set { userID = value; }
        }

        private V_EmployeeSurvey employeeSurveyInfo;
        public V_EmployeeSurvey EmployeeSurveyInfo
        {
            get { return employeeSurveyInfo; }
            set { employeeSurveyInfo = value; }
        }

        #region 模式窗口
        public event UIRefreshedHandler OnUIRefreshed;

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
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
        public void DoAction(string actionType) { }

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
            return Utility.GetResourceStr("VIEWTITLE", "INVOLVEDINTHEINVESTIGATION");
        }

        private void CancelAndClose()
        {

        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
            empSurveysWS.DoClose();
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