/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_WF_MESSAGEDEFINE.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/6 14:28:30   
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
    /// 默认消息配置表（如公文发送，考勤异常批量发送同样的消息标题)
    /// </summary>
     [DataContract]
    public class T_WF_MESSAGEBODYDEFINE
    {

      

        /// <summary>
        /// 消息定义ID
        /// </summary>
        [DataMember]
         public string DEFINEID { get; set; }

        /// <summary>
        /// 所属公司ID
        /// </summary>
        [DataMember]
        public string COMPANYID { get; set; }

        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string SYSTEMCODE { get; set; }

        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        [DataMember]
        public string MESSAGEBODY { get; set; }

        /// <summary>
        /// 消息URL
        /// </summary>
        [DataMember]
        public string MESSAGEURL { get; set; }

        /// <summary>
        /// 消息类型（默认、0，撤销、1）
        /// </summary>
        [DataMember]
        public decimal MESSAGETYPE { get; set; }

        /// <summary>
        /// 接受类型 0、按照角色 1、按照人员
        /// </summary>
        [DataMember]
        public decimal RECEIVETYPE { get; set; }

        /// <summary>
        /// 创建日期时间
        /// </summary>
        [DataMember]
        public DateTime? CREATEDATE { get; set; }
      

        /// <summary>
        /// 创建用户名
        /// </summary>
        [DataMember]
        public string CREATEUSERNAME { get; set; }

        /// <summary>
        /// 创建用户ID
        /// </summary>
        [DataMember]
        public string CREATEUSERID { get; set; }

        /// <summary>
        /// 岗位ID
        /// </summary>
        [DataMember]
        public string RECEIVEPOSTID { get; set; }

        /// <summary>
        /// 岗位名称
        /// </summary>
        [DataMember]
        public string RECEIVEPOSTNAME { get; set; }

        /// <summary>
        /// 接收用户ID
        /// </summary>
        [DataMember]
        public string RECEIVERUSERID { get; set; }

        /// <summary>
        /// 接收用户名称
        /// </summary>
        [DataMember]
        public string RECEIVERUSERNAME { get; set; }
    }
}
