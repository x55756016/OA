 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：Msgbodydefine.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[消息定义] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [消息定义]
     /// </summary>
    [DataContract]
    public class T_FLOW_MSGBODYDEFINE
    {
    	 
        /// <summary>
        /// 定义ID
        /// </summary>
        [DataMember]
        public string  DEFINEID { get; set; }
         
        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string  SYSTEMCODE { get; set; }
         
        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string  MODELCODE { get; set; }
         
        /// <summary>
        /// 消息体
        /// </summary>
        [DataMember]
        public string  MSGBODY { get; set; }
         
        /// <summary>
        /// 消息URL
        /// </summary>
        [DataMember]
        public string  MSGURL { get; set; }
         
        /// <summary>
        /// 创建日期
        /// </summary>
        [DataMember]
        public string  CREATEDATE { get; set; }
         
        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public string  CREATETIME { get; set; }
         
        /// <summary>
        /// 公司代码
        /// </summary>
        [DataMember]
        public string  COMPANYCODE { get; set; }
         
        /// <summary>
        /// 创建用户名
        /// </summary>
        [DataMember]
        public string  CREATEUSERNAME { get; set; }
         
        /// <summary>
        /// 创建用户ID
        /// </summary>
        [DataMember]
        public string  CREATEUSERID { get; set; }
         
        /// <summary>
        /// 消息类型（默认，撤销）
        /// </summary>
        [DataMember]
        public string  MSGTYPE { get; set; }
        
    }
}