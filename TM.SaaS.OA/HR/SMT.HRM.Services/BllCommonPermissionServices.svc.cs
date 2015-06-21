using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using SMT.HRM.BLL.Permission;
using TM_SaaS_OA_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using SMT.HRM.DAL.Permission;
using SMT.HRM.CustomModel.Permission;
using SMT.Foundation.Log;
using SMT.HRM.BLL.Permission;

namespace SMT.SaaS.Permission.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BllCommonPermissionServices
    {
        
        
        /// <summary>
        /// 获取用户菜单的权限范围 通过BOOCOMMON获取 2010-9-14 
        /// </summary>
        /// <param name="menuCode">菜单编码</param>
        /// <param name="userID">用户ID</param>
        /// <param name="OwnerCompanyIDs">有权限的公司ID</param>
        /// <param name="OwnerDepartmentIDs">有权限的部门ID</param>
        /// <param name="OwnerPositionIDs">有权限的岗位ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<V_BllCommonUserPermission> GetUserMenuPermsByUserPermissionBllCommon(string menuCode, string userID, ref string OwnerCompanyIDs, ref string OwnerDepartmentIDs, ref string OwnerPositionIDs)
        {

            //SysUserBLL bll = new SysUserBLL();
            //IQueryable<V_Permission> plist = bll.GetUserMenuPerms(menuCode, userID);
            #region 使用缓存
            try
            {
                using (SysUserBLL bll = new SysUserBLL())
                {
                    List<V_BllCommonUserPermission> plist;
                        
                        IQueryable<V_BllCommonUserPermission> IQlist = bll.GetUserMenuPermsByUserPermisionBllCommon(menuCode, userID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);
                        //if(IQlist != null)
                        plist = IQlist !=null ? IQlist.ToList() : null;
                      

                    return plist == null ? null : plist;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("PermissionBllCommon出现错误：" + menuCode + System.DateTime.Now.ToString("d")+" "+ex.ToString());
                return null;
            }
            #endregion
        }

        
        
        //<summary>
        //获取用户菜单的权限范围 通过BOOCOMMON获取 2011-9-08 
        //</summary>
        //<param name="menuCode">菜单编码</param>
        //<param name="userID">用户ID</param>
        /// <param name="OwnerCompanyIDs">有权限的公司ID</param>
        /// <param name="OwnerDepartmentIDs">有权限的部门ID</param>
        /// <param name="OwnerPositionIDs">有权限的岗位ID</param>
        /// <param name="StrPermissionValue">权限值</param>
        /// <returns>返回对应的权限信息</returns>
        [OperationContract]
        public List<V_BllCommonUserPermission> GetUserMenuPermsByUserPermissionBllCommonAddPermissionValue(string menuCode, string userID, ref string OwnerCompanyIDs, ref string OwnerDepartmentIDs, ref string OwnerPositionIDs,string StrPermissionValue)
        {
            try
            {
                using (SysUserBLL bll = new SysUserBLL())
                {
                    List<V_BllCommonUserPermission> plist;
                    IQueryable<V_BllCommonUserPermission> IQlist = bll.GetUserMenuPermsByUserPermisionBllCommonAddPermissionValue(menuCode, userID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs, StrPermissionValue);
                    plist = IQlist != null ? IQlist.ToList() : null;
                    return plist == null ? null : plist;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("GetUserMenuPermsByUserPermissionBllCommonAddPermissionValue出现错误：" + menuCode + System.DateTime.Now.ToString("d") + " " + ex.ToString());
                return null;
            }
        }
        

        public string getCutomterPermission()
        {
           CustomerPermission obj=new CustomerPermission();
           return JsonHelper.GetJson<CustomerPermission>(obj);
        }

        /// <summary>
        /// 新框架中根据查询的条件、员工ID、menucode、系统名 查找该用户所拥有的权限  2011-4-20
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="queryParas"></param>
        /// <param name="employeeID"></param>
        /// <param name="menuCode"></param>
        /// <param name="SysCode"></param>
        [OperationContract]
        public List<object> SetOrganizationFilterToNewFrame(ref string filterString, object[] paras, string employeeID, string menuCode, string SysCode)
        {            
            using (SysUserBLL bll = new SysUserBLL())
            {
                List<object> queryParas = new List<object>();
                if (paras != null)
                {
                    queryParas.AddRange(paras);
                }
                bll.SetOrganizationFilterToNewFrame(ref filterString, ref queryParas, employeeID, menuCode, SysCode);
                return queryParas;
            }
                        
        }
       



        
    }
}
