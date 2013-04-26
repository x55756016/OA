using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
 
using SMT.SAAS.Platform.WebParts.Models;
using SMT.SAAS.Platform.WebParts.NewsWS;
 
namespace SMT.SAAS.Platform.WebParts.ViewModels
{
    public class RollNewsViewModel
    {
        PlatformServicesClient client = null;
        //基础服务通讯
        BasicServices services = null;

        public RollNewsViewModel()
        {
            if (services == null)
                services = new BasicServices();
            client = services.PlatformClient;

            if (client != null)
            {
                client.GetNewsListByParamsCompleted += new EventHandler<GetNewsListByParamsCompletedEventArgs>(client_GetNewsListByParamsCompleted);
                client.GetNewsListByParamsAsync("0|1", 10, "1");
            }
        }

        void client_GetNewsListByParamsCompleted(object sender, GetNewsListByParamsCompletedEventArgs e)
        {
            
        }
    }
}
