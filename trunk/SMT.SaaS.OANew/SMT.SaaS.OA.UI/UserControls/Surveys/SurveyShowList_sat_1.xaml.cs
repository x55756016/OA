using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    ///显示结果-- 题目列表 
    /// </summary>
    public partial class SurveyShowList_sat_1 : BaseForm, IClient
    {
        ///// <summary>
        ///// 题目当前选择的答案 value
        ///// </summary>
        //private string subAnswer = "";      

        private T_OA_SATISFACTIONDETAIL subjectViewInfo;
        ObservableCollection<T_SYS_DICTIONARY> _oanswer;
        V_SatisfactionResult subAnswerResult;
        
        /// <summary>
        /// 满意度答案级别 最高级别 为 1
        /// </summary>
        public string AnswerID = "";
        /// <summary>
        /// 满意度答案级别人数 所占百分比达到多少 为通过
        /// </summary>
        public decimal PercentAge = 0;
        /// <summary>
        /// 题目
        /// </summary>
        public T_OA_SATISFACTIONDETAIL SubjectViewInfo { get { return subjectViewInfo; } set { subjectViewInfo = value; } }      
        /// <summary>
        /// 模板答案
        /// </summary>
        public ObservableCollection<T_SYS_DICTIONARY> oanswer { get { return _oanswer; } set { _oanswer = value; } }      
        /// <summary>
        /// 题目答案统计结果
        /// </summary>
        public V_SatisfactionResult SubAnswerResult { get { return subAnswerResult; } set { subAnswerResult = value; } }
        /// <summary>
        /// 题目统计结果，回传到方案 统计
        /// </summary>
        event EventHandler<SatisfactionResultEventArgs> event_SatisfactionResult;

        public SurveyShowList_sat_1(EventHandler<SatisfactionResultEventArgs> ev)
        {
            InitializeComponent();
            event_SatisfactionResult += ev;
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            txbSubId.Text = subjectViewInfo.SUBJECTID.ToString() + ".";
            txbSubContent.Text = subjectViewInfo.CONTENT;

            int count = 0; //该题目总票数 
            int AnswerID_count = 0;  // 达到答案级别的 票数

            //构造一个答案组，并统计通过率
            foreach (T_SYS_DICTIONARY i in _oanswer)
            {
                V_SSubjectAnswerResult v=null;
                if(subAnswerResult!=null)
                 v = subAnswerResult.LstAnswer.FirstOrDefault(ee => decimal.Parse(ee.AnswerCode) == i.DICTIONARYVALUE);
                if (v == null)
                    v = new V_SSubjectAnswerResult();
                else
                {
                    if (int.Parse(AnswerID) >= int.Parse(v.AnswerCode))
                        AnswerID_count = +v.Count;
                    count += v.Count;
                }
                SurveyAnswer_sat_1 sa = new SurveyAnswer_sat_1(i, v.Count);
                spAnswerList.Children.Add(sa);
            }

            //题目答案统计结果
            decimal d = decimal.Round(AnswerID_count / (count == 0 ? 1 : count) * 100, 1);
            if (d >= PercentAge)
                txtSubResult.Text = "结果：通过(" + d.ToString() + "%)";
            else
                txtSubResult.Text = "结果：不通过(" + d.ToString() + "%)";

            //回传到方案 统计
            SatisfactionResultEventArgs ev=new SatisfactionResultEventArgs();
            ev.Count=count;
            ev.AnswerID_count=AnswerID_count;
            if (event_SatisfactionResult != null)
                event_SatisfactionResult(this, ev);
        }

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
    public class SatisfactionResultEventArgs : EventArgs
    {
        /// <summary>
        /// 总票数 
        /// </summary>
       public int Count = 0; 
        /// <summary>
        /// 达到答案级别的 票数
        /// </summary>
       public int AnswerID_count = 0;  
    }
}
