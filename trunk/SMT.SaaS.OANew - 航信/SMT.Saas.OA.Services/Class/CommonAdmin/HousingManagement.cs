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
using SMT.SaaS.OA.DAL;



namespace SMT.SaaS.OA.Services
{
    public partial class SmtOACommonAdmin
    {
        
     
        
        
        
        ServiceClient workFlowWS = new ServiceClient();

        #region 房源信息
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
        /// <summary>
        /// 获取住房信息(分页)
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示行数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">值</param>
        /// <param name="pageCount">记录总数</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HOUSEINFO> GetHouseInfoListPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                IQueryable<T_OA_HOUSEINFO> ent = houseBll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, "T_OA_HOUSEINFO");
                return ent.Count() > 0 ? ent.ToList() : null;
            }
        }

        /// <summary>
        /// 根据ID获取房源信息
        /// </summary>
        /// <param name="houseID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HOUSEINFO> GetHouseInfoById(string houseID)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                return houseBll.GetHouseInfoById(houseID);
            }
        }

        /// <summary>
        /// 获取房源架构树
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<V_HouseInfoTree> GetHouseInfoTree()
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                return houseBll.GetHouseInfoTree();
            }
        }
        
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="licenseObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddHouse(T_OA_HOUSEINFO houseObj)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                string returnStr = "";
                if (!houseBll.IsExist(houseObj))
                {
                    if (!houseBll.AddHouse(houseObj))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "此房源已经存在,添加数据失败！";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="houseObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateHouse(T_OA_HOUSEINFO houseObj)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                string returnStr = "";

                if (!houseBll.UpdateHouse(houseObj))
                {
                    returnStr = "更新数据失败";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="houseID"></param>
        /// <returns></returns>
        [OperationContract]
        public string DeleteHouse(string[] houseID, ref string errorMsg)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                string returnStr = "";

                if (!houseBll.DeleteHouse(houseID, ref errorMsg))
                {
                    returnStr = "删除数据失败";
                }
                return returnStr;
            }
        }

        [OperationContract]
        public List<V_HouseHirer> GetHouseHirerListPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                IQueryable<V_HouseHirer> ent = houseBll.HirerQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
                return ent.Count() > 0 ? ent.ToList() : null;
            }
        }

        #endregion

        #region 房源发布信息
        /// <summary>
        /// 分页获取房源发布信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HOUSEINFOISSUANCE> GetIssunaceListPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            //IQueryable<T_OA_HOUSEINFOISSUANCE> ent = issuanceBll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())
                {
                    IQueryable<T_OA_HOUSEINFOISSUANCE> ent = null;
                    if (checkState == ((int)CheckStates.ALL).ToString())
                    {
                        ent = issuanceBll.GetIssuanceQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    else
                    {
                        ent = issuanceBll.GetIssuanceQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }

                    return ent !=null ? ent.ToList() : null;
                }
                else
                {
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "housingIssuance", loginUserInfo.companyID, loginUserInfo.userID);
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
                    IQueryable<T_OA_HOUSEINFOISSUANCE> ent = issuanceBll.GetIssuanceQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                    return ent.Count() > 0 ? ent.ToList() : null;
                    //return null;
                }
            }
        }

        [OperationContract]
        public List<T_OA_HOUSEINFOISSUANCE> GetIssunaceForWebPart()
        {
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                IQueryable<T_OA_HOUSEINFOISSUANCE> ent = issuanceBll.GetIssuanceForWebPart();
                return ent.Count() > 0 ? ent.ToList() : null;
            }
        }

        /// <summary>
        /// 获取发布信息
        /// </summary>
        /// <param name="issuanceID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HOUSEINFOISSUANCE> GetIssuanceListById(string issuanceID)
        {
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                return issuanceBll.GetIssuanceListById(issuanceID);
            }
        }
        /// <summary>
        /// 获取发布房源信息
        /// </summary>
        /// <param name="issuanceID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HOUSEINFO> GetIssuanceHouseInfoList(string issuanceID)
        {
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                return issuanceBll.GetIssuanceHouseInfoListById(issuanceID);
            }
        }
        /// <summary>
        /// 获取发布房源清单
        /// </summary>
        /// <param name="issuanceID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HOUSELIST> GetIssuanceHouseList(string issuanceID)
        {
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                return issuanceBll.GetIssuanceHouseList(issuanceID);
            }
        }
        /// <summary>
        /// 获取发布范围
        /// </summary>
        /// <param name="issuanceID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_DISTRIBUTEUSER> GetDistributeUserList(string issuanceID)
        {
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                return issuanceBll.GetDistributeUserList(issuanceID);
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="issuanceObj"></param>
        /// <param name="houseListObj"></param>
        /// <param name="distributeListObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddIssuance(T_OA_HOUSEINFOISSUANCE issuanceObj, List<T_OA_HOUSELIST> houseListObj, List<T_OA_DISTRIBUTEUSER> distributeListObj)
        {
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                string returnStr = "";

                if (!issuanceBll.AddHouseInfoIssuance(issuanceObj, houseListObj, distributeListObj))
                {
                    returnStr = "添加数据失败";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="issuanceObj"></param>
        /// <param name="houseListObj"></param>
        /// <param name="distributeListObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateIssuance(T_OA_HOUSEINFOISSUANCE issuanceObj, List<T_OA_HOUSELIST> houseListObj, List<T_OA_DISTRIBUTEUSER> distributeListObj,bool SubmitFlag)
        {
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                string returnStr = "";

                if (!issuanceBll.UpdateHouseInfoIssuance(issuanceObj, houseListObj, distributeListObj, SubmitFlag))
                {
                    returnStr = "修改数据失败";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="issuanceID"></param>
        /// <returns></returns>
        [OperationContract]
        public string DeleteIssuance(string[] issuanceID)
        {
            string returnStr = "";
            using (HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll())
            {
                if (!issuanceBll.DeleteHouseInfoIssuance(issuanceID))
                {
                    returnStr = "删除数据失败";
                }
                return returnStr;
            }
        }

        //[OperationContract]
        //public int SubmitIssuanceFlow(T_OA_HOUSEINFOISSUANCE obj, FLOW_FLOWRECORDDETAIL_T flowRecordInfo, string toUserId)
        //{
        //    if (workFlowWS.StartFlow(flowRecordInfo, "", toUserId, "Add") != "OK")
        //    {
        //        return -1;
        //    }
        //    if (!issuanceBll.UpdateIssuance(obj))
        //    {
        //        return -1;
        //    }
        //    return 1;
        //}
        #endregion

        #region 租房管理
        /// <summary>
        /// 获取租房申请记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="checkState"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HIREAPP> GetHireAppListPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())
                {
                    IQueryable<T_OA_HIREAPP> ent = houseHireAppBll.GetHireAppQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    return ent.Count() > 0 ? ent.ToList() : null;
                }
                else
                {
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "houseHireApp", loginUserInfo.companyID, loginUserInfo.userID);
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
                    IQueryable<T_OA_HIREAPP> ent = houseHireAppBll.GetHireAppQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                    return ent.Count() > 0 ? ent.ToList() : null;
                }
            }
        }

        /// <summary>
        /// 通过视图v_househirer显示租房的详细信息  2010-5-20 by liujx
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="checkState"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_HouseHireApp> GetHireAppListPagingByHouseInfoOrList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())
                {
                    IQueryable<V_HouseHireApp> ent;
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ent = houseHireAppBll.GetHireAppQueryWithPagingByHouseInfoOrList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        ent = houseHireAppBll.GetHireAppQueryWithPagingByHouseInfoOrList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    return ent.Count() > 0 ? ent.ToList() : null;
                }
                else
                {
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "houseHireApp", loginUserInfo.companyID, loginUserInfo.userID);
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
                    IQueryable<V_HouseHireApp> ent = houseHireAppBll.GetHireAppQueryWithPagingByHouseInfoOrList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                    return ent.Count() > 0 ? ent.ToList() : null;
                }
            }
        }

        /// <summary>
        /// 通过视图v_househirer获取个人通过审核的租房记录  2010-5-20 by liujx
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="checkState"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_HouseHireApp> GetHireAppListPagingByMember(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                IQueryable<V_HouseHireApp> ent = houseHireAppBll.GetHireAppQueryWithPagingByHouseInfoOrList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                return ent!=null ? ent.ToList() : null;
            }
            
        }



        /// <summary>
        /// 根据ID获取租房申请记录
        /// </summary>
        /// <param name="hireAppID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HIREAPP> GetHireAppByID(string hireAppID)
        {
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                IQueryable<T_OA_HIREAPP> ent = houseHireAppBll.GetHireAppById(hireAppID);
                return ent.Count() > 0 ? ent.ToList() : null;
            }
        }

        /// <summary>
        /// 根据根据房源列表ID获取租房申请记录
        /// </summary>
        /// <param name="hireAppID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_HouseHireList> GetHireAppByHouseListID(string ListID)
        {
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                IQueryable<V_HouseHireList> ent = houseHireAppBll.GetHireAppInfoByHouseListId(ListID);
                return ent.Count() > 0 ? ent.ToList() : null;
            }
        }

        /// <summary>
        /// 新增租房申请
        /// </summary>
        /// <param name="hireAppObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddHireApp(T_OA_HIREAPP hireAppObj)
        {
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                string returnStr = "";

                if (!houseHireAppBll.IsHired(hireAppObj.T_OA_HOUSELIST, hireAppObj.OWNERID))
                {
                    returnStr = houseHireAppBll.AddHireApp(hireAppObj);

                }
                else
                {
                    returnStr = "HOUSEISOKNOTHIRE";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 更新租房申请
        /// </summary>
        /// <param name="hireAppObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateHireApp(T_OA_HIREAPP hireAppObj)
        {
            
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                string returnStr = "";
                if (!houseHireAppBll.UpdateHireApp(hireAppObj))
                {
                    returnStr = "修改数据失败";
                }
                return returnStr;
            }
        }

        /// <summary>
        ///删除租房申请
        /// </summary>
        /// <param name="hireAppID"></param>
        /// <returns></returns>
        [OperationContract]
        public string DeleteHireApp(string[] hireAppID)
        {
            
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                string returnStr = "";
                if (!houseHireAppBll.DeleteHireApp(hireAppID))
                {
                    returnStr = "删除数据失败";
                }
                return returnStr;
            }
        }
        

        /// <summary>
        /// 获取当前用户能申请的租房房间列表信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HOUSEINFO> GetHireAppHouseListPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {          
            //if (checkState != ((int)CheckStates.WaittingApproval).ToString())
            //{
            //    IQueryable<T_OA_HIREAPP> ent = houseHireAppBll.GetHireAppQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, null, checkState, loginUserInfo.userID);
            //    return ent.Count() > 0 ? ent.ToList() : null;
            //}
            //else
            //{
            //    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "0", "housingHireApp", loginUserInfo.companyID, loginUserInfo.userID);
            //    if (flowList == null)
            //    {
            //        return null;
            //    }
            //    List<V_FlowAPP> flowAppList = new List<V_FlowAPP>();
            //    for (int i = 0; i < flowList.Length; i++)
            //    {

            //        V_FlowAPP App = new V_FlowAPP();
            //        App.Guid = flowList[i].GUID;
            //        App.FormID = flowList[i].FormID;
            //        App.EditUserID = flowList[i].EditUserID;
            //        App.EditUserName = flowList[i].EditUserName;
            //        flowAppList.Add(App);
            //    }
            //    checkState = ((int)CheckStates.Approving).ToString();
            //    IQueryable<T_OA_HIREAPP> ent = houseHireAppBll.GetHireAppQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
            //    return ent.Count() > 0 ? ent.ToList() : null;
            //}
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                IQueryable<T_OA_HOUSEINFO> ent = houseHireAppBll.GetHireAppHouseInfoQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, loginUserInfo.userID);
                return ent != null ? ent.ToList() : null;
            }
        }


        [OperationContract]
        //获取发布的房源信息
        public List<V_HouseHireList> GetMemberHireHouseAppListPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll())
            {
                IQueryable<V_HouseHireList> ent = houseHireAppBll.GetHireAppHouseInfoListQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, loginUserInfo.userID);
                return ent != null ? ent.ToList() : null;
            }
        }
        #endregion

        #region 租房记录
        /// <summary>
        /// 根据ID获取租房记录
        /// </summary>
        /// <param name="hireAppID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_HIRERECORD> GetHireRecordByID(string RecordID)
        {
            using (HouseHireRecord HouseRecordBll = new HouseHireRecord())
            {
                IQueryable<T_OA_HIRERECORD> ent = HouseRecordBll.GetHireRecordById(RecordID);
                return ent.Count() > 0 ? ent.ToList() : null;
            }
        }

        
        /// <summary>
        /// 新增租房记录
        /// </summary>
        /// <param name="hireAppObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddHireRecord(T_OA_HIRERECORD RecordObj)
        {
            using (HouseHireRecord HouseRecordBll = new HouseHireRecord())
            {
                return HouseRecordBll.AddHireRecord(RecordObj);
            }
        }

        /// <summary>
        /// 更新租房申请
        /// </summary>
        /// <param name="hireAppObj"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateHireRecord(T_OA_HIRERECORD RecordObj)
        {
            using (HouseHireRecord HouseRecordBll = new HouseHireRecord())
            {
                string returnStr = "";

                if (!HouseRecordBll.UpdateHireRecord(RecordObj))
                {
                    returnStr = "修改数据失败";
                }
                return returnStr;
            }
        }

        [OperationContract]
        public List<V_HireRecord> GetHireRecordListPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount,string checkState, LoginUserInfo loginUser)
        {
            using (HouseHireRecord HouseRecordBll = new HouseHireRecord())
            {
                IQueryable<V_HireRecord> ent = HouseRecordBll.GetHireRecordQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, checkState, loginUser.userID);
                return ent != null ? ent.ToList() : null;
            }
        }

        /// <summary>
        /// 返回房源发布信息和会议通知，考虑兼职的情况
        /// </summary>
        /// <param name="userID">登陆人ID</param>
        /// <param name="postIDs">岗位ID集合</param>
        /// <param name="companyIDs">公司ID集合</param>
        /// <param name="departmentIDs">部门ID集合</param>
        /// <returns></returns>
        [OperationContract]        
        public List<V_SystemNotice> GetHouseIssueAndNoticeInfos(string userID, List<string> postIDs, List<string> companyIDs, List<string> departmentIDs)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                IQueryable<V_SystemNotice> ent = houseBll.GetHouseAndNoticeInfo(userID, postIDs, companyIDs, departmentIDs);
                return ent != null ? ent.ToList() : null;
            }
            
        }

        /// <summary>
        /// 获取刚发布的公文
        /// </summary>
        /// <returns></returns>
        [OperationContract]        
        public List<V_SystemNotice> RefreshSendDocData(string employeeid, int pageIndex
            , int pageSize, ref int pageCount, ref int DataCount, List<string> postIDs
            , List<string> companyIDs, List<string> departmentIDs
            , ref bool NeedGetNewData, bool NeedAllData
            , string filterString, List<object> paras, string Doctitle)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                List<V_SystemNotice> ent = houseBll.RefreshSendDocData(employeeid,pageIndex, pageSize, ref pageCount
                    , ref DataCount, postIDs, companyIDs, departmentIDs, ref NeedGetNewData, NeedAllData
                    , filterString, paras, Doctitle);
                return ent != null ? ent : null;
            }
        }


        /// <summary>
        /// 返回房源发布信息和会议通知  专门给手机使用
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页的数量</param>
        /// <param name="pageCount">页的总数</param>
        /// <param name="DataCount">总数量</param>
        /// <param name="userID">用户ID</param>
        /// <param name="postIDs">岗位ID集合</param>
        /// <param name="companyIDs">公司ID集合</param>
        /// <param name="departmentIDs">部门ID集合</param>
        /// <returns></returns>
        [OperationContract]        
        public List<V_SystemNotice> GetHouseIssueAndNoticeInfosToMobile(int pageIndex, int pageSize,ref int pageCount,ref int DataCount,string userID, List<string> postIDs, List<string> companyIDs, List<string> departmentIDs
            )
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                IQueryable<V_SystemNotice> ent = houseBll.GetHouseAndNoticeInfoToMobile(pageIndex,pageSize, ref pageCount, ref DataCount,userID, postIDs, companyIDs, departmentIDs
                ,string.Empty,null,string.Empty);
                return ent != null ? ent.ToList() : null;
                
            }

        }
        /// <summary>
        /// 手机版获取前后的记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="postIDs">岗位id集合</param>
        /// <param name="companyIDs">公司ID集合</param>
        /// <param name="departmentIDs">部门ID集合</param>
        /// <param name="formid">表单ID</param>
        /// <param name="Preview">前一条记录</param>
        /// <param name="Next">后一条记录</param>
        /// <returns></returns>
        [OperationContract]
        public T_OA_SENDDOC GetSysNoticeByFormidToMobile(string userID, List<string> postIDs, List<string> companyIDs, List<string> departmentIDs, string formid, ref V_SystemNotice Preview, ref V_SystemNotice Next)
        {
            using (HouseInfoManagerBll houseBll = new HouseInfoManagerBll())
            {
                T_OA_SENDDOC ent = houseBll.GetHouseAndNoticeInfoByPriveAndNext(userID,postIDs,companyIDs,departmentIDs,formid,ref Preview,ref Next);
                return ent ;
            }
        }
        [OperationContract]
        //获取租房人的费用 2010-6-25
        public decimal GetEmployeeHireRecordFee(string EmployeeID, int year, int month)
        {
            using (HouseHireRecord HouseRecordBll = new HouseHireRecord())
            {
                return HouseRecordBll.GetEmployeeHireRecordFee(EmployeeID, year, month);
            }
        }
        [OperationContract]
        //通过引擎调用 获取租房费用信息
        public void FromEngineToAddHireRecord(string HireAppID)
        {
            using (HouseHireRecord HouseRecordBll = new HouseHireRecord())
            {
                HouseRecordBll.GetHireAppToHireRecord(HireAppID);
            }
        }
        #endregion
    }
}
