using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using SMT.Foundation.Log;
using System.Data.Objects;

namespace SMT.SaaS.OA.BLL
{
   public class EmployeeSurveyingBll:BaseBll<T_OA_REQUIRERESULT>
   {
       #region 新增
       /// <summary>
       /// 新增调查结果,每一个题目对应一条记录
       /// </summary>
       public bool AddSurveying(List<T_OA_REQUIRERESULT> resultList,string masterID,string requireID)
       {
           try
           {
               base.BeginTransaction();
               foreach (var data in resultList)
               {
               data.T_OA_REQUIREMASTERReference.EntityKey = Utility.AddEntityKey("T_OA_REQUIREMASTER", "REQUIREMASTERID", masterID);
               data.T_OA_REQUIREReference.EntityKey = Utility.AddEntityKey("T_OA_REQUIRE", "REQUIREID", requireID);
               dal.AddToContext(data);
               }
               int AddFlag = dal.SaveContextChanges();
               if (AddFlag > 0)
               {
                   base.CommitTransaction();
                   return true;
               }
               base.RollbackTransaction();
               return false;
           }
           catch (Exception ex)
           {
               base.RollbackTransaction();
               Tracer.Debug("员工参与调查EmployeeSurveyingBll-AddSurveying" + System.DateTime.Now.ToString() + " " + ex.ToString());
               return false;
           }
       }

       #endregion

       #region 查询
       /// <summary>
       /// 根据申请ID查询方案题目、方案内容及问题集合
       /// </summary>
       public V_EmployeeSurveying GetDataBySurveying(string requireID)
       {
           try
           {             
               var data = from ents in dal.GetObjects<T_OA_REQUIRE>()
                          .Include("T_OA_REQUIREMASTER")
                          join subject in
                              (from sub in dal.GetObjects<T_OA_REQUIREDETAIL2>()
                               select new V_EmployeeSurveyInformation { Subject = sub })
                           on ents.T_OA_REQUIREMASTER.REQUIREMASTERID equals subject.Subject.REQUIREMASTERID into list
                           where ents.REQUIREID==requireID
                          select new V_EmployeeSurveying
                          {
                              SubjectList=list,
                              MasterID=ents.T_OA_REQUIREMASTER.REQUIREMASTERID,
                              RequireID=ents.REQUIREID,
                              Title=ents.APPTITLE,
                              Content=ents.CONTENT
                          };
              return data != null?data.FirstOrDefault():null;
           }
           catch (Exception ex)
           {
             Tracer.Debug("员工参与调查EmployeeSurveyingBll-GetDataBySurveying" + System.DateTime.Now.ToString() + " " + ex.ToString());
             return null;
           }
       }

       #endregion
   }
}
