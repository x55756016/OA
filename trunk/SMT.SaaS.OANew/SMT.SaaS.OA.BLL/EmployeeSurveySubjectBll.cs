using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.Foundation.Log;
namespace SMT.SaaS.OA.BLL
{
    public class EmployeeSurveySubjectBll : BaseBll<T_OA_REQUIREDETAIL2>
    {
        public IQueryable<T_OA_REQUIREDETAIL2> GetInfoListByMasterId(string requireId)
        {
            var q = from ent in dal.GetTable()
                    where ent.REQUIREMASTERID == requireId
                    orderby ent.SUBJECTID
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        public bool AddEmployeeSurveySubject(T_OA_REQUIREDETAIL2 entity)
        {
            try
            {
                EmployeeSurveySubjectDal essDal = new EmployeeSurveySubjectDal();
                return essDal.AddSurveySubject(entity);
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveySubjectBll-AddEmployeeSurveySubject" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        public bool DeleteEmployeeSurveySubject(string primaryKey)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.REQUIREDETAIL2ID == primaryKey
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
                Tracer.Debug("员工调查EmployeeSurveySubjectBll-DeleteEmployeeSurveySubject" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>1 成功;0 没有更改 ;-1 数据连接失败</returns>
        public int UpdateEmployeeSurveySubject(T_OA_REQUIREDETAIL2 entity)
        {
            try
            {
                EmployeeSurveySubjectDal essDal = new EmployeeSurveySubjectDal();
                return essDal.UpdateSurveySubject(entity) ? 1 : -1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveySubjectBll-UpdateEmployeeSurveySubject" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
    }
}