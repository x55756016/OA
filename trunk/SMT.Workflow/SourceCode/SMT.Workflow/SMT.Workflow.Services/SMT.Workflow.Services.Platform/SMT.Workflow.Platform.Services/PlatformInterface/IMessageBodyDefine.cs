using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Common.Model.FlowEngine;
using System.ServiceModel;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    [ServiceContract]
    public interface IMessageBodyDefine
    {

        /// <summary>
        /// 获取默认消息的分页列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="strFilter"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        List<T_WF_MESSAGEBODYDEFINE> GetDefaultMessgeList(int pageIndex, int pageSize, string strFilter, string strOrderBy, ref int pageCount);

        [OperationContract]
        string AddDefaultMessge(T_WF_MESSAGEBODYDEFINE entity);


        [OperationContract]
        string EditDefaultMessge(T_WF_MESSAGEBODYDEFINE entity);
        [OperationContract]
        string DeleteDefaultMessge(string DeleteID);
    }
}