using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using TM_SaaS_OA_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using System.Data;
using System.Data.Objects;
using System.Collections.ObjectModel;
using SMT.HRM.IMServices.IMServiceWS;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SMT.Foundation.Log;
using SMT.HRM.BLL.Common;
using SMT.HRM.BLL.Permission;



namespace SMT.HRM.BLL
{
    public class EmployeeBLL : BaseBll<T_HR_EMPLOYEE>, ILookupEntity
    {

        public void  test()
        {
            int pageCount = 0;
            var ents = (from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                    orderby ent.CREATEDATE descending
                    select ent).AsQueryable();
            ents = Utility.Pager<T_HR_EMPLOYEE>(ents, 1, 20, ref pageCount);
            var items = ents.ToList();
        }
        /// <summary>
        /// 员工档案分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEE> GetEmployeesPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");


            IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                                             join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                             where (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                                             select o;
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select o;
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           where d.DEPARTMENTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select o;
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           where p.POSTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select o;
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEE>(ents, pageIndex, pageSize, ref pageCount);
            return ents;

        }
        /// <summary>
        /// 模糊查找员工
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> SearchEmployeesByName(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, string strCompanyID, ref int pageCount)
        {

            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              where (ep.EDITSTATE == "1" && ep.CHECKSTATE == "2") && (filterString.Contains(o.EMPLOYEECNAME) || filterString.Contains(o.EMAIL))
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMAIL = o.EMAIL,
                                                  OWNERCOMPANYID = o.OWNERCOMPANYID
                                              };

            ents = ents.OrderBy(sort);
            if (!string.IsNullOrEmpty(strCompanyID))
            {
                paras = new List<object>();
                paras.Add(strCompanyID);
                ents = ents.Where(" OWNERCOMPANYID=@0", paras.ToArray());
            }
            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }

        /// <summary>
        /// 根据岗位id获取岗位下面员工(有权限控制),目前只组织架构用到，所以只传岗位id和当前员工id即可
        /// 权限控制实体为T_HR_EMPLOYEE，与人事档案权限一致
        /// </summary>
        /// <param name="postID">岗位id</param>
        /// <param name="userID">当前员工id</param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEE> GetEmployeePostByPostIDView(string postID, string userID)
        {
            #region 根据权限过滤数据
            List<object> paras = new List<object>();
            string filterString = "";
            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_EMPLOYEE");
            }
            #endregion  employeePosts

            var ents = from emp in dal.GetObjects()
                       join em in dal.GetObjects<T_HR_EMPLOYEEPOST>() on emp.EMPLOYEEID equals em.T_HR_EMPLOYEE.EMPLOYEEID
                       join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                       join pd in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pd.POSTDICTIONARYID
                       where p.POSTID == postID && em.EDITSTATE == "1"
                       select emp;

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }

            return ents.Count() > 0 ? ents.ToList() : null;

        }
        /// <summary>
        /// 员工基本信息 weirui 2012/8/20 添加
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeBasicInfoPagingView(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");


            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                              where o.EMPLOYEESTATE != "2"
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  //员工ID
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  //员工中文名
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  //员工编号
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  //员工英文名
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  //员工性别
                                                  SEX = o.SEX,
                                                  //证件号码
                                                  IDNUMBER = o.IDNUMBER,
                                                  //手机号码
                                                  MOBILE = o.MOBILE,
                                                  //电子邮件
                                                  EMAIL = o.EMAIL,
                                                  //指纹ID
                                                  FINGERPRINTID = o.FINGERPRINTID,

                                                  //公司ID
                                                  OWNERCOMPANYID = o.OWNERCOMPANYID,
                                                  //部门ID
                                                  OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                                                  //岗位ID
                                                  OWNERPOSTID = o.OWNERPOSTID,
                                                  //员工状态
                                                  EMPLOYEESTATE = o.EMPLOYEESTATE,

                                                  OWNERID = o.EMPLOYEEID,
                                                  CREATEUSERID = o.CREATEUSERID

                                              };
            switch (sType)
            {
                //公司
                case "Company":
                    ents = from o in dal.GetObjects()
                           where o.OWNERCOMPANYID == sValue && o.EMPLOYEESTATE != "2"
                           select new V_EMPLOYEEVIEW
                           {
                               //员工ID
                               EMPLOYEEID = o.EMPLOYEEID,
                               //员工中文名
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               //员工编号
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               //员工英文名
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               //员工性别
                               SEX = o.SEX,
                               //证件号码
                               IDNUMBER = o.IDNUMBER,
                               //手机号码
                               MOBILE = o.MOBILE,
                               //电子邮件
                               EMAIL = o.EMAIL,
                               //指纹ID
                               FINGERPRINTID = o.FINGERPRINTID,

                               //公司ID
                               OWNERCOMPANYID = o.OWNERCOMPANYID,
                               //部门ID
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               //岗位ID
                               OWNERPOSTID = o.OWNERPOSTID,
                               //员工状态
                               EMPLOYEESTATE = o.EMPLOYEESTATE,

                               OWNERID = o.EMPLOYEEID,
                               CREATEUSERID = o.CREATEUSERID
                           };
                    break;
                //部门
                case "Department":
                    ents = from o in dal.GetObjects()
                           where o.OWNERDEPARTMENTID == sValue && o.EMPLOYEESTATE != "2"
                           select new V_EMPLOYEEVIEW
                           {
                               //员工ID
                               EMPLOYEEID = o.EMPLOYEEID,
                               //员工中文名
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               //员工编号
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               //员工英文名
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               //员工性别
                               SEX = o.SEX,
                               //证件号码
                               IDNUMBER = o.IDNUMBER,
                               //手机号码
                               MOBILE = o.MOBILE,
                               //电子邮件
                               EMAIL = o.EMAIL,
                               //指纹ID
                               FINGERPRINTID = o.FINGERPRINTID,

                               //公司ID
                               OWNERCOMPANYID = o.OWNERCOMPANYID,
                               //部门ID
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               //岗位ID
                               OWNERPOSTID = o.OWNERPOSTID,
                               //员工状态
                               EMPLOYEESTATE = o.EMPLOYEESTATE,

                               OWNERID = o.EMPLOYEEID,
                               CREATEUSERID = o.CREATEUSERID
                           };
                    break;
                //岗位
                case "Post":
                    ents = from o in dal.GetObjects()
                           where o.OWNERPOSTID == sValue && o.EMPLOYEESTATE != "2"
                           select new V_EMPLOYEEVIEW
                           {
                               //员工ID
                               EMPLOYEEID = o.EMPLOYEEID,
                               //员工中文名
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               //员工编号
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               //员工英文名
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               //员工性别
                               SEX = o.SEX,
                               //证件号码
                               IDNUMBER = o.IDNUMBER,
                               //手机号码
                               MOBILE = o.MOBILE,
                               //电子邮件
                               EMAIL = o.EMAIL,
                               //指纹ID
                               FINGERPRINTID = o.FINGERPRINTID,

                               //公司ID
                               OWNERCOMPANYID = o.OWNERCOMPANYID,
                               //部门ID
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               //岗位ID
                               OWNERPOSTID = o.OWNERPOSTID,
                               //员工状态
                               EMPLOYEESTATE = o.EMPLOYEESTATE,

                               OWNERID = o.EMPLOYEEID,
                               CREATEUSERID = o.CREATEUSERID

                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }

        /// <summary>
        /// 获取员工通讯录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetContactsListPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");


            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                              join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                              join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                              where (o.EMPLOYEESTATE!="2" && ep.ISAGENCY=="0" && ep.CHECKSTATE=="2" && ep.EDITSTATE=="1")
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  COMPANYNAME = c.CNAME,
                                                  DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                  POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                                  SEX = o.SEX,
                                                  EMAIL = o.EMAIL,
                                                  IDNUMBER = o.IDNUMBER,
                                                  FINGERPRINTID = o.FINGERPRINTID,
                                                  MOBILE = o.MOBILE,
                                                  OWNERID = o.EMPLOYEEID,
                                                  OWNERPOSTID = p.POSTID,
                                                  OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                  OWNERCOMPANYID = c.COMPANYID,
                                                  CREATEUSERID = o.CREATEUSERID,
                                                  EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                                  ISAGENCY = ep.ISAGENCY,
                                                  POSTLEVEL = ep.POSTLEVEL,
                                                  OFFICEPHONE = o.OFFICEPHONE
                                              };
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && (o.EMPLOYEESTATE != "2" && ep.ISAGENCY == "0" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where d.DEPARTMENTID == sValue && (o.EMPLOYEESTATE != "2" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where p.POSTID == sValue && (o.EMPLOYEESTATE != "2" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               ISAGENCY = ep.ISAGENCY,
                               POSTLEVEL = ep.POSTLEVEL,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }

        /// <summary>
        /// 导出员工通讯录
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        public byte[] ExportContactsList(string companyID, string filterString, string[] paras, string sType, string sValue, string userId)
        {
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);


                var company = (from e in dal.GetObjects<T_HR_COMPANY>()
                               where e.COMPANYID == companyID
                               select e).FirstOrDefault();
                string companyName = company.CNAME;
                #region 获取公司所有员工
                IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                                  join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                                  join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                                  join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                                  join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                                  where (o.EMPLOYEESTATE != "2" && ep.ISAGENCY == "0" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1")
                                                  select new V_EMPLOYEEVIEW
                                                  {
                                                      EMPLOYEEID = o.EMPLOYEEID,
                                                      EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                      EMPLOYEECODE = o.EMPLOYEECODE,
                                                      EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                      COMPANYNAME = c.CNAME,
                                                      DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                      POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                                      SEX = o.SEX,
                                                      EMAIL = o.EMAIL,
                                                      IDNUMBER = o.IDNUMBER,
                                                      FINGERPRINTID = o.FINGERPRINTID,
                                                      MOBILE = o.MOBILE,
                                                      OWNERID = o.EMPLOYEEID,
                                                      OWNERPOSTID = p.POSTID,
                                                      OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                      OWNERCOMPANYID = c.COMPANYID,
                                                      CREATEUSERID = o.CREATEUSERID,
                                                      EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                                      ISAGENCY = ep.ISAGENCY,
                                                      POSTLEVEL = ep.POSTLEVEL,
                                                      OFFICEPHONE = o.OFFICEPHONE
                                                  };
                switch (sType)
                {
                    case "Company":
                        ents = from o in dal.GetObjects()
                               join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                               join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                               join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                               join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                               where c.COMPANYID == sValue && (o.EMPLOYEESTATE != "2" && ep.ISAGENCY == "0" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1")
                               select new V_EMPLOYEEVIEW
                               {
                                   EMPLOYEEID = o.EMPLOYEEID,
                                   EMPLOYEECNAME = o.EMPLOYEECNAME,
                                   EMPLOYEECODE = o.EMPLOYEECODE,
                                   EMPLOYEEENAME = o.EMPLOYEEENAME,
                                   COMPANYNAME = c.CNAME,
                                   DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                   POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                   SEX = o.SEX,
                                   EMAIL = o.EMAIL,
                                   IDNUMBER = o.IDNUMBER,
                                   FINGERPRINTID = o.FINGERPRINTID,
                                   MOBILE = o.MOBILE,
                                   OWNERID = o.EMPLOYEEID,
                                   OWNERPOSTID = p.POSTID,
                                   OWNERDEPARTMENTID = d.DEPARTMENTID,
                                   OWNERCOMPANYID = c.COMPANYID,
                                   CREATEUSERID = o.CREATEUSERID,
                                   POSTLEVEL = ep.POSTLEVEL,
                                   ISAGENCY = ep.ISAGENCY,
                                   EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                   OFFICEPHONE = o.OFFICEPHONE
                               };
                        break;
                    case "Department":
                        ents = from o in dal.GetObjects()
                               join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                               join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                               join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                               join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                               where d.DEPARTMENTID == sValue && (o.EMPLOYEESTATE != "2" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1")
                               select new V_EMPLOYEEVIEW
                               {
                                   EMPLOYEEID = o.EMPLOYEEID,
                                   EMPLOYEECNAME = o.EMPLOYEECNAME,
                                   EMPLOYEECODE = o.EMPLOYEECODE,
                                   EMPLOYEEENAME = o.EMPLOYEEENAME,
                                   COMPANYNAME = c.CNAME,
                                   DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                   POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                   SEX = o.SEX,
                                   EMAIL = o.EMAIL,
                                   IDNUMBER = o.IDNUMBER,
                                   FINGERPRINTID = o.FINGERPRINTID,
                                   MOBILE = o.MOBILE,
                                   OWNERID = o.EMPLOYEEID,
                                   OWNERPOSTID = p.POSTID,
                                   OWNERDEPARTMENTID = d.DEPARTMENTID,
                                   OWNERCOMPANYID = c.COMPANYID,
                                   CREATEUSERID = o.CREATEUSERID,
                                   POSTLEVEL = ep.POSTLEVEL,
                                   ISAGENCY = ep.ISAGENCY,
                                   EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                   OFFICEPHONE = o.OFFICEPHONE
                               };
                        break;
                    case "Post":
                        ents = from o in dal.GetObjects()
                               join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                               join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                               join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                               join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                               where p.POSTID == sValue && (o.EMPLOYEESTATE != "2" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1")
                               select new V_EMPLOYEEVIEW
                               {
                                   EMPLOYEEID = o.EMPLOYEEID,
                                   EMPLOYEECNAME = o.EMPLOYEECNAME,
                                   EMPLOYEECODE = o.EMPLOYEECODE,
                                   EMPLOYEEENAME = o.EMPLOYEEENAME,
                                   COMPANYNAME = c.CNAME,
                                   DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                   POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                   SEX = o.SEX,
                                   EMAIL = o.EMAIL,
                                   IDNUMBER = o.IDNUMBER,
                                   FINGERPRINTID = o.FINGERPRINTID,
                                   MOBILE = o.MOBILE,
                                   OWNERID = o.EMPLOYEEID,
                                   OWNERPOSTID = p.POSTID,
                                   OWNERDEPARTMENTID = d.DEPARTMENTID,
                                   OWNERCOMPANYID = c.COMPANYID,
                                   CREATEUSERID = o.CREATEUSERID,
                                   EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                   ISAGENCY = ep.ISAGENCY,
                                   POSTLEVEL = ep.POSTLEVEL,
                                   OFFICEPHONE = o.OFFICEPHONE
                               };
                        break;
                }
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                #endregion

                byte[] result;
                
                #region 定义表头名称
                DataTable dt = new DataTable();
                DataColumn colComp = new DataColumn();
                colComp.ColumnName = "公司";
                colComp.DataType = typeof(string);
                dt.Columns.Add(colComp);

                DataColumn colDept = new DataColumn();
                colDept.ColumnName = "部门";
                colDept.DataType = typeof(string);
                dt.Columns.Add(colDept);

                DataColumn colPost = new DataColumn();
                colPost.ColumnName = "岗位";
                colPost.DataType = typeof(string);
                dt.Columns.Add(colPost);

                DataColumn colCordSD = new DataColumn();
                colCordSD.ColumnName = "姓名";
                colCordSD.DataType = typeof(string);
                dt.Columns.Add(colCordSD);


                DataColumn col13 = new DataColumn();
                col13.ColumnName = Utility.GetResourceStr("办公电话");
                col13.DataType = typeof(string);
                dt.Columns.Add(col13);

                DataColumn col3 = new DataColumn();
                col3.ColumnName = Utility.GetResourceStr("手机");
                col3.DataType = typeof(string);
                dt.Columns.Add(col3);

                DataColumn col7 = new DataColumn();
                col7.ColumnName = Utility.GetResourceStr("电子邮箱");
                col7.DataType = typeof(string);
                dt.Columns.Add(col7);

                #endregion

                if (ents != null && ents.Any())
                {
                    foreach (var item in ents.ToList())
                    {
                        DataRow row = dt.NewRow();
                        row[0] = item.COMPANYNAME;
                        row[1] = item.DEPARTMENTNAME;
                        row[2] = item.POSTNAME;
                        row[3] = item.EMPLOYEECNAME;
                        row[4] = item.OFFICEPHONE;
                        row[5] = item.MOBILE;
                        row[6] = item.EMAIL;
                        dt.Rows.Add(row);
                    }
                    result = Utility.OutFileStream(companyName + Utility.GetResourceStr(" 员工通讯录"), dt);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployee导出员工通讯录:" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 员工档案视图分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeViewsPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");


            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                              join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                              join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                              where (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                                              //where ep.EDITSTATE == "1" && ep.CHECKSTATE == "2"
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  COMPANYNAME = c.CNAME,
                                                  DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                  POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                                  SEX = o.SEX,
                                                  EMAIL = o.EMAIL,
                                                  IDNUMBER = o.IDNUMBER,
                                                  FINGERPRINTID = o.FINGERPRINTID,
                                                  MOBILE = o.MOBILE,
                                                  OWNERID = o.EMPLOYEEID,
                                                  OWNERPOSTID = p.POSTID,
                                                  OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                  OWNERCOMPANYID = c.COMPANYID,
                                                  CREATEUSERID = o.CREATEUSERID,
                                                  EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                                  ISAGENCY = ep.ISAGENCY,
                                                  POSTLEVEL = ep.POSTLEVEL,
                                                  OFFICEPHONE = o.OFFICEPHONE

                                              };
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where d.DEPARTMENTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where p.POSTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               ISAGENCY = ep.ISAGENCY,
                               POSTLEVEL = ep.POSTLEVEL,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }

        /// <summary>
        /// 员工档案视图分页
        /// 为MVC提供
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeViewsPagingForMVC(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID, ref int recordCount)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");


            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                              join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                              join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                              where (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                                              //where ep.EDITSTATE == "1" && ep.CHECKSTATE == "2"
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  COMPANYNAME = c.BRIEFNAME,
                                                  DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                  POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                                  SEX = o.SEX,
                                                  EMAIL = o.EMAIL,
                                                  IDNUMBER = o.IDNUMBER,
                                                  FINGERPRINTID = o.FINGERPRINTID,
                                                  MOBILE = o.MOBILE,
                                                  OWNERID = o.EMPLOYEEID,
                                                  OWNERPOSTID = p.POSTID,
                                                  OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                  OWNERCOMPANYID = c.COMPANYID,
                                                  CREATEUSERID = o.CREATEUSERID,
                                                  EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                                  ISAGENCY = ep.ISAGENCY,
                                                  POSTLEVEL = ep.POSTLEVEL,
                                                  OFFICEPHONE = o.OFFICEPHONE

                                              };
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.BRIEFNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where d.DEPARTMENTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.BRIEFNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where p.POSTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.BRIEFNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               ISAGENCY = ep.ISAGENCY,
                               POSTLEVEL = ep.POSTLEVEL,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            recordCount = ents.Count();
            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }


        /// <summary>
        /// 获取员工信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeViewsPagingForMVCNoFilter(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID, ref int recordCount)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");


            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                              join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                              join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                              where (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" ) && ep.CHECKSTATE == "2"
                                              //where ep.EDITSTATE == "1" && ep.CHECKSTATE == "2"
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  COMPANYNAME = c.BRIEFNAME,
                                                  DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                  POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                                  SEX = o.SEX,
                                                  EMAIL = o.EMAIL,
                                                  IDNUMBER = o.IDNUMBER,
                                                  FINGERPRINTID = o.FINGERPRINTID,
                                                  MOBILE = o.MOBILE,
                                                  OWNERID = o.EMPLOYEEID,
                                                  OWNERPOSTID = p.POSTID,
                                                  OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                  OWNERCOMPANYID = c.COMPANYID,
                                                  CREATEUSERID = o.CREATEUSERID,
                                                  EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                                  ISAGENCY = ep.ISAGENCY,
                                                  POSTLEVEL = ep.POSTLEVEL,
                                                  OFFICEPHONE = o.OFFICEPHONE

                                              };
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" ) && ep.CHECKSTATE == "2"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.BRIEFNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where d.DEPARTMENTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" ) && ep.CHECKSTATE == "2"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.BRIEFNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where p.POSTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" ) && ep.CHECKSTATE == "2"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.BRIEFNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               ISAGENCY = ep.ISAGENCY,
                               POSTLEVEL = ep.POSTLEVEL,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            recordCount = ents.Count();
            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }

        /// <summary>
        /// 获取离职员工的信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType">机构类型（公司，部门，岗位）</param>
        /// <param name="sValue">对于机构类型的ID</param>
        /// <param name="userID">根据传入的userID进行权限过滤</param>
        /// <returns>离职员工试图</returns>
        public List<V_EMPLOYEEVIEW> GetLeaveEmployeeViewsPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");


            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                              join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                              join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                              //  join le in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on ep.EMPLOYEEPOSTID equals le.EMPLOYEEPOSTID
                                              where o.EMPLOYEESTATE == "2"
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  FINGERPRINTID = o.FINGERPRINTID,                                                  
                                                  COMPANYNAME = c.CNAME,
                                                  DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                  POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                                  SEX = o.SEX,
                                                  EMAIL = o.EMAIL,
                                                  IDNUMBER = o.IDNUMBER,
                                                  MOBILE = o.MOBILE,
                                                  OWNERID = o.EMPLOYEEID,
                                                  OWNERPOSTID = p.POSTID,
                                                  OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                  OWNERCOMPANYID = c.COMPANYID,
                                                  CREATEUSERID = o.CREATEUSERID,
                                                  POSTLEVEL = ep.POSTLEVEL,
                                                  EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                                  EMPLOYEESTATE = o.EMPLOYEESTATE,
                                                  ISAGENCY = ep.ISAGENCY,
                                                  OFFICEPHONE = o.OFFICEPHONE
                                              };
            switch (sType.ToUpper())
            {
                case "COMPANY":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           //  join le in dal.GetObjects<T_HR_LEFTOFFICE>() on ep.EMPLOYEEPOSTID equals le.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                           where c.COMPANYID == sValue && o.EMPLOYEESTATE == "2"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               EMPLOYEESTATE = o.EMPLOYEESTATE,
                               FINGERPRINTID = o.FINGERPRINTID,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "DEPARTMENT":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           // join le in dal.GetObjects<T_HR_LEFTOFFICE>() on ep.EMPLOYEEPOSTID equals le.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                           where d.DEPARTMENTID == sValue && o.EMPLOYEESTATE == "2"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               EMPLOYEESTATE = o.EMPLOYEESTATE,
                               FINGERPRINTID = o.FINGERPRINTID,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
                case "POST":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           //join le in dal.GetObjects<T_HR_LEFTOFFICE>() on ep.EMPLOYEEPOSTID equals le.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                           where p.POSTID == sValue && o.EMPLOYEESTATE == "2"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               EMPLOYEESTATE = o.EMPLOYEESTATE,
                               FINGERPRINTID = o.FINGERPRINTID,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               ISAGENCY = ep.ISAGENCY,
                               POSTLEVEL = ep.POSTLEVEL,
                               OFFICEPHONE = o.OFFICEPHONE
                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }



        public List<V_EMPLOYEEVIEW> GetLeaveEmployeeViewsPagingForMVC(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID, ref int recordCount)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");


            IQueryable<V_EMPLOYEEVIEW> ents = from l in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>()
                                              join o in dal.GetObjects<T_HR_EMPLOYEE>()  on l.EMPLOYEEID equals o.EMPLOYEEID
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                              join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                              join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                              //  join le in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on ep.EMPLOYEEPOSTID equals le.EMPLOYEEPOSTID
                                              //where o.EMPLOYEESTATE == "2"
                                              //考虑做了离职确认后再入职其它公司
                                              where o.EMPLOYEESTATE =="2" || l.CHECKSTATE =="2"
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  FINGERPRINTID = o.FINGERPRINTID,
                                                  COMPANYNAME = c.BRIEFNAME,
                                                  DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                  POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                                  SEX = o.SEX,
                                                  EMAIL = o.EMAIL,
                                                  IDNUMBER = o.IDNUMBER,
                                                  MOBILE = o.MOBILE,
                                                  OWNERID = o.EMPLOYEEID,
                                                  OWNERPOSTID = p.POSTID,
                                                  OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                  OWNERCOMPANYID = c.COMPANYID,
                                                  CREATEUSERID = o.CREATEUSERID,
                                                  POSTLEVEL = ep.POSTLEVEL,
                                                  EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                                  EMPLOYEESTATE = o.EMPLOYEESTATE,
                                                  ISAGENCY = ep.ISAGENCY,
                                                  OFFICEPHONE = o.OFFICEPHONE
                                              };
            switch (sType.ToUpper())
            {
                case "COMPANY":
                    //ents = from o in dal.GetObjects()
                    //       join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                    //       join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                    //       join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                    //       join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                    //       //  join le in dal.GetObjects<T_HR_LEFTOFFICE>() on ep.EMPLOYEEPOSTID equals le.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                    //       where c.COMPANYID == sValue && o.EMPLOYEESTATE == "2"
                    //       select new V_EMPLOYEEVIEW
                    //       {
                    //           EMPLOYEEID = o.EMPLOYEEID,
                    //           EMPLOYEECNAME = o.EMPLOYEECNAME,
                    //           EMPLOYEECODE = o.EMPLOYEECODE,
                    //           EMPLOYEEENAME = o.EMPLOYEEENAME,
                    //           EMPLOYEESTATE = o.EMPLOYEESTATE,
                    //           FINGERPRINTID = o.FINGERPRINTID,
                    //           COMPANYNAME = c.BRIEFNAME,
                    //           DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                    //           POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                    //           SEX = o.SEX,
                    //           EMAIL = o.EMAIL,
                    //           IDNUMBER = o.IDNUMBER,
                    //           MOBILE = o.MOBILE,
                    //           OWNERID = o.EMPLOYEEID,
                    //           OWNERPOSTID = p.POSTID,
                    //           OWNERDEPARTMENTID = d.DEPARTMENTID,
                    //           OWNERCOMPANYID = c.COMPANYID,
                    //           CREATEUSERID = o.CREATEUSERID,
                    //           OFFICEPHONE = o.OFFICEPHONE
                    //       };
                    ents = from  ent in ents
                               where ent.OWNERCOMPANYID == sValue
                               select ent;
                    break;
                case "DEPARTMENT":
                    //ents = from o in dal.GetObjects()
                    //       join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                    //       join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                    //       join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                    //       join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                    //       // join le in dal.GetObjects<T_HR_LEFTOFFICE>() on ep.EMPLOYEEPOSTID equals le.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                    //       where d.DEPARTMENTID == sValue && o.EMPLOYEESTATE == "2"
                    //       select new V_EMPLOYEEVIEW
                    //       {
                    //           EMPLOYEEID = o.EMPLOYEEID,
                    //           EMPLOYEECNAME = o.EMPLOYEECNAME,
                    //           EMPLOYEECODE = o.EMPLOYEECODE,
                    //           EMPLOYEEENAME = o.EMPLOYEEENAME,
                    //           EMPLOYEESTATE = o.EMPLOYEESTATE,
                    //           FINGERPRINTID = o.FINGERPRINTID,
                    //           COMPANYNAME = c.BRIEFNAME,
                    //           DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                    //           POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                    //           SEX = o.SEX,
                    //           EMAIL = o.EMAIL,
                    //           IDNUMBER = o.IDNUMBER,
                    //           MOBILE = o.MOBILE,
                    //           OWNERID = o.EMPLOYEEID,
                    //           OWNERPOSTID = p.POSTID,
                    //           OWNERDEPARTMENTID = d.DEPARTMENTID,
                    //           OWNERCOMPANYID = c.COMPANYID,
                    //           CREATEUSERID = o.CREATEUSERID,
                    //           OFFICEPHONE = o.OFFICEPHONE
                    //       };
                    ents = from ent in ents
                           where ent.OWNERDEPARTMENTID == sValue
                           select ent;
                    break;
                case "POST":
                    //ents = from o in dal.GetObjects()
                    //       join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                    //       join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                    //       join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                    //       join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                    //       //join le in dal.GetObjects<T_HR_LEFTOFFICE>() on ep.EMPLOYEEPOSTID equals le.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                    //       where p.POSTID == sValue && o.EMPLOYEESTATE == "2"
                    //       select new V_EMPLOYEEVIEW
                    //       {
                    //           EMPLOYEEID = o.EMPLOYEEID,
                    //           EMPLOYEECNAME = o.EMPLOYEECNAME,
                    //           EMPLOYEECODE = o.EMPLOYEECODE,
                    //           EMPLOYEEENAME = o.EMPLOYEEENAME,
                    //           EMPLOYEESTATE = o.EMPLOYEESTATE,
                    //           FINGERPRINTID = o.FINGERPRINTID,
                    //           COMPANYNAME = c.BRIEFNAME,
                    //           DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                    //           POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                    //           SEX = o.SEX,
                    //           EMAIL = o.EMAIL,
                    //           IDNUMBER = o.IDNUMBER,
                    //           MOBILE = o.MOBILE,
                    //           OWNERID = o.EMPLOYEEID,
                    //           OWNERPOSTID = p.POSTID,
                    //           OWNERDEPARTMENTID = d.DEPARTMENTID,
                    //           OWNERCOMPANYID = c.COMPANYID,
                    //           CREATEUSERID = o.CREATEUSERID,
                    //           EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                    //           ISAGENCY = ep.ISAGENCY,
                    //           POSTLEVEL = ep.POSTLEVEL,
                    //           OFFICEPHONE = o.OFFICEPHONE
                    //       };
                    ents = from ent in ents
                           where ent.OWNERPOSTID == sValue
                           select ent;
                    break;
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            recordCount = ents.Count();
            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }
        /// <summary>
        /// 有效的员工视图
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeViewsActivedPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");

            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects().Distinct()
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                              join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                              join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                              where ep.EDITSTATE == "1"
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  SEX = o.SEX,
                                                  EMAIL = o.EMAIL,
                                                  IDNUMBER = o.IDNUMBER,
                                                  MOBILE = o.MOBILE,
                                                  OWNERID = o.EMPLOYEEID,
                                                  OWNERPOSTID = p.POSTID,
                                                  OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                  OWNERCOMPANYID = c.COMPANYID,
                                                  CREATEUSERID = o.CREATEUSERID,

                                              };
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && ep.EDITSTATE == "1"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where d.DEPARTMENTID == sValue && ep.EDITSTATE == "1"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID
                           };
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where p.POSTID == sValue && ep.EDITSTATE == "1"
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID

                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }
        /// <summary>
        /// 用户授权时调用的员工档案分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEE> GetEmployeesWithOutPermissions(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            IQueryable<T_HR_EMPLOYEE> ents = from o in dal.GetObjects()
                                             join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                             where (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                                             select o;
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select o;
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           where d.DEPARTMENTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select o;
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           where p.POSTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select o;
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEE>(ents, pageIndex, pageSize, ref pageCount);
            return ents;

        }
        /// <summary>
        /// 用户授权时调用 员工档案视图分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeViewsWithOutPermissions(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            IQueryable<V_EMPLOYEEVIEW> ents = from o in dal.GetObjects()
                                              join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                              join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                              join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                              join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                              where (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                                              //where ep.EDITSTATE == "1" && ep.CHECKSTATE == "2"
                                              select new V_EMPLOYEEVIEW
                                              {
                                                  EMPLOYEEID = o.EMPLOYEEID,
                                                  EMPLOYEECNAME = o.EMPLOYEECNAME,
                                                  EMPLOYEECODE = o.EMPLOYEECODE,
                                                  EMPLOYEEENAME = o.EMPLOYEEENAME,
                                                  COMPANYNAME = c.CNAME,
                                                  DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                  POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                                  SEX = o.SEX,
                                                  EMAIL = o.EMAIL,
                                                  IDNUMBER = o.IDNUMBER,
                                                  FINGERPRINTID = o.FINGERPRINTID,
                                                  MOBILE = o.MOBILE,
                                                  OWNERID = o.EMPLOYEEID,
                                                  OWNERPOSTID = p.POSTID,
                                                  OWNERDEPARTMENTID = d.DEPARTMENTID,
                                                  OWNERCOMPANYID = c.COMPANYID,
                                                  CREATEUSERID = o.CREATEUSERID,
                                                  EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                                                  ISAGENCY = ep.ISAGENCY,
                                                  POSTLEVEL = ep.POSTLEVEL

                                              };
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where d.DEPARTMENTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               POSTLEVEL = ep.POSTLEVEL,
                               ISAGENCY = ep.ISAGENCY,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID
                           };
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where p.POSTID == sValue && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                           select new V_EMPLOYEEVIEW
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               EMPLOYEECODE = o.EMPLOYEECODE,
                               EMPLOYEEENAME = o.EMPLOYEEENAME,
                               COMPANYNAME = c.CNAME,
                               DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                               SEX = o.SEX,
                               EMAIL = o.EMAIL,
                               IDNUMBER = o.IDNUMBER,
                               FINGERPRINTID = o.FINGERPRINTID,
                               MOBILE = o.MOBILE,
                               OWNERID = o.EMPLOYEEID,
                               OWNERPOSTID = p.POSTID,
                               OWNERDEPARTMENTID = d.DEPARTMENTID,
                               OWNERCOMPANYID = c.COMPANYID,
                               CREATEUSERID = o.CREATEUSERID,
                               EMPLOYEEPOSTID = ep.EMPLOYEEPOSTID,
                               ISAGENCY = ep.ISAGENCY,
                               POSTLEVEL = ep.POSTLEVEL
                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_EMPLOYEEVIEW>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;

        }

        /// <summary>
        /// 根据员工id查询员工打卡机指纹编号
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <returns></returns>
        public string GetEmployeePrintIDByEmployeeID(string EmployeeID)
        {
            var ents = from o in dal.GetObjects()
                       where o.EMPLOYEEID == EmployeeID
                       select o.FINGERPRINTID;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据员工id查询员工打卡机指纹编号
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <returns></returns>
        public List<string> GetEmployeePrintIDByCompanyID(string CompanyID, string employeeid)
        {
            string filterString = string.Empty;
            List<object> queryParas = new List<object>();
            SetOrganizationFilter(ref filterString, ref queryParas, employeeid, "T_HR_ACCESSRECORD");
            List<string> empPrint = new List<string>();
            var ents = from o in dal.GetObjects()
                       join empPost in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals empPost.T_HR_EMPLOYEE.EMPLOYEEID
                       join post in dal.GetObjects<T_HR_POST>() on empPost.T_HR_POST.POSTID equals post.POSTID
                       join dep in dal.GetObjects<T_HR_DEPARTMENT>() on post.T_HR_DEPARTMENT.DEPARTMENTID equals dep.DEPARTMENTID
                       where dep.T_HR_COMPANY.COMPANYID == CompanyID
                       && o.FINGERPRINTID != null
                       orderby o.FINGERPRINTID
                       select o;
            if (ents.Count() > 0)
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                var ents1 = from o in ents
                            select o.FINGERPRINTID;
                empPrint = ents1.Distinct().ToList();

            }
            return empPrint;
        }



        /// <summary>
        /// 根据员工打卡机指纹编号查询员工
        /// </summary>
        /// <param name="strFingerPrintID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEE GetEmployeeByFingerPrintID(string strFingerPrintID)
        {
            var ents = from o in dal.GetObjects()
                       where o.FINGERPRINTID == strFingerPrintID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据员工打卡机指纹编号查询员工
        /// </summary>
        /// <param name="strFingerPrintID"></param>
        /// <returns></returns>
        public string GetEmployeeIdByFingerPrintID(string strFingerPrintID)
        {
            var ents = from o in dal.GetObjects()
                       where o.FINGERPRINTID == strFingerPrintID
                       select o.EMPLOYEEID;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据员工打卡机指纹编号查询员工
        /// </summary>
        /// <param name="strFingerPrintID"></param>
        /// <returns></returns>
        public V_EMPLOYEEDETAIL GetEmployeeOrgByFingerPrintID(string strFingerPrintID)
        {
            try
            {
                var ents = (from o in dal.GetObjects()
                            where o.FINGERPRINTID == strFingerPrintID
                            select o).FirstOrDefault();
                V_EMPLOYEEDETAIL employee = GetEmployeeDetailView(ents.EMPLOYEEID);
                return employee != null ? employee : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据员工名称获取员工信息
        /// </summary>
        /// <param name="employeeCName"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEE GetEmployeeByName(string employeeCName)
        {
            //CommonUserInfo.EmployeeID = employeeID;//传递当前用户ID
            var ents = from o in dal.GetObjects()
                       where o.EMPLOYEECNAME == employeeCName
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据员工姓名获取员工信息
        /// </summary>
        /// <param name="employeeCNames"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeByNames(List<string> employeeCNames)
        {
            try
            {
                SMT.Foundation.Log.Tracer.Debug("GetEmployeeByNames-根据员工姓名获取员工信息开始");
                var ent = from e in dal.GetObjects()
                          join c in dal.GetObjects<T_HR_COMPANY>() on e.OWNERCOMPANYID equals c.COMPANYID
                          join d in dal.GetObjects<T_HR_DEPARTMENT>() on e.OWNERDEPARTMENTID equals d.DEPARTMENTID
                          join dic in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dic.DEPARTMENTDICTIONARYID
                          join p in dal.GetObjects<T_HR_POST>() on e.OWNERPOSTID equals p.POSTID
                          join pic in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pic.POSTDICTIONARYID
                          join ec in employeeCNames on e.EMPLOYEECNAME equals ec
                          //EMPLOYEESTATE表示已离职
                          where e.EMPLOYEESTATE != "4" && e.EMPLOYEESTATE != "2"
                          select new V_EMPLOYEEVIEW
                          {
                              EMPLOYEEID = e.EMPLOYEEID,
                              EMPLOYEECNAME = e.EMPLOYEECNAME,
                              COMPANYNAME = c.CNAME,
                              DEPARTMENTNAME = dic.DEPARTMENTNAME,
                              POSTNAME = pic.POSTNAME,
                              OWNERCOMPANYID = e.OWNERCOMPANYID,
                              OWNERDEPARTMENTID = e.OWNERDEPARTMENTID,
                              OWNERPOSTID = e.OWNERPOSTID,
                              OWNERID = e.OWNERID
                          };
                return ent == null ? null : ent.ToList();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("GetEmployeeByNames-根据员工姓名获取员工信息错误" + ex.ToString());
                return null;
            }
        }

        public List<V_EMPLOYEEVIEW> GetEmployeeByNameForWP(string empName, decimal postLevel, PostLeavelCompareType type)
        {
            if (type == PostLeavelCompareType.Equal)
            {
                var ent = from e in dal.GetObjects() where empName.Contains(e.EMPLOYEECNAME) && e.EMPLOYEESTATE != "2"
                          join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                          join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                          join pic in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pic.POSTDICTIONARYID
                          join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                          join dic in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dic.DEPARTMENTDICTIONARYID
                          join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                          where e.EMPLOYEESTATE != "2" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1" 
                              && ep.POSTLEVEL== postLevel
                          select new V_EMPLOYEEVIEW
                          {
                              EMPLOYEEID = e.EMPLOYEEID,
                              EMPLOYEECNAME = e.EMPLOYEECNAME,
                              COMPANYNAME = c.CNAME,
                              DEPARTMENTNAME = dic.DEPARTMENTNAME,
                              POSTNAME = pic.POSTNAME,
                              POSTLEVEL = ep.POSTLEVEL,
                              OWNERCOMPANYID = e.OWNERCOMPANYID,
                              OWNERDEPARTMENTID = e.OWNERDEPARTMENTID,
                              OWNERPOSTID = e.OWNERPOSTID,
                              OWNERID = e.OWNERID
                          };
                return ent == null ? null : ent.ToList();
            }
            if (type == PostLeavelCompareType.LargeThan)
            {
                var ent = from e in dal.GetObjects() where empName.Contains(e.EMPLOYEECNAME) && e.EMPLOYEESTATE != "2"
                          join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                          join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                          join pic in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pic.POSTDICTIONARYID
                          join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                          join dic in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dic.DEPARTMENTDICTIONARYID
                          join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                          where e.EMPLOYEESTATE != "2" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1" && e.EMPLOYEECNAME.Contains(empName)
                              && ep.POSTLEVEL > postLevel
                          select new V_EMPLOYEEVIEW
                          {
                              EMPLOYEEID = e.EMPLOYEEID,
                              EMPLOYEECNAME = e.EMPLOYEECNAME,
                              COMPANYNAME = c.CNAME,
                              DEPARTMENTNAME = dic.DEPARTMENTNAME,
                              POSTNAME = pic.POSTNAME,
                              POSTLEVEL = ep.POSTLEVEL,
                              OWNERCOMPANYID = e.OWNERCOMPANYID,
                              OWNERDEPARTMENTID = e.OWNERDEPARTMENTID,
                              OWNERPOSTID = e.OWNERPOSTID,
                              OWNERID = e.OWNERID
                          };
                return ent == null ? null : ent.ToList();
            }
            if (type == PostLeavelCompareType.LessThan)
            {
                var ent = from e in dal.GetObjects() where empName.Contains(e.EMPLOYEECNAME) && e.EMPLOYEESTATE != "2"
                          join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                          join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                          join pic in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pic.POSTDICTIONARYID
                          join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                          join dic in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dic.DEPARTMENTDICTIONARYID
                          join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                          where ep.CHECKSTATE == "2" && ep.EDITSTATE == "1"
                              && ep.POSTLEVEL < postLevel
                          select new V_EMPLOYEEVIEW
                          {
                              EMPLOYEEID = e.EMPLOYEEID,
                              EMPLOYEECNAME = e.EMPLOYEECNAME,
                              COMPANYNAME = c.CNAME,
                              DEPARTMENTNAME = dic.DEPARTMENTNAME,
                              POSTNAME = pic.POSTNAME,
                              POSTLEVEL = ep.POSTLEVEL,
                              OWNERCOMPANYID = e.OWNERCOMPANYID,
                              OWNERDEPARTMENTID = e.OWNERDEPARTMENTID,
                              OWNERPOSTID = e.OWNERPOSTID,
                              OWNERID = e.OWNERID
                          };
                return ent == null ? null : ent.ToList(); 
            }
            
            return null;
        }

        /// <summary>
        /// 获取所有员工岗位
        /// </summary>
        public List<V_EMPLOYEEVIEW> GetAllEmployeePostForWP()
        {
            var ent = from e in dal.GetObjects()
                      join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                      join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                      join pic in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pic.POSTDICTIONARYID
                      join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                      join dic in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dic.DEPARTMENTDICTIONARYID
                      join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                      where ep.CHECKSTATE == "2" && ep.EDITSTATE == "1" && e.EMPLOYEESTATE != "2"
                      select new V_EMPLOYEEVIEW
                      {
                          EMPLOYEEID = e.EMPLOYEEID,
                          EMPLOYEECNAME = e.EMPLOYEECNAME,
                          COMPANYNAME = c.CNAME,
                          DEPARTMENTNAME = dic.DEPARTMENTNAME,
                          POSTNAME = pic.POSTNAME,
                          POSTLEVEL = ep.POSTLEVEL,
                          OWNERCOMPANYID = c.COMPANYID,
                          OWNERDEPARTMENTID = d.OWNERDEPARTMENTID,
                          OWNERPOSTID = p.OWNERPOSTID,
                          OWNERID = e.OWNERID,
                          ISAGENCY = ep.ISAGENCY
                      };
            return ent == null ? null : ent.ToList();

        }

        /// <summary>
        /// 根据员工ID集合获取员工信息
        /// </summary>
        /// <param name="employeeIDs"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeInfosByEmployeeIDs(List<string> employeeIDs)
        {
            var ent = from e in dal.GetObjects()
                      join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                      join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                      join pic in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pic.POSTDICTIONARYID
                      join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                      join dic in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dic.DEPARTMENTDICTIONARYID
                      join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                      where ep.CHECKSTATE == "2" && ep.EDITSTATE == "1" && e.EMPLOYEESTATE != "2"
                      && employeeIDs.Contains(e.EMPLOYEEID)
                      select new V_EMPLOYEEVIEW
                      {
                          EMPLOYEEID = e.EMPLOYEEID,
                          EMPLOYEECNAME = e.EMPLOYEECNAME,
                          COMPANYNAME = c.CNAME,
                          DEPARTMENTNAME = dic.DEPARTMENTNAME,
                          POSTNAME = pic.POSTNAME,
                          POSTLEVEL = ep.POSTLEVEL,
                          OWNERCOMPANYID = c.COMPANYID,
                          OWNERDEPARTMENTID = d.OWNERDEPARTMENTID,
                          OWNERPOSTID = p.OWNERPOSTID,
                          OWNERID = e.OWNERID,
                          ISAGENCY = ep.ISAGENCY
                      };
            return ent == null ? null : ent.ToList();

        }

        /// <summary>
        /// 根据员工ID查询员工
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>员工信息</returns>
        public T_HR_EMPLOYEE GetEmployeeByID(string employeeID)
        {
            CommonUserInfo.EmployeeID = employeeID;//传递当前用户ID

            var ents = from o in dal.GetObjects()
                       where o.EMPLOYEEID == employeeID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据员工ID集合查询所有员工
        /// </summary>
        /// <param name="employeeIDs">员工ID</param>
        /// <returns>员工信息</returns>
        public List<T_HR_EMPLOYEE> GetEmployeeByIDs(string[] employeeIDs)
        {
            if (employeeIDs.Count() > 0)
            {
                string filterString = "";

                IList<object> queryParas = new List<object>();

                foreach (string id in employeeIDs)
                {
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " OR ";

                    filterString += " EMPLOYEEID==@" + queryParas.Count().ToString() + " ";
                    queryParas.Add(id);
                }


                var ents = dal.GetObjects().Where(filterString, queryParas.ToArray());

                return ents.Count() > 0 ? ents.ToList() : null;
            }
            else
            {
                return null;
            }
        }
        public List<EmployeeContactWays> GetEmployeeToEngine(string[] employeeIDs)
        {
            if (employeeIDs.Count() > 0)
            {
                string filterString = "";

                IList<object> queryParas = new List<object>();

                foreach (string id in employeeIDs)
                {
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " OR ";

                    filterString += " EMPLOYEEID==@" + queryParas.Count().ToString() + " ";
                    queryParas.Add(id);
                }
                var ents = from c in dal.GetObjects()
                           select new EmployeeContactWays
                           {
                               EMPLOYEEID = c.EMPLOYEEID,
                               EMPLOYEENAME = c.EMPLOYEECNAME,
                               TELPHONE = c.MOBILE,
                               RTX = c.OTHERCOMMUNICATE,
                               MAILADDRESS = c.EMAIL,
                               COMPANYID = c.OWNERCOMPANYID,
                               DEPARTMENTID = c.OWNERDEPARTMENTID,
                               POSTID = c.OWNERPOSTID,
                               EMPLOYEESTATE = c.EMPLOYEESTATE
                           };

                ents = ents.Where(filterString, queryParas.ToArray());

                return ents.Count() > 0 ? ents.ToList() : null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据身份证号码查询员工实体
        /// </summary>
        /// <param name="strIDCard">身份证号码</param>
        /// <returns>返回员工信息，如果没有则返回NULL</returns>
        private T_HR_EMPLOYEE GetEmployeeByIDCard(string strIDCard)
        {
            ///TOADD:根据离职时间判断是否可以入职(从参数设置里面获取入职时间)
            return dal.GetObjects().FirstOrDefault(s => s.IDNUMBER == strIDCard && s.EMPLOYEESTATE == "2");
        }
        /// <summary>
        /// 获取指定岗位下的所有员工
        /// </summary>
        /// <param name="level">员工级别</param>
        /// <returns>所有员工</returns>
        public List<T_HR_EMPLOYEE> GetEmployeesByPostLevel(string level)
        {
            int intLevel = Convert.ToInt16(level);
            var ents = from o in dal.GetObjects()
                       join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                       join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                       join pd in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pd.POSTDICTIONARYID
                       where pd.POSTLEVEL == intLevel
                       select o;
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        /// <summary>
        /// 获取指定岗位区间所有员工 及岗位级别
        /// </summary>
        /// <param name="level">员工级别</param>
        /// <returns>所有员工</returns>
        public List<V_EMPOYEEPOSTLEVEL> GetEmployeesByPostLevel(IList<int> levelA, IList<int> levelB)
        {
            //将参数按从小到大排列

            List<V_EMPOYEEPOSTLEVEL> tmp = new List<V_EMPOYEEPOSTLEVEL>();
            List<V_EMPOYEEPOSTLEVEL> ents = new List<V_EMPOYEEPOSTLEVEL>();
            int la = 0;
            int lb = 0;
            for (int i = 0; i < levelA.Count(); i++)
            {
                la = levelA[i];
                lb = levelB[i];
                ents = (from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                        join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                        join pd in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pd.POSTDICTIONARYID
                        where (pd.POSTLEVEL >= la && pd.POSTLEVEL <= lb) && ep.ISAGENCY == "0"
                        select new V_EMPOYEEPOSTLEVEL
                        {
                            POSTLEVEL = pd.POSTLEVEL,
                            T_HR_EMPLOYEE = ep.T_HR_EMPLOYEE
                        }
                        ).ToList();

                tmp.AddRange(ents);

            }

            return tmp.Count() > 0 ? tmp.ToList() : null;

        }

        /// <summary>
        /// 根据员工id获取员工信息
        /// </summary>
        /// <param name="employeeID">员工id</param>
        /// <returns>员工信息</returns>
        public V_EMPLOYEEVIEW GetEmployeeInfoByEmployeeID(string employeeID)
        {
            var ents = (from em in dal.GetObjects()
                        where em.EMPLOYEEID == employeeID
                        select new V_EMPLOYEEVIEW
                        {
                            EMPLOYEECNAME = em.EMPLOYEECNAME,
                            EMPLOYEESTATE = em.EMPLOYEESTATE
                        }).FirstOrDefault();

            var ent = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                      join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                      join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                      join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                      where em.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && em.EDITSTATE == "1" && em.ISAGENCY == "0"//有效的主岗位
                      select new V_EMPLOYEEVIEW
                      {
                          EMPLOYEECNAME = ents.EMPLOYEECNAME,
                          EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                          OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                          OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                          OWNERPOSTID = em.T_HR_POST.POSTID,
                          OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                          COMPANYNAME = c.CNAME,
                          DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                          POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                          POSTLEVEL = em.POSTLEVEL,
                          EMPLOYEESTATE = ents.EMPLOYEESTATE
                      };
            if (ent != null && ent.Count() > 0)
            {
                return ent.FirstOrDefault();
            }
            else
            {
                return ents;
            }

        }


        /// <summary>
        /// 根据员工id获取员工信息
        /// </summary>
        /// <param name="employeeID">员工id</param>
        /// <returns>员工信息</returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeByIDsToSendDoc(string[] employeeIDs)
        {
            List<V_EMPLOYEEVIEW> listEmployees = new List<V_EMPLOYEEVIEW>();
            try
            {
                var ent = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").Include("T_HR_EMPLOYEE")
                          join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                          join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                          join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                          join a in dal.GetObjects<T_HR_EMPLOYEE>() on em.T_HR_EMPLOYEE.EMPLOYEEID equals a.EMPLOYEEID
                          //where em.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && em.EDITSTATE == "1" && em.ISAGENCY == "0"//有效的主岗位
                          where em.EDITSTATE == "1" && em.ISAGENCY == "0"//有效的主岗位
                          && employeeIDs.Contains(a.EMPLOYEEID)
                          select new V_EMPLOYEEVIEW
                          {
                              EMPLOYEECNAME = a.EMPLOYEECNAME,
                              EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                              OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                              OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                              OWNERPOSTID = em.T_HR_POST.POSTID,
                              OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                              COMPANYNAME = c.CNAME,
                              DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                              POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                              POSTLEVEL = em.POSTLEVEL,
                              EMPLOYEESTATE = a.EMPLOYEESTATE

                          };
                if (ent != null && ent.Count() > 0)
                {
                    return listEmployees = ent.ToList();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeeBLL-GetEmployeeByIDsToSendDoc出现错误：" + ex.ToString());
            }
            return listEmployees;
        }

        /// <summary>
        /// 根据员工ID查询员工详细信息
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>员工详细信息</returns>
        public V_EMPLOYEEPOST GetEmployeeDetailByID(string employeeID)
        {
            V_EMPLOYEEPOST tempPost = new V_EMPLOYEEPOST();

            CommonUserInfo.EmployeeID = employeeID;//传递当前用户ID
            LoginUser currentUser = CommonUserInfo.CurrentConfig.CurrentUser;
            //if (currentUser.EmployeeInfo == null)
            //{
            var ents = dal.GetObjects().FirstOrDefault(s => s.EMPLOYEEID == employeeID);

            if (ents != null)
            {
                tempPost.T_HR_EMPLOYEE = ents;
                var eposts = dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY").Where(s => s.T_HR_EMPLOYEE.EMPLOYEEID == ents.EMPLOYEEID && s.EDITSTATE == "1").OrderBy(s => s.ISAGENCY);
                // var eposts = DataContext.T_HR_EMPLOYEEPOST.Include("T_HR_EMPLOYEE").Include("T_HR_POST").Where(s => s.T_HR_EMPLOYEE.EMPLOYEEID == ents.EMPLOYEEID);
                //dal.GetObjects<T_HR_POST>().MergeOption = MergeOption.NoTracking;
                //DataContext.T_HR_POSTDICTIONARY.MergeOption = MergeOption.NoTracking;
                //DataContext.T_HR_DEPARTMENT.MergeOption = MergeOption.NoTracking;
                //DataContext.T_HR_DEPARTMENTDICTIONARY.MergeOption = MergeOption.NoTracking;
                //DataContext.T_HR_COMPANY.MergeOption = MergeOption.NoTracking;
                //dal.GetObjects().MergeOption = MergeOption.NoTracking;
                tempPost.EMPLOYEEPOSTS = eposts.Count() > 0 ? eposts.ToList() : null;

                //if (tempPost.EMPLOYEEPOSTS != null)
                //{
                //    foreach (var ep in tempPost.EMPLOYEEPOSTS)
                //    {
                //        //加载岗位字典
                //        ep.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();
                //        //加载部门
                //        ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                //        //加载部门字典
                //        ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
                //        //加载公司
                //        ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                //    }
                //}
            }
            //缓存当前用户
            //currentUser.EmployeeInfo = tempPost;
            //}
            //else
            //{
            //    tempPost = currentUser.EmployeeInfo;
            //}





            return tempPost;
        }

        /// <summary>
        /// 获取员工岗位信息 员工岗位审核通过
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public V_EMPLOYEEPOST GetVEmployeePostByEmployeeID(string employeeId)
        {
            var ents = dal.GetObjects().FirstOrDefault(s => s.EMPLOYEEID == employeeId);
            V_EMPLOYEEPOST employeePost = new V_EMPLOYEEPOST();
            if (ents != null)
            {
                employeePost.T_HR_EMPLOYEE = ents;
                var eposts = dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Where(s => s.T_HR_EMPLOYEE.EMPLOYEEID == ents.EMPLOYEEID && s.CHECKSTATE=="2").OrderBy(s => s.CREATEDATE);

                employeePost.EMPLOYEEPOSTS = eposts.Count() > 0 ? eposts.ToList() : null;

                if (employeePost.EMPLOYEEPOSTS != null)
                {
                    foreach (var ep in employeePost.EMPLOYEEPOSTS)
                    {
                        //加载岗位字典
                        ep.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();
                        //加载部门
                        ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                        //加载部门字典
                        ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
                        //加载公司
                        ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                    }
                }
            }
            return employeePost;
        }

        /// <summary>
        /// 根据员工ID查询员工详细信息
        /// </summary>
        /// <param name="employeeID">员工ID集合</param>
        /// <returns>员工详细信息</returns>
        public List<V_EMPLOYEEPOST> GetEmployeeDetailByIDs(string[] employeeID)
        {
            List<V_EMPLOYEEPOST> postList = new List<V_EMPLOYEEPOST>();
            foreach (var id in employeeID)
            {
                var ents = dal.GetObjects().FirstOrDefault(s => s.EMPLOYEEID == id);
                V_EMPLOYEEPOST tempPost = new V_EMPLOYEEPOST();
                if (ents != null)
                {
                    tempPost.T_HR_EMPLOYEE = ents;
                    var eposts = dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Where(s => s.T_HR_EMPLOYEE.EMPLOYEEID == ents.EMPLOYEEID && s.EDITSTATE == "1").OrderBy(s => s.ISAGENCY);

                    tempPost.EMPLOYEEPOSTS = eposts.Count() > 0 ? eposts.ToList() : null;

                    if (tempPost.EMPLOYEEPOSTS != null)
                    {
                        foreach (var ep in tempPost.EMPLOYEEPOSTS)
                        {
                            //加载岗位字典
                            ep.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();
                            //加载部门
                            ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                            //加载部门字典
                            ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
                            //加载公司
                            ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                        }
                    }
                }
                postList.Add(tempPost);
            }
            return postList;
        }
        /// <summary>
        /// 根据参数获取员工ID集合
        /// </summary>
        /// <param name="companyIDs"></param>
        /// <param name="departmentIDs"></param>
        /// <param name="postIDs"></param>
        /// <param name="employeeIDs"></param>
        /// <returns></returns>
        public List<string> GetEmployeeIDsByParas(IList<string> companyIDs, IList<string> departmentIDs, IList<string> postIDs)
        {
            //TODO: 分页
            List<string> temp = new List<string>();
            List<string> employeetmp = new List<string>();
            if (companyIDs.Count > 0)
            {
                foreach (string cid in companyIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on a.T_HR_POST.POSTID equals b.POSTID
                                   where b.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == cid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (departmentIDs.Count > 0)
            {
                foreach (string did in departmentIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on a.T_HR_POST.POSTID equals b.POSTID
                                   where b.T_HR_DEPARTMENT.DEPARTMENTID == did && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (postIDs.Count > 0)
            {
                foreach (string pid in postIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                   where a.T_HR_POST.POSTID == pid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }


            return temp;
        }


        /// <summary>
        /// 根据参数获取员工ID集合
        /// </summary>
        /// <param name="companyIDs"></param>
        /// <param name="departmentIDs"></param>
        /// <param name="postIDs"></param>
        /// <param name="employeeIDs"></param>
        /// <returns></returns>
        public List<string> GetEmployeeIDsByParasByBalancePost(IList<string> companyIDs, IList<string> departmentIDs, IList<string> postIDs)
        {
            //TODO: 分页
            List<string> temp = new List<string>();
            List<string> employeetmp = new List<string>();
            if (companyIDs.Count > 0)
            {
                foreach (string cid in companyIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on a.T_HR_POST.POSTID equals b.POSTID
                                   where b.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == cid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   && a.EDITSTATE == "1"
                                   && a.ISAGENCY == "0"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (departmentIDs.Count > 0)
            {
                foreach (string did in departmentIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on a.T_HR_POST.POSTID equals b.POSTID
                                   where b.T_HR_DEPARTMENT.DEPARTMENTID == did && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   && a.EDITSTATE == "1"
                                   && a.ISAGENCY == "0"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (postIDs.Count > 0)
            {
                foreach (string pid in postIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                   where a.T_HR_POST.POSTID == pid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   && a.EDITSTATE == "1"
                                   && a.ISAGENCY == "0"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }


            return temp;
        }
        /// <summary>
        /// 查找公司id，
        /// </summary>
        /// <param name="companyIDs"></param>
        /// <param name="departmentIDs"></param>
        /// <param name="postIDs"></param>
        /// <returns></returns>
        public List<string> GetEmployeeIDsWithParas(List<string> companyIDs, bool isContainChildCompany, List<string> departmentIDs, bool isContainChildDepartment, List<string> postIDs)
        {
            //TODO: 分页
            string logger = string.Empty;
            List<string> temp = new List<string>();
            List<string> employeetmp = new List<string>();
            if (companyIDs.Count > 0)
            {
                if (isContainChildCompany)
                {
                    List<string> childCompanyIDs = new CompanyBLL().GetChildCompanyByCompanyID(companyIDs.ToList());
                    foreach (var id in childCompanyIDs)
                    {
                        if (!companyIDs.Contains(id))
                        {
                            companyIDs.Add(id);
                        }
                    }
                }
                logger = "===roger logger:companyIDS===";
                foreach (string cid in companyIDs)
                {
                    logger += cid + "，";
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on a.T_HR_POST.POSTID equals b.POSTID
                                   where b.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == cid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   && a.EDITSTATE == "1" && a.CHECKSTATE == "2"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (departmentIDs.Count > 0)
            {
                if (isContainChildDepartment)
                {
                    List<string> childdepartmentIDs = new DepartmentBLL().GetChildDepartmentBydepartmentID(departmentIDs.ToList());
                    foreach (var id in childdepartmentIDs)
                    {
                        if (!departmentIDs.Contains(id))
                        {
                            departmentIDs.Add(id);
                        }
                    }
                }
                logger += "===roger logger:departmentsIDs===";
                foreach (string did in departmentIDs)
                {
                    logger += did + "，";
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on a.T_HR_POST.POSTID equals b.POSTID
                                   where b.T_HR_DEPARTMENT.DEPARTMENTID == did && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   && a.EDITSTATE == "1" && a.CHECKSTATE == "2"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (postIDs.Count > 0)
            {
                logger += "===roger logger:postIDs===";
                foreach (string pid in postIDs)
                {
                    logger += pid + "，";
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                   where a.T_HR_POST.POSTID == pid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   && a.EDITSTATE == "1" && a.CHECKSTATE == "2"
                                   select a.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            SMT.Foundation.Log.Tracer.Debug(logger);

            return temp;
        }
        /// <summary>
        /// 根据参数获取员工信息
        /// </summary>
        /// <param name="companyIDs"></param>
        /// <param name="departmentIDs"></param>
        /// <param name="postIDs"></param>
        /// <param name="employeeIDs"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEPOST> GetEmployeeDetailByParas(IList<string> companyIDs, IList<string> departmentIDs, IList<string> postIDs, IList<string> employeeIDs)
        {
            //TODO: 分页
            List<T_HR_EMPLOYEE> temp = new List<T_HR_EMPLOYEE>();
            List<T_HR_EMPLOYEE> employeetmp = new List<T_HR_EMPLOYEE>();
            if (companyIDs.Count > 0)
            {
                foreach (string cid in companyIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on a.T_HR_POST.POSTID equals b.POSTID
                                   where b.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == cid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   select a.T_HR_EMPLOYEE).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (departmentIDs.Count > 0)
            {
                foreach (string did in departmentIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join b in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on a.T_HR_POST.POSTID equals b.POSTID
                                   where b.T_HR_DEPARTMENT.DEPARTMENTID == did && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   select a.T_HR_EMPLOYEE).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (postIDs.Count > 0)
            {
                foreach (string pid in postIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                   where a.T_HR_POST.POSTID == pid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   select a.T_HR_EMPLOYEE).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            if (employeeIDs.Count > 0)
            {
                foreach (string eid in employeeIDs)
                {
                    employeetmp = (from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                   where a.T_HR_EMPLOYEE.EMPLOYEEID == eid && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1"
                                   select a.T_HR_EMPLOYEE).ToList();
                    temp.AddRange(employeetmp);
                }
            }
            //    IQueryable<T_HR_EMPLOYEE> ents = from a in dal.GetObjects()
            //                                    select a;
            // ents = ents.OrderBy(string.IsNullOrEmpty(sort) ? "EMPLOYEEID" : sort);

            //  ents = Utility.Pager<T_HR_EMPLOYEE>(ents, pageIndex, pageSize, ref pageCount);

            //  T_HR_EMPLOYEE[] temp = ents.Count() > 0 ? ents.ToArray() : null;
            List<V_EMPLOYEEPOST> EmployeePost = new List<V_EMPLOYEEPOST>();

            if (temp != null)
            {
                foreach (var ent in temp)
                {
                    V_EMPLOYEEPOST tempPost = new V_EMPLOYEEPOST();
                    tempPost.T_HR_EMPLOYEE = ent;
                    tempPost = GetEmployeeDetailByID(ent.EMPLOYEEID);
                    EmployeePost.Add(tempPost);
                }
            }


            return EmployeePost;
        }

        public List<T_HR_EMPLOYEEPOST> GetEmployeeDetailByParasPaging(int pageIndex, int pageSize, string sort, ref int pageCount, IList<string> companyIDs, IList<string> departmentIDs, IList<string> postIDs, IList<string> employeeIDs)
        {
            //TODO: 分页
            //IQueryable<T_HR_EMPLOYEE> temp = new List<T_HR_EMPLOYEE>();
            IQueryable<T_HR_EMPLOYEEPOST> employeetmp = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                        where 1 != 1
                                                        select a;
            if (companyIDs.Count > 0)
            {
                StringBuilder companyID = new StringBuilder();
                foreach (string cid in companyIDs)
                {
                    companyID.Append(cid + ",");
                }
                string cids = companyID.ToString();
                employeetmp = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                              where a.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID.Contains(cids) && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1" && a.EDITSTATE == "1"
                              select a;
            }
            if (departmentIDs.Count > 0)
            {
                StringBuilder departmentID = new StringBuilder();
                foreach (string did in departmentIDs)
                {
                    departmentID.Append(did + ",");
                }
                string dids = departmentID.ToString();
                IQueryable<T_HR_EMPLOYEEPOST> employeeDepartment = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                                                                   where a.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID.Contains(dids) && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1" && a.EDITSTATE == "1"
                                                                   select a;
                if (employeeDepartment.Count() > 0)
                {
                    employeetmp = employeetmp.Union(employeeDepartment);
                }
            }
            if (postIDs.Count > 0)
            {
                StringBuilder postID = new StringBuilder();
                foreach (string pid in postIDs)
                {
                    postID.Append(pid + ",");

                }
                string pids = postID.ToString();
                IQueryable<T_HR_EMPLOYEEPOST> employePostTmp = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                                                               where a.T_HR_POST.POSTID.Contains(pids) && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1" && a.EDITSTATE == "1"
                                                               select a;
                if (employePostTmp.Count() > 0)
                {
                    employeetmp = employeetmp.Union(employePostTmp);
                }
            }
            if (employeeIDs.Count > 0)
            {
                StringBuilder employeeID = new StringBuilder();
                foreach (string eid in employeeIDs)
                {
                    employeeID.Append(eid + ",");

                }
                string eids = employeeID.ToString();
                IQueryable<T_HR_EMPLOYEEPOST> employeesTmp = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                                                             where a.T_HR_EMPLOYEE.EMPLOYEEID.Contains(eids) && a.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && a.T_HR_EMPLOYEE.EDITSTATE == "1" && a.EDITSTATE == "1"
                                                             select a;
                if (employeesTmp.Count() > 0)
                {
                    employeetmp = employeetmp.Union(employeesTmp);
                }
            }
            //    IQueryable<T_HR_EMPLOYEE> ents = from a in dal.GetObjects()
            //                                    select a;
            employeetmp = employeetmp.OrderBy(string.IsNullOrEmpty(sort) ? "T_HR_EMPLOYEE.EMPLOYEECNAME" : sort);

            employeetmp = Utility.Pager<T_HR_EMPLOYEEPOST>(employeetmp, pageIndex, pageSize, ref pageCount);
            return employeetmp.Count() > 0 ? employeetmp.ToList() : null;
        }
        /// <summary>
        /// 添加员工基本资料
        /// </summary>
        /// <param name="entity">员工实体</param>
        public void EmployeeAdd(T_HR_EMPLOYEE entity, string companyID, ref string strMsg)
        {
            try
            {
                //根据员工编号和编
                //var tempEnt = dal.GetObjects().FirstOrDefault(s => s.EMPLOYEECODE == entity.EMPLOYEECODE
                //|| s.IDNUMBER == entity.IDNUMBER);
                //生成员工编码
                bool flag = true;
                Random r = new Random();
                string employeeCode = entity.EMPLOYEECODE;
                int i = 0;
                List<int> j = new List<int>();
                j.Add(0);
                while (flag)
                {
                    i = r.Next(1, 1000);
                    if (!j.Contains(i))
                    {
                        j.Add(i);
                        employeeCode += (i / 100).ToString() + ((i / 10) % 10).ToString() + (i % 10).ToString();
                        var codes = from n in dal.GetObjects()
                                    join m in dal.GetObjects<T_HR_EMPLOYEEPOST>() on n.EMPLOYEEID equals m.T_HR_EMPLOYEE.EMPLOYEEID
                                    where (n.EMPLOYEECODE == employeeCode) && m.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == companyID
                                    select n;
                        if (codes.Count() == 0)
                        {
                            flag = false;
                            entity.EMPLOYEECODE = employeeCode;
                        }
                    }
                }
                var tempEnt = from c in dal.GetObjects()
                              join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                              where (c.EMPLOYEECODE == entity.EMPLOYEECODE || c.IDNUMBER == entity.IDNUMBER) && b.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == companyID
                              select c;
                if (tempEnt.Count() > 0)
                {
                    // throw new Exception("EMPLOYEEREPETITION");
                    strMsg = "EMPLOYEEREPETITION";
                    return;
                }
                entity.EDITSTATE = Convert.ToInt32(SMT.HRM.BLL.Common.EditStates.UnActived).ToString();
                ////根据身体证查询黑名单
                //BlackListBLL blackBll = new BlackListBLL();
                //if (blackBll.GetBlackListByIDCard(entity.IDNUMBER) != null)
                //{
                //    throw new Exception("BLACKLISTEXIST");
                //}
                //if (GetEmployeeByIDCard(entity.IDNUMBER) != null)
                //{
                //    throw new Exception("EMPLOYEELEAVE");
                //}
                T_HR_EMPLOYEE temp = new T_HR_EMPLOYEE();
                Utility.CloneEntity<T_HR_EMPLOYEE>(entity, temp);
                if (entity.T_HR_RESUME != null)
                {
                    temp.T_HR_RESUMEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", entity.T_HR_RESUME.RESUMEID);

                }
                dal.Add(temp);
                //清MVC缓存
                MvcCacheClear(entity, "Add");
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeAdd:" + ex.Message);
                throw (ex);
            }
        }

        /// <summary>
        /// 根据身份证号码判断
        /// </summary>
        /// <param name="sNumberID"></param>
        /// <returns></returns>
        public bool EmployeeIsEntry(string sNumberID, ref string blackMessage, ref string[] leaveMessage)
        {
            try
            {
                BlackListBLL blackBll = new BlackListBLL();
                T_HR_BLACKLIST black = blackBll.GetBlackListByIDCard(sNumberID);
                if (black != null)
                {
                    //  throw new Exception("BLACKLISTEXIST");
                    // strMsg = "BLACKLISTEXIST";
                    blackMessage = black.EDITSTATE;
                }
                ///TOEDIT:根据员工离职状态修改，是否离职超过X月
                //if (GetEmployeeByIDCard(sNumberID) != null)
                //{
                //    //  throw new Exception("EMPLOYEELEAVE");
                //    strMsg = "EMPLOYEELEAVE";
                //    return false;
                //}

                var employees = from c in dal.GetObjects()
                                where c.IDNUMBER == sNumberID
                                select c;
                if (employees.Count() > 0)
                {
                    leaveMessage = new string[2];
                    T_HR_EMPLOYEE employee = employees.FirstOrDefault();


                    leaveMessage[0] = employee.EMPLOYEEID;
                    //没有入职单表示 第一次录入员工档案 按新人入职处理
                    var entry = from c in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                                where c.T_HR_EMPLOYEE.EMPLOYEEID == employee.EMPLOYEEID
                                select c;
                    if (!entry.Any())
                    {
                        return true;
                    }
                    else
                    {
                        if (entry.FirstOrDefault().CHECKSTATE == "1" || entry.FirstOrDefault().CHECKSTATE == "0")
                        {
                            leaveMessage[1] = "LimitEntry";//离职时间小于指定的时间
                        }
                        if (entry.FirstOrDefault().CHECKSTATE == "3")
                        {
                            leaveMessage[1] = "UnApproved";
                        }
                    }
                    //在职不能再入职
                    if ((employee.EMPLOYEESTATE == "0" && employee.EDITSTATE != "0") || (employee.EMPLOYEESTATE == "3" && employee.EDITSTATE != "0") || employee.EMPLOYEESTATE == "1")
                    {
                        SMT.Foundation.Log.Tracer.Debug("员工已经在职不能再入职:" + employee.EMPLOYEEID + "---" + employee.EMPLOYEECNAME + "--员工状态：" + employee.EMPLOYEESTATE + "--员工Edit状态：" + employee.EDITSTATE);
                        return false;
                    }

                    var ents = from c in dal.GetObjects<T_HR_LEFTOFFICE>()
                               join b in dal.GetObjects() on c.T_HR_EMPLOYEE.EMPLOYEEID equals b.EMPLOYEEID
                               where b.IDNUMBER == sNumberID && c.ISAGENCY == "0" && c.CHECKSTATE == "2"
                               select c;

                    if (string.IsNullOrEmpty(leaveMessage[1]) && ents.Count() > 0)
                    {
                        //leaveMessage[0] = employee.EMPLOYEEID;

                        DateTime? leaveDate = ents.OrderByDescending(s => s.LEFTOFFICEDATE).FirstOrDefault().LEFTOFFICEDATE;
                        if ((System.DateTime.Now - leaveDate.Value).Days / 30 < 6)
                        {
                            leaveMessage[1] = "LessThan";//离职时间小于指定的时间
                        }

                    }
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("员工入职时身份证号不重复。");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工入职第一步产生错误:" + ex.ToString());
                return false;
            }

            return true;
        }

        public void EmployeeUpdate(T_HR_EMPLOYEE sourceEntity, string companyID, ref string strMsg)
        {
            try
            {
                //var temp = dal.GetObjects().FirstOrDefault(s => (s.EMPLOYEECODE == sourceEntity.EMPLOYEECODE
                //   || s.IDNUMBER == sourceEntity.IDNUMBER) && s.EMPLOYEEID != sourceEntity.EMPLOYEEID);
                var tempEnt = from c in dal.GetObjects()
                              join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                              where (c.EMPLOYEECODE == sourceEntity.EMPLOYEECODE || c.IDNUMBER == sourceEntity.IDNUMBER) && b.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == companyID && c.EMPLOYEEID != sourceEntity.EMPLOYEEID
                              select c;
                if (tempEnt.Count() > 0)
                {
                    // throw new Exception("EMPLOYEEREPETITION");
                    strMsg = "EMPLOYEEREPETITION";
                    return;
                }

                var ents = from ent in dal.GetObjects()
                           where ent.EMPLOYEEID == sourceEntity.EMPLOYEEID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity(sourceEntity, ent);
                    if (sourceEntity.T_HR_RESUME != null)
                    {
                        ent.T_HR_RESUMEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", sourceEntity.T_HR_RESUME.RESUMEID);

                    }
                    int i = dal.Update(ent);
                    #region 重新生成年假
                    EmployeeLevelDayCountBLL leaveDayBll = new EmployeeLevelDayCountBLL();
                    leaveDayBll.CalculateEmployeeLevelDayCountByOrgID("4", ent.EMPLOYEEID);
                    #endregion
                    //清MVC缓存
                    MvcCacheClear(ent, "Modify");
                    if (i > 0)
                    {

                        #region 调用即时通讯接口
                        //调用即时通讯的接口  

                        //string StrMessages = "员工姓名：" + ent.EMPLOYEECNAME + "员工ID：" + ent.EMPLOYEEID;
                        //SMT.Foundation.Log.Tracer.Debug("员工信息修改开始调用即时通讯接口" + StrMessages);
                        //EmployeeEntryBLL entry = new EmployeeEntryBLL();
                        //entry.AddImInstantMessage(ent,null);
                        //AddImInstantMessage(ent, entpost.FirstOrDefault());
                        this.UpdateInstantMessage(ent);

                        #endregion
                    }

                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeUpdatewithCompanyId:" + ex.Message);
                throw (ex);
            }
        }


        public void EmployeeUpdate(T_HR_EMPLOYEE sourceEntity, ref string strMsg)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => (s.EMPLOYEECODE == sourceEntity.EMPLOYEECODE
                   || s.IDNUMBER == sourceEntity.IDNUMBER) && s.EMPLOYEEID != sourceEntity.EMPLOYEEID);
                if (temp != null)
                {
                    // throw new Exception("EMPLOYEEREPETITION");
                    strMsg = "EMPLOYEEREPETITION";
                    return;
                }

                var ents = from ent in dal.GetObjects()
                           where ent.EMPLOYEEID == sourceEntity.EMPLOYEEID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity(sourceEntity, ent);
                    if (sourceEntity.T_HR_RESUME != null)
                    {
                        ent.T_HR_RESUMEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", sourceEntity.T_HR_RESUME.RESUMEID);

                    }
                    dal.Update(ent);
                    //清MVC缓存
                    MvcCacheClear(ent, "Modify");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeUpdate:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 新增或添加员工档案
        /// </summary>
        /// <param name="sourceEntity"></param>
        /// <param name="strMsg"></param>
        public void EmployeeAddOrUpdate(T_HR_EMPLOYEE sourceEntity, ref string strMsg)
        {
            try
            {
                dal.BeginTransaction();
                var ents = from ent in dal.GetObjects()
                           where ent.EMPLOYEEID == sourceEntity.EMPLOYEEID
                           select ent;
                if (sourceEntity.T_HR_RESUME != null)
                {
                    sourceEntity.T_HR_RESUMEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", sourceEntity.T_HR_RESUME.RESUMEID);

                }
                sourceEntity.T_HR_RESUME = null;
                if (string.IsNullOrEmpty(sourceEntity.EMPLOYEECODE))
                {
                    sourceEntity.EMPLOYEECODE = " ";
                }
                if (ents.Count() > 0)
                {

                    dal.UpdateFromContext(sourceEntity);
                    //清MVC缓存
                    MvcCacheClear(sourceEntity, "Modify");
                }
                else
                {
                    dal.AddToContext(sourceEntity);
                    //清MVC缓存
                    MvcCacheClear(sourceEntity, "Add");
                }
                dal.SaveContextChanges();
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                strMsg = ex.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeAddOrUpdate:" + ex.Message);
                if (ex.InnerException != null)
                {
                    SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeAddOrUpdate:" + ex.InnerException);
                }
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="employeeIDs"></param>
        /// <returns></returns>
        public int EmployeeDelete(string[] employeeIDs)
        {
            //var ids = employeeIDs.ToList();
            //var ents1 = from e in dal.GetObjects()                        
            //           select e;

            //var ttt = ents1.ToList().Where(e => ids.Contains(e.EMPLOYEEID));

            foreach (string id in employeeIDs)
            {
                var ents = from e in dal.GetObjects()
                           where e.EMPLOYEEID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    //清MVC缓存
                    MvcCacheClear(ent, "Delete");
                }
            }

            return dal.SaveContextChanges();
        }

        /// <summary>
        /// 根据公司ID获取该公司所属的所有非离职员工
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEE> GetEmployeeByCompanyID(string strCompanyID)
        {
            string strIsAgnecy = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
            string strEditState = Convert.ToInt32(Common.EditStates.Actived).ToString();
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            //查正式和试用员工
            IQueryable<T_HR_EMPLOYEE> qy = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY") on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                           join en in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE") on e.T_HR_EMPLOYEE.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                                           where c.COMPANYID == strCompanyID && e.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && en.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState
                                           select e.T_HR_EMPLOYEE;

            //查已经过离职确认，但还未到离职时间的员工
            IQueryable<T_HR_EMPLOYEE> qn = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY") on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                           join l in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on e.T_HR_EMPLOYEE.EMPLOYEEID equals l.EMPLOYEEID
                                           where c.COMPANYID == strCompanyID && l.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy
                                           select e.T_HR_EMPLOYEE;

            IQueryable<T_HR_EMPLOYEE> ents = qy;
            if (qn.Count() > 0)
            {
                ents = qy.Concat(qn);
            }

            return ents;
        }

        /// <summary>
        /// 根据公司ID获取该公司所属的所有非离职员工
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEE> GetEmployeeByCompanyID(string strCompanyID, DateTime dtCheckDate)
        {
            string strIsAgnecy = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
            string strEditState = Convert.ToInt32(Common.EditStates.Actived).ToString();
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            //查正式和试用员工
            IQueryable<T_HR_EMPLOYEE> qy = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY") on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                           join en in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE") on e.T_HR_EMPLOYEE.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                                           where c.COMPANYID == strCompanyID && e.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && en.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState
                                           select e.T_HR_EMPLOYEE;

            //查已经过离职确认，但还未到离职时间的员工
            IQueryable<T_HR_EMPLOYEE> qn = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY") on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                           join l in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on e.T_HR_EMPLOYEE.EMPLOYEEID equals l.EMPLOYEEID
                                           where c.COMPANYID == strCompanyID && l.CHECKSTATE == strCheckState && l.LEFTOFFICEDATE >= dtCheckDate && e.ISAGENCY == strIsAgnecy
                                           select e.T_HR_EMPLOYEE;

            IQueryable<T_HR_EMPLOYEE> ents = qy;
            if (qn.Count() > 0)
            {
                ents = qy.Concat(qn);
            }

            return ents;
        }

        /// <summary>
        /// 根据部门ID获取该公司所属的所有非离职员工
        /// </summary>
        /// <param name="strDepartmentID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEE> GetEmployeeByDepartmentID(string strDepartmentID)
        {
            string strIsAgnecy = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
            string strEditState = Convert.ToInt32(Common.EditStates.Actived).ToString();
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            //查正式和试用员工
            IQueryable<T_HR_EMPLOYEE> qy = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join en in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE") on e.T_HR_EMPLOYEE.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                                           where d.DEPARTMENTID == strDepartmentID && e.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && en.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState
                                           select e.T_HR_EMPLOYEE;

            //查已经过离职确认，但还未到离职时间的员工
            IQueryable<T_HR_EMPLOYEE> qn = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                           join l in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on e.T_HR_EMPLOYEE.EMPLOYEEID equals l.EMPLOYEEID
                                           where d.DEPARTMENTID == strDepartmentID && l.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy
                                           select e.T_HR_EMPLOYEE;

            IQueryable<T_HR_EMPLOYEE> ents = qy;
            if (qn.Count() > 0)
            {
                ents = qy.Concat(qn);
            }

            return ents;
        }

        /// <summary>
        /// 根据岗位ID获取该岗位所属的所有非离职员工
        /// </summary>
        /// <param name="strPostID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEE> GetEmployeeByPostID(string strPostID)
        {
            string strIsAgnecy = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
            string strEditState = Convert.ToInt32(Common.EditStates.Actived).ToString();
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            //查正式和试用员工
            IQueryable<T_HR_EMPLOYEE> qy = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join en in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE") on e.T_HR_EMPLOYEE.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                                           where e.T_HR_POST.POSTID == strPostID && e.T_HR_EMPLOYEE.EMPLOYEESTATE != "2" && en.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState
                                           select e.T_HR_EMPLOYEE;

            //查已经过离职确认，但还未到离职时间的员工
            IQueryable<T_HR_EMPLOYEE> qn = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                           join l in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on e.T_HR_EMPLOYEE.EMPLOYEEID equals l.EMPLOYEEID
                                           where e.T_HR_POST.POSTID == strPostID && l.CHECKSTATE == strCheckState && e.ISAGENCY == strIsAgnecy
                                           select e.T_HR_EMPLOYEE;

            IQueryable<T_HR_EMPLOYEE> ents = qy;
            if (qn.Count() > 0)
            {
                ents = qy.Concat(qn);
            }

            return ents;
        }

        /// <summary>
        /// 获取岗位员工ID集合
        /// </summary>
        /// <param name="strPostID"></param>
        /// <returns></returns>
        public IQueryable<string> GetEmployeeIDsByPostID(string strPostID)
        {
            string strIsAgnecy = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
            string strEditState = Convert.ToInt32(Common.EditStates.Actived).ToString();
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            var ents = from c in dal.GetObjects()
                       join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                       where b.T_HR_POST.POSTID == strPostID && b.EDITSTATE == strEditState
                       select c.EMPLOYEEID;
            return ents;
        }
        /// <summary>
        /// 根据身份证号码，查询简历库是否有员工信息，如果有，就赋值给员工实体
        /// </summary>
        /// <param name="sNumberID">身份证号码</param>
        /// <returns></returns>
        public T_HR_EMPLOYEE GetEmployeeByNumberID(string sNumberID)
        {
            T_HR_EMPLOYEE retEnt = new T_HR_EMPLOYEE();
            retEnt.EMPLOYEEID = Guid.NewGuid().ToString();
            ResumeBLL bll = new ResumeBLL();
            T_HR_RESUME ent = bll.GetResumeByNumber(sNumberID);
            if (ent != null)
            {
                retEnt.EMPLOYEECNAME = ent.NAME;
                retEnt.SEX = ent.SEX;
                retEnt.NATION = ent.NATION;
                retEnt.PROVINCE = ent.PROVINCE;
                retEnt.HEIGHT = ent.HEIGHT;
                retEnt.MARRIAGE = ent.MARRIAGE;
                retEnt.IDNUMBER = ent.IDCARDNUMBER;
                retEnt.POLITICS = ent.POLITICS;
                retEnt.EMAIL = ent.EMAIL;
                retEnt.MOBILE = ent.MOBILE;
                retEnt.PHOTO = ent.PHOTO;
                retEnt.T_HR_RESUME = ent;

            }
            return retEnt;
        }


        /// <summary>
        /// 存在相同的指纹编号返回true
        /// </summary>
        /// <param name="FingerPrintID"></param>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public bool IsExistFingerPrintID(string FingerPrintID, string employeeID)
        {
            var ents = from c in dal.GetObjects()
                       where c.FINGERPRINTID == FingerPrintID && c.EMPLOYEEID != employeeID && c.EMPLOYEESTATE!="2"
                       select c;
            if (ents.Count() > 0)
            {
                return true;
            }

            return false;
        }

        public IQueryable<T_HR_EMPLOYEE> GetEmployeesPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
            //员工入职审核通过再显示。

            ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
            ///
            var ents = from o in dal.GetObjects()
                       join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                       join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                       join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                       join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                       select new
                       {
                           EMPLOYEE = o,
                           POSTID = p.POSTID,
                           DEPARTMENTID = d.DEPARTMENTID,
                           COMPANYID = c.COMPANYID,
                           CREATEUSERID = o.CREATEUSERID,
                           OWNERCOMPANYID = o.OWNERCOMPANYID,
                           OWNERPOSTID = o.OWNERPOSTID,
                           OWNERID = o.OWNERID,
                           OWNERDEPARTMENT = o.OWNERDEPARTMENTID
                       };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            IQueryable<T_HR_EMPLOYEE> ent = from n in ents
                                            select n.EMPLOYEE;
            ent = ent.OrderBy(sort);

            ent = Utility.Pager<T_HR_EMPLOYEE>(ent, pageIndex, pageSize, ref pageCount);
            return ent;

        }
        /// <summary>
        /// 根据公司ID，计算员工当前时间的工龄
        /// </summary>
        /// <param name="strCompanyID"></param>
        public void UpdateEmployeeWorkAgeByID(string strCompanyID)
        {
            DateTime dtCheck = DateTime.Now;
            DateTime dtCur = DateTime.Parse(dtCheck.ToString("yyyy-MM") + "-1");
            IQueryable<T_HR_EMPLOYEE> ents = GetEmployeeByCompanyID(strCompanyID, dtCur);

            if (ents.Count() == 0)
            {
                return;
            }

            foreach (T_HR_EMPLOYEE item in ents)
            {
                T_HR_EMPLOYEEENTRY entEntry = dal.GetObjects<T_HR_EMPLOYEEENTRY>().Where(c => c.T_HR_EMPLOYEE.EMPLOYEEID == item.EMPLOYEEID).FirstOrDefault();
                if (entEntry == null)
                {
                    continue;
                }

                decimal dWorkAgeCheck = 0;
                //    dWorkAgeCheck =Convert.ToDecimal( item.WORKINGAGE);
                decimal.TryParse(item.WORKINGAGE.ToString(), out dWorkAgeCheck);

                DateTime dtEntryDate = entEntry.ENTRYDATE.Value;
                TimeSpan tsStart = new TimeSpan(dtEntryDate.Ticks);
                TimeSpan tsEnd = new TimeSpan(dtCheck.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart);

                int iTotalDay = ts.Days;

                decimal dWorkAge = iTotalDay / 30;
                if (dWorkAge <= dWorkAgeCheck)
                {
                    continue;
                }

                dal.Update(item);

            }
        }
        /// <summary>
        /// 检查该用户所属公司是否存在下级公司
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>返回该用户所属公司是否存在下级公司</returns>
        public bool CheckChildCompanyByUserID(string userID)
        {
            //bool返回值
            bool back = false;
            //查询该用户所有岗位的所有公司
            var ents = from v in dal.GetObjects<T_HR_COMPANY>()
                       join em in dal.GetObjects() on v.COMPANYID equals em.OWNERCOMPANYID
                       join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on em.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                       join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                       join dp in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals dp.DEPARTMENTID
                       where em.EMPLOYEEID == userID && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1" && v.COMPANYID == dp.T_HR_COMPANY.COMPANYID
                       select v;
            if (ents.Count() > 0)
            {
                foreach (var item in ents)
                {
                    //查询该用户所有岗位的所有公司是否有下级公司
                    var childCompany = from t in dal.GetObjects<T_HR_COMPANY>()
                                       where t.FATHERID == item.COMPANYID
                                       select t;
                    if (childCompany.Count() > 0)
                    {
                        back = true;
                        return back;
                    }
                }
            }
            else
            {
                return back;
            }
            return back;
        }


        /// <summary>
        /// 根据传入的实体名和主键id，去相应实体里面根据
        /// employeeID，postID，departmentID，companyID找出相应的信息（可扩充）
        /// </summary>
        /// <param name="employeeID">员工id</param>
        /// <param name="postID">岗位id</param>
        /// <param name="departmentID">部门id</param>
        /// <param name="companyID">公司id</param>
        /// <returns>相应员工的组织架构名称</returns>
        public V_EMPLOYEEVIEW GetEmpOrgInfoByID(string employeeID, string postID, string departmentID, string companyID)
        {
            try
            {

                V_EMPLOYEEVIEW employeeView = new V_EMPLOYEEVIEW();

                //员工信息
                var entEmployee = (from e in dal.GetObjects()
                                   where e.EMPLOYEEID == employeeID
                                   select e).FirstOrDefault();

                //岗位名
                var entPostName = (from p in dal.GetObjects<T_HR_POST>()
                                   join pd in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pd.POSTDICTIONARYID
                                   where p.POSTID == postID
                                   select pd.POSTNAME).FirstOrDefault();


                //部门名
                var entDepartmentName = (from d in dal.GetObjects<T_HR_DEPARTMENT>()
                                         join dd in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dd.DEPARTMENTDICTIONARYID
                                         where d.DEPARTMENTID == departmentID
                                         select dd.DEPARTMENTNAME).FirstOrDefault();

                //公司名
                string strCompanyName = string.Empty;



                var entcom = (from c in dal.GetObjects<T_HR_COMPANY>()
                              where c.COMPANYID == companyID
                              select c).FirstOrDefault();
                if (entcom != null)
                {
                    strCompanyName = entcom.BRIEFNAME == null ? entcom.CNAME : entcom.BRIEFNAME;
                }
                //岗位级别
                decimal? enPostLevel = 0;
                try
                {
                    var enPostLevels = (from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                       join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                       where p.POSTID == postID
                                       && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1"
                                       && ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID
                                       select ep.POSTLEVEL).FirstOrDefault();
                    enPostLevel = enPostLevels;
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug("获取员工岗位级别信息出错" + DateTime.Now.ToString() + ex.Message);
                    var enPostLevels = (from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                        join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                        where p.POSTID == postID
                                        && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1"
                                        //&& ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID
                                        select ep.POSTLEVEL).FirstOrDefault();
                    enPostLevel = enPostLevels;
                }
                employeeView.EMPLOYEECNAME = entEmployee.EMPLOYEECNAME;
                employeeView.POSTNAME = entPostName;
                employeeView.DEPARTMENTNAME = entDepartmentName;
                employeeView.COMPANYNAME = strCompanyName;
                employeeView.SEX = entEmployee.SEX;
                employeeView.POSTLEVEL = enPostLevel;
                employeeView.EMPLOYEEID = entEmployee.EMPLOYEEID;
                employeeView.EMPLOYEECODE = entEmployee.EMPLOYEECODE;
                employeeView.EMPLOYEESTATE = entEmployee.EMPLOYEESTATE;
                return employeeView;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取员工信息出错" + DateTime.Now.ToString() + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 导出员工档案
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        public byte[] ExportEmployee(string companyID, string filterString, string[] paras, string userId)
        {
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);

                SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_HR_EMPLOYEE");

                var company = (from e in dal.GetObjects<T_HR_COMPANY>()
                               where e.COMPANYID == companyID
                               select e).FirstOrDefault();
                string companyName = company.CNAME;
                #region 获取公司所有员工
                var entEmployee = from e in dal.GetObjects()
                                  join c in dal.GetObjects<T_HR_COMPANY>() on e.OWNERCOMPANYID equals c.COMPANYID
                                  join d in dal.GetObjects<T_HR_DEPARTMENT>() on e.OWNERDEPARTMENTID equals d.DEPARTMENTID
                                  join dic in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals dic.DEPARTMENTDICTIONARYID
                                  join p in dal.GetObjects<T_HR_POST>() on e.OWNERPOSTID equals p.POSTID
                                  join pic in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pic.POSTDICTIONARYID

                                  join resume in dal.GetObjects<T_HR_RESUME>() on e.IDNUMBER equals resume.IDCARDNUMBER into Retemp
                                  from t0 in Retemp.DefaultIfEmpty()//左连接

                                  join edu in dal.GetObjects<T_HR_EDUCATEHISTORY>() on t0.RESUMEID equals edu.T_HR_RESUME.RESUMEID into Resumetemp
                                  from t1 in Resumetemp.DefaultIfEmpty()//左连接

                                  join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on e.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID into Entertemp
                                  from t2 in Entertemp.DefaultIfEmpty()//左连接

                                  join m in dal.GetObjects<T_HR_EMPLOYEECHECK>() on e.EMPLOYEEID equals m.T_HR_EMPLOYEE.EMPLOYEEID into Checktemp
                                  from t3 in Checktemp.DefaultIfEmpty()//左连接

                                  join p in dal.GetObjects<T_HR_PENSIONMASTER>() on e.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID into Pensiontemp
                                  from t4 in Pensiontemp.DefaultIfEmpty()//左连接
                                  where e.OWNERCOMPANYID == companyID //&& e.EMPLOYEESTATE != "4"//4为未提交
                                  select new V_EmployeeEntryInfo
                                  {
                                      EmployeeID = e.EMPLOYEEID,
                                      EmployeeName = e.EMPLOYEECNAME,
                                      CompanyName = c.CNAME,
                                      EmployeeCode = e.EMPLOYEECODE,
                                      DepartmentName = dic.DEPARTMENTNAME,
                                      PostName = pic.POSTNAME,
                                      EmployeeState = e.EMPLOYEESTATE,
                                      Sex = e.SEX,
                                      IdNumber = e.IDNUMBER,
                                      FingerPrintID = e.FINGERPRINTID,
                                      SocialServiceYear = e.SOCIALSERVICEYEAR,
                                      Nation = e.NATION,
                                      BirthdayDate = e.BIRTHDAY,
                                      Marriage = e.MARRIAGE,
                                      Mobile = e.MOBILE,
                                      OfficePhone = e.OFFICEPHONE,
                                      Bank = e.BANKID,
                                      BankCardNumber = e.BANKCARDNUMBER,
                                      PensionComputerNo = t4.COMPUTERNO,
                                      PensionCardID = t4.CARDID,
                                      PensionCheckState = t4.CHECKSTATE,
                                      Email = e.EMAIL,
                                      RegResidence = e.REGRESIDENCE,
                                      FamilyAddress = e.FAMILYADDRESS,
                                      CurrentAddress = e.CURRENTADDRESS,
                                      UrgencyPerson = e.URGENCYPERSON,
                                      UrgencyContact = e.URGENCYCONTACT,
                                      EntryDates = t2.ENTRYDATE,
                                      CheckDate = t3.BEREGULARDATE,
                                      Education = e.TOPEDUCATION,
                                      GraduateSchool = t1.SCHOONAME,
                                      Specialty = t1.SPECIALTY,
                                      Remark = e.REMARK,
                                      CreateUserID = e.CREATEUSERID,
                                      OwnerID = e.OWNERID,
                                      OwnerPostID = e.OWNERPOSTID,
                                      OwnerDepartmentID = e.OWNERDEPARTMENTID,
                                      OwnerCompanyID = e.OWNERCOMPANYID
                                  };
                var temp = entEmployee.Where(filterString, queryParas.ToArray());
                var source = temp.GroupBy(t => t.EmployeeID);
                //temp = entEmployee.GroupBy(t => t.EmployeeID);
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    temp = temp.Where(filterString, queryParas.ToArray());
                //}

                List<V_EmployeeEntryInfo> list = new List<V_EmployeeEntryInfo>();
                foreach (var item in source)
                {
                    var info = item.Where(t => t.PensionCheckState == "2").FirstOrDefault();//审核通过的社保信息
                    if (info != null)
                    {
                        list.Add(info);
                    }
                    else
                    {
                        info = item.Where(t => !string.IsNullOrWhiteSpace(t.PensionCardID)).FirstOrDefault();//没有审核通过的则加载有卡号的
                        if (info != null)
                        {
                            list.Add(info);
                        }
                        else
                        {
                            list.Add(item.FirstOrDefault());//连卡号都没有的情况也存在
                        }
                    }
                }
                #endregion
                byte[] result;
                DataTable dt = TableToExportInit();//定义表头
                if (list != null && list.Any())
                {
                    DataTable dttoExport = GetDataConversion(dt, list);//组装数据
                    result = Utility.OutFileStream(companyName + Utility.GetResourceStr(" 员工档案信息"), dttoExport);
                    return result;
                }
                else
                {
                    return null;
                }
                #region 旧方法
                //if (entEmployee!=null&&entEmployee.Count() > 0)
                //{
                //    #region 定义表头名字，添加表头后面要添加相应数据
                //    List<string> colName = new List<string>();
                //    colName.Add("员工姓名");
                //    colName.Add("员工编号");
                //    colName.Add("性别");
                //    colName.Add("身份证号码");
                //    colName.Add("指纹编号");
                //    colName.Add("社保缴交起始时间");
                //    colName.Add("民族");
                //    colName.Add("身高");
                //    colName.Add("出生日期");
                //    colName.Add("血型");
                //    colName.Add("手机号码");
                //    colName.Add("开户银行");
                //    colName.Add("银行账号");
                //    colName.Add("籍贯");
                //    colName.Add("电子邮件");
                //    colName.Add("户口所在地");
                //    colName.Add("家庭地址");
                //    colName.Add("目前居住地");
                //    colName.Add("员工状态");
                //    colName.Add("备注");
                //    #endregion
                //    StringBuilder sb = new StringBuilder();
                //    for (int i = 0; i < colName.Count; i++)
                //    {
                //        sb.Append(colName[i] + ",");
                //    }
                //    sb.Append("\r\n"); //换行
                //    foreach (var ent in entEmployee)
                //    {
                //        #region 每一行加入数据
                //        sb.Append(ent.EMPLOYEECNAME + ",");
                //        sb.Append(ent.EMPLOYEECODE + ",");
                //        sb.Append(ent.SEX + ",");
                //        sb.Append(ent.IDNUMBER + ",");
                //        sb.Append(ent.FINGERPRINTID + ",");
                //        sb.Append(ent.SOCIALSERVICEYEAR + ",");
                //        sb.Append(ent.NATION + ",");
                //        sb.Append(ent.HEIGHT + ",");
                //        sb.Append(ent.BIRTHDAY + ",");
                //        sb.Append(ent.BLOODTYPE + ",");
                //        sb.Append(ent.MOBILE + ",");
                //        sb.Append(ent.BANKID + ",");
                //        sb.Append(ent.BANKCARDNUMBER + ",");
                //        sb.Append(ent.PROVINCE + ",");
                //        sb.Append(ent.EMAIL + ",");
                //        sb.Append(ent.REGRESIDENCE + ",");
                //        sb.Append(ent.FAMILYADDRESS + ",");
                //        sb.Append(ent.CURRENTADDRESS + ",");
                //        sb.Append(ent.EMPLOYEESTATE + ",");
                //        sb.Append(ent.REMARK + ",");
                //        #endregion
                //        sb.Append("\r\n");//换行
                //    }
                //    byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
                //    return result;
                //}
                //else
                //{
                //    return null;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployee导出员工档案出错:" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 数据组装
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="V_EmployeeEntryInfo"></param>
        /// <returns></returns>
        private DataTable GetDataConversion(DataTable dt, List<V_EmployeeEntryInfo> entEmployee)
        {
            dt.Rows.Clear();
            foreach (var item in entEmployee)
            {
                try
                {
                    List<T_SYS_DICTIONARY> tmp = new List<T_SYS_DICTIONARY>();//new PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] );
                    using (SysDictionaryBLL bll = new SysDictionaryBLL())
                    {
                        List<T_SYS_DICTIONARY> dictlist = bll.GetSysDictionaryByCategory(new List<string>{ "EMPLOYEESTATE", "TOPEDUCATION", "NATION", "SEX", "MARRIAGE" });
                    }
                    
                    DataRow row = dt.NewRow();
                    decimal dicValue = -1;
                    T_SYS_DICTIONARY tempDic = null;
                    #region 每行数据
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        switch (i)
                        {
                            case 0: row[i] = item.EmployeeName; break;
                            case 1: row[i] = item.CompanyName; break;
                            case 2: row[i] = item.DepartmentName; break;
                            case 3: row[i] = item.PostName; break;
                            case 4:
                                dicValue = this.ConverTo(item.EmployeeState);
                                tempDic = tmp.Where(s => s.DICTIONCATEGORY == "EMPLOYEESTATE" && s.DICTIONARYVALUE == dicValue).FirstOrDefault();
                                row[i] = tempDic == null ? "" : tempDic.DICTIONARYNAME;
                                break;
                            case 5:
                                dicValue = this.ConverTo(item.Sex);
                                tempDic = tmp.Where(s => s.DICTIONCATEGORY == "SEX" && s.DICTIONARYVALUE == dicValue).FirstOrDefault();
                                row[i] = tempDic == null ? "" : tempDic.DICTIONARYNAME;
                                break;
                            case 6: row[i] = item.IdNumber; break;
                            case 7: row[i] = item.FingerPrintID; break;
                            case 8: row[i] = item.SocialServiceYear; break;
                            case 9:
                                dicValue = this.ConverTo(item.Nation);
                                tempDic = tmp.Where(s => s.DICTIONCATEGORY == "NATION" && s.DICTIONARYVALUE == dicValue).FirstOrDefault();
                                row[i] = tempDic == null ? "" : tempDic.DICTIONARYNAME;
                                break;
                            case 10: row[i] = item.BirthdayDate == null ? "" : item.BirthdayDate.Value.ToString("yyyy-MM-dd"); break;
                            case 11:
                                dicValue = this.ConverTo(item.Marriage);
                                tempDic = tmp.Where(s => s.DICTIONCATEGORY == "MARRIAGE" && s.DICTIONARYVALUE == dicValue).FirstOrDefault();
                                row[i] = tempDic == null ? "" : tempDic.DICTIONARYNAME; break;
                            case 12: row[i] = item.Mobile; break;
                            case 13: row[i] = item.OfficePhone; break;
                            case 14: row[i] = item.Bank; break;
                            case 15: row[i] = item.BankCardNumber; break;
                            case 16: row[i] = item.PensionComputerNo; break;
                            case 17: row[i] = item.PensionCardID; break;
                            case 18: row[i] = item.Email; break;
                            case 19: row[i] = item.RegResidence; break;
                            case 20: row[i] = item.FamilyAddress; break;
                            case 21: row[i] = item.CurrentAddress; break;
                            case 22: row[i] = item.UrgencyPerson; break;
                            case 23: row[i] = item.UrgencyContact; break;
                            case 24: row[i] = item.EntryDates == null ? "" : item.EntryDates.Value.ToString("yyyy-MM-dd"); break;
                            case 25: row[i] = item.CheckDate == null ? "" : item.CheckDate.Value.ToString("yyyy-MM-dd"); ; break;
                            case 26:
                                dicValue = this.ConverTo(item.Education);
                                tempDic = tmp.Where(s => s.DICTIONCATEGORY == "TOPEDUCATION" && s.DICTIONARYVALUE == dicValue).FirstOrDefault();
                                row[i] = tempDic == null ? "" : tempDic.DICTIONARYNAME; break;
                            case 27: row[i] = item.GraduateSchool; break;
                            case 28: row[i] = item.Specialty; break;
                            case 29: row[i] = item.Remark; break;
                        }
                    }
                    dt.Rows.Add(row);
                    #endregion
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug("ExportEmployee导出员工档案组装DataTable时出错:" + ex.Message);
                    return null;
                }
            }
            return dt;
        }

        /// <summary>
        ///把string型简单的转成 decimal
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        private decimal ConverTo(string strValue)
        {
            try
            {
                return Convert.ToDecimal(strValue);
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// 定义表头
        /// </summary>
        /// <returns></returns>
        private DataTable TableToExportInit()
        {
            DataTable dt = new DataTable();
            #region 定义表头名称
            DataColumn colCordSD = new DataColumn();
            colCordSD.ColumnName = "员工姓名";
            colCordSD.DataType = typeof(string);
            dt.Columns.Add(colCordSD);

            DataColumn colComp = new DataColumn();
            colComp.ColumnName = "公司";
            colComp.DataType = typeof(string);
            dt.Columns.Add(colComp);

            DataColumn colDept = new DataColumn();
            colDept.ColumnName = "部门";
            colDept.DataType = typeof(string);
            dt.Columns.Add(colDept);

            DataColumn colPost = new DataColumn();
            colPost.ColumnName = "岗位";
            colPost.DataType = typeof(string);
            dt.Columns.Add(colPost);

            DataColumn col11 = new DataColumn();
            col11.ColumnName = Utility.GetResourceStr("员工状态");
            col11.DataType = typeof(string);
            dt.Columns.Add(col11);

            DataColumn colCordFD = new DataColumn();
            colCordFD.ColumnName = "性别";
            colCordFD.DataType = typeof(string);
            dt.Columns.Add(colCordFD);

            DataColumn colCordYD = new DataColumn();
            colCordYD.ColumnName = "身份证号码";
            colCordYD.DataType = typeof(string);
            dt.Columns.Add(colCordYD);

            DataColumn colCordMD = new DataColumn();
            colCordMD.ColumnName = "指纹编号";
            colCordMD.DataType = typeof(string);
            dt.Columns.Add(colCordMD);

            DataColumn colsoc = new DataColumn();
            colsoc.ColumnName = Utility.GetResourceStr("社保缴交起始时间");
            colsoc.DataType = typeof(string);
            dt.Columns.Add(colsoc);

            DataColumn colNation = new DataColumn();
            colNation.ColumnName = Utility.GetResourceStr("民族");
            colNation.DataType = typeof(string);
            dt.Columns.Add(colNation);

            //DataColumn colHight = new DataColumn();
            //colHight.ColumnName = Utility.GetResourceStr("身高");
            //colHight.DataType = typeof(string);
            //dt.Columns.Add(colHight);

            DataColumn col1 = new DataColumn();
            col1.ColumnName = Utility.GetResourceStr("出生日期");
            col1.DataType = typeof(string);
            dt.Columns.Add(col1);

            DataColumn colMarry = new DataColumn();
            colMarry.ColumnName = Utility.GetResourceStr("婚姻状况");
            colMarry.DataType = typeof(string);
            dt.Columns.Add(colMarry);

            DataColumn col3 = new DataColumn();
            col3.ColumnName = Utility.GetResourceStr("手机号码");
            col3.DataType = typeof(string);
            dt.Columns.Add(col3);

            DataColumn col13 = new DataColumn();
            col13.ColumnName = Utility.GetResourceStr("办公电话");
            col13.DataType = typeof(string);
            dt.Columns.Add(col13);

            DataColumn col4 = new DataColumn();
            col4.ColumnName = Utility.GetResourceStr("开户银行");
            col4.DataType = typeof(string);
            dt.Columns.Add(col4);

            DataColumn col5 = new DataColumn();
            col5.ColumnName = Utility.GetResourceStr("银行账号");
            col5.DataType = typeof(string);
            dt.Columns.Add(col5);

            DataColumn colPenCardComputerID = new DataColumn();
            colPenCardComputerID.ColumnName = Utility.GetResourceStr("社保电脑号");
            colPenCardComputerID.DataType = typeof(string);
            dt.Columns.Add(colPenCardComputerID);

            DataColumn colPenCardID = new DataColumn();
            colPenCardID.ColumnName = Utility.GetResourceStr("社保卡号");
            colPenCardID.DataType = typeof(string);
            dt.Columns.Add(colPenCardID);

            DataColumn col7 = new DataColumn();
            col7.ColumnName = Utility.GetResourceStr("电子邮件");
            col7.DataType = typeof(string);
            dt.Columns.Add(col7);

            DataColumn col8 = new DataColumn();
            col8.ColumnName = Utility.GetResourceStr("户口所在地");
            col8.DataType = typeof(string);
            dt.Columns.Add(col8);

            DataColumn col9 = new DataColumn();
            col9.ColumnName = Utility.GetResourceStr("家庭地址");
            col9.DataType = typeof(string);
            dt.Columns.Add(col9);

            DataColumn col10 = new DataColumn();
            col10.ColumnName = Utility.GetResourceStr("目前居住地");
            col10.DataType = typeof(string);
            dt.Columns.Add(col10);

            DataColumn colUrgency = new DataColumn();
            colUrgency.ColumnName = Utility.GetResourceStr("紧急联系人");
            colUrgency.DataType = typeof(string);
            dt.Columns.Add(colUrgency);

            DataColumn colUrgencyContact = new DataColumn();
            colUrgencyContact.ColumnName = Utility.GetResourceStr("紧急联系方式");
            colUrgencyContact.DataType = typeof(string);
            dt.Columns.Add(colUrgencyContact);

            DataColumn colEntryDate = new DataColumn();
            colEntryDate.ColumnName = Utility.GetResourceStr("入职时间");
            colEntryDate.DataType = typeof(string);
            dt.Columns.Add(colEntryDate);

            DataColumn colCheckDate = new DataColumn();
            colCheckDate.ColumnName = Utility.GetResourceStr("转正时间");
            colCheckDate.DataType = typeof(string);
            dt.Columns.Add(colCheckDate);

            DataColumn colEdu = new DataColumn();
            colEdu.ColumnName = Utility.GetResourceStr("学历");
            colEdu.DataType = typeof(string);
            dt.Columns.Add(colEdu);

            DataColumn colSch = new DataColumn();
            colSch.ColumnName = Utility.GetResourceStr("毕业院校");
            colSch.DataType = typeof(string);
            dt.Columns.Add(colSch);

            DataColumn colSpc = new DataColumn();
            colSpc.ColumnName = Utility.GetResourceStr("专业");
            colSpc.DataType = typeof(string);
            dt.Columns.Add(colSpc);

            DataColumn colRemark = new DataColumn();
            colRemark.ColumnName = Utility.GetResourceStr("备注");
            colRemark.DataType = typeof(string);
            dt.Columns.Add(colRemark);
            #endregion
            return dt;
        }

        #region  手机端通讯录接口
        /// <summary>
        /// 获取权限内的所有员工
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">查询条件</param>
        /// <param name="paras">条件的参数</param>
        /// <param name="pageCount">页面数量</param>
        /// <param name="userID">登录人ID，权限过滤所需</param>
        /// <param name="isGetPhoto">是否获取相片</param>
        /// <returns>返回符合条件的员工列表</returns>
        public IQueryable<V_MOBILEEMPLOYEE> GetEmployeeListMobile(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, bool isGetPhoto)
        {
            //接收条件中的参数
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            //权限过滤
            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
            //调用获取员工方法
            var ents = GetEmployeeComm(isGetPhoto);
            //判断查询条件，条件不为空就进行过滤
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            else
            {
                ents = ents.Where(t => t.OwnerCompanyID == userID);
            }
            //排序
            ents = ents.OrderBy(t => t.BriefName).ThenBy(t=>t.DepartMentName).ThenBy(t=>t.EmployeeCNName);
            //分页
            ents = Utility.Pager<V_MOBILEEMPLOYEE>(ents, pageIndex, pageSize, ref pageCount);
            //IQueryable<V_MOBILEEMPLOYEE> list = ents.OrderBy(t => t.BriefName).OrderBy(t => t.DepartMentName).OrderBy(t => t.EmployeeCNName);
            return ents;
        }
        /// <summary>
        /// 获取员工详细信息
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <param name="employeePostID">员工岗位ID</param>
        /// <returns>返回员工详细信息</returns>
        public V_MOBILEEMPLOYEE GetEmployeeSingleMobile(string employeeID, string employeePostID)
        {
            var ent = GetEmployeeComm(true).Where(s => s.EmployeeId == employeeID && s.OwnerPostID == employeePostID).FirstOrDefault();
            return ent;
        }
        /// <summary>
        /// 获取员工
        /// </summary>
        /// <param name="isGetPhoto">是否获取照片</param>
        /// <returns>返回所有有效员工</returns>
        public IQueryable<V_MOBILEEMPLOYEE> GetEmployeeComm(bool isGetPhoto)
        {
            IQueryable<V_MOBILEEMPLOYEE> ents;
            ents = from o in dal.GetObjects()
                   join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                   join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                   join pd in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pd.POSTDICTIONARYID
                   join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                   join cd in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on
                       d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals cd.DEPARTMENTDICTIONARYID
                   join cm in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals cm.COMPANYID
                   where o.EDITSTATE == "1" && (o.EMPLOYEESTATE == "0" || o.EMPLOYEESTATE == "1" || o.EMPLOYEESTATE == "3") && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1"
                   select new V_MOBILEEMPLOYEE
                   {
                       EmployeeId = o.EMPLOYEEID,
                       EmployeeName = o.EMPLOYEEENAME,
                       EmployeeCNName = o.EMPLOYEECNAME,
                       Sex = o.SEX,
                       Photo = isGetPhoto == true ? o.PHOTO : null,
                       Email = o.EMAIL,
                       Address = o.FAMILYADDRESS,
                       Mobile = o.MOBILE,
                       Tel = o.OFFICEPHONE,
                       UrgencyPerson = o.URGENCYPERSON,
                       UrgencyContact = o.URGENCYCONTACT,
                       OwnerCompanyID = cm.COMPANYID,
                       CompanyName = cm.CNAME,
                       BriefName = cm.BRIEFNAME,
                       OwnerDepartMentID = d.DEPARTMENTID,
                       DepartMentName = cd.DEPARTMENTNAME,
                       OwnerPostID = p.POSTID,
                       PostName = pd.POSTNAME,
                       CreateUserID = o.CREATEUSERID,
                       OwnerID = o.OWNERID,
                       PostLevel = ep.POSTLEVEL
                   };
            return ents;
        }
        #endregion

        public string GetEmailNameIsExistNameAddOne(string userName,string employeeId)
        {
            //SMT.PermissionServiceClient client =
            //    new PermissionServiceClient();
            

            string StrReturn = "";
            //try
            //{
            //    bool hasReption = true;
            //    int i = 1;
            //    while (hasReption)
            //    {
            //        var q = from ent in dal.GetTable()
            //                where ent.EMAIL == userName + "@sinomaster.com" && ent.EMPLOYEEID != employeeId
            //                select ent;

            //        string refUserName = client.GetUserNameIsExistNameAddOne(userName, employeeId);

            //        if (q.Count() <= 0 && refUserName == userName)
            //        {
            //            hasReption = false;
            //            StrReturn = userName;
            //        }
            //        else
            //        {
            //            StrReturn = userName + i.ToString();
            //            userName = StrReturn;
            //        }
            //        i = i + 1;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Tracer.Debug("GetEmailNameIsExistNameAddOne" + System.DateTime.Now.ToString() + " " + ex.ToString());

            //}
            return StrReturn;
        }

        #region  预算所需的员工岗位情况的接口
        /// <summary>
        /// 个人费用报销离职入职提示接口
        /// </summary>
        /// <param name="list">传入集合参数</param>
        /// <returns>返回处理后的集合</returns>
        public List<V_EMPLOYEEPOSTFORFB> GetEmployeeListForFB(List<V_EMPLOYEEPOSTFORFB> list)
        {
            if (list.Count != 0)
            {
                foreach (V_EMPLOYEEPOSTFORFB item in list)
                {
                    IQueryable<V_EMPLOYEEPOSTFORFB> ents;
                    //用员工ID和岗位ID进行查询
                    ents = from v in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on v.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           where v.EMPLOYEEID == item.OWNERID && v.OWNERPOSTID == item.OWNERPOSTID
                           select new V_EMPLOYEEPOSTFORFB
                           {
                               PERSONBUDGETAPPLYDETAILID = item.PERSONBUDGETAPPLYDETAILID,
                               OWNERID = item.OWNERID,
                               OWNERPOSTID = item.OWNERPOSTID,
                               EMPLOYEESTATE = v.EMPLOYEESTATE,
                               ISAGENCY = ep.ISAGENCY
                           };

                    if (ents.Count() == 0)
                    {
                        ents = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                               where ep.T_HR_EMPLOYEE.EMPLOYEEID == item.OWNERID && ep.T_HR_POST.POSTID == item.OWNERPOSTID
                               select new V_EMPLOYEEPOSTFORFB
                               {
                                   PERSONBUDGETAPPLYDETAILID = item.PERSONBUDGETAPPLYDETAILID,
                                   OWNERID = ep.T_HR_EMPLOYEE.EMPLOYEEID,
                                   OWNERPOSTID = ep.T_HR_POST.POSTID,
                                   EMPLOYEESTATE = ep.T_HR_EMPLOYEE.EMPLOYEESTATE,
                                   ISAGENCY = ep.ISAGENCY
                               };
                    }
                    //如果查询有结果，则去检查此岗位是否在异动中，否则去查找异动后的岗位
                    if (ents != null && ents.Count() > 0)
                    {
                        var check = from o in dal.GetObjects<T_HR_EMPLOYEEPOSTCHANGE>()
                                    where o.T_HR_EMPLOYEE.EMPLOYEEID == item.OWNERID && o.FROMPOSTID == item.OWNERPOSTID
                                    && o.CHECKSTATE == "1"
                                    select o;
                        //获取个人活动经费
                        var sumSelf = from v in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>()
                                      join custom in dal.GetObjects<T_HR_SALARYARCHIVE>() on v.T_HR_SALARYARCHIVE.SALARYARCHIVEID equals custom.SALARYARCHIVEID
                                      where custom.CHECKSTATE == "2" && custom.EDITSTATE == "1" && custom.EMPLOYEEID == item.OWNERID
                                      orderby v.CREATEDATE descending
                                      select v;
                        if (check != null && check.Count() > 0)
                        {
                            item.EMPLOYEESTATE = "10";
                        }
                        else
                        {
                            item.EMPLOYEESTATE = ents.FirstOrDefault().EMPLOYEESTATE;
                            item.ISAGENCY = ents.FirstOrDefault().ISAGENCY;
                        }
                        //个人活动经费
                        if (sumSelf != null && sumSelf.Count() > 0)
                        {
                            item.SUM = sumSelf.FirstOrDefault().SUM;
                        }
                        else
                        {
                            //item.SUM = 0;
                        }

                    }
                    else
                    {
                        //查找异动后的岗位
                        var checkNew = from o in dal.GetObjects<T_HR_EMPLOYEEPOSTCHANGE>()
                                       where o.T_HR_EMPLOYEE.EMPLOYEEID == item.OWNERID && o.FROMPOSTID == item.OWNERPOSTID
                                       && o.CHECKSTATE == "2"
                                       orderby o.UPDATEDATE descending
                                       select o;
                        //如果异动后的岗位存在
                        if (checkNew != null && checkNew.Count() > 0)
                        {
                            //查询此岗位是否代理岗位
                            var findState = from s in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                            where s.T_HR_EMPLOYEE.EMPLOYEEID == item.OWNERID && s.T_HR_POST.POSTID == checkNew.FirstOrDefault().TOPOSTID
                                            && s.EDITSTATE == "1"
                                            select s;
                            //获取个人活动经费
                            var sumSelf = from v in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>()
                                          join custom in dal.GetObjects<T_HR_SALARYARCHIVE>() on v.T_HR_SALARYARCHIVE.SALARYARCHIVEID equals custom.SALARYARCHIVEID
                                          where custom.CHECKSTATE == "2" && custom.EDITSTATE == "1" && custom.EMPLOYEEID == item.OWNERID
                                          orderby v.CREATEDATE descending
                                          select v;
                            //个人活动经费
                            if (sumSelf != null && sumSelf.Count() > 0)
                            {
                                item.SUM = sumSelf.FirstOrDefault().SUM;
                            }
                            else
                            {
                                //item.SUM = 0;
                            }
                            item.ISAGENCY = findState.FirstOrDefault().ISAGENCY;
                            item.EMPLOYEESTATE = "11";
                            //返回此岗位的所有相关信息
                            var detail = from p in dal.GetObjects<T_HR_POST>()
                                         join pd in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pd.POSTDICTIONARYID
                                         join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                         join cd in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on
                                             d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals cd.DEPARTMENTDICTIONARYID
                                         join cm in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals cm.COMPANYID
                                         where p.POSTID == findState.FirstOrDefault().T_HR_POST.POSTID
                                         select new V_EMPLOYEEPOSTFORFB
                                         {
                                             NEWPOSTID = p.POSTID,
                                             NEWPOSTNAME = pd.POSTNAME,
                                             NEWDEPARTMENTID = d.DEPARTMENTID,
                                             NEWDEPARTMENTNAME = cd.DEPARTMENTNAME,
                                             NEWCOMPANYID = cm.COMPANYID,
                                             NEWCOMPANYNAME = cm.CNAME
                                         };
                            if (detail != null && detail.Count() > 0)
                            {
                                item.NEWPOSTID = detail.FirstOrDefault().NEWPOSTID;
                                item.NEWPOSTNAME = detail.FirstOrDefault().NEWPOSTNAME;
                                item.NEWDEPARTMENTID = detail.FirstOrDefault().NEWDEPARTMENTID;
                                item.NEWDEPARTMENTNAME = detail.FirstOrDefault().NEWDEPARTMENTNAME;
                                item.NEWCOMPANYID = detail.FirstOrDefault().NEWCOMPANYID;
                                item.NEWCOMPANYNAME = detail.FirstOrDefault().NEWCOMPANYNAME;
                            }
                            else
                            {
                                item.EMPLOYEESTATE = "12";
                            }
                        }
                        else
                        {
                            item.EMPLOYEESTATE = "12";
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取员工个人活动经费(根据公司生成)
        /// </summary>
        /// <param name="strCompantID">公司ID</param>
        /// <param name="OwnerId">活动经费下拨人ID</param>
        /// <returns>返回员工个人活动经费</returns>
        public List<V_EMPLOYEEFUNDS> GetEmployeeFunds(string strCompantID, string OwnerId)
        {
            try
            {
                //定义返回的集合
                List<V_EMPLOYEEFUNDS> list = new List<V_EMPLOYEEFUNDS>();
                List<V_EMPLOYEEFUNDS> employeeAll = new List<V_EMPLOYEEFUNDS>();

                #region 根据发薪公司获取员工
                EmployeeSalaryRecordBLL esBll = new EmployeeSalaryRecordBLL();
                string strmsg = "通过发薪结构结算薪资：";
                var company = dal.GetObjects<T_HR_COMPANY>().Where(c => c.COMPANYID == strCompantID).FirstOrDefault();

                int year = DateTime.Now.Year; int month = DateTime.Now.Month - 1;
                //查询在职的结算岗位为指定岗位的员工及员工薪资档案
                //List<T_HR_EMPLOYEE> list = new List<T_HR_EMPLOYEE>();
                //获取员工的基本信息
                var ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join pd in dal.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pd.POSTDICTIONARYID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join cd in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals cd.DEPARTMENTDICTIONARYID
                           join cm in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals cm.COMPANYID
                           join salaryAhive in dal.GetTable<T_HR_SALARYARCHIVE>() on o.EMPLOYEEID equals salaryAhive.EMPLOYEEID
                           where salaryAhive.PAYCOMPANY == strCompantID//指定的发薪机构的薪资档案
                           && salaryAhive.CHECKSTATE == "2"//薪资档案审核通过的
                           && o.EMPLOYEESTATE != "2"//在职员工
                           && (salaryAhive.OTHERSUBJOIN < year
                           || (salaryAhive.OTHERSUBJOIN == year
                           && salaryAhive.OTHERADDDEDUCT <= month))
                           && o.EDITSTATE == "1" && (o.EMPLOYEESTATE == "0" || o.EMPLOYEESTATE == "1" || o.EMPLOYEESTATE == "3") && ep.CHECKSTATE == "2"
                           && ep.EDITSTATE == "1" && ep.ISAGENCY == "0"//主岗位
                           orderby ep.POSTLEVEL
                           select new 
                           {   ATTENDANCEORGID=salaryAhive.ATTENDANCEORGID,
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYECNAME = o.EMPLOYEECNAME,
                               EMPLOYEESTATE = o.EMPLOYEESTATE,
                               ISAGENCY = ep.ISAGENCY,
                               COMPANYID = cm.COMPANYID,
                               COMPANYNAME = cm.BRIEFNAME,
                               DEPARTMENTID = d.DEPARTMENTID,
                               DEPARTMENTNAME = cd.DEPARTMENTNAME,
                               POSTID = p.POSTID,
                               POSTNAME = pd.POSTNAME,
                               EMPLOYEENAME=o.EMPLOYEECNAME
                           };
                if (ents.Count() > 0)
                {
                    var employees = ents.Distinct().ToList();
                   Tracer.Debug(strmsg + "活动经费下拨：获取到所有发薪机构 " + company.CNAME 
                       + " 上的员工薪资档案，共 " + employees.Count().ToString() + "条");                    
                    int i = 1;
                    foreach (var employee in employees)
                    {
                        try
                        {
                            var q = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                    join salaryAhive in dal.GetTable<T_HR_SALARYARCHIVE>()
                                    on ent.EMPLOYEEID equals salaryAhive.EMPLOYEEID
                                    where salaryAhive.EMPLOYEEID == employee.EMPLOYEEID
                                          && salaryAhive.CHECKSTATE == "2"    //终审通过的薪资档案
                                          && ent.EMPLOYEESTATE != "2"    //员工还在职
                                          && (salaryAhive.OTHERSUBJOIN < year
                                          || (salaryAhive.OTHERSUBJOIN == year
                                          && salaryAhive.OTHERADDDEDUCT <= month))
                                    select salaryAhive;
                            if (q.Count() > 0)
                            {
                                List<T_HR_SALARYARCHIVE> Salarylist = q.ToList();
                                var salays = Salarylist.OrderByDescending(s => s.OTHERSUBJOIN).ThenByDescending(p => p.OTHERADDDEDUCT).ThenByDescending(s => s.CREATEDATE);
                                //获取该员工最新的一条生效的薪资档案。
                                T_HR_SALARYARCHIVE salaryAhive = salays.FirstOrDefault();
                                //如果最新薪资档案的发薪机构不为空且不等于当前结算的机构，那么跳过去
                                if (!string.IsNullOrEmpty(salaryAhive.PAYCOMPANY))
                                {
                                    if (salaryAhive.PAYCOMPANY != strCompantID)
                                    {
                                        Tracer.Debug("活动经费下拨：该员工被跳过，因员工最新薪资档案获取到的发薪机构跟当前结算机构不符，员工姓名：" + salaryAhive.EMPLOYEENAME
                                            + " 年份：" + year + " 月份：" + month);
                                        continue;
                                    }
                                }
                            }
                            if (!esBll.checkEmployeeHaveMainPostInCompany(employee.EMPLOYEEID, employee.ATTENDANCEORGID))
                            {
                                Tracer.Debug("活动经费下拨：该员工被跳过，因为该员工在考勤机构已经没有生效的主岗位,"
                                    + "员工姓名：" + employee.EMPLOYEENAME + ",员工id：" + employee.EMPLOYEEID
                                    + " 发薪结构：" + company.CNAME + " 发薪机构id" + strCompantID);
                                continue;
                            }
                            //
                            var qs = from ent in employeeAll
                                     where ent.EMPLOYEEID == employee.EMPLOYEEID
                                     select ent;
                            if (qs.Count() > 0)
                            {
                                continue;
                            }
                            Tracer.Debug("活动经费下拨：通过发薪机构获取的最终员工：" + employee.EMPLOYEENAME + System.Environment.NewLine);

                            V_EMPLOYEEFUNDS ve = new V_EMPLOYEEFUNDS();
                            ve.EMPLOYEEID = employee.EMPLOYEEID;
                            ve.EMPLOYECNAME = employee.EMPLOYEENAME;
                            ve.EMPLOYEESTATE = employee.EMPLOYEESTATE;
                            ve.ISAGENCY = employee.ISAGENCY;
                            ve.COMPANYID = employee.COMPANYID;
                            ve.COMPANYNAME = employee.COMPANYNAME;
                            ve.DEPARTMENTID = employee.DEPARTMENTID;
                            ve.DEPARTMENTNAME = employee.DEPARTMENTNAME;
                            ve.POSTID = employee.POSTID;
                            ve.POSTNAME = employee.POSTNAME;
                            employeeAll.Add(ve);
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("活动经费下拨异常：" + ex.ToString());
                        }
                    }
                }
                else
                {
                    Tracer.Debug("活动经费下拨：通过发薪结构获取的员工薪资档案为空，发薪机构：" + company.CNAME);
                    return list;
                }
                #endregion

                #region 获取该公司员工信息


                if (employeeAll == null || employeeAll.Count() == 0)
                {
                    return list;
                }
                else
                {
                    for(int i=0;i<employeeAll.Count()-1;i++)
                    {
                        var item = employeeAll[i];                        
                        Tracer.Debug("活动经费下拨人员主岗位过滤结果："+i+" "+item.COMPANYNAME + item.DEPARTMENTNAME + item.POSTNAME + item.EMPLOYECNAME + System.Environment.NewLine);
                    }
                }
                #endregion

                DepartmentBLL bllDept = new DepartmentBLL();
                SalaryArchiveBLL bllSalary = new SalaryArchiveBLL();
                AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL();
                AttendMonthlyBalanceBLL bllAttendBalance = new AttendMonthlyBalanceBLL();
                decimal dYear = 0, dMonth = 0;
                dYear = DateTime.Now.AddMonths(-1).Year;     //取年份
                dMonth = DateTime.Now.AddMonths(-1).Month;   //取月份
                Tracer.Debug("GetEmployeeFunds:strCompantID" + strCompantID + " OwnerId:" + OwnerId
                    + "年份:" + dYear + " 月份：" + dMonth);
                //计算应出勤天数使用
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                dtStart = DateTime.Parse(DateTime.Now.AddMonths(-1).ToString("yyyy-MM") + "-1");
                dtEnd = dtStart.AddMonths(1).AddSeconds(-1);

                foreach (var item in employeeAll)
                {
                    if (item.EMPLOYECNAME == "林敏")
                    {
                        string str = "";
                    }
                    var change = from pg in dal.GetObjects<T_HR_EMPLOYEEPOSTCHANGE>()
                                 where pg.T_HR_EMPLOYEE.EMPLOYEEID == item.EMPLOYEEID && pg.FROMPOSTID == item.POSTID
                                 && pg.CHECKSTATE == "1"
                                 select pg;
                    if (change != null && change.Count() > 0)
                    {
                        item.EMPLOYEESTATE = "10";
                    }
                    #region 查询员工活动经费

                    string strFullDeptName = string.Empty;
                    bllDept.GetFullDepartmentNameByID(item.DEPARTMENTID, ref strFullDeptName);
                    item.DEPARTMENTNAME = strFullDeptName;

                    //查询该员工是否有个人活动经费
                    T_HR_SALARYARCHIVE archive = bllSalary.GetSalaryArchiveApprovedByEmployeeID(item.EMPLOYEEID, int.Parse(dYear.ToString()), int.Parse(dMonth.ToString()), strCompantID);
                    if (archive == null)
                    {
                        string msg = " 活动经费下拨：" + item.EMPLOYECNAME + " 未获取到该员工有效的薪资档案";
                        item.ATTENDREMARK = "未找到该员工有效的薪资档案";
                        Tracer.Debug(msg);
                        continue;
                    }
                    Tracer.Debug("GetEmployeeFunds 员工：" + item.EMPLOYECNAME + "获取的活动经费档案id：" + archive.SALARYARCHIVEID);
                    var custom = from cs in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>().Include("T_HR_SALARYARCHIVE")
                                 where cs.T_HR_SALARYARCHIVE.SALARYARCHIVEID == archive.SALARYARCHIVEID
                                 && cs.ASSIGNERID == OwnerId
                                 select cs;

                    if (custom == null || custom.Count() == 0)
                    {
                        string msg = " 活动经费下拨：" + item.EMPLOYECNAME + " 该员工的薪资档案未设置活动经费";
                        item.ATTENDREMARK = "该员工在薪资档案中未设置活动经费";
                        Tracer.Debug(msg);
                        continue;
                    }

                    T_HR_CUSTOMGUERDONARCHIVE entCustomArchive = custom.ToList().FirstOrDefault();
                    if (entCustomArchive == null || entCustomArchive.SUM == null || entCustomArchive.SUM == 0)
                    {
                        string msg = " 活动经费下拨：" + item.EMPLOYECNAME + " 该员工的薪资档案未设置活动经费";
                        item.ATTENDREMARK = "该员工在薪资档案中未设置活动经费";
                        continue;
                    }
                    #endregion

                    //查询员工的出勤情况                        
                    var attend = from at in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>()
                                 where at.EMPLOYEEID == item.EMPLOYEEID && at.CHECKSTATE == "2" && at.EDITSTATE == "1"
                                 && at.BALANCEYEAR == dYear && at.BALANCEMONTH == dMonth
                                 orderby at.CREATEDATE descending
                                 select at;
                    if (attend != null && attend.Count() > 0)
                    {
                        #region 有考勤的情况
                        T_HR_ATTENDMONTHLYBALANCE entCurMonth = attend.FirstOrDefault();

                        item.NEEDATTENDDAYS = entCurMonth.NEEDATTENDDAYS;
                        if (entCurMonth.NEEDATTENDDAYS == null)
                        {
                            T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign = bllAttendanceSolutionAsign.GetAttSolAsignByEmployeeIDAndDate(item.EMPLOYEEID, dtStart, dtEnd);

                            if (entAttSolAsign.T_HR_ATTENDANCESOLUTION != null)
                            {
                                decimal? dNeedDays = bllAttendBalance.GetNeedAttendDays(dtStart, dtEnd, item.COMPANYID,item.EMPLOYEEID, entAttSolAsign.T_HR_ATTENDANCESOLUTION);
                                if (dNeedDays.Value > 0)
                                {
                                    item.NEEDATTENDDAYS = dNeedDays;
                                }
                            }
                        }

                        item.REALATTENDDAYS = entCurMonth.REALATTENDDAYS;
                        if (entCurMonth.REALNEEDATTENDDAYS != entCurMonth.REALATTENDDAYS)
                        {
                            item.REALATTENDDAYS = entCurMonth.REALNEEDATTENDDAYS - entCurMonth.AFFAIRLEAVEDAYS;
                            //病假单独算
                            //if (entCurMonth.SICKLEAVEDAYS != null)
                            //{
                            //    decimal dSickleavedays = entCurMonth.SICKLEAVEDAYS.Value - 1;
                            //    if (dSickleavedays > 0)
                            //    {
                            //        item.REALATTENDDAYS = item.REALATTENDDAYS - dSickleavedays;
                            //    }
                            //}
                        }
                        try
                        {
                            if (strCompantID == "bac05c76-0f5b-40ae-b73b-8be541ed35ed")//只有在线公司迟到会扣活动经费。。。
                            {
                                #region 在线公司
                                decimal? realDays = item.REALATTENDDAYS - entCurMonth.SICKLEAVEDAYS;
                                string reMart = GetIsAgency(item.ISAGENCY) + "；" + GetEmployeeDicration(item.EMPLOYEESTATE, item.EMPLOYEEID) +
                           "；应出勤" + item.NEEDATTENDDAYS.ToString() + "天，实际出勤" + realDays.ToString() + "天";
                                if (entCurMonth.LATEMINUTES != null)
                                {
                                    if (entCurMonth.LATEMINUTES.Value > 0)
                                    {
                                        reMart = reMart + "，迟到" + entCurMonth.LATEMINUTES + "分钟：";
                                    }
                                }
                                item.ATTENDREMARK = reMart;
                                item.NEEDSUM = custom.ToList().FirstOrDefault().SUM;
                                //实际下拨金额=（实际出勤天数/应出勤天数*应下拨金额）-迟到扣款（总额/应出勤时间/每天工作小时数/60分*迟到分钟数）
                                decimal total = Convert.ToDecimal((item.REALATTENDDAYS / item.NEEDATTENDDAYS) * item.NEEDSUM);
                                if (entCurMonth.LATEMINUTES != null)
                                {
                                    if (entCurMonth.LATEMINUTES.Value > 0)
                                    {
                                        decimal d = 0;
                                        AttendanceSolutionAsignBLL bll = new AttendanceSolutionAsignBLL();
                                        T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign
                                            = bll.GetAttendanceSolutionAsignByEmployeeIDAndDate(item.EMPLOYEEID, DateTime.Now);
                                        if (entAttSolAsign != null)
                                        {
                                            d = entAttSolAsign.T_HR_ATTENDANCESOLUTION.WORKTIMEPERDAY.Value;

                                            if (d > 0)
                                            {
                                                double LaterValue = (double)((item.NEEDSUM / item.NEEDATTENDDAYS).Value)
                                                    / (double)d / 60 * (double)entCurMonth.LATEMINUTES.Value;
                                                total = total - Convert.ToDecimal(LaterValue);
                                            }
                                        }
                                    }
                                }
                                item.REALSUM = Math.Round(total);
                                //如果有病假情况，一个月可以带薪一天病假
                                if (entCurMonth.SICKLEAVEDAYS != null && entCurMonth.SICKLEAVEDAYS > 1)
                                {
                                    total = CalculateSickSum(entCurMonth, item.EMPLOYEEID, total);//扣除病假
                                    item.REALSUM = Math.Round(total);
                                }
                                list.Add(item);
                                #endregion
                            }
                            else
                            {
                                decimal? realDays = item.REALATTENDDAYS - entCurMonth.SICKLEAVEDAYS;
                                item.ATTENDREMARK = GetIsAgency(item.ISAGENCY) + "；" + GetEmployeeDicration(item.EMPLOYEESTATE, item.EMPLOYEEID) +
                                    "；应出勤" + item.NEEDATTENDDAYS.ToString() + "天，实际出勤" + realDays.ToString() + "天";
                                item.NEEDSUM = custom.ToList().FirstOrDefault().SUM;
                                decimal total = Convert.ToDecimal((item.REALATTENDDAYS / item.NEEDATTENDDAYS) * item.NEEDSUM); //实际下拨金额=（实际出勤天数/应出勤天数*应下拨金额）

                                item.REALSUM = Math.Round(total);
                                //如果有病假情况，一个月可以带薪一天病假
                                if (entCurMonth.SICKLEAVEDAYS != null && entCurMonth.SICKLEAVEDAYS > 1)
                                {
                                    total = CalculateSickSum(entCurMonth, item.EMPLOYEEID, total);//扣除病假
                                    item.REALSUM = Math.Round(total);
                                }
                                list.Add(item);
                            }
                        }
                        catch (Exception ex)
                        {
                            Utility.SaveLog("GetEmployeeFunds算取在线公司经费时出错，错误信息：" + ex.ToString() + "员工ID姓名为" + item.EMPLOYEEID + item.EMPLOYECNAME);
                            continue;
                        }
                    }
                    else
                    {
                        item.NEEDATTENDDAYS = 0;
                        item.REALATTENDDAYS = 0;
                        item.ATTENDREMARK = GetIsAgency(item.ISAGENCY) + "；"
                            + GetEmployeeDicration(item.EMPLOYEESTATE, item.EMPLOYEEID)
                            + "；" + "未获取到员工当月考勤结算记录，活动经费无法计算，请手工录入。";
                        item.NEEDSUM = custom.ToList().FirstOrDefault().SUM;
                        item.REALSUM = 0;
                        list.Add(item);
                    }
                }


                //list = list.Where(c => c.COMPANYID == strCompantID && c.ISAGENCY == "0").ToList();
                List<V_EMPLOYEEFUNDS> entResList = new List<V_EMPLOYEEFUNDS>();

                foreach (V_EMPLOYEEFUNDS item in list)
                {
                    var q = from c in entResList
                            where c.EMPLOYEEID == item.EMPLOYEEID
                            select c;

                    if (q.Count() > 0)
                    {
                        continue;
                    }
                    Tracer.Debug("活动经费下拨结果:"
                        + item.COMPANYNAME + "-" + item.DEPARTMENTNAME + "-" + item.POSTNAME + "-" + item.EMPLOYECNAME
                        + "  金额："+item.REALSUM +"   备注："+ item.ATTENDREMARK+System.Environment.NewLine);
                    entResList.Add(item);
                }
                return entResList;
            }
            catch (Exception ex)
            {
                Utility.SaveLog("GetEmployeeFunds运行出错，错误信息：" + ex.ToString());
                throw ex;
            }
        }
        /// <summary>
        /// 扣除员工病假扣款的活动经费
        /// </summary>
        /// <param name="attend">该员工的该月考勤数据</param>
        /// <param name="employeeID">员工ID</param>
        /// <param name="sum">下拨活动经费</param>
        /// <returns>扣除病假后的活动经费</returns>
        private decimal CalculateSickSum(T_HR_ATTENDMONTHLYBALANCE attend, string employeeID, decimal sum)
        {
            try
            {
                decimal dedSum = 0;//扣除的金额
                decimal year = DateTime.Now.Year;
                var ent = from at in dal.GetObjects<T_HR_ATTENDYEARLYBALANCE>()
                          where at.EMPLOYEEID == employeeID && at.BALANCEYEAR == year
                          orderby at.BALANCEDATE descending
                          select at;
                if (ent != null && ent.Count() > 0)
                {
                    decimal YearSickDays = ent.FirstOrDefault().SICKLEAVEDAYS.Value;//年度结算里面记录的总病假天数
                    decimal needDays = attend.NEEDATTENDDAYS.Value;//应出勤天数
                    decimal sickDays = attend.SICKLEAVEDAYS.Value;//当月请病假天数
                    int workingAge = GetEmployeeWorkAgeByID(employeeID);//工作时长，单位：月
                    double ratio = 0;//扣除系数
                    if (YearSickDays <= 12)//当年病假没有超过12天
                    {
                        ratio = 0.2;
                    }
                    else
                    {
                        ratio = 0.4;
                        if (workingAge <= 36)//工作36个月（三年）内
                        {
                            ratio = 0.6;
                        }
                    }
                    dedSum = (sum / needDays) * (sickDays - 1) * Convert.ToDecimal(ratio);//扣除的金额=（经费/应出勤天数）*（病假-1）*相应系数
                }
                sum = sum - dedSum;
                return sum;
            }
            catch (Exception ex)
            {
                Utility.SaveLog("GetEmployeeFunds扣除病假扣款异常：" + ex.ToString());
                return sum;
            }
        }
        /// <summary>
        /// 获取员工个人活动经费(根据所选员工)
        /// </summary>
        /// <param name="list">自定义员工信息集合</param>
        /// <returns>返回自定义员工信息集合</returns>
        public List<V_EMPLOYEEFUNDS> GetEmployeeFundsList(List<V_EMPLOYEEFUNDS> list)
        {
            try
            {
                if (list.Count != 0)
                {
                    DepartmentBLL bllDept = new DepartmentBLL();
                    SalaryArchiveBLL bllSalary = new SalaryArchiveBLL();
                    AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL();
                    AttendMonthlyBalanceBLL bllAttendBalance = new AttendMonthlyBalanceBLL();
                    decimal dYear = 0, dMonth = 0;
                    dYear = DateTime.Now.AddMonths(-1).Year;     //取年份
                    dMonth = DateTime.Now.AddMonths(-1).Month;   //取月份

                    //计算应出勤天数使用
                    DateTime dtStart = new DateTime();
                    DateTime dtEnd = new DateTime();
                    dtStart = DateTime.Parse(DateTime.Now.AddMonths(-1).ToString("yyyy-MM") + "-1");
                    dtEnd = dtStart.AddMonths(1).AddSeconds(-1);

                    foreach (V_EMPLOYEEFUNDS item in list)
                    {
                        
                        IQueryable<V_EMPLOYEEFUNDS> ents;
                        //用员工ID和岗位ID进行查询
                        ents = from v in dal.GetObjects()
                               join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on v.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                               where v.EMPLOYEEID == item.EMPLOYEEID
                               && ep.T_HR_POST.POSTID == item.POSTID
                               && ep.EDITSTATE == "1"    
                               select new V_EMPLOYEEFUNDS
                               {
                                   EMPLOYEEID = item.EMPLOYEEID,
                                   POSTID = item.POSTID,
                                   EMPLOYEESTATE = v.EMPLOYEESTATE,
                                   ISAGENCY = ep.ISAGENCY
                               };

                        if (ents.Count() == 0)
                        {
                            ents = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   where ep.T_HR_EMPLOYEE.EMPLOYEEID == item.EMPLOYEEID && ep.T_HR_POST.POSTID == item.POSTID
                                   select new V_EMPLOYEEFUNDS
                                   {
                                       EMPLOYEEID = ep.T_HR_EMPLOYEE.EMPLOYEEID,
                                       POSTID = ep.T_HR_POST.POSTID,
                                       EMPLOYEESTATE = ep.T_HR_EMPLOYEE.EMPLOYEESTATE,
                                       ISAGENCY = ep.ISAGENCY
                                   };
                        }
                        //如果查询有结果，则去检查此岗位是否在异动中
                        if (ents != null && ents.Count() > 0)
                        {
                            var change = from pg in dal.GetObjects<T_HR_EMPLOYEEPOSTCHANGE>()
                                         where pg.T_HR_EMPLOYEE.EMPLOYEEID == item.EMPLOYEEID && pg.FROMPOSTID == item.POSTID
                                         && pg.CHECKSTATE == "1"
                                         select pg;
                            if (change != null && change.Count() > 0)
                            {
                                item.EMPLOYEESTATE = "10";
                            }
                            else
                            {
                                item.EMPLOYEESTATE = ents.FirstOrDefault().EMPLOYEESTATE;
                            }
                            item.ISAGENCY = ents.FirstOrDefault().ISAGENCY;
                            if (item.ISAGENCY == "1")
                            {
                                item.ATTENDREMARK = "兼职员工下拨活动经费获取不到考勤结算记录无法计算活动经费金额，请确认。";
                                continue;
                            }
                            //查询该员工是否有个人活动经费
                            T_HR_SALARYARCHIVE archive = bllSalary.GetSalaryArchiveApprovedByEmployeeID(item.EMPLOYEEID, int.Parse(dYear.ToString()), int.Parse(dMonth.ToString()), item.COMPANYID);
                            if (archive == null)
                            {
                                item.ATTENDREMARK = "未找到该员工有效的薪资档案";
                                continue;
                            }

                            var custom = from cs in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>().Include("T_HR_SALARYARCHIVE")
                                         where cs.T_HR_SALARYARCHIVE.SALARYARCHIVEID == archive.SALARYARCHIVEID
                                         select cs;


                            if (custom == null)
                            {
                                item.ATTENDREMARK = "该员工在薪资档案中未设置活动经费";
                                continue;
                            }

                            if (custom.Count() == 0)
                            {
                                item.ATTENDREMARK = "该员工在薪资档案中未设置活动经费";
                                continue;
                            }

                            T_HR_CUSTOMGUERDONARCHIVE entCustomArchive = custom.ToList().FirstOrDefault();
                            if (entCustomArchive == null)
                            {
                                item.ATTENDREMARK = "该员工在薪资档案中未设置活动经费";
                                continue;
                            }

                            if (entCustomArchive.SUM == null)
                            {
                                item.ATTENDREMARK = "该员工在薪资档案中未设置活动经费";
                                continue;
                            }

                            if (entCustomArchive.SUM == 0)
                            {
                                item.ATTENDREMARK = "该员工在薪资档案中设置的活动经费为0";
                                continue;
                            }


                            //查询员工的出勤情况
                            var attend = from at in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>()
                                         where at.EMPLOYEEID == item.EMPLOYEEID && at.CHECKSTATE == "2" && at.EDITSTATE == "1"
                                         && at.BALANCEYEAR == DateTime.Now.Year && at.BALANCEMONTH == DateTime.Now.Month-1
                                         orderby at.CREATEDATE descending
                                         select at;
                            if (attend != null && attend.Count() > 0)
                            {
                                T_HR_ATTENDMONTHLYBALANCE entCurMonth = attend.FirstOrDefault();

                                item.NEEDATTENDDAYS = entCurMonth.NEEDATTENDDAYS;
                                if (entCurMonth.NEEDATTENDDAYS == null)
                                {
                                    T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign = bllAttendanceSolutionAsign.GetAttSolAsignByEmployeeIDAndDate(item.EMPLOYEEID, dtStart, dtEnd);

                                    if (entAttSolAsign.T_HR_ATTENDANCESOLUTION != null)
                                    {
                                        decimal? dNeedDays = bllAttendBalance.GetNeedAttendDays(dtStart, dtEnd, item.COMPANYID,item.EMPLOYEEID, entAttSolAsign.T_HR_ATTENDANCESOLUTION);
                                        if (dNeedDays.Value > 0)
                                        {
                                            item.NEEDATTENDDAYS = dNeedDays;
                                        }
                                    }
                                }

                                item.REALATTENDDAYS = entCurMonth.REALATTENDDAYS;
                                if (entCurMonth.REALNEEDATTENDDAYS != entCurMonth.REALATTENDDAYS)
                                {
                                    item.REALATTENDDAYS = entCurMonth.REALNEEDATTENDDAYS - entCurMonth.AFFAIRLEAVEDAYS;
                                    //病假单独算
                                    //if (entCurMonth.SICKLEAVEDAYS != null)
                                    //{
                                    //    decimal dSickleavedays = entCurMonth.SICKLEAVEDAYS.Value - 1;
                                    //    if (dSickleavedays > 0)
                                    //    {
                                    //        item.REALATTENDDAYS = item.REALATTENDDAYS - dSickleavedays;
                                    //    }
                                    //}
                                }
                                try
                                {
                                    if (item.COMPANYID == "bac05c76-0f5b-40ae-b73b-8be541ed35ed")//只有在线公司迟到会扣活动经费。。。
                                    {
                                        #region 在线公司
                                        decimal? realDays = item.REALATTENDDAYS - entCurMonth.SICKLEAVEDAYS;
                                        string reMart = GetIsAgency(item.ISAGENCY) + "；" + GetEmployeeDicration(item.EMPLOYEESTATE, item.EMPLOYEEID) +
                                   "；应出勤" + item.NEEDATTENDDAYS.ToString() + "天，实际出勤" + realDays.ToString() + "天";
                                        if (entCurMonth.LATEMINUTES != null)
                                        {
                                            if (entCurMonth.LATEMINUTES.Value > 0)
                                            {
                                                reMart = reMart + "，迟到" + entCurMonth.LATEMINUTES + "分钟：";
                                            }
                                        }
                                        item.ATTENDREMARK = reMart;
                                        item.NEEDSUM = custom.ToList().FirstOrDefault().SUM;
                                        //实际下拨金额=（实际出勤天数/应出勤天数*应下拨金额）-迟到扣款（总额/应出勤时间/每天工作小时数/60分*迟到分钟数）
                                        decimal total = Convert.ToDecimal((item.REALATTENDDAYS / item.NEEDATTENDDAYS) * item.NEEDSUM);
                                        if (entCurMonth.LATEMINUTES != null)
                                        {
                                            if (entCurMonth.LATEMINUTES.Value > 0)
                                            {
                                                decimal d = 0;
                                                AttendanceSolutionAsignBLL bll = new AttendanceSolutionAsignBLL();
                                                T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign
                                                    = bll.GetAttendanceSolutionAsignByEmployeeIDAndDate(item.EMPLOYEEID, DateTime.Now);
                                                if (entAttSolAsign != null)
                                                {
                                                    d = entAttSolAsign.T_HR_ATTENDANCESOLUTION.WORKTIMEPERDAY.Value;

                                                    if (d > 0)
                                                    {
                                                        double LaterValue = (double)((item.NEEDSUM / item.NEEDATTENDDAYS).Value)
                                                            / (double)d / 60 * (double)entCurMonth.LATEMINUTES.Value;
                                                        total = total - Convert.ToDecimal(LaterValue);
                                                    }
                                                }
                                            }
                                        }
                                        item.REALSUM = Math.Round(total);
                                        //如果有病假情况，一个月可以带薪一天病假
                                        if (entCurMonth.SICKLEAVEDAYS != null && entCurMonth.SICKLEAVEDAYS > 1)
                                        {
                                            total = CalculateSickSum(entCurMonth, item.EMPLOYEEID, total);//扣除病假
                                            item.REALSUM = Math.Round(total);
                                        }
                                        //list.Add(item);
                                    }
                                        #endregion
                                    else
                                    {
                                        decimal? realDays = item.REALATTENDDAYS - entCurMonth.SICKLEAVEDAYS;
                                        item.ATTENDREMARK = GetIsAgency(item.ISAGENCY) + "；" + GetEmployeeDicration(item.EMPLOYEESTATE, item.EMPLOYEEID) +
                                            "；应出勤" + item.NEEDATTENDDAYS.ToString() + "天，实际出勤" + realDays.ToString() + "天";
                                        item.NEEDSUM = custom.ToList().FirstOrDefault().SUM;
                                        decimal total = Convert.ToDecimal((item.REALATTENDDAYS / item.NEEDATTENDDAYS) * item.NEEDSUM); //实际下拨金额=（实际出勤天数/应出勤天数*应下拨金额）
                                        item.REALSUM = Math.Round(total);
                                        //如果有病假情况，一个月可以带薪一天病假
                                        if (entCurMonth.SICKLEAVEDAYS != null && entCurMonth.SICKLEAVEDAYS > 1)
                                        {
                                            total = CalculateSickSum(entCurMonth, item.EMPLOYEEID, total);//扣除病假
                                            item.REALSUM = Math.Round(total);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Utility.SaveLog("GetEmployeeFunds算取在线公司经费时出错，错误信息：" + ex.ToString() + "员工ID姓名为" + item.EMPLOYEEID + item.EMPLOYECNAME);
                                    continue;
                                }
                            }
                        #endregion
                            else
                            {
                                item.NEEDATTENDDAYS = 0;
                                item.REALATTENDDAYS = 0;
                                item.ATTENDREMARK = GetIsAgency(item.ISAGENCY) + "；"
                                + GetEmployeeDicration(item.EMPLOYEESTATE, item.EMPLOYEEID)
                                + "；" + "未获取到员工当月考勤结算记录，活动经费无法计算，请手工录入。";
                                item.NEEDSUM = entCustomArchive.SUM;
                                item.REALSUM = 0;
                            }

                        }
                    }
                }

                //if (list != null)
                //{
                //    list = list.Where(c => c.ISAGENCY == "0").ToList();
                //}

                return list;
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                return list;
            }
        }

        /// <summary>
        /// 员工状态描述
        /// </summary>
        /// <param name="state">状态值</param>
        /// <returns>返回员工状态描述</returns>
        private string GetEmployeeDicration(string state, string employeeid)
        {
            string employeeState = "";
            switch (state)
            {
                case "0":
                    //在试用期加入此员工的入职时间
                    employeeState = "该员工处于试用期，";
                    employeeState += this.GetEmployeeJoinDate(employeeid) + "请注意";
                    //employeeState = "试用期，请注意";
                    break;
                case "1":
                    employeeState = "在职";
                    employeeState += this.GetEmployeeJoinDate(employeeid);
                    break;
                case "2":
                    //离职中显示离职的时间
                    employeeState = "员工已离职,";
                    employeeState += this.GetEmployeeLeftOfficeDate(employeeid) + "请删除";
                    //employeeState = "员工已离职，请删除";
                    break;
                case "3":

                    employeeState = "此员工离职中，请注意";
                    break;
                case "4":
                    employeeState = "未入职，请删除";
                    break;
                case "10":
                    employeeState = "此岗位异动中，请异动后再处理";
                    break;
            }
            return employeeState;
        }
        /// <summary>
        /// 获取员工入职的时间
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        private string GetEmployeeJoinDate(string employeeid)
        {
            string StrReturn = "";
            var ents = from ent in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")
                       where ent.T_HR_EMPLOYEE.EMPLOYEEID == employeeid
                       select ent;
            if (ents.Count() > 0)
            {
                if (ents.FirstOrDefault().ENTRYDATE != null)
                {
                    StrReturn = "入职时间为：" + ((DateTime)ents.FirstOrDefault().ENTRYDATE).ToString("yyyy年MM月dd日");
                }
            }
            return StrReturn;
        }
        /// <summary>
        /// 获取员工离职的时间
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        private string GetEmployeeLeftOfficeDate(string employeeid)
        {
            string StrReturn = "";
            var ents = from ent in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE")
                       where ent.T_HR_LEFTOFFICE.T_HR_EMPLOYEE.EMPLOYEEID == employeeid
                       select ent;
            if (ents.Count() > 0)
            {
                if (ents.FirstOrDefault().CONFIRMDATE != null)
                {
                    StrReturn = "离职时间为：" + ((DateTime)ents.FirstOrDefault().CONFIRMDATE).ToString("yyyy年MM月dd日");
                }
            }
            return StrReturn;
        }
        /// <summary>
        /// 岗位性质描述
        /// </summary>
        /// <param name="isAgency">性质值</param>
        /// <returns>返回岗位性质描述</returns>
        private string GetIsAgency(string isAgency)
        {
            string mes = "";
            switch (isAgency)
            {
                case "0":
                    mes = "主职";
                    break;
                case "1":
                    mes = "兼职";
                    break;
            }
            return mes;
        }
        #endregion

        #region ILookupEntity 成员

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            //TODO: 分页
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            IQueryable<T_HR_EMPLOYEE> ents = from a in dal.GetObjects()
                                             select a;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(string.IsNullOrEmpty(sort) ? "EMPLOYEEID" : sort);

            ents = Utility.Pager<T_HR_EMPLOYEE>(ents, pageIndex, pageSize, ref pageCount);

            T_HR_EMPLOYEE[] temp = ents.Count() > 0 ? ents.ToArray() : null;
            List<V_EMPLOYEEPOST> EmployeePost = new List<V_EMPLOYEEPOST>();

            if (temp != null)
            {
                foreach (var ent in temp)
                {
                    V_EMPLOYEEPOST tempPost = new V_EMPLOYEEPOST();
                    //tempPost.EMPLOYEEID = ent.EMPLOYEEID;
                    //tempPost.EMPLOYEECODE = ent.EMPLOYEECODE;
                    //tempPost.EMPLOYEECNAME = ent.EMPLOYEECNAME;
                    //tempPost.EMPLOYEEENAME = ent.EMPLOYEEENAME;
                    //tempPost.SEX = ent.SEX;
                    //tempPost.IDNUMBER = ent.IDNUMBER;
                    //var employeePost = DataContext.T_HR_EMPLOYEEPOST.Include("T_HR_EMPLOYEE").Include("T_HR_POST").FirstOrDefault(s => s.T_HR_EMPLOYEE.EMPLOYEEID == ent.EMPLOYEEID);
                    //if (employeePost != null)
                    //{
                    //    //加载岗位字典
                    //    employeePost.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();
                    //    //加载部门
                    //    employeePost.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                    //    //加载部门字典
                    //    employeePost.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
                    //    //加载公司
                    //    employeePost.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                    //}
                    //tempPost.EMPLOYEEPOST = employeePost;
                    tempPost.T_HR_EMPLOYEE = ent;
                    tempPost = GetEmployeeDetailByID(ent.EMPLOYEEID);
                    EmployeePost.Add(tempPost);
                }
            }


            return EmployeePost.ToArray(); ;
        }



        /// <summary>
        /// 获取员工的上级
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="LeaderType">0 直接上级, 1 间接上级 ,2 部门负责人</param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEE> GetEmployeeLeader(string employeeID, int LeaderType)
        {
            List<T_HR_EMPLOYEE> employees = new List<T_HR_EMPLOYEE>();
            if (LeaderType != 2)
            {
                //var employeeposts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                //                    where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && ep.EDITSTATE == "1" && ep.CHECKSTATE == "2" && ep.ISAGENCY == "0"
                //                    select ep;
                var employeeposts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                    where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && ep.EDITSTATE == "1" && ep.CHECKSTATE == "2"
                                    select ep;
                if (employeeposts.Count() > 0)
                {
                    var employeepost = employeeposts.FirstOrDefault();

                    if (!string.IsNullOrEmpty(employeepost.T_HR_POST.FATHERPOSTID))
                    {
                        var directLeaderPost = dal.GetObjects<T_HR_POST>().Where(lp => lp.POSTID == employeepost.T_HR_POST.FATHERPOSTID).FirstOrDefault();
                        if (directLeaderPost != null)
                        {
                            if (LeaderType == 0)
                            {
                                employees = (from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                             where em.T_HR_POST.POSTID == directLeaderPost.POSTID && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                             select em.T_HR_EMPLOYEE).ToList();
                            }
                            else
                            {
                                var indirectLeaderPost = dal.GetObjects<T_HR_POST>().Where(ILP => ILP.POSTID == directLeaderPost.FATHERPOSTID).FirstOrDefault();

                                if (indirectLeaderPost != null)
                                {
                                    employees = (from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                                 where em.T_HR_POST.POSTID == indirectLeaderPost.POSTID && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                                 select em.T_HR_EMPLOYEE).ToList();
                                }
                                else
                                {
                                    employees = null;
                                }
                            }

                        }
                        else
                        {
                            employees = null;
                        }

                    }
                }
            }
            else
            {
                //var empposts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").Include("T_HR_POST.T_HR_DEPARTMENT")
                //               where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && ep.EDITSTATE == "1" && ep.CHECKSTATE == "2" && ep.ISAGENCY == "0"
                //               select ep;
                var empposts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").Include("T_HR_POST.T_HR_DEPARTMENT")
                               where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && ep.EDITSTATE == "1" && ep.CHECKSTATE == "2"
                               select ep;
                if (empposts.Count() > 0)
                {
                    var tmp = empposts.FirstOrDefault();
                    employees = (from eps in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")
                                 where eps.T_HR_POST.POSTID == tmp.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTBOSSHEAD && eps.EDITSTATE == "1"
                                 select eps.T_HR_EMPLOYEE).ToList();
                }
                else
                {
                    employees = null;
                }

            }
            return employees;
        }
        /// <summary>
        /// 获取部门负责人
        /// </summary>
        /// <param name="departmentID">部门ID</param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEEPOST> GetDepartmentLeaders(string departmentID)
        {
            List<T_HR_EMPLOYEEPOST> employees = new List<T_HR_EMPLOYEEPOST>();


            var departments = from d in dal.GetObjects<T_HR_DEPARTMENT>()
                              where d.DEPARTMENTID == departmentID
                              select d;
            if (departments.Count() > 0)
            {
                var tmp = departments.FirstOrDefault();
                employees = (from eps in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                             where eps.T_HR_POST.POSTID == tmp.DEPARTMENTBOSSHEAD && eps.EDITSTATE == "1"
                             select eps).ToList();
            }
            else
            {
                employees = null;
            }


            return employees;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PostID"></param>
        /// <param name="LeaderType"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeLeaders(string PostID, int LeaderType)
        {
            List<V_EMPLOYEEVIEW> employees = new List<V_EMPLOYEEVIEW>();
            if (LeaderType != 2)
            {

                var posts = from c in dal.GetObjects<T_HR_POST>()
                            where c.POSTID == PostID
                            select c;

                if (posts.Count() > 0)
                {
                    var post = posts.FirstOrDefault();

                    if (!string.IsNullOrEmpty(post.FATHERPOSTID))
                    {
                        var directLeaderPost = dal.GetObjects<T_HR_POST>().Where(lp => lp.POSTID == post.FATHERPOSTID).FirstOrDefault();
                        if (directLeaderPost != null)
                        {
                            if (LeaderType == 0)
                            {
                                var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                                     join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                                     join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                                     join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                                     where em.T_HR_POST.POSTID == directLeaderPost.POSTID && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                                     select new V_EMPLOYEEVIEW
                                                     {
                                                         EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                         EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                         OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                                         OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                                         OWNERPOSTID = em.T_HR_POST.POSTID,
                                                         OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                         COMPANYNAME = c.BRIEFNAME,
                                                         DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                         POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                                     };
                                if (diretemployees.Count() > 0)
                                {
                                    employees = diretemployees.ToList();
                                }
                                else
                                {
                                    employees = null;
                                }
                            }
                            else
                            {
                                var indirectLeaderPost = dal.GetObjects<T_HR_POST>().Where(ILP => ILP.POSTID == directLeaderPost.FATHERPOSTID).FirstOrDefault();

                                if (indirectLeaderPost != null)
                                {
                                   
                                    var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                         join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                                         join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                                         join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                                         where em.T_HR_POST.POSTID == indirectLeaderPost.POSTID && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                                         select new V_EMPLOYEEVIEW
                                                         {
                                                             EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                             EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                             OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                                             OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                                             OWNERPOSTID = em.T_HR_POST.POSTID,
                                                             OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                             COMPANYNAME = c.BRIEFNAME,
                                                             DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                             POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                                         };
                                    if (diretemployees.Count() > 0)
                                    {
                                        employees = diretemployees.ToList();
                                    }
                                    else
                                    {
                                        employees = null;
                                    }
                                }
                                else
                                {
                                    employees = null;
                                }
                            }

                        }
                        else
                        {
                            employees = null;
                        }

                    }
                }
            }
            else
            {
                //var empposts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").Include("T_HR_POST.T_HR_DEPARTMENT")
                //               where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && ep.EDITSTATE == "1" && ep.CHECKSTATE == "2" && ep.ISAGENCY == "0"
                //               select ep;

                //岗位所在部门
                var departments = from dp in dal.GetObjects<T_HR_DEPARTMENT>()
                                  join p in dal.GetObjects<T_HR_POST>() on dp.DEPARTMENTID equals p.T_HR_DEPARTMENT.DEPARTMENTID
                                  where p.POSTID == PostID
                                  select dp;
                if (departments.Count() > 0)
                {
                    var tmp = departments.FirstOrDefault();
                    
                    var diretemployees = from eps in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                         join p in dal.GetObjects<T_HR_POST>() on eps.T_HR_POST.POSTID equals p.POSTID
                                         join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                         join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID 
                                         where eps.T_HR_POST.POSTID == tmp.DEPARTMENTBOSSHEAD && eps.EDITSTATE == "1"
                                         select new V_EMPLOYEEVIEW
                                         {
                                             EMPLOYEECNAME = eps.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                             EMPLOYEEID = eps.T_HR_EMPLOYEE.EMPLOYEEID,
                                             OWNERCOMPANYID = eps.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                             OWNERDEPARTMENTID = eps.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                             OWNERPOSTID = eps.T_HR_POST.POSTID,
                                             OWNERID = eps.T_HR_EMPLOYEE.EMPLOYEEID,
                                             COMPANYNAME = c.BRIEFNAME,
                                             DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                             POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                         };
                    if (diretemployees.Count() > 0)
                    {
                        employees = diretemployees.ToList();
                    }
                    else
                    {
                        employees = null;
                    }
                }
                else
                {
                    employees = null;
                }

            }
            if (employees != null && employees.Count() > 0)
            {
                SMT.Foundation.Log.Tracer.Debug("返回直接上级或部门负责人为" + employees[0].EMPLOYEECNAME + " 查询的岗位：" + PostID);
            }
            return employees;
        }

        public List<V_EMPLOYEEVIEW> GetEmployeeLeaderForWP(string employeeId, string postId)
        {
            StringBuilder logMsg = new StringBuilder();
            logMsg.AppendLine("工作计划调用GetEmployeeLeaderForWP,employeeId=" + employeeId + ",postId=" + postId);
            List<V_EMPLOYEEVIEW> employees = new List<V_EMPLOYEEVIEW>();
            T_HR_EMPLOYEE employee = dal.GetObjects<T_HR_EMPLOYEE>().Where(emp => emp.EMPLOYEEID == employeeId && emp.FATHEREMPLOYEEID != null && emp.EMPLOYEESTATE != "2").FirstOrDefault();
            
            if (employee != null)
            {
                if (!string.IsNullOrEmpty(employee.FATHEREMPLOYEEID))
                {
                    T_HR_EMPLOYEE fatherEmployee = dal.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEEID == employee.FATHEREMPLOYEEID && t.EMPLOYEESTATE != "2").FirstOrDefault();
                    if (fatherEmployee != null)
                    {
                        logMsg.AppendLine("在员工个人档案里设置了直接上级姓名,且改员工没有离职，返回FATHEREMPLOYEEID=" + employee.FATHEREMPLOYEEID);
                        var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                             join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                             join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                             join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                             where em.T_HR_EMPLOYEE.EMPLOYEEID == employee.FATHEREMPLOYEEID && em.EDITSTATE == "1" && em.CHECKSTATE == "2" && em.ISAGENCY == "0"
                                             select new V_EMPLOYEEVIEW
                                             {
                                                 EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                 EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                 OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                                 OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                                 OWNERPOSTID = em.T_HR_POST.POSTID,
                                                 OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                 COMPANYNAME = c.BRIEFNAME,
                                                 DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                 POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                             };
                        employees = diretemployees.ToList();
                    }
                    else
                    {
                        employees = GetEmployeeLeaders(postId, 0);
                    }
                }
            }
            else
            {
                logMsg.AppendLine("在员工个人档案里没有设置直接上级姓名，调用GetEmployeeLeaders");
                employees = GetEmployeeLeaders(postId, 0);
            }
            SMT.Foundation.Log.Tracer.Debug(logMsg.ToString());
            return employees;

            ////黄文科、王锡虎、林志刚、李鸿玲 直接上级 王作君
            //List<string> list = new List<string>() { "b9108cf8-dc4f-4f8f-8ffb-2994cd5a5376", "4ae35ebf-731a-42bd-bc58-a08bdb15c5ee", "e5eb28e3-fb0c-4206-ad57-8711a90e752d","746c335f-eae1-449d-9478-c43a70244169","b66bd5d1-384b-4f9a-b9db-a7725021e5d0" };
            //if (list.Contains(employeeId))
            //{
            //    var empId = "b98810b7-e08b-4086-bf16-0aff95dc0324"; //王作君 
            //    var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
            //                         join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
            //                         join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
            //                         join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
            //                         where em.T_HR_EMPLOYEE.EMPLOYEEID == empId && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
            //                         select new V_EMPLOYEEVIEW
            //                         {
            //                             EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
            //                             EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
            //                             OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
            //                             OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
            //                             OWNERPOSTID = em.T_HR_POST.POSTID,
            //                             OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
            //                             COMPANYNAME = c.BRIEFNAME,
            //                             DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
            //                             POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
            //                         };
            //    employees = diretemployees.ToList();
            //}
            //else
            //{
            //    employees = GetEmployeeLeaders(postId, 0);
            //}
            
        }

        /// <summary>
        /// 提交流程获取直接上级和部门负责人
        /// LeaderType 0 直接上级 1 隔级上级 2 部门负责人
        /// </summary>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeeLeaderForFlow(string employeeID, string postID, int leaderType)
        {
            List<V_EMPLOYEEVIEW> employees = new List<V_EMPLOYEEVIEW>();
            if (leaderType == 0)
            {
                employees = GetDirectEmployeeLeader(employeeID, postID);
            }
            if (leaderType == 1)
            {
                employees = GetInDirectEmployeeLeader(employeeID, postID);
            }
            if (leaderType == 2)
            {
                employees = GetDepartBossHeadLeader(postID);
            }
            return employees;
        }

        /// <summary>
        /// 找员工直接上级
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        private List<V_EMPLOYEEVIEW> GetDirectEmployeeLeader(string employeeId, string postId)
        {
            List<V_EMPLOYEEVIEW> employees = new List<V_EMPLOYEEVIEW>();
            //先查询员工个人档案里查询有没有直接上级
            T_HR_EMPLOYEE employee = dal.GetObjects<T_HR_EMPLOYEE>().Where(emp => emp.EMPLOYEEID == employeeId && emp.FATHEREMPLOYEEID != null && emp.EMPLOYEESTATE != "2").FirstOrDefault();
            if (employee != null)
            {
                if (!string.IsNullOrEmpty(employee.FATHEREMPLOYEEID))
                {
                    //判断直接上级有没有离职
                    T_HR_EMPLOYEE fatherEmployee = dal.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEEID == employee.FATHEREMPLOYEEID && t.EMPLOYEESTATE != "2").FirstOrDefault();
                    if (fatherEmployee != null)
                    {
                        //没有离职，则返回该员工的信息
                        employees = GetEmployeeView(employee.FATHEREMPLOYEEID);
                    }
                    else
                    {
                        //根据岗位找直接上级
                        employees = GetEmployeeDirectLeaderByPostId(postId);
                    }
                }
            }
            else
            {
                //没有则更近岗位找直接上级
                employees = GetEmployeeDirectLeaderByPostId(postId);
            }
            return employees;
        }

        /// <summary>
        /// 找员工隔级上级
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        private List<V_EMPLOYEEVIEW> GetInDirectEmployeeLeader(string employeeId, string postId)
        {
            List<V_EMPLOYEEVIEW> employees = new List<V_EMPLOYEEVIEW>();
            //先查询员工个人档案里查询有没有直接上级
            T_HR_EMPLOYEE employee = dal.GetObjects<T_HR_EMPLOYEE>().Where(emp => emp.EMPLOYEEID == employeeId && emp.FATHEREMPLOYEEID != null && emp.EMPLOYEESTATE != "2").FirstOrDefault();
            if (employee != null)
            {
                if (!string.IsNullOrEmpty(employee.FATHEREMPLOYEEID))
                {
                    //查询员工上级的上级
                    T_HR_EMPLOYEE indirectEmployee = dal.GetObjects<T_HR_EMPLOYEE>().Where(emp => emp.EMPLOYEEID == employee.EMPLOYEEID && emp.FATHEREMPLOYEEID != null && emp.EMPLOYEESTATE != "2").FirstOrDefault();
                    if (indirectEmployee != null)
                    {
                        //没有离职，则返回该员工的信息
                        employees = GetEmployeeView(employee.FATHEREMPLOYEEID);
                    }
                    else
                    {
                        //如果没有上级，则用上级的岗位ID查询直接上级
                        employees = GetEmployeeDirectLeaderByPostId(indirectEmployee.OWNERPOSTID);
                    }
                }
                else
                {
                    //根据岗位找隔级上级
                    employees = GetEmployeeInDirectLeaderByPostId(postId);
                }
            }
            else
            {
                //没有根据岗位找隔级上级
                employees = GetEmployeeInDirectLeaderByPostId(postId);
            }
            return employees;
        }

        private List<V_EMPLOYEEVIEW> GetEmployeeView(string employeeId)
        {
            var employeeView = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                 join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                 join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                 join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                 where em.T_HR_EMPLOYEE.EMPLOYEEID == employeeId && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                 select new V_EMPLOYEEVIEW
                                 {
                                     EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                     EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                     OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                     OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                     OWNERPOSTID = em.T_HR_POST.POSTID,
                                     OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                     COMPANYNAME = c.BRIEFNAME,
                                     DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                     POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                 };
            return employeeView.ToList();
        }

        /// <summary>
        /// 根据岗位找直接上级
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        private List<V_EMPLOYEEVIEW> GetEmployeeDirectLeaderByPostId(string postId)
        {
            List<V_EMPLOYEEVIEW> employees = new List<V_EMPLOYEEVIEW>();
            var posts = from c in dal.GetObjects<T_HR_POST>()
                        where c.POSTID == postId
                        select c;
            if (posts.Count() > 0)
            {
                var post = posts.FirstOrDefault();
                if (!string.IsNullOrEmpty(post.FATHERPOSTID))
                {
                    var directLeaderPost = dal.GetObjects<T_HR_POST>().Where(t => t.POSTID == post.FATHERPOSTID).FirstOrDefault();
                    if (directLeaderPost != null)
                    {
                        var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                             join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                             join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                             join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                             where em.T_HR_POST.POSTID == directLeaderPost.POSTID && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                             select new V_EMPLOYEEVIEW
                                             {
                                                 EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                 EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                 OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                                 OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                                 OWNERPOSTID = em.T_HR_POST.POSTID,
                                                 OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                 COMPANYNAME = c.BRIEFNAME,
                                                 DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                 POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                             };
                        if (diretemployees.Count() > 0)
                        {
                            employees = diretemployees.ToList();
                        }
                        else
                        {
                            employees = null;
                        }
                    }
                }
            }
            return employees;
        }

        /// <summary>
        /// 根据岗位找隔级上级
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        private List<V_EMPLOYEEVIEW> GetEmployeeInDirectLeaderByPostId(string postId)
        {
            List<V_EMPLOYEEVIEW> employees = new List<V_EMPLOYEEVIEW>();
            var posts = from c in dal.GetObjects<T_HR_POST>()
                        where c.POSTID == postId
                        select c;
            if (posts.Count() > 0)
            {
                var post = posts.FirstOrDefault();
                //判断岗位有没有上级
                if (!string.IsNullOrEmpty(post.FATHERPOSTID))
                {
                    var directLeaderPost = dal.GetObjects<T_HR_POST>().Where(t => t.POSTID == post.FATHERPOSTID).FirstOrDefault();
                    if (directLeaderPost != null)
                    {
                        var indirectLeaderPost = dal.GetObjects<T_HR_POST>().Where(t => t.POSTID == directLeaderPost.FATHERPOSTID).FirstOrDefault();
                        //判断岗位的岗位有没有上级
                        if (indirectLeaderPost != null)
                        {
                            var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                 join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                                 join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                                 join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                                 where em.T_HR_POST.POSTID == indirectLeaderPost.POSTID && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                                 select new V_EMPLOYEEVIEW
                                                 {
                                                     EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                     EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                     OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                                     OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                                     OWNERPOSTID = em.T_HR_POST.POSTID,
                                                     OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                                     COMPANYNAME = c.BRIEFNAME,
                                                     DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                     POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                                 };
                            if (diretemployees.Count() > 0)
                            {
                                employees = diretemployees.ToList();
                            }
                            else
                            {
                                employees = null;
                            }
                        }
                        else
                        {
                            employees = null;
                        }
                    }
                }
            }
            return employees;
        }

        /// <summary>
        /// 根据岗位找部门负责人
        /// </summary>
        /// <param name="PostID"></param>
        /// <returns></returns>
        private List<V_EMPLOYEEVIEW> GetDepartBossHeadLeader(string PostID)
        {
            List<V_EMPLOYEEVIEW> employees = new List<V_EMPLOYEEVIEW>();
            #region 找部门负责人
            var departments = from dp in dal.GetObjects<T_HR_DEPARTMENT>()
                              join p in dal.GetObjects<T_HR_POST>() on dp.DEPARTMENTID equals p.T_HR_DEPARTMENT.DEPARTMENTID
                              where p.POSTID == PostID
                              select dp;
            if (departments.Count() > 0)
            {
                var tmp = departments.FirstOrDefault();

                var diretemployees = from eps in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                     join p in dal.GetObjects<T_HR_POST>() on eps.T_HR_POST.POSTID equals p.POSTID
                                     join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                     join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                     where eps.T_HR_POST.POSTID == tmp.DEPARTMENTBOSSHEAD && eps.EDITSTATE == "1"
                                     select new V_EMPLOYEEVIEW
                                     {
                                         EMPLOYEECNAME = eps.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                         EMPLOYEEID = eps.T_HR_EMPLOYEE.EMPLOYEEID,
                                         OWNERCOMPANYID = eps.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                         OWNERDEPARTMENTID = eps.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                         OWNERPOSTID = eps.T_HR_POST.POSTID,
                                         OWNERID = eps.T_HR_EMPLOYEE.EMPLOYEEID,
                                         COMPANYNAME = c.BRIEFNAME,
                                         DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                         POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                     };
                if (diretemployees.Count() > 0)
                {
                    employees = diretemployees.ToList();
                }
                else
                {
                    employees = null;
                }
            }
            else
            {
                employees = null;
            }
            #endregion
            return employees;
        }

        /// <summary>
        /// 获取员工的生效信息概要
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns>员工的姓名，工龄，岗位集合</returns>
        public V_EMPLOYEEDETAIL GetEmployeeDetailView(string employeeID)
        {
            try
            {
                var ent = from em in dal.GetObjects<T_HR_EMPLOYEE>()
                          //join en in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on c.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                          join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                          on em.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                          where ep.EDITSTATE == "1" && ep.CHECKSTATE == "2" && em.EMPLOYEEID == employeeID
                          select new
                          {
                              //employee = em,
                              em.EMPLOYEEID,
                              em.EMPLOYEECNAME,
                              em.EMPLOYEEENAME,
                              em.EMPLOYEECODE,
                              em.EMPLOYEESTATE,
                              em.OFFICEPHONE,
                              em.SEX,
                              //em.PHOTO,
                              em.MOBILE,
                              em.CURRENTADDRESS,
                              ep.EMPLOYEEPOSTID,
                              ep.ISAGENCY,
                              ep.T_HR_POST.POSTID,
                              ep.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                              ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                              ep.T_HR_POST.COMPANYID,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.BRIEFNAME,
                              ep.POSTLEVEL
                          };
                //employeeDetail = vemployee.FirstOrDefault();
                V_EMPLOYEEDETAIL employeeDetail = new V_EMPLOYEEDETAIL();
                employeeDetail.EMPLOYEEPOSTS = new List<V_EMPLOYEEPOSTBRIEF>();
                // employeeDetail.POSTS = new List<string>();
                foreach (var item in ent.ToList())
                {
                    if (item.ISAGENCY == "0")
                    {
                        employeeDetail.EMPLOYEEID = item.EMPLOYEEID;
                        employeeDetail.EMPLOYEENAME = item.EMPLOYEECNAME;
                        employeeDetail.EMPLOYEEENAME = item.EMPLOYEEENAME;
                        employeeDetail.EMPLOYEECODE = item.EMPLOYEECODE;
                        employeeDetail.EMPLOYEESTATE = item.EMPLOYEESTATE;
                        employeeDetail.OFFICEPHONE = item.OFFICEPHONE;
                        employeeDetail.SEX = item.SEX;
                        //PHOTO = e.PHOTO,
                        employeeDetail.WORKAGE = GetEmployeeWorkAgeMonthByID(item.EMPLOYEEID);
                        employeeDetail.MOBILE = item.MOBILE;
                        employeeDetail.CURRENTADDRESS = item.CURRENTADDRESS;
                        employeeDetail.POSTID = item.POSTID;
                    }

                    V_EMPLOYEEPOSTBRIEF tmp = new V_EMPLOYEEPOSTBRIEF();
                    tmp.EMPLOYEEPOSTID = item.EMPLOYEEPOSTID;
                    tmp.ISAGENCY = item.ISAGENCY;
                    tmp.POSTID = item.POSTID;
                    tmp.DepartmentID = item.DEPARTMENTID;
                    tmp.CompanyID = item.COMPANYID;
                    tmp.PostName = item.POSTNAME;
                    tmp.DepartmentName = item.DEPARTMENTNAME;
                    string cname = string.Empty;
                    if (!string.IsNullOrEmpty(item.BRIEFNAME))
                    {
                        tmp.CompanyName = item.BRIEFNAME;
                    }
                    else
                    {
                        tmp.CompanyName = item.CNAME;
                    }
                    tmp.POSTLEVEL = item.POSTLEVEL;
                    employeeDetail.EMPLOYEEPOSTS.Add(tmp);
                }
                //}

                employeeDetail.EMPLOYEEPOSTS = employeeDetail.EMPLOYEEPOSTS.OrderBy(s => s.ISAGENCY).ToList();

                return employeeDetail;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// 获取员工的生效信息概要
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns>员工的姓名，工龄，岗位集合</returns>
        public string GetEmployeeNameById(string employeeID)
        {
            string employeeName = string.Empty;
            try
            {
                var ent = from em in dal.GetObjects<T_HR_EMPLOYEE>()
                          where em.EMPLOYEEID == employeeID
                          select em.EMPLOYEECNAME;
                employeeName = ent.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
            return employeeName;
        }
        /// <summary>
        /// 获取员工的历史信息概要（包括已离职员工）
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns>员工的姓名，工龄，岗位集合</returns>
        public V_EMPLOYEEDETAIL GetEmployeeHistoryDetailView(string employeeID)
        {
            string keyString = "GetEmployeeHistoryDetailView" + employeeID;

            V_EMPLOYEEDETAIL employeeDetail = new V_EMPLOYEEDETAIL();
            try
            {
                //if (CacheManager.GetCache(keyString) == null)
                //{
                //    lock (this)
                //    {
                #region 获取员工历史信息
                var ent = from em in dal.GetObjects<T_HR_EMPLOYEE>()
                          //join en in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on c.EMPLOYEEID equals en.T_HR_EMPLOYEE.EMPLOYEEID
                          join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                          on em.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                          where ep.EDITSTATE == "1" && ep.CHECKSTATE == "2" && em.EMPLOYEEID == employeeID
                          select new
                          {
                              //employee = em,
                              em.EMPLOYEEID,
                              em.EMPLOYEECNAME,
                              em.EMPLOYEEENAME,
                              em.EMPLOYEECODE,
                              em.EMPLOYEESTATE,
                              em.OFFICEPHONE,
                              em.SEX,
                              //em.PHOTO,
                              em.MOBILE,
                              em.CURRENTADDRESS,
                              ep.EMPLOYEEPOSTID,
                              ep.ISAGENCY,
                              ep.T_HR_POST.POSTID,
                              ep.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                              ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                              ep.T_HR_POST.COMPANYID,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.BRIEFNAME,
                              ep.POSTLEVEL
                          };
                if (ent.Count() < 1)
                {
                    ent = from em in dal.GetObjects<T_HR_EMPLOYEE>()
                          join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                          on em.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                          where em.EMPLOYEEID == employeeID
                          select new
                          {
                              em.EMPLOYEEID,
                              em.EMPLOYEECNAME,
                              em.EMPLOYEEENAME,
                              em.EMPLOYEECODE,
                              em.EMPLOYEESTATE,
                              em.OFFICEPHONE,
                              em.SEX,
                              em.MOBILE,
                              em.CURRENTADDRESS,
                              ep.EMPLOYEEPOSTID,
                              ep.ISAGENCY,
                              ep.T_HR_POST.POSTID,
                              ep.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                              ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                              ep.T_HR_POST.COMPANYID,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                              ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.BRIEFNAME,
                              ep.POSTLEVEL
                          };
                }
                employeeDetail.EMPLOYEEPOSTS = new List<V_EMPLOYEEPOSTBRIEF>();
                foreach (var item in ent.ToList())
                {
                    if (item.ISAGENCY == "0")
                    {
                        employeeDetail.EMPLOYEEID = item.EMPLOYEEID;
                        employeeDetail.EMPLOYEENAME = item.EMPLOYEECNAME;
                        employeeDetail.EMPLOYEEENAME = item.EMPLOYEEENAME;
                        employeeDetail.EMPLOYEECODE = item.EMPLOYEECODE;
                        employeeDetail.EMPLOYEESTATE = item.EMPLOYEESTATE;
                        employeeDetail.OFFICEPHONE = item.OFFICEPHONE;
                        employeeDetail.SEX = item.SEX;
                        //PHOTO = e.PHOTO,
                        employeeDetail.MOBILE = item.MOBILE;
                        employeeDetail.CURRENTADDRESS = item.CURRENTADDRESS;
                        employeeDetail.POSTID = item.POSTID;
                    }

                    V_EMPLOYEEPOSTBRIEF tmp = new V_EMPLOYEEPOSTBRIEF();
                    tmp.EMPLOYEEPOSTID = item.EMPLOYEEPOSTID;
                    tmp.ISAGENCY = item.ISAGENCY;
                    tmp.POSTID = item.POSTID;
                    tmp.DepartmentID = item.DEPARTMENTID;
                    tmp.CompanyID = item.COMPANYID;
                    tmp.PostName = item.POSTNAME;
                    tmp.DepartmentName = item.DEPARTMENTNAME;
                    string cname = string.Empty;
                    if (!string.IsNullOrEmpty(item.BRIEFNAME))
                    {
                        tmp.CompanyName = item.BRIEFNAME;
                    }
                    else
                    {
                        tmp.CompanyName = item.CNAME;
                    }
                    tmp.POSTLEVEL = item.POSTLEVEL;
                    employeeDetail.EMPLOYEEPOSTS.Add(tmp);
                }
                //}

                employeeDetail.EMPLOYEEPOSTS = employeeDetail.EMPLOYEEPOSTS.OrderBy(s => s.ISAGENCY).ToList();



                //CacheManager.AddCache(keyString, employeeDetail);
                #endregion
                //}
                return employeeDetail;
                //}
                //else
                //{
                //    employeeDetail = (V_EMPLOYEEDETAIL)CacheManager.GetCache(keyString);
                //}
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.ToString());
            }
            return null;
        }
        /// <summary>
        /// 根据员工ID获取员工照片
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>员工照片，为byte，没有返回null</returns>
        public byte[] GetEmployeePhotoByID(string employeeID)
        {
            var ent = (from em in dal.GetObjects<T_HR_EMPLOYEE>()
                       where em.EMPLOYEEID == employeeID
                       select em).FirstOrDefault();
            byte[] employeePhoto = null;
            if (ent != null && ent.PHOTO != null)
            {
                //employeePhoto = CreateThumbnail(ent.PHOTO, 51, 76);//生成缩略图
                employeePhoto = ent.PHOTO;
            }
            return employeePhoto;

        }

        /// <summary>
        /// 根据员工id获取员工工作时间，以员工入职里面的入职时间计算
        /// </summary>
        /// <param name="employeeID">员工id</param>
        /// <returns>工作时间</returns>
        public int GetEmployeeWorkAgeMonthByID(string employeeID)
        {
            int allMonth = 0;
            var entry = (from en in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                         where en.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && en.CHECKSTATE == "2"
                         select en).ToList().OrderByDescending(c=>c.CREATEDATE).FirstOrDefault();
            if (entry != null && entry.ENTRYDATE != null)
            {
                DateTime entryDate = entry.ENTRYDATE.Value;
                DateTime Nowdate = System.DateTime.Now;
                allMonth=Nowdate.Year * 12 + Nowdate.Month - entryDate.Year * 12 - entryDate.Month;

                if (allMonth>0)
                {
                    return allMonth;
                }
            }
            return allMonth;

        }

        /// <summary>
        /// 根据员工id获取员工工作时间，以员工入职里面的入职时间计算
        /// </summary>
        /// <param name="employeeID">员工id</param>
        /// <returns>工作时间</returns>
        public int GetEmployeeWorkAgeByID(string employeeID)
        {
            int workAge = 0;
            var entry = (from en in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                         where en.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && en.EDITSTATE == "1"
                         select en).FirstOrDefault();
            if (entry != null && entry.ENTRYDATE != null)
            {
                DateTime entryDate = entry.ENTRYDATE.Value;
                DateTime Nowdate = System.DateTime.Now;
                workAge = (Nowdate.Year - entryDate.Year) * 12 + (Nowdate.Month - entryDate.Month - 1);
                if (Nowdate.Day >= entryDate.Day)
                {
                    workAge += 1;
                }
            }
            return workAge;

        }
        //        /// <summary>
        //        /// 通过视图获取员工的信息概要
        //        /// </summary>
        //        /// <param name="employeeID"></param>
        //        /// <returns>员工的姓名，岗位集合</returns>
        //        public V_EMPLOYEEDETAIL GetEmployeeDetailView(string employeeID)
        //        {
        //            string str = @"select * from V_EMPLOYEEDETAIL where EDITSTATE = '1'
        //                        && ep.CHECKSTATE = '2' && em.EMPLOYEEID = '" + employeeID + "'";
        //            DataTable dt = dal.CustomerQuery(str) as DataTable;

        //            V_EMPLOYEEDETAIL employeeDetail = new V_EMPLOYEEDETAIL();
        //            if (dt != null && dt.Rows.Count > 0)
        //            {
        //                employeeDetail.EMPLOYEEID = dt.Rows[0]["EMPLOYEEID"].ToString();
        //                employeeDetail.EMPLOYEENAME = dt.Rows[0]["EMPLOYEECNAME"].ToString();
        //                employeeDetail.EMPLOYEEENAME = dt.Rows[0]["EMPLOYEEENAME"].ToString();
        //                employeeDetail.EMPLOYEESTATE = dt.Rows[0]["EMPLOYEESTATE"].ToString();
        //                employeeDetail.CURRENTADDRESS = dt.Rows[0]["CURRENTADDRESS"].ToString();
        //                employeeDetail.MOBILE = dt.Rows[0]["MOBILE"].ToString();
        //                employeeDetail.OFFICEPHONE = dt.Rows[0]["OFFICEPHONE"].ToString();
        //                employeeDetail.SEX = dt.Rows[0]["SEX"].ToString();
        //                employeeDetail.EMPLOYEEPOSTS = new List<V_EMPLOYEEPOSTBRIEF>();
        //                decimal value;
        //                foreach (DataRow item in dt.Rows)
        //                {
        //                    if (item["ISAGENCY"].ToString() == "0")
        //                    {
        //                        //主岗位
        //                        employeeDetail.POSTID = item["POSTID"].ToString();
        //                    }
        //                    V_EMPLOYEEPOSTBRIEF tmp = new V_EMPLOYEEPOSTBRIEF();
        //                    tmp.EMPLOYEEPOSTID = item["EMPLOYEEPOSTID"].ToString();
        //                    tmp.ISAGENCY = item["ISAGENCY"].ToString();
        //                    tmp.POSTID = item["POSTID"].ToString();
        //                    tmp.DepartmentID = item["DEPARTMENTID"].ToString();
        //                    tmp.CompanyID = item["COMPANYID"].ToString();
        //                    tmp.PostName = item["EMPLOYEEPOSTID"].ToString();
        //                    tmp.DepartmentName = item["DEPARTMENTNAME"].ToString();
        //                    tmp.CompanyName = item["CNAME"].ToString();
        //                    decimal.TryParse(item["POSTLEVEL"].ToString(), out value);
        //                    tmp.POSTLEVEL = value;
        //                    employeeDetail.EMPLOYEEPOSTS.Add(tmp);
        //                }
        //            }
        //            return employeeDetail;
        //        }
        /// <summary>
        /// 生成缩略图纯数据
        /// </summary>
        /// <param name="data1">数据1</param>
        /// <param name="data2">数据2</param>
        /// <param name="LimitW">限宽</param>
        /// <param name="LimitH">限高</param>
        public byte[] CreateThumbnail(byte[] data1, double LimitW, double LimitH)
        {
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(data1)) as System.Drawing.Bitmap;
                System.Drawing.SizeF size = new System.Drawing.SizeF(image.Width, image.Height);
                size.Width = (float)LimitW;
                size.Height = (float)LimitH;
                if (size.Height <= 0)
                {
                    size.Height = image.Height * size.Width / image.Width;
                }
                if (size.Width <= 0)
                {
                    size.Width = image.Width * size.Height / image.Height;
                }
                System.Drawing.Image bitmap = new System.Drawing.Bitmap(Convert.ToInt16(size.Width), Convert.ToInt16(size.Height));
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                g.DrawImage(image, rect, new System.Drawing.Rectangle(0, 0, image.Width, image.Height), System.Drawing.GraphicsUnit.Pixel);
                ImageCodecInfo myImageCodecInfo;
                System.Drawing.Imaging.Encoder myEncoder;
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;
                myImageCodecInfo = ImageCodecInfo.GetImageEncoders()[0];
                myEncoder = System.Drawing.Imaging.Encoder.Quality;
                myEncoderParameters = new EncoderParameters(1);
                myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, myImageCodecInfo, myEncoderParameters);
                byte[] data2 = ms.ToArray();
                myEncoderParameter.Dispose();
                myEncoderParameters.Dispose();
                image.Dispose();
                bitmap.Dispose();
                g.Dispose();
                ms.Dispose();
                return data2;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.ToString());
                return null;
            }
        }



        /// <summary>
        /// 根据岗位ID和考勤结算获取员工
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEE> GetEmployeeByPostIDandAttBalance(string postID, int year, int month)
        {
            var employees = from c in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>()
                            join b in dal.GetObjects<T_HR_EMPLOYEE>() on c.EMPLOYEEID equals b.EMPLOYEEID into tmp
                            from b in tmp.DefaultIfEmpty()
                            where c.OWNERPOSTID == postID && c.BALANCEYEAR == year && c.BALANCEMONTH == month
                            select b;
            return employees.Count() > 0 ? employees.ToList() : null;
        }

        /// <summary>
        /// 根据员工身份证获取员工信息
        /// </summary>
        /// <param name="idnumbers"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeesByIdnumber(string[] idnumbers)
        {
            if (idnumbers.Count() > 0)
            {
                StringBuilder idnumbersb = new StringBuilder();
                for (int i = 0; i < idnumbers.Length; i++)
                {
                    if (i == 0 || i == idnumbers.Length - 1)
                    {
                        idnumbersb.Append(idnumbers[0]);
                    }
                    else idnumbersb.Append(idnumbers[i] + ",");
                }
                string IDs = idnumbersb.ToString();
                var employees = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                where c.IDNUMBER.Contains(IDs)
                                select new V_EMPLOYEEVIEW
                                {
                                    EMPLOYEEID = c.EMPLOYEEID,
                                    EMPLOYEECNAME = c.EMPLOYEECNAME,
                                    EMPLOYEEENAME = c.EMPLOYEEENAME,
                                    SEX = c.SEX,
                                    EMAIL = c.EMAIL,
                                    IDNUMBER = c.IDNUMBER,
                                    FINGERPRINTID = c.FINGERPRINTID,
                                    MOBILE = c.MOBILE,
                                    OWNERID = c.OWNERID,
                                    OWNERCOMPANYID = c.OWNERCOMPANYID,
                                    OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                                    OWNERPOSTID = c.OWNERPOSTID,
                                    CREATEUSERID = c.CREATEUSERID
                                };
                return employees.Count() > 0 ? employees.ToList() : null;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 根据员工ID集合获取员工视图
        /// </summary>
        /// <param name="Employeeids"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetEmployeesByEmployeeIds(string[] Employeeids)
        {
            List<V_EMPLOYEEVIEW> LstViews = new List<V_EMPLOYEEVIEW>();
            string StrMessage = "";//记录日志信息
            try
            {


                if (Employeeids.Any())
                {
                    //StringBuilder idnumber = new StringBuilder();
                    //for (int i = 0; i < Employeeids.Length; i++)
                    //{
                    //    if (i == 0 || i == Employeeids.Length - 1)
                    //    {
                    //        idnumber.Append(Employeeids[0]);
                    //    }
                    //    else idnumber.Append(Employeeids[i] + ",");
                    //}
                    //string IDs = idnumber.ToString();
                    var employees = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                    join e in Employeeids on c.EMPLOYEEID equals e
                                    // where c.EMPLOYEEID.Contains(IDs)
                                    select new V_EMPLOYEEVIEW
                                    {
                                        EMPLOYEEID = c.EMPLOYEEID,
                                        EMPLOYEECNAME = c.EMPLOYEECNAME,
                                        EMPLOYEEENAME = c.EMPLOYEEENAME,
                                        SEX = c.SEX,
                                        EMAIL = c.EMAIL,
                                        IDNUMBER = c.IDNUMBER,
                                        FINGERPRINTID = c.FINGERPRINTID,
                                        MOBILE = c.MOBILE,
                                        OWNERID = c.OWNERID,
                                        OWNERCOMPANYID = c.OWNERCOMPANYID,
                                        OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                                        OWNERPOSTID = c.OWNERPOSTID,
                                        CREATEUSERID = c.CREATEUSERID
                                    };
                    if (employees.Count() > 0)
                    {
                        LstViews = employees.ToList();
                    }

                }

            }
            catch (Exception ex)
            {
                StrMessage = "获取员工信息出错：" + ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(StrMessage);
            }
            return LstViews;
        }

        /// <summary>
        /// 通讯录
        /// </summary>
        /// <returns></returns>
        public byte[] ExportEmployeeContract()
        {
            DataTable dt = new DataTable();

            DataColumn colCordSD = new DataColumn();
            colCordSD.ColumnName = Utility.GetResourceStr("姓名");
            colCordSD.DataType = typeof(string);
            dt.Columns.Add(colCordSD);

            DataColumn colCordED = new DataColumn();
            colCordED.ColumnName = Utility.GetResourceStr("业务电话");
            colCordED.DataType = typeof(string);
            dt.Columns.Add(colCordED);

            DataColumn colCordBank = new DataColumn();
            colCordBank.ColumnName = Utility.GetResourceStr("移动电话");
            colCordBank.DataType = typeof(string);
            dt.Columns.Add(colCordBank);

            DataColumn colCordLevel = new DataColumn();
            colCordLevel.ColumnName = Utility.GetResourceStr("电子邮件地址");
            colCordLevel.DataType = typeof(string);
            dt.Columns.Add(colCordLevel);


            var ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                       join b in dal.GetObjects<T_HR_COMPANY>() on a.OWNERCOMPANYID equals b.COMPANYID
                       join c in dal.GetObjects<T_HR_DEPARTMENT>() on a.OWNERDEPARTMENTID equals c.DEPARTMENTID
                       join d in dal.GetObjects<T_HR_POST>() on a.OWNERPOSTID equals d.POSTID
                       where a.EMPLOYEESTATE != "2" && a.OWNERCOMPANYID != "cafdca8a-c630-4475-a65d-490d052dca36" && a.OWNERCOMPANYID != "7cd6c0a4-9735-476a-9184-103b962d3383"
                       orderby b.CNAME
                       select new
                       {
                           EMPLOYEENAME = b.CNAME + " - " + c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + " - " + d.T_HR_POSTDICTIONARY.POSTNAME + " - " + a.EMPLOYEECNAME,
                           OFFICEPHONE = a.OFFICEPHONE,
                           MOBLIE = a.MOBILE,
                           EMAIL = a.EMAIL
                       };
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    DataRow row = dt.NewRow();
                    row[0] = ent.EMPLOYEENAME;
                    row[1] = ent.OFFICEPHONE;
                    row[2] = ent.MOBLIE;
                    row[3] = ent.EMAIL;
                    dt.Rows.Add(row);
                }
                //  byte[] result = Utility.OutFileStream(Utility.GetResourceStr("通讯录"), dt);
                //  byte[] result = 
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.Append(dt.Columns[i].ColumnName + ",");
                }
                sb.Append("\r\n");
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sb.Append(dt.Rows[j][i] + ",");
                    }
                    sb.Append("\r\n");
                }
                byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
                return result;
            }
            else
            {
                return null;
            }
        }
        public List<V_EMPLOYEESTATICINFO> GetEmployeesIntime(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
            //int workAge = (Nowdate.Year - entryDate.Year) * 12 + (Nowdate.Month - entryDate.Month - 1);
            //if (Nowdate.Day >= entryDate.Day)
            //{
            //    workAge += 1;
            //}

            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                       join b in dal.GetObjects<T_HR_POST>() on a.T_HR_POST.POSTID equals b.POSTID
                       join c in dal.GetObjects<T_HR_DEPARTMENT>() on b.T_HR_DEPARTMENT.DEPARTMENTID equals c.DEPARTMENTID
                       join d in dal.GetObjects<T_HR_COMPANY>() on c.T_HR_COMPANY.COMPANYID equals d.COMPANYID
                       //join e in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on new { a.EMPLOYEEPOSTID, a.T_HR_EMPLOYEE.EMPLOYEEID } equals new { e.EMPLOYEEPOSTID, e.T_HR_EMPLOYEE.EMPLOYEEID } into g
                       join e in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on a.T_HR_EMPLOYEE.EMPLOYEEID equals e.T_HR_EMPLOYEE.EMPLOYEEID into g
                       from f in g.DefaultIfEmpty()
                       where a.EDITSTATE == "1" && f.EDITSTATE != "2"
                       select new V_EMPLOYEESTATICINFO
                       {
                           EmployeeName = a.T_HR_EMPLOYEE.EMPLOYEECNAME,
                           EmployeeState = a.T_HR_EMPLOYEE.EMPLOYEESTATE,
                           EntryDate = f.ENTRYDATE,
                           BirthDay = a.T_HR_EMPLOYEE.BIRTHDAY,
                           PostName = b.T_HR_POSTDICTIONARY.POSTNAME,
                           OWNERPOSTID = b.POSTID,
                           OWNERDEPARTMENTID = c.DEPARTMENTID,
                           DepartmentName = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                           OWNERCOMPANYID = d.COMPANYID,
                           CName = d.CNAME,
                           Sex = a.T_HR_EMPLOYEE.SEX == "0" ? "女" : "男",
                           Nation = a.T_HR_EMPLOYEE.NATION,
                           Education = a.T_HR_EMPLOYEE.TOPEDUCATION,
                           Age = a.T_HR_EMPLOYEE.BIRTHDAY == null ? 0 : (DateTime.Now.Year - a.T_HR_EMPLOYEE.BIRTHDAY.Value.Year),
                           WorkAge = f == null ? 0 : (DateTime.Now > f.ENTRYDATE ? ((DateTime.Now.Year - f.ENTRYDATE.Value.Year) * 12 + (DateTime.Now.Month - f.ENTRYDATE.Value.Month - 1) + 1) : ((DateTime.Now.Year - f.ENTRYDATE.Value.Year) * 12 + (DateTime.Now.Month - f.ENTRYDATE.Value.Month - 1))),
                           PostChangeType = a.ISAGENCY == "0" ? ((f == null || f.EMPLOYEEPOSTID != a.EMPLOYEEPOSTID) ? "异动" : "入职") : "",
                           FamilyAddress = a.T_HR_EMPLOYEE.FAMILYADDRESS,
                           IDNumber = a.T_HR_EMPLOYEE.IDNUMBER,
                           OWNERID = a.T_HR_EMPLOYEE.EMPLOYEEID,
                           CREATEUSERID = a.T_HR_EMPLOYEE.CREATEUSERID,
                           IsAgency = a.ISAGENCY

                       };
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            return ents.Count() > 0 ? ents.ToList() : null;
        }
        public byte[] ExportEmployeesIntime(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            try
            {
                List<V_EMPLOYEESTATICINFO> employeeInfos = GetEmployeesIntime(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
                if (employeeInfos != null)
                {
                    List<string> colName = new List<string>();
                    colName.Add("部门名称");
                    colName.Add("岗位");
                    colName.Add("员工姓名");
                    colName.Add("入职时间");
                    colName.Add("性别");
                    colName.Add("民族");
                    colName.Add("学历");
                    colName.Add("出生年月");
                    colName.Add("年龄");
                    colName.Add("服务时间");
                    colName.Add("员工状态");
                    colName.Add("异动类型");

                    //var tmp = new PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "EMPLOYEESTATE", "TOPEDUCATION", "NATION" });
                    List<T_SYS_DICTIONARY> tmp = new List<T_SYS_DICTIONARY>();// new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "LEAVETYPEVALUE" });
                    using (SysDictionaryBLL bll = new SysDictionaryBLL())
                    {
                        tmp = bll.GetSysDictionaryByCategory(new List<string> { "EMPLOYEESTATE", "TOPEDUCATION", "NATION" });
                    }
                    //Dictionary<string, string> nations = new Dictionary<string, string>();
                    //nations.Add("0", "汉族");
                    //nations.Add("1", "苗族");
                    //nations.Add("2", "土家族");
                    //nations.Add("3", "傣族");
                    //nations.Add("4", "壮族");

                    //Dictionary<string, string> EmployeeStates = new Dictionary<string, string>();
                    //EmployeeStates.Add("0", "在职");
                    //EmployeeStates.Add("1", "试用期");
                    //EmployeeStates.Add("2", "离职");



                    //Dictionary<string, string> Educations = new Dictionary<string, string>();
                    //Educations.Add("7", "院士及以上");
                    //Educations.Add("0", "小学及以下");
                    //Educations.Add("1", "初中");
                    //Educations.Add("2", "高中\\中专");
                    //Educations.Add("3", "大专");
                    //Educations.Add("5", "硕士");
                    //Educations.Add("4", "本科");

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < colName.Count; i++)
                    {
                        sb.Append(colName[i] + ",");
                    }
                    sb.Append("\r\n"); // 列头

                    //内容
                    foreach (var employeeinfo in employeeInfos)
                    {
                        decimal nationValue;
                        decimal employeeStateValue;
                        decimal educateValue;
                        T_SYS_DICTIONARY nationDict = null;
                        T_SYS_DICTIONARY employeeStateDict = null;
                        T_SYS_DICTIONARY educateDict = null;
                        if (decimal.TryParse(employeeinfo.Nation, out nationValue))
                        {
                            nationDict = tmp.Where(s => s.DICTIONCATEGORY == "NATION" && s.DICTIONARYVALUE == nationValue).FirstOrDefault();
                        }
                        if (decimal.TryParse(employeeinfo.EmployeeState, out employeeStateValue))
                        {
                            employeeStateDict = tmp.Where(s => s.DICTIONCATEGORY == "EMPLOYEESTATE" && s.DICTIONARYVALUE == employeeStateValue).FirstOrDefault();
                        }
                        if (decimal.TryParse(employeeinfo.Education, out educateValue))
                        {
                            educateDict = tmp.Where(s => s.DICTIONCATEGORY == "TOPEDUCATION" && s.DICTIONARYVALUE == educateValue).FirstOrDefault();
                        }
                        string strEployeeState = employeeStateDict != null ? employeeStateDict.DICTIONARYNAME : "";
                        sb.Append(employeeinfo.DepartmentName + ",");
                        sb.Append(employeeinfo.PostName + ",");
                        sb.Append(employeeinfo.EmployeeName + ",");
                        sb.Append(employeeinfo.EntryDate.Value.ToShortDateString() + ",");
                        sb.Append(employeeinfo.Sex + ",");
                        sb.Append((nationDict != null ? nationDict.DICTIONARYNAME : "") + ",");
                        sb.Append((educateDict != null ? educateDict.DICTIONARYNAME : "") + ",");
                        sb.Append((employeeinfo.BirthDay != null ? employeeinfo.BirthDay.Value.ToShortDateString() : "") + ",");
                        sb.Append((employeeinfo.Age != null ? employeeinfo.Age.Value.ToString() : "") + ",");//年龄
                        sb.Append(employeeinfo.WorkAge + ",");//服务时间----
                        sb.Append((employeeinfo.IsAgency == "0" ? strEployeeState : strEployeeState + "(兼职)") + ",");//员工状态
                        sb.Append(employeeinfo.PostChangeType + ",");//异动类型
                        sb.Append("\r\n");
                    }
                    byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
                    return result;

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeesIntime:" + ex.Message);
                return null;
            }
        }

        public List<V_EMPLOYEESTATICINFO> GetEmployeesRoster(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
            //int workAge = (Nowdate.Year - entryDate.Year) * 12 + (Nowdate.Month - entryDate.Month - 1);
            //if (Nowdate.Day >= entryDate.Day)
            //{
            //    workAge += 1;
            //}

            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                       join b in dal.GetObjects<T_HR_POST>() on a.T_HR_POST.POSTID equals b.POSTID
                       join c in dal.GetObjects<T_HR_DEPARTMENT>() on b.T_HR_DEPARTMENT.DEPARTMENTID equals c.DEPARTMENTID
                       join d in dal.GetObjects<T_HR_COMPANY>() on c.T_HR_COMPANY.COMPANYID equals d.COMPANYID
                       join e in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on a.T_HR_EMPLOYEE.EMPLOYEEID equals e.T_HR_EMPLOYEE.EMPLOYEEID into g
                       from f in g.DefaultIfEmpty()
                       join i in dal.GetObjects<T_HR_EMPLOYEEPOSTCHANGE>() on a.EMPLOYEEPOSTID equals i.EMPLOYEEPOSTID into h
                       from j in h.DefaultIfEmpty()
                       select new V_EMPLOYEESTATICINFO
                       {
                           EmployeeName = a.T_HR_EMPLOYEE.EMPLOYEECNAME,
                           EmployeeState = a.T_HR_EMPLOYEE.EMPLOYEESTATE,
                           EntryDate = f.ENTRYDATE,
                           BirthDay = a.T_HR_EMPLOYEE.BIRTHDAY,
                           PostName = b.T_HR_POSTDICTIONARY.POSTNAME,
                           OWNERPOSTID = b.POSTID,
                           OWNERDEPARTMENTID = c.DEPARTMENTID,
                           DepartmentName = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                           OWNERCOMPANYID = d.COMPANYID,
                           CName = d.CNAME,
                           Sex = a.T_HR_EMPLOYEE.SEX == "0" ? "女" : "男",
                           Nation = a.T_HR_EMPLOYEE.NATION,
                           Education = a.T_HR_EMPLOYEE.TOPEDUCATION,
                           Age = a.T_HR_EMPLOYEE.BIRTHDAY == null ? 0 : (DateTime.Now.Year - a.T_HR_EMPLOYEE.BIRTHDAY.Value.Year),
                           WorkAge = f == null ? 0 : (DateTime.Now > f.ENTRYDATE ? ((DateTime.Now.Year - f.ENTRYDATE.Value.Year) * 12 + (DateTime.Now.Month - f.ENTRYDATE.Value.Month - 1) + 1) : ((DateTime.Now.Year - f.ENTRYDATE.Value.Year) * 12 + (DateTime.Now.Month - f.ENTRYDATE.Value.Month - 1))),
                           PostChangeType = a.ISAGENCY == "0" ? ((f == null || f.EMPLOYEEPOSTID != a.EMPLOYEEPOSTID) ? "异动" : "入职") : "",
                           FamilyAddress = a.T_HR_EMPLOYEE.FAMILYADDRESS,
                           IDNumber = a.T_HR_EMPLOYEE.IDNUMBER,
                           OWNERID = a.T_HR_EMPLOYEE.EMPLOYEEID,
                           CREATEUSERID = a.T_HR_EMPLOYEE.CREATEUSERID

                       };
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        public IQueryable<T_HR_EMPLOYEE> GetEmployeeContracts(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
            //员工入职审核通过再显示。

            ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
            ///
            var ents = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                       join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                       join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                       join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                       join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                       select new
                       {
                           EMPLOYEE = o,
                           POSTID = p.POSTID,
                           DEPARTMENTID = d.DEPARTMENTID,
                           COMPANYID = c.COMPANYID,
                           CREATEUSERID = o.CREATEUSERID,
                           OWNERCOMPANYID = o.OWNERCOMPANYID,
                           OWNERPOSTID = o.OWNERPOSTID,
                           OWNERID = o.OWNERID,
                           OWNERDEPARTMENT = o.OWNERDEPARTMENTID
                       };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            IQueryable<T_HR_EMPLOYEE> ent = from n in ents
                                            select n.EMPLOYEE;
            ent = ent.OrderBy(sort);

            ent = Utility.Pager<T_HR_EMPLOYEE>(ent, pageIndex, pageSize, ref pageCount);
            return ent;

        }

        public IQueryable<EmployeeContactWays> GetEmployeeContractsList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            var ents = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                       join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                       join d in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals d.POSTID
                       join e in dal.GetObjects<T_HR_DEPARTMENT>() on d.T_HR_DEPARTMENT.DEPARTMENTID equals e.DEPARTMENTID
                       join f in dal.GetObjects<T_HR_COMPANY>() on e.T_HR_COMPANY.COMPANYID equals f.COMPANYID
                       where b.EDITSTATE == "1"
                       select new EmployeeContactWays
                       {
                           EMPLOYEEID = c.EMPLOYEEID,
                           EMPLOYEENAME = c.EMPLOYEECNAME,
                           EMPLOYEEENAME = c.EMPLOYEEENAME,
                           EMPLOYEECODE = c.EMPLOYEECODE,
                           SEX = c.SEX,
                           OFFICEPHONE = c.OFFICEPHONE,
                           TELPHONE = c.MOBILE,
                           MAILADDRESS = c.EMAIL,
                           RTX = c.OTHERCOMMUNICATE,
                           PHOTO = c.PHOTO,
                           URGENCYPERSON = c.URGENCYPERSON,
                           URGENCYCONTACT = c.URGENCYCONTACT,
                           CURRENTADDRESS = c.CURRENTADDRESS,
                           COMPANYID = f.COMPANYID,
                           COMPANYNAME = f.CNAME,
                           POSTID = d.POSTID,
                           POSTNAME = d.T_HR_POSTDICTIONARY.POSTNAME,
                           DEPARTMENTID = e.DEPARTMENTID,
                           DEPARTMENTNAME = e.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                       };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<EmployeeContactWays>(ents, pageIndex, pageSize, ref pageCount);
            return ents;

        }
        #endregion

        #region 调用即时通讯接口
        private void UpdateInstantMessage(T_HR_EMPLOYEE Info)
        {
            string StrMessage = "";
            try
            {
                string UserId = "";
                string UserName = "";// 登录帐号
                string NickName = "";// 员工姓名
                string Gender = "";//性别(填 男 或 女)
                string Telephone = "";// 电话
                string Mobile = "";// 手机
                string Email = "";// 邮箱
                string Address = "";// 地址
                string Sort = "0";// 排序

                DataSyncServiceClient IMCient = new DataSyncServiceClient();
                UserId = Info.EMPLOYEEID;
                UserName = Info.EMPLOYEEENAME;
                NickName = Info.EMPLOYEECNAME;
                Telephone = Info.OFFICEPHONE;
                Mobile = Info.MOBILE;
                Email = Info.EMAIL;
                Address = Info.CURRENTADDRESS;
                if (Info.SEX == "0")
                {
                    Gender = "女";
                }
                if (Info.SEX == "1")
                {
                    Gender = "男";
                }

                SMT.Foundation.Log.Tracer.Debug("修改员工信息时开始调用即时通讯接口：" + NickName);
                StrMessage = IMCient.UpdateEmployeeBaseInfo(UserId, UserName, NickName, Gender, Telephone, Mobile, Email, Address, Sort);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("修改员工信息获取即时通讯接口发生错误" + ex.ToString());
            }
            SMT.Foundation.Log.Tracer.Debug("修改员工信息获取即时通讯接口结果：" + StrMessage);
            //AddOrUpdateImUser
        }
        #endregion

        #region 外包组调用的接口--根据截止时间查在岗人数
        /// <summary>
        /// 根据截止时间查在岗人数
        /// </summary>
        /// <param name="strOwnerID">登录员工ID</param>
        /// <param name="strOrgType">机构类型</param>
        /// <param name="strOrgID">机构ID</param>
        /// <param name="dtCheckDate">截止时间</param>
        /// <param name="strMsg">返回处理消息</param>
        /// <returns></returns>
        public int GetInserviceEmployeeCount(string strOwnerID, string strOrgType, string strOrgID, DateTime dtCheckDate, ref string strMsg)
        {
            int iRes = 0;
            try
            {
                List<object> queryParas = new List<object>();
                string filterString = string.Empty;

                if (string.IsNullOrWhiteSpace(strOwnerID))
                {
                    strMsg = "根据截至时间查在岗人数函数执行出错，该函数接收的传递参数如下：strOwnerID为空，此参数的值不可为空，否则禁止查询在职人数";
                    Utility.SaveLog(strMsg);
                    return iRes;
                }

                if (string.IsNullOrWhiteSpace(strOrgType) || string.IsNullOrWhiteSpace(strOrgID))
                {
                    strMsg = "根据截至时间查在岗人数函数执行出错，该函数接收的传递参数如下：strOrgType, strOrgID的值不可为空，否则禁止查询在职人数";
                    Utility.SaveLog(strMsg);
                    return iRes;
                }

                if (dtCheckDate == null)
                {
                    strMsg = "根据截至时间查在岗人数函数执行出错，该函数接收的传递参数如下：dtCheckDate的值不可为空，否则禁止查询在职人数";
                    Utility.SaveLog(strMsg);
                    return iRes;
                }

                SetOrganizationFilter(ref filterString, ref queryParas, strOwnerID, "T_HR_EMPLOYEE");

                StringBuilder strInserviceTotalSql = new StringBuilder();
                StringBuilder strDimissionTotalSql = new StringBuilder();

                if (string.IsNullOrWhiteSpace(filterString) || queryParas.Count() == 0)
                {
                    strMsg = "根据截至时间查在岗人数函数执行返回空行，当前登录人员无员工个人档案查询权限，禁止查询在职人数";
                    Utility.SaveLog(strMsg);
                    return iRes;
                }

                filterString = filterString.Replace("==", "=");

                if (filterString.Contains("CREATEUSERID"))
                {
                    filterString = filterString.Replace("CREATEUSERID", "OWNERID");
                }

                if (filterString.Contains("OWNERCOMPANYID"))
                {
                    filterString = filterString.Replace("OWNERCOMPANYID", "OWNERCOMPANYID");
                }

                if (filterString.Contains("OWNERDEPARTMENTID"))
                {
                    filterString = filterString.Replace("OWNERDEPARTMENTID", "OWNERDEPARTMENTID");
                }

                if (filterString.Contains("OWNERPOSTID"))
                {
                    filterString = filterString.Replace("OWNERPOSTID", "OWNERPOSTID");
                }

                if (filterString.Contains("OWNERID"))
                {
                    filterString = filterString.Replace("OWNERID", "OWNERID");
                }


                filterString = filterString.ToLower().Trim();

                if (!filterString.StartsWith("and"))
                {
                    filterString = " and " + filterString;
                }

                for (int i = queryParas.Count() - 1; i >= 0; i--)
                {
                    filterString = filterString.Replace("@" + i.ToString(), "'" + queryParas[i].ToString() + "'");
                }

                switch (strOrgType)
                {
                    case "1":
                        filterString += " and p.ownercompanyid = ('" + strOrgID + "') ";
                        break;
                    case "2":
                        filterString += " and p.ownerdepartmentid = ('" + strOrgID + "') ";
                        break;
                }

                filterString += " and p.createdate <= to_date('" + dtCheckDate.ToString() + "', 'yyyy-MM-dd hh24:mi:ss') ";

                strInserviceTotalSql.Append(" select count(*) from t_hr_employeechangehistory p where p.Formtype = '0'");
                strInserviceTotalSql.Append(filterString);

                strDimissionTotalSql.Append(" select count(*) from t_hr_employeechangehistory p where p.Formtype = '2'");
                strDimissionTotalSql.Append(filterString);

                object objInserviceTotal = dal.ExecuteCustomerSql(strInserviceTotalSql.ToString());
                object objDimissionTotal = dal.ExecuteCustomerSql(strInserviceTotalSql.ToString());

                int iInservice = 0, iDimission = 0;
                if (objInserviceTotal != null)
                {
                    int.TryParse(objInserviceTotal.ToString(), out iInservice);
                }

                if (objDimissionTotal != null)
                {
                    int.TryParse(objDimissionTotal.ToString(), out iDimission);
                }

                iRes = iInservice - iDimission;
                return iRes;
            }
            catch (Exception ex)
            {
                strMsg = "查询数据出错，请检查传递的参数是否有问题";
                if (dtCheckDate == null)
                {
                    Utility.SaveLog("根据截至时间查在岗人数函数执行出错，该函数接收的传递参数如下：strOwnerID:" + strOwnerID + ", strOrgType:" + strOrgType
                                       + ", strOrgID:" + strOrgID + ", dtCheckDate: null。 出错信息为：" + ex.ToString());
                }
                else
                {
                    Utility.SaveLog("根据截至时间查在岗人数函数执行出错，该函数接收的传递参数如下：strOwnerID:" + strOwnerID + ",strOrgType:" + strOrgType
                                       + ", strOrgID:" + strOrgID + ", dtCheckDate: " + dtCheckDate.ToString() + "。 出错信息为：" + ex.ToString());
                }

            }
            return iRes;
        }
        #endregion

        public List<V_EMPLOYEEVIEW> GetAllEmployeeLeaderForWP(string employeeId, string postId)
        {
            List<V_EMPLOYEEVIEW> employeeList = new List<V_EMPLOYEEVIEW>();
            var postLeader = dal.GetObjects<T_HR_POST>().Where(t => t.POSTID == postId).FirstOrDefault();
            GetAllEmployeeLeaderForSub(postLeader.FATHERPOSTID, ref employeeList);
            return employeeList;
        }

        private bool GetAllEmployeeLeaderForSub(string postId, ref List<V_EMPLOYEEVIEW> employeeList)
        {
            var postLeader = dal.GetObjects<T_HR_POST>().Where(t => t.POSTID == postId).FirstOrDefault();
            if (postLeader != null)
            {
                if (string.IsNullOrEmpty(postLeader.FATHERPOSTID))
                    return false;
                
                var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                     join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                     join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                     join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                     where em.T_HR_POST.POSTID == postId && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                     select new V_EMPLOYEEVIEW
                                     {
                                         EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                         EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                         OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                         OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                         OWNERPOSTID = em.T_HR_POST.POSTID,
                                         OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                         COMPANYNAME = c.BRIEFNAME,
                                         DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                         POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                         POSTLEVEL = em.POSTLEVEL
                                     };
                if (diretemployees != null && diretemployees.Any())
                {
                    //V_EMPLOYEEVIEW v = diretemployees.FirstOrDefault();
                    employeeList.AddRange(diretemployees);
                }
                GetAllEmployeeLeaderForSub(postLeader.FATHERPOSTID, ref employeeList);
                
            }
            else
            {
                var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                     join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                     join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                     join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                     where em.T_HR_POST.POSTID == postId && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                     select new V_EMPLOYEEVIEW
                                     {
                                         EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                         EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                         OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                         OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                         OWNERPOSTID = em.T_HR_POST.POSTID,
                                         OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                         COMPANYNAME = c.BRIEFNAME,
                                         DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                         POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                         POSTLEVEL = em.POSTLEVEL
                                     };
                if (diretemployees != null && diretemployees.Any())
                {
                    //V_EMPLOYEEVIEW v = diretemployees.FirstOrDefault();
                    employeeList.AddRange(diretemployees);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据岗位获取部门负责人所在的部门信息，提供给工作计划调用
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public List<SMT.HRM.CustomModel.V_DEPARTMENT> GetDepartmentBossHeadList(string postId)
        {
            IQueryable<SMT.HRM.CustomModel.V_DEPARTMENT> ents = from s in dal.GetObjects<T_HR_DEPARTMENT>()
                                                                .Include("T_HR_DEPARTMENTDICTIONARY")
                                                                .Where(t => t.DEPARTMENTBOSSHEAD == postId && t.CHECKSTATE == "2" && t.EDITSTATE == "1")
                                                                select new SMT.HRM.CustomModel.V_DEPARTMENT
                                                                {
                                                                    DEPARTMENTID = s.DEPARTMENTID,
                                                                    DEPARTMENTNAME = s.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                                                                };
            return ents.ToList();
        }

        public List<V_EMPLOYEEVIEW> GetTaskLeaderForWP(string employeeId, string postId, SMT.HRM.CustomModel.Common.Enums.TaskType taskType,string employeeId2,string postId2)
        {
            if (taskType == CustomModel.Common.Enums.TaskType.TaskLower)
            {
                return GetTaskLeaderForWPLower(employeeId, postId);
            }
            if (taskType == CustomModel.Common.Enums.TaskType.TaskUpper)
            {
                return GetTaskLeaderForWpUpper(employeeId, postId);
            }
            if (taskType == CustomModel.Common.Enums.TaskType.TaskLeader)
            {
                return GetSameLeaderForWP(employeeId, postId, employeeId2, postId2);
            }
            if (taskType == CustomModel.Common.Enums.TaskType.TaskTogether)
            {
                return GetTaskTogether(employeeId, postId);
            }
            return null;
        }

        private List<V_EMPLOYEEVIEW> GetTaskTogether(string employeeId,string postId)
        {
            List<V_EMPLOYEEVIEW> view1 = new List<V_EMPLOYEEVIEW>();
            view1 = GetTaskLeaderForWPLower(employeeId, postId);

            List<V_EMPLOYEEVIEW> view2 = GetTaskLeaderForWpUpper(employeeId, postId);
            view1.AddRange(view2);

            return view1;
        }

        #region 下达任务：当前员工的所有下级
      
        private List<V_EMPLOYEEVIEW> GetTaskLeaderForWPLower(string employeeId, string postId)
        {
            #region
            //List<V_EMPLOYEEVIEW> employeeSource = new List<V_EMPLOYEEVIEW>();

            //List<T_HR_EMPLOYEE> employees = dal.GetObjects<T_HR_EMPLOYEE>().Where(emp => emp.FATHEREMPLOYEEID == employeeId && emp.EMPLOYEESTATE != "2").ToList();
            ////如果在员工个人档案里设置了直接下级
            //if (employees != null && employees.Any())
            //{
            //    foreach (var emp in employees)
            //    {
            //        #region 把直接下级保存到 employeeSource里
            //        var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
            //                             join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
            //                             join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
            //                             join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
            //                             where em.T_HR_EMPLOYEE.EMPLOYEEID == emp.EMPLOYEEID && em.EDITSTATE == "1" && em.CHECKSTATE == "2" && em.ISAGENCY == "0"
            //                             select new V_EMPLOYEEVIEW
            //                             {
            //                                 EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
            //                                 EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
            //                                 OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
            //                                 OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
            //                                 OWNERPOSTID = em.T_HR_POST.POSTID,
            //                                 OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
            //                                 COMPANYNAME = c.BRIEFNAME,
            //                                 DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
            //                                 POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
            //                                 FATHEREmployeeID = postId
            //                             };
            //        employeeSource.AddRange(diretemployees.ToList());
            //        #endregion
            //    }
            //}
            //else
            //{
            //    var posts = dal.GetObjects<T_HR_POST>().Where(t => t.FATHERPOSTID == postId).ToList();
            //    if (posts.Any())
            //    {
            //        foreach (var post in posts)
            //        {
            //            var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
            //                                 join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
            //                                 join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
            //                                 join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
            //                                 where p.POSTID == post.POSTID && em.EDITSTATE == "1" && em.CHECKSTATE == "2" && em.ISAGENCY == "0"
            //                                 select new V_EMPLOYEEVIEW
            //                                 {
            //                                     EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
            //                                     EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
            //                                     OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
            //                                     OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
            //                                     OWNERPOSTID = em.T_HR_POST.POSTID,
            //                                     OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
            //                                     COMPANYNAME = c.BRIEFNAME,
            //                                     DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
            //                                     POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
            //                                     FATHEREmployeeID = postId
            //                                 };
            //            if (diretemployees != null && diretemployees.Any())
            //            {
            //                employeeSource.AddRange(diretemployees);
            //            }
            //        }
            //    }
            //}
            //return employeeSource;
            #endregion
            StringBuilder logMsg = new StringBuilder();
            logMsg.AppendLine("调用获取GetTaskLeaderForWPLower下达任务 employeeId=" + employeeId + ",postId=" + postId + "开始时间" + DateTime.Now.ToString());
            List<V_EMPLOYEEVIEW> employeeList = new List<V_EMPLOYEEVIEW>();
            GetTaskLeaderForWPLowerSubByEmployeeId(employeeId, postId, ref employeeList);
            logMsg.AppendLine("调用获取结束时间:" + DateTime.Now.ToString());
            SMT.Foundation.Log.Tracer.Debug(logMsg.ToString());
            return employeeList;
        }


        private List<V_EMPLOYEEVIEW> employeeAll;

        public List<V_EMPLOYEEVIEW> EmployeeAll
        {
            get
            {
                if (employeeAll == null)
                {
                    employeeAll = (from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                  join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                  join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                  join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                  where  em.EDITSTATE == "1" && em.CHECKSTATE == "2" && em.ISAGENCY == "0"
                                  && em.T_HR_EMPLOYEE.EMPLOYEESTATE != "2"
                                  select new V_EMPLOYEEVIEW
                                  {
                                      EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                      EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                      OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                      OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                      OWNERPOSTID = em.T_HR_POST.POSTID,
                                      FATHERPOSTID=em.T_HR_POST.FATHERPOSTID,
                                      OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                      COMPANYNAME = c.BRIEFNAME,
                                      DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                      POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                      FATHEREmployeeID = em.T_HR_EMPLOYEE.FATHEREMPLOYEEID
                                  }).ToList();
                }
                return employeeAll; 
            }
            set { employeeAll = value; }
        }

        private List<T_HR_POST> postList;
        public List<T_HR_POST> PostList
        {
            get
            {
                if (postList == null)
                {
                    postList = dal.GetObjects<T_HR_POST>().Where(t => t.CHECKSTATE == "2" && t.EDITSTATE == "1").ToList();
                }
                return postList;
            }
            set
            {
                postList = value;
            }
        }
        private bool GetTaskLeaderForWPLowerSubByEmployeeId(string employeeId, string postId, ref List<V_EMPLOYEEVIEW> employeeSource)
        {
            List<V_EMPLOYEEVIEW> employees = EmployeeAll.Where(emp => emp.FATHEREmployeeID == employeeId ).ToList();
            //如果在员工个人档案里设置了直接下级
            if (employees != null && employees.Any())
            {
                foreach (var emp in employees)
                {
                    #region 把直接下级保存到 employeeSource里
                    var diretemployees = from em in EmployeeAll
                                         where em.EMPLOYEEID == emp.EMPLOYEEID 
                                         select new V_EMPLOYEEVIEW
                                         {
                                             EMPLOYEECNAME = em.EMPLOYEECNAME,
                                             EMPLOYEEID = em.EMPLOYEEID,
                                             OWNERCOMPANYID = em.OWNERCOMPANYID,
                                             OWNERDEPARTMENTID = em.OWNERDEPARTMENTID,
                                             OWNERPOSTID = em.OWNERPOSTID,
                                             OWNERID = em.OWNERID,
                                             COMPANYNAME = em.COMPANYNAME,
                                             DEPARTMENTNAME = em.DEPARTMENTNAME,
                                             POSTNAME = em.POSTNAME,
                                             FATHEREmployeeID = em.FATHEREmployeeID,
                                             FATHERPOSTID = postId
                                         }; 
                    employeeSource.AddRange(diretemployees.ToList());

                    GetTaskLeaderForWPLowerSubByEmployeeId(emp.EMPLOYEEID, emp.OWNERPOSTID, ref employeeSource);
                    #endregion
                }
            }
            else
            {
                GetTaskLeaderForWPLowerSubByPostId(employeeId, postId, ref employeeSource);
            }
            return false;
        }

        private bool GetTaskLeaderForWPLowerSubByPostId(string employeeId, string postId, ref List<V_EMPLOYEEVIEW> employeeSource)
        {
            var diretemployees = from em in EmployeeAll where em.FATHERPOSTID == postId
                                 select new V_EMPLOYEEVIEW
                                 {
                                     EMPLOYEECNAME = em.EMPLOYEECNAME,
                                     EMPLOYEEID = em.EMPLOYEEID,
                                     OWNERCOMPANYID = em.OWNERCOMPANYID,
                                     OWNERDEPARTMENTID = em.OWNERDEPARTMENTID,
                                     OWNERPOSTID = em.OWNERPOSTID,
                                     OWNERID = em.OWNERID,
                                     COMPANYNAME = em.COMPANYNAME,
                                     DEPARTMENTNAME = em.DEPARTMENTNAME,
                                     POSTNAME = em.POSTNAME,
                                     FATHERPOSTID = postId
                                 };
            if (diretemployees != null && diretemployees.Any())
            {
                employeeSource.AddRange(diretemployees);
                foreach (var employee in diretemployees)
                {
                    GetTaskLeaderForWPLowerSubByEmployeeId(employee.EMPLOYEEID, employee.OWNERPOSTID, ref employeeSource);
                }
            }

            //var posts = PostList.Where(t => t.FATHERPOSTID == postId).ToList(); //dal.GetObjects<T_HR_POST>().Where(t => t.FATHERPOSTID == postId).ToList(); //EmployeeAll.Where(t => t.FATHERPOSTID == postId).ToList();
            //if (posts.Any())
            //{
            //    foreach (var post in posts)
            //    {
            //        var diretemployees = from em in EmployeeAll
            //                             where em.OWNERPOSTID == post.OWNERPOSTID 
            //                             select new V_EMPLOYEEVIEW
            //                             {
            //                                 EMPLOYEECNAME = em.EMPLOYEECNAME,
            //                                 EMPLOYEEID = em.EMPLOYEEID,
            //                                 OWNERCOMPANYID = em.OWNERCOMPANYID,
            //                                 OWNERDEPARTMENTID = em.OWNERDEPARTMENTID,
            //                                 OWNERPOSTID = em.OWNERPOSTID,
            //                                 OWNERID = em.OWNERID,
            //                                 COMPANYNAME = em.COMPANYNAME,
            //                                 DEPARTMENTNAME = em.DEPARTMENTNAME,
            //                                 POSTNAME = em.POSTNAME,
            //                                 FATHEREmployeeID = em.FATHEREmployeeID,
            //                                 FATHERPOSTID = em.FATHERPOSTID
            //                             };
            //        if (diretemployees != null && diretemployees.Any())
            //        {
            //            employeeSource.AddRange(diretemployees);
            //            foreach (var employee in diretemployees)
            //            {
            //                GetTaskLeaderForWPLowerSubByEmployeeId(employee.EMPLOYEEID, employee.OWNERPOSTID, ref employeeSource);
            //            }
            //        }
            //    }
            //}
            return false;
        }

        #endregion

        #region 支持任务：当前员工的所有上级

        private List<V_EMPLOYEEVIEW> GetTaskLeaderForWpUpper(string employeeId, string postId)
        {
            StringBuilder logMsg = new StringBuilder();
            logMsg.AppendLine("调用获取GetTaskLeaderForWpUpper直接上级 employeeId=" + employeeId + ",postId=" + postId + "开始时间" + DateTime.Now.ToString());
            List<V_EMPLOYEEVIEW> employeeList = new List<V_EMPLOYEEVIEW>();
            GetTaskLeaderForWPByEmployeeDirect(employeeId, postId, ref employeeList);
            logMsg.AppendLine("调用获取结束时间:" + DateTime.Now.ToString());
            SMT.Foundation.Log.Tracer.Debug(logMsg.ToString());
            return employeeList;
        }

        private bool GetTaskLeaderForWPByEmployeeDirect(string employeeId, string postId, ref List<V_EMPLOYEEVIEW> employeeSource)
        {
            T_HR_EMPLOYEE employee = dal.GetObjects<T_HR_EMPLOYEE>().Where(emp => emp.EMPLOYEEID == employeeId && emp.FATHEREMPLOYEEID != null && emp.EMPLOYEESTATE != "2").FirstOrDefault();
            //如果在员工个人档案里设置了直接上级
            if (employee != null)
            {
                //如果直接上级没有离职
                T_HR_EMPLOYEE fatherEmployee = dal.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEEID == employee.FATHEREMPLOYEEID && t.EMPLOYEESTATE != "2").FirstOrDefault();
                if (fatherEmployee != null)
                {
                    #region 把直接上级保存到 employeeSource里
                    var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                         join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                         join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                         join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                         where em.T_HR_EMPLOYEE.EMPLOYEEID == employee.FATHEREMPLOYEEID && em.EDITSTATE == "1" && em.CHECKSTATE == "2" && em.ISAGENCY == "0"
                                         select new V_EMPLOYEEVIEW
                                         {
                                             EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                             EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                             OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                             OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                             OWNERPOSTID = em.T_HR_POST.POSTID,
                                             OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                             COMPANYNAME = c.BRIEFNAME,
                                             DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                             POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME
                                         };
                    employeeSource.AddRange(diretemployees.ToList());
                    #endregion
                    if (string.IsNullOrEmpty(fatherEmployee.FATHEREMPLOYEEID))
                    {
                        //循环再取上级的上级
                        GetTaskLeaderForWPByEmployeeDirect(fatherEmployee.EMPLOYEEID, fatherEmployee.OWNERPOSTID, ref employeeSource);
                    }
                }
                else
                {
                    //如果直接上级已经离职,循环递归岗位
                    {
                        GetTaskLeaderForWPByPostId(employeeId, postId, ref employeeSource);
                    }
                }
            }
            else
            {
                GetTaskLeaderForWPByPostId(employeeId, postId, ref employeeSource);
            }
            return false;
        }

        private bool GetTaskLeaderForWPByPostId(string employeeId, string postId, ref List<V_EMPLOYEEVIEW> employeeSource)
        {
            var posts = dal.GetObjects<T_HR_POST>().Where(t => t.POSTID == postId).FirstOrDefault();
            if (string.IsNullOrEmpty(posts.FATHERPOSTID))
            {
                return false;
            }
            var diretemployees = from em in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                 join p in dal.GetObjects<T_HR_POST>() on em.T_HR_POST.POSTID equals p.POSTID
                                 join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                 join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                 where em.T_HR_POST.POSTID == posts.FATHERPOSTID && em.EDITSTATE == "1" && em.CHECKSTATE == "2"
                                 select new V_EMPLOYEEVIEW
                                 {
                                     EMPLOYEECNAME = em.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                     EMPLOYEEID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                     OWNERCOMPANYID = em.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                     OWNERDEPARTMENTID = em.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                     OWNERPOSTID = em.T_HR_POST.POSTID,
                                     OWNERID = em.T_HR_EMPLOYEE.EMPLOYEEID,
                                     COMPANYNAME = c.BRIEFNAME,
                                     DEPARTMENTNAME = d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                     POSTNAME = p.T_HR_POSTDICTIONARY.POSTNAME,
                                     POSTLEVEL = em.POSTLEVEL
                                 };
            if (diretemployees != null && diretemployees.Any())
            {
                employeeSource.AddRange(diretemployees);
                foreach (var employee in diretemployees)
                {
                    GetTaskLeaderForWPByEmployeeDirect(employee.EMPLOYEEID, employee.OWNERPOSTID, ref employeeSource);
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// 获取2个员工 有共同上级的员工
        /// </summary>
        private List<V_EMPLOYEEVIEW> GetSameLeaderForWP(string employeeId1,string postId1, string employeeId2,string postId2)
        {
            List<V_EMPLOYEEVIEW> employeeView1 = GetTaskLeaderForWpUpper(employeeId1, postId1);
            List<V_EMPLOYEEVIEW> employeeView2 = GetTaskLeaderForWpUpper(employeeId2, postId2);
            List<V_EMPLOYEEVIEW> sameLeaderView = new List<V_EMPLOYEEVIEW>();
            foreach (var view in employeeView1)
            {
                foreach (var view2 in employeeView2)
                {
                    if (view.EMPLOYEEID == view2.EMPLOYEEID)
                    {
                        sameLeaderView.Add(view);
                    }
                }
            }
            return sameLeaderView;
        }
    }
}
