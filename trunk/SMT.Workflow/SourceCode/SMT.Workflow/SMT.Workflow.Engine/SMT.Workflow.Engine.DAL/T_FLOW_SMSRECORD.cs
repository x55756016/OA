/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_FLOW_SMSRECORD.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/25 9:26:45   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.DAL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.Engine.DAL
{
    /// <summary>
    /// 短信实体
    /// </summary>
    public class T_FLOW_SMSRECORD
    {
        /// <summary>
        /// 短信记录ID
        /// </summary>
        public string SMSRECORD { get; set; }

        /// <summary>
        /// 记录触发批次
        /// </summary>
        public string BATCHNUMBER { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public string COMPANYID { get; set; }

        /// <summary>
        /// 发送状态
        /// </summary>
        public int? SENDSTATUS { get; set; }

        /// <summary>
        /// 短信账号
        /// </summary>
        public string ACCOUNT { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string MOBILE { get; set; }

        /// <summary>
        /// 发送内容
        /// </summary>
        public string SENDMESSAGE { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime? SENDTIME { get; set; }

        /// <summary>
        /// 短信类型
        /// </summary>
        public string OWNERID { get; set; }

        /// <summary>
        /// 所属人名称
        /// </summary>
        public string OWNERNAME { get; set; }
        /// <summary>
        ///短信数量
        /// </summary>
        public int? TASKCOUNT { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
    }
}
