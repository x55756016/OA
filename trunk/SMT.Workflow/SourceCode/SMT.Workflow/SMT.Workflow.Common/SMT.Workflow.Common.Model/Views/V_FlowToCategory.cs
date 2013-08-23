/*---------------------------------------------------------------------  
    * 版　权：Copyright ©  SmtOnline  2011    
    * 文件名：V_FlowToCategory.cs  
    * 创建者：zhangyh   
    * 创建日期：2011/10/11 10:55:46   
    * CLR版本： 4.0.30319.1  
    * 命名空间：SMT.Workflow.Common.Model 
    * 模块名称：
    * 描　　述：[流程与分类视图] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.Views
{    
    /// <summary>
    /// [流程与分类视图]
    /// </summary>
    [DataContract]
    public class V_FLOWTOCATEGORY
    {
        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }
        /// <summary>
        /// 模块名称
        /// </summary>
        [DataMember]
        public string MODENAME { get; set; }

        /// <summary>
        /// 流程分类ID
        /// </summary>
        [DataMember]
        public string FLOWCATEGORYID { get; set; }

        /// <summary>
        /// 流程分类描述
        /// </summary>
        [DataMember]
        public string FLOWCATEGORYDESC { get; set; }

        /// <summary>
        /// 流程代码
        /// </summary>
        [DataMember]
        public string FLOWCODE { get; set; }

        /// <summary>
        /// 名称描述
        /// </summary>
        [DataMember]
        public string DESCRIPTION { get; set; }

        /// <summary>
        /// 创建人名
        /// </summary>
        [DataMember]
        public string CREATEUSERNAME { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public string CREATEDATE { get; set; }
    }
}
