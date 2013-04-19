using System;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    /// 题目列表
    /// </summary>
    public partial class SurveyShowList_sat : BaseForm, IClient
    {
        public SurveyShowList_sat()
        {
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            txbSubId.Text = subjectViewInfo.SUBJECTID.ToString() + ".";
            txbSubContent.Text = subjectViewInfo.CONTENT;

            //构造一个答案组，并根据resultDetail加载以前选择的答案值
            foreach (T_SYS_DICTIONARY i in _oanswer)
            {
                SurveyAnswer_sat sa = new SurveyAnswer_sat(i,resultDetail);
                sa.event_AnswerSelected += new EventHandler<AnswerSelectedEventArgs>(event_AnswerSelected);
                sa.IsShowResult = isShowResult;
                //显示结果时用
                if (resultDetail != null)
                    if (resultDetail.RESULT == i.DICTIONARYVALUE.ToString())
                    {
                        // subAnswer = resultDetail.RESULT; 注释;event_AnswerSelected自动触发赋值
                        sa.IsSelected = true;
                    }
                spAnswerList.Children.Add(sa);
            }
        }
        //只能单选
        void event_AnswerSelected(object sender, AnswerSelectedEventArgs e)
        {
            foreach (SurveyAnswer_sat sa in spAnswerList.Children)
            {
                if (sa.txbAnswerContent.Text.ToString() == e.DICTIONARYNAME && e.DICTIONARYNAME!="")
                {
                    subAnswer = e.DICTIONARYVALUE;
                    sa.ckbSelect.IsChecked = true;
                }
                else
                    sa.ckbSelect.IsChecked = false;
            }
        }
        /// <summary>
        /// 题目当前选择的答案 value
        /// </summary>
        private string subAnswer = "";
        /// <summary>
        /// 获取选择结果
        /// </summary>
        private T_OA_SATISFACTIONRESULT resultDetail;
        public T_OA_SATISFACTIONRESULT ResultDetail
        {
            get
            {
                resultDetail = new T_OA_SATISFACTIONRESULT();
                resultDetail.SATISFACTIONRESULTID = System.Guid.NewGuid().ToString(); 
                resultDetail.RESULT = subAnswer;
                resultDetail.CONTENT = "无用";
                resultDetail.SUBJECTID = subjectViewInfo.SUBJECTID;               
                return resultDetail;
            }
            set {  resultDetail = value;}
        }

        private T_OA_SATISFACTIONDETAIL subjectViewInfo;
        public T_OA_SATISFACTIONDETAIL SubjectViewInfo {  get  {  return subjectViewInfo; } set { subjectViewInfo = value; } }
        private int isShowResult;
        public int IsShowResult { get { return isShowResult; } set { isShowResult = value; } }
        /// <summary>
        /// 模板答案
        /// </summary>
        ObservableCollection<T_SYS_DICTIONARY> _oanswer;
        public ObservableCollection<T_SYS_DICTIONARY> oanswer { get { return _oanswer; } set { _oanswer = value; } }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            //throw new NotImplementedException();
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
