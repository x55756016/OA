using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.SaaS.OA.DAL;
using System.Collections.ObjectModel;
using SMT_OA_EFModel;


namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        [OperationContract]
        public IQueryable<T_OA_REQUIREMASTER> GetInfroByTitle(string title)
        {
            using (EmployeeSurveysMasterBll masterBll = new EmployeeSurveysMasterBll())
            {
                return masterBll.GetInfoListByTitle(title);

            }
        }
        [OperationContract]
        public bool AddEmployeeSurveyMaster(T_OA_REQUIREMASTER addId)
        {
            using (EmployeeSurveysMasterBll masterBll = new EmployeeSurveysMasterBll())
            {
                return masterBll.Add(addId);

            }
        }
        [OperationContract]
        public int AddEmployeeSurveyAnswer(T_OA_REQUIREDETAIL addId)
        {
            using (EmployeeSurveysAnswerBll answerBll = new EmployeeSurveysAnswerBll())
            {
                return answerBll.AddAnswer(addId);
 
            }

        }
        [OperationContract]
        public IQueryable<T_OA_REQUIREDETAIL> GetEmployeeAnswer(string senId, decimal key)
        {
            using (EmployeeSurveysAnswerBll answerBll = new EmployeeSurveysAnswerBll())
            {
                return answerBll.GetInfoListByMasterId(senId, key);
            }
        }
        [OperationContract]
        public bool AddEmployeeSurveyQuestion(T_OA_REQUIREDETAIL2 addId)
        {

            using (EmployeeSurveySubjectBll questionBll = new EmployeeSurveySubjectBll())
            {
                return questionBll.AddEmployeeSurveySubject(addId);

            }
        }
        


}
}