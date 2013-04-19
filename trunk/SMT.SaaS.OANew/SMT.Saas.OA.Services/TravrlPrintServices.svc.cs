using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.SaaS.OA.BLL;
using SMT_OA_EFModel;
using SMT.Foundation.Log;
using System.Collections.Generic;

namespace SMT.SaaS.OA.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class TravrlPrintServices
    {
        #region 查询报销明细
        [OperationContract]
        public List<T_OA_REIMBURSEMENTDETAIL> GetTravelReimbursementDetail(string detailId)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                List<T_OA_REIMBURSEMENTDETAIL> details = TrBll.GetTravelReimbursementDetail(detailId);
                return details;
            }
        }
        #endregion
    }
}
