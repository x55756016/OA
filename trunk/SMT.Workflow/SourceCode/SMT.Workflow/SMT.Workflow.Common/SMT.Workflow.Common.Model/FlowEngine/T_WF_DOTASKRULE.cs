/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_WF_DOTASKRULE.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 10:31:39   
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
    /// [待办任务消息规则主表]
    /// </summary>
    [DataContract]
    public class T_WF_DOTASKRULE
    {

        /// <summary>
        /// 待办规则主表ID
        /// </summary>
        [DataMember]
        public string DOTASKRULEID { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        [DataMember]
        public string COMPANYID { get; set; }

        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string SYSTEMCODE { get; set; }
        /// <summary>
        /// 系统名称
        /// </summary>
        [DataMember]
        public string SYSTEMNAME { get; set; }
        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        [DataMember]
        public string MODELNAME { get; set; }

        /// <summary>
        /// 触发条件的单据状态
        /// </summary>
        [DataMember]
        public decimal? TRIGGERORDERSTATUS { get; set; }

        /// <summary>
        /// 创建日期时间
        /// </summary>
        [DataMember]
        public DateTime CREATEDATETIME { get; set; }

        /// <summary>
        /// 明细表
        /// </summary>
        [DataMember]
        public T_WF_DOTASKRULEDETAIL DOTASKRULEDETAIL { get; set; }

        

    }
}
