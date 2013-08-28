using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.BLL
{
    public class CuostomFlowsDefineBLL
    {
        #region 自动发起流程增、删、改、查
        CuostomFlowsDefineDAL dal = new CuostomFlowsDefineDAL();
        //自动发起流程查询
        public IQueryable<T_FLOW_CUSTOMFLOWDEFINE> GetCuostomFlowsDefine(string filterString)
        {
            return dal.GetCuostomFlowsDefine(filterString);
        }
         //自动发起流程查询
        public IQueryable<T_FLOW_CUSTOMFLOWDEFINE> GetListCuostomFlowsDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            return dal.GetListCuostomFlowsDefine(filterString, pageIndex, pageSize, ref pageCount);
        }
        //添加自动发起流程
        public bool AddCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            return dal.AddCuostomFlowsDefine(ListCuostomFlowsDefine);
        }
        //修改自动发起流程
        public bool UpdateCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            return dal.UpdateCuostomFlowsDefine(ListCuostomFlowsDefine);
        }
        //删除自动发起流程
        public bool DeleteCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            return dal.DeleteCuostomFlowsDefine(ListCuostomFlowsDefine);
        }
        #endregion
    }
}
