/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_WF_ENGINEPREFIX.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 10:40:29   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Common.Model.FlowEngine 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowEngine
{
    /// <summary>
    /// [引擎前缀]
    /// </summary>
    [DataContract]
    public class T_WF_ENGINEPREFIX
    {

        /// <summary>
        /// 前缀代码
        /// </summary>
        [DataMember]
        public string PREFIXCODE { get; set; }

        /// <summary>
        /// 前缀名称
        /// </summary>
        [DataMember]
        public string PREFIXNAME { get; set; }

        /// <summary>
        /// DEFAULTBIT
        /// </summary>
        [DataMember]
        public string DEFAULTBIT { get; set; }

        /// <summary>
        /// 当前顺序
        /// </summary>
        [DataMember]
        public decimal CURRENTORDER { get; set; }

        /// <summary>
        /// 顺序长度
        /// </summary>
        [DataMember]
        public decimal ORDERLENGTH { get; set; }

    }
}
