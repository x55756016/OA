using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Common.Model.Views;

namespace SMT.Workflow.Platform.BLL
{
    public class FlowTriggerBLL
    {
        #region 自动发起流程增、删、改、查
        FlowTriggerDAL dal = new FlowTriggerDAL();
         //自动触发规则定义
        public IQueryable<V_DotaskRule> GetFlowTrigger(string filterString)
        {
            return dal.GetFlowTrigger(filterString);
        }
        //查询触发规则定义
        public IQueryable<V_DotaskRule> GetListFlowTrigger(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            return dal.GetListFlowTrigger(filterString, pageIndex, pageSize, ref pageCount);
        }
        ////添加触发规则定义
        //public bool AddFlowTrigger(List<T_FLOW_FLOWPROCESSDEFINE> ListFlowTrigger)
        //{
        //    return dal.AddFlowTrigger(ListFlowTrigger);
        //}
        ////修改触发规则定义
        //public bool UpdateFlowTrigger(List<T_FLOW_FLOWPROCESSDEFINE> ListFlowTrigger)
        //{
        //    return dal.UpdateFlowTrigger(ListFlowTrigger);
        //}
        //删除触发规则定义
        public bool DeleteFlowTrigger(List<T_WF_DOTASKRULEDETAIL> ListFlowTrigger)
        {
            return dal.DeleteFlowTrigger(ListFlowTrigger);
        }
        #endregion
    }
}
