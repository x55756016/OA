/*
 * 文件名：DepartmentBLL.cs
 * 作  用：
 * 创建人：
 * 创建时间：2010-2-26 14:19:12
 * 修改人：向寒咏
 * 修改说明：增加缓存
 * 修改时间：2010-7-7 14:19:12
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class DepartmentBLLBak : BaseBll<T_HR_DEPARTMENT>, ILookupEntity
    {
        private List<T_HR_DEPARTMENT> listDEPARTMENT;

        public List<T_HR_DEPARTMENT> ListDEPARTMENT
        {
            get
            {

                List<T_HR_DEPARTMENT> lsdic;
                if (CacheManager.GetCache("T_HR_DEPARTMENT") != null)
                {
                    lsdic = (List<T_HR_DEPARTMENT>)CacheManager.GetCache("T_HR_DEPARTMENT");
                }
                else
                {

                    var ents = from a in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                               select a;

                    lsdic = ents.ToList();
                    CacheManager.AddCache("T_HR_DEPARTMENT", lsdic);
                }

                return lsdic.Count() > 0 ? lsdic : null;
            }


            set { listDEPARTMENT = value; }
        }
        /// <summary>
        /// 获取全部可用的部门信息
        /// </summary>
        /// <returns>可用部门信息列表</returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentActived(string userID)
        {
            IQueryable<T_HR_DEPARTMENT> ents = GetDepartmentActived(userID, "3", "T_HR_DEPARTMENT");
            return ents;
        }

        /// <summary>
        /// 获取全部可用的部门信息
        /// </summary>
        /// <returns>可用部门信息列表</returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentActived(string userID,string perm,string entity)
        {
            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();

            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, entity, perm);
                SetDepartmentFilter(ref filterString, ref paras, userID);
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = "(" + filterString + ")";
                filterString += " and EDITSTATE==@" + paras.Count;
                paras.Add(state);
            }
            else
            {
                filterString = "EDITSTATE==@" + paras.Count;
                paras.Add(state);
            }

            filterString += " and CHECKSTATE==@" + paras.Count;
            paras.Add(checkState);

            IQueryable<T_HR_DEPARTMENT> ents = ListDEPARTMENT.AsQueryable();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            return ents;
        }

        /// <summary>
        /// 获取当前员工ID隶属于公司的所有部门
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="queryParas"></param>
        /// <param name="employeeID"></param>
        private void SetDepartmentFilter(ref string filterString, ref List<object> queryParas, string employeeID)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return;
            }
            EmployeeBLL bll = new EmployeeBLL();
            T_HR_EMPLOYEE emp = bll.GetEmployeeByID(employeeID);
            if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
            {
                emp.T_HR_EMPLOYEEPOST.Load();
            }
            foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
            {
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " OR ";

                if (!ep.T_HR_POSTReference.IsLoaded)
                    ep.T_HR_POSTReference.Load();

                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

                filterString += "DEPARTMENTID==@" + queryParas.Count().ToString();
                queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);

                //下级公司
                //SetInferiorCompany(ep, ref filterString, ref queryParas);  
                //SetInferiorCompany(ep.T_HR_POST.T_HR_DEPARTMENT, ref filterString, ref queryParas);  
            }
        }

        //private void SetInferiorCompany(T_HR_DEPARTMENT ep, ref string filterString, ref List<object> queryParas)
        //{
        //    var tempEnt = dal.GetObjects().Where(s => s.DEPARTMENTID == ep.DEPARTMENTID);
        //    if (tempEnt != null)
        //    {
        //        foreach (var ent in tempEnt)
        //        {
        //            if (!string.IsNullOrEmpty(filterString))
        //                filterString += " OR ";

        //            filterString += "DEPARTMENTID==@" + queryParas.Count().ToString();
        //            queryParas.Add(ep.DEPARTMENTID);
        //            SetInferiorCompany(ent, ref filterString, ref queryParas);
        //        }
        //    }
        //}
        private void SetInferiorCompany(T_HR_EMPLOYEEPOST ep, ref string filterString, ref List<object> queryParas)
        {
            var tempEnt = dal.GetObjects <T_HR_COMPANY>().Where(s => s.T_HR_COMPANY2.COMPANYID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
            if (tempEnt != null)
            {
                foreach (var ent in tempEnt)
                {
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " OR ";

                    filterString += "DEPARTMENTID==@" + queryParas.Count().ToString();
                    queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                    SetInferiorCompany(ep, ref filterString, ref queryParas);
                }
            }
        }


        /// <summary>
        /// 获取除审核状态不通过和编辑状态为删除全部可用的部门信息
        /// </summary>
        /// <returns>可用部门信息列表</returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentAll(string userID)
        {
            string state = ((int)EditStates.Deleted).ToString();
            string checkState = ((int)CheckStates.UnApproved).ToString();
            //var ents = from a in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
            //           where a.EDITSTATE == state && a.CHECKSTATE == auditState
            //           select a;
            List<object> paras = new List<object>();
            string filterString = "";

            if(!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_DEPARTMENT");
                SetDepartmentFilter(ref filterString, ref paras, userID);
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = "(" + filterString + ")";
                filterString += " and EDITSTATE<>@" + paras.Count;
                paras.Add(state);
            }
            else
            {
                filterString = "EDITSTATE<>@" + paras.Count;
                paras.Add(state);
            }

            filterString += " and CHECKSTATE<>@" + paras.Count;
            paras.Add(checkState);

            IQueryable<T_HR_DEPARTMENT> ents = ListDEPARTMENT.AsQueryable();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            return ents;
        }

        /// <summary>
        /// 获取指定公司全部可用的部门信息
        /// </summary>
        /// 函数创建人：吴鹏
        /// 函数创建时间：2010年1月23日, 16:32:22
        /// <param name="companyID">公司ID</param>
        /// <returns>可用部门信息列表</returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentActivedByCompanyID(string companyID)
        {
            string strFilter = string.Empty;
            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();
            List<string> strArgs = new List<string>();

            if (!string.IsNullOrEmpty(companyID))
            {
                strFilter = "T_HR_COMPANY.COMPANYID == @0";
                strArgs.Add(companyID);
            }
            if (!string.IsNullOrEmpty(strFilter))
            {
                strFilter += " and ";
            }
            strFilter += " EDITSTATE==@" + strArgs.Count;
            strArgs.Add(state);
            if (!string.IsNullOrEmpty(strFilter))
            {
                strFilter += " and ";
            }
            strFilter += " CHECKSTATE==@" + strArgs.Count;
            strArgs.Add(checkState);

            var q = from d in ListDEPARTMENT.AsQueryable()          
                    select d;           

            return q.Where(strFilter, strArgs.ToArray());
        }

        /// <summary>
        /// 根据公司ID和用户ID获取部门
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentActivedByCompanyIDAndUserID(string companyID,string userID)
        {
            string strFilter = string.Empty;
            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();
            List<object> strArgs = new List<object>();

            if (!string.IsNullOrEmpty(companyID))
            {
                strFilter = "T_HR_COMPANY.COMPANYID == @0";
                strArgs.Add(companyID);
            }
            if (!string.IsNullOrEmpty(strFilter))
            {
                strFilter += " and ";
            }
            strFilter += " EDITSTATE==@" + strArgs.Count;
            strArgs.Add(state);
            if (!string.IsNullOrEmpty(strFilter))
            {
                strFilter += " and ";
            }
            strFilter += " CHECKSTATE==@" + strArgs.Count;
            strArgs.Add(checkState);
            SetOrganizationFilter(ref strFilter, ref strArgs, userID, "T_HR_DEPARTMENT");

            var q = from d in ListDEPARTMENT.AsQueryable()
                    select d;

            return q.Where(strFilter, strArgs.ToArray());
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的部门信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_DEPARTMENT> DepartmentPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string strCheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            string tempString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref tempString, ref queryParas, userID, "T_HR_DEPARTMENT");
                SetDepartmentFilter(ref tempString, ref queryParas, userID);
            }

            IQueryable<T_HR_DEPARTMENT> ents = ListDEPARTMENT.AsQueryable();
            if (!string.IsNullOrEmpty(tempString))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    tempString = filterString + " and (" + tempString+")";
                }
            }
            else
            {
                tempString = filterString;
            }

            SetFilterWithflow("DEPARTMENTID", "T_HR_DEPARTMENT", userID, ref strCheckState, ref tempString, ref queryParas);

            if (!string.IsNullOrEmpty(tempString))
            {
                ents = ents.Where(tempString, queryParas.ToArray());
            }
            //BUG修改人:喻建华  时间:2010-06-19 修改原因:审核过滤错误
            if (!string.IsNullOrEmpty(strCheckState))
            {
                ents = ents.Where(m => m.CHECKSTATE == strCheckState);
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_DEPARTMENT>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 根据部门ID获取公司信息
        /// </summary>
        /// <param name="companyID">部门ID</param>
        /// <returns>返回部门信息</returns>
        public T_HR_DEPARTMENT GetDepartmentById(string depID)
        {
            var ents = from ent in ListDEPARTMENT.AsQueryable()
                       where ent.DEPARTMENTID == depID
                       select ent;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        public List<T_HR_DEPARTMENT> GetDepartmentByCompanyId(string comapnyID)
        {
            var ents = from ent in ListDEPARTMENT.AsQueryable()
                       where ent.T_HR_COMPANY.COMPANYID == comapnyID
                       select ent;
            return ents.ToList();
        }
        /// <summary>
        /// 添加公司部门
        /// </summary>
        /// <param name="entity">公司部门实例</param>
        public void DepartmentAdd(T_HR_DEPARTMENT entity)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.T_HR_COMPANY.COMPANYID == entity.T_HR_COMPANY.COMPANYID
                && s.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE == entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE);
                if (temp != null)
                {
                    throw new Exception("Repetition");
                }
                T_HR_DEPARTMENT ent = new T_HR_DEPARTMENT();
                Utility.CloneEntity<T_HR_DEPARTMENT>(entity, ent);
                ent.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                ent.T_HR_COMPANYReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY.COMPANYID);
                
                dal.Add(ent);
                CacheManager.RemoveCache("T_HR_DEPARTMENT");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// 变更公司部门
        /// </summary>
        /// <param name="entity">公司部门实例</param>
        public void DepartmentUpdate(T_HR_DEPARTMENT entity)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.T_HR_COMPANY.COMPANYID == entity.T_HR_COMPANY.COMPANYID
                && s.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE == entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE && s.DEPARTMENTID != entity.DEPARTMENTID);
                if (temp != null)
                {
                    throw new Exception("Repetition");
                }
                var ents = from ent in dal.GetObjects()
                           where ent.DEPARTMENTID == entity.DEPARTMENTID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_DEPARTMENT>(entity, ent);
                    if (entity.T_HR_DEPARTMENTDICTIONARY != null)
                    {
                        ent.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                    }
                    if (entity.T_HR_COMPANY != null)
                    {
                        ent.T_HR_COMPANYReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY.COMPANYID);
                    }
                    //如果审核状态为审核通过则添加部门历史
                    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        T_HR_DEPARTMENTHISTORY departmentHis = new T_HR_DEPARTMENTHISTORY();
                        departmentHis.RECORDSID = Guid.NewGuid().ToString();
                        departmentHis.DEPARTMENTID = entity.DEPARTMENTID;
                        departmentHis.DEPARTMENTFUNCTION = entity.DEPARTMENTFUNCTION;
                        departmentHis.EDITSTATE = entity.EDITSTATE;
                        departmentHis.COMPANYID = entity.T_HR_COMPANY.COMPANYID;
                        departmentHis.CREATEUSERID = entity.CREATEUSERID;
                        departmentHis.CREATEDATE = entity.CREATEDATE;
                        departmentHis.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                        departmentHis.OWNERDEPARTMENTID = entity.OWNERDEPARTMENTID;
                        departmentHis.OWNERID = entity.OWNERDEPARTMENTID;
                        departmentHis.OWNERPOSTID = entity.OWNERPOSTID;
                        departmentHis.UPDATEDATE = entity.UPDATEDATE;
                        departmentHis.UPDATEUSERID = entity.UPDATEUSERID;
                        if (entity.T_HR_DEPARTMENTDICTIONARY != null)
                        {
                            departmentHis.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                         new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                        }
                        departmentHis.REUSEDATE = DateTime.Now;
                        if (entity.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            departmentHis.CANCELDATE = DateTime.Now;
                        }
                        //DataContext.AddObject("T_HR_DEPARTMENTHISTORY", departmentHis);
                        dal.AddToContext(departmentHis);
                    }
                    dal.SaveContextChanges();
                    //DataContext.SaveChanges();
                    CacheManager.RemoveCache("T_HR_DEPARTMENT");
                    //WorkflowUtility.CallWorkflow("部门变更审核工作流", ent);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// 根据部门ID查找部门信息
        /// </summary>
        /// <param name="companyID">部门ID</param>
        /// <returns>返回部门实例</returns>
        public int GetDepartCount(string companyID)
        {
            var ents = from o in ListDEPARTMENT.AsQueryable()
                       where o.T_HR_COMPANY.COMPANYID == companyID
                       select o;

            return ents.Count();
        }
        /// <summary>
        /// 部门下是否有岗位
        /// </summary>
        /// <param name="departID">部门ID</param>
        /// <returns>是否成功找到岗位</returns>
        public bool IsExistChilds(string departID)
        {
            //是否分配了岗位
            PostBLL tmpBll = new PostBLL();
            int count = tmpBll.GetPositionCount(departID);
            if (count > 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="id">部门ID</param>
        public void DepartmentDelete(string id)
        {
            var entitys = (from ent in dal.GetTable()
                           where ent.DEPARTMENTID == id
                           select ent);
            if (entitys.Count() > 0)
            {
                var entity = entitys.FirstOrDefault();
                if (IsExistChilds(id))
                {
                    throw new Exception("此部门已关联岗位，不能删除！");
                }
                dal.Delete(entity);
                CacheManager.RemoveCache("T_HR_DEPARTMENT");
            }
            else
            {
                throw new Exception("没有找到对应实体！");
            }

        }
        /// <summary>
        /// 撤消部门
        /// </summary>
        /// <param name="sourceEntity">部门实体</param>
        public void DepartmentCancel(T_HR_DEPARTMENT sourceEntity)
        {

            var entitys = (from ent in dal.GetTable()
                           where ent.DEPARTMENTID == sourceEntity.DEPARTMENTID
                           select ent);

            if (entitys.Count() > 0)
            {
                var entity = entitys.FirstOrDefault();
                if (IsExistChilds(sourceEntity.DEPARTMENTID))
                {
                    throw new Exception("此部门已关联岗位，不能撤消！");
                }
                entity.EDITSTATE = sourceEntity.EDITSTATE;
                entity.CHECKSTATE = sourceEntity.CHECKSTATE;
                entity.UPDATEUSERID = sourceEntity.UPDATEUSERID;
                entity.UPDATEDATE = sourceEntity.UPDATEDATE;
                dal.Update(entity);
                WorkflowUtility.CallWorkflow("申请撤消工作流", entity);
                CacheManager.RemoveCache("T_HR_DEPARTMENT");
            }
            else
            {
                throw new Exception("没有找到对应实体！");
            }
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{

        //    IQueryable<T_HR_DEPARTMENT> ents = from a in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
        //                                       select a;


        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_DEPARTMENT");
                SetDepartmentFilter(ref filterString, ref queryParas, userID);
            }
            

            IQueryable<T_HR_DEPARTMENT> ents =
                from a in ListDEPARTMENT.AsQueryable()
                select a; 

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_DEPARTMENT>(ents, pageIndex, pageSize, ref pageCount);

            return ents.Count() > 0 ? ents.ToArray() : null;
        }
    }
}
