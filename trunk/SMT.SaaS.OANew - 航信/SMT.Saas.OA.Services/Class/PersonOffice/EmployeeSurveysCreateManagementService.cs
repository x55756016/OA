using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.SaaS.OA.DAL.Views;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        #region Master 主表
        [OperationContract]
        public List<T_OA_REQUIREMASTER> GetInfoListByOptFlag(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState)
        {
            using (EmployeeSurveysMasterBll empSurveysCreateBll = new EmployeeSurveysMasterBll())
            {
                IQueryable<T_OA_REQUIREMASTER> calendarList = empSurveysCreateBll.GetInfoListByOptFlag(checkState);
                if (calendarList == null)
                {
                    return null;
                }
                else
                {
                    return calendarList.ToList();
                }
            }
        }

        [OperationContract]
        public T_OA_REQUIREMASTER GetInfoById(string masterID)
        {
            using (EmployeeSurveysMasterBll empSurveysCreateBll = new EmployeeSurveysMasterBll())
            {
                IQueryable<T_OA_REQUIREMASTER> infoList = empSurveysCreateBll.GetInfoListById(masterID);
                if (infoList == null)
                {
                    return null;
                }
                else
                {
                    return infoList.ToList()[0];
                }
            }
        }
        [OperationContract]
        public V_EmployeeSurveySubject GetInfoByTitle(string masterTitle)
        {
            V_EmployeeSurveySubject listAll = new V_EmployeeSurveySubject();
              string masterId=listAll.Master.REQUIREMASTERID;
            using (EmployeeSurveysMasterBll masterBll = new EmployeeSurveysMasterBll())
            {
                IQueryable<T_OA_REQUIREMASTER> masterList = masterBll.GetInfoListByTitle(masterTitle);
                if (masterList == null)
                {
                    return null;
                }
                else
                {
                   listAll.Master=masterList.ToList()[0];
                  
                }
            }
            using (EmployeeSurveySubjectBll questionBll = new EmployeeSurveySubjectBll())
            {
                IQueryable<T_OA_REQUIREDETAIL2> questionList = questionBll.GetInfoListByMasterId(masterId);
                if (questionList == null)
                {
                    return null;
                }
                else
                {
                    listAll.Question = questionList.ToList()[0];
                }
            }
                 using(EmployeeSurveysAnswerBll answerBll=new EmployeeSurveysAnswerBll())
              {
               IQueryable<T_OA_REQUIREDETAIL> answerList =answerBll.GetInfoByMasterId(masterId);
                if(answerList==null)
                {
                  return null;
                }
                else
                {
                   listAll.Answer=answerList.ToList()[0];
                }
            }
            return listAll;
 
            }
        
        [OperationContract]
        public int AddEmpSurveys(T_OA_REQUIREMASTER obj)
        {
            using (EmployeeSurveysMasterBll empSurveysCreateBll = new EmployeeSurveysMasterBll())
            {
                bool sucess = empSurveysCreateBll.Add(obj);
                if (sucess == false)
                {
                    return -1;
                }
                return 1;
            }
        }
        [OperationContract]
        public bool AddEmployeeSurvey(T_OA_REQUIREMASTER key)
        {
            using (EmployeeSurveysMasterBll masterBll = new EmployeeSurveysMasterBll())
            {
                bool sucess = masterBll.Add(key);
                if (sucess == false)
                {
                    return false;
                }
                return true;
 
            }
        }

        [OperationContract]
        public bool DeleteEmpSurveys(List<T_OA_REQUIREMASTER> objList)
        {
            using (EmployeeSurveysMasterBll empSurveysCreateBll = new EmployeeSurveysMasterBll())
            {
                using (EmployeeSurveysAnswerBll empSurveysAnswerBll = new EmployeeSurveysAnswerBll())
                {
                    using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
                    {
                        empSurveysCreateBll.BeginTransaction();
                        try
                        {
                            bool isSccess = true;
                            foreach (T_OA_REQUIREMASTER obj in objList)
                            {
                                if (empSurveysCreateBll.Delete(obj.REQUIREMASTERID))
                                { //删除题目和答案
                                    IQueryable<T_OA_REQUIREDETAIL2> objDetail2List = empSurveysSubjectBll.GetInfoListByMasterId(obj.REQUIREMASTERID);
                                    if (objDetail2List != null)
                                    {
                                        foreach (T_OA_REQUIREDETAIL2 objDetail2 in objDetail2List)
                                        {
                                            IQueryable<T_OA_REQUIREDETAIL> detailList = empSurveysAnswerBll.GetInfoListByMasterId(objDetail2.REQUIREMASTERID, objDetail2.SUBJECTID);
                                            if (detailList != null)
                                            {
                                                foreach (T_OA_REQUIREDETAIL objDetail in detailList)
                                                {
                                                    if (empSurveysAnswerBll.DeleteEmployeeSurveysAnswer(objDetail.REQUIREDETAILID))
                                                    {

                                                    }
                                                    else
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (isSccess)
                            {
                                empSurveysCreateBll.CommitTransaction();
                                return true;
                            }
                            else
                            {
                                empSurveysCreateBll.RollbackTransaction();
                                return false;
                            }
                        }
                        catch
                        {
                            empSurveysCreateBll.RollbackTransaction();
                            return false;
                        }
                    }
                }
            }
        }

        [OperationContract]
        public int UpdateEmpSurveys(T_OA_REQUIREMASTER obj)
        {
            using (EmployeeSurveysMasterBll empSurveysCreateBll = new EmployeeSurveysMasterBll())
            {
                return empSurveysCreateBll.UpdateMaster(obj);
            }
        }
        #endregion

        #region Detail2 题目
        [OperationContract]
        public List<T_OA_REQUIREDETAIL2> GetEmpSurveysSubjectList(string requireId)
        {
            using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
            {
                IQueryable<T_OA_REQUIREDETAIL2> detailList = empSurveysSubjectBll.GetInfoListByMasterId(requireId);
                if (detailList != null)
                {
                    return detailList.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        [OperationContract]
        public int DelEmpSurveysSubject(T_OA_REQUIREDETAIL2 obj)
        {
            using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
            {
                if (!empSurveysSubjectBll.DeleteEmployeeSurveySubject(obj.REQUIREDETAIL2ID))
                {
                    return -1;
                }
                return 1;
            }
        }

        [OperationContract]
        public int AddEmpSurveysSubject(T_OA_REQUIREDETAIL2 obj)
        {
            using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
            {
                int n = empSurveysSubjectBll.Add(obj) ? 1 : 0;
                return n;
            }
        }

        [OperationContract]
        public int AddEmpSurveysSubjectList(List<T_OA_REQUIREDETAIL2> objList)
        {
            using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
            {
                //事物
                int n = 0;
                foreach (T_OA_REQUIREDETAIL2 obj in objList)
                    n += empSurveysSubjectBll.Add(obj) ? 1 : 0;
                return n;
            }
        }
        //保存 题目，答案
        [OperationContract]
        public int Save_SubjectAnswer(List<V_EmployeeSurveySubject> lstAdd,List<V_EmployeeSurveySubject> lstUpd)
        {
            using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
            {
                //事物
                int n = 0;
                foreach (V_EmployeeSurveySubject i in lstAdd)
                    n += empSurveysSubjectBll.Add(i.SubjectInfo) ? 1 : 0;
                foreach (V_EmployeeSurveySubject i in lstAdd)
                    n += empSurveysSubjectBll.UpdateEmployeeSurveySubject(i.SubjectInfo);
                return n;
            }
        }

        [OperationContract]
        public int DelEmpSurveysSubjectList(List<T_OA_REQUIREDETAIL2> objList)
        {
            using (EmployeeSurveysAnswerBll empSurveysAnswerBll = new EmployeeSurveysAnswerBll())
            {
                using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
                {
                    //事务
                    bool sucess = true;
                    foreach (T_OA_REQUIREDETAIL2 obj in objList)
                    {
                        if (empSurveysSubjectBll.DeleteEmployeeSurveySubject(obj.REQUIREDETAIL2ID))
                        {
                            IQueryable<T_OA_REQUIREDETAIL> detailList = empSurveysAnswerBll.GetInfoListByMasterId(obj.REQUIREMASTERID, obj.SUBJECTID);
                            if (detailList != null)
                            {
                                foreach (T_OA_REQUIREDETAIL objDetail in detailList)
                                {
                                    if (empSurveysAnswerBll.DeleteEmployeeSurveysAnswer(objDetail.REQUIREDETAILID))
                                    {

                                    }
                                    else
                                    {
                                        sucess = false;
                                        break;
                                    }
                                }
                            }
                            if (sucess == false)
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    return 1;
                }
            }
        }

        [OperationContract]
        public int UpdateEmpSurveysSubject(T_OA_REQUIREDETAIL2 objSubject)
        {
            using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
            {
                //事物
                if (empSurveysSubjectBll.UpdateEmployeeSurveySubject(objSubject) != 1)
                {
                    return -1;
                }
                return 1;
            }
        }

        [OperationContract]
        public int UpdateEmpSurveysSubjectList(List<T_OA_REQUIREDETAIL2> objList)
        {
            using (EmployeeSurveySubjectBll empSurveysSubjectBll = new EmployeeSurveySubjectBll())
            {
                //事物
                int nRet = 0;
                foreach (T_OA_REQUIREDETAIL2 obj in objList)
                {
                    nRet = empSurveysSubjectBll.UpdateEmployeeSurveySubject(obj);
                }
                if (nRet == -1)//没有修改也成功
                {
                    return -1;
                }
                return 1;
            }
        }
        #endregion

        #region Detail 答案
        [OperationContract]
        public int AddEmpSurveysAnswer(T_OA_REQUIREDETAIL obj)
        {
            using (EmployeeSurveysAnswerBll empSurveysAnswerBll = new EmployeeSurveysAnswerBll())
            {
                return empSurveysAnswerBll.Add(obj) ? 1 : 0;
            }
        }

        [OperationContract]
        public List<T_OA_REQUIREDETAIL> GetEmpSurveysAnswerList(string requireId, decimal subjectId)
        {
            using (EmployeeSurveysAnswerBll empSurveysAnswerBll = new EmployeeSurveysAnswerBll())
            {
                IQueryable<T_OA_REQUIREDETAIL> detailList = empSurveysAnswerBll.GetInfoListByMasterId(requireId, subjectId);
                if (detailList != null)
                {
                    return detailList.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        [OperationContract]
        public int AddEmpSurveysAnswerList(List<T_OA_REQUIREDETAIL> objList)
        {
            using (EmployeeSurveysAnswerBll empSurveysAnswerBll = new EmployeeSurveysAnswerBll())
            {
                //事物
                int n = 0;
                foreach (T_OA_REQUIREDETAIL obj in objList)
                {
                    n += empSurveysAnswerBll.Add(obj) ? 1 : 0;
                }
                return n;
            }
        }

        [OperationContract]
        public int DelEmpSurveysAnswerList(List<T_OA_REQUIREDETAIL> objList)
        {
            using (EmployeeSurveysAnswerBll empSurveysAnswerBll = new EmployeeSurveysAnswerBll())
            {
                //事物
                bool sucess = true;
                foreach (T_OA_REQUIREDETAIL obj in objList)
                {
                    sucess = empSurveysAnswerBll.DeleteEmployeeSurveysAnswer(obj.REQUIREDETAILID);
                }
                if (sucess == false)
                {
                    return -1;
                }
                return 1;
            }
        }

        [OperationContract]
        public int UpdateEmpSurveysAnswerList(List<T_OA_REQUIREDETAIL> objList)
        {
            using (EmployeeSurveysAnswerBll empSurveysAnswerBll = new EmployeeSurveysAnswerBll())
            {
                //事物
                int nRet = 0;
                foreach (T_OA_REQUIREDETAIL obj in objList)
                {
                    nRet = empSurveysAnswerBll.Update(obj);
                }
                if (nRet == -1)//没有修改也成功
                {
                    return -1;
                }
                return 1;
            }
        }
        #endregion

        #region 结果

        [OperationContract]
        public int AddEmpSurveysResultList(List<T_OA_REQUIRERESULT> objList)
        {
            using (EmployeeSurveysResultBll empSurveysResultBll = new EmployeeSurveysResultBll())
            {
                //事物
                bool sucess = true;
                foreach (T_OA_REQUIRERESULT obj in objList)
                {
                    sucess = empSurveysResultBll.Add(obj);
                }
                if (sucess == false)
                {
                    return -1;
                }
                return 1;
            }
        }
        #endregion
    }
}
