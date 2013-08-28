using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.Services.PlatformInterface;
using System.ServiceModel;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model.FlowXml;
using SMT.Workflow.Common.Model.Views;
using SMT.Workflow.Common.DataAccess;
namespace SMT.Workflow.Platform.Services
{
    /// <summary>
    /// 模块定义服务
    /// </summary>
    public partial class PlatformServices : IFlow_ModelDefine
    {
        FlowModelDefineBLL FlowModelBll = new FlowModelDefineBLL();
        #region 模块定义调整的新接口
        /// <summary>
        ///  查询模块分页列表
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<FLOW_MODELDEFINE_T> GetModelDefineList(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            try
            {
                return FlowModelBll.GetModelDefineList(filterString, pageIndex, pageSize, ref pageCount);
            }
            catch (Exception ex)
            {                
                LogHelper.WriteLog("查询模块定义出错：" + ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 新增模块代码
        /// </summary>
        /// <param name="FlowModel"></param>
        /// <returns></returns>
        public string AddModelDefine(FLOW_MODELDEFINE_T FlowModel)
        {
            try
            {
                return FlowModelBll.AddModelDefine(FlowModel);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("新增模块定义出错：" + ex.Message);
                return "0";
            }
        }

        /// <summary>
        /// 修改模块代码
        /// </summary>
        /// <param name="FlowModel"></param>
        /// <returns></returns>
        public string UpdateModelDefine(FLOW_MODELDEFINE_T FlowModel)
        {
            try
            {
                return FlowModelBll.UpdateModelDefine(FlowModel);

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("修改模块定义出错：" + ex.Message);
                return "0";
            }
        }
        /// <summary>
        /// 删除模块代码
        /// </summary>
        /// <param name="deleteList"></param>
        /// <returns></returns>
        public string DeleteModelDefine(List<string> deleteList)
        {
            try
            {
                return FlowModelBll.DeleteModelDefine(deleteList);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("删除模块定义出错：" + ex.Message);
                return "0";
            }
        }
        /// <summary>
        /// 获取所有的系统代码模块代码
        /// </summary>
        /// <returns></returns>
        public List<FLOW_MODELDEFINE_T> GetSystemCodeModelCodeList()
        {
            try
            {
                return FlowModelBll.GetSystemCodeModelCodeList();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取所有的系统代码模块代码出错：" + ex.Message);
                return null;
            }
        }
        #endregion

        #region  模块增、删、改、查方法

    
        //测试
        //public string FlowModelDefine(V_Modeldefine FlowModel)
        //{
        //    return null;
        //}
        //哪些公司在模块中可以允许自选流程
        public List<FLOW_MODELDEFINE_FREEFLOW> GetFreeFlowList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            return FlowModelBll.GetFreeFlowList(pageSize, pageIndex, strFilter, strOrderBy, ref  pageCount);
        }
        //哪些公司在模块中可以允许提单人撒回流程
        public List<FLOW_MODELDEFINE_FLOWCANCLE> GetFlowCancleList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            return FlowModelBll.GetFlowCancleList(pageSize, pageIndex, strFilter, strOrderBy, ref  pageCount);
        }
        //删除允许自选流程的公司
        public bool DeleteFreeFlow(string modelcode, string companyid)
        {
            return FlowModelBll.DeleteFreeFlow(modelcode, companyid);
        }
        //删除在模块中可以允许提单人撒回流程
        public bool DeleteFlowCancle(string modelcode, string companyid)
        {
            return FlowModelBll.DeleteFlowCancle(modelcode, companyid);
        }
        #endregion
    }
}
