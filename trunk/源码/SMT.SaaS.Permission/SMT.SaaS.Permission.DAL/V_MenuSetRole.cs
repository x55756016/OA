///本想和类V_UserMenuPermission一起使用，考虑到这个类是
///专门给平台调用 ，也许以后会增加字段这样会影响到角色设置
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.DAL
{
    public class V_MenuSetRole
    {
        /// <summary>
        /// 菜单ID
        /// </summary>
        public string ENTITYMENUID { get; set; }
        /// <summary>
        /// 菜单名
        /// </summary>
        public string MENUNAME { get; set; }
        /// <summary>
        /// 父菜单ID
        /// </summary>
        public string PARENTMENUID { get; set; }
        
        /// <summary>
        /// 菜单序号
        /// </summary>
        public decimal ORDERNUMBER { get; set; }
    }
}
