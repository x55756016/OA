using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.SaaS.LocalData
{
    /// <summary>
    /// 本地菜单实体
    /// </summary>
    public class T_SYS_ENTITYMENU
    {
        /// <summary>
        /// 子系统名称
        /// </summary>
        public string CHILDSYSTEMNAME;
        /// <summary>
        /// 实体编码
        /// </summary>
        public string ENTITYCODE;
        /// <summary>
        /// 实体菜单ID
        /// </summary>
        public string ENTITYMENUID;
        /// <summary>
        /// 实体名称
        /// </summary>
        public string ENTITYNAME;
        /// <summary>
        /// 是否是系统菜单
        /// </summary>
        public string HASSYSTEMMENU;
        /// <summary>
        /// 帮助文件地址
        /// </summary>
        public string HELPFILEPATH;
        /// <summary>
        /// 帮助标题
        /// </summary>
        public string HELPTITLE;
        /// <summary>
        /// 是否受权限控制
        /// </summary>
        public string ISAUTHORITY;
        /// <summary>
        /// 菜单编号
        /// </summary>
        public string MENUCODE;
        /// <summary>
        /// 菜单图标地址
        /// </summary>
        public string MENUICONPATH;
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MENUNAME;
        /// <summary>
        /// 菜单序号
        /// </summary>
        public decimal ORDERNUMBER;
        /// <summary>
        /// 系统类型
        /// </summary>
        public string SYSTEMTYPE;
        /// <summary>
        /// 父级菜单ID
        /// </summary>
        public string SUPERIORID;
        /// <summary>
        /// 菜单链接地址
        /// </summary>
        public string URLADDRESS;
    }
}
