/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_WF_DOTASKMESSAGE.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 10:29:30   
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
    /// [待办消息列表]
    /// </summary>
    [DataContract]
    public class T_WF_DOTASKMESSAGE
    {

        /// <summary>
        /// 待办消息ID
        /// </summary>
        [DataMember]
        public string DOTASKMESSAGEID { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        [DataMember]
        public string MESSAGEBODY { get; set; }

        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string SYSTEMCODE { get; set; }

        /// <summary>
        /// 接收用户
        /// </summary>
        [DataMember]
        public string RECEIVEUSERID { get; set; }

        /// <summary>
        /// 单据ID
        /// </summary>
        [DataMember]
        public string ORDERID { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        [DataMember]
        public string COMPANYID { get; set; }

        /// <summary>
        /// 消息状态(0、未处理 1、已处理 、2、消息撤销 )
        /// </summary>
        [DataMember]
        public decimal MESSAGESTATUS { get; set; }

        /// <summary>
        /// 邮件状态(0、未发送 1、已发送、2、未知 )
        /// </summary>
        [DataMember]
        public decimal MAILSTATUS { get; set; }

        /// <summary>
        /// TRX状态(0、未发送 1、已发送、2、未知 )
        /// </summary>
        [DataMember]
        public decimal RTXSTATUS { get; set; }

        /// <summary>
        /// 创建日期时间
        /// </summary>
        [DataMember]
        public DateTime CREATEDATETIME { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public string REMARK { get; set; }

    }
}
