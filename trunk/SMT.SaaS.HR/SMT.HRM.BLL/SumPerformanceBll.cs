/// <summary>
/// Log No.： 1
/// Modify Desc： 使用V_PERFORMANCERECORD、V_PERFORMANCERECORDDETAIL（含员工信息），检查连接，部门过滤
/// Modifier： 冉龙军
/// Modify Date： 2010-08-11
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
    public class SumPerformanceBll : BaseBll<T_HR_SUMPERFORMANCERECORD>
    {
        public IQueryable<T_HR_SUMPERFORMANCERECORD> GetSumPerformancePaging(int pageIndex, int pageSize, string sort, string filterString,
            string[] paras, ref int pageCount, string sType, string sValue, string userID, bool isSelf, string strCheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            IQueryable<T_HR_SUMPERFORMANCERECORD> ents = dal.GetObjects();
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SUMPERFORMANCERECORD");
                //员工入职审核通过再显示。

                ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
                ///
                if (!string.IsNullOrEmpty(strCheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }

                if (isSelf)
                {
                    ents = from o in dal.GetObjects()
                           join p in dal.GetObjects<T_HR_PERFORMANCERECORD>() on o.SUMID equals p.T_HR_SUMPERFORMANCERECORD.SUMID
                           where p.APPRAISEEID == userID
                           select o;
                }
                else
                {
                    switch (sType)
                    {
                        case "Company":
                            // 1s 冉龙军
                            //ents = from o in dal.GetObjects()
                            //       join d in DataContext.T_HR_DEPARTMENT on o.CREATECOMPANYID equals d.DEPARTMENTID
                            //       join c in DataContext.T_HR_COMPANY on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                            //       where c.COMPANYID == sValue
                            //       select o;
                            ents = from o in dal.GetObjects()
                                   join d in dal.GetObjects<T_HR_DEPARTMENT>() on o.CREATEDEPARTMENTID equals d.DEPARTMENTID
                                   join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                   where c.COMPANYID == sValue
                                   select o;
                            // 1e
                            break;
                        case "Department":
                            ents = from o in dal.GetObjects()
                                   join d in dal.GetObjects<T_HR_DEPARTMENT>() on o.CREATEDEPARTMENTID equals d.DEPARTMENTID
                                   where d.DEPARTMENTID == sValue
                                   select o;
                            break;
                        default:
                            ents = from o in dal.GetObjects()
                                   select o;
                            break;
                    }
                }

            }
            else
            {
                SetFilterWithflow("SUMID", "T_HR_SUMPERFORMANCERECORD", userID, ref strCheckState, ref filterString, ref queryParas);


            }

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SUMPERFORMANCERECORD>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }

        public IQueryable<T_HR_SUMPERFORMANCERECORD> GetSumPerformancePagingByTime(int pageIndex, int pageSize, string sort, string filterString,
            string[] paras, ref int pageCount, string sType, string sValue, string userID, bool isSelf, string startTime, string endTime)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SUMPERFORMANCERECORD");
            //员工入职审核通过再显示。

            ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
            ///

            IQueryable<T_HR_SUMPERFORMANCERECORD> ents = dal.GetObjects();
            if (isSelf)
            {
                ents = from o in dal.GetObjects()
                       join p in dal.GetObjects<T_HR_PERFORMANCERECORD>() on o.SUMID equals p.T_HR_SUMPERFORMANCERECORD.SUMID
                       where p.APPRAISEEID == userID
                       select o;
            }
            else
            {
                switch (sType)
                {
                    case "Company":
                        // 1s 冉龙军
                        //ents = from o in dal.GetObjects()
                        //       join d in DataContext.T_HR_DEPARTMENT on o.CREATECOMPANYID equals d.DEPARTMENTID
                        //       join c in DataContext.T_HR_COMPANY on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                        //       where c.COMPANYID == sValue
                        //       select o;
                        ents = from o in dal.GetObjects()
                               join d in dal.GetObjects<T_HR_DEPARTMENT>() on o.CREATEDEPARTMENTID equals d.DEPARTMENTID
                               join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                               where c.COMPANYID == sValue
                               select o;
                        // 1e
                        break;
                    case "Department":
                        ents = from o in dal.GetObjects()
                               join d in dal.GetObjects<T_HR_DEPARTMENT>() on o.CREATEDEPARTMENTID equals d.DEPARTMENTID
                               where d.DEPARTMENTID == sValue
                               select o;
                        break;
                    default:
                        ents = from o in dal.GetObjects()
                               select o;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(startTime))
            {
                DateTime sTime = Convert.ToDateTime(startTime);
                ents = ents.Where(s => s.UPDATEDATE >= sTime);
            }
            if (!string.IsNullOrEmpty(endTime))
            {
                DateTime eTime = Convert.ToDateTime(endTime).AddDays(1);
                ents = ents.Where(s => s.UPDATEDATE <= eTime);
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SUMPERFORMANCERECORD>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }
        public IQueryable<T_HR_PERFORMANCERECORD> GetPerformanceAllBySumID(string sumID)
        {
            var q = from ent in dal.GetObjects<T_HR_PERFORMANCERECORD>()
                    where ent.T_HR_SUMPERFORMANCERECORD.SUMID == sumID
                    select ent;
            return q;
        }

        // 1s 冉龙军
        /// <summary>
        /// 获取绩效考核个人记录（平均分）
        /// </summary>
        /// <param name="sumID"></param>
        /// <returns></returns>
        public IQueryable<V_PERFORMANCERECORD> GetPerformanceEmployeeAllBySumID(string sumID)
        {
            var q = from a in dal.GetObjects<T_HR_PERFORMANCERECORD>()
                    join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.APPRAISEEID equals b.EMPLOYEEID
                    where a.T_HR_SUMPERFORMANCERECORD.SUMID == sumID
                    select new V_PERFORMANCERECORD
                    {
                        T_HR_PERFORMANCERECORD = a,
                        EMPLOYEECODE = b.EMPLOYEECODE,
                        EMPLOYEECNAME = b.EMPLOYEECNAME


                    };

            return q;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="sumID"></param>
        /// <returns></returns>
        public IQueryable<V_PERFORMANCERECORD> GetPerformanceEmployeeAllPaging(int pageIndex, int pageSize, string sort, string filterString,
            string[] paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            var ents = from a in dal.GetObjects<T_HR_PERFORMANCERECORD>()
                       join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.APPRAISEEID equals b.EMPLOYEEID
                       //where a.T_HR_SUMPERFORMANCERECORD.SUMID == sumID
                       select new V_PERFORMANCERECORD
                       {
                           T_HR_PERFORMANCERECORD = a,
                           EMPLOYEECODE = b.EMPLOYEECODE,
                           EMPLOYEECNAME = b.EMPLOYEECNAME


                       };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_PERFORMANCERECORD>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }
        public IQueryable<V_PERFORMANCERECORD> GetEmployeePerformancePagingByTime(int pageIndex, int pageSize, string sort, string filterString,
          string[] paras, ref int pageCount, string sType, string sValue, string userID, string startTime, string endTime)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SUMPERFORMANCERECORD");
            //员工入职审核通过再显示。

            ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
            ///
            if (!string.IsNullOrEmpty(filterString))
            {
                filterString += " or ";
            }
            filterString += " T_HR_PERFORMANCERECORD.APPRAISEEID==@" + queryParas.Count().ToString();
            queryParas.Add(userID);
            IQueryable<V_PERFORMANCERECORD> ents = from o in dal.GetObjects<T_HR_PERFORMANCERECORD>()
                                                   join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals p.EMPLOYEEID
                                                   join q in dal.GetObjects<T_HR_EMPLOYEEPOST>() on p.EMPLOYEEID equals q.T_HR_EMPLOYEE.EMPLOYEEID
                                                   join r in dal.GetObjects<T_HR_POST>() on q.T_HR_POST.POSTID equals r.POSTID
                                                   join s in dal.GetObjects<T_HR_DEPARTMENT>() on r.T_HR_DEPARTMENT.DEPARTMENTID equals s.DEPARTMENTID
                                                   join t in dal.GetObjects<T_HR_COMPANY>() on s.T_HR_COMPANY.COMPANYID equals t.COMPANYID
                                                   where q.EDITSTATE == "1"
                                                   select new V_PERFORMANCERECORD
                                                   {
                                                       T_HR_PERFORMANCERECORD = o,
                                                       EMPLOYEECNAME = p.EMPLOYEECNAME,
                                                       EMPLOYEECODE = p.EMPLOYEECODE,
                                                       SUMSTART = o.T_HR_SUMPERFORMANCERECORD.SUMSTART,
                                                       SUMEND = o.T_HR_SUMPERFORMANCERECORD.SUMEND,
                                                       SUMID = o.T_HR_SUMPERFORMANCERECORD.SUMID,
                                                       PERFORMANCEID = o.PERFORMANCEID,
                                                       OWNERID = o.APPRAISEEID,
                                                       OWNERDEPARTMENTID = s.DEPARTMENTID,
                                                       OWNERCOMPANYID = t.COMPANYID,
                                                       OWNERPOSTID = r.POSTID,
                                                       CREATEUSERID = o.T_HR_SUMPERFORMANCERECORD.CREATEUSERID
                                                   };
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects<T_HR_PERFORMANCERECORD>()
                           join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals p.EMPLOYEEID
                           join q in dal.GetObjects<T_HR_EMPLOYEEPOST>() on p.EMPLOYEEID equals q.T_HR_EMPLOYEE.EMPLOYEEID
                           join r in dal.GetObjects<T_HR_POST>() on q.T_HR_POST.POSTID equals r.POSTID
                           join s in dal.GetObjects<T_HR_DEPARTMENT>() on r.T_HR_DEPARTMENT.DEPARTMENTID equals s.DEPARTMENTID
                           join t in dal.GetObjects<T_HR_COMPANY>() on s.T_HR_COMPANY.COMPANYID equals t.COMPANYID
                           where t.COMPANYID == sValue && q.EDITSTATE == "1"
                           select new V_PERFORMANCERECORD
                           {
                               T_HR_PERFORMANCERECORD = o,
                               EMPLOYEECNAME = p.EMPLOYEECNAME,
                               EMPLOYEECODE = p.EMPLOYEECODE,
                               SUMSTART = o.T_HR_SUMPERFORMANCERECORD.SUMSTART,
                               SUMEND = o.T_HR_SUMPERFORMANCERECORD.SUMEND,
                               SUMID = o.T_HR_SUMPERFORMANCERECORD.SUMID,
                               PERFORMANCEID = o.PERFORMANCEID,
                               OWNERID = o.APPRAISEEID,
                               OWNERDEPARTMENTID = s.DEPARTMENTID,
                               OWNERCOMPANYID = t.COMPANYID,
                               OWNERPOSTID = r.POSTID,
                               CREATEUSERID = o.T_HR_SUMPERFORMANCERECORD.CREATEUSERID
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects<T_HR_PERFORMANCERECORD>()
                           join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals p.EMPLOYEEID
                           join q in dal.GetObjects<T_HR_EMPLOYEEPOST>() on p.EMPLOYEEID equals q.T_HR_EMPLOYEE.EMPLOYEEID
                           join r in dal.GetObjects<T_HR_POST>() on q.T_HR_POST.POSTID equals r.POSTID
                           join s in dal.GetObjects<T_HR_DEPARTMENT>() on r.T_HR_DEPARTMENT.DEPARTMENTID equals s.DEPARTMENTID
                           join t in dal.GetObjects<T_HR_COMPANY>() on s.T_HR_COMPANY.COMPANYID equals t.COMPANYID
                           where s.DEPARTMENTID == sValue && q.EDITSTATE == "1"
                           select new V_PERFORMANCERECORD
                           {
                               T_HR_PERFORMANCERECORD = o,
                               EMPLOYEECNAME = p.EMPLOYEECNAME,
                               EMPLOYEECODE = p.EMPLOYEECODE,
                               SUMSTART = o.T_HR_SUMPERFORMANCERECORD.SUMSTART,
                               SUMEND = o.T_HR_SUMPERFORMANCERECORD.SUMEND,
                               SUMID = o.T_HR_SUMPERFORMANCERECORD.SUMID,
                               PERFORMANCEID = o.PERFORMANCEID,
                               OWNERID = o.APPRAISEEID,
                               OWNERDEPARTMENTID = s.DEPARTMENTID,
                               OWNERCOMPANYID = t.COMPANYID,
                               OWNERPOSTID = r.POSTID,
                               CREATEUSERID = o.T_HR_SUMPERFORMANCERECORD.CREATEUSERID
                           };
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(startTime))
            {
                DateTime sTime = Convert.ToDateTime(startTime);
                ents = ents.Where(s => s.SUMSTART >= sTime);
            }
            if (!string.IsNullOrEmpty(endTime))
            {
                DateTime eTime = Convert.ToDateTime(endTime).AddDays(1);
                ents = ents.Where(s => s.SUMEND <= eTime);
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_PERFORMANCERECORD>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }
        /// <summary>
        /// 获取绩效考核记录个人明细
        /// </summary>
        /// <param name="sumID"></param>
        /// <returns></returns>
        public IQueryable<V_PERFORMANCERECORDDETAIL> GetPerformanceDetailEmployeeAllBySumID(string sumID)
        {

            var q = from a in dal.GetObjects<T_HR_PERFORMANCEDETAIL>()
                    join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.T_HR_PERFORMANCERECORD.APPRAISEEID equals b.EMPLOYEEID
                    where a.T_HR_PERFORMANCERECORD.T_HR_SUMPERFORMANCERECORD.SUMID == sumID
                    select new V_PERFORMANCERECORDDETAIL
                    {
                        T_HR_PERFORMANCERECORD = a.T_HR_PERFORMANCERECORD,
                        T_HR_KPIRECORD = a.T_HR_KPIRECORD,
                        EMPLOYEECODE = b.EMPLOYEECODE,
                        EMPLOYEECNAME = b.EMPLOYEECNAME


                    };

            return q;
        }
        /// <summary>
        /// 获取绩效考核记录个人明细
        /// </summary>
        /// <param name="sumID"></param>
        /// <returns></returns>
        public IQueryable<V_PERFORMANCERECORDDETAIL> GetPerformanceDetailEmployeeAllPaging(int pageIndex, int pageSize, string sort, string filterString,
            string[] paras, ref int pageCount, string userID, string startTime, string endTime)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            //var ents = from a in DataContext.T_HR_PERFORMANCEDETAIL
            //        join b in DataContext.T_HR_EMPLOYEE on a.T_HR_PERFORMANCERECORD.APPRAISEEID equals b.EMPLOYEEID
            //       //where a.T_HR_PERFORMANCERECORD.T_HR_SUMPERFORMANCERECORD.SUMID == sumID
            //        select new V_PERFORMANCERECORDDETAIL
            //        {
            //            T_HR_PERFORMANCERECORD = a.T_HR_PERFORMANCERECORD,
            //            T_HR_KPIRECORD = a.T_HR_KPIRECORD,
            //            EMPLOYEECODE = b.EMPLOYEECODE,
            //            EMPLOYEECNAME = b.EMPLOYEECNAME,
            //            PERFORMANCEID = a.T_HR_PERFORMANCERECORD.PERFORMANCEID,
            //            FLOWNAME = a.T_HR_KPIRECORD.T_HR_KPIPOINT.FLOWID
            //        };
            var ents = from a in dal.GetObjects<T_HR_KPIRECORD>()
                       join b in dal.GetObjects<T_HR_PERFORMANCEDETAIL>() on a.KPIRECORDID equals b.T_HR_KPIRECORD.KPIRECORDID
                       join c in dal.GetObjects<T_HR_PERFORMANCERECORD>() on b.T_HR_PERFORMANCERECORD.PERFORMANCEID equals c.PERFORMANCEID
                       join d in dal.GetObjects<T_HR_EMPLOYEE>() on c.APPRAISEEID equals d.EMPLOYEEID
                       //join b in DataContext.T_HR_EMPLOYEE on a.T_HR_PERFORMANCERECORD.APPRAISEEID equals b.EMPLOYEEID
                       //where a.T_HR_PERFORMANCERECORD.T_HR_SUMPERFORMANCERECORD.SUMID == sumID
                       select new V_PERFORMANCERECORDDETAIL
                       {
                           T_HR_PERFORMANCERECORD = c,
                           T_HR_KPIRECORD = a,
                           EMPLOYEECODE = d.EMPLOYEECODE,
                           EMPLOYEECNAME = d.EMPLOYEECNAME,
                           PERFORMANCEID = c.PERFORMANCEID,
                           FLOWNAME = a.T_HR_KPIPOINT.FLOWID
                       };
            //if (!string.IsNullOrEmpty(startTime))
            //{
            //    DateTime sTime = Convert.ToDateTime(startTime);
            //    ents = ents.Where(s => s.T_HR_KPIRECORD.UPDATEDATE >= sTime);
            //}
            //if (!string.IsNullOrEmpty(endTime))
            //{
            //    DateTime eTime = Convert.ToDateTime(endTime);
            //    ents = ents.Where(s => s.T_HR_KPIRECORD.UPDATEDATE <= eTime);
            //}
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_PERFORMANCERECORDDETAIL>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }
        // 1e
        public T_HR_PERFORMANCERECORD GetPerformanceRecordByID(string recordID)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_HR_PERFORMANCERECORD>()
                           where a.PERFORMANCEID == recordID
                           select a;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetPerformanceRecordByID:" + ex.Message);
                throw ex;
            }
            return null;
        }

        /// <summary>
        /// 新建汇总信息。将人员KPI明细记录汇总后，保存汇总信息
        /// </summary>
        /// <param name="entType"></param>
        public void AddSumPerformance(T_HR_SUMPERFORMANCERECORD entType)
        {
            //System.Data.Common.DbTransaction tran = null;
            try
            {
                // 1s 冉龙军
                //DataContext.Connection.Open();
                //if (DataContext.Connection.State == System.Data.ConnectionState.Closed)
                //{
                //    DataContext.Connection.Open();
                //}
                //// 1e
                //tran = DataContext.Connection.BeginTransaction();
                dal.BeginTransaction();
                //实体不存在
                if (entType == null)
                    throw new Exception("{REQUIREDFIELDS}");

                //评分方式不存在
                if (entType.T_HR_PERFORMANCERECORD == null || entType.T_HR_PERFORMANCERECORD.Count == 0)
                    throw new Exception("{PERSONREQUIREDFIELDS}");

                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.SUMNAME == entType.SUMNAME
                || s.SUMID == entType.SUMID);
                if (tempEnt != null)
                {
                    throw new Exception("Repetition");
                }
                #region
                // 1s 冉龙军
                //T_HR_SUMPERFORMANCERECORD entTemp = new T_HR_SUMPERFORMANCERECORD();
                //Utility.CloneEntity<T_HR_SUMPERFORMANCERECORD>(entType, entTemp);
                ////断开关联
                //Utility.RefreshEntity(entTemp);

                ////绩效考核信息
                //if (entType.T_HR_PERFORMANCERECORD != null)
                //{
                //    List<T_HR_PERFORMANCERECORD> recordList = entType.T_HR_PERFORMANCERECORD.ToList();
                //    decimal? score = 0;
                //    foreach (T_HR_PERFORMANCERECORD record in recordList)
                //    {
                //        //添加绩效考核记录
                //        entTemp.T_HR_PERFORMANCERECORD.Add(record);
                //        //断开关联
                //        Utility.RefreshEntity(record);

                //        //获取相关的KPI明细记录
                //        List<T_HR_KPIRECORD> kpiRecordList = (new KPIRecordBll()).GetKPIRecordByUserIDForSum(record.APPRAISEEID, entTemp.SUMSTART.Value, entTemp.SUMEND.Value);
                //        //个人KPI记录得分
                //        decimal? kpiScore = 0;
                //        if (kpiRecordList != null && kpiRecordList.Count > 0)
                //        {
                //            foreach (T_HR_KPIRECORD kpiRecord in kpiRecordList)
                //            {
                //                kpiScore += kpiRecord.SUMSCORE;
                //                //新建绩效考核和KPI明细的关联记录
                //                T_HR_PERFORMANCEDETAIL detail = new T_HR_PERFORMANCEDETAIL();
                //                detail.PERFORMANCEDETAILID = Guid.NewGuid().ToString();
                //                detail.T_HR_KPIRECORD = kpiRecord;//关联KPI
                //                detail.T_HR_PERFORMANCERECORD = record;//关联绩效考核
                //                record.T_HR_PERFORMANCEDETAIL.Add(detail);
                //                //断开关联
                //                Utility.RefreshEntity(detail);
                //            }
                //            //计算个人KPI明细记录平均分
                //            kpiScore = decimal.Round(kpiScore.Value / kpiRecordList.Count, 2);
                //        }
                //        //计算个人KPI明细记录平均分
                //        record.PERFORMANCESCORE = kpiScore;
                //        score += record.PERFORMANCESCORE;
                //    }
                //    //计算部门绩效考核平均分
                //    entTemp.SUMSCORE = decimal.Round(score.Value / recordList.Count, 2);
                //}

                //entTemp.UPDATEDATE = System.DateTime.Now;
                //entTemp.CREATEDATE = System.DateTime.Now;
                //dal.Add(entTemp);
                #endregion
                //绩效考核信息
                if (entType.T_HR_PERFORMANCERECORD != null)
                {
                    List<T_HR_PERFORMANCERECORD> recordList = entType.T_HR_PERFORMANCERECORD.ToList();
                    decimal? score = 0;
                    foreach (T_HR_PERFORMANCERECORD record in recordList)
                    {
                        //添加绩效考核记录
                        entType.T_HR_PERFORMANCERECORD.Add(record);
                        //断开关联
                        //Utility.RefreshEntity(record);

                        //获取相关的KPI明细记录
                        List<T_HR_KPIRECORD> kpiRecordList = (new KPIRecordBll()).GetKPIRecordByUserIDForSum(record.APPRAISEEID, entType.SUMSTART.Value, entType.SUMEND.Value);
                        //个人KPI记录得分
                        decimal? kpiScore = 0;
                        if (kpiRecordList != null && kpiRecordList.Count > 0)
                        {
                            foreach (T_HR_KPIRECORD kpiRecord in kpiRecordList)
                            {
                                // 1s 冉龙军
                                //kpiScore += kpiRecord.SUMSCORE;
                                if (kpiRecord.SUMSCORE != null)
                                {
                                    kpiScore += kpiRecord.SUMSCORE;
                                }
                                // 1e
                                //新建绩效考核和KPI明细的关联记录
                                T_HR_PERFORMANCEDETAIL detail = new T_HR_PERFORMANCEDETAIL();
                                detail.PERFORMANCEDETAILID = Guid.NewGuid().ToString();
                                //detail.T_HR_KPIRECORD = kpiRecord;//关联KPI
                                detail.T_HR_KPIRECORDReference = new EntityReference<T_HR_KPIRECORD>();
                                detail.T_HR_KPIRECORDReference.EntityKey =
                                      new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIRECORD", "KPIRECORDID", kpiRecord.KPIRECORDID);
                                //detail.T_HR_PERFORMANCERECORD = record;//关联绩效考核
                                detail.T_HR_PERFORMANCERECORDReference = new EntityReference<T_HR_PERFORMANCERECORD>();
                                detail.T_HR_PERFORMANCERECORDReference.EntityKey =
                                       new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_PERFORMANCERECORD", "PERFORMANCEID", record.PERFORMANCEID);
                                record.T_HR_PERFORMANCEDETAIL.Add(detail);
                                //断开关联
                                //Utility.RefreshEntity(detail);
                            }
                            //计算个人KPI明细记录平均分
                            kpiScore = decimal.Round(kpiScore.Value / kpiRecordList.Count, 2);
                        }
                        //计算个人KPI明细记录平均分
                        record.PERFORMANCESCORE = kpiScore;
                        score += record.PERFORMANCESCORE;
                    }
                    //计算部门绩效考核平均分
                    entType.SUMSCORE = decimal.Round(score.Value / recordList.Count, 2);
                }

                entType.UPDATEDATE = System.DateTime.Now;
                entType.CREATEDATE = System.DateTime.Now;
                //dal.Add(entType);
                Add(entType);
                // 1e
                //提交事务
                //tran.Commit();
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                //if (tran != null)
                //    tran.Rollback();
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddSumPerformance:" + ex.Message);
                throw ex;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 更新汇总信息，不重新汇总
        /// </summary>
        /// <param name="entType"></param>
        public void UpdateSumPerformance(T_HR_SUMPERFORMANCERECORD entType)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.SUMNAME == entType.SUMNAME
                   && s.SUMID != entType.SUMID);
                if (temp != null)
                {
                    throw new Exception("Repetition");
                }
                var ents = from ent in dal.GetObjects()
                           where ent.SUMID == entType.SUMID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    entType.UPDATEDATE = System.DateTime.Now;
                    Utility.CloneEntity(entType, ent);
                    //  dal.Update(ent);
                    Update(ent);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateSumPerformance:" + ex.Message);
                throw (ex);
            }
        }

        /// <summary>
        /// 更新汇总信息，并且重新汇总
        /// </summary>
        /// <param name="entType"></param>
        public void UpdateSumPerformanceAndSum(T_HR_SUMPERFORMANCERECORD entType)
        {
            // System.Data.Common.DbTransaction tran = null;
            try
            {
                // 1s 冉龙军
                //DataContext.Connection.Open();
                ////if (DataContext.Connection.State == System.Data.ConnectionState.Closed)
                ////{
                ////    DataContext.Connection.Open();
                ////}
                ////// 1e
                ////tran = DataContext.Connection.BeginTransaction();
                dal.BeginTransaction();

                //实体不存在
                if (entType == null)
                    throw new Exception("{REQUIREDFIELDS}");

                //评分方式不存在
                if (entType.T_HR_PERFORMANCERECORD == null || entType.T_HR_PERFORMANCERECORD.Count == 0)
                    throw new Exception("{PERSONREQUIREDFIELDS}");

                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.SUMNAME == entType.SUMNAME
                || s.SUMID != entType.SUMID);
                if (tempEnt != null)
                {
                    throw new Exception("Repetition");
                }
                //获取汇总信息
                var ents = from ent in dal.GetObjects().Include("T_HR_PERFORMANCERECORD.T_HR_PERFORMANCEDETAIL")
                           where ent.SUMID == entType.SUMID
                           select ent;
                //获取实体
                var entTemp = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                #region 删除子表信息
                //删除子表信息
                if (entTemp != null)
                {
                    //删除个人绩效考核记录
                    entTemp.T_HR_PERFORMANCERECORD.ToList().ForEach(performance =>
                    {
                        //删除考核和KPI关联记录
                        performance.T_HR_PERFORMANCEDETAIL.ToList().ForEach(detail =>
                        {
                            Utility.RefreshEntity(detail);
                            dal.DeleteFromContext(detail);
                        });
                        Utility.RefreshEntity(performance);
                        dal.DeleteFromContext(performance);
                    });
                }
                #endregion 删除子表信息

                Utility.CloneEntity<T_HR_SUMPERFORMANCERECORD>(entType, entTemp);
                //断开关联
                Utility.RefreshEntity(entTemp);

                #region 重新计算绩效考核信息
                //绩效考核信息
                if (entType.T_HR_PERFORMANCERECORD != null)
                {
                    List<T_HR_PERFORMANCERECORD> recordList = entType.T_HR_PERFORMANCERECORD.ToList();
                    decimal? score = 0;
                    foreach (T_HR_PERFORMANCERECORD record in recordList)
                    {
                        //添加绩效考核记录
                        entTemp.T_HR_PERFORMANCERECORD.Add(record);
                        //断开关联
                        Utility.RefreshEntity(record);

                        //获取相关的KPI明细记录
                        List<T_HR_KPIRECORD> kpiRecordList = (new KPIRecordBll()).GetKPIRecordByUserIDForSum(record.APPRAISEEID, entTemp.SUMSTART.Value, entTemp.SUMEND.Value);
                        //个人KPI记录得分
                        decimal? kpiScore = 0;
                        foreach (T_HR_KPIRECORD kpiRecord in kpiRecordList)
                        {
                            kpiScore += kpiRecord.SUMSCORE;
                            //新建绩效考核和KPI明细的关联记录
                            T_HR_PERFORMANCEDETAIL detail = new T_HR_PERFORMANCEDETAIL();
                            detail.PERFORMANCEDETAILID = Guid.NewGuid().ToString();
                            detail.T_HR_KPIRECORD = kpiRecord;//关联KPI
                            detail.T_HR_PERFORMANCERECORD = record;//关联绩效考核
                            record.T_HR_PERFORMANCEDETAIL.Add(detail);
                            //断开关联
                            Utility.RefreshEntity(detail);
                        }
                        //计算个人KPI明细记录平均分
                        record.PERFORMANCESCORE = decimal.Round(kpiScore.Value / kpiRecordList.Count, 2);
                        score += record.PERFORMANCESCORE;
                    }
                    //计算部门绩效考核平均分
                    entTemp.SUMSCORE = decimal.Round(score.Value / recordList.Count, 2);
                }
                #endregion 重新计算绩效考核信息

                entTemp.UPDATEDATE = System.DateTime.Now;
                entTemp.CREATEDATE = System.DateTime.Now;
                dal.SaveContextChanges();
                //提交事务
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateSumPerformanceAndSum:" + ex.Message);
                throw ex;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 删除汇总信息和其子表信息
        /// </summary>
        /// <param name="sumId"></param>
        /// <returns></returns>
        public int DeleteSumPerformance(string sumId)
        {
            // System.Data.Common.DbTransaction tran = null;
            try
            {
                //if(DataContext.Connection.State != System.Data.ConnectionState.Open)
                //    DataContext.Connection.Open();
                //tran = DataContext.Connection.BeginTransaction();

                var ents = from e in dal.GetObjects().Include("T_HR_PERFORMANCERECORD.T_HR_PERFORMANCEDETAIL")
                           where e.SUMID == sumId
                           select e;
                //获取实体
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    //删除个人绩效考核记录
                    ent.T_HR_PERFORMANCERECORD.ToList().ForEach(performance =>
                    {
                        //删除考核和KPI关联记录
                        performance.T_HR_PERFORMANCEDETAIL.ToList().ForEach(detail =>
                        {
                            Utility.RefreshEntity(detail);
                            dal.DeleteFromContext(detail);
                        });
                        Utility.RefreshEntity(performance);
                        dal.DeleteFromContext(performance);
                    });

                    dal.DeleteFromContext(ent);
                    DeleteMyRecord(ent);
                }
                int i = dal.SaveContextChanges();
                //提交事务

                return i;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteSumPerformance:" + ex.Message);
                throw ex;
            }
            finally
            {

            }
        }

        public int DeleteSumPerformances(string[] sumIdList)
        {
            /// System.Data.Common.DbTransaction tran = null;
            try
            {
                // 1s 冉龙军
                //DataContext.Connection.Open();

                // 1e
                dal.BeginTransaction();
                int i = 0;
                foreach (string sumid in sumIdList)
                {
                    i += DeleteSumPerformance(sumid);
                }
                //提交事务
                // tran.Commit();
                dal.CommitTransaction();
                return i;
            }
            catch (Exception ex)
            {
                //if (tran != null)
                //    tran.Rollback();
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteSumPerformances:" + ex.Message);
                throw ex;
            }
            finally
            {
            }
        }

        public void AddPerformanceRecord(T_HR_PERFORMANCERECORD entType)
        {
            throw new NotImplementedException();
        }

        public int AddPerformanceRecordList(List<T_HR_PERFORMANCERECORD> entList)
        {
            throw new NotImplementedException();
        }

        public void UpdatePerformanceRecord(T_HR_PERFORMANCERECORD entType)
        {
            throw new NotImplementedException();
        }

        public int[] UpdatePerformanceRecordList(List<T_HR_PERFORMANCERECORD> entList, string[] employeeIDs)
        {
            throw new NotImplementedException();
        }

        public int DeletePerformanceRecord(string randomGroupId)
        {
            throw new NotImplementedException();
        }

        public int DeletePerformanceRecords(string[] groupPersonIDs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取人员绩效考核平均分
        /// </summary>
        /// <param name="employeIDs">人员列表</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public Dictionary<string, decimal> GetEmployePerformance(List<string> employeIDs, DateTime startTime, DateTime endTime)
        {
            //返回的列表
            Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
            try
            {
                //过滤条件
                string filterString = string.Empty;
                //参数
                List<object> queryParas = new List<object>();
                //设置过滤条件和参数
                for (int i = 0; i < employeIDs.Count; i++)
                {
                    //if (!string.IsNullOrEmpty(filterString))
                    //    filterString += " OR ";
                    //filterString += "APPRAISEEID==@" + (i + 1).ToString();
                    //queryParas.Add(employeIDs[i]);
                    filterString += filterString.Equals(string.Empty) ? employeIDs[i] : "," + employeIDs[i];
                }
                if (startTime > endTime)
                {
                    DateTime temp = new DateTime();
                    temp = startTime;
                    startTime = endTime;
                    endTime = temp;
                }
                string[] w = filterString.Split(',');
                //搜索KPI记录的平均值
                var q = from st in dal.GetObjects<T_HR_KPIRECORD>().ToList()
                        where st.UPDATEDATE > startTime && st.UPDATEDATE < endTime && st.SUMSCORE != null
                        && w.Contains(st.APPRAISEEID)
                        group st by st.APPRAISEEID into g
                        select new
                        {
                            APPRAISEEID = g.Key,
                            AverageScore = g.Average(p => p.SUMSCORE)
                        };
                //过滤结果
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    q = q.Where(filterString, queryParas.ToArray());
                //}
                //将数据加入载体
                foreach (var a in q)
                {
                    dic.Add(a.APPRAISEEID, a.AverageScore.Value);
                }
                return dic;
            }
            catch (Exception expt)
            {
                dic.Add(expt.Message, -1);
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteSumPerformances:" + expt.Message);
                return dic;
            }
        }
        // 1s 冉龙军
        /// <summary>
        /// 获取人员绩效考核平均分（接口专用）
        /// </summary>
        /// <param name="employeIDs">人员列表</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public Dictionary<string, decimal> GetEmployePerformanceInterface(List<string> employeIDs, DateTime startTime, DateTime endTime)
        {
            //返回的列表
            Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
            try
            {
                //过滤条件
                string filterString = string.Empty;
                //参数
                List<object> queryParas = new List<object>();
                //设置过滤条件和参数
                for (int i = 0; i < employeIDs.Count; i++)
                {
                    filterString += filterString.Equals(string.Empty) ? employeIDs[i] : "," + employeIDs[i];
                }
                if (startTime > endTime)
                {
                    DateTime temp = new DateTime();
                    temp = startTime;
                    startTime = endTime;
                    endTime = temp;
                }
                string[] w = filterString.Split(',');
                //搜索KPI记录的平均值
                //var q = from st in dal.DataContext.T_HR_KPIRECORD.ToList()
                //        where st.UPDATEDATE > startTime && st.UPDATEDATE < endTime && st.SUMSCORE != null
                //        && w.Contains(st.APPRAISEEID)
                //        group st by st.APPRAISEEID into g
                //        select new
                //        {
                //            APPRAISEEID = g.Key,
                //            AverageScore = g.Average(p => p.SUMSCORE)
                //        };
                var q = from ssp in dal.GetObjects()
                        join spr in dal.GetObjects<T_HR_PERFORMANCERECORD>() on ssp.SUMID equals spr.T_HR_SUMPERFORMANCERECORD.SUMID
                        join spd in dal.GetObjects<T_HR_PERFORMANCEDETAIL>() on spr.PERFORMANCEID equals spd.T_HR_PERFORMANCERECORD.PERFORMANCEID
                        join st in dal.GetObjects<T_HR_KPIRECORD>() on spd.T_HR_KPIRECORD.KPIRECORDID equals st.KPIRECORDID
                        where ssp.CHECKSTATE == "2" && st.UPDATEDATE >= startTime && st.UPDATEDATE <= endTime
                            && st.SUMSCORE != null && w.Contains(st.APPRAISEEID)
                        group st by st.APPRAISEEID into g
                        select new
                        {
                            APPRAISEEID = g.Key,
                            AverageScore = g.Average(p => p.SUMSCORE)
                        };
                //过滤结果
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    q = q.Where(filterString, queryParas.ToArray());
                //}
                //将数据加入载体
                foreach (var a in q)
                {
                    dic.Add(a.APPRAISEEID, a.AverageScore.Value);
                }
                return dic;
            }
            catch (Exception expt)
            {
                dic.Add(expt.Message, -1);
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetEmployePerformanceInterface:" + expt.Message);
                return dic;
            }
        }
        // 1e

        #region /// author 喻建华  time:2010-09-08  10:00

        /// begin
        /// <summary>
        /// 获取绩效考核汇总记录实体
        /// </summary>
        /// <param name="sumid"></param>
        /// <returns></returns>
        public T_HR_SUMPERFORMANCERECORD GetSumPerformanceRecordByID(string sumid)
        {
            var ents = from a in dal.GetObjects()
                       where a.SUMID == sumid
                       select a;
            if (ents.Count() > 0)
            {
                return ents.FirstOrDefault() != null ? ents.FirstOrDefault() : null;
            }
            return null;
        }
        ///  end
        #endregion

        #region 绩效外部接口
        public List<V_EMPLOYEEVIEW> GetPerformenceEmployee(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int Count, ref int pageCount, string sType, string sValue, string userID, string startTime, string endTime, int sartlevel, int endlevel)
        {
            try
            {
                IQueryable<V_PERFORMANCERECORD> q = GetEmployeePerformancePagingByTime(pageIndex, pageSize, sort,
                                                                                           filterString,
                                                                                           paras, ref pageCount, sType,
                                                                                           sValue, userID, startTime,
                                                                                          endTime);

                List<V_PERFORMANCERECORD> entViews = new List<V_PERFORMANCERECORD>();
                if (q != null)
                {
                    entViews = q.ToList();
                }

                var idnumbers = (from a in entViews
                                 join b in dal.GetObjects<T_HR_EMPLOYEE>()
                                 on a.OWNERID equals b.EMPLOYEEID
                                 select b.IDNUMBER).ToArray();
                if (idnumbers.Length > 0)
                {
                    EmployeeBLL employeebll = new EmployeeBLL();
                    var ents = employeebll.GetEmployeesByIdnumber(idnumbers);
                    ents.Skip(sartlevel).Take(endlevel);
                    if (ents.Count > 0) Count = ents.Count;
                    return ents.Count > 0 ? ents.ToList() : null;
                }
                return null;
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 绩效控件内进行手动打分后，记录被考核人的个人打分情况（明细及汇总）
        /// </summary>
        /// <param name="record"></param>
        public void SaveMyRecordByKPIRd(T_HR_KPIRECORD record)
        {
            //检查
            var sums = from pd in dal.GetObjects<T_HR_PERFORMANCEDETAIL>().Include("T_HR_PERFORMANCERECORD").Include("T_HR_KPIRECORD")
                       where pd.T_HR_KPIRECORD.KPIRECORDID == record.KPIRECORDID
                       select pd;

            var kpis = from k in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT")
                       where k.KPIRECORDID == record.KPIRECORDID
                       select k;

            if (kpis == null)
            {
                return;
            }

            if (kpis.FirstOrDefault() == null)
            {
                return;
            }

            T_HR_KPIRECORD entKPIRd = kpis.FirstOrDefault();

            if (sums == null)
            {
                AddPerformanceRdAndSumsRd(entKPIRd);
            }
            else
            {
                if (sums.Count() > 0)
                {
                    UpdatePerformanceRdAndSumsRd(entKPIRd);
                }
                else
                {
                    AddPerformanceRdAndSumsRd(entKPIRd);
                }
            }
        }

        /// <summary>
        /// 添加被考核人的个人打分情况（明细及汇总）
        /// </summary>
        /// <param name="record"></param>
        private void AddPerformanceRdAndSumsRd(T_HR_KPIRECORD record)
        {
            EmployeeBLL bllEmp = new EmployeeBLL();

            V_EMPLOYEEDETAIL entEmpView = bllEmp.GetEmployeeDetailView(record.APPRAISEEID);
            if (entEmpView == null)
            {
                Utility.SaveLog("添加被考核人的个人打分情况（明细及汇总）失败，当前被考核人不存在。关键信息：T_HR_KPIRECORD.APPRAISEEID:" + record.APPRAISEEID);
                return;
            }

            if (entEmpView.EMPLOYEEPOSTS == null)
            {
                Utility.SaveLog("添加被考核人的个人打分情况（明细及汇总）失败，当前被考核人的岗位信息无效。关键信息：T_HR_KPIRECORD.APPRAISEEID:" + record.APPRAISEEID);
                return;
            }

            if (entEmpView.EMPLOYEEPOSTS.Count() == 0)
            {
                Utility.SaveLog("添加被考核人的个人打分情况（明细及汇总）失败，当前被考核人的岗位信息无效。关键信息：T_HR_KPIRECORD.APPRAISEEID:" + record.APPRAISEEID);
                return;
            }

            if (record == null)
            {
                return;
            }

            if (record.T_HR_KPIPOINT == null)
            {
                return;
            }

            //检查
            var sums = from pd in dal.GetObjects<T_HR_PERFORMANCEDETAIL>().Include("T_HR_PERFORMANCERECORD").Include("T_HR_KPIRECORD").Include("T_HR_PERFORMANCERECORD.T_HR_SUMPERFORMANCERECORD")
                       where pd.T_HR_KPIRECORD.T_HR_KPIPOINT.KPIPOINTID == record.T_HR_KPIPOINT.KPIPOINTID
                       select pd;

            T_HR_PERFORMANCERECORD entPerRd = null;
            T_HR_SUMPERFORMANCERECORD entSumRd = null;

            bool bIsUpdate = false;
            if (sums != null)
            {
                if (sums.Count() > 0)
                {
                    entPerRd = sums.FirstOrDefault().T_HR_PERFORMANCERECORD;
                }
                else
                {
                    entPerRd = new T_HR_PERFORMANCERECORD();
                }
            }


            if (entPerRd.T_HR_SUMPERFORMANCERECORD != null)
            {
                entSumRd = entPerRd.T_HR_SUMPERFORMANCERECORD;
                bIsUpdate = true;
            }
            else
            {
                entSumRd = new T_HR_SUMPERFORMANCERECORD();
            }

            //添加汇总记录
            entSumRd.SUMDEPTID = entEmpView.EMPLOYEEPOSTS[0].DepartmentID;
            entSumRd.SUMPERSONID = record.APPRAISEEID;
            entSumRd.SUMSTART = record.CREATEDATE;
            entSumRd.SUMEND = record.CREATEDATE;
            entSumRd.SUMREMARK = record.REMARK;
            entSumRd.SUMDATE = record.CREATEDATE;
            entSumRd.CREATEDATE = DateTime.Now;
            entSumRd.CREATEUSERID = entEmpView.EMPLOYEEID;
            entSumRd.CREATEPOSTID = entEmpView.EMPLOYEEPOSTS[0].POSTID;
            entSumRd.CREATEDEPARTMENTID = entEmpView.EMPLOYEEPOSTS[0].DepartmentID;
            entSumRd.CREATECOMPANYID = entEmpView.EMPLOYEEPOSTS[0].CompanyID;

            entSumRd.OWNERID = entEmpView.EMPLOYEEID;
            entSumRd.OWNERPOSTID = entEmpView.EMPLOYEEPOSTS[0].POSTID;
            entSumRd.OWNERDEPARTMENTID = entEmpView.EMPLOYEEPOSTS[0].DepartmentID;
            entSumRd.OWNERCOMPANYID = entEmpView.EMPLOYEEPOSTS[0].CompanyID;

            if (bIsUpdate)
            {
                entSumRd.SUMCOUNT += 1;
                entSumRd.SUMSCORE += record.SUMSCORE;
                dal.UpdateFromContext(entSumRd);
            }
            else
            {
                entSumRd.SUMID = System.Guid.NewGuid().ToString().ToUpper();
                entSumRd.SUMCOUNT = 1;  //初次添加，计数：1
                entSumRd.SUMSCORE = record.SUMSCORE;
                dal.AddToContext(entSumRd);
            }
            dal.SaveContextChanges();

            //添加被考核人的个人打分明细，保证与KPI明细（T_HR_KPIRECORD）记录一对多关系            
            entPerRd.APPRAISEEID = record.APPRAISEEID;
            entPerRd.PERFORMANCEREMARK = record.REMARK;
            entPerRd.T_HR_SUMPERFORMANCERECORD = entSumRd;

            if (entPerRd.T_HR_SUMPERFORMANCERECORD != null)
            {
                entPerRd.T_HR_SUMPERFORMANCERECORDReference.EntityKey = new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_SUMPERFORMANCERECORD", "SUMID", entPerRd.T_HR_SUMPERFORMANCERECORD.SUMID);
            }

            if (bIsUpdate)
            {
                entPerRd.PERFORMANCESCORE += record.SUMSCORE;
                dal.UpdateFromContext(entPerRd);
            }
            else
            {
                entPerRd.PERFORMANCEID = System.Guid.NewGuid().ToString().ToUpper();
                entPerRd.PERFORMANCESCORE = record.SUMSCORE;
                dal.AddToContext(entPerRd);
            }

            dal.SaveContextChanges();

            //建立KPI明细(T_HR_KPIRECORD)记录 与 被考核人 绩效考核记录（T_HR_PERFORMANCERECORD）的关系
            T_HR_PERFORMANCEDETAIL entPersDetail = new T_HR_PERFORMANCEDETAIL();
            entPersDetail.PERFORMANCEDETAILID = System.Guid.NewGuid().ToString().ToUpper();
            entPersDetail.T_HR_KPIRECORDReference.EntityKey = new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_KPIRECORD", "KPIRECORDID", record.KPIRECORDID);
            entPersDetail.T_HR_PERFORMANCERECORDReference.EntityKey = new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_PERFORMANCERECORD", "PERFORMANCEID", entPerRd.PERFORMANCEID);

            dal.Add(entPersDetail);
        }

        /// <summary>
        /// 修改被考核人的个人打分情况（明细及汇总）
        /// </summary>
        /// <param name="record"></param>
        private void UpdatePerformanceRdAndSumsRd(T_HR_KPIRECORD record)
        {
            if (string.IsNullOrWhiteSpace(record.APPRAISEEID))
            {
                return;
            }


            EmployeeBLL bllEmp = new EmployeeBLL();
            V_EMPLOYEEDETAIL entEmpView = bllEmp.GetEmployeeDetailView(record.APPRAISEEID);
            if (entEmpView == null)
            {
                Utility.SaveLog("添加被考核人的个人打分情况（明细及汇总）失败，当前被考核人不存在。关键信息：T_HR_KPIRECORD.APPRAISEEID:" + record.APPRAISEEID);
                return;
            }

            if (entEmpView.EMPLOYEEPOSTS == null)
            {
                Utility.SaveLog("添加被考核人的个人打分情况（明细及汇总）失败，当前被考核人的岗位信息无效。关键信息：T_HR_KPIRECORD.APPRAISEEID:" + record.APPRAISEEID);
                return;
            }

            if (entEmpView.EMPLOYEEPOSTS.Count() == 0)
            {
                Utility.SaveLog("添加被考核人的个人打分情况（明细及汇总）失败，当前被考核人的岗位信息无效。关键信息：T_HR_KPIRECORD.APPRAISEEID:" + record.APPRAISEEID);
                return;
            }

            if (record == null)
            {
                return;
            }

            if (record.T_HR_KPIPOINT == null)
            {
                return;
            }


            //检查
            var sums = from pd in dal.GetObjects<T_HR_PERFORMANCEDETAIL>().Include("T_HR_PERFORMANCERECORD").Include("T_HR_KPIRECORD").Include("T_HR_PERFORMANCERECORD.T_HR_SUMPERFORMANCERECORD")
                       where pd.T_HR_KPIRECORD.T_HR_KPIPOINT.KPIPOINTID == record.T_HR_KPIPOINT.KPIPOINTID
                       select pd;

            T_HR_PERFORMANCERECORD entPerRd = null;
            T_HR_SUMPERFORMANCERECORD entSumRd = null;
            bool bIsUpdate = false;
            if (sums != null)
            {
                if (sums.Count() > 0)
                {
                    entPerRd = sums.FirstOrDefault().T_HR_PERFORMANCERECORD;
                }
            }

            if (entPerRd != null)
            {
                if (entPerRd.T_HR_SUMPERFORMANCERECORD != null)
                {
                    entSumRd = entPerRd.T_HR_SUMPERFORMANCERECORD;
                    bIsUpdate = true;
                }
            }

            //添加汇总记录            

            if (bIsUpdate)
            {
                entSumRd.SUMCOUNT = sums.Count();
                entSumRd.SUMSCORE = sums.Sum(c => c.T_HR_KPIRECORD.SUMSCORE);
                entSumRd.UPDATEDATE = DateTime.Now;
                dal.UpdateFromContext(entSumRd);
                dal.SaveContextChanges();
            }



            //添加被考核人的个人打分明细，保证与KPI明细（T_HR_KPIRECORD）记录一对多关系            

            if (entPerRd.T_HR_SUMPERFORMANCERECORD != null)
            {
                entPerRd.T_HR_SUMPERFORMANCERECORDReference.EntityKey = new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_SUMPERFORMANCERECORD", "SUMID", entPerRd.T_HR_SUMPERFORMANCERECORD.SUMID);
            }

            if (bIsUpdate)
            {
                entPerRd.PERFORMANCESCORE = sums.Sum(c => c.T_HR_KPIRECORD.SUMSCORE);
                entPerRd.PERFORMANCEREMARK = record.REMARK;
                dal.UpdateFromContext(entPerRd);
                dal.SaveContextChanges();
            }
        }
    }
}
