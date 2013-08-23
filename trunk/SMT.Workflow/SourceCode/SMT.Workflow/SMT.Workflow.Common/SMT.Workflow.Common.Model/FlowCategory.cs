 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：FlowCategory.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[流程分类表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [流程分类表]
     /// </summary>
    [DataContract]
    public class FLOW_FLOWCATEGORY
    {
    	 
        /// <summary>
        /// 流程分类ID
        /// </summary>
        [DataMember]
        public string  FLOWCATEGORYID { get; set; }
         
        /// <summary>
        /// 流程分类描述
        /// </summary>
        [DataMember]
        public string  FLOWCATEGORYDESC { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>
        [DataMember]
        public string COMPANYID { get; set; }
        
    }
}