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
    public partial class SmtOADocumentAdmin
    {

        #region 合同类型管理
        [OperationContract]//获取所有信息
        public List<T_OA_CONTRACTTYPE> GetContractTypeInfo(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                List<T_OA_CONTRACTTYPE> TypeList = cdb.GetInquiryContractType(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);
                if (TypeList == null)
                {
                    return null;
                }
                else
                {
                    return TypeList.ToList();
                }
            }
        }
        [OperationContract]//根据类型ID获取类型信息
        public T_OA_CONTRACTTYPE GetContractTypeById(string contractID)
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                T_OA_CONTRACTTYPE MyContract = cdb.GetContractTypeById(contractID);
                return MyContract == null ? null : MyContract;
            }
        }
        [OperationContract]//根据条件查询合同类型信息
        private bool IsExistContractType(string ContractType, string ContractTypeID)
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                return cdb.IsExistContractType(ContractType, ContractTypeID);
            }
        }
        [OperationContract]//更新类型信息
        public string UpdateContraType(T_OA_CONTRACTTYPE contraTypeInfo)
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                string returnStr = "";
                if (!this.IsExistContractType(contraTypeInfo.CONTRACTTYPE, contraTypeInfo.CONTRACTLEVEL))
                {
                    if (!cdb.UpdateContraType(contraTypeInfo))
                    {
                        returnStr = "更新数据失败！";
                    }
                }
                else
                {
                    returnStr = "合同类型信息已经存在,更新数据失败";
                }
                return returnStr;
            }
        }
        [OperationContract]//删除类型信息
        public bool DeleteContraType(string[] contraTypeID)
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                return cdb.DeleteContraType(contraTypeID);
            }
        }
        [OperationContract]//添加合同类型信息
        public string ContractTypeAdd(T_OA_CONTRACTTYPE contractType)
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                string returnStr = "";
                if (!this.IsExistContractType(contractType.CONTRACTTYPE, contractType.CONTRACTLEVEL))
                {

                    if (!cdb.ContractTypeAdd(contractType))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "合同类型信息已经存在,添加数据失败";
                }
                return returnStr;
            }
        }
        [OperationContract]
        public List<V_ContractType> GetContractTypeRoomInfosListBySearch(string StrContractTypeName, string StrID, string strContractLevel)
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                List<V_ContractType> ContractTypeTypeList = cdb.GetContractTypeRoomInfosListBySearch(StrContractTypeName, StrID, strContractLevel);
                if (ContractTypeTypeList == null)
                {
                    return null;
                }
                else
                {
                    return ContractTypeTypeList.ToList();
                }
            }
        }
        [OperationContract]//获取某合同类型的所有模板名称
        public List<T_OA_CONTRACTTYPE> GetContractTypeTemplateNameInfosByContractType(string StrContractType)
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                List<T_OA_CONTRACTTYPE> TypeTemplateList = cdb.GetContractTemplateNameInfosByContractType(StrContractType);
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
        [OperationContract]//获取所有类型名
        public List<T_OA_CONTRACTTYPE> GetContractTypeNameInfosToCombox()
        {
            using (ContractTypeDefinitionBLL cdb = new ContractTypeDefinitionBLL())
            {
                return cdb.GetContractTypeNameInfos();
            }
        }
        #endregion

        #region 合同模板管理
        [OperationContract]//获取所有的模板信息
        public List<V_ContractTemplate> GetContractTemplateInfo(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (ContractTemplateBLL ctbll = new ContractTemplateBLL())
            {
                List<V_ContractTemplate> TypeTemplateList = ctbll.GetContractTemplate(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, loginUserInfo.userID);
                if (TypeTemplateList == null)
                {
                    return null;
                }
                else
                {
                    return TypeTemplateList.ToList();
                }
            }
        }
        [OperationContract]//根据模板ID获取模板信息
        public T_OA_CONTRACTTEMPLATE GetContractTemplateById(string contractID)
        {
            using (ContractTemplateBLL ctbll = new ContractTemplateBLL())
            {
                T_OA_CONTRACTTEMPLATE MyContract = ctbll.GetContractTemplateById(contractID);
                return MyContract == null ? null : MyContract;
            }
        }
        [OperationContract]//更新模板信息
        public string UpdateContraTemplate(T_OA_CONTRACTTEMPLATE contraTemplateInfo)
        {
            using (ContractTemplateBLL ctbll = new ContractTemplateBLL())
            {
                string returnStr = "";
                if (!ctbll.IsExistContractTemplate(contraTemplateInfo.CONTRACTTEMPLATENAME,
                    contraTemplateInfo.CREATECOMPANYID, contraTemplateInfo.CONTRACTLEVEL, contraTemplateInfo.CONTRACTTITLE))
                {
                    if (!ctbll.UpdateContraTemplate(contraTemplateInfo))
                    {
                        returnStr = "更新数据失败！";
                    }
                }
                else
                {
                    returnStr = "合同模板信息已经存在,更新数据失败";
                }
                return returnStr;
            }
        }
        [OperationContract]//删除模板信息
        public bool DeleteContraTemplate(string[] contraTemplateID)
        {
            using (ContractTemplateBLL ctbll = new ContractTemplateBLL())
            {
                return ctbll.DeleteContraTemplate(contraTemplateID);
            }
        }
        [OperationContract]//添加合同模板信息
        public string ContractTemplateAdd(T_OA_CONTRACTTEMPLATE contractTemplate)
        {
            using (ContractTemplateBLL ctbll = new ContractTemplateBLL())
            {
                string returnStr = "";
                if (!ctbll.IsExistContractTemplate(contractTemplate.CONTRACTTEMPLATENAME,
                    contractTemplate.CREATECOMPANYID, contractTemplate.CONTRACTLEVEL, contractTemplate.CONTRACTTITLE))
                {
                    if (!ctbll.ContractTemplateAdd(contractTemplate))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "合同模板信息已经存在,添加数据失败";
                }
                return returnStr;
            }
        }
        [OperationContract]//获取某一合同类型的所有模板名称
        public List<T_OA_CONTRACTTEMPLATE> GetContractTypeTemplateNameByContractTypeInfos(string StrContractType)
        {
            using (ContractTemplateBLL ctbll = new ContractTemplateBLL())
            {
                List<T_OA_CONTRACTTEMPLATE> TypeTemplateList = ctbll.GetContractTemplateInfosByContractType(StrContractType);
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

        #region 合同申请管理
        [OperationContract]//获取所有的申请信息
        public List<T_OA_CONTRACTAPP> GetContractApprovalInfo()
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                IQueryable<T_OA_CONTRACTAPP> ContracApprovalList = cab.GetContractApplications();
                return ContracApprovalList == null ? null : ContracApprovalList.ToList();
            }
        }
        [OperationContract]//根据ID获取合同申请
        public V_ContractApplications GetContractApprovalById(string contractApprovalID)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                V_ContractApplications ContracApproval = cab.GetContractApprovalById(contractApprovalID);
                return ContracApproval == null ? null : ContracApproval;
            }
        }
        [OperationContract]//更新申请信息
        public string UpdateContraApproval(T_OA_CONTRACTAPP contraApprovalInfo)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                string returnStr = "";
                if (!cab.UpdateContraApproval(contraApprovalInfo))
                {
                    returnStr = "更新数据失败！";
                }
                return returnStr;
            }
        }
        [OperationContract]//删除申请信息
        public bool DeleteContraApproval(string[] contraApprovalID)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                return cab.DeleteContraApproval(contraApprovalID);
            }
        }
        [OperationContract]//添加合同申请信息
        public string ContractApprovalAdd(T_OA_CONTRACTAPP ContractApproval)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                string returnStr = "";
                if (!cab.IsExistContractApproval(ContractApproval.CONTRACTCODE, ContractApproval.CONTRACTTITLE,
                    ContractApproval.CONTRACTTYPEID, ContractApproval.CONTRACTLEVEL))
                {
                    if (!cab.ContractApprovalAdd(ContractApproval))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "合同申请信息已经存在,添加数据失败";
                }
                return returnStr;
            }
        }
        [OperationContract]//添加合同打印
        public string ContractPrintingAdd(T_OA_CONTRACTPRINT ContractPrintingInfo)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                string returnStr = "";

                if (!cab.ContractPrintingAdd(ContractPrintingInfo))
                {
                    returnStr = "添加数据失败";
                }
                return returnStr;
            }
        }
        [OperationContract]//(更新合同打印)上传附件
        public string UpdateContractPrinting(T_OA_CONTRACTPRINT contractPrinting)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                string result = "";
                if (!cab.UpdateContractPrinting(contractPrinting))
                {
                    result = "更新或上传数据失败！";
                }
                return result;
            }
        }
        [OperationContract]//查询合同打印
        public List<V_ContractPrint> GetInquiryContractPrintingRecord(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                List<V_ContractPrint> ContractPrintList = cab.InquiryContractPrintingRecord(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
                if (ContractPrintList == null)
                {
                    return null;
                }
                else
                {
                    return ContractPrintList.ToList();
                }
            }
        }
        [OperationContract]//查询已打印和上传的合同
        public List<V_ContractPrint> GetInquiryContractPrintingRecordInfo(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                List<V_ContractPrint> ContractPrintList = cab.GetInquiryContractPrintingRecordInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
                if (ContractPrintList == null)
                {
                    return null;
                }
                else
                {
                    return ContractPrintList.ToList();
                }
            }
        }
        [OperationContract]//根据ID获取合同申请(打印)
        public V_ContractPrint GetContractPrintingById(string contractPrintinglID)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                V_ContractPrint ContractPrinting = cab.GetContractPrintingById(contractPrintinglID);
                return ContractPrinting == null ? null : ContractPrinting;
            }
        }
        [OperationContract]//添加合同查看申请
        public string ContractViewapplicationsAdd(T_OA_CONTRACTVIEW ContractApplicationView)
        {
            using (ContractViewapplicationsBLL cvb = new ContractViewapplicationsBLL())
            {
                string returnStr = "";

                if (!cvb.ContractViewapplicationsAdd(ContractApplicationView))
                {
                    returnStr = "添加数据失败";
                }
                return returnStr;
            }
        }
        [OperationContract]//(更新合同查看申请)提交审核
        public string UpdateContractView(T_OA_CONTRACTVIEW contractView)
        {
            using (ContractViewapplicationsBLL cvb = new ContractViewapplicationsBLL())
            {
                string result = "";
                if (!cvb.UpdateContractView(contractView))
                {
                    result = "更新数据失败！";
                }
                return result;
            }
        }
        [OperationContract]//删除申请信息
        public bool DeleteViewapplications(string[] viewapplicationsID)
        {
            using (ContractViewapplicationsBLL cvb = new ContractViewapplicationsBLL())
            {
                return cvb.DeleteViewapplications(viewapplicationsID);
            }
        }
        [OperationContract]//查询合同查看申请
        public List<V_ContractView> GetInquiryViewContractApplication(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (ContractViewapplicationsBLL cvb = new ContractViewapplicationsBLL())
            {
                List<V_ContractView> ContractViewList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交申请信息
                {
                    //List<V_ContractView> ContractViewList = cab.GetInquiryViewContractApplication(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState);
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ContractViewList = cvb.GetInquiryViewContractApplication(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        ContractViewList = cvb.GetInquiryViewContractApplication(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    return ContractViewList != null ? ContractViewList.ToList() : null;
                }
                else                    //通过工作流获取用户要审批的申请信息
                {
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "T_OA_CONTRACTVIEW", loginUserInfo.companyID, loginUserInfo.userID);
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

                    ContractViewList = cvb.GetInquiryViewContractApplication(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                    if (ContractViewList == null)
                    {
                        return null;
                    }
                    else
                    {
                        return ContractViewList.ToList();
                    }
                }
            }
        }
        [OperationContract]//根据标题、ID获取合同的申请信息
        public List<V_ContractApplications> GetContractApprovalRoomInfosListBySearch(string StrContractApprovalName, string StrID, string strContractLevel, string strContractLogo)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                List<V_ContractApplications> ContractApprovalList = cab.GetApprovalRoomInfosListBySearch(StrContractApprovalName, StrID, strContractLevel, strContractLogo);
                if (ContractApprovalList == null)
                {
                    return null;
                }
                else
                {
                    return ContractApprovalList.ToList();
                }
            }
        }
        [OperationContract]//合同是否能被查看
        public bool IsContractCanBrowser(string ContractID)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                return cab.IsContractCanBrowser(ContractID);
            }
        }
        #endregion

        #region 根据ID获取合同查看申请
        [OperationContract]
        public T_OA_CONTRACTVIEW GetContractViewById(string contractViewID)
        {
            using (ContractViewapplicationsBLL cvb = new ContractViewapplicationsBLL())
            {
                T_OA_CONTRACTVIEW ContractView = cvb.GetContractViewById(contractViewID);
                return ContractView == null ? null : ContractView;
            }
        }
        #endregion

        #region 查询用户申请记录
        /// <summary>
        /// 查询用户申请记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_ContractApplications> GetApprovalListByUserId(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                List<V_ContractApplications> ArchivesList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交申请信息
                {
                    //List<V_ContractApplications> ArchivesList = cab.GetApprovalInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState);
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ArchivesList = cab.GetApprovalInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        ArchivesList = cab.GetApprovalInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    return ArchivesList != null ? ArchivesList.ToList() : null;
                }
                else                    //通过工作流获取用户要审批的申请信息
                {
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "T_OA_CONTRACTAPP", loginUserInfo.companyID, loginUserInfo.userID);
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

                    ArchivesList = cab.GetApprovalInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
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

        #region 查询用户申请记录(合同打印)
        /// <summary>
        /// 查询用户申请记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_ContractApplications> GetApprovalListById(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (ContractApprovalBLL cab = new ContractApprovalBLL())
            {
                List<V_ContractApplications> ArchivesList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交申请信息
                {
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ArchivesList = cab.GetApprovalInfoPrinting(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState);
                    }
                    else
                    {
                        ArchivesList = cab.GetApprovalInfoPrinting(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "");
                    }
                    return ArchivesList != null ? ArchivesList.ToList() : null;
                }
                else                    //通过工作流获取用户要审批的申请信息
                {
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "T_OA_CONTRACTAPP", loginUserInfo.companyID, loginUserInfo.userID);
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

                    ArchivesList = cab.GetApprovalInfoPrinting(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState);
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
    }
}
