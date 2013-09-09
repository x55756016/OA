using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Data.Objects;
using SMT.Foundation.Log;
using SMT.HRM.CustomModel;
namespace SMT.HRM.BLL
{
    public class PostBLL : BaseBll<T_HR_POST>, ILookupEntity, IOperate
    {
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

            IQueryable<T_HR_POST> ents = dal.GetObjects().Include("T_HR_POSTDICTIONARY");
            //dal.GetObjects().MergeOption = MergeOption.NoTracking;
            //DataContext.T_HR_DEPARTMENT.MergeOption = MergeOption.NoTracking;
            //DataContext.T_HR_POSTDICTIONARY.MergeOption = MergeOption.NoTracking;


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
            string canceledEditState = ((int)EditStates.Canceled).ToString(); //by xiedx 已撤销的也不能看。。
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
                filterString += " and EDITSTATE<>@" + paras.Count;
                paras.Add(canceledEditState);
            }
            else
            {
                filterString = "EDITSTATE<>@" + paras.Count;
                paras.Add(state);
                filterString += " and EDITSTATE<>@" + paras.Count;
                paras.Add(canceledEditState);
            }

            filterString += " and CHECKSTATE<>@" + paras.Count;
            paras.Add(checkState);

            IQueryable<T_HR_POST> ents = dal.GetObjects().Include("T_HR_DEPARTMENT").Include("T_HR_POSTDICTIONARY");
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
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                if (!string.IsNullOrEmpty(userID))
                {
                    SetOrganizationFilter(ref tempString, ref queryParas, userID, "T_HR_POST");
                    SetPostFilter(ref tempString, ref queryParas, userID);
                }

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
            }
            else
            {
                SetFilterWithflow("POSTID", "T_HR_POST", userID, ref strCheckState, ref tempString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }

            IQueryable<T_HR_POST> ents = dal.GetObjects().Include("T_HR_DEPARTMENT.T_HR_COMPANY").Include("T_HR_POSTDICTIONARY").Include("T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY");
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
            //if (ent != null)
            //{
            //    foreach (var temp in ent.ToList())
            //    {
            //        temp.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
            //        temp.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
            //    }
            //}

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
            var ents = from ent in dal.GetObjects().Include("T_HR_POSTDICTIONARY").Include("T_HR_DEPARTMENT").Include("T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_DEPARTMENT.T_HR_COMPANY")
                       where ent.POSTID == postID
                       select ent;
            //dal.GetObjects().MergeOption = MergeOption.NoTracking;
            //DataContext.T_HR_POSTDICTIONARY.MergeOption = MergeOption.NoTracking;
            //DataContext.T_HR_DEPARTMENT.MergeOption = MergeOption.NoTracking;
            //DataContext.T_HR_DEPARTMENTDICTIONARY.MergeOption = MergeOption.NoTracking;
            //DataContext.T_HR_COMPANY.MergeOption = MergeOption.NoTracking;
            T_HR_POST emp = ents.Count() > 0 ? ents.FirstOrDefault() : null;
            if (emp != null)
            {
                //加载公司
                //emp.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                //加载部门字典
                //emp.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
            }
            return emp;
        }
        public List<T_HR_POST> GetPostByDepartId(string departID)
        {
            var ents = from ent in dal.GetObjects().Include("T_HR_POSTDICTIONARY")
                       where ent.T_HR_DEPARTMENT.DEPARTMENTID == departID
                       select ent;
            return ents.ToList();
        }
        /// <summary>
        /// 获取某一部门下的所有岗位
        /// </summary>
        /// <param name="departID">部门ID</param>
        /// <param name="IsAll">是否获取所有部门</param>
        /// <returns>返回该部门下所有的岗位集合：包括生效和没生效</returns>
        public List<T_HR_POST> GetAllPostByDepartId(string departID,bool IsAll)
        {
            List<T_HR_POST> AllPosts = new List<T_HR_POST>();
            try
            {
                if (IsAll)
                {
                    AllPosts = this.GetPostByDepartId(departID);
                }
                else
                {
                    var ents = from ent in dal.GetObjects().Include("T_HR_POSTDICTIONARY")
                               where ent.T_HR_DEPARTMENT.DEPARTMENTID == departID
                               && ent.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString() && ent.EDITSTATE == Convert.ToInt32(EditStates.Actived).ToString()
                               select ent;

                    AllPosts = ents.Count() > 0 ? ents.ToList() : null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取某一部门下的岗位IDGetAllPostByDepartId出错：" + ex.ToString());
            }
            return AllPosts;
        }
        /// <summary>
        /// 添加岗位
        /// </summary>
        /// <param name="entity">岗位信息实例</param>
        public void PostAdd(T_HR_POST entity, ref string strMsg)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.T_HR_DEPARTMENT.DEPARTMENTID == entity.T_HR_DEPARTMENT.DEPARTMENTID
                && s.T_HR_POSTDICTIONARY.POSTCODE == entity.T_HR_POSTDICTIONARY.POSTCODE);
                if (temp != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                T_HR_POST ent = new T_HR_POST();
                Utility.CloneEntity<T_HR_POST>(entity, ent);
                //岗位字典
                ent.T_HR_POSTDICTIONARYReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                //部门
                ent.T_HR_DEPARTMENTReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENT", "DEPARTMENTID", entity.T_HR_DEPARTMENT.DEPARTMENTID);
                ent.CREATEDATE = System.DateTime.Now;
                //  dal.Add(ent);
                Add(ent);
                // WorkflowUtility.CallWorkflow("岗位添加申请审核工作流", ent);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " PostAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 修改岗位
        /// </summary>
        /// <param name="entity">岗位信息实例</param>
        public void PostUpdate(T_HR_POST entity, ref string strMsg)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.T_HR_DEPARTMENT.DEPARTMENTID == entity.T_HR_DEPARTMENT.DEPARTMENTID
                   && s.T_HR_POSTDICTIONARY.POSTCODE == entity.T_HR_POSTDICTIONARY.POSTCODE && s.POSTID != entity.POSTID);
                if (temp != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
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
                    ent.T_HR_POSTDICTIONARY = new T_HR_POSTDICTIONARY();
                    ent.T_HR_POSTDICTIONARY.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                    ent.T_HR_POSTDICTIONARYReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                    //部门
                    ent.T_HR_DEPARTMENT = new T_HR_DEPARTMENT();
                    ent.T_HR_DEPARTMENT.EntityKey =
                     new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENT", "DEPARTMENTID", entity.T_HR_DEPARTMENT.DEPARTMENTID);

                    ent.T_HR_DEPARTMENTReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENT", "DEPARTMENTID", entity.T_HR_DEPARTMENT.DEPARTMENTID);
                    ent.UPDATEDATE = System.DateTime.Now;
                    Update(ent);
                    //DataContext.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Modified);

                    //DataContext.SaveChanges();
                    //如果审核状态为审核通过则添加岗位历史
                    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        PostHistoryBLL phbll = new PostHistoryBLL();
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
                        //if (entity.T_HR_POSTDICTIONARY != null)
                        //{
                        //    postHis.T_HR_POSTDICTIONARYReference.EntityKey =
                        // new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                        //}
                        postHis.T_HR_POSTDICTIONARY = new T_HR_POSTDICTIONARY();
                        postHis.T_HR_POSTDICTIONARY.POSTDICTIONARYID = entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID;
                        postHis.REUSEDATE = DateTime.Now;
                        if (entity.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            postHis.CANCELDATE = DateTime.Now;
                        }
                        //  DataContext.AddObject("T_HR_POSTHISTORY", postHis);
                        phbll.PostHistoryAdd(postHis);
                        new CompanyBLL().EditVersion("岗位");
                    }

                    //   DataContext.SaveChanges();
                    //WorkflowUtility.CallWorkflow("岗位变更申请审核工作流", ent);
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " PostUpdate:" + ex.Message);
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
            string stateActived = Convert.ToInt32(EditStates.Actived).ToString();
            string statePendingCanceled = Convert.ToInt32(EditStates.PendingCanceled).ToString();
            var ents = from ent in dal.GetObjects()
                       where ent.T_HR_DEPARTMENT.DEPARTMENTID == departID && (ent.EDITSTATE == stateActived || ent.EDITSTATE == statePendingCanceled)
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
        /// 获取岗位的空缺人数
        /// </summary>
        /// <param name="positionID"></param>
        /// <returns></returns>
        public int GetPostNumber(string positionID)
        {
            int number = 0;

            var ent = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                      where c.T_HR_POST.POSTID == positionID && c.CHECKSTATE == "2" && c.EDITSTATE == "1" && c.T_HR_EMPLOYEE.EMPLOYEESTATE != "3"
                      select c;
            var postnumber = from b in dal.GetObjects()
                             where b.POSTID == positionID
                             select b.POSTNUMBER;
            if (postnumber.FirstOrDefault() != null)
            {
                number = Convert.ToInt32(postnumber.FirstOrDefault().Value);
            }
            return number - ent.Count();

        }

        /// <summary>
        /// 返回在岗人数
        /// </summary>
        /// <param name="positionID"></param>
        /// <returns></returns>
        public int GetOnPostNumber(string positionID)
        {
            int number = 0;

            var ent = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                      where c.T_HR_POST.POSTID == positionID && c.CHECKSTATE == "2" && c.EDITSTATE == "1" && c.T_HR_EMPLOYEE.EMPLOYEESTATE != "3"
                      select c;
            var postnumber = from b in dal.GetObjects()
                             where b.POSTID == positionID
                             select b.POSTNUMBER;
            if (ent.Count() >0)
            {
                number = ent.Count();
            }
            return number;

        }

        /// <summary>
        /// 删除部门岗位信息
        /// </summary>
        /// <param name="id">岗位ID</param>
        public void PostDelete(string id, ref string strMsg)
        {
            var entitys = (from ent in dal.GetObjects()
                           where ent.POSTID == id
                           select ent);
            if (entitys.Count() > 0)
            {
                var entity = entitys.FirstOrDefault();
                if (IsExistChilds(id))
                {
                    //  throw new Exception("此岗位下已关联员工，不能删除！");
                    strMsg = "POSTHASEMPLOYEE";
                    return;
                }
                // dal.Delete(entity);
                Delete(entity);
                new CompanyBLL().EditVersion("岗位");
            }
            else
            {
                // throw new Exception("没有找到对应实体！");
                strMsg = "NOTFOUND";
            }

        }
        /// <summary>
        /// 撤消岗位信息
        /// </summary>
        /// <param name="sourceEntity">岗位实体</param>
        public void PostCancel(T_HR_POST sourceEntity, ref string strMsg)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.POSTID == sourceEntity.POSTID
                               select ent);

                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    if (IsExistChilds(sourceEntity.POSTID))
                    {
                        //  throw new Exception("此岗位下已关联员工，不能撤消！");
                        strMsg = "POSTHASEMPLOYEE";
                        return;
                    }
                    entity.EDITSTATE = sourceEntity.EDITSTATE;
                    entity.CHECKSTATE = sourceEntity.CHECKSTATE;
                    entity.UPDATEUSERID = sourceEntity.UPDATEUSERID;
                    entity.UPDATEDATE = System.DateTime.Now;
                    // dal.Update(entity);
                    Update(entity);
                    new CompanyBLL().EditVersion("岗位");
                    //  WorkflowUtility.CallWorkflow("申请撤消工作流", entity);

                }
                else
                {
                    //throw new Exception("没有找到对应实体！");
                    strMsg = "NOTFOUND";
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " PostCancel:" + ex.Message);
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

            IQueryable<T_HR_POST> ents = from a in dal.GetObjects().Include("T_HR_POSTDICTIONARY").Include("T_HR_DEPARTMENT")
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
        /// <summary>
        /// 根据ID判断是否是上级岗位
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        public bool IsFatherPost(string postID)
        {
            var ent = from c in dal.GetObjects<T_HR_POST>()
                      where c.FATHERPOSTID == postID && c.EDITSTATE == "1"
                      select c;
            return ent.Count() > 0 ? true : false;

        }
        /// <summary>
        /// 获取指定时间后更新的岗位
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public IQueryable<T_HR_POST> GetPostWithSpecifiedTime(string startDate)
        {
            DateTime start;
            bool flag;
            flag = DateTime.TryParse(startDate, out start);
            if (flag)
            {
                IQueryable<T_HR_POST> ents = from c in dal.GetObjects().Include("T_HR_DEPARTMENT").Include("T_HR_POSTDICTIONARY")
                                             where c.UPDATEDATE >= start || c.CREATEDATE >= start
                                             select c;

                return ents;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取全部可用的岗位视图
        /// </summary>
        /// <returns></returns>
        public IQueryable<SMT.HRM.CustomModel.V_POST> GetAllPostView(string userID)
        {
            #region 根据权限过滤数据
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_POST", "3");
                SetPostFilter(ref filterString, ref paras, userID);
            }
            #endregion

            IQueryable<T_HR_POST> ents = from c in dal.GetObjects()
                                         select c;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            var postViews = from c in ents
                            select new SMT.HRM.CustomModel.V_POST
                            {
                                POSTID = c.POSTID,
                                POSTNAME = c.T_HR_POSTDICTIONARY.POSTNAME,
                                // POSTDICTIONARYID = c.T_HR_POSTDICTIONARY.POSTDICTIONARYID,
                                DEPARTMENTID = c.T_HR_DEPARTMENT.DEPARTMENTID,
                                //  DEPARTMENTNAME = c.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                CHECKSTATE = c.CHECKSTATE,
                                EDITSTATE = c.EDITSTATE,
                                //  COMPANYID = c.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                //   CNAME = c.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                FATHERPOSTID = c.FATHERPOSTID
                            };
            return postViews;
        }

        /// <summary>
        /// 获取指定时间后更新的岗位视图
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<SMT.HRM.CustomModel.V_POST> GetPostViewByDateAndUser(string startDate, string userID)
        {
            #region 根据权限过滤数据
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_POST", "3");
                SetPostFilter(ref filterString, ref paras, userID);
            }
            #endregion
            IQueryable<T_HR_POST> ents = from c in dal.GetObjects()
                                         select c;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            DateTime start;
            bool flag;
            flag = DateTime.TryParse(startDate, out start);
            if (!flag)
                return null;
            var postViews = from c in ents
                            where c.UPDATEDATE >= start
                            select new SMT.HRM.CustomModel.V_POST
                            {
                                POSTID = c.POSTID,
                                POSTNAME = c.T_HR_POSTDICTIONARY.POSTNAME,
                                // POSTDICTIONARYID = c.T_HR_POSTDICTIONARY.POSTDICTIONARYID,
                                DEPARTMENTID = c.T_HR_DEPARTMENT.DEPARTMENTID,
                                //  DEPARTMENTNAME = c.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                CHECKSTATE = c.CHECKSTATE,
                                EDITSTATE = c.EDITSTATE,
                                //  COMPANYID = c.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                //   CNAME = c.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                FATHERPOSTID = c.FATHERPOSTID
                            };

            return postViews;
        }

        /// <summary>
        /// 根据实体权限获取岗位视图
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="perm"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IQueryable<SMT.HRM.CustomModel.V_POST> GetPostView(string userID, string perm, string entity)
        {
            #region 根据权限过滤数据
            if (string.IsNullOrEmpty(perm))
            {
                perm = "3";
            }
            if (string.IsNullOrEmpty(entity))
            {
                entity = "T_HR_POST";
            }
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, entity, perm);
                SetPostFilter(ref filterString, ref paras, userID);
            }
            #endregion

            IQueryable<T_HR_POST> ents = from c in dal.GetObjects()
                                         select c;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            var postViews = from c in ents
                            select new SMT.HRM.CustomModel.V_POST
                            {
                                POSTID = c.POSTID,
                                POSTNAME = c.T_HR_POSTDICTIONARY.POSTNAME,
                                // POSTDICTIONARYID = c.T_HR_POSTDICTIONARY.POSTDICTIONARYID,
                                DEPARTMENTID = c.T_HR_DEPARTMENT.DEPARTMENTID,
                                //  DEPARTMENTNAME = c.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                CHECKSTATE = c.CHECKSTATE,
                                EDITSTATE = c.EDITSTATE,
                                //  COMPANYID = c.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                //   CNAME = c.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                FATHERPOSTID = c.FATHERPOSTID
                            };
            return postViews;
        }

        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var post = (from c in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT").Include("T_HR_POSTDICTIONARY")
                            where c.POSTID == EntityKeyValue
                            select c).FirstOrDefault();
                if (post != null)
                {
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        if (post.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            post.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                        }
                        else
                        {
                            post.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                        }
                    }
                    if (CheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        if (post.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            post.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                        }
                        else
                        {
                            post.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        }
                    }
                    post.CHECKSTATE = CheckState;
                    PostUpdate(post, ref strMsg);
                    if (string.IsNullOrEmpty(strMsg))
                    {
                        i = 1;
                    }
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
