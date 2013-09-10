using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.OA.DAL.Views;
//using SMT.SaaS.OA.Services.FlowService;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        /// <summary>
        /// 获取事项审批信息
        /// </summary>
        /// <param name="id">事项审批ID</param>
        /// <returns>返回事项审批实体</returns>
        [OperationContract]
        private T_OA_APPROVALINFOTEMPLET Get_ApporvalTemplet(string id)
        {
            ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll();
            //using (ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll())
            //{
            return approvalBll.Get_ApporvalTemplet(id);
            //}
        }
        /// <summary>
        /// 获取事项审批信息
        /// </summary>
        /// <param name="id">事项审批ID</param>
        /// <returns>返回事项审批实体</returns>
        [OperationContract]
        private T_OA_APPROVALINFOTEMPLET Get_ApporvalTempletByApporvalType(string id)
        {
            ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll();
            //using (ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll())
            //{
            return approvalBll.Get_ApporvalTempletByApporvalType(id);
            //}
        }
        /// <summary>
        /// 获取事项审批列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="flagState"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_APPROVALINFOTEMPLET> GetApporvalTempletList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string flagState, LoginUserInfo loginUserInfo)//0待审核  1已审核
        {
            ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll();
            //using (ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll())
            //{
            if (flagState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                IQueryable<T_OA_APPROVALINFOTEMPLET> approvalList = approvalBll.GetApprovalTempletList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, null, flagState);
                if (approvalList == null)
                {
                    return null;
                }
                else
                {
                    return approvalList.ToList();
                }
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (flagState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "1", "0", "T_OA_APPROVALINFOTEMPLET", "", loginUserInfo.userID);
                if (flowList == null)
                {
                    return null;
                }
                List<string> guidStringList = new List<string>();
                foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                {
                    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T.FORMID);
                }
                if (guidStringList.Count < 1)
                {
                    return null;
                }
                IQueryable<T_OA_APPROVALINFOTEMPLET> approList = approvalBll.GetApprovalTempletList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, guidStringList, flagState);
                if (approList == null)
                {
                    return null;
                }
                else
                {
                    return approList.ToList();
                }
            }
            //}
        }
        /// <summary>
        /// 添加事项审批类型
        /// </summary>
        /// <param name="approvakInfo">实体</param>
        /// <param name="ApprovalCode">返回事项审批编号</param>
        /// <returns></returns>
        [OperationContract]
        public int AddApporvalTemplet(T_OA_APPROVALINFOTEMPLET approvakInfo,ref string ApprovalCode)
        {
            try
            {
                ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll();
                //using (ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll())
                //{
                bool sucess = approvalBll.AddApprovalTemplet(approvakInfo, ref ApprovalCode);
                if (sucess == false)
                    return -1;
                return 1;
            }
            catch (Exception ex)
            {                
                Tracer.Debug(ex.ToString());
                return 0;
            }
            //}
        }
        [OperationContract]
        public bool DeleteApporvalTemplet(T_OA_APPROVALINFOTEMPLET approvakInfo)
        {
            ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll();
            //using (ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll())
            //{
            return approvalBll.DeleteApprovalTemplet(approvakInfo);
            //}
        }
        /// <summary>
        /// 更新事项审批实体
        /// </summary>
        /// <param name="approvakInfo">需要更新的实现审批实体</param>
        /// <returns>成功返回 1 ，失败为-1</returns>
        [OperationContract]
        public int UpdateApporvalTemplet(T_OA_APPROVALINFOTEMPLET approvakInfo)
        {
            ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll();
            //using (ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll())
            //{
            if (approvalBll.UpdateApprovalTemplet(approvakInfo) != -1)
                return 1;
            else
                return -1;
            //}
        }
        [OperationContract]
        public int DeleteApporvalTempletList(List<T_OA_APPROVALINFOTEMPLET> approvakInfo)
        {
            ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll();
            //using (ApprovalTempletManagementBll approvalBll = new ApprovalTempletManagementBll())
            //{
            foreach (T_OA_APPROVALINFOTEMPLET obj in approvakInfo)
            {
                if (!approvalBll.DeleteApprovalTemplet(obj))
                {
                    return -1;
                }
            }
            return 1;
            //}
        }
    }
}