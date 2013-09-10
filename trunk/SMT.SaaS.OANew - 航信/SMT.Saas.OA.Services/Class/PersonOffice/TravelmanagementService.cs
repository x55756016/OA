using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.OA.DAL.Views;
using SMT.SaaS.BLLCommonServices.FlowWFService;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        ServiceClient workFlowWS = new ServiceClient();

        #region 出差管理
        /// <summary>
        /// 获取所有的申请信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_BUSINESSTRIP> GetTravelmanagementInfo(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                IQueryable<T_OA_BUSINESSTRIP> TravelmanagementList = TL.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, "T_OA_BUSINESSTRIP");
                return TravelmanagementList == null ? null : TravelmanagementList.ToList();
            }
        }
        /// <summary>
        /// 根据申请ID获取申请信息
        /// </summary>
        /// <param name="contractID">申请编号</param>
        /// <returns></returns>
        [OperationContract]
        public T_OA_BUSINESSTRIP GetTravelmanagementById(string TravelmanagementID)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                T_OA_BUSINESSTRIP TravelmanagementById = TL.GetTravelmanagementById(TravelmanagementID);
                return TravelmanagementById == null ? null : TravelmanagementById;
            }
        }
        //[OperationContract]
        //public T_OA_BUSINESSTRIP GetTravelmanagementBysId()
        //{
        //    using (TravelmanagementBLL TL = new TravelmanagementBLL())
        //    {
        //        T_OA_BUSINESSTRIP MyContract = TL.GetTravelmanagementBysId();
        //        return MyContract != null ? MyContract : null;
        //    }
        //}

        [OperationContract]
        public T_OA_BUSINESSTRIP GetTravelmanagementBysId(string tripid)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                T_OA_BUSINESSTRIP MyContract = TL.GetTravelmanagementBysId(tripid);
                return MyContract != null ? MyContract : null;
            }
        }

        /// <summary>
        /// 根据出差申请ID查询出差报告、报销的ID
        /// </summary>
        /// <param name="businesstripId"></param>
        /// <returns></returns>
        [OperationContract]
        public V_Travelmanagement GetAccordingToBusinesstripIdCheck(string businesstripId)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                V_Travelmanagement TravelmanagementById = TL.GetAccordingToBusinesstripIdCheck(businesstripId);
                return TravelmanagementById == null ? null : TravelmanagementById;
            }
        }

        /// <summary>
        /// 检查是否存在该申请信息
        /// </summary>
        /// <param name="ContractApproval"></param>
        /// <param name="ContractApprovalID"></param>
        /// <returns></returns>
        [OperationContract]
        private bool IsExistTravelmanagement(string Travelmanagement, string TravelmanagementID)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                return TL.IsExistContractTravelmanagement(Travelmanagement, TravelmanagementID);
            }
        }
        /// <summary>
        /// 更新申请信息
        /// </summary>
        /// <param name="contraApprovalInfo"></param>
        [OperationContract]
        public string UpdateTravelmanagement(T_OA_BUSINESSTRIP TravelmanagementInfo, List<T_OA_BUSINESSTRIPDETAIL> TraveDetail,string FormType)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                string Travelmanagement = "";
                if (!TL.UpdateTravelmanagement(TravelmanagementInfo, TraveDetail, FormType))
                {
                    Travelmanagement = "更新数据失败！";
                }
                return Travelmanagement;
            }
        }
        /// <summary>
        /// 删除申请信息
        /// </summary>
        /// <param name="contraApprovalID"></param>
        /// <returns></returns>
        [OperationContract]
        public bool DeleteTravelmanagement(string[] TravelmanagementID, ref bool FBControl)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                return TL.DeleteTravelmanagement(TravelmanagementID, ref FBControl);
            }
        }
        /// <summary>
        /// 添加出差申请信息
        /// </summary>
        /// <param name="ContractApproval"></param>
        /// <returns></returns>
        [OperationContract]
        public string TravelmanagementAdd(T_OA_BUSINESSTRIP Travelmanagement, List<T_OA_BUSINESSTRIPDETAIL> TraveDetail)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                string returnStr = "";

                if (!TL.TravelmanagementAdd(Travelmanagement, TraveDetail))
                {
                    returnStr = "添加数据失败";
                }
                return returnStr;
            }
        }
        /// <summary>
        /// 获取所有的申请信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_BUSINESSTRIP> GetTravelmanagementRoomInfos()
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                List<T_OA_BUSINESSTRIP> ContractApprovalRoomList = TL.GetTravelmanagementRooms();
                if (ContractApprovalRoomList == null)
                {
                    return null;
                }
                else
                {
                    return ContractApprovalRoomList;
                }
            }
        }
        /// <summary>
        /// 根据标题、ID获取申请信息
        /// </summary>
        /// <param name="StrContractApprovalName"></param>
        /// <param name="StrID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_Travelmanagement> GetTravelmanagementRoomInfosListBySearch(string ownerid, string DepCity, string DestCity, string startTime, string endTime)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                List<V_Travelmanagement> TravelmanagementList = TL.GetTravelmanagementRoomInfosListBySearch(ownerid, DepCity, DestCity, startTime, endTime);
                if (TravelmanagementList == null)
                {
                    return null;
                }
                else
                {
                    return TravelmanagementList.ToList();
                }
            }
        }
        #endregion

        [OperationContract]
        public List<T_OA_BUSINESSTRIPDETAIL> GetBusinesstripDetail(string buipId)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                List<T_OA_BUSINESSTRIPDETAIL> details = TL.GetBusinesstripDetail(buipId);
                return details;
            }
        }

        #region 出差申请是否能被查看
        /// <summary>
        /// 是否能被查看
        /// </summary>
        /// <param name="ContractID"></param>
        /// <returns></returns>
        [OperationContract]
        public bool IsContractCanBrowser(string TravelmanagementID)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                return TL.IsContractCanBrowser(TravelmanagementID);
            }
        }

        #endregion

        #region 查询用户申请记录
        /// <summary>
        /// 查询用户申请记录
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="searchObj"></param>
        /// <param name="formIDList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_Travelmanagement> GetTravelmanagementListByUserId(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (TravelmanagementBLL TL = new TravelmanagementBLL())
            {
                List<V_Travelmanagement> TravelmanagementList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交申请信息
                {
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        TravelmanagementList = TL.GetTravelmanagementInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        TravelmanagementList = TL.GetTravelmanagementInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    return TravelmanagementList != null ? TravelmanagementList.ToList() : null;
                }
                else                    //通过工作流获取用户要审批的申请信息
                {
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "1", "0", "T_OA_BUSINESSTRIP", loginUserInfo.companyID, loginUserInfo.userID);
                    FLOW_FLOWRECORDDETAIL_T[] ReportflowList = workFlowWS.GetFlowInfo("", "", "1", "0", "T_OA_BUSINESSREPORT", loginUserInfo.companyID, loginUserInfo.userID);
                    FLOW_FLOWRECORDDETAIL_T[] ReimflowList = workFlowWS.GetFlowInfo("", "", "1", "0", "T_OA_TRAVELREIMBURSEMENT", loginUserInfo.companyID, loginUserInfo.userID);
                    List<V_FlowAPP> flowAppList = new List<V_FlowAPP>();
                    if (flowList != null)
                    {
                        for (int i = 0; i < flowList.Length; i++)
                        {
                            V_FlowAPP App = new V_FlowAPP();
                            App.Guid = flowList[i].FLOWRECORDDETAILID;
                            App.FormID = flowList[i].FLOW_FLOWRECORDMASTER_T.FORMID;
                            App.EditUserID = flowList[i].EDITUSERID;
                            App.EditUserName = flowList[i].EDITUSERNAME;
                            flowAppList.Add(App);
                        }
                    }
                    //出差报告流程调用
                    if (ReportflowList != null)
                    {
                        for (int i = 0; i < ReportflowList.Length; i++)
                        {
                            V_FlowAPP App = new V_FlowAPP();
                            App.Guid = ReportflowList[i].FLOWRECORDDETAILID;
                            App.FormID = ReportflowList[i].FLOW_FLOWRECORDMASTER_T.FORMID;
                            App.EditUserID = ReportflowList[i].EDITUSERID;
                            App.EditUserName = ReportflowList[i].EDITUSERNAME;
                            flowAppList.Add(App);
                        }
                    }
                    //出差报销
                    if (ReimflowList != null)
                    {
                        for (int i = 0; i < ReimflowList.Length; i++)
                        {
                            V_FlowAPP App = new V_FlowAPP();
                            App.Guid = ReimflowList[i].FLOWRECORDDETAILID;
                            App.FormID = ReimflowList[i].FLOW_FLOWRECORDMASTER_T.FORMID;
                            App.EditUserID = ReimflowList[i].EDITUSERID;
                            App.EditUserName = ReimflowList[i].EDITUSERNAME;
                            flowAppList.Add(App);
                        }
                    }
                    checkState = ((int)CheckStates.Approving).ToString();
                    TravelmanagementList = TL.GetTravelmanagementInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                }
                return TravelmanagementList != null ? TravelmanagementList.ToList() : null;
            }
        }
        #endregion

        //#region 选择出差报告时查询出差申请记录
        ///// <summary>
        ///// 选择出差报告时查询出差申请记录
        ///// </summary>
        ///// <param name="userID"></param>
        ///// <param name="searchObj"></param>
        ///// <param name="formIDList"></param>
        ///// <param name="checkState"></param>
        ///// <returns></returns>
        //[OperationContract]
        //public List<V_Travelmanagement> GetCheckTravelmanagement(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        //{
        //    using (TravelmanagementBLL TL = new TravelmanagementBLL())
        //    {
        //        List<V_Travelmanagement> TravelmanagementList = TL.GetCheckTravelmanagement(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
        //        return TravelmanagementList != null ? TravelmanagementList.ToList() : null;
        //    }
        //}
        //#endregion

        #region 查找某员工的未解决的出差申请/报销
        [OperationContract]
        public Dictionary<string, string> GetUnderwayTravelmanagement(string employeeid)
        {
            using (TravelmanagementBLL TL=new TravelmanagementBLL())
            {
                return TL.GetUnderwayTravelmanagement(employeeid);
            }
        }
        #endregion
    }
}
