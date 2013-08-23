 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：T.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:58:33   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[流程模型定义表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [流程模型定义表]
     /// </summary>
    [DataContract]
    public class FLOW_FLOWDEFINE_T
    {
    	 
        /// <summary>
        /// 流程定义ID
        /// </summary>
        [DataMember]
        public string  FLOWDEFINEID { get; set; }
         
        /// <summary>
        /// 流程代码
        /// </summary>
        [DataMember]
        public string  FLOWCODE { get; set; }
         
        /// <summary>
        /// 名称描述
        /// </summary>
        [DataMember]
        public string  DESCRIPTION { get; set; }
         
        /// <summary>
        /// 模型文件
        /// </summary>
        [DataMember]
        public string  XOML { get; set; }
         
        /// <summary>
        /// 模型规则
        /// </summary>
        [DataMember]
        public string  RULES { get; set; }
         
        /// <summary>
        /// 模型布局
        /// </summary>
        [DataMember]
        public string  LAYOUT { get; set; }

        /// <summary>
        /// 流程定义文件
        /// </summary>
        [DataMember]
        public string WFLAYOUT { get; set; }
         
        /// <summary>
        /// 流程类型 -- 0:审批流程, 1:任务流程
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
        /// 业务系统:OA,HR,TM等
        /// </summary>
        [DataMember]
        public string SYSTEMCODE { get; set; }

        /// <summary>
        /// 业务对象：各种申请报销单
        /// </summary>
        [DataMember]
        public string BUSINESSOBJECT { get; set; }
    }
}