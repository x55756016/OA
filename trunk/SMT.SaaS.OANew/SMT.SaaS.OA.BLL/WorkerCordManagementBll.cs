using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Linq.Dynamic;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class WorkerCordManagementBll : BaseBll<T_OA_WORKRECORD>
    {

        public IQueryable<T_OA_WORKRECORD> GetWorkerCodeListByUserID(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            var q = from ent in dal.GetTable()
                    select ent;
            List<object> queryParas = new List<object>();
            if (paras != null)
            {
                queryParas.AddRange(paras);
            }
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_WORKRECORD");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_WORKRECORD>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public bool AddWorkCord(T_OA_WORKRECORD entity)
        {
            try
            {
                dal.AddToContext(entity);
                int i = dal.SaveContextChanges();
                if (i == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("日程管理WorkerCordManagementBll-AddWorkCord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        public bool DeleteWorkCord(string calendarGuid)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               //where ent.GUID == calendarGuid
                               where ent.WORKRECORDID == calendarGuid
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("日程管理WorkerCordManagementBll-DeleteWorkCord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        public void UpdateWorkCord(T_OA_WORKRECORD entity)
        {
            try
            {
                var workerCordList = from ent in dal.GetTable()
                                     //where ent.GUID == entity.GUID
                                     where ent.WORKRECORDID == entity.WORKRECORDID
                                     select ent;
                if (workerCordList.Count() > 0)
                {
                    var workerCordInfo = workerCordList.FirstOrDefault();
                    workerCordInfo.TITLE = entity.TITLE;
                    workerCordInfo.CONTENT = entity.CONTENT;
                    workerCordInfo.CREATEDATE = entity.CREATEDATE;
                    workerCordInfo.PLANTIME = entity.PLANTIME;
                    workerCordInfo.CREATEUSERID = entity.CREATEUSERID;
                    dal.Update(workerCordInfo);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("日程管理WorkerCordManagementBll-UpdateWorkCord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
    }
}