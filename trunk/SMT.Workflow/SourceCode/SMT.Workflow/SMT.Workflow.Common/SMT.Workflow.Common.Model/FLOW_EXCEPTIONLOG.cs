/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2012     
	 * 文件名：FLOW_EXCEPTIONLOG.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2012/9/7 13:50:59   
	 * NET版本： 4.0.30319.225 
	 * 命名空间：SMT.Workflow.Monitoring.Model 
	 * 模块名称：流程监控
	 * 描　　述： 异常记录日志实体
	 * 修改人员：
	 * 修改日期：
	 * 修改内容：
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model
{
    /// <summary>
    /// [异常记录日志]
    /// </summary>
    [DataContract]
    public class FLOW_EXCEPTIONLOG
    {

        /// <summary>
        /// 主键ID
        /// </summary>
        [DataMember]
        public string ID { get; set; }

        /// <summary>
        /// 业务ID
        /// </summary>
        [DataMember]
        public string FORMID { get; set; }

        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 状态:未处理;已处理
        /// </summary>
        [DataMember]
        public string STATE { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [DataMember]
        public DateTime CREATEDATE { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [DataMember]
        public string CREATENAME { get; set; }

        /// <summary>
        /// 处理日期
        /// </summary>
        [DataMember]
        public DateTime UPDATEDATE { get; set; }

        /// <summary>
        /// 处理人
        /// </summary>
        [DataMember]
        public string UPDATENAME { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public string REMARK { get; set; }

        /// <summary>
        /// 提交信息
        /// </summary>
        [DataMember]
        public string SUBMITINFO { get; set; }

        /// <summary>
        /// 异常日志信息
        /// </summary>
        [DataMember]
        public string LOGINFO { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        [DataMember]
        public string MODELNAME { get; set; }

        /// <summary>
        /// 单据所属人ID
        /// </summary>
        [DataMember]
        public string OWNERID { get; set; }

        /// <summary>
        /// 单据所属人姓名
        /// </summary>
        [DataMember]
        public string OWNERNAME { get; set; }

        /// <summary>
        /// 单据所属人公司ID
        /// </summary>
        [DataMember]
        public string OWNERCOMPANYID { get; set; }

        /// <summary>
        /// 单据所属人公司名称
        /// </summary>
        [DataMember]
        public string OWNERCOMPANYNAME { get; set; }

        /// <summary>
        /// 单据所属人部门ID
        /// </summary>
        [DataMember]
        public string OWNERDEPARMENTID { get; set; }

        /// <summary>
        /// 单据所属人部门名称
        /// </summary>
        [DataMember]
        public string OWNERDEPARMENTNAME { get; set; }

        /// <summary>
        /// 单据所属人岗位ID
        /// </summary>
        [DataMember]
        public string OWNERPOSTID { get; set; }

        /// <summary>
        /// 单据所属人岗位名称
        /// </summary>
        [DataMember]
        public string OWNERPOSTNAME { get; set; }

        /// <summary>
        /// 审核状态;审核通过,审核不通过
        /// </summary>
        [DataMember]
        public string AUDITSTATE { get; set; }

    }
}
