/*---------------------------------------------------------------------  
    * 版　权：Copyright ©  SmtOnline  2011    
    * 文件名：V_FlowToCategory.cs  
    * 创建者：zhangyh   
    * 创建日期：2011/10/27 10:55:46   
    * CLR版本： 4.0.30319.1  
    * 命名空间：SMT.Workflow.Common.Model 
    * 模块名称：
    * 描　　述：[流程定义视图] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SMT.Workflow.Common.Model.Views
{
    /// <summary>
    /// 流程定义视图
    /// </summary>
    [DataContract]
    public class V_FLOWDEFINITION
    {        
        /// <summary>
        /// 流程定义
        /// </summary>
        [DataMember]
        public FLOW_FLOWDEFINE_T FlowDefinition { get; set; }

        /// <summary>
        /// 模块与流程关联
        /// </summary>
        [DataMember]
        public FLOW_MODELFLOWRELATION_T ModelRelation { get; set; }
    

    }
}
