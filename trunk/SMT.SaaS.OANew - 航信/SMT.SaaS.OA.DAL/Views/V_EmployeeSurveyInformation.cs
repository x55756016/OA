using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
   public class V_EmployeeSurveyInformation
    {
      public T_OA_REQUIREDETAIL2 Subject { get; set; }
      public IEnumerable<T_OA_REQUIREDETAIL>  AnswerList{ get; set; }
    }
}
