using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using SMT.Foundation.Log;
//添加即时通讯接口引用
using SMT.HRM.IMServices.IMServiceWS;


namespace SMT.SaaS.Permission.BLL
{
    public class RoleEntityMenuBLL: BaseBll<T_SYS_ROLEENTITYMENU>
    {
        #region "增删改查"
        /// <summary>
        /// 根据系统类型获取角色菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统角色菜单</param>
        /// <returns>角色菜单信息列表</returns>
        public IQueryable<T_SYS_ROLEENTITYMENU> GetRoleEntityMenuByType(string sysType)
        {

            try
            {
                var ents = from a in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE")
                           where string.IsNullOrEmpty(sysType) || a.T_SYS_ENTITYMENU.SYSTEMTYPE == sysType
                           select a;

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-GetRoleEntityMenuByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }
        /// <summary>
        /// 根据角色菜单ID获取角色菜单信息
        /// </summary>
        /// <param name="sysType">角色菜单ID</param>
        /// <returns>角色菜单信息</returns>
        public T_SYS_ROLEENTITYMENU GetRoleEntityMenuByID(string roleEntityMenuID)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE")
                           where ent.ROLEENTITYMENUID == roleEntityMenuID
                           select ent;
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-GetRoleEntityMenuByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据菜单ID和角色ID获取一条记录
        /// </summary>
        /// <param name="EntityMenuID">菜单ID</param>
        /// <param name="RoleID">角色ID</param>
        /// <returns></returns>
        public T_SYS_ROLEENTITYMENU GetRoleEntityMenuByIDAndRoleID(string EntityMenuID,string RoleID)
        {
            try
            {
                var ents = from ent in GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE")
                           where ent.T_SYS_ENTITYMENU.ENTITYMENUID == EntityMenuID && ent.T_SYS_ROLE.ROLEID == RoleID
                           select ent;
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-GetRoleEntityMenuByIDAndRoleID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 修改系统角色菜单
        /// </summary>
        /// <param name="entity">被修改的角色菜单的实体</param>
        public void RoleEntityMenuUpdate(T_SYS_ROLEENTITYMENU sourceEntity)
        {
            try
            {
                var ents = from ent in dal.GetTable()
                           where ent.ROLEENTITYMENUID == sourceEntity.ROLEENTITYMENUID
                           select ent;

                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();

                    Utility.CloneEntity<T_SYS_ROLEENTITYMENU>(sourceEntity, ent);

                    ent.T_SYS_ROLEReference.EntityKey =
                        new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", sourceEntity.T_SYS_ROLE.ROLEID);
                    
                    ent.T_SYS_ENTITYMENUReference.EntityKey =
                        new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", sourceEntity.T_SYS_ENTITYMENU.ENTITYMENUID);

                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-RoleEntityMenuUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        /// <summary>
        /// 添加系统角色菜单
        /// </summary>
        /// <param name="sourceEntity">被添加的角色菜单的实体</param>
        public void RoleEntityMenuAdd(T_SYS_ROLEENTITYMENU sourceEntity)
        {
            try
            {
                T_SYS_ROLEENTITYMENU ent = new T_SYS_ROLEENTITYMENU();
                ent.ROLEENTITYMENUID = sourceEntity.ROLEENTITYMENUID;

                Utility.CloneEntity<T_SYS_ROLEENTITYMENU>(sourceEntity, ent);
                //ent.T_SYS_ROLE.ROLEID 
                ent.T_SYS_ROLEReference.EntityKey =
                    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", sourceEntity.T_SYS_ROLE.ROLEID);
                ent.T_SYS_ENTITYMENUReference.EntityKey =
                    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", sourceEntity.T_SYS_ENTITYMENU.ENTITYMENUID);

                dal.Add(ent);
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-RoleEntityMenuAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        SysRoleEntityMenuDAL rolemenuDal = new SysRoleEntityMenuDAL();
        public void RoleEntityBeginTransaction()
        {
            rolemenuDal.BeginTransaction();
        }

        public void RoleEntityCommitTransaction()
        {
            rolemenuDal.CommitTransaction();
        }

        public void RoleEntityRollbackTransaction()
        {
            rolemenuDal.RollbackTransaction();
        }

        /// <summary>
        /// 批量添加系统角色菜单
        /// eidt by ljx
        /// </summary>
        /// <param name="sourceEntity">被添加的角色菜单的实体</param>
        public bool RoleEntityMenuBatchAdd(T_SYS_ENTITYMENU[] sourceEntity, string[] StrRangeList, string StrRoleID, T_SYS_DICTIONARY[] PermList)
        {
            try
            {
                string StrReturn = "";
                int k = 0;
                if (sourceEntity.Count() > 0)
                {
                    this.RoleEntityBeginTransaction();
                    //string StrFormID = "";

                    foreach (var obj in sourceEntity)
                    {
                        k++;
                        T_SYS_ROLEENTITYMENU rolemenu = new T_SYS_ROLEENTITYMENU();
                        rolemenu.ROLEENTITYMENUID = System.Guid.NewGuid().ToString();
                        rolemenu.T_SYS_ENTITYMENU.ENTITYMENUID = obj.ENTITYMENUID;
                        rolemenu.T_SYS_ROLE.ROLEID = StrRoleID;
                        rolemenu.UPDATEDATE = null;
                        rolemenu.UPDATEUSER = "";
                        rolemenu.CREATEDATE = System.DateTime.Now;
                        rolemenu.CREATEUSER = "admin";
                        //rolemenu.DATARANGE  =StrRangeList[k].ToString();
                        rolemenu.REMARK = "";

                        int i = dal.Add(rolemenu);
                        if (i == 1)
                        {
                            //StrReturn = "";
                            T_SYS_ROLEMENUPERMISSION PermRole = new T_SYS_ROLEMENUPERMISSION();
                            PermRole.ROLEMENUPERMID = System.Guid.NewGuid().ToString();
                            //PermRole.T_SYS_PERMISSION.PERMISSIONID = 

                        }
                        else
                        {
                            StrReturn = "false";
                        }
                    }

                    if (StrReturn != "")
                    {
                        this.RoleEntityRollbackTransaction();
                    }
                    else
                    {
                        this.RoleEntityCommitTransaction();
                        return true;
                    }


                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-RoleEntityMenuBatchAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }

        }

        private T_SYS_ROLEENTITYMENU AddRoleEntityInfo(string EntityID, string StrRoleID)
        {
            try
            {
                T_SYS_ROLEENTITYMENU Childrolemenu = new T_SYS_ROLEENTITYMENU();
                Childrolemenu.ROLEENTITYMENUID = System.Guid.NewGuid().ToString();

                Childrolemenu.T_SYS_ENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", EntityID);

                Childrolemenu.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", StrRoleID);
                Childrolemenu.UPDATEDATE = null;
                Childrolemenu.UPDATEUSER = "";
                Childrolemenu.CREATEDATE = System.DateTime.Now;
                Childrolemenu.CREATEUSER = "admin";
                Childrolemenu.REMARK = "";
                int AddResult = dal.Add(Childrolemenu);
                return AddResult > 0 ? Childrolemenu : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-AddRoleEntityInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        private T_SYS_ROLEENTITYMENU UpdateRoleEntityInfoByRoleEntityID(T_SYS_ROLEENTITYMENU RoleEntityObj)
        {
            try
            {
                T_SYS_ROLEENTITYMENU Childrolemenu = new T_SYS_ROLEENTITYMENU();


                Childrolemenu.T_SYS_ENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", RoleEntityObj.T_SYS_ENTITYMENU.ENTITYMENUID);
                Childrolemenu.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", RoleEntityObj.T_SYS_ROLE.ROLEID);
                Childrolemenu.UPDATEDATE = null;
                Childrolemenu.UPDATEUSER = "";
                Childrolemenu.CREATEDATE = System.DateTime.Now;
                Childrolemenu.CREATEUSER = "admin";
                Childrolemenu.REMARK = "";
                int AddResult = dal.Update(Childrolemenu);
                return AddResult > 0 ? Childrolemenu : null;
            }
            catch(Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-UpdateRoleEntityInfoByRoleEntityID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 删除系统角色菜单
        /// </summary>
        /// <param name="menuID">角色菜单ID</param>
        /// <returns>是否删除成功</returns>
        public bool RoleEntityMenuDelete(string id)
        {
            try
            {
                var entitys = (from ent in GetObjects()
                               where ent.ROLEENTITYMENUID == id
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.DeleteFromContext(entity);
                    //dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-RoleEntityMenuDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        public IQueryable<T_SYS_ROLEENTITYMENU> GetRoleEntityIDListInfos(string RoleID)
        {
            try
            {
                var q = from a in this.GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE")
                        where a.T_SYS_ROLE.ROLEID == RoleID
                        select a;
                return q;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-GetRoleEntityIDListInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 根据角色ID获取角色所拥有的权限2011-5-20
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public IQueryable<V_RoleEntity> GetRoleEntityIDListInfosNew(string RoleID)
        {
            try
            {
                var q = from a in this.GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE")
                        where a.T_SYS_ROLE.ROLEID == RoleID
                        select new V_RoleEntity { 
                            ENTITYMENUID = a.T_SYS_ENTITYMENU.ENTITYMENUID,
                            ROLEENTITYMENUID = a.ROLEENTITYMENUID,
                            ROLEID = a.T_SYS_ROLE.ROLEID
                        };

                return q;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-GetRoleEntityIDListInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 根据角色ID获取角色所拥有的权限2011-5-20
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public IQueryable<V_RoleEntity> GetRoleEntityIDListInfosNewToUserRoleApp(string RoleID,ref List<T_SYS_ENTITYMENU> listmenu)
        {
            try
            {
                var q = from a in this.GetObjects().Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE")
                        where a.T_SYS_ROLE.ROLEID == RoleID
                        select new V_RoleEntity
                        {
                            ENTITYMENUID = a.T_SYS_ENTITYMENU.ENTITYMENUID,
                            ROLEENTITYMENUID = a.ROLEENTITYMENUID,
                            ROLEID = a.T_SYS_ROLE.ROLEID
                        };
                if (q != null)
                {
                    if (q.Count() > 0)
                    {
                        List<string> menuids = new List<string>();
                        q.ToList().ForEach(item => {
                            if (!(menuids.IndexOf(item.ENTITYMENUID) > -1))
                                menuids.Add(item.ENTITYMENUID);
                        });

                        var ents = from ent in dal.GetObjects<T_SYS_ENTITYMENU>()
                                   where menuids.Contains(ent.ENTITYMENUID)
                                   select ent;
                        if (ents != null)
                        {
                            if (ents.Count() > 0)
                            {
                                ents.ToList().ForEach(item =>
                                {
                                    item.ENTITYCODE = "";
                                    item.CHILDSYSTEMNAME = "";
                                    item.CREATEDATE = null;
                                    item.CREATEUSER = "";
                                    item.HASSYSTEMMENU = "";
                                    item.ISAUTHORITY = "";
                                    item.MENUCODE = "";
                                    item.MENUICONPATH = "";
                                    item.ORDERNUMBER = 0;
                                    item.REMARK = "";
                                    item.SYSTEMTYPE = "";
                                    item.T_SYS_ENTITYMENU2 = null;
                                    item.T_SYS_ENTITYMENU2Reference = null;
                                    item.UPDATEDATE = null;
                                    item.UPDATEUSER = "";
                                    item.URLADDRESS = "";
                                    
                                });
                                listmenu = ents.ToList();
                            }
                        }
                    }
                }
                

                return q;
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-GetRoleEntityIDListInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 通过角色菜单ID 查看角色菜单权限里是否有记录
        /// 存在记录返回 true 否则返回false  2010-9-26
        /// </summary>
        /// <param name="RoleEntityID">角色菜单ID</param>
        /// <returns></returns>
        private bool GetRoleEntityPermissionByRoleEntityID(string RoleEntityID)
        {
            try
            {
                var q = from a in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")
                        where a.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == RoleEntityID
                        select a;
                return q.Count() > 0 ? true : false;
            }
            catch(Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-GetRoleEntityPermissionByRoleEntityID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        #endregion

        #region"角色授权"
        /// <summary>
        /// 授权主函数
        /// </summary>
        /// <param name="tmpString">tmpString由 字符串组成</param>
        /// <returns></returns>
        public bool RoleEntityMenuBatchAddInfosList(string tmpString, string StrRoleID, string StrAddUser)
        {
            try
            {
                string StrReturn = "";
                SysRoleBLL RoleBll = new SysRoleBLL();
                //SysEntityPermBLL  EntityBll = new SysRoleEntityPermBLL();
                SysEntityMenuBLL EntityBll = new SysEntityMenuBLL();
                SysPermissionBLL PermissionBll = new SysPermissionBLL();
                T_SYS_ROLE RoleT = new T_SYS_ROLE();
                RoleT = RoleBll.GetSysRoleByID(StrRoleID);
                IQueryable<T_SYS_ROLEENTITYMENU> QueryRoleEntity = GetRoleEntityIDListInfos(StrRoleID);
                List<T_SYS_ROLEENTITYMENU> listRoleEntity = QueryRoleEntity.Count() > 0 ? QueryRoleEntity.ToList() : null;
                // 如果T_Sys_RoleEntity表中存在某一角色的记录 则为修改
                if (listRoleEntity == null)
                {
                    UpdateRoleInfo(StrRoleID, StrAddUser);
                    return RoleEntityMenuPermissionAdd(tmpString, StrRoleID, StrAddUser);

                }// end 添加
                else  //修改状态
                {
                    //return RoleEntityMenuPermissionUpdate(tmpString, StrRoleID, StrAddUser, listRoleEntity);
                    UpdateRoleInfo(StrRoleID, StrAddUser);
                    return RoleEntityMenuPermissionUpdateNew(tmpString, StrRoleID, StrAddUser, listRoleEntity);
                }// end edit
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-RoleEntityMenuBatchAddInfosList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 更新角色信息的更新时间和更新人
        /// </summary>
        /// <param name="roleID">角色ID</param>
        /// <param name="userID">用户ID</param>
        public void UpdateRoleInfo(string roleID, string userID)
        {
            try
            {
                var ents = (from ent in dal.GetObjects<T_SYS_ROLE>()
                           where ent.ROLEID == roleID
                           select ent).FirstOrDefault();
                if (ents!=null)
                {
                    var user = (from ent in dal.GetObjects<T_SYS_USER>()
                                where ent.SYSUSERID == userID
                                select ent).FirstOrDefault();
                    ents.UPDATEDATE = DateTime.Now;
                    if (user != null)
                    {
                        ents.UPDATEUSER = user.EMPLOYEEID;
                        ents.UPDATEUSERNAME = user.EMPLOYEENAME;
                    }
                    dal.Update(ents);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("保存授权更新角色信息是报错UpdateRoleInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }
        }
        /// <summary>
        /// 用户角色申请 用户建立个角色  对相应的角色进行设置
        /// </summary>
        /// <param name="RoleInfo"></param>
        /// <param name="tmpString"></param>
        /// <param name="StrAddUser"></param>
        /// <returns></returns>
        public bool UserRoleApplyEntityMenuBatchAddInfosList(T_SYS_ROLE RoleInfo,string tmpString,string StrAddUser)
        {
            bool IsReturn = true;
            try
            {
                dal.BeginTransaction();
                SysRoleBLL bll = new SysRoleBLL();
                //权限申请  中每个用户有且只有一个角色名称
                var ents = from ent in dal.GetObjects<T_SYS_ROLE>()
                           where  ent.ISAUTHORY =="1" && ent.OWNERID == StrAddUser
                           select ent;
                if (ents.Count() > 0)
                {
                    //IsReturn = bll.SysRoleUpdateByCheckForRoleApp(RoleInfo);
                    RoleInfo = ents.FirstOrDefault();
                }
                else
                {
                    IsReturn = bll.AddSysRoleInfoForRoleApp(RoleInfo);
                }
                if (IsReturn)
                {
                    string StrReturn = "";
                    SysRoleBLL RoleBll = new SysRoleBLL();
                    //SysEntityPermBLL  EntityBll = new SysRoleEntityPermBLL();
                    SysEntityMenuBLL EntityBll = new SysEntityMenuBLL();
                    SysPermissionBLL PermissionBll = new SysPermissionBLL();
                    //T_SYS_ROLE RoleT = new T_SYS_ROLE();
                    //RoleT = RoleBll.GetSysRoleByID(RoleInfo.ROLEID);
                    IQueryable<T_SYS_ROLEENTITYMENU> QueryRoleEntity = GetRoleEntityIDListInfos(RoleInfo.ROLEID);
                    List<T_SYS_ROLEENTITYMENU> listRoleEntity = QueryRoleEntity.Count() > 0 ? QueryRoleEntity.ToList() : null;
                    // 如果T_Sys_RoleEntity表中存在某一角色的记录 则为修改
                    if (listRoleEntity == null)
                    {
                        IsReturn = RoleEntityMenuPermissionAdd(tmpString, RoleInfo.ROLEID, StrAddUser);

                    }// end 添加
                    else  //修改状态
                    {                        
                        IsReturn = RoleEntityMenuPermissionUpdateNew(tmpString, RoleInfo.ROLEID, StrAddUser, listRoleEntity);
                    }// end edit
                }
                else
                {
                    IsReturn = false;
                }
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-RoleEntityMenuBatchAddInfosList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsReturn= false;
            }
            if (IsReturn)
            {
                dal.CommitTransaction();
                BLLCommonServices.Utility.SubmitMyRecord<T_SYS_ROLE>(RoleInfo);
            }
            else
            {
                dal.RollbackTransaction();
            }
            return IsReturn;
        }


        #region 添加角色权限菜单


        private bool RoleEntityMenuPermissionAdd(string tmpString, string StrRoleID, string StrAddUser)
        {
            try
            {
                SysRoleMenuPermBLL permBll = new SysRoleMenuPermBLL();
                //SysEntityMenuBLL EntityBll = new SysEntityMenuBLL();
                SysPermissionBLL PermissionBll = new SysPermissionBLL();
                #region "添加角色实体菜单T_SYS_RoleEntityMenu"

                string StrReturn = "";
                //SysEntityPermBLL  EntityBll = new SysRoleEntityPermBLL();



                //try
                //{
                string[] firstStr = tmpString.Split('#');
                //T_SYS_ENTITYMENU[] sourceEntity;
                //this.RoleEntityBeginTransaction();
                for (int i = 0; i < firstStr.Length; i++)
                {
                    if (string.IsNullOrEmpty(firstStr[i]))
                        continue;
                    string[] EntityPermStr = firstStr[i].Split('@');//获取菜单ID,将菜单实体和权限菜单分开
                    string[] EntityStr = EntityPermStr[1].Split(',');
                    string EntityID = EntityStr[0].ToString(); //ENTITYID
                    //string EntityParentID = EntityStr[1].ToString(); //父ID

                    //T_SYS_ROLEENTITYMENU ParentMenu = GetRoleEntityMenuByIDAndRoleID(EntityParentID, StrRoleID);
                    T_SYS_ROLEENTITYMENU rolemenu = new T_SYS_ROLEENTITYMENU();
                    rolemenu.ROLEENTITYMENUID = System.Guid.NewGuid().ToString();
                    rolemenu.T_SYS_ENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", EntityID);
                    //rolemenu.T_SYS_ENTITYMENU.ENTITYMENUID = EntityParentID;
                    rolemenu.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", StrRoleID);
                    rolemenu.T_SYS_ROLEMENUPERMISSION = null;//将其设置为NULL
                    rolemenu.UPDATEDATE = null;
                    rolemenu.UPDATEUSER = "";
                    rolemenu.CREATEDATE = System.DateTime.Now;
                    rolemenu.CREATEUSER = "admin";
                    //rolemenu.DATARANGE  =StrRangeList[k].ToString();
                    rolemenu.REMARK = "";

                    dal.Add(rolemenu);

                    //Add RoleEntityMenuPermission
                    AddROLEMENUPERMISSION(permBll, PermissionBll, StrAddUser, StrReturn, EntityPermStr, rolemenu);


                }

                
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-RoleEntityMenuPermissionAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
                        
            return true;

            #endregion
        }
        #region "添加角色菜单权限T_SYS_RoleMenuPermission"
        private void  AddROLEMENUPERMISSION(SysRoleMenuPermBLL permBll, SysPermissionBLL PermissionBll,string StrAddUser, string StrReturn, string[] EntityPermStr, T_SYS_ROLEENTITYMENU rolemenu)
        {
            try
            {
                //StrReturn = "";
                //格式 datarange,permid;
                string[] PermissionEntitis = EntityPermStr[0].Split(';');
                //SysRoleMenuPermBLL permBll = new SysRoleMenuPermBLL();
                for (int p = 0; p < PermissionEntitis.Length - 1; p++)
                {
                    string StrDataRange = PermissionEntitis[p].Split(',')[0];//数据范围
                    string StrPermissionID = PermissionEntitis[p].Split(',')[1];//权限ID
                    if (!string.IsNullOrEmpty(StrDataRange))
                    {
                        T_SYS_ROLEMENUPERMISSION PermRole = new T_SYS_ROLEMENUPERMISSION();
                        PermRole.ROLEMENUPERMID = System.Guid.NewGuid().ToString();
                        PermRole.DATARANGE = StrDataRange;
                        PermRole.T_SYS_PERMISSIONReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", StrPermissionID);

                        PermRole.T_SYS_ROLEENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLEENTITYMENU", "ROLEENTITYMENUID", rolemenu.ROLEENTITYMENUID);

                        PermRole.CREATEDATE = System.DateTime.Now;
                        PermRole.CREATEUSER = StrAddUser;
                        PermRole.UPDATEDATE = null;
                        PermRole.UPDATEUSER = "";
                        PermRole.EXTENDVALUE = "";

                        int k =dal.Add(PermRole);

                        #region 调用即时通讯接口
                        
                        if (k > 0)
                        {
                            //存在公司信息或部门信息菜单授权则调用
                            //string MenuCompanyID = "55623178-A187-421a-8556-067E6908207A";//公司菜单ID
                            //string MenuDepartmentID = "04F86C10-02E3-4874-A198-4EC986C288CC";//部门菜单ID
                            //if (rolemenu.T_SYS_ENTITYMENU.ENTITYMENUID == MenuCompanyID) 
                            //{
                            //    AddUpdateUserDepart(rolemenu.T_SYS_ROLE.ROLEID, StrDataRange,SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                            //}
                            //if (rolemenu.T_SYS_ENTITYMENU.ENTITYMENUID == MenuDepartmentID)
                            //{
                            //    AddUpdateUserDepart(rolemenu.T_SYS_ROLE.ROLEID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment);
                            //}
                        }
                        #endregion


                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-AddROLEMENUPERMISSION" + System.DateTime.Now.ToString() + " " + ex.ToString());
                
            }
          
        }
        #endregion
        #endregion

        #region 角色没赋任何权限删除权限
        /// <summary>
        /// 删除某1角色所对应的菜单和权限信息
        /// </summary>
        /// <param name="listRoleEntity"></param>
        /// <returns></returns>
        private bool BatchDelRoleEntity(List<T_SYS_ROLEENTITYMENU> listRoleEntity)
        {
            
            bool IsResult = false;
            try
            {
                SysRoleMenuPermBLL permBll = new SysRoleMenuPermBLL();


                string EntityIDs = "";
                foreach (T_SYS_ROLEENTITYMENU entity in listRoleEntity)
                {
                    EntityIDs += entity.ROLEENTITYMENUID + ",";
                }
                EntityIDs = EntityIDs.Substring(0, EntityIDs.Length - 1);
                string[] EndEntityIDs = EntityIDs.Split(',');
                List<T_SYS_ROLEMENUPERMISSION> listrolemenuperm = permBll.GetSysRoleMenuPermByRoleEntityID(EndEntityIDs);
                if (listrolemenuperm != null)
                {
                    //不为空则先删除 T_SYS_ROLEENTITYPERMISSON的数据 再删除其它T_SYS_ROLEENTITY的数据
                    foreach (T_SYS_ROLEMENUPERMISSION menuperm in listrolemenuperm)
                    {
                        bool delresult = permBll.SysRoleMenuPermDelete(menuperm.ROLEMENUPERMID);
                        if (!delresult)
                        {
                            break;
                        }
                    }

                    //删除T_SYS_ROLEENTITY的数据
                    for (int n = 0; n < EndEntityIDs.Length; n++)
                    {
                        bool RoleEntity = this.RoleEntityMenuDelete(EndEntityIDs[n]);
                        if (!RoleEntity)
                        {
                            break;
                        }
                    }

                    int IntResult = dal.SaveContextChanges();
                    if (IntResult > 0)
                    {
                        IsResult = true;
                    }



                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-BatchDelRoleEntity" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsResult = false;
            }

            return IsResult;
        }
        #endregion


        #region 根据菜单ID组删除权限
        private bool BatchDelRoleEntityMenu(string RoleID, string[] Entitys)
        {
            bool IsResult = true;
            try
            {
                SysRoleMenuPermBLL permBll = new SysRoleMenuPermBLL();


                string EntityIDs = "";

                List<T_SYS_ROLEMENUPERMISSION> listrolemenuperm = permBll.GetSysRoleMenuPermByRoleEntityID(Entitys);
                if (listrolemenuperm != null)
                {
                    //不为空则先删除 T_SYS_ROLEENTITYPERMISSON的数据 再删除其它T_SYS_ROLEENTITY的数据
                    foreach (T_SYS_ROLEMENUPERMISSION menuperm in listrolemenuperm)
                    {
                        bool delresult = permBll.SysRoleMenuPermDelete(menuperm.ROLEMENUPERMID);
                        if (!delresult)
                        {
                            break;
                        }
                    }

                    //删除T_SYS_ROLEENTITY的数据
                    for (int n = 0; n < Entitys.Length; n++)
                    {
                        bool RoleEntity = this.RoleEntityMenuDelete(Entitys[n]);
                        if (!RoleEntity)
                        {
                            break;
                        }
                    }

                    int IntResult = dal.SaveContextChanges();
                    if (IntResult > 0)
                    {
                        IsResult = true;
                    }
                    else
                    {
                        IsResult = true;
                    }



                }
            }
            catch(Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-BatchDelRoleEntityMenu" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsResult = false;
            }

            return IsResult;
        }
        #endregion

        #region 修改菜单权限，只返回有变动的数据
        /// <summary>
        /// edit 2010-6-17 liujx
        /// </summary>
        /// <param name="tmpString"></param>
        /// <param name="StrRoleID"></param>
        /// <param name="StrAddUser"></param>
        /// <param name="listRoleEntity"></param>
        /// <returns></returns>
        private bool RoleEntityMenuPermissionUpdateNew(string tmpString, string StrRoleID, string StrAddUser, List<T_SYS_ROLEENTITYMENU> listRoleEntity)
        {
            
            string StrReturn = "";
            SysRoleBLL RoleBll = new SysRoleBLL();
            //SysEntityPermBLL  EntityBll = new SysRoleEntityPermBLL();
            SysEntityMenuBLL EntityBll = new SysEntityMenuBLL();
            SysPermissionBLL PermissionBll = new SysPermissionBLL();
            SysRoleMenuPermBLL permBll = new SysRoleMenuPermBLL();
            T_SYS_ROLE RoleT = new T_SYS_ROLE();

            RoleT = RoleBll.GetSysRoleByID(StrRoleID);
            try
            {
                string[] firstStr = tmpString.Split('#');
                string EntityIDsList = "";//菜单ID 列表
                //string ParentEntityIDsList = "";//父菜单ID列表
                //先找出不存在的记录 将不存在的记录做删除标志
                List<T_SYS_ROLEENTITYMENU> RoleEntityList = new List<T_SYS_ROLEENTITYMENU>();
                RoleEntityList = listRoleEntity;
               //2011-05-14 修改
                for (int i = 0; i < firstStr.Length; i++)
                {
                    string[] EntityPermStr = firstStr[i].Split('@');//获取菜单ID,将菜单实体和权限菜单分开
                    string[] EntityStr = EntityPermStr[1].Split(',');
                    string EntityID = EntityStr[0].ToString(); //新的菜单ID
                    EntityIDsList += EntityID + ",";                   
                    string[] PermissionEntitis = EntityPermStr[0].Split(';');

                    T_SYS_ROLEENTITYMENU ChildMenu = GetRoleEntityMenuByIDAndRoleID(EntityID, StrRoleID);

                    #region if ChildMenu Null 如果角色菜单在旧记录中不存在，则先添加角色菜单
                    if (ChildMenu == null) //为空则添加
                    {
                        T_SYS_ROLEENTITYMENU Childrolemenu = new T_SYS_ROLEENTITYMENU();
                        Childrolemenu.ROLEENTITYMENUID = System.Guid.NewGuid().ToString();
                        Childrolemenu.T_SYS_ENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", EntityID);
                        Childrolemenu.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", StrRoleID);
                        Childrolemenu.UPDATEDATE = null;
                        Childrolemenu.UPDATEUSER = "";
                        Childrolemenu.CREATEDATE = System.DateTime.Now;
                        Childrolemenu.CREATEUSER = StrAddUser;
                        Childrolemenu.REMARK = "";
                        //int AddResult = 1;//dal.Add(Childrolemenu);
                        int k = dal.Add(Childrolemenu);                     
                        #region 添加角色菜单实体对应的权限数据范围
                        for (int p = 0; p < PermissionEntitis.Length - 1; p++)
                        {
                            string StrDataRange = "";// PermissionEntitis[p].Split(',')[0];//数据范围
                            string StrPermissionID = "";// PermissionEntitis[p].Split(',')[1];//权限ID  2011-9-9 edit
                            if (PermissionEntitis[p].IndexOf(',') > -1)
                            {
                                StrDataRange = PermissionEntitis[p].Split(',')[0];//数据范围
                                StrPermissionID = PermissionEntitis[p].Split(',')[1];//权限ID
                            }
                            else
                            {
                                continue;
                            }
                            //T_SYS_PERMISSION PermissionT = new T_SYS_PERMISSION();
                            //PermissionT = PermissionBll.GetSysPermissionByID(StrPermissionID);
                            if (!string.IsNullOrEmpty(StrDataRange))
                            {
                                T_SYS_ROLEMENUPERMISSION PermRole = new T_SYS_ROLEMENUPERMISSION();
                                PermRole.ROLEMENUPERMID = System.Guid.NewGuid().ToString();
                                PermRole.DATARANGE = StrDataRange;
                                PermRole.T_SYS_PERMISSIONReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", StrPermissionID);
                                PermRole.T_SYS_ROLEENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLEENTITYMENU", "ROLEENTITYMENUID", Childrolemenu.ROLEENTITYMENUID);
                                PermRole.CREATEDATE = System.DateTime.Now;
                                PermRole.CREATEUSER = StrAddUser;
                                PermRole.UPDATEDATE = null;
                                PermRole.UPDATEUSER = "";
                                bool kk = permBll.SysRoleMenuPermAdd(PermRole, StrPermissionID, Childrolemenu.ROLEENTITYMENUID);
                                if (!kk)
                                {
                                    StrReturn = "false";
                                }

                                #region 调用即时通讯接口
                                if (k > 0)
                                {
                                    //存在公司信息或部门信息菜单授权则调用
                                    //string MenuCompanyID = "55623178-A187-421a-8556-067E6908207A";//公司菜单ID
                                    //string MenuDepartmentID = "04F86C10-02E3-4874-A198-4EC986C288CC";//部门菜单ID
                                    //if (EntityID == MenuCompanyID)
                                    //{
                                    //    AddUpdateUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                                    //}
                                    //if (EntityID == MenuDepartmentID)
                                    //{
                                    //    AddUpdateUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment);
                                    //}
                                }                                
                                #endregion
                            }
                        }
                        
                        #endregion                       
                    }
                    #endregion

                    #region else存在角色菜单实体，如果角色菜单实体对应的权限数据范围为空，则删除. 否则判断以前对应的角色菜单权限数据范围是否存在，存在则更新，不存在则添加
                    else  
                    {
                        string Newpermissions = ""; //新的权限ID
                        for (int p = 0; p < PermissionEntitis.Length - 1; p++)
                        {
                            string StrDataRange = "";// PermissionEntitis[p].Split(',')[0];//数据范围
                            string StrPermissionID = "";// PermissionEntitis[p].Split(',')[1];//权限ID
                            if (PermissionEntitis[p].IndexOf(',') > -1)
                            {
                                StrDataRange = PermissionEntitis[p].Split(',')[0];//数据范围
                                StrPermissionID = PermissionEntitis[p].Split(',')[1];//权限ID
                            }
                            else
                            {
                                continue;
                            }
                            T_SYS_PERMISSION perm = new T_SYS_PERMISSION();
                            perm = PermissionBll.GetSysPermissionByID(StrPermissionID);
                            Newpermissions += StrPermissionID + ",";
                            //T_SYS_PERMISSION PermissionT = new T_SYS_PERMISSION();
                            //PermissionT = PermissionBll.GetSysPermissionByID(StrPermissionID);
                            T_SYS_ROLEMENUPERMISSION ExistRecord = permBll.GetSysRoleMenuPermByPermIDAndRoleEntityID(StrPermissionID, ChildMenu.ROLEENTITYMENUID);

                            #region 判断以前对应的角色菜单权限数据范围是否存在，存在则更新
                            if (ExistRecord != null)
                            { //修改
                                if (!string.IsNullOrEmpty(StrDataRange))
                                {
                                    if (StrDataRange != ExistRecord.DATARANGE)
                                    {
                                        T_SYS_ROLEMENUPERMISSION PermRole = new T_SYS_ROLEMENUPERMISSION();
                                        PermRole.ROLEMENUPERMID = ExistRecord.ROLEMENUPERMID;
                                        PermRole.DATARANGE = StrDataRange;
                                        PermRole.EXTENDVALUE = ExistRecord.EXTENDVALUE;
                                        PermRole.T_SYS_PERMISSIONReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", StrPermissionID);
                                        PermRole.T_SYS_ROLEENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLEENTITYMENU", "ROLEENTITYMENUID", ChildMenu.ROLEENTITYMENUID);
                                        PermRole.CREATEDATE = ExistRecord.CREATEDATE;
                                        PermRole.CREATEUSER = ExistRecord.CREATEUSER;
                                        PermRole.UPDATEDATE = System.DateTime.Now; ;
                                        PermRole.UPDATEUSER = StrAddUser;                                        
                                        bool kk = permBll.SysRoleMenuPermUpdate(PermRole);
                                        //dal.DataContext.AddObject(PermRole.GetType().Name, PermRole);

                                        if (!kk)
                                        {
                                            StrReturn = "editfalse";
                                        }

                                        #region 调用即时通讯接口
                                        
                                        //存在公司信息或部门信息菜单授权则调用
                                        //string MenuCompanyID = "55623178-A187-421a-8556-067E6908207A";//公司菜单ID
                                        //string MenuDepartmentID = "04F86C10-02E3-4874-A198-4EC986C288CC";//部门菜单ID
                                        //if (EntityID == MenuCompanyID)
                                        //{
                                        //    AddUpdateUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                                        //}
                                        //if (EntityID == MenuDepartmentID)
                                        //{
                                        //    AddUpdateUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment);
                                        //}
                                        
                                        #endregion

                                    }
                                    //dal.Add(PermRole);

                                }
                                #region 数据范围为空 ，则删除 对应的信息  
                                else //删除记录
                                {
                                    bool delresult = permBll.SysRoleMenuPermDelete(ExistRecord.ROLEMENUPERMID);
                                    if (!delresult)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        #region 调用即时通讯接口
                                        
                                        //存在公司信息或部门信息菜单授权则调用
                                        //string MenuCompanyID = "55623178-A187-421a-8556-067E6908207A";//公司菜单ID
                                        //string MenuDepartmentID = "04F86C10-02E3-4874-A198-4EC986C288CC";//部门菜单ID
                                        //if (EntityID == MenuCompanyID)
                                        //{
                                        //    DelUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                                        //}
                                        //if (EntityID == MenuDepartmentID)
                                        //{
                                        //    DelUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment);
                                        //}
                                        
                                        #endregion
                                    }

                                }
                                #endregion

                            }
                            #endregion

                            #region 判断以前对应的角色菜单权限数据范围是否存在,不存在则添加
                            else
                            {//添加角色菜单权限记录
                                if (!string.IsNullOrEmpty(StrDataRange))
                                {
                                    T_SYS_ROLEMENUPERMISSION PermRole = new T_SYS_ROLEMENUPERMISSION();
                                    PermRole.ROLEMENUPERMID = System.Guid.NewGuid().ToString();
                                    PermRole.DATARANGE = StrDataRange;

                                    PermRole.T_SYS_PERMISSIONReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", StrPermissionID);
                                    PermRole.T_SYS_ROLEENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLEENTITYMENU", "ROLEENTITYMENUID", ChildMenu.ROLEENTITYMENUID);
                                    PermRole.CREATEDATE = System.DateTime.Now;
                                    PermRole.CREATEUSER = StrAddUser;
                                    PermRole.UPDATEDATE = null;
                                    PermRole.UPDATEUSER = "";
                                    bool kk = permBll.SysRoleMenuPermAdd(PermRole, StrPermissionID, ChildMenu.ROLEENTITYMENUID);
                                    if (!kk)
                                    {
                                        StrReturn = "addfalse";
                                    }

                                    //存在公司信息或部门信息菜单授权则调用
                                    //string MenuCompanyID = "55623178-A187-421a-8556-067E6908207A";//公司菜单ID
                                    //string MenuDepartmentID = "04F86C10-02E3-4874-A198-4EC986C288CC";//部门菜单ID
                                    //if (EntityID == MenuCompanyID)
                                    //{
                                    //    AddUpdateUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company);
                                    //}
                                    //if (EntityID == MenuDepartmentID)
                                    //{
                                    //    AddUpdateUserDepart(StrRoleID, StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment);
                                    //}
                                    //dal.Add(PermRole);

                                }
                            }
                            #endregion
                        }
                     //为了删除角色菜单，先（清空）删除菜单权限
                    int nn=dal.SaveContextChanges();
                    #region 删除现不存在的权限
                    if (!(GetRoleEntityPermissionByRoleEntityID(ChildMenu.ROLEENTITYMENUID)))
                    {
                        if (!RoleEntityMenuDelete(ChildMenu.ROLEENTITYMENUID))
                        {
                            StrReturn = "RoleEntityMenuDeleteISERROR";
                        }
                    }
                    #endregion


                    }
                    #endregion
                    // end childmenu
                }// end for

                int m=dal.SaveContextChanges();
                
                if (StrReturn != "")
                {
                    return false;
                }
                else
                {
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("角色菜单RoleEntityMenuBLL-RoleEntityMenuPermissionUpdateNew" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;

            }
            

            
            return true;
        }
        #endregion

        #region 修改角色菜单权限
        
        #endregion

        #endregion


        #region 即时通讯接口
        /// <summary>
        /// 添加或修改即时通讯的组织架构
        /// </summary>
        /// <param name="RoleId">角色ID</param>
        /// <param name="StrDataRange">范围大小</param>
        /// <param name="StrType">1 表示公司 2 部门</param>
        /// <returns></returns>
        //public string AddUpdateUserDepart(string  RoleId,string StrDataRange,SMT.SaaS.Permission.BLL.Utility.IMOrganize OrgType)
        //{
        //    string StrMessage = "";
        //    try
        //    {
        //        var ents = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_ROLE").Include("T_SYS_USER")
        //                   where ent.T_SYS_ROLE.ROLEID == RoleId
        //                   select ent;
        //        if (ents.Count() > 0)
        //        {
        //            string UserIDs ="";
        //            foreach(var ent in ents)
        //            {
        //                if(!(UserIDs.IndexOf(ent.T_SYS_USER.EMPLOYEEID) >-1))
        //                {
        //                    UserIDs += ent.T_SYS_USER.EMPLOYEEID + ",";
        //                }
        //            }
                    
        //            if(UserIDs !="")
        //            {
        //                UserIDs = UserIDs.Substring(0,UserIDs.Length-1);
        //                StrMessage = InsertDataToImServices(OrgType, StrDataRange, UserIDs);
        //            }
        //            else
        //            {
        //                StrMessage ="没有获取到角色对应的员工信息";
        //                return StrMessage;
        //            }
                    
                    
        //        }
        //        else
        //        {
        //            StrMessage = "角色" + RoleId + "没有分配给用户";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        StrMessage = "调用即时通讯接口出现错误AddUpdateUserDepart,角色ID" + RoleId + ex.ToString();
        //        SMT.Foundation.Log.Tracer.Debug(StrMessage);
        //    }
        //    return StrMessage;
        //}


        /// <summary>
        /// 自定义权限添加即时通讯接口调用
        /// </summary>
        /// <param name="RoleId">角色ID</param>
        /// <param name="OrganizeID">组织架构ID</param>
        /// <param name="StrDataRange">权限范围：0公司 1 部门</param>
        /// <param name="OrgType">类型：公司、部门</param>
        /// <returns></returns>
        //public string AddCustomerPermissionUpdateUserDepart(string RoleId, string OrganizeID, SMT.SaaS.Permission.BLL.Utility.IMOrganize StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize OrgType)
        //{
        //    string StrMessage = "";
        //    try
        //    {
        //        var ents = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_ROLE").Include("T_SYS_USER")
        //                   where ent.T_SYS_ROLE.ROLEID == RoleId
        //                   select ent;
        //        if (ents.Count() > 0)
        //        {
        //            string UserIDs = "";
        //            foreach (var ent in ents)
        //            {
        //                if (!(UserIDs.IndexOf(ent.T_SYS_USER.EMPLOYEEID) > -1))
        //                {
        //                    UserIDs += ent.T_SYS_USER.EMPLOYEEID + ",";
        //                }
        //            }

        //            if (UserIDs != "")
        //            {
        //                UserIDs = UserIDs.Substring(0, UserIDs.Length - 1);
        //                StrMessage = InsertCustomerPermissionDataToImServices(OrgType, OrganizeID,StrDataRange, UserIDs);
        //            }
        //            else
        //            {
        //                StrMessage = "没有获取到角色对应的员工信息";
        //                return StrMessage;
        //            }


        //        }
        //        else
        //        {
        //            StrMessage = "角色" + RoleId + "没有分配给用户";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        StrMessage = "调用即时通讯接口出现错误AddUpdateUserDepart,角色ID" + RoleId + ex.ToString();
        //        SMT.Foundation.Log.Tracer.Debug(StrMessage);
        //    }
        //    return StrMessage;
        //}

        /// <summary>
        /// 自定义权限添加组织架构权限
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="OrgDataRange"></param>
        /// <param name="UserIDs"></param>
        /// <returns></returns>
        //private string InsertCustomerPermissionDataToImServices(SMT.SaaS.Permission.BLL.Utility.IMOrganize OrgType, string OrganizeId,SMT.SaaS.Permission.BLL.Utility.IMOrganize OrgDataRange, string UserIDs)
        //{
        //    string StrMessage = "";

        //    string[] EmployeeIDs = UserIDs.Split(',');

            
        //    List<string> StrDepartIds = new List<string>();
        //    if (OrgType == SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company)
        //    {                
        //        StrDepartIds.Add(OrganizeId);
        //    }
        //    if (OrgType == SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment)
        //    {
        //        //公司级别   
        //        if (OrgDataRange == SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company)
        //        {
        //            StrDepartIds = GetDepartmentIDs(OrganizeId);
        //        }
        //        else
        //        {
        //            StrDepartIds.Add(OrganizeId);
        //        }

        //    }

        //    List<UserDept> LstDepts = new List<UserDept>();
        //    for (int i = 0; i < EmployeeIDs.Count(); i++)
        //    {
        //        for (int m = 0; m < StrDepartIds.Count(); m++)
        //        {
        //            UserDept dept = new UserDept();
        //            dept.DeptId = StrDepartIds[m];
        //            dept.UserId = EmployeeIDs[i];
        //            dept.ExtensionData = null;
        //            var Departs = from ent in LstDepts
        //                          where ent.DeptId == StrDepartIds[m]
        //                          && ent.UserId == EmployeeIDs[i]
        //                          select ent;
        //            if (!(Departs.Count() > 0))
        //            {
        //                LstDepts.Add(dept);
        //            }
        //        }
        //    }
                


        //    if (LstDepts.Count() > 0)
        //    {
        //        SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯接口AddUpdateUserDepart");
        //        ServiceClient ImClient = new ServiceClient();
        //        StrMessage = ImClient.AddUpdateUserDepart(LstDepts.ToArray());
        //        SMT.Foundation.Log.Tracer.Debug("调用即时通讯接口的结果AddUpdateUserDepart" + StrMessage);
        //    }

            
        //    return StrMessage;
        //}


        /// <summary>
        /// 添加信息到即时通讯中
        /// </summary>
        /// <param name="OrgType">组织架构类型</param>
        /// <param name="UserIDs">员工ID</param>
        /// <returns></returns>
        //private  string InsertDataToImServices(SMT.SaaS.Permission.BLL.Utility.IMOrganize OrgType,string StrDataRang, string UserIDs)
        //{
        //    string StrMessage = "";
            
        //    string[] EmployeeIDs = UserIDs.Split(',');
        //    SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient PersonClient = new SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();

        //    ServiceClient ImClient = new ServiceClient();
        //    SMT.SaaS.BLLCommonServices.PersonnelWS.V_EMPLOYEEPOST[] emppost = PersonClient.GetEmployeeDetailByIDs(EmployeeIDs);
        //    if (emppost.Count() > 0)
        //    {
        //        List<UserDept> LstDepts = new List<UserDept>();
                
        //        for (int i = 0; i < emppost.Count(); i++)
        //        {
                    
        //            string EmployeeID = emppost[i].T_HR_EMPLOYEE.EMPLOYEEID;//员工ID
                    
        //            for (int k = 0; k < emppost[i].EMPLOYEEPOSTS.Count(); k++)
        //            {
        //                string StrOrganizeId = "";
        //                List<string> StrDepartIds = new List<string>();
        //                if (OrgType == SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company)
        //                {                            
        //                    StrOrganizeId = emppost[i].EMPLOYEEPOSTS[k].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
        //                    StrDepartIds.Add(StrOrganizeId);
        //                }
        //                if (OrgType == SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment)
        //                {
        //                    //公司级别
        //                    if (StrDataRang == "1")
        //                    {
        //                        StrOrganizeId = emppost[i].EMPLOYEEPOSTS[k].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
        //                        StrDepartIds = GetDepartmentIDs(StrOrganizeId);
        //                    }
        //                    else
        //                    {
        //                        StrOrganizeId = emppost[i].EMPLOYEEPOSTS[k].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
        //                        StrDepartIds.Add(StrOrganizeId);
        //                    }
        //                }


        //                for (int m = 0; m < StrDepartIds.Count(); m++)
        //                {
        //                    UserDept dept = new UserDept();
        //                    dept.DeptId = StrOrganizeId;
        //                    dept.UserId = EmployeeID;
        //                    dept.ExtensionData = null;
        //                    var Departs = from ent in LstDepts
        //                                  where ent.DeptId == StrDepartIds[m]
        //                                  && ent.UserId == EmployeeID
        //                                  select ent;
        //                    if (!(Departs.Count() > 0))
        //                    {
        //                        LstDepts.Add(dept);
        //                    }
        //                }
                        

        //            }

        //        }

        //        if (LstDepts.Count() > 0)
        //        {
        //            SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯接口AddUpdateUserDepart");
        //            StrMessage = ImClient.AddUpdateUserDepart(LstDepts.ToArray());
        //            SMT.Foundation.Log.Tracer.Debug("调用即时通讯接口的结果AddUpdateUserDepart" + StrMessage);
        //        }
        //    }
        //    else
        //    {
        //        StrMessage = "调用即时通讯时没有获取到员工详细信息";
        //    }
        //    return StrMessage;
        //}


        #region 根据公司ID获取公司的所有部门
        /// <summary>
        /// 根据公司ID获取所有部门ID
        /// </summary>
        /// <param name="companyId">公司ID</param>
        /// <returns>返回部门ID集合</returns>
        public List<string> GetDepartmentIDs(string companyId)
        {
            List<string> StrIDs = new List<string>();
            try
            {
                BLLCommonServices.OrganizationWS.OrganizationServiceClient orgClient = new BLLCommonServices.OrganizationWS.OrganizationServiceClient();
                BLLCommonServices.OrganizationWS.T_HR_DEPARTMENT[] LstDepartment = orgClient.GetDepartmentActivedByCompanyID(companyId);
                if (LstDepartment.Count() > 0)
                {
                    for (int i = 0; i < LstDepartment.Count(); i++)
                    {
                        StrIDs.Add(LstDepartment[i].DEPARTMENTID);
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("调用即时通讯前获取公司的部门信息出错"+ ex.ToString());
            }
            return StrIDs;
        }
        #endregion

        /// <summary>
        /// 删除即时通讯接口的数据
        /// </summary>
        /// <param name="RoleId"></param>
        /// <param name="StrDataRange"></param>
        /// <param name="OrgType"></param>
        /// <returns></returns>
        //public string DelUserDepart(string RoleId, string StrDataRange, SMT.SaaS.Permission.BLL.Utility.IMOrganize OrgType)
        //{
        //    string StrMessage = "";
        //    try
        //    {
        //        var ents = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_ROLE").Include("T_SYS_USER")
        //                   where ent.T_SYS_ROLE.ROLEID == RoleId
        //                   select ent;
        //        if (ents.Count() > 0)
        //        {
        //            string UserIDs = "";
        //            foreach (var ent in ents)
        //            {
        //                if (!(UserIDs.IndexOf(ent.T_SYS_USER.EMPLOYEEID) > -1))
        //                {
        //                    UserIDs += ent.T_SYS_USER.EMPLOYEEID + ",";
        //                }
        //            }

        //            if (UserIDs != "")
        //            {
        //                UserIDs = UserIDs.Substring(0, UserIDs.Length - 1);
        //                StrMessage = DelDataFromImServices(OrgType,StrDataRange, UserIDs);
        //            }
        //            else
        //            {
        //                StrMessage = "没有获取到角色对应的员工信息";
        //                return StrMessage;
        //            }


        //        }
        //        else
        //        {
        //            StrMessage = "角色" + RoleId + "没有分配给用户";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        StrMessage = "调用即时通讯接口出现错误AddUpdateUserDepart,角色ID" + RoleId + ex.ToString();
        //        SMT.Foundation.Log.Tracer.Debug(StrMessage);
        //    }
        //    return StrMessage;
        //}



        /// <summary>
        /// 删除即时通讯的组织架构信息
        /// </summary>
        /// <param name="OrgType">组织架构类型</param>
        /// <param name="UserIDs">员工ID</param>
        /// <returns></returns>
        //private string DelDataFromImServices(SMT.SaaS.Permission.BLL.Utility.IMOrganize OrgType,string StrDataRange, string UserIDs)
        //{
        //    string StrMessage = "";

        //    string[] EmployeeIDs = UserIDs.Split(',');
        //    SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient PersonClient = new SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();

        //    ServiceClient ImClient = new ServiceClient();
        //    SMT.SaaS.BLLCommonServices.PersonnelWS.V_EMPLOYEEPOST[] emppost = PersonClient.GetEmployeeDetailByIDs(EmployeeIDs);
        //    if (emppost.Count() > 0)
        //    {
        //        List<UserDept> LstDepts = new List<UserDept>();
        //        for (int i = 0; i < emppost.Count(); i++)
        //        {
        //            string EmployeeID = emppost[i].T_HR_EMPLOYEE.EMPLOYEEID;//员工ID

        //            for (int k = 0; k < emppost[i].EMPLOYEEPOSTS.Count(); k++)
        //            {
        //                string StrOrganizeId = "";
        //                List<string> StrDepartIds = new List<string>();
        //                if (OrgType == SMT.SaaS.Permission.BLL.Utility.IMOrganize.Company)
        //                {
        //                    StrOrganizeId = emppost[i].EMPLOYEEPOSTS[k].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;

        //                }
        //                if (OrgType == SMT.SaaS.Permission.BLL.Utility.IMOrganize.Deaprtment)
        //                {
        //                    //公司级别
        //                    if (StrDataRange == "1")
        //                    {
        //                        StrOrganizeId = emppost[i].EMPLOYEEPOSTS[k].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
        //                        StrDepartIds = GetDepartmentIDs(StrOrganizeId);
        //                    }
        //                    else
        //                    {
        //                        StrOrganizeId = emppost[i].EMPLOYEEPOSTS[k].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
        //                        StrDepartIds.Add(StrOrganizeId);
        //                    }
        //                    StrOrganizeId = emppost[i].EMPLOYEEPOSTS[k].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
        //                }

        //                for (int m = 0; m < StrDepartIds.Count(); m++)
        //                {
        //                    UserDept dept = new UserDept();
        //                    dept.DeptId = StrOrganizeId;
        //                    dept.UserId = EmployeeID;
        //                    dept.ExtensionData = null;
        //                    var Departs = from ent in LstDepts
        //                                  where ent.DeptId == StrDepartIds[m]
        //                                  && ent.UserId == EmployeeID
        //                                  select ent;
        //                    if (!(Departs.Count() > 0))
        //                    {
        //                        LstDepts.Add(dept);
        //                    }
        //                }
                            
                        
        //            }

        //        }
        //        if (LstDepts.Count() > 0)
        //        {
        //            SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯接口");
        //            StrMessage = ImClient.DelUserDepart(LstDepts.ToArray());
        //            SMT.Foundation.Log.Tracer.Debug("调用即时通讯接口的结果AddUpdateUserDepart" + StrMessage);
        //        }
        //    }
        //    else
        //    {
        //        StrMessage = "调用即时通讯时没有获取到员工详细信息";
        //    }
        //    return StrMessage;
        //}

        #endregion



    }
}
