


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.Services.PlatformInterface;
using System.ServiceModel;

using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.Views;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.DataAccess;

namespace SMT.Workflow.Platform.Services
{
    /// <summary>
    /// 流程定义服务
    /// </summary>
    public partial class PlatformServices : IFlowDefine
    {
        FlowDefineBLL flowBLL = new FlowDefineBLL();

        /// <summary>
        /// 获取流程列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strFilter"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<V_FlowDefine> GetFlowDefineList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {
                return flowBLL.GetFlowDefineList(pageSize, pageIndex, strFilter, strOrderBy, ref pageCount);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取流程列表义出错：" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 新增流程
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        public string  AddFlowDefine(V_FLOWDEFINITION flow)
        {
            try
            {
                if(flowBLL.AddFlowDefine(flow))
                {
                    return "1";
                }
                return "新建流程失败！";
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("新建流程出错：" + ex.Message + "   公司ID:" + flow.ModelRelation.COMPANYID);
                return ex.Message;
            }
        }

        /// <summary>
        /// 根据FlowCode取流程
        /// </summary>
        /// <returns>V_FLOWDEFINITION</returns>
        public V_FLOWDEFINITION GetFlowEntity(string flowCode)
        {
            try
            {
                return flowBLL.GetFlowEntity(flowCode);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("根据FlowCode取流程出错：" + ex.Message + "   flowCode:" + flowCode);
                return null;                
            }
        }

        /// <summary>
        /// 删除流程
        /// </summary>
        /// <param name="flowCodeList"></param>
        /// <returns></returns>
        public string DeleteFlow(List<string> flowCodeList)
        {        
            try
            {
                if (flowBLL.DeleteFlow(flowCodeList))
                {
                    return "1";
                }
                return "删除流程失败！";
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("删除流程出错：" + ex.Message + "   flowCodeList[0]:" + flowCodeList[0]);
                return ex.Message;
            }
        }
    }
}
