using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using SMT.HRM.CustomModel;
using SMT.HRM.IMServices.IMServiceWS;
namespace SMT.HRM.BLL
{
    public class EmployeePostBLL : BaseBll<T_HR_EMPLOYEEPOST>
    {
        /// <summary>
        /// 获取指定岗位下员工数
        /// </summary>
        /// <param name="positionID">岗位ID</param>
        /// <returns>员工数</returns>
        public int GetEmployeesCount(string positionID)
        {
            string editstate = Convert.ToInt32(EditStates.Actived).ToString();
            var ents = from ent in dal.GetObjects()
                       where ent.T_HR_POST.POSTID == positionID && (ent.EDITSTATE == editstate)
                       select ent;
            return ents == null ? 0 : ents.Count();
        }

        /// <summary>
        /// 根据员工信息ID找到对应的岗位信息
        /// </summary>
        /// <param name="employeeID">员工信息ID</param>
        /// <returns>返回员工岗位实体</returns>
        public T_HR_EMPLOYEEPOST GetEmployeePostByEmployeeID(string employeeID)
        {
            var ents = dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_POST").FirstOrDefault(s => s.T_HR_EMPLOYEE.EMPLOYEEID == employeeID);
            if (ents != null)
            {
                //加载岗位字典
                ents.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();
                //加载部门
                ents.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                //加载部门字典
                ents.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
                //加载公司
                ents.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
            }
            return ents;
        }
        /// <summary>
        ///根据员工ID获取员工的有效非代理岗位
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEEPOST GetEmployeePostActivedByEmployeeID(string employeeID)
        {
            var ents = dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_POST").FirstOrDefault(s => s.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && s.ISAGENCY == "0" && s.EDITSTATE == "1");
            if (ents != null)
            {
                //加载岗位字典
                ents.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();
                //加载部门
                ents.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                //加载部门字典
                ents.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();
                //加载公司
                ents.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
            }
            return ents;
        }
        public List<T_HR_EMPLOYEEPOST> GetEmployeePostByPostID(string postID)
        {
            var ents = from emp in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_POST.T_HR_POSTDICTIONARY")
                       where emp.T_HR_POST.POSTID == postID && emp.EDITSTATE == "1"
                       select emp;

            return ents.Count() > 0 ? ents.ToList() : null;
        }


         /// <summary>
        /// 根据岗位id获取岗位下面员工(有权限控制),目前只组织架构用到，所以只传岗位id和当前员工id即可
        /// 权限控制实体为T_HR_EMPLOYEE，与人事档案权限一致（不行，组织架构控件为公共控件，以前很多人能看到，改了后很多人看不到）
        /// </summary>
        /// <param name="postID">岗位id</param>
        /// <param name="userID">当前员工id</param>
        /// <returns></returns>
        //public List<T_HR_EMPLOYEEPOST> GetEmployeePostByPostIDView(string postID, string userID)
        //{
           
        //        EmployeeBLL employeeBll = new EmployeeBLL();
        //        List<T_HR_EMPLOYEE> employeeList = new List<T_HR_EMPLOYEE>();
        //        List<string> listEmployeeID = new List<string>();
        //        //调用这个方法获得进行了员工档案权限过滤后的员工id
        //        employeeList = employeeBll.GetEmployeePostByPostIDView(postID, userID);
        //        if (employeeList != null)
        //        {
        //            foreach (var temp in employeeList)
        //            {
        //                listEmployeeID.Add(temp.EMPLOYEEID);
        //            }
        //        }
        //            var ents = from emp in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_POST.T_HR_POSTDICTIONARY")
        //                       where emp.T_HR_POST.POSTID == postID && emp.EDITSTATE == "1" && listEmployeeID.Contains(emp.T_HR_EMPLOYEE.EMPLOYEEID)
        //                       select emp;

        //            return ents.Count() > 0 ? ents.ToList() : null;
                
        //}
        public List<T_HR_EMPLOYEEPOST> GetEmployeePostByPostIDs(IList<string> postIDs)
        {
            List<T_HR_EMPLOYEEPOST> ents = new List<T_HR_EMPLOYEEPOST>();
            List<T_HR_EMPLOYEEPOST> ent = new List<T_HR_EMPLOYEEPOST>();
            //foreach (var id in postIDs)
            //{
            //    ent = (from e in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
            //           where e.T_HR_POST.POSTID == id
            //           select e.T_HR_EMPLOYEE).ToList();
            //    ents.AddRange(ent);
            //}

            //return ents;
            foreach (var id in postIDs)
            {
                ent = (from emp in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                       where emp.T_HR_POST.POSTID == id
                       select emp).ToList();
                ents.AddRange(ent);
            }

            return ents;
        }
        /// <summary>
        /// 新增岗位信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string EmployeePostAdd(T_HR_EMPLOYEEPOST entity)
        {
            string strMsg = string.Empty;
            try
            {
                T_HR_EMPLOYEEPOST tmp = new T_HR_EMPLOYEEPOST();
                Utility.CloneEntity(entity, tmp);
                if (entity.T_HR_EMPLOYEE != null)
                {
                    tmp.T_HR_EMPLOYEEReference.EntityKey =
                                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                }
                if (entity.T_HR_POST != null)
                {
                    tmp.T_HR_POSTReference.EntityKey =
                                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", entity.T_HR_POST.POSTID);
                }

                if (dal.Add(tmp) > 0)
                {
                    //清MVC缓存
                    MvcCacheClear(tmp, "Add");
                    strMsg = "SUCCESSED";
                }
            }
            catch (Exception e)
            {
                // strMsg = e.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeePostAdd:" + e.Message);
            }
            return strMsg;
        }
        /// <summary>
        /// 修改岗位信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string EmployeePostUpdate(T_HR_EMPLOYEEPOST entity)
        {
            string strMsg = string.Empty;
            try
            {
                //var ents = from c in dal.GetObjects().Include("T_HR_POST").Include("T_HR_EMPLOYEE")
                //           where c.EMPLOYEEPOSTID == entity.EMPLOYEEPOSTID
                //           select c;
                //if (ents.Count() > 0)
                //{
                //T_HR_EMPLOYEEPOST ent = ents.FirstOrDefault();
                //Utility.CloneEntity<T_HR_EMPLOYEEPOST>(entity, ent);
                if (entity.T_HR_EMPLOYEE != null)
                {
                    entity.T_HR_EMPLOYEE = entity.T_HR_EMPLOYEE;
                    entity.T_HR_EMPLOYEEReference.EntityKey =
                                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    entity.T_HR_EMPLOYEE.EntityKey =
                                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                }
                if (entity.T_HR_POST != null)
                {
                    entity.T_HR_POST = entity.T_HR_POST;
                    entity.T_HR_POSTReference.EntityKey =
                                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", entity.T_HR_POST.POSTID);
                    entity.T_HR_POST.EntityKey =
                                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", entity.T_HR_POST.POSTID);
                }
                entity.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEPOST", "EMPLOYEEPOSTID", entity.EMPLOYEEPOSTID);
                //  Utility.RefreshEntity(ent);
                int i=0;
                i=dal.Update(entity);
                if (i > 0)
                {
                    //DelDeparmentMember(entity.T_HR_EMPLOYEE.EMPLOYEEID, entity.T_HR_POST.POSTID); 
                }
                //清MVC缓存
                MvcCacheClear(entity, "Modify");
                strMsg = "SUCCESSED";
            }
            catch (Exception e)
            {
                //strMsg = e.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeePostUpdate:" + e.Message);
            }
            return strMsg;
        }


        /// <summary>
        /// 获取员工未提交审核的岗位
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="postID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEEPOST GetEmployeePostByEmployeeIDAndPostID(string employeeID, string postID)
        {

            var ents = from c in dal.GetObjects()
                       where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && c.T_HR_POST.POSTID == postID && c.CHECKSTATE == "0" && c.EDITSTATE == "0"
                       select c;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;

        }
        /// <summary>
        /// 根据ID获取员工岗位
        /// </summary>
        /// <param name="employeePostID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEEPOST GetEmployeePostByID(string employeePostID)
        {

            var ents = from c in dal.GetObjects()
                       where c.EMPLOYEEPOSTID == employeePostID
                       select c;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;

        }
        /// <summary>
        /// 根据员工信息ID找到所有对应的岗位信息
        /// </summary>
        /// <param name="employeeID">员工信息ID</param>
        /// <returns>员工所有岗位实体</returns>
        public List<T_HR_EMPLOYEEPOST> GetAllPostByEmployeeID(string employeeID)
        {
            var ents = from p in dal.GetObjects().Include("T_HR_POST")
                       where p.T_HR_EMPLOYEE.EMPLOYEEID == employeeID
                       select p;
            List<T_HR_EMPLOYEEPOST> posts = ents.ToList();
            foreach (var p in posts)
            {
                if (!p.T_HR_POST.T_HR_POSTDICTIONARYReference.IsLoaded)
                    p.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();

                if (!p.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded)
                {
                    p.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                    if (p.T_HR_POST.T_HR_DEPARTMENT != null)
                    {
                        if (!p.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.IsLoaded)
                            p.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();

                        if (!p.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded)
                            p.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                    }
                }
            }
            return posts;
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEEPOST> GetPostsActivedByEmployeeID(string employeeID)
        {
            var ents = from p in dal.GetObjects().Include("T_HR_POST")
                       where p.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && p.EDITSTATE == "1"
                       select p;
            List<T_HR_EMPLOYEEPOST> posts = ents.ToList();
            foreach (var p in posts)
            {
                if (!p.T_HR_POST.T_HR_POSTDICTIONARYReference.IsLoaded)
                    p.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();

                if (!p.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded)
                {
                    p.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                    if (p.T_HR_POST.T_HR_DEPARTMENT != null)
                    {
                        if (!p.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.IsLoaded)
                            p.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();

                        if (!p.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded)
                            p.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                    }
                }
            }
            return posts;
        }
        /// <summary>
        /// 根据ID获取员工岗位
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEEPOST GetEmployeePostByEmployeePostID(string employeePostID)
        {
            var ents = from p in dal.GetObjects().Include("T_HR_POST")
                       where p.EMPLOYEEPOSTID == employeePostID
                       select p;
            T_HR_EMPLOYEEPOST post = ents.FirstOrDefault();

            if (post != null)
            {
                if (!post.T_HR_POST.T_HR_POSTDICTIONARYReference.IsLoaded)
                    post.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();

                if (!post.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded)
                {
                    post.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                    if (post.T_HR_POST.T_HR_DEPARTMENT != null)
                    {
                        if (!post.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.IsLoaded)
                            post.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARYReference.Load();

                        if (!post.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded)
                            post.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                    }
                }
            }
            return post;
        }

        /// <summary>
        ///     Add by 罗捷
        ///     通过员工ID和岗位ID获取IsAgency状态
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public int GetIsAgencyByEmployeeIdAndPostId(string employeeId, string postId)
        {
            ///获取T_HR_EMPLOYEEPOST表的内容，根据employeeId和PostId查询
            ///CheckState==“2”，EditState==“1”
            int result = -1;
            try
            {
                //根据员工和岗位ID查询生效且是代理的员工岗位信息
                var ents = from p in dal.GetObjects()
                           where p.T_HR_EMPLOYEE.EMPLOYEEID == employeeId && p.T_HR_POST.POSTID == postId
                           && p.CHECKSTATE == "2" && p.EDITSTATE == "1"
                           && p.ISAGENCY == "1"
                           select p;

                if (ents.Count() > 0)
                {
                    result = 1;
                }
                else
                {
                    result = 0;
                }
                
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("调用EmployeePostBll-GetIsAgencyByEmployeeIdAndPostId" + ex.ToString());
                result = -1;
            }
            return result;

        }

        public string updateAllpostByemployeeID(string employeeID)
        {
            string strMeg = string.Empty;

            try
            {
                List<T_HR_EMPLOYEEPOST> eposts = GetAllPostByEmployeeID(employeeID);
                foreach (var item in eposts)
                {
                    item.EDITSTATE = "0";
                    EmployeePostUpdate(item);
                }

            }
            catch (Exception ex)
            {
                //strMeg = ex.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " updateAllpostByemployeeID:" + ex.Message);
            }
            return strMeg;

        }
        /// <summary>
        /// 根据员工的薪资等级
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public string EmployeePostSalaryLevelUpdate(T_HR_EMPLOYEEPOST employeepost)
        {
            string strMsg = string.Empty;
            try
            {
                var ents = from c in dal.GetObjects()
                           where c.T_HR_EMPLOYEE.EMPLOYEEID == employeepost.T_HR_EMPLOYEE.EMPLOYEEID && c.EDITSTATE == "1"
                           select c;
                if (ents.Count() > 0)
                {
                    T_HR_EMPLOYEEPOST ent = ents.FirstOrDefault();
                    ent.SALARYLEVEL = employeepost.SALARYLEVEL;
                    dal.Update(ent);
                    strMsg = "SUCCESSED";

                }


            }
            catch (Exception e)
            {
                //strMsg = e.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeePostSalaryLevelUpdate:" + e.Message);
            }
            return strMsg;
        }

        public string EmployeePostSalaryLevelUpdate(string employeeid, decimal salaryevel)
        {
            string strMsg = string.Empty;
            try
            {
                var ents = from c in dal.GetObjects()
                           //where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeid && c.EDITSTATE == "1" && c.ISAGENCY == "0"
                           //喻建华 修改于2010-12-22 start
                           where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeid && c.EDITSTATE == "1" && c.ISAGENCY == "0"
                           //end
                           select c;
                if (ents.Count() > 0)
                {
                    T_HR_EMPLOYEEPOST ent = ents.FirstOrDefault();
                    ent.SALARYLEVEL = salaryevel;
                    dal.Update(ent);
                    strMsg = "SUCCESSED";

                }

            }
            catch (Exception e)
            {
                //strMsg = e.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeePostSalaryLevelUpdate:" + e.Message);
            }
            return strMsg;
        }

        /// <summary>
        /// 根据员工ID，获取其基础信息及其所有主职，兼职的岗位，部门，公司名称及ID
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        public V_EMPLOYEEDETAIL GetEmployeePostBriefByEmployeeID(string employeeid)
        {
            var ent = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                      where c.EMPLOYEEID == employeeid
                      select new V_EMPLOYEEDETAIL
                      {
                          EMPLOYEEID = c.EMPLOYEEID,
                          EMPLOYEENAME = c.EMPLOYEECNAME,
                          EMPLOYEEENAME = c.EMPLOYEEENAME,
                          EMPLOYEECODE = c.EMPLOYEECODE,
                          EMPLOYEESTATE = c.EMPLOYEESTATE,
                          OFFICEPHONE = c.OFFICEPHONE,
                          MOBILE = c.MOBILE,
                          SEX = c.SEX
                      };

            V_EMPLOYEEDETAIL employeeDetail = new V_EMPLOYEEDETAIL();
            if (ent.Count() > 0)
            {
                employeeDetail = ent.FirstOrDefault();
            }
            else
            {
                return null;
            }

            //按岗位进行升序排序，保证主岗位为第一条
            var ents = (from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                       where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeid && c.EDITSTATE == "1"
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
                       }).OrderBy(c => c.ISAGENCY);
            employeeDetail.EMPLOYEEPOSTS = ents.Count() > 0 ? ents.ToList() : null;
            return ents.Count() > 0 ? employeeDetail : null;
        }

        /// <summary>
        /// 根据员工ID，获取其基础信息及其所有主职，兼职的岗位，部门，公司名称及ID，包括离职岗位-by luojie
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        public V_EMPLOYEEDETAIL GetAllEmployeePostBriefByEmployeeID(string employeeid)
        {
            try
            {

                var ent = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                          where c.EMPLOYEEID == employeeid
                          select new V_EMPLOYEEDETAIL
                          {
                              EMPLOYEEID = c.EMPLOYEEID,
                              EMPLOYEENAME = c.EMPLOYEECNAME,
                              EMPLOYEEENAME = c.EMPLOYEEENAME,
                              EMPLOYEECODE = c.EMPLOYEECODE,
                              EMPLOYEESTATE = c.EMPLOYEESTATE,
                              OFFICEPHONE = c.OFFICEPHONE,
                              MOBILE = c.MOBILE,
                              SEX = c.SEX
                          };

                V_EMPLOYEEDETAIL employeeDetail = new V_EMPLOYEEDETAIL();
                if (ent.Count() > 0)
                {
                    employeeDetail = ent.FirstOrDefault();
                }
                else
                {
                    return null;
                }

                //按岗位进行升序排序，保证主岗位为第一条
                var ents = (from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                            where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeid
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
                            }).OrderBy(c => c.ISAGENCY);
                employeeDetail.EMPLOYEEPOSTS = ents.Count() > 0 ? ents.ToList() : null;
                return ents.Count() > 0 ? employeeDetail : null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "EmployeePostBll-GetAllEmployeePostBriefByEmployeeID-Error:" + ex.ToString());
            }
            return null;

        }

        public List<V_EMPLOYEEDETAIL> GetEmployeePostBriefByEmployeeID(List<string> employeeids)
        {
            List<V_EMPLOYEEDETAIL> details = new List<V_EMPLOYEEDETAIL>();
            foreach (var employeeid in employeeids)
            {
                var ent = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                          where c.EMPLOYEEID == employeeid
                          select new V_EMPLOYEEDETAIL
                          {
                              EMPLOYEEID = c.EMPLOYEEID,
                              EMPLOYEENAME = c.EMPLOYEECNAME,
                          };

                V_EMPLOYEEDETAIL employeeDetail = new V_EMPLOYEEDETAIL();
                if (ent.Count() > 0)
                {
                    employeeDetail = ent.FirstOrDefault();
                }
                else
                {
                    continue;
                }

                var ents = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                           where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeid && c.EDITSTATE == "1"
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
                employeeDetail.EMPLOYEEPOSTS = ents.Count() > 0 ? ents.ToList() : null;
                if (ents.Count() > 0)
                {
                    details.Add(employeeDetail);
                }
            }
            return details;
        }

        /// <summary>
        /// 通过岗位ID获取对应的员工的信息
        /// </summary>
        /// <param name="StrPostID"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEE> GetEmployeePostBriefByPostID(string StrPostID)
        {
            List<T_HR_EMPLOYEE> details = new List<T_HR_EMPLOYEE>();
            try
            {
                List<string> employeeids = new List<string>();//员工ID集合
                //岗位ID是审核通过且生效的
                var Posts = from ent in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE")//.Include("T_HR_POST")
                            where ent.T_HR_POST.POSTID == StrPostID && ent.CHECKSTATE == "2"
                            && ent.EDITSTATE =="1"
                            select ent;
                if (Posts.Count() > 0)
                {
                    Posts.ToList().ForEach(item =>
                    {
                        employeeids.Add(item.T_HR_EMPLOYEE.EMPLOYEEID);
                    });
                }
                var entEmployees = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                   where employeeids.Contains(ent.EMPLOYEEID)
                                   && ent.EDITSTATE =="1"
                                   select ent;
                details = entEmployees.Count() > 0 ? entEmployees.ToList() : null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeePostBLL-GetEmployeePostBriefByPostID出现错误："+ ex.ToString());
            }
            return details;
        }



        #region 流程用户信息使用
        /// <summary>
        /// 根据员工ID集合获取用户信息
        /// </summary>
        /// <param name="employeeids"></param>
        /// <returns></returns>
        public List<V_FlowUserInfo> GetFlowUserInfoPostBriefByEmployeeID(List<string> employeeids)
        {
            List<V_FlowUserInfo> details = new List<V_FlowUserInfo>();
            //提示信息
            string StrMessage = "";
            try
            {
                if (employeeids.Count() > 0)
                {
                    StringBuilder idnumber = new StringBuilder();
                    for (int i = 0; i < employeeids.Count; i++)
                    {
                        if (i < employeeids.Count - 1)
                        {
                            idnumber.Append(employeeids[i] + ",");
                        }
                        else
                        {
                            idnumber.Append(employeeids[i]);
                        }
                       
                        
                    }
                    string IDs = idnumber.ToString();
                    //SMT.Foundation.Log.Tracer.Debug("查找的IDs："+IDs);
                    var ents = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                               where c.EDITSTATE == "1" && c.CHECKSTATE == "2"
                               select new V_FlowUserInfo
                                       {
                                           UserID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                           EmployeeName = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                           CompayID = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                           CompayName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                           DepartmentID = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                           DepartmentName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                           PostID = c.T_HR_POST.POSTID,
                                           PostName = c.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                                           IsHead = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTBOSSHEAD == c.T_HR_EMPLOYEE.EMPLOYEEID ? true : false,
                                           IsSuperior = false
                                       };

                    if (employeeids.Count() == 1)
                    {
                        ents = from ent in ents
                               where ent.UserID == IDs
                               select ent;

                    }
                    else
                    {
                        ents = from ent in ents
                               where ent.UserID.Contains(IDs)
                               select ent;
                    }


                    if (ents.Count() > 0)
                    {
                        details = ents.ToList();
                    }
                }
                else
                {
                    StrMessage = "传入的员工ID集合数为0GetEmployeePostBriefByEmployeeID:";
                    SMT.Foundation.Log.Tracer.Debug(StrMessage);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("流程获取员工信息出错GetEmployeePostBriefByEmployeeID:" + ex.ToString());
            }
            return details;
        }


        /// <summary>
        /// 通过部门ID:(查找部门负责人,包括人员所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="departmentID">部门ID</param>
        /// <returns></returns>
        public List<V_FlowUserInfo> GetDepartmentHeadByDepartmentID(string departmentID)
        {
            List<V_FlowUserInfo> details = new List<V_FlowUserInfo>();
            //信息提示
            string StrMessage = "";
            try
            {
                if (string.IsNullOrEmpty(departmentID))
                {
                    return details;
                }
                var EntDepartment = from ent in dal.GetObjects<T_HR_DEPARTMENT>()
                                    where ent.DEPARTMENTID == departmentID
                                    select ent;
                if (EntDepartment.Count() > 0)
                {
                    //员工ID
                    string StrEmployeeId = "";
                    //部门负责人岗位ID
                    string StrEmployeePostId = EntDepartment.FirstOrDefault().DEPARTMENTBOSSHEAD;
                    
                    //根据岗位ID获取对应的员工
                    var entPosts = from ent in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                       .Include("T_HR_POST").Include("T_HR_EMPLOYEE")
                                   where ent.T_HR_POST.POSTID == StrEmployeePostId
                                   && ent.CHECKSTATE =="2" && ent.EDITSTATE =="1"
                                   select ent;
                    if (entPosts.Count() == 0)
                    {
                        StrMessage = "GetDepartmentHeadByDepartmentID获取员工岗位出错，岗位ID:";
                        SMT.Foundation.Log.Tracer.Debug( StrMessage + StrEmployeePostId);
                        return details;
                    }
                    else
                    {
                        StrEmployeeId = entPosts.FirstOrDefault().T_HR_EMPLOYEE.EMPLOYEEID;
                        
                    }
                    //员工ID
                    //string StrEmployeeId = EntDepartment.FirstOrDefault().DEPARTMENTBOSSHEAD;
                    var EntEmployee = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                      where ent.EMPLOYEEID == StrEmployeeId && ent.EDITSTATE != "2"
                                      select ent;


                    if (EntEmployee.Count() > 0)
                    {
                        var ents = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                   where c.T_HR_EMPLOYEE.EMPLOYEEID == StrEmployeeId && c.EDITSTATE == "1"
                                   select new V_EMPLOYEEPOSTFLOWUSER
                                   {
                                       EMPLOYEEPOSTID = c.EMPLOYEEPOSTID,
                                       EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                       EMPLOYEENAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                       POSTID = c.T_HR_POST.POSTID,
                                       PostName = c.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                                       DepartmentID = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                       DepartmentName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                       CompanyID = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                       CompanyName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                       POSTLEVEL = c.POSTLEVEL,
                                       ISAGENCY = c.ISAGENCY,
                                       ISHEAD = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTBOSSHEAD == StrEmployeeId ? true : false,
                                       ISSUPERIOR = false

                                   };
                        if (ents.Count() > 0)
                        {
                            foreach (var ent in ents)
                            {
                                V_FlowUserInfo FlowUser = new V_FlowUserInfo();
                                FlowUser.UserID = ent.EMPLOYEEID;
                                FlowUser.EmployeeName = ent.EMPLOYEENAME;
                                FlowUser.CompayID = ent.CompanyID;
                                FlowUser.CompayName = ent.CompanyName;
                                FlowUser.DepartmentID = ent.DepartmentID;
                                FlowUser.DepartmentName = ent.DepartmentName;
                                FlowUser.PostID = ent.POSTID;
                                FlowUser.PostName = ent.PostName;
                                FlowUser.IsHead = ent.ISHEAD;
                                FlowUser.IsSuperior = ent.ISSUPERIOR;
                                details.Add(FlowUser);
                                //if()
                            }

                        }
                    }
                    else
                    {
                        StrMessage = "GetDepartmentHeadByDepartmentID没有获取到员工信息,员工ID：";
                        SMT.Foundation.Log.Tracer.Debug(StrMessage + StrEmployeeId);
                    }
                }
                

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("GetDepartmentHeadByDepartmentID" + ex.ToString());
            }
            return details;
        }
        /// <summary>
        ///通过岗位ID: (查找[直接上级]，[隔级上级]，包括所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        public List<V_FlowUserInfo> GetSuperiorByPostID(string postID)
        {
            List<V_FlowUserInfo> details = new List<V_FlowUserInfo>();
            //提示信息
            string StrMessage = "";
            try
            {
                if (string.IsNullOrEmpty(postID))
                {
                    return details;
                }
                var EntPost = from ent in dal.GetObjects<T_HR_POST>()
                                    where ent.POSTID == postID
                                    select ent;
                if (EntPost.Count() > 0)
                {
                    #region 直接上级

                    var ents = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                               where c.T_HR_POST.POSTID == postID && c.EDITSTATE == "1"
                               select new V_EMPLOYEEPOSTFLOWUSER
                               {
                                   EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                   EMPLOYEENAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                   EMPLOYEEPOSTID = c.EMPLOYEEPOSTID,
                                   POSTID = c.T_HR_POST.POSTID,
                                   PostName = c.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                                   DepartmentID = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                   DepartmentName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                   CompanyID = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                   CompanyName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                   POSTLEVEL = c.POSTLEVEL,
                                   ISAGENCY = c.ISAGENCY,
                                   ISHEAD = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTBOSSHEAD == c.T_HR_EMPLOYEE.EMPLOYEEID ? true : false,
                                   ISSUPERIOR = true

                               };
                    if (ents.Count() > 0)
                    {
                        foreach (var ent in ents)
                        {
                            V_FlowUserInfo FlowUser = new V_FlowUserInfo();
                            FlowUser.UserID = ent.EMPLOYEEID;
                            FlowUser.EmployeeName = ent.EMPLOYEENAME;
                            FlowUser.CompayID = ent.CompanyID;
                            FlowUser.CompayName = ent.CompanyName;
                            FlowUser.DepartmentID = ent.DepartmentID;
                            FlowUser.DepartmentName = ent.DepartmentName;
                            FlowUser.PostID = ent.POSTID;
                            FlowUser.PostName = ent.PostName;
                            FlowUser.IsHead = ent.ISHEAD;
                            FlowUser.IsSuperior = ent.ISSUPERIOR;
                            var EntDetails = from det in details
                                             where det.CompayID == ent.CompanyID
                                     && det.PostID == ent.POSTID && det.DepartmentID == ent.DepartmentID
                                     && det.UserID == ent.EMPLOYEEID
                                             select det;
                            if (EntDetails.Count() == 0)
                            {
                                details.Add(FlowUser);
                            }
                            //if()
                        }

                    }
                    else
                    {
                        StrMessage = "GetDepartmentHeadByDepartmentID,根据岗位ID获取员工为空，岗位ID为：";
                        SMT.Foundation.Log.Tracer.Debug(StrMessage + postID);
                    }

                    #endregion

                    #region 隔级上级
                    //获取岗位的直接上级
                    var FatherPosts = from ent in dal.GetObjects<T_HR_POST>()
                                      where ent.FATHERPOSTID == EntPost.FirstOrDefault().FATHERPOSTID
                                      select ent;
                    if (FatherPosts.Count() > 0)
                    {
                        string StrFatherPostid = FatherPosts.FirstOrDefault().POSTID;
                        var FatherPostents = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                             where c.T_HR_POST.POSTID == StrFatherPostid && c.EDITSTATE == "1"
                                             select new V_EMPLOYEEPOSTFLOWUSER
                                             {
                                                 EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                                 EMPLOYEENAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                 EMPLOYEEPOSTID = c.EMPLOYEEPOSTID,
                                                 POSTID = c.T_HR_POST.POSTID,
                                                 PostName = c.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                                                 DepartmentID = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                                 DepartmentName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                 CompanyID = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                                 CompanyName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                                 POSTLEVEL = c.POSTLEVEL,
                                                 ISAGENCY = c.ISAGENCY,
                                                 ISHEAD = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTBOSSHEAD == c.T_HR_EMPLOYEE.EMPLOYEEID ? true : false,
                                                 ISSUPERIOR = false

                                             };
                        if (FatherPostents.Count() > 0)
                        {
                            foreach (var ent in FatherPostents)
                            {
                                V_FlowUserInfo FlowUser = new V_FlowUserInfo();
                                FlowUser.UserID = ent.EMPLOYEEID;
                                FlowUser.EmployeeName = ent.EMPLOYEENAME;
                                FlowUser.CompayID = ent.CompanyID;
                                FlowUser.CompayName = ent.CompanyName;
                                FlowUser.DepartmentID = ent.DepartmentID;
                                FlowUser.DepartmentName = ent.DepartmentName;
                                FlowUser.PostID = ent.POSTID;
                                FlowUser.PostName = ent.PostName;
                                FlowUser.IsHead = ent.ISHEAD;
                                FlowUser.IsSuperior = ent.ISSUPERIOR;
                                var EntDetails = from det in details
                                                 where det.CompayID == ent.CompanyID
                                         && det.PostID == ent.POSTID && det.DepartmentID == ent.DepartmentID
                                         && det.UserID == ent.EMPLOYEEID
                                                 select det;
                                if (EntDetails.Count() == 0)
                                {
                                    details.Add(FlowUser);
                                }

                                //details.Add(FlowUser);
                                //if()
                            }

                        }
                    }
                    else
                    {
                        StrMessage = "GetDepartmentHeadByDepartmentID,获取隔级岗位为空，岗位ID为：";
                        SMT.Foundation.Log.Tracer.Debug(StrMessage + postID);
                    }
                    #endregion

                }
                else
                {
                    StrMessage = "GetDepartmentHeadByDepartmentID,获取岗位为空，岗位ID为：";
                    SMT.Foundation.Log.Tracer.Debug(StrMessage + postID);
                }


            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("GetDepartmentHeadByDepartmentID" + ex.ToString());
            }
            return details;
        }
        /// <summary>
        /// 通过用户ID:（查找所在的公司、部门、岗位、角色，一个人可能同时在多个公司任职）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<V_FlowUserInfo> GetFlowUserByUserID(string userID)
        {
            List<V_FlowUserInfo> details = new List<V_FlowUserInfo>();
            //提醒信息
            string StrMessage = "";
            try
            {
                if (string.IsNullOrEmpty(userID))
                {
                    return details;
                }
                var EntEmployee = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                    where ent.EMPLOYEEID == userID
                                    select ent;
                
                //部门负责人ID                   
                if (EntEmployee.Count() > 0)
                {
                    var ents = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                               where c.T_HR_EMPLOYEE.EMPLOYEEID == userID && c.EDITSTATE == "1"
                               select new V_EMPLOYEEPOSTFLOWUSER
                               {
                                   EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                   EMPLOYEENAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                   EMPLOYEEPOSTID = c.EMPLOYEEPOSTID,
                                   POSTID = c.T_HR_POST.POSTID,
                                   PostName = c.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                                   DepartmentID = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                   DepartmentName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                   CompanyID = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                                   CompanyName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                                   POSTLEVEL = c.POSTLEVEL,
                                   ISAGENCY = c.ISAGENCY,
                                   ISHEAD = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTBOSSHEAD == userID ? true : false,
                                   ISSUPERIOR = false

                               };
                    if (ents.Count() > 0)
                    {
                        foreach (var ent in ents)
                        {
                            V_FlowUserInfo FlowUser = new V_FlowUserInfo();
                            FlowUser.UserID = userID;
                            FlowUser.EmployeeName = ent.EMPLOYEENAME;
                            FlowUser.CompayID = ent.CompanyID;
                            FlowUser.CompayName = ent.CompanyName;
                            FlowUser.DepartmentID = ent.DepartmentID;
                            FlowUser.DepartmentName = ent.DepartmentName;
                            FlowUser.PostID = ent.POSTID;
                            FlowUser.PostName = ent.PostName;
                            FlowUser.IsHead = ent.ISHEAD;
                            FlowUser.IsSuperior = ent.ISSUPERIOR;
                            details.Add(FlowUser);
                            //if()
                        }

                    }
                    else
                    {
                        StrMessage = "GetDepartmentHeadByDepartmentID,获取员工岗位信息为空,员工ID:";
                        SMT.Foundation.Log.Tracer.Debug(StrMessage + userID);
                    }
                }
                else
                {
                    StrMessage = "GetDepartmentHeadByDepartmentID,获取员工信息为空,员工ID:";
                    SMT.Foundation.Log.Tracer.Debug( StrMessage + userID);
                }
                
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("GetDepartmentHeadByDepartmentID" + ex.ToString());
            }
            return details;
        }
        /// <summary>
        /// 通过用户ID,模块代码:（查询是否使用代理人，包括所在的公司、部门、岗位、角色）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="modelCode">模块代码</param>
        /// <returns></returns>
        public V_FlowUserInfo GetAgentUser(string userID, string modelCode)
        {
            return null;
        }




        #endregion
    }
}
