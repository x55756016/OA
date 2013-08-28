using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model.FlowEngine;


namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : ITriggerDefine
    {
        TriggerDefineBLL FlowTriggerBll = new TriggerDefineBLL();
        //定时触发查询
        public List<T_WF_TIMINGTRIGGERCONFIG> GetFlowTriggerDefine(string filterString)
        {
            return FlowTriggerBll.GetFlowTriggerDefine(filterString).ToList();
        }
        //定时触发查询
        public List<T_WF_TIMINGTRIGGERCONFIG> GetListFlowTriggerDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            return FlowTriggerBll.GetListFlowTriggerDefine(filterString,pageIndex, pageSize, ref pageCount).ToList();
        }
        ////添加定时触发
        //public bool AddTriggerDefine(List<T_FLOW_TIMINGTRIGGERDEFINE> ListFlowTrigger)
        //{
        //    return FlowTriggerBll.AddTriggerDefine(ListFlowTrigger);
        //}
        ////修改定时触发
        //public bool UpdateTriggerDefine(List<T_FLOW_TIMINGTRIGGERDEFINE> ListFlowTrigger)
        //{
        //    if (FlowTriggerBll.DeleteTriggerDefine(ListFlowTrigger))
        //    {
        //        return FlowTriggerBll.AddTriggerDefine(ListFlowTrigger);
        //    }
        //    else 
        //    {
        //        return false;
        //    }
        //}
        public bool GetTriggerDefine(TriggDefine TDefine)
        {
            return true;
        }
        //删除定时触发
        public bool DeleteTriggerDefine(List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger)
        {
            return FlowTriggerBll.DeleteTriggerDefine(ListFlowTrigger);
        }
    }
}