/// <summary>
/// Log No.： 1
/// Modify Desc： 使用V_KPIRECORD（用户和业务信息）,汇总LINQ（暂时），接口专用
/// Modifier： 冉龙军
/// Modify Date： 2010-08-10
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

namespace SMT.HRM.BLL
{
    public class KPIRecordBll : BaseBll<T_HR_KPIRECORD>
    {
        /// <summary>
        /// 添加KPI明细记录
        /// </summary>
        /// <param name="entity">KPIRecord实例</param>
        public void KPIRecordAdd(T_HR_KPIRECORD entity)
        {
            try
            {
                #region// 1s 冉龙军
                //var tempEnt = DataContext.T_HR_KPIRECORD.FirstOrDefault(s => s.BUSINESSCODE == entity.BUSINESSCODE
                //    //|| s.FlowRecordCode == entity.FlowRecordCode
                //    //|| s.StepDetailCode == entity.StepDetailCode
                //);
                //if (tempEnt != null)
                //{
                //    throw new Exception("Repetition");
                //}
                //T_HR_KPIRECORD ent = new T_HR_KPIRECORD();
                //Utility.CloneEntity<T_HR_KPIRECORD>(entity, ent);
                //Utility.RefreshEntity(ent);

                ////如果父公司为空，就不赋值
                ////if (entity.T_HR_KPIPOINT != null)
                ////{
                ////    ent.T_HR_KPIPOINTReference.EntityKey =
                ////        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIRECORD", "KPIPIONTID", entity.T_HR_KPIPOINT.KPIPOINTID);
                ////}
                //dal.Add(ent);
                // 2s 冉龙军
                #endregion
                // 打分时可能要修改
                var tempEnt = dal.GetObjects<T_HR_KPIRECORD>().FirstOrDefault(s => s.BUSINESSCODE == entity.BUSINESSCODE
                    && s.T_HR_KPIPOINT.KPIPOINTID == entity.T_HR_KPIPOINT.KPIPOINTID && s.KPIRECORDID == entity.KPIRECORDID
                    //|| s.FlowRecordCode == entity.FlowRecordCode
                    //|| s.StepDetailCode == entity.StepDetailCode
                );
                if (tempEnt != null)
                {
                    throw new Exception("Repetition");
                }
                //if (entity == null || entity.APPRAISEEID == "")
                //{
                //    return;
                //}
                // 2e

                string KPIPointID = entity.T_HR_KPIPOINT.KPIPOINTID;
                entity.T_HR_KPIPOINT = null;
                if (entity.T_HR_KPIPOINTReference == null)
                {
                    entity.T_HR_KPIPOINTReference = new EntityReference<T_HR_KPIPOINT>();
                }
                entity.T_HR_KPIPOINTReference.EntityKey =
                       new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIPOINT", "KPIPOINTID", KPIPointID);
                dal.Add(entity);
                // 1e
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " KPIRecordAdd:" + ex.Message);
                throw (ex);
            }
        }
        // 1s 冉龙军
        /// <summary>
        /// 添加KPI明细记录（接口专用）
        /// </summary>
        /// <param name="entity">KPIRecord实例</param>
        public void KPIRecordAddInterface(T_HR_KPIRECORD entity)
        {
            try
            {

                var tempEnt = dal.GetObjects<T_HR_KPIRECORD>().FirstOrDefault(s => s.BUSINESSCODE == entity.BUSINESSCODE);
                if (tempEnt != null)
                {
                    throw new Exception("Repetition");
                }

                dal.Add(entity);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " KPIRecordAddInterface:" + ex.Message);
                throw (ex);
            }
        }
        // 1e
        /// <summary>
        /// 变更KPI明细记录
        /// </summary>
        /// <param name="entity">公司实例</param>
        public void KPIRecordUpdate(T_HR_KPIRECORD entity)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_HR_KPIRECORD>()
                           where ent.KPIRECORDID == entity.KPIRECORDID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();

                    Utility.CloneEntity<T_HR_KPIRECORD>(entity, ent);
                    ent.COMPLAINSTATUS = entity.COMPLAINSTATUS;
                    if (entity.T_HR_KPIPOINT != null)
                    {
                        ent.T_HR_KPIPOINTReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIPOINT", "KPIPOINTID", entity.T_HR_KPIPOINT.KPIPOINTID);

                        //    ent.T_HR_KPIPOINT.EntityKey =
                        //new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_KPIPOINT", "KPIPOINTID", entity.T_HR_KPIPOINT.KPIPOINTID);

                    }
                    Utility.RefreshEntity(ent);

                    int i = dal.Update(ent);
                    SMT.Foundation.Log.Tracer.Debug("考核记录ID：" + ent.KPIRECORDID + "更新标志:" + i.ToString() + " " + System.DateTime.Now.ToString());

                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + ex.Message + " 更新绩效记录分数");
            }
        }


        /// <summary>
        /// 获取薪资发放实体(一条)
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回薪资发放实体(一条)</returns>
        public T_HR_KPIRECORD GetKPIRecord(string formCode, string flowID, string stepID)
        {
            try
            {
                // 1s 冉龙军
                // 改stepID为stepCode（当流程中有重复的步骤时，此方法不可取）
                //var ents = from a in DataContext.T_HR_KPIRECORD
                //           where a.STEPDETAILCODE == stepID
                //           && a.BUSINESSCODE == formCode
                //           //&& a.FLOWRECORDCODE == flowID//暂时不考虑流程ID，因为没有存入流程ID
                //           select a;
                var ents = from a in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT").Include("T_HR_KPIPOINT.T_HR_SCORETYPE")
                           where a.T_HR_KPIPOINT.STEPID == stepID
                           && a.BUSINESSCODE == formCode
                           //&& a.FLOWRECORDCODE == flowID//暂时不考虑流程ID，因为没有存入流程ID
                           select a;
                // 1e
                if (ents.Count() > 0)
                {
                    return ents.ToList().OrderByDescending(s => s.CREATEDATE).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetKPIRecord:" + ex.Message);
                throw ex;
            }
            return null;
        }
        // 1s 冉龙军
        /// <summary>
        /// 获取薪资发放实体(一条)（接口专用）
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回薪资发放实体(一条)</returns>
        public T_HR_KPIRECORD GetKPIRecordInterface(string formCode)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_HR_KPIRECORD>()
                           where a.BUSINESSCODE == formCode
                           select a;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetKPIRecordInterface:" + ex.Message);
                throw ex;
            }
            return null;
        }
        // 1e
        // 1s
        //public IQueryable<T_HR_KPIRECORD> GetKPIRecordPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID)
        //{
        //    List<object> queryParas = new List<object>();
        //    queryParas.AddRange(paras);

        //    //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_KPIRECORD");
        //    //员工入职审核通过再显示。

        //    ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
        //    ///

        //    IQueryable<T_HR_KPIRECORD> ents = DataContext.T_HR_KPIRECORD.Include("T_HR_KPIPOINT");
        //    switch (sType)
        //    {
        //        case "Company":
        //            ents = from o in DataContext.T_HR_KPIRECORD.Include("T_HR_KPIPOINT")
        //                   join ep in DataContext.T_HR_EMPLOYEEPOST on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in DataContext.T_HR_POST on ep.T_HR_POST.POSTID equals p.POSTID
        //                   join d in DataContext.T_HR_DEPARTMENT on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
        //                   join c in DataContext.T_HR_COMPANY on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
        //                   where c.COMPANYID == sValue
        //                   select o;
        //            break;
        //        case "Department":
        //            ents = from o in DataContext.T_HR_KPIRECORD.Include("T_HR_KPIPOINT")
        //                   join ep in DataContext.T_HR_EMPLOYEEPOST on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in DataContext.T_HR_POST on ep.T_HR_POST.POSTID equals p.POSTID
        //                   join d in DataContext.T_HR_DEPARTMENT on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
        //                   where d.DEPARTMENTID == sValue
        //                   select o;
        //            break;
        //        case "Post":
        //            ents = from o in DataContext.T_HR_KPIRECORD.Include("T_HR_KPIPOINT")
        //                   join ep in DataContext.T_HR_EMPLOYEEPOST on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in DataContext.T_HR_POST on ep.T_HR_POST.POSTID equals p.POSTID
        //                   where p.POSTID == sValue
        //                   select o;
        //            break;
        //    }
        //    if (!string.IsNullOrEmpty(filterString))
        //    {
        //        ents = ents.Where(filterString, queryParas.ToArray());
        //    }
        //    ents = ents.OrderBy(sort);

        //    ents = Utility.Pager<T_HR_KPIRECORD>(ents, pageIndex, pageSize, ref pageCount);
        //    return ents;
        //}
        /// <summary>
        /// 根据参数获取KPI明细记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public IQueryable<V_KPIRECORD> GetKPIRecordPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID, string startDate, string endDate, string strCheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_KPIRECORD");
            //员工入职审核通过再显示。

            ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
            ///


            var ents = from a in dal.GetObjects<T_HR_KPIRECORD>()
                       join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.APPRAISEEID equals b.EMPLOYEEID
                       join d in dal.GetObjects<T_HR_KPIPOINT>() on a.T_HR_KPIPOINT.KPIPOINTID equals d.KPIPOINTID into TEMPKPI
                       from e in TEMPKPI.DefaultIfEmpty()
                       select new V_KPIRECORD
                       {
                           T_HR_KPIRECORD = a,
                           EMPLOYEECODE = b.EMPLOYEECODE,
                           EMPLOYEECNAME = b.EMPLOYEECNAME,
                           KPIPOINTREMARK = a.KPIDESCRIPTION,
                           FLOWID = a.FLOWDESCRIPTION,
                           CREATEUSERID = a.CREATEUSERID,
                           OWNERID = a.OWNERID,
                           OWNERPOSTID = a.OWNERPOSTID,
                           OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                           OWNERCOMPANYID = a.OWNERCOMPANYID
                       };

            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects<T_HR_KPIRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals b.EMPLOYEEID
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           join f in dal.GetObjects<T_HR_KPIPOINT>() on o.T_HR_KPIPOINT.KPIPOINTID equals f.KPIPOINTID into TEMPKPI
                           from e in TEMPKPI.DefaultIfEmpty()
                           where c.COMPANYID == sValue && ep.ISAGENCY == "0" && ep.EDITSTATE == "1"
                           select new V_KPIRECORD
                           {
                               T_HR_KPIRECORD = o,
                               EMPLOYEECODE = b.EMPLOYEECODE,
                               EMPLOYEECNAME = b.EMPLOYEECNAME,
                               KPIPOINTREMARK = o.KPIDESCRIPTION,
                               FLOWID = o.FLOWDESCRIPTION,
                               CREATEUSERID = o.CREATEUSERID,
                               OWNERID = o.OWNERID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               OWNERCOMPANYID = o.OWNERCOMPANYID
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT")
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals b.EMPLOYEEID
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join f in dal.GetObjects<T_HR_KPIPOINT>() on o.T_HR_KPIPOINT.KPIPOINTID equals f.KPIPOINTID into TEMPKPI
                           from e in TEMPKPI.DefaultIfEmpty()
                           where d.DEPARTMENTID == sValue && ep.ISAGENCY == "0" && ep.EDITSTATE == "1"
                           select new V_KPIRECORD
                           {
                               T_HR_KPIRECORD = o,
                               EMPLOYEECODE = b.EMPLOYEECODE,
                               EMPLOYEECNAME = b.EMPLOYEECNAME,
                               KPIPOINTREMARK = o.KPIDESCRIPTION,
                               FLOWID = o.FLOWDESCRIPTION,
                               CREATEUSERID = o.CREATEUSERID,
                               OWNERID = o.OWNERID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               OWNERCOMPANYID = o.OWNERCOMPANYID
                           };
                    break;
                case "Post":
                    ents = from o in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT")
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on o.APPRAISEEID equals b.EMPLOYEEID
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.APPRAISEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join f in dal.GetObjects<T_HR_KPIPOINT>() on o.T_HR_KPIPOINT.KPIPOINTID equals f.KPIPOINTID into TEMPKPI
                           from e in TEMPKPI.DefaultIfEmpty()
                           where p.POSTID == sValue && ep.ISAGENCY == "0" && ep.EDITSTATE == "1"
                           select new V_KPIRECORD
                           {
                               T_HR_KPIRECORD = o,
                               EMPLOYEECODE = b.EMPLOYEECODE,
                               EMPLOYEECNAME = b.EMPLOYEECNAME,
                               KPIPOINTREMARK = o.KPIDESCRIPTION,
                               FLOWID = o.FLOWDESCRIPTION,
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

            if (!string.IsNullOrEmpty(strCheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                filterString += " T_HR_KPIRECORD.COMPLAINSTATUS == @" + queryParas.Count();
                queryParas.Add(strCheckState);
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_KPIRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        // 1e

        /// <summary>
        /// 根据RecordID获取KPI明细记录
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public T_HR_KPIRECORD GetKPIRecordById(string recordId)
        {
            try
            {
                // 1s 冉龙军
                //var ents = from ent in dal.DataContext.T_HR_KPIRECORD.Include("T_HR_KPIPOINT.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RANDOMGROUPPERSON")
                //           where ent.KPIRECORDID == recordId
                //           select ent;
                var ents = from ent in dal.GetObjects<T_HR_KPIRECORD>().Include("T_HR_KPIPOINT.T_HR_SCORETYPE.T_HR_RANDOMGROUP")
                           where ent.KPIRECORDID == recordId
                           select ent;
                // 1e
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetKPIRecordById:" + ex.Message);
                throw ex;
            }
            return null;
        }

        public List<T_HR_KPIRECORD> GetKPIRecordByUserIDForSum(string userId, DateTime startTime, DateTime endTime)
        {
            try
            {
                // 1s 冉龙军
                //var ents = from ent in dal.DataContext.T_HR_KPIRECORD
                //           where ent.APPRAISEEID == userId && ent.SUMSCORE != null && ent.COMPLAINSTATUS == "1"
                //           && ent.UPDATEDATE > startTime && ent.UPDATEDATE < endTime
                //           select ent;
                var ents = from ent in dal.GetObjects<T_HR_KPIRECORD>()
                           where ent.APPRAISEEID == userId
                           && ent.UPDATEDATE > startTime && ent.UPDATEDATE < endTime
                           select ent;
                // 1e
                return ents.Count() > 0 ? ents.ToList() : null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetKPIRecordByUserIDForSum:" + ex.Message);
                throw ex;
            }
        }

        public bool CheckKPIRecordIsComplainingByUserID(List<string> userIdList, DateTime startTime, DateTime endTime)
        {
            try
            {
                foreach (string userId in userIdList)
                {
                    var ents = from ent in dal.GetObjects<T_HR_KPIRECORD>()
                               where ent.APPRAISEEID == userId && ent.SUMSCORE != null && ent.COMPLAINSTATUS == "2"
                               && ent.UPDATEDATE > startTime && ent.UPDATEDATE < endTime
                               select ent;
                    if (ents.Count() > 0)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CheckKPIRecordIsComplainingByUserID:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 绩效控件手动打分，并返回结果
        /// </summary>
        /// <param name="kpiPoint"></param>
        /// <param name="formCode"></param>
        /// <param name="flowID"></param>
        /// <param name="lastStepCode"></param>
        /// <param name="AppraiseeID"></param>
        /// <param name="AppraiserID"></param>
        /// <param name="score"></param>
        /// <param name="scoretype"></param>
        /// <returns></returns>
        public T_HR_KPIRECORD SaveKPIRecord(T_HR_KPIPOINT kpiPoint, string formCode, string flowID, string lastStepCode, string AppraiseeID, string AppraiserID, int score, int scoretype)
        {
            try
            {
                bool isAdd = false; //判断是否为添加的标示。
                // 1s 冉龙军
                // 改stepID为stepCode（当流程中有重复的步骤时，此方法不可取）
                //T_HR_KPIRECORD record = GetKPIRecord(formCode, flowID, lastStepCode);
                T_HR_KPIRECORD record = GetKPIRecord(formCode, flowID, kpiPoint.STEPID);
                // 1e
                if (record == null)
                {
                    isAdd = true;
                    record = InitialKPIRecord(formCode, flowID, lastStepCode, kpiPoint);
                }
                // 1s 冉龙军
                else
                {
                    if (record.OWNERCOMPANYID != null && record.OWNERDEPARTMENTID != null && record.OWNERPOSTID != null &&
                        record.OWNERID != null)
                    {
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(AppraiseeID))
                        {
                        }
                        else
                        {
                            // 绩效所有者
                            using (EmployeeBLL kpiDetailEmployeeBLL = new EmployeeBLL())
                            {
                                V_EMPLOYEEPOST kpiDetailRecord = kpiDetailEmployeeBLL.GetEmployeeDetailByID(AppraiseeID);
                                if (kpiDetailRecord != null && kpiDetailRecord.EMPLOYEEPOSTS[0] != null &&
                                    kpiDetailRecord.EMPLOYEEPOSTS[0].T_HR_POST != null
                                    && kpiDetailRecord.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT != null &&
                                    kpiDetailRecord.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY != null)
                                {
                                    record.OWNERCOMPANYID =
                                        kpiDetailRecord.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                                    record.OWNERDEPARTMENTID =
                                        kpiDetailRecord.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                                    record.OWNERPOSTID = kpiDetailRecord.EMPLOYEEPOSTS[0].T_HR_POST.POSTID;
                                    record.OWNERID = AppraiseeID;
                                    record.CREATEUSERID = AppraiseeID;
                                }
                            }
                        }
                    }
                }
                // 1e
                switch (scoretype)
                {
                    //系统评分
                    case 0:
                        record.SYSTEMSCORE = score;
                        record.APPRAISEEID = AppraiseeID;
                        record.UPDATEUSERID = AppraiseeID;
                        break;
                    //手动评分
                    case 1:
                        record.MANUALSCORE = score;
                        record.APPRAISEEID = AppraiseeID;
                        record.APPRAISERID = AppraiserID;
                        record.UPDATEUSERID = AppraiserID;
                        break;
                    //抽查评分
                    case 2:
                        record.RANDOMSCORE = score;
                        record.APPRAISEEID = AppraiseeID;
                        record.RANDOMPERSONID = AppraiserID;
                        record.UPDATEUSERID = AppraiserID;
                        break;
                    // 1s 冉龙军
                    //抽查评分
                    case 3:
                        record.RANDOMPERSONID = AppraiserID;
                        break;
                    // 1e
                }

                //判断打分是否已经全部打完
                // 1s 冉龙军
                //if ((kpiPoint.T_HR_SCORETYPE.ISSYSTEMSCORE.Trim() == "0" || record.SYSTEMSCORE != null)         //不需要机打，或者已经打过
                //    && (kpiPoint.T_HR_SCORETYPE.ISMANUALSCORE.Trim() == "0" || record.MANUALSCORE != null)      //不需要人打，或者已经打过
                //    && (kpiPoint.T_HR_SCORETYPE.ISRANDOMSCORE.Trim() == "0" || record.RANDOMSCORE != null))     //不需要抽查，或者已经打过
                //{
                //    record.SUMSCORE = CulcalateScore(record);
                //}
                #region 全部打完才出总分
                //if (kpiPoint != null && kpiPoint.T_HR_SCORETYPE != null)
                //{
                //    if ((kpiPoint.T_HR_SCORETYPE.ISSYSTEMSCORE.Trim() == "0" || record.SYSTEMSCORE != null) //不需要机打，或者已经打过
                //        && (kpiPoint.T_HR_SCORETYPE.ISMANUALSCORE.Trim() == "0" || record.MANUALSCORE != null)
                //        //不需要人打，或者已经打过
                //        && (kpiPoint.T_HR_SCORETYPE.ISRANDOMSCORE.Trim() == "0" || record.RANDOMSCORE != null))
                //    //不需要抽查，或者已经打过
                //    {
                //        record.SUMSCORE = CulcalateScore(record);
                //    }
                //}
                #endregion
                // 计算总分
                record.SUMSCORE = CulcalateScore(record);

                // 1e
                KPIRecordBll bll = new KPIRecordBll();

                record.UPDATEDATE = System.DateTime.Now; //修改时间
                if (isAdd)
                {
                    bll.KPIRecordAdd(record); //添加KPI明细记录
                }
                else
                {
                    bll.KPIRecordUpdate(record); //修改KPI明细记录
                }

                SumPerformanceBll bllSumPerf = new SumPerformanceBll();
                bllSumPerf.SaveMyRecordByKPIRd(record);

                return record;
            }
            catch (Exception ex)
            {
                Utility.SaveLog("绩效控件执行手动打分出错，出错原因：" + ex.ToString());
                return null;
            }
        }



        /// <summary>
        /// 保存KPI明细记录信息（接口专用）
        /// </summary>
        /// <param name="kpirecord"></param>
        /// <param name="AppraiseeID"></param>
        /// <param name="AppraiserID"></param>
        /// <param name="score"></param>
        /// <param name="scoretype"></param>
        /// <returns></returns>
        public T_HR_KPIRECORD SaveKPIRecordInterface(T_HR_KPIRECORD kpirecord, string AppraiseeID, string AppraiserID, int score, int scoretype)
        {
            try
            {
                bool isAdd = false; //判断是否为添加的标示。

                T_HR_KPIRECORD record = GetKPIRecordInterface(kpirecord.BUSINESSCODE);
                if (record == null)
                {
                    isAdd = true;
                    record = InitialKPIRecordInterface(kpirecord);
                }
                //获取权限字段
                EmployeeBLL bllEmployee = new EmployeeBLL();
                T_HR_EMPLOYEE ep = bllEmployee.GetEmployeeByID(AppraiseeID);
                if (ep != null)
                {
                    record.OWNERID = ep.OWNERID;
                    record.OWNERPOSTID = ep.OWNERPOSTID;
                    record.OWNERDEPARTMENTID = ep.OWNERDEPARTMENTID;
                    record.OWNERCOMPANYID = ep.OWNERCOMPANYID;
                }
                switch (scoretype)
                {
                    //系统评分
                    case 0:
                        record.SYSTEMSCORE = score;
                        record.APPRAISEEID = AppraiseeID;
                        record.UPDATEUSERID = AppraiseeID;
                        break;
                    //手动评分
                    case 1:
                        record.MANUALSCORE = score;
                        record.APPRAISEEID = AppraiseeID;
                        record.APPRAISERID = AppraiserID;
                        record.UPDATEUSERID = AppraiserID;
                        break;
                    //抽查评分
                    case 2:
                        record.RANDOMSCORE = score;
                        record.APPRAISEEID = AppraiseeID;
                        record.RANDOMPERSONID = AppraiserID;
                        record.UPDATEUSERID = AppraiserID;
                        break;
                }


                if (record.SYSTEMSCORE != null //不需要机打，或者已经打过
                    && record.MANUALSCORE != null //不需要人打，或者已经打过
                    && record.RANDOMSCORE != null) //不需要抽查，或者已经打过
                {
                    record.SUMSCORE = CulcalateScore(record);
                }

                using (KPIRecordBll bll = new KPIRecordBll())
                {
                    record.UPDATEDATE = System.DateTime.Now; //修改时间
                    if (isAdd)
                        bll.KPIRecordAddInterface(record); //添加KPI明细记录
                    else
                        bll.KPIRecordUpdate(record); //修改KPI明细记录
                    return record;
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                return null;
            }
        }

        // 1e
        /// <summary>
        /// 当所有打分都完成之后，计算总分
        /// </summary>
        /// <param name="record">KPI明细记录</param>
        /// <returns>KPI明细记录总分</returns>
        private decimal? CulcalateScore(T_HR_KPIRECORD record)
        {
            decimal? sunScore = 0;
            //计算系统评分
            if (record.SYSTEMSCORE != null && record.SYSTEMWEIGHT != null)
                sunScore += record.SYSTEMSCORE * record.SYSTEMWEIGHT / 100;
            //计算手动评分
            if (record.MANUALSCORE != null && record.MANUALWEIGHT != null)
                sunScore += record.MANUALSCORE * record.MANUALWEIGHT / 100;
            //计算抽查评分
            if (record.RANDOMSCORE != null && record.RANDOMWEIGHT != null)
                sunScore += record.RANDOMSCORE * record.RANDOMWEIGHT / 100;
            return sunScore;
        }

        /// <summary>
        /// 新建KPIRecord，并初始化
        /// </summary>
        /// <param name="formCode"></param>
        /// <param name="flowID"></param>
        /// <param name="stepID"></param>
        /// <param name="kpiPoint"></param>
        /// <returns></returns>
        private T_HR_KPIRECORD InitialKPIRecord(string formCode, string flowID, string stepID, T_HR_KPIPOINT kpiPoint)
        {
            T_HR_KPIRECORD record = new T_HR_KPIRECORD();
            record.KPIRECORDID = Guid.NewGuid().ToString();
            record.T_HR_KPIPOINT = kpiPoint;
            record.BUSINESSCODE = formCode;
            record.FLOWRECORDCODE = flowID;
            record.STEPDETAILCODE = stepID;
            record.SYSTEMWEIGHT = kpiPoint.T_HR_SCORETYPE.SYSTEMWEIGHT;
            record.MANUALWEIGHT = kpiPoint.T_HR_SCORETYPE.MANUALWEIGHT;
            record.RANDOMWEIGHT = kpiPoint.T_HR_SCORETYPE.RANDOMWEIGHT;
            record.UPDATEDATE = System.DateTime.Now;
            return record;
        }



        // 1s 冉龙军
        /// <summary>
        /// 新建KPIRecord，并初始化（接口专用）
        /// </summary>
        /// <param name="formCode"></param>
        /// <param name="flowID"></param>
        /// <param name="stepID"></param>
        /// <param name="kpiPoint"></param>
        /// <returns></returns>
        private T_HR_KPIRECORD InitialKPIRecordInterface(T_HR_KPIRECORD kpirecord)
        {
            // 1s 冉龙军
            // 待试
            //T_HR_KPIRECORD record = new T_HR_KPIRECORD();
            //record = kpirecord;
            //record.KPIRECORDID = Guid.NewGuid().ToString();
            //record.UPDATEDATE = System.DateTime.Now;
            //return record;
            kpirecord.KPIRECORDID = Guid.NewGuid().ToString();
            kpirecord.CREATEDATE = System.DateTime.Now;
            return kpirecord;
            // 1e

        }
    }
}
