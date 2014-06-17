/*
 * 文件名：SysEntityMenuBLL.cs
 * 作  用：权限对应的菜单
 * 创建人：刘建兴
 * 创建时间：2010-2-26 14:19:12
 * 修改人：刘建兴
 * 修改说明：增加缓存
 * 修改时间：2010-7-7 14:19:12
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using System.Linq.Dynamic;
using System.Data;
using SMT.Foundation.Log;
using System.Configuration.Assemblies;
using System.Configuration;

namespace SMT.SaaS.Permission.BLL
{
    public class SysEntityMenuBLL : BaseBll<T_SYS_ENTITYMENU>
    {

        private List<T_SYS_ENTITYMENU> listTemp;

        public List<T_SYS_ENTITYMENU> ListTemp
        {
            get
            {

                List<T_SYS_ENTITYMENU> lsdic;
                if (CacheManager.GetCache("T_SYS_ENTITYMENU") != null)
                {
                    lsdic = (List<T_SYS_ENTITYMENU>)CacheManager.GetCache("T_SYS_ENTITYMENU");
                }
                else
                {
                    var ents = from a in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                               select a;
                    //var ents = from a in dal.GetObjects().Include("T_SYS_ENTITYMENU2")
                    //           select a;
                    //var ents = from a in dal.GetTable()// DataContext.T_SYS_ENTITYMENU.Include("T_SYS_ENTITYMENU2")                               
                    //           select a;

                    lsdic = ents.ToList();
                    CacheManager.AddCache("T_SYS_ENTITYMENU", lsdic);
                }

                return lsdic.Count() > 0 ? lsdic : null;
            }


            set { listTemp = value; }
        }

        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetSysMenuByType(string sysType,string parentID,T_SYS_FBADMIN UserFb)
        {
            try
            {
                //var ents = from a in ListTemp.AsQueryable()
                //           where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType)
                //           && (string.IsNullOrEmpty(parentID) || (a.T_SYS_ENTITYMENU2 != null && a.T_SYS_ENTITYMENU2.ENTITYMENUID == parentID))
                //           orderby a.ORDERNUMBER
                           
                //           select a;
                //只显示受权限控制的菜单
                IQueryable<T_SYS_ENTITYMENU> ents;
                ents = from a in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                           where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType)
                           && (string.IsNullOrEmpty(parentID) || (a.T_SYS_ENTITYMENU2 != null && a.T_SYS_ENTITYMENU2.ENTITYMENUID == parentID))
                           && a.ISAUTHORITY == "1"
                           orderby a.T_SYS_ENTITYMENU2.ENTITYMENUID, a.ORDERNUMBER

                           select a;
                List<T_SYS_ENTITYMENU> all = new List<T_SYS_ENTITYMENU>();
                if (UserFb != null)
                {
                    #region 如果不是预算超级管理员隐藏权限中“预算管理员设置”、预算中的“系统字典维护”2个菜单

                    if (UserFb.ISSUPPERADMIN == "0")
                    {
                        var fbents = ents.Where(p => p.MENUCODE == "T_SYS_FBADMIN");//预算管理员设置
                        var fbsubject = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTTYPE");//系统科目字典维护
                        if (fbents != null)
                        {
                            if (fbents.Count() > 0)
                            {
                                string fbId = fbents.FirstOrDefault().ENTITYMENUID;
                                //menuids.Remove(fbId);
                                ents = ents.Where(p => !fbId.Equals(p.ENTITYMENUID));
                            }
                        }
                        if (fbsubject != null)
                        {
                            if (fbsubject.Count() > 0)
                            {
                                string DictId = fbsubject.FirstOrDefault().ENTITYMENUID;
                                ents = ents.Where(p => !DictId.Equals(p.ENTITYMENUID));
                            }
                        }
                    }

                    #endregion
                }
                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(item =>
                    {
                        item.CHILDSYSTEMNAME ="";
                        item.ENTITYNAME = "";
                        item.ENTITYCODE = "";                        
                        item.CREATEUSER = "";
                        item.UPDATEDATE = null;
                        item.UPDATEUSER = "";
                        item.CREATEDATE = null;
                        item.CREATEUSER = "";
                        item.MENUICONPATH = "";
                        item.REMARK = "";
                        item.URLADDRESS = "";

                        all.Add(item);

                    });
                }
                return all.Count() > 0 ? all.AsQueryable() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据系统类型获取菜单信息,z
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetSysMenuByTypeToFbAdmin(string sysType, string parentID)
        {
            try
            {
                //var ents = from a in ListTemp.AsQueryable()
                //           where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType)
                //           && (string.IsNullOrEmpty(parentID) || (a.T_SYS_ENTITYMENU2 != null && a.T_SYS_ENTITYMENU2.ENTITYMENUID == parentID))
                //           orderby a.ORDERNUMBER

                //           select a;
                //只显示受权限控制的菜单
                IQueryable<T_SYS_ENTITYMENU> ents;
                //string menuIDs = System.Configuration.ConfigurationManager.AppSettings["FbAdminMenus"].ToString();
                ents = from a in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                           where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType)
                           && (string.IsNullOrEmpty(parentID) || (a.T_SYS_ENTITYMENU2 != null && a.T_SYS_ENTITYMENU2.ENTITYMENUID == parentID))
                           //&& a.ISAUTHORITY == "1" && !menuIDs.Contains(a.ENTITYMENUID)
                           && a.ISAUTHORITY == "1" 
                           orderby a.T_SYS_ENTITYMENU2.ENTITYMENUID, a.ORDERNUMBER

                           select a;

                //ents = ents.Where(p => !menuIDs.Contains(p.ENTITYMENUID));
                var fbEnts = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTTYPE");
                string SysDictId = "";
                if (fbEnts != null)
                {
                    if (fbEnts.Count() > 0)
                    {
                        SysDictId = fbEnts.FirstOrDefault().ENTITYMENUID;
                    }
                }
                
                List<T_SYS_ENTITYMENU> all = new List<T_SYS_ENTITYMENU>();
                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(item =>
                    {
                        item.CHILDSYSTEMNAME = "";
                        item.ENTITYNAME = "";
                        item.ENTITYCODE = "";
                        item.CREATEUSER = "";
                        item.UPDATEDATE = null;
                        item.UPDATEUSER = "";
                        item.CREATEDATE = null;
                        item.CREATEUSER = "";
                        item.MENUICONPATH = "";
                        item.REMARK = "";
                        item.URLADDRESS = "";
                        if (SysDictId != item.ENTITYMENUID)
                        {
                            all.Add(item);//过滤预算系统中系统字典维护菜单
                        }
                    });
                }
                return all.Count() > 0 ? all.AsQueryable() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetSysMenuByTypeToLookUp(string sysType, string parentID)
        {
            try
            {
                IQueryable<T_SYS_ENTITYMENU> ents;
                ents = from a in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                       where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType)                          
                       orderby a.ORDERNUMBER
                       select a;
                if (!string.IsNullOrEmpty(parentID))
                {
                    ents = ents.Where(p => p.T_SYS_ENTITYMENU2 != null && p.T_SYS_ENTITYMENU2.ENTITYMENUID == parentID);
                }

                var fbEnts = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTTYPE" || p.MENUCODE == "T_FB_SUBJECTCOMPANYSET");

                List<string> SysDictId = new List<string>();
                if (fbEnts != null)
                {
                    if (fbEnts.Count() > 0)
                    {
                        fbEnts.ToList().ForEach(item =>
                        {
                            SysDictId.Add(item.ENTITYMENUID);
                        });

                    }
                }

                       
                List<T_SYS_ENTITYMENU> all = new List<T_SYS_ENTITYMENU>();
                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(item =>
                    {
                        item.CHILDSYSTEMNAME = "";
                        item.ENTITYNAME = "";
                        item.ENTITYCODE = "";
                        item.CREATEUSER = "";
                        item.UPDATEDATE = null;
                        item.UPDATEUSER = "";
                        item.CREATEDATE = null;
                        item.CREATEUSER = "";
                        item.MENUICONPATH = "";
                        item.REMARK = "";
                        item.URLADDRESS = "";
                        if (!(SysDictId.IndexOf(item.ENTITYMENUID) > -1))
                        {
                            all.Add(item);
                        }

                    });
                }
                return all.Count() > 0 ? all.AsQueryable() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }


        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetSysMenuByTypeToLookUpForNoFbAdmin(string sysType, string parentID)
        {
            try
            {
                IQueryable<T_SYS_ENTITYMENU> ents;
                ents = from a in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                       where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType)
                       orderby a.ORDERNUMBER
                       select a;
                if (!string.IsNullOrEmpty(parentID))
                    ents = ents.Where(p => p.T_SYS_ENTITYMENU2 != null && p.T_SYS_ENTITYMENU2.ENTITYMENUID == parentID);

                //string MenuIds = System.Configuration.ConfigurationManager.AppSettings["FbAdminMenus"].ToString();
                //if (MenuIds.IndexOf(',') > 0)
                //{
                //    string[] Arrids = MenuIds.Split(',');
                //    ents = ents.Where(p => !Arrids.Contains(p.ENTITYMENUID));
                //}
                //var fbEnts = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTTYPE");
                var fbEnts = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTTYPE" || p.MENUCODE == "T_FB_SUBJECTCOMPANYSET");
                
                List<string> SysDictId = new List<string>();
                if (fbEnts != null)
                {
                    if (fbEnts.Count() > 0)
                    {
                        fbEnts.ToList().ForEach(item =>
                        {
                            SysDictId.Add(item.ENTITYMENUID);
                        });

                    }
                }

                

                List<T_SYS_ENTITYMENU> all = new List<T_SYS_ENTITYMENU>();
                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(item =>
                    {
                        item.CHILDSYSTEMNAME = "";
                        item.ENTITYNAME = "";
                        item.ENTITYCODE = "";
                        item.CREATEUSER = "";
                        item.UPDATEDATE = null;
                        item.UPDATEUSER = "";
                        item.CREATEDATE = null;
                        item.CREATEUSER = "";
                        item.MENUICONPATH = "";
                        item.REMARK = "";
                        item.URLADDRESS = "";
                        if (!(SysDictId.IndexOf(item.ENTITYMENUID) >-1) )
                        {
                            all.Add(item);
                        }

                    });
                }
                return all.Count() > 0 ? all.AsQueryable() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

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
        public IQueryable<T_SYS_ENTITYMENU> GetSysMenuByTypeWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID,bool IsFBAdmin)
        {
            try
            {
                var ents = from ent in ListTemp.AsQueryable()
                           

                           select ent;
                if (!IsFBAdmin)
                {
                    string MenuIds = System.Configuration.ConfigurationManager.AppSettings["FbAdminMenus"].ToString();
                    ents = ents.Where(p=> !MenuIds.Contains(p.ENTITYMENUID));
                }
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_ENTITYMENU");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_SYS_ENTITYMENU>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByTypeWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                //
            }

        }
        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetSysMenuNameByTypeInfo(string sysType)
        {
            try
            {
                IQueryable<T_SYS_ENTITYMENU> ents;
                if (string.IsNullOrEmpty(sysType))
                {
                    ents = from a in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                           select a;
                }
                else
                {
                    //ents = from a in ListTemp.AsQueryable()
                    //       where a.SYSTEMTYPE == sysType && !string.IsNullOrEmpty(a.ENTITYNAME) && a.HASSYSTEMMENU == "1" && a.T_SYS_ENTITYMENU2 != null && !string.IsNullOrEmpty(a.URLADDRESS)
                    //       orderby a.T_SYS_ENTITYMENU2.ENTITYMENUID, a.ORDERNUMBER
                    //       select a;
                    //显示授权菜单：根据类型、菜单名不为空、是授权、父ID不为空、受权限的menucode不为空2011-4-22 ljx

                    ents = from a in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                           where a.SYSTEMTYPE == sysType && !string.IsNullOrEmpty(a.ENTITYNAME) && a.ISAUTHORITY == "1" && a.T_SYS_ENTITYMENU2 != null && !string.IsNullOrEmpty(a.MENUCODE)
                           ///orderby a.T_SYS_ENTITYMENU2.ENTITYMENUID, a.ORDERNUMBER
                           select a;

                   
                }
               
                                ;
                List<T_SYS_ENTITYMENU> entityMenu = new List<T_SYS_ENTITYMENU>();
                IQueryable<T_SYS_ENTITYMENU> Resultents = null;
                foreach (var q in ents)
                {
                    T_SYS_ENTITYMENU ent = new T_SYS_ENTITYMENU();
                    ent.MENUNAME = q.MENUNAME;
                    ent.ENTITYMENUID = q.ENTITYMENUID;
                    ent.T_SYS_ENTITYMENU2 = null;
                    ent.SYSTEMTYPE = q.SYSTEMTYPE;
                    ent.ORDERNUMBER = q.ORDERNUMBER;
                    ent.CHILDSYSTEMNAME = "";
                    ent.CREATEDATE = null;
                    ent.CREATEUSER = null;
                    ent.ENTITYCODE = "";
                    ent.HASSYSTEMMENU = "";
                    ent.ISAUTHORITY = "";
                    ent.MENUCODE = "";
                    ent.MENUICONPATH = "";
                    ent.REMARK = "";
                    ent.UPDATEUSER = "";
                    ent.URLADDRESS = "";
                    ent.UPDATEDATE = null;
                    entityMenu.Add(ent);
                }
                

                if (entityMenu != null)
                {
                    Resultents = from ent in entityMenu.AsQueryable()
                              orderby  ent.ORDERNUMBER
                              select ent;
                    
                }
                return Resultents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuNameByTypeInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据系统类型获取菜单信息  修改为显示自定义类
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<V_MenuSetRole> GetSysMenuNameByTypeInfoNew(string sysType)
        {
            try
            {
                IQueryable<T_SYS_ENTITYMENU> ents;
                ents = from a in ListTemp.AsQueryable()
                       select a;

                if (!string.IsNullOrEmpty(sysType))
                {
                    ents = from a in ListTemp.AsQueryable()
                           where a.SYSTEMTYPE == sysType && !string.IsNullOrEmpty(a.MENUNAME) && a.ISAUTHORITY == "1" && a.T_SYS_ENTITYMENU2 != null && !string.IsNullOrEmpty(a.MENUCODE)
                           ///orderby a.T_SYS_ENTITYMENU2.ENTITYMENUID, a.ORDERNUMBER
                           select a;
                }
                
                ;
                List<V_MenuSetRole> entityMenu = new List<V_MenuSetRole>();
                IQueryable<V_MenuSetRole> ResultMenu = null;
                //过滤掉预算中系统科目字典维护菜单
                
                
                var fbEnts = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTTYPE" || p.MENUCODE == "T_FB_SUBJECTCOMPANYSET");
                //var fbSubset = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTCOMPANYSET");//公司科目设置
                List<string> SysDictId = new List<string>();
                if (fbEnts != null)
                {
                    if (fbEnts.Count() > 0)
                    {
                        fbEnts.ToList().ForEach(item =>
                        {
                            SysDictId.Add(item.ENTITYMENUID);
                        });
                        
                    }
                }

                
                
                foreach (var q in ents)
                {
                    V_MenuSetRole ent = new V_MenuSetRole();
                    ent.MENUNAME = q.MENUNAME;
                    ent.ENTITYMENUID = q.ENTITYMENUID;
                    ent.PARENTMENUID = q.T_SYS_ENTITYMENU2.ENTITYMENUID;                    
                    ent.ORDERNUMBER = q.ORDERNUMBER;

                    if (!(SysDictId.IndexOf(ent.ENTITYMENUID) > -1))
                    {
                        entityMenu.Add(ent);
                    }
                }


                if (entityMenu != null)
                {
                    ResultMenu = entityMenu.AsQueryable();
                    ResultMenu = from ent in ResultMenu
                                 orderby ent.PARENTMENUID,ent.ORDERNUMBER
                                 select ent;

                }
                return ResultMenu ;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuNameByTypeInfoNew" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据系统类型获取菜单信息  修改为显示自定义类
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<V_MenuSetRole> GetSysMenuNameByTypeInfoNewToFbAdmins(string sysType)
        {
            try
            {
                IQueryable<T_SYS_ENTITYMENU> ents;
                ents = from a in ListTemp.AsQueryable()
                       select a;
                //需要过滤的菜单ID
                string menuIDs = System.Configuration.ConfigurationManager.AppSettings["FbAdminMenus"].ToString();
                
                if (!string.IsNullOrEmpty(sysType))
                {
                    ents = from a in ListTemp.AsQueryable()
                           where a.SYSTEMTYPE == sysType && !string.IsNullOrEmpty(a.MENUNAME) && a.ISAUTHORITY == "1" && a.T_SYS_ENTITYMENU2 != null && !string.IsNullOrEmpty(a.MENUCODE)
                           
                           ///orderby a.T_SYS_ENTITYMENU2.ENTITYMENUID, a.ORDERNUMBER
                           select a;
                }
                //ents = ents.Where(p => !menuIDs.Contains(p.ENTITYMENUID));
                //隐藏系统科目维护和公司科目设置
                var fbEnts = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTTYPE" || p.MENUCODE == "T_FB_SUBJECTCOMPANYSET");
                //var fbSubset = ents.Where(p => p.MENUCODE == "T_FB_SUBJECTCOMPANYSET");//公司科目设置
                List<string> SysDictId = new List<string>();
                if (fbEnts != null)
                {
                    if (fbEnts.Count() > 0)
                    {
                        fbEnts.ToList().ForEach(item => {
                            SysDictId.Add(item.ENTITYMENUID);
                        });
                        //SysDictId = fbEnts.FirstOrDefault().ENTITYMENUID;
                    }
                }
                
                List<V_MenuSetRole> entityMenu = new List<V_MenuSetRole>();
                IQueryable<V_MenuSetRole> ResultMenu = null;
                foreach (var q in ents)
                {
                    V_MenuSetRole ent = new V_MenuSetRole();
                    ent.MENUNAME = q.MENUNAME;
                    ent.ENTITYMENUID = q.ENTITYMENUID;
                    ent.PARENTMENUID = q.T_SYS_ENTITYMENU2.ENTITYMENUID;
                    ent.ORDERNUMBER = q.ORDERNUMBER;
                    if (!(SysDictId.IndexOf(ent.ENTITYMENUID) >-1))
                    {
                        entityMenu.Add(ent);
                    }
                }


                if (entityMenu != null)
                {
                    ResultMenu = entityMenu.AsQueryable();
                    ResultMenu = from ent in ResultMenu
                                 orderby ent.PARENTMENUID, ent.ORDERNUMBER
                                 select ent;

                }
                return ResultMenu;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuNameByTypeInfoNew" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetSysLeftMenu(string sysType,string userID)
        {
            try
            {
                var ents = from a in this.GetObjects().Include("T_SYS_ENTITYMENU2")//ListTemp.AsQueryable()
                           where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType) && a.HASSYSTEMMENU == "1"
                           orderby a.ORDERNUMBER
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenu" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
            

        }
        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息  2010-6-29
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetSysLeftMenuFilterPermission(string sysType, string userID,ref List<string> menuids)
        {
            try
            {
                var ents = from a in ListTemp.AsQueryable()
                           where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType) && a.HASSYSTEMMENU == "1"
                           orderby a.ORDERNUMBER
                           select a;
                var entspermission = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                                     join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                                     join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                                     join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                     join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                                     join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                     where ur.T_SYS_USER.SYSUSERID == userID &&   n.PERMISSIONVALUE == "3" && m.SYSTEMTYPE == sysType
                                     select new V_Permission
                                     {
                                         RoleMenuPermission = p,
                                         Permission = n,
                                         EntityMenu = m
                                     };
                if (entspermission != null)
                {
                    foreach (var menuid in entspermission.ToList())
                    {
                        menuids.Add(menuid.EntityMenu.ENTITYMENUID);
                    }

                }
                if (menuids.Count() == 0)
                {
                    var UserEnt = from ent in dal.GetObjects<T_SYS_USER>()
                                  where ent.SYSUSERID == userID
                                  select ent;
                    if (UserEnt.FirstOrDefault().ISMANGER == 1)
                    {
                        var SystemEnts = from a in ListTemp.AsQueryable()
                                         where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == "7") && a.HASSYSTEMMENU == "1"
                                         orderby a.ORDERNUMBER
                                         select a;
                        foreach (var menuid in SystemEnts.ToList())
                        {
                            menuids.Add(menuid.ENTITYMENUID);
                        }

                    }
                }
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenuFilterPermission" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }


        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息  2010-6-29
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<V_UserMenuPermission> GetSysLeftMenuFilterPermissionToNewFrame( string userID,T_SYS_FBADMIN FbUser)
        {
            try
            {
                List<string> menuids = new List<string>();
                List<string> Fatherids = new List<string>();//父ID集合
                
                //获取是菜单但不受权限控制的菜单

                IQueryable<V_Permission> entspermissionAll;
                IQueryable<V_Permission>  entspermission = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                                     join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                                     join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                                     join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                     join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                                     join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                     //where ur.T_SYS_USER.SYSUSERID == userID && n.PERMISSIONVALUE == "3" && m.HASSYSTEMMENU == "1"
                                     where ur.T_SYS_USER.SYSUSERID == userID && n.PERMISSIONVALUE == "3" && m.HASSYSTEMMENU =="1" 
                                     select new V_Permission
                                     {
                                         RoleMenuPermission = p,
                                         Permission = n,
                                         EntityMenu = m
                                     };
                //entspermission = from u in dal.GetObjects<T_SYS_USER>()
                //                 join ur in dal.GetObjects<T_SYS_USERROLE>() on u.SYSUSERID equals ur.T_SYS_USER.SYSUSERID
                //                 join r in dal.GetObjects<T_SYS_ROLE>() on ur.T_SYS_ROLE.ROLEID equals r.ROLEID
                //                 join re in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on r.ROLEID equals re.T_SYS_ROLE.ROLEID
                //                 join en in dal.GetObjects<T_SYS_ENTITYMENU>() on re.T_SYS_ENTITYMENU.ENTITYMENUID equals en.ENTITYMENUID
                //                 join rep in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>() on re.ROLEENTITYMENUID equals rep.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID
                //                 join p in dal.GetObjects<T_SYS_PERMISSION>() on rep.T_SYS_PERMISSION.PERMISSIONID equals p.PERMISSIONID                               
                //                 where (u.SYSUSERID == userID && p.PERMISSIONVALUE == "3" && en.HASSYSTEMMENU == "1")
                //                 select new V_Permission
                //                 {
                //                     RoleMenuPermission = rep,
                //                     Permission = p,
                //                     EntityMenu = en
                //                 };

                //IQueryable<V_Permission> Fbentspermission=  //预算管理员设置
                //                    from u in dal.GetObjects<T_SYS_USER>()
                //                    join fa in dal.GetObjects<T_SYS_FBADMIN>() on u.SYSUSERID equals fa.SYSUSERID
                //                    join fr in dal.GetObjects<T_SYS_FBADMINROLE>() on fa.FBADMINID equals fr.T_SYS_FBADMIN.FBADMINID
                //                    join fbr in dal.GetObjects<T_SYS_ROLE>() on fr.ROLEID equals fbr.ROLEID
                //                    join fbre in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on fbr.ROLEID equals fbre.T_SYS_ROLE.ROLEID
                //                    join fben in dal.GetObjects<T_SYS_ENTITYMENU>() on fbre.T_SYS_ENTITYMENU.ENTITYMENUID equals fben.ENTITYMENUID
                //                    join fbrep in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>() on fbre.ROLEENTITYMENUID equals fbrep.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID
                //                    join fbp in dal.GetObjects<T_SYS_PERMISSION>() on fbrep.T_SYS_PERMISSION.PERMISSIONID equals fbp.PERMISSIONID
                //                where (u.SYSUSERID == userID && fbp.PERMISSIONVALUE == "3" && fben.HASSYSTEMMENU == "1")
                //                    select new V_Permission
                //                    {
                //                        RoleMenuPermission = fbrep,
                //                        Permission = fbp,
                //                        EntityMenu = fben
                //                    };
                //entspermissionAll = entspermission.Union(Fbentspermission);
                entspermissionAll = entspermission;
                if (entspermissionAll != null)
                {
                    var entityMenu = (from en in entspermissionAll
                                     select en.EntityMenu.ENTITYMENUID).Distinct().ToList();

                    foreach (var menuid in entityMenu)
                    {
                        menuids.Add(menuid);                        
                    }

                    var q = from ent in menuids
                            where ent=="709D9380-5405-4429-B047-20100401D255"
                            select ent;
                    if (FbUser!=null && q.FirstOrDefault() == null)
                    {
                        Tracer.Debug("菜单预算系统科目设置未获取到,系统用户id：" + FbUser.SYSUSERID);
                    }

                }
                //if (menuids.Count() == 0)
                //{
                // 管理员得权限
                var UserEnt = from ent in dal.GetObjects<T_SYS_USER>()
                                where ent.SYSUSERID == userID
                                select ent;
                decimal? IntIsManager = 0;//是否是权限管理员
                if (UserEnt != null)
                {
                    if (UserEnt.Count() > 0)
                    {
                        var SystemEnts = from a in ListTemp.AsQueryable()
                                         where a.SYSTEMTYPE == "7" && a.HASSYSTEMMENU == "1" && a.ISAUTHORITY == "1"
                                         orderby a.ORDERNUMBER
                                         select a;
                        IntIsManager =UserEnt.FirstOrDefault().ISMANGER;
                        if (IntIsManager == 1)
                        {
                            if (SystemEnts != null)
                            {
                                if (SystemEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                    
                                }
                            }

                        }
                        
                        if (FbUser != null)
                        {
                            #region 如果不是预算超级管理员隐藏权限中“预算管理员设置”、预算中的“系统字典维护”2个菜单

                            if (FbUser.ISSUPPERADMIN == "0")
                            {
                                Tracer.Debug("非超级预算管理员,移除一下菜单：预算管理员，系统科目字典维护，公司科目设置。");
                                var fbents = SystemEnts.Where(p => p.MENUCODE == "T_SYS_FBADMIN");//预算管理员设置
                                var fbsubject = entspermissionAll.Where(p => p.EntityMenu.MENUCODE == "T_FB_SUBJECTTYPE");//系统科目字典维护
                                var fbSubjectSet =  entspermissionAll.Where(p => p.EntityMenu.MENUCODE == "T_FB_SUBJECTCOMPANYSET");//公司科目设置
                                SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient bb = new BLLCommonServices.PersonnelWS.PersonnelServiceClient();
                                //判断当前登陆人是否有下级子公司
                                //bool IsChildCompany = bb.CheckChildCompanyByUserID(UserEnt.FirstOrDefault().EMPLOYEEID);

                                #region 预算管理员                                
                                
                                if (fbents != null)
                                {
                                    if (fbents.Count() > 0)
                                    {
                                        //if (!IsChildCompany)
                                        //{
                                        //    string fbId = fbents.FirstOrDefault().ENTITYMENUID;
                                        //    menuids.Remove(fbId);
                                        //}
                                        //else
                                        //{
                                            string fbId = fbents.FirstOrDefault().ENTITYMENUID;
                                            menuids.Add(fbId);
                                            menuids.Add(fbents.FirstOrDefault().T_SYS_ENTITYMENU2.ENTITYMENUID);
                                        //}
                                    }
                                }

                                #endregion

                                #region 系统科目字典维护                                
                                
                                if (fbsubject != null)
                                {
                                    if (fbsubject.Count() > 0)
                                    {
                                        string DictId = "";
                                        fbsubject.ToList().ForEach(item => {
                                            DictId = item.EntityMenu.ENTITYMENUID;
                                            menuids.Remove(DictId);
                                        });
                                        
                                        
                                    }
                                }

                                #endregion

                                #region 公司科目设置菜单

                                if (fbSubjectSet != null)
                                {
                                    if (fbSubjectSet.Count() > 0)
                                    {
                                        string fbSetId = "";
                                        fbSubjectSet.ToList().ForEach(item =>
                                        {
                                            //if (!IsChildCompany)
                                            //{
                                            //    fbSetId = item.EntityMenu.ENTITYMENUID;
                                            //    menuids.Remove(fbSetId);
                                            //}
                                            //else
                                            //{
                                            fbSetId = item.EntityMenu.ENTITYMENUID;
                                            menuids.Add(fbSetId);
                                            //}
                                        });


                                    }
                                }

                                #endregion
                                
                            }
                            else
                            {
                                #region 如果是超级预算员但又不是权限管理员则添加预算管理员设置菜单

                                if (FbUser.ISSUPPERADMIN == "1" && IntIsManager == 0)
                                {
                                    Tracer.Debug("是超级预算管理员,并且不是系统权限管理员，增加菜单：预算管理员设置。");
                                    var fbents = SystemEnts.Where(p => p.MENUCODE == "T_SYS_FBADMIN");//预算管理员设置                                
                                    if (fbents != null)
                                    {
                                        if (fbents.Count() > 0)
                                        {
                                            string fbId = fbents.FirstOrDefault().ENTITYMENUID;
                                            menuids.Add(fbId);
                                            menuids.Add(fbents.FirstOrDefault().T_SYS_ENTITYMENU2.ENTITYMENUID);
                                        }
                                    }
                                }
                                #endregion
                            }

                            #endregion
                        }
                        //流程权限
                       

                        if (UserEnt.FirstOrDefault().ISFLOWMANAGER == "1")
                        {
                            var SystemFlowsEnts = from a in ListTemp.AsQueryable()
                                                  where a.SYSTEMTYPE == "8" && a.HASSYSTEMMENU == "1"
                                                  orderby a.ORDERNUMBER
                                                  select a;
                            if (SystemFlowsEnts != null)
                            {
                                if (SystemFlowsEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemFlowsEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }

                        }
                        //引擎权限

                        if (UserEnt.FirstOrDefault().ISENGINEMANAGER == "1")
                        {
                            var SystemEngineEnts = from a in ListTemp.AsQueryable()
                                                   where a.SYSTEMTYPE == "9" && a.HASSYSTEMMENU == "1"
                                                   orderby a.ORDERNUMBER
                                                   select a;
                            if (SystemEngineEnts != null)
                            {
                                if (SystemEngineEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemEngineEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }

                        }
                    }
                }
                //获取是父菜单的情况
                
                var ents = from a in ListTemp.AsQueryable()
                           where a.HASSYSTEMMENU == "1" && a.ISAUTHORITY == "0" //&& Fatherids.Contains(a.ENTITYMENUID)
                           orderby a.ORDERNUMBER
                           select a;

                if (ents.Count() > 0 && menuids.Count() >0)
                {
                    //添加所有的非受权限的信息
                    //添加第一级的菜单：父菜单为空
                    var entfirsts = ents.Where(item => item.T_SYS_ENTITYMENU2 == null);
                    if (entfirsts.Count() > 0)
                    {
                        foreach (var first in entfirsts.ToList())
                        {
                            menuids.Add(first.ENTITYMENUID);
                        }
                    }
                    var entchilds = ents.Where(item=>item.T_SYS_ENTITYMENU2 !=null);//取2级或以上的菜单
                    var allchilds = from ent in ListTemp.AsQueryable()
                                    where ent.T_SYS_ENTITYMENU2 != null
                                    select ent;
                    if (entchilds.Count() > 0)
                    {

                        foreach (var menuid in entchilds.ToList())
                        {
                            //menuids.Add(menuid.ENTITYMENUID);//添加父菜单ID
                            //获取某一菜单的子菜单
                            var childmenus = from ent in allchilds //ListTemp.AsQueryable() //dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")// 
                                             where ent.T_SYS_ENTITYMENU2.ENTITYMENUID == menuid.ENTITYMENUID && ent.T_SYS_ENTITYMENU2 != null
                                             select ent;
                            if (childmenus != null)
                            {
                                var entmenus = from ent in childmenus
                                               where menuids.Contains(ent.ENTITYMENUID)
                                               select ent;
                                if (entmenus != null)
                                {
                                    if (entmenus.Count() > 0)
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }
                        }
                    }

                }
                //}
                var  entsPermission = from ent in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                       where menuids.Contains(ent.ENTITYMENUID)
                       select new V_UserMenuPermission { 
                           ENTITYMENUID = ent.ENTITYMENUID,
                           MENUNAME = ent.ENTITYNAME,
                           MENUCODE = ent.ENTITYCODE,
                           MENUICONPATH = ent.MENUICONPATH,
                           ORDERNUMBER = ent.ORDERNUMBER,
                           EntityMenuFatherID = (ent.T_SYS_ENTITYMENU2==null ? "":ent.T_SYS_ENTITYMENU2.ENTITYMENUID),
                           SYSTEMTYPE = ent.SYSTEMTYPE,
                           CHILDSYSTEMNAME = ent.CHILDSYSTEMNAME,
                           URLADDRESS  = ent.URLADDRESS
                           
                       };
                return entsPermission;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenuFilterPermissionToNewFrame" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }



        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息  2011-12-16  非预算管理员使用
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<V_UserMenuPermission> GetSysLeftMenuFilterPermissionToNewFrameForNotFbAdmin(string userID, T_SYS_FBADMIN FbUser)
        {
            try
            {
                List<string> menuids = new List<string>();
                List<string> Fatherids = new List<string>();//父ID集合


                IQueryable<V_Permission> entspermission;
                entspermission = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                                     join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                                     join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                                     join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                     join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                                     join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                     //where ur.T_SYS_USER.SYSUSERID == userID && n.PERMISSIONVALUE == "3" && m.HASSYSTEMMENU == "1"
                                     where ur.T_SYS_USER.SYSUSERID == userID && n.PERMISSIONVALUE == "3" && m.HASSYSTEMMENU == "1"

                                     select new V_Permission
                                     {
                                         RoleMenuPermission = p,
                                         Permission = n,
                                         EntityMenu = m
                                     };
                if (entspermission != null)
                {

                    var q = from ent in entspermission
                            where ent.EntityMenu.ENTITYMENUID == "9b58888d-cf4e-40cf-bab6-de4ee00d0ceb"
                            select ent;
                    if (q.Count() > 0)
                    {
                        //有新流程定义菜单权限
                    }
                    
                    foreach (var menuid in entspermission.ToList())
                    {                      
                        
                        menuids.Add(menuid.EntityMenu.ENTITYMENUID);                        
                        
                    }


                }
                //if (menuids.Count() == 0)
                //{
                // 管理员得权限
                var UserEnt = from ent in dal.GetObjects<T_SYS_USER>()
                              where ent.SYSUSERID == userID
                              select ent;
                decimal? IntIsManager = 0;
                if (UserEnt != null)
                {
                    if (UserEnt.Count() > 0)
                    {

                        var SystemEnts = from a in ListTemp.AsQueryable()
                                         where a.SYSTEMTYPE == "7" && a.HASSYSTEMMENU == "1" && a.ISAUTHORITY == "1"
                                         orderby a.ORDERNUMBER
                                         select a;
                        IntIsManager = UserEnt.FirstOrDefault().ISMANGER;
                        if (IntIsManager == 1)
                        {

                            if (SystemEnts != null)
                            {
                                if (SystemEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }

                                }
                            }

                        }
                        
                        
                        if (FbUser != null)
                        {
                            #region 如果不是预算超级管理员隐藏权限中“预算管理员设置”、预算中的“系统字典维护”2个菜单

                            if (FbUser.ISSUPPERADMIN == "0")
                            {
                                var fbents = SystemEnts.Where(p => p.MENUCODE == "T_SYS_FBADMIN");//预算管理员设置
                                var fbsubject = entspermission.Where(p => p.EntityMenu.MENUCODE == "T_FB_SUBJECTTYPE");//系统科目字典维护
                                var fbSubjectSet = entspermission.Where(p => p.EntityMenu.MENUCODE == "T_FB_SUBJECTCOMPANYSET");//公司科目设置
                                SMT.SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient bb = new BLLCommonServices.PersonnelWS.PersonnelServiceClient();
                                //判断当前登陆人是否有下级子公司
                                //bool IsChildCompany = bb.CheckChildCompanyByUserID(UserEnt.FirstOrDefault().EMPLOYEEID);

                                #region 预算管理员                                
                                
                                //if (fbents != null)
                                //{
                                //    if (fbents.Count() > 0)
                                //    {
                                //        if (!IsChildCompany)
                                //        {
                                //            string fbId = fbents.FirstOrDefault().ENTITYMENUID;
                                //            menuids.Remove(fbId);
                                //        }
                                //    }
                                //}
                                #endregion

                                #region 系统科目字典维护

                                if (fbsubject != null)
                                {
                                    if (fbsubject.Count() > 0)
                                    {
                                        string DictId = "";
                                        fbsubject.ToList().ForEach(item=>{
                                            DictId = item.EntityMenu.ENTITYMENUID;
                                            menuids.Remove(DictId);
                                        });
                                        
                                    }
                                }
                                #endregion

                                #region 公司科目分配菜单

                                //if (fbSubjectSet != null)
                                //{
                                //    if (fbSubjectSet.Count() > 0)
                                //    {
                                //        string fbSetId = "";
                                //        fbSubjectSet.ToList().ForEach(item =>
                                //        {
                                //            if (!IsChildCompany)
                                //            {
                                //                fbSetId = item.EntityMenu.ENTITYMENUID;
                                //                menuids.Remove(fbSetId);
                                //            }
                                            
                                //        });

                                //    }
                                //}

                                #endregion

                            }
                            else
                            {
                                #region 如果是超级预算员但又不是权限管理员则添加预算管理员设置菜单

                                if (FbUser.ISSUPPERADMIN == "1" && IntIsManager == 0)
                                {
                                    var fbents = SystemEnts.Where(p => p.MENUCODE == "T_SYS_FBADMIN");//预算管理员设置                                
                                    if (fbents != null)
                                    {
                                        if (fbents.Count() > 0)
                                        {
                                            string fbId = fbents.FirstOrDefault().ENTITYMENUID;
                                            menuids.Add(fbId);
                                            menuids.Add(fbents.FirstOrDefault().T_SYS_ENTITYMENU2.ENTITYMENUID);//添加父菜单
                                        }
                                    }
                                }
                                #endregion
                            }

                            #endregion
                        }

                        //流程权限
                        if (UserEnt.FirstOrDefault().ISFLOWMANAGER == "1")
                        {
                            var SystemFlowsEnts = from a in ListTemp.AsQueryable()
                                                  where a.SYSTEMTYPE == "8" && a.HASSYSTEMMENU == "1"
                                                  orderby a.ORDERNUMBER
                                                  select a;
                            if (SystemFlowsEnts != null)
                            {
                                if (SystemFlowsEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemFlowsEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }

                        }
                        //引擎权限

                        if (UserEnt.FirstOrDefault().ISENGINEMANAGER == "1")
                        {
                            var SystemEngineEnts = from a in ListTemp.AsQueryable()
                                                   where a.SYSTEMTYPE == "9" && a.HASSYSTEMMENU == "1"
                                                   orderby a.ORDERNUMBER
                                                   select a;
                            if (SystemEngineEnts != null)
                            {
                                if (SystemEngineEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemEngineEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }

                        }
                    }
                }
                //获取是父菜单的情况

                var ents = from a in ListTemp.AsQueryable()
                           where a.HASSYSTEMMENU == "1" && a.ISAUTHORITY == "0" //&& Fatherids.Contains(a.ENTITYMENUID)
                           orderby a.ORDERNUMBER
                           select a;

                if (ents.Count() > 0 && menuids.Count() > 0)
                {
                    //添加所有的非受权限的信息
                    //添加第一级的菜单：父菜单为空
                    var entfirsts = ents.Where(item => item.T_SYS_ENTITYMENU2 == null);
                    if (entfirsts.Count() > 0)
                    {
                        foreach (var first in entfirsts.ToList())
                        {
                            menuids.Add(first.ENTITYMENUID);
                        }
                    }
                    var entchilds = ents.Where(item => item.T_SYS_ENTITYMENU2 != null);//取2级或以上的菜单
                    var allchilds = from ent in ListTemp.AsQueryable()
                                    where ent.T_SYS_ENTITYMENU2 != null
                                    select ent;
                    if (entchilds.Count() > 0)
                    {

                        foreach (var menuid in entchilds.ToList())
                        {
                            //menuids.Add(menuid.ENTITYMENUID);//添加父菜单ID
                            //获取某一菜单的子菜单
                            var childmenus = from ent in allchilds //ListTemp.AsQueryable() //dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")// 
                                             where ent.T_SYS_ENTITYMENU2.ENTITYMENUID == menuid.ENTITYMENUID && ent.T_SYS_ENTITYMENU2 != null
                                             select ent;
                            if (childmenus != null)
                            {
                                var entmenus = from ent in childmenus
                                               where menuids.Contains(ent.ENTITYMENUID)
                                               select ent;
                                if (entmenus != null)
                                {
                                    if (entmenus.Count() > 0)
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }
                        }
                    }

                }
                //}
                List<string> Newmenuids = new List<string>();
                //string FBmenuIDs = System.Configuration.ConfigurationManager.AppSettings["FbAdminMenus"].ToString();
                string fbAdminMenu = System.Configuration.ConfigurationManager.AppSettings["PMFbAdminMenus"].ToString();
                //string[] ArrIDs = FBmenuIDs.Split(',');
                
                if (menuids.Count() > 0)
                {
                    for (int i = 0; i < menuids.Count(); i++)
                    {
                        
                        if (!(fbAdminMenu.Contains(menuids[i])))
                        {
                            Newmenuids.Add(menuids[i]);
                            //menuids.Remove(menuids[i]);
                        }
                    }
                }
                
                var entsPermission = from ent in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                                     where Newmenuids.Contains(ent.ENTITYMENUID)
                                     select new V_UserMenuPermission
                                     {
                                         ENTITYMENUID = ent.ENTITYMENUID,
                                         MENUNAME = ent.ENTITYNAME,
                                         MENUCODE = ent.ENTITYCODE,
                                         MENUICONPATH = ent.MENUICONPATH,
                                         ORDERNUMBER = ent.ORDERNUMBER,
                                         EntityMenuFatherID = (ent.T_SYS_ENTITYMENU2 == null ? "" : ent.T_SYS_ENTITYMENU2.ENTITYMENUID),
                                         SYSTEMTYPE = ent.SYSTEMTYPE,
                                         CHILDSYSTEMNAME = ent.CHILDSYSTEMNAME,
                                         URLADDRESS = ent.URLADDRESS

                                     };

                return entsPermission;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenuFilterPermissionToNewFrame" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }





        /// <summary>
        /// 根据用户ID和权限值获取相应的权限信息
        /// </summary>
        /// <param name="userID">系统用户ID</param>
        /// <param name="PermisionValue">权限值 1 2 3 4</param>
        /// <returns></returns>
        public IQueryable<V_UserMenuPermission> GetSysLeftMenuFilterPermissionToNewFrameAndPermision(string userID,string PermisionValue)
        {
            try
            {
                List<string> menuids = new List<string>();
                List<string> Fatherids = new List<string>();//父ID集合

               
                var entspermission = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                                     join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                                     join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                                     join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                     join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                                     join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                     //where ur.T_SYS_USER.SYSUSERID == userID && n.PERMISSIONVALUE == "3" && m.HASSYSTEMMENU == "1"
                                     where ur.T_SYS_USER.SYSUSERID == userID && n.PERMISSIONVALUE == PermisionValue && m.HASSYSTEMMENU == "1"

                                     select new V_Permission
                                     {
                                         RoleMenuPermission = p,
                                         Permission = n,
                                         EntityMenu = m
                                     };
                if (entspermission != null)
                {
                    foreach (var menuid in entspermission.ToList())
                    {
                        menuids.Add(menuid.EntityMenu.ENTITYMENUID);
                        
                    }


                }
                
                var UserEnt = from ent in dal.GetObjects<T_SYS_USER>()
                              where ent.SYSUSERID == userID
                              select ent;
                if (UserEnt != null)
                {
                    if (UserEnt.Count() > 0)
                    {
                        if (UserEnt.FirstOrDefault().ISMANGER == 1)
                        {
                            var SystemEnts = from a in ListTemp.AsQueryable()
                                             where a.SYSTEMTYPE == "7" && a.HASSYSTEMMENU == "1" && a.ISAUTHORITY == "1"
                                             orderby a.ORDERNUMBER
                                             select a;
                            if (SystemEnts != null)
                            {
                                if (SystemEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }

                        }
                        //流程权限

                        if (UserEnt.FirstOrDefault().ISFLOWMANAGER == "1")
                        {
                            var SystemFlowsEnts = from a in ListTemp.AsQueryable()
                                                  where a.SYSTEMTYPE == "8" && a.HASSYSTEMMENU == "1"
                                                  orderby a.ORDERNUMBER
                                                  select a;
                            if (SystemFlowsEnts != null)
                            {
                                if (SystemFlowsEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemFlowsEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }

                        }
                        //引擎权限

                        if (UserEnt.FirstOrDefault().ISENGINEMANAGER == "1")
                        {
                            var SystemEngineEnts = from a in ListTemp.AsQueryable()
                                                   where a.SYSTEMTYPE == "9" && a.HASSYSTEMMENU == "1"
                                                   orderby a.ORDERNUMBER
                                                   select a;
                            if (SystemEngineEnts != null)
                            {
                                if (SystemEngineEnts.Count() > 0)
                                {
                                    foreach (var menuid in SystemEngineEnts.ToList())
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }

                        }
                    }
                }
                //获取是父菜单的情况

                var ents = from a in ListTemp.AsQueryable()
                           where a.HASSYSTEMMENU == "1" && a.ISAUTHORITY == "0" //&& Fatherids.Contains(a.ENTITYMENUID)
                           orderby a.ORDERNUMBER
                           select a;

                if (ents.Count() > 0 && menuids.Count() > 0)
                {
                    //添加所有的非受权限的信息
                    //添加第一级的菜单：父菜单为空
                    var entfirsts = ents.Where(item => item.T_SYS_ENTITYMENU2 == null);
                    if (entfirsts.Count() > 0)
                    {
                        foreach (var first in entfirsts.ToList())
                        {
                            menuids.Add(first.ENTITYMENUID);
                        }
                    }
                    var entchilds = ents.Where(item => item.T_SYS_ENTITYMENU2 != null);//取2级或以上的菜单
                    var allchilds = from ent in ListTemp.AsQueryable()
                                    where ent.T_SYS_ENTITYMENU2 != null
                                    select ent;
                    if (entchilds.Count() > 0)
                    {

                        foreach (var menuid in entchilds.ToList())
                        {
                            //menuids.Add(menuid.ENTITYMENUID);//添加父菜单ID
                            //获取某一菜单的子菜单
                            var childmenus = from ent in allchilds //ListTemp.AsQueryable() //dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")// 
                                             where ent.T_SYS_ENTITYMENU2.ENTITYMENUID == menuid.ENTITYMENUID && ent.T_SYS_ENTITYMENU2 != null
                                             select ent;
                            if (childmenus != null)
                            {
                                var entmenus = from ent in childmenus
                                               where menuids.Contains(ent.ENTITYMENUID)
                                               select ent;
                                if (entmenus != null)
                                {
                                    if (entmenus.Count() > 0)
                                    {
                                        menuids.Add(menuid.ENTITYMENUID);
                                    }
                                }
                            }
                        }
                    }

                }
                //}
                var entsPermission = from ent in dal.GetObjects<T_SYS_ENTITYMENU>().Include("T_SYS_ENTITYMENU2")
                                     where menuids.Contains(ent.ENTITYMENUID)
                                     select new V_UserMenuPermission
                                     {
                                         ENTITYMENUID = ent.ENTITYMENUID,
                                         MENUNAME = ent.ENTITYNAME,
                                         MENUCODE = ent.ENTITYCODE,
                                         MENUICONPATH = ent.MENUICONPATH,
                                         ORDERNUMBER = ent.ORDERNUMBER,
                                         EntityMenuFatherID = (ent.T_SYS_ENTITYMENU2 == null ? "" : ent.T_SYS_ENTITYMENU2.ENTITYMENUID),
                                         SYSTEMTYPE = ent.SYSTEMTYPE,
                                         CHILDSYSTEMNAME = ent.CHILDSYSTEMNAME,
                                         URLADDRESS = ent.URLADDRESS

                                     };
                return entsPermission;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenuFilterPermissionToNewFrame" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }



        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息  2010-6-29  专门针对权限系统做处理
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetSysLeftMenuFilterPermissionToPermission(string userID, ref List<string> menuids)
        {
            try
            {
                SysUserBLL userBll = new SysUserBLL();
                T_SYS_USER user = new T_SYS_USER();
                user = userBll.GetUserByID(userID);
                if (user == null)
                    return null;
                string flowCode = "";//流程代码
                string engineCode = "";//引擎代码
                if (user.ISFLOWMANAGER == "1")
                    flowCode = "4";
                if (user.ISENGINEMANAGER == "1")
                    engineCode = "5";
                var ents = from a in ListTemp.AsQueryable()
                           where a.SYSTEMTYPE == "7" && a.HASSYSTEMMENU == "1" || ((!string.IsNullOrEmpty(flowCode) && a.SYSTEMTYPE == flowCode)) || ((!string.IsNullOrEmpty(engineCode) && a.SYSTEMTYPE == engineCode))
                           orderby a.ORDERNUMBER
                           select a;
                var entspermission = from p in dal.GetObjects<T_SYS_ROLEMENUPERMISSION>().Include("T_SYS_ROLEENTITYMENU")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                                     join e in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on p.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID equals e.ROLEENTITYMENUID
                                     join n in dal.GetObjects<T_SYS_PERMISSION>() on p.T_SYS_PERMISSION.PERMISSIONID equals n.PERMISSIONID
                                     join m in dal.GetObjects<T_SYS_ENTITYMENU>() on e.T_SYS_ENTITYMENU.ENTITYMENUID equals m.ENTITYMENUID
                                     join r in dal.GetObjects<T_SYS_ROLE>() on e.T_SYS_ROLE.ROLEID equals r.ROLEID
                                     join ur in dal.GetObjects<T_SYS_USERROLE>() on r.ROLEID equals ur.T_SYS_ROLE.ROLEID
                                     where ur.T_SYS_USER.SYSUSERID == userID && n.PERMISSIONVALUE == "3" && (m.SYSTEMTYPE == "7" || (!string.IsNullOrEmpty(flowCode) && m.SYSTEMTYPE == flowCode) || (!string.IsNullOrEmpty(engineCode) && m.SYSTEMTYPE == engineCode)) 
                                     select new V_Permission
                                     {
                                         RoleMenuPermission = p,
                                         Permission = n,
                                         EntityMenu = m
                                     };
                if (entspermission != null)
                {
                    foreach (var menuid in entspermission.ToList())
                    {
                        menuids.Add(menuid.EntityMenu.ENTITYMENUID);
                    }

                }
                if (menuids.Count() == 0)
                {
                    var UserEnt = from ent in dal.GetObjects<T_SYS_USER>()
                                  where ent.SYSUSERID == userID
                                  select ent;
                    if (UserEnt.FirstOrDefault().ISMANGER == 1)
                    {
                        var SystemEnts = from a in ListTemp.AsQueryable()
                                         where a.HASSYSTEMMENU == "1" && (a.SYSTEMTYPE == "7" || (!string.IsNullOrEmpty(flowCode) && a.SYSTEMTYPE == flowCode) || (!string.IsNullOrEmpty(engineCode) && a.SYSTEMTYPE == engineCode))
                                         orderby a.ORDERNUMBER
                                         select a;
                        foreach (var menuid in SystemEnts.ToList())
                        {
                            menuids.Add(menuid.ENTITYMENUID);
                        }

                    }
                }
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenuFilterPermission" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 登录用户获取菜单
        /// </summary>
        /// <param name="sysType">系统类型</param>
        /// <param name="userID">用户ID</param>
        /// <param name="IsChanged">判断权限是否改动</param>
        /// <returns></returns>
        public List<V_EntityMenu> GetEntityMenuByUser(string sysType, string userID, ref string IsChanged)
        {


            try
            {

                //            string sql = @"select  menu.entitymenuid,menu.superiorid,menu.menuname,menu.menuiconpath,menu.urladdress,menu.ordernumber,menu.systemtype,menu.childsystemname,case when userMenu.Menuname is null then '0' else '1' end as CanRead";
                //            sql += @" from  t_sys_entitymenu menu left join (select distinct u.employeename,me.menuname,me.entitymenuid 
                //                      from 
                //                      t_sys_user u
                //                      inner join t_sys_userrole ur on u.sysuserid=ur.sysuserid
                //                      inner join t_sys_role r on ur.roleid=r.roleid
                //                      inner join t_sys_roleentitymenu rm on r.roleid=rm.roleid
                //                      inner join t_sys_rolemenupermission rmp on rm.roleentitymenuid=rmp.roleentitymenuid
                //                      inner join t_sys_entitymenu me on rm.entitymenuid=me.entitymenuid";
                //            sql += @" where  me.systemtype='" + sysType + "'" + " and u.sysuserid='" + userID + "'" + @") userMenu";
                //            sql += @" on menu.entitymenuid=userMenu.entitymenuid     where menu.hassystemmenu='1' and menu.systemtype='" + sysType + "'";
                string sql = @"select  menu.entitymenuid,menu.menucode,menu.superiorid,menu.menuname,menu.menuiconpath,menu.urladdress,menu.ordernumber,menu.systemtype,menu.childsystemname,case when userMenu.Menuname is null then '0' else '1' end as CanRead";
                sql += @" from  t_sys_entitymenu menu left join (SELECT DISTINCT U.EMPLOYEENAME,ME.MENUNAME,ME.ENTITYMENUID 
                    from t_sys_user u
                    inner join t_sys_userrole ur on u.sysuserid=ur.sysuserid
                    inner join t_sys_role r on ur.roleid=r.roleid
                    inner join t_sys_entitymenucustomperm cm on r.roleid = cm.roleid 
                    inner join t_sys_entitymenu me on CM.entitymenuid=me.entitymenuid ";
                sql += @" where  me.systemtype='" + sysType + "' " + " and u.sysuserid='" + userID + "'";
                sql += @" UNION select distinct u.employeename,me.menuname,me.entitymenuid   
                    from t_sys_user u
                    inner join t_sys_userrole ur on u.sysuserid=ur.sysuserid
                    inner join t_sys_role r on ur.roleid=r.roleid
                    inner join t_sys_roleentitymenu rm on r.roleid=rm.roleid
                    inner join t_sys_rolemenupermission rmp on rm.roleentitymenuid=rmp.roleentitymenuid
                    inner join t_sys_entitymenu me on rm.entitymenuid=me.entitymenuid ";
                sql += @" where  me.systemtype='" + sysType + "' " + " and u.sysuserid='" + userID + "') userMenu";
                sql += @" on menu.entitymenuid=userMenu.entitymenuid     where menu.hassystemmenu='1' and menu.systemtype='" + sysType + "'";
                DataTable value = (DataTable)dal.ExecuteCustomerSql(sql);
                List<V_EntityMenu> ents = (from ent in value.AsEnumerable()
                                           select new V_EntityMenu
                                           {
                                               ENTITYMENUID = ent["entitymenuid"].ToString(),
                                               MENUCODE = ent["menucode"].ToString(),
                                               MENUNAME = ent["menuname"].ToString(),
                                               EntityMenuFatherID = ent["superiorid"].ToString(),
                                               MENUICONPATH = ent["menuiconpath"].ToString(),
                                               URLADDRESS = ent["urladdress"].ToString(),
                                               ORDERNUMBER = System.Convert.ToDecimal(ent["ordernumber"]),
                                               SYSTEMTYPE = ent["systemtype"].ToString(),
                                               CHILDSYSTEMNAME = ent["childsystemname"].ToString(),
                                               CanRead = ent["CanRead"].ToString()
                                           }).ToList();
                //返回只读的菜单
                ents = (from ent in ents
                        where ent.CanRead == "1"
                        select ent).ToList();


                IsChanged = "1";//以后再做验证
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetEntityMenuByUser" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 登录用户获取菜单 主要针对权限
        /// </summary>
        /// <param name="sysType">系统类型</param>
        /// <param name="userID">用户ID</param>
        /// <param name="IsChanged">判断权限是否改动</param>
        /// <returns></returns>
        public List<V_EntityMenu> GetEntityMenuByUsertoPermission(string sysType, string userID, ref string IsChanged)
        {


            try
            {

                SysUserBLL userBll = new SysUserBLL();
                T_SYS_USER user = new T_SYS_USER();
                user = userBll.GetUserByID(userID);
                if (user == null)
                    return null;
                string flowCode = "";//流程代码
                string engineCode = "";//引擎代码
                if (user.ISFLOWMANAGER == "1")
                    flowCode = "4";
                if (user.ISENGINEMANAGER == "1")
                    engineCode = "5";
                string sql = @"select  menu.entitymenuid,menu.menucode,menu.superiorid,menu.menuname,menu.menuiconpath,menu.urladdress,menu.ordernumber,menu.systemtype,menu.childsystemname,case when userMenu.Menuname is null then '0' else '1' end as CanRead";
                sql += @" from  t_sys_entitymenu menu left join (SELECT DISTINCT U.EMPLOYEENAME,ME.MENUNAME,ME.ENTITYMENUID 
                    from t_sys_user u
                    inner join t_sys_userrole ur on u.sysuserid=ur.sysuserid
                    inner join t_sys_role r on ur.roleid=r.roleid
                    inner join t_sys_entitymenucustomperm cm on r.roleid = cm.roleid 
                    inner join t_sys_entitymenu me on CM.entitymenuid=me.entitymenuid ";
                sql += @" where  me.systemtype='" + sysType + "' " + " and u.sysuserid='" + userID + "'";
                if (!string.IsNullOrEmpty(flowCode))
                    sql += @" or me.systemtype='" + flowCode + "' ";
                if (!string.IsNullOrEmpty(engineCode))
                    sql += @" or me.systemtype='" + engineCode + "' ";
                sql += @" UNION select distinct u.employeename,me.menuname,me.entitymenuid   
                    from t_sys_user u
                    inner join t_sys_userrole ur on u.sysuserid=ur.sysuserid
                    inner join t_sys_role r on ur.roleid=r.roleid
                    inner join t_sys_roleentitymenu rm on r.roleid=rm.roleid
                    inner join t_sys_rolemenupermission rmp on rm.roleentitymenuid=rmp.roleentitymenuid
                    inner join t_sys_entitymenu me on rm.entitymenuid=me.entitymenuid ";
                sql += @" where  me.systemtype='" + sysType + "' ";
                if (!string.IsNullOrEmpty(flowCode))
                    sql += @" or me.systemtype='" + flowCode + "' ";
                if (!string.IsNullOrEmpty(engineCode))
                    sql += @" or me.systemtype='" + engineCode + "' ";
                sql += @" and u.sysuserid='" + userID + "') userMenu";
                sql += @" on menu.entitymenuid=userMenu.entitymenuid     where menu.hassystemmenu='1' and menu.systemtype='" + sysType + "'";
                if (!string.IsNullOrEmpty(flowCode))
                    sql += @" or menu.systemtype='" + flowCode + "' ";
                if (!string.IsNullOrEmpty(engineCode))
                    sql += @" or menu.systemtype='" + engineCode + "' ";
                DataTable value = (DataTable)dal.ExecuteCustomerSql(sql);
                List<V_EntityMenu> ents = (from ent in value.AsEnumerable()
                                           select new V_EntityMenu
                                           {
                                               ENTITYMENUID = ent["entitymenuid"].ToString(),
                                               MENUCODE = ent["menucode"].ToString(),
                                               MENUNAME = ent["menuname"].ToString(),
                                               EntityMenuFatherID = ent["superiorid"].ToString(),
                                               MENUICONPATH = ent["menuiconpath"].ToString(),
                                               URLADDRESS = ent["urladdress"].ToString(),
                                               ORDERNUMBER = System.Convert.ToDecimal(ent["ordernumber"]),
                                               SYSTEMTYPE = "7",//重新设置为7  在平台中只认 7作为主菜单
                                               CHILDSYSTEMNAME = ent["childsystemname"].ToString(),
                                               CanRead = ent["CanRead"].ToString()
                                           }).ToList();
                //返回只读的菜单
                ents = (from ent in ents
                        where ent.CanRead == "1"
                        select ent).ToList();


                IsChanged = "1";//以后再做验证
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetEntityMenuByUser" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取所有的菜单 2010-12-03 
        /// </summary>
        
        /// <returns></returns>
        public List<V_EntityMenu> GetEntityMenuAll()
        {
            try
            {
                var EntityMenus = from a in dal.GetObjects().Include("T_SYS_ENTITYMENU2")
                                  where a.HASSYSTEMMENU == "1"
                                  orderby a.ORDERNUMBER
                                  select a;
                if (EntityMenus.Count() > 0)
                {
                    List<V_EntityMenu> ents = (from ent in EntityMenus
                                               select new V_EntityMenu
                                               {
                                                   ENTITYMENUID = ent.ENTITYMENUID,
                                                   MENUCODE = ent.MENUCODE,
                                                   MENUNAME = ent.MENUNAME,
                                                   EntityMenuFatherID = ent.T_SYS_ENTITYMENU2.ENTITYMENUID,
                                                   MENUICONPATH = ent.MENUICONPATH,
                                                   URLADDRESS = ent.URLADDRESS,
                                                   ORDERNUMBER = ent.ORDERNUMBER,
                                                   SYSTEMTYPE = (ent.SYSTEMTYPE == "4" || ent.SYSTEMTYPE=="5" ) ? "7":ent.SYSTEMTYPE,
                                                   CHILDSYSTEMNAME = ent.CHILDSYSTEMNAME,
                                                   CanRead = "0"
                                               }).ToList();
                    return ents;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetEntityMenuAll" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
            
        }



        public IQueryable<T_SYS_ENTITYMENU> GetSysLeftMenutt(string sysType, string userID)
        {
            try
            {
                var tmps = from a in ListTemp.AsQueryable()
                           join b in dal.GetObjects<T_SYS_ROLEENTITYMENU>() on a.ENTITYMENUID equals b.T_SYS_ENTITYMENU.ENTITYMENUID
                           join c in dal.GetObjects<T_SYS_USERROLE>() on b.T_SYS_ROLE.ROLEID equals c.T_SYS_ROLE.ROLEID
                           join d in dal.GetObjects<T_SYS_USER>() on c.T_SYS_USER equals d
                           where a.SYSTEMTYPE == sysType && d.EMPLOYEEID == userID
                           select a;


                return tmps;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenutt" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        

        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public List<T_SYS_ENTITYMENU> GetSysMenuInfosByParentID(string parentID)
        {
            try
            {
                var ents = from a in ListTemp.AsQueryable()
                           //where a = parentID
                           where a.T_SYS_ENTITYMENU2.ENTITYMENUID == parentID
                           orderby a.ORDERNUMBER
                           select a;
                return ents.ToList();
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuInfosByParentID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据菜单ID获取菜单信息
        /// </summary>
        /// <param name="menuID">菜单ID</param>
        /// <returns>菜单信息</returns>
        public T_SYS_ENTITYMENU GetSysMenuByID(string menuID)
        {
            try
            {
                var ents = from ent in ListTemp.AsQueryable()
                           where ent.ENTITYMENUID == menuID
                           select ent;
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据菜单ID获取菜单信息
        /// </summary>
        /// <param name="menuID">菜单ID</param>
        /// <returns>菜单信息</returns>
        public T_SYS_ENTITYMENU GetSysMenuByIDToCustomer(string menuID)
        {
            try
            {
                T_SYS_ENTITYMENU lsdic;
                string keyString = "GetSysMenuByIDToCustomer" + menuID;
                if (CacheManager.GetCache(keyString) != null)
                {
                    lsdic = (T_SYS_ENTITYMENU)CacheManager.GetCache(keyString);
                }
                else
                {
                    var ents = from ent in ListTemp.AsQueryable()
                               where ent.ENTITYMENUID == menuID
                               select ent;
                    lsdic = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    CacheManager.AddCache(keyString, lsdic);
                }


                return lsdic;

                
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据实体名称获取菜单信息
        /// </summary>
        /// <param name="sysType">菜单ID</param>
        /// <returns>菜单信息</returns>
        public T_SYS_ENTITYMENU GetSysMenuByEntityCode(string entityCode)
        {
            try
            {
                var ents = from ent in ListTemp.AsQueryable()
                           where ent.ENTITYCODE == entityCode
                           select ent;

                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByEntityCode" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 根据实体名称获取菜单信息
        /// </summary>
        /// <param name="sysType">菜单ID</param>
        /// <returns>菜单信息</returns>
        public T_SYS_ENTITYMENU GetSysMenuByEntityName(string entityName)
        {
            try
            {
                var ents = from ent in ListTemp.AsQueryable()
                           where ent.ENTITYNAME == entityName
                           select ent;

                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysMenuByEntityName" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 修改系统菜单
        /// </summary>
        /// <param name="entity">被修改的菜单的实体</param>
        public void SysMenuUpdate(T_SYS_ENTITYMENU entity)
        {
            try
            {
                var ents = from ent in dal.GetTable()
                           where ent.ENTITYMENUID == entity.ENTITYMENUID
                           select ent;
                
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    ent.T_SYS_ENTITYMENU2 = entity.T_SYS_ENTITYMENU2;
                    //ent.T_SYS_ENTITYMENU2.EntityKey = entity.T_SYS_ENTITYMENU2.EntityKey;
                    Utility.CloneEntity<T_SYS_ENTITYMENU>(entity, ent);
                    
                    if (entity.T_SYS_ENTITYMENU2 != null)
                    {
                        ent.T_SYS_ENTITYMENU2 = entity.T_SYS_ENTITYMENU2;
                        //ent.T_SYS_ENTITYMENU2Reference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", entity.T_SYS_ENTITYMENU2.ENTITYMENUID);
                        //T_OA_LENDARCHIVES tmpobj = archivesContext.GetObjectByKey(archivesObj.EntityKey) as T_OA_LENDARCHIVES;
                        //tmpobj.T_OA_ARCHIVES = archivesContext.GetObjectByKey(archivesObj.T_OA_ARCHIVES.EntityKey) as T_OA_ARCHIVES;
                        //archivesContext.ApplyPropertyChanges(archivesObj.EntityKey.EntitySetName, archivesObj);
                    }
                    else
                        ent.T_SYS_ENTITYMENU2Reference = null;


                    int i=dal.Update(ent);
                    if (i > 0)
                    {
                        CacheManager.RemoveCache("T_SYS_ENTITYMENU");
                        //CacheManager.RemoveCache("GetSysMenuByID"+ent.ENTITYMENUID);
                        new SysDictionaryBLL().EditVersion("菜单");//修改菜单版本
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-SysMenuUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //
            }
        }

        /// <summary>
        /// 删除系统菜单
        /// </summary>
        /// <param name="menuID">菜单ID</param>
        /// <returns>是否删除成功</returns>
        public string SysMenuDelete(string menuID)
        {
            string StrReturn = "";
            //在权限菜单表中存在的不能删除
            //
            try
            {
                CommDAL<T_SYS_ROLEENTITYMENU> tmpdal = new CommDAL<T_SYS_ROLEENTITYMENU>();
                var tmpEnts = (from ent in tmpdal.GetTable()
                               where ent.T_SYS_ENTITYMENU.ENTITYMENUID == menuID
                               select ent);
                if (tmpEnts.Count() > 0)
                {
                    //TODO:多语言与自定义异常
                    return StrReturn = "MENUISUSEDPLEASEDELEETEOTHER";
                    //throw new Exception("此菜单已关联权限，请先删除权限菜单关联！");
                }

                var entitys = (from ent in dal.GetTable()
                               where ent.ENTITYMENUID == menuID
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    int i=dal.Delete(entity);
                    if (i > 0)
                    {
                        CacheManager.RemoveCache("T_SYS_ENTITYMENU");
                        new SysDictionaryBLL().EditVersion("菜单");//修改菜单版本
                    }
                    else
                    {
                        StrReturn="ERROR";
                    }
                    
                }
                return StrReturn;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-SysMenuDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "SYSTEMERRORPLEASELINKDADMIN";//数据库错误，请联系管理员
                
            }
        }

        public string SysMenuAdd(T_SYS_ENTITYMENU sourceEntity)
        {
            //throw new NotImplementedException();
            string StrReturn="";
            try
            {
                var Entitys = from ents in dal.GetObjects<T_SYS_ENTITYMENU>()
                              where ents.ENTITYCODE == sourceEntity.ENTITYCODE && ents.MENUNAME == sourceEntity.MENUNAME && ents.SYSTEMTYPE == sourceEntity.SYSTEMTYPE
                              select ents;
                if(Entitys.Count() >0)
                {
                    StrReturn = "REPEATBASEDATANAME";
                    return StrReturn;
                }
                    
                T_SYS_ENTITYMENU ent = new T_SYS_ENTITYMENU();
                ent.ENTITYMENUID = sourceEntity.ENTITYMENUID;

                Utility.CloneEntity<T_SYS_ENTITYMENU>(sourceEntity, ent);

                if (sourceEntity.T_SYS_ENTITYMENU2 != null && !string.IsNullOrEmpty(sourceEntity.T_SYS_ENTITYMENU2.ENTITYMENUID))
                {
                    ent.T_SYS_ENTITYMENU2Reference.EntityKey =
                        new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_ENTITYMENU", "ENTITYMENUID", sourceEntity.T_SYS_ENTITYMENU2.ENTITYMENUID);
                }

                int i =dal.Add(ent);
                if (!(i > 0))
                    StrReturn = "ERROR";
                CacheManager.RemoveCache("T_SYS_ENTITYMENU");
                new SysDictionaryBLL().EditVersion("菜单");//修改菜单版本
                return StrReturn;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-SysMenuAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "ERROR";
                //
            }
        }


        /// <summary>
        /// 根据菜单ID集合 和所属 系统类型  获取相应的 菜单集合
        /// </summary>
        /// <param name="MenuIDs"></param>
        /// <param name="Systype"></param>
        /// <returns></returns>
        public IQueryable<T_SYS_ENTITYMENU> GetEntityMenuByMenuIDs(string[] MenuIDs,string Systype)
        {
            try
            {
                var ents = from ent in dal.GetObjects()
                           //where ent.SYSTEMTYPE == Systype
                           select ent;
                if (!string.IsNullOrEmpty(Systype))
                    ents = ents.Where(p => p.SYSTEMTYPE == Systype);
                ents = ents.Where(p => MenuIDs.Contains(p.ENTITYMENUID));
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetEntityMenuByMenuIDs" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }



        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        public IQueryable<T_SYS_ENTITYMENU> GetCustomerPermissionMenus(string employeeid)
        {
            try
            {
                //ConfigurationSettings.GetConfig("CustomerPermissionInfo/UserID");

                List<string> MenuCodes = new List<string>();
                //主要添加公司信息、部门信息、岗位信息、薪资体系、薪资方案、薪资项、地区差异补贴、城市地区分类
                //考勤方案定义、考勤方案设置
                //考勤设置所有菜单
                
                //MenuCodes.Add("T_HR_COMPANY");
                //MenuCodes.Add("T_HR_DEPARTMENT");
                //MenuCodes.Add("T_HR_POST");//岗位信息
                //MenuCodes.Add("T_HR_SALARYSYSTEM");//薪资体系
                //MenuCodes.Add("T_HR_SALARYSOLUTION");//薪资方案设置
                //MenuCodes.Add("T_HR_OVERTIMEREWARD");//加班类别设置
                //MenuCodes.Add("T_HR_VACATIONSET");//公共假期设置
                //MenuCodes.Add("T_HR_LEAVETYPESET");//假期标准设置
                //MenuCodes.Add("T_HR_ATTENDANCEDEDUCTMASTER");//考勤扣款标准
                //MenuCodes.Add("T_HR_SHIFTDEFINE");//打卡时间设置
                //MenuCodes.Add("T_HR_SCHEDULINGTEMPLATEMASTER");//作息方案设置
                //MenuCodes.Add("T_HR_ATTENDANCESOLUTIONASIGN");//考勤方案分配
                //MenuCodes.Add("T_HR_ATTENDANCESOLUTION");//考勤方案定义
                //MenuCodes.Add("T_HR_AREADIFFERENCE");//城市地区分类
                //MenuCodes.Add("T_OA_BUSINESSREPORT");//出差报告2011-5-25 add
                //MenuCodes.Add("T_OA_TRAVELREIMBURSEMENT");//出差报销
                //MenuCodes.Add("T_OA_BUSINESSTRIP");//出差申请
                //MenuCodes.Add("T_OA_TRAVELSOLUTIONS");//出差方案  2011-6-1增加
                //MenuCodes.Add("T_OA_AREAALLOWANCE");//出差补贴
                //MenuCodes.Add("T_FB_SUBJECTCOMPANY");//公司科目维护
                //MenuCodes.Add("T_FB_SUBJECTDEPTMENT");//部门科目维护
                //MenuCodes.Add("T_OA_AREADIFFERENCE");//出差城市分类
                //MenuCodes.Add("T_OA_SENDDOC");//公司公文
                string fileName = System.Web.HttpContext.Current.Server.MapPath("NoShowMenus\\NoShowMenus.xml");
                System.Xml.Linq.XDocument sourceFile = System.Xml.Linq.XDocument.Load(fileName);

                var ent = from xml in sourceFile.Root.Elements("Employes")
                          where xml.Attribute("employeeid").Value == employeeid
                          select xml;
                List<string> NoMenus = new List<string>();
                if (!(ent.Count() > 0))
                {
                    var entmenus = from xml in sourceFile.Root.Elements("Menus")                    
                                   select xml;
                    if (entmenus.Count() > 0)
                    {
                        entmenus.ToList().ForEach(item => {
                            NoMenus.Add(item.Attribute("menuid").Value);
                        });
                    }
                    //var tmp = ent.FirstOrDefault();
                    //if (tmp.Attribute("Version").Value != null)
                    //{
                    //    version = tmp.Attribute("Version").Value;
                    //}
                    //double ver;
                    //double.TryParse(version, out ver);
                    //tmp.Attribute("Version").Value = (ver + 1).ToString();
                    //tmp.Attribute("EditDate").Value = System.DateTime.Now.ToString();
                    //sourceFile.Save(fileName);
                }

                List<T_SYS_ENTITYMENU> ListMenus = new List<T_SYS_ENTITYMENU>();
                //var ents = from a in this.GetObjects().Include("T_SYS_ENTITYMENU2")
                //           where MenuCodes.Contains(a.MENUCODE) && a.HASSYSTEMMENU == "1"
                //           orderby a.ORDERNUMBER
                //           select a;
                var ents = from a in this.GetObjects().Include("T_SYS_ENTITYMENU2")
                           where a.T_SYS_ENTITYMENU2 != null && a.HASSYSTEMMENU == "1" 
                           && (a.SYSTEMTYPE == "0" || a.SYSTEMTYPE =="1" || a.SYSTEMTYPE=="3")
                           && a.ISAUTHORITY =="1" 
                           orderby a.SYSTEMTYPE,a.ORDERNUMBER
                           select a;
                
                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(
                        item =>{
                            
                            item.HASSYSTEMMENU = "";
                            item.ISAUTHORITY = "";
                            item.T_SYS_ENTITYMENU2 = null;
                            item.REMARK = "";
                            //item.SYSTEMTYPE = "";
                            item.UPDATEDATE = null;
                            item.UPDATEUSER = "";
                            item.URLADDRESS = "";
                            item.CREATEDATE = null;
                            item.CREATEUSER = "";
                            item.ENTITYCODE = "";
                            item.CHILDSYSTEMNAME = "";
                            item.ISAUTHORITY = "";
                            item.MENUICONPATH = "";
                            item.ENTITYNAME = "";
                            
                        }

                    
                        );
                }
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("菜单SysEntityMenuBLL-GetSysLeftMenu" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }


        }

    }
}
