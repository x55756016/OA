using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using SMT.Foundation.Log;
using SMT.SaaS.Permission.BLL.PersonnelWS;
using SMT.SaaS.Permission.CustomerModel;



namespace SMT.SaaS.Permission.BLL
{

    #region 系统角色
    public class SysRoleBLL : BaseBll<T_SYS_ROLE>
    {
        /// <summary>
        /// 根据系统类型获取角色信息
        /// </summary>u
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>角色信息列表</returns>
        public IQueryable<T_SYS_ROLE> GetSysRoleByType(string sysType)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetSysRoleByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 添加系统角色信息
        /// </summary>        
        /// <returns>添加结果</returns>
        public bool AddSysRoleInfo(T_SYS_ROLE RoleObj)
        {
            try
            {
                RoleObj.CREATEDATE = DateTime.Now;
                RoleObj.UPDATEDATE = DateTime.Now;
                int i = dal.Add(RoleObj);
                if (i == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-AddSysRoleInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                //throw (ex);
            }
        }

        /// <summary>
        /// 添加系统角色信息  给用户权限申请使用
        /// </summary>        
        /// <returns>添加结果</returns>
        public bool AddSysRoleInfoForRoleApp(T_SYS_ROLE RoleObj)
        {
            try
            {
                RoleObj.CREATEDATE = DateTime.Now;
                RoleObj.UPDATEDATE = DateTime.Now;
                int i = dal.Add(RoleObj);
                if (i > 0)
                {
                    SaveMyRecord(RoleObj);//添加到我的单据中
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-AddSysRoleInfoForRoleApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                //throw (ex);
            }
        }
        

        /// <summary>
        /// 根据系统角色ID获取角色信息
        /// </summary>
        /// <param name="sysType">角色ID</param>
        /// <returns>角色信息</returns>
        public T_SYS_ROLE GetSysRoleByID(string roleID)
        {
            try
            {
                var ents = from ent in GetObjects()
                           where ent.ROLEID == roleID
                           select ent;

                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetSysRoleByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据系统角色ID获取角色信息 加了  缓存
        /// </summary>
        /// <param name="sysType">角色ID</param>
        /// <returns>角色信息</returns>
        public T_SYS_ROLE GetSysRoleByIDToCustomer(string roleID)
        {
            try
            {
                T_SYS_ROLE lsdic;
                string keyString = "GetSysRoleByIDToCustomer" + roleID;
                if (CacheManager.GetCache(keyString) != null)
                {
                    lsdic = (T_SYS_ROLE)CacheManager.GetCache(keyString);
                }
                else
                {
                    var ents = from ent in GetObjects()
                               where ent.ROLEID == roleID
                               select ent;
                    lsdic = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    
                    CacheManager.AddCache(keyString, lsdic);
                }


                return lsdic;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetSysRoleByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 返回是否存在相同的
        /// </summary>
        /// <param name="StrRoleName"></param>
        /// <returns>返回真假，如果存在则为正  否则假</returns>
        public bool GetExistRoleInfoByRoleName(string StrRoleName)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetTable()
                        where ent.ROLENAME == StrRoleName
                        select ent;
                if (q.Count() > 0)
                {
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetExistRoleInfoByRoleName" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 返回是否存在相同的角色  根据不同的公司
        /// </summary>
        /// <param name="StrRoleName"></param>
        /// <returns>返回真假，如果存在则为正  否则假</returns>
        public bool GetExistRoleInfoByRoleNameAndComapnyID(string StrRoleName,string StrCompanyID)
        {
            bool IsExist = false;
            try
            {
                var q = from ent in dal.GetTable()
                        where ent.ROLENAME == StrRoleName && ent.OWNERCOMPANYID == StrCompanyID
                        select ent;
                if (q.Count() > 0)
                {
                    IsExist = true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetExistRoleInfoByRoleNameAndComapnyID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsExist = false;
            }
            return IsExist;

        }


        /// <summary>
        /// 返回是否存在相同的角色  根据不同的公司
        /// </summary>
        /// <param name="StrRoleName"></param>
        /// <returns>返回真假，如果存在则为正  否则假</returns>
        public bool GetExistRoleInfoByRoleNameAndComapnyIDAndDepartmentid(string StrRoleName, string StrCompanyID,string StrDepartmentid)
        {
            bool IsExist = false;
            try
            {
                var q = from ent in dal.GetTable()
                        where ent.ROLENAME == StrRoleName && ent.OWNERCOMPANYID == StrCompanyID && ent.OWNERDEPARTMENTID == StrDepartmentid
                        select ent;
                if (q.Count() > 0)
                {
                    IsExist = true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetExistRoleInfoByRoleNameAndComapnyID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsExist = false;
            }
            return IsExist;

        }
        


        /// <summary>
        /// 修改系统菜单
        /// </summary>
        /// <param name="entity">被修改的菜单的实体</param>
        public string SysRoleUpdate(T_SYS_ROLE sourceEntity,ref string StrResult)
        {
            dal.BeginTransaction();
            try
            {
                //this.Update(sourceEntity);
                //dal.Update(sourceEntity);
                var ents = from ent in dal.GetTable()
                           where ent.ROLEID == sourceEntity.ROLEID
                           select ent;

                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    string roleName = ent.ROLENAME;
                    //ent.T_SYS_ROLE2 = sourceEntity.T_SYS_ROLE2;
                    ent.SYSTEMTYPE = sourceEntity.SYSTEMTYPE;
                    ent.ROLENAME = sourceEntity.ROLENAME;
                    ent.REMARK = sourceEntity.REMARK;
                    ent.OWNERCOMPANYID = sourceEntity.OWNERCOMPANYID;
                    ent.CHECKSTATE = sourceEntity.CHECKSTATE;
                    //ent.CREATEDATE = sourceEntity.CREATEDATE;
                    ent.OWNERDEPARTMENTID = sourceEntity.OWNERDEPARTMENTID;
                    ent.UPDATEDATE = DateTime.Now;
                    ent.UPDATEUSER = sourceEntity.UPDATEUSER;
                    ent.UPDATEUSERNAME = sourceEntity.UPDATEUSERNAME;
                    
                    //ent.T_SYS_ROLE2Reference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", sourceEntity.T_SYS_ROLE2.ROLEID);

                    int i=dal.Update(ent);
                    if (i == 0)
                    {
                        StrResult = "ERROR";
                    }
                    else
                    { 
                        //如果是审核通过的则将该角色给申请人加上该角色
                        if (sourceEntity.CHECKSTATE == "2")
                        {
                            SysUserRoleBLL roleBll = new SysUserRoleBLL();
                            if (roleBll.AddUserRoleInfo(sourceEntity.OWNERID, sourceEntity))
                                dal.CommitTransaction();
                            else
                                dal.RollbackTransaction();
                        }
                        else
                        {
                            dal.CommitTransaction();
                        }
                        try
                        {
                            bool flag = outInterfaceClient.UpdateFlowRole(ent.ROLEID, sourceEntity.ROLENAME, roleName, sourceEntity.UPDATEUSERNAME);
                            if (!flag)
                            {
                                Tracer.Debug(DateTime.Now.ToString() + "调用UpdateFlowRole方法返回false，角色ID:" + ent.ROLEID);
                            }
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug(DateTime.Now.ToString() + "调用UpdateFlowRole方法出错：" + ex.ToString() + "角色ID：" + ent.ROLEID);
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-SysRoleUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrResult = "ERROR";
                dal.RollbackTransaction();
                return "抱歉!修改角色信息出错,可能是角色所属部门或公司已失效.";
                //throw (ex);
            }
        }


        /// <summary>
        /// 修改系统菜单
        /// </summary>
        /// <param name="entity">被修改的菜单的实体</param>
        public bool SysRoleUpdateByCheck(T_SYS_ROLE sourceEntity)
        {
            try
            {
               return base.Update(sourceEntity);
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-SysRoleUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //StrResult = "ERROR";
                //throw (ex);
            }
            return false;
        }


        /// <summary>
        /// 修改系统菜单
        /// </summary>
        /// <param name="entity">被修改的菜单的实体</param>
        public bool SysRoleUpdateByCheckForRoleApp(T_SYS_ROLE sourceEntity)
        {
            try
            {
                bool IsResult = base.Update(sourceEntity);
                if (IsResult)
                {
                    SaveMyRecord(sourceEntity);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-SysRoleUpdateByCheckForRoleApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //StrResult = "ERROR";
                //throw (ex);
            }
            return false;
        }


        /// <summary>
        /// 获取所有的系统角色信息
        /// </summary>
        /// <returns>返回系统角色列表</returns>
        public List<T_SYS_ROLE> GetAllSysRoleInfos(string Systype,string StrCompanyID)
        {
            try
            {
                var ents = from ent in dal.GetTable()
                           
                           select ent;
                if (!string.IsNullOrEmpty(Systype))
                {
                    ents = ents.Where(s => s.SYSTEMTYPE == Systype);
                }
                if (!string.IsNullOrEmpty(StrCompanyID))
                {
                    ents = ents.Where(s => s.OWNERCOMPANYID == StrCompanyID);
                }

                ents = ents.OrderByDescending(k=>k.CREATEDATE);
                
                return ents.Count() > 0 ? ents.ToList() : null;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetAllSysRoleInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                //throw (ex);
            }
        }


        /// <summary>
        /// 获取所有的系统角色信息
        /// </summary>
        /// <returns>返回系统角色列表</returns>
        public List<T_SYS_ROLE> GetAllSysRoleInfosByCompanyIdAndDepartmentID(string Systype, string StrCompanyID,string DepartmentId)
        {
            try
            {
                var ents = from ent in dal.GetObjects()

                           select ent;
                if (!string.IsNullOrEmpty(Systype))
                {
                    ents = ents.Where(s => s.SYSTEMTYPE == Systype);
                }
                if (!string.IsNullOrEmpty(StrCompanyID))
                {
                    ents = ents.Where(s => s.OWNERCOMPANYID == StrCompanyID);
                }
                if (!string.IsNullOrEmpty(DepartmentId))
                {
                    ents = ents.Where(s => s.OWNERDEPARTMENTID == DepartmentId);
                }
                ents = ents.OrderByDescending(k => k.CREATEDATE);

                return ents.Count() > 0 ? ents.ToList() : null;

            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetAllSysRoleInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                //throw (ex);
            }
        }

        public IQueryable<T_SYS_ROLE> GetAllSysRoleInfosWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in GetObjects()

                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_ROLE");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_SYS_ROLE>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetAllSysRoleInfosWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }

        }
        /// <summary>
        /// 通过公司ID过滤角色 2010-8-24 liujx
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_SYS_ROLE> GetAllSysRoleInfosWithPagingByCompanyIDs(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID,string[] CompanyIDs)
        {
            try
            {
                //SysUserBLL userbll = new SysUserBLL();
                //string aa = "";
                //string bb = "";
                //string cc = "";
                //userbll.GetUserMenuPermsByUserPermisionBllCommon("T_HR_SALARYARCHIVE", "d9b5aea7-e9fa-493b-8d1a-61539aeaceac", ref aa, ref bb, ref cc);
                //2011年12月8日修改不包含预算配置员角色
                var ents = from ent in dal.GetObjects<T_SYS_ROLE>()//DataContext.T_SYS_ROLE
                           where !ent.ROLENAME.Contains("预算配置员")
                           select ent;



               

                //BLLCommonServices.OrganizationWS.OrganizationServiceClient clinet = new BLLCommonServices.OrganizationWS.OrganizationServiceClient();

                //var q = from cp in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                //        join n in dal.GetObjects<T_SYS_PERMISSION>() on cp.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                //        join m in dal.GetObjects<T_SYS_ENTITYMENU>() on cp.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                //        join r in dal.GetObjects<T_SYS_ROLE>() on cp.T_SYS_ROLE.ROLEID equals r.ROLEID
                //        join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                //        where ur.T_SYS_USER.SYSUSERID == userID
                //        && m.ENTITYCODE == "T_HR_DEPARTMENT"
                //        && n.PERMISSIONVALUE == "3"//查看部门的权限
                //        select cp;
                //if (q.Count() > 0)
                //{
                //    foreach (var item in q.ToList())
                //    {
                //        ownerDepartmentids.Add(item.DEPARTMENTID);
                //    }
                //    foreach (var item in q.ToList())
                //    {
                //        SMT.SaaS.BLLCommonServices.OrganizationWS.T_HR_DEPARTMENT[] departMents = clinet.GetDepartmentActivedByCompanyID(item.COMPANYID);
                //        if (departMents.Length > 0)
                //        {
                //            foreach (var dep in departMents)
                //            {
                //                ownerDepartmentids.Add(dep.DEPARTMENTID);
                //            }
                //        }
                //    }
                    
                //}
                List<string> ownerCompanyids = new List<string>();
                V_EMPLOYEEPOST ep = personnelclient.GetEmployeeDetailByID(userID);
                if (ep.EMPLOYEEPOSTS.Count() > 0)
                {
                    foreach (var item in ep.EMPLOYEEPOSTS.ToList())
                    {

                        ownerCompanyids.Add(item.T_HR_POST.COMPANYID);
                          
                    }
                }

                //if (ownerCompanyids.Count() > 0)
                //{
                //    ents = from ent in ents
                //           where ownerCompanyids.Contains(ent.OWNERCOMPANYID)
                //           select ent;//增加员工所有岗位的部门id过滤条件-ken2013-11-1
                //}
                //if (CompanyIDs.Count() > 0)
                //{
                //    string ownerCompanyid = CompanyIDs[0];
                //    ents = ents.Where(p => ownerCompanyid.Contains(p.OWNERCOMPANYID));//增加员工所有岗位的部门id过滤条件-ken2013-11-1
                //}
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                //BLLCommonServices.Utility aa = new BLLCommonServices.Utility();
                //aa.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_DEPARTMENT");


                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        //ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                        ents = ents.Where(filterString, queryParas.ToArray());

                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_SYS_ROLE>(ents, pageIndex, pageSize, ref pageCount);
              
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetAllSysRoleInfosWithPagingByCompanyIDs" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }

        public IQueryable<T_SYS_ROLE_V> GetRoleView(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string[] CompanyIDs)
        {
            try
            {
                var ents = GetAllSysRoleInfosWithPagingByCompanyIDs(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, CompanyIDs);

                List<T_SYS_ROLE_V> roles=new List<T_SYS_ROLE_V>();
                foreach(var item in ents.ToList())
                {
                    T_SYS_ROLE_V role=new T_SYS_ROLE_V();
                    role.ROLEID= item.ROLEID;
                    role.ROLENAME=item.ROLENAME;
                    role.OWNERDEPARTMENTNAME=GetDepartMentNameByid(item.OWNERDEPARTMENTID);
                    role.OWNERCOMPANYNAME=GetCompanNameByid(item.OWNERCOMPANYID);
                    role.SYSTTMTYPENAME = item.SYSTEMTYPE;
                    role.UPDATEUSERNAME = item.UPDATEUSERNAME;
                    role.UPDATEDATE = item.UPDATEDATE;
                    roles.Add(role);
                }
                return roles.AsQueryable();

            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-GetAllSysRoleInfosWithPagingByCompanyIDs" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;

            }

        }

        /// <summary>
        /// 批量删除角色信息
        /// </summary>
        /// <param name="ArrSysRoleIDs"></param>
        /// <returns></returns>
        public string BatchDeleteSysRoleInfos(string[] ArrSysRoleIDs)
        {
            string StrReturn = "";
            dal.BeginTransaction();
            try
            {
                //ComuumDAL<T_SYS_ROLEENTITYMENU> tmpdal = new CommDAL<T_SYS_ROLEENTITYMENU>();
                
                ////角色菜单权限
                //var tmpEnts = (from ent in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE")
                //               where ent.T_SYS_ROLE!=null &&  ArrSysRoleIDs.Contains(ent.T_SYS_ROLE.ROLEID)
                //               select ent);
                
                //if (tmpEnts != null)
                //{
                //    if (tmpEnts.Count() > 0)
                //    {
                //        //TODO:多语言与自定义异常
                //       //throw new Exception("此角色已关联角色，请先删除角色角色关联！");
                //        StrReturn = "有角色和菜单关联，不能删除";
                //    }
                //}
                //CommDAL<T_SYS_USERROLE> tmpURdal = new CommDAL<T_SYS_USERROLE>();
                var tmpUREnts = (from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_ROLE").Include("T_SYS_USER")
                                 where ent.T_SYS_ROLE !=null && ArrSysRoleIDs.Contains(ent.T_SYS_ROLE.ROLEID)
                                 select ent);
                if (tmpUREnts != null)
                {
                    if (tmpUREnts.Count() > 0)
                    {
                        //TODO:多语言与自定义异常
                        //throw new Exception("此角色已关联用户，请先删除用户角色关联！");
                        StrReturn = "以下用户和角色关联，不能删除：";
                        bool noUser = true;
                        int pos = 0;
                        using (SysUserRoleBLL surBll = new SysUserRoleBLL())
                        {
                            foreach (var user in tmpUREnts)
                            {
                                string userName;
                                if (user.T_SYS_USER != null)
                                {
                                    //离职的账户直接自动删除关联的角色
                                    if (user.T_SYS_USER.STATE == "1")
                                    {
                                        userName = user.T_SYS_USER.EMPLOYEENAME;
                                        if (pos != 0) StrReturn += "，";
                                        StrReturn += userName;
                                        pos++;
                                        noUser = false;
                                    }
                                    else
                                    {
                                        var userRoles = from ur in dal.GetObjects<T_SYS_USERROLE>()
                                                        where ur.T_SYS_USER.SYSUSERID == user.T_SYS_USER.SYSUSERID
                                                        select ur;
                                        if (userRoles != null)
                                        {
                                            foreach (var u in userRoles)
                                            {
                                                surBll.SysUserRoleDelete(u.USERROLEID);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //在职的用户，
                        if (noUser)
                        {
                            StrReturn = string.Empty;
                        }
                    }
                }

                //查看角色和自定义权限是否有关联
                var tmpCustomerEnts = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>().Include("T_SYS_ROLE")
                                      where ent.T_SYS_ROLE != null && ArrSysRoleIDs.Contains(ent.T_SYS_ROLE.ROLEID)
                                      select ent;
                if (tmpCustomerEnts != null)
                {
                    if (tmpCustomerEnts.Count() > 0)
                    {
                        //TODO:多语言与自定义异常
                        //throw new Exception("此角色已关联角色，请先删除角色角色关联！");
                        // StrReturn = "有角色和自定义权限有关联，不能删除";
                        foreach (var item in tmpCustomerEnts)
                        {
                            dal.DeleteFromContext(item);
                        }
                    }
                }

                //自动删除角色与菜单的关联
                var tmpEnts = (from ent in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE")
                               where ent.T_SYS_ROLE != null && ArrSysRoleIDs.Contains(ent.T_SYS_ROLE.ROLEID)
                               select ent);
                int recordREM = 0;
                if (string.IsNullOrEmpty(StrReturn) && tmpEnts != null)
                {
                    if (tmpEnts.Count() > 0)
                    {
                        //CommDAL<T_SYS_ROLEENTITYMENU> dalREM = new CommDAL<T_SYS_ROLEENTITYMENU>();
                        //dalREM.DeleteFromContext(tmpEnts);
                        foreach (var te in tmpEnts)
                        {
                            //删除T_SYS_ROLEMENUPERMISSION表的信息
                            var tmpPrmn = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>()
                                          where ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == te.ROLEENTITYMENUID
                                          select ent;
                            if (tmpPrmn.Count() > 0)
                            {
                                foreach (var tp in tmpPrmn)
                                {
                                    dal.DeleteFromContext(tp);
                                }
                            }
                            dal.DeleteFromContext(te);
                        }
                        recordREM = dal.SaveContextChanges();
                        
                    }
                }

                if (string.IsNullOrEmpty(StrReturn))
                {


                    var entitys = from ent in dal.GetObjects()
                                  where ArrSysRoleIDs.Contains(ent.ROLEID)
                                  select ent;
                    //var entitys = from ent in DataContext.T_SYS_ROLE.ToList()// dal.GetTable().ToList()
                    //              where ArrSysRoleIDs.Contains(ent.ROLEID)
                    //              select ent;
                    if (entitys.Count() > 0)
                    {
                        foreach (var obj in entitys)
                        {
                            dal.DeleteFromContext(obj);
                        }
                        int i = dal.SaveContextChanges();
                        if (i > 0)
                        {
                            dal.CommitTransaction();
                        }
                        else
                        {
                            dal.RollbackTransaction();
                            StrReturn = "error";
                        }
                      
                                                
                    }
                }
                
                return StrReturn;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色SysRoleBLL-BatchDeleteSysRoleInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                dal.RollbackTransaction();
                return "error";
                //throw (ex);
            }
        } 

        ///// <summary>
        ///// 删除角色
        ///// </summary>
        ///// <param name="menuID">角色ID</param>
        ///// <returns>是否删除成功</returns>
        //public bool SysRoleDelete(string roleID)
        //{

        //    try
        //    {
        //        //在用户角色与角色角色中存在的不能删除
        //        CommDAL<T_us> tmpdal = new CommDAL<T_SYS_ROLE_PERM>();
        //        var tmpEnts = (from ent in tmpdal.GetTable()
        //                       where ent.T_SYS_ROLE.ROLEID == roleID
        //                       select ent);
        //        if (tmpEnts.Count() > 0)
        //        {
        //            //TODO:多语言与自定义异常
        //            throw new Exception("此角色已关联角色，请先删除角色角色关联！");
        //        }


        //        CommDAL<T_SYS_USER_ROLE> tmpURdal = new CommDAL<T_SYS_USER_ROLE>();
        //        var tmpUREnts = (from ent in tmpdal.GetTable()
        //                         where ent.T_SYS_ROLE.ROLEID == roleID
        //                         select ent);
        //        if (tmpUREnts.Count() > 0)
        //        {
        //            //TODO:多语言与自定义异常
        //            throw new Exception("此角色已关联用户，请先删除用户角色关联！");
        //        }

        //        var entitys = (from ent in dal.GetTable()
        //                       where ent.ROLEID == roleID
        //                       select ent);
        //        if (entitys.Count() > 0)
        //        {
        //            var entity = entitys.FirstOrDefault();
        //            dal.Delete(entity);
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //}
        public IQueryable<T_SYS_ROLE> GetUserRoleByUser(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID, List<string> guidStringList, string checkState)
        {
            var q = from ent in dal.GetObjects<T_SYS_ROLE>()
                    select ent;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    q = from ent in q
                        where guidStringList.Contains(ent.ROLEID)
                        select ent;
                    //q = q.ToList().Where(x => guidStringList.Contains(x.APPROVALID)).AsQueryable();
                }
            }
            else//创建人
            {
                //q = q.Where(ent => ent.CREATEUSERID == userID);
                if (checkState != "5")
                {
                    q = q.Where(ent => ent.CHECKSTATE == checkState);
                }
            }
            List<object> queryParas = new List<object>();
            if (paras != null)
            {
                queryParas.AddRange(paras);
            }
            string bb = filterString;
            if (guidStringList == null)
            {
                if (!(filterString.IndexOf("OWNERID") > -1))
                {
                    SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_ROLE");
                }

            }
            
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            
            q = Utility.Pager<T_SYS_ROLE>(q, pageIndex, pageSize, ref pageCount);
            
            if (q.Count() > 0)
            {
                return q;
            }
            

            return null;
        }


        public void SetOrganizationFilter(ref string filterString, ref List<object> queryParas, string employeeID, string entityName)
        {
            //注意，用户总是能看到自己创建的记录

            //获取正常的角色用户权限            
            try
            {
                int maxPerm = -1;
                T_SYS_USER CacheUser = new T_SYS_USER();
                T_HR_EMPLOYEE CachePerson = new T_HR_EMPLOYEE();
                PersonnelServiceClient personclient = new PersonnelServiceClient();
                //获取权限管理中用户信息使用缓存
                //PermissionWS.T_SYS_USER user = PermClient.GetUserByEmployeeID(employeeID);
                using (SysUserBLL userbll = new SysUserBLL())
                {
                    CacheUser = userbll.GetUserByEmployeeID(employeeID);
                    CachePerson = personclient.GetEmployeeByID(employeeID);
                    string OwnerCompanyIDs = "";
                    string OwnerDepartmentIDs = "";
                    string OwnerPositionIDs = "";

                    IQueryable<V_BllCommonUserPermission> perms = userbll.GetUserMenuPermsByUserPermisionBllCommon(entityName, employeeID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);
                    bool hasPerms = true;
                    bool hasCustomerPerms = true;//自定义权限
                    if ((perms == null || perms.Count() <= 0) && string.IsNullOrEmpty(OwnerCompanyIDs) && string.IsNullOrEmpty(OwnerDepartmentIDs) && string.IsNullOrEmpty(OwnerPositionIDs))
                    {
                        hasPerms = false;
                        hasCustomerPerms = false;
                        if (!string.IsNullOrEmpty(filterString))
                            filterString += " AND ";

                        filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                        queryParas.Add(employeeID);

                        filterString += ") OR CREATEUSER==@" + queryParas.Count().ToString() + ")";
                        queryParas.Add(employeeID);
                        return;
                    }
                    else
                    {
                        //有权限的情况下进行判断  否则查看自定义权限
                        if (perms.Count() > 0)
                        {
                            var tmpperms = perms.Where(p => p.PermissionDataRange == "3").ToList();
                            if (tmpperms.Count > 0)
                                maxPerm = tmpperms.Min(p => Convert.ToInt32(p.RoleMenuPermissionValue));
                        }
                    }

                    

                    //取员工岗位
                    PersonnelServiceClient PersonClient = new PersonnelServiceClient();
                    V_EMPLOYEEPOST emppost = PersonClient.GetEmployeeDetailByID(employeeID);

                    CachePerson.T_HR_EMPLOYEEPOST = emppost.EMPLOYEEPOSTS;


                    //获取自定义权限  20100914注释  目前没使用自定义权限 
                    //int custPerm = GetCustomPerms(entityName, CachePerson);
                    //if (custPerm < maxPerm)
                    //    maxPerm = custPerm;

                    //看整个集团的
                    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(SMT.SaaS.Permission.BLL.Utility.PermissionRange.Organize))
                    {
                        return;
                    }
                    //看整个公司的
                    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(SMT.SaaS.Permission.BLL.Utility.PermissionRange.Company))
                    {
                        if (!string.IsNullOrEmpty(filterString))
                            filterString += " AND ";

                        filterString += " ((";
                        int i = 0;
                        foreach (T_HR_EMPLOYEEPOST ep in CachePerson.T_HR_EMPLOYEEPOST)
                        {

                            if (i > 0)
                                filterString += " OR ";

                            filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();

                            queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);

                            i++;
                        }
                        filterString += ") OR CREATEUSER==@" + queryParas.Count().ToString() + ")";
                        queryParas.Add(employeeID);

                    }
                    //看部门的
                    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(SMT.SaaS.Permission.BLL.Utility.PermissionRange.Department))
                    {
                        if (!string.IsNullOrEmpty(filterString))
                            filterString += " AND ";

                        filterString += " ((";
                        int i = 0;
                        foreach (T_HR_EMPLOYEEPOST ep in CachePerson.T_HR_EMPLOYEEPOST)
                        {

                            if (i > 0)
                                filterString += " OR ";

                            filterString += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();

                            queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);

                            i++;
                        }

                        filterString += ") OR CREATEUSER==@" + queryParas.Count().ToString() + ")";
                        queryParas.Add(employeeID);
                    }
                    //看岗位的
                    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(SMT.SaaS.Permission.BLL.Utility.PermissionRange.Post))
                    {
                        if (!string.IsNullOrEmpty(filterString))
                            filterString += " AND ";

                        filterString += " ((";
                        int i = 0;
                        foreach (T_HR_EMPLOYEEPOST ep in CachePerson.T_HR_EMPLOYEEPOST)
                        {

                            if (i > 0)
                                filterString += " OR ";

                            filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();

                            queryParas.Add(ep.T_HR_POST.POSTID);

                            i++;
                        }
                        filterString += ") OR CREATEUSER==@" + queryParas.Count().ToString() + ")";
                        queryParas.Add(employeeID);
                    }
                    //看员工
                    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(SMT.SaaS.Permission.BLL.Utility.PermissionRange.Employee) || maxPerm == -1)
                    {
                        if (!string.IsNullOrEmpty(filterString))
                            filterString += " AND ";

                        filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                        queryParas.Add(employeeID);

                        filterString += ") OR CREATEUSER==@" + queryParas.Count().ToString() + ")";
                        queryParas.Add(employeeID);
                    }



                    //自定义权限存在公司集合
                    if (!string.IsNullOrEmpty(OwnerCompanyIDs))
                    {
                        filterString += " OR ";
                        filterString += " (";
                        string[] ArrCompany = OwnerCompanyIDs.Split(',');
                        for (int i = 0; i < ArrCompany.Length; i++)
                        {

                            if (!string.IsNullOrEmpty(ArrCompany[i].Trim()))
                            {
                                if (i > 0) filterString += " OR ";
                                filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();
                                queryParas.Add(ArrCompany[i]);
                            }
                        }
                        filterString += ") ";
                    }
                    //看自定义权限中的部门信息
                    if (!string.IsNullOrEmpty(OwnerDepartmentIDs))
                    {
                        filterString += " OR ";
                        filterString += " (";
                        string[] ArrDepartment = OwnerDepartmentIDs.Split(',');
                        for (int i = 0; i < ArrDepartment.Length; i++)
                        {
                            if (i > 0) filterString += " OR ";
                            filterString += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();
                            queryParas.Add(ArrDepartment[i]);
                        }
                        filterString += ") ";
                    }
                    //看自定义权限中岗位的信息
                    if (!string.IsNullOrEmpty(OwnerPositionIDs))
                    {
                        filterString += " OR ";
                        filterString += " (";
                        string[] ArrPosition = OwnerPositionIDs.Split(',');
                        for (int i = 0; i < ArrPosition.Length; i++)
                        {
                            if (i > 0) filterString += " OR ";
                            filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();
                            queryParas.Add(ArrPosition[i]);
                        }
                        filterString += ") ";
                    }
                }
            }
            catch (Exception ex)
            {
                //有异常只返回自己创建的或属于自己的信息
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " AND ";

                filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                queryParas.Add(employeeID);

                filterString += ") OR CREATEUSER==@" + queryParas.Count().ToString() + ")";
                queryParas.Add(employeeID);

            }

        }
    }
    #endregion


}
