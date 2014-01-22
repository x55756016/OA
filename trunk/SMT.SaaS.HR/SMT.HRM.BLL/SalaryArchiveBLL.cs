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
using BLLCommonServices = SMT.SaaS.BLLCommonServices;
using SMT.HRM.BLL.Report;
using System.Collections.ObjectModel;

namespace SMT.HRM.BLL
{
    public class SalaryArchiveBLL : BaseBll<T_HR_SALARYARCHIVE>, ILookupEntity, IOperate
    {
        public void SalaryArchiveAdd(T_HR_SALARYARCHIVE obj)
        {
            T_HR_SALARYARCHIVE ent = new T_HR_SALARYARCHIVE();
            Utility.CloneEntity<T_HR_SALARYARCHIVE>(obj, ent);
            if (obj.T_HR_SALARYSTANDARD != null)
            {
                ent.T_HR_SALARYSTANDARDReference.EntityKey =
            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);

            }
            Utility.RefreshEntity(ent);
            dal.Add(ent);

            //try
            //{
            //    BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYARCHIVE>(ent);
            //}
            //catch (Exception ex)
            //{
            //    SMT.Foundation.Log.Tracer.Debug(ex.Message);
            //}
            //DataContext.AddObject("T_HR_SALARYARCHIVE", ent);
            //DataContext.SaveChanges();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public void AddSalaryArchive(T_HR_SALARYARCHIVE obj)
        {
            T_HR_SALARYARCHIVE ent = new T_HR_SALARYARCHIVE();
            Utility.CloneEntity<T_HR_SALARYARCHIVE>(obj, ent);
            if (obj.T_HR_SALARYSTANDARD != null)
            {
                ent.T_HR_SALARYSTANDARDReference.EntityKey =
            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);

            }
            dal.Add(ent);

            //try
            //{
            //    BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYARCHIVE>(ent);
            //}
            //catch (Exception ex)
            //{
            //    SMT.Foundation.Log.Tracer.Debug(ex.Message);
            //}
            //DataContext.AddObject("T_HR_SALARYARCHIVE", ent);
            //DataContext.SaveChanges();
        }
        /// <summary>
        /// 更新薪资档案
        /// </summary>
        /// <param name="entity"></param>
        public void SalaryArchiveUpdate(T_HR_SALARYARCHIVE obj)
        {
            try
            {
                var ent = from a in dal.GetTable()
                          where a.SALARYARCHIVEID == obj.SALARYARCHIVEID
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_SALARYARCHIVE tmpEnt = ent.FirstOrDefault();

                    tmpEnt.CHECKSTATE = obj.CHECKSTATE;
                    tmpEnt.EDITSTATE = obj.EDITSTATE;
                    tmpEnt.REMARK = obj.REMARK;


                    string sql = @"UPDATE T_HR_SALARYARCHIVE t ";
                    sql += " SET t.CHECKSTATE = '" + obj.CHECKSTATE + "',t.EDITSTATE='" + obj.EDITSTATE + "'";
                    sql += ",t.REMARK='" + obj.REMARK + "' WHERE t.SALARYARCHIVEID = '" + obj.SALARYARCHIVEID + "'";
                    EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                    bll.ExecuteSql(sql, "T_HR_SALARYARCHIVE");
                    if (obj.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        sql = @" update t_hr_employeepost t set t.salarylevel='" + tmpEnt.SALARYLEVEL.ToString() + "' where t.employeepostid='" + tmpEnt.EMPLOYEEPOSTID + "' ";
                        bll.ExecuteSql(sql, "T_HR_EMPLOYEEPOST");
                    }

                    //tmpEnt.UPDATEDATE = obj.UPDATEDATE;
                    //Utility.CloneEntity<T_HR_SALARYARCHIVE>(obj, tmpEnt);

                    //            if (obj.T_HR_SALARYSTANDARD != null)
                    //            {
                    //                //tmpEnt.T_HR_SALARYSTANDARDReference.EntityKey =
                    //                //        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                    //                tmpEnt.T_HR_SALARYSTANDARDReference.EntityKey =
                    //new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                    //                tmpEnt.T_HR_SALARYSTANDARD.EntityKey =
                    //new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                    //            }
                    //            else
                    //            {
                    //                //Utility.RefreshEntity(obj);
                    //                tmpEnt.T_HR_SALARYSTANDARDReference.EntityKey = null;
                    //            }
                    //            dal.Update(tmpEnt);
                    //try
                    //{
                    //    BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYARCHIVE>(tmpEnt);
                    //}
                    //catch (Exception ex)
                    //{
                    //    if (ex.InnerException != null)
                    //    {
                    //        SMT.Foundation.Log.Tracer.Debug(ex.InnerException.Message);
                    //    }
                    //    else
                    //    {
                    //        SMT.Foundation.Log.Tracer.Debug(ex.Message);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    SMT.Foundation.Log.Tracer.Debug(ex.InnerException.Message);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug(ex.Message);
                }
                //throw ex;
            }

        }

        /// <summary>
        /// 根据员工ID更新薪资档案
        /// </summary>
        /// <param name="noid"></param>
        /// <param name="employeeid">员工ID</param>
        /// <param name="year">终止年份</param>
        /// <param name="month">终止月份</param>
        public bool SalaryArchiveUpdateByEmployee(string noid, string employeeid, int year, int month)
        {
            try
            {
                var ent = from a in dal.GetTable()
                          where a.SALARYARCHIVEID != noid && a.CHECKSTATE == "2" && a.EMPLOYEEID == employeeid
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_SALARYARCHIVE tmpEnt = ent.FirstOrDefault();

                    if (tmpEnt.T_HR_SALARYSTANDARD != null)
                    {
                        tmpEnt.T_HR_SALARYSTANDARDReference.EntityKey =
        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", tmpEnt.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                        tmpEnt.T_HR_SALARYSTANDARD.EntityKey =
        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", tmpEnt.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                    }
                    else
                    {
                        tmpEnt.T_HR_SALARYSTANDARDReference.EntityKey = null;
                    }
                    tmpEnt.OTHERSUBJOIN = year;
                    tmpEnt.OTHERADDDEDUCT = month;
                    dal.Update(tmpEnt);
                    return true;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
            return false;
        }

        /// <summary>
        /// 薪资档案删除
        /// </summary>
        /// <param name="IDs">薪资档案ID</param>
        /// <returns></returns>
        public int SalaryArchiveDelete(string[] IDs)
        {
            int IntResult = 0;
            dal.BeginTransaction();
            try
            {
                CustomGuerdonArchiveBLL custombll = new CustomGuerdonArchiveBLL();
                foreach (string id in IDs)
                {
                    var ents = from e in dal.GetObjects<T_HR_SALARYARCHIVE>()
                               where e.SALARYARCHIVEID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        int IntCustom = custombll.CustomGuerdonArchiveDelByArchiveID(id);
                        if (IntCustom == 0)
                        {
                            dal.RollbackTransaction();
                            return IntResult;
                        }
                        //删除薪资档案的薪资项

                        int IntArchive = ArchiveDeleteByArchiveID(ent.SALARYARCHIVEID);
                        if (IntArchive == 0)
                        {
                            dal.RollbackTransaction();
                            return IntResult;
                        }
                        dal.DeleteFromContext(ent);

                    }

                    //TODO:删除项目所包含的明细
                }

                IntResult = dal.SaveContextChanges();
                if (IntResult > 0)
                {
                    dal.CommitTransaction();
                }
                else
                {
                    dal.RollbackTransaction();
                }
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug("SalaryArichiveBll-SalaryArchiveDelete  " + DateTime.Now.ToString() + ex.Message);
            }
            return IntResult;
        }

        /// <summary>
        /// 薪资档案删除(单独)
        /// </summary>
        /// <param name="id">薪资档案ID</param>
        /// <returns></returns>
        public int SalaryArchiveSingleDelete(string id)
        {
            CustomGuerdonArchiveBLL custombll = new CustomGuerdonArchiveBLL();
            var ents = from e in dal.GetObjects<T_HR_SALARYARCHIVE>()
                       where e.SALARYARCHIVEID == id
                       select e;
            var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

            if (ent != null)
            {
                custombll.CustomGuerdonArchiveDelByArchiveID(id);
                dal.DeleteFromContext(ent);
                //try
                //{
                //    BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYARCHIVE>(ent);
                //}
                //catch { }
                //dal.Delete(ent);
            }
            //TODO:删除项目所包含的明细

            return dal.SaveContextChanges();
        }


        /// <summary>
        /// 薪资档案删除(只删除薪资方案)liujx
        /// </summary>
        /// <param name="id">薪资档案ID</param>
        /// <returns></returns>
        public int SalaryArchiveSingleDeleteById(string id)
        {
            int IntResult = 0;
            try
            {
                CustomGuerdonArchiveBLL custombll = new CustomGuerdonArchiveBLL();
                var ents = from e in dal.GetObjects<T_HR_SALARYARCHIVE>()
                           where e.SALARYARCHIVEID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);

                }
                //TODO:删除项目所包含的明细

                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("SalaryArchiveBLL--SalaryArchiveSingleDeleteById:" + System.DateTime.Now.ToString() + ex.Message);
                return IntResult;
            }
        }

        /// <summary>
        /// 薪资档案删除(当月删除)
        /// </summary>
        /// <param name="noID"></param>
        /// <param name="employeeid">员工ID</param>
        /// <returns></returns>
        public int SalaryArchiveDelete(string noID, string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYARCHIVE>()
                       where a.EMPLOYEEID == employeeid && a.SALARYARCHIVEID != noID && a.CHECKSTATE == "2"
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    //删除薪资档案的薪资项
                    ArchiveDeleteByArchiveID(ent.SALARYARCHIVEID);
                    // 删除薪资档案
                    SalaryArchiveSingleDelete(ent.SALARYARCHIVEID);
                }
            }
            return ents.Count();
        }
        /// <summary>
        /// 薪资档案删除(过期删除)
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        public int SalaryArchiveDelete(string employeeid)
        {
            decimal? year = Convert.ToDecimal(System.DateTime.Now.Year);
            decimal? month = Convert.ToDecimal(System.DateTime.Now.Month);
            var ents = from a in dal.GetObjects<T_HR_SALARYARCHIVE>()
                       where a.EMPLOYEEID == employeeid && a.CHECKSTATE == "2" && a.OTHERSUBJOIN != null && a.OTHERADDDEDUCT != null
                       select a;
            ents = ents.Where(m => m.OTHERSUBJOIN <= year && m.OTHERADDDEDUCT < month);
            //ents = ents.Where(m => m.OVERYEAR <= year && m.OVERMONTH < month);
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    //删除薪资档案的薪资项
                    ArchiveDeleteByArchiveID(ent.SALARYARCHIVEID);
                    // 删除薪资档案
                    SalaryArchiveSingleDelete(ent.SALARYARCHIVEID);
                }
            }
            return ents.Count();
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_SALARYARCHIVE> ents = from a in DataContext.T_HR_SALARYARCHIVE
        //                                          select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYARCHIVE");

            IQueryable<T_HR_SALARYARCHIVE> ents = dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD");



            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYARCHIVE>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        public T_HR_SALARYARCHIVE GetSalaryArchiveByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD")
                       where o.SALARYARCHIVEID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 获取薪资档案主表视图
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public V_SALARYARCHIVEMASTER GetSalaryArchiveMasterByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD")
                       join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEPOSTID equals b.EMPLOYEEPOSTID into tmp
                       from b in tmp.DefaultIfEmpty()
                       where o.SALARYARCHIVEID == ID
                       select new V_SALARYARCHIVEMASTER
                       {
                           archive = o,
                           CompanyName = b == null ? "" : b.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                           CompanyID = b == null ? "" : b.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                           DepartmentName = b == null ? "" : b.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                           DepartmentID = b == null ? "" : b.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                           PostName = b == null ? "" : b.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                           PostID = b == null ? "" : b.T_HR_POST.POSTID,
                           standerID = o.T_HR_SALARYSTANDARD.SALARYSTANDARDID,
                           standerName = o.T_HR_SALARYSTANDARD.SALARYSTANDARDNAME

                       };


            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public new IQueryable<T_HR_SALARYARCHIVE> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            IQueryable<T_HR_SALARYARCHIVE> ents = dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYARCHIVE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }


        #region 薪资档案带有权限的分页查询
        /// <summary>
        /// 带有权限的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页 </param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数 </param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <param name="queryCode">查询码 0-可用 1-所有</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_SALARYARCHIVE> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount,
                                                              string userID, string CheckState, int orgtype, string orgid, int queryCode, string companyID)
        {
            List<object> queryParas = new List<object>();

            queryParas.AddRange(paras);
            //orgtype:0 公司 1部门 2岗位
            IQueryable<T_HR_SALARYARCHIVE> ents = GetArchiveFilter(orgtype, orgid);

            //var en = ents.GroupBy(y => y.SALARYARCHIVEID).Select(g => new { group = g.Key, groupcontent = g });
            //foreach (var v in en)
            //{
            //    ent.Add(v.groupcontent.FirstOrDefault());
            //}
            //ents = ent.AsQueryable();

            //如果是查询可用的薪资档案 则审核状态一定是通过的
            if (queryCode == 0) CheckState = ((int)CheckStates.Approved).ToString();

            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYARCHIVE");

                if (CheckState != Convert.ToInt32(CheckStates.All).ToString())
                {
                    if (queryParas.Count() > 0)
                    {
                        filterString += " AND ";
                    }

                    filterString += "CHECKSTATE==@" + queryParas.Count().ToString();
                    queryParas.Add(CheckState);
                }
            }
            else
            {
                SetFilterWithflow("SALARYARCHIVEID", "T_HR_SALARYARCHIVE", userID, ref CheckState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }

            if (ents.Count() > 0)
            {
                ents = QueryByUsableCode(ents, queryCode, companyID);
            }
            if (ents.Count() > 0)
            {
                ents = ents.OrderBy(sort);

                ents = Utility.Pager<T_HR_SALARYARCHIVE>(ents, pageIndex, pageSize, ref pageCount);
            }

            return ents;
        }

        /// <summary>
        /// 薪资档案过滤
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_HR_SALARYARCHIVE> GetArchiveFilter(int orgtype, string orgid)
        {
            IQueryable<T_HR_SALARYARCHIVE> ents = dal.GetObjects<T_HR_SALARYARCHIVE>();
            switch (orgtype)
            {
                case 0:
                    ents = (from a in dal.GetObjects<T_HR_SALARYARCHIVE>()
                            join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                            join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                            join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                            where d.T_HR_COMPANY.COMPANYID == orgid && b.ISAGENCY == "0" //&& b.EDITSTATE == "1"
                            select a).Distinct();
                    break;
                case 1:
                    ents = (from a in dal.GetObjects<T_HR_SALARYARCHIVE>()
                            join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                            join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                            join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                            where d.DEPARTMENTID == orgid && b.ISAGENCY == "0" // && b.EDITSTATE == "1" 
                            select a).Distinct();
                    break;
                case 2:
                    ents = (from a in dal.GetObjects<T_HR_SALARYARCHIVE>()
                            join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                            join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                            where c.POSTID == orgid && b.ISAGENCY == "0" //&& b.EDITSTATE == "1"
                            select a).Distinct();
                    break;
            }
            return ents;
        }

        /// <summary>
        /// 根据查询码查找可用的或所有薪资档案
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queryCode"></param>
        /// <returns></returns>
        private IQueryable<T_HR_SALARYARCHIVE> QueryByUsableCode(IQueryable<T_HR_SALARYARCHIVE> entity, int queryCode, string companyID)
        {
            try
            {
                IQueryable<T_HR_SALARYARCHIVE> result = entity; //返回的结果
                if (entity != null)
                {
                    List<string> UsersList = new List<string>(); //用户名单

                    switch (queryCode)
                    {
                        case 0:
                            UsersList = GetUsers(entity);
                            if (UsersList != null)
                            {
                                result = QueryByUserID(entity, UsersList, companyID);
                            }
                            break;
                        default:
                            result = entity;
                            break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Foundation.Log.Tracer.Debug("SalaryArchiveBll-QueryByUserID" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 通过用户ID获取可用的实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="usersID"></param>
        /// <returns></returns>
        private IQueryable<T_HR_SALARYARCHIVE> QueryByUserID(IQueryable<T_HR_SALARYARCHIVE> entity, List<string> usersID, string companyID)
        {
            try
            {
                IQueryable<T_HR_SALARYARCHIVE> result;//结果
                List<string> saID = new List<string>();
                //创建日期小于现在的档案是可用的，按日期排序取第一个为正使用的
                DateTime dateNow = System.DateTime.Now;
                int Year = dateNow.Year;
                int Month = dateNow.Month;
                if (usersID.Count() > 0)
                {
                    //获取相应的formid
                    foreach (var userID in usersID)
                    {
                        if (!string.IsNullOrWhiteSpace(userID))
                        {
                            //var TheEnt = (from e in entity
                            //              join em in dal.GetObjects<T_HR_EMPLOYEE>() on e.EMPLOYEEID equals em.EMPLOYEEID
                            //              where e.OWNERID == userID && e.CREATEDATE < dateNow && e.OWNERCOMPANYID==em.OWNERCOMPANYID
                            //              orderby e.CREATEDATE descending                                        
                            //              select e).FirstOrDefault();
                            var TheEnt = this.GetSalaryArchiveApprovedByEmployeeID(userID, Year, Month);//,companyID);
                            if (TheEnt != null) saID.Add(TheEnt.SALARYARCHIVEID);
                        }
                    }

                    //通过相应formid查找可用的实体

                    if (saID.Count() > 0)
                    {
                        var rsl = from e in entity
                                  where saID.Contains(e.SALARYARCHIVEID)
                                  orderby e.SALARYARCHIVEID
                                  select e;
                        result = rsl;
                    }
                    else
                    {
                        result = null;
                    }
                    return result;
                }
                else
                {
                    Foundation.Log.Tracer.Debug("SalaryArchiveBll-QueryByUserID：缺少用户IDs");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Foundation.Log.Tracer.Debug("SalaryArchiveBll-QueryByUserID" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Add:    luojie
        /// Date:   2012-11-03
        /// Forwhat:获取所有拥有薪资档案的人的名单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private List<string> GetUsers(IQueryable<T_HR_SALARYARCHIVE> entity)
        {
            if (entity != null)
            {
                List<string> UsersList = new List<string>();
                try
                {
                    UsersList = (from e in entity
                                 group e by e.OWNERID into g
                                 select g.Key).ToList();
                    if (UsersList.Count() > 0)
                    {
                        return UsersList;
                    }
                }
                catch (Exception ex)
                {
                    Foundation.Log.Tracer.Debug("SalaryArchiveBll-GetUsers:" + ex.ToString());
                    return null;
                }
            }
            return null;
        }
        #endregion

        /// <summary>
        /// 根据员工id获取薪资档案
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>薪资档案实体</returns>
        public T_HR_SALARYARCHIVE GetSalaryArchiveByEmployeeID(string employeeID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD")
                       where o.EMPLOYEEID == employeeID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        public List<T_HR_SALARYARCHIVE> GetSalaryArchiveByEmployeeIDList(List<string> employeeIDList)
        {
            List<T_HR_SALARYARCHIVE> salaryList = new List<T_HR_SALARYARCHIVE>();
            foreach (var employeeID in employeeIDList)
            {
                var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>()
                           where o.EMPLOYEEID == employeeID
                           select o;
            }
            return salaryList;
        }


        /// <summary>
        /// 根据员工ID获取审核通过的薪资档案(非离职)
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        public T_HR_SALARYARCHIVE GetSalaryArchiveApprovedByEmployeeID(string employeeID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD")
                       //join p in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on o.EMPLOYEEID equals p.EMPLOYEEID
                       join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.EMPLOYEEID equals p.EMPLOYEEID
                       where o.EMPLOYEEID == employeeID && o.CHECKSTATE == "2"  //&& p.EMPLOYEESTATE != "2"  //(是否离职)
                       orderby o.CREATEDATE descending
                       select o;
            if (ents.Count() > 0)
            {
                if (ents.Count() < 2)
                {
                    return ents.FirstOrDefault();
                }
                else
                {
                    T_HR_SALARYARCHIVE temp = ents.ToList()[0];

                    if (temp.EDITSTATE == "1")
                    {
                        return temp;
                    }
                    else
                    {
                        int year = System.DateTime.Now.Year;
                        int month = System.DateTime.Now.Month;
                        ents = ents.Where(n => n.OTHERSUBJOIN != null && n.OTHERADDDEDUCT != null);
                        ents = ents.Where(m => m.OTHERSUBJOIN == year && m.OTHERADDDEDUCT == month);
                        return ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///  根据员工ID, 薪资档案的所属公司ID，薪资结算年份及月份，获取其最新可用的薪资档案
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public T_HR_SALARYARCHIVE GetSalaryArchiveApprovedByEmployeeID(string employeeID, int year, int month, string companyid)
        {
            //var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD")
            //           //join p in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on o.EMPLOYEEID equals p.EMPLOYEEID
            //           join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.EMPLOYEEID equals p.EMPLOYEEID
            //           join d in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEPOSTID equals d.EMPLOYEEPOSTID
            //           join e in dal.GetObjects<T_HR_POST>() on d.T_HR_POST.POSTID equals e.POSTID
            //           join f in dal.GetObjects<T_HR_DEPARTMENT>() on e.T_HR_DEPARTMENT.DEPARTMENTID equals f.DEPARTMENTID
            //           join g in dal.GetObjects<T_HR_COMPANY>() on f.T_HR_COMPANY.COMPANYID equals g.COMPANYID
            //           where o.EMPLOYEEID == employeeID && o.CHECKSTATE == "2" && g.COMPANYID == companyid //&& p.EMPLOYEESTATE != "2"  //(是否离职)
            //           orderby o.CREATEDATE descending
            //           select o;
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD")
                       //join p in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on o.EMPLOYEEID equals p.EMPLOYEEID
                       join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.EMPLOYEEID equals p.EMPLOYEEID
                       where o.EMPLOYEEID == employeeID && o.CHECKSTATE == "2"  //&& p.EMPLOYEESTATE != "2"  //(是否离职)                       
                       select o;
            if (ents.Count() > 0)
            {

                ents = ents.OrderByDescending(s => s.CREATEDATE).ThenByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT);
                foreach (T_HR_SALARYARCHIVE item in ents)
                {
                    //if (item.OTHERSUBJOIN == year && item.OTHERADDDEDUCT <= month && item.OWNERCOMPANYID == companyid)
                    //{
                    //    return item;
                    //}
                    //if (item.OTHERSUBJOIN < year && item.OWNERCOMPANYID == companyid)
                    //{
                    //    return item;
                    //}
                    //去掉公司限制，暂时解决跨机构的情况
                    if (item.OTHERSUBJOIN == year && item.OTHERADDDEDUCT <= month)
                    {
                        return item;
                    }
                    if (item.OTHERSUBJOIN < year)
                    {
                        return item;
                    }
                }

            }
            return null;
        }

        /// <summary>
        /// 根据员工ID年月获取员工薪资信息(年月暂不用)
        /// </summary>
        /// <param name="employeeIDs">员工IDs</param>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>员工薪资</returns>
        public List<T_HR_SALARYARCHIVE> GetSalaryArchiveByEmployeeIDs(List<string> employeeIDs, int year, int month)
        {
            List<T_HR_SALARYARCHIVE> salaryList = new List<T_HR_SALARYARCHIVE>();
            employeeIDs.ForEach(it =>
            {
                var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>()
                           join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.EMPLOYEEID equals p.EMPLOYEEID
                           where o.EMPLOYEEID == it && o.CHECKSTATE == "2"
                           select o;
                if (ents.Count() > 0)
                {
                    ents = ents.OrderByDescending(s => s.CREATEDATE).ThenByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT);
                    salaryList.Add(ents.FirstOrDefault());//默认取第一条
                }
                else //居然有没有审核通过的薪资档案，没办法只能这样取
                {
                    var en = from o in dal.GetObjects<T_HR_SALARYARCHIVE>()
                             join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.EMPLOYEEID equals p.EMPLOYEEID
                             where o.EMPLOYEEID == it
                             select o;
                    if (en.Count() > 0)
                    {
                        en = en.OrderByDescending(s => s.CREATEDATE).ThenByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT);
                        salaryList.Add(en.FirstOrDefault());//默认取第一条
                    }
                }
            });
            return salaryList;
        }

        /// <summary>
        /// 根据员工ID，薪资结算年份及月份，获取其最新可用的薪资档案
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public T_HR_SALARYARCHIVE GetSalaryArchiveApprovedByEmployeeID(string employeeID, int year, int month)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD")
                       //join p in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on o.EMPLOYEEID equals p.EMPLOYEEID
                       join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.EMPLOYEEID equals p.EMPLOYEEID
                       where o.EMPLOYEEID == employeeID && o.CHECKSTATE == "2"  //&& p.EMPLOYEESTATE != "2"  //(是否离职)                       
                       select o;
            if (ents.Count() > 0)
            {

                ents = ents.OrderByDescending(s => s.CREATEDATE).ThenByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT);
                foreach (T_HR_SALARYARCHIVE item in ents)
                {
                    if (item.OTHERSUBJOIN == year && item.OTHERADDDEDUCT <= month)
                    {
                        return item;
                    }
                    if (item.OTHERSUBJOIN < year)
                    {
                        return item;
                    }
                }

            }
            return null;
        }

        public T_HR_SALARYARCHIVE GetSalaryArchiveByEmployeeIDAndStandID(string employeeID, string standID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVE>().Include("T_HR_SALARYSTANDARD")
                       where o.EMPLOYEEID == employeeID && o.T_HR_SALARYSTANDARD.SALARYSTANDARDID == standID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }



        # region 生成薪资档案
        public int CreateSalaryArchive(int objectType, string objectID, T_HR_SALARYARCHIVE archive, bool createType)
        {
            int rslt = 0;
            EmployeeBLL bllEmployee = new EmployeeBLL();
            List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();
            DateTime dtCheck = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
            if (objectType <= 3)
            {
                switch (objectType)
                {
                    case 1:
                        entEmployees = bllEmployee.GetEmployeeByCompanyID(objectID, dtCheck).ToList();
                        //   CreateCompanyArchive(objectID, archive);
                        break;
                    case 2:
                        entEmployees = bllEmployee.GetEmployeeByDepartmentID(objectID).ToList();
                        //   CreateDepartArchive(objectID, archive);
                        break;
                    case 3:
                        entEmployees = bllEmployee.GetEmployeeByPostID(objectID).ToList();
                        //   CreatePostArchive(objectID, archive);
                        break;
                    //case 3:
                    //    CreateEmployeeSalaryArchiveByemployeeID(objectID, archive);
                    //    break;
                }
                foreach (T_HR_EMPLOYEE temp in entEmployees)
                {
                    CreateEmployeeSalaryArchive(temp, archive, 0, createType);
                    //dal.GetObjects<SaveChanges();
                }

            }
            else if (objectType == 4)
            {
                CreateEmployeeSalaryArchiveByemployeeID(objectID, archive, createType);
            }
            rslt = 1;
            return rslt;
        }
        #region
        public void CreateCompanyArchive(string companyID, T_HR_SALARYARCHIVE archive, bool createType)
        {
            DepartmentBLL bll = new DepartmentBLL();
            List<T_HR_DEPARTMENT> emplist = bll.GetDepartmentByCompanyId(companyID);
            foreach (var emp in emplist)
            {
                CreateDepartArchive(emp.DEPARTMENTID, archive, createType);
            }
        }
        public void CreateDepartArchive(string departID, T_HR_SALARYARCHIVE archive, bool createType)
        {
            PostBLL bll = new PostBLL();
            List<T_HR_POST> emplist = bll.GetPostByDepartId(departID);
            foreach (var emp in emplist)
            {
                CreatePostArchive(emp.POSTID, archive, createType);
            }
        }
        public void CreatePostArchive(string postID, T_HR_SALARYARCHIVE archive, bool createType)
        {
            EmployeePostBLL bll = new EmployeePostBLL();

            List<T_HR_EMPLOYEEPOST> emplist = bll.GetEmployeePostByPostID(postID);
            foreach (var emp in emplist)
            {

                CreateEmployeeSalaryArchive(emp.T_HR_EMPLOYEE, archive, 0, createType);
            }
        }
        #endregion
        #region  根据员工实体生成薪资档案(现供员工入职调用)
        /// <summary>
        /// 根据员工实体生成薪资档案
        /// </summary>
        /// <param name="employee"></param>
        public void CreateEmployeeArchiveByEmployee(T_HR_EMPLOYEE employee, bool createType)
        {
            EmployeePostBLL bll = new EmployeePostBLL();
            T_HR_EMPLOYEEPOST post = bll.GetEmployeePostActivedByEmployeeID(employee.EMPLOYEEID);
            if (post != null)
            {
                SalaryStandardBLL standbll = new SalaryStandardBLL();
                //获取员工的薪资方案
                string solutionID = standbll.GetSolutionIDByIDType(2, post.T_HR_POST.POSTID);
                T_HR_SALARYARCHIVE archive = new T_HR_SALARYARCHIVE();
                archive.SALARYARCHIVEID = Guid.NewGuid().ToString();
                archive.SALARYSOLUTIONID = solutionID;
                archive.CREATEUSERID = employee.CREATEUSERID;
                archive.OWNERCOMPANYID = employee.OWNERCOMPANYID;
                archive.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                archive.OWNERID = employee.EMPLOYEEID;
                archive.OWNERPOSTID = employee.OWNERPOSTID;
                archive.CREATEDATE = employee.CREATEDATE;
                archive.CHECKSTATE = Convert.ToInt16(CheckStates.UnSubmit).ToString();
                CreateEmployeeSalaryArchive(employee, archive, 1, createType);
            }

        }
        #endregion
        /// <summary>
        /// 生成员工薪资档案
        /// </summary>
        /// <param name="objectID">员工ID</param>
        /// <param name="archive">薪资档案</param>
        public void CreateEmployeeSalaryArchiveByemployeeID(string objectID, T_HR_SALARYARCHIVE archive, bool createType)
        {
            EmployeePostBLL bll = new EmployeePostBLL();
            T_HR_EMPLOYEEPOST post = bll.GetEmployeePostByEmployeeID(objectID);
            CreateEmployeeSalaryArchive(post.T_HR_EMPLOYEE, archive, 1, createType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="archive"></param>
        /// <param name="generateType">是否单独生成某个员工的档案</param>
        /// <param name="createType">档案创建类型:异动和非异动(false异动)</param>
        public void CreateEmployeeSalaryArchive(T_HR_EMPLOYEE employee, T_HR_SALARYARCHIVE archive, int generateType, bool createType)
        {

            string employeeID = "";
            dal.BeginTransaction();
            try
            {

                employeeID = employee.EMPLOYEEID;
                //根据员工ID 和方案ID获取员工的薪资标准
                // T_HR_SALARYSTANDARD stand = GetStandardByEmployeeIDAndSolutionID(employeeID, archive.SALARYSOLUTIONID);
                string employeeSalaryLevel = archive.SALARYLEVEL.ToString();
                var stand = (from st in dal.GetObjects<T_HR_SALARYSTANDARD>()
                             join sa in dal.GetObjects<T_HR_SALARYLEVEL>() on st.T_HR_SALARYLEVEL.SALARYLEVELID equals sa.SALARYLEVELID
                             where st.SALARYSOLUTIONID == archive.SALARYSOLUTIONID && sa.T_HR_POSTLEVELDISTINCTION.POSTLEVEL == archive.POSTLEVEL && sa.SALARYLEVEL == employeeSalaryLevel
                             select st).FirstOrDefault();

                //根据员工的ID 和薪资标准ID查找 薪资档案  如果存在审核通过的就不在创建档案

                if (stand != null)
                {
                    //   T_HR_SALARYARCHIVE salaryArchive = GetSalaryArchiveByEmployeeIDAndStandID(employeeID, stand.SALARYSTANDARDID);
                    // T_HR_SALARYARCHIVE salaryArchive = GetSalaryArchiveByEmployeeID(employeeID);
                    var salaryArchive = (from c in dal.GetObjects<T_HR_SALARYARCHIVE>()
                                         where c.SALARYARCHIVEID == archive.SALARYARCHIVEID
                                         select c).FirstOrDefault();

                    #region 修改薪资档案
                    if (salaryArchive != null)
                    {
                        //审核通过 且不是单独生成个人薪资档案
                        if (salaryArchive.CHECKSTATE == Convert.ToInt16(CheckStates.Approved).ToString() && generateType == 0)
                        {
                            return;
                        }

                        #region  审核通过时FORM已经添加了档案的历史
                        //添加薪资档案历史
                        //SalaryArchiveHisAdd(salaryArchive);

                        //添加薪资档案历史项
                        //AddSalaryArchiveHisItem(salaryArchive);
                        #endregion

                        //添加自定义薪资档案历史
                        //  SalaryCustomArchiveHisAdd(salaryArchive.SALARYARCHIVEID);
                        //删除自定义薪资档案
                        int DelCustom = CustomGuerdonArchiveDelByArchiveID(salaryArchive);
                        if (DelCustom == 0)//没删除成功则回滚
                        {
                            dal.RollbackTransaction();
                            return;
                        }
                        //删除薪资档案的薪资项
                        int IntDelArchive = ArchiveDeleteByArchiveID(salaryArchive.SALARYARCHIVEID);
                        if (IntDelArchive == 0)
                        {
                            dal.RollbackTransaction();
                            return;
                        }
                        // 删除薪资档案
                        int IntDelSingle = SalaryArchiveSingleDeleteById(salaryArchive.SALARYARCHIVEID);
                        if (IntDelSingle == 0)
                        {
                            dal.RollbackTransaction();
                            return;
                        }
                        //dal.DeleteFromContext(salaryArchive);
                        //dal.SaveContextChanges();
                    }
                    # endregion
                    //重新生成薪资档案
                    salaryArchive = null;
                    salaryArchive = new T_HR_SALARYARCHIVE();
                    if (string.IsNullOrEmpty(archive.SALARYARCHIVEID))
                        salaryArchive.SALARYARCHIVEID = Guid.NewGuid().ToString();
                    else
                        salaryArchive.SALARYARCHIVEID = archive.SALARYARCHIVEID;
                    //员工信息
                    salaryArchive.EMPLOYEEID = employeeID;
                    salaryArchive.EMPLOYEENAME = employee.EMPLOYEECNAME;
                    salaryArchive.EMPLOYEECODE = employee.EMPLOYEECODE;


                    salaryArchive.T_HR_SALARYSTANDARD = new T_HR_SALARYSTANDARD();
                    salaryArchive.T_HR_SALARYSTANDARD.SALARYSTANDARDID = stand.SALARYSTANDARDID;

                    salaryArchive.CREATEDATE = System.DateTime.Now;
                    salaryArchive.SALARYSOLUTIONID = archive.SALARYSOLUTIONID;
                    salaryArchive.SALARYSOLUTIONNAME = archive.SALARYSOLUTIONNAME;
                    salaryArchive.REMARK = archive.REMARK;
                    salaryArchive.POSTLEVEL = archive.POSTLEVEL;
                    salaryArchive.SALARYLEVEL = archive.SALARYLEVEL;
                    //换公司后 就不读取以前的薪资档案了
                    salaryArchive.OLDPOSTLEVEL = archive.OLDPOSTLEVEL;
                    salaryArchive.OLDSALARYLEVEL = archive.OLDSALARYLEVEL;

                    salaryArchive.SKILLPOSTLEVEL = archive.SKILLPOSTLEVEL;
                    salaryArchive.SKILLSALARYLEVEL = archive.SKILLSALARYLEVEL;

                    salaryArchive.BALANCE = archive.BALANCE;
                    salaryArchive.EMPLOYEEPOSTID = archive.EMPLOYEEPOSTID;
                    // 关于权限的字段
                    salaryArchive.CREATEUSERID = archive.CREATEUSERID;
                    salaryArchive.OWNERPOSTID = archive.OWNERPOSTID;
                    salaryArchive.OWNERID = employeeID;
                    salaryArchive.OWNERDEPARTMENTID = archive.OWNERDEPARTMENTID;
                    salaryArchive.OWNERCOMPANYID = archive.OWNERCOMPANYID;
                    salaryArchive.CREATEPOSTID = archive.CREATEPOSTID;

                    //薪级异动相关信息
                    salaryArchive.EDITSTATE = archive.EDITSTATE;
                    salaryArchive.OTHERSUBJOIN = archive.OTHERSUBJOIN;
                    salaryArchive.OTHERADDDEDUCT = archive.OTHERADDDEDUCT;

                    //审核
                    salaryArchive.CHECKSTATE = archive.CHECKSTATE;

                    //发薪机构
                    salaryArchive.PAYCOMPANY = archive.PAYCOMPANY;

                    //考勤机构ID
                    if (!string.IsNullOrWhiteSpace(archive.ATTENDANCEORGID))
                    {
                        salaryArchive.ATTENDANCEORGID = archive.ATTENDANCEORGID;
                    }

                    //考勤机构名称
                    if (!string.IsNullOrWhiteSpace(archive.ATTENDANCEORGNAME))
                    {
                        salaryArchive.ATTENDANCEORGNAME = archive.ATTENDANCEORGNAME;
                    }

                    //结算岗位ID
                    if (!string.IsNullOrWhiteSpace(archive.BALANCEPOSTID))
                    {
                        salaryArchive.BALANCEPOSTID = archive.BALANCEPOSTID;
                    }

                    //结算岗位名称
                    if (!string.IsNullOrWhiteSpace(archive.BALANCEPOSTNAME))
                    {
                        salaryArchive.BALANCEPOSTNAME = archive.BALANCEPOSTNAME;
                    }

                    //添加薪资档案
                    SalaryArchiveAdd(salaryArchive);
                    //添加薪资档案项目
                    AddArchiveItems(salaryArchive);
                    //添加自定义薪资档案
                    CustomArchiveAdd(salaryArchive);
                }
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                //dal.CommitTransaction();有错误是回滚而不是提交
                dal.RollbackTransaction();
                ex.Message.ToString();
                //throw ex;
            }
        }
        public decimal GetFixSalary(decimal postlevel, string salarylevel, string solutionID)
        {
            try
            {
                decimal fixSalary = 0;
                List<T_HR_SALARYSTANDARDITEM> standerItems = new List<T_HR_SALARYSTANDARDITEM>();//薪资标准的薪资项
                //根据岗位级别和薪资级别获取薪资标准
                T_HR_SALARYSTANDARD stand = new T_HR_SALARYSTANDARD();
                var employeeStand = from st in dal.GetObjects<T_HR_SALARYSTANDARD>()
                                    join sa in dal.GetObjects<T_HR_SALARYLEVEL>() on st.T_HR_SALARYLEVEL.SALARYLEVELID equals sa.SALARYLEVELID
                                    where st.SALARYSOLUTIONID == solutionID && sa.T_HR_POSTLEVELDISTINCTION.POSTLEVEL == postlevel && sa.SALARYLEVEL == salarylevel
                                    select st;
                if (employeeStand.Count() > 0)
                {
                    stand = employeeStand.FirstOrDefault();
                }
                else
                {
                    stand = null;
                }

                if (stand != null)
                {
                    standerItems = GetstandardItemByStandardID(stand.SALARYSTANDARDID);
                    foreach (T_HR_SALARYSTANDARDITEM item in standerItems)
                    {
                        //if (item.T_HR_SALARYITEM.SALARYITEMID == "7692ec13-0785-432d-abd8-aa26b993216c")
                        if (item.T_HR_SALARYITEM.SALARYITEMNAME == "固定收入合计")
                        {
                            fixSalary = Convert.ToDecimal(item.SUM);
                            break;
                        }
                    }
                }
                return fixSalary;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                //throw ex;
                return 0;
            }
        }

        public List<string> GetOldFixSalary(string employeeid)
        {
            List<string> ents = new List<string>();
            try
            {
                decimal fixSalary = 0;
                List<T_HR_SALARYSTANDARDITEM> standerItems = new List<T_HR_SALARYSTANDARDITEM>();//薪资标准的薪资项
                #region
                //var archiveHis = (from c in dal.GetObjects<T_HR_SALARYARCHIVEHIS>()
                //                  where c.EMPLOYEEID == employeeid
                //                  orderby c.CREATEDATE descending
                //                  select c).ToList();
                //if (archiveHis != null)
                //{
                //    if (archiveHis.Count > 0)
                //    {
                //        var tmp = archiveHis[0];
                //        fixSalary = GetFixSalary(Convert.ToDecimal(tmp.POSTLEVEL), tmp.SALARYLEVEL.ToString(), tmp.SALARYSOLUTIONID);
                //        ents.Add(fixSalary.ToString());
                //        ents.Add(tmp.SALARYLEVEL.ToString());
                //        ents.Add(tmp.POSTLEVEL.ToString());
                //    }

                //}
                #endregion
                var archives = from o in dal.GetObjects<T_HR_SALARYARCHIVE>()
                               where o.EMPLOYEEID == employeeid && o.CHECKSTATE == "2"
                               select o;
                var tmp = archives.OrderByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT).ThenByDescending(q => q.UPDATEDATE).ToList().FirstOrDefault();
                if (tmp != null)
                {
                    fixSalary = GetFixSalary(Convert.ToDecimal(tmp.POSTLEVEL), tmp.SALARYLEVEL.ToString(), tmp.SALARYSOLUTIONID);
                    ents.Add(fixSalary.ToString());
                    ents.Add(tmp.SALARYLEVEL.ToString());
                    ents.Add(tmp.POSTLEVEL.ToString());
                    ents.Add(tmp.OWNERCOMPANYID);
                }
                return ents;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                //throw ex;
                return ents;
            }
        }
        /// <summary>
        /// 添加自定义薪资档案历史
        /// </summary>
        /// <param name="archiveID"></param>
        void SalaryCustomArchiveHisAdd(string archiveID)
        {
            CustomGuerdonArchiveBLL archiveBLL = new CustomGuerdonArchiveBLL();
            CustomGuerdonArchiveHISBLL hisBLL = new CustomGuerdonArchiveHISBLL();
            List<T_HR_CUSTOMGUERDONARCHIVE> archiveList = new List<T_HR_CUSTOMGUERDONARCHIVE>();
            archiveList = archiveBLL.GetCustomGuerdonArchiveByArchiveID(archiveID);
            if (archiveList != null)
            {
                foreach (T_HR_CUSTOMGUERDONARCHIVE temp in archiveList)
                {
                    T_HR_CUSTOMGUERDONARCHIVEHIS archiveHis = new T_HR_CUSTOMGUERDONARCHIVEHIS();
                    archiveHis.CUSTOMGUERDONARCHIVEID = Guid.NewGuid().ToString();
                    archiveHis.CREATEUSERID = temp.CREATEUSERID;
                    archiveHis.CREATEDATE = System.DateTime.Now;
                    archiveHis.T_HR_SALARYARCHIVEHIS = new T_HR_SALARYARCHIVEHIS();
                    archiveHis.T_HR_SALARYARCHIVEHIS.SALARYARCHIVEID = archiveID;
                    archiveHis.SUM = temp.SUM;
                    archiveHis.REMARK = temp.REMARK;
                    archiveHis.CUSTOMERGUERDONID = temp.CUSTOMERGUERDONID;
                    dal.Add(archiveHis);
                    //hisBLL.CustomGuerdonArchiveHISAdd(archiveHis);
                }
            }

        }
        /// <summary>
        ///添加自定义薪资
        /// </summary>
        /// <param name="stander"></param>
        /// <returns></returns>
        public void CustomArchiveAdd(T_HR_SALARYARCHIVE archive)
        {
            try
            {
                CustomGuerdonBLL custombll = new CustomGuerdonBLL();
                CustomGuerdonArchiveBLL GuerdonArchivebll = new CustomGuerdonArchiveBLL();
                List<V_CUSTOMGUERDON> salaryCusomvView = new List<V_CUSTOMGUERDON>();
                if (archive.T_HR_SALARYSTANDARD != null)
                {
                    salaryCusomvView = custombll.GetCustomGuerdon(archive.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                    T_HR_CUSTOMGUERDONARCHIVE customArchive = new T_HR_CUSTOMGUERDONARCHIVE();
                    foreach (V_CUSTOMGUERDON a in salaryCusomvView)
                    {
                        customArchive.SUM = a.GUERDONSUM;
                        customArchive.CUSTOMERGUERDONID = a.CUSTOMGUERDONSETID;
                        customArchive.CUSTOMGUERDONARCHIVEID = Guid.NewGuid().ToString();
                        customArchive.T_HR_SALARYARCHIVE = new T_HR_SALARYARCHIVE();
                        customArchive.T_HR_SALARYARCHIVE.SALARYARCHIVEID = archive.SALARYARCHIVEID;
                        //GuerdonArchivebll.CustomGuerdonArchiveAdd(customArchive);
                        dal.AddToContext(customArchive);
                        //DataContext.AddObject("T_HR_CUSTOMGUERDONARCHIVE", customArchive);
                    }
                }
                dal.SaveContextChanges();
            }
            catch { }
        }
        /// <summary>
        /// 添加薪资档案历史
        /// </summary>
        //public void SalaryArchiveHisAdd(T_HR_SALARYARCHIVE archive)
        //{
        //    SalaryArchiveHisBLL hisBll = new SalaryArchiveHisBLL();
        //    T_HR_SALARYARCHIVEHIS his = new T_HR_SALARYARCHIVEHIS();
        //    his.SALARYSOLUTIONID = archive.SALARYSOLUTIONID;
        //    his.SALARYARCHIVEID = archive.SALARYARCHIVEID;
        //    his.CREATECOMPANYID = archive.CREATECOMPANYID;
        //    his.CREATEDATE = archive.CREATEDATE;
        //    his.CREATEDEPARTMENTID = archive.CREATEDEPARTMENTID;
        //    his.CREATEPOSTID = archive.CREATEPOSTID;
        //    his.CREATEUSERID = archive.CREATEUSERID;

        //    his.FOODALLOWANCE = archive.FOODALLOWANCE;
        //    his.HOUSINGALLOWANCE = archive.HOUSINGALLOWANCE;
        //    his.HOUSINGALLOWANCEDEDUCT = archive.HOUSINGALLOWANCEDEDUCT;
        //    his.POSTSALARY = archive.POSTSALARY;
        //    his.BASESALARY = archive.BASESALARY;
        //    his.OTHERADDDEDUCT = archive.OTHERADDDEDUCT;
        //    his.OTHERADDDEDUCTDESC = archive.OTHERADDDEDUCTDESC;
        //    his.AREADIFALLOWANCE = archive.AREADIFALLOWANCE;

        //    his.SECURITYALLOWANCE = archive.SECURITYALLOWANCE;
        //    his.OTHERSUBJOIN = archive.OTHERSUBJOIN;
        //    his.OTHERSUBJOINDESC = archive.OTHERSUBJOINDESC;
        //    his.PERSONALINCOMERATIO = archive.PERSONALINCOMERATIO;
        //    his.PERSONALSIRATIO = archive.PERSONALINCOMERATIO;

        //    his.CREATEUSERID = archive.CREATEUSERID;
        //    his.CREATEDATE = System.DateTime.Now;
        //    his.EMPLOYEEID = archive.EMPLOYEEID;
        //    his.EMPLOYEENAME = archive.EMPLOYEENAME;
        //    his.EMPLOYEECODE = archive.EMPLOYEECODE;
        //    his.OWNERCOMPANYID = archive.OWNERCOMPANYID;
        //    his.OWNERDEPARTMENTID = archive.OWNERDEPARTMENTID;
        //    his.OWNERID = archive.OWNERID;
        //    his.OWNERPOSTID = archive.OWNERPOSTID;
        //    if (archive.T_HR_SALARYSTANDARD != null)
        //    {
        //        his.SALARYSTANDARDID = archive.T_HR_SALARYSTANDARD.SALARYSTANDARDID;
        //    }
        //    dal.AddToContext(his);
        //    dal.SaveContextChanges();
        //    //DataContext.AddObject("T_HR_SALARYARCHIVEHIS", his);
        //    //hisBll.SalaryArchiveHisAdd(his);
        //}
        public int CustomGuerdonArchiveDelByArchiveID(T_HR_SALARYARCHIVE archive)
        {
            var ents = from e in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>().Include("T_HR_SALARYARCHIVE")
                       where e.T_HR_SALARYARCHIVE.SALARYARCHIVEID == archive.SALARYARCHIVEID
                       select e;

            //检测员工是否有活动经费下拨额度，如果没有，则返回1，以便修改薪资档案时，能正常进行
            if (ents != null)
            {
                if (ents.Count() == 0)
                {
                    return 1;
                }
            }

            //CustomGuerdonArchiveBLL bll = new CustomGuerdonArchiveBLL();
            //bll.CustomGuerdonArchiveDelByArchiveID(archive.SALARYARCHIVEID);
            foreach (var q in ents)
            {
                dal.DeleteFromContext(q);
                //DataContext.DeleteObject(q);
            }
            return dal.SaveContextChanges();

        }
        /// <summary>
        /// 最初的薪资  员工工资的结算基本数据
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回最初的薪资</returns>
        public double InitialSalary(string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                       join b in dal.GetObjects<T_HR_POST>() on a.T_HR_POST.POSTID equals b.POSTID
                       join c in dal.GetObjects<T_HR_POSTDICTIONARY>() on b.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals c.POSTDICTIONARYID
                       join d in dal.GetObjects<T_HR_POSTLEVELDISTINCTION>() on c.POSTLEVEL equals d.POSTLEVEL
                       where a.T_HR_EMPLOYEE.EMPLOYEEID == employeeid //&& a.ISAGENCY == "0"
                       select new
                       {
                           d.BASICSALARY,
                           d.LEVELBALANCE,
                           c.SALARYLEVEL
                       };
            if (ents.Count() > 0)
            {
                var ent = ents.FirstOrDefault();
                return Convert.ToDouble(ent.BASICSALARY) + Convert.ToDouble(ent.LEVELBALANCE) * (40 - Convert.ToDouble(ent.SALARYLEVEL));
            }//薪资等级数目???
            return 0;
        }
        /// <summary>
        /// 根据员工ID,方案标准获取薪资标准
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        public T_HR_SALARYSTANDARD GetStandardByEmployeeIDAndSolutionID(string employeeid, string solutionID)
        {
            T_HR_SALARYSTANDARD stand = new T_HR_SALARYSTANDARD();
            decimal? postlevel = 0;
            string salarylevel = "";
            var tmp = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                      where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeid && ep.ISAGENCY == "0" && ep.EDITSTATE == "1"
                      select new
                      {
                          SALARYLEVEL = ep.SALARYLEVEL,
                          POSTLEVEL = ep.POSTLEVEL
                      };
            if (tmp.Count() > 0)
            {
                postlevel = Convert.ToDecimal(tmp.FirstOrDefault().POSTLEVEL);
                salarylevel = tmp.FirstOrDefault().SALARYLEVEL.ToString();
            }
            else
            {
                return null;
            }
            var employeeStand = from st in dal.GetObjects<T_HR_SALARYSTANDARD>()
                                join sa in dal.GetObjects<T_HR_SALARYLEVEL>() on st.T_HR_SALARYLEVEL.SALARYLEVELID equals sa.SALARYLEVELID
                                where st.SALARYSOLUTIONID == solutionID && sa.T_HR_POSTLEVELDISTINCTION.POSTLEVEL == postlevel && sa.SALARYLEVEL == salarylevel
                                select st;
            if (employeeStand.Count() > 0)
            {
                stand = employeeStand.FirstOrDefault();
            }
            else
            {
                stand = null;
            }
            return stand;
            #region
            //var tmp = (from a in DataContext.T_HR_EMPLOYEEPOST.Include("T_HR_EMPLOYEE")
            //           join b in DataContext.T_HR_POST on a.T_HR_POST.POSTID equals b.POSTID
            //           where a.T_HR_EMPLOYEE.EMPLOYEEID == employeeid
            //           select b.SALARYLEVEL).FirstOrDefault();
            //if (string.IsNullOrEmpty(tmp))
            //{
            //    return null;
            //}

            //else
            //{
            //    #region 采用增加岗位时选择的薪资级别
            //    var ents = from a in DataContext.T_HR_EMPLOYEEPOST.Include("T_HR_EMPLOYEE")
            //               join b in DataContext.T_HR_POST on a.T_HR_POST.POSTID equals b.POSTID
            //               join c in DataContext.T_HR_POSTDICTIONARY on b.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals c.POSTDICTIONARYID
            //               join d in DataContext.T_HR_POSTLEVELDISTINCTION on c.POSTLEVEL equals d.POSTLEVEL
            //               join e in DataContext.T_HR_SALARYLEVEL on new { d.POSTLEVELID, b.SALARYLEVEL } equals new
            //               {
            //                   e.T_HR_POSTLEVELDISTINCTION.POSTLEVELID,
            //                   e.SALARYLEVEL
            //               }
            //               join f in DataContext.T_HR_SALARYSTANDARD on e.SALARYLEVELID equals f.T_HR_SALARYLEVEL.SALARYLEVELID
            //               where a.T_HR_EMPLOYEE.EMPLOYEEID == employeeid && a.ISAGENCY == "0" && f.SALARYSOLUTIONID ==solutionID
            //               select f;


            //    if (ents.Count() > 0)
            //    {
            //        stand = ents.FirstOrDefault();
            //        return stand;
            //    }
            //    else
            //    {
            //        return null;
            //    }
            //    #endregion
            //}
            #endregion
        }

        /// <summary>
        /// 根据员工ID 方案ID 薪资等级获取薪资标准
        /// </summary>
        /// <param name="employeeid"></param>
        /// <param name="solutionID"></param>
        /// <param name="Salarylevel"></param>
        /// <returns></returns>
        public T_HR_SALARYSTANDARD GetStandardByEmployeeIDAndSolutionIDAndSalarylevel(string employeeid, string solutionID, int Salarylevel)
        {
            T_HR_SALARYSTANDARD stand = new T_HR_SALARYSTANDARD();
            decimal? postlevel = 0;
            string salarylevel = Salarylevel.ToString();
            var tmp = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                      where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeid && ep.ISAGENCY == "0" && ep.EDITSTATE == "1"
                      select new
                      {
                          SALARYLEVEL = ep.SALARYLEVEL,
                          POSTLEVEL = ep.POSTLEVEL
                      };
            if (tmp.Count() > 0)
            {
                postlevel = Convert.ToDecimal(tmp.FirstOrDefault().POSTLEVEL);
                //  salarylevel = tmp.FirstOrDefault().SALARYLEVEL.ToString();
            }
            else
            {
                return null;
            }
            var employeeStand = from st in dal.GetObjects<T_HR_SALARYSTANDARD>()
                                join sa in dal.GetObjects<T_HR_SALARYLEVEL>() on st.T_HR_SALARYLEVEL.SALARYLEVELID equals sa.SALARYLEVELID
                                join pls in dal.GetObjects<T_HR_POSTLEVELDISTINCTION>() on sa.T_HR_POSTLEVELDISTINCTION.POSTLEVELID equals pls.POSTLEVELID
                                where st.SALARYSOLUTIONID == solutionID && pls.POSTLEVEL == postlevel && sa.SALARYLEVEL == salarylevel
                                select st;
            if (employeeStand.Count() > 0)
            {
                stand = employeeStand.FirstOrDefault();
            }
            else
            {
                stand = null;
            }
            return stand;
        }
        /// <summary>
        /// 根据标准ID获取标准的薪资项
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        public List<T_HR_SALARYSTANDARDITEM> GetstandardItemByStandardID(string standardid)
        {
            List<T_HR_SALARYSTANDARDITEM> items = new List<T_HR_SALARYSTANDARDITEM>();
            SalaryStandardItemBLL bll = new SalaryStandardItemBLL();
            items = bll.GetSalaryStandardItemsByStandardID(standardid);
            return items;
        }

        /// <summary>
        /// 根据标准的薪资项ID获取标准设置
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        public List<T_HR_SALARYITEM> GetSalaryItemSetByItems(List<T_HR_SALARYSTANDARDITEM> items)
        {
            List<T_HR_SALARYITEM> salaryItems = new List<T_HR_SALARYITEM>();
            SalaryItemSetBLL bll = new SalaryItemSetBLL();
            foreach (var item in items)
            {
                salaryItems.Add(bll.GetSalaryItemSetByID(item.T_HR_SALARYITEM.SALARYITEMID));
            }
            return salaryItems;
        }

        public void AddArchiveItems(T_HR_SALARYARCHIVE archive)
        {

            List<T_HR_SALARYITEM> salaryItems = new List<T_HR_SALARYITEM>();
            List<T_HR_SALARYSTANDARDITEM> standerItems = new List<T_HR_SALARYSTANDARDITEM>();
            SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL();
            standerItems = GetstandardItemByStandardID(archive.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
            // salaryItems = GetSalaryItemSetByItems(standerItems);
            foreach (var item in standerItems)
            {
                T_HR_SALARYARCHIVEITEM archiveitem = new T_HR_SALARYARCHIVEITEM();
                archiveitem.SALARYARCHIVEITEM = Guid.NewGuid().ToString();
                //archiveitem.T_HR_SALARYARCHIVE =  new T_HR_SALARYARCHIVE();
                //archiveitem.T_HR_SALARYARCHIVE.SALARYARCHIVEID = archive.SALARYARCHIVEID;

                archiveitem.T_HR_SALARYARCHIVEReference.EntityKey =
           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVE", "SALARYARCHIVEID", archive.SALARYARCHIVEID);


                //archiveitem.T_HR_SALARYARCHIVE.SALARYSOLUTIONID = archive.SALARYSOLUTIONID;

                archiveitem.SALARYSTANDARDID = archive.T_HR_SALARYSTANDARD.SALARYSTANDARDID;
                archiveitem.SUM = string.IsNullOrEmpty(item.SUM) ? string.Empty : AES.AESEncrypt(item.SUM);
                archiveitem.CALCULATEFORMULA = item.T_HR_SALARYITEM.CALCULATEFORMULA;
                archiveitem.CALCULATEFORMULACODE = item.T_HR_SALARYITEM.CALCULATEFORMULACODE;
                archiveitem.SALARYITEMID = item.T_HR_SALARYITEM.SALARYITEMID;
                archiveitem.CREATEUSERID = archive.CREATEUSERID;
                archiveitem.CREATEDATE = System.DateTime.Now;
                archiveitem.ORDERNUMBER = item.ORDERNUMBER;
                Utility.RefreshEntity(archiveitem);
                //bll.Add(archiveitem);
                dal.AddToContext(archiveitem);
                //DataContext.AddObject("T_HR_SALARYARCHIVEITEM", archiveitem);

                //bll.SalaryArchiveItemAdd(archiveitem);
            }
            dal.SaveContextChanges();
            //DataContext.SaveChanges();

        }

        public void AddArchiveItems(T_HR_SALARYARCHIVE archive, T_HR_SALARYSTANDARD stand)
        {

            List<T_HR_SALARYITEM> salaryItems = new List<T_HR_SALARYITEM>();
            List<T_HR_SALARYSTANDARDITEM> standerItems = new List<T_HR_SALARYSTANDARDITEM>();
            SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL();
            standerItems = GetstandardItemByStandardID(stand.SALARYSTANDARDID);
            // salaryItems = GetSalaryItemSetByItems(standerItems);
            foreach (var item in standerItems)
            {
                T_HR_SALARYARCHIVEITEM archiveitem = new T_HR_SALARYARCHIVEITEM();
                archiveitem.SALARYARCHIVEITEM = Guid.NewGuid().ToString();
                //archiveitem.T_HR_SALARYARCHIVE =  new T_HR_SALARYARCHIVE();
                //archiveitem.T_HR_SALARYARCHIVE.SALARYARCHIVEID = archive.SALARYARCHIVEID;

                archiveitem.T_HR_SALARYARCHIVEReference.EntityKey =
           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVE", "SALARYARCHIVEID", archive.SALARYARCHIVEID);


                //archiveitem.T_HR_SALARYARCHIVE.SALARYSOLUTIONID = archive.SALARYSOLUTIONID;

                archiveitem.SALARYSTANDARDID = stand.SALARYSTANDARDID;
                archiveitem.SUM = string.IsNullOrEmpty(item.SUM) ? string.Empty : AES.AESEncrypt(item.SUM);
                archiveitem.CALCULATEFORMULA = item.T_HR_SALARYITEM.CALCULATEFORMULA;
                archiveitem.CALCULATEFORMULACODE = item.T_HR_SALARYITEM.CALCULATEFORMULACODE;
                archiveitem.SALARYITEMID = item.T_HR_SALARYITEM.SALARYITEMID;
                archiveitem.CREATEUSERID = archive.CREATEUSERID;
                archiveitem.CREATEDATE = System.DateTime.Now;
                archiveitem.ORDERNUMBER = item.ORDERNUMBER;
                Utility.RefreshEntity(archiveitem);
                //bll.Add(archiveitem);
                dal.AddToContext(archiveitem);
                //DataContext.AddObject("T_HR_SALARYARCHIVEITEM", archiveitem);

                //bll.SalaryArchiveItemAdd(archiveitem);
            }
            dal.SaveContextChanges();
            //DataContext.SaveChanges();

        }

        /// <summary>
        /// 增加薪资档案历史项
        /// </summary>
        /// <param name="archive"></param>
        //public void AddSalaryArchiveHisItem(T_HR_SALARYARCHIVE archive)
        //{
        //    List<T_HR_SALARYITEM> salaryItems = new List<T_HR_SALARYITEM>();
        //    List<T_HR_SALARYSTANDARDITEM> standerItems = new List<T_HR_SALARYSTANDARDITEM>();
        //    standerItems = GetstandardItemByStandardID(archive.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
        //    foreach (var item in standerItems)
        //    {
        //        T_HR_SALARYARCHIVEHISITEM archivehisitem = new T_HR_SALARYARCHIVEHISITEM();
        //        archivehisitem.SALARYARCHIVEITEMID = Guid.NewGuid().ToString();
        //        archivehisitem.T_HR_SALARYARCHIVEHISReference.EntityKey =
        //   new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVEHIS", "SALARYARCHIVEID", archive.SALARYARCHIVEID);

        //        archivehisitem.SALARYSTANDARDID = archive.T_HR_SALARYSTANDARD.SALARYSTANDARDID;
        //        archivehisitem.SUM = string.IsNullOrEmpty(item.SUM) ? string.Empty : AES.AESEncrypt(item.SUM);
        //        archivehisitem.CALCULATEFORMULA = item.T_HR_SALARYITEM.CALCULATEFORMULA;
        //        archivehisitem.CALCULATEFORMULACODE = item.T_HR_SALARYITEM.CALCULATEFORMULACODE;
        //        archivehisitem.SALARYITEMID = item.T_HR_SALARYITEM.SALARYITEMID;
        //        archivehisitem.CREATEUSERID = archive.CREATEUSERID;
        //        archivehisitem.CREATEDATE = System.DateTime.Now;
        //        archivehisitem.ORDERNUMBER = item.ORDERNUMBER;
        //        dal.AddToContext(archivehisitem);
        //        dal.SaveContextChanges();
        //        //dal.AddObject("T_HR_SALARYARCHIVEHISITEM", archivehisitem);
        //    }
        //}

        public int ArchiveDeleteByArchiveID(string archiveID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVEITEM>().Include("T_HR_SALARYARCHIVE")
                       where o.T_HR_SALARYARCHIVE.SALARYARCHIVEID == archiveID
                       select o;



            //List<T_HR_SALARYARCHIVEITEM> items = new List<T_HR_SALARYARCHIVEITEM>();
            //items = GetSalaryArchiveItemsByArchiveID(archiveID);
            if (ents.Count() > 0)
            {
                foreach (var item in ents)
                {
                    dal.DeleteFromContext(item);
                    //DataContext.DeleteObject(item);
                }
                return dal.SaveContextChanges();
            }
            else
            {
                return 1;//不存在薪资项默认为1
            }


            //SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL();
            //bll.SalaryArchiveItemDeleteByArchiveID(archiveID);
        }
        public void CreateEmployeeSalaryArchiveItems(List<string> solutionID)
        {
            try
            {
                dal.BeginTransaction();
                foreach (var strItem in solutionID)
                {
                    // 根据方案ID获取薪资档案
                    var salaryArchives = from c in dal.GetObjects<T_HR_SALARYARCHIVE>()
                                         where c.SALARYSOLUTIONID == strItem
                                         select c;
                    #region
                    foreach (var salaryArchive in salaryArchives)
                    {
                        //根据方案ID和薪资档案的信息获取员工的薪资标准
                        string employeeSalaryLevel = salaryArchive.SALARYLEVEL.ToString();
                        var stand = (from st in dal.GetObjects<T_HR_SALARYSTANDARD>()
                                     join sa in dal.GetObjects<T_HR_SALARYLEVEL>() on st.T_HR_SALARYLEVEL.SALARYLEVELID equals sa.SALARYLEVELID
                                     where st.SALARYSOLUTIONID == salaryArchive.SALARYSOLUTIONID && sa.T_HR_POSTLEVELDISTINCTION.POSTLEVEL == salaryArchive.POSTLEVEL && sa.SALARYLEVEL == employeeSalaryLevel
                                     select st).FirstOrDefault();

                        if (stand != null)
                        {
                            //删除薪资档案的薪资项
                            //  ArchiveDeleteByArchiveID(salaryArchive.SALARYARCHIVEID);
                            //salaryArchive.T_HR_SALARYSTANDARD = new T_HR_SALARYSTANDARD();
                            //salaryArchive.T_HR_SALARYSTANDARD.SALARYSTANDARDID = stand.SALARYSTANDARDID;
                            salaryArchive.T_HR_SALARYSTANDARDReference.EntityKey =
       new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", stand.SALARYSTANDARDID);
                            dal.UpdateFromContext(salaryArchive);
                            dal.SaveContextChanges();
                            //添加薪资档案项目
                            AddArchiveItems(salaryArchive, stand);

                        }

                    }
                    #endregion

                }
                dal.CommitTransaction();
            }


            catch (Exception ex)
            {
                dal.CommitTransaction();
                ex.Message.ToString();
            }
        }
        #endregion

        /// <summary>
        /// 服务引擎
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {

                int i = 0;
                //根据薪资档案ID查询薪资档案表
                var archive = (from c in dal.GetObjects<T_HR_SALARYARCHIVE>()
                               where c.SALARYARCHIVEID == EntityKeyValue
                               select c).FirstOrDefault();
                if (archive != null)
                {
                    //审核状态
                    archive.CHECKSTATE = CheckState;
                    //更改时间
                    archive.UPDATEDATE = DateTime.Now;
                    //更新表薪资档案表
                    dal.UpdateFromContext(archive);
                    i = dal.SaveContextChanges();
                    //如果终审通过
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        #region 修改员工岗位表
                        //根据员工岗位ID查员工岗位表
                        var empost = (from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                      where c.EMPLOYEEPOSTID == archive.EMPLOYEEPOSTID
                                      select c).FirstOrDefault();
                        if (empost != null)
                        {
                            //薪资等级
                            empost.SALARYLEVEL = archive.SALARYLEVEL;
                            //更新工岗位表
                            dal.UpdateFromContext(empost);
                            dal.SaveContextChanges();
                        }

                        #endregion

                        #region 员工薪资档案报表信息同步 weirui 2012-7-11
                        T_HR_EMPLOYEECHANGEHISTORY employeeEntity = new T_HR_EMPLOYEECHANGEHISTORY();

                        employeeEntity.RECORDID = Guid.NewGuid().ToString();
                        //员工ID
                        //employeeEntity.T_HR_EMPLOYEE.EMPLOYEEID = archive.EMPLOYEEID;
                        employeeEntity.T_HR_EMPLOYEEReference.EntityKey =
                                         new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", archive.EMPLOYEEID);
                        //员工姓名
                        employeeEntity.EMPOLYEENAME = archive.EMPLOYEENAME;

                        //根据员工ID查询员工基本信息表
                        var employee = dal.GetObjects<T_HR_EMPLOYEE>().FirstOrDefault(s => s.EMPLOYEEID == archive.EMPLOYEEID);
                        //指纹编号
                        if (employee != null)
                        {
                            employeeEntity.FINGERPRINTID = employee.FINGERPRINTID;
                        }

                        //0.入职1.异动2.离职3.薪资级别变更4.签订合同
                        employeeEntity.FORMTYPE = "3";
                        //记录原始单据id（员工入职表ID）
                        employeeEntity.FORMID = archive.SALARYARCHIVEID;
                        //主岗位非主岗位
                        employeeEntity.ISMASTERPOSTCHANGE = empost.ISAGENCY;
                        //包括 异动类型及离职类型 0:1=异动类型：离职类型
                        //employeeEntity.CHANGETYPE = "0";
                        //异动时间
                        employeeEntity.CHANGETIME = DateTime.Now;
                        //异动原因
                        employeeEntity.CHANGEREASON = "员工薪资档案";
                        //异动前岗位id
                        employeeEntity.OLDPOSTID = archive.EMPLOYEEPOSTID;

                        //备注
                        employeeEntity.REMART = archive.REMARK;
                        //创建时间
                        employeeEntity.CREATEDATE = DateTime.Now;
                        //所属员工ID
                        employeeEntity.OWNERID = archive.OWNERID;
                        //所属岗位ID
                        employeeEntity.OWNERPOSTID = archive.OWNERPOSTID;
                        //所属部门ID
                        employeeEntity.OWNERDEPARTMENTID = archive.OWNERDEPARTMENTID;
                        //所属公司ID
                        employeeEntity.OWNERCOMPANYID = archive.OWNERCOMPANYID;
                        dal.AddToContext(employeeEntity);
                        dal.SaveContextChanges();
                        #endregion
                    }
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }


        #region 导出员工薪资档案变更记录
        /// <summary>
        /// 导出员工薪资档案变更记录
        /// </summary>
        /// <param name="pageIndex">当前页 </param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数 </param>
        /// <param name="userID">用户ID</param>
        /// <param name="queryCode">查询码 0-有效 1-历史</param>
        /// <returns>查询结果集</returns>
        public byte[] ExportSalaryArchive(string sort, string filterString, IList<object> paras,
                                                              string userID, string CheckState, int orgtype, string orgid, int queryCode, string companyID)
        {
            byte[] result = null;
            List<V_SALARYARCHIVEITEM> salaryItemList = new List<V_SALARYARCHIVEITEM>();
            List<V_SALARYARCHIVEITEM> itemList = new List<V_SALARYARCHIVEITEM>();
            List<T_HR_SALARYARCHIVE> entlist = new List<T_HR_SALARYARCHIVE>();
            List<T_HR_SALARYARCHIVE> entlistSort = new List<T_HR_SALARYARCHIVE>();
            
            List<string> archiveids = new List<string>();
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                //orgtype:0 公司 1部门 2岗位
                IQueryable<T_HR_SALARYARCHIVE> ents = GetArchiveFilter(orgtype, orgid);
                //如果是查询可用的薪资档案 则审核状态一定是通过的
                if (queryCode == 0)
                    CheckState = ((int)CheckStates.Approved).ToString();
                if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
                {
                    SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYARCHIVE");
                    if (CheckState != Convert.ToInt32(CheckStates.All).ToString())
                    {
                        if (queryParas.Count() > 0)
                        {
                            filterString += " AND ";
                        }
                        filterString += "CHECKSTATE==@" + queryParas.Count().ToString();
                        queryParas.Add(CheckState);
                    }
                }
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                if (ents.Any())
                {
                    ents = QueryByUsableCode(ents, queryCode, companyID);
                    foreach (var item in ents)
                    {
                        archiveids.Add(item.SALARYARCHIVEID);
                    }
                    using (SalaryArchiveItemBLL salaryArchiveItemBLL = new SalaryArchiveItemBLL())
                    {
                        salaryItemList = salaryArchiveItemBLL.GetSalaryArchiveItemsByArchiveIDs(archiveids);
                    }
                    foreach (var item in ents)
                    {
                        itemList = (from c in salaryItemList
                                    where c.SALARYARCHIVEID == item.SALARYARCHIVEID
                                    select c).ToList();
                        if (itemList.Any())
                        {
                            #region 设置薪资项
                            var baseSalary = itemList.Where(m => m.SALARYITEMNAME == "基本工资");
                            var postSalary = itemList.Where(m => m.SALARYITEMNAME == "岗位工资");
                            var securityAllowance = itemList.Where(m => m.SALARYITEMNAME == "保密津贴");
                            var housingAllowance = itemList.Where(m => m.SALARYITEMNAME == "住房补贴");
                            var areadifAllowance = itemList.Where(m => m.SALARYITEMNAME == "地区差异补贴");
                            var foodAllowance = itemList.Where(m => m.SALARYITEMNAME == "餐费补贴");
                            if (baseSalary.Any())
                            {
                                item.BASESALARY = Convert.ToDecimal(AES.AESDecrypt(baseSalary.FirstOrDefault().SUM));
                            }
                            else
                            {
                                item.BASESALARY = 0;
                            }
                            if (postSalary.Any())
                            {
                                item.POSTSALARY = Convert.ToDecimal(AES.AESDecrypt(postSalary.FirstOrDefault().SUM));
                            }
                            else
                            {
                                item.POSTSALARY = 0;
                            }
                            if (securityAllowance.Any())
                            {
                                item.SECURITYALLOWANCE = Convert.ToDecimal(AES.AESDecrypt(securityAllowance.FirstOrDefault().SUM));
                            }
                            else
                            {
                                item.SECURITYALLOWANCE = 0;
                            }
                            if (housingAllowance.Any())
                            {
                                item.HOUSINGALLOWANCE = Convert.ToDecimal(AES.AESDecrypt(housingAllowance.FirstOrDefault().SUM));
                            }
                            else
                            {
                                item.HOUSINGALLOWANCE = 0;
                            }
                            if (areadifAllowance.Any())
                            {
                                item.AREADIFALLOWANCE = Convert.ToDecimal(AES.AESDecrypt(areadifAllowance.FirstOrDefault().SUM));
                            }
                            else
                            {
                                item.AREADIFALLOWANCE = 0;
                            }
                            if (foodAllowance.Any())
                            {
                                item.FOODALLOWANCE = Convert.ToDecimal(AES.AESDecrypt(foodAllowance.FirstOrDefault().SUM));
                            }
                            else
                            {
                                item.FOODALLOWANCE = 0;
                            }
                            #endregion
                        }
                        else
                        {
                            item.BASESALARY = 0;
                            item.POSTSALARY = 0;
                            item.SECURITYALLOWANCE = 0;
                            item.HOUSINGALLOWANCE = 0;
                            item.AREADIFALLOWANCE = 0;
                            item.FOODALLOWANCE = 0;
                            SMT.Foundation.Log.Tracer.Debug("位置:SMT.HRM.BLL.SalaryArchiveBLL.ExportSalaryArchive();\r\n"
                            + "错误内容：根据薪资档案ID无法找到薪资档案薪资项，薪资档案ID:" + item.SALARYARCHIVEID
                           +"\r\n员工姓名:" + item.EMPLOYEENAME +";\r\n薪资年月:"+item.OTHERSUBJOIN+"年"+item.OTHERADDDEDUCT+"月" );
                        }
                        entlist.Add(item);
                    }
                    entlistSort = entlist.OrderByDescending(c => c.OTHERSUBJOIN).ThenByDescending(c => c.OTHERADDDEDUCT).OrderBy(c => c.EMPLOYEENAME).ToList();
                    result = ExportSalaryArchiveToExcel(entlistSort);
                }
                else {
                    SMT.Foundation.Log.Tracer.Debug("薪资档案导出出错：\r\n位置:SMT.HRM.BLL.SalaryArchiveBLL.ExportSalaryArchive();行1927\r\n根据filterString过滤薪资档案为空");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("薪资档案导出异常：\r\n位置:SMT.HRM.BLL.SalaryArchiveBLL.ExportSalaryArchive();\r\n异常信息:" + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 导出员工薪资档案变更记录
        /// </summary>
        public static byte[] ExportSalaryArchiveToExcel(List<T_HR_SALARYARCHIVE> salaryArchiveList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetSalaryArchiveBody(salaryArchiveList).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());
            return by;
        }

        public static StringBuilder GetSalaryArchiveBody(List<T_HR_SALARYARCHIVE> salaryArchiveList)
        {
            StringBuilder s = new StringBuilder();
            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" BORDER=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td  align=center class=\"title\" colspan=\"13\">员工薪资档案变更记录表</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");
            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" >序号</td>");
            s.Append("<td align=center class=\"title\" >员工编号</td>");
            s.Append("<td align=center class=\"title\" >员工姓名</td>");
            s.Append("<td align=center class=\"title\" >生效日期</td>");
            s.Append("<td align=center class=\"title\" >基本工资</td>");
            s.Append("<td align=center class=\"title\" >岗位工资</td>");
            s.Append("<td align=center class=\"title\" >保密津贴</td>");
            s.Append("<td align=center class=\"title\" >住房补贴</td>");
            s.Append("<td align=center class=\"title\" >地区差异补贴</td>");
            s.Append("<td align=center class=\"title\" >餐费补贴</td>");
            s.Append("<td align=center class=\"title\" >固定收入合计</td>");
            s.Append("<td align=center class=\"title\" >终审通过时间</td>");
            s.Append("<td align=center class=\"title\" >备注</td>");
            s.Append("</tr>");

            if (salaryArchiveList.Any())
            {
                foreach (var item in salaryArchiveList)
                {
                    decimal sum = 0;
                    s.Append("<tr>");
                    s.Append("<td class=\"x1282\">" + (salaryArchiveList.IndexOf(item)+1).ToString() + "</td>");
                    s.Append("<td class=\"x1282\">" + item.EMPLOYEECODE + "</td>");
                    s.Append("<td class=\"x1282\">" + item.EMPLOYEENAME + "</td>");
                    s.Append("<td class=\"x1282\">" + item.OTHERSUBJOIN + "年" + item.OTHERADDDEDUCT + "月" + "</td>");
                    s.Append("<td class=\"x1282\">" +  item.BASESALARY.Value + "</td>"); //基本工资
                    sum += item.BASESALARY.Value;
                    s.Append("<td class=\"x1282\">" + item.POSTSALARY.Value + "</td>"); //岗位工资
                    sum += item.POSTSALARY.Value;
                    s.Append("<td class=\"x1282\">" + item.SECURITYALLOWANCE.Value + "</td>");//保密津贴
                    sum += item.SECURITYALLOWANCE.Value;
                    s.Append("<td class=\"x1282\">" + item.HOUSINGALLOWANCE.Value + "</td>"); //住房津贴
                    sum += item.HOUSINGALLOWANCE.Value;
                    s.Append("<td class=\"x1282\">" + item.AREADIFALLOWANCE.Value + "</td>"); //地区差异补贴
                    sum += item.AREADIFALLOWANCE.Value;
                    s.Append("<td class=\"x1282\">" + item.FOODALLOWANCE.Value + "</td>"); //餐费补贴
                    sum += item.FOODALLOWANCE.Value;
                    s.Append("<td class=\"x1282\">" + sum + "</td>"); //固定收入合计
                    if (item.CHECKSTATE == "2")
                    {
                        if (item.UPDATEDATE.HasValue)
                        {
                            s.Append("<td class=\"x1282\">" + item.UPDATEDATE.Value.ToString("yyyy年MM月dd日") + "</td>"); //审核通过时间
                        }
                    }
                    else {
                        if (item.UPDATEDATE.HasValue)
                        {
                            s.Append("<td class=\"x1282\">未终审通过</td>"); //审核通过时间
                        }
                    }
                    //else {
                    //    s.Append("<td class=\"x1282\">" + item.CHECKSTATE + "</td>"); 
                    //}
                    s.Append("<td class=\"x1282\">" + item.REMARK + "</td>"); //备注
                    s.Append("</tr>");
                }
            }
            s.Append("</table>");
            s.Append("</body></html>");
            return s;
        }
        #endregion
    }
}
