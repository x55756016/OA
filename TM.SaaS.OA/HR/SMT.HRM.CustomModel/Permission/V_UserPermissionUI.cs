using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TM_SaaS_OA_EFModel;
using SMT.HRM.CustomModel.Permission;

namespace SMT.HRM.CustomModel.Permission
{
    [Serializable]
    /// <summary>
    /// 用来处理各个项目的UI中的权限的获取
    /// </summary>
    public class V_UserPermissionUI
    {
        /// <summary>
        /// 权限值
        /// </summary>
        public string DataRange { get; set; }
        /// <summary>
        /// 权限数据范围
        /// </summary>
        public string PermissionValue { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public string EntityMenuID { get; set; }
        /// <summary>
        /// 菜单编码
        /// </summary>
        public string MenuCode { get; set; }
        /// <summary>
        /// 第1个对象类型，第2个对象ID  例如：公司：公司ID
        /// </summary>
        public CustomerPermission CustomerPermission { get; set; }
    }
}
