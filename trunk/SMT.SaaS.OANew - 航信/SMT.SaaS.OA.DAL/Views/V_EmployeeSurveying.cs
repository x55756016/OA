using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
   public class V_EmployeeSurveying
    {
        public string  MasterID { get; set; }//方案ID
        public string RequireID { get; set; }//申请ID
        public IEnumerable<V_EmployeeSurveyInformation> SubjectList { get; set; }//题目及答案集合
        public IEnumerable<T_OA_REQUIRERESULT> ResultList { get; set; }//参与调查集合
        public string Title { get; set; }//调查标题
        public string Content { get; set; }//调查内容

    }
}
 