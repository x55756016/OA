/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_FLOW_ENGINENOTES.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/20 14:22:44   
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
    [DataContract]
    public class T_FLOW_ENGINENOTES
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public decimal NOTESID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MESSAGEBODY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string APPLICATIONCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVEUSER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVEDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVETIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MESSAGESTATUS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATEUSERID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATEDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATETIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string APPLICATIONORDERCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string COMPANYCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MAILSTATUS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RTXSTATUS { get; set; }
    }
}
