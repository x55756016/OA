using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.HRM.BLL;

namespace SMT.HRM.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class HRUpdateCheckStateServices
    {
        [OperationContract]
        public void DoWork()
        {
            // 在此处添加操作实现
            return;
        }
        #region 统一更新审核状态
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

            SMT.Foundation.Log.Tracer.Debug("______________HR UpdateCheckState开始： strEntityName：" + strEntityName
                    + " EntityKeyName:" + EntityKeyName + " EntityKeyValue：" + EntityKeyValue
                    + " CheckState:" + CheckState);

            int i=0;
            i=SMT.HRM.BLL.Utility.UpdateCheckState(strEntityName, EntityKeyName, EntityKeyValue, CheckState);

            SMT.Foundation.Log.Tracer.Debug("______________UpdateCheckState成功： strEntityName：" + strEntityName
                    + " EntityKeyName:" + EntityKeyName + " EntityKeyValue：" + EntityKeyValue
                    + " CheckState:" + CheckState + " 受影响的记录数：" + i.ToString());

            return i;
        }
        #endregion
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
    }
}
