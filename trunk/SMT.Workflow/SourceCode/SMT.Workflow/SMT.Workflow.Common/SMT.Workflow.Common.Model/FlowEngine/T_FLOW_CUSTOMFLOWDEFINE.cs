/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_FLOW_CUSTOMFLOWDEFINE.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/21 9:34:17   
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
    [DataContract]
    public class T_FLOW_CUSTOMFLOWDEFINE
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PROCESSID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string COMPANYCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string SYSTEMCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string SYSTEMNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MODELNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FUNCTIONNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FUNCTIONDES { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PROCESSWCFURL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PROCESSFUNCNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PROCESSFUNCPAMETER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PAMETERSPLITCHAR { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string WCFBINDINGCONTRACT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MESSAGEBODY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MSGLINKURL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVEUSER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVEUSERNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OWNERCOMPANYID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OWNERDEPARTMENTID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OWNERPOSTID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATEDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATETIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATEUSERNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATEUSERID { get; set; }
    }
}
