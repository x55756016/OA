using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
//using SMT.SaaS.OA.Services.FlowService;
using SMT.SaaS.BLLCommonServices.FlowWFService;

using SMT.SaaS.OA.DAL.Views;
using SMT.SaaS.OA.DAL;


namespace SMT.SaaS.OA.Services
{

    public partial class SmtOADocumentAdmin
    {
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
        
        
        ServiceClient workFlowWS = new ServiceClient();



        #region 机构管理
        /// <summary>
        /// 查询机构
        /// </summary>
        /// <param name="organObj"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_ORGANIZATION> GetOrganList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            // 在此处添加操作实现
            IQueryable<T_OA_ORGANIZATION> organList = null;
            using (OrganManagementBll organBll = new OrganManagementBll())
            {
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())
                {
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        organList = organBll.GetOrganQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        organList = organBll.GetOrganQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }

                }
                else   //获取审批列表
                {
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "Organization", loginUserInfo.companyID, loginUserInfo.userID);
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
                    organList = organBll.GetOrganQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);

                }
                return organList != null ? organList.ToList() : null;
            }
        }

        /// <summary>
        /// 根据机构ID获取机构信息
        /// </summary>
        /// <param name="organID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_ORGANIZATION> GetOrganListByOrganId(string organID)
        {
            // 在此处添加操作实现
            //DateTime dt1 = DateTime.Now;
            using (OrganManagementBll organBll = new OrganManagementBll())
            {
                IQueryable<T_OA_ORGANIZATION> organList = organBll.GetOrganByOrganId(organID);
                //DateTime dt2 = DateTime.Now;
                //string a = (dt2 - dt1).TotalSeconds.ToString();
                return organList != null ? organList.ToList() : null;
            }
        }


        /// <summary>
        /// 根据机构ID获取证照
        /// </summary>
        /// <param name="organID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_LICENSEMASTER> GetLicenseMatsterListByOrganId(string organID)
        {
            // 在此处添加操作实现
            //DateTime dt1 = DateTime.Now;
            using (OrganManagementBll organBll = new OrganManagementBll())
            {
                IQueryable<T_OA_LICENSEMASTER> organList = organBll.GetLicenseMasterListByOrganId(organID);
                //DateTime dt2 = DateTime.Now;
                //string a = (dt2 - dt1).TotalSeconds.ToString();
                return organList != null ? organList.ToList() : null;
            }
        }


        /// <summary>
        /// 新增机构
        /// </summary>
        /// <param name="organObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddOrgan(T_OA_ORGANIZATION organObj,List<T_OA_LICENSEMASTER> licenseMasterList)
        {
            using (OrganManagementBll organBll = new OrganManagementBll())
            {
                string returnStr = "";

                if (!IsExistOrgan(organObj.ORGCODE))
                {
                    if (!organBll.AddOrgan(organObj, licenseMasterList))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "此机构已经存在,添加数据失败！";
                }
                return returnStr;
            }
        }

        //机构是否已存在
        [OperationContract]
        private bool IsExistOrgan(string organCode)
        {
            using (OrganManagementBll organBll = new OrganManagementBll())
            {
                return organBll.IsExistOrgan(organCode);
            }
        }

        //更新机构信息
        [OperationContract]
        public bool UpdateOrgan(T_OA_ORGANIZATION organObj, List<T_OA_LICENSEMASTER> licenseMasterList)
        {
            using (OrganManagementBll organBll = new OrganManagementBll())
            {
                return organBll.UpdateOrgan(organObj, licenseMasterList);
            }
        }

        [OperationContract]
        //删除机构信息
        public bool DeleteOrgan(string[] organCode,ref bool FBControl)
        {
            using (OrganManagementBll organBll = new OrganManagementBll())
            {
                return organBll.DeleteOrgan(organCode, ref FBControl);
            }
        }

        //提交审核
        //[OperationContract]
        //public int SubmitFlow(T_OA_ORGANIZATION organObj, FLOW_FLOWRECORDDETAIL_T flowRecordInfo, string toUserId)
        //{
        //    //if (workFlowWS.StartFlow(flowRecordInfo, "", toUserId, "Add") == "OK")
        //    //{
        //    //    if (organBll.UpdateOrgan(organObj))
        //    //    {
        //    //        return 1;
        //    //    }
        //    //}
        
        //    return -1;
        //}
        #endregion

        #region 证照管理

        #region 证照主表
        /// <summary>
        /// 获取证照主表信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_LICENSEMASTER> GetLicenseListPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (LicenseManagementBll licenseMasterBll = new LicenseManagementBll())
            {
                IQueryable<T_OA_LICENSEMASTER> ent = licenseMasterBll.GetLicenseList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return ent != null ? ent.ToList() : null;
            }
        }

        [OperationContract]
        public List<T_OA_LICENSEMASTER> GetLicenseListById(string licenseID)
        {
            using (LicenseManagementBll licenseMasterBll = new LicenseManagementBll())
            {
                return licenseMasterBll.GetLicenseById(licenseID);
            }
        }
        #endregion

        #region 证照子表
        /// <summary>
        /// 获取证照详细信息
        /// </summary>
        /// <param name="licenseID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_LICENSEDETAIL> GetLicenseDetailList(string licenseID)
        {
            using (LicenseDetailManagementBll licenseDetailBll = new LicenseDetailManagementBll())
            {
                IQueryable<T_OA_LICENSEDETAIL> list = licenseDetailBll.GetLicenseDetailById(licenseID);
                return list == null ? null : list.ToList();
            }
        }


        [OperationContract]
        public string AddLicenseDetail(T_OA_LICENSEDETAIL licenseObj)
        {
            using (LicenseDetailManagementBll licenseDetailBll = new LicenseDetailManagementBll())
            {
                string returnStr = "";

                if (!licenseDetailBll.AddLicenseDetail(licenseObj))
                {
                    returnStr = "添加数据失败";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 更新证照详细信息
        /// </summary>
        /// <param name="licenseObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateLicenseDetail(T_OA_LICENSEDETAIL[] licenseDetailObj,T_OA_LICENSEMASTER licenseMasterObj)
        {
            string returnStr = "";
            using (LicenseDetailManagementBll licenseDetailBll = new LicenseDetailManagementBll())
            {
                if (!licenseDetailBll.UpdateLicenseDetail(licenseDetailObj, licenseMasterObj))
                {
                    returnStr = "更新数据失败";
                }
                return returnStr;
            }
        }
        #endregion


        #endregion 

        #region 证照外借管理
        /// <summary>
        /// 获取外借证照信息
        /// </summary>
        /// <param name="param"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_LICENSEUSER> GetLicenseBorrowList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            IQueryable<T_OA_LICENSEUSER> licenseList = null;
            using (LicenseBorrowBll licenseBorrowBll = new LicenseBorrowBll())
            {
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())
                {
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        licenseList = licenseBorrowBll.GetLicenseQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        licenseList = licenseBorrowBll.GetLicenseQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                }
                else
                {
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "licenseBorrow", loginUserInfo.companyID, loginUserInfo.userID);
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
                    licenseList = licenseBorrowBll.GetLicenseQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                }
                return licenseList != null ? licenseList.ToList() : null;
            }
        }

        /// <summary>
        /// 根据ID获取外借证照信息
        /// </summary>
        /// <param name="licenseID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_OA_LICENSEUSER GetLicenseBorrowListById(string licenseID)
        {
            using (LicenseBorrowBll licenseBorrowBll = new LicenseBorrowBll())
            {
                return licenseBorrowBll.GetLicenseById(licenseID);
            }
        }

        /// <summary>
        /// 新增外借证照
        /// </summary>
        /// <param name="licenseObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddLicenseBorrow(T_OA_LICENSEUSER licenseObj)
        {
            string returnStr = "";
            using (LicenseBorrowBll licenseBorrowBll = new LicenseBorrowBll())
            {
                if (IsBorrowed(licenseObj.T_OA_LICENSEMASTER.LICENSEMASTERID))
                {
                    if (!licenseBorrowBll.AddLicenseBorrow(licenseObj))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "此证照已借出！";
                }
                return returnStr;
            }
        }

        [OperationContract]
        private bool IsBorrowed(string licenseID)
        {
            using (LicenseBorrowBll licenseBorrowBll = new LicenseBorrowBll())
            {
                return licenseBorrowBll.IsBorrowed(licenseID);
            }
        }

        /// <summary>
        /// 更新外借证照
        /// </summary>
        /// <param name="licenseObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateLicenseBorrow(T_OA_LICENSEUSER licenseObj)
        {
            string returnStr = "";
            using (LicenseBorrowBll licenseBorrowBll = new LicenseBorrowBll())
            {
                if (!licenseBorrowBll.UpdateLicenseBorrow(licenseObj))
                {
                    returnStr = "更新数据失败";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 删除外借证照
        /// </summary>
        /// <param name="lendingID"></param>
        /// <returns></returns>
        [OperationContract]
        public string DeleteLicenseBorrow(string[] licenseID)
        {
            string returnStr = "";
            using (LicenseBorrowBll licenseBorrowBll = new LicenseBorrowBll())
            {
                if (!licenseBorrowBll.DeleteLicenseBorrow(licenseID))
                {
                    returnStr = "删除数据失败";
                }
                return returnStr;
            }
        }
        

        //提交审核
        /// <summary>
        /// 
        /// </summary>
        /// <param name="licenseObj"></param>
        /// <param name="flowRecordInfo"></param>
        /// <param name="toUserId"></param>
        /// <returns></returns>
        //[OperationContract]
        //public int LicenseBorrowSubmitFlow(T_OA_LICENSEUSER licenseObj, FLOW_FLOWRECORDDETAIL_T flowRecordInfo, string toUserId)
        //{
        //    if (workFlowWS.StartFlow(flowRecordInfo, "", toUserId, "Add") == "OK")
        //    {
        //        if (licenseBorrowBll.UpdateLicenseBorrow(licenseObj))
        //        {
        //            return 1;
        //        }
        //    }
        //    return -1;
        //}

        
        /// <summary>
        /// Lookup控件查询Entity的方法
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>Entity记录集Xml</returns>
        [OperationContract]
        public string GetLookupOjbects(EntityNames entityName, Dictionary<string, string> args, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            string objxml = "";
            object ents = Utility.GetLookupData(entityName, args,pageIndex,pageSize,sort,filterString,paras, ref pageCount,userID);
            if (ents != null)
            {
                objxml = SerializerHelper.ContractObjectToXml(ents);
            }
            //object other = SerializerHelper.XmlToContractObject(objxml,typeof(T_HR_COMPANY[]));
            return objxml;
            //return null;
        }

        #endregion


        #region 证照归还管理
        [OperationContract]
        public List<T_OA_LICENSEUSER> GetBorrowAppList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string lendFlag, LoginUserInfo loginUserInfo)
        {
            using (LicenseReturnBll licenseReturnBll = new LicenseReturnBll())
            {
                IQueryable<T_OA_LICENSEUSER> borrowAppList = licenseReturnBll.GetLicenseBorrowAppListQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, lendFlag, loginUserInfo.userID);
                return borrowAppList != null ? borrowAppList.ToList() : null;
            }  
        }

        [OperationContract]
        public bool LendOrReturn(T_OA_LICENSEUSER licenseObj, string action)
        {
            using (LicenseReturnBll licenseReturnBll = new LicenseReturnBll())
            {
                return licenseReturnBll.LendOrReturn(licenseObj, action);
            }
        }
        #endregion
    }
       
}
