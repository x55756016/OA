using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;
namespace SMT.SaaS.OA.DAL
{
    public class EmployeeSurveySubjectDal : BaseDAL
    {
        public EmployeeSurveySubjectDal()
        {
            
        }

        public bool AddSurveySubject(T_OA_REQUIREDETAIL2 surveySubjectInfo)
        {
            try
            {
                surveySubjectInfo.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                base.Add(surveySubjectInfo);
                int i = SaveContextChanges();
                if (i > 0)
                {
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

        public bool UpdateSurveySubject(T_OA_REQUIREDETAIL2 surveySubjectInfo)
        {
            try
            {
                if (surveySubjectInfo.EntityKey == null)
                {
                    surveySubjectInfo.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                    base.Add(surveySubjectInfo);
                }
                else
                {
                    T_OA_REQUIREDETAIL2 tmpobj = base.GetObjectByEntityKey(surveySubjectInfo.EntityKey) as T_OA_REQUIREDETAIL2;
                    if (tmpobj.T_OA_REQUIREMASTER == null)
                    {
                        tmpobj.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(((System.Data.Objects.DataClasses.EntityReference)(surveySubjectInfo.T_OA_REQUIREMASTERReference)).EntityKey) as T_OA_REQUIREMASTER;
                    }
                    else
                    {
                        tmpobj.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                    }
                    base.Update(surveySubjectInfo);
                }
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
