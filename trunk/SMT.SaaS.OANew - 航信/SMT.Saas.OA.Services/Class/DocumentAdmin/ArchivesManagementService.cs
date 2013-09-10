

//*****************档案管理****************


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
using SMT.SaaS.OA;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOADocumentAdmin
    {
        
        
        
        //private SMT.SaaS.OA.Services.FlowService.ServiceClient workFlowWS = new ServiceClient();
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
        #region 档案信息管理
        //获取所有的档案信息
        [OperationContract]
        public List<T_OA_ARCHIVES> GetArchives(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (ArchivesManagementBll archivesBll = new ArchivesManagementBll())
            {
                //List<T_OA_ARCHIVES> ArchivesList = ArchivesBll.GetArchives().ToList<T_OA_ARCHIVES>;
                IQueryable<T_OA_ARCHIVES> ArchivesList = archivesBll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, "OAARCHIVES");//2010-7-22 liujx
                //IQueryable<T_OA_ARCHIVES> ArchivesList = archivesBll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, "T_OA_ARCHIVES");
                return ArchivesList == null ? null : ArchivesList.ToList();
            }
        }

        //根据档案ID获取档案信息
        [OperationContract]
        public T_OA_ARCHIVES GetArchivesById(string archivesID)
        {
            using (ArchivesManagementBll archivesBll = new ArchivesManagementBll())
            {
                T_OA_ARCHIVES MyArchives = archivesBll.GetArchivesById(archivesID);
                return MyArchives == null ? null : MyArchives;
            }
        }

        //获取最新一条档案信息
        [OperationContract]
        public T_OA_ARCHIVES GetLastArchives()
        {
            using (ArchivesManagementBll archivesBll = new ArchivesManagementBll())
            {
                T_OA_ARCHIVES myArchives = archivesBll.GetLastArchives();
                return myArchives == null ? null : myArchives;
            }
        }

        [OperationContract]
        //新增档案信息
        public string AddArchives(T_OA_ARCHIVES archivesInfo)
        {
            using (ArchivesManagementBll archivesBll = new ArchivesManagementBll())
            {
                string returnStr = "";

                if (!this.IsExistArchives(archivesInfo.ARCHIVESTITLE, archivesInfo.OWNERCOMPANYID))
                {
                    if (!archivesBll.AddArchives(archivesInfo))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "档案信息已经存在,添加数据失败";
                }
                return returnStr;
            }
        }

        [OperationContract]
        //更新档案信息
        public string UpdateArchives(T_OA_ARCHIVES archivesInfo)
        {
            using (ArchivesManagementBll archivesBll = new ArchivesManagementBll())
            {
                string returnStr = "";

                if (!archivesBll.UpdateArchives(archivesInfo))
                {
                    returnStr = "更新数据失败";
                }
                return returnStr;
            }
        }

        [OperationContract]
        //删除档案信息
        public string DeleteArchives(string[] archivesID, ref string errorMsg)
        {
            using (ArchivesManagementBll archivesBll = new ArchivesManagementBll())
            {
                string returnStr = "";

                if (!archivesBll.DeleteArchives(archivesID, ref errorMsg))
                {
                    returnStr = "删除数据失败";
                }
                return returnStr;
            }
        }

        [OperationContract]
        //根据条件查询档案信息
        public List<T_OA_ARCHIVES> GetArchivesInfosListBySearch(string type, string title)
        {
            using (ArchivesManagementBll archivesBll = new ArchivesManagementBll())
            {
                IQueryable<T_OA_ARCHIVES> ArchivesList = archivesBll.GetArchivesInfosListBySearch(type, title);
                return ArchivesList == null ? null : ArchivesList.ToList();
            }
        }

        [OperationContract]
        //根据条件查询档案信息
        private bool IsExistArchives(string title, string createUserID)
        {
            using (ArchivesManagementBll archivesBll = new ArchivesManagementBll())
            {
                return archivesBll.IsExistArchives(title, createUserID);
            }
        }
        #endregion

        #region 档案借阅管理
        //获取所有可以借阅档案信息
        [OperationContract]
        public List<T_OA_ARCHIVES> GetArchivesCanBorrow()
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                return archivesLendingBll.GetArchivesCanBorrow();
            }
        }

        //根据条件获取所有可以借阅档案信息
        [OperationContract]
        public List<T_OA_ARCHIVES> GetArchivesCanBorrowByCondition(string title, string type)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                return archivesLendingBll.GetArchivesCanBorrowByCondition(title, type);
            }
        }

        [OperationContract]
        //新增档案借阅信息
        public string AddArchivesLending(T_OA_LENDARCHIVES archivesLendingInfo)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                string returnStr = "";

                if (!archivesLendingBll.AddArchivesLending(archivesLendingInfo))
                {
                    returnStr = "添加数据失败";
                }
                return returnStr;
            }
        }

        [OperationContract]
        //更新档案借阅信息
        public string UpdateArchivesLending(T_OA_LENDARCHIVES archivesLendingInfo)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                string returnStr = "";


                if (!archivesLendingBll.UpdateArchivesLending(archivesLendingInfo))
                {
                    returnStr = "更新数据失败";
                }
                //}
                return returnStr;
            }
        }

        [OperationContract]
        //删除档案借阅信息
        public string DeleteArchivesLending(string[] lendingID)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                string returnStr = "";

                if (!archivesLendingBll.DeleteArchivesLening(lendingID))
                {
                    returnStr = "删除数据失败";
                }
                return returnStr;
            }
        }



        [OperationContract]
        //查询档案能否被更新
        public bool IsArchivesCanUpdate(T_OA_LENDARCHIVES archivesLendingInfo)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                return archivesLendingBll.IsArchivesCanUpdate(archivesLendingInfo);
            }
        }

        //查询用户借阅记录
        [OperationContract]
        public List<T_OA_LENDARCHIVES> GetLendingListByUserId(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                IQueryable<T_OA_LENDARCHIVES> ArchivesList = null;

                if (checkState != ((int)CheckStates.WaittingApproval).ToString())  //获取用户的提交借阅信息
                {
                    if (checkState != ((int)CheckStates.ALL).ToString())
                    {
                        ArchivesList = archivesLendingBll.GetArchivesLendingInfoQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, checkState, loginUserInfo.userID);
                    }
                    else
                    {
                        ArchivesList = archivesLendingBll.GetArchivesLendingInfoQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, null, "", loginUserInfo.userID);
                    }
                }
                else                    //通过工作流获取用户要审批的借阅信息
                {
                    V_ArchivesLending a = new V_ArchivesLending();
                    FLOW_FLOWRECORDDETAIL_T flowInfo = new FLOW_FLOWRECORDDETAIL_T();   //审核人 操作              
                    //FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "0", "archivesLending", loginUserInfo.companyID, loginUserInfo.userID);
                    FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", "0", "archivesLending", "", loginUserInfo.userID);
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
                    ArchivesList = archivesLendingBll.GetArchivesLendingInfoQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, flowAppList, checkState, loginUserInfo.userID);
                }
                return ArchivesList != null ? ArchivesList.ToList() : null;
            }
        }

        //根据借阅记录ID查询借阅记录
        [OperationContract]
        public List<T_OA_LENDARCHIVES> GetLendingListByLendingId(string lendingID)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                IQueryable<T_OA_LENDARCHIVES> ArchivesList = archivesLendingBll.GetArchivesLendingInfoById(lendingID);
                return ArchivesList != null ? ArchivesList.ToList() : null;
            }
        }



        //档案是否能被查看
        [OperationContract]
        public bool IsArchivesCanBrowser(string archivesID)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                return archivesLendingBll.IsArchivesCanBrowser(archivesID);
            }
        }
        #endregion

        #region 档案归还管理
        [OperationContract]
        //新增档案借阅信息
        public string AddArchivesReturn(T_OA_LENDARCHIVES archivesObj)
        {
            using (ArchivesReturnBll archivesReturnBll = new ArchivesReturnBll())
            {
                string returnStr = "";

                if (!archivesReturnBll.AddArchivesReturn(archivesObj))
                {
                    returnStr = "归还失败";
                }
                return returnStr;
            }
        }

        //查询用户已借档案信息
        [OperationContract]
        public List<T_OA_LENDARCHIVES> GetReturnListByUserId(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (ArchivesReturnBll archivesReturnBll = new ArchivesReturnBll())
            {
                return archivesReturnBll.GetArchivesReturnInfo(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
            }
        }

        //获取员工没有归还的信息
        [OperationContract]        
        public List<string> GetEmployeeNotReturnListByUserId(string StrEmployeeID)
        {
            using (ArchivesLendingBll archivesLendingBll = new ArchivesLendingBll())
            {
                return archivesLendingBll.GetEmployeeNotReturnData(StrEmployeeID);
            }
        }

        #endregion
    }
}
