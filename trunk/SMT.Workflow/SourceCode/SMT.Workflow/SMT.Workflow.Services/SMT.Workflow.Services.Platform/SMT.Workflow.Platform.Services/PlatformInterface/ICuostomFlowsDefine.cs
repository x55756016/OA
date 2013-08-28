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
    public interface ICuostomFlowsDefine
    {
        //自动发起流程查询
        [OperationContract]
        List<T_FLOW_CUSTOMFLOWDEFINE> GetCuostomFlowsDefine(string filterString);
        //自动发起流程查询
        [OperationContract]
        List<T_FLOW_CUSTOMFLOWDEFINE> GetListCuostomFlowsDefine(string filterString, int pageIndex, int pageSize, ref int pageCount);
        //添加自动发起流程
        [OperationContract]
        bool AddCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine);
        
        [OperationContract]
        bool GetsCuostomFlowsDefine(CuostomFlowsDefine CTDefine);
         //修改自动发起流程
        [OperationContract]
        bool UpdateCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine);
        //删除自动发起流程
        [OperationContract]
        bool DeleteCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine);
    }
}