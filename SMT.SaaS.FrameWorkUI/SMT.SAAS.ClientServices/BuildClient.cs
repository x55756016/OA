
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using PermissionWS=SMT.Saas.Tools.PermissionWS;
namespace SMT.SAAS.ClientServices
{
    public class BuildClient
    {
      
        private static BasicHttpBinding CreateBasicBinding()
        {
            return new BasicHttpBinding()
            {
                ReceiveTimeout = new System.TimeSpan(0, 30, 0),
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647
            };
        }

        private static CustomBinding CreateCustomBinding()
        {

            var elements = new List<BindingElement>();
            elements.Add(new BinaryMessageEncodingBindingElement());
            elements.Add(new HttpTransportBindingElement()
            {
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647
            });

            return new CustomBinding(elements)
            {
                ReceiveTimeout = new System.TimeSpan(0, 30, 0)

            };
        }
        private static PermissionWS.PermissionServiceClient CreatePermissionClient()
        {
            //return new PermissionWS.PermissionServiceClient(CreateCustomBinding(), CreateAddress(PERMISSION_SVC_NAME));
            return new PermissionWS.PermissionServiceClient();
        }

        /// <summary>
        /// 创建一个权限的WCF服务
        /// </summary>
        public static PermissionWS.PermissionServiceClient PermissionClient
        {
            get { return CreatePermissionClient(); }
        }

    }
}
