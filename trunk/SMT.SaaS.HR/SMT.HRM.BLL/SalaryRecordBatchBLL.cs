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
using System.Data;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using System.Reflection;
namespace SMT.HRM.BLL
{
    public class SalaryRecordBatchBLL : BaseBll<T_HR_SALARYRECORDBATCH>, IOperate
    {
        private EmployeeSalaryRecordBLL dals = new EmployeeSalaryRecordBLL();
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
        public IQueryable<T_HR_EMPLOYEESALARYRECORD> GetSalaryRecordAuditPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
            return bll.GetAutoEmployeeSalaryRecordPagings(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, strCheckState, userID);
        }

        /// <summary>
        /// 筛选并组装薪资记录
        /// </summary>
        /// <param name="orgtype">组织架构类型</param>
        /// <param name="orgid">类型ID</param>
        /// <returns></returns>
        public IQueryable<V_EMPLOYEESALARYRECORD> GetResultset(int orgtype, string orgid)
        {
            IQueryable<V_EMPLOYEESALARYRECORD> ents = null;  //= dal.GetObjects<V_EMPLOYEESALARYRECORD>();
            switch (orgtype)
            {
                case 0:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.T_HR_COMPANY.COMPANYID == orgid && b.ISAGENCY == "0" // && b.EDITSTATE == "1"
                           select new V_EMPLOYEESALARYRECORD
                           {
                               EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                               EMPLOYEEID = a.EMPLOYEEID,
                               EMPLOYEENAME = a.EMPLOYEENAME,
                               SALARYYEAR = a.SALARYYEAR,
                               SALARYMONTH = a.SALARYMONTH,
                               ACTUALLYPAY = a.ACTUALLYPAY,
                               DEPARTMENT = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POST = c.T_HR_POSTDICTIONARY.POSTNAME,
                               MONTHLYBATCHID = a.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID,
                               POSTLEVEL = b.POSTLEVEL,
                               SALARYLEVEL = b.SALARYLEVEL,
                               OWNERCOMPANYID = a.OWNERCOMPANYID,
                               CHECKSTATE = a.CHECKSTATE,
                               T_HR_EMPLOYEESALARYRECORD = a,
                               CREATEUSERID = a.CREATEUSERID
                           };
                    break;
                case 1:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.DEPARTMENTID == orgid && b.ISAGENCY == "0"// && b.EDITSTATE == "1"
                           select new V_EMPLOYEESALARYRECORD
                           {
                               EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                               EMPLOYEEID = a.EMPLOYEEID,
                               EMPLOYEENAME = a.EMPLOYEENAME,
                               SALARYYEAR = a.SALARYYEAR,
                               SALARYMONTH = a.SALARYMONTH,
                               ACTUALLYPAY = a.ACTUALLYPAY,
                               DEPARTMENT = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POST = c.T_HR_POSTDICTIONARY.POSTNAME,
                               MONTHLYBATCHID = a.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID,
                               POSTLEVEL = b.POSTLEVEL,
                               SALARYLEVEL = b.SALARYLEVEL,
                               OWNERCOMPANYID = a.OWNERCOMPANYID,
                               CHECKSTATE = a.CHECKSTATE,
                               T_HR_EMPLOYEESALARYRECORD = a,
                               CREATEUSERID = a.CREATEUSERID
                           };
                    break;
                case 2:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where c.POSTID == orgid && b.ISAGENCY == "0" //&& b.EDITSTATE == "1"
                           select new V_EMPLOYEESALARYRECORD
                           {
                               EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                               EMPLOYEEID = a.EMPLOYEEID,
                               EMPLOYEENAME = a.EMPLOYEENAME,
                               SALARYYEAR = a.SALARYYEAR,
                               SALARYMONTH = a.SALARYMONTH,
                               ACTUALLYPAY = a.ACTUALLYPAY,
                               DEPARTMENT = c.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POST = c.T_HR_POSTDICTIONARY.POSTNAME,
                               MONTHLYBATCHID = a.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID,
                               POSTLEVEL = b.POSTLEVEL,
                               SALARYLEVEL = b.SALARYLEVEL,
                               OWNERCOMPANYID = a.OWNERCOMPANYID,
                               CHECKSTATE = a.CHECKSTATE,
                               T_HR_EMPLOYEESALARYRECORD = a,
                               CREATEUSERID = a.CREATEUSERID
                           };
                    break;
            }
            return ents;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询-薪资NEW
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<V_EMPLOYEESALARYRECORD> GetMassAuditSalaryRecordPagings(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            try
            {

                var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals b.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_DEPARTMENT>() on b.OWNERDEPARTMENTID equals c.OWNERDEPARTMENTID
                           join d in dal.GetObjects<T_HR_POST>() on b.OWNERPOSTID equals d.OWNERPOSTID
                           join e in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.ABSENTTIMES equals e.SALARYARCHIVEID
                           select new
                           {

                               EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                               EMPLOYEEID = a.EMPLOYEEID,
                               EMPLOYEENAME = a.EMPLOYEENAME,
                               SALARYYEAR = a.SALARYYEAR,
                               SALARYMONTH = a.SALARYMONTH,
                               ACTUALLYPAY = a.ACTUALLYPAY,
                               DEPARTMENT = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POST = d.T_HR_POSTDICTIONARY.POSTNAME,
                               MONTHLYBATCHID = a.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID,
                               POSTLEVEL = e == null ? null : e.POSTLEVEL,
                               SALARYLEVEL = e == null ? null : e.SALARYLEVEL,
                               OWNERCOMPANYID = a.OWNERCOMPANYID,
                               CHECKSTATE = a.CHECKSTATE,
                               T_HR_EMPLOYEESALARYRECORD = a,
                               CREATEUSERID = a.CREATEUSERID,
                               OWNERID = a.OWNERID
                               // salaryitem = a.T_HR_EMPLOYEESALARYRECORDITEM
                           };

                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");
                //  SetFilterWithflow("EMPLOYEESALARYRECORDID", "T_HR_EMPLOYEESALARYRECORD", userID, ref strCheckState, ref filterString, ref queryParas);
                if (strCheckState == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " AND";
                    }

                    filterString += " CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
                if (orgtype != 3)//不是提交单条记录
                {
                    ents = ents.Where(s => s.OWNERCOMPANYID == orgid);
                }

                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                string year = starttime.Year.ToString();
                string month = starttime.Month.ToString();
                ents = ents.Where(m => m.SALARYYEAR == year && m.SALARYMONTH == month);
                ents = ents.OrderBy(sort);
                //  var recoders = ents.ToList();
                #region
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
                #endregion
                int counts = ents.Count();
                decimal? tempTotalMoney = 0;
                string tempAvgMoney = string.Empty;
                if (counts > 0)
                {
                    foreach (var a in ents)
                    {
                        tempTotalMoney += string.IsNullOrEmpty(a.ACTUALLYPAY) ? 0 : Convert.ToDecimal(AES.AESDecrypt(a.ACTUALLYPAY));
                    }
                    tempAvgMoney = (Math.Round(Convert.ToDecimal(tempTotalMoney / counts), 2)).ToString();
                }
                #region
                //ents = ents.OrderBy(m => m.SALARYYEAR);
                //ents = ents.OrderBy(m => m.SALARYMONTH);
                List<V_EMPLOYEESALARYRECORD> records = new List<V_EMPLOYEESALARYRECORD>();
                foreach (var entItem in ents)
                {
                    V_EMPLOYEESALARYRECORD item = new V_EMPLOYEESALARYRECORD();
                    //string salaryArchiveID = entItem.salaryitem.FirstOrDefault().SALARYARCHIVEID;
                    //var archive = (from c in dal.GetObjects<T_HR_SALARYARCHIVE>()
                    //               where c.SALARYARCHIVEID == salaryArchiveID
                    //               select c).FirstOrDefault();
                    item.EMPLOYEESALARYRECORDID = entItem.EMPLOYEESALARYRECORDID;
                    item.EMPLOYEEID = entItem.EMPLOYEEID;
                    item.EMPLOYEENAME = entItem.EMPLOYEENAME;
                    item.SALARYYEAR = entItem.SALARYYEAR;
                    item.SALARYMONTH = entItem.SALARYMONTH;
                    item.ACTUALLYPAY = entItem.ACTUALLYPAY;
                    item.DEPARTMENT = entItem.DEPARTMENT;
                    item.POST = entItem.POST;
                    item.MONTHLYBATCHID = entItem.MONTHLYBATCHID;
                    item.POSTLEVEL = entItem.POSTLEVEL;
                    item.SALARYLEVEL = entItem.SALARYLEVEL;
                    item.OWNERCOMPANYID = entItem.OWNERCOMPANYID;
                    item.CHECKSTATE = entItem.CHECKSTATE;
                    item.T_HR_EMPLOYEESALARYRECORD = entItem.T_HR_EMPLOYEESALARYRECORD;
                    item.CREATEUSERID = entItem.CREATEUSERID;
                    records.Add(item);
                }
                #endregion
                records.FirstOrDefault().PAYTOTALMONEY = tempTotalMoney != null ? tempTotalMoney.ToString() : "0";
                records.FirstOrDefault().AVGMONEY = tempAvgMoney;

                return records.AsQueryable();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 批量审核
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="orgtype"></param>
        /// <param name="orgid"></param>
        /// <param name="strCheckState"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<V_salaryRecordDetailView> GetAuditSalaryRecordsPagings(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            try
            {

                var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals b.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_DEPARTMENT>() on b.OWNERDEPARTMENTID equals c.OWNERDEPARTMENTID
                           join d in dal.GetObjects<T_HR_POST>() on b.OWNERPOSTID equals d.OWNERPOSTID
                           join e in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.ABSENTTIMES equals e.SALARYARCHIVEID
                           select new
                           {

                               EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                               EMPLOYEEID = a.EMPLOYEEID,
                               EMPLOYEENAME = a.EMPLOYEENAME,
                               SALARYYEAR = a.SALARYYEAR,
                               SALARYMONTH = a.SALARYMONTH,
                               ACTUALLYPAY = a.ACTUALLYPAY,
                               DEPARTMENT = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POST = d.T_HR_POSTDICTIONARY.POSTNAME,
                               MONTHLYBATCHID = a.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID,
                               POSTLEVEL = e == null ? null : e.POSTLEVEL,
                               SALARYLEVEL = e == null ? null : e.SALARYLEVEL,
                               OWNERCOMPANYID = a.OWNERCOMPANYID,
                               CHECKSTATE = a.CHECKSTATE,
                               T_HR_EMPLOYEESALARYRECORD = a,
                               CREATEUSERID = a.CREATEUSERID,
                               OWNERID = a.OWNERID,
                               salaryitem = a.T_HR_EMPLOYEESALARYRECORDITEM
                           };

                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");
                if (strCheckState == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " AND";
                    }

                    filterString += " CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
                if (orgtype != 3)//不是提交单条记录
                {
                    ents = ents.Where(s => s.OWNERCOMPANYID == orgid);
                }

                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                string year = starttime.Year.ToString();
                string month = starttime.Month.ToString();
                ents = ents.Where(m => m.SALARYYEAR == year && m.SALARYMONTH == month);
                ents = ents.OrderBy(sort);

                int counts = ents.Count();
                decimal? tempTotalMoney = 0;
                string tempAvgMoney = string.Empty;
                if (counts <= 0)
                {
                    return null;
                }
                //if (counts > 0)
                //{
                //    foreach (var a in ents)
                //    {
                //        tempTotalMoney += string.IsNullOrEmpty(a.ACTUALLYPAY) ? 0 : Convert.ToDecimal(AES.AESDecrypt(a.ACTUALLYPAY));
                //    }
                //    tempAvgMoney = (Math.Round(Convert.ToDecimal(tempTotalMoney / counts), 2)).ToString();
                //}
                #region  查询薪资项
                var salaryRecordTmp = ents.FirstOrDefault();
                List<T_HR_SALARYITEM> modelSalaryItems = new List<T_HR_SALARYITEM>();//存储薪资项 目前是集团的薪资项
                if (salaryRecordTmp != null)
                {
                    var salaryRecorditemTmp = salaryRecordTmp.salaryitem.FirstOrDefault();
                    if (salaryRecorditemTmp != null)
                    {
                        var tmp = (from c in dal.GetObjects<T_HR_SALARYITEM>()
                                   where c.SALARYITEMID == salaryRecorditemTmp.SALARYITEMID
                                   select c).FirstOrDefault();
                        if (tmp != null)
                        {
                            var tmps = from c in dal.GetObjects<T_HR_SALARYITEM>()
                                       where c.OWNERCOMPANYID == tmp.OWNERCOMPANYID
                                       select c;
                            if (tmps.Count() > 0)
                            {
                                modelSalaryItems = tmps.ToList();
                            }
                        }
                    }
                }
                #endregion

                #region  查询加扣款
                var entAddsums = from c in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                                 where c.DEALYEAR == salaryRecordTmp.SALARYYEAR && c.DEALMONTH == salaryRecordTmp.SALARYMONTH && c.CHECKSTATE == "2"
                                 select c;
                #endregion

                #region
                List<V_salaryRecordDetailView> records = new List<V_salaryRecordDetailView>();
                foreach (var entItem in ents)
                {
                    V_salaryRecordDetailView item = new V_salaryRecordDetailView();
                    List<V_EMPLOYEESALARYRECORDITEM> salaryRecorders = new List<V_EMPLOYEESALARYRECORDITEM>();
                    item.EMPLOYEESALARYRECORDID = entItem.EMPLOYEESALARYRECORDID;
                    item.EMPLOYEEID = entItem.EMPLOYEEID;
                    item.EMPLOYEENAME = entItem.EMPLOYEENAME;
                    item.SALARYYEAR = entItem.SALARYYEAR;
                    item.SALARYMONTH = entItem.SALARYMONTH;
                    item.ACTUALLYPAY = entItem.ACTUALLYPAY;
                    item.DEPARTMENT = entItem.DEPARTMENT;
                    item.POST = entItem.POST;
                    item.MONTHLYBATCHID = entItem.MONTHLYBATCHID;
                    item.POSTLEVEL = entItem.POSTLEVEL;
                    item.SALARYLEVEL = entItem.SALARYLEVEL;
                    item.OWNERCOMPANYID = entItem.OWNERCOMPANYID;
                    item.CHECKSTATE = entItem.CHECKSTATE;
                    item.CREATEUSERID = entItem.CREATEUSERID;
                    tempTotalMoney += string.IsNullOrEmpty(entItem.ACTUALLYPAY) ? 0 : Convert.ToDecimal(AES.AESDecrypt(entItem.ACTUALLYPAY));
                    List<T_HR_EMPLOYEESALARYRECORDITEM> salaryItemsList = entItem.salaryitem.OrderBy(s => s.ORDERNUMBER).ToList();
                    foreach (var detail in salaryItemsList)
                    {
                        V_EMPLOYEESALARYRECORDITEM recordItem = new V_EMPLOYEESALARYRECORDITEM();
                        recordItem.SALARYRECORDITEMID = detail.SALARYRECORDITEMID;
                        recordItem.SALARYITEMNAME = modelSalaryItems.Where(s => s.SALARYITEMID == detail.SALARYITEMID).FirstOrDefault().SALARYITEMNAME;
                        recordItem.ORDERNUMBER = detail.ORDERNUMBER;
                        recordItem.SUM = detail.SUM;
                        salaryRecorders.Add(recordItem);
                    }
                    //查询加扣款 添加备注项
                    string remark = string.Empty;
                    V_EMPLOYEESALARYRECORDITEM addsumItem = new V_EMPLOYEESALARYRECORDITEM();
                    addsumItem.SALARYITEMNAME = "备注";

                    addsumItem.SALARYRECORDITEMID = Guid.NewGuid().ToString();

                    addsumItem.ORDERNUMBER = 100;
                    if (entAddsums.Count() > 0)
                    {
                        var addSumsForEmployee = from c in entAddsums
                                                 where c.EMPLOYEEID == item.EMPLOYEEID
                                                 select c;
                        foreach (var addsum in addSumsForEmployee)
                        {
                            remark += addsum.PROJECTNAME + "：" + addsum.PROJECTMONEY + "；";
                        }

                        addsumItem.SUM = AES.AESEncrypt(remark);
                    }
                    salaryRecorders.Add(addsumItem);
                    item.EMPLOYEESALARYRECORDITEMS = salaryRecorders;
                    records.Add(item);
                }
                #endregion
                tempAvgMoney = (Math.Round(Convert.ToDecimal(tempTotalMoney / counts), 2)).ToString();
                records.FirstOrDefault().PAYTOTALMONEY = tempTotalMoney != null ? tempTotalMoney.ToString() : "0";
                records.FirstOrDefault().AVGMONEY = tempAvgMoney;

                return records;
            }
            catch
            {
                return null;
            }
        }
        public DataSetData GetAuditSalaryRecords(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            try
            {
                #region 查询数据
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals b.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_DEPARTMENT>() on b.OWNERDEPARTMENTID equals c.OWNERDEPARTMENTID
                           join d in dal.GetObjects<T_HR_POST>() on b.OWNERPOSTID equals d.OWNERPOSTID
                           join e in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.ABSENTTIMES equals e.SALARYARCHIVEID
                           select new
                           {

                               EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                               EMPLOYEEID = a.EMPLOYEEID,
                               EMPLOYEENAME = a.EMPLOYEENAME,
                               SALARYYEAR = a.SALARYYEAR,
                               SALARYMONTH = a.SALARYMONTH,
                               ACTUALLYPAY = a.ACTUALLYPAY,
                               DEPARTMENT = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POST = d.T_HR_POSTDICTIONARY.POSTNAME,
                               MONTHLYBATCHID = a.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID,
                               POSTLEVEL = e == null ? null : e.POSTLEVEL,
                               SALARYLEVEL = e == null ? null : e.SALARYLEVEL,
                               OWNERCOMPANYID = a.OWNERCOMPANYID,
                               CHECKSTATE = a.CHECKSTATE,
                               T_HR_EMPLOYEESALARYRECORD = a,
                               CREATEUSERID = a.CREATEUSERID,
                               OWNERID = a.OWNERID,
                               salaryitem = a.T_HR_EMPLOYEESALARYRECORDITEM
                           };

                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");
                if (strCheckState == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " AND";
                    }

                    filterString += " CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
                if (orgtype != 3)//不是提交单条记录
                {
                    ents = ents.Where(s => s.OWNERCOMPANYID == orgid);
                }

                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                
                string year = starttime.Year.ToString();
                string month = starttime.Month.ToString();
                ents = ents.Where(m => m.SALARYYEAR == year && m.SALARYMONTH == month);
                ents = ents.OrderBy(sort).ToList().AsQueryable();
                //按部门排序
                //ents = ents.OrderBy("DEPARTMENT");
                int counts = ents.Count();
                decimal? tempTotalMoney = 0;
                string tempAvgMoney = string.Empty;
                if (counts <= 0)
                {
                    return null;
                }
                #endregion
                #region  查询薪资项 以获取薪资项的名称
                var salaryRecordTmp = ents.FirstOrDefault();
                List<T_HR_SALARYITEM> modelSalaryItems = new List<T_HR_SALARYITEM>();//存储薪资项 目前是集团的薪资项
                if (salaryRecordTmp != null)
                {
                    var salaryRecorditemTmp = salaryRecordTmp.salaryitem.FirstOrDefault();
                    if (salaryRecorditemTmp != null)
                    {
                        var tmp = (from c in dal.GetObjects<T_HR_SALARYITEM>()
                                   where c.SALARYITEMID == salaryRecorditemTmp.SALARYITEMID
                                   select c).FirstOrDefault();
                        if (tmp != null)
                        {
                            var tmps = from c in dal.GetObjects<T_HR_SALARYITEM>()
                                       where c.OWNERCOMPANYID == tmp.OWNERCOMPANYID
                                       select c;
                            if (tmps.Count() > 0)
                            {
                                modelSalaryItems = tmps.ToList();
                            }
                        }
                    }
                }
                #endregion

                #region  查询加扣款
                var entAddsums = from c in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                                 where c.DEALYEAR == salaryRecordTmp.SALARYYEAR && c.DEALMONTH == salaryRecordTmp.SALARYMONTH && c.CHECKSTATE == "2"
                                 select c;
                #endregion

                #region 构建类似用tabel的结构
                DataSetData dsd = new DataSetData();
                dsd.Tables = new ObservableCollection<DataTableInfo>();

                DataTableInfo tableInfo = new DataTableInfo { TableName = "Table1" };
                dsd.Tables.Add(tableInfo);
                tableInfo.Columns = new ObservableCollection<DataColumnInfo>();

                DataColumnInfo col = new DataColumnInfo { ColumnName = "EMPLOYEESALARYRECORDID", ColumnTitle = "薪资记录ID", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = true, IsReadOnly = true, IsRequired = true, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col);

                DataColumnInfo col1 = new DataColumnInfo { ColumnName = "EMPLOYEEID", ColumnTitle = "员工ID", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = false, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col1);

                DataColumnInfo col6 = new DataColumnInfo { ColumnName = "DEPARTMENT", ColumnTitle = "部门", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = false, IsShow = true, IsNeedSum = false };
                tableInfo.Columns.Add(col6);

                DataColumnInfo col7 = new DataColumnInfo { ColumnName = "POST", ColumnTitle = "工作岗位", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = false, IsShow = true, IsNeedSum = false };
                tableInfo.Columns.Add(col7);

                DataColumnInfo col2 = new DataColumnInfo { ColumnName = "EMPLOYEENAME", ColumnTitle = "姓名", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = false, IsShow = true, IsNeedSum = false };
                tableInfo.Columns.Add(col2);

                DataColumnInfo colPostCode = new DataColumnInfo { ColumnName = "POSTCODE", ColumnTitle = "职级代码", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = false, IsShow = true, IsNeedSum = false };
                tableInfo.Columns.Add(colPostCode);

                DataColumnInfo col3 = new DataColumnInfo { ColumnName = "SALARYYEAR", ColumnTitle = "发薪年份", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = false, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col3);

                DataColumnInfo col4 = new DataColumnInfo { ColumnName = "SALARYMONTH", ColumnTitle = "发薪月份", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = false, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col4);

                DataColumnInfo col5 = new DataColumnInfo { ColumnName = "ACTUALLYPAY", ColumnTitle = "实发金额", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = true, IsShow = true, IsNeedSum = true };
                tableInfo.Columns.Add(col5);

                //DataColumnInfo col8 = new DataColumnInfo { ColumnName = "MONTHLYBATCHID", ColumnTitle = "批量审核ID", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                //tableInfo.Columns.Add(col8);

                DataColumnInfo col9 = new DataColumnInfo { ColumnName = "POSTLEVEL", ColumnTitle = "岗位级别", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col9);

                DataColumnInfo col10 = new DataColumnInfo { ColumnName = "SALARYLEVEL", ColumnTitle = "薪资级别", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = false, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col10);

                DataColumnInfo col11 = new DataColumnInfo { ColumnName = "OWNERCOMPANYID", ColumnTitle = "发薪公司ID", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = false, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col11);

                DataColumnInfo col12 = new DataColumnInfo { ColumnName = "CHECKSTATE", ColumnTitle = "审核状态", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = false, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col12);

                DataColumnInfo col13 = new DataColumnInfo { ColumnName = "CREATEUSERID", ColumnTitle = "创建人ID", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = false, IsEncrypt = false, IsShow = false, IsNeedSum = false };
                tableInfo.Columns.Add(col13);

                //存储合计值
                Dictionary<string, decimal> dictSum = new Dictionary<string, decimal>();

                int j = 6;
                List<T_HR_EMPLOYEESALARYRECORDITEM> ItemsListForColName = salaryRecordTmp.salaryitem.OrderBy(s => s.ORDERNUMBER).ToList();
                foreach (var tmpItem in ItemsListForColName)
                {
                    var tp = modelSalaryItems.Where(s => s.SALARYITEMID == tmpItem.SALARYITEMID).FirstOrDefault();
                    DataColumnInfo colsalaryItem = null;
                    if (tp.SALARYITEMNAME == "税率" || tp.SALARYITEMNAME == "扣税基数" || tp.SALARYITEMNAME == "纳税系数")
                    {
                        colsalaryItem = new DataColumnInfo { ColumnName = "SALARYITEM" + j, ColumnTitle = tp == null ? "" : tp.SALARYITEMNAME, DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = true, IsShow = true, IsNeedSum = false };
                    }
                    else
                    {
                        colsalaryItem = new DataColumnInfo { ColumnName = "SALARYITEM" + j, ColumnTitle = tp == null ? "" : tp.SALARYITEMNAME, DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = true, IsShow = true, IsNeedSum = true };
                    }
                    tableInfo.Columns.Add(colsalaryItem);
                    j++;
                }

                DataColumnInfo colx = new DataColumnInfo { ColumnName = "REMARK", ColumnTitle = "备注", DataTypeName = typeof(string).FullName, MaxLength = 50, IsKey = false, IsReadOnly = true, IsRequired = true, IsEncrypt = true, IsShow = true, IsNeedSum = false };
                tableInfo.Columns.Add(colx);

                #endregion
                #region 构建xml  结构同DataSet getxml()相同
                StringBuilder sb = new StringBuilder();
                sb.Append("<NewDataSet>");

                foreach (var entItem in ents)
                {
                    sb.Append("<Table1>");

                    //职级代码
                    byte[] array = new byte[1];
                    array[0] = (byte)(65 + Convert.ToInt32(entItem.POSTLEVEL));
                    string PostCode = Encoding.ASCII.GetString(array).ToString();
                    PostCode += "-" + entItem.POSTLEVEL.ToString();

                    sb.Append("<EMPLOYEESALARYRECORDID>" + entItem.EMPLOYEESALARYRECORDID + "</EMPLOYEESALARYRECORDID>");
                    sb.Append("<EMPLOYEEID>" + entItem.EMPLOYEEID + "</EMPLOYEEID>");
                    sb.Append("<EMPLOYEENAME>" + entItem.EMPLOYEENAME + "</EMPLOYEENAME>");
                    sb.Append("<SALARYYEAR>" + entItem.SALARYYEAR + "</SALARYYEAR>");
                    sb.Append("<SALARYMONTH>" + entItem.SALARYMONTH + "</SALARYMONTH>");
                    sb.Append("<ACTUALLYPAY>" + entItem.ACTUALLYPAY + "</ACTUALLYPAY>");
                    sb.Append("<DEPARTMENT>" + entItem.DEPARTMENT + "</DEPARTMENT>");
                    sb.Append("<POST>" + entItem.POST + "</POST>");
                    sb.Append("<POSTCODE>" + PostCode + "</POSTCODE>");
                    //sb.Append("<MONTHLYBATCHID>" + entItem.MONTHLYBATCHID + "</MONTHLYBATCHID>");
                    sb.Append("<POSTLEVEL>" + entItem.POSTLEVEL.ToString() + "</POSTLEVEL>");
                    sb.Append("<SALARYLEVEL>" + entItem.SALARYLEVEL.ToString() + "</SALARYLEVEL>");
                    sb.Append("<OWNERCOMPANYID>" + entItem.OWNERCOMPANYID + "</OWNERCOMPANYID>");
                    sb.Append("<CHECKSTATE>" + entItem.CHECKSTATE + "</CHECKSTATE>");
                    sb.Append("<CREATEUSERID>" + entItem.CREATEUSERID + "</CREATEUSERID>");

                    tempTotalMoney += string.IsNullOrEmpty(entItem.ACTUALLYPAY) ? 0 : Convert.ToDecimal(AES.AESDecrypt(entItem.ACTUALLYPAY));

                    List<T_HR_EMPLOYEESALARYRECORDITEM> salaryItemsList = entItem.salaryitem.OrderBy(s => s.ORDERNUMBER).ToList();
                    int n = 6;
                    foreach (var detail in salaryItemsList)
                    {
                        sb.Append("<SALARYITEM" + n + ">" + detail.SUM + "</SALARYITEM" + n + ">");
                        if (dictSum.ContainsKey("SALARYITEM" + n))
                        {
                            dictSum["SALARYITEM" + n] += string.IsNullOrEmpty(detail.SUM) ? 0 : Convert.ToDecimal(AES.AESDecrypt(detail.SUM));
                        }
                        else
                        {
                            dictSum.Add("SALARYITEM" + n, string.IsNullOrEmpty(detail.SUM) ? 0 : Convert.ToDecimal(AES.AESDecrypt(detail.SUM)));
                        }
                        n++;
                    }
                    //查询加扣款 添加备注项
                    string remark = string.Empty;
                    if (entAddsums.Count() > 0)
                    {
                        var addSumsForEmployee = from c in entAddsums
                                                 where c.EMPLOYEEID == entItem.EMPLOYEEID
                                                 select c;
                        foreach (var addsum in addSumsForEmployee)
                        {
                            remark += addsum.PROJECTNAME + "：" + addsum.PROJECTMONEY + "；";
                        }

                        remark = AES.AESEncrypt(remark);
                    }
                    sb.Append("<REMARK>" + remark + "</REMARK>");
                    sb.Append("</Table1>");
                }
                //添加合计值
                sb.Append("<Table1>");
                sb.Append("<EMPLOYEESALARYRECORDID>" + "" + "</EMPLOYEESALARYRECORDID>");//合计值 不要设置ID
                sb.Append("<EMPLOYEEID>" + "" + "</EMPLOYEEID>");
                sb.Append("<EMPLOYEENAME>" + "--" + "</EMPLOYEENAME>");
                sb.Append("<SALARYYEAR>" + "" + "</SALARYYEAR>");
                sb.Append("<SALARYMONTH>" + "" + "</SALARYMONTH>");
                sb.Append("<ACTUALLYPAY>" + AES.AESEncrypt(tempTotalMoney.ToString()) + "</ACTUALLYPAY>");
                sb.Append("<DEPARTMENT>" + "合计" + "</DEPARTMENT>");
                sb.Append("<POST>" + "--" + "</POST>");
                sb.Append("<POSTCODE>" + "--" + "</POSTCODE>");
                //sb.Append("<MONTHLYBATCHID>" + "" + "</MONTHLYBATCHID>");
                sb.Append("<POSTLEVEL>" + "" + "</POSTLEVEL>");
                sb.Append("<SALARYLEVEL>" + "" + "</SALARYLEVEL>");
                sb.Append("<OWNERCOMPANYID>" + "" + "</OWNERCOMPANYID>");
                sb.Append("<CHECKSTATE>" + "" + "</CHECKSTATE>");
                sb.Append("<CREATEUSERID>" + "" + "</CREATEUSERID>");

                int t = 6;
                foreach (var ent in ItemsListForColName)
                {
                    var tp = modelSalaryItems.Where(s => s.SALARYITEMID == ent.SALARYITEMID).FirstOrDefault();
                    if (tp.SALARYITEMNAME == "税率" || tp.SALARYITEMNAME == "扣税基数" || tp.SALARYITEMNAME == "纳税系数")
                    {
                        sb.Append("<SALARYITEM" + t + ">" + AES.AESEncrypt("--") + "</SALARYITEM" + t + ">");
                    }
                    else
                    {
                        sb.Append("<SALARYITEM" + t + ">" + AES.AESEncrypt(dictSum["SALARYITEM" + t].ToString()) + "</SALARYITEM" + t + ">");
                    }
                    t++;
                }
                sb.Append("</Table1>");
                //合计值 和平均值
                tempAvgMoney = (Math.Round(Convert.ToDecimal(tempTotalMoney / counts), 2)).ToString();
                sb.Append("<Table2>");
                sb.Append("<sum>" + tempTotalMoney.ToString() + "</sum>");
                sb.Append("<avg>" + tempAvgMoney.ToString() + "</avg>");
                sb.Append("</Table2>");
                sb.Append("</NewDataSet>");
                #endregion
                dsd.DataXML = sb.ToString();

                return dsd;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取薪资记录审核统计后数据信息
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="orgtype"></param>
        /// <param name="orgid"></param>
        /// <param name="strCheckState"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<string> GetSalaryRecordAuditSum(string sort, string filterString, IList<object> paras, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID, out List<string> nameData)
        {
            List<string> result = new List<string>();
            nameData = new List<string>();
            bool signed = false;

            string year = string.Empty;
            string filter2 = "";
            List<string> queryParasDt = new List<string>();
            DateTime dtNull = Convert.ToDateTime("0001-1-1 0:00:00");

            List<object> queryParas = new List<object>();
            List<V_EMPLOYEESALARYRECORD> ent = new List<V_EMPLOYEESALARYRECORD>();
            queryParas.AddRange(paras);
            //IQueryable<V_EMPLOYEESALARYRECORD> ents = GetResultset(orgtype, orgid);
            //var en = ents.GroupBy(y => y.EMPLOYEESALARYRECORDID).Select(g => new { group = g.Key, groupcontent = g });
            //foreach (var v in en)
            //{
            //    ent.Add(v.groupcontent.FirstOrDefault());
            //}
            //ents = ent.AsQueryable();

            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       where a.OWNERCOMPANYID == orgid
                       select a;
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");

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
            ents = ents.OrderBy(m => m.SALARYYEAR);
            ents = ents.OrderBy(m => m.SALARYMONTH);

            if (ents.Count() > 0)
            {
                string sid = ents.FirstOrDefault().EMPLOYEESALARYRECORDID;
                decimal[] templist = new decimal[
                    (from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>().Include("T_HR_EMPLOYEESALARYRECORD")
                     where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == sid
                     orderby a.ORDERNUMBER
                     select a).ToList().Count];
                foreach (var e in ents)
                {
                    var tempents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>().Include("T_HR_EMPLOYEESALARYRECORD")
                                   where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == e.EMPLOYEESALARYRECORDID
                                   orderby a.ORDERNUMBER
                                   select a;
                    if (tempents.Count() > 0)
                    {
                        int i = 0;
                        foreach (var temp in tempents)
                        {
                            if (!signed)
                            {
                                var tempname = from d in dal.GetObjects<T_HR_SALARYITEM>()
                                               where d.SALARYITEMID == temp.SALARYITEMID
                                               select new
                                               {
                                                   SALARYITEMNAME = d.SALARYITEMNAME
                                               };
                                try
                                {
                                    nameData.Add(tempname.FirstOrDefault().SALARYITEMNAME);
                                }
                                catch { nameData.Add("0"); }
                            }
                            templist[i] += Convert.ToDecimal(string.IsNullOrEmpty(temp.SUM) ? "0" : AES.AESDecrypt(temp.SUM));
                            i++;
                        }
                        signed = true;
                    }

                }
                foreach (var tl in templist)
                {
                    result.Add(Math.Round(tl, 2).ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// 获取FB相关的薪资总额
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectID"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public V_RETURNFBI GetSalarySum(int objectType, string objectID, int year, int month, string userID)
        {
            int pageCount = 0;
            string[] paras = new string[1];
            V_RETURNFBI vfb = new V_RETURNFBI();
            DateTime starttime = new DateTime(year, month, 1);
            DateTime endtime = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            IQueryable<V_EMPLOYEESALARYRECORD> q = GetMassAuditSalaryRecordPagings(1, 3, "EMPLOYEESALARYRECORDID", "", paras, ref pageCount, starttime, endtime, objectType, objectID, "0", userID);
            if (q != null)
            {
                if (q.Count() > 0)
                {
                    V_EMPLOYEESALARYRECORD ent = q.FirstOrDefault();
                    if (ent != null)
                    {
                        vfb.COMPANYID = ent.OWNERCOMPANYID;
                        vfb.SALARYSUM = string.IsNullOrEmpty(ent.PAYTOTALMONEY) ? 0 : Convert.ToDecimal(ent.PAYTOTALMONEY);
                        return vfb;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 查询薪资记录批量审核实体
        /// </summary>
        /// <param name="SalaryRecordBatchID">薪资记录批量审核ID</param>
        /// <returns>返回薪资记录批量审核实体</returns>
        public T_HR_SALARYRECORDBATCH GetSalaryRecordBatchByID(string SalaryRecordBatchID)
        {
            if (SalaryRecordBatchID == string.Empty) return null;
            var ents = from a in dal.GetObjects<T_HR_SALARYRECORDBATCH>()
                       where a.MONTHLYBATCHID == SalaryRecordBatchID
                       select a;
            if (ents.Count() > 0) return ents.FirstOrDefault();
            return null;
        }

        /// <summary>
        /// 检测员工指定年月的薪资记录是否已经提交审核(用于删除社保缴交记录判断时使用),返回判断结果
        /// </summary>
        /// <param name="strmsg">查询返回的详细消息(无提交记录时，返回消息为空)</param>
        /// <param name="filterString">查询条件</param>
        /// <param name="paras">查询参数</param>
        /// <returns>true/false</returns>
        public bool GetSalaryBatchAuditState(ref string strmsg, string filterString, string[] paras)
        {
            bool bIsAudit = false;
            if (string.IsNullOrWhiteSpace(filterString))
            {
                return bIsAudit;
            }

            if (paras == null)
            {
                return bIsAudit;
            }

            if (paras.Count() == 0)
            {
                return bIsAudit;
            }

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       select a;

            ents = ents.Where(filterString, queryParas.ToArray());

            if (ents == null)
            {
                return bIsAudit;
            }

            if (ents.Count() == 0)
            {
                return bIsAudit;
            }

            int iAudit = 0;
            StringBuilder strTemp = new StringBuilder();
            foreach (var item in ents)
            {
                if (item.CHECKSTATE == Convert.ToInt32(CheckStates.Approving).ToString() || item.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    iAudit += 1;
                    strTemp.Append(item.EMPLOYEENAME + ";");
                }
            }

            if (strTemp.Length > 0)
            {
                strmsg = "员工：" + strTemp.ToString().TrimEnd(';') + "薪资记录已提交,不可删除对应的社保记录！";
            }

            if (iAudit > 0)
            {
                bIsAudit = true;
            }

            return bIsAudit;
        }

        /// <summary>
        /// 新增薪资记录批量审核
        /// </summary>
        /// <param name="entity">薪资记录批量审核实体</param>
        /// <returns></returns>
        public bool SalaryRecordBatchAdd(T_HR_SALARYRECORDBATCH entity, string[] salaryrecordids)
        {
            int i = 0;
            try
            {
                i = dal.Add(entity);
                if (i > 0)
                {
                    foreach (var id in salaryrecordids)
                    {
                        var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                   where a.EMPLOYEESALARYRECORDID == id
                                   select a;
                        if (ents.Count() > 0)
                        {
                            T_HR_EMPLOYEESALARYRECORD ent = ents.FirstOrDefault();// new T_HR_EMPLOYEESALARYRECORD();
                            //ents.FirstOrDefault();
                            //ent.T_HR_SALARYRECORDBATCHReference.EntityKey =
                            //    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYRECORDBATCH", "MONTHLYBATCHID", entity.MONTHLYBATCHID);

                            Utility.CloneEntity<T_HR_EMPLOYEESALARYRECORD>(ents.FirstOrDefault(), ent);
                            ent.CHECKSTATE = entity.CHECKSTATE;

                            string sql = @"UPDATE t_hr_employeesalaryrecord t ";
                            sql += " SET t.checkstate = '" + entity.CHECKSTATE + "',t.monthlybatchid='" + entity.MONTHLYBATCHID + "'";
                            sql += " WHERE t.employeesalaryrecordid = '" + ent.EMPLOYEESALARYRECORDID + "'";
                            EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                            bll.ExecuteSql(sql, "t_hr_employeesalaryrecord");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                ex.Message.ToString();
            }
            if (i > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 更新薪资记录批量审核
        /// </summary>
        /// <param name="entity">薪资记录批量审核实体</param>
        /// <returns></returns>
        public void SalaryRecordBatchUpdate(T_HR_SALARYRECORDBATCH entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.MONTHLYBATCHID == entity.MONTHLYBATCHID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_SALARYRECORDBATCH>(entity, ent);
                    dal.Update(ent);
                    var entrecords = from b in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                     join c in dal.GetObjects<T_HR_SALARYRECORDBATCH>() on b.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID equals c.MONTHLYBATCHID
                                     where c.MONTHLYBATCHID == entity.MONTHLYBATCHID
                                     select b;
                    if (entrecords.Count() > 0)
                    {
                        foreach (var en in entrecords)
                        {
                            T_HR_EMPLOYEESALARYRECORD temp = new T_HR_EMPLOYEESALARYRECORD();
                            Utility.CloneEntity<T_HR_EMPLOYEESALARYRECORD>(en, temp);
                            temp.CHECKSTATE = entity.CHECKSTATE;
                            string sql = @"UPDATE t_hr_employeesalaryrecord t ";
                            sql += " SET t.checkstate = '" + entity.CHECKSTATE + "'";
                            sql += " WHERE t.employeesalaryrecordid = '" + en.EMPLOYEESALARYRECORDID + "'";
                            //dal.ExecuteCustomerSql(sql);
                            EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                            bll.ExecuteSql(sql, "t_hr_employeesalaryrecord");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                ex.Message.ToString();
                //throw ex;
            }
        }

        /// <summary>
        /// 删除薪资记录批量审核，可同时删除多行记录
        /// </summary>
        /// <param name="SalaryRecordBatchIDs">薪资记录批量审核ID数组</param>
        /// <returns></returns>
        public int SalaryRecordBatchDelete(string[] SalaryRecordBatchIDs)
        {
            foreach (string id in SalaryRecordBatchIDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYRECORDBATCH>()
                           where e.MONTHLYBATCHID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    // DataContext.DeleteObject(ent);
                }
            }
            return dal.SaveContextChanges(); //DataContext.SaveChanges();
        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                dal.BeginTransaction();
                var recordBatch = (from c in dal.GetObjects<T_HR_SALARYRECORDBATCH>()
                                   where c.MONTHLYBATCHID == EntityKeyValue
                                   select c).FirstOrDefault();
                if (recordBatch != null)
                {
                    recordBatch.CHECKSTATE = CheckState;
                    recordBatch.UPDATEDATE = DateTime.Now;
                    dal.UpdateFromContext(recordBatch);
                    string strSql = " update T_HR_EMPLOYEESALARYRECORD t  set t.checkstate ='" + CheckState + "' where t.monthlybatchid='" + EntityKeyValue + "' ";
                    dal.ExecuteCustomerSql(strSql);
                    i = dal.SaveContextChanges();
                }
                dal.CommitTransaction();

                //当单据审核通过时，调用FB服务，生成一张对应公司的活动经费下拨单_____2012/8/10 9:50:24注释，暂停使用
                string strMsg=string.Empty;
                AssignPersonMoney(CheckState, recordBatch.OWNERCOMPANYID, ref strMsg);

                return i;
            }
            catch (Exception e)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }

        /// <summary>
        /// 根据薪资批量审核单的状态，及下拨公司的是否为一级分公司，确定是否产生下拨单到集团人力资源中心总经理处
        /// </summary>
        /// <param name="strCheckState"></param>
        /// <param name="strCompanyID"></param>
        public void AssignPersonMoney(string strCheckState, string strCompanyID, ref string strMsg)
        {
            try
            {
                if (strCheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    CompanyBLL bllComp = new CompanyBLL();
                    var entComp =from ent in  dal.GetTable<T_HR_COMPANY>()
                                          where ent.COMPANYID==strCompanyID
                                              select ent;
                    string strAssignCompanyID = System.Configuration.ConfigurationManager.AppSettings["PersonMoneyAssignCompany"];
                    if (entComp.Count()<1)
                    {
                        strMsg = "下拨公司(公司ID:" + strCompanyID + ")不存在，生成下拨公司(公司ID:" + strCompanyID + ")的活动经费下拨单失败";
                        SMT.Foundation.Log.Tracer.Debug(strMsg);
                        return;
                    }

                    if (entComp.FirstOrDefault().FATHERID != strAssignCompanyID)
                    {
                        strMsg = "下拨公司(公司ID:" + strCompanyID + ")不属于一级分公司，不生成下拨公司(公司ID:" + strCompanyID + ")的活动经费下拨单";
                        SMT.Foundation.Log.Tracer.Debug(strMsg);
                        return;
                    }

                    using (SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient clientFB = new SaaS.BLLCommonServices.FBServiceWS.FBServiceClient())
                    {
                        string strAssignOwnerID = System.Configuration.ConfigurationManager.AppSettings["PersonMoneyAssignOwner"];
                        clientFB.CreatePersonMoneyAssignInfo(strCompanyID, strAssignOwnerID);
                        strMsg = "生成下拨公司(公司ID:" + strCompanyID + ")的活动经费下拨单成功";
                        SMT.Foundation.Log.Tracer.Debug(strMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                strMsg = "生成下拨公司(公司ID:" + strCompanyID + ")的活动经费下拨单失败，失败原因：" + ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(strMsg);
            }
        }

    }
}
