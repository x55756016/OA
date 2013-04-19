using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using System.Data.Objects;
using SMT.Foundation.Core;
using SMT.SaaS.OA.DAL.Views;
using System.Data;
using System.Data.Objects.DataClasses;
namespace SMT.SaaS.OA.DAL
{
    public class EmployeeSurveyViewDal : CommDaL<T_OA_REQUIREMASTER>
    {
        //SMT_OA_EFModelContext objModelContext = new SMT_OA_EFModelContext();
        public ObjectQuery<T_OA_REQUIREMASTER> GetMasterList()
        {
            return GetObjects<T_OA_REQUIREMASTER>();
        }
        public ObjectQuery<T_OA_REQUIREDETAIL2> GetSubjectList()
        {
            return GetObjects<T_OA_REQUIREMASTER>() == null ? null : GetObjects<T_OA_REQUIREDETAIL2>().Include("T_OA_REQUIREMASTER");
        }
        public ObjectQuery<T_OA_REQUIREDETAIL> GetAnswerList()
        {
            return GetObjects<T_OA_REQUIREMASTER>() == null ? null : GetObjects<T_OA_REQUIREDETAIL>();
        }
        #region 没用

        #region 结果
        public bool AddRequireResult(T_OA_REQUIRERESULT requireResultInfo)
        {
            try
            {
                requireResultInfo.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(requireResultInfo.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                int i = base.Add(requireResultInfo);
                return i > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public bool UpdateRequireResult(T_OA_REQUIRERESULT requireResultInfo)
        {
            try
            {
                T_OA_REQUIRERESULT tmpobj = base.GetObjectByEntityKey(requireResultInfo.EntityKey) as T_OA_REQUIRERESULT;
                tmpobj.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(requireResultInfo.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                int i = base.Update(requireResultInfo);
                return i >= 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        #region 答案
        public bool AddRequireAnswer(T_OA_REQUIREDETAIL requireRequireAnswer)
        {
            try
            {
                int i = base.Add(requireRequireAnswer);
                return i > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public int UpdateRequireAnswer(T_OA_REQUIREDETAIL requireRequireAnswer)
        {
            try
            {
                T_OA_REQUIREDETAIL tmpobj = base.GetObjectByEntityKey(requireRequireAnswer.EntityKey) as T_OA_REQUIREDETAIL;
                int i = base.Update(requireRequireAnswer);
                return i >= 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        #region 题目
        public bool AddSurveySubject(T_OA_REQUIREDETAIL2 surveySubjectInfo)
        {
            try
            {
                surveySubjectInfo.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                base.Add(surveySubjectInfo);
                int i = SaveContextChanges();
                return i > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        //更新题目
        public int UpdateSurveySubject(T_OA_REQUIREDETAIL2 surveySubjectInfo)
        {
            try
            {
                if (surveySubjectInfo.EntityKey == null)
                    surveySubjectInfo.EntityKey = new EntityKey
                        ("SMT_OA_EFModelContext" + ".T_OA_REQUIREDETAIL2", new Dictionary<string, object>  
                        { { "REQUIREMASTERID", surveySubjectInfo.REQUIREMASTERID }, 
                        { "SUBJECTID", surveySubjectInfo.SUBJECTID } });

                //T_OA_REQUIREDETAIL2 tmpobj = base.GetObjectByEntityKey(surveySubjectInfo.EntityKey) as T_OA_REQUIREDETAIL2;
                T_OA_REQUIREDETAIL2 tmpobj = base.GetObjects<T_OA_REQUIREDETAIL2>().Where(s => s.REQUIREDETAIL2ID == surveySubjectInfo.REQUIREDETAIL2ID).FirstOrDefault();
                if (surveySubjectInfo.T_OA_REQUIREMASTER != null)
                {
                    if (surveySubjectInfo.T_OA_REQUIREMASTER.EntityKey == null)
                    {
                        surveySubjectInfo.T_OA_REQUIREMASTER.EntityKey = new
                            System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_REQUIREMASTER", "REQUIREMASTERID", surveySubjectInfo.T_OA_REQUIREMASTER.REQUIREMASTERID);
                    }
                }
                if (tmpobj.T_OA_REQUIREMASTER != null)
                {
                    tmpobj.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                }
                base.UpdateFromContext(surveySubjectInfo);

                int i = SaveContextChanges();
                return i >= 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        #region 调查主表
        public bool AddRequireMaster(T_OA_REQUIREMASTER requireMasterInfo)
        {
            try
            {
                base.Add(requireMasterInfo);
                int i = SaveContextChanges();
                return i > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public int UpdateRequireMaster(T_OA_REQUIREMASTER requireMasterInfo)
        {
            try
            {
                if (requireMasterInfo.EntityKey == null)
                    requireMasterInfo.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_REQUIREMASTER", "REQUIREMASTERID", requireMasterInfo.REQUIREMASTERID);
                T_OA_REQUIREMASTER tmpobj = base.GetObjectByEntityKey(requireMasterInfo.EntityKey) as T_OA_REQUIREMASTER;

                base.UpdateFromContext(requireMasterInfo);

                int i = base.SaveContextChanges();
                return i >= 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        #region 题目视图
        public bool AddSubjectView(V_EmployeeSurveySubject requireSubjectView)
        {
            EmployeeSurveySubjectViewDal essvDal = new EmployeeSurveySubjectViewDal();
            return essvDal.AddSubjectView(requireSubjectView);
        }
        public bool UpdateSubjectView(V_EmployeeSurveySubject requireSubjectView)
        {
            EmployeeSurveySubjectViewDal essvDal = new EmployeeSurveySubjectViewDal();
            return essvDal.UpdateSubjectView(requireSubjectView);
        }
        #endregion
        #endregion

        #region 总视图
        // 无用
        public int AddEmployeeSurveyView(V_EmployeeSurvey employeeSurveyView)
        {
            int n = 0;
            base.Add(employeeSurveyView.RequireMaster);
            SaveContextChanges();

            foreach (V_EmployeeSurveySubject subjectView in employeeSurveyView.SubjectViewList)
            {
                employeeSurveyView.RequireMaster.T_OA_REQUIREDETAIL2.Add(subjectView.SubjectInfo);

                foreach (T_OA_REQUIREDETAIL anserInfo in subjectView.AnswerList)
                {
                    //base.Add(anserInfo);
                    base.AddToContext(anserInfo);
                    RefreshEntity(anserInfo);
                }
            }
            n = SaveContextChanges();

            return n;
        }
        // 无用
        public int UpdateEmployeeSurveyView(V_EmployeeSurvey employeeSurveyView)
        {
            try
            {
                T_OA_REQUIREMASTER tmpobj = base.GetObjectByEntityKey(employeeSurveyView.RequireMaster.EntityKey) as T_OA_REQUIREMASTER;
                base.Update(employeeSurveyView.RequireMaster);
                int m = SaveContextChanges();
                if (m < 0)
                {
                    return -1;
                }
                foreach (V_EmployeeSurveySubject subjectView in employeeSurveyView.SubjectViewList)
                {
                    foreach (T_OA_REQUIREDETAIL anserInfo in subjectView.AnswerList)
                    {
                        T_OA_REQUIREDETAIL answerTemObj = base.GetObjectByEntityKey(anserInfo.EntityKey) as T_OA_REQUIREDETAIL;
                        //base.Update(anserInfo);
                        base.UpdateFromContext(anserInfo);
                        //int i = SaveContextChanges();
                        //if (i < 1)
                        //{
                        //    return -1;
                        //}
                    }
                }
                return base.SaveContextChanges() > 0 ? 1 : -1;

            }
            catch (Exception ex)
            {
                return -1;
                throw (ex);
            }
        }

        public void RefreshEntity(EntityObject entity)
        {
            var rs = (entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();

            foreach (IRelatedEnd re in rs)
            {
                List<EntityObject> list = new List<EntityObject>();
                foreach (var item in re)
                {
                    list.Add(item as EntityObject);
                }
                list.ForEach(p =>
                {
                    if (re.GetType().BaseType == typeof(EntityReference))
                    {
                        EntityKey eKey = p.EntityKey;
                        if (eKey != null)
                        {
                            (re as EntityReference).EntityKey = eKey;
                            re.Remove(p);
                        }
                    }

                });
            }
        }

        // 无用
        public void DeleteEmployeeSurveyView(V_EmployeeSurvey employeeSurveyView)
        {

        }
        #endregion


        #region 调查申请
        /// <summary>
        /// 添加 无用
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int Add_ESurveyApp(T_OA_REQUIRE info)
        {
            int n = 0;
            try
            {
                n = base.Add(info);
            }
            catch (Exception ex)
            {
                return 0;
                throw (ex);
            }
            return n;
        }
        public int Upd_ESurveyApp(T_OA_REQUIRE info)
        {
            int n = 0;
            try
            {
                if (info.EntityKey == null)
                {
                    info.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(info.T_OA_REQUIREMASTER.EntityKey) as T_OA_REQUIREMASTER;
                    base.Add(info);
                }
                else
                {
                    T_OA_REQUIRE tmpobj = base.GetObjectByEntityKey(info.EntityKey) as T_OA_REQUIRE;
                    tmpobj.T_OA_REQUIREMASTER = base.GetObjectByEntityKey(info.T_OA_REQUIREMASTERReference.EntityKey) as T_OA_REQUIREMASTER;
                    base.UpdateFromContext(info);
                }
                n = SaveContextChanges();
                n = n == 0 ? 1 : n; // 没有任何更新时，SaveChanges会返回0
            }
            catch (Exception ex)
            {
                return 0;
                throw (ex);
            }
            return n;
        }
        #endregion

        #region 调查发布
        public int Upd_ESurveyResult(T_OA_REQUIREDISTRIBUTE info)
        {
            int n = 0;
            try
            {
                n = SaveContextChanges();
                n = n == 0 ? 1 : n; // 没有任何更新时，SaveChanges会返回0
            }
            catch (Exception ex)
            {
                return 0;
                throw (ex);
            }
            return n;
        }
        #endregion
    }
}