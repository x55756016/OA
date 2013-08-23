 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：Msgdefine.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[消息定义表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [消息定义表]
     /// </summary>
    [DataContract]
    public class T_FLOW_MSGDEFINE
    {
    	 
        /// <summary>
        /// 消息ID
        /// </summary>
        [DataMember]
        public decimal MSGID { get; set; }
         
        /// <summary>
        /// 消息Key
        /// </summary>
        [DataMember]
        public string  MSGKEY { get; set; }
         
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
        /// 消息内容
        /// </summary>
        [DataMember]
        public string  MSGCONTENT { get; set; }
         
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
        
    }
}