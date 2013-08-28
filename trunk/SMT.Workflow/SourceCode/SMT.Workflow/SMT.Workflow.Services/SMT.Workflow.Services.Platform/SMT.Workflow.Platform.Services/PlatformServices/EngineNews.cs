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
    public partial class PlatformServices : IEngine
    {
        #region  默认消息增、删、改、查方法
        EngineBLL FlowMsgBll = new EngineBLL();
        //默认消息查询
        public List<T_WF_MESSAGEBODYDEFINE> GetFlowMsgDefine()
        {
            return FlowMsgBll.GetFlowMsgDefine().ToList();
        }
        //默认消息查询
        public List<T_WF_MESSAGEBODYDEFINE> GetListFlowMsgDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            return FlowMsgBll.GetListFlowMsgDefine(filterString,pageIndex, pageSize, ref pageCount).ToList();
        }
        //默认消息查询
        public T_WF_MESSAGEBODYDEFINE GetListFlowMsgBodyDefine(string moduleCode)
        {
            return FlowMsgBll.GetListFlowMsgBodyDefine(moduleCode);
        }
        //添加默认消息
        public bool AddFlowMsgDefine(T_WF_MESSAGEBODYDEFINE FlowMsg)
        {
            return FlowMsgBll.AddFlowMsgDefine(FlowMsg);
        }
        //修改默认消息
        public bool UpdateFlowMsgDefine(T_WF_MESSAGEBODYDEFINE FlowMsg)
        {
            return FlowMsgBll.UpdateFlowMsgDefine(FlowMsg);
        }
        //删除默认消息
        public bool DeleteFlowMsgDefine(List<T_WF_MESSAGEBODYDEFINE> FlowMsglList)
        {
            return FlowMsgBll.DeleteFlowMsgDefine(FlowMsglList);
        }
        #endregion
    }
}