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
    /// 员工调查--显示结果
    /// </summary>
    public partial class EmployeeSubmissions_1 : BaseForm,IClient, IEntityEditor
    {
        SmtOAPersonOfficeClient _VM;

        /// <summary>
        /// 调查申请
        /// </summary>
        public T_OA_REQUIREDISTRIBUTE Require { get; set; }       
        /// <summary>
        /// 题目答案统计结果
        /// </summary>
        ObservableCollection<V_SatisfactionResult> _osubAnswerResult = new ObservableCollection<V_SatisfactionResult>();
        /// <summary>
        /// 方案
        /// </summary>
        private V_EmployeeSurvey _Survey { get; set; }

        public EmployeeSubmissions_1()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeSubmissions_1_Loaded);
         }

        void EmployeeSubmissions_1_Loaded(object sender, RoutedEventArgs e)
        {
            _VM = new SmtOAPersonOfficeClient();
            _VM.Get_ESurveyCompleted += new EventHandler<Get_ESurveyCompletedEventArgs>(Get_ESurveyCompleted);
            _VM.Result_ESurveyByRequireIDCompleted += new EventHandler<Result_ESurveyByRequireIDCompletedEventArgs>(Result_ESurveyByRequireIDCompleted);
            _VM.Get_ESurveyAsync(Require.T_OA_REQUIRE.T_OA_REQUIREMASTER.REQUIREMASTERID);
        }
       
        void Get_ESurveyCompleted(object sender, Get_ESurveyCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                _Survey = e.Result as V_EmployeeSurvey;
                //获取该申请单的统计结果
                _VM.Result_ESurveyByRequireIDAsync(Require.T_OA_REQUIRE.REQUIREID);
            }
        }      
        void Result_ESurveyByRequireIDCompleted(object sender, Result_ESurveyByRequireIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                _osubAnswerResult = e.Result;
                txbTitle.Text = _Survey.RequireMaster.REQUIRETITLE;
                txbContent.Text = _Survey.RequireMaster.CONTENT;

                foreach (V_EmployeeSurveySubject subInfo in _Survey.SubjectViewList)
                {
                    V_SatisfactionResult v = _osubAnswerResult.FirstOrDefault(ee => ee.SubjectID == subInfo.SubjectInfo.SUBJECTID);

                    SurveyShowList_1 subjectItem = new SurveyShowList_1();
                    subjectItem.SubjectViewInfo = subInfo;
                    subjectItem._SubAnswerResult = v;
                    spSurveysList.Children.Add(subjectItem);
                }
            }
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
        public void DoAction(string actionType) {  }

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

        private void CancelAndClose() { }
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