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
    public partial class SmtOAPersonOffice
    {

        [OperationContract]
        public List<T_OA_WORKRECORD> GetWorkerCordListByUserId(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId)
        {
            WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll();
            //using (WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll())
            //{
            IQueryable<T_OA_WORKRECORD> workerCordList = workerCordBll.GetWorkerCodeListByUserID(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userId);
            if (workerCordList == null)
            {
                return null;
            }
            else
            {
                return workerCordList.ToList();
            }
            //}
        }

        [OperationContract]
        public int DeleteWorkerCordList(List<T_OA_WORKRECORD> objList)
        {
            WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll();
            //using (WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll())
            //{
            foreach (T_OA_WORKRECORD obj in objList)
            {
                if (!workerCordBll.DeleteWorkCord(obj.WORKRECORDID))
                {
                    return -1;
                }
            }
            return 1;
            //}
        }


        [OperationContract]
        public int WorkerCordAdd(T_OA_WORKRECORD obj)
        {
            WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll();
            //using (WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll())
            //{
            bool sucess = workerCordBll.AddWorkCord(obj);
            if (sucess == false)
            {
                return -1;
            }
            return 1;
            //}
        }

        [OperationContract]
        public bool WorkerCordDel(T_OA_WORKRECORD obj)
        {
            WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll();
            //using (WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll())
            //{
            return workerCordBll.DeleteWorkCord(obj.WORKRECORDID);
            //}
        }

        [OperationContract]
        public void WorkerCordUpdate(T_OA_WORKRECORD obj)
        {
            WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll();
            //using (WorkerCordManagementBll workerCordBll = new WorkerCordManagementBll())
            //{
            workerCordBll.UpdateWorkCord(obj);
            //}
        }
    }
}