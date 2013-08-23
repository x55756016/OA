using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.Common.Model
{
    /// <summary>
    /// 初始化的默认消息
    /// </summary>
    public class T_WF_DEFAULTMESSAGE
    {
        /// <summary>
        /// 消息ID(GUID)
        /// </summary>
        public string MESSAGEID { get; set; }
        /// <summary>
        /// 系统代码
        /// </summary>
        public string SYSTEMCODE { get; set; }
        /// <summary>
        /// 系统名称
        /// </summary>
        public string SYSTEMNAME { get; set; }
        /// <summary>
        /// 模块代码
        /// </summary>
        public string MODELCODE { get; set; }
        /// <summary>
        /// 模块名称
        /// </summary>
        public string MODELNAME { get; set; }
        /// <summary>
        /// 应用URL
        /// </summary>
        public string APPLICATIONURL { get; set; }
        /// <summary>
        /// 审核状态:1审核中,2审核通过,3审核不通过
        /// </summary>
        public string MESSAGECONTENT { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public decimal? AUDITSTATE { get; set; }
        /// <summary>
        /// 创建日期时间
        /// </summary>
        public DateTime? CREATEDATE { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CREATEUSERID { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        public string CREATEUSERNAME { get; set; }
        /// <summary>
        /// 修改人ID
        /// </summary>
        public string UPDATEUSERID { get; set; }
        /// <summary>
        /// 修改人姓名
        /// </summary>
        public string UPDATEUSERNAME { get; set; }
        /// <summary>
        /// 修改日期时间
        /// </summary>
        public DateTime? UPDATEDATE { get; set; }
    }
}
