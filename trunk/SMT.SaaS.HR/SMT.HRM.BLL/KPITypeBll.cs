/// <summary>
/// Log No.： 1
/// Modify Desc： 显示打分，update抽查组时的处理，检查连接
/// Modifier： 冉龙军
/// Modify Date： 2010-08-03
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
    public class KPITypeBll : BaseBll<T_HR_KPITYPE>
    {
        /// <summary>
        /// 获取KPI类型列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public IQueryable<T_HR_KPITYPE> GetKPITypesPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
            ///
            // 1s 冉龙军
            //IQueryable<T_HR_KPITYPE> ents = DataContext.T_HR_KPITYPE;
            //ents = from kt in DataContext.T_HR_KPITYPE
            //       //join pd in DataContext.T_HR_PENSIONDETAIL on kt.UPDATEUSERID equals pd.per
            //       select kt;
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_KPITYPE");
            IQueryable<T_HR_KPITYPE> ents = dal.GetObjects<T_HR_KPITYPE>().Include("T_HR_SCORETYPE");
            //ents = from kt in dal.GetObjects<T_HR_KPITYPE>().Include("T_HR_SCORETYPE")
            //       select kt;
            // 1e

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_KPITYPE>(ents, pageIndex, pageSize, ref pageCount);
            return ents;

        }

        /// <summary>
        /// 获取薪资发放实体(一条)
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回薪资发放实体(一条)</returns>
        public T_HR_KPITYPE GetKPIType(string kpiTypeID)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_HR_KPITYPE>().Include("T_HR_SCORETYPE.T_HR_RANDOMGROUP")//T_HR_RANDOMGROUP.T_HR_KPIREMIND
                           where a.KPITYPEID == kpiTypeID
                           select a;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetKPIType:" + ex.Message);
                throw ex;
            }
            return null;
        }

        /// <summary>
        /// 获取全部提醒信息
        /// </summary>
        /// <returns></returns>
        public List<T_HR_KPIREMIND> GetRemindByScoreTypeID(string scoreTypeID)
        {
            var q = from ent in dal.GetObjects<T_HR_KPIREMIND>()
                    where ent.T_HR_SCORETYPE.SCORETYPEID == scoreTypeID
                    select ent;
            return q.ToList();
        }

        #region 增删改操作

        /// <summary>
        /// 增加KPI类别信息和评分方式
        /// </summary>
        /// <param name="entity">KPI类别实体.评分方式</param>
        public void AddKPIType(T_HR_KPITYPE entTemp, ref string strMsg)
        {
            try
            {
                // 1s 冉龙军
                //DataContext.Connection.Open();
                //if (  DataContext.Connection.State == System.Data.ConnectionState.Closed)
                //{
                //    DataContext.Connection.Open();
                //}
                //// 1e
                //tran = DataContext.Connection.BeginTransaction();
                dal.BeginTransaction();
                //实体不存在
                if (entTemp == null)
                {
                    //  throw new Exception("{REQUIREDFIELDS}");
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }

                //评分方式不存在
                if (entTemp.T_HR_SCORETYPE == null)
                {
                    // throw new Exception("{KPITYPEREQUIREDFIELDS}");
                    strMsg = "{KPITYPEREQUIREDFIELDS}";
                    return;
                }

                //保存评分方式
                T_HR_SCORETYPE score = entTemp.T_HR_SCORETYPE;
                var scoreType = dal.GetObjects<T_HR_SCORETYPE>().FirstOrDefault(s => s.SCORETYPEID == score.SCORETYPEID);
                if (scoreType != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "评分方式重复";
                    return;
                }
                T_HR_SCORETYPE entST = new T_HR_SCORETYPE();
                Utility.CloneEntity<T_HR_SCORETYPE>(score, entST);
                if (score.T_HR_RANDOMGROUP != null)
                    entST.T_HR_RANDOMGROUPReference.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RANDOMGROUP",
                    "RANDOMGROUPID", score.T_HR_RANDOMGROUP.RANDOMGROUPID);
                Utility.RefreshEntity(entST);

                //保存提醒信息
                if (score.T_HR_KPIREMIND != null)
                {
                    List<T_HR_KPIREMIND> remindList = score.T_HR_KPIREMIND.ToList();
                    foreach (T_HR_KPIREMIND remind in remindList)
                    {
                        entST.T_HR_KPIREMIND.Add(remind);
                        Utility.RefreshEntity(remind);
                    }
                }

                //保存KPI点信息
                var tempEnt = dal.GetObjects<T_HR_KPITYPE>().FirstOrDefault(s => s.KPITYPEID == entTemp.KPITYPEID);
                if (tempEnt != null)
                {
                    //  throw new Exception("Repetition");
                    strMsg = "KPI点信息重复";
                }
                T_HR_KPITYPE ent = new T_HR_KPITYPE();
                Utility.CloneEntity<T_HR_KPITYPE>(entTemp, ent);
                //更新KPI点的外键关联
                ent.T_HR_SCORETYPE = entST;
                Utility.RefreshEntity(ent);
                dal.Add(entST);

                //提交事务
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddKPIType:" + ex.Message);
                strMsg = ex.Message;
            }
            finally
            {
            }

        }

        /// <summary>
        /// 修改KPI类别信息和评分方式
        /// </summary>
        /// <param name="entTemp">KPI类别实体.评分方式</param>
        public void UpdateKPIType(T_HR_KPITYPE entTemp, ref string strMsg)
        {
            try
            {
                //实体不存在
                if (entTemp == null)
                {
                    // throw new Exception("{REQUIREDFIELDS}");
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }
                //评分方式不存在
                if (entTemp.T_HR_SCORETYPE == null)
                {
                    //  throw new Exception("{KPITYPEREQUIREDFIELDS}");
                    strMsg = "{KPITYPEREQUIREDFIELDS}";
                    return;
                }

                ////更新评分方式
                //var typeEnts = from ent in DataContext.T_HR_SCORETYPE
                //           where ent.SCORETYPEID == entTemp.T_HR_SCORETYPE.SCORETYPEID
                //           select ent;
                //if (typeEnts.Count() > 0)
                //{
                //    var ent = typeEnts.FirstOrDefault();
                //    Utility.CloneEntity<T_HR_SCORETYPE>(entTemp.T_HR_SCORETYPE, ent);
                //}
                //注释原因：将更新评分方式移至下方更新KPI类别信息中，未知是否可行

                //更新KPI类别信息
                var ents = from ent in dal.GetObjects<T_HR_KPITYPE>()
                           where ent.KPITYPEID == entTemp.KPITYPEID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    //更新评分方式
                    Utility.CloneEntity<T_HR_SCORETYPE>(entTemp.T_HR_SCORETYPE, ent.T_HR_SCORETYPE);
                    //更新KPI类别信息
                    Utility.CloneEntity(entTemp, ent);
                    Update(ent);
                }

                //统一提交
                // dal.SaveContextChanges();
                // DataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateKPIType:" + ex.Message);
                throw ex;
            }
        }


        public void UpdateKPIType(T_HR_KPITYPE entType, List<T_HR_KPIREMIND> addList, List<T_HR_KPIREMIND> updateList, List<T_HR_KPIREMIND> delList, ref string strMsg)
        {

            try
            {
                //实体不存在
                if (entType == null)
                {
                    //  throw new Exception("{REQUIREDFIELDS}");
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }
                //评分方式不存在
                if (entType.T_HR_SCORETYPE == null)
                {
                    // throw new Exception("{KPITYPEREQUIREDFIELDS}");
                    strMsg = "{KPITYPEREQUIREDFIELDS}";
                    return;
                }

                //更新KPI类别信息
                var ents = from ent in dal.GetObjects<T_HR_KPITYPE>()
                           where ent.KPITYPEID == entType.KPITYPEID
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
                    //// 1e
                    //tran = edm.Connection.BeginTransaction();

                    //更新KPI类别信息和评分方式
                    var ent = ents.FirstOrDefault();

                    //更新KPI类别信息
                    Utility.CloneEntity(entType, ent);
                    //Utility.RefreshEntity(ent);
                    dal.UpdateFromContext(ent);
                    dal.SaveContextChanges();
                    //更新评分方式
                    var scoreTypes = from sco in dal.GetObjects<T_HR_SCORETYPE>()
                                     where sco.SCORETYPEID == entType.T_HR_SCORETYPE.SCORETYPEID
                                     select sco;
                    if (scoreTypes.Count() > 0)
                    {
                        T_HR_SCORETYPE scoreType = scoreTypes.FirstOrDefault();
                        Utility.CloneEntity<T_HR_SCORETYPE>(entType.T_HR_SCORETYPE, scoreType);
                        // 1s 冉龙军
                        //ent.T_HR_SCORETYPE.T_HR_RANDOMGROUPReference.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RANDOMGROUP",
                        //    "RANDOMGROUPID", entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);
                        if (entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                        {
                            //Utility.CloneEntity<T_HR_RANDOMGROUP>(entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP, ent.T_HR_SCORETYPE.T_HR_RANDOMGROUP);

                            scoreType.T_HR_RANDOMGROUPReference.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RANDOMGROUP",
                                "RANDOMGROUPID", entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID);

                        }
                        //Utility.RefreshEntity(scoreType);
                        dal.Update(scoreType);
                        if (entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                        {
                            string strSql = @" update T_HR_SCORETYPE set RANDOMGROUPID ='" + entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID + "'";

                            dal.ExecuteCustomerSql(strSql);
                        }
                        else
                        {
                            string strSql = @" update T_HR_SCORETYPE set RANDOMGROUPID =''";
                            dal.ExecuteCustomerSql(strSql);
                        }
                    }


                    // 1e
                    //Utility.RefreshEntity(entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP);

                    //Utility.CloneEntity<T_HR_RANDOMGROUP>(entType.T_HR_SCORETYPE.T_HR_RANDOMGROUP, ent.T_HR_SCORETYPE.T_HR_RANDOMGROUP);

                    #region 更新提醒信息

                    //增加提醒信息
                    if (addList != null && addList.Count > 0)
                        foreach (T_HR_KPIREMIND remind in addList)
                        {
                            var tempEnt = dal.GetObjects<T_HR_KPIREMIND>().FirstOrDefault(s => s.REMINDID == remind.REMINDID);
                            if (tempEnt == null)
                            {
                                //添加提醒信息
                                //ent.T_HR_SCORETYPE.T_HR_KPIREMIND.Add(remind);
                                //断开外键
                                Utility.RefreshEntity(remind);
                                //添加提醒信息
                                dal.AddToContext(remind);
                                //this.DataContext.AddObject("T_HR_KPIREMIND", remind);
                            }
                        }


                    //删除提醒信息
                    if (delList != null && delList.Count > 0)
                        foreach (T_HR_KPIREMIND remind in delList)
                        {
                            var tempEnt = dal.GetObjects<T_HR_KPIREMIND>().FirstOrDefault(s => s.REMINDID == remind.REMINDID);
                            if (tempEnt != null)
                            {
                                //断开外键
                                Utility.RefreshEntity(tempEnt);
                                //删除提醒信息
                                //ent.T_HR_SCORETYPE.T_HR_KPIREMIND.Remove(remind);
                                //dal.Delete(remind);
                                dal.DeleteFromContext(remind);
                                //this.DataContext.DeleteObject(tempEnt);
                            }
                        }

                    //更新提醒信息
                    if (updateList != null && updateList.Count > 0)
                        foreach (T_HR_KPIREMIND remind in updateList)
                        {
                            var ress = from re in dal.GetObjects<T_HR_KPIREMIND>()
                                       where re.REMINDID == remind.REMINDID
                                       select re;
                            if (ress.Count() > 0)
                            {
                                var re = ress.FirstOrDefault();
                                Utility.CloneEntity(remind, re);
                                dal.UpdateFromContext(re);
                            }

                            //foreach (T_HR_KPIREMIND re in ent.T_HR_SCORETYPE.T_HR_KPIREMIND)
                            //{
                            //    if (remind.REMINDID.Equals(re.REMINDID))
                            //    {
                            //        //更新KPI类别信息
                            //        Utility.CloneEntity(remind, re);
                            //        break;
                            //    }

                            //}
                            //var tempEnt = DataContext.T_HR_KPIREMIND.FirstOrDefault(s => s.REMINDID == remind.REMINDID);
                            //if (tempEnt != null)
                            //{
                            //    //更新提醒信息
                            //    Utility.CloneEntity<T_HR_KPIREMIND>(tempEnt, remind);
                            //}
                        }

                    #endregion 更新提醒信息

                    //统一提交
                    dal.SaveContextChanges();
                    //DataContext.SaveChanges();

                    //tran.Commit();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateKPITypes:" + ex.Message);
                //if (tran != null)
                //    tran.Rollback();
                //throw ex;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 删除列表
        /// </summary>
        /// <param name="employeeIDs">需要删除的ID列表</param>
        /// <returns></returns>
        public int DeleteKPITypes(string[] kpiTypeIDs, ref string strMsg)
        {
            foreach (string id in kpiTypeIDs)
            {
                try
                {
                    //获取实体列表
                    var ents = from e in dal.GetObjects<T_HR_KPITYPE>().Include("T_HR_SCORETYPE")
                               where e.KPITYPEID == id
                               select e;
                    //获取实体
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        if (ent.T_HR_SCORETYPE != null)
                        {
                            //检测scoretype 是否被使用
                            var scoreType = from kpi in dal.GetObjects<T_HR_KPIPOINT>()
                                            where kpi.T_HR_SCORETYPE.SCORETYPEID == ent.T_HR_SCORETYPE.SCORETYPEID
                                            select kpi;
                            if (scoreType.Count() > 0)
                            {
                                strMsg = ent.KPITYPENAME + ",";
                                break;
                            }
                            //获取提醒实体列表
                            var reminds = from e in dal.GetObjects<T_HR_KPIREMIND>().Include("T_HR_SCORETYPE")
                                          where e.T_HR_SCORETYPE.SCORETYPEID == ent.T_HR_SCORETYPE.SCORETYPEID
                                          select e;
                            List<T_HR_KPIREMIND> list = reminds.ToList();
                            if (list.Count > 0)
                            {
                                list.ForEach(item =>
                                {
                                    Utility.RefreshEntity(item);
                                    dal.DeleteFromContext(item);
                                    // DataContext.DeleteObject(item);
                                });
                            }
                            dal.DeleteFromContext(ent.T_HR_SCORETYPE);
                            // DataContext.DeleteObject(ent.T_HR_SCORETYPE);
                        }
                        dal.DeleteFromContext(ent);
                        //DataContext.DeleteObject(ent);
                    }
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteKPITypes:" + ex.Message);
                }
            }
            return dal.SaveContextChanges();
        }


        /// <summary>
        /// 删除和评分方式
        /// </summary>
        /// <param name="id">KPI类别ID</param>
        /// <returns></returns>
        public int DeleteKPIType(string id)
        {
            try
            {
                var ents = from e in dal.GetObjects<T_HR_KPITYPE>()
                           where e.KPITYPEID == id
                           select e;
                //获取实体
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    //DataContext.DeleteObject(ent);
                    //获取提醒实体列表
                    var reminds = from e in dal.GetObjects<T_HR_KPIREMIND>()
                                  where e.T_HR_SCORETYPE.SCORETYPEID == ent.T_HR_SCORETYPE.SCORETYPEID
                                  select e;
                    reminds.ToList().ForEach(item =>
                    {
                        Utility.RefreshEntity(item);
                        dal.DeleteFromContext(item);
                        //DataContext.DeleteObject(item);
                    });
                    dal.DeleteFromContext(ent.T_HR_SCORETYPE);
                    //DataContext.DeleteObject(ent.T_HR_SCORETYPE);
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex) 
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteKPIType:" + ex.Message);
                throw ex;
            }
        }

        #endregion 增删改操作

        public List<T_HR_KPITYPE> GetKPITypeAll()
        {
            var q = from ent in dal.GetObjects<T_HR_KPITYPE>().Include("T_HR_SCORETYPE.T_HR_RANDOMGROUP")
                    select ent;
            return q.ToList();
        }
        /// <summary>
        /// 根据权限获取kpi类别
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<T_HR_KPITYPE> GetKPITypeWithPermission(string filterString, IList<object> paras, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_KPITYPE");
            var q = from ent in dal.GetObjects<T_HR_KPITYPE>().Include("T_HR_SCORETYPE.T_HR_RANDOMGROUP").Include("T_HR_SCORETYPE")
                    select ent;
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.Where(filterString, queryParas.ToArray());
            }
            if (q.Count() > 0)
            {
                List<T_HR_KPITYPE> kpiTypeList = q.ToList();
                foreach (T_HR_KPITYPE kpiType in kpiTypeList)
                {
                    List<T_HR_KPIREMIND> list = GetRemindByScoreTypeID(kpiType.T_HR_SCORETYPE.SCORETYPEID);
                    foreach (T_HR_KPIREMIND remind in list)
                    {
                        kpiType.T_HR_SCORETYPE.T_HR_KPIREMIND.Add(remind);
                    }
                }
                return kpiTypeList;
            }
            else
            {
                return null;
            }

        }
    }
}
