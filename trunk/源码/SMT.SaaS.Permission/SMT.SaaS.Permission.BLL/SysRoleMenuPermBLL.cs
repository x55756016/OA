using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Log;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.BLL
{
    public class SysRoleMenuPermBLL : BaseBll<T_SYS_ROLEMENUPERMISSION>
    {
        /// <summary>
        /// 根据系统类型获取角色权限信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统角色权限</param>
        /// <returns>角色权限信息列表</returns>
        public IQueryable<T_SYS_ROLEMENUPERMISSION> GetSysRoleMenuPermByType(string sysType)
        {
            try
            {
                var ents = from a in base.GetObjects().Include("T_SYS_ROLE").Include("T_SYS_PERMISSION")
                           where string.IsNullOrEmpty(sysType)
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetSysRoleMenuPermByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 根据角色权限ID获取角色权限信息
        /// </summary>
        /// <param name="sysType">角色权限ID</param>
        /// <returns>角色权限信息</returns>
        public T_SYS_ROLEMENUPERMISSION GetSysRoleMenuPermByID(string RoleMenuPermID)
        {
            try
            {
                var ents = from ent in dal.GetTable()
                           where ent.ROLEMENUPERMID == RoleMenuPermID
                           select ent;
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetSysRoleMenuPermByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        public bool GetSysRoleMenuPermByPermIDAndRoleEntityIDAndRang(string StrpermId, string StrRoleEntityID, string StrDataRang)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
                           where ent.DATARANGE == StrDataRang && ent.T_SYS_PERMISSION.PERMISSIONID == StrpermId && ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == StrRoleEntityID
                           select ent;
                if (ents.Count() > 0)
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
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetSysRoleMenuPermByPermIDAndRoleEntityIDAndRang" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
            //return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }


        public T_SYS_ROLEMENUPERMISSION GetSysRoleMenuPermByPermIDAndRoleEntityID(string StrpermId, string StrRoleEntityID)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
                           where ent.T_SYS_PERMISSION.PERMISSIONID == StrpermId && ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == StrRoleEntityID
                           select ent;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault(); ;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetSysRoleMenuPermByPermIDAndRoleEntityID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
            //return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        public IQueryable<T_SYS_ROLEMENUPERMISSION> GetSysRoleMenuPermissionByList()
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
                           //where ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == StrRoleEntityID
                           select ent;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetSysRoleMenuPermissionByList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public List<T_SYS_ROLEMENUPERMISSION> GetSysRoleMenuPermByRoleEntityID(string[] StrRoleEntityID)
        {
            try
            {
                IQueryable<T_SYS_ROLEMENUPERMISSION> BB = this.GetSysRoleMenuPermissionByList();
                List<T_SYS_ROLEMENUPERMISSION> CC = BB.ToList();
                var q = from a in CC
                        where StrRoleEntityID.Contains(a.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID)
                        select a;
                return q.Count() > 0 ? q.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetSysRoleMenuPermByRoleEntityID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
            //return q.ToList();


        }
        /// <summary>
        /// 添加角色权限
        /// </summary>
        /// <param name="sourceEntity">添加角色权限的实体对象</param>
        public bool SysRoleMenuPermAdd(T_SYS_ROLEMENUPERMISSION sourceEntity,string StrPermID,string StrRoleEntityID)
        {
            try
            {
                T_SYS_ROLEMENUPERMISSION ent = new T_SYS_ROLEMENUPERMISSION();
                ent.ROLEMENUPERMID = sourceEntity.ROLEMENUPERMID;
                ent.CREATEDATE = sourceEntity.CREATEDATE;
                ent.CREATEUSER = sourceEntity.CREATEUSER;
                ent.UPDATEDATE = sourceEntity.UPDATEDATE;
                ent.UPDATEUSER = sourceEntity.UPDATEUSER;
                ent.DATARANGE = sourceEntity.DATARANGE;
                ent.EXTENDVALUE = "";
                ent.T_SYS_PERMISSIONReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", StrPermID);
                ent.T_SYS_ROLEENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLEENTITYMENU", "ROLEENTITYMENUID", StrRoleEntityID);
                //ent.T_SYS_PERMISSION.PERMISSIONID = sourceEntity.T_SYS_PERMISSION.PERMISSIONID;
                //ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID = sourceEntity.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID;
                //ent.T_SYS_ROLEReference.EntityKey =
                //    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", sourceEntity.T_SYS_ROLE.ROLEID);
                //ent.T_SYS_PERMISSIONReference.EntityKey =
                //    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", sourceEntity.T_SYS_PERMISSION.PERMISSIONID);

                //int i = 1; //dal.Add(ent);
                //dal.AddToContext(ent);
                
                //dal.AddObject(ent.GetType().Name,ent);
                bool i = Add(ent);
                if (i )
                {
                    //StrReturn = "";
                    //T_SYS_ROLEMENUPERMISSION PermRole = new T_SYS_ROLEMENUPERMISSION();
                    //PermRole.ROLEMENUPERMID = System.Guid.NewGuid().ToString();
                    //PermRole.T_SYS_PERMISSION.PERMISSIONID = 
                    return true;

                }
                else
                {
                    //StrReturn = "false";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-SysRoleMenuPermAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //throw (ex);
                return false;
            }
        }
        /// <summary>
        /// 修改角色权限
        /// </summary>
        /// <param name="entity">被修改的角色权限的实体</param>
        public bool SysRoleMenuPermUpdate(T_SYS_ROLEMENUPERMISSION sourceEntity)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
                           where ent.ROLEMENUPERMID == sourceEntity.ROLEMENUPERMID
                           select ent;

                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    //Utility.CloneEntity(sourceEntity,ent);
                    Utility.CloneEntity<T_SYS_ROLEMENUPERMISSION>(sourceEntity, ent);

                    //ent.T_SYS_ROLEReference.EntityKey =
                    //    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", sourceEntity.T_SYS_ROLE.ROLEID);

                    //ent.T_SYS_ENTITYMENUReference.EntityKey =
                    //    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", sourceEntity.T_SYS_ENTITYMENU.ENTITYMENUID);


                    //ent.T_SYS_PERMISSIONReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", sourceEntity.T_SYS_PERMISSION.PERMISSIONID);
                    //ent.T_SYS_ROLEENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLEENTITYMENU", "ROLEENTITYMENUID", sourceEntity.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID);
                    //ent.T_SYS_ROLEReference.EntityKey =
                    //    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", sourceEntity.T_SYS_ROLE.ROLEID);
                    //ent.T_SYS_PERMISSIONReference.EntityKey =
                    //    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", sourceEntity.T_SYS_PERMISSION.PERMISSIONID);
                    bool i = Update(ent);
                    //int i = 1;
                    //dal.UpdateFromContext(ent);
                    if (i )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-SysRoleMenuPermUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                //throw (ex);
                
            }
        }
        /// <summary>
        /// 删除角色权限
        /// </summary>
        /// <param name="menuID">角色权限ID</param>
        /// <returns>是否删除成功</returns>
        public bool SysRoleMenuPermDelete(string id)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.ROLEMENUPERMID == id
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    //dal.DeleteFromContext(entity);
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-SysRoleMenuPermDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                //throw (ex);
            }
        }

        public IQueryable<T_SYS_ROLEMENUPERMISSION> GetRoleEntityPermissionListByRoleEntityID(string StrRoleEntityID)
        {
            try
            {
                var ents = from a in GetObjects().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
                           where a.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == StrRoleEntityID
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetRoleEntityPermissionListByRoleEntityID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        
        /// <summary>
        /// 根据roleEntityID列表返回 信息
        /// </summary>
        /// <param name="StrRoleEntityID"></param>
        /// <returns></returns>
        public List<T_SYS_ROLEMENUPERMISSION> GetPermissions(string[] StrRoleEntityID)
        {
            try
            {
                IQueryable<T_SYS_ROLEMENUPERMISSION> iqperm = this.GetRoleMenuPerm();
                List<T_SYS_ROLEMENUPERMISSION> listperm = iqperm.ToList();
                
                var q = from a in listperm
                        where StrRoleEntityID.Contains(a.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID)
                        select a;
                return q.ToList();
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetPermissions" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw(ex);
            }

        }
        public IQueryable<T_SYS_ROLEMENUPERMISSION> GetRoleMenuPerm()
        {
            try
            {
                var ents = from a in GetObjects().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
                           //where StrRoleEntityID.Contains(a.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID)
                           select a;

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色权限SysRoleMenuPermBLL-GetRoleMenuPerm" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="OwnerCompanyId"></param>
        /// <returns></returns>
        public string GetPublishSenddocUser(string OwnerCompanyId)
        {
            string StrUser = "";
            var ents = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLE").Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")
                       where ent.T_SYS_ROLEENTITYMENU.T_SYS_ROLE.OWNERCOMPANYID == OwnerCompanyId
                       select ent;
            return StrUser;
        }

        


        
    }
}
