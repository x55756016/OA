using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

using SMT.SaaS.OA.DAL.Views;
using SMT.Foundation.Log;
namespace SMT.SaaS.OA.BLL
{
    public class EmployeeSurveysResultBll : BaseBll<T_OA_REQUIRERESULT>
    {
        public IQueryable<V_EmployeeID> GetEmployeeListByMasterId(string requireId)
        {
            var q = from ent in dal.GetTable()
                    where ent.T_OA_REQUIREMASTER.REQUIREMASTERID == requireId //&& ent.SUBJECTID == subjectId
                    group ent by new
                                    {
                                        ent.OWNERID,
                                        ent.OWNERNAME,
                                        ent.OWNERCOMPANYID,
                                        ent.OWNERDEPARTMENTID,
                                        ent.OWNERPOSTID
                                    }
                        into g
                        select new V_EmployeeID { EmployeeID = g.Key.OWNERID, EmployeeName = g.Key.OWNERNAME, EmployeeCompanyID = g.Key.OWNERCOMPANYID, EmployeeDepartmentID = g.Key.OWNERDEPARTMENTID, EmployeePostID = g.Key.OWNERPOSTID };
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public int GetResultCount(T_OA_REQUIREDETAIL entity)
        {
            //var nCount = (from ent in dal.GetTable()
            //              where ent.T_OA_REQUIREMASTER.REQUIREMASTERID == entity.REQUIREMASTERID && ent.SUBJECTID == entity.SUBJECTID && ent.RESULT == entity.CODE
            //              orderby ent.T_OA_REQUIREMASTER.REQUIREMASTERID, ent.SUBJECTID
            //              select ent).Count();
            //return nCount;
            return 0;
        }

        public IQueryable<T_OA_REQUIRERESULT> GetSubjectResultByUserID(string userID, string masterID)
        {
            var m = from ent in dal.GetTable()
                    where ent.T_OA_REQUIREMASTER.REQUIREMASTERID == masterID && ent.OWNERID == userID
                    select ent;
            if (m.Count() > 0)
            {
                return m;
            }
            return null;
        }
        EmployeeSurveysResultDal esrDal ;//= new EmployeeSurveysResultDal();
        public new bool Add(T_OA_REQUIRERESULT entity)
        {
            try
            {
                Utility.RefreshEntity(entity);
                return dal.Add(entity) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveysResultBll-Add" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        public bool Delete(string primaryKey)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.REQUIRERESULTID == primaryKey
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
                Tracer.Debug("员工调查EmployeeSurveysResultBll-Delete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>1 成功;0 没有更改 ;-1 数据连接失败</returns>
        public int Update(T_OA_REQUIRERESULT entity)
        {
            try
            {
                EmployeeSurveysResultDal esrDal = new EmployeeSurveysResultDal();
                return esrDal.UpdateRequireResult(entity) ? 1 : -1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveysResultBll-Update" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
    }
}