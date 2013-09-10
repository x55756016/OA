using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Log;
using SMT.SaaS.OA.DAL.Views;
using System.Data.Objects;

namespace SMT.SaaS.OA.BLL
{
    public class EmployeeSurveyBll : BaseBll<T_OA_REQUIREMASTER>
    {
        #region 新增
        /// <summary>
        /// 新增调查方案、题目、答案
        /// </summary>
        public bool AddRequireMaster(V_EmployeeSurveyMaster addMasterKey)
        {
            try
            {
                bool addFlag = false;
                dal.BeginTransaction();
                addFlag = base.Add(addMasterKey.masterEntity);
                if (addFlag)
                {
                    addMasterKey.answerList.ToList().ForEach(item => { dal.AddToContext(item); });
                    int addAnswer = dal.SaveContextChanges();
                    if (addAnswer > 0)
                    {
                        base.CommitTransaction();
                        return true;
                    }
                }
                base.RollbackTransaction();
                return false;
            }
            catch (Exception ex)
            {
                base.RollbackTransaction();
                Tracer.Debug("员工调查方案EmployeeSurveyBll-AddRequireMaster" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 修改
        /// <summary>
        /// 修改调查方案、题目、答案
        /// </summary>
        public bool UpdRequireMaster(V_EmployeeSurveyMaster updMaterKey)
        {
            try
            {
                string _masterKey = updMaterKey.masterEntity.REQUIREMASTERID;
                var data = GetMasterDataByLoading(_masterKey);//验证数据
                if (data != null)
                {
                    base.BeginTransaction();
                    foreach (T_OA_REQUIREDETAIL key in updMaterKey.answerList)
                    {
                        key.EntityKey = Utility.AddEntityKey("T_OA_REQUIREDETAIL", "REQUIREDETAILID", key.REQUIREDETAILID);
                        dal.UpdateFromContext(key);
                    }
                    int updFlag = dal.SaveContextChanges();//如果答案保存成功
                    foreach (var m in updMaterKey.masterEntity.T_OA_REQUIREDETAIL2)
                    {
                        var child = dal.GetObjects<T_OA_REQUIREDETAIL2>()
                                 .Where(item => item.SUBJECTID == m.SUBJECTID && item.REQUIREMASTERID == m.REQUIREMASTERID)
                            .Select(items => items);

                    }
                    if (updFlag > 0)
                    {
                        foreach (var key in updMaterKey.masterEntity.T_OA_REQUIREDETAIL2)
                        {
                            Dictionary<string, object> dictonary = new Dictionary<string, object>() { { "REQUIREMASTERID", key.REQUIREMASTERID }, { "SUBJECTID", key.SUBJECTID } };
                            //联合主键(题号和方案号),传递字典集合
                            key.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_REQUIREDETAIL2", dictonary);
                            if (key.T_OA_REQUIREMASTERReference == null)
                            {
                                key.T_OA_REQUIREMASTERReference.EntityKey = Utility.AddEntityKey("T_OA_REQUIREMASTER", "REQUIREMASTERID", _masterKey);
                            }
                        }
                        updMaterKey.masterEntity.EntityKey = Utility.AddEntityKey("T_OA_REQUIREMASTER", "REQUIREMASTERID", _masterKey);
                        int updKey = base.Update(updMaterKey.masterEntity);
                        if (updKey > 0)
                        {
                            base.CommitTransaction();
                            return true;
                        }

                    }
                    base.RollbackTransaction();
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                base.RollbackTransaction();
                Tracer.Debug("员工调查方案EmployeeSurveyBll-UpdRequireMaster" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion

        #region 查询
        /// <summary>
        /// 根据主键ID查询方案、题目和答案的集合
        /// </summary>
        /// <param name="masterKey">方案主键ID</param>
        /// <returns>方案、题目和答案及单独的方案主键ID</returns>
        public V_EmployeeSurveyMaster GetMasterDataByLoading(string masterKey)
        {
            try
            {
                var data = from master in dal.GetObjects<T_OA_REQUIREMASTER>()
                           join sub in
                               (from subject in dal.GetObjects<T_OA_REQUIREDETAIL2>()
                                join ans in
                                    (from answ in dal.GetObjects<T_OA_REQUIREDETAIL>()
                                     orderby answ.CODE
                                     select answ)
                        on new { subject.REQUIREMASTERID, subject.SUBJECTID } equals new { ans.REQUIREMASTERID, ans.SUBJECTID } into answer
                                select new V_EmployeeSurveyInformation { Subject = subject, AnswerList = answer })
                   on master.REQUIREMASTERID equals sub.Subject.REQUIREMASTERID into subject
                           where master.REQUIREMASTERID == masterKey
                           select new V_EmployeeSurveyMaster
                                      {
                                          masterEntity = master,
                                          SubjectList = subject,
                                          RequireMasterId = master.REQUIREMASTERID
                                      };
                return data != null ? data.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveyBll-GetMasterDataByLoad" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 用于查询方案子页面查询相关方案,并提供分页、排序功能
        /// </summary>
        /// <param name="pageCount">页面页数</param>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="pageSize">页面显示数据条目</param>
        /// <param name="checkstate">审核状态</param>
        /// <param name="datetimes">开始与结束日期数组</param>
        /// <returns></returns>
        public IQueryable<V_EmployeeSurveyMaster> GetEmployeeSurveyByCheckstateAndDate(int pageCount, int pageIndex, int pageSize, string checkstate, DateTime[] datetimes)
        {
            try
            {
                DateTime startTime = datetimes[0];
                DateTime endTime = datetimes[1];
                var data = from ents in dal.GetObjects<T_OA_REQUIREMASTER>()
                           where ents.CHECKSTATE == checkstate && ents.CREATEDATE >= startTime && ents.CREATEDATE <= endTime
                           orderby ents.CREATEDATE descending
                           select new V_EmployeeSurveyMaster
                  {
                      Content = ents.CONTENT,
                      SurveyTitle = ents.REQUIRETITLE,
                      RequireMasterId = ents.REQUIREMASTERID,
                      OwnerName = ents.OWNERNAME,
                      CreateDate = ents.CREATEDATE
                  };
                return data != null ? Utility.Pager<V_EmployeeSurveyMaster>(data.AsQueryable(), pageIndex, pageSize, ref pageCount) : null;

            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveyBll-GetEmployeeSurveyByCheckstateAndDate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据审核状态获取数据
        /// </summary>
        public IQueryable<T_OA_REQUIREMASTER> GetSurveyDataByCultivateForCheckState(string checkState)
        {
            try
            {
                var data = from ent in dal.GetObjects<T_OA_REQUIREMASTER>()
                           .Include("T_OA_REQUIRE")
                           .Include("T_OA_REQUIRERESULT")
                           where ent.CHECKSTATE == checkState
                           orderby ent.CREATEDATE
                           select ent;
                return data != null ? data.AsQueryable() : null;

            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveyBll-GetEmployeeSurveyDataByCultivate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据申请ID和参与调查的员工ID获取结果数据
        /// </summary>
        public IQueryable<T_OA_REQUIRERESULT> GetResultByCultivateForID(string requireID, string ownerID)
        {
            try
            {
                var data = from ents in dal.GetObjects<T_OA_REQUIRERESULT>()
                           where ents.OWNERID == ownerID && ents.T_OA_REQUIRE.REQUIREID == requireID
                           orderby ents.CREATEDATE descending
                           select ents;
                Console.WriteLine((data as ObjectQuery).ToTraceString());
                return data != null ? data.AsQueryable() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveyBll-GetResultByCultivateForID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion
    }

}
