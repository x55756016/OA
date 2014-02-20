using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;
using System.Data;
using SMT.HRM.BLL.Report;
using SMT.HRM.IMServices.IMServiceWS;
namespace SMT.HRM.BLL
{
    public class EmployeePostChangeBLL : BaseBll<T_HR_EMPLOYEEPOSTCHANGE>, IOperate
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
        public IQueryable<T_HR_EMPLOYEEPOSTCHANGE> EmployeePostChangePaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEEPOSTCHANGE");

                if (!string.IsNullOrEmpty(CheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(CheckState);
                }
            }
            else
            {
                SetFilterWithflow("POSTCHANGEID", "T_HR_EMPLOYEEPOSTCHANGE", userID, ref CheckState, ref  filterString, ref queryParas);

                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }
            IQueryable<T_HR_EMPLOYEEPOSTCHANGE> ents = dal.GetObjects().Include("T_HR_EMPLOYEE");

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEEPOSTCHANGE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
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
        public IQueryable<V_EMPLOYEEPOSTCHANGE> EmployeePostChangeViewPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            string approved = Convert.ToInt32(CheckStates.Approved).ToString();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEEPOSTCHANGE");
            SetFilterWithflow("POSTCHANGEID", "T_HR_EMPLOYEEPOSTCHANGE", userID, ref CheckState, ref  filterString, ref queryParas);
            if (!string.IsNullOrEmpty(CheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " and ";
                }
                filterString += "CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(CheckState);
            }
            IQueryable<V_EMPLOYEEPOSTCHANGE> ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEE")
                                                    join d in dal.GetObjects<T_HR_COMPANY>() on c.FROMCOMPANYID equals d.COMPANYID into fromCompanyTmp
                                                    from d in fromCompanyTmp.DefaultIfEmpty()
                                                    join e in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY") on c.FROMDEPARTMENTID equals e.DEPARTMENTID into fromDepTmp
                                                    from e in fromDepTmp.DefaultIfEmpty()
                                                    join f in dal.GetObjects<T_HR_POST>().Include("T_HR_POSTDICTIONARY") on c.FROMPOSTID equals f.POSTID into fromDostTmp
                                                    from f in fromDostTmp.DefaultIfEmpty()
                                                    join g in dal.GetObjects<T_HR_COMPANY>() on c.TOCOMPANYID equals g.COMPANYID into toCompanyTmp
                                                    from g in toCompanyTmp.DefaultIfEmpty()
                                                    join h in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY") on c.TODEPARTMENTID equals h.DEPARTMENTID into toDepTmp
                                                    from h in toDepTmp.DefaultIfEmpty()

                                                    join i in dal.GetObjects<T_HR_POST>().Include("T_HR_POSTDICTIONARY") on c.TOPOSTID equals i.POSTID into toPostTmp
                                                    from i in toPostTmp.DefaultIfEmpty()

                                                    where c.CHECKSTATE == approved//审核通过
                                                    select new V_EMPLOYEEPOSTCHANGE
                                                    {
                                                        EMPLOYEEPOSTCHANGE = c,
                                                        FROMCOMPANY = d.CNAME,
                                                        FROMDEPARTMENT = e.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                        FROMPOST = f.T_HR_POSTDICTIONARY.POSTNAME,
                                                        TOCOMPANY = g.CNAME,
                                                        TODEPARTMENT = h.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                        TOPOST = i.T_HR_POSTDICTIONARY.POSTNAME,
                                                        ENDDATE = c.ENDDATE,
                                                        CREATEUSERID = c.CREATEUSERID,
                                                        OWNERCOMPANYID = c.OWNERCOMPANYID,
                                                        OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                                                        OWNERID = c.OWNERID,
                                                        OWNERPOSTID = c.OWNERPOSTID

                                                    };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_EMPLOYEEPOSTCHANGE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }




        /// <summary>
        /// 添加员工异动记录
        /// </summary>
        /// <param name="entity">员工异动实体</param>
        public void EmployeePostChangeAdd(T_HR_EMPLOYEEPOSTCHANGE entity, ref string strMsg)
        {
            try
            {
                var tmp = from c in dal.GetObjects()
                          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && c.TOPOSTID == entity.TOPOSTID
                          && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1")
                          select c;
                if (tmp.Count() > 0)
                {
                    //throw new Exception("EXIST");
                    strMsg = "已经存在该异动";
                    return;
                }
                var ents = from c in dal.GetObjects()
                           where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && c.ISAGENCY == entity.ISAGENCY && c.ISAGENCY == "0"
                           && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1")
                           select c;
                if (ents.Count() > 0)
                {
                    //throw new Exception("EXIST");
                    strMsg = "已经存在主岗位异动，还未审核完毕，不能创建此次异动";
                    return;
                }
                var epost = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                            where ep.T_HR_POST.POSTID == entity.TOPOSTID && ep.EDITSTATE == "1" && ep.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && ep.POSTLEVEL == entity.TOPOSTLEVEL
                            select ep;
                if (epost.Count() > 0)
                {
                    strMsg = "不能重复担任同一岗位";
                    return;
                }
                T_HR_EMPLOYEEPOSTCHANGE ent = new T_HR_EMPLOYEEPOSTCHANGE();
                Utility.CloneEntity<T_HR_EMPLOYEEPOSTCHANGE>(entity, ent);
                ent.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);

                //dal.Add(ent);
                Add(ent, ent.CREATEUSERID);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeePostChangeAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 更新员工异动记录
        /// </summary>
        /// <param name="entity">员工异动实体</param>
        public void EmployeePostChangeUpdate(T_HR_EMPLOYEEPOSTCHANGE entity, ref string strMsg)
        {
            try
            {
                #region
                //var tmp = from c in dal.GetObjects()
                //          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && c.TOPOSTID == entity.TOPOSTID
                //          && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1") && c.POSTCHANGEID != entity.POSTCHANGEID
                //          select c;
                //if (tmp.Count() > 0)
                //{
                //    //throw new Exception("EXIST");
                //    strMsg = "EXIST";
                //    return;
                //}
                //var ent = dal.GetObjects().FirstOrDefault(s => s.POSTCHANGEID == entity.POSTCHANGEID);
                //if (ent != null)
                //{
                //    Utility.CloneEntity<T_HR_EMPLOYEEPOSTCHANGE>(entity, ent);
                //    if (entity.T_HR_EMPLOYEE != null)
                //    {
                //        ent.T_HR_EMPLOYEEReference.EntityKey =
                //                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                //    }
                //    dal.Update(ent);
                //}
                #endregion
                var tmp = from c in dal.GetObjects()
                          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && c.TOPOSTID == entity.TOPOSTID
                          && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1") && c.POSTCHANGEID != entity.POSTCHANGEID
                          select c;
                if (tmp.Count() > 0)
                {
                    //throw new Exception("EXIST");
                    strMsg = "EXIST";
                    return;
                }

                entity.EntityKey =
                                     new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEPOSTCHANGE", "POSTCHANGEID", entity.POSTCHANGEID);
                if (entity.T_HR_EMPLOYEE != null)
                {
                    entity.T_HR_EMPLOYEEReference.EntityKey =
                                       new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    entity.T_HR_EMPLOYEE.EntityKey =
                                      new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                }
                //dal.Update(entity);
                Update(entity, entity.CREATEUSERID);


            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeePostChangeUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 为员工入职添加异动记录
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="strMsg"></param>
        public void AddEmployeePostChangeForEntry(T_HR_EMPLOYEEPOSTCHANGE entity)
        {
            try
            {
                //var tmp = from c in dal.GetObjects()
                //          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && c.TOPOSTID == entity.TOPOSTID
                //          && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1")
                //          select c;
                //if (tmp.Count() > 0)
                //{
                //    return;
                //}
                T_HR_EMPLOYEEPOSTCHANGE ent = new T_HR_EMPLOYEEPOSTCHANGE();
                Utility.CloneEntity<T_HR_EMPLOYEEPOSTCHANGE>(entity, ent);
                ent.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                Tracer.Debug("AddEmployeePostChangeForEntry" + ex.Message);
            }
        }
        /// <summary>
        /// 删除员工异动记录
        /// </summary>
        /// <param name="employeePostChangeIDs">员工异动记录ID组</param>
        /// <returns></returns>
        public int EmployeePostChangeDelete(string[] employeePostChangeIDs)
        {
            foreach (string id in employeePostChangeIDs)
            {
                var ent = dal.GetObjects().FirstOrDefault(s => s.POSTCHANGEID == id);
                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    DeleteMyRecord(ent);
                }
            }
            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据员工记录ID获取员工异动记录信息
        /// </summary>
        /// <param name="employeePostChangeID">员工异动记录ID</param>
        /// <returns>返回员工异动记录信息</returns>
        public T_HR_EMPLOYEEPOSTCHANGE GetEmployeePostChangeByID(string employeePostChangeID)
        {
            return dal.GetObjects().Include("T_HR_EMPLOYEE").FirstOrDefault(s => s.POSTCHANGEID == employeePostChangeID);
        }

        public string EmployeePostChange(T_HR_EMPLOYEEPOSTCHANGE entity)
        {
            string strMessage = "";
            try
            {
                EmployeePostBLL epostbll = new EmployeePostBLL();
                EmployeeBLL ebll = new EmployeeBLL();
                bool flag = true;
                //查询员工
                T_HR_EMPLOYEE employee = new T_HR_EMPLOYEE();
                var employees = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                where c.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID
                                select c;
                if (employees != null)
                {

                    employee = employees.FirstOrDefault();
                    //判断异动类型 且不是代理
                    if (entity.POSTCHANGCATEGORY == "0" && entity.ISAGENCY == "0")
                    {
                        //内部异动
                        employee.OWNERPOSTID = entity.TOPOSTID;
                        employee.OWNERDEPARTMENTID = entity.TODEPARTMENTID;
                    }
                    else if (entity.POSTCHANGCATEGORY == "1" && entity.ISAGENCY == "0")
                    {
                        //外部异动
                        employee.OWNERDEPARTMENTID = entity.TODEPARTMENTID;
                        employee.OWNERPOSTID = entity.TOPOSTID;
                        employee.OWNERCOMPANYID = entity.TOCOMPANYID;
                    }
                }
                else
                {
                    strMessage = "EMPLOYEENOTFOUND";
                    return strMessage;
                }
                //旧职位 
                var eOldPosts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                where ep.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && ep.ISAGENCY == "0" && ep.EDITSTATE == "1"
                                select ep;
                T_HR_EMPLOYEEPOST eOldpost = new T_HR_EMPLOYEEPOST();
                if (eOldPosts.Count() > 0)
                {
                    eOldpost = eOldPosts.FirstOrDefault();
                    //   eOldpost.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                }
                //else
                //{
                //    eOldpost = null;
                //}

                //新职位

                //var eNewPosts = from en in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                //                where en.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID
                //                   && en.T_HR_POST.POSTID == entity.TOPOSTID && en.CHECKSTATE == "0" && en.EDITSTATE == "0"
                //                select en;
                var eNewPosts = from en in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                where en.EMPLOYEEPOSTID == entity.EMPLOYEEPOSTID
                                select en;
                T_HR_EMPLOYEEPOST epost = new T_HR_EMPLOYEEPOST();
                if (eNewPosts.Count() > 0)
                {
                    epost = eNewPosts.FirstOrDefault();
                    //epost.T_HR_POST = new T_HR_POST();
                    //epost.T_HR_POST.POSTID = entity.TOPOSTID;
                    //epost.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                    //epost.T_HR_EMPLOYEE.EMPLOYEEID = entity.T_HR_EMPLOYEE.EMPLOYEEID;
                    epost.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    epost.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    // 岗位是代理
                    if (epost.ISAGENCY == "1")
                    {
                        flag = false;
                    }

                }


                if (flag == true)
                {
                    string strMsg = "";
                    //更改员工的所属公司 部门 岗位信息
                    ebll.EmployeeUpdate(employee, ref strMsg);
                    ////如果不是兼职 将旧职位设为无效
                    eOldpost.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    // epostbll.Update(eOldpost);
                    dal.UpdateFromContext(eOldpost);
                    dal.SaveContextChanges();

                    //不是兼职 将旧职位删除
                    // epostbll.Delete(eOldpost);

                    //添加异动记录
                    var employeepostchange = from pc in dal.GetObjects<T_HR_EMPLOYEEPOSTCHANGE>()
                                             where pc.EMPLOYEEPOSTID == eOldpost.EMPLOYEEPOSTID
                                             select pc;
                    if (employeepostchange.Count() > 0)
                    {
                        T_HR_EMPLOYEEPOSTCHANGE epchange = employeepostchange.FirstOrDefault();
                        if (entity.CHANGEDATE != null)
                        {
                            epchange.ENDDATE = Convert.ToDateTime(entity.CHANGEDATE);
                        }
                        epchange.ENDREASON = entity.POSTCHANGREASON;
                        dal.UpdateFromContext(epchange);
                        dal.SaveContextChanges();
                    }
                }
                //新的职位生效
                // epostbll.Update(epost);
                dal.UpdateFromContext(epost);
                dal.SaveContextChanges();
                //不是代理岗位 ，修改薪资档案
                //if (flag == true)
                //{
                //    SalaryArchiveBLL sbll = new SalaryArchiveBLL();
                //    sbll.CreateEmployeeArchiveByEmployee(employee);
                //}
                return "SUCESSED";
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeePostChangeUpdate:" + ex.Message);
                return ex.Message.ToString();

            }
        }
        /// <summary>
        /// 根据员工ID获取最近的非代理异动
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEEPOSTCHANGE GetLastChangeByEmployeeID(string employeeID)
        {
            T_HR_EMPLOYEEPOSTCHANGE ent = null;
            var ents = from c in dal.GetObjects()
                       where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && c.ISAGENCY == "0"
                       select c;
            if (ents.Count() > 0)
            {
                ent = ents.OrderBy(s => s.CHANGEDATE).FirstOrDefault();
            }
            return ent;
        }

        /// <summary>
        /// 服务引擎调用，更新单据的状态
        /// </summary>
        /// <param name="strEntityName">实体名</param>
        /// <param name="EntityKeyName">主键名</param>
        /// <param name="EntityKeyValue">主键id</param>
        /// <param name="CheckState">审核状态</param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string StrMessage = "";
                T_HR_EMPLOYEEPOST EmployeePost = new T_HR_EMPLOYEEPOST();
                dal.BeginTransaction();//事务操作开始标示，和后面的CommitTransaction对应，没有CommitTransaction那两方法之间的数据操作，都不会生效

                #region 员工异动审核时判断该员工所异动到的岗位是否生效，如果没有生效则不能审核通过
                if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString() || CheckState == Convert.ToInt32(CheckStates.Approving).ToString())
                {
                    var ent = dal.GetObjects().FirstOrDefault(s => s.POSTCHANGEID == EntityKeyValue);
                    PostBLL post = new PostBLL();


                    if (ent != null && post.GetPostById(ent.TOPOSTID) != null && post.GetPostById(ent.TOPOSTID).EDITSTATE != "1")//所找出的岗位没有生效，则返回
                    {
                        SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " ---岗位id:" + ent.TOPOSTID + "生效状态" + post.GetPostById(ent.TOPOSTID).EDITSTATE);
                        return i;
                    }
                }
                #endregion

                //根据异动表ID查找员工异动表信息
                var tmp = (from c in dal.GetObjects<T_HR_EMPLOYEEPOSTCHANGE>()
                           where c.POSTCHANGEID == EntityKeyValue
                           select new
                           {
                               EMPLOYEEPOSTCHANGE = c,
                               EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID

                           }).FirstOrDefault();
                if (tmp != null)
                {
                    //员工异动表
                    var employeePostChange = tmp.EMPLOYEEPOSTCHANGE;

                    

                    //员工ID
                    string employeeID = tmp.EMPLOYEEID;
                    //异动审核状态
                    employeePostChange.CHECKSTATE = CheckState;
                    //更改时间为当前时间
                    employeePostChange.UPDATEDATE = DateTime.Now;
                    //更新异动表
                    dal.UpdateFromContext(employeePostChange);

                    //审核通过 执行的业务
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        #region 更新相关信息
                        bool flag = true;

                        var employee = (from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                        where c.EMPLOYEEID == employeeID
                                        select c).FirstOrDefault();

                        var employeePost = (from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                            where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && ep.T_HR_POST.POSTID == employeePostChange.FROMPOSTID
                                            select ep).FirstOrDefault();

                        if (employeePost != null)
                        {
                            if (employeePost.ISAGENCY == "0" || employeePostChange.ISAGENCY == "1")
                            {
                                #region 主岗位异动
                                //更具员工ID查询员工基本信息表

                                if (employee != null)
                                {
                                    //判断异动类型 且不是代理
                                    if (employeePostChange.POSTCHANGCATEGORY == "0" && employeePostChange.ISAGENCY == "0")
                                    {
                                        //内部异动
                                        employee.OWNERPOSTID = employeePostChange.TOPOSTID;
                                        employee.OWNERDEPARTMENTID = employeePostChange.TODEPARTMENTID;
                                        ////更新员工表里的岗位等级
                                        //if (employeePostChange.TOPOSTLEVEL != null)
                                        //{
                                        //    employee.EMPLOYEELEVEL = employeePostChange.TOPOSTLEVEL.ToString();
                                        //}
                                    }
                                    else if (employeePostChange.POSTCHANGCATEGORY == "1" && employeePostChange.ISAGENCY == "0")
                                    {
                                        //外部异动
                                        employee.OWNERDEPARTMENTID = employeePostChange.TODEPARTMENTID;
                                        employee.OWNERPOSTID = employeePostChange.TOPOSTID;
                                        employee.OWNERCOMPANYID = employeePostChange.TOCOMPANYID;
                                    }
                                }
                                //旧职位 
                                var eOldPost = (from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && ep.ISAGENCY == "0" && ep.EDITSTATE == "1"
                                                select ep).FirstOrDefault();


                                var epost = (from en in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                             where en.EMPLOYEEPOSTID == employeePostChange.EMPLOYEEPOSTID
                                             select en).FirstOrDefault();
                                if (epost != null)
                                {
                                    EmployeePost = epost;//给即时通讯接口调用
                                    epost.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                                    epost.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                                    // 岗位是代理
                                    if (epost.ISAGENCY == "1")
                                    {
                                        flag = false;
                                    }

                                }
                                if (flag == true)
                                {
                                    //更改员工的所属公司 部门 岗位信息
                                    dal.UpdateFromContext(employee);
                                    #region  主岗位异动，个人活动经费随行
                                    SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient client = new SaaS.BLLCommonServices.FBServiceWS.FBServiceClient();
                                    string message = "";
                                    try
                                    {
                                        SMT.SaaS.BLLCommonServices.FBServiceWS.T_HR_EMPLOYEEPOSTCHANGE postChange = new SaaS.BLLCommonServices.FBServiceWS.T_HR_EMPLOYEEPOSTCHANGE
                                        {
                                            POSTCHANGEID = tmp.EMPLOYEEPOSTCHANGE.POSTCHANGEID,
                                            POSTCHANGCATEGORY = tmp.EMPLOYEEPOSTCHANGE.POSTCHANGCATEGORY,
                                            POSTCHANGREASON = tmp.EMPLOYEEPOSTCHANGE.POSTCHANGREASON,
                                            FROMCOMPANYID = tmp.EMPLOYEEPOSTCHANGE.FROMCOMPANYID,
                                            TOCOMPANYID = tmp.EMPLOYEEPOSTCHANGE.TOCOMPANYID,
                                            FROMDEPARTMENTID = tmp.EMPLOYEEPOSTCHANGE.FROMDEPARTMENTID,
                                            TODEPARTMENTID = tmp.EMPLOYEEPOSTCHANGE.TODEPARTMENTID,
                                            FROMPOSTID = tmp.EMPLOYEEPOSTCHANGE.FROMPOSTID,
                                            TOPOSTID = tmp.EMPLOYEEPOSTCHANGE.TOPOSTID,
                                            FROMPOSTLEVEL = tmp.EMPLOYEEPOSTCHANGE.FROMPOSTLEVEL,
                                            TOPOSTLEVEL = tmp.EMPLOYEEPOSTCHANGE.TOPOSTLEVEL,
                                            FROMSALARYLEVEL = tmp.EMPLOYEEPOSTCHANGE.TOSALARYLEVEL,
                                            TOSALARYLEVEL = tmp.EMPLOYEEPOSTCHANGE.TOSALARYLEVEL,
                                            EMPLOYEEPOSTID = tmp.EMPLOYEEPOSTCHANGE.EMPLOYEEPOSTID,
                                            ISAGENCY = tmp.EMPLOYEEPOSTCHANGE.ISAGENCY,
                                            EMPLOYEECODE = tmp.EMPLOYEEPOSTCHANGE.EMPLOYEECODE,
                                            EMPLOYEENAME = tmp.EMPLOYEEPOSTCHANGE.EMPLOYEENAME,
                                            CHANGEDATE = tmp.EMPLOYEEPOSTCHANGE.CHANGEDATE,
                                            ENDDATE = tmp.EMPLOYEEPOSTCHANGE.ENDDATE,
                                            ENDREASON = tmp.EMPLOYEEPOSTCHANGE.ENDREASON,
                                            CHECKSTATE = tmp.EMPLOYEEPOSTCHANGE.CHECKSTATE,
                                            REMARK = tmp.EMPLOYEEPOSTCHANGE.REMARK,
                                            OWNERID = tmp.EMPLOYEEPOSTCHANGE.OWNERID,
                                            OWNERPOSTID = tmp.EMPLOYEEPOSTCHANGE.OWNERPOSTID,
                                            OWNERDEPARTMENTID = tmp.EMPLOYEEPOSTCHANGE.OWNERDEPARTMENTID,
                                            OWNERCOMPANYID = tmp.EMPLOYEEPOSTCHANGE.OWNERCOMPANYID,
                                            CREATEUSERID = tmp.EMPLOYEEPOSTCHANGE.CREATEUSERID,
                                            CREATEPOSTID = tmp.EMPLOYEEPOSTCHANGE.CREATEPOSTID,
                                            CREATEDEPARTMENTID = tmp.EMPLOYEEPOSTCHANGE.CREATEDEPARTMENTID,
                                            CREATECOMPANYID = tmp.EMPLOYEEPOSTCHANGE.CREATECOMPANYID,
                                            CREATEDATE = tmp.EMPLOYEEPOSTCHANGE.CREATEDATE,
                                            UPDATEUSERID = tmp.EMPLOYEEPOSTCHANGE.UPDATEUSERID,
                                            UPDATEDATE = tmp.EMPLOYEEPOSTCHANGE.UPDATEDATE
                                        };
                                        postChange.T_HR_EMPLOYEE = new SaaS.BLLCommonServices.FBServiceWS.T_HR_EMPLOYEE();
                                        postChange.T_HR_EMPLOYEE.EMPLOYEEID = tmp.EMPLOYEEID;

                                        //调用个人活动经费迁移接口
                                        client.HRPersonPostChanged(postChange, ref message);
                                        SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + "预算 返回结果:" + message);
                                    }
                                    catch (Exception e)
                                    {
                                        SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + "预算实体赋值错误:" + e.ToString());
                                    }


                                    #endregion
                                    ////如果不是兼职 将旧职位设为无效
                                    if (eOldPost != null)
                                    {
                                        eOldPost.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                                    }
                                    dal.UpdateFromContext(eOldPost);
                                }
                                dal.UpdateFromContext(epost);
                                #endregion
                            }
                            else
                            {
                                #region 兼职岗位异动
                                employeePost.EDITSTATE = "0";//原岗位 改为0未生效
                                employeePost.CHECKSTATE = "2";
                                dal.UpdateFromContext(employeePost);

                                var toPost = dal.GetObjects<T_HR_POST>().Where(t => t.POSTID == employeePostChange.TOPOSTID).FirstOrDefault();
                                employeePost.T_HR_POST = toPost;
                                dal.UpdateFromContext(employeePost);

                                var toemployeePost = (from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                    where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && ep.T_HR_POST.POSTID == employeePostChange.TOPOSTID
                                                    select ep).FirstOrDefault();
                                if (toemployeePost != null)
                                {
                                    toemployeePost.ISAGENCY = "1"; //异动后岗位 为兼职岗位
                                    toemployeePost.EDITSTATE = "1"; //改为生效
                                    dal.UpdateFromContext(toemployeePost);
                                }

                                #endregion
                            }
                        }
                        #endregion

                        #region 员工异动报表服务同步 weirui 2012-7-9

                        try
                        {
                            T_HR_EMPLOYEECHANGEHISTORY employeeEntity = new T_HR_EMPLOYEECHANGEHISTORY();

                            employeeEntity.RECORDID = Guid.NewGuid().ToString();
                            //员工ID
                            //employeeEntity.T_HR_EMPLOYEE.EMPLOYEEID = employeePostChange.T_HR_EMPLOYEE.EMPLOYEEID;
                            employeeEntity.T_HR_EMPLOYEEReference.EntityKey =
                                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", employeeID);
                            //员工姓名
                            employeeEntity.EMPOLYEENAME = employeePostChange.EMPLOYEENAME;
                            //指纹编号
                            employeeEntity.FINGERPRINTID = employee.FINGERPRINTID;
                            //0.入职1.异动2.离职3.薪资级别变更4.签订合同
                            employeeEntity.FORMTYPE = "1";
                            //记录原始单据id（员工入职表ID）
                            employeeEntity.FORMID = employeePostChange.POSTCHANGEID;
                            //主岗位非主岗位
                            employeeEntity.ISMASTERPOSTCHANGE = employeePostChange.ISAGENCY;
                            //包括 异动类型及离职类型 0:1=异动类型：离职类型
                            employeeEntity.CHANGETYPE = "0";
                            //异动时间
                            employeeEntity.CHANGETIME = DateTime.Now;
                            //异动原因
                            employeeEntity.CHANGEREASON = employeePostChange.POSTCHANGREASON;
                            //异动前岗位id
                            employeeEntity.OLDPOSTID = employeePostChange.FROMPOSTID;
                            //根据异动前岗位ID查找岗位表，查询岗位字典ID
                            //var oldPostInfo = dal.GetObjects<T_HR_POST>().FirstOrDefault(s => s.POSTID == employeeEntity.OLDPOSTID);

                            //连表查询，异动前岗位名称
                            var oldPostInfo = (from c in dal.GetObjects<T_HR_POST>()
                                               join m in dal.GetObjects<T_HR_POSTDICTIONARY>()
                                               on c.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals m.POSTDICTIONARYID
                                               where c.POSTID == employeePostChange.FROMPOSTID
                                               select new
                                               {
                                                   OLDPOSTNAME = m.POSTNAME
                                               }).FirstOrDefault();
                            if (oldPostInfo != null)
                            {
                                //根据岗位字典ID查询字典表，查询岗位名称
                                //var oldPostDictionary = dal.GetObjects<T_HR_POSTDICTIONARY>().FirstOrDefault(s => s.POSTDICTIONARYID == oldPostInfo.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                                //异动前岗位名称
                                employeeEntity.OLDPOSTNAME = oldPostInfo.OLDPOSTNAME;
                            }

                            //异动前岗位级别
                            employeeEntity.OLDPOSTLEVEL = employeePostChange.FROMPOSTLEVEL.ToString();
                            //异动前薪资级别
                            employeeEntity.OLDSALARYLEVEL = employeePostChange.FROMSALARYLEVEL.ToString();

                            //异动前部门id
                            employeeEntity.OLDDEPARTMENTID = employeePostChange.FROMDEPARTMENTID;
                            //链表查询，异动前部门名称
                            //var oldDepartment = dal.GetObjects<T_HR_DEPARTMENT>().FirstOrDefault(s => s.DEPARTMENTID == employeeEntity.OLDDEPARTMENTID);
                            var oldDepartment = (from c in dal.GetObjects<T_HR_DEPARTMENT>()
                                                 join m in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>()
                                                 on c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals m.DEPARTMENTDICTIONARYID
                                                 where c.DEPARTMENTID == employeePostChange.FROMDEPARTMENTID
                                                 select new
                                                 {
                                                     OLDDEPARTMENTNAME = m.DEPARTMENTNAME
                                                 }).FirstOrDefault();
                            if (oldDepartment != null)
                            {
                                //根据部门字典ID查询部门字典表，查询部门名称
                                //var oldDepartmentDictionary = dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>().FirstOrDefault(s => s.DEPARTMENTDICTIONARYID == oldDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                                //异动前部门名称
                                employeeEntity.OLDDEPARTMENTNAME = oldDepartment.OLDDEPARTMENTNAME;
                            }
                            //异动前公司id
                            employeeEntity.OLDCOMPANYID = employeePostChange.FROMCOMPANYID;
                            //查找公司表，直接得到公司名称
                            //var oldCompany = dal.GetObjects<T_HR_COMPANY>().FirstOrDefault(s => s.COMPANYID == employeeEntity.OLDCOMPANYID);
                            var oldCompany = (from c in dal.GetObjects<T_HR_COMPANY>()
                                              where c.COMPANYID == employeePostChange.FROMCOMPANYID
                                              select new
                                              {
                                                  OLDCOMPANYNAMECH = c.CNAME,
                                                  OLDCOMPANYNAMEEN = c.ENAME
                                              }).FirstOrDefault();
                            if (oldCompany != null)
                            {
                                //根据公司字典ID查询公司字典表，查询公司名称
                                //var oldCompanyHistory = dal.GetObjects<T_HR_COMPANYHISTORY>().FirstOrDefault(s => s.COMPANYID == oldCompany.COMPANYID);
                                //异动前公司名称(中文)
                                employeeEntity.OLDCOMPANYNAME = oldCompany.OLDCOMPANYNAMECH;
                            }

                            //异动前薪资额度
                            //employeeEntity.OLDSALARYSUM = null;

                            //异动后岗位id
                            employeeEntity.NEXTPOSTID = employeePostChange.TOPOSTID;
                            //根据异动前岗位ID查找岗位表，查询岗位字典ID
                            //var newPostInfo = dal.GetObjects<T_HR_POST>().FirstOrDefault(s => s.POSTID == employeeEntity.NEXTPOSTID);
                            //连表查询，异动后岗位名称
                            var newPostInfo = (from c in dal.GetObjects<T_HR_POST>()
                                               join m in dal.GetObjects<T_HR_POSTDICTIONARY>()
                                               on c.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals m.POSTDICTIONARYID
                                               where c.POSTID == employeePostChange.TOPOSTID
                                               select new
                                               {
                                                   NEXTPOSTNAME = m.POSTNAME
                                               }).FirstOrDefault();

                            if (newPostInfo != null)
                            {
                                //根据岗位字典ID查询字典表，查询岗位名称
                                //var newPostDictionary = dal.GetObjects<T_HR_POSTDICTIONARY>().FirstOrDefault(s => s.POSTDICTIONARYID == newPostInfo.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                                //异动后岗位名称
                                employeeEntity.NEXTPOSTNAME = newPostInfo.NEXTPOSTNAME;
                            }

                            //异动后岗位级别
                            employeeEntity.NEXTPOSTLEVEL = employeePostChange.TOPOSTLEVEL.ToString();
                            //异动后薪资级别
                            employeeEntity.NEXTCOMPANYNAME = employeePostChange.TOSALARYLEVEL.ToString();

                            //异动后部门id
                            employeeEntity.NEXTDEPARTMENTID = employeePostChange.TODEPARTMENTID;
                            //根据异动前部门ID查找部门表，查询部门字典ID
                            //var newDepartment = dal.GetObjects<T_HR_DEPARTMENT>().FirstOrDefault(s => s.DEPARTMENTID == employeeEntity.NEXTDEPARTMENTID);
                            var newDepartment = (from c in dal.GetObjects<T_HR_DEPARTMENT>()
                                                 join m in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>()
                                                 on c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals m.DEPARTMENTDICTIONARYID
                                                 where c.DEPARTMENTID == employeePostChange.TODEPARTMENTID
                                                 select new
                                                 {
                                                     NEXTDEPARTMENTNAME = m.DEPARTMENTNAME
                                                 }).FirstOrDefault();

                            if (newDepartment != null)
                            {
                                //根据部门字典ID查询部门字典表，查询部门名称
                                //var newDepartmentDictionary = dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>().FirstOrDefault(s => s.DEPARTMENTDICTIONARYID == newDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                                //异动后部门名称
                                employeeEntity.NEXTDEPARTMENTNAME = newDepartment.NEXTDEPARTMENTNAME;
                            }


                            //异动后公司id
                            employeeEntity.NEXTCOMPANYID = employeePostChange.TOCOMPANYID;
                            //根据异动前公司ID查找公司表，查询公司字典ID
                            //var newCompany = dal.GetObjects<T_HR_COMPANY>().FirstOrDefault(s => s.COMPANYID == employeeEntity.NEXTCOMPANYID);
                            //根据公司字典ID查询公司字典表，查询公司名称
                            var newCompany = (from c in dal.GetObjects<T_HR_COMPANY>()
                                              where c.COMPANYID == employeePostChange.TOCOMPANYID
                                              select new
                                              {
                                                  OLDCOMPANYNAMECH = c.CNAME,
                                                  OLDCOMPANYNAMEEN = c.ENAME
                                              }).FirstOrDefault();

                            if (newCompany != null)
                            {
                                //var newCompanyHistory = dal.GetObjects<T_HR_COMPANYHISTORY>().FirstOrDefault(s => s.COMPANYID == newCompany.COMPANYID);
                                //异动后公司名称
                                employeeEntity.NEXTCOMPANYNAME = newCompany.OLDCOMPANYNAMECH;
                            }

                            //备注
                            employeeEntity.REMART = employeePostChange.REMARK;
                            //创建时间
                            employeeEntity.CREATEDATE = DateTime.Now;
                            //所属员工ID
                            employeeEntity.OWNERID = employeePostChange.OWNERID;
                            //所属岗位ID
                            employeeEntity.OWNERPOSTID = employeePostChange.OWNERPOSTID;
                            //所属部门ID
                            employeeEntity.OWNERDEPARTMENTID = employeePostChange.OWNERDEPARTMENTID;
                            //所属公司ID
                            employeeEntity.OWNERCOMPANYID = employeePostChange.OWNERCOMPANYID;

                            dal.AddToContext(employeeEntity);
                        }
                        catch (Exception ex)
                        {
                            SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + "员工异动报表服务同步:" + ex.ToString());
                        }

                        #endregion

                    }
                    i = dal.SaveContextChanges();
                    #region 调用即时通讯的接口


                    if (employeePostChange.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {

                        this.AddIMInstantMessageForPostChange(employeePostChange);
                        //EmployeeEntryBLL bll = new EmployeeEntryBLL();
                        //if (EmployeePost != null)
                        //{
                        //    StrMessage = "员工岗位异动开始调用即时通讯接口";
                        //    SMT.Foundation.Log.Tracer.Debug(StrMessage);
                        //    var Employes = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                        //                   where ent.EMPLOYEEID == tmp.EMPLOYEEID
                        //                   select ent;
                        //    if (Employes.Count() > 0)
                        //    {
                        //        bll.AddImInstantMessage(Employes.FirstOrDefault(), EmployeePost);
                        //    }
                        //    else
                        //    {
                        //        StrMessage = "员工岗位异动开始调用即时通讯时员工为空";
                        //        SMT.Foundation.Log.Tracer.Debug(StrMessage);
                        //    }

                        //}

                    }
                    #endregion

                }
                dal.CommitTransaction();
                return i;
            }
            catch (Exception e)
            {
                dal.RollbackTransaction();//回滚
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.ToString());
                return 0;
            }
        }


        #region 员工异动调即时通讯接口
        private void AddIMInstantMessageForPostChange(T_HR_EMPLOYEEPOSTCHANGE EntObj)
        {
            string StrMessage = "";
            try
            {
                string StrFromDeptID = "";
                string StrTODeptID = "";
                string StrFromPostID = "";
                string StrToPostID = "";
                string StrUserID = "";
                string PostName = "";
                string CompanyName = "";
                string StrToCompanyID = "";

                DataSyncServiceClient IMCient = new DataSyncServiceClient();
                var ents = from ent in dal.GetObjects<T_HR_EMPLOYEEPOSTCHANGE>().Include("T_HR_EMPLOYEE")
                           where ent.POSTCHANGEID == EntObj.POSTCHANGEID
                           select ent;
                if (ents.Count() > 0)
                {
                    StrFromDeptID = ents.FirstOrDefault().FROMDEPARTMENTID;
                    StrTODeptID = ents.FirstOrDefault().TODEPARTMENTID;
                    StrFromPostID = ents.FirstOrDefault().FROMPOSTID;
                    StrToPostID = ents.FirstOrDefault().TOPOSTID;
                    StrUserID = ents.FirstOrDefault().T_HR_EMPLOYEE.EMPLOYEEID;
                    StrToCompanyID = ents.FirstOrDefault().TOCOMPANYID;
                    var epost = from en in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                where en.EMPLOYEEPOSTID == EntObj.EMPLOYEEPOSTID
                                select en;
                    if (epost.Count() > 0)
                    {
                        string StrEmployeePostID = epost.FirstOrDefault().EMPLOYEEPOSTID;
                        var entsEmployeePosts = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                where c.EMPLOYEEPOSTID == StrEmployeePostID
                                                select new V_EMPLOYEEPOSTBRIEF
                                                {
                                                    EMPLOYEEPOSTID = c.EMPLOYEEPOSTID,
                                                    POSTID = c.T_HR_POST.POSTID,
                                                    PostName = c.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                                                    DepartmentID = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                                    DepartmentName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                    CompanyID = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                                    CompanyName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                                    POSTLEVEL = c.POSTLEVEL,
                                                    ISAGENCY = c.ISAGENCY
                                                };
                        if (entsEmployeePosts.Count() > 0)
                        {
                            CompanyName = entsEmployeePosts.FirstOrDefault().CompanyName;
                            PostName = entsEmployeePosts.FirstOrDefault().PostName;
                        }
                        else
                        {
                            SMT.Foundation.Log.Tracer.Debug("员工异动时EmployeeMove获取组织架构信息为空");
                        }
                    }
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("员工异动时EmployeeMove获取异动信息为空");
                }

                if (EntObj.ISAGENCY == "0")
                {
                    //主职异动
                    SMT.Foundation.Log.Tracer.Debug("主职异动时开始调用即时通讯接口");
                    StrMessage = IMCient.EmployeeMove(StrUserID, StrFromDeptID, StrTODeptID, StrFromPostID, StrToPostID, PostName, CompanyName);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("兼职异动时开始调用即时通讯接口");
                    StrMessage = IMCient.EmployeePartTimeEntry(StrUserID, StrTODeptID, StrToPostID, PostName, CompanyName);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工异动时调即时通讯接口发生错误：" + ex.ToString());
            }
            SMT.Foundation.Log.Tracer.Debug("员工异动时调即时通讯接口结果：" + StrMessage);


        }
        #endregion


        #region 获取出差报销的信息
        /// <summary>
        /// Add     :luojie
        /// Date    :2012/11/06
        /// ForWaht :调用出差申请的接口，查询是否有进行中出差信息
        /// </summary>
        /// <param name="employeeid">用户ID</param>
        /// <returns>Dictionary<"单据","ID"></returns>
        public Dictionary<string, string> CheckBusinesstrip(string employeeid)
        {
            Dictionary<string, string> ResultDiction = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(employeeid))
                {
                    SMT.SaaS.BLLCommonServices.OAPersonalWS.SmtOAPersonOfficeClient OaOffice = new SaaS.BLLCommonServices.OAPersonalWS.SmtOAPersonOfficeClient();
                    ResultDiction = OaOffice.GetUnderwayTravelmanagement(employeeid);
                    return ResultDiction;
                }
                else
                {
                    Tracer.Debug("EmployeePostChangeBLL-CheckBusinesstrip  未获取正确的用户ID");
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("EmployeePostChangeBLL-CheckBusinesstrip  " + ex.ToString());
                return null;
            }
            return ResultDiction;

        }
        #endregion


    }
}
