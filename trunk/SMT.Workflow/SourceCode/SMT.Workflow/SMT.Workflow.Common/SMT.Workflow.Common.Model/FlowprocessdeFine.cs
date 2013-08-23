 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：FlowprocessdeFine.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[流程处理定义表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [流程处理定义表]
     /// </summary>
    [DataContract]
    public class T_FLOW_FLOWPROCESSDEFINE
    {
    	 
        /// <summary>
        /// 处理ID
        /// </summary>
        [DataMember]
        public string  PROCESSID { get; set; }
         
        /// <summary>
        /// 引擎编码
        /// </summary>
        [DataMember]
        public string  ENGINECODE { get; set; }
         
        /// <summary>
        /// 公司编码
        /// </summary>
        [DataMember]
        public string  COMPANYCODE { get; set; }
         
        /// <summary>
        /// 系统编码
        /// </summary>
        [DataMember]
        public string  SYSTEMCODE { get; set; }
         
        /// <summary>
        /// 系统名称
        /// </summary>
        [DataMember]
        public string  SYSTEMNAME { get; set; }
         
        /// <summary>
        /// 模块编码
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
        public string TRIGGERCONDITION { get; set; }
         
        /// <summary>
        /// 处理WCF的URL
        /// </summary>
        [DataMember]
        public string  PROCESSWCFURL { get; set; }
         
        /// <summary>
        /// 处理功能名
        /// </summary>
        [DataMember]
        public string  PROCESSFUNCNAME { get; set; }
         
        /// <summary>
        /// 处理功能参数
        /// </summary>
        [DataMember]
        public string  PROCESSFUNCPAMETER { get; set; }
         
        /// <summary>
        /// 参数分解符
        /// </summary>
        [DataMember]
        public string  PAMETERSPLITCHAR { get; set; }
         
        /// <summary>
        /// WCF绑定的契约
        /// </summary>
        [DataMember]
        public string  WCFBINDINGCONTRACT { get; set; }
         
        /// <summary>
        /// 消息体
        /// </summary>
        [DataMember]
        public string  MESSAGEBODY { get; set; }
         
        /// <summary>
        /// 可处理日期
        /// </summary>
        [DataMember]
        public string  AVAILABILITYPROCESSDATES { get; set; }
         
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
        /// 应用URL
        /// </summary>
        [DataMember]
        public string  APPLICATIONURL { get; set; }
         
        /// <summary>
        /// 接收用户
        /// </summary>
        [DataMember]
        public string  RECEIVEUSER { get; set; }
         
        /// <summary>
        /// 接收用户名
        /// </summary>
        [DataMember]
        public string  RECEIVEUSERNAME { get; set; }
         
        /// <summary>
        /// 所属公司ID
        /// </summary>
        [DataMember]
        public string  OWNERCOMPANYID { get; set; }
         
        /// <summary>
        /// 所属部门ID
        /// </summary>
        [DataMember]
        public string  OWNERDEPARTMENTID { get; set; }
         
        /// <summary>
        /// 所属岗位ID
        /// </summary>
        [DataMember]
        public string  OWNERPOSTID { get; set; }
         
        /// <summary>
        /// 缺省消息
        /// </summary>
        [DataMember]
        public string  DEFAULTMSG { get; set; }
         
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
        /// 处理功能语言
        /// </summary>
        [DataMember]
        public string  PROCESSFUNCLANGUAGE { get; set; }
         
        /// <summary>
        /// 是否其它来源
        /// </summary>
        [DataMember]
        public string  ISOTHERSOURCE { get; set; }
         
        /// <summary>
        /// 其它系统代码
        /// </summary>
        [DataMember]
        public string  OTHERSYSTEMCODE { get; set; }
         
        /// <summary>
        /// 其它系统模块
        /// </summary>
        [DataMember]
        public string  OTHERMODELCODE { get; set; }
        
    }
}