using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class EmployeeSurveysAnswerBll : BaseBll<T_OA_REQUIREDETAIL>
    {
        public IQueryable<T_OA_REQUIREDETAIL> GetInfoListByMasterId(string requireId, decimal subjectId)
        {
            var q = from ent in dal.GetTable()
                    where ent.REQUIREMASTERID == requireId && ent.SUBJECTID == subjectId
                    orderby ent.CODE, ent.SUBJECTID
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        public IQueryable<T_OA_REQUIREDETAIL> GetInfoByMasterId(string requireId)
        {
            var q = from ent in dal.GetTable()
                    where ent.REQUIREMASTERID == requireId 
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public int AddAnswer(T_OA_REQUIREDETAIL entity)
        {
            try
            {
                int i = dal.Add(entity);
                if (i > 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveysAnswerBll-AddAnswer" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        public bool DeleteEmployeeSurveysAnswer(string primaryKey)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.REQUIREDETAILID == primaryKey
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
                Tracer.Debug("员工调查EmployeeSurveysAnswerBll-DeleteEmployeeSurveysAnswer" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>1 成功;0 没有更改 ;-1 数据连接失败</returns>
        public int UpdateEmployeeSurveysAnswer(T_OA_REQUIREDETAIL entity)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.REQUIREDETAILID == entity.REQUIREDETAILID
                            select ent;
                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.CONTENT = entity.CONTENT;
                    user.CODE = entity.CODE;
                    //user.CREATEDATE = entity.CREATEDATE;
                    //user.CREATEUSERID = entity.CREATEUSERID;
                    //user.REQUIREID = entity.REQUIREID;
                    //user.SUBJECTID = entity.SUBJECTID;
                    user.UPDATEDATE = System.DateTime.Now;
                    user.UPDATEUSERID = entity.UPDATEUSERID;
                    user.UPDATEUSERNAME = entity.UPDATEUSERNAME;
                    if (dal.Update(user) == -1)
                    {
                        return -1;
                    }
                }
                else
                {
                    if (dal.Add(entity) == -1)
                    {
                        return -1;
                    }
                }
                return 1;
            }
            catch(Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveysAnswerBll-UpdateEmployeeSurveysAnswer" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public int GetAnswersCount(string requireId, decimal subjectId)
        {
            var q = from ent in dal.GetTable()
                    where ent.REQUIREMASTERID == requireId && ent.SUBJECTID == subjectId
                    orderby ent.CODE, ent.SUBJECTID
                    select ent;
            return q.Count();
        }
    }
}