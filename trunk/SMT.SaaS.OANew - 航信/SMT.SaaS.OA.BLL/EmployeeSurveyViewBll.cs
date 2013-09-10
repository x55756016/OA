using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL;
using System.Linq.Dynamic;
using SMT.Foundation.Log;
using System.Data.Objects;
namespace SMT.SaaS.OA.BLL
{
    /// <summary>
    /// 调查方案，调查申请，调查发布模块用
    /// </summary>
    public class EmployeeSurveyViewBll : BaseBll<V_EmployeeSurvey>
    {
        #region 调查方案
        //public IQueryable<V_EmployeeSurvey> GetEmployeeSurveyViewListByFlag(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        //{
        //    EmployeeSurveyViewDal employeeSurveyViewDal = new EmployeeSurveyViewDal();
        //    var data = from master in dal.GetObjects<T_OA_REQUIREMASTER>()
        //               join sub in
        //                   (from subject in dal.GetObjects<T_OA_REQUIREDETAIL2>().Include("T_OA_REQUIREMASTER")
        //                    join ans in
        //                        (from answ in dal.GetObjects<T_OA_REQUIREDETAIL>()
        //                         orderby answ.CODE
        //                         select
        //                             answ)
        //                        on new { subject.REQUIREMASTERID, subject.SUBJECTID } equals new { ans.REQUIREMASTERID, ans.SUBJECTID } into answer
        //                    select new V_EmployeeSurveySubject { SubjectInfo = subject, AnswerList = answer })
        //              on master.REQUIREMASTERID equals sub.SubjectInfo.REQUIREMASTERID into
        //               subject

        //               select new V_EmployeeSurvey
        //               {
        //                   RequireMaster = master,
        //                   SubjectViewList = subject,
        //                   OWNERCOMPANYID = master.OWNERCOMPANYID,
        //                   OWNERDEPARTMENTID = master.OWNERDEPARTMENTID,
        //                   OWNERID = master.OWNERID,
        //                   OWNERPOSTID = master.OWNERPOSTID,
        //                   CREATEUSERID = master.CREATEUSERID
        //               };

        //    var m = from master in employeeSurveyViewDal.GetMasterList()
        //            //where master.CHECKSTATE == checkState
        //            join sub in
        //                (from subject in employeeSurveyViewDal.GetSubjectList()
        //                 join ans in
        //                     //employeeSurveyViewDal.GetAnswerList()
        //                     (from answ in employeeSurveyViewDal.GetAnswerList() orderby answ.CODE select answ)
        //                 on new { subject.REQUIREMASTERID, subject.SUBJECTID } equals new { ans.REQUIREMASTERID, ans.SUBJECTID } into answer
        //                 //orderby subject.SUBJECTID
        //                 select new V_EmployeeSurveySubject { SubjectInfo = subject, AnswerList = answer })
        //            on master.REQUIREMASTERID equals sub.SubjectInfo.REQUIREMASTERID into subject
        //            select new V_EmployeeSurvey
        //            {
        //                RequireMaster = master,
        //                SubjectViewList = subject,
        //                OWNERCOMPANYID = master.OWNERCOMPANYID,
        //                OWNERDEPARTMENTID = master.OWNERDEPARTMENTID,
        //                OWNERID = master.OWNERID,
        //                OWNERPOSTID = master.OWNERPOSTID,
        //                CREATEUSERID = master.CREATEUSERID
        //            };
        //    if (checkState == "4")//审批人
        //    {
        //        if (guidStringList != null)
        //        {
        //            m = from ent in m
        //                where guidStringList.Contains(ent.RequireMaster.REQUIREMASTERID)
        //                select ent;
        //            m = m.ToList().Where(x => guidStringList.Contains(x.RequireMaster.REQUIREMASTERID)).AsQueryable();
        //        }
        //    }
        //    else//创建人
        //    {
        //        //m = m.Where(ent => ent.RequireMaster.CREATEUSERID == userId);
        //        if (checkState != "5")
        //        {
        //            m = m.Where(ent => ent.RequireMaster.CHECKSTATE == checkState);
        //        }
        //    }
        //    List<object> queryParas = new List<object>();
        //    if (paras != null)
        //    {
        //        queryParas.AddRange(paras);
        //    }
        //    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "OAREQUIREMASTER");
        //    if (!string.IsNullOrEmpty(filterString))
        //    {
        //        //m = m.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
        //    }
        //    m = m.OrderBy(sort);
        //    m = Utility.Pager<V_EmployeeSurvey>(m, pageIndex, pageSize, ref pageCount);
        //    if (m.Count() > 0)
        //    {
        //        return m;
        //    }
        //    return null;
        //}

        /// <summary>
        /// 查询员工调查方案
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
        public IQueryable<V_EmployeeSurvey> GetEmployeeSurveyViewListByFlag(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var ents = from master in dal.GetObjects<T_OA_REQUIREMASTER>()
                    join sub in
                        (from subject in dal.GetObjects<T_OA_REQUIREDETAIL2>()
                         join ans in
                             (from answ in dal.GetObjects<T_OA_REQUIREDETAIL>() orderby answ.CODE select answ)
                         on new { subject.REQUIREMASTERID, subject.SUBJECTID } equals new { ans.REQUIREMASTERID, ans.SUBJECTID } into answer
                         select new V_EmployeeSurveySubject { SubjectInfo = subject, AnswerList = answer })
                    on master.REQUIREMASTERID equals sub.SubjectInfo.REQUIREMASTERID into subject
                    select new V_EmployeeSurvey
                    {
                        RequireMaster = master,
                        SubjectViewList = subject,
                        OWNERCOMPANYID = master.OWNERCOMPANYID,
                        OWNERDEPARTMENTID = master.OWNERDEPARTMENTID,
                        OWNERID = master.OWNERID,
                        OWNERPOSTID = master.OWNERPOSTID,
                        CREATEUSERID = master.CREATEUSERID
                    };
            List<object> queryParas = new List<object>();
            if (paras != null)
            {
                queryParas.AddRange(paras);
            }
            // 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            if (checkState != "4")
            {
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "OAREQUIREMASTER");

                if (checkState != "5")
                {
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        if (!string.IsNullOrEmpty(filterString))
                        {
                            filterString += " AND";
                        }

                        filterString += " RequireMaster.CHECKSTATE == @" + queryParas.Count();
                        queryParas.Add(checkState);
                    }
                }
            }
            else
            {
                UtilityClass.SetFilterWithflow("RequireMaster.REQUIREMASTERID", "OAREQUIREMASTER", userId, ref checkState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count())
                {
                    return null;
                }
            }

            if (queryParas.Count > 0)
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_EmployeeSurvey>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }

        //审核通过的调查方案
        public IQueryable<V_EmployeeSurvey> Get_ESurveyChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            EmployeeSurveyViewDal employeeSurveyViewDal = new EmployeeSurveyViewDal();
            DateTime d = DateTime.Parse(paras[0].ToString()).Date;
            DateTime d2 = DateTime.Parse(paras[1].ToString()).AddDays(1);
            var m = from master in employeeSurveyViewDal.GetMasterList()
                    join sub in
                        (from subject in employeeSurveyViewDal.GetSubjectList()
                         join ans in
                             (from answ in employeeSurveyViewDal.GetAnswerList() orderby answ.CREATEDATE select answ)
                         on new { subject.REQUIREMASTERID, subject.SUBJECTID } equals new { ans.REQUIREMASTERID, ans.SUBJECTID } into answer
                         select new V_EmployeeSurveySubject { SubjectInfo = subject, AnswerList = answer }
                         )
                    on master.REQUIREMASTERID equals sub.SubjectInfo.REQUIREMASTERID into subject
                    select new V_EmployeeSurvey { RequireMaster = master, SubjectViewList = subject };

            m = m.Where(ent => ent.RequireMaster.CHECKSTATE == checkState && ent.RequireMaster.CREATEDATE >= d && ent.RequireMaster.CREATEDATE < d2);
            return m;
        }
        //参与调查时用 ||平台审核进入时用
        public IQueryable<V_EmployeeSurvey> Get_ESurvey(string requireMasterID)
        {
            EmployeeSurveyViewDal employeeSurveyViewDal = new EmployeeSurveyViewDal();
            var vanswer = (from answ in employeeSurveyViewDal.GetAnswerList() orderby answ.CODE ascending select answ);
            var m = from master in employeeSurveyViewDal.GetMasterList()
                    join sub in
                        (from subject in employeeSurveyViewDal.GetSubjectList()
                         join ans in vanswer
                         on new { subject.REQUIREMASTERID, subject.SUBJECTID } equals new { ans.REQUIREMASTERID, ans.SUBJECTID } into answer
                         orderby subject.SUBJECTID ascending
                         select new V_EmployeeSurveySubject { SubjectInfo = subject, AnswerList = answer }
                         )
                    on master.REQUIREMASTERID equals sub.SubjectInfo.REQUIREMASTERID into subject
                    where master.REQUIREMASTERID == requireMasterID
                    select new V_EmployeeSurvey { RequireMaster = master, SubjectViewList = subject };

            //   m = m.Where(ent => ent.RequireMaster.REQUIREMASTERID == requireMasterID);
            return m;
        }
        private EmployeeSurveySubjectViewBll subjectViewBll = new EmployeeSurveySubjectViewBll();
        private EmployeeSurveysMasterBll masterBll = new EmployeeSurveysMasterBll();
        private EmployeeSurveysAnswerBll answerBll = new EmployeeSurveysAnswerBll();
        public int AddEmployeeSurveysView(V_EmployeeSurvey employeeSurveyView)
        {
            try
            {
                using (EmployeeSurveyViewDal employeeSurveyViewDal = new EmployeeSurveyViewDal())
                {
                    if (employeeSurveyViewDal.AddEmployeeSurveyView(employeeSurveyView) == -1)
                        return -1;
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查方案EmployeeSurveyViewBll-AddEmployeeSurveysView" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public int DeleteEmployeeSurveyViewList(List<V_EmployeeSurvey> infoViewList)
        {
            try
            {
                masterBll.BeginTransaction();
                foreach (V_EmployeeSurvey viewEmployeeSurvey in infoViewList)
                {
                    foreach (V_EmployeeSurveySubject viewSubject in viewEmployeeSurvey.SubjectViewList)
                    {
                        if (subjectViewBll.DeleteEmployeeSurveySubjectView(viewSubject) == -1)
                        {
                            masterBll.RollbackTransaction();
                            return -1;
                        }
                    }
                    if (!masterBll.Delete(viewEmployeeSurvey.RequireMaster.REQUIREMASTERID))
                    {
                        masterBll.RollbackTransaction();
                        return -1;
                    }
                }
                masterBll.CommitTransaction();
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查方案EmployeeSurveyViewBll-DeleteEmployeeSurveyViewList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        //更新方案 新
        public int Upd_ESurvey(T_OA_REQUIREMASTER survey, List<V_EmployeeSurveySubject> addLst, List<V_EmployeeSurveySubject> updLst)
        {
            try
            {
                //masterBll.BeginTransaction();
                //更新方案
                EmployeeSurveyViewDal esvDal = new EmployeeSurveyViewDal();
                if (esvDal.UpdateRequireMaster(survey) == -1)
                {
                    masterBll.RollbackTransaction();
                    return -1;
                }
                foreach (V_EmployeeSurveySubject i in addLst)
                {
                    //添加   题目   答案     
                    if (subjectViewBll.AddEmployeeSurveySubjectView(i) < 1)
                    {
                        masterBll.RollbackTransaction();
                        return -1;
                    }
                }
                foreach (V_EmployeeSurveySubject i in updLst)
                {
                    //更新题目
                    if (esvDal.UpdateSurveySubject(i.SubjectInfo) < 1)
                    {
                        masterBll.RollbackTransaction();
                        return -1;
                    }
                    //更新答案
                    if (UpdataAnswerList(i.SubjectInfo, i.AnswerList.ToList()) < 1)
                    {
                        masterBll.RollbackTransaction();
                        return -1;
                    }
                }
                masterBll.CommitTransaction();
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查方案EmployeeSurveyViewBll-Upd_ESurvey" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }

        }
        //EmployeeSurveyViewDal esvDal = new EmployeeSurveyViewDal();
        //更新方案 旧
        public int UpdateEmployeeSurveyView(V_EmployeeSurvey infoView)
        {
            try
            {
                EmployeeSurveyViewDal esvDal = new EmployeeSurveyViewDal();
                masterBll.BeginTransaction();
                if (esvDal.UpdateRequireMaster(infoView.RequireMaster) == -1)
                {
                    masterBll.RollbackTransaction();
                    return -1;
                }
                //题目 添加、更新
                List<V_EmployeeSurveySubject> subjectViewList = GetSubjectViewListByMasterID(infoView.RequireMaster.REQUIREMASTERID).ToList();

                //////删除
                IEnumerable<V_EmployeeSurveySubject> lstsub = infoView.SubjectViewList;
                foreach (V_EmployeeSurveySubject i in lstsub)
                {
                    bool isAdd = true;

                    foreach (V_EmployeeSurveySubject v in subjectViewList)
                    {
                        if (i.SubjectInfo.REQUIREDETAIL2ID == v.SubjectInfo.REQUIREDETAIL2ID)
                            isAdd = false; break;
                    }
                    if (isAdd)
                    {      //添加   题目   答案     
                        if (subjectViewBll.AddEmployeeSurveySubjectView(i) < 1)
                        {
                            masterBll.RollbackTransaction();
                            return -1;
                        }
                    }
                    else
                    {
                        //更新题目
                        if (esvDal.UpdateSurveySubject(i.SubjectInfo) < 1)
                        {
                            masterBll.RollbackTransaction();
                            return -1;
                        }
                        //更新答案
                        if (UpdataAnswerList(i.SubjectInfo, i.AnswerList.ToList()) < 1)
                        {
                            masterBll.RollbackTransaction();
                            return -1;
                        }
                    }
                }
                masterBll.CommitTransaction();
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查方案EmployeeSurveyViewBll-UpdateEmployeeSurveyView" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        /// <summary>
        /// 更新答案
        /// </summary>
        /// <param name="subjectInfo"></param>
        /// <param name="answerList"></param>
        /// <returns></returns>
        private int UpdataAnswerList(T_OA_REQUIREDETAIL2 subjectInfo, List<T_OA_REQUIREDETAIL> lstAns)
        {
            try
            {
                EmployeeSurveysAnswerBll answerBll = new EmployeeSurveysAnswerBll();

                for (int j = 0; j < lstAns.Count; j++)
                    if (answerBll.UpdateEmployeeSurveysAnswer(lstAns.ToList()[j]) == -1)
                        return -1;
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查方案EmployeeSurveyViewBll-UpdataAnswerList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public IQueryable<V_EmployeeSurveySubject> GetSubjectViewListByMasterID(string masterId)
        {
            EmployeeSurveyViewDal employeeSurveyViewDal = new EmployeeSurveyViewDal();
            var s = from subject in employeeSurveyViewDal.GetSubjectList()
                    where subject.REQUIREMASTERID == masterId
                    join ans in employeeSurveyViewDal.GetAnswerList() on new { subject.REQUIREMASTERID, subject.SUBJECTID } equals new { ans.REQUIREMASTERID, ans.SUBJECTID } into answer
                    select new V_EmployeeSurveySubject { SubjectInfo = subject, AnswerList = answer };
            if (s.Count() > 0)
            {
                return s;
            }
            return null;
        }

        /// <summary>
        /// 获取结果数量   //无用 10-7-8
        /// </summary>
        private EmployeeSurveysResultBll empSurveysResultBll = new EmployeeSurveysResultBll();
        public int GetResultCount(T_OA_REQUIREDETAIL obj)
        {
            int i = empSurveysResultBll.GetResultCount(obj);
            if (i > 0)
            {
                return i;
            }
            else
            {
                return 0;
            }
        }
        //统计每个调查申请单的结果明细
        public List<V_SatisfactionResult> Result_SurveyByRequireID(string id)
        {
            List<V_SatisfactionResult> lstSub = new List<V_SatisfactionResult>();

            var v = from ent in dal.GetObjects<T_OA_REQUIRERESULT>()
                    where ent.T_OA_REQUIRE.REQUIREID == id
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
                //sub.SubjectID = i.Key;
                sub.LstAnswer = lstAnswer;
                lstSub.Add(sub);
            }

            return lstSub;
        }
        public List<T_OA_REQUIRERESULT> GetSubjectResultByUserID(string userID, string masterID)
        {
            IQueryable<T_OA_REQUIRERESULT> subjectList = empSurveysResultBll.GetSubjectResultByUserID(userID, masterID);
            if (subjectList != null)
            {
                return subjectList.ToList();
            }
            return null;
        }

        public int AddResultInfoList(List<T_OA_REQUIRERESULT> objList)
        {
            try
            {
                //先将所有的结果删除  然后再添加
                string requiredid = objList[0].T_OA_REQUIRE.REQUIREID;
                var ents = from ent in dal.GetObjects<T_OA_REQUIRERESULT>()
                           where ent.T_OA_REQUIRE.REQUIREID == requiredid
                           select ent;
                if (ents.Count() > 0)
                {
                    foreach (T_OA_REQUIRERESULT a in ents)
                    {
                        dal.DeleteFromContext(a);
                    }
                }
                dal.SaveContextChanges();
                foreach (T_OA_REQUIRERESULT obj in objList)
                {
                    if (!empSurveysResultBll.Add(obj))
                    {
                        return -1;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查方案EmployeeSurveyViewBll-AddResultInfoList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
                throw (ex);
            }
        }

        public List<V_EmployeeID> GetEmployeeList(string reqID)
        {
            IQueryable<V_EmployeeID> empIdList = empSurveysResultBll.GetEmployeeListByMasterId(reqID);
            if (empIdList != null)
            {
                return empIdList.ToList();
            }
            return null;
        }

        #endregion 调查方案

        #region 调查申请

        public T_OA_REQUIRE Get_ESurveyApp(string id)
        {
            var m = from master in dal.GetObjects<T_OA_REQUIRE>().Include("T_OA_REQUIREMASTER") where master.REQUIREID == id select master;
            if (m.Count() > 0)
                return m.ToList()[0];
            else
                return null;
        }

        public List<T_OA_REQUIRE> Get_ESurveyApps(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var m = from master in dal.GetObjects<T_OA_REQUIRE>().Include("T_OA_REQUIREMASTER")
                    select master;

            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    m = from ent in m
                        where guidStringList.Contains(ent.REQUIREID)
                        select ent;
                    //m = m.ToList().Where(x => guidStringList.Contains(x.REQUIREID)).AsQueryable();
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
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "EmployeeSurveysApp");
            if (!string.IsNullOrEmpty(filterString))
            {
                m = m.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            m = m.OrderBy(sort);
            m = Utility.Pager<T_OA_REQUIRE>(m, pageIndex, pageSize, ref pageCount);
            if (m.Count() > 0)
            {
                return m.ToList();
            }
            return null;
        }

        /// <summary>
        /// 参与的员工调查
        /// </summary>
        public IQueryable<V_MyEusurvey> Get_MyVisistedSurvey(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState, string postID, string companyID, string departmentID)
        {
            try
            {
                string _checkState = Convert.ToInt32(CheckStates.Approved).ToString();
                var ents = from require in dal.GetObjects<T_OA_REQUIRE>()
                              .Include("T_OA_REQUIREMASTER")
                              .Include("T_OA_REQUIRERESULT")
                           join users in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                           on require.REQUIREID equals users.FORMID
                          // where require.STARTDATE <= DateTime.Now && require.CHECKSTATE == _checkState && require.ENDDATE >= DateTime.Now
                           select new V_MyEusurvey
                           {
                               OWNERCOMPANYID = require.OWNERCOMPANYID,
                               OWNERID = require.OWNERID,
                               OWNERDEPARTMENTID = require.OWNERDEPARTMENTID,
                               OWNERPOSTID = require.OWNERPOSTID,
                               OARequire = require,
                               OAResult = require.T_OA_REQUIRERESULT,
                               OAMaster = require.T_OA_REQUIREMASTER,
                               distrbuteuser = users
                           };

                if (ents.Count() > 0)
                {
                    //ents = ents.Where(s =>
                    //    s.distrbuteuser.VIEWER == userId
                    //    || s.distrbuteuser.VIEWER == postID
                    //    || s.distrbuteuser.VIEWER == departmentID
                    //    || s.distrbuteuser.VIEWER == companyID);
                    List<object> queryParas = new List<object>();
                    if (paras != null && paras.Count() > 0)
                    {

                        queryParas.AddRange(paras);
                    }

                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas);
                    }

                    //List<V_MyEusurvey> _delList = ents.ToList();
                    //foreach (var data in _delList)//如果已经参与过调查,则在结果中去除这条数据
                    //{
                    //    if (data.OAResult.Count() > 0)
                    //    {
                    //        _delList.Remove(data);
                    //    }
                    //}
                    ents = ents.AsQueryable().OrderBy(sort);
                    ents = Utility.Pager<V_MyEusurvey>(ents, pageIndex, pageSize, ref pageCount);
                    return ents;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查申请EmployeeSurveyViewBll-Get_MyVisistedSurvey" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }

        //审核通过的调查申请单
        public IQueryable<T_OA_REQUIRE> Get_ESurveyAppChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            DateTime d = DateTime.Parse(paras[0].ToString());
            DateTime d2 = DateTime.Parse(paras[1].ToString()).AddDays(1);
            var q = from ent in dal.GetObjects<T_OA_REQUIRE>().Include("T_OA_REQUIREMASTER")
                    where ent.CHECKSTATE == checkState && ent.STARTDATE >= d && ent.STARTDATE < d2
                    select ent;
            return q;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int Add_EsurveyApp(T_OA_REQUIRE info)
        {
            try
            {
                Utility.RefreshEntity(info);
                return dal.Add(info);
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查申请EmployeeSurveyViewBll-Add_EsurveyApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        public int Upd_ESurveyApp(T_OA_REQUIRE info)
        {
            try
            {
                EmployeeSurveyViewDal employeeSurveyViewDal = new EmployeeSurveyViewDal();
                return employeeSurveyViewDal.Upd_ESurveyApp(info);
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查申请EmployeeSurveyViewBll-Upd_ESurveyApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        //CommDaL<T_OA_REQUIRE> dal1 = new CommDaL<T_OA_REQUIRE>();
        public int Del_ESurveyApp(List<T_OA_REQUIRE> lst)
        {

            int n = 0;
            try
            {
                foreach (T_OA_REQUIRE i in lst)
                {
                    var entitys = (from ent in dal.GetObjects<T_OA_REQUIRE>()
                                   where ent.REQUIREID == i.REQUIREID
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
                Tracer.Debug("调查申请EmployeeSurveyViewBll-Del_ESurveyApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
                throw (ex);
            }
            return n;
        }
        #endregion

        #region 调查发布

        public T_OA_REQUIREDISTRIBUTE Get_ESurveyResult(string id)
        {
            var m = from master in dal.GetObjects<T_OA_REQUIREDISTRIBUTE>().Include("T_OA_REQUIRE.T_OA_REQUIREMASTER") where master.REQUIREDISTRIBUTEID == id select master;
            if (m.Count() > 0)
                return m.ToList()[0];
            else
                return null;
        }

        public IQueryable<T_OA_REQUIREDISTRIBUTE> Get_ESurveyResults(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var m = from master in dal.GetObjects<T_OA_REQUIREDISTRIBUTE>().Include("T_OA_REQUIRE.T_OA_REQUIREMASTER")
                    select master;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)

                    m = from ent in m
                        where guidStringList.Contains(ent.REQUIREDISTRIBUTEID)
                        select ent;
                //m = m.ToList().Where(x => guidStringList.Contains(x.REQUIREDISTRIBUTEID)).AsQueryable();
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

            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "EmployeeSurveysDistribute");
            if (!string.IsNullOrEmpty(filterString))
            {
                //m = m.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            m = m.OrderBy(sort);
            m = Utility.Pager<T_OA_REQUIREDISTRIBUTE>(m, pageIndex, pageSize, ref pageCount);
            if (m.Count() > 0)
                return m;
            return null;
        }

        public int Add_ESurveyResult(T_OA_REQUIREDISTRIBUTE info)
        {
            try
            {
                Utility.RefreshEntity(info);
                return dal.Add(info);
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查发布EmployeeSurveyViewBll-Add_ESurveyResult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        public int Upd_ESurveyResult(T_OA_REQUIREDISTRIBUTE info)
        {
            try
            {
                var users = from ent in dal.GetObjects<T_OA_REQUIREDISTRIBUTE>()
                            where ent.REQUIREDISTRIBUTEID == info.REQUIREDISTRIBUTEID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    Utility.CloneEntity(info, user);
                    int i = dal.Update(user);
                    if (i > 0)
                    {
                        return i;
                    }
                    else
                    {
                        return 0;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Tracer.Debug("调查发布EmployeeSurveyViewBll-Upd_ESurveyResult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
                throw (ex);
            }
        }
        //CommDaL<T_OA_REQUIREDISTRIBUTE> dal2 = new CommDaL<T_OA_REQUIREDISTRIBUTE>();
        public int Del_ESurveyResult(List<T_OA_REQUIREDISTRIBUTE> lst)
        {
            int n = 0;
            try
            {
                foreach (T_OA_REQUIREDISTRIBUTE i in lst)
                {
                    var entitys = (from ent in dal.GetObjects<T_OA_REQUIREDISTRIBUTE>()
                                   where ent.REQUIREDISTRIBUTEID == i.REQUIREDISTRIBUTEID
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
                Tracer.Debug("调查发布EmployeeSurveyViewBll-Del_ESurveyResult" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
                throw (ex);
            }
            return n;
        }

        /// <summary>
        /// 增加员工调查申请表
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="postID"></param>
        /// <param name="companyID"></param>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public bool EmployeeSurveyAppAdd(T_OA_REQUIRE key)
        {
            using (EmployeeSurveyViewDal appDal = new EmployeeSurveyViewDal())
            {
                if ((appDal != null) && (appDal.Add_ESurveyApp(key) == 1))
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
        /// 获取员工调查和满意度调查的发布结果
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="postID"></param>
        /// <param name="companyID"></param>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public IQueryable<V_SystemNotice> GetStaffSurveyInfo(string userID, string postID, string companyID, string departmentID)
        {
            var survey = from p in dal.GetObjects<T_OA_REQUIREDISTRIBUTE>().Include("T_OA_REQUIRE.T_OA_REQUIREMASTER")//员工调查发布
                         where p.CHECKSTATE == "2"
                         orderby p.CREATEDATE descending
                         select new V_SystemNotice { FormId = p.REQUIREDISTRIBUTEID, FormTitle = p.DISTRIBUTETITLE, Formtype = "StaffSurvey", FormDate = Convert.ToDateTime(p.CREATEDATE) };

            var surveys = from a in dal.GetObjects<T_OA_SATISFACTIONDISTRIBUTE>().Include("T_OA_SATISFACTIONREQUIRE.T_OA_SATISFACTIONMASTER")//满意度调查发布
                          where a.CHECKSTATE == "2"
                          orderby a.CREATEDATE descending
                          select new V_SystemNotice { FormId = a.SATISFACTIONDISTRIBUTEID, FormTitle = a.DISTRIBUTETITLE, Formtype = "SatisfactionSurvey", FormDate = Convert.ToDateTime(a.CREATEDATE) };

            var entity = survey.Union(surveys);

            return entity;
        }
        #endregion
    }
}