/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_WF_DOTASKRULE.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 10:31:39   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Common.Model.FlowEngine 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowEngine
{
    /// <summary>
    /// 我的单据
    /// </summary>
    [DataContract]
    public class T_WF_PERSONALRECORD
    {

        /// <summary>
        /// 个人单据ID
        /// </summary>
        [DataMember]
        public string PERSONALRECORDID { get; set; }

        /// <summary>
        /// 系统类型
        /// </summary>
        [DataMember]
        public string SYSTYPE { get; set; }

        /// <summary>
        /// 所属模块代码
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 单据ID
        /// </summary>
        [DataMember]
        public string MODELID { get; set; }

        /// <summary>
        /// 单据审核状态
        /// </summary>
        [DataMember]
        public decimal CHECKSTATE { get; set; }

        /// <summary>
        /// 所属员工ID
        /// </summary>
        [DataMember]
        public string OWNERID { get; set; }

        /// <summary>
        /// 所属岗位ID
        /// </summary>
        [DataMember]
        public string OWNERPOSTID { get; set; }

        /// <summary>
        /// 所属部门ID
        /// </summary>
        [DataMember]
        public string OWNERDEPARTMENTID { get; set; }

        /// <summary>
        /// 所属公司ID
        /// </summary>
        [DataMember]
        public string OWNERCOMPANYID { get; set; }

        /// <summary>
        /// 参数配置
        /// </summary>
        [DataMember]
        public string CONFIGINFO { get; set; }

        /// <summary>
        /// 单据简要描叙
        /// </summary>
        [DataMember]
        public string MODELDESCRIPTION { get; set; }

        /// <summary>
        /// 是否转发(0表示非转发，1表示转发)
        /// </summary>
        [DataMember]
        public decimal ISFORWARD { get; set; }

        /// <summary>
        /// 是否已查看(0表示未查看，1表示已查看)
        /// </summary>
        [DataMember]
        public decimal ISVIEW { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public DateTime CREATEDATE { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [DataMember]
        public DateTime UPDATEDATE { get; set; }

    }
}
