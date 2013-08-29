using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class PensionMasterBLL : BaseBll<T_HR_PENSIONMASTER>, IOperate
    {
        /// <summary>
        /// 员工社保缴交日期字段数据导入到员工个人档案里去（PL/SQL也可以）
        /// </summary>
        public void PensionMaterToEmployee()
        {
            try
            {
                //社保有数据，员工档案没数据，这就是要导入的数据
                var entPen = from p in dal.GetObjects()
                             join e in dal.GetObjects<T_HR_EMPLOYEE>() on p.T_HR_EMPLOYEE.EMPLOYEEID equals e.EMPLOYEEID
                             where p.SOCIALSERVICEYEAR != null && e.SOCIALSERVICEYEAR == null && p.CHECKSTATE == "2"
                             select p;
                //没有社保的员工档案信息
                var entEmp = from e in dal.GetObjects<T_HR_EMPLOYEE>()
                             where e.SOCIALSERVICEYEAR == null
                             select e;
                int count = 0;
                SMT.Foundation.Log.Tracer.Debug("更新开始时间："+System.DateTime.Now.ToString());
                if (entEmp != null && entPen != null && entPen.Count() > 0 && entEmp.Count() > 0)
                {
                    //慢慢替换进去
                    foreach (var itEmp in entEmp)
                    {
                        foreach (var itPen in entPen)
                        {
                            if (itPen.OWNERID != null && itPen.OWNERID == itEmp.EMPLOYEEID)
                            {
                                itEmp.SOCIALSERVICEYEAR = itPen.SOCIALSERVICEYEAR;
                                dal.Update(itEmp);
                                count++;
                                break;
                            }
                        }
                    }
                }
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "PensionMaterToEmployee更新社保进档案总共: " + count+" 条数据");
                SMT.Foundation.Log.Tracer.Debug("更新结束时间：" + System.DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "PensionMaterToEmployee更新社保进档案异常:" + ex.Message);
            }
           
        }

        /// <summary>
        /// 新增社保档案记录
        /// </summary>
        /// <param name="entity">社保档案实体</param>
        public void PensionMasterAdd(T_HR_PENSIONMASTER entity, ref string strMsg)
        {
            try
            {
                //entity.SOCIALSERVICE;
                T_HR_PENSIONMASTER pension = new T_HR_PENSIONMASTER();
                //entity.SOCIALSERVICE;
                var entTmp = from c in dal.GetObjects()
                             where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && c.ISVALID == "1"
                             select c;
                if (entTmp.Count() > 0)
                {
                    // throw new Exception("EXIST");
                    strMsg = "EXIST";
                    return;
                }
                Utility.CloneEntity<T_HR_PENSIONMASTER>(entity, pension);
                if (entity.T_HR_EMPLOYEE != null)
                {
                    pension.T_HR_EMPLOYEEReference.EntityKey =
                         new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                }
                pension.CREATEDATE = System.DateTime.Now;
                dal.Add(pension);
                // Add(pension);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "PensionMasterAdd:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 员工入职录入社保记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string PensionMasterByEmployeeAdd(T_HR_EMPLOYEE entity)
        {
            try
            {
                T_HR_PENSIONMASTER pension = new T_HR_PENSIONMASTER();
                pension.PENSIONMASTERID = Guid.NewGuid().ToString();
                pension.T_HR_EMPLOYEEReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.EMPLOYEEID);
                pension.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                pension.OWNERDEPARTMENTID = entity.OWNERDEPARTMENTID;
                pension.OWNERPOSTID = entity.OWNERPOSTID;
                pension.OWNERID = entity.OWNERID;
                pension.CREATEUSERID = entity.CREATEUSERID;
                pension.CREATEDATE = DateTime.Now;
                pension.ISVALID = "0";
                dal.Add(pension);
                return "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "PensionMasterByEmployeeAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 更新社保档案记录
        /// </summary>
        /// <param name="entity">社保档案实体</param>
        public void PensionMasterUpdate(T_HR_PENSIONMASTER entity)
        {
            try
            {
                var ents = from a in dal.GetObjects()
                           where a.PENSIONMASTERID == entity.PENSIONMASTERID
                           select a;
                if (ents.Count() > 0)
                {
                    T_HR_PENSIONMASTER pension = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_PENSIONMASTER>(entity, pension);
                    if (entity.T_HR_EMPLOYEE != null)
                    {
                        pension.T_HR_EMPLOYEEReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    }
                    //pension.CARDID = entity.CARDID;
                    //pension.COMPUTERNO = entity.COMPUTERNO;
                    //pension.PENSIONCITY = entity.PENSIONCITY;
                    //pension.STARTDATE = entity.STARTDATE;
                    //pension.LASTDATE = entity.LASTDATE;
                    //pension.ISVALID = entity.ISVALID;
                    //pension.REMARK = entity.REMARK;
                    //pension.CREATEUSERID = entity.CREATEUSERID;
                    //pension.CREATEDATE = entity.CREATEDATE;
                    int i = dal.Update(pension);
                    SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "PensionMasterUpdate CheckState:" + pension.CHECKSTATE + ",UpdateResult:" + i.ToString());
                    //  Update(pension);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "PensionMasterUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 可删除多条记录
        /// </summary>
        /// <param name="pensionMasterID">社保档案ID组</param>
        /// <returns></returns>
        public int PensionMasterDelete(string[] pensionMasterIDs)
        {
            try
            {
                foreach (string id in pensionMasterIDs)
                {

                    var ents = from a in dal.GetObjects()
                               where a.PENSIONMASTERID == id
                               select a;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                        //  DeleteMyRecord(ent);
                    }


                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "PensionMasterDelete:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 根据社保档案ID查询实体
        /// </summary>
        /// <param name="pensionMasterID">社保档案ID</param>
        /// <returns>返回社保档案信息</returns>
        public T_HR_PENSIONMASTER GetPensionMasterByID(string pensionMasterID)
        {
            var ents = from a in dal.GetObjects().Include("T_HR_EMPLOYEE")
                       where a.PENSIONMASTERID == pensionMasterID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 根据员工ID获取社保档案
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_PENSIONMASTER GetPensionMasterByEmployeeID(string employeeID)
        {
            var ents = from a in dal.GetObjects().Include("T_HR_EMPLOYEE")
                       where a.T_HR_EMPLOYEE.EMPLOYEEID == employeeID
                       select a;
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
        public IQueryable<T_HR_PENSIONMASTER> PensionMasterPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckstate, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);


            if (strCheckstate != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_PENSIONMASTER");
                if (strCheckstate != Convert.ToInt32(CheckStates.All).ToString())
                {
                    if (queryParas.Count() > 0)
                    {
                        filterString += " and ";
                    }

                    filterString += " CHECKSTATE == @" + queryParas.Count().ToString();
                    queryParas.Add(strCheckstate);
                }

            }
            else
            {
                SetFilterWithflow("PENSIONMASTERID", "T_HR_PENSIONMASTER", userID, ref strCheckstate, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }

            IQueryable<T_HR_PENSIONMASTER> ents = dal.GetObjects().Include("T_HR_EMPLOYEE");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_PENSIONMASTER>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 引擎更新单据状态专用
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var entity = (from c in dal.GetObjects()
                              where c.PENSIONMASTERID == EntityKeyValue
                              select c).FirstOrDefault();
                if (entity != null)
                {
                    entity.CHECKSTATE = CheckState;
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        entity.ISVALID = "1";
                    }
                    PensionMasterUpdate(entity);
                    i = 1;
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }

        }
    }
}
