/*---------------------------------------------------------------------  
    * 版　权：Copyright ©  SmtOnline  2011    
    * 文件名：TimingTriggerdefine.cs  
    * 创建者：LONGKC   
    * 创建日期：2011/10/21 11:59:33   
    * CLR版本： 4.0.30319.1  
    * 命名空间：SMT.Workflow.Common.Model 
    * 模块名称：
    * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class T_FLOW_TIMINGTRIGGERDEFINE
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
        public string APPLICATIONORDERCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PROCESSSTARTDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PROCESSSTARTTIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PROCESSCYCLE { get; set; }

        /// <summary>
        /// 接收者
        /// </summary>
        [DataMember]
        public string RECEIVER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string RECEIVEROLE { get; set; }

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
        public string DATASTATUS { get; set; }

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
        public string FUNCTIONMARK { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string MODELCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string TRIGGERTYPE { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [DataMember]
        public string TRIGGERDESCRIPTION { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        [DataMember]
        public string SYSTEMNAME { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        [DataMember]
        public string MODELNAME { get; set; }

        /// <summary>
        /// 定时触发名称
        /// </summary>
        [DataMember]
        public string TRIGGERNAME { get; set; }

        /// <summary>
        /// 接收者名称
        /// </summary>
        [DataMember]
        public string RECEIVERNAME { get; set; }

        /// <summary>
        /// 接口类型（引擎，定时接口）
        /// </summary>
        [DataMember]
        public string CONTRACTTYPE { get; set; }

        /// <summary>
        /// 处理周期次数
        /// </summary>
        [DataMember]
        public string PROCESSNUM { get; set; }

    }
}
