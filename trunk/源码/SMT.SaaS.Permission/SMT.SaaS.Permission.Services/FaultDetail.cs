using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;


namespace SMT.SaaS.Permission.Services
{
    [DataContract]
    public class FaultDetail
    {
        /// <summary>
        /// 异常级别, 0:正常,大于0:提示,小于0:异常
        /// </summary>
        [DataMember]
        public int FaultLevel { get; set; }

        /// <summary>
        ///异常编码(可返回用于多语言) 
        /// </summary>
        [DataMember]
        public int FaultCode { get; set; }

        /// <summary>
        /// 异常描述
        /// </summary>
        [DataMember]
        public string FaultMessage { get; set; }
    }
}
