using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL;
using System.Linq.Dynamic;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    /// <summary>
    /// 满意度调查
    /// </summary>
    public class SatisfactionBll : BaseBll<T_OA_SATISFACTIONMASTER>
    {
        #region 调查方案
        public IQueryable<V_Satisfaction> Get_SSurveys(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            try
            {
                var lt = dal.GetObjects<T_OA_SATISFACTIONMASTER>()
                    .Include("T_OA_SATISFACTIONDETAIL")
                    .Select(x=>x);
                var m = from master in dal.GetObjects<T_OA_SATISFACTIONMASTER>()
                        join sub in dal.GetObjects<T_OA_SATISFACTIONDETAIL>().Include("T_OA_SATISFACTIONMASTER")
                        on master.SATISFACTIONMASTERID equals sub.SATISFACTIONMASTERID into subject
                        select new V_Satisfaction
                        {
                            RequireMaster = master,
                            SubjectViewList = subject,
                            OWNERCOMPANYID = master.OWNERCOMPANYID,
                            CREATEUSERID = master.CREATEUSERID,
                            OWNERDEPARTMENTID = master.OWNERDEPARTMENTID,
                            OWNERID = master.OWNERID,
                            OWNERPOSTID = master.OWNERPOSTID
                        };
                if (checkState == "4")//审批人
                {
                    if (guidStringList != null)
                    {
                       
                           
                        m = from ent in m
                            where guidStringList.Contains(ent.RequireMaster.SATISFACTIONMASTERID)
                            select ent;
                        //m = m.ToList().Where(x => guidStringList.Contains(x.RequireMaster.SATISFACTIONMASTERID)).AsQueryable();
                    }
                }
                else//创建人
                {
                    m = m.Where(ent => ent.RequireMaster.CREATEUSERID == userId);
                    if (checkState != "5")
                        m = m.Where(ent => ent.RequireMaster.CHECKSTATE == checkState);
                }

                List<object> queryParas = new List<object>();
                if (paras != null)
                    queryParas.AddRange(paras);

                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_SATISFACTIONMASTER");
                if (!string.IsNullOrEmpty(filterString))
                    m = m.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                m = m.OrderBy(sort);
                m = Utility.Pager<V_Satisfaction>(m, pageIndex, pageSize, ref pageCount);
                if (m.Count() > 0)
                {
                    return m;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Get_SSurveys" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }
        //审核通过的调查方案
        public IQueryable<V_Satisfaction> Get_SSurveyChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            DateTime d = DateTime.Parse(paras[0].ToString()).Date;
            DateTime d2 = DateTime.Parse(paras[1].ToString()).AddDays(1);
            var m = from master in dal.GetObjects<T_OA_SATISFACTIONMASTER>()
                    join sub in dal.GetObjects<T_OA_SATISFACTIONDETAIL>().Include("T_OA_SATISFACTIONMASTER")
                    on master.SATISFACTIONMASTERID equals sub.SATISFACTIONMASTERID into subject
                    select new V_Satisfaction { RequireMaster = master, SubjectViewList = subject };

            m = m.Where(ent => ent.RequireMaster.CHECKSTATE == checkState && ent.RequireMaster.CREATEDATE >= d && ent.RequireMaster.CREATEDATE < d2);
            return m;
        }
        //参与调查时用 
        public IQueryable<V_Satisfaction> Get_SSurvey(string requireMasterID)
        {
            var m = from master in dal.GetObjects<T_OA_SATISFACTIONMASTER>()
                    join sub in dal.GetObjects<T_OA_SATISFACTIONDETAIL>().Include("T_OA_SATISFACTIONMASTER")
                    on master.SATISFACTIONMASTERID equals sub.SATISFACTIONMASTERID into subject
                    select new V_Satisfaction { RequireMaster = master, SubjectViewList = subject };

            m = m.Where(ent => ent.RequireMaster.SATISFACTIONMASTERID == requireMasterID);
            return m;
        }
        //删除 满意度方案 
        public int Del_SSurveys(List<V_Satisfaction> infoViewList)
        {
            try
            {
                this.BeginTransaction();
                foreach (V_Satisfaction i in infoViewList)
                {
                    foreach (T_OA_SATISFACTIONDETAIL j in i.SubjectViewList)
                    {
                        if (Del_SSurveySub(j.SATISFACTIONDETAILID) < 1)
                        {
                            this.RollbackTransaction();
                            return -1;
                        }
                    }
                    if (Del_SSurvey(i.RequireMaster.SATISFACTIONMASTERID) < 1)
                    {
                        this.RollbackTransaction();
                        return -1;
                    }
                }
                this.CommitTransaction();
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Del_SSurveys" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        //del方案
        public int Del_SSurvey(string id)
        {  
            int n = 0;
            try
            {
                var entitys = (from ent in dal.GetObjects<T_OA_SATISFACTIONMASTER>()
                               where ent.SATISFACTIONMASTERID == id
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    n = dal.Delete(entity);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Del_SSurvey" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return n;
                throw (ex);
            }
            return n;
        }
        /// <summary>
        /// 删除题目
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public int Del_SSurveySub(string id)
        {
            int n = 0;
            try
            {
                var entitys = (from ent in dal.GetObjects<T_OA_SATISFACTIONDETAIL>()
                               where ent.SATISFACTIONDETAILID == id
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    n = dal.Delete(entity);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Del_SSurveySub" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return n;
                throw (ex);
            }
            return n;
        }
        /// 根据 方案获取 题目
        public IQueryable<T_OA_SATISFACTIONDETAIL> Get_SubByParentID(string parentID)
        {
            var s = from subject in dal.GetObjects<T_OA_SATISFACTIONDETAIL>().Include("T_OA_SATISFACTIONMASTER")
                    where subject.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID == parentID
                    select subject;
            if (s.Count() > 0)
                return s;
            return null;
        }
        //更新审核状态
        public int Upd_SSurveyChecked(V_Satisfaction infoView)
        {
            try
            {
                SatisfactionDal satDal = new SatisfactionDal();
                if (satDal.Upd_SSurvey(infoView.RequireMaster) > 0)
                    return 1;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Upd_SSurveyChecked" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public bool Add_SatisfactionMaster(T_OA_SATISFACTIONMASTER key)
        {
            using (SatisfactionDal dal = new SatisfactionDal())
            {
                bool bl = dal.Add_SurveySatisfactionsMaster(key);
                if (bl == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 更新方案 
        /// </summary>
        /// <param name="requireMasterInfo"></param>
        /// <returns></returns>
        public int Upd_SSurveys(T_OA_SATISFACTIONMASTER requireMasterInfo)
        {
            int n = 0;
            try
            {
                if (requireMasterInfo.EntityKey == null)
                    requireMasterInfo.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_REQUIREMASTER", "SATISFACTIONMASTERID", requireMasterInfo.SATISFACTIONMASTERID);
                T_OA_SATISFACTIONMASTER tmpobj = dal.GetObjectByEntityKey(requireMasterInfo.EntityKey) as T_OA_SATISFACTIONMASTER;
                n = Update(requireMasterInfo);
                //n = SaveContextChanges();
                n = n == 0 ? 1 : n; // 没有任何更新时，SaveChanges会返回0
            }
            catch (Exception ex)
            {
                return n;
                throw (ex);
            }
            return n;
        }

        //更新方案、题目
        public int Upd_SSurvey(V_Satisfaction infoView)
        {
            try
            {
                SatisfactionDal satDal = new SatisfactionDal();
                this.BeginTransaction();
                //更新方案
                if (Upd_SSurveys(infoView.RequireMaster) < 1)
                {
                    this.RollbackTransaction();
                    return -1;
                }
                //题目 添加、更新
                List<T_OA_SATISFACTIONDETAIL> subjectViewList = Get_SubByParentID(infoView.RequireMaster.SATISFACTIONMASTERID).ToList();
                //////删除
                IEnumerable<T_OA_SATISFACTIONDETAIL> lstsub = infoView.SubjectViewList;
                foreach (T_OA_SATISFACTIONDETAIL i in lstsub)
                {
                    bool isAdd = true;

                    foreach (T_OA_SATISFACTIONDETAIL v in subjectViewList)
                    {
                        if (i.SATISFACTIONDETAILID.Equals(v.SATISFACTIONDETAILID))
                        { isAdd = false; break; }
                    }
                    if (isAdd)
                    {      //添加   题目  
                        Utility.RefreshEntity(i);
                        if (dal.Add(i) < 1)
                        {
                            this.RollbackTransaction();
                            return -1;
                        }
                    }
                    else
                    {
                        //更新题目
                        if (satDal.Upd_SSurveySub(i) < 1)
                        {
                            this.RollbackTransaction();
                            return -1;
                        }
                    }
                }
                this.CommitTransaction();
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Upd_SSurvey" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        public int Add_SSurvey(V_Satisfaction v)
        {
            try
            {
                int n = 0;
                Utility.RefreshEntity(v.RequireMaster);
                n += dal.Add(v.RequireMaster);
                foreach (T_OA_SATISFACTIONDETAIL subjectView in v.SubjectViewList)
                {
                    Utility.RefreshEntity(subjectView);
                    //n += dal.Add(subjectView);
                    dal.AddToContext(subjectView);
                }
                n = dal.SaveContextChanges();
                return n;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Add_SSurvey" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        #endregion 

        #region 调查申请
        public T_OA_SATISFACTIONREQUIRE Get_SSurveyApp(string id)
        {
            try
            {
                var m = from master in dal.GetObjects<T_OA_SATISFACTIONREQUIRE>().Include("T_OA_SATISFACTIONMASTER") where master.SATISFACTIONREQUIREID == id select master;
                if (m.Count() > 0)
                    return m.ToList()[0];
                else
                    return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Add_SSurvey" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        public IQueryable<T_OA_SATISFACTIONREQUIRE> Get_SSurveyApps(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var m = from master in dal.GetObjects<T_OA_SATISFACTIONREQUIRE>().Include("T_OA_SATISFACTIONMASTER")
                    select master;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    m = from ent in m
                        where guidStringList.Contains(ent.SATISFACTIONREQUIREID)
                        select ent;
                    //m = m.ToList().Where(x => guidStringList.Contains(x.SATISFACTIONREQUIREID)).AsQueryable();
                }
            }
            else//创建人
            {
                m = m.Where(ent => ent.CREATEUSERID == userId);
                if (checkState != "5")
                {
                    m = m.Where(ent => ent.CHECKSTATE == checkState);
                }
            }
            List<object> queryParas = new List<object>();
            if (paras != null)
            {
                queryParas.AddRange(paras);
            }
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_SATISFACTIONREQUIRE");
            if (!string.IsNullOrEmpty(filterString))
            {
                m = m.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            m = m.OrderBy(sort);
            m = Utility.Pager<T_OA_SATISFACTIONREQUIRE>(m, pageIndex, pageSize, ref pageCount);
            if (m.Count() > 0)
            {
                return m;
            }
            return null;
        }
        //审核通过的调查申请单
        public IQueryable<T_OA_SATISFACTIONREQUIRE> Get_SSurveyAppChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            if (paras != null && paras.Count() > 1)
            {
                DateTime d = DateTime.Parse(paras[0].ToString());
                DateTime d2 = DateTime.Parse(paras[1].ToString()).AddDays(1);
                var q = from ent in dal.GetObjects<T_OA_SATISFACTIONREQUIRE>().Include("T_OA_SATISFACTIONMASTER")
                        where ent.CHECKSTATE == checkState && ent.STARTDATE >= d && ent.STARTDATE < d2
                        select ent;
                return q;
            }
            else
            {
                var q = from ent in dal.GetObjects<T_OA_SATISFACTIONREQUIRE>().Include("T_OA_SATISFACTIONMASTER")
                        where ent.CHECKSTATE == checkState
                        select ent;
                return q;
            }
        }
        /// <summary>
        /// 参与的满意度调查方案
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userId"></param>
        /// <param name="guidStringList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public IQueryable<V_MyStaticfaction> Get_SaticfactionSurveyAppChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState, string postID, string companyID, string departmentID)
        {
            try
            {

                var ents = (from a in dal.GetObjects<T_OA_SATISFACTIONREQUIRE>().Include("T_OA_SATISFACTIONMASTER")
                            join b in dal.GetObjects<T_OA_SATISFACTIONMASTER>() on a.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID equals b.SATISFACTIONMASTERID
                            join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on a.SATISFACTIONREQUIREID equals c.FORMID
                            where a.CHECKSTATE==checkState
                            orderby a.CREATEDATE descending
                            select new V_MyStaticfaction
                            {
                                OARequire = a,
                                OAMaster = b,
                                distrbuteuser = c,
                                OWNERCOMPANYID = a.OWNERCOMPANYID,
                                OWNERID = a.OWNERID,
                                OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                OWNERPOSTID = a.OWNERPOSTID
                            });
                if (ents.Count() > 0)
                {


                    ents = ents.Where(s => (s.OARequire.CHECKSTATE == "2") &&
                        (s.distrbuteuser.VIEWER == userId
                        || s.distrbuteuser.VIEWER == postID
                        || s.distrbuteuser.VIEWER == departmentID
                        || s.distrbuteuser.VIEWER == companyID));
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);

                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }

                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_MyStaticfaction>(ents, pageIndex, pageSize, ref pageCount);
                    return ents;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Get_SaticfactionSurveyAppChecked" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }

        public int Add_SSurveyApp(T_OA_SATISFACTIONREQUIRE info)
        {
            try
            {
                Utility.RefreshEntity(info);
                return dal.Add(info);
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Add_SSurveyApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }

        }
        // 添加发布对象的修改
        public int Upd_SSurveyApp(T_OA_SATISFACTIONREQUIRE info)
        {
            try
            {
                SatisfactionDal satDal = new SatisfactionDal();
                return satDal.Upd_SSurveyApp(info);
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Upd_SSurveyApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public int Del_SSurveyApp(List<T_OA_SATISFACTIONREQUIRE> lst)
        {
       
            try
            {
                foreach (T_OA_SATISFACTIONREQUIRE i in lst)
                {
                    var entitys = (from ent in dal.GetObjects<T_OA_SATISFACTIONREQUIRE>()
                                   where ent.SATISFACTIONREQUIREID == i.SATISFACTIONREQUIREID
                                   select ent);
                    if (entitys.Count() > 0)
                    {
                        var entity = entitys.FirstOrDefault();
                        dal.DeleteFromContext(entity);
                    }
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Del_SSurveyApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
                throw (ex);
            }
           
        }
        #endregion 调查申请

        #region 调查发布
        public T_OA_SATISFACTIONDISTRIBUTE Get_SSurveyResult(string id)
        {
            var m = from master in dal.GetObjects<T_OA_SATISFACTIONDISTRIBUTE>().Include("T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER") where master.SATISFACTIONDISTRIBUTEID == id select master;
            if (m.Count() > 0)
                return m.ToList()[0];
            else
                return null;
        }
        public IQueryable<T_OA_SATISFACTIONDISTRIBUTE> Get_SSurveyResults(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var m = from master in dal.GetObjects<T_OA_SATISFACTIONDISTRIBUTE>().Include("T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER")
                    select master;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    m = from ent in m
                        where guidStringList.Contains(ent.SATISFACTIONDISTRIBUTEID)
                        select ent;
                    //m = m.ToList().Where(x => guidStringList.Contains(x.SATISFACTIONDISTRIBUTEID)).AsQueryable();
                }
            }
            else//创建人
            {
                m = m.Where(ent => ent.CREATEUSERID == userId);
                if (checkState != "5")
                    m = m.Where(ent => ent.CHECKSTATE == checkState);
            }
            List<object> queryParas = new List<object>();
            if (paras != null)
                queryParas.AddRange(paras);

            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_SATISFACTIONDISTRIBUTE");
            if (!string.IsNullOrEmpty(filterString))
                m = m.ToList().AsQueryable().Where(filterString, queryParas.ToArray());

            m = m.OrderBy(sort);
            m = Utility.Pager<T_OA_SATISFACTIONDISTRIBUTE>(m, pageIndex, pageSize, ref pageCount);
            if (m.Count() > 0)
                return m;
            return null;
        }

        public int Add_SSurveyResult(T_OA_SATISFACTIONDISTRIBUTE info)
        {
            try
            {
                Utility.RefreshEntity(info);
                return dal.Add(info);
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Add_SSurveyResult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        public int Upd_SSurveyResult(T_OA_SATISFACTIONDISTRIBUTE info)
        {
            try
            {
                SatisfactionDal satDal = new SatisfactionDal();
                return satDal.Upd_SSurveyResult(info);
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Upd_SSurveyResult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        public int Del_SSurveyResult(List<T_OA_SATISFACTIONDISTRIBUTE> lst)
        {
            int n = 0;
            try
            {
                foreach (T_OA_SATISFACTIONDISTRIBUTE i in lst)
                {
                    var entitys = (from ent in dal.GetObjects<T_OA_SATISFACTIONDISTRIBUTE>()
                                   where ent.SATISFACTIONDISTRIBUTEID == i.SATISFACTIONDISTRIBUTEID
                                   select ent);
                    if (entitys.Count() > 0)
                    {
                        var entity = entitys.FirstOrDefault();
                        dal.DeleteFromContext(entity);
                    }
                }
                n = dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查SatisfactionBll-Del_SSurveyResult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
                throw (ex);
            }
            return n;
        }
        #endregion

        #region 调查结果
        //统计每个调查申请单的结果明细
        public List<V_SatisfactionResult> Result_SurveyByRequireID(string id)
        {
            List<V_SatisfactionResult> lstSub = new List<V_SatisfactionResult>();

            var v = from ent in dal.GetObjects<T_OA_SATISFACTIONRESULT>()
                    where ent.T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID == id
                    select ent;
            var subkeys = from ent in v group ent by ent.SUBJECTID into e select e;

            foreach (var i in subkeys)
            {
                V_SatisfactionResult sub = new V_SatisfactionResult();
                List<V_SSubjectAnswerResult> lstAnswer = new List<V_SSubjectAnswerResult>();

                var v2 = from ent in v where ent.SUBJECTID == i.Key select ent;
                var answerKeys = from ent in v2 where ent.SUBJECTID == i.Key group ent by ent.RESULT into e select e;
                foreach (var j in answerKeys)
                {
                    V_SSubjectAnswerResult info = new V_SSubjectAnswerResult();
                    var v3 = (from ent in v2 where ent.RESULT == j.Key select ent).Count();
                    info.AnswerCode = j.Key;
                    info.Count = int.Parse(v3.ToString());
                    lstAnswer.Add(info);
                }
                sub.SubjectID = i.Key;
                sub.LstAnswer = lstAnswer;
                lstSub.Add(sub);
            }

            return lstSub;
        }
        public int Result_SurveySubID(string surveyID, string subID, string resultValue)
        {
            var nCount = (from ent in dal.GetObjects<T_OA_SATISFACTIONRESULT>()
                          where ent.T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID == surveyID && ent.SUBJECTID == decimal.Parse(subID) && ent.RESULT == resultValue
                          orderby ent.SUBJECTID
                          select ent).Count();
            return nCount;
        }
        //保存调查结果
        public int Result_Save(List<T_OA_SATISFACTIONRESULT> objList)
        {
            int n = 0;
            string requiredid = objList[0].T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID;
            var ents = from ent in dal.GetObjects<T_OA_SATISFACTIONRESULT>()
                       where ent.T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID == requiredid
                       select ent;
            if (ents.Count() > 0)
            {
                foreach (T_OA_SATISFACTIONRESULT a in ents)
                {
                    dal.DeleteFromContext(a);
                }
            }
            dal.SaveContextChanges();
            foreach (T_OA_SATISFACTIONRESULT obj in objList)
            {
                var q = from ent in dal.GetObjects<T_OA_SATISFACTIONRESULT>()
                        where ent.SUBJECTID == obj.SUBJECTID && ent.T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID == obj.T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID
                        && ent.RESULT == obj.RESULT && ent.CONTENT == obj.CONTENT
                        select ent;

                Utility.RefreshEntity(obj);
                //n += dal.Add(obj);
                dal.AddToContext(obj);
            }
            n = dal.SaveContextChanges();
            return n;
        }
        //获取 某个方案 所有题目的所选答案
        public List<T_OA_SATISFACTIONRESULT> Result_SubByUserID(string userID, string surveyID)
        {
            var m = from ent in dal.GetObjects<T_OA_SATISFACTIONRESULT>().Include("T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER")
                    where ent.T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID == surveyID && ent.OWNERID == userID
                    select ent;
            if (m.Count() > 0)
                return m.ToList();
            return null;
        }
        //获取该次调查的人员名单
        public List<V_EmployeeID> Result_EmployeeByRequireID(string requireId)
        {
            var q = from ent in dal.GetObjects<T_OA_SATISFACTIONRESULT>().Include("T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER")
                    where ent.T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID == requireId
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
                return q.ToList();
            return null;
        }
        /// <summary>
        /// 获取员工的所有满意度调查结果 用来做参与调查时的 判断
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public IQueryable<T_OA_SATISFACTIONRESULT> GetStaticfactonVisistedByEmployID(string UserID)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_SATISFACTIONRESULT>()
                        where ent.OWNERID == UserID
                        select ent;
                return q;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }
        #endregion

    }
}
