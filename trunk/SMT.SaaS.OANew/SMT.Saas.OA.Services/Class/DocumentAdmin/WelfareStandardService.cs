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
//using SMT.SaaS.OA.Services.FlowService;
using SMT.SaaS.BLLCommonServices.FlowWFService;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOADocumentAdmin
    {

        #region 福利标准管理
        [OperationContract]//获取所有的申请信息
        public List<T_OA_WELFAREMASERT> GetWelfareStandardInfo()
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                IQueryable<T_OA_WELFAREMASERT> WelfareStandardList = bal.GetWelfareStandard();
                return WelfareStandardList == null ? null : WelfareStandardList.ToList();
            }
        }
        [OperationContract]//根据申请ID获取申请信息
        public T_OA_WELFAREMASERT GetWelfareById(string WelfareID)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                T_OA_WELFAREMASERT WelfareStandard = bal.GetWelfareById(WelfareID);
                return WelfareStandard == null ? null : WelfareStandard;
            }
        }
        [OperationContract]//根据岗位&岗位级别获取福利标准
        public List<T_OA_WELFAREMASERT> GetWelfareStandardById(string PostLevela, string PostLevelb, string PostId)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                List<T_OA_WELFAREMASERT> MyContract = bal.GetWelfareStandardById(PostLevela, PostLevelb, PostId);
                return MyContract == null ? null : MyContract;
            }
        }
        [OperationContract]//查询福利标准明细
        public List<T_OA_WELFAREDETAIL> GetBenefitsAdministrationDetails(string welfreRoId, string companyId, string welfareId)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                List<T_OA_WELFAREDETAIL> details = bal.GetBenefitsAdministrationDetails(welfreRoId, companyId, welfareId);
                return details;
            }
        }
        [OperationContract]//查询福利标准明细
        public List<T_OA_WELFAREDETAIL> GetBenefitsDetailsAdministration(string welfreRoId, string companyId, DateTime releaseTime, string checkState)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                List<T_OA_WELFAREDETAIL> details = bal.GetBenefitsDetailsAdministration(welfreRoId, companyId, releaseTime, checkState);
                return details;
            }
        }
        [OperationContract]
        public T_OA_WELFAREMASERT GetBenefitsAdministrationInfo(string welfreRoId, string companyId, string chckState, string welfareId)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                return bal.GetBenefitsAdministrationInfo(welfreRoId, companyId, chckState, welfareId);
            }
        }
        [OperationContract]//更新标准信息
        public string UpdateWelfareStandard(T_OA_WELFAREMASERT Welfare, List<T_OA_WELFAREDETAIL> WelfaredDetails)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                string result = "";
                if (!bal.UpdateWelfare(Welfare, WelfaredDetails))
                {
                    result = "修改数据失败！";
                }
                return result;
            }
        }
        [OperationContract]//删除申请信息
        public bool DeleteWelfareStandard(string[] WelfareStandardID)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                return bal.DeleteWelfare(WelfareStandardID);
            }
        }
        [OperationContract]//添加福利标准申请信息
        public string WelfareStandardAdd(T_OA_WELFAREMASERT WelfareStandard, List<T_OA_WELFAREDETAIL> WelfaredDetails)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                string returnStr = "";

                if (!bal.WelfareAdd(WelfareStandard, WelfaredDetails))
                {
                    returnStr = "添加数据失败";
                }
                return returnStr;
            }
        }

        #region 该福利信息是否能被查看
        /// <summary>
        /// 该福利信息是否能被查看
        /// </summary>
        /// <param name="ContractID"></param>
        /// <returns></returns>
        [OperationContract]
        public bool IsWelfareCanBrowser(string WelfareStandard)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                return bal.IsCanBrowser(WelfareStandard);
            }
        }

        #endregion

        #region 查询用户申请记录
        /// <summary>
        /// 查询用户申请记录(福利标准)
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="searchObj"></param>
        /// <param name="formIDList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_WelfareStandard> GetWelfareListByUserId(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                List<V_WelfareStandard> ArchivesList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交申请信息
                {
                    //List<V_WelfareStandard> ArchivesList = bal.GetWelfareInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ArchivesList = bal.GetWelfareInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        ArchivesList = bal.GetWelfareInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    return ArchivesList != null ? ArchivesList.ToList() : null;
                }
                else                    //通过工作流获取用户要审批的申请信息
                {
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "T_OA_WELFAREMASERT", loginUserInfo.companyID, loginUserInfo.userID);
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

                    ArchivesList = bal.GetWelfareInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                    if (ArchivesList == null)
                    {
                        return null;
                    }
                    else
                    {
                        return ArchivesList.ToList();
                    }
                }
            }
        }
        [OperationContract]
        //获取某一福利项目的所有信息
        public List<T_OA_WELFAREMASERT> GetWelfareInformation()
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                List<T_OA_WELFAREMASERT> TypeTemplateList = bal.GetWelfareByInformation();
                if (TypeTemplateList == null)
                {
                    return null;
                }
                else
                {
                    return TypeTemplateList;
                }
            }
        }
        #endregion
        #endregion

        #region 福利明细管理
        [OperationContract]//获取所有的岗位      
        public List<T_OA_WELFAREDETAIL> GetWelfarePostId()
        {
            using (BenefitsAdministrationBLL bal = new BenefitsAdministrationBLL())
            {
                return bal.GetWelfarePostId();
            }
        }
        [OperationContract]//更新明细信息
        public string UpdateWelfarePaymentDetails(T_OA_WELFAREDISTRIBUTEDETAIL WelfarePaymentDetailsInfo)
        {
            using (WelfarePaymentDetailsBLL wdb = new WelfarePaymentDetailsBLL())
            {
                string result = "";
                if (!wdb.UpdateWelfarePaymentDetails(WelfarePaymentDetailsInfo))
                {
                    result = "修改数据失败！";
                }
                return result;
            }
        }
        [OperationContract]//获取所有的明细信息(用视图的服务器分页查询)
        public List<V_WelfareDetails> GetWelfarePaymentDetailsInfos(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (WelfarePaymentDetailsBLL wdb = new WelfarePaymentDetailsBLL())
            {
                List<V_WelfareDetails> WelfarePaymentDetails = wdb.GetWelfarePaymentDetails(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null);
                if (WelfarePaymentDetails == null)
                {
                    return null;
                }
                else
                {
                    return WelfarePaymentDetails.ToList();
                }
            }
        }
        [OperationContract]//获取所有的明细信息(不用视图的服务器分页查询)
        public List<T_OA_WELFAREDISTRIBUTEDETAIL> GetWelfarePaymentDetailsInquiry(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (WelfarePaymentDetailsBLL wdb = new WelfarePaymentDetailsBLL())
            {
                IQueryable<T_OA_WELFAREDISTRIBUTEDETAIL> WelfarePaymentDetails = wdb.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, "T_OA_WELFAREDISTRIBUTEDETAIL");
                if (WelfarePaymentDetails == null)
                {
                    return null;
                }
                else
                {
                    return WelfarePaymentDetails.ToList();
                }
            }
        }
        [OperationContract]
        public List<T_OA_WELFAREDISTRIBUTEDETAIL> GetByIdWelfarePaymentDetails(string distributeMasterId)
        {
            using (WelfarePaymentDetailsBLL wdb = new WelfarePaymentDetailsBLL())
            {
                List<T_OA_WELFAREDISTRIBUTEDETAIL> WelfarePaymentDetails = wdb.GetByIdWelfarePaymentDetails(distributeMasterId);
                if (WelfarePaymentDetails == null)
                {
                    return null;
                }
                else
                {
                    return WelfarePaymentDetails;
                }
            }
        }
        [OperationContract]//根据福利项目ID查询出所有的公司
        public List<T_OA_WELFAREMASERT> GetWelfareByWelfareproID(string welfareproID, string checkState)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                return wpbll.GetWelfareByWelfareproID(welfareproID, checkState);
            }
        }
        [OperationContract]//根据公司Id查询福利标准信息
        public T_OA_WELFAREMASERT GetWelfareByCompanyId(string companyId, string welfreRoId, DateTime releaseTime, string checkState, string welfareId)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                return wpbll.GetWelfareByCompanyId(companyId, welfreRoId, releaseTime, checkState, welfareId);
            }
        }
        [OperationContract]//删除福利发放明细
        public bool DeleteWelfarePaymentDetail(string[] welfarePaymentDetailId)
        {
            using (WelfarePaymentDetailsBLL wdb = new WelfarePaymentDetailsBLL())
            {
                return wdb.DeleteWelfarePaymentDetail(welfarePaymentDetailId);
            }
        }
        #endregion

        #region 福利发放管理
        [OperationContract]//获取所有的申请信息
        public List<T_OA_WELFAREDISTRIBUTEMASTER> GetWelfareProvisionInfo()
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                IQueryable<T_OA_WELFAREDISTRIBUTEMASTER> WelfareStandardList = wpbll.GetWelfareProvisionStandard();
                return WelfareStandardList == null ? null : WelfareStandardList.ToList();
            }
        }
        [OperationContract]//根据申请ID获取申请信息
        public T_OA_WELFAREDISTRIBUTEMASTER GetProvisionById(string WelfareID)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                T_OA_WELFAREDISTRIBUTEMASTER WelfareStandard = wpbll.GetWelfareProvisionById(WelfareID);
                return WelfareStandard == null ? null : WelfareStandard;
            }
        }
        [OperationContract]//根据福利标准申请ID关联发放申请ID
        public T_OA_WELFAREMASERT GetWelfareMasert(string welfareId)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                T_OA_WELFAREMASERT MyContract = wpbll.GetWelfareMasert(welfareId);
                return MyContract;
            }
        }
        [OperationContract]//根据条件查询福利标准申请信息
        private bool IsExistWelfareProvision(string WelfareProvision, string WelfareStandardID)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                return wpbll.IsExistWelfareProvision(WelfareProvision, WelfareStandardID);
            }
        }
        [OperationContract]//更新申请信息
        public string UpdateWelfareProvision(T_OA_WELFAREDISTRIBUTEMASTER WelfareStandardInfo)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                string result = "";
                if (!wpbll.UpdateWelfareProvision(WelfareStandardInfo))
                {
                    result = "修改数据失败！";
                }
                return result;
            }
        }
        [OperationContract]//删除申请信息
        public bool DeleteWelfareProvision(string[] WelfareStandardID)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                return wpbll.DeleteWelfareProvision(WelfareStandardID);
            }
        }
        [OperationContract]//添加福利发放申请信息
        public string WelfareProvisionAdd(T_OA_WELFAREDISTRIBUTEMASTER WelfareProvision, List<T_OA_WELFAREDISTRIBUTEDETAIL> WelfareDetails)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                string returnStr = "";
                if (!this.IsExistWelfareProvision(WelfareProvision.WELFAREDISTRIBUTETITLE, WelfareProvision.CONTENT))
                {
                    if (!wpbll.WelfareProvisionAdd(WelfareProvision, WelfareDetails))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                return returnStr;
            }
        }
        [OperationContract]//添加福利发放撤销记录
        public string WelfarePaymentWithdrawalAdd(T_OA_WELFAREDISTRIBUTEUNDO WelfarePaymentWithdrawalView)
        {
            using (WelfarePaymentWithdrawalBLL wpwl = new WelfarePaymentWithdrawalBLL())
            {
                string returnStr = "";
                if (!wpwl.IsExistWelfarePaymentWithdrawal(WelfarePaymentWithdrawalView.WELFAREDISTRIBUTEUNDOID,
                    WelfarePaymentWithdrawalView.T_OA_WELFAREDISTRIBUTEMASTER.WELFAREDISTRIBUTEMASTERID))
                {
                    if (!wpwl.WelfarePaymentWithdrawalAdd(WelfarePaymentWithdrawalView))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                return returnStr;
            }
        }
        [OperationContract]//更新申请信息
        public string UpdateWelfarePaymentWithdrawal(T_OA_WELFAREDISTRIBUTEUNDO WelfarePaymentWithdrawalView)
        {
            using (WelfarePaymentWithdrawalBLL wpwl = new WelfarePaymentWithdrawalBLL())
            {
                string result = "";
                if (!wpwl.UpdateWelfarePaymentWithdrawal(WelfarePaymentWithdrawalView))
                {
                    result = "修改数据失败！";
                }
                return result;
            }
        }
        [OperationContract]//根据申请ID获取撤销记录
        public T_OA_WELFAREDISTRIBUTEUNDO GetWelfarePaymentWithdrawalById(string beingWithdrawnId)
        {
            using (WelfarePaymentWithdrawalBLL wpwl = new WelfarePaymentWithdrawalBLL())
            {
                T_OA_WELFAREDISTRIBUTEUNDO WelfarePaymentWithdrawal = wpwl.GetWelfarePaymentWithdrawalById(beingWithdrawnId);
                return WelfarePaymentWithdrawal == null ? null : WelfarePaymentWithdrawal;
            }
        }

        #region 查询用户申请记录
        /// <summary>
        /// 查询用户申请记录(福利发放)
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="searchObj"></param>
        /// <param name="formIDList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_WelfareProvision> GetWelfareProvisionListByUserId(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                List<V_WelfareProvision> ArchivesList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交申请信息
                {
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ArchivesList = wpbll.GetWelfareProvisionInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        ArchivesList = wpbll.GetWelfareProvisionInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    return ArchivesList != null ? ArchivesList.ToList() : null;
                }
                else                    //通过工作流获取用户要审批的申请信息
                {
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "T_OA_WELFAREDISTRIBUTEMASTER", loginUserInfo.companyID, loginUserInfo.userID);
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

                    ArchivesList = wpbll.GetWelfareProvisionInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                    if (ArchivesList == null)
                    {
                        return null;
                    }
                    else
                    {
                        return ArchivesList.ToList();
                    }
                }
            }
        }
        #endregion

        [OperationContract]//查询发放记录
        public List<V_WelfareProvision> GetWelfarePSelectRecord(int pageIndex, int pageSize, string sort, object[] paras, ref int pageCount, string checkState)
        {
            using (WelfareProvisionBLL wpbll = new WelfareProvisionBLL())
            {
                List<V_WelfareProvision> WelfarePSelectRecordList = wpbll.GetWelfarePSelectRecord(pageIndex, pageSize, sort, paras, ref  pageCount, checkState);
                if (WelfarePSelectRecordList == null)
                {
                    return null;
                }
                else
                {
                    return WelfarePSelectRecordList.ToList();
                }
            }
        }

        #region 查询发放撤销记录
        /// <summary>
        /// 查询发放撤销记录(福利发放撤销)
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="searchObj"></param>
        /// <param name="formIDList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_WelfarePaymentWithdrawal> GetWelfarePaymentWithdrawal(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (WelfarePaymentWithdrawalBLL wpwl = new WelfarePaymentWithdrawalBLL())
            {
                List<V_WelfarePaymentWithdrawal> ArchivesList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交申请信息
                {
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ArchivesList = wpwl.GetWelfarePaymentWithdrawal(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        ArchivesList = wpwl.GetWelfarePaymentWithdrawal(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    return ArchivesList != null ? ArchivesList.ToList() : null;
                }
                else                    //通过工作流获取用户要审批的申请信息
                {
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "T_OA_WELFAREDISTRIBUTEUNDO", loginUserInfo.companyID, loginUserInfo.userID);
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

                    ArchivesList = wpwl.GetWelfarePaymentWithdrawal(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                    if (ArchivesList == null)
                    {
                        return null;
                    }
                    else
                    {
                        return ArchivesList.ToList();
                    }
                }
            }
        }
        #endregion

        [OperationContract]//删除撤销信息
        public bool DeletePaymentWithdrawal(string[] paymentWithdrawalID)
        {
            using (WelfarePaymentWithdrawalBLL wpwl = new WelfarePaymentWithdrawalBLL())
            {
                return wpwl.DeletePaymentWithdrawal(paymentWithdrawalID);
            }
        }
        #endregion
    }
}
