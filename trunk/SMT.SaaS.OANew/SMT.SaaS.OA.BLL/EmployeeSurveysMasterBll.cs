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
    public class EmployeeSurveysMasterBll : BaseBll<T_OA_REQUIREMASTER>
    {
        public IQueryable<T_OA_REQUIREMASTER> GetInfoListByOptFlag(string checkState)
        {
            if (checkState != "4")
            {
                var q = from ent in dal.GetTable()
                        where ent.CHECKSTATE == checkState
                        orderby ent.CHECKSTATE, ent.UPDATEDATE
                        select ent;
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            else//4 为全部
            {
                var q = from ent in dal.GetTable()
                        orderby ent.CHECKSTATE, ent.UPDATEDATE
                        select ent;
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
        }

        public IEnumerable<T_OA_REQUIREMASTER> GetInfoListByFlowFlag(string checkState, List<string> idList)
        {
            var q = from ent in dal.GetTable().ToList()
                    where ent.CHECKSTATE == checkState && idList.Contains(ent.REQUIREMASTERID)
                    orderby ent.CHECKSTATE, ent.UPDATEDATE
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }


        public IQueryable<T_OA_REQUIREMASTER> GetInfoListById(string masterId)
        {
            var q = from ent in dal.GetTable()
                    where ent.REQUIREMASTERID == masterId
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        public IQueryable<T_OA_REQUIREMASTER> GetInfoListByTitle(string masterTitle)
        {
            var q = from ent in dal.GetTable()
                    where ent.REQUIRETITLE == masterTitle
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            else
            {
                return null;
            }
        }

        public bool Add(T_OA_REQUIREMASTER entity)
        {
            try
            {
                int i = dal.Add(entity);
                if (i > 0)
                {
                    List<T_OA_REQUIREMASTER> mast = dal.GetObjects<T_OA_REQUIREMASTER>().ToList();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveysMasterBll-Add" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        public int AddEmployeeSurveysView(V_EmployeeSurvey employeeSurveyView)
        {
            try
            {
                BeginTransaction();
                int n = 0;

                foreach (V_EmployeeSurveySubject subjectView in employeeSurveyView.SubjectViewList)
                {
                    employeeSurveyView.RequireMaster.T_OA_REQUIREDETAIL2.Add(subjectView.SubjectInfo);
                }

                dal.Add(employeeSurveyView.RequireMaster);
                foreach (V_EmployeeSurveySubject subjectView in employeeSurveyView.SubjectViewList)
                {
                    foreach (T_OA_REQUIREDETAIL anserInfo in subjectView.AnswerList)
                    {
                        dal.AddToContext(anserInfo);
                        Utility.RefreshEntity(anserInfo);
                    }
                }
                n += dal.SaveContextChanges();
                CommitTransaction();
                return n;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveysMasterBll-AddEmployeeSurveysView" + System.DateTime.Now.ToString() + " " + ex.ToString());
                RollbackTransaction();
                return -1;
                throw (ex);
            }
        }

        public bool Delete(string primaryKey)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.REQUIREMASTERID == primaryKey
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
                Tracer.Debug("员工调查EmployeeSurveysMasterBll-Delete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>1 成功;0 没有更改 ;-1 数据连接失败</returns>
        public int UpdateMaster(T_OA_REQUIREMASTER entity)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.REQUIREMASTERID == entity.REQUIREMASTERID
                            select ent;
                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.CONTENT = entity.CONTENT;
                    user.CHECKSTATE = entity.CHECKSTATE;
                    user.CREATEDATE = entity.CREATEDATE;
                    user.CREATEUSERID = entity.CREATEUSERID;

                    //user.T_OA_REQUIREDETAIL2 = entity.T_OA_REQUIREDETAIL2;
                    //user.T_OA_REQUIRERESULT = entity.T_OA_REQUIRERESULT;
                    user.REQUIRETITLE = entity.REQUIRETITLE;
                    user.UPDATEDATE = entity.UPDATEDATE;
                    user.UPDATEUSERID = entity.UPDATEUSERID;
                    user.UPDATEUSERNAME = entity.UPDATEUSERNAME;

                    if (dal.Update(user) == 1)
                    {
                        return 1;
                    }
                }
                return 0;
            }
            catch(Exception ex)
            {
                Tracer.Debug("员工调查EmployeeSurveysMasterBll-UpdateMaster" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
    }
}