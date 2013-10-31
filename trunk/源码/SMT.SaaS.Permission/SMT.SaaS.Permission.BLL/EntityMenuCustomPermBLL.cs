/*
 * 文件名：EntityMenuCustomPermBLL.cs
 * 作  用：角色对应的自定义权限
 * 创建人：刘建兴
 * 创建时间：2010-2-24 9:11:36
 * 修改人：刘建兴
 * 修改说明：增加缓存
 * 修改时间：2010-10-25 22:25:32
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Log;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using SMT.SaaS.Permission.DAL.views;


namespace SMT.SaaS.Permission.BLL
{
    public class EntityMenuCustomPermBLL : BaseBll<T_SYS_ENTITYMENUCUSTOMPERM>
    {
        /// <summary>
        /// 根据系统类型获取自定义菜单权限信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统自定义菜单权限</param>
        /// <returns>自定义菜单权限信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetEntityMenuCustomPermByType(string sysType)
        {
            try
            {
                var ents = from a in base.GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION")
                           where string.IsNullOrEmpty(sysType) || a.T_SYS_ENTITYMENU.SYSTEMTYPE == sysType
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetEntityMenuCustomPermByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 添加自定义菜单权限
        /// </summary>
        /// <param name="sourceEntity">添加自定义菜单权限的实体对象</param>
        public void EntityMenuCustomPermAdd(T_SYS_ENTITYMENUCUSTOMPERM ent)
        {
            try
            {
                //存在公司信息或部门信息菜单授权则调用
                string MenuCompanyID = "55623178-A187-421a-8556-067E6908207A";//公司菜单ID
                string MenuDepartmentID = "04F86C10-02E3-4874-A198-4EC986C288CC";//部门菜单ID
                string EntityID = ent.T_SYS_ENTITYMENU.ENTITYMENUID;//菜单ID
                string StrRoleID = ent.T_SYS_ROLE.ROLEID;//角色ID
                RoleEntityMenuBLL Rolebll = new RoleEntityMenuBLL();

                if (!string.IsNullOrEmpty(ent.COMPANYID))
                {
                    var ents = from p in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                                   .Include("T_SYS_PERMISSION").Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                               where  p.COMPANYID == ent.COMPANYID  
                               && p.T_SYS_ENTITYMENU.ENTITYMENUID == ent.T_SYS_ENTITYMENU.ENTITYMENUID
                               && p.T_SYS_PERMISSION.PERMISSIONID == ent.T_SYS_PERMISSION.PERMISSIONID 
                               && p.T_SYS_ROLE.ROLEID == ent.T_SYS_ROLE.ROLEID
                               select p;

                    if (ents.Count() == 0)
                    {
                        Utility.RefreshEntity(ent);
                        base.Add(ent);

                        //if (EntityID == MenuCompanyID)
                        //{
                        //    Rolebll.AddCustomerPermissionUpdateUserDepart(StrRoleID, ent.COMPANYID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company,SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                        //}
                        //if (EntityID == MenuDepartmentID)
                        //{
                        //    Rolebll.AddCustomerPermissionUpdateUserDepart(StrRoleID, ent.COMPANYID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                        //}
                    }
                }
                //部门
                if (!string.IsNullOrEmpty(ent.DEPARTMENTID))
                {
                    var ents = from p in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                                   .Include("T_SYS_PERMISSION").Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                               where  p.DEPARTMENTID == ent.DEPARTMENTID 
                               && p.T_SYS_ENTITYMENU.ENTITYMENUID == ent.T_SYS_ENTITYMENU.ENTITYMENUID
                               && p.T_SYS_PERMISSION.PERMISSIONID == ent.T_SYS_PERMISSION.PERMISSIONID 
                               && p.T_SYS_ROLE.ROLEID == ent.T_SYS_ROLE.ROLEID
                               select p;

                    if (ents.Count() == 0)
                    {
                        Utility.RefreshEntity(ent);
                        base.Add(ent);

                        //if (EntityID == MenuCompanyID)
                        //{
                        //    Rolebll.AddCustomerPermissionUpdateUserDepart(StrRoleID, ent.DEPARTMENTID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                        //}
                        //if (EntityID == MenuDepartmentID)
                        //{
                        //    Rolebll.AddCustomerPermissionUpdateUserDepart(StrRoleID, ent.DEPARTMENTID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                        //}
                    }
                }
                //岗位
                if (!string.IsNullOrEmpty(ent.POSTID))
                {
                    var ents = from p in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                                   .Include("T_SYS_PERMISSION").Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                               where p.POSTID == ent.POSTID && p.T_SYS_ENTITYMENU.ENTITYMENUID == ent.T_SYS_ENTITYMENU.ENTITYMENUID
                               && p.T_SYS_PERMISSION.PERMISSIONID == ent.T_SYS_PERMISSION.PERMISSIONID 
                               && p.T_SYS_ROLE.ROLEID == ent.T_SYS_ROLE.ROLEID
                               select p;

                    if (ents.Count() == 0)
                    {
                        Utility.RefreshEntity(ent);
                        base.Add(ent);

                        
                    }
                }
                
                //DataContext.AddObject("T_SYS_ENTITYMENUCUSTOMPERM", ent);
                //DataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-EntityMenuCustomPermAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        /// <summary>
        /// 修改自定义菜单权限
        /// </summary>
        /// <param name="entity">被修改的自定义菜单权限的实体</param>
        public void EntityMenuCustomPermUpdate(T_SYS_ENTITYMENUCUSTOMPERM sourceEntity)
        {
            try
            {
                var ents = from p in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                               .Include("T_SYS_PERMISSION").Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                           where p.T_SYS_ENTITYMENU.ENTITYMENUID == sourceEntity.T_SYS_ENTITYMENU.ENTITYMENUID
                           && p.T_SYS_PERMISSION.PERMISSIONID == sourceEntity.T_SYS_PERMISSION.PERMISSIONID 
                           && p.T_SYS_ROLE.ROLEID == sourceEntity.T_SYS_ROLE.ROLEID
                           select p;
                if (ents.Count() > 0)
                { 
                    if(!string.IsNullOrEmpty(sourceEntity.COMPANYID))
                        ents = ents.Where(p=>p.COMPANYID == sourceEntity.COMPANYID);
                    if(!string.IsNullOrEmpty(sourceEntity.DEPARTMENTID))
                        ents = ents.Where(p=>p.COMPANYID == sourceEntity.DEPARTMENTID);
                    if (!string.IsNullOrEmpty(sourceEntity.POSTID))
                        ents = ents.Where(p => p.COMPANYID == sourceEntity.POSTID);
                }

                if (ents.Count() == 0)
                {
                    Utility.RefreshEntity(sourceEntity);
                    base.Add(sourceEntity);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-EntityMenuCustomPermUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        /// <summary>
        /// 删除自定义菜单权限
        /// </summary>
        /// <param name="menuID">自定义菜单权限ID</param>
        /// <returns>是否删除成功</returns>
        public bool EntityMenuCustomPermDelete(string id)
        {
            try
            {
                dal.BeginTransaction();
                var entitys = (from ent in dal.GetTable()
                               where ent.ENTITYMENUCUSTOMPERMID == id
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    int i = dal.Delete(entity);
                    if (i > 0)
                    {
                        dal.CommitTransaction();
                        return true;
                    }
                    else
                    {
                        dal.RollbackTransaction();
                        return false;
                    }
                    
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-EntityMenuCustomPermDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                //throw (ex);
            }
        }

        /// <summary>
        /// 根据ID获取自定义权限
        /// </summary>
        /// <param name="id">自定义权限ID</param>
        /// <returns>自定义权限</returns>
        public T_SYS_ENTITYMENUCUSTOMPERM GetEntityMenuCustomPermByID(string id)
        {
            try
            {
                var ents = from ent in GetObjects()
                               .Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION")
                           where ent.ENTITYMENUCUSTOMPERMID == id
                           select ent;
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetEntityMenuCustomPermByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据角色ID获取自定义权限
        /// </summary>
        /// <param name="id">自定义权限ID</param>
        /// <returns>自定义权限</returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomPermByRoleID(string Roleid)
        {
            try
            {
                var ents = from ent in GetObjects()
                               .Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION").Include("T_SYS_ROLE")
                           where ent.T_SYS_ROLE.ROLEID == Roleid
                           select ent;
                return ents.Count() > 0 ? ents : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomPermByRoleID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据角色ID获取自定义权限
        /// </summary>
        /// <param name="id">自定义权限ID</param>
        /// <returns>自定义权限</returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomPermByRoleIDToCustomer(string Roleid)
        {
            try
            {
                IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> lsdic;
                string keyString = "GetCustomPermByRoleIDToCustomer" + Roleid;
                if (CacheManager.GetCache(keyString) != null)
                {
                    lsdic = (IQueryable<T_SYS_ENTITYMENUCUSTOMPERM>)CacheManager.GetCache(keyString);
                }
                else
                {
                    var ents = from ent in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION").Include("T_SYS_ROLE")
                               where ent.T_SYS_ROLE.ROLEID == Roleid
                               select ent;
                    lsdic = ents.Count() > 0 ? ents : null;

                    CacheManager.AddCache(keyString, lsdic);
                }

                return lsdic;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomPermByRoleID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据角色ID获取自定义权限
        /// </summary>
        /// <param name="id">自定义权限ID</param>
        /// <returns>自定义权限</returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomPermByRoleIDAndEntityIDToCustomer(string Roleid,List<string> menus)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION").Include("T_SYS_ROLE")
                            where ent.T_SYS_ROLE.ROLEID == Roleid && menus.Contains( ent.T_SYS_ENTITYMENU.ENTITYMENUID)
                            select ent;
                return  ents.Count() > 0 ? ents : null;

            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomPermByRoleID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 根据角色ID、菜单ID、权限值ID获取自定义权限
        /// </summary>
        /// <param name="id">自定义权限ID</param>
        /// <returns>自定义权限</returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomPermByRoleIDAndEntityIDAndPermissionidToCustomer(string Roleid, string menusid,List<string> lstpermids)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION").Include("T_SYS_ROLE")
                           where ent.T_SYS_ROLE.ROLEID == Roleid 
                           && lstpermids.Contains(ent.T_SYS_PERMISSION.PERMISSIONID) 
                           && ent.T_SYS_ENTITYMENU.ENTITYMENUID == menusid
                           select ent;
                return ents.Count() > 0 ? ents : null;

            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomPermByRoleID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 根据角色ID、菜单ID、权限值ID获取自定义权限
        /// </summary>
        /// <param name="id">自定义权限ID</param>
        /// <returns>自定义权限</returns>
        public T_SYS_ENTITYMENUCUSTOMPERM GetCustomPermByRoleIDAndEntityIDAndPermissionidAndOrgIDAndOrgTypeToCustomer(string Roleid, string menusid, string permissionid,string orgid,string orgtype)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION").Include("T_SYS_ROLE")
                           where ent.T_SYS_ROLE.ROLEID == Roleid && ent.T_SYS_PERMISSION.PERMISSIONID == permissionid 
                           && ent.T_SYS_ENTITYMENU.ENTITYMENUID == menusid 
                           
                           select ent;
                if (ents.Count() > 0)
                {
                    switch (orgtype)
                    { 
                        case "0"://公司
                            ents = ents.Where(item=> item.COMPANYID == orgid);
                            break;
                        case "1"://部门
                            ents = ents.Where(item => item.DEPARTMENTID == orgid);
                            break;
                        case "2"://岗位
                            ents = ents.Where(item => item.POSTID == orgid);
                            break;
                    }
                }
                    
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;

            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomPermByRoleID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据角色ID获取自定义权限
        /// </summary>
        /// <param name="id">自定义权限ID</param>
        /// <returns>自定义权限</returns>
        public IQueryable<V_EntityMenuCustomperm> GetCustomPermByRoleIDNew(string Roleid)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION")
                           where ent.T_SYS_ROLE.ROLEID == Roleid
                           select new V_EntityMenuCustomperm { 
                               ENTITYCUSTOMERPERMISSIONID = ent.ENTITYMENUCUSTOMPERMID,
                               ENTITYMENUID = ent.T_SYS_ENTITYMENU.ENTITYMENUID,
                               PERMISSIONID = ent.T_SYS_PERMISSION.PERMISSIONID,
                               COMPANYID = ent.COMPANYID,
                               COMPANYNAME = ent.COMPANYNAME,
                               DEPARTMENTID = ent.DEPARTMENTID,
                               DEPARTMENTNAME = ent.DEPARTMENTNAME,
                               POSTID = ent.POSTID,
                               POSTNAME = ent.POSTNAME,
                               ROLEID = ent.T_SYS_ROLE.ROLEID,
                               REMARK = ent.REMARK

                           };
                return ents.Count() > 0 ? ents : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomPermByRoleID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取岗位指定菜单权限
        /// </summary>
        /// <param name="menuCode">菜单编号,菜单编号为空时返回岗位所有菜单权限</param>
        /// <param name="postID">岗位ID</param>
        /// <returns>自定义菜单权限</returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomPostMenuPerms(string menuCode, string postID)
        {
            try
            {
                var ents = from a in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION")
                           where (string.IsNullOrEmpty(menuCode) || a.T_SYS_ENTITYMENU.MENUCODE == menuCode) 
                           && a.POSTID == postID
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomPostMenuPerms" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取岗位指定菜单权限
        /// </summary>
        /// <param name="menuCode">菜单编号,菜单编号为空时返回部门所有菜单权限</param>
        /// <param name="departID">部门ID</param>
        /// <returns>自定义菜单权限</returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomDepartMenuPerms(string menuCode, string departID)
        {
            try
            {
                var ents = from a in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION")
                           where (string.IsNullOrEmpty(menuCode) || a.T_SYS_ENTITYMENU.MENUCODE == menuCode) 
                           && a.DEPARTMENTID == departID
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomDepartMenuPerms" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取岗位指定菜单权限
        /// </summary>
        /// <param name="menuCode">菜单编号,菜单编号为空时返回公司所有菜单权限</param>
        /// <param name="companyID">公司ID</param>
        /// <returns>自定义菜单权限</returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomCompanyMenuPerms(string menuCode, string companyID)
        {
            try
            {
                var ents = from a in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_PERMISSION")
                           where (string.IsNullOrEmpty(menuCode) || a.T_SYS_ENTITYMENU.MENUCODE == menuCode) 
                           && a.COMPANYID == companyID
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomCompanyMenuPerms" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 返回登录用户所对应的自定义的权限值 2010-10-15
        /// </summary>
        /// <param name="StrUserID"></param>
        /// <returns></returns>
        public IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomerPermissionByUser(string StrUserID)
        {
            try
            {
                SysUserRoleBLL UserRole = new SysUserRoleBLL();
                List<T_SYS_USERROLE> ListUserRole = new List<T_SYS_USERROLE>();
                IQueryable<T_SYS_USERROLE> IListUserRole =UserRole.GetSysUserRoleByUser(StrUserID);
                if(IListUserRole != null)
                {
                    ListUserRole = IListUserRole.ToList();
                }
                string StrUserRole = "";
                if (ListUserRole.Count() > 0)
                {
                    //ListUserRole = UserRole.GetSysUserRoleByUser(StrUserID).ToList();
                    foreach (var User in ListUserRole)
                    {
                        StrUserRole += User.T_SYS_ROLE.ROLEID.ToString() + ",";
                    }
                }
                SysRoleBLL roleBll = new SysRoleBLL();
                SysEntityMenuBLL menuBll = new SysEntityMenuBLL();
                EntityMenuCustomPermBLL customerBll = new EntityMenuCustomPermBLL();
                var CustomerEnts = from ent in customerBll.GetTable()
                                   join e in roleBll.GetTable() on ent.T_SYS_ROLE.ROLEID equals e.ROLEID
                                   join m in menuBll.GetTable() on ent.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID

                                   select ent;
                CustomerEnts = CustomerEnts.Where(p => p.T_SYS_ROLE.ROLEID.Contains(StrUserRole));
                return CustomerEnts.Count() > 0 ? CustomerEnts : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义菜单权限EntityMenuCustomPermBLL-GetCustomerPermissionByUser" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 根据登录用户 和菜单ID返回 该用户时候有自定义 按钮
        /// </summary>
        /// <param name="StrUserID"></param>
        /// <returns></returns>
        public T_SYS_ENTITYMENUCUSTOMPERM GetCustomerPermissionByUserIDAndEntityCode(string StrUserID,string StrEntityCode)
        {
            DateTime date1 = DateTime.Now;
            try
            {
                SysUserRoleBLL UserRole = new SysUserRoleBLL();
                List<T_SYS_USERROLE> ListUserRole = new List<T_SYS_USERROLE>();
                IQueryable<T_SYS_USERROLE> IListUserRole = UserRole.GetSysUserRoleByUser(StrUserID);
                if(IListUserRole != null)
                {
                    ListUserRole = IListUserRole.ToList();
                }
                string StrUserRole = "";
                if (ListUserRole.Count() > 0)
                {
                    DateTime dt1 = DateTime.Now;
                    TimeSpan ttt1 = dt1 - date1;
                    //ListUserRole = UserRole.GetSysUserRoleByUser(StrUserID).ToList();
                    DateTime tttt1 = DateTime.Now;
                    TimeSpan sp1 = tttt1 - dt1;
                    foreach (var User in ListUserRole)
                    {
                        StrUserRole += User.T_SYS_ROLE.ROLEID.ToString() + ",";
                    }
                }
                DateTime date3 = DateTime.Now;
                TimeSpan tt1 = date3 - date1;
                //SysRoleBLL roleBll = new SysRoleBLL();
                //SysEntityMenuBLL menuBll = new SysEntityMenuBLL();
                //EntityMenuCustomPermBLL customerBll = new EntityMenuCustomPermBLL();
                var ents = from ent in dal.GetObjects<T_SYS_ENTITYMENU>()
                           where ent.MENUCODE == StrEntityCode
                           select ent;
                if (ents.Count() > 0)
                {
                    var CustomerEnts = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>().Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                                       join e in dal.GetObjects<T_SYS_ROLE>() on ent.T_SYS_ROLE.ROLEID equals e.ROLEID
                                       join m in dal.GetObjects<T_SYS_ENTITYMENU>() on ent.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID

                                       select ent;
                    if (CustomerEnts.Count() > 0)
                        CustomerEnts = CustomerEnts.Where(p => p.T_SYS_ROLE.ROLEID.Contains(StrUserRole));
                    if (CustomerEnts.Count() > 0)
                        CustomerEnts = CustomerEnts.Where(p => p.T_SYS_ENTITYMENU.ENTITYMENUID == ents.FirstOrDefault().ENTITYMENUID);


                    DateTime date2 = DateTime.Now;
                    TimeSpan aa = date2 - date3;
                    return CustomerEnts.Count() > 0 ? CustomerEnts.FirstOrDefault() : null;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("自定义EntityMenuCustomPermBLL-GetCustomerPermissionByUserIDAndEntityCode" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        

        }

        public bool UpdateCustomerPermission(string RoleID, List<CustomerPermission> objs, ref string strResult)
        {
            bool IsResult = true;
            dal.BeginTransaction();
            try
            {
                
                SysEntityMenuBLL menuBll = new SysEntityMenuBLL();
                SysPermissionBLL permBll = new SysPermissionBLL();
                SysRoleBLL roleBll = new SysRoleBLL();
                IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> cuspers = GetCustomPermByRoleIDToCustomer(RoleID);
                List<T_SYS_ENTITYMENUCUSTOMPERM> tmpCustomers = new List<T_SYS_ENTITYMENUCUSTOMPERM>();//旧的没存在的数据
                List<T_SYS_ENTITYMENUCUSTOMPERM> tmpNewCustomers = new List<T_SYS_ENTITYMENUCUSTOMPERM>();//新的数据
                List<T_SYS_ENTITYMENUCUSTOMPERM> tmpNewAddCustomers = new List<T_SYS_ENTITYMENUCUSTOMPERM>();//需要添加的数据
                List<T_SYS_ENTITYMENUCUSTOMPERM> tmpDeleteCustomers = new List<T_SYS_ENTITYMENUCUSTOMPERM>();//需要删除的数据
                List<string> menuids = new List<string>();//菜单ID集合
                List<string> permissions = new List<string>();//权限值ID集合
                List<OrgObject> orgs = new List<OrgObject>();//组织架构ID集合
                string companyid = "";
                string departmentid = "";
                string postid = "";

                if (cuspers != null)
                {
                    #region 注释
                    //if (cuspers.Count() > 0)
                    //{
                    //    foreach (T_SYS_ENTITYMENUCUSTOMPERM item in cuspers)
                    //    {                            
                    //        companyid = item.COMPANYID;
                    //        departmentid = item.DEPARTMENTID;
                    //        postid = item.POSTID;
                    //        //判断公司ID是否存在
                    //        if (!string.IsNullOrEmpty(companyid))
                    //        {
                    //            OrgObject org = new OrgObject();
                    //            org.OrgID = companyid;
                    //            org.OrgType = "0";
                    //            var ents = from ent in cuspers
                    //                       where ent.COMPANYID == companyid && ent.T_SYS_PERMISSION.PERMISSIONID == item.T_SYS_PERMISSION.PERMISSIONID
                    //                             && ent.T_SYS_ENTITYMENU.ENTITYMENUID == item.T_SYS_ENTITYMENU.ENTITYMENUID
                    //                       select ent;
                    //            if (!(ents.Count() > 0))
                    //                orgs.Add(org);
                    //        }
                    //        //判断公司ID是否存在
                    //        if (!string.IsNullOrEmpty(departmentid))
                    //        {                                
                    //            OrgObject org = new OrgObject();
                    //            org.OrgID = departmentid;
                    //            org.OrgType = "1";
                    //            var ents = from ent in cuspers
                    //                       where ent.DEPARTMENTID == departmentid && ent.T_SYS_PERMISSION.PERMISSIONID == item.T_SYS_PERMISSION.PERMISSIONID
                    //                             && ent.T_SYS_ENTITYMENU.ENTITYMENUID == item.T_SYS_ENTITYMENU.ENTITYMENUID
                    //                       select ent;
                    //            if (!(ents.Count() > 0))
                    //                orgs.Add(org);
                    //        }
                    //        //判断岗位ID是否存在
                    //        if (!string.IsNullOrEmpty(postid))
                    //        {                                
                    //            OrgObject org = new OrgObject();
                    //            org.OrgID = postid;
                    //            org.OrgType = "2";

                    //            var ents = from ent in cuspers
                    //                       where ent.POSTID == postid && ent.T_SYS_PERMISSION.PERMISSIONID == item.T_SYS_PERMISSION.PERMISSIONID
                    //                             && ent.T_SYS_ENTITYMENU.ENTITYMENUID == item.T_SYS_ENTITYMENU.ENTITYMENUID
                    //                       select ent;
                    //            if (!(ents.Count() > 0))
                    //                orgs.Add(org);
                    //        }
                    //        //string strId = item.ENTITYMENUCUSTOMPERMID;
                    //        //EntityMenuCustomPermDelete(strId);
                    //    }
                    //}
                    #endregion
                    
                }
                SysPermissionBLL bllPer = new SysPermissionBLL();
                T_SYS_ROLE entRole = roleBll.GetSysRoleByIDToCustomer(RoleID);//获取角色ID实体对象，通过缓存读取

                foreach (var Menus in objs)
                {
                    if (Menus.PermissionValue == null)
                    {
                        continue;
                        //tmpDeleteCustomers.Add(Menus);
                    }

                    if (Menus.PermissionValue.Count() == 0)
                    {
                        continue;
                    }
                    T_SYS_ENTITYMENU entMenu = menuBll.GetSysMenuByIDToCustomer(Menus.EntityMenuId);//获取缓存  
                    foreach (var Perms in Menus.PermissionValue)
                    {
                        if (Perms.OrgObjects == null)
                        {
                            continue;
                        }

                        if (Perms.OrgObjects.Count() == 0)
                        {
                            continue;
                        }

                        T_SYS_PERMISSION entPer = bllPer.GetAttachSysPermissionByIDToCustomer(Perms.Permission);//获取权限实体对象 ，添加了缓存
                        foreach (var OrgIns in Perms.OrgObjects)
                        {
                            T_SYS_ENTITYMENUCUSTOMPERM customerPer = new T_SYS_ENTITYMENUCUSTOMPERM();
                            customerPer.ENTITYMENUCUSTOMPERMID = Guid.NewGuid().ToString();

                            customerPer.T_SYS_ROLEReference.EntityKey = entRole.EntityKey;
                            customerPer.T_SYS_ROLE = entRole;
                            //customerPer.T_SYS_ENTITYMENU = entMenu;
                            customerPer.T_SYS_ENTITYMENUReference.EntityKey = entMenu.EntityKey;
                            customerPer.T_SYS_ENTITYMENU = entMenu;
                            //customerPer.T_SYS_PERMISSION = entPer;
                            customerPer.T_SYS_PERMISSIONReference.EntityKey = entPer.EntityKey;
                            customerPer.T_SYS_PERMISSION = entPer;

                            customerPer.CREATEDATE = DateTime.Now;
                            customerPer.UPDATEDATE = DateTime.Now;
                            
                            var entperms = from ent in cuspers
                                           where ent.T_SYS_ENTITYMENU.ENTITYMENUID == entMenu.ENTITYMENUID 
                                           && ent.T_SYS_ROLE.ROLEID == entRole.ROLEID && ent.T_SYS_PERMISSION.PERMISSIONID == entPer.PERMISSIONID
                                           select ent;
                            switch (OrgIns.OrgType)
                            {

                                //公司
                                case "0":
                                    customerPer.COMPANYID = OrgIns.OrgID;
                                    if (entperms != null)
                                    {
                                        if (entperms.Count() > 0)
                                        {
                                            entperms = entperms.Where(p => p.COMPANYID == OrgIns.OrgID);
                                        }
                                    }
                                    break;
                                case "1"://部门
                                    customerPer.DEPARTMENTID = OrgIns.OrgID;
                                    if (entperms.Count() > 0)
                                    {
                                        entperms = entperms.Where(p => p.DEPARTMENTID == OrgIns.OrgID);
                                    }
                                    break;
                                case "2"://岗位
                                    customerPer.POSTID = OrgIns.OrgID;
                                    if (entperms.Count() > 0)
                                    {
                                        entperms = entperms.Where(p => p.POSTID == OrgIns.OrgID);
                                    }
                                    break;
                            }
                            var entcustomers = from ent in tmpNewCustomers
                                       where ent.T_SYS_PERMISSION.PERMISSIONID == customerPer.T_SYS_PERMISSION.PERMISSIONID
                                       && ent.T_SYS_ENTITYMENU.ENTITYMENUID == customerPer.T_SYS_ENTITYMENU.ENTITYMENUID
                                       && ent.T_SYS_ROLE.ROLEID == customerPer.T_SYS_ROLE.ROLEID
                                       && ent.COMPANYID == customerPer.COMPANYID && ent.DEPARTMENTID == customerPer.DEPARTMENTID
                                       && ent.POSTID == customerPer.POSTID
                                       select ent;
                            if(entcustomers.Count() ==0)
                                tmpNewCustomers.Add(customerPer);
                            if (entperms != null)
                            {
                                if (entperms.Count() == 0)
                                {
                                    var ents = from ent in cuspers
                                               where ent.T_SYS_PERMISSION.PERMISSIONID == customerPer.T_SYS_PERMISSION.PERMISSIONID 
                                               && ent.T_SYS_ENTITYMENU.ENTITYMENUID == customerPer.T_SYS_ENTITYMENU.ENTITYMENUID
                                               && ent.T_SYS_ROLE.ROLEID == customerPer.T_SYS_ROLE.ROLEID
                                               && ent.COMPANYID == customerPer.COMPANYID && ent.DEPARTMENTID == customerPer.DEPARTMENTID
                                               && ent.POSTID == customerPer.POSTID
                                               select ent;

                                    if (ents.Count() == 0)
                                        tmpNewAddCustomers.Add(customerPer);
                                    //EntityMenuCustomPermUpdate(customerPer);
                                }
                                
                                //tmpNewCustomers.Add(customerPer);
                                
                            }
                            //EntityMenuCustomPermAdd(customerPer);
                        }
                    }
                }
                //开始修改数据
                if (cuspers.Count() >0)
                {
                    cuspers.ToList().ForEach(
                        item => {
                            var ents = from ent in tmpNewCustomers
                                       where ent.T_SYS_PERMISSION.PERMISSIONID == item.T_SYS_PERMISSION.PERMISSIONID 
                                       && ent.T_SYS_ENTITYMENU.ENTITYMENUID == item.T_SYS_ENTITYMENU.ENTITYMENUID
                                       && ent.T_SYS_ROLE.ROLEID == item.T_SYS_ROLE.ROLEID 
                                       && ent.COMPANYID == item.COMPANYID && ent.DEPARTMENTID == item.DEPARTMENTID
                                       && ent.POSTID == item.POSTID
                                       select ent;

                            if (ents.Count() == 0)
                                tmpDeleteCustomers.Add(item);
                        }
                        );
                }

                IsResult = this.BatchUpdateCustomerPermision(tmpNewAddCustomers, tmpDeleteCustomers);
                if (IsResult)
                    dal.CommitTransaction();
                else
                    dal.RollbackTransaction();

            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                IsResult= false;
            }
            return IsResult;
        }



        public bool UpdateCustomerPermissionnew(string RoleID, List<CustomerPermission> objs, ref string strResult)
        {
            bool IsResult = true;
            //dal.BeginTransaction();
            try
            {

                SysEntityMenuBLL menuBll = new SysEntityMenuBLL();
                SysPermissionBLL permBll = new SysPermissionBLL();
                SysRoleBLL roleBll = new SysRoleBLL();
                IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> cuspers = GetCustomPermByRoleIDToCustomer(RoleID);
                List<T_SYS_ENTITYMENUCUSTOMPERM> tmpCustomers = new List<T_SYS_ENTITYMENUCUSTOMPERM>();//旧的没存在的数据
                List<T_SYS_ENTITYMENUCUSTOMPERM> tmpNewCustomers = new List<T_SYS_ENTITYMENUCUSTOMPERM>();//新的数据
                List<T_SYS_ENTITYMENUCUSTOMPERM> tmpNewAddCustomers = new List<T_SYS_ENTITYMENUCUSTOMPERM>();//需要添加的数据
                List<T_SYS_ENTITYMENUCUSTOMPERM> tmpDeleteCustomers = new List<T_SYS_ENTITYMENUCUSTOMPERM>();//需要删除的数据
                List<string> menuids = new List<string>();//菜单ID集合
                List<string> permissions = new List<string>();//权限值ID集合
                List<OrgObject> orgs = new List<OrgObject>();//组织架构ID集合
                string companyid = "";
                string departmentid = "";
                string postid = "";
                string menuid = "";
                string Permissionid = "";
                
                SysPermissionBLL bllPer = new SysPermissionBLL();
                T_SYS_ROLE entRole = roleBll.GetSysRoleByIDToCustomer(RoleID);//获取角色ID实体对象，通过缓存读取
                List<string> tmpDeleteMenus = new List<string>();
                List<string> tmpDeletePermissions = new List<string>();//删除的角色ID
                List<string> tmpDeleteOrgs = new List<string>(); //删除的组织架构ID
                foreach (var Menus in objs)
                {
                    menuid = Menus.EntityMenuId;//菜单ID
                    if (Menus.PermissionValue == null)
                    {
                        #region 删除了菜单
                        //如果是直接删除了菜单 则全部删除
                        string MenuId = Menus.EntityMenuId;
                        if (Menus.PermissionValue == null)
                        {
                            if (Menus.EntityMenuId.IndexOf("#") > -1)
                            {
                                tmpDeleteMenus.Add(MenuId.Substring(0, MenuId.Length - 2));//去掉 "#0"
                            }
                            else
                            {
                                continue;
                            }
                            //tmpDeleteCustomers.Add(Menus);
                        }
                        
                        #endregion
                    }
                    else
                    {
                        #region 没有删除菜单
                        if (Menus.PermissionValue.Count() == 0)
                        {
                            continue;
                        }
                        T_SYS_ENTITYMENU entMenu = menuBll.GetSysMenuByIDToCustomer(Menus.EntityMenuId);//获取缓存  

                        foreach (var Perms in Menus.PermissionValue)
                        {

                            Permissionid = Perms.Permission;//权限ID
                            if (Perms.OrgObjects == null)
                            {
                                //continue;
                                #region 删除了权限值
                                string PermID = Perms.Permission;
                                if (PermID.IndexOf("#") > -1)
                                {
                                    if (!(tmpDeleteMenus.IndexOf(PermID) > -1))
                                        tmpDeletePermissions.Add(PermID.Substring(0, PermID.Length - 2));//去掉 "#0"
                                }
                                #endregion

                            }
                            else
                            {
                                #region 没有删除权限值

                                T_SYS_PERMISSION entPer = bllPer.GetAttachSysPermissionByIDToCustomer(Perms.Permission);//获取权限实体对象 ，添加了缓存
                                foreach (var OrgIns in Perms.OrgObjects)
                                {
                                    

                                    if (OrgIns.OrgID.IndexOf('#') > -1)
                                    {
                                        #region 获取被删除的权限范围

                                        string orgid = OrgIns.OrgID.Split('#')[0];//由  #组成的字符串的前面的字符
                                        T_SYS_ENTITYMENUCUSTOMPERM customer = new T_SYS_ENTITYMENUCUSTOMPERM();
                                        //customer = 
                                        customer = GetCustomPermByRoleIDAndEntityIDAndPermissionidAndOrgIDAndOrgTypeToCustomer(RoleID, menuid, Permissionid, orgid, OrgIns.OrgType);
                                        tmpDeleteCustomers.Add(customer);
                                       #endregion
                                    }
                                    
                                    else
                                    {
                                        #region 没有被删除的权限范围，包含添加的
                                        
                                        
                                        T_SYS_ENTITYMENUCUSTOMPERM customerPer = new T_SYS_ENTITYMENUCUSTOMPERM();
                                        customerPer.ENTITYMENUCUSTOMPERMID = Guid.NewGuid().ToString();

                                        customerPer.T_SYS_ROLEReference.EntityKey = entRole.EntityKey;
                                        customerPer.T_SYS_ROLE = entRole;
                                        //customerPer.T_SYS_ENTITYMENU = entMenu;
                                        customerPer.T_SYS_ENTITYMENUReference.EntityKey = entMenu.EntityKey;
                                        customerPer.T_SYS_ENTITYMENU = entMenu;
                                        //customerPer.T_SYS_PERMISSION = entPer;
                                        customerPer.T_SYS_PERMISSIONReference.EntityKey = entPer.EntityKey;
                                        customerPer.T_SYS_PERMISSION = entPer;

                                        customerPer.CREATEDATE = DateTime.Now;
                                        customerPer.UPDATEDATE = DateTime.Now;

                                        var entperms = from ent in cuspers
                                                       where ent.T_SYS_ENTITYMENU.ENTITYMENUID == entMenu.ENTITYMENUID
                                                       && ent.T_SYS_ROLE.ROLEID == entRole.ROLEID 
                                                       && ent.T_SYS_PERMISSION.PERMISSIONID == entPer.PERMISSIONID
                                                       select ent;
                                        switch (OrgIns.OrgType)
                                        {

                                            //公司
                                            case "0":
                                                customerPer.COMPANYID = OrgIns.OrgID;
                                                if (entperms != null)
                                                {
                                                    if (entperms.Count() > 0)
                                                    {
                                                        entperms = entperms.Where(p => p.COMPANYID == OrgIns.OrgID);
                                                    }
                                                }
                                                break;
                                            case "1"://部门
                                                customerPer.DEPARTMENTID = OrgIns.OrgID;
                                                if (entperms.Count() > 0)
                                                {
                                                    entperms = entperms.Where(p => p.DEPARTMENTID == OrgIns.OrgID);
                                                }
                                                break;
                                            case "2"://岗位
                                                customerPer.POSTID = OrgIns.OrgID;
                                                if (entperms.Count() > 0)
                                                {
                                                    entperms = entperms.Where(p => p.POSTID == OrgIns.OrgID);
                                                }
                                                break;
                                        }

                                        if (entperms != null)
                                        {
                                            if (entperms.Count() == 0)
                                            {
                                                var ents = from ent in cuspers
                                                           where ent.T_SYS_PERMISSION.PERMISSIONID == customerPer.T_SYS_PERMISSION.PERMISSIONID
                                                           && ent.T_SYS_ENTITYMENU.ENTITYMENUID == customerPer.T_SYS_ENTITYMENU.ENTITYMENUID
                                                           && ent.T_SYS_ROLE.ROLEID == customerPer.T_SYS_ROLE.ROLEID
                                                           && ent.COMPANYID == customerPer.COMPANYID && ent.DEPARTMENTID == customerPer.DEPARTMENTID
                                                           && ent.POSTID == customerPer.POSTID
                                                           select ent;

                                                if (ents.Count() == 0)
                                                {
                                                    if (tmpNewAddCustomers.Count() == 0)//为空时不查重复记录
                                                    {
                                                        tmpNewAddCustomers.Add(customerPer);
                                                    }
                                                    else
                                                    {
                                                        var entcustomers = from ent in tmpNewAddCustomers
                                                                           where ent.T_SYS_PERMISSION.PERMISSIONID == customerPer.T_SYS_PERMISSION.PERMISSIONID
                                                                           && ent.T_SYS_ENTITYMENU.ENTITYMENUID == customerPer.T_SYS_ENTITYMENU.ENTITYMENUID
                                                                           && ent.T_SYS_ROLE.ROLEID == customerPer.T_SYS_ROLE.ROLEID
                                                                           && ent.COMPANYID == customerPer.COMPANYID && ent.DEPARTMENTID == customerPer.DEPARTMENTID
                                                                           && ent.POSTID == customerPer.POSTID
                                                                           select ent;
                                                        if (entcustomers.Count() == 0)
                                                            tmpNewAddCustomers.Add(customerPer);
                                                    }
                                                }
                                                //EntityMenuCustomPermUpdate(customerPer);
                                            }

                                            //tmpNewCustomers.Add(customerPer);

                                        }
                                        #endregion
                                    }
                                   

                                
                                }
                                #endregion
                            }

                            
                            
                        }
                        #region 获取删除的权限的集合

                        //获取需要删除的权限ID集合
                        IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> Delperms = GetCustomPermByRoleIDAndEntityIDAndPermissionidToCustomer(RoleID, menuid, tmpDeletePermissions);
                        if (Delperms != null)
                        {
                            Delperms.ToList().ForEach(item =>
                            {
                                tmpDeleteCustomers.Add(item);
                            });
                        }
                        
                        #endregion
                        
                        #endregion
                    }
                    

                    
                }
                //开始修改数据
                if (cuspers.Count() > 0)
                {
                    
                    IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> perms = GetCustomPermByRoleIDAndEntityIDToCustomer(RoleID,tmpDeleteMenus);
                    if (perms != null)
                    {
                        perms.ToList().ForEach(item => {
                            tmpDeleteCustomers.Add(item);
                        });
                    }
                       
                }

                IsResult = this.BatchUpdateCustomerPermision(tmpNewAddCustomers, tmpDeleteCustomers);
                //if (IsResult)
                //    dal.CommitTransaction();
                //else
                //    dal.RollbackTransaction();

            }
            catch (Exception ex)
            {
                //dal.RollbackTransaction();
                IsResult = false;
            }
            return IsResult;
        }


        /// <summary>
        /// 批量修改 自定义权限
        /// </summary>
        /// <param name="addList">需要添加的权限</param>
        /// <param name="delList">需要删除的权限</param>
        /// <returns></returns>
        public bool BatchUpdateCustomerPermision(List<T_SYS_ENTITYMENUCUSTOMPERM> addList, List<T_SYS_ENTITYMENUCUSTOMPERM> delList)
        {
            bool IsResult = true;
            try
            {
                dal.BeginTransaction();
                RoleEntityMenuBLL Rolebll = new RoleEntityMenuBLL();
                string MenuCompanyID = "55623178-A187-421a-8556-067E6908207A";//公司菜单ID
                string MenuDepartmentID = "04F86C10-02E3-4874-A198-4EC986C288CC";//部门菜单ID
                T_SYS_ENTITYMENUCUSTOMPERM entPermObj = new T_SYS_ENTITYMENUCUSTOMPERM();
                if (addList.Count() > 0)
                {
                    entPermObj = addList.FirstOrDefault();
                }
                else
                {
                    entPermObj = delList.FirstOrDefault();
                }
                string EntityID = entPermObj.T_SYS_ENTITYMENU.ENTITYMENUID;//菜单ID
                string StrRoleID = entPermObj.T_SYS_ROLE.ROLEID;//角色ID
                if (addList.Count() > 0)
                {
                    foreach (var ent in addList)
                    {
                        //公司
                        if (!string.IsNullOrEmpty(ent.COMPANYID))
                        {
                            var ents = from p in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                                           .Include("T_SYS_PERMISSION").Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                                       where p.COMPANYID == ent.COMPANYID 
                                       && p.T_SYS_ENTITYMENU.ENTITYMENUID == ent.T_SYS_ENTITYMENU.ENTITYMENUID
                                       && p.T_SYS_PERMISSION.PERMISSIONID == ent.T_SYS_PERMISSION.PERMISSIONID 
                                       && p.T_SYS_ROLE.ROLEID == ent.T_SYS_ROLE.ROLEID
                                       select p;

                            if (ents.Count() == 0)
                            {
                                Utility.RefreshEntity(ent);
                                //base.Add(ent);
                                dal.AddToContext(ent);
                                //if (EntityID == MenuCompanyID)
                                //{
                                //    Rolebll.AddCustomerPermissionUpdateUserDepart(StrRoleID, ent.COMPANYID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                                //}
                                //if (EntityID == MenuDepartmentID)
                                //{
                                //    Rolebll.AddCustomerPermissionUpdateUserDepart(StrRoleID, ent.COMPANYID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                                //}
                            }
                        }
                        //部门
                        if (!string.IsNullOrEmpty(ent.DEPARTMENTID))
                        {
                            var ents = from p in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                                           .Include("T_SYS_PERMISSION").Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                                       where p.DEPARTMENTID == ent.DEPARTMENTID 
                                       && p.T_SYS_ENTITYMENU.ENTITYMENUID == ent.T_SYS_ENTITYMENU.ENTITYMENUID
                                       && p.T_SYS_PERMISSION.PERMISSIONID == ent.T_SYS_PERMISSION.PERMISSIONID 
                                       && p.T_SYS_ROLE.ROLEID == ent.T_SYS_ROLE.ROLEID
                                       select p;

                            if (ents.Count() == 0)
                            {
                                Utility.RefreshEntity(ent);
                                //base.Add(ent);
                                dal.AddToContext(ent);
                                //if (EntityID == MenuCompanyID)
                                //{
                                //    Rolebll.AddCustomerPermissionUpdateUserDepart(StrRoleID, ent.DEPARTMENTID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                                //}
                                //if (EntityID == MenuDepartmentID)
                                //{
                                //    Rolebll.AddCustomerPermissionUpdateUserDepart(StrRoleID, ent.DEPARTMENTID, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                                //}
                            }
                        }
                        //岗位
                        if (!string.IsNullOrEmpty(ent.POSTID))
                        {
                            var ents = from p in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                                           .Include("T_SYS_PERMISSION").Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                                       where p.POSTID == ent.POSTID && p.T_SYS_ENTITYMENU.ENTITYMENUID == ent.T_SYS_ENTITYMENU.ENTITYMENUID
                                       && p.T_SYS_PERMISSION.PERMISSIONID == ent.T_SYS_PERMISSION.PERMISSIONID 
                                       && p.T_SYS_ROLE.ROLEID == ent.T_SYS_ROLE.ROLEID
                                       select p;

                            if (ents.Count() == 0)
                            {
                                Utility.RefreshEntity(ent);
                                //base.Add(ent);
                                dal.AddToContext(ent);
                            }
                        }
                    }
                }
                if (delList.Count() > 0)
                {
                    delList.ForEach(item => {
                        dal.DeleteFromContext(item);
                    });
                }

                int i = dal.SaveContextChanges();
                if (i > 0)
                    dal.CommitTransaction();
                else
                    dal.RollbackTransaction();


            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                IsResult = false;
            }

            return IsResult;
        }

    }
}
