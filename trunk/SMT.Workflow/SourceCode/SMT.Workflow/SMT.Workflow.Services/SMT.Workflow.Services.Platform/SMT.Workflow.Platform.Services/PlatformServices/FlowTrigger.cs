using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Common.Model.Views;

namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : IFlowTrigger
    {
        #region 触发规则定义增、删、改、查
        FlowTriggerBLL FlowsTriggerBll = new FlowTriggerBLL();
        //查询触发规则定义
        public List<V_DotaskRule> GetFlowTrigger(string filterString)
        {
            return FlowsTriggerBll.GetFlowTrigger(filterString).ToList();
        }
        //查询触发规则定义
        public List<V_DotaskRule> GetListFlowTrigger(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            return FlowsTriggerBll.GetListFlowTrigger(filterString ,pageIndex, pageSize, ref pageCount).ToList();
        }
        ////添加触发规则定义
        //public bool AddFlowTrigger(List<T_FLOW_FLOWPROCESSDEFINE> ListFlowTrigger)
        //{
        //    return FlowsTriggerBll.AddFlowTrigger(ListFlowTrigger);
        //}
        ////修改触发规则定义
        //public bool UpdateFlowTrigger(List<T_FLOW_FLOWPROCESSDEFINE> ListFlowTrigger)
        //{
        //    if (FlowsTriggerBll.DeleteFlowTrigger(ListFlowTrigger))
        //    {
        //        return FlowsTriggerBll.AddFlowTrigger(ListFlowTrigger);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //    //return FlowsTriggerBll.UpdateFlowTrigger(ListFlowTrigger);
        //}
        //删除触发规则定义
        public bool DeleteFlowTrigger(List<T_WF_DOTASKRULEDETAIL> ListFlowTrigger)
        {
            return FlowsTriggerBll.DeleteFlowTrigger(ListFlowTrigger);
        }
        //测试
        public bool GetFlowTriggerRlues(FlowTriggerRules TRules)
        {
            return true;
        }
        #endregion
    }
}