 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：TimingevenTTrigger.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[定时事件触发表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [定时事件触发表]
     /// </summary>
    [DataContract]
    public class T_FLOW_TIMINGEVENTTRIGGER
    {
    	 
        /// <summary>
        /// 处理ID
        /// </summary>
        [DataMember]
        public decimal PROCESSID { get; set; }
         
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
        /// 应用顺序代码
        /// </summary>
        [DataMember]
        public string  APPLICATIONORDERCODE { get; set; }
         
        /// <summary>
        /// 处理开始日期
        /// </summary>
        [DataMember]
        public string  PROCESSSTARTDATE { get; set; }
         
        /// <summary>
        /// 处理开始时间
        /// </summary>
        [DataMember]
        public string  PROCESSSTARTTIME { get; set; }
         
        /// <summary>
        /// 处理周期
        /// </summary>
        [DataMember]
        public string  PROCESSCYCLE { get; set; }
         
        /// <summary>
        /// 接收用户
        /// </summary>
        [DataMember]
        public string  RECEIVEUSER { get; set; }
         
        /// <summary>
        /// 接收角色
        /// </summary>
        [DataMember]
        public string  RECEIVEROLE { get; set; }
         
        /// <summary>
        /// 消息体
        /// </summary>
        [DataMember]
        public string  MESSAGEBODY { get; set; }
         
        /// <summary>
        /// 消息链接URL
        /// </summary>
        [DataMember]
        public string  MSGLINKURL { get; set; }
         
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
        /// 参数分割符
        /// </summary>
        [DataMember]
        public string  PAMETERSPLITCHAR { get; set; }
         
        /// <summary>
        /// WCF绑定契约
        /// </summary>
        [DataMember]
        public string  WCFBINDINGCONTRACT { get; set; }
         
        /// <summary>
        /// 数据状态
        /// </summary>
        [DataMember]
        public string  DATASTATUS { get; set; }
         
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
        /// 更新日期
        /// </summary>
        [DataMember]
        public string  UPDATEDATE { get; set; }
         
        /// <summary>
        /// 更新时间
        /// </summary>
        [DataMember]
        public string  UPDATETIME { get; set; }
         
        /// <summary>
        /// 功能备注
        /// </summary>
        [DataMember]
        public string  FUNCTIONMARK { get; set; }
         
        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string  MODELCODE { get; set; }
         
        /// <summary>
        /// 触发类型
        /// </summary>
        [DataMember]
        public string  TRIGGERTYPE { get; set; }
         
        /// <summary>
        /// 契约类型
        /// </summary>
        [DataMember]
        public string  CONTRACTTYPE { get; set; }
        
    }
}