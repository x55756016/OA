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

            SMT.Foundation.Log.Tracer.Debug("手机审单调用了" + strEntityName + "表单键值：" + EntityKeyName);
            SMT.Foundation.Log.Tracer.Debug("表单ID为：" + EntityKeyValue + System.DateTime.Now.ToString());
            SMT.Foundation.Log.Tracer.Debug("审核状态为：" + CheckState);
            return SMT.HRM.BLL.Utility.UpdateCheckState(strEntityName, EntityKeyName, EntityKeyValue, CheckState);
        }
        #endregion
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
    }
}
