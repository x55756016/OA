/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_WF_DOTASK.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/6 14:26:23   
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
    /// [待办任务列表]
    /// </summary>
    [DataContract]
    public class T_WF_DOTASK
    {

        /// <summary>
        /// 创建日期
        /// </summary>
        [DataMember]
        public DateTime CREATEDATETIME { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public string REMARK { get; set; }

        /// <summary>
        /// 待办任务ID
        /// </summary>
        [DataMember]
        public string DOTASKID { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        [DataMember]
        public string COMPANYID { get; set; }

        /// <summary>
        /// 单据ID
        /// </summary>
        [DataMember]
        public string ORDERID { get; set; }

        /// <summary>
        /// 单据所属人ID
        /// </summary>
        [DataMember]
        public string ORDERUSERID { get; set; }

        /// <summary>
        /// 单据所属人名称
        /// </summary>
        [DataMember]
        public string ORDERUSERNAME { get; set; }

        /// <summary>
        /// 单据状态
        /// </summary>
        [DataMember]
        public decimal ORDERSTATUS { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        [DataMember]
        public string MESSAGEBODY { get; set; }

        /// <summary>
        /// 应用URL
        /// </summary>
        [DataMember]
        public string APPLICATIONURL { get; set; }

        /// <summary>
        /// 接收用户ID
        /// </summary>
        [DataMember]
        public string RECEIVEUSERID { get; set; }

        /// <summary>
        /// 处理剩余时间（主要针对KPI考核）
        /// </summary>
        [DataMember]
        public DateTime? BEFOREPROCESSDATE { get; set; }

        /// <summary>
        /// 待办任务类型(0、待办任务、1、流程咨询、3 )
        /// </summary>
        [DataMember]
        public decimal DOTASKTYPE { get; set; }

        /// <summary>
        /// 待办关闭时间
        /// </summary>
        [DataMember]
        public DateTime? CLOSEDDATE { get; set; }

        /// <summary>
        /// 引擎代码
        /// </summary>
        [DataMember]
        public string ENGINECODE { get; set; }

        /// <summary>
        /// 代办任务状态(0、未处理 1、已处理 、2、任务撤销 )
        /// </summary>
        [DataMember]
        public decimal DOTASKSTATUS { get; set; }

        /// <summary>
        /// 邮件状态(0、未发送 1、已发送、2、未知 )
        /// </summary>
        [DataMember]
        public decimal MAILSTATUS { get; set; }

        /// <summary>
        /// RTX状态(0、未发送 1、已发送、2、未知 )
        /// </summary>
        [DataMember]
        public decimal RTXSTATUS { get; set; }

        /// <summary>
        /// 是否已提醒(0、未提醒 1、已提醒、2、未知 )
        /// </summary>
        [DataMember]
        public decimal ISALARM { get; set; }

        /// <summary>
        /// 应用字段值
        /// </summary>
        [DataMember]
        public string APPFIELDVALUE { get; set; }

        /// <summary>
        /// 流程XML
        /// </summary>
        [DataMember]
        public string FLOWXML { get; set; }

        /// <summary>
        /// 应用XML
        /// </summary>
        [DataMember]
        public string APPXML { get; set; }

        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string SYSTEMCODE { get; set; }

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

    }
}
