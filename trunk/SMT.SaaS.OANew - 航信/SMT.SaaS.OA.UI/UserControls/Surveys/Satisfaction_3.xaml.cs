using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    /// 显示每个员工的明细结果
    /// </summary>
    public partial class Satisfaction_3 : BaseForm, IClient, IEntityEditor
    {
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();
     
        /// <summary>
        /// 模板答案
        /// </summary>
        ObservableCollection<T_SYS_DICTIONARY> _oanswer;
        public Satisfaction_3(ObservableCollection<T_SYS_DICTIONARY> oanswer)
        {
            InitializeComponent();
            _VM.Result_SubByUserIDCompleted += new EventHandler<Result_SubByUserIDCompletedEventArgs>(Result_SubByUserIDCompleted);
            _oanswer = oanswer;
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //获取方案 题目，答案
            txbTitle.Text = EmployeeSurveyInfo.RequireMaster.SATISFACTIONTITLE;
            txbContent.Text = EmployeeSurveyInfo.RequireMaster.CONTENT;
            _VM.Result_SubByUserIDAsync(userID, EmployeeSurveyInfo.RequireMaster.SATISFACTIONMASTERID);
            RefreshUI(RefreshedTypes.ProgressBar);
        }
      
        //某个考试人员的调查试卷
        void Result_SubByUserIDCompleted(object sender, Result_SubByUserIDCompletedEventArgs e)
        {
            ObservableCollection<T_OA_SATISFACTIONRESULT> subjectResult = e.Result;
            foreach (T_OA_SATISFACTIONDETAIL subInfo in EmployeeSurveyInfo.SubjectViewList)
            {
                SurveyShowList_sat subjectItem = new SurveyShowList_sat();
                subjectItem.ResultDetail = subjectResult.Where(q => q.SUBJECTID == subInfo.SUBJECTID).FirstOrDefault();
                subjectItem.SubjectViewInfo = subInfo;
                subjectItem.oanswer = _oanswer;
                subjectItem.IsShowResult = 3;
                spSurveysList.Children.Add(subjectItem);
            }
            RefreshUI(RefreshedTypes.ProgressBar);
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

        private V_Satisfaction employeeSurveyInfo;
        public V_Satisfaction EmployeeSurveyInfo
        {
            get {  return employeeSurveyInfo;  }
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
        public void DoAction(string actionType) {}

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
            return Utility.GetResourceStr("EmployeeSurveyDistribute");
        }

        private void CancelAndClose()
        {

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