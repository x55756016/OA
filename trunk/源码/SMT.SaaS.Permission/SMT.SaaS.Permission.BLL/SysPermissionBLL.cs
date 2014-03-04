using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Dynamic;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using SMT.Foundation.Log;

namespace SMT.SaaS.Permission.BLL
{
    public class SysPermissionBLL:BaseBll<T_SYS_PERMISSION>
    {
        SysPermissionDAL permissiondal = new SysPermissionDAL();
        /// <summary>
        /// 获取所有权限定义
        /// </summary>
        /// <returns>权限定义列表</returns>
        public IQueryable<T_SYS_PERMISSION> GetSysPermissionAll()
        {
            try
            {
                //dal.DataContext.ContextOptions.LazyLoadingEnabled = 
                var ents = from a in dal.GetTable()
                           orderby a.PERMISSIONVALUE
                           select a;
                           
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-GetSysPermissionAll" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw(ex);
            }
        }

        public IQueryable<T_SYS_PERMISSION> GetSysCommonPermissionAll()
        {
            try
            {                
                var ents = from a in dal.GetTable()
                           where a.ISCOMMOM =="1"
                           orderby a.PERMISSIONVALUE
                           select a;

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-GetSysCommonPermissionAll" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }

        public string AddPermission(T_SYS_PERMISSION Perm)
        {
            String StrReturn = "";
            try
            {
                var ents = from ent in dal.GetTable()
                           where ent.PERMISSIONNAME == Perm.PERMISSIONNAME && ent.PERMISSIONCODE == Perm.PERMISSIONCODE
                           select ent;
                var entCode = from ent in dal.GetTable()
                              where ent.PERMISSIONCODE == Perm.PERMISSIONCODE
                              select ent;
                if (ents.Count() > 0)
                    StrReturn = "PERMISSIONISEXIST";//权限已存在

                if (entCode.Count() > 0)
                    StrReturn = "PERMISSIONCODEISEXIST";//权限编码已存在
                if (string.IsNullOrEmpty(StrReturn))
                {
                    Utility.RefreshEntity(Perm);
                    int i = dal.Add(Perm);
                    if (!(i > 0))
                        StrReturn = "ERROR";

                }
                else
                {
                    
                    return StrReturn;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-AddPermission" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrReturn = "SYSTEMERRORPLEASELINKDADMIN";//系统错误 请联系管理员
            }
            return StrReturn;
        }

        /// <summary>
        /// 服务器端分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_SYS_PERMISSION> GetSysPermissionAllWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in GetObjects()
                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_PERMISSION");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_SYS_PERMISSION>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-GetSysPermissionAllWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }

        }


        /// <summary>
        /// 根据权限ID获取权限定义
        /// </summary>
        /// <param name="ID">权限ID</param>
        /// <returns>权限定义列表</returns>
        public T_SYS_PERMISSION GetSysPermissionByID(string ID)
        {
            try
            {
                SysEntityMenuBLL menuBll = new SysEntityMenuBLL();
                var ent1 = from ent in dal.GetObjects()
                           where ent.PERMISSIONID == ID
                           select ent;
                if (ent1.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(ent1.FirstOrDefault().PERMISSIONCODE)) //不是公共权限
                    {
                        var ents = from ent in dal.GetObjects().Include("T_SYS_ENTITYMENU")
                                   where ent.PERMISSIONID == ID
                                   select ent;
                        //ents.ToList();

                        //var ents = from ent in dal.GetObjects().Include("T_SYS_ENTITYMENU")
                        //           join e in menuBll.GetTable() on ent.T_SYS_ENTITYMENU.ENTITYMENUID equals e.ENTITYMENUID
                        //           where ent.PERMISSIONID == ID
                        //           select ent;
                        return ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    }

                }
                return ent1.Count() > 0 ? ent1.FirstOrDefault() : null;


            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-GetSysPermissionByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 修改权限定义
        /// </summary>
        /// <param name="entity">被修改的权限的实体</param>
        public string SysPermissionUpdate(T_SYS_PERMISSION entity)
        {
            string StrReturn="";
            try
            {
                var Mulents = from ent in dal.GetTable()
                              where ent.PERMISSIONCODE == entity.PERMISSIONCODE
                              select ent;
                if (Mulents.Count() > 1)
                {
                    StrReturn = "PERMISSIONCODEISEXIST"; //权限编码已经存在
                    return StrReturn;
                }

                var ents = from ent in dal.GetTable()
                           where ent.PERMISSIONID == entity.PERMISSIONID 
                           select ent;

                if (ents.Count() > 0)
                {
                    //var ent = ents.FirstOrDefault();
                    //ent.SYSTEMTYPE = entity.SYSTEMTYPE;
                    //Utility.CloneEntity(entity,ent);
                    
                    //if(ent.T_SYS_ENTITYMENUReference.EntityKey != null)
                    //    ent.T_SYS_ENTITYMENUReference.EntityKey =
                    //    new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ROLEID", entity.T_SYS_ENTITYMENUReference.EntityKey.EntityKeyValues[0].Value.ToString());

                    //ent.PERMISSIONNAME = entity.PERMISSIONNAME;
                    //ent.REMARK = entity.REMARK;
                    //ent.ISCOMMOM = entity.ISCOMMOM;
                    //ent.PERMISSIONCODE = entity.PERMISSIONCODE;
                    //ent.PERMISSIONVALUE = entity.PERMISSIONVALUE;
                    //ent.UPDATEUSER = entity.UPDATEUSER;
                    //ent.UPDATEDATE = entity.UPDATEDATE;

                    //int i = dal.Update(ent);
                    int i = dal.Update(entity);
                    if (!(i > 0))
                        StrReturn = "ERROR";
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-SysPermissionUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrReturn = "SYSTEMERRORPLEASELINKDADMIN";//系统错误 请联系管理员                
            }
            return StrReturn;
        }

        /// <summary>
        /// 删除权限定义
        /// </summary>
        /// <param name="ID">权限ID</param>
        /// <returns>是否删除成功</returns>
        public string SysPermissionDelete(string ID)
        {
            
            try
            {
                //TODO:验证权限是否能被删除     
                //throw new Exception("有字集，不能删除！");
                string StrReturn = "";
                var entitys = (from ent in dal.GetTable()
                               where ent.PERMISSIONID == ID
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    //判断是否和角色关联
                    var PermissionRole = from ent in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_PERMISSION")
                                         where ent.T_SYS_PERMISSION.PERMISSIONID == entity.PERMISSIONID
                                         select ent;
                    if (PermissionRole.Count() > 0)
                    {
                        StrReturn = "PERMISSIONRELATEDROLE";//和角色有关联
                        return StrReturn;
                    }
                    var CustomerPermission = from ent in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>().Include("T_SYS_PERMISSION")
                                             where ent.T_SYS_PERMISSION.PERMISSIONID == entity.PERMISSIONID
                                             select ent;
                    if (CustomerPermission.Count() > 0)
                    {
                        StrReturn = "PERMISSIONRELATEDCUSTOMER";//和自定义角色有关联
                        return StrReturn;
                    }
                    //判断是否和自定义权限有关联
                    int i = dal.Delete(entity);
                    if (!(i > 0))
                    {
                        StrReturn = "ERROR";
                    }
                    
                }
                return StrReturn;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-SysPermissionDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "ERROR";
                //throw (ex);
            }
        }

        /// <summary>
        /// 根据权限名称查询
        /// </summary>
        /// <param name="sName">权限名称</param>
        /// <returns>权限定义列表</returns>
        public IQueryable<T_SYS_PERMISSION> FindSysPermissionByStr(string sName)
        {
            try
            {
                var q = from ent in dal.GetTable()
                        //where (string.IsNullOrEmpty(sName) || ent.PERMISSIONNAME == sName)
                        where ent.PERMISSIONNAME.Contains(sName)
                        orderby ent.PERMISSIONNAME
                        select ent;
                return q;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-FindSysPermissionByStr" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据系统类型查询
        /// </summary>
        /// <param name="sName">权限名称</param>
        /// <returns>权限定义列表</returns>
        public IQueryable<T_SYS_PERMISSION> FindSysPermissionByType(string type)
        {
            try
            {
                var q = from ent in dal.GetTable()
                        //where (string.IsNullOrEmpty(type) || ent.SYSTEMTYPE == type)
                        select ent;
                return q;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-GetEntityMenuByMenuIDs" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        
        public T_SYS_PERMISSION GetAttachSysPermissionByID(string strPermissionId)
        {
            try
            {
                T_SYS_PERMISSION ent = GetSysPermissionByID(strPermissionId);
                
                return ent;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-FindSysPermissionByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取  权限信息  设置了缓存
        /// </summary>
        /// <param name="strPermissionId"></param>
        /// <returns></returns>
        public T_SYS_PERMISSION GetAttachSysPermissionByIDToCustomer(string strPermissionId)
        {
            try
            {
                T_SYS_PERMISSION ent = null;
                string keystring = "GetAttachSysPermissionByIDToCustomer" + strPermissionId;


                if (CacheManager.GetCache(keystring) != null)
                {
                    ent = (T_SYS_PERMISSION)CacheManager.GetCache(keystring);
                }
                else
                {
                    ent = GetSysPermissionByID(strPermissionId);

                    CacheManager.AddCache(keystring, ent);
                }
                return ent;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-FindSysPermissionByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public IQueryable<T_SYS_PERMISSION> GetPermissionByEntityID(string strEntityId)
        {
            try
            {
                var ents = from ent in dal.GetObjects().Include("T_SYS_ENTITYMENU")
                           //where ent.ISCOMMOM == "1" || ent.T_SYS_ENTITYMENU.ENTITYMENUID == strEntityId
                           select ent;
                //if (ents != null)
                //{
                //    if (ents.Count() > 0)
                //        ents = ents.Where(p => p.T_SYS_ENTITYMENU.ENTITYMENUID == strEntityId);
                //}
                //var ents = from ent in dal.GetObjects().Include("T_SYS_ENTITYMENU")
                //           where ent.ISCOMMOM == "1" || (!string.IsNullOrEmpty(strEntityId) && ent.T_SYS_ENTITYMENU.ENTITYMENUID == strEntityId && ent.ISCOMMOM=="0")
                //           select ent;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限SysPermissionBLL-GetPermissionByEntityID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        class DateArray
        {
            public DateTime? urUpdate{get;set;}
            public DateTime? urCreate { get; set; }
            public DateTime? remUpdate { get; set; }
            public DateTime? remCreate { get; set; }
            public DateTime? rmpUpdate { get; set; }
            public DateTime? rmpCreate { get; set; }
        }

        #region 获取权限更新相关信息
        /// <summary>
        /// 获取最近的权限更新的时间,键值分别为T_SYS_USERROLE,T_SYS_ROLEENTITYMENU,T_SYS_ROLEMENUPERMISSION,T_SYS_ENTITYMENUCUSTOMPERM
        /// </summary>
        /// <param name="employeeid">用户id</param>
        /// <returns>更新时间(包括取自定义权限)</returns>
        public Dictionary<string,DateTime?> GetLatestTimeOfPermission(string employeeid)
        {
            Dictionary<string, DateTime?> latestTime = new Dictionary<string,DateTime?>();
            try
            {
                //获取权限的更新时间
                var entsPer = from u in dal.GetObjects<T_SYS_USER>()
                              join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                              join rem in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on ur.T_SYS_ROLE.ROLEID equals rem.T_SYS_ROLE.ROLEID
                              join rmp in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>() on rem.ROLEENTITYMENUID equals rmp.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID
                              where u.EMPLOYEEID == employeeid
                              select new DateArray
                              {
                                  urUpdate = ur.UPDATEDATE,
                                  urCreate = ur.CREATEDATE,
                                  remUpdate = rem.UPDATEDATE,
                                  remCreate = rem.CREATEDATE,
                                  rmpUpdate = rmp.UPDATEDATE,
                                  rmpCreate = rmp.CREATEDATE
                              };
                           //select new DateTime?[]
                           //{
                           //    ur.UPDATEDATE,ur.CREATEDATE,
                           //    rem.UPDATEDATE,rem.CREATEDATE,
                           //    rmp.UPDATEDATE,rmp.CREATEDATE
                           //};

                //获取自定义权限的最近更新时间
                var entsCPer = from ecp in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                               join ur in dal.GetObjects<T_SYS_USERROLE>() on ecp.T_SYS_ROLE.ROLEID equals ur.T_SYS_ROLE.ROLEID
                               join u in dal.GetObjects<T_SYS_USER>() on ur.T_SYS_USER.SYSUSERID equals u.SYSUSERID
                               where u.EMPLOYEEID == employeeid
                               select ecp.UPDATEDATE;

                //依次声明为T_SYS_UserRole   T_SYS_RoleEntityMenu T_SYS_RoleMenuPermission的更新时间
                DateTime defaultDate = Convert.ToDateTime("2000/1/1 00:00:00");
                DateTime? roleDate = defaultDate,
                          roleEntityDate = defaultDate,
                          perDate = defaultDate,
                          cpDate = defaultDate;

                //取两个更新时间里最大的
                if (entsPer!=null && entsPer.Count() > 0)
                {
                    var Dates = entsPer.ToArray();
                    for (int i = 0; i < Dates.Count(); i++)
                    {
                        if (Dates[i] != null)
                        {
                            if (Dates[i].urUpdate != null)
                            {
                                DateTime? tempDate = Dates[i].urUpdate > Dates[i].urCreate ? Dates[i].urUpdate : Dates[i].urCreate;
                                if (tempDate > roleDate) roleDate = tempDate;
                            }
                            else
                            {
                                if (roleDate < Dates[i].urCreate) roleDate = Dates[i].urCreate;
                            }

                            if (Dates[i].remUpdate != null)
                            {
                                DateTime? tempDate = Dates[i].remUpdate > Dates[i].remCreate ? Dates[i].remUpdate : Dates[i].remCreate;
                                if (tempDate > roleEntityDate) roleEntityDate = tempDate;
                            }
                            else
                            {
                                if (roleEntityDate < Dates[i].remCreate) roleEntityDate = Dates[i].remCreate;
                            }

                            if (Dates[i].rmpUpdate != null)
                            {
                                DateTime? tempDate = Dates[i].rmpUpdate > Dates[i].rmpCreate ? Dates[i].rmpUpdate : Dates[i].rmpCreate;
                                if (tempDate > perDate) perDate = tempDate;
                            }
                            else
                            {
                                if (perDate < Dates[i].rmpCreate) perDate = Dates[i].rmpCreate;
                            }
                        }
                    }
                }
                if (entsCPer != null && entsCPer.Count() > 0)
                {
                    cpDate = entsCPer.Max();
                }
                latestTime.Add("T_SYS_USERROLE", roleDate);
                latestTime.Add("T_SYS_ROLEENTITYMENU", roleEntityDate);
                latestTime.Add("T_SYS_ROLEMENUPERMISSION",perDate);
                latestTime.Add("T_SYS_ENTITYMENUCUSTOMPERM",cpDate);
            }
            catch (Exception ex)
            {
                Tracer.Debug("SysPermissionBLLL-GetLatestTimeOfPermission出错:" + ex.ToString());
            }
            return latestTime;
        }

        /// <summary>
        /// 获取最近的权限数据数量,键值分别为T_SYS_USERROLE,T_SYS_ROLEENTITYMENU,T_SYS_ROLEMENUPERMISSION,T_SYS_ENTITYMENUCUSTOMPERM
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public Dictionary<string,int> GetPermissionCounts(string employeeId)
        {
            Dictionary<string,int> result = new Dictionary<string,int>();
            try
            {
                //获取权限的更新时间
                var entsPer = from u in dal.GetObjects<T_SYS_USER>()
                              join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                              join rem in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on ur.T_SYS_ROLE.ROLEID equals rem.T_SYS_ROLE.ROLEID
                              join rmp in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>() on rem.ROLEENTITYMENUID equals rmp.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID
                              where u.EMPLOYEEID == employeeId
                              select rmp;

                var entsEntity = from u in dal.GetObjects<T_SYS_USER>()
                              join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                              join rem in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on ur.T_SYS_ROLE.ROLEID equals rem.T_SYS_ROLE.ROLEID
                              where u.EMPLOYEEID == employeeId
                              select rem;

                var entsRole = from u in dal.GetObjects<T_SYS_USER>()
                              join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                              where u.EMPLOYEEID == employeeId
                              select ur;

                //获取自定义权限的最近更新时间
                var entsCPer = from ecp in dal.GetObjects<T_SYS_ENTITYMENUCUSTOMPERM>()
                               join ur in dal.GetObjects<T_SYS_USERROLE>() on ecp.T_SYS_ROLE.ROLEID equals ur.T_SYS_ROLE.ROLEID
                               join u in dal.GetObjects<T_SYS_USER>() on ur.T_SYS_USER.SYSUSERID equals u.SYSUSERID
                               where u.EMPLOYEEID == employeeId
                               select ecp;
                int roleCount = entsRole.Count(),
                    rentCount = entsEntity.Count(),
                    perCount = entsPer.Count(),
                    cperCount = entsCPer.Count();

                result.Add("T_SYS_USERROLE", roleCount);
                result.Add("T_SYS_ROLEENTITYMENU", rentCount);
                result.Add("T_SYS_ROLEMENUPERMISSION", perCount);
                result.Add("T_SYS_ENTITYMENUCUSTOMPERM", cperCount);            
            }
            catch (Exception ex)
            {
                Tracer.Debug("SysPermissionBLLL-GetPermissionCounts出错:" + ex.ToString());
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 获取某些公司的流程管理员
        /// </summary>
        /// <param name="companyIDs">公司ID集合</param>
        /// <returns>返回系统用户集合</returns>
        public List<T_SYS_USER> GetFlowManagers(List<string> companyIDs)
        {
            List<T_SYS_USER> listUsers = new List<T_SYS_USER>();
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_USER>()
                           where ent.STATE == "1" && ent.ISFLOWMANAGER == "1"
                           && companyIDs.Contains(ent.OWNERCOMPANYID)
                           select ent;
                if (ents.Count() > 0)
                {
                    listUsers = ents.ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return listUsers;
        }

    }
}
