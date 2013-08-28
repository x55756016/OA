using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.Views;
using SMT.Workflow.Platform.DAL;

namespace SMT.Workflow.Platform.BLL
{
    public class FlowDefineBLL
    {
        FlowDefineDAL dal = new FlowDefineDAL();
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
                return dal.GetFlowDefineList(pageSize, pageIndex, strFilter, strOrderBy, ref pageCount);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 新增流程
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        public bool AddFlowDefine(V_FLOWDEFINITION flow)
        {
            try
            {
                return dal.AddFlowDefine(flow);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
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
                return dal.GetFlowEntity(flowCode);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 删除流程
        /// </summary>
        /// <param name="flowCodeList"></param>
        /// <returns></returns>
        public bool DeleteFlow(List<string> flowCodeList)
        {
            try
            {
                return dal.DeleteFlow(flowCodeList);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

      
    }
}
