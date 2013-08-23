 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：T.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:57:15   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[流程审批实例表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
using System.Data.Objects.DataClasses;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [流程审批实例表]
     /// </summary>
    [DataContract]
    public class FLOW_FLOWRECORDMASTER_T
    {
        [DataMember]
        public EntityCollection<FLOW_FLOWRECORDDETAIL_T> FLOW_FLOWRECORDDETAIL_T { get; set; }
    	 
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  FLOWRECORDMASTERID { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  INSTANCEID { get; set; }
         
        /// <summary>
        /// 0:固定流程，1：自选流程
        /// </summary>
        [DataMember]
        public string  FLOWSELECTTYPE { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  MODELCODE { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  FLOWCODE { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  FORMID { get; set; }
         
        /// <summary>
        /// 0:审批流程，1：任务流程
        /// </summary>
        [DataMember]
        public string  FLOWTYPE { get; set; }
         
        /// <summary>
        /// 1:审批中，2：审批通过，3审批不通过，5撤销(为与字典保持一致)
        /// </summary>
        [DataMember]
        public string  CHECKSTATE { get; set; }
         
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
        public DateTime? EDITDATE { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  ACTIVEROLE { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  BUSINESSOBJECT { get; set; }
         
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string  KPITIMEXML { get; set; }
        
    }
}