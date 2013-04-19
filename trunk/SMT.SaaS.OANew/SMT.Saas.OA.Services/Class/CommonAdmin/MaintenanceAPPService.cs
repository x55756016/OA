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
    public partial class SmtOACommonAdmin
    {
        //T_OA_MAINTENANCEAPP
        
        //private SMT.SaaS.OA.Services.FlowService.ServiceClient workFlowWS = new ServiceClient();
        [OperationContract]
        private T_OA_MAINTENANCEAPP Get_VMApp(string id)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            return maBll.Get_VMApp(id);
        }
        //审核通过的申请单
        [OperationContract]
        private List<T_OA_MAINTENANCEAPP> Get_VMAppChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            return maBll.Get_VMAppChecked(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId);
        }
        [OperationContract]
        private List<T_OA_MAINTENANCEAPP> GetMaintenanceAppList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            IEnumerable<T_OA_MAINTENANCEAPP> maintenanceAppList = null;
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                maintenanceAppList = maBll.GetInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (checkState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_MAINTENANCEAPP", companyId, userId);
                if (flowList == null)
                {
                    return null;
                }
                List<string> guidStringList = new List<string>();
                foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                {
                    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T .FORMID );
                }
                if (guidStringList.Count < 1)
                {
                    return null;
                }
                maintenanceAppList = maBll.GetInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
            }
            if (maintenanceAppList == null)
            {
                return null;
            }
            else
            {
                return maintenanceAppList.ToList();
            }
        }

        [OperationContract]
        private int AddMaintenanceApp(T_OA_MAINTENANCEAPP cvInfo)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            if (maBll.AddInfo(cvInfo) == true)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        private int UpdateMaintenanceApp(T_OA_MAINTENANCEAPP cvInfo)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            if (maBll.UpdateInfo(cvInfo) == -1)
            {
                return -1;
            }
            return 1;
        }

        [OperationContract]
        private int DeleteMaintenanceApp(T_OA_MAINTENANCEAPP cvInfo)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            if (maBll.DeleteInfo(cvInfo) == true)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        private int DeleteMaintenanceAppList(List<T_OA_MAINTENANCEAPP> cvInfoList)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            foreach (T_OA_MAINTENANCEAPP cvInfo in cvInfoList)
            {
                if (maBll.DeleteInfo(cvInfo) != true)
                {
                    return -1;
                }
            }
            return 1;
        }
        #region 维修记录
        //平台审核 进入
        [OperationContract]
        private T_OA_MAINTENANCERECORD Get_VMRecord(string id)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            return maBll.Get_VMRecord(id);
        }
        [OperationContract]
        private List<T_OA_MAINTENANCERECORD> Get_VMRecords(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string companyId, string userId, string checkState)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            IEnumerable<T_OA_MAINTENANCERECORD> maintenanceAppList = null;
            if (checkState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                maintenanceAppList = maBll.Get_VMRecords(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, null, checkState);
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (checkState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_OA_MAINTENANCERECORD", companyId, userId);
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
                maintenanceAppList = maBll.Get_VMRecords(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId, guidStringList, checkState);
            }
            if (maintenanceAppList == null)
            {
                return null;
            }
            else
            {
                return maintenanceAppList.ToList();
            }
        }

        [OperationContract]
        public int Add_VMRecord(T_OA_MAINTENANCERECORD info)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            return maBll.Add_VMRecord(info);
        }
        [OperationContract]
        public int Upd_VMRecord(T_OA_MAINTENANCERECORD info)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            return maBll.Upd_VMRecord(info);
        }
        [OperationContract]
        public int Del_VMRecord(List<T_OA_MAINTENANCERECORD> lst)
        {
            MaintenanceAPPBll maBll = new MaintenanceAPPBll();
            return maBll.Del_VMRecord(lst);
        }

        #endregion 
    }
}