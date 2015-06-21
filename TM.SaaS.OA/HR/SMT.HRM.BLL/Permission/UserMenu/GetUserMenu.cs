
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.HRM.DAL.Permission;
using TM_SaaS_OA_EFModel;
using System.Linq.Dynamic;
using System.Data;
using SMT.Foundation.Log;
using System.Configuration.Assemblies;
using System.Configuration;
using SMT.HRM.CustomModel.Permission;
using System.Web.Hosting;
using System.Xml.Linq;
using SMT.HRM.BLL;

namespace SMT.HRM.BLL.Permission
{
    public partial class SysEntityMenuBLL
    {
        public List<V_UserMenuPermission> GetSysLeftMenuFilterPermissionToNewFrame(string SYSUSERID)
        {
            List<V_UserMenuPermission> menuList = new List<V_UserMenuPermission>();
            var User = (from a in dal.GetObjects<T_SYS_USER>()
                        join b in dal.GetObjects<T_SYS_FBADMIN>() on a.EMPLOYEEID equals b.EMPLOYEEID
                        into temp  from tt in temp.DefaultIfEmpty()
                        where a.SYSUSERID == SYSUSERID
                        select new
                        {
                            a.ISMANGER,
                            a.ISFLOWMANAGER,
                            ISSUPPERADMIN=tt==null?"":tt.ISSUPPERADMIN,
                            ISCOMPANYADMIN=tt==null?"":tt.ISCOMPANYADMIN
                        }).FirstOrDefault();
            if (User != null)
            {
                menuList = GetSysLeftMenuFilterPermissionToNewFrame(SYSUSERID, User.ISMANGER.ToString(), User.ISSUPPERADMIN, User.ISCOMPANYADMIN, User.ISFLOWMANAGER);
            }

            return menuList;
        }
        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息  2010-6-29
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public List<V_UserMenuPermission> GetSysLeftMenuFilterPermissionToNewFrame(string SYSUSERID, string isAdmin,
            string isFbAdmin, string isFbCompanyAdmin, string isFlowAdmin)
        {

            List<V_UserMenuPermission> AllMenus = CacheAllMenus;
            List<V_UserMenuPermission> userMenus = new List<V_UserMenuPermission>();
           
            try
            {

                #region 获取所有菜单
                //获取是菜单但不受权限控制的菜单
                //IQueryable<V_Permission> entspermissionAll;
                string mvcMenu = "MVC";
                List<string> menuids = (from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")
                                        join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                                        join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                                        join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                        join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                                        join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                        where ur.T_SYS_USER.SYSUSERID == SYSUSERID
                                        && n.PERMISSIONVALUE == "3"
                                        && m.HASSYSTEMMENU == "1"
                                        select m.ENTITYMENUID).Distinct().ToList();

                #endregion

                #region 管理员菜单
                if (isAdmin == "1")
                {
                    var SystemEnts = (from a in AllMenus
                                     where a.SYSTEMTYPE == "7" && a.HASSYSTEMMENU == "1" && a.ISAUTHORITY == "1"                                     
                                     orderby a.ORDERNUMBER
                                     select a.ENTITYMENUID).ToList();
                    if (SystemEnts.Count()>0)
                    {
                        menuids.AddRange(SystemEnts);
                    }

                }
                #endregion

                #region 预算管理员菜单
                // 如果不是预算超级管理员隐藏权限中“预算管理员设置”、预算中的“系统字典维护”2个菜单
                Tracer.Debug("非超级预算管理员,移除一下菜单：预算管理员设置，系统科目字典维护，公司科目分配。");
                var fbadminMenuId = "8eb5cf13-ecd3-4f4b-a0d7-8ce4658d19c5";//预算管理员设置
                var fbSubjectMenuId = "709D9380-5405-4429-B047-20100401D255";//系统科目字典维护
                var fbCompanySubjectMenuId = "599D904D-14FB-4637-8B8B-00AF6C1B49E7";//公司科目分配
                if (isFbAdmin == "1")
                {
                    menuids.Add(fbSubjectMenuId);
                    menuids.Add(fbadminMenuId);
                    menuids.Add(fbCompanySubjectMenuId);
                }
                else
                {
                    menuids.Remove(fbSubjectMenuId);
                    menuids.Remove(fbadminMenuId);
                    menuids.Remove(fbCompanySubjectMenuId);
                }
                #endregion

                #region 流程菜单
                if (isFlowAdmin == "1")
                {
                    var FlowsMenuids = (from a in AllMenus
                                        where a.SYSTEMTYPE == "8" && a.HASSYSTEMMENU == "1"
                                        orderby a.ORDERNUMBER
                                        select a.ENTITYMENUID).ToList();
                    if (FlowsMenuids.Count() > 0)
                    {
                        menuids.AddRange(FlowsMenuids);
                    }
                }
                #endregion

                #region 获取所有菜单，包括父级菜单
                List<string> MenusFAtherResult = new List<string>();
                menuids=menuids.Distinct().ToList();

                MenusFAtherResult.AddRange(menuids);

                foreach (var menuid in menuids)
                {                 
                    getFatherId(menuid,AllMenus, ref MenusFAtherResult);
                }
                userMenus = (from ent in AllMenus
                             where MenusFAtherResult.Contains(ent.ENTITYMENUID)
                             select ent).ToList();
               var ttt= userMenus.Select(c => c.EntityMenuFatherID == string.Empty);

                #endregion

            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenuFilterPermissionToNewFrame" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }
            return userMenus;

        }

        private void getFatherId(string menuId, List<V_UserMenuPermission> AllMenus, ref List<string> MenusFAtherResult)
        {

            var entity = (from ent in AllMenus
                            where ent.ENTITYMENUID == menuId
                            select new { ent.EntityMenuFatherID, ent.URLADDRESS }).FirstOrDefault();
                if(entity==null)
                {
                    //如果父菜单为空
                    return;
                }
                if (!string.IsNullOrEmpty(entity.URLADDRESS))
                {
                    if (entity.URLADDRESS.Contains("mvc"))
                    {
                        //移除mvc菜单
                        MenusFAtherResult.Remove(menuId);
                        return;
                    }
                }
                if(string.IsNullOrEmpty(entity.EntityMenuFatherID))
                {
                    //如果父菜单为空
                    return;
                }
                else
                {
                    //添加父菜单，并继续寻找上级菜单
                    MenusFAtherResult.Add(entity.EntityMenuFatherID);
                    getFatherId(entity.EntityMenuFatherID, AllMenus, ref MenusFAtherResult);
                }
        }
    }
}
