using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.Views;
using SMT.Workflow.Common.Model.FlowXml;

namespace SMT.Workflow.Platform.BLL
{
    public class FlowModelDefineBLL
    {

        FlowModelDefineDAL dal = new FlowModelDefineDAL();
        #region
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
                return dal.GetModelDefineList(filterString, pageIndex, pageSize, ref pageCount);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
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
                if (dal.GetExistModelDefine(FlowModel.MODELCODE, FlowModel.DESCRIPTION,""))
                {
                    return "10";
                }
                else
                {
                    return dal.AddModelDefine(FlowModel);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
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
                if (dal.GetExistModelDefine(FlowModel.MODELCODE, FlowModel.DESCRIPTION,FlowModel.MODELDEFINEID))
                {
                    return "10";
                }
                else
                {
                    return dal.UpdateModelDefine(FlowModel);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
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
                return dal.DeleteModelDefine(deleteList);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
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
                return dal.GetSystemCodeModelCodeList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion
   
        //哪些公司在模块中可以允许自选流程
        public List<FLOW_MODELDEFINE_FREEFLOW> GetFreeFlowList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            return dal.GetFreeFlowList(pageSize, pageIndex, strFilter, strOrderBy, ref  pageCount);
        }
        //哪些公司在模块中可以允许提单人撒回流程
        public List<FLOW_MODELDEFINE_FLOWCANCLE> GetFlowCancleList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            return dal.GetFlowCancleList(pageSize, pageIndex, strFilter, strOrderBy, ref  pageCount);
        }
        //删除允许自选流程的公司
        public bool DeleteFreeFlow(string modelcode, string companyid)
        {
            return dal.DeleteFreeFlow(modelcode, companyid);
        }
        //删除在模块中可以允许提单人撒回流程
        public bool DeleteFlowCancle(string modelcode, string companyid)
        {
            return dal.DeleteFlowCancle(modelcode, companyid);
        }
    }
}
