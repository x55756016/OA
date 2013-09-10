using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    /// <summary>
    /// 员工调查申请自定义类
    /// </summary>         
   public class V_EmployeeSurveyApp
    {
       public string SurveyTitle { get; set; }
       public string RequireContent { get; set; }
       public string AnswerGroupId { get; set; }
       public string DistributeuserId { get; set; }
       public string RequireId { get; set; }
       public string CheckState { get; set; }
       public string MasterId { get; set; }
       public string RequireresultId { get; set; }
       public string OwnerName { get; set; }
       public DateTime? CreateDate{ get; set; }
   
       public T_OA_REQUIRE requireEntity;
       public T_OA_REQUIREMASTER masterEntity;
       public IEnumerable<T_OA_REQUIRERESULT> ResultList;//参与调查后所保存的结果集合
       public IEnumerable<T_OA_DISTRIBUTEUSER> distributeuserList;//新增的分发集合
       public IEnumerable<T_OA_DISTRIBUTEUSER> oldDistributeuserList;//删除的分发集合
    }
}
