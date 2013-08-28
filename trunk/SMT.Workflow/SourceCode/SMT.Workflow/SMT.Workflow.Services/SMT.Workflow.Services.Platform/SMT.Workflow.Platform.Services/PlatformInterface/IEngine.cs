using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowXml;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    /// <summary>
    /// 默认消息接口
    /// </summary>
    [ServiceContract]
    public interface IEngine
    {

        //默认消息查询
        [OperationContract]
        List<T_WF_MESSAGEBODYDEFINE> GetFlowMsgDefine();
        //默认消息查询
        [OperationContract]
        List<T_WF_MESSAGEBODYDEFINE> GetListFlowMsgDefine(string filterString, int pageIndex, int pageSize, ref int pageCount);
        //默认消息查询
        [OperationContract]
        T_WF_MESSAGEBODYDEFINE GetListFlowMsgBodyDefine(string moduleCode);
        //添加默认消息
        [OperationContract]
        bool AddFlowMsgDefine(T_WF_MESSAGEBODYDEFINE FlowMsg);
        //修改默认消息
        [OperationContract]
        bool UpdateFlowMsgDefine(T_WF_MESSAGEBODYDEFINE FlowMsg);
        //删除默认消息
        [OperationContract]
        bool DeleteFlowMsgDefine(List<T_WF_MESSAGEBODYDEFINE> FlowMsglList);
       
    }
}