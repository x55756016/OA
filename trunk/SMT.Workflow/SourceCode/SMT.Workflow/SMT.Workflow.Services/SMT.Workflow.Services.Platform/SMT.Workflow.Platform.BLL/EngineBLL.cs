using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.BLL
{
    public class EngineBLL
    {
        #region 默认消息增、删、改、查
        EngineDAL dal = new EngineDAL();
        //查询默认消息
        public IQueryable<T_WF_MESSAGEBODYDEFINE> GetFlowMsgDefine()
        {
            return dal.GetFlowMsgDefine();
        }
        //查询默认消息
        public IQueryable<T_WF_MESSAGEBODYDEFINE> GetListFlowMsgDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            return dal.GetListFlowMsgDefine(filterString,pageIndex,pageSize,ref pageCount);
        }
        //查询默认消息
        public T_WF_MESSAGEBODYDEFINE GetListFlowMsgBodyDefine(string moduleCode)
        {
            return dal.GetListFlowMsgBodyDefine(moduleCode);
        }
        //添加默认消息
        public bool AddFlowMsgDefine(T_WF_MESSAGEBODYDEFINE FlowMsg)
        {
            return dal.AddFlowMsgDefine(FlowMsg);
        }
        //修改默认消息
        public bool UpdateFlowMsgDefine(T_WF_MESSAGEBODYDEFINE FlowMsg)
        {
            return dal.UpdateFlowMsgDefine(FlowMsg);
        }
        //删除默认消息
        public bool DeleteFlowMsgDefine(List<T_WF_MESSAGEBODYDEFINE> FlowMsgList)
        {
            return dal.DeleteFlowMsgDefine(FlowMsgList);
        }
        //删除默认消息
        public bool DeleteFlowMsgsDefine(T_WF_MESSAGEBODYDEFINE FlowMsgList)
        {
            return dal.DeleteFlowMsgsDefine(FlowMsgList);
        }
        #endregion
    }
}
