//出差报销服务
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

        #region 出差报销管理
        /// <summary>
        /// 获取所有的报销信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_TRAVELREIMBURSEMENT> GetTravelReimbursementInfo()
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                IQueryable<T_OA_TRAVELREIMBURSEMENT> TravelReimbursementList = TrBll.GetTravelReimbursement();
                return TravelReimbursementList == null ? null : TravelReimbursementList.ToList();
            }
        }
        /// <summary>
        /// 根据报销ID获取报销信息
        /// </summary>
        /// <param name="TravelReimbursementID">报销编号</param>
        /// <returns></returns>
        [OperationContract]
        public T_OA_TRAVELREIMBURSEMENT GetTravelReimbursementById(string TravelReimbursementID)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                T_OA_TRAVELREIMBURSEMENT TravelReimbursement = TrBll.GetTravelReimbursementById(TravelReimbursementID);
                return TravelReimbursement != null ? TravelReimbursement : null;
            }
        }
        /// <summary>
        /// 检查是否存在该报销信息
        /// </summary>
        /// <param name="ownerid">报销人</param>
        /// <param name="TravelReimbursementID">报销单ID</param>
        /// <returns></returns>
        [OperationContract]
        private bool IsExistTravelReimbursement(string ownerid, string TravelReimbursementID)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                return TrBll.IsExistContractTravelReimbursement(ownerid, TravelReimbursementID);
            }
        }

        /// <summary>
        /// 检查出差报告是否生成了出差报销单
        /// </summary>
        /// <param name="ownerid">报销人</param>
        /// <param name="TravelReimbursementID">报销单ID</param>
        /// <returns></returns>
        [OperationContract]
        private bool IsExistTravelReimbursementBySportid(string ownerid, string sportid)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                return TrBll.IsExistTravelReimbursementBySportid(ownerid, sportid);
            }
        }


        /// <summary>
        /// 更新报销单号
        /// </summary>
        /// <param name="TravelReimbursementInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateNoClaims(T_OA_TRAVELREIMBURSEMENT TravelNoClaims, List<T_OA_REIMBURSEMENTDETAIL> portDetail, string FormType)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                string result = "";
                if (!TrBll.UpdateNoClaims(TravelNoClaims, portDetail, FormType))
                {
                    result = "修改数据失败！";
                }
                return result;
            }
        }
        /// <summary>
        /// 更新报销信息
        /// </summary>
        /// <param name="TravelReimbursementInfo">报销单实体</param>
        /// <param name="portDetail">报销明细</param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateTravelReimbursement(T_OA_TRAVELREIMBURSEMENT TravelReimbursementInfo, List<T_OA_REIMBURSEMENTDETAIL> portDetail, string FormType)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                string result = "";
                if (!TrBll.UpdateTravelReimbursement(TravelReimbursementInfo, portDetail, FormType))
                {
                    result = "修改数据失败！";
                }
                return result;
            }
        }
        /// <summary>
        /// 删除报销信息
        /// </summary>
        /// <param name="TravelReimbursementID">报销单实体</param>
        /// <param name="FBControl">费用</param>
        /// <returns></returns>
        [OperationContract]
        public bool DeleteTravelReimbursement(string[] TravelReimbursementID, ref bool FBControl)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                return TrBll.DeleteTravelReimbursement(TravelReimbursementID, ref FBControl);
            }
        }

        /// <summary>
        /// 通过出差申请id删除报销信息
        /// </summary>
        /// <param name="TravelReimbursementID">报销单实体</param>
        /// <param name="FBControl">费用</param>
        /// <returns></returns>
        [OperationContract]
        public bool DeleteTravelReimbursementByBusinesstripId(string[] TravelmanagementID, ref bool FBControl)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                var TravelReimbursementID = TrBll.GetReimbursementIDsByBusinesstripId(TravelmanagementID);
                if (TravelmanagementID.Count() > 0)
                {
                    return TrBll.DeleteTravelReimbursement(TravelReimbursementID.ToArray(), ref FBControl);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("TravelReimbursermentService-DeleteTravelReimbursementByBusinesstripId TravelReimbursementID为零");
                }
                return false;
            }
        }

        ///// <summary>
        ///// 添加出差报销信息
        ///// </summary>
        ///// <param name="TravelReimbursement">报销单实体</param>
        ///// <param name="portDetail">报销明细</param>
        ///// <returns></returns>
        //[OperationContract]
        //public string TravelReimbursementAdd(T_OA_TRAVELREIMBURSEMENT TravelReimbursement, List<T_OA_REIMBURSEMENTDETAIL> portDetail)
        //{
        //    using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
        //    {
        //        string returnStr = "";
        //        if (!this.IsExistTravelReimbursement(TravelReimbursement.CLAIMSWERE, TravelReimbursement.TRAVELREIMBURSEMENTID))
        //        {
        //            if (!TrBll.TravelReimbursementAdd(TravelReimbursement, portDetail))
        //            {
        //                returnStr = "添加数据失败";
        //            }
        //        }
        //        else
        //        {
        //            returnStr = "合同报销信息已经存在,添加数据失败";
        //        }
        //        return returnStr;
        //    }
        //}


        /// <summary>
        /// 添加出差报销信息  edit ljx 2011-8-29 
        /// 修改理由：如果用户添加了一条则不再添加
        /// </summary>
        /// <param name="TravelReimbursement">报销单实体</param>
        /// <param name="portDetail">报销明细</param>
        /// <returns></returns>
        [OperationContract]
        public string TravelReimbursementAdd(T_OA_TRAVELREIMBURSEMENT TravelReimbursement, List<T_OA_REIMBURSEMENTDETAIL> portDetail)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                string returnStr = "";
                if (!this.IsExistTravelReimbursementBySportid(TravelReimbursement.CLAIMSWERE, TravelReimbursement.T_OA_BUSINESSTRIP.BUSINESSTRIPID))
                {
                    if (!this.IsExistTravelReimbursement(TravelReimbursement.CLAIMSWERE, TravelReimbursement.TRAVELREIMBURSEMENTID))
                    {
                        if (!TrBll.TravelReimbursementAdd(TravelReimbursement, portDetail))
                        {
                            returnStr = "添加数据失败";
                        }
                    }
                    else
                    {
                        returnStr = "出差报销已经存在,请勿重复添加";
                    }
                }
                else
                {
                    returnStr = "出差报销已经存在,请勿重复添加";
                }
                return returnStr;
            }
        }

        [OperationContract]
        public string TravelReimbursementAddSimple(T_OA_TRAVELREIMBURSEMENT TravelReimbursement, List<T_OA_REIMBURSEMENTDETAIL> portDetail,string busnid)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                string returnStr = "";
                if (!TrBll.TravelReimbursementAdd(TravelReimbursement, portDetail))
                {
                    returnStr = "添加数据失败";
                }
                else
                {
                    TrBll.DeleteTheSameTravelreimbursement(busnid);
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 通过出差申请ID检测是否已存在自动生成出差报销
        /// </summary>
        /// <param name="businesstripId">出差申请ID</param>
        /// <returns>true为存在，false为不存在</returns>
        [OperationContract]
        public bool CheckTravelReimbursementByBusinesstrip(string businesstripId)
        {
            using (TravelReimbursementBLL trBll = new TravelReimbursementBLL())
            {
                return trBll.CheckTravelReimbursementByBusinesstrip(businesstripId);
            }
        }

        /// <summary>
        /// 获取所有的报销信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_TRAVELREIMBURSEMENT> GetTravelReimbursementRoomInfos()
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                List<T_OA_TRAVELREIMBURSEMENT> TravelReimbursementRoomList = TrBll.GetTravelReimbursementRooms();
                if (TravelReimbursementRoomList == null)
                {
                    return null;
                }
                else
                {
                    return TravelReimbursementRoomList;
                }
            }
        }
        #endregion

        #region 查询报销明细
        [OperationContract]
        public List<T_OA_REIMBURSEMENTDETAIL> GetTravelReimbursementDetail(string detailId)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                List<T_OA_REIMBURSEMENTDETAIL> details = TrBll.GetTravelReimbursementDetail(detailId);
                return details;
            }
        }
        #endregion

        #region 查询用户报销记录
        /// <summary>
        /// 查询用户报销记录
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="searchObj"></param>
        /// <param name="formIDList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_TravelReimbursement> GetTravelReimbursementListByUserId(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (TravelReimbursementBLL TrBll = new TravelReimbursementBLL())
            {
                List<V_TravelReimbursement> ArchivesList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())//获取用户的提交报销信息
                {
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ArchivesList = TrBll.GetTravelReimbursementInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        ArchivesList = TrBll.GetTravelReimbursementInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    return ArchivesList != null ? ArchivesList.ToList() : null;
                }
                else                    //通过工作流获取用户要审批的报销信息
                {
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "T_OA_TRAVELREIMBURSEMENT", loginUserInfo.companyID, loginUserInfo.userID);
                    if (flowList == null)
                    {
                        return null;
                    }
                    List<V_FlowAPP> flowAppList = new List<V_FlowAPP>();
                    for (int i = 0; i < flowList.Length; i++)
                    {
                        V_FlowAPP App = new V_FlowAPP();
                        App.Guid = flowList[i].FLOWRECORDDETAILID;
                        App.FormID = flowList[i].FLOW_FLOWRECORDMASTER_T.FORMID;
                        App.EditUserID = flowList[i].EDITUSERID;
                        App.EditUserName = flowList[i].EDITUSERNAME;
                        flowAppList.Add(App);
                    }
                    checkState = ((int)CheckStates.Approving).ToString();
                    ArchivesList = TrBll.GetTravelReimbursementInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                }
                return ArchivesList != null ? ArchivesList.ToList() : null;
            }
        }
        #endregion
    }
}