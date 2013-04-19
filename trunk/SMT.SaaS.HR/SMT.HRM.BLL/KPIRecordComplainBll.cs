/// <summary>
/// Log No.： 1
/// Modify Desc： 检查连接，修改申诉状态
/// Modifier： 冉龙军
/// Modify Date： 2010-08-31
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
    public class KPIRecordComplainBll : BaseBll<T_HR_KPIRECORDCOMPLAIN>
    {
        public void AddKPIRecordComplain(T_HR_KPIRECORDCOMPLAIN entType)
        {
            //System.Data.Common.DbTransaction tran = null;
            try
            {
                // 1s 冉龙军
                //DataContext.Connection.Open();
                //entType.CHECKSTATE = "1";
                //if (DataContext.Connection.State == System.Data.ConnectionState.Closed)
                //{
                //    DataContext.Connection.Open();
                //}
                //// 1e
                //tran = DataContext.Connection.BeginTransaction();
                //dal.BeginTransaction();
                //实体不存在
                if (entType == null)
                    throw new Exception("{REQUIREDFIELDS}");
                //KPI明细记录不存在
                if (entType.T_HR_KPIRECORD == null)
                    throw new Exception("{KPIRECORDREQUIREDFIELDS}");

                var tempEnt = dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>().FirstOrDefault(s => s.COMPLAINID == entType.COMPLAINID);
                if (tempEnt != null)
                {
                    throw new Exception("Repetition");
                }
                entType.UPDATEDATE = System.DateTime.Now;
                entType.CREATEDATE = System.DateTime.Now;

                //更新KPI类别信息
                var ents = from ent in dal.GetObjects<T_HR_KPIRECORD>()
                           where ent.KPIRECORDID == entType.T_HR_KPIRECORD.KPIRECORDID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    //更新评分方式
                    Utility.CloneEntity<T_HR_KPIRECORD>(entType.T_HR_KPIRECORD, ent);
                    // 1s 冉龙军
                    //ent.COMPLAINSTATUS = "1";
                    ent.COMPLAINSTATUS = "0";


                    KPIRecordBll bll = new KPIRecordBll();

                    if (ent.T_HR_KPIPOINT != null)
                    {
                        ent.T_HR_KPIPOINTReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIPOINT", "KPIPOINTID", ent.T_HR_KPIPOINT.KPIPOINTID);

                        ent.T_HR_KPIPOINT.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIPOINT", "KPIPOINTID", ent.T_HR_KPIPOINT.KPIPOINTID);

                    }
                    Utility.RefreshEntity(ent);

                    bll.Update(ent);


                    if (entType.T_HR_KPIRECORD != null)
                    {
                        entType.T_HR_KPIRECORDReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIRECORD", "KPIRECORDID", entType.T_HR_KPIRECORD.KPIRECORDID);

                        entType.T_HR_KPIRECORD.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIRECORD", "KPIRECORDID", entType.T_HR_KPIRECORD.KPIRECORDID);

                    }
                    Utility.RefreshEntity(entType);
                    dal.Add(entType);


                    //ent.T_HR_KPIRECORDCOMPLAIN.Add(entType);
                }

                //统一提交
                //dal.SaveContextChanges();
                //DataContext.SaveChanges();

                //提交事务
                //dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                //dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddKPIRecordComplain:" + ex.Message);
                throw ex;
            }
            finally
            {

            }
        }
        public void UpdateKPIRecordComplain(T_HR_KPIRECORDCOMPLAIN entType)
        {
            //System.Data.Common.DbTransaction tran = null;
            try
            {
                //if (DataContext.Connection.State == System.Data.ConnectionState.Closed)  //处理数据连接未关闭错误
                //{
                //    DataContext.Connection.Open();
                //}
                //tran = DataContext.Connection.BeginTransaction();
                //dal.BeginTransaction();
                //实体不存在
                if (entType == null)
                    throw new Exception("{REQUIREDFIELDS}");
                //KPI明细记录不存在
                if (entType.T_HR_KPIRECORD == null)
                    throw new Exception("{KPIRECORDREQUIREDFIELDS}");

                var ents = from ent in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>().Include("T_HR_KPIRECORD")
                           where ent.COMPLAINID == entType.COMPLAINID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    entType.UPDATEDATE = System.DateTime.Now;
                    if (entType.CHECKSTATE != null)
                    {

                        // 1s 冉龙军
                        //if (entType.CHECKSTATE.Trim().Equals("0") && entType.REVIEWSCORE != null)
                        //    //不同意，将审批得分置为null
                        //    entType.REVIEWSCORE = null;
                        //else
                        //    //同意，则修改KPI记录的总得分
                        //    entType.T_HR_KPIRECORD.SUMSCORE = entType.REVIEWSCORE;
                        ////修改KPI记录状态
                        //entType.T_HR_KPIRECORD.COMPLAINSTATUS = "2";
                        //同意，则修改KPI记录的总得分
                        if (entType.CHECKSTATE.Trim().Equals("2") && entType.REVIEWSCORE != null)
                            entType.T_HR_KPIRECORD.SUMSCORE = entType.REVIEWSCORE;
                        //if (entType.CHECKSTATE.Trim().Equals("1"))
                        //    entType.T_HR_KPIRECORD.COMPLAINSTATUS = "1";
                        //修改KPI记录状态
                        //entType.T_HR_KPIRECORD.COMPLAINSTATUS = entType.CHECKSTATE;
                        // 1e
                    }

                    Utility.CloneEntity<T_HR_KPIRECORDCOMPLAIN>(entType, ent);

                    if (ent.T_HR_KPIRECORD != null)
                    {
                        try
                        {

                            ent.T_HR_KPIRECORDReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIRECORD", "KPIRECORDID", entType.T_HR_KPIRECORD.KPIRECORDID);


                            //    ent.T_HR_KPIRECORDReference.EntityKey =
                            //new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIRECORD", "KPIRECORDID", ent.T_HR_KPIRECORD.KPIRECORDID);

                            //    ent.T_HR_KPIRECORD.EntityKey =
                            //new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIRECORD", "KPIRECORDID", ent.T_HR_KPIRECORD.KPIRECORDID);

                        }
                        catch { }
                    }
                    //Utility.RefreshEntity(ent);
                    dal.Update(ent);

                    //统一提交
                    // dal.SaveContextChanges();
                    //DataContext.SaveChanges();
                }

                //提交事务
                //dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                //dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateKPIRecordComplain:" + ex.Message);
                throw ex;
            }
            finally
            {

            }
        }

        public IQueryable<T_HR_KPIRECORDCOMPLAIN> GetKPIRecordComplainByRecord(string recordId)
        {
            var q = from ent in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>().Include("T_HR_KPIRECORD.T_HR_KPIPOINT")
                    where ent.T_HR_KPIRECORD.KPIRECORDID == recordId
                    orderby ent.COMPLAINDATE descending
                    select ent;
            return q;
        }

        public T_HR_KPIRECORDCOMPLAIN GetKPIRecordComplain(string complainId)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>().Include("T_HR_KPIRECORD.T_HR_KPIPOINT")
                           where ent.COMPLAINANTID == complainId
                           select ent;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetKPIRecordComplain:" + ex.Message);
                throw ex;
            }
            return null;
        }

        /////..............................................................

        #region  YJH  create time 2010-10-19
        /// <summary>
        /// 根据参数获取KPI明细记录的申诉记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public IQueryable<V_COMPLAINRECORD> GetComplainRecordPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, int sType, string sValue, string userID, string startDate, string endDate, string strCheckState)
        {
            List<object> queryParas = new List<object>();

            queryParas.AddRange(paras);

            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_KPIRECORDCOMPLAIN");

                if (!string.IsNullOrEmpty(strCheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "T_HR_KPIRECORDCOMPLAIN.CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
            }
            else
            {
                SetFilterWithflow("COMPLAINID", "T_HR_KPIRECORDCOMPLAIN", userID, ref strCheckState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count())
                {
                    return null;
                }
            }

            var ents = from a in dal.GetObjects<T_HR_KPIRECORD>()
                       join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.APPRAISEEID equals b.EMPLOYEEID
                       join k in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>() on a.KPIRECORDID equals k.T_HR_KPIRECORD.KPIRECORDID
                       join d in dal.GetObjects<T_HR_KPIPOINT>() on a.T_HR_KPIPOINT.KPIPOINTID equals d.KPIPOINTID into TEMPKPI
                       from e in TEMPKPI.DefaultIfEmpty()
                       select new V_COMPLAINRECORD
                       {
                           T_HR_KPIRECORDCOMPLAIN = k,
                           T_HR_KPIRECORD = a,
                           FLOWID = e.FLOWID,
                           EMPLOYEECODE = b.EMPLOYEECODE,
                           EMPLOYEECNAME = b.EMPLOYEECNAME,
                           CREATEUSERID = a.CREATEUSERID,
                           OWNERID = a.OWNERID,
                           OWNERPOSTID = a.OWNERPOSTID,
                           OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                           OWNERCOMPANYID = a.OWNERCOMPANYID
                          
                       };

            switch (sType)
            {
                case 0:
                    ents = from k in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>()
                           join o in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT") on k.T_HR_KPIRECORD.KPIRECORDID equals o.KPIRECORDID
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals b.EMPLOYEEID
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           //join f in dal.GetObjects<T_HR_KPIPOINT>() on o.T_HR_KPIPOINT.KPIPOINTID equals f.KPIPOINTID into TEMPKPI
                           //from e in TEMPKPI.DefaultIfEmpty()
                           where c.COMPANYID == sValue
                           select new V_COMPLAINRECORD
                           {
                               T_HR_KPIRECORDCOMPLAIN = k,
                               T_HR_KPIRECORD = o,
                               FLOWID = o.T_HR_KPIPOINT.FLOWID,
                               EMPLOYEECODE = b.EMPLOYEECODE,
                               EMPLOYEECNAME = b.EMPLOYEECNAME,
                               CREATEUSERID = o.CREATEUSERID,
                               OWNERID = o.OWNERID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               OWNERCOMPANYID = o.OWNERCOMPANYID
                               
                           };
                    break;
                case 1:
                    ents = from k in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>()
                           join o in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT") on k.T_HR_KPIRECORD.KPIRECORDID equals o.KPIRECORDID
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals b.EMPLOYEEID
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID

                           //join f in dal.GetObjects<T_HR_KPIPOINT>() on o.T_HR_KPIPOINT.KPIPOINTID equals f.KPIPOINTID into TEMPKPI
                           //from e in TEMPKPI.DefaultIfEmpty()
                           where d.DEPARTMENTID == sValue
                           select new V_COMPLAINRECORD
                           {
                               T_HR_KPIRECORDCOMPLAIN = k,
                               T_HR_KPIRECORD = o,
                               FLOWID = o.T_HR_KPIPOINT.FLOWID,
                               EMPLOYEECODE = b.EMPLOYEECODE,
                               EMPLOYEECNAME = b.EMPLOYEECNAME,
                               CREATEUSERID = o.CREATEUSERID,
                               OWNERID = o.OWNERID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               OWNERCOMPANYID = o.OWNERCOMPANYID
                             
                           };
                    break;
                case 2:
                    ents = from k in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>()
                           join o in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT") on k.T_HR_KPIRECORD.KPIRECORDID equals o.KPIRECORDID
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals b.EMPLOYEEID
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           //join f in dal.GetObjects<T_HR_KPIPOINT>() on o.T_HR_KPIPOINT.KPIPOINTID equals f.KPIPOINTID into TEMPKPI
                           //from e in TEMPKPI.DefaultIfEmpty()
                           where p.POSTID == sValue
                           select new V_COMPLAINRECORD
                           {
                               T_HR_KPIRECORDCOMPLAIN = k,
                               T_HR_KPIRECORD = o,
                               FLOWID = o.T_HR_KPIPOINT.FLOWID,
                               EMPLOYEECODE = b.EMPLOYEECODE,
                               EMPLOYEECNAME = b.EMPLOYEECNAME,
                               CREATEUSERID = o.CREATEUSERID,
                               OWNERID = o.OWNERID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               OWNERCOMPANYID = o.OWNERCOMPANYID
                              
                           };
                    break;
            }

            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime sTime = Convert.ToDateTime(startDate);
                ents = ents.Where(s => s.T_HR_KPIRECORD.UPDATEDATE >= sTime);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime eTime = Convert.ToDateTime(endDate).AddDays(1);
                ents = ents.Where(s => s.T_HR_KPIRECORD.UPDATEDATE <= eTime);
            }

            //if (!string.IsNullOrEmpty(strCheckState))
            //{
            //    if (!string.IsNullOrEmpty(filterString))
            //    {
            //        filterString += " AND";
            //    }
            //    filterString += " T_HR_KPIRECORDCOMPLAIN.CHECKSTATE == @" + queryParas.Count();
            //    queryParas.Add(strCheckState);
            //}

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_COMPLAINRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 根据申诉记录ID获取V_COMPLAINRECORD类型数据
        /// </summary>
        /// <param name="CompainRecordID">申诉记录ID</param>
        /// <returns></returns>
        public V_COMPLAINRECORD GetVcomplainRecordByID(string CompainRecordID)
        {
            var ents = from a in dal.GetObjects<T_HR_KPIRECORD>()
                       join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.APPRAISEEID equals b.EMPLOYEEID
                       join k in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>() on a.KPIRECORDID equals k.T_HR_KPIRECORD.KPIRECORDID
                       join d in dal.GetObjects<T_HR_KPIPOINT>() on a.T_HR_KPIPOINT.KPIPOINTID equals d.KPIPOINTID into TEMPKPI
                       from e in TEMPKPI.DefaultIfEmpty()
                       where k.COMPLAINID == CompainRecordID
                       select new V_COMPLAINRECORD
                       {
                           T_HR_KPIRECORDCOMPLAIN = k,
                           T_HR_KPIRECORD = a,
                           FLOWID = e.FLOWID,
                           EMPLOYEECODE = b.EMPLOYEECODE,
                           EMPLOYEECNAME = b.EMPLOYEECNAME,
                           CREATEUSERID = a.CREATEUSERID,
                           OWNERID = a.OWNERID,
                           OWNERPOSTID = a.OWNERPOSTID,
                           OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                           OWNERCOMPANYID = a.OWNERCOMPANYID
                       };
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据ID获取申诉记录实体
        /// </summary>
        /// <param name="CompainRecordID"></param>
        /// <returns></returns>
        public T_HR_KPIRECORDCOMPLAIN GetCompainRecordByID(string CompainRecordID)
        {
            var ents = from a in dal.GetTable()
                       where a.COMPLAINANTID == CompainRecordID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 更新申诉实体
        /// </summary>
        /// <param name="entity">自定义薪资设置实体</param>
        public void CompainRecordUpdate(T_HR_KPIRECORDCOMPLAIN entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.COMPLAINID == entity.COMPLAINID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_KPIRECORDCOMPLAIN>(entity, ent);
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CompainRecordUpdate:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 申诉实体
        /// </summary>
        /// <param name="kpirecordid">实体ID</param>
        public void CompainRecordUpdateOver(string kpirecordid)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.T_HR_KPIRECORD.KPIRECORDID == kpirecordid
                           select a;
                if (ents.Count() > 0)
                {
                    foreach (var ent in ents)
                    {
                        T_HR_KPIRECORDCOMPLAIN temp = new T_HR_KPIRECORDCOMPLAIN();
                        temp = ent;
                        if (temp.CHECKSTATE != "2") temp.CHECKSTATE = "3";
                        //temp.CHECKSTATE = "2";
                        dal.Update(ent);
                    }

                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CompainRecordUpdateOver:" + ex.Message);
            }
        }

        /// <summary>
        /// 删除申诉记录，可同时删除多行记录
        /// </summary>
        /// <param name="ComplainRecords">申诉ID数组</param>
        /// <returns></returns>
        public int ComplainRecordDelete(string[] ComplainRecords)
        {
            foreach (string id in ComplainRecords)
            {
                var ents = from e in dal.GetObjects<T_HR_KPIRECORDCOMPLAIN>()
                           where e.COMPLAINID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    //DataContext.DeleteObject(ent);
                }
            }

            return dal.SaveContextChanges();
        }
        #endregion


    }
}
