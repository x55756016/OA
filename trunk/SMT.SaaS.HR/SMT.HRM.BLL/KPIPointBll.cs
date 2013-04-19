/// <summary>
/// Log No.： 1
/// Modify Desc： update打分方式时的处理，空的businessID（relationID）时使用flowCode为GetKPIPoint()返回结果，检查连接
/// Modifier： 冉龙军
/// Modify Date： 2010-08-09
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using System.Data.Objects;

namespace SMT.HRM.BLL
{
    public class KPIPointBll : BaseBll<T_HR_KPIPOINT>
    {

        /// <summary>
        /// 新增KPI点信息，未完成
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public void AddKPIPoint(T_HR_KPIPOINT entTemp)
        {
            System.Data.Common.DbTransaction tran = null;
            try
            {
                // 1s 冉龙军
                //DataContext.Connection.Open();
                //if (DataContext.Connection.State == System.Data.ConnectionState.Closed)
                //{
                //    DataContext.Connection.Open();
                //}
                //tran = DataContext.Connection.BeginTransaction();
                dal.BeginTransaction();
                // 1e
                //实体不存在
                if (entTemp == null)
                {
                    throw new Exception("{REQUIREDFIELDS}");
                }
                //评分方式不存在
                if (entTemp.T_HR_SCORETYPE == null)
                {
                    throw new Exception("{KPIPOINTREQUIREDFIELDS}");
                }

                // 1s 冉龙军
                ////保存评分方式
                //T_HR_SCORETYPE score = entTemp.T_HR_SCORETYPE;
                //var scoreType = DataContext.T_HR_SCORETYPE.FirstOrDefault(s => s.SCORETYPEID == score.SCORETYPEID);
                //if (scoreType != null)
                //{
                //    throw new Exception("Repetition");
                //}
                //T_HR_SCORETYPE entST = new T_HR_SCORETYPE();
                //Utility.CloneEntity<T_HR_SCORETYPE>(score, entST);
                //if (score.T_HR_RANDOMGROUP != null)
                //    entST.T_HR_RANDOMGROUPReference.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RANDOMGROUP",
                //    "RANDOMGROUPID", score.T_HR_RANDOMGROUP.RANDOMGROUPID);
                //Utility.RefreshEntity(entST);

                ////保存提醒信息
                //if (score.T_HR_KPIREMIND != null)
                //{
                //    List<T_HR_KPIREMIND> remindList = score.T_HR_KPIREMIND.ToList();
                //    foreach (T_HR_KPIREMIND remind in remindList)
                //    {
                //        entST.T_HR_KPIREMIND.Add(remind);
                //        Utility.RefreshEntity(remind);
                //    }
                //}
                //保存KPI点信息
                //var tempEnt = DataContext.T_HR_KPIPOINT.FirstOrDefault(s => s.KPIPOINTID == entTemp.KPIPOINTID);
                //if (tempEnt != null)
                //{
                //    throw new Exception("Repetition");
                //}

                //T_HR_KPIPOINT ent = new T_HR_KPIPOINT();
                //Utility.CloneEntity<T_HR_KPIPOINT>(entTemp, ent);
                ////更新KPI点的外键关联
                ////ent.T_HR_SCORETYPEReference.EntityKey =
                ////        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIPOINT", "ScoreTypeID", entTemp.T_HR_SCORETYPE.SCORETYPEID);
                ////保存KPI点信息
                //ent.T_HR_SCORETYPE = entST;
                //Utility.RefreshEntity(ent);

                //保存KPI点信息
                //T_HR_KPIPOINT ent = new T_HR_KPIPOINT();
                //Utility.CloneEntity<T_HR_KPIPOINT>(entTemp, ent);
                var ents = from c in dal.GetObjects<T_HR_KPIPOINT>()
                           where c.STEPID == entTemp.STEPID
                           select c;
                //if (ents.Count() > 0)
                //{
                //    return;
                //}
                Utility.RefreshEntity(entTemp);
                //更新KPI点的外键关联
                //if (entTemp.T_HR_SCORETYPE != null)
                //    ent.T_HR_SCORETYPEReference.EntityKey =
                //        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIPOINT", "ScoreTypeID", entTemp.T_HR_SCORETYPE.SCORETYPEID);
                //if (entTemp.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                //    ent.T_HR_SCORETYPE.T_HR_RANDOMGROUPReference.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RANDOMGROUP",
                //    "RANDOMGROUPID", entTemp.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);
                // 1e
                dal.Add(entTemp);
                //提交事务
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddKPIPoint:" + ex.Message);
                throw ex;
            }
            finally
            {

            }
        }



        public void UpdateKPIPoint(T_HR_KPIPOINT entTemp)
        {
            try
            {
                //实体不存在
                if (entTemp == null)
                {
                    throw new Exception("{REQUIREDFIELDS}");
                }
                //评分方式不存在
                if (entTemp.T_HR_SCORETYPE == null)
                {
                    throw new Exception("{KPIPOINTREQUIREDFIELDS}");
                }

                //更新KPI类别信息
                var ents = from ent in dal.GetObjects<T_HR_KPIPOINT>().Include("T_HR_SCORETYPE")
                           where ent.KPIPOINTID == entTemp.KPIPOINTID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    //更新评分方式
                    Utility.CloneEntity<T_HR_SCORETYPE>(entTemp.T_HR_SCORETYPE, ent.T_HR_SCORETYPE);
                    //更新KPI类别信息
                    Utility.CloneEntity(entTemp, ent);
                }

                //统一提交
                dal.SaveContextChanges();
                //DataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateKPIPoint:" + ex.Message);
                throw ex;
            }
        }

        public void UpdateKPIPoint(T_HR_KPIPOINT entType, List<T_HR_KPIREMIND> addList, List<T_HR_KPIREMIND> updateList, List<T_HR_KPIREMIND> delList)
        {
            try
            {
                //实体不存在
                if (entType == null)
                {
                    throw new Exception("{REQUIREDFIELDS}");
                }
                //评分方式不存在
                if (entType.T_HR_SCORETYPE == null)
                {
                    throw new Exception("{KPIPOINTREQUIREDFIELDS}");
                }

                //更新KPI类别信息
                var ents = from ent in dal.GetObjects<T_HR_KPIPOINT>().Include("T_HR_SCORETYPE.T_HR_RANDOMGROUP")
                           where ent.KPIPOINTID == entType.KPIPOINTID
                           select ent;
                if (ents.Count() > 0)
                {

                    //edm = this.DataContext;
                    // 1s 冉龙军
                    //edm.Connection.Open();
                    //if (edm.Connection.State == System.Data.ConnectionState.Closed)
                    //{
                    //    edm.Connection.Open();
                    //}
                    //tran = edm.Connection.BeginTransaction();
                    dal.BeginTransaction();

                    // 1s 冉龙军
                    ////更新KPI类别信息和评分方式
                    //var ent = ents.FirstOrDefault();

                    ////更新KPI类别信息
                    //Utility.CloneEntity(entType, ent);
                    ////Utility.RefreshEntity(ent);

                    ////更新评分方式
                    //Utility.CloneEntity<T_HR_SCORETYPE>(entType.T_HR_SCORETYPE, ent.T_HR_SCORETYPE);
                    //if (entType.T_HR_SCORETYPE.ISRANDOMSCORE == "1" && entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                    //    ent.T_HR_SCORETYPE.T_HR_RANDOMGROUPReference.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RANDOMGROUP",
                    //    "RANDOMGROUPID", entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);
                    //更新KPI类别信息和评分方式
                    var ent = ents.FirstOrDefault();

                    //更新KPI类别信息
                    Utility.CloneEntity(entType, ent);
                    //Utility.RefreshEntity(ent);

                    if (entType.T_HR_SCORETYPE != null)
                        ent.T_HR_SCORETYPEReference.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SCORETYPE",
                        "SCORETYPEID", entType.T_HR_SCORETYPE.SCORETYPEID);
                    // 1e

                    #region 更新提醒信息
                    // 1s 冉龙军
                    ////增加提醒信息
                    //if (addList != null && addList.Count > 0)
                    //    foreach (T_HR_KPIREMIND remind in addList)
                    //    {
                    //        var tempEnt = DataContext.T_HR_KPIREMIND.FirstOrDefault(s => s.REMINDID == remind.REMINDID);
                    //        if (tempEnt == null)
                    //        {
                    //            //添加提醒信息
                    //            //ent.T_HR_SCORETYPE.T_HR_KPIREMIND.Add(remind);
                    //            //断开外键
                    //            Utility.RefreshEntity(remind);
                    //            //添加提醒信息
                    //            this.DataContext.AddObject("T_HR_KPIREMIND", remind);
                    //        }
                    //    }

                    ////删除提醒信息
                    //if (delList != null && delList.Count > 0)
                    //    foreach (T_HR_KPIREMIND remind in delList)
                    //    {
                    //        var tempEnt = DataContext.T_HR_KPIREMIND.FirstOrDefault(s => s.REMINDID == remind.REMINDID);
                    //        if (tempEnt != null)
                    //        {
                    //            //断开外键
                    //            Utility.RefreshEntity(tempEnt);
                    //            //删除提醒信息
                    //            //ent.T_HR_SCORETYPE.T_HR_KPIREMIND.Remove(remind);
                    //            //dal.Delete(remind);
                    //            this.DataContext.DeleteObject(tempEnt);
                    //        }
                    //    }

                    ////更新提醒信息
                    //if (updateList != null && updateList.Count > 0)
                    //    foreach (T_HR_KPIREMIND remind in updateList)
                    //    {
                    //        var ress = from re in DataContext.T_HR_KPIREMIND
                    //                   where re.REMINDID == remind.REMINDID
                    //                   select re;
                    //        if (ress.Count() > 0)
                    //        {
                    //            var re = ress.FirstOrDefault();
                    //            Utility.CloneEntity(remind, re);
                    //        }
                    //    }
                    // 1e
                    #endregion 更新提醒信息

                    //统一提交
                    // int i=    dal.SaveContextChanges();
                    //DataContext.SaveChanges();
                    dal.Update(ent);
                    dal.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateKPIPoints:" + ex.Message);
                throw ex;
            }
            finally
            {

            }
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteKPIPoint(string id)
        {
            var ents = from e in dal.GetObjects<T_HR_KPIPOINT>()
                       where e.KPIPOINTID == id
                       select e;
            //获取实体
            var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

            if (ent != null)
            {
                dal.DeleteFromContext(ent);
                //DataContext.DeleteObject(ent);
                try
                {
                    dal.DeleteFromContext(ent.T_HR_SCORETYPE);
                    //DataContext.DeleteObject(ent.T_HR_SCORETYPE);
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteKPIPoint:" + ex.Message);
                }
            }

            return dal.SaveContextChanges();
        }


        /// <summary>
        /// 根据业务内容获取KPI点的信息
        /// </summary>
        /// <param name="systemID"></param>
        /// <param name="businessID"></param>
        /// <param name="flowID"></param>
        /// <param name="stepID"></param>
        /// <returns>KPI点</returns>
        public T_HR_KPIPOINT GetKPIPoint(string systemID, string businessID, string flowCode, string stepCode)
        {
            try
            {
                // 1s 冉龙军
                // 注意：空的businessID（relationID）时使用flowCode为GetKPIPoint()返回结果
                if (flowCode != "")
                {

                    var entsTmp = from a in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT")

                                  where a.BUSINESSCODE == flowCode //业务ID（FormID）
                                      //&& a.T_HR_KPIPOINT.SYSTEMID == systemID
                                  && a.T_HR_KPIPOINT.STEPID == stepCode
                                  select a.T_HR_KPIPOINT;
                    if (entsTmp.Count() > 0)
                    {
                        T_HR_KPIPOINT pointTmp = entsTmp.FirstOrDefault();
                        // 2s 冉龙军
                        //return pointTmp;
                        // businessID = pointTmp.BUSINESSID;
                        businessID = pointTmp.KPIPOINTID;
                        // 2e
                    }
                    else
                    {
                        return null;
                    }
                }
                // 1e

                var ents = from a in dal.GetObjects<T_HR_KPIPOINT>().Include("T_HR_SCORETYPE.T_HR_RANDOMGROUP").Include("T_HR_SCORETYPE.T_HR_KPIREMIND").Include("T_HR_SCORETYPE")
                           //where a.BUSINESSID == businessID
                           //&& a.SYSTEMID == systemID
                           //    //&& a.FLOWID == flowID//暂时不考虑流程ID，因为没有存入流程ID
                           //&& a.STEPID == stepCode
                           where a.KPIPOINTID == businessID //已经查出kpi信息 ，直接使用kpirecordid 
                           select a;

                if (ents.Count() > 0)
                {
                    T_HR_KPIPOINT point = ents.FirstOrDefault();

                    //if (point.T_HR_SCORETYPE != null && point.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                    //{
                    //    point.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON = new EntityCollection<T_HR_RAMDONGROUPPERSON>();
                    //    List<T_HR_RAMDONGROUPPERSON> list = new RandomGroupBll().GetRandomGroupPersonByGroupID(point.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID).ToList();
                    //    foreach (T_HR_RAMDONGROUPPERSON person in list)
                    //    {
                    //        point.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON.Add(person);
                    //    }
                    //}

                    return point;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteKPIPoint:" + ex.Message);
                throw ex;
            }
            return null;
        }


        /// <summary>
        /// 根据ID获取KPI点的信息
        /// </summary>
        /// <param name="kpiPointID">KPI点的ID</param>
        /// <returns>KPI点</returns>
        public T_HR_KPIPOINT GetKPIPoint(string kpiPointID)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_HR_KPIPOINT>().Include("T_HR_SCORETYPE")
                           where a.KPIPOINTID == kpiPointID
                           select a;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetKPIPoint:" + ex.Message);
                throw ex;
            }
            return null;
        }

        public List<T_HR_KPIPOINT> GetKPIPointListByBusinessCode(string businessCode)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_HR_KPIPOINT>().Include("T_HR_SCORETYPE")
                           where a.BUSINESSID == businessCode
                           select a;


                if (ents.Count() > 0)
                {
                    return ents.ToList();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetKPIPointListByBusinessCode:" + ex.Message);
                throw ex;
            }
            return null;
        }
    }
}
