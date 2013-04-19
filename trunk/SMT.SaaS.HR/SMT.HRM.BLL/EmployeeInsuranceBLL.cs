using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class EmployeeInsuranceBLL : BaseBll<T_HR_EMPLOYEEINSURANCE>
    {
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
        public IQueryable<T_HR_EMPLOYEEINSURANCE> EmployeeInsurancePaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckState, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            List<object> objArgs = new List<object>();
            objArgs.Add(paras);
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEEINSURANCE");



                if (!string.IsNullOrEmpty(strCheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " AND";
                    }

                    filterString += " CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
            }
            else
            {
                SetFilterWithflow("EMPLOYINSURANCEID", "T_HR_EMPLOYEEINSURANCE", userID, ref strCheckState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }

            IQueryable<T_HR_EMPLOYEEINSURANCE> ents = dal.GetObjects().Include("T_HR_EMPLOYEE");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEEINSURANCE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 添加保险记录
        /// </summary>
        /// <param name="entity"></param>
        public void EmployeeInsuranceAdd(T_HR_EMPLOYEEINSURANCE entity, ref string strMsg)
        {
            try
            {
                var entTmp = from c in dal.GetObjects()
                             where c.CONTRACTCODE == entity.CONTRACTCODE
                             select c;
                if (entTmp.Count() > 0)
                {
                    // throw new Exception("INSURANCECONTRACTEXIST");
                    strMsg = "INSURANCECONTRACTEXIST";
                    return;
                }
                T_HR_EMPLOYEEINSURANCE ent = new T_HR_EMPLOYEEINSURANCE();
                Utility.CloneEntity(entity, ent);
                // ent.EMPLOYINSURANCEID = entity.EMPLOYINSURANCEID;
                if (entity.T_HR_EMPLOYEE != null)
                {
                    ent.T_HR_EMPLOYEEReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                }
                //ent.INSURANCECATEGORY = entity.INSURANCECATEGORY;
                //ent.INSURANCECOMPANY = entity.INSURANCECOMPANY;
                //ent.INSURANCENAME = entity.INSURANCENAME;
                //ent.INSURANCECOST = entity.INSURANCECOST;
                //ent.CONTRACTCODE = entity.CONTRACTCODE;
                //ent.STARTDATE = entity.STARTDATE;
                //ent.LASTDATE = entity.LASTDATE;
                //ent.PERIOD = entity.PERIOD;
                //ent.ALARMDAY = entity.ALARMDAY;
                //ent.INSURANCEPAY = entity.INSURANCEPAY;
                //ent.EDITSTATE = entity.EDITSTATE;
                //ent.CHECKSTATE = entity.CHECKSTATE;
                //ent.UPDATEUSERID = entity.UPDATEUSERID;
                //ent.UPDATEDATE = entity.UPDATEDATE;
                dal.Add(ent);
               // Add(ent);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeInsuranceAdd:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 修改保险记录
        /// </summary>
        /// <param name="entity"></param>
        public void EmployeeInsuranceUpdate(T_HR_EMPLOYEEINSURANCE entity, ref string strMsg)
        {
            try
            {
                var entTmp = from c in dal.GetObjects()
                             where c.CONTRACTCODE == entity.CONTRACTCODE && c.T_HR_EMPLOYEE.EMPLOYEEID != entity.T_HR_EMPLOYEE.EMPLOYEEID
                             select c;
                if (entTmp.Count() > 0)
                {
                    // throw new Exception("INSURANCECONTRACTEXIST");
                    strMsg = "INSURANCECONTRACTEXIST";
                    return;
                }
                entity.EntityKey =
                   new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEINSURANCE", "EMPLOYINSURANCEID", entity.EMPLOYINSURANCEID);

                if (entity.T_HR_EMPLOYEE != null)
                {
                    entity.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    entity.T_HR_EMPLOYEE.EntityKey =
                   new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                }
                dal.Update(entity);
               // Update(entity);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeInsuranceUpdate:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 删除多项保险记录
        /// </summary>
        /// <param name="employInsuranceIDs">保险记录组</param>
        /// <returns></returns>
        public int EmployeeInsuranceDelete(string[] employInsuranceIDs)
        {
            foreach (string dd in employInsuranceIDs)
            {
                T_HR_EMPLOYEEINSURANCE ent = dal.GetObjects().FirstOrDefault(s => s.EMPLOYINSURANCEID == dd);

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                  //  DeleteMyRecord(ent);
                }
            }
            return dal.SaveContextChanges();
        }

        /// <summary>
        /// 根据保险记录ID查询保险记录实体
        /// </summary>
        /// <param name="employInsuranceID">保险记录ID</param>
        /// <returns>返回记录实体</returns>
        public T_HR_EMPLOYEEINSURANCE GetEmployeeInsuranceByID(string employInsuranceID)
        {
            var ents = from a in dal.GetObjects().Include("T_HR_EMPLOYEE")
                       where a.EMPLOYINSURANCEID == employInsuranceID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据员工ID获取保险记录
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEEINSURANCE> GetEmployeeInsuranceByEmployeeID(string employeeID)
        {
            var ents = dal.GetObjects().Include("T_HR_EMPLOYEE").Where(s => s.T_HR_EMPLOYEE.EMPLOYEEID == employeeID);
            return ents.Count() > 0 ? ents.ToList() : null;
        }
    }
}
