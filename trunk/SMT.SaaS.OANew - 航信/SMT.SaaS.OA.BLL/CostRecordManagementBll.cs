using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    public class CostRecordManagementBll : BaseBll<T_OA_COSTRECORD>
    {
        //private CostRecordManagementDal dal = new CostRecordManagementDal();
        public IQueryable<T_OA_COSTRECORD> GetInfoList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            var q = from ent in dal.GetObjects<T_OA_COSTRECORD>().Include("T_OA_VEHICLE")
                    select ent;
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAVEHICLECOST");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_COSTRECORD>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public bool AddInfo(T_OA_COSTRECORD ApprovalInfo)
        {
            CostRecordManagementDal crDal = new CostRecordManagementDal();
            return crDal.AddCostRecord(ApprovalInfo);
        }
        CommDaL<T_OA_COSTRECORD> dal1 = new CommDaL<T_OA_COSTRECORD>();

        public bool DeleteInfo(T_OA_COSTRECORD ApprovalInfo)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects<T_OA_COSTRECORD>()
                               where ent.COSTRECORDID == ApprovalInfo.COSTRECORDID
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal1.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }
        public int UpdateInfo(T_OA_COSTRECORD ApprovalInfo)
        {
            CostRecordManagementDal crDal = new CostRecordManagementDal();
            return crDal.UpdateCostRecord(ApprovalInfo) == true ? 1 : -1;
        }
    }
}