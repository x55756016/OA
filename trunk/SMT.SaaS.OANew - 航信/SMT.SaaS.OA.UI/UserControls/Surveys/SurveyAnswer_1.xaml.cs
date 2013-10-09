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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    /// 员工调查--显示结果, 构造一个题目的 每个答案
    /// </summary>
    public partial class SurveyAnswer_1 : BaseForm, IClient
    {
        public SurveyAnswer_1(string code,string content,int rltCount)
        {
            InitializeComponent();
            txbAnswerId.Text = code+".";
            txbAnswerContent.Text = content;
            txbResultCount.Text = "  (" + rltCount.ToString() + "票)";
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
