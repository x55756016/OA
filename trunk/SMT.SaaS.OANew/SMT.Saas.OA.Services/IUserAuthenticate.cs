using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;

namespace SMT.SaaS.OA.Services
{
    [ServiceContract]
    public interface IUserAuthenticate
    {
        [OperationContract]
        string VerifyUser(string username, string password, string appcode);
    }
}