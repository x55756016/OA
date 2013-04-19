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
using System.Reflection;

namespace SMT.HRM.BLL
{
    public class CustomGuerdonRecordBLL: BaseBll<T_HR_CUSTOMGUERDONRECORD>, ILookupEntity
    {
        protected int nsize = 2;
        protected bool IsComputer = false;
        protected string[] construe = new string[9];
        protected SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient FBSclient = new SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient();
        /// <summary>
        /// 根据自定义薪资记录ID查询实体
        /// </summary>
        /// <param name="CustomGuerdonRecordID">自定义薪资记录ID</param>
        /// <returns>返回自定义薪资记录实体</returns>
        public T_HR_CUSTOMGUERDONRECORD GetCustomGuerdonRecordByID(string CustomGuerdonRecordID)
        {
            var ents = from a in dal.GetTable()
                       where a.CUSTOMGUERDONRECORDID == CustomGuerdonRecordID 
                       select a;
            
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据员工ID查询自定义薪资记录实体
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>返回自定义薪资记录实体</returns>
        public T_HR_CUSTOMGUERDONRECORD GetEmployeeCustomRecordOne(string employeeID, string year, string month)
        {
            var ents = from r in dal.GetTable()
                       where r.EMPLOYEEID == employeeID && r.SALARYMONTH == month && r.SALARYYEAR == year
                       select r;
            if (ents.Count() > 0)
            {
                return ents.FirstOrDefault();                   
            }
            return null;
        }

        /// <summary>
        /// 根据员工ID查询自定义薪资记录实体
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>返回自定义薪资记录实体</returns>
        public List<T_HR_CUSTOMGUERDONRECORD> GetEmployeeCustomRecord(string employeeID, string year, string month)
        {
           var ents = from r in dal.GetTable()
                       where r.EMPLOYEEID == employeeID && r.SALARYMONTH == month && r.SALARYYEAR == year
                       select r;
           List<T_HR_CUSTOMGUERDONRECORD> record = new List<T_HR_CUSTOMGUERDONRECORD>();
           if (ents.Count() > 0)
           {
               foreach (var e in ents)
                   record.Add(e);
           }
           return record;
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_CUSTOMGUERDONRECORD> ents = from a in DataContext.T_HR_CUSTOMGUERDONRECORD
        //                                             select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            IQueryable<T_HR_CUSTOMGUERDONRECORD> ents = 
                from a in dal.GetObjects()
                                                     select a;
            return ents.Count() > 0 ? ents.ToArray() : null;
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
        public IQueryable<T_HR_CUSTOMGUERDONRECORD> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime,string strCheckState, string userID)
        {
            string year = string.Empty;
            List<object> queryParas = new List<object>();
            List<string> queryParasDt = new List<string>();
            DateTime dtNull = Convert.ToDateTime("0001-1-1 0:00:00");

            List<object> objArgs = new List<object>();
            objArgs.Add(paras);

            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_CUSTOMGUERDONRECORD");

            //SetFilterWithflow("CUSTOMGUERDONRECORDID", "T_HR_CUSTOMGUERDONRECORD", userID, ref strCheckState, ref filterString, ref objArgs);

            SetFilterWithflow("CUSTOMGUERDONRECORDID", "T_HR_CUSTOMGUERDONRECORD", userID, ref strCheckState, ref filterString, ref queryParas);

            if (!string.IsNullOrEmpty(strCheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                filterString += " CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(strCheckState);
            }

            IQueryable<T_HR_CUSTOMGUERDONRECORD> ents = dal.GetObjects();
            try
            {
                ents = (IQueryable<T_HR_CUSTOMGUERDONRECORD>)MatchFilter(ents, "EMPLOYEENAME", "EMPLOYEENAME==@0 and ", filterString, queryParas);
            }
            catch { }

            //string s = filterString.Replace("EMPLOYEENAME==@0 and ", string.Empty);
            //filterString = s;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            #region ---字符日期检索
            //string filter2 = "";
            //int t = 0;
            //if (starttime != dtNull && endtime != dtNull)
            //{
            //    for (int i = starttime.Year; i <= endtime.Year; i++)
            //    {
            //        year = i.ToString();
            //        if (t == 0)                                                                                       //起始段                                                                                          
            //        {
            //            if (starttime.Year == endtime.Year)
            //            {
            //                ents = ents.Where(m => m.SALARYYEAR == year);
            //                if (starttime.Month == endtime.Month)                                                     //同年同月                                                         
            //                {
            //                    string month = starttime.Month.ToString();
            //                    ents = ents.Where(m => m.SALARYMONTH == month);
            //                    break;
            //                }
            //                else                                                                                      //同年不同月
            //                {
            //                    bool temp = false;
            //                    for (int j = starttime.Month; j <= endtime.Month; j++)
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
            //            else                                                                                          //不同年月                                                                                                                
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
            //        else if (Convert.ToInt32(year) == endtime.Year)                                                   //末段                                
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
            //        else                                                                                              //中段                                                                                                   
            //        {
            //            filter2 += " or  SALARYYEAR==@" + queryParasDt.Count().ToString();
            //            queryParasDt.Add(year);
            //        }
            //        t++;
            //    }
            //}
            #endregion

            //string s = paras[0].ToString();
            //string filterStr = " @0.Contains(EMPLOYEENAME)";
            //List<object> objArgs1 = new List<object>();
            //objArgs1.Add(s);
            //ents = ents.Where(filterStr, objArgs1.ToArray());

            //ents = ents.Where(x => s.IndexOf(x.EMPLOYEENAME) > -1);

            ents = ents.OrderBy(sort);
            ents = Utility.Pager<T_HR_CUSTOMGUERDONRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        //public object MatchFilter(string entsname, string filters, string filter, string filterString, List<object> queryParas)
        //{
        //    Type types = Type.GetType("SMT.HRM.BLL." + entsname + "BLL");

        //    if (types != null)
        //    {
        //        int i = filterString.IndexOf(filters);
        //        string s = filterString.Replace(filter, string.Empty);
        //        filterString = s;
        //        if (i > 0) queryParas.RemoveAt(0);
        //        MethodInfo mth = types.GetMethod("Where");
        //        object[] obj = { filterString, queryParas.ToArray() };
        //        object ob = mth.Invoke(entsname, new object[2] { filterString, queryParas.ToArray() });
        //        return ob;
        //    }
        //    return null;
        //}

        public object MatchFilter<T>(T entsname,string filters, string filter,string filterString,List<object> queryParas)
        {
            Type types = entsname.GetType();
            if (types != null)
            {
                int i = filterString.IndexOf(filters);
                string s = filterString.Replace(filter, string.Empty);
                filterString = s;
                if(i>0)queryParas.RemoveAt(0);
                MethodInfo mth = types.GetMethod("Where");
                object objtypes = Activator.CreateInstance(types);
                object[] obj = { filterString, queryParas.ToArray() };
                object ob = mth.Invoke(objtypes, new object[2] { filterString, queryParas.ToArray() });
                return ob;
            }
            return null;
        }

        /// <summary>
        /// 更新自定义薪资记录
        /// </summary>
        /// <param name="entity">自定义薪资记录实体</param>
        public void CustomGuerdonRecordUpdate(T_HR_CUSTOMGUERDONRECORD entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.CUSTOMGUERDONRECORDID == entity.CUSTOMGUERDONRECORDID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    ent.GENERATETYPE = entity.GENERATETYPE;
                    ent.SALARYSUM = entity.SALARYSUM;
                    ent.CHECKSTATE = entity.CHECKSTATE;
                    ent.EMPLOYEEID = entity.EMPLOYEEID;
                    ent.EMPLOYEECODE = entity.EMPLOYEECODE;
                    ent.EMPLOYEENAME = entity.EMPLOYEENAME;
                    ent.SALARYMONTH = entity.SALARYMONTH;
                    ent.SALARYYEAR = entity.SALARYYEAR;
                    ent.GUERDONNAME = entity.GUERDONNAME;
                    ent.CUSTOMERGUERDONID = entity.CUSTOMERGUERDONID;
                    ent.REMARK = entity.REMARK;
                    ent.UPDATEDATE = entity.UPDATEDATE;
                    ent.UPDATEUSERID = entity.UPDATEUSERID;
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除自定义薪资记录，可同时删除多行记录
        /// </summary>
        /// <param name="CustomGuerdonRecords">自定义薪资记录ID数组</param>
        /// <returns></returns>
        public int CustomGuerdonRecordDelete(string[] CustomGuerdonRecords)
        {
            foreach (string id in CustomGuerdonRecords)
            {
                var ents = from e in dal.GetObjects()
                           where e.CUSTOMGUERDONRECORDID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }

            return dal.SaveContextChanges(); 
        }
        public int CustomGuerdonRecordDelete(string employeeID, string year, string month)
        {
            var ents = from a in dal.GetObjects()
                       where a.EMPLOYEEID == employeeID && a.SALARYYEAR == year && a.SALARYMONTH == month
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    if (ent != null) dal.DeleteFromContext(ent);
                }
                return dal.SaveContextChanges();  
            }
            else
            {
                return 0;
            }
        }


        public string CustomGuerdonRecordAccount(int objectType, string objectID, int year, int month, string construes)
        {
            bool result = true;
            string resultstr = string.Empty;
            construe = construes.Split(';');
            #region 预算....
            //List<FBServiceBLLWS.T_FB_BUDGETACCOUNT> fblist;
            //try
            //{
            //List<V_RETURNFBI> glist = CustomGuerdonRecord(objectType, objectID, year, month, true);      //统计自定义薪资
            //    if (glist.Count > 0)
            //    {
            //    if (objectType == 0)
            //        fblist = FBSclient.FetchSalaryBudget(glist.FirstOrDefault().COMPANYID, "").ToList();    //预算核算
            //    else
            //        fblist = FBSclient.FetchSalaryBudget(glist.FirstOrDefault().COMPANYID, glist.FirstOrDefault().DEPARTMENTID).ToList();
            //    foreach (V_RETURNFBI v in glist)
            //    {
            //        foreach (FBServiceBLLWS.T_FB_BUDGETACCOUNT fb in fblist)
            //        {
            //            if ((v.COMPANYID == fb.OWNERCOMPANYID) && (v.DEPARTMENTID == fb.OWNERDEPARTMENTID))
            //            {
            //                if (v.SALARYSUM > fb.USABLEMONEY)
            //                {
            //                    resultstr += v.DEPARTMENTNAME + ";";
            //                    result = false;
            //                }
            //            }
            //        }
            //    }
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

            //if (result) CustomGuerdonRecord(objectType, objectID, year, month, false);
            #endregion
            CustomGuerdonRecord(objectType, objectID, year, month, false);
            return resultstr;
        }


        public List<V_RETURNFBI> CustomGuerdonRecord(int objectType, string objectID, int year, int month, bool markType)
        {
            var ents1 = from c in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                        where c.DEPARTMENTID == objectID
                        select new
                        {
                            COMPANYID = c.T_HR_COMPANY.COMPANYID,
                            COMPANYNAME = c.T_HR_COMPANY.CNAME,
                            DEPARTMENTID = c.DEPARTMENTID,
                            DEPARTMENTNAME = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                        };
            var ents2 = from b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT").Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                        where b.POSTID == objectID
                        select new
                        {
                            COMPANYID = b.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                            COMPANYNAME = b.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                            DEPARTMENTID = b.T_HR_DEPARTMENT.DEPARTMENTID,
                            DEPARTMENTNAME = b.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                        };
            var ents3 = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").Include("T_HR_DEPARTMENT").Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                        where a.T_HR_EMPLOYEE.EMPLOYEEID == objectID
                        select new
                        {
                            COMPANYID = a.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                            COMPANYNAME = a.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                            DEPARTMENTID = a.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                            DEPARTMENTNAME = a.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                        };

            List<V_RETURNFBI> vfb = new List<V_RETURNFBI>();
            if (!markType)
            {
                IsComputer = true;
                switch (objectType)
                {
                    case 0:
                        GenerateCompanyRecord(objectID, year.ToString(), month.ToString());
                        break;
                    case 1:
                        GenerateDepartmentRecord(objectID, year.ToString(), month.ToString());
                        break;
                    case 2:
                        GeneratePostRecord(objectID, year.ToString(), month.ToString());
                        break;
                    case 3:
                        GenerateEmployeeRecord(objectID, year.ToString(), month.ToString());
                        break;
                }
            }
            else
            {
                switch (objectType)
                {
                    case 0:
                        vfb = GenerateCompanyRecord(objectID, year.ToString(), month.ToString());
                        break;
                    case 1:
                        if (ents1.Count() > 0)
                        {
                            V_RETURNFBI vr1 = new V_RETURNFBI();
                            var en = ents1.FirstOrDefault();
                            vr1.COMPANYID = en.COMPANYID;
                            vr1.COMPANYNAME = en.COMPANYNAME;
                            vr1.DEPARTMENTID = en.DEPARTMENTID;
                            vr1.DEPARTMENTNAME = en.DEPARTMENTNAME;
                            vr1.SALARYSUM = GenerateDepartmentRecord(objectID, year.ToString(), month.ToString());
                            vfb.Add(vr1);
                        }
                        break;
                    case 2:
                        if (ents2.Count() > 0)
                        {
                            V_RETURNFBI vr2 = new V_RETURNFBI();
                            var en = ents2.FirstOrDefault();
                            vr2.COMPANYID = en.COMPANYID;
                            vr2.COMPANYNAME = en.COMPANYNAME;
                            vr2.DEPARTMENTID = en.DEPARTMENTID;
                            vr2.DEPARTMENTNAME = en.DEPARTMENTNAME;
                            vr2.SALARYSUM = GeneratePostRecord(objectID, year.ToString(), month.ToString());
                            vfb.Add(vr2);
                        }
                        break;
                    case 3:
                        if (ents3.Count() > 0)
                        {
                            V_RETURNFBI vr3 = new V_RETURNFBI();
                            var en = ents3.FirstOrDefault();
                            vr3.COMPANYID = en.COMPANYID;
                            vr3.COMPANYNAME = en.COMPANYNAME;
                            vr3.DEPARTMENTID = en.DEPARTMENTID;
                            vr3.DEPARTMENTNAME = en.DEPARTMENTNAME;
                            vr3.SALARYSUM = GenerateEmployeeRecord(objectID, year.ToString(), month.ToString());
                            vfb.Add(vr3);
                        }
                        break;
                }
            }

            return vfb;
        }

        public List<V_RETURNFBI> GenerateCompanyRecord(string companyID, string year, string month)
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
                vr.COMPANYNAME = ents.FirstOrDefault().CNAME;
                vr.DEPARTMENTID = emp.DEPARTMENTID;
                vr.DEPARTMENTNAME = dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                vr.SALARYSUM = GenerateDepartmentRecord(emp.DEPARTMENTID, year, month);
                lvr.Add(vr);
            }
            return lvr;
        }
        public decimal GenerateDepartmentRecord(string departID, string year, string month)
        {
            decimal result = 0;
            PostBLL bll = new PostBLL();
            List<T_HR_POST> emplist = bll.GetPostByDepartId(departID);
            foreach (var emp in emplist)
            {
               result += GeneratePostRecord(emp.POSTID, year, month);
            }
            return result;
        }
        public decimal GeneratePostRecord(string postID, string year, string month)
        {
            decimal result = 0;
            EmployeePostBLL bll = new EmployeePostBLL();
            List<T_HR_EMPLOYEEPOST> emplist = bll.GetEmployeePostByPostID(postID);
            foreach (var emp in emplist)
            {
               result += GenerateEmployeeRecord(emp.T_HR_EMPLOYEE.EMPLOYEEID, year, month);
            }
            return result;
        }

        public decimal GenerateEmployeeRecord(string employeeID, string year, string month)
        {
            decimal total = 0;
            var ents = from a in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>()
                       join b in dal.GetObjects<T_HR_CUSTOMGUERDONSET>() on a.CUSTOMERGUERDONID equals b.CUSTOMGUERDONSETID
                       join c in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.T_HR_SALARYARCHIVE.SALARYARCHIVEID equals c.SALARYARCHIVEID
                       where a.T_HR_SALARYARCHIVE.EMPLOYEEID == employeeID
                       select new
                       {
                           GENERATETYPE = b.CALCULATORTYPE,
                           SUM = a.SUM,
                           CHECKSTATE = a.T_HR_SALARYARCHIVE.CHECKSTATE,
                           EMPLOYEEID = a.T_HR_SALARYARCHIVE.EMPLOYEEID,
                           EMPLOYEECODE = a.T_HR_SALARYARCHIVE.EMPLOYEECODE,
                           EMPLOYEENAME = a.T_HR_SALARYARCHIVE.EMPLOYEENAME,
                           GUERDONNAME = b.GUERDONNAME,
                           CUSTOMERGUERDONID = b.CUSTOMGUERDONSETID,
                           GUERDONCATEGORY = b.GUERDONCATEGORY,
                           REMARK = c.REMARK
                       };

                T_HR_EMPLOYEESALARYRECORD esrecord = GetSalaryRecord(employeeID, year, month);

                List<T_HR_CUSTOMGUERDONRECORD> recordlist = GetEmployeeCustomRecord(employeeID, year, month);

                nsize = GetAccuracy(employeeID);
                if (recordlist.Count() > 0)                                                        //已经有至少一个自定义记录存在
                {
                    CustomGuerdonRecordDelete(employeeID, year, month);
                } 
                foreach (var en in ents)
                {
                    T_HR_CUSTOMGUERDONRECORD cgrecord = new T_HR_CUSTOMGUERDONRECORD();
                    cgrecord.CUSTOMGUERDONRECORDID = Guid.NewGuid().ToString();
                    cgrecord.EMPLOYEEID = employeeID;
                    cgrecord.GENERATETYPE = en.GENERATETYPE;
                    cgrecord.SALARYSUM = en.SUM;
                    cgrecord.CHECKSTATE = en.CHECKSTATE;
                    cgrecord.EMPLOYEEID = en.EMPLOYEEID;
                    cgrecord.EMPLOYEECODE = en.EMPLOYEECODE;
                    cgrecord.EMPLOYEENAME = en.EMPLOYEENAME;
                    cgrecord.GUERDONNAME = en.GUERDONNAME;
                    cgrecord.CUSTOMERGUERDONID = en.CUSTOMERGUERDONID;
                    cgrecord.REMARK = en.REMARK;
                    cgrecord.SALARYMONTH = month;
                    cgrecord.SALARYYEAR = year;
                    cgrecord.CREATEDATE = System.DateTime.Now;
                    try
                    {
                        cgrecord.CREATEUSERID = construe[0];
                        cgrecord.CREATEPOSTID = construe[1];
                        cgrecord.CREATEDEPARTMENTID = construe[2];
                        cgrecord.CREATECOMPANYID = construe[3];

                        cgrecord.OWNERID = construe[4];
                        cgrecord.OWNERPOSTID = construe[5];
                        cgrecord.OWNERDEPARTMENTID = construe[6];
                        cgrecord.OWNERCOMPANYID = construe[7];
                    }
                    catch (Exception exx)
                    {
                        exx.Message.ToString();
                    }

                    if (en.GUERDONCATEGORY == "1")                                      //1是加2是减
                        total += Convert.ToDecimal(en.SUM);
                    else
                    {
                        total -= Convert.ToDecimal(en.SUM);
                        cgrecord.SALARYSUM = -en.SUM;
                    }
                    if (IsComputer) 
                    { 
                        dal.Add(cgrecord);
                    }
                }

            if (esrecord != null)
            {
                decimal? temp = esrecord.CUSTOMERSUM;
                esrecord.CUSTOMERSUM = Math.Round(total, nsize);
                esrecord.ACTUALLYPAY += Math.Round(Convert.ToDecimal(total - temp), nsize);
                //esrecord.PERSONALINCOMETAX = 更新个税
                esrecord.SUBTOTAL += Math.Round(Convert.ToDecimal(total - temp), nsize);
                if (IsComputer) new EmployeeSalaryRecordBLL().Update(esrecord);
            }
            return total;
        }

        #region ------------重要的备用代码
        //public decimal GenerateEmployeeRecord(string employeeID, string year, string month)
        //{
        //    decimal total = 0;
        //    var ents = from a in DataContext.T_HR_CUSTOMGUERDONARCHIVE
        //               join b in DataContext.T_HR_CUSTOMGUERDONSET on a.CUSTOMERGUERDONID equals b.CUSTOMGUERDONSETID
        //               join c in DataContext.T_HR_SALARYARCHIVE  on a.T_HR_SALARYARCHIVE.SALARYARCHIVEID equals c.SALARYARCHIVEID
        //               where a.T_HR_SALARYARCHIVE.EMPLOYEEID == employeeID
        //               select new 
        //               {
        //                    GENERATETYPE = b.CALCULATORTYPE,
        //                    SUM = a.SUM,
        //                    CHECKSTATE = a.T_HR_SALARYARCHIVE.CHECKSTATE,
        //                    EMPLOYEEID = a.T_HR_SALARYARCHIVE.EMPLOYEEID,
        //                    EMPLOYEECODE = a.T_HR_SALARYARCHIVE.EMPLOYEECODE,
        //                    EMPLOYEENAME = a.T_HR_SALARYARCHIVE.EMPLOYEENAME,
        //                    GUERDONNAME = b.GUERDONNAME,
        //                    CUSTOMERGUERDONID = b.CUSTOMGUERDONSETID,
        //                    GUERDONCATEGORY = b.GUERDONCATEGORY,
        //                    REMARK = c.REMARK
        //               };

        //    T_HR_EMPLOYEESALARYRECORD esrecord = GetSalaryRecord(employeeID, year, month);

        //    List<T_HR_CUSTOMGUERDONRECORD> recordlist = GetEmployeeCustomRecord(employeeID, year, month);

        //    nsize = GetAccuracy(employeeID);

        //    if (recordlist.Count<0)                                                        //没有一个记录时生成记录
        //    {
        //        foreach(var en in ents)
        //        {
        //           T_HR_CUSTOMGUERDONRECORD cgrecord = new T_HR_CUSTOMGUERDONRECORD();
        //            cgrecord.CUSTOMGUERDONRECORDID = Guid.NewGuid().ToString();
        //            cgrecord.EMPLOYEEID = employeeID;
        //            cgrecord.GENERATETYPE = en.GENERATETYPE;
        //            cgrecord.SALARYSUM = en.SUM;
        //            cgrecord .CHECKSTATE = en.CHECKSTATE;
        //            cgrecord .EMPLOYEEID = en.EMPLOYEEID;
        //            cgrecord .EMPLOYEECODE = en.EMPLOYEECODE;
        //            cgrecord .EMPLOYEENAME = en.EMPLOYEENAME;
        //            cgrecord .GUERDONNAME = en.GUERDONNAME;
        //            cgrecord.CUSTOMERGUERDONID = en.CUSTOMERGUERDONID;
        //            cgrecord .REMARK = en.REMARK;
        //            cgrecord.SALARYMONTH = month;
        //            cgrecord.SALARYYEAR = year;
        //            cgrecord.CREATEDATE = System.DateTime.Now;
        //            try
        //            {
        //                cgrecord.CREATEUSERID = construe[0];
        //                cgrecord.CREATEPOSTID = construe[1];
        //                cgrecord.CREATEDEPARTMENTID = construe[2];
        //                cgrecord.CREATECOMPANYID = construe[3];

        //                cgrecord.OWNERID = construe[4];
        //                cgrecord.OWNERPOSTID = construe[5];
        //                cgrecord.OWNERDEPARTMENTID = construe[6];
        //                cgrecord.OWNERCOMPANYID = construe[7];
        //            }
        //            catch (Exception exx)
        //            {
        //                exx.Message.ToString();
        //            }

        //            if (en.GUERDONCATEGORY == "1")                                      //1是加2是减
        //                total += Convert.ToDecimal(en.SUM);
        //            else
        //            {
        //                total -= Convert.ToDecimal(en.SUM);
        //                cgrecord.SALARYSUM = -en.SUM;
        //            }
        //            if (IsComputer) dal.Add(cgrecord);
        //        }
        //    }
        //    else                                                                        //有部分记录时生成记录or完全没有记录
        //    {
        //        foreach (var en in ents)
        //        {
        //            bool isOld=false;
        //            T_HR_CUSTOMGUERDONRECORD cgrecord = new T_HR_CUSTOMGUERDONRECORD();
        //            foreach (T_HR_CUSTOMGUERDONRECORD record in recordlist)
        //            {
        //                if (en.CUSTOMERGUERDONID == record.CUSTOMERGUERDONID)
        //                {
        //                    cgrecord.CUSTOMGUERDONRECORDID = record.CUSTOMGUERDONRECORDID;
        //                    cgrecord.UPDATEDATE = System.DateTime.Now;
        //                    isOld = true; break;
        //                }
        //            }
        //            cgrecord.EMPLOYEEID = employeeID;
        //            cgrecord.GENERATETYPE = en.GENERATETYPE;
        //            cgrecord.SALARYSUM = en.SUM;
        //            cgrecord.CHECKSTATE = en.CHECKSTATE;
        //            cgrecord.EMPLOYEEID = en.EMPLOYEEID;
        //            cgrecord.EMPLOYEECODE = en.EMPLOYEECODE;
        //            cgrecord.EMPLOYEENAME = en.EMPLOYEENAME;
        //            cgrecord.GUERDONNAME = en.GUERDONNAME;
        //            cgrecord.CUSTOMERGUERDONID = en.CUSTOMERGUERDONID;
        //            cgrecord.REMARK = en.REMARK;
        //            cgrecord.SALARYMONTH = month;
        //            cgrecord.SALARYYEAR = year;
        //            if (en.GUERDONCATEGORY == "1")
        //            {
        //                total += Convert.ToDecimal(en.SUM);
        //            }
        //            else
        //            {
        //                total -= Convert.ToDecimal(en.SUM);
        //                cgrecord.SALARYSUM = -en.SUM;
        //            }
        //            if (isOld)
        //            {
        //                if (IsComputer) dal.Update(cgrecord);                            
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    cgrecord.CREATEUSERID = construe[0];
        //                    cgrecord.CREATEPOSTID = construe[1];
        //                    cgrecord.CREATEDEPARTMENTID = construe[2];
        //                    cgrecord.CREATECOMPANYID = construe[3];

        //                    cgrecord.OWNERID = construe[4];
        //                    cgrecord.OWNERPOSTID = construe[5];
        //                    cgrecord.OWNERDEPARTMENTID = construe[6];
        //                    cgrecord.OWNERCOMPANYID = construe[7];
        //                }
        //                catch (Exception exx)
        //                {
        //                    exx.Message.ToString();
        //                }
        //                cgrecord.CUSTOMGUERDONRECORDID = Guid.NewGuid().ToString();   
        //                cgrecord.CREATEDATE = System.DateTime.Now;
        //                if (IsComputer) dal.Add(cgrecord);
        //            }

        //        }
        //        //cgrecord.UPDATEUSERID = entity.UPDATEUSERID
        //    }
        //    if (esrecord != null)
        //    {
        //        decimal? temp = esrecord.CUSTOMERSUM;
        //        esrecord.CUSTOMERSUM = Math.Round(total,nsize);
        //        esrecord.ACTUALLYPAY += Math.Round(Convert.ToDecimal(total - temp),nsize);
        //        //esrecord.PERSONALINCOMETAX = 更新个税
        //        esrecord.SUBTOTAL +=  Math.Round(Convert.ToDecimal(total - temp),nsize);
        //        if (IsComputer) new EmployeeSalaryRecordBLL().Update(esrecord);
        //    }
        //    return total;
        //}
        #endregion

        /// <summary>
        /// 获取薪资精度
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回薪资精度</returns>
        private int GetAccuracy(string employeeid)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYARCHIVE>()
                       join  b in dal.GetObjects<T_HR_SALARYSOLUTION>() on a.SALARYSOLUTIONID  equals b.SALARYSOLUTIONID
                       where a.EMPLOYEEID == employeeid
                       select new
                       {
                           SALARYPRECISION = b.SALARYPRECISION
                       };
            if (ents.Count() > 0) return Convert.ToInt32(ents.FirstOrDefault().SALARYPRECISION);
            return nsize;
        }

        public T_HR_EMPLOYEESALARYRECORD GetSalaryRecord(string employeeID,string year,string month)                              //必须是为审核的
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       where a.EMPLOYEEID == employeeID && a.SALARYMONTH == month && a.SALARYYEAR == year && a.CHECKSTATE == "2"
                       select a;
            T_HR_EMPLOYEESALARYRECORD record = ents.Count() > 0 ? ents.FirstOrDefault() : null;
            return record;
        }

    }
}
