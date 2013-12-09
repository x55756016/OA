using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using SMT.Foundation.Log;
using SMT.SaaS.Permission.BLL.PersonnelWS;
using SMT.SaaS.Permission.DAL;

namespace SMT.SaaS.Permission.BLL
{
    public class FbAdminBLL:BaseBll<T_SYS_ROLE>
    {
        /// <summary>
        /// 添加预算管理员
        /// 1 先添加预算管理员的角色，并添加对应的增加、删除、修改、查看对应的公司级的权限
        /// 2 如果是超级管理员则添加对应的自定义权限
        /// 3 添加预算管理员和角色关联的数据
        /// </summary>
        /// <param name="lstobj"></param>
        /// <returns></returns>
        public string AddFbAdmin(List<T_SYS_FBADMIN> lstobj)
        {
            string StrReturn = "";
            dal.BeginTransaction();
            try
            {

                if (lstobj != null)
                {
                    if (lstobj.Count() > 0)
                    {
                        //获取所有角色信息


                        #region 初始化变量
                        var entroles = from ent in dal.GetObjects<T_SYS_ROLE>()
                                       select ent;
                        var entusers = from ent in dal.GetObjects<T_SYS_USER>()
                                       select ent;
                        string SysDictEntity = GetMenuid("T_FB_SUBJECTTYPE");//系统字典MUNUid
                        string CompanySubEntity = GetMenuid("T_FB_SUBJECTCOMPANY");//公司科目维护ID
                        string DepartmentSubEntity = GetMenuid("T_FB_SUBJECTDEPTMENT");//部门科目维护ID
                        string SubjectCompanySet = GetMenuid("T_FB_SUBJECTCOMPANYSET");//公司科目分配

                        string PermissionAddID = GetPermissionID(Convert.ToInt32(Permissions.Add));//权限添加ID
                        string PermissionEditID = GetPermissionID(Convert.ToInt32(Permissions.Edit));//修改ID
                        string PermissionDelID = GetPermissionID(Convert.ToInt32(Permissions.Delete));//权限删除ID
                        string PermissionViewID = GetPermissionID(Convert.ToInt32(Permissions.Browse));//权限查看ID
                        #endregion

                        for (int i = 0; i < lstobj.Count(); i++)
                        {
                            //dal.Add(lstobj[i]);
                            string stremployid = lstobj[i].EMPLOYEEID;
                            string strCompanyId = lstobj[i].EMPLOYEECOMPANYID;
                            var entFb = from e in dal.GetObjects<T_SYS_FBADMIN>()
                                        where e.EMPLOYEEID == stremployid && e.EMPLOYEECOMPANYID == strCompanyId
                                        select e;
                            if (entFb != null && entFb.Any())//判断该员工在此公司有没有记录，有则不能进行添加
                            {
                                StrReturn = "该员工已在此公司有管理员角色";
                                dal.RollbackTransaction();
                                return StrReturn;
                            }
                            var entuser = entusers.Where(p => p.EMPLOYEEID == stremployid);
                            string SysuserID = "";
                            if (entuser.Count() > 0)
                            {
                                SysuserID = entuser.FirstOrDefault().SYSUSERID;
                            }
                            else
                            {
                                break;
                            }
                            string strcompanyid = lstobj[i].OWNERCOMPANYID;//公司ID
                            string strrolename = lstobj[i].ROLENAME;
                            var companyrole = entroles.Where(p => p.OWNERCOMPANYID == strcompanyid && p.ROLENAME.Contains(strrolename));
                            if (companyrole != null)
                            {
                                //这里控制了一个公司只有一个人
                                //if (companyrole.Count() == 0)
                                //{
                                T_SYS_ROLE role;//角色实体
                                int k;//添加角色返回的值
                                AddRole(lstobj, i, out role, out k);
                                if (k > 0)
                                {
                                    #region 添加系统字典菜单

                                    if (lstobj[i].ISSUPPERADMIN == "1") //只有超级预算管理员才可以显示系统字典
                                    {
                                        if (SysDictEntity != null)//添加系统字典菜单角色
                                        {
                                            T_SYS_ROLEENTITYMENU roleSystemDicEnt = AddRoleEntity(SysDictEntity, role);
                                            if (roleSystemDicEnt == null)
                                            {
                                                dal.RollbackTransaction();
                                                StrReturn = "ERROR";
                                                return StrReturn;
                                            }
                                            else
                                            {
                                                bool IsResult = AddRoleEntityPermissionByCEDV(PermissionAddID, PermissionEditID, PermissionDelID, PermissionViewID, role, roleSystemDicEnt);
                                                if (IsResult == false)
                                                {
                                                    dal.RollbackTransaction();
                                                    StrReturn = "ERROR";
                                                    return StrReturn;
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    #region 添加公司科目维护菜单

                                    if (CompanySubEntity != null)//添加公司科目菜单角色
                                    {
                                        T_SYS_ROLEENTITYMENU roleCompanyEnt = AddRoleEntity(CompanySubEntity, role);
                                        if (roleCompanyEnt == null)
                                        {
                                            dal.RollbackTransaction();
                                            StrReturn = "ERROR";
                                            return StrReturn;
                                        }
                                        else
                                        {
                                            bool IsResult = AddRoleEntityPermissionByCEDV(PermissionAddID, PermissionEditID, PermissionDelID, PermissionViewID, role, roleCompanyEnt);
                                            if (IsResult == false)
                                            {
                                                dal.RollbackTransaction();
                                                StrReturn = "ERROR";
                                                return StrReturn;
                                            }
                                        }
                                    }
                                    #endregion

                                    #region 添加部门科目维护菜单

                                    if (DepartmentSubEntity != null)//添加部门科目维护菜单角色
                                    {
                                        T_SYS_ROLEENTITYMENU roleDepartmentEnt = AddRoleEntity(DepartmentSubEntity, role);
                                        if (roleDepartmentEnt == null)
                                        {
                                            dal.RollbackTransaction();
                                            StrReturn = "ERROR";
                                            return StrReturn;
                                        }
                                        else
                                        {
                                            bool IsResult = AddRoleEntityPermissionByCEDV(PermissionAddID, PermissionEditID, PermissionDelID, PermissionViewID, role, roleDepartmentEnt);
                                            if (IsResult == false)
                                            {
                                                dal.RollbackTransaction();
                                                StrReturn = "ERROR";
                                                return StrReturn;
                                            }
                                        }
                                    }
                                    #endregion

                                    #region 添加公司科目分配

                                    if (SubjectCompanySet != null)//添加部门科目维护菜单角色
                                    {
                                        T_SYS_ROLEENTITYMENU SubjectSetEnt = AddRoleEntity(SubjectCompanySet, role);
                                        if (SubjectSetEnt == null)
                                        {
                                            dal.RollbackTransaction();
                                            StrReturn = "ERROR";
                                            return StrReturn;
                                        }
                                        else
                                        {
                                            bool IsResult = AddRoleEntityPermissionByCEDV(PermissionAddID, PermissionEditID, PermissionDelID, PermissionViewID, role, SubjectSetEnt);
                                            if (IsResult == false)
                                            {
                                                dal.RollbackTransaction();
                                                StrReturn = "ERROR";
                                                return StrReturn;
                                            }
                                        }
                                    }
                                    #endregion

                                    #region 用户赋角色并提交事务
                                    lstobj[i].CREATEDATE = System.DateTime.Now;
                                    lstobj[i].SYSUSERID = SysuserID;
                                    int IntFb = dal.Add(lstobj[i]);
                                    if (IntFb > 0)
                                    {

                                        T_SYS_FBADMINROLE fbrole = new T_SYS_FBADMINROLE();
                                        fbrole.FBADMINROLEID = System.Guid.NewGuid().ToString();
                                        fbrole.ROLEID = role.ROLEID;
                                        fbrole.T_SYS_FBADMINReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_FBADMIN", "FBADMINID", lstobj[i].FBADMINID);
                                        fbrole.ADDDATE = System.DateTime.Now;
                                        int IntAdminRole = dal.Add(fbrole);
                                        if (IntAdminRole == 0)
                                        {
                                            dal.RollbackTransaction();
                                            StrReturn = "ERROR";
                                            return StrReturn;
                                        }
                                    }
                                    int IntUserRole = AddUserRole(lstobj, i, SysuserID, role);
                                    if (IntUserRole > 0)
                                    {
                                        //dal.CommitTransaction();
                                    }
                                    else
                                    {
                                        dal.RollbackTransaction();
                                        StrReturn = "ERROR";
                                        return StrReturn;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    dal.RollbackTransaction();
                                    StrReturn = "ERROR";
                                    return StrReturn;
                                }
                                //} //控制了只一个人，暂时先注释
                            }
                        }
                    }
                }
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                StrReturn = "ERROR";
                Tracer.Debug("FBADMINBLL中AddFbAdmin出现错误" + System.DateTime.Now.ToString() + "错误信息：" + ex.ToString());
            }
            return StrReturn;
        }


        /// <summary>
        /// 修改预算管理员
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <param name="ownercompanyid">公司ID</param>
        /// <param name="fbAdmin">新员工信息（只需要取员工信息）</param>
        /// <returns></returns>
        public string UpdateFbAdmin(string employeeid, string employeeCompanyID, T_SYS_FBADMIN fbAdmin)
        {
            string strMsg = string.Empty;
            try
            {
               var ents = (from ent in dal.GetObjects<T_SYS_FBADMIN>()
                           where ent.EMPLOYEEID == employeeid && ent.EMPLOYEECOMPANYID == employeeCompanyID
                           select ent).FirstOrDefault();
               var entusers = (from ent in dal.GetObjects<T_SYS_USER>()
                               where ent.EMPLOYEEID == fbAdmin.EMPLOYEEID
                              select ent).FirstOrDefault();
               if (ents != null && entusers != null)
               {
                   ents.SYSUSERID = entusers.SYSUSERID;//员工系统用户ID
                   ents.UPDATEUSERID = fbAdmin.UPDATEUSERID;
                   ents.UPDATEDATE = DateTime.Now;
                   ents.UPDATEUSERNAME = fbAdmin.UPDATEUSERNAME;
                   ents.EMPLOYEEID = fbAdmin.EMPLOYEEID;
                   ents.EMPLOYEEPOSTID = fbAdmin.EMPLOYEEPOSTID;
                   ents.EMPLOYEEDEPARTMENTID = fbAdmin.EMPLOYEEDEPARTMENTID;
                   ents.EMPLOYEECOMPANYID = fbAdmin.EMPLOYEECOMPANYID;

                   int IntFb = dal.Update(ents);
                   if (IntFb <= 0)
                   {
                       strMsg = "修改失败";
                   }
               }
               else
               {
                   strMsg = "获取员工预算管理员或系统用户信息失败";
               }
            }
            catch (Exception ex)
            {
                strMsg = "更新出错：" + ex.ToString();
            }
            return strMsg;
        }

        #region 添加用户角色
        
        /// <summary>
        /// 添加用户角色 在添加用户角色前要先清空缓存
        /// </summary>
        /// <param name="lstobj"></param>
        /// <param name="i"></param>
        /// <param name="SysuserID"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private int AddUserRole(List<T_SYS_FBADMIN> lstobj, int i, string SysuserID, T_SYS_ROLE role)
        {
            int IntUserRole=0;
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_ROLE").Include("T_SYS_USER")
                           where ent.T_SYS_ROLE.ROLEID == role.ROLEID
                           && ent.T_SYS_USER.SYSUSERID == SysuserID
                           select ent;

                T_SYS_USERROLE AddUserRole = new T_SYS_USERROLE();
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        if (ents.FirstOrDefault() != null)
                        {
                            AddUserRole = ents.FirstOrDefault();
                            IntUserRole = 2;
                        }
                    }
                }
                if (string.IsNullOrEmpty(AddUserRole.USERROLEID))
                {
                    AddUserRole.USERROLEID = System.Guid.NewGuid().ToString();

                    AddUserRole.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", role.ROLEID);
                    AddUserRole.T_SYS_USERReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_USER", "SYSUSERID", SysuserID);
                    AddUserRole.OWNERCOMPANYID = lstobj[i].OWNERCOMPANYID;


                    AddUserRole.UPDATEDATE = System.DateTime.Now;
                    AddUserRole.UPDATEUSER = lstobj[i].SYSUSERID;
                    AddUserRole.CREATEUSER = lstobj[i].SYSUSERID;
                    AddUserRole.CREATEDATE = System.DateTime.Now;
                    IntUserRole = dal.Add(AddUserRole);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("FBADMINBLL中AddUserRole出现错误" + System.DateTime.Now.ToString() + "错误信息：" + ex.ToString());
            }
            return IntUserRole;
        }

        #endregion

        #region 添加用户角色
        
        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="lstobj"></param>
        /// <param name="i"></param>
        /// <param name="role"></param>
        /// <param name="k"></param>
        private void AddRole(List<T_SYS_FBADMIN> lstobj, int i, out T_SYS_ROLE role, out int k)
        {
            role = new T_SYS_ROLE();
            k = 0;
            try
            {
                string roleName = lstobj[i].ROLENAME;
                var ents = from ent in dal.GetObjects<T_SYS_ROLE>()
                           where ent.ROLENAME == roleName
                           select ent;
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        if (ents.FirstOrDefault() != null)
                        {
                            role = ents.FirstOrDefault();
                        }
                    }
                }
                if (string.IsNullOrEmpty(role.ROLEID))
                {

                    role.ROLEID = System.Guid.NewGuid().ToString();
                    role.SYSTEMTYPE = "3";//3为预算系统
                    role.OWNERCOMPANYID = lstobj[i].OWNERCOMPANYID;
                    role.OWNERDEPARTMENTID = lstobj[i].OWNERDEPARTMENTID;
                    role.OWNERPOSTID = lstobj[i].OWNERPOSTID;
                    role.REMARK = "系统默认设置管理员设置";
                    role.CREATEDATE = System.DateTime.Now;
                    role.ROLENAME = lstobj[i].ROLENAME;
                    role.CHECKSTATE = "2";
                    role.CREATECOMPANYID = lstobj[i].OWNERCOMPANYID;
                    role.CREATEDEPARTMENTID = lstobj[i].OWNERDEPARTMENTID;
                    role.CREATEPOSTID = lstobj[i].OWNERPOSTID;
                    role.CREATEUSER = lstobj[i].SYSUSERID;
                    k = dal.Add(role);
                }
                else
                {
                    k = 2;//为2则表示已经存在
                }
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("FBADMINBLL中AddRole出现错误" + System.DateTime.Now.ToString() + "错误信息：" + ex.ToString());
            }
        }


        #endregion

        /// <summary>
        /// 添加新建、修改、删除、查看 权限的角色菜单权限值
        /// </summary>
        /// <param name="PermissionAddID">新建权限ID</param>
        /// <param name="PermissionEditID">修改权限ID</param>
        /// <param name="PermissionDelID">删除权限ID</param>
        /// <param name="PermissionViewID">查看权限ID</param>
        /// <param name="role">角色实体</param>
        /// <param name="roleSystemDicEnt">菜单实体</param>
        /// <returns></returns>
        private bool AddRoleEntityPermissionByCEDV(string PermissionAddID, string PermissionEditID, string PermissionDelID, string PermissionViewID, T_SYS_ROLE role, T_SYS_ROLEENTITYMENU roleSystemDicEnt)
        {
            bool IsResult = false;
            int IntAdd = AddRoleEntityPermission(PermissionAddID, role, roleSystemDicEnt);//添加
            int IntEdit = AddRoleEntityPermission(PermissionEditID, role, roleSystemDicEnt);//修改
            int IntDel = AddRoleEntityPermission(PermissionDelID, role, roleSystemDicEnt);//删除
            int IntView = AddRoleEntityPermission(PermissionViewID, role, roleSystemDicEnt);//查看
            if (IntAdd > 0 && IntEdit > 0 && IntDel > 0 && IntView > 0)
            {
                IsResult = true;
            }
            return IsResult;
        }

        #region 添加角色菜单权限        
        
        /// <summary>
        /// 添加角色菜单权限
        /// </summary>
        /// <param name="PermissionAddID"></param>
        /// <param name="role"></param>
        /// <param name="roleSystemDicEnt"></param>
        private int AddRoleEntityPermission(string PermissionAddID, T_SYS_ROLE role, T_SYS_ROLEENTITYMENU roleSystemDicEnt)
        {
            int IntResult = 0;
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_PERMISSION").Include("T_SYS_ROLEENTITYMENU")
                           where ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == roleSystemDicEnt.ROLEENTITYMENUID
                           && ent.T_SYS_PERMISSION.PERMISSIONID == PermissionAddID
                           select ent;

                T_SYS_ROLEMENUPERMISSION PermRole = new T_SYS_ROLEMENUPERMISSION();
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        if (ents.FirstOrDefault() != null)
                        {
                            PermRole = ents.FirstOrDefault();
                            IntResult = 1;
                        }
                    }
                }

                if (string.IsNullOrEmpty(PermRole.ROLEMENUPERMID))
                {
                    PermRole.ROLEMENUPERMID = System.Guid.NewGuid().ToString();
                    PermRole.DATARANGE = "1";//默认为公司级别
                    PermRole.T_SYS_PERMISSIONReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PERMISSION", "PERMISSIONID", PermissionAddID);
                    PermRole.T_SYS_ROLEENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLEENTITYMENU", "ROLEENTITYMENUID", roleSystemDicEnt.ROLEENTITYMENUID);

                    PermRole.CREATEDATE = System.DateTime.Now;
                    PermRole.CREATEUSER = role.OWNERID;
                    PermRole.UPDATEDATE = null;
                    PermRole.UPDATEUSER = "";
                    PermRole.EXTENDVALUE = "";
                    IntResult = dal.Add(PermRole);
                    //if (IntResult == 0)
                    //{
                    //    dal.RollbackTransaction();
                    //}
                }
            }
            catch (Exception ex)
            {
                IntResult = 0;
                Tracer.Debug("FBADMINBLL中AddRoleEntityPermission出现错误" + System.DateTime.Now.ToString()+"错误信息："+ex.ToString());
                
            }
            return IntResult;
        }
        #endregion

        #region 添加角色菜单

        /// <summary>
        /// 添加角色菜单
        /// </summary>
        /// <param name="SysDictEntity"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private T_SYS_ROLEENTITYMENU AddRoleEntity(string SysDictEntityId, T_SYS_ROLE role)
        {
            T_SYS_ROLEENTITYMENU rolemenu = null;
            try
            {
                rolemenu = new T_SYS_ROLEENTITYMENU();
                var ents = from ent in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                           where ent.T_SYS_ROLE.ROLEID == role.ROLEID && ent.T_SYS_ENTITYMENU.ENTITYMENUID == SysDictEntityId
                           select ent;
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        if (ents.FirstOrDefault() != null)
                        {
                            rolemenu = ents.FirstOrDefault();
                        }
                    }
                }

                if (string.IsNullOrEmpty(rolemenu.ROLEENTITYMENUID))
                {
                    rolemenu.ROLEENTITYMENUID = System.Guid.NewGuid().ToString();

                    rolemenu.T_SYS_ENTITYMENUReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", SysDictEntityId);
                    //rolemenu.T_SYS_ENTITYMENU.ENTITYMENUID = EntityParentID;
                    rolemenu.T_SYS_ROLEReference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ROLE", "ROLEID", role.ROLEID);
                    rolemenu.T_SYS_ROLEMENUPERMISSION = null;//将其设置为NULL
                    rolemenu.UPDATEDATE = null;
                    rolemenu.UPDATEUSER = "";
                    rolemenu.CREATEDATE = System.DateTime.Now;
                    rolemenu.CREATEUSER = role.OWNERID;
                    rolemenu.REMARK = "";
                    int k = dal.Add(rolemenu);
                    if (k == 0)
                    {
                        rolemenu = null;
                    }
                }
            }
            catch (Exception ex)
            {
                rolemenu = null;
                Tracer.Debug("FBADMINBLL中AddRoleEntity出现错误" + System.DateTime.Now.ToString() + "错误信息：" + ex.ToString());
            }
            return rolemenu;
        }

        #endregion

        #region 返回实体菜单

        /// <summary>
        /// 根据菜单标号返回菜单ID
        /// </summary>
        /// <param name="MenuCode"></param>
        /// <returns></returns>
        private string GetMenuid(string MenuCode)
        {
            string StrReturn = "";
            var entmenus = from ent in dal.GetObjects<T_SYS_ENTITYMENU>()
                           where ent.MENUCODE == MenuCode
                           select ent;
            if (entmenus != null)
            {
                if (entmenus.Count() > 0)
                {
                    if (entmenus.FirstOrDefault() != null)
                    {
                        StrReturn = entmenus.FirstOrDefault().ENTITYMENUID;
                    }
                }
            }
           
            return StrReturn;
        }

        #endregion

        #region 返回权限的GUID        
       
        /// <summary>
        /// 返回权限值的GUID
        /// </summary>
        /// <param name="PermissionValue"></param>
        /// <returns></returns>
        private string GetPermissionID(int PermissionValue)
        {
            string StrReturn = "";
            string StrPerm = PermissionValue.ToString();
            var entpermissions = from ent in dal.GetObjects<T_SYS_PERMISSION>()
                                 where ent.PERMISSIONVALUE == StrPerm
                                 select ent;
            if (entpermissions != null)
            {
                if (entpermissions.Count() > 0)
                {
                    if (entpermissions.FirstOrDefault() != null)
                    {
                        StrReturn = entpermissions.FirstOrDefault().PERMISSIONID;
                    }
                }
            }
            return StrReturn;
        }
        #endregion

        /// <summary>
        /// 获取管理员
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="companyids"></param>
        /// <returns></returns>
        public IQueryable<V_FBAdmin> GetFbAdmins(string userID,List<string>  companyids)
        {
            IQueryable<V_FBAdmin> LstFbAdmins=null ;
            try
            {
                List<string> employids = new List<string>();
                List<V_FBAdmin> FbAdmins = new List<V_FBAdmin>();
                var ents = from ent in dal.GetObjects<T_SYS_FBADMIN>()
                           where ent.ISCOMPANYADMIN=="1" && ent.ISSUPPERADMIN=="0"
                           && companyids.Contains(ent.OWNERCOMPANYID) || ent.CREATEUSERID == userID
                           select ent;
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        ents.ToList().ForEach(item => {
                            //employids.Add(item.SYSUSERID);
                            V_FBAdmin fbadmin = new V_FBAdmin();
                            fbadmin.FBADMINID = item.FBADMINID;
                            fbadmin.ADDUSERID = item.CREATEUSERID;
                            fbadmin.EMPLOYEEID = item.EMPLOYEEID;
                            fbadmin.OWNERCOMPANYID = item.OWNERCOMPANYID;
                            fbadmin.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                            fbadmin.OWNERPOSTID = item.OWNERPOSTID;
                            fbadmin.EMPLOYEECOMPANYID = item.EMPLOYEECOMPANYID;
                            fbadmin.EMPLOYEEDEPARTMENTID = item.EMPLOYEEDEPARTMENTID;
                            fbadmin.EMPLOYEEPOSTID = item.EMPLOYEEPOSTID;
                            fbadmin.AddDate =(DateTime?) item.CREATEDATE;
                            FbAdmins.Add(fbadmin);
                        });
                    }
                  
                }
                
                if (FbAdmins.Count() > 0)
                {
                    LstFbAdmins = FbAdmins.AsQueryable();
                }
                

            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单FbAdminBLL-GetSysMenuByTypeWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                //
            }
            return LstFbAdmins;
        }

        /// <summary>
        /// 根据系统用户ID，判断用户是否是预算管理员
        /// </summary>
        /// <param name="sysUserID"></param>
        /// <returns></returns>
        public T_SYS_FBADMIN getFbAdminBySysUserID(string sysUserID)
        {
            T_SYS_FBADMIN User = null;
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_FBADMIN>()
                           where ent.SYSUSERID == sysUserID
                           select ent;
                
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        User = ents.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单FbAdminBLL-getFbAdminBySysUserID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                
            }
            return User;
        }



        /// <summary>
        /// 根据系统用户ID，判断用户是否是预算管理员
        /// 
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <returns>0 为有错误  1 公司预算员  2超级预算员</returns>
        public int getFbAdminByUserID(string EmployeeID)
        {
            int IntResult = 0;
            try
            {
                var Users = from ent in dal.GetObjects<T_SYS_USER>()
                            where ent.EMPLOYEEID == EmployeeID
                            select ent;
                if (Users != null)
                {
                    if (Users.Count() > 0)
                    {
                        var ents = from ent in dal.GetObjects<T_SYS_FBADMIN>()
                                   where ent.SYSUSERID == Users.FirstOrDefault().SYSUSERID
                                   select ent;
                        if (ents != null)
                        {
                            if (ents.Count() > 0)
                            {
                                if (ents.FirstOrDefault().ISCOMPANYADMIN == "1")
                                {
                                    IntResult = 1;
                                }
                                if (ents.FirstOrDefault().ISSUPPERADMIN =="1")
                                {
                                    IntResult = 2;
                                }
                            }
                        }
                    }
                }
                
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单FbAdminBLL-getFbAdminByUserID" + System.DateTime.Now.ToString() + " " + ex.ToString());

            }
            return IntResult;
        }

        public string DeleteFbAdmin(string employeeid, string employeeCompanyID)
        {
            string StrReturn = "";
            dal.BeginTransaction();
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_FBADMIN>()
                           where ent.EMPLOYEEID == employeeid && ent.EMPLOYEECOMPANYID == employeeCompanyID
                           select ent;
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        string StrSearch = "预算配置员";
                        var entroles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                                       where StrSearch.Contains(ent.T_SYS_ROLE.ROLENAME)
                                       && ent.T_SYS_USER.EMPLOYEEID == employeeid
                                       select ent;
                        if (entroles != null)
                        {
                            if (entroles.Count() > 0)
                            {
                              int IntK=  dal.Delete(entroles.FirstOrDefault());
                              if (IntK == 0)
                              {
                                  StrReturn = "DELROLEERROR";
                              }
                            }
                        }
                        foreach (var item in ents)//删除一个人，则该公司下面所有的预算管理员都删除
                        {
                            var fbRoles = from ent in dal.GetObjects<T_SYS_FBADMINROLE>().Include("T_SYS_FBADMIN")
                                          where ent.T_SYS_FBADMIN.FBADMINID == item.FBADMINID

                                          select ent;
                            if (fbRoles != null)
                            {
                                if (fbRoles.Count() > 0)
                                {
                                    int IntR = dal.Delete(fbRoles.FirstOrDefault());
                                    if (IntR == 0)
                                    {
                                        StrReturn = "DELFBROLEERROR";
                                    }
                                }
                            }

                            int IntM = dal.Delete(item);
                            if (IntM == 0)
                            {
                                StrReturn = "DELFBADMINERROR";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StrReturn = "ERROR";
                Tracer.Debug("菜单FbAdminBLL-DeleteFbAdmin" + System.DateTime.Now.ToString() + " " + ex.ToString());
                
            }
            if (string.IsNullOrEmpty(StrReturn))
            {
                dal.CommitTransaction();
            }
            else
            {
                dal.RollbackTransaction();
            }
            return StrReturn;
        }

    }
}
