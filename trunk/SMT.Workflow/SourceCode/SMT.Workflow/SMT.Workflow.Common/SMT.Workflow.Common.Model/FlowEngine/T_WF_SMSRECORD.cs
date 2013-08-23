/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_WF_SMSRECORD.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 10:37:48   
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
    /// [短信发送记录表]
    /// </summary>
    [DataContract]
    public class T_WF_SMSRECORD
    {

        /// <summary>
        /// 短信记录ID
        /// </summary>
        [DataMember]
        public string SMSRECORD { get; set; }

        /// <summary>
        /// 记录触发批次
        /// </summary>
        [DataMember]
        public string BATCHNUMBER { get; set; }

        /// <summary>
        /// (发送方)公司ID
        /// </summary>
        [DataMember]
        public string COMPANYID { get; set; }

        /// <summary>
        /// 发送状态
        /// </summary>
        [DataMember]
        public decimal SENDSTATUS { get; set; }

        /// <summary>
        /// 短信账号
        /// </summary>
        [DataMember]
        public string ACCOUNT { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [DataMember]
        public string MOBILE { get; set; }

        /// <summary>
        /// 发送内容
        /// </summary>
        [DataMember]
        public string SENDMESSAGE { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        [DataMember]
        public DateTime SENDTIME { get; set; }

        /// <summary>
        /// 短信类型
        /// </summary>
        [DataMember]
        public decimal SMSTYPE { get; set; }

        /// <summary>
        /// 所属人名称
        /// </summary>
        [DataMember]
        public string OWNERNAME { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public string REMARK { get; set; }

    }
}
