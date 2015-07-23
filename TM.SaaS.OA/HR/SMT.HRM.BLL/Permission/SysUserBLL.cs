using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL.Permission;
using SMT.Foundation.Log;
using TM_SaaS_OA_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using System.Data.Objects;
using SMT.HRM.CustomModel.Permission;
using System.Threading;
using SMT.SaaS.SmtOlineEn;
using SMT.HRM.CustomModel;
using SMT.HRM.BLL;
using TM.SaaS.CommonBll;


namespace SMT.HRM.BLL.Permission
{
    #region 系统用户
    public class SysUserBLL : BaseBll<T_SYS_USER>, ILookupEntity
    {


        /// <summary>
        /// 根据用户名称得到用户信息
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户信息</returns>
        public T_SYS_USER GetUserInfo(string userName)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.USERNAME == userName && a.STATE == "1"
                           select a;
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据用户ID得到用户信息
        /// </summary>
        /// <param name="userName">用户ID</param>
        /// <returns>用户信息</returns>
        public T_SYS_USER GetUserByID(string userID)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.SYSUSERID == userID
                           select a;

                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据员工ID得到用户信息
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>用户信息</returns>
        public T_SYS_USER GetUserByEmployeeID(string employeeID)
        {
            try
            {
                string keyString = "GetUserByEmployeeID" + employeeID;
                T_SYS_USER user;
                if (WCFCache.Current[keyString] == null)
                {


                    user = (from a in dal.GetTable()
                           where a.EMPLOYEEID == employeeID
                           select a).FirstOrDefault();
                    WCFCache.Current.Insert(keyString, user, DateTime.Now.AddMinutes(15));


                }
                else
                {
                    user = (T_SYS_USER)WCFCache.Current[keyString];

                }



                return user;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserByEmployeeID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public bool AddSysUserInfo(T_SYS_USER UserObj)
        {
            try
            {
                int i = dal.Add(UserObj);
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
                Tracer.Debug("系统用户SysUserBLL-AddSysUserInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }
        
        /// <summary>
        /// 修改系统用户信息
        /// </summary>
        /// <param name="UserObj"></param>
        /// <returns></returns>
        public bool UpdateSysUserInfo(T_SYS_USER UserObj)
        {

            try
            {
                //var users = from ent in dal.GetObjects<T_SYS_USER>()
                //            where ent.USERNAME == UserObj.USERNAME
                //            select ent;
                var entity = from ent in dal.GetTable()
                             where ent.SYSUSERID == UserObj.SYSUSERID
                             select ent;

                //dal.BeginTransaction();
                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();
                    Utility.CloneEntity<T_SYS_USER>(UserObj, entitys);
                    int i = 0;
                    //if (entitys.STATE == "0")//禁用用户
                    //{
                    //    var roleids = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER")
                    //                  where ent.T_SYS_USER.SYSUSERID == UserObj.SYSUSERID
                    //                  select ent;
                    //    if (roleids.Count() > 0)
                    //    {
                    //        //if (DeleteRoleAndPermissonForUpdate(entitys.SYSUSERID))
                    //        //{
                    //            if (dal.Update(entitys) > 0)
                    //            {
                    //                dal.CommitTransaction();
                    //                return true;
                    //            }
                    //        //}

                    //    }
                    //    else
                    //    {
                    //        entitys.UPDATEDATE = System.DateTime.Now;
                    //        i = dal.Update(entitys);
                    //        if (i > 0)
                    //        {
                    //            dal.CommitTransaction();

                    //            return true;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    entitys.UPDATEDATE = System.DateTime.Now;
                    i = dal.Update(entitys);
                    if (i > 0)
                    {
                        try
                        {
                            string useMail = System.Configuration.ConfigurationManager.AppSettings["CompanyHasMail"].ToString();
                            if (useMail == "1")
                            {
                                //添加方法同时修改邮件用户密码
                                changeMailPassword(entitys);
                            }
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("获取是否使用邮箱是出错：" + ex.ToString());
                        }                        
                        //dal.CommitTransaction();
                        return true;
                    }
                    //}
                }
                return false;
            }
            catch (Exception ex)
            {
                //dal.RollbackTransaction();
                Tracer.Debug("系统用户SysUserBLL-UpdateSysUserInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        /// <summary>
        /// Added by luojie
        /// 更改邮件密码的接口
        /// </summary>
        /// <param name="entitys">T_SYS_USER实体</param>
        /// <returns>是否保存成功</returns>
        private bool changeMailPassword(T_SYS_USER entitys)
        {
            bool result=true;
            //using (MailWS.MailServiceClient mailClient = new MailWS.MailServiceClient())
            //{ 
            //    try
            //    {
            //        MailWS.HREmployee user = new MailWS.HREmployee();
            //        //解密
            //        SMT.SaaS.SmtOlineEn.SmtOlineDES des = new SmtOlineDES();
            //        if (entitys != null && entitys.STATE=="1")
            //        {
            //            //获取更改的用户的id
            //            user.EmployeeId = entitys.EMPLOYEEID;
            //            user.Password = des.getValue(entitys.PASSWORD);
                        
            //            List<MailWS.HREmployee> users = new List<MailWS.HREmployee>();
            //            users.Add(user);
            //            //储存明文
            //            Tracer.Debug("修改邮件密码-用户：" + user.EmployeeId + " 密码:" + entitys.PASSWORD);
            //            if (users != null) mailClient.ChangePassword(users.ToArray());
            //        }
            //        else
            //        {
            //            Tracer.Debug("未成功修改邮件密码");
            //            result = false;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Tracer.Debug("修改邮件密码出错：" + ex.ToString());
            //        return false;
            //    }
            //}
            return result;
        }

        /// <summary>
        /// 修改系统用户信息
        /// 供离职确认时调用
        /// </summary>
        /// <param name="UserObj"></param>
        /// <returns></returns>
        public bool UpdateSysUserInfoForEmployeeLeftOffice(T_SYS_USER UserObj, string StrOwnerCompanyid, string StrPostid, bool IsMain)
        {
            bool IsResult = false;

            try
            {
                string keyString = "GetUserByEmployeeID" + UserObj.EMPLOYEEID;
                WCFCache.Current[keyString] = null;
                //清空即时通讯的缓存
                string InstantkeyString = "ImInstantLoginUsers";
                WCFCache.Current[InstantkeyString] = null;


                Tracer.Debug("离职确认调用UpdateSysUserInfoForEmployeeLeftOffice" + System.DateTime.Now.ToString() + " 系统用户ID:" + UserObj.SYSUSERID.ToString());
                Tracer.Debug("离职确认调用UpdateSysUserInfoForEmployeeLeftOffice" + System.DateTime.Now.ToString() + " 公司ID:" + StrOwnerCompanyid);
                Tracer.Debug("离职确认调用UpdateSysUserInfoForEmployeeLeftOffice" + System.DateTime.Now.ToString() + " 岗位ID:" + StrPostid);

                var entity = from ent in dal.GetTable()
                             where ent.SYSUSERID == UserObj.SYSUSERID
                             select ent;
                #region 员工是否存在
                //如果是主职离职，则去除所有的角色
                //并禁用用户状态
                if (IsMain)
                {
                    Tracer.Debug("离职确认调用UpdateSysUserInfoForEmployeeLeftOffice,主岗位离职。");
                   IsResult =  this.UpdateSysUserInfo(UserObj);
                }
                else
                {
                    if (entity.Count() > 0)
                    {
                        var entitys = entity.FirstOrDefault();
                        Utility.CloneEntity<T_SYS_USER>(UserObj, entitys);
                        dal.BeginTransaction();
                        int i = 0;
                        if (UserObj.STATE == "0")//禁用用户
                        {
                            #region 记录日志

                            Tracer.Debug("离职确认调用UpdateSysUserInfoForEmployeeLeftOffice" + System.DateTime.Now.ToString() + " 用户状态:" + entitys.STATE);
                            Tracer.Debug("离职确认调用UpdateSysUserInfoForEmployeeLeftOffice" + System.DateTime.Now.ToString() + " 用户状态:" + entitys.STATE);
                            Tracer.Debug("离职员工：" + UserObj.EMPLOYEENAME + " " + System.DateTime.Now.ToString() + " 用户状态:" + entitys.STATE);
                            Tracer.Debug("离职确认调用OWNERCOMPANYID" + System.DateTime.Now.ToString() + " 公司ID:" + StrOwnerCompanyid);
                            Tracer.Debug("离职确认调用EMPLOYEEPOSTID" + System.DateTime.Now.ToString() + " 岗位ID:" + StrPostid);

                            #endregion

                            var roleids = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER")
                                          where ent.T_SYS_USER.SYSUSERID == UserObj.SYSUSERID
                                          && ent.OWNERCOMPANYID == StrOwnerCompanyid
                                          && ent.EMPLOYEEPOSTID == StrPostid
                                          select ent;
                            if (roleids.Count() > 0)
                            {

                                Tracer.Debug("离职确认调用UpdateSysUserInfoForEmployeeLeftOffice,离职岗位有角色。");
                                foreach (var obj in roleids)
                                {
                                    Tracer.Debug("离职确认时删除了角色：" + obj.T_SYS_ROLE.ROLENAME + " 角色ID:" + obj.T_SYS_ROLE.ROLEID + System.DateTime.Now.ToString());
                                    //删除用户角色
                                    //存储区更新、插入或删除语句影响到了意外的行数({0})。实体在加载后可能被修改或删除。刷新 ObjectStateManager 项。
                                    if (obj.EntityKey != null)
                                    {
                                        dal.DeleteFromContext(obj);
                                    }
                                }
                                //保存用户角色的删除
                                int k = dal.SaveContextChanges();
                                #region 判断是否保存成功

                                if (k > 0)
                                {
                                    dal.CommitTransaction();
                                    IsResult = true;
                                    
                                }
                                else
                                {
                                    dal.RollbackTransaction();
                                    Tracer.Debug("离职确认调用UpdateSysUserInfoForEmployeeLeftOffice,离职确认删除角色失败。" + System.DateTime.Now.ToString());
                                }

                                #endregion


                            }

                        }

                    }
                }

                #endregion
                
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("系统用户SysUserBLL-UpdateSysUserInfoForEmployeeLeftOffice" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsResult = false;

            }
            return IsResult;
        }

        /// <summary>
        /// 修改系统用户密码信息
        /// </summary>
        /// <param name="UserObj"></param>
        /// <returns></returns>
        public bool UpdateSysUserPasswordByUsername(string sysuserid,string username,string pwd)
        {

            try
            {
                //var users = from ent in dal.GetObjects<T_SYS_USER>()
                //            where ent.USERNAME == UserObj.USERNAME
                //            select ent;
                var entity = from ent in dal.GetTable()
                             where ent.EMPLOYEEID == sysuserid
                             select ent;

                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();
                    entitys.PASSWORD = pwd;
                    entitys.UPDATEDATE = System.DateTime.Now;
                    
                    int i = 0;
                    
                    i = dal.Update(entitys);
                    if (i > 0)
                    {
                        try
                        {
                            string useMail = System.Configuration.ConfigurationManager.AppSettings["CompanyHasMail"].ToString();
                            if (useMail == "1")
                            {
                                //添加方法同时修改邮件用户密码
                                changeMailPassword(entitys);
                            }
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("获取是否使用邮箱是出错：" + ex.ToString());
                        }
                        return true;
                    }
                    
                }
                return false;
            }
            catch (Exception ex)
            {
                
                Tracer.Debug("系统用户SysUserBLL-UpdateSysUserPasswordByUsername" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;

            }
        }

        /// <summary>
        /// 当用户被禁用时删除 其对应的角色
        /// 权限、自定义权限
        /// </summary>
        /// <param name="userid"></param>
        private bool DeleteRoleAndPermisson(string userid)
        {
            bool IsTrue = true;
            try
            {
                //用户角色
                var roleids = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                              where ent.T_SYS_USER.SYSUSERID == userid
                              select ent;
                int delpermission = 0;
                dal.BeginTransaction();
                if (roleids.Count() > 0)
                {
                    foreach (var obj in roleids)
                    {
                        var roleentitys = from ent in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE")
                                          where ent.T_SYS_ROLE.ROLEID == obj.T_SYS_ROLE.ROLEID
                                          select ent;
                        if (roleentitys.Count() > 0)
                        {
                            foreach (var roleentity in roleentitys)
                            {
                                var rolepermissions = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")
                                                      where ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == roleentity.ROLEENTITYMENUID
                                                      select ent;
                                if (rolepermissions.Count() > 0)
                                {
                                    foreach (var rolepermission in rolepermissions)
                                    {
                                        //删除角色菜单权限
                                        dal.DeleteFromContext(rolepermission);
                                    }
                                    delpermission = dal.SaveContextChanges();
                                }
                                //删除角色菜单信息
                                dal.DeleteFromContext(roleentity);

                            }
                            //保存角色菜单的删除
                            dal.SaveContextChanges();

                        }
                        //删除用户角色
                        dal.DeleteFromContext(obj);
                    }
                    //保存用户角色的删除
                    dal.SaveContextChanges();
                    dal.CommitTransaction();
                }
                else
                {
                    IsTrue = false;
                }
                //string roleid="";
                ////角色菜单
                //var roleentitys = from ent in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE")
                //                  where ent.T_SYS_ROLE.ROLEID == roleid
                //                  select ent;
                //string roleentityid="";
                ////角色菜单权限
                //var rolepermissions = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("")
                //                      where ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == roleentityid
                //                      select ent;
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("系统用户SysUserBLL-DeleteRoleAndPermisson" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsTrue = false;
                
            }
            return IsTrue;

        }

        /// <summary>
        /// 当用户被禁用时删除 其对应的角色
        /// 权限、自定义权限
        /// </summary>
        /// <param name="userid"></param>
        private bool DeleteRoleAndPermissonForUpdate(string userid)
        {
            bool IsTrue = true;
            try
            {
                //用户角色
                var roleids = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                              where ent.T_SYS_USER.SYSUSERID == userid
                              select ent;
                int delpermission = 0;

                if (roleids.Count() > 0)
                {
                    //只删除用户角色   角色对应的权限不删除
                    foreach (var obj in roleids)
                    {
                        //var roleentitys = from ent in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE")
                        //                  where ent.T_SYS_ROLE.ROLEID == obj.T_SYS_ROLE.ROLEID
                        //                  select ent;
                        //if (roleentitys.Count() > 0)
                        //{

                        //    //foreach (var roleentity in roleentitys)
                        //    //{
                        //    //    var rolepermissions = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")
                        //    //                          where ent.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == roleentity.ROLEENTITYMENUID
                        //    //                          select ent;
                        //    //    if (rolepermissions.Count() > 0)
                        //    //    {
                        //    //        foreach (var rolepermission in rolepermissions)
                        //    //        {
                        //    //            //删除角色菜单权限
                        //    //            dal.DeleteFromContext(rolepermission);
                        //    //        }
                        //    //        delpermission = dal.SaveContextChanges();
                        //    //    }
                        //    //    //删除角色菜单信息
                        //    //    dal.DeleteFromContext(roleentity);

                        //    //}
                        //    //保存角色菜单的删除
                        //    dal.SaveContextChanges();

                        //}
                        //删除用户角色
                        //存储区更新、插入或删除语句影响到了意外的行数({0})。实体在加载后可能被修改或删除。刷新 ObjectStateManager 项。
                        if (obj.EntityKey != null)
                        {
                            dal.DeleteFromContext(obj);
                        }
                    }
                    //保存用户角色的删除
                    dal.SaveContextChanges();

                }
                else
                {
                    IsTrue = false;
                }

            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("系统用户SysUserBLL-DeleteRoleAndPermissonForUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsTrue = false;
                
            }
            return IsTrue;

        }



        /// <summary>
        /// 当用户被禁用时删除 其对应的角色
        /// 权限、自定义权限
        /// </summary>
        /// <param name="userid"></param>
        private bool DeleteRoleAndPermissonForUpdateTOLeftOffice(string userid)
        {
            bool IsTrue = true;
            try
            {
                //用户角色
                var roleids = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                              where ent.T_SYS_USER.SYSUSERID == userid
                              select ent;
                int delpermission = 0;

                if (roleids.Count() > 0)
                {
                    //只删除用户角色   角色对应的权限不删除
                    foreach (var obj in roleids)
                    {
                        
                        //删除用户角色
                        //存储区更新、插入或删除语句影响到了意外的行数({0})。实体在加载后可能被修改或删除。刷新 ObjectStateManager 项。
                        if (obj.EntityKey != null)
                        {
                            dal.DeleteFromContext(obj);
                        }
                    }
                    //保存用户角色的删除
                    dal.SaveContextChanges();

                }
                else
                {
                    IsTrue = false;
                }

            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("系统用户SysUserBLL-DeleteRoleAndPermissonForUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsTrue = false;

            }
            return IsTrue;

        }


        /// <summary>
        /// 查询是否存在相同的用户名
        /// </summary>
        /// <param name="StrName"></param>
        /// <returns></returns>
        public bool GetSysUserInfoByAdd(string StrUserName)
        {
            bool IsExist = false;
            try
            {
                var q = from ent in dal.GetTable()
                        where ent.USERNAME == StrUserName
                        select ent;
                if (q.Count() > 0)
                {

                    IsExist = true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetSysUserInfoByAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsExist = false;
            }
            return IsExist;
        }

        /// <summary>
        ///  离职后再入职 不用新建  只需修改
        /// </summary>
        /// <param name="user">新系统用户信息</param>
        /// <param name="strMsg">回传信息</param>
        public void SysUserInfoAddORUpdate(T_SYS_USER user, ref string strMsg)
        {
            try
            {
                dal.BeginTransaction();
                var q = from ent in dal.GetTable()
                        where ent.USERNAME == user.USERNAME && ent.EMPLOYEEID != user.EMPLOYEEID
                        select ent;
                if (q.Count() > 0)
                {
                    strMsg = "REPEAT";
                    return;
                }

                var users = from c in dal.GetTable()
                            where c.EMPLOYEEID == user.EMPLOYEEID
                            select c;
                if (users.Count() > 0)
                {
                    T_SYS_USER tmp = users.FirstOrDefault();
                    tmp.PASSWORD = user.PASSWORD;
                    tmp.USERNAME = user.USERNAME;
                    tmp.UPDATEDATE = DateTime.Now;
                    tmp.EMPLOYEENAME = user.EMPLOYEENAME;
                    tmp.ISENGINEMANAGER = user.ISENGINEMANAGER;
                    tmp.ISFLOWMANAGER = user.ISFLOWMANAGER;
                    tmp.ISMANGER = user.ISMANGER;
                    tmp.REMARK = user.REMARK;
                    tmp.STATE = user.STATE;
                    tmp.OWNERCOMPANYID = user.OWNERCOMPANYID;
                    tmp.OWNERDEPARTMENTID = user.OWNERDEPARTMENTID;
                    tmp.OWNERPOSTID = user.OWNERPOSTID;
                    tmp.OWNERID = user.OWNERID;
                    dal.UpdateFromContext(tmp);
                }
                else
                {
                    dal.AddToContext(user);
                }
                dal.SaveContextChanges();
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("系统用户SysUserBLL-SysUserInfoAddORUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }
        }

        /// <summary>
        /// 判断用户名是否重复  重复的话在后面加+1返回 2011-4-20
        /// </summary>
        /// <param name="user"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public string GetUserNameIsExistAddOne(string UserName, string employeeid)
        {
            string StrReturn = "";
            try
            {

                bool hasReption = true;
                int i = 1;
                while (hasReption)
                {
                    var q = from ent in dal.GetTable()
                            where ent.USERNAME == UserName && ent.EMPLOYEEID != employeeid
                            select ent;


                    if (q.Count() <= 0)
                    {
                        hasReption = false;
                        StrReturn = UserName;
                    }
                    else
                    {
                        StrReturn = UserName + i.ToString();
                        UserName = StrReturn;
                    }
                    i = i + 1;
                }


            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserNameIsExist" + System.DateTime.Now.ToString() + " " + ex.ToString());

            }
            return StrReturn;
        }

        /// <summary>
        /// 获取所有的系统用户信息
        /// </summary>
        /// <returns>返回系统用户列表</returns>
        public List<T_SYS_USER> GetAllSysUserInfos()
        {
            try
            {
                var ents = from ent in dal.GetTable()
                           where ent.STATE == "1"
                           orderby ent.CREATEDATE descending
                           select ent;
                return ents.Count() > 0 ? ents.ToList() : null;

            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetAllSysUserInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                ;
            }
        }
        /// <summary>
        /// 服务器端分页 获取所有用户信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_SYS_USER> GetAllSysUserInfosWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USER>()
                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_USER");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_SYS_USER>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetAllSysUserInfosWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }

        /// <summary>
        /// 服务器端分页 根据公司ID获取用户
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_SYS_USER> GetAllSysUserInfosWithPagingByCompanyIDs(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string[] CompanyIDs)
        {
            try
            {
                var ents = from ent in dal.GetTable() // DataContext.T_SYS_USER

                           select ent;
                ents = ents.Where(p => CompanyIDs.Contains(p.OWNERCOMPANYID));
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_USER");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_SYS_USER>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetAllSysUserInfosWithPagingByCompanyIDs" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }

        //批量删除系统用户信息
        public string BatchDeleteSysUserInfos(string[] ArrSysUserIDs)
        {
            string StrReturn = "";
            try
            {



                foreach (var a in ArrSysUserIDs)
                {
                    var tmpUREnts = (from ent in dal.GetObjects<T_SYS_USERROLE>() // DataContext.T_SYS_USERROLE
                                     where ent.T_SYS_USER.SYSUSERID == a
                                     select ent);
                    if (tmpUREnts.Count() > 0)
                    {
                        //TODO:多语言与自定义异常
                        //throw new Exception("此角色已关联用户，请先删除用户角色关联！");
                        StrReturn = "有角色和用户关联，不能删除";
                        break;
                    }
                    if (string.IsNullOrEmpty(StrReturn))
                    {
                        var entitys = from ent in dal.GetObjects()
                                      where ent.SYSUSERID == a
                                      select ent;

                        if (entitys.Count() > 0)
                        {
                            foreach (var obj in entitys)
                            {
                                Utility.RefreshEntity(obj);
                                //DataContext.DeleteObject(obj);
                                //dal.Delete(obj);
                                dal.DeleteFromContext(obj);
                            }

                        }
                    }
                }
                //DataContext.SaveChanges();
                dal.SaveContextChanges();


                return StrReturn;

            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-BatchDeleteSysUserInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "error";
                
            }
        }

        /// <summary>
        /// 批量更新用户状态 添加了UserState  用来表示是 禁用还是启用
        /// </summary>
        /// <param name="ArrSysUserIDs"></param>
        /// <returns></returns>
        public string BatchUpdateSysUserInfos(string[] ArrSysUserIDs, string StrUserID, string UserName, string UserState)
        {
            string StrReturn = "";
            try
            {

                if (ArrSysUserIDs.Length > 0)
                {

                    var entitys = from ent in dal.GetTable().ToList()
                                  where ArrSysUserIDs.Contains(ent.EMPLOYEEID)
                                  select ent;

                    if (entitys.Count() > 0)
                    {
                        foreach (var obj in entitys)
                        {
                            //dal.Delete(obj);
                            obj.STATE = UserState;
                            //obj.UPDATEDATE = System.DateTime.Now;
                            //obj.UPDATEUSER = StrUserID;
                            //obj.USERNAME = UserName;

                            dal.Update(obj);
                        }

                    }

                }

                return StrReturn;

            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-BatchUpdateSysUserInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "error";
                
            }
        }

        /// <summary>
        /// 根据用户名称得到用户所拥有的权限
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        public IQueryable<V_Permission> GetUserPermissionByUser(string userID)
        {
            try
            {
                //var ents = from p in DataContext.T_SYS_PERMISSION.Include("T_SYS_PERM_MENU")
                //           join rp in DataContext.T_SYS_ROLE_PERM on p.PERMISSIONID equals rp.T_SYS_PERMISSION.PERMISSIONID
                //           join ur in DataContext.T_SYS_USER_ROLE on rp.T_SYS_ROLE.ROLEID equals ur.T_SYS_ROLE.ROLEID
                //           where ur.T_SYS_USER.USERNAME == userName
                //           select rp.T_SYS_PERMISSION;
                //return ents;

                var ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                           join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           where ur.T_SYS_USER.SYSUSERID == userID
                           select new V_Permission
                           {
                               RoleMenuPermission = p,
                               Permission = n,
                               EntityMenu = m
                           };


                //T_SYS_ROLEMENUPERMISSION p = new T_SYS_ROLEMENUPERMISSION();
                //p.T_SYS_PERMISSION.PERMISSIONID

                List<V_Permission> rl = ents.ToList();
                //return null;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserPermissionByUser" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }




        /// <summary>
        /// 根据用户名称得到用户所拥有的权限使用第2套方法  减少 网络流量
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        public IQueryable<V_UserPermissionUI> GetUserPermissionByUserToUI(string userID, string StrMenuID)
        {
            try
            {
                IQueryable<V_UserPermissionUI> ents = null;
                List<V_UserPermissionUI> rl = new List<V_UserPermissionUI>();

                //如果是管理员 这直接返回一个数据
                

                if (string.IsNullOrEmpty(StrMenuID))
                {
                    //供旧版本使用
                    ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                           join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           where ur.T_SYS_USER.SYSUSERID == userID
                           select new V_UserPermissionUI
                           {
                               DataRange = p.DATARANGE,
                               PermissionValue = n.PERMISSIONVALUE,
                               MenuCode = m.MENUCODE,
                               EntityMenuID = m.ENTITYMENUID

                           };
                }
                else
                {
                    //添加了菜单ID过滤  2010-11-10 
                    ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                           join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           where ur.T_SYS_USER.SYSUSERID == userID && m.ENTITYMENUID == StrMenuID
                           select new V_UserPermissionUI
                           {
                               DataRange = p.DATARANGE,
                               PermissionValue = n.PERMISSIONVALUE,
                               MenuCode = m.MENUCODE,
                               EntityMenuID = m.ENTITYMENUID

                           };


                }
                //rl = ents.ToList();
                Tracer.Debug("系统用户SysUserBLL-GetUserPermissionByUserToUI调用我了" + System.DateTime.Now.ToString() + " " + ents.Count());
                if (ents.Count() > 0) rl = ents.ToList();
                //添加自定义权限
                //var custents = (from u in dal.GetObjects<T_SYS_USER>().Include("T_SYS_USERROLE")
                //                join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                //                //join ro in dal.GetObjects<T_SYS_ROLE>()
                //                join ecp in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>() on ur.T_SYS_ROLE.ROLEID equals ecp.T_SYS_ROLE.ROLEID
                //                join p in dal.GetObjects<T_SYS_PERMISSION>() on ecp.T_SYS_PERMISSION.PERMISSIONID equals p.PERMISSIONID
                //                join em in dal.GetObjects<T_SYS_ENTITYMENU>() on ecp.T_SYS_ENTITYMENU.ENTITYMENUID equals em.ENTITYMENUID
                //                where ur.T_SYS_USER.SYSUSERID == userID //&& ecp.T_SYS_ENTITYMENU.ENTITYMENUID == StrMenuID
                //                select new
                //                {
                //                    ecp.T_SYS_PERMISSION.PERMISSIONID,
                //                    ecp.T_SYS_ENTITYMENU.ENTITYMENUID,
                //                    ecp.T_SYS_PERMISSION.PERMISSIONVALUE,
                //                    ecp.COMPANYID,
                //                    ecp.DEPARTMENTID,
                //                    ecp.POSTID
                //                }).Distinct();
                #region 是管理员的情况下 不再进行权限过滤  2011-07-26

                if (!string.IsNullOrEmpty(StrMenuID))
                {
                    string UserPermision = "GetUserPermissionByUserToUI" + userID;
                    T_SYS_USER tmpUser = new T_SYS_USER();
                    if (CacheManagerMem.GetCache(UserPermision) != null)
                    {

                        tmpUser = (T_SYS_USER)CacheManagerMem.GetCache(UserPermision);
                    }
                    else
                    {
                        var TmpUserEnts = from a in dal.GetObjects<T_SYS_USER>()
                                          where a.EMPLOYEEID == userID
                                          select a;
                        if (TmpUserEnts != null)
                        {
                            if (TmpUserEnts.Count() > 0)
                            {
                                tmpUser = TmpUserEnts.FirstOrDefault();
                                CacheManagerMem.AddCache(UserPermision, tmpUser);
                            }
                        }


                    }

                    if (tmpUser != null)
                    {

                        V_UserPermissionUI Permission = new V_UserPermissionUI();

                        if (tmpUser.ISMANGER == 1)
                        {

                            Permission.DataRange = "4";
                            Permission.PermissionValue = "1";
                            Permission.MenuCode = "1";
                            Permission.EntityMenuID = StrMenuID;
                        }
                        //流程权限

                        if (tmpUser.ISFLOWMANAGER == "1")
                        {

                            Permission.DataRange = "4";
                            Permission.PermissionValue = "1";
                            Permission.MenuCode = "1";
                            Permission.EntityMenuID = StrMenuID;
                        }
                        //引擎权限

                        if (tmpUser.ISENGINEMANAGER == "1")
                        {

                            Permission.DataRange = "4";
                            Permission.PermissionValue = "1";
                            Permission.MenuCode = "1";
                            Permission.EntityMenuID = StrMenuID;

                        }
                        
                        rl.Add(Permission);

                        //return rl.AsQueryable();

                    }
                }
                #endregion

                var custents = (from u in dal.GetObjects<T_SYS_USER>().Include("T_SYS_USERROLE")
                                join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                                //join ro in dal.GetObjects<T_SYS_ROLE>()
                                join ecp in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>() on ur.T_SYS_ROLE.ROLEID equals ecp.T_SYS_ROLE.ROLEID
                                join p in dal.GetObjects<T_SYS_PERMISSION>() on ecp.T_SYS_PERMISSION.PERMISSIONID equals p.PERMISSIONID
                                join em in dal.GetObjects<T_SYS_ENTITYMENU>() on ecp.T_SYS_ENTITYMENU.ENTITYMENUID equals em.ENTITYMENUID
                                where ur.T_SYS_USER.SYSUSERID == userID && (ecp.T_SYS_ENTITYMENU.ENTITYMENUID == StrMenuID && !string.IsNullOrEmpty(StrMenuID))
                                select new
                                {
                                    ecp.T_SYS_PERMISSION.PERMISSIONID,
                                    ecp.T_SYS_ENTITYMENU.ENTITYMENUID,
                                    ecp.T_SYS_PERMISSION.PERMISSIONVALUE,
                                    ecp.COMPANYID,
                                    ecp.DEPARTMENTID,
                                    ecp.POSTID
                                });
                if (!string.IsNullOrEmpty(StrMenuID))
                    custents = custents.Where(p => p.ENTITYMENUID == StrMenuID);
                foreach (var q in custents)
                {
                    CustomerPermission cp = new CustomerPermission();
                    cp.PermissionValue = new List<PermissionValue>();
                    cp.EntityMenuId = q.ENTITYMENUID;
                    //公司
                    if (!string.IsNullOrEmpty(q.COMPANYID))
                    {
                        PermissionValue pv = new PermissionValue();
                        pv.OrgObjects = new List<OrgObject>();
                        pv.Permission = q.PERMISSIONID;
                        OrgObject org = new OrgObject();
                        org.OrgType = "0";
                        org.OrgID = q.COMPANYID;
                        pv.OrgObjects.Add(org);
                        cp.PermissionValue.Add(pv);
                    }
                    //部门
                    if (!string.IsNullOrEmpty(q.DEPARTMENTID))
                    {
                        PermissionValue pv = new PermissionValue();
                        pv.OrgObjects = new List<OrgObject>();
                        pv.Permission = q.PERMISSIONID;
                        OrgObject org = new OrgObject();
                        org.OrgType = "1";
                        org.OrgID = q.DEPARTMENTID;
                        pv.OrgObjects.Add(org);
                        cp.PermissionValue.Add(pv);
                    }
                    //岗位
                    if (!string.IsNullOrEmpty(q.POSTID))
                    {
                        PermissionValue pv = new PermissionValue();
                        pv.OrgObjects = new List<OrgObject>();
                        pv.Permission = q.PERMISSIONID;
                        OrgObject org = new OrgObject();
                        org.OrgType = "2";
                        org.OrgID = q.POSTID;
                        pv.OrgObjects.Add(org);
                        cp.PermissionValue.Add(pv);
                    }
                    if (ents.Count() == 0)
                    {
                        List<V_UserPermissionUI> ListPermission = new List<V_UserPermissionUI>();
                        V_UserPermissionUI PUI = new V_UserPermissionUI();
                        PUI.CustomerPermission = cp;
                        ListPermission.Add(PUI);
                        ents = ListPermission.AsQueryable<V_UserPermissionUI>();
                    }
                    else
                    {
                        for (int i = 0; i < rl.Count(); i++)
                        {
                            if (rl[i].PermissionValue == q.PERMISSIONVALUE && rl[i].EntityMenuID == q.ENTITYMENUID)
                            {
                                rl[i].CustomerPermission = new CustomerPermission();
                                rl[i].CustomerPermission = cp;
                            }
                        }

                    }

                }


                return rl.AsQueryable();
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserPermissionByUserToUI" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }



        /// <summary>
        /// 根据用户名称得到用户所拥有的权限使用第2套方法  减少 网络流量
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        public IQueryable<V_UserPermissionUI> GetUserPermissionByUserToUINotForFbAdmin(string userID, string StrMenuID)
        {
            try
            {
                IQueryable<V_UserPermissionUI> ents = null;
                List<V_UserPermissionUI> rl = new List<V_UserPermissionUI>();

                //如果是管理员 这直接返回一个数据

                
                if (string.IsNullOrEmpty(StrMenuID))
                {
                    //供旧版本使用
                    ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                           join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           where ur.T_SYS_USER.SYSUSERID == userID
                           select new V_UserPermissionUI
                           {
                               DataRange = p.DATARANGE,
                               PermissionValue = n.PERMISSIONVALUE,
                               MenuCode = m.MENUCODE,
                               EntityMenuID = m.ENTITYMENUID

                           };
                    

                }
                else
                {
                    //添加了菜单ID过滤  2010-11-10 
                    ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                           join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           where ur.T_SYS_USER.SYSUSERID == userID && m.ENTITYMENUID == StrMenuID
                           select new V_UserPermissionUI
                           {
                               DataRange = p.DATARANGE,
                               PermissionValue = n.PERMISSIONVALUE,
                               MenuCode = m.MENUCODE,
                               EntityMenuID = m.ENTITYMENUID

                           };


                }
                //var entss = ents.Where(p => p.EntityMenuID == "709D9380-5405-4429-B047-20100401D255");
                
                string menuIDs = System.Configuration.ConfigurationManager.AppSettings["FbAdminMenus"].ToString();
                string[] ArrIds = menuIDs.Split(',');
                //ents = ents.Where(p => !p.EntityMenuID.Contains(menuIDs));
                ents = ents.Where(p => !ArrIds.Contains(p.EntityMenuID));
                
                if (ents.Count() > 0) rl = ents.ToList();
                
                #region 是管理员的情况下 不再进行权限过滤  2011-07-26

                if (!string.IsNullOrEmpty(StrMenuID))
                {
                    string UserPermision = "GetUserPermissionByUserToUINotForFbAdmin" + userID;
                    T_SYS_USER tmpUser = new T_SYS_USER();
                    if (CacheManagerMem.GetCache(UserPermision) != null)
                    {

                        tmpUser = (T_SYS_USER)CacheManagerMem.GetCache(UserPermision);
                    }
                    else
                    {
                        var TmpUserEnts = from a in dal.GetObjects<T_SYS_USER>()
                                          where a.EMPLOYEEID == userID
                                          select a;
                        if (TmpUserEnts != null)
                        {
                            if (TmpUserEnts.Count() > 0)
                            {
                                tmpUser = TmpUserEnts.FirstOrDefault();
                                CacheManagerMem.AddCache(UserPermision, tmpUser);
                            }
                        }


                    }

                    if (tmpUser != null)
                    {

                        V_UserPermissionUI Permission = new V_UserPermissionUI();

                        if (tmpUser.ISMANGER == 1)
                        {

                            Permission.DataRange = "4";
                            Permission.PermissionValue = "1";
                            Permission.MenuCode = "1";
                            Permission.EntityMenuID = StrMenuID;
                        }
                        //流程权限

                        if (tmpUser.ISFLOWMANAGER == "1")
                        {

                            Permission.DataRange = "4";
                            Permission.PermissionValue = "1";
                            Permission.MenuCode = "1";
                            Permission.EntityMenuID = StrMenuID;
                        }
                        //引擎权限

                        if (tmpUser.ISENGINEMANAGER == "1")
                        {

                            Permission.DataRange = "4";
                            Permission.PermissionValue = "1";
                            Permission.MenuCode = "1";
                            Permission.EntityMenuID = StrMenuID;

                        }
                        rl.Add(Permission);
                        //FbAdminBLL fbBll = new FbAdminBLL();
                        //T_SYS_FBADMIN Fb = fbBll.getFbAdminBySysUserID(tmpUser.SYSUSERID);
                        //if(!menuIDs.Contains(Permission.EntityMenuID))                        
                        //{
                        //    //只有是预算管理员才加载预算字典维护、公司科目维护、部门科目维护三个菜单
                        //    if(Fb != null)
                        //    {
                        //        rl.Add(Permission);
                        //    }
                        //}
                        
                        

                        //return rl.AsQueryable();

                    }
                }
                #endregion

                //var custents = (from u in dal.GetObjects<T_SYS_USER>().Include("T_SYS_USERROLE")
                //                join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                //                //join ro in dal.GetObjects<T_SYS_ROLE>()
                //                join ecp in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>() on ur.T_SYS_ROLE.ROLEID equals ecp.T_SYS_ROLE.ROLEID
                //                join p in dal.GetObjects<T_SYS_PERMISSION>() on ecp.T_SYS_PERMISSION.PERMISSIONID equals p.PERMISSIONID
                //                join em in dal.GetObjects<T_SYS_ENTITYMENU>() on ecp.T_SYS_ENTITYMENU.ENTITYMENUID equals em.ENTITYMENUID
                //                where ur.T_SYS_USER.SYSUSERID == userID && (ecp.T_SYS_ENTITYMENU.ENTITYMENUID == StrMenuID && !string.IsNullOrEmpty(StrMenuID))
                //                select new
                //                {
                //                    ecp.T_SYS_PERMISSION.PERMISSIONID,
                //                    ecp.T_SYS_ENTITYMENU.ENTITYMENUID,
                //                    ecp.T_SYS_PERMISSION.PERMISSIONVALUE,
                //                    ecp.COMPANYID,
                //                    ecp.DEPARTMENTID,
                //                    ecp.POSTID
                //                });
                //if (!string.IsNullOrEmpty(StrMenuID))
                //    custents = custents.Where(p => p.ENTITYMENUID == StrMenuID);
                //foreach (var q in custents)
                //{
                //    CustomerPermission cp = new CustomerPermission();
                //    cp.PermissionValue = new List<PermissionValue>();
                //    cp.EntityMenuId = q.ENTITYMENUID;
                //    //公司
                //    if (!string.IsNullOrEmpty(q.COMPANYID))
                //    {
                //        PermissionValue pv = new PermissionValue();
                //        pv.OrgObjects = new List<OrgObject>();
                //        pv.Permission = q.PERMISSIONID;
                //        OrgObject org = new OrgObject();
                //        org.OrgType = "0";
                //        org.OrgID = q.COMPANYID;
                //        pv.OrgObjects.Add(org);
                //        cp.PermissionValue.Add(pv);
                //    }
                //    //部门
                //    if (!string.IsNullOrEmpty(q.DEPARTMENTID))
                //    {
                //        PermissionValue pv = new PermissionValue();
                //        pv.OrgObjects = new List<OrgObject>();
                //        pv.Permission = q.PERMISSIONID;
                //        OrgObject org = new OrgObject();
                //        org.OrgType = "1";
                //        org.OrgID = q.DEPARTMENTID;
                //        pv.OrgObjects.Add(org);
                //        cp.PermissionValue.Add(pv);
                //    }
                //    //岗位
                //    if (!string.IsNullOrEmpty(q.POSTID))
                //    {
                //        PermissionValue pv = new PermissionValue();
                //        pv.OrgObjects = new List<OrgObject>();
                //        pv.Permission = q.PERMISSIONID;
                //        OrgObject org = new OrgObject();
                //        org.OrgType = "2";
                //        org.OrgID = q.POSTID;
                //        pv.OrgObjects.Add(org);
                //        cp.PermissionValue.Add(pv);
                //    }
                //    if (ents.Count() == 0)
                //    {
                //        List<V_UserPermissionUI> ListPermission = new List<V_UserPermissionUI>();
                //        V_UserPermissionUI PUI = new V_UserPermissionUI();
                //        PUI.CustomerPermission = cp;
                //        ListPermission.Add(PUI);
                //        ents = ListPermission.AsQueryable<V_UserPermissionUI>();
                //    }
                //    else
                //    {
                //        for (int i = 0; i < rl.Count(); i++)
                //        {
                //            if (rl[i].PermissionValue == q.PERMISSIONVALUE && rl[i].EntityMenuID == q.ENTITYMENUID)
                //            {
                //                rl[i].CustomerPermission = new CustomerPermission();
                //                rl[i].CustomerPermission = cp;
                //            }
                //        }

                //    }

                //}


                return rl.AsQueryable();
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserPermissionByUserToUINotForFbAdmin" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据用户名称得到用户所拥有的权限
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        public IQueryable<V_Permission> GetPermissionByRoleID(string RoleID)
        {
            try
            {
                //SysRoleMenuPermBLL roleMenuP = new SysRoleMenuPermBLL();
                //RoleEntityMenuBLL roleMenu = new RoleEntityMenuBLL();
                //SysPermissionBLL permbll = new SysPermissionBLL();
                //SysEntityMenuBLL Entitybll = new SysEntityMenuBLL();
                //SysRoleBLL rolebll = new SysRoleBLL();
                var ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU").Include("T_SYS_PERMISSION")//Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ENTITYMENU").Include("T_SYS_ROLE") on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID

                           where e.T_SYS_ROLE.ROLEID == RoleID
                           select new V_Permission
                           {
                               RoleMenuPermission = p,
                               Permission = n,
                               EntityMenu = m,
                               EntityRole = e,
                               Role = r
                           };

                //DataContext.T_SYS_ROLEMENUPERMISSION.MergeOption = MergeOption.NoTracking;
                //DataContext.T_SYS_ROLEENTITYMENU.MergeOption = MergeOption.NoTracking;
                //DataContext.T_SYS_PERMISSION.MergeOption = MergeOption.NoTracking;
                //DataContext.T_SYS_ENTITYMENU.MergeOption = MergeOption.NoTracking;
                //DataContext.T_SYS_ROLE.MergeOption = MergeOption.NoTracking;
                //T_SYS_ROLEMENUPERMISSION p = new T_SYS_ROLEMENUPERMISSION();
                //p.T_SYS_PERMISSION.PERMISSIONID

                List<V_Permission> rl = ents.ToList();
                //return null;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetPermissionByRoleID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据用户名称得到用户所拥有的权限
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        public IQueryable<V_UserPermissionRoleID> GetPermissionByRoleIDSecond(string RoleID)
        {
            try
            {
                var ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>().Include("T_SYS_ROLE") on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID

                           where e.T_SYS_ROLE.ROLEID == RoleID
                           select new V_UserPermissionRoleID
                           {
                               RoleEntityMenuID = e.ROLEENTITYMENUID,
                               RoleEntityPermissionID = p.ROLEMENUPERMID,
                               PermissionDataRange = p.DATARANGE,
                               EntityMenuID = m.ENTITYMENUID,
                               PermissionID = n.PERMISSIONID,
                               RoleID = r.ROLEID
                           };

                List<V_UserPermissionRoleID> rl = ents.ToList();
                //return null;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetPermissionByRoleIDSecond" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 获取用户菜单的权限范围
        /// </summary>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="userID">用户ID</param>
        /// <returns>权限</returns>
        public IQueryable<V_Permission> GetUserMenuPerms(string menuCode, string userID)
        {
            try
            {
                var emenu = from item in dal.GetObjects<T_SYS_ENTITYMENU>()
                            where item.MENUCODE == menuCode
                            select item;
                if (emenu.Count() == 0)
                {
                    return null;
                }
                var ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in emenu on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                           join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           where ur.T_SYS_USER.SYSUSERID == userID && m.MENUCODE == menuCode
                           select new V_Permission
                           {
                               RoleMenuPermission = p,
                               Permission = n,
                               EntityMenu = m
                           };

                List<V_Permission> rl = ents.ToList();

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserMenuPerms" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取用户菜单的权限范围 返回的是字符串 2010-8-31
        /// </summary>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="userID">用户ID</param>
        /// <returns>权限</returns>
        public IQueryable<V_UserPermission> GetUserMenuPermsByUserPermision(string menuCode, string userID)
        {
            try
            {
                var emenu = from item in dal.GetObjects<T_SYS_ENTITYMENU>()
                            where item.MENUCODE == menuCode
                            select item;
                if (emenu.Count() == 0)
                {
                    return null;
                }
                var ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in emenu on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                           join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           where ur.T_SYS_USER.SYSUSERID == userID && m.MENUCODE == menuCode
                           select new V_UserPermission
                           {
                               RoleMenuPermissionValue = p.DATARANGE,
                               PermissionDataRange = n.PERMISSIONVALUE,
                               EntityMenuID = m.ENTITYMENUID,
                               RoleID = r.ROLEID,
                               EntityRoleID = e.ROLEENTITYMENUID

                           };

                List<V_UserPermission> rl = ents.ToList();

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserMenuPermsByUserPermision" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取用户菜单的权限范围 返回的是字符串  供BLLCOMMONSERVICES调用 2010-9-14
        /// 只返回一个权限范围值
        /// </summary>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="userID">用户ID</param>
        /// <param name="OwnerCompanyIDs"></param>
        /// <param name="OwnerDepartmentIDs"></param>
        /// <param name="OwnerPositionIDs"></param>
        /// <returns></returns>
        public IQueryable<V_BllCommonUserPermission> GetUserMenuPermsByUserPermisionBllCommon(string menuCode, string userID, ref string OwnerCompanyIDs, ref string OwnerDepartmentIDs, ref string OwnerPositionIDs)
        {
            try
            {
                  List<V_BllCommonUserPermission> plist;
                    string keyString = "BllCommonUserMenuPermsstring" + menuCode + userID;
                    string Companykey = "BllOwnerCompanyIDs" + menuCode + userID;
                    string Departmentkey = "BllOwnerDepartmentIDs" + menuCode + userID;
                    string Positionkey = "BllOwnerPositionIDs" + menuCode + userID;
                    if (WCFCache.Current[keyString] != null)
                    {
                        plist = (List<V_BllCommonUserPermission>)WCFCache.Current[keyString];
                        OwnerCompanyIDs = (string)WCFCache.Current[Companykey];
                        OwnerDepartmentIDs = (string)WCFCache.Current[Departmentkey];
                        OwnerPositionIDs = (string)WCFCache.Current[Positionkey];
                        return plist.AsQueryable();
                    }
                    else
                    {



                        var emenu = from item in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                                    where item.MENUCODE == menuCode
                                    select item;
                        if (emenu.Count() == 0)
                        {
                            return null;
                        }

                        var ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                                   join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                                   join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                                   join m in emenu on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                   join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                                   join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                   where ur.T_SYS_USER.SYSUSERID == userID && m.MENUCODE == menuCode
                                   select new V_BllCommonUserPermission
                                   {
                                       PermissionDataRange = n.PERMISSIONVALUE,
                                       RoleMenuPermissionValue = p.DATARANGE,
                                       OwnerCompanyID = r.OWNERCOMPANYID,
                                       OwnerPostID = ur.EMPLOYEEPOSTID,
                                       OwnerDepartmentID = r.OWNERDEPARTMENTID

                                   };

                        SysUserRoleBLL UserRole = new SysUserRoleBLL();
                        List<T_SYS_USERROLE> ListUserRole = new List<T_SYS_USERROLE>();
                        IQueryable<T_SYS_USERROLE> IListUserRole = UserRole.GetSysUserRoleByUser(userID);
                        if (IListUserRole != null)
                        {
                            ListUserRole = IListUserRole.ToList();
                        }
                        string StrUserRole = "";
                        if (ListUserRole.Count() > 0)
                        {
                            //ListUserRole = UserRole.GetSysUserRoleByUser(userID).ToList();
                            foreach (var User in ListUserRole)
                            {
                                if (User.T_SYS_ROLE != null)
                                    StrUserRole += User.T_SYS_ROLE.ROLEID.ToString() + ",";
                            }
                            if (!string.IsNullOrEmpty(StrUserRole))
                            {
                                StrUserRole = StrUserRole.Substring(0, StrUserRole.Length - 1);

                                var Customer = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>().Include("T_SYS_PERMISSION")
                                                  .Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                                               join m in dal.GetObjects<T_SYS_ENTITYMENU>() on ent.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                               join ur in dal.GetObjects<T_SYS_USERROLE>() on ent.T_SYS_ROLE.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                               join USER in dal.GetObjects<T_SYS_USER>() on ur.T_SYS_USER.SYSUSERID equals USER.SYSUSERID
                                               where m.MENUCODE == menuCode && ent.T_SYS_PERMISSION.PERMISSIONVALUE == "3"
                                               && USER.SYSUSERID == userID//默认是查看的自定义权限
                                               select new V_UserRoleCustomerPerm
                                               {
                                                   ENTITYMENUCUSTOMPERM = ent,
                                                   ROLE = ent.T_SYS_ROLE
                                               };

                                //ObjectQuery<T_SYS_USERROLE> aa = dal.GetObjects<T_SYS_USERROLE>();
                                List<T_SYS_ENTITYMENUCUSTOMPERM> CustomerEnts = new List<T_SYS_ENTITYMENUCUSTOMPERM>();
                                if (Customer.Count() > 0)
                                {
                                    List<V_UserRoleCustomerPerm> ListPerm = new List<V_UserRoleCustomerPerm>();
                                    ListPerm = Customer.ToList();

                                    //去掉Contains函数的使用

                                    for (int i = 0; i < ListPerm.Count(); i++)
                                    {
                                        if (StrUserRole.IndexOf(ListPerm[i].ROLE.ROLEID) > -1)
                                        {
                                            CustomerEnts.Add(ListPerm[i].ENTITYMENUCUSTOMPERM);
                                        }
                                    }

                                }


                                if (CustomerEnts.Count() > 0)
                                {
                                    foreach (var ent in CustomerEnts)
                                    {
                                        if (ent == null)
                                            continue;
                                        if (!(string.IsNullOrEmpty(ent.COMPANYID)))
                                        {
                                            if (!(OwnerCompanyIDs.IndexOf(ent.COMPANYID) > -1))
                                                OwnerCompanyIDs += ent.COMPANYID.ToString() + ",";
                                        }
                                        else if (!(string.IsNullOrEmpty(ent.DEPARTMENTID)))
                                        {
                                            if (!(OwnerDepartmentIDs.IndexOf(ent.DEPARTMENTID) > -1))
                                                OwnerDepartmentIDs += ent.DEPARTMENTID.ToString() + ",";
                                        }
                                        else
                                        {
                                            if (!(string.IsNullOrEmpty(ent.POSTID)))
                                            {
                                                if (!(OwnerPositionIDs.IndexOf(ent.POSTID) > -1))
                                                    OwnerPositionIDs += ent.POSTID.ToString() + ",";
                                            }
                                        }
                                    }
                                    ///将相应的字符串截取掉最后的,
                                    if (!(string.IsNullOrEmpty(OwnerCompanyIDs)))
                                        OwnerCompanyIDs = OwnerCompanyIDs.Substring(0, OwnerCompanyIDs.Length - 1);
                                    if (!(string.IsNullOrEmpty(OwnerDepartmentIDs)))
                                        OwnerDepartmentIDs = OwnerDepartmentIDs.Substring(0, OwnerDepartmentIDs.Length - 1);
                                    if (!(string.IsNullOrEmpty(OwnerPositionIDs)))
                                        OwnerPositionIDs = OwnerPositionIDs.Substring(0, OwnerPositionIDs.Length - 1);

                                }

                            }
                        }

                        plist = ents.ToList();
                        WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(1));
                        WCFCache.Current.Insert(Companykey, OwnerCompanyIDs, DateTime.Now.AddMinutes(1));
                        WCFCache.Current.Insert(Departmentkey, OwnerDepartmentIDs, DateTime.Now.AddMinutes(1));
                        WCFCache.Current.Insert(Positionkey, OwnerPositionIDs, DateTime.Now.AddMinutes(1));

                        return ents;
                    }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserMenuPermsByUserPermisionBllCommon" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }



        /// <summary>
        /// 获取用户菜单的权限范围 返回的是字符串  供BLLCOMMONSERVICES调用 2011-9-08
        /// 只返回一个权限范围值
        /// </summary>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="userID">用户ID</param>
        /// <param name="OwnerCompanyIDs"></param>
        /// <param name="OwnerDepartmentIDs"></param>
        /// <param name="OwnerPositionIDs"></param>
        /// <returns></returns>
        public IQueryable<V_BllCommonUserPermission> GetUserMenuPermsByUserPermisionBllCommonAddPermissionValue(string menuCode, string userID, ref string OwnerCompanyIDs, ref string OwnerDepartmentIDs, ref string OwnerPositionIDs, string StrPermissionValue)
        {
            try
            {
                List<V_BllCommonUserPermission> plist;
                string keyString = "BllCommonUserMenuPermsstring" + menuCode + StrPermissionValue + userID;
                string Companykey = "BllOwnerCompanyIDs" + menuCode + StrPermissionValue + userID;
                string Departmentkey = "BllOwnerDepartmentIDs" + menuCode + StrPermissionValue + userID;
                string Positionkey = "BllOwnerPositionIDs" + menuCode + StrPermissionValue + userID;
                if (WCFCache.Current[keyString] != null)
                {

                    plist = (List<V_BllCommonUserPermission>)WCFCache.Current[keyString];
                    OwnerCompanyIDs = (string)WCFCache.Current[Companykey];
                    OwnerDepartmentIDs = (string)WCFCache.Current[Departmentkey];
                    OwnerPositionIDs = (string)WCFCache.Current[Positionkey];

                    return plist.AsQueryable();

                }
                else
                {



                    var emenu = from item in dal.GetObjects<T_SYS_ENTITYMENU>()
                                where item.MENUCODE == menuCode
                                select item;
                    if (emenu.Count() == 0)
                    {
                        return null;
                    }
                    if (string.IsNullOrEmpty(StrPermissionValue))
                    {
                        return null;
                    }
                    var ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                               join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                               join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                               join m in emenu on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                               join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                               join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                               where ur.T_SYS_USER.SYSUSERID == userID && m.MENUCODE == menuCode
                               select new V_BllCommonUserPermission
                               {
                                   PermissionDataRange = n.PERMISSIONVALUE,
                                   RoleMenuPermissionValue = p.DATARANGE,
                                   OwnerCompanyID = r.OWNERCOMPANYID,
                                   OwnerDepartmentID = r.OWNERDEPARTMENTID,
                                   OwnerPostID = r.OWNERPOSTID

                               };
                    SysUserRoleBLL UserRole = new SysUserRoleBLL();
                    List<T_SYS_USERROLE> ListUserRole = new List<T_SYS_USERROLE>();
                    IQueryable<T_SYS_USERROLE> IListUserRole = UserRole.GetSysUserRoleByUser(userID);
                    if (IListUserRole != null)
                    {
                        ListUserRole = IListUserRole.ToList();
                    }
                    string StrUserRole = "";
                    if (ListUserRole.Count() > 0)
                    {

                        foreach (var User in ListUserRole)
                        {
                            if (User.T_SYS_ROLE != null)
                                StrUserRole += User.T_SYS_ROLE.ROLEID.ToString() + ",";
                        }
                        if (!string.IsNullOrEmpty(StrUserRole))
                        {
                            StrUserRole = StrUserRole.Substring(0, StrUserRole.Length - 1);

                            var CustomerEnts = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>().Include("T_SYS_PERMISSION")
                                                .Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                                               join e in dal.GetObjects<T_SYS_ROLE>() on ent.T_SYS_ROLE.ROLEID equals e.ROLEID
                                               join m in dal.GetObjects<T_SYS_ENTITYMENU>() on ent.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                               where m.MENUCODE == menuCode && ent.T_SYS_PERMISSION.PERMISSIONVALUE == StrPermissionValue
                                               select ent;

                            //ObjectQuery<T_SYS_USERROLE> aa = dal.GetObjects<T_SYS_USERROLE>();

                            if (CustomerEnts.Count() > 0)
                                //CustomerEnts = CustomerEnts.Where(p => StrUserRole.Contains(p.T_SYS_ROLE.ROLEID));
                                CustomerEnts = CustomerEnts.Where(p => p.T_SYS_ROLE.ROLEID.Contains(StrUserRole));


                            if (CustomerEnts.Count() > 0)
                            {
                                foreach (var ent in CustomerEnts)
                                {
                                    if (ent == null)
                                        continue;
                                    if (!(string.IsNullOrEmpty(ent.COMPANYID)))
                                    {
                                        if (!(OwnerCompanyIDs.IndexOf(ent.COMPANYID) > -1))
                                            OwnerCompanyIDs += ent.COMPANYID.ToString() + ",";
                                    }
                                    else if (!(string.IsNullOrEmpty(ent.DEPARTMENTID)))
                                    {
                                        if (!(OwnerDepartmentIDs.IndexOf(ent.DEPARTMENTID) > -1))
                                            OwnerDepartmentIDs += ent.DEPARTMENTID.ToString() + ",";
                                    }
                                    else
                                    {
                                        if (!(string.IsNullOrEmpty(ent.POSTID)))
                                        {
                                            if (!(OwnerPositionIDs.IndexOf(ent.POSTID) > -1))
                                                OwnerPositionIDs += ent.POSTID.ToString() + ",";
                                        }
                                    }
                                }
                                ///将相应的字符串截取掉最后的,
                                if (!(string.IsNullOrEmpty(OwnerCompanyIDs)))
                                    OwnerCompanyIDs = OwnerCompanyIDs.Substring(0, OwnerCompanyIDs.Length - 1);
                                if (!(string.IsNullOrEmpty(OwnerDepartmentIDs)))
                                    OwnerDepartmentIDs = OwnerDepartmentIDs.Substring(0, OwnerDepartmentIDs.Length - 1);
                                if (!(string.IsNullOrEmpty(OwnerPositionIDs)))
                                    OwnerPositionIDs = OwnerPositionIDs.Substring(0, OwnerPositionIDs.Length - 1);

                            }

                        }
                    }



                    plist = ents.ToList();
                   
                    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(1));
                    WCFCache.Current.Insert(Companykey, OwnerCompanyIDs, DateTime.Now.AddMinutes(1));
                    WCFCache.Current.Insert(Departmentkey, OwnerDepartmentIDs, DateTime.Now.AddMinutes(1));
                    WCFCache.Current.Insert(Positionkey, OwnerPositionIDs, DateTime.Now.AddMinutes(1));

                    return ents;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserMenuPermsByUserPermisionBllCommon" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 获取用户菜单的权限范围 返回的是字符串  供BLLCOMMONSERVICES调用 2010-9-14
        /// 只返回一个权限范围值
        /// </summary>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="userID">用户ID</param>
        /// <param name="OwnerCompanyIDs"></param>
        /// <param name="OwnerDepartmentIDs"></param>
        /// <param name="OwnerPositionIDs"></param>
        /// <returns></returns>
        public IQueryable<V_BllCommonUserPermission> GetUserMenuPermsByUserPermisionBllCommonToNewFrame(string menuCode, string userID, ref string OwnerCompanyIDs, ref string OwnerDepartmentIDs, ref string OwnerPositionIDs, string SysCode)
        {
            try
            {
                var emenu = from item in dal.GetObjects<T_SYS_ENTITYMENU>()
                            where item.MENUCODE == menuCode && item.SYSTEMTYPE == SysCode
                            select item;
                if (emenu.Count() == 0)
                {
                    return null;
                }
                var ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                           join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                           join m in emenu on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                           join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                           join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           where ur.T_SYS_USER.SYSUSERID == userID && m.MENUCODE == menuCode
                           select new V_BllCommonUserPermission
                           {
                               PermissionDataRange = n.PERMISSIONVALUE,
                               RoleMenuPermissionValue = p.DATARANGE

                           };
                SysUserRoleBLL UserRole = new SysUserRoleBLL();
                List<T_SYS_USERROLE> ListUserRole = new List<T_SYS_USERROLE>();
                
                IQueryable<T_SYS_USERROLE> IlistUserRole = UserRole.GetSysUserRoleByUser(userID);
                if (IlistUserRole != null)
                {
                    ListUserRole = UserRole.GetSysUserRoleByUser(userID).ToList();
                }
                string StrUserRole = "";
                if (ListUserRole.Count() > 0)
                {
                    //ListUserRole = UserRole.GetSysUserRoleByUser(userID).ToList();
                    foreach (var User in ListUserRole)
                    {
                        if (User.T_SYS_ROLE != null)
                            StrUserRole += User.T_SYS_ROLE.ROLEID.ToString() + ",";
                    }
                    if (!string.IsNullOrEmpty(StrUserRole))
                    {
                        StrUserRole = StrUserRole.Substring(0, StrUserRole.Length - 1);


                        var CustomerEnts = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>().Include("T_SYS_ROLE").Include("T_SYS_ENTITYMENU")
                                           join e in dal.GetObjects<T_SYS_ROLE>() on ent.T_SYS_ROLE.ROLEID equals e.ROLEID
                                           join m in dal.GetObjects<T_SYS_ENTITYMENU>() on ent.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                           where m.MENUCODE == menuCode
                                           select ent;
                        if (CustomerEnts.Count() > 0)
                            //CustomerEnts = CustomerEnts.Where(p => StrUserRole.Contains(p.T_SYS_ROLE.ROLEID));
                            CustomerEnts = CustomerEnts.Where(p => p.T_SYS_ROLE.ROLEID.Contains(StrUserRole));


                        if (CustomerEnts.Count() > 0)
                        {
                            foreach (var ent in CustomerEnts)
                            {
                                if (ent == null)
                                    continue;
                                if (!(string.IsNullOrEmpty(ent.COMPANYID)))
                                {
                                    if (!(OwnerCompanyIDs.IndexOf(ent.COMPANYID) > -1))
                                        OwnerCompanyIDs += ent.COMPANYID.ToString() + ",";
                                }
                                else if (!(string.IsNullOrEmpty(ent.DEPARTMENTID)))
                                {
                                    if (!(OwnerDepartmentIDs.IndexOf(ent.DEPARTMENTID) > -1))
                                        OwnerDepartmentIDs += ent.DEPARTMENTID.ToString() + ",";
                                }
                                else
                                {
                                    if (!(string.IsNullOrEmpty(ent.POSTID)))
                                    {
                                        if (!(OwnerPositionIDs.IndexOf(ent.POSTID) > -1))
                                            OwnerPositionIDs += ent.POSTID.ToString() + ",";
                                    }
                                }
                            }
                            ///将相应的字符串截取掉最后的,
                            if (!(string.IsNullOrEmpty(OwnerCompanyIDs)))
                                OwnerCompanyIDs = OwnerCompanyIDs.Substring(0, OwnerCompanyIDs.Length - 1);
                            if (!(string.IsNullOrEmpty(OwnerDepartmentIDs)))
                                OwnerDepartmentIDs = OwnerDepartmentIDs.Substring(0, OwnerDepartmentIDs.Length - 1);
                            if (!(string.IsNullOrEmpty(OwnerPositionIDs)))
                                OwnerPositionIDs = OwnerPositionIDs.Substring(0, OwnerPositionIDs.Length - 1);

                        }

                    }
                }


                var CustomerEnts1 = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>().Include("T_SYS_ROLE")
                                    join e in dal.GetObjects<T_SYS_ROLE>() on ent.T_SYS_ROLE.ROLEID equals e.ROLEID
                                    join m in dal.GetObjects<T_SYS_ENTITYMENU>() on ent.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                    join ur in dal.GetObjects<T_SYS_USERROLE>() on e.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                    join k in dal.GetObjects<T_SYS_USER>() on ur.T_SYS_USER.SYSUSERID equals k.SYSUSERID
                                    where k.SYSUSERID == userID
                                    select ent;


                List<V_BllCommonUserPermission> rl = ents.ToList();

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserMenuPermsByUserPermisionBllCommon" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// LookupEntity接口函数，取出Lookup窗口所需数据
        /// </summary>
        /// <param name="args">查询参数集</param>
        /// <returns>对像集合</returns>
        public System.Data.Objects.DataClasses.EntityObject[] GetLookupData(Dictionary<string, string> args)
        {
            try
            {
                IQueryable<T_SYS_USER> objs = from a in dal.GetTable()
                                              select a;

                return objs.Count() > 0 ? objs.ToArray() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetLookupData" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        public System.Data.Objects.DataClasses.EntityObject[] GetLookupData(Dictionary<string, string> args, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            try
            {
                IQueryable<T_SYS_USER> objs = from a in dal.GetTable()
                                              select a;

                return objs.Count() > 0 ? objs.ToArray() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetLookupData" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 返回用户信息
        /// </summary>
        /// <returns>用户信息列表</returns>
        public IQueryable<T_SYS_USER> GetUserPermissionAll()
        {
            try
            {
                var ents = from a in dal.GetTable()
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserPermissionAll" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 为平台提供登录(手机登录在使用此接口)
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        public SMT.HRM.CustomModel.Permission.V_UserLogin AddUserLoginHis(string UserName, string Pwd, string Ip)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USER>()
                           where ent.USERNAME == UserName && ent.PASSWORD==Pwd && ent.STATE == "1"
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent=ents.FirstOrDefault();
                    SysUserLoginRecordBll UserLoginBll = new SysUserLoginRecordBll();
                    T_SYS_USERLOGINRECORD loginrecord = new T_SYS_USERLOGINRECORD();
                    loginrecord.LOGINRECORDID = System.Guid.NewGuid().ToString();
                    loginrecord.LOGINTIME = System.DateTime.Now.ToLongTimeString();
                    loginrecord.LOGINDATE = System.DateTime.Now;
                    loginrecord.REMARK = "";
                    loginrecord.USERNAME = ent.SYSUSERID;
                    loginrecord.ONLINESTATE = "1";
                    loginrecord.LOGINIP = Ip;
                    loginrecord.LOGINMONTH = System.DateTime.Now.Month;
                    loginrecord.LOGINYEAR = System.DateTime.Now.Year;
                    V_EMPLOYEEDETAIL EmployeDetail = employeeBll.GetEmployeeDetailView(ent.EMPLOYEEID);
                    
                    if (EmployeDetail != null)
                    {
                        if (EmployeDetail.EMPLOYEEPOSTS[0] != null)
                        {
                            loginrecord.OWNERID = EmployeDetail.EMPLOYEEID;
                            loginrecord.OWNERPOSTID = EmployeDetail.EMPLOYEEPOSTS[0].EMPLOYEEPOSTID;
                            loginrecord.OWNERDEPARTMENTID = EmployeDetail.EMPLOYEEPOSTS[0].DepartmentID;
                            loginrecord.OWNERCOMPANYID = EmployeDetail.EMPLOYEEPOSTS[0].CompanyID;
                            loginrecord.OWNERNAME = EmployeDetail.EMPLOYEENAME;
                            loginrecord.OWNERPOSTNAME = EmployeDetail.EMPLOYEEPOSTS[0].PostName;
                            loginrecord.OWNERDEPARTMENTNAME = EmployeDetail.EMPLOYEEPOSTS[0].DepartmentName;
                            loginrecord.OWNERCOMPANYNAME = EmployeDetail.EMPLOYEEPOSTS[0].CompanyName;
                        }else
                        {
                            SMT.Foundation.Log.Tracer.Debug("BLl AddUserLoginHis GetUserInfoByLogin员工岗位信息为空");
                        }
                    }
                    else
                    {
                        SMT.Foundation.Log.Tracer.Debug("BLl AddUserLoginHis GetUserInfoByLogin员工基本信息为空");
                    }
                    if (UserLoginBll.AddUserLoginInfo(loginrecord))
                    {
                        //string Ismanage = ents.FirstOrDefault().ISMANGER.ToString();
                        var users = new SMT.HRM.CustomModel.Permission.V_UserLogin();
                        return users;
                    }
                    else
                    {
                        SMT.Foundation.Log.Tracer.Debug("BLl AddUserLoginHis 添加员工登录历史错误：" + UserName + " 登录IP" + Ip);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserInfoByLogin" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 系统登录
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Pwd"></param>
        /// <returns>0：登录失败，1：管理员登录成功，2：普通用户登录成功</returns>
        public SMT.HRM.CustomModel.Permission.V_UserLogin systemUserLogin(string UserName, string Pwd)
        {
            string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USER>()
                           where ent.USERNAME == UserName && ent.STATE == "1"
                           select ent;
                if (ents.Count() > 0)
                {
                    T_SYS_USER user = ents.FirstOrDefault();
                    SMT.HRM.CustomModel.Permission.V_UserLogin vur = new SMT.HRM.CustomModel.Permission.V_UserLogin();
                    vur.SYSUSERID = user.SYSUSERID;
                    vur.EMPLOYEEID = user.EMPLOYEEID;
                    vur.ISMANAGER = user.ISMANGER.ToString();

                    SMT.SaaS.SmtOlineEn.SmtOlineDES des = new SmtOlineDES();
                    string UserPwd = "";
                    try
                    {
                        UserPwd = des.getValue(user.PASSWORD);
                    }
                    catch (Exception ex)
                    {
                        string logger = "登录用户 " + UserName + " 的密码：" + Pwd + "。\r\n";
                        Tracer.Debug(logger);
                        Tracer.Debug("用户登录密码为:" + user.PASSWORD);
                        Tracer.Debug("des.getValue(employee.PASSWORD)出错:" + ex.ToString());
                    }
                    string UserPwdMD5 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(UserPwd, "MD5");
                   
                    if (UserPwdMD5.ToUpper() == Pwd.ToUpper())
                    {
                        ThreadPool.QueueUserWorkItem(delegate(object o)
                        {
                            Tracer.Debug("员工登录历史:" + UserName + " " + Ip);
                            this.AddUserLoginHis(UserName, Ip);
                        });
                        return vur;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-systemUserLogin" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }



        /// <summary>
        /// 为平台提供登录
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        public SMT.HRM.CustomModel.Permission.V_UserLogin GetUserInfoByLoginToTaken(string UserName, string Pwd, string Ip, string StrTaken)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USER>()
                           where ent.USERNAME == UserName && ent.PASSWORD == Pwd && ent.STATE == "1"
                           select ent;
                if (ents.Count() > 0)
                {

                    SysUserLoginRecordBll UserLoginBll = new SysUserLoginRecordBll();
                    T_SYS_USERLOGINRECORD loginrecord = new T_SYS_USERLOGINRECORD();
                    loginrecord.LOGINRECORDID = System.Guid.NewGuid().ToString();
                    loginrecord.LOGINTIME = System.DateTime.Now.ToLongTimeString();
                    loginrecord.LOGINDATE = System.DateTime.Now;
                    loginrecord.REMARK = "";
                    loginrecord.USERNAME = ents.FirstOrDefault().SYSUSERID;
                    loginrecord.ONLINESTATE = "1";
                    loginrecord.LOGINIP = Ip;

                    if (UserLoginBll.AddUserLoginInfo(loginrecord))
                    {
                        string Ismanage = ents.FirstOrDefault().ISMANGER.ToString();
                        var users = from user in ents
                                    select new SMT.HRM.CustomModel.Permission.V_UserLogin
                                    {
                                        SYSUSERID = user.SYSUSERID,
                                        EMPLOYEEID = user.EMPLOYEEID,
                                        ISMANAGER = Ismanage,
                                        LOGINRECORDID = loginrecord.LOGINRECORDID
                                    };
                        //添加令牌
                        //StrTaken = BLLCommonServices.Utility.CreateToken(ents.FirstOrDefault());
                        return users.FirstOrDefault();
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserInfoByLogin" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }
        /// <summary>
        /// 用户注销
        /// </summary>
        /// <param name="UserID"></param>
        public bool LoginOut(string UserID, string loginRecordid, string Ip)
        {
            bool Istrue = true;
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USERLOGINRECORD>()
                           where ent.LOGINRECORDID == loginRecordid
                           select ent;
                T_SYS_USERLOGINRECORD ent1 = ents.FirstOrDefault();
                ent1.ONLINESTATE = "0";
                //dal.Update(ent1);
                dal.UpdateFromContext(ent1);
                T_SYS_USERLOGINRECORDHIS history = new T_SYS_USERLOGINRECORDHIS();
                history.LOGINRECORDHISID = System.Guid.NewGuid().ToString();
                history.LOGINDATE = System.DateTime.Now;
                history.LOGINTIME = System.DateTime.Now.ToShortTimeString();
                history.REMARK = "";
                history.USERNAME = UserID;
                history.LOGINIP = Ip;
                //dal.Add(history);
                dal.AddToContext(history);
                int i = dal.SaveContextChanges();
                if (!(i > 1))
                {
                    Istrue = false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-LoginOut" + System.DateTime.Now.ToString() + " " + ex.ToString());
                Istrue = false;
            }
            return Istrue;
        }

        /// <summary>
        /// 用户注销
        /// </summary>
        /// <param name="UserID"></param>
        public bool LoginOutConfirm(string UserID, string loginRecordid, string Ip, bool IsLogin)
        {
            bool Istrue = true;
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USERLOGINRECORD>()
                           where ent.LOGINRECORDID == loginRecordid
                           select ent;
                T_SYS_USERLOGINRECORD ent1 = ents.FirstOrDefault();
                if (IsLogin)
                {
                    ent1.ONLINESTATE = "1";
                }
                else
                {
                    ent1.ONLINESTATE = "0";
                    //dal.Update(ent1);
                    dal.UpdateFromContext(ent1);
                    T_SYS_USERLOGINRECORDHIS history = new T_SYS_USERLOGINRECORDHIS();
                    history.LOGINRECORDHISID = System.Guid.NewGuid().ToString();
                    history.LOGINDATE = System.DateTime.Now;
                    history.LOGINTIME = System.DateTime.Now.ToShortTimeString();
                    history.REMARK = "";
                    history.USERNAME = UserID;
                    history.LOGINIP = Ip;
                    //dal.Add(history);
                    dal.AddToContext(history);
                }

                int i = dal.SaveContextChanges();
                if (!(i > 1))
                {
                    Istrue = false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-LoginOut" + System.DateTime.Now.ToString() + " " + ex.ToString());
                Istrue = false;
            }
            return Istrue;
        }

        /// <summary>
        /// 为平台提供登录
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        public T_SYS_USER AddUserLoginHis(string UserName, string Ip)
        {
            try
            {
                using (CommDAL<T_SYS_USERLOGINRECORD> urdal = new CommDAL<T_SYS_USERLOGINRECORD>())
                {
                    var ents = from ent in urdal.GetObjects<T_SYS_USER>()
                               where ent.USERNAME == UserName && ent.STATE == "1"
                               select ent;
                    if (ents == null)
                        return null;

                    if (ents.Count() > 0)
                    {
                        T_SYS_USERLOGINRECORD loginrecord = new T_SYS_USERLOGINRECORD();
                        loginrecord.LOGINRECORDID = System.Guid.NewGuid().ToString();
                        loginrecord.LOGINTIME = System.DateTime.Now.ToLongTimeString();
                        loginrecord.LOGINDATE = System.DateTime.Now;
                        loginrecord.REMARK = "";
                        loginrecord.USERNAME = ents.FirstOrDefault().SYSUSERID;
                        loginrecord.ONLINESTATE = "1";
                        loginrecord.LOGINIP = Ip;
                        loginrecord.LOGINYEAR = System.DateTime.Now.Year;
                        loginrecord.LOGINMONTH = System.DateTime.Now.Month;

                        SMT.Foundation.Log.Tracer.Debug("开始调用员工基本信息");
                        SMT.Foundation.Log.Tracer.Debug("开始调用员工基本信息，开始时间：");
                        V_EMPLOYEEDETAIL EmployeDetail = empPostbll.GetEmployeePostBriefByEmployeeID(ents.FirstOrDefault().EMPLOYEEID);
                        SMT.Foundation.Log.Tracer.Debug("开始调用员工基本信息，结束时间：");
                        if (EmployeDetail != null)
                        {
                            SMT.Foundation.Log.Tracer.Debug("员工基本信息不为空");
                            if (EmployeDetail.EMPLOYEEPOSTS[0] != null)
                            {
                                SMT.Foundation.Log.Tracer.Debug("EmployeDetail.EMPLOYEEPOSTS[0]不为null");
                                loginrecord.OWNERID = EmployeDetail.EMPLOYEEID;
                                loginrecord.OWNERPOSTID = EmployeDetail.EMPLOYEEPOSTS[0].EMPLOYEEPOSTID;
                                loginrecord.OWNERDEPARTMENTID = EmployeDetail.EMPLOYEEPOSTS[0].DepartmentID;
                                loginrecord.OWNERCOMPANYID = EmployeDetail.EMPLOYEEPOSTS[0].CompanyID;
                                loginrecord.OWNERNAME = EmployeDetail.EMPLOYEENAME;
                                loginrecord.OWNERPOSTNAME = EmployeDetail.EMPLOYEEPOSTS[0].PostName;
                                loginrecord.OWNERDEPARTMENTNAME = EmployeDetail.EMPLOYEEPOSTS[0].DepartmentName;
                                loginrecord.OWNERCOMPANYNAME = EmployeDetail.EMPLOYEEPOSTS[0].CompanyName;
                            }
                        }
                        else
                        {
                            SMT.Foundation.Log.Tracer.Debug("员工基本信息为空");
                        }
                        if (urdal.Add(loginrecord) > 0)
                        {
                            return ents.FirstOrDefault();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserLoginInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 必定登录接口
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        public T_SYS_USER GetUserLoginInfo(string userId, string Ip)
        {
            try
            {
                using (CommDAL<T_SYS_USERLOGINRECORD> urdal = new CommDAL<T_SYS_USERLOGINRECORD>())
                {
                    var ents = from ent in urdal.GetObjects<T_SYS_USER>()
                               where ent.SYSUSERID == userId && ent.STATE == "1"
                               select ent;
                    if (ents == null)
                        return null;

                    if (ents.Count() > 0)
                    {
                        T_SYS_USERLOGINRECORD loginrecord = new T_SYS_USERLOGINRECORD();
                        loginrecord.LOGINRECORDID = System.Guid.NewGuid().ToString();
                        loginrecord.LOGINTIME = System.DateTime.Now.ToLongTimeString();
                        loginrecord.LOGINDATE = System.DateTime.Now;
                        loginrecord.REMARK = "";
                        loginrecord.USERNAME = ents.FirstOrDefault().SYSUSERID;
                        loginrecord.ONLINESTATE = "1";
                        loginrecord.LOGINIP = Ip;
                        loginrecord.LOGINYEAR = System.DateTime.Now.Year;
                        loginrecord.LOGINMONTH = System.DateTime.Now.Month;

                        SMT.Foundation.Log.Tracer.Debug("开始调用员工基本信息");
                        SMT.Foundation.Log.Tracer.Debug("开始调用员工基本信息，开始时间：");
                        V_EMPLOYEEDETAIL EmployeDetail = empPostbll.GetEmployeePostBriefByEmployeeID(ents.FirstOrDefault().EMPLOYEEID);
                        SMT.Foundation.Log.Tracer.Debug("开始调用员工基本信息，结束时间：");
                        if (EmployeDetail != null)
                        {
                            SMT.Foundation.Log.Tracer.Debug("员工基本信息不为空");
                            if (EmployeDetail.EMPLOYEEPOSTS[0] != null)
                            {
                                SMT.Foundation.Log.Tracer.Debug("EmployeDetail.EMPLOYEEPOSTS[0]不为null");
                                loginrecord.OWNERID = EmployeDetail.EMPLOYEEID;
                                loginrecord.OWNERPOSTID = EmployeDetail.EMPLOYEEPOSTS[0].EMPLOYEEPOSTID;
                                loginrecord.OWNERDEPARTMENTID = EmployeDetail.EMPLOYEEPOSTS[0].DepartmentID;
                                loginrecord.OWNERCOMPANYID = EmployeDetail.EMPLOYEEPOSTS[0].CompanyID;
                                loginrecord.OWNERNAME = EmployeDetail.EMPLOYEENAME;
                                loginrecord.OWNERPOSTNAME = EmployeDetail.EMPLOYEEPOSTS[0].PostName;
                                loginrecord.OWNERDEPARTMENTNAME = EmployeDetail.EMPLOYEEPOSTS[0].DepartmentName;
                                loginrecord.OWNERCOMPANYNAME = EmployeDetail.EMPLOYEEPOSTS[0].CompanyName;
                            }
                        }
                        else
                        {
                            SMT.Foundation.Log.Tracer.Debug("员工基本信息为空");
                        }
                        if (urdal.Add(loginrecord) > 0)
                        {
                            return ents.FirstOrDefault();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserLoginInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }
        /// <summary>
        /// 返回在线用户信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CompanyIDs"></param>
        /// <returns></returns>
        public List<V_OnlineUser> GetOnLineUsers(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string[] CompanyIDs)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USERLOGINRECORD>()
                           join user in dal.GetObjects<T_SYS_USER>() on ent.USERNAME equals user.SYSUSERID

                           where ent.ONLINESTATE == "1"
                           select new V_OnlineUser
                           {
                               EMPLOYEECODE = user.EMPLOYEECODE,
                               EMPLOYEENAME = user.EMPLOYEENAME,
                               USERNAME = user.USERNAME,
                               SYSUSERID = user.SYSUSERID,
                               EMPLOYEEID = user.EMPLOYEEID,
                               LOGINIP = ent.LOGINIP,
                               ONLINESTATE = ent.ONLINESTATE,
                               LOGINDATE = (DateTime)ent.LOGINDATE

                           };

                return ents == null ? null : ents.ToList();
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetOnLineUsers" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据用户id过滤该用户所拥有的 自定义的 公司或部门ID集合 2011-1-13
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="departmentids"></param>
        /// <returns></returns>
        public List<string> GetCompanyidsAndDepartmentidsByUserid(string userid, ref List<string> departmentids)
        {
            try
            {
                List<string> companyids = new List<string>();
                List<string> departments = new List<string>();
                var roles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER")
                            where ent.T_SYS_USER.SYSUSERID == userid
                            select ent;
                if (roles.Count() > 0)
                {
                    List<string> roleids = new List<string>();
                    roles.ToList().ForEach(item =>
                    {
                        roleids.Add(item.T_SYS_ROLE.ROLEID);
                    });
                    if (roleids.Count() > 0)
                    {
                        var ents = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>().Include("T_SYS_ROLE")
                                   where roleids.Contains(ent.T_SYS_ROLE.ROLEID)
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            ents.ToList().ForEach(
                                item =>
                                {
                                    companyids.Add(item.COMPANYID);
                                    departments.Add(item.DEPARTMENTID);
                                }
                                );
                            departmentids = departments;
                            return companyids;
                        }
                    }
                    departmentids = null;
                    return null;

                }
                else
                {
                    departmentids = null;
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetCompanyidsAndDepartmentidsByUserid" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        ///////新框架使用
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserId">用户ID</param>
        /// <param name="MenuCode">菜单的menucode字段值</param>
        /// <param name="queryParas">值列表</param>
        /// <param name="Employee">员工对象</param>
        /// <returns></returns>
        public string GetPermisionDataRangeByUseridAndMenuCode(string UserId, string MenuCode, ref List<object> queryParas, T_HR_EMPLOYEE Employee)
        {
            string StrResult = "";
            string OwnerCompanyIDs = "", OwnerDepartmentIDs = "", OwnerPositionIDs = "";
            IQueryable<V_BllCommonUserPermission> IQlist = this.GetUserMenuPermsByUserPermisionBllCommon(MenuCode, UserId, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);

            return StrResult;
        }


        //新框架调用  2011-4-20
        /// <summary>
        /// 根据系统代码用户ID 菜单名和 操作名来判断是否有操作权限
        /// </summary>
        /// <param name="SysCode">系统名</param>
        /// <param name="userID">用户ID</param>
        /// <param name="MenuCode">menucode</param>
        /// <param name="PermissionValue">操作值</param>
        /// <param name="StrMenuID"></param>        
        public bool GetUserPermissionBySysCodeAndUserAndModelCode(string SysCode, string MenuCode, string userID, string PermissionValue)
        {
            bool IsReturn = true;
            try
            {

                IQueryable<V_UserPermissionUI> ents = null;
                List<V_UserPermissionUI> rl = new List<V_UserPermissionUI>();
                string Menuid = "";//
                var menus = from ent in dal.GetObjects<T_SYS_ENTITYMENU>()
                            where ent.MENUCODE == MenuCode
                            select ent;
                if (menus == null)
                    return false;
                if (menus.Count() > 0)
                    Menuid = menus.FirstOrDefault().ENTITYMENUID;
                if (string.IsNullOrEmpty(Menuid))
                    return false;
                //添加了菜单ID过滤  2010-11-10 
                ents = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                       join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                       join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                       join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                       join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                       join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                       //where ur.T_SYS_USER.SYSUSERID == userID && m.MENUCODE == MenuCode && m.SYSTEMTYPE == SysCode
                       where ur.T_SYS_USER.EMPLOYEEID == userID && m.ENTITYMENUID == Menuid && m.SYSTEMTYPE == SysCode
                       select new V_UserPermissionUI
                       {
                           DataRange = p.DATARANGE,
                           PermissionValue = n.PERMISSIONVALUE,
                           MenuCode = m.MENUCODE,
                           EntityMenuID = m.ENTITYMENUID

                       };



                //rl = ents.ToList();
                if (ents.Count() > 0) rl = ents.ToList();


                var custents = (from u in dal.GetObjects<T_SYS_USER>().Include("T_SYS_USERROLE")
                                join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                                //join ro in dal.GetObjects<T_SYS_ROLE>()
                                join ecp in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>() on ur.T_SYS_ROLE.ROLEID equals ecp.T_SYS_ROLE.ROLEID
                                join p in dal.GetObjects<T_SYS_PERMISSION>() on ecp.T_SYS_PERMISSION.PERMISSIONID equals p.PERMISSIONID
                                join em in dal.GetObjects<T_SYS_ENTITYMENU>() on ecp.T_SYS_ENTITYMENU.ENTITYMENUID equals em.ENTITYMENUID
                                where ur.T_SYS_USER.SYSUSERID == userID && (ecp.T_SYS_ENTITYMENU.ENTITYMENUID == Menuid && !string.IsNullOrEmpty(Menuid))
                                select new
                                {
                                    ecp.T_SYS_PERMISSION.PERMISSIONID,
                                    ecp.T_SYS_ENTITYMENU.ENTITYMENUID,
                                    ecp.T_SYS_PERMISSION.PERMISSIONVALUE,
                                    ecp.COMPANYID,
                                    ecp.DEPARTMENTID,
                                    ecp.POSTID

                                });
                if (!string.IsNullOrEmpty(MenuCode))
                    custents = custents.Where(p => p.ENTITYMENUID == Menuid);
                foreach (var q in custents)
                {
                    CustomerPermission cp = new CustomerPermission();
                    cp.PermissionValue = new List<PermissionValue>();
                    cp.EntityMenuId = q.ENTITYMENUID;
                    //公司
                    if (!string.IsNullOrEmpty(q.COMPANYID))
                    {
                        PermissionValue pv = new PermissionValue();
                        pv.OrgObjects = new List<OrgObject>();
                        pv.Permission = q.PERMISSIONID;
                        OrgObject org = new OrgObject();
                        org.OrgType = "0";
                        org.OrgID = q.COMPANYID;
                        pv.OrgObjects.Add(org);
                        cp.PermissionValue.Add(pv);
                    }
                    //部门
                    if (!string.IsNullOrEmpty(q.DEPARTMENTID))
                    {
                        PermissionValue pv = new PermissionValue();
                        pv.OrgObjects = new List<OrgObject>();
                        pv.Permission = q.PERMISSIONID;
                        OrgObject org = new OrgObject();
                        org.OrgType = "1";
                        org.OrgID = q.DEPARTMENTID;
                        pv.OrgObjects.Add(org);
                        cp.PermissionValue.Add(pv);
                    }
                    //岗位
                    if (!string.IsNullOrEmpty(q.POSTID))
                    {
                        PermissionValue pv = new PermissionValue();
                        pv.OrgObjects = new List<OrgObject>();
                        pv.Permission = q.PERMISSIONID;
                        OrgObject org = new OrgObject();
                        org.OrgType = "2";
                        org.OrgID = q.POSTID;
                        pv.OrgObjects.Add(org);
                        cp.PermissionValue.Add(pv);
                    }
                    if (ents.Count() == 0)
                    {
                        List<V_UserPermissionUI> ListPermission = new List<V_UserPermissionUI>();
                        V_UserPermissionUI PUI = new V_UserPermissionUI();
                        PUI.CustomerPermission = cp;
                        ListPermission.Add(PUI);
                        ents = ListPermission.AsQueryable<V_UserPermissionUI>();
                    }
                    else
                    {
                        for (int i = 0; i < rl.Count(); i++)
                        {
                            if (rl[i].PermissionValue == q.PERMISSIONVALUE && rl[i].EntityMenuID == q.ENTITYMENUID)
                            {
                                rl[i].CustomerPermission = new CustomerPermission();
                                rl[i].CustomerPermission = cp;
                            }
                        }

                    }

                }

                var entperms = from ent in rl
                               where ent.PermissionValue == PermissionValue
                               select ent;
                if (entperms == null)
                    IsReturn = false;
                if (entperms.Count() == 0)
                    IsReturn = false;

                //return rl.AsQueryable();
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserPermissionBySysCodeAndUserAndModelCode" + System.DateTime.Now.ToString() + " " + ex.ToString());
                IsReturn = false;
            }

            return IsReturn;

        }


        /// <summary>
        /// 新框架使用
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="queryParas"></param>
        /// <param name="employeeID"></param>
        /// <param name="menuCode"></param>
        /// <param name="SysCode"></param>
        public void SetOrganizationFilterToNewFrame(ref string filterString, ref List<object> queryParas, string employeeID, string menuCode, string SysCode)
        {
            //注意，用户总是能看到自己创建的记录

            //获取正常的角色用户权限            
            try
            {
                int maxPerm = -1;
                //PersonnelServiceClient client = new PersonnelServiceClient();

                T_HR_EMPLOYEE CachePerson = employeeBll.GetEmployeeByID(employeeID);
                T_SYS_USER CacheUser = GetUserByEmployeeID(employeeID);
                string userID = "";
                if (CachePerson == null)
                    return;
                if (CacheUser == null)
                    return;
                userID = CacheUser.SYSUSERID;
                string OwnerCompanyIDs = "";
                string OwnerDepartmentIDs = "";
                string OwnerPositionIDs = "";

                List<V_BllCommonUserPermission> perms = GetUserMenuPermsByUserPermisionBllCommonToNewFrame(menuCode, userID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs, SysCode).ToList();
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

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    queryParas.Add(employeeID);
                    return;
                }
                else
                {

                    var tmpperms = perms.Where(p => p.PermissionDataRange == "3").ToList();
                    if (tmpperms.Count > 0)
                        maxPerm = tmpperms.Min(p => Convert.ToInt32(p.RoleMenuPermissionValue));
                }



                //取员工岗位                
                V_EMPLOYEEPOST emppost = employeeBll.GetEmployeeDetailByID(CachePerson.EMPLOYEEID);
                foreach (var item in emppost.EMPLOYEEPOSTS)
                {
                    CachePerson.T_HR_EMPLOYEEPOST.Add(item);
                }

                //获取自定义权限  20100914注释  目前没使用自定义权限 
                //int custPerm = GetCustomPerms(entityName, CachePerson);
                //if (custPerm < maxPerm)
                //    maxPerm = custPerm;

                //看整个集团的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Organize))
                {
                    return;
                }
                //看整个公司的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Company))
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
                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    queryParas.Add(employeeID);

                }
                //看部门的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Department))
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

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    queryParas.Add(employeeID);
                }
                //看岗位的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Post))
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
                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    queryParas.Add(employeeID);
                }
                //看员工
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Employee))
                {
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " AND ";

                    filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
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
            catch (Exception ex)
            {
                //有异常只返回自己创建的或属于自己的信息
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " AND ";

                filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                queryParas.Add(employeeID);

                filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                queryParas.Add(employeeID);

            }

        }

        /// <summary>
        /// 判断用户操作的权限范围  是否可以修改、删除某一条记录
        /// </summary>
        /// <param name="perm"></param>
        /// <param name="StrEmployeeid"></param>
        /// <returns></returns>
        public bool GetUserOperationPermission(EntityPermission operation, string employeeID)
        {
            bool IsResult = false;
            try
            {
                int maxPerm = -1;
                //PersonnelServiceClient client = new PersonnelServiceClient();

                T_HR_EMPLOYEE CachePerson = employeeBll.GetEmployeeByID(employeeID);
                T_SYS_USER CacheUser = GetUserByEmployeeID(employeeID);
                string userID = "";
                if (CachePerson == null)
                    return IsResult;
                if (CacheUser == null)
                    return IsResult;
                userID = CacheUser.SYSUSERID;
                string OwnerCompanyIDs = "";
                string OwnerDepartmentIDs = "";
                string OwnerPositionIDs = "";

                List<V_BllCommonUserPermission> perms = GetUserMenuPermsByUserPermisionBllCommonToNewFrame(operation.MENUCODE, userID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs, operation.SYSTEMCODE).ToList();
                bool hasPerms = true;
                bool hasCustomerPerms = true;//自定义权限
                if ((perms == null || perms.Count() <= 0) && string.IsNullOrEmpty(OwnerCompanyIDs) && string.IsNullOrEmpty(OwnerDepartmentIDs) && string.IsNullOrEmpty(OwnerPositionIDs))
                {

                    return IsResult;
                }
                else
                {

                    var tmpperms = perms.Where(p => p.PermissionDataRange == "3").ToList();
                    if (tmpperms.Count > 0)
                        maxPerm = tmpperms.Min(p => Convert.ToInt32(p.RoleMenuPermissionValue));
                }



                //取员工岗位
                V_EMPLOYEEPOST emppost = employeeBll.GetEmployeeDetailByID(CachePerson.EMPLOYEEID);
                foreach (var item in emppost.EMPLOYEEPOSTS)
                {
                    CachePerson.T_HR_EMPLOYEEPOST.Add(item);
                }


                //获取自定义权限  20100914注释  目前没使用自定义权限 
                //int custPerm = GetCustomPerms(entityName, CachePerson);
                //if (custPerm < maxPerm)
                //    maxPerm = custPerm;


                //看整个公司的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Company))
                {


                    foreach (T_HR_EMPLOYEEPOST ep in CachePerson.T_HR_EMPLOYEEPOST)
                    {
                        if (ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == operation.OWNERCOMPANYID)
                        {
                            IsResult = true;
                            break;
                        }

                    }

                }
                //看部门的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Department))
                {

                    foreach (T_HR_EMPLOYEEPOST ep in CachePerson.T_HR_EMPLOYEEPOST)
                    {

                        if (ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID == operation.OWNERDEPARTMENTID)
                        {
                            IsResult = true;
                            break;
                        }

                    }

                }
                //看岗位的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Post))
                {

                    foreach (T_HR_EMPLOYEEPOST ep in CachePerson.T_HR_EMPLOYEEPOST)
                    {

                        if (ep.T_HR_POST.POSTID == operation.OWNERPOSTID)
                        {
                            IsResult = true;
                            break;
                        }

                    }

                }
                //看员工
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(Utility.PermissionRange.Employee))
                {
                    if (operation.OWNERID == employeeID)
                    {
                        IsResult = true;

                    }
                }



                //自定义权限存在公司集合
                if (!string.IsNullOrEmpty(OwnerCompanyIDs))
                {

                    string[] ArrCompany = OwnerCompanyIDs.Split(',');
                    if (!string.IsNullOrEmpty(operation.OWNERCOMPANYID))
                    {
                        if (ArrCompany.Count() > 0)
                        {
                            if (Array.IndexOf<string>(ArrCompany, operation.OWNERCOMPANYID) > -1)
                            {
                                IsResult = true;
                            }
                        }
                    }

                }
                //看自定义权限中的部门信息
                if (!string.IsNullOrEmpty(OwnerDepartmentIDs))
                {

                    string[] ArrDepartment = OwnerDepartmentIDs.Split(',');
                    if (!string.IsNullOrEmpty(operation.OWNERDEPARTMENTID))
                    {
                        if (ArrDepartment.Count() > 0)
                        {
                            if (Array.IndexOf<string>(ArrDepartment, operation.OWNERDEPARTMENTID) > -1)
                            {
                                IsResult = true;
                            }
                        }
                    }
                }
                //看自定义权限中岗位的信息
                if (!string.IsNullOrEmpty(OwnerPositionIDs))
                {

                    string[] ArrPosition = OwnerPositionIDs.Split(',');
                    if (!string.IsNullOrEmpty(operation.OWNERPOSTID))
                    {
                        if (ArrPosition.Count() > 0)
                        {
                            if (Array.IndexOf<string>(ArrPosition, operation.OWNERPOSTID) > -1)
                            {
                                IsResult = true;
                            }
                        }
                    }
                }

                return IsResult;
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetUserPermissionBySysCodeAndUserAndModelCode" + System.DateTime.Now.ToString() + " " + ex.ToString());

            }
            return IsResult;
        }

        public void InsertProvince()
        {
            //2012-07-11 注释掉，没有相应的实体
            //var entcountry = from ent in dal.GetObjects<T_SYS_COUNTRY>()
            //                 where ent.COUNTRYNAME == "中国"
            //                 select ent;
            //var entprovinces = from ent in dal.GetObjects<T_SYS_PROVINCE1>()
            //                   select ent;
            
            //var entcity = from ent in dal.GetObjects<T_SYS_CITY1>()
            //              select ent;
            //var entareas = from ent in dal.GetObjects<T_SYS_AREA1>()
            //               select ent;
            //var entdict = from ent in dal.GetObjects<T_SYS_DICTIONARY>()
            //              where ent.DICTIONCATEGORY== "CITY"  
            //              select ent;
            //var entcityss = from ent in dal.GetObjects<T_SYS_PROVINCECITY>()
            //                select ent;
            //#region 省


            //if (entprovinces.Count() > 0)
            //{
            //    for (int i = 0; i < entprovinces.Count(); i++)
            //    {
            //        T_SYS_PROVINCECITY aa = new T_SYS_PROVINCECITY();
            //        aa.PROVINCEID = System.Guid.NewGuid().ToString();
            //        aa.AREANAME = entprovinces.ToList()[i].PROVINCE;

            //        aa.COUNTRYID = entcountry.FirstOrDefault().COUNTRYID;
            //        if (aa.AREANAME.IndexOf('市') > -1)
            //        {
            //            aa.ISPROVINCE = "1";
            //            aa.ISCITY = "1";

            //            string cc = aa.AREANAME.Substring(0, 2).ToString();
            //            var entfinds = from ent in entdict
            //                           where ent.DICTIONARYNAME == cc
            //                           select ent;
            //            if (entfinds.Count() > 0)
            //            {
            //                aa.AREAVALUE = entfinds.FirstOrDefault().DICTIONARYVALUE;
            //            }
            //        }
            //        else
            //        {
            //            aa.ISPROVINCE = "1";
            //            aa.ISCITY = "0";

            //        }
            //        aa.CREATEUSERID = "";
            //        aa.CREATEDATE = System.DateTime.Now;
            //        provinceBll bb = new provinceBll();
            //        string ccc = "";
            //        bb.SysDictionaryAdd(aa, ref ccc);
            //    }


            //}
            //#endregion

            //#region 市区
            //for (int i = 0; i < entcity.Count(); i++)
            //{
            //    T_SYS_PROVINCECITY aa = new T_SYS_PROVINCECITY();
            //    aa.PROVINCEID = System.Guid.NewGuid().ToString();
            //    aa.AREANAME = entcity.ToList()[i].CITY;
            //    string provinceid = entcity.ToList()[i].FATHER.ToString();
            //    var entaa = entprovinces.Where(p=>p.PROVINCEID == provinceid);
            //    var entbb = from ent in dal.GetObjects<T_SYS_PROVINCECITY>()
            //                where ent.AREANAME == entaa.FirstOrDefault().PROVINCE
            //                select ent;

            //    aa.COUNTRYID = entcountry.FirstOrDefault().COUNTRYID;
            //    aa.T_SYS_PROVINCECITY2 = entbb.FirstOrDefault();
                
            //        aa.ISPROVINCE = "0";
            //        aa.ISCITY = "1";

            //        //string cc = aa.AREANAME.Substring(0, 2).ToString();
            //        //var entfinds = from ent in entdict
            //        //               where ent.DICTIONARYNAME == cc
            //        //               select ent;
            //        //if (entfinds.Count() > 0)
            //        //{
            //        //    aa.AREAVALUE = entfinds.FirstOrDefault().DICTIONARYVALUE;
            //        //}
                
                
            //    aa.CREATEUSERID = "";
            //    aa.CREATEDATE = System.DateTime.Now;
            //    provinceBll bb = new provinceBll();
            //    string ccc = "";
            //    bb.SysDictionaryAdd(aa, ref ccc);
            //}
            //#endregion

            //2012-07-11注释：没有相应的表结构
            //#region 县区
            //for (int i = 0; i < entareas.Count(); i++)
            //{
            //    T_SYS_PROVINCECITY aa = new T_SYS_PROVINCECITY();
            //    aa.PROVINCEID = System.Guid.NewGuid().ToString();
            //    aa.AREANAME = entareas.ToList()[i].AREA;
            //    //var entcitys2 = entcityss.Where(p=>p); 
                
            //    string cityid = entareas.ToList()[i].FATHER.ToString();
            //    var entaa = entcity.Where(p => p.CITYID == cityid);
            //    var entbb = from ent in dal.GetObjects<T_SYS_PROVINCECITY>()
            //                where ent.AREANAME == entaa.FirstOrDefault().CITY && ent.ISCITY =="1"
            //                select ent;

            //    if (aa.AREANAME.IndexOf("区") > -1)
            //    {
            //        if (aa.AREANAME == "市辖区")
            //        {
            //            aa.AREANAME = entaa.FirstOrDefault().CITY.ToString();

            //        }
            //        else
            //        {
            //            continue;
            //        }
            //    }
                
            //    aa.COUNTRYID = entcountry.FirstOrDefault().COUNTRYID;
            //    aa.T_SYS_PROVINCECITY2 = entbb.FirstOrDefault();

            //    aa.ISPROVINCE = "0";
            //    aa.ISCITY = "0";
            //    aa.ISCOUNTRYTOWN = "1";

            //    //string cc = aa.AREANAME.Substring(0, 2).ToString();
            //    //var entfinds = from ent in entdict
            //    //               where ent.DICTIONARYNAME == cc
            //    //               select ent;
            //    //if (entfinds.Count() > 0)
            //    //{
            //    //    aa.AREAVALUE = entfinds.FirstOrDefault().DICTIONARYVALUE;
            //    //}


            //    aa.CREATEUSERID = "";
            //    aa.CREATEDATE = System.DateTime.Now;
            //    provinceBll bb = new provinceBll();
            //    string ccc = "";
            //    bb.SysDictionaryAdd(aa, ref ccc);
            //}
            //#endregion


        }


        #region 流程获取员工信息
        /// <summary>
        /// 通过角色ID:(查找所有人员，包括人员所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="roleID">角色ID</param>
        /// <returns></returns>
        public List<FlowUserInfo> GetFlowUserInfoByRoleID(string roleID)
        {
            List<FlowUserInfo> ListUserInfos = new List<FlowUserInfo>();
            try
            {
                

                var Roles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_ROLE").Include("T_SYS_USER")
                                  where ent.T_SYS_ROLE.ROLEID == roleID
                                  select  ent;
                var EmployeeRoles = from ent in Roles
                                    //where ent.T_SYS_ROLE.ROLEID == roleID
                                    select new { ent.T_SYS_USER.EMPLOYEEID, ent.T_SYS_ROLE };
                
                //存在角色对应的员工信息            
                if (EmployeeRoles.Count() > 0)
                {
                    string[] ListEmployeeIDs = EmployeeRoles.Select(ent => ent.EMPLOYEEID).ToArray();
                    List<T_SYS_ROLE> roles = EmployeeRoles.Select(ent => ent.T_SYS_ROLE).ToList();
                        //SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient Personnel = new SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();
                    List<V_FlowUserInfo> ListEmployeePost = new List<V_FlowUserInfo>();// Personnel.GetFlowUserInfoPostBriefByEmployeeID(ListEmployeeIDs);
                        using (EmployeePostBLL bll = new EmployeePostBLL())
                        {
                           // Tracer.Debug("进入GetFlowUserInfoPostBriefByEmployeeID：" + employeeids.Count().ToString());
                            ListEmployeePost = bll.GetFlowUserInfoPostBriefByEmployeeID(ListEmployeeIDs.ToList());
                        }   
                    //判断用户角色所在岗位，此功能暂时关闭
                        var q = from ent in ListEmployeePost
                                select new FlowUserInfo
                                {
                                    CompayID = ent.CompayID,
                                    CompayName = ent.CompayName,
                                    DepartmentID = ent.DepartmentID,
                                    DepartmentName = ent.DepartmentName,
                                    PostID = ent.PostID,
                                    PostName = ent.PostName,
                                    UserID = ent.UserID,
                                    EmployeeName = ent.EmployeeName,
                                    Roles = roles,
                                    IsHead = ent.IsHead,
                                    IsSuperior = ent.IsSuperior,
                                };
                        #region 打印对应的岗位信息
                        Tracer.Debug("***********角色对应的员工信息 开始***************");
                        foreach (var aa in ListEmployeePost)
                        {
                            Tracer.Debug("CompayID =" + aa.CompayID);
                             Tracer.Debug("CompayName ="+ aa.CompayName);
                             Tracer.Debug("       DepartmentID ="+ aa.DepartmentID);
                             Tracer.Debug("      DepartmentName ="+ aa.DepartmentName);
                             Tracer.Debug("     PostID = "+aa.PostID);
                             Tracer.Debug("     PostName ="+ aa.PostName);
                             Tracer.Debug("       UserID = "+aa.UserID);
                             Tracer.Debug("       EmployeeName ="+ aa.EmployeeName);                                    
                              Tracer.Debug("      IsHead ="+ aa.IsHead);
                              Tracer.Debug("      IsSuperior =" + aa.IsSuperior);
                        }
                        Tracer.Debug("**************角色对应的员工信息 结束*********************");
                        #endregion
                        #region 存在员工岗位信息
                        if (ListEmployeePost.Count() > 0)
                        {
                            foreach (var ent in Roles)
                            {
                                string strDebug = "";
                                if (ent.T_SYS_ROLE != null)
                                {
                                    strDebug ="角色对应的岗位ID:" + ent.POSTID + "对应的角色名称：" + ent.T_SYS_ROLE.ROLENAME;
                                    strDebug += "角色ID:"+ ent.T_SYS_ROLE.ROLEID;
                                }
                                else
                                {
                                    strDebug = "角色对应的岗位ID:" + ent.POSTID;
                                }
                                Tracer.Debug("对应的信息为："+ strDebug);
                                var employeePosts = from em in ListEmployeePost
                                                    where em.PostID == ent.POSTID         
                                                    //公司ID employeePostID 存放的ID为公司ID
                                                    //&& em.CompayID == ent.EMPLOYEEPOSTID 
                                                    select new FlowUserInfo
                                                    {
                                                        CompayID = em.CompayID,
                                                        CompayName = em.CompayName,
                                                        DepartmentID = em.DepartmentID,
                                                        DepartmentName = em.DepartmentName,
                                                        PostID = em.PostID,
                                                        PostName = em.PostName,
                                                        UserID = em.UserID,
                                                        EmployeeName = em.EmployeeName,
                                                        Roles = roles,
                                                        IsHead = em.IsHead,
                                                        IsSuperior = em.IsSuperior,
                                                    };
                                if (!string.IsNullOrEmpty(ent.EMPLOYEEPOSTID))
                                {
                                    employeePosts = employeePosts.Where(s=>s.CompayID == ent.EMPLOYEEPOSTID);
                                }
                                if (employeePosts.Count() > 0)
                                {
                                    foreach (var flowUser in employeePosts)
                                    {
                                        var exist = from ex in ListUserInfos
                                                    where ex.UserID == flowUser.UserID
                                                    && ex.PostID == flowUser.PostID
                                                    select ex;
                                        if (exist.Count() == 0)
                                        {
                                            ListUserInfos.Add(flowUser);
                                        }
                                    }
                                }
                            }
                            #region 注释掉，现在使用角色对应的岗位才显示出来

                            q.ToList().ForEach(s =>
                            {
                                var entUsers = from en1 in ListUserInfos
                                               where en1.UserID == s.UserID
                                               select en1;
                                if (entUsers.Count() == 0)
                                {
                                    var qUsers = from em in q
                                                 where em.UserID == s.UserID
                                                 select em;
                                    qUsers.ToList().ForEach(m =>
                                    {
                                        var exist2 = from ex in ListUserInfos
                                                    where ex.UserID == m.UserID
                                                    && ex.PostID == m.PostID
                                                    select ex;
                                        if (exist2.Count() == 0)
                                        {
                                            ListUserInfos.Add(m);
                                        }
                                    });
                                }
                            });
                            //ListUserInfos = q.ToList();
                            #endregion
                            
                        }
                        #endregion
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("GetFlowUserInfoByRoleID出现错误：" + ex.ToString() + "角色ID：" + roleID);
            }

            return ListUserInfos;
        }
        

        /// <summary>
        /// 获取流程员工信息
        /// </summary>
        /// <param name="ListRoles"></param>
        /// <param name="ListEmployeePost"></param>
        /// <param name="IntDetails"></param>
        /// <param name="IntEmployeePosts"></param>
        /// <returns></returns>
        private static FlowUserInfo SetFlowUserInfo(List<T_SYS_ROLE> ListRoles, V_FlowUserInfo[] ListEmployeePost, int IntDetails)
        {
            FlowUserInfo FlowUser = new FlowUserInfo();
            FlowUser.CompayID = ListEmployeePost[IntDetails].CompayID;
            FlowUser.CompayName = ListEmployeePost[IntDetails].CompayName;
            FlowUser.DepartmentID = ListEmployeePost[IntDetails].DepartmentID;
            FlowUser.DepartmentName = ListEmployeePost[IntDetails].DepartmentName;
            FlowUser.PostID = ListEmployeePost[IntDetails].PostID;
            FlowUser.PostName = ListEmployeePost[IntDetails].PostName;
            FlowUser.UserID = ListEmployeePost[IntDetails].UserID;
            FlowUser.EmployeeName = ListEmployeePost[IntDetails].EmployeeName;
            FlowUser.Roles = ListRoles;
            FlowUser.IsHead = ListEmployeePost[IntDetails].IsHead;
            FlowUser.IsSuperior = ListEmployeePost[IntDetails].IsSuperior;
            return FlowUser;
        }


        /// <summary>
        /// 通过部门ID:(查找部门负责人,包括人员所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="departmentID">部门ID</param>
        /// <returns></returns>
        public List<FlowUserInfo> GetDepartmentHeadByDepartmentID(string departmentID)
        {
            List<FlowUserInfo> ListUserInfos = new List<FlowUserInfo>();
            try
            {
                if (string.IsNullOrEmpty(departmentID))
                {
                    return ListUserInfos;
                }
                Tracer.Debug("开始调用HR中的服务，GetDepartmentHeadByDepartmentID "+ departmentID);
                //SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient Personnel = new SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();
                List<V_FlowUserInfo> ListEmployeePost = new List<V_FlowUserInfo>();// Personnel.GetDepartmentHeadByDepartmentID(departmentID);
                using (EmployeePostBLL bll = new EmployeePostBLL())
                {
                    Tracer.Debug("进入GetDepartmentHeadByDepartmentID：" + departmentID);
                    ListEmployeePost= bll.GetDepartmentHeadByDepartmentID(departmentID);
                }

                if (ListEmployeePost.Count() > 0)
                {
                    if (ListEmployeePost.Count() > 0)
                    {
                        for (int i = 0; i < ListEmployeePost.Count(); i++)
                        {
                            FlowUserInfo FlowUser = new FlowUserInfo();
                            FlowUser.CompayID = ListEmployeePost[i].CompayID;
                            FlowUser.CompayName = ListEmployeePost[i].CompayName;
                            FlowUser.DepartmentID = ListEmployeePost[i].DepartmentID;
                            FlowUser.DepartmentName = ListEmployeePost[i].DepartmentName;
                            FlowUser.PostID = ListEmployeePost[i].PostID;
                            FlowUser.PostName = ListEmployeePost[i].PostName;
                            FlowUser.UserID = ListEmployeePost[i].UserID;
                            FlowUser.EmployeeName = ListEmployeePost[i].EmployeeName;
                            List<T_SYS_ROLE> ListRoles = new List<T_SYS_ROLE>();
                            string StrEmployeeID =ListEmployeePost[i].UserID;
                            var Roles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                                        where ent.T_SYS_USER.EMPLOYEEID == StrEmployeeID
                                        select ent.T_SYS_ROLE;
                            if (Roles.Count() > 0)
                            {
                                ListRoles = Roles.ToList();
                            }
                            FlowUser.Roles = ListRoles;
                            FlowUser.IsHead = ListEmployeePost[i].IsHead;
                            FlowUser.IsSuperior = ListEmployeePost[i].IsSuperior;
                            ListUserInfos.Add(FlowUser);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("GetDepartmentHeadByDepartmentID出现错误:" + ex.ToString() + "部门ID："+ departmentID );
            }
            return ListUserInfos;
        }

        /// <summary>
        ///通过岗位ID: (查找[直接上级]，[隔级上级]，包括所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        public List<FlowUserInfo> GetSuperiorByPostID(string postID)
        {
            List<FlowUserInfo> ListUserInfos = new List<FlowUserInfo>();
            try
            {
                if (string.IsNullOrEmpty(postID))
                {
                    return ListUserInfos;
                }
                Tracer.Debug("开始调用HR中的服务，GetSuperiorByPostID " + postID);
                //SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient Personnel = new SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();
                List<V_FlowUserInfo> ListEmployeePost = new List<V_FlowUserInfo>();// Personnel.GetSuperiorByPostID(postID);
                using (EmployeePostBLL bll = new EmployeePostBLL())
                {
                    Tracer.Debug("进入GetSuperiorByPostID：" + postID);
                    ListEmployeePost= bll.GetSuperiorByPostID(postID);
                }
                if (ListEmployeePost.Count() > 0)
                {
                    if (ListEmployeePost.Count() > 0)
                    {
                        for (int i = 0; i < ListEmployeePost.Count(); i++)
                        {
                            FlowUserInfo FlowUser = new FlowUserInfo();
                            FlowUser.CompayID = ListEmployeePost[i].CompayID;
                            FlowUser.CompayName = ListEmployeePost[i].CompayName;
                            FlowUser.DepartmentID = ListEmployeePost[i].DepartmentID;
                            FlowUser.DepartmentName = ListEmployeePost[i].DepartmentName;
                            FlowUser.PostID = ListEmployeePost[i].PostID;
                            FlowUser.PostName = ListEmployeePost[i].PostName;
                            FlowUser.UserID = ListEmployeePost[i].UserID;
                            FlowUser.EmployeeName = ListEmployeePost[i].EmployeeName;
                            List<T_SYS_ROLE> ListRoles = new List<T_SYS_ROLE>();
                            string StrEmployeeID = ListEmployeePost[i].UserID;
                            var Roles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                                        where ent.T_SYS_USER.EMPLOYEEID == StrEmployeeID
                                        select ent.T_SYS_ROLE;
                            if (Roles.Count() > 0)
                            {
                                ListRoles = Roles.ToList();
                            }
                            FlowUser.Roles = ListRoles;
                            FlowUser.IsHead = ListEmployeePost[i].IsHead;
                            FlowUser.IsSuperior = ListEmployeePost[i].IsSuperior;
                            ListUserInfos.Add(FlowUser);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("GetSuperiorByPostID出现错误:" + ex.ToString() +"岗位ID：" + postID);
            }
            return ListUserInfos;
        }


        /// <summary>
        /// 通过角色ID和公司ID:(查找所有人员，包括人员所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="roleID">角色ID</param>
        /// <param name="companyID">公司ID</param>
        /// <returns></returns>
        public List<FlowUserInfo> GetUserByRoleIDAndCompanyID(string roleID, string companyID)
        {
            List<FlowUserInfo> ListUserInfos = new List<FlowUserInfo>();
            try
            {
                //根据角色ID和公司ID获取人员信息
                var Roles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_ROLE").Include("T_SYS_USER")
                            where ent.T_SYS_ROLE.ROLEID == roleID && ent.OWNERCOMPANYID == companyID
                            select ent;
                //存在角色对应的员工信息            
                if (Roles.Count() > 0)
                {
                    //角色集合
                    //List<T_SYS_ROLE> ListRoles = new List<T_SYS_ROLE>();
                    //将默认的角色作为集合传入
                    //ListRoles.Add(Roles.FirstOrDefault().T_SYS_ROLE);
                    string StrEmployeeIDs = "";
                    foreach (var ent in Roles)
                    {
                        if (ent.T_SYS_USER != null)
                        {
                            StrEmployeeIDs += ent.T_SYS_USER.EMPLOYEEID + ",";
                        }
                    }
                    if (!string.IsNullOrEmpty(StrEmployeeIDs))
                    {
                        StrEmployeeIDs = StrEmployeeIDs.Substring(0, StrEmployeeIDs.Length - 1);
                        string[] ListEmployeeIDs = StrEmployeeIDs.Split(',');


                        EmployeePostBLL empPostBll = new EmployeePostBLL();
                        List<V_FlowUserInfo> ListEmployeePost = empPostBll.GetFlowUserInfoPostBriefByEmployeeID(ListEmployeeIDs.ToList());
                        #region 存在员工岗位信息

                        if (ListEmployeePost.Count() > 0)
                        {
                            for (int i = 0; i < ListEmployeePost.Count(); i++)
                            {
                                FlowUserInfo FlowUser = new FlowUserInfo();
                                FlowUser.CompayID = ListEmployeePost[i].CompayID;
                                FlowUser.CompayName = ListEmployeePost[i].CompayName;
                                FlowUser.DepartmentID = ListEmployeePost[i].DepartmentID;
                                FlowUser.DepartmentName = ListEmployeePost[i].DepartmentName;
                                FlowUser.PostID = ListEmployeePost[i].PostID;
                                FlowUser.PostName = ListEmployeePost[i].PostName;
                                FlowUser.UserID = ListEmployeePost[i].UserID;
                                FlowUser.EmployeeName = ListEmployeePost[i].EmployeeName;
                                List<T_SYS_ROLE> ListRoles = new List<T_SYS_ROLE>();
                                string StrFlowEmployeeID = ListEmployeePost[i].UserID;
                                var UserRoles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                                                where ent.T_SYS_USER.EMPLOYEEID == StrFlowEmployeeID
                                                select ent.T_SYS_ROLE;
                                if (Roles.Count() > 0)
                                {
                                    ListRoles = UserRoles.ToList();
                                }
                                FlowUser.Roles = ListRoles;
                                FlowUser.IsHead = ListEmployeePost[i].IsHead;
                                FlowUser.IsSuperior = ListEmployeePost[i].IsSuperior;
                                ListUserInfos.Add(FlowUser);
                            }
                        }

                        #endregion

                    }
                    else
                    {
                        Tracer.Debug("GetFlowUserInfoByRoleID出现错误：员工ID集合为空，角色ID：" + roleID);
                    }

                }
                else
                {
                    Tracer.Debug("GetFlowUserInfoByRoleID出现错误,获取的用户角色为空：角色ID：" + roleID);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("GetFlowUserInfoByRoleID出现错误：" + ex.ToString() + "角色ID：" + roleID);
            }

            return ListUserInfos;
        }


        /// <summary>
        /// 通过用户ID:（查找所在的公司、部门、岗位、角色，一个人可能同时在多个公司任职）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<FlowUserInfo> GetFlowUserByUserID(string userID)
        {
            List<FlowUserInfo> ListUserInfos = new List<FlowUserInfo>();
            try
            {
                if (string.IsNullOrEmpty(userID))
                {
                    return ListUserInfos;
                }
                Tracer.Debug("开始调用HR中的服务，GetFlowUserByUserID " + userID);
                Tracer.Debug("开始调用时间:" + System.DateTime.Now.ToString());
                DateTime Dt1 = System.DateTime.Now;

            
                EmployeePostBLL epBll = new EmployeePostBLL();
                List<V_FlowUserInfo> ListEmployeePost = epBll.GetFlowUserByUserID(userID);
                DateTime Dt2 = System.DateTime.Now;
                Tracer.Debug("GetFlowUserByUserID调用HR服务花费时间:" +(Dt2-Dt1).ToString());
                if (ListEmployeePost.Count() > 0)
                {
                    for (int i = 0; i < ListEmployeePost.Count(); i++)
                    {
                        FlowUserInfo FlowUser = new FlowUserInfo();
                        FlowUser.CompayID = ListEmployeePost[i].CompayID;
                        FlowUser.CompayName = ListEmployeePost[i].CompayName;
                        FlowUser.DepartmentID = ListEmployeePost[i].DepartmentID;
                        FlowUser.DepartmentName = ListEmployeePost[i].DepartmentName;
                        FlowUser.PostID = ListEmployeePost[i].PostID;
                        FlowUser.PostName = ListEmployeePost[i].PostName;
                        FlowUser.UserID = ListEmployeePost[i].UserID;
                        FlowUser.EmployeeName = ListEmployeePost[i].EmployeeName;
                        List<T_SYS_ROLE> ListRoles = new List<T_SYS_ROLE>();
                        string StrFlowEmployeeID = ListEmployeePost[i].UserID;
                        var Roles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                                    where ent.T_SYS_USER.EMPLOYEEID == StrFlowEmployeeID
                                    select ent.T_SYS_ROLE;
                        if (Roles.Count() > 0)
                        {
                            ListRoles = Roles.ToList();
                        }
                        FlowUser.Roles = ListRoles;
                        FlowUser.IsHead = ListEmployeePost[i].IsHead;
                        FlowUser.IsSuperior = ListEmployeePost[i].IsSuperior;
                        ListUserInfos.Add(FlowUser);
                    }
                }
                else
                {
                    Tracer.Debug("GetFlowUserByUserID出现错误,获取用户岗位信息为空，用户ID：" + userID);
                }
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("GetFlowUserByUserID出现错误:" + ex.ToString() +"用户ID：" + userID);
            }
            return ListUserInfos;
        }


        /// <summary>
        /// 通过用户ID,模块代码:（查询是否使用代理人，包括所在的公司、部门、岗位、角色）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="modelCode">模块代码</param>
        /// <returns></returns>
        public List<FlowUserInfo> GetAgentUser(string userID, string modelCode)
        {
            return null;
        }
        #endregion


        /// <summary>
        /// 修改系统用户密码信息
        /// </summary>
        /// <param name="UserObj"></param>
        /// <returns></returns>
        public string UpdatePwdByUserNameAndPwd(string username, string pwd)
        {
            string IsReturn = "";
            try
            {
               
                var entity = from ent in dal.GetTable()
                             where ent.USERNAME == username
                             select ent;

                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();
                    entitys.PASSWORD = pwd;
                    entitys.UPDATEDATE = System.DateTime.Now;

                    int i = 0;

                    i = dal.Update(entitys);
                    if (i > 0)
                    {
                        IsReturn = "";
                    }

                }
                else
                {
                    IsReturn = "帐号不存在";
                }
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-UpdatePwdByUserNameAndPwd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                
            }
            return IsReturn;
        }

        /// <summary>
        /// 获取所有的超级预算管理员
        /// </summary>
        /// <returns></returns>
        public List<V_SupperFbAdmin> GetSupperAdmins()
        {
            List<V_SupperFbAdmin> listAdmins = new List<V_SupperFbAdmin>();
            try
            {
                string setCompanyIDs = string.Empty;
                try
                {
                    setCompanyIDs = System.Configuration.ConfigurationManager.AppSettings["SupperFbAdminCompanyID"].ToString();
                }
                catch (Exception ex)
                {
                    Tracer.Debug("系统用户SysUserBLL-GetSupperAdmins没有设置超级预算管理员的公司ID值:" + ex.ToString());
                }
                //var setCompanyIDs = app
                CompanyBLL bll = new CompanyBLL();
                var companys = bll.GetCompanyAll("");
                //获取超级预算管理员信息
                var ents = from ent in dal.GetObjects<T_SYS_FBADMIN>()
                           //join ur in DataContext.T_SYS_USER_ROLE on rp.T_SYS_ROLE.ROLEID equals ur.T_SYS_ROLE.ROLEID
                           join a in dal.GetObjects<T_SYS_USER>() on ent.SYSUSERID equals a.SYSUSERID
                           where ent.ISSUPPERADMIN == "1"
                           select new V_SupperFbAdmin 
                           { 
                               SysUserID = a.SYSUSERID,
                               EmployeeID = a.EMPLOYEEID,
                               EmployeeName = a.EMPLOYEENAME,
                               CompanyID = a.OWNERCOMPANYID,
                               EmployeeState = a.STATE                               
                           };
                foreach (var ent in ents)
                {
                    V_SupperFbAdmin admin = new V_SupperFbAdmin();
                    admin = ent;
                    admin.CanSetCompanyID = setCompanyIDs;
                    string companyName = string.Empty;
                    T_HR_COMPANY  company =companys.Where(s=>s.COMPANYID == ent.CompanyID).FirstOrDefault();
                    if(company != null)
                    {
                        if(string.IsNullOrEmpty(company.BRIEFNAME))
                        {
                            companyName = company.CNAME;
                        }
                        else
                        {
                            companyName = company.BRIEFNAME;
                        }
                    }
                    admin.CompanyName = companyName;
                    listAdmins.Add(admin);
                }                
            }
            catch (Exception ex)
            {
                Tracer.Debug("系统用户SysUserBLL-GetSupperAdmins获取超级预算管理员出错"  + ex.ToString());
            }
            return listAdmins;
        }

        /// <summary>
        /// 修改系统用户信息
        /// </summary>
        /// <param name="UserObj"></param>
        /// <param name="isSupper"></param>
        /// <returns></returns>
        public bool UpdateSysUserInfoForNewSupper(T_SYS_USER UserObj,bool isSupper)
        {
            try
            {                
                var entity = from ent in dal.GetObjects<T_SYS_USER>()
                             where ent.SYSUSERID == UserObj.SYSUSERID
                             select ent;
                dal.BeginTransaction();
                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();
                    Utility.CloneEntity<T_SYS_USER>(UserObj, entitys);
                    int i = 0;                    
                    entitys.UPDATEDATE = System.DateTime.Now;
                    i = dal.Update(entitys);
                    if (i > 0)
                    {
                        var ents = from ent in dal.GetObjects<T_SYS_FBADMIN>()
                                   where ent.ISSUPPERADMIN == "1"
                                   && ent.OWNERCOMPANYID == entitys.OWNERCOMPANYID
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            List<string> fbAdmins = new List<string>();
                            foreach (var ent in ents)
                            {
                                fbAdmins.Add(ent.FBADMINID);
                            }
                            T_SYS_FBADMIN fbAdmin = new T_SYS_FBADMIN();
                            T_SYS_FBADMINROLE adminRole = new T_SYS_FBADMINROLE();
                            var entRoles = from ent in dal.GetObjects<T_SYS_FBADMINROLE>().Include("T_SYS_FBADMIN")
                                           where fbAdmins.Contains(ent.T_SYS_FBADMIN.FBADMINID)
                                           select ent;
                            string strSysUserID = string.Empty;
                            if (entRoles.Count() > 0)
                            {
                                adminRole =entRoles.FirstOrDefault();
                                fbAdmin = adminRole.T_SYS_FBADMIN;
                                strSysUserID = fbAdmin.SYSUSERID;
                            }
                            //T_SYS_FBADMIN fbAdmin = ents.FirstOrDefault();
                            fbAdmin.OWNERCOMPANYID = entitys.OWNERCOMPANYID;
                            fbAdmin.OWNERDEPARTMENTID = entitys.OWNERDEPARTMENTID;
                            fbAdmin.OWNERPOSTID = entitys.OWNERPOSTID;
                            fbAdmin.SYSUSERID = entitys.SYSUSERID;
                            fbAdmin.EMPLOYEEID = entitys.EMPLOYEEID;
                            fbAdmin.EMPLOYEECOMPANYID = entitys.OWNERCOMPANYID;
                            fbAdmin.EMPLOYEEDEPARTMENTID = entitys.OWNERDEPARTMENTID;
                            fbAdmin.EMPLOYEEPOSTID = entitys.OWNERPOSTID;
                            int k = dal.Update(fbAdmin);
                            if (k > 0)
                            {
                                string strRoleID = adminRole.ROLEID;
                                
                                var userroles = from ent in dal.GetObjects<T_SYS_USERROLE>().Include("T_SYS_USER").Include("T_SYS_ROLE")
                                                where ent.T_SYS_ROLE.ROLEID ==strRoleID  && ent.T_SYS_USER.SYSUSERID == strSysUserID
                                                select ent;
                                if (userroles.Count() > 0)
                                {
                                    T_SYS_USERROLE userroleSigle = userroles.FirstOrDefault();
                                    //赋予现在修改的员工的值
                                    userroleSigle.T_SYS_USERReference.EntityKey =
                        new System.Data.EntityKey("TM_SaaS_OA_EFModelContext.T_SYS_USER", "SYSUSERID", entitys.SYSUSERID);
                        //            userroleSigle.T_SYS_USER.SYSUSERID = entitys.SYSUSERID;
                                    userroleSigle.T_SYS_USER = entitys;
                                    userroleSigle.UPDATEDATE = DateTime.Now;
                                    int bb = dal.Update(userroleSigle);
                                    if (bb > 0)
                                    {
                                        dal.CommitTransaction();
                                    }
                                    else
                                    {
                                        Tracer.Debug("UpdateSysUserInfoForNewSupper-更新用户角色回滚回滚，原来的用户："+ strSysUserID);
                                        if (userroleSigle.T_SYS_ROLE != null)
                                        {
                                            Tracer.Debug("UpdateSysUserInfoForNewSupper-更新用户角色回滚回滚，更新角色：" + strRoleID + "。角色名称:" + userroleSigle.T_SYS_ROLE.ROLENAME);
                                        }
                                        Tracer.Debug("UpdateSysUserInfoForNewSupper-更新用户角色回滚回滚，现有用户："+ entitys.SYSUSERID+".员工名称:"+ entitys.EMPLOYEENAME);
                                        Tracer.Debug("UpdateSysUserInfoForNewSupper-更新用户角色回滚回滚：");
                                        dal.RollbackTransaction();
                                        return false;
                                    }
                                }
                                else
                                {
                                    Tracer.Debug("UpdateSysUserInfoForNewSupper-没获取到用户角色信息回滚：");
                                    dal.RollbackTransaction();
                                    return false;
                                }
                                
                            }
                            else
                            {
                                Tracer.Debug("UpdateSysUserInfoForNewSupper-更新T_SYS_FBADMIN回滚：");
                                dal.RollbackTransaction();
                                return false;
                            }
                        }
                        try
                        {
                            string useMail = System.Configuration.ConfigurationManager.AppSettings["CompanyHasMail"].ToString();
                            if (useMail == "1")
                            {
                                //添加方法同时修改邮件用户密码
                                changeMailPassword(entitys);
                            }
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("获取是否使用邮箱是出错：" + ex.ToString());
                            
                        }
                        //dal.CommitTransaction();
                        return true;
                    }
                    //}
                }
                return false;
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                //dal.RollbackTransaction();
                Tracer.Debug("系统用户SysUserBLL-UpdateSysUserInfoForNewSupper："  + ex.ToString());
                return false;
            }
        }

    }
    #endregion


}
