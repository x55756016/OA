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
using SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    ///员工调查---> 参与调查或 查看详情 用
    /// </summary>
    public partial class SurveyAnswer : BaseForm, IClient
    {
        private T_OA_REQUIREDETAIL answerInfo = null;
        public SurveyAnswer(T_OA_REQUIREDETAIL ansInfo)
        {
            InitializeComponent();
            answerInfo = ansInfo;
           // empSurveysManage.GetResultCountCompleted += new EventHandler<GetResultCountCompletedEventArgs>(empSurveysManage_GetResultCountCompleted);
        }

        //void empSurveysManage_GetResultCountCompleted(object sender, GetResultCountCompletedEventArgs e)
        //{
        //    txbResultCount.Visibility = Visibility.Visible;
        //    ckbSelect.Visibility = Visibility.Collapsed;
        //    txbResultCount.Text = "  (" + e.Result.ToString() + "票)";
        //}
       // private SmtOAPersonOfficeClient empSurveysManage = new SmtOAPersonOfficeClient();
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            txbAnswerId.Text = answerInfo.CODE;
            txbAnswerContent.Text = answerInfo.CONTENT;
            //if (isShowResult == 1)//查看票数
            //{
            //    empSurveysManage.GetResultCountAsync(answerInfo);
            //}
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
        public int IsShowResult
        {
            get
            {
                return isShowResult;
            }
            set { isShowResult = value; }
        }

        private string answerKey;
        public string AnswerKey
        {
            get
            {
                answerKey = answerInfo.CODE;
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
