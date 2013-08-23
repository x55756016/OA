 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：Triggerlog.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[触发日志表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [触发日志表]
     /// </summary>
    [DataContract]
    public class TRIGGERLOG
    {
    	 
        /// <summary>
        /// 触发ID
        /// </summary>
        [DataMember]
        public string  TRIIGERID { get; set; }
         
        /// <summary>
        /// 触发用户
        /// </summary>
        [DataMember]
        public string  TRIGERUSER { get; set; }
         
        /// <summary>
        /// 触发日期
        /// </summary>
        [DataMember]
        public DateTime TRIGGERDATA { get; set; }
         
        /// <summary>
        /// 触发状态
        /// </summary>
        [DataMember]
        public string  TRIGGERSTATUS { get; set; }
         
        /// <summary>
        /// 自动刷新
        /// </summary>
        [DataMember]
        public string  AUTOREFLESH { get; set; }
         
        /// <summary>
        /// 列表数
        /// </summary>
        [DataMember]
        public string  LISTCOUNT { get; set; }
         
        /// <summary>
        /// TOP
        /// </summary>
        [DataMember]
        public string  TOP { get; set; }
        
    }
}