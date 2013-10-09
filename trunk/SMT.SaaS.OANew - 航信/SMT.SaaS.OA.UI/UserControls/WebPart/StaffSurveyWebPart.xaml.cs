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
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.UserControls.WebPart
{
    public partial class StaffSurveyWebPart : BaseForm,IClient, IEntityEditor
    {

        #region 全局变量
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();

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
        private string t_rqId = string.Empty;
        #endregion

        #region 构造函数
        public StaffSurveyWebPart(string rqId)
        {
            InitializeComponent();
            t_rqId = rqId;
            _VM.Get_ESurveyCompleted += new EventHandler<Get_ESurveyCompletedEventArgs>(Get_ESurveyCompleted);
            _VM.Result_ESurveyByRequireIDCompleted += new EventHandler<Result_ESurveyByRequireIDCompletedEventArgs>(Result_ESurveyByRequireIDCompleted);
            _VM.Get_ESurveyResultCompleted += new EventHandler<Get_ESurveyResultCompletedEventArgs>(Get_ESurveyResultCompleted);
            _VM.Get_ESurveyResultAsync(t_rqId);
        }
        #endregion

        #region 根据ID获取
        void Get_ESurveyResultCompleted(object sender, Get_ESurveyResultCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                Require = e.Result;

                _VM.Get_ESurveyAsync(Require.T_OA_REQUIRE.T_OA_REQUIREMASTER.REQUIREMASTERID);
            }
        }
        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //获取方案 题目，答案
            //_VM.Get_ESurveyAsync(Require.T_OA_REQUIRE.T_OA_REQUIREMASTER.REQUIREMASTERID);
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
