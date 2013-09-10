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

namespace SMT.SaaS.OA.Services
{

    public partial class SmtOACommonOffice 
    {

        #region 会议室管理

        //private SMT.SaaS.OA.Services.FlowService.ServiceClient workFlowWS = new ServiceClient();
        
        [OperationContract]
        //根据会议室ID获取会议室信息
        public List<T_OA_MEETINGROOM> GetMeetingRoomInfoByID(string RoomId)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                IQueryable<T_OA_MEETINGROOM> MeetingRoomList = RoomBll.GetMeetingRoomListByID(RoomId);
                if (MeetingRoomList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingRoomList.ToList();
                }
            }
        }


        
        [OperationContract]
        //获取会议室信息
        public List<T_OA_MEETINGROOM> GetMeetingRoomInfos(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                IQueryable<T_OA_MEETINGROOM> MeetingRoomList = RoomBll.GetMeetingRoomInfosList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);

                return MeetingRoomList != null ? MeetingRoomList.ToList() : null;
            }
        }



        [OperationContract]
        //添加会议室信息
        public string MeetingRoomdAdd(T_OA_MEETINGROOM obj)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                return RoomBll.AddRoomInfo(obj); ;
            }
        }

        [OperationContract]
        //删除会议室信息
        public bool MeetingRoomdDel(string RoomID)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                return RoomBll.DeleteRoomInfo(RoomID);
            }
        }
        [OperationContract]
        //批量删除会议室信息
        public string BatchDelMeetingRoomdInfos(string[] RoomIDs)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                return RoomBll.BatchDeleteMeetingRoomInfos(RoomIDs);
            }
        }

        [OperationContract]
        //更新会议室信息
        public void MeetingRoomUpdate(T_OA_MEETINGROOM obj)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                RoomBll.UpdateRoomInfo(obj);
            }
        }

        [OperationContract]
        private bool IsExistMeetingRoom(string RoomName,string CompanyID)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                return RoomBll.GetMeetingRoomByRoomName(RoomName, CompanyID);
            }

        }

        [OperationContract]
        public T_OA_MEETINGROOM GetMeetingRoomById(string RoomId)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                return RoomBll.GetMeetingRoomById(RoomId);
            }
        }
        [OperationContract]
        public List<T_OA_MEETINGROOM> GetMeetingRoomNameInfosToCombox()
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                IQueryable<T_OA_MEETINGROOM> MeetingRoomList = RoomBll.GetMeetingRoomNameInfos();

                return MeetingRoomList.Count() > 0 ? MeetingRoomList.ToList() : null;
            }
        }


        [OperationContract]
        public List<T_OA_MEETINGROOM> GetMeetintRoomInfosListBySearch(string StrMeetingName, string StrMemo)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                IQueryable<T_OA_MEETINGROOM> MeetingTypeList = RoomBll.GetMeetingRoomInfosListBySearch(StrMeetingName, StrMemo);
                if (MeetingTypeList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingTypeList.ToList();
                }
            }
        }
        [OperationContract]
        //获取某一公司的会议室信息
        public List<T_OA_MEETINGROOM> GetMeetingRoomTreeInfosByCompanyID(string UserID)
        {
            using (MeetingRoomBll RoomBll = new MeetingRoomBll())
            {
                IQueryable<T_OA_MEETINGROOM> MeetingRoomList = RoomBll.GetMeetingRoomTreeInfosListByCompanyID(UserID);
                if (MeetingRoomList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingRoomList.ToList();
                }
            }
        }
        

        #endregion

        #region 会议时间变更
        


        [OperationContract]
        public string MeetingTimeChangeAdd(T_OA_MEETINGTIMECHANGE obj)
        {
            using (MeetingInfoTimeChangeManagementBll MeetingInfoTimeClient = new MeetingInfoTimeChangeManagementBll())
            {
                return MeetingInfoTimeClient.AddMeetingInfoTimeChangeInfo(obj); ;
            }
            
        }


        [OperationContract]
        private bool IsExistMeetingTimeChange(string StrMeetingInfoID, DateTime DtStart, DateTime DtEnd, string StrCreateUser)
        {
            using (MeetingInfoTimeChangeManagementBll MeetingInfoTimeClient = new MeetingInfoTimeChangeManagementBll())
            {
                return MeetingInfoTimeClient.GetMeetingInfoTimeChangeByAppIDStartOrEndOrCreateUser(StrMeetingInfoID, DtStart, DtEnd, StrCreateUser);
            }
        }
        #endregion

        #region 与会人员管理
        

        [OperationContract]
        //添加        
        public string MeetingStaffInfoAdd(T_OA_MEETINGSTAFF obj)
        {
            using (MeetingStaffManagementBll MeetingStaffBll = new MeetingStaffManagementBll())
            {
                return MeetingStaffBll.AddMeetingStaffInfo(obj);
            }
        }

        [OperationContract]
        //批量添加与会人员 
        public string BatchAddMeetingStaffInfos(List<T_OA_MEETINGSTAFF> obj)
        {
            using (MeetingStaffManagementBll MeetingStaffBll = new MeetingStaffManagementBll())
            {
                string returnStr = "";

                foreach (T_OA_MEETINGSTAFF StaffT in obj)
                {
                    returnStr = MeetingStaffBll.AddMeetingStaffInfo(StaffT);
                }

                return returnStr;
            }
        }
        [OperationContract]
        public void UpdateMeetingStaffsByEmcce(T_OA_MEETINGSTAFF obj)
        {
            using (MeetingStaffManagementBll MeetingStaffBll = new MeetingStaffManagementBll())
            {
                MeetingStaffBll.UpdateMeetingStaffInfo(obj);
            }
        }

        [OperationContract]
        private bool IsExistMeetingStaffInfoByAdd(string StrMeetingInfoId, string UserID)
        {
            using (MeetingStaffManagementBll MeetingStaffBll = new MeetingStaffManagementBll())
            {
                return MeetingStaffBll.GetMeetingStaffByMeetingInfoAndUserID(StrMeetingInfoId, UserID);
            }

        }


        [OperationContract]
        public bool MeetingStaffInfoDel(string StrMeetingStaffID)
        {
            using (MeetingStaffManagementBll MeetingStaffBll = new MeetingStaffManagementBll())
            {
                return MeetingStaffBll.DeleteMeetingStaffInfo(StrMeetingStaffID);
            }

        }

        [OperationContract]
        public void MeetingStaffUpdateInfos(T_OA_MEETINGSTAFF obj)
        {
            using (MeetingStaffManagementBll MeetingStaffBll = new MeetingStaffManagementBll())
            {
                MeetingStaffBll.UpdateMeetingStaffInfo(obj);
            }
        }

        [OperationContract]
        public List<T_OA_MEETINGSTAFF> GetAllMeetingStaffInfosByMeetingInfoID(string StrMeetingInfoID)
        {
            using (MeetingStaffManagementBll MeetingStaffBll = new MeetingStaffManagementBll())
            {
                return MeetingStaffBll.GetMeetingStaffInfosByMeetingInfoID(StrMeetingInfoID);
            }
        }
        [OperationContract]
        //获取参会人员上传的内容、附件信息
        public List<V_MyMeetingInfosManagement> GetMeetingStaffByMeetingInfoIdEmcee(string StrFlag,string StrIsOk,string StrMeetingInfoID)
        {
            using (MeetingStaffManagementBll MeetingStaffBll = new MeetingStaffManagementBll())
            {
                IQueryable<V_MyMeetingInfosManagement> StaffList = null;

                StaffList = MeetingStaffBll.GetMeetingStaffInfosByMeetingInfoIDEmcee(StrFlag, StrIsOk, StrMeetingInfoID);
                return StaffList.Count() > 0 ? StaffList.ToList() : null;
            }
        }
        #endregion

        #region 会议室时间变更
        

        [OperationContract]
        public string MeetingRoomTimeChangeAdd(T_OA_MEETINGROOMTIMECHANGE obj)
        {
            using (MeetingRoomTimeChangeManagementBll MeetingTimeChangeBll = new MeetingRoomTimeChangeManagementBll())
            {
                return MeetingTimeChangeBll.AddMeetingRoomTimeChangeInfo(obj);
            }
        }


        [OperationContract]
        private bool IsExistMeetingRoomTimeChange(string StrAppId, DateTime DtStart, DateTime DtEnd, string StrCreateUser)
        {
            using (MeetingRoomTimeChangeManagementBll MeetingTimeChangeBll = new MeetingRoomTimeChangeManagementBll())
            {
                return MeetingTimeChangeBll.GetMeetingTimeChangeByAppIDStartOrEndOrCreateUser(StrAppId, DtStart, DtEnd, StrCreateUser);
            }

        }
        #endregion

        #region 会议室申请
        

        [OperationContract]
        //添加        
        public string MeetingRoomAppInfoAdd(T_OA_MEETINGROOMAPP obj)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                return RoomAppBll.AddMeetingRoomAppInfo(obj);
            }
            
        }

        [OperationContract]       
        private bool IsExistMeetingRoomAppInfoByAdd(string StrRoomName, string StrDepartment, DateTime startdt, DateTime enddt, string StrCreateUserID)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                return RoomAppBll.GetMeetingRoomAppInfoByRoomStartEnd(StrRoomName, StrDepartment, startdt, enddt, StrCreateUserID);
            }

        }

        

        [OperationContract]
        private bool IsExistMeetingRoomJustUsing(string StrRoomName, DateTime startdt, DateTime enddt)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                return RoomAppBll.GetMeetingRoomJustUsingByRoomStartEnd(StrRoomName, startdt, enddt);
            }

        }




        [OperationContract]
        public bool MeetingRoomAppInfoDel(string StrRoomAppInfoID)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                return RoomAppBll.DeleteMeetingRoomAppInfo(StrRoomAppInfoID);
            }

        }

        [OperationContract]
        //批量删
        public bool MeetingRoomAppBatchDel(string[] StrRoomAppIDs)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                return RoomAppBll.BatchDeleteMeetingRoomAppInfo(StrRoomAppIDs);
            }
        }


        [OperationContract]
        public string MeetingRoomAppUpdate(T_OA_MEETINGROOMAPP obj)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                return RoomAppBll.UpdateMeetingRoomAppInfo(obj);
            }
        }

        [OperationContract]
        public T_OA_MEETINGROOMAPP GetMeetingRoomAppSingleInfoByAppId(string StrRoomAppId)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                return RoomAppBll.GetMeetingRoomAppInfoById(StrRoomAppId);
            }
        }


        [OperationContract]
        // 2010-3-23 添加审核状态
        public List<T_OA_MEETINGROOMAPP> GetMeetingRoomAppInfos(string StrCheckState)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                List<T_OA_MEETINGROOMAPP> MeetingInfosList = RoomAppBll.GetMeetingRoomAppInfos(StrCheckState);
                if (MeetingInfosList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingInfosList;
                }
            }
        }
        [OperationContract]
        ///根据开始时间、结束时间获取审核通过的会议室申请信息
        public List<T_OA_MEETINGROOMAPP> GetMeetingRoomAppBySelectRooms(DateTime start, DateTime end, string StrCheckState)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                IQueryable<T_OA_MEETINGROOMAPP> MeetingInfosList = RoomAppBll.GetMeetingRoomAppInfos(start,end,StrCheckState);
                if (MeetingInfosList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingInfosList.ToList();
                }
            }
        }
        [OperationContract]
        public List<V_MeetingRoomApp> GetMeetingRoomAppInfosByFlow(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                IQueryable<V_MeetingRoomApp> RoomAppsList = null;

                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取待审核信息
                {
                    if (checkState == ((int)CheckStates.ALL).ToString())
                    {
                        RoomAppsList = RoomAppBll.GetMeetingRoomAppInfosByFlow(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    else
                    {
                        RoomAppsList = RoomAppBll.GetMeetingRoomAppInfosByFlow(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                }
                else
                {
                    ServiceClient workFlowWS = new ServiceClient();
                    V_MeetingRoomApp a = new V_MeetingRoomApp();
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作

                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "MeetingRoomApp", loginUserInfo.companyID, loginUserInfo.userID);
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
                    RoomAppsList = RoomAppBll.GetMeetingRoomAppInfosByFlow(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                }
                return RoomAppsList != null ? RoomAppsList.ToList() : null;
            }
        }
        

        [OperationContract]
        //查询
        public List<T_OA_MEETINGROOMAPP> GetMeetintRoomAppInfosListBySearch(string StrMeetingRoom, string StrDepartment, DateTime DtStart, DateTime DtEnd, string StrCheckState, string StrCreateUser)
        {
            using (MeetingRoomAppManagementBll RoomAppBll = new MeetingRoomAppManagementBll())
            {
                IQueryable<T_OA_MEETINGROOMAPP> MeetingRoomAppList = RoomAppBll.GetMeetingRoomAppListBySearch(StrMeetingRoom, DtStart, DtEnd, StrDepartment, StrCheckState, StrCreateUser);
                if (MeetingRoomAppList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingRoomAppList.ToList();
                }
            }
        }
        #endregion

        #region 会议管理
        
        //private SMT.SaaS.OA.Services.FlowService.ServiceClient workFlowWS = new SMT.SaaS.OA.Services.FlowService.ServiceClient();
        [OperationContract]
        //添加        
        public string MeetingInfoAdd(T_OA_MEETINGINFO obj,List<T_OA_MEETINGSTAFF> meetingstaffObj,List<T_OA_MEETINGCONTENT> ContentObj,T_OA_MEETINGMESSAGE meetingmessageObj)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.AddMeetingInfo(obj, meetingstaffObj, ContentObj, meetingmessageObj);
            }
            
        }


        [OperationContract]
        private bool IsExistMeetingInfoByAdd(string StrTitle, string StrMeetingType, string StrRoom, string StrCreatUser, DateTime startdt, DateTime enddt)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.GetMeetingInfoByAdd(StrTitle, StrMeetingType, StrRoom, StrCreatUser, startdt, enddt);
                //return MeetingInfoBLL.GetMeetingTemplateByTemplateNameAndType(StrMeetingType, StrTemplateName);
            }

        }

        [OperationContract]
        public bool MeetingInfoDel(string StrMeetingInfoID)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.DeleteMeetingInfo(StrMeetingInfoID);
            }

        }


        [OperationContract]
        public int MeetingInfoUpdateByForm(T_OA_MEETINGINFO obj, List<T_OA_MEETINGSTAFF> meetingstaffObj, List<T_OA_MEETINGCONTENT> ContentObj, T_OA_MEETINGMESSAGE meetingmessageObj)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.UpdateMeetingInfo(obj, meetingstaffObj, ContentObj, meetingmessageObj);
            }
        }

        [OperationContract]
        public int MeetingInfoUpdate(T_OA_MEETINGINFO obj)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.UpdateMeetingInfo(obj);
            }
            
        }

        [OperationContract]
        //批量删除
        public bool MeetingInfoBatchDel(string[] StrMeetingInfoIDs)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.BatchDeleteMeetingInfo(StrMeetingInfoIDs);
            }
        }


        [OperationContract]
        public T_OA_MEETINGINFO GetMeetingInfoSingleInfoById(string StrMeetingInfoId)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.GetMeetingInfoById(StrMeetingInfoId);
            }
        }


        [OperationContract]
        //获取会议通知信息
        public T_OA_MEETINGMESSAGE GetMeetingMessageByID(string MessageID)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.GetMeetingMessageByID(MessageID);
            }
        }
        //public List<T_OA_MEETINGINFO> GetMeetingInfos(string StrState, List<string> idList)
        //{
        //    List<T_OA_MEETINGINFO> MeetingInfosList = MeetingInfoBLL.GetMeetingInfos(StrState, idList);
        //    if (MeetingInfosList == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return MeetingInfosList;
        //    }
        //}

        [OperationContract]
        public List<T_OA_MEETINGINFO> GetMeetintInfosListByTitleTimeSearch(string StrTitle, string StrDepartment, string StrType, string StrContent, DateTime DtStart, DateTime DtEnd, string StrCheckState)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                IQueryable<T_OA_MEETINGINFO> MeetingInfoList = MeetingInfoBLL.GetMeetingInfosListByTitleTimeSearch(StrTitle, StrType, StrDepartment, DtStart, DtEnd, StrContent, StrCheckState);
                if (MeetingInfoList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingInfoList.ToList();
                }
            }
        }


        //[OperationContract]
        //public List<V_MyMeetingInfosManagement> GetMyMeetingInfosManagement(string StrUserID)
        //{

        //    IQueryable<V_MyMeetingInfosManagement> MyMeetingInfosList = MeetingInfoBLL.GeMeetingMembertMeetingInfos(StrUserID);
        //    if (MyMeetingInfosList != null)
        //    {
        //        return MyMeetingInfosList.ToList();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        [OperationContract]
        public List<V_MyMeetingInfosManagement> GetMyMeetingInfosManagement(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount,string StrUserID)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {

                IQueryable<V_MyMeetingInfosManagement> MyMeetingInfosList = MeetingInfoBLL.GeMeetingMembertMeetingInfos(pageIndex, pageSize, sort, filterString, paras, ref pageCount, StrUserID);
                if (MyMeetingInfosList != null)
                {
                    return MyMeetingInfosList.ToList();
                }
                else
                {
                    return null;
                }
            }
        }
        [OperationContract]
        ///获取员工主持的会议
        public List<V_MyMeetingInfosManagement> GetMyEmceeMeetingInfosManagement(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string StrUserID)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                IQueryable<V_MyMeetingInfosManagement> MyMeetingInfosList = MeetingInfoBLL.GeMemberEmceeMeetingInfos(pageIndex, pageSize, sort, filterString, paras, ref pageCount, StrUserID);
                if (MyMeetingInfosList != null)
                {
                    return MyMeetingInfosList.ToList();
                }
                else
                {
                    return null;
                }
            }
        }
        [OperationContract]
        public int UpdateMeetingNoticeInfo(T_OA_MEETINGINFO info, T_OA_MEETINGMESSAGE message)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.UpdateMeetingNoticeInfo(info, message);
            }
        }

        [OperationContract]
        public string GetFlowIdByKey(string masterID, string userId, string flagStatus)//T 待审  F 审完
        {
            //FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();
            //flowInfo.EditUserID = userId;
            //flowInfo.Flag = flagStatus;     //审批标志，0为待审批,1为已审批完成
            //flowInfo.CompanyID = "";
            //flowInfo.CreateDate = System.DateTime.Now;
            //flowInfo.FlowCode = "";
            //flowInfo.FormID = masterID;           //保存的模块表ID
            //flowInfo.GUID = "";             //
            //flowInfo.InstanceID = "";
            //flowInfo.ModelCode = "";        //模块代码
            //flowInfo.OfficeID = "";         //岗位ID
            //flowInfo.CreateUserID = "";     //创建流程用户ID
            //flowInfo.StateCode = "";        //状态代码
            //flowInfo.ParentStateCode = "";  //父状态代码
            //FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo(flowInfo);
            //if (flowList != null && flowList.Count() > 0)
            //{
            //    return flowList[0].GUID;
            //}
            return null;
        }

        [OperationContract]
        public V_MeetingInfo GetMeetingNoticeByNoticeID(string NoticeId)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                return MeetingInfoBLL.GetMeetingNoticeByNoticeID(NoticeId);
            }
        }
        
        //public List<T_OA_MEETINGINFO> GetInfoListByFlow(string userId, string isChecked)
        //{
        //    //FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作
        //    //flowInfo.EditUserID = userId;
        //    //flowInfo.Flag = isChecked;                              //审批标志，0为待审批,1为已审批完成
        //    //flowInfo.CompanyID = "";
        //    //flowInfo.CreateDate = System.DateTime.Now;
        //    //flowInfo.FlowCode = "";
        //    //flowInfo.FormID = "";                                   //保存的模块表ID
        //    //flowInfo.GUID = "";
        //    //flowInfo.InstanceID = "";
        //    //flowInfo.ModelCode = "APP";                                //模块代码
        //    //flowInfo.OfficeID = "";                                 //岗位ID
        //    //flowInfo.CreateUserID = "";                             //创建流程用户ID
        //    //flowInfo.StateCode = "";                                //状态代码
        //    //flowInfo.ParentStateCode = "";                          //父状态代码
        //    //FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo(flowInfo);
        //    //if (flowList == null)
        //    //{
        //    //    return null;
        //    //}
        //    //return null;
        //    List<string> guidStringList = new List<string>();
        //    //if (flowList != null)
        //    //{
        //    //    if (flowList.Count() > 0)
        //    //    {

        //    //        foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
        //    //        {
        //    //            guidStringList.Add(f.FormID);
        //    //        }

        //    //    }
        //    //}
        //    //if (guidStringList.Count < 1)
        //    //{
        //    //    return null;
        //    //}
        //    IEnumerable<T_OA_MEETINGINFO> approList = MeetingInfoBLL.GetMeetingInfos(isChecked, guidStringList);//.GetInfoListByFlowFlag("2", guidStringList);//未通过
        //    if (approList == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return approList.ToList();
        //    }
        //}  2010-3-30 注释

        [OperationContract]
        public List<V_MeetingInfo> GetMeetingInfoListByFlow(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                IQueryable<V_MeetingInfo> MeetingInfoList = null;
                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交借阅信息
                {
                    if (checkState == ((int)CheckStates.ALL).ToString())
                    {
                        MeetingInfoList = MeetingInfoBLL.GetMeetingInfos(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                    else
                    {
                        MeetingInfoList = MeetingInfoBLL.GetMeetingInfos(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                }
                else                    //通过工作流获取用户要审批的借阅信息
                {
                    ServiceClient workFlowWS = new ServiceClient();
                    V_MeetingInfo a = new V_MeetingInfo();
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "MeetingInfo", loginUserInfo.companyID, loginUserInfo.userID);
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
                    MeetingInfoList = MeetingInfoBLL.GetMeetingInfos(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                }
                return MeetingInfoList != null ? MeetingInfoList.ToList() : null;
            }
        }


        #region 流程

        [OperationContract]
        public int SubmitFlow(T_OA_MEETINGINFO obj, FLOW_FLOWRECORDDETAIL_T flowRecordInfo, string toUserId)
        {
            using (MeetingManagementBll MeetingInfoBLL = new MeetingManagementBll())
            {
                MeetingInfoBLL.BeginTransaction();

                int nRet = MeetingInfoBLL.UpdateMeetingInfo(obj);
                if (nRet != 1)
                {
                    MeetingInfoBLL.RollbackTransaction();
                    return -1;
                }
                //if (workFlowWS.StartFlow(flowRecordInfo, "", toUserId, "Add") != "OK")
                //{
                //    MeetingInfoBLL.RollbackTransaction();
                //    return -1;
                //}
                MeetingInfoBLL.CommitTransaction();
                return 1;
            }
        }

        [OperationContract]
        public int SubmitComment(FLOW_FLOWRECORDDETAIL_T flowRecordInfo, string toUserId, string empComment)
        {

            //FLOW_FLOWRECORDDETAIL_T[] result = workFlowWS.GetFlowInfo(flowRecordInfo);
            //flowRecordInfo = result[0];
            //flowRecordInfo.Content = empComment;
            //if (workFlowWS.StartFlow(flowRecordInfo, "", toUserId, "Update") == "OK")
            //{
            //    return 1;
            //}
            //else
            //{
            //    return -1;
            //}
            return 1;
        }

        //private FLOW_FLOWRECORDDETAIL_T[] GetFlowListByUser(string userId, string flagStatus)
        //{
        //    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();
        //    flowInfo.EditUserID = userId;
        //    flowInfo.Flag = flagStatus;     //审批标志，0为待审批,1为已审批完成
        //    flowInfo.CompanyID = "";
        //    flowInfo.CreateDate = System.DateTime.Now;
        //    flowInfo.FlowCode = "";
        //    flowInfo.FormID = "";           //保存的模块表ID
        //    flowInfo.GUID = "";             //
        //    flowInfo.InstanceID = "";
        //    flowInfo.ModelCode = "";        //模块代码
        //    flowInfo.OfficeID = "";         //岗位ID
        //    flowInfo.CreateUserID = "";     //创建流程用户ID
        //    flowInfo.StateCode = "";        //状态代码
        //    flowInfo.ParentStateCode = "";  //父状态代码
        //    //return workFlowWS.GetFlowInfo(flowInfo);
        //    return flowInfo;
        //}
        //private List<string> GetGuidList(FLOW_FLOWRECORDDETAIL_T[] flowList)
        //{
        //    List<string> guidStringList = new List<string>();
        //    foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
        //    {
        //        guidStringList.Add(f.GUID);
        //    }
        //    if (guidStringList.Count < 1)
        //    {
        //        return null;
        //    }
        //    return guidStringList;
        //}
        #endregion
        #endregion

        #region 会议类型管理
        
        [OperationContract]
        public List<T_OA_MEETINGTYPE> GetMeetingTypeInfoByID(string TypeId)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                IQueryable<T_OA_MEETINGTYPE> MeetingTypeList = MeetingTypeBll.GetMeetingMeetingTypeListByID(TypeId);
                if (MeetingTypeList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingTypeList.ToList();
                }
            }
        }


        
        [OperationContract]
        public List<T_OA_MEETINGTYPE> GetMeetingTypeInfos(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                IQueryable<T_OA_MEETINGTYPE> MeetingRoomList = MeetingTypeBll.GetMeetingTypeInfos(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

                return MeetingRoomList != null ? MeetingRoomList.ToList() : null;
            }
        }




        [OperationContract]
        public List<T_OA_MEETINGTYPE> GetMeetintTypeInfosListBySearch(T_OA_MEETINGTYPE searchMeetingTypeInfo)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                IQueryable<T_OA_MEETINGTYPE> MeetingTypeList = MeetingTypeBll.GetMeetingTypeListBySearch(searchMeetingTypeInfo);
                if (MeetingTypeList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingTypeList.ToList();
                }
            }
        }



        [OperationContract]
        public string MeetingTypeAdd(T_OA_MEETINGTYPE obj)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                return MeetingTypeBll.AddMeetingTypeInfo(obj);
            }
        }



        [OperationContract]
        public bool MeetingTypeDel(string TypeID)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                using (BumfCompanySendDocManagementBll bl=new BumfCompanySendDocManagementBll())
                {
                    string state="2";
                    bl.UpdateCheckStateBumfEngine(TypeID, state);
                }
                return MeetingTypeBll.DeleteMeetingTypeInfo(TypeID);
            }

        }


        [OperationContract]
        public string MeetingTypeBatchDel(string[] ArrTypeID)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                return MeetingTypeBll.BatchDeleteMeetingTypeInfo(ArrTypeID);
            }

        }



        [OperationContract]
        public void MeetingTypeUpdate(T_OA_MEETINGTYPE obj)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                MeetingTypeBll.UpdateMeetingTypeInfo(obj);
            }
        }

        [OperationContract]
        private bool IsExistMeetingType(string MeetingType)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                return MeetingTypeBll.GetMeetingTypeByMeetingType(MeetingType);
            }

        }

        [OperationContract]
        public T_OA_MEETINGTYPE GetMeetingTypeById(string TypeId)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                return MeetingTypeBll.GetMeetingTypeNameById(TypeId);
                //return TypeBll.GetSingleMeetingTypeInfoByTypeId(TypeId);
            }
        }

        [OperationContract]
        public T_OA_MEETINGTYPE GetMeetingTypeSingleInfoById(string TypeId)
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {

                return MeetingTypeBll.GetSingleMeetingTypeInfoByTypeId(TypeId);
            }
        }
        [OperationContract]
        public List<T_OA_MEETINGTYPE> GetMeetingTypeNameInfosToCombox()
        {
            using (MeetingTypeManagementBll MeetingTypeBll = new MeetingTypeManagementBll())
            {
                IQueryable<T_OA_MEETINGTYPE> TypeList = MeetingTypeBll.GetMeetingTypeNameInfos();
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
        #endregion

        #region 会议类型模板
        
        [OperationContract]
        public List<T_OA_MEETINGTEMPLATE> GetMeetingTypeTemplateInfoByID(string StrTemplateId)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                IQueryable<T_OA_MEETINGTEMPLATE> TypeTemplateList = TypeTemplateBll.GetMeetingTemplateListByID(StrTemplateId);
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


        //[OperationContract]
        //public List<T_OA_MEETINGTEMPLATE> GetTypeTemplateInfos()
        //{
        //    List<T_OA_MEETINGTEMPLATE> TypeTemplateList = TypeTemplateBll.GetMeetingTypeTemplateInfos();
        //    if (TypeTemplateList == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return TypeTemplateList;
        //    }
        //}

        [OperationContract]
        public List<T_OA_MEETINGTEMPLATE> GetTypeTemplateInfos(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                IQueryable<T_OA_MEETINGTEMPLATE> TypeTemplateList = TypeTemplateBll.GetMeetingTypeTemplateInfos(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

                return TypeTemplateList != null ? TypeTemplateList.ToList() : null;
            }
        }

        [OperationContract]
        public bool BatchDelMeetingTypeTemplateInfos(string[] TemplateIDs)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                return TypeTemplateBll.BatchDeleteMeetingTypeTemplateInfos(TemplateIDs);
            }
        }



        [OperationContract]
        //获取某一会议类型的所有模板名称
        public List<T_OA_MEETINGTEMPLATE> GetMeetingTypeTemplateNameInfosByMeetingType(string StrMeetingType)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                List<T_OA_MEETINGTEMPLATE> TypeTemplateList = TypeTemplateBll.GetMeetingTemplateNameInfosByMeetingType(StrMeetingType);
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


        [OperationContract]
        public List<T_OA_MEETINGTEMPLATE> GetMeetintTypeTemplateInfosListBySearch(string StrMeetingType, string StrTemplateName, string StrContent)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                IQueryable<T_OA_MEETINGTEMPLATE> MeetingTypeList = TypeTemplateBll.GetMeetingTypeTemplateInfosListBySearch(StrMeetingType, StrTemplateName, StrContent);
                if (MeetingTypeList == null)
                {
                    return null;
                }
                else
                {
                    return MeetingTypeList.ToList();
                }
            }
        }



        [OperationContract]
        public string MeetingTypeTemplateAdd(T_OA_MEETINGTEMPLATE obj)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                return TypeTemplateBll.AddMeetingTemplateInfo(obj); ;
            }
        }


        [OperationContract]
        private bool IsExistMeetingTypeTemplate(string StrMeetingType, string StrTemplateName)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                return TypeTemplateBll.GetMeetingTemplateByTemplateNameAndType(StrMeetingType, StrTemplateName);
            }

        }



        [OperationContract]
        public bool MeetingTypeTemplateDel(string StrTemplateID)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                return TypeTemplateBll.DeleteMeetingTemplateInfo(StrTemplateID);
            }
        }


        [OperationContract]
        public string MeetingTypeTemplateUpdate(T_OA_MEETINGTEMPLATE obj)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                return TypeTemplateBll.UpdateMeetingTemplateInfo(obj);
            }
        }




        [OperationContract]
        public T_OA_MEETINGTEMPLATE GetMeetingTypeTemplateSingleInfoById(string StrTemplateId)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                return TypeTemplateBll.GetMeetingTemplateNameById(StrTemplateId);
            }
        }

        [OperationContract]
        public string GetMeetingTypeTemplateContentInfoByMeetingTypeAndTemplateName(string StrTemplateName, string StrType)
        {
            using (MeetingTemplateManagementBll TypeTemplateBll = new MeetingTemplateManagementBll())
            {
                return TypeTemplateBll.GetMeetingTemplateContentByTemplateNameAndType(StrType, StrTemplateName);
            }
        }
        #endregion

        #region 会议内容管理
        
        [OperationContract]
        public string MeetingContentAdd(T_OA_MEETINGCONTENT obj)
        {
            using (MeetingContentManagementBll ContentBll = new MeetingContentManagementBll())
            {
                return ContentBll.AddMeetingContentInfo(obj);
            }
        }

        [OperationContract]
        public string BatchAddMeetingContentInfos(List<T_OA_MEETINGCONTENT> obj)
        {
            using (MeetingContentManagementBll ContentBll = new MeetingContentManagementBll())
            {
                string returnStr = "";

                foreach (T_OA_MEETINGCONTENT ContentT in obj)
                {
                    returnStr = ContentBll.AddMeetingContentInfo(ContentT);
                }

                return returnStr;
            }
        }



        [OperationContract]
        private bool IsExistMeetingContentInfo(string StrMeetingType, string StruserID)
        {
            using (MeetingContentManagementBll ContentBll = new MeetingContentManagementBll())
            {
                return ContentBll.GetMeetingConetentInfoByMeetingInfoIDAndUserID(StrMeetingType, StruserID);
            }
        }



        [OperationContract]
        public bool MeetingContentDel(string StrContentID)
        {
            using (MeetingContentManagementBll ContentBll = new MeetingContentManagementBll())
            {
                return ContentBll.DeleteMeetingContentInfo(StrContentID);
            }
        }

        [OperationContract]
        public void MeetingContentInfoUpdate(T_OA_MEETINGCONTENT obj)
        {
            using (MeetingContentManagementBll ContentBll = new MeetingContentManagementBll())
            {
                ContentBll.UpdateMeetingContentInfo(obj);
            }
        }

        [OperationContract]
        public T_OA_MEETINGCONTENT GetMeetingContentSingleInfoById(string StrContentId)
        {
            using (MeetingContentManagementBll ContentBll = new MeetingContentManagementBll())
            {
                return ContentBll.GetMeetingContentByContentId(StrContentId);
            }
        }

        [OperationContract]
        public List<T_OA_MEETINGCONTENT> GetMeetingContentInfosByMeetngInfoID(string StrMeetingInfoID)
        {
            using (MeetingContentManagementBll ContentBll = new MeetingContentManagementBll())
            {
                return ContentBll.GetMeetingContentInfosByMeetingInfoID(StrMeetingInfoID);
            }
        }
        #endregion


        

        
        
    }


   

}
