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
   public class SalaryArchiveHisBLL :BaseBll<T_HR_SALARYARCHIVEHIS>
    {
       /// <summary>
       /// 添加薪资档案历史
       /// </summary>
       /// <param name="obj"></param>
       /// <returns></returns>
       public void SalaryArchiveHisAdd(T_HR_SALARYARCHIVEHIS obj)
        {
            T_HR_SALARYARCHIVEHIS ent = new T_HR_SALARYARCHIVEHIS();
            Utility.CloneEntity<T_HR_SALARYARCHIVEHIS>(obj, ent);
            dal.Add(ent);
        }

       /// <summary>
       /// 根据ID获取薪资档案历史
       /// </summary>
       /// <param name="ID"></param>
       /// <returns></returns>
       public T_HR_SALARYARCHIVEHIS GetSalaryArchiveHisByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVEHIS>()
                       where o.SALARYARCHIVEID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
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
       public IQueryable<T_HR_SALARYARCHIVEHIS> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string userID)
        {
            string year = string.Empty;
            List<object> queryParas = new List<object>();

            List<T_HR_SALARYARCHIVEHIS> ent = new List<T_HR_SALARYARCHIVEHIS>();
            //IQueryable<T_HR_SALARYARCHIVEHIS> ents = GetSalaryArchiveHisFilter(orgtype, orgid);
            //var en = ents.GroupBy(y => y.SALARYARCHIVEID).Select(g => new { group = g.Key, groupcontent = g });
            //foreach (var v in en)
            //{
            //    ent.Add(v.groupcontent.FirstOrDefault());
            //}
            //ents = ent.AsQueryable();

            queryParas.AddRange(paras);

            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYARCHIVEHIS");

            IQueryable<T_HR_SALARYARCHIVEHIS> ents = dal.GetObjects<T_HR_SALARYARCHIVEHIS>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }

            ents = ents.Where(m => m.CREATEDATE>=starttime);

            ents = ents.Where(m => m.CREATEDATE <= endtime);

            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYARCHIVEHIS>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

       /// <summary>
       /// 薪资档案历史过滤
       /// </summary>
       /// <returns></returns>
       public IQueryable<T_HR_SALARYARCHIVEHIS> GetSalaryArchiveHisFilter(int orgtype, string orgid)
       {
           IQueryable<T_HR_SALARYARCHIVEHIS> ents = dal.GetObjects<T_HR_SALARYARCHIVEHIS>();
           switch (orgtype)
           {
               case 0:
                   ents = from a in dal.GetObjects<T_HR_SALARYARCHIVEHIS>()
                          join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                          join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                          join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                          where d.T_HR_COMPANY.COMPANYID == orgid && b.EDITSTATE == "1" && b.ISAGENCY == "0"
                          select a;
                   break;
               case 1:
                   ents = from a in dal.GetObjects<T_HR_SALARYARCHIVEHIS>()
                          join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                          join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                          join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                          where d.DEPARTMENTID == orgid && b.EDITSTATE == "1" && b.ISAGENCY == "0"
                          select a;
                   break;
               case 2:
                   ents = from a in dal.GetObjects<T_HR_SALARYARCHIVEHIS>()
                          join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                          join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                          where c.POSTID == orgid && b.EDITSTATE == "1" && b.ISAGENCY == "0"
                          select a;
                   break;
           }
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
       public IQueryable<V_SALARYARCHIVES> VQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string userID)
       {
           string year = string.Empty;
           List<object> queryParas = new List<object>();

           List<T_HR_SALARYARCHIVEHIS> ent = new List<T_HR_SALARYARCHIVEHIS>();
           IQueryable<T_HR_SALARYARCHIVEHIS> ents = GetSalaryArchiveHisFilter(orgtype, orgid);

           var en = ents.GroupBy(y => y.SALARYARCHIVEID).Select(g => new { group = g.Key, groupcontent = g });
           foreach (var v in en)
           {
               ent.Add(v.groupcontent.FirstOrDefault());
           }
           ents = ent.AsQueryable();

           queryParas.AddRange(paras);

           SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYARCHIVEHIS");

           if (!string.IsNullOrEmpty(filterString))
           {
               ents = ents.Where(filterString, queryParas.ToArray());
           }

           ents = ents.Where(m => m.CREATEDATE >= starttime);

           ents = ents.Where(m => m.CREATEDATE <= endtime);

           ents = ents.OrderBy(sort);

           IQueryable<V_SALARYARCHIVES> e = GetVSalaryArchiveHis(ents);

           e = Utility.Pager<V_SALARYARCHIVES>(e, pageIndex, pageSize, ref pageCount);

           return e;

       }

       /// <summary>
       /// 薪资档案历史过滤视图化
       /// </summary>
       /// <returns></returns>
       public IQueryable<V_SALARYARCHIVES> GetVSalaryArchiveHis(IQueryable<T_HR_SALARYARCHIVEHIS> his )
       {
           List<V_SALARYARCHIVES> ents = new List<V_SALARYARCHIVES> (); 
           foreach(var a in his)
           {
               V_SALARYARCHIVES temp = new V_SALARYARCHIVES();
               var ent = from b in dal.GetObjects<T_HR_SALARYSTANDARD>()
                         join c in dal.GetObjects<T_HR_SALARYLEVEL>() on b.T_HR_SALARYLEVEL.SALARYLEVELID equals c.SALARYLEVELID
                         join d in dal.GetObjects<T_HR_POSTLEVELDISTINCTION>() on c.T_HR_POSTLEVELDISTINCTION.POSTLEVELID equals d.POSTLEVELID
                         where b.SALARYSTANDARDID == a.SALARYSTANDARDID
                         select new  
                         {
                            SALARYLEVEL = c.SALARYLEVEL,
                            POSTLEVEL = d.POSTLEVEL,
                            SALARYSUM = c.SALARYSUM
                         };
               if (ent.Count() > 0)
               {
                   temp.POSTLEVEL = ent.FirstOrDefault().POSTLEVEL!=null? ent.FirstOrDefault().POSTLEVEL.ToString():string.Empty;
                   temp.SALARYLEVEL = ent.FirstOrDefault().SALARYLEVEL;
               }
               temp.SALARYARCHIVEID = a.SALARYARCHIVEID;
               temp.EMPLOYEENAME = a.EMPLOYEENAME;
               temp.EMPLOYEECODE = a.EMPLOYEECODE;
               temp.CREATEDATE = a.CREATEDATE;
               ents.Add(temp);
           }
           return ents.AsQueryable();
       }
      
       
    }
}
