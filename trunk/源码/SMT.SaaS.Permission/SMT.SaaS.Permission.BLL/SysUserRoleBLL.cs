using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using SMT.Foundation.Log;
using SMT.SaaS.BLLCommonServices.PersonnelWS;

namespace SMT.SaaS.Permission.BLL
{
    public class SysUserRoleBLL : BaseBll<T_SYS_USERROLE>
    {

        private SysUserRoleDAL UserRoleDal = new SysUserRoleDAL();

        /// <summary>
        /// 根据系统类型获取用户角色信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统用户角色</param>
        /// <returns>用户角色信息列表</returns>
        public IQueryable<T_SYS_USERROLE> GetSysUserRoleByType(string sysType)
        {
            try
            {
                var ents = from a in base.GetObjects().Include("T_SYS_USER").Include("T_SYS_ROLE")
                           where string.IsNullOrEmpty(sysType) || a.T_SYS_ROLE.SYSTEMTYPE == sysType
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-GetSysUserRoleByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 根据用户角色ID获取用户角色信息
        /// </summary>
        /// <param name="sysType">用户角色ID</param>
        /// <returns>用户角色信息</returns>
        public T_SYS_USERROLE GetSysUserRoleByID(string userRoleID)
        {
            try
            {
                //var ents = from ent in base.GetObjects().Include("T_SYS_USER").Include("T_SYS_ROLE")
                //           where ent.T_SYS_USER.SYSUSERID == userRoleID
                //           select ent;
                var ents = from ent in base.GetObjects().Include("T_SYS_USER").Include("T_SYS_ROLE")
                           where ent.USERROLEID == userRoleID
                           select ent;
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-GetSysUserRoleByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        


        /// <summary>
        /// 根据用户获取用户角色信息
        /// </summary>
        /// <param name="sysType">用户</param>
        /// <returns>用户角色信息列表</returns>
        public IQueryable<T_SYS_USERROLE> GetSysUserRoleByUser(string userID)
        {
            try
            {
                string str = "预算配置员";
                var ents = from ent in GetObjects().Include("T_SYS_USER").Include("T_SYS_ROLE")
                           where ent.T_SYS_USER.SYSUSERID == userID
                           //&& !ent.T_SYS_ROLE.ROLENAME.Contains("预算配置员")
                           //&& !str.Contains(ent.T_SYS_ROLE.ROLENAME)
                           select ent;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-GetSysUserRoleByUser" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 根据角色获取该角色下的所有用户信息
        /// </summary>
        /// <param name="roleID">角色ID</param>
        /// <returns>用户列表</returns>
        public IQueryable<T_SYS_USER> GetSysUserByRole(string roleID)
        {
            try
            {
                //获取用户角色信息                
                var userroles = from ent in GetObjects().Include("T_SYS_USER").Include("T_SYS_ROLE")
                           where ent.T_SYS_ROLE.ROLEID == roleID
                           select ent.T_SYS_USER;

                List<string> UserIds = new List<string>();
                if (userroles.Count() > 0)
                {
                    userroles.ToList().ForEach(item =>
                    {
                        UserIds.Add(item.EMPLOYEEID);
                    });

                    if (UserIds.Count() > 0)
                    {
                        PersonnelServiceClient client = new PersonnelServiceClient();
                        V_EMPLOYEEDETAIL[] Employees = client.GetEmployeesPostBriefByEmployeeID(UserIds.ToArray());
                        V_EMPLOYEEPOST[] Employeesq = client.GetEmployeeDetailByIDs(UserIds.ToArray());
                        Employeesq.ToList().ForEach(item => { 
                            //item.EMPLOYEEPOSTS[0].
                        });
                        List<T_SYS_USER> AllUsers = new List<T_SYS_USER>();
                        if (Employees.Count() > 0)
                        {
                            foreach(var item in Employees.ToList())
                            {
                                if (item.EMPLOYEEPOSTS != null)
                                {
                                    if (item.EMPLOYEEPOSTS.Count() > 0)
                                    {
                                        item.EMPLOYEEPOSTS.ToList().ForEach(itempost =>
                                        {
                                            T_SYS_USER UserInfo = new T_SYS_USER();

                                            UserInfo.SYSUSERID = Guid.NewGuid().ToString();
                                            UserInfo.EMPLOYEEID = item.EMPLOYEEID;
                                            string StrCompany = itempost.CompanyName;
                                            string strDepartment = itempost.DepartmentName;
                                            string strPost = itempost.PostName;
                                            //UserInfo.EMPLOYEENAME = StrCompany + "-" + strDepartment + "-" + strPost+"-" + item.EMPLOYEENAME;
                                            UserInfo.EMPLOYEENAME = item.EMPLOYEENAME;
                                            UserInfo.EMPLOYEECODE = item.EMPLOYEECODE;
                                            UserInfo.OWNERCOMPANYID = itempost.CompanyID;
                                            UserInfo.OWNERDEPARTMENTID = itempost.DepartmentID;
                                            UserInfo.OWNERPOSTID = itempost.POSTID;
                                            UserInfo.OWNERID = item.EMPLOYEEID;
                                            UserInfo.CREATEDATE = null;
                                            UserInfo.CREATEUSER = "";
                                            UserInfo.ISENGINEMANAGER = "";
                                            UserInfo.ISFLOWMANAGER = "";
                                            UserInfo.ISMANGER = 0;
                                            UserInfo.PASSWORD = "";
                                            UserInfo.REMARK = StrCompany + "-" + strDepartment + "-" + strPost + "-" + item.EMPLOYEENAME;
                                            UserInfo.STATE = "";
                                            UserInfo.UPDATEDATE = null;
                                            UserInfo.UPDATEUSER = "";
                                            AllUsers.Add(UserInfo);

                                        });

                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                                
                            }


                        }
                        if (AllUsers.Count() > 0)
                            return AllUsers.AsQueryable();
                        else
                            return null;
                    }
                    return null;
                }
                            
                
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-GetSysUserByRole" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        public void UserRoleBeginTransaction()
        {
            UserRoleDal.BeginTransaction();
        }
        public void UserRoleCommitTransaction()
        {
            UserRoleDal.CommitTransaction();
        }
        public void UserRoleRollbackTransaction()
        {
            UserRoleDal.RollbackTransaction();
        }


        /// <summary>
        /// 添加用户角色
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="RoleInfo"></param>
        /// <returns></returns>
        public bool AddUserRoleInfo(string UserID, T_SYS_ROLE RoleInfo)
        {
            bool IsReturn = true;
            try
            {
                SysUserBLL userbll = new SysUserBLL();
                T_SYS_USER UserInfo ;
                UserInfo = userbll.GetUserByEmployeeID(UserID);
                if (UserInfo == null)//没有用户则终止
                    return false;
                T_SYS_USERROLE AddUserRole = new T_SYS_USERROLE();
                AddUserRole.USERROLEID = System.Guid.NewGuid().ToString();

                AddUserRole.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", RoleInfo.ROLEID);
                AddUserRole.T_SYS_USERReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_USER", "SYSUSERID", UserInfo.SYSUSERID);
                AddUserRole.OWNERCOMPANYID = UserInfo.OWNERCOMPANYID;
                AddUserRole.POSTID = UserInfo.OWNERPOSTID;

                AddUserRole.UPDATEDATE = System.DateTime.Now;
                AddUserRole.UPDATEUSER = UserID;
                AddUserRole.CREATEUSER = UserID;
                AddUserRole.CREATEDATE = System.DateTime.Now;
                int i = UserRoleDal.Add(AddUserRole);
                if (!(i > 0))
                    IsReturn = false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-AddUserRoleInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsReturn = false;
            }
            return IsReturn;
        }


        public bool BatchAddUserRoleInfo(T_SYS_USER objuser, T_SYS_ROLE[] RoleList, string StrUserName, DateTime AddDate)
        {
            try
            {
                string StrReturn = "";
                if (RoleList.Count() > 0)
                {
                    this.UserRoleBeginTransaction();
                    try
                    {
                        bool delresult = this.DelUserRoleInfos(objuser.SYSUSERID);
                        if (delresult)
                        {
                            //string StrFormID = "";
                            foreach (T_SYS_ROLE tmprole in RoleList)
                            {
                                T_SYS_USERROLE AddUserRole = new T_SYS_USERROLE();
                                AddUserRole.USERROLEID = System.Guid.NewGuid().ToString();

                                AddUserRole.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", tmprole.ROLEID);
                                AddUserRole.T_SYS_USERReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_USER", "SYSUSERID", objuser.SYSUSERID);
                                AddUserRole.OWNERCOMPANYID = objuser.OWNERCOMPANYID;


                                AddUserRole.UPDATEDATE = System.DateTime.Now;
                                AddUserRole.UPDATEUSER = StrUserName;
                                AddUserRole.CREATEUSER = StrUserName;
                                AddUserRole.CREATEDATE = AddDate;
                                int i = UserRoleDal.Add(AddUserRole);

                                if (i == 3)
                                {
                                    StrReturn = "";
                                }
                                else
                                {
                                    StrReturn = "false";
                                }
                            }
                        }
                        //DataContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        StrReturn = "bbbb";
                    }


                    if (StrReturn != "")
                    {
                        this.UserRoleRollbackTransaction();
                    }
                    else
                    {
                        this.UserRoleCommitTransaction();
                        return true;
                    }
                    //return true;


                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-BatchAddUserRoleInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        private bool DelUserRoleInfos(string StrUserID)
        {
            try
            {

                var entitys = from ent in GetObjects().Include("T_SYS_USER")
                              where ent.T_SYS_USER.SYSUSERID == StrUserID
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        var infos = (from ent in UserRoleDal.GetTable()
                                     where ent.USERROLEID == obj.USERROLEID
                                     select ent);

                        if (infos.Count() > 0)
                        {
                            var entity = infos.FirstOrDefault();
                            UserRoleDal.Delete(entity);

                        }

                    }
                    return true;


                }
                else
                {
                    return true;
                }
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-DelUserRoleInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }


        
        public List<T_SYS_USERROLE> GetUserRoleListInfos(T_SYS_USER objuser, T_SYS_ROLE[] RoleList, string StrUserName, DateTime AddDate)
        {
            try
            {
                List<T_SYS_USERROLE> aa = new List<T_SYS_USERROLE>();
                foreach (T_SYS_ROLE tmprole in RoleList)
                {
                    T_SYS_USERROLE bb = new T_SYS_USERROLE();
                    bb.USERROLEID = System.Guid.NewGuid().ToString();
                    bb.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", tmprole.ROLEID);
                    bb.T_SYS_USERReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_USER", "SYSUSERID", objuser.SYSUSERID);
                    bb.UPDATEDATE = null;
                    bb.UPDATEUSER = "";
                    bb.CREATEUSER = StrUserName;
                    bb.CREATEDATE = AddDate;
                    aa.Add(bb);
                }
                return aa;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-GetUserRoleListInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        

        /// <summary>
        /// 修改用户角色
        /// </summary>
        /// <param name="entity">被修改的用户角色的实体</param>
        public void SysUserRoleUpdate(T_SYS_USERROLE sourceEntity)
        {
            try
            {
                var ents = from ent in UserRoleDal.GetTable()
                           where ent.USERROLEID == sourceEntity.USERROLEID
                           select ent;

                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    ent.CREATEDATE = sourceEntity.CREATEDATE;
                    ent.CREATEUSER = sourceEntity.CREATEUSER;
                    ent.UPDATEDATE = sourceEntity.UPDATEDATE;
                    ent.UPDATEUSER = sourceEntity.UPDATEUSER;
                    //ent.T_SYS_USER_PERIMISSIONReference.EntityKey =
                    //new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_USER", "EMPLOYEEID", sourceEntity.T_SYS_USER.EMPLOYEEID);
                    ent.T_SYS_ROLEReference.EntityKey =
                        new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", sourceEntity.T_SYS_ROLE.ROLEID);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-SysUserRoleUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //throw (ex);
            }
        }
        /// <summary>
        /// 删除用户角色
        /// </summary>
        /// <param name="menuID">用户角色ID</param>
        /// <returns>是否删除成功</returns>
        public bool SysUserRoleDelete(string id)
        {
            try
            {
                var entitys = from ent in UserRoleDal.GetTable()
                              where ent.USERROLEID == id
                              select ent;
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();

                    dal.Delete(entity);
                    return true;
                }
                //var ents = from e in GetObjects()
                //           where e.USERROLEID == id
                //           select e;

                //var ent1 = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                //if (ent1 != null)
                //{
                    
                //    DataContext.DeleteObject(ent1);
                //    int i =DataContext.SaveChanges();
                //    if ( i> 0)
                //    {
                //        return true;
                //    }
                //    else
                //    {
                //        return false;
                //    }
                    
                //}

                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-SysUserRoleDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }



        public bool BatchAddUserRoleInfoByUserRoleList(List<T_SYS_USERROLE> RoleObj,ref string StrResult)
        {
            int i = 0;
            try
            {
                string IsBreak = "";
                RoleObj.ForEach(item =>
                {
                    string roleId=item.T_SYS_ROLEReference.EntityKey.EntityKeyValues[0].Value.ToString();
                    string sysUserId=item.T_SYS_USERReference.EntityKey.EntityKeyValues[0].Value.ToString();
                    var q = from a in GetObjects()
                            where a.T_SYS_ROLE.ROLEID == roleId
                            && a.T_SYS_USER.SYSUSERID == sysUserId
                            && a.OWNERCOMPANYID == item.OWNERCOMPANYID && a.POSTID == item.POSTID && a.EMPLOYEEPOSTID == item.EMPLOYEEPOSTID
                            select a;
                    if (q.Count() == 0)
                    {
                        dal.AddToContext(item);
                    }
                    else
                    {
                        IsBreak = "ISREPEAT";                        
                    }
                });
                if (string.IsNullOrEmpty(IsBreak))
                {
                    i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        #region 调用即时通讯接口
                        
                        #endregion
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    StrResult = IsBreak;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-BatchAddUserRoleInfoByUserRoleList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 返回用户所拥有的系统类型 2010-9-20
        /// 不是管理员则不显示权限的信息
        /// </summary>
        /// <param name="StrUserId"></param>
        /// <param name="StrResult">返回值 是否有设置角色、错误</param>
        /// <returns></returns>
        public string GetSystemTypeByUserID(string StrUserId,ref string StrResult)
        {
            try
            {
                string StrReturn = "";
                var User = from ent in dal.GetObjects<T_SYS_USER>()
                           where ent.SYSUSERID == StrUserId
                           select ent;

                var UserRole = from ent in GetObjects().Include("T_SYS_ROLE")
                               where ent.T_SYS_USER.SYSUSERID == StrUserId
                               select ent.T_SYS_ROLE.ROLEID;
                string[] ArrUserRole = UserRole.ToArray();

                if (ArrUserRole.Count() > 0)
                {
                    var entspermission = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                                         join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                                         join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                                         join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                         join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                                         join ur in GetObjects() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                         where ur.T_SYS_USER.SYSUSERID == StrUserId && n.PERMISSIONVALUE == "3" && ArrUserRole.Contains(r.ROLEID)
                                         select new V_Permission
                                         {
                                             RoleMenuPermission = p,
                                             Permission = n,
                                             EntityMenu = m
                                         };
                    List<string> menuids = new List<string>();
                    if (entspermission != null && entspermission.Count() > 0)
                    {
                        foreach (var menuid in entspermission.ToList())
                        {
                            menuids.Add(menuid.EntityMenu.ENTITYMENUID);
                        }
                    }
                    var EntityList = from ent in dal.GetObjects<T_SYS_ENTITYMENU>()
                                     where menuids.Contains(ent.ENTITYMENUID)
                                     select ent.SYSTEMTYPE;
                    EntityList = EntityList.Distinct();
                    if (EntityList.Count() > 0)
                    {
                        foreach (var ent in EntityList)
                        {
                            StrReturn += ent + ",";
                        }
                        StrReturn = StrReturn.Substring(0, StrReturn.Length - 1);//去掉最后一个,


                    }
                }
                else
                {
                    StrResult = "NOSETTINGROLE";
                }

                if (User.Count() > 0)
                {
                    if (User.FirstOrDefault() != null)
                    {
                        if (User.FirstOrDefault().ISMANGER != null)
                        {
                            if (User.FirstOrDefault().ISMANGER.ToString() == "1")
                            {
                                //是管理员 则加上权限的参数
                                if (StrReturn != "")
                                {
                                    StrReturn += ",7";  // 7 为权限的systype
                                }
                                else
                                {
                                    StrReturn = "7";
                                }
                            }
                        }
                        if (User.FirstOrDefault().ISFLOWMANAGER != null)
                        {
                            if (User.FirstOrDefault().ISFLOWMANAGER.ToString() == "1")
                            {
                                //是管理员 则加上权限的参数
                                if (StrReturn != "")
                                {
                                    StrReturn += ",8";  // 7 为权限的systype
                                }
                                else
                                {
                                    StrReturn = "8";
                                }
                            }
                        }
                        if (User.FirstOrDefault().ISENGINEMANAGER != null)
                        {
                            if (User.FirstOrDefault().ISENGINEMANAGER.ToString() == "1")
                            {
                                //是管理员 则加上权限的参数
                                if (StrReturn != "")
                                {
                                    StrReturn += ",9";  // 7 为权限的systype
                                }
                                else
                                {
                                    StrReturn = "9";
                                }
                            }
                        }

                    }
                    
                    StrResult = ""; //如果是管理员 而没其它权限 则设置为正常
                }
                return StrReturn;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户角色SysUserRoleBLL-GetSystemTypeByUserID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "ERROR";
                //throw(ex);
            }
        }


        #region 调用即时通讯接口
        //public string InsertDataToImServices(List<T_SYS_USERROLE> RoleObj)
        //{
        //    string StrMessage = "";
        //    try
        //    {
        //        if (RoleObj.Count() == 0)
        //        {
        //            return StrMessage;
        //        }
        //        string MenuCompanyID = "55623178-A187-421a-8556-067E6908207A";//公司菜单ID
        //        string MenuDepartmentID = "04F86C10-02E3-4874-A198-4EC986C288CC";//部门菜单ID
        //        string PermissionId = "C3DEE881-C6E5-48e4-95A9-BBB8275A933D";//查看的权限ID
        //        RoleObj.ForEach(item => {
        //            #region 授权操作
                    
        //            var CompanyPerms = from ent in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
        //                        where ent.T_SYS_ROLE.ROLEID == item.T_SYS_ROLE.ROLEID
        //                        && ent.T_SYS_ENTITYMENU.ENTITYMENUID == MenuCompanyID
        //                        select ent;
        //            var DepartmentPerms = from ent in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
        //                               where ent.T_SYS_ROLE.ROLEID == item.T_SYS_ROLE.ROLEID
        //                               && ent.T_SYS_ENTITYMENU.ENTITYMENUID == MenuCompanyID
        //                               select ent;
        //            RoleEntityMenuBLL RoleEntity = new RoleEntityMenuBLL();
        //            string StrRoleID = item.T_SYS_ROLE.ROLEID;
        //            string StrDataRange = "";
        //            if (CompanyPerms.Count() > 0)
        //            {
        //                var PermCompanyValue = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
        //                                where ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == CompanyPerms.FirstOrDefault().ROLEENTITYMENUID
        //                                && ent.T_SYS_PERMISSION.PERMISSIONID == PermissionId
        //                                select ent;
        //                if (PermCompanyValue.Count() > 0)
        //                {
        //                    StrDataRange = PermCompanyValue.FirstOrDefault().DATARANGE;
        //                    //SMT.Foundation.Log.Tracer.Debug("用户授权公司信息开始调用即时通讯接口");
        //                    //StrMessage =RoleEntity.AddUpdateUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
        //                    //SMT.Foundation.Log.Tracer.Debug("用户授权公司查看信息调用即时通讯接口完成结果：" + StrMessage);
        //                }
                        
        //            }
        //            if (DepartmentPerms.Count() > 0)
        //            {
        //                var PermDepartmentValue = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
        //                                       where ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == CompanyPerms.FirstOrDefault().ROLEENTITYMENUID
        //                                       && ent.T_SYS_PERMISSION.PERMISSIONID == PermissionId
        //                                       select ent;
        //                if (PermDepartmentValue.Count() > 0)
        //                {
        //                    //StrDataRange = PermDepartmentValue.FirstOrDefault().DATARANGE;
        //                    //SMT.Foundation.Log.Tracer.Debug("用户授权部门查看信息开始调用即时通讯接口");
        //                    //StrMessage = RoleEntity.AddUpdateUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment);
        //                    //SMT.Foundation.Log.Tracer.Debug("用户授权部门查看授权信息调用即时通讯接口完成结果：" + StrMessage);
        //                }
                        
        //            }

        //            #endregion

        //            #region 自定义授权操作

        //            var CustomerCompanyPerms = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
        //                                           .Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE").Include("T_SYS_PERMISSION")
        //                                       where ent.T_SYS_ROLE.ROLEID == item.T_SYS_ROLE.ROLEID
        //                                       && ent.T_SYS_ENTITYMENU.ENTITYMENUID == MenuCompanyID
        //                                       && ent.T_SYS_PERMISSION.PERMISSIONID ==PermissionId
        //                                       select ent;
        //            var CustomerDepartmentPerms = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
        //                                          .Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE").Include("T_SYS_PERMISSION")
        //                                       where ent.T_SYS_ROLE.ROLEID == item.T_SYS_ROLE.ROLEID
        //                                       && ent.T_SYS_ENTITYMENU.ENTITYMENUID == MenuDepartmentID
        //                                       && ent.T_SYS_PERMISSION.PERMISSIONID == PermissionId
        //                                       select ent;
        //            if (CustomerCompanyPerms.Count() > 0)
        //            {
        //                CustomerCompanyPerms.ToList().ForEach(itemcopany => {
        //                    string StrOrgId = "";
        //                    if (!string.IsNullOrEmpty(itemcopany.COMPANYID))
        //                    {
        //                        StrOrgId = itemcopany.COMPANYID;
        //                    }
        //                    if (!string.IsNullOrEmpty(itemcopany.DEPARTMENTID))
        //                    {
        //                        StrOrgId = itemcopany.DEPARTMENTID;
        //                    }
        //                    //SMT.Foundation.Log.Tracer.Debug("用户自定义授权公司查看信息开始调用即时通讯接口");
        //                    //StrMessage = RoleEntity.AddCustomerPermissionUpdateUserDepart(StrRoleID, StrOrgId, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
        //                    //SMT.Foundation.Log.Tracer.Debug("用户自定义授权公司查看授权信息调用即时通讯接口完成结果：" + StrMessage);
        //                });
                                                
        //            }
        //            if (CustomerDepartmentPerms.Count() > 0)
        //            {
        //                CustomerDepartmentPerms.ToList().ForEach(itemdepartment => {
        //                    string StrOrgId = "";
        //                    if (!string.IsNullOrEmpty(itemdepartment.COMPANYID))
        //                    {
        //                        StrOrgId = itemdepartment.COMPANYID;
        //                    }
        //                    if (!string.IsNullOrEmpty(itemdepartment.DEPARTMENTID))
        //                    {
        //                        StrOrgId = itemdepartment.DEPARTMENTID;
        //                    }
        //                    //SMT.Foundation.Log.Tracer.Debug("用户自定义授权部门查看信息开始调用即时通讯接口");
        //                    //RoleEntity.AddCustomerPermissionUpdateUserDepart(StrRoleID, CustomerDepartmentPerms.FirstOrDefault().DEPARTMENTID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
        //                    //SMT.Foundation.Log.Tracer.Debug("用户自定义授权部门查看授权信息调用即时通讯接口完成结果：" + StrMessage);
        //                });
                        
        //            }
        //            #endregion


        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        SMT.Foundation.Log.Tracer.Debug("用户授角色时调用即时通讯接口错误"+ ex.ToString());
        //    }
        //    return StrMessage;
        //}
        #endregion


    }
}
