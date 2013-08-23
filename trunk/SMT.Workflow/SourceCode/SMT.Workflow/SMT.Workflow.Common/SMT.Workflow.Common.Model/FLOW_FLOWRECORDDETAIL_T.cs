 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：T.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:57:49   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[流程审批明细表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
using System.Data.Objects.DataClasses;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [流程审批明细表]
     /// </summary>
    [DataContract]
    public class FLOW_FLOWRECORDDETAIL_T
    {
        [DataMember]
        public EntityCollection<FLOW_CONSULTATION_T> FLOW_CONSULTATION_T { get; set; }
        [DataMember]
        public FLOW_FLOWRECORDMASTER_T FLOW_FLOWRECORDMASTER_T { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  FLOWRECORDDETAILID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  FLOWRECORDMASTERID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  STATECODE { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  PARENTSTATEID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  CONTENT { get; set; }
         
        /// <summary>
        /// 同意：1，不同意:0 ,未处理:2，会签同意7，会签不同意8
        /// </summary>
        [DataMember]
        public string  CHECKSTATE { get; set; }
         
        /// <summary>
        /// 已审批：1，未审批:0
        /// </summary>
        [DataMember]
        public string  FLAG { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  CREATEUSERID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  CREATEUSERNAME { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  CREATECOMPANYID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  CREATEDEPARTMENTID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  CREATEPOSTID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime? CREATEDATE { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  EDITUSERID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  EDITUSERNAME { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  EDITCOMPANYID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  EDITDEPARTMENTID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  EDITPOSTID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime? EDITDATE { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  AGENTUSERID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string AGENTERNAME { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime? AGENTEDITDATE { get; set; }
        
    }
}