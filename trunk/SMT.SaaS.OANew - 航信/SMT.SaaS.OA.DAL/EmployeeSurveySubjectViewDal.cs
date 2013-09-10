using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using System.Data.Objects;
using SMT.Foundation.Core;
using SMT.SaaS.OA.DAL.Views;
namespace SMT.SaaS.OA.DAL
{
    public class EmployeeSurveySubjectViewDal : CommDaL<T_OA_REQUIREMASTER>
    {
        public EmployeeSurveySubjectViewDal()
        {
            
        }
        EmployeeSurveysAnswerDal esaDal = new EmployeeSurveysAnswerDal();
        public bool AddSubjectView(V_EmployeeSurveySubject requireSubjectView)
        {
            try
            {
                if (requireSubjectView.SubjectInfo.T_OA_REQUIREMASTER.EntityKey == null)
                    requireSubjectView.SubjectInfo.T_OA_REQUIREMASTER.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_REQUIREMASTER", "REQUIREMASTERID",  requireSubjectView.SubjectInfo.T_OA_REQUIREMASTER.REQUIREMASTERID);
                requireSubjectView.SubjectInfo.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(requireSubjectView.SubjectInfo.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                //objModelContext.AddObject("T_OA_REQUIREDETAIL2", requireSubjectView.SubjectInfo);
                base.Add(requireSubjectView.SubjectInfo);
                int i = SaveContextChanges();
                if (i > 0)
                {
                    foreach (T_OA_REQUIREDETAIL anserInfo in requireSubjectView.AnswerList)
                    {
                        if (!esaDal.AddAnswer(anserInfo))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public bool UpdateSubjectView(V_EmployeeSurveySubject requireSubjectView)
        {
            try
            {
                EmployeeSurveysAnswerDal esaDal = new EmployeeSurveysAnswerDal();
                if (requireSubjectView.SubjectInfo.T_OA_REQUIREMASTER.EntityKey == null)
                    requireSubjectView.SubjectInfo.T_OA_REQUIREMASTER.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_REQUIREMASTER", "REQUIREMASTERID", requireSubjectView.SubjectInfo.T_OA_REQUIREMASTER.REQUIREMASTERID);
                T_OA_REQUIREDETAIL2 tmpobj = base.GetObjectByEntityKey(requireSubjectView.SubjectInfo.EntityKey) as T_OA_REQUIREDETAIL2;
                base.Update(requireSubjectView.SubjectInfo);
                int i = SaveContextChanges();
                if (i < 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}