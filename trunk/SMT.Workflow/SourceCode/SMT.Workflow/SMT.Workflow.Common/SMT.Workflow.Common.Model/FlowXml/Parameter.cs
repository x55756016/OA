using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SMT.Workflow.Common.Model.FlowXml
{
    /// <summary>
    /// WCF参数自定义模型
    /// </summary>
    [DataContract]
    public class Parameter
    {
        [DataMember]
        public string Name
        {
            get;
            set;
        }
        [DataMember]
        public string Description
        {
            get;
            set;
        }
        [DataMember]
        public string Value
        {
            get;
            set;
        }
        [DataMember]
        public string TableName
        {
            get;
            set;
        }
    }
}
