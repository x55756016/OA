/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2012     
	 * 文件名：FLOW_INSTANCE_STATE.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2012/9/10 8:49:12   
	 * NET版本： 4.0.30319.225 
	 * 命名空间：SMT.Workflow.Monitoring.Model 
	 * 模块名称：
	 * 描　　述： 
	 * 修改人员：
	 * 修改日期：
	 * 修改内容：
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model
{
    /// <summary>
    /// [流程审核过程中的持久化实例]
    /// </summary>
    [DataContract]
    public class FLOW_INSTANCE_STATE
    {

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string INSTANCE_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public byte[] STATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public decimal STATUS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public decimal UNLOCKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public decimal BLOCKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string INFO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime MODIFIED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OWNER_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime OWNED_UNTIL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime NEXT_TIMER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FORMID { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [DataMember]
        public string CREATEID { get; set; }

        /// <summary>
        /// 创建人姓名
        /// </summary>
        [DataMember]
        public string CREATENAME { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public DateTime CREATEDATE { get; set; }

        /// <summary>
        /// 下一个审核人ID
        /// </summary>
        [DataMember]
        public string EDITID { get; set; }

        /// <summary>
        /// 下一个审核人姓名
        /// </summary>
        [DataMember]
        public string EDITNAME { get; set; }

    }
}
