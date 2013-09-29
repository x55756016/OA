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
using System.Data;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    public class EmployeeAddSumBLL : BaseBll<T_HR_EMPLOYEEADDSUM>, IOperate
    {
        /// <summary>
        /// 添加员工加扣款
        /// </summary>
        /// <param name="obj"> 加扣款实例</param>
        public void EmployeeAddSumADD(T_HR_EMPLOYEEADDSUM obj)
        {
            try
            {
                dal.Add(obj);
                // BLLCommonServices.Utility.SubmitMyRecord<T_HR_EMPLOYEEADDSUM>(obj);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeeAddSumADD:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 批量添加员工加扣款
        /// </summary>
        /// <param name="obj"> 加扣款实例集合</param>
        public bool EmployeeAddSumLotsofADD(List<T_HR_EMPLOYEEADDSUM> objs)
        {
            try
            {
                foreach (T_HR_EMPLOYEEADDSUM obj in objs)
                {
                    if (string.IsNullOrEmpty(obj.PROJECTCODE))
                    {
                        var ent = (from c in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                                   where c.ADDSUMID == obj.ADDSUMID
                                   select c).FirstOrDefault();
                        if (ent == null)
                        {                           
                            dal.AddToContext(obj);
                        }
                        else
                        {
                            ent.DEALMONTH = obj.DEALMONTH;
                            ent.DEALYEAR = obj.DEALYEAR;
                            ent.PROJECTMONEY = obj.PROJECTMONEY;
                            ent.PROJECTNAME = obj.PROJECTNAME;
                            ent.SYSTEMTYPE = obj.SYSTEMTYPE;
                            ent.REMARK = obj.REMARK;
                            
                            // ent.UPDATEDATE = System.DateTime.Now;
                            dal.UpdateFromContext(ent);
                        }
                    }
                    else
                    {
                        var ent = (from c in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                                   where c.ADDSUMID == obj.ADDSUMID
                                   select c).FirstOrDefault();
                        if (ent != null)
                        {
                            dal.DeleteFromContext(ent);
                        }

                    }

                }
                return dal.SaveContextChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeeAddSumLotsofADD:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 更新员工加扣款
        /// </summary>
        /// <param name="entity">加扣款实例</param>
        public void EmployeeAddSumUpdate(T_HR_EMPLOYEEADDSUM obj)
        {
            try
            {
                var ent = from a in dal.GetObjects().Include("T_HR_EMPLOYEEADDSUMBATCH")
                          where a.ADDSUMID == obj.ADDSUMID
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_EMPLOYEEADDSUM tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_HR_EMPLOYEEADDSUM>(obj, tmpEnt);

                    if (obj.T_HR_EMPLOYEEADDSUMBATCH != null)
                    {
                        tmpEnt.T_HR_EMPLOYEEADDSUMBATCHReference.EntityKey =
                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEADDSUMBATCH", "MONTHLYBATCHID", obj.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID);
                        tmpEnt.T_HR_EMPLOYEEADDSUMBATCH.EntityKey =
                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEADDSUMBATCH", "MONTHLYBATCHID", obj.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID);
                    }
                    dal.Update(tmpEnt);
                    //try
                    //{
                    //    BLLCommonServices.Utility.SubmitMyRecord<T_HR_EMPLOYEEADDSUM>(obj);
                    //}
                    //catch (Exception ex)
                    //{
                    //    SMT.Foundation.Log.Tracer.Debug(ex.Message);
                    //}
                }
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug(e.Message);
                e.Message.ToString();
                //throw e;
            }

        }
        /// <summary>
        /// 删除员工加扣款
        /// </summary>
        /// <param name="IDs">加扣款ID</param>
        /// <returns></returns>
        public int EmployeeAddSumDelete(string[] IDs)
        {
            int i = 0;
            try
            {
                foreach (string id in IDs)
                {
                    var ents = from e in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                               where e.ADDSUMID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                        //try
                        //{
                        //    BLLCommonServices.Utility.SubmitMyRecord<T_HR_EMPLOYEEADDSUM>(ent);
                        //}
                        //catch (Exception ex)
                        //{
                        //    SMT.Foundation.Log.Tracer.Debug(ex.Message);
                        //}
                    }

                    //TODO:删除项目所包含的明细
                }
                i = dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeeAddSumDelete:" + ex.Message);
            }

            return i;
        }

        /// <summary>
        /// 通过员工ID集合删除员工加扣款
        /// </summary>
        /// <param name="employeeIDs">员工ID集合</param>
        /// <returns></returns>
        public int EmployeeAddSumByEmployeeIDDelete(string[] employeeIDs, string year, string month)
        {
            try
            {
                foreach (string id in employeeIDs)
                {
                    var ents = from e in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                               where e.EMPLOYEEID == id && e.DEALYEAR == year && e.DEALMONTH == month
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                        //  BLLCommonServices.Utility.SubmitMyRecord<T_HR_EMPLOYEEADDSUM>(ent);
                    }

                    //TODO:删除项目所包含的明细
                }

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeeAddSumByEmployeeIDDelete:" + ex.Message);
            }
            return dal.SaveContextChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_SALARYSTANDARD> ents = from a in DataContext.T_HR_SALARYSTANDARD
        //                                           select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}
        /// <summary>
        /// 根据ID获取员工加扣款
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEEADDSUM GetEmployeeAddSumByID(string ID)
        {
            var ents = from a in dal.GetTable()
                       where a.ADDSUMID == ID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 员工加扣款视图
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public V_EmployeeAddsumView GetEmployeeAddSumViewByID(string ID)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                       join b in dal.GetObjects<T_HR_COMPANY>() on a.OWNERCOMPANYID equals b.COMPANYID
                       join c in dal.GetObjects<T_HR_DEPARTMENT>() on a.OWNERDEPARTMENTID equals c.DEPARTMENTID
                       join d in dal.GetObjects<T_HR_POST>() on a.OWNERPOSTID equals d.POSTID
                       where a.ADDSUMID == ID
                       select new V_EmployeeAddsumView
                       {
                           ADDSUMID = a.ADDSUMID,
                           EMPLOYEECODE = a.EMPLOYEECODE,
                           EMPLOYEENAME = a.EMPLOYEENAME,
                           PROJECTNAME = a.PROJECTNAME,
                           PROJECTCODE = a.PROJECTCODE,
                           PROJECTMONEY = a.PROJECTMONEY,
                           SYSTEMTYPE = a.SYSTEMTYPE,
                           DEALYEAR = a.DEALYEAR,
                           DEALMONTH = a.DEALMONTH,
                           CHECKSTATE = a.CHECKSTATE,
                           REMARK = a.REMARK,
                           OWNERID = a.OWNERID,
                           OWNERPOSTID = a.OWNERPOSTID,
                           OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                           OWNERCOMPANYID = a.OWNERCOMPANYID,
                           CREATEPOSTID = a.CREATEPOSTID,
                           CREATEDEPARTMENTID = a.CREATEDEPARTMENTID,
                           CREATECOMPANYID = a.CREATECOMPANYID,
                           CREATEDATE = a.CREATEDATE,
                           CREATEUSERID = a.CREATEUSERID,
                           UPDATEUSERID = a.UPDATEUSERID,
                           UPDATEDATE = a.UPDATEDATE,
                           EMPLOYEEID = a.EMPLOYEEID,
                           MONTHLYBATCHID = a.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID,
                           CompanyName = b.CNAME,
                           PostName = d.T_HR_POSTDICTIONARY.POSTNAME,
                           DepartmentName = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
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
        public new IQueryable<T_HR_EMPLOYEEADDSUM> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            IQueryable<T_HR_EMPLOYEEADDSUM> ents = dal.GetTable(); ;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEEADDSUM>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

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
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEEADDSUM> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, string userID, string CheckState, int orgtype, string orgid)
        {
            bool isAudit = false;//因为批量审核和查询条件都使用该方法，所以批量审核时加一个标识，在这里进行区分
            if (!string.IsNullOrWhiteSpace(filterString) && filterString == "audit")
            {
                filterString = "";
                isAudit = true;
            }
            string year = string.Empty;
            string filter2 = "";
            List<string> queryParasDt = new List<string>();
            DateTime dtNull = Convert.ToDateTime("0001-1-1 0:00:00");

            List<object> queryParas = new List<object>();

            List<T_HR_EMPLOYEEADDSUM> ent = new List<T_HR_EMPLOYEEADDSUM>();
            IQueryable<T_HR_EMPLOYEEADDSUM> ents = GetAddSumFilter(orgtype, orgid);
            var en = ents.GroupBy(y => y.ADDSUMID).Select(g => new { group = g.Key, groupcontent = g });
            foreach (var v in en)
            {
                ent.Add(v.groupcontent.FirstOrDefault());
            }
            ents = ent.AsQueryable();

            queryParas.AddRange(paras);
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEEADDSUM");

                if (!string.IsNullOrEmpty(CheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    if (isAudit && CheckState == Convert.ToString(Convert.ToInt32(CheckStates.All)))//表示为批量审核，这是也加载审核未通过的数据
                    {
                        CheckState = Convert.ToString(Convert.ToInt32(CheckStates.UnSubmit));
                        filterString += "(CHECKSTATE == @" + queryParas.Count();
                        queryParas.Add(CheckState);
                        filterString += " or CHECKSTATE == @" + queryParas.Count();
                        queryParas.Add(Convert.ToInt32(CheckStates.UnApproved).ToString());//并且未提交的或者审核未通过的，把审核未通过的也算在批量审核内
                        filterString += ")";
                    }
                    else
                    {
                        filterString += "CHECKSTATE == @" + queryParas.Count();
                        queryParas.Add(CheckState);
                    }
                }
            }
            else
            {
                SetFilterWithflow("ADDSUMID", "T_HR_EMPLOYEEADDSUM", userID, ref CheckState, ref  filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }

            //IQueryable<T_HR_EMPLOYEEADDSUM> ents = DataContext.T_HR_EMPLOYEEADDSUM;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            int t = 0;
            if (starttime != dtNull && endtime != dtNull)
            {
                for (int i = starttime.Year; i <= endtime.Year; i++)
                {
                    year = i.ToString();
                    if (t == 0)                                                                                              //起始段
                    {
                        if (starttime.Year == endtime.Year)
                        {
                            ents = ents.Where(m => m.DEALYEAR == year);
                            if (starttime.Month == endtime.Month)                                                            //同年同月
                            {
                                string month = starttime.Month.ToString();
                                ents = ents.Where(m => m.DEALMONTH == month);
                                break;
                            }
                            else
                            {
                                bool temp = false;
                                for (int j = starttime.Month; j <= endtime.Month; j++)                                        //同年不同月
                                {
                                    if (temp)
                                    {
                                        filter2 += " or ";
                                    }
                                    string month = j.ToString();
                                    filter2 += " DEALMONTH==@" + queryParasDt.Count().ToString();
                                    queryParasDt.Add(month);
                                    temp = true;
                                }
                                ents = ents.Where(filter2, queryParasDt.ToArray());
                                break;
                            }
                        }
                        else                                                                                                  //不同年月
                        {
                            bool temp = false;
                            for (int j = starttime.Month; j <= 12; j++)
                            {
                                if (temp)
                                {
                                    filter2 += " or ";
                                }
                                string month = j.ToString();
                                filter2 += " DEALMONTH==@" + queryParasDt.Count().ToString();
                                queryParasDt.Add(month);
                                filter2 += " and DEALYEAR==@" + queryParasDt.Count().ToString();
                                queryParasDt.Add(year);
                                temp = true;
                            }
                        }
                    }
                    else if (Convert.ToInt32(year) == endtime.Year)                                                             //末段
                    {
                        for (int j = 1; j <= endtime.Month; j++)
                        {
                            string month = j.ToString();
                            filter2 += " or DEALYEAR==@" + queryParasDt.Count().ToString();
                            queryParasDt.Add(year);
                            filter2 += " and  DEALMONTH==@" + queryParasDt.Count().ToString();
                            queryParasDt.Add(month);
                        }
                        ents = ents.Where(filter2, queryParasDt.ToArray());
                        break;
                    }
                    else                                                                                                         //中段
                    {
                        filter2 += " or  DEALYEAR==@" + queryParasDt.Count().ToString();
                        queryParasDt.Add(year);
                    }

                    t++;
                }
            }

            ents = Utility.Pager<T_HR_EMPLOYEEADDSUM>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }


        /// <summary>
        /// 员工加扣款过滤
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEEADDSUM> GetAddSumFilter(int orgtype, string orgid)
        {
            IQueryable<T_HR_EMPLOYEEADDSUM> ents = dal.GetObjects();
            switch (orgtype)
            {
                case 0:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>().Include("T_HR_EMPLOYEEADDSUMBATCH")
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           where d.T_HR_COMPANY.COMPANYID == orgid && b.ISAGENCY == "0"
                           select a;
                    break;
                case 1:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>().Include("T_HR_EMPLOYEEADDSUMBATCH")
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           where d.DEPARTMENTID == orgid && b.ISAGENCY == "0"
                           select a;
                    break;
                case 2:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>().Include("T_HR_EMPLOYEEADDSUMBATCH")
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           where c.POSTID == orgid && b.ISAGENCY == "0"
                           select a;
                    break;
            }
            return ents;
        }

        /// <summary>
        /// 员工过滤
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEE> GetEmployeeFilter(int orgtype, string orgid, DateTime starttime, DateTime endtime)
        {
            IQueryable<T_HR_EMPLOYEE> ents = dal.GetObjects<T_HR_EMPLOYEE>();
            switch (orgtype)
            {
                case -1:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                           where a.EMPLOYEESTATE == "2"
                           select a;
                    break;
                case 0:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join e in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.T_HR_COMPANY.COMPANYID == orgid && e.CHECKSTATE == "2" && e.STOPPAYMENTDATE >= starttime && e.STOPPAYMENTDATE <= endtime
                           select a;
                    break;
                case 1:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join e in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.DEPARTMENTID == orgid && e.CHECKSTATE == "2" && e.STOPPAYMENTDATE >= starttime && e.STOPPAYMENTDATE <= endtime
                           select a;
                    break;
                case 2:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join e in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on a.EMPLOYEEID equals e.EMPLOYEEID
                           where c.POSTID == orgid && e.CHECKSTATE == "2" && e.STOPPAYMENTDATE >= starttime && e.STOPPAYMENTDATE <= endtime
                           select a;
                    break;
            }
            return ents;
        }

        /// <summary>
        /// 获取离职人员信息
        /// </summary>
        /// <param name="pageIndex">当前页 </param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数 </param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        public IQueryable<V_RESIGN> GetResign(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, string userID, int orgtype, string orgid)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            var ents = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                       join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                       join d in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals d.POSTID
                       join e in dal.GetObjects<T_HR_DEPARTMENT>() on d.T_HR_DEPARTMENT.DEPARTMENTID equals e.DEPARTMENTID
                       join f in dal.GetObjects<T_HR_COMPANY>() on e.T_HR_COMPANY.COMPANYID equals f.COMPANYID
                       join g in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on b.EMPLOYEEPOSTID equals g.EMPLOYEEPOSTID
                       where b.ISAGENCY == "0"
                       select new V_RESIGN
                       {
                           EMPLOYEEID = c.EMPLOYEEID,
                           EMPLOYEENAME = c.EMPLOYEECNAME,
                           EMPLOYEECODE = c.EMPLOYEECODE,
                           OWNERCOMPANYID = f.COMPANYID,
                           CompanyName = f.CNAME,
                           OWNERDEPARTMENTID = e.DEPARTMENTID,
                           DepartmentName = e.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                           OWNERPOSTID = d.POSTID,
                           OWNERID = c.EMPLOYEEID,
                           PostName = d.T_HR_POSTDICTIONARY.POSTNAME,
                           LeftDate = g.LEFTOFFICEDATE,
                           EmployeePostID = g.EMPLOYEEPOSTID,
                           CREATEUSERID = c.CREATEUSERID
                       };
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_RESIGN>(ents, pageIndex, pageSize, ref pageCount);

            return ents;

        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                var addsum = (from c in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                              where c.ADDSUMID == EntityKeyValue
                              select c).FirstOrDefault();
                if (addsum != null)
                {
                    addsum.CHECKSTATE = CheckState;
                    addsum.UPDATEDATE = DateTime.Now;
                    dal.UpdateFromContext(addsum);
                    i = dal.SaveContextChanges();
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }

        /// <summary>
        /// 导入员工社保记录并返回显示
        /// </summary>
        /// <param name="strPath">路径</param>
        /// <param name="paras">参数字典</param>
        /// <param name="strMsg">返回的参数</param>
        public List<T_HR_EMPLOYEEADDSUM> ImportEmployeeAddSumFromExcelForShow(string strPath, Dictionary<string, string> paras, ref string strMsg, bool IsPreview)
        {
           
            try
            {
                return ImportDataFromFile(strPath, paras, ref strMsg,IsPreview);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ImportPensionByImportExcel:" + ex.Message);

            }
            return null;
        }


        /// <summary>
        /// 读取指定路径的Excel，将数据导入到DataTable内(依靠Office组件读取)
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        private List<T_HR_EMPLOYEEADDSUM> ImportDataFromFile(string strPath, Dictionary<string, string> paras, ref string strMsg, bool IsPreview)
        {
            List<T_HR_EMPLOYEEADDSUM> ListEmployeeAddSum = new List<T_HR_EMPLOYEEADDSUM>();

            Excel.Application xApp = new Excel.ApplicationClass();
            xApp.Visible = true;
            try
            {
                Excel.Workbook xBook = xApp.Workbooks._Open(strPath,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value
                , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                , Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                Excel.Worksheet xSheet = (Excel.Worksheet)xBook.Sheets[1];
                int iCount = xSheet.UsedRange.Rows.Count;
                if (iCount < 1)
                {
                    strMsg = "员工加扣款导入失败：无导入数据";
                    Tracer.Debug(strMsg);
                    return null;
                }

                for (int i = 0; i < iCount; i++)
                {
                    int iRowIndex = i + 2;  //Excel起始列从1开始计数，首列为标题列，因此数据列计数应先加2
                    if (iRowIndex > iCount) break;
                    Excel.Range rngCompany = (Excel.Range)xSheet.Cells[iRowIndex, 1];         //所属公司
                    Excel.Range rngDep = (Excel.Range)xSheet.Cells[iRowIndex, 2];         //所属部门
                    Excel.Range rngPost = (Excel.Range)xSheet.Cells[iRowIndex, 3];         //所属岗位
                    Excel.Range rngEmpName = (Excel.Range)xSheet.Cells[iRowIndex, 4];     //员工姓名
                    Excel.Range rngImpType = (Excel.Range)xSheet.Cells[iRowIndex, 5];     //处理类型
                    Excel.Range rngProjectName = (Excel.Range)xSheet.Cells[iRowIndex, 6];         //项目名
                    Excel.Range rngProjectValue = (Excel.Range)xSheet.Cells[iRowIndex, 7];         //项目金额
                    Excel.Range rngRemark = (Excel.Range)xSheet.Cells[iRowIndex, 8];        //备注


                    T_HR_EMPLOYEEADDSUM employeeAddSum = new T_HR_EMPLOYEEADDSUM();
                    employeeAddSum.ADDSUMID = Guid.NewGuid().ToString();
                    //公司
                    T_HR_COMPANY com = null;
                    if (rngCompany.Text != null)
                    {
                        string cname = rngCompany.Text.ToString().Trim();
                        com = (from ent in dal.GetObjects<T_HR_COMPANY>()
                                   where ent.CNAME == cname
                                   select ent).FirstOrDefault();
                        if (com != null)
                        {
                            employeeAddSum.OWNERCOMPANYID = com.COMPANYID;
                        }
                        else
                        {
                            strMsg = "员工加扣款导入失败：行号：" + iRowIndex + " 列号：1 导入的公司系统中不存在";
                            Tracer.Debug(strMsg);
                            break;
                        }
                    }
                    else
                    {
                        strMsg="员工加扣款导入失败：行号：" + iRowIndex+" 列号：1 值为空";
                        Tracer.Debug(strMsg);
                        break;
                    }

                    //部门
                    T_HR_DEPARTMENT dep = null; 
                    if (rngDep.Text != null)
                    {
                        string depname = rngDep.Text.ToString().Trim();
                        dep = (from ent in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY")
                                   where ent.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME == depname
                                   && ent.T_HR_COMPANY.COMPANYID==com.COMPANYID
                                   select ent).FirstOrDefault();
                        if (dep != null)
                        {
                            employeeAddSum.OWNERDEPARTMENTID = dep.DEPARTMENTID;
                        }
                        else
                        {
                            strMsg="员工加扣款导入失败：行号：" + iRowIndex + " 列号：2 导入的部门系统中不存在";
                            Tracer.Debug(strMsg);
                            break;
                        }
                    }
                    else
                    {
                        strMsg="员工加扣款导入失败：行号：" + iRowIndex + " 列号：2 值为空 部门名称为空";
                        Tracer.Debug(strMsg);
                        break;
                    }
                    //岗位
                    T_HR_POST pos = null;
                    if (rngPost.Text != null)
                    {
                        string postname = rngPost.Text.ToString().Trim();
                        pos = (from ent in dal.GetObjects<T_HR_POST>().Include("T_HR_POSTDICTIONARY")
                                   where ent.T_HR_POSTDICTIONARY.POSTNAME == postname
                                   && ent.T_HR_DEPARTMENT.DEPARTMENTID==dep.DEPARTMENTID
                                   && ent.COMPANYID==com.COMPANYID
                                   select ent).FirstOrDefault();
                        if (pos != null)
                        {
                            employeeAddSum.OWNERPOSTID = pos.POSTID;
                        }
                        else
                        {
                            strMsg="员工加扣款导入失败：行号：" + iRowIndex + " 列号：3 导入的岗位系统不存在";
                            Tracer.Debug(strMsg);
                            break;
                        }
                    }
                    else
                    {
                        strMsg="员工加扣款导入失败：行号：" + iRowIndex + " 列号：3 值为空 部门名称为空";
                        Tracer.Debug(strMsg);
                        break;
                    }

                    //员工
                    T_HR_EMPLOYEE employee = null;
                    if (rngEmpName.Text != null)
                    {
                        string employeeName = rngEmpName.Text.ToString().Trim();

                        employee = (from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                    join empPost in dal.GetObjects<T_HR_EMPLOYEEPOST>() on ent.EMPLOYEEID equals empPost.T_HR_EMPLOYEE.EMPLOYEEID
                                    join post in dal.GetObjects<T_HR_POST>() on empPost.T_HR_POST.POSTID equals post.POSTID
                                    where ent.EMPLOYEECNAME == employeeName && post.POSTID == pos.POSTID
                               select ent).FirstOrDefault();
                        if (employee != null)
                        {
                            employeeAddSum.OWNERID = employee.EMPLOYEEID;
                            employeeAddSum.EMPLOYEEID = employee.EMPLOYEEID;
                            employeeAddSum.EMPLOYEENAME = employee.EMPLOYEECNAME;
                        }
                        else
                        {
                            strMsg="员工加扣款导入失败：行号：" + iRowIndex + " 列号：4 导入的员工系统不存在";
                            Tracer.Debug(strMsg);
                            break;
                        }
                    }
                    else
                    {
                       strMsg="员工加扣款导入失败：行号：" + iRowIndex + " 列号：4 值为空 员工名称为空";
                       Tracer.Debug(strMsg);
                        break;
                    }
                    //处理类型：
                    if (rngImpType.Text != null)
                    {
                        switch(rngImpType.Text.ToString().Trim())
                        {
                                  
                            case "员工加扣款":
                                employeeAddSum.SYSTEMTYPE="0";
                                break;
                                  case "员工代扣款":
                                 employeeAddSum.SYSTEMTYPE="1";
                                break;
                                  case "绩效奖金":
                                 employeeAddSum.SYSTEMTYPE="2";
                                break;
                                 case "其他......":
                                 employeeAddSum.SYSTEMTYPE="3";
                                break;
                        }                  
                    }
                    else
                    {
                        strMsg = "员工加扣款导入失败：行号：" + iRowIndex + " 列号：5 值为空 处理类型为空";
                        Tracer.Debug(strMsg);
                        break;
                    }
                    //项目名
                    if (rngProjectName.Text != null)
                    {
                        employeeAddSum.PROJECTNAME = rngProjectName.Text.ToString().Trim();
                    }
                    else
                    {
                        strMsg="员工加扣款导入失败：行号：" + iRowIndex + " 列号：6 值为空 项目名称为空";
                        Tracer.Debug(strMsg);
                        break;
                    }

                    //项目金额
                    if (rngProjectValue.Text != null)
                    {
                        employeeAddSum.PROJECTMONEY =decimal.Parse(rngProjectValue.Text.ToString().Trim());
                    }
                    else
                    {
                        strMsg = "员工加扣款导入失败：行号：" + iRowIndex + " 列号：7 值为空 项目金额为空";
                        Tracer.Debug(strMsg);
                        break;
                    }

                    //项目备注
                    if (rngRemark.Text != null)
                    {
                        employeeAddSum.REMARK = rngRemark.Text.ToString().Trim();
                    }
                    else
                    {
                         strMsg = "员工加扣款导入失败：行号：" + iRowIndex + " 列号：8 值为空 项目备注为空";
                         Tracer.Debug(strMsg);
                        break;
                    }
                    employeeAddSum.DEALYEAR = paras["YEAR"].ToString();
                    employeeAddSum.DEALMONTH = paras["MONTH"].ToString();

                    employeeAddSum.CREATEUSERID = paras["CREATEUSERID"].ToString();
                    employeeAddSum.CREATEPOSTID = paras["CREATEPOSTID"].ToString();
                    employeeAddSum.CREATEDEPARTMENTID = paras["CREATEDEPARTMENTID"].ToString();
                    employeeAddSum.CREATECOMPANYID = paras["CREATECOMPANYID"].ToString();


                    employeeAddSum.CREATEDATE = DateTime.Now;
                    employeeAddSum.UPDATEDATE = DateTime.Now;
                    employeeAddSum.CHECKSTATE = "0";//未提交
                    if (IsPreview == false)
                    {
                       dal.Add(employeeAddSum);
                    }
                    else
                    {
                        ListEmployeeAddSum.Add(employeeAddSum);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
            finally
            {
                xApp.Quit();
            }

            return ListEmployeeAddSum;
        }


        /// <summary>
        /// 构建导入的DataTable
        /// </summary>
        /// <returns></returns>
        private DataTable MakeTableForImport()
        {
            T_HR_EMPLOYEEADDSUM addSum=new T_HR_EMPLOYEEADDSUM();
            
            DataTable dt = new DataTable();

            DataColumn colDepartment = new DataColumn();
            colDepartment.ColumnName = Utility.GetResourceStr(addSum.OWNERCOMPANYID.GetType().Name);
            colDepartment.DataType = typeof(string);
            dt.Columns.Add(colDepartment);

            DataColumn colEmployeename = new DataColumn();
            colEmployeename.ColumnName = Utility.GetResourceStr(addSum.OWNERDEPARTMENTID.GetType().Name);
            colEmployeename.DataType = typeof(string);
            dt.Columns.Add(colEmployeename);

            DataColumn colFingerprintid = new DataColumn();
            colFingerprintid.ColumnName = Utility.GetResourceStr(addSum.OWNERPOSTID.GetType().Name);
            colFingerprintid.DataType = typeof(string);
            dt.Columns.Add(colFingerprintid);

            DataColumn colPunchdate = new DataColumn();
            colPunchdate.ColumnName = Utility.GetResourceStr(addSum.OWNERID.GetType().Name);
            colPunchdate.DataType = typeof(string);
            dt.Columns.Add(colPunchdate);

            DataColumn colClockID = new DataColumn();
            colClockID.ColumnName = Utility.GetResourceStr(addSum.PROJECTNAME.GetType().Name);
            colClockID.DataType = typeof(string);
            dt.Columns.Add(colClockID);

            DataColumn colPROJECTMONEY = new DataColumn();
            colPROJECTMONEY.ColumnName = Utility.GetResourceStr(addSum.PROJECTMONEY.GetType().Name);
            colPROJECTMONEY.DataType = typeof(string);
            dt.Columns.Add(colPROJECTMONEY);

            DataColumn colREMARK = new DataColumn();
            colREMARK.ColumnName = Utility.GetResourceStr(addSum.REMARK.GetType().Name);
            colREMARK.DataType = typeof(string);
            dt.Columns.Add(colREMARK);

            return dt;
        }
    }
}
