/*
 * 文件名：ChartReportsBll.cs
 * 作  用：T_HR_EMPLOYEEENTRY 业务逻辑类
 * 创建人：魏瑞
 * 创建时间：2012-7-6
 * 修改人：
 * 修改时间：
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.HRM.CustomModel.Reports;
using SMT_HRM_EFModel;

using SMT.HRM.DAL;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using System.Data;
using SMT.SaaS.BLLCommonServices;
using System.Data.Objects;
using System.Collections.ObjectModel;


namespace SMT.HRM.BLL.Report
{
    public class ChartReportsBll : BaseBll<T_HR_EMPLOYEE>
    {
        /// <summary>
        /// 得到所有员工数据根据公司ID
        /// </summary>
        /// <param name="Companyid"></param>
        /// <returns></returns>
        public List<EmployeeMarriage> NewGetAllEmployeeentryInfo(string Companyid, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                var q = from ent in dal.GetTable()
                        join entry in dal.GetTable<T_HR_EMPLOYEEENTRY>()
                        on ent.EMPLOYEEID equals entry.T_HR_EMPLOYEE.EMPLOYEEID
                        where ent.OWNERCOMPANYID == Companyid
                        && entry.ENTRYDATE >= StartDate
                        && entry.ENTRYDATE <= EndDate
                        orderby entry.ENTRYDATE
                        select new EmployeeMarriage
                        {
                            DataEnty = entry.ENTRYDATE,
                            MarriageEnty = ent.MARRIAGE,
                            EmployName = ent.EMPLOYEECNAME
                        };
                if (q.Count() > 0)
                {
                    return q.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            // List<EmployeeentryEnty> listEmploy = null;

        }

        /// <summary>
        /// 人工婚姻报表Pie 
        /// </summary>
        /// <param name="Companyid">公司ID</param>
        /// <param name="StartDate">开始时间</param>
        /// <param name="EndDate">结束时间</param>
        /// <param name="filterString">where</param>
        /// <param name="paras"></param>
        /// <param name="userID">员工ID</param>
        /// <returns></returns>
        public List<PieEmployeece> GetPieEmployeeceInfo(string Companyid, string style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID) 
        {
            try
            {
                #region 暂时删除（只查本公司）
                ////根据传递过来的Companyid查询公司表，得到是否有子公司
                //var CompanyInfo = from ent in dal.GetObjects<T_HR_COMPANY>()
                //                  where ent.FATHERID == Companyid
                //                  select new 
                //                  {
                //                      COMPANYID = ent.COMPANYID,
                //                      //name = ent.CNAME
                //                  };
                //List<object> queryParas = new List<object>();
                //queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                //IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                //                                 select o;
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    ents = ents.Where(filterString, queryParas.ToArray());
                //}
                //if (ents.Count()>0)
                //{
                    //var q = from ent in ents
                    //        join entry in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                    //        on ent.EMPLOYEEID equals entry.T_HR_EMPLOYEE.EMPLOYEEID
                    //        where ent.OWNERCOMPANYID == Companyid
                    //        && ent.EMPLOYEESTATE != "2"
                    //        && entry.ENTRYDATE >= StartDate
                    //        && entry.ENTRYDATE <= EndDate
                //        group ent by ent.MARRIAGE into g
                //        select new PieEmployeece
                //        {
                //            marriage = g.Key,
                //            CountEmployeece = g.Count()
                //        };
                //if (q.Count() > 0)
                //{
                //    return q.ToList();
                //}
                //}
                #endregion
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                                                 select o;
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }

                if (ents.Count()>0)
                {
                    ReportsDAL repDal = new ReportsDAL();
                    DataTable dt = repDal.GetDataEmployeecInfo(Companyid, style, StartDate, EndDate);
                    List<PieEmployeece> list = new List<PieEmployeece>();
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            PieEmployeece pie = new PieEmployeece();
                            pie.EMPLOYEEID = item["EMPLOYEEID"].ToString();
                            pie.marriage = item["MARRIAGE"].ToString();
                            pie.Name = item["EMPLOYEECNAME"].ToString();
                            list.Add(pie);
                        }

                        if (list.Count() > 0)
                        {
                            var q = from entry in list
                                    join ent in ents 
                                    on entry .EMPLOYEEID equals ent.EMPLOYEEID
                                    group entry by entry.marriage into g
                                    select new PieEmployeece
                                    {
                                        marriage = g.Key,
                                        CountEmployeece = g.Count()
                                    };
                            return q.ToList();
                        }
                    }
                }
                return null;
	        }
            catch (Exception ex)
	        {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "GetPieEmployeeceInfo:" + ex.Message);
                throw ex;	        
            }
        }


        /// <summary>
        /// 员工性别报表Pie
        /// </summary>
        /// <param name="Companyid"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<PieEmployeece> GetSexPieEmployeeceInfo(string Companyid, string style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                #region 暂时删除 只查本公司
                //List<object> queryParas = new List<object>();
                //queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                //IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                //                                 select o;
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    ents = ents.Where(filterString, queryParas.ToArray());
                //}

                //if (ents.Count() > 0)
                //{
                //    var q = from ent in dal.GetObjects()
                //            join entry in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                //            on ent.EMPLOYEEID equals entry.T_HR_EMPLOYEE.EMPLOYEEID
                //            where ent.OWNERCOMPANYID == Companyid
                //            && ent.EMPLOYEESTATE != "2"
                //            && entry.ENTRYDATE >= StartDate
                //            && entry.ENTRYDATE <= EndDate
                //            group ent by ent.SEX into g
                //            select new PieEmployeece
                //            {
                //                Sex = g.Key,
                //                CountEmployeece = g.Count()
                //            };
                //    if (q.Count() > 0)
                //    {
                //        return q.ToList();
                //    }
                //}
                #endregion 
                ReportsDAL repDal = new ReportsDAL();
                DataTable dt = repDal.GetDataEmployeecInfo(Companyid, style, StartDate, EndDate);

                List<PieEmployeece> list = new List<PieEmployeece>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        PieEmployeece pie = new PieEmployeece();
                        pie.Sex = item["SEX"].ToString();
                        pie.Name = item["EMPLOYEECNAME"].ToString();
                        list.Add(pie);
                    }
                }

                if (list.Count() > 0)
                {
                    var q = from ent in list
                            group ent by ent.Sex into g
                            select new PieEmployeece
                            {
                                Sex = g.Key,
                                CountEmployeece = g.Count()
                            };
                    return q.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "GetSexPieEmployeeceInfo:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 员工学历报表Pie
        /// </summary>
        /// <param name="Companyid"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<PieEmployeece> GetEducationPieEmployeeceInfo(string Companyid, string style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                #region 暂时删除 只查本公司
                //List<object> queryParas = new List<object>();
                //queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                //IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                //                                 select o;
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    ents = ents.Where(filterString, queryParas.ToArray());
                //}

                //if (ents.Count() > 0)
                //{
                //    var q = from ent in dal.GetObjects()
                //            join entry in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                //            on ent.EMPLOYEEID equals entry.T_HR_EMPLOYEE.EMPLOYEEID
                //            where ent.OWNERCOMPANYID == Companyid
                //            && ent.EMPLOYEESTATE != "2"
                //            && entry.ENTRYDATE >= StartDate
                //            && entry.ENTRYDATE <= EndDate
                //            group ent by ent.TOPEDUCATION into g
                //            orderby g.Key descending
                //            select new PieEmployeece
                //            {
                //                Education=g.Key,
                //                CountEmployeece = g.Count()
                //            };

                //    if (q.Count() > 0)
                //    {
                //        return q.ToList();
                //    }
                //}

                #endregion
                ReportsDAL repDal = new ReportsDAL();
                DataTable dt = repDal.GetDataEmployeecInfo(Companyid, style, StartDate, EndDate);
                List<PieEmployeece> list = new List<PieEmployeece>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        PieEmployeece pie = new PieEmployeece();
                        pie.Education = item["TOPEDUCATION"].ToString();
                        pie.Name = item["EMPLOYEECNAME"].ToString();
                        list.Add(pie);
                    }
                }

                if (list.Count() > 0)
                {
                    //不聚合，到前面计算
                    var q = from ent in list
                            orderby ent.Education
                            select ent;
                            //group ent by ent.Education into g
                            //select new PieEmployeece
                            //{
                            //    Education = g.Key,
                            //    CountEmployeece = g.Count()
                            //};
                    return q.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "GetEducationPieEmployeeceInfo:" + ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// 年龄对比Pie图
        /// </summary>
        /// <param name="Companyid"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<PieEmployeece> GetAgePieEmployeeceInfo(string Companyid,string Style,string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                #region 暂时删除 只查本公司
                //List<object> queryParas = new List<object>();
                //queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                //IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                //                                 select o;
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    ents = ents.Where(filterString, queryParas.ToArray());
                //}

                //if (ents.Count() > 0)
                //{
                //    var q = from ent in dal.GetObjects()
                //            join entry in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                //            on ent.EMPLOYEEID equals entry.T_HR_EMPLOYEE.EMPLOYEEID
                //            where ent.OWNERCOMPANYID == Companyid
                //            && ent.EMPLOYEESTATE != "2"
                //            && entry.ENTRYDATE >= StartDate
                //            && entry.ENTRYDATE <= EndDate
                //            orderby ent.BIRTHDAY
                //            select new PieEmployeece
                //            {
                //                Birthday = ent.BIRTHDAY,
                //                Name = ent.EMPLOYEECNAME,
                //                EMPLOYEEID = ent.EMPLOYEEID,
                //                //年龄默认全是0
                //                Age ="0",
                //                //
                //                CountEmployeece =0
                //            };
                //    if (q.Count() > 0)
                //    {
                //        return q.ToList();
                //    }
                //}
                #endregion
                ReportsDAL repDal = new ReportsDAL();
                DataTable dt = repDal.GetDataEmployeecInfo(Companyid, Style, StartDate, EndDate);
                List<PieEmployeece> list = new List<PieEmployeece>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        PieEmployeece pie = new PieEmployeece();
                        //出身年月
                        if (!string.IsNullOrEmpty(item["BIRTHDAY"].ToString()))
                        {
                            pie.Birthday =DateTime.Parse(item["BIRTHDAY"].ToString());
                            pie.Name = item["EMPLOYEECNAME"].ToString();
                            list.Add(pie);
                        }
                    }
                }
                if (list.Count>0)
                {
                    //不聚合，到前面计算
                    var q = from ent in list
                            orderby ent.Birthday
                            select ent;
                    return q.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "GetAgePieEmployeeceInfo:" + ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// 员工司龄对比Pie图
        /// </summary>
        /// <param name="Companyid"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<PieEmployeece> GetLengthServicePie(string Companyid, string Style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                #region 暂时删除 只查本公司
                //List<object> queryParas = new List<object>();
                //queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                //IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                //                                 select o;
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    ents = ents.Where(filterString, queryParas.ToArray());
                //}

                //if (ents.Count() > 0)
                //{
                //    var q = from ent in dal.GetObjects()
                //            join entry in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                //            on ent.EMPLOYEEID equals entry.T_HR_EMPLOYEE.EMPLOYEEID
                //            where ent.OWNERCOMPANYID == Companyid
                //            && ent.EMPLOYEESTATE != "2"
                //            && entry.ENTRYDATE >= StartDate
                //            && entry.ENTRYDATE <= EndDate
                //            orderby entry.ENTRYDATE
                //            select new PieEmployeece
                //            {
                //                ENTRYDATE = entry.ENTRYDATE,
                //                Name = ent.EMPLOYEECNAME,
                //                EMPLOYEEID = ent.EMPLOYEEID,
                //                //
                //                CountEmployeece = 0
                //            };
                //    if (q.Count() > 0)
                //    {
                //        return q.ToList();
                //    }
                //}
                #endregion
                ReportsDAL repDal = new ReportsDAL();
                DataTable dt = repDal.GetDataEmployeecInfo(Companyid, Style, StartDate, EndDate);
                List<PieEmployeece> list = new List<PieEmployeece>();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        PieEmployeece pie = new PieEmployeece();
                        if (!string.IsNullOrEmpty(item["ENTRYDATE"].ToString()))
                        {
                            //入职日期
                            pie.ENTRYDATE = DateTime.Parse(item["ENTRYDATE"].ToString());
                            pie.Name = item["EMPLOYEECNAME"].ToString();
                            list.Add(pie);
                        }
                    }
                }
                if (list.Count > 0)
                {
                    //不聚合，到前面计算
                    var q = from ent in list
                            orderby ent.ENTRYDATE
                            select ent;
                    return q.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "GetLengthServicePie:" + ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// 集团各产业员工分配比例
        /// </summary>
        /// <param name="Companyid"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<PieEmployeece> GetAllCompanyInfo(string Companyid, string Style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                #region 暂时删掉 只查本公司
                //List<object> queryParas = new List<object>();
                //queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                //IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                //                                 select o;
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    ents = ents.Where(filterString, queryParas.ToArray());
                //}

                //if (ents.Count()>0)
                //{
                //    var q = from ent in dal.GetObjects()
                //            join entry in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                //            on ent.EMPLOYEEID equals entry.T_HR_EMPLOYEE.EMPLOYEEID
                //            join m in dal.GetObjects<T_HR_COMPANY>()
                //            on ent.OWNERCOMPANYID equals m.COMPANYID
                //            where ent.EMPLOYEESTATE != "2"
                //            && entry.ENTRYDATE >= StartDate
                //            && entry.ENTRYDATE <= EndDate
                //            group ent by ent.OWNERCOMPANYID into g
                //            select new PieEmployeece
                //            {
                //                OWNERCOMPANYID =g.Key,
                //                CountEmployeece=g.Count()
                //            };
                //}
                #endregion

                ReportsDAL reportDal = new ReportsDAL();
                DataTable dt = reportDal.GetEmployeecByYearMM(Companyid, Style, StartDate, EndDate);
                List<PieEmployeece> list = new List<PieEmployeece>();
                if (dt.Rows.Count>0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        PieEmployeece employee = new PieEmployeece();
                        //员工ID
                        employee.EMPLOYEEID = dr["EMPLOYEEID"].ToString();
                        //员工姓名
                        employee.Name = dr["Name"].ToString();

                        //公司ID
                        employee.OWNERCOMPANYID = dr["OWNERCOMPANYID"].ToString();
                        //公司英文名字
                        employee.ENAME = dr["ENAME"].ToString();
                        //公司中文名字
                        employee.CNAME = dr["CNAME"].ToString();
                        list.Add(employee);
                    }  
                }

                if (list.Count() >0)
                {
                    //聚合
                    var q = from t in list
                            group t by t.CNAME into g
                            select new PieEmployeece
                            {
                                //公司中文名字
                                CNAME = g.Key,
                                //人数
                                CountEmployeece = g.Count()
                            };
                    return q.ToList();
                }

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "GetAllCompanyInfo:" + ex.Message);
                throw ex;
            }
            return null;
        }
    }
}
