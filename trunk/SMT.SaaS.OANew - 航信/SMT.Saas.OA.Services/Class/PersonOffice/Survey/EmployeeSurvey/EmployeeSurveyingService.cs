using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        /// <summary>
        /// 新增调查结果,每一个题目对应一条记录
        /// </summary>
        [OperationContract]
        public V_EmployeeSurveying GetDataBySurveying(string requireID)
        {
            if (!string.IsNullOrEmpty(requireID))
            {
                using (EmployeeSurveyingBll getBll = new EmployeeSurveyingBll())
                {
                    return getBll.GetDataBySurveying(requireID);
                }
            }
            return null;
        }

        /// <summary>
        /// 根据申请ID查询方案题目、方案内容及问题集合
        /// </summary>
        [OperationContract]
        public bool AddSurveying(List<T_OA_REQUIRERESULT> resultList,string masterID,string requireID)
        {
            if (resultList != null && resultList.Count() > 0)
            {
                using (EmployeeSurveyingBll addBll = new EmployeeSurveyingBll())
                {
                    return addBll.AddSurveying(resultList,masterID,requireID);
                }
            }
            return false;
        }
    }
}