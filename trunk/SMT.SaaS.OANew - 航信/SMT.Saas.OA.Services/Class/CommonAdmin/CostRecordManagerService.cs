using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
//using SMT.EntityFlowSys;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOACommonAdmin
    {
        
        [OperationContract]
        private List<T_OA_COSTRECORD> GetCostRecordList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            CostRecordManagementBll crmBll = new CostRecordManagementBll();
            IEnumerable<T_OA_COSTRECORD> infoList = crmBll.GetInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
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
        private int AddCostRecord(T_OA_COSTRECORD cvInfo)
        {
            CostRecordManagementBll crmBll = new CostRecordManagementBll();
            if (crmBll.AddInfo(cvInfo) == true)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        private int AddCostRecordList(List<T_OA_COSTRECORD> cvInfoList)
        {
            CostRecordManagementBll crmBll = new CostRecordManagementBll();
            foreach (T_OA_COSTRECORD cvInfo in cvInfoList)
            {
                if (crmBll.AddInfo(cvInfo) != true)
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        private int UpdateCostRecord(T_OA_COSTRECORD cvInfo)
        {
            CostRecordManagementBll crmBll = new CostRecordManagementBll();
            if (crmBll.UpdateInfo(cvInfo) == -1)
            {
                return -1;
            }
            return 1;
        }

        [OperationContract]
        private int UpdateCostRecordList(List<T_OA_COSTRECORD> cvInfoList)
        {
            CostRecordManagementBll crmBll = new CostRecordManagementBll();
            foreach (T_OA_COSTRECORD cvInfo in cvInfoList)
            {
                if (crmBll.UpdateInfo(cvInfo) == -1)
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        private int DeleteCostRecord(T_OA_COSTRECORD cvInfo)
        {
            CostRecordManagementBll crmBll = new CostRecordManagementBll();
            if (crmBll.DeleteInfo(cvInfo) == true)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        private int DeleteCostRecordList(List<T_OA_COSTRECORD> cvInfoList)
        {
            CostRecordManagementBll crmBll = new CostRecordManagementBll();
            foreach (T_OA_COSTRECORD cvInfo in cvInfoList)
            {
                if (crmBll.DeleteInfo(cvInfo) != true)
                {
                    return -1;
                }
            }
            return 1;
        }
    }
}
