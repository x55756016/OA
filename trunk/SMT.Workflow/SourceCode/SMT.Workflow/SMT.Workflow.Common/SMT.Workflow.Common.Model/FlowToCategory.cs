 /*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：FlowtoCategory.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/10/9 10:55:46   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.Model 
	 * 模块名称：
	 * 描　　述：[流程与分类关系表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model
{
     /// <summary>
     /// [流程与分类关系表]
     /// </summary>
    [DataContract]
    public class FLOW_FLOWTOCATEGORY
    {
    	 
        /// <summary>
        /// 流程分类ID
        /// </summary>
        [DataMember]
        public string  FLOWCATEGORYID { get; set; }
         
        /// <summary>
        /// 流程代码
        /// </summary>
        [DataMember]
        public string  FLOWCODE { get; set; }
    }
}