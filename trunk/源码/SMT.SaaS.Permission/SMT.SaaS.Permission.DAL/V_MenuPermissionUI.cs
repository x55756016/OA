//给设置权限时使用的类
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;

namespace SMT.SaaS.Permission.DAL
{
    public class V_MenuPermissionUI
    {
        //菜单ID
        public string ENTITYMENUID { get; set; }
        //菜单名称
        public string MENUNAME { get; set; }        
        //菜单父ID
        public string EntityMenuFatherID { get; set; }        
        //菜单顺序
        public decimal ORDERNUMBER { get; set; }
        //系统类型
        public string SYSTEMTYPE { get; set; }
        
    }
}
