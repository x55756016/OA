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
    public class PerformanceRewardRecordBLL : BaseBll<T_HR_PERFORMANCEREWARDRECORD>
    {
        protected bool IsComputer = false;
        List<string> employeIDs = new List<string>();
        List<SMT_HRM_EFModel.T_HR_EMPLOYEE> employes = new List<SMT_HRM_EFModel.T_HR_EMPLOYEE>();
        protected string[] construe = new string[9];
        protected SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient FBSclient = new SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient();

        /// <summary>
        /// 根据ID获取绩效
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T_HR_PERFORMANCEREWARDRECORD GetPerformanceRewardByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_PERFORMANCEREWARDRECORD>()
                       where o.PERFORMANCEREWARDRECORDID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 更新绩效记录
        /// </summary>
        /// <param name="obj"></param>
        public void PerformanceRewardRecordUpdate(T_HR_PERFORMANCEREWARDRECORD obj)
        {
            var ent = from a in dal.GetTable()
                      where a.PERFORMANCEREWARDRECORDID == obj.PERFORMANCEREWARDRECORDID
                      select a;
            if (ent.Count() > 0)
            {
                T_HR_PERFORMANCEREWARDRECORD tmpEnt = ent.FirstOrDefault();

                Utility.CloneEntity<T_HR_PERFORMANCEREWARDRECORD>(obj, tmpEnt);

                dal.Update(tmpEnt);

                SMT.SaaS.BLLCommonServices.Utility.SubmitMyRecord<T_HR_PERFORMANCEREWARDRECORD>(ent);
            }
        }
        /// <summary>
        /// 删除绩效记录
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int PerformanceRewardRecordDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_PERFORMANCEREWARDRECORD>()
                           where e.PERFORMANCEREWARDRECORDID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    SMT.SaaS.BLLCommonServices.Utility.SubmitMyRecord<T_HR_PERFORMANCEREWARDRECORD>(ent);
                }

            }

            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页 </param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数 </param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_PERFORMANCEREWARDRECORD> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime startTime, DateTime endTime, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
        
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_PERFORMANCEREWARDRECORD");
            SetFilterWithflow("PERFORMANCEREWARDRECORDID", "T_HR_PERFORMANCEREWARDRECORD", userID, ref CheckState, ref  filterString, ref queryParas);
            if (!string.IsNullOrEmpty(CheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " and ";
                }
                filterString += "CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(CheckState);
            }
            IQueryable<T_HR_PERFORMANCEREWARDRECORD> ents = dal.GetObjects<T_HR_PERFORMANCEREWARDRECORD>();
          
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            //ents = ents.Where(m => m.CREATEDATE >= startTime);
            //ents = ents.Where(m => m.CREATEDATE <= endTime);
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_PERFORMANCEREWARDRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 添加绩效记录
        /// </summary>
        /// <param name="obj"></param>
        public bool PerformanceRewardRecordAdd(int orgtype, string orgid, string year, string month, DateTime startTime, DateTime endTime, string construes)
        {
            T_HR_PERFORMANCEREWARDRECORD entity = new T_HR_PERFORMANCEREWARDRECORD();
            Dictionary<string, decimal> getPerformance = new Dictionary<string, decimal>();
            construe = construes.Split(';');
            employes.Clear();
            employeIDs.Clear();
            switch (orgtype)
            {
                case 0:
                    GenerateCompanySalary(orgid, year, month);
                    break;
                case 1:
                    GenerateDepartmentSalary(orgid, year, month);
                    break;
                case 2:
                    GeneratePostSalary(orgid, year, month);
                    break;
            }

            SumPerformanceBll bll = new SumPerformanceBll();//获取绩效数据   (GetEmployePerformanceInterface)
            getPerformance = bll.GetEmployePerformance(employeIDs,startTime,endTime);
            if (getPerformance.Count > 0)
            { 
                int i=0;
                foreach(string gp in getPerformance.Keys)
                {
                    var ent = from b in dal.GetObjects<T_HR_PERFORMANCEREWARDRECORD>()
                              where b.EMPLOYEEID == gp && b.SALARYYEAR == year && b.SALARYMONTH == month
                              select b;
                    if (ent.Count() > 0) entity = ent.FirstOrDefault();
                    entity.EMPLOYEEID = gp;
                    entity.EMPLOYEENAME = employes[i].EMPLOYEECNAME;
                    entity.EMPLOYEECODE = employes[i].EMPLOYEECODE;
                    entity.SALARYYEAR = year;
                    entity.SALARYMONTH = month;
                    entity.STARTDATE = startTime;
                    entity.ENDDATE = endTime;
                    entity.CHECKSTATE = "0";
                    entity.PERFORMANCESCORE = Convert.ToDecimal(getPerformance[gp].ToString());
                    try
                    {
                        entity.CREATEUSERID = construe[0];
                        entity.CREATEPOSTID = construe[1];
                        entity.CREATEDEPARTMENTID = construe[2];
                        entity.CREATECOMPANYID = construe[3];

                        entity.OWNERID = construe[4];
                        entity.OWNERPOSTID = construe[5];
                        entity.OWNERDEPARTMENTID = construe[6];
                        entity.OWNERCOMPANYID = construe[7];
                    }
                    catch (Exception exx)
                    {
                        SMT.Foundation.Log.Tracer.Debug(exx.Message);
                        exx.Message.ToString();
                    }
                    if (ent.Count() > 0)
                    {
                        dal.Update(entity);
                    }
                    else
                    {
                        entity.PERFORMANCEREWARDRECORDID = Guid.NewGuid().ToString();
                        dal.AddToContext(entity);
                        //DataContext.AddObject("T_HR_PERFORMANCEREWARDRECORD", entity);
                    }
                    i++;
                }
                if(dal.SaveContextChanges()>0)return true;
            }
            return false;
        }

        public void GenerateCompanySalary(string companyID, string year, string month)
        {
            DepartmentBLL bll = new DepartmentBLL();
            List<SMT_HRM_EFModel.T_HR_DEPARTMENT> emplist = bll.GetDepartmentByCompanyId(companyID);
            foreach (var emp in emplist)
            {
                GenerateDepartmentSalary(emp.DEPARTMENTID, year, month);
            }
        }

        public void GenerateDepartmentSalary(string departID, string year, string month)
        {
            PostBLL bll = new PostBLL();
            List<SMT_HRM_EFModel.T_HR_POST> emplist = bll.GetPostByDepartId(departID);
            foreach (var emp in emplist)
            {
                GeneratePostSalary(emp.POSTID, year, month);
            }
        }
        public void GeneratePostSalary(string postID, string year, string month)
        {
            EmployeePostBLL bll = new EmployeePostBLL();    
            EmployeeSalaryRecordBLL bllemp = new EmployeeSalaryRecordBLL();
            List<SMT_HRM_EFModel.T_HR_EMPLOYEEPOST> emplist = bll.GetEmployeePostByPostID(postID);
            foreach (var emp in emplist)
            {
                employes.Add(bllemp.GetEmployeeInfor(emp.T_HR_EMPLOYEE.EMPLOYEEID));
                employeIDs.Add(emp.T_HR_EMPLOYEE.EMPLOYEEID);
            }
        }


    }
}
