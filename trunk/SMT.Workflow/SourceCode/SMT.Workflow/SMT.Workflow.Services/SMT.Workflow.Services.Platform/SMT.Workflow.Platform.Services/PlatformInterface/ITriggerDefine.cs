using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    //定时触发接口
    [ServiceContract]
    public interface ITriggerDefine
    {
        //定时触发查询
        [OperationContract]
        List<T_WF_TIMINGTRIGGERCONFIG> GetFlowTriggerDefine(string filterString);
         //定时触发查询
        [OperationContract]
        List<T_WF_TIMINGTRIGGERCONFIG> GetListFlowTriggerDefine(string filterString, int pageIndex, int pageSize, ref int pageCount);
        ////添加定时接口
        //[OperationContract]
        //bool AddTriggerDefine(List<T_FLOW_TIMINGTRIGGERDEFINE> ListFlowTrigger);
        ////修改定时接口
        //[OperationContract]
        //bool UpdateTriggerDefine(List<T_FLOW_TIMINGTRIGGERDEFINE> ListFlowTrigger);
        //测试
        [OperationContract]
        bool GetTriggerDefine(TriggDefine TDefine);
        //删除定时触发
        [OperationContract]
        bool DeleteTriggerDefine(List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger);

    }
}