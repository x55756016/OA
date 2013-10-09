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
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.UserControls
{

    public partial class SurveyAnswer_sat : BaseForm, IClient
    {
        
        private T_OA_SATISFACTIONRESULT resultDetail;
        private T_SYS_DICTIONARY answerInfo = null;
        public EventHandler<AnswerSelectedEventArgs> event_AnswerSelected;
        public SurveyAnswer_sat(T_SYS_DICTIONARY ansInfo, T_OA_SATISFACTIONRESULT res)
        {
            InitializeComponent();
            answerInfo = ansInfo;
            resultDetail = res;
            empSurveysManage.Result_SurveySubIDCompleted += new EventHandler<Result_SurveySubIDCompletedEventArgs>(Result_SurveySubIDCompleted);
        }

        void Result_SurveySubIDCompleted(object sender, Result_SurveySubIDCompletedEventArgs e)
        {
            txbResultCount.Visibility = Visibility.Visible;
            ckbSelect.Visibility = Visibility.Collapsed;
            txbResultCount.Text = "  (" + e.Result.ToString() + "票)";
        }
        private SmtOAPersonOfficeClient empSurveysManage = new SmtOAPersonOfficeClient();
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            txbAnswerContent.Text = answerInfo.DICTIONARYNAME;
            if (isShowResult == 1)//查看票数
            {
                if(resultDetail!=null)
                empSurveysManage.Result_SurveySubIDAsync(resultDetail.T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID,resultDetail.SUBJECTID.ToString(),resultDetail.RESULT);
            }
            if (isShowResult == 0)//员工提交答案
            {
                ckbSelect.Visibility = Visibility.Visible;
            }
            if (isShowResult == 2)// 审批页面
            {
                ckbSelect.Visibility = Visibility.Collapsed;
            }
        }
        private int isShowResult;
        public int IsShowResult  {  get  { return isShowResult; }  set { isShowResult = value; } }

        private string answerKey;
        /// <summary>
        /// 答案的id,即T_SYS_DICTIONARY.DICTIONARYVALUE
        /// </summary>
        public string AnswerKey
        {
            get
            {
                answerKey = answerInfo.DICTIONARYVALUE.ToString();
                return answerKey;
            }
            set { answerKey = value; }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                isSelected = Convert.ToBoolean(ckbSelect.IsChecked);
                return isSelected;
            }
            set
            {
                isSelected = value;
                ckbSelect.IsChecked = isSelected;
            }
        }

        private void ckbSelect_Checked(object sender, RoutedEventArgs e)
        {
            bool b = (bool)((RadioButton)sender).IsChecked;
            if (b)//<--可能无用
                if (event_AnswerSelected != null)
                {
                    AnswerSelectedEventArgs ev = new AnswerSelectedEventArgs();
                    ev.DICTIONARYVALUE = answerInfo.DICTIONARYVALUE.ToString();
                    ev.DICTIONARYNAME = this.txbAnswerContent.Text;
                    event_AnswerSelected(this, ev);
                }
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
    /// <summary>
    /// 答案事件参数
    /// </summary>
    public class AnswerSelectedEventArgs : EventArgs
    {
       public string DICTIONARYNAME;
       public string DICTIONARYVALUE;
    }
}
