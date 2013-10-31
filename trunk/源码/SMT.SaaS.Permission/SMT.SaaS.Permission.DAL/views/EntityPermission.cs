using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.Permission.DAL
{
    /// <summary>
    /// 主要用来处理新框架中获取操作的权限
    /// </summary>
    public class EntityPermission
    {
        /// <summary>
        /// 实体ID
        /// </summary>
        public string ENTITYID { get; set; }
        /// <summary>
        /// 所属公司ID
        /// </summary>
        public string OWNERCOMPANYID { get; set; }
        /// <summary>
        /// 所属部门ID
        /// </summary>
        public string OWNERDEPARTMENTID { get; set; }
        /// <summary>
        /// 所属岗位ID
        /// </summary>
        public string OWNERPOSTID { get; set; }
        /// <summary>
        /// 所属人ID
        /// </summary>
        public string OWNERID { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CREATEUSERID { get; set; }
        /// <summary>
        /// 菜单编号
        /// </summary>
        public string MENUCODE { get; set; }
        /// <summary>
        /// 操作值
        /// </summary>
        public string OPERATIONVALUE { get; set; }
        /// <summary>
        /// 操作人ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 系统编号
        /// </summary>
        public string SYSTEMCODE { get; set; }
        
    }
}
