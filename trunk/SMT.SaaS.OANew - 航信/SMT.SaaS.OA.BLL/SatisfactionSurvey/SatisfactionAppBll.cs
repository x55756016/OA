//******满意度调查申请BLL**************×
//编写人:勒中玉
//编写时间：2011-6-7
//**************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Log;
using SMT.SaaS.OA.DAL.Views;
using SMT.SaaS.OA.DAL.Views;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.DAL.Views;

namespace SMT.SaaS.OA.BLL
{
    public class SatisfactionAppBll : BaseBll<T_OA_SATISFACTIONREQUIRE>
    {
        
        #region 增
        /// <summary>
        /// 新增满意度调查申请
        /// </summary>
        /// <param name="addRequireEntity">满意度调查实体</param>
        /// <param name="distributeList">分布范围实体集合</param>
        /// <returns>返回是否新增成功</returns>
        public bool AddSatisfactionApp(V_Satisfactions addView)
        {
            try
            {
                base.BeginTransaction();
                bool add = base.Add(addView.requireEntity);
                if (add)
                {
                    foreach (var item in addView.distributeuserList)
                    {
                        dal.Add(item);
                    }
                    base.CommitTransaction();
                    return true;
                }
                base.RollbackTransaction();
                return false;
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("满意度调查方案SatisfactionAppBll-AddSatisfactionApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion

        #region 该
        /// <summary>
        /// 更新满意度调查申请
        /// </summary>
        /// <param name="updRequireEntity">满意度调查申请实体</param>
        /// <param name="updDistributeList">发布范围实体集合</param>
        /// <returns>返回是否修改成功</returns>
        public bool UpdSatisfactionApp(V_Satisfactions updView)
        {
            try
            {
                string _foreignKey = updView.requireEntity.SATISFACTIONREQUIREID;
                string _primeKey = updView.Satisfactionmasterid;
                var requireEnt = dal.GetObjects<T_OA_SATISFACTIONREQUIRE>()
                    .Where(ents => ents.SATISFACTIONREQUIREID == updView.Satisfactionrequireid)
                    .Select(ent => ent).Single();
               
                base.BeginTransaction();
                foreach (var del in updView.oldDistributeuserList)
                {
                    var delData = dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                        .Where(item => new { item.MODELNAME, item.FORMID, item.VIEWER, item.VIEWTYPE } == new { del.MODELNAME, del.FORMID, del.VIEWER, del.VIEWTYPE })
                        .Select(data => data)
                        .Single();
                    dal.DeleteFromContext(delData);
                }
                int _delFlag = dal.SaveContextChanges();
                if (_delFlag > 0)
                {
                    foreach(var ents in updView.distributeuserList)
                    {
                      dal.Add(ents);
                    }
                  updView.requireEntity.EntityKey=Utility.AddEntityKey("T_OA_SATISFACTIONREQUIRE","SATISFACTIONREQUIREID",_primeKey);
                    if(updView.requireEntity.T_OA_SATISFACTIONMASTERReference.EntityKey==null)
                    {
                        updView.requireEntity.T_OA_SATISFACTIONMASTERReference.EntityKey=Utility.AddEntityKey("T_OA_SATISFACTIONMASTER","SATISFACTIONMASTERID",_foreignKey);
                    }
                    bool _updFlag=base.Add(updView.requireEntity);
                    if(_updFlag)
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
                Tracer.Debug("满意度调查方案SatisfactionAppBll-UpdSatisfactionApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }

        }
        #endregion

        #region 查

        /// <summary>
        /// 根据审核条件,创建日期获取满意度调查申请集合
        /// </summary>
        /// <param name="pageCount">页面当前数目</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页面项的数目</param>
        /// <param name="checkstate">审核状态</param>
        /// <param name="dateTimes">开始与结束时间</param>
        /// <returns>返回符合条件的申请</returns>
        public IQueryable<V_Satisfactions> GetRequireByCheckstateAndDate(int pageCount, int pageIndex, int pageSize, string checkstate, DateTime[] dateTimes)
        {
            DateTime startTime = dateTimes[0];
            DateTime endTime = dateTimes[1];
            try
            {
                var entRequire = from ents in dal.GetObjects()
                              .Include("T_OA_SATISFACTIONRESULT")
                                 where ents.STARTDATE >= startTime && ents.ENDDATE <= endTime && ents.CHECKSTATE == checkstate
                                 orderby ents.CREATEDATE
                                 select new V_Satisfactions
                                 {
                                     Satisfactionrequireid = ents.SATISFACTIONREQUIREID,
                                     resultList = ents.T_OA_SATISFACTIONRESULT,
                                     SurveyTitle = ents.SATISFACTIONTITLE,
                                     Content = ents.CONTENT,
                                     OwnerName = ents.OWNERNAME,
                                     CreateDate = ents.CREATEDATE,
                                     AnswerGroupid = ents.ANSWERGROUPID
                                 };
                if (entRequire.Count() > 0)
                {
                    List<V_Satisfactions> _delList = entRequire.ToList();
                        foreach (var ent in _delList)
                        {
                            if (ent.resultList.Count() > 0)
                            {
                                _delList.Remove(ent);
                            }

                        entRequire = _delList.AsQueryable();
                    }
                }
                    entRequire = entRequire.AsQueryable();
                    entRequire = Utility.Pager<V_Satisfactions>(entRequire, pageIndex, pageSize, ref pageCount);
                    return entRequire != null && entRequire.Count() > 0 ? entRequire : null;
                }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查申请SatisfactionAppBll-GetRequireByCheckstateAndDate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 子页面加载时取获取分发范围和满意度调查申请
        /// </summary>
        /// <param name="appId">申请表主键</param>
        /// <returns>对应的数据</returns>
        public V_Satisfactions GetSatisfactionAppChild(string appId)
        {
            try
            {
                var requireEnt = from ent in dal.GetObjects<T_OA_SATISFACTIONREQUIRE>()
                                 .Include("T_OA_SATISFACTIONMASTER")
                                 join ents in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                                 on ent.SATISFACTIONREQUIREID equals ents.FORMID into list
                                 where ent.SATISFACTIONREQUIREID == appId
                                 select new V_Satisfactions
                                 {
                                    distributeuserList=list,
                                    requireEntity=ent,
                                    Satisfactionrequireid=ent.SATISFACTIONREQUIREID,
                                    Satisfactionmasterid=ent.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID
                                  };

                return  requireEnt.Count()>0?requireEnt.FirstOrDefault():null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调申请SatisfactionAppBll-GetRequireByCheckstateAndDate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }
        #endregion
      
    }
}
