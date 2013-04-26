using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace SMT.SAAS.Platform.Services
{
    // 注意: 如果更改此处的类名 "PlatformServices"，也必须更新 Web.config 中对 "PlatformServices" 的引用。
 
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class PlatformServices : IPlatformServices
    {
       
    }
}
