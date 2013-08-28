using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Common.Model.Views;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    /// <summary>
    /// 触发规则定义接口
    /// </summary>
    [ServiceContract]
    public interface IFlowTrigger
    {
        //查询触发规则定义
        [OperationContract]
        List<V_DotaskRule> GetFlowTrigger(string filterString);
        //查询触发规则定义
        [OperationContract]
        List<V_DotaskRule> GetListFlowTrigger(string filterString, int pageIndex, int pageSize, ref int pageCount);
        ////添加触发规则定义
        //[OperationContract]
        //bool AddFlowTrigger(List<T_FLOW_FLOWPROCESSDEFINE> ListFlowTrigger);
        ////修改触发规则定义
        //[OperationContract]
        //bool UpdateFlowTrigger(List<T_FLOW_FLOWPROCESSDEFINE> ListFlowTrigger);
        //删除触发规则定义
        [OperationContract]
        bool DeleteFlowTrigger(List<T_WF_DOTASKRULEDETAIL> ListFlowTrigger);
        //测试
        [OperationContract]
        bool GetFlowTriggerRlues(FlowTriggerRules TRules);
    }
}