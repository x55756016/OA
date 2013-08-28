using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.Views;
using SMT.Workflow.Common.Model.FlowXml;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    /// <summary>
    /// 模块定义接口
    /// </summary>
    [ServiceContract]
    public interface IFlow_ModelDefine
    {
        #region 模块定义服务接口调整的新接口 kangxf 2012/09/05
        /// <summary>
        ///  查询模块分页列表
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        List<FLOW_MODELDEFINE_T> GetModelDefineList(string filterString, int pageIndex, int pageSize, ref int pageCount);

        /// <summary>
        /// 新增模块代码
        /// </summary>
        /// <param name="FlowModel"></param>
        /// <returns></returns>
        [OperationContract]
        string AddModelDefine(FLOW_MODELDEFINE_T FlowModel);

        /// <summary>
        /// 修改模块代码
        /// </summary>
        /// <param name="FlowModel"></param>
        /// <returns></returns>
        [OperationContract]
        string UpdateModelDefine(FLOW_MODELDEFINE_T FlowModel);

         /// <summary>
        /// 删除模块代码
        /// </summary>
        /// <param name="deleteList"></param>
        /// <returns></returns>
        [OperationContract]
        string DeleteModelDefine(List<string> deleteList);

        /// <summary>
        /// 获取所有的系统代码模块代码
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<FLOW_MODELDEFINE_T> GetSystemCodeModelCodeList();
        #endregion

        #region 模块接口
        //查询模块
    
        //string FlowModelDefine(V_Modeldefine FlowModel);
        //哪些公司在模块中可以允许自选流程
         [OperationContract]
         List<FLOW_MODELDEFINE_FREEFLOW> GetFreeFlowList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount);
        
        //哪些公司在模块中可以允许提单人撒回流程
         [OperationContract]
         List<FLOW_MODELDEFINE_FLOWCANCLE> GetFlowCancleList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount);
         //删除允许自选流程的公司
            [OperationContract]
        bool DeleteFreeFlow(string modelcode, string companyid);
        
         //删除在模块中可以允许提单人撒回流程
            [OperationContract]
            bool DeleteFlowCancle(string modelcode, string companyid);
         
        #endregion
    }
}
