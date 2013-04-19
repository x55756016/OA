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
    public class EmployeeAddSumBatchBLL : BaseBll<T_HR_EMPLOYEEADDSUMBATCH>, IOperate
    {
        private EmployeeAddSumBLL dals = new EmployeeAddSumBLL();
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
        public IQueryable<T_HR_EMPLOYEEADDSUM> GetEmployeeAddSumAuditPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, DateTime starttime, DateTime endtime, string userID, string CheckState, int orgtype, string orgid)
        {
            EmployeeAddSumBLL bll = new EmployeeAddSumBLL();
            IQueryable<T_HR_EMPLOYEEADDSUM> q = null;
            if (CheckState != Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                if (paras.Count > 0)
                {
                    string ID = paras[0].ToString();
                    q = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>().Include("T_HR_EMPLOYEEADDSUMBATCH")
                        where a.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID == ID
                        select a;
                }
                else
                {
                    List<T_HR_EMPLOYEEADDSUM> list = new List<T_HR_EMPLOYEEADDSUM>();
                    var temp = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, userID, CheckState, orgtype, orgid);
                    if (temp != null)
                    {
                        list = temp.ToList();
                        foreach (var li in list)
                        {
                            var x = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>().Include("T_HR_EMPLOYEEADDSUMBATCH")
                                    where a.ADDSUMID == li.ADDSUMID
                                    select a;
                            if (x.Count() > 0)
                            {
                                if (x.FirstOrDefault().T_HR_EMPLOYEEADDSUMBATCH != null)
                                {
                                    string id = x.FirstOrDefault().T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID;
                                    if (!string.IsNullOrEmpty(id))
                                    {
                                        q = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>().Include("T_HR_EMPLOYEEADDSUMBATCH")
                                            where a.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID == id
                                            select a;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, userID, CheckState, orgtype, orgid);
            }
            return q;
        }

        /// <summary>
        /// 查询员工加扣款批量审核实体
        /// </summary>
        /// <param name="EmployeeAddSumBatchID">员工加扣款批量审核ID</param>
        /// <returns>返回员工加扣款批量审核实体</returns>
        public T_HR_EMPLOYEEADDSUMBATCH GetEmployeeAddSumBatchByID(string EmployeeAddSumBatchID)
        {
            if (EmployeeAddSumBatchID == string.Empty) return null;
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUMBATCH>()
                       where a.MONTHLYBATCHID == EmployeeAddSumBatchID
                       select a;
            if (ents.Count() > 0) return ents.FirstOrDefault();
            return null;
        }

        /// <summary>
        /// 查询员工加扣款批量审核实体
        /// </summary>
        /// <param name="EmployeeAddSumIDs">员工加扣款ID集合</param>
        /// <returns>返回员工加扣款批量审核实体</returns>
        public T_HR_EMPLOYEEADDSUMBATCH GetEmployeeAddSumBatchByID(string[] EmployeeAddSumIDs)
        {
            if (EmployeeAddSumIDs.Count() <= 0) return null;
            foreach (var ID in EmployeeAddSumIDs)
            {
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUMBATCH>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEADDSUM>() on a.MONTHLYBATCHID equals b.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID
                           where b.ADDSUMID == ID
                           select a;
                if (ents.Count() > 0) return ents.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// 新增员工加扣款批量审核
        /// </summary>
        /// <param name="entity">员工加扣款批量审核实体</param>
        /// <returns></returns>
        public bool EmployeeAddSumBatchAdd(T_HR_EMPLOYEEADDSUMBATCH entity, string[] addsumids)
        {
            int i = 0;
            try
            {
                i = dal.Add(entity);
                if (i > 0)
                {
                    foreach (var id in addsumids)
                    {
                        var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                                   where a.ADDSUMID == id
                                   select a;
                        if (ents.Count() > 0)
                        {
                            T_HR_EMPLOYEEADDSUM ent = ents.FirstOrDefault();
                            ent.CHECKSTATE = entity.CHECKSTATE;
                            string sql = "UPDATE T_HR_EMPLOYEEADDSUM T SET T.MONTHLYBATCHID = '" + entity.MONTHLYBATCHID + "'";
                            sql += " , T.CHECKSTATE = '" + entity.CHECKSTATE + "' WHERE T.ADDSUMID = '" + ent.ADDSUMID + "'";
                            EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                            bll.ExecuteSql(sql, "T_HR_EMPLOYEEADDSUM");
                            //EmployeeAddSumBLL bll = new EmployeeAddSumBLL();
                            //ent.T_HR_EMPLOYEEADDSUMBATCH = new T_HR_EMPLOYEEADDSUMBATCH(); 
                            //ent.T_HR_EMPLOYEEADDSUMBATCH.EntityKey =
                            //    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEADDSUMBATCH", "MONTHLYBATCHID", entity.MONTHLYBATCHID);
                            //bll.Update(ent);
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
        /// 更新员工加扣款批量审核
        /// </summary>
        /// <param name="entity">员工加扣款批量审核实体</param>
        /// <returns></returns>
        public void EmployeeAddSumBatchUpdate(T_HR_EMPLOYEEADDSUMBATCH entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.MONTHLYBATCHID == entity.MONTHLYBATCHID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_EMPLOYEEADDSUMBATCH>(entity, ent);
                    dal.Update(ent);
                    var entaddsums = from b in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                                     join c in dal.GetObjects<T_HR_EMPLOYEEADDSUMBATCH>() on b.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID equals c.MONTHLYBATCHID
                                     where c.MONTHLYBATCHID == entity.MONTHLYBATCHID
                                     select b;
                    if (entaddsums.Count() > 0)
                    {
                        foreach (var en in entaddsums)
                        {
                            T_HR_EMPLOYEEADDSUM temp = en;
                            temp.CHECKSTATE = entity.CHECKSTATE;
                            dals.EmployeeAddSumUpdate(temp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 删除员工加扣款批量审核，可同时删除多行记录
        /// </summary>
        /// <param name="AddSumBatchIDs">员工加扣款批量审核ID数组</param>
        /// <returns></returns>
        public int EmployeeAddSumBatchDelete(string[] AddSumBatchIDs)
        {
            foreach (string id in AddSumBatchIDs)
            {
                var ents = from e in dal.GetObjects<T_HR_EMPLOYEEADDSUMBATCH>()
                           where e.MONTHLYBATCHID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }

        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                dal.BeginTransaction();
                var addsumBatch = (from c in dal.GetObjects<T_HR_EMPLOYEEADDSUMBATCH>()
                                   where c.MONTHLYBATCHID == EntityKeyValue
                                   select c).FirstOrDefault();
                if (addsumBatch != null)
                {
                    addsumBatch.CHECKSTATE = CheckState;
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        addsumBatch.EDITSTATE = "1";
                    }
                    addsumBatch.UPDATEDATE = DateTime.Now;
                    dal.UpdateFromContext(addsumBatch);
                    string strSql = " update T_HR_EMPLOYEEADDSUM t  set t.checkstate ='" + CheckState + "' where t.monthlybatchid='" + EntityKeyValue + "' ";
                    dal.ExecuteCustomerSql(strSql);
                    i = dal.SaveContextChanges();
                }
                dal.CommitTransaction();
                return i;
            }
            catch (Exception e)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }
    }
}
