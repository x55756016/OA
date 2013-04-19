using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOACommonAdmin
    {
        
        [OperationContract]
        private List<T_OA_ACCIDENTRECORD> GetAccidentRecordList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            AccidentRecordManageBll accidentRecordBll = new AccidentRecordManageBll();
            IEnumerable<T_OA_ACCIDENTRECORD> infoList = accidentRecordBll.GetInfoList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
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
        private int AddAccidentRecord(T_OA_ACCIDENTRECORD cvInfo)
        {
            AccidentRecordManageBll accidentRecordBll = new AccidentRecordManageBll();
            if (accidentRecordBll.AddInfo(cvInfo) == true)
            {
                return 1;
            }
            return -1;
        }

        [OperationContract]
        private int AddAccidentRecordList(List<T_OA_ACCIDENTRECORD> cvInfoList)
        {
            AccidentRecordManageBll accidentRecordBll = new AccidentRecordManageBll();
            foreach (T_OA_ACCIDENTRECORD cvInfo in cvInfoList)
            {
                if (accidentRecordBll.AddInfo(cvInfo) != true)
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        private int UpdateAccidentRecord(T_OA_ACCIDENTRECORD cvInfo)
        {
            AccidentRecordManageBll accidentRecordBll = new AccidentRecordManageBll();
            if (accidentRecordBll.UpdateInfo(cvInfo) == -1)
            {
                return -1;
            }
            return 1;
        }

        [OperationContract]
        private int UpdateAccidentRecordList(List<T_OA_ACCIDENTRECORD> cvInfoList)
        {
            AccidentRecordManageBll accidentRecordBll = new AccidentRecordManageBll();
            foreach (T_OA_ACCIDENTRECORD cvInfo in cvInfoList)
            {
                if (accidentRecordBll.UpdateInfo(cvInfo) == -1)
                {
                    return -1;
                }
            }
            return 1;
        }

        [OperationContract]
        private bool DeleteAccidentRecordList(string[] ApprovalInfoId)
        {
           AccidentRecordManageBll accidentRecordBll = new AccidentRecordManageBll();
            
            return accidentRecordBll.DeleteInfo(ApprovalInfoId);
        }
    }
}
