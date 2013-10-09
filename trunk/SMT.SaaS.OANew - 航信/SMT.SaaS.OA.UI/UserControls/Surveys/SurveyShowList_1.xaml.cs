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
    ///员工调查--显示结果, 构造每个题目
    /// </summary>
    public partial class SurveyShowList_1 : BaseForm, IClient
    {
        /// <summary>
        /// 题目答案统计结果
        /// </summary>
        public V_SatisfactionResult _SubAnswerResult { get; set; }
      
        private V_EmployeeSurveySubject subjectViewInfo;
        /// <summary>
        ///   方案中的题目
        /// </summary>
        public V_EmployeeSurveySubject SubjectViewInfo {  get {  return subjectViewInfo;  } set { subjectViewInfo = value; }}

        public SurveyShowList_1()
        {
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            txbSubId.Text = subjectViewInfo.SubjectInfo.SUBJECTID.ToString() + ".";
            txbSubContent.Text = subjectViewInfo.SubjectInfo.CONTENT;

            System.Linq.IOrderedEnumerable<T_OA_REQUIREDETAIL> oanswerList = subjectViewInfo.AnswerList.OrderBy(ee => ee.CODE);
            foreach (T_OA_REQUIREDETAIL i in oanswerList)
            {
                if (i.CONTENT != "无")
                {
                    //根据答案 获取每个答案的统计结果
                    V_SSubjectAnswerResult v = null;
                    if (_SubAnswerResult != null)
                        v = _SubAnswerResult.LstAnswer.FirstOrDefault(ee => ee.AnswerCode == i.CODE);
                    if (v == null)
                        v = new V_SSubjectAnswerResult();                             

                    SurveyAnswer_1 sa = new SurveyAnswer_1(i.CODE,i.CONTENT,v.Count);
                    spAnswerList.Children.Add(sa);
                }
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
