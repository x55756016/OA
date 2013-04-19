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
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.UserControls.WebPart
{
    public partial class SatisfactionSurveyWebPart : BaseForm, IClient, IEntityEditor
    {
        #region 全局变量
        private SMTLoading loadbar = new SMTLoading();

        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();
        /// <summary>
        /// 取字典 答案
        /// </summary>
        private PermissionServiceClient permissionClient = new PermissionServiceClient();
        /// <summary>
        /// 模板答案
        /// </summary>
        ObservableCollection<T_SYS_DICTIONARY> _oanswer;
        /// <summary>
        /// 答案结果统计 
        /// </summary>
        ObservableCollection<V_SatisfactionResult> _osubAnswerResult = new ObservableCollection<V_SatisfactionResult>();
        /// <summary>
        /// 统计总票数
        /// </summary> 
        int Count = 0;
        /// <summary>
        /// 达到答案级别的 统计票数
        /// </summary> 
        int AnswerID_count = 0;

        private void SaveResult() { }
        private string userID;
        public string UserID
        {
            get { return userID; }
            set { userID = value; }
        }
        private T_OA_SATISFACTIONDISTRIBUTE distribute;
        /// <summary>
        /// 调查申请
        /// </summary>
        public T_OA_SATISFACTIONDISTRIBUTE Distribute { get { return distribute; } set { distribute = value; } }
        private V_Satisfaction employeeSurveyInfo;
        public V_Satisfaction EmployeeSurveyInfo
        {
            get { return employeeSurveyInfo; }
            set { employeeSurveyInfo = value; }
        }
        private string t_Sid = string.Empty;
        #endregion

        #region 构造函数
        public SatisfactionSurveyWebPart(string sfId)
        {
            InitializeComponent();
            this.t_Sid = sfId;
            permissionClient.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(GetSysDictionaryByFatherIDCompleted);
            _VM.Get_SSurveyCompleted += new EventHandler<Get_SSurveyCompletedEventArgs>(Get_SSurveyCompleted);
            _VM.Result_SurveyByRequireIDCompleted += new EventHandler<Result_SurveyByRequireIDCompletedEventArgs>(Result_SurveyByRequireIDCompleted);
            _VM.Get_SSurveyResultCompleted += new EventHandler<Get_SSurveyResultCompletedEventArgs>(Get_SSurveyResultCompleted);
            _VM.Get_SSurveyResultAsync(t_Sid);
        }
        #endregion

        #region 根据ID查询
        void Get_SSurveyResultCompleted(object sender, Get_SSurveyResultCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                if (e.Result != null)
                {
                    distribute = e.Result;

                    permissionClient.GetSysDictionaryByFatherIDAsync(distribute.T_OA_SATISFACTIONREQUIRE.ANSWERGROUPID);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            loadbar.Start();
            //permissionClient.GetSysDictionaryByFatherIDAsync(distribute.T_OA_SATISFACTIONREQUIRE.ANSWERGROUPID);
        }

        #region 答案
        void GetSysDictionaryByFatherIDCompleted(object sender, GetSysDictionaryByFatherIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                //获取方案 题目，答案
                _VM.Get_SSurveyAsync(distribute.T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID);
                _oanswer = e.Result;
            }
            else
                loadbar.Stop();
        }
        #endregion

        #region 获取方案 题目，答案
        void Get_SSurveyCompleted(object sender, Get_SSurveyCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeeSurveyInfo = e.Result as V_Satisfaction;

                txtAnswerID.Text = distribute.ANSWERID;
                txtPercentAge.Text = distribute.PERCENTAGE.ToString();
                txtStartDate.Text = Convert.ToDateTime(distribute.T_OA_SATISFACTIONREQUIRE.STARTDATE).ToShortDateString();
                txtEndDate.Text = Convert.ToDateTime(distribute.T_OA_SATISFACTIONREQUIRE.ENDDATE).ToShortDateString();
                txbTitle.Text = EmployeeSurveyInfo.RequireMaster.SATISFACTIONTITLE;
                txbContent.Text = EmployeeSurveyInfo.RequireMaster.CONTENT;

                //获取统计结果
                _VM.Result_SurveyByRequireIDAsync(distribute.T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID);
            }
            else
                loadbar.Stop();
        }
        #endregion

        #region 统计结果
        void Result_SurveyByRequireIDCompleted(object sender, Result_SurveyByRequireIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                _osubAnswerResult = e.Result;
                foreach (T_OA_SATISFACTIONDETAIL subInfo in EmployeeSurveyInfo.SubjectViewList)
                {
                    V_SatisfactionResult v = _osubAnswerResult.FirstOrDefault(ee => ee.SubjectID == subInfo.SUBJECTID);

                    SurveyShowList_sat_1 subjectItem = new SurveyShowList_sat_1(SumResult);
                    subjectItem.SubjectViewInfo = subInfo;
                    subjectItem.oanswer = _oanswer;
                    subjectItem.SubAnswerResult = v;
                    subjectItem.AnswerID = distribute.ANSWERID;
                    subjectItem.PercentAge =Convert.ToDecimal(distribute.PERCENTAGE);

                    spSurveysList.Children.Add(subjectItem);
                }

            }
            loadbar.Stop();
        }
        #endregion

        #region SumResult
        private void SumResult(object sender, SatisfactionResultEventArgs e)
        {
            Count += e.Count;
            AnswerID_count += e.AnswerID_count;

            //-----------------
            //该调查统计结果
            decimal d = decimal.Round(Convert.ToDecimal(AnswerID_count) / Convert.ToDecimal((Count == 0 ? 1 : Count)) * 100, 1);
            if (d >= distribute.PERCENTAGE)
                txtResult.Text = "通过( " + AnswerID_count.ToString() + "/" + Count.ToString() + " = " + d.ToString() + "% )";
            else
                txtResult.Text = "不通过( " + AnswerID_count.ToString() + "/" + Count.ToString() + " = " + d.ToString() + "% )";
        }
        #endregion
        
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
        public void DoAction(string actionType)
        {
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
            return Utility.GetResourceStr("EmployeeSurveyDistribute");
        }

        private void CancelAndClose()
        {

        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            throw new NotImplementedException();
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
