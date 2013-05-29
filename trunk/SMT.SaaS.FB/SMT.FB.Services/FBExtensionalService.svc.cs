/*
 * 文件名：FBExtensionalService.svc.cs
 * 作  用：预算引擎调用服务类
 * 创建人：吴鹏
 * 创建时间：2010年1月19日, 17:52:38
 * 修改人：
 * 修改时间：
 */

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SMT.Foundation.Log;
using Smt.Global.IContract;

namespace SMT.FB.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FBExtensionalService : IEventTriggerProcess
    {
        public void EventTriggerProcess(string param)
        {
            try
            {
                Tracer.Warn(string.Format("在{0} 执行预算自动月结", System.DateTime.Now.ToString("yyyy-MM-dd")));
                FBService service = new FBService();
                service.CloseBudget();
            }
            catch (Exception ex)
            {
                Tracer.Error("自动月结执行出错: " + ex.ToString());
                throw ex;
            }
        }
    }

}
