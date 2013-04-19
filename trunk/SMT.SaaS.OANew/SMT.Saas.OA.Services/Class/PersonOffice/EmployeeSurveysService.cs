using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using SMT.Foundation.Log;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
//using SMT.SaaS.OA.Services.FlowService;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.SaaS.OA.DAL;
using System.Collections.ObjectModel;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {

       EmployeeSurveysBLL bll = new EmployeeSurveysBLL();

       /// <summary>
       /// 查询员工调查方案
       /// </summary>
       /// <param name="pageIndex"></param>
       /// <param name="pageSize"></param>
       /// <param name="sort"></param>
       /// <param name="filterString"></param>
       /// <param name="paras"></param>
       /// <param name="pageCount"></param>
       /// <param name="userId"></param>
       /// <param name="checkState"></param>
       /// <returns></returns>
         [OperationContract]
        public List<T_OA_REQUIREMASTER> GetEmployeeSurveys(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, string checkState)
        {
            try
            {
                IQueryable<T_OA_REQUIREMASTER> GradeList = bll.GetEmployeeSurveys(pageIndex, pageSize, sort,
                                                                                      filterString, paras, ref pageCount,
                                                                                      userId, checkState);
                return GradeList != null ? GradeList.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Error(ex.Message);
                return null;
            }
        }

         /// <summary>
         /// 根据申请ID查询方案题目、方案内容及问题集合 
         /// </summary>
         /// <param name="requireID"></param>
         /// <returns></returns>
         [OperationContract]
        public V_EmployeeSurveysModel GetDataByRequireID(string requireID)
         {
             try
             {
                 return  bll.GetDataByRequireID(requireID);
             }
             catch (Exception ex)
             {
                 Tracer.Error(ex.Message);
                 return null;
             }
         }

         /// <summary>
         /// 删除员工调查方案
         /// </summary>
         /// <param name="requiremasterIDs"></param>
         /// <returns></returns>
        [OperationContract]
        public bool DeleteEmployeeSurveys(string[] requiremasterIDs)
         {
             try
             {
                 return bll.DeleteEmployeeSurveys(requiremasterIDs);
             }
             catch (Exception ex)
             {
                 Tracer.Error(ex.Message);
                 return false;
             }
         }


        /// <summary>
         /// 查询题目选项，答案表
         /// </summary>
         /// <param name="requiremasterID">方案ID</param>
         /// <param name="subjectID">题号</param>
         /// <returns></returns>
         [OperationContract]
        public List<V_REQUIRERESULTMODE> GetAnswer(string requiremasterID, decimal subjectID, string createuserID)
         {
             try
             {
                return bll.GetAnswer(requiremasterID,subjectID,createuserID);
             }
             catch (Exception ex)
             {
                 Tracer.Error(ex.Message);
                 return null;
             }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="result"></param>
         /// <returns></returns>
         [OperationContract]
         public bool EditRequireresult(List<T_OA_REQUIRERESULT> result, string[] IDs)
         {
             try
             {
                 // 添加调查结果
                 if (result != null && result.Count > 0)
                 {
                     return bll.AddRequireresult(result);
                 }
                 // 删除调查结果
                 else //if(IDs!=null&&IDs.Count()>0)
                 {
                     return bll.DeleteRequireresult(IDs);
                 }
             }
             catch (Exception ex)
             {
                 Tracer.Error(ex.Message);
                 return false;
             } 
         }

        /// <summary>
         /// 删除调查结果
         /// </summary>
         /// <param name="IDs"></param>
         /// <returns></returns>
         [OperationContract]
        public bool DeleteRequireresult(string[] IDs)
         {
             try
             {
                 return bll.DeleteRequireresult(IDs);
             }
             catch (Exception ex)
             {
                 Tracer.Error(ex.Message);
                 return false;
             }
         }

         /// <summary>
        ///  审核员工调查申请表 生成待办任务
        /// </summary>
        /// <param name="require"></param>
        /// <returns></returns>
         [OperationContract]
        public bool CheckRequire(T_OA_REQUIRE require)
         {
             try
             {
                 return bll.CheckRequire(require);
             }
             catch (Exception ex)
             {
                 Tracer.Error(ex.Message);
                 return false;
             }
         }


         /// <summary>
        /// 查询员工调查发布申请
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userId"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
         [OperationContract]
        public List<T_OA_REQUIREDISTRIBUTE> GetRequireDistribute(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, string checkState)
           {
               try
               {
                   IQueryable<T_OA_REQUIREDISTRIBUTE> GradeList = bll.GetRequireDistribute(pageIndex, pageSize, sort,
                                                                                         filterString, paras, ref pageCount,
                                                                                         userId, checkState);
                   return GradeList != null ? GradeList.ToList() : null;
               }
               catch (Exception ex)
               {
                   Tracer.Error(ex.Message);
                   return null;
               }

           }

         /// <summary>
        /// 修改调查方案
        /// </summary>
        /// <param name="master"></param>
        /// <returns></returns>
       [OperationContract]
        public bool UpdateEmployeeSurveys(V_EmployeeSurveyMaster master)
          {
              try
              {
                  return bll.UpdateEmployeeSurveys(master);
              }
              catch (Exception ex)
              {
                  Tracer.Error(ex.Message);
                  return false;
              }
          }



    }
}