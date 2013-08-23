 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：Flowtrigger.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[流程触发表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [流程触发表]
     /// </summary>
    [DataContract]
    public class T_FLOW_FLOWTRIGGER
    {
    	 
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
        /// 引擎代码
        /// </summary>
        [DataMember]
        public string  ENGINECODE { get; set; }
         
        /// <summary>
        /// 公司代码
        /// </summary>
        [DataMember]
        public string  COMPANYCODE { get; set; }
         
        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string  SYSTEMCODE { get; set; }
         
        /// <summary>
        /// 系统名称
        /// </summary>
        [DataMember]
        public string  SYSTEMNAME { get; set; }
         
        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string  MODELCODE { get; set; }
         
        /// <summary>
        /// 模块名称
        /// </summary>
        [DataMember]
        public string  MODELNAME { get; set; }
         
        /// <summary>
        /// 触发条件
        /// </summary>
        [DataMember]
        public string  TRIGGERCONDITION { get; set; }
        
    }
}