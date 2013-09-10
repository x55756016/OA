using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.SaaS.OA.BLL;
using SMT_OA_EFModel;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class OAUpdateCheckServices
    {
        /// <summary>
        /// 修改实体审核状态
        /// </summary>
        /// <param name="strEntityName">实体名</param>
        /// <param name="EntityKeyName">主键名</param>
        /// <param name="EntityKeyValue">主键值</param>
        /// <param name="CheckState">审核状态</param>
        [OperationContract]
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            // 在此处添加操作实现
            //Type  bb = strEntityName
            Tracer.Debug("OAUpdateCheckServices服务调用了UpdateCheckState，调用了：" + strEntityName +"单据号：" + EntityKeyValue +System.DateTime.Now.ToString() + "\n");
            CommBll<T_OA_AGENTSET> Combll = new CommBll<T_OA_AGENTSET>();
            return Combll.UpdateCheckState(strEntityName, EntityKeyName, EntityKeyValue, CheckState);
            
        }

        // 在此处添加更多操作并使用 [OperationContract] 标记它们
    }
}
