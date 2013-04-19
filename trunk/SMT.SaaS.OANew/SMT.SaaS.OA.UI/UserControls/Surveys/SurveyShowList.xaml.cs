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
namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    /// 每个题目
    /// </summary>
    public partial class SurveyShowList : BaseForm, IClient
    {

        private V_EmployeeSurveySubject subjectViewInfo;
        public V_EmployeeSurveySubject SubjectViewInfo { get { return subjectViewInfo; } set { subjectViewInfo = value; } }

        private int isShowResult;
        public int IsShowResult { get { return isShowResult; } set { isShowResult = value; } }
        public SurveyShowList()
        {
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            txbSubId.Text = subjectViewInfo.SubjectInfo.SUBJECTID.ToString() + ".";
            txbSubContent.Text = subjectViewInfo.SubjectInfo.CONTENT;
            System.Linq.IOrderedEnumerable<T_OA_REQUIREDETAIL> oanswerList = subjectViewInfo.AnswerList.OrderBy(ee => ee.CODE);
            foreach (T_OA_REQUIREDETAIL anserInfo in oanswerList)
            {
                if (anserInfo.CONTENT != "无")
                {
                    SurveyAnswer sa = new SurveyAnswer(anserInfo);
                    sa.IsShowResult = isShowResult;
                    if (isShowResult != 0 && isShowResult != 3)
                        txtComments.Visibility = Visibility.Collapsed;
                    if (resultDetail != null)
                        if (resultDetail.RESULT == anserInfo.CODE)
                        {
                            sa.IsSelected = true;
                            if (resultDetail.CONTENT != null)
                                txtComments.Text = resultDetail.CONTENT;
                        }
                    spAnswerList.Children.Add(sa);
                }

            }
            ImageButton btn = new ImageButton();
            btn.Margin = new Thickness(0);
            btn.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4406in.png", Utility.GetResourceStr("OTHERSUGGEST"));
            btn.Click += new RoutedEventHandler(chk_Other);
            spAnswerList.Children.Add(btn);
        }
        void chk_Other(object sender, EventArgs e)
        {
            txtComments.Visibility = txtComments.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        private T_OA_REQUIRERESULT resultDetail;
        public T_OA_REQUIRERESULT ResultDetail
        {
            get
            {
                resultDetail = new T_OA_REQUIRERESULT();
                for (int i = 0; i < spAnswerList.Children.Count; i++)
                {
                    SurveyAnswer sa = spAnswerList.Children[i] as SurveyAnswer;
                    if (sa != null)
                    {
                        if (sa.IsSelected)
                        {
                            resultDetail.RESULT = sa.AnswerKey;
                            resultDetail.CONTENT = txtComments.Text;
                            //resultDetail.T_OA_REQUIREMASTER.REQUIREID = subjectViewInfo.SubjectInfo.REQUIREID;
                           // resultDetail.SUBJECTID = subjectViewInfo.SubjectInfo.SUBJECTID;
                            break;
                        }
                    }
                }
                if (resultDetail.RESULT == null)
                {
                    resultDetail.RESULT = "";
                }
                return resultDetail;
            }
            set
            {
                resultDetail = value;
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
}
