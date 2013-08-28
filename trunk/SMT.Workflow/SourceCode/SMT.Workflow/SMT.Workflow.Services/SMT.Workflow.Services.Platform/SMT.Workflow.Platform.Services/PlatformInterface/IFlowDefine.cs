using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.Views;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    /// <summary>
    /// 流程定义接口
    /// </summary>
    [ServiceContract]
    public interface IFlowDefine
    {
        /// <summary>
        /// 获取流程列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strFilter"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        List<V_FlowDefine> GetFlowDefineList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount);

        /// <summary>
        /// 新增流程
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        [OperationContract]
        string AddFlowDefine(V_FLOWDEFINITION flow);

        /// <summary>
        /// 根据FlowCode取流程
        /// </summary>
        /// <returns>V_FLOWDEFINITION</returns>
        [OperationContract]
        V_FLOWDEFINITION GetFlowEntity(string flowCode);

        /// <summary>
        /// 删除流程
        /// </summary>
        /// <param name="flowCodeList"></param>
        /// <returns></returns>
        [OperationContract]
        string DeleteFlow(List<string> flowCodeList);
    }
}
