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
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Data;
using BLLCommonServices = SMT.SaaS.BLLCommonServices;
using System.Data.OracleClient;
using SMT.Foundation.Log;




namespace SMT.HRM.BLL
{

    public class EmployeeSalaryRecordBLL : BaseBll<T_HR_EMPLOYEESALARYRECORD>, ILookupEntity,IDisposable
    {
        protected int nsize = 2;
        protected bool getresult = false;
        protected bool IsComputer = false;
        protected string[] construe = new string[9];
        protected decimal nodeductday = 0;
        protected decimal calcheck = 0;
        protected Dictionary<string, string> getCaches = new Dictionary<string, string>();
        protected Dictionary<string, string> salaryItemCaches = new Dictionary<string, string>();
        private List<SalaryItemsOrders> SalaryItemsOrderCache = new List<SalaryItemsOrders>();
        private int iGetItemValue = 0;
        private T_HR_ATTENDANCESOLUTION AttendsolutionForSalary;
        protected StringBuilder sbRemark = new StringBuilder();
        protected List<T_HR_EMPLOYEESALARYRECORD> recordAdd = new List<T_HR_EMPLOYEESALARYRECORD>();
        protected List<T_HR_EMPLOYEESALARYRECORD> recordUpdate = new List<T_HR_EMPLOYEESALARYRECORD>();
        protected Dictionary<object, object> GetInfor = new Dictionary<object, object>();
        protected SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient FBSclient = new SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient();
        private T_HR_SALARYARCHIVE archiveForAreaAllowrance;

        private OracleCommand myOraclecommad;
        private static string connectString = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"].ToString();
        public EmployeeSalaryRecordBLL()
        {
            OracleConnection myOracleConn = new OracleConnection(connectString);
            if (myOraclecommad == null)
            {
                myOraclecommad = new OracleCommand();
                myOraclecommad.Connection = myOracleConn;
            }
        }


        public void EmployeeSalaryRecordAdd(T_HR_EMPLOYEESALARYRECORD obj)
        {
            T_HR_EMPLOYEESALARYRECORD ent = new T_HR_EMPLOYEESALARYRECORD();
            Utility.CloneEntity<T_HR_EMPLOYEESALARYRECORD>(obj, ent);
            dal.Add(ent);
            // BLLCommonServices.Utility.SubmitMyRecord<T_HR_EMPLOYEESALARYRECORD>(ent);
        }

        public void EmployeeSalaryRecordUpdate(T_HR_EMPLOYEESALARYRECORD obj)
        {

            var ent = from a in dal.GetTable()
                      where a.EMPLOYEESALARYRECORDID == obj.EMPLOYEESALARYRECORDID
                      select a;
            if (ent.Count() > 0)
            {
                T_HR_EMPLOYEESALARYRECORD tmpEnt = ent.FirstOrDefault();

                Utility.CloneEntity<T_HR_EMPLOYEESALARYRECORD>(obj, tmpEnt);

                if (obj.T_HR_SALARYRECORDBATCH != null)
                {
                    tmpEnt.T_HR_SALARYRECORDBATCHReference.EntityKey =
                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYRECORDBATCH", "MONTHLYBATCHID", obj.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID);
                }

                dal.Update(tmpEnt);
            }

        }

        /// <summary>
        /// 获取薪资对比记录
        /// </summary>
        /// <param name="SentEmployeeSalaryRecordID"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEESALARYRECORD> SalaryContrast(string SentEmployeeSalaryRecordID)
        {
            List<T_HR_EMPLOYEESALARYRECORD> entList = new List<T_HR_EMPLOYEESALARYRECORD>();
            var enta = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       where a.EMPLOYEESALARYRECORDID == SentEmployeeSalaryRecordID
                       select a;
            if (enta.Count() > 0)
            {
                string salaryyear = string.Empty, salarymonth = string.Empty;
                T_HR_EMPLOYEESALARYRECORD ent = enta.FirstOrDefault();
                entList.Add(ent);
                switch (ent.SALARYMONTH)
                {
                    case "1":
                        salaryyear = (Convert.ToInt32(ent.SALARYYEAR) - 1).ToString();
                        salarymonth = "12";
                        break;
                    default:
                        salaryyear = ent.SALARYYEAR;
                        salarymonth = (Convert.ToInt32(ent.SALARYMONTH) - 1).ToString();
                        break;
                }
                var entb = from b in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           where b.SALARYYEAR == salaryyear && b.SALARYMONTH == salarymonth
                           select b;
                if (entb.Count() > 0)
                    entList.Add(entb.FirstOrDefault());
            }
            return entList;
        }

        /// <summary>
        /// 获取薪资对比记录(获取所有)
        /// </summary>
        /// <param name="SentEmployeeSalaryRecordID"></param>
        /// <param name="nowData"></param>
        /// <param name="lastData"></param>
        /// <param name="titleData"></param>
        /// <returns></returns>
        public void SalaryContrast(string SentEmployeeSalaryRecordID, out List<string> nowData, out List<string> lastData, out List<string> titleData)
        {
            List<string> temp = new List<string>();
            nowData = new List<string>();
            lastData = new List<string>();
            titleData = new List<string>();

            var enta = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       where a.EMPLOYEESALARYRECORDID == SentEmployeeSalaryRecordID
                       select a;
            if (enta.Count() > 0)
            {
                string salaryyear = string.Empty, salarymonth = string.Empty;
                T_HR_EMPLOYEESALARYRECORD ent = enta.FirstOrDefault();
                nowData.Add(AES.AESDecrypt(ent.ACTUALLYPAY));

                var entaItem = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                               join b in dal.GetObjects<T_HR_SALARYITEM>() on a.SALARYITEMID equals b.SALARYITEMID
                               where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == SentEmployeeSalaryRecordID
                               orderby a.ORDERNUMBER
                               select new
                               {
                                   a.SUM,
                                   b.SALARYITEMNAME
                               };
                if (entaItem.Count() > 0)
                {
                    foreach (var e in entaItem)
                    {
                        nowData.Add(AES.AESDecrypt(e.SUM));
                        titleData.Add(e.SALARYITEMNAME);
                    }
                }

                switch (ent.SALARYMONTH)
                {
                    case "1":
                        salaryyear = (Convert.ToInt32(ent.SALARYYEAR) - 1).ToString();
                        salarymonth = "12";
                        break;
                    default:
                        salaryyear = ent.SALARYYEAR;
                        salarymonth = (Convert.ToInt32(ent.SALARYMONTH) - 1).ToString();
                        break;
                }
                var entb = from c in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           where c.SALARYYEAR == salaryyear && c.SALARYMONTH == salarymonth && c.EMPLOYEEID == ent.EMPLOYEEID
                           select c;
                if (entb.Count() > 0)
                {
                    T_HR_EMPLOYEESALARYRECORD templast = entb.FirstOrDefault();
                    lastData.Add(AES.AESDecrypt(templast.ACTUALLYPAY));

                    var entbItem = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                                   join b in dal.GetObjects<T_HR_SALARYITEM>() on a.SALARYITEMID equals b.SALARYITEMID
                                   where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == templast.EMPLOYEESALARYRECORDID
                                   orderby a.ORDERNUMBER
                                   select new
                                   {
                                       a.SUM
                                   };
                    if (entbItem.Count() > 0)
                    {
                        foreach (var e in entbItem)
                        {
                            lastData.Add(AES.AESDecrypt(e.SUM));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 薪资记录批量删除
        /// </summary>
        /// <param name="EmployeeSalaryRecordIDs">薪资记录IDS</param>
        /// <returns></returns>
        public int EmployeeSalaryRecordOrItemDelete(string[] EmployeeSalaryRecordIDs)
        {
            EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL();
            foreach (string id in EmployeeSalaryRecordIDs)
            {
                var ents = from e in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           where e.EMPLOYEESALARYRECORDID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    //if(bll.EmployeeSalaryRecordItemDelete(ent.EMPLOYEESALARYRECORDID)>0)
                    try
                    {
                        bll.EmployeeSalaryRecordItemDelete(ent.EMPLOYEESALARYRECORDID);
                    }
                    catch (Exception ex)
                    {
                        SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                    }
                    dal.DeleteFromContext(ent);

                    //    BLLCommonServices.Utility.SubmitMyRecord<T_HR_EMPLOYEESALARYRECORD>(ent);

                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();
        }
        public int EmployeeSalaryRecordDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           where e.EMPLOYEESALARYRECORDID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    //BLLCommonServices.Utility.SubmitMyRecord<T_HR_EMPLOYEESALARYRECORD>(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();
        }
        public EntityObject[] GetLookupData(Dictionary<string, string> args)
        {
            IQueryable<T_HR_EMPLOYEESALARYRECORD> ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                                         select a;
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       select a;
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        public T_HR_EMPLOYEESALARYRECORD GetEmployeeSalaryRecordByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>().Include("T_HR_SALARYRECORDBATCH")
                       where o.EMPLOYEESALARYRECORDID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询-新查询NEW
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEESALARYRECORD> GetAutoEmployeeSalaryRecordPagings(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            string year = starttime.Year.ToString();
            string month = starttime.Month.ToString();


            List<string> queryParasDt = new List<string>();
            DateTime dtNull = Convert.ToDateTime("0001-1-1 0:00:00");

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            List<T_HR_EMPLOYEESALARYRECORD> ent = new List<T_HR_EMPLOYEESALARYRECORD>();
            //PaymentBLL pb = new PaymentBLL();
            IQueryable<T_HR_EMPLOYEESALARYRECORD> ents = dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>();
            switch (orgtype)
            {
                case 0:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.T_HR_COMPANY.COMPANYID == orgid && b.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                           select a;
                    break;
                case 1:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.DEPARTMENTID == orgid && b.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                           select a;
                    break;
                case 2:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where c.POSTID == orgid && b.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                           select a;
                    break;
            }



            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");

                if (!string.IsNullOrEmpty(strCheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
            }
            else
            {
                SetFilterWithflow("EMPLOYEESALARYRECORDID", "T_HR_EMPLOYEESALARYRECORD", userID, ref strCheckState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }


            if (!string.IsNullOrEmpty(filterString))
            {
                //ents = ents.Where(filterString, paras.ToArray());
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.Where(m => m.SALARYYEAR == year && m.SALARYMONTH == month);
            ents = ents.Distinct();
            ents = ents.OrderBy(c=>c.EMPLOYEENAME);

            ///
            //var en = ents.GroupBy(y => y.EMPLOYEESALARYRECORDID).Select(g => new { group = g.Key, groupcontent = g });
            //foreach (var v in en)
            //{
            //    ent.Add(v.groupcontent.FirstOrDefault());
            //}
            //ents = ent.AsQueryable();
            //var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
            //           where a.OWNERCOMPANYID == orgid
            //           select a;

           

            //int t = 0;
            //if (starttime != dtNull && endtime != dtNull)
            //{
            //    for (int i = starttime.Year; i <= endtime.Year; i++)
            //    {
            //        year = i.ToString();
            //        if (t == 0)                                                                                              //起始段
            //        {
            //            if (starttime.Year == endtime.Year)
            //            {
            //                ents = ents.Where(m => m.SALARYYEAR == year);
            //                if (starttime.Month == endtime.Month)                                                            //同年同月
            //                {
            //                    string month = starttime.Month.ToString();
            //                    ents = ents.Where(m => m.SALARYMONTH == month);
            //                    break;
            //                }
            //                else
            //                {
            //                    bool temp = false;
            //                    for (int j = starttime.Month; j <= endtime.Month; j++)                                        //同年不同月
            //                    {
            //                        if (temp)
            //                        {
            //                            filter2 += " or ";
            //                        }
            //                        string month = j.ToString();
            //                        filter2 += " SALARYMONTH==@" + queryParasDt.Count().ToString();
            //                        queryParasDt.Add(month);
            //                        temp = true;
            //                    }
            //                    ents = ents.Where(filter2, queryParasDt.ToArray());
            //                    break;
            //                }
            //            }
            //            else                                                                                                  //不同年月
            //            {
            //                bool temp = false;
            //                for (int j = starttime.Month; j <= 12; j++)
            //                {
            //                    if (temp)
            //                    {
            //                        filter2 += " or ";
            //                    }
            //                    string month = j.ToString();
            //                    filter2 += " SALARYMONTH==@" + queryParasDt.Count().ToString();
            //                    queryParasDt.Add(month);
            //                    filter2 += " and SALARYYEAR==@" + queryParasDt.Count().ToString();
            //                    queryParasDt.Add(year);
            //                    temp = true;
            //                }
            //            }
            //        }
            //        else if (Convert.ToInt32(year) == endtime.Year)                                                             //末段
            //        {
            //            for (int j = 1; j <= endtime.Month; j++)
            //            {
            //                string month = j.ToString();
            //                filter2 += " or SALARYYEAR==@" + queryParasDt.Count().ToString();
            //                queryParasDt.Add(year);
            //                filter2 += " and  SALARYMONTH==@" + queryParasDt.Count().ToString();
            //                queryParasDt.Add(month);
            //            }
            //            ents = ents.Where(filter2, queryParasDt.ToArray());
            //            break;
            //        }
            //        else                                                                                                         //中段
            //        {
            //            filter2 += " or  SALARYYEAR==@" + queryParasDt.Count().ToString();
            //            queryParasDt.Add(year);
            //        }

            //        t++;
            //    }
            //}
            //ents = ents.OrderBy(m => m.CREATEDATE);
            //ents = ents.OrderBy(m => m.SALARYYEAR);
            //ents = ents.OrderBy(m => m.SALARYMONTH);

            ents = Utility.Pager<T_HR_EMPLOYEESALARYRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询(用于同表菜单不同)
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="MenuSign">菜单标识</param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEESALARYRECORD> GetAutoEmployeeSalaryRecordPagings(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID, string MenuSign)
        {
            string year = string.Empty;
            string filter2 = "";
            List<string> queryParasDt = new List<string>();
            DateTime dtNull = Convert.ToDateTime("0001-1-1 0:00:00");

            List<object> queryParas = new List<object>();
            List<T_HR_EMPLOYEESALARYRECORD> ent = new List<T_HR_EMPLOYEESALARYRECORD>();
            PaymentBLL pb = new PaymentBLL();
            IQueryable<T_HR_EMPLOYEESALARYRECORD> ents = pb.GetResultset(orgtype, orgid);
            var en = ents.GroupBy(y => y.EMPLOYEESALARYRECORDID).Select(g => new { group = g.Key, groupcontent = g });
            foreach (var v in en)
            {
                ent.Add(v.groupcontent.FirstOrDefault());
            }
            ents = ent.AsQueryable();

            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, MenuSign);

            //SetFilterWithflow("EMPLOYEESALARYRECORDID", "T_HR_EMPLOYEESALARYRECORD", userID, ref strCheckState, ref filterString, ref queryParas);


            if (!string.IsNullOrEmpty(strCheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                filterString += " CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(strCheckState);
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                //ents = ents.Where(filterString, paras.ToArray());
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
                            ents = ents.Where(m => m.SALARYYEAR == year);
                            if (starttime.Month == endtime.Month)                                                            //同年同月
                            {
                                string month = starttime.Month.ToString();
                                ents = ents.Where(m => m.SALARYMONTH == month);
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
                                    filter2 += " SALARYMONTH==@" + queryParasDt.Count().ToString();
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
                                filter2 += " SALARYMONTH==@" + queryParasDt.Count().ToString();
                                queryParasDt.Add(month);
                                filter2 += " and SALARYYEAR==@" + queryParasDt.Count().ToString();
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
                            filter2 += " or SALARYYEAR==@" + queryParasDt.Count().ToString();
                            queryParasDt.Add(year);
                            filter2 += " and  SALARYMONTH==@" + queryParasDt.Count().ToString();
                            queryParasDt.Add(month);
                        }
                        ents = ents.Where(filter2, queryParasDt.ToArray());
                        break;
                    }
                    else                                                                                                         //中段
                    {
                        filter2 += " or  SALARYYEAR==@" + queryParasDt.Count().ToString();
                        queryParasDt.Add(year);
                    }

                    t++;
                }
            }
            ents = ents.OrderBy(m => m.CREATEDATE);
            ents = ents.OrderBy(m => m.SALARYYEAR);
            ents = ents.OrderBy(m => m.SALARYMONTH);

            ents = Utility.Pager<T_HR_EMPLOYEESALARYRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }


        /// <summary>
        /// 用于实体Grid中显示数据的分页查询-新查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEESALARYRECORD> NewQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, string strCheckState, string userID)
        {

            var entsmaster = dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>();
            foreach (var ent in entsmaster)
            {
                var entschildren = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                                   where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == ent.EMPLOYEESALARYRECORDID
                                   select a;
                //组装成显示列表
            }

            string year = string.Empty;
            string filter2 = "";
            List<object> queryParas = new List<object>();
            List<string> queryParasDt = new List<string>();
            DateTime dtNull = Convert.ToDateTime("0001-1-1 0:00:00");
            queryParas.AddRange(paras);

            List<object> objArgs = new List<object>();
            objArgs.Add(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");

            //SetFilterWithflow("EMPLOYEESALARYRECORDID", "T_HR_EMPLOYEESALARYRECORD", userID, ref strCheckState, ref filterString, ref objArgs);

            SetFilterWithflow("EMPLOYEESALARYRECORDID", "T_HR_EMPLOYEESALARYRECORD", userID, ref strCheckState, ref filterString, ref queryParas);

            if (!string.IsNullOrEmpty(strCheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                filterString += " CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(strCheckState);
            }


            IQueryable<T_HR_EMPLOYEESALARYRECORD> ents = dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>();

            #region

            //if (!string.IsNullOrEmpty(filterString))
            //{
            //    ents = ents.Where(filterString, queryParas.ToArray());
            //}

            //int t = 0;
            //if (starttime != dtNull && endtime != dtNull)
            //{
            //    for (int i = starttime.Year; i <= endtime.Year; i++)
            //    {
            //        year = i.ToString();
            //        if (t == 0)                                                                                              //起始段
            //        {
            //            if (starttime.Year == endtime.Year)
            //            {
            //                ents = ents.Where(m => m.SALARYYEAR == year);
            //                if (starttime.Month == endtime.Month)                                                            //同年同月
            //                {
            //                    string month = starttime.Month.ToString();
            //                    ents = ents.Where(m => m.SALARYMONTH == month);
            //                    break;
            //                }
            //                else
            //                {
            //                    bool temp = false;
            //                    for (int j = starttime.Month; j <= endtime.Month; j++)                                        //同年不同月
            //                    {
            //                        if (temp)
            //                        {
            //                            filter2 += " or ";
            //                        }
            //                        string month = j.ToString();
            //                        filter2 += " SALARYMONTH==@" + queryParasDt.Count().ToString();
            //                        queryParasDt.Add(month);
            //                        temp = true;
            //                    }
            //                    ents = ents.Where(filter2, queryParasDt.ToArray());
            //                    break;
            //                }
            //            }
            //            else                                                                                                  //不同年月
            //            {
            //                bool temp = false;
            //                for (int j = starttime.Month; j <= 12; j++)
            //                {
            //                    if (temp)
            //                    {
            //                        filter2 += " or ";
            //                    }
            //                    string month = j.ToString();
            //                    filter2 += " SALARYMONTH==@" + queryParasDt.Count().ToString();
            //                    queryParasDt.Add(month);
            //                    filter2 += " and SALARYYEAR==@" + queryParasDt.Count().ToString();
            //                    queryParasDt.Add(year);
            //                    temp = true;
            //                }
            //            }
            //        }
            //        else if (Convert.ToInt32(year) == endtime.Year)                                                             //末段
            //        {
            //            for (int j = 1; j <= endtime.Month; j++)
            //            {
            //                string month = j.ToString();
            //                filter2 += " or SALARYYEAR==@" + queryParasDt.Count().ToString();
            //                queryParasDt.Add(year);
            //                filter2 += " and  SALARYMONTH==@" + queryParasDt.Count().ToString();
            //                queryParasDt.Add(month);
            //            }
            //            ents = ents.Where(filter2, queryParasDt.ToArray());
            //            break;
            //        }
            //        else                                                                                                         //中段
            //        {
            //            filter2 += " or  SALARYYEAR==@" + queryParasDt.Count().ToString();
            //            queryParasDt.Add(year);
            //        }

            //        t++;
            //    }
            //}
            //ents = ents.OrderBy(m => m.CREATEDATE);
            //ents = ents.OrderBy(m => m.SALARYYEAR);
            //ents = ents.OrderBy(m => m.SALARYMONTH);

            //ents = Utility.Pager<T_HR_EMPLOYEESALARYRECORD>(ents, pageIndex, pageSize, ref pageCount);

            #endregion

            return ents;
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
        public IQueryable<T_HR_EMPLOYEESALARYRECORD> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, string strCheckState, string userID)
        {
            string year = string.Empty;
            string filter2 = "";
            List<object> queryParas = new List<object>();
            List<string> queryParasDt = new List<string>();
            DateTime dtNull = Convert.ToDateTime("0001-1-1 0:00:00");
            queryParas.AddRange(paras);

            List<object> objArgs = new List<object>();
            objArgs.Add(paras);

            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");

                if (!string.IsNullOrEmpty(strCheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
            }
            else
            {
                SetFilterWithflow("EMPLOYEESALARYRECORDID", "T_HR_EMPLOYEESALARYRECORD", userID, ref strCheckState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }


            IQueryable<T_HR_EMPLOYEESALARYRECORD> ents = dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }

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
                            ents = ents.Where(m => m.SALARYYEAR == year);
                            if (starttime.Month == endtime.Month)                                                            //同年同月
                            {
                                string month = starttime.Month.ToString();
                                ents = ents.Where(m => m.SALARYMONTH == month);
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
                                    filter2 += " SALARYMONTH==@" + queryParasDt.Count().ToString();
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
                                filter2 += " SALARYMONTH==@" + queryParasDt.Count().ToString();
                                queryParasDt.Add(month);
                                filter2 += " and SALARYYEAR==@" + queryParasDt.Count().ToString();
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
                            filter2 += " or SALARYYEAR==@" + queryParasDt.Count().ToString();
                            queryParasDt.Add(year);
                            filter2 += " and  SALARYMONTH==@" + queryParasDt.Count().ToString();
                            queryParasDt.Add(month);
                        }
                        ents = ents.Where(filter2, queryParasDt.ToArray());
                        break;
                    }
                    else                                                                                                         //中段
                    {
                        filter2 += " or  SALARYYEAR==@" + queryParasDt.Count().ToString();
                        queryParasDt.Add(year);
                    }

                    t++;
                }
            }
            ents = ents.OrderBy(m => m.CREATEDATE);
            ents = ents.OrderBy(m => m.SALARYYEAR);
            ents = ents.OrderBy(m => m.SALARYMONTH);

            ents = Utility.Pager<T_HR_EMPLOYEESALARYRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        public T_HR_EMPLOYEESALARYRECORD GetEmployeeSalaryRecord(string employeeID, string year, string month)
        {
            var ents = from r in dal.GetTable()
                       where r.EMPLOYEEID == employeeID && r.SALARYMONTH == month && r.SALARYYEAR == year
                       select r;

            T_HR_EMPLOYEESALARYRECORD record = ents.Count() > 0 ? ents.FirstOrDefault() : null;

            return record;
        }

        /// <summary>
        /// 预算验证
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回验证结果</returns>
        public List<string> FBStatistics(string employeeID, int year, int month)
        {
            string resultstr = string.Empty;//,string departmentID
            List<string> reslist = new List<string>();
            //FBServiceBLLWS.T_FB_BUDGETACCOUNT fblist;
            string strBalanceEmployeeID = string.Empty;
            SMT.SaaS.BLLCommonServices.FBServiceWS.T_FB_BUDGETACCOUNT fblist;
            V_RETURNFBI glist = GenerateSalaryRecord(10,string.Empty,string.Empty,strBalanceEmployeeID, 3, employeeID, year, month, true).FirstOrDefault();
            if (glist != null)
            {
                try
                {
                    //预算(现在只有人事的部门可以进行预算)传公司,反馈部门,公司ID
                    // fblist = FBSclient.FetchSalaryBudget(glist.COMPANYID, null).FirstOrDefault();
                    //fblist = FBSclient.FetchSalaryBudget(glist.COMPANYID, glist.DEPARTMENTID).FirstOrDefault();
                    fblist = FBSclient.FetchSalaryBudget(null, glist.DEPARTMENTID).FirstOrDefault();
                    if (fblist != null)
                    {
                        //if (glist.SALARYSUM > fblist.USABLEMONEY)
                        //{
                        resultstr = glist.DEPARTMENTNAME;
                        reslist.Add("false");
                        //}
                        //else
                        //{
                        //    reslist.Add("true");
                        //}
                        reslist.Add(fblist.USABLEMONEY.ToString());
                        reslist.Add(glist.DEPARTMENTID);
                        //reslist[1]=fblist.USABLEMONEY.ToString();
                        //reslist[2]=glist.DEPARTMENTID;
                    }
                    else
                    {
                        reslist.Add("false");
                        reslist.Add("SYSTEMERRORPLEASELINKDADMIN");
                        //reslist[1]="SYSTEMERRORPLEASELINKDADMIN";
                    }
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                    reslist.Add("false");
                    reslist.Add(ex.Message + " InnerException: " + ex.StackTrace);
                    resultstr = ex.Message + " InnerException: " + ex.StackTrace.ToString();
                }
            }
            else
            {
                reslist.Add("false");
                reslist[1] = "NODATA";
            }
            return reslist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectID"></param>
        /// <param name="dSum"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="userID"></param>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public List<string> FBStatisticsByChoose(int objectType, string objectID, decimal dSum, int year, int month, string userID, string departmentID)
        {
            string resultstr = string.Empty;
            List<string> reslist = new List<string>();
            SMT.SaaS.BLLCommonServices.FBServiceWS.T_FB_BUDGETACCOUNT fblist;
            SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL();
            V_RETURNFBI glist = bll.GetSalarySum(objectType, objectID, year, month, userID);
            if (dSum <= 0 || glist == null)
            {
                reslist.Add("false");
                reslist.Add("NODATA");
                return reslist;
            }

            if (glist.SALARYSUM == null)
            {
                reslist.Add("false");
                reslist.Add("NODATA");
                return reslist;
            }

            if (glist.SALARYSUM <= 0)
            {
                reslist.Add("false");
                reslist.Add("NODATA");
                return reslist;
            }


            try
            {
                //预算(现在只有人事的部门可以进行预算)传公司,反馈部门,公司ID
                fblist = FBSclient.FetchSalaryBudget(null, departmentID).FirstOrDefault();
                if (fblist != null)
                {
                    if (dSum > fblist.USABLEMONEY)
                    {
                        resultstr = fblist.OWNERDEPARTMENTID;
                        reslist.Add("false");
                    }
                    else
                    {
                        reslist.Add("true");
                    }
                    reslist.Add(fblist.USABLEMONEY.ToString());
                    reslist.Add(fblist.OWNERDEPARTMENTID);
                }
                else
                {
                    reslist.Add("false");
                    reslist.Add("SYSTEMERRORPLEASELINKDADMIN");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                reslist.Add("false");
                reslist.Add(ex.Message + " InnerException: " + ex.StackTrace);
                resultstr = ex.Message + " InnerException: " + ex.StackTrace.ToString();
            }

            return reslist;
        }

        public List<string> FBStatistics(int objectType, string objectID, int year, int month, string userID, string departmentID)
        {
            string resultstr = string.Empty;
            List<string> reslist = new List<string>();
            SMT.SaaS.BLLCommonServices.FBServiceWS.T_FB_BUDGETACCOUNT fblist;
            SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL();
            //List<V_RETURNFBI> glist = GenerateSalaryRecord(objectType, objectID, year, month, true);
            V_RETURNFBI glist = bll.GetSalarySum(objectType, objectID, year, month, userID);
            if (glist != null)
            {
                try
                {
                    //预算(现在只有人事的部门可以进行预算)传公司,反馈部门,公司ID
                    fblist = FBSclient.FetchSalaryBudget(null, departmentID).FirstOrDefault();
                    if (fblist != null)
                    {
                        if (glist.SALARYSUM > fblist.USABLEMONEY)
                        {
                            resultstr = fblist.OWNERDEPARTMENTID;
                            reslist.Add("false");
                        }
                        else
                        {
                            reslist.Add("true");
                        }
                        reslist.Add(fblist.USABLEMONEY.ToString());
                        reslist.Add(fblist.OWNERDEPARTMENTID);
                        //reslist[1] = fblist.USABLEMONEY.ToString();
                        //reslist[2] = fblist.OWNERDEPARTMENTID;
                    }
                    else
                    {
                        reslist.Add("false");
                        reslist.Add("SYSTEMERRORPLEASELINKDADMIN");
                        //reslist[1]="SYSTEMERRORPLEASELINKDADMIN";
                    }

                    #region ---
                    //foreach (V_RETURNFBI v in glist)
                    //{
                    //    fblist = FBSclient.FetchSalaryBudget(v.COMPANYID, v.DEPARTMENTID).FirstOrDefault();
                    //    if (fblist != null)
                    //    {
                    //        if (v.SALARYSUM > fblist.USABLEMONEY)
                    //        {
                    //            resultstr = v.DEPARTMENTNAME;
                    //            reslist.Add("false");
                    //        }
                    //        else
                    //        {
                    //            reslist.Add("true");
                    //        }
                    //        reslist.Add(fblist.USABLEMONEY.ToString());
                    //        reslist.Add(v.DEPARTMENTID);
                    //    }
                    //    else
                    //    {
                    //        reslist.Add("false");
                    //        reslist.Add("SYSTEMERRORPLEASELINKDADMIN");
                    //    }
                    //}
                    #endregion
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                    reslist.Add("false");
                    reslist.Add(ex.Message + " InnerException: " + ex.StackTrace);
                    resultstr = ex.Message + " InnerException: " + ex.StackTrace.ToString();
                }
            }
            else
            {
                reslist.Add("false");
                reslist.Add("NODATA");
            }
            return reslist;
        }

        public Dictionary<object, object> SalaryRecordAccount(int GernerateType, string GenerateEmployeePostid, string GenerateCompanyid,int objectType, string objectID, int year, int month, string construes, bool calType)
        {
            //统计数据的结果：NODATA无数据；ERROR有错误；OK成功的   calType,true,直接计算生成，false，只生成前检查
            string resultstr = string.Empty;
            construe = construes.Split(';');
            getresult = false;
            IsComputer = calType;
            calcheck = 0;
            string strBalanceEmployeeID = string.Empty;
            if (construe.Length > 0)
            {
                strBalanceEmployeeID = construe[0];
            }
            try
            {
                GenerateSalaryRecord(GernerateType,GenerateCompanyid, GenerateEmployeePostid, strBalanceEmployeeID, objectType, objectID, year, month, false);

            }
            catch (Exception ex)
            {
                resultstr = ex.Message.ToString();
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    SMT.Foundation.Log.Tracer.Debug(ex.StackTrace);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                }
                //throw ex;
            }
            if (!getresult)
            {
                GetInfor.Add("END", "NODATA");
            }
            else
            {
                if (GetInfor.Count() > 0)
                    GetInfor.Add("END", "ERROR");
                else if (calcheck == 0)
                    GetInfor.Add("END", "NODATAGENERATE");
                else
                    GetInfor.Add("END", "OK");
            }
            //}
            calcheck = 0;
            return GetInfor;

            #region --备用代码

            //如果是生成薪资
            //if (IsComputer)
            //{
            //    try
            //    {
            //        GenerateSalaryRecord(GernerateType,GenerateEmployeePostid,strBalanceEmployeeID, objectType, objectID, year, month, false);
            //        //Repayment(objectType, objectID, year, month);  //执行还款
            //        GetInfor.Add("END", "OK");
            //    }
            //    catch (Exception ex)
            //    {
            //        resultstr = ex.Message.ToString();
            //        GetInfor.Add("END", resultstr);
            //        SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
            //        if (ex.InnerException != null)
            //        {
            //            SMT.Foundation.Log.Tracer.Debug("701行" + ex.StackTrace);
            //        }
            //        else
            //        {
            //            SMT.Foundation.Log.Tracer.Debug("701行" + ex.Message + " InnerException: " + ex.StackTrace);
            //        }
            //        //throw ex;
            //    }
            //}
            //else
            //{
            //bool result = true;
            //List<FBServiceBLLWS.T_FB_BUDGETACCOUNT> fblist;                                                     
            //try
            //{
            //    List<V_RETURNFBI> glist = GenerateSalaryRecord(objectType, objectID, year, month, true);      //统计薪资
            //    if (glist.Count > 0)
            //    {
            //        if (objectType == 0)
            //            fblist = FBSclient.FetchSalaryBudget(glist.FirstOrDefault().COMPANYID, "").ToList();    //预算核算
            //        else
            //            fblist = FBSclient.FetchSalaryBudget(glist.FirstOrDefault().COMPANYID, glist.FirstOrDefault().DEPARTMENTID).ToList();
            //        foreach (V_RETURNFBI v in glist)
            //        {
            //            foreach (FBServiceBLLWS.T_FB_BUDGETACCOUNT fb in fblist)
            //            {
            //                if ((v.COMPANYID == fb.OWNERCOMPANYID) && (v.DEPARTMENTID == fb.OWNERDEPARTMENTID))
            //                {
            //                    if (v.SALARYSUM > fb.USABLEMONEY)
            //                    {
            //                        resultstr += v.DEPARTMENTNAME + ";";
            //                        result = false;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        result = false;
            //        resultstr = "NODATA";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    result = false;
            //    resultstr = ex.Message.ToString();
            //}

            //if (result) 
            #endregion
        }

        /// <summary>
        ///  共用方法
        /// </summary>
        /// <param name="strBalanceEmployeeID">薪资结算人的员工ID</param>
        /// <param name="objectType">薪资结算的机构类型</param>
        /// <param name="objectID">薪资结算的机构ID</param>
        /// <param name="year">薪资结算年份</param>
        /// <param name="month">薪资结算月份</param>
        /// <param name="markType">true表示直接生成薪资数据，false表示查询预算需要的数据判断预算是否够用</param>
        /// <returns>返回数值扣除预算</returns>
        public List<V_RETURNFBI> GenerateSalaryRecord(int GernerateType,string GenerateCompanyid,string GenerateEmployeePostid,string strBalanceEmployeeID, int objectType, string objectID, int year, int month, bool markType)
        {
            List<V_RETURNFBI> needBudgedAccount = new List<V_RETURNFBI>();
            string strCompanyID = string.Empty;
            if (objectType == 3 && objectID.Contains(";"))
            {
                string[] strlist = string.Copy(objectID).Split(';');
                if (strlist.Length == 2)
                {
                    objectID = strlist[0];
                    strCompanyID = strlist[1];
                }
            }

            if (!markType)
            {
                switch (GernerateType)
                {
                    case 0://发薪机构
                        GenerateEmployeeBySalaryCompany(strBalanceEmployeeID,GenerateEmployeePostid,GenerateCompanyid, year, month);
                        break;
                    case 1://指定组织架构
                        GernerateSlaryByOrgType(objectType, strBalanceEmployeeID, GenerateEmployeePostid,objectID, year, month, GenerateCompanyid);
                        break;
                    case 2://离职薪资
                        GenerateLeftOfficeEmployeeSalary(objectID, strBalanceEmployeeID,GenerateEmployeePostid, year, month, GenerateCompanyid);                        
                        break;
                    case 3://结算岗位薪资
                        GenerateAsinBalancePostEmployeeSalary(GenerateEmployeePostid, strBalanceEmployeeID, year, month, GenerateCompanyid);     
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (objectType)
                {
                    case 0:
                        needBudgedAccount = GenerateCompanySalary(strBalanceEmployeeID,GenerateEmployeePostid, objectID, year.ToString(), month.ToString());
                        break;
                    case 1:
                        var ents1 = from c in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                                    where c.DEPARTMENTID == objectID
                                    select new
                                    {
                                        COMPANYID = c.T_HR_COMPANY.COMPANYID,
                                        COMPANYNAME = c.T_HR_COMPANY.CNAME,
                                        DEPARTMENTID = c.DEPARTMENTID,
                                        DEPARTMENTNAME = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                                    };
                        if (ents1.Count() > 0)
                        {
                            V_RETURNFBI vr1 = new V_RETURNFBI();
                            var en = ents1.FirstOrDefault();
                            vr1.COMPANYID = en.COMPANYID;
                            vr1.COMPANYNAME = en.COMPANYNAME;
                            vr1.DEPARTMENTID = en.DEPARTMENTID;
                            vr1.DEPARTMENTNAME = en.DEPARTMENTNAME;
                            vr1.SALARYSUM = GenerateDepartmentSalary(strBalanceEmployeeID,GenerateEmployeePostid, objectID, year.ToString(), month.ToString());
                            needBudgedAccount.Add(vr1);
                        }
                        break;
                    case 2:
                        var ents2 = from b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT").Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                                    where b.POSTID == objectID
                                    select new
                                    {
                                        COMPANYID = b.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                        COMPANYNAME = b.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                        DEPARTMENTID = b.T_HR_DEPARTMENT.DEPARTMENTID,
                                        DEPARTMENTNAME = b.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                                    };
                        if (ents2.Count() > 0)
                        {
                            V_RETURNFBI vr2 = new V_RETURNFBI();
                            var en = ents2.FirstOrDefault();
                            vr2.COMPANYID = en.COMPANYID;
                            vr2.COMPANYNAME = en.COMPANYNAME;
                            vr2.DEPARTMENTID = en.DEPARTMENTID;
                            vr2.DEPARTMENTNAME = en.DEPARTMENTNAME;
                            vr2.SALARYSUM = GeneratePostSalary(strBalanceEmployeeID,GenerateEmployeePostid, objectID, year.ToString(), month.ToString());
                            needBudgedAccount.Add(vr2);
                        }
                        break;
                    case 3:
                        var ents3 = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").Include("T_HR_DEPARTMENT").Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                                    where a.T_HR_EMPLOYEE.EMPLOYEEID == objectID
                                    select new
                                    {
                                        COMPANYID = a.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                        COMPANYNAME = a.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                        DEPARTMENTID = a.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                        DEPARTMENTNAME = a.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                                    };
                        if (ents3.Count() > 0)
                        {
                            V_RETURNFBI vr3 = new V_RETURNFBI();
                            var en = ents3.FirstOrDefault();
                            vr3.COMPANYID = en.COMPANYID;
                            vr3.COMPANYNAME = en.COMPANYNAME;
                            vr3.DEPARTMENTID = en.DEPARTMENTID;
                            vr3.DEPARTMENTNAME = en.DEPARTMENTNAME;
                            vr3.SALARYSUM = GetEmployeeSalary(objectID, year.ToString(), month.ToString());
                            //vr3.SALARYSUM = GenerateEmployeeSalary(objectID, year.ToString(), month.ToString());
                            needBudgedAccount.Add(vr3);
                        }
                        break;
                }
            }

            

            return needBudgedAccount;
        }
        /// <summary>
        /// 结算指定组织架构的员工薪资
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="strBalanceEmployeeID"></param>
        /// <param name="objectID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="strCompanyID"></param>
        private void GernerateSlaryByOrgType(int objectType, string strBalanceEmployeeID,string GenerateEmployeePostid, string objectID, int year, int month, string strCompanyID)
        {
            switch (objectType)
            {
                case 0://公司
                    GenerateCompanySalary(strBalanceEmployeeID,GenerateEmployeePostid, objectID, year.ToString(), month.ToString());
                    break;
                case 1://部门
                    GenerateDepartmentSalary(strBalanceEmployeeID,GenerateEmployeePostid, objectID, year.ToString(), month.ToString());
                    break;
                case 2://岗位
                    GeneratePostSalary(strBalanceEmployeeID,GenerateEmployeePostid, objectID, year.ToString(), month.ToString());
                    break;
                case 3://员工
                    T_HR_SALARYARCHIVE ent = GetEmployeeAcitiveSalaryArchive(objectID,GenerateEmployeePostid, strCompanyID,year,month);
                    if (ent != null)
                    {
                        if (!checkEmployeeHaveMainPostInCompany(objectID, strCompanyID))
                        {
                            Tracer.Debug("根据组织架构结算员工薪资，该员工被跳过，因为该员工在结算人所在的公司没有生效的主岗位," + "员工id：" + objectID + " 公司id" + strCompanyID);
                            break;
                        }
                        GenerateEmployeeSalary(1,ent, strBalanceEmployeeID, objectID, year.ToString(), month.ToString(), strCompanyID);
                    }
                    else
                    {
                        Tracer.Debug("根据组织架构生成员工月薪异常，异常员工id：" + objectID + " 异常原因：无有效的薪资档案或员工状态异常"
                          + " 结算员工岗位id：" + GenerateEmployeePostid + " 考勤结算记录所属公司id（获取薪资档案的公司id）：" + strCompanyID);
                    }
                    break;
            }
        }

        /// <summary>
        /// 结算指定了结算岗位的员工薪资
        /// </summary>
        /// <param name="postid"></param>
        /// <returns></returns>
        private void GenerateAsinBalancePostEmployeeSalary(string GenerateEmployeePostid, string strBalanceEmployeeID, int year, int month, string GenerateCompanyid)
        {
            var company = dal.GetObjects<T_HR_COMPANY>().Where(c => c.COMPANYID == GenerateCompanyid).FirstOrDefault();
            //查询在职的结算岗位为指定岗位的员工及员工薪资档案
            //List<T_HR_EMPLOYEE> list = new List<T_HR_EMPLOYEE>();
            var q = from employee in dal.GetObjects<T_HR_EMPLOYEE>()
                    join salaryAhive in dal.GetTable<T_HR_SALARYARCHIVE>()
                    on employee.EMPLOYEEID equals salaryAhive.EMPLOYEEID
                    where salaryAhive.BALANCEPOSTID == GenerateEmployeePostid
                    && salaryAhive.CHECKSTATE == "2"
                    && employee.EMPLOYEESTATE != "2" //在职的
                    && salaryAhive.OTHERSUBJOIN <= year
                    && (salaryAhive.OTHERSUBJOIN < year
                    || (salaryAhive.OTHERSUBJOIN == year
                    && salaryAhive.OTHERADDDEDUCT <= month))
                    select salaryAhive;

            if (q.Count() > 0)
            {
                Tracer.Debug("获取到所有结算岗位 " + GenerateEmployeePostid + " 上的员工薪资档案，共 " + q.Count().ToString() + "条");

                List<T_HR_SALARYARCHIVE> SalaryArchivelist = q.ToList();
                var employees = (from ent in SalaryArchivelist
                                 select new { ent.EMPLOYEEID, ent.EMPLOYEENAME }).Distinct().ToList();
                int i=1;
                foreach (var employee in employees)
                {
                    try
                    {
                        //获取员工最新生效的一条薪资档案信息以判断是否可以继续操作
                        T_HR_SALARYARCHIVE entLast = GetEmployeeAcitiveSalaryArchive(employee.EMPLOYEEID, GenerateEmployeePostid, GenerateCompanyid, year, month);
                        if (entLast == null)
                        {
                            Tracer.Debug("根据结算岗位结算员工薪资，该员工被跳过，因为该员工最新的薪资档案验证后为空," + "员工姓名：" + employee.EMPLOYEENAME + ",员工id：" + employee.EMPLOYEEID + " 发薪结构：" + company.CNAME + " 发薪机构id" + GenerateCompanyid);
                            //验证失败，跳过。
                            continue;
                        }
                        
                        T_HR_SALARYARCHIVE employeeSalaryArchive
                            = SalaryArchivelist.Where(c => c.EMPLOYEEID == employee.EMPLOYEEID).OrderByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT).ThenByDescending(s => s.CREATEDATE).FirstOrDefault();

                        GenerateEmployeeSalary(i, employeeSalaryArchive, strBalanceEmployeeID, employee.EMPLOYEEID, year.ToString(), month.ToString(), GenerateCompanyid);
                        i++;
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug("结算指定结算岗位薪资异常,员工id" + employee.EMPLOYEEID + " 异常原因：" + ex.ToString());
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 结算离职员工的薪资
        /// </summary>
        /// <param name="postid"></param>
        /// <returns></returns>
        private void GenerateLeftOfficeEmployeeSalary(string leftemployeeid, string strBalanceEmployeeID, string GenerateEmployeePostid, int year, int month, string GenerateCompanyid)
        {
            try
            {
                var emp = (from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                           where ent.EMPLOYEEID == leftemployeeid
                           select ent).FirstOrDefault();
                Tracer.Debug("开始结算离职人员薪资，结算年月：" + year + "-" + month + "：员工id：" + leftemployeeid + "，员工姓名：" + emp.EMPLOYEECNAME + " 员工状态(2为已离职)：" + emp.EMPLOYEESTATE);
                //查询在职的结算岗位为指定岗位的员工及员工薪资档案
                //找出最新一条离职确认记录并通过离职的岗位找到所属公司。
                var leftConfimList = (from ent in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>()
                                  where ent.EMPLOYEEID == leftemployeeid
                                  select ent).ToList();//.OrderByDescending(c=>c.STOPPAYMENTDATE).FirstOrDefault();
                var leftConfim = (from item in leftConfimList
                                  orderby item.STOPPAYMENTDATE descending
                                  select item).FirstOrDefault();
                if (leftConfim == null)
                {
                    Tracer.Debug("员工离职信息为空，跳过结算薪资" + "，员工姓名：" + emp.EMPLOYEECNAME);
                    return;
                }
                //只有当月离职或上月离职可结算出结果
                if (leftConfim.STOPPAYMENTDATE.Value <= new DateTime(DateTime.Now.Year,DateTime.Now.Month-1,1))
                {
                    Tracer.Debug("结算离职薪资，非当月或上月离职员工，跳过结算薪资" + "，员工姓名：" 
                        + emp.EMPLOYEECNAME
                        + " 离职日期：" + leftConfim.STOPPAYMENTDATE.Value.ToString("yyyy-MM-dd"));
                    return;
                }
                var lastCompanyid = (from ent in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                     where ent.EMPLOYEEPOSTID == leftConfim.EMPLOYEEPOSTID
                                     select ent.T_HR_POST.COMPANYID).FirstOrDefault();
                if (string.IsNullOrEmpty(lastCompanyid))
                {
                    Tracer.Debug("结算离职薪资，离职公司id为空，跳过结算薪资" + "，员工姓名：" + emp.EMPLOYEECNAME);
                    return;
                }
                //获取离职公司的最后一条薪资记录
                var q = from employee in dal.GetObjects<T_HR_EMPLOYEE>().Include("T_HR_SALARYARCHIVE")
                        join salaryAhive in dal.GetTable<T_HR_SALARYARCHIVE>()
                            on employee.EMPLOYEEID equals salaryAhive.EMPLOYEEID
                        where salaryAhive.OWNERID == leftemployeeid
                        && salaryAhive.CHECKSTATE == "2"
                        //&& employee.EMPLOYEESTATE == "2"//已离职
                        && salaryAhive.OWNERCOMPANYID == lastCompanyid
                        && salaryAhive.OTHERSUBJOIN <= year
                        && (salaryAhive.OTHERSUBJOIN < year
                        || (salaryAhive.OTHERSUBJOIN == year
                        && salaryAhive.OTHERADDDEDUCT <= month))
                        select salaryAhive;

                if (q.Count() > 0)
                {
                    Tracer.Debug("获取离职员工： " + leftemployeeid + " 上的员工薪资档案，共 " + q.Count().ToString() + "条");

                    List<T_HR_SALARYARCHIVE> salarylist = q.ToList();
                    var employees = (from ent in salarylist
                                     select new { ent.EMPLOYEEID, ent.EMPLOYEENAME }).Distinct().ToList();
                    int i = 1;
                    foreach (var employee in employees)
                    {
                        try
                        {
                            T_HR_SALARYARCHIVE salaryArchive
                                = salarylist.Where(c => c.EMPLOYEEID == employee.EMPLOYEEID).OrderByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT).ThenByDescending(s => s.CREATEDATE).FirstOrDefault();
                            if (!string.IsNullOrEmpty(salaryArchive.BALANCEPOSTID))
                            {
                                if (salaryArchive.BALANCEPOSTID != GenerateEmployeePostid)
                                {
                                    Tracer.Debug("结算离职员工薪资，该员工被跳过，因为该员工的薪资档案设置的结算岗位跟结算人的主岗位不符," + "员工姓名：" 
                                        + employee.EMPLOYEENAME + ",员工id：" + employee.EMPLOYEEID + " 发薪机构id" + GenerateCompanyid);
                                    continue;
                                }
                            }
                            GenerateEmployeeSalary(i, salaryArchive, strBalanceEmployeeID, employee.EMPLOYEEID, year.ToString(), month.ToString(), GenerateCompanyid);
                            i++;
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("结算离职员工薪资异常,员工id" + employee + " 异常原因：" + ex.ToString());
                            continue;
                        }
                    }
                }
                else
                {
                    Tracer.Debug("开始结算离职人员薪资：员工id：" + leftemployeeid + "未获取到薪资档案，跳过，发薪机构：" + GenerateCompanyid);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
        }
        
        /// <summary>
        /// 生成公司的薪资
        /// </summary>
        /// <param name="strBalanceEmployeeID">薪资结算人的员工ID</param>
        /// <param name="companyID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public List<V_RETURNFBI> GenerateCompanySalary(string strBalanceEmployeeID,string GenerateEmployeePostid, string companyID, string year, string month)
        {

            List<V_RETURNFBI> lvr = new List<V_RETURNFBI>();
            DepartmentBLL bll = new DepartmentBLL();
            List<T_HR_DEPARTMENT> emplist = bll.GetDepartmentByCompanyId(companyID);
            var ents = from a in dal.GetObjects<T_HR_COMPANY>()
                       where a.COMPANYID == companyID
                       select a;
            foreach (var emp in emplist)
            {
                V_RETURNFBI vr = new V_RETURNFBI();
                T_HR_DEPARTMENT dep = new T_HR_DEPARTMENT();
                dep = bll.GetDepartmentById(emp.DEPARTMENTID);
                vr.COMPANYID = companyID;
                try
                {
                    vr.COMPANYNAME = ents.FirstOrDefault().CNAME;
                    vr.DEPARTMENTID = emp.DEPARTMENTID;
                    vr.DEPARTMENTNAME = dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    vr.SALARYSUM = GenerateDepartmentSalary(strBalanceEmployeeID,GenerateEmployeePostid, emp.DEPARTMENTID, year, month);
                }
                catch (Exception ex)
                {
                    vr.SALARYSUM = 0;

                    if (ex.InnerException != null)
                    {
                        SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                    }
                    else
                    {
                        SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                    }
                }
                lvr.Add(vr);
            }
            return lvr;
        }

        /// <summary>
        /// 生成部门的薪资
        /// </summary>
        /// <param name="strBalanceEmployeeID">薪资结算人的员工ID</param>
        /// <param name="departID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public decimal GenerateDepartmentSalary(string strBalanceEmployeeID,string GenerateEmployeePostid, string departID, string year, string month)
        {
            decimal result = 0;
            PostBLL bll = new PostBLL();
            List<T_HR_POST> emplist = bll.GetPostByDepartId(departID);
            foreach (var emp in emplist)
            {
                try
                {
                    result += GeneratePostSalary(strBalanceEmployeeID,GenerateEmployeePostid, emp.POSTID, year, month);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                    }
                    else
                    {
                        SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 生成岗位的薪资
        /// </summary>
        /// <param name="strBalanceEmployeeID">薪资结算人的员工ID</param>
        /// <param name="postID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public decimal GeneratePostSalary(string strBalanceEmployeeID,string GenerateEmployeePostid, string postID, string year, string month)
        {
            decimal result = 0;
            EmployeeBLL bll = new EmployeeBLL();
            //  List<T_HR_EMPLOYEE> emplist = bll.GetEmployeeByPostIDandAttBalance(postID, Convert.ToInt32(year), Convert.ToInt32(month));
            int salaryYear = Convert.ToInt32(year);
            int salaryMonth = Convert.ToInt32(month);
            var emplist = from c in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>()
                          join b in dal.GetObjects<T_HR_EMPLOYEE>() on c.EMPLOYEEID equals b.EMPLOYEEID into tmp
                          from b in tmp.DefaultIfEmpty()
                          where c.OWNERPOSTID == postID && c.BALANCEYEAR == salaryYear && c.BALANCEMONTH == salaryMonth
                          select new
                          {
                              EMPLOYEEENAME = b.EMPLOYEECNAME,
                              EMPLOYEEID = b.EMPLOYEEID,
                              CompanyID = c.OWNERCOMPANYID,
                              EMPLOYEECNAME = b.EMPLOYEECNAME

                          };
            if (emplist.Count() <= 0) return 0;
            int i = 1;
            foreach (var emp in emplist)
            {
                try
                {
                    if (!checkEmployeeHaveMainPostInCompany(emp.EMPLOYEEID, emp.CompanyID))
                    {
                        Tracer.Debug("根据组织架构结算员工薪资，该员工被跳过，因为该员工在结算人所在的公司没有生效的主岗位," + "员工姓名：" + emp.EMPLOYEECNAME + ",员工id：" + emp.EMPLOYEEID + " 公司id" + emp.CompanyID);
                        continue;
                    }
                    T_HR_SALARYARCHIVE salaryArchive = GetEmployeeAcitiveSalaryArchive(emp.EMPLOYEEID, GenerateEmployeePostid,emp.CompanyID, int.Parse(year), int.Parse(month));
                    if (salaryArchive != null)
                    {
                        DateTime startTime = DateTime.Now;
                        SMT.Foundation.Log.Tracer.Debug("根据岗位开始生成员工" + emp.EMPLOYEECNAME + "薪资------------------------------------------");
                        result += GenerateEmployeeSalary(i,salaryArchive, strBalanceEmployeeID, emp.EMPLOYEEID, year, month, emp.CompanyID);
                        i++;
                        DateTime endTime = DateTime.Now;
                        SMT.Foundation.Log.Tracer.Debug("根据岗位结束生成员工" + emp.EMPLOYEECNAME + "薪资------------------------------------------" + "耗时：" + (endTime - startTime).TotalSeconds + "秒");

                        
                    }
                    else
                    {
                        Tracer.Debug("根据组织架构生成员工月薪异常，异常员工id：" + emp.EMPLOYEEID + " 异常原因：无有效的薪资档案或员工状态异常"
                            + " 结算员工岗位id："+ GenerateEmployeePostid +" 考勤结算记录所属公司id（获取薪资档案的公司id）："+ emp.CompanyID +" 年"+ int.Parse(year) +
                            " 月"+ int.Parse(month));
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Debug("根据岗位生成员工月薪异常，异常员工id：" + emp.EMPLOYEEID + " 异常原因：" + ex.ToString());
                    continue;
                }
            }
            return result;
        }


        private string SalaryArchiveId = string.Empty;
        /// <summary>
        /// 生成员工的薪资
        /// </summary>
        /// <param name="strBalanceEmployeeID">薪资结算人的员工ID</param>
        /// <param name="employeeID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="GenerateCompanyid">结算的公司id（发薪机构id）</param>
        /// <returns></returns>
        public decimal GenerateEmployeeSalary(int index,T_HR_SALARYARCHIVE SalaryArchive, string strBalanceEmployeeID, string employeeID, string year, string month, string GenerateCompanyid)
        {
            salaryItemCaches.Clear();
            getCaches.Clear();//start cache
            string actuallypay = "0";
            sbRemark.Remove(0, sbRemark.Length);
            int salaryYear = Convert.ToInt32(year);
            int salaryMonth = Convert.ToInt32(month);
            EmployeeSalaryRecordItemBLL recorditem = new EmployeeSalaryRecordItemBLL();
            List<T_HR_EMPLOYEESALARYRECORDITEM> recorditems = new List<T_HR_EMPLOYEESALARYRECORDITEM>();
            T_HR_ATTENDMONTHLYBALANCE attendMonthlyBalance = new T_HR_ATTENDMONTHLYBALANCE();
            T_HR_EMPLOYEE emp = GetEmployeeInfor(employeeID);

            Tracer.Debug("**********************开始生成第" + index.ToString() + " 条薪资，员工姓名：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月" + "的月薪，薪资档案id：" + SalaryArchive.SALARYARCHIVEID
                + " 结算薪资机构id：" + GenerateCompanyid);

            #region ---NewItemCode新生成薪资的代码
            
            #region 判断是否已审核及已发放薪资
            T_HR_EMPLOYEESALARYRECORD record = (from r in dal.GetTable()
                                                where r.EMPLOYEEID == employeeID 
                                                && r.SALARYMONTH == month && r.SALARYYEAR == year
                                                && r.OWNERCOMPANYID == GenerateCompanyid
                                                select r).FirstOrDefault();
            if (record != null)
            {
                if (record.PAYCONFIRM == "2" 
                    || record.CHECKSTATE == "2" 
                    || record.CHECKSTATE == "1")
                {
                    Tracer.Debug("员工姓名：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月" + "的月薪生成被跳过，已结算过："
                        + "PAYCONFIRM:" + record.PAYCONFIRM + "CHECKSTATE:" + record.CHECKSTATE);
                    return 0;
                }
            }
            else
            {
                Tracer.Debug("开始生成员工姓名：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月" + "的月薪，record 为null,employeeid: " + employeeID);
            }
            #endregion

            #region  判断合同
            //var employeecontracts = from c in dal.GetObjects<T_HR_EMPLOYEECONTRACT>()
            //                        where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && c.CHECKSTATE == "2"
            //                        select c;
            //var employeecontract = employeecontracts.OrderByDescending(s => s.TODATE).FirstOrDefault();
            //if (employeecontract == null)
            //{
            //    GetInfor.Add(emp.EMPLOYEECNAME, "无有效劳动合同");
            //    return 0;
            //}
            //DateTime dt;
            //bool secuceed = DateTime.TryParse(employeecontract.TODATE, out dt);
            //if (!secuceed)
            //{
            //    GetInfor.Add(emp.EMPLOYEECNAME, "劳动合同过期");
            //    return 0;
            //}
            //if (dt.Date < DateTime.Now.Date)
            //{
            //    GetInfor.Add(emp.EMPLOYEECNAME, "劳动合同过期");
            //    return 0;
            //}
            #endregion

            #region 判断薪资档案的发薪机构及考勤机构是否为空           
            if (string.IsNullOrEmpty(SalaryArchive.PAYCOMPANY))
            {
                if (GetInfor.Keys.Contains(emp.EMPLOYEECNAME))
                {
                    return 0;
                }
                GetInfor.Add(emp.EMPLOYEECNAME, "员工薪资档案中未找到设置的发薪机构");
                Tracer.Debug("结算薪资项问题2001" + emp.EMPLOYEECNAME + GetInfor.FirstOrDefault().Value);
                return 0;
            }
            if (string.IsNullOrEmpty(SalaryArchive.ATTENDANCEORGID))
            {
                SalaryArchive.ATTENDANCEORGID = SalaryArchive.PAYCOMPANY;
            }
            #endregion

            #region 获取考勤结算结果
            var ams = from a in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>()
                      where a.EMPLOYEEID == employeeID 
                      && a.CHECKSTATE == "2" 
                      && a.BALANCEYEAR == salaryYear && a.BALANCEMONTH == salaryMonth
                      && a.OWNERCOMPANYID == SalaryArchive.ATTENDANCEORGID
                      select a;

            if (ams == null)
            {
                if (GetInfor.Keys.Contains(emp.EMPLOYEECNAME))
                {
                    return 0;
                }

                GetInfor.Add(emp.EMPLOYEECNAME, "无考勤结算记录");
                Tracer.Debug("结算薪资项问题1539" + emp.EMPLOYEECNAME + GetInfor.FirstOrDefault().Value);
                return 0;
            }

            if (ams.Count() == 0)
            {
                if (GetInfor.Keys.Contains(emp.EMPLOYEECNAME))
                {
                    return 0;
                }

                GetInfor.Add(emp.EMPLOYEECNAME, "无考勤结算记录" + GetInfor.FirstOrDefault().Value);
                Tracer.Debug("结算薪资项问题 1551" + emp.EMPLOYEECNAME + GetInfor.FirstOrDefault().Value);
                return 0;
            }
            else if (ams.Count() == 1)
            {
                attendMonthlyBalance = ams.FirstOrDefault();
            }
            else
            {
                attendMonthlyBalance = ams.Where(c => c.OWNERCOMPANYID == GenerateCompanyid).FirstOrDefault();
            }

            if (attendMonthlyBalance == null)
            {
                if (GetInfor.Keys.Contains(emp.EMPLOYEECNAME))
                {
                    Tracer.Debug("结算薪资项问题 1567" + emp.EMPLOYEECNAME + GetInfor.FirstOrDefault().Value);
                    return 0;
                }

                GetInfor.Add(emp.EMPLOYEECNAME, "无考勤结算记录");
                Tracer.Debug("结算薪资项问题 1570" +emp.EMPLOYEECNAME+ GetInfor.FirstOrDefault().Value);
                return 0;
            }
            #endregion

            #region 获取考勤方案
            var attendAsign 
                = GetAttendanceSolutionAsignByEmployeeID(attendMonthlyBalance.OWNERCOMPANYID
                , SalaryArchive.ATTENDANCEORGID, attendMonthlyBalance.OWNERPOSTID, employeeID);
            if (attendAsign == null || attendAsign.T_HR_ATTENDANCESOLUTION == null)
            {
                if (GetInfor.Keys.Contains(emp.EMPLOYEECNAME))
                {
                    Tracer.Debug("结算薪资项问题 1582" + emp.EMPLOYEECNAME + GetInfor.FirstOrDefault().Value);
                    return 0;
                }

                GetInfor.Add(emp.EMPLOYEECNAME, "无考勤方案");
                Tracer.Debug("结算薪资项问题 1587" + emp.EMPLOYEECNAME + GetInfor.FirstOrDefault().Value);
                return 0;
            }
            AttendsolutionForSalary = attendAsign.T_HR_ATTENDANCESOLUTION;
            #endregion

            getresult = true;
            try
            {
                #region  获取薪资档案              
                //按传递进来的薪资档案计算薪资
                T_HR_SALARYARCHIVE GenerateCompanyEmployeeArchive = SalaryArchive;
                archiveForAreaAllowrance = SalaryArchive;
                SalaryArchiveId = SalaryArchive.SALARYARCHIVEID;
                #endregion

                nsize = GetAccuracy(employeeID);
                bool isNew = true;
                if (record == null)
                {
                    Tracer.Debug("生成员工姓名：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月"
                        + "的月薪，record == null");

                    record = new T_HR_EMPLOYEESALARYRECORD();
                    record.EMPLOYEESALARYRECORDID = Guid.NewGuid().ToString();
                    record.EMPLOYEEID = employeeID;
                    try
                    {
                        record.CREATEUSERID = construe[0];
                        record.CREATEPOSTID = construe[1];
                        //  record.CREATEPOSTID = archive.CREATEPOSTID;
                        record.CREATEDEPARTMENTID = construe[2];
                        record.CREATECOMPANYID = construe[3];

                        record.OWNERID = employeeID;
                        record.OWNERPOSTID = construe[1];
                        record.OWNERDEPARTMENTID = construe[2];
                        record.OWNERCOMPANYID = GenerateCompanyid;

                        record.ATTENDANCEUNUSUALTIMES = emp.OWNERCOMPANYID;//记录薪资人的所属公司
                        record.ATTENDANCEUNUSUALTIME = emp.OWNERPOSTID;//记录薪资人的所属岗位
                        record.ABSENTTIMES = archiveForAreaAllowrance.SALARYARCHIVEID;//记录生成薪资使用的薪资档案
                        
                    }
                    catch (Exception exx)
                    {
                        exx.Message.ToString();
                        if (exx.InnerException != null)
                        {
                            SMT.Foundation.Log.Tracer.Debug(exx.InnerException.Message);
                        }
                        else
                        {
                            SMT.Foundation.Log.Tracer.Debug(exx.Message);
                        }
                    }
                    isNew = true;
                }
                else
                {
                    Tracer.Debug("生成员工姓名：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月"
                          + "的月薪，record 不为 null");
                    isNew = false;
                    if (record.CHECKSTATE == ((int)CheckStates.Approved).ToString() || record.CHECKSTATE == ((int)CheckStates.Approving).ToString())
                    {
                        Tracer.Debug("生成员工姓名：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月"
                            + "的月薪，已提交审核，跳过");
                        return 0;
                    }
                }

                #region ---薪资计算 NEWCAL

                var commonSalaryItems = from c in dal.GetObjects<T_HR_SALARYITEM>()
                                        where c.OWNERCOMPANYID == GenerateCompanyEmployeeArchive.PAYCOMPANY
                                        select c;
                if (commonSalaryItems.Count() < 1)
                {
                    Tracer.Debug("结算"+ year + "年" + month 
                        + "月 员工薪资失败，根据薪资档案的发薪机构获取的薪资项目<1 员工姓名：" 
                        + emp.EMPLOYEECNAME + " " 
                        + " 薪资档案id：" + GenerateCompanyEmployeeArchive.SALARYARCHIVEID
                        + " 发薪机构id：" + GenerateCompanyEmployeeArchive.PAYCOMPANY);
                    return 0;
                }
                //改为全集团不在使用同一套薪资项目
                var ents = from archiveItem in dal.GetObjects<T_HR_SALARYARCHIVEITEM>().Include("T_HR_SALARYARCHIVE")
                           join tmpItem in dal.GetObjects<T_HR_SALARYITEM>() on archiveItem.SALARYITEMID equals tmpItem.SALARYITEMID
                           join salaryItem in commonSalaryItems on tmpItem.SALARYITEMNAME equals salaryItem.SALARYITEMNAME
                           where archiveItem.T_HR_SALARYARCHIVE.EMPLOYEEID == employeeID 
                           && archiveItem.T_HR_SALARYARCHIVE.SALARYARCHIVEID == GenerateCompanyEmployeeArchive.SALARYARCHIVEID
                           orderby salaryItem.SALARYITEMCODE
                           select new { archiveItem, salaryItem, salaryArchive = archiveItem.T_HR_SALARYARCHIVE };

                if (ents.Count() > 0) //CREATE SALARYRECORDITEM
                {
                    record.SALARYSTANDARDID = ents.FirstOrDefault().archiveItem.SALARYSTANDARDID;

                    //实例化计算薪资项的方法
                    Operation opFunc = new Operation();

                    //循环薪资档案中的薪资项目生成金额
                    foreach (var ent in ents)
                    {
                        if (ent.archiveItem == null || ent.salaryItem == null || ent.salaryArchive == null)
                        {
                            SMT.Foundation.Log.Tracer.Debug("员工薪资档案，员工薪资档案中的薪资项其中一项为空");
                            return 0;
                        }
                        var tempSalaryItem = ent.salaryItem;
                        DateTime st = System.DateTime.Now;
                        SMT.Foundation.Log.Tracer.Debug("姓名：" + emp.EMPLOYEECNAME+" 薪资项："+tempSalaryItem.SALARYITEMNAME + ":");
                        if (tempSalaryItem.SALARYITEMNAME == "假期其它扣款")
                        {
                        }
                        //"1、手工录入 ；2、薪资档案中输入；3、计算公式；"
                        if (tempSalaryItem.CALCULATORTYPE == "2" && tempSalaryItem.GUERDONSUM == 0)
                        {
                            if (tempSalaryItem != null)
                            {
                                getCaches.Add(tempSalaryItem.SALARYITEMID, ent.archiveItem.SUM.ToString());
                                continue;
                            }
                        }
                        else //否则公式计算，生成员工薪资记录
                        {
                            bool actSign = false;
                            T_HR_EMPLOYEESALARYRECORDITEM en = new T_HR_EMPLOYEESALARYRECORDITEM();
                            en.SALARYRECORDITEMID = Guid.NewGuid().ToString();
                            en.T_HR_EMPLOYEESALARYRECORD = record;
                            en.SALARYITEMID = ent.archiveItem.SALARYITEMID;
                            en.SALARYARCHIVEID = ent.salaryArchive.SALARYARCHIVEID;
                            en.SALARYSTANDARDID = ent.archiveItem.SALARYSTANDARDID;
                            en.ORDERNUMBER = ent.archiveItem.ORDERNUMBER;
                            if (tempSalaryItem.CALCULATORTYPE != "1")
                            {
                                //地区差异补贴
                                if (tempSalaryItem.ENTITYCOLUMNCODE == "AREADIFALLOWANCE")
                                {
                                    en.SUM = AreaSubsidy().ToString();  //计算地差补贴
                                }
                                //考勤异常扣款
                                else if (tempSalaryItem.ENTITYCOLUMNCODE == "ATTENDANCEUNUSUALDEDUCT")
                                {
                                    decimal dDeduct = 0, dTemp = 0;
                                    //考勤异常扣款---针对迟到，早退和漏打卡,而旷工则转用薪资项目设置的公式计算
                                    dDeduct = AbnormalDeduct(employeeID, Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToDecimal(attendMonthlyBalance.ABSENTDAYS));

                                    string strDeduct = string.Empty;
                                    GetValueWithSalaryItem(employeeID, salaryYear, salaryMonth, opFunc, tempSalaryItem, ref strDeduct);
                                    decimal.TryParse(strDeduct, out dTemp);
                                    dDeduct += dTemp;

                                    en.SUM = dDeduct.ToString();
                                    getCaches.Add(tempSalaryItem.SALARYITEMID, en.SUM.ToString());
                                }
                                else
                                {
                                    bool overTime = true;
                                    if (tempSalaryItem.ENTITYCOLUMNCODE == "OVERTIME") //加班费计算
                                    {

                                        if (AttendsolutionForSalary != null)
                                        {
                                            //如果考勤方案设置加班无报酬，则加班费为0
                                            if (AttendsolutionForSalary.OVERTIMEPAYTYPE == "1" || AttendsolutionForSalary.OVERTIMEPAYTYPE == "3")
                                            {
                                                en.SUM = "0";
                                                overTime = false;
                                            }
                                        }
                                        #region
                                        //DateTime dtAttendRdStart = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1);
                                        //DateTime dtAttendRdEnd = dtAttendRdStart.AddMonths(1).AddDays(-1);

                                        //AttendanceRecordBLL attrecordBll = new AttendanceRecordBLL();
                                        //var attrecord = attrecordBll.GetAttendanceRecordByEmployeeIDAndDate(employeeID, dtAttendRdStart, dtAttendRdEnd).FirstOrDefault();
                                        //if (attrecord != null)
                                        //{
                                        //    var attSolution = (from c in dal.GetObjects<T_HR_ATTENDANCESOLUTION>()
                                        //                       where c.ATTENDANCESOLUTIONID == attrecord.ATTENDANCESOLUTIONID
                                        //                       select c).FirstOrDefault();
                                        //    if (attSolution.OVERTIMEPAYTYPE == "1" || attSolution.OVERTIMEPAYTYPE == "3")
                                        //    {
                                        //        en.SUM = "0";
                                        //        overTime = false;
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    AttendanceSolutionBLL solutionBll = new AttendanceSolutionBLL();
                                        //    var anOtherSolution = solutionBll.GetAttendanceSolutionByEmployeeID(employeeID);
                                        //    if (anOtherSolution != null)
                                        //    {
                                        //        if (anOtherSolution.OVERTIMEPAYTYPE == "1" || anOtherSolution.OVERTIMEPAYTYPE == "3")
                                        //        {
                                        //            en.SUM = "0";
                                        //            overTime = false;
                                        //        }
                                        //    }
                                        //}
                                        #endregion
                                    }
                                    //使用公式计算薪资时，薪资项是加班费且加班无报酬，不执行这段代码
                                    if (!string.IsNullOrEmpty(tempSalaryItem.CALCULATEFORMULACODE) && overTime == true)
                                    {
                                        string str = string.Empty;
                                        GetValueWithSalaryItem(employeeID, salaryYear, salaryMonth, opFunc, tempSalaryItem, ref str);

                                        //废弃代码，处理个别人员时使用--2012/7/11 8:58
                                        //if ((tempSalaryItem.SALARYITEMNAME == "个人社保负担" || tempSalaryItem.SALARYITEMNAME == "住房公积金扣款") && companyid == "142e5b31-4596-4a90-9be6-e422c9784810" && (employeeID == "7bc9a7e1-bf07-45dd-a509-7c918d42c199" || employeeID == "804046d6-6f3c-4f7d-94cd-22a394ff4d75"))
                                        //{
                                        //    str = "0";
                                        //}

                                        en.SUM = Alternative(Convert.ToDecimal(string.IsNullOrEmpty(str) ? "0" : str), 2).ToString();
                                        getCaches.Add(tempSalaryItem.SALARYITEMID, en.SUM.ToString());

                                        //额外处理的出勤工资,和税前应发合计,若执行国际上通用的'4舍6入'规则,只要在薪资项目公式round定义即可
                                        //屏蔽下方的方法
                                        //if (tempSalaryItem.ENTITYCOLUMNCODE == "WORKINGSALARY" || tempSalaryItem.ENTITYCOLUMNCODE == "PRETAXSUBTOTAL")
                                        //{
                                        //    en.SUM = Alternative(Convert.ToDecimal(en.SUM), 0).ToString();
                                        //}
                                        //实发
                                        if (tempSalaryItem.ENTITYCOLUMNCODE == "ACTUALLYPAY")
                                        {
                                            actSign = true;
                                            en.SUM = Convert.ToDecimal(en.SUM) < 0 ? "0" : en.SUM;
                                            actuallypay = en.SUM;
                                            record.ACTUALLYPAY = AES.AESEncrypt(Convert.ToDecimal(Math.Floor(Convert.ToDouble(en.SUM))).ToString());
                                        }
                                    }
                                    else
                                    {
                                        en.SUM = ent.archiveItem.SUM != null ? ent.archiveItem.SUM : "0";
                                    }
                                }
                                en.SUM = AES.AESEncrypt(en.SUM);
                            }
                            else
                            {
                                en.SUM = ent.archiveItem.SUM;
                                if (string.IsNullOrWhiteSpace(ent.archiveItem.SUM) && tempSalaryItem.GUERDONSUM != null)
                                {
                                    en.SUM = AES.AESEncrypt(tempSalaryItem.GUERDONSUM.Value.ToString());
                                }
                            }

                            en.REMARK = ent.archiveItem.REMARK;
                            en.CREATEDATE = System.DateTime.Now;
                            SMT.Foundation.Log.Tracer.Debug((System.DateTime.Now - st).ToString());
                            //添加薪资项记录
                            if (!actSign) recorditems.Add(recorditem.GetEmployeeSalaryRecordItem(en));
                        }
                    }
                }
                else
                {
                    Tracer.Debug("结算员工薪资失败，获取的薪资档案薪资项目为空：员工id：" + employeeID + "薪资档案id：" + GenerateCompanyEmployeeArchive.SALARYARCHIVEID);
                }

                #endregion

                record.PAIDTYPE = GetPaidType(employeeID);  //发放方式
                record.REMARK = sbRemark.ToString();
                record.PAIDTYPE = GetPaidType(employeeID);
                record.EMPLOYEECODE = GenerateCompanyEmployeeArchive.EMPLOYEECODE;
                record.EMPLOYEENAME = GenerateCompanyEmployeeArchive.EMPLOYEENAME;
                record.PAYCONFIRM = "0";
                record.CHECKSTATE = "0";
                record.CREATEDATE = System.DateTime.Now;
                record.SALARYYEAR = year;
                record.SALARYMONTH = month;
                record.T_HR_EMPLOYEESALARYRECORDITEM.Clear(); IsComputer = true;
                if (IsComputer)
                {
                    if (isNew)
                    {
                       int i= dal.Add(record);
                       if (i > 0)
                       {
                           Tracer.Debug("生成员工姓名：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月"
                          + "的月薪，未生成过，添加新记录");
                       }
                    }
                    else
                    {
                       
                        int i = dal.Update(record);
                        if (i > 0)
                        {
                            Tracer.Debug("生成员工姓名：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月"
                            + "的月薪，已生成过，修改记录");
                        }
                        recorditem.EmployeeSalaryRecordItemDelete(record.EMPLOYEESALARYRECORDID);
                    }

                    if (recorditems.Count() > 0)
                    {
                        foreach (var ent in recorditems)
                        {
                            dal.AddToContext(ent);
                            //recorditem.Add(ent);
                        }
                        dal.SaveContextChanges();
                    }
                }
                calcheck += string.IsNullOrEmpty(actuallypay) ? 0 : Convert.ToDecimal(actuallypay);
                calcheck = calcheck > 0 ? 1 : 0;

                Tracer.Debug("**********************结束生成第" +index.ToString() + "条记录，员工：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月" + "的月薪");         
                return Convert.ToDecimal(actuallypay);
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                Tracer.Debug("生成员工：" + emp.EMPLOYEECNAME + " " + year + "年" + month + "月" + "的月薪异常"+ex.ToString());
                if (emp != null)
                {
                    try
                    {
                        GetInfor.Add(emp.EMPLOYEECODE, emp.EMPLOYEEENAME);
                    }
                    catch (Exception e)
                    {
                        SMT.Foundation.Log.Tracer.Debug(e.ToString());
                    }
                }
                return 0;
            }

            #endregion
            
            #region ---核心代码废弃---

            //T_HR_EMPLOYEE emp = GetEmployeeInfor(employeeID);
            //SalaryArchiveBLL bll = new SalaryArchiveBLL();
            //T_HR_ATTENDMONTHLYBALANCE attendMonthlyBalance = new T_HR_ATTENDMONTHLYBALANCE();
            //attendMonthlyBalance = GetMonthTotal(employeeID, Convert.ToInt32(year), Convert.ToInt32(month));
            //getresult = true;
            //try
            //{
            //    T_HR_SALARYARCHIVE archive = bll.GetSalaryArchiveByEmployeeID(employeeID);

            //    T_HR_EMPLOYEESALARYRECORD record = GetEmployeeSalaryRecord(employeeID, year, month);

            //    nsize = GetAccuracy(employeeID);
            //    #region testing
            //    decimal? de = 0;
            //    if (employeeID == "e29d08df-54cc-408d-97c1-6813a6452df2")
            //        de = InitialSalary(employeeID);
            //    #endregion
            //    bool isNew = true;
            //    if (record == null)
            //    {
            //        record = new T_HR_EMPLOYEESALARYRECORD();
            //        record.EMPLOYEESALARYRECORDID = Guid.NewGuid().ToString();
            //        record.EMPLOYEEID = archive.EMPLOYEEID;
            //        try
            //        {
            //            record.CREATEUSERID = construe[0];
            //            record.CREATEPOSTID = construe[1];
            //            record.CREATEDEPARTMENTID = construe[2];
            //            record.CREATECOMPANYID = construe[3];

            //            record.OWNERID = construe[4];
            //            record.OWNERPOSTID = construe[5];
            //            record.OWNERDEPARTMENTID = construe[6];
            //            record.OWNERCOMPANYID = construe[7];
            //        }
            //        catch (Exception exx)
            //        {
            //            exx.Message.ToString();
            //        }
            //        isNew = true;
            //    }
            //    else
            //    {
            //        isNew = false;
            //        //if (record.CHECKSTATE == "2") return 0;
            //    }

            //    #region  no data

            //    record.LOANDEDUCT = 0; //借款抵扣v
            //    record.MANTISSADEDUCT = 0; //尾数扣款
            //    record.OFFENCEDEDUCT = 0; //违纪扣款
            //    record.VACATIONDEDUCT = 0; //假期其他扣款
            //    record.PERSONALINCOMETAX = GetPersonalTax(employeeID, Convert.ToInt32(year), Convert.ToInt32(month)); //个人所得税
            //    record.PERSONALSICOST = ComputeSicost(employeeID, Convert.ToInt32(year), Convert.ToInt32(month)); //个人社保负担

            //    #endregion

            //    record.PERFORMANCESUM = GetPerformanceSum(employeeID, Convert.ToInt32(year), Convert.ToInt32(month)); //绩效奖金额
            //    record.CUSTOMERSUM = GetCustomerSum(employeeID, Convert.ToInt32(year), Convert.ToInt32(month)); //自定义项金额
            //    record.ABSENCEDAYS = attendMonthlyBalance.NEEDATTENDDAYS - attendMonthlyBalance.REALATTENDDAYS;//缺勤天数
            //    record.LEAVEDEDUCT = CalculationLeave(employeeID, Convert.ToInt32(year), Convert.ToInt32(month)); //请假扣款
            //    record.PAIDTYPE = GetPaidType(employeeID);  //发放方式
            //    record.ATTENDANCEUNUSUALDEDUCT = AbnormalDeduct(employeeID, Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToDecimal(attendMonthlyBalance.ABSENTDAYS)); //考勤异常扣款
            //    record.ATTENDANCEUNUSUALTIME = AbnormalTimes(employeeID, Convert.ToInt32(year), Convert.ToInt32(month)).ToString(); //考勤异常次数
            //    record.ATTENDANCEUNUSUALTIMES = AbnormalTime(employeeID, Convert.ToInt32(year), Convert.ToInt32(month)).ToString();  //考勤异常时长
            //    record.EVECTIONSUBSIDY = GetEvectionMoney(employeeID, Convert.ToInt32(year), Convert.ToInt32(month));  //出差补助
            //    record.EVECTIONTIMES = GetEvectionTimes(employeeID, Convert.ToInt32(year), Convert.ToInt32(month));//出差时长
            //    record.OVERTIMESUM = OverTimesMoney(employeeID, Convert.ToInt32(year), Convert.ToInt32(month));   //加班费
            //    record.ABSENTTIMES = attendMonthlyBalance.ABSENTDAYS.ToString();//旷工天数
            //    record.ABSENTDEDUCT = AbsentTotal(Convert.ToDecimal(attendMonthlyBalance.ABSENTDAYS), employeeID, Convert.ToInt32(year), Convert.ToInt32(month));//旷工扣款
            //    record.LEAVETIME = (attendMonthlyBalance.AFFAIRLEAVEDAYS + attendMonthlyBalance.SICKLEAVEDAYS + attendMonthlyBalance.OTHERLEAVEDAYS).ToString();//请假时长
            //    record.OVERTIMETIMES = attendMonthlyBalance.OVERTIMESUMHOURS.ToString();//加班时长
            //    record.BASICSALARY = (archive.BASESALARY == null) ? 0 : archive.BASESALARY; //基本工资
            //    //record.BASICSALARY = BasicSalary(archive.EMPLOYEEID);  //基本工资
            //    record.WORKINGSALARY = WorkingSalaryTotal(archive.EMPLOYEEID, Convert.ToInt32(year), Convert.ToInt32(month));//出勤工资
            //    record.POSTSALARY = (archive.POSTSALARY == null) ? 0 : archive.POSTSALARY;//岗位工资
            //    record.SECURITYALLOWANCE = (archive.SECURITYALLOWANCE == null) ? 0 : archive.SECURITYALLOWANCE;//保密津贴
            //    record.HOUSINGALLOWANCE = (archive.HOUSINGALLOWANCE == null) ? 0 : archive.HOUSINGALLOWANCE;//住房津贴
            //    record.AREADIFALLOWANCE = (archive.AREADIFALLOWANCE == null) ? 0 : archive.AREADIFALLOWANCE;//地区差异补贴
            //    record.FOODALLOWANCE = (archive.FOODALLOWANCE == null) ? 0 : archive.FOODALLOWANCE;//餐费补贴
            //    record.HOUSINGALLOWANCEDEDUCT = (archive.HOUSINGALLOWANCEDEDUCT == null) ? 0 : archive.HOUSINGALLOWANCEDEDUCT;//住房津贴扣款
            //    record.OTHERADDDEDUCT = (archive.OTHERADDDEDUCT == null) ? 0 : archive.OTHERADDDEDUCT;//其他加扣款
            //    record.OTHERSUBJOIN = (archive.OTHERSUBJOIN == null) ? 0 : archive.OTHERSUBJOIN;//其它代扣款
            //    record.FIXEDINCOMESUM = record.BASICSALARY + record.POSTSALARY + record.SECURITYALLOWANCE + record.HOUSINGALLOWANCE + record.AREADIFALLOWANCE + record.FOODALLOWANCE;//固定收入合计
            //    record.DEDUCTTOTAL = record.ATTENDANCEUNUSUALDEDUCT + record.LEAVEDEDUCT + record.VACATIONDEDUCT + record.HOUSINGALLOWANCEDEDUCT + record.PERSONALSICOST + record.PERSONALINCOMETAX + record.OFFENCEDEDUCT + record.LOANDEDUCT + record.MANTISSADEDUCT + record.OTHERADDDEDUCT + record.OTHERSUBJOIN + GetAddDeduct(employeeID, Convert.ToInt32(year), Convert.ToInt32(month));//扣款合计
            //    record.SUBTOTAL = record.BASICSALARY + record.POSTSALARY + record.SECURITYALLOWANCE + record.HOUSINGALLOWANCE + record.AREADIFALLOWANCE + record.FOODALLOWANCE + record.WORKINGSALARY + record.OVERTIMESUM + record.EVECTIONSUBSIDY + record.PERFORMANCESUM + record.CUSTOMERSUM;   //应发小计
            //    record.ACTUALLYPAY = record.SUBTOTAL - record.DEDUCTTOTAL; //实发工资
            //    record.PRETAXSUBTOTAL = record.ACTUALLYPAY - record.PERSONALINCOMETAX;  //税前应发合计
            //    record.BALANCE = ComputeBalance(Convert.ToDecimal(record.ACTUALLYPAY), employeeID, Convert.ToInt32(year), Convert.ToInt32(month)); //差额

            //    record.EMPLOYEECODE = archive.EMPLOYEECODE;
            //    record.EMPLOYEENAME = archive.EMPLOYEENAME;
            //    record.PAYCONFIRM = "0";
            //    record.CHECKSTATE = "0";
            //    record.CREATEDATE = System.DateTime.Now;
            //    record.SALARYYEAR = year;
            //    record.SALARYMONTH = month;
            //    if (isNew)
            //        dal.Add(record);
            //    //if (IsComputer) dal.Add(record);
            //    else
            //        dal.Update(record);
            //    //if (IsComputer) dal.Update(record);
            //    return Convert.ToDecimal(record.ACTUALLYPAY);
            //    #region ---
            //    //foreach()
            //    //{
            //    //    dal.DataContext.AddObject("T_HR_ATTENDANCERECORD", entAttRd);
            //    //}
            //    //dal.DataContext.SaveChanges();
            //    #endregion
            //}
            //catch
            //{
            //    if (emp != null) GetInfor.Add(emp.EMPLOYEECODE, emp.EMPLOYEEENAME);
            //    return 0;
            //}


            #endregion
        }

        /// <summary>
        /// 获取员工结算日期生效的最新一条薪资档案
        /// </summary>
        /// <param name="employeeid">员工id</param>
        /// <param name="year">结算年份</param>
        /// <param name="month">结算月份</param>
        /// <returns></returns>
        private T_HR_SALARYARCHIVE GetEmployeeAcitiveSalaryArchive(string employeeid,string GenerateEmployeePostid, string payCompanyid, int year, int month)
        {
            var q = from employee in dal.GetObjects<T_HR_EMPLOYEE>().Include("T_HR_SALARYARCHIVE")
                    join salaryAhive in dal.GetTable<T_HR_SALARYARCHIVE>()
                        on employee.EMPLOYEEID equals salaryAhive.EMPLOYEEID
                    where salaryAhive.EMPLOYEEID == employeeid                   
                    && salaryAhive.CHECKSTATE == "2"    //终审通过的薪资档案
                    && employee.EMPLOYEESTATE != "2"    //员工还在职
                    && (salaryAhive.OTHERSUBJOIN < year
                    || (salaryAhive.OTHERSUBJOIN == year
                    && salaryAhive.OTHERADDDEDUCT <= month))
                    select salaryAhive;
            if (q.Count() > 0)
            {
                List<T_HR_SALARYARCHIVE> list = q.ToList();
                var ents = list.OrderByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT).ThenByDescending(s => s.CREATEDATE);
                //获取该员工最新的一条生效的薪资档案。
                T_HR_SALARYARCHIVE salaryAhive = ents.FirstOrDefault();
                //if (salaryAhive.EMPLOYEENAME == "梅黠")
                //{

                //}
                //如果最新薪资档案的发薪机构不为空且不等于当前结算的机构，那么跳过去
                if(!string.IsNullOrEmpty(salaryAhive.PAYCOMPANY))
                {
                    if(salaryAhive.PAYCOMPANY != payCompanyid)
                    {
                        Tracer.Debug("员工最新薪资档案获取到的发薪机构跟当前结算机构不符，薪资档案获取为空，员工姓名：" + salaryAhive.EMPLOYEENAME
                            + " 结算年份：" + year + " 结算月份：" + month);
                        return null;
                    }
                }
                if (!string.IsNullOrEmpty(salaryAhive.BALANCEPOSTID))//如果薪资档案的结算岗位不为空且不等于现在的结算人主岗位，跳过
                {
                    if (salaryAhive.BALANCEPOSTID != GenerateEmployeePostid)
                    {
                        Tracer.Debug("员工最新薪资档案获取到的结算岗位跟当前结算岗位不符，薪资档案获取为空，员工姓名：" + salaryAhive.EMPLOYEENAME
                            + " 结算年份：" + year+ " 结算月份：" + month);
                        return null;
                    }
                }
                return salaryAhive;
            }
            else
            {
                Tracer.Debug("无有效的薪资档案，员工id：" + employeeid + " 结算年份：" + year
                    + " 结算月份：" + month);
                return null;
            }
        }

        /// <summary>
        /// 根据员工ID获取考勤方案信息(解决一个员工在系统内多个公司入职的情况)
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        private T_HR_ATTENDANCESOLUTIONASIGN GetAttendanceSolutionAsignByEmployeeID(string strCompanyID, string strDepId, string strPostID, string strEmployeeID)
        {
            T_HR_ATTENDANCESOLUTIONASIGN entRes = null;
            if (string.IsNullOrEmpty(strCompanyID) || string.IsNullOrEmpty(strDepId) || string.IsNullOrEmpty(strPostID) || string.IsNullOrEmpty(strEmployeeID))
            {
                return entRes;
            }

            DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();

            var ents = from n in dal.GetObjects <T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                       where n.OWNERCOMPANYID == strCompanyID && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                       orderby n.ASSIGNEDOBJECTTYPE ascending
                       select n;

            foreach (T_HR_ATTENDANCESOLUTIONASIGN item in ents)
            {
                if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString())
                {
                    if (item.ASSIGNEDOBJECTID.Contains(strEmployeeID))
                    {
                        entRes = item;
                        break;
                    }
                }
                else
                {
                    if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strPostID)
                        {
                            entRes = item;
                            break;
                        }
                    }
                    else if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strDepId)
                        {
                            entRes = item;
                            break;
                        }
                    }
                    else if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strCompanyID)
                        {
                            entRes = item;
                            break;
                        }
                    }
                }
            }

            return entRes;
        }


        private void GetValueWithSalaryItem(string employeeID, int salaryYear, int salaryMonth, Operation opFunc, T_HR_SALARYITEM tempSalaryItem, ref string str)
        {
            if (string.IsNullOrEmpty(tempSalaryItem.CALCULATEFORMULACODE))
            {
                return;
            }

            //递归解析薪资项，执行薪资项公式，获取薪资项值
            str = AutoAssemblyCharacter(tempSalaryItem.CALCULATEFORMULACODE, employeeID, salaryYear, salaryMonth);          //新公式运算代码
            try
            {
                str = opFunc.Evalcal(string.IsNullOrEmpty(str) ? "0" : str).ToString();
                str = string.IsNullOrEmpty(str) ? "0" : Math.Round(Convert.ToDecimal(str), 2).ToString();//4->2
            }
            catch (Exception ex)
            {
                str = "0";
                if (ex.InnerException != null)
                {
                    SMT.Foundation.Log.Tracer.Debug("1132行" + ex.Message + " InnerException: " + ex.StackTrace);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("1132行" + ex.Message + " InnerException: " + ex.StackTrace);
                }
            }
        }
        /// <summary>
        /// 根据发薪机构生成薪资
        /// </summary>
        /// <param name="PayCompanyID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public void GenerateEmployeeBySalaryCompany(string strBalanceEmployeeID,string GenerateEmployeePostid, string PayCompanyID, int year, int month)
        {
            string strmsg = "通过发薪结构结算薪资：";
            var company = dal.GetObjects<T_HR_COMPANY>().Where(c => c.COMPANYID == PayCompanyID).FirstOrDefault();
                
            //查询在职的结算岗位为指定岗位的员工及员工薪资档案
            //List<T_HR_EMPLOYEE> list = new List<T_HR_EMPLOYEE>();
            var q = from employee in dal.GetObjects<T_HR_EMPLOYEE>().Include("T_HR_SALARYARCHIVE")
                    join salaryAhive in dal.GetTable<T_HR_SALARYARCHIVE>()
                    on employee.EMPLOYEEID equals salaryAhive.EMPLOYEEID
                    where salaryAhive.PAYCOMPANY == PayCompanyID//指定的发薪机构的薪资档案
                    && salaryAhive.CHECKSTATE == "2"//薪资档案审核通过的
                    && employee.EMPLOYEESTATE != "2"//在职员工
                    && (salaryAhive.OTHERSUBJOIN < year
                    ||(salaryAhive.OTHERSUBJOIN == year 
                    && salaryAhive.OTHERADDDEDUCT <= month))
                    select salaryAhive;

            int isalaryAchiveCount = q.Count();
            if (isalaryAchiveCount > 0)
            {
                List<T_HR_SALARYARCHIVE> SalaryArchivelist = q.ToList();
                
                Tracer.Debug(strmsg+"获取到所有发薪机构 " + PayCompanyID + " 上的员工薪资档案，共 " + isalaryAchiveCount.ToString() + "条");
                var employees = (from ent in SalaryArchivelist
                                                select new { ent.EMPLOYEEID, ent.EMPLOYEENAME }).Distinct().ToList();
                    int i = 1;
                    foreach (var employee in employees)
                    {
                        try
                        {
                            T_HR_SALARYARCHIVE entLast=GetEmployeeAcitiveSalaryArchive(employee.EMPLOYEEID,GenerateEmployeePostid,PayCompanyID,year,month);
                            if (entLast==null)
                            {
                                Tracer.Debug("根据发薪机构结算员工薪资，该员工被跳过，因为该员工最新的薪资档案结算岗位不为空," + "员工姓名：" + employee.EMPLOYEENAME + ",员工id：" + employee.EMPLOYEEID + " 发薪结构：" + company.CNAME + " 发薪机构id" + PayCompanyID);
                                continue;
                            }
                            if (!checkEmployeeHaveMainPostInCompany(employee.EMPLOYEEID, PayCompanyID))
                            {
                                Tracer.Debug("根据发薪机构结算员工薪资，该员工被跳过，因为该员工在发薪机构已经没有生效的主岗位,"+"员工姓名："+employee.EMPLOYEENAME+",员工id：" + employee.EMPLOYEEID + " 发薪结构：" + company.CNAME + " 发薪机构id" + PayCompanyID);
                                continue;
                            }
                            //获取员工最新生效的一条薪资档案
                            T_HR_SALARYARCHIVE employeeSalaryArchive
                                = SalaryArchivelist.Where(c => c.EMPLOYEEID == employee.EMPLOYEEID).OrderByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT).ThenByDescending(s => s.CREATEDATE).FirstOrDefault();

                            GenerateEmployeeSalary(i,employeeSalaryArchive, strBalanceEmployeeID, employee.EMPLOYEEID, year.ToString(), month.ToString(), PayCompanyID);
                            i++;
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("结算指定发薪机构薪资异常," + "员工姓名：" + employee.EMPLOYEENAME + ",员工id：" + employee.EMPLOYEEID + " 异常原因：" + ex.ToString());
                            continue;
                        }

                    }               
            }
        }


        /// <summary>
        /// 检测指定的员工在指定的公司是否有生效的主岗位
        /// </summary>
        /// <param name="employeeid"></param>
        /// <param name="companyid"></param>
        /// <returns></returns>
        private bool checkEmployeeHaveMainPostInCompany(string employeeid,string companyid)
        {
            bool flag = false;
            var q = from ent in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                    where ent.T_HR_EMPLOYEE.EMPLOYEEID == employeeid
                    && ent.ISAGENCY == "0" 
                    && ent.CHECKSTATE == "2"
                    && ent.EDITSTATE == "1"
                    && ent.T_HR_POST.COMPANYID == companyid
                    select ent;
            if (q.Count() > 0)
            {
                return true;
            }
            return flag;
        }

        /// <summary>
        /// 获取员工金额
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public decimal GetEmployeeSalary(string employeeID, string year, string month)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       where a.EMPLOYEEID == employeeID && a.SALARYYEAR == year && a.SALARYMONTH == month
                       select a;
            if (ents.Count() > 0)
            {
                try
                {
                    return ents.FirstOrDefault() != null ? Convert.ToDecimal(AES.AESDecrypt(ents.FirstOrDefault().ACTUALLYPAY)) : 0;
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                }
            }
            return 0;
        }

        /// <summary>
        /// 计算其他假期扣款
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回假期其他扣款</returns>
        //public decimal GetVacationDeduct(string employeeid, int year, int month)
        //{
        //    nodeductday = 0;
        //    T_HR_ATTENDMONTHLYBALANCE monthtotal = GetMonthTotal(employeeid, year, month);
        //    decimal? yeartotal = 0;
        //    var ents = from a in dal.GetObjects<T_HR_ATTENDYEARLYBALANCE>()
        //               where a.BALANCEYEAR == year
        //               select a;
        //    if (ents.Count() > 0) yeartotal = ents.FirstOrDefault().SICKLEAVEDAYS;
        //    if (monthtotal != null)
        //    {
        //        decimal result = 0;
        //        try
        //        {
        //            result = GetMealAllowance(employeeid) / Convert.ToDecimal(21.75) * Convert.ToDecimal((monthtotal.MARRYDAYS + monthtotal.MATERNITYLEAVEDAYS + monthtotal.NURSESDAYS + monthtotal.FUNERALLEAVEDAYS + monthtotal.TRIPDAYS));//(婚假 + 丧假 + 路程假 + 产假 + 看护假)
        //            if (yeartotal <= 5)
        //            {
        //                if (monthtotal.SICKLEAVEDAYS <= 0)
        //                {
        //                    return Math.Round(result, nsize);
        //                }
        //                else if (monthtotal.SICKLEAVEDAYS == 1)
        //                {
        //                    //nodeductday = 1;
        //                    return Math.Round(result, nsize);
        //                }
        //                else
        //                {
        //                    //nodeductday = 1;
        //                    result += Convert.ToDecimal((SystemBasicSalary(employeeid) * (25 + 30) / 100 + AreaSubsidy(employeeid) + GetMealAllowance(employeeid)) / Convert.ToDecimal(21.75) * (monthtotal.SICKLEAVEDAYS - 1));
        //                }
        //            }
        //            else
        //                result += Convert.ToDecimal((SystemBasicSalary(employeeid) * (25 + 30) / 100 + AreaSubsidy(employeeid) + GetMealAllowance(employeeid)) / Convert.ToDecimal(21.75) * monthtotal.SICKLEAVEDAYS);

        //        }
        //        catch (Exception ex)
        //        {
        //            SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
        //        }
        //        return Math.Round(result, nsize);
        //    }
        //    return 0;
        //}

        /// <summary>
        /// 计算个人所得税
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算个人所得税</returns>
        public decimal GetPersonalTax(decimal shouldPay, decimal insurance, string employeeid, decimal moneydecut)
        {
            decimal? getPtax = 0, taxRate = 0, Qcd = 0, taxPay = 0;       //taxRate税率 ,Qcd速算扣除数,taxPay应纳税所得额;
            var ents = from a in dal.GetObjects<T_HR_SALARYTAXES>()
                       join b in dal.GetObjects<T_HR_SALARYSOLUTION>() on a.T_HR_SALARYSOLUTION.SALARYSOLUTIONID equals b.SALARYSOLUTIONID
                       join c in dal.GetObjects<T_HR_SALARYARCHIVE>() on b.SALARYSOLUTIONID equals c.SALARYSOLUTIONID
                       where c.EMPLOYEEID == employeeid
                       select new
                       {
                           TAXESBASIC = b.TAXESBASIC,
                           TAXESSUM = a.TAXESSUM,
                           TAXESRATE = a.TAXESRATE,
                           CALCULATEDEDUCT = a.CALCULATEDEDUCT,
                           SOLUTIONTAXESRATE = b.TAXESCOSTRATE
                       };
            try
            {
                if (ents.Count() > 0)
                {
                    taxPay = shouldPay * ents.FirstOrDefault().SOLUTIONTAXESRATE - insurance - moneydecut - ents.FirstOrDefault().TAXESBASIC;
                    if (taxPay < 0) return 0;
                    ents = ents.OrderBy(m => m.TAXESSUM);
                    foreach (var ent in ents)
                    {
                        if (taxPay < ent.TAXESSUM)
                        {
                            taxRate = ent.TAXESRATE;
                            Qcd = ent.CALCULATEDEDUCT;
                            break;
                        }
                    }
                    if (taxRate == 0 && Qcd == 0)
                    {
                        ents = ents.Skip(ents.Count() - 1).Take(1);
                        taxRate = ents.FirstOrDefault().TAXESRATE;
                        Qcd = ents.FirstOrDefault().CALCULATEDEDUCT;
                    }
                    getPtax = taxPay * taxRate - Qcd;
                    return getPtax < 0 ? 0 : Alternative(Math.Round(Convert.ToDecimal(getPtax), 3));
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                ex.Message.ToString();
            }
            return 0;
        }

        /// <summary>
        /// 四舍五入
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="num">保留小数位数</param>
        /// <returns>返回四舍五入结果值</returns>
        public decimal Alternative(decimal value)
        {
            decimal v = value - (int)value;
            if (v == 0) return value;
            string dealwith = value.ToString();
            int Mantissa = Convert.ToInt32(dealwith.Substring(dealwith.Length - 1, 1));
            if (Mantissa >= 5)
            {
                if (v.ToString().Length == 3)
                {
                    return (int)value + 1;
                }
                else
                    return Convert.ToDecimal((int)value) + Convert.ToDecimal(v.ToString().Substring(0, v.ToString().Length - 1)) + Convert.ToDecimal(1 / Math.Pow(10, v.ToString().Length - 3));
            }
            else
            {
                return Convert.ToDecimal(dealwith.Substring(0, dealwith.Length - 1));
            }
        }

        public decimal Alternative(decimal value, int num)
        {
            num++;
            bool symbolSign = true;
            decimal v = value - (int)value;
            if (v < 0) symbolSign = false;
            if (v == 0 || num < 1) return value;
            string tempv = Math.Abs(v).ToString();
            string dealwith = string.Empty;

            if (Math.Abs(v).ToString().Length - 2 <= num - 1)
            {
                return value;
            }

            if (v.ToString().Length - 2 < num)
            {
                return value;
            }
            else if (v.ToString().Length - 2 > num)
            {
                dealwith = (int)value + tempv.Substring(1, num + 1);
            }
            else
                dealwith = value.ToString();
            int Mantissa = Convert.ToInt32(dealwith.Substring(dealwith.Length - 1, 1));
            if (Mantissa >= 5)
            {
                decimal result = Convert.ToDecimal(tempv.Substring(0, num + 1)) + Convert.ToDecimal(1 / Math.Pow(10, num - 1));
                if (symbolSign)
                {
                    return Convert.ToDecimal((int)value) + result;
                }
                else
                {
                    return Convert.ToDecimal((int)value) - result;
                }
            }
            else
            {
                return Convert.ToDecimal(dealwith.Substring(0, dealwith.Length - 1));
            }
        }

        /// <summary>
        /// 获取员工实体
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回员工实体</returns>
        public T_HR_EMPLOYEE GetEmployeeInfor(string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                       where a.EMPLOYEEID == employeeid
                       select a;
            if (ents.Count() > 0)
                return ents.FirstOrDefault();
            else
                return null;
        }

        /// <summary>
        /// 获取员工加扣款
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回员工加扣款</returns>
        public List<decimal> GetAddDeduct(string employeeid, int year, int month)
        {
            decimal result1 = 0, result2 = 0;
            string years, months;
            years = year.ToString();
            months = month.ToString();
            List<decimal> adddeduct = new List<decimal>();
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                       where a.EMPLOYEEID == employeeid && a.DEALYEAR == years && a.DEALMONTH == months
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var v in ents)
                {
                    if (v.SYSTEMTYPE != "1")
                    {
                        result1 += Convert.ToDecimal(v.PROJECTMONEY);//加扣款
                    }
                    else
                    {
                        result2 += Convert.ToDecimal(v.PROJECTMONEY);//代扣款
                    }
                    sbRemark.Append(v.PROJECTNAME + ":" + v.PROJECTMONEY + " " + v.REMARK + "  ");
                }
            }
            adddeduct.Add(result1);
            adddeduct.Add(result2);
            return adddeduct;
        }

        /// <summary>
        /// 获取薪资精度
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回薪资精度</returns>
        private int GetAccuracy(string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYARCHIVE>()
                       join b in dal.GetObjects<T_HR_SALARYSOLUTION>() on a.SALARYSOLUTIONID equals b.SALARYSOLUTIONID
                       where a.EMPLOYEEID == employeeid
                       select new
                       {
                           SALARYPRECISION = b.SALARYPRECISION
                       };

            if (ents.Count() > 0)
            {
                return Convert.ToInt32(ents.FirstOrDefault().SALARYPRECISION);
            }
            return nsize;
        }

        /// <summary>
        /// 获取发放方式
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回发放方式</returns>
        public string GetPaidType(string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYARCHIVE>()
                       join b in dal.GetObjects<T_HR_SALARYSOLUTION>() on a.SALARYSOLUTIONID equals b.SALARYSOLUTIONID
                       where a.EMPLOYEEID == employeeid
                       select new
                       {
                           PAYTYPE = b.PAYTYPE
                       };

            if (ents.Count() > 0)
            {
                if (ents.FirstOrDefault() != null)
                    return ents.FirstOrDefault().PAYTYPE;
            }
            return "1";
        }

        /// <summary>
        /// 计算其他请假扣款
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算其他请假扣款</returns>
        public decimal OtherVacation(string employeeid, int year, int month)
        {
            DateTime dttemp = new DateTime(year, month, 1);
            var entsa = from a in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                        join c in dal.GetObjects<T_HR_LEAVETYPESET>() on a.T_HR_LEAVETYPESET.LEAVETYPESETID equals c.LEAVETYPESETID
                        where a.EMPLOYEEID == employeeid && a.CHECKSTATE == "2" && c.ISFREELEAVEDAY == "0" && c.LEAVETYPEVALUE != "2" && c.LEAVETYPEVALUE != "3"
                        select a;
            entsa = entsa.Where(m => m.STARTDATETIME <= dttemp);
            entsa = entsa.Where(m => m.ENDDATETIME >= dttemp);
            if (entsa.Count() > 0)
            {
                decimal days = 0;
                foreach (var ent in entsa)
                {
                    var entsb = from b in dal.GetObjects<T_HR_ADJUSTLEAVE>()
                                where b.EMPLOYEEID == employeeid && ent.T_HR_LEAVETYPESET.LEAVETYPESETID == b.LEAVETYPESETID
                                select b;
                    entsb = entsb.Where(m => m.BEGINTIME <= dttemp);
                    entsb = entsb.Where(m => m.ENDTIME >= dttemp);
                    if (entsb.Count() > 0)
                    {
                        days += Convert.ToDecimal(ent.LEAVEDAYS - entsb.FirstOrDefault().ADJUSTLEAVEDAYS);
                    }
                }
                return DaySalaryTotal(employeeid, year, month) * days;
            }
            return 0;
        }

        /// <summary>
        /// 计算请假扣款
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算请假扣款</returns>
        public decimal CalculationLeave(string employeeid, int year, int month)
        {
            try
            {
                T_HR_ATTENDMONTHLYBALANCE ent = GetMonthTotal(employeeid, year, month);
                return Math.Round(DaySalaryTotal(employeeid, year, month) * Convert.ToDecimal(ent.AFFAIRLEAVEDAYS + ent.SICKLEAVEDAYS) + OtherVacation(employeeid, year, month), nsize);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);

            }
            return 0;
        }

        /// <summary>
        /// 计算考勤异常扣款
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算考勤异常扣款</returns>
        public decimal AbnormalDeduct(string employeeid, int year, int month, decimal absentdays)
        {
            decimal absentDecuctValue = 0;
            try
            {
                if (salaryItemCaches.ContainsKey(employeeid + "absentDecuctValue"))
                {
                    absentDecuctValue = decimal.Parse(salaryItemCaches[employeeid + "absentDecuctValue"]);
                    SMT.Foundation.Log.Tracer.Debug("从缓存读取考勤异常扣款:" + absentDecuctValue.ToString());
                }
                else
                {
                    absentDecuctValue = AbsentDeduct(employeeid, "1", year, month) + AbsentDeduct(employeeid, "2", year, month) + AbsentDeduct(employeeid, "3", year, month);// +AbsentTotal(absentdays, employeeid, year, month);
                    salaryItemCaches.Add(employeeid + "absentDecuctValue", absentDecuctValue.ToString());
                }
                return absentDecuctValue;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                return 0;
            }
            //abnormal 迟到,1 早退,2未刷卡,3旷工4
        }

        /// <summary>
        /// 计算考勤异常次数
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算考勤异常次数</returns>
        public decimal AbnormalTimes(string employeeid, int year, int month)
        {
            try
            {
                T_HR_ATTENDMONTHLYBALANCE ent = GetMonthTotal(employeeid, year, month);
                return Convert.ToDecimal(ent.LATETIMES + ent.LEAVEEARLYTIMES + ent.FORGETCARDTIMES + ent.ABSENTDAYS);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                return 0;
            }
        }

        /// <summary>
        /// 计算考勤异常天数
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算考勤异常天数</returns>
        public decimal AbnormalTime(string employeeid, int year, int month)
        {
            try
            {
                T_HR_ATTENDMONTHLYBALANCE ent = GetMonthTotal(employeeid, year, month);
                return Convert.ToDecimal(ent.LATEDAYS + ent.LEAVEEARLYDAYS + ent.ABSENTDAYS);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                return 0;
            }
        }

        /// <summary>
        /// 计算自定义项金额
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算自定义项金额</returns>
        public decimal GetCustomerSum(string employeeid, int year, int month)
        {
            decimal result = 0;
            string years = year.ToString();
            string months = month.ToString();
            var ents = from a in dal.GetObjects<T_HR_CUSTOMGUERDONRECORD>()
                       where a.EMPLOYEEID == employeeid && a.SALARYYEAR == years && a.SALARYMONTH == months
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    result += Convert.ToDecimal(ent.SALARYSUM);
                }
            }
            return Math.Round(result, nsize);
        }

        /// <summary>
        /// 计算绩效奖金额
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算绩效奖金额</returns>
        public decimal GetPerformanceSum(string employeeid, int year, int month, decimal baseSalary)
        {
            decimal multiple = 1;//倍数
            string years = year.ToString();
            string months = month.ToString();
            var ents = from a in dal.GetObjects<T_HR_PERFORMANCEREWARDRECORD>()
                       where a.PERFORMANCEREWARDRECORDID == employeeid && a.CHECKSTATE == "2" && a.SALARYYEAR == years && a.SALARYMONTH == months
                       select a;
            if (ents.Count() > 0)
            {
                var ent = ents.FirstOrDefault();
                decimal result = 0;
                result += Convert.ToDecimal(baseSalary * multiple * PostCoefficient(employeeid) * ent.PERFORMANCESCORE);
                return Math.Round(result, nsize);
            }
            return 0;
        }
        /// <summary>
        /// 获取员工岗位系数
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回员工岗位系数</returns>
        public decimal PostCoefficient(string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                       join b in dal.GetObjects<T_HR_POST>() on a.T_HR_POST.POSTID equals b.POSTID
                       where a.T_HR_EMPLOYEE.EMPLOYEEID == employeeid
                       select new
                       {
                           POSTCOEFFICIENT = b.POSTCOEFFICIENT
                       };
            if (ents.Count() > 0) return Convert.ToDecimal(ents.FirstOrDefault().POSTCOEFFICIENT);
            return 0;
        }

        /// <summary>
        /// 计算公基金代扣
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回公基金代扣</returns>
        public decimal MoneyDeduct(string employeeid, int year, int month)
        {
            decimal? years = Convert.ToDecimal(year);
            decimal? months = Convert.ToDecimal(month);
            var ents = from a in dal.GetObjects<T_HR_PENSIONDETAIL>()
                       join b in dal.GetObjects<T_HR_PENSIONMASTER>() on a.CARDID equals b.CARDID
                       join c in dal.GetObjects<T_HR_EMPLOYEE>() on b.T_HR_EMPLOYEE.EMPLOYEEID equals c.EMPLOYEEID
                       where c.EMPLOYEEID == employeeid && a.PENSIONYEAR == years && a.PENSIONMOTH == months
                       select a;
            if (ents.Count() > 0)
            {
                return Convert.ToDecimal(ents.FirstOrDefault().HOUSINGFUNDCOMPANYCOST);
            }
            return 0;
        }

        /// <summary>
        /// 计算个人社保负担
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算个人社保负担</returns>
        public decimal ComputeSicost(string employeeid, int year, int month)
        {
            decimal? years = Convert.ToDecimal(year);
            decimal? months = Convert.ToDecimal(month);
            var ents = from a in dal.GetObjects<T_HR_PENSIONDETAIL>()
                       join b in dal.GetObjects<T_HR_PENSIONMASTER>() on a.CARDID equals b.CARDID
                       join c in dal.GetObjects<T_HR_EMPLOYEE>() on b.T_HR_EMPLOYEE.EMPLOYEEID equals c.EMPLOYEEID
                       where c.EMPLOYEEID == employeeid && a.PENSIONYEAR == years && a.PENSIONMOTH == months
                       select a;
            if (ents.Count() > 0)
            {
                return Convert.ToDecimal(ents.FirstOrDefault().TOTALPERSONCOST);
            }
            return 0;
        }

        /// <summary>
        /// 计算差额
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算差额</returns>
        public decimal ComputeBalance(string employeeid, decimal shouldPay, decimal insurance)
        {
            decimal? taxPay = 0;
            var ents = from b in dal.GetObjects<T_HR_SALARYSOLUTION>()
                       join c in dal.GetObjects<T_HR_SALARYARCHIVE>() on b.SALARYSOLUTIONID equals c.SALARYSOLUTIONID
                       where c.EMPLOYEEID == employeeid
                       select new
                       {
                           TAXESBASIC = b.TAXESBASIC,
                           SOLUTIONTAXESRATE = b.TAXESCOSTRATE
                       };
            try
            {
                if (ents.Count() > 0)
                {
                    taxPay = shouldPay * ents.FirstOrDefault().SOLUTIONTAXESRATE - insurance - ents.FirstOrDefault().TAXESBASIC;
                    return Math.Round(Convert.ToDecimal(taxPay), nsize);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                return 0;
            }
            return 0;
            #region -------
            //decimal result = 0;//decimal nowsalary, string employeeid, int year, int month,
            //string years = "0";
            //string months = "0";
            //if (month == 1)
            //{
            //    years = (year - 1).ToString();
            //    months = "12";
            //}
            //else 
            //{
            //    years = years.ToString();
            //    months = (month - 1).ToString();
            //}
            //var ents = from a in DataContext.T_HR_EMPLOYEESALARYRECORD
            //           where a.EMPLOYEEID == employeeid && a.SALARYYEAR == years && a.SALARYMONTH == months
            //           select a;
            //if (ents.Count() > 0)
            //{
            //    var ent = ents.FirstOrDefault();
            //    result = nowsalary - Convert.ToDecimal(ent.ACTUALLYPAY);
            //}
            //return  Math.Round(result,nsize);
            #endregion
        }


        /// <summary>
        /// 计算出差补助
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算出差补助</returns>
        public decimal GetEvectionMoney(string employeeid, int year, int month)
        {
            decimal result = 0;
            DateTime startdates = new DateTime(year, month, 1);
            DateTime enddates = new DateTime(year, month + 1, 1);
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEEVECTIONRECORD>()
                       where a.EMPLOYEEID == employeeid && a.STARTDATE >= startdates && a.ENDDATE <= enddates   //&&  a.CHECKSTATE == "2" 都是审核通过的
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    if (ent.SUBSIDYTYPE == "1")
                    {
                        result += Convert.ToDecimal(ent.SUBSIDYVALUE * ent.TOTALDAYS);
                    }
                    else
                    {
                        result += Convert.ToDecimal(ent.SUBSIDYVALUE);
                    }
                }
            }
            return Math.Round(result, nsize);
        }
        /// <summary>
        /// 计算出差天数
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算出差天数</returns>
        public decimal GetEvectionTimes(string employeeid, int year, int month)
        {
            decimal result = 0;
            DateTime startdates = new DateTime(year, month, 1);
            DateTime enddates = new DateTime(year, month + 1, 1);
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEEVECTIONRECORD>()
                       where a.EMPLOYEEID == employeeid && a.CHECKSTATE == "2" && a.STARTDATE >= startdates && a.ENDDATE <= enddates
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    result += Convert.ToDecimal(ent.TOTALDAYS);
                }
            }
            return Math.Round(result);
        }

        /// <summary>
        /// 计算加班费
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算加班费</returns>
        public decimal OverTimesMoney(string employeeid, int year, int month)
        {
            try
            {
                if (AbsentDeduct(employeeid, "5", year, month) == 1)
                {
                    return Math.Round(SystemBasicSalary(employeeid) / Convert.ToDecimal(21.75) * Convert.ToDecimal(GetMonthTotal(employeeid, year, month).OVERTIMESUMDAYS), nsize);
                    // return Math.Round((DaySalaryTotal(employeeid, year, month) / 480) * Convert.ToDecimal(GetMonthTotal(employeeid, year, month).OVERTIMESUMHOURS) * 60, nsize);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// 计算矿工扣款
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算矿工扣款</returns>
        public decimal AbsentTotal(decimal absentdaty, string employeeid, int year, int month)
        {
            try
            {
                return Math.Round(DaySalaryTotal(employeeid, year, month) * absentdaty * AbsentDeduct(employeeid, "4", year, month), nsize);
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug(e.Message);
                e.Message.ToString();
            }
            return 0;
        }

        /// <summary>
        /// 计算扣款
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算扣款</returns>
        public decimal AbsentDeduct(string employeeid, string abnormal, int year, int month)
        {
            //abnormal 迟到,1 早退,2漏打卡,3旷工4
            //var ents = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_COMPANY").Include("T_HR_DEPARTMENT").Include("T_HR_POST")
            //           where a.T_HR_EMPLOYEE.EMPLOYEEID == employeeid && a.CHECKSTATE == "2" && a.EDITSTATE == "1"
            //           select new
            //           {
            //               POSTID = a.T_HR_POST.POSTID,
            //               DEPARTMENTID = a.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
            //               COMPANYID = a.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID
            //           };

            //if (ents.Count() > 0)
            //{
            //    var ent = ents.FirstOrDefault();

            //    var entsp = from a in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
            //                where a.ASSIGNEDOBJECTID == ent.POSTID && a.CHECKSTATE == "2"
            //                select a;
            //    if (entsp.Count() > 0)
            //    {
            //        var entp = entsp.FirstOrDefault();
            //        return Minddlefun(employeeid, abnormal, entp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID, year, month);
            //    }
            //    var entsd = from a in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
            //                where a.ASSIGNEDOBJECTID == ent.DEPARTMENTID && a.CHECKSTATE == "2"
            //                select a;
            //    if (entsd.Count() > 0)
            //    {
            //        var entd = entsd.FirstOrDefault();
            //        return Minddlefun(employeeid, abnormal, entd.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID, year, month);
            //    }
            //    var entsc = from a in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
            //                where a.ASSIGNEDOBJECTID == ent.COMPANYID && a.CHECKSTATE == "2"
            //                select a;
            //    if (entsc.Count() > 0)
            //    {
            //        var entc = entsc.FirstOrDefault();
            //        return Minddlefun(employeeid, abnormal, entc.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID, year, month);
            //    }


            if (AttendsolutionForSalary != null)
            {
                return Minddlefun(employeeid, abnormal, AttendsolutionForSalary.ATTENDANCESOLUTIONID, year, month);
            }

            #region
            //DateTime dtAttendRdStart = new DateTime(year, month, 1);
            //DateTime dtAttendRdEnd = dtAttendRdStart.AddMonths(1).AddDays(-1);

            //AttendanceRecordBLL attrecordBll = new AttendanceRecordBLL();
            //var attrecord = attrecordBll.GetAttendanceRecordByEmployeeIDAndDate(employeeid, dtAttendRdStart, dtAttendRdEnd).FirstOrDefault();
            //if (attrecord != null)
            //{
            //    //AttendanceSolutionBLL attasBll = new AttendanceSolutionBLL();
            //    //var ent = attasBll.GetAttendanceSolutionByID(attrecord.FirstOrDefault().ATTENDANCESOLUTIONID);

            //    //if (ent != null)
            //    //{
            //    return Minddlefun(employeeid, abnormal, attrecord.ATTENDANCESOLUTIONID, year, month);
            //    // }
            //}
            //else
            //{
            //    AttendanceSolutionBLL solutionBll = new AttendanceSolutionBLL();
            //    var anOtherSolution = solutionBll.GetAttendanceSolutionByEmployeeID(employeeid);
            //    if (anOtherSolution != null)
            //    {
            //        return Minddlefun(employeeid, abnormal, anOtherSolution.ATTENDANCESOLUTIONID, year, month);
            //    }
            //}
            #endregion
            //#region 员工级别的考勤运算
            //var ente = from a in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
            //           select a;
            //bool sign = false;
            //string aid = string.Empty;
            //foreach (var e in ente)
            //{
            //    if (e.ASSIGNEDOBJECTID.IndexOf(employeeid) != -1)
            //    {
            //        sign = true;
            //        aid = e.ASSIGNEDOBJECTID;
            //        break;
            //    }
            //}
            //if (sign)
            //{
            //    sign = false;
            //    return Minddlefun(employeeid, abnormal, aid, year, month);
            //}
            //#endregion

            // }
            return 0;
        }

        public decimal Minddlefun(string employeeid, string abnormal, string attendanceSolutionID, int year, int month)
        {
            //abnormal 迟到,1 早退,2漏打卡,3旷工4
            decimal result = 0;
            T_HR_ATTENDMONTHLYBALANCE ent = GetMonthTotal(employeeid, year, month);
            try
            {
                switch (Convert.ToInt32(abnormal))
                {
                    case 1:
                        result = GetDeduct(attendanceSolutionID, abnormal, employeeid, Convert.ToDecimal(ent.LATETIMES), year, month);
                        break;
                    case 2:
                        result = GetDeduct(attendanceSolutionID, abnormal, employeeid, Convert.ToDecimal(ent.LEAVEEARLYTIMES), year, month);
                        break;
                    case 3:
                        result = GetDeduct(attendanceSolutionID, abnormal, Convert.ToDecimal(ent.FORGETCARDTIMES));
                        break;
                    case 4:
                        result = GetRatio(attendanceSolutionID);
                        break;
                    case 5:
                        result = GetIsOverTimes(attendanceSolutionID);
                        break;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                result = 0;
            }
            return result;
        }

        public decimal GetIsOverTimes(string attendanceSolutionID)
        {
            //加班方式确定返回1要计费,0则否
            var ents = from a in dal.GetObjects<T_HR_ATTENDANCESOLUTION>()
                       where a.ATTENDANCESOLUTIONID == attendanceSolutionID && a.OVERTIMEPAYTYPE == "2" && a.CHECKSTATE == "2"
                       select a;
            if (ents.Count() > 0)
            {
                return 1;
            }
            return 0;
        }

        public decimal GetRatio(string attendanceSolutionID)
        {
            //计算旷工扣款的系数
            var ents = from a in dal.GetObjects<T_HR_ATTENDANCESOLUTIONDEDUCT>()
                       join b in dal.GetObjects<T_HR_ATTENDANCEDEDUCTMASTER>() on a.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID equals b.DEDUCTMASTERID
                       join c in dal.GetObjects<T_HR_ATTENDANCEDEDUCTDETAIL>() on a.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID equals c.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID
                       where a.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == attendanceSolutionID && b.ATTENDABNORMALTYPE == "4"
                       orderby c.FINESUM
                       select new
                       {
                           FINERATIO = c.FINERATIO,
                       };
            if (ents.Count() > 0)
            {
                var ent = ents.FirstOrDefault();
                return Convert.ToDecimal(ent.FINERATIO);
            }
            return 0;
        }

        public decimal GetDeduct(string attendanceSolutionID, string abnormal, decimal times)
        {
            //计算漏打卡扣款
            //abnormal 迟到,1 早退,2漏打卡,3旷工4 
            if (times <= 0) //无漏打卡 不需要计算
                return 0;
            decimal result = 0;
            var ents = from a in dal.GetObjects<T_HR_ATTENDANCESOLUTIONDEDUCT>()
                       join b in dal.GetObjects<T_HR_ATTENDANCEDEDUCTMASTER>() on a.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID equals b.DEDUCTMASTERID
                       join c in dal.GetObjects<T_HR_ATTENDANCEDEDUCTDETAIL>() on a.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID equals c.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID
                       where a.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == attendanceSolutionID && b.ATTENDABNORMALTYPE == abnormal
                       orderby c.LOWESTTIMES ascending
                       select new TempType
                       {
                           FINERATIO = c.FINERATIO,
                           FINESUM = c.FINESUM,
                           LOW = c.PARAMETERVALUE,
                           MAINTYPE = b.ATTENDABNORMALTYPE,
                           DETAILTYPE = c.FINETYPE,
                           LOWESTTIMES = c.LOWESTTIMES,
                           HIGHESTTIMES = c.HIGHESTTIMES,
                           ISACCUMULATING = b.ISACCUMULATING,     //(1.是否2是累加)
                           ALLOWLOSTCARDTIMES = a.T_HR_ATTENDANCESOLUTION.ALLOWLOSTCARDTIMES   //每月允许漏打卡次数
                       };
            if (ents.Count() > 0 && ents.FirstOrDefault().ALLOWLOSTCARDTIMES < times)
            {
                List<TempType> entlist = new List<TempType>();

                foreach (var entt in ents)
                {
                    entlist.Add(entt);
                }
                //漏打卡的算法 (考勤异常扣款设置里只设置要扣钱的从0次开始,不扣钱的只要在考勤方案设置,设置不扣钱的次数即可)
                if (entlist[0].ISACCUMULATING == "2")
                {
                    int i = Convert.ToInt32(times - entlist[0].ALLOWLOSTCARDTIMES);
                    foreach (var ent in entlist)
                    {
                        if (ent.MAINTYPE == "3")
                        {
                            if (i >= ent.HIGHESTTIMES)
                                result += Convert.ToDecimal((ent.HIGHESTTIMES - ent.LOWESTTIMES) * ent.FINESUM);
                            else
                                result += Convert.ToDecimal((i - ent.LOWESTTIMES + 1) * ent.FINESUM);
                        }
                        if (i <= ent.HIGHESTTIMES) break;
                    }

                    //foreach (var ent in entlist)
                    //{
                    //    if (ent.MAINTYPE == "3")
                    //    {
                    //        if (times >= ent.HIGHESTTIMES)
                    //        {
                    //            result += Convert.ToDecimal((ent.HIGHESTTIMES - ent.LOWESTTIMES) * ent.FINESUM);
                    //        }
                    //        else if (times >= ent.LOWESTTIMES)
                    //        {
                    //            result += Convert.ToDecimal((times - ent.LOWESTTIMES + 1) * ent.FINESUM);
                    //        }
                    //        else
                    //        {
                    //            break;
                    //        }
                    //    }
                    //}
                    return Math.Round(result, nsize);

                }
                else
                {
                    foreach (var e in entlist)
                    {
                        if (e.LOWESTTIMES <= times && e.HIGHESTTIMES >= times)
                            return Math.Round(Convert.ToDecimal(e.FINESUM), nsize);
                        //return Math.Round(Convert.ToDecimal(entlist[Convert.ToInt32(times) - 1].FINESUM), nsize);
                    }
                }

                #region 有限段算法
                //int i = 0;
                //foreach (var entt in ents)
                //{
                //    if (i == 0) 
                //    {
                //        ent = entt;
                //    }
                //    if (i == 1)
                //    {
                //        entTwo = entt; 
                //    }
                //    if (i == 2)
                //    {
                //        entThree = entt; break;
                //    }
                //    i++;
                //}
                ////漏打卡的算法
                //if (ent.MAINTYPE == "3") 
                //{
                //    if (times > Convert.ToDecimal(ent.HIGHESTTIMES) && times < Convert.ToDecimal(entTwo.HIGHESTTIMES))
                //    {
                //       return  Math.Round((times - Convert.ToDecimal(ent.HIGHESTTIMES)) * Convert.ToDecimal(entTwo.FINESUM),nsize);
                //    }
                //    if (times > Convert.ToDecimal(entTwo.HIGHESTTIMES))
                //    {
                //        return Math.Round(Convert.ToDecimal(entThree.LOWESTTIMES - entTwo.LOWESTTIMES) * Convert.ToDecimal(entTwo.FINESUM) + (times - Convert.ToDecimal(entThree.LOWESTTIMES)) * Convert.ToDecimal(entThree.FINESUM),nsize);
                //    }
                //}
                #endregion
            }
            return 0;
        }

        public decimal GetDeduct(string attendanceSolutionID, string abnormal, string employeeid, decimal times, int year, int month)
        {
            //abnormal 迟到,1 早退,2漏打卡,3旷工4
            decimal? result = 0;
            var ents = from a in dal.GetObjects<T_HR_ATTENDANCESOLUTIONDEDUCT>()
                       join b in dal.GetObjects<T_HR_ATTENDANCEDEDUCTMASTER>() on a.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID equals b.DEDUCTMASTERID
                       join c in dal.GetObjects<T_HR_ATTENDANCEDEDUCTDETAIL>() on a.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID equals c.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID
                       where a.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == attendanceSolutionID && b.ATTENDABNORMALTYPE == abnormal
                       orderby c.LOWESTTIMES ascending
                       select new TempType
                       {
                           FINERATIO = c.FINERATIO,
                           FINESUM = c.FINESUM,
                           LOW = c.PARAMETERVALUE,
                           MAINTYPE = b.ATTENDABNORMALTYPE,
                           DETAILTYPE = c.FINETYPE,
                           LOWESTTIMES = c.LOWESTTIMES,
                           HIGHESTTIMES = c.HIGHESTTIMES,
                           ISACCUMULATING = b.ISACCUMULATING,    //(1.是否2是累加)
                           ALLOWLATEMAXTIMES = a.T_HR_ATTENDANCESOLUTION.ALLOWLATEMAXTIMES,   //每月允许迟到次数
                           ALLOWLATEMAXMINUTE = a.T_HR_ATTENDANCESOLUTION.ALLOWLATEMAXMINUTE  //每月允许迟到总时间
                       };

            if (ents.Count() > 0 && ents.FirstOrDefault().ALLOWLATEMAXTIMES < times)
            {
                List<TempType> tpList = new List<TempType>();

                foreach (var en in ents)
                {
                    tpList.Add(en);
                }

                //迟到与早退的算法 1-4迟到,5-8早退

                if ((tpList[0].MAINTYPE == "1" && tpList[0].DETAILTYPE == "1") || (tpList[0].MAINTYPE == "2" && tpList[0].DETAILTYPE == "5"))
                    return Math.Round(times * Convert.ToDecimal(tpList[0].FINESUM), nsize);
                if (tpList[0].MAINTYPE == "1" && tpList[0].DETAILTYPE == "2" || tpList[0].MAINTYPE == "2" && tpList[0].DETAILTYPE == "6")
                {
                    decimal resultSum = Math.Round((DaySalaryTotal(employeeid, year, month) / (8 * 60)) * GetMinutes(employeeid, abnormal, tpList[0].DETAILTYPE, 0, year, month), nsize);
                    if (resultSum < Convert.ToDecimal(tpList[0].FINESUM)) return Convert.ToDecimal(tpList[0].FINESUM);
                    return Math.Round(resultSum, nsize);
                }
                if ((tpList[0].MAINTYPE == "1" && tpList[0].DETAILTYPE == "3") || (tpList[0].MAINTYPE == "2" && tpList[0].DETAILTYPE == "7"))
                {
                    if (times <= Convert.ToDecimal(tpList[0].HIGHESTTIMES))
                        return Math.Round((DaySalaryTotal(employeeid, year, month) / (8 * 60)) * GetMinutes(employeeid, abnormal, tpList[0].DETAILTYPE, Convert.ToInt32(tpList[0].HIGHESTTIMES), year, month), nsize);
                    else
                    {
                        for (int i = 0; i < tpList.Count; i++)
                        {

                            if (i == 0)
                            {
                                result = Math.Round((DaySalaryTotal(employeeid, year, month) / (8 * 60)) * GetMinutes(employeeid, abnormal, tpList[i].DETAILTYPE, Convert.ToInt32(tpList[i].HIGHESTTIMES), year, month), nsize);
                            }
                            else
                            {
                                result += (tpList[i].HIGHESTTIMES - tpList[i].LOWESTTIMES) * tpList[i].FINESUM;
                            }
                        }
                        return Math.Round(Convert.ToDecimal(result), nsize);
                    }
                }
                if ((tpList[0].MAINTYPE == "1" && tpList[0].DETAILTYPE == "4") || (tpList[0].MAINTYPE == "2" && tpList[0].DETAILTYPE == "8"))
                {
                    if (tpList[0].ISACCUMULATING == "2")
                    {
                        int i = Convert.ToInt32(times - tpList[0].ALLOWLATEMAXTIMES);
                        foreach (var ent in tpList)
                        {
                            if (i <= 0) break;
                            result += (ent.HIGHESTTIMES - ent.LOWESTTIMES) * ent.FINESUM;
                            i = i - Convert.ToInt32(ent.HIGHESTTIMES - ent.LOWESTTIMES);
                        }
                        return Math.Round(Convert.ToDecimal(result), nsize);



                        //if (times <= Convert.ToDecimal(tpList[0].HIGHESTTIMES))
                        //    return Math.Round(times * Convert.ToDecimal(tpList[0].FINESUM), nsize);
                        //else
                        //{
                        //    foreach (var ent in tpList)
                        //    {
                        //        result += (ent.HIGHESTTIMES - ent.LOWESTTIMES) * ent.FINESUM;
                        //    }
                        //    return Math.Round(Convert.ToDecimal(result), nsize);
                        //}
                    }
                    else
                    {
                        foreach (var e in tpList)
                        {
                            if (e.HIGHESTTIMES == times)
                                return Math.Round(Convert.ToDecimal(e.FINESUM), nsize);
                        }
                        // return Math.Round(Convert.ToDecimal(tpList[Convert.ToInt32(times) - 1].FINESUM), nsize);
                    }
                }

            }

            #region  有限段算法
            //if (ents.Count()> 0) 
            //{
            //    var ent = ents.FirstOrDefault();
            //    var entTwo = ents.FirstOrDefault();
            //    int i=0;
            //    foreach(var entt in ents)
            //    {
            //        if (i == 0)
            //        {
            //            ent = entt;
            //        }
            //        if (i == 1)
            //        {
            //            entTwo = entt; break;
            //        }
            //        i++;
            //    }
            //    //迟到与早退的算法 1-4迟到,5-8早退
            //    //if (ent.MAINTYPE == "1" || ent.MAINTYPE == "2") 
            //    //{
            //        if ((ent.MAINTYPE == "1" && ent.DETAILTYPE == "1") || (ent.MAINTYPE == "2" && ent.DETAILTYPE == "5"))
            //            return Math.Round(times * Convert.ToDecimal(ent.FINESUM), nsize);
            //        if (ent.MAINTYPE == "1" && ent.DETAILTYPE == "2" || ent.MAINTYPE == "2" && ent.DETAILTYPE == "6")                                            
            //        {
            //            decimal resultSum = Math.Round((DaySalaryTotal(employeeid, year, month) / (8 * 60)) * GetMinutes(employeeid, abnormal, ent.DETAILTYPE, 0, year, month), nsize);
            //            if (resultSum < Convert.ToDecimal(ent.FINESUM)) return Convert.ToDecimal(ent.FINESUM);
            //            return Math.Round(resultSum, nsize);
            //        }
            //        if ((ent.MAINTYPE == "1" && ent.DETAILTYPE == "3") || (ent.MAINTYPE == "2" && ent.DETAILTYPE == "7"))
            //        {
            //            if (times <= Convert.ToDecimal(ent.HIGHESTTIMES))
            //                return Math.Round((DaySalaryTotal(employeeid, year, month) / (8 * 60)) * GetMinutes(employeeid, abnormal,ent.DETAILTYPE, Convert.ToInt32(ent.HIGHESTTIMES), year, month), nsize);
            //            else
            //                return Math.Round((DaySalaryTotal(employeeid, year, month) / (8 * 60)) * GetMinutes(employeeid, abnormal, ent.DETAILTYPE, Convert.ToInt32(ent.HIGHESTTIMES), year, month)+(times - Convert.ToDecimal(ent.HIGHESTTIMES)) * Convert.ToDecimal(entTwo.FINESUM), nsize);
            //        }
            //        if ((ent.MAINTYPE == "1" && ent.DETAILTYPE == "4") || (ent.MAINTYPE == "2" && ent.DETAILTYPE == "8"))
            //        {
            //            if (times <= Convert.ToDecimal(ent.HIGHESTTIMES))
            //                return Math.Round(times * Convert.ToDecimal(ent.FINESUM), nsize);
            //            else
            //                return Math.Round(times * Convert.ToDecimal(entTwo.FINESUM), nsize);
            //        }

            //    //}

            //}
            #endregion
            return 0;
        }

        /// <summary>
        /// 统计月度迟到或者早退的迟到分钟数
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <param name="abnormal">异常类型</param>
        /// <param name="ntimes">考勤边界次数</param>
        /// <param name="year">考勤年份 new T_HR_EMPLOYEEABNORMRECORD</param>
        /// <param name="month">考勤月份</param>
        /// <returns>返回统计月度迟到或者早退的迟到分钟数  old  T_HR_EMPLOYEESIGNINDETAIL</returns>
        public decimal GetMinutes(string employeeid, string abnormal, string detailtype, int ntimes, int year, int month)
        {
            decimal getmin = 0;
            DateTime starttime = new DateTime(year, month, 1);
            DateTime endtime = new DateTime(year, month + 1, 1);
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEABNORMRECORD>().Include("T_HR_ATTENDANCERECORD")
                       where a.T_HR_ATTENDANCERECORD.EMPLOYEEID == employeeid
                       select a;
            if (ents.Count() > 0 && (abnormal == "1" || abnormal == "2"))
            {
                if (abnormal == "1")
                    ents = ents.Where(m => m.ABNORMCATEGORY == "1");
                if (abnormal == "2")
                    ents = ents.Where(m => m.ABNORMCATEGORY == "2");
                ents = ents.Where(m => m.ABNORMALDATE >= starttime);
                ents = ents.Where(m => m.ABNORMALDATE < endtime);
                ents = ents.OrderBy(x => x.ABNORMALDATE);
                if (ntimes > 0 && (detailtype == "3" || detailtype == "7"))
                    ents = ents.Take(ntimes);
                if (ents.Count() > 0)
                {
                    foreach (var ent in ents)
                    {
                        getmin += Convert.ToDecimal(ent.ABNORMALTIME);
                    }
                }
            }
            return Math.Round(getmin * 60, nsize);
        }

        /// <summary>
        /// 考勤月度结算信息
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回考勤月度结算信息</returns>
        public T_HR_ATTENDMONTHLYBALANCE GetMonthTotal(string employeeid, int year, int month)
        {
            var ents = from a in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>()
                       where a.EMPLOYEEID == employeeid && a.CHECKSTATE == "2"
                       select a;
            T_HR_ATTENDMONTHLYBALANCE ent = new T_HR_ATTENDMONTHLYBALANCE();
            //ent.ABSENTDAYS = 0;
            //ent.NEEDATTENDDAYS = 0;
            //ent.REALATTENDDAYS = 0;
            //ent.OVERTIMESUMHOURS = 0;
            //ent.AFFAIRLEAVEDAYS = 0;
            //ent.SICKLEAVEDAYS = 0;
            //ent.OTHERLEAVEDAYS = 0;
            if (ents.Count() > 0)
            {
                ents = ents.Where(m => m.BALANCEYEAR == year);
                ents = ents.Where(m => m.BALANCEMONTH == month);
                if (ents.Count() > 0)
                {
                    ent = ents.FirstOrDefault();
                    return ent;
                }
            }
            return null;
        }

        /// <summary>
        /// 最初的薪资  员工工资的结算基本数据
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回最初的薪资</returns>
        public decimal InitialSalary(string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                       join b in dal.GetObjects<T_HR_POST>() on a.T_HR_POST.POSTID equals b.POSTID
                       join c in dal.GetObjects<T_HR_POSTDICTIONARY>() on b.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals c.POSTDICTIONARYID
                       join d in dal.GetObjects<T_HR_POSTLEVELDISTINCTION>() on c.POSTLEVEL equals d.POSTLEVEL
                       where a.T_HR_EMPLOYEE.EMPLOYEEID == employeeid   //&& a.ISAGENCY == "0"    ISAGENCY为0表示正式职位
                       select new
                       {
                           d.POSTLEVELID,
                           d.BASICSALARY,
                           d.LEVELBALANCE,
                           c.SALARYLEVEL
                       };
            if (ents.Count() > 0)
            {
                var ent = ents.FirstOrDefault();
                var entt = from e in dal.GetObjects<T_HR_SALARYLEVEL>()
                           where e.T_HR_POSTLEVELDISTINCTION.POSTLEVELID == ent.POSTLEVELID && e.SALARYLEVEL == ent.SALARYLEVEL
                           select new
                           {
                               e.T_HR_POSTLEVELDISTINCTION.POSTLEVELID,
                               e.SALARYLEVEL,
                               e.SALARYSUM
                           };
                if (entt.Count() > 0) return Convert.ToDecimal(entt.FirstOrDefault().SALARYSUM);  //直接获取库内工资
                // return Math.Round(Convert.ToDecimal(ent.BASICSALARY) + Convert.ToDecimal(ent.LEVELBALANCE) * (40 - Convert.ToDecimal(ent.SALARYLEVEL)),nsize);//直接计算薪资最初工资
            }
            return 0;
        }



        /// <summary>
        /// 计算日薪
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回日薪</returns>
        public decimal DaySalaryTotal(string employeeid, int year, int month)
        {
            var ent = GetMonthTotal(employeeid, year, month);
            if (ent != null)
            {
                return Math.Round(SystemBasicSalary(employeeid) * 35 / 100 / Convert.ToDecimal(ent.NEEDATTENDDAYS), nsize);
            }
            return 0;
        }

        /// <summary>
        /// 基本薪资   
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回基本薪资</returns>
        public decimal BasicSalary(string employeeid)
        {
            SalaryArchiveBLL bll = new SalaryArchiveBLL();
            try
            {
                T_HR_SALARYARCHIVE archive = bll.GetSalaryArchiveByEmployeeID(employeeid);
                if (archive != null) return Convert.ToDecimal(archive.BASESALARY);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);

                return 0;
            }
            return 0;
        }
        public decimal BasicSalary2(string employeeid)                  //暂时停用
        {
            return Math.Round(InitialSalary(employeeid) * 35 / 100, nsize);
        }

        /// <summary>
        /// 出勤工资统计
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回出勤工资</returns>
        //public decimal WorkingSalaryTotal(decimal absencedays, string employeeid, int year, int month)
        //{
        //    decimal? worksalary = 0;
        //    try
        //    {
        //        var ent = GetMonthTotal(employeeid, year, month);
        //        if (ent != null && ent.NEEDATTENDDAYS != 0)
        //        {
        //            if (getREALATTENDDAYS(employeeid).WORKDAYTYPE == "1")
        //            {
        //                // 标准薪资+地区差异补贴+餐费补贴  -  标准薪资+地区差异补贴+餐费补贴  /21.75*缺勤天数+加班费+值班津贴+假期其它扣款
        //                worksalary = (SystemBasicSalary(employeeid) + AreaSubsidy(employeeid) + GetMealAllowance(employeeid));
        //                worksalary -= (SystemBasicSalary(employeeid) + AreaSubsidy(employeeid) + GetMealAllowance(employeeid)) / Convert.ToDecimal(21.75) * absencedays;
        //            }
        //            else
        //            {
        //                worksalary = (SystemBasicSalary(employeeid) + AreaSubsidy(employeeid) + GetMealAllowance(employeeid));
        //                worksalary -= (SystemBasicSalary(employeeid) + AreaSubsidy(employeeid) + GetMealAllowance(employeeid)) / Convert.ToDecimal(21.75) * absencedays;
        //            }
        //            //return Math.Round((SystemBasicSalary(employeeid) + AreaSubsidy(employeeid) + GetMealAllowance(employeeid)) / Convert.ToDecimal(ent.NEEDATTENDDAYS) * Convert.ToDecimal(ent.REALATTENDDAYS), nsize);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
        //    }
        //    if (worksalary < 0) worksalary = 0;
        //    return Math.Round(Convert.ToDecimal(worksalary), nsize);
        //}

        private AttendanceSolutionBLL asbll = new AttendanceSolutionBLL();
        public T_HR_ATTENDANCESOLUTION getREALATTENDDAYS(string strEmployeeID)
        {
            T_HR_ATTENDANCESOLUTION sol = new T_HR_ATTENDANCESOLUTION();
            try
            {

                sol = asbll.GetAttendanceSolutionByEmployeeID(strEmployeeID);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);

            }
            return sol;
        }

        /// <summary>
        /// 薪资体系薪资   
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回薪资体系薪资</returns>
        public decimal SystemBasicSalary(string employeeid)
        {
            SalaryArchiveBLL bll = new SalaryArchiveBLL();
            try
            {
                string salaryArchiveID = SalaryArchiveId;// bll.GetSalaryArchiveApprovedByEmployeeID(employeeid).SALARYARCHIVEID;
                var ents = from a in dal.GetObjects<T_HR_SALARYSTANDARD>()
                           join b in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.SALARYSTANDARDID equals b.T_HR_SALARYSTANDARD.SALARYSTANDARDID
                           where b.EMPLOYEEID == employeeid && b.SALARYARCHIVEID == salaryArchiveID
                           select new
                           {
                               SYSTEMBASESALARY = a.BASESALARY
                           };
                if (ents.Count() > 0)
                {
                    return Convert.ToDecimal(ents.FirstOrDefault().SYSTEMBASESALARY);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);

                return 0;
            }
            return 0;
        }

        /// <summary>
        /// 计算项的值
        /// </summary>
        /// <param name="itemid">薪资项的ID</param>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回计算项的值</returns>
        public decimal AutoCalItem(string itemid, string employeeid)
        {
            decimal temp = 0;
            var ents = from a in dal.GetObjects<T_HR_SALARYITEM>()
                       where a.SALARYITEMID == itemid
                       select a;
            if (ents.Count() > 0)
            {
                int i = 0;
                if (string.IsNullOrEmpty(ents.FirstOrDefault().CALCULATEFORMULACODE))
                {
                    if (!string.IsNullOrEmpty(ents.FirstOrDefault().GUERDONSUM.ToString()))
                        return Convert.ToDecimal(ents.FirstOrDefault().GUERDONSUM);
                    else
                        return 0;
                }
                string[] ent = ents.FirstOrDefault().CALCULATEFORMULACODE.Split(',');
                foreach (string e in ent)
                {
                    if (!CalPartValue(e) && !string.IsNullOrEmpty(e))
                    {
                        if (e.Length > (31 + 1))    //1主要是为了过滤标准薪资
                        {
                            if (i > 0)
                                temp = CalPartValue(temp, AutoCalItem(e, employeeid), ent[i - 1]);
                            else
                                temp += AutoCalItem(e, employeeid);
                        }
                        else if (e == GetBzxzid())
                        {
                            if (i > 0)
                                temp = CalPartValue(temp, SystemBasicSalary(employeeid), ent[i - 1]);
                            else
                                temp += SystemBasicSalary(employeeid);
                        }
                        else
                        {
                            if (i > 0)
                                temp = CalPartValue(temp, Convert.ToDecimal(e), ent[i - 1]);
                            else
                                temp += Convert.ToDecimal(e);
                        }
                    }
                    i++;
                }
                return temp;
            }
            return 0;
        }

        /// <summary>
        /// 计算项的值
        /// </summary>
        /// <param name="itemid">薪资项的ID</param>
        /// <param name="bzxzValue">标准薪资值</param>
        /// <returns>返回计算项的值</returns>
        public decimal AutoCalItem(string itemid, decimal bzxzValue)
        {
            decimal temp = 0;
            var ents = from a in dal.GetObjects<T_HR_SALARYITEM>()
                       where a.SALARYITEMID == itemid
                       select a;
            if (ents.Count() > 0)
            {
                int i = 0;
                if (string.IsNullOrEmpty(ents.FirstOrDefault().CALCULATEFORMULACODE))
                {
                    if (!string.IsNullOrEmpty(ents.FirstOrDefault().GUERDONSUM.ToString()))
                        return Convert.ToDecimal(ents.FirstOrDefault().GUERDONSUM);
                    else
                        return 0;
                }
                string[] ent = ents.FirstOrDefault().CALCULATEFORMULACODE.Split(',');
                foreach (string e in ent)
                {
                    if (!CalPartValue(e) && !string.IsNullOrEmpty(e))
                    {
                        if (e.Length > (31 + 1))    //1主要是为了过滤标准薪资
                        {
                            if (i > 0)
                                temp = CalPartValue(temp, AutoCalItem(e, bzxzValue), ent[i - 1]);
                            else
                                temp += AutoCalItem(e, bzxzValue);
                        }
                        else if (e == GetBzxzid())
                        {
                            if (i > 0)
                                temp = CalPartValue(temp, bzxzValue, ent[i - 1]);
                            else
                                temp += bzxzValue;
                        }
                        else
                        {
                            if (i > 0)
                                temp = CalPartValue(temp, Convert.ToDecimal(e), ent[i - 1]);
                            else
                                temp += Convert.ToDecimal(e);
                        }
                    }
                    i++;
                }
                return temp;
            }
            return 0;
        }

        public decimal GetBzxzValue(string employeeid, string salryarchiveID)
        {
            //return 1000;
            return SystemBasicSalary(employeeid);
        }

        public bool CalPartValue(string symbol)
        {
            bool result = false;
            switch (symbol)
            {
                case "+":
                case "-":
                case "X":
                case "/":
                    result = true;
                    break;
                case "":
                    break;
                //case "(":
                //    break;
                //case ")":
                //    break;
            }
            return result;
        }

        public decimal CalPartValue(decimal fristvalue, decimal nextvalue, string symbol)
        {
            switch (symbol)
            {
                case "":
                case "+":
                    fristvalue += nextvalue;
                    break;
                case "-":
                    fristvalue -= nextvalue;
                    break;
                case "X":
                    fristvalue = fristvalue * nextvalue;
                    break;
                case "/":
                    fristvalue = fristvalue / nextvalue;
                    break;
                //case "(":
                //    amount = amount(list.GUERDONSUM;
                //    break;
                //case ")":
                //    amount = amount)list.GUERDONSUM;
                //    break;
            }
            return fristvalue;
        }

        /// <summary>
        /// 返回"bzxzbzxzbzxz"
        /// </summary>
        /// <returns></returns>
        public string GetBzxzid()
        {                              //得到标准薪资ID
            string BZXZ = "bzxz";
            for (int i = 0; i < 3; i++)
            {
                BZXZ += BZXZ;
            }
            return BZXZ;
        }


        /// <summary>
        /// 计算住房扣款
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回住房扣款</returns>
        public decimal HousingDeduct(string employeeid, int year, int month)
        {                                                                            //PROJECTCODE为-1的是住房扣款
            string years, months;
            try
            {
                years = year.ToString();
                months = month.ToString();
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                           join b in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.EMPLOYEECODE equals b.EMPLOYEECODE
                           where b.EMPLOYEEID == employeeid && a.DEALYEAR == years && a.DEALMONTH == months && a.PROJECTCODE == "-1"
                           select a;
                if (ents.Count() > 0)
                {
                    return Convert.ToDecimal(ents.FirstOrDefault().PROJECTMONEY);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);

            }
            return 0;
        }

        /// <summary>
        /// 计算地区差异补贴
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回地区差异补贴</returns>
        public decimal AreaSubsidy()
        {
            //SalaryArchiveBLL bll = new SalaryArchiveBLL();
            //T_HR_SALARYARCHIVE salarychive = bll.GetSalaryArchiveApprovedByEmployeeID(employeeid);
            //if (salarychive == null) return 0;
            var en = from m in dal.GetObjects<T_HR_SALARYSOLUTION>()
                     where m.SALARYSOLUTIONID == archiveForAreaAllowrance.SALARYSOLUTIONID
                     select new
                     {
                         AREADIFFERENCEID = m.T_HR_AREADIFFERENCE.AREADIFFERENCEID
                     };

            string areadif = "";
            try
            {
                if (en.Count() > 0)
                    areadif = en.FirstOrDefault().AREADIFFERENCEID;
                else return 0;


                string postLevel = archiveForAreaAllowrance.POSTLEVEL.ToString();
                var ents = from a in dal.GetObjects<T_HR_AREAALLOWANCE>()
                           where a.POSTLEVEL == postLevel && a.T_HR_AREADIFFERENCE.AREADIFFERENCEID == areadif
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    return Convert.ToDecimal(ent.ALLOWANCE);
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);

            }
            return 0;
        }

        /// <summary>
        /// 还款
        /// </summary>
        /// <returns>返回还款</returns>
        public void Repayment(int objectType, string objectID, int year, int month)
        {
            SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[] debtinfolist;
            string strBalanceEmployeeID = string.Empty;
            V_RETURNFBI glist = GenerateSalaryRecord(10,string.Empty,string.Empty,strBalanceEmployeeID, objectType, objectID, year, month, true).FirstOrDefault();
            List<T_HR_EMPLOYEESALARYRECORD> salaryrecords = new List<T_HR_EMPLOYEESALARYRECORD>();
            if (glist != null)
            {//查询还款人,获取借款信息(eid,money)
                try
                {
                    int i = 0;
                    //debtinfolist = FBSclient.GetBorrowers(glist.COMPANYID, glist.DEPARTMENTID);
                    debtinfolist = FBSclient.GetBorrowers(glist.COMPANYID, string.Empty); //有用方法
                    SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[] listDebt = new SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[debtinfolist.Count()];
                    SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[] listDebtRes = new SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[debtinfolist.Count()];
                    foreach (var v in debtinfolist)                                      //打包实发工资
                    {
                        var ent = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                  where a.EMPLOYEEID == v.EmployeeID
                                  select a;
                        try
                        {
                            if (ent.Count() > 0) salaryrecords.Add(ent.FirstOrDefault());
                            listDebt[i].EmployeeID = v.EmployeeID;
                            listDebt[i].UsableSalary = Convert.ToDecimal(ent.FirstOrDefault().ACTUALLYPAY == null ? "0" : ent.FirstOrDefault().ACTUALLYPAY);
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                        }
                        i++;
                    }

                    listDebtRes = FBSclient.RepayBySalary(listDebt, SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceRepayType.Plan);   //发送工资,获取预还款额
                    EmployeeAddSumBLL addbll = new EmployeeAddSumBLL();       //添加员工加扣款的还款
                    foreach (var v in listDebtRes)
                    {
                        T_HR_EMPLOYEEADDSUM addsum = new T_HR_EMPLOYEEADDSUM();
                        addsum.ADDSUMID = v.OrderID;                          //预算还款单号
                        addsum.DEALMONTH = month.ToString();
                        addsum.DEALYEAR = year.ToString();
                        addsum.EMPLOYEEID = v.EmployeeID;
                        addsum.PROJECTCODE = "-3";
                        addsum.PROJECTMONEY = v.Debt;
                        addsum.CHECKSTATE = "2";
                        addsum.CREATEDATE = System.DateTime.Now;
                        //权限字段
                        try
                        {
                            addsum.CREATEUSERID = construe[0];
                            addsum.CREATEPOSTID = construe[1];
                            addsum.CREATEDEPARTMENTID = construe[2];
                            addsum.CREATECOMPANYID = construe[3];

                            addsum.OWNERID = construe[4];
                            addsum.OWNERPOSTID = construe[5];
                            addsum.OWNERDEPARTMENTID = construe[6];
                            addsum.OWNERCOMPANYID = construe[7];
                        }
                        catch (Exception exx)
                        {
                            SMT.Foundation.Log.Tracer.Debug(exx.Message);
                            exx.Message.ToString();
                        }
                        dal.AddToContext(addsum);
                        //addbll.DataContext.AddObject("T_HR_EMPLOYEEADDSUM", addsum);
                    }
                    dal.SaveContextChanges();
                    //addbll.DataContext.SaveChanges();
                    for (int j = 0; j < listDebtRes.Length; j++)
                    {
                        salaryrecords[j].ACTUALLYPAY = (Convert.ToDecimal(salaryrecords[j].ACTUALLYPAY) - listDebtRes[j].Debt).ToString();
                    }
                    EmployeeSalaryRecordBLL salaryrecordbll = new EmployeeSalaryRecordBLL();//修改员工实发.
                    foreach (T_HR_EMPLOYEESALARYRECORD sr in salaryrecords)
                        salaryrecordbll.EmployeeSalaryRecordUpdate(sr);

                }
                catch (Exception ex)
                {
                    Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                }
                //发放时候真扣 
            }
        }

        /// <summary>
        /// 撤消还款(审核不通过时候撤消还款)
        /// </summary>
        /// <returns>返回结果</returns>
        public bool UndoRepayment(string employeeid, string year, string month)
        {
            SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[] listDebt = new SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[1];
            EmployeeAddSumBLL addbll = new EmployeeAddSumBLL();
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                       where a.EMPLOYEEID == employeeid && a.PROJECTCODE == "-3" && a.DEALYEAR == year && a.DEALMONTH == month
                       select a;
            if (ents.Count() > 0)
            {
                string[] sid = new string[1];
                sid[0] = ents.FirstOrDefault().ADDSUMID;
                //FB操作撤消还款
                listDebt[0].OrderID = sid[0];
                FBSclient.RepayBySalary(listDebt, SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceRepayType.Canel);
                addbll.EmployeeAddSumDelete(sid);
            }
            return false;
        }
        /// <summary>
        /// 批量撤消还款(审核不通过时候批量撤消还款)
        /// </summary>
        /// <returns>返回结果</returns>
        public bool UndoRepayment(int objectType, string objectID, string year, string month)
        {
            int i = 0;
            List<string> sid = new List<string>();
            EmployeeAddSumBLL addbll = new EmployeeAddSumBLL();
            List<string> tempIDs = GetEmployeeIDs(objectType, objectID);
            SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[] listDebt = new SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[tempIDs.Count];
            foreach (string id in tempIDs)
            {
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                           where a.EMPLOYEEID == id && a.PROJECTCODE == "-3" && a.DEALYEAR == year && a.DEALMONTH == month
                           select a;
                if (ents.Count() > 0)
                {
                    listDebt[i].OrderID = ents.FirstOrDefault().ADDSUMID;
                    sid.Add(listDebt[i].OrderID);
                    i++;
                }
            }
            if (listDebt.Count() > 0)
            {
                addbll.EmployeeAddSumDelete(sid.ToArray());
                //FB操作撤消还款
                FBSclient.RepayBySalary(listDebt, SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceRepayType.Canel);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 按机构类型获取员工ID集合
        /// </summary>
        /// <returns>返回员工ID集合</returns>
        public List<string> GetEmployeeIDs(int objectType, string objectID)
        {
            List<string> result = new List<string>();
            List<T_HR_EMPLOYEESALARYRECORD> records = new List<T_HR_EMPLOYEESALARYRECORD>();
            switch (objectType)
            {
                case 0:
                    IQueryable<T_HR_EMPLOYEESALARYRECORD> eidc = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                                 join b in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>() on a.EMPLOYEEPOSTID equals b.EMPLOYEEID
                                                                 join c in dal.GetObjects<T_HR_POST>() on a.T_HR_POST.POSTID equals c.POSTID
                                                                 join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                                                 join e in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals e.COMPANYID
                                                                 where d.DEPARTMENTID == objectID
                                                                 select b;

                    records = eidc.ToList();
                    break;
                case 1:
                    IQueryable<T_HR_EMPLOYEESALARYRECORD> eidd = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                                 join b in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>() on a.EMPLOYEEPOSTID equals b.EMPLOYEEID
                                                                 join c in dal.GetObjects<T_HR_POST>() on a.T_HR_POST.POSTID equals c.POSTID
                                                                 join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                                                 where d.DEPARTMENTID == objectID
                                                                 select b;

                    records = eidd.ToList();
                    break;
                case 2:
                    IQueryable<T_HR_EMPLOYEESALARYRECORD> eidp = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                                 join b in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>() on a.EMPLOYEEPOSTID equals b.EMPLOYEEID
                                                                 where a.T_HR_POST.POSTID == objectID
                                                                 select b;

                    records = eidp.ToList();
                    break;
            }
            if (records.Count > 0)
            {
                foreach (var r in records)
                {
                    result.Add(r.EMPLOYEEID);
                }
            }
            else
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// 餐费补贴
        /// </summary>
        /// <returns>返回餐费补贴</returns>
        public decimal GetMealAllowance(string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYITEM>()
                       join b in dal.GetObjects<T_HR_SALARYARCHIVEITEM>() on a.SALARYITEMID equals b.SALARYITEMID
                       join c in dal.GetObjects<T_HR_SALARYARCHIVE>() on b.T_HR_SALARYARCHIVE.SALARYARCHIVEID equals c.SALARYARCHIVEID
                       where c.EMPLOYEEID == employeeid && a.ENTITYCOLUMNCODE == "FOODALLOWANCE"
                       select new
                       {
                           SUM = b.SUM
                       };
            if (ents.Count() > 0)
            {
                try
                {
                    return Convert.ToDecimal(ents.FirstOrDefault().SUM);
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug(ex.Message + " InnerException: " + ex.StackTrace);
                }
            }
            return 0;
        }

        /// <summary>
        /// 计算薪资项的值
        /// </summary>
        /// <param name="itemid">薪资项的ID</param>
        /// <param name="bzxzValue">标准薪资的值</param>
        /// <returns></returns>
        public string AutoCalculateItem(string itemid, decimal bzxzValue, string areaMoney)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYITEM>()
                       where a.SALARYITEMID == itemid
                       select a;
            string tempcode = string.Empty, result = string.Empty;
            if (ents.Count() <= 0) return string.Empty;
            try
            {
                tempcode = ents.FirstOrDefault().CALCULATEFORMULACODE;
                string ent = tempcode.Replace("{" + GetBzxzid() + "}", bzxzValue.ToString());
                string[] temp = ent.Split('{');

                //实例化计算薪资项的方法
                Operation opFunc = new Operation();

                foreach (string t in temp)
                {
                    int i = t.IndexOf('}');
                    if (i != -1)
                    {
                        string value = t.Substring(0, i);
                        var e = from a in dal.GetObjects<T_HR_SALARYITEM>()
                                where a.SALARYITEMID == value
                                select a;
                        if (e.Count() > 0)
                        {
                            T_HR_SALARYITEM tempitem = e.FirstOrDefault();
                            if (tempitem.ENTITYCOLUMNCODE == "AREADIFALLOWANCE")
                                tempitem.GUERDONSUM = Convert.ToDecimal(areaMoney);
                            ent = ent.Replace("{" + value + "}", tempitem.GUERDONSUM != null ? tempitem.GUERDONSUM.ToString() : "0");
                        }
                        else
                        {
                            ent = ent.Replace("{" + value + "}", "0");
                        }
                    }
                }
                result = Alternative(Convert.ToDecimal(opFunc.Evalcal(string.IsNullOrEmpty(ent) ? "0" : ent)), 2).ToString();
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug(e.Message);
                e.Message.ToString();
            }
            return result;
        }

        /// <summary>
        /// 递归解析薪资项目ID
        /// </summary>
        /// <param name="strcode">需要处理的计算公式编码字符</param>
        /// <returns>返回解析薪资项目ID后的结果</returns>
        public string AutoAssemblyCharacter(string strcode, string employeeid, int year, int month)
        {
            int i = strcode.IndexOf("SELECT");
            string getFront = string.Empty;
            Regex re = new Regex(@"\{\s*([^}]+)\s*\}");
            if (!re.IsMatch(strcode))
            {
                if (i != -1) //没有ID有条件公式
                {
                    return GetSqlResult(strcode, employeeid, year, month);
                }
                else return strcode;
            }
            string getstr = re.Match(strcode).Value;
            int strIndex = strcode.IndexOf(getstr);
            int getstrLength = getstr.Length;
            getstr = getstr.Substring(1, getstr.Length - 2);  //解析薪资项目ID    
            string getvalue = GetItemsValue(getstr);
            if (getstr == GetBzxzid())
            {
                getstr = SystemBasicSalary(employeeid).ToString();
            }
            else if (getvalue != string.Empty)
            {
                getstr = getvalue;
            }
            else
            {
                var ents = (from a in dal.GetObjects<T_HR_SALARYITEM>()
                            where a.SALARYITEMID == getstr
                            select a).FirstOrDefault();
                if (ents != null)
                {
                    bool overTime = true;
                    # region 加班费
                    if (ents.ENTITYCOLUMNCODE == "OVERTIME") //加班费计算
                    {
                        if (AttendsolutionForSalary != null)
                        {
                            if (AttendsolutionForSalary.OVERTIMEPAYTYPE == "1" || AttendsolutionForSalary.OVERTIMEPAYTYPE == "3")
                            {
                                getstr = "0";
                                overTime = false;
                            }
                        }
                        #region
                        //DateTime dtAttendRdStart = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1);
                        //DateTime dtAttendRdEnd = dtAttendRdStart.AddMonths(1).AddDays(-1);

                        //AttendanceRecordBLL attrecordBll = new AttendanceRecordBLL();
                        //var attrecord = attrecordBll.GetAttendanceRecordByEmployeeIDAndDate(employeeid, dtAttendRdStart, dtAttendRdEnd).FirstOrDefault();
                        //if (attrecord != null)
                        //{
                        //    var attSolution = (from c in dal.GetObjects<T_HR_ATTENDANCESOLUTION>()
                        //                       where c.ATTENDANCESOLUTIONID == attrecord.ATTENDANCESOLUTIONID
                        //                       select c).FirstOrDefault();
                        //    if (attSolution.OVERTIMEPAYTYPE == "1" || attSolution.OVERTIMEPAYTYPE == "3")
                        //    {
                        //        getstr = "0";
                        //        overTime = false;
                        //    }
                        //}
                        //else
                        //{
                        //    AttendanceSolutionBLL solutionBll = new AttendanceSolutionBLL();
                        //    var anOtherSolution = solutionBll.GetAttendanceSolutionByEmployeeID(employeeid);
                        //    if (anOtherSolution != null)
                        //    {
                        //        if (anOtherSolution.OVERTIMEPAYTYPE == "1" || anOtherSolution.OVERTIMEPAYTYPE == "3")
                        //        {
                        //            getstr = "0";
                        //            overTime = false;
                        //        }
                        //    }
                        //}
                        #endregion
                    }
                    # endregion
                    if (ents.ENTITYCOLUMNCODE == "AREADIFALLOWANCE")
                    {
                        getstr = AreaSubsidy().ToString();  //地区差异补贴特殊处理
                    }
                    else if (ents.ENTITYCOLUMNCODE == "ATTENDANCEUNUSUALDEDUCT")
                    {
                        T_HR_ATTENDMONTHLYBALANCE attendMB = GetMonthTotal(employeeid, year, month);
                        if (attendMB != null)
                        {
                            //处理考勤异常扣款,为空的情况,月薪生成涵数已经处理了                        
                            getstr = AbnormalDeduct(employeeid, year, month, Convert.ToDecimal(attendMB.ABSENTDAYS
                                != null ? attendMB.ABSENTDAYS : 0)).ToString();

                            //针对已加入公式的                            
                            string entstr = ents.CALCULATEFORMULACODE;
                            if (!string.IsNullOrEmpty(entstr) && overTime == true)
                            {
                                string tempstring = string.Empty;
                                try
                                {
                                    Operation opFunc = new Operation();

                                    tempstring = AutoAssemblyCharacter(entstr, employeeid, year, month);
                                    tempstring = opFunc.Evalcal(string.IsNullOrEmpty(tempstring) ? "0" : tempstring).ToString();
                                    getstr = string.IsNullOrEmpty(tempstring) ? "0" : Math.Round(Convert.ToDecimal(tempstring), 2).ToString();
                                }
                                catch (Exception ex)
                                {
                                    getstr = "0";
                                    if (ex.InnerException != null)
                                    {
                                        SMT.Foundation.Log.Tracer.Debug("3635行" + ex.StackTrace);
                                    }
                                    else
                                    {
                                        SMT.Foundation.Log.Tracer.Debug("3635行" + ex.Message + " InnerException: " + ex.StackTrace);
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        string entstr = ents.CALCULATEFORMULACODE;
                        if (!string.IsNullOrEmpty(entstr) && overTime == true)
                        {
                            string tempstring = string.Empty;
                            try
                            {
                                if (ents.ENTITYCOLUMNCODE == "WORKINGSALARY"
                                    || ents.ENTITYCOLUMNCODE == "PRETAXSUBTOTAL")//额外处理的出勤工资,和税前应发合计,若执行国际上通用的'4舍6入'规则,只要在薪资项目公式round定义即可  
                                {
                                    //实例化计算薪资项的方法
                                    Operation opFunc = new Operation();

                                    tempstring = AutoAssemblyCharacter(entstr, employeeid, year, month);
                                    getstr = Alternative(Convert.ToDecimal(opFunc.Evalcal(string.IsNullOrEmpty(tempstring) ? "0" : tempstring)), 0).ToString();
                                }
                                else
                                {
                                    getstr = AutoAssemblyCharacter(entstr, employeeid, year, month);
                                }
                            }
                            catch (Exception ex)
                            {
                                getstr = "0";
                                if (ex.InnerException != null)
                                {
                                    SMT.Foundation.Log.Tracer.Debug("3635行" + ex.StackTrace);
                                }
                                else
                                {
                                    SMT.Foundation.Log.Tracer.Debug("3635行" + ex.Message + " InnerException: " + ex.StackTrace);
                                }

                            }
                        }
                        else
                        {
                            getstr = ents.GUERDONSUM.ToString();
                        }
                    }
                }
            }


            if (strIndex >= 1) getFront = strcode.Substring(0, strIndex);
            string tempstr = strcode.Substring(strIndex + getstrLength);
            string getEnd = AutoAssemblyCharacter(tempstr, employeeid, year, month);
            if (i != -1)
            {
                return GetSqlResult(getFront + getstr + getEnd, employeeid, year, month); //有ID有条件公式
            }
            else
            {
                return getFront + getstr + getEnd;
            }
        }



        int iflag = 0;
        public string GetSqlResult(string strcode, string employeeid, int year, int month)
        {
            int signIndex = -1;
            strcode = strcode.Trim();
            string[] filterTableName = { "T_HR_EMPLOYEEADDSUM", "T_HR_PENSIONDETAIL", "T_HR_ATTENDMONTHLYBALANCE", "T_HR_EMPLOYEESALARYRECORD", "T_HR_PERFORMANCEREWARDRECORD" };
            string[] filterYear = { "DEALYEAR", "PENSIONYEAR", "BALANCEYEAR", "SALARYYEAR", "SALARYYEAR" };
            string[] filterMonth = { "DEALMONTH", "PENSIONMOTH", "BALANCEMONTH", "SALARYMONTH", "SALARYMONTH" };
            int j = strcode.IndexOf("WHERE");
            for (int i = 0; i <= filterTableName.Length - 1; i++)
            {
                if (strcode.IndexOf(filterTableName[i]) != -1)
                {
                    signIndex = i;
                    break;
                }
            }
            if (j != -1)
            {
                if (signIndex == 2)
                {
                    strcode += "AND " + filterTableName[signIndex] + ".EMPLOYEEID ='" + employeeid + "'";
                }
                else
                {
                    strcode += "AND " + filterTableName[signIndex] + ".EMPLOYEEID ='" + employeeid + "'";
                }
            }
            else
            {
                if (signIndex == 2)
                {
                    strcode += " WHERE " + filterTableName[signIndex] + ".EMPLOYEEID ='" + employeeid + "'";
                }
                else
                {
                    strcode += " WHERE  EMPLOYEEID = '" + employeeid + "'";
                    //strcode += " WHERE "+ filterTableName[signIndex] + ".EMPLOYEEID = '" + employeeid + "'";
                }
            }

            if (signIndex != -1)
            {
                strcode += " AND " + filterTableName[signIndex] + "." + filterYear[signIndex] + " = '" + year + "' AND " + filterTableName[signIndex] + "." + filterMonth[signIndex] + " = '" + month + "'";
            }
            if (signIndex == 0)
            {
                strcode += "AND " + filterTableName[signIndex] + ".CHECKSTATE ='" + ((int)CheckStates.Approved).ToString() + "'";
            }
            try
            {
                string comb = "SELECT  ROUND(";
                int mid = strcode.IndexOf("FROM");
                comb += strcode.Substring(6, mid - 7);
                comb += ",3)";
                comb += strcode.Substring(mid, strcode.Length - mid);
                strcode = comb;
                //return string.IsNullOrEmpty(dal.ExecuteCustomerSql(strcode).ToString()) ? "0" : dal.ExecuteCustomerSql(strcode).ToString();
#if Debug

                SMT.Foundation.Log.Tracer.Debug("StartTime:" + DateTime.Now.ToLongTimeString());
                SMT.Foundation.Log.Tracer.Debug("______________第" + iflag + "次计算薪资项______________" + "\n");
                
                iflag++;
#endif
                string salaryItemValue = string.Empty;
                if (!salaryItemCaches.Keys.Contains(strcode))
                {
                    salaryItemValue = ExecuteSqlScalar(strcode);
                    salaryItemCaches.Add(strcode, salaryItemValue);

                    try
                    {                      
                        if (System.Configuration.ConfigurationSettings.AppSettings.Get("TraceSalarySql") == "true")
                        {
                            Tracer.Debug("执行的sql:" + strcode + " 结果：" + salaryItemValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug(ex.ToString());
                    }
                }
                else
                {
                    salaryItemValue = salaryItemCaches[strcode];
#if Debug
                    SMT.Foundation.Log.Tracer.Debug("______________第" + iGetItemValue + "次从缓存中取薪资公式值______________" + "\n");
#endif
                    iGetItemValue++;
                }

                return string.IsNullOrEmpty(salaryItemValue) ? "0" : salaryItemValue;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    SMT.Foundation.Log.Tracer.Debug("执行的Sql语句：" + strcode + ";在3715行" + ex.StackTrace);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("执行的Sql语句：" + strcode + ";在3715行" + ex.Message + " InnerException: " + ex.StackTrace);
                }
                return "0";
            }
        }
        /// <summary>
        /// 查询一行数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public string ExecuteSqlScalar(string sql)
        {
            if (myOraclecommad.Connection.State == ConnectionState.Closed)
            {
                Tracer.Debug("打开了oracle的一次连接，通过myOraclecommad");
                myOraclecommad.Connection.Open();
            }
            myOraclecommad.CommandText = sql;
            //OracleCommand ocom = new OracleCommand(sql, myOraclecommad.Connection);
            object dr = myOraclecommad.ExecuteScalar();
            if (dr != null)
            {
                return dr.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// 执行数据库管理操作
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="entity">实体名</param>
        /// <returns></returns>
        public bool ExecuteSql(string sql, string entity)
        {
            int i = -1;

            if (myOraclecommad.Connection.State == ConnectionState.Closed)
            {
                Tracer.Debug("打开了oracle的一次连接，通过myOraclecommad");
                myOraclecommad.Connection.Open();
            }
            //OracleCommand ocom = new OracleCommand(sql, myOracleConn);
            myOraclecommad.CommandText = sql;
            //OracleDataAdapter oda = new OracleDataAdapter(sql, myOraclecommad.Connection);
            //i = oda.Fill(new DataSet(), entity);
            i = myOraclecommad.ExecuteNonQuery();
            if (i != -1) return true;
            else return false;
        }

        /// <summary>
        /// 解析薪资项目的值
        /// </summary>
        /// <param name="itemid">需要处理的薪资项目ID</param>
        /// <returns>返回解析薪资项目后的值</returns>
        public string GetItemsValue(string itemid)
        {
            foreach (var id in getCaches.Keys)
            {
                if (id == itemid) return getCaches[id].ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 过滤薪资标准
        /// </summary>
        /// <returns>返回过滤的薪资标准</returns>
        public List<string> FilterStandard(string year, string month)
        {
            List<string> listid = new List<string>();
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       where a.SALARYYEAR == year && a.SALARYMONTH == month
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    listid.Add(ent.SALARYSTANDARDID);
                }
                return listid;
            }
            return null;
        }

        public void initialOrderSalaryItems()
        {
            SalaryItemsOrders c1 = new SalaryItemsOrders("ea67192b-a34a-42b9-b56c-253c9f8e0f35", 0);//基本工资
            SalaryItemsOrderCache.Add(c1);
            SalaryItemsOrders c2 = new SalaryItemsOrders("4b83c88a-2b60-43f0-bf22-9a88a78204d0", 1);//保密津贴
            SalaryItemsOrderCache.Add(c2);
            SalaryItemsOrders c3 = new SalaryItemsOrders("f4ad7117-1d2f-4515-b216-0586d2fd8664", 2);//住房补贴
            SalaryItemsOrderCache.Add(c3);
            SalaryItemsOrders c4 = new SalaryItemsOrders("fba9f626-ab50-41e6-b479-2649cd7e20d4", 3);//岗位工资
            SalaryItemsOrderCache.Add(c4);
            SalaryItemsOrders c5 = new SalaryItemsOrders("421cc017-b203-4cfa-9396-f617663107eb", 4);//绩效奖金
            SalaryItemsOrderCache.Add(c5);
            SalaryItemsOrders c6 = new SalaryItemsOrders("99e04010-9e4b-4853-9b4c-616846865bea", 5);//值班津贴
            SalaryItemsOrderCache.Add(c6);
            SalaryItemsOrders c7 = new SalaryItemsOrders("a1448582-f354-43db-a2a9-1f138b98d2d2", 6);//个人社保负担
            SalaryItemsOrderCache.Add(c7);
            SalaryItemsOrders c8 = new SalaryItemsOrders("58d9c878-150f-4d14-b1a9-d7f07f8527c1", 7);//地区差异补贴
            SalaryItemsOrderCache.Add(c8);
            SalaryItemsOrders c9 = new SalaryItemsOrders("bee94c6a-e860-4939-8572-fff587486392", 8);//住房公积金
            SalaryItemsOrderCache.Add(c9);
            SalaryItemsOrders c10 = new SalaryItemsOrders("8540c3f1-b448-4eec-adf7-83bff08550bd", 9);//纳税系数
            SalaryItemsOrderCache.Add(c10);
            SalaryItemsOrders c11 = new SalaryItemsOrders("039e58e4-7877-49f0-8a1b-3250634f785c", 10);//扣税基数
            SalaryItemsOrderCache.Add(c11);
            SalaryItemsOrders c12 = new SalaryItemsOrders("86926078-d4c4-4fb6-a9c2-1910843a9309", 11);//餐费补贴
            SalaryItemsOrderCache.Add(c12);
            SalaryItemsOrders c13 = new SalaryItemsOrders("23993b15-5e3e-4780-8273-a367b143b046", 12);//加班费
            SalaryItemsOrderCache.Add(c13);
            SalaryItemsOrders c14 = new SalaryItemsOrders("c356bcf0-add3-40fe-b4f6-2e30e86daebe", 13);//其他代扣款
            SalaryItemsOrderCache.Add(c14);
            SalaryItemsOrders c15 = new SalaryItemsOrders("2c8c93e5-c870-470d-9339-007e191232ed", 14);//其他加扣款
            SalaryItemsOrderCache.Add(c15);
            SalaryItemsOrders c16 = new SalaryItemsOrders("7692ec13-0785-432d-abd8-aa26b993216c", 15);//固定收入合计
            SalaryItemsOrderCache.Add(c16);
            SalaryItemsOrders c17 = new SalaryItemsOrders("857b5020-0abb-4a7b-b087-812450c5c0e9", 16);//缺勤天数
            SalaryItemsOrderCache.Add(c17);
            SalaryItemsOrders c18 = new SalaryItemsOrders("7a4a0abf-24a0-481d-ade1-c52d4a0c56cf", 17);//'假期其它扣款'
            SalaryItemsOrderCache.Add(c18);
            SalaryItemsOrders c19 = new SalaryItemsOrders("916ddaea-a612-40b8-908e-cd28c1a57137", 18);//出勤工资
            SalaryItemsOrderCache.Add(c19);
            SalaryItemsOrders c20 = new SalaryItemsOrders("0c473d24-fa15-431a-994f-40523d5fe09a", 19);//税前应发合计
            SalaryItemsOrderCache.Add(c20);
            SalaryItemsOrders c21 = new SalaryItemsOrders("9a42a345-7d5f-439f-9820-9cdbbcbec871", 20);//计税工资
            SalaryItemsOrderCache.Add(c21);
            SalaryItemsOrders c22 = new SalaryItemsOrders("d62310ea-f5ec-4393-862c-da3500722689", 21);//差额
            SalaryItemsOrderCache.Add(c22);
            SalaryItemsOrders c23 = new SalaryItemsOrders("5c142665-ecda-489c-8e7d-95a39c2144cf", 22);//税率
            SalaryItemsOrderCache.Add(c23);
            SalaryItemsOrders c24 = new SalaryItemsOrders("68d52d34-805e-4873-bb21-544746ec5d1e", 23);//速算扣除
            SalaryItemsOrderCache.Add(c24);
            SalaryItemsOrders c25 = new SalaryItemsOrders("a3162966-cb70-497a-a972-180337d8fffd", 24);//个人所得税
            SalaryItemsOrderCache.Add(c25);
            SalaryItemsOrders c26 = new SalaryItemsOrders("c6cfc5c4-8d5b-4290-aa8d-2af3858502d1", 25);//考勤异常扣款
            SalaryItemsOrderCache.Add(c26);
            SalaryItemsOrders c27 = new SalaryItemsOrders("bc8a3557-c931-4ec5-984f-42a01b8cf75b", 26);//应发小计
            SalaryItemsOrderCache.Add(c27);
            SalaryItemsOrders c28 = new SalaryItemsOrders("924b2771-39ac-4fcb-810b-5b5ff2d89a4c", 27);//实发工资
            SalaryItemsOrderCache.Add(c28);
            SalaryItemsOrders c29 = new SalaryItemsOrders("c8bf6ab9-6578-4fff-8d69-6ec274754de5", 28);//尾数扣款
            SalaryItemsOrderCache.Add(c29);
            SalaryItemsOrders c30 = new SalaryItemsOrders("e7e58f22-eae0-4026-970d-c8c70a9faee9", 29);//扣款合计
            SalaryItemsOrderCache.Add(c30);

        }

        void IDisposable.Dispose()
        {    
            this.myOraclecommad.Dispose();
            base.Dispose();
        }
    }




    #region struct type and class
    public class SalaryItemsOrders
    {
        public SalaryItemsOrders(string SalaryItemId, int OrderNumber)
        {
            orderNumber = OrderNumber;
            salaryItemId = SalaryItemId;
        }

        public int orderNumber;
        public string salaryItemId;
    }

    /// <summary>
    /// 漏打卡，迟到,早退 公共视图结构
    /// </summary>
    public class TempType
    {
        public decimal? FINERATIO { get; set; }
        public decimal? FINESUM { get; set; }
        public decimal? LOW { get; set; }
        public string MAINTYPE { get; set; }
        public string DETAILTYPE { get; set; }
        public decimal? LOWESTTIMES { get; set; }
        public decimal? HIGHESTTIMES { get; set; }
        public string ISACCUMULATING { get; set; }
        public decimal? ALLOWLATEMAXTIMES { get; set; }
        public decimal? ALLOWLOSTCARDTIMES { get; set; }
        public decimal? ALLOWLATEMAXMINUTE { get; set; }
    }



    /// <summary>
    /// 运算结果
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// 计算结果,如果表达式出错则抛出异常
        /// </summary>
        /// <param name="statement">表达式</param>
        /// <returns>结果</returns>
        public object Evalcal(string statement)
        {
            return _evaluatorType.InvokeMember(
                        "Evalcal",
                        BindingFlags.InvokeMethod,
                        null,
                        _evaluator,
                        new object[] { statement }
                     );
        }

        public Operation()
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("JScript");

            CompilerParameters parameters;
            parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;

            CompilerResults results;
            results = provider.CompileAssemblyFromSource(parameters, _jscriptSource);

            Assembly assembly = results.CompiledAssembly;
            _evaluatorType = assembly.GetType("Operation");

            _evaluator = Activator.CreateInstance(_evaluatorType);
        }

        private object _evaluator = null;
        private Type _evaluatorType = null;

        /// <summary>
        /// _jscriptSource
        /// </summary>
        private readonly string _jscriptSource =
            @"class Operation
              {
                  public function Evalcal(expr : String) : String 
                  { 
                     return eval(expr); 
                  }
              }";


    }

    #endregion
}
