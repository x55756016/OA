using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.SaaS.OA.DAL.Views;
using System.Web.Security;
using System.Web;
using System.Security.Principal;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.Services
{

    //public partial class SmtOACommonOffice:IUserAuthenticate
    public partial class SmtOACommonOffice 
    {

        #region 初始化变量
        //公文类型
        
        //公文级别
        
        //公文缓急
        

        //公文类型模板
        
        //公司发文
        

        //文档发布
        
        //工作流
        
        #endregion

        #region 公文类型

        [OperationContract]
        //增
        public string DocTypeAdd(T_OA_SENDDOCTYPE obj)
        {

            using (BumfDocTypeManagementBll TypeBll = new BumfDocTypeManagementBll())
            {
                return TypeBll.AddDocTypeInfo(obj);
            }
        }
         
        

        [OperationContract]
        private bool IsExistDocTypeInfo(string StrDocType)
        {
            using (BumfDocTypeManagementBll TypeBll = new BumfDocTypeManagementBll())
            {
                return TypeBll.GetDocTypeInfoByAdd(StrDocType);
            }           

        }       

        [OperationContract]
        //删
        public string DocTypeBatchDel(string[] StrDocTypeIDs)
        {
            using (BumfDocTypeManagementBll TypeBll = new BumfDocTypeManagementBll())
            {
                return TypeBll.BatchDeleteDocTypeInfos(StrDocTypeIDs);
            }
            
        }

        [OperationContract]
        //改
        public void DocTypeInfoUpdate(T_OA_SENDDOCTYPE obj)
        {
            using (BumfDocTypeManagementBll TypeBll = new BumfDocTypeManagementBll())
            {
                TypeBll.UpdateDocTypeInfo(obj);
            }
            
        }

        [OperationContract]
        //查单
        public T_OA_SENDDOCTYPE GetDocTypeSingleInfoById(string StrDocTypeId)
        {
            using (BumfDocTypeManagementBll TypeBll = new BumfDocTypeManagementBll())
            {
                return TypeBll.GetDocTypeInfoById(StrDocTypeId);
            }
            
        }
        [OperationContract]
        //所有文档类型
        public List<T_OA_SENDDOCTYPE> GetDocTypeInfos()
        {
            using (BumfDocTypeManagementBll TypeBll = new BumfDocTypeManagementBll())
            {
                List<T_OA_SENDDOCTYPE> DocTypeInfosList = TypeBll.GetDocTypeInfos();
                if (DocTypeInfosList == null)
                {
                    return null;
                }
                else
                {
                    return DocTypeInfosList;
                }
            }
            
            
        }

        [OperationContract]
        //函数有重载 以后考虑公司ID 部门ID时再调用
        public List<T_OA_SENDDOCTYPE> GetDocTypeInfosListBySearch(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (BumfDocTypeManagementBll TypeBll = new BumfDocTypeManagementBll())
            {
                return TypeBll.GetDocTypeInfosListByTypeCompanyDepartmentSearch(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

            }
           
            
        }
        

        [OperationContract]
        //获取公文类型名称        
        public List<string> GetDocTypeNameInfosToCombox()
        {
            using (BumfDocTypeManagementBll TypeBll = new BumfDocTypeManagementBll())
            {
                List<string> DocTypeList = TypeBll.GetDocTypeNameInfos();
                return DocTypeList != null ? DocTypeList.ToList() : null;
            }
            
            
        }

        

        
       
        #endregion

        #region 公文缓急
        [OperationContract]
        //增
        public string DocPriorityAdd(T_OA_PRIORITIES obj)
        {
            BumfPrioritiesManagementBll PriorityBll = new BumfPrioritiesManagementBll();
            return  PriorityBll.AddPrioritiesInfo(obj);
            
        }



        [OperationContract]
        private bool IsExistDocPriorityInfo(string StrDocPriority)
        {
            BumfPrioritiesManagementBll PriorityBll = new BumfPrioritiesManagementBll();
            return PriorityBll.GetPrioritiesInfoByAdd(StrDocPriority);

        }

        [OperationContract]
        //删
        public bool DocPriorityBatchDel(string[] StrDocPriorityIDs)
        {
            BumfPrioritiesManagementBll PriorityBll = new BumfPrioritiesManagementBll();
            return PriorityBll.BatchDeletePrioritiesInfos(StrDocPriorityIDs);
        }

        [OperationContract]
        //改
        public void DocPriorityInfoUpdate(T_OA_PRIORITIES obj)
        {
            BumfPrioritiesManagementBll PriorityBll = new BumfPrioritiesManagementBll();
            PriorityBll.UpdatePrioritiesInfo(obj);
        }

        [OperationContract]
        //查单
        public T_OA_PRIORITIES GetDocPrioritySingleInfoById(string StrDocPriorityId)
        {
            BumfPrioritiesManagementBll PriorityBll = new BumfPrioritiesManagementBll();
            return PriorityBll.GetPrioritiesInfoById(StrDocPriorityId);
        }

        [OperationContract]
        //所有文档缓急信息
        public List<T_OA_PRIORITIES> GetDocPrioritiesInfos()
        {
            BumfPrioritiesManagementBll PriorityBll = new BumfPrioritiesManagementBll();
            List<T_OA_PRIORITIES> DocPrioritiesInfosList = PriorityBll.GetPrioritiesInfos();
            if (DocPrioritiesInfosList == null)
            {
                return null;
            }
            else
            {
                return DocPrioritiesInfosList;
            }

        }

        
        [OperationContract]
        public List<T_OA_PRIORITIES> GetDocPriorityInfosListBySearch(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            BumfPrioritiesManagementBll PriorityBll = new BumfPrioritiesManagementBll();
            IQueryable<T_OA_PRIORITIES> GradeList = PriorityBll.GetPrioritiesInfosListByTypeCompanyDepartmentSearch(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

            return GradeList != null ? GradeList.ToList() : null;
        }

        [OperationContract]
        //获取缓急类型名称        
        public List<string> GetProritityNameInfosToCombox()
        {
            BumfPrioritiesManagementBll PriorityBll = new BumfPrioritiesManagementBll();
            List<string> listPriority= PriorityBll.GetProritityNameInfos();
            if (listPriority != null)
            {
               return listPriority.ToList();
            }
            else
            {
                return null;
            }
        }
       
        #endregion

        #region 公文密级

        [OperationContract]
        //增
        public string GradeAdd(T_OA_GRADED obj)
        {
            BumfGradeManagementBll GradeBll = new BumfGradeManagementBll();
            return GradeBll.AddGradeInfo (obj);;
        }



        [OperationContract]
        private bool IsExistGradeInfo(string StrGrade)
        {
            BumfGradeManagementBll GradeBll = new BumfGradeManagementBll();
            return GradeBll.GetGradeInfoByAdd(StrGrade);

        }

        [OperationContract]
        //删
        public bool GradeBatchDel(string[] StrGradeIDs)
        {
            BumfGradeManagementBll GradeBll = new BumfGradeManagementBll();
            return GradeBll.BatchDeleteGradeInfos(StrGradeIDs);
        }

        [OperationContract]
        //改
        public void GradeInfoUpdate(T_OA_GRADED obj)
        {
            BumfGradeManagementBll GradeBll = new BumfGradeManagementBll();
            GradeBll.UpdateGradeInfo(obj);
        }

        [OperationContract]
        //查单
        public T_OA_GRADED GetGradeSingleInfoById(string StrGradeId)
        {
            BumfGradeManagementBll GradeBll = new BumfGradeManagementBll();
            return GradeBll.GetGradeInfoById(StrGradeId);
        }


        [OperationContract]
        //所有文档密级信息
        public List<T_OA_GRADED> GetGradeInfos()
        {
            BumfGradeManagementBll GradeBll = new BumfGradeManagementBll();
            List<T_OA_GRADED> GradeInfosList = GradeBll.GetGradeInfos();
            if (GradeInfosList == null)
            {
                return null;
            }
            else
            {
                return GradeInfosList;
            }

        }

                
        [OperationContract]
        public List<T_OA_GRADED> GetGradeInfosListBySearch(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            BumfGradeManagementBll GradeBll = new BumfGradeManagementBll();
            IQueryable<T_OA_GRADED> GradeList = GradeBll.GetGradeInfosListByTypeCompanyDepartmentSearch(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

            return GradeList != null ? GradeList.ToList() : null;
        }

        [OperationContract]
        //获取公文级别类型名称        
        public List<string> GetDocGradeNameInfosToCombox()
        {
            BumfGradeManagementBll GradeBll = new BumfGradeManagementBll();
            List<string> GradeList =  GradeBll.GetGradeNameInfos();
            return GradeList != null ? GradeList.ToList() : null;
        }


        #endregion

        #region 公文类型模板
        [OperationContract]
        //增
        public string DocTypeTemplateAdd(T_OA_SENDDOCTEMPLATE obj)
        {
            using (BumfDocTypeTemplateManagementBll DocTypeTemplateBll = new BumfDocTypeTemplateManagementBll())
            {
                return DocTypeTemplateBll.AddDocTypeTemplateInfo(obj);
            }
            
            
        }



        [OperationContract]
        private bool IsExistDocTypeTemplateInfo(string StrTitle, string StrType, string StrGrade, string StrProritity, string StrCompanyID, string StrDepartmentID, string StrPositionID)
        {
            using (BumfDocTypeTemplateManagementBll DocTypeTemplateBll = new BumfDocTypeTemplateManagementBll())
            {
                return DocTypeTemplateBll.GetDocTypeTemplateInfoByAdd(StrTitle, StrType, StrGrade, StrProritity, StrCompanyID, StrDepartmentID, StrPositionID);
            }
            
        }

        [OperationContract]
        //删
        public bool DocTypeTemplateBatchDel(string[] StrDocTypeTemplateIDs)
        {
            using (BumfDocTypeTemplateManagementBll DocTypeTemplateBll = new BumfDocTypeTemplateManagementBll())
            {
                return DocTypeTemplateBll.BatchDeleteDocTypeTemplateInfos(StrDocTypeTemplateIDs);
            }
            
        }

        /// <summary>
        /// 更新公文模板
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="StrResult">如果重复则返回重复信息</param>
        [OperationContract]
        //改
        public void DocTypeTemplateInfoUpdate(T_OA_SENDDOCTEMPLATE obj,ref string StrResult)
        {
            using (BumfDocTypeTemplateManagementBll DocTypeTemplateBll = new BumfDocTypeTemplateManagementBll())
            {
                DocTypeTemplateBll.UpdateDocTypeTemplateInfo(obj, ref StrResult);
            }
            
            
        }

        [OperationContract]
        //查单
        public T_OA_SENDDOCTEMPLATE GetDocTypeTemplateSingleInfoById(string StrDocTypeTemplateId)
        {
            using (BumfDocTypeTemplateManagementBll DocTypeTemplateBll = new BumfDocTypeTemplateManagementBll())
            {
                return DocTypeTemplateBll.GetDocTypeTemplateInfoById(StrDocTypeTemplateId);
            }
            
            
        }
        [OperationContract]
        //根据公文类型获取模板名
        public List<T_OA_SENDDOCTEMPLATE> GetDocTypeTemplateContentByDocType(string StrDocType)
        {
            using (BumfDocTypeTemplateManagementBll DocTypeTemplateBll = new BumfDocTypeTemplateManagementBll())
            {
                List<T_OA_SENDDOCTEMPLATE> DocTypeTemplateInfosList = DocTypeTemplateBll.GetDocTypeTemplateNameInfosByDocType(StrDocType);
                if (DocTypeTemplateInfosList == null)
                {
                    return null;
                }
                else
                {
                    return DocTypeTemplateInfosList;
                }
            }
            
        }
        [OperationContract]
        //所有文档类型
        public List<T_OA_SENDDOCTEMPLATE> GetDocTypeTemplateInfos()
        {
            using (BumfDocTypeTemplateManagementBll DocTypeTemplateBll = new BumfDocTypeTemplateManagementBll())
            {
                List<T_OA_SENDDOCTEMPLATE> DocTypeTemplateInfosList = DocTypeTemplateBll.GetDocTypeTemplateInfos();
                if (DocTypeTemplateInfosList == null)
                {
                    return null;
                }
                else
                {
                    return DocTypeTemplateInfosList;
                }
            }
            
            

        }

        //[OperationContract]
        ////函数有重载 以后考虑公司ID 部门ID时再调用
        //public List<T_OA_SENDDOCTEMPLATE> GetDocTypeTemplateInfosListBySearch(string StrDocTypeTemplate, DateTime DtStart, DateTime DtEnd,string TemplateName,string StrContent,string StrGrade,string StrPriority,string StrDocType)
        //{
        //    IQueryable<T_OA_SENDDOCTEMPLATE> DocTypeTemplateList = DocTypeTemplateBll.GetDocTypeTemplateInfosListByTypeCompanyDepartmentSearch(StrDocTypeTemplate, DtStart, DtEnd,TemplateName,StrContent,StrGrade,StrPriority,StrDocType);
        //    if (DocTypeTemplateList == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return DocTypeTemplateList.ToList();
        //    }
        //}
        [OperationContract]
        public List<T_OA_SENDDOCTEMPLATE> GetDocTypeTemplateInfosListBySearch(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (BumfDocTypeTemplateManagementBll DocTypeTemplateBll = new BumfDocTypeTemplateManagementBll())
            {
                IQueryable<T_OA_SENDDOCTEMPLATE> DocTypeTemplateList = DocTypeTemplateBll.GetDocTypeTemplateInfosListByTypeCompanyDepartmentSearch(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

                return DocTypeTemplateList != null ? DocTypeTemplateList.ToList() : null;
            }
            
        }
       
        #endregion

        #region 公司发文


        [OperationContract]
        //增
        public string SendDocAdd(T_OA_SENDDOC obj)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                return SendDocBll.AddSendDocInfo(obj);
            }
            
        }



        [OperationContract]
        private bool IsExistSendDocInfo(string StrTitle, string StrType, string StrGrade, string StrProritity,string SendDepart, string StrCompanyID, string StrDepartmentID, string StrPositionID)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                return SendDocBll.GetSendDocInfoByAdd(StrTitle, StrGrade, StrProritity, StrType, SendDepart, StrCompanyID, StrDepartmentID, StrPositionID);
            }

        }

        [OperationContract]
        //删
        public bool SendDocBatchDel(string[] StrSendDocIDs)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                return SendDocBll.BatchDeleteSendDocInfos(StrSendDocIDs);
            }
        }
        /// <summary>
        /// 公司发文修改  
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="StrResult">返回值 判断是否重复</param>
        [OperationContract]
        //改
        public void SendDocInfoUpdate(T_OA_SENDDOC obj,ref string StrResult)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                SendDocBll.UpdateSendDocInfo(obj, ref StrResult);
            }
        }

        [OperationContract]
        //查单
        public T_OA_SENDDOC GetSendDocSingleInfoById(string StrSendDocId)
        {
            BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll();
            
                return SendDocBll.GetSendDocInfoById(StrSendDocId);
            
        }
        [OperationContract]
        //所有发文信息
        public List<T_OA_SENDDOC> GetSendDocInfos()
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                List<T_OA_SENDDOC> SendDocInfosList = SendDocBll.GetSendDocInfos();
                if (SendDocInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SendDocInfosList;
                }
            }

        }

        [OperationContract]
        //获取已发布的文档信息
        public List<T_OA_SENDDOC> GetDistrbutedSendDocInfos(string StrTitle, string StrContent, DateTime DtStart, DateTime DtEnd, string StrGrade, string StrProritity, string StrDocType)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                List<T_OA_SENDDOC> SendDocInfosList = SendDocBll.GetDistributedSendDocInfos(StrTitle, StrContent, DtStart, DtEnd, StrGrade, StrProritity, StrDocType);
                if (SendDocInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SendDocInfosList;
                }
            }

        }

        [OperationContract]
        //获取已归档的文档信息
        public List<T_OA_SENDDOC> GetSavedSendDocInfos(string StrTitle,string StrContent,DateTime DtStart,DateTime DtEnd,string StrGrade,string StrProritity,string StrdocType)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                List<T_OA_SENDDOC> SendDocInfosList = SendDocBll.GetSavedSendDocInfos(StrTitle, StrContent, DtStart, DtEnd, StrGrade, StrProritity, StrdocType);
                if (SendDocInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SendDocInfosList;
                }
            }

        }



        [OperationContract]
        //函数有重载 以后考虑公司ID 部门ID时再调用
        public List<T_OA_SENDDOC> GetSendDocInfosListBySearch(string StrSendDoc, DateTime DtStart, DateTime DtEnd)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                List<T_OA_SENDDOC> SendDocList = SendDocBll.GetSendDocInfosListByTypeCompanyDepartmentSearch(StrSendDoc, DtStart, DtEnd);
                if (SendDocList == null)
                {
                    return null;
                }
                else
                {
                    return SendDocList.ToList();
                }
            }
        }

        
        /// <summary>
        /// 已发布的公文文档
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_SENDDOC> GetDistrbutedSendDoc(int inttake)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                return SendDocBll.GetDistrbutedInfos(inttake);
            }
        }

        [OperationContract]
        //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
        public List<V_BumfCompanySendDoc> GetSendDocInfosListByWorkFlow(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            try
            {
                using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
                {
                    List<V_BumfCompanySendDoc> CompanyDocList = null;
                    if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的公司文档信息
                    {
                        //List<V_ArchivesLending> CompanyDocList = archivesLendingBll.GetArchivesLendingInfo(userID, searchObj, null, checkState);
                        if (checkState != ((int)CheckStates.ALL).ToString())
                        {
                            CompanyDocList = SendDocBll.GetSendDocInfosListByTypeCompanyDepartmentSearch(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                        }
                        else
                        {
                            CompanyDocList = SendDocBll.GetSendDocInfosListByTypeCompanyDepartmentSearch(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                        }

                    }
                    else                    //通过工作流获取公司文档信息
                    {
                        ServiceClient workFlowWS = new ServiceClient();
                        V_BumfCompanySendDoc a = new V_BumfCompanySendDoc();
                        FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                        FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "T_OA_SENDDOC", loginUserInfo.companyID, loginUserInfo.userID);
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
                        CompanyDocList = SendDocBll.GetSendDocInfosListByTypeCompanyDepartmentSearch(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                    }
                    return CompanyDocList != null ? CompanyDocList : null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("CompanySendDoc:"+System.DateTime.Now.ToString("d")+" "+ex.ToString());
                return null;
            }
        }
        [OperationContract]
        public V_BumfCompanySendDoc GetBumfDocInfo(string docId)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                return SendDocBll.GetBumfDocInfo(docId);
            }
        }

        [OperationContract]
        //获取我的公文
        public List<V_BumfCompanySendDoc> GetMYSendDocInfosList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                IQueryable<V_BumfCompanySendDoc> CompanyDocList = null;

                CompanyDocList = SendDocBll.GetMySendDocInfosList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID, loginUserInfo.postID, loginUserInfo.companyID, loginUserInfo.departmentID);


                return CompanyDocList != null ? CompanyDocList.ToList() : null;
            }
        }

        #region 流程

        [OperationContract]
        public int SubmitCompanyDocFlow(T_OA_SENDDOC obj, FLOW_FLOWRECORDDETAIL_T flowRecordInfo, string toUserId)
        {
            using (BumfCompanySendDocManagementBll SendDocBll = new BumfCompanySendDocManagementBll())
            {
                SendDocBll.BeginTransaction();

                string nRet = this.SendDocAdd(obj); //添加公文文档信息
                if (nRet != "")
                {
                    SendDocBll.RollbackTransaction();
                    return -1;
                }
                //if (workFlowWS.StartFlow(flowRecordInfo, "", toUserId, "Add") != "OK")
                //{
                //    SendDocBll.RollbackTransaction();
                //    return -1;
                //}
                SendDocBll.CommitTransaction();
                return 1;
            }
        }

        

        [OperationContract]
        public int SubmitUpdateFlowComment1(string FlowGuid,string toUserId, string FlowContent)
        {

            
            return 1;
        }



        private List<string> GetGuidList(FLOW_FLOWRECORDDETAIL_T[] flowList)
        {
            List<string> guidStringList = new List<string>();
            
            return guidStringList;
        }

        #endregion

        #endregion

        #region 文档发布
        [OperationContract]
        //增
        public string DocDistrbuteAdd(T_OA_DISTRIBUTEUSER obj)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                string returnStr = "";

                bool sucess = DocDistrbuteBll.AddDocDistrbuteInfo(obj);
                if (sucess == false)
                {
                    returnStr = "添加公文发布失败";
                }

                return returnStr;
            }
        }
        [OperationContract]
        //批量添加文档发布
        public bool DocDistrbuteBatchAdd(T_OA_DISTRIBUTEUSER[] obj)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                return DocDistrbuteBll.BatchAddDocDistrbuteInfo(obj);
            }
        }

        [OperationContract]
        //批量添加文档发布
        public bool BatchAddCompanyDocDistrbute(T_OA_DISTRIBUTEUSER[] obj, List<string> employeeids, T_OA_SENDDOC doc)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                return DocDistrbuteBll.BatchAddDocDistrbuteInfoOnlyDoc(obj, employeeids, doc);
            }
        }

        [OperationContract]        
        public bool BatchAddCompanyDocDistrbuteForNew(List<T_OA_DISTRIBUTEUSER> obj,List<string> LstCompanyIDs,List<string> LstDepartmentIDs,List<string> LstPostIDs, List<string> employeeids, T_OA_SENDDOC doc)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                return DocDistrbuteBll.BatchAddDocDistrbuteInfoOnlyDocForNew(obj,LstCompanyIDs, LstDepartmentIDs, LstPostIDs, employeeids, doc);
            }
        }

        [OperationContract]
        public bool CloseDocTask(string docId, string userId)
        {
            try
            {
                BumfCompanySendDocManagementBll.CloseDotask(docId, userId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [OperationContract]
        //删
        public bool DocDistrbuteBatchDel(string[] StrDistrbuteIDs)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                return DocDistrbuteBll.BatchDeleteDocDistrbuteInfos(StrDistrbuteIDs);
            }
        }

        [OperationContract]
        //改
        public bool DocDistrbuteInfoUpdate(T_OA_DISTRIBUTEUSER obj)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                return DocDistrbuteBll.UpdateDocDistrbuteInfo(obj);
            }
        }
        /// <summary>
        /// 批量修改发布对象
        /// </summary>
        /// <param name="obj">发布对象集合</param>
        /// <param name="EntityID">表单ID</param>
        /// <returns></returns>
        [OperationContract]
        //改
        public bool DocDistrbuteInfoUpdateByBatch(List<T_OA_DISTRIBUTEUSER> obj,string EntityID)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                return DocDistrbuteBll.UpdateDocDistrbuteInfoByBatch(obj, EntityID);
            }
        }

        [OperationContract]
        //查单
        public T_OA_DISTRIBUTEUSER GetDocDistrbuteSingleInfoById(string StrDistrbuteId)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                return DocDistrbuteBll.GetDocDistrbuteInfoById(StrDistrbuteId);
            }
        }
        

        [OperationContract]        
        public List<T_OA_DISTRIBUTEUSER> GetDocDistrbuteInfos(string StrFormID)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                List<T_OA_DISTRIBUTEUSER> DistrbuteInfosList = DocDistrbuteBll.GetDocDistrbuteInfos(StrFormID);
                if (DistrbuteInfosList == null)
                {
                    return null;
                }
                else
                {
                    return DistrbuteInfosList;
                }
            }

        }

        [OperationContract]
        // 根据表单ID获取发布对象为员工的 员工ID集合 2010-7-8 ljx
        public List<string> GetDocDistrbuteInfosByFormID(string StrFormID)
        {
            using (BumfCompanyDocDistrbuteManagementBll DocDistrbuteBll = new BumfCompanyDocDistrbuteManagementBll())
            {
                List<string> DistrbuteInfosList = DocDistrbuteBll.GetDocDistrbuteUserInfosByFormID(StrFormID);
                if (DistrbuteInfosList == null)
                {
                    return null;
                }
                else
                {
                    return DistrbuteInfosList;
                }
            }

        }


        [OperationContract]
        //获取公文编号
        public List<V_CompanyDocNum> GetCompanyDocNumsByUserid(string Userid,string sort,ref string filterstring)
        {
            using (BumfCompanySendDocManagementBll bll = new BumfCompanySendDocManagementBll())
            {
                return bll.GetCompanyDocNumsBuUserID(Userid,sort,filterstring);
            }
        }
        
        #endregion


        #region IUserAuthenticate 成员

        public string VerifyUser(string username, string password, string appcode)
        {
            System.Web.Security.FormsAuthentication.SetAuthCookie(username, true);
            // 创建验证票
            System.Web.Configuration.FormsAuthenticationConfiguration formsConfig = new System.Web.Configuration.FormsAuthenticationConfiguration();
            FormsAuthenticationTicket formAuthTicket = new
                FormsAuthenticationTicket(
                        1,                              // 版本
                        username,                          // 用户名称
                        DateTime.Now,                   // 创建时间
                        DateTime.Now.AddMinutes(formsConfig.Timeout.TotalMinutes),    // 失效时间
                        true, "");    // 用户数据

            //加密票
            string encryptedTicket = FormsAuthentication.Encrypt(formAuthTicket);
            // 以加密票的密文存入Cookie
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

            authCookie.HttpOnly = true;
            authCookie.Path = FormsAuthentication.FormsCookiePath;
            authCookie.Secure = FormsAuthentication.RequireSSL;
            if (FormsAuthentication.CookieDomain != null)
            {
                authCookie.Domain = FormsAuthentication.CookieDomain;
            }
            if (formAuthTicket.IsPersistent)
            {
                authCookie.Expires = formAuthTicket.Expiration;
            }

            HttpContext.Current.Response.Cookies.Add(authCookie);
            FormsIdentity identity = new FormsIdentity(formAuthTicket);
            GenericPrincipal principal = new GenericPrincipal(identity, null);
            HttpContext.Current.User = principal;


            return "";
            
        }

        #endregion
    }
}
