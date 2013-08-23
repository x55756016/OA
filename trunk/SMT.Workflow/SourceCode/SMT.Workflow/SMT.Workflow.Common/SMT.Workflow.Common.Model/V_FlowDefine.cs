/****************************************************************
 * 作者：    亢晓方
 * 书写时间：2012/9/12 15:41:32 
 * 内容概要： 
 *  ------------------------------------------------------
 * 修改：    
****************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model
{
    public class V_FlowDefine
    {
        /// <summary>
        /// 公司名称
        /// </summary>
        [DataMember]
        public string CompanyName { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        [DataMember]
        public string DepartmentName{ get; set; }
        /// <summary>
        /// 系统代码
        /// </summary>
        [DataMember]
        public string SystemCode { get; set; }
        /// <summary>
        /// 系统名称
        /// </summary>
        [DataMember]
        public string SystemName { get; set; }
        /// <summary>
        /// 模块代码
        /// </summary>
        [DataMember]
        public string ModelCode { get; set; }
        /// <summary>
        /// 模块名称
        /// </summary>
        [DataMember]
        public string ModelName { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        [DataMember]
        public string FlowName { get; set; }

        /// <summary>
        /// 流程代码
        /// </summary>
        [DataMember]
        public string FlowCode { get; set; }

         /// <summary>
        /// 创建人
        /// </summary>
        [DataMember]
        public string CREATEUSERNAME { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public DateTime? CREATEDATE { get; set; }
      
    }
}
