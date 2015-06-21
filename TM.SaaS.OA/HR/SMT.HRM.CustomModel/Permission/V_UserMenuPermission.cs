///用户所拥有的菜单 2011-5-19
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TM_SaaS_OA_EFModel;

namespace SMT.HRM.CustomModel.Permission
{
    [Serializable]
    [DataContract]
    public class V_UserMenuPermission
    {
        [DataMember]
        //菜单ID
        public string ENTITYMENUID { get; set; }
        [DataMember]
        //菜单名称
        public string MENUNAME { get; set; }
        [DataMember]
        //菜单编号
        public string MENUCODE { get; set; }
        [DataMember]
        //菜单父ID
        public string EntityMenuFatherID { get; set; }
        [DataMember]
        //菜单图标
        public string MENUICONPATH { get; set; }
        [DataMember]
        //菜单地址
        public string URLADDRESS { get; set; }
        [DataMember]
        //菜单顺序
        public decimal ORDERNUMBER { get; set; }
        [DataMember]
        //系统类型
        public string SYSTEMTYPE { get; set; }
        [DataMember]
        //子系统名称
        public string CHILDSYSTEMNAME { get; set; }
        [DataMember]
        /// <summary>
        /// 是否菜单
        /// </summary>
        public string HASSYSTEMMENU { get; set; }
        [DataMember]
        /// <summary>
        /// 是否权限项
        /// </summary>
        public string ISAUTHORITY { get; set; }
    }
}
