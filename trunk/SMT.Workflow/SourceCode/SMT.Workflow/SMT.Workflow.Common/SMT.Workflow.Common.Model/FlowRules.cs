/*---------------------------------------------------------------------  
    * 版　权：Copyright ©  SmtOnline  2011    
    * 文件名：Rules.cs  
    * 创建者：LONGKC   
    * 创建日期：2011/11/18 11:43:57   
    * CLR版本： 4.0.30319.1  
    * 命名空间：SMT.Workflow.Common.Model 
    * 模块名称：
    * 描　　述：[规则表] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{
    /// <summary>
    /// [规则表](一个流程对应多条规则)
    /// </summary>
    [DataContract]
    public class FLOW_RULES
    {

        /// <summary>
        /// 规则ID
        /// </summary>
        [DataMember]
        public string RULESID { get; set; }

        /// <summary>
        /// 流程代码
        /// </summary>
        [DataMember]
        public string FLOWCODE { get; set; }

        /// <summary>
        /// 条件名称
        /// </summary>
        [DataMember]
        public string CONDITIONNAME { get; set; }

        /// <summary>
        /// 比较操作
        /// </summary>
        [DataMember]
        public string OPERATE { get; set; }

        /// <summary>
        /// 比较值
        /// </summary>
        [DataMember]
        public string COMPAREVALUE { get; set; }

    }
}