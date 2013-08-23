/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：T_FLOW_ENGINEMSGLIST.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/19 10:11:08   
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

namespace EngineDataModel
{
    /// <summary>
    /// [代办任务]
    /// </summary>
    [DataContract]
    public class T_FLOW_ENGINEMSGLIST
    {

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MODELNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MESSAGEID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string ORDERNODECODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MESSAGEBODY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string APPLICATIONURL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string APPLICATIONCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVEUSER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVEDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVETIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime BEFOREPROCESSDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MESSAGESTATUS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OPERATIONUSER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OPERATIONDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OPERATIONTIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATEUSERID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATEDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string UPDATEUSERID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string UPDATEDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string UPDATETIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string ENGINECODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CREATETIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MAILSTATUS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RTXSTATUS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 是否已提醒
        /// </summary>
        [DataMember]
        public string ISALARM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string COMPANYCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string APPFIELDVALUE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FLOWXML { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string APPXML { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string TEMPFIELD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string SYSTEMNAME { get; set; }

    }
}
