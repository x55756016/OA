using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class PostBLLBak : BaseBll<T_HR_POST>, ILookupEntity
    {
        private List<T_HR_POST> listPOST;

        public List<T_HR_POST> ListPOST
        {
            get
            {

                List<T_HR_POST> lsdic;
                if (CacheManager.GetCache("T_HR_POST") != null)
                {
                    lsdic = (List<T_HR_POST>)CacheManager.GetCache("T_HR_POST");
                }
                else
                {

                    var ents = from a in  dal.GetObjects().Include("T_HR_DEPARTMENT").Include("T_HR_POSTDICTIONARY")
                               select a;

                    lsdic = ents.ToList();
                    CacheManager.AddCache("T_HR_POST", lsdic);
                }

                return lsdic.Count() > 0 ? lsdic : null;
            }


            set { listPOST = value; }
        }

                /// <summary>
        /// 获取全部可用的岗位信息
        /// </summary>
        /// <returns>可用岗位信息列表</returns>
        public IQueryable<T_HR_POST> GetPostActived(string userID)
        {
            IQueryable<T_HR_POST> ents = GetPostActived(userID, "3", "T_HR_POST");
            return ents;
        }
        /// <summary>
        /// 获取全部可用的岗位信息
        /// </summary>
        /// <returns>可用岗位信息列表</returns>
        public IQueryable<T_HR_POST> GetPostActived(string userID, string perm, string entity)
        {
            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();
            //var ents = from a in dal.GetObjects().Include("T_HR_DEPARTMENT").Include("T_HR_POSTDICTIONARY")
            //           where a.EDITSTATE == state && a.CHECKSTATE == auditState
            //           select a;
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, entity, perm);
                SetPostFilter(ref filterString, ref paras, userID);
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

            IQueryable<T_HR_POST> ents = ListPOST.AsQueryable();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            return ents;
        }
        /// <summary>
        /// 获取全部可用的岗位信息
        /// </summary>
        /// <returns>可用岗位信息列表</returns>
        public IQueryable<T_HR_POST> GetPostAll(string userID)
        {
            string state = ((int)EditStates.Deleted).ToString();
            string checkState = ((int)CheckStates.UnApproved).ToString();
            //var ents = from a in dal.GetObjects().Include("T_HR_DEPARTMENT").Include("T_HR_POSTDICTIONARY")
            //           where a.EDITSTATE == state && a.CHECKSTATE == auditState
            //           select a;
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_POST");
                SetPostFilter(ref filterString, ref paras, userID);
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

            IQueryable<T_HR_POST> ents = ListPOST.AsQueryable();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            return ents;
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的岗位信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public List<T_HR_POST> PostPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string strCheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            string tempString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref tempString, ref queryParas, userID, "T_HR_POST");
                SetPostFilter(ref tempString, ref queryParas, userID);
            }

            IQueryable<T_HR_POST> ents = ListPOST.AsQueryable();
            if (!string.IsNullOrEmpty(tempString))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    tempString = "(" + tempString + ")";
                    tempString += " and " + filterString;
                }
            }
            else
            {
                tempString = filterString;
            }
            SetFilterWithflow("POSTID", "T_HR_POST", userID, ref strCheckState, ref tempString, ref queryParas);

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

            var ent = Utility.Pager<T_HR_POST>(ents, pageIndex, pageSize, ref pageCount);
            if (ent != null)
            {
                foreach (var temp in ent.ToList())
                {
                    temp.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                    temp.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
                }
            }

            return ent != null ? ent.ToList() : null;
        }
        /// <summary>
        /// 获取当前员工ID隶属于公司的所有岗位
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="queryParas"></param>
        /// <param name="employeeID"></param>
        private void SetPostFilter(ref string filterString, ref List<object> queryParas, string employeeID)
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
                if (!ep.T_HR_POSTReference.IsLoaded)
                    ep.T_HR_POSTReference.Load();

                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

                //var ent = DataContext.T_HR_DEPARTMENT.Where(s => s.T_HR_COMPANY.COMPANYID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                //if (ent != null)
                //{
                //    foreach (var tempEnt in ent)
                //    {
                //        if (!string.IsNullOrEmpty(filterString))
                //            filterString += " OR ";
                //        filterString += "T_HR_DEPARTMENT.DEPARTMENTID==@" + queryParas.Count().ToString();
                //        queryParas.Add(tempEnt.DEPARTMENTID);
                //    }
                //}

                if (!string.IsNullOrEmpty(filterString))
                    filterString += " OR ";
                filterString += "POSTID==@" + queryParas.Count().ToString();
                queryParas.Add(ep.T_HR_POST.POSTID);

                //下级公司
                //SetInferiorCompany(ep, ref filterString, ref queryParas);
            }
        }

        private void SetInferiorCompany(T_HR_EMPLOYEEPOST ep, ref string filterString, ref List<object> queryParas)
        {
            var tempEnt = dal.GetObjects<T_HR_COMPANY>().Where(s => s.T_HR_COMPANY2.COMPANYID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
            if (tempEnt != null)
            {
                foreach (var comEnt in tempEnt)
                {
                    var ent = dal.GetObjects<T_HR_DEPARTMENT>().Where(s => s.T_HR_COMPANY.COMPANYID == comEnt.COMPANYID);
                    if (ent != null)
                    {
                        foreach (var depEnt in ent)
                        {
                            if (!string.IsNullOrEmpty(filterString))
                                filterString += " OR ";
                            filterString += "T_HR_DEPARTMENT.DEPARTMENTID==@" + queryParas.Count().ToString();
                            queryParas.Add(depEnt.DEPARTMENTID);
                        }
                    }
                    SetInferiorCompany(ep, ref filterString, ref queryParas);
                }
            }
        }
        /// <summary>
        /// 根据岗位ID获取岗位信息
        /// </summary>
        /// <param name="companyID">岗位ID</param>
        /// <returns>返回岗位信息</returns>
        public T_HR_POST GetPostById(string postID)
        {
            var ents = from ent in ListPOST.AsQueryable()
                       where ent.POSTID == postID
                       select ent;
            T_HR_POST emp = ents.Count() > 0 ? ents.FirstOrDefault() : null;
            if (emp != null)
            {
                //加载公司
                emp.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                //加载部门字典
                emp.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
            }
            return emp;
        }
        public List<T_HR_POST> GetPostByDepartId(string departID)
        {
            var ents = from ent in ListPOST.AsQueryable()
                       where ent.T_HR_DEPARTMENT.DEPARTMENTID == departID
                       select ent;
            return ents.ToList();
        }
        /// <summary>
        /// 添加岗位
        /// </summary>
        /// <param name="entity">岗位信息实例</param>
        public void PostAdd(T_HR_POST entity)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.T_HR_DEPARTMENT.DEPARTMENTID == entity.T_HR_DEPARTMENT.DEPARTMENTID
                && s.T_HR_POSTDICTIONARY.POSTCODE == entity.T_HR_POSTDICTIONARY.POSTCODE);
                if (temp != null)
                {
                    throw new Exception("Repetition");
                }
                T_HR_POST ent = new T_HR_POST();
                Utility.CloneEntity<T_HR_POST>(entity, ent);
                //岗位字典
                ent.T_HR_POSTDICTIONARYReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                //部门
                ent.T_HR_DEPARTMENTReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENT", "DEPARTMENTID", entity.T_HR_DEPARTMENT.DEPARTMENTID);
                
                dal.Add(ent);
                CacheManager.RemoveCache("T_HR_POST");
               // WorkflowUtility.CallWorkflow("岗位添加申请审核工作流", ent);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 修改岗位
        /// </summary>
        /// <param name="entity">岗位信息实例</param>
        public void PostUpdate(T_HR_POST entity)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.T_HR_DEPARTMENT.DEPARTMENTID == entity.T_HR_DEPARTMENT.DEPARTMENTID
                   && s.T_HR_POSTDICTIONARY.POSTCODE == entity.T_HR_POSTDICTIONARY.POSTCODE && s.POSTID != entity.POSTID);
                if (temp != null)
                {
                    throw new Exception("Repetition");
                }
                var ents = from ent in dal.GetObjects()
                            where ent.POSTID == entity.POSTID
                           select ent;
                 if (ents.Count() > 0)
                 {                    
                     var ent = ents.FirstOrDefault();
                     Utility.CloneEntity<T_HR_POST>(entity, ent);
                     ////岗位编置有变动就需要发起工作流
                     //if (ent.POSTNUMBER.GetValueOrDefault() != ent.POSTNUMBER.GetValueOrDefault())
                     //{
                     //    WorkflowUtility.CallWorkflow("岗位编制变更申请工作流", ent);
                     //}
                     
                     //岗位字典
                     ent.T_HR_POSTDICTIONARYReference.EntityKey =
                         new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                     //部门
                     ent.T_HR_DEPARTMENTReference.EntityKey =
                         new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENT", "DEPARTMENTID", entity.T_HR_DEPARTMENT.DEPARTMENTID);

                     //如果审核状态为审核通过则添加岗位历史
                     if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                     {
                         T_HR_POSTHISTORY postHis = new T_HR_POSTHISTORY();
                         postHis.RECORDSID = Guid.NewGuid().ToString();
                         //postHis = Utility.CloneObject<T_HR_POSTHISTORY>(entity);
                         postHis.CHANGEPOST = entity.CHANGEPOST;
                         postHis.COMPANYID = entity.COMPANYID;
                         postHis.CREATEUSERID = entity.CREATEUSERID;
                         postHis.CREATEDATE = entity.CREATEDATE;
                         postHis.DEPARTMENTNAME = entity.DEPARTMENTNAME;
                         postHis.FATHERPOSTID = entity.FATHERPOSTID;
                         postHis.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                         postHis.OWNERDEPARTMENTID = entity.OWNERDEPARTMENTID;
                         postHis.OWNERID = entity.OWNERID;
                         postHis.OWNERPOSTID = entity.OWNERPOSTID;
                         postHis.POSTFUNCTION = entity.POSTFUNCTION;
                         postHis.POSTGOAL = entity.POSTGOAL;
                         postHis.POSTID = entity.POSTID;
                         postHis.POSTNUMBER = entity.POSTNUMBER;
                         postHis.UNDERNUMBER = entity.UNDERNUMBER;
                         postHis.UPDATEDATE = entity.UPDATEDATE;
                         postHis.UPDATEUSERID = entity.UPDATEUSERID;
                         postHis.PROMOTEDIRECTION = entity.PROMOTEDIRECTION;
                         postHis.DEPARTMENTID = entity.T_HR_DEPARTMENT.DEPARTMENTID;                        
                         if (entity.T_HR_POSTDICTIONARY != null)
                         {
                             postHis.T_HR_POSTDICTIONARYReference.EntityKey =
                          new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                         }
                         postHis.REUSEDATE = DateTime.Now;
                         if (entity.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                         {
                             postHis.CANCELDATE = DateTime.Now;
                         }
                         //DataContext.AddObject("T_HR_POSTHISTORY", postHis);
                         dal.AddToContext(postHis);
                     }
                     dal.SaveContextChanges();
                     //DataContext.SaveChanges();
                     CacheManager.RemoveCache("T_HR_POST");
                     //WorkflowUtility.CallWorkflow("岗位变更申请审核工作流", ent);
                 }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取指定部门下的岗位数
        /// </summary>
        /// <param name="departID">部门ID</param>
        /// <returns>岗位数量</returns>
        public int GetPositionCount(string departID)
        {
            var ents = from ent in ListPOST.AsQueryable()
                       where ent.T_HR_DEPARTMENT.DEPARTMENTID == departID
                       select ent;
            return ents == null ? 0 : ents.Count();
        }

        /// <summary>
        /// 岗位下是否有员工
        /// </summary>
        /// <param name="positionID">岗位ID</param>
        /// <returns>是否成功找到员工</returns>
        public bool IsExistChilds(string positionID)
        {
            //是否分配了员工
            EmployeePostBLL tmpBll = new EmployeePostBLL();
            int count = tmpBll.GetEmployeesCount(positionID);
            if (count > 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 获取岗位的人数
        /// </summary>
        /// <param name="positionID"></param>
        /// <returns></returns>
        public int GetPostNumber(string positionID)
        {
            int number = 0;

            var ent = from c in dal.GetObjects <T_HR_EMPLOYEEPOST>()
                      where c.T_HR_POST.POSTID ==positionID
                      select c;
            var postnumber = from b in ListPOST.AsQueryable()
                       where b.POSTID == positionID
                       select b.POSTNUMBER;
            number = Convert.ToInt32(postnumber.FirstOrDefault().Value);
            return number-ent.Count();
            
        }

        /// <summary>
        /// 删除部门岗位信息
        /// </summary>
        /// <param name="id">岗位ID</param>
        public void PostDelete(string id)
        {
            var entitys = (from ent in dal.GetTable()
                           where ent.POSTID == id
                           select ent);
            if (entitys.Count() > 0)
            {
                var entity = entitys.FirstOrDefault();
                if (IsExistChilds(id))
                {
                    throw new Exception("此岗位下已关联员工，不能删除！");
                }
                dal.Delete(entity);
                CacheManager.RemoveCache("T_HR_POST");
            }
            else
            {
                throw new Exception("没有找到对应实体！");
            }

        }
        /// <summary>
        /// 撤消岗位信息
        /// </summary>
        /// <param name="sourceEntity">岗位实体</param>
        public void PostCancel(T_HR_POST sourceEntity)
        {
            
            var entitys = (from ent in dal.GetTable()
                           where ent.POSTID == sourceEntity.POSTID
                           select ent);

            if (entitys.Count() > 0)
            {
                var entity = entitys.FirstOrDefault();
                if (IsExistChilds(sourceEntity.POSTID))
                {
                    throw new Exception("此岗位下已关联员工，不能撤消！");
                }
                entity.EDITSTATE = sourceEntity.EDITSTATE;
                entity.CHECKSTATE = sourceEntity.CHECKSTATE;
                entity.UPDATEUSERID = sourceEntity.UPDATEUSERID;
                entity.UPDATEDATE = sourceEntity.UPDATEDATE;
                dal.Update(entity);

                WorkflowUtility.CallWorkflow("申请撤消工作流", entity);
                CacheManager.RemoveCache("T_HR_POST");

            }
            else
            {
                throw new Exception("没有找到对应实体！");
            }
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    string state = ((int)EditStates.Actived).ToString();
        //    string checkState = ((int)CheckStates.Approved).ToString();
        //    IQueryable<T_HR_POST> ents = from a in dal.GetObjects().Include("T_HR_POSTDICTIONARY")
        //                                    where a.CHECKSTATE == checkState && a.EDITSTATE == state
        //                                    select a;

        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_POST");
                SetPostFilter(ref filterString, ref queryParas, userID);
            }

            IQueryable<T_HR_POST> ents = from a in ListPOST.AsQueryable()
                                         where a.CHECKSTATE == checkState && a.EDITSTATE == state
                                         select a;

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            var ent = Utility.Pager<T_HR_POST>(ents, pageIndex, pageSize, ref pageCount);
            if (ent != null)
            {
                foreach (var temp in ent.ToList())
                {
                    temp.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                    temp.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
                }
            }

            return ents.Count() > 0 ? ents.ToArray() : null;
        }
    }
}
