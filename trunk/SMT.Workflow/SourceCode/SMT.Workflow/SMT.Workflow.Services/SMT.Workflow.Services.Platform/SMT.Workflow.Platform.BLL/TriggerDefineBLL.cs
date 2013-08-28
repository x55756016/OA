using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.BLL
{
    public class TriggerDefineBLL
    {
        #region 定时触发增、删、改、查
        TriggerDefineDAL dal = new TriggerDefineDAL();
         //定时触发查询
        public IQueryable<T_WF_TIMINGTRIGGERCONFIG> GetFlowTriggerDefine(string filterString)
        {
            return dal.GetFlowTriggerDefine(filterString);
        }
         //定时触发查询
        public IQueryable<T_WF_TIMINGTRIGGERCONFIG> GetListFlowTriggerDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            return dal.GetListFlowTriggerDefine(filterString,pageIndex, pageSize, ref pageCount);
        }
        ////查询默认消息
        //public IQueryable<T_FLOW_MSGBODYDEFINE> GetListFlowMsgDefine(string filterString, object[] paras)
        //{
        //    return dal.GetListFlowMsgDefine(filterString, paras);
        //}
        ////查询默认消息
        //public T_FLOW_MSGBODYDEFINE GetListFlowMsgBodyDefine(string moduleCode)
        //{
        //    return dal.GetListFlowMsgBodyDefine(moduleCode);
        //}
        ////添加默认消息
        //public bool AddTriggerDefine(List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger)
        //{
        //    return dal.AddTriggerDefine(ListFlowTrigger);
        //}
        ////修改默认消息
        //public bool UpdateFlowMsgDefine(T_FLOW_MSGBODYDEFINE FlowMsg)
        //{
        //    return dal.UpdateFlowMsgDefine(FlowMsg);
        //}
        //删除定时触发
        public bool DeleteTriggerDefine(List<T_WF_TIMINGTRIGGERCONFIG> ListFlowTrigger)
        {
            return dal.DeleteTriggerDefine(ListFlowTrigger);
        }
        #endregion
    }
}
