using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    public class AccidentRecordManageBll : BaseBll<T_OA_ACCIDENTRECORD>
    {
        AccidentRecordManageDal armDal = new AccidentRecordManageDal();
        public IQueryable<T_OA_ACCIDENTRECORD> GetInfoList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            var q = from ent in dal.GetObjects<T_OA_ACCIDENTRECORD>().Include("T_OA_VEHICLE")
                    select ent;
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAVEHICLEACCIDENT");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_ACCIDENTRECORD>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public bool AddInfo(T_OA_ACCIDENTRECORD ApprovalInfo)
        {
            return armDal.AddAccidentRecord(ApprovalInfo);

        }
        public bool DeleteInfo(string[] ApprovalInfoId)
        {
            try
            {
                var entitys = from ent in dal.GetObjects().ToList()
                             where ApprovalInfoId.Contains(ent.ACCIDENTRECORDID)
                             select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        dal.DeleteFromContext(obj);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
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
        public int UpdateInfo(T_OA_ACCIDENTRECORD ApprovalInfo)
        {
            return armDal.UpdateAccidentRecord(ApprovalInfo) ? 1 : -1;
        }
    }
}