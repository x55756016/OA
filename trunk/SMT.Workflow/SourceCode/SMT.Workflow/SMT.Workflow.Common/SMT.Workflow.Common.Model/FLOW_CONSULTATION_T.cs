/*---------------------------------------------------------------------  
    * 版　权：Copyright ©  SmtOnline  2011    
    * 文件名：T.cs  
    * 创建者：LONGKC   
    * 创建日期：2011/10/9 10:59:09   
    * CLR版本： 4.0.30319.1  
    * 命名空间：SMT.Workflow.Common.Model 
    * 模块名称：
    * 描　　述：[咨询] 	 
* ---------------------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
namespace SMT.Workflow.Common.Model
{

 
    [DataContract]
    public class FLOW_CONSULTATION_T
    {
        public FLOW_FLOWRECORDDETAIL_T FLOW_FLOWRECORDDETAIL_T { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CONSULTATIONID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FLOWRECORDDETAILID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CONSULTATIONUSERID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CONSULTATIONUSERNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CONSULTATIONCONTENT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime? CONSULTATIONDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string REPLYUSERID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string REPLYUSERNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string REPLYCONTENT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public DateTime? REPLYDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FLAG { get; set; }

       
    }
}