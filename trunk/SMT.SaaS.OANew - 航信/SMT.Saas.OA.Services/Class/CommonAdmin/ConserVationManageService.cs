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

namespace SMT.SaaS.OA.Services
{
    /// <summary>
    ///车辆 保养记录，申请
    /// </summary>
    public partial class SmtOACommonAdmin
    {
        

        //平台审核 进入
        [OperationContract]
        private T_OA_CONSERVATION Get_VConserVation(string id)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            return cvmBll.Get_VConserVation(id);
        }
        [OperationContract]
        private List<T_OA_CONSERVATION> GetConserVationList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, int pageCount, string companyId, string userId, string checkState)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            IQueryable<T_OA_CONSERVATION> conserVationList = null;
            if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                conserVationList = cvmBll.GetInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (checkState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_CONSERVATION", companyId, userId);//ConserVationForm
                if (flowList == null)
                {
                    return null;
                }
                List<string> guidStringList = new List<string>();
                foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                {
                    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T .FORMID);
                }
                if (guidStringList.Count < 1)
                {
                    return null;
                }
                conserVationList = cvmBll.GetInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
            }
            if (conserVationList == null)
            {
                return null;
            }
            else
            {
                return conserVationList.ToList();
            }
        }
         //查询
        [OperationContract]
        public IEnumerable<T_OA_CONSERVATION> Sel_VCCheckeds(int pageIndex, int pageSize, string sort, string filterString, object[] paras,  int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            return cvmBll.Sel_VCCheckeds(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userId, guidStringList, checkState);
        }

        [OperationContract]
        private int AddConserVation(T_OA_CONSERVATION cvInfo)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            if (cvmBll.AddInfo(cvInfo) == true)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]   //add by zl
        public int AddConserVation_i(T_OA_CONSERVATION cvInfo)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            if (cvmBll.AddInfo(cvInfo) == true)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        private int UpdateConserVation(T_OA_CONSERVATION cvInfo)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            if (cvmBll.UpdateInfo(cvInfo) == -1)
            {
                return -1;
            }
            return 1;
        }

        [OperationContract]
        private int DeleteConserVation(T_OA_CONSERVATION cvInfo)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            if (cvmBll.DeleteInfo(cvInfo) == true)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        private int DeleteConserVationList(List<T_OA_CONSERVATION> cvInfoList)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            foreach (T_OA_CONSERVATION cvInfo in cvInfoList)
            {
                if (cvmBll.DeleteInfo(cvInfo) != true)
                {
                    return -1;
                }
            }
            return 1;
        }
            #region 保养记录   

        //平台审核 进入
        [OperationContract]
        private T_OA_CONSERVATIONRECORD Get_VCRecord(string id)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
           return  cvmBll.Get_VCRecord(id);
        }
        [OperationContract]
        public IEnumerable<T_OA_CONSERVATIONRECORD> Get_VCRecords(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            IEnumerable<T_OA_CONSERVATIONRECORD> infoList = null;
            if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                infoList = cvmBll.Get_VCRecords(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, null, checkState);
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (checkState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_CONSERVATIONRECORD", loginUserInfo.companyID, loginUserInfo.userID);
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
                infoList = cvmBll.Get_VCRecords(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, guidStringList, checkState);
            }
            if (infoList == null)
            {
                return null;
            }
            else
            {
                return infoList.ToList();
            }
        }

        [OperationContract]
        public int Add_VCRecord(T_OA_CONSERVATIONRECORD info)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            return cvmBll.Add_VCRecord(info);
        }
        [OperationContract]
        public int Upd_VCRecord(T_OA_CONSERVATIONRECORD info)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            return cvmBll.Upd_VCRecord(info);
        }
        [OperationContract]
        public int Del_VCRecords(List<T_OA_CONSERVATIONRECORD> lst)
        {
            ConserVationManagementBll cvmBll = new ConserVationManagementBll();
            return cvmBll.Del_VCRecords(lst);
        }
            #endregion
    }
}
