using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT.Workflow.Common.Model;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    [ServiceContract]
    public interface IDefaultMessage
    {

        [OperationContract]
        List<T_WF_DEFAULTMESSAGE> GetMessage(int pageIndex, int pageSize, string strFilter, string strOrderBy, ref int pageCount);

        [OperationContract]
        string InitMessage();

        [OperationContract]
        string AddMessage(T_WF_DEFAULTMESSAGE entity);

        [OperationContract]
        string EditMessage(T_WF_DEFAULTMESSAGE entity);

        [OperationContract]
        string DeleteMessage(string MessageID);
    }
}