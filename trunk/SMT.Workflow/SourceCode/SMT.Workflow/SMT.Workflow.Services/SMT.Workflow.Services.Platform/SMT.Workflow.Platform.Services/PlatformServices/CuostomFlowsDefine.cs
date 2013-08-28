using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : ICuostomFlowsDefine
    {
        CuostomFlowsDefineBLL CuostomFlowBll = new CuostomFlowsDefineBLL();
        //自动发起流程查询
        public List<T_FLOW_CUSTOMFLOWDEFINE> GetCuostomFlowsDefine(string filterString)
        {
            return CuostomFlowBll.GetCuostomFlowsDefine(filterString).ToList();
        }
        //自动发起流程查询
        public List<T_FLOW_CUSTOMFLOWDEFINE> GetListCuostomFlowsDefine(string filterString, int pageIndex, int pageSize, ref int pageCount)
        {
            return CuostomFlowBll.GetListCuostomFlowsDefine(filterString,pageIndex, pageSize, ref pageCount).ToList();
        }
        //添加自动发起流程
        public bool AddCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            return CuostomFlowBll.AddCuostomFlowsDefine(ListCuostomFlowsDefine);
        }
        public bool GetsCuostomFlowsDefine(CuostomFlowsDefine CTDefine)
        {
            return true;
        }
        //修改自动发起流程
        public bool UpdateCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            return CuostomFlowBll.UpdateCuostomFlowsDefine(ListCuostomFlowsDefine);
        }
        //删除自动发起流程
        public bool DeleteCuostomFlowsDefine(List<T_FLOW_CUSTOMFLOWDEFINE> ListCuostomFlowsDefine)
        {
            return CuostomFlowBll.DeleteCuostomFlowsDefine(ListCuostomFlowsDefine);
        }
    }
}