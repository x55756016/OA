using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.SaaS.Permission.BLL;
using System.Collections.Generic;
using SMT_System_EFModel;
using SMT.SaaS.Permission.DAL;

namespace SMT.SaaS.Permission.Services
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class CurrentContextPermision
    {
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
                        PermissionService p = new PermissionService();
                        UserFb = p.getFbAdmin(userID);
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

                return plist != null ? plist : null;
            }
        }
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
    }
}
