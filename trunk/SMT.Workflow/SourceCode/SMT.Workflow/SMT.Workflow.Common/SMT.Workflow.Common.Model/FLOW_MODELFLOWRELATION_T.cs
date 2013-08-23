 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：T.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[模块与流程定义关联表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [模块与流程定义关联表]
     /// </summary>
    [DataContract]
    public class FLOW_MODELFLOWRELATION_T
    {
        /// <summary>
        /// 模块对象
        /// </summary>
        [DataMember]
        public FLOW_MODELDEFINE_T FLOW_MODELDEFINE_T { get; set; }
         
        /// <summary>
        /// 流程对象
        /// </summary>
        [DataMember]
        public FLOW_FLOWDEFINE_T FLOW_FLOWDEFINE_T { get; set; }
        /// <summary>
        /// 关联ID
        /// </summary>
        [DataMember]
        public string  MODELFLOWRELATIONID { get; set; }
         
        /// <summary>
        /// 公司ID
        /// </summary>
        [DataMember]
        public string  COMPANYID { get; set; }
         
        /// <summary>
        /// 部门ID
        /// </summary>
        [DataMember]
        public string  DEPARTMENTID { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        [DataMember]
        public string COMPANYNAME { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        [DataMember]
        public string DEPARTMENTNAME { get; set; }

        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string SYSTEMCODE { get; set; }
        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string  MODELCODE { get; set; }
         
        /// <summary>
        /// 流程代码
        /// </summary>
        [DataMember]
        public string  FLOWCODE { get; set; }
         
        /// <summary>
        /// 1这可用，0为不可用
        /// </summary>
        [DataMember]
        public string  FLAG { get; set; }
         
        /// <summary>
        /// 0:审批流程，1：任务流程
        /// </summary>
        [DataMember]
        public string  FLOWTYPE { get; set; }
         
        /// <summary>
        /// 创建人ID
        /// </summary>
        [DataMember]
        public string  CREATEUSERID { get; set; }
         
        /// <summary>
        /// 创建人名
        /// </summary>
        [DataMember]
        public string  CREATEUSERNAME { get; set; }
         
        /// <summary>
        /// 创建公司ID
        /// </summary>
        [DataMember]
        public string  CREATECOMPANYID { get; set; }
         
        /// <summary>
        /// 创建部门ID
        /// </summary>
        [DataMember]
        public string  CREATEDEPARTMENTID { get; set; }
         
        /// <summary>
        /// 创建岗位ID
        /// </summary>
        [DataMember]
        public string  CREATEPOSTID { get; set; }
         
        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public DateTime? CREATEDATE { get; set; }
         
        /// <summary>
        /// 修改人ID
        /// </summary>
        [DataMember]
        public string  EDITUSERID { get; set; }
         
        /// <summary>
        /// 修改人用户名
        /// </summary>
        [DataMember]
        public string  EDITUSERNAME { get; set; }
         
        /// <summary>
        /// 修改时间
        /// </summary>
        [DataMember]
        public DateTime? EDITDATE { get; set; }
       /// <summary>
        /// 流程名称
        /// </summary>
        [DataMember]
        public string DESCRIPTION { get; set; }
        
        
    }
}