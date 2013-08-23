/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_WF_DOTASKRULEDETAIL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 10:32:32   
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
    /// [待办任务消息规则细表]
    /// </summary>
    [DataContract]
    public class T_WF_DOTASKRULEDETAIL
    {

        /// <summary>
        /// 应用URL
        /// </summary>
        [DataMember]
        public string APPLICATIONURL { get; set; }

        /// <summary>
        /// 接收用户
        /// </summary>
        [DataMember]
        public string RECEIVEUSER { get; set; }

        /// <summary>
        /// 接收用户名
        /// </summary>
        [DataMember]
        public string RECEIVEUSERNAME { get; set; }

        /// <summary>
        /// 所属公司ID
        /// </summary>
        [DataMember]
        public string OWNERCOMPANYID { get; set; }

        /// <summary>
        /// 所属部门ID
        /// </summary>
        [DataMember]
        public string OWNERDEPARTMENTID { get; set; }

        /// <summary>
        /// 所属岗位ID
        /// </summary>
        [DataMember]
        public string OWNERPOSTID { get; set; }

        /// <summary>
        /// 是否缺省消息
        /// </summary>
        [DataMember]
        public decimal ISDEFAULTMSG { get; set; }

        /// <summary>
        /// 处理功能语言
        /// </summary>
        [DataMember]
        public string PROCESSFUNCLANGUAGE { get; set; }

        /// <summary>
        /// 是否其它来源
        /// </summary>
        [DataMember]
        public string ISOTHERSOURCE { get; set; }

        /// <summary>
        /// 其它系统代码
        /// </summary>
        [DataMember]
        public string OTHERSYSTEMCODE { get; set; }

        /// <summary>
        /// 其它系统模块
        /// </summary>
        [DataMember]
        public string OTHERMODELCODE { get; set; }    

        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public string REMARK { get; set; }

        /// <summary>
        /// 待办规则明细ID
        /// </summary>
        [DataMember]
        public string DOTASKRULEDETAILID { get; set; }

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
        /// WCF的URL
        /// </summary>
        [DataMember]
        public string WCFURL { get; set; }

        /// <summary>
        /// 所调方法名称
        /// </summary>
        [DataMember]
        public string FUNCTIONNAME { get; set; }

        /// <summary>
        /// 方法参数
        /// </summary>
        [DataMember]
        public string FUNCTIONPARAMTER { get; set; }

        /// <summary>
        /// 参数分解符
        /// </summary>
        [DataMember]
        public string PAMETERSPLITCHAR { get; set; }

        /// <summary>
        /// WCF绑定的契约
        /// </summary>
        [DataMember]
        public string WCFBINDINGCONTRACT { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        [DataMember]
        public string MESSAGEBODY { get; set; }

        /// <summary>
        /// 可处理日期（剩余天数）
        /// </summary>
        [DataMember]
        public decimal LASTDAYS { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [DataMember]
        public string CREATEUSERNAME { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [DataMember]
        public string CREATEUSERID { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [DataMember]
        public DateTime CREATEDATETIME { get; set; }

    }
}
