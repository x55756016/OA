using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;



namespace SMT.SaaS.Permission.DAL.views
{
    public class V_EntityMenuCustomperm
    {
        /// <summary>
        /// 自定义权限ID
        /// </summary>
        public string ENTITYCUSTOMERPERMISSIONID { get; set; }
        /// <summary>
        /// 权限ID
        /// </summary>
        public string PERMISSIONID { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public string ENTITYMENUID { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>
        public string COMPANYID { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string COMPANYNAME { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        public string DEPARTMENTID { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DEPARTMENTNAME { get; set; }
        /// <summary>
        /// 岗位ID
        /// </summary>
        public string POSTID { get; set; }
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string POSTNAME { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public string ROLEID { get; set; }
        
    }
}
