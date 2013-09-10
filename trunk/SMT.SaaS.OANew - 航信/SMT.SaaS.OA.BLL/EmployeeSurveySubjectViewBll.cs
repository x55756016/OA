using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL;
using SMT.Foundation.Log;
namespace SMT.SaaS.OA.BLL
{
    public class EmployeeSurveySubjectViewBll : BaseBll<T_OA_REQUIREDETAIL>
    {
        public int AddEmployeeSurveySubjectView(V_EmployeeSurveySubject subjectInfo)
        {
            try
            {
                EmployeeSurveySubjectViewDal subjectViewDal = new EmployeeSurveySubjectViewDal();
                return subjectViewDal.AddSubjectView(subjectInfo) ? 1 : -1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveySubjectViewBll-AddEmployeeSurveySubjectView" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public int DeleteEmployeeSurveySubjectView(V_EmployeeSurveySubject subjectInfo)
        {
            try
            {
                EmployeeSurveySubjectBll subjectBll = new EmployeeSurveySubjectBll();
                EmployeeSurveysAnswerBll answerBll = new EmployeeSurveysAnswerBll();
                foreach (T_OA_REQUIREDETAIL anserInfo in subjectInfo.AnswerList)
                {
                    if (!answerBll.DeleteEmployeeSurveysAnswer(anserInfo.REQUIREDETAILID))
                    {
                        return -1;
                    }
                }
                if (!subjectBll.DeleteEmployeeSurveySubject(subjectInfo.SubjectInfo.REQUIREDETAIL2ID))
                {
                    return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveySubjectViewBll-DeleteEmployeeSurveySubjectView" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public int UpdateEmployeeSurveySubjectView(V_EmployeeSurveySubject subjectInfo)
        {
            try
            {
                EmployeeSurveySubjectBll subjectBll = new EmployeeSurveySubjectBll();
                EmployeeSurveysAnswerBll answerBll = new EmployeeSurveysAnswerBll();
                if (subjectBll.UpdateEmployeeSurveySubject(subjectInfo.SubjectInfo) == -1)
                {
                    return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveySubjectViewBll-UpdateEmployeeSurveySubjectView" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
    }
}