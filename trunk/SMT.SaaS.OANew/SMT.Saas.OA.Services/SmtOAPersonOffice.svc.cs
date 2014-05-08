using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class SmtOAPersonOffice
    {
        [OperationContract]
        public void DoWork()
        {
            // 在此处添加操作实现
            
        //T_OA_APPROVALINFOTEMPLET a = new T_OA_APPROVALINFOTEMPLET();
       
            return;
        }
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
    }
}
