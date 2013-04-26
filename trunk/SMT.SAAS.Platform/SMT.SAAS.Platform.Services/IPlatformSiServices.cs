using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT.SAAS.Platform.Model;

namespace SMT.SAAS.Platform.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPlatformSiServices" in both code and config file together.
    [ServiceContract]
    public interface IPlatformSiServices
    {
        [OperationContract]
        bool AddShortCut(ShortCut model);

        [OperationContract]
        bool AddShortCutByList(List<ShortCut> models);

        [OperationContract]
        bool AddShortCutByUser(List<ShortCut> models,string userID);

        [OperationContract]
        List<ShortCut> GetShortCutByUser(string userSysID);
    }
}
