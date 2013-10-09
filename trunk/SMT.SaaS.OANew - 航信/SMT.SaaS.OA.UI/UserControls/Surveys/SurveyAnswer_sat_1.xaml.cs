using System.Windows;
using System.Windows.Controls;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.UserControls
{

    public partial class SurveyAnswer_sat_1 : BaseForm, IClient
    {
        
        private T_SYS_DICTIONARY answerInfo = null;
        public SurveyAnswer_sat_1(T_SYS_DICTIONARY ansInfo, int rlt)
        {
            InitializeComponent();
            answerInfo = ansInfo;
            txbAnswerContent.Text = answerInfo.DICTIONARYNAME;
            txbResultCount.Text = "  (" + rlt.ToString() + "票)";     
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
           
        }    
        private string answerKey;
        /// <summary>
        /// 答案的id,即T_SYS_DICTIONARY.DICTIONARYVALUE,暂时没有用
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

        #region IForm 成员

        public void ClosedWCFClient()
        {
            //throw new System.NotImplementedException();
        }

        public bool CheckDataContenxChange()
        {
            throw new System.NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    } 
}
