using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT.SaaS.Permission.BLL;
using SMT_System_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using SMT.SaaS.Permission.DAL;
using SMT.SaaS.Permission.DAL.views;
using SMT.Foundation.Log;
using InterActiveDirectory;
using SMT.SaaS.Permission.BLL.PersonnelWS;
using SMT.SaaS.Permission.CustomerModel;
//using SMT.SaaS.BLLCommonServices.FlowWFService;


namespace SMT.SaaS.Permission.Services
{

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceContract]
    // [ServiceContract(SessionMode = SessionMode.Allowed)]//龙康才新增
    public class PermissionService
    {
        //private ClientStorage cache = ClientStorage.Instance;//龙康才新增
        private static int CallTime = 0;
        private string IpAddress = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();

        #region T_SYS_MENU 系统菜单

        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysLeftMenu(string sysType, string userID)
        {
            List<T_SYS_ENTITYMENU> menuList;
            string keyString = "GetSysLeftMenu" + sysType + userID;
            //GetSysLeftMenuFilterPermissionToNewFrame(userID);
            if (WCFCache.Current[keyString] == null)
            {
                using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
                {
                    menuList = bll.GetSysLeftMenu(sysType, userID).ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));
                }
            }
            else
            {
                menuList = (List<T_SYS_ENTITYMENU>)WCFCache.Current[keyString];
            }
            return menuList.Count() > 0 ? menuList : null;
        }
        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <param name="userID">用户ID</param>
        /// <returns>菜单信息列表</returns>
        /// <param name="menuids"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysLeftMenuFilterPermission(string sysType, string userID, ref List<string> menuids)
        {

            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                IQueryable<T_SYS_ENTITYMENU> menuList = bll.GetSysLeftMenuFilterPermission(sysType, userID, ref menuids);

                return menuList.Count() > 0 ? menuList.ToList() : null;
            }

        }

        /// <summary>
        /// 根据用户与系统类型获取该用户拥有权限的菜单信息2011-5-19
        /// </summary>
        /// <param name="sysType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <param name="userID">用户ID</param>
        /// <returns>菜单信息列表</returns>
        /// <param name="menuids"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_UserMenuPermission> GetSysLeftMenuFilterPermissionToNewFrame(string userID)
        {
            //using (SysUserBLL bll = new SysUserBLL())
            //{
            //    List<V_BllCommonUserPermission> plist;
            //    string menuCode = "T_HR_COMPANY";
            //    userID = "f89fb1ce-460f-4f53-8c45-04d8b5d45aca";
            //    string keyString = "BllCommonUserMenuPermsstring" + menuCode + userID;
            //    string Companykey = "BllOwnerCompanyIDs" + menuCode + userID;
            //    string Departmentkey = "BllOwnerDepartmentIDs" + menuCode + userID;
            //    string Positionkey = "BllOwnerPositionIDs" + menuCode + userID;
            //    string OwnerCompanyIDs = "";
            //    string OwnerDepartmentIDs = "";
            //    string OwnerPositionIDs = "";
            //    //if (WCFCache.Current[keyString] == null)
            //    //{

            //        IQueryable<V_BllCommonUserPermission> IQlist = bll.GetUserMenuPermsByUserPermisionBllCommon(menuCode, userID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);
            //        //if(IQlist != null)
            //        plist = IQlist != null ? IQlist.ToList() : null;
            //        WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(1));
            //        WCFCache.Current.Insert(Companykey, OwnerCompanyIDs, DateTime.Now.AddMinutes(1));
            //        WCFCache.Current.Insert(Departmentkey, OwnerDepartmentIDs, DateTime.Now.AddMinutes(1));
            //        WCFCache.Current.Insert(Positionkey, OwnerPositionIDs, DateTime.Now.AddMinutes(1));


            //    //}
            //    //else
            //    //{
            //    //    plist = (List<V_BllCommonUserPermission>)WCFCache.Current[keyString];
            //    //    OwnerCompanyIDs = (string)WCFCache.Current[Companykey];
            //    //    OwnerDepartmentIDs = (string)WCFCache.Current[Departmentkey];
            //    //    OwnerPositionIDs = (string)WCFCache.Current[Positionkey];

            //    //}

            //    return null;
            //}

            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                Tracer.Debug("系统用户SysUserBLL-GetSysLeftMenuFilterPermissionToNewFrame");
                T_SYS_FBADMIN UserFb = null;

                T_SYS_USER Userinfo = this.GetUserByID(userID);
                if (Userinfo != null)
                {
                    UserFb = this.getFbAdmin(Userinfo.SYSUSERID);
                }

                IQueryable<V_UserMenuPermission> menuList = UserFb != null ? bll.GetSysLeftMenuFilterPermissionToNewFrame(userID, UserFb) : bll.GetSysLeftMenuFilterPermissionToNewFrameForNotFbAdmin(userID, UserFb);

                return menuList != null ? menuList.ToList() : null;
            }

        }

        /// <summary>
        /// 根据用户ID获取用户的权限信息：加上了权限值的参数  2011-10-18
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="PermissionValue">权限值：0：新建；1：修改； 2：删除；3:查看；4：公文发布；5：新闻发布；6：转发 </param>
        /// <returns></returns>
        [OperationContract]
        public List<V_UserMenuPermission> GetSysLeftMenuFilterPermissionToNewFrameAndPermission(string userID,string PermissionValue)
        {

            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                IQueryable<V_UserMenuPermission> menuList = bll.GetSysLeftMenuFilterPermissionToNewFrameAndPermision(userID, PermissionValue);

                return menuList != null ? menuList.ToList() : null;
            }

        }

        /// <summary>
        /// 用户根据系统类型获取对应的菜单信息
        /// </summary>
        /// <param name="sysType"></param>
        /// <param name="userID"></param>
        /// <param name="Flag"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EntityMenu> GetEntityMenuByUser(string sysType, string userID, ref string Flag)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<V_EntityMenu> menuList = bll.GetEntityMenuByUser(sysType, userID, ref Flag);
                if (menuList == null) return null;
                return menuList.Count() > 0 ? menuList.ToList() : null;
            }
        }

        /// <summary>
        /// 获取所有的菜单信息
        /// </summary>
        /// <param name="sysType"></param>
        /// <param name="userID"></param>
        /// <param name="Flag"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EntityMenu> GetEntityMenuAll()
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<V_EntityMenu> menuList = bll.GetEntityMenuAll();
                return menuList.Count() > 0 ? menuList.ToList() : null;
            }
        }

        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysMenuByType(string systemType, string parentID)
        {
            //SysEntityMenuBLL bll = new SysEntityMenuBLL();
            //IQueryable<T_SYS_ENTITYMENU> menuList = bll.GetSysMenuByType(systemType, parentID);
            #region 龙康才新增
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<T_SYS_ENTITYMENU> menuList;
                string keyString = "GetSysMenuByType" + systemType + parentID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_ENTITYMENU> IQList = bll.GetSysMenuByType(systemType, parentID,null);

                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));
                }
                else
                {
                    menuList = (List<T_SYS_ENTITYMENU>)WCFCache.Current[keyString];
                }
            #endregion
                return menuList.Count() > 0 ? menuList : null;
            }
        }

        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysMenuByTypeTOFbAdmin(string systemType, string parentID,string employeeID)
        {
            //SysEntityMenuBLL bll = new SysEntityMenuBLL();
            //IQueryable<T_SYS_ENTITYMENU> menuList = bll.GetSysMenuByType(systemType, parentID);
            #region 龙康才新增
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<T_SYS_ENTITYMENU> menuList;
                string keyString = "";
                T_SYS_FBADMIN UserFb = null;
                if (!string.IsNullOrEmpty(employeeID))
                {
                    T_SYS_USER Userinfo = this.GetUserByEmployeeID(employeeID);
                    if (Userinfo != null)
                    {
                        UserFb = this.getFbAdmin(Userinfo.SYSUSERID);
                    }
                    if (UserFb == null)
                    {
                        keyString = "GetSysMenuByType" + systemType + parentID;
                    }
                    else
                    {
                        keyString = "GetSysMenuByType" + systemType + parentID + employeeID;
                    }

                }
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_ENTITYMENU> IQList = UserFb != null ? bll.GetSysMenuByType(systemType, parentID,UserFb):bll.GetSysMenuByTypeToFbAdmin(systemType, parentID);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));
                }
                else
                {
                    menuList = (List<T_SYS_ENTITYMENU>)WCFCache.Current[keyString];
                }
            #endregion
                return menuList.Count() > 0 ? menuList : null;
            }
        }

        
        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysMenuByPermission(string systemType, string UserID)
        {


            return null;
        }


        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysMenuByTypeToLookUp(string systemType, string parentID)
        {
            //SysEntityMenuBLL bll = new SysEntityMenuBLL();
            //IQueryable<T_SYS_ENTITYMENU> menuList = bll.GetSysMenuByType(systemType, parentID);
            #region 龙康才新增
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<T_SYS_ENTITYMENU> menuList;
                string keyString = "GetSysMenuByTypeToLookUp" + systemType + parentID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_ENTITYMENU> IQList = bll.GetSysMenuByTypeToLookUp(systemType, parentID);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));
                }
                else
                {
                    menuList = (List<T_SYS_ENTITYMENU>)WCFCache.Current[keyString];
                }
            #endregion
                return menuList != null ? menuList : null;
            }
        }



        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysMenuByTypeToLookUpForFbAdmin(string systemType, string parentID,string EmployeeID)
        {
            //SysEntityMenuBLL bll = new SysEntityMenuBLL();
            //IQueryable<T_SYS_ENTITYMENU> menuList = bll.GetSysMenuByType(systemType, parentID);
            #region 龙康才新增
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<T_SYS_ENTITYMENU> menuList;
                T_SYS_USER Userinfo = this.GetUserByEmployeeID(EmployeeID);
                string keyString = "GetSysMenuByTypeToLookUp" + systemType + parentID + EmployeeID;
                if (Userinfo == null)
                {
                    return null;
                }

                T_SYS_FBADMIN UserFb = null;
                
                if (WCFCache.Current[keyString] == null)
                {
                    if (!string.IsNullOrEmpty(EmployeeID))
                    {
                        UserFb = this.getFbAdmin(Userinfo.SYSUSERID);
                    }
                    IQueryable<T_SYS_ENTITYMENU> IQList = UserFb == null ? bll.GetSysMenuByTypeToLookUpForNoFbAdmin(systemType, parentID) : bll.GetSysMenuByTypeToLookUp(systemType, parentID);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));
                }
                else
                {
                    menuList = (List<T_SYS_ENTITYMENU>)WCFCache.Current[keyString];
                }
            #endregion
                return menuList != null ? menuList : null;
            }
        }


        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysMenuByTypePaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                T_SYS_USER Userinfo = this.GetUserByEmployeeID(loginUserInfo.userID);
                T_SYS_FBADMIN UserFb = null;
                bool IsFbAdmin = false;//是否预算管理员
                if (Userinfo != null)
                {
                    UserFb = this.getFbAdmin(Userinfo.SYSUSERID);
                }
                if (UserFb != null)
                {
                    IsFbAdmin = true;
                }
                IQueryable<T_SYS_ENTITYMENU> menuList = bll.GetSysMenuByTypeWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, IsFbAdmin);

                return menuList !=null ? menuList.ToList() : null;
            }
        }


        /// <summary>
        /// 根据实体编码获取菜单信息
        /// </summary>
        /// <param name="entityCode">实体编码</param>
        /// <returns>菜单信息</returns>
        [OperationContract]
        public T_SYS_ENTITYMENU GetSysMenuByEntityCode(string entityCode)
        {


            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {

                T_SYS_ENTITYMENU menu;
                string keyString = "GetSysMenuByEntityCode" + entityCode;
                if (WCFCache.Current[keyString] == null)
                {

                    menu = bll.GetSysMenuByEntityCode(entityCode);
                    WCFCache.Current.Insert(keyString, menu, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menu = (T_SYS_ENTITYMENU)WCFCache.Current[keyString];
                }

                return menu;
            }

        }
        /// <summary>
        /// 根据系统类型获取菜单信息 OA
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<V_MenuSetRole> GetOASysMenuByType(string systemType)
        {
            #region 龙康才新增
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<V_MenuSetRole> menuList;
                string keyString = "GetOASysMenuByType" + systemType;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_MenuSetRole> IQList = bll.GetSysMenuNameByTypeInfoNew(systemType);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<V_MenuSetRole>)WCFCache.Current[keyString];
                }

                return menuList.Count() > 0 ? menuList : null;
            }
            #endregion
        }
        /// <summary>
        /// 根据系统类型获取菜单信息  HR
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<V_MenuSetRole> GetHRSysMenuByType(string systemType)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                #region 龙康才新增
                List<V_MenuSetRole> menuList;
                string keyString = "GetHRSysMenuByType" + systemType;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_MenuSetRole> IQList = bll.GetSysMenuNameByTypeInfoNew(systemType);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<V_MenuSetRole>)WCFCache.Current[keyString];
                }
                #endregion
                return menuList.Count() > 0 ? menuList.ToList() : null;
            }
        }
        /// <summary>
        /// 根据系统类型获取菜单信息  FB 预算系统
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<V_MenuSetRole> GetFBSysMenuByType(string systemType,string employeeid)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                #region 龙康才新增
                List<V_MenuSetRole> menuList;
                string keyString = "";
                T_SYS_FBADMIN UserFb=null;
                if (!string.IsNullOrEmpty(employeeid))
                {
                    T_SYS_USER Userinfo = this.GetUserByEmployeeID(employeeid);
                    if (Userinfo != null)
                    {
                        UserFb = this.getFbAdmin(Userinfo.SYSUSERID);
                    }
                    if (UserFb == null)
                    {
                        keyString = "GetFBSysMenuByType" + systemType;
                    }
                    else
                    {
                        keyString = "GetFBSysMenuByType" + systemType + employeeid;
                    }
                }
                else
                {
                    keyString = "GetFBSysMenuByType" + systemType;
                }
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_MenuSetRole> IQList = UserFb != null ? bll.GetSysMenuNameByTypeInfoNew(systemType) : bll.GetSysMenuNameByTypeInfoNewToFbAdmins(systemType);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<V_MenuSetRole>)WCFCache.Current[keyString];
                }
                #endregion
                return menuList.Count() > 0 ? menuList : null;
            }
        }
        /// <summary>
        /// 根据系统类型获取菜单信息 LM 物流
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<V_MenuSetRole> GetLMSysMenuByType(string systemType)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                #region 龙康才新增
                List<V_MenuSetRole> menuList;
                string keyString = "GetLMSysMenuByType" + systemType;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_MenuSetRole> IQList = bll.GetSysMenuNameByTypeInfoNew(systemType);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<V_MenuSetRole>)WCFCache.Current[keyString];
                }
                #endregion
                return menuList.Count() > 0 ? menuList : null;
            }
        }
        /// <summary>
        /// 根据系统类型获取权限系统 pm
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<V_MenuSetRole> GetPMSysMenuByType(string systemType)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                #region 龙康才新增
                List<V_MenuSetRole> menuList;
                string keyString = "GetPMSysMenuByType" + systemType;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_MenuSetRole> IQList = bll.GetSysMenuNameByTypeInfoNew(systemType);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<V_MenuSetRole>)WCFCache.Current[keyString];
                }
                #endregion
                return menuList.Count() > 0 ? menuList : null;
            }
        }
        /// <summary>
        /// 根据系统类型获取菜单信息 edm
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统菜单</param>
        /// <returns>菜单信息列表</returns>
        [OperationContract]
        public List<V_MenuSetRole> GetEDMSysMenuByType(string systemType)
        {

            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                List<V_MenuSetRole> menuList;
                string keyString = "GetEDMSysMenuByType" + systemType;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_MenuSetRole> IQList = bll.GetSysMenuNameByTypeInfoNew(systemType);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<V_MenuSetRole>)WCFCache.Current[keyString];
                }

                return menuList.Count() > 0 ? menuList : null;
            }

        }
        /// <summary>
        /// 根据菜单ID获取系统菜单
        /// </summary>
        /// <param name="menuID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_SYS_ENTITYMENU GetSysMenuByID(string menuID)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {

                #region 龙康才新增
                T_SYS_ENTITYMENU menu;
                string keyString = "GetSysMenuByID" + menuID;
                if (WCFCache.Current[keyString] == null)
                {

                    menu = bll.GetSysMenuByID(menuID);
                    WCFCache.Current.Insert(keyString, menu, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menu = (T_SYS_ENTITYMENU)WCFCache.Current[keyString];
                }
                return menu;
                #endregion
            }
        }
        /// <summary>
        /// 获取某一菜单的子菜单
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetSysMenuInfosListByParentID(string parentID)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                #region 龙康才新增
                List<T_SYS_ENTITYMENU> menu;
                string keyString = "GetSysMenuInfosListByParentID" + parentID;
                if (WCFCache.Current[keyString] == null)
                {

                    menu = bll.GetSysMenuInfosByParentID(parentID);
                    WCFCache.Current.Insert(keyString, menu, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menu = (List<T_SYS_ENTITYMENU>)WCFCache.Current[keyString];
                }
                return menu;
                #endregion
            }
        }
        /// <summary>
        /// 新增系统菜单
        /// </summary>
        /// <param name="obj">系统菜单</param>
        [OperationContract]
        public string SysMenuAdd(T_SYS_ENTITYMENU obj)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                if (obj != null)
                {
                    string keyString = "GetSysMenuByType" + obj.SYSTEMTYPE;
                    string keyStringLookUp = "GetSysMenuByTypeToLookUp" + obj.SYSTEMTYPE;

                    if (WCFCache.Current[keyString] != null)
                    {
                        WCFCache.Current[keyString] = null;
                    }
                    if (WCFCache.Current[keyStringLookUp] != null)
                    {
                        WCFCache.Current[keyStringLookUp] = null;
                    }
                }
                return bll.SysMenuAdd(obj);
            }
        }
        /// <summary>
        /// 修改系统菜单 2011-12-12 修改菜单后清空所有菜单的缓存
        /// </summary>
        /// <param name="obj">系统菜单</param>
        [OperationContract]
        public void SysMenuUpdate(T_SYS_ENTITYMENU obj)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                string keyString = "GetSysMenuByID" + obj.ENTITYMENUID;
                string keyStringLookUp = "GetSysMenuByTypeToLookUp" + obj.SYSTEMTYPE;

                if (WCFCache.Current[keyString] != null)
                {
                    WCFCache.Current[keyString] = null;
                }
                if (WCFCache.Current[keyStringLookUp] != null)
                {
                    WCFCache.Current[keyStringLookUp] = null;
                }
                                
                bll.SysMenuUpdate(obj);
            }
        }

        /// <summary>
        /// 删除系统菜单
        /// </summary>
        /// <param name="menuID">系统菜单编号</param>
        [OperationContract]
        public string SysMenuDelete(string menuID)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                return bll.SysMenuDelete(menuID);
            }
        }
        /// <summary>
        /// 根据菜单ID集合和 系统类型 获取菜单集合
        /// </summary>
        /// <param name="MenuIDs"></param>
        /// <param name="StrSysType"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetEntityMenuByMenuIDs(string[] MenuIDs, string StrSysType)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {
                IQueryable<T_SYS_ENTITYMENU> Listmenus = bll.GetEntityMenuByMenuIDs(MenuIDs, StrSysType);
                return Listmenus != null ? Listmenus.ToList() : null;
            }
        }

        #endregion

        #region T_SYS_ROLEENTITYMENU 系统角色菜单
        /// <summary>
        /// 根据系统类型获取菜单信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统角色菜单</param>
        /// <returns>角色菜单信息列表</returns>
        [OperationContract]
        public List<T_SYS_ROLEENTITYMENU> GetRoleEntityMenuByType(string systemType)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {
                #region 龙康才新增
                List<T_SYS_ROLEENTITYMENU> menuList;
                string keyString = "GetRoleEntityMenuByType" + systemType;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_ROLEENTITYMENU> IQList = bll.GetRoleEntityMenuByType(systemType);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<T_SYS_ROLEENTITYMENU>)WCFCache.Current[keyString];
                }
                #endregion
                return menuList.Count() > 0 ? menuList : null;
            }
        }
        /// <summary>
        /// 根据菜单ID获取系统角色菜单
        /// </summary>
        /// <param name="menuID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_SYS_ROLEENTITYMENU GetRoleEntityMenuByID(string menuID)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {
                #region 龙康才新增
                T_SYS_ROLEENTITYMENU menuList;
                string keyString = "GetRoleEntityMenuByID" + menuID;
                if (WCFCache.Current[keyString] == null)
                {

                    menuList = bll.GetRoleEntityMenuByID(menuID);
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (T_SYS_ROLEENTITYMENU)WCFCache.Current[keyString];
                }
                return menuList;
                #endregion
            }
        }
        /// <summary>
        /// 新增系统角色菜单
        /// </summary>
        /// <param name="obj">系统角色菜单</param>
        [OperationContract]
        public void RoleEntityMenuAdd(T_SYS_ROLEENTITYMENU obj)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {
                bll.RoleEntityMenuAdd(obj);
            }
        }
        /// <summary>
        /// 修改系统角色菜单
        /// </summary>
        /// <param name="obj">系统角色菜单</param>
        [OperationContract]
        public void RoleEntityMenuUpdate(T_SYS_ROLEENTITYMENU obj)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {
                bll.RoleEntityMenuUpdate(obj);
            }
        }
        [OperationContract]
        public bool BatchAddRoleEntityPermissionInfos(string StrTmpList, string StrAdduser, string StrRoleID)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {
                string keyString = "RoleEntityID" + StrRoleID;
                String KeyNewString ="GetRoleEntityIDListInfosByRoleIDNew" + StrRoleID;//修改后的视图的显示
                string keyStringPermission = "GetPermissionByRoleIDSecond" + StrRoleID;
                //WCFCache.Current.Remove(WCFCache.Current[keyString]);
                WCFCache.Current[keyString] = null;
                WCFCache.Current[KeyNewString] = null;
                //WCFCache.Current.Remove(WCFCache.Current[keyStringPermission]);
                WCFCache.Current[keyStringPermission] = null;
                return bll.RoleEntityMenuBatchAddInfosList(StrTmpList, StrRoleID, StrAdduser);
                //return false;
            }
        }
        /// <summary>
        /// 批量添加个人角色权限  2011-5-27
        /// </summary>
        /// <param name="StrTmpList"></param>
        /// <param name="StrAdduser"></param>
        /// <param name="StrRoleID"></param>
        /// <returns></returns>
        [OperationContract]
        public bool UserRoleApplyBatchAddRoleEntityPermissionInfos(T_SYS_ROLE EntRole, string StrTmpList, string StrAdduser)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {
                string keyString = "RoleEntityID" + EntRole.ROLEID;
                String KeyNewString = "GetRoleEntityIDListInfosByRoleIDNew" + EntRole.ROLEID;//修改后的视图的显示
                string keyStringPermission = "GetPermissionByRoleIDSecond" + EntRole.ROLEID;
                //WCFCache.Current.Remove(WCFCache.Current[keyString]);
                WCFCache.Current[keyString] = null;
                WCFCache.Current[KeyNewString] = null;
                //WCFCache.Current.Remove(WCFCache.Current[keyStringPermission]);
                WCFCache.Current[keyStringPermission] = null;
                return bll.UserRoleApplyEntityMenuBatchAddInfosList(EntRole, StrTmpList, StrAdduser);
                //return bll.RoleEntityMenuBatchAddInfosList(StrTmpList, EntRole.ROLEID, StrAdduser);
                //return false;
            }
        }

        /// <summary>
        /// 删除系统角色菜单
        /// </summary>
        /// <param name="menuID">系统角色菜单编号</param>
        [OperationContract]
        public void RoleEntityMenuDelete(string id)
        {
            RoleEntityMenuBLL bll = new RoleEntityMenuBLL();
            //bll.RoleEntityMenuDelete(id);
        }
        /// <summary>
        /// 获取某一角色的菜单
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_ROLEENTITYMENU> GetRoleEntityIDListInfosByRoleID(string RoleID)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {
                #region 龙康才新增
                List<T_SYS_ROLEENTITYMENU> Listmenu = new List<T_SYS_ROLEENTITYMENU>();
                List<T_SYS_ROLEENTITYMENU> menuList;
                string keyString = "GetRoleEntityIDListInfosByRoleID" + RoleID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_ROLEENTITYMENU> IList = bll.GetRoleEntityIDListInfos(RoleID);

                    menuList = IList != null ? IList.ToList() : null;
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<T_SYS_ROLEENTITYMENU>)WCFCache.Current[keyString];
                }
                #endregion

                foreach (var q in menuList)
                {
                    T_SYS_ROLEENTITYMENU temp = new T_SYS_ROLEENTITYMENU();
                    temp.T_SYS_ENTITYMENU = new T_SYS_ENTITYMENU();
                    if (q.T_SYS_ENTITYMENU != null)
                    {
                        temp.T_SYS_ENTITYMENU.ENTITYMENUID = q.T_SYS_ENTITYMENU.ENTITYMENUID;
                    }
                    else
                    {
                        continue;
                    }
                    temp.ROLEENTITYMENUID = q.ROLEENTITYMENUID;
                    Listmenu.Add(temp);
                }

                return Listmenu.Count() > 0 ? Listmenu : null;
            }
        }


        /// <summary>
        /// 获取某一角色的菜单  使用视图实现 2011-5-20
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_RoleEntity> GetRoleEntityIDListInfosByRoleIDNew(string RoleID)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {
                
                List<V_RoleEntity> Listmenu = new List<V_RoleEntity>();
                List<V_RoleEntity> menuList;
                string keyString = "GetRoleEntityIDListInfosByRoleIDNew" + RoleID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_RoleEntity> IList = bll.GetRoleEntityIDListInfosNew(RoleID);

                    menuList = IList != null ? IList.ToList() : null;
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<V_RoleEntity>)WCFCache.Current[keyString];
                }
                

                foreach (var q in menuList)
                {
                    V_RoleEntity temp = new V_RoleEntity();
                    
                    if (string.IsNullOrEmpty(q.ENTITYMENUID))
                    {
                       continue;
                    }
                    
                    Listmenu.Add(temp);
                }

                return Listmenu.Count() > 0 ? Listmenu : null;
            }
        }

        /// <summary>
        /// 获取某一角色的菜单  使用视图实现 2011-5-20
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_RoleEntity> GetRoleEntityIDListInfosByRoleIDNewToUserRoleApp(string RoleID,ref List<T_SYS_ENTITYMENU> listmenu)
        {
            using (RoleEntityMenuBLL bll = new RoleEntityMenuBLL())
            {

                List<V_RoleEntity> ListmenuV = new List<V_RoleEntity>();
                List<V_RoleEntity> menuList;
                List<T_SYS_ENTITYMENU> tmpListmenu = new List<T_SYS_ENTITYMENU>();
                string keyString = "GetRoleEntityIDListInfosByRoleIDNewToUserRoleApp" + RoleID;
                string keyStringMenu = "GetRoleEntityIDListInfosByRoleIDNewToUserRoleAppMenu" + RoleID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_RoleEntity> IList = bll.GetRoleEntityIDListInfosNewToUserRoleApp(RoleID, ref listmenu);
                    
                    menuList = IList != null ? IList.ToList() : null;
                    tmpListmenu = listmenu;
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));
                    WCFCache.Current.Insert(keyStringMenu, tmpListmenu, DateTime.Now.AddMinutes(15));
                }
                else
                {
                    menuList = (List<V_RoleEntity>)WCFCache.Current[keyString];
                    listmenu = (List<T_SYS_ENTITYMENU>)WCFCache.Current[keyStringMenu];
                }


                foreach (var q in menuList)
                {
                    V_RoleEntity temp = new V_RoleEntity();

                    if (string.IsNullOrEmpty(q.ENTITYMENUID))
                    {
                        continue;
                    }

                    ListmenuV.Add(temp);
                }

                return ListmenuV.Count() > 0 ? ListmenuV : null;
            }
        }
        #endregion

        #region T_SYS_PERMISSION 权限定义
        /// <summary>
        /// 获取权限定义列表
        /// </summary>
        /// <returns>返回所有权限列表</returns>
        [OperationContract]
        public List<T_SYS_PERMISSION> GetSysPermissionAll()
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                #region 龙康才新增
                List<T_SYS_PERMISSION> perList;

                WCFCache.Current["SysPermissionAll"] = null;
                string keyString = "SysPermissionAll";
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_PERMISSION> IQList = bll.GetSysPermissionAll();
                    perList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, perList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perList = (List<T_SYS_PERMISSION>)WCFCache.Current[keyString];
                }


                #endregion
                return perList.Count() > 0 ? perList : null;
            }
        }

        /// <summary>
        /// 获取公共权限定义列表
        /// </summary>
        /// <returns>返回所有权限列表</returns>
        [OperationContract]
        public List<T_SYS_PERMISSION> GetSysCommonPermissionAll()
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                #region 龙康才新增
                List<T_SYS_PERMISSION> perList;

                WCFCache.Current["SysCommonPermissionAll"] = null;
                string keyString = "SysCommonPermissionAll";
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_PERMISSION> IQList = bll.GetSysCommonPermissionAll();
                    perList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, perList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perList = (List<T_SYS_PERMISSION>)WCFCache.Current[keyString];
                }


                #endregion
                return perList.Count() > 0 ? perList : null;
            }

        }

        /// <summary>
        /// 获取权限定义列表和特殊的权限
        /// </summary>
        /// <returns>返回所有权限列表</returns>
        [OperationContract]
        public List<T_SYS_PERMISSION> GetSysPermissionByEntityID(string EntityID)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                //SysPermissionBLL bll = new SysPermissionBLL();
                //IQueryable<T_SYS_PERMISSION> perList = bll.GetSysPermissionAll();
                #region 龙康才新增
                List<T_SYS_PERMISSION> perList;
                string keyString = "SysPermissionAll" + EntityID;


                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_PERMISSION> IQList = bll.GetPermissionByEntityID(EntityID);
                    perList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, perList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perList = (List<T_SYS_PERMISSION>)WCFCache.Current[keyString];
                }

                #endregion
                return perList.Count() > 0 ? perList : null;
            }
        }

        [OperationContract]
        //所有用户信息 2010-6-10
        public List<T_SYS_PERMISSION> GetSysPermissionAllPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                #region 龙康才新增
                List<T_SYS_PERMISSION> perList;
                string keyString = "PermissionAllPagingCache";
                if (WCFCache.Current[keyString] == null)
                {

                    perList = bll.GetSysPermissionAll().ToList();
                    WCFCache.Current.Insert(keyString, perList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perList = (List<T_SYS_PERMISSION>)WCFCache.Current[keyString];
                }
                #endregion
                return perList != null ? perList : null;

            }


        }
        /// <summary>
        /// 根据系统类型查询
        /// </summary>
        /// <param name="sName">权限名称</param>
        /// <returns>权限定义列表</returns>
        [OperationContract]
        public List<T_SYS_PERMISSION> FindSysPermissionByType(string type)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                #region 龙康才新增
                List<T_SYS_PERMISSION> perList;
                string keyString = "FindSysPermissionByType" + type;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_PERMISSION> IQList = bll.FindSysPermissionByType(type);
                    perList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, perList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perList = (List<T_SYS_PERMISSION>)WCFCache.Current[keyString];
                }
                #endregion
                return perList.Count() > 0 ? perList : null;
            }
        }
        /// <summary>
        /// 根据权限ID获取权限定义
        /// </summary>
        /// <param name="perID">权限ID</param>
        /// <returns></returns>
        [OperationContract]
        public T_SYS_PERMISSION GetSysPermissionByID(string perID)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                #region 龙康才新增
                T_SYS_PERMISSION perList;
                string keyString = "GetSysPermissionByID" + perID;
                if (WCFCache.Current[keyString] == null)
                {

                    perList = bll.GetSysPermissionByID(perID);
                    WCFCache.Current.Insert(keyString, perList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perList = (T_SYS_PERMISSION)WCFCache.Current[keyString];
                }
                return perList;
                #endregion
            }
        }
        /// <summary>
        /// 新增权限定义
        /// </summary>
        /// <param name="obj">权限定义</param>
        [OperationContract]
        public string SysPermissionAdd(T_SYS_PERMISSION obj)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                string keyString = "PermissionAllPagingCache" ;
                WCFCache.Current[keyString] = null;//清缓存
                return bll.AddPermission(obj);
            }
            //bll.Add(obj);
        }
        /// <summary>
        /// 修改权限定义
        /// </summary>
        /// <param name="obj">权限定义</param>
        [OperationContract]
        public string SysPermissionUpdate(T_SYS_PERMISSION obj)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                string keyString = "PermissionAllPagingCache";
                WCFCache.Current[keyString] = null;//清缓存
                string AddkeyString = "GetSysPermissionByID" + obj.PERMISSIONID;
                WCFCache.Current[AddkeyString] = null;
                return bll.SysPermissionUpdate(obj);
            }
        }
        /// <summary>
        /// 删除权限定义
        /// </summary>
        /// <param name="perID">权限ID</param>
        [OperationContract]
        public string SysPermissionDelete(string perID)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                string keyString = "PermissionAllPagingCache";
                WCFCache.Current[keyString] = null;//清缓存
                return bll.SysPermissionDelete(perID);
            }
        }
        /// <summary>
        /// 根椐权限名称查询
        /// </summary>
        /// <param name="sName">权限名称</param>
        /// <returns>sName为空时返回所有权限定义</returns>
        [OperationContract]
        public List<T_SYS_PERMISSION> FindSysPermissionByStr(string sName)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                #region 龙康才新增
                List<T_SYS_PERMISSION> perList;
                string keyString = "FindSysPermissionByStr" + sName;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_PERMISSION> IQList = bll.FindSysPermissionByStr(sName);
                    perList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, perList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perList = (List<T_SYS_PERMISSION>)WCFCache.Current[keyString];
                }
                #endregion
                return perList.Count() > 0 ? perList : null;
            }
        }
        #endregion

        #region 角色管理


        [OperationContract]
        //增
        public string SysRoleInfoAdd(T_SYS_ROLE obj)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                string returnStr = "";
                if (!this.IsExistSysRoleInfoByRoleNameAndCompanyAndDepartmentid(obj.ROLENAME, obj.OWNERCOMPANYID,obj.OWNERDEPARTMENTID))
                {
                    bool sucess = RoleBll.AddSysRoleInfo(obj);
                    if (sucess == false)
                    {
                        returnStr = "添加系统角色失败";
                    }
                }
                else
                {

                    returnStr = "系统角色已经存在";
                }
                return returnStr;
            }
        }

        [OperationContract]
        private bool IsExistSysRoleInfo(string StrRoleName)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                return RoleBll.GetExistRoleInfoByRoleName(StrRoleName);
            }
        }
        /// <summary>
        /// 通过角色名和公司ID判断记录是否重复 2010-8-5 liujianx
        /// </summary>
        /// <param name="StrRoleName"></param>
        /// <param name="CompanyID"></param>
        /// <returns></returns>
        [OperationContract]
        private bool IsExistSysRoleInfoByRoleNameAndCompanyAndDepartmentid(string StrRoleName, string CompanyID,string StrDepartmentid)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                return RoleBll.GetExistRoleInfoByRoleNameAndComapnyIDAndDepartmentid(StrRoleName, CompanyID,StrDepartmentid);
            }
        }



        [OperationContract]
        //改
        public void SysRoleInfoUpdate(T_SYS_ROLE obj, ref string StrResult)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                StrResult = RoleBll.SysRoleUpdate(obj, ref StrResult);
            }
        }

        [OperationContract]
        //查单
        public T_SYS_ROLE GetSysRoleSingleInfoById(string StrRoleId)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                return RoleBll.GetSysRoleByID(StrRoleId);
            }
        }
        [OperationContract]
        //所有系统角色信息
        public List<T_SYS_ROLE> GetSysRoleInfos(string Systype, string StrCompanyID)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                List<T_SYS_ROLE> SysRoleInfosList = RoleBll.GetAllSysRoleInfos(Systype, StrCompanyID);
                if (SysRoleInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SysRoleInfosList;
                }
            }

        }


        [OperationContract]
        //所有系统角色信息
        public List<T_SYS_ROLE> GetSysRoleInfosByCompanyIdAndDepartmentId(string Systype, string StrCompanyID, string StrDepartmentID)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                List<T_SYS_ROLE> SysRoleInfosList = RoleBll.GetAllSysRoleInfosByCompanyIdAndDepartmentID(Systype, StrCompanyID, StrDepartmentID);
                if (SysRoleInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SysRoleInfosList;
                }
            }

        }

        /// <summary>
        /// 服务器端分页 角色
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        //所有系统角色信息
        public List<T_SYS_ROLE> GetSysRoleInfosPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                IQueryable<T_SYS_ROLE> SysRoleInfosList = RoleBll.GetAllSysRoleInfosWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);
                if (SysRoleInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SysRoleInfosList.ToList();
                }
            }

        }

        /// <summary>
        /// 服务器端分页 角色
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        //所有系统角色信息
        public List<T_SYS_ROLE> GetSysRoleInfosPagingByCompanyIDs(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo, string[] CompanyIDs)
        {
            
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                //SysUserBLL userbll = new SysUserBLL();
                //SysUserBLL userbll = new SysUserBLL();
                //string companyids = "";
                //string departids = "";
                //string postids = "";
                //userbll.GetUserMenuPermsByUserPermisionBllCommon("T_OA_APPROVALINFO", "2ec631ed-0fc0-4b7f-a86c-28581eeab068", ref companyids, ref departids, ref postids);
                IQueryable<T_SYS_ROLE> SysRoleInfosList = RoleBll.GetAllSysRoleInfosWithPagingByCompanyIDs(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, CompanyIDs);
                //GetSysLeftMenuFilterPermissionToNewFrame("543e0360-6e27-4f8a-82cb-6df1f9ea6a8d");//85b414ab-87b3-4740-aef4-1d89f3f380cc
                //GetSysLeftMenuFilterPermissionToNewFrame("85b414ab-87b3-4740-aef4-1d89f3f380cc");
                //GetUserByEmployeeID("940d667e-4c04-425a-b347-b82719f39c71");
                
                if (SysRoleInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SysRoleInfosList.ToList();
                }
            }

        }

        /// <summary>
        /// 服务器端分页 角色
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        //获取系统角色-视图
        public List<T_SYS_ROLE_V> GetRoleView(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo, string[] CompanyIDs)
        {

            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                //SysUserBLL userbll = new SysUserBLL();
                //SysUserBLL userbll = new SysUserBLL();
                //string companyids = "";
                //string departids = "";
                //string postids = "";
                //userbll.GetUserMenuPermsByUserPermisionBllCommon("T_OA_APPROVALINFO", "2ec631ed-0fc0-4b7f-a86c-28581eeab068", ref companyids, ref departids, ref postids);
                IQueryable<T_SYS_ROLE_V> SysRoleInfosList = RoleBll.GetRoleView(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, CompanyIDs);
                //GetSysLeftMenuFilterPermissionToNewFrame("543e0360-6e27-4f8a-82cb-6df1f9ea6a8d");//85b414ab-87b3-4740-aef4-1d89f3f380cc
                //GetSysLeftMenuFilterPermissionToNewFrame("85b414ab-87b3-4740-aef4-1d89f3f380cc");
                //GetUserByEmployeeID("940d667e-4c04-425a-b347-b82719f39c71");

                if (SysRoleInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SysRoleInfosList.ToList();
                }
            }

        }

        [OperationContract]
        //批量删除
        public string SysRoleBatchDel(string[] StrRoleIDs)
        {
            using (SysRoleBLL RoleBll = new SysRoleBLL())
            {
                return RoleBll.BatchDeleteSysRoleInfos(StrRoleIDs);
            }
        }
        #endregion

        #region T_SYS_DICTIONARY 系统字典
        /// <summary>
        /// 根据字典类型获取对应的字典集合
        /// </summary>
        /// <param name="category">字典类型</param>
        /// <returns>字典类型对应的字典集合</returns>
        [OperationContract]
        public List<T_SYS_DICTIONARY> GetSysDictionaryByCategory(string category)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                //GetSysLeftMenuFilterPermissionToNewFrame("85b414ab-87b3-4740-aef4-1d89f3f380cc");
                IQueryable<T_SYS_DICTIONARY> dictlist = bll.GetSysDictionaryByCategory(category);

                return dictlist != null ? dictlist.ToList() : null;
            }
        }

        /// <summary>
        /// 取所有字典集合
        /// </summary>
        /// <param name="category"></param>
        /// <returns>字典集合</returns>
        [OperationContract]
        public List<T_SYS_DICTIONARY> GetAllSysDictionary()
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                IQueryable<T_SYS_DICTIONARY> dictlist = bll.GetAllSysDictionary();

                return dictlist != null ? dictlist.ToList() : null;
            }
        }

        /// <summary>
        /// 根据字典类型和值获取字典名称
       /// </summary>
       /// <param name="category">字典类型</param>
       /// <param name="value">字典值</param>
       /// <returns>字典名称</returns>
        [OperationContract]
        public string GetSysDictionaryByCategoryAndValue(string category, string value)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                return bll.GetSysDictionaryByCategoryAndValue(category, value);
            }
        }



        /// <summary>
        /// 根据父级字典ID获取子级字典值
        /// </summary>
        /// <param name="fatherID">父字典ID</param>
        /// <returns>返回子字典的集合</returns>
        [OperationContract]
        public List<T_SYS_DICTIONARY> GetSysDictionaryByFatherID(string fatherID)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                IQueryable<T_SYS_DICTIONARY> dictlist = bll.GetSysDictionaryByFatherID(fatherID);
                return dictlist.Count() > 0 ? dictlist.ToList() : null;
            }
        }
        /// <summary>
        /// 根据字典类型集合获取所有的字典信息
        /// </summary>
        /// <param name="category">字典类型集合</param>
        /// <returns>对应的字典集合</returns>
        [OperationContract]
        public List<T_SYS_DICTIONARY> GetSysDictionaryByCategoryList(List<string> category)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                List<T_SYS_DICTIONARY> dictlist = bll.GetSysDictionaryByCategory(category);
                return dictlist;
            }
        }
        /// <summary>
        /// 增加字典
        /// </summary>
        /// <param name="dict"></param>
        [OperationContract]
        public void SysDictionaryAdd(T_SYS_DICTIONARY dict, ref string strMsg)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                
                bll.SysDictionaryAdd(dict, ref strMsg);
            }
        }
        /// <summary>
        /// 更新字典
        /// </summary>
        /// <param name="dict"></param>
        [OperationContract]
        public void SysDictionaryUpdate(T_SYS_DICTIONARY dict, ref string strMsg)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {

                bll.SysDictionaryUpdate(dict, ref strMsg);
            }
        }
        /// <summary>
        /// 删除字典
        /// </summary>
        /// <param name="dict"></param>
        [OperationContract]
        public void SysDictionaryDelete(string[] IDs)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                bll.SysDictionaryDelete(IDs);
            }
        }

        /// <summary>
        /// 获取批量导入的城市信息
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="empInfoDic"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        [OperationContract]
        public bool ImportCityCSV(UploadFileModel uploadFile, Dictionary<string, string> empInfoDic, ref string strMsg)
        {
            string strPath = string.Empty;
            SaveFile(uploadFile, out strPath);//获取文件路径
            string strPhysicalPath = System.Web.HttpContext.Current.Server.MapPath(strPath);//到时测试strPath为空是是否报错
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                return bll.ImportCityCSV(strPhysicalPath, empInfoDic, ref strMsg);
            }
        }
        #region 文件上传服务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="strFilePath">上传文件存储的相对路径</param>
        [OperationContract]
        public void SaveFile(UploadFileModel UploadFile, out string strFilePath)
        {
            // Store File to File System
            //string strVirtualPath = System.Configuration.ConfigurationManager.AppSettings["FileUploadLocation"].ToString();//权限系统没有文件路径配置
            string strNewFileName = string.Empty;
            string strVirtualPath = "/UploadedFiles/";
            if (!string.IsNullOrWhiteSpace(UploadFile.FileName))
            {
                strNewFileName = DateTime.Now.ToString("yyMMddhhmmss") + DateTime.Now.Millisecond.ToString() + UploadFile.FileName.Substring(UploadFile.FileName.LastIndexOf("."));
            }
            string strPath = System.Web.HttpContext.Current.Server.MapPath(strVirtualPath) + strNewFileName;
            if (System.IO.Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(strVirtualPath)) == false)
            {
                System.IO.Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath(strVirtualPath));
            }
            System.IO.FileStream FileStream = new System.IO.FileStream(strPath, System.IO.FileMode.Create);
            FileStream.Write(UploadFile.File, 0, UploadFile.File.Length);
            FileStream.Close();
            FileStream.Dispose();
            strFilePath = strVirtualPath + strNewFileName;
        }
        #endregion
        /// <summary>
        /// 获取所有字典集合
        /// </summary>
        /// <returns>返回所有字典集合</returns>
        [OperationContract]
        public List<T_SYS_DICTIONARY> GetSysDictionaryCategory()
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                return bll.GetSysDictionaryCategory();
            }
        }
        /// <summary>
        /// 获取字典系统类型
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_DICTIONARY> GetSysDictionarySysType()
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                return bll.GetSysDictionarySysType();
            }
        }
        /// <summary>
        /// 获取某条字典信息
        /// </summary>
        /// <param name="dictID">字典ID</param>
        /// <returns>返回某条字典信息</returns>
        [OperationContract]
        public T_SYS_DICTIONARY GetSysDictionaryByID(string dictID)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                return bll.GetSysDictionaryByID(dictID);
            }
        }
        /// <summary>
        /// 根据条件获取字典
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_DICTIONARY> GetSysDictionaryByFilter(string filter, string sort, string[] paras)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                return bll.GetSysDictionaryByFilter(filter, sort, paras);
            }
        }


        /// <summary>
        /// 根据条件获取字典
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_DICTIONARY> GetSysDictionaryByFilterPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                IQueryable<T_SYS_DICTIONARY> menuList = bll.GetSysDictionaryByFilterWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);

                if (menuList != null)
                {
                    return menuList.Count() > 0 ? menuList.ToList() : null;
                }
                else
                {
                    return null;
                }
            }


        }
        /// <summary>
        /// 获取字典的版本
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="lsDepartmentids"></param>
        /// <returns></returns>
        [OperationContract]
        public string GetSystemDictioanryVersion()
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                return bll.ReadVersion();
            }

        }
        #endregion

        #region T_SYS_ENTITYMENUCUSTOMPERM 自定义角色权限
        ///// <summary>
        ///// 获取用户自定义权限
        ///// </summary>
        ///// <returns></returns>
        //[OperationContract]
        //public List<CustomerPermission> GetCutomterPermissionObj(string RoleID)
        //{
        //    //EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL();
        //    //IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> CusPermissions = bll.GetCustomPermByRoleID(RoleID);

        //    using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
        //    {
        //        IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> cuspers = bll.GetCustomPermByRoleID(RoleID);
        //        List<CustomerPermission> CusPers = new List<CustomerPermission>();
        //        if (cuspers != null)
        //        {
        //            //var entiyMenuPs = from ent in cuspers
        //            //                  where ent.T_SYS_ROLE.ROLEID == RoleID
        //            //                  select ent;
        //            //group ent by new
        //            //{
        //            //    ent.T_SYS_ENTITYMENU.ENTITYMENUID,
        //            //    ent.T_SYS_PERMISSION.PERMISSIONID
        //            //}
        //            //    into g
        //            //    select new
        //            //    {
        //            //        MenuId = g.Key.ENTITYMENUID,
        //            //        PermissionId = g.Key.PERMISSIONID,
        //            //        g
        //            //    };
        //            //遍历取菜单
        //            foreach (var menus in cuspers)
        //            {
        //                CustomerPermission obj = new CustomerPermission();
        //                obj.EntityMenuId = menus.T_SYS_ENTITYMENU.ENTITYMENUID;
        //                obj.PermissionValue = new List<PermissionValue>();

        //                //遍历取权限
        //                var Perms = from ent in cuspers
        //                            where ent.T_SYS_ENTITYMENU.ENTITYMENUID == menus.T_SYS_ENTITYMENU.ENTITYMENUID
        //                            select ent;
        //                foreach (var Perm in Perms)
        //                {
        //                    PermissionValue menuP = new PermissionValue();
        //                    menuP.Permission = Perm.T_SYS_PERMISSION.PERMISSIONID;
        //                    menuP.OrgObjects = new List<OrgObject>();
        //                    //遍历组织架构
        //                    var orgObj = from ent in cuspers
        //                                 where ent.T_SYS_ENTITYMENU.ENTITYMENUID == menus.T_SYS_ENTITYMENU.ENTITYMENUID
        //                                 && ent.T_SYS_PERMISSION.PERMISSIONID == Perm.T_SYS_PERMISSION.PERMISSIONID
        //                                 select ent;

        //                    foreach (var item in orgObj)
        //                    {
        //                        OrgObject orgobj = new OrgObject();
        //                        if (!string.IsNullOrWhiteSpace(item.COMPANYID))
        //                        {
        //                            orgobj.OrgID = item.COMPANYID;
        //                            orgobj.OrgType = "0";
        //                        }
        //                        if (!string.IsNullOrWhiteSpace(item.DEPARTMENTID))
        //                        {
        //                            orgobj.OrgID = item.DEPARTMENTID;
        //                            orgobj.OrgType = "1";
        //                        }
        //                        if (!string.IsNullOrWhiteSpace(item.POSTID))
        //                        {
        //                            orgobj.OrgID = item.POSTID;
        //                            orgobj.OrgType = "2";
        //                        }
        //                        menuP.OrgObjects.Add(orgobj);
        //                    }
        //                    obj.PermissionValue.Add(menuP);
        //                }
        //                CusPers.Add(obj);

        //            }
        //        }

        //        return CusPers;
        //    }
        //}

        /// <summary>
        /// 获取用户自定义权限
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<CustomerPermission> GetCutomterPermissionObj(string RoleID)
        {
            //EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL();
            //IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> CusPermissions = bll.GetCustomPermByRoleID(RoleID);
            try
            {
                SMT.Foundation.Log.Tracer.Debug("开始获取自定义角色222:" + RoleID + " " + System.DateTime.Now.ToString());
                using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
                {

                    IQueryable<V_EntityMenuCustomperm> cuspers = bll.GetCustomPermByRoleIDNew(RoleID);
                    List<CustomerPermission> CusPers = new List<CustomerPermission>();
                    if (cuspers != null)
                    {
                        //var entiyMenuPs = from ent in cuspers
                        //                  where ent.T_SYS_ROLE.ROLEID == RoleID
                        //                  select ent;
                        SMT.Foundation.Log.Tracer.Debug("开始获取自定义角色记录数:" + cuspers.Count().ToString() + " " + System.DateTime.Now.ToString());
                        if (cuspers.Count() > 0)
                        {
                            //遍历取菜单
                            foreach (var menus in cuspers)
                            {
                                CustomerPermission obj = new CustomerPermission();
                                obj.EntityMenuId = menus.ENTITYMENUID;
                                obj.PermissionValue = new List<PermissionValue>();

                                if (cuspers.Count() > 0)
                                {
                                    var ent1 = from ent in CusPers
                                               where ent.EntityMenuId == obj.EntityMenuId
                                               select ent;
                                    if (ent1.Count() > 0)
                                        continue;
                                }
                                //遍历取权限
                                var Perms = from ent in cuspers
                                            where ent.ENTITYMENUID == menus.ENTITYMENUID
                                            select ent;
                                foreach (var Perm in Perms)
                                {
                                    PermissionValue menuP = new PermissionValue();
                                    menuP.Permission = Perm.PERMISSIONID;
                                    menuP.OrgObjects = new List<OrgObject>();
                                    //遍历组织架构
                                    var orgObj = from ent in cuspers
                                                 where ent.ENTITYMENUID == menus.ENTITYMENUID
                                                 && ent.PERMISSIONID == Perm.PERMISSIONID
                                                 select ent;

                                    foreach (var item in orgObj)
                                    {
                                        OrgObject orgobj = new OrgObject();
                                        if (!string.IsNullOrWhiteSpace(item.COMPANYID))
                                        {
                                            orgobj.OrgID = item.COMPANYID;
                                            orgobj.OrgType = "0";
                                        }
                                        if (!string.IsNullOrWhiteSpace(item.DEPARTMENTID))
                                        {
                                            orgobj.OrgID = item.DEPARTMENTID;
                                            orgobj.OrgType = "1";
                                        }
                                        if (!string.IsNullOrWhiteSpace(item.POSTID))
                                        {
                                            orgobj.OrgID = item.POSTID;
                                            orgobj.OrgType = "2";
                                        }
                                        menuP.OrgObjects.Add(orgobj);
                                    }
                                    obj.PermissionValue.Add(menuP);
                                }
                                CusPers.Add(obj);

                            }
                        }
                    }


                    return CusPers;

                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("GetCutomterPermissionObj:" + ex.Message.ToString());
                return null;
            }


        }

        /// <summary>
        /// 设置用户自定义权限
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="strResult"></param>
        [OperationContract]
        public void SetCutomterPermissionObj(string RoleID, List<CustomerPermission> objs, ref string strResult)
        {
            try
            {
                using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
                {
                    if (string.IsNullOrWhiteSpace(RoleID))
                    {
                        return;
                    }


                    RoleEntityMenuBLL roleEmBll = new RoleEntityMenuBLL();
                    roleEmBll.UpdateRoleInfo(RoleID, strResult);//修改信息
                    strResult = string.Empty;                    
                    IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> cuspers = bll.GetCustomPermByRoleID(RoleID);
                    //if (cuspers != null)
                    //{
                    //    if (cuspers.Count() > 0)
                    //    {
                    //        foreach (T_SYS_ENTITYMENUCUSTOMPERM item in cuspers)
                    //        {
                    //            string strId = item.ENTITYMENUCUSTOMPERMID;
                    //            EntityMenuCustomPermDelete(strId);
                    //        }
                    //    }
                    //}
                    SysPermissionBLL bllPer = new SysPermissionBLL();
                    T_SYS_ROLE entRole = GetSysRoleSingleInfoById(RoleID);//获取角色ID实体对象

                    foreach (var Menus in objs)
                    {
                        if (Menus.PermissionValue == null)
                        {
                            continue;
                        }

                        if (Menus.PermissionValue.Count() == 0)
                        {
                            continue;
                        }
                        T_SYS_ENTITYMENU entMenu = GetSysMenuByID(Menus.EntityMenuId);
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

                            T_SYS_PERMISSION entPer = bllPer.GetAttachSysPermissionByID(Perms.Permission);//获取权限实体对象
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

                                switch (OrgIns.OrgType)
                                {

                                    //公司
                                    case "0":
                                        customerPer.COMPANYID = OrgIns.OrgID;
                                        break;
                                    case "1"://部门
                                        customerPer.DEPARTMENTID = OrgIns.OrgID;
                                        break;
                                    case "2"://岗位
                                        customerPer.POSTID = OrgIns.OrgID;
                                        break;
                                }

                                customerPer.CREATEDATE = DateTime.Now;
                                customerPer.UPDATEDATE = DateTime.Now;

                                EntityMenuCustomPermAdd(customerPer);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strResult = ex.Message;
            }
        }

        /// <summary>
        /// 设置用户自定义权限
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="strResult"></param>
        [OperationContract]
        public bool UpdateCutomterPermissionObj(string RoleID, List<CustomerPermission> objs, ref string strResult)
        {
            try
            {
                //using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
                //{
                EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL();
                    if (string.IsNullOrWhiteSpace(RoleID))
                    {
                        return false;
                    }

                    return bll.UpdateCustomerPermissionnew(RoleID, objs, ref strResult);


                //}
            }
            catch (Exception ex)
            {
                strResult = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取用户自定义权限
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public static string GetCustomerPermissionJson(string roleId)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> cuspers = bll.GetCustomPermByRoleID(roleId);
                var q = from ent in cuspers
                        group ent by new
                        {
                            ent.T_SYS_ENTITYMENU.ENTITYMENUID,
                            ent.T_SYS_PERMISSION.PERMISSIONID
                        }
                            into g
                            select new
                            {

                                MenuId = g.Key.ENTITYMENUID,
                                PermissionId = g.Key.PERMISSIONID,
                                g
                            };

                CustomerPermission obj = new CustomerPermission();
                obj.EntityMenuId = q.FirstOrDefault().MenuId;
                obj.PermissionValue = new List<PermissionValue>();

                foreach (var menus in q)
                {
                    PermissionValue menuP = new PermissionValue();
                    menuP.Permission = menus.PermissionId;
                    menuP.OrgObjects = new List<OrgObject>();
                    foreach (var item in menus.g)
                    {
                        OrgObject orgobj = new OrgObject();
                        if (!string.IsNullOrWhiteSpace(item.COMPANYID))
                        {
                            orgobj.OrgID = item.COMPANYID;
                            orgobj.OrgType = "0";
                        }
                        if (!string.IsNullOrWhiteSpace(item.DEPARTMENTID))
                        {
                            orgobj.OrgID = item.DEPARTMENTID;
                            orgobj.OrgType = "1";
                        }
                        if (!string.IsNullOrWhiteSpace(item.POSTID))
                        {
                            orgobj.OrgID = item.POSTID;
                            orgobj.OrgType = "2";
                        }
                        menuP.OrgObjects.Add(orgobj);
                    }
                    obj.PermissionValue.Add(menuP);
                }


                //objs.PermissionValue = p;
                return JsonHelper.GetJson<CustomerPermission>(obj);
            }
        }
        /// <summary>
        /// 根据系统类型获取自定义菜单权限信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统自定义菜单权限</param>
        /// <returns>自定义菜单权限信息列表</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENUCUSTOMPERM> GetEntityMenuCustomPermByType(string systemType)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> menuList = bll.GetEntityMenuCustomPermByType(systemType);
                return menuList.Count() > 0 ? menuList.ToList() : null;
            }
        }
        /// <summary>
        /// 根据自定义菜单权限ID获取系统自定义菜单权限
        /// </summary>
        /// <param name="menuID">自定义菜单权限ID</param>
        /// <returns>自定义菜单权限信息</returns>
        [OperationContract]
        public T_SYS_ENTITYMENUCUSTOMPERM GetEntityMenuCustomPermByID(string id)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                return bll.GetEntityMenuCustomPermByID(id);
            }
        }
        /// <summary>
        /// 新增自定义菜单权限
        /// </summary>
        /// <param name="obj">系统自定义菜单权限</param>
        [OperationContract]
        public void EntityMenuCustomPermAdd(T_SYS_ENTITYMENUCUSTOMPERM obj)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                bll.EntityMenuCustomPermAdd(obj);
            }
        }
        /// <summary>
        /// 修改系统自定义菜单权限
        /// </summary>
        /// <param name="obj">系统自定义菜单权限</param>
        [OperationContract]
        public void EntityMenuCustomPermUpdate(T_SYS_ENTITYMENUCUSTOMPERM obj)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                bll.EntityMenuCustomPermUpdate(obj);
            }
        }

        /// <summary>
        /// 删除系统自定义菜单权限
        /// </summary>
        /// <param name="menuID">系统自定义菜单权限编号</param>
        [OperationContract]
        public void EntityMenuCustomPermDelete(string id)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                bll.EntityMenuCustomPermDelete(id);
            }
        }

        /// <summary>
        /// 判断登录用户在某一菜单中时候有自定义权限 2010-10-19
        /// </summary>
        /// <param name="menuID">系统自定义菜单权限编号</param>
        [OperationContract]
        public T_SYS_ENTITYMENUCUSTOMPERM GetCustomerPermissionByUserIDAndEntityCode(string StrUserId, string StrEntityCode)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                return bll.GetCustomerPermissionByUserIDAndEntityCode(StrUserId, StrEntityCode);
            }
        }

        /// <summary>
        /// 获取岗位指定菜单权限
        /// </summary>
        /// <param name="menuCode">菜单编号,菜单编号为空时返回岗位所有菜单权限</param>
        /// <param name="postID">岗位ID</param>
        /// <returns>自定义菜单权限</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomPostMenuPerms(string menuCode, string postID)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                #region 龙康才新增
                IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> perms;
                string keyString = "GetCustomPostMenuPerms" + menuCode + postID;
                if (WCFCache.Current[keyString] == null)
                {

                    perms = bll.GetCustomPostMenuPerms(menuCode, postID);
                    WCFCache.Current.Insert(keyString, perms, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perms = (IQueryable<T_SYS_ENTITYMENUCUSTOMPERM>)WCFCache.Current[keyString];
                }
                #endregion
                return perms.Count() > 0 ? perms.ToList() : null;
            }
        }
        /// <summary>
        /// 获取岗位指定菜单权限
        /// </summary>
        /// <param name="menuCode">菜单编号,菜单编号为空时返回部门所有菜单权限</param>
        /// <param name="departID">部门ID</param>
        /// <returns>自定义菜单权限</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomDepartMenuPerms(string menuCode, string departID)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                #region 龙康才新增
                List<T_SYS_ENTITYMENUCUSTOMPERM> perms;
                string keyString = "GetCustomDepartMenuPerms" + menuCode + departID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> IQList = bll.GetCustomDepartMenuPerms(menuCode, departID);
                    perms = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, perms, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perms = (List<T_SYS_ENTITYMENUCUSTOMPERM>)WCFCache.Current[keyString];
                }
                #endregion
                return perms.Count() > 0 ? perms : null;
            }
        }
        /// <summary>
        /// 获取岗位指定菜单权限
        /// </summary>
        /// <param name="menuCode">菜单编号,菜单编号为空时返回公司所有菜单权限</param>
        /// <param name="companyID">公司ID</param>
        /// <returns>自定义菜单权限</returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENUCUSTOMPERM> GetCustomCompanyMenuPerms(string menuCode, string companyID)
        {
            using (EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL())
            {
                #region 龙康才新增
                List<T_SYS_ENTITYMENUCUSTOMPERM> perms;
                string keyString = "GetCustomCompanyMenuPerms" + menuCode + companyID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> IQList = bll.GetCustomCompanyMenuPerms(menuCode, companyID);
                    perms = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, perms, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    perms = (List<T_SYS_ENTITYMENUCUSTOMPERM>)WCFCache.Current[keyString];
                }
                #endregion
                return perms.Count() > 0 ? perms : null;
            }
        }

        #endregion


        #region T_SYS_USERROLE 用户角色
        /// <summary>
        /// 根据系统类型获取用户角色信息
        /// </summary>
        /// <param name="systemType">系统类型,为空时获取所有类型的系统用户角色</param>
        /// <returns>用户角色信息列表</returns>
        [OperationContract]
        public List<T_SYS_USERROLE> GetSysUserRoleByType(string systemType)
        {
            using (SysUserRoleBLL bll = new SysUserRoleBLL())
            {
                #region 龙康才新增
                List<T_SYS_USERROLE> menuList;
                string keyString = "GetSysUserRoleByType" + systemType;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_USERROLE> IQList = bll.GetSysUserRoleByType(systemType);
                    menuList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, menuList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menuList = (List<T_SYS_USERROLE>)WCFCache.Current[keyString];
                }
                #endregion
                return menuList.Count() > 0 ? menuList : null;
            }
        }
        /// <summary>
        /// 根据用户角色ID获取系统角色权限
        /// </summary>
        /// <param name="menuID">用户角色ID</param>
        /// <returns>用户角色信息</returns>
        [OperationContract]
        public T_SYS_USERROLE GetSysUserRoleByID(string menuID)
        {
            using (SysUserRoleBLL bll = new SysUserRoleBLL())
            {
                return bll.GetSysUserRoleByID(menuID);
            }

        }

        /// <summary>
        /// 根据用户获取系统角色权限
        /// </summary>
        /// <param name="menuID">用户ID</param>
        /// <returns>用户角色信息列表</returns>
        [OperationContract]
        public List<T_SYS_USERROLE> GetSysUserRoleByUser(string UserID)
        {
            using (SysUserRoleBLL bll = new SysUserRoleBLL())
            {
                #region 龙康才新增
                List<T_SYS_USERROLE> userRoleList;
                string keyString = "GetSysUserRoleByUser" + UserID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_USERROLE> IQList = bll.GetSysUserRoleByUser(UserID);
                    userRoleList = IQList == null ? null : IQList.ToList();
                    //WCFCache.Current.Insert(keyString, userRoleList, DateTime.Now.AddMinutes(15));
                    WCFCache.Current.Insert(keyString, userRoleList, DateTime.Now.AddSeconds(5));

                }
                else
                {
                    userRoleList = (List<T_SYS_USERROLE>)WCFCache.Current[keyString];
                }
                #endregion
                return userRoleList;
            }
        }




        /// <summary>
        /// 根据角色获取该角色下的所有用户信息
        /// </summary>
        /// <param name="roleID">角色ID</param>
        /// <returns>用户列表</returns>
        [OperationContract]
        public List<T_SYS_USER> GetSysUserByRole(string roleID)
        {
            using (SysUserRoleBLL bll = new SysUserRoleBLL())
            {
                #region 龙康才新增
                List<T_SYS_USER> userList;
                string keyString = "GetSysUserByRole" + roleID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_USER> IQList = bll.GetSysUserByRole(roleID);
                    userList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, userList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    userList = (List<T_SYS_USER>)WCFCache.Current[keyString];
                }
                #endregion
                return userList !=null ? userList : null;
            }
        }


        

        ///// <summary>
        ///// 新增用户角色
        ///// </summary>
        ///// <param name="obj">系统用户角色</param>
        //[OperationContract]
        //public void SysUserRoleAdd(T_SYS_USERROLE obj)
        //{
        //    SysUserRoleBLL bll = new SysUserRoleBLL();
        //    bll.SysUserRoleAdd(obj);
        //}
        /// <summary>
        /// 新增用户角色
        /// </summary>
        /// <param name="RoleList">角色列表</param>
        /// <param name="userobj">用户对象</param>
        /// <param name="StrUserName">添加人用户名</param>
        /// <param name="AddDate">添加时间</param>
        /// <returns></returns>
        [OperationContract]
        //添加用户角色
        public bool UserRoleBatchAddInfos(T_SYS_ROLE[] RoleList, T_SYS_USER userobj, string StrUserName, DateTime AddDate)
        {

            using (SysUserRoleBLL UserRoleBll = new SysUserRoleBLL())
            {
                //先清缓存GetSysUserRoleByUser
                string keyString = "GetSysUserRoleByUser" + userobj.SYSUSERID;
                WCFCache.Current[keyString] = null;
                return UserRoleBll.BatchAddUserRoleInfo(userobj, RoleList, StrUserName, AddDate);
            }
        }
        /// <summary>
        /// 通过用户角色列表批量添加信息
        /// </summary>
        /// <param name="RoleObj"></param>
        /// <returns></returns>
        [OperationContract]
        public bool BatchAddUserRole(List<T_SYS_USERROLE> RoleObj, ref string IsResult)
        {
            using (SysUserRoleBLL UserRoleBll = new SysUserRoleBLL())
            {
                return UserRoleBll.BatchAddUserRoleInfoByUserRoleList(RoleObj, ref IsResult);
            }
        }

        [OperationContract]
        //修改用户角色
        public void UserRoleUpdateInfo(T_SYS_USERROLE UserRoleObj)
        {
            using (SysUserRoleBLL UserRoleBll = new SysUserRoleBLL())
            {
                UserRoleBll.SysUserRoleUpdate(UserRoleObj);
            }
        }

        /// <summary>
        /// 修改系统用户角色
        /// </summary>
        /// <param name="obj">系统用户角色</param>
        [OperationContract]
        public void SysUserRoleUpdate(T_SYS_USERROLE obj)
        {
            using (SysUserRoleBLL bll = new SysUserRoleBLL())
            {
                bll.SysUserRoleUpdate(obj);
            }
        }

        /// <summary>
        /// 删除系统用户角色
        /// </summary>
        /// <param name="menuID">系统用户角色编号</param>
        [OperationContract]
        public void SysUserRoleDelete(string userroleid)
        {
            using (SysUserRoleBLL bll = new SysUserRoleBLL())
            {
                T_SYS_USERROLE role = new T_SYS_USERROLE();
                role = bll.GetSysUserRoleByID(userroleid);
                if (role != null)
                { 
                    string keyString = "GetSysUserRoleByUser" + role.T_SYS_USER.SYSUSERID;
                    WCFCache.Current[keyString] = null;
                }
                
                bll.SysUserRoleDelete(userroleid);
            }
        }



        #endregion

        #region T_SYS_USER 用户信息及权限
        /// <summary>
        /// 根据用户名称得到用户信息
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户信息</returns>
        [OperationContract]
        public T_SYS_USER GetUserInfo(string userName)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                return bll.GetUserInfo(userName);
            }
        }

        /// <summary>
        /// 根据用户ID得到用户信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>用户信息</returns>
        [OperationContract]
        public T_SYS_USER GetUserByID(string userID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                return bll.GetUserByID(userID);
            }
        }


        private static int GetUserByEmployeeIDTimes = 0;
        /// <summary>
        /// 根据员工ID得到用户信息
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>用户信息</returns>
        [OperationContract]
        public T_SYS_USER GetUserByEmployeeID(string employeeID)
        {

            using (SysUserBLL bll = new SysUserBLL())
            {
                #region 龙康才新增
                T_SYS_USER plist;
                string keyString = "GetUserByEmployeeID" + employeeID;
                if (WCFCache.Current[keyString] == null)
                {


                    plist = bll.GetUserByEmployeeID(employeeID);
                    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(15));


                }
                else
                {
                    plist = (T_SYS_USER)WCFCache.Current[keyString];

                }
                #endregion

                return plist;
            }
        }


        private static int getPermissionTimes = 0;
        /// <summary>
        /// 根据用户名称得到用户所拥有的权限
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        [OperationContract]
        public List<V_Permission> GetUserPermissionByUser(string userID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                SysUserRoleBLL RoleBll = new SysUserRoleBLL();
                string StrResult = "";
                RoleBll.GetSystemTypeByUserID(userID, ref StrResult);

                #region 龙康才新增
                List<V_Permission> plist;
                string keyString = "GetUserPermissionByUser" + userID;
                if (WCFCache.Current[keyString] == null)
                {

                    IQueryable<V_Permission> IQList = bll.GetUserPermissionByUser(userID);
                    plist = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(15));


                }
                else
                {
                    plist = (List<V_Permission>)WCFCache.Current[keyString];
                }
                #endregion

                return plist.Count() > 0 ? plist : null;
            }
        }

        /// <summary>
        /// 修改预算管理员
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <param name="ownercompanyid">公司ID</param>
        /// <param name="fbAdmin">新员工信息（只需要取员工信息）</param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateFbAdmin(string employeeid, string ownercompanyid, T_SYS_FBADMIN fbAdmin)
        {
            using (FbAdminBLL bll = new FbAdminBLL())
            {
                return bll.UpdateFbAdmin(employeeid, ownercompanyid, fbAdmin);
            }
        }
        /// <summary>
        /// 根据用户名称得到用户所拥有的权限  简化版 2010-9-27 添加了预算管理员的判断 2011-12-15
        /// 
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        [OperationContract]
        public List<V_UserPermissionUI> GetUserPermissionByUserToUI(string userID)
        {

            using (SysUserBLL bll = new SysUserBLL())
            {
                #region 龙康才新增
                List<V_UserPermissionUI> plist;                
                T_SYS_FBADMIN UserFb = null;
                string keyString = "GetUserPermissionByUserToUI" + userID;
                if (WCFCache.Current[keyString] == null)
                {
                    
                    if (!string.IsNullOrEmpty(userID))
                    {
                        UserFb = this.getFbAdmin(userID);
                    }
                    IQueryable<V_UserPermissionUI> IQList = UserFb != null ? bll.GetUserPermissionByUserToUI(userID, "") : bll.GetUserPermissionByUserToUINotForFbAdmin(userID, "");
                    plist = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(15));
                }
                else
                {
                    plist = (List<V_UserPermissionUI>)WCFCache.Current[keyString];
                }
                #endregion

                return plist !=null ? plist : null;
            }
        }

        /// <summary>
        /// 根据用户名称得到用户所拥有的权限  简化版 2010-9-27
        /// 2010-11-10 添加菜单ID过滤
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        [OperationContract]
        public List<V_UserPermissionUI> GetEntityPermissionByUser(string userID, string StrMenuId)
        {

            using (SysUserBLL bll = new SysUserBLL())
            {
                IQueryable<V_UserPermissionUI> plist;
                plist = bll.GetUserPermissionByUserToUI(userID, StrMenuId);
                return plist.Count() > 0 ? plist.ToList() : null;
            }
        }


        private static int GetUserMenuPermsTimes = 0;
        /// <summary>
        /// 获取用户菜单的权限范围
        /// </summary>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="userID">用户ID</param>
        /// <returns>权限</returns>
        [OperationContract]
        public List<V_Permission> GetUserMenuPerms(string menuCode, string userID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                #region 龙康才新增
                List<V_Permission> plist;
                string keyString = "UserMenuPerms" + menuCode + userID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_Permission> IQList = bll.GetUserMenuPerms(menuCode, userID);
                    plist = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(15));


                }
                else
                {
                    Tracer.Debug(keyString);
                    //Tracer.Debug("--" + "-------调用了缓存-----------------");
                    plist = (List<V_Permission>)WCFCache.Current[keyString];

                }
                //Tracer.Serializer(plist, "V_Permission");
                #endregion

                return plist == null ? null : plist;

            }
        }


        /// <summary>
        /// 获取用户菜单的权限范围
        /// </summary>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="userID">用户ID</param>
        /// <returns>权限</returns>
        [OperationContract]
        public List<V_UserPermission> GetUserMenuPermsByUserPermission(string menuCode, string userID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                #region 龙康才新增
                List<V_UserPermission> plist;
                string keyString = "UserMenuPermsstring" + menuCode + userID;
                if (WCFCache.Current[keyString] == null)
                {

                    IQueryable<V_UserPermission> IQList = bll.GetUserMenuPermsByUserPermision(menuCode, userID);
                    plist = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    Tracer.Debug(keyString);
                    //Tracer.Debug("--" + "-------调用了缓存-----------------");
                    plist = (List<V_UserPermission>)WCFCache.Current[keyString];

                }
                //Tracer.Serializer(plist, "V_Permission");
                #endregion

                return plist == null ? null : plist;
            }

        }

        /// <summary>
        /// 根据角色ID该角色所有的权限  2010-6-13 liujx
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        [OperationContract]
        public List<V_Permission> GetPermissionByRoleID(string RoleID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                #region 龙康才新增
                List<V_Permission> plist;
                string keyString = "GetPermissionByRoleID" + RoleID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_Permission> IQList = bll.GetPermissionByRoleID(RoleID);
                    plist = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    plist = (List<V_Permission>)WCFCache.Current[keyString];
                }
                #endregion
                return plist.Count() > 0 ? plist : null;
            }
        }

        /// <summary>
        /// 根据角色ID该角色所有的权限  2010-9-28 liujx
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>用户所拥有的权限列表</returns>
        [OperationContract]
        public List<V_UserPermissionRoleID> GetPermissionByRoleIDSecond(string RoleID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                #region 龙康才新增
                List<V_UserPermissionRoleID> plist;
                string keyString = "GetPermissionByRoleIDSecond" + RoleID;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_UserPermissionRoleID> IQList = bll.GetPermissionByRoleIDSecond(RoleID);
                    plist = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    plist = (List<V_UserPermissionRoleID>)WCFCache.Current[keyString];
                }
                #endregion
                return plist.Count() > 0 ? plist : null;
            }
        }

        /// <summary>
        /// 获取所有用户信息
        /// </summary>
        /// <returns>返回用户信息列表</returns>
        [OperationContract]
        public List<T_SYS_USER> GetUserPermissionAll()
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                #region 龙康才新增
                List<T_SYS_USER> pList;
                string keyString = "GetUserPermissionAll";
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_USER> IQList = bll.GetUserPermissionAll();
                    pList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, pList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    pList = (List<T_SYS_USER>)WCFCache.Current[keyString];
                }
                #endregion
                return pList.Count() > 0 ? pList : null;
            }
        }
        /// <summary>
        /// 用户登录验证
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="Pwd">密码：加密后的密码</param>
        /// <returns>登录成功，返回系统用户信息</returns>
        [OperationContract]
        public T_SYS_USER UserLogin(string userName, string Pwd)
        {
            try
            {
                using (SysUserBLL bll = new SysUserBLL())
                {
                    string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
                    return bll.AddUserLoginHis(userName, Ip);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录错误信息：" + System.DateTime.Now.ToString("d") + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取登录用户信息
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
        [OperationContract]
        public List<V_OnlineUser> GetOnLineUsers(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string[] CompanyIDs)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                return bll.GetOnLineUsers(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, CompanyIDs);
            }
        }

        #endregion

        #region Lookup 数据查询实现
        /// <summary>
        /// Lookup控件查询Entity的方法
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>Entity记录集Xml</returns>

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="args"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public string GetLookupOjbects(EntityNames entityName, Dictionary<string, string> args, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            string objxml = "";
            EntityObject[] ents = Utility.GetLookupData(entityName, args, pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
            if (ents != null)
            {
                //  List<EntityObject> list = ents.ToList();
                objxml = Utility.SerializeObject(ents);
            }

            //例子
            //List<T_SYS_DICTIONARY> list1 = new List<T_SYS_DICTIONARY>();
            //list1 = BLLUtility.DeserializeObject<List<T_SYS_DICTIONARY>>(objxml);
            return objxml;
        }
        #endregion

        #region 系统用户

        [OperationContract]
        //增
        public string SysUserInfoAdd(T_SYS_USER obj)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                string returnStr = "";
                if (!this.IsExistSysUserInfo(obj.USERNAME))
                {
                    bool sucess = UserBll.AddSysUserInfo(obj);
                    if (sucess == false)
                    {
                        returnStr = "添加系统用户失败";
                    }
                    else
                    {
                        //清空即时通讯的缓存
                        string keyString = "ImInstantLoginUsers";
                        WCFCache.Current[keyString] = null;
                    }
                }
                else
                {

                    returnStr = "系统用户已经存在";
                }
                return returnStr;
            }
        }

        [OperationContract]
        public void SysUserInfoAddORUpdate(T_SYS_USER obj, ref string strMsg)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                //清空即时通讯的缓存
                string keyString = "ImInstantLoginUsers";
                WCFCache.Current[keyString] = null;
                UserBll.SysUserInfoAddORUpdate(obj, ref strMsg);
            }
        }

        [OperationContract]
        private bool IsExistSysUserInfo(string StrUserName)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                return UserBll.GetSysUserInfoByAdd(StrUserName);
            }
        }

        [OperationContract]
        //改
        public bool SysUserInfoUpdate(T_SYS_USER obj)
        {
            Tracer.Debug("修改了用户信息" + obj.USERNAME + obj.EMPLOYEENAME);
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                string keyString = "GetUserByEmployeeID" + obj.EMPLOYEEID;
                WCFCache.Current[keyString] = null;
                //清空即时通讯的缓存
                string InstantkeyString = "ImInstantLoginUsers";
                WCFCache.Current[InstantkeyString] = null;
                //Tracer.Debug("修改了用户信息"+obj.USERNAME + obj.EMPLOYEENAME);
                return UserBll.UpdateSysUserInfo(obj);
            }
        }

        /// <summary>
        /// 员工离职确认
        /// </summary>
        /// <param name="obj">系统用户</param>
        /// <param name="StrOwnerCompanyid">公司ID</param>
        /// <param name="StrPostid">岗位ID</param>
        /// <param name="IsMain">是否主职，如果是主职则删除所有的角色</param>
        /// <returns></returns>
        [OperationContract]        
        public bool SysUserInfoUpdateForEmployeeLeft(T_SYS_USER obj,string StrOwnerCompanyid,string StrPostid,bool IsMain)
        {
            Tracer.Debug("修改了用户信息" + obj.USERNAME + obj.EMPLOYEENAME);
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                string keyString = "GetUserByEmployeeID" + obj.EMPLOYEEID;
                WCFCache.Current[keyString] = null;
                //清空即时通讯的缓存
                string InstantkeyString = "ImInstantLoginUsers";
                WCFCache.Current[InstantkeyString] = null;
                //Tracer.Debug("修改了用户信息"+obj.USERNAME + obj.EMPLOYEENAME);
                return UserBll.UpdateSysUserInfoForEmployeeLeftOffice(obj, StrOwnerCompanyid, StrPostid, IsMain);
            }
        }


        /// <summary>
        /// 通过用户名修改密码
        /// </summary>
        /// <param name="sysuserid"></param>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [OperationContract]        
        public bool SysUserInfoUpdateByUserIdandUsername(string userid, string username, string pwd)
        {

            using (SysUserBLL UserBll = new SysUserBLL())
            {
                string keyString = "GetUserByEmployeeID" + userid;
                WCFCache.Current[keyString] = null;
                //清空即时通讯的缓存
                string InstantkeyString = "ImInstantLoginUsers";
                WCFCache.Current[InstantkeyString] = null;
                //Tracer.Debug("修改了用户信息"+obj.USERNAME + obj.EMPLOYEENAME);
                return UserBll.UpdateSysUserPasswordByUsername(userid,username,pwd);
            }

        }

        [OperationContract]
        //改用户密码及AD用户密码   zl
        public bool SysUserInfoUpdateToRtxAndEmail(string UserAccount, string NewPassword1, string OldPassword1, string NewPassword, out string strMsg)
        {

            AD_Users adUsers = new AD_Users();
            adUsers.SetPassword(UserAccount, NewPassword1, OldPassword1, NewPassword, out strMsg);
            if (string.IsNullOrEmpty(strMsg))
            {
                //清空即时通讯的缓存
                string InstantkeyString = "ImInstantLoginUsers";
                WCFCache.Current[InstantkeyString] = null;
                return true;
            }
            else
            {
                return false;
            }

        }
        //所有系统角色信息
        [OperationContract]
        public List<T_SYS_USER> GetSysUserAllInfos()
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                List<T_SYS_USER> SysUserInfosList = UserBll.GetAllSysUserInfos();
                if (SysUserInfosList == null)
                {
                    return null;
                }
                else
                {
                    return SysUserInfosList;
                }
            }
        }

        [OperationContract]
        //所有用户信息 2010-6-10
        public List<T_SYS_USER> GetSysUserAllInfosPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                IQueryable<T_SYS_USER> recordList = UserBll.GetAllSysUserInfosWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);
                return recordList == null ? null : recordList.ToList();
            }
        }
        [OperationContract]
        //所有用户信息 根据用户所属公司获取 2010-8-24
        public List<T_SYS_USER> GetSysUserAllInfosPagingByCompanyIDs(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo, string[] CompanyIDs)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                IQueryable<T_SYS_USER> recordList = UserBll.GetAllSysUserInfosWithPagingByCompanyIDs(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, CompanyIDs);
                return recordList == null ? null : recordList.ToList();
            }
        }

        [OperationContract]
        //批量删除
        public string SysUserBatchDel(string[] StrSysUserIDs)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                //清空即时通讯的缓存
                string InstantkeyString = "ImInstantLoginUsers";
                WCFCache.Current[InstantkeyString] = null;
                return UserBll.BatchDeleteSysUserInfos(StrSysUserIDs);
            }
        }
        [OperationContract]
        //批量修改用户状态
        public string SysUserBatchUpdate(string[] StrSysUserIDs, string StrUserID, string UserName, string UserState)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                //清空对应的用户的缓存
                if (StrSysUserIDs.Count() > 0)
                {
                    //清空即时通讯的缓存
                    string InstantkeyString = "ImInstantLoginUsers";
                    WCFCache.Current[InstantkeyString] = null;
                    for (int i = 0; i < StrSysUserIDs.Count(); i++)
                    {
                        string keyString = "GetUserByEmployeeID" + StrSysUserIDs[i];
                        WCFCache.Current[keyString] = null;
                    }
                }
                return UserBll.BatchUpdateSysUserInfos(StrSysUserIDs, StrUserID, UserName, UserState);
            }
        }
        #endregion

        #region 用户登录记录

        [OperationContract]
        //增
        public bool SysUserLoginRecordInfoAdd(T_SYS_USERLOGINRECORD obj)
        {
            using (SysUserLoginRecordBll UserLoginBll = new SysUserLoginRecordBll())
            {
                return UserLoginBll.AddUserLoginInfo(obj);
            }
        }



        [OperationContract]
        //改
        public bool SysUserLoginRecordInfoUpdate(T_SYS_USERLOGINRECORD obj)
        {
            using (SysUserLoginRecordBll UserLoginBll = new SysUserLoginRecordBll())
            {
                return UserLoginBll.UpdateUserLoginRecordStateInfo(obj);
            }
        }


        [OperationContract]
        //所有用户登录信息
        public List<V_UserLoginRecord> GetSysUserLoginRecordInfos()
        {
            using (SysUserLoginRecordBll UserLoginBll = new SysUserLoginRecordBll())
            {
                #region 龙康才新增
                List<V_UserLoginRecord> recordList;
                string keyString = "GetSysUserLoginRecordInfos";
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_UserLoginRecord> IQList = UserLoginBll.GetAllUserLoginRecordInfos();
                    recordList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, recordList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    recordList = (List<V_UserLoginRecord>)WCFCache.Current[keyString];
                }
                #endregion
                return recordList.Count() > 0 ? recordList.ToList() : null;
            }
        }
        /// <summary>
        /// 用户注销
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="LoginRecordId"></param>
        /// <returns></returns>
        [OperationContract]
        public bool UserLoginOut(string UserID, string LoginRecordId)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
                return bll.LoginOut(UserID, LoginRecordId, Ip);
            }
        }
        [OperationContract]
        public bool UserLoginOperation(string UserID, string LoginRecordId, bool IsLogin)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
                return bll.LoginOutConfirm(UserID, LoginRecordId, Ip, IsLogin);
            }
        }

        [OperationContract]
        //所有用户登录信息 2010-6-10
        public List<V_UserLoginRecord> GetSysUserLoginRecordInfosPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (SysUserLoginRecordBll UserLoginBll = new SysUserLoginRecordBll())
            {
                IQueryable<V_UserLoginRecord> recordList = UserLoginBll.GetAllUserLoginRecordInfosWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);
                SysUserBLL bll = new SysUserBLL();
                //string OwnerCompanyIDs = "";
                //string OwnerDepartmentIDs = "";
                //string OwnerPositionIDs = "";
                //IQueryable<V_BllCommonUserPermission> plist;
                //plist = bll.GetUserMenuPermsByUserPermisionBllCommon("T_HR_SALARYSOLUTION", "2e41f129-cab4-4864-832e-6a2d6fba2750", ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);
                //plist = bll.GetUserMenuPermsByUserPermisionBllCommonAddPermissionValue("T_FB_DEPTBUDGETAPPLYMASTER", "286ab878-be35-4cb9-83d4-51e4f0344637", ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs, "0");
                //plist = bll.GetUserMenuPermsByUserPermisionBllCommon("T_HR_COMPANY", "2e67b94c-888f-4488-8663-d356740efcc6", ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);
                //plist = bll.GetUserMenuPermsByUserPermisionBllCommon("T_OA_WORKRECORD", "5dabd014-e418-4d0e-a88e-440ec012857e", ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);
                if (recordList == null)
                {
                    return null;
                }
                else
                {
                    return recordList.ToList();
                }
            }
        }


        [OperationContract]
        //查询用户登录信息
        public List<V_UserLoginRecord> GetSysUserLoginRecordInfosBySearch(string StrState, DateTime DtStart, DateTime DtEnd)
        {
            using (SysUserLoginRecordBll UserLoginBll = new SysUserLoginRecordBll())
            {
                #region 龙康才新增
                List<V_UserLoginRecord> LoginList;
                string keyString = "UserLoginRecordInfos" + StrState + DtStart.ToString() + DtEnd.ToString(); ;
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<V_UserLoginRecord> IQList = UserLoginBll.GetUserLoginRecordInfosBySearch(StrState, DtStart, DtEnd);
                    LoginList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, LoginList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    LoginList = (List<V_UserLoginRecord>)WCFCache.Current[keyString];
                }
                return LoginList.Count() > 0 ? LoginList : null;
                #endregion
            }
        }
        [OperationContract]
        //获取员工1时间段的登录信息
        public List<T_SYS_USERLOGINRECORD> GetUserLoginRecordByEmployeeIDAndDate(string employeeid, DateTime start, DateTime end)
        {
            using (SysUserLoginRecordBll UserLoginBll = new SysUserLoginRecordBll())
            {
                IQueryable<T_SYS_USERLOGINRECORD> LoginList = UserLoginBll.GetUserLoginRecordByDate(employeeid, start, end);
                return LoginList.Count() > 0 ? LoginList.ToList() : null;
            }
        }
        #endregion

        #region 用户登录历史记录

        [OperationContract]
        //增
        public bool SysUserLoginHistoryRecordInfoAdd(T_SYS_USERLOGINRECORDHIS obj)
        {
            using (SysUserLoginRecordHistoryBll UserLoginHistoryBll = new SysUserLoginRecordHistoryBll())
            {
                return UserLoginHistoryBll.AddUserLoginHistoryInfo(obj);
            }
        }

        [OperationContract]
        //改
        public bool SysUserLoginHistoryRecordInfoUpdate(T_SYS_USERLOGINRECORDHIS obj)
        {
            using (SysUserLoginRecordHistoryBll UserLoginHistoryBll = new SysUserLoginRecordHistoryBll())
            {
                return UserLoginHistoryBll.UpdateUserLoginHistoryRecordStateInfo(obj);
            }
        }


        [OperationContract]
        //所有用户登录信息
        public List<V_UserLoginRecordHistory> GetSysUserLoginHistoryRecordAllInfos()
        {
            using (SysUserLoginRecordHistoryBll UserLoginHistoryBll = new SysUserLoginRecordHistoryBll())
            {
                IQueryable<V_UserLoginRecordHistory> HistoryList = UserLoginHistoryBll.GetAllUserLoginHistoryRecordInfos();
                return HistoryList.Count() > 0 ? HistoryList.ToList() : null;
            }
        }

        [OperationContract]
        //所有用户登录信息 2010-6-10
        public List<V_UserLoginRecordHistory> GetSysUserLoginHistoryRecordAllInfosPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (SysUserLoginRecordHistoryBll UserLoginHistoryBll = new SysUserLoginRecordHistoryBll())
            {
                IQueryable<V_UserLoginRecordHistory> recordList = UserLoginHistoryBll.GetAllUserLoginHistoryRecordInfosWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);
                return recordList == null ? null : recordList.ToList();
            }
        }


        #endregion

        #region 角色菜单权限
        [OperationContract]
        public List<T_SYS_ROLEMENUPERMISSION> GetRoleEntityPermissionByRoleEntityID(string RoleEntityID)
        {
            using (SysRoleMenuPermBLL RoleEntityPermBll = new SysRoleMenuPermBLL())
            {
                #region 龙康才新增
                List<T_SYS_ROLEMENUPERMISSION> InfosList;
                string keyString = "GetRoleEntityPermissionByRoleEntityID" + RoleEntityID.ToString();
                if (WCFCache.Current[keyString] == null)
                {
                    IQueryable<T_SYS_ROLEMENUPERMISSION> IQList = RoleEntityPermBll.GetRoleEntityPermissionListByRoleEntityID(RoleEntityID);
                    InfosList = IQList == null ? null : IQList.ToList();
                    WCFCache.Current.Insert(keyString, InfosList, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    InfosList = (List<T_SYS_ROLEMENUPERMISSION>)WCFCache.Current[keyString];
                }
                #endregion
                return InfosList.Count() > 0 ? InfosList : null;
            }
        }
        ///// <summary>
        ///// 返回T_SYS_ROLEENTITYPERM中 ROLEENTITYID的集合
        ///// </summary>
        ///// <param name="RoleEntityID"></param>
        ///// <returns></returns>
        //[OperationContract]
        //public List<T_SYS_ROLEMENUPERMISSION> GetRoleEntityPermissionByRoleEntityIDsList(string[] RoleEntityID)
        //{
        //    SysRoleMenuPermBLL RoleEntityPermBll = new SysRoleMenuPermBLL();
        //    //List<T_SYS_ROLEMENUPERMISSION> InfosList = RoleEntityPermBll.GetRoleEntityPermissionListByRoleEntityIDsList(RoleEntityID);
        //    List<T_SYS_ROLEMENUPERMISSION> InfosList = RoleEntityPermBll.GetPermissions(RoleEntityID);

        //    return InfosList.Count() > 0 ? InfosList.ToList() : null;
        //}
        /// <summary>
        /// 返回T_SYS_ROLEENTITYPERM中 ROLEENTITYID的集合
        /// </summary>
        /// <param name="RoleEntityID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_ROLEMENUPERMISSION> GetRolePerms(string[] RoleEntityID)
        {


            using (SysRoleMenuPermBLL RoleEntityPermBll = new SysRoleMenuPermBLL())
            {
                List<T_SYS_ROLEMENUPERMISSION> InfosList;
                InfosList = RoleEntityPermBll.GetPermissions(RoleEntityID);

                List<T_SYS_ROLEMENUPERMISSION> roleMenuPerList = new List<T_SYS_ROLEMENUPERMISSION>();
                foreach (var q in InfosList)
                {
                    T_SYS_ROLEMENUPERMISSION temp = new T_SYS_ROLEMENUPERMISSION();

                    temp.T_SYS_PERMISSION = new T_SYS_PERMISSION();
                    temp.T_SYS_PERMISSION.PERMISSIONID = q.T_SYS_PERMISSION.PERMISSIONID;
                    temp.T_SYS_ROLEENTITYMENU = new T_SYS_ROLEENTITYMENU();
                    temp.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID = q.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID;
                    temp.DATARANGE = q.DATARANGE;
                    roleMenuPerList.Add(temp);
                }
                return roleMenuPerList != null ? roleMenuPerList : null;
            }
        }

        /// <summary>
        /// 根据登录用户获取  获取其所拥有的系统 返回格式为 0,1,2
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        [OperationContract]
        public string GetSystemTypeByUserID(string UserID, ref string StrResult)
        {

            using (SysUserRoleBLL RoleBll = new SysUserRoleBLL())
            {
                return RoleBll.GetSystemTypeByUserID(UserID, ref StrResult);
                //InstantMessagingServices instang = new InstantMessagingServices();
                //instang.EmployeeLogin("zhangwei", "5D5CF0D006578902A0D50FF33ABC2D6F");
                //return null;
            }
        }

        #endregion

        #region 获取IP
        /// <summary>
        /// lll
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public string ReturnIpAddress()
        {
            string IpAddress = "";
            IpAddress = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
            return IpAddress;

        }

        /// <summary>
        /// 供新框架使用  根据系统名、菜单名、用户ID、操作值 返回是否有权限 2011-4-20
        /// </summary>
        /// <param name="SysCode"></param>
        /// <param name="menuCode"></param>
        /// <param name="userID"></param>
        /// <param name="PermissionValue"></param>
        /// <returns></returns>
        [OperationContract]
        public bool GetUserPermissionBySysCodeAndUserAndModelCode(string SysCode, string menuCode, string userID, string PermissionValue)
        {
            bool IsReturn = false;
            using (SysUserBLL bll = new SysUserBLL())
            {
                IsReturn = bll.GetUserPermissionBySysCodeAndUserAndModelCode(SysCode, menuCode, userID, PermissionValue);
            }

            return IsReturn;

        }

        #endregion

        /// <summary>
        /// 根据用户获取其所拥有的自定义权限的 公司ID集合部门ID集合  2011-1-13
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="lsDepartmentids"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetCompanyidsAndDepartmentidsByUserid(string userid, ref List<string> lsDepartmentids)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                return UserBll.GetCompanyidsAndDepartmentidsByUserid(userid, ref lsDepartmentids);
            }

        }

        /// <summary>
        /// 根据字典类型获取所有的字典值,根据时间段取值
        /// </summary>
        /// <param name="sysType">字典类型,为空时获取所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        [OperationContract]
        public List<V_Dictionary> GetSysDictionaryByCategoryByUpdateDate(string category, DateTime dt)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                IQueryable<V_Dictionary> dictlist = bll.GetSysDictionaryByCategoryByUpdateDate(category, dt);
                if (dictlist != null)
                {
                    return dictlist.Count() > 0 ? dictlist.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取用户的操作是否合法：修改、删除
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        [OperationContract]
        public bool GetUserOperationPermission(EntityPermission permission, string employeeid)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                return bll.GetUserOperationPermission(permission, employeeid);
            }
        }

        [OperationContract]
        public string GetUserNameIsExistNameAddOne(string UserName, string employeeid)
        {
            using (SysUserBLL UserBll = new SysUserBLL())
            {
                return UserBll.GetUserNameIsExistAddOne(UserName, employeeid);
            }
        }
        /// <summary>
        /// 获取自定义权限菜单
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_ENTITYMENU> GetCustomerPermissionMenus(string employeeid)
        {
            using (SysEntityMenuBLL MenurBll = new SysEntityMenuBLL())
            {
                IQueryable<T_SYS_ENTITYMENU> ents = MenurBll.GetCustomerPermissionMenus(employeeid);
                return ents == null ? null : ents.ToList();

            }
        }
        /// <summary>
        /// 根据字典类型集合获取对应的字典集合
        /// 此方法是个视图，减少了字段节约网络流量
        /// </summary>
        /// <param name="categorys">类型集合</param>
        /// <returns>返回对应的字典集合</returns>
        [OperationContract]
        public List<V_Dictionary> GetDictionaryByCategoryArray(string[] categorys)
        {
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                IQueryable<V_Dictionary> Listmenus = bll.GetSysDictionaryByCategoryArray(categorys);
                return Listmenus != null ? Listmenus.ToList() : null;
            }
        }
        [OperationContract]
        public List<T_SYS_ROLE> GetRoleInfosByUser(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string flagState, LoginUserInfo loginUserInfo)//0待审核  1已审核
        {
            SysRoleBLL roleBll = new SysRoleBLL();
            
            //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            //{
            if (flagState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                IQueryable<T_SYS_ROLE> approvalList = roleBll.GetUserRoleByUser(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, null, flagState);
                if (approvalList == null)
                {
                    return null;
                }
                else
                {
                    return approvalList.ToList();
                }
            }
            else//审批人
            {
                //SMT.SaaS.BLLCommonServices.BllCommonUserPermissionWS.BllCommonPermissionServicesClient ServiceClient = new BLLCommonServices.BllCommonUserPermissionWS.BllCommonPermissionServicesClient();

                //ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (flagState == "4")
                {
                    isView = "0";
                }
                //FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "", isView, "T_SYS_ROLEAPP", loginUserInfo.companyID, loginUserInfo.userID);
                //if (flowList == null)
                //{
                //    return null;
                //}
                List<string> guidStringList = new List<string>();
                //foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                //{
                //    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T.FORMID);
                //}
                //if (guidStringList.Count < 1)
                //{
                //    return null;
                //}
                IQueryable<T_SYS_ROLE> approList = roleBll.GetUserRoleByUser(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, guidStringList, flagState);
                if (approList == null)
                {
                    return null;
                }
                else
                {
                    return approList.ToList();
                }
            }
        }



        #region 省会城市
        /// <summary>
        /// 获取省会城市
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_COUNTRY> GetCountry()
        {
            using (provinceBll bll = new provinceBll())
            {
                return bll.GetCountry();
            }
        }

        /// <summary>
        /// 获取每个国家对应的省、市地区
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_ProvinceCity> GetProvinceCity(string countryId)
        {
            using (provinceBll bll = new provinceBll())
            {
                return bll.GetProvinceCity(countryId);
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RoleList"></param>
        /// <param name="userobj"></param>
        /// <param name="StrUserName"></param>
        /// <param name="AddDate"></param>
        /// <returns></returns>
        [OperationContract]        
        public string BatchAddFBAdmins(List<T_SYS_FBADMIN> lstAdmin)
        {

            using (FbAdminBLL UserRoleBll = new FbAdminBLL())
            {
                //先清缓存
                if (lstAdmin.Count() > 0)
                {
                    List<T_SYS_USER> users = this.GetSysUserAllInfos();
                    for (int i = 0; i < lstAdmin.Count(); i++)
                    {
                        T_SYS_USER EntUser = new T_SYS_USER();
                        EntUser = users.Where(p=>p.EMPLOYEEID == lstAdmin[i].EMPLOYEEID).FirstOrDefault();
                        string keyString = "GetSysUserRoleByUser" + EntUser.SYSUSERID;
                        WCFCache.Current[keyString] = null;
                    }
                    
                }
                return UserRoleBll.AddFbAdmin(lstAdmin);
                
            }
        }

        /// <summary>
        /// 返回预算管理员的集合
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="companyids"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_FBAdmin> getFbAdmins(string userID, List<string> companyids)
        {
            using (FbAdminBLL bll = new FbAdminBLL())
            {
                IQueryable<V_FBAdmin> Admins= bll.GetFbAdmins(userID,companyids);
                return Admins == null ? null : Admins.ToList();

            }
        }

        /// <summary>
        /// 根据用户系统ID判断用户是否是预算管理员
        /// </summary>
        /// <param name="sysUserID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_SYS_FBADMIN getFbAdmin(string sysUserID)
        {
            using (FbAdminBLL bll = new FbAdminBLL())
            {
                T_SYS_FBADMIN Admin = bll.getFbAdminBySysUserID(sysUserID);
                return Admin ;

            }
        }

        [OperationContract]
        public string DeleteFbAdmin(string employeeId, string ownercompanyId)
        {
            using (FbAdminBLL bll = new FbAdminBLL())
            {
                //先获取用户信息，再清空权限缓存
                T_SYS_USER User = this.GetUserByEmployeeID(employeeId);
                if (User != null)
                {
                    string keyString = "GetSysUserRoleByUser" + User.SYSUSERID;
                    WCFCache.Current[keyString] = null;
                }
                
                return bll.DeleteFbAdmin(employeeId,ownercompanyId);
                
            }
        }

        /// <summary>
        /// 根据用户ID获取用户是超级预算管理员或公司管理员
        /// 返回值：        
        /// </summary>
        /// <param name="sysUserID"></param>
        /// <returns>
        /// 0：系统有错误
        /// 1：公司预算员
        /// 2：超级预算员
        /// </returns>
        [OperationContract]
        public int getFbAdminByEmployeeID(string EmployeeID)
        {
            using (FbAdminBLL bll = new FbAdminBLL())
            {
                int IntResult  = bll.getFbAdminByUserID(EmployeeID);
                return IntResult;

            }
        }



        #region 引擎调用用户信息
        /// <summary>
        /// 通过角色ID:(查找所有人员，包括人员所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="roleID">角色ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<FlowUserInfo> GetFlowUserInfoByRoleID(string roleID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                Tracer.Debug("流程调用了GetFlowUserInfoByRoleID："+ roleID);
                return bll.GetFlowUserInfoByRoleID(roleID);
            }
        }

        /// <summary>
        /// 通过部门ID:(查找部门负责人,包括人员所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="departmentID">部门ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<FlowUserInfo> GetDepartmentHeadByDepartmentID(string departmentID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                Tracer.Debug("流程调用了GetDepartmentHeadByDepartmentID：" + departmentID);
                return bll.GetDepartmentHeadByDepartmentID(departmentID);
            }
        }

        /// <summary>
        ///通过岗位ID: (查找[直接上级]，[隔级上级]，包括所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<FlowUserInfo> GetSuperiorByPostID(string postID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                Tracer.Debug("流程调用了GetSuperiorByPostID：" + postID);
                return bll.GetSuperiorByPostID(postID);
            }
        }

        /// <summary>
        /// 通过角色ID和公司ID:(查找所有人员，包括人员所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="roleID">角色ID</param>
        /// <param name="companyID">公司ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<FlowUserInfo> GetUserByRoleIDAndCompanyID(string roleID, string companyID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                Tracer.Debug("流程调用了GetUserByRoleIDAndCompanyID：" + "角色ID:"+ roleID +" 公司ID："+ companyID);
                return bll.GetUserByRoleIDAndCompanyID(roleID,companyID);
            }

        }
        

        /// <summary>
        /// 通过用户ID:（查找所在的公司、部门、岗位、角色，一个人可能同时在多个公司任职）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<FlowUserInfo> GetFlowUserByUserID(string userID)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                Tracer.Debug("流程调用了GetFlowUserByUserID：" + "用户ID:" + userID );
                return bll.GetFlowUserByUserID(userID);
            }
        }
        /// <summary>
        /// 通过用户ID,模块代码:（查询是否使用代理人，包括所在的公司、部门、岗位、角色）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="modelCode">模块代码</param>
        /// <returns></returns>
        public FlowUserInfo GetAgentUser(string userID, string modelCode)
        {
            return null;
        }

        /// <summary>
        /// 根据用户名修改用户密码 ljx-2012-8-24
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <param name="pwd">需要修改的密码</param>
        /// <returns></returns>
        [OperationContract]
        public string UpdatePwdByUserNameAndPwd(string UserName, string pwd)
        {
            
            using (SysUserBLL bll = new SysUserBLL())
            {
                string IsReturn = "";
                Tracer.Debug(UserName +"开始修改密码,用户密码:" + pwd);
                IsReturn = bll.UpdatePwdByUserNameAndPwd(UserName,pwd);
                return IsReturn;
            }
            
        }



        /// <summary>
        /// 通过菜单名获取菜单信息  add by ljx 2012-9-5
        /// </summary>
        /// <param name="EntityName">菜单名</param>
        /// <returns></returns>
        [OperationContract]
        public T_SYS_ENTITYMENU GetEntityMenuByName(string EntityName)
        {
            using (SysEntityMenuBLL bll = new SysEntityMenuBLL())
            {

                T_SYS_ENTITYMENU menu;
                string keyString = "GetSysMenuByEntityCode" + EntityName;
                if (WCFCache.Current[keyString] == null)
                {

                    menu = bll.GetSysMenuByEntityName(EntityName);
                    WCFCache.Current.Insert(keyString, menu, DateTime.Now.AddMinutes(15));

                }
                else
                {
                    menu = (T_SYS_ENTITYMENU)WCFCache.Current[keyString];
                }

                return menu;
            }

        }
        #endregion

        #region 权限更新时间
        /// <summary>
        /// 获取最近的权限更新的时间和现在权限项的数量,而数量减少则为删除某个角色或某项权限,
        /// 键值分别为T_SYS_USERROLE,T_SYS_ROLEENTITYMENU,T_SYS_ROLEMENUPERMISSION,T_SYS_ENTITYMENUCUSTOMPERM
        /// </summary>
        /// <param name="employeeid">用户id</param>
        /// <returns>V_PermissionUpdateState对象</returns>
        [OperationContract]
        public V_PermissionUpdateState GetLatestTimeOfPermission(string employeeid)
        {
            using (SysPermissionBLL bll = new SysPermissionBLL())
            {
                try
                {
                    V_PermissionUpdateState perUpdate = new V_PermissionUpdateState();
                    perUpdate.Timer = bll.GetLatestTimeOfPermission(employeeid);
                    perUpdate.Counter = bll.GetPermissionCounts(employeeid);
                    return perUpdate;
                }
                catch(Exception ex)
                {
                    Tracer.Debug("PermissionService-GetLatestTimeOfPermission获取时间出错:" + ex.ToString());
                    return null;
                }
            }
        }
        #endregion
    }
}
